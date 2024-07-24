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
    public static class MapAction_Order
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("指令 Order##tool_Selection_Order", MapEditorState.SelectedAction == MapEditorAction.Selection_Order))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Order;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Order)
            {
                ImguiUtils.WrappedText("改变当前地图列表上选中的指令\nChange the current selection's order within the map object list.");
                ImguiUtils.WrappedText("");
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Order)
            {
                if (ImGui.Button("上 Move Up##action_Selection_OrderUp", new Vector2(150, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Up);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无选中对象 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Move the selected map object up one space within its map object list category. Has no effect if already the first entry.");

                ImGui.SameLine();

                if (ImGui.Button("下 Move Down##action_Selection_OrderDown", new Vector2(150, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Down);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无选中对象 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Move the selected map object down one space within its map object list category. Has no effect if already the last entry.");

                if (ImGui.Button("顶 Move to Top##action_Selection_OrderTop", new Vector2(150, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Top);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无选中对象 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Move the selected map object to the top of its map object list category. Has no effect if already the first entry.");

                ImGui.SameLine();

                if (ImGui.Button("底 Move to Bottom##action_Selection_OrderBottom", new Vector2(150, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Bottom);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无选中对象 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Move the selected map object to the bottom of its map object list category. Has no effect if already the last entry.");
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Order)
            {
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderUp.HintText)} for Move Up");
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderDown.HintText)} for Move Down");
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderTop.HintText)} for Move to Top");
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderBottom.HintText)} for Move to Bottom");
                ImguiUtils.WrappedText("");
            }
        }

        public static void MoveSelection(ViewportSelection _selection, OrderMoveDir dir)
        {
            OrderMapObjectsAction action = new(MapEditorState.Universe, MapEditorState.Scene, _selection.GetFilteredSelection<MsbEntity>().ToList(), dir);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
