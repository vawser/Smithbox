using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions;
using StudioCore.Editors.MapEditor.MapQuery;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Actions.ActionHandler;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolWindow
{
    private MapEditorScreen Screen;
    private ActionHandler Handler;

    public ToolWindow(MapEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_MapEditor"))
        {
            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);
            var thinButtonSize = new Vector2(windowWidth, 24);


            ///--------------------
            /// Global Property Search
            ///--------------------
            if (ImGui.CollapsingHeader("Global Property Search"))
            {
                Screen.MapQueryHandler.IsOpen = true;
                Screen.MapQueryHandler.DisplayInput();
                Screen.MapQueryHandler.DisplayResults();
            }
            else
            {
                Screen.MapQueryHandler.IsOpen = false;
            }

            ///--------------------
            /// Local Property Search
            ///--------------------
            if (ImGui.CollapsingHeader("Local Property Search"))
            {
                Screen.PropSearch.Display();
            }

            ///--------------------
            /// Entity ID Assigner
            ///--------------------
            /// TODO: re-do as a Global Property Edit, use Map Query Engine stuff
            /*
            if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                if (ImGui.CollapsingHeader("Entity ID Assigner"))
                {
                    ImguiUtils.WrappedText("Assign an Entity Group ID to all entities across all maps,\noptionally filtering by specific attributes.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Entity Group ID");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##entityGroupInput", ref CFG.Current.Toolbar_EntityGroupID);
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Filter");

                    ImGui.PushItemWidth(defaultButtonSize.X);
                    if (ImGui.BeginCombo("##filterAttribute", Handler.SelectedFilter.GetDisplayName()))
                    {
                        foreach (var entry in Enum.GetValues(typeof(EntityFilterType)))
                        {
                            var target = (EntityFilterType)entry;

                            if (ImGui.Selectable($"{target.GetDisplayName()}"))
                            {
                                Handler.SelectedFilter = target;
                                break;
                            }
                        }

                        ImGui.EndCombo();
                    }
                    ImguiUtils.ShowHoverTooltip("When assigning the Entity Group ID, the action will only assign it to entities that match this attribute.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Filter Input");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputText("##entityGroupAttribute", ref CFG.Current.Toolbar_EntityGroup_Attribute, 255);
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Target Map");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    if (ImGui.BeginCombo("##mapTargetFilter", Handler.SelectedMapFilter))
                    {
                        IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> orderedMaps = Screen.Universe.LoadedObjectContainers.OrderBy(k => k.Key);

                        foreach (var entry in orderedMaps)
                        {
                            if (ImGui.Selectable($"{entry.Key}"))
                            {
                                Handler.SelectedMapFilter = entry.Key;
                                break;
                            }
                        }

                        if (ImGui.Selectable($"All"))
                        {
                            Handler.SelectedMapFilter = "All";
                        }

                        ImGui.EndCombo();
                    }
                    ImguiUtils.ShowHoverTooltip("When assigning the Entity Group ID, the action will only assign it to entities that match this attribute.");
                    ImguiUtils.WrappedText("");

                    if (Handler.SelectedMapFilter == "All")
                    {
                        ImguiUtils.WrappedText("WARNING: applying this to all maps will take a few minutes,\nexpect Smithbox to hang until it finishes.");
                        ImguiUtils.WrappedText("");
                    }

                    if (ImGui.Button("Assign", defaultButtonSize))
                    {
                        Handler.ApplyEntityAssigner();
                    }
                }
            }
            */


            ///--------------------
            /// Selection Groups
            ///--------------------
            if (ImGui.CollapsingHeader("Selection Groups"))
            {
                Screen.SelectionGroupEditor.Display();
            }

            ///--------------------
            /// Import Prefab
            ///--------------------
            if (ImGui.CollapsingHeader("Import Prefab"))
            {
                Screen.PrefabEditor.ImportPrefabMenu();
                Screen.PrefabEditor.PrefabTree();
            }

            ///--------------------
            /// Export Prefab
            ///--------------------
            if (ImGui.CollapsingHeader("Export Prefab"))
            {
                Screen.PrefabEditor.ExportPrefabMenu();
                Screen.PrefabEditor.PrefabTree();
            }

            ///--------------------
            /// Create
            ///--------------------
            if (ImGui.CollapsingHeader("Create"))
            {
                ImguiUtils.WrappedText("Create a new object within the target map.");
                ImguiUtils.WrappedText("");

                if (Screen.Universe.LoadedObjectContainers == null)
                {
                    ImguiUtils.WrappedText("No maps have been loaded yet.");
                    ImguiUtils.WrappedText("");
                }
                else if (Screen.Universe.LoadedObjectContainers != null &&
                    !Screen.Universe.LoadedObjectContainers.Any())
                {
                    ImguiUtils.WrappedText("No maps have been loaded yet.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Target Map:");
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
                    ImguiUtils.WrappedText("");

                    if (Handler._targetMap != (null, null))
                    {
                        var map = (MapContainer)Handler._targetMap.Item2;

                        ImguiUtils.WrappedText("Target Type:");
                        if (map.BTLParents.Any())
                        {
                            if (ImGui.Checkbox("BTL Light", ref CFG.Current.Toolbar_Create_Light))
                            {
                                CFG.Current.Toolbar_Create_Part = false;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = false;
                            }
                            ImguiUtils.ShowHoverTooltip("Create a BTL Light object.");
                        }

                        if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        ImguiUtils.ShowHoverTooltip("Create a Part object.");

                        if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        ImguiUtils.ShowHoverTooltip("Create a Region object.");

                        if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                        {
                            CFG.Current.Toolbar_Create_Light = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Part = false;
                        }
                        ImguiUtils.ShowHoverTooltip("Create an Event object.");
                        ImguiUtils.WrappedText("");


                        if (ImGui.Button("Create Object", defaultButtonSize))
                        {
                            Handler.ApplyObjectCreation();
                        }

                        ImguiUtils.WrappedText("");

                        if (CFG.Current.Toolbar_Create_Light)
                        {
                            // Nothing
                        }

                        if (CFG.Current.Toolbar_Create_Part)
                        {
                            ImguiUtils.WrappedText("Part Type:");
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
                                ImguiUtils.WrappedText("Region Type:");

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
                            ImguiUtils.WrappedText("Event Type:");
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
                ImguiUtils.WrappedText("Duplicate the current selection.");
                ImguiUtils.WrappedText("");

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
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
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                ImguiUtils.WrappedText("");

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
                Handler.DisplayDuplicateToMapMenu(true);
            }

            ///--------------------
            /// Move to Camera
            ///--------------------
            if (ImGui.CollapsingHeader("Move to Camera"))
            {
                ImguiUtils.WrappedText("Move the current selection to the camera position.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Camera Offset Distance:");

                ImGui.PushItemWidth(defaultButtonSize.X);
                if(ImGui.SliderFloat("##Offset distance", ref CFG.Current.Toolbar_Move_to_Camera_Offset, 0, 100))
                {
                    if (CFG.Current.Toolbar_Move_to_Camera_Offset < 0)
                        CFG.Current.Toolbar_Move_to_Camera_Offset = 0;

                    if (CFG.Current.Toolbar_Move_to_Camera_Offset > 100)
                        CFG.Current.Toolbar_Move_to_Camera_Offset = 100;
                }
                ImguiUtils.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the distance at which the current selection is offset from the camera when this action is used.");

                ImguiUtils.WrappedText("");

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
                ImguiUtils.WrappedText("Rotate the current selection by the following parameters.");
                ImguiUtils.WrappedText("");

                var rot = CFG.Current.Toolbar_Rotate_Increment;

                ImguiUtils.WrappedText("Rotation Type:");
                if (ImGui.Checkbox("X", ref CFG.Current.Toolbar_Rotate_X))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to X.");

                if (ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Rotate_Y))
                {
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to Y.");

                if (ImGui.Checkbox("Y Pivot", ref CFG.Current.Toolbar_Rotate_Y_Pivot))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Fixed_Rotate = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to Y and pivot with respect to others within the selection.");

                if (ImGui.Checkbox("Fixed Rotation", ref CFG.Current.Toolbar_Fixed_Rotate))
                {
                    CFG.Current.Toolbar_Rotate_Y = false;
                    CFG.Current.Toolbar_Rotate_X = false;
                    CFG.Current.Toolbar_Rotate_Y_Pivot = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the rotation axis to specified values below.");
                ImguiUtils.WrappedText("");

                if (!CFG.Current.Toolbar_Fixed_Rotate)
                {
                    ImguiUtils.WrappedText("Degree Increment:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    if(ImGui.SliderFloat("##Degree Increment", ref rot, -180.0f, 180.0f))
                    {
                        CFG.Current.Toolbar_Rotate_Increment = Math.Clamp(rot, -180.0f, 180.0f);
                    }
                    ImguiUtils.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

                    ImguiUtils.WrappedText("");
                }
                else
                {
                    var x = CFG.Current.Toolbar_Rotate_FixedAngle[0];
                    var y = CFG.Current.Toolbar_Rotate_FixedAngle[1];
                    var z = CFG.Current.Toolbar_Rotate_FixedAngle[2];

                    ImguiUtils.WrappedText("Fixed Rotation");
                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("X##fixedRotationX", ref x))
                    {
                        x = Math.Clamp(x, -360f, 360f);
                    }
                    ImguiUtils.ShowHoverTooltip("Set the X component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Y##fixedRotationX", ref y))
                    {
                        y = Math.Clamp(y, -360f, 360f);
                    }
                    ImguiUtils.ShowHoverTooltip("Set the Y component of the fixed rotation action.");

                    ImGui.PushItemWidth(100);
                    if (ImGui.InputFloat("Z##fixedRotationZ", ref z))
                    {
                        z = Math.Clamp(z, -360f, 360f);
                    }
                    ImguiUtils.ShowHoverTooltip("Set the Z component of the fixed rotation action.");
                    ImguiUtils.WrappedText("");

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
                ImguiUtils.WrappedText("Scramble the current selection's position, rotation and scale by the following parameters.");
                ImguiUtils.WrappedText("");

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
                ImguiUtils.WrappedText("Position");
                ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosX", ref randomOffsetMin_Pos_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position X co-ordinate.");

                ImGui.SameLine();

                ImGui.InputFloat("##offsetMaxPosX", ref randomOffsetMax_Pos_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position X co-ordinate.");

                ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosY", ref randomOffsetMin_Pos_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position Y co-ordinate.");

                ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Position in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the position Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the position Z co-ordinate.");
                ImGui.Text("");

                // Rotation
                ImguiUtils.WrappedText("Rotation");
                ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotX", ref randomOffsetMin_Rot_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotX", ref randomOffsetMax_Rot_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation X co-ordinate.");

                ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotY", ref randomOffsetMin_Rot_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Rotation in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the rotation Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the rotation Z co-ordinate.");
                ImGui.Text("");

                // Scale
                ImguiUtils.WrappedText("Scale");
                ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
                ImguiUtils.ShowHoverTooltip("Include the X co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleX", ref randomOffsetMin_Scale_X);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale X co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale X co-ordinate.");

                ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
                ImguiUtils.ShowHoverTooltip("Include the Y co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale Y co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale Y co-ordinate.");

                ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
                ImguiUtils.ShowHoverTooltip("Include the Z co-ordinate of the selection's Scale in the scramble.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
                ImguiUtils.ShowHoverTooltip("Minimum amount to add to the scale Z co-ordinate.");

                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
                ImguiUtils.ShowHoverTooltip("Maximum amount to add to the scale Z co-ordinate.");
                ImguiUtils.WrappedText("");

                ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
                ImguiUtils.ShowHoverTooltip("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");
                ImguiUtils.WrappedText("");

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
                ImguiUtils.WrappedText("Replicate the current selection by the following parameters.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Replicate Style:");
                if (ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Line shape.");

                if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                {
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Circle shape.");

                if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Square shape.");
                ImguiUtils.WrappedText("");

                // Line
                if (CFG.Current.Replicator_Mode_Line)
                {
                    ImguiUtils.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                    ImguiUtils.ShowHoverTooltip("The amount of new entities to create (from the first selection).");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Offset per Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                    ImguiUtils.ShowHoverTooltip("The distance between each newly created entity.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Replicate Direction:");
                    if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the X-axis.");

                    if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the Y-axis.");

                    if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the Z-axis.");
                    ImguiUtils.WrappedText("");

                    ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                    ImguiUtils.ShowHoverTooltip("When enabled, the position offset will be applied in the opposite direction.");
                }

                // Circle
                if (CFG.Current.Replicator_Mode_Circle)
                {
                    ImguiUtils.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points within the circle on which the entities are placed.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Circle Radius:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                    ImguiUtils.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nThe radius of the circle on which to place the entities.");

                    ImguiUtils.WrappedText("");

                    if (CFG.Current.Replicator_Circle_Size < 1)
                        CFG.Current.Replicator_Circle_Size = 1;

                }

                // Square
                if (CFG.Current.Replicator_Mode_Square)
                {
                    ImguiUtils.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Square_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points on one side of the square on which the entities are placed.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Square Width:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
                    ImguiUtils.ShowHoverTooltip("The width of the square on which to place the entities.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Square Height:");
                    ImGui.PushItemWidth(defaultButtonSize.X);
                    ImGui.InputFloat("##Depth", ref CFG.Current.Replicator_Square_Depth);
                    ImguiUtils.ShowHoverTooltip("The depth of the square on which to place the entities.");
                    ImguiUtils.WrappedText("");

                    if (CFG.Current.Replicator_Square_Width < 1)
                        CFG.Current.Replicator_Square_Width = 1;

                    if (CFG.Current.Replicator_Square_Size < 2)
                        CFG.Current.Replicator_Square_Size = 2;

                    if (CFG.Current.Replicator_Square_Depth < 1)
                        CFG.Current.Replicator_Square_Depth = 1;
                }

                ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
                ImguiUtils.ShowHoverTooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2 && Smithbox.ProjectType != ProjectType.AC6)
                {
                    if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID))
                    {
                        if (CFG.Current.Replicator_Increment_Entity_ID)
                        {
                            CFG.Current.Replicator_Clear_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
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
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Replicator_Clear_Entity_Group_IDs);
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                ImguiUtils.WrappedText("");

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
                ImguiUtils.WrappedText("Set the current selection to the closest grid position.");
                ImguiUtils.WrappedText("");

                ImGui.Checkbox("X", ref CFG.Current.Toolbar_Move_to_Grid_X);
                ImguiUtils.ShowHoverTooltip("Move the current selection to the closest X co-ordinate within the map grid.");

                ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                ImguiUtils.ShowHoverTooltip("Move the current selection to the closest Y co-ordinate within the map grid.");

                ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                ImguiUtils.ShowHoverTooltip("Move the current selection to the closest Z co-ordinate within the map grid.");

                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Grid Height");
                ImGui.PushItemWidth(defaultButtonSize.X);
                if (ImGui.SliderFloat("Grid height", ref CFG.Current.MapEditor_Viewport_Grid_Height, -10000, 10000))
                {
                    if (CFG.Current.MapEditor_Viewport_Grid_Height < -10000)
                        CFG.Current.MapEditor_Viewport_Grid_Height = -10000;

                    if (CFG.Current.MapEditor_Viewport_Grid_Height > 10000)
                        CFG.Current.MapEditor_Viewport_Grid_Height = 10000;
                }
                ImguiUtils.ShowHoverTooltip("Press Ctrl+Left Click to input directly.\nSet the current height of the map grid.");

                ImguiUtils.WrappedText("");

                if (ImGui.Button("Move Selection to Grid", defaultButtonSize))
                {
                    Handler.ApplyMovetoGrid();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
