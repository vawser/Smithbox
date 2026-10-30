using Havok.Shared;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// A wrapper class that associates a Havok file within a binder
/// with the editing state of that file.
/// </summary>
public class HavokFileWrapper
{
    /// <summary>
    /// The name of this file within its binder
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The root container of the Havok file, null until the file is loaded
    /// </summary>
    public IRootContainer Root { get; set; }

    /// <summary>
    /// Whether the file has been modified since it was last loaded or saved
    /// </summary>
    public bool IsDirty { get; set; }

    public HavokFileWrapper(string key)
    {
        Key = key;
        IsDirty = false;
    }

    public bool IsLoaded()
    {
        return Root != null;
    }
}
