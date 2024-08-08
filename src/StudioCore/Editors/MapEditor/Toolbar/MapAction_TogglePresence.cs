using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_TogglePresence
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Toggle Presence##tool_Selection_Presence", MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Presence))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Toggle_Presence;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Presence)
            {
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                {
                    ImguiUtils.WrappedText("Toggle the load status of the current selection in-game.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Toggle the Dummy status of the current selection in-game.");
                    ImguiUtils.WrappedText("");
                }

                if (ImGui.Checkbox("Disable", ref CFG.Current.Toolbar_Presence_Dummify))
                {
                    CFG.Current.Toolbar_Presence_Undummify = false;
                }
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                    ImguiUtils.ShowHoverTooltip("Make the current selection Dummy Objects/Asset/Enemy types.");
                else
                    ImguiUtils.ShowHoverTooltip("Disable the current selection, preventing them from being loaded in-game.");

                if (ImGui.Checkbox("Enable", ref CFG.Current.Toolbar_Presence_Undummify))
                {
                    CFG.Current.Toolbar_Presence_Dummify = false;
                }
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                    ImguiUtils.ShowHoverTooltip("Make the current selection (if Dummy) normal Objects/Asset/Enemy types.");
                else
                    ImguiUtils.ShowHoverTooltip("Enable the current selection, allow them to be loaded in-game.");

                if (Smithbox.ProjectType == ProjectType.ER)
                {
                    ImGui.Checkbox("Use Game Edition Disable", ref CFG.Current.Toolbar_Presence_Dummy_Type_ER);
                    ImguiUtils.ShowHoverTooltip("Use the GameEditionDisable property to disable entities instead of the Dummy entity system.");
                }
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Presence)
            {
                if (ImGui.Button("Apply##action_Selection_Toggle_Presence", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyTogglePresence(_selection);
                    }
                }
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Presence)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MAP_MakeDummyObject.HintText)} for Disable");
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.MAP_MakeNormalObject.HintText)} for Enable");
            }
        }

        public static void ApplyTogglePresence(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
            {
                if (CFG.Current.Toolbar_Presence_Dummify)
                {
                    ER_DummySelection(_selection);
                }
                if (CFG.Current.Toolbar_Presence_Undummify)
                {
                    ER_UnDummySelection(_selection);
                }
            }
            else
            {
                if (CFG.Current.Toolbar_Presence_Dummify)
                {
                    DummySelection(_selection);
                }
                if (CFG.Current.Toolbar_Presence_Undummify)
                {
                    UnDummySelection(_selection);
                }
            }
        }

        public static void ER_DummySelection(ViewportSelection _selection)
        {
            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();
            foreach (MsbEntity s in sourceList)
            {
                if (Smithbox.ProjectType == ProjectType.ER)
                {
                    s.SetPropertyValue("GameEditionDisable", 1);
                }
            }
        }

        public static void ER_UnDummySelection(ViewportSelection _selection)
        {
            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();
            foreach (MsbEntity s in sourceList)
            {
                if (Smithbox.ProjectType == ProjectType.ER)
                {
                    s.SetPropertyValue("GameEditionDisable", 0);
                }
            }
        }

        public static void DummySelection(ViewportSelection _selection)
        {
            string[] sourceTypes = { "Enemy", "Object", "Asset" };
            string[] targetTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
            DummyUndummySelection(_selection, sourceTypes, targetTypes);
        }

        public static void UnDummySelection(ViewportSelection _selection)
        {
            string[] sourceTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
            string[] targetTypes = { "Enemy", "Object", "Asset" };
            DummyUndummySelection(_selection, sourceTypes, targetTypes);
        }

        private static void DummyUndummySelection(ViewportSelection _selection, string[] sourceTypes, string[] targetTypes)
        {
            Type msbclass;
            switch (Smithbox.ProjectType)
            {
                case ProjectType.DES:
                    msbclass = typeof(MSBD);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    msbclass = typeof(MSB1);
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    msbclass = typeof(MSB2);
                    //break;
                    return; //idk how ds2 dummies should work
                case ProjectType.DS3:
                    msbclass = typeof(MSB3);
                    break;
                case ProjectType.BB:
                    msbclass = typeof(MSBB);
                    break;
                case ProjectType.SDT:
                    msbclass = typeof(MSBS);
                    break;
                case ProjectType.ER:
                    msbclass = typeof(MSBE);
                    break;
                case ProjectType.AC6:
                    msbclass = typeof(MSB_AC6);
                    break;
                default:
                    throw new ArgumentException("type must be valid");
            }
            List<MsbEntity> sourceList = _selection.GetFilteredSelection<MsbEntity>().ToList();

            ChangeMapObjectType action = new(MapEditorState.Universe, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);
            MapEditorState.ActionManager.ExecuteAction(action);
        }
    }
}
