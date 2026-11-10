using SoulsFormats;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// A wrapper class that associates a material file within a binder
/// with the editing state of that file.
/// </summary>
public abstract class MaterialFileWrapper
{
    /// <summary>
    /// The name of this file within its binder
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Edits done minus edits undone since the last load or save, so undoing back to that state is clean.
    /// Undoing past a save goes negative, which is a change too.
    /// </summary>
    private int ChangeBalance;

    /// <summary>
    /// Set by a save that writes the file with no edits
    /// </summary>
    private bool ForcedDirty;

    /// <summary>
    /// Whether the material has been modified since it was last loaded or saved.
    /// Setting it true marks it modified with no edit; setting it false marks the current state as saved.
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

    /// <summary>
    /// Called by an action when it is done or redone
    /// </summary>
    public void MarkChanged() => ChangeBalance++;

    /// <summary>
    /// Called by an action when it is undone
    /// </summary>
    public void MarkUndone() => ChangeBalance--;

    protected MaterialFileWrapper(string key)
    {
        Key = key;
    }
}

public class MTDFileWrapper : MaterialFileWrapper
{
    public MTD MTD { get; set; }

    public MTDFileWrapper(string key, MTD mtd) : base(key)
    {
        MTD = mtd;
    }
}

public class MATBINFileWrapper : MaterialFileWrapper
{
    public MATBIN MATBIN { get; set; }

    public MATBINFileWrapper(string key, MATBIN matbin) : base(key)
    {
        MATBIN = matbin;
    }
}
