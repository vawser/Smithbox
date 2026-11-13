using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.SoundEditor;

public enum SoundMiddleware
{
    Fmod,
    Wwise,
}

/// <summary>
/// Holds the data for the Sound Editor: every sound bank the project's file dictionary lists, and an index of the
/// streamed .wem files. Data Flow: list only at setup (ER has ~600 banks and ~10k wems); a bank is read when selected.
/// </summary>
public class SoundData : IDisposable
{
    public ProjectEntry Project;

    public SoundMiddleware Middleware;

    /// <summary>File aliases, Sounds aliases and the Wwise name table.</summary>
    public SoundNames Names;

    /// <summary>Sound ID / name → bank, without opening every bank (built on first use).</summary>
    public SoundIndex Index;

    private readonly HashSet<string> _allPaths = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The path of a listed file whose name ends with <paramref name="fileName"/>, or null.</summary>
    public string FindPath(string fileName) => _allPaths.FirstOrDefault(p => p.EndsWith("/" + fileName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Every container, sorted by group then name.</summary>
    public List<SoundContainer> Containers = new();

    /// <summary>Wwise language folders at the data root (AC6 /enus, /ja; ER/NR /enus/wem)</summary>
    public static readonly HashSet<string> LanguageFolders = new(StringComparer.OrdinalIgnoreCase) { "enus", "ja", "eus", "jpjp", "frfr", "dede" };

    // Wwise media ID -> streamed .wem paths (one per language folder)
    private readonly Dictionary<uint, List<string>> _wemPaths = new();

    public int StreamedWemCount => _wemPaths.Count;

    public SoundData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        var type = Project.Descriptor.ProjectType;
        Middleware = EditorConditions.UsesWwise(type) ? SoundMiddleware.Wwise : SoundMiddleware.Fmod;

        Names = new SoundNames(Project);
        if (Middleware == SoundMiddleware.Wwise)
            Names.LoadWwise();
        Index = new SoundIndex(this);
        FevCatalog = new SoundFevCatalog(this);

        foreach (var e in Project.Locator.FileDictionary.Entries)
            _allPaths.Add(e.Path);

        // which sound files come from the sd archives (ER sd/sd_dlc02, NR sd/sd_dlc01, AC6 sd)
        foreach (var e in Project.Locator.FileDictionary.Entries.Concat(Project.Locator.UnvalidatedFileDictionary.Entries))
            if (e.Archive != null && e.Archive.StartsWith("sd", StringComparison.OrdinalIgnoreCase) && IsBankOrWem(e.Path))
                _sdPaths.Add(e.Path);

        Edits = new SoundEdits(this);
        MediaMap = new SoundMediaMap(this);

        // The locator's dictionary holds only validated entries. That is right for AC6 (228 listed banks aren't in
        // the game), but every ER sd/sd_dlc02 entry is marked unvalidated although the files exist. So unvalidated
        // sound entries are taken too, when the VFS has the file.
        var fs = Project.VFS.FS;
        var paths = new HashSet<string>(Project.Locator.FileDictionary.Entries.Select(e => e.Path), StringComparer.OrdinalIgnoreCase);
        int recovered = 0;
        foreach (var entry in Project.Locator.UnvalidatedFileDictionary.Entries)
        {
            if (!IsSoundPath(entry.Path) || paths.Contains(entry.Path))
                continue;
            if (fs.FileExists(entry.Path) && paths.Add(entry.Path))
                recovered++;
        }

        var list = new List<SoundContainer>();

        foreach (var path in paths)
        {
            if (Middleware == SoundMiddleware.Wwise)
            {
                if (path.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase))
                    list.Add(SoundContainer.Create(this, path, SoundContainerKind.WwiseBank));
                else if (path.EndsWith(".wem", StringComparison.OrdinalIgnoreCase) &&
                         uint.TryParse(System.IO.Path.GetFileNameWithoutExtension(path), out var id))
                    (_wemPaths.TryGetValue(id, out var l) ? l : _wemPaths[id] = new()).Add(path);
            }
            else
            {
                if (path.EndsWith(".fsb", StringComparison.OrdinalIgnoreCase))
                    list.Add(SoundContainer.Create(this, path, SoundContainerKind.Fsb));
                else if (path.StartsWith("/sound/", StringComparison.OrdinalIgnoreCase) &&
                         (path.EndsWith(".bnd", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".bnd.dcx", StringComparison.OrdinalIgnoreCase)))
                    list.Add(SoundContainer.Create(this, path, SoundContainerKind.FsbBinder));
            }
        }

        Containers = list
            .OrderBy(c => c.Group, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Language, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Smithbox.Log(this, LOC.Get("SOUND_Data_Setup_PASS", Containers.Count, _wemPaths.Count));
        if (recovered > 0)
            Smithbox.Log(this, LOC.Get("SOUND_Data_Setup_Recovered", recovered));
        return true;
    }

    private bool IsSoundPath(string path)
    {
        if (Middleware == SoundMiddleware.Wwise)
            return path.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".wem", StringComparison.OrdinalIgnoreCase);

        return path.EndsWith(".fsb", StringComparison.OrdinalIgnoreCase)
            || (path.StartsWith("/sound/", StringComparison.OrdinalIgnoreCase) && (path.EndsWith(".bnd", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".bnd.dcx", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>FMOD: event projects shared by several banks (built on first use).</summary>
    public SoundFevCatalog FevCatalog;

    /// <summary>FMOD: loose .fev files and binders of projects (DS2S frpg2.fevbnd.dcx).</summary>
    public IEnumerable<string> FmodProjectPaths()
        => _allPaths.Where(p => p.EndsWith(".fev", StringComparison.OrdinalIgnoreCase) ||
                                p.EndsWith(".fevbnd.dcx", StringComparison.OrdinalIgnoreCase) ||
                                p.EndsWith(".fevbnd", StringComparison.OrdinalIgnoreCase));

    /// <summary>Replaced sounds not yet saved (write plan W2).</summary>
    public SoundEdits Edits;

    /// <summary>Wwise: which banks hold or reference each media ID (built on first use).</summary>
    public SoundMediaMap MediaMap;

    // VFS paths of .bnk/.wem files from the sd archives
    private readonly HashSet<string> _sdPaths = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsBankOrWem(string path)
        => path.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".wem", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Where a sound file lives in the project: the VFS path, except for files of the sd archives, whose VFS paths
    /// have no folder (/cs_c2010.bnk) while the game reads loose ones from sd/ (write plan W0, D4): /sd/cs_c2010.bnk.
    /// </summary>
    public string ProjectPath(string path) => _sdPaths.Contains(path) ? "/sd" + path : path;

    /// <summary>Does the project folder hold its own copy of this file?</summary>
    public bool IsInProject(string path)
    {
        try { return Project.VFS.ProjectFS.FileExists(ProjectPath(path)); }
        catch { return false; }
    }

    /// <summary>Writes a sound file to the project (at <see cref="ProjectPath"/>).</summary>
    public void WriteProjectFile(string path, byte[] bytes) => Project.VFS.ProjectFS.WriteFile(ProjectPath(path), bytes);

    /// <summary>
    /// The bytes of a file: the project's copy first (for sd files, at /sd/…, which the VFS path doesn't reach), then the
    /// project's VFS. Null if missing.
    /// </summary>
    public byte[] ReadFile(string path)
    {
        var mapped = ProjectPath(path);
        if (mapped != path)
        {
            var pfs = Project.VFS.ProjectFS;
            if (pfs.FileExists(mapped))
                return pfs.ReadFile(mapped)?.ToArray();
        }

        var fs = Project.VFS.FS;
        if (!fs.FileExists(path))
            return null;

        return fs.ReadFile(path)?.ToArray();
    }

    /// <summary>The game's own bytes of a file (no project copy), or null.</summary>
    public byte[] ReadVanillaFile(string path)
    {
        var fs = Project.VFS.VanillaFS;
        if (!fs.FileExists(path))
            return null;
        return fs.ReadFile(path)?.ToArray();
    }

    /// <summary>The streamed .wem paths of a media ID (one per language folder), or none.</summary>
    public IReadOnlyList<string> StreamedWems(uint mediaId)
        => _wemPaths.TryGetValue(mediaId, out var l) ? l : Array.Empty<string>();

    /// <summary>
    /// The streamed .wem of a media ID: the one in the bank's language folder if there is one, else the SFX one,
    /// else any. Null if the dictionary has none.
    /// </summary>
    public string FindStreamedWem(uint mediaId, string language)
    {
        if (!_wemPaths.TryGetValue(mediaId, out var paths))
            return null;

        if (!string.IsNullOrEmpty(language))
        {
            var match = paths.FirstOrDefault(p => p.Contains($"/{language}/", StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }

        return paths.FirstOrDefault(p => !LanguageFolders.Any(l => p.Contains($"/{l}/", StringComparison.OrdinalIgnoreCase)))
            ?? paths[0];
    }

    public void Dispose()
    {
        Containers = new();
        _wemPaths.Clear();
        VgmstreamCli.ClearTempFiles();
    }
}
