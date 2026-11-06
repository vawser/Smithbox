using Havok.hk2019;
using SoulsFormats;

// The Havok 2019 (AC6) compressed meshes, with the values the builder fills
static class Ac6
{
    public static void Survey(BXF4 bxf, byte[] compendium)
    {
        var counts = new Dictionary<string, Dictionary<string, int>>();
        void Count(string k, object v) { if (!counts.TryGetValue(k, out var d)) counts[k] = d = new(); var s = v?.ToString() ?? "null"; d[s] = d.GetValueOrDefault(s) + 1; }

        var unifiedCompendium = compendium == null ? null : Havok.Files.HavokFile.ReadCompendium(compendium, Havok.HavokProfile.AC6);

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx")))
        {
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            hkRootLevelContainer root;
            try
            {
                root = (hkRootLevelContainer)Havok.Files.HavokFile.Read(bytes, Havok.HavokProfile.AC6, unifiedCompendium).Root;
            }
            catch (Exception e) { Count("read failed", e.Message.Substring(0, Math.Min(60, e.Message.Length))); continue; }

            foreach (var body in root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().SelectMany(s => s.m_systemDatas).SelectMany(s => s.m_bodyCinfos))
            {
                Count("shape", body.m_shape?.GetType().Name);
                if (body.m_shape is not hknpCompressedMeshShape cms) continue;
                var data = (hknpCompressedMeshShapeData)cms.m_data;
                var tree = data.m_meshTree;
                Count("flags", cms.m_flags); Count("radius", cms.m_convexRadius); Count("tagCodec", $"{cms.m_shapeTagCodecInfo:X}");
                Count("hasSimd", data.m_hasSimdTree); Count("compact", data.m_simdTree.m_isCompact);
                Count("simd0 leaf", data.m_simdTree.m_nodes.FirstOrDefault()?.m_isLeaf);
                Count("conn headers", data.m_connectivity.m_headers.Count > 0);
                Count("keyBits==bitsPerKey", cms.m_numShapeKeyBits == tree.m_bitsPerKey);
                Count("bitsPerKey==bitlen(maxKey)", tree.m_bitsPerKey == 32 - System.Numerics.BitOperations.LeadingZeroCount(tree.m_maxKeyValue));
                Count("interior==maxKey+1", cms.m_triangleIsInterior.m_storage.m_numBits == tree.m_maxKeyValue + 1);
                Count("domain w", $"{tree.m_domain.m_min.W},{tree.m_domain.m_max.W}");
                Count("runs", string.Join(",", tree.m_primitiveDataRuns.Select(r => r.m_value).Distinct()));
                Count("runs==sections", tree.m_primitiveDataRuns.Count == tree.m_sections.Count);
                Count("tree nodes==2s-1", tree.m_nodes.Count == 2 * tree.m_sections.Count - 1);
                foreach (var s in tree.m_sections) { Count("sec page/layer/flags", $"{s.m_page}/{s.m_layerData}/{s.m_flags}"); Count("sec nodes==2p-1", s.m_nodes.Count == 2 * s.m_numPrimitives - 1); }
                if (cms is fsnpCustomParamCompressedMeshShape f)
                {
                    Count("pParam", f.m_pParam == null ? "null" : $"v{f.m_pParam.m_version:X} vs{f.m_pParam.m_vertexDataStride} ts{f.m_pParam.m_triangleDataStride}");
                    Count("triToKey==keys", f.m_triangleIndexToShapeKey.Count == tree.m_numPrimitiveKeys);
                    if (f.m_pParam != null) foreach (var pd in f.m_pParam.m_primitiveDataArray) { Count("matName", pd.m_materialNameData); Count("vtx FF", pd.m_vertexData.All(b => b == 0xFF)); }
                }
            }
        }
        foreach (var (k, d) in counts) Console.WriteLine($"{k}: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(8).Select(x => $"{x.Key} x{x.Value}"))}");
    }
}
