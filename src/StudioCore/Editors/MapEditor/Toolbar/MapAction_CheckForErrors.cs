using HKX2;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_CheckForErrors
    {

        private static (string, ObjectContainer) _targetMap;

        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("Check Entity ID Errors##tool_Selection_Check_for_Errors", MapEditorState.SelectedAction == MapEditorAction.Selection_Check_for_Errors))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Check_for_Errors;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Check_for_Errors)
            {
                ImguiUtils.WrappedText("This tool will check for any incorrect property assignments.");
                ImguiUtils.WrappedText("Invalid assignments will be noted in the logger.");
                ImguiUtils.WrappedText("");

                if (MapEditorState.Universe.LoadedObjectContainers == null)
                {
                    ImguiUtils.WrappedText("No maps have been loaded yet.");
                    ImguiUtils.WrappedText("");
                }
                else if (MapEditorState.Universe.LoadedObjectContainers != null && !MapEditorState.Universe.LoadedObjectContainers.Any())
                {
                    ImguiUtils.WrappedText("No maps have been loaded yet.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Target Map:");
                    if (ImGui.BeginCombo("##Targeted Map", _targetMap.Item1))
                    {
                        foreach (var obj in MapEditorState.Universe.LoadedObjectContainers)
                        {
                            if (obj.Value != null)
                            {
                                if (ImGui.Selectable(obj.Key))
                                {
                                    _targetMap = (obj.Key, obj.Value);
                                    break;
                                }
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Check_for_Errors)
            {
                if (ImGui.Button("Apply##action_Selection_Check_for_Errors", new Vector2(200, 32)))
                {
                    ApplyErrorCheck(_selection);
                }
            }

        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Check_for_Errors)
            {
            }
        }

        public static void ApplyErrorCheck(ViewportSelection _selection)
        {
            if (MapEditorState.Universe.LoadedObjectContainers == null)
                return;

            if (!MapEditorState.Universe.LoadedObjectContainers.Any())
                return;

            HashSet<uint> vals = new();
            bool hasError = false;

            if(_targetMap != (null, null))
            {
                var loadedMap = (MapContainer)_targetMap.Item2;

                // Entity ID
                foreach (var e in loadedMap?.Objects)
                {
                    var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);

                    if (val == null)
                        continue;

                    uint entUint;

                    if (val is int entInt)
                        entUint = (uint)entInt;
                    else
                        entUint = (uint)val;

                    if (entUint == 0 || entUint == uint.MaxValue)
                        continue;

                    if (!vals.Add(entUint))
                    {
                        vals.Add(entUint);

                        hasError = true;
                        TaskLogs.AddLog($"Duplicate Entity ID: {entUint.ToString()} in {e.Name}");
                    }
                }

                // Entity Group ID
                foreach (var e in loadedMap?.Objects)
                {
                    if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                    {
                        if (e.WrappedObject is MSBE.Part)
                        {
                            MSBE.Part part = (MSBE.Part)e.WrappedObject;

                            List<uint> checkedEntityGroups = new List<uint>();

                            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                            {
                                if (part.EntityGroupIDs[i] == 0)
                                    continue;

                                if (checkedEntityGroups.Count > 0)
                                {
                                    foreach (var group in checkedEntityGroups)
                                    {
                                        if (part.EntityGroupIDs[i] == group)
                                        {
                                            hasError = true;
                                            TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                        }
                                    }
                                }

                                checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                            }
                        }
                    }
                    if (Smithbox.ProjectType == ProjectType.SDT)
                    {
                        if (e.WrappedObject is MSBS.Part)
                        {
                            MSBS.Part part = (MSBS.Part)e.WrappedObject;

                            List<int> checkedEntityGroups = new List<int>();

                            for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                            {
                                if (part.EntityGroupIDs[i] == -1)
                                    continue;

                                if (checkedEntityGroups.Count > 0)
                                {
                                    foreach (var group in checkedEntityGroups)
                                    {
                                        if (part.EntityGroupIDs[i] == group)
                                        {
                                            hasError = true;
                                            TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                        }
                                    }
                                }

                                checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                            }
                        }
                    }
                    if (Smithbox.ProjectType == ProjectType.DS3)
                    {
                        if (e.WrappedObject is MSB3.Part)
                        {
                            MSB3.Part part = (MSB3.Part)e.WrappedObject;

                            List<int> checkedEntityGroups = new List<int>();

                            for (int i = 0; i < part.EntityGroups.Length; i++)
                            {
                                if (part.EntityGroups[i] == -1)
                                    continue;

                                if (checkedEntityGroups.Count > 0)
                                {
                                    foreach (var group in checkedEntityGroups)
                                    {
                                        if (part.EntityGroups[i] == group)
                                        {
                                            hasError = true;
                                            TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroups[i].ToString()} in {e.Name}");
                                        }
                                    }
                                }

                                checkedEntityGroups.Add(part.EntityGroups[i]);
                            }
                        }
                    }
                }
            }

            if(!hasError)
            {
                TaskLogs.AddLog($"No errors found.");
            }
        }
    }
}
