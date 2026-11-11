using Andre.IO.VFS;
using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The project's event folder: every .emevd file, read on first use
/// </summary>
public class EmevdBank
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    /// <summary>common, common_func, then the map files by name</summary>
    public List<EmevdFileWrapper> Files { get; private set; } = new();

    private Dictionary<string, EmevdFileWrapper> ByName = new(StringComparer.OrdinalIgnoreCase);

    public EmevdBank(ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        TargetFS = targetFs;
    }

    public bool Setup()
    {
        var entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Extension == "emevd" && e.Folder.StartsWith("/event", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var emelds = Project.Locator.FileDictionary.Entries
            .Where(e => e.Extension == "emeld" && e.Folder.StartsWith("/event", StringComparison.OrdinalIgnoreCase))
            .GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Path, StringComparer.OrdinalIgnoreCase);

        var files = new List<EmevdFileWrapper>();

        // A dictionary can list both an uncompressed copy and a .dcx (DS1R): keep one per name, the .dcx
        foreach (var group in entries.GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase))
        {
            var entry = group.OrderByDescending(e => e.Path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase)).First();
            var inProject = Project.VFS.ProjectFS?.FileExists(entry.Path) ?? false;

            emelds.TryGetValue(entry.Filename, out var emeld);

            files.Add(new EmevdFileWrapper(this, entry.Path, entry.Filename, inProject, emeld));
        }

        Files = files
            .OrderBy(f => SortGroup(f.Name))
            .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ByName = Files.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

        return true;
    }

    private static int SortGroup(string name)
    {
        if (name.Equals("common", StringComparison.OrdinalIgnoreCase))
            return 0;
        if (name.Equals("common_func", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (name.StartsWith("common", StringComparison.OrdinalIgnoreCase))
            return 2;
        return 3;
    }

    public List<string> GetDirtyFiles()
    {
        return Files.Where(f => f.IsDirty).Select(f => f.Path).ToList();
    }

    /// <summary>Saves every file with unsaved changes. Returns false if any failed.</summary>
    public bool SaveAllDirty()
    {
        var ok = true;

        foreach (var file in Files.Where(f => f.IsDirty))
        {
            if (!file.Save())
                ok = false;
        }

        return ok;
    }

    public EmevdFileWrapper FindFile(string name)
    {
        return name != null && ByName.TryGetValue(name, out var f) ? f : null;
    }

    /// <summary>
    /// The event an Initialize Event call starts: the calling file's own event first, then common_func's and
    /// common's (the games' shared events), reading those files if needed.
    /// </summary>
    public (EmevdFileWrapper file, EMEVD.Event ev) ResolveEvent(EmevdFileWrapper from, long id)
    {
        var own = from?.FindEvent(id);
        if (own != null)
            return (from, own);

        foreach (var name in new[] { "common_func", "common" })
        {
            var file = FindFile(name);
            if (file == null || file == from || !file.EnsureLoaded())
                continue;

            var ev = file.FindEvent(id);
            if (ev != null)
                return (file, ev);
        }

        return (null, null);
    }
}
