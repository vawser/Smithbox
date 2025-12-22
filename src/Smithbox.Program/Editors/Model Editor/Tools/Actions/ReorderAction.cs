using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ReorderAction
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ReorderAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MODEL_MoveObjectUp) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }

        // Order (Down)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MODEL_MoveObjectDown) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }

        // Order (Top)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MODEL_MoveObjectTop) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }

        // Order (Bottom)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MODEL_MoveObjectBottom) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Move Up
        if (ImGui.Selectable("Move Up"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }
        UIHelper.Tooltip($"Move the currently selected map objects up by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectUp.HintText}");

        // Move Down
        if (ImGui.Selectable("Move Down"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }
        UIHelper.Tooltip($"Move the currently selected map objects down by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectDown.HintText}");

        // Move Top
        if (ImGui.Selectable("Move to Top"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }
        UIHelper.Tooltip($"Move the currently selected map objects to the top of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectTop.HintText}");

        // Move Bottom
        if (ImGui.Selectable("Move to Bottom"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
        UIHelper.Tooltip($"Move the currently selected map objects to the bottom of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectBottom.HintText}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Move Selected Up in List", KeyBindings.Current.MAP_MoveObjectUp.HintText))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }

        if (ImGui.MenuItem("Move Selected Down in List", KeyBindings.Current.MAP_MoveObjectDown.HintText))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }

        if (ImGui.MenuItem("Move Selected to the List Top", KeyBindings.Current.MAP_MoveObjectTop.HintText))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }

        if (ImGui.MenuItem("Move Selected to the List Bottom", KeyBindings.Current.MAP_MoveObjectBottom.HintText))
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
            if (Editor.Selection.SelectedModelWrapper != null)
            {
                var container = Editor.Selection.SelectedModelWrapper.Container;

                if (container != null)
                {
                    var selection = Editor.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

                    var action = new OrderModelObjectAction(Editor, Project, container, selection, direction);

                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
