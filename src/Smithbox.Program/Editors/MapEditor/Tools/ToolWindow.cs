using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolWindow
{
    private MapEditorScreen Editor;
    private MapActionHandler Handler;

    public ToolWindow(MapEditorScreen screen, MapActionHandler handler)
    {
        Editor = screen;
        Handler = handler;
    }

    public void OnProjectChanged()
    {

    }

    public bool FocusLocalPropertySearch = false;

    public void OnGui()
    {
        if (Editor.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_MapEditor"))
        {
            Editor.FocusManager.SwitchWindowContext(MapEditorContext.ToolWindow);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);
            var thinButtonSize = new Vector2(windowWidth, 24);

            ///--------------------
            /// Create
            ///--------------------
            if (ImGui.CollapsingHeader("Create"))
            {
                UIHelper.WrappedText("Create a new object within the target map.");
                UIHelper.WrappedText("");

                if (Editor.IsAnyMapLoaded())
                {
                    UIHelper.WrappedText("No maps have been loaded yet.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Target Map:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                    {
                        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
                        {
                            var mapID = entry.Key.Filename;
                            var container = entry.Value.MapContainer;

                            if (container != null)
                            {
                                if (ImGui.Selectable(mapID))
                                {
                                    Handler._targetMap = (mapID, container);
                                    break;
                                }
                            }
                        }
                        ImGui.EndCombo();
                    }
                    UIHelper.WrappedText("");

                    if (Handler._targetMap != (null, null))
                    {
                        var map = (MapContainer)Handler._targetMap.Item2;

                        UIHelper.WrappedText("Target Type:");
                        if (map.BTLParents.Any())
                        {
                            if (ImGui.Checkbox("BTL Light", ref CFG.Current.Toolbar_Create_Light))
                            {
                                CFG.Current.Toolbar_Create_Part = false;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = false;
                            }
                            UIHelper.Tooltip("Create a BTL Light object.");
                        }

                        if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        UIHelper.Tooltip("Create a Part object.");

                        if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        UIHelper.Tooltip("Create a Region object.");

                        if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Part = false;
                        }
                        UIHelper.Tooltip("Create an Event object.");
                        UIHelper.WrappedText("");


                        if (ImGui.Button("Create Object", defaultButtonSize))
                        {
                            Handler.ApplyObjectCreation();
                        }

                        UIHelper.WrappedText("");

                        if (CFG.Current.Toolbar_Create_Light)
                        {
                            // Nothing
                        }

                        if (CFG.Current.Toolbar_Create_Part)
                        {
                            UIHelper.WrappedText("Part Type:");
                            ImGui.BeginChild("msb_part_selection", new Vector2((windowWidth - 10), (windowHeight / 4)));

                            foreach ((string, Type) p in Handler._partsClasses)
                            {
                                if (ImGui.RadioButton(p.Item1, p.Item2 == Handler._createPartSelectedType))
                                {
                                    Handler._createPartSelectedType = p.Item2;
                                }
                            }

                            ImGui.EndChild();
                        }

                        if (CFG.Current.Toolbar_Create_Region)
                        {
                            // MSB format that only have 1 region type
                            if (Handler._regionClasses.Count == 1)
                            {
                                Handler._createRegionSelectedType = Handler._regionClasses[0].Item2;
                            }
                            else
                            {
                                UIHelper.WrappedText("Region Type:");

                                ImGui.BeginChild("msb_region_selection", new Vector2((windowWidth - 10), (windowHeight / 4)));

                                foreach ((string, Type) p in Handler._regionClasses)
                                {
                                    if (ImGui.RadioButton(p.Item1, p.Item2 == Handler._createRegionSelectedType))
                                    {
                                        Handler._createRegionSelectedType = p.Item2;
                                    }
                                }

                                ImGui.EndChild();
                            }
                        }

                        if (CFG.Current.Toolbar_Create_Event)
                        {
                            UIHelper.WrappedText("Event Type:");
                            ImGui.BeginChild("msb_event_selection", new Vector2((windowWidth - 10), (windowHeight / 4)));

                            foreach ((string, Type) p in Handler._eventClasses)
                            {
                                if (ImGui.RadioButton(p.Item1, p.Item2 == Handler._createEventSelectedType))
                                {
                                    Handler._createEventSelectedType = p.Item2;
                                }
                            }

                            ImGui.EndChild();
                        }
                    }
                }
            }

            ///--------------------
            /// Duplicate
            ///--------------------
            if (ImGui.CollapsingHeader("Duplicate"))
            {
                UIHelper.WrappedText("Duplicate the current selection.");
                UIHelper.WrappedText("");

                if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                        }
                    }
                    UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
                }

                if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                    UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_PartNames);
                    UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
                }

                if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Increment_Entity_ID = false;
                        }
                    }
                    UIHelper.Tooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
                    UIHelper.Tooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                UIHelper.WrappedText("");

                if (ImGui.Button("Duplicate Selection", defaultButtonSize))
                {
                    Handler.ApplyDuplicate();
                }
            }

            ///--------------------
            /// Duplicate to Map
            ///--------------------
            if (ImGui.CollapsingHeader("Duplicate to Map"))
            {
                Handler.DisplayDuplicateToMapMenu(MapDuplicateToMapType.ToolWindow);
            }

            ///--------------------
            /// Move to Camera
            ///--------------------
            if (ImGui.CollapsingHeader("Move to Camera"))
            {
                UIHelper.WrappedText("Move the current selection to the camera position.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Camera Offset Distance:");

                ImGui.PushItemWidth(defaultButtonSize.X);
                if(ImGui.SliderFloat("##Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100))
                {
                    if (CFG.Current.Toolbar_Move_to_Camera_Offset < 0)
                        CFG.Current.Toolbar_Move_to_Camera_Offset = 0;

                    if (CFG.Current.Toolbar_Move_to_Camera_Offset > 100)
                        CFG.Current.Toolbar_Move_to_Camera_Offset = 100;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");

                UIHelper.WrappedText("");

                if (ImGui.Button("Move Selection to Camera", defaultButtonSize))
                {
                    Handler.ApplyMoveToCamera();
                }
            }

            ///--------------------
            /// Rotate
            ///--------------------
            if (ImGui.CollapsingHeader("Rotate"))
            {
                UIHelper.WrappedText("Rotate the current selection by the following parameters.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Rotation Type:");
                if (ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                UIHelper.Tooltip("Set the rotation axis to X.");

                if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                {
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                UIHelper.Tooltip("Set the rotation axis to Y.");

                if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                UIHelper.Tooltip("Set the rotation axis to Y and pivot with respect to others within the selection.");

                if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                }
                UIHelper.Tooltip("Set the rotation axis to specified values below.");
                UIHelper.WrappedText("");

                if (CFG.Current.Toolbar_Fixed_Rotate)
                {
                    var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
                    var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
                    var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

                    UIHelper.WrappedText("Fixed Rotation");
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("X##fixedRotationX", ref x))
                    {
                        x = Math.Clamp(x, -360f, 360f);
                    }
                    UIHelper.Tooltip("Set the X component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                    {
                        y = Math.Clamp(y, -360f, 360f);
                    }
                    UIHelper.Tooltip("Set the Y component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                    {
                        z = Math.Clamp(z, -360f, 360f);
                    }
                    UIHelper.Tooltip("Set the Z component of the fixed rotation action.");
                    UIHelper.WrappedText("");

                    CFG.Current.Toolbar_Rotate_FixedAngle = new Vector3(x, y, z);
                }

                if (ImGui.Button("Rotate Selection", defaultButtonSize))
                {
                    Handler.ApplyRotation();
                }
            }

            ///--------------------
            /// Scramble
            ///--------------------
            if (ImGui.CollapsingHeader("Scramble"))
            {
                UIHelper.WrappedText("Scramble the current selection's position, rotation and scale by the following parameters.");
                UIHelper.WrappedText("");

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
                UIHelper.WrappedText("Position");
                ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
                UIHelper.Tooltip("Include the X co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosX", ref randomOffsetMin_Pos_X);
                UIHelper.Tooltip("Minimum amount to add to the position X co-ordinate.");

                ImGui.SameLine();

                ImGui.InputFloat("##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                UIHelper.Tooltip("Maximum amount to add to the position X co-ordinate.");

                ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                UIHelper.Tooltip("Include the Y co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                UIHelper.Tooltip("Minimum amount to add to the position Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                UIHelper.Tooltip("Maximum amount to add to the position Y co-ordinate.");

                ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                UIHelper.Tooltip("Include the Z co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                UIHelper.Tooltip("Minimum amount to add to the position Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                UIHelper.Tooltip("Maximum amount to add to the position Z co-ordinate.");
                ImGui.Text("");

                // Rotation
                UIHelper.WrappedText("Rotation");
                ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                UIHelper.Tooltip("Include the X co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotX", ref randomOffsetMin_Rot_X);
                UIHelper.Tooltip("Minimum amount to add to the rotation X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                UIHelper.Tooltip("Maximum amount to add to the rotation X co-ordinate.");

                ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                UIHelper.Tooltip("Include the Y co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                UIHelper.Tooltip("Minimum amount to add to the rotation Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                UIHelper.Tooltip("Maximum amount to add to the rotation Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                UIHelper.Tooltip("Include the Z co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                UIHelper.Tooltip("Minimum amount to add to the rotation Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                UIHelper.Tooltip("Maximum amount to add to the rotation Z co-ordinate.");
                ImGui.Text("");

                // Scale
                UIHelper.WrappedText("Scale");
                ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                UIHelper.Tooltip("Include the X co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                UIHelper.Tooltip("Minimum amount to add to the scale X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                UIHelper.Tooltip("Maximum amount to add to the scale X co-ordinate.");

                ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                UIHelper.Tooltip("Include the Y co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                UIHelper.Tooltip("Minimum amount to add to the scale Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                UIHelper.Tooltip("Maximum amount to add to the scale Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                UIHelper.Tooltip("Include the Z co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                UIHelper.Tooltip("Minimum amount to add to the scale Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                UIHelper.Tooltip("Maximum amount to add to the scale Z co-ordinate.");
                UIHelper.WrappedText("");

                ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                UIHelper.Tooltip("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");
                UIHelper.WrappedText("");

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

                if (ImGui.Button("Scramble Selection", defaultButtonSize))
                {
                    Handler.ApplyScramble();
                }
            }

            ///--------------------
            /// Replicate
            ///--------------------
            if (ImGui.CollapsingHeader("Replicate"))
            {
                UIHelper.WrappedText("Replicate the current selection by the following parameters.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Replicate Style:");
                if (ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                UIHelper.Tooltip("Replicate the first selection in the Line shape.");

                if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                {
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                UIHelper.Tooltip("Replicate the first selection in the Circle shape.");

                if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                UIHelper.Tooltip("Replicate the first selection in the Square shape.");
                UIHelper.WrappedText("");

                // Line
                if (CFG.Current.Replicator_Mode_Line)
                {
                    UIHelper.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                    UIHelper.Tooltip("The amount of new entities to create (from the first selection).");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Offset per Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                    UIHelper.Tooltip("The distance between each newly created entity.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Replicate Direction:");
                    if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    UIHelper.Tooltip("Replicate on the X-axis.");

                    if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    UIHelper.Tooltip("Replicate on the Y-axis.");

                    if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                    }
                    UIHelper.Tooltip("Replicate on the Z-axis.");
                    UIHelper.WrappedText("");

                    ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                    UIHelper.Tooltip("When enabled, the position offset will be applied in the opposite direction.");
                }

                // Circle
                if (CFG.Current.Replicator_Mode_Circle)
                {
                    UIHelper.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
                    UIHelper.Tooltip("The number of points within the circle on which the entities are placed.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Circle Radius:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                    UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nThe radius of the circle on which to place the entities.");

                    UIHelper.WrappedText("");

                    if (CFG.Current.Replicator_Circle_Size < 1)
                        CFG.Current.Replicator_Circle_Size = 1;

                }

                // Square
                if (CFG.Current.Replicator_Mode_Square)
                {
                    UIHelper.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Square_Size);
                    UIHelper.Tooltip("The number of points on one side of the square on which the entities are placed.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Square Width:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
                    UIHelper.Tooltip("The width of the square on which to place the entities.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Square Height:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Depth", ref CFG.Current.Replicator_Square_Depth);
                    UIHelper.Tooltip("The depth of the square on which to place the entities.");
                    UIHelper.WrappedText("");

                    if (CFG.Current.Replicator_Square_Width < 1)
                        CFG.Current.Replicator_Square_Width = 1;

                    if (CFG.Current.Replicator_Square_Size < 2)
                        CFG.Current.Replicator_Square_Size = 2;

                    if (CFG.Current.Replicator_Square_Depth < 1)
                        CFG.Current.Replicator_Square_Depth = 1;
                }

                ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
                UIHelper.Tooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

                if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2 && Editor.Project.ProjectType != ProjectType.AC6)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID))
                    {
                        if (CFG.Current.Replicator_Increment_Entity_ID)
                        {
                            CFG.Current.Replicator_Clear_Entity_ID = false;
                        }
                    }
                    UIHelper.Tooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                }

                if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                    UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_PartNames);
                    UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
                }

                if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Replicator_Clear_Entity_ID))
                    {
                        if (CFG.Current.Replicator_Clear_Entity_ID)
                        {
                            CFG.Current.Replicator_Increment_Entity_ID = false;
                        }
                    }
                    UIHelper.Tooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Replicator_Clear_Entity_Group_IDs);
                    UIHelper.Tooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                UIHelper.WrappedText("");

                if (ImGui.Button("Replicate Selection", defaultButtonSize))
                {
                    Handler.ApplyReplicate();
                }
            }

            ///--------------------
            /// Move to Grid
            ///--------------------
            if (ImGui.CollapsingHeader("Move to Grid"))
            {
                UIHelper.WrappedText("Set the current selection to the closest grid position.");
                UIHelper.WrappedText("");

                ImGui.Checkbox("X", ref CFG.Current.Toolbar_Move_to_Grid_X);
                UIHelper.Tooltip("Move the current selection to the closest X co-ordinate within the map grid.");

                ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                UIHelper.Tooltip("Move the current selection to the closest Y co-ordinate within the map grid.");

                ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                UIHelper.Tooltip("Move the current selection to the closest Z co-ordinate within the map grid.");

                UIHelper.WrappedText("");

                UIHelper.WrappedText("Grid Height");
                ImGui.PushItemWidth(defaultButtonSize.X);
                if (ImGui.SliderFloat("Grid height", ref CFG.Current.MapEditor_Viewport_Grid_Height, -10000, 10000))
                {
                    if (CFG.Current.MapEditor_Viewport_Grid_Height < -10000)
                        CFG.Current.MapEditor_Viewport_Grid_Height = -10000;

                    if (CFG.Current.MapEditor_Viewport_Grid_Height > 10000)
                        CFG.Current.MapEditor_Viewport_Grid_Height = 10000;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the current height of the map grid.");

                UIHelper.WrappedText("");

                if (ImGui.Button("Move Selection to Grid", defaultButtonSize))
                {
                    Handler.ApplyMovetoGrid();
                }
            }

            ImGui.Separator();

            ///--------------------
            /// Import Prefab
            ///--------------------
            if (ImGui.CollapsingHeader("Import Prefab"))
            {
                Editor.PrefabView.ImportPrefabMenu();
                Editor.PrefabView.PrefabTree();
            }

            ///--------------------
            /// Export Prefab
            ///--------------------
            if (ImGui.CollapsingHeader("Export Prefab"))
            {
                Editor.PrefabView.ExportPrefabMenu();
                Editor.PrefabView.PrefabTree();
            }

            ///--------------------
            /// Selection Groups
            ///--------------------
            if (ImGui.CollapsingHeader("Selection Groups"))
            {
                Editor.SelectionGroupView.Display();
            }

            ///--------------------
            /// Movement Increments
            ///--------------------
            if (ImGui.CollapsingHeader("Movement Increments"))
            {
                UIHelper.WrappedText("The current settings to use for the Movement Increment shortcuts.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Current Movement Increment:");
                ImGui.SameLine();
                Editor.KeyboardMovement.DisplayCurrentMovementIncrement();

                UIHelper.WrappedText("");

                UIHelper.WrappedText($"Shortcut: {KeyBindings.Current.MAP_KeyboardMove_CycleIncrement.HintText}");
                if (ImGui.Button("Cycle Increment", thinButtonSize))
                {
                    Editor.KeyboardMovement.CycleIncrementType();
                }
                UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");
                UIHelper.WrappedText("");

                // 0
                UIHelper.WrappedText("Movement Increment [0]:");
                ImGui.PushItemWidth(thinButtonSize.X);

                var unit0 = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                if (ImGui.SliderFloat("##movementIncrement0", ref unit0, 0.0f, 999.0f))
                {
                    CFG.Current.MapEditor_Selection_Movement_Increment_0 = unit0;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

                // 1
                UIHelper.WrappedText("Movement Increment [1]:");
                ImGui.PushItemWidth(thinButtonSize.X);

                var unit1 = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                if (ImGui.SliderFloat("##movementIncrement1", ref unit1, 0.0f, 999.0f))
                {
                    CFG.Current.MapEditor_Selection_Movement_Increment_1 = unit1;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

                // 2
                UIHelper.WrappedText("Movement Increment [2]:");
                ImGui.PushItemWidth(thinButtonSize.X);

                var unit2 = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                if (ImGui.SliderFloat("##movementIncrement2", ref unit2, 0.0f, 999.0f))
                {
                    CFG.Current.MapEditor_Selection_Movement_Increment_2 = unit2;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

                // 3
                UIHelper.WrappedText("Movement Increment [3]:");
                ImGui.PushItemWidth(thinButtonSize.X);

                var unit3 = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                if (ImGui.SliderFloat("##movementIncrement3", ref unit3, 0.0f, 999.0f))
                {
                    CFG.Current.MapEditor_Selection_Movement_Increment_3 = unit3;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

                // 4
                UIHelper.WrappedText("Movement Increment [4]:");
                ImGui.PushItemWidth(thinButtonSize.X);

                var unit4 = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                if (ImGui.SliderFloat("##movementIncrement4", ref unit4, 0.0f, 999.0f))
                {
                    CFG.Current.MapEditor_Selection_Movement_Increment_4 = unit4;
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the movement increment amount used by keyboard move.");

                UIHelper.WrappedText("");

                ImGui.Checkbox("Display movement increment type", ref CFG.Current.Viewport_DisplayMovementIncrement);
                UIHelper.Tooltip("Display the current movement increment type you are using in the information panel.");
            }

            ///--------------------
            /// Rotation Increments
            ///--------------------
            if (ImGui.CollapsingHeader("Rotation Increments"))
            {
                UIHelper.WrappedText("The current settings to use for the Rotation Increment shortcuts.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Current Rotation Increment:");
                ImGui.SameLine();
                Editor.RotationIncrement.DisplayCurrentRotateIncrement();

                UIHelper.WrappedText("");

                UIHelper.WrappedText($"Shortcut: {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText}");
                if (ImGui.Button("Cycle Increment", thinButtonSize))
                {
                    Editor.RotationIncrement.CycleIncrementType();
                }
                UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Degree Increment [0]:");
                ImGui.PushItemWidth(defaultButtonSize.X);

                var rot0 = CFG.Current.Toolbar_Rotate_Increment_0;
                if (ImGui.SliderFloat("##degreeIncrement0", ref rot0, -360.0f, 360.0f))
                {
                    CFG.Current.Toolbar_Rotate_Increment_0 = Math.Clamp(rot0, -360.0f, 360.0f);
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                UIHelper.WrappedText("Degree Increment [1]:");
                ImGui.PushItemWidth(defaultButtonSize.X);

                var rot1 = CFG.Current.Toolbar_Rotate_Increment_1;
                if (ImGui.SliderFloat("##degreeIncrement1", ref rot1, -360.0f, 360.0f))
                {
                    CFG.Current.Toolbar_Rotate_Increment_1 = Math.Clamp(rot1, -360.0f, 360.0f);
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                UIHelper.WrappedText("Degree Increment [2]:");
                ImGui.PushItemWidth(defaultButtonSize.X);

                var rot2 = CFG.Current.Toolbar_Rotate_Increment_2;
                if (ImGui.SliderFloat("##degreeIncrement2", ref rot2, -360.0f, 360.0f))
                {
                    CFG.Current.Toolbar_Rotate_Increment_2 = Math.Clamp(rot2, -360.0f, 360.0f);
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                UIHelper.WrappedText("Degree Increment [3]:");
                ImGui.PushItemWidth(defaultButtonSize.X);

                var rot3 = CFG.Current.Toolbar_Rotate_Increment_3;
                if (ImGui.SliderFloat("##degreeIncrement3", ref rot3, -360.0f, 360.0f))
                {
                    CFG.Current.Toolbar_Rotate_Increment_3 = Math.Clamp(rot3, -360.0f, 360.0f);
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                UIHelper.WrappedText("Degree Increment [4]:");
                ImGui.PushItemWidth(defaultButtonSize.X);

                var rot4 = CFG.Current.Toolbar_Rotate_Increment_4;
                if (ImGui.SliderFloat("##degreeIncrement4", ref rot4, -360.0f, 360.0f))
                {
                    CFG.Current.Toolbar_Rotate_Increment_4 = Math.Clamp(rot4, -360.0f, 360.0f);
                }
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                UIHelper.WrappedText("");

                ImGui.Checkbox("Display rotation increment in viewport", ref CFG.Current.Viewport_DisplayRotationIncrement);
                UIHelper.Tooltip("Display the current degree increment type you are using in the information panel.");
            }

            ImGui.Separator();

            ///--------------------
            /// Local Property Search
            ///--------------------
            if (FocusLocalPropertySearch)
            {
                FocusLocalPropertySearch = false;
                ImGui.SetNextItemOpen(true);
            }
            if (ImGui.CollapsingHeader("Local Property Search"))
            {
                Editor.LocalSearchView.Display();
            }

            ///--------------------
            /// Global Property Search
            ///--------------------
            if (ImGui.CollapsingHeader("Global Property Search"))
            {
                if (!Editor.MapQueryView.Bank.MapBankInitialized && !Editor.MapQueryView.UserLoadedData)
                {
                    if (ImGui.Button("Load Map Data", defaultButtonSize))
                    {
                        Editor.MapQueryView.Setup();
                    }
                }

                Editor.MapQueryView.IsOpen = true;
                Editor.MapQueryView.DisplayInput();
                Editor.MapQueryView.DisplayResults();
            }
            else
            {
                Editor.MapQueryView.IsOpen = false;
            }

            ///--------------------
            /// Property Mass Edit
            ///--------------------
            if (ImGui.CollapsingHeader("Property Mass Edit"))
            {
                Editor.MassEditHandler.Display();
            }

            ///--------------------
            /// Simple Treasure Maker
            ///--------------------
            if (Editor.TreasureMaker.IsSupported())
            {
                if (ImGui.CollapsingHeader("Simple Treasure Maker"))
                {
                    Editor.TreasureMaker.Display();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
