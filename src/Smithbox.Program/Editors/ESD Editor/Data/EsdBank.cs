using Andre.IO.VFS;
using SoulsFormats;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The project's ESD files: talk, chr and menu binders (*.talkesdbnd / *.chresdbnd / *.menuesdbnd) and loose .esd
/// files (chr c0000/enemyCommon, DS2's /ezstate folder). Containers are listed at setup and read on first use.
/// </summary>
public class EsdBank
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public List<EsdContainer> Containers { get; private set; } = new();

    private Dictionary<string, EsdContainer> ByName = new(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] Extensions = { "esd", "talkesdbnd", "chresdbnd", "menuesdbnd" };

    public EsdBank(ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        TargetFS = targetFs;
    }

    public bool Setup()
    {
        var entries = Project.Locator.FileDictionary.Entries
            .Where(e => Extensions.Contains(e.Extension, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // DS2 / SotFS: names of machines and states, shipped beside the event ESDs
        var edds = Project.Locator.FileDictionary.Entries
            .Where(e => string.Equals(e.Extension, "edd", StringComparison.OrdinalIgnoreCase))
            .GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Path, StringComparer.OrdinalIgnoreCase);

        var containers = new List<EsdContainer>();

        // A dictionary can list both an uncompressed copy and a .dcx: keep one per file, the .dcx
        foreach (var group in entries.GroupBy(e => $"{e.Folder}/{e.Filename}.{e.Extension}", StringComparer.OrdinalIgnoreCase))
        {
            var entry = group.OrderByDescending(e => e.Path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase)).First();
            var inProject = Project.VFS.ProjectFS?.FileExists(entry.Path) ?? false;
            var isBinder = !entry.Extension.Equals("esd", StringComparison.OrdinalIgnoreCase);

            edds.TryGetValue(entry.Filename, out var edd);

            var name = isBinder ? $"{entry.Filename}.{entry.Extension}" : entry.Filename;
            containers.Add(new EsdContainer(this, entry.Path, name, isBinder, KindOf(entry.Path), inProject, edd));
        }

        Containers = containers
            .OrderBy(c => c.Kind)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ByName = new(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Containers)
            ByName.TryAdd(c.Name, c);

        return true;
    }

    /// <summary>The script kind of a file, from its path</summary>
    public static EsdScriptKind KindOf(string path)
    {
        var p = path.ToLowerInvariant();
        if (p.Contains("talkesdbnd"))
            return EsdScriptKind.Talk;
        if (p.Contains("menuesdbnd"))
            return EsdScriptKind.Menu;
        if (p.Contains("/chr/") || p.Contains("chresdbnd"))
            return EsdScriptKind.Chr;
        if (p.Contains("/ezstate/"))
        {
            var f = System.IO.Path.GetFileName(p);
            if (f.StartsWith("ai"))
                return EsdScriptKind.AI;
            if (f.StartsWith("event"))
                return EsdScriptKind.Event;
            if (f.StartsWith("talk"))
                return EsdScriptKind.Talk;
        }
        return EsdScriptKind.Other;
    }

    /// <summary>Project files of deleted containers, moved into the project's backup folder on the next save</summary>
    public List<string> PendingDeletes { get; } = new();

    public List<string> GetDirtyFiles()
    {
        return Containers.Where(c => c.IsDirty || c.PendingRevert).Select(c => c.Path).Concat(PendingDeletes).Distinct().ToList();
    }

    /// <summary>Whether the next save moves project files away (deleted or reverted containers)</summary>
    public bool HasFileRemovals => PendingDeletes.Count > 0 || Containers.Any(c => c.PendingRevert);

    /// <summary>
    /// Saves every file with unsaved changes. Deleted containers' project files, and reverted ones', are moved into
    /// the project's backup folder first (a reverted container then reads the game's file again). Returns false if
    /// any failed.
    /// </summary>
    public bool SaveAllDirty()
    {
        var ok = true;

        foreach (var path in PendingDeletes.ToList())
        {
            if (MoveToBackup(path))
                PendingDeletes.Remove(path);
            else
                ok = false;
        }

        foreach (var c in Containers.Where(c => c.PendingRevert).ToList())
        {
            if (MoveToBackup(c.Path))
                c.ResetToVanilla();
            else
                ok = false;
        }

        foreach (var c in Containers.Where(c => c.IsDirty))
            if (!c.Save())
                ok = false;
        return ok;
    }

    /// <summary>
    /// Moves a project file into &lt;project&gt;/.backup/&lt;time&gt;_deleted/&lt;its path&gt; (the project scan skips dot
    /// folders). Nothing to do when the project has no such file.
    /// </summary>
    private bool MoveToBackup(string path)
    {
        var projectFs = Project.VFS.ProjectFS;
        try
        {
            if (!projectFs.FileExists(path))
                return true;

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_deleted";
            var relative = $".backup/{stamp}{path}";
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.Join(Project.Descriptor.ProjectPath, relative)));
            projectFs.Move(path, relative);
            Smithbox.Log(this, LOC.Get("ESD_Data_Log_Moved_To_Backup", path, relative));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Move_To_Backup_Failed", path), e);
            return false;
        }
    }

    /// <summary>Adds a container at its sorted place (kind, then name); returns the index</summary>
    public int Insert(EsdContainer container, int index = -1)
    {
        if (index < 0)
        {
            index = Containers.FindIndex(c => c.Kind > container.Kind || c.Kind == container.Kind && string.Compare(c.Name, container.Name, StringComparison.OrdinalIgnoreCase) > 0);
            if (index < 0)
                index = Containers.Count;
        }
        Containers.Insert(Math.Clamp(index, 0, Containers.Count), container);
        ByName[container.Name] = container;
        return index;
    }

    /// <summary>Takes a container out of the list; returns where it was</summary>
    public int Remove(EsdContainer container)
    {
        var index = Containers.IndexOf(container);
        if (index >= 0)
            Containers.RemoveAt(index);
        if (ByName.TryGetValue(container.Name, out var c) && c == container)
            ByName.Remove(container.Name);
        return index;
    }

    /// <summary>Whether a path is free for a new file: no container has it, and neither the project nor the game has the file</summary>
    public bool IsFreePath(string path)
    {
        return !Containers.Any(c => c.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) && !PendingDeletes.Contains(path, StringComparer.OrdinalIgnoreCase)
            && !(Project.VFS.FS?.FileExists(path) ?? false);
    }

    public EsdContainer FindContainer(string name)
    {
        return name != null && ByName.TryGetValue(name, out var c) ? c : null;
    }
}

/// <summary>
/// One file holding ESDs: a binder of them, or a loose .esd. Read on first use.
/// </summary>
public class EsdContainer
{
    public EsdBank Bank { get; }

    /// <summary>The VFS path, e.g. /script/talk/m10_00_00_00.talkesdbnd.dcx</summary>
    public string Path { get; }

    /// <summary>e.g. m10_00_00_00.talkesdbnd, or c0000 for a loose file</summary>
    public string Name { get; }

    public bool IsBinder { get; }

    public EsdScriptKind Kind { get; }

    /// <summary>The project folder has its own copy (it overrides the game's)</summary>
    public bool InProject { get; private set; }

    /// <summary>Made in the editor and not saved yet: no file has its path</summary>
    public bool IsNew { get; private set; }

    /// <summary>For a new binder: the binder its header and entry layout are copied from, and its folder rename</summary>
    public string TemplatePath { get; private set; }
    public (string From, string To)? Retarget { get; private set; }

    /// <summary>Deleted, but the game ships it too: the next save moves the project copy away and reads the game's</summary>
    public bool PendingRevert { get; set; }

    /// <summary>Whether the game itself has this file (it can't be removed, only reverted to)</summary>
    public bool InGame => Bank.Project.VFS.VanillaFS?.FileExists(Path) ?? false;

    /// <summary>A container made in the editor (a duplicated binder or loose ESD, a new binder): loaded, unsaved</summary>
    public static EsdContainer CreateNew(EsdBank bank, string path, string name, bool isBinder, EsdScriptKind kind,
        string templatePath, (string, string)? retarget, Func<EsdContainer, List<EsdFileWrapper>> files)
    {
        var c = new EsdContainer(bank, path, name, isBinder, kind, false, null)
        {
            IsNew = true,
            TemplatePath = templatePath,
            Retarget = retarget,
        };
        c.Files = files(c);
        c.IsLoaded = true;
        return c;
    }

    /// <summary>After its project copy is moved away: unloaded, to read the game's file on next use</summary>
    public void ResetToVanilla()
    {
        Files = new();
        IsLoaded = false;
        LoadError = null;
        InProject = false;
        PendingRevert = false;
        IsDirty = false;
        Revision++;
    }

    // Edits done minus edits undone since the last load or save
    private int ChangeBalance;
    private bool ForcedDirty;

    /// <summary>
    /// Whether the file has changed since it was loaded or saved. Setting it true marks it changed with no edit;
    /// setting it false marks the current state as saved.
    /// </summary>
    public bool IsDirty
    {
        get => ChangeBalance != 0 || ForcedDirty;
        set
        {
            ForcedDirty = value;
            if (!value)
                ChangeBalance = 0;
        }
    }

    /// <summary>Counts every edit and undo, so views rebuild what they work out from the ESDs</summary>
    public int Revision { get; private set; }

    public void MarkChanged() { ChangeBalance++; Revision++; }

    public void MarkUndone() { ChangeBalance--; Revision++; }

    /// <summary>
    /// Writes the file into the project folder. A binder is read again (the project's copy if it has one, else the
    /// game's), its edited ESD entries replaced, and written with its own compression; untouched entries keep their
    /// bytes (EsdSurvey bndcheck: every vanilla binder rewrites byte-identical, and ESD.Write is byte-exact). A loose
    /// file is written with the compression it was read with.
    /// </summary>
    public bool Save()
    {
        if (!IsLoaded)
            return false;

        var project = Bank.Project;
        var fs = project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(project);

        // No game type: with one, WriteWithBackup treats a BND4 as an encrypted regulation (DS3, ER, NR, AC6)
        bool ok;

        if (IsBinder)
        {
            IBinder bnd;
            try
            {
                // A new binder starts as a copy of its template's (header, entry layout), its folder renamed
                var source = IsNew ? TemplatePath : Path;
                var data = source != null ? fs.ReadFile(source) : null;
                if (data == null)
                {
                    Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Failed_to_Find_File", source ?? Path));
                    return false;
                }

                var raw = DCX.Is(data.Value) ? DCX.Decompress(data.Value, out _) : data.Value;
                bnd = IsBnd4(raw) ? BND4.Read(data.Value) : BND3.Read(data.Value);

                // The binder's .esd entries become this container's list: kept, replaced, added (duplicated ESDs) and
                // removed (deleted ones), in FromSoftware's layout (EsdBinderEntries; EsdSurvey binderedit). A file
                // that failed to read keeps its entry's bytes.
                var wanted = Files.Where(f => f.EntryName != null).Select(f => (f.EntryName, f.Esd?.Write())).ToList();
                var error = EsdBinderEntries.Apply(bnd, wanted, IsNew ? Retarget : null);
                if (error != null)
                {
                    Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Binder_Rebuild_Failed", error, Path));
                    bnd.Dispose();
                    return false;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Failed_to_Write_File", Path), e);
                return false;
            }

            ok = bnd is BND4 b4
                ? ProjectUtils.WriteWithBackup(project, fs, toFs, Path, b4)
                : ProjectUtils.WriteWithBackup(project, fs, toFs, Path, (BND3)bnd);
        }
        else
        {
            var esd = Files.FirstOrDefault()?.Esd;
            if (esd == null)
                return false;

            // WriteWithBackup disposes binders only; the ESD stays usable
            ok = ProjectUtils.WriteWithBackup(project, fs, toFs, Path, esd);
        }

        if (!ok)
            return false;

        IsDirty = false;
        InProject = true;
        IsNew = false;
        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", Path));
        return true;
    }

    public static bool IsBnd4(Memory<byte> raw)
    {
        var span = raw.Span;
        return span.Length >= 4 && span[0] == 'B' && span[1] == 'N' && span[2] == 'D' && span[3] == '4';
    }

    /// <summary>DS2's .edd beside a loose ESD (machine and state names); null if none</summary>
    public string EddPath { get; }

    public List<EsdFileWrapper> Files { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    public string LoadError { get; private set; }

    private readonly object LoadLock = new();

    public EsdContainer(EsdBank bank, string path, string name, bool isBinder, EsdScriptKind kind, bool inProject, string eddPath)
    {
        Bank = bank;
        Path = path;
        Name = name;
        IsBinder = isBinder;
        Kind = kind;
        InProject = inProject;
        EddPath = eddPath;
    }

    /// <summary>Reads the file and the ESDs in it, if not yet. Returns false if it can't be read (see <see cref="LoadError"/>).</summary>
    public bool EnsureLoaded()
    {
        if (IsLoaded)
            return true;

        lock (LoadLock)
        {
            return Load();
        }
    }

    private bool Load()
    {
        if (IsLoaded)
            return true;

        if (LoadError != null)
            return false;

        try
        {
            var data = Bank.TargetFS.ReadFile(Path);

            if (data == null)
            {
                LoadError = LOC.Get("ESD_Data_Log_Failed_to_Find_File", Path);
                Smithbox.LogError(this, LoadError);
                return false;
            }

            var files = new List<EsdFileWrapper>();

            if (IsBinder)
            {
                // Not BND4.IsRead: the binders implement a different Read overload, so the generic IsRead throws.
                // Decompress once and pick the binder by its magic.
                var raw = DCX.Is(data.Value) ? DCX.Decompress(data.Value) : data.Value;
                using IBinder bnd = IsBnd4(raw) ? BND4.Read(raw) : BND3.Read(raw);

                foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".esd", StringComparison.OrdinalIgnoreCase)))
                {
                    var entryName = System.IO.Path.GetFileName(f.Name);
                    files.Add(EsdFileWrapper.Read(this, System.IO.Path.GetFileNameWithoutExtension(entryName), entryName, f.Bytes));
                }
            }
            else
            {
                var file = EsdFileWrapper.Read(this, Name, null, data.Value);
                file.ImportEdd(ReadEdd());
                files.Add(file);
            }

            Files = files;
            IsLoaded = true;
            return true;
        }
        catch (Exception e)
        {
            LoadError = LOC.Get("ESD_Data_Log_Failed_to_Read_File", Path);
            Smithbox.LogError(this, LoadError, e);
            return false;
        }
    }

    private EDD ReadEdd()
    {
        if (EddPath == null)
            return null;

        try
        {
            var data = Bank.TargetFS.ReadFile(EddPath);
            return data != null ? EDD.Read(data.Value) : null;
        }
        catch (Exception e)
        {
            // Names only; the ESD still shows
            Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Failed_to_Read_Edd", EddPath), e);
            return null;
        }
    }
}

/// <summary>
/// One ESD: a binder entry or a loose file. Read with its container.
/// </summary>
public class EsdFileWrapper
{
    public EsdContainer Container { get; }

    /// <summary>The ESD's name without extension, e.g. t100000 (set by a rename)</summary>
    public string Name { get; internal set; }

    /// <summary>The binder entry's file name, e.g. t100000.esd; null for a loose file</summary>
    public string EntryName { get; internal set; }

    public ESD Esd { get; private set; }

    public string LoadError { get; private set; }

    public EsdScriptKind Kind => Container.Kind;

    /// <summary>Names from DS2's shipped .edd: machine id → name, (machine, state) → name</summary>
    public Dictionary<long, string> MachineNames { get; } = new();
    public Dictionary<(long, long), string> StateNames { get; } = new();

    private EsdFileWrapper(EsdContainer container, string name, string entryName)
    {
        Container = container;
        Name = name;
        EntryName = entryName;
    }

    public static EsdFileWrapper Read(EsdContainer container, string name, string entryName, Memory<byte> data)
    {
        var file = new EsdFileWrapper(container, name, entryName);

        try
        {
            file.Esd = ESD.Read(data);
        }
        catch (Exception e)
        {
            file.LoadError = LOC.Get("ESD_Data_Log_Failed_to_Read_Entry", entryName ?? name, container.Path);
            Smithbox.LogError(container, file.LoadError, e);
        }

        return file;
    }

    /// <summary>A new loose ESD (the only file of its container)</summary>
    public static EsdFileWrapper CreateLoose(EsdContainer container, string name, ESD esd)
    {
        return new EsdFileWrapper(container, name, null) { Esd = esd };
    }

    /// <summary>A new binder entry (a duplicated ESD): saved into the binder as name.esd</summary>
    public static EsdFileWrapper Create(EsdContainer container, string name, ESD esd)
    {
        return new EsdFileWrapper(container, name, name + ".esd") { Esd = esd };
    }

    public void ImportEdd(EDD edd)
    {
        if (edd == null)
            return;

        foreach (var m in edd.Machines)
        {
            if (!string.IsNullOrEmpty(m.Name))
                MachineNames.TryAdd(m.ID, m.Name);

            foreach (var s in m.States)
                if (!string.IsNullOrEmpty(s.Name))
                    StateNames.TryAdd((m.ID, s.ID), s.Name);
        }
    }

    public bool IsLoaded => Esd != null;
}
