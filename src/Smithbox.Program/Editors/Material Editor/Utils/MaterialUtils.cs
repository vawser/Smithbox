using Hexa.NET.ImGui;
using StudioCore.Application;

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
