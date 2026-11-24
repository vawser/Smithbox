using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The Behavior Editor's Tools window: Search, Used By and Problems (see Claude/Documentation/UI.md (Tools window conventions))
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
        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Search
        SearchTool.OnToolWindow();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        // Used By
        UsedByTool.OnToolWindow();

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        // Problems
        ProblemsTool.OnToolWindow();

        Sections.End();
    }
}
