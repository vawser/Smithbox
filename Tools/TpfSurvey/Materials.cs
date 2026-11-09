using SoulsFormats;

/// <summary>
/// Dumps the materials of every FLVER in a geombnd: the FLVER's own texture entries, and the MATBIN's samplers
/// (from allmaterial.matbinbnd), so texture roles can be checked against what the renderer binds.
/// </summary>
static class Materials
{
    public static void Run(string root, string geombnd)
    {
        var matbins = new Dictionary<string, MATBIN>(StringComparer.OrdinalIgnoreCase);
        var mbnd = BND4.Read(DCX.Decompress(File.ReadAllBytes(Path.Combine(root, @"material\allmaterial.matbinbnd.dcx"))).ToArray());
        foreach (var f in mbnd.Files.Where(f => f.Name.EndsWith(".matbin", StringComparison.OrdinalIgnoreCase)))
            matbins[Path.GetFileNameWithoutExtension(f.Name)] = MATBIN.Read(f.Bytes);

        var bnd = BND4.Read(DCX.Decompress(File.ReadAllBytes(Path.Combine(root, geombnd))).ToArray());
        foreach (var f in bnd.Files)
        {
            Console.WriteLine($"== {f.Name}");
            if (!f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)) continue;
            var flver = FLVER2.Read(f.Bytes);
            foreach (var m in flver.Materials)
            {
                Console.WriteLine($"  material '{m.Name}' mtd '{m.MTD}'");
                foreach (var t in m.Textures)
                    Console.WriteLine($"    flver tex type '{t.ParamName}' path '{t.Path}'");
                if (matbins.TryGetValue(Path.GetFileNameWithoutExtension(m.MTD), out var mb))
                {
                    Console.WriteLine($"    matbin shader '{mb.ShaderPath}' source '{mb.SourcePath}'");
                    foreach (var s in mb.Samplers)
                        Console.WriteLine($"    sampler '{s.Type}' key {s.Key} path '{s.Path}'");
                }
                else Console.WriteLine("    (matbin not found)");
            }
        }
    }
}

/// <summary>
/// Over every DB_NT FLVER: how often a FLVER texture entry has a path, how often it differs from the MATBIN
/// sampler, and whether the differing path's suffix fits the sampler's role.
/// </summary>
static class MaterialOverrides
{
    static string Role(string sampler)
    {
        var s = sampler.ToLowerInvariant();
        if (s.Contains("albedo")) return "albedo";
        if (s.Contains("normal")) return "normal";
        if (s.Contains("metallic")) return "metallic";
        if (s.Contains("mask")) return "mask";
        if (s.Contains("emissive")) return "emissive";
        return "other";
    }

    static string Suffix(string path)
    {
        var n = Path.GetFileNameWithoutExtension(path.Replace('\\', '/')).ToLowerInvariant();
        var i = n.LastIndexOf('_');
        return i < 0 ? "-" : n[i..];
    }

    public static void Run(string root)
    {
        var matbins = new Dictionary<string, MATBIN>(StringComparer.OrdinalIgnoreCase);
        var mbnd = BND4.Read(DCX.Decompress(File.ReadAllBytes(Path.Combine(root, @"material\allmaterial.matbinbnd.dcx"))).ToArray());
        foreach (var f in mbnd.Files.Where(f => f.Name.EndsWith(".matbin", StringComparison.OrdinalIgnoreCase)))
            matbins[Path.GetFileNameWithoutExtension(f.Name)] = MATBIN.Read(f.Bytes);

        int entries = 0, withPath = 0, same = 0, differ = 0, noMatbinPath = 0;
        var differBySuffix = new Dictionary<string, int>();
        var examples = new List<string>();
        foreach (var file in Directory.EnumerateFiles(root, "*.*bnd.dcx", SearchOption.AllDirectories)
                     .Where(p => p.EndsWith(".geombnd.dcx") || p.EndsWith(".chrbnd.dcx") || p.EndsWith(".partsbnd.dcx") || p.EndsWith(".mapbnd.dcx")))
        {
            BND4 bnd;
            try { bnd = BND4.Read(DCX.Decompress(File.ReadAllBytes(file)).ToArray()); } catch { continue; }
            foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
            {
                FLVER2 flver;
                try { flver = FLVER2.Read(f.Bytes); } catch { continue; }
                foreach (var m in flver.Materials)
                {
                    matbins.TryGetValue(Path.GetFileNameWithoutExtension(m.MTD), out var mb);
                    foreach (var t in m.Textures)
                    {
                        entries++;
                        if (string.IsNullOrEmpty(t.Path)) continue;
                        withPath++;
                        var sampler = mb?.Samplers.FirstOrDefault(s => s.Type == t.ParamName);
                        if (sampler == null || string.IsNullOrEmpty(sampler.Path)) { noMatbinPath++; continue; }
                        var a = Path.GetFileNameWithoutExtension(t.Path.Replace('\\', '/'));
                        var b = Path.GetFileNameWithoutExtension(sampler.Path.Replace('\\', '/'));
                        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) { same++; continue; }
                        differ++;
                        var key = $"{Role(t.ParamName)} flver{Suffix(t.Path)} matbin{Suffix(sampler.Path)}";
                        differBySuffix[key] = differBySuffix.GetValueOrDefault(key) + 1;
                        if (examples.Count < 25) examples.Add($"{Path.GetFileName(file)}:{m.Name} {t.ParamName}: flver {a} matbin {b}");
                    }
                }
            }
        }

        Console.WriteLine($"texture entries {entries}, with path {withPath}: same as matbin {same}, differ {differ}, matbin empty {noMatbinPath}");
        foreach (var kv in differBySuffix.OrderByDescending(k => k.Value)) Console.WriteLine($"  {kv.Value,6} {kv.Key}");
        foreach (var e in examples) Console.WriteLine($"  {e}");
    }
}
