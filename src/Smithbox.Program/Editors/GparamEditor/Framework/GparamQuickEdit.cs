using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamQuickEdit
{
    private string _targetFileString = "";
    private string _targetGroupString = "";
    private string _targetFieldString = "";
    private string _valueFilterString = "";
    private string _valueCommandString = "";

    private bool[] filterTruth = null;

    public List<GparamValueChangeAction> actions = new List<GparamValueChangeAction>();

    public GparamEditorScreen Screen;

    public FileDictionaryEntry TargetFile;
    public GPARAM targetGparam;
    public GPARAM.Param targetParamGroup;
    public GPARAM.IField targetParamField;

    public RandomNumberGenerator RandomSource;

    public GparamQuickEdit(GparamEditorScreen screen)
    {
        Screen = screen;
        RandomSource = RandomNumberGenerator.Create();
    }

    private bool displayFileFilterSection = true;
    private bool displayGroupFilterSection = true;
    private bool displayFieldFilterSection = true;
    private bool displayValueFilterSection = true;
    private bool displayValueCommandSection = true;

    public void DisplayCheatSheet()
    {
        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "File Filters:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##fileFilterToggle"))
        {
            displayFileFilterSection = !displayFileFilterSection;
        }
        ImGui.Separator();
        if (displayFileFilterSection)
        {
            UIHelper.WrappedText($"File arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all files.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current file selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"file:[<name>]");
            UIHelper.WrappedText("Targets the file with the specified name.");
            UIHelper.WrappedText("");
        }

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Group Filters:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##groupFilterToggle"))
        {
            displayGroupFilterSection = !displayGroupFilterSection;
        }
        ImGui.Separator();
        if (displayGroupFilterSection)
        {
            UIHelper.WrappedText($"Group arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all groups.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current group selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"group:[<name>]");
            UIHelper.WrappedText("Targets the groups with the specified name.");
            UIHelper.WrappedText("");
        }

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Field Filters:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##fieldFilterToggle"))
        {
            displayFieldFilterSection = !displayFieldFilterSection;
        }
        ImGui.Separator();
        if (displayFieldFilterSection)
        {
            UIHelper.WrappedText($"Field arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all fields.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current field selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"field:[<name>]");
            UIHelper.WrappedText("Targets the fields with the specified name.");
            UIHelper.WrappedText("");
        }

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Value Filters:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##valueFilterToggle"))
        {
            displayValueFilterSection = !displayValueFilterSection;
        }
        ImGui.Separator();
        if (displayValueFilterSection)
        {
            UIHelper.WrappedText($"Filter arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current value row selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_ID}:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> ID.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Index}:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> row index.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Time of Day.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Value}:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Value. For multi-values split them like so: [<x>,<x>]");
            UIHelper.WrappedText("");
        }

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Value Commands:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##valueCommandToggle"))
        {
            displayValueCommandSection = !displayValueCommandSection;
        }
        ImGui.Separator();
        if (displayValueCommandSection)
        {
            UIHelper.WrappedText($"Command arguments can be chained by using the '{CFG.Current.Gparam_QuickEdit_Chain}' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Set}:[<x>]");
            UIHelper.WrappedText("Sets target rows to <x> Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Add}:[<x>]");
            UIHelper.WrappedText("Adds <x> to the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Subtract}:[<x>]");
            UIHelper.WrappedText("Subtracts <x> from the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Multiply}:[<x>]");
            UIHelper.WrappedText("Multiplies the Value of the target rows by <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_SetByRow}:[<x>]");
            UIHelper.WrappedText("Sets target rows to the Value of row ID <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Restore}");
            UIHelper.WrappedText("Sets target rows to their vanilla Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{CFG.Current.Gparam_QuickEdit_Random}:[<x>][<y>]");
            UIHelper.WrappedText("Sets target rows to a random value between <x> and <y>. First is the minimum, second is the maximum.");
            UIHelper.WrappedText("");
        }
    }

    public void DisplayInputWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);

        ImGui.Text("File Filter:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##targetParamString", ref _targetFileString, 255);
        UIHelper.Tooltip("Enter target file arguments here.");

        ImGui.Text("Group Filter:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##targetGroupString", ref _targetGroupString, 255);
        UIHelper.Tooltip("Enter target group arguments here.");

        ImGui.Text("Field Filter:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##targetFieldString", ref _targetFieldString, 255);
        UIHelper.Tooltip("Enter target field arguments here.");

        ImGui.Text("Value Filter:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##filterString", ref _valueFilterString, 255);
        UIHelper.Tooltip("Enter value filter arguments here.");

        ImGui.Text("Value Command:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##commandString", ref _valueCommandString, 255);
        UIHelper.Tooltip("Enter value command arguments here.");

        if (ImGui.Button("Execute", thirdButtonSize))
        {
            ExecuteQuickEdit();
        }

        ImGui.SameLine();
        if (ImGui.Button("Fill from Selection", thirdButtonSize))
        {
            GenerateQuickEditCommands();
        }
        UIHelper.Tooltip("Automatically fill the filter input based on current selection.");

        ImGui.SameLine();
        if (ImGui.Button("Clear", thirdButtonSize))
        {
            ClearQuickEditCommands();
        }
    }

    public void ClearQuickEditCommands()
    {
        _targetFileString = "";
        _targetGroupString = "";
        _targetFieldString = "";
        _valueFilterString = "";
        _valueCommandString = "";
    }

    public void GenerateQuickEditCommands()
    {
        _targetFileString = "";
        _targetGroupString = "";
        _targetFieldString = "";
        _valueFilterString = "";

        if (Screen.Selection._selectedGparamKey != null)
        {
            UpdateFileFilter(Screen.Selection._selectedGparamKey);
        }
        else
        {
            _valueFilterString = "*";
        }

        if (Screen.Selection._selectedParamGroup != null)
        {
            UpdateGroupFilter(Screen.Selection._selectedParamGroup.Key);
        }
        else
        {
            _valueFilterString = "*";
        }

        if (Screen.Selection._selectedParamField != null)
        {
            UpdateFieldFilter(Screen.Selection._selectedParamField.Key);
        }
        else
        {
            _valueFilterString = "*";
        }

        if (Screen.Selection._selectedParamField != null)
        {
            var fieldIndex = -1;
            for (int i = 0; i < Screen.Selection._selectedParamField.Values.Count; i++)
            {
                if (Screen.Selection._selectedParamField.Values[i] == Screen.Selection._selectedFieldValue)
                {
                    fieldIndex = i;
                    break;
                }
            }

            if (fieldIndex != -1)
            {
                UpdateValueRowFilter(fieldIndex);
            }
        }
        else
        {
            _valueFilterString = "*";
        }
    }

    public void ExecuteQuickEdit()
    {
        List<string> resolvedList = new();
        string curParamName = "";
        string curGroupName = "";
        string curFieldName = "";

        List<EditorAction> actionList = new();

        foreach (var entry in Screen.Project.GparamData.PrimaryBank.Entries)
        {
            if (IsTargetFile(entry.Key))
            {
                TargetFile = entry.Key;
                targetGparam = entry.Value;
                GPARAM data = entry.Value;
                curParamName = entry.Key.Filename;

                for (int i = 0; i < data.Params.Count; i++)
                {
                    GPARAM.Param curEntry = data.Params[i];

                    if (IsTargetGroup(curEntry))
                    {
                        targetParamGroup = curEntry;
                        GPARAM.Param pData = targetParamGroup;
                        curGroupName = pData.Name;

                        for (int k = 0; k < pData.Fields.Count; k++)
                        {
                            GPARAM.IField fEntry = pData.Fields[k];

                            if (IsTargetField(fEntry))
                            {
                                curFieldName = fEntry.Name;
                                targetParamField = fEntry;
                                resolvedList.Add($"{curParamName}:{curGroupName}:{curFieldName}");

                                var actions = ResolveQuickEdit(curParamName, curGroupName, targetParamField);
                                actionList.Add(actions);
                            }
                        }
                    }
                }
            }
        }

        if (resolvedList.Count > 0)
        {
            foreach (var entry in resolvedList)
            {
                //TaskLogs.AddLog($"Applied Quick Edit to: {entry}");
            }

            if (actionList.Count > 0)
            {

                var compoundAction = new CompoundAction(actionList);
                Screen.EditorActionManager.ExecuteAction(compoundAction);
            }
        }
        else
        {
            TaskLogs.AddLog($"Quick Edit could not be applied.", LogLevel.Warning);
        }
    }

    public bool IsTargetFile(FileDictionaryEntry entry)
    {
        var match = false;

        var commands = _targetFileString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
        foreach (var command in commands)
        {
            if (command == "*")
            {
                match = true;
                continue;
            }

            if (command == "selection")
            {
                if (Screen.Selection._selectedGparamKey == entry.Filename)
                {
                    match = true;
                    continue;
                }
            }

            Match filterMatch = null;
            filterMatch = Regex.Match(command, $@"{CFG.Current.Gparam_QuickEdit_File}:\[(.*)\]");

            if (filterMatch.Success && filterMatch.Groups.Count >= 2)
            {
                string commandValue = filterMatch.Groups[1].Value;

                if (commandValue == entry.Filename || commandValue == "*")
                {
                    match = true;
                }
            }
        }

        return match;
    }
    public bool IsTargetGroup(GPARAM.Param entry)
    {
        var match = false;

        var commands = _targetGroupString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
        foreach (var command in commands)
        {
            if (command == "*")
            {
                match = true;
                continue;
            }

            if (command == "selection")
            {
                if (Screen.Selection._selectedParamGroup.Key == entry.Key || Screen.Selection._selectedParamGroup.Name == entry.Name)
                {
                    match = true;
                    continue;
                }
            }

            Match filterMatch = null;
            filterMatch = Regex.Match(command, $@"{CFG.Current.Gparam_QuickEdit_Group}:\[(.*)\]");

            if (filterMatch.Success && filterMatch.Groups.Count >= 2)
            {
                string commandValue = filterMatch.Groups[1].Value;

                if (commandValue == entry.Name || commandValue == entry.Key || commandValue == "*")
                {
                    match = true;
                }
            }
        }

        return match;
    }
    public bool IsTargetField(GPARAM.IField entry)
    {
        var match = false;

        var commands = _targetFieldString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
        foreach (var command in commands)
        {
            if (command == "*")
            {
                match = true;
                continue;
            }

            if (command == "selection")
            {
                if (Screen.Selection._selectedParamField.Key == entry.Key)
                {
                    match = true;
                    continue;
                }
            }

            Match filterMatch = null;
            filterMatch = Regex.Match(command, $@"{CFG.Current.Gparam_QuickEdit_Field}:\[(.*)\]");

            if (filterMatch.Success && filterMatch.Groups.Count >= 2)
            {
                string commandValue = filterMatch.Groups[1].Value;

                if (commandValue == entry.Name || commandValue == entry.Key || commandValue == "*")
                {
                    match = true;
                }
            }
        }

        return match;
    }

    public void UpdateFileFilter(string name)
    {
        if (_targetFileString != "")
        {
            _targetFileString = $"{_targetFileString}+{CFG.Current.Gparam_QuickEdit_File}:[{name}]";
        }
        else
        {
            _targetFileString = $"{CFG.Current.Gparam_QuickEdit_File}:[{name}]";
        }
    }
    public void UpdateGroupFilter(string key)
    {
        var input = key;

        if (_targetGroupString != "")
        {
            _targetGroupString = $"{_targetGroupString}+{CFG.Current.Gparam_QuickEdit_Group}:[{input}]";
        }
        else
        {
            _targetGroupString = $"{CFG.Current.Gparam_QuickEdit_Group}:[{input}]";
        }
    }
    public void UpdateFieldFilter(string key)
    {
        var input = key;

        if (_targetFieldString != "")
        {
            _targetFieldString = $"{_targetFieldString}+{CFG.Current.Gparam_QuickEdit_Field}:[{input}]";
        }
        else
        {
            _targetFieldString = $"{CFG.Current.Gparam_QuickEdit_Field}:[{input}]";
        }
    }
    public void UpdateValueRowFilter(int index)
    {
        if (_valueFilterString != "")
        {
            _valueFilterString = $"{_valueFilterString}+{CFG.Current.Gparam_QuickEdit_Index}:[{index}]";
        }
        else
        {
            _valueFilterString = $"{CFG.Current.Gparam_QuickEdit_Index}:[{index}]";
        }
    }

    private GparamBatchChangeAction ResolveQuickEdit(string gparamName, string groupName, GPARAM.IField targetField)
    {
        filterTruth = new bool[targetField.Values.Count];
        actions = new List<GparamValueChangeAction>();

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            filterTruth[i] = false;
        }

        // Filter arguments
        var filters = _valueFilterString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
        foreach (var filter in filters)
        {
            FilterAll(targetField, filter);
            FilterSelection(targetField, filter);
            FilterId(targetField, filter);
            FilterIndex(targetField, filter);
            FilterTimeOfDay(targetField, filter);
            FilterValue(targetField, filter);
        }

        // Command arguments
        var commands = _valueCommandString.Split($"{CFG.Current.Gparam_QuickEdit_Chain}");
        foreach (var command in commands)
        {
            CommandAdjust(targetField, command, EditEffectType.Set, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.Add, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.Subtract, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.Multiply, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.SetByRow, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.Restore, gparamName, groupName);
            CommandAdjust(targetField, command, EditEffectType.Random, gparamName, groupName);
        }

        // Combine all the individual changes into a single action
        // so Undo/Redo treats the Quick Edit changes as one discrete change
        return new GparamBatchChangeAction(actions);
    }

    private (bool, object) GetVanillaValue(int index)
    {
        object vanillaValue = -1;
        bool foundValue = false;

        // Find vanilla value
        foreach (var entry in Screen.Project.GparamData.VanillaBank.Entries)
        {
            if (entry.Key.Filename == Screen.Selection._selectedGparamKey)
            {
                foreach (var paramGroup in entry.Value.Params)
                {
                    if (paramGroup.Key == Screen.Selection._selectedParamGroup.Key)
                    {
                        foreach (var paramField in paramGroup.Fields)
                        {
                            if (paramField.Key == Screen.Selection._selectedParamField.Key)
                            {
                                if (paramField.Values.Count > index)
                                {
                                    vanillaValue = paramField.Values[index].Value;
                                    foundValue = true;
                                    break;
                                }
                            }
                        }
                        if (foundValue)
                            break;
                    }
                }
            }
            if (foundValue)
                break;
        }

        return (foundValue, vanillaValue);
    }

    private void CommandAdjust(GPARAM.IField targetField, string commandArg, EditEffectType effectType, string gparamName, string groupName)
    {
        //TaskLogs.AddLog(commandArg);

        Match valueCommandMatch = null;

        if (effectType == EditEffectType.Set)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Set}:\[(.*)\]");
        }
        if (effectType == EditEffectType.Add)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Add}:\[(.*)\]");
        }
        if (effectType == EditEffectType.Subtract)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Subtract}:\[(.*)\]");
        }
        if (effectType == EditEffectType.Multiply)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Multiply}:\[(.*)\]");
        }
        if (effectType == EditEffectType.SetByRow)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_SetByRow}:\[(.*)\]");
        }
        if (effectType == EditEffectType.Restore)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Restore}");
        }
        if (effectType == EditEffectType.Random)
        {
            valueCommandMatch = Regex.Match(commandArg, $@"{CFG.Current.Gparam_QuickEdit_Random}:\[(.*)\]\[(.*)\]");
        }

        if (valueCommandMatch == null)
        {
            return;
        }

        // Ignore commands that are not set for bools
        if (targetField is GPARAM.BoolField)
        {
            if (effectType == EditEffectType.Add ||
                effectType == EditEffectType.Subtract ||
                effectType == EditEffectType.Multiply ||
                effectType == EditEffectType.SetByRow)
            {
                return;
            }
        }

        var proceed = false;

        if (effectType is EditEffectType.Set or EditEffectType.Add or EditEffectType.Subtract or EditEffectType.Multiply or EditEffectType.SetByRow)
        {
            if (valueCommandMatch.Success && valueCommandMatch.Groups.Count >= 2)
            {
                proceed = true;
            }
        }
        // Separate since Restore doesn't have pass a parameter
        else if (effectType is EditEffectType.Restore)
        {
            if (valueCommandMatch.Success)
            {
                proceed = true;
            }
        }
        else if (effectType is EditEffectType.Random)
        {
            if (valueCommandMatch.Success && valueCommandMatch.Groups.Count >= 3)
            {
                proceed = true;
            }
        }

        if (proceed)
        {
            string valueCommandString = valueCommandMatch.Groups[1].Value;
            string valueCommandString_secondary = "";

            // Read secondary group for "random" command
            if (effectType is EditEffectType.Random)
            {
                valueCommandString_secondary = valueCommandMatch.Groups[2].Value;
            }

            // Set this to a dummy value since "restore" doesn't pass a value
            if (effectType == EditEffectType.Restore)
            {
                valueCommandString = "0";
            }

            int rowsetId = -1;
            // Read value as int for set by row
            if (effectType == EditEffectType.SetByRow)
            {
                int.TryParse(valueCommandString, out rowsetId);
            }

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
                        var valid = int.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (intField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = intField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    int newValue = StudioCore.Utils.GenerateRandomInt(RandomSource, commandValue, commandValue_secondary);
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, newValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                    // UINT
                    else if (targetField is GPARAM.UintField uintField)
                    {
                        uint commandValue = 0;
                        var valid = uint.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (uintField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = uintField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    int newValue = StudioCore.Utils.GenerateRandomInt(RandomSource, (int)commandValue, commandValue_secondary);

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, (uint)newValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                    // SHORT
                    else if (targetField is GPARAM.ShortField shortField)
                    {
                        short commandValue = 0;
                        var valid = short.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (shortField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = shortField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    int newValue = StudioCore.Utils.GenerateRandomInt(RandomSource, commandValue, commandValue_secondary);

                                    if (newValue > short.MaxValue)
                                        newValue = short.MaxValue;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, (short)newValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                    // SBYTE
                    else if (targetField is GPARAM.SbyteField sbyteField)
                    {
                        sbyte commandValue = 0;
                        var valid = sbyte.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (sbyteField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = sbyteField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    int newValue = StudioCore.Utils.GenerateRandomInt(RandomSource, commandValue, commandValue_secondary);

                                    if (newValue > sbyte.MaxValue)
                                        newValue = sbyte.MaxValue;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, (sbyte)newValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                    // BYTE
                    else if (targetField is GPARAM.ByteField byteField)
                    {
                        byte commandValue = 0;
                        var valid = byte.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (byteField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = byteField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    int newValue = StudioCore.Utils.GenerateRandomInt(RandomSource, commandValue, commandValue_secondary);

                                    if (newValue > byte.MaxValue)
                                        newValue = byte.MaxValue;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, (byte)newValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                    // BOOL
                    else if (targetField is GPARAM.BoolField boolField)
                    {
                        int commandValue = 0;
                        var valid = int.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            else if (effectType == EditEffectType.Random)
                            {
                                int commandValue_secondary = -1;
                                var valid_secondary = int.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    double newValue = StudioCore.Utils.GenerateRandomDouble(RandomSource, 0, 1);

                                    bool newBool = false;

                                    if (newValue > 0.5)
                                        newBool = true;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, newBool, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            else
                            {
                                // Always set bools
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                    }
                    // FLOAT
                    else if (targetField is GPARAM.FloatField floatField)
                    {
                        float commandValue = 0.0f;
                        var valid = float.TryParse(valueCommandString, out commandValue);

                        if (valid)
                        {
                            if (effectType == EditEffectType.Set)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Add)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Subtract)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.Multiply)
                            {
                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                actions.Add(action);
                            }
                            if (effectType == EditEffectType.SetByRow)
                            {
                                if (floatField.Values.Any(x => x.Id == rowsetId))
                                {
                                    commandValue = floatField.Values.Find(x => x.Id == rowsetId).Value;

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Restore)
                            {
                                bool foundValue = false;
                                object vanillaValue = null;
                                (foundValue, vanillaValue) = GetVanillaValue(i);

                                if (foundValue)
                                {
                                    var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                            }
                            if (effectType == EditEffectType.Random)
                            {
                                float commandValue_secondary = -1;
                                var valid_secondary = float.TryParse(valueCommandString_secondary, out commandValue_secondary);

                                if (valid_secondary)
                                {
                                    double newValue = StudioCore.Utils.GenerateRandomDouble(RandomSource, (double)commandValue, (double)commandValue_secondary);

                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, (float)newValue, i, ValueChangeType.Set);
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

                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                        if (effectType == EditEffectType.Restore)
                        {
                            bool foundValue = false;
                            object vanillaValue = null;
                            (foundValue, vanillaValue) = GetVanillaValue(i);

                            if (foundValue)
                            {
                                var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }

                        if (!valueCommandString.Contains(","))
                        {
                            continue;
                        }

                        var parts = valueCommandString.Split(",");

                        if (parts.Length > 1)
                        {
                            float commandValue1 = 0.0f;
                            var valid1 = float.TryParse(parts[0], out commandValue1);

                            float commandValue2 = 0.0f;
                            var valid2 = float.TryParse(parts[1], out commandValue2);

                            if (valid1 && valid2)
                            {
                                commandValue = new Vector2(commandValue1, commandValue2);

                                if (effectType == EditEffectType.Set)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Random)
                                {
                                    var partsSecondary = valueCommandString_secondary.Split(",");

                                    if (partsSecondary.Length > 1)
                                    {
                                        float commandValue_secondary1 = 0.0f;
                                        var valid_secondary1 = float.TryParse(partsSecondary[0], out commandValue_secondary1);

                                        float commandValue_secondary2 = 0.0f;
                                        var valid_secondary2 = float.TryParse(partsSecondary[1], out commandValue_secondary2);

                                        if (valid_secondary1 && valid_secondary2)
                                        {
                                            double newValue1 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue1, (double)commandValue_secondary1);
                                            double newValue2 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue2, (double)commandValue_secondary2);

                                            Vector2 newValue = new Vector2((float)newValue1, (float)newValue2);

                                            GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, newValue, i, ValueChangeType.Set);
                                            actions.Add(action);
                                        }
                                    }
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

                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                        if (effectType == EditEffectType.Restore)
                        {
                            bool foundValue = false;
                            object vanillaValue = null;
                            (foundValue, vanillaValue) = GetVanillaValue(i);

                            if (foundValue)
                            {
                                var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }

                        if (!valueCommandString.Contains(","))
                        {
                            continue;
                        }

                        var parts = valueCommandString.Split(",");

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
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Random)
                                {
                                    var partsSecondary = valueCommandString_secondary.Split(",");

                                    if (partsSecondary.Length > 2)
                                    {
                                        float commandValue_secondary1 = 0.0f;
                                        var valid_secondary1 = float.TryParse(partsSecondary[0], out commandValue_secondary1);

                                        float commandValue_secondary2 = 0.0f;
                                        var valid_secondary2 = float.TryParse(partsSecondary[1], out commandValue_secondary2);

                                        float commandValue_secondary3 = 0.0f;
                                        var valid_secondary3 = float.TryParse(partsSecondary[2], out commandValue_secondary3);

                                        if (valid_secondary1 && valid_secondary2 && valid_secondary3)
                                        {
                                            double newValue1 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue1, (double)commandValue_secondary1);
                                            double newValue2 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue2, (double)commandValue_secondary2);
                                            double newValue3 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue3, (double)commandValue_secondary3);

                                            Vector3 newValue = new Vector3((float)newValue1, (float)newValue2, (float)newValue3);

                                            GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, newValue, i, ValueChangeType.Set);
                                            actions.Add(action);
                                        }
                                    }
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

                                GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }
                        if (effectType == EditEffectType.Restore)
                        {
                            bool foundValue = false;
                            object vanillaValue = null;
                            (foundValue, vanillaValue) = GetVanillaValue(i);

                            if (foundValue)
                            {
                                var action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                                actions.Add(action);
                            }
                        }

                        if (!valueCommandString.Contains(","))
                        {
                            continue;
                        }

                        var parts = valueCommandString.Split(",");

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
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Set);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Add)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Addition);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Subtract)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Subtraction);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Multiply)
                                {
                                    GparamValueChangeAction action = new GparamValueChangeAction(gparamName, groupName, targetField, entry, commandValue, i, ValueChangeType.Multiplication);
                                    actions.Add(action);
                                }
                                if (effectType == EditEffectType.Random)
                                {
                                    var partsSecondary = valueCommandString_secondary.Split(",");

                                    if (partsSecondary.Length > 3)
                                    {
                                        float commandValue_secondary1 = 0.0f;
                                        var valid_secondary1 = float.TryParse(partsSecondary[0], out commandValue_secondary1);

                                        float commandValue_secondary2 = 0.0f;
                                        var valid_secondary2 = float.TryParse(partsSecondary[1], out commandValue_secondary2);

                                        float commandValue_secondary3 = 0.0f;
                                        var valid_secondary3 = float.TryParse(partsSecondary[2], out commandValue_secondary3);

                                        float commandValue_secondary4 = 0.0f;
                                        var valid_secondary4 = float.TryParse(partsSecondary[3], out commandValue_secondary4);

                                        if (valid_secondary1 && valid_secondary2 && valid_secondary3 && valid_secondary4)
                                        {
                                            double newValue1 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue1, (double)commandValue_secondary1);
                                            double newValue2 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue2, (double)commandValue_secondary2);
                                            double newValue3 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue3, (double)commandValue_secondary3);
                                            double newValue4 = StudioCore.Utils.GenerateRandomDouble(
                                                RandomSource, (double)commandValue4, (double)commandValue_secondary4);

                                            Vector4 newValue = new Vector4((float)newValue1, (float)newValue2, (float)newValue3, (float)newValue4);

                                            GparamValueChangeAction action = new GparamValueChangeAction(
                                                gparamName, groupName, targetField, entry, newValue, i, ValueChangeType.Set);
                                            actions.Add(action);
                                        }
                                    }
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
    private void FilterSelection(GPARAM.IField targetField, string filterArg)
    {
        if (filterArg == "selection")
        {
            for (int i = 0; i < targetField.Values.Count; i++)
            {
                if (Screen.Selection._selectedFieldValueKey == i)
                {
                    filterTruth[i] = true;
                }
            }
        }
    }

    private void FilterId(GPARAM.IField targetField, string filterArg)
    {
        var idMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_ID}:\[([0-9]+)\]");

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

    private void FilterIndex(GPARAM.IField targetField, string filterArg)
    {
        var idxMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_Index}:\[([0-9]+)\]");

        if (idxMatch.Success && idxMatch.Groups.Count >= 2)
        {
            int targetIdx = -1;
            int.TryParse(idxMatch.Groups[1].Value, out targetIdx);

            for (int i = 0; i < targetField.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = targetField.Values[i];

                if (i == targetIdx)
                {
                    filterTruth[i] = true;
                    //TaskLogs.AddLog($"Filter: Matched ID {targetId} - {entry.Id}");
                }
            }
        }
    }

    private void FilterTimeOfDay(GPARAM.IField targetField, string filterArg)
    {
        var todMatch = Regex.Match(filterArg, $@"{CFG.Current.Gparam_QuickEdit_TimeOfDay}:\[([0-9]+)\]");

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
