using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_GoToInObjectList
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Go to in Object List##tool_Selection_GoToInObjectList", MapEditorState.SelectedAction == MapEditorAction.Selection_Go_to_in_Object_List))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Go_to_in_Object_List;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }
        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Go_to_in_Object_List)
            {
                ImguiUtils.WrappedText("Select the associated map object in the map object list based upon our viewport selection (first if multiple are selected).");
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Go_to_in_Object_List)
            {
                if (ImGui.Button("Apply##action_Selection_Go_to_in_Object_List", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyGoToInObjectList(_selection);
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
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Go_to_in_Object_List)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List.HintText)}");
            }
        }

        public static void ApplyGoToInObjectList(ViewportSelection _selection)
        {
            _selection.GotoTreeTarget = _selection.GetSingleSelection();
        }
    }
}
