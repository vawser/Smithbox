using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_Replicate
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Replicate##tool_Selection_Replicate", MapEditorState.SelectedAction == MapEditorAction.Selection_Replicate))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Replicate;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Replicate)
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
                    ImGui.PushItemWidth(300);
                    ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                    ImguiUtils.ShowHoverTooltip("The amount of new entities to create (from the first selection).");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Offset per Replicate:");
                    ImGui.PushItemWidth(300);
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
                    ImGui.PushItemWidth(300);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points within the circle on which the entities are placed.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Circle Radius:");
                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Replicator_Circle_Radius_Specific_Input = !CFG.Current.Replicator_Circle_Radius_Specific_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Replicator_Circle_Radius_Specific_Input)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius);
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                    }
                    ImguiUtils.ShowHoverTooltip("The radius of the circle on which to place the entities.");
                    ImguiUtils.WrappedText("");

                    if (CFG.Current.Replicator_Circle_Size < 1)
                        CFG.Current.Replicator_Circle_Size = 1;

                }

                // Square
                if (CFG.Current.Replicator_Mode_Square)
                {
                    ImguiUtils.WrappedText("Amount to Replicate:");
                    ImGui.PushItemWidth(300);
                    ImGui.InputInt("##Size", ref CFG.Current.Replicator_Square_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points on one side of the square on which the entities are placed.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Square Width:");
                    ImGui.PushItemWidth(300);
                    ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
                    ImguiUtils.ShowHoverTooltip("The width of the square on which to place the entities.");
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Square Height:");
                    ImGui.PushItemWidth(300);
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

                if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.DS2 && Project.Type != ProjectType.AC6)
                {
                    if(ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID))
                    {
                        if(CFG.Current.Replicator_Increment_Entity_ID)
                        {
                            CFG.Current.Replicator_Clear_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                }

                if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }

                if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.DS2)
                {
                    if(ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Replicator_Clear_Entity_ID))
                    {
                        if(CFG.Current.Replicator_Clear_Entity_ID)
                        {
                            CFG.Current.Replicator_Increment_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Replicator_Clear_Entity_Group_IDs);
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Replicate)
            {
                if (ImGui.Button("Apply##action_Selection_Replicate", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyReplicate(_selection);
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
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Replicate)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Replicate.HintText)}");
            }
        }

        public static void ApplyReplicate(ViewportSelection _selection)
        {
            ReplicateMapObjectsAction action = new(MapEditorState.Toolbar, MapEditorState.Universe, MapEditorState.Scene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), MapEditorState.ActionManager);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
