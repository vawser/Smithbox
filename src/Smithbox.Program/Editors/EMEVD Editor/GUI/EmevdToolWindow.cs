using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor's Tools window: Search, Called By, Palette and Problems (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class EmevdToolWindow
{
    private readonly EmevdEditorView View;

    public EmevdPaletteTool PaletteTool;
    public EmevdProblemsTool ProblemsTool;
    public EmevdCalledByTool CalledByTool;
    public EmevdSearchTool SearchTool;
    public EmevdMassEditTool MassEditTool;
    public EmevdTemplatesTool TemplatesTool;

    private readonly ToolSections Sections = new();

    public EmevdToolWindow(EmevdEditorView view)
    {
        View = view;

        PaletteTool = new(view);
        ProblemsTool = new(view);
        CalledByTool = new(view);
        SearchTool = new(view);
        MassEditTool = new(view);
        TemplatesTool = new(view);
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

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        if (CFG.Current.Interface_EmevdEditor_Tool_Search)
        {
            // Search
            SearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_EmevdEditor_Tool_MassEdit)
        {
            // Mass Edit
            MassEditTool.OnToolWindow();
        }

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        if (CFG.Current.Interface_EmevdEditor_Tool_CalledBy)
        {
            // Called By
            CalledByTool.OnToolWindow();
        }

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        if (CFG.Current.Interface_EmevdEditor_Tool_Palette)
        {
            // Palette
            PaletteTool.OnToolWindow();
        }

        if (CFG.Current.Interface_EmevdEditor_Tool_Templates)
        {
            // Templates
            TemplatesTool.OnToolWindow();
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
            // Search
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Search")}##emevdSearchToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Search = !CFG.Current.Interface_EmevdEditor_Tool_Search;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Search);

            // Mass Edit
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Mass_Edit")}##emevdMassEditToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_MassEdit = !CFG.Current.Interface_EmevdEditor_Tool_MassEdit;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_MassEdit);

            // Called By
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Called_By")}##emevdCalledByToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_CalledBy = !CFG.Current.Interface_EmevdEditor_Tool_CalledBy;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_CalledBy);

            // Palette
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Palette")}##emevdPaletteToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Palette = !CFG.Current.Interface_EmevdEditor_Tool_Palette;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Palette);

            // Templates
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Templates")}##emevdTemplatesToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Templates = !CFG.Current.Interface_EmevdEditor_Tool_Templates;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Templates);

            // Problems
            if (ImGui.MenuItem($"{LOC.Get("EMEVD_ToolWindow_View_Toggle_Problems")}##emevdProblemsToolToggle"))
                CFG.Current.Interface_EmevdEditor_Tool_Problems = !CFG.Current.Interface_EmevdEditor_Tool_Problems;
            GUI.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_Problems);

            ImGui.EndMenu();
        }
    }
}
