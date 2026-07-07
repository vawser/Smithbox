using Hexa.NET.ImGui;
using SoulsFormats;
using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.GparamEditor;

public class GparamDataFinder
{
    public GparamEditorView View;
    public ProjectEntry Project;

    private string _targetFileString = "";
    private string _targetGroupString = "";
    private string _targetFieldString = "";
    private string _valueFilterString = "";

    private bool[] filterTruth = null;

    private List<GparamSearchResult> _results = new();
    private bool _uniqueValuesOnly = false;

    public GparamDataFinder(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("DataFinderSection", ImGuiChildFlags.Borders);

        GUI.SimpleHeader("File Filter", "");

        GUI.SinglelineTextInput("targetParamString", ref _targetFileString);
        GUI.Tooltip("Enter target file arguments here.");

        GUI.SimpleHeader("Group Filter", "");

        GUI.SinglelineTextInput("targetGroupString", ref _targetGroupString);
        GUI.Tooltip("Enter target group arguments here.");

        GUI.SimpleHeader("Field Filter", "");

        GUI.SinglelineTextInput("targetFieldString", ref _targetFieldString);
        GUI.Tooltip("Enter target field arguments here.");

        GUI.SimpleHeader("Value Filter", "");

        GUI.SinglelineTextInput("filterString", ref _valueFilterString);
        GUI.Tooltip("Enter value filter arguments here.");

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        ImGui.Checkbox("Unique Values Only", ref _uniqueValuesOnly);
        GUI.Tooltip("Only show the first result for each distinct value.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("quickEditActions",
            "fillFromSelection", "Fill from Selection", "", FillInputs,
            "clearInputs", "Clear", "", ClearInputs,
            "generateResults", "Generate", "", CollateResults);

        GUI.Spacer();
        GUI.SimpleHeader("Results", "");

        if (_results.Count == 0)
        {
            ImGui.TextDisabled("No results.");

            ImGui.EndChild();
            return;
        }

        IEnumerable<GparamSearchResult> displayResults = _results;

        if (_uniqueValuesOnly)
        {
            displayResults = _results
                .GroupBy(r => r.Value.Value?.ToString() ?? "")
                .Select(g => g.First());
        }

        var displayList = displayResults.ToList();
        
        ImGui.Text($"Number of matches: {displayList.Count}{(_uniqueValuesOnly ? $" (of {_results.Count})" : "")}");

        ImGui.BeginChild("ResultsSection", ImGuiChildFlags.Borders);
        for (int i = 0; i < displayList.Count; i++)
        {
            var result = displayList[i];

            string label = $"{result.FileEntry.Filename} > {result.Group.Name} > {result.Field.Name} [{result.Value.ID}]##{i}";

            if (ImGui.Selectable(label))
            {
                SelectResult(View, result);
            }
            GUI.Tooltip($"File: {result.FileEntry.Filename}\nGroup: {result.Group.Name} ({result.Group.Key})\nField: {result.Field.Name} ({result.Field.Key})\nValue ID: {result.Value.ID}");
        }
        ImGui.EndChild();

        ImGui.EndChild();
    }

    private void SelectResult(GparamEditorView view, GparamSearchResult result)
    {
        view.Selection.SelectedFileEntry = result.FileEntry;

        var targetEntry = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename
         == result.FileEntry.Filename && e.Key.Extension == result.FileEntry.Extension);

        view.Selection._selectedGparamKey = targetEntry.Key.Filename;
        view.Selection._selectedGparam = targetEntry.Value;

        // Select group
        if (view.Selection.IsFileSelected())
        {
            GPARAM data = view.Selection.GetSelectedGparam();

            for (int i = 0; i < data.Params.Count; i++)
            {
                if (data.Params[i].Key == result.Group.Key)
                {
                    view.Selection._selectedParamGroupKey = data.Params[i].Key;
                    view.Selection._selectedParamGroupIndex = i;

                    break;
                }
            }
        }

        // Select field
        if (view.Selection.IsGparamGroupSelected())
        {
            GPARAM.Param group = view.Selection.GetSelectedGroup();

            for (int i = 0; i < group.Fields.Count; i++)
            {
                if (group.Fields[i].Key == result.Field.Key)
                {
                    view.Selection._selectedParamFieldKey = group.Fields[i].Key;
                    view.Selection._selectedParamFieldIndex = i;

                    break;
                }
            }
        }

        // Select field value row
        if (view.Selection.IsGparamFieldSelected())
        {
            GPARAM.IField field = view.Selection.GetSelectedField();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (field.Values[i].ID == result.Value.ID)
                {
                    view.Selection._selectedFieldValueKey = field.Values[i].ID;
                    view.Selection._selectedFieldValueIndex = i;
                    view.Selection.DuplicateValueID = field.Values[i].ID;

                    break;
                }
            }
        }
    }

    public void ClearInputs()
    {
        _targetFileString = "";
        _targetGroupString = "";
        _targetFieldString = "";
        _valueFilterString = "";
    }

    public void FillInputs()
    {
        _targetFileString = "";
        _targetGroupString = "";
        _targetFieldString = "";
        _valueFilterString = "";

        if (View.Selection._selectedGparamKey != null)
            UpdateFileFilter(View.Selection._selectedGparamKey);
        else
            _valueFilterString = "*";

        if (View.Selection._selectedParamGroupKey != null)
            UpdateGroupFilter(View.Selection._selectedParamGroupKey);
        else
            _valueFilterString = "*";

        if (View.Selection._selectedParamFieldKey != null)
            UpdateFieldFilter(View.Selection._selectedParamFieldKey);
        else
            _valueFilterString = "*";

        if (View.Selection._selectedParamFieldKey != null)
        {
            var selectedField = View.Selection.GetSelectedField();
            var selectedValue = View.Selection.GetSelectedValue();
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

    public void CollateResults()
    {
        _results.Clear();

        List<Task<bool>> loadTasks = new List<Task<bool>>();

        foreach (var entry in Project.Handler.GparamData.PrimaryBank.Entries)
        {
            if (!IsTargetFile(View, entry.Key))
                continue;

            if (entry.Value == null)
            {
                Task<bool> loadTask = Project.Handler.GparamData.PrimaryBank.LoadGraphicsParam(entry.Key);

                loadTasks.Add(loadTask);
            }
        }

        Task.WaitAll(loadTasks);

        foreach (var entry in Project.Handler.GparamData.PrimaryBank.Entries)
        {
            GPARAM data = entry.Value;

            if (data == null)
                continue;

            foreach (GPARAM.Param curEntry in data.Params)
            {
                if (!IsTargetGroup(View, curEntry))
                    continue;

                foreach (GPARAM.IField curField in curEntry.Fields)
                {
                    if (!IsTargetField(View, curField))
                        continue;

                    ResolveValues(View, entry.Key, entry.Value, curEntry, curField);
                }
            }
        }
    }

    public bool IsTargetFile(GparamEditorView view, FileDictionaryEntry entry)
    {
        foreach (var command in _targetFileString.Split("+"))
        {
            if (command == "*")
                return true;

            if (command == "selection" && view.Selection._selectedGparamKey == entry.Filename)
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

    public bool IsTargetGroup(GparamEditorView view, GPARAM.Param entry)
    {
        foreach (var command in _targetGroupString.Split("+"))
        {
            if (command == "*")
                return true;

            if (command == "selection")
            {
                var sel = view.Selection.GetSelectedGroup();
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

    public bool IsTargetField(GparamEditorView view, GPARAM.IField entry)
    {
        foreach (var command in _targetFieldString.Split("+"))
        {
            if (command == "*")
                return true;

            if (command == "selection" && view.Selection.GetSelectedField().Key == entry.Key)
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

    private void ResolveValues(GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM gparam, GPARAM.Param group, GPARAM.IField targetField)
    {
        filterTruth = new bool[targetField.Values.Count];

        foreach (var filter in _valueFilterString.Split("+"))
        {
            FilterAll(targetField, filter);
            FilterSelection(view, targetField, filter);
            FilterId(targetField, filter);
            FilterIndex(targetField, filter);
            FilterTimeOfDay(targetField, filter);
            FilterValue(targetField, filter);
        }

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            if (!filterTruth[i])
                continue;

            _results.Add(new GparamSearchResult
            {
                FileEntry = fileEntry,
                Gparam = gparam,
                Group = group,
                Field = targetField,
                Value = targetField.Values[i],
                ValueIndex = i
            });
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

    private void FilterSelection(GparamEditorView view, GPARAM.IField targetField, string filterArg)
    {
        if (filterArg == "selection")
        {
            for (int i = 0; i < targetField.Values.Count; i++)
            {
                if (view.Selection._selectedFieldValueIndex == i)
                    filterTruth[i] = true;
            }
        }
    }

    private void FilterId(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"id:\[([0-9]+)\]");
        if (!m.Success || m.Groups.Count < 2) 
            return;

        if (!int.TryParse(m.Groups[1].Value, out int targetId)) 
            return;

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            if (targetField.Values[i].ID == targetId)
                filterTruth[i] = true;
        }
    }

    private void FilterIndex(GPARAM.IField targetField, string filterArg)
    {
        var m = Regex.Match(filterArg, @"index:\[([0-9]+)\]");
        if (!m.Success || m.Groups.Count < 2) 
            return;

        if (!int.TryParse(m.Groups[1].Value, out int targetIdx)) 
            return;

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
        if (!m.Success || m.Groups.Count < 2) 
            return;

        string targetValue = m.Groups[1].Value;

        for (int i = 0; i < targetField.Values.Count; i++)
        {
            bool matched = targetField switch
            {
                GPARAM.LongField f => long.TryParse(targetValue, out long iv) && f.Values[i].Value == iv,
                GPARAM.UlongField f => ulong.TryParse(targetValue, out ulong uv) && f.Values[i].Value == uv,
                GPARAM.IntField f => int.TryParse(targetValue, out int iv) && f.Values[i].Value == iv,
                GPARAM.UintField f => uint.TryParse(targetValue, out uint uv) && f.Values[i].Value == uv,
                GPARAM.ShortField f => short.TryParse(targetValue, out short sv) && f.Values[i].Value == sv,
                GPARAM.UshortField f => ushort.TryParse(targetValue, out ushort sv) && f.Values[i].Value == sv,
                GPARAM.SbyteField f => sbyte.TryParse(targetValue, out sbyte bv) && f.Values[i].Value == bv,
                GPARAM.ByteField f => byte.TryParse(targetValue, out byte byv) && f.Values[i].Value == byv,
                GPARAM.BoolField f => bool.TryParse(targetValue, out bool blv) && f.Values[i].Value == blv,
                GPARAM.FloatField f => float.TryParse(targetValue, out float fv) && f.Values[i].Value == fv,
                GPARAM.DoubleField f => double.TryParse(targetValue, out double fv) && f.Values[i].Value == fv,
                GPARAM.Vector2Field f => MatchVector2(targetValue, f.Values[i].Value),
                GPARAM.Vector3Field f => MatchVector3(targetValue, f.Values[i].Value),
                GPARAM.Vector4Field f => MatchVector4(targetValue, f.Values[i].Value),
                GPARAM.ColorField f => MatchColor(targetValue, f.Values[i].Value),
                GPARAM.StringField f => $"{targetValue}" == f.Values[i].Value,
                _ => false
            };

            if (matched)
                filterTruth[i] = true;
        }
    }
    private static bool MatchVector2(string s, Vector2 v)
    {
        var f = GparamConstructUtils.ParseFloats(s, 2);
        return f != null && new Vector2(f[0], f[1]) == v;
    }

    private static bool MatchVector3(string s, Vector3 v)
    {
        var f = GparamConstructUtils.ParseFloats(s, 3);
        return f != null && new Vector3(f[0], f[1], f[2]) == v;
    }

    private static bool MatchVector4(string s, Vector4 v)
    {
        var f = GparamConstructUtils.ParseFloats(s, 4);
        return f != null && new Vector4(f[0], f[1], f[2], f[3]) == v;
    }

    private static bool MatchColor(string s, Color v)
    {
        var f = GparamConstructUtils.ParseInts(s, 4);

        return f != null && Color.FromArgb(f[0], f[1], f[2], f[3]) == v;
    }
}
public class GparamSearchResult
{
    public FileDictionaryEntry FileEntry { get; set; }
    public GPARAM Gparam { get; set; }
    public GPARAM.Param Group { get; set; }
    public GPARAM.IField Field { get; set; }
    public GPARAM.IFieldValue Value { get; set; }
    public int ValueIndex { get; set; }
}
