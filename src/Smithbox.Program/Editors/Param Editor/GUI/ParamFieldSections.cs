using StudioCore.Application;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The section headers of the Param Editor's Fields window (panels drawn above the field table, such as the Random
/// Appear Slots), drawn as the Tools window draws its categories: a separator with the title in it
/// (<see cref="ToolSections.CategoryHeader"/>). Double-clicking a title hides or shows its section. The hidden
/// sections are kept for the session and shared by every row, so a section stays hidden while the selection changes.
/// The Map Editor's Properties window does the same with <see cref="MapEditor.MapPropertySections"/>.
/// </summary>
public static class ParamFieldSections
{
    public const string RandomAppearKey = "randomAppear";
    public const string MapPlacementKey = "mapPlacement";

    private static readonly HashSet<string> Collapsed = new();

    /// <summary>
    /// Draws a section title. Returns true when the section's contents should be drawn.
    /// </summary>
    /// <param name="key">Identifies the section, one of the keys above</param>
    /// <param name="title">The displayed title</param>
    /// <param name="description">What the section shows, put above the double-click hint in the tooltip</param>
    /// <param name="count">How many entries the section has, shown while it is hidden. Null shows no count.</param>
    public static bool Header(string key, string title, string description = null, int? count = null)
    {
        var hint = LOC.Get("PARAM_FieldWindow_Section_TT");
        var tooltip = string.IsNullOrEmpty(description) ? hint : $"{description}\n\n{hint}";

        return ToolSections.CategoryHeader(key, title, count, Collapsed, tooltip);
    }
}
