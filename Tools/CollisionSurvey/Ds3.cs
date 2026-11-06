using Havok.hk2014;
using SoulsFormats;

// The DS3 (Havok 2014) compressed meshes: the values the builder fills, and a rebuild check
static class Ds3
{
    public static void Survey(BXF4 bxf, bool rebuild, bool detail, int maxShown)
    {
        var counts = new Dictionary<string, Dictionary<string, int>>();
        void Count(string k, object v) { if (!counts.TryGetValue(k, out var d)) counts[k] = d = new(); var s = v?.ToString() ?? "null"; d[s] = d.GetValueOrDefault(s) + 1; }
        int shown = 0;

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx")))
        {
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            Havok.HavokDocument document;
            hkRootLevelContainer root;
            try { document = Havok.Files.HavokFile.Read(bytes, Havok.HavokProfile.DS3); root = (hkRootLevelContainer)document.Root; }
            catch (Exception e) { Count("read failed", e.Message.Substring(0, Math.Min(60, e.Message.Length))); continue; }

            foreach (var body in root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().SelectMany(s => s.m_systemDatas).SelectMany(s => s.m_bodyCinfos))
            {
                Count("shape", body.m_shape?.GetType().Name);
                if (body.m_shape is not fsnpCustomParamCompressedMeshShape cms) continue;

                if (rebuild)
                {
                    try
                    {
                        var errors = Ds3Rebuild.Check(document, body, cms, out var stats);
                        Count("rebuild ok", errors.Count == 0);
                        Console.WriteLine($"{file.Name} {body.m_name}: {(errors.Count == 0 ? "OK" : $"{errors.Count} errors")} | {stats}");
                        foreach (var e in errors.Take(5)) Console.WriteLine("    " + e);
                    }
                    catch (Exception e)
                    {
                        Count("rebuild ok", "exception");
                        Console.WriteLine($"{file.Name} {body.m_name}: exception {e}");
                    }
                    finally { body.m_shape = cms; }
                    continue;
                }

                var data = cms.m_data;
                var tree = data.m_meshTree;
                Count("flags", cms.m_flags); Count("radius", cms.m_convexRadius); Count("tagCodec", $"{cms.m_shapeTagCodecInfo:X}");
                Count("dispatch", cms.m_dispatchType); Count("userData", cms.m_userData); Count("properties", cms.m_properties?.GetType().Name);
                Count("welding map", $"mask {cms.m_edgeWeldingMap?.m_secondaryKeyMask} bits {cms.m_edgeWeldingMap?.m_sencondaryKeyBits} p {cms.m_edgeWeldingMap?.m_primaryKeyToIndex?.Count} v {cms.m_edgeWeldingMap?.m_valueAndSecondaryKeys?.Count}");
                Count("quadIsFlat bits", cms.m_quadIsFlat?.m_storage?.m_numBits == 0 ? "0" : cms.m_quadIsFlat?.m_storage?.m_numBits == tree.m_maxKeyValue + 1 ? "maxKey+1" : "other");
                Count("interior bits", cms.m_triangleIsInterior?.m_storage?.m_numBits == 0 ? "0" : cms.m_triangleIsInterior?.m_storage?.m_numBits == tree.m_maxKeyValue + 1 ? "maxKey+1" : $"other");
                Count("simd nodes", data.m_simdTree?.m_nodes?.Count is int c ? (c == 0 ? "0" : c <= 3 ? c.ToString() : ">3") : "null");
                Count("keyBits==bitsPerKey", cms.m_numShapeKeyBits == tree.m_bitsPerKey);
                Count("bitsPerKey==bitlen(maxKey)", tree.m_bitsPerKey == 32 - System.Numerics.BitOperations.LeadingZeroCount(tree.m_maxKeyValue));
                Count("domain w", $"{tree.m_domain.m_min.W},{tree.m_domain.m_max.W}");
                Count("runs", string.Join(",", tree.m_primitiveDataRuns.Select(r => r.m_value.m_data).Distinct().Take(4)));
                Count("runs==sections", tree.m_primitiveDataRuns.Count == tree.m_sections.Count);
                Count("tree nodes==2s-1", tree.m_nodes.Count == 2 * tree.m_sections.Count - 1);
                for (int i = 0; i < tree.m_sections.Count; i++)
                {
                    var s = tree.m_sections[i];
                    Count("sec page/layer/flags/unused", $"{s.m_page}/{s.m_layerData}/{s.m_flags}/{s.m_unusedData}");
                    Count("sec nodes==2p-1", s.m_nodes.Count == 2 * (s.m_primitives.m_data & 0xFF) - 1);
                    Count("sec sharedVertices.lo==numPacked", (s.m_sharedVertices.m_data & 0xFF) == s.m_numPackedVertices);
                    Count("sec dataRuns", $"count {s.m_dataRuns.m_data & 0xFF} first==i {(s.m_dataRuns.m_data >> 8) == i}");
                    Count("sec numSharedIndices==next-first", i + 1 < tree.m_sections.Count
                        ? ((tree.m_sections[i + 1].m_sharedVertices.m_data >> 8) - (s.m_sharedVertices.m_data >> 8) == s.m_numSharedIndices).ToString() : "last");
                }
                Count("pParam", cms.m_pParam == null ? "null" : $"v{cms.m_pParam.m_version:X} vs{cms.m_pParam.m_vertexDataStride} ts{cms.m_pParam.m_triangleDataStride}");
                Count("triToKey==keys", cms.m_triangleIndexToShapeKey?.Count == tree.m_numPrimitiveKeys);
                if (cms.m_pParam != null) foreach (var pd in cms.m_pParam.m_primitiveDataArray) { Count("matName", pd.m_materialNameData); Count("vtx FF", pd.m_vertexData == null ? "null" : pd.m_vertexData.All(b => b == 0xFF).ToString()); }

                if (shown++ < maxShown)
                {
                    Console.WriteLine($"==== {file.Name} {body.m_name}: flags {cms.m_flags} keyBits {cms.m_numShapeKeyBits} radius {cms.m_convexRadius} tag {cms.m_shapeTagCodecInfo:X} sections {tree.m_sections.Count} prims {tree.m_primitives.Count} keys {tree.m_numPrimitiveKeys} maxKey {tree.m_maxKeyValue} bits {tree.m_bitsPerKey}");
                    Console.WriteLine($"  body: mat {body.m_materialId} filter {body.m_collisionFilterInfo:X} flags {body.m_flags} motion {body.m_motionId} quality {body.m_qualityId}");
                    Console.WriteLine($"  quadIsFlat {cms.m_quadIsFlat?.m_storage?.m_numBits} words {string.Join(" ", cms.m_quadIsFlat?.m_storage?.m_words?.Take(4).Select(w => w.ToString("X8")) ?? [])} interior {cms.m_triangleIsInterior?.m_storage?.m_numBits}");
                    Console.WriteLine($"  simd {data.m_simdTree?.m_nodes?.Count}: {string.Join(" | ", data.m_simdTree?.m_nodes?.Take(3).Select(n => $"{n.m_data[0]:X},{n.m_data[1]:X},{n.m_data[2]:X},{n.m_data[3]:X} lx {n.m_lx}") ?? [])}");
                    foreach (var s in tree.m_sections.Take(2))
                        Console.WriteLine($"  section: packed {s.m_firstPackedVertex}+{s.m_numPackedVertices} shared.m_data {s.m_sharedVertices.m_data:X} numShared {s.m_numSharedIndices} prims {s.m_primitives.m_data:X} runs {s.m_dataRuns.m_data:X} leaf {s.m_leafIndex} domain {s.m_domain.m_min}-{s.m_domain.m_max} nodes {s.m_nodes.Count}");
                    Console.WriteLine($"  runs: {string.Join(" ", tree.m_primitiveDataRuns.Take(5).Select(r => $"[{r.m_value.m_data} i{r.m_index} c{r.m_count}]"))}");
                    Console.WriteLine($"  triToKey: {string.Join(",", cms.m_triangleIndexToShapeKey?.Take(6).Select(k => k.ToString("X8")) ?? [])}");
                    if (cms.m_pParam != null)
                        foreach (var t in cms.m_pParam.m_triangleDataArray.Take(3))
                            Console.WriteLine($"    tri: prim {t.m_primitiveDataIndex} tri {t.m_triangleDataIndex} v {t.m_vertexIndexA},{t.m_vertexIndexB},{t.m_vertexIndexC}");
                }
            }
        }
        foreach (var (k, d) in counts) Console.WriteLine($"{k}: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(8).Select(x => $"{x.Key} x{x.Value}"))}");
    }
}
