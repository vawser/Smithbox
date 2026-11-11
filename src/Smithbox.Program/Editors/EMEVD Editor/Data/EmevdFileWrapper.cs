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
    public bool InProject { get; }

    /// <summary>The .emeld beside it, which holds event names (DS1, DS1R, BB, DB_NT); null if none</summary>
    public string EmeldPath { get; }

    public EMEVD Emevd { get; private set; }

    public bool IsLoaded => Emevd != null;

    /// <summary>Why the file couldn't be read, once a load failed</summary>
    public string LoadError { get; private set; }

    private Dictionary<long, EMEVD.Event> EventsById;

    public EmevdFileWrapper(EmevdBank bank, string path, string name, bool inProject, string emeldPath)
    {
        Bank = bank;
        Path = path;
        Name = name;
        InProject = inProject;
        EmeldPath = emeldPath;
    }

    /// <summary>The kind of file, for grouping: common, common_func or a map</summary>
    public bool IsCommon => Name.StartsWith("common", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads the file if it isn't yet. Returns false if it can't be read (see <see cref="LoadError"/>).</summary>
    public bool EnsureLoaded()
    {
        if (Emevd != null)
            return true;

        if (LoadError != null)
            return false;

        try
        {
            var data = Bank.TargetFS.ReadFile(Path);

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

            Emevd = emevd;
            EventsById = new();

            foreach (var ev in emevd.Events)
                EventsById.TryAdd(ev.ID, ev);

            return true;
        }
        catch (Exception e)
        {
            LoadError = LOC.Get("EMEVD_Data_Log_Failed_to_Read_File", Path);
            Smithbox.LogError(this, LoadError, e);
            return false;
        }
    }

    public EMEVD.Event FindEvent(long id)
    {
        return EventsById != null && EventsById.TryGetValue(id, out var ev) ? ev : null;
    }
}
