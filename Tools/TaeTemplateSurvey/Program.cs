using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Text;
using System.Xml;

// Surveys the time act events of a game against a TAE template.
//   TaeTemplateSurvey dump <GAME> <cacheFile>               reads every time act and caches the raw events
//   TaeTemplateSurvey report <cacheFile> <template> <out>   compares the cached events against the template
//   TaeTemplateSurvey exe <exe> <out> [cacheFile]           (see ExeScan.cs)
// dump must run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var mode = args[0];

if (mode == "dump")
{
    var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
        .Select(l => l.Split(':', 2))
        .Where(p => p.Length == 2)
        .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

    var game = Enum.Parse<Game>(args[1]);
    using var fs = BuildFS(game, gamePaths[args[1]]);
    Dump(fs, args[2]);
}
else if (mode == "report")
{
    Report.Run(args[1], args[2], args[3]);
}
else if (mode == "compare")
{
    // compare <template> <other>... : fields with no name, or a different layout, in the first template
    var main = TemplateReader.Read(args[1]);
    var others = args.Skip(2).Select(a => (Path.GetFileName(Path.GetDirectoryName(a)) + "/" + Path.GetFileName(a), TemplateReader.Read(a))).ToList();

    foreach (var ev in main.Values.OrderBy(e => e.ID))
    {
        var lines = new List<string>();

        foreach (var (name, other) in others)
        {
            if (!other.TryGetValue(ev.ID, out var o))
                continue;

            if (o.Name != ev.Name)
                lines.Add($"   [{name}] event name '{o.Name}'");

            if (o.Size != ev.Size)
                lines.Add($"   [{name}] size 0x{o.Size:X} vs 0x{ev.Size:X}");

            foreach (var f in o.Fields)
            {
                var m = ev.Fields.FirstOrDefault(x => x.Offset == f.Offset);
                var mName = m?.Name;
                if (m == null || m.Type != f.Type || (f.Name != null && f.Name != mName) || f.Enum.Count > (m?.Enum.Count ?? 0))
                    lines.Add($"   [{name}] +{f.Offset:X2} {f.Type} {f.Name} (enum {f.Enum.Count})   <-  ours: {m?.Type} {mName} (enum {m?.Enum.Count})");
            }
        }

        if (lines.Count > 0)
        {
            Console.WriteLine($"{ev.ID} {ev.Name}");
            lines.ForEach(Console.WriteLine);
        }
    }
}
else if (mode == "switches")
{
    ExeScan.SwitchesIn(args[1], Convert.ToUInt64(args[2], 16), Convert.ToUInt64(args[3], 16));
}
else if (mode == "disasm")
{
    ExeScan.Disasm(args[1], Convert.ToUInt64(args[2], 16), int.Parse(args[3]));
}
else if (mode == "trace")
{
    if (args.Contains("NR")) ParamTrace.UseNightreign();
    // trace <exe> <dispatcher> <template> <cache> <out> [type]
    ExeReport.Run(args[1], args[2].Split(",").Select(a => Convert.ToUInt64(a, 16)).ToArray(), args[3], args[4], args[5], args.Length > 6 && int.TryParse(args[6], out var only) ? only : -1);
}
else if (mode == "sizecheck")
{
    SizeCheck.Run(args[1], args[2]);
}
else if (mode == "roundtrip")
{
    RoundTrip.Run(args[1], args[2]);
}
else if (mode == "modules")
{
    ExeScan.Load(args[1]);
    Rtti.Load();
    // modules <exe> : finds the builders; modules <exe> <builder> <setSlot> <skip,...> : reads the slots
    if (args.Length < 4) Rtti.ModuleBag();
    else foreach (var (o, c) in Rtti.BagSlots(Convert.ToUInt64(args[2], 16), Convert.ToUInt64(args[3], 16), args.Length > 4 ? args[4].Split(",").Select(a => Convert.ToUInt64(a, 16)).ToArray() : new ulong[0]).OrderBy(x => x.Key)) Console.WriteLine($"  +{o:X3} {c}");
}
else if (mode == "vt")
{
    ExeScan.Load(args[1]);
    Rtti.Load();
    foreach (var a in args.Skip(2)) { var va = Convert.ToUInt64(a, 16); Console.WriteLine($"{a}: {(Rtti.VtableClass.TryGetValue(va, out var c) ? c : "-")} {(Rtti.VirtualOf.TryGetValue(va, out var v) ? v.Class + "::vf" + v.Index : "")}"); }
}
else if (mode == "unnamed")
{
    Unnamed.Run(args[1], args[2]);
}
else if (mode == "evidence")
{
    Evidence.Run(args[1], args[2]);
}
else if (mode == "apply")
{
    Apply.Run(args[1], args[2]);
}
else if (mode == "func")
{
    ExeScan.Load(args[1]);
    var f = Rtti.ParamTraceFunctions();
    foreach (var a in args.Skip(2)) Console.WriteLine($"{a} -> {f(Convert.ToUInt64(a, 16)):X}");
}
else if (mode == "findcmp")
{
    FindCmp.Run(args[1], long.Parse(args[2]));
}
else if (mode == "layoutdiff")
{
    LayoutDiff.Run(args[1], args[2]);
}
else if (mode == "port")
{
    Port.Run(args[1], args[2], args[3], args[4], args.Length > 5 ? args[5] : null);
}
else if (mode == "exe")
{
    ExeScan.Run(args[1], args[2], args.Length > 3 ? args[3] : null);
}

static VirtualFileSystem BuildFS(Game game, string root)
{
    var real = new RealVirtualFileSystem(root, false);
    if (game is Game.DB or Game.DB_NT or Game.BB or Game.DES) return real;
    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}

static void Dump(VirtualFileSystem fs, string cacheFile)
{
    var paths = new[] { "/chr", "/asset", "/obj", "/parts" }
        .SelectMany(d => fs.GetFileNamesMatchingRecursive(d, @"(?i)\.anibnd(\.dcx)?$"))
        .Select(p => p.Replace('\\', '/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
        .ToList();

    Console.WriteLine($"{paths.Count} binders");

    using var bw = new BinaryWriter(File.Create(cacheFile));
    int files = 0, events = 0;

    foreach (var path in paths)
    {
        byte[] raw;

        try
        {
            raw = fs.ReadFile(path).Value.ToArray();
            if (DCX.Is(raw))
                raw = DCX.Decompress(raw).ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine($"  {path}: {e.Message}");
            continue;
        }

        foreach (var (name, bytes) in TimeActs(raw, 0))
        {
            TAE tae;

            try
            {
                tae = TAE.Read(bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine($"  {path}/{name}: {e.Message}");
                continue;
            }

            files++;
            var source = $"{Path.GetFileName(path)}/{Path.GetFileName(name)}";

            foreach (var anim in tae.Animations)
            {
                foreach (var ev in anim.Events)
                {
                    var p = ev.GetParameterBytes(false);
                    bw.Write(ev.Type);
                    bw.Write(source);
                    bw.Write(anim.ID);
                    bw.Write(ev.StartTime);
                    bw.Write(ev.EndTime);
                    bw.Write(p.Length);
                    bw.Write(p);
                    events++;
                }
            }
        }
    }

    Console.WriteLine($"{files} time acts, {events} events");
}

// Time acts may sit in a binder inside a binder, as with the anibnds of objects
static IEnumerable<(string Name, byte[] Bytes)> TimeActs(byte[] data, int depth)
{
    if (depth > 2 || !BND4.Is(data))
        yield break;

    BND4 bnd;

    try
    {
        bnd = BND4.Read(data);
    }
    catch
    {
        yield break;
    }

    foreach (var file in bnd.Files)
    {
        var bytes = file.Bytes.ToArray();

        if (file.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase))
        {
            if (bytes.Length >= 4 && Encoding.ASCII.GetString(bytes, 0, 4) == "TAE ")
                yield return (file.Name, bytes);
        }
        else if (file.Name.Contains(".anibnd", StringComparison.OrdinalIgnoreCase))
        {
            if (DCX.Is(bytes))
                bytes = DCX.Decompress(bytes).ToArray();

            foreach (var inner in TimeActs(bytes, depth + 1))
                yield return inner;
        }
    }
}

public sealed record CachedEvent(int Type, string Source, long AnimID, float Start, float End, byte[] Bytes);

public static class Cache
{
    public static List<CachedEvent> Read(string file)
    {
        var list = new List<CachedEvent>();
        using var br = new BinaryReader(File.OpenRead(file));

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var type = br.ReadInt32();
            var source = br.ReadString();
            var anim = br.ReadInt64();
            var start = br.ReadSingle();
            var end = br.ReadSingle();
            var len = br.ReadInt32();
            list.Add(new CachedEvent(type, source, anim, start, end, br.ReadBytes(len)));
        }

        return list;
    }
}

public sealed class TemplateField
{
    public string Type;
    public string Name;
    public int Offset;
    public int Size;
    public string Assert;
    public Dictionary<long, string> Enum = new();
}

public sealed class TemplateEvent
{
    public int ID;
    public string Name;
    public List<TemplateField> Fields = new();
    public int Size => Fields.Count == 0 ? 0 : Fields[^1].Offset + Fields[^1].Size;
}

public static class TemplateReader
{
    public static int SizeOf(string type, XmlNode node) => type switch
    {
        "b" or "u8" or "s8" or "x8" => 1,
        "u16" or "s16" or "x16" => 2,
        "u32" or "s32" or "x32" or "f32" => 4,
        "f32grad" or "u64" or "s64" or "x64" or "f64" => 8,
        "aob" => int.Parse(node.Attributes["length"].Value),
        _ => throw new Exception(type)
    };

    public static Dictionary<int, TemplateEvent> Read(string path)
    {
        var xml = new XmlDocument();
        xml.Load(path);
        var result = new Dictionary<int, TemplateEvent>();

        foreach (XmlNode node in xml.DocumentElement.SelectNodes("action | event"))
        {
            var ev = new TemplateEvent { ID = int.Parse(node.Attributes["id"].Value), Name = node.Attributes["name"]?.Value };
            var offset = 0;

            foreach (XmlNode p in node.ChildNodes)
            {
                if (p.NodeType != XmlNodeType.Element)
                    continue;

                var f = new TemplateField
                {
                    Type = p.Name,
                    Name = p.Attributes["name"]?.Value,
                    Offset = offset,
                    Size = SizeOf(p.Name, p),
                    Assert = p.Attributes["assert"]?.Value,
                };

                foreach (XmlNode e in p.SelectNodes("entry"))
                {
                    if (long.TryParse(e.Attributes["value"].Value, out var v))
                        f.Enum[v] = e.Attributes["name"].Value;
                }

                ev.Fields.Add(f);
                offset += f.Size;
            }

            result[ev.ID] = ev;
        }

        return result;
    }
}
