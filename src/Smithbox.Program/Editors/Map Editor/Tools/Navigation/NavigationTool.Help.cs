using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Built-in help (Navigation.md, R7): a ? button at the end of each tab bar opens the open tab's
/// description and steps (GUI.TabHelpButton), ? buttons beside the Tool and Brush combos explain the chosen entry
/// (GUI.HelpButton), and a glossary of the navigation terms is opened from the header, so tooltips and the tabs
/// themselves can stay short.
/// </summary>
public partial class NavigationTool
{
    /// <summary>The glossary terms, in the order listed: MAP_Navigation_Glossary_{term} and its _Text.</summary>
    private static readonly string[] GlossaryTerms =
    {
        "Navmesh", "Sets", "NameID", "Placeholder", "Quarters", "Connections", "Links", "Gates", "Clearance",
        "FaceData", "NodeBanks",
    };

    private const string GlossaryPopup = "##navigationGlossaryPopup";

    /// <summary>The help of a tab of the Havok mode: its description (where it has one) then its steps.</summary>
    private static string[] TabHelpTexts(NavTab tab)
    {
        var description = tab switch
        {
            NavTab.Overlay => "MAP_Navigation_View_Description",
            NavTab.Edit => "MAP_Navigation_Edit_Description",
            NavTab.Regenerate => "MAP_Navigation_Regenerate_Description",
            NavTab.Models => "MAP_Navigation_Models_Description",
            NavTab.Gates => "MAP_Navigation_Gates_Description",
            NavTab.Links => "MAP_Navigation_Links_Description",
            NavTab.FaceData => "MAP_Navigation_Face_Data_Description",
            _ => null,
        };

        return new[] { description, $"MAP_Navigation_Help_{tab}" }
            .Where(k => k != null)
            .Select(k => LOC.Get(k))
            .ToArray();
    }

    /// <summary>The glossary button, for the header.</summary>
    private void DisplayGlossaryButton()
    {
        if (ImGui.Button($"{Icons.QuestionCircle}##navigationGlossary"))
            ImGui.OpenPopup(GlossaryPopup);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Glossary_TT"));

        DisplayGlossary();
    }

    private void DisplayGlossary()
    {
        // A set width, as wrapped text in an auto-sized window never wraps; the height fits up to a limit, then scrolls
        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);
        ImGui.SetNextWindowSizeConstraints(Vector2.Zero, new Vector2(480, 520) * DPI.UIScale());

        if (!ImGui.BeginPopup(GlossaryPopup))
            return;

        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Glossary_Header"), LOC.Get("MAP_Navigation_Glossary_TT"));

        foreach (var term in GlossaryTerms)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get($"MAP_Navigation_Glossary_{term}"));
            GUI.WrappedText(LOC.Get($"MAP_Navigation_Glossary_{term}_Text"));
        }

        ImGui.EndPopup();
    }
}
