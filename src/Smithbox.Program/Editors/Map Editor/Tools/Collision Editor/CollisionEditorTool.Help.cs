using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Built-in help (Collision.md (Collision Editor), R9), as the Navigation tool has it: a ? button at the end of the tab
/// bar opens the open tab's description and steps (GUI.TabHelpButton), a ? beside the Brush combo explains the chosen
/// mode (GUI.HelpButton), and a glossary of the collision terms is opened from the header, so tooltips and the tabs
/// themselves can stay short.
/// </summary>
public partial class CollisionEditorTool
{
    /// <summary>The glossary terms, in the order listed: MAP_CollisionEditor_Glossary_{term} and its _Text.</summary>
    private static readonly string[] GlossaryTerms =
    {
        "Layers", "Files", "Binder", "Body", "Model", "Linked", "Radius", "Facing", "Material", "PlacementOrb",
        "FallProtection", "DropEdges", "Filter", "Commit",
    };

    private const string GlossaryPopup = "##collisionEditorGlossaryPopup";

    /// <summary>The ? at the end of the tab bar: the open tab's name, its description, then its steps.</summary>
    private static void DisplayTabHelpButton(CollisionEditorSource tab)
    {
        var (name, description) = tab switch
        {
            CollisionEditorSource.Material => ("MAP_CollisionEditor_Tab_Material", "MAP_CollisionEditor_Material_Description"),
            CollisionEditorSource.Create =>("MAP_CollisionEditor_Tab_Create", "MAP_CollisionEditor_Create_Description"),
            CollisionEditorSource.Subtract => ("MAP_CollisionEditor_Tab_Subtract", "MAP_CollisionEditor_Subtract_Description"),
            CollisionEditorSource.Cleanup =>("MAP_CollisionEditor_Tab_Cleanup", "MAP_CollisionEditor_Cleanup_Description"),
            _ => ("MAP_CollisionEditor_Tab_Edit", "MAP_CollisionEditor_Description"),
        };

        GUI.TabHelpButton("collisionEditorTabHelp", LOC.Get(name),
            LOC.Get(description), LOC.Get($"MAP_CollisionEditor_Help_{tab}"));
    }

    /// <summary>The glossary button, for the header.</summary>
    private void DisplayGlossaryButton()
    {
        if (ImGui.Button($"{Icons.QuestionCircle}##collisionEditorGlossary"))
            ImGui.OpenPopup(GlossaryPopup);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Glossary_TT"));

        DisplayGlossary();
    }

    private void DisplayGlossary()
    {
        // A set width, as wrapped text in an auto-sized window never wraps; the height fits up to a limit, then scrolls
        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);
        ImGui.SetNextWindowSizeConstraints(Vector2.Zero, new Vector2(480, 520) * DPI.UIScale());

        if (!ImGui.BeginPopup(GlossaryPopup))
            return;

        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Glossary_Header"), LOC.Get("MAP_CollisionEditor_Glossary_TT"));

        foreach (var term in GlossaryTerms)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get($"MAP_CollisionEditor_Glossary_{term}"));
            GUI.WrappedText(LOC.Get($"MAP_CollisionEditor_Glossary_{term}_Text"));
        }

        ImGui.EndPopup();
    }
}
