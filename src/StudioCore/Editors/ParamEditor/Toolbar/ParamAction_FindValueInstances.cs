using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Interface;
using System.Numerics;
using SoulsFormats;
using StudioCore.Banks.AliasBank;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_FindValueInstances
    {
        private static string _searchValue = "";
        private static string _cachedSearchValue = "";

        private static List<ParamValueResult> ParamResults = new();
        private static List<AliasValueResult> AliasResults = new();

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

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Value:");
                    ImGui.InputText("##searchValue", ref _searchValue, 255);
                    ImguiUtils.ShowHoverTooltip("The value to search for.");

                    ImGui.Checkbox("Initial Match Only", ref CFG.Current.Param_Toolbar_FindValueInstances_InitialMatchOnly);
                    ImguiUtils.ShowHoverTooltip("Only display the first match within a param, instead of all matches.");
                    ImguiUtils.WrappedText("");

                    var Size = ImGui.GetWindowSize();

                    float mult = 2;
                    if(AliasResults.Count > 0)
                    {
                        mult = 1;
                    }
                    float EditX = (Size.X / 100) * 95;
                    float EditY = (Size.Y / 100) * (35 * mult);

                    if (ParamResults.Count > 0)
                    {
                        ImguiUtils.WrappedText("Params:");
                        ImGui.BeginChild("##paramResultSection", new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));

                        // Param Results
                        ImguiUtils.WrappedText($"Value {_cachedSearchValue}: {ParamResults.Count} matches");

                        foreach (var result in ParamResults)
                        {
                            if (ImGui.Selectable($"{result.Param}: {result.Row}: {result.Field}##ValueSearcher"))
                            {
                                EditorCommandQueue.AddCommand($@"param/select/-1/{result.Param}/{result.Row}/{result.Field}");
                            }
                        }
                        ImGui.EndChild();
                    }

                    // Alias Results
                    if (AliasResults.Count > 0)
                    {
                        ImguiUtils.WrappedText("");
                        ImguiUtils.WrappedText("Aliases:");
                        ImGui.BeginChild("##aliasResultSection", new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));

                        ImguiUtils.WrappedText($"Value {_cachedSearchValue}: {AliasResults.Count} matches");

                        foreach (var result in AliasResults)
                        {
                            if (ImGui.Selectable($"{result.Alias}: {result.ID}: {result.Name}##AliasValueSearcher"))
                            {
                                
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
            if (ParamToolbar.SelectedAction == ParamToolbarAction.FindValueInstances)
            {
                if (ImGui.Button("Apply##action_Selection_FindValueInstances", new Vector2(200, 32)))
                {
                    ParamResults = new();
                    AliasResults = new();

                    SearchParamValue();
                    SearchAliasValue();
                }
            }
        }

        public static void SearchAliasValue()
        {
            _cachedSearchValue = _searchValue;

            // Cutscene
            if (Smithbox.BankHandler.CutsceneAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.CutsceneAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Cutscene");
                }
            }

            // Flag
            if (Smithbox.BankHandler.EventFlagAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.EventFlagAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Event Flag");
                }
            }

            // Characters
            if (Smithbox.BankHandler.CharacterAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Character");
                }
            }

            // Assets
            if (Smithbox.BankHandler.AssetAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Object");
                }
            }

            // Parts
            if (Smithbox.BankHandler.AssetAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.PartAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Part");
                }
            }

            // MapPieces
            if (Smithbox.BankHandler.AssetAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.MapAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Map Piece");
                }
            }

            // Movies
            if (Smithbox.BankHandler.MovieAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.MovieAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Movie");
                }
            }

            // Particles
            if (Smithbox.BankHandler.ParticleAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.ParticleAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Particle");
                }
            }

            // Sounds
            if (Smithbox.BankHandler.SoundAliases.Aliases != null)
            {
                foreach (var entry in Smithbox.BankHandler.SoundAliases.Aliases.list)
                {
                    AddAliasResult(entry, _searchValue, "Sound");
                }
            }
        }

        public static void AddAliasResult(AliasReference entry, string value, string aliasName)
        {
            if (entry.id == value)
            {
                AliasValueResult valueResult = new AliasValueResult();
                valueResult.Alias = aliasName;
                valueResult.ID = entry.id;
                valueResult.Name = entry.name;
                AliasResults.Add(valueResult);
            }
        }

        public static void SearchParamValue()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    _cachedSearchValue = _searchValue;
                    GetParamsWithValue(_searchValue);

                    if (ParamResults.Count > 0)
                    {
                        var message = $"Found value {_searchValue} in the following params:\n";
                        foreach (var result in ParamResults)
                        {
                            message += $"  {result.Param}\n";
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

        public static void GetParamsWithValue(string value)
        {
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
                        ParamValueResult paramValueResult = new ParamValueResult();
                        paramValueResult.Row = id.ToString();
                        paramValueResult.Param = p.Key;
                        paramValueResult.Field = fieldName;
                        ParamResults.Add(paramValueResult);

                        // Skip matching more if this is enabled
                        if(CFG.Current.Param_Toolbar_FindValueInstances_InitialMatchOnly)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public class ParamValueResult
    {
        public string Param;
        public string Row;
        public string Field;

        public ParamValueResult() { }
    }

    public class AliasValueResult
    {
        public string Alias;
        public string ID;
        public string Name;

        public AliasValueResult() { }
    }
}
