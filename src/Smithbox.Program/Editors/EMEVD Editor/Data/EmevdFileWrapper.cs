using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// One .emevd file of the project. Read on first use: ER alone has 589 files, so the bank doesn't read them all at
/// startup.
/// </summary>
public class EmevdFileWrapper
{
    public EmevdBank Bank { get; }

    /// <summary>The VFS path, e.g. /event/m10_00_00_00.emevd.dcx</summary>
    public string Path { get; }

    /// <summary>The file name without extensions, e.g. m10_00_00_00</summary>
    public string Name { get; }

    /// <summary>The project folder has its own copy (it overrides the game's)</summary>
    public bool InProject { get; private set; }

    // Edits done minus edits undone since the last load or save
    private int ChangeBalance;
    private bool ForcedDirty;

    /// <summary>
    /// Whether the file has changed since it was loaded or saved. Setting it true marks it changed with no edit
    /// (Allow Unmodified Save); setting it false marks the current state as saved.
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

    /// <summary>Counts every edit and undo, so views rebuild what they work out from the file</summary>
    public int Revision { get; private set; }

    public void MarkChanged() { ChangeBalance++; Revision++; }

    public void MarkUndone() { ChangeBalance--; Revision++; }

    /// <summary>
    /// Writes the file into the project folder with the compression it was read with. EMEVD.Read → Write is
    /// byte-identical on every readable vanilla file (Claude/Tools/EmevdEditorCheck roundtrip), so only the edited
    /// bytes change. The .emeld isn't written: names aren't edited yet.
    /// </summary>
    public bool Save()
    {
        if (Emevd == null)
            return false;

        // DS2 / SotFS: an entry of the regulation binder
        if (EntryName != null)
            return Bank.SaveRegulation(new[] { this });

        try
        {
            Bank.Project.VFS.ProjectFS.WriteFile(Path, Emevd.Write());
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Write_File", Path), e);
            return false;
        }

        MarkSaved();
        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", Path));

        return true;
    }

    /// <summary>The current state is what the project now holds</summary>
    public void MarkSaved()
    {
        IsDirty = false;
        InProject = true;
    }

    /// <summary>The entry's name inside the regulation binder (DS2, SotFS), e.g. SpEffectRing.emevd; null for a loose file</summary>
    public string EntryName { get; }

    /// <summary>The .emeld beside it, which holds event names (DS1, DS1R, BB, DB_NT); null if none</summary>
    public string EmeldPath { get; }

    public EMEVD Emevd { get; private set; }

    public bool IsLoaded => Emevd != null;

    /// <summary>Why the file couldn't be read, once a load failed</summary>
    public string LoadError { get; private set; }

    private Dictionary<long, EMEVD.Event> EventsById;

    public EmevdFileWrapper(EmevdBank bank, string path, string name, bool inProject, string emeldPath, string entryName = null)
    {
        Bank = bank;
        Path = path;
        Name = name;
        InProject = inProject;
        EmeldPath = emeldPath;
        EntryName = entryName;
    }

    /// <summary>The kind of file, for grouping: common, common_func or a map</summary>
    public bool IsCommon => Name.StartsWith("common", StringComparison.OrdinalIgnoreCase);

    private readonly object LoadLock = new();

    /// <summary>
    /// Reads the file if it isn't yet. Returns false if it can't be read (see <see cref="LoadError"/>). Safe to call
    /// from the Tools window's background scans while the UI reads other files.
    /// </summary>
    public bool EnsureLoaded()
    {
        if (Emevd != null)
            return true;

        lock (LoadLock)
        {
            return Load();
        }
    }

    private bool Load()
    {
        if (Emevd != null)
            return true;

        if (LoadError != null)
            return false;

        try
        {
            var data = EntryName != null
                ? (Bank.ReadRegulationEntry(EntryName) is { } entry ? (Memory<byte>?)new Memory<byte>(entry) : null)
                : Bank.TargetFS.ReadFile(Path);

            if (data == null)
            {
                LoadError = LOC.Get("EMEVD_Data_Log_Failed_to_Find_File", Path);
                Smithbox.LogError(this, LoadError);
                return false;
            }

            var emevd = EMEVD.Read(data.Value);

            if (EmeldPath != null)
            {
                try
                {
                    var eld = Bank.TargetFS.ReadFile(EmeldPath);
                    if (eld != null)
                        emevd.ImportEMELD(EMELD.Read(eld.Value));
                }
                catch (Exception e)
                {
                    // Names only; the events still show
                    Smithbox.LogError(this, LOC.Get("EMEVD_Data_Log_Failed_to_Read_Emeld", EmeldPath), e);
                }
            }

            Reindex(emevd);
            Emevd = emevd;

            return true;
        }
        catch (Exception e)
        {
            LoadError = LOC.Get("EMEVD_Data_Log_Failed_to_Read_File", Path);
            Smithbox.LogError(this, LoadError, e);
            return false;
        }
    }

    // The revision the ID index was built at: adding, removing or renumbering an event is an edit, so it's rebuilt
    private int IndexRevision;

    private void Reindex(EMEVD emevd)
    {
        var index = new Dictionary<long, EMEVD.Event>();
        foreach (var ev in emevd.Events)
            index.TryAdd(ev.ID, ev);

        IndexRevision = Revision;
        EventsById = index;
    }

    public EMEVD.Event FindEvent(long id)
    {
        if (Emevd == null)
            return null;

        if (IndexRevision != Revision)
            Reindex(Emevd);

        return EventsById.TryGetValue(id, out var ev) ? ev : null;
    }

    public bool HasEvent(long id) => FindEvent(id) != null;
}
