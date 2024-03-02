using ImGuiNET;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_ToggleVisibility
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Toggle Visibility##tool_Selection_ToggleVisibility", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Toggle_Visibility;

                if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                {
                    Act(_selection);
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Toggle_Visibility)
            {
                ImGui.Text("Toggle the visibility of the current selection or all objects.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Flip.HintText)} for Selection (Flip).");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip.HintText)} for all Objects (Flip).");

                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled.HintText)} for Selection (Enabled).");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled.HintText)} for all Objects (Enabled).");

                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled.HintText)} for Selection (Disabled).");
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled.HintText)} for all Objects (Disabled).");

                ImGui.Separator();

                ImGui.Text("Target:");
                if (ImGui.Checkbox("Selection", ref CFG.Current.Toolbar_Visibility_Target_Selection))
                {
                    CFG.Current.Toolbar_Visibility_Target_All = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the target state to our current selection.");

                if (ImGui.Checkbox("All", ref CFG.Current.Toolbar_Visibility_Target_All))
                {
                    CFG.Current.Toolbar_Visibility_Target_Selection = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the target state to all objects.");

                ImGui.Separator();
                ImGui.Text("State:");
                if (ImGui.Checkbox("Visible", ref CFG.Current.Toolbar_Visibility_State_Enabled))
                {
                    CFG.Current.Toolbar_Visibility_State_Disabled = false;
                    CFG.Current.Toolbar_Visibility_State_Flip = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the target selection visible state to enabled.");

                if (ImGui.Checkbox("Invisible", ref CFG.Current.Toolbar_Visibility_State_Disabled))
                {
                    CFG.Current.Toolbar_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Visibility_State_Flip = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the target selection visible state to disabled.");

                if (ImGui.Checkbox("Flip", ref CFG.Current.Toolbar_Visibility_State_Flip))
                {
                    CFG.Current.Toolbar_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Visibility_State_Disabled = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the target selection visible state to opposite of its current state.");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Visibility_Target_Selection)
            {
                HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();

                foreach (Entity s in selected)
                {
                    if (CFG.Current.Toolbar_Visibility_State_Enabled)
                        s.EditorVisible = true;

                    if (CFG.Current.Toolbar_Visibility_State_Disabled)
                        s.EditorVisible = false;

                    if (CFG.Current.Toolbar_Visibility_State_Flip)
                        s.EditorVisible = !s.EditorVisible;
                }
            }
            if (CFG.Current.Toolbar_Visibility_Target_All)
            {
                foreach (ObjectContainer m in MapEditorState.Universe.LoadedObjectContainers.Values)
                {
                    if (m == null)
                    {
                        continue;
                    }

                    foreach (Entity obj in m.Objects)
                    {
                        if (CFG.Current.Toolbar_Visibility_State_Enabled)
                            obj.EditorVisible = true;

                        if (CFG.Current.Toolbar_Visibility_State_Disabled)
                            obj.EditorVisible = false;

                        if (CFG.Current.Toolbar_Visibility_State_Flip)
                            obj.EditorVisible = !obj.EditorVisible;
                    }
                }
            }
        }

        public static void ForceVisibilityState(bool visible, bool invisible, bool flip)
        {
            CFG.Current.Toolbar_Visibility_State_Enabled = visible;
            CFG.Current.Toolbar_Visibility_State_Disabled = invisible;
            CFG.Current.Toolbar_Visibility_State_Flip = flip;
        }
    }
}
