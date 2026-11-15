using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>The FX Editor's Tools window: the viewport preview, plus the shared grid and rendering tools.</summary>
public class FxToolWindow
{
    private readonly FxEditorView Editor;

    private readonly ToolSections Sections = new();

    public FxToolWindow(FxEditorView editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        Sections.Begin();

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        Editor.PreviewTool.OnToolWindow();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        Editor.SearchTool.OnToolWindow();

        Editor.ReferencesTool.OnToolWindow();

        // Assets
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Assets"));

        Editor.ResourcesTool.OnToolWindow();

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        Editor.GridTool.OnToolWindow();

        // Rendering: sets its own category, so it comes last
        Editor.RenderingTool.OnToolWindow();

        Sections.End();
    }
}
