using HKX2;
using SoulsFormats;
using StudioCore.Editors.HavokEditor;
using System.Numerics;

// Rebuilds a DS3 compressed mesh with the Collision Generator's builder, writes and re-reads the file, and checks
// that every triangle decodes back to its input through its key, every tree bounds what it holds, and the
// materials line up.
static class Ds3Rebuild
{
    public static List<string> Check(hkRootLevelContainer root, hknpBodyCinfo body, fsnpCustomParamCompressedMeshShape vanilla, out string stats)
    {
        var errors = new List<string>();
        var (vertices, indices, materials) = Decode(vanilla);
        int triangleCount = indices.Count / 3;

        var built = HavokCompressedMeshGenerator_HKX2.BuildCompressedMeshShape(vertices, indices, materials);

        // Write the file with the rebuilt shape and read it back
        body.m_shape = built;
        var bw = new BinaryWriterEx(false);
        new PackFileSerializer().Serialize(root, bw);
        var reread = (hkRootLevelContainer)new PackFileDeserializer().Deserialize(new BinaryReaderEx(false, bw.FinishBytes()));
        int bodyIndex = root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First()
            .m_systemDatas.SelectMany(s => s.m_bodyCinfos).ToList().IndexOf(body);
        var shape = (fsnpCustomParamCompressedMeshShape)reread.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First()
            .m_systemDatas.SelectMany(s => s.m_bodyCinfos).ElementAt(bodyIndex).m_shape;

        var data = shape.m_data;
        var tree = data.m_meshTree;
        var vTree = vanilla.m_data.m_meshTree;
        stats = $"tris {triangleCount} sections {vTree.m_sections.Count}->{tree.m_sections.Count} packed {vTree.m_packedVertices.Count}->{tree.m_packedVertices.Count} shared {vTree.m_sharedVertices.Count}->{tree.m_sharedVertices.Count} keyBits {vTree.m_bitsPerKey}->{tree.m_bitsPerKey}";

        if (shape.m_flags != vanilla.m_flags) stats += $" (vanilla flags {vanilla.m_flags})";

        // Every triangle decodes back to its input, through its key
        float maxError = 0;
        var keyBounds = new Dictionary<uint, (Vector3 Min, Vector3 Max)>();
        for (int t = 0; t < triangleCount; t++)
        {
            var stored = shape.m_triangleIndexToShapeKey[t];
            if (!data.TryGetShapeKeyTriangle(stored, out var a, out var b, out var c))
            {
                errors.Add($"tri {t}: key {stored:X8} decodes to nothing");
                continue;
            }

            var corners = new[] { a, b, c };
            for (int i = 0; i < 3; i++)
                maxError = MathF.Max(maxError, (corners[i] - vertices[indices[t * 3 + i]]).Length());

            keyBounds[stored >> (32 - tree.m_bitsPerKey)] = (Vector3.Min(Vector3.Min(a, b), c), Vector3.Max(Vector3.Max(a, b), c));

            var param = shape.m_pParam.m_triangleDataArray[t];
            if (shape.m_pParam.m_primitiveDataArray[(int)param.m_primitiveDataIndex].m_materialNameData != materials[t])
                errors.Add($"tri {t}: material mismatch");
        }
        if (maxError > 0.03f) errors.Add($"max vertex error {maxError}");
        stats += $" maxErr {maxError:G3}";

        if (keyBounds.Count != triangleCount) errors.Add($"keys not unique: {keyBounds.Count}/{triangleCount}");
        if (tree.m_maxKeyValue != keyBounds.Keys.Max()) errors.Add("max key mismatch");

        // The tree over the sections, then each section's own tree
        var seenSections = new HashSet<int>();
        void Walk5(int index, Vector3 pMin, Vector3 pMax, bool isRoot)
        {
            var n = tree.m_nodes[index];
            var (min, max) = isRoot ? (pMin, pMax) : (n.DecompressMin(pMin, pMax), n.DecompressMax(pMin, pMax));
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
                var (mn, mx) = sRoot ? (qMin, qMax) : (m.DecompressMin(qMin, qMax), m.DecompressMax(qMin, qMax));
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

        return errors;
    }

    private static (List<Vector3>, List<int>, List<uint>) Decode(fsnpCustomParamCompressedMeshShape shape)
    {
        var data = shape.m_data;
        var tree = data.m_meshTree;
        int shift = 32 - tree.m_bitsPerKey;

        var materialOfKey = new Dictionary<uint, uint>();
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
            if (!weld.TryGetValue(v, out var i)) { i = vertices.Count; vertices.Add(v); weld[v] = i; }
            return i;
        }

        for (int s = 0; s < tree.m_sections.Count; s++)
        {
            var section = tree.m_sections[s];
            for (int p = 0; p < (section.m_primitives.m_data & 0xFF); p++)
            {
                for (int half = 0; half < 2; half++)
                {
                    if (!data.TryGetShapeKeyTriangle(data.GetShapeKey(s, p, half), out var a, out var b, out var c))
                        continue;

                    int ia = Add(a), ib = Add(b), ic = Add(c);
                    if (ia == ib || ib == ic || ia == ic)
                        continue;

                    indices.AddRange([ia, ib, ic]);
                    materials.Add(materialOfKey.GetValueOrDefault((uint)(s << 8 | p << 1 | half)));
                }
            }
        }

        return (vertices, indices, materials);
    }

    private static Vector3 V3(Vector4 v) => new(v.X, v.Y, v.Z);

    private static bool Contains(Vector3 min, Vector3 max, Vector3 bMin, Vector3 bMax)
    {
        const float eps = 1e-4f;
        return bMin.X >= min.X - eps && bMin.Y >= min.Y - eps && bMin.Z >= min.Z - eps
            && bMax.X <= max.X + eps && bMax.Y <= max.Y + eps && bMax.Z <= max.Z + eps;
    }
}
