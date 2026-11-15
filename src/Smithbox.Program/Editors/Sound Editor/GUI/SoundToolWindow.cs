using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The Sound Editor's Tools window: the tools listed by category down the side (see Claude/Documentation/Tool UI
/// Styling.md).
/// </summary>
public class SoundToolWindow
{
    /// <summary>The window's ImGui ID, so other code can focus it.</summary>
    public const string WindowId = "###soundEditor_ToolWindow";

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
