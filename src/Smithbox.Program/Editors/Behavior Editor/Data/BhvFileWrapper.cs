using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// A .bhv file within a behbnd, with its editing state
/// </summary>
public class BhvFileWrapper
{
    public BhvBinderWrapper Binder { get; }

    /// <summary>The entry name within the binder</summary>
    public string Key { get; }

    /// <summary>The binder entry ID (7000600 basenormal, 7000610 weapon, 7000620+ wNNN)</summary>
    public int EntryID { get; }

    public BHV BHV { get; }

    /// <summary>The file name without folder, e.g. w315.bhv</summary>
    public string Name => Path.GetFileName(Key.Replace('\\', '/'));

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

    /// <summary>Counts every edit and undo, for views that cache what they work out from the file</summary>
    public int Revision { get; private set; }

    public void MarkChanged() { ChangeBalance++; Revision++; }

    public void MarkUndone() { ChangeBalance--; Revision++; }

    public BhvFileWrapper(BhvBinderWrapper binder, string key, int entryId, BHV bhv)
    {
        Binder = binder;
        Key = key;
        EntryID = entryId;
        BHV = bhv;
    }
}
