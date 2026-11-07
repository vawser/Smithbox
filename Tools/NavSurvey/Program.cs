using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Text.RegularExpressions;

// NavSurvey extract <outDir>  : writes every ER .nvc / .onav, decompressed
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var mode = args[0];
if (mode == "exe") { ExeScan.Run(args); return; }
if (mode.StartsWith("raw")) { if (mode == "rawnvc") Raw.Nvc(args[1]); else if (mode == "rawkeys") Keys.Nvc(args[1]); else if (mode == "rawonav") Raw.Onav(args[1]); return; }
var root = gamePaths["ER"];
using var fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, Game.ER), new RealVirtualFileSystem(root, false)]);

var dict = File.ReadAllText(Path.Combine("Assets", "File Dictionaries", "ER-File-Dictionary.json"));
var paths = Regex.Matches(dict, @"""(/map/(?:nvc|onav)/[^""]+.(?:nvc|onav).dcx)""").Select(m => m.Groups[1].Value).Distinct().ToList();
Console.WriteLine($"{paths.Count} paths");

if (mode == "extract")
{
    var outDir = args[1];
    int ok = 0, missing = 0;
    foreach (var p in paths)
    {
        var f = fs.ReadFile(p);
        if (f == null) { missing++; continue; }
        var bytes = DCX.Decompress(f.Value.ToArray(), out DCX.Type ty);
        var dst = Path.Combine(outDir, p.TrimStart('/').Replace(".dcx", ""));
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        File.WriteAllBytes(dst, bytes);
        ok++;
        if (ok == 1) Console.WriteLine($"DCX type: {ty}");
    }
    Console.WriteLine($"extracted {ok}, missing {missing}");
}

// find <vfs path> <hex u32>... : where the values sit in a file or the files of a binder
if (mode == "find")
{
    var vals = args.Skip(2).Select(h => Convert.ToUInt32(h, 16)).ToArray();
    var data = DCX.Decompress(fs.ReadFile(args[1]).Value.ToArray(), out DCX.Type _);
    var files = new List<(string, byte[])>();
    if (BND4.IsRead(data, out BND4 bnd)) foreach (var f in bnd.Files) { var d = f.Bytes.ToArray(); if (DCX.Is(d)) d = DCX.Decompress(d).ToArray(); files.Add((f.Name, d)); }
    else files.Add((args[1], data));
    foreach (var (n, d) in files)
        for (int i = 0; i + 4 <= d.Length; i++)
        {
            var v = BitConverter.ToUInt32(d, i);
            if (vals.Contains(v)) Console.WriteLine($"{Path.GetFileName(n)} @0x{i:X} = {v:X8}  (len {d.Length})");
        }
}

// exists <regex> : which dictionary paths matching the regex exist in the game
if (mode == "exists")
{
    var re = new Regex(args[1]);
    var all = Regex.Matches(dict, "\"(/[^\"]+)\"").Select(m => m.Groups[1].Value).Where(p => re.IsMatch(p)).Distinct().ToList();
    var have = all.Where(p => fs.FileExists(p)).ToList();
    Console.WriteLine($"{all.Count} listed, {have.Count} exist");
    foreach (var p in have.Take(int.Parse(args.Length > 2 ? args[2] : "20"))) Console.WriteLine(p);
}

// dump <vfs path> [bytes] : hex of the start of a decompressed file
if (mode == "dump")
{
    var data = DCX.Decompress(fs.ReadFile(args[1]).Value.ToArray(), out DCX.Type _);
    int n = Math.Min(data.Length, args.Length > 2 ? int.Parse(args[2]) : 256);
    for (int i = 0; i < n; i += 16) Console.WriteLine($"{i:X6}: {BitConverter.ToString(data, i, Math.Min(16, n - i)).Replace("-", " ")}");
}

if (mode == "roundtrip")
    RoundTrip.Run(paths.Select(p => (p.Replace(".dcx", ""), fs.ReadFile(p))).Where(x => x.Item2 != null).Select(x => (x.Item1, DCX.Decompress(x.Item2.Value.ToArray(), out DCX.Type _))));
if (mode == "check") RoundTrip.Check(fs, paths);
if (mode == "mapnav") MapNav.Run(fs, dict, args);
if (mode == "navrows") MapNavRows.Run(fs, args[1]);
if (mode == "validate") Validate.Run(fs, dict, args);
if (mode == "overlay") Overlay.Run(fs, dict, args);
if (mode == "names") Names.Run(fs, dict, args);
if (mode == "havokedit") HavokEdit.Run(fs, dict, args);
if (mode == "navvalues") MapNavValues.Run(fs, dict, args[1], int.Parse(args[2]));
