using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

public static class MaterialUtils
{
    public static bool SupportsMATBIN(ProjectEntry project)
    {
        if(project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            return true;
        }

        return false;
    }

    #region MATBIN Keys
    // The rules are in MaterialKeys, which has no editor dependencies

    public static uint MatbinKey(string text) => MaterialKeys.Compute(text);

    public static string KeySourceProperty(object obj) => MaterialKeys.SourceProperty(obj);

    /// <summary>
    /// Whether this property is a MATBIN key worked out from other text, so it isn't edited by hand
    /// </summary>
    public static bool IsDerivedKey(object obj, string propName) =>
        propName == "Key" && KeySourceProperty(obj) != null;

    public static void ApplyMatbinKey(object obj) => MaterialKeys.Apply(obj);
    #endregion

    /// <summary>
    /// Centers the current list child on an entry, for use before a clipper as the entry may not be submitted yet.
    /// </summary>
    public static void ScrollToIndex(int index)
    {
        if (index < 0)
            return;

        var rowHeight = ImGui.GetTextLineHeightWithSpacing();

        ImGui.SetScrollY(index * rowHeight - ImGui.GetWindowHeight() * 0.5f);
    }
}
