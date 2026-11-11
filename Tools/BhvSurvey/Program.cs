using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Text.Json;

// BhvSurvey <extract|roundtrip|edit> [GAME ...]   (run from src/Smithbox.Data)
// Layout research on an extracted dump (BHVSURVEY_OUT): analyze, stateblob, blobs <ver> <kindHex> <max>, dump <file>
//   extract    writes every .bhv in the game's behbnds to BHVSURVEY_OUT (default: ./bhv_dump/<game>/<bnd>/<file>)
//              and prints each file's header
//   roundtrip  SoulsFormats.BHV read -> write -> byte compare for every .bhv
//   edit       edits each file through SoulsFormats.BHV, rewrites and re-reads it, and prints format facts
// Games default to AC6.

var mode = args.Length > 0 ? args[0] : "extract";
var outRootEnv = Environment.GetEnvironmentVariable("BHVSURVEY_OUT") ?? "bhv_dump";
if (mode == "analyze") { Analyze.Run(outRootEnv); return; }
if (mode == "stateblob") { StateBlob.Run(outRootEnv); return; }
if (mode == "blobs") { StateBlobExamples.Run(outRootEnv, int.Parse(args[1]), Convert.ToInt32(args[2], 16), int.Parse(args[3])); return; }
if (mode == "fields") { Fields.Run(outRootEnv, int.Parse(args[1]), args[2]); return; }
if (mode == "split") { Split.Run(outRootEnv); return; }
if (mode == "enummatch") { EnumMatch.Run(outRootEnv, args[1], int.Parse(args[2])); return; }
if (mode == "parammatch") { ParamMatch.Run(outRootEnv, args[1], int.Parse(args[2])); return; }
if (mode == "refmatch") { RefMatch.Run(outRootEnv, int.Parse(args[1])); return; }
if (mode == "statedataids") { StateDataIds.Run(outRootEnv, int.Parse(args[1])); return; }
if (mode == "findref") { FindRef.Run(args[1], args.Skip(2)); return; }
if (mode == "animrefs") { AnimRefs.Run(args[1], args.Skip(2)); return; }
if (mode == "dump") { Dump.Run(args[1]); return; }
var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "Claude", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var games = args.Length > 1 ? args[1..] : ["AC6"];
var outRoot = Environment.GetEnvironmentVariable("BHVSURVEY_OUT") ?? "bhv_dump";

foreach (var gameName in games)
{
    Console.WriteLine($"################ {gameName} ################");
    var root = gamePaths[gameName];
    var game = Enum.Parse<Game>(gameName);

    List<VirtualFileSystem> layers = [];
    try { layers.Add(ArchiveBinderVirtualFileSystem.FromGameFolder(root, game)); }
    catch (Exception e) { Console.WriteLine($"  no archives ({e.GetType().Name}), loose files only"); }
    layers.Add(new RealVirtualFileSystem(root, false));
    var fs = new CompundVirtualFileSystem(layers);

    var dictName = gameName == "DB_NT" ? "DB-NT" : gameName;
    var json = JsonDocument.Parse(File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{dictName}-File-Dictionary.json")));
    var bnds = json.RootElement.GetProperty("Entries").EnumerateArray()
        .Select(e => e.GetProperty("Path").GetString())
        .Where(p => p.EndsWith(".behbnd.dcx")).Distinct().ToList();

    int total = 0, ok = 0;
    var anibnds = json.RootElement.GetProperty("Entries").EnumerateArray().Select(e => e.GetProperty("Path").GetString()).Where(p => p.EndsWith(".anibnd.dcx")).Distinct().ToList();
    var editFiles = new List<(string, byte[])>();
    var animCache = new Dictionary<string, HashSet<long>>();
    foreach (var bndPath in bnds)
    {
        if (!fs.FileExists(bndPath)) { Console.WriteLine($"  missing {bndPath}"); continue; }
        BND4 bnd;
        try { bnd = BND4.Read(fs.ReadFile(bndPath).Value); }
        catch (Exception e) { Console.WriteLine($"  {bndPath}: {e.Message}"); continue; }

        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".bhv", StringComparison.OrdinalIgnoreCase)))
        {
            total++;
            var name = Path.GetFileName(f.Name);
            var bytes = f.Bytes.ToArray();
            var bndName = Path.GetFileName(bndPath).Split('.')[0];

            if (mode == "extract")
            {
                var dir = Path.Combine(outRoot, gameName, bndName);
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, name), bytes);
                var h = new BinaryReader(new MemoryStream(bytes));
                short ver = h.ReadInt16(); short u2 = h.ReadInt16(); int size = h.ReadInt32();
                h.BaseStream.Position = 0x20;
                var hdr = Enumerable.Range(0, 8).Select(_ => h.ReadInt32()).ToArray();
                Console.WriteLine($"  {bndName}/{name,-16} id={f.ID} v{ver} u2={u2} size={size:X}/{bytes.Length:X} " +
                    $"states@{hdr[0]:X}x{hdr[1]} B@{hdr[2]:X}x{hdr[3]} C@{hdr[4]:X}x{hdr[5] & 0xFFFF}(sz{hdr[5] >> 16:X}) D@{hdr[6]:X}x{hdr[7]}");
            }
            else if (mode == "animmatch")
            {
                if (!animCache.TryGetValue(bndName, out var anims)) animCache[bndName] = anims = AnimMatch.LoadAnims(fs, anibnds.Where(p => Path.GetFileName(p).StartsWith(bndName)));
                AnimMatch.Add(BHV.Read(bytes), anims);
            }
            else if (mode == "edit") editFiles.Add(($"{bndName}/{name}", bytes));
            else if (mode == "roundtrip")
            {
                try
                {
                    var bhv = BHV.Read(bytes);
                    var written = bhv.Write();
                    int diff = FirstDiff(bytes, written);
                    if (diff < 0) ok++;
                    else { Console.WriteLine($"  MISMATCH {bndName}/{name}: len {bytes.Length:X} -> {written.Length:X}, first diff @{diff:X}"); Directory.CreateDirectory(Path.Combine(outRoot, "mismatch")); File.WriteAllBytes(Path.Combine(outRoot, "mismatch", $"{bndName}_{name}.orig"), bytes); File.WriteAllBytes(Path.Combine(outRoot, "mismatch", $"{bndName}_{name}.new"), written); }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  FAIL {bndName}/{name}: {e.GetType().Name}: {e.Message}");
                }
            }
        }
    }
    if (mode == "edit") EditCheck.Run(editFiles);
    if (mode == "animmatch") { Console.WriteLine($"  anim sets: {string.Join(" ", animCache.Select(kv => $"{kv.Key}:{kv.Value.Count}"))}"); AnimMatch.Report(); }
    Console.WriteLine($"  {total} bhv files{(mode == "roundtrip" ? $", {ok} byte-identical" : "")}");
}

static int FirstDiff(byte[] a, byte[] b)
{
    int n = Math.Min(a.Length, b.Length);
    for (int i = 0; i < n; i++) if (a[i] != b[i]) return i;
    return a.Length == b.Length ? -1 : n;
}
