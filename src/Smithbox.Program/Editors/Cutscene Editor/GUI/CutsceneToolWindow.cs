using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>The Cutscene Editor's Tools window (see Claude/Documentation/Tool UI Styling.md): General → Cutscene Filters.</summary>
public class CutsceneToolWindow
{
    private readonly CutsceneEditorScreen Editor;

    public CutsceneFiltersTool FiltersTool;

    private readonly ToolSections Sections = new();

    public CutsceneToolWindow(CutsceneEditorScreen editor)
    {
        Editor = editor;
        FiltersTool = new CutsceneFiltersTool(editor);
    }

    public void Display()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        if (CFG.Current.Interface_CutsceneEditor_Tool_Filters)
            FiltersTool.OnToolWindow();

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        // Character Preview: what the player-like actors (c0000) wear
        if (CFG.Current.Interface_CutsceneEditor_Tool_CharacterPreview)
            Editor.CharacterPreview.OnToolWindow();

        Sections.End();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##cutsceneToolViewMenu"))
            return;

        // Cutscene Filters
        if (ImGui.MenuItem($"{LOC.Get("CUTSCENE_ToolWindow_View_Toggle_Filters")}##cutsceneFiltersToolToggle"))
            CFG.Current.Interface_CutsceneEditor_Tool_Filters = !CFG.Current.Interface_CutsceneEditor_Tool_Filters;
        GUI.ShowActiveStatus(CFG.Current.Interface_CutsceneEditor_Tool_Filters);

        // Character Preview
        if (ImGui.MenuItem($"{LOC.Get("CUTSCENE_ToolWindow_View_Toggle_Character_Preview")}##cutsceneCharacterPreviewToggle"))
            CFG.Current.Interface_CutsceneEditor_Tool_CharacterPreview = !CFG.Current.Interface_CutsceneEditor_Tool_CharacterPreview;
        GUI.ShowActiveStatus(CFG.Current.Interface_CutsceneEditor_Tool_CharacterPreview);

        ImGui.EndMenu();
    }
}
