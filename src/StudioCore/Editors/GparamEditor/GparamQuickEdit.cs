using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using static StudioCore.Editor.GparamValueChangeAction;

namespace StudioCore.Editors.GparamEditor
{
    public class GparamQuickEdit
    {
        public enum EditEffectType
        {
            Set,
            Add,
            Subtract,
            Multiply,
            SetByRow
        }

        public string filterArguments = "";

        public string commandArguments = "";

        private string _targetFileString = "";
        private string _targetGroupString = "";
        private string _targetFieldString = "";

        private string _filterString = "";
        private string _commandString = "";

        private bool[] filterTruth = null;

        public List<GparamValueChangeAction> actions = new List<GparamValueChangeAction>();

        public GparamEditorScreen Screen;

        public GparamQuickEdit(GparamEditorScreen screen)
        {
            Screen = screen;
        }

        public void DisplayCheatSheet()
        {
            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "File Filters:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"File arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"*");
            ImguiUtils.WrappedText("Targets all files.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"<name>");
            ImguiUtils.WrappedText("Targets the file with the specified name.");
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Group Filters:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"Group arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"*");
            ImguiUtils.WrappedText("Targets all groups.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"<name>");
            ImguiUtils.WrappedText("Targets the groups with the specified name.");
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Field Filters:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"Field arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"*");
            ImguiUtils.WrappedText("Targets all fields.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"<name>");
            ImguiUtils.WrappedText("Targets the fields with the specified name.");
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Value Filters:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"Filter arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"*");
            ImguiUtils.WrappedText("Targets all rows.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_ID}:<x>");
            ImguiUtils.WrappedText("Targets all rows with <x> ID.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:<x>");
            ImguiUtils.WrappedText("Targets all rows with <x> Time of Day.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_Value}:<x>");
            ImguiUtils.WrappedText("Targets all rows with <x> Value. For multi-values split them like so: [<x>,<x>]");
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Value Commands:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"Command arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_Set}:[<x>]");
            ImguiUtils.WrappedText("Sets target rows to <x> Value.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_Add}:[<x>]");
            ImguiUtils.WrappedText("Adds <x> to the Value of the target rows.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_Subtract}:[<x>]");
            ImguiUtils.WrappedText("Subtracts <x> from the Value of the target rows.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_Multiply}:[<x>]");
            ImguiUtils.WrappedText("Multiplies the Value of the target rows by <x>.");
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{CFG.Current.Gparam_QuickEdit_SetByRow}:[<x>]");
            ImguiUtils.WrappedText("Sets target rows to the Value of row ID <x>.");
            ImguiUtils.WrappedText("");
        }

        public GPARAM targetGparam;
        public GPARAM.Param targetParamGroup;
        public GPARAM.IField targetParamField;

        public void DisplayInputWindow()
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            ImGui.Text("File Filter:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText("##targetParamString", ref _targetFileString, 255);
            ImguiUtils.ShowHoverTooltip("Enter target file arguments here.");

            ImGui.Text("Group Filter:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText("##targetGroupString", ref _targetGroupString, 255);
            ImguiUtils.ShowHoverTooltip("Enter target group arguments here.");

            ImGui.Text("Field Filter:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText("##targetFieldString", ref _targetFieldString, 255);
            ImguiUtils.ShowHoverTooltip("Enter target field arguments here.");

            ImGui.Text("Value Filter:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText("##filterString", ref _filterString, 255);
            ImguiUtils.ShowHoverTooltip("Enter value filter arguments here.");

            ImGui.Text("Value Command:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText("##commandString", ref _commandString, 255);
            ImguiUtils.ShowHoverTooltip("Enter value command arguments here.");

            if (ImGui.Button("Execute", defaultButtonSize))
            {
                ExecuteQuickEdit();
            }
        }

        private List<string> resolvedList = new();
        private string curParamName = "";
        private string curGroupName = "";
        private string curFieldName = "";

        private void ExecuteQuickEdit()
        {
            resolvedList = new();

            List<string> targetedFiles = _targetFileString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}").ToList();
            List<string> targetedGroups = _targetGroupString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}").ToList();
            List<string> targetedFields = _targetFieldString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}").ToList();

            foreach (var (name, info) in GparamParamBank.ParamBank)
            {
                if (targetedFiles.Any(x => x == name) || _targetFileString == "*")
                {
                    targetGparam = info.Gparam;
                    GPARAM data = info.Gparam;
                    curParamName = name;

                    for (int i = 0; i < data.Params.Count; i++)
                    {
                        GPARAM.Param entry = data.Params[i];

                        if (targetedGroups.Any(x => x == entry.Name) || _targetGroupString == "*")
                        {
                            targetParamGroup = entry;
                            GPARAM.Param pData = targetParamGroup;
                            curGroupName = pData.Name;

                            for (int k = 0; k < pData.Fields.Count; k++)
                            {
                                GPARAM.IField fEntry = pData.Fields[k];

                                TaskLogs.AddLog($"Param Field: {fEntry.Name}");

                                if (targetedFields.Any(x => x == fEntry.Name) || _targetFieldString == "*")
                                {
                                    curFieldName = fEntry.Name;
                                    targetParamField = fEntry;
                                    // TODO: Return bundle of actions so we can execute them as one clean compound action
                                    ResolveQuickEdit(targetParamField);
                                    resolvedList.Add($"{curParamName}:{curGroupName}:{curFieldName}");
                                }
                            }
                        }
                    }
                }
            }

            if(resolvedList.Count > 0)
            {
                foreach(var entry in resolvedList)
                {
                    TaskLogs.AddLog($"Applied Quick Edit to: {entry}");
                }
            }
            else
            {
                TaskLogs.AddLog($"Quick Edit could not be applied.");
            }
        }

        public void UpdateFileFilter(string name)
        {
            if(_targetFileString != "")
            {
                _targetFileString = $"{_targetFileString}+{name}";
            }
            else
            {
                _targetFileString = $"{name}";
            }
        }
        public void UpdateGroupFilter(string name)
        {
            if (_targetGroupString != "")
            {
                _targetGroupString = $"{_targetGroupString}+{name}";
            }
            else
            {
                _targetGroupString = $"{name}";
            }
        }
        public void UpdateFieldFilter(string name)
        {
            if (_targetFieldString != "")
            {
                _targetFieldString = $"{_targetFieldString}+{name}";
            }
            else
            {
                _targetFieldString = $"{name}";
            }
        }

        private void ResolveQuickEdit(GPARAM.IField targetField)
        {
            filterTruth = new bool[targetField.Values.Count];
            actions = new List<GparamValueChangeAction>();

            for (int i = 0; i < targetField.Values.Count; i++)
            {
                filterTruth[i] = false;
            }

            // Filter arguments
            var filters = _filterString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach(var filter in filters) 
            {
                FilterAll(targetField, filter);
                FilterId(targetField, filter);
                FilterTimeOfDay(targetField, filter);
                FilterValue(targetField, filter);
            }

            // Command arguments
            var commands = _commandString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
            foreach (var command in commands)
            {
                CommandAdjust(targetField, command, EditEffectType.Set);
                CommandAdjust(targetField, command, EditEffectType.Add);
                CommandAdjust(targetField, command, EditEffectType.Subtract);
                CommandAdjust(targetField, command, EditEffectType.Multiply);
                CommandAdjust(targetField, command, EditEffectType.SetByRow);
            }

            // Combine all the individual changes into a single action
            // so Undo/Redo treats the Quick Edit changes as one discrete change
            GparamBatchChangeAction batchAction = new GparamBatchChangeAction(actions);
            GparamEditorScreen.EditorActionManager.ExecuteAction(batchAction);
        }

        private void CommandAdjust(GPARAM.IField targetField, string commandArg, EditEffectType effectType)
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
            if (targetField is GPARAM.BoolField)
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

                for (int i = 0; i < targetField.Values.Count; i++)
                {
                    // Only change if it matches filter truth
                    if (filterTruth[i] == true)
                    {
                        GPARAM.IFieldValue entry = targetField.Values[i];

                        //TaskLogs.AddLog($"entry: {entry.Id} {entry.Value}");

                        // INT
                        if (targetField is GPARAM.IntField intField)
                        {
                            int commandValue = -1;
                            var valid = int.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if(effectType == EditEffectType.SetByRow)
                                {
                                    if (intField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = intField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // UINT
                        else if (targetField is GPARAM.UintField uintField)
                        {
                            uint commandValue = 0;
                            var valid = uint.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (uintField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = uintField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // SHORT
                        else if (targetField is GPARAM.ShortField shortField)
                        {
                            short commandValue = 0;
                            var valid = short.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (shortField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = shortField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // SBYTE
                        else if (targetField is GPARAM.SbyteField sbyteField)
                        {
                            sbyte commandValue = 0;
                            var valid = sbyte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (sbyteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = sbyteField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // BYTE
                        else if (targetField is GPARAM.ByteField byteField)
                        {
                            byte commandValue = 0;
                            var valid = byte.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (byteField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = byteField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // BOOL
                        else if (targetField is GPARAM.BoolField boolField)
                        {
                            int commandValue = 0;
                            var valid = int.TryParse(setValue, out commandValue);
                            var boolean = false;

                            if (commandValue == 1)
                                boolean = true;

                            if (valid)
                            {
                                // Always set bools
                                GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                        // FLOAT
                        else if (targetField is GPARAM.FloatField floatField)
                        {
                            float commandValue = 0.0f;
                            var valid = float.TryParse(setValue, out commandValue);

                            if (valid)
                            {
                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.SetByRow)
                                {
                                    if (floatField.Values.Any(x => x.Id == rowsetId))
                                    {
                                        commandValue = floatField.Values.Find(x => x.Id == rowsetId).Value;

                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR2
                        else if (targetField is GPARAM.Vector2Field vector2Field)
                        {
                            Vector2 commandValue = new Vector2(0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector2Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector2Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
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
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR3
                        else if (targetField is GPARAM.Vector3Field vector3Field)
                        {
                            Vector3 commandValue = new Vector3(0, 0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector3Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector3Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
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
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                        // VECTOR4
                        else if (targetField is GPARAM.Vector4Field vector4Field)
                        {
                            Vector4 commandValue = new Vector4(0, 0, 0, 0);

                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (vector4Field.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = vector4Field.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
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
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Set);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Add)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Addition);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Subtract)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                        actions.Add(action);
                                    }
                                    if (effectType == EditEffectType.Multiply)
                                    {
                                        GparamValueChangeAction action = new GparamValueChangeAction(targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                        actions.Add(action);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FilterAll(GPARAM.IField targetField, string filterArg)
        {
            if (filterArg == "*")
            {
                for (int i = 0; i < targetField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = targetField.Values[i];

                    filterTruth[i] = true;
                    //TaskLogs.AddLog($"Filter All: {entry.Id}");
                }
            }
        }

        private void FilterId(GPARAM.IField targetField, string filterArg)
        {
            var idMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_ID}:([0-9]+)");

            if (idMatch.Success && idMatch.Groups.Count >= 2)
            {
                int targetId = -1;
                int.TryParse(idMatch.Groups[1].Value, out targetId);

                for (int i = 0; i < targetField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = targetField.Values[i];

                    if (entry.Id == targetId)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched ID {targetId} - {entry.Id}");
                    }
                }
            }
        }

        private void FilterTimeOfDay(GPARAM.IField targetField, string filterArg)
        {
            var todMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:([0-9]+)");

            if (todMatch.Success && todMatch.Groups.Count >= 2)
            {
                float targetTod = -1;
                float.TryParse(todMatch.Groups[1].Value, out targetTod);

                for (int i = 0; i < targetField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = targetField.Values[i];

                    if (entry.Unk04 == targetTod)
                    {
                        filterTruth[i] = true;
                        //TaskLogs.AddLog($"Filter: Matched Time of Day {targetTod} - {entry.Id}");
                    }
                }
            }
        }

        private void FilterValue(GPARAM.IField targetField, string filterArg)
        {
            var valueMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_Value}:\[(.*)\]");

            if (valueMatch.Success && valueMatch.Groups.Count >= 2)
            {
                string targetValue = valueMatch.Groups[1].Value;

                for (int i = 0; i < targetField.Values.Count; i++)
                {
                    GPARAM.IFieldValue entry = targetField.Values[i];

                    // INT
                    if (targetField is GPARAM.IntField intField)
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
                    else if (targetField is GPARAM.UintField uintField)
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
                    else if (targetField is GPARAM.ShortField shortField)
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
                    else if (targetField is GPARAM.SbyteField sbyteField)
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
                    else if (targetField is GPARAM.ByteField byteField)
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
                    else if (targetField is GPARAM.BoolField boolField)
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
                    else if (targetField is GPARAM.FloatField floatField)
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
                    else if (targetField is GPARAM.Vector2Field vector2Field)
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
                    else if (targetField is GPARAM.Vector3Field vector3Field)
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
                    else if (targetField is GPARAM.Vector4Field vector4Field)
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
