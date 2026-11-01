using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors;

/// <summary>
/// Tracks the collapsing sections of a Tools window, so that only one is shown at a time.
/// Tools draw their headers with <see cref="Header"/>, which acts as a normal
/// collapsing header outside of a Begin/End pair.
/// </summary>
public class ToolSections
{
    private static ToolSections Current;

    private string ActiveSection;
    private bool ActiveSectionDrawn;

    public void Begin()
    {
        Current = this;
        ActiveSectionDrawn = false;
    }

    public void End()
    {
        Current = null;

        // The active section is no longer displayed, so bring the others back
        if (!ActiveSectionDrawn)
        {
            ActiveSection = null;
        }
    }

    /// <summary>
    /// The Options menu for the menubar of a Tools window.
    /// </summary>
    public static void OptionsMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Options")}##toolOptionsMenuHeader"))
        {
            ImGui.Checkbox($"{LOC.Get("PREF_Interface_Tool_Window_Single_Section")}##toolSingleSection", ref CFG.Current.Interface_Tool_Window_Single_Section);
            GUI.Tooltip(LOC.Get("PREF_Interface_Tool_Window_Single_Section_TT"));

            ImGui.EndMenu();
        }
    }

    public static bool Header(string label)
    {
        if (Current == null || !CFG.Current.Interface_Tool_Window_Single_Section)
        {
            return ImGui.CollapsingHeader(label);
        }

        return Current.DrawHeader(label);
    }

    private bool DrawHeader(string label)
    {
        if (ActiveSection != null && ActiveSection != label)
        {
            return false;
        }

        var isOpen = ImGui.CollapsingHeader(label);

        ActiveSection = isOpen ? label : null;
        ActiveSectionDrawn = true;

        return isOpen;
    }
}
