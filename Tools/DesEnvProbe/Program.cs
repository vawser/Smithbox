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

// lights <USRDIR> <defs folder>: every MSBD Light event against its part and the area's PointLightBank
if (args[0] == "lights")
{
    var defs = Directory.GetFiles(args[2], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    var banks = new Dictionary<string, Andre.Formats.Param>();
    foreach (var bndPath in Directory.GetFiles(Path.Combine(args[1], "param", "drawparam"), "a??_drawparam.parambnd"))
    {
        var area = Path.GetFileName(bndPath).Substring(1, 2);
        var f = BND3.Read(bndPath).Files.FirstOrDefault(x => x.Name.EndsWith($"m{area}_PointLightBank.param"));
        if (f == null) continue;
        var p = Andre.Formats.Param.Read(f.Bytes);
        p.ApplyParamdef(defs.First(d => d.ParamType == p.ParamType));
        banks[area] = p;
        Console.WriteLine($"a{area} PointLightBank rows: " + string.Join(" ", p.Rows.Take(12).Select(r => $"{r.ID}'{r.Name}'")) + (p.Rows.Count > 12 ? " …" : ""));
    }
    var kinds = new Dictionary<string, int>();
    var idStats = new List<(string Map, int Id, int Unk, string Kind, string Model)>();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var mapId = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        foreach (var ev in msb.Events.Lights)
        {
            var part = msb.Parts.GetEntries().FirstOrDefault(p => p.Name == ev.PartName);
            var kind = part?.GetType().Name ?? "none";
            kinds[kind] = kinds.GetValueOrDefault(kind) + 1;
            idStats.Add((mapId, ev.PointLightID, ev.PointLightBankID, kind, part?.ModelName ?? "-"));
        }
    }
    Console.WriteLine("part kinds: " + string.Join(", ", kinds.Select(k => $"{k.Key} {k.Value}")));
    foreach (var g in idStats.GroupBy(s => s.Map))
    {
        var area = g.Key.Substring(1, 2);
        banks.TryGetValue(area, out var bank);
        var ids = g.Select(s => s.Id).ToList();
        int inBank = ids.Count(i => bank?[i] != null);
        int mod100 = ids.Count(i => bank?[i % 100] != null);
        int div10 = ids.Count(i => bank?[i / 10] != null);
        Console.WriteLine($"{g.Key}: {ids.Count} events, ids {ids.Min()}-{ids.Max()}, distinct {ids.Distinct().Count()}; row exists as id {inBank}, id%100 {mod100}, id/10 {div10}; PointLightBankID {string.Join(",", g.Select(s => s.Unk).Distinct().Take(8))}");
        Console.WriteLine("   models: " + string.Join(", ", g.GroupBy(s => s.Model).OrderByDescending(x => x.Count()).Take(6).Select(x => $"{x.Key}×{x.Count()} ids[{string.Join(",", x.Select(s => s.Id).Distinct().OrderBy(i => i).Take(6))}]")));
    }
}

// flverlights <flver>: a FLVER0's bones and dummies, to look for point light positions
if (args[0] == "flverlights")
{
    var bytes = args[1].EndsWith(".dcx") ? DCX.Decompress(args[1]).ToArray() : File.ReadAllBytes(args[1]);
    var f = FLVER0.Read(bytes);
    Console.WriteLine($"{Path.GetFileName(args[1])}: {f.Nodes.Count} bones, {f.Dummies.Count} dummies, {f.Materials.Count} materials, {f.Meshes.Count} meshes");
    foreach (var b in f.Nodes.Take(40)) Console.WriteLine($"  bone '{b.Name}' t {b.Translation} parent {b.ParentIndex}");
    foreach (var d in f.Dummies.Take(20)) Console.WriteLine($"  dummy ref {d.ReferenceID} pos {d.Position}");
    foreach (var m in f.Materials.Take(10)) Console.WriteLine($"  mat '{m.Name}' {m.MTD}");
}

// lightparts <USRDIR> <map> <model>: the Light events on parts of a model, with the part, and the FLVER's bone names
if (args[0] == "lightparts")
{
    var o = new StringBuilder();
    var msb = MSBD.Read(Path.Combine(args[1], "map", "MapStudio", args[2] + ".msb"));
    var parts = msb.Parts.GetEntries().Where(p => p.ModelName.Equals(args[3], StringComparison.OrdinalIgnoreCase)).ToList();
    foreach (var p in parts)
    {
        o.AppendLine($"part {p.Name} ({p.GetType().Name}) pos {p.Position} rot {p.Rotation}");
        foreach (var ev in msb.Events.Lights.Where(e => e.PartName == p.Name))
            o.AppendLine($"  light '{ev.Name}' id {ev.PointLightID} unk {ev.PointLightBankID} eventId {ev.EventID} region {ev.RegionName}");
    }
    var flv = Directory.GetFiles(Path.Combine(args[1], "map", args[2]), args[3] + "*.flver").FirstOrDefault();
    if (flv != null)
    {
        var f = FLVER0.Read(File.ReadAllBytes(flv));
        for (int i = 0; i < f.Nodes.Count; i++) o.AppendLine($"  bone {i} '{f.Nodes[i].Name}' t {f.Nodes[i].Translation}");
    }
    File.WriteAllText("lightparts.txt", o.ToString(), Encoding.UTF8);
    Console.WriteLine("Wrote lightparts.txt");
}

// lightnames <USRDIR>: whether Light event names (or their number) name any other MSB entry, and what other entries
// share the light's name prefix
if (args[0] == "lightnames")
{
    var o = new StringBuilder();
    int byName = 0, total = 0;
    var kinds = new Dictionary<string, int>();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var msb = MSBD.Read(msbPath);
        var entries = new List<(string Kind, string Name, object E)>();
        entries.AddRange(msb.Regions.GetEntries().Select(r => (r.GetType().Name, r.Name, (object)r)));
        entries.AddRange(msb.Parts.GetEntries().Select(p => ("Part." + p.GetType().Name, p.Name, (object)p)));
        entries.AddRange(msb.Events.GetEntries().Where(e => e is not MSBD.Event.Light).Select(e => ("Event." + e.GetType().Name, e.Name, (object)e)));
        foreach (var ev in msb.Events.Lights)
        {
            total++;
            var hit = entries.FirstOrDefault(e => e.Name == ev.Name);
            if (hit.Name != null) { byName++; kinds[hit.Kind] = kinds.GetValueOrDefault(hit.Kind) + 1; }
        }
        if (Path.GetFileNameWithoutExtension(msbPath) == "m01_00_00_00")
        {
            o.AppendLine("m01 region kinds: " + string.Join(", ", msb.Regions.GetEntries().GroupBy(r => r.GetType().Name).Select(g => $"{g.Key} {g.Count()}")));
            o.AppendLine("m01 regions with 燭 or light-ish names: " + string.Join(", ", msb.Regions.GetEntries().Where(r => r.Name.Contains("燭") || r.Name.Contains("光") || r.Name.Contains("ライト") || r.Name.ToLower().Contains("light")).Take(20).Select(r => $"{r.Name}@{r.Position}")));
            o.AppendLine("m01 parts with 燭: " + string.Join(", ", msb.Parts.GetEntries().Where(p => p.Name.Contains("燭")).Take(20).Select(p => $"{p.Name}({p.ModelName})@{p.Position}")));
            o.AppendLine("m01 light names: " + string.Join(", ", msb.Events.Lights.Take(30).Select(e => $"{e.Name}->{e.PartName}#{e.PointLightID}")));
        }
    }
    o.AppendLine($"Light events whose name names another entry: {byName} of {total}: " + string.Join(", ", kinds.Select(k => $"{k.Key} {k.Value}")));
    File.WriteAllText("lightnames.txt", o.ToString(), Encoding.UTF8);
    Console.WriteLine("Wrote lightnames.txt");
}

// lightregions <USRDIR>: for Light events with a same-name region, the region's index (in the full region list and
// among regions only), entity ID and position against the PointLightID; and the unmatched events
if (args[0] == "lightregions")
{
    var o = new StringBuilder();
    int idxEq = 0, ent = 0, matched = 0, offsetSame = 0;
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var map = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        var regions = msb.Regions.GetEntries();
        var offsets = new List<int>();
        foreach (var ev in msb.Events.Lights)
        {
            var ri = regions.FindIndex(r => r.Name == ev.Name);
            if (ri < 0) continue;
            matched++;
            if (ri == ev.PointLightID) idxEq++;
            if (regions[ri].EntityID == ev.PointLightID) ent++;
            offsets.Add(ri - ev.PointLightID);
        }
        var common = offsets.GroupBy(x => x).OrderByDescending(g => g.Count()).FirstOrDefault();
        if (common != null) offsetSame += common.Count();
        var unmatched = msb.Events.Lights.Where(ev => regions.All(r => r.Name != ev.Name)).ToList();
        o.AppendLine($"{map}: {msb.Events.Lights.Count} lights, {offsets.Count} with a region; region index - id: most common {common?.Key} ×{common?.Count()}, distinct {offsets.Distinct().Count()}; unmatched {unmatched.Count}: " +
            string.Join(", ", unmatched.Take(6).Select(e => $"{e.Name}->{e.PartName}#{e.PointLightID}")));
    }
    o.AppendLine($"matched {matched}: region index == id {idxEq}, EntityID == id {ent}, on the map's most common offset {offsetSame}");
    File.WriteAllText("lightregions.txt", o.ToString(), Encoding.UTF8);
    Console.WriteLine("Wrote lightregions.txt");
}

// lightindex <USRDIR>: every Light event's region by index PointLightID: in range or not, its name and shape, and the
// events whose region name differs from the event's
if (args[0] == "lightindex")
{
    var o = new StringBuilder();
    int inRange = 0, total = 0;
    var shapes = new Dictionary<string, int>();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var map = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        var regions = msb.Regions.GetEntries();
        foreach (var ev in msb.Events.Lights)
        {
            total++;
            if (ev.PointLightID < 0 || ev.PointLightID >= regions.Count) { o.AppendLine($"{map} {ev.Name} #{ev.PointLightID}: OUT OF RANGE ({regions.Count} regions)"); continue; }
            inRange++;
            var r = regions[ev.PointLightID];
            var shape = r.Shape?.GetType().Name ?? "null";
            shapes[shape] = shapes.GetValueOrDefault(shape) + 1;
            if (r.Name != ev.Name)
                o.AppendLine($"{map} '{ev.Name}' #{ev.PointLightID} unk {ev.PointLightBankID} part {ev.PartName} -> region '{r.Name}' {shape} @{r.Position}");
        }
    }
    o.AppendLine($"in range {inRange} of {total}; shapes: " + string.Join(", ", shapes.Select(s => $"{s.Key} {s.Value}")));
    File.WriteAllText("lightindex.txt", o.ToString(), Encoding.UTF8);
    Console.WriteLine("Wrote lightindex.txt");
}

// lightrows <USRDIR> <defs>: PointLightBankID against the area's PointLightBank: does the row exist, and its values
if (args[0] == "lightrows")
{
    var defs = Directory.GetFiles(args[2], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    var o = new StringBuilder();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var map = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        if (msb.Events.Lights.Count == 0) continue;
        var area = map.Substring(1, 2);
        var f = BND3.Read(Path.Combine(args[1], "param", "drawparam", $"a{area}_drawparam.parambnd")).Files.First(x => x.Name.EndsWith($"m{area}_PointLightBank.param"));
        var p = Andre.Formats.Param.Read(f.Bytes);
        p.ApplyParamdef(defs.First(d => d.ParamType == p.ParamType));
        foreach (var g in msb.Events.Lights.GroupBy(e => e.PointLightBankID).OrderBy(g => g.Key))
        {
            var row = p[g.Key];
            string V(string n) => row == null ? "-" : Convert.ToString(row[n].Value.Value);
            o.AppendLine($"{map} PointLightBankID {g.Key} ×{g.Count()}: " + (row == null ? "NO ROW" : $"'{row.Name}' dwindle {V("dwindleBegin")}-{V("dwindleEnd")} col {V("colR")},{V("colG")},{V("colB")} a{V("colA")}") + $"  e.g. {g.First().Name}");
        }
    }
    File.WriteAllText("lightrows.txt", o.ToString(), Encoding.UTF8);
    Console.WriteLine("Wrote lightrows.txt");
}

// enemycol <USRDIR>: enemies' own LightID against their CollisionName's LightID, per map
if (args[0] == "enemycol")
{
    int same = 0, diff = 0, none = 0;
    var o = new StringBuilder();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(args[1], "map", "MapStudio"), "m*.msb"))
    {
        var map = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        var cols = msb.Parts.Collisions.ToDictionary(c => c.Name, c => c);
        int s = 0, d = 0, n = 0;
        var pairs = new Dictionary<string, int>();
        foreach (var e in msb.Parts.Enemies.Cast<MSBD.Part.EnemyBase>().Concat(msb.Parts.DummyEnemies))
        {
            if (string.IsNullOrEmpty(e.CollisionName) || !cols.TryGetValue(e.CollisionName, out var c)) { n++; continue; }
            if (c.LightID == e.LightID) s++; else { d++; var k = $"{e.LightID}->{c.LightID}"; pairs[k] = pairs.GetValueOrDefault(k) + 1; }
        }
        same += s; diff += d; none += n;
        if (s + d + n > 0) o.AppendLine($"{map}: same {s}, differ {d}, no collision {n}; own->collision " + string.Join(", ", pairs.OrderByDescending(p => p.Value).Take(6).Select(p => $"{p.Key}×{p.Value}")));
    }
    o.AppendLine($"total: same {same}, differ {diff}, no collision {none}");
    Console.Write(o.ToString());
}

// rownames <USRDIR> <defs> <area> <bank> <ids...>: row names and a few fields of a bank's rows
if (args[0] == "rownames")
{
    var defs = Directory.GetFiles(args[2], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    var f = BND3.Read(Path.Combine(args[1], "param", "drawparam", $"a{args[3]}_drawparam.parambnd")).Files.First(x => x.Name.EndsWith($"m{args[3]}_{args[4]}.param"));
    var p = Andre.Formats.Param.Read(f.Bytes);
    p.ApplyParamdef(defs.First(d => d.ParamType == p.ParamType));
    var sb = new StringBuilder();
    foreach (var id in args.Skip(5).Select(int.Parse))
    {
        var r = p[id];
        sb.AppendLine(r == null ? $"{id}: no row" : $"{id} '{r.Name}': L0 {r["degRotX_0"].Value.Value}/{r["degRotY_0"].Value.Value} a{r["colA_0"].Value.Value}, du a{r["colA_du"].Value.Value}, u a{r["colA_u"].Value.Value}, envDif a{r["envDif_colA"].Value.Value}");
    }
    File.WriteAllText("rownames.txt", sb.ToString(), Encoding.UTF8);
}

// hemistats <USRDIR> <defs>: the hemisphere pairs (_u/_d, _du/_dd) of character rows (named キャラ) against the rest
if (args[0] == "hemistats")
{
    var defs = Directory.GetFiles(args[2], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    var groups = new Dictionary<string, List<(int U, int D, int Du, int Dd, int L0, int Env)>>();
    foreach (var bndPath in Directory.GetFiles(Path.Combine(args[1], "param", "drawparam"), "a??_drawparam.parambnd"))
    {
        var area = Path.GetFileName(bndPath).Substring(1, 2);
        var f = BND3.Read(bndPath).Files.First(x => x.Name.EndsWith($"m{area}_LightBank.param"));
        var p = Andre.Formats.Param.Read(f.Bytes);
        p.ApplyParamdef(defs.First(d => d.ParamType == p.ParamType));
        int V(Andre.Formats.Param.Row r, string n) => Convert.ToInt32(r[n].Value.Value);
        foreach (var r in p.Rows)
        {
            if (string.IsNullOrEmpty(r.Name)) continue;
            var key = r.Name.Contains("キャラ") ? "character rows" : "other named rows";
            if (!groups.TryGetValue(key, out var l)) groups[key] = l = new();
            l.Add((V(r, "colA_u"), V(r, "colA_d"), V(r, "colA_du"), V(r, "colA_dd"), V(r, "colA_0"), V(r, "envDif_colA")));
        }
    }
    var sb = new StringBuilder();
    foreach (var (k, l) in groups)
    {
        string S(Func<(int U, int D, int Du, int Dd, int L0, int Env), int> f) => $"mean {l.Average(x => f(x)):0.#}, zero {l.Count(x => f(x) == 0)}";
        sb.AppendLine($"{k} ({l.Count}): u [{S(x => x.U)}] d [{S(x => x.D)}] du [{S(x => x.Du)}] dd [{S(x => x.Dd)}] L0 [{S(x => x.L0)}] envDif [{S(x => x.Env)}]");
    }
    File.WriteAllText("hemistats.txt", sb.ToString(), Encoding.UTF8);
}
