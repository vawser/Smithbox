using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

// Phase 0 of the Unified Havok Library Plan. For a game, samples the Havok files in each kind of container,
// records what each file says about itself (container, SDK, pointer size, endian, classes), and runs every
// current library that claims the format through read → write → re-read → compare.
//
//   HavokLibSurvey <GAME|ALL> [containers per category = 30] [files per container = 8] [out dir] [workers]
//
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.
// Writes "<out dir>/<GAME>.md" (report) and "<out dir>/Golden/<GAME>.txt" (one file per distinct format/class set).

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var coreCheck = args.Contains("--core");
var coreRegistryLines = coreCheck ? CoreCheck.CompareRegistries() : null;
if (coreRegistryLines != null) foreach (var l in coreRegistryLines) Console.WriteLine(l);
Survey.TypeDiffEnabled = args.Contains("--typediff");
args = args.Where(a => a is not ("--core" or "--typediff")).ToArray();

var maxContainers = args.Length > 1 ? int.Parse(args[1]) : 30;
var maxFiles = args.Length > 2 ? int.Parse(args[2]) : 8;
var outDir = args.Length > 3 ? args[3] : Path.Combine("..", "..", "Documentation", "Claude", "Havok Library Survey");
var workers = args.Length > 4 ? int.Parse(args[4]) : Math.Clamp(Environment.ProcessorCount / 2, 1, 8);

var games = args[0].Equals("ALL", StringComparison.OrdinalIgnoreCase)
    ? gamePaths.Keys.Where(k => Enum.TryParse<Game>(k, out _)).ToList()
    : [args[0]];

Directory.CreateDirectory(Path.Combine(outDir, "Golden"));

foreach (var gameName in games)
{
    if (!gamePaths.TryGetValue(gameName, out var root) || !Directory.Exists(root))
    {
        Console.WriteLine($"{gameName}: no game folder ({root}), skipped");
        continue;
    }

    var game = Enum.Parse<Game>(gameName);
    var sw = Stopwatch.StartNew();
    Console.WriteLine($"==== {gameName}: {root}");

    using var fs = Survey.BuildFS(game, root, out var fsNote);
    TypeDiff.Reset();
    var containers = Survey.FindContainers(fs, maxContainers, out var categoryCounts);
    Console.WriteLine($"{containers.Count} containers sampled from {categoryCounts.Values.Sum()} ({fsNote})");

    var results = new ConcurrentBag<FileResult>();
    var containerErrors = new ConcurrentBag<(string Path, string Error)>();
    var queue = new ConcurrentQueue<ContainerRef>(containers);
    var done = 0;

    // Dedicated threads with large stacks: behavior graphs are deep, and both the readers and the graph
    // compare recurse through them.
    var threads = Enumerable.Range(0, workers).Select(_ => new Thread(() =>
    {
        while (queue.TryDequeue(out var c))
        {
            try
            {
                foreach (var r in Survey.SurveyContainer(fs, game, c, maxFiles))
                    results.Add(r);
            }
            catch (Exception e)
            {
                containerErrors.Add((c.Path, Library.Describe(e)));
            }
            var n = Interlocked.Increment(ref done);
            if (n % 10 == 0 || n == containers.Count)
                Console.WriteLine($"  {n}/{containers.Count} containers, {results.Count} files");
        }
    }, 512 * 1024 * 1024)).ToList();
    threads.ForEach(t => t.Start());
    threads.ForEach(t => t.Join());

    var reportPath = Path.Combine(outDir, $"{gameName}.md");
    var goldenPath = Path.Combine(outDir, "Golden", $"{gameName}.txt");
    Report.Write(reportPath, goldenPath, gameName, root, fsNote, args, coreRegistryLines, categoryCounts, containers.Count,
        results.ToList(), containerErrors.ToList(), sw.Elapsed,
        Survey.TypeDiffEnabled ? TypeDiff.Report(Havok.HavokProfile.FromGame(gameName)) : null);
    Console.WriteLine($"{gameName}: {results.Count} files in {sw.Elapsed.TotalSeconds:F0}s → {Path.GetFullPath(reportPath)}");
}

public sealed record ContainerRef(string Category, string Path, string BdtPath);

public sealed class FileResult
{
    public string Category;
    public string Container;
    public string Inner;
    public int Size;
    public SniffInfo Sniff;
    public string CoreSniffDiff; // Havok.Core disagreement with the survey sniffer, null when they agree
    public List<string> Classes = new();
    public List<LibResult> Libs = new();
}

public static class Survey
{
    public static bool TypeDiffEnabled;

    public static readonly Library[] Libraries =
    [
        new SoulsFormatsHkx(),
        new Hkx2Packfile(),
        new Hkx2Tagfile(),
        new HkLib2018(),
        new HkLib2019(),
        new UnifiedTagfile("Unified 2018 legacy", "2018", Havok.Formats.Tagfile.TagfileDialect.Hk2018Legacy, "HKLib 2018"),
        new UnifiedTagfile("Unified 2018", "2018", Havok.Formats.Tagfile.TagfileDialect.Hk2018, "HKLib 2018"),
        new UnifiedTagfile("Unified 2019", "2019", Havok.Formats.Tagfile.TagfileDialect.Hk2019, "HKLib 2019"),
    ];

    // Containers holding Havok files, by kind. Anything else is ignored.
    private static readonly (string Category, Regex Pattern)[] Categories =
    [
        ("hkxbhd", new Regex(@"\.hkxbhd$", RegexOptions.IgnoreCase)),
        ("anibnd", new Regex(@"\.anibnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("behbnd", new Regex(@"\.behbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("chrbnd", new Regex(@"\.chrbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("objbnd", new Regex(@"\.objbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("partsbnd", new Regex(@"\.partsbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("nvmhktbnd", new Regex(@"\.nvmhktbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("mapbnd", new Regex(@"\.mapbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("loose hkx", new Regex(@"\.(hkx|hkt)(\.dcx)?$", RegexOptions.IgnoreCase)),
    ];

    private static readonly Regex AnyContainer = new(
        @"\.(hkxbhd|anibnd|behbnd|chrbnd|objbnd|partsbnd|nvmhktbnd|mapbnd|hkx|hkt)(\.dcx)?$", RegexOptions.IgnoreCase);

    private static readonly Regex HavokInner = new(@"\.(hkx|hkt|hkc)(\.dcx)?$", RegexOptions.IgnoreCase);
    private static readonly Regex CompendiumInner = new(@"\.compendium(\.dcx)?$", RegexOptions.IgnoreCase);
    private static readonly Regex NestedBinder = new(@"bnd(\.dcx)?$", RegexOptions.IgnoreCase);

    public static VirtualFileSystem BuildFS(Game game, string root, out string note)
    {
        var real = new RealVirtualFileSystem(root, false);
        try
        {
            var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
            note = "archives + loose files";
            return new CompundVirtualFileSystem([binder, real]);
        }
        catch (Exception e)
        {
            note = $"loose files only ({e.GetType().Name})";
            return real;
        }
    }

    public static List<ContainerRef> FindContainers(VirtualFileSystem fs, int maxPerCategory, out Dictionary<string, int> categoryCounts)
    {
        var all = fs.GetFileNamesMatchingRecursive("/", AnyContainer.ToString())
            .Select(p => p.Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var picked = new List<ContainerRef>();
        categoryCounts = new Dictionary<string, int>();
        foreach (var group in all.GroupBy(p => Categories.First(c => c.Pattern.IsMatch(p)).Category))
        {
            var list = group.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            categoryCounts[group.Key] = list.Count;
            foreach (var p in Spread(list, maxPerCategory))
            {
                var bdt = group.Key == "hkxbhd" ? Path.ChangeExtension(p, ".hkxbdt") : null;
                picked.Add(new ContainerRef(group.Key, p, bdt));
            }
        }
        return picked;
    }

    /// <summary>
    /// Up to <paramref name="max"/> items spread evenly over the list, so a sample covers every area of the game.
    /// </summary>
    public static IEnumerable<T> Spread<T>(IReadOnlyList<T> list, int max)
    {
        if (list.Count <= max)
            return list;
        return Enumerable.Range(0, max).Select(i => list[(int)((long)i * list.Count / max)]);
    }

    private static byte[] Decompress(Memory<byte> bytes) =>
        DCX.Is(bytes) ? DCX.Decompress(bytes).ToArray() : bytes.ToArray();

    public static List<FileResult> SurveyContainer(VirtualFileSystem fs, Game game, ContainerRef c, int maxFiles)
    {
        var results = new List<FileResult>();

        if (c.Category == "loose hkx")
        {
            var bytes = Decompress(fs.ReadFileOrThrow(c.Path));
            var ctx = new ContainerContext { Game = game };

            // Loose files unpacked from a binder keep their compendium beside them
            var sniff = Sniffer.Sniff(bytes);
            if (sniff.UsesCompendium)
            {
                var dir = Path.GetDirectoryName(c.Path)?.Replace('\\', '/') ?? "/";
                foreach (var cp in fs.GetFileNamesMatching(dir, CompendiumInner.ToString()))
                {
                    var comp = Decompress(fs.ReadFileOrThrow(cp.Replace('\\', '/')));
                    if (ctx.Compendium == null || Sniffer.Sniff(comp).CompendiumIds.Contains(sniff.CompendiumRef))
                        ctx.Compendium = comp;
                }
            }
            results.Add(SurveyFile(ctx, c.Category, c.Path, Path.GetFileName(c.Path), bytes));
            return results;
        }

        IBinder binder;
        if (c.BdtPath != null)
        {
            var bhd = fs.ReadFileOrThrow(c.Path);
            var bdt = fs.ReadFileOrThrow(c.BdtPath);
            binder = BXF4.IsBHD(bhd) ? BXF4.Read(bhd, bdt) : BXF3.Read(bhd, bdt);
        }
        else
        {
            binder = ReadBinder(Decompress(fs.ReadFileOrThrow(c.Path)));
        }

        using (binder)
            SurveyBinder(binder, game, c.Category, c.Path, maxFiles, results, 0);
        return results;
    }

    private static IBinder ReadBinder(byte[] bytes)
    {
        if (BND4.Is(bytes)) return BND4.Read(bytes);
        if (BND3.Is(bytes)) return BND3.Read(bytes);
        throw new InvalidDataException("not a BND3/BND4");
    }

    private static void SurveyBinder(IBinder binder, Game game, string category, string path, int maxFiles, List<FileResult> results, int depth)
    {
        // A binder can hold several compendiums (AC6 and SDT keep one per animation subfolder). Tagfiles name the
        // one they use by id (TCRF), and each compendium lists its ids (TCID).
        var plain = new ContainerContext { Game = game };
        var byId = new Dictionary<ulong, ContainerContext>();
        ContainerContext firstComp = null;
        foreach (var compFile in binder.Files.Where(f => CompendiumInner.IsMatch(f.Name)))
        {
            var bytes = Decompress(compFile.Bytes);
            var sniff = Sniffer.Sniff(bytes);
            var cctx = new ContainerContext { Game = game, Compendium = bytes, CompendiumSdk = sniff.Sdk };
            firstComp ??= cctx;
            foreach (var id in sniff.CompendiumIds) byId.TryAdd(id, cctx);
            results.Add(new FileResult { Category = category, Container = path, Inner = compFile.Name, Size = bytes.Length, Sniff = sniff, CoreSniffDiff = CoreCheck.CompareSniff(bytes, sniff) });
            if (TypeDiffEnabled) TypeDiff.Collect(bytes, sniff, compFile.Name);
        }

        ContainerContext ContextFor(SniffInfo s) =>
            !s.UsesCompendium ? plain
            : byId.TryGetValue(s.CompendiumRef, out var c) ? c
            : firstComp ?? plain;

        // Skeletons are few and matter to every consumer, so always take them; spread the rest
        var havok = binder.Files.Where(f => HavokInner.IsMatch(f.Name)).OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var chosen = havok.Where(f => f.Name.Contains("skeleton", StringComparison.OrdinalIgnoreCase)).Take(2)
            .Concat(Spread(havok, maxFiles))
            .Distinct()
            .ToList();

        foreach (var f in chosen)
        {
            var bytes = Decompress(f.Bytes);
            results.Add(SurveyFile(ContextFor(Sniffer.Sniff(bytes)), category, path, f.Name, bytes));
        }

        if (depth > 0)
            return;
        foreach (var nested in binder.Files.Where(f => NestedBinder.IsMatch(f.Name)).Take(4))
        {
            try
            {
                using var inner = ReadBinder(Decompress(nested.Bytes));
                SurveyBinder(inner, game, category, $"{path} > {nested.Name}", maxFiles, results, depth + 1);
            }
            catch
            {
                // Not a binder after all
            }
        }
    }

    private static FileResult SurveyFile(ContainerContext ctx, string category, string container, string inner, byte[] bytes)
    {
        var r = new FileResult { Category = category, Container = container, Inner = inner, Size = bytes.Length, Sniff = Sniffer.Sniff(bytes) };
        r.CoreSniffDiff = CoreCheck.CompareSniff(bytes, r.Sniff);
        if (TypeDiffEnabled) TypeDiff.Collect(bytes, r.Sniff, inner);

        foreach (var lib in Libraries.Where(l => l.Applies(r.Sniff, ctx.Game)))
            r.Libs.Add(lib.Run(bytes, ctx, r.Sniff.UsesCompendium));

        // Unified engine vs the HKLib serializer it replaces: the same objects read, the same bytes written
        foreach (var (unified, lib) in r.Libs.Join(Libraries.OfType<UnifiedTagfile>(), l => l.Lib, u => u.Name, (l, u) => (l, u)))
        {
            unified.ParityWith = lib.ParityWith;
            var hklib = r.Libs.FirstOrDefault(l => l.Lib == lib.ParityWith);
            if (hklib == null || !hklib.Read || !unified.Read) continue;
            unified.ParityCompared = true;
            try { unified.ParityGraphDiff = GraphCompare.Compare(hklib.Root, unified.Root); }
            catch (Exception e) { unified.ParityGraphDiff = "compare threw: " + Library.Describe(e); }
            if (hklib.Written != null && unified.Written != null)
                unified.ParityBytesEqual = hklib.Written.AsSpan().SequenceEqual(unified.Written);
            if (unified.Written != null && !unified.ByteIdentical)
                unified.SectionDiff = SectionDiff.Describe(bytes, unified.Written);
        }

        // Classes: the packfile's own list, else the classes in the first typed graph that was read
        if (r.Sniff.Container == "Packfile" && r.Sniff.Classes.Count > 0)
            r.Classes = r.Sniff.Classes.Distinct().OrderBy(n => n).ToList();
        else if (r.Libs.FirstOrDefault(l => l.Read && l.Lib != "SoulsFormats HKX") is { } typed)
        {
            try { r.Classes = GraphCompare.HavokClasses(typed.Root); }
            catch { }
        }
        else
            r.Classes = r.Sniff.Classes.Where(n => n.Length > 2 && (n.StartsWith("hk") || n.StartsWith("fsnp") || n.StartsWith("CMSG") || n.StartsWith("Custom"))).Distinct().OrderBy(n => n).ToList();

        foreach (var l in r.Libs) { l.Root = null; l.Written = null; } // keep memory bounded
        return r;
    }
}
