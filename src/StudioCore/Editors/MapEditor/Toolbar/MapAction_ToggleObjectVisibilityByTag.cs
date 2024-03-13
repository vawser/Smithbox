using ImGuiNET;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_ToggleObjectVisibilityByTag
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Toggle Object Visibility by Tag##tool_Selection_Toggle_Object_Visibility_by_Tag", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Toggle_Object_Visibility_by_Tag;

                if (ImGui.IsMouseDoubleClicked(0))
                {
                    Act(_selection);
                }
            }
        }
        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Toggle_Object_Visibility_by_Tag)
            {
                ImGui.Text("Toggle the visibility of map objects, filtering the targets by tag\n(you can view tags in the Model or Map Asset Browsers).");
                ImGui.Separator();

                ImGui.Text("Target Tag:");
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                ImguiUtils.ShowHoverTooltip("Specific which tag the map objects will be filtered by.");

                ImGui.Text("State:");
                if (ImGui.Checkbox("Visible", ref CFG.Current.Toolbar_Tag_Visibility_State_Enabled))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the visible state to enabled.");

                if (ImGui.Checkbox("Invisible", ref CFG.Current.Toolbar_Tag_Visibility_State_Disabled))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                }
                ImguiUtils.ShowHoverTooltip("Set the visible state to disabled.");
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            foreach (ObjectContainer m in MapEditorState.Universe.LoadedObjectContainers.Values)
            {
                if (m == null)
                {
                    continue;
                }

                foreach (Entity obj in m.Objects)
                {
                    if (obj.IsPart())
                    {
                        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("Objects"))
                        {
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entry.id == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }

                        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"))
                        {
                            var entryName = $"m{entry.id.Split("_").Last()}";
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entryName == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }

                        obj.UpdateRenderModel();
                    }
                }
            }
        }

    }
}
