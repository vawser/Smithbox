using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace StudioCore.Editors.MapEditor;

public class MovementCycleConfigTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public MovementCycleConfigTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcuts
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_ToggleDiscreteMovement))
        {
            CFG.Current.MapEditor_Selection_Movement_DiscreteApplication = !CFG.Current.MapEditor_Selection_Movement_DiscreteApplication;
        }

        if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement) || InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrementBackward))
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement))
            {
                CycleIncrementType();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrementBackward))
            {
                CycleIncrementType(true);
            }
        }

        var x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
        var y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
        var z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;

        switch (CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                break;
            case 1:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                break;
            case 2:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                break;
            case 3:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                break;
            case 4:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                break;
        }

        List<ViewportAction> actlist = new();
        HashSet<Entity> sels = Editor.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

        foreach (Entity sel in sels)
        {
            bool xMovement_Positive = false;
            bool xMovement_Negative = false;
            bool yMovement_Positive = false;
            bool yMovement_Negative = false;
            bool zMovement_Positive = false;
            bool zMovement_Negative = false;

            if (CFG.Current.MapEditor_Selection_Movement_DiscreteApplication)
            {
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_PositiveX))
                {
                    xMovement_Positive = true;
                }
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_NegativeX))
                {
                    xMovement_Negative = true;
                }
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_PositiveY))
                {
                    yMovement_Positive = true;
                }
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_NegativeY))
                {
                    yMovement_Negative = true;
                }
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_PositiveZ))
                {
                    zMovement_Positive = true;
                }
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_NegativeZ))
                {
                    zMovement_Negative = true;
                }
            }
            else
            {
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveX))
                {
                    xMovement_Positive = true;
                }
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeX))
                {
                    xMovement_Negative = true;
                }
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveY))
                {
                    yMovement_Positive = true;
                }
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeY))
                {
                    yMovement_Negative = true;
                }
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveZ))
                {
                    zMovement_Positive = true;
                }
                if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeZ))
                {
                    zMovement_Negative = true;
                }
            }

            Transform localT = sel.GetLocalTransform();

            // X
            if (xMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X + x_increment, position.Y, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (xMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X - x_increment, position.Y, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            // Y
            if (yMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y + y_increment, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (yMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y - y_increment, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            // Z
            if (zMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y, position.Z + z_increment);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (zMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y, position.Z - z_increment);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
        }

        if (actlist.Any())
        {
            ViewportCompoundAction action = new(actlist);
            Editor.EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Movement Increments"))
        {
            UIHelper.SimpleHeader("Current Movement Increment", "Current Movement Increment", $"Shortcut: {KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement.HintText}", UI.Current.ImGui_Default_Text_Color);

            Editor.MovementCycleConfigTool.DisplayCurrentMovementIncrement();

            ImGui.Checkbox("Display movement increment type", ref CFG.Current.Viewport_DisplayMovementIncrement);
            UIHelper.Tooltip("Display the current movement increment type you are using in the information panel.");

            if (ImGui.Button("Cycle Increment", DPI.WholeWidthButton(windowWidth, 24)))
            {
                Editor.MovementCycleConfigTool.CycleIncrementType();
            }
            UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");

            UIHelper.SimpleHeader("Movement Increment [0]", "Movement Increment [0]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var unit0 = CFG.Current.MapEditor_Selection_Movement_Increment_0;
            if (ImGui.SliderFloat("##movementIncrement0", ref unit0, 0.0f, 999.0f))
            {
                CFG.Current.MapEditor_Selection_Movement_Increment_0 = unit0;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

            UIHelper.SimpleHeader("Movement Increment [1]", "Movement Increment [1]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var unit1 = CFG.Current.MapEditor_Selection_Movement_Increment_1;
            if (ImGui.SliderFloat("##movementIncrement1", ref unit1, 0.0f, 999.0f))
            {
                CFG.Current.MapEditor_Selection_Movement_Increment_1 = unit1;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

            UIHelper.SimpleHeader("Movement Increment [2]", "Movement Increment [2]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var unit2 = CFG.Current.MapEditor_Selection_Movement_Increment_2;
            if (ImGui.SliderFloat("##movementIncrement2", ref unit2, 0.0f, 999.0f))
            {
                CFG.Current.MapEditor_Selection_Movement_Increment_2 = unit2;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

            UIHelper.SimpleHeader("Movement Increment [3]", "Movement Increment [3]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var unit3 = CFG.Current.MapEditor_Selection_Movement_Increment_3;
            if (ImGui.SliderFloat("##movementIncrement3", ref unit3, 0.0f, 999.0f))
            {
                CFG.Current.MapEditor_Selection_Movement_Increment_3 = unit3;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

            UIHelper.SimpleHeader("Movement Increment [4]", "Movement Increment [4]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var unit4 = CFG.Current.MapEditor_Selection_Movement_Increment_4;
            if (ImGui.SliderFloat("##movementIncrement4", ref unit4, 0.0f, 999.0f))
            {
                CFG.Current.MapEditor_Selection_Movement_Increment_4 = unit4;
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

            ImGui.Checkbox("Enable discrete movement", ref CFG.Current.MapEditor_Selection_Movement_DiscreteApplication);
            UIHelper.Tooltip($"If enabled, the key must be pressed and released for each application.\nShortcut: {KeyBindings.Current.MAP_KeyboardMove_ToggleDiscreteMovement.HintText}");
        }
    }

    public void CycleIncrementType(bool decrement = false)
    {
        if(decrement)
        {
            CFG.Current.MapEditor_Selection_Movement_IncrementType -= 1;
            if (CFG.Current.MapEditor_Selection_Movement_IncrementType < 0)
            {
                CFG.Current.MapEditor_Selection_Movement_IncrementType = 4;
            }
        }
        else
        {
            CFG.Current.MapEditor_Selection_Movement_IncrementType += 1;
            if (CFG.Current.MapEditor_Selection_Movement_IncrementType > 4)
            {
                CFG.Current.MapEditor_Selection_Movement_IncrementType = 0;
            }
        }
    }

    public void DisplayViewportMovementIncrement()
    {
        switch (CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_0}");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_1}");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_2}");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_3}");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_4}");
                break;
        }
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");
    }

    public void DisplayCurrentMovementIncrement()
    {
        switch (CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment [0]: {CFG.Current.MapEditor_Selection_Movement_Increment_0}");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment [1]: {CFG.Current.MapEditor_Selection_Movement_Increment_1}");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment [2]: {CFG.Current.MapEditor_Selection_Movement_Increment_2}");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment [3]: {CFG.Current.MapEditor_Selection_Movement_Increment_3}");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Movement Increment [4]: {CFG.Current.MapEditor_Selection_Movement_Increment_4}");
                break;
        }
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleMovementIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");
    }
}
