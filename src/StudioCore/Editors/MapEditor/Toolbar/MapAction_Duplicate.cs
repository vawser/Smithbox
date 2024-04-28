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
    public static class MapAction_Duplicate
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Duplicate##tool_Selection_Duplicate", MapEditorState.SelectedAction == MapEditorAction.Selection_Duplicate))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Duplicate;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Duplicate)
            {
                ImGui.Text("Duplicate the current selection.");
                ImGui.Text("");

                if (Project.Type != ProjectType.DS2S)
                {
                    if(ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
                    {
                        if(CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
                }

                if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
                }

                if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
                {
                    ImGui.Checkbox("Increment UnkPartNames for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_UnkPartNames);
                    ImguiUtils.ShowHoverTooltip("When enabled, the duplicated Asset entities UnkPartNames property will be updated.");
                }

                if (Project.Type != ProjectType.DS2S)
                {
                    if(ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_ID))
                    {
                        if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                        {
                            CFG.Current.Toolbar_Duplicate_Increment_Entity_ID = false;
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

                    ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
                    ImguiUtils.ShowHoverTooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
                }

                if (Project.Type != ProjectType.DS2S)
                {
                    ImGui.Text("");
                }
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Duplicate)
            {
                if (ImGui.Button("Apply##action_Selection_Duplicate", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyDuplicate(_selection);
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
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Duplicate)
            {
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Core_Duplicate.HintText)}");
            }
        }

        public static void ApplyDuplicate(ViewportSelection _selection)
        {
            CloneMapObjectsAction action = new(MapEditorState.Universe, MapEditorState.Scene, _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
