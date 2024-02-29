using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class Action_Scramble
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Scramble)
            {
                if (ImGui.Selectable("Scramble##tool_Selection_Scramble", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapToolbar.CurrentTool = SelectedTool.Selection_Scramble;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapToolbar.CurrentTool == SelectedTool.Selection_Scramble)
            {
                ImGui.Text("Scramble the current selection's position, rotation and scale by the following parameters.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Scramble.HintText)}");
                ImGui.Separator();

                var randomOffsetMin_Pos_X = CFG.Current.Scrambler_OffsetMin_Position_X;
                var randomOffsetMin_Pos_Y = CFG.Current.Scrambler_OffsetMin_Position_Y;
                var randomOffsetMin_Pos_Z = CFG.Current.Scrambler_OffsetMin_Position_Z;

                var randomOffsetMax_Pos_X = CFG.Current.Scrambler_OffsetMax_Position_X;
                var randomOffsetMax_Pos_Y = CFG.Current.Scrambler_OffsetMax_Position_Y;
                var randomOffsetMax_Pos_Z = CFG.Current.Scrambler_OffsetMax_Position_Z;

                var randomOffsetMin_Rot_X = CFG.Current.Scrambler_OffsetMin_Rotation_X;
                var randomOffsetMin_Rot_Y = CFG.Current.Scrambler_OffsetMin_Rotation_Y;
                var randomOffsetMin_Rot_Z = CFG.Current.Scrambler_OffsetMin_Rotation_Z;

                var randomOffsetMax_Rot_X = CFG.Current.Scrambler_OffsetMax_Rotation_X;
                var randomOffsetMax_Rot_Y = CFG.Current.Scrambler_OffsetMax_Rotation_Y;
                var randomOffsetMax_Rot_Z = CFG.Current.Scrambler_OffsetMax_Rotation_Z;

                var randomOffsetMin_Scale_X = CFG.Current.Scrambler_OffsetMin_Scale_X;
                var randomOffsetMin_Scale_Y = CFG.Current.Scrambler_OffsetMin_Scale_Y;
                var randomOffsetMin_Scale_Z = CFG.Current.Scrambler_OffsetMin_Scale_Z;

                var randomOffsetMax_Scale_X = CFG.Current.Scrambler_OffsetMax_Scale_X;
                var randomOffsetMax_Scale_Y = CFG.Current.Scrambler_OffsetMax_Scale_Y;
                var randomOffsetMax_Scale_Z = CFG.Current.Scrambler_OffsetMax_Scale_Z;

                // Position
                ImGui.Text("Position");
                ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinPosX", ref randomOffsetMin_Pos_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position X co-ordinate.");

                ImGui.SameLine();

                ImGui.InputFloat("Upper Bound##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position X co-ordinate.");

                ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position Y co-ordinate.");

                ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position Z co-ordinate.");

                // Rotation
                ImGui.Text("Rotation");
                ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinRotX", ref randomOffsetMin_Rot_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation X co-ordinate.");

                ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation Z co-ordinate.");

                // Scale
                ImGui.Text("Scale");
                ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale X co-ordinate.");

                ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Lower Bound##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Upper Bound##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale Z co-ordinate.");

                ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                ImguiUtils.ShowHoverTooltip("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");

                // Clamp floats
                randomOffsetMin_Pos_X = Math.Clamp(randomOffsetMin_Pos_X, -10000f, 10000f);
                randomOffsetMin_Pos_Y = Math.Clamp(randomOffsetMin_Pos_Y, -10000f, 10000f);
                randomOffsetMin_Pos_Z = Math.Clamp(randomOffsetMin_Pos_Z, -10000f, 10000f);

                randomOffsetMax_Pos_X = Math.Clamp(randomOffsetMax_Pos_X, -10000f, 10000f);
                randomOffsetMax_Pos_Y = Math.Clamp(randomOffsetMax_Pos_Y, -10000f, 10000f);
                randomOffsetMax_Pos_Z = Math.Clamp(randomOffsetMax_Pos_Z, -10000f, 10000f);

                randomOffsetMin_Rot_X = Math.Clamp(randomOffsetMin_Rot_X, 0.0f, 360f);
                randomOffsetMin_Rot_Y = Math.Clamp(randomOffsetMin_Rot_Y, 0.0f, 360f);
                randomOffsetMin_Rot_Z = Math.Clamp(randomOffsetMin_Rot_Z, 0.0f, 360f);

                randomOffsetMax_Rot_X = Math.Clamp(randomOffsetMax_Rot_X, 0.0f, 360f);
                randomOffsetMax_Rot_Y = Math.Clamp(randomOffsetMax_Rot_Y, 0.0f, 360f);
                randomOffsetMax_Rot_Z = Math.Clamp(randomOffsetMax_Rot_Z, 0.0f, 360f);

                randomOffsetMin_Scale_X = Math.Clamp(randomOffsetMin_Scale_X, 0.0f, 100f);
                randomOffsetMin_Scale_Y = Math.Clamp(randomOffsetMin_Scale_Y, 0.0f, 100f);
                randomOffsetMin_Scale_Z = Math.Clamp(randomOffsetMin_Scale_Z, 0.0f, 100f);

                randomOffsetMax_Scale_X = Math.Clamp(randomOffsetMax_Scale_X, 0.0f, 100f);
                randomOffsetMax_Scale_Y = Math.Clamp(randomOffsetMax_Scale_Y, 0.0f, 100f);
                randomOffsetMax_Scale_Z = Math.Clamp(randomOffsetMax_Scale_Z, 0.0f, 100f);

                CFG.Current.Scrambler_OffsetMin_Position_X = randomOffsetMin_Pos_X;
                CFG.Current.Scrambler_OffsetMin_Position_Y = randomOffsetMin_Pos_Y;
                CFG.Current.Scrambler_OffsetMin_Position_Z = randomOffsetMin_Pos_Z;

                CFG.Current.Scrambler_OffsetMax_Position_X = randomOffsetMax_Pos_X;
                CFG.Current.Scrambler_OffsetMax_Position_Y = randomOffsetMax_Pos_Y;
                CFG.Current.Scrambler_OffsetMax_Position_Z = randomOffsetMax_Pos_Z;

                CFG.Current.Scrambler_OffsetMin_Rotation_X = randomOffsetMin_Rot_X;
                CFG.Current.Scrambler_OffsetMin_Rotation_Y = randomOffsetMin_Rot_Y;
                CFG.Current.Scrambler_OffsetMin_Rotation_Z = randomOffsetMin_Rot_Z;

                CFG.Current.Scrambler_OffsetMax_Rotation_X = randomOffsetMax_Rot_X;
                CFG.Current.Scrambler_OffsetMax_Rotation_Y = randomOffsetMax_Rot_Y;
                CFG.Current.Scrambler_OffsetMax_Rotation_Z = randomOffsetMax_Rot_Z;

                CFG.Current.Scrambler_OffsetMin_Scale_X = randomOffsetMin_Scale_X;
                CFG.Current.Scrambler_OffsetMin_Scale_Y = randomOffsetMin_Scale_Y;
                CFG.Current.Scrambler_OffsetMin_Scale_Z = randomOffsetMin_Scale_Z;

                CFG.Current.Scrambler_OffsetMax_Scale_X = randomOffsetMax_Scale_X;
                CFG.Current.Scrambler_OffsetMax_Scale_Y = randomOffsetMax_Scale_Y;
                CFG.Current.Scrambler_OffsetMax_Scale_Z = randomOffsetMax_Scale_Z;
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel), true));
            }

            CompoundAction action = new(actlist);
            MapToolbar.ActionManager.ExecuteAction(action);
        }

        public static Transform GetScrambledTransform(Entity sel)
        {
            float posOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_X, CFG.Current.Scrambler_OffsetMax_Position_X);
            float posOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Y, CFG.Current.Scrambler_OffsetMax_Position_Y);
            float posOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Z, CFG.Current.Scrambler_OffsetMax_Position_Z);

            float rotOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_X, CFG.Current.Scrambler_OffsetMax_Rotation_X);
            float rotOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Y, CFG.Current.Scrambler_OffsetMax_Rotation_Y);
            float rotOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Z, CFG.Current.Scrambler_OffsetMax_Rotation_Z);

            float scaleOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_X, CFG.Current.Scrambler_OffsetMax_Scale_X);
            float scaleOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Y, CFG.Current.Scrambler_OffsetMax_Scale_Y);
            float scaleOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Z, CFG.Current.Scrambler_OffsetMax_Scale_Z);

            Transform objT = sel.GetLocalTransform();

            var newTransform = Transform.Default;

            var radianRotateAmount = 0.0f;
            var rot_x = objT.EulerRotation.X;
            var rot_y = objT.EulerRotation.Y;
            var rot_z = objT.EulerRotation.Z;

            var newPos = objT.Position;
            var newRot = objT.Rotation;
            var newScale = objT.Scale;

            if (CFG.Current.Scrambler_RandomisePosition_X)
            {
                newPos = new Vector3(newPos[0] + posOffset_X, newPos[1], newPos[2]);
            }
            if (CFG.Current.Scrambler_RandomisePosition_Y)
            {
                newPos = new Vector3(newPos[0], newPos[1] + posOffset_Y, newPos[2]);
            }
            if (CFG.Current.Scrambler_RandomisePosition_Z)
            {
                newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset_Z);
            }

            newTransform.Position = newPos;

            if (CFG.Current.Scrambler_RandomiseRotation_X)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_X;
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }
            if (CFG.Current.Scrambler_RandomiseRotation_Y)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_Y;
                rot_y = objT.EulerRotation.Y + radianRotateAmount;
            }
            if (CFG.Current.Scrambler_RandomiseRotation_Z)
            {
                radianRotateAmount = (float)Math.PI / 180 * rotOffset_Z;
                rot_z = objT.EulerRotation.Z + radianRotateAmount;
            }

            if (CFG.Current.Scrambler_RandomiseRotation_X || CFG.Current.Scrambler_RandomiseRotation_Y || CFG.Current.Scrambler_RandomiseRotation_Z)
            {
                newTransform.EulerRotation = new Vector3(rot_x, rot_y, rot_z);
            }
            else
            {
                newTransform.Rotation = newRot;
            }

            // If shared scale, the scale randomisation will be the same for X, Y, Z
            if (CFG.Current.Scrambler_RandomiseScale_SharedScale)
            {
                scaleOffset_Y = scaleOffset_X;
                scaleOffset_Z = scaleOffset_X;
            }

            if (CFG.Current.Scrambler_RandomiseScale_X)
            {
                newScale = new Vector3(scaleOffset_X, newScale[1], newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Y)
            {
                newScale = new Vector3(newScale[0], scaleOffset_Y, newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Z)
            {
                newScale = new Vector3(newScale[0], newScale[1], scaleOffset_Z);
            }

            newTransform.Scale = newScale;

            return newTransform;
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
