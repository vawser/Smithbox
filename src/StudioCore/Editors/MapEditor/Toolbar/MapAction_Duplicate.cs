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
    public static class MapAction_Duplicate
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Duplicate##tool_Selection_Duplicate", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Duplicate;

                if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                {
                    Act(_selection);
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            CloneMapObjectsAction action = new(MapEditorState.Universe, MapEditorState.Scene, _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
            MapEditorState.ActionManager.ExecuteAction(action);
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Duplicate)
            {
                ImGui.Text("Duplicate the current selection.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Core_Duplicate.HintText)}");
                ImGui.Separator();

                if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
                }

                if (Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Project.Type == ProjectType.ER)
                {
                    ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }
            }
        }
    }
}
