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
            Add("Enemy time", $"{e.ActivateTimeStartHour:D2}:{e.ActivateTimeStartMinute:D2}-{e.ActivateTimeEndHour:D2}:{e.ActivateTimeEndMinute:D2}");
            Add("Enemy UnkT14", e.UnkT14);
        }
        foreach (var a in msb.Parts.Assets)
        {
            Add("Asset UnkT51", a.UnkT51);
            Add("Asset UnkT13", a.ExcludeActivateRatioOverride_PS4);
            Add("Asset UnkT52", a.ExcludeActivateRatioOverride_XboxOne);
        }
        var map = path.Split('/').Last()[..12];
        foreach (var p in msb.Parts.GetEntries())
        {
            if (p.UnkE3E != 0) Add("UnkE3E examples", $"{map} {p.GetType().Name} {p.Name} = {p.UnkE3E}");
            if (p.UnkE3E != 0 && map.EndsWith("_00") && map.StartsWith("m1")) Add("UnkE3E legacy _00", $"{map} {p.GetType().Name} {p.Name} ({p.ModelName})");
            Add("UnkE3E by map suffix", $"_{map[10..12]} {p.GetType().Name} {p.UnkE3E}");
            if (p.UnkE3C != -1) Add("UnkE3C examples", $"{map} {p.GetType().Name} {p.Name} = {p.UnkE3C}");
            Add("UnkE3E by type", $"{p.GetType().Name} {p.UnkE3E}");
            Add("UnkE3C by type", $"{p.GetType().Name} {p.UnkE3C}");
        }
        foreach (var a in msb.Parts.Assets)
            if (a.UnkT51 != 0) Add("UnkT51 examples", $"{map} {a.Name} ({a.ModelName}) = {a.UnkT51}");
        foreach (var c in msb.Parts.Collisions)
        {
            var v = c.DisplayGroupStruct.Condition;
            Add("Collision DisplayGroupStruct.Condition bytes", v == -1 ? "-1" : $"{v & 0xFF} {(v >> 8) & 0xFF} {(v >> 16) & 0xFF} {(v >> 24) & 0xFF}");
            if (v != -1 && v != 0) Add("Collision Condition (own map)", $"{path.Split('/').Last()[..12]} -> m{v & 0xFF:D2}_{(v >> 8) & 0xFF:D2}_{(v >> 16) & 0xFF:D2}");
        }
        foreach (var p in msb.Events.PlatoonInfo)
            Add("Platoon State bytes", $"{p.State} {p.UnkT05} {p.UnkT06} {p.UseMemberEntityIDs} ids {p.MemberEntityIDs[0]},{p.MemberEntityIDs[1]}");
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
