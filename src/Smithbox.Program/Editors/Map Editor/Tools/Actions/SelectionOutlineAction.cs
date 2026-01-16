using StudioCore.Application;
using StudioCore.Keybinds;

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
        if (InputManager.IsPressed(InputAction.Cycle_Render_Outline_Mode))
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