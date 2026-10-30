using SoulsFormats;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// A wrapper class that associates a GPARAM file entry with its loaded GPARAM object
/// and the editing state of that object.
/// </summary>
public class GparamWrapper
{
    public FileDictionaryEntry FileEntry { get; set; }

    /// <summary>
    /// The loaded GPARAM object. This is null until the file has been loaded.
    /// </summary>
    public GPARAM Gparam { get; set; }

    /// <summary>
    /// Whether the GPARAM has been modified since it was last loaded or saved
    /// </summary>
    public bool IsDirty { get; set; }

    public GparamWrapper(FileDictionaryEntry fileEntry)
    {
        FileEntry = fileEntry;
        Gparam = null;
        IsDirty = false;
    }

    /// <summary>
    /// Whether the GPARAM object has been loaded for this file
    /// </summary>
    public bool IsLoaded()
    {
        return Gparam != null;
    }
}
