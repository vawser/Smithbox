using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class AdjustToGridAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    private bool OpenPopup = false;

    public ViewportTargetGridType CurrentTargetGrid = ViewportTargetGridType.Primary;

    public AdjustToGridAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {
        if (OpenPopup)
        {
            ImGui.OpenPopup("GridPlacementConfigurationPopup");
            OpenPopup = false;
        }

        if (ImGui.BeginPopup("GridPlacementConfigurationPopup"))
        {
            DisplayMenu();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        // Configure Grid Placement
        if (InputManager.IsPressed(KeybindID.MapEditor_Configure_Grid_Placement))
        {
            OpenPopup = true;
        }

        // Adjust to Grid
        if (InputManager.IsPressed(KeybindID.MapEditor_Cycle_Selected_Grid_Type))
        {
            AdjustSelectionToGrid(CurrentTargetGrid);
        }

        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Move_to_Primary_Grid))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Primary);
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Move_to_Secondary_Grid))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Secondary);
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Move_to_Tertiary_Grid))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Tertiary);
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu("Adjust to Grid"))
        {
            if (ImGui.Selectable("Primary"))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Primary);
            }
            GUI.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Move_to_Primary_Grid)}");

            if (ImGui.Selectable("Secondary"))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Secondary);
            }
            GUI.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Move_to_Secondary_Grid)}");

            if (ImGui.Selectable("Tertiary"))
            {
                AdjustSelectionToGrid(ViewportTargetGridType.Tertiary);
            }
            GUI.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Move_to_Tertiary_Grid)}");

            ImGui.EndMenu();
        }

        if (ImGui.Selectable("Configure Grid Placement"))
        {
            OpenPopup = true;
        }
        GUI.Tooltip($"Configure the grid placement for the Adjust to Grid action.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Configure_Grid_Placement)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // Not shown here
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        GUI.SimpleHeader("Target Grid", "Target Grid", "The grid to configure the adjustment against.", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.BeginCombo($"##targetGrid", CurrentTargetGrid.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(ViewportTargetGridType)))
            {
                var curEnum = (ViewportTargetGridType)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", CurrentTargetGrid == curEnum))
                {
                    CurrentTargetGrid = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("The target grid to adjust against.");

        var grid = GetGrid(CurrentTargetGrid);

        GUI.SimpleHeader("Position", "Position", "The position axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

        ImGui.Checkbox("X##posX", ref grid.SnapPosition_X);
        ImGui.SameLine();

        ImGui.Checkbox("Y##posY", ref grid.SnapPosition_Y);
        ImGui.SameLine();

        ImGui.Checkbox("Z##posZ", ref grid.SnapPosition_Z);

        GUI.SimpleHeader("Rotation", "Rotation", "The rotation axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

        ImGui.Checkbox("X##rotX", ref grid.SnapRotation_X);
        ImGui.SameLine();

        ImGui.Checkbox("Y##rotY", ref grid.SnapRotation_Y);
        ImGui.SameLine();

        ImGui.Checkbox("Z##rotZ", ref grid.SnapRotation_Z);

        GUI.SimpleHeader("Root Axis", "Root Axis", "The axis to treat as the 'floor'.", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.BeginCombo($"##targetAxis", grid.RootAxis.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(ViewportGridRootAxis)))
            {
                var curEnum = (ViewportGridRootAxis)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", grid.RootAxis == curEnum))
                {
                    grid.RootAxis = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("The target axis to use when rooting the entity. This is the axis that is treated as the 'floor'.");

        if (ImGui.Button("Adjust Selection to Grid", DPI.WholeWidthButton(550f * DPI.UIScale(), 24)))
        {
            AdjustSelectionToGrid(CurrentTargetGrid);
        }
    }

    private static ViewportGridSettings GetGrid(ViewportTargetGridType targetGrid)
    {
        return ViewportGridConfig.Get(ViewportGridEditor.Map, targetGrid switch
        {
            ViewportTargetGridType.Secondary => ViewportGridType.Secondary,
            ViewportTargetGridType.Tertiary => ViewportGridType.Tertiary,
            _ => ViewportGridType.Primary
        });
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void AdjustSelectionToGrid(ViewportTargetGridType targetGrid)
    {
        ApplyGridTransform(GetGrid(targetGrid));

        View.DelayPicking();
    }

    public void ApplyGridTransform(ViewportGridSettings grid)
    {
        if (View.ViewportSelection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);

                actlist.Add(sel.GetUpdateTransformAction(GetGridTransform(sel, grid)));
            }

            ViewportCompoundAction action = new(actlist);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            Smithbox.LogError<AdjustToGridAction>("No object selected.");
        }
    }

    public Transform GetGridTransform(Entity sel, ViewportGridSettings grid)
    {
        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;

        // The same transform the grid is drawn with, so objects snap to the plane that is shown
        var gridTransform = ViewportGridMovement.GetGridTransform(grid);
        var sectionSize = Math.Max(grid.SectionSize, 0.001f);

        newTransform.Position = SnapPositionToGrid(objT.Position,
            gridTransform.Position,
            gridTransform.Rotation,
            sectionSize,
            grid.RootAxis,
            grid.SnapPosition_X, grid.SnapPosition_Y, grid.SnapPosition_Z);

        newTransform.Rotation = SnapRotationToGrid(objT.Rotation,
            gridTransform.Rotation,
            grid.SnapRotation_X, grid.SnapRotation_Y, grid.SnapRotation_Z);

        newTransform.Scale = objT.Scale;

        return newTransform;
    }

    public Vector3 SnapPositionToGrid(Vector3 position,
        Vector3 gridOrigin,
        Quaternion gridRotation,
        float sectionSize,
        ViewportGridRootAxis curRootAxis,
        bool applyX,
        bool applyY,
        bool applyZ)
    {
        Matrix4x4 gridWorld = Matrix4x4.CreateFromQuaternion(gridRotation) *
                              Matrix4x4.CreateTranslation(gridOrigin);
        Matrix4x4.Invert(gridWorld, out var gridLocal);

        Vector3 localPos = Vector3.Transform(position, gridLocal);

        float snappedX = applyX ? (float)Math.Round(localPos.X / sectionSize) * sectionSize : localPos.X;
        float snappedY = applyY ? (float)Math.Round(localPos.Y / sectionSize) * sectionSize : localPos.Y;
        float snappedZ = applyZ ? (float)Math.Round(localPos.Z / sectionSize) * sectionSize : localPos.Z;

        // Handles the axis that is to be treated as the 'floor'
        if (curRootAxis is ViewportGridRootAxis.X)
        {
            snappedX = applyX ? 0f : localPos.X;
        }
        if (curRootAxis is ViewportGridRootAxis.Y)
        {
            snappedY = applyY ? 0f : localPos.Y;
        }
        if (curRootAxis is ViewportGridRootAxis.Z)
        {
            snappedZ = applyZ ? 0f : localPos.Z;
        }

        Vector3 snappedLocal = new Vector3(snappedX, snappedY, snappedZ);

        return Vector3.Transform(snappedLocal, gridWorld);
    }

    public Quaternion SnapRotationToGrid(Quaternion entityRot,
        Quaternion gridRotation,
        bool applyX,
        bool applyY,
        bool applyZ)
    {
        Matrix4x4 gridMat = Matrix4x4.CreateFromQuaternion(gridRotation);
        Matrix4x4 entityMat = Matrix4x4.CreateFromQuaternion(entityRot);

        Vector3 gridX = new Vector3(gridMat.M11, gridMat.M12, gridMat.M13);
        Vector3 gridY = new Vector3(gridMat.M21, gridMat.M22, gridMat.M23);
        Vector3 gridZ = new Vector3(gridMat.M31, gridMat.M32, gridMat.M33);

        Vector3 entX = new Vector3(entityMat.M11, entityMat.M12, entityMat.M13);
        Vector3 entY = new Vector3(entityMat.M21, entityMat.M22, entityMat.M23);
        Vector3 entZ = new Vector3(entityMat.M31, entityMat.M32, entityMat.M33);

        if (applyX)
            entX = gridX;

        if (applyY)
            entY = gridY;

        if (applyZ)
            entZ = gridZ;

        Matrix4x4 finalMat = new Matrix4x4(
            entX.X, entX.Y, entX.Z, 0,
            entY.X, entY.Y, entY.Z, 0,
            entZ.X, entZ.Y, entZ.Z, 0,
            0, 0, 0, 1);

        return Quaternion.CreateFromRotationMatrix(finalMat);
    }

    public static Quaternion FromEulerDegrees(float pitchDeg, float yawDeg, float rollDeg)
    {
        float pitch = MathF.PI / 180f * pitchDeg;
        float yaw = MathF.PI / 180f * yawDeg;
        float roll = MathF.PI / 180f * rollDeg;

        float cy = MathF.Cos(yaw * 0.5f);
        float sy = MathF.Sin(yaw * 0.5f);
        float cp = MathF.Cos(pitch * 0.5f);
        float sp = MathF.Sin(pitch * 0.5f);
        float cr = MathF.Cos(roll * 0.5f);
        float sr = MathF.Sin(roll * 0.5f);

        Quaternion q;
        q.W = cr * cp * cy + sr * sp * sy;
        q.X = sr * cp * cy - cr * sp * sy;
        q.Y = cr * sp * cy + sr * cp * sy;
        q.Z = cr * cp * sy - sr * sp * cy;

        return q;
    }

    public static Vector3 ToEulerDegrees(Quaternion q)
    {
        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        float roll = MathF.Atan2(sinr_cosp, cosr_cosp);

        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float pitch;
        if (MathF.Abs(sinp) >= 1)
            pitch = MathF.CopySign(MathF.PI / 2, sinp);
        else
            pitch = MathF.Asin(sinp);

        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        float yaw = MathF.Atan2(siny_cosp, cosy_cosp);

        return new Vector3(
            pitch * 180f / MathF.PI,
            yaw * 180f / MathF.PI,
            roll * 180f / MathF.PI
        );
    }
}