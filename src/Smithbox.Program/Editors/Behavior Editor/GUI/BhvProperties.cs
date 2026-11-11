using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected record: its structure fields, the known fields inside its raw blobs, and the blobs as hex.
/// P2: every value is editable with undo; blobs keep their length (resizing is P3).
/// </summary>
public class BhvProperties
{
    private readonly BhvEditorView View;

    public BhvProperties(BhvEditorView view)
    {
        View = view;
    }

    private BhvSelection Selection => View.Selection;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_Properties_Header"), LOC.Get("BHV_Properties_Header_TT"));

        var file = Selection.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File"));
            return;
        }

        var b = file.BHV;

        ImGui.BeginChild("bhvProperties", Vector2.Zero, ImGuiChildFlags.Borders);

        if (b.Version != 12)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("BHV_Properties_Old_Version", b.Version));

        switch (Selection.Target)
        {
            case BhvTarget.File: DrawFile(file, b); break;
            case BhvTarget.State: DrawState(file, b); break;
            case BhvTarget.Transition: DrawTransition(file, b); break;
            case BhvTarget.Condition: DrawCondition(file, b); break;
            case BhvTarget.Animation: DrawAnimation(file, b); break;
            case BhvTarget.StructC: DrawStructC(file, b); break;
            case BhvTarget.StructD: DrawStructD(file, b); break;
            default: ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File")); break;
        }

        ImGui.EndChild();
    }

    #region Targets
    private void DrawFile(BhvFileWrapper file, BHV b)
    {
        var transitions = b.States.Sum(s => s.Transitions.Count);
        var conditions = b.States.Sum(s => s.Transitions.Sum(t => t.Conditions.Count));

        if (BeginInfo("bhvFileInfo"))
        {
            Row(LOC.Get("BHV_Properties_File"), file.Key);
            Row(LOC.Get("BHV_Properties_Entry_ID"), file.EntryID.ToString());
            Row(LOC.Get("BHV_Properties_Version"), b.Version.ToString());
            Row(LOC.Get("BHV_Properties_Kind"), BhvDisplay.KindLabel(b));
            Row(LOC.Get("BHV_Properties_Counts"), LOC.Get("BHV_Properties_Counts_Value", b.States.Count, transitions, conditions, b.Animations.Count));
            Row(LOC.Get("BHV_Properties_Struct_C_Size"), $"0x{b.StructCSize:X}");

            EditRow(LOC.Get("BHV_Properties_File_Unk02"), "##fileUnk02", file, b.Unk02, short.MinValue, short.MaxValue,
                () => b.Unk02, v => b.Unk02 = v, v => (short)v);

            ImGui.EndTable();
        }

        Blob("bhvPreamble", LOC.Get("BHV_Properties_Preamble", b.Preamble.Length), file, () => b.Preamble, BhvLayout.PreambleFields(b));
    }

    private void DrawState(BhvFileWrapper file, BHV b)
    {
        var state = Selection.SelectedState;
        if (state == null)
            return;

        var index = Selection.StateIndex;
        var (_, preambleRefs) = BhvIndexRemap.CountStateRefs(b, index);
        var incoming = View.Format.IncomingTransitions(file, index).Count;

        if (BeginInfo("bhvStateInfo"))
        {
            Row(LOC.Get("BHV_Properties_Index"), index.ToString());

            // IDs are unique in every vanilla file (P0); the game finds states by ID in places, so keep them so
            EditRow(LOC.Get("BHV_Properties_ID"), "##stateId", file, state.ID, short.MinValue, short.MaxValue,
                () => state.ID, v => state.ID = v, v => (short)v,
                v => b.States.Any(s => s != state && s.ID == v) ? LOC.Get("BHV_Edit_Duplicate_State_ID", v) : null);
            GUI.Tooltip(LOC.Get("BHV_Properties_ID_TT"));

            EditRow(LOC.Get("BHV_Properties_State_Kind"), "##stateKind", file, state.Kind, short.MinValue, short.MaxValue,
                () => state.Kind, v => state.Kind = v, v => (short)v);
            GUI.Tooltip(LOC.Get("BHV_Properties_State_Kind_TT"));

            Row(LOC.Get("BHV_Properties_Transitions"), LOC.Get("BHV_Properties_Transitions_Value", state.Transitions.Count, incoming));
            Row(LOC.Get("BHV_Properties_Preamble_Refs"), preambleRefs.ToString());

            ImGui.EndTable();
        }

        Blob("bhvStateBody", LOC.Get("BHV_Properties_Body", state.Body.Length), file, () => state.Body, BhvLayout.StateBodyFields(b));
        Blob("bhvStateData", LOC.Get("BHV_Properties_State_Data", state.Data.Length), file, () => state.Data, new(), v => state.Data = v);
    }

    private void DrawTransition(BhvFileWrapper file, BHV b)
    {
        var transition = Selection.SelectedTransition;
        if (transition == null)
            return;

        if (BeginInfo("bhvTransitionInfo"))
        {
            Row(LOC.Get("BHV_Properties_From"), BhvDisplay.StateLabel(b, Selection.StateIndex));
            Row(LOC.Get("BHV_Properties_Index"), Selection.TransitionIndex.ToString());

            // The game reads the target as an index with no range check (0x140814570), so it must exist
            EditRow(LOC.Get("BHV_Properties_Target"), "##transitionTarget", file, transition.TargetStateIndex, 0, b.States.Count - 1,
                () => transition.TargetStateIndex, v => transition.TargetStateIndex = v, v => (int)v);
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Ref_Text, BhvDisplay.StateLabel(b, transition.TargetStateIndex));

            Row(LOC.Get("BHV_Properties_Conditions"), transition.Conditions.Count.ToString());

            ImGui.EndTable();
        }

        Blob("bhvTransitionParams", LOC.Get("BHV_Properties_Params", transition.Params.Length), file, () => transition.Params,
            BhvLayout.TransitionParamFields(b, transition.Params));
    }

    private void DrawCondition(BhvFileWrapper file, BHV b)
    {
        var condition = Selection.SelectedCondition;
        if (condition == null)
            return;

        if (BeginInfo("bhvConditionInfo"))
        {
            Row(LOC.Get("BHV_Properties_Index"), Selection.ConditionIndex.ToString());

            EditRow(LOC.Get("BHV_Properties_Condition_Type"), "##conditionType", file, condition.Type, byte.MinValue, byte.MaxValue,
                () => condition.Type, v => condition.Type = v, v => (byte)v);
            GUI.Tooltip(LOC.Get("BHV_Properties_Condition_Type_TT"));

            EditRow(LOC.Get("BHV_Properties_Condition_Unk02"), "##conditionUnk02", file, condition.Unk02, byte.MinValue, byte.MaxValue,
                () => condition.Unk02, v => condition.Unk02 = v, v => (byte)v);
            GUI.Tooltip(LOC.Get("BHV_Properties_Condition_Unk02_TT"));

            EditRow(LOC.Get("BHV_Properties_Condition_Unk03"), "##conditionUnk03", file, condition.Unk03, byte.MinValue, byte.MaxValue,
                () => condition.Unk03, v => condition.Unk03 = v, v => (byte)v);
            EditRow(LOC.Get("BHV_Properties_Condition_Unk08"), "##conditionUnk08", file, condition.Unk08, byte.MinValue, byte.MaxValue,
                () => condition.Unk08, v => condition.Unk08 = v, v => (byte)v);
            EditRow(LOC.Get("BHV_Properties_Condition_Unk0B"), "##conditionUnk0B", file, condition.Unk0B, byte.MinValue, byte.MaxValue,
                () => condition.Unk0B, v => condition.Unk0B = v, v => (byte)v);

            ImGui.EndTable();
        }

        Blob("bhvConditionData", LOC.Get("BHV_Properties_Condition_Data", condition.Data.Length), file, () => condition.Data, setLength: v => condition.Data = v, fields:
            BhvLayout.ConditionDataFields(b, condition));
    }

    private void DrawAnimation(BhvFileWrapper file, BHV b)
    {
        var index = Selection.AnimationIndex;
        if (index < 0 || index >= b.Animations.Count)
            return;

        if (BeginInfo("bhvAnimationInfo"))
        {
            Row(LOC.Get("BHV_Properties_Index"), index.ToString());
            Row(LOC.Get("BHV_Properties_Used_By"), View.Format.AnimationUserCount(file, index).ToString());
            ImGui.EndTable();
        }

        // Animation records have a fixed size per version, so the array itself is the stable owner
        var record = b.Animations[index];
        Blob("bhvAnimation", LOC.Get("BHV_Properties_Record", record.Length), file, () => record,
            BhvLayout.AnimationFields(b, b.Kind is BHVKind.Weapon));
    }

    private void DrawStructC(BhvFileWrapper file, BHV b)
    {
        var index = Selection.StructIndex;
        if (index < 0 || index >= b.StructCs.Count)
            return;

        if (BeginInfo("bhvStructCInfo"))
        {
            Row(LOC.Get("BHV_Properties_Index"), index.ToString());
            ImGui.EndTable();
        }

        var record = b.StructCs[index];
        Blob("bhvStructC", LOC.Get("BHV_Properties_Record", record.Length), file, () => record, new());
    }

    private void DrawStructD(BhvFileWrapper file, BHV b)
    {
        var index = Selection.StructIndex;
        if (index < 0 || index >= b.StructDs.Count)
            return;

        var d = b.StructDs[index];

        if (BeginInfo("bhvStructDInfo"))
        {
            Row(LOC.Get("BHV_Properties_Index"), index.ToString());
            EditRow(LOC.Get("BHV_Properties_Unk10"), "##structDUnk10", file, d.Unk10, int.MinValue, int.MaxValue,
                () => d.Unk10, v => d.Unk10 = v, v => (int)v);
            Row(LOC.Get("BHV_Properties_Struct_DA_Count"), d.StructDAs.Count.ToString());
            ImGui.EndTable();
        }

        for (int i = 0; i < d.StructDAs.Count; i++)
        {
            var record = d.StructDAs[i];
            Blob($"bhvStructDA_{i}", LOC.Get("BHV_Properties_Struct_DA", i, record.Length), file, () => record, new());
        }
    }
    #endregion

    #region Helpers
    private static bool BeginInfo(string id)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            return false;

        ImGui.TableSetupColumn("name");
        ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);
        return true;
    }

    private static void Row(string name, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(name);
        ImGui.TableNextColumn();
        ImGui.Text(value);
    }

    /// <summary>
    /// A row with an integer box for a typed record field. The name cell is the last item drawn, so a
    /// GUI.Tooltip after this call attaches to the box.
    /// </summary>
    private void EditRow<T>(string name, string id, BhvFileWrapper file, long current, long min, long max,
        Func<T> get, Action<T> set, Func<long, T> convert, Func<long, string> validate = null)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(name);
        ImGui.TableNextColumn();

        if (View.Input.Int(id, current, min, max, out var value, validate, 160))
            View.SetValue(file, get, set, convert(value));
    }

    /// <summary>
    /// A blob's known fields and hex. With <paramref name="setLength"/> (variable-length blobs: state data,
    /// condition data) a length box resizes it: the start is kept and new bytes are zero.
    /// </summary>
    private void Blob(string id, string title, BhvFileWrapper file, Func<byte[]> blob, List<BhvField> fields, Action<byte[]> setLength = null)
    {
        GUI.Spacer();
        GUI.SimpleHeader(title, LOC.Get("BHV_Properties_Blob_TT"));

        if (setLength != null)
        {
            ImGui.Text(LOC.Get("BHV_Properties_Length"));
            ImGui.SameLine();
            if (View.Input.Int($"##{id}_length", blob().Length, 0, BhvStructureEdits.MaxBlobLength, out var length, null, 120))
                View.Structure.Resize(file, blob, setLength, (int)length);
            GUI.Tooltip(LOC.Get("BHV_Properties_Length_TT"));
        }

        View.Format.FieldTable(id, file, blob, fields);

        if (CFG.Current.BehaviorEditor_Properties_Show_Hex)
            View.Format.HexView(id, file, blob, fields);
    }
    #endregion
}
