using SoulsFormats;
using System.Numerics;
using System.Text;

// Reads every Demon's Souls FLVER0 (chr, obj, parts, map pieces) and counts the features that the renderer's
// FLVER0 path (FlverResource.LoadInternalDeS / ProcessMesh(FLVER0.Mesh)) handles differently from FLVER2.
// Usage: DesModelSurvey <USRDIR> <report.md>

if (args.Length > 0 && args[0] == "dcxpairs")
{
    DcxPairs.Run(args[1]);
    return;
}

if (args.Length > 0 && args[0] == "lightmap")
{
    LightmapSurvey.Run(args[1], args.Length > 2 ? args[2] : "des_lightmap_survey.md");
    return;
}

var root = args.Length > 0 ? args[0] : @"G:\Game Data Repository\Demon's Souls\PS3_GAME\USRDIR";
var reportPath = args.Length > 1 ? args[1] : "des_model_survey.md";

var mtdNames = LoadMtdNames(Path.Combine(root, "mtd", "mtd.mtdbnd"));
var stats = new Stats();
var allTex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var misses = new List<(string Cat, string Model, string Tex, string Path)>();
// The textures of each container, keyed as a FLVER texture path would name it: chr/c2000, obj/o0000, parts/wp_a_1509, map/m02
var containers = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
var commonParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (var common in new[] { "parts/common_body.tpf", "parts/common_ghost.tpf", "facegen/facegen.tpf", "other/systex_tex.tpf", "other/envlight.tpf" })
    AddTpf(commonParts, Path.Combine(root, common));
containers["common"] = commonParts;
// Which kinds of mesh share a material: decides whether placement can stay a material flag
var sharedMaterialModels = 0;

// --- Characters: chr/cXXXX/cXXXX.chrbnd, textures in the loose cXXXX.tpf beside it and any TPF in the binder
foreach (var dir in Directory.GetDirectories(Path.Combine(root, "chr")))
{
    var id = Path.GetFileName(dir);
    var bndPath = Path.Combine(dir, $"{id}.chrbnd");
    var tex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    AddTpf(tex, Path.Combine(dir, $"{id}.tpf"));
    containers[$"chr/{id}"] = tex;

    if (!File.Exists(bndPath))
    {
        // c0000 style folders without a chrbnd still have a loose flver
        var loose = Path.Combine(dir, $"{id}.flver");
        if (File.Exists(loose))
            Survey("chr", id, File.ReadAllBytes(loose), tex, null);
        continue;
    }

    var bnd = BND3.Read(bndPath);
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase)))
        AddTpf(tex, f.Bytes.ToArray());
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
        Survey("chr", $"{id}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray(), tex, null);
}

// --- Objects and parts: one binder each, textures inside it
foreach (var (cat, folder, ext) in new[] { ("obj", "obj", ".objbnd"), ("parts", "parts", ".partsbnd") })
{
    foreach (var bndPath in Directory.GetFiles(Path.Combine(root, folder), "*" + ext))
    {
        BND3 bnd;
        try { bnd = BND3.Read(bndPath); }
        catch (Exception e) { stats.Fail(cat, Path.GetFileName(bndPath), "binder: " + e.Message); continue; }

        var tex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase)))
            AddTpf(tex, f.Bytes.ToArray());
        containers[$"{cat}/{Path.GetFileNameWithoutExtension(bndPath)}"] = tex;
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
            Survey(cat, $"{Path.GetFileNameWithoutExtension(bndPath)}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray(), tex, null);
    }
}

// --- Map pieces: map/mAA_BB_CC_DD/*.flver. The renderer loads the area TPFs map/mAA/mAA_NNNN.tpf;
// the map's own map/mAA_BB_CC_DD/mAA_BB_CC_DD.tpf is tracked apart, as it is not loaded
foreach (var dir in Directory.GetDirectories(Path.Combine(root, "map"), "m??_??_??_??"))
{
    var mapId = Path.GetFileName(dir);
    var area = mapId.Substring(0, 3);
    var areaTex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var areaDir = Path.Combine(root, "map", area);
    if (Directory.Exists(areaDir))
        foreach (var tpf in Directory.GetFiles(areaDir, "*.tpf"))
            AddTpf(areaTex, tpf);

    var mapTex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    AddTpf(mapTex, Path.Combine(dir, $"{mapId}.tpf"));
    containers[$"map/{area}"] = areaTex;
    containers[$"mapown/{mapId}"] = mapTex;

    foreach (var flver in Directory.GetFiles(dir, "*.flver"))
        Survey("map", $"{mapId}/{Path.GetFileName(flver)}", File.ReadAllBytes(flver), areaTex, mapTex);
}

// Missing textures: in another container of the game (a cross reference the pool only fills if that container is loaded), or nowhere
foreach (var m in misses)
{
    var p = (m.Path ?? "").Replace('\\', '/').ToLowerInvariant();
    var src = p.Contains("/chr/") ? "chr" : p.Contains("/obj/") ? "obj" : p.Contains("/parts/") ? "parts" : p.Contains("/map/") ? "map" : "other";
    var where = allTex.Contains(m.Tex) ? "elsewhere in game" : "nowhere";
    stats.Inc($"missing tex -> {src} path, {where}", m.Cat);
    stats.Sample($"missing tex -> {src} path, {where}", $"{m.Model}: {m.Path}");

    // Would loading the container the path names find it?
    var segs = p.Split('/', StringSplitOptions.RemoveEmptyEntries);
    string key = null;
    var ci = Array.IndexOf(segs, src);
    if (src is "chr" or "obj" && ci >= 0 && ci + 1 < segs.Length) key = $"{src}/{segs[ci + 1]}";
    else if (src == "map" && ci >= 0 && ci + 1 < segs.Length && segs[ci + 1].Length >= 3) key = $"map/{segs[ci + 1][..3]}";
    else if (src == "parts")
    {
        var ti = Array.LastIndexOf(segs, "tex");
        key = ti > 0 && segs[ti - 1] != "parts" ? $"parts/{segs[ti - 1]}" : "common";
    }

    string outcome;
    if (key != null && containers.TryGetValue(key, out var set) && set.Contains(m.Tex)) outcome = "path names container";
    else if (containers["common"].Contains(m.Tex)) outcome = "in common TPFs";
    else if (m.Cat == "map" && containers.Where(x => x.Key.StartsWith("mapown/")).Any(x => x.Value.Contains(m.Tex))) outcome = "in a map's own TPF";
    else if (allTex.Contains(m.Tex)) outcome = "elsewhere, path wrong";
    else outcome = "nowhere";
    stats.Inc($"missing tex resolve {outcome}", m.Cat);
    stats.Sample($"missing tex resolve {outcome}", $"{m.Model}: {m.Path}");
}
stats.Inc($"bind node shared material models {sharedMaterialModels}");

File.WriteAllText(reportPath, stats.Report());
Console.WriteLine($"Wrote {reportPath}");

void Survey(string cat, string name, byte[] bytes, HashSet<string> tex, HashSet<string> altTex)
{
    FLVER0 flver;
    try { flver = FLVER0.Read(bytes); }
    catch (Exception e) { stats.Fail(cat, name, e.GetType().Name + ": " + e.Message); return; }

    var c = stats.Cat(cat);
    c.Models++;
    stats.Inc($"version {flver.Header.Version:X}");

    var worldOf = new Matrix4x4[flver.Nodes.Count];
    for (var i = 0; i < flver.Nodes.Count; i++)
        worldOf[i] = flver.ComputeBoneWorldMatrix(i);

    var issues = new HashSet<string>();

    // Materials
    foreach (var mat in flver.Materials)
    {
        var mtd = Path.GetFileNameWithoutExtension(mat.MTD ?? "");
        stats.Inc($"mtd {mtd}", cat);
        if (!mtdNames.Contains(mtd)) issues.Add("mtd missing from mtd.mtdbnd: " + mtd);

        if (mat.Layouts.Count > 1) issues.Add("material has >1 layout");

        foreach (var t in mat.Textures)
        {
            stats.Inc($"texparam {t.ParamName}");
            var texName = Path.GetFileNameWithoutExtension((t.Path ?? "").Replace('\\', '/'));
            if (texName == "") { issues.Add("texture with empty path"); continue; }

            if (tex.Contains(texName)) c.TexFound++;
            else if (altTex != null && altTex.Contains(texName)) { c.TexInMapTpfOnly++; issues.Add("texture only in the map's own TPF"); }
            else { c.TexMissing++; misses.Add((cat, name, texName, t.Path)); stats.Sample($"missing texture ({cat})", $"{name}: {t.ParamName} {t.Path}"); }
        }

        foreach (var layout in mat.Layouts)
        {
            var sig = string.Join(" ", layout.Select(m => $"{m.Semantic}{m.Index}:{m.Type}"));
            stats.Inc($"layout {sig}", cat);
        }
    }

    // A material used by both bind-pose and rigid meshes
    if (flver.Meshes.GroupBy(m => m.MaterialIndex).Any(g => g.Select(m => m.Dynamic).Distinct().Count() > 1))
    {
        sharedMaterialModels++;
        issues.Add("material shared by bind-pose and rigid meshes");
    }

    // Meshes
    foreach (var mesh in flver.Meshes)
    {
        c.Meshes++;
        var mat = flver.Materials[mesh.MaterialIndex];
        var layout0 = mat.Layouts.Count > 0 ? mat.Layouts[0] : null;
        var meshLayout = mesh.LayoutIndex < mat.Layouts.Count ? mat.Layouts[mesh.LayoutIndex] : null;

        if (mesh.LayoutIndex != 0) { c.MeshLayoutNot0++; issues.Add("mesh layout index != 0 (renderer reads Layouts[0])"); }
        if (!mesh.CullBackfaces) { c.DoubleSided++; issues.Add("double sided mesh (renderer always culls)"); }
        if (mesh.TriangleStrip) c.Strips++;
        if (mesh.Vertices.Count > 65535) { c.Over16Bit++; issues.Add(">65535 vertices"); }
        stats.Inc($"dynamic {mesh.Dynamic}", cat);

        var normalType = meshLayout?.FirstOrDefault(m => m.Semantic == FLVER.LayoutSemantic.Normal)?.Type;
        var renderNormalW = layout0 != null && layout0.Any(m => m.Semantic == FLVER.LayoutSemantic.Normal &&
            m.Type is FLVER.LayoutType.UByte4 or FLVER.LayoutType.Byte4E or FLVER.LayoutType.Byte4);
        var hasIdx = layout0?.Any(m => m.Semantic == FLVER.LayoutSemantic.BoneIndices) == true;
        var hasWgt = layout0?.Any(m => m.Semantic == FLVER.LayoutSemantic.BoneWeights) == true;
        stats.Inc($"normal {normalType} dyn{mesh.Dynamic} idx{(hasIdx ? 1 : 0)} wgt{(hasWgt ? 1 : 0)}", cat);

        // The renderer skins by bones[NormalW] for the whole material when the normal is a 4-byte type
        if (mesh.Dynamic == 1 && renderNormalW)
        {
            // Bind pose meshes should be drawn untransformed; non-zero NormalW moves them
            if (mesh.Vertices.Any(v => v.NormalW != 0 && !IsIdentity(worldOf[Math.Min(v.NormalW, worldOf.Length - 1)])))
            {
                c.SkinnedButNormalW++;
                issues.Add("bind-pose mesh drawn with bones[NormalW]");
            }
        }

        if (mesh.Dynamic == 0)
        {
            c.Static++;
            var used = mesh.UsedBoneCount;
            var nws = mesh.Vertices.Select(v => v.NormalW).Distinct().ToList();
            var maxNw = nws.Count > 0 ? nws.Max() : 0;
            if (maxNw >= used && used > 0) c.NormalWBeyondLocal++;
            if (maxNw >= flver.Nodes.Count) c.NormalWBeyondNodes++;

            // Where the game would place each vertex: the node NormalW names through the mesh's bone list
            var needsLocalMap = nws.Any(nw => nw < used && mesh.BoneIndices[nw] != nw);
            if (needsLocalMap) { c.NormalWNeedsRemap++; issues.Add("static mesh NormalW is local to the mesh bone list"); }

            var nonIdentity = nws.Any(nw =>
            {
                var node = nw < used ? mesh.BoneIndices[nw] : nw;
                return node >= 0 && node < worldOf.Length && !IsIdentity(worldOf[node]);
            });
            if (nonIdentity) c.StaticNeedsBone++;

            if (nonIdentity && !renderNormalW && !(hasIdx && !hasWgt))
            {
                c.StaticBoneIgnored++;
                issues.Add("static mesh needs node transform, renderer applies none");
            }

            // The renderer's index-no-weight path: vertex BoneIndices[0] through the mesh list, LOCAL node transform
            if (hasIdx && !hasWgt)
            {
                c.IndexNoWeight++;
                var nestedWrong = mesh.Vertices.Take(64).Any(v =>
                {
                    var bi = v.BoneIndices[0];
                    if (bi < 0 || bi >= used) return false;
                    var node = mesh.BoneIndices[bi];
                    if (node < 0 || node >= worldOf.Length) return false;
                    return !NearlyEqual(flver.Nodes[node].ComputeLocalTransform(), worldOf[node]);
                });
                if (nestedWrong) { c.IndexNoWeightNested++; issues.Add("index-no-weight path uses local, not world, node transform"); }
            }

            // Node of the mesh itself (FLVER2 uses it as LocalTransform when there is no NormalW skinning)
            if (mesh.NodeIndex >= 0 && mesh.NodeIndex < worldOf.Length && !IsIdentity(worldOf[mesh.NodeIndex]))
                c.MeshNodeNonIdentity++;
        }

        // Binding through BoneIndices[0], as DES carries it (NormalW of the Color normal isn't a node index)
        {
            var used = mesh.UsedBoneCount;
            int localWrong = 0, needsWorld = 0;
            foreach (var v in mesh.Vertices)
            {
                var bi = v.BoneIndices[0];
                if (bi < 0 || bi >= used) continue;
                var node = mesh.BoneIndices[bi];
                if (node < 0 || node >= worldOf.Length) continue;
                if (!IsIdentity(worldOf[node])) needsWorld++;
                if (!NearlyEqual(flver.Nodes[node].ComputeLocalTransform(), worldOf[node])) localWrong++;
            }

            var tag = $"dyn{mesh.Dynamic} idx{(hasIdx ? 1 : 0)} wgt{(hasWgt ? 1 : 0)}";
            if (needsWorld > 0) stats.Inc($"bind node non-identity {tag}", cat);

            if (hasIdx && !hasWgt && mesh.Dynamic == 0 && localWrong > 0)
            {
                c.StaticNestedFull++;
                issues.Add("static rigid mesh placed by local node transform, needs world");
            }
            if (hasIdx && !hasWgt && mesh.Dynamic == 1 && needsWorld > 0)
            {
                c.BindPoseMoved++;
                issues.Add("bind-pose mesh without weights moved by the index-no-weight path");
            }
            if (hasWgt && mesh.Dynamic == 0 && needsWorld > 0)
            {
                c.StaticWeightedUnplaced++;
                issues.Add("static weighted mesh drawn without its node transform");
            }
        }

        // Index sanity after triangulation
        var tris = mesh.Triangulate(flver.Header.Version, false, false);
        if (tris.Any(i => i >= mesh.Vertices.Count && i != 0xFFFF)) { c.BadIndices++; issues.Add("index out of range"); }
        if (tris.Count == 0) { c.EmptyMeshes++; }
    }

    foreach (var issue in issues)
    {
        stats.Inc($"issue {issue}", cat);
        stats.Sample($"issue {issue}", $"{cat}: {name}");
    }
    if (issues.Count > 0) c.ModelsWithIssues++;
}

static bool IsIdentity(Matrix4x4 m) => NearlyEqual(m, Matrix4x4.Identity);

static bool NearlyEqual(Matrix4x4 a, Matrix4x4 b)
{
    for (var r = 0; r < 4; r++)
        for (var col = 0; col < 4; col++)
            if (MathF.Abs(a[r, col] - b[r, col]) > 1e-3f) return false;
    return true;
}

void AddTpf(HashSet<string> set, object src)
{
    try
    {
        TPF tpf = src switch
        {
            string p when File.Exists(p) => TPF.Read(p),
            byte[] b => TPF.Read(b),
            _ => null
        };
        if (tpf == null) return;
        foreach (var t in tpf.Textures) { set.Add(t.Name); allTex.Add(t.Name); }
    }
    catch { }
}

static HashSet<string> LoadMtdNames(string path)
{
    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (!File.Exists(path)) return set;
    foreach (var f in BND3.Read(path).Files)
        set.Add(Path.GetFileNameWithoutExtension(f.Name));
    return set;
}

class CatStats
{
    public int Models, ModelsWithIssues, Meshes, Static, DoubleSided, Strips, Over16Bit, MeshLayoutNot0;
    public int TexFound, TexMissing, TexInMapTpfOnly;
    public int SkinnedButNormalW, NormalWBeyondLocal, NormalWBeyondNodes, NormalWNeedsRemap, StaticNeedsBone, StaticBoneIgnored;
    public int IndexNoWeight, IndexNoWeightNested, MeshNodeNonIdentity, BadIndices, EmptyMeshes, StaticNestedFull, BindPoseMoved, StaticWeightedUnplaced;
}

class Stats
{
    readonly Dictionary<string, CatStats> cats = new();
    readonly Dictionary<string, Dictionary<string, int>> counts = new();
    readonly Dictionary<string, List<string>> samples = new();
    readonly List<string> failures = new();

    public CatStats Cat(string c) => cats.TryGetValue(c, out var s) ? s : cats[c] = new CatStats();

    public void Inc(string key, string cat = "all")
    {
        if (!counts.TryGetValue(key, out var d)) counts[key] = d = new();
        d[cat] = d.GetValueOrDefault(cat) + 1;
    }

    public void Sample(string key, string text)
    {
        if (!samples.TryGetValue(key, out var l)) samples[key] = l = new();
        if (l.Count < 40) l.Add(text);
    }

    public void Fail(string cat, string name, string msg)
    {
        Cat(cat);
        failures.Add($"{cat}: {name}: {msg}");
        Inc("read failure", cat);
    }

    public string Report()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# DES model survey\n");
        sb.AppendLine("| Category | Models | With issues | Meshes | Static | Double sided | Layout!=0 | Tex found | Tex missing | Tex only in map TPF |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (var (k, c) in cats)
            sb.AppendLine($"| {k} | {c.Models} | {c.ModelsWithIssues} | {c.Meshes} | {c.Static} | {c.DoubleSided} | {c.MeshLayoutNot0} | {c.TexFound} | {c.TexMissing} | {c.TexInMapTpfOnly} |");

        sb.AppendLine("\n| Category | Static rigid nested (all verts) | Bind pose moved | Static weighted unplaced |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var (k, c) in cats)
            sb.AppendLine($"| {k} | {c.StaticNestedFull} | {c.BindPoseMoved} | {c.StaticWeightedUnplaced} |");

        sb.AppendLine("\n| Category | Bind pose drawn by NormalW | NormalW >= used bones | NormalW >= nodes | NormalW needs remap | Static needs bone | Static bone ignored | Idx-no-weight | Idx-no-weight nested | Mesh node non-identity | Bad indices | Empty | >65535 verts |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var (k, c) in cats)
            sb.AppendLine($"| {k} | {c.SkinnedButNormalW} | {c.NormalWBeyondLocal} | {c.NormalWBeyondNodes} | {c.NormalWNeedsRemap} | {c.StaticNeedsBone} | {c.StaticBoneIgnored} | {c.IndexNoWeight} | {c.IndexNoWeightNested} | {c.MeshNodeNonIdentity} | {c.BadIndices} | {c.EmptyMeshes} | {c.Over16Bit} |");

        foreach (var prefix in new[] { "issue", "missing tex", "bind node", "read failure", "version", "dynamic", "normal", "layout", "texparam", "mtd" })
        {
            sb.AppendLine($"\n## {prefix}\n");
            foreach (var (k, d) in counts.Where(x => x.Key.StartsWith(prefix)).OrderByDescending(x => x.Value.Values.Sum()).Take(prefix == "mtd" ? 80 : 60))
                sb.AppendLine($"- `{k}`: " + string.Join(", ", d.Select(x => $"{x.Key} {x.Value}")));
        }

        sb.AppendLine("\n## Samples\n");
        foreach (var (k, l) in samples)
        {
            sb.AppendLine($"### {k}\n");
            foreach (var s in l) sb.AppendLine($"- {s}");
            sb.AppendLine();
        }

        sb.AppendLine("\n## Read failures\n");
        foreach (var f in failures.Take(80)) sb.AppendLine($"- {f}");

        return sb.ToString();
    }
}
