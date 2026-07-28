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
        // Move Up
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Reorder_Up_Title")}##moveUpAction"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Reorder_Up_Context_TT", InputManager.GetHint(KeybindID.Reorder_Up)));

        // Move Down
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Reorder_Down_Title")}##moveDownAction"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Reorder_Down_Context_TT", InputManager.GetHint(KeybindID.Reorder_Down)));

        // Move to Top
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Reorder_Top_Title")}##moveToTopAction"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Reorder_Top_Context_TT", InputManager.GetHint(KeybindID.Reorder_Top)));

        // Move to Bottom
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Reorder_Bottom_Title")}##moveToBottomAction"))
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Reorder_Bottom_Context_TT", InputManager.GetHint(KeybindID.Reorder_Bottom)));
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Move Up
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Reorder_Up_Title")}##moveUpAction", InputManager.GetHint(KeybindID.Reorder_Up)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Up);
        }

        // Move Down
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Reorder_Down_Title")}##moveDownAction", InputManager.GetHint(KeybindID.Reorder_Down)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Down);
        }

        // Move to Top
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Reorder_Top_Title")}##moveToTopAction", InputManager.GetHint(KeybindID.Reorder_Top)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Top);
        }

        // Move to Bottom
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Reorder_Bottom_Title")}##moveToBottomAction", InputManager.GetHint(KeybindID.Reorder_Bottom)))
        {
            ApplyReorder(TreeObjectOrderMovementType.Bottom);
        }
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
            Smithbox.LogError<ReorderAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}
