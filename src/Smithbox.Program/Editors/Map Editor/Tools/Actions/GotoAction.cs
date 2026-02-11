using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;

namespace StudioCore.Editors.MapEditor;

public class GotoAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public GotoAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Jump))
            {
                GotoMapObjectEntry();
            }
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
        if (ImGui.MenuItem("Go to in List", InputManager.GetHint(KeybindID.Jump)))
        {
            GotoMapObjectEntry();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void GotoMapObjectEntry()
    {
        if (View.ViewportSelection.IsSelection())
        {
            View.ViewportSelection.GotoTreeTarget = View.ViewportSelection.GetSingleSelection();
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }

        View.DelayPicking();
    }
}