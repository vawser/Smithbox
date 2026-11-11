using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor's Tools window: Palette and Problems (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class EmevdToolWindow
{
    private readonly EmevdEditorView View;

    public EmevdPaletteTool PaletteTool;
    public EmevdProblemsTool ProblemsTool;

    private readonly ToolSections Sections = new();

    public EmevdToolWindow(EmevdEditorView view)
    {
        View = view;

        PaletteTool = new(view);
        ProblemsTool = new(view);
    }

    public void Draw()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        if (CFG.Current.Interface_EmevdEditor_Tool_Palette)
        {
            // Palette
            PaletteTool.OnToolWindow();
        }

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        if (CFG.Current.Interface_EmevdEditor_Tool_Problems)
        {
            // Problems
            ProblemsTool.OnToolWindow();
        }

        Sections.End();
    }

    private void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##emevdToolViewMenu"))
        {
            // Palette
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Palette")}##emevdPaletteToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Palette = !CFG.Current.Interface_EmevdEditor_Tool_Palette;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Palette);

            // Problems
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Problems")}##emevdProblemsToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Problems = !CFG.Current.Interface_EmevdEditor_Tool_Problems;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Problems);

            ImGui.EndMenu();
        }
    }
}
