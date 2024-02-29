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
    public static class MapAction_Rotate
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Rotate)
            {
                if (ImGui.Selectable("Rotate##tool_Selection_Rotate", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapEditorState.CurrentTool = SelectedTool.Selection_Rotate;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Rotate)
            {
                ImGui.Text("Rotate the current selection by the following parameters.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_X.HintText)} for Rotate X");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y.HintText)} for Rotate Y");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Rotate_Y_Pivot.HintText)} for Rotate Pivot Y");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Reset_Rotation.HintText)} for Fixed Rotation");
                ImGui.Separator();

                var rot = CFG.Current.Toolbar_Rotate_Increment;

                if (ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to X.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                {
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to Y.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to Y and pivot with respect to others within the selection.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to specified values below.");

                if (ImGui.Button("Switch"))
                {
                    CFG.Current.Toolbar_Rotate_Specific_Input = !CFG.Current.Toolbar_Rotate_Specific_Input;
                }
                ImGui.SameLine();

                if (CFG.Current.Toolbar_Rotate_Specific_Input)
                {
                    ImGui.PushItemWidth(200);
                    if (ImGui.InputFloat("Degree Increment", ref rot))
                    {
                        CFG.Current.Toolbar_Rotate_Increment = Math.Clamp(rot, -180.0f, 180.0f);
                    }
                }
                else
                {
                    ImGui.PushItemWidth(200);
                    ImGui.SliderFloat("Degree Increment", ref rot, -180.0f, 180.0f);
                }
                ImguiUtils.ShowHoverTooltip("Set the angle increment amount used by the rotation.");

                var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
                var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
                var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

                ImGui.Text("Fixed Rotation");
                ImGui.PushItemWidth(100);
                if (ImGui.InputFloat("X##fixedRotationX", ref x))
                {
                    x = Math.Clamp(x, -360f, 360f);
                }
                ImguiUtils.ShowHoverTooltip("Set the X component of the fixed rotation action.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                {
                    y = Math.Clamp(y, -360f, 360f);
                }
                ImguiUtils.ShowHoverTooltip("Set the Y component of the fixed rotation action.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                {
                    z = Math.Clamp(z, -360f, 360f);
                }
                ImguiUtils.ShowHoverTooltip("Set the Z component of the fixed rotation action.");

                ImGui.SameLine();

                CFG.Current.Toolbar_Rotate_FixedAngle = new Vector3(x, y, z);
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Rotate_X)
            {
                ArbitraryRotation_Selection(_selection, new Vector3(1, 0, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y)
            {
                ArbitraryRotation_Selection(_selection, new Vector3(0, 1, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y_Pivot)
            {
                ArbitraryRotation_Selection(_selection, new Vector3(0, 1, 0), true);
            }
            if (CFG.Current.Toolbar_Fixed_Rotate)
            {
                SetSelectionToFixedRotation(_selection);
            }
        }

        public static void ArbitraryRotation_Selection(ViewportSelection _selection, Vector3 axis, bool pivot)
        {
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

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
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment;
                    rot_x = objT.EulerRotation.X + radianRotateAmount;
                }

                if (axis.Y != 0)
                {
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment;
                    rot_y = objT.EulerRotation.Y + radianRotateAmount;
                }

                if (pivot)
                {
                    newPos = Utils.RotateVectorAboutPoint(objT.Position, centerT.Position, axis, radianRotateAmount);
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
                CompoundAction action = new(actlist);
                MapEditorState.ActionManager.ExecuteAction(action);
            }
        }

        /// <summary>
        /// Set current selection to fixed rotation.
        /// </summary>
        public static void SetSelectionToFixedRotation(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();

            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);
            foreach (Entity s in selected)
            {
                Vector3 pos = s.GetLocalTransform().Position;
                Transform newRot = new(pos, CFG.Current.Toolbar_Rotate_FixedAngle);

                actlist.Add(s.GetUpdateTransformAction(newRot));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                MapEditorState.ActionManager.ExecuteAction(action);
            }
        }
    }
}
