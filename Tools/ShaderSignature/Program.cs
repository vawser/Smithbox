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
