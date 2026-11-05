using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;

// Byte-perfect round trip of every GPARAM the project locator picks up, per game.
//   GparamRoundTrip <outDir> [GAME...]
// Must run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.
// Matches ProjectFileLocator: /param/**.gparam, BB /param/**.gparambnd, DS2 /filter/**.fltparam.
//
// Per file it checks:
//   raw   - decompressed GPARAM bytes == GPARAM.Read(...).Write(DCX.Type.None)
//   save  - container bytes as the editor would save them (DCX / BND4 rewrite) == original

var outDir = Path.GetFullPath(args[0]);
Directory.CreateDirectory(outDir);

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

// GAME or GAME=path (path overrides game_data.txt, e.g. BB=...\Loose\dvdroot_ps4)
var games = new List<string>();
foreach (var a in args.Skip(1))
{
    var p = a.Split('=', 2);
    if (p.Length == 2) gamePaths[p[0]] = p[1];
    games.Add(p[0]);
}
if (games.Count == 0) games = gamePaths.Keys.ToList();

using var log = new StreamWriter(Path.Combine(outDir, "report.txt"));
void Log(string s) { Console.WriteLine(s); log.WriteLine(s); log.Flush(); }

foreach (var gameName in games)
{
    if (!gamePaths.TryGetValue(gameName, out var root) || !Directory.Exists(root))
    {
        Log($"== {gameName}: game folder missing, skipped");
        continue;
    }

    var game = Enum.Parse<Game>(gameName);
    VirtualFileSystem fs;

    try { fs = BuildFS(game, root); }
    catch (Exception e) { Log($"== {gameName}: VFS failed: {e.Message}"); continue; }

    var paths = new List<string>();
    paths.AddRange(Find(fs, "/param", @"(?i)\.gparam(\.dcx)?$"));
    if (game is Game.BB) paths.AddRange(Find(fs, "/param", @"(?i)\.gparambnd(\.dcx)?$"));
    if (game is Game.DS2 or Game.DS2S) paths.AddRange(Find(fs, "/filter", @"(?i)\.fltparam(\.dcx)?$"));
    paths = paths.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

    var stats = new Stats();
    var gameOut = Path.Combine(outDir, gameName);

    foreach (var path in paths)
    {
        byte[] raw;
        try { raw = fs.ReadFile(path).Value.ToArray(); }
        catch (Exception e) { stats.Fail($"{path}: read file failed: {e.Message}"); continue; }

        if (path.Contains(".gparambnd", StringComparison.OrdinalIgnoreCase))
            CheckBinder(path, raw, stats, gameOut);
        else
            CheckFile(path, raw, stats, gameOut);
    }

    Log($"== {gameName}: {paths.Count} files, {stats.Gparams} GPARAMs");
    foreach (var v in stats.Versions.OrderBy(k => k.Key))
        Log($"   version {v.Key}: {v.Value}");
    foreach (var c in stats.Compression.OrderBy(k => k.Key.ToString()))
        Log($"   dcx {c.Key}: {c.Value}");
    Log($"   raw  byte-perfect: {stats.RawOk}/{stats.Gparams}");
    Log($"   save byte-perfect: {stats.SaveOk}/{stats.SaveTotal}");
    foreach (var f in stats.Failures.Take(40))
        Log($"   ! {f}");
    if (stats.Failures.Count > 40)
        Log($"   ... {stats.Failures.Count - 40} more");
}

void CheckFile(string path, byte[] raw, Stats stats, string gameOut)
{
    byte[] plain;
    DCX.Type dcx = DCX.Type.None;

    try
    {
        plain = DCX.Is(raw) ? DCX.Decompress(raw, out dcx).ToArray() : raw;
    }
    catch (Exception e) { stats.Fail($"{path}: decompress failed: {e.Message}"); return; }

    stats.Compression[dcx] = stats.Compression.GetValueOrDefault(dcx) + 1;

    var gparam = CheckRaw(path, plain, stats, gameOut);
    if (gparam == null)
        return;

    // As GparamBank.HandleExternalFile saves: Read(file bytes).Write() with the read compression
    stats.SaveTotal++;
    try
    {
        var saved = GPARAM.Read(raw).Write();
        if (saved.AsSpan().SequenceEqual(raw))
            stats.SaveOk++;
        else
        {
            // Recompressed bytes differ; check whether the content still matches
            var back = DCX.Is(saved) ? DCX.Decompress(saved).ToArray() : saved;
            stats.Fail($"{path}: save bytes differ ({dcx}, {raw.Length} -> {saved.Length}, content {(back.AsSpan().SequenceEqual(plain) ? "same" : "DIFFERENT")})");
        }
    }
    catch (Exception e) { stats.Fail($"{path}: save failed: {e.Message}"); }
}

void CheckBinder(string path, byte[] raw, Stats stats, string gameOut)
{
    BND4 bnd;
    try { bnd = BND4.Read(raw); }
    catch (Exception e) { stats.Fail($"{path}: BND4 read failed: {e.Message}"); return; }

    stats.Compression[bnd.Compression] = stats.Compression.GetValueOrDefault(bnd.Compression) + 1;

    foreach (var file in bnd.Files)
    {
        var bytes = file.Bytes.ToArray();
        var inner = DCX.Is(bytes) ? DCX.Decompress(bytes).ToArray() : bytes;
        CheckRaw($"{path}|{file.Name}", inner, stats, gameOut);
    }

    // As GparamBank.HandleContainerFile saves: each GPARAM rewritten, binder rewritten
    stats.SaveTotal++;
    try
    {
        var bnd2 = BND4.Read(raw);
        foreach (var file in bnd2.Files)
            file.Bytes = GPARAM.Read(file.Bytes).Write();
        var saved = bnd2.Write();
        if (saved.AsSpan().SequenceEqual(raw))
            stats.SaveOk++;
        else
        {
            var plainRaw = DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw;
            var plainSaved = bnd2.Write(DCX.Type.None);
            stats.Fail($"{path}: binder save bytes differ ({bnd.Compression}, {raw.Length} -> {saved.Length}, content {(plainSaved.AsSpan().SequenceEqual(plainRaw) ? "same" : "DIFFERENT")})");
        }
    }
    catch (Exception e) { stats.Fail($"{path}: binder save failed: {e.Message}"); }
}

GPARAM CheckRaw(string name, byte[] plain, Stats stats, string gameOut)
{
    stats.Gparams++;

    GPARAM gparam;
    try { gparam = GPARAM.Read(plain); }
    catch (Exception e) { stats.Fail($"{name}: GPARAM read failed: {e.Message}"); return null; }

    var key = $"V{(int)gparam.Version}{(gparam.WideStrings ? " wide" : "")}";
    stats.Versions[key] = stats.Versions.GetValueOrDefault(key) + 1;

    byte[] written;
    try { written = gparam.Write(DCX.Type.None); }
    catch (Exception e) { stats.Fail($"{name}: GPARAM write failed: {e.Message}"); return null; }

    if (written.AsSpan().SequenceEqual(plain))
    {
        stats.RawOk++;
        return gparam;
    }

    int at = 0;
    while (at < plain.Length && at < written.Length && plain[at] == written[at]) at++;
    stats.Fail($"{name}: raw differs ({plain.Length} -> {written.Length}, first diff 0x{at:X})");

    // Keep the first few for inspection
    if (stats.Dumped++ < 5)
    {
        Directory.CreateDirectory(gameOut);
        var stem = Path.GetFileName(name.Replace('|', '_'));
        File.WriteAllBytes(Path.Combine(gameOut, stem + ".orig"), plain);
        File.WriteAllBytes(Path.Combine(gameOut, stem + ".written"), written);
    }

    return null;
}

static IEnumerable<string> Find(VirtualFileSystem fs, string dir, string pattern)
{
    try { return fs.GetFileNamesMatchingRecursive(dir, pattern).Select(p => p.Replace('\\', '/')).ToList(); }
    catch { return []; }
}

static VirtualFileSystem BuildFS(Game game, string root)
{
    var real = new RealVirtualFileSystem(root, false);
    if (game is Game.DB or Game.DB_NT or Game.BB or Game.DES) return real;
    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}

class Stats
{
    public int Gparams, RawOk, SaveOk, SaveTotal, Dumped;
    public Dictionary<string, int> Versions = new();
    public Dictionary<DCX.Type, int> Compression = new();
    public List<string> Failures = new();
    public void Fail(string s) => Failures.Add(s);
}
