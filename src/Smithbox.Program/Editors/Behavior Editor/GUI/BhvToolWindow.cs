using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The Behavior Editor's Tools window: Search, Used By and Problems (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class BhvToolWindow
{
    private readonly BhvEditorView View;

    public BhvSearchTool SearchTool;
    public BhvUsedByTool UsedByTool;
    public BhvProblemsTool ProblemsTool;

    private readonly ToolSections Sections = new();

    public BhvToolWindow(BhvEditorView view, ProjectEntry project)
    {
        View = view;

        SearchTool = new(view);
        UsedByTool = new(view);
        ProblemsTool = new(view, project);
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

        if (CFG.Current.Interface_BehaviorEditor_Tool_Search)
        {
            // Search
            SearchTool.OnToolWindow();
        }

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        if (CFG.Current.Interface_BehaviorEditor_Tool_UsedBy)
        {
            // Used By
            UsedByTool.OnToolWindow();
        }

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        if (CFG.Current.Interface_BehaviorEditor_Tool_Problems)
        {
            // Problems
            ProblemsTool.OnToolWindow();
        }

        Sections.End();
    }

    private void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##behaviorToolViewMenu"))
        {
            // Search
            if (ImGui.MenuItem($"{LOC.Get("BHV_ToolWindow_View_Toggle_Search")}##behaviorSearchToolToggle"))
                CFG.Current.Interface_BehaviorEditor_Tool_Search = !CFG.Current.Interface_BehaviorEditor_Tool_Search;
            GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Search);

            // Used By
            if (ImGui.MenuItem($"{LOC.Get("BHV_ToolWindow_View_Toggle_Used_By")}##behaviorUsedByToolToggle"))
                CFG.Current.Interface_BehaviorEditor_Tool_UsedBy = !CFG.Current.Interface_BehaviorEditor_Tool_UsedBy;
            GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_UsedBy);

            // Problems
            if (ImGui.MenuItem($"{LOC.Get("BHV_ToolWindow_View_Toggle_Problems")}##behaviorProblemsToolToggle"))
                CFG.Current.Interface_BehaviorEditor_Tool_Problems = !CFG.Current.Interface_BehaviorEditor_Tool_Problems;
            GUI.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_Problems);

            ImGui.EndMenu();
        }
    }
}
