using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Security.Cryptography;

// Byte-perfect round trip of every Elden Ring MSB: decompressed bytes == MSBE.Read(...).Write(DCX.Type.None).
//   MsbRoundTrip <outFile> [GAME]
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.
// The output lists each file as "path<TAB>ok|DIFF@offset|ERROR msg<TAB>sha1 of the input", so two runs can be diffed.

var outFile = Path.GetFullPath(args[0]);
var gameName = args.Length > 1 ? args[1] : "ER";

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var root = gamePaths[gameName];
var game = Enum.Parse<Game>(gameName);
var fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, game), new RealVirtualFileSystem(root, false)]);

var paths = fs.GetFileNamesMatchingRecursive("/map/mapstudio", @"(?i)\.msb(\.dcx)?$").Select(p => p.Replace('\\', '/'))
    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

// stats mode: histograms of chosen values over every MSB instead of the round trip
if (args.Length > 2 && args[2] == "stats")
{
    var hist = new SortedDictionary<string, SortedDictionary<string, int>>();
    void Add(string key, object v)
    {
        if (!hist.TryGetValue(key, out var h)) hist[key] = h = new();
        var s = v?.ToString() ?? "null";
        h[s] = h.GetValueOrDefault(s) + 1;
    }
    foreach (var path in paths)
    {
        MSBE msb;
        try { msb = MSBE.Read(fs.ReadFile(path).Value.ToArray()); } catch { continue; }
        foreach (var e in msb.Parts.Enemies.Cast<MSBE.Part.EnemyBase>().Concat(msb.Parts.DummyEnemies))
        {
            Add("Enemy time", $"{e.UnkT28:D2}:{e.UnkT29:D2}-{e.UnkT2A:D2}:{e.UnkT2B:D2}");
            Add("Enemy UnkT14", e.UnkT14);
        }
        foreach (var a in msb.Parts.Assets)
        {
            Add("Asset UnkT51", a.UnkT51);
            Add("Asset UnkT13", a.UnkT13);
            Add("Asset UnkT52", a.UnkT52);
        }
        foreach (var p in msb.Events.PlatoonInfo)
            Add("Platoon State bytes", $"{p.State & 0xFF:X2} {(p.State >> 8) & 0xFF:X2} {(p.State >> 16) & 0xFF:X2} {(p.State >> 24) & 0xFF:X2}");
    }
    foreach (var (k, h) in hist)
    {
        Console.WriteLine($"== {k} ({h.Values.Sum()})");
        foreach (var (v, n) in h.OrderByDescending(x => x.Value).Take(25)) Console.WriteLine($"   {v}\t{n}");
    }
    return;
}

var lines = new List<string>();
int ok = 0, diff = 0, error = 0;
foreach (var path in paths)
{
    string result;
    string sha = "";
    try
    {
        var raw = fs.ReadFile(path).Value.ToArray();
        var plain = DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw;
        sha = Convert.ToHexString(SHA1.HashData(plain));
        var msb = MSBE.Read(plain);
        var back = msb.Write(DCX.Type.None);
        var n = Math.Min(back.Length, plain.Length);
        var at = -1;
        for (var i = 0; i < n; i++) if (back[i] != plain[i]) { at = i; break; }
        if (at < 0 && back.Length != plain.Length) at = n;
        if (at < 0) { result = "ok"; ok++; }
        else { result = $"DIFF@0x{at:X} ({plain.Length} -> {back.Length})"; diff++; }
    }
    catch (Exception e) { result = "ERROR " + e.Message.Replace('\n', ' '); error++; }
    lines.Add($"{path}\t{result}\t{sha}");
}

File.WriteAllLines(outFile, lines);
Console.WriteLine($"{paths.Count} MSBs: {ok} byte-perfect, {diff} differ, {error} errors");
foreach (var l in lines.Where(l => !l.Contains("\tok\t")).Take(20)) Console.WriteLine("  " + l);
