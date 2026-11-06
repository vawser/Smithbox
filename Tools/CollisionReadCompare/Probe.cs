using System.Numerics;
using SoulsFormats;

// One-off checks.
public static class Probe
{
    /// <summary>
    /// Sekiro walls: every shape key and its local triangle, from HKX2 (as WallBank used it) and from
    /// CollisionGeometry.CompressedTriangles (as it does now). Returns (keys compared, keys that differ).
    /// </summary>
    public static (int Keys, int Different, string First) Walls(byte[] data, byte[] compendium, Havok.HavokCompendium unified)
    {
        var root = (HKX2.hkRootLevelContainer)new HKX2.TagFileDeserializer().Deserialize(data, compendium == null ? null : HKX2.TagFileDeserializer.ReadCompendium(compendium));
        var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<HKX2.hknpPhysicsSceneData>().FirstOrDefault();
        var body = scene?.m_systemDatas.SelectMany(s => s.m_bodyCinfos).FirstOrDefault(b => b.m_shape is HKX2.fsnpCustomParamCompressedMeshShape);
        if (body == null) return (0, 0, null);

        var old = new Dictionary<uint, (Vector3, Vector3, Vector3)>();
        var shape = (HKX2.fsnpCustomParamCompressedMeshShape)body.m_shape;
        var d = shape.m_data;
        for (int s = 0; s < d.m_meshTree.m_sections.Count; s++)
            for (int p = 0; p < (int)(d.m_meshTree.m_sections[s].m_primitives.m_data & 0xFF); p++)
                for (int half = 0; half < 2; half++)
                {
                    uint key = d.GetShapeKey(s, p, half);
                    if (d.TryGetShapeKeyTriangle(key, out var a, out var b, out var c)) old[key] = (a, b, c);
                }

        var doc = Havok.Files.HavokFile.Read(data, Havok.HavokProfile.SDT, unified);
        var mesh = (Havok.Shared.IHavokCompressedMesh)Havok.Shared.CollisionGeometry.Bodies(doc.Objects).First(x => x.Shape is Havok.Shared.IHavokCompressedMesh).Shape;
        var now = Havok.Shared.CollisionGeometry.CompressedTriangles(mesh).ToDictionary(t => t.Key, t => (t.A, t.B, t.C));

        int different = 0; string first = null;
        foreach (var key in old.Keys.Union(now.Keys))
        {
            bool same = old.TryGetValue(key, out var o) && now.TryGetValue(key, out var n) && o == n;
            if (same) continue;
            different++;
            first ??= $"key {key:X8}: {(old.ContainsKey(key) ? old[key].ToString() : "missing")} vs {(now.ContainsKey(key) ? now[key].ToString() : "missing")}";
        }
        return (old.Count, different, first);
    }

    // does a DS3 compressed mesh hold more packed vertices than a ushort indexes?
    public static void PackedRange(byte[] data)
    {
        var root = (HKX2.hkRootLevelContainer)new HKX2.PackFileDeserializer().Deserialize(new BinaryReaderEx(false, data));
        var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<HKX2.hknpPhysicsSceneData>().First();
        foreach (var body in scene.m_systemDatas.SelectMany(s => s.m_bodyCinfos))
            if (body.m_shape is HKX2.fsnpCustomParamCompressedMeshShape m)
            {
                var t = m.m_data.m_meshTree;
                long max = t.m_sections.Max(s => (long)s.m_firstPackedVertex + (s.m_sharedVertices.m_data & 0xFF));
                Console.WriteLine($"packed vertices {t.m_packedVertices.Count}, highest packed index used {max}, over 65535: {max > 65535}");
            }
    }
}
