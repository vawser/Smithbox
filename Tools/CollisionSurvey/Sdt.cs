using Havok.hk2016;
using Havok.Shared;
using SoulsFormats;
using System.Numerics;

// The Sekiro (Havok 2016) compressed meshes: the values the builder fills, and a rebuild check.
// SDT keeps DS3's mesh tree (packed section words) under 2018's shape header.
static class Sdt
{
    public static void Survey(BXF4 bxf, byte[] compendium, bool rebuild, bool detail, int maxShown)
    {
        var profile = Havok.HavokProfile.SDT;
        var unifiedCompendium = compendium == null ? null : Havok.Files.HavokFile.ReadCompendium(compendium, profile);
        var counts = new Dictionary<string, Dictionary<string, int>>();
        void Count(string k, object v) { if (!counts.TryGetValue(k, out var d)) counts[k] = d = new(); var s = v?.ToString() ?? "null"; d[s] = d.GetValueOrDefault(s) + 1; }
        int shown = 0;

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx")))
        {
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            Havok.HavokDocument document;
            hkRootLevelContainer root;
            try { document = Havok.Files.HavokFile.Read(bytes, profile, unifiedCompendium); root = (hkRootLevelContainer)document.Root; }
            catch (Exception e) { Count("read failed", e.Message.Substring(0, Math.Min(60, e.Message.Length))); continue; }

            Count("compendium", document.Compendium != null);

            foreach (var system in root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().SelectMany(s => s.m_systemDatas))
            {
                Count("bodies per system", system.m_bodyCinfos.Count);
                Count("referenced objects", string.Join(",", system.m_referencedObjects.Select(o => o?.GetType().Name).Distinct()));
                foreach (var body in system.m_bodyCinfos)
                {
                    Count("shape", body.m_shape?.GetType().Name);
                    Count("attachedBody", body.m_attachedBody);

                    if (body.m_shape is hknpExternMeshShape ext)
                    {
                        Count("extern flags", ext.m_flags);
                        Count("extern type/dispatch", $"{ext.m_type}/{ext.m_dispatchType}");
                        Count("extern bv data", ext.m_boundingVolumeData?.GetType().Name);
                        Count("extern geometry", ext.m_geometry?.GetType().Name);
                        if (ext.m_boundingVolumeData is hknpExternMeshShapeData ed)
                            Count("extern simd nodes", ed.m_simdTree?.m_nodes?.Count is int n ? (n <= 3 ? n.ToString() : ">3") : "null");
                        Console.WriteLine($"  extern {file.Name} {body.m_name}: flags {ext.m_flags} radius {ext.m_convexRadius} keyBits {ext.m_numShapeKeyBits} tag {ext.m_shapeTagCodecInfo:X} mat {ext.m_materialTable?.GetType().Name}");
                    }

                    if (body.m_shape is not fsnpCustomParamCompressedMeshShape cms) continue;

                    if (rebuild)
                    {
                        try
                        {
                            var errors = SdtRebuild.Check(document, unifiedCompendium, body, cms, out var stats);
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

                    var data = cms.m_data as hknpCompressedMeshShapeData;
                    Count("data", cms.m_data?.GetType().Name);
                    if (data == null) continue;
                    var tree = data.m_meshTree;
                    Count("flags", cms.m_flags); Count("radius", cms.m_convexRadius); Count("tagCodec", $"{cms.m_shapeTagCodecInfo:X}");
                    Count("type", cms.m_type); Count("dispatch", cms.m_dispatchType); Count("userData", cms.m_userData);
                    Count("properties", cms.m_properties?.GetType().Name); Count("materialTable", cms.m_materialTable?.GetType().Name);
                    Count("interior bits", cms.m_triangleIsInterior?.m_storage?.m_numBits == 0 ? "0" : cms.m_triangleIsInterior?.m_storage?.m_numBits == tree.m_maxKeyValue + 1 ? "maxKey+1" : "other");
                    Count("interior words all 0", cms.m_triangleIsInterior?.m_storage?.m_words?.All(w => w == 0));
                    Count("simd nodes", data.m_simdTree?.m_nodes?.Count is int c ? (c == 0 ? "0" : c <= 3 ? c.ToString() : ">3") : "null");
                    Count("connectivity", $"headers {(data.m_connectivity.m_headers.Count == 0 ? "0" : data.m_connectivity.m_headers.Count == tree.m_sections.Count ? "=sections" : "other")} local {(data.m_connectivity.m_localLinks.Count > 0 ? ">0" : "0")}");
                    Count("keyBits==bitsPerKey", cms.m_numShapeKeyBits == tree.m_bitsPerKey);
                    Count("bitsPerKey==bitlen(maxKey)", tree.m_bitsPerKey == 32 - BitOperations.LeadingZeroCount(tree.m_maxKeyValue));
                    Count("numPrimitiveKeys==prims", tree.m_numPrimitiveKeys == tree.m_primitives.Count);
                    Count("flatConvex", tree.m_primitiveStoresIsFlatConvex);
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
                        Count("sec domain w", $"{s.m_domain.m_min.W},{s.m_domain.m_max.W}");
                        Count("sec numSharedIndices==next-first", i + 1 < tree.m_sections.Count
                            ? ((tree.m_sections[i + 1].m_sharedVertices.m_data >> 8) - (s.m_sharedVertices.m_data >> 8) == s.m_numSharedIndices).ToString() : "last");
                    }
                    Count("pParam", cms.m_pParam == null ? "null" : $"v{cms.m_pParam.m_version:X} vs{cms.m_pParam.m_vertexDataStride} ts{cms.m_pParam.m_triangleDataStride}");
                    Count("triToKey==keys", cms.m_triangleIndexToShapeKey?.Count == tree.m_numPrimitiveKeys);
                    if (cms.m_pParam != null)
                        foreach (var pd in cms.m_pParam.m_primitiveDataArray)
                        {
                            Count("matName", pd.m_materialNameData);
                            Count("vtx FF", pd.m_vertexData == null ? "null" : pd.m_vertexData.All(b => b == 0xFF).ToString());
                            Count("tri/prim data empty", pd.m_triangleData.Count + pd.m_primitiveData.Count == 0);
                        }

                    if (detail && shown < maxShown) SdtSimd.Dump(cms);
                    if (shown++ < maxShown)
                    {
                        Console.WriteLine($"==== {file.Name} {body.m_name}: flags {cms.m_flags} keyBits {cms.m_numShapeKeyBits} radius {cms.m_convexRadius} tag {cms.m_shapeTagCodecInfo:X} sections {tree.m_sections.Count} prims {tree.m_primitives.Count} keys {tree.m_numPrimitiveKeys} maxKey {tree.m_maxKeyValue} bits {tree.m_bitsPerKey}");
                        Console.WriteLine($"  body: mat {body.m_materialId} filter {body.m_collisionFilterInfo:X} flags {body.m_flags} motion {body.m_motionId} quality {body.m_qualityId} motionType {body.m_motionType}");
                        Console.WriteLine($"  interior {cms.m_triangleIsInterior?.m_storage?.m_numBits}");
                        Console.WriteLine($"  simd {data.m_simdTree?.m_nodes?.Count}: {string.Join(" | ", data.m_simdTree?.m_nodes?.Take(3).Select(n => $"{n.m_data[0]:X},{n.m_data[1]:X},{n.m_data[2]:X},{n.m_data[3]:X} lx {n.m_lx}") ?? [])}");
                        foreach (var s in tree.m_sections.Take(2))
                            Console.WriteLine($"  section: packed {s.m_firstPackedVertex}+{s.m_numPackedVertices} shared.m_data {s.m_sharedVertices.m_data:X} numShared {s.m_numSharedIndices} prims {s.m_primitives.m_data:X} runs {s.m_dataRuns.m_data:X} leaf {s.m_leafIndex} domain {s.m_domain.m_min}-{s.m_domain.m_max} nodes {s.m_nodes.Count}");
                        Console.WriteLine($"  runs: {string.Join(" ", tree.m_primitiveDataRuns.Take(5).Select(r => $"[{r.m_value.m_data} i{r.m_index} c{r.m_count}]"))}");
                        Console.WriteLine($"  triToKey: {string.Join(",", cms.m_triangleIndexToShapeKey?.Take(6).Select(k => k.ToString("X8")) ?? [])}");
                        if (detail && cms.m_pParam != null)
                            foreach (var t in cms.m_pParam.m_triangleDataArray.Take(3))
                                Console.WriteLine($"    tri: prim {t.m_primitiveDataIndex} tri {t.m_triangleDataIndex} v {t.m_vertexIndexA},{t.m_vertexIndexB},{t.m_vertexIndexC}");
                    }
                }
            }
        }
        foreach (var (k, d) in counts) Console.WriteLine($"{k}: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(8).Select(x => $"{x.Key} x{x.Value}"))}");
    }
}
