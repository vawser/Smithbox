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
    /// Whether the material has been modified since it was last loaded or saved
    /// </summary>
    public bool IsDirty { get; set; }

    protected MaterialFileWrapper(string key)
    {
        Key = key;
        IsDirty = false;
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
