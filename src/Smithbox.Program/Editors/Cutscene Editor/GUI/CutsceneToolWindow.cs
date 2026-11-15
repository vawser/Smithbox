using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>The Cutscene Editor's Tools window (see Claude/Documentation/Tool UI Styling.md): General → Cutscene Filters.</summary>
public class CutsceneToolWindow
{
    private readonly CutsceneEditorView Editor;

    public CutsceneFiltersTool FiltersTool;

    private readonly ToolSections Sections = new();

    public CutsceneToolWindow(CutsceneEditorView editor)
    {
        Editor = editor;
        FiltersTool = new CutsceneFiltersTool(editor);
    }

    public void Display()
    {
        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        FiltersTool.OnToolWindow();

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        // Character Preview: what the player-like actors (c0000) wear
        Editor.CharacterPreview.OnToolWindow();

        Sections.End();
    }
}
