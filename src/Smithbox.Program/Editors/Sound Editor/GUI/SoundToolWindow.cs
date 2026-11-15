using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The Sound Editor's Tools window: the tools listed by category down the side (see Claude/Documentation/Tool UI
/// Styling.md).
/// </summary>
public class SoundToolWindow
{
    /// <summary>The window's ImGui ID (one per view), so other code can focus it.</summary>
    public string WindowId => $"###soundEditor_ToolWindow_{View.ViewIndex}";

    private readonly SoundEditorView View;

    private ToolSections Sections = new();

    public SoundToolWindow(SoundEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        Sections.Begin();

        // Search & Edit

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));


        View.SearchTool.OnToolWindow();

        Sections.End();
    }
}
