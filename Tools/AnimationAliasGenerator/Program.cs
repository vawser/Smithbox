using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

// Probe / generate animation aliases from character behaviour files.
//   AnimationAliasGenerator probe <GAME> [limit]
//   AnimationAliasGenerator generate <GAME> <outFile>
// Must run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.

var gamePaths = File.ReadAllLines(args.Length > 3 ? args[3] : Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var mode = args[0];
var gameName = args[1];
var game = Enum.Parse<Game>(gameName);
var root = gamePaths[gameName];

using var fs = BuildFS(game, root);

if (mode == "probe")
{
    var limit = args.Length > 2 ? int.Parse(args[2]) : 3;
    Probe(fs, game, limit);
}
else if (mode == "list")
{
    foreach (var f in fs.GetFileNamesMatching("/chr", args[2]).Take(50))
        Console.WriteLine(f);
}
else if (mode == "binder")
{
    // binder <GAME> <path>: lists the files of a binder, for games whose behaviours aren't in a behbnd
    var raw = fs.ReadFile(args[2]).Value.ToArray();
    var data = DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw;
    Console.WriteLine($"  magic: {System.Text.Encoding.ASCII.GetString(data, 0, 4)}");
    IBinder bnd = BND4.Is(data) ? BND4.Read(data) : BND3.Read(data);
    foreach (var f in bnd.Files.Where(f => !f.Name.Contains("\\a", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("beh", StringComparison.OrdinalIgnoreCase)).Take(40))
        Console.WriteLine($"  [{f.ID}] {f.Name} ({f.Bytes.Length})");
}
else if (mode == "generate")
{
    // generate <GAME> <outFile> [unmatched]
    var stats = new AliasBuilder.Stats();
    var includeUnmatched = args.Length > 3 && args[3] == "unmatched";

    var aliases = AliasBuilder.Build(fs, game, stats, includeUnmatched);

    AliasBuilder.Write(aliases, args[2]);

    Console.WriteLine($"RESULT {gameName}: binders={stats.Binders} characters={stats.Characters} aliases={stats.Aliases} " +
        $"unmatched={stats.Unmatched} notInTimeActs={stats.NotInTimeActs} timeActAnimations={stats.TimeActAnimations} " +
        $"noTimeActs={stats.NoTimeActs.Count} ({string.Join(",", stats.NoTimeActs.Take(12))}) errors={stats.Errors.Count}");

    foreach (var error in stats.Errors.Take(20))
        Console.WriteLine($"RESULT   ERROR {error}");
}
else if (mode == "packprobe")
{
    // packprobe <GAME> <binder> : shows the layout of the selectors and clips in a packfile behaviour
    var raw = fs.ReadFile(args[2]).Value.ToArray();
    var bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);

    foreach (var file in bnd.Files.Where(f => f.Name.Contains("Behaviors", StringComparison.OrdinalIgnoreCase)).Take(1))
    {
        var reader = new PackfileReader(file.Bytes.ToArray());

        Console.WriteLine($"{file.Name}: {reader.Objects.Count} objects");

        foreach (var g in reader.Objects.GroupBy(o => o.ClassName).OrderByDescending(g => g.Count()).Take(25))
            Console.WriteLine($"  {g.Count(),6} {g.Key}");

        foreach (var className in new[] { "CustomManualSelectorGenerator", "hkbClipGenerator", "hkbManualSelectorGenerator" })
        {
            var shown = 0;

            for (var i = 0; i < reader.Objects.Count && shown < 3; i++)
            {
                if (reader.Objects[i].ClassName != className)
                    continue;

                shown++;

                var (offset, _) = reader.Objects[i];
                var size = reader.SizeOf(i);

                Console.WriteLine($"-- {className} @0x{offset:X} size 0x{size:X}");

                foreach (var (fieldOffset, value) in reader.StringFields(offset, size))
                    Console.WriteLine($"   +0x{fieldOffset:X2} \"{value}\"");

                for (uint o = 0x40; o + 16 <= size; o += 8)
                {
                    var arr = reader.ReadPointerArray(offset + o);
                    if (arr.Count > 0)
                        Console.WriteLine($"   +0x{o:X2} array of {arr.Count}: {string.Join(", ", arr.Take(3).Select(a => reader.Objects.FirstOrDefault(x => x.Offset == a).ClassName))}");
                }

                var ints = Enumerable.Range(0, (int)(size - 0x40) / 4)
                    .Select(k => (Offset: 0x40 + k * 4, Value: reader.ReadInt32(offset + 0x40 + (uint)k * 4)))
                    .Where(e => e.Value != 0)
                    .Take(24);

                Console.WriteLine($"   non-zero ints: {string.Join(" ", ints.Select(e => $"+0x{e.Offset:X}={e.Value}"))}");
            }
        }
    }
}
else if (mode == "scan")
{
    // scan <GAME> <character filter regex>
    var filter = new Regex(args[2], RegexOptions.IgnoreCase);

    foreach (var path in BehaviorReader.FindBehaviorBinders(fs, game).Where(p => filter.IsMatch(p)))
    {
        var result = BehaviorReader.ReadBinder(fs, game, path);

        Console.WriteLine($"== {path}: {result.Selectors.Count} selectors, {result.Errors.Count} errors");

        foreach (var error in result.Errors)
            Console.WriteLine($"  ERROR {error}");

        foreach (var s in result.Selectors.Take(25))
            Console.WriteLine($"  {s.SelectorName} [{s.AnimID}] <- {string.Join(", ", s.Clips.Take(6))}");
    }
}

static VirtualFileSystem BuildFS(Game game, string root)
{
    // The Bloodborne repository keeps the game files a few folders down
    var bbRoot = Path.Combine(root, "Loose", "dvdroot_ps4");

    if (game == Game.BB && Directory.Exists(bbRoot))
        root = bbRoot;

    var real = new RealVirtualFileSystem(root, false);

    if (game is Game.BB or Game.DES or Game.DB_NT or Game.DB)
        return real;

    var binder = ArchiveBinderVirtualFileSystem.FromGameFolder(root, game);
    return new CompundVirtualFileSystem([binder, real]);
}

static void Probe(VirtualFileSystem fs, Game game, int limit)
{
    var behbnds = fs.GetFileNamesMatching("/chr", @"\.behbnd(\.dcx)?$").OrderBy(e => e).ToList();
    Console.WriteLine($"behbnd count: {behbnds.Count}");

    foreach (var path in behbnds.Take(limit))
    {
        Console.WriteLine($"== {path}");
        var data = fs.ReadFile(path);
        if (data == null) { Console.WriteLine("  (unreadable)"); continue; }

        var bnd = BND4.Read(data.Value);
        foreach (var f in bnd.Files)
            Console.WriteLine($"  [{f.ID}] {f.Name} ({f.Bytes.Length})");
    }
}
