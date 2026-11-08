using SoulsFormats;
using System.Text;

// Reads every Demon's Souls file of the formats Smithbox's editors save, writes it back with SoulsFormats and compares.
// Usage: DesFormatRoundTrip <USRDIR> <paramdef folder> <report.md>
// exact = byte-identical; reread-equal = bytes differ but the re-read file has the same content (checked per format);
// differs / write failed / reread failed otherwise.

// paramstats <USRDIR>: the string-section layout of every DES param: where the header's strings offset points
// against the end of the row data and the first name, zero name offsets, shared name offsets, trailing bytes
if (args.Length > 0 && args[0] == "paramstats")
{
    var tally = new SortedDictionary<string, int>();
    var samples = new Dictionary<string, string>();
    void S(string k, string s) { tally[k] = tally.GetValueOrDefault(k) + 1; samples.TryAdd(k, s); }
    var bnds = Directory.GetFiles(Path.Combine(args[1], "param", "gameparam"), "*.parambnd")
        .Concat(Directory.GetFiles(Path.Combine(args[1], "param", "drawparam"), "*.parambnd"));
    foreach (var bndPath in bnds)
        foreach (var f in BND3.Read(bndPath).Files)
        {
            var name = $"{Path.GetFileName(bndPath)}/{Path.GetFileName(f.Name)}";
            var d = f.Bytes.ToArray();
            var br = new BinaryReaderEx(d[0x2C] == 0xFF, d);
            uint strings = br.GetUInt32(0);
            int dataStart = br.GetUInt16(4), rows = br.GetUInt16(0xA);
            var dataOffs = Enumerable.Range(0, rows).Select(i => br.GetUInt32(0x30 + i * 12 + 4)).ToList();
            var nameOffs = Enumerable.Range(0, rows).Select(i => br.GetUInt32(0x30 + i * 12 + 8)).ToList();
            long rowSize = rows > 1 ? dataOffs[1] - dataOffs[0] : 0;
            long dataEnd = rows > 0 ? dataOffs[^1] + rowSize : dataStart;
            var nz = nameOffs.Where(n => n != 0).ToList();
            long firstName = nz.Count > 0 ? nz.Min() : -1;
            S($"format {d[0x2C]:X2} {d[0x2D]:X2} {d[0x2E]:X2} {d[0x2F]:X2}", name);
            S($"first name - data end = {(firstName < 0 ? "no names" : (firstName - dataEnd).ToString())}", name);
            S($"strings offset - first name = {(firstName < 0 ? "no names" : ((long)strings - firstName).ToString())}", $"{name}: strings 0x{strings:X} first name 0x{firstName:X} data end 0x{dataEnd:X}");
            S($"strings offset - data end = {(long)strings - dataEnd}", $"{name}: strings 0x{strings:X} data end 0x{dataEnd:X}");
            S($"zero name offsets: {(nameOffs.Any(n => n == 0) ? "some" : "none")}", name);
            var dupNames = nz.GroupBy(n => n).Any(g => g.Count() > 1);
            var sameText = nz.Select(n => br.GetShiftJIS(n)).GroupBy(t => t).Any(g => g.Count() > 1);
            S($"shared offsets {dupNames}, repeated text {sameText}", name);
            // Name ends in order; trailing bytes after the last name's terminator
            if (nz.Count > 0)
            {
                long last = nz.Max();
                long end = last; while (end < d.Length && d[end] != 0) end++; end++;
                var trail = d.Length - end;
                bool allZero = d.Skip((int)end).All(b => b == 0);
                S($"trailing after last name: {trail} ({(allZero ? "zero" : "data")}), end%0x10={end % 0x10}", $"{name}: len 0x{d.Length:X} last name end 0x{end:X}");
                bool ordered = nz.SequenceEqual(nz.OrderBy(n => n));
                S($"names in row order: {ordered}", name);
            }
        }
    foreach (var (k, v) in tally) Console.WriteLine($"{v,4}  {k}   e.g. {samples[k]}");
    return;
}

// paramdump <parambnd> <param name> <paramdef folder>: header fields, row table and string layout, original next to rewritten
if (args.Length > 0 && args[0] == "paramdump")
{
    var bnd = BND3.Read(args[1]);
    var file = bnd.Files.First(f => Path.GetFileName(f.Name).Contains(args[2], StringComparison.OrdinalIgnoreCase));
    var defsP = Directory.GetFiles(args[3], "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();
    var orig = file.Bytes.ToArray();
    var p = PARAM.Read(orig);
    p.ApplyParamdefCarefully(defsP.First(d => d.ParamType == p.ParamType));
    var w = p.Write();
    foreach (var (label, data) in new[] { ("orig", orig), ("new", w) })
    {
        var br = new BinaryReaderEx(data[0x2C] == 0xFF, data);
        int rows = br.GetUInt16(0xA);
        Console.WriteLine($"[{label}] len {data.Length} strings 0x{br.GetUInt32(0):X} dataStart 0x{br.GetUInt16(4):X} unk06 {br.GetInt16(6)} ver {br.GetInt16(8)} rows {rows} format {data[0x2C]:X2} {data[0x2D]:X2} {data[0x2E]:X2} {data[0x2F]:X2}");
        Console.WriteLine($"   0x00 {Convert.ToHexString(data, 0, 0x40)}");
        int hdr = 0x30;
        var names = new List<string>();
        for (int i = 0; i < Math.Min(rows, 6); i++)
        {
            int o = hdr + i * 12;
            uint nameOff = br.GetUInt32(o + 8);
            names.Add($"{br.GetInt32(o)}: data 0x{br.GetUInt32(o + 4):X} name 0x{nameOff:X} '{(nameOff == 0 ? "(none)" : br.GetShiftJIS(nameOff))}'");
        }
        foreach (var n in names) Console.WriteLine($"   {n}");
        int rowEnd = hdr + rows * 12;
        Console.WriteLine($"   row table ends 0x{rowEnd:X}; bytes to data start: {Convert.ToHexString(data, rowEnd, Math.Max(0, Math.Min(0x40, br.GetUInt16(4) - rowEnd)))}");
        var nameOffs = Enumerable.Range(0, rows).Select(i => br.GetUInt32(hdr + i * 12 + 8)).ToList();
        Console.WriteLine($"   name offsets: {nameOffs.Count(n => n == 0)} zero, {nameOffs.Distinct().Count()} distinct of {rows}; min 0x{nameOffs.Where(n => n != 0).DefaultIfEmpty(0u).Min():X}");
        int tail = Math.Max(0, data.Length - 0x30);
        Console.WriteLine($"   tail {Convert.ToHexString(data, tail, data.Length - tail)}");
    }
    return;
}

// emptybnd <anibnd>: walks a BND3 whose header says 0 files (format 0x2E entries are 0x18 bytes) up to its
// first name, and finds the Havok packfiles and time acts in the rest of the file
if (args.Length > 0 && args[0] == "emptybnd")
{
    var bytes = File.ReadAllBytes(args[1]);
    var br = new BinaryReaderEx(true, bytes);
    Console.WriteLine($"{Path.GetFileName(args[1])}: {bytes.Length} bytes, version {Encoding.ASCII.GetString(bytes, 4, 8)}, format 0x{bytes[0xC]:X2}, count {br.GetInt32(0x10)}, header fields 0x14 {br.GetInt32(0x14):X}");
    int firstName = int.MaxValue, entries = 0, nonZero = 0;
    int stride = (bytes[0xC] & 0x20) != 0 ? 0x18 : 0x14;
    for (int o = 0x20; o + stride <= bytes.Length && o < firstName; o += stride, entries++)
    {
        int csize = br.GetInt32(o + 4), off = br.GetInt32(o + 8), id = br.GetInt32(o + 12), name = br.GetInt32(o + 16), usize = stride == 0x18 ? br.GetInt32(o + 20) : 0;
        if (name > 0 && name < firstName) firstName = name;
        if (csize != 0 || off != 0 || id != 0 || usize != 0) nonZero++;
        if (entries < 3) Console.WriteLine($"  entry {entries}: csize {csize} off 0x{off:X} id {id} name 0x{name:X} '{br.GetShiftJIS(name)}' usize {usize}");
    }
    Console.WriteLine($"  {entries} entries before the names at 0x{firstName:X}; {nonZero} with any size, offset or id");
    var hits = new List<string>();
    for (int i = 0; i + 8 <= bytes.Length; i += 4)
    {
        uint m = br.GetUInt32(i);
        if (m == 0x57E0E057 && br.GetUInt32(i + 4) == 0x10C0C010) hits.Add($"hkx@0x{i:X}");
        else if (m == 0x54414520) hits.Add($"TAE@0x{i:X}");
    }
    Console.WriteLine($"  {hits.Count} files found: {string.Join(" ", hits.Take(12))}{(hits.Count > 12 ? " ..." : "")}");
    int lastName = firstName;
    while (lastName < bytes.Length && (bytes[lastName] != 0 || (lastName + 1 < bytes.Length && bytes[lastName + 1] != 0))) lastName++;
    Console.WriteLine($"  names end near 0x{lastName:X}; first file data at {(hits.Count > 0 ? hits[0] : "none")}");
    return;
}

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

        // The class the Param Editor loads and saves with (ParamBank.LoadParameters_DES / SaveParameters_DES)
        Check("PARAM (editor, Andre.Formats)", name, f.Bytes.ToArray(),
            b =>
            {
                var p = Andre.Formats.Param.Read(b);
                var def = defs.FirstOrDefault(d => d.ParamType == p.ParamType);
                if (def == null) throw new InvalidDataException($"no paramdef for {p.ParamType}");
                p.ApplyParamdef(def);
                return p;
            },
            p => p.Write(), CompareAndreParam);
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

static string CompareAndreParam(Andre.Formats.Param a, Andre.Formats.Param b)
{
    if (a.Rows.Count != b.Rows.Count) return "rows: count";
    for (int r = 0; r < a.Rows.Count; r++)
    {
        var x = a.Rows[r]; var y = b.Rows[r];
        if (x.ID != y.ID) return $"rows: id {r}";
        if (x.Name != y.Name) return $"rows: name {x.ID} '{x.Name}' -> '{y.Name}'";
        for (int c = 0; c < x.Cells.Count; c++)
        {
            var vx = x.Cells[c].Value; var vy = y.Cells[c].Value;
            if (!Equals(vx, vy) && !(vx is byte[] bx && vy is byte[] by && bx.SequenceEqual(by)))
                return $"cells: {x.ID} {x.Cells[c].Def.InternalName}";
        }
    }
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
