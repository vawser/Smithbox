using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class ReorderAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public ReorderAction(MapEditorScreen editor, ProjectEntry project)
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
            if (InputManager.IsPressed(InputAction.Reorder_Up))
            {
                ApplyReorder(TreeObjectOrderMovementType.Up);
            }

            if (InputManager.IsPressed(InputAction.Reorder_Down))
            {
                ApplyReorder(TreeObjectOrderMovementType.Down);
            }

            if (InputManager.IsPressed(InputAction.Reorder_Top))
            {
                ApplyReorder(TreeObjectOrderMovementType.Top);
            }

            if (InputManager.IsPressed(InputAction.Reorder_Bottom))
            {
                ApplyReorder(TreeObjectOrderMovementType.Bottom);
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        // Only supported for these types
        if (ent.WrappedObject is IMsbPart or IMsbRegion or IMsbEvent)
        {
            // Move Up
            if (ImGui.Selectable("Move Up"))
            {
                ApplyReorder(TreeObjectOrderMovementType.Up);
            }
            UIHelper.Tooltip($"Move the currently selected map objects up by one in the map object list  for this object type.\n\nShortcut: {InputManager.GetHint(InputAction.Reorder_Up)}");

            // Move Down
            if (ImGui.Selectable("Move Down"))
            {
                ApplyReorder(TreeObjectOrderMovementType.Down);
            }
            UIHelper.Tooltip($"Move the currently selected map objects down by one in the map object list  for this object type.\n\nShortcut: {InputManager.GetHint(InputAction.Reorder_Down)}");

            // Move Top
            if (ImGui.Selectable("Move to Top"))
            {
                ApplyReorder(TreeObjectOrderMovementType.Top);
            }
            UIHelper.Tooltip($"Move the currently selected map objects to the top of the map object list for this object type.\n\nShortcut: {InputManager.GetHint(InputAction.Reorder_Top)}");

            // Move Bottom
            if (ImGui.Selectable("Move to Bottom"))
            {
                ApplyReorder(TreeObjectOrderMovementType.Bottom);
            }
            UIHelper.Tooltip($"Move the currently selected map objects to the bottom of the map object list for this object type.\n\nShortcut: {InputManager.GetHint(InputAction.Reorder_Bottom)}");

            ImGui.Separator();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Move Up", InputManager.GetHint(InputAction.Reorder_Up)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }

        if (ImGui.MenuItem("Move Down", InputManager.GetHint(InputAction.Reorder_Down)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }

        if (ImGui.MenuItem("Move to Top", InputManager.GetHint(InputAction.Reorder_Top)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }

        if (ImGui.MenuItem("Move to Bottom", InputManager.GetHint(InputAction.Reorder_Bottom)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
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
    public void ApplyReorder(TreeObjectOrderMovementType direction)
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            OrderMapObjectsAction action = new(Editor, Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList(), direction);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}

