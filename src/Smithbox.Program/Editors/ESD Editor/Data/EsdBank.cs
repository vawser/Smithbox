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

    public List<string> GetDirtyFiles()
    {
        return Containers.Where(c => c.IsDirty).Select(c => c.Path).ToList();
    }

    /// <summary>Saves every file with unsaved changes. Returns false if any failed.</summary>
    public bool SaveAllDirty()
    {
        var ok = true;
        foreach (var c in Containers.Where(c => c.IsDirty))
            if (!c.Save())
                ok = false;
        return ok;
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
                var data = fs.ReadFile(Path);
                if (data == null)
                {
                    Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Failed_to_Find_File", Path));
                    return false;
                }

                var raw = DCX.Is(data.Value) ? DCX.Decompress(data.Value, out _) : data.Value;
                bnd = IsBnd4(raw) ? BND4.Read(data.Value) : BND3.Read(data.Value);

                foreach (var file in Files.Where(f => f.Esd != null && f.EntryName != null))
                {
                    var entry = bnd.Files.Find(f => System.IO.Path.GetFileName(f.Name).Equals(file.EntryName, StringComparison.OrdinalIgnoreCase));
                    if (entry == null)
                    {
                        Smithbox.LogError(this, LOC.Get("ESD_Data_Log_Entry_Missing", file.EntryName, Path));
                        bnd.Dispose();
                        return false;
                    }
                    entry.Bytes = file.Esd.Write();
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

    /// <summary>The ESD's name without extension, e.g. t100000</summary>
    public string Name { get; }

    /// <summary>The binder entry's file name, e.g. t100000.esd; null for a loose file</summary>
    public string EntryName { get; }

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
