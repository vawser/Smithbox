using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class RotateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public RotationAxis RotationAxis = RotationAxis.X;

    public RotateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        // Rotate (X-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_X_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
        }

        // Rotate (Y-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Y_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
        }

        // Rotate Pivot (Y-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Pivot_Y_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
        }

        // Negative Rotate (X-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Minus_X_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
        }

        // Negative Rotate (Y-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Minus_Y_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
        }

        // Negative Rotate Pivot (Y-axis)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis))
        {
            ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
        }

        // Rotate (Fixed Increment)
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotate_Fixed_Angle))
        {
            SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
        }

        // Reset Rotation
        if (InputManager.IsPressed(KeybindID.MapEditor_Reset_Rotation))
        {
            SetSelectionToFixedRotation(new Vector3(0, 0, 0));
        }

        // Only apply in Map Editor screen
        if (InputManager.IsPressed(KeybindID.MapEditor_Rotation_Increment_Cycle_Type))
        {
            CycleIncrementType();
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Rotation_Increment_Cycle_Type_Backwards))
        {
            CycleIncrementType(true);
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu("Rotation"))
        {
            // Rotate (X-axis)
            if (ImGui.MenuItem("Rotate Selection (+ x-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_X_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            // Negative Rotate (X-axis)
            if (ImGui.MenuItem("Rotate Selection (- x-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            // Rotate (Y-axis)
            if (ImGui.MenuItem("Rotate Selection (+ y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Pivot_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            // Negative Rotate (Y-axis)
            if (ImGui.MenuItem("Rotate Selection (- y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_X_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            // Rotate Pivot (Y-axis)
            if (ImGui.MenuItem("Pivot Selection (+ y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            // Negative Rotate Pivot (Y-axis)
            if (ImGui.MenuItem("Pivot Selection (- y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
            }

            // Rotate Fixed Angle
            if (ImGui.MenuItem("Rotate Selection (fixed angle)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Fixed_Angle)))
            {
                SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            // Reset Rotation
            if (ImGui.MenuItem("Reset Selected Rotation", InputManager.GetHint(KeybindID.MapEditor_Reset_Rotation)))
            {
                SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if(ImGui.BeginMenu("Rotation"))
        {
            // Rotate (X-axis)
            if (ImGui.MenuItem("Rotate Selection (+ x-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_X_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            // Negative Rotate (X-axis)
            if (ImGui.MenuItem("Rotate Selection (- x-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_X_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            // Rotate (Y-axis)
            if (ImGui.MenuItem("Rotate Selection (+ y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            // Negative Rotate (Y-axis)
            if (ImGui.MenuItem("Rotate Selection (- y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            // Rotate Pivot (Y-axis)
            if (ImGui.MenuItem("Pivot Selection (+ y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Pivot_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            // Negative Rotate Pivot (Y-axis)
            if (ImGui.MenuItem("Pivot Selection (- y-axis)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Minus_Pivot_Y_Axis)))
            {
                ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
            }

            // Rotate Fixed Increment
            if (ImGui.MenuItem("Rotate Selection (fixed angle)", InputManager.GetHint(KeybindID.MapEditor_Rotate_Fixed_Angle)))
            {
                SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            // Reset Rotation
            if (ImGui.MenuItem("Reset Selection Rotation", InputManager.GetHint(KeybindID.MapEditor_Reset_Rotation)))
            {
                SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        DisplayMenu();
    }

    public void DisplayMenu()
    {
        UIHelper.WrappedText("Use this to incrementally rotate a map object, and to configure how the rotate shortcut functions.");

        var currentIncrement = "";

        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                currentIncrement = $"Degree Increment 0: {CFG.Current.Toolbar_Rotate_Increment_0}°";
                break;
            case 1:
                currentIncrement = $"Degree Increment 1: {CFG.Current.Toolbar_Rotate_Increment_1}°";
                break;
            case 2:
                currentIncrement = $"Degree Increment 2: {CFG.Current.Toolbar_Rotate_Increment_2}°";
                break;
            case 3:
                currentIncrement = $"Degree Increment 3: {CFG.Current.Toolbar_Rotate_Increment_3}°";
                break;
            case 4:
                currentIncrement = $"Degree Increment 4: {CFG.Current.Toolbar_Rotate_Increment_4}°";
                break;
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader($"Current Rotation Increment: {currentIncrement}", $"Press {InputManager.GetHint(KeybindID.MapEditor_Rotation_Increment_Cycle_Type)} to cycle the degree increment used by Rotate Selection on X/Y Axis.");

        UIHelper.MultiButtonInput("rotShiftActions",
            "cycleIncrementType",
            "Cycle Increment",
            $"Press {InputManager.GetHint(KeybindID.MapEditor_Rotation_Increment_Cycle_Type)} to cycle the degree increment used by Rotate Selection on X/Y Axis.",
            CycleIncrementTypeAction);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");

        ImGui.Checkbox("Display rotation increment in viewport", ref CFG.Current.Viewport_DisplayRotationIncrement);
        UIHelper.Tooltip("Display the current degree increment type you are using in the information panel.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Axis", "The axis of rotation to use.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##rotationAxisSelect", RotationAxis.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(RotationAxis)))
            {
                var axisType = (RotationAxis)entry;

                if (ImGui.Selectable(axisType.GetDisplayName(), axisType == RotationAxis))
                {
                    RotationAxis = axisType;

                    switch(RotationAxis)
                    {
                        case RotationAxis.X:
                            CFG.Current.Toolbar_Rotate_X = true;
                            CFG.Current.Toolbar_Rotate_Y = false;
                            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                            CFG.Current.Toolbar_Fixed_Rotate = false;
                            break;
                        case RotationAxis.Y:
                            CFG.Current.Toolbar_Rotate_X = false;
                            CFG.Current.Toolbar_Rotate_Y = true;
                            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                            CFG.Current.Toolbar_Fixed_Rotate = false;
                            break;
                        case RotationAxis.Y_Pivot:
                            CFG.Current.Toolbar_Rotate_X = false;
                            CFG.Current.Toolbar_Rotate_Y = false;
                            CFG.Current.Toolbar_Rotate_Y_Pivot = true;
                            CFG.Current.Toolbar_Fixed_Rotate = false;
                            break;
                        case RotationAxis.Fixed_Angle:
                            CFG.Current.Toolbar_Rotate_X = false;
                            CFG.Current.Toolbar_Rotate_Y = false;
                            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                            CFG.Current.Toolbar_Fixed_Rotate = true;
                            break;
                    }
                }
            }

            ImGui.EndCombo();
        }

        if (CFG.Current.Toolbar_Fixed_Rotate)
        {
            var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
            var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
            var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

            UIHelper.SimpleHeader("Fixed Axis", "Fixed Axis", "The axis of rotation to use for the fixed rotation action.", UI.Current.ImGui_Default_Text_Color);

            DPI.ApplyInputWidth(100f);
            if (ImGui.InputFloat("X##fixedRotationX", ref x))
            {
                x = Math.Clamp(x, -360f, 360f);
            }
            UIHelper.Tooltip("Set the X component of the fixed rotation action.");

            DPI.ApplyInputWidth(100f);
            if (ImGui.InputFloat("Y##fixedRotationX", ref y))
            {
                y = Math.Clamp(y, -360f, 360f);
            }
            UIHelper.Tooltip("Set the Y component of the fixed rotation action.");

            DPI.ApplyInputWidth(100f);
            if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
            {
                z = Math.Clamp(z, -360f, 360f);
            }
            UIHelper.Tooltip("Set the Z component of the fixed rotation action.");

            CFG.Current.Toolbar_Rotate_FixedAngle = new Vector3(x, y, z);
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Increment: 0", "");

        UIHelper.SetInputWidth();

        var rot0 = CFG.Current.Toolbar_Rotate_Increment_0;
        if (ImGui.SliderFloat("##degreeIncrement0", ref rot0, -360.0f, 360.0f))
        {
            CFG.Current.Toolbar_Rotate_Increment_0 = Math.Clamp(rot0, -360.0f, 360.0f);
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Increment: 1", "");

        UIHelper.SetInputWidth();

        var rot1 = CFG.Current.Toolbar_Rotate_Increment_1;
        if (ImGui.SliderFloat("##degreeIncrement1", ref rot1, -360.0f, 360.0f))
        {
            CFG.Current.Toolbar_Rotate_Increment_1 = Math.Clamp(rot1, -360.0f, 360.0f);
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Increment: 2", "");

        UIHelper.SetInputWidth();

        var rot2 = CFG.Current.Toolbar_Rotate_Increment_2;
        if (ImGui.SliderFloat("##degreeIncrement2", ref rot2, -360.0f, 360.0f))
        {
            CFG.Current.Toolbar_Rotate_Increment_2 = Math.Clamp(rot2, -360.0f, 360.0f);
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Increment: 3", "");

        UIHelper.SetInputWidth();

        var rot3 = CFG.Current.Toolbar_Rotate_Increment_3;
        if (ImGui.SliderFloat("##degreeIncrement3", ref rot3, -360.0f, 360.0f))
        {
            CFG.Current.Toolbar_Rotate_Increment_3 = Math.Clamp(rot3, -360.0f, 360.0f);
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Rotation Increment: 4", "");

        UIHelper.SetInputWidth();

        var rot4 = CFG.Current.Toolbar_Rotate_Increment_4;
        if (ImGui.SliderFloat("##degreeIncrement4", ref rot4, -360.0f, 360.0f))
        {
            CFG.Current.Toolbar_Rotate_Increment_4 = Math.Clamp(rot4, -360.0f, 360.0f);
        }
        UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("rotateActions",
            "applyRotate", "Apply Rotation to Selection", "", ApplyRotation);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyRotation()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (CFG.Current.Toolbar_Rotate_X)
            {
                ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y)
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y_Pivot)
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }
            if (CFG.Current.Toolbar_Fixed_Rotate)
            {
                SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }
        }
        else
        {
            Smithbox.LogError<RotateAction>("No object selected.");
        }

        View.DelayPicking();
    }

    public void ArbitraryRotation_Selection(Vector3 axis, bool pivot)
    {
        List<ViewportAction> actlist = new();
        HashSet<Entity> sels = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

        // Get the center position of the selections
        Vector3 accumPos = Vector3.Zero;
        foreach (Entity sel in sels)
        {
            accumPos += sel.GetLocalTransform().Position;
        }

        Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

        foreach (Entity s in sels)
        {
            Transform objT = s.GetLocalTransform();

            var radianRotateAmount = 0.0f;
            var rot_x = objT.EulerRotation.X;
            var rot_y = objT.EulerRotation.Y;
            var rot_z = objT.EulerRotation.Z;

            var newPos = Transform.Default;

            if (axis.X != 0)
            {
                radianRotateAmount = GetRadianRotateAmount();
                // Makes radian rotate amount negative if axis X argument is negative
                radianRotateAmount = (axis.X < 0) ? -radianRotateAmount : radianRotateAmount;
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }

            if (axis.Y != 0)
            {
                radianRotateAmount = GetRadianRotateAmount();
                // Makes radian rotate amount negative if axis Y argument is negative
                radianRotateAmount = (axis.Y < 0) ? -radianRotateAmount : radianRotateAmount;
                rot_y = objT.EulerRotation.Y + radianRotateAmount;
            }

            if (pivot)
            {
                newPos = Utils.RotateVectorAboutPoint(objT.Position, centerT.Position, Vector3.Abs(axis), radianRotateAmount);
            }
            else
            {
                newPos.Position = objT.Position;
            }

            newPos.EulerRotation = new Vector3(rot_x, rot_y, rot_z);

            actlist.Add(s.GetUpdateTransformAction(newPos));
        }

        if (actlist.Any())
        {
            ViewportCompoundAction action = new(actlist);

            View.ViewportActionManager.ExecuteAction(action);
        }
    }

    public void SetSelectionToFixedRotation(Vector3 newRotation)
    {
        List<ViewportAction> actlist = new();

        HashSet<Entity> selected = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);
        foreach (Entity s in selected)
        {
            Vector3 pos = s.GetLocalTransform().Position;
            Transform newRot = new(pos, newRotation);

            actlist.Add(s.GetUpdateTransformAction(newRot));
        }

        if (actlist.Any())
        {
            ViewportCompoundAction action = new(actlist);

            View.ViewportActionManager.ExecuteAction(action);
        }
    }

    public void CycleIncrementTypeAction()
    {
        CycleIncrementType();
    }
    public void CycleIncrementType(bool decrement = false)
    {
        if (decrement)
        {
            CFG.Current.Toolbar_Rotate_IncrementType -= 1;
            if (CFG.Current.Toolbar_Rotate_IncrementType < 0)
            {
                CFG.Current.Toolbar_Rotate_IncrementType = 4;
            }
        }
        else
        {
            CFG.Current.Toolbar_Rotate_IncrementType += 1;
            if (CFG.Current.Toolbar_Rotate_IncrementType > 4)
            {
                CFG.Current.Toolbar_Rotate_IncrementType = 0;
            }
        }
    }

    public void DisplayViewportHint()
    {
        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_0}°");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_1}°");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_2}°");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_3}°");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_4}°");
                break;
        }
        UIHelper.Tooltip($"Press {InputManager.GetHint(KeybindID.MapEditor_Rotation_Increment_Cycle_Type)} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
    }

    public float GetRadianRotateAmount()
    {
        var amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_0;

        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_0;
                break;
            case 1:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_1;
                break;
            case 2:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_2;
                break;
            case 3:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_3;
                break;
            case 4:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_4;
                break;
        }

        return amount;
    }
}

public enum RotationAxis
{
    [Display(Name = "X")]
    X,
    [Display(Name = "Y")]
    Y,
    [Display(Name = "Y Pivot")]
    Y_Pivot,
    [Display(Name = "Fixed Angle")]
    Fixed_Angle
}