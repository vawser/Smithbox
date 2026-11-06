using Havok.hk2016;
using Havok.Shared;
using System.Numerics;

// Rebuilds a Sekiro compressed mesh with the Collision Generator's builder, writes and re-reads the file, and checks
// that every triangle decodes back to its input through its key, the mesh trees and the SIMD tree bound what they
// hold, and the materials line up.
static class SdtRebuild
{
    public static List<string> Check(Havok.HavokDocument document, Havok.HavokCompendium compendium, hknpBodyCinfo body,
        fsnpCustomParamCompressedMeshShape vanilla, out string stats)
    {
        var errors = new List<string>();
        var root = (hkRootLevelContainer)document.Root;
        var (vertices, indices, materials) = Decode(vanilla);
        int triangleCount = indices.Count / 3;

        var built = Havok.hk2016.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(vertices, indices, materials);

        // Write the file with the rebuilt shape and read it back, as the Havok Editor saves it
        var bodies = root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First().m_systemDatas.SelectMany(s => s.m_bodyCinfos).ToList();
        int bodyIndex = bodies.IndexOf((hknpPhysicsSystemData.bodyCinfoWithAttachment)body);
        var swap = Havok.hk2016.Builders.CollisionShapeBuilder.ReplaceMeshShape(root, built, bodyIndex)!.Value;
        swap.Apply();
        hkRootLevelContainer reread;
        try { reread = (hkRootLevelContainer)Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(document), Havok.HavokProfile.SDT, compendium).Root; }
        finally { swap.Undo(); }

        var system = reread.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First().m_systemDatas[0];
        var shape = (fsnpCustomParamCompressedMeshShape)system.m_bodyCinfos[bodyIndex].m_shape;
        if (!system.m_referencedObjects.Contains(shape)) errors.Add("referenced objects don't hold the new shape");
        if (system.m_referencedObjects.OfType<fsnpCustomParamCompressedMeshShape>().Count() != 1) errors.Add("referenced objects still hold the old shape");

        var data = (hknpCompressedMeshShapeData)shape.m_data;
        var tree = data.m_meshTree;
        var vTree = ((hknpCompressedMeshShapeData)vanilla.m_data).m_meshTree;
        int shift = 32 - tree.m_bitsPerKey;
        stats = $"tris {triangleCount} sections {vTree.m_sections.Count}->{tree.m_sections.Count} packed {vTree.m_packedVertices.Count}->{tree.m_packedVertices.Count} shared {vTree.m_sharedVertices.Count}->{tree.m_sharedVertices.Count} keyBits {vTree.m_bitsPerKey}->{tree.m_bitsPerKey} simd {((hknpCompressedMeshShapeData)vanilla.m_data).m_simdTree.m_nodes.Count}->{data.m_simdTree.m_nodes.Count}";

        // Every triangle decodes back to its input, through its key (decoded with the shared reader the renderer uses)
        var decoded = CollisionGeometry.CompressedTriangles(shape).ToDictionary(t => t.Key, t => (t.A, t.B, t.C));
        float maxError = 0;
        var keyBounds = new Dictionary<uint, (Vector3 Min, Vector3 Max)>();
        for (int t = 0; t < triangleCount; t++)
        {
            var stored = shape.m_triangleIndexToShapeKey[t];
            if (!decoded.TryGetValue(stored, out var tri))
            {
                errors.Add($"tri {t}: key {stored:X8} decodes to nothing");
                continue;
            }

            var corners = new[] { tri.A, tri.B, tri.C };
            for (int i = 0; i < 3; i++)
                maxError = MathF.Max(maxError, (corners[i] - vertices[indices[t * 3 + i]]).Length());

            keyBounds[stored >> shift] = (Vector3.Min(Vector3.Min(tri.A, tri.B), tri.C), Vector3.Max(Vector3.Max(tri.A, tri.B), tri.C));

            var param = shape.m_pParam.m_triangleDataArray[t];
            if (shape.m_pParam.m_primitiveDataArray[(int)param.m_primitiveDataIndex].m_materialNameData != materials[t])
                errors.Add($"tri {t}: material mismatch");
        }
        if (maxError > 0.03f) errors.Add($"max vertex error {maxError}");
        stats += $" maxErr {maxError:G3}";

        if (keyBounds.Count != triangleCount) errors.Add($"keys not unique: {keyBounds.Count}/{triangleCount}");
        if (tree.m_maxKeyValue != keyBounds.Keys.Max()) errors.Add("max key mismatch");
        if (shape.m_triangleIsInterior.m_storage.m_numBits != tree.m_maxKeyValue + 1) errors.Add("interior bits");

        // The tree over the sections, then each section's own tree
        var seenSections = new HashSet<int>();
        void Walk5(int index, Vector3 pMin, Vector3 pMax, bool isRoot)
        {
            var n = tree.m_nodes[index];
            var (min, max) = isRoot ? (pMin, pMax) : Decompress(n.m_xyz, pMin, pMax);
            if ((n.m_hiData & 0x80) != 0)
            {
                Walk5(index + 1, min, max, false);
                Walk5(index + 2 * (((n.m_hiData & 0x7F) << 8) | n.m_loData), min, max, false);
                return;
            }

            int s = ((n.m_hiData & 0x7F) << 8) | n.m_loData;
            seenSections.Add(s);
            var section = tree.m_sections[s];
            if (section.m_leafIndex != index) errors.Add($"section {s}: leaf index");

            int prims = (int)(section.m_primitives.m_data & 0xFF);
            var seen = new HashSet<int>();
            void Walk4(int i, Vector3 qMin, Vector3 qMax, bool sRoot)
            {
                var m = section.m_nodes[i];
                var (mn, mx) = sRoot ? (qMin, qMax) : Decompress(m.m_xyz, qMin, qMax);
                if ((m.m_data & 1) != 0)
                {
                    Walk4(i + 1, mn, mx, false);
                    Walk4(i + (m.m_data & 0xFE), mn, mx, false);
                    return;
                }

                int p = m.m_data >> 1;
                seen.Add(p);
                var bb = keyBounds[(uint)(s << 8 | p << 1)];
                if (!Contains(mn, mx, bb.Min, bb.Max) || !Contains(min, max, bb.Min, bb.Max))
                    errors.Add($"section {s} prim {p}: outside its boxes");
            }
            Walk4(0, V3(section.m_domain.m_min), V3(section.m_domain.m_max), true);
            if (seen.Count != prims) errors.Add($"section {s}: tree reaches {seen.Count}/{prims}");
        }
        Walk5(0, V3(tree.m_domain.m_min), V3(tree.m_domain.m_max), true);
        if (seenSections.Count != tree.m_sections.Count) errors.Add($"top tree reaches {seenSections.Count}/{tree.m_sections.Count}");

        // The SIMD tree, read the way the vanilla files hold it: lane data is value << 1 | isLeaf
        var nodes = data.m_simdTree.m_nodes;
        var simdLeaves = new HashSet<uint>();
        if (nodes.Count < 2 || nodes[0].m_data.Any(d => d != 0)) errors.Add("simd: no empty sentinel");
        void WalkSimd(int index, Vector3 pMin, Vector3 pMax, int depth)
        {
            if (depth > 64) { errors.Add("simd: too deep"); return; }
            var n = nodes[index];
            for (int l = 0; l < 4; l++)
            {
                var (min, max) = (new Vector3(Lane(n.m_lx, l), Lane(n.m_ly, l), Lane(n.m_lz, l)), new Vector3(Lane(n.m_hx, l), Lane(n.m_hy, l), Lane(n.m_hz, l)));
                if (min.X > max.X) { if (n.m_data[l] != 0) errors.Add($"simd node {index}: empty lane with data"); continue; }
                if (!Contains(pMin, pMax, min, max)) errors.Add($"simd node {index} lane {l}: outside its parent");
                uint d = n.m_data[l];
                if ((d & 1) == 1)
                {
                    uint key = d >> 1;
                    if (!simdLeaves.Add(key)) errors.Add($"simd: key {key:X} twice");
                    if (!keyBounds.TryGetValue(key, out var bb)) errors.Add($"simd: leaf key {key:X} isn't a triangle");
                    else if (!Contains(min, max, bb.Min, bb.Max)) errors.Add($"simd: key {key:X} outside its lane");
                }
                else
                {
                    int child = (int)(d >> 1);
                    if (child <= index || child >= nodes.Count) { errors.Add($"simd node {index}: child {child}"); continue; }
                    WalkSimd(child, min, max, depth + 1);
                }
            }
        }
        if (nodes.Count >= 2) WalkSimd(1, new Vector3(float.MinValue), new Vector3(float.MaxValue), 0);
        if (simdLeaves.Count != triangleCount) errors.Add($"simd reaches {simdLeaves.Count}/{triangleCount}");

        return errors;
    }

    /// <summary>
    /// The vanilla triangles, welded, with their materials (decoded with the shared reader)
    /// </summary>
    private static (List<Vector3>, List<int>, List<uint>) Decode(fsnpCustomParamCompressedMeshShape shape)
    {
        var tree = ((hknpCompressedMeshShapeData)shape.m_data).m_meshTree;
        int shift = 32 - tree.m_bitsPerKey;

        // Some vanilla meshes have no triangle data (vertex data stride 0); their triangles get material 0
        var materialOfKey = new Dictionary<uint, uint>();
        for (int i = 0; i < Math.Min(shape.m_triangleIndexToShapeKey.Count, shape.m_pParam?.m_triangleDataArray.Count ?? 0); i++)
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
            if (!weld.TryGetValue(v, out var i)) { i = vertices.Count; vertices.Add(v); weld[v] = i; }
            return i;
        }

        foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(shape))
        {
            int ia = Add(a), ib = Add(b), ic = Add(c);
            if (ia == ib || ib == ic || ia == ic)
                continue;

            indices.AddRange([ia, ib, ic]);
            materials.Add(materialOfKey.GetValueOrDefault(key >> shift));
        }

        return (vertices, indices, materials);
    }

    // The 2016 4- and 5-byte codecs decode as the 2014 ones do (Havok.hk2014/Manual)
    private static (Vector3, Vector3) Decompress(byte[] xyz, Vector3 pMin, Vector3 pMax)
    {
        Vector3 min = default, max = default;
        for (int a = 0; a < 3; a++)
        {
            float lo = (xyz[a] >> 4) * (xyz[a] >> 4) / 226f, hi = (xyz[a] & 0xF) * (xyz[a] & 0xF) / 226f;
            float pLo = a == 0 ? pMin.X : a == 1 ? pMin.Y : pMin.Z, pHi = a == 0 ? pMax.X : a == 1 ? pMax.Y : pMax.Z;
            float vLo = lo * (pHi - pLo) + pLo, vHi = -hi * (pHi - pLo) + pHi;
            if (a == 0) { min.X = vLo; max.X = vHi; } else if (a == 1) { min.Y = vLo; max.Y = vHi; } else { min.Z = vLo; max.Z = vHi; }
        }
        return (min, max);
    }

    private static float Lane(Vector4 v, int l) => l switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => v.W };

    private static Vector3 V3(Vector4 v) => new(v.X, v.Y, v.Z);

    private static bool Contains(Vector3 min, Vector3 max, Vector3 bMin, Vector3 bMax)
    {
        const float eps = 1e-4f;
        return bMin.X >= min.X - eps && bMin.Y >= min.Y - eps && bMin.Z >= min.Z - eps
            && bMax.X <= max.X + eps && bMax.Y <= max.Y + eps && bMax.Z <= max.Z + eps;
    }
}
