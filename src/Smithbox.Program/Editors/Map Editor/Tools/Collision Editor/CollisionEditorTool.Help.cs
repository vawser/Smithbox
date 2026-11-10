using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Built-in help (Collision Editor UX Analysis, R9), as the Navigation tool has it: a "How this works" section at the
/// top of each tab, open the first time the tab is shown and closed after that (remembered in CFG), and a glossary of
/// the collision terms opened from the header, so tooltips can stay short.
/// </summary>
public partial class CollisionEditorTool
{
    /// <summary>Whether each tab's help is open this session; starts from whether the tab was seen before.</summary>
    private readonly Dictionary<CollisionEditorSource, bool> HelpOpen = new();

    /// <summary>The glossary terms, in the order listed: MAP_CollisionEditor_Glossary_{term} and its _Text.</summary>
    private static readonly string[] GlossaryTerms =
    {
        "Layers", "Files", "Binder", "Body", "Model", "Linked", "Radius", "Facing", "Material", "PlacementOrb",
        "FallProtection", "DropEdges", "Filter", "Commit",
    };

    private const string GlossaryPopup = "##collisionEditorGlossaryPopup";

    /// <summary>A tab's "How this works" section, drawn at the top of the tab's content.</summary>
    private void DisplayTabHelp(CollisionEditorSource tab)
    {
        if (!HelpOpen.TryGetValue(tab, out var open))
        {
            var seen = CFG.Current.MapEditor_CollisionEditor_Help_Seen;
            open = !seen.Contains(tab.ToString());

            // Shown once: from the next session it starts closed
            if (open)
                seen.Add(tab.ToString());
        }

        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Help_Header"), LOC.Get("MAP_CollisionEditor_Help_Header_TT"), ref open);
        HelpOpen[tab] = open;

        if (open)
            GUI.WrappedText(LOC.Get($"MAP_CollisionEditor_Help_{tab}"));

        GUI.Spacer();
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
