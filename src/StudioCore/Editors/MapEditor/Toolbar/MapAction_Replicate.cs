using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_Replicate
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Replicate)
            {
                if (ImGui.Selectable("Replicate##tool_Selection_Replicate", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapEditorState.CurrentTool = SelectedTool.Selection_Replicate;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Replicate)
            {
                ImGui.Text("Replicate the current selection by the following parameters.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Replicate.HintText)}");
                ImGui.Separator();

                if (ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Line shape.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
                {
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Circle shape.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Square shape.");

                // WIP
                /*
                ImGui.SameLine();
                if (ImGui.Checkbox("Sphere", ref CFG.Current.Replicator_Mode_Sphere))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Box = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Sphere shape.");

                ImGui.SameLine();
                if (ImGui.Checkbox("Box", ref CFG.Current.Replicator_Mode_Box))
                {
                    CFG.Current.Replicator_Mode_Circle = false;
                    CFG.Current.Replicator_Mode_Line = false;
                    CFG.Current.Replicator_Mode_Square = false;
                    CFG.Current.Replicator_Mode_Sphere = false;
                }
                ImguiUtils.ShowHoverTooltip("Replicate the first selection in the Box shape.");

                */

                // Line
                if (CFG.Current.Replicator_Mode_Line)
                {
                    ImGui.PushItemWidth(200);
                    ImGui.InputInt("Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                    ImguiUtils.ShowHoverTooltip("The amount of new entities to create (from the first selection).");

                    ImGui.PushItemWidth(200);
                    ImGui.InputInt("Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                    ImguiUtils.ShowHoverTooltip("The distance between each newly created entity.");

                    if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the X-axis.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the Y-axis.");

                    ImGui.SameLine();
                    if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                    {
                        CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                        CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Replicate on the Z-axis.");

                    ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                    ImguiUtils.ShowHoverTooltip("When enabled, the position offset will be applied in the opposite direction.");
                }

                // Circle
                if (CFG.Current.Replicator_Mode_Circle)
                {
                    ImGui.PushItemWidth(200);
                    ImGui.InputInt("Size", ref CFG.Current.Replicator_Circle_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points within the circle on which the entities are placed.");

                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Replicator_Circle_Radius_Specific_Input = !CFG.Current.Replicator_Circle_Radius_Specific_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Replicator_Circle_Radius_Specific_Input)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Radius", ref CFG.Current.Replicator_Circle_Radius);
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                    }
                    ImguiUtils.ShowHoverTooltip("The radius of the circle on which to place the entities.");

                    if (CFG.Current.Replicator_Circle_Size < 1)
                        CFG.Current.Replicator_Circle_Size = 1;

                }

                // Square
                if (CFG.Current.Replicator_Mode_Square)
                {
                    ImGui.PushItemWidth(200);
                    ImGui.InputInt("Size", ref CFG.Current.Replicator_Square_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points on one side of the square on which the entities are placed.");

                    ImGui.PushItemWidth(200);
                    ImGui.InputFloat("Width", ref CFG.Current.Replicator_Square_Width);
                    ImguiUtils.ShowHoverTooltip("The width of the square on which to place the entities.");

                    ImGui.PushItemWidth(200);
                    ImGui.InputFloat("Depth", ref CFG.Current.Replicator_Square_Depth);
                    ImguiUtils.ShowHoverTooltip("The depth of the square on which to place the entities.");

                    if (CFG.Current.Replicator_Square_Width < 1)
                        CFG.Current.Replicator_Square_Width = 1;

                    if (CFG.Current.Replicator_Square_Size < 2)
                        CFG.Current.Replicator_Square_Size = 2;

                    if (CFG.Current.Replicator_Square_Depth < 1)
                        CFG.Current.Replicator_Square_Depth = 1;
                }

                // Sphere
                if (CFG.Current.Replicator_Mode_Sphere)
                {
                    ImGui.PushItemWidth(200);
                    ImGui.InputInt("Size", ref CFG.Current.Replicator_Sphere_Size);
                    ImguiUtils.ShowHoverTooltip("The number of points within the sphere on which the entities are placed.");

                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input;
                    }

                    ImGui.SameLine();
                    if (CFG.Current.Replicator_Sphere_Horizontal_Radius_Specific_Input)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius);

                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Horizontal Radius", ref CFG.Current.Replicator_Sphere_Horizontal_Radius, 0.1f, 100);
                    }
                    ImguiUtils.ShowHoverTooltip("The radius of the sphere on which to place the entities.");

                    if (ImGui.Button("Switch"))
                    {
                        CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input = !CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input;
                    }
                    ImGui.SameLine();
                    if (CFG.Current.Replicator_Sphere_Vertical_Radius_Specific_Input)
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.InputFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius);
                    }
                    else
                    {
                        ImGui.PushItemWidth(200);
                        ImGui.SliderFloat("Vertical Radius", ref CFG.Current.Replicator_Sphere_Vertical_Radius, 0.1f, 100);
                    }
                    ImguiUtils.ShowHoverTooltip("The vertical radius of the sphere on which to place the entities.");

                    if (CFG.Current.Replicator_Sphere_Size < 1)
                        CFG.Current.Replicator_Sphere_Size = 1;
                }

                // Box

                // General Settings
                ImGui.Separator();

                ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
                ImguiUtils.ShowHoverTooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

                if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
                }

                if (Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Replicator_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            ReplicateMapObjectsAction action = new(MapEditorState.Toolbar, MapEditorState.Universe, MapEditorState.Scene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), MapEditorState.ActionManager);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
