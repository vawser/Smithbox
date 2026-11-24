#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor's Tools window (see Claude/Documentation/Tool UI Styling.md):
/// General → Viewport Filters; Editing → Randomise; Validation → Face Check; Preview → Character Preview,
/// Animation Browser.
/// </summary>
public class FaceGenToolWindow
{
    public FaceGenEditorView View;
    public FaceGenViewportFiltersTool ViewportFiltersTool;
    public FaceGenRandomiseTool RandomiseTool;
    public FaceGenCheckTool CheckTool;
    public FaceGenLibraryTool LibraryTool;

    private readonly ToolSections Sections = new();

    public FaceGenToolWindow(FaceGenEditorView view)
    {
        View = view;
        ViewportFiltersTool = new FaceGenViewportFiltersTool(view);
        RandomiseTool = new FaceGenRandomiseTool(view);
        CheckTool = new FaceGenCheckTool(view);
        LibraryTool = new FaceGenLibraryTool(view);
    }

    public void Display()
    {
        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_ViewportFilters)
            ViewportFiltersTool.OnToolWindow();

        // Editing
        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_Randomise)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));
            RandomiseTool.OnToolWindow();
        }

        // Data Transfer
        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_FaceLibrary)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data_Transfer"));
            LibraryTool.OnToolWindow();
        }

        // Validation
        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_FaceCheck)
        {
            ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));
            CheckTool.OnToolWindow();
        }

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_CharacterPreview)
            View.CharacterPreview.OnToolWindow();

        if (CFG.Current.Interface.Interface_FaceGenEditor_Tool_AnimationBrowser)
            View.AnimationTool.OnToolWindow();

        Sections.End();
    }
}
