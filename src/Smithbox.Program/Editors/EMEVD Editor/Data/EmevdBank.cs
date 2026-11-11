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

    /// <summary>DS2 and SotFS keep their EMEVD files (the SpEffect scripts) inside the regulation binder</summary>
    public const string RegulationPath = "enc_regulation.bnd.dcx";

    public bool InRegulation => Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;

    // The regulation's .emevd entries as read at setup, by entry name: each file reads its own on first use
    private Dictionary<string, byte[]> RegulationEntries = new(StringComparer.OrdinalIgnoreCase);

    public bool Setup()
    {
        if (InRegulation)
            return SetupRegulation();

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

    /// <summary>
    /// DS2 / SotFS: one file per .emevd entry of the regulation. A file counts as in the project when the project's
    /// regulation holds different bytes for it than the game's (the Param Editor writes the regulation too, so the
    /// project having one says nothing about its events).
    /// </summary>
    private bool SetupRegulation()
    {
        var current = ReadRegulation(TargetFS);

        if (current == null)
        {
            Files = new();
            ByName = new(StringComparer.OrdinalIgnoreCase);
            return false;
        }

        var vanilla = Project.VFS.ProjectFS?.FileExists(RegulationPath) == true ? ReadRegulation(Project.VFS.VanillaFS) : null;
        var vanillaEntries = vanilla?.Files
            .Where(f => f.Name.EndsWith(".emevd", StringComparison.OrdinalIgnoreCase))
            .GroupBy(f => System.IO.Path.GetFileName(f.Name), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Bytes, StringComparer.OrdinalIgnoreCase);

        var files = new List<EmevdFileWrapper>();
        RegulationEntries = new(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in current.Files.Where(f => f.Name.EndsWith(".emevd", StringComparison.OrdinalIgnoreCase)))
        {
            var entryName = System.IO.Path.GetFileName(entry.Name);
            if (!RegulationEntries.TryAdd(entryName, entry.Bytes.ToArray()))
                continue;

            var inProject = vanillaEntries != null
                && (!vanillaEntries.TryGetValue(entryName, out var original) || !original.Span.SequenceEqual(entry.Bytes.Span));

            var name = System.IO.Path.GetFileNameWithoutExtension(entryName);
            files.Add(new EmevdFileWrapper(this, $"{RegulationPath}/{entryName}", name, inProject, null, entryName));
        }

        current.Dispose();
        vanilla?.Dispose();

        Files = files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
        ByName = Files.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

        return true;
    }

    /// <summary>The regulation binder from a file system, decrypted if it's the game's encrypted copy; null if none or unreadable</summary>
    private BND4 ReadRegulation(VirtualFileSystem fs)
    {
        try
        {
            var data = fs?.ReadFile(RegulationPath);

            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Find_File", RegulationPath));
                return null;
            }

            var bytes = data.Value.ToArray();
            return BND4.Is(bytes) ? BND4.Read(bytes) : SFUtil.DecryptDS2Regulation(bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Read_File", RegulationPath), e);
            return null;
        }
    }

    /// <summary>A regulation .emevd entry's bytes as read at setup or last saved; null if the regulation has none</summary>
    public byte[] ReadRegulationEntry(string entryName)
    {
        return entryName != null && RegulationEntries.TryGetValue(entryName, out var data) ? data : null;
    }

    /// <summary>
    /// Writes the given files into the project's regulation. The regulation is read again from disk so the Param
    /// Editor's saved params are kept, and only these .emevd entries are replaced. The game's encrypted copy is
    /// written decrypted, with a .bak of it, as the Param Editor does.
    /// </summary>
    public bool SaveRegulation(IReadOnlyCollection<EmevdFileWrapper> files)
    {
        var toSave = files.Where(f => f.Emevd != null && f.EntryName != null).ToList();
        if (toSave.Count == 0)
            return true;

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);

        BND4 bnd;
        try
        {
            var data = fs.ReadFile(RegulationPath);
            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Find_File", RegulationPath));
                return false;
            }

            var bytes = data.Value.ToArray();

            if (BND4.Is(bytes))
            {
                bnd = BND4.Read(bytes);
            }
            else
            {
                bnd = SFUtil.DecryptDS2Regulation(bytes);

                if (!toFs.FileExists($"{RegulationPath}.bak") && CFG.Current.Project_Enable_Backup_Saves)
                    toFs.WriteFile($"{RegulationPath}.bak", bytes);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Read_File", RegulationPath), e);
            return false;
        }

        var written = new List<(EmevdFileWrapper File, byte[] Data)>();

        try
        {
            foreach (var file in toSave)
            {
                var entry = bnd.Files.Find(f => System.IO.Path.GetFileName(f.Name).Equals(file.EntryName, StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Find_File", file.Path));
                    continue;
                }

                var emevdData = file.Emevd.Write();
                entry.Bytes = emevdData;
                written.Add((file, emevdData));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Write_File", RegulationPath), e);
            bnd.Dispose();
            return false;
        }

        var ok = ProjectUtils.WriteWithBackup(Project, fs, toFs, RegulationPath, bnd);
        bnd.Dispose();

        if (!ok)
            return false;

        foreach (var (file, emevdData) in written)
        {
            RegulationEntries[file.EntryName] = emevdData;
            file.MarkSaved();
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", file.Path));
        }

        return written.Count == toSave.Count;
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
        // DS2 / SotFS: every changed file goes into one regulation write
        if (InRegulation)
            return SaveRegulation(Files.Where(f => f.IsDirty).ToList());

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
