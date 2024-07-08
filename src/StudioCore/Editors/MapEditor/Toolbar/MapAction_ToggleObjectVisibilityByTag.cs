using ImGuiNET;
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
    public static class MapAction_ToggleObjectVisibilityByTag
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Toggle Object Visibility by Tag##tool_Selection_Toggle_Object_Visibility_by_Tag", MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Object_Visibility_by_Tag))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Toggle_Object_Visibility_by_Tag;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }
        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Object_Visibility_by_Tag)
            {
                ImguiUtils.WrappedText("Toggle the visibility of map objects, filtering the " +
                    "\ntargets by tag.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Target Tag:");
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                ImguiUtils.ShowHoverTooltip("Specific which tag the map objects will be filtered by.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("State:");
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
                ImguiUtils.WrappedText("");
            }
        }
        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Object_Visibility_by_Tag)
            {
                if (ImGui.Button("Apply##action_Selection_Toggle_Object_Visibility_by_Tag", new Vector2(200, 32)))
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
                                foreach (var entry in Smithbox.BankHandler.AssetAliases.GetList())
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

                                foreach (var entry in Smithbox.BankHandler.MapPieceAliases.GetList())
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

        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Toggle_Object_Visibility_by_Tag)
            {

            }
        }
    }
}
