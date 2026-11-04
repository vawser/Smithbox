namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// How the parameters of events are named in the properties
/// </summary>
public enum TimeActFieldNameMode
{
    /// <summary>
    /// The annotated name from the meta, where a parameter has one, or else the internal name
    /// </summary>
    Annotation = 0,

    /// <summary>
    /// The name in the event template
    /// </summary>
    Internal = 1
}
