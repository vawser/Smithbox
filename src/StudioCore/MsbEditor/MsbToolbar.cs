using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;
using System.IO;
using System;
using System.Threading;
using static SoulsFormats.ACB;
using DotNext;
using Silk.NET.SDL;
using Veldrid.Utilities;

namespace StudioCore.MsbEditor
{
    public class MsbToolbar
    {
        private readonly ActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly Selection _selection;

        private AssetLocator _assetLocator;
        private MsbEditorScreen _msbEditor;

        private Universe _universe;

        private IViewport _viewport;

        public MsbToolbar(RenderScene scene, Selection sel, ActionManager manager, Universe universe, AssetLocator locator, MsbEditorScreen editor, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _assetLocator = locator;
            _msbEditor = editor;
            _viewport = viewport;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (_assetLocator.Type == GameType.Undefined)
                return;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Toolbar##MsbMenubar"))
            {
                // Button Bar
                if (ImGui.Button("Controls##ControlsToggle"))
                {
                    CFG.Current.Toolbar_ShowControlsMenu = true;
                    CFG.Current.Toolbar_ShowToolsMenu = false;
                    CFG.Current.Toolbar_ShowScramblerMenu = false;
                    CFG.Current.Toolbar_ShowReplicatorMenu = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Tools##ToolsToggle"))
                {
                    CFG.Current.Toolbar_ShowControlsMenu = false;
                    CFG.Current.Toolbar_ShowToolsMenu = true;
                    CFG.Current.Toolbar_ShowScramblerMenu = false;
                    CFG.Current.Toolbar_ShowReplicatorMenu = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Scrambler##ScramblerToggle"))
                {
                    CFG.Current.Toolbar_ShowControlsMenu = false;
                    CFG.Current.Toolbar_ShowToolsMenu = false;
                    CFG.Current.Toolbar_ShowScramblerMenu = true;
                    CFG.Current.Toolbar_ShowReplicatorMenu = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Replicator##ReplicatorToggle"))
                {
                    CFG.Current.Toolbar_ShowControlsMenu = false;
                    CFG.Current.Toolbar_ShowToolsMenu = false;
                    CFG.Current.Toolbar_ShowScramblerMenu = false;
                    CFG.Current.Toolbar_ShowReplicatorMenu = true;
                }

                // Controls
                if (CFG.Current.Toolbar_ShowControlsMenu)
                {
                    ImGui.Separator();

                    ImGui.Text($"Hold right click in viewport to activate camera mode");
                    ImGui.Text($"Forward: {KeyBindings.Current.Viewport_Cam_Forward.HintText}\n" +
                        $"Left: {KeyBindings.Current.Viewport_Cam_Left.HintText}\n" +
                        $"Back: {KeyBindings.Current.Viewport_Cam_Back.HintText}\n" +
                        $"Right: {KeyBindings.Current.Viewport_Cam_Right.HintText}\n" +
                        $"Up: {KeyBindings.Current.Viewport_Cam_Up.HintText}\n" +
                        $"Down: {KeyBindings.Current.Viewport_Cam_Down.HintText}\n" +
                        $"Fast cam: Shift\n" +
                        $"Slow cam: Ctrl\n" +
                        $"Tweak speed: Mouse wheel");
                }

                // Tools
                if (CFG.Current.Toolbar_ShowToolsMenu)
                {
                    ImGui.Separator();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Move the camera to the current selection (first if multiple are selected).");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(200);
                    if (ImGui.Button("Go to Selection"))
                    {
                        GotoSelectionInSceneTree();
                    }
                    ImGui.SameLine();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Move the current selection to the camera position.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Move Selection to Camera"))
                    {
                        MoveSelectionToCamera();
                    }

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Frame the current selection in the viewport (first if multiple are selected).");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Frame Selection"))
                    {
                        FrameSelection();
                    }
                    ImGui.SameLine();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Reset the rotation of the current selection to <0, 0, 0>.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Reset Rotation"))
                    {
                        ResetRotationSelection();
                    }

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Enable the visibility of all objects.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Force Visbility"))
                    {
                        ShowAllObjects();
                    }
                    ImGui.SameLine();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Toggle the visibility of the current selection.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Toggle Visbility"))
                    {
                        ToggleSelectionVisibility();
                    }

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Rotate the current selection by the Arbitary Rotation Roll increment.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Rotate: X"))
                    {
                        ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
                    }
                    ImGui.SameLine();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Rotate the current selection by the Arbitary Rotation Yaw increment.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Rotate: Y"))
                    {
                        ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
                    }
                    ImGui.SameLine();

                    if (CFG.Current.ShowUITooltips)
                    {
                        Utils.ShowHelpMarker("Rotate the current selection by the Arbitary Rotation Yaw increment.");
                        ImGui.SameLine();
                    }
                    ImGui.PushItemWidth(80);
                    if (ImGui.Button("Rotate: Y Pivot"))
                    {
                        ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
                    }
                }

                // Scrambler
                if (CFG.Current.Toolbar_ShowScramblerMenu)
                {
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
                    ImGui.Text("Scramble Position");
                    ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinPosX", ref randomOffsetMin_Pos_X);
                    ImGui.SameLine();

                    ImGui.InputFloat("Offset Max##offsetMaxPosX", ref randomOffsetMax_Pos_X);

                    ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxPosY", ref randomOffsetMax_Pos_Y);

                    ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);

                    // Rotation
                    ImGui.Text("Scramble Rotation");
                    ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinRotX", ref randomOffsetMin_Rot_X);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxRotX", ref randomOffsetMax_Rot_X);

                    ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxRotY", ref randomOffsetMax_Rot_Y);

                    ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);

                    // Scale
                    ImGui.Text("Scramble Scale");
                    ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                    ImGui.SameLine();
                    ImGui.Text("     ");
                    ImGui.SameLine();
                    ImGui.Checkbox("Shared Scale##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);

                    ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);

                    ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Min##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100);
                    ImGui.InputFloat("Offset Max##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);

                    ImGui.Separator();

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

                    if (ImGui.Button("Scramble Selection"))
                    {
                        List<Action> actlist = new();
                        foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
                        {
                            sel.ClearTemporaryTransform(false);
                            actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel)));
                        }

                        CompoundAction action = new(actlist);
                        _actionManager.ExecuteAction(action);
                    }
                }

                // Replicator
                if (CFG.Current.Toolbar_ShowReplicatorMenu)
                {
                    ImGui.Separator();

                }
            }
            ImGui.End();
        }
        /// <summary>
        /// Go to the selected object (first if multiple are selected) in the scene tree.
        /// </summary>
        private void GotoSelectionInSceneTree()
        {
            _selection.GotoTreeTarget = _selection.GetSingleSelection();
        }

        /// <summary>
        /// Frame selected object (first if multiple are selected) in viewport.
        /// </summary>
        public void FrameSelection()
        {
            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
            var first = false;
            BoundingBox box = new();
            foreach (Entity s in selected)
            {
                if (s.RenderSceneMesh != null)
                {
                    if (!first)
                    {
                        box = s.RenderSceneMesh.GetBounds();
                        first = true;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, s.RenderSceneMesh.GetBounds());
                    }
                }
                else if (s.Container.RootObject == s)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = s.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    if (!first)
                    {
                        first = true;
                        box = nodeBox;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, nodeBox);
                    }
                }
            }

            if (first)
            {
                _viewport.FrameBox(box);
            }
        }

        /// <summary>
        /// Show all objects in every map
        /// </summary>
        public void ShowAllObjects()
        {
            foreach (ObjectContainer m in _universe.LoadedObjectContainers.Values)
            {
                if (m == null)
                {
                    continue;
                }

                foreach (Entity obj in m.Objects)
                {
                    obj.EditorVisible = true;
                }
            }
        }

        /// <summary>
        /// Toggle visiblity of selected objects
        /// </summary>
        public void ToggleSelectionVisibility()
        {
            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
            var allhidden = true;
            foreach (Entity s in selected)
            {
                if (s.EditorVisible)
                {
                    allhidden = false;
                }
            }

            foreach (Entity s in selected)
            {
                s.EditorVisible = allhidden;
            }
        }

        /// <summary>
        /// Move current selection to the current camera position
        /// </summary>
        private void MoveSelectionToCamera()
        {
            List<Action> actlist = new();
            HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, _viewport.WorldView.CameraTransform.RotationMatrix);
            Vector3 camPos = _viewport.WorldView.CameraTransform.Position;
            Vector3 targetCamPos = camPos + (camDir * CFG.Current.Map_MoveSelectionToCamera_Radius);

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
                _actionManager.ExecuteAction(action);
            }
        }

        /// <summary>
        /// Rotate the selected objects by a fixed amount on the specified axis
        /// </summary>
        private void ArbitraryRotation_Selection(Vector3 axis, bool pivot)
        {
            List<Action> actlist = new();
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
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Map_ArbitraryRotation_X_Shift;
                    rot_x = objT.EulerRotation.X + radianRotateAmount;
                }

                if (axis.Y != 0)
                {
                    radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Map_ArbitraryRotation_Y_Shift;
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
                _actionManager.ExecuteAction(action);
            }
        }

        /// <summary>
        /// Tool: Reset current selection to 0, 0, 0 rotation.
        /// </summary>
        private void ResetRotationSelection()
        {
            List<Action> actlist = new();

            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);
            foreach (Entity s in selected)
            {
                Vector3 pos = s.GetLocalTransform().Position;
                var rot_x = 0;
                var rot_y = 0;
                var rot_z = 0;

                Transform newRot = new(pos, new Vector3(rot_x, rot_y, rot_z));

                actlist.Add(s.GetUpdateTransformAction(newRot));
            }

            if (actlist.Any())
            {
                CompoundAction action = new(actlist);
                _actionManager.ExecuteAction(action);
            }
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private Transform GetScrambledTransform(Entity sel)
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
            if(CFG.Current.Scrambler_RandomiseScale_SharedScale)
            {
                scaleOffset_Y = scaleOffset_X;
                scaleOffset_Z = scaleOffset_X;
            }

            if (CFG.Current.Scrambler_RandomiseScale_X)
            {
                newScale = new Vector3(newScale[0] + scaleOffset_X, newScale[1], newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Y)
            {
                newScale = new Vector3(newScale[0], newScale[1] + scaleOffset_Y, newScale[2]);
            }
            if (CFG.Current.Scrambler_RandomiseScale_Z)
            {
                newScale = new Vector3(newScale[0], newScale[1], newScale[2] + scaleOffset_Z);
            }

            newTransform.Scale = newScale;

            return newTransform;
        }
    }
}
