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

if (args.Contains("--buildlayoutoverlay"))
{
    var target = Havok.HavokProfile.FromGame(args.First(a => !a.StartsWith("--")))!;
    foreach (var l in LayoutOverlay.Write(target, Havok.Reflection.HavokLayoutRules.ItaniumBB, "DS3")) Console.WriteLine(l);
    return;
}

if (args.Contains("--registryoverlay"))
{
    // --registryoverlay <base registry> <variant registry> <game> <output>
    var a = args.Where(x => x != "--registryoverlay").ToArray();
    foreach (var l in RegistryOverlay.Write(a[0], a[1], a[2], a[3])) Console.WriteLine(l);
    return;
}

if (args.Contains("--filldumps"))
{
    var fillLines = DumpFill.Run(Path.Combine("..", "Havok", "ClassDumps"), Path.Combine("..", "Havok", "HKX.ClassGeneration", "classxmlds3"));
    File.WriteAllLines(Path.Combine("..", "..", "Documentation", "Claude", "Havok Library Survey", "Dump Fill.md"), fillLines);
    foreach (var l in fillLines) Console.WriteLine(l);
    return;
}

if (args.Contains("--layoutcheck"))
{
    var layoutLines = LayoutCheck.Run();
    var layoutPath = Path.Combine("..", "..", "Documentation", "Claude", "Havok Library Survey", "Layout Check.md");
    File.WriteAllLines(layoutPath, layoutLines);
    foreach (var l in layoutLines) Console.WriteLine(l);
    return;
}

var coreCheck = args.Contains("--core");
// --core: Core's header sniffing against the survey's (per file). Its registries were compared with HKLib's until Phase 7.
List<string> coreRegistryLines = null;
if (coreRegistryLines != null) foreach (var l in coreRegistryLines) Console.WriteLine(l);
Survey.BuildRegistry = args.Contains("--buildregistry");
Survey.BuildOverlay = args.Contains("--buildoverlay");
var layoutPredict = args.Contains("--layoutpredict");
Survey.DumpClasses = args.Contains("--dumpclasses");
Survey.TypeDiffEnabled = args.Contains("--typediff") || layoutPredict || Survey.BuildRegistry || Survey.BuildOverlay;
args = args.Where(a => a is not ("--core" or "--typediff" or "--buildregistry" or "--buildoverlay" or "--layoutpredict" or "--dumpclasses")).ToArray();

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
    ClassDump.Reset();
    DecodeCompare.Reset();
    var profile = Havok.HavokProfile.FromGame(gameName);
    var buildSdk = profile?.Sdk switch
    {
        Havok.HavokSdk.Hk2015 => "20150100",
        Havok.HavokSdk.Hk2016 => "20160200",
        _ => null
    };
    Survey.BuildSdkPrefix = Survey.BuildRegistry ? buildSdk?[..4] : Survey.BuildOverlay ? profile?.Sdk switch { Havok.HavokSdk.Hk2018 => "2018", Havok.HavokSdk.Hk2019 => "2019", _ => null } : null;
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
    var typeLines = Survey.TypeDiffEnabled ? TypeDiff.Report(profile) : null;
    if (Survey.BuildRegistry)
    {
        // registries for SDKs that have none, written into Havok.Core's Res folder
        if (buildSdk == null)
            typeLines.Add($"- --buildregistry: {gameName} already has a registry (or its SDK isn't built from files); nothing written.");
        else
        {
            var registryPath = Path.Combine("..", "Havok", "Havok.Core", "Res", $"HavokTypeRegistry{buildSdk}.xml");
            typeLines.AddRange(TypeDiff.WriteRegistry(registryPath, buildSdk, gameName));
            foreach (var l in typeLines.TakeLast(3)) Console.WriteLine(l);
        }
    }
    if (Survey.BuildOverlay && profile?.RegistryFile != null)
    {
        // an overlay of the game's own types on its SDK's base registry, written into Havok.Core's Res folder
        var overlayPath = Path.Combine("..", "Havok", "Havok.Core", "Res",
            $"{Path.GetFileNameWithoutExtension(profile.RegistryFile)}_{gameName}.xml");
        var overlayLines = TypeDiff.WriteOverlay(overlayPath, profile);
        typeLines.AddRange(overlayLines);
        foreach (var l in overlayLines.Take(12)) Console.WriteLine(l);
    }
    var decodeLines = DecodeCompare.Report();
    if (decodeLines.Count > 0)
    {
        (typeLines ??= new List<string>()).AddRange(decodeLines);
        foreach (var l in decodeLines.Skip(5).Take(12)) Console.WriteLine(l);
    }
    if (Survey.DumpClasses)
    {
        // class dumps from the files' own hkClass reflection, for Tools/HavokClassDumpConvert
        var dumpLines = ClassDump.Write(Path.Combine("..", "Havok", "ClassDumps"), gameName);
        (typeLines ??= new List<string>()).AddRange(dumpLines);
        foreach (var l in dumpLines) Console.WriteLine(l);
    }
    if (layoutPredict && profile?.RegistryFile != null)
    {
        var predictLines = LayoutPredict.Report(profile, TypeDiff.CollectedTypes, Havok.Reflection.HavokLayoutRules.Itanium);
        typeLines.AddRange(predictLines);
        foreach (var l in predictLines) Console.WriteLine(l);
    }
    Report.Write(reportPath, goldenPath, gameName, root, fsNote, args, coreRegistryLines, categoryCounts, containers.Count,
        results.ToList(), containerErrors.ToList(), sw.Elapsed, typeLines);
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
    public string Layout;        // tagfile section tree
    public List<string> Classes = new();
    public List<LibResult> Libs = new();
}

public static class Survey
{
    public static bool TypeDiffEnabled;
    public static bool BuildRegistry;
    public static bool BuildOverlay;
    public static bool DumpClasses;
    public static string BuildSdkPrefix;

    public static readonly Library[] Libraries =
    [
        new SoulsFormatsHkx(),
        new GeneratedPackfile(),
        new GeneratedPackfile(preserveSource: false),
        new GeneratedPackfile32(),
        new UnifiedTagfile("Generated 2018", "2018", Havok.Formats.Tagfile.TagfileDialect.Hk2018, null, Havok.hk2018.ClassSet.For),
        new UnifiedTagfile("Generated 2019", "2019", Havok.Formats.Tagfile.TagfileDialect.Hk2019, null, Havok.hk2019.ClassSet.For),
        new UnifiedTagfile("Unified 2016", "2016", Havok.Formats.Tagfile.TagfileDialect.Hk2016, null, _ => Havok.hk2016.ClassSet.For(Havok.HavokProfile.SDT)),
        new UnifiedTagfile("Unified 2015", "2015", Havok.Formats.Tagfile.TagfileDialect.Hk2015, null, _ => Havok.hk2015.ClassSet.For(Havok.HavokProfile.DS1R)),
    ];

    // Containers holding Havok files, by kind. Anything else is ignored.
    private static readonly (string Category, Regex Pattern)[] Categories =
    [
        ("hkxbhd", new Regex(@"\.hkxbhd$", RegexOptions.IgnoreCase)),
        ("anibnd", new Regex(@"\.anibnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("behbnd", new Regex(@"\.behbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("chrbnd", new Regex(@"\.chrbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("objbnd", new Regex(@"\.objbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        // asset collision (ER, NR, AC6, the Duskbloods)
        ("geomhkxbnd", new Regex(@"\.geomhkxbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("partsbnd", new Regex(@"\.partsbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("nvmhktbnd", new Regex(@"\.nvmhktbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("mapbnd", new Regex(@"\.mapbnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        // cutscenes and rumble (camera) animations, which the Havok Editor also opens
        ("cutscenebnd", new Regex(@"\.(cutscenebnd|remobnd)(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("rumblebnd", new Regex(@"\.rumblebnd(\.dcx)?$", RegexOptions.IgnoreCase)),
        ("loose hkx", new Regex(@"\.(hkx|hkt)(\.dcx)?$", RegexOptions.IgnoreCase)),
    ];

    private static readonly Regex AnyContainer = new(
        @"\.(hkxbhd|anibnd|behbnd|chrbnd|objbnd|geomhkxbnd|partsbnd|nvmhktbnd|mapbnd|cutscenebnd|remobnd|rumblebnd|hkx|hkt)(\.dcx)?$", RegexOptions.IgnoreCase);

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
            if (TypeDiffEnabled) TypeDiff.Collect(bytes, sniff, compFile.Name, BuildSdkPrefix);
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
        if (r.Sniff.Container == "Tagfile") r.Layout = SectionDiff.Layout(bytes);
        if (TypeDiffEnabled) TypeDiff.Collect(bytes, r.Sniff, inner, BuildSdkPrefix);
        if (DumpClasses) ClassDump.Collect(bytes, r.Sniff, inner);
        DecodeCompare.Collect(bytes, r.Sniff, ctx.Game, inner);

        foreach (var lib in Libraries.Where(l => l.Applies(r.Sniff, ctx.Game)))
            r.Libs.Add(lib.Run(bytes, ctx, r.Sniff.UsesCompendium));

        // Unified engine vs the HKLib serializer it replaces: the same objects read, the same bytes written
        foreach (var (unified, lib) in r.Libs.Join(Libraries.OfType<UnifiedTagfile>(), l => l.Lib, u => u.Name, (l, u) => (l, u)))
        {
            unified.ParityWith = lib.ParityWith;
            if (unified.Written != null && !unified.ByteIdentical)
                unified.SectionDiff = SectionDiff.Describe(bytes, unified.Written);
            var hklib = r.Libs.FirstOrDefault(l => l.Lib == lib.ParityWith);
            if (hklib == null || !hklib.Read || !unified.Read) continue;
            unified.ParityCompared = true;
            try { unified.ParityGraphDiff = lib.CrossClassSet ? GraphCompare.CompareAcross(hklib.Root, unified.Root) : GraphCompare.Compare(hklib.Root, unified.Root); }
            catch (Exception e) { unified.ParityGraphDiff = "compare threw: " + Library.Describe(e); }
            if (hklib.Written != null && unified.Written != null)
                unified.ParityBytesEqual = hklib.Written.AsSpan().SequenceEqual(unified.Written);
        }

        // Unified packfile engine: where a write isn't byte-identical (compared with HKX2's reader and writer until Phase 7)
        foreach (var packfile in r.Libs.Where(l => l.Lib.StartsWith("Generated 2014 packfile")))
        {
            if (packfile.Written != null && !packfile.ByteIdentical)
                packfile.SectionDiff = "packfile: first diff at 0x" + packfile.FirstByteDiff.ToString("X");
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
