using Andre.Core;
using Andre.IO.VFS;
using Havok.hk2018;
using Havok.hk2018.hkcdStaticMeshTree;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// Dumps the layout of the compressed mesh shapes in a map's collision, so the Collision Generator's
// compressed mesh builder can match what the game ships.
//   CollisionSurvey <GAME> <mapId> [max files] [h|l] [file filter regex] [detail]
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var gameName = args[0];
var game = Enum.Parse<Game>(gameName);
var mapId = args[1];
var maxFiles = args.Length > 2 ? int.Parse(args[2]) : 3;
var type = args.Length > 3 ? args[3] : "h";
var filter = new Regex(args.Length > 4 ? args[4] : ".", RegexOptions.IgnoreCase);
var detail = args.Length > 5 && args[5] == "detail";
var rebuild = args.Length > 5 && args[5] == "rebuild";

using var fs = BuildFS(game, gamePaths[gameName]);

// How the low and high collision of each model follow each other (Collision Editor linked layers)
if (type == "link") { Link.Survey(fs, gameName, mapId, maxFiles, filter); return; }

// The Collision Editor's Create tab: a new file made from a template, appended to the binder and read back
if (type == "create") { Create.Survey(fs, gameName, mapId); return; }

var bhdPath = game is Game.DS3 or Game.SDT or Game.BB
    ? $"/map/{mapId}/{type}{mapId.Substring(1)}.hkxbhd"
    : $"/map/{mapId.Substring(0, 3)}/{mapId}/{type}{mapId.Substring(1)}.hkxbhd";
var bdtPath = Path.ChangeExtension(bhdPath, ".hkxbdt");
var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(bdtPath).Value.ToArray());

var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
byte[] compendium = null;
if (compFile != null) { compendium = compFile.Bytes.ToArray(); if (DCX.Is(compendium)) compendium = DCX.Decompress(compendium).ToArray(); }

if (game == Game.AC6) { Ac6.Survey(bxf, compendium); return; }
if (game == Game.DS3) { Ds3.Survey(bxf, rebuild, detail, maxFiles); return; }
if (game == Game.BB) { Ds3.Survey(bxf, rebuild, detail, maxFiles, Havok.HavokProfile.BB); return; }
if (game == Game.SDT) { Sdt.Survey(bxf, compendium, rebuild, detail, maxFiles); return; }

// Read and written through the unified Havok library, with the binder's compendium, as Smithbox does
var profile = Havok.HavokProfile.FromGame(gameName);
var unifiedCompendium = compendium == null ? null : Havok.Files.HavokFile.ReadCompendium(compendium, profile);

var aggregate = new Dictionary<string, Dictionary<string, int>>();
void Count(string key, object value)
{
    if (!aggregate.TryGetValue(key, out var d)) aggregate[key] = d = new();
    var s = value?.ToString() ?? "null";
    d[s] = d.GetValueOrDefault(s) + 1;
}

var shown = 0;
foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx") && filter.IsMatch(f.Name)))
{
    var bytes = file.Bytes.ToArray();
    if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();

    Havok.HavokDocument document;
    hkRootLevelContainer root;
    try
    {
        document = Havok.Files.HavokFile.Read(bytes, profile, unifiedCompendium);
        root = (hkRootLevelContainer)document.Root;
    }
    catch (Exception e) { Console.WriteLine($"{file.Name}: read failed {e.Message}"); continue; }

    var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().FirstOrDefault();
    if (scene == null) continue;

    foreach (var system in scene.m_systemDatas)
    {
        foreach (var body in system.m_bodyCinfos)
        {
            Count("shape type", body.m_shape?.GetType().Name);
            if (body.m_shape is not hknpCompressedMeshShape cms) continue;

            var data = (hknpCompressedMeshShapeData)cms.m_data;
            var tree = data.m_meshTree;

            if (rebuild && cms is fsnpCustomParamCompressedMeshShape vanillaShape)
            {
                var vanillaErrors = new List<string>();
                var vanillaPrims = Rebuild.CheckVanillaTrees(vanillaShape, vanillaErrors);

                // Write the rebuilt shape in place of the vanilla one and read it back
                fsnpCustomParamCompressedMeshShape RoundTrip(fsnpCustomParamCompressedMeshShape rebuilt)
                {
                    body.m_shape = rebuilt;
                    try
                    {
                        var written = Havok.Files.HavokFile.Write(document);
                        var reread = (hkRootLevelContainer)Havok.Files.HavokFile.Read(written, profile, unifiedCompendium).Root;
                        return (fsnpCustomParamCompressedMeshShape)reread.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First()
                            .m_systemDatas.SelectMany(x => x.m_bodyCinfos).First(b => b.m_name == body.m_name).m_shape;
                    }
                    finally
                    {
                        body.m_shape = vanillaShape;
                    }
                }

                try
                {
                    var errors = Rebuild.Check(vanillaShape, RoundTrip, out var stats);
                    errors.InsertRange(0, vanillaErrors.Take(3));
                    Count("rebuild ok", errors.Count == 0);
                    Console.WriteLine($"{file.Name} {body.m_name}: {(errors.Count == 0 ? "OK" : $"{errors.Count} errors")} vanilla prims checked {vanillaPrims} | {stats}");
                    foreach (var e in errors.Take(5)) Console.WriteLine("    " + e);
                }
                catch (Exception e)
                {
                    Count("rebuild ok", "exception");
                    Console.WriteLine($"{file.Name} {body.m_name}: exception {e}");
                }
                continue;
            }

            Count("flags", cms.m_flags);
            Count("type", cms.m_type);
            Count("dispatch", cms.m_dispatchType);
            Count("convexRadius", cms.m_convexRadius);
            Count("userData", cms.m_userData);
            Count("properties", cms.m_properties?.GetType().Name);
            Count("shapeTagCodecInfo", $"0x{cms.m_shapeTagCodecInfo:X}");
            Count("materialTable", cms.m_materialTable?.GetType().Name);
            Count("externShapes", cms.m_externShapes.Count);
            Count("hasSimdTree", data.m_hasSimdTree);
            Count("simd nodes", data.m_simdTree.m_nodes.Count);
            Count("simd compact", data.m_simdTree.m_isCompact);
            Count("connectivity headers==sections", data.m_connectivity.m_headers.Count == tree.m_sections.Count);
            Count("connectivity has local links", data.m_connectivity.m_localLinks.Count > 0);
            Count("interior bits == prims", cms.m_triangleIsInterior.m_storage.m_numBits == tree.m_primitives.Count);
            Count("interior bits", cms.m_triangleIsInterior.m_storage.m_numBits == 0 ? "0" : ">0");
            Count("primitiveStoresIsFlatConvex", tree.m_primitiveStoresIsFlatConvex);
            Count("bitsPerKey", tree.m_bitsPerKey);
            Count("numShapeKeyBits", cms.m_numShapeKeyBits);
            Count("numShapeKeyBits - bitsPerKey", cms.m_numShapeKeyBits - tree.m_bitsPerKey);
            Count("maxKeyValue==(1<<bitsPerKey)-1", tree.m_maxKeyValue == (1u << tree.m_bitsPerKey) - 1);
            Count("numPrimitiveKeys==prims", tree.m_numPrimitiveKeys == tree.m_primitives.Count);
            Count("tree nodes == 2*sections-1", tree.m_nodes.Count == 2 * tree.m_sections.Count - 1);
            Count("domain w", $"{tree.m_domain.m_min.W},{tree.m_domain.m_max.W}");
            Count("dataRun values distinct", string.Join(",", tree.m_primitiveDataRuns.Select(r => r.m_value).Distinct().OrderBy(x => x).Take(6)));

            var bitsNeeded = (int)Math.Ceiling(Math.Log2(tree.m_sections.Count));
            Count("bitsPerKey - log2(sections)", tree.m_bitsPerKey - bitsNeeded);

            foreach (var s in tree.m_sections)
            {
                Count("section page", s.m_page);
                Count("section layerData", s.m_layerData);
                Count("section flags", s.m_flags);
                Count("section numDataRuns", s.m_numDataRuns);
                Count("section nodes==2*prims-1", s.m_nodes.Count == 2 * s.m_numPrimitives - 1);
                Count("section domain w", $"{s.m_domain.m_min.W},{s.m_domain.m_max.W}");
            }
            for (int i = 0; i < tree.m_sections.Count; i++)
                Count("section leafIndex == i", tree.m_sections[i].m_leafIndex == i);

            if (cms is fsnpCustomParamCompressedMeshShape fsnp)
            {
                Count("fsnp pParam", fsnp.m_pParam == null ? "null" : $"v{fsnp.m_pParam.m_version} vs{fsnp.m_pParam.m_vertexDataStride} ts{fsnp.m_pParam.m_triangleDataStride}");
                Count("fsnp triToKey == prims", fsnp.m_triangleIndexToShapeKey.Count == tree.m_primitives.Count);
                Count("fsnp triToKey count", fsnp.m_triangleIndexToShapeKey.Count == 0 ? "0" : ">0");
                if (fsnp.m_pParam != null)
                {
                    Count("fsnp triData==prims", fsnp.m_pParam.m_triangleDataArray.Count == tree.m_primitives.Count);
                    Count("fsnp primData count", fsnp.m_pParam.m_primitiveDataArray.Count);
                    foreach (var pd in fsnp.m_pParam.m_primitiveDataArray) { Count("matName", pd.m_materialNameData); Count("vertexData==FF", pd.m_vertexData.All(b => b == 0xFF)); Count("tri/prim data empty", pd.m_triangleData.Count + pd.m_primitiveData.Count == 0); }
                    var lowMask = (1u << (32 - tree.m_bitsPerKey)) - 1; Count("triToKey low bits all 1", fsnp.m_triangleIndexToShapeKey.All(k => (k & lowMask) == lowMask));
                }
            }

            if (shown >= maxFiles) continue;
            shown++;

            Console.WriteLine($"==== {file.Name} body '{body.m_name}' {cms.GetType().Name}");
            Console.WriteLine($"  body: mat {body.m_materialId} filter 0x{body.m_collisionFilterInfo:X} flags {body.m_flags} motion {body.m_motionId} quality {body.m_qualityId} pos {body.m_position} rot {body.m_orientation} userData {body.m_userData}");
            Console.WriteLine($"  shape: flags {cms.m_flags} keyBits {cms.m_numShapeKeyBits} radius {cms.m_convexRadius} tagCodec 0x{cms.m_shapeTagCodecInfo:X} matTable {cms.m_materialTable?.GetType().Name} interior bits {cms.m_triangleIsInterior.m_storage.m_numBits} words {cms.m_triangleIsInterior.m_storage.m_words?.Count}");
            Console.WriteLine($"  tree: keys {tree.m_numPrimitiveKeys} bitsPerKey {tree.m_bitsPerKey} maxKey {tree.m_maxKeyValue} flatConvex {tree.m_primitiveStoresIsFlatConvex} sections {tree.m_sections.Count} prims {tree.m_primitives.Count} sharedIdx {tree.m_sharedVerticesIndex.Count} packed {tree.m_packedVertices.Count} shared {tree.m_sharedVertices.Count} runs {tree.m_primitiveDataRuns.Count} nodes {tree.m_nodes.Count}");
            Console.WriteLine($"  domain {tree.m_domain.m_min} - {tree.m_domain.m_max}");
            Console.WriteLine($"  simd: nodes {data.m_simdTree.m_nodes.Count} compact {data.m_simdTree.m_isCompact} has {data.m_hasSimdTree}");
            Console.WriteLine($"  connectivity: headers {data.m_connectivity.m_headers.Count} local {data.m_connectivity.m_localLinks.Count} global {data.m_connectivity.m_globalLinks.Count}");

            if (cms.m_materialTable is hknpMaterialPalette pal)
                foreach (var e in pal.m_entries.Take(4))
                    Console.WriteLine($"  palette: id {e.m_materialId} name {e.m_name} mat {e.m_material?.GetType().Name}");

            if (cms is fsnpCustomParamCompressedMeshShape f2)
            {
                Console.WriteLine($"  fsnp triToKey {f2.m_triangleIndexToShapeKey.Count}: {string.Join(",", f2.m_triangleIndexToShapeKey.Take(12).Select(x => $"0x{x:X}"))}");
                if (f2.m_pParam is { } p)
                {
                    Console.WriteLine($"  pParam v{p.m_version} vstride {p.m_vertexDataStride} tstride {p.m_triangleDataStride} tris {p.m_triangleDataArray.Count} prims {p.m_primitiveDataArray.Count}");
                    foreach (var t in p.m_triangleDataArray.Take(6))
                        Console.WriteLine($"    tri: prim {t.m_primitiveDataIndex} tri {t.m_triangleDataIndex} v {t.m_vertexIndexA},{t.m_vertexIndexB},{t.m_vertexIndexC}");
                    foreach (var pd in p.m_primitiveDataArray.Take(4))
                        Console.WriteLine($"    primData: vtx [{pd.m_vertexData.Count}] tri [{pd.m_triangleData.Count}] {string.Join(" ", pd.m_triangleData.Take(16).Select(b => b.ToString("X2")))} prim [{pd.m_primitiveData.Count}] {string.Join(" ", pd.m_primitiveData.Take(16).Select(b => b.ToString("X2")))} matName 0x{pd.m_materialNameData:X}");
                }
            }

            Console.WriteLine($"  runs: {string.Join(" ", tree.m_primitiveDataRuns.Take(10).Select(r => $"[{r.m_value} i{r.m_index} c{r.m_count}]"))}");
            Console.WriteLine($"  tree nodes: {string.Join(" ", tree.m_nodes.Take(detail ? 40 : 9).Select(n => $"{n.m_xyz[0]:X2}{n.m_xyz[1]:X2}{n.m_xyz[2]:X2}:{n.m_hiData:X2}{n.m_loData:X2}"))}");

            for (int i = 0; i < Math.Min(tree.m_sections.Count, detail ? 6 : 3); i++)
            {
                var s = tree.m_sections[i];
                Console.WriteLine($"  section {i}: codec [{string.Join(",", s.m_codecParms.Select(x => x.ToString("G6")))}] packed {s.m_firstPackedVertexIndex}+{s.m_numPackedVertices} shared {s.m_firstSharedVertexIndex} prims {s.m_firstPrimitiveIndex}+{s.m_numPrimitives} runs {s.m_firstDataRunIndex}+{s.m_numDataRuns} page {s.m_page} leaf {s.m_leafIndex} layer {s.m_layerData} flags {s.m_flags} nodes {s.m_nodes.Count}");
                Console.WriteLine($"    domain {s.m_domain.m_min} - {s.m_domain.m_max}");
                Console.WriteLine($"    nodes: {string.Join(" ", s.m_nodes.Take(detail ? 40 : 9).Select(n => $"{n.m_xyz[0]:X2}{n.m_xyz[1]:X2}{n.m_xyz[2]:X2}:{n.m_data:X2}"))}");
                var prims = tree.m_primitives.Skip((int)s.m_firstPrimitiveIndex).Take(s.m_numPrimitives).ToList();
                Console.WriteLine($"    prims: {string.Join(" ", prims.Take(detail ? 40 : 10).Select(p => $"{p.m_indices[0]},{p.m_indices[1]},{p.m_indices[2]},{p.m_indices[3]}"))}");
                Console.WriteLine($"    quads {prims.Count(p => p.m_indices[2] != p.m_indices[3])}/{prims.Count}, shared count {tree.m_sharedVerticesIndex.Count}");
                if (detail && i < data.m_connectivity.m_headers.Count)
                {
                    var h = data.m_connectivity.m_headers[i];
                    Console.WriteLine($"    conn header: local {h.m_baseLocal} global {h.m_baseGlobal}");
                }
            }

            // The domain of a section vs the codec: is scale = extent / (2047,2047,1023)?
            var s0 = tree.m_sections[0];
            var ext = s0.m_domain.m_max - s0.m_domain.m_min;
            Console.WriteLine($"  section0 extent/codec scale: {ext.X / s0.m_codecParms[3]:G6} {ext.Y / s0.m_codecParms[4]:G6} {ext.Z / s0.m_codecParms[5]:G6} offset-min {s0.m_codecParms[0] - s0.m_domain.m_min.X:G4} {s0.m_codecParms[1] - s0.m_domain.m_min.Y:G4} {s0.m_codecParms[2] - s0.m_domain.m_min.Z:G4}");

            if (detail && data.m_connectivity.m_localLinks.Count > 0)
            {
                Console.WriteLine($"  local links: {string.Join(" ", data.m_connectivity.m_localLinks.Take(48).Select(b => b.ToString("X2")))}");
                Console.WriteLine($"  global links: {string.Join(" ", data.m_connectivity.m_globalLinks.Take(16).Select(b => b.m_value.ToString("X")))}");
            }
            if (detail && data.m_simdTree.m_nodes.Count > 0)
            {
                foreach (var n in data.m_simdTree.m_nodes.Take(16))
                    Console.WriteLine($"  simd node leaf {n.m_isLeaf} data {string.Join(",", n.m_data.Select(d => $"0x{d:X}"))} x {n.m_lx}-{n.m_hx} y {n.m_ly}-{n.m_hy} z {n.m_lz}-{n.m_hz}");
            }
            if (detail)
            {
                Console.WriteLine($"  interior words: {string.Join(" ", cms.m_triangleIsInterior.m_storage.m_words.Take(16).Select(w => w.ToString("X8")))}");
                if (cms is fsnpCustomParamCompressedMeshShape f3 && f3.m_pParam != null)
                {
                    foreach (var pd in f3.m_pParam.m_primitiveDataArray.Take(3))
                        Console.WriteLine($"  vertexData: {string.Join(" ", pd.m_vertexData.Take(32).Select(b => b.ToString("X2")))}");
                    Console.WriteLine($"  all triToKey: {string.Join(",", f3.m_triangleIndexToShapeKey.Take(40).Select(k => $"{(k >> (32 - tree.m_bitsPerKey)):X}"))}");
                    // Shape key -> primitive: does the triangle data list the primitive's vertices?
                    Console.WriteLine($"  key-ordered pParam tris: {string.Join(" ", f3.m_triangleIndexToShapeKey.Select((k, i) => (k >> (32 - tree.m_bitsPerKey), i)).OrderBy(x => x.Item1).Take(12).Select(x => $"{x.Item1:X}<-{x.i}"))}");
                }
            }
        }
    }
}

Console.WriteLine();
foreach (var (k, d) in aggregate)
    Console.WriteLine($"{k}: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(10).Select(x => $"{x.Key} x{x.Value}"))}");

static VirtualFileSystem BuildFS(Game game, string root)
{
    // BB is kept unpacked, as loose files
    if (game == Game.BB) return new RealVirtualFileSystem(Path.Combine(root, "Loose", "dvdroot_ps4"), false);
    var real = new RealVirtualFileSystem(root, false);
    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}
