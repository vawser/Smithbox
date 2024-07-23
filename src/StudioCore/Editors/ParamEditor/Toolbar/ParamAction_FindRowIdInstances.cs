using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_FindRowIdInstances
    {
        private static int _searchID = 0;
        private static int _cachedSearchID = 0;
        private static int _searchIndex = -1;
        private static List<string> _paramResults = new();

        public static void Select()
        {
            if (ImGui.RadioButton("Find Instances of Row ID##tool_SearchRowID", ParamToolbar.SelectedAction == ParamToolbarAction.FindRowIdInstances))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.FindRowIdInstances;
            }
            ImguiUtils.ShowHoverTooltip("Use this to search for all instances of a specific row ID.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.FindRowIdInstances)
            {
                ImguiUtils.WrappedText("Display all instances of a specificed row ID.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Row ID:");
                    ImGui.InputInt("##searchRowId", ref _searchID);
                    ImguiUtils.ShowHoverTooltip("The row ID to search for.");

                    ImguiUtils.WrappedText("Row Index:");
                    ImGui.InputInt("##searchRowIndex", ref _searchIndex);
                    ImguiUtils.ShowHoverTooltip("The row index to search for. -1 for any");

                    ImguiUtils.WrappedText("");

                    if (_paramResults.Count > 0)
                    {
                        var Size = ImGui.GetWindowSize();
                        float EditX = (Size.X / 100) * 95;
                        float EditY = (Size.Y / 100) * 75;

                        ImGui.BeginChild("##resultSection", new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));
                        ImguiUtils.WrappedText($"ID {_cachedSearchID}: {_paramResults.Count} matches");

                        foreach (var paramName in _paramResults)
                        {
                            if (ImGui.Selectable($"{paramName}##RowSearcher"))
                            {
                                EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{_cachedSearchID}");
                            }
                        }

                        ImGui.EndChild();
                    }

                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.FindRowIdInstances)
            {
                if (ImGui.Button("Apply##action_Selection_SearchRowID", new Vector2(200, 32)))
                {
                    SearchRowID();
                }

            }
        }

        public static void SearchRowID()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    _cachedSearchID = _searchID;
                    _paramResults = GetParamsWithRowID(_searchID, _searchIndex);

                    if (_paramResults.Count > 0)
                    {
                        var message = $"Found row ID {_searchID} in the following params:\n";
                        foreach (var line in _paramResults)
                            message += $"  {line}\n";
                        TaskLogs.AddLog(message,
                            LogLevel.Information, TaskLogs.LogPriority.Low);
                    }
                    else
                    {
                        TaskLogs.AddLog($"No params found with row ID {_searchID}",
                            LogLevel.Information, TaskLogs.LogPriority.High);
                    }
                }
            }
        }

        public static List<string> GetParamsWithRowID(int id, int index)
        {
            List<string> output = new();
            foreach (var p in ParamBank.PrimaryBank.Params)
            {
                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    var r = p.Value.Rows[i];
                    if (r.ID == id && (index == -1 || index == i))
                    {
                        output.Add(p.Key);
                        break;
                    }
                }
            }

            return output;
        }
    }
}
