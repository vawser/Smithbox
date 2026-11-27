using StudioCore.Application;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The section headers of the Map Editor's Properties window, drawn as the Tools window draws its categories: a
/// separator with the title in it. Double-clicking a title hides or shows its section. The hidden sections are kept
/// for the session and shared by every map object, so a section stays hidden while the selection changes.
/// </summary>
public static class MapPropertySections
{
    public const string SummaryKey = "header";
    public const string PropertiesKey = "properties";
    public const string VanillaCompareKey = "vanillaCompare";
    public const string ReferencesKey = "references";
    public const string ReferencedByKey = "referencedBy";
    public const string ParamsKey = "params";
    public const string ConnectionKey = "connection";
    public const string ConnectionsKey = "connections";
    public const string TargetMapKey = "targetMap";
    public const string SmallBasesKey = "smallBases";

    // Compare with Vanilla starts hidden, as its collapsing header did
    private static readonly HashSet<string> Collapsed = new() { VanillaCompareKey };

    /// <summary>
    /// Draws a section title. Returns true when the section's contents should be drawn.
    /// </summary>
    /// <param name="key">Identifies the section, one of the keys above</param>
    /// <param name="title">The displayed title</param>
    /// <param name="description">What the section shows, put above the double-click hint in the tooltip</param>
    /// <param name="count">How many entries the section has, shown while it is hidden. Null shows no count.</param>
    public static bool Header(string key, string title, string description = null, int? count = null)
    {
        var hint = LOC.Get("MAP_Properties_Section_TT");
        var tooltip = string.IsNullOrEmpty(description) ? hint : $"{description}\n\n{hint}";

        return ToolSections.CategoryHeader(key, title, count, Collapsed, tooltip);
    }
}
