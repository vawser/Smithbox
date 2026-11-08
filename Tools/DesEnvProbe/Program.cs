using SoulsFormats;
using System.Text;
using System.Text.RegularExpressions;

// Temporary probe for the DES DrawParam plan, Phase 5.
// tex <tpf> <name>: dumps the raw bytes and DDS header of one PS3 texture
// shaders <shaderbnd> <out.md>: lists a shaderbnd's entries and the identifier strings in each (Cg parameter names)
if (args[0] == "tex")
{
    var tpf = TPF.Read(args[1]);
    var t = tpf.Textures.First(x => x.Name.Equals(args[2], StringComparison.OrdinalIgnoreCase));
    Console.WriteLine($"{t.Name} fmt {t.Format} type {t.Type} mips {t.Mipmaps} bytes {t.Bytes.Length}");
    Console.WriteLine("raw: " + BitConverter.ToString(t.Bytes, 0, 64));
    return;
}

if (args[0] == "shaders")
{
    var bytes = File.ReadAllBytes(args[1]);
    IBinder bnd = BND3.Is(bytes) ? BND3.Read(bytes) : BND4.Read(bytes);
    var sb = new StringBuilder();
    sb.AppendLine($"# {Path.GetFileName(args[1])}: {bnd.Files.Count} entries");
    sb.AppendLine();
    var all = new SortedDictionary<string, int>();
    foreach (var f in bnd.Files)
    {
        var data = f.Bytes.ToArray();
        var ascii = Encoding.ASCII.GetString(data.Select(b => b >= 32 && b < 127 ? b : (byte)0).ToArray());
        var ids = Regex.Matches(ascii, @"[A-Za-z_][A-Za-z0-9_\.\[\]]{3,}").Select(m => m.Value).Distinct().ToList();
        foreach (var id in ids) all[id] = all.GetValueOrDefault(id) + 1;
        sb.AppendLine($"- `{f.Name}` ({data.Length} B): {string.Join(", ", ids.Take(60))}");
    }
    sb.AppendLine();
    sb.AppendLine("## Identifiers by entry count");
    sb.AppendLine();
    foreach (var (k, v) in all.OrderByDescending(kv => kv.Value)) sb.AppendLine($"- {k}: {v}");
    File.WriteAllText(args[2], sb.ToString());
    Console.WriteLine($"Wrote {args[2]}");
}

// mtds <mtdbnd> <out.md>: every MTD's shader path and params, and how many use each lighting-related value
if (args[0] == "mtds")
{
    var bnd = BND3.Read(args[1]);
    var sb = new StringBuilder();
    var shaders = new SortedDictionary<string, int>();
    var paramVals = new SortedDictionary<string, SortedDictionary<string, int>>();
    foreach (var f in bnd.Files)
    {
        var mtd = MTD.Read(f.Bytes);
        shaders[mtd.ShaderPath] = shaders.GetValueOrDefault(mtd.ShaderPath) + 1;
        foreach (var p in mtd.Params)
        {
            var v = p.Value is Array a ? string.Join(",", a.Cast<object>()) : p.Value?.ToString();
            if (!paramVals.TryGetValue(p.Name, out var d)) paramVals[p.Name] = d = new();
            d[v] = d.GetValueOrDefault(v) + 1;
        }
    }
    sb.AppendLine($"# DES MTDs: {bnd.Files.Count}");
    sb.AppendLine();
    sb.AppendLine("## Shader paths");
    foreach (var (k, v) in shaders) sb.AppendLine($"- {k}: {v}");
    sb.AppendLine();
    sb.AppendLine("## Params (value: count)");
    foreach (var (k, d) in paramVals) sb.AppendLine($"- **{k}**: " + string.Join("; ", d.OrderByDescending(x => x.Value).Take(12).Select(x => $"{x.Key}: {x.Value}")));
    File.WriteAllText(args[2], sb.ToString());
    Console.WriteLine($"Wrote {args[2]}");
}

// mtdcorr <mtdbnd>: g_LightingType and g_EnvSpcSlotNo by MTD folder, and slot against g_SpecularPower
if (args[0] == "mtdcorr")
{
    var bnd = BND3.Read(args[1]);
    var rows = new List<(string Folder, string Name, string LT, string Slot, string Pow, string Shader)>();
    foreach (var f in bnd.Files)
    {
        var mtd = MTD.Read(f.Bytes);
        string P(string n) => mtd.Params.FirstOrDefault(p => p.Name == n)?.Value?.ToString() ?? "-";
        var parts = f.Name.Split((char)92);
        rows.Add((parts.Length > 1 ? parts[^2] : "", parts[^1], P("g_LightingType"), P("g_EnvSpcSlotNo"), P("g_SpecularPower"), mtd.ShaderPath));
    }
    foreach (var g in rows.GroupBy(r => (r.Folder, r.LT)).OrderBy(g => g.Key.Folder).ThenBy(g => g.Key.LT))
        Console.WriteLine($"{g.Key.Folder} LightingType {g.Key.LT}: {g.Count()}  e.g. {string.Join(", ", g.Take(4).Select(r => r.Name))}");
    Console.WriteLine();
    foreach (var g in rows.Where(r => r.Slot != "-").GroupBy(r => r.Slot).OrderBy(g => g.Key))
        Console.WriteLine($"slot {g.Key}: SpecularPower " + string.Join(", ", g.GroupBy(r => r.Pow).OrderBy(x => double.Parse(x.Key == "-" ? "-1" : x.Key)).Select(x => $"{x.Key}×{x.Count()}")) + $"; LightingType " + string.Join(",", g.GroupBy(r => r.LT).Select(x => $"{x.Key}×{x.Count()}")));
    Console.WriteLine();
    foreach (var g in rows.GroupBy(r => r.LT).OrderBy(g => g.Key))
        Console.WriteLine($"LightingType {g.Key} shaders: " + string.Join(", ", g.GroupBy(r => Path.GetFileName(r.Shader)).OrderByDescending(x => x.Count()).Take(6).Select(x => $"{x.Key}×{x.Count()}")));
}

// peaks <strip.png> [count]: the brightest local maxima of a cubemap strip written by DesDrawParamSurvey envexport
// (faces +X -X +Y -Y +Z -Z, D3D cube convention), as directions, elevation and yaw (atan2(x, z))
if (args[0] == "peaks")
{
    using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(args[1]);
    int s = img.Height, n = args.Length > 2 ? int.Parse(args[2]) : 8;
    float L(int x, int y) { var p = img[x, y]; return (p.R + p.G + p.B) / 3f; }
    var peaks = new List<(float V, int F, int X, int Y)>();
    for (int f = 0; f < 6; f++)
        for (int y = 1; y < s - 1; y++)
            for (int x = 1; x < s - 1; x++)
            {
                float v = L(f * s + x, y);
                bool max = true;
                for (int dy = -1; dy <= 1 && max; dy++) for (int dx = -1; dx <= 1; dx++) if ((dx != 0 || dy != 0) && L(f * s + x + dx, y + dy) > v) { max = false; break; }
                if (max && v > 30) peaks.Add((v, f, x, y));
            }
    string[] faces = ["+X", "-X", "+Y", "-Y", "+Z", "-Z"];
    foreach (var p in peaks.OrderByDescending(p => p.V).DistinctBy(p => (p.F, p.X / 3, p.Y / 3)).Take(n))
    {
        float u = 2 * (p.X + 0.5f) / s - 1, v = 2 * (p.Y + 0.5f) / s - 1;
        var d = p.F switch { 0 => (1f, -v, -u), 1 => (-1f, -v, u), 2 => (u, 1f, v), 3 => (u, -1f, -v), 4 => (u, -v, 1f), _ => (-u, -v, -1f) };
        var len = MathF.Sqrt(d.Item1 * d.Item1 + d.Item2 * d.Item2 + d.Item3 * d.Item3);
        var (dx2, dy2, dz2) = (d.Item1 / len, d.Item2 / len, d.Item3 / len);
        Console.WriteLine($"{p.V,5:0} face {faces[p.F]} ({p.X},{p.Y}) dir ({dx2:0.00},{dy2:0.00},{dz2:0.00}) elev {MathF.Asin(dy2) * 180 / MathF.PI,6:0.0} yaw {MathF.Atan2(dx2, dz2) * 180 / MathF.PI,7:0.0}");
    }
}

// lightpeaks <USRDIR> <defs folder> <envtex png folder>: for each distinct (area, envSpc_3 map), the brightest peaks of
// that map (D3D cube directions, elevation, yaw = atan2(x, z)) beside the directions of the LightBank rows that use it
// (lights 0-2 and _s, elevation degRotX, yaw degRotY, intensity colA), to fit the yaw convention
if (args[0] == "lightpeaks")
{
    var defs = Directory.GetFiles(args[2], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    foreach (var bndPath in Directory.GetFiles(Path.Combine(args[1], "param", "drawparam"), "a??_drawparam.parambnd"))
    {
        var area = Path.GetFileName(bndPath).Substring(1, 2);
        var lf = BND3.Read(bndPath).Files.First(f => f.Name.EndsWith($"m{area}_LightBank.param"));
        var p = Andre.Formats.Param.Read(lf.Bytes);
        p.ApplyParamdef(defs.First(d => d.ParamType == p.ParamType));
        int V(Andre.Formats.Param.Row r, string n) => Convert.ToInt32(r[n].Value.Value);
        foreach (var g in p.Rows.GroupBy(r => V(r, "envSpc_3")))
        {
            var png = Path.Combine(args[3], $"m{area}_9999_EnvSpc_m{area}_{g.Key:000}.png");
            if (!File.Exists(png)) png = Directory.GetFiles(args[3], $"m{area}_9999_*", SearchOption.TopDirectoryOnly).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).EndsWith($"_{g.Key:000}", StringComparison.OrdinalIgnoreCase) && f.Contains("spc", StringComparison.OrdinalIgnoreCase));
            if (png == null) continue;
            Console.WriteLine($"== a{area} {Path.GetFileName(png)} used by {g.Count()} rows");
            foreach (var pk in Peaks(png, 3)) Console.WriteLine($"   peak {pk.V,4:0} elev {pk.Elev,6:0.0} yaw {pk.Yaw,7:0.0}");
            foreach (var r in g.Where(r => V(r, "colA_0") > 0 || V(r, "colA_1") > 0).Take(4))
                Console.WriteLine($"   row {r.ID,2} '{r.Name}': " + string.Join("  ", new[] { "0", "1", "2", "s" }.Select(k => $"L{k} {V(r, "degRotX_" + k)}/{V(r, "degRotY_" + k)} a{V(r, "colA_" + k)}")));
        }
    }
}

static List<(float V, float Elev, float Yaw)> Peaks(string path, int n)
{
    using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(path);
    int s = img.Height;
    float L(int x, int y) { var p = img[x, y]; return (p.R + p.G + p.B) / 3f; }
    var peaks = new List<(float V, float Elev, float Yaw)>();
    var raw = new List<(float V, int F, int X, int Y)>();
    for (int f = 0; f < 6; f++)
        for (int y = 1; y < s - 1; y++)
            for (int x = 1; x < s - 1; x++)
            {
                float v = L(f * s + x, y); bool max = true;
                for (int dy = -1; dy <= 1 && max; dy++) for (int dx = -1; dx <= 1; dx++) if ((dx != 0 || dy != 0) && L(f * s + x + dx, y + dy) > v) { max = false; break; }
                if (max && v > 30) raw.Add((v, f, x, y));
            }
    foreach (var p in raw.OrderByDescending(p => p.V))
    {
        float u = 2 * (p.X + 0.5f) / s - 1, v = 2 * (p.Y + 0.5f) / s - 1;
        var d = p.F switch { 0 => (1f, -v, -u), 1 => (-1f, -v, u), 2 => (u, 1f, v), 3 => (u, -1f, -v), 4 => (u, -v, 1f), _ => (-u, -v, -1f) };
        var len = MathF.Sqrt(d.Item1 * d.Item1 + d.Item2 * d.Item2 + d.Item3 * d.Item3);
        float elev = MathF.Asin(d.Item2 / len) * 180 / MathF.PI, yaw = MathF.Atan2(d.Item1, d.Item3) * 180 / MathF.PI;
        if (peaks.Any(q => MathF.Abs(q.Elev - elev) < 12 && MathF.Abs(((q.Yaw - yaw + 540) % 360) - 180) < 12)) continue;
        peaks.Add((p.V, elev, yaw));
        if (peaks.Count == n) break;
    }
    return peaks;
}
