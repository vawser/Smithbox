using HKLib.hk2018;
using StudioCore.Editors.HavokEditor;
using System.Numerics;

// Rebuilds a vanilla compressed mesh with the Collision Generator's builder and checks the result: every triangle
// decodes back to its input, every tree bounds what it holds, the keys and materials line up, and the shape survives
// a write and re-read.
static class Rebuild
{
    public static (List<Vector3> Vertices, List<int> Indices, List<uint> Materials) Decode(fsnpCustomParamCompressedMeshShape shape)
    {
        var data = (hknpCompressedMeshShapeData)shape.m_data;
        var tree = data.m_meshTree;

        // Shape key -> material, through the FromSoftware parameters
        var materialOfKey = new Dictionary<uint, uint>();
        int shift = 32 - tree.m_bitsPerKey;
        for (int i = 0; i < shape.m_triangleIndexToShapeKey.Count; i++)
        {
            var group = shape.m_pParam.m_triangleDataArray[i].m_primitiveDataIndex;
            materialOfKey[shape.m_triangleIndexToShapeKey[i] >> shift] = shape.m_pParam.m_primitiveDataArray[(int)group].m_materialNameData;
        }

        var vertices = new List<Vector3>();
        var weld = new Dictionary<Vector3, int>();
        var indices = new List<int>();
        var materials = new List<uint>();

        int Add(Vector3 v)
        {
            if (!weld.TryGetValue(v, out var i))
            {
                i = vertices.Count;
                vertices.Add(v);
                weld[v] = i;
            }
            return i;
        }

        for (int s = 0; s < tree.m_sections.Count; s++)
        {
            var section = tree.m_sections[s];
            for (int p = 0; p < section.m_numPrimitives; p++)
            {
                var prim = tree.m_primitives[(int)section.m_firstPrimitiveIndex + p];
                if (prim.m_indices[0] == 0xDE && prim.m_indices[1] == 0xAD && prim.m_indices[2] == 0xDE && prim.m_indices[3] == 0xAD)
                    continue;

                var v = prim.m_indices.Select(i => Add(Vertex(tree, section, i))).ToArray();
                uint key = (uint)(s << 8 | p << 1);

                indices.AddRange([v[0], v[1], v[2]]);
                materials.Add(materialOfKey.GetValueOrDefault(key));

                if (prim.m_indices[2] != prim.m_indices[3])
                {
                    indices.AddRange([v[0], v[2], v[3]]);
                    materials.Add(materialOfKey.GetValueOrDefault(key | 1));
                }
            }
        }

        return (vertices, indices, materials);
    }

    public static Vector3 Vertex(hknpCompressedMeshShapeTree tree, HKLib.hk2018.hkcdStaticMeshTree.Section section, byte index)
    {
        if (index < section.m_numPackedVertices)
        {
            var offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
            var scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);
            return HavokCompressedMeshBuilder.DecompressPackedVertex(tree.m_packedVertices[(int)(section.m_firstPackedVertexIndex + index)], scale, offset);
        }

        var shared = tree.m_sharedVerticesIndex[(int)section.m_firstSharedVertexIndex + index - section.m_numPackedVertices];
        return HavokCompressedMeshBuilder.DecompressSharedVertex(tree.m_sharedVertices[shared],
            new Vector3(tree.m_domain.m_min.X, tree.m_domain.m_min.Y, tree.m_domain.m_min.Z),
            new Vector3(tree.m_domain.m_max.X, tree.m_domain.m_max.Y, tree.m_domain.m_max.Z));
    }

    public static List<string> Check(fsnpCustomParamCompressedMeshShape vanilla, Func<fsnpCustomParamCompressedMeshShape, fsnpCustomParamCompressedMeshShape> roundTrip, out string stats)
    {
        var errors = new List<string>();
        var (vertices, indices, materials) = Decode(vanilla);
        int triangleCount = indices.Count / 3;

        var build = HavokCompressedMeshBuilder.Build(vertices, indices, materials);
        var shape = roundTrip(HavokCompressedMeshGenerator_HKX3.ToShape(build));

        var data = (hknpCompressedMeshShapeData)shape.m_data;
        var tree = data.m_meshTree;
        var vTree = ((hknpCompressedMeshShapeData)vanilla.m_data).m_meshTree;

        stats = $"tris {triangleCount} sections {vTree.m_sections.Count}->{tree.m_sections.Count} packed {vTree.m_packedVertices.Count}->{tree.m_packedVertices.Count} shared {vTree.m_sharedVertices.Count}->{tree.m_sharedVertices.Count} keyBits {vTree.m_bitsPerKey}->{tree.m_bitsPerKey} simd {((hknpCompressedMeshShapeData)vanilla.m_data).m_simdTree.m_nodes.Count}->{data.m_simdTree.m_nodes.Count}";

        // 1. Every triangle decodes back to its input, through its key
        float maxError = 0;
        int shift = 32 - tree.m_bitsPerKey;
        var decodedBounds = new Dictionary<uint, (Vector3 Min, Vector3 Max)>();
        for (int t = 0; t < triangleCount; t++)
        {
            uint key = shape.m_triangleIndexToShapeKey[t] >> shift;
            if ((shape.m_triangleIndexToShapeKey[t] & ((1u << shift) - 1)) != (1u << shift) - 1)
                errors.Add($"tri {t}: stored key low bits not set");

            var section = tree.m_sections[(int)(key >> 8)];
            var prim = tree.m_primitives[(int)(section.m_firstPrimitiveIndex + ((key & 0xFF) >> 1))];
            var corners = prim.m_indices.Take(3).Select(i => Vertex(tree, section, i)).ToArray();

            for (int c = 0; c < 3; c++)
                maxError = MathF.Max(maxError, (corners[c] - vertices[indices[t * 3 + c]]).Length());

            decodedBounds[key] = (Vector3.Min(Vector3.Min(corners[0], corners[1]), corners[2]), Vector3.Max(Vector3.Max(corners[0], corners[1]), corners[2]));

            var param = shape.m_pParam.m_triangleDataArray[t];
            if (shape.m_pParam.m_primitiveDataArray[(int)param.m_primitiveDataIndex].m_materialNameData != materials[t])
                errors.Add($"tri {t}: material mismatch");
        }
        if (maxError > 0.03f)
            errors.Add($"max vertex error {maxError}");
        stats += $" maxErr {maxError:G3}";

        if (decodedBounds.Count != triangleCount)
            errors.Add($"keys not unique: {decodedBounds.Count} for {triangleCount} triangles");
        if (tree.m_maxKeyValue != decodedBounds.Keys.Max())
            errors.Add("max key mismatch");
        if (shape.m_triangleIsInterior.m_storage.m_numBits != tree.m_maxKeyValue + 1)
            errors.Add("interior bits");

        // 2. The tree over the sections bounds each section's triangles, and its leaves are the sections' leaf indices
        var domainMin = V3(tree.m_domain.m_min);
        var domainMax = V3(tree.m_domain.m_max);
        var seenSections = new HashSet<int>();
        void Walk5(int index, Vector3 pMin, Vector3 pMax, bool root)
        {
            var n = tree.m_nodes[index];
            var (min, max) = root ? (pMin, pMax) : Box(n.m_xyz, pMin, pMax);
            if ((n.m_hiData & 0x80) != 0)
            {
                Walk5(index + 1, min, max, false);
                Walk5(index + 2 * (((n.m_hiData & 0x7F) << 8) | n.m_loData), min, max, false);
                return;
            }

            int s = ((n.m_hiData & 0x7F) << 8) | n.m_loData;
            seenSections.Add(s);
            var section = tree.m_sections[s];
            if (section.m_leafIndex != index)
                errors.Add($"section {s}: leaf index {section.m_leafIndex} != {index}");

            for (int p = 0; p < section.m_numPrimitives; p++)
            {
                var b = decodedBounds[(uint)(s << 8 | p << 1)];
                if (!Contains(min, max, b.Min, b.Max))
                    errors.Add($"section {s} prim {p}: outside its leaf in the section tree");
            }

            // 3. The section's own tree
            var seenPrims = new HashSet<int>();
            void Walk4(int i, Vector3 qMin, Vector3 qMax, bool sRoot)
            {
                var m = section.m_nodes[i];
                var (mn, mx) = sRoot ? (qMin, qMax) : Box(m.m_xyz, qMin, qMax);
                if ((m.m_data & 1) != 0)
                {
                    Walk4(i + 1, mn, mx, false);
                    Walk4(i + (m.m_data & 0xFE), mn, mx, false);
                    return;
                }

                int p = m.m_data >> 1;
                seenPrims.Add(p);
                var b = decodedBounds[(uint)(s << 8 | p << 1)];
                if (!Contains(mn, mx, b.Min, b.Max))
                    errors.Add($"section {s} prim {p}: outside its leaf box");
            }
            Walk4(0, V3(section.m_domain.m_min), V3(section.m_domain.m_max), true);
            if (seenPrims.Count != section.m_numPrimitives)
                errors.Add($"section {s}: tree reaches {seenPrims.Count}/{section.m_numPrimitives} prims");
            if (!Contains(domainMin, domainMax, V3(section.m_domain.m_min), V3(section.m_domain.m_max)))
                errors.Add($"section {s}: domain outside the mesh domain");
        }
        Walk5(0, domainMin, domainMax, true);
        if (seenSections.Count != tree.m_sections.Count)
            errors.Add($"top tree reaches {seenSections.Count}/{tree.m_sections.Count} sections");

        // 4. The SIMD tree holds each key once, inside its lane bounds
        var seenKeys = new HashSet<uint>();
        void WalkSimd(int index)
        {
            var n = data.m_simdTree.m_nodes[index];
            for (int lane = 0; lane < 4; lane++)
            {
                var min = new Vector3(Lane(n.m_lx, lane), Lane(n.m_ly, lane), Lane(n.m_lz, lane));
                var max = new Vector3(Lane(n.m_hx, lane), Lane(n.m_hy, lane), Lane(n.m_hz, lane));
                if (min.X > max.X)
                    continue; // empty lane

                if (n.m_isLeaf)
                {
                    if (!seenKeys.Add(n.m_data[lane]))
                        errors.Add($"simd: key {n.m_data[lane]:X} twice");
                    else if (!decodedBounds.TryGetValue(n.m_data[lane], out var b) || !Contains(min, max, b.Min, b.Max))
                        errors.Add($"simd: key {n.m_data[lane]:X} outside its lane");
                }
                else
                {
                    WalkSimd((int)n.m_data[lane]);
                }
            }
        }
        WalkSimd(1);
        if (seenKeys.Count != triangleCount)
            errors.Add($"simd reaches {seenKeys.Count}/{triangleCount} keys");

        return errors;
    }

    /// <summary>
    /// Checks the tree decoding against a vanilla mesh: each primitive must sit inside its leaf box
    /// </summary>
    public static int CheckVanillaTrees(fsnpCustomParamCompressedMeshShape vanilla, List<string> errors)
    {
        var tree = ((hknpCompressedMeshShapeData)vanilla.m_data).m_meshTree;
        int checkedPrims = 0;
        for (int s = 0; s < tree.m_sections.Count; s++)
        {
            var section = tree.m_sections[s];
            void Walk4(int i, Vector3 qMin, Vector3 qMax, bool sRoot)
            {
                var m = section.m_nodes[i];
                var (mn, mx) = sRoot ? (qMin, qMax) : Box(m.m_xyz, qMin, qMax);
                if ((m.m_data & 1) != 0)
                {
                    Walk4(i + 1, mn, mx, false);
                    Walk4(i + (m.m_data & 0xFE), mn, mx, false);
                    return;
                }

                var prim = tree.m_primitives[(int)section.m_firstPrimitiveIndex + (m.m_data >> 1)];
                if (prim.m_indices[0] == 0xDE && prim.m_indices[1] == 0xAD)
                    return;
                foreach (var idx in prim.m_indices)
                {
                    var v = Vertex(tree, section, idx);
                    if (!Contains(mn, mx, v, v))
                    {
                        errors.Add($"vanilla section {s} prim {m.m_data >> 1}: vertex {v} outside {mn}-{mx}");
                        return;
                    }
                }
                checkedPrims++;
            }
            Walk4(0, V3(section.m_domain.m_min), V3(section.m_domain.m_max), true);
        }
        return checkedPrims;
    }

    private static Vector3 V3(Vector4 v) => new(v.X, v.Y, v.Z);

    private static float Lane(Vector4 v, int lane) => lane switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => v.W };

    private static (Vector3, Vector3) Box(byte[] xyz, Vector3 pMin, Vector3 pMax)
    {
        var (x0, x1) = HavokCompressedMeshBuilder.DecompressAxis(xyz[0], pMin.X, pMax.X);
        var (y0, y1) = HavokCompressedMeshBuilder.DecompressAxis(xyz[1], pMin.Y, pMax.Y);
        var (z0, z1) = HavokCompressedMeshBuilder.DecompressAxis(xyz[2], pMin.Z, pMax.Z);
        return (new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
    }

    private static bool Contains(Vector3 min, Vector3 max, Vector3 bMin, Vector3 bMax)
    {
        const float eps = 1e-4f;
        return bMin.X >= min.X - eps && bMin.Y >= min.Y - eps && bMin.Z >= min.Z - eps
            && bMax.X <= max.X + eps && bMax.Y <= max.Y + eps && bMax.Z <= max.Z + eps;
    }
}
