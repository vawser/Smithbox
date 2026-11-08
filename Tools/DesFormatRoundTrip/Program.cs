using SoulsFormats;
using System.Text;

// Reads every Demon's Souls file of the formats Smithbox's editors save, writes it back with SoulsFormats and compares.
// Usage: DesFormatRoundTrip <USRDIR> <paramdef folder> <report.md>
// exact = byte-identical; reread-equal = bytes differ but the re-read file has the same content (checked per format);
// differs / write failed / reread failed otherwise.

// taegame <chr folder> <label>: the TAE roundtrip over another game's loose anibnds, for checking that the
// shared TAE code still writes them as before
if (args.Length > 0 && args[0] == "taegame")
{
    var tally = new SortedDictionary<string, List<string>>();
    void T(string k, string s) { if (!tally.TryGetValue(k, out var l)) tally[k] = l = new(); l.Add(s); }
    foreach (var path in Directory.GetFiles(args[1]).Where(p => p.EndsWith(".anibnd") || p.EndsWith(".anibnd.dcx")))
    {
        IBinder bnd;
        try { bnd = BND3.Is(path) ? BND3.Read(path) : BND4.Read(path); }
        catch (Exception e) { T("binder read failed", $"{Path.GetFileName(path)}: {e.Message}"); continue; }
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase)))
        {
            var name = $"{Path.GetFileName(path)}/{Path.GetFileName(f.Name)}";
            var bytes = f.Bytes.ToArray();
            TAE a;
            try { a = TAE.Read(bytes); } catch (Exception e) { T("read failed", $"{name}: {e.Message.Split('\n')[0]}"); continue; }
            byte[] w;
            try { w = a.Write(); } catch (Exception e) { T("write failed", $"{name}: {e.Message.Split('\n')[0]}"); continue; }
            if (w.AsSpan().SequenceEqual(bytes)) { T($"{a.Format} exact", name); continue; }
            string diff;
            try { diff = CompareTae(a, TAE.Read(w)); } catch (Exception e) { diff = "reread: " + e.Message.Split('\n')[0]; }
            T(diff == null ? $"{a.Format} bytes differ, content equal" : $"{a.Format} content differs: {diff.Split(':')[0]}",
                $"{name}: {diff ?? ""} (orig {bytes.Length}, new {w.Length}, first diff 0x{FirstDiff(bytes, w):X})");
        }
    }
    Console.WriteLine($"## {args[2]}");
    foreach (var (k, v) in tally)
    {
        Console.WriteLine($"{k}: {v.Count}");
        if (!k.EndsWith("exact"))
            foreach (var s in v.Take(4)) Console.WriteLine($"   {s}");
    }
    return;
}

var root = args.Length > 0 ? args[0] : @"G:\Game Data Repository\Demon's Souls\PS3_GAME\USRDIR";
var defFolder = args.Length > 1 ? args[1] : @"..\..\src\Smithbox.Data\Assets\PARAM\DES\Defs";
var reportPath = args.Length > 2 ? args[2] : "des_format_roundtrip.md";

if (args.Length > 3 && args[3].StartsWith("taeanim:"))
{
    // taeanim:<chr>/<file.tae>: each animation's struct offsets next to the 4 "flag" bytes of its anim file struct
    var parts = args[3][8..].Split('/');
    var bnd = BND3.Read(Path.Combine(root, "chr", parts[0], $"{parts[0]}.anibnd"));
    var orig = bnd.Files.First(f => Path.GetFileName(f.Name).Equals(parts[1], StringComparison.OrdinalIgnoreCase)).Bytes.ToArray();
    var br = new BinaryReaderEx(true, orig);
    int idAt = 0x50; while (br.GetInt32(idAt + 0x10) != 0x90) idAt += 4; int animCount = br.GetInt32(idAt + 4), animsOffset = br.GetInt32(idAt + 8);
    Console.WriteLine($"anims {animCount} at 0x{animsOffset:X}");
    for (int i = 0; i < animCount; i++)
    {
        int entry = animsOffset + i * 0x10;
        int id = br.GetInt32(entry), body = br.GetInt32(entry + 4);
        int evCount = br.GetInt32(body), evHdr = br.GetInt32(body + 4), grpCount = br.GetInt32(body + 8), grp = br.GetInt32(body + 12);
        int timesCount = br.GetInt32(body + 16), times = br.GetInt32(body + 20), animFile = br.GetInt32(body + 24);
        int nameOffOff = br.GetInt32(animFile + 4);
        int flags = br.GetInt32(nameOffOff + 4), src = br.GetInt32(nameOffOff + 8);
        Console.WriteLine($"anim {id}: entry 0x{entry:X} body 0x{body:X} evHdr 0x{evHdr:X} ({evCount}) groups 0x{grp:X} ({grpCount}) times 0x{times:X} ({timesCount}) animFile 0x{animFile:X} type {br.GetInt32(animFile)} flags 0x{flags:X8} src {src}");
    }
    return;
}

if (args.Length > 3 && args[3].StartsWith("taedump:"))
{
    // taedump:<chr>/<file.tae>: the first byte difference, and each event's parameter length before and after
    var parts = args[3][8..].Split('/');
    var bnd = BND3.Read(Path.Combine(root, "chr", parts[0], $"{parts[0]}.anibnd"));
    var file = bnd.Files.First(f => Path.GetFileName(f.Name).Equals(parts[1], StringComparison.OrdinalIgnoreCase));
    var orig = file.Bytes.ToArray();
    var tae = TAE.Read(orig);
    var w = tae.Write();
    var re = TAE.Read(w);
    int d = 0x10; while (d < Math.Min(orig.Length, w.Length) && orig[d] == w[d]) d++; // past the file size field
    if (Environment.GetEnvironmentVariable("TAE_AT") is string at) d = Convert.ToInt32(at, 16);
    Console.WriteLine($"format {tae.Format} BE={tae.BigEndian} orig {orig.Length} new {w.Length} first diff 0x{d:X}");
    for (int o = Math.Max(0, (d & ~0xF) - 0x20); o < Math.Min(orig.Length, (d & ~0xF) + 0xC0); o += 0x10)
        Console.WriteLine($"{o:X6} orig {Convert.ToHexString(orig, o, Math.Min(16, orig.Length - o))}   new {(o < w.Length ? Convert.ToHexString(w, o, Math.Min(16, w.Length - o)) : "")}");
    for (int a = 0; a < Math.Min(3, tae.Animations.Count); a++)
        for (int e = 0; e < tae.Animations[a].Events.Count; e++)
        {
            var x = tae.Animations[a].Events[e].GetParameterBytes(tae.BigEndian);
            var y = re.Animations[a].Events[e].GetParameterBytes(re.BigEndian);
            Console.WriteLine($"anim {tae.Animations[a].ID} ev {e} type {tae.Animations[a].Events[e].Type} unk04 {tae.Animations[a].Events[e].Unk04}/{re.Animations[a].Events[e].Unk04} len {x.Length}/{y.Length} {Convert.ToHexString(x)} | {Convert.ToHexString(y)}");
        }
    return;
}

var results = new SortedDictionary<string, SortedDictionary<string, List<string>>>();
void Add(string format, string bucket, string sample)
{
    if (!results.TryGetValue(format, out var b)) results[format] = b = new();
    if (!b.TryGetValue(bucket, out var l)) b[bucket] = l = new();
    l.Add(sample);
}

// Writes a file back and buckets it; compare decides content equality when the bytes differ
void Check<T>(string format, string name, byte[] bytes, Func<byte[], T> read, Func<T, byte[]> write, Func<T, T, string> compare)
{
    T a;
    try { a = read(bytes); }
    catch (Exception e) { Add(format, "read failed", $"{name}: {e.GetType().Name}: {e.Message}"); return; }

    byte[] w;
    try { w = write(a); }
    catch (Exception e) { Add(format, $"write failed: {e.GetType().Name}", $"{name}: {e.Message}"); return; }

    if (w.AsSpan().SequenceEqual(bytes)) { Add(format, "exact", name); return; }

    T b;
    try { b = read(w); }
    catch (Exception e) { Add(format, "reread failed", $"{name}: {e.GetType().Name}: {e.Message}"); return; }

    var diff = compare(a, b);
    if (diff == null)
        Add(format, "bytes differ, content equal", $"{name} (orig {bytes.Length}, new {w.Length}, first diff 0x{FirstDiff(bytes, w):X})");
    else
        Add(format, $"content differs: {diff.Split(':')[0]}", $"{name}: {diff}");
}

// --- MSBD
foreach (var path in Directory.GetFiles(Path.Combine(root, "map", "MapStudio"), "*.msb"))
{
    Check("MSBD", Path.GetFileName(path), File.ReadAllBytes(path), b => MSBD.Read(b), m => m.Write(), CompareMsb);
}

// --- PARAM (gameparam and drawparam binders), with the DES paramdefs Smithbox ships
var defs = Directory.GetFiles(defFolder, "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
var paramBnds = Directory.GetFiles(Path.Combine(root, "param", "gameparam"), "*.parambnd")
    .Concat(Directory.GetFiles(Path.Combine(root, "param", "drawparam"), "*.parambnd"));
foreach (var bndPath in paramBnds)
{
    var bnd = BND3.Read(bndPath);
    foreach (var f in bnd.Files)
    {
        var name = $"{Path.GetFileName(bndPath)}/{Path.GetFileName(f.Name)}";
        Check("PARAM", name, f.Bytes.ToArray(),
            b =>
            {
                var p = PARAM.Read(b);
                var def = defs.FirstOrDefault(d => d.ParamType == p.ParamType);
                if (def == null) throw new InvalidDataException($"no paramdef for {p.ParamType}");
                if (!p.ApplyParamdefCarefully(def)) throw new InvalidDataException($"paramdef {p.ParamType} doesn't fit");
                return p;
            },
            p => p.Write(), CompareParam);
    }
    // The binder itself
    Check("BND3 (parambnd)", Path.GetFileName(bndPath), File.ReadAllBytes(bndPath), b => BND3.Read(b), x => x.Write(), CompareBnd);
}

// --- FMG
foreach (var bndPath in Directory.GetFiles(Path.Combine(root, "msg"), "*.msgbnd", SearchOption.AllDirectories))
{
    var bnd = BND3.Read(bndPath);
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".fmg", StringComparison.OrdinalIgnoreCase)))
        Check("FMG", $"{Path.GetRelativePath(root, bndPath)}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray(), b => FMG.Read(b), x => x.Write(), CompareFmg);
}

// --- TAE, chr anibnds. "TAE (editor)" applies the DES template to every event it fits exactly, as the Time Act
// Editor does (TimeActEntry.ApplyTemplate), then writes
var taeTemplatePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(defFolder))!, "..", "..", "TAE", "DES", "Defs", "Character.xml");
TAE.Template.BankTemplate taeBank = null;
try { taeBank = TAE.Template.ReadXMLFile(taeTemplatePath, _ => Enumerable.Empty<KeyValuePair<string, string>>())[0]; }
catch (Exception e) { Add("TAE (editor)", "template failed to load", e.Message); }
int templatedEvents = 0, untemplatedEvents = 0;

TAE ApplyEditorTemplate(TAE tae)
{
    foreach (var anim in tae.Animations)
        foreach (var ev in anim.Events)
        {
            if (taeBank == null || !taeBank.TryGetValue(ev.Type, out var t)) { untemplatedEvents++; continue; }
            var original = ev.GetParameterBytes(tae.BigEndian);
            var copy = ev.GetClone(tae.BigEndian);
            bool fits;
            try
            {
                copy.ApplyTemplate(tae.BigEndian, t, true);
                var w = copy.GetParameterBytes(tae.BigEndian);
                fits = true;
                for (int i = 0; i < Math.Max(original.Length, w.Length) && fits; i++)
                    fits = (i < original.Length ? original[i] : 0) == (i < w.Length ? w[i] : 0);
            }
            catch { fits = false; }
            if (fits) { ev.ApplyTemplate(tae.BigEndian, t, true); templatedEvents++; }
            else untemplatedEvents++;
        }
    return tae;
}

foreach (var dir in Directory.GetDirectories(Path.Combine(root, "chr")))
{
    var ani = Path.Combine(dir, $"{Path.GetFileName(dir)}.anibnd");
    if (!File.Exists(ani)) continue;
    BND3 bnd;
    try { bnd = BND3.Read(ani); } catch { continue; }
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase)))
    {
        Check("TAE", $"{Path.GetFileName(dir)}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray(), b => TAE.Read(b), x => x.Write(), CompareTae);
        Check("TAE (editor)", $"{Path.GetFileName(dir)}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray(), b => ApplyEditorTemplate(TAE.Read(b)), x => x.Write(), CompareTae);
    }
    Check("BND3 (anibnd)", Path.GetFileName(ani), File.ReadAllBytes(ani), b => BND3.Read(b), x => x.Write(), CompareBnd);

    // The compressed twin is the copy the editors list; an empty read means the header's file count is zero
    var aniDcx = ani + ".dcx";
    if (File.Exists(aniDcx))
    {
        try
        {
            var n = BND3.Read(aniDcx).Files.Count;
            Add("BND3 (anibnd.dcx)", n == 0 ? "reads as empty" : "has files", $"{Path.GetFileName(aniDcx)}: {n} files, {new FileInfo(aniDcx).Length} bytes");
        }
        catch (Exception e) { Add("BND3 (anibnd.dcx)", "read failed", $"{Path.GetFileName(aniDcx)}: {e.Message}"); }
    }
}

// --- MTD
{
    var bnd = BND3.Read(Path.Combine(root, "mtd", "mtd.mtdbnd"));
    foreach (var f in bnd.Files)
        Check("MTD", Path.GetFileName(f.Name), f.Bytes.ToArray(), b => MTD.Read(b), x => x.Write(), (a, b) => a.Params.Count == b.Params.Count && a.Textures.Count == b.Textures.Count ? null : "counts");
}

// --- BND3 / TPF: chr binders and loose chr textures
foreach (var dir in Directory.GetDirectories(Path.Combine(root, "chr")))
{
    var id = Path.GetFileName(dir);
    var chr = Path.Combine(dir, $"{id}.chrbnd");
    if (File.Exists(chr))
        Check("BND3 (chrbnd)", Path.GetFileName(chr), File.ReadAllBytes(chr), b => BND3.Read(b), x => x.Write(), CompareBnd);
    var tpf = Path.Combine(dir, $"{id}.tpf");
    if (File.Exists(tpf))
        Check("TPF", Path.GetFileName(tpf), File.ReadAllBytes(tpf), b => TPF.Read(b), x => x.Write(), (a, b) => a.Textures.Count == b.Textures.Count ? null : "texture count");
}

// --- DCX: a compressed twin re-compressed with its own type
foreach (var path in Directory.GetFiles(Path.Combine(root, "chr"), "*.chrbnd.dcx", SearchOption.AllDirectories).Take(40))
{
    Check("DCX (chrbnd.dcx)", Path.GetFileName(path), File.ReadAllBytes(path),
        b => BND3.Read(b), x => x.Write(), CompareBnd);
}

// Per event type: the parameter sizes the files give it, against the template's size
var sizes = new SortedDictionary<int, SortedDictionary<int, int>>();
foreach (var dir in Directory.GetDirectories(Path.Combine(root, "chr")))
{
    var ani = Path.Combine(dir, $"{Path.GetFileName(dir)}.anibnd");
    if (!File.Exists(ani)) continue;
    BND3 sbnd;
    try { sbnd = BND3.Read(ani); } catch { continue; }
    foreach (var f in sbnd.Files.Where(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase)))
    {
        TAE t;
        try { t = TAE.Read(f.Bytes.ToArray()); } catch { continue; }
        foreach (var anim in t.Animations)
            for (int e = 0; e < anim.Events.Count; e++)
            {
                // The last event of an animation runs to the next struct, so its size says nothing
                if (e == anim.Events.Count - 1) continue;
                var ev = anim.Events[e];
                var n = ev.GetParameterBytes(t.BigEndian).Length;
                if (!sizes.TryGetValue(ev.Type, out var m)) sizes[ev.Type] = m = new();
                m[n] = m.GetValueOrDefault(n) + 1;
            }
    }
}

var sb = new StringBuilder();
sb.AppendLine("# Demon's Souls format roundtrip");
sb.AppendLine();
sb.AppendLine("## TAE parameter sizes against the DES template (events other than an animation's last)");
sb.AppendLine();
sb.AppendLine("| Type | Template size | Padded | File sizes (count) | Shrinks |");
sb.AppendLine("|---|---|---|---|---|");
foreach (var (type, m) in sizes)
{
    int? ts = taeBank != null && taeBank.TryGetValue(type, out var tt) ? tt.GetAllParametersByteCount() : null;
    int? padded = ts == null ? null : (ts.Value + 15) / 16 * 16;
    var shrinks = padded != null && m.Keys.Any(k => k > padded) ? "**yes**" : "";
    sb.AppendLine($"| {type} | {ts?.ToString() ?? "—"} | {padded?.ToString() ?? "—"} | {string.Join(", ", m.Select(kv => $"{kv.Key} ({kv.Value})"))} | {shrinks} |");
}
sb.AppendLine();
sb.AppendLine($"TAE (editor): {templatedEvents} event reads took the template, {untemplatedEvents} kept raw bytes (reads and re-reads both counted).");
sb.AppendLine();
sb.AppendLine("| Format | Bucket | Files |");
sb.AppendLine("|---|---|---|");
foreach (var (fmt, b) in results)
    foreach (var (k, v) in b)
        sb.AppendLine($"| {fmt} | {k} | {v.Count} |");
sb.AppendLine();
foreach (var (fmt, b) in results)
    foreach (var (k, v) in b)
    {
        if (k == "exact") continue;
        sb.AppendLine($"## {fmt}: {k}");
        foreach (var s in v.Take(8)) sb.AppendLine($"- {s}");
        sb.AppendLine();
    }
File.WriteAllText(reportPath, sb.ToString());
Console.WriteLine(sb.ToString());

static int FirstDiff(byte[] a, byte[] b)
{
    int n = Math.Min(a.Length, b.Length);
    for (int i = 0; i < n; i++) if (a[i] != b[i]) return i;
    return n;
}

static string CompareMsb(MSBD a, MSBD b)
{
    var la = ((IMsb)a); var lb = ((IMsb)b);
    if (la.Models.GetEntries().Count != lb.Models.GetEntries().Count) return "models: count";
    if (la.Parts.GetEntries().Count != lb.Parts.GetEntries().Count) return "parts: count";
    if (la.Regions.GetEntries().Count != lb.Regions.GetEntries().Count) return "regions: count";
    if (la.Events.GetEntries().Count != lb.Events.GetEntries().Count) return "events: count";
    var pa = la.Parts.GetEntries(); var pb = lb.Parts.GetEntries();
    for (int i = 0; i < pa.Count; i++)
        if (pa[i].Name != pb[i].Name || pa[i].Position != pb[i].Position || pa[i].ModelName != pb[i].ModelName) return $"parts: {i} {pa[i].Name}";
    return null;
}

static string CompareParam(PARAM a, PARAM b)
{
    if (a.Rows.Count != b.Rows.Count) return "rows: count";
    for (int r = 0; r < a.Rows.Count; r++)
    {
        if (a.Rows[r].ID != b.Rows[r].ID) return $"rows: id {r}";
        if (a.Rows[r].Name != b.Rows[r].Name) return $"rows: name {a.Rows[r].ID}";
        for (int c = 0; c < a.Rows[r].Cells.Count; c++)
            if (!Equals(a.Rows[r].Cells[c].Value, b.Rows[r].Cells[c].Value) && !(a.Rows[r].Cells[c].Value is byte[] x && b.Rows[r].Cells[c].Value is byte[] y && x.SequenceEqual(y)))
                return $"cells: {a.Rows[r].ID} {a.Rows[r].Cells[c].Def.InternalName}";
    }
    return null;
}

static string CompareFmg(FMG a, FMG b)
{
    if (a.Entries.Count != b.Entries.Count) return "entries: count";
    for (int i = 0; i < a.Entries.Count; i++)
        if (a.Entries[i].ID != b.Entries[i].ID || a.Entries[i].Text != b.Entries[i].Text) return $"entries: {a.Entries[i].ID}";
    return null;
}

static string CompareTae(TAE a, TAE b)
{
    if (a.Animations.Count != b.Animations.Count) return "animations: count";
    for (int i = 0; i < a.Animations.Count; i++)
    {
        var x = a.Animations[i]; var y = b.Animations[i];
        if (x.ID != y.ID) return $"animations: id {i}";
        if (x.EventGroups.Count != y.EventGroups.Count) return $"groups: count anim {x.ID}";
        for (int g = 0; g < x.EventGroups.Count; g++)
        {
            if (x.EventGroups[g].GroupType != y.EventGroups[g].GroupType) return $"groups: type anim {x.ID} group {g}";
            var mx = x.Events.Select(ev => ev.Group == null ? -1 : x.EventGroups.IndexOf(ev.Group)).ToList();
            var my = y.Events.Select(ev => ev.Group == null ? -1 : y.EventGroups.IndexOf(ev.Group)).ToList();
            if (!mx.SequenceEqual(my)) return $"groups: membership anim {x.ID}";
        }
        if (x.AnimFileName != y.AnimFileName) return $"animations: file name {x.ID}";
        if (x.MiniHeader.Type != y.MiniHeader.Type) return $"animations: mini header type {x.ID}";
        if (x.Events.Count != y.Events.Count) return $"events: count anim {x.ID}";
        for (int e = 0; e < x.Events.Count; e++)
        {
            var ex = x.Events[e]; var ey = y.Events[e];
            if (ex.Type != ey.Type || ex.StartTime != ey.StartTime || ex.EndTime != ey.EndTime) return $"events: header anim {x.ID} event {e}";
            if (!ex.GetParameterBytes(a.BigEndian).SequenceEqual(ey.GetParameterBytes(b.BigEndian))) return $"events: parameters anim {x.ID} event {e} type {ex.Type}";
        }
    }
    return null;
}

static string CompareBnd(BND3 a, BND3 b)
{
    if (a.Files.Count != b.Files.Count) return "files: count";
    for (int i = 0; i < a.Files.Count; i++)
    {
        if (a.Files[i].Name != b.Files[i].Name || a.Files[i].ID != b.Files[i].ID) return $"files: name/id {i}";
        if (!a.Files[i].Bytes.Span.SequenceEqual(b.Files[i].Bytes.Span)) return $"files: bytes {a.Files[i].Name}";
    }
    return null;
}
