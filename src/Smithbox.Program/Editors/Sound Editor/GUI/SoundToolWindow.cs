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
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Search & Edit

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));


        if (CFG.Current.Interface_SoundEditor_Tool_Search)
        {
            View.SearchTool.OnToolWindow();
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("SOUND_ToolWindow_View_Header")}##viewMenuHeader"))
        {
            // Search
            if (ImGui.MenuItem($"{LOC.Get("SOUND_ToolWindow_View_Toggle_Search")}##searchToolToggle"))
            {
                CFG.Current.Interface_SoundEditor_Tool_Search = !CFG.Current.Interface_SoundEditor_Tool_Search;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_SoundEditor_Tool_Search);

            ImGui.EndMenu();
        }
    }
}
