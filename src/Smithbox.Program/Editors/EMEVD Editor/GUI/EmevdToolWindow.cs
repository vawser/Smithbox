using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor's Tools window: Search, Called By, Palette and Problems (see Claude/Documentation/UI.md (Tools window conventions))
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
        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Search
        SearchTool.OnToolWindow();

        // Mass Edit
        MassEditTool.OnToolWindow();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        // Called By
        CalledByTool.OnToolWindow();

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Palette
        PaletteTool.OnToolWindow();

        // Templates
        TemplatesTool.OnToolWindow();

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        // Problems
        ProblemsTool.OnToolWindow();

        Sections.End();
    }
}
