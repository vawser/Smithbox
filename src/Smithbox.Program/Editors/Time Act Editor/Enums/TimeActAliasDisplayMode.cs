namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// How the aliases of the animations are shown in the animation list
/// </summary>
public enum TimeActAliasDisplayMode
{
    /// <summary>
    /// Shown after the name, wrapping onto further lines when it doesn't fit
    /// </summary>
    Wrap = 0,

    /// <summary>
    /// Shown after the name on the same line, cut short with an ellipsis when it doesn't fit
    /// </summary>
    Truncate = 1,

    /// <summary>
    /// Not shown. The filter still matches them.
    /// </summary>
    Hide = 2
}
