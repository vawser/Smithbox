using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class DeleteAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public DeleteAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                ApplyDelete();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Delete"))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Delete)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected map objects.");
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
    public void ApplyDelete()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            DeleteMapObjectsAction action = new(Editor,
            Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList(), true);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}