using Google.Protobuf.WellKnownTypes;
using HKX2;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SoulsFormats.FFXDLSE;
using static SoulsFormats.GPARAM;
using static StudioCore.Editor.GparamValueChangeAction;

namespace StudioCore.Editors.GparamEditor
{
    public static class GparamQuickEdit
    {
        public enum EditEffectType
        {
            Set,
            Add,
            Subtract,
            Multiply,
            SetByRow
        }

        public static string filterArguments = "";

        public static string commandArguments = "";

        public static GPARAM.IField SelectedParamField;

        private static string _filterString = "";
        private static string _commandString = "";

        private static bool[] filterTruth = null;

        public static List<GparamValueChangeAction> actions = new List<GparamValueChangeAction>();

        public static void OnGui()
        {
            filterArguments = $"Filter arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character." +
            "\n" +
            "\n*" +
            "\nTargets all rows." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_ID}:<x>" +
            "\nTargets all rows with <x> ID." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_TimeOfDay}:<x>" +
            "\nTargets all rows with <x> Time of Day." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_Value}:[<x>]" +
            "\nTargets all rows with <x> Value." +
            "\nFor multi-values split them like so: [<x>,<x>]" +
            "\n";

            commandArguments = $"Command arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_Set}:[<x>]" +
            "\nSets target rows to <x> Value." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_Add}:[<x>]" +
            "\nAdds <x> to the Value of the target rows." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_Subtract}:[<x>]" +
            "\nSubtracts <x> from the Value of the target rows." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_Multiply}:[<x>]" +
            "\nMultiplies the Value of the target rows by <x>." +
            "\n" +
            $"\n{CFG.Current.Gparam_QuickEdit_SetByRow}:<x>" +
            "\nSets target rows to the Value of row ID <x>." +
            "\n";

            ImGui.Text("Filter: ");
            ImguiUtils.ShowWideHoverTooltip($"{filterArguments}");
            ImGui.SameLine();

            ImGui.InputText("##filterString", ref _filterString, 255);

            ImGui.Text("Command:");
            ImguiUtils.ShowWideHoverTooltip($"{commandArguments}");
            ImGui.SameLine();

            ImGui.InputText("##commandString", ref _commandString, 255);

            if(ImGui.Button("Execute"))
            {
                filterTruth = new bool[SelectedParamField.Values.Count];
                ExecuteQuickEdit();
            }
        }

        private static void ExecuteQuickEdit()
        {
            actions = new List<GparamValueChangeAction>();

            for (int i = 0; i < SelectedParamField.Values.Count; i++)
            {
                filterTruth[i] = false;
            }

            // Filter arguments
            var filters = _filterString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach(var filter in filters) 
            {
                FilterAll(filter);
                FilterId(filter);
                FilterTimeOfDay(filter);
                FilterValue(filter);
            }

            // Command arguments
            var commands = _commandString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach (var command in commands)
            {
                CommandAdjust(command, EditEffectType.Set);
                CommandAdjust(command, EditEffectType.Add);
                CommandAdjust(command, EditEffectType.Subtract);
                CommandAdjust(command, EditEffectType.Multiply);
                CommandAdjust(command, EditEffectType.SetByRow);
            }

            // Combine all the individual changes into a single action
            // so Undo/Redo treats the Quick Edit changes as one discrete change
            GparamBatchChangeAction batchAction = new GparamBatchChangeAction(actions);
            GparamEditorScreen.EditorActionManager.ExecuteAction(batchAction);
        }

        private static void CommandAdjust(string commandArg, EditEffectType effectType)
        {
            //TaskLogs.AddLog(commandArg);

            Match setValueMatch = null;

            if(effectType == EditEffectType.Set)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Set}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Add)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Add}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Subtract)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Subtract}:\[(.*)\]");
            }
            if (effectType == EditEffectType.Multiply)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Multiply}:\[(.*)\]");
            }
            if (effectType == EditEffectType.SetByRow)
            {
                setValueMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_SetByRow}:\[(.*)\]");
            }

            if (setValueMatch == null)
            {
                return;
            }

            // Ignore commands that are not set for bools
            if (SelectedParamField is GPARAM.BoolField)
            {
                if (effectType != EditEffectType.Set)
                {
                    return;
                }
            }

            if (setValueMatch.Success && setValueMatch.Groups.Count >= 2)
            {
                string setValue = setValueMatch.Groups[1].Value;

                int rowsetId = -1;
                int.TryParse(setValue, out rowsetId);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    // Only change if it matches filter truth
                    if (filterTruth[i] == true)
                    {
                        GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                        //TaskLogs.AddLog($"entry: {entry.Id} {entry.Value}");

                        // INT
                        if (SelectedParamField is GPARAM.IntField intField)
                        {
                            int commandValue = -1;
                            var valid = int.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if(effectType == EditEffectType.SetByRow)
                                {
                                    if (intField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = intField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // UINT
                        else if (SelectedParamField is GPARAM.UintField uintField)
                        {
                            uint commandValue = 0;
                            var valid = uint.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (uintField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = uintField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // SHORT
                        else if (SelectedParamField is GPARAM.ShortField shortField)
                        {
                            short commandValue = 0;
                            var valid = short.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (shortField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = shortField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // SBYTE
                        else if (SelectedParamField is GPARAM.SbyteField sbyteField)
                        {
                            sbyte commandValue = 0;
                            var valid = sbyte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (sbyteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = sbyteField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // BYTE
                        else if (SelectedParamField is GPARAM.ByteField byteField)
                        {
                            byte commandValue = 0;
                            var valid = byte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (byteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = byteField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // BOOL
                        else if (SelectedParamField is GPARAM.BoolField boolField)
                        {
                            int commandValue = 0;
                            var valid = int.TryParse(setValue, out commandValue);
                            var boolean = false;

                            if (commandValue == 1)
                                boolean = true;

                            if (valid)
                            {
                                // Always set bools
                                GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                        // FLOAT
                        else if (SelectedParamField is GPARAM.FloatField floatField)
                        {
                            float commandValue = 0.0f;
                            var valid = float.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (floatField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = floatField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR2
                        else if (SelectedParamField is GPARAM.Vector2Field vector2Field)
                        {
                            Vector2 commandValue = new Vector2(0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector2Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector2Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 1)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                if(valid1 && valid2)
                                {
                                    commandValue = new Vector2(commandValue1, commandValue2);

                                    if (effectType == EditEffectType.Set)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR3
                        else if (SelectedParamField is GPARAM.Vector3Field vector3Field)
                        {
                            Vector3 commandValue = new Vector3(0, 0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector3Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector3Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 2)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                float commandValue3 = 0.0f;
                                var valid3 = float.TryParse(parts[2], out commandValue3);

                                if (valid1 && valid2 && valid3)
                                {
                                    commandValue = new Vector3(commandValue1, commandValue2, commandValue3);

                                    if (effectType == EditEffectType.Set)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR4
                        else if (SelectedParamField is GPARAM.Vector4Field vector4Field)
                        {
                            Vector4 commandValue = new Vector4(0, 0, 0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector4Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector4Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }

                            if (!setValue.Contains(","))
                            {
                                continue;
                            }

                            var parts = setValue.Split(",");

                            if (parts.Length > 3)
                            {
                                float commandValue1 = 0.0f;
                                var valid1 = float.TryParse(parts[0], out commandValue1);

                                float commandValue2 = 0.0f;
                                var valid2 = float.TryParse(parts[1], out commandValue2);

                                float commandValue3 = 0.0f;
                                var valid3 = float.TryParse(parts[2], out commandValue3);

                                float commandValue4 = 0.0f;
                                var valid4 = float.TryParse(parts[3], out commandValue4);

                                if (valid1 && valid2 && valid3 && valid4)
                                {
                                    commandValue = new Vector4(commandValue1, commandValue2, commandValue3, commandValue4);

                                    if (effectType == EditEffectType.Set)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(SelectedParamField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FilterAll(string filterArg)
        {
            if (filterArg == "*")
            {
                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    filterTruth[i] = true;
                    //TaskLogs.AddLog($"Filter All: {entry.Id}");
                }
            }
        }

        private static void FilterId(string filterArg)
        {
            var idMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_ID}:([0-9]+)");

            if (idMatch.Success && idMatch.Groups.Count >= 2)
            {
                int targetId = -1;
                int.TryParse(idMatch.Groups[1].Value, out targetId);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    if (entry.Id == targetId)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched ID {targetId} - {entry.Id}");
                    }
                }
            }
        }

        private static void FilterTimeOfDay(string filterArg)
        {
            var todMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:([0-9]+)");

            if (todMatch.Success && todMatch.Groups.Count >= 2)
            {
                float targetTod = -1;
                float.TryParse(todMatch.Groups[1].Value, out targetTod);

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    if (entry.Unk04 == targetTod)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched Time of Day {targetTod} - {entry.Id}");
                    }
                }
            }
        }

        private static void FilterValue(string filterArg)
        {
            var valueMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_Value}:\[(.*)\]");

            if (valueMatch.Success && valueMatch.Groups.Count >= 2)
            {
                string targetValue = valueMatch.Groups[1].Value;

                for (int i = 0; i < SelectedParamField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = SelectedParamField.Values[i];

                    // INT
                    if (SelectedParamField is GPARAM.IntField intField)
                    {
                        int fieldValue = intField.Values[i].Value;

                        int commandValue = -1;
                        var valid = int.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value INT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // UINT
                    else if (SelectedParamField is GPARAM.UintField uintField)
                    {
                        uint fieldValue = uintField.Values[i].Value;

                        uint commandValue = 0;
                        var valid = uint.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value UINT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // SHORT
                    else if (SelectedParamField is GPARAM.ShortField shortField)
                    {
                        short fieldValue = shortField.Values[i].Value;

                        short commandValue = 0;
                        var valid = short.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value SHORT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // SBYTE
                    else if (SelectedParamField is GPARAM.SbyteField sbyteField)
                    {
                        sbyte fieldValue = sbyteField.Values[i].Value;

                        sbyte commandValue = 0;
                        var valid = sbyte.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value SBYTE {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // BYTE
                    else if (SelectedParamField is GPARAM.ByteField byteField)
                    {
                        byte fieldValue = byteField.Values[i].Value;

                        byte commandValue = 0;
                        var valid = byte.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value BYTE {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // BOOL
                    else if (SelectedParamField is GPARAM.BoolField boolField)
                    {
                        bool fieldValue = boolField.Values[i].Value;

                        bool commandValue = false;
                        var valid = bool.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value BOOL {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // FLOAT
                    else if (SelectedParamField is GPARAM.FloatField floatField)
                    {
                        float fieldValue = floatField.Values[i].Value;

                        float commandValue = 0.0f;
                        var valid = float.TryParse(targetValue, out commandValue);

                        if (valid)
                        {
                            if (fieldValue == commandValue)
                            {
                                filterTruth[i] = true;
                                //TaskLogs.AddLog($"Filter: Matched Value FLOAT {commandValue} - {entry.Id}");
                            }
                        }
                    }
                    // VECTOR2
                    else if (SelectedParamField is GPARAM.Vector2Field vector2Field)
                    {
                        Vector2 fieldValue = vector2Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 1)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            if (valid1 && valid2)
                            {
                                Vector2 commandVector = new Vector2(commandValue1, commandValue2);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR2 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                    // VECTOR3
                    else if (SelectedParamField is GPARAM.Vector3Field vector3Field)
                    {
                        Vector3 fieldValue = vector3Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 2)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            float commandValue3 = 0.0f;
                            var valid3 = float.TryParse(parts[2], out commandValue3);

                            if (valid1 && valid2 && valid3)
                            {
                                Vector3 commandVector = new Vector3(commandValue1, commandValue2, commandValue3);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR3 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                    // VECTOR4
                    else if (SelectedParamField is GPARAM.Vector4Field vector4Field)
                    {
                        Vector4 fieldValue = vector4Field.Values[i].Value;

                        if (!targetValue.Contains(","))
                        {
                            continue;
                        }

                        var parts = targetValue.Split(",");

                        if (parts.Length > 3)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            float commandValue3 = 0.0f;
                            var valid3 = float.TryParse(parts[2], out commandValue3);

                            float commandValue4 = 0.0f;
                            var valid4 = float.TryParse(parts[3], out commandValue4);


                            if (valid1 && valid2 && valid3 && valid4)
                            {
                                Vector4 commandVector = new Vector4(commandValue1, commandValue2, commandValue3, commandValue4);

                                if (fieldValue == commandVector)
                                {
                                    filterTruth[i] = true;
                                    //TaskLogs.AddLog($"Filter: Matched Value VECTOR4 {commandVector} - {entry.Id}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
