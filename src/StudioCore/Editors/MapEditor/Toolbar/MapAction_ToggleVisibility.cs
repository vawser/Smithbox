using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_ToggleVisibility
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Toggle Visibility##tool_Selection_ToggleVisibility", MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Visibility))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Toggle_Visibility;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Visibility)
            {
                ImguiUtils.WrappedText("Toggle the visibility of the current selection or all objects within Smithbox.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Target:");
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
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("State:");
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
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Visibility)
            {
                if (ImGui.Button("Apply##action_Selection_Toggle_Visibility", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyToggleVisibility(_selection);
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
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Visibility)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Flip.HintText)} for Selection (Flip).");
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip.HintText)} for all Objects (Flip).");

                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled.HintText)} for Selection (Enabled).");
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled.HintText)} for all Objects (Enabled).");

                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled.HintText)} for Selection (Disabled).");
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled.HintText)} for all Objects (Disabled).");
            }
        }

        public static void ApplyToggleVisibility(ViewportSelection _selection)
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
