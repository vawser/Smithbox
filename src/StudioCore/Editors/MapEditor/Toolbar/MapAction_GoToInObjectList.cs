using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_GoToInObjectList
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Go to in Object List##tool_Selection_GoToInObjectList", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Go_to_in_Object_List;

                if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                {
                    Act(_selection);
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            _selection.GotoTreeTarget = _selection.GetSingleSelection();
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Go_to_in_Object_List)
            {
                ImGui.Text("Move the camera to the current selection (first if multiple are selected).");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List.HintText)}");
                ImGui.Separator();
            }
        }
    }
}
