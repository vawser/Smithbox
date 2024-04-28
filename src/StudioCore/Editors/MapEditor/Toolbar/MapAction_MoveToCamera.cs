using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_MoveToCamera
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Move to Camera##tool_Selection_MoveToCamera", MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Camera))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Move_to_Camera;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Camera)
            {
                ImGui.Text("Move the current selection to the camera position.");
                ImGui.Text("");

                ImGui.Text("Camera Offset Distance:");
                if (ImGui.Button("Switch"))
                {
                    CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input = !CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input;
                }
                ImGui.SameLine();
                if (CFG.Current.Toolbar_Move_to_Camera_Offset_Specific_Input)
                {
                    var offset = CFG.Current.Toolbar_Move_to_Camera_Offset;

                    ImGui.PushItemWidth(200);
                    ImGui.InputFloat("##Offset distance", ref offset);
                    ImguiUtils.ShowHoverTooltip("Set the distance at which the current selection is offset from the camera when this action is used.");

                    if (offset < 0)
                        offset = 0;

                    if (offset > 100)
                        offset = 100;

                    CFG.Current.Toolbar_Move_to_Camera_Offset = offset;
                }
                else
                {
                    ImGui.PushItemWidth(200);
                    ImGui.SliderFloat("##Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100);
                    ImguiUtils.ShowHoverTooltip("Set the distance at which the current selection is offset from the camera when this action is used.");
                }
                ImGui.Text("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Camera)
            {
                if (ImGui.Button("Apply##action_Selection_Move_to_Camera", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyMoveToCamera(_selection);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Camera)
            {
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Move_Selection_to_Camera.HintText)}");
            }
        }

        public static void ApplyMoveToCamera(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, MapEditorState.Viewport.WorldView.CameraTransform.RotationMatrix);
            Vector3 camPos = MapEditorState.Viewport.WorldView.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

            // Get the accumulated center position of all selections
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in sels)
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                {
                    // Use bounding box origin as center
                    accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                }
                else
                {
                    // Use actual position as center
                    accumPos += sel.GetRootLocalTransform().Position;
                }
            }

            Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

            // Offset selection positions to place accumulated center in front of camera
            foreach (Entity sel in sels)
            {
                Transform localT = sel.GetLocalTransform();
                Transform rootT = sel.GetRootTransform();

                // Get new localized position by applying reversed root offsets to target camera position.  
                Vector3 newPos = Vector3.Transform(targetCamPos, Quaternion.Inverse(rootT.Rotation))
                                 - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));

                // Offset from center of multiple selections.
                Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                          - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
                Vector3 offsetFromCenter = localCenter - localT.Position;
                newPos -= offsetFromCenter;

                Transform newT = new(newPos, localT.EulerRotation);

                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                MapEditorState.ActionManager.ExecuteAction(action);
            }
        }
    }
}
