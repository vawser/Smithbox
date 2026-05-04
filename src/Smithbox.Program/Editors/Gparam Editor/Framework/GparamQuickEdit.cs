using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamQuickEdit
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    private string _targetFileString = "";
    private string _targetGroupString = "";
    private string _targetFieldString = "";
    private string _valueFilterString = "";
    private string _valueCommandString = "";

    private bool[] filterTruth = null;

    public List<EditValueAction> actions = new List<EditValueAction>();

    public FileDictionaryEntry TargetFile;
    public GPARAM targetGparam;
    public GPARAM.Param targetParamGroup;
    public GPARAM.IField targetParamField;

    public RandomNumberGenerator RandomSource;

    public GparamQuickEdit(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
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
        if (ImGui.Button($"{Icons.Bars}##fileFilterToggle", DPI.StandardButtonSize))
            displayFileFilterSection = !displayFileFilterSection;
        ImGui.Separator();
        if (displayFileFilterSection)
        {
            UIHelper.WrappedText($"File arguments can be chained by using the '+' character.");
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
        if (ImGui.Button($"{Icons.Bars}##groupFilterToggle", DPI.StandardButtonSize))
            displayGroupFilterSection = !displayGroupFilterSection;
        ImGui.Separator();
        if (displayGroupFilterSection)
        {
            UIHelper.WrappedText($"Group arguments can be chained by using the '+' character.");
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
        if (ImGui.Button($"{Icons.Bars}##fieldFilterToggle", DPI.StandardButtonSize))
            displayFieldFilterSection = !displayFieldFilterSection;
        ImGui.Separator();
        if (displayFieldFilterSection)
        {
            UIHelper.WrappedText($"Field arguments can be chained by using the '+' character.");
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
        if (ImGui.Button($"{Icons.Bars}##valueFilterToggle", DPI.StandardButtonSize))
            displayValueFilterSection = !displayValueFilterSection;
        ImGui.Separator();
        if (displayValueFilterSection)
        {
            UIHelper.WrappedText($"Filter arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current value row selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"id:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> ID.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"index:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> row index.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"tod:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Time of Day.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"value:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Value. For multi-values split them like so: [<x>,<x>]");
            UIHelper.WrappedText("");
        }

        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Value Commands:");
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Bars}##valueCommandToggle", DPI.StandardButtonSize))
            displayValueCommandSection = !displayValueCommandSection;
        ImGui.Separator();
        if (displayValueCommandSection)
        {
            UIHelper.WrappedText($"Command arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"set:[<x>]");
            UIHelper.WrappedText("Sets target rows to <x> Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"add:[<x>]");
            UIHelper.WrappedText("Adds <x> to the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"sub:[<x>]");
            UIHelper.WrappedText("Subtracts <x> from the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"mult:[<x>]");
            UIHelper.WrappedText("Multiplies the Value of the target rows by <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"rowset:[<x>]");
            UIHelper.WrappedText("Sets target rows to the Value of row ID <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"restore");
            UIHelper.WrappedText("Sets target rows to their vanilla Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"random:[<x>][<y>]");
            UIHelper.WrappedText("Sets target rows to a random value between <x> and <y>.");
            UIHelper.WrappedText("");
        }
    }

    public void DisplayInputWindow()
    {
        ImGui.Text("File Filter:");
        DPI.ApplyInputWidth();
        ImGui.InputText("##targetParamString", ref _targetFileString, 255);
        UIHelper.Tooltip("Enter target file arguments here.");

        ImGui.Text("Group Filter:");
        DPI.ApplyInputWidth();
        ImGui.InputText("##targetGroupString", ref _targetGroupString, 255);
        UIHelper.Tooltip("Enter target group arguments here.");

        ImGui.Text("Field Filter:");
        DPI.ApplyInputWidth();
        ImGui.InputText("##targetFieldString", ref _targetFieldString, 255);
        UIHelper.Tooltip("Enter target field arguments here.");

        ImGui.Text("Value Filter:");
        DPI.ApplyInputWidth();
        ImGui.InputText("##filterString", ref _valueFilterString, 255);
        UIHelper.Tooltip("Enter value filter arguments here.");

        ImGui.Text("Value Command:");
        DPI.ApplyInputWidth();
        ImGui.InputText("##commandString", ref _valueCommandString, 255);
        UIHelper.Tooltip("Enter value command arguments here. For values represented by a vector, separate each digit with the brackets with the , symbol.");

        if (ImGui.Button("Fill from Selection", DPI.StandardButtonSize))
        {
            GenerateQuickEditCommands();
        }
        UIHelper.Tooltip("Automatically fill the filter input based on current selection.");

        if (ImGui.Button("Clear", DPI.StandardButtonSize))
        {
            ClearQuickEditCommands();
        }
        UIHelper.Tooltip("Clear quick edit form.");

        if (ImGui.Button("Execute", DPI.StandardButtonSize))
        {
            ExecuteQuickEdit();
        }
        UIHelper.Tooltip("Apply quick edit.");
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

        if (Parent.Selection._selectedGparamKey != null)
            UpdateFileFilter(Parent.Selection._selectedGparamKey);
        else
            _valueFilterString = "*";

        if (Parent.Selection._selectedParamGroupKey != null)
            UpdateGroupFilter(Parent.Selection._selectedParamGroupKey);
        else
            _valueFilterString = "*";

        if (Parent.Selection._selectedParamFieldKey != null)
            UpdateFieldFilter(Parent.Selection._selectedParamFieldKey);
        else
            _valueFilterString = "*";

        if (Parent.Selection._selectedParamFieldKey != null)
        {
            var selectedField = Parent.Selection.GetSelectedField();
            var selectedValue = Parent.Selection.GetSelectedValue();
            int fieldIndex = -1;

            for (int i = 0; i < selectedField.Values.Count; i++)
            {
                if (selectedField.Values[i] == selectedValue)
                {
                    fieldIndex = i;
                    break;
                }
            }

            if (fieldIndex != -1)
                UpdateValueRowFilter(fieldIndex);
        }
        else
        {
            _valueFilterString = "*";
        }
    }

    public void ExecuteQuickEdit()
    {
        List<string> resolvedList = new();
        List<EditorAction> actionList = new();

        foreach (var entry in Project.Handler.GparamData.PrimaryBank.Entries)
        {
            if (!IsTargetFile(entry.Key))
                continue;

            TargetFile = entry.Key;
            targetGparam = entry.Value;
            GPARAM data = entry.Value;
            string curParamName = entry.Key.Filename;

            foreach (GPARAM.Param curEntry in data.Params)
            {
                if (!IsTargetGroup(curEntry))
                    continue;

                targetParamGroup = curEntry;
                string curGroupName = targetParamGroup.Key;

                foreach (GPARAM.IField fEntry in targetParamGroup.Fields)
                {
                    if (!IsTargetField(fEntry))
                        continue;

                    targetParamField = fEntry;
                    resolvedList.Add($"{curParamName}:{curGroupName}:{fEntry.Name}");
                    actionList.Add(ResolveQuickEdit(curParamName, curGroupName, targetParamField));
                }
            }
        }

        if (resolvedList.Count > 0)
        {
            if (actionList.Count > 0)
            {
                var compoundAction = new CompoundAction(actionList);
                Parent.ActionManager.ExecuteAction(compoundAction);
            }
        }
        else
        {
            Smithbox.Log(this, $"Quick Edit could not be applied.", LogLevel.Warning);
        }
    }

    public bool IsTargetFile(FileDictionaryEntry entry)
    {
        foreach (var command in _targetFileString.Split("+"))
        {
            if (command == "*") 
                return true;

            if (command == "selection" && Parent.Selection._selectedGparamKey == entry.Filename)
                return true;

            var m = Regex.Match(command, @"file:\[(.*)\]");

            if (m.Success && m.Groups.Count >= 2)
            {
                string val = m.Groups[1].Value;
                if (val == entry.Filename || val == "*") return true;
            }
        }
        return false;
    }

    public bool IsTargetGroup(GPARAM.Param entry)
    {
        foreach (var command in _targetGroupString.Split("+"))
        {
            if (command == "*") 
                return true;

            if (command == "selection")
            {
                var sel = Parent.Selection.GetSelectedGroup();
                if (sel.Key == entry.Key || sel.Name == entry.Name) return true;
            }

            var m = Regex.Match(command, @"group:\[(.*)\]");

            if (m.Success && m.Groups.Count >= 2)
            {
                string val = m.Groups[1].Value;
                if (val == entry.Name || val == entry.Key || val == "*") return true;
            }
        }
        return false;
    }

    public bool IsTargetField(GPARAM.IField entry)
    {
        foreach (var command in _targetFieldString.Split("+"))
        {
            if (command == "*")
                return true;

            if (command == "selection" && Parent.Selection.GetSelectedField().Key == entry.Key)
                return true;

            var m = Regex.Match(command, @"field:\[(.*)\]");

            if (m.Success && m.Groups.Count >= 2)
            {
                string val = m.Groups[1].Value;
                if (val == entry.Name || val == entry.Key || val == "*") return true;
            }
        }
        return false;
    }

    public void UpdateFileFilter(string name)
    {
        _targetFileString = _targetFileString != ""
            ? $"{_targetFileString}+file:[{name}]"
            : $"file:[{name}]";
    }

    public void UpdateGroupFilter(string key)
    {
        _targetGroupString = _targetGroupString != ""
            ? $"{_targetGroupString}+group:[{key}]"
            : $"group:[{key}]";
    }

    public void UpdateFieldFilter(string key)
    {
        _targetFieldString = _targetFieldString != ""
            ? $"{_targetFieldString}+field:[{key}]"
            : $"field:[{key}]";
    }

    public void UpdateValueRowFilter(int index)
    {
        _valueFilterString = _valueFilterString != ""
            ? $"{_valueFilterString}+index:[{index}]"
            : $"index:[{index}]";
    }

    private BatchChangeAction ResolveQuickEdit(string gparamName, string groupName, GPARAM.IField targetField)
    {
        var match = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == gparamName);

        if (match.Value == null)
            return null;

        var gparam = match.Value;

        var group = gparam.Params.FirstOrDefault(e => e.Key == groupName);

        if(group == null) 
            return null;

        filterTruth = new bool[targetField.Values.Count];
        actions = new List<EditValueAction>();

        foreach (var filter in _valueFilterString.Split("+"))
        {
            FilterAll(targetField, filter);
            FilterSelection(targetField, filter);
            FilterId(targetField, filter);
            FilterIndex(targetField, filter);
            FilterTimeOfDay(targetField, filter);
            FilterValue(targetField, filter);
        }

        foreach (var command in _valueCommandString.Split("+"))
        {
            CommandAdjust(targetField, command, EditEffectType.Set, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.Add, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.Subtract, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.Multiply, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.SetByRow, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.Restore, gparam, group);
            CommandAdjust(targetField, command, EditEffectType.Random, gparam, group);
        }

        return new BatchChangeAction(actions);
    }

    private (bool, object) GetVanillaValue(int index)
    {
        var selectedGroup = Parent.Selection.GetSelectedGroup();
        var selectedField = Parent.Selection.GetSelectedField();

        foreach (var entry in Project.Handler.GparamData.VanillaBank.Entries)
        {
            if (entry.Key.Filename != Parent.Selection._selectedGparamKey)
                continue;

            foreach (var paramGroup in entry.Value.Params)
            {
                if (paramGroup.Key != selectedGroup.Key)
                    continue;

                foreach (var paramField in paramGroup.Fields)
                {
                    if (paramField.Key == selectedField.Key && paramField.Values.Count > index)
                        return (true, paramField.Values[index].Value);
                }
            }
        }

        return (false, -1);
    }

    // Parses a comma-separated string into floats, returns null if any fail or count doesn't match.
    private static float[] ParseFloats(string s, int count)
    {
        var parts = s.Split(",");

        if (parts.Length < count) 
            return null;

        var result = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (!float.TryParse(parts[i], out result[i])) 
                return null;
        }

        return result;
    }

    private void AddAction(GPARAM data, GPARAM.Param group, GPARAM.IField field,
        GPARAM.IFieldValue entry, object value, int index, ValueChangeType changeType)
    {
        var newAction = new EditValueAction(Project, data, group, field, new List<IFieldValue>() { entry }, value, changeType);
        actions.Add(newAction);
    }

    private void CommandAdjust(GPARAM.IField targetField, string commandArg, EditEffectType effectType,
        GPARAM data, Param group)
    {
        // Match the command pattern for this effect type
        Match match = effectType switch
        {
            EditEffectType.Set => Regex.Match(commandArg, @"set:\[(.*)\]"),
            EditEffectType.Add => Regex.Match(commandArg, @"add:\[(.*)\]"),
            EditEffectType.Subtract => Regex.Match(commandArg, @"sub:\[(.*)\]"),
            EditEffectType.Multiply => Regex.Match(commandArg, @"mult:\[(.*)\]"),
            EditEffectType.SetByRow => Regex.Match(commandArg, @"rowset:\[(.*)\]"),
            EditEffectType.Restore => Regex.Match(commandArg, @"restore"),
            EditEffectType.Random => Regex.Match(commandArg, @"random:\[(.*)\]\[(.*)\]"),
            _ => null
        };

        if (match == null) 
            return;

        bool proceed = effectType switch
        {
            EditEffectType.Restore => match.Success,
            EditEffectType.Random => match.Success && match.Groups.Count >= 3,
            _ => match.Success && match.Groups.Count >= 2
        };

        if (!proceed) 
            return;

        // Bool fields only support Set, Restore, and Random
        if (targetField is GPARAM.BoolField &&
            effectType is EditEffectType.Add or EditEffectType.Subtract
                       or EditEffectType.Multiply or EditEffectType.SetByRow)
        {
            return;
        }

        string primaryArg = effectType != EditEffectType.Restore ? match.Groups[1].Value : "0";
        string secondaryArg = effectType == EditEffectType.Random ? match.Groups[2].Value : "";

        // Map EditEffectType to ValueChangeType for the action
        ValueChangeType ChangeType(EditEffectType e) => e switch
        {
            EditEffectType.Add => ValueChangeType.Addition,
            EditEffectType.Subtract => ValueChangeType.Subtraction,
            EditEffectType.Multiply => ValueChangeType.Multiplication,
            _ => ValueChangeType.Set
        };

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            if (!filterTruth[i]) 
                continue;

            var entry = targetField.Values[i];

            // Restore is field-type agnostic — handle it once here
            if (effectType == EditEffectType.Restore)
            {
                var (found, vanillaValue) = GetVanillaValue(i);
                if (found)
                {
                    AddAction(data, group, targetField, entry, vanillaValue, i, ValueChangeType.Set);
                }

                continue;
            }

            switch (targetField)
            {
                case GPARAM.IntField intField:
                    {
                        if (!int.TryParse(primaryArg, out int v))
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = intField.Values.Find(x => x.ID == int.Parse(primaryArg));

                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    Utils.GenerateRandomInt(RandomSource, v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }
                case GPARAM.UintField uintField:
                    {
                        if (!uint.TryParse(primaryArg, out uint v)) 
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = uintField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    (uint)Utils.GenerateRandomInt(RandomSource, (int)v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }
                case GPARAM.ShortField shortField:
                    {
                        if (!short.TryParse(primaryArg, out short v)) 
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = shortField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                int rand = Utils.GenerateRandomInt(RandomSource, v, v2);
                                AddAction(data, group, targetField, entry,
                                    (short)System.Math.Min(rand, short.MaxValue), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }
                case GPARAM.UshortField ushortField:
                    {
                        if (!ushort.TryParse(primaryArg, out ushort v))
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = ushortField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                int rand = Utils.GenerateRandomInt(RandomSource, v, v2);
                                AddAction(data, group, targetField, entry,
                                    (ushort)System.Math.Min(rand, ushort.MaxValue), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }
                case GPARAM.SbyteField sbyteField:
                    {
                        if (!sbyte.TryParse(primaryArg, out sbyte v)) 
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = sbyteField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                int rand = Utils.GenerateRandomInt(RandomSource, v, v2);
                                AddAction(data, group, targetField, entry,
                                    (sbyte)System.Math.Min(rand, sbyte.MaxValue), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.ByteField byteField:
                    {
                        if (!byte.TryParse(primaryArg, out byte v)) 
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = byteField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out int v2))
                            {
                                int rand = Utils.GenerateRandomInt(RandomSource, v, v2);
                                AddAction(data, group, targetField, entry,
                                    (byte)System.Math.Min(rand, byte.MaxValue), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.BoolField boolField:
                    {
                        if (effectType == EditEffectType.Random)
                        {
                            if (int.TryParse(secondaryArg, out _))
                            {
                                AddAction(data, group, targetField, entry,
                                    Utils.GenerateRandomDouble(RandomSource, 0, 1) > 0.5, i, ValueChangeType.Set);
                            }
                        }
                        else if (int.TryParse(primaryArg, out int v))
                        {
                            AddAction(data, group, targetField, entry, v, i, ValueChangeType.Set);
                        }
                        break;
                    }

                case GPARAM.FloatField floatField:
                    {
                        if (!float.TryParse(primaryArg, out float v)) 
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = floatField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (float.TryParse(secondaryArg, out float v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    (float)Utils.GenerateRandomDouble(RandomSource, v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.DoubleField doubleField:
                    {
                        if (!double.TryParse(primaryArg, out double v))
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = doubleField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (double.TryParse(secondaryArg, out double v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    (double)Utils.GenerateRandomDouble(RandomSource, v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.Vector2Field vector2Field:
                    {
                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = vector2Field.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                            break;
                        }

                        var f = ParseFloats(primaryArg, 2);

                        if (f == null) 
                            break;

                        var v = new Vector2(f[0], f[1]);

                        if (effectType == EditEffectType.Random)
                        {
                            var f2 = ParseFloats(secondaryArg, 2);

                            if (f2 == null) 
                                break;

                            AddAction(data, group, targetField, entry, new Vector2(
                                (float)Utils.GenerateRandomDouble(RandomSource, f[0], f2[0]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[1], f2[1])), i, ValueChangeType.Set);
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.Vector3Field vector3Field:
                    {
                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = vector3Field.Values.Find(x => x.ID == int.Parse(primaryArg));

                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                            break;
                        }
                        var f = ParseFloats(primaryArg, 3);

                        if (f == null) 
                            break;

                        var v = new Vector3(f[0], f[1], f[2]);

                        if (effectType == EditEffectType.Random)
                        {
                            var f2 = ParseFloats(secondaryArg, 3);

                            if (f2 == null) 
                                break;

                            AddAction(data, group, targetField, entry, new Vector3(
                                (float)Utils.GenerateRandomDouble(RandomSource, f[0], f2[0]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[1], f2[1]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[2], f2[2])), i, ValueChangeType.Set);
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.Vector4Field vector4Field:
                    {
                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = vector4Field.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }

                            break;
                        }

                        var f = ParseFloats(primaryArg, 4);

                        if (f == null) 
                            break;

                        var v = new Vector4(f[0], f[1], f[2], f[3]);

                        if (effectType == EditEffectType.Random)
                        {
                            var f2 = ParseFloats(secondaryArg, 4);

                            if (f2 == null)
                                break;

                            AddAction(data, group, targetField, entry, new Vector4(
                                (float)Utils.GenerateRandomDouble(RandomSource, f[0], f2[0]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[1], f2[1]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[2], f2[2]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[3], f2[3])), i, ValueChangeType.Set);
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }

                        break;
                    }

                case GPARAM.ColorField colorField:
                    {
                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = colorField.Values.Find(x => x.ID == int.Parse(primaryArg));
                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }

                            break;
                        }

                        var f = ParseFloats(primaryArg, 4);

                        if (f == null)
                            break;

                        var v = new Vector4(f[0], f[1], f[2], f[3]);

                        if (effectType == EditEffectType.Random)
                        {
                            var f2 = ParseFloats(secondaryArg, 4);

                            if (f2 == null)
                                break;

                            AddAction(data, group, targetField, entry, new Vector4(
                                (float)Utils.GenerateRandomDouble(RandomSource, f[0], f2[0]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[1], f2[1]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[2], f2[2]),
                                (float)Utils.GenerateRandomDouble(RandomSource, f[3], f2[3])), i, ValueChangeType.Set);
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }

                        break;
                    }

                case GPARAM.LongField longField:
                    {
                        if (!long.TryParse(primaryArg, out long v))
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = longField.Values.Find(x => x.ID == int.Parse(primaryArg));

                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (long.TryParse(secondaryArg, out long v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    Utils.GenerateRandomLong(RandomSource, v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.UlongField ulongField:
                    {
                        if (!ulong.TryParse(primaryArg, out ulong v))
                            break;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = ulongField.Values.Find(x => x.ID == int.Parse(primaryArg));

                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else if (effectType == EditEffectType.Random)
                        {
                            if (ulong.TryParse(secondaryArg, out ulong v2))
                            {
                                AddAction(data, group, targetField, entry,
                                    Utils.GenerateRandomULong(RandomSource, v, v2), i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }

                case GPARAM.StringField strField:
                    {
                        string v = primaryArg;

                        if (effectType == EditEffectType.SetByRow)
                        {
                            var src = strField.Values.Find(x => x.ID == int.Parse(primaryArg));

                            if (src != null)
                            {
                                AddAction(data, group, targetField, entry, src.Value, i, ValueChangeType.Set);
                            }
                        }
                        else
                        {
                            AddAction(data, group, targetField, entry, v, i, ChangeType(effectType));
                        }
                        break;
                    }
            }
        }
    }

    private void FilterAll(GPARAM.IField targetField, string filterArg)
    {
        if (filterArg == "*")
        {
            for (int i = 0; i < targetField.Values.Count; i++)
                filterTruth[i] = true;
        }
    }

    private void FilterSelection(GPARAM.IField targetField, string filterArg)
    {
        if (filterArg == "selection")
        {
            for (int i = 0; i < targetField.Values.Count; i++)
            {
                if (Parent.Selection._selectedFieldValueIndex == i)
                    filterTruth[i] = true;
            }
        }
    }

    private void FilterId(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"id:\[([0-9]+)\]");
        if (!m.Success || m.Groups.Count < 2) return;

        if (!int.TryParse(m.Groups[1].Value, out int targetId)) return;

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            if (targetField.Values[i].ID == targetId)
                filterTruth[i] = true;
        }
    }

    private void FilterIndex(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"index:\[([0-9]+)\]");
        if (!m.Success || m.Groups.Count < 2) return;

        if (!int.TryParse(m.Groups[1].Value, out int targetIdx)) return;

        if (targetIdx < targetField.Values.Count)
            filterTruth[targetIdx] = true;
    }

    private void FilterTimeOfDay(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"tod:\[([0-9]+)\]");
        if (!m.Success || m.Groups.Count < 2) return;

        if (!float.TryParse(m.Groups[1].Value, out float targetTod)) return;

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            if (targetField.Values[i].TimeOfDay == targetTod)
                filterTruth[i] = true;
        }
    }

    private void FilterValue(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"value:\[(.*)\]");
        if (!m.Success || m.Groups.Count < 2) return;

        string targetValue = m.Groups[1].Value;

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            bool matched = targetField switch
            {
                GPARAM.IntField f => int.TryParse(targetValue, out int iv) && f.Values[i].Value == iv,
                GPARAM.UintField f => uint.TryParse(targetValue, out uint uv) && f.Values[i].Value == uv,
                GPARAM.ShortField f => short.TryParse(targetValue, out short sv) && f.Values[i].Value == sv,
                GPARAM.SbyteField f => sbyte.TryParse(targetValue, out sbyte bv) && f.Values[i].Value == bv,
                GPARAM.ByteField f => byte.TryParse(targetValue, out byte byv) && f.Values[i].Value == byv,
                GPARAM.BoolField f => bool.TryParse(targetValue, out bool blv) && f.Values[i].Value == blv,
                GPARAM.FloatField f => float.TryParse(targetValue, out float fv) && f.Values[i].Value == fv,
                GPARAM.Vector2Field f => MatchVector2(targetValue, f.Values[i].Value),
                GPARAM.Vector3Field f => MatchVector3(targetValue, f.Values[i].Value),
                GPARAM.Vector4Field f => MatchVector4(targetValue, f.Values[i].Value),
                _ => false
            };

            if (matched) filterTruth[i] = true;
        }
    }

    private static bool MatchVector2(string s, Vector2 v)
    {
        var f = ParseFloats(s, 2);
        return f != null && new Vector2(f[0], f[1]) == v;
    }

    private static bool MatchVector3(string s, Vector3 v)
    {
        var f = ParseFloats(s, 3);
        return f != null && new Vector3(f[0], f[1], f[2]) == v;
    }

    private static bool MatchVector4(string s, Vector4 v)
    {
        var f = ParseFloats(s, 4);
        return f != null && new Vector4(f[0], f[1], f[2], f[3]) == v;
    }
}