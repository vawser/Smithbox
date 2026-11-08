using SoulsFormats;
using StudioCore.Renderer;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// One shader's signature: for each of its textures, which channels go to which G-buffer components.
/// </summary>
class ShaderSignature
{
    public string Shader { get; set; } = "";
    public string Variant { get; set; } = "";
    public bool Discard { get; set; }
    public List<string> DiscardDeps { get; set; } = new();
    public List<TextureUse> Textures { get; set; } = new();
    /// <summary>Output component to everything it depends on</summary>
    public SortedDictionary<string, List<string>> Outputs { get; set; } = new(StringComparer.Ordinal);
    /// <summary>The numbered permutations and the material texture slots each samples; empty for a single shader</summary>
    public List<Permutation> Permutations { get; set; } = new();
}

class Permutation
{
    public int Number { get; set; }
    public List<int> Slots { get; set; } = new();
}

class TextureUse
{
    public string Resource { get; set; } = "";
    public int Slot { get; set; } = -1;
    public string Role { get; set; } = "";
    /// <summary>Channel to the output components it reaches</summary>
    public SortedDictionary<string, List<string>> Channels { get; set; } = new(StringComparer.Ordinal);
    /// <summary>What its coordinates depend on, over every sample of it</summary>
    public List<string> Coordinates { get; set; } = new();
}

static class Signatures
{
    const string Dxc = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\dxc.exe";

    // The deferred G-buffer pass first; shaders that only draw forward (transparent, effects) fall back to it
    static readonly string[] VariantOrder = ["_Gbuf.ppo", "_Gbuf_[A].ppo", "_Fwd.ppo", "_Fwd_[A].ppo"];
    static readonly Regex SlotOf = new(@"Texture2D(?:Array)?_(\d+)", RegexOptions.Compiled);

    /// <summary>Picks the variant, disassembles it once into the cache, and returns its text</summary>
    public static (string variant, string[] lines)? Disassemble(Bundle bundle, string cacheDir)
    {
        BundleFile file = null;
        string variant = null;
        foreach (var suffix in VariantOrder)
        {
            file = bundle.Files.FirstOrDefault(f => Bundles.Safe(f.Name).Equals(bundle.Name + suffix, StringComparison.OrdinalIgnoreCase));
            if (file != null) { variant = suffix[1..^4]; break; }

            // Numbered permutations (ER characters from 0, AC6 from 1): the lowest stands for them
            var permutation = new Regex($"^{Regex.Escape(bundle.Name)}_(\\d+){Regex.Escape(suffix)}$", RegexOptions.IgnoreCase);
            var numbered = bundle.Files.Select(f => (f, m: permutation.Match(Bundles.Safe(f.Name)))).Where(x => x.m.Success)
                .OrderBy(x => int.Parse(x.m.Groups[1].Value)).FirstOrDefault();
            if (numbered.f != null) { file = numbered.f; variant = $"{numbered.m.Groups[1].Value}{suffix[..^4]}"; break; }
        }
        if (file == null)
        {
            file = bundle.Files.FirstOrDefault(f => f.Name.EndsWith(".ppo"));
            if (file == null) return null;
            variant = Path.GetFileNameWithoutExtension(Bundles.Safe(file.Name))[bundle.Name.Length..].TrimStart('_');
        }

        var dir = Path.Combine(cacheDir, Bundles.Safe(bundle.Name));
        Directory.CreateDirectory(dir);
        var bin = Path.Combine(dir, Bundles.Safe(file.Name));
        var txt = bin + ".txt";
        if (!File.Exists(txt))
        {
            File.WriteAllBytes(bin, file.Bytes);
            var psi = new ProcessStartInfo(Dxc, $"-dumpbin \"{bin}\"") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            using var p = Process.Start(psi)!;
            var output = p.StandardOutput.ReadToEndAsync();
            var error = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            File.WriteAllText(txt, output.Result + error.Result);
        }
        return (variant, File.ReadAllLines(txt));
    }

    /// <summary>
    /// Every permutation of the first variant the bundle has: the one plain file, or all the numbered ones.
    /// Permutations add texture slots (a feature each), so the shader's signature is their union.
    /// </summary>
    public static List<(string variant, int permutation, string[] lines)> DisassembleAll(Bundle bundle, string cacheDir)
    {
        foreach (var suffix in VariantOrder)
        {
            var plain = bundle.Files.FirstOrDefault(f => Bundles.Safe(f.Name).Equals(bundle.Name + suffix, StringComparison.OrdinalIgnoreCase));
            if (plain != null)
            {
                var one = Disassemble(new Bundle(bundle.Name, bundle.Binder, [plain]), cacheDir);
                return [(suffix[1..^4], -1, one!.Value.lines)];
            }

            var permutation = new Regex($"^{Regex.Escape(bundle.Name)}_(\\d+){Regex.Escape(suffix)}$", RegexOptions.IgnoreCase);
            var numbered = bundle.Files.Select(f => (f, m: permutation.Match(Bundles.Safe(f.Name)))).Where(x => x.m.Success).ToList();
            if (numbered.Count == 0) continue;
            return numbered.Select(x =>
            {
                var single = new Bundle(bundle.Name, bundle.Binder, [new BundleFile(bundle.Name + suffix, x.f.Bytes)]);
                var dis = Disassemble(single, Path.Combine(cacheDir, "perm" + x.m.Groups[1].Value));
                return (suffix[1..^4], int.Parse(x.m.Groups[1].Value), dis!.Value.lines);
            }).OrderBy(x => x.Item2).ToList();
        }

        var any = Disassemble(bundle, cacheDir);
        return any == null ? [] : [(any.Value.variant, -1, any.Value.lines)];
    }

    /// <summary>The union of the permutations' signatures, with each permutation's texture slots kept</summary>
    public static ShaderSignature Merge(List<(int permutation, ShaderSignature sig)> parts)
    {
        var first = parts[0].sig;
        var merged = new ShaderSignature { Shader = first.Shader, Variant = parts.Count > 1 ? $"{first.Variant} x{parts.Count}" : first.Variant };
        foreach (var (perm, s) in parts)
        {
            merged.Discard |= s.Discard;
            merged.DiscardDeps = merged.DiscardDeps.Union(s.DiscardDeps).OrderBy(x => x, StringComparer.Ordinal).ToList();
            foreach (var (o, deps) in s.Outputs)
                merged.Outputs[o] = merged.Outputs.TryGetValue(o, out var cur) ? cur.Union(deps).OrderBy(x => x, StringComparer.Ordinal).ToList() : deps;
            foreach (var t in s.Textures)
            {
                var m = merged.Textures.FirstOrDefault(x => x.Resource == t.Resource);
                if (m == null) { merged.Textures.Add(m = new TextureUse { Resource = t.Resource, Slot = t.Slot, Role = t.Role }); }
                foreach (var (ch, outs) in t.Channels)
                    m.Channels[ch] = m.Channels.TryGetValue(ch, out var cur) ? cur.Union(outs).ToList() : outs.ToList();
                m.Coordinates = m.Coordinates.Union(t.Coordinates).OrderBy(x => x, StringComparer.Ordinal).ToList();
            }
            if (perm >= 0)
                merged.Permutations.Add(new Permutation { Number = perm, Slots = s.Textures.Where(t => t.Slot >= 0).Select(t => t.Slot).Distinct().OrderBy(x => x).ToList() });
        }
        return merged;
    }

    public static ShaderSignature Build(string shader, string variant, Dxil d, Dictionary<int, string> roles)
    {
        var sig = new ShaderSignature { Shader = shader, Variant = variant, Discard = d.HasDiscard, DiscardDeps = d.DiscardDeps.ToList() };
        foreach (var (output, deps) in d.OutputDeps) sig.Outputs[output] = deps.ToList();

        foreach (var res in d.Resources.Where(r => r.Type == "texture"))
        {
            var use = new TextureUse { Resource = res.Name };
            var m = SlotOf.Match(res.Name);
            if (m.Success && res.Name.Contains("snp", StringComparison.Ordinal))
            {
                use.Slot = int.Parse(m.Groups[1].Value);
                use.Role = roles.GetValueOrDefault(use.Slot, "");
            }
            else use.Role = "(engine)";

            void AddLeaves(IEnumerable<string> deps, string output)
            {
                foreach (var leaf in deps.Where(l => l.StartsWith(res.Name + ".", StringComparison.Ordinal)))
                {
                    var ch = leaf[(res.Name.Length + 1)..];
                    if (!use.Channels.TryGetValue(ch, out var list)) use.Channels[ch] = list = new();
                    if (!list.Contains(output)) list.Add(output);
                }
            }

            foreach (var (output, deps) in d.OutputDeps) AddLeaves(deps, output);
            AddLeaves(d.DiscardDeps, "discard");
            // A texture whose channels move the coordinates of another (distortion, parallax, flow)
            foreach (var s in d.Samples) AddLeaves(s.Coordinates, $"uv({s.Texture})");

            use.Coordinates = d.Samples.Where(s => s.Texture == res.Name).SelectMany(s => s.Coordinates).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (use.Channels.Count > 0 || d.Samples.Any(s => s.Texture == res.Name)) sig.Textures.Add(use);
        }
        return sig;
    }

    /// <summary>Shader name to texture slot to role, from the materials of the shader that name the slot</summary>
    public static Dictionary<string, Dictionary<int, string>> ReadRoles(Andre.IO.VFS.VirtualFileSystem fs, string dict)
    {
        var result = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
        var binders = Regex.Matches(dict, @"""(/material/[^""]+\.matbinbnd(?:\.devpatch)?\.dcx)""").Select(m => m.Groups[1].Value).Distinct();
        foreach (var path in binders)
        {
            var file = fs.ReadFile(path);
            if (file == null) continue;
            foreach (var f in BND4.Read(file.Value).Files.Where(f => f.Name.EndsWith(".matbin", StringComparison.OrdinalIgnoreCase)))
            {
                MaterialDefinition def;
                try { def = MaterialDefinition.FromMatbin(f.Name, MATBIN.Read(f.Bytes), null); } catch { continue; }
                if (!result.TryGetValue(def.ShaderName, out var slots)) result[def.ShaderName] = slots = new();
                foreach (var s in def.Samplers.Where(s => s.Slot >= 0))
                    slots.TryAdd(s.Slot, s.Suffix >= 0 ? $"{s.Role}_{s.Suffix}" : s.Role);
            }
        }
        return result;
    }

    /// <summary>Shader to placed triangles, from the Phase 1 census</summary>
    public static Dictionary<string, long> ReadWeights(string censusDir, string game)
    {
        var path = Path.Combine(censusDir, $"{game}_Shaders.tsv");
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return result;
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            var c = line.Split('\t');
            result[c[0]] = long.Parse(c[3]);
        }
        return result;
    }

    /// <summary>MetallicMap_0 and the like become MetallicMap, so a channel map groups layers together</summary>
    static string BaseRole(string role) => Regex.Replace(role, @"_\d+$", "");

    public static void Write(string outDir, string game, List<ShaderSignature> sigs, Dictionary<string, long> weights, List<string> failures)
    {
        File.WriteAllText(Path.Combine(outDir, $"{game}_Signatures.json"),
            JsonSerializer.Serialize(sigs.OrderBy(s => s.Shader, StringComparer.Ordinal).ToList(), new JsonSerializerOptions { WriteIndented = true }));

        long total = weights.Values.Sum();
        string Share(long v) => total == 0 ? "0" : (100.0 * v / total).ToString("F3", CultureInfo.InvariantCulture);

        // Role.channel -> the set of outputs it goes to, totalled over the shaders used in the game
        var map = new Dictionary<(string role, string ch, string outs), (int shaders, long placed, string example)>();
        foreach (var s in sigs.Where(s => weights.ContainsKey(s.Shader)))
        {
            var w = weights[s.Shader];
            var counted = new HashSet<(string, string, string)>();
            foreach (var t in s.Textures)
            {
                var role = t.Role == "" ? $"slot{t.Slot}" : t.Role == "(engine)" ? $"(engine) {t.Resource}" : BaseRole(t.Role);
                var channels = t.Channels.Count == 0 ? new SortedDictionary<string, List<string>> { ["-"] = new() { "(unused)" } } : t.Channels;
                foreach (var (ch, outs) in channels)
                {
                    var outputs = string.Join(" ", outs.Select(o => o.StartsWith("uv(") ? "uv" : o).Distinct().OrderBy(x => x, StringComparer.Ordinal));
                    var key = (role, ch, outputs);
                    // Once per shader, whatever number of layers share the role
                    if (!counted.Add(key)) continue;
                    var cur = map.GetValueOrDefault(key);
                    map[key] = (cur.shaders + 1, cur.placed + w, cur.example == null || w > weights.GetValueOrDefault(cur.example) ? s.Shader : cur.example);
                }
            }
        }

        var sb = new StringBuilder("Role\tChannel\tOutputs\tShaders\tPlacedTriangles\tPlacedShare%\tExampleShader\n");
        foreach (var ((role, ch, outs), v) in map.OrderBy(x => x.Key.role, StringComparer.Ordinal).ThenBy(x => x.Key.ch, StringComparer.Ordinal).ThenByDescending(x => x.Value.placed))
            sb.Append($"{role}\t{ch}\t{outs}\t{v.shaders}\t{v.placed}\t{Share(v.placed)}\t{v.example}\n");
        File.WriteAllText(Path.Combine(outDir, $"{game}_ChannelMap.tsv"), sb.ToString());

        sb = new StringBuilder("Shader\tVariant\tPlacedShare%\tSlot\tRole\tChannels\tCoordinates\n");
        foreach (var s in sigs.OrderByDescending(s => weights.GetValueOrDefault(s.Shader)))
            foreach (var t in s.Textures.OrderBy(t => t.Slot))
                sb.Append($"{s.Shader}\t{s.Variant}\t{Share(weights.GetValueOrDefault(s.Shader))}\t{t.Slot}\t{t.Role}\t" +
                    $"{string.Join(" ", t.Channels.Select(c => $"{c.Key}>{string.Join(",", c.Value)}"))}\t{string.Join(" ", t.Coordinates)}\n");
        File.WriteAllText(Path.Combine(outDir, $"{game}_ShaderTextures.tsv"), sb.ToString());

        // Each permutation's texture slots, so a material can be matched to the one it would use
        sb = new StringBuilder("Shader\tPermutation\tSlots\tRoles\n");
        foreach (var s in sigs.Where(s => s.Permutations.Count > 0).OrderByDescending(s => weights.GetValueOrDefault(s.Shader)))
            foreach (var p in s.Permutations)
                sb.Append($"{s.Shader}\t{p.Number}\t{string.Join(" ", p.Slots)}\t" +
                    $"{string.Join(" ", p.Slots.Select(slot => s.Textures.FirstOrDefault(t => t.Slot == slot)?.Role ?? "?"))}\n");
        File.WriteAllText(Path.Combine(outDir, $"{game}_Permutations.tsv"), sb.ToString());

        // What each slot means to the game, for the binding audit in Tools/MaterialSurvey
        var meanings = new SortedDictionary<string, SlotMeanings>(StringComparer.Ordinal);
        foreach (var s in sigs)
        {
            var m = new SlotMeanings();
            foreach (var t in s.Textures.Where(t => t.Slot >= 0))
                m.Slots[t.Slot] = new SlotMeaning { Meaning = Meaning(t), UvSet = UvSet(t) };
            foreach (var p in s.Permutations) m.Permutations[p.Number] = p.Slots;
            meanings[s.Shader] = m;
        }
        File.WriteAllText(Path.Combine(outDir, $"{game}_SlotMeanings.json"), JsonSerializer.Serialize(meanings));

        File.WriteAllLines(Path.Combine(outDir, $"{game}_SignatureFailures.txt"), failures);
    }

    /// <summary>
    /// What a texture is to the game, from where its channels go (see GBuffer.md): rgb to RT2 is an albedo, rg to
    /// RT0 a normal (Gloss if its blue goes to RT1.y), red alone to RT1.x a metallic, rgb to RT4 an emissive.
    /// A channel that reaches the normal and the albedo at once weighs layers (Mask); one that only moves another
    /// sample's UVs is Coordinates. A texture can be several of these at once in a blend, in which case the
    /// first that matches wins.
    /// </summary>
    public static string Meaning(TextureUse t)
    {
        List<string> Of(string ch) => t.Channels.TryGetValue(ch, out var o) ? o : [];
        bool Has(string ch, string output) => Of(ch).Contains(output);
        bool Any(string ch, string prefix) => Of(ch).Exists(o => o.StartsWith(prefix, StringComparison.Ordinal));

        if (t.Channels.Count == 0) return "Unused";
        // A normal's red and green bend the normal, and its blue doesn't, as z is rebuilt from them. Some shaders
        // also color by the normal (crystal, trees), so reaching the albedo as well doesn't rule it out
        // Red and green that go nowhere but the normal are one too, whatever blue and alpha do (C[DetailBlend] also
        // weighs by them in some permutations)
        var onlyNormal = Of("r").Count > 0 && Of("g").Count > 0 && Of("r").Concat(Of("g")).All(o => o.StartsWith("RT0.", StringComparison.Ordinal));
        // A mask of two channels looks the same, except that as a layer's weight it reaches the gloss as well, which a
        // normal's red and green never do (its gloss is its blue)
        // A blue that is the gloss and nothing else marks a normal map however its red and green are used (AC6's [MS]
        // blends weigh their layers by the normals too)
        var blueIsGloss = Of("b").Count == 1 && Of("b")[0] == "RT1.y";
        if (Any("r", "RT0.") && Any("g", "RT0.") && blueIsGloss) return "NormalGloss";
        if (Any("r", "RT0.") && Any("g", "RT0.") && (onlyNormal || (!Any("b", "RT0.") && !Has("r", "RT1.y") && !Has("g", "RT1.y"))))
            return Has("b", "RT1.y") ? "NormalGloss" : "Normal";
        if (Has("r", "RT2.x") && Has("g", "RT2.y") && Has("b", "RT2.z") && !Any("r", "RT0.")) return "Albedo";
        if (Any("r", "RT4.") && !Any("r", "RT2.") && !Any("r", "RT0.")) return "Emissive";
        if (t.Channels.Values.Any(o => o.Exists(x => x.StartsWith("RT0.")) && o.Exists(x => x.StartsWith("RT2.")))) return "Mask";
        // Character shaders' wet surfaces darken the albedo by the metallic too, so only the normal rules it out
        if (Has("r", "RT1.x") && !Any("r", "RT0.") && Of("g").Count == 0 && Of("b").Count == 0) return "Metallic";
        if (t.Channels.Values.All(o => o.All(x => x.StartsWith("uv(")))) return "Coordinates";
        if (t.Channels.Values.Any(o => o.Contains("RT3.x"))) return "Subsurface";
        if (t.Channels.Values.All(o => o.All(x => x.EndsWith(".w") || x == "discard"))) return "Alpha";
        return "Other";
    }

    /// <summary>0 if the texture is sampled with the first UV set (TEXCOORD0.xy), 1 the second (TEXCOORD0.zw), -1 if neither or both</summary>
    public static int UvSet(TextureUse t)
    {
        var first = t.Coordinates.Contains("in:TEXCOORD0.x") || t.Coordinates.Contains("in:TEXCOORD0.y");
        var second = t.Coordinates.Contains("in:TEXCOORD0.z") || t.Coordinates.Contains("in:TEXCOORD0.w");
        return first == second ? -1 : first ? 0 : 1;
    }
}

class SlotMeaning
{
    public string Meaning { get; set; } = "";
    public int UvSet { get; set; } = -1;
}

class SlotMeanings
{
    public SortedDictionary<int, SlotMeaning> Slots { get; set; } = new();
    public SortedDictionary<int, List<int>> Permutations { get; set; } = new();
}
