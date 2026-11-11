using Andre.IO.VFS;
using StudioCore.Application;
using System.Collections.Concurrent;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Every behbnd of the project (chr/cXXXX.behbnd.dcx), with its .bhv files read
/// </summary>
public class BhvBank
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    private readonly ConcurrentDictionary<FileDictionaryEntry, BhvBinderWrapper> Loaded = new();

    /// <summary>
    /// The behbnds that loaded, sorted by name
    /// </summary>
    public List<BhvBinderWrapper> Binders { get; private set; } = new();

    public BhvBank(ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        // The Havok Editor's locator already finds the behbnds; in AC6 they hold .bhv files instead of Havok graphs
        var tasks = Project.Locator.HavokBehaviorFiles.Entries.Select(async entry =>
        {
            var wrapper = new BhvBinderWrapper(Project, entry, TargetFS);
            if (await wrapper.Load())
                Loaded[entry] = wrapper;
        });

        await Task.WhenAll(tasks);

        Binders = Loaded.Values
            .Where(b => b.Files.Count > 0)
            .OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return true;
    }

    public List<string> GetDirtyFiles()
    {
        var result = new List<string>();

        foreach (var binder in Binders)
        {
            foreach (var file in binder.Files.Where(f => f.IsDirty))
                result.Add($"{binder.FilePath} : {file.Key}");
        }

        return result;
    }

    /// <summary>
    /// Saves every behbnd holding a .bhv with unsaved changes. Returns false if any failed.
    /// </summary>
    public bool SaveAllDirty()
    {
        var ok = true;

        foreach (var binder in Binders.Where(b => b.IsDirty()))
        {
            if (!binder.SaveDirtyFiles())
                ok = false;
        }

        return ok;
    }
}
