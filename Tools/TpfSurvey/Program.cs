using SoulsFormats;
using System.Text;

// DB_NT (Switch 2) TPF survey. Parses the texture headers by hand, so it does not depend on the TPF reader's asserts,
// and groups them by the format fields with the data size against what linear BCn/ASTC layouts would need.
//   TpfSurvey                 census of every TPF (loose .tpf.dcx and .tpfbhd/.tpfbdt binders)
//   TpfSurvey dump <file>     hex dump of the start of one decompressed TPF
//   TpfSurvey check [n]       reads, lays out and headerizes every texture through SoulsFormats (first n files)
//   TpfSurvey raw <romfs-relative tpf> <texture> <bytes>   hex dump of one texture's data
//   TpfSurvey preview <tpf> <dir>   top mip of each texture as PNG
//   TpfSurvey materials <romfs-relative geombnd>   FLVER texture entries and MATBIN samplers per material
//   TpfSurvey overrides       FLVER texture paths against the MATBIN samplers over every model

var root = @"G:\Game Data Repository\Duskbloods\0400d1d02399a000-BebTjl\base\romfs";

if (args.Length >= 2 && args[0] == "dump")
{
    var b = DCX.Decompress(File.ReadAllBytes(args[1])).ToArray();
    for (int i = 0; i < Math.Min(b.Length, 0x400); i += 16)
        Console.WriteLine($"{i:X4}: " + string.Join(" ", b.Skip(i).Take(16).Select(x => x.ToString("X2"))));
    return;
}

if (args.Length >= 1 && args[0] == "check")
{
    Check.Run(root, args.Length >= 2 ? int.Parse(args[1]) : int.MaxValue);
    return;
}

if (args.Length >= 4 && args[0] == "raw")
{
    RawDump.Run(root, args[1], args[2], int.Parse(args[3]));
    return;
}

if (args.Length >= 1 && args[0] == "overrides")
{
    MaterialOverrides.Run(root);
    return;
}

if (args.Length >= 2 && args[0] == "materials")
{
    Materials.Run(root, args[1]);
    return;
}

if (args.Length >= 3 && args[0] == "preview")
{
    Check.Preview(args[1], args[2]);
    return;
}

var entries = new List<Entry>();
var errors = 0;

foreach (var path in Directory.EnumerateFiles(root, "*.tpf.dcx", SearchOption.AllDirectories))
{
    try { Parse(DCX.Decompress(File.ReadAllBytes(path)).ToArray(), Path.GetRelativePath(root, path), entries); }
    catch (Exception e) { errors++; if (errors < 10) Console.WriteLine($"ERR {path}: {e.Message}"); }
}
foreach (var bhd in Directory.EnumerateFiles(root, "*.tpfbhd", SearchOption.AllDirectories))
{
    try
    {
        var bdt = Path.ChangeExtension(bhd, ".tpfbdt");
        var bxf = BXF4.Read(File.ReadAllBytes(bhd), File.ReadAllBytes(bdt));
        foreach (var f in bxf.Files)
        {
            var data = DCX.Is(f.Bytes) ? DCX.Decompress(f.Bytes).ToArray() : f.Bytes.ToArray();
            Parse(data, Path.GetRelativePath(root, bhd) + ":" + f.Name, entries);
        }
    }
    catch (Exception e) { errors++; if (errors < 10) Console.WriteLine($"ERR {bhd}: {e.Message}"); }
}

Console.WriteLine($"{entries.Count} textures, {errors} errors");
Console.WriteLine();
Console.WriteLine("Header field census:");
foreach (var g in entries.GroupBy(e => (e.Platform, e.Flag2, e.Encoding))) Console.WriteLine($"  platform {g.Key.Platform:X} flag2 {g.Key.Flag2} enc {g.Key.Encoding}: {g.Count()}");
foreach (var g in entries.GroupBy(e => e.Type)) Console.WriteLine($"  type {g.Key}: {g.Count()}");
foreach (var g in entries.GroupBy(e => e.Flags1)) Console.WriteLine($"  flags1 {g.Key}: {g.Count()}");
foreach (var g in entries.GroupBy(e => e.Unk2)) Console.WriteLine($"  unk2 {g.Key}: {g.Count()}");
foreach (var g in entries.GroupBy(e => e.Unk3)) Console.WriteLine($"  unk3 0x{g.Key:X}: {g.Count()}");
foreach (var g in entries.GroupBy(e => e.FloatFlag)) Console.WriteLine($"  floatflag {g.Key}: {g.Count()}");
foreach (var g in entries.GroupBy(e => (e.Type, e.Count))) Console.WriteLine($"  type {g.Key.Type} count {g.Key.Count}: {g.Count()}");

Console.WriteLine();
Console.WriteLine("Format groups (fmt / fmt2): count, size / (w*h*mipFactor) ratio range, name suffixes, examples");
foreach (var g in entries.GroupBy(e => (e.Format, e.Format2)).OrderBy(g => g.Key.Format))
{
    var ratios = g.Select(e => e.Size / (double)(e.W * e.H * e.Count)).ToList();
    var suffixes = g.Select(e => Suffix(e.Name)).GroupBy(s => s).OrderByDescending(s => s.Count()).Take(6).Select(s => $"{s.Key}x{s.Count()}");
    Console.WriteLine($"  fmt 0x{g.Key.Format:X2} fmt2 0x{g.Key.Format2:X2} ({g.Key.Format2}): {g.Count()}  bytes/px {ratios.Min():F3}..{ratios.Max():F3}  [{string.Join(" ", suffixes)}]");
    foreach (var e in g.Take(4))
        Console.WriteLine($"      {e.W}x{e.H} mips {e.Mips} type {e.Type} count {e.Count} size 0x{e.Size:X}  {e.Name}  ({e.Source})");
}

static string Suffix(string name) { var i = name.LastIndexOf('_'); return i < 0 ? "-" : name[i..].ToLowerInvariant(); }

static void Parse(byte[] b, string source, List<Entry> list)
{
    var br = new BinaryReader(new MemoryStream(b));
    if (Encoding.ASCII.GetString(b, 0, 4) != "TPF\0") throw new Exception("not TPF");
    br.BaseStream.Position = 8;
    int count = br.ReadInt32();
    byte platform = br.ReadByte(), flag2 = br.ReadByte(), enc = br.ReadByte(); br.ReadByte();
    for (int i = 0; i < count; i++)
    {
        var e = new Entry { Source = source, Platform = platform, Flag2 = flag2, Encoding = enc };
        e.Offset = br.ReadUInt32(); e.Size = br.ReadInt32();
        e.Format = br.ReadByte(); e.Type = br.ReadByte(); e.Mips = br.ReadByte(); e.Flags1 = br.ReadByte();
        e.W = br.ReadInt16(); e.H = br.ReadInt16();
        e.Count = br.ReadInt32(); e.Unk2 = br.ReadInt32(); e.Unk3 = br.ReadInt32();
        uint nameOffset = br.ReadUInt32();
        e.FloatFlag = br.ReadInt32();
        e.Format2 = br.ReadInt32();
        if (e.FloatFlag == 1) { br.ReadInt32(); int len = br.ReadInt32(); br.BaseStream.Position += len; }
        var sb = new StringBuilder();
        for (var p = (int)nameOffset; p + 1 < b.Length; p += 2) { var c = BitConverter.ToChar(b, p); if (c == 0) break; sb.Append(c); }
        e.Name = sb.ToString();
        list.Add(e);
    }
}

class Entry
{
    public string Source, Name;
    public byte Platform, Flag2, Encoding, Format, Type, Mips, Flags1;
    public uint Offset;
    public int Size, Count, Unk2, Unk3, FloatFlag, Format2;
    public short W, H;
}
