using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools;

public class AdjustToGridAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private bool OpenPopup = false;

    public TargetGrid CurrentTargetGrid = TargetGrid.Primary;

    public AdjustToGridAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
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
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ConfigureGridPlacement))
        {
            OpenPopup = true;
        }

        // Adjust to Grid
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_AdjustToGrid_SelectedType))
        {
            AdjustSelectionToGrid(CurrentTargetGrid);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_AdjustToGrid_Primary))
        {
            AdjustSelectionToGrid(TargetGrid.Primary);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_AdjustToGrid_Secondary))
        {
            AdjustSelectionToGrid(TargetGrid.Secondary);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_AdjustToGrid_Tertiary))
        {
            AdjustSelectionToGrid(TargetGrid.Tertiary);
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
                AdjustSelectionToGrid(TargetGrid.Primary);
            }
            UIHelper.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {KeyBindings.Current.MAP_AdjustToGrid_Primary.HintText}");

            if (ImGui.Selectable("Secondary"))
            {
                AdjustSelectionToGrid(TargetGrid.Secondary);
            }
            UIHelper.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {KeyBindings.Current.MAP_AdjustToGrid_Secondary.HintText}");

            if (ImGui.Selectable("Tertiary"))
            {
                AdjustSelectionToGrid(TargetGrid.Tertiary);
            }
            UIHelper.Tooltip($"Adjust the current selection to the grid.\n\nShortcut: {KeyBindings.Current.MAP_AdjustToGrid_Tertiary.HintText}");

            ImGui.EndMenu();
        }

        if (ImGui.Selectable("Configure Grid Placement"))
        {
            OpenPopup = true;
        }
        UIHelper.Tooltip($"Configure the grid placement for the Adjust to Grid action.\n\nShortcut: {KeyBindings.Current.MAP_ConfigureGridPlacement.HintText}");
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
        UIHelper.SimpleHeader("Target Grid", "Target Grid", "The grid to configure the adjustment against.", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.BeginCombo($"##targetGrid", CurrentTargetGrid.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(TargetGrid)))
            {
                var curEnum = (TargetGrid)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", CurrentTargetGrid == curEnum))
                {
                    CurrentTargetGrid = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("The target grid to adjust against.");

        // Primary
        if (CurrentTargetGrid is TargetGrid.Primary)
        {
            var curRootAxis = CFG.Current.MapEditor_PrimaryGrid_Configure_RootAxis;

            UIHelper.SimpleHeader("Position", "Position", "The position axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##posX", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##posY", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##posZ", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_Z);

            UIHelper.SimpleHeader("Rotation", "Rotation", "The rotation axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##rotX", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##rotY", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##rotZ", ref CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_Z);

            UIHelper.SimpleHeader("Root Axis", "Root Axis", "The axis to treat as the 'floor'.", UI.Current.ImGui_Default_Text_Color);

            if (ImGui.BeginCombo($"##targetAxis", curRootAxis.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(RootAxis)))
                {
                    var curEnum = (RootAxis)entry;

                    if (ImGui.Selectable($"{curEnum.GetDisplayName()}", curRootAxis == curEnum))
                    {
                        CFG.Current.MapEditor_PrimaryGrid_Configure_RootAxis = curEnum;
                    }
                }

                ImGui.EndCombo();
            }
            UIHelper.Tooltip("The target axis to use when rooting the entity. This is the axis that is treated as the 'floor'.");
        }
        // Secondary
        else if (CurrentTargetGrid is TargetGrid.Secondary)
        {
            var curRootAxis = CFG.Current.MapEditor_SecondaryGrid_Configure_RootAxis;

            UIHelper.SimpleHeader("Position", "Position", "The position axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##posX", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##posY", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##posZ", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_Z);

            UIHelper.SimpleHeader("Rotation", "Rotation", "The rotation axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##rotX", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##rotY", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##rotZ", ref CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_Z);

            UIHelper.SimpleHeader("Root Axis", "Root Axis", "The axis to treat as the 'floor'.", UI.Current.ImGui_Default_Text_Color);

            if (ImGui.BeginCombo($"##targetAxis", curRootAxis.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(RootAxis)))
                {
                    var curEnum = (RootAxis)entry;

                    if (ImGui.Selectable($"{curEnum.GetDisplayName()}", curRootAxis == curEnum))
                    {
                        CFG.Current.MapEditor_SecondaryGrid_Configure_RootAxis = curEnum;
                    }
                }

                ImGui.EndCombo();
            }
            UIHelper.Tooltip("The target axis to use when rooting the entity. This is the axis that is treated as the 'floor'.");
        }
        // Tertiary
        else if (CurrentTargetGrid is TargetGrid.Tertiary)
        {
            var curRootAxis = CFG.Current.MapEditor_TertiaryGrid_Configure_RootAxis;

            UIHelper.SimpleHeader("Position", "Position", "The position axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##posX", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##posY", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##posZ", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_Z);

            UIHelper.SimpleHeader("Rotation", "Rotation", "The rotation axes to use when adjusting to the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##rotX", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##rotY", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##rotZ", ref CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_Z);

            UIHelper.SimpleHeader("Root Axis", "Root Axis", "The axis to treat as the 'floor'.", UI.Current.ImGui_Default_Text_Color);

            if (ImGui.BeginCombo($"##targetAxis", curRootAxis.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(RootAxis)))
                {
                    var curEnum = (RootAxis)entry;

                    if (ImGui.Selectable($"{curEnum.GetDisplayName()}", curRootAxis == curEnum))
                    {
                        CFG.Current.MapEditor_TertiaryGrid_Configure_RootAxis = curEnum;
                    }
                }

                ImGui.EndCombo();
            }
            UIHelper.Tooltip("The target axis to use when rooting the entity. This is the axis that is treated as the 'floor'.");
        }

        if (ImGui.Button("Adjust Selection to Grid", DPI.WholeWidthButton(550f * DPI.UIScale(), 24)))
        {
            AdjustSelectionToGrid(CurrentTargetGrid);
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void AdjustSelectionToGrid(TargetGrid targetGrid)
    {
        if (targetGrid is TargetGrid.Primary)
        {
            ApplyGridTransform(
                CFG.Current.MapEditor_PrimaryGrid_Configure_RootAxis,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_X,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_Y,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyPosition_Z,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_X,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_Y,
                CFG.Current.MapEditor_PrimaryGrid_Configure_ApplyRotation_Z);
        }
        else if (targetGrid is TargetGrid.Secondary)
        {
            ApplyGridTransform(
                CFG.Current.MapEditor_SecondaryGrid_Configure_RootAxis,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_X,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_Y,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyPosition_Z,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_X,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_Y,
                CFG.Current.MapEditor_SecondaryGrid_Configure_ApplyRotation_Z);
        }
        else if (targetGrid is TargetGrid.Tertiary)
        {
            ApplyGridTransform(
                CFG.Current.MapEditor_TertiaryGrid_Configure_RootAxis,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_X,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_Y,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyPosition_Z,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_X,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_Y,
                CFG.Current.MapEditor_TertiaryGrid_Configure_ApplyRotation_Z);
        }
    }

    public void ApplyGridTransform(RootAxis curRootAxis,
        bool applyPosX, bool applyPosY, bool applyPosZ,
        bool applyRotX, bool applyRotY, bool applyRotZ)
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in Editor.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(Editor, false);

                var transform = GetGridTransform(sel, curRootAxis,
                    applyPosX, applyPosY, applyPosZ,
                    applyRotX, applyRotY, applyRotZ);
                actlist.Add(sel.GetUpdateTransformAction(transform));
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public Transform GetGridTransform(Entity sel,
        RootAxis curRootAxis,
        bool applyPosX, bool applyPosY, bool applyPosZ,
        bool applyRotX, bool applyRotY, bool applyRotZ)
    {
        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;
        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        Vector3 gridOrigin = new Vector3(
            CFG.Current.MapEditor_PrimaryGrid_Position_X,
            CFG.Current.MapEditor_PrimaryGrid_Position_Y,
            CFG.Current.MapEditor_PrimaryGrid_Position_Z);

        Quaternion gridRotation =
            Quaternion.CreateFromYawPitchRoll(
                CFG.Current.MapEditor_PrimaryGrid_Rotation_Y * MathF.PI / 180f,
                CFG.Current.MapEditor_PrimaryGrid_Rotation_X * MathF.PI / 180f,
                CFG.Current.MapEditor_PrimaryGrid_Rotation_Z * MathF.PI / 180f);

        var sectionSize = CFG.Current.MapEditor_PrimaryGrid_SectionSize;

        if (CurrentTargetGrid is TargetGrid.Secondary)
        {
            gridOrigin = new Vector3(
            CFG.Current.MapEditor_SecondaryGrid_Position_X,
            CFG.Current.MapEditor_SecondaryGrid_Position_Y,
            CFG.Current.MapEditor_SecondaryGrid_Position_Z);

            gridRotation =
            Quaternion.CreateFromYawPitchRoll(
                CFG.Current.MapEditor_SecondaryGrid_Rotation_Y * MathF.PI / 180f,
                CFG.Current.MapEditor_SecondaryGrid_Rotation_X * MathF.PI / 180f,
                CFG.Current.MapEditor_SecondaryGrid_Rotation_Z * MathF.PI / 180f);

            sectionSize = CFG.Current.MapEditor_SecondaryGrid_SectionSize;
        }

        if (CurrentTargetGrid is TargetGrid.Tertiary)
        {
            gridOrigin = new Vector3(
            CFG.Current.MapEditor_TertiaryGrid_Position_X,
            CFG.Current.MapEditor_TertiaryGrid_Position_Y,
            CFG.Current.MapEditor_TertiaryGrid_Position_Z);

            gridRotation =
            Quaternion.CreateFromYawPitchRoll(
                CFG.Current.MapEditor_TertiaryGrid_Rotation_Y * MathF.PI / 180f,
                CFG.Current.MapEditor_TertiaryGrid_Rotation_X * MathF.PI / 180f,
                CFG.Current.MapEditor_TertiaryGrid_Rotation_Z * MathF.PI / 180f);


            sectionSize = CFG.Current.MapEditor_TertiaryGrid_SectionSize;
        }

        // Get new position for entity
        newPos = SnapPositionToGrid(newPos,
            gridOrigin,
            gridRotation,
            sectionSize,
            curRootAxis,
            applyPosX, applyPosY, applyPosZ);

        // Get new rotation for entity
        newRot = SnapRotationToGrid(newRot,
            gridRotation,
            applyRotX, applyRotY, applyRotZ);

        newTransform.Position = newPos;
        newTransform.Rotation = newRot;
        newTransform.Scale = newScale;

        return newTransform;
    }

    public Vector3 SnapPositionToGrid(Vector3 position,
        Vector3 gridOrigin,
        Quaternion gridRotation,
        float sectionSize,
        RootAxis curRootAxis,
        bool applyX,
        bool applyY,
        bool applyZ)
    {
        Matrix4x4 gridWorld = Matrix4x4.CreateFromQuaternion(gridRotation) *
                              Matrix4x4.CreateTranslation(gridOrigin);
        Matrix4x4.Invert(gridWorld, out var gridLocal);

        Vector3 localPos = Vector3.Transform(position, gridLocal);

        float snappedX = applyX ? (float)Math.Round(localPos.X / sectionSize) * sectionSize : localPos.X;
        float snappedY = applyZ ? (float)Math.Round(localPos.Y / sectionSize) * sectionSize : localPos.Y;
        float snappedZ = applyZ ? (float)Math.Round(localPos.Z / sectionSize) * sectionSize : localPos.Z;

        // Handles the axis that is to be treated as the 'floor'
        if (curRootAxis is RootAxis.X)
        {
            snappedX = applyX ? 0f : localPos.X;
        }
        if (curRootAxis is RootAxis.Y)
        {
            snappedY = applyY ? 0f : localPos.Y;
        }
        if (curRootAxis is RootAxis.Z)
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