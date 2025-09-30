using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Framework.MassEdit;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Core;

public class MapPopupGridPlacement
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public bool GridPlacementPopupOpen = false;

    private bool OpenPopup = false;

    private TargetGrid CurrentTargetGrid = TargetGrid.Primary;

    private bool ApplyPosition_X = false;
    private bool ApplyPosition_Y = true;
    private bool ApplyPosition_Z = false;

    private bool ApplyRotation_X = false;
    private bool ApplyRotation_Y = false;
    private bool ApplyRotation_Z = false;

    public MapPopupGridPlacement(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Show()
    {
        OpenPopup = true;
    }

    public void Display()
    {
        if (OpenPopup)
        {
            ImGui.OpenPopup("GridPlacementConfigurationPopup");
            OpenPopup = false;
        }

        if (ImGui.BeginPopup("GridPlacementConfigurationPopup"))
        {
            GridPlacementPopupOpen = true;

            //if (ImGui.BeginCombo($"##targetGrid", CurrentTargetGrid.GetDisplayName()))
            //{
            //    foreach (var entry in Enum.GetValues(typeof(TargetGrid)))
            //    {
            //        var curEnum = (TargetGrid)entry;

            //        if (ImGui.Selectable($"{curEnum.GetDisplayName()}", CurrentTargetGrid == curEnum))
            //        {
            //            CurrentTargetGrid = curEnum;
            //        }
            //    }

            //    ImGui.EndCombo();
            //}

            UIHelper.SimpleHeader("Position", "Position", "The position axes to use when adjusting to the grid.", UI.Current.ImGui_AliasName_Text);

            ImGui.Checkbox("X##posX", ref ApplyPosition_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##posY", ref ApplyPosition_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##posZ", ref ApplyPosition_Z);

            UIHelper.SimpleHeader("Rotation", "Rotation", "The rotation axes to use when adjusting to the grid.", UI.Current.ImGui_AliasName_Text);

            ImGui.Checkbox("X##rotX", ref ApplyRotation_X);
            ImGui.SameLine();

            ImGui.Checkbox("Y##rotY", ref ApplyRotation_Y);
            ImGui.SameLine();

            ImGui.Checkbox("Z##rotZ", ref ApplyRotation_Z);

            if (ImGui.Button("Adjust Selection to Grid", DPI.WholeWidthButton(550f * DPI.UIScale(), 24)))
            {
                ApplyGridTransform();

                GridPlacementPopupOpen = false;
            }

            ImGui.EndPopup();
        }
        else
        {
            GridPlacementPopupOpen = false;
        }
    }

    public void ApplyGridTransform()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in Editor.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(Editor, false);

                var transform = GetGridTransform(sel);
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

    public Transform GetGridTransform(Entity sel)
    {
        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;
        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (ApplyPosition_X)
        {
            float temp = newPos[0] / CFG.Current.MapEditor_PrimaryGrid_SectionSize;
            float newPosX = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_PrimaryGrid_SectionSize;

            newPos = new Vector3(newPosX, newPos[1], newPos[2]);
        }

        if (ApplyPosition_Y)
        {
            newPos = new Vector3(newPos[0], CFG.Current.MapEditor_PrimaryGrid_Position_Y, newPos[2]);
        }

        if (ApplyPosition_Z)
        {
            float temp = newPos[2] / CFG.Current.MapEditor_PrimaryGrid_SectionSize;
            float newPosZ = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_PrimaryGrid_SectionSize;

            newPos = new Vector3(newPos[0], newPos[1], newPosZ);
        }

        if (ApplyRotation_X)
        {
            var euler = ToEulerDegrees(newRot);
            newRot = FromEulerDegrees(CFG.Current.MapEditor_PrimaryGrid_Rotation_X, euler.Y, euler.Z);
        }

        if (ApplyRotation_Y)
        {
            var euler = ToEulerDegrees(newRot);
            newRot = FromEulerDegrees(euler.X, CFG.Current.MapEditor_PrimaryGrid_Rotation_Y, euler.Z);
        }

        if (ApplyRotation_Z)
        {
            var euler = ToEulerDegrees(newRot);
            newRot = FromEulerDegrees(euler.X, euler.Y, CFG.Current.MapEditor_PrimaryGrid_Rotation_Z);
        }

        newTransform.Position = newPos;
        newTransform.Rotation = newRot;
        newTransform.Scale = newScale;

        return newTransform;
    }

    private enum TargetGrid
    {
        [Display(Name = "Primary")]
        Primary = 0,
        [Display(Name = "Secondary")]
        Secondary = 1,
        [Display(Name = "Tertiary")]
        Tertiary = 2
    }

    public static Quaternion FromEulerDegrees(float pitchDeg, float yawDeg, float rollDeg)
    {
        // Convert to radians
        float pitch = MathF.PI / 180f * pitchDeg; // X
        float yaw = MathF.PI / 180f * yawDeg;   // Y
        float roll = MathF.PI / 180f * rollDeg;  // Z

        // Half angles
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
