using SoulsFormats;
using StudioCore.Application;

using StudioCore.Utilities;
using System.Text.Json;

namespace StudioCore.Port;

/// <summary>
/// One project's files for the port: its <see cref="ProjectVFS"/> (project folder over the game) and the paths there
/// are, which the game's file dictionary lists plus the files in the project folder. Opened without the editors.
/// </summary>
public sealed class PortGame : IDisposable
{
    public ProjectDescriptor Descriptor { get; }
    public ProjectVFS Vfs { get; }

    /// <summary>Lower-case paths with a leading '/', as the file dictionary writes them.</summary>
    public HashSet<string> Paths { get; } = new(StringComparer.OrdinalIgnoreCase);

    private PortGame(ProjectDescriptor descriptor)
    {
        Descriptor = descriptor;
        Vfs = new ProjectVFS(new ProjectEntry { Descriptor = descriptor });
    }

    public static PortGame Open(ProjectDescriptor descriptor)
    {
        // KRAK files (ER) need Oodle, which each game ships next to its executable
        OodleSetup.CopyFromGameDirectory(descriptor.DataPath);

        var game = new PortGame(descriptor);
        game.Vfs.Initialize();

        var dictionary = Path.Combine(AppContext.BaseDirectory, "Assets", "File Dictionaries", $"{descriptor.ProjectType}-File-Dictionary.json");
        if (File.Exists(dictionary))
        {
            using var json = JsonDocument.Parse(File.ReadAllText(dictionary));
            foreach (var entry in json.RootElement.GetProperty("Entries").EnumerateArray())
            {
                // The "sd" archives hold the low-resolution copies the game streams in; they aren't ported
                var archive = entry.TryGetProperty("Archive", out var a) ? a.GetString() ?? "" : "";
                if (archive.Contains("sd"))
                    continue;

                var path = entry.GetProperty("Path").GetString();
                if (!string.IsNullOrEmpty(path))
                    game.Paths.Add(path);
            }
        }

        if (Directory.Exists(descriptor.ProjectPath))
        {
            foreach (var file in Directory.EnumerateFiles(descriptor.ProjectPath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(descriptor.ProjectPath, file).Replace('\\', '/');
                if (!relative.StartsWith(".smithbox", StringComparison.OrdinalIgnoreCase))
                    game.Paths.Add("/" + relative.ToLowerInvariant());
            }
        }

        return game;
    }

    public bool Exists(string path) => Vfs.FS.FileExists(path);

    public byte[] Read(string path) => Vfs.FS.FileExists(path) ? Vfs.FS.ReadFile(path)?.ToArray() : null;

    /// <summary>The game's copy, ignoring the project folder.</summary>
    public byte[] ReadVanilla(string path) => Vfs.VanillaFS.FileExists(path) ? Vfs.VanillaFS.ReadFile(path)?.ToArray() : null;

    /// <summary>The project's own copy if it has one, else null (for files the port merges into).</summary>
    public byte[] ReadProjectOnly(string path) => Vfs.ProjectFS.FileExists(path) ? Vfs.ProjectFS.ReadFile(path)?.ToArray() : null;

    /// <summary>Writes into the project folder (never the game folder).</summary>
    public void Write(string path, byte[] bytes)
    {
        Vfs.ProjectFS.WriteFile(path, bytes);
        Paths.Add("/" + path.TrimStart('/').Replace('\\', '/').ToLowerInvariant());
    }

    /// <summary>The project folder's absolute path of a game path.</summary>
    public string ProjectFile(string path) => Path.Combine(Descriptor.ProjectPath, path.TrimStart('/'));

    /// <summary>A map's map piece binders: DS3 map/mAA_BB_00_00/, ER map/mAA/mAA_BB_CC_DD/.</summary>
    public List<string> MapPieces(string mapId)
    {
        var folder = MapFolder(mapId);
        return Paths.Where(p => p.StartsWith(folder, StringComparison.OrdinalIgnoreCase)
            && p.EndsWith(".mapbnd.dcx", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p).ToList();
    }

    public string MapFolder(string mapId) => Descriptor.ProjectType is ProjectType.DS3 ? $"/map/{mapId}/" : $"/map/{mapId[..3]}/{mapId}/";

    /// <summary>The maps with an MSB, from map/mapstudio.</summary>
    public List<string> Maps() => Paths
        .Where(p => p.StartsWith("/map/mapstudio/", StringComparison.OrdinalIgnoreCase) && p.EndsWith(".msb.dcx", StringComparison.OrdinalIgnoreCase))
        .Select(p => Path.GetFileName(p)[..^".msb.dcx".Length])
        .Where(m => System.Text.RegularExpressions.Regex.IsMatch(m, @"^m\d\d_\d\d_\d\d_\d\d$"))
        .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToList();

    /// <summary>The FLVERs of a map piece binder, with the binder's stem.</summary>
    public IEnumerable<(string Stem, FLVER2 Flver, BinderFile File)> Flvers(string path)
    {
        var bytes = Read(path);
        if (bytes == null) yield break;

        foreach (var f in BND4.Read(bytes).Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
            yield return (Path.GetFileNameWithoutExtension(f.Name), FLVER2.Read(f.Bytes), f);
    }

    public void Dispose() => Vfs.Dispose();
}

/// <summary>The lines of a port run, shown live by the window and saved as the report. Thread-safe.</summary>
public sealed class PortLog
{
    private readonly List<string> _lines = new();
    private readonly object _lock = new();

    public int Warnings { get; private set; }
    public int Failures { get; private set; }

    public void Line(string text = "")
    {
        lock (_lock) _lines.Add(text);
    }

    public void Warn(string text)
    {
        lock (_lock) { _lines.Add("  !! " + text); Warnings++; }
    }

    public void Fail(string text)
    {
        lock (_lock) { _lines.Add("  FAIL " + text); Failures++; }
    }

    /// <summary>A check: logged only when it fails.</summary>
    public bool Check(bool ok, string what)
    {
        if (!ok) Fail(what);
        return ok;
    }

    public List<string> Snapshot()
    {
        lock (_lock) return _lines.ToList();
    }
}
