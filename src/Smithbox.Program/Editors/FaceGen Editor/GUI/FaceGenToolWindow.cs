#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor's Tools window (see Claude/Documentation/Tool UI Styling.md):
/// General → Viewport Filters; Preview → Character Preview, Animation Browser.
/// </summary>
public class FaceGenToolWindow
{
    public FaceGenEditorView View;
    public FaceGenViewportFiltersTool ViewportFiltersTool;

    private readonly ToolSections Sections = new();

    public FaceGenToolWindow(FaceGenEditorView view)
    {
        View = view;
        ViewportFiltersTool = new FaceGenViewportFiltersTool(view);
    }

    public void Display()
    {
        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        if (CFG.Current.Interface_FaceGenEditor_Tool_ViewportFilters)
            ViewportFiltersTool.OnToolWindow();

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        if (CFG.Current.Interface_FaceGenEditor_Tool_CharacterPreview)
            View.CharacterPreview.OnToolWindow();

        if (CFG.Current.Interface_FaceGenEditor_Tool_AnimationBrowser)
            View.AnimationTool.OnToolWindow();

        Sections.End();
    }
}
