using StudioCore.Application;
using StudioCore.Keybinds;

namespace StudioCore.Editors.MapEditor;

public class SelectionOutlineAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public SelectionOutlineAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputManager.IsPressed(KeybindID.Cycle_Render_Outline_Mode))
        {
            CFG.Current.Viewport_Enable_Selection_Tint = !CFG.Current.Viewport_Enable_Selection_Tint;
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