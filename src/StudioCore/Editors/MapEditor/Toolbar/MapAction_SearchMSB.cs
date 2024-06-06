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
    public static class MapAction_Search_MSB
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Find Value Instances##tool_Selection_MapAction_Search_MSB", MapEditorState.SelectedAction == MapEditorAction.Selection_Search_MSB))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Search_MSB;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Search_MSB)
            {
                ImguiUtils.WrappedText("Display all instances of a specificed value within the map files.");
                ImguiUtils.WrappedText("");
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Search_MSB)
            {
                
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Search_MSB)
            {
            }
        }

        public static void SearchMapsForValue(ViewportSelection _selection, string searchValue)
        {

        }
    }
}
