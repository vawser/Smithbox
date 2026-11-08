using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Text.RegularExpressions;

// ShaderSignature <ER|NR|AC6> <mode> ...
//   list <bundleRegex>                : the files inside the matching .shaderbdle bundles
//   extract <outDir> <bundleRegex>    : writes the files of the matching bundles
// Run with the working directory at src/Smithbox.Data, so the VFS dictionaries are found.

var gameName = args[0];
var mode = args[1];
var game = Enum.Parse<Game>(gameName);
var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var root = gamePaths[gameName];

List<VirtualFileSystem> layers = [];
try { layers.Add(ArchiveBinderVirtualFileSystem.FromGameFolder(root, game)); } catch { }
layers.Add(new RealVirtualFileSystem(root, false));
using var fs = new CompundVirtualFileSystem(layers);
var dict = File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{gameName}-File-Dictionary.json"));

var bundles = Bundles.Read(fs, dict);

if (mode == "list")
{
    var re = new Regex(args[2], RegexOptions.IgnoreCase);
    foreach (var b in bundles.Where(b => re.IsMatch(b.Name)).Take(5))
    {
        Console.WriteLine($"{b.Name}  ({b.Binder})");
        foreach (var f in b.Files) Console.WriteLine($"   {f.Name}  {f.Bytes.Length}  {Bundles.Magic(f.Bytes)}");
    }
    Console.WriteLine($"{bundles.Count} bundles");
}

if (mode == "extract")
{
    var re = new Regex(args[3], RegexOptions.IgnoreCase);
    foreach (var b in bundles.Where(b => re.IsMatch(b.Name)))
    {
        var dir = Path.Combine(args[2], Bundles.Safe(b.Name));
        Directory.CreateDirectory(dir);
        foreach (var f in b.Files) File.WriteAllBytes(Path.Combine(dir, Bundles.Safe(f.Name)), f.Bytes);
    }
}

// params <shader> : the materials of a shader, grouped by their bool and int params, to see what picks a permutation
// perms <cacheDir> <shader> : for each numbered G-buffer permutation, the material texture slots it samples
if (mode == "perms")
{
    var b = bundles.First(x => x.Name.Equals(args[3], StringComparison.OrdinalIgnoreCase));
    var re = new Regex($"^{Regex.Escape(b.Name)}_(\\d+)_Gbuf\\.ppo$", RegexOptions.IgnoreCase);
    var groups = new Dictionary<string, List<int>>();
    foreach (var f in b.Files)
    {
        var m = re.Match(Bundles.Safe(f.Name));
        if (!m.Success) continue;
        var one = new Bundle(b.Name, b.Binder, [new BundleFile(b.Name + "_Gbuf.ppo", f.Bytes)]);
        var dis = Signatures.Disassemble(one, Path.Combine(args[2], "perm" + m.Groups[1].Value));
        var d = Dxil.Parse(dis.Value.lines);
        var slots = string.Join(" ", d.Samples.Select(s => s.Texture).Where(t => t.Contains("snp")).Distinct()
            .Select(t => Regex.Match(t, @"Texture2D\w*?_(\d+)").Groups[1].Value).OrderBy(int.Parse));
        if (!groups.TryGetValue(slots, out var list)) groups[slots] = list = new();
        list.Add(int.Parse(m.Groups[1].Value));
    }
    foreach (var (slots, perms) in groups.OrderBy(g => g.Value.Min()))
        Console.WriteLine($"slots [{slots}]  permutations {string.Join(",", perms.OrderBy(x => x))}");
}

// layouts <vfsBinderPath> : each FLVER mesh's material and vertex layout, and the range of its float UV members
if (mode == "layouts")
{
    foreach (var f in BND4.Read(fs.ReadFile(args[2]).Value).Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
    {
        var flver = FLVER2.Read(f.Bytes);
        Console.WriteLine(Bundles.Safe(f.Name));
        foreach (var mesh in flver.Meshes.Take(6))
        {
            var mat = flver.Materials[mesh.MaterialIndex];
            Console.WriteLine($"  mesh: {Path.GetFileName(mat.MTD)}  {mesh.Vertices.Count} vertices");
            foreach (var bi in mesh.VertexBuffers)
                foreach (var m in flver.BufferLayouts[bi.LayoutIndex])
                    Console.WriteLine($"     {m.Semantic} {m.Index} {m.Type}");
            // The values of every UV member past the first two
            for (var uv = 2; uv < (mesh.Vertices.FirstOrDefault()?.UVs.Count ?? 0); uv++)
            {
                var xs = mesh.Vertices.Select(v => v.UVs[uv]).ToList();
                Console.WriteLine($"     UVs[{uv}] x {xs.Min(v => v.X):0.###}..{xs.Max(v => v.X):0.###}  y {xs.Min(v => v.Y):0.###}..{xs.Max(v => v.Y):0.###}  z {xs.Min(v => v.Z):0.###}..{xs.Max(v => v.Z):0.###}");
            }
        }
    }
}

// exprfile <dumpbin.txt> <output> [depth] : the same for any disassembly, such as a vertex shader's TEXCOORD8.w
if (mode == "exprfile")
{
    Console.WriteLine(Dxil.Parse(File.ReadAllLines(args[2])).Expression(args[3], args.Length > 4 ? int.Parse(args[4]) : 14));
}

// expr <cacheDir> <shader> <output> [permutation] [depth] : the expression an output is computed by
if (mode == "expr")
{
    var b = bundles.First(x => x.Name.Equals(args[3], StringComparison.OrdinalIgnoreCase));
    var all = Signatures.DisassembleAll(b, args[2]);
    var perm = args.Length > 5 ? int.Parse(args[5]) : all[0].permutation;
    var dis = all.First(x => x.permutation == perm);
    Console.WriteLine($"{b.Name} {dis.variant} perm {perm}: {args[4]} =");
    Console.WriteLine(Dxil.Parse(dis.lines).Expression(args[4], args.Length > 6 ? int.Parse(args[6]) : 14));
}

// metallic <cacheDir> <outJson> : shader to texture slot to the param added to its red before the metallic is
// saturated ("snp_0_float_0:0", the param and its component), "" for a slot saturated on its own. Over every permutation
if (mode == "metallic")
{
    var table = new SortedDictionary<string, SortedDictionary<int, string>>(StringComparer.Ordinal);
    var slotOf = new Regex(@"snp_\d+_Texture2D(?:Array)?_(\d+)$");
    var paramOf = new Regex(@"(snp_\d+_\w+?_\d+)_(\d+)$");
    int withOffset = 0;
    Parallel.ForEach(bundles, b =>
    {
        var slots = new SortedDictionary<int, string>();
        foreach (var (_, _, lines) in Signatures.DisassembleAll(b, args[2]))
        {
            foreach (var (tex, ch, field) in Dxil.Parse(lines).SaturatedInputs("RT1.x"))
            {
                var s = slotOf.Match(tex);
                if (!s.Success || ch != "r") continue;
                var slot = int.Parse(s.Groups[1].Value);
                var p = field == null ? null : paramOf.Match(field);
                var value = p is { Success: true } ? $"{p.Groups[1].Value}:{p.Groups[2].Value}" : "";
                // An offset found in any permutation wins over none
                if (!slots.TryGetValue(slot, out var cur) || cur == "") slots[slot] = value;
            }
        }
        if (slots.Count == 0) return;
        lock (table)
        {
            table[b.Name] = slots;
            if (slots.Values.Any(v => v != "")) withOffset++;
        }
    });
    File.WriteAllText(args[3], System.Text.Json.JsonSerializer.Serialize(table));
    Console.WriteLine($"{table.Count} shaders with a saturated metallic input, {withOffset} with an offset param, written to {args[3]}");
}

// blends <cacheDir> <outJson> [shaderRegex] : blend recipes from the shaders (Recipes.cs). Each is checked against the
// checked-in {GAME}_Blends.json where that has one; the ones it lacks are written to outJson, keyed as it is keyed
if (mode == "blends")
{
    var filter = new Regex(args.Length > 4 ? args[4] : ".");
    var folder = Path.Combine("Assets", "Material Definitions");
    var blendsPath = Path.Combine(folder, $"{gameName}_Blends.json");
    var checkedIn = File.Exists(blendsPath)
        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, StudioCore.Renderer.MaterialBlendRecipe>>(File.ReadAllText(blendsPath))
        : new();
    var generated = new SortedDictionary<string, StudioCore.Renderer.MaterialBlendRecipe>(StringComparer.Ordinal);
    var compared = new List<(string key, List<string> diffs)>();
    var problems = new Dictionary<string, int>();
    var vsCache = Path.Combine(args[2], "_vs");

    Parallel.ForEach(bundles.Where(b => filter.IsMatch(b.Name)), b =>
    {
        // The vertex shader the G-buffer pass draws with, for where the weights' vertex values come from
        Dxil vs = null;
        var vsFile = b.Files.FirstOrDefault(f => Bundles.Safe(f.Name).Equals(b.Name + "_Gbuf.vpo", StringComparison.OrdinalIgnoreCase))
            ?? b.Files.FirstOrDefault(f => Bundles.Safe(f.Name).StartsWith(b.Name + "_Gbuf", StringComparison.OrdinalIgnoreCase) && f.Name.EndsWith(".vpo"))
            ?? b.Files.FirstOrDefault(f => Bundles.Safe(f.Name).Equals(b.Name + "_Fwd.vpo", StringComparison.OrdinalIgnoreCase));
        if (vsFile != null)
        {
            var dis = Signatures.Disassemble(new Bundle(b.Name, b.Binder, [new BundleFile(Bundles.Safe(vsFile.Name).Replace(".vpo", ".ppo"), vsFile.Bytes)]), vsCache);
            if (dis != null) vs = Dxil.Parse(dis.Value.lines);
        }
        string VertexSource(string psInput)
        {
            if (vs == null) return null;
            var e = vs.Expression(psInput[3..], 2);
            return e.StartsWith("in:") && !e.Contains(' ') ? e[3..] : null;
        }

        foreach (var (variant, perm, lines) in Signatures.DisassembleAll(b, args[2]))
        {
            if (!variant.Contains("Gbuf")) continue;
            var d = Dxil.Parse(lines);
            var sig = Signatures.Build(b.Name, variant, d, new());
            var albedos = sig.Textures.Where(t => t.Slot >= 0 && Signatures.Meaning(t) == "Albedo").Select(t => t.Slot).ToHashSet();
            var normals = sig.Textures.Where(t => t.Slot >= 0 && Signatures.Meaning(t) is "Normal" or "NormalGloss").Select(t => t.Slot).ToHashSet();
            if (albedos.Count < 2) continue;

            var key = perm >= 0 ? $"{b.Name}@{perm}" : b.Name;
            var r = Recipes.Build(d, albedos, normals, VertexSource);
            lock (generated)
            {
                foreach (var p in r.Problems) problems[Regex.Replace(p, @"\d+", "N")] = problems.GetValueOrDefault(Regex.Replace(p, @"\d+", "N")) + 1;
                if (checkedIn.TryGetValue(key, out var theirs)) compared.Add((key, r.Ok ? Recipes.Compare(r.Recipe, theirs) : r.Problems.Select(p => "problem: " + p).ToList()));
                // Only what the viewport can draw (MaterialDefinition.ResolveBlendRecipe): masks, COLOR1.x, and up to two
                // TEXCOORDs, the first (TEXCOORD1 if it is one) xyz and the second x only
                else if (r.Ok && r.Recipe.Layers.Count > 0 && Drawable(r.Recipe))
                    generated[key] = r.Recipe;
            }
        }
    });

    var matching = compared.Count(c => c.diffs.Count == 0);
    Console.WriteLine($"Checked-in recipes reproduced: {matching} of {compared.Count}");
    foreach (var (diff, n) in compared.SelectMany(c => c.diffs.Select(x => Regex.Replace(x, ":.*", ""))).GroupBy(x => x).Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2).Take(15))
        Console.WriteLine($"  {n,5}  {diff}");
    foreach (var (key, diffs) in compared.Where(c => c.diffs.Count > 0).Take(args.Length > 5 ? int.Parse(args[5]) : 8))
        Console.WriteLine($"  {key}: {string.Join("; ", diffs.Take(6))}");
    Console.WriteLine("Problems:");
    foreach (var (p, n) in problems.OrderByDescending(x => x.Value).Take(12)) Console.WriteLine($"  {n,5}  {p}");
    File.WriteAllText(args[3], System.Text.Json.JsonSerializer.Serialize(generated, new System.Text.Json.JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
    Console.WriteLine($"{generated.Count} new recipes written to {args[3]}");
}

// overlays <cacheDir> <outJson> : the overlays the checked-in _Details.json can't express, found in the expressions:
// the albedo overlay of [Ov_AN] shaders, x = A + s(g - 0.5)(O - A), result = sat(x + s(0.5 - g)(O - x)), g = O^0.454,
// and the normal overlay N + s(O * 2 - 1). Written as _Details.json entries, keyed shader@permutation as that is
if (mode == "overlays")
{
    var albedoOverlay = new Regex(@"cb:(\S+?) \* \(0\.5 - exp2\(\(log2\((\S+?)\.r\) \* 0\.454\)\)\)");
    var normalOverlay = new Regex(@"\(cb:(\S+?) \* \(\((\S+?)\.g \* 2\) \+ -1\)\)");
    var bareNormal = new Regex(@"(?<!cb:\S+ \* )\(\((\S+?)\.g \* 2\) \+ -1\)");
    var slotOf = new Regex(@"snp_\d+_Texture2D_(\d+)$");
    static string Short(string field) { var i = field.IndexOf("snp_", StringComparison.Ordinal); return i >= 0 ? field[i..] : field; }
    var table = new SortedDictionary<string, StudioCore.Renderer.MaterialDetailRecipe>(StringComparer.Ordinal);
    Parallel.ForEach(bundles, b =>
    {
        foreach (var (_, perm, lines) in Signatures.DisassembleAll(b, args[2]))
        {
            var d = Dxil.Parse(lines);
            var albedo = albedoOverlay.Match(d.Expression("RT2.x", 30));
            var normalExpression = d.Expression("RT0.x", 30);
            var normal = normalOverlay.Match(normalExpression);
            // Only an overlay on top of a base normal that is used as it is: a main normal scaled by a param would
            // match too, and taking it for an overlay would leave the material without one
            var bare = bareNormal.Matches(normalExpression).Select(m => m.Groups[1].Value).ToHashSet();
            if (normal.Success && (bare.Count == 0 || bare.Contains(normal.Groups[2].Value))) normal = Match.Empty;
            var recipe = new StudioCore.Renderer.MaterialDetailRecipe();
            // Only strengths the material sets: engine fields such as FC_RimLightingParam.w have no value to read here
            if (albedo.Success && Short(albedo.Groups[1].Value).StartsWith("snp_") && slotOf.Match(albedo.Groups[2].Value) is { Success: true } a)
            {
                recipe.Albedo = int.Parse(a.Groups[1].Value);
                recipe.AlbedoBlend = "Overlay";
                recipe.AlbedoStrength = Short(albedo.Groups[1].Value);
            }
            if (normal.Success && Short(normal.Groups[1].Value).StartsWith("snp_") && slotOf.Match(normal.Groups[2].Value) is { Success: true } n)
            {
                recipe.Normal = int.Parse(n.Groups[1].Value);
                recipe.NormalStrength = Short(normal.Groups[1].Value);
            }
            if (recipe.Albedo == null && recipe.Normal == null) continue;
            lock (table) table[perm >= 0 ? $"{b.Name}@{perm}" : b.Name] = recipe;
        }
    });
    File.WriteAllText(args[3], System.Text.Json.JsonSerializer.Serialize(table, new System.Text.Json.JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
    Console.WriteLine($"{table.Count} overlays ({table.Values.Count(r => r.AlbedoBlend != null)} with an albedo overlay), written to {args[3]}");
}

// uvsets <slotMeanings.json> <outJson> : shader to texture slot to the UV set it samples with ("0" or "1"), for the
// slots sampled with one set only, over every permutation
if (mode == "uvsets")
{
    var meanings = System.Text.Json.JsonSerializer.Deserialize<SortedDictionary<string, SlotMeanings>>(File.ReadAllText(args[2]));
    var table = new SortedDictionary<string, SortedDictionary<int, string>>(StringComparer.Ordinal);
    int secondSet = 0;
    foreach (var (shader, m) in meanings)
    {
        var slots = new SortedDictionary<int, string>();
        foreach (var (slot, meaning) in m.Slots.Where(x => x.Value.UvSet >= 0)) slots[slot] = meaning.UvSet.ToString();
        if (slots.Count == 0) continue;
        table[shader] = slots;
        if (slots.Values.Contains("1")) secondSet++;
    }
    File.WriteAllText(args[3], System.Text.Json.JsonSerializer.Serialize(table));
    Console.WriteLine($"{table.Count} shaders, {secondSet} with a slot on the second UV set, written to {args[3]}");
}

// layermetal <signatures.json> : for each blend recipe, which metallic map goes with which layer, by suffix and by UV group
if (mode == "layermetal")
{
    var all = System.Text.Json.JsonSerializer.Deserialize<List<ShaderSignature>>(File.ReadAllText(args[2]))
        .ToDictionary(s => s.Shader, StringComparer.OrdinalIgnoreCase);
    var folder = Path.Combine("Assets", "Material Definitions");
    var blends = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, StudioCore.Renderer.MaterialBlendRecipe>>(File.ReadAllText(Path.Combine(folder, $"{gameName}_Blends.json")));
    var groups = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, string>>>(File.ReadAllText(Path.Combine(folder, $"{gameName}_TextureGroups.json")));
    int layersTotal = 0, bySuffix = 0, byGroup = 0, agree = 0, none = 0, recipesWithMetal = 0;
    static int SuffixOf(string role) { var m = Regex.Match(role, @"_(\d+)$"); return m.Success ? int.Parse(m.Groups[1].Value) : 0; }
    foreach (var (key, r) in blends)
    {
        var shader = key.Split('@')[0];
        if (!all.TryGetValue(shader, out var sig)) continue;
        var perm = key.Contains('@') ? int.Parse(key.Split('@')[1]) : -1;
        var slots = sig.Permutations.FirstOrDefault(p => p.Number == perm)?.Slots ?? sig.Textures.Select(t => t.Slot).ToList();
        var tex = sig.Textures.Where(t => t.Slot >= 0 && slots.Contains(t.Slot)).GroupBy(t => t.Slot).ToDictionary(x => x.Key, x => x.First());
        var metals = tex.Values.Where(t => t.Role.StartsWith("MetallicMap")).ToList();
        if (metals.Count == 0) continue;
        recipesWithMetal++;
        groups.TryGetValue(shader, out var g);
        var albedos = new[] { r.Albedo }.Concat(r.Layers.Select(l => l.Albedo)).ToList();
        var line = new List<string>();
        foreach (var a in albedos)
        {
            layersTotal++;
            var aRole = tex.TryGetValue(a, out var at) ? at.Role : "?";
            var s = metals.Where(m => SuffixOf(m.Role) == SuffixOf(aRole)).Select(m => m.Slot).ToList();
            var gr = g != null && g.TryGetValue(a, out var ag) ? metals.Where(m => g.TryGetValue(m.Slot, out var mg) && mg == ag).Select(m => m.Slot).ToList() : [];
            if (s.Count == 1) bySuffix++;
            if (gr.Count == 1) byGroup++;
            if (s.Count == 1 && gr.Count == 1 && s[0] == gr[0]) agree++;
            if (s.Count == 0 && gr.Count == 0) none++;
            line.Add($"A{a}({aRole}) suffix[{string.Join(",", s)}] group[{string.Join(",", gr)}]");
        }
        if (args.Length > 3) Console.WriteLine($"{key}: metals {string.Join(",", metals.Select(m => $"{m.Slot}:{m.Role}"))} | {string.Join("  ", line)}");
    }
    Console.WriteLine($"{recipesWithMetal} recipes with metallic maps, {layersTotal} layers: unique by suffix {bySuffix}, unique by UV group {byGroup}, both and agree {agree}, neither {none}");
}

// checkrecipes <signatures.json> : whether every slot the checked-in recipe tables name is one the shader samples
if (mode == "checkrecipes")
{
    var all = System.Text.Json.JsonSerializer.Deserialize<List<ShaderSignature>>(File.ReadAllText(args[2]));
    var sigs = all.ToDictionary(s => s.Shader, s => s.Textures.Where(t => t.Slot >= 0).Select(t => t.Slot).ToHashSet(), StringComparer.OrdinalIgnoreCase);
    // shader@N is the permutation GXFT_SATStaticSwitch = N picks
    foreach (var s in all)
        foreach (var p in s.Permutations)
            sigs[$"{s.Shader}@{p.Number}"] = p.Slots.ToHashSet();
    var folder = Path.Combine("Assets", "Material Definitions");
    int ok = 0, bad = 0, missing = 0;
    void Check(string table, string shader, IEnumerable<int?> slots)
    {
        if (!sigs.TryGetValue(shader, out var have)) { missing++; Console.WriteLine($"{table} {shader}: no such shader"); return; }
        var absent = slots.Where(s => s != null && !have.Contains(s.Value)).Select(s => s.Value).Distinct().ToList();
        if (absent.Count == 0) { ok++; return; }
        bad++;
        Console.WriteLine($"{table} {shader}: slots {string.Join(",", absent)} not sampled (shader samples {string.Join(",", have.OrderBy(x => x))})");
    }
    T Load<T>(string suffix) where T : new()
    {
        var p = Path.Combine(folder, $"{gameName}{suffix}.json");
        return File.Exists(p) ? System.Text.Json.JsonSerializer.Deserialize<T>(File.ReadAllText(p)) : new T();
    }
    foreach (var (s, r) in Load<Dictionary<string, StudioCore.Renderer.MaterialBlendRecipe>>("_Blends"))
        Check("Blends", s, new int?[] { r.Albedo, r.Normal, r.Mask, r.DetailMask }.Concat(r.Layers.SelectMany(l => new int?[] { l.Albedo, l.Normal })));
    foreach (var (s, r) in Load<Dictionary<string, StudioCore.Renderer.MaterialDetailRecipe>>("_Details")) Check("Details", s, [r.Albedo, r.Normal]);
    foreach (var (s, r) in Load<Dictionary<string, StudioCore.Renderer.MaterialMagmaRecipe>>("_Magma")) Check("Magma", s, [r.Texture]);
    foreach (var (s, r) in Load<Dictionary<string, StudioCore.Renderer.MaterialWaterRecipe>>("_Water")) Check("Water", s, r.Layers.Select(l => (int?)l.Slot));
    foreach (var (s, r) in Load<Dictionary<string, StudioCore.Renderer.MaterialFurRecipe>>("_Fur")) Check("Fur", s, r.FirstUvSlots.Concat(r.ThirdUvSlots).Append(r.Cut).Select(x => (int?)x));
    foreach (var (s, r) in Load<Dictionary<string, Dictionary<int, StudioCore.Renderer.MaterialTextureInput>>>("_TextureInputs")) Check("TextureInputs", s, r.Keys.Select(k => (int?)k));
    Console.WriteLine($"{ok} consistent, {bad} name slots the shader doesn't sample, {missing} shaders not found");
}

if (mode == "params")
{
    var combos = new Dictionary<string, (int n, string example, string samplers)>();
    foreach (var path in Regex.Matches(dict, @"""(/material/[^""]+\.matbinbnd\.dcx)""").Select(m => m.Groups[1].Value).Distinct())
    {
        var file = fs.ReadFile(path);
        if (file == null) continue;
        foreach (var f in BND4.Read(file.Value).Files.Where(f => f.Name.EndsWith(".matbin")))
        {
            var mb = MATBIN.Read(f.Bytes);
            if (!Path.GetFileNameWithoutExtension(mb.ShaderPath).Equals(args[2], StringComparison.OrdinalIgnoreCase)) continue;
            var key = string.Join(" ", mb.Params.Where(p => (p.Name.Contains("snp_") || p.Name.Contains("StaticSwitch")) && p.Value is bool or int or int[] or float).OrderBy(p => p.Name)
                .Select(p => $"{Regex.Replace(p.Name, "^.*snp_", "")}={(p.Value is int[] a ? string.Join(",", a) : p.Value)}"));
            var samplers = string.Join(" ", mb.Samplers.Where(s => s.Path != "").Select(s => Regex.Replace(s.Type, "^.*Texture2D_", "")).OrderBy(x => x));
            var cur = combos.GetValueOrDefault(key);
            combos[key] = (cur.n + 1, cur.example ?? Path.GetFileName(f.Name), cur.samplers ?? samplers);
        }
    }
    Console.WriteLine($"{combos.Count} distinct bool/int combinations");
    foreach (var (k, v) in combos.OrderByDescending(x => x.Value.n).Take(25)) Console.WriteLine($"{v.n,5}  {k}   | {v.samplers}   e.g. {v.example}");
}

// bnd <vfsPath> <outDir> [nameRegex] : the files of a plain shader binder such as /shader/gxrenderershader.shaderbnd.dcx
if (mode == "bnd")
{
    var re = new Regex(args.Length > 4 ? args[4] : ".", RegexOptions.IgnoreCase);
    Directory.CreateDirectory(args[3]);
    var n = 0;
    foreach (var f in BND4.Read(fs.ReadFile(args[2]).Value).Files.Where(f => re.IsMatch(f.Name)))
    {
        File.WriteAllBytes(Path.Combine(args[3], Bundles.Safe(f.Name)), f.Bytes.ToArray());
        n++;
    }
    Console.WriteLine($"{n} files");
}

// one <cacheDir> <shader> : the signature of one shader, printed
// run <cacheDir> <outDir> [censusDir] [shaderRegex] : every shader's signature, and the channel map over the game
if (mode is "one" or "run")
{
    var cacheDir = args[2];
    var roles = Signatures.ReadRoles(fs, dict);
    var filter = mode == "one" ? new Regex($"^{Regex.Escape(args[3])}$") : new Regex(args.Length > 5 ? args[5] : ".");
    var sigs = new List<ShaderSignature>();
    var failures = new List<string>();
    var targets = bundles.Where(b => filter.IsMatch(b.Name)).ToList();
    Console.WriteLine($"{targets.Count} shaders");
    var done = 0;
    Parallel.ForEach(targets, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, b =>
    {
        try
        {
            var all = Signatures.DisassembleAll(b, cacheDir);
            if (all.Count == 0) { lock (failures) failures.Add($"{b.Name}: no pixel shader"); return; }
            var slotRoles = roles.GetValueOrDefault(b.Name) ?? new();
            var sig = Signatures.Merge(all.Select(x => (x.permutation, Signatures.Build(b.Name, x.variant, Dxil.Parse(x.lines), slotRoles))).ToList());
            lock (sigs) sigs.Add(sig);
        }
        catch (Exception e) { lock (failures) failures.Add($"{b.Name}: {e.Message}"); }
        var n = Interlocked.Increment(ref done);
        if (n % 200 == 0) Console.WriteLine($"  {n}");
    });

    if (mode == "one")
    {
        foreach (var s in sigs)
        {
            Console.WriteLine($"{s.Shader} ({s.Variant}), discard {s.Discard} {string.Join(" ", s.DiscardDeps)}");
            foreach (var t in s.Textures)
            {
                Console.WriteLine($"  slot {t.Slot} {t.Role} [{t.Resource}]  uv: {string.Join(" ", t.Coordinates)}");
                foreach (var (ch, outs) in t.Channels) Console.WriteLine($"     .{ch} -> {string.Join(" ", outs)}");
            }
            foreach (var (o, deps) in s.Outputs) Console.WriteLine($"  {o} <- {string.Join(" ", deps)}");
        }
        foreach (var f in failures) Console.WriteLine(f);
    }
    else
    {
        var outDir = args[3];
        Directory.CreateDirectory(outDir);
        var weights = Signatures.ReadWeights(args.Length > 4 ? args[4] : Path.Combine(outDir, "..", "Census"), gameName);
        Signatures.Write(outDir, gameName, sigs, weights, failures);
        Console.WriteLine($"{sigs.Count} signatures, {failures.Count} failures, written to {outDir}");
    }
}

static bool Drawable(StudioCore.Renderer.MaterialBlendRecipe recipe)
{
    if (!recipe.Layers.All(l => l.Source is "Mask.r" or "Mask.g" or "Mask.b" or "COLOR1.x" || Regex.IsMatch(l.Source, @"^TEXCOORD\d+\.[xyz]$")))
        return false;
    var attributes = recipe.Layers.Select(l => Regex.Match(l.Source, @"^TEXCOORD(\d+)").Groups[1].Value).Where(x => x != "")
        .Select(int.Parse).Distinct().OrderBy(x => x == 1 ? 0 : 1).ThenBy(x => x).ToList();
    return attributes.Count <= 1
        || (attributes.Count == 2 && recipe.Layers.Where(l => l.Source.StartsWith($"TEXCOORD{attributes[1]}.")).All(l => l.Source.EndsWith(".x")));
}

record BundleFile(string Name, byte[] Bytes);
record Bundle(string Name, string Binder, List<BundleFile> Files);

static class Bundles
{
    public static string Safe(string name) => Regex.Replace(name.Replace((char)92, '/').Split('/')[^1], @"[<>:""|?*]", "_");

    public static string Magic(byte[] b) => b.Length >= 4 ? new string(b.Take(4).Select(c => c is >= 32 and < 127 ? (char)c : '.').ToArray()) : "";

    /// <summary>Every .shaderbdle of the game, the non ray tracing binders, devpatch ones last so they win</summary>
    public static List<Bundle> Read(VirtualFileSystem fs, string dict)
    {
        var binders = Regex.Matches(dict, @"""(/shader(?:_d3d12)?/[^""]+\.shaderbdlebnd(?:\.devpatch)?\.dcx)""")
            .Select(m => m.Groups[1].Value).Distinct().Where(p => !p.Contains("[rt]"))
            .OrderBy(p => p.Contains("devpatch") ? 1 : 0).ThenBy(p => p, StringComparer.Ordinal).ToList();
        var result = new Dictionary<string, Bundle>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in binders)
        {
            var file = fs.ReadFile(path);
            if (file == null) continue;
            foreach (var entry in BND4.Read(file.Value).Files.Where(f => f.Name.EndsWith(".shaderbdle")))
            {
                var name = Path.GetFileNameWithoutExtension(Safe(entry.Name));
                var files = BND4.Read(entry.Bytes).Files.Select(f => new BundleFile(f.Name, f.Bytes.ToArray())).ToList();
                result[name] = new Bundle(name, Path.GetFileName(path), files);
            }
        }
        return result.Values.ToList();
    }
}
