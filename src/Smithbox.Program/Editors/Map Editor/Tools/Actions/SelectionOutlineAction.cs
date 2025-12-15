using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

public class SelectionOutlineAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public SelectionOutlineAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
        {
            CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Not shown here
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
}