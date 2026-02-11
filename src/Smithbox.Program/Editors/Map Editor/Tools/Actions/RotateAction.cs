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

namespace StudioCore.Editors.MapEditor;

public class RotateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

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
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Rotate"))
        {
            DisplayMenu();
        }
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();

        UIHelper.SimpleHeader("Rotation Axis", "Rotation Axis", "The axis of rotation to use.", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
        {
            CFG.Current.Toolbar_Rotate_Y = false;
            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
            CFG.Current.Toolbar_Fixed_Rotate = false;
        }
        UIHelper.Tooltip("Set the rotation axis to X.");

        ImGui.SameLine();

        if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
        {
            CFG.Current.Toolbar_Rotate_X = false;
            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
            CFG.Current.Toolbar_Fixed_Rotate = false;
        }
        UIHelper.Tooltip("Set the rotation axis to Y.");

        ImGui.SameLine();

        if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
        {
            CFG.Current.Toolbar_Rotate_Y = false;
            CFG.Current.Toolbar_Rotate_X = false;
            CFG.Current.Toolbar_Fixed_Rotate = false;
        }
        UIHelper.Tooltip("Set the rotation axis to Y and pivot with respect to others within the selection.");

        ImGui.SameLine();

        if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
        {
            CFG.Current.Toolbar_Rotate_Y = false;
            CFG.Current.Toolbar_Rotate_X = false;
            CFG.Current.Toolbar_Rotate_Y_Pivot = false;
        }
        UIHelper.Tooltip("Set the rotation axis to specified values below.");

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

        UIHelper.WrappedText("");

        if (ImGui.Button("Rotate Selection", DPI.WholeWidthButton(windowWidth, 24)))
        {
            ApplyRotation();
        }
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
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
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
                radianRotateAmount = View.RotationIncrementTool.GetRadianRotateAmount();
                // Makes radian rotate amount negative if axis X argument is negative
                radianRotateAmount = (axis.X < 0) ? -radianRotateAmount : radianRotateAmount;
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }

            if (axis.Y != 0)
            {
                radianRotateAmount = View.RotationIncrementTool.GetRadianRotateAmount();
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
}