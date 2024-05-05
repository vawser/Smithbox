using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor.Toolbar;
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
using SoulsFormats;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_FindValueInstances
    {
        private static string _searchValue = "";
        private static string _cachedSearchValue = "";

        private static List<int> _rowIdResults = new();
        private static List<string> _paramResults = new();
        private static List<string> _fieldNameResults = new();

        public static void Select()
        {
            if (ImGui.RadioButton("Find Instances of Value##tool_SearchValueInstances", ParamToolbar.SelectedAction == ParamToolbarAction.FindValueInstances))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.FindValueInstances;
            }
            ImguiUtils.ShowHoverTooltip("Use this to search for all instances of a specific value.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.FindValueInstances)
            {
                ImguiUtils.WrappedText("Display all instances of a specificed value.");
                ImguiUtils.WrappedText("");

                if (!ParamEditorScreen._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Value:");
                    ImGui.InputText("##searchValue", ref _searchValue, 255);
                    ImguiUtils.ShowHoverTooltip("The value to search for.");

                    ImguiUtils.WrappedText("");

                    if (_paramResults.Count > 0)
                    {
                        ImguiUtils.WrappedText($"Value {_cachedSearchValue}: {_paramResults.Count} matches");

                        for(int i = 0; i < _paramResults.Count; i++)
                        {
                            var paramName = _paramResults[i];
                            var rowId = _rowIdResults[i];
                            var fieldName = _fieldNameResults[i];

                            if (ImGui.Selectable($"{paramName}: {fieldName}##ValueSearcher"))
                            {
                                EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{rowId}/{fieldName}");
                            }
                        }
                    }

                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.FindValueInstances)
            {
                if (ImGui.Button("Apply##action_Selection_FindValueInstances", new Vector2(200, 32)))
                {
                    _paramResults = new();
                    _rowIdResults = new();
                    _fieldNameResults = new();

                    SearchValue();
                }

            }
        }

        public static void SearchValue()
        {
            var selectedParam = ParamEditorScreen._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    _cachedSearchValue = _searchValue;
                    (_rowIdResults, _paramResults, _fieldNameResults) = GetParamsWithValue(_searchValue);

                    if (_paramResults.Count > 0)
                    {
                        var message = $"Found value {_searchValue} in the following params:\n";
                        foreach (var line in _paramResults)
                        {
                            message += $"  {line}\n";
                            TaskLogs.AddLog(message,
                                LogLevel.Information, TaskLogs.LogPriority.Low);
                        }
                    }
                    else
                    {
                        TaskLogs.AddLog($"No params found with value {_searchValue}",
                            LogLevel.Information, TaskLogs.LogPriority.High);
                    }
                }
            }
        }

        public static (List<int>, List<string>, List<string>) GetParamsWithValue(string value)
        {
            List<int> rowIdOutput = new();
            List<string> valueOutput = new();
            List<string> fieldOutput = new();

            foreach (var p in ParamBank.PrimaryBank.Params)
            {
                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    var success = false;
                    var isMatch = false;
                    var r = p.Value.Rows[i];
                    var id = r.ID;
                    string fieldName = "";

                    foreach(var field in r.Cells)
                    {
                        PARAMDEF.DefType type = field.Def.DisplayType;

                        switch (type)
                        {
                            case PARAMDEF.DefType.s8:
                                sbyte sbyteVal;
                                success = sbyte.TryParse(value, out sbyteVal);
                                if (success)
                                {
                                    if (sbyteVal == (sbyte)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.u8:
                                byte byteVal;
                                success = byte.TryParse(value, out byteVal);
                                if (success)
                                {
                                    if (byteVal == (byte)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.s16:
                                short shortVal;
                                success = short.TryParse(value, out shortVal);
                                if (success)
                                {
                                    if (shortVal == (short)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.u16:
                                ushort ushortVal;
                                success = ushort.TryParse(value, out ushortVal);
                                if (success)
                                {
                                    if (ushortVal == (ushort)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.s32:
                                int intVal;
                                success = int.TryParse(value, out intVal);
                                if (success)
                                {
                                    if (intVal == (int)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.u32:
                                uint uintVal;
                                success = uint.TryParse(value, out uintVal);
                                if (success)
                                {
                                    if (uintVal == (uint)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.f32:
                                float floatVal;
                                success = float.TryParse(value, out floatVal);
                                if (success)
                                {
                                    if (floatVal == (float)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.b32:
                                bool boolVal;
                                success = bool.TryParse(value, out boolVal);
                                if (success)
                                {
                                    if (boolVal == (bool)field.Value)
                                    {
                                        fieldName = field.Def.InternalName;
                                        isMatch = true;
                                    }
                                }
                                break;
                            case PARAMDEF.DefType.fixstr:
                            case PARAMDEF.DefType.fixstrW:
                                string strVal = value;
                                if (strVal == (string)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                                break;
                            default: break;
                        }
                    }

                    if (isMatch)
                    {
                        rowIdOutput.Add(id);
                        valueOutput.Add(p.Key);
                        fieldOutput.Add(fieldName);
                        break;
                    }
                }
            }

            return (rowIdOutput, valueOutput, fieldOutput);
        }
    }
}
