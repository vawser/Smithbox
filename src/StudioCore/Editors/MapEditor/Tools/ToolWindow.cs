using ImGuiNET;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Framework.MapActionHandler;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolWindow
{
    private MapEditorScreen Screen;
    private MapActionHandler Handler;

    public ToolWindow(MapEditorScreen screen, MapActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void OnProjectChanged()
    {

    }

    public bool FocusLocalPropertySearch = false;

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_MapEditor"))
        {
            Smithbox.EditorHandler.MapEditor.FocusManager.SwitchWindowContext(MapEditorContext.ToolWindow);

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

                if (Screen.Universe.LoadedObjectContainers == null)
                {
                    UIHelper.WrappedText("No maps have been loaded yet.");
                    UIHelper.WrappedText("");
                }
                else if (Screen.Universe.LoadedObjectContainers != null &&
                    !Screen.Universe.LoadedObjectContainers.Any())
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
                        foreach (var obj in Screen.Universe.LoadedObjectContainers)
                        {
                            if (obj.Value != null)
                            {
                                if (ImGui.Selectable(obj.Key))
                                {
                                    Handler._targetMap = (obj.Key, obj.Value);
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
                            UIHelper.ShowHoverTooltip("Create a BTL Light object.");
                        }

                        if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        UIHelper.ShowHoverTooltip("Create a Part object.");

                        if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        UIHelper.ShowHoverTooltip("Create a Region object.");

                        if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Part = false;
                        }
                        UIHelper.ShowHoverTooltip("Create an Event object.");
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

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                    UIHelper.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);
                    UIHelper.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Increment_Entity_ID = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
                    UIHelper.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
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
                UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");

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
                UIHelper.ShowHoverTooltip("Set the rotation axis to X.");

                if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                {
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                UIHelper.ShowHoverTooltip("Set the rotation axis to Y.");

                if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                UIHelper.ShowHoverTooltip("Set the rotation axis to Y and pivot with respect to others within the selection.");

                if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                }
                UIHelper.ShowHoverTooltip("Set the rotation axis to specified values below.");
                UIHelper.WrappedText("");

                if (!CFG.Current.Toolbar_Fixed_Rotate)
                {
                    UIHelper.WrappedText("Current Degree Increment:");
                    ImGui.SameLine();
                    RotationIncrement.DisplayCurrentRotateIncrement();

                    UIHelper.WrappedText("");

                    if (ImGui.Button("Cycle Increment", thinButtonSize))
                    {
                        RotationIncrement.CycleIncrementType();
                    }
                    UIHelper.ShowHoverTooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Degree Increment [0]:");
                    ImGui.PushItemWidth(defaultButtonSize.X);

                    var rot0 = CFG.Current.Toolbar_Rotate_Increment_0;
                    if (ImGui.SliderFloat("##degreeIncrement0", ref rot0, -360.0f, 360.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment_0 = Math.Clamp(rot0, -360.0f, 360.0f);
                    }
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    UIHelper.WrappedText("Degree Increment [1]:");
                    ImGui.PushItemWidth(defaultButtonSize.X);

                    var rot1 = CFG.Current.Toolbar_Rotate_Increment_1;
                    if (ImGui.SliderFloat("##degreeIncrement1", ref rot1, -360.0f, 360.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment_1 = Math.Clamp(rot1, -360.0f, 360.0f);
                    }
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    UIHelper.WrappedText("Degree Increment [2]:");
                    ImGui.PushItemWidth(defaultButtonSize.X);

                    var rot2 = CFG.Current.Toolbar_Rotate_Increment_2;
                    if (ImGui.SliderFloat("##degreeIncrement2", ref rot2, -360.0f, 360.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment_2 = Math.Clamp(rot2, -360.0f, 360.0f);
                    }
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    UIHelper.WrappedText("Degree Increment [3]:");
                    ImGui.PushItemWidth(defaultButtonSize.X);

                    var rot3 = CFG.Current.Toolbar_Rotate_Increment_3;
                    if (ImGui.SliderFloat("##degreeIncrement3", ref rot3, -360.0f, 360.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment_3 = Math.Clamp(rot3, -360.0f, 360.0f);
                    }
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    UIHelper.WrappedText("Degree Increment [4]:");
                    ImGui.PushItemWidth(defaultButtonSize.X);

                    var rot4 = CFG.Current.Toolbar_Rotate_Increment_4;
                    if (ImGui.SliderFloat("##degreeIncrement4", ref rot4, -360.0f, 360.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment_4 = Math.Clamp(rot4, -360.0f, 360.0f);
                    }
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    UIHelper.WrappedText("");
                }
                else
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
                    UIHelper.ShowHoverTooltip("Set the X component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                    {
                        y = Math.Clamp(y, -360f, 360f);
                    }
                    UIHelper.ShowHoverTooltip("Set the Y component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                    {
                        z = Math.Clamp(z, -360f, 360f);
                    }
                    UIHelper.ShowHoverTooltip("Set the Z component of the fixed rotation action.");
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
                UIHelper.ShowHoverTooltip("Include the X co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosX", ref randomOffsetMin_Pos_X);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the position X co-ordinate.");

                ImGui.SameLine();

                ImGui.InputFloat("##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the position X co-ordinate.");

                ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                UIHelper.ShowHoverTooltip("Include the Y co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the position Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the position Y co-ordinate.");

                ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                UIHelper.ShowHoverTooltip("Include the Z co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the position Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the position Z co-ordinate.");
                ImGui.Text("");

                // Rotation
                UIHelper.WrappedText("Rotation");
                ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                UIHelper.ShowHoverTooltip("Include the X co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotX", ref randomOffsetMin_Rot_X);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the rotation X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the rotation X co-ordinate.");

                ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                UIHelper.ShowHoverTooltip("Include the Y co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the rotation Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the rotation Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                UIHelper.ShowHoverTooltip("Include the Z co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the rotation Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the rotation Z co-ordinate.");
                ImGui.Text("");

                // Scale
                UIHelper.WrappedText("Scale");
                ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                UIHelper.ShowHoverTooltip("Include the X co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the scale X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the scale X co-ordinate.");

                ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                UIHelper.ShowHoverTooltip("Include the Y co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the scale Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the scale Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                UIHelper.ShowHoverTooltip("Include the Z co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                UIHelper.ShowHoverTooltip("Minimum amount to add to the scale Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                UIHelper.ShowHoverTooltip("Maximum amount to add to the scale Z co-ordinate.");
                UIHelper.WrappedText("");

                ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                UIHelper.ShowHoverTooltip("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");
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
                UIHelper.ShowHoverTooltip("Replicate the first selection in the Line shape.");

                if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                {
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                UIHelper.ShowHoverTooltip("Replicate the first selection in the Circle shape.");

                if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                UIHelper.ShowHoverTooltip("Replicate the first selection in the Square shape.");
                UIHelper.WrappedText("");

                // Line
                if (CFG.Current.Replicator_Mode_Line)
                {
                    UIHelper.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                    UIHelper.ShowHoverTooltip("The amount of new entities to create (from the first selection).");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Offset per Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                    UIHelper.ShowHoverTooltip("The distance between each newly created entity.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Replicate Direction:");
                    if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    UIHelper.ShowHoverTooltip("Replicate on the X-axis.");

                    if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    UIHelper.ShowHoverTooltip("Replicate on the Y-axis.");

                    if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                    }
                    UIHelper.ShowHoverTooltip("Replicate on the Z-axis.");
                    UIHelper.WrappedText("");

                    ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                    UIHelper.ShowHoverTooltip("When enabled, the position offset will be applied in the opposite direction.");
                }

                // Circle
                if (CFG.Current.Replicator_Mode_Circle)
                {
                    UIHelper.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
                    UIHelper.ShowHoverTooltip("The number of points within the circle on which the entities are placed.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Circle Radius:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                    UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nThe radius of the circle on which to place the entities.");

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
                    UIHelper.ShowHoverTooltip("The number of points on one side of the square on which the entities are placed.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Square Width:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
                    UIHelper.ShowHoverTooltip("The width of the square on which to place the entities.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Square Height:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Depth", ref CFG.Current.Replicator_Square_Depth);
                    UIHelper.ShowHoverTooltip("The depth of the square on which to place the entities.");
                    UIHelper.WrappedText("");

                    if (CFG.Current.Replicator_Square_Width < 1)
                        CFG.Current.Replicator_Square_Width = 1;

                    if (CFG.Current.Replicator_Square_Size < 2)
                        CFG.Current.Replicator_Square_Size = 2;

                    if (CFG.Current.Replicator_Square_Depth < 1)
                        CFG.Current.Replicator_Square_Depth = 1;
                }

                ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
                UIHelper.ShowHoverTooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2 && Smithbox.ProjectType != ProjectType.AC6)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID))
                    {
                        if (CFG.Current.Replicator_Increment_Entity_ID)
                        {
                            CFG.Current.Replicator_Clear_Entity_ID = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                    UIHelper.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);
                    UIHelper.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Replicator_Clear_Entity_ID))
                    {
                        if (CFG.Current.Replicator_Clear_Entity_ID)
                        {
                            CFG.Current.Replicator_Increment_Entity_ID = false;
                        }
                    }
                    UIHelper.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Replicator_Clear_Entity_Group_IDs);
                    UIHelper.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
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
                UIHelper.ShowHoverTooltip("Move the current selection to the closest X co-ordinate within the map grid.");

                ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                UIHelper.ShowHoverTooltip("Move the current selection to the closest Y co-ordinate within the map grid.");

                ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                UIHelper.ShowHoverTooltip("Move the current selection to the closest Z co-ordinate within the map grid.");

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
                UIHelper.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the current height of the map grid.");

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
                Screen.PrefabView.ImportPrefabMenu();
                Screen.PrefabView.PrefabTree();
            }

            ///--------------------
            /// Export Prefab
            ///--------------------
            if (ImGui.CollapsingHeader("Export Prefab"))
            {
                Screen.PrefabView.ExportPrefabMenu();
                Screen.PrefabView.PrefabTree();
            }

            ///--------------------
            /// Selection Groups
            ///--------------------
            if (ImGui.CollapsingHeader("Selection Groups"))
            {
                Screen.SelectionGroupView.Display();
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
                Screen.LocalSearchView.Display();
            }

            ///--------------------
            /// Global Property Search
            ///--------------------
            if (ImGui.CollapsingHeader("Global Property Search"))
            {
                if (!Screen.MapQueryView.Bank.MapBankInitialized && !Screen.MapQueryView.UserLoadedData)
                {
                    if (ImGui.Button("Load Map Data", defaultButtonSize))
                    {
                        Screen.MapQueryView.Setup();
                    }
                }

                Screen.MapQueryView.IsOpen = true;
                Screen.MapQueryView.DisplayInput();
                Screen.MapQueryView.DisplayResults();
            }
            else
            {
                Screen.MapQueryView.IsOpen = false;
            }

            ///--------------------
            /// Property Mass Edit
            ///--------------------
            if (ImGui.CollapsingHeader("Property Mass Edit"))
            {
                MsbMassEdit.Display();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
