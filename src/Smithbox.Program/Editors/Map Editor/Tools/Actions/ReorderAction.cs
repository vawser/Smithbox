using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(OrderMoveDir.Up);
        }

        // Order (Down)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(OrderMoveDir.Down);
        }

        // Order (Top)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(OrderMoveDir.Top);
        }

        // Order (Bottom)
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReorder(OrderMoveDir.Bottom);
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
                ApplyReorder(OrderMoveDir.Up);
            }
            UIHelper.Tooltip($"Move the currently selected map objects up by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectUp.HintText}");

            // Move Down
            if (ImGui.Selectable("Move Down"))
            {
                ApplyReorder(OrderMoveDir.Down);
            }
            UIHelper.Tooltip($"Move the currently selected map objects down by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectDown.HintText}");

            // Move Top
            if (ImGui.Selectable("Move to Top"))
            {
                ApplyReorder(OrderMoveDir.Top);
            }
            UIHelper.Tooltip($"Move the currently selected map objects to the top of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectTop.HintText}");

            // Move Bottom
            if (ImGui.Selectable("Move to Bottom"))
            {
                ApplyReorder(OrderMoveDir.Bottom);
            }
            UIHelper.Tooltip($"Move the currently selected map objects to the bottom of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectBottom.HintText}");

            ImGui.Separator();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Move Selected Up in List", KeyBindings.Current.MAP_MoveObjectUp.HintText))
        {
            ApplyReorder(OrderMoveDir.Up);
        }

        if (ImGui.MenuItem("Move Selected Down in List", KeyBindings.Current.MAP_MoveObjectDown.HintText))
        {
            ApplyReorder(OrderMoveDir.Down);
        }

        if (ImGui.MenuItem("Move Selected to the List Top", KeyBindings.Current.MAP_MoveObjectTop.HintText))
        {
            ApplyReorder(OrderMoveDir.Top);
        }

        if (ImGui.MenuItem("Move Selected to the List Bottom", KeyBindings.Current.MAP_MoveObjectBottom.HintText))
        {
            ApplyReorder(OrderMoveDir.Bottom);
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
    public void ApplyReorder(OrderMoveDir direction)
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

