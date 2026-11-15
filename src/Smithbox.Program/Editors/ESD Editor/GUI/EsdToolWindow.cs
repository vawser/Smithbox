using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor's Tools window: Search, Called By and Palette (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class EsdToolWindow
{
    private readonly EsdEditorView View;

    public EsdPaletteTool PaletteTool;
    public EsdSearchTool SearchTool;
    public EsdCalledByTool CalledByTool;

    private readonly ToolSections Sections = new();

    public EsdToolWindow(EsdEditorView view)
    {
        View = view;
        PaletteTool = new(view);
        SearchTool = new(view);
        CalledByTool = new(view);
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

        // Called By
        CalledByTool.OnToolWindow();

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Palette
        PaletteTool.OnToolWindow();

        Sections.End();
    }
}
