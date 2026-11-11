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
    public bool InProject { get; }

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
                var span = raw.Span;
                var isBnd4 = span.Length >= 4 && span[0] == 'B' && span[1] == 'N' && span[2] == 'D' && span[3] == '4';
                using IBinder bnd = isBnd4 ? BND4.Read(raw) : BND3.Read(raw);

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
