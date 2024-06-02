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
            if (ImGui.RadioButton("Reorder##tool_Selection_Order", MapEditorState.SelectedAction == MapEditorAction.Selection_Order))
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
                ImguiUtils.WrappedText("Change the current selection order within the map object list.");
                ImguiUtils.WrappedText("");
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Order)
            {
                if (ImGui.Button("Move Up##action_Selection_OrderUp", new Vector2(100, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Up);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Move Down##action_Selection_OrderDown", new Vector2(100, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        MoveSelection(_selection, OrderMoveDir.Down);
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
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Order)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderUp.HintText)}");
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MapEditor_MoveOrderDown.HintText)}");
            }
        }

        public static void MoveSelection(ViewportSelection _selection, OrderMoveDir dir)
        {
            OrderMapObjectsAction action = new(MapEditorState.Universe, MapEditorState.Scene, _selection.GetFilteredSelection<MsbEntity>().ToList(), dir);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
