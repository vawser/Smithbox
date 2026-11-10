using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Text;
using System.Text.Json;

// MaterialEditorSurvey <files|dupes|roundtrip|keys|ids|all> [GAME ...]
//
// Material Editor Plan research, run with the working directory at src/Smithbox.Data:
//   files      R4: the material binders in each game's file dictionary, which the editor's locator rule picks up,
//              which it misses, the binder format against what the editor assumes, unreadable files
//   dupes      R2: material short names held by more than one binder, and whether the copies differ
//   roundtrip  R3: binder, MTD and MATBIN read -> write -> byte compare; MATBIN Float3 trailing floats
//   keys       R1: MATBIN.Key, Param.Key and Sampler.Key against candidate hashes and other rules
//   ids        R3: binder file ID / name / flag patterns, for adding files to a binder
// Games default to every game in game_data.txt.

var modes = args.Length > 0 ? args[0] : "all";
var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "Claude", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var games = args.Length > 1 ? args[1..] : gamePaths.Keys.ToArray();

foreach (var gameName in games)
{
    Console.WriteLine();
    Console.WriteLine($"################ {gameName} ################");

    GameMaterials game;
    try
    {
        game = GameMaterials.Load(gameName, gamePaths[gameName]);
    }
    catch (Exception e)
    {
        Console.WriteLine($"  load failed: {e.GetType().Name}: {e.Message}");
        continue;
    }

    using (game)
    {
        if (modes is "files" or "all") Surveys.Files(game);
        if (modes is "dupes" or "all") Surveys.Dupes(game);
        if (modes is "roundtrip" or "all") Surveys.RoundTrip(game);
        if (modes is "keys" or "all") Surveys.Keys(game);
        if (modes is "ids" or "all") Surveys.Ids(game);
        if (modes is "inspect") Inspect.Run(game, Environment.GetEnvironmentVariable("MATSURVEY_FILE") ?? "");
        if (modes is "keydump") Inspect.KeyDump(game);
        if (modes is "check") CheckSurvey.Run(game);
        if (modes is "rebuild") RebuildSurvey.Run(game);
        if (modes is "shaders") ShaderSurvey.Run(game);
    }
}

public enum MaterialKind { MTD, MATBIN }

public record DictEntry(string Archive, string Path, string Folder, string Filename, string Extension);

public class MaterialBinder
{
    public string Path;
    public MaterialKind Kind;
    public bool Selected;          // picked up by the editor's locator rule
    public byte[] Raw;             // as stored (maybe DCX)
    public byte[] Data;            // decompressed
    public IBinder Binder;
    public string Format => Data == null ? "missing" : Encoding.ASCII.GetString(Data, 0, 4).TrimEnd('\0');
}

public class GameMaterials : IDisposable
{
    public string Name;
    public Game Game;
    public VirtualFileSystem Fs;
    public List<MaterialBinder> Binders = new();

    // The editor's locator rule (ProjectFileLocator.ShouldAddToMtdFiles and the matbinbnd check), minus sd archives
    public static bool EditorPicksMtd(DictEntry e, string game)
    {
        if (game is "DS2" or "DS2S")
            return e.Extension == "bnd" && e.Folder.StartsWith("/material") && e.Filename == "allmaterialbnd";

        return e.Extension == "mtdbnd" && e.Folder.StartsWith("/mtd");
    }

    public static bool EditorPicksMatbin(DictEntry e) => e.Folder.StartsWith("/material") && e.Extension == "matbinbnd";

    public static GameMaterials Load(string name, string root)
    {
        var game = Enum.Parse<Game>(name);
        var result = new GameMaterials { Name = name, Game = game };

        List<VirtualFileSystem> layers = [];
        try { layers.Add(ArchiveBinderVirtualFileSystem.FromGameFolder(root, game)); }
        catch (Exception e) { Console.WriteLine($"  no archives ({e.GetType().Name}), loose files only"); }
        layers.Add(new RealVirtualFileSystem(root, false));
        result.Fs = new CompundVirtualFileSystem(layers);

        var dictName = name == "DB_NT" ? "DB-NT" : name;
        var json = JsonDocument.Parse(File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{dictName}-File-Dictionary.json")));

        foreach (var el in json.RootElement.GetProperty("Entries").EnumerateArray())
        {
            var e = new DictEntry(el.GetProperty("Archive").GetString() ?? "", el.GetProperty("Path").GetString(),
                el.GetProperty("Folder").GetString() ?? "", el.GetProperty("Filename").GetString() ?? "",
                el.GetProperty("Extension").GetString() ?? "");

            var lower = e.Path.ToLowerInvariant();
            var looksMtd = lower.Contains("mtdbnd") || (name is "DS2" or "DS2S" && lower.Contains("allmaterialbnd"));
            var looksMatbin = lower.Contains("matbinbnd");

            if (!looksMtd && !looksMatbin)
                continue;

            var isSd = e.Archive.Contains("sd");
            var kind = looksMatbin ? MaterialKind.MATBIN : MaterialKind.MTD;
            var selected = !isSd && (kind == MaterialKind.MTD ? EditorPicksMtd(e, name) : EditorPicksMatbin(e));

            var binder = new MaterialBinder { Path = e.Path, Kind = kind, Selected = selected };

            var file = result.Fs.FileExists(e.Path) ? result.Fs.ReadFile(e.Path) : null;
            if (file != null)
            {
                binder.Raw = file.Value.ToArray();
                binder.Data = DCX.Is(binder.Raw) ? DCX.Decompress(binder.Raw).ToArray() : binder.Raw;

                try
                {
                    binder.Binder = binder.Format == "BND3" ? BND3.Read(binder.Data) : BND4.Read(binder.Data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  binder read failed {e.Path}: {ex.Message}");
                }
            }

            result.Binders.Add(binder);
        }

        // Game load order: base before devpatch, so later binders override earlier ones
        result.Binders = result.Binders.OrderBy(b => b.Path.Contains("devpatch") ? 1 : 0)
            .ThenBy(b => b.Path, StringComparer.Ordinal).ToList();

        return result;
    }

    /// <summary>
    /// The material files of a binder, decompressed if the inner file is compressed
    /// </summary>
    public static IEnumerable<(BinderFile file, byte[] data, bool innerDcx)> Files(MaterialBinder b)
    {
        if (b.Binder == null)
            yield break;

        var ext = b.Kind == MaterialKind.MTD ? ".mtd" : ".matbin";

        foreach (var f in b.Binder.Files)
        {
            if (!f.Name.Contains(ext, StringComparison.OrdinalIgnoreCase))
                continue;

            var raw = f.Bytes.ToArray();
            var dcx = DCX.Is(raw);
            yield return (f, dcx ? DCX.Decompress(raw).ToArray() : raw, dcx);
        }
    }

    public static string ShortName(string fileName) =>
        Path.GetFileNameWithoutExtension(fileName.Replace('\\', '/').Split('/').Last());

    public void Dispose() => (Fs as IDisposable)?.Dispose();
}
