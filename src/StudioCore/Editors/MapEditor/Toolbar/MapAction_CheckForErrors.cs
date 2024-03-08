using HKX2;
using ImGuiNET;
using SoulsFormats;
using StudioCore.UserProject;
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
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Check for Errors##tool_Selection_Check_for_Errors", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Check_for_Errors;

                if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                {
                    if (MapEditorState.LoadedMaps.Any())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Check_for_Errors)
            {
                ImGui.Text("This tool will check for any incorrect property assignments.");
                ImGui.Text("Invalid assignments will be noted in the logger.");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            HashSet<uint> vals = new();
            bool hasError = false;

            foreach (var loadedMap in MapEditorState.LoadedMaps)
            {
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
                    if (Project.Type == ProjectType.ER)
                    {
                        if (e.WrappedObject is MSBE.Part)
                        {
                            MSBE.Part part = (MSBE.Part)e.WrappedObject;
                        }
                    }
                    if (Project.Type == ProjectType.SDT)
                    {
                        if (e.WrappedObject is MSBS.Part)
                        {
                            MSBS.Part part = (MSBS.Part)e.WrappedObject;
                        }
                    }
                    if (Project.Type == ProjectType.DS3)
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
