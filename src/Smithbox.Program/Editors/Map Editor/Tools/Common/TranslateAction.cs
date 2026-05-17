using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class TranslateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public TranslateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
    public void OnShortcut()
    {
        if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Toggle_Discrete_Mode))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_DiscreteApplication = !CFG.Current.MapEditor_Selection_Position_Increment_DiscreteApplication;
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Cycle_Type))
        {
            CycleIncrementType();
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Cycle_Type_Backwards))
        {
            CycleIncrementType(true);
        }

        bool applyTranslate = false;
        bool xMovement_Positive = false;
        bool xMovement_Negative = false;
        bool yMovement_Positive = false;
        bool yMovement_Negative = false;
        bool zMovement_Positive = false;
        bool zMovement_Negative = false;

        if (CFG.Current.MapEditor_Selection_Position_Increment_DiscreteApplication)
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Positive_X))
            {
                xMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Negative_X))
            {
                xMovement_Negative = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Positive_Y))
            {
                yMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Negative_Y))
            {
                yMovement_Negative = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Positive_Z))
            {
                zMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Position_Increment_Negative_Z))
            {
                zMovement_Negative = true;
                applyTranslate = true;
            }
        }
        else
        {
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Positive_X))
            {
                xMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Negative_X))
            {
                xMovement_Negative = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Positive_Y))
            {
                yMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Negative_Y))
            {
                yMovement_Negative = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Positive_Z))
            {
                zMovement_Positive = true;
                applyTranslate = true;
            }
            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_Position_Increment_Negative_Z))
            {
                zMovement_Negative = true;
                applyTranslate = true;
            }
        }

        if (applyTranslate)
        {
            HandleIncrementTranslation(
                xMovement_Positive, xMovement_Negative, 
                yMovement_Positive, yMovement_Negative, 
                zMovement_Positive, zMovement_Negative);
        }
    }

    public void OnContext()
    {
        //if (ImGui.BeginMenu("Translate"))
        //{

        //    ImGui.EndMenu();
        //}
    }

    public void OnMenu()
    {
        //if (ImGui.BeginMenu("Rotation"))
        //{

        //    ImGui.EndMenu();
        //}
    }

    public void OnToolWindow()
    {
        DisplayMenu();
    }

    public void DisplayMenu()
    {
        UIHelper.WrappedText("Use this to incrementally translate a map object.");

        var currentIncrement = "";

        switch (CFG.Current.MapEditor_Selection_Position_IncrementType)
        {
            case 0:
                currentIncrement = $"Position Increment 0: {CFG.Current.MapEditor_Selection_Position_Increment_0}";
                break;
            case 1:
                currentIncrement = $"Position Increment 1: {CFG.Current.MapEditor_Selection_Position_Increment_1}";
                break;
            case 2:
                currentIncrement = $"Position Increment 2: {CFG.Current.MapEditor_Selection_Position_Increment_2}";
                break;
            case 3:
                currentIncrement = $"Position Increment 3: {CFG.Current.MapEditor_Selection_Position_Increment_3}";
                break;
            case 4:
                currentIncrement = $"Position Increment 4: {CFG.Current.MapEditor_Selection_Position_Increment_4}";
                break;
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader($"Current Position Increment: {currentIncrement}", $"Press {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Cycle_Type)} to cycle the position increment used when moving a selection via Keyboard Move.");

        UIHelper.MultiButtonInput("posShiftActions",
            "cycleIncrementType",
            "Cycle Increment",
            $"Press {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Cycle_Type)} to cycle the position increment used when moving a selection via Keyboard Move.",
            CycleIncrementTypeAction);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");

        ImGui.Checkbox("Display position increment type", ref CFG.Current.Viewport_DisplayPositionIncrement);
        UIHelper.Tooltip("Display the current position increment type you are using in the information panel.");

        ImGui.Checkbox("Enable discrete movement", ref CFG.Current.MapEditor_Selection_Position_Increment_DiscreteApplication);
        UIHelper.Tooltip($"If enabled, the key must be pressed and released for each application.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Toggle_Discrete_Mode)}");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Position Increment: 0", "");

        UIHelper.SetInputWidth();
        var unit0 = CFG.Current.MapEditor_Selection_Position_Increment_0;
        if (ImGui.SliderFloat("##positionIncrement0", ref unit0, 0.0f, 999.0f))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_0 = unit0;
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the position increment amount used by keyboard move.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Position Increment: 1", "");

        UIHelper.SetInputWidth();

        var unit1 = CFG.Current.MapEditor_Selection_Position_Increment_1;
        if (ImGui.SliderFloat("##positionIncrement1", ref unit1, 0.0f, 999.0f))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_1 = unit1;
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the position increment amount used by keyboard move.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Position Increment: 2", "");

        UIHelper.SetInputWidth();

        var unit2 = CFG.Current.MapEditor_Selection_Position_Increment_2;
        if (ImGui.SliderFloat("##positionIncrement2", ref unit2, 0.0f, 999.0f))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_2 = unit2;
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the position increment amount used by keyboard move.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Position Increment: 3", "");

        UIHelper.SetInputWidth();

        var unit3 = CFG.Current.MapEditor_Selection_Position_Increment_3;
        if (ImGui.SliderFloat("##positionIncrement3", ref unit3, 0.0f, 999.0f))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_3 = unit3;
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the position increment amount used by keyboard move.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Position Increment: 4", "");

        UIHelper.SetInputWidth();

        var unit4 = CFG.Current.MapEditor_Selection_Position_Increment_4;
        if (ImGui.SliderFloat("##positionIncrement4", ref unit4, 0.0f, 999.0f))
        {
            CFG.Current.MapEditor_Selection_Position_Increment_4 = unit4;
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the position increment amount used by keyboard move.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("incrementalActions",
            "incrementMove_PosX", "Move Selection on +X Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Positive_X)}", MoveSelection_Increment_PositiveX,
            "incrementMove_NegX", "Move Selection on -X Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Negative_X)}", MoveSelection_Increment_NegativeX,
            "incrementMove_PosY", "Move Selection on +Y Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Positive_Y)}", MoveSelection_Increment_PositiveY,
            "incrementMove_NegY", "Move Selection on -Y Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Negative_Y)}", MoveSelection_Increment_NegativeY,
            "incrementMove_PosZ", "Move Selection on +Z Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Positive_Z)}", MoveSelection_Increment_PositiveZ,
            "incrementMove_NegZ", "Move Selection on -Z Axis", $"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Negative_Z)}", MoveSelection_Increment_NegativeZ);

    }

    public void MoveSelection_Increment_PositiveX()
    {
        bool xMovement_Positive = true;
        bool xMovement_Negative = false;
        bool yMovement_Positive = false;
        bool yMovement_Negative = false;
        bool zMovement_Positive = false;
        bool zMovement_Negative = false;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void MoveSelection_Increment_NegativeX()
    {
        bool xMovement_Positive = false;
        bool xMovement_Negative = true;
        bool yMovement_Positive = false;
        bool yMovement_Negative = false;
        bool zMovement_Positive = false;
        bool zMovement_Negative = false;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void MoveSelection_Increment_PositiveY()
    {
        bool xMovement_Positive = false;
        bool xMovement_Negative = false;
        bool yMovement_Positive = true;
        bool yMovement_Negative = false;
        bool zMovement_Positive = false;
        bool zMovement_Negative = false;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void MoveSelection_Increment_NegativeY()
    {
        bool xMovement_Positive = false;
        bool xMovement_Negative = false;
        bool yMovement_Positive = false;
        bool yMovement_Negative = true;
        bool zMovement_Positive = false;
        bool zMovement_Negative = false;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void MoveSelection_Increment_PositiveZ()
    {
        bool xMovement_Positive = false;
        bool xMovement_Negative = false;
        bool yMovement_Positive = false;
        bool yMovement_Negative = false;
        bool zMovement_Positive = true;
        bool zMovement_Negative = false;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void MoveSelection_Increment_NegativeZ()
    {
        bool xMovement_Positive = false;
        bool xMovement_Negative = false;
        bool yMovement_Positive = false;
        bool yMovement_Negative = false;
        bool zMovement_Positive = false;
        bool zMovement_Negative = true;

        HandleIncrementTranslation(
            xMovement_Positive, xMovement_Negative,
            yMovement_Positive, yMovement_Negative,
            zMovement_Positive, zMovement_Negative);
    }

    public void CycleIncrementTypeAction()
    {
        CycleIncrementType();
    }

    public void CycleIncrementType(bool decrement = false)
    {
        if (decrement)
        {
            CFG.Current.MapEditor_Selection_Position_IncrementType -= 1;
            if (CFG.Current.MapEditor_Selection_Position_IncrementType < 0)
            {
                CFG.Current.MapEditor_Selection_Position_IncrementType = 4;
            }
        }
        else
        {
            CFG.Current.MapEditor_Selection_Position_IncrementType += 1;
            if (CFG.Current.MapEditor_Selection_Position_IncrementType > 4)
            {
                CFG.Current.MapEditor_Selection_Position_IncrementType = 0;
            }
        }
    }

    public void HandleIncrementTranslation(
        bool xMovement_Positive = false, bool xMovement_Negative = false,
        bool yMovement_Positive = false, bool yMovement_Negative = false, 
        bool zMovement_Positive = false, bool zMovement_Negative = false)
    {
        var x_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;
        var y_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;
        var z_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;

        switch (CFG.Current.MapEditor_Selection_Position_IncrementType)
        {
            case 0:
                x_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;
                y_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;
                z_increment = CFG.Current.MapEditor_Selection_Position_Increment_0;
                break;
            case 1:
                x_increment = CFG.Current.MapEditor_Selection_Position_Increment_1;
                y_increment = CFG.Current.MapEditor_Selection_Position_Increment_1;
                z_increment = CFG.Current.MapEditor_Selection_Position_Increment_1;
                break;
            case 2:
                x_increment = CFG.Current.MapEditor_Selection_Position_Increment_2;
                y_increment = CFG.Current.MapEditor_Selection_Position_Increment_2;
                z_increment = CFG.Current.MapEditor_Selection_Position_Increment_2;
                break;
            case 3:
                x_increment = CFG.Current.MapEditor_Selection_Position_Increment_3;
                y_increment = CFG.Current.MapEditor_Selection_Position_Increment_3;
                z_increment = CFG.Current.MapEditor_Selection_Position_Increment_3;
                break;
            case 4:
                x_increment = CFG.Current.MapEditor_Selection_Position_Increment_4;
                y_increment = CFG.Current.MapEditor_Selection_Position_Increment_4;
                z_increment = CFG.Current.MapEditor_Selection_Position_Increment_4;
                break;
        }

        List<ViewportAction> actlist = new();
        HashSet<Entity> sels = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

        foreach (Entity sel in sels)
        {
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

            View.ViewportActionManager.ExecuteAction(action);
        }
    }

    public void DisplayViewportHint()
    {
        switch (CFG.Current.MapEditor_Selection_Position_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Position Increment: {CFG.Current.MapEditor_Selection_Position_Increment_0}");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Position Increment: {CFG.Current.MapEditor_Selection_Position_Increment_1}");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Position Increment: {CFG.Current.MapEditor_Selection_Position_Increment_2}");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Position Increment: {CFG.Current.MapEditor_Selection_Position_Increment_3}");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Position Increment: {CFG.Current.MapEditor_Selection_Position_Increment_4}");
                break;
        }
        UIHelper.Tooltip($"Press {InputManager.GetHint(KeybindID.MapEditor_Position_Increment_Cycle_Type)} to cycle the position increment used when moving a selection via Keyboard Move.");
    }
}