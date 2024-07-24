﻿using ImGuiNET;
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
    public static class MapAction_MoveToGrid
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Interface_MapEditor_Viewport_Grid)
            {
                if (ImGui.RadioButton("移至网格 Move to Grid##tool_Selection_Move_to_Grid", MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Grid))
                {
                    MapEditorState.SelectedAction = MapEditorAction.Selection_Move_to_Grid;
                }

                if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
                {
                    ImGui.SameLine();
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Grid)
            {
                ImguiUtils.WrappedText("将当前选择设置为最近的网格位置\nSet the current selection to the closest grid position.");
                ImguiUtils.WrappedText("");

                ImGui.Checkbox("X", ref CFG.Current.Toolbar_Move_to_Grid_X);
                ImguiUtils.ShowHoverTooltip("将当前选择移动到地图网格内最近的 X 坐标\nMove the current selection to the closest X coordinate within the map grid.");

                ImGui.SameLine();
                ImGui.Checkbox("Y", ref CFG.Current.Toolbar_Move_to_Grid_Y);
                ImguiUtils.ShowHoverTooltip("将当前选择移动到地图网格内最近的 Y 坐标\nMove the current selection to the closest Y coordinate within the map grid.");

                ImGui.SameLine();
                ImGui.Checkbox("Z", ref CFG.Current.Toolbar_Move_to_Grid_Z);
                ImguiUtils.ShowHoverTooltip("将当前选择移动到地图网格内最近的 Z 坐标\nMove the current selection to the closest Z coordinate within the map grid.");

                if (ImGui.Button("选择 Switch"))
                {
                    CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input = !CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input;
                }
                ImGui.SameLine();
                if (CFG.Current.Toolbar_Move_to_Grid_Specific_Height_Input)
                {
                    var height = CFG.Current.MapEditor_Viewport_Grid_Height;

                    ImGui.PushItemWidth(200);
                    ImGui.InputFloat("网格高 Grid height", ref height);
                    ImguiUtils.ShowHoverTooltip("设置当前地图网格高度 Set the current height of the map grid.");

                    if (height < -10000)
                        height = -10000;

                    if (height > 10000)
                        height = 10000;

                    CFG.Current.MapEditor_Viewport_Grid_Height = height;
                }
                else
                {
                    ImGui.PushItemWidth(200);
                    ImGui.SliderFloat("网格高 Grid height", ref CFG.Current.MapEditor_Viewport_Grid_Height, -10000, 10000);
                    ImguiUtils.ShowHoverTooltip("设置当前地图网格高度 Set the current height of the map grid.");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Grid)
            {
                if (ImGui.Button("应用 Apply##action_Selection_Move_to_Grid", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyMovetoGrid(_selection);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无选中对象 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
            }
        }

        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Move_to_Grid)
            {
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Set_to_Grid.HintText)}");
            }
        }

        public static void ApplyMovetoGrid(ViewportSelection _selection)
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetGridTransform(sel)));
            }

            CompoundAction action = new(actlist);
            MapEditorState.ActionManager.ExecuteAction(action);
        }

        public static Transform GetGridTransform(Entity sel)
        {
            Transform objT = sel.GetLocalTransform();

            var newTransform = Transform.Default;
            var newPos = objT.Position;
            var newRot = objT.Rotation;
            var newScale = objT.Scale;

            if (CFG.Current.Toolbar_Move_to_Grid_X)
            {
                float temp = newPos[0] / CFG.Current.MapEditor_Viewport_Grid_Square_Size;
                float newPosX = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_Viewport_Grid_Square_Size;

                newPos = new Vector3(newPosX, newPos[1], newPos[2]);
            }

            if (CFG.Current.Toolbar_Move_to_Grid_Z)
            {
                float temp = newPos[2] / CFG.Current.MapEditor_Viewport_Grid_Square_Size;
                float newPosZ = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_Viewport_Grid_Square_Size;

                newPos = new Vector3(newPos[0], newPos[1], newPosZ);
            }

            if (CFG.Current.Toolbar_Move_to_Grid_Y)
            {
                newPos = new Vector3(newPos[0], CFG.Current.MapEditor_Viewport_Grid_Height, newPos[2]);
            }

            newTransform.Position = newPos;
            newTransform.Rotation = newRot;
            newTransform.Scale = newScale;

            return newTransform;
        }
    }
}
