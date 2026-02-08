using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ReorderAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ReorderAction(ModelEditorView view, ProjectEntry project)
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
            if (InputManager.IsPressed(KeybindID.Reorder_Up))
            {
                ApplyReorder(TreeObjectOrderMovementType.Up);
            }

            if (InputManager.IsPressed(KeybindID.Reorder_Down))
            {
                ApplyReorder(TreeObjectOrderMovementType.Down);
            }

            if (InputManager.IsPressed(KeybindID.Reorder_Top))
            {
                ApplyReorder(TreeObjectOrderMovementType.Top);
            }

            if (InputManager.IsPressed(KeybindID.Reorder_Bottom))
            {
                ApplyReorder(TreeObjectOrderMovementType.Bottom);
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Move Up"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }
        UIHelper.Tooltip($"Move the currently selected model objects up by one in the model object list  for this object type.\n\nShortcut: {InputManager.GetHint(KeybindID.Reorder_Up)}");

        // Move Down
        if (ImGui.Selectable("Move Down"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }
        UIHelper.Tooltip($"Move the currently selected model objects down by one in the model object list  for this object type.\n\nShortcut: {InputManager.GetHint(KeybindID.Reorder_Down)}");

        // Move Top
        if (ImGui.Selectable("Move to Top"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }
        UIHelper.Tooltip($"Move the currently selected model objects to the top of the model object list for this object type.\n\nShortcut: {InputManager.GetHint(KeybindID.Reorder_Top)}");

        // Move Bottom
        if (ImGui.Selectable("Move to Bottom"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
        UIHelper.Tooltip($"Move the currently selected model objects to the bottom of the model object list for this object type.\n\nShortcut: {InputManager.GetHint(KeybindID.Reorder_Bottom)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Move Up", InputManager.GetHint(KeybindID.Reorder_Up)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }

        if (ImGui.MenuItem("Move Down", InputManager.GetHint(KeybindID.Reorder_Down)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }

        if (ImGui.MenuItem("Move to Top", InputManager.GetHint(KeybindID.Reorder_Top)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }

        if (ImGui.MenuItem("Move to Bottom", InputManager.GetHint(KeybindID.Reorder_Bottom)))
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
        if (View.ViewportSelection.IsSelection())
        {
            if (View.Selection.SelectedModelWrapper != null)
            {
                var container = View.Selection.SelectedModelWrapper.Container;

                if (container != null)
                {
                    var selection = View.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

                    var action = new OrderModelObjectAction(View, Project, container, selection, direction);

                    View.ViewportActionManager.ExecuteAction(action);
                }
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
