using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Globalization;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Search: records by a known field's value, by condition type, by state ID or kind, or by a byte pattern in
/// any raw blob (for the undecoded data), across the files in scope.
/// </summary>
public class BhvSearchTool
{
    private readonly BhvEditorView View;

    private enum SearchMode { Field, ConditionType, StateId, StateKind, Bytes }

    private readonly BhvToolScope Scope;
    private SearchMode Mode = SearchMode.Field;

    // Field mode: the known fields, by label key, gathered from the selected file
    private string FieldKey;
    private List<(string Key, BhvFieldType Type)> FieldChoices = new();
    private BhvFileWrapper FieldChoicesFile;

    private long IntValue;
    private float FloatValue;
    private string BytesText = "";

    private List<BhvToolResult> Results;
    private string Summary = "";

    public BhvSearchTool(BhvEditorView view)
    {
        View = view;
        Scope = new(view, "bhvSearchScope", BhvScope.Binder);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("BHV_SearchTool_Header")}##behaviorSearchToolHeader"))
            return;

        ImGui.BeginChild("BehaviorSearchToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("BHV_SearchTool_Hint"));

        // Search For
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_SearchTool_Mode_Header"), LOC.Get("BHV_SearchTool_Mode_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##behaviorSearchMode", LOC.Get($"BHV_SearchTool_Mode_{Mode}")))
        {
            foreach (var mode in Enum.GetValues<SearchMode>())
                if (ImGui.Selectable($"{LOC.Get($"BHV_SearchTool_Mode_{mode}")}##behaviorSearchMode_{mode}", mode == Mode))
                    Mode = mode;
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("BHV_SearchTool_Mode_TT"));

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_SearchTool_Value_Header"), LOC.Get("BHV_SearchTool_Value_Header_TT"));

        var ready = DrawValue();

        var scopeOk = Scope.Draw(allowModified: true);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_SearchTool_Actions_Header"), LOC.Get("BHV_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("behaviorSearchActions",
            "behaviorSearchFind", LOC.Get("BHV_SearchTool_Find"), LOC.Get("BHV_SearchTool_Find_TT"),
            Find, scopeOk && ready);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_SearchTool_Results_Header"), LOC.Get("BHV_SearchTool_Results_Header_TT"));

        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("BHV_Tool_Not_Run"));
            return;
        }

        GUI.WrappedText(Summary);
        BhvResultList.Draw(View, "BehaviorSearchResults", Results);
    }

    /// <summary>The inputs for the mode; false when the search can't run yet</summary>
    private bool DrawValue()
    {
        switch (Mode)
        {
            case SearchMode.Field:
            {
                RefreshFieldChoices();
                if (FieldChoices.Count == 0)
                {
                    GUI.WrappedText(LOC.Get("BHV_SearchTool_Field_Needs_File"));
                    return false;
                }

                FieldKey ??= FieldChoices[0].Key;

                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("##behaviorSearchField", LOC.Get(FieldKey)))
                {
                    foreach (var (key, _) in FieldChoices)
                        if (ImGui.Selectable($"{LOC.Get(key)}##behaviorSearchField_{key}", key == FieldKey))
                            FieldKey = key;
                    ImGui.EndCombo();
                }
                GUI.Tooltip(LOC.Get("BHV_SearchTool_Field_TT"));

                var type = FieldChoices.FirstOrDefault(c => c.Key == FieldKey).Type;
                if (type is BhvFieldType.F32)
                {
                    if (View.Input.Float("##behaviorSearchFloat", FloatValue, out var f))
                        FloatValue = f;
                }
                else if (View.Input.Int("##behaviorSearchFieldValue", IntValue, int.MinValue, int.MaxValue, out var v))
                {
                    IntValue = v;
                }
                GUI.Tooltip(LOC.Get("BHV_SearchTool_Value_TT"));
                return true;
            }

            case SearchMode.Bytes:
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputText("##behaviorSearchBytes", ref BytesText, 255);
                GUI.Tooltip(LOC.Get("BHV_SearchTool_Bytes_TT"));
                return ParseBytes(BytesText) != null;
            }

            default:
            {
                if (View.Input.Int("##behaviorSearchIntValue", IntValue, int.MinValue, int.MaxValue, out var v))
                    IntValue = v;
                GUI.Tooltip(LOC.Get("BHV_SearchTool_Value_TT"));
                return true;
            }
        }
    }

    /// <summary>The known fields of the selected file's tables (field names depend on version and kind)</summary>
    private void RefreshFieldChoices()
    {
        var file = View.Selection.SelectedFile;
        if (file == FieldChoicesFile)
            return;

        FieldChoicesFile = file;
        FieldChoices = file == null ? new() : BhvRecords.Enumerate(file.BHV)
            .SelectMany(r => r.Fields)
            .GroupBy(f => f.LabelKey)
            .Select(g => (g.Key, g.First().Type))
            .ToList();

        if (FieldKey != null && FieldChoices.All(c => c.Key != FieldKey))
            FieldKey = null;
    }

    private static byte[] ParseBytes(string text)
    {
        var digits = new string(text.Where(Uri.IsHexDigit).ToArray());
        if (digits.Length == 0 || digits.Length % 2 != 0)
            return null;

        var bytes = new byte[digits.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = byte.Parse(digits.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return bytes;
    }

    private void Find()
    {
        var files = Scope.Files();
        var pattern = Mode == SearchMode.Bytes ? ParseBytes(BytesText) : null;

        Results = new();
        var total = 0;

        void Hit(BhvFileWrapper file, BhvRecordKind kind, int s, int t, int c, int index, int sub, string tag, string detail)
        {
            total++;
            if (Results.Count >= BhvResultList.MaxResults)
                return;

            var where = BhvDisplay.LocationLabel(file.BHV, kind, s, t, c, index, sub);
            Results.Add(new(file, kind, s, t, c, index, tag, UI.Current.ImGui_Ref_Text, detail.Length > 0 ? $"{where}  ·  {detail}" : where));
        }

        foreach (var file in files)
        {
            var b = file.BHV;

            switch (Mode)
            {
                case SearchMode.ConditionType:
                    for (int s = 0; s < b.States.Count; s++)
                        for (int t = 0; t < b.States[s].Transitions.Count; t++)
                            for (int c = 0; c < b.States[s].Transitions[t].Conditions.Count; c++)
                                if (b.States[s].Transitions[t].Conditions[c].Type == IntValue)
                                    Hit(file, BhvRecordKind.ConditionData, s, t, c, -1, -1, "", "");
                    break;

                case SearchMode.StateId:
                case SearchMode.StateKind:
                    for (int s = 0; s < b.States.Count; s++)
                    {
                        var v = Mode == SearchMode.StateId ? b.States[s].ID : b.States[s].Kind;
                        if (v == IntValue)
                            Hit(file, BhvRecordKind.StateBody, s, -1, -1, -1, -1, "", "");
                    }
                    break;

                case SearchMode.Field:
                    foreach (var record in BhvRecords.Enumerate(b))
                        foreach (var field in record.Fields.Where(f => f.LabelKey == FieldKey))
                        {
                            if (field.Read(record.Blob) is not double v)
                                continue;

                            var match = field.Type is BhvFieldType.F32
                                ? BitConverter.SingleToInt32Bits((float)v) == BitConverter.SingleToInt32Bits(FloatValue)
                                : (long)v == IntValue;

                            if (match)
                                Hit(file, record.Kind, record.State, record.Transition, record.Condition, record.Index, record.SubIndex,
                                    $"0x{field.Offset:X2}", "");
                        }
                    break;

                case SearchMode.Bytes:
                    foreach (var record in BhvRecords.Enumerate(b))
                        foreach (var offset in BhvRecords.FindBytes(record.Blob, pattern))
                            Hit(file, record.Kind, record.State, record.Transition, record.Condition, record.Index, record.SubIndex,
                                $"0x{offset:X2}", LOC.Get($"BHV_Record_{record.Kind}"));
                    break;
            }
        }

        Summary = LOC.Get("BHV_SearchTool_Summary", total, files.Count);
        if (total > Results.Count)
            Summary += "\n" + LOC.Get("BHV_Tool_Results_Capped", Results.Count);
    }
}
