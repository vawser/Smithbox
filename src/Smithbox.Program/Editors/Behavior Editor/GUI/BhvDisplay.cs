using StudioCore.Editors.Common;
using Hexa.NET.ImGui;
using SoulsFormats;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Shared formatting for the windows: record labels, field values and what they refer to, and per-file facts that
/// are worked out once per revision (incoming transitions, animation users)
/// </summary>
public class BhvDisplay
{
    private readonly BhvEditorView View;

    public BhvDisplay(BhvEditorView view)
    {
        View = view;
    }

    #region Labels
    public static string StateLabel(BHV b, int index)
    {
        if (index < 0 || index >= b.States.Count)
            return index < 0 ? LOC.Get("BHV_Label_None") : LOC.Get("BHV_Label_Out_Of_Range", index);

        var s = b.States[index];
        return LOC.Get("BHV_Label_State", index, s.ID, s.Kind);
    }

    /// <summary>The TAE animation ID an animation record starts with, or -1</summary>
    public static int AnimationId(BHV b, int index)
    {
        if (index < 0 || index >= b.Animations.Count || b.Animations[index].Length < 4)
            return -1;

        return BitConverter.ToInt32(b.Animations[index], BHV.V12.AnimationId);
    }

    public static string AnimationLabel(BHV b, int index)
    {
        if (index < 0)
            return LOC.Get("BHV_Label_None");

        if (index >= b.Animations.Count)
            return LOC.Get("BHV_Label_Out_Of_Range", index);

        var id = AnimationId(b, index);
        return id < 0 ? LOC.Get("BHV_Label_Animation_No_Id", index) : LOC.Get("BHV_Label_Animation", index, id);
    }

    /// <summary>The animation record a state plays (body +0x00)</summary>
    public static int StateAnimationIndex(BHV.State s) =>
        s.Body.Length >= 2 ? BitConverter.ToInt16(s.Body, BHV.V12.BodyAnimationIndex) : -1;

    /// <summary>Where a record sits, for tool results: "State 11 (ID 11, kind 0) · transition #2 · condition 0"</summary>
    public static string LocationLabel(BHV b, BhvRecordKind kind, int state, int transition, int condition, int index, int subIndex = -1)
    {
        var label = kind switch
        {
            BhvRecordKind.Preamble => LOC.Get("BHV_Location_Preamble"),
            BhvRecordKind.Animation => AnimationLabel(b, index),
            BhvRecordKind.StructC => LOC.Get("BHV_Location_Struct_C", index),
            BhvRecordKind.StructDA => LOC.Get("BHV_Location_Struct_DA", index, subIndex),
            _ => StateLabel(b, state),
        };

        if (kind is BhvRecordKind.StateData)
            label += "  ·  " + LOC.Get("BHV_Location_Data");
        if (kind is BhvRecordKind.TransitionParams or BhvRecordKind.ConditionData && transition >= 0)
            label += "  ·  " + LOC.Get("BHV_Location_Transition", transition);
        if (kind is BhvRecordKind.ConditionData && condition >= 0)
            label += "  ·  " + LOC.Get("BHV_Location_Condition", condition);

        return label;
    }

    public static string KindLabel(BHV b) => b.Kind switch
    {
        BHVKind.Weapon => LOC.Get("BHV_Kind_Weapon"),
        BHVKind.Base => LOC.Get("BHV_Kind_Base"),
        BHVKind.Empty => LOC.Get("BHV_Kind_Empty"),
        _ => LOC.Get("BHV_Kind_Unknown"),
    };
    #endregion

    #region Fields
    public static string FormatValue(BhvField field, double value) => field.Type switch
    {
        BhvFieldType.F32 => ((float)value).ToString("0.###", CultureInfo.InvariantCulture),
        _ => ((long)value).ToString(CultureInfo.InvariantCulture),
    };

    /// <summary>What a field's value means: an enum name, a referenced record, a param row; empty if nothing</summary>
    public string Meaning(BHV b, BhvField field, double value)
    {
        var v = (int)value;

        if (field.Enum != null)
            return View.Enums?.GetName(field.Enum, v) ?? (field.Type is BhvFieldType.U8 && v == 0 ? "" : LOC.Get("BHV_Label_Unknown_Enum_Value"));

        return field.Ref switch
        {
            BhvRefKind.State => v < 0 ? LOC.Get("BHV_Label_None") : StateLabel(b, v),
            BhvRefKind.Animation => v < 0 ? LOC.Get("BHV_Label_None") : AnimationLabel(b, v),
            BhvRefKind.TaeAnimation => v < 0 ? LOC.Get("BHV_Label_None") : LOC.Get("BHV_Label_Tae_Animation"),
            BhvRefKind.ChangeStateMatrixParam => v == 0 ? LOC.Get("BHV_Label_None") : ParamRowLabel(ChangeStateMatrixParam, v),
            BhvRefKind.SpEffectParam => v <= 0 ? LOC.Get("BHV_Label_None") : ParamRowLabel(SpEffectParam, v),
            _ => "",
        };
    }

    public const string ChangeStateMatrixParam = "BehaviorChangeStateMatrixParam";
    public const string SpEffectParam = "SpEffectParam";

    /// <summary>
    /// Whether a param row exists in the project's params; null if the params aren't loaded (Param Editor off)
    /// </summary>
    public bool? ParamRowExists(string param, int id)
    {
        var bank = View.Project.Handler.ParamData?.PrimaryBank;
        if (bank?.Params == null || !bank.Params.TryGetValue(param, out var p))
            return null;

        return p.Rows.Any(r => r.ID == id);
    }

    private string ParamRowLabel(string param, int id) => ParamRowExists(param, id) switch
    {
        false => LOC.Get("BHV_Label_Param_Row_Missing", param, id),
        _ => LOC.Get("BHV_Label_Param_Row", param),
    };

    /// <summary>
    /// A small button after a field's meaning that opens what it refers to: a state or animation record here,
    /// a TAE animation in the Time Act Editor, a param row in the Param Editor
    /// </summary>
    public void ReferenceLink(string id, BhvFileWrapper file, BhvField field, int v)
    {
        var b = file.BHV;

        (string Label, string Tooltip, Action Open) link = field.Ref switch
        {
            BhvRefKind.State when v >= 0 && v < b.States.Count =>
                (LOC.Get("BHV_Link_Go"), LOC.Get("BHV_Link_State_TT"), () => View.Selection.SelectState(v, true)),
            BhvRefKind.Animation when v >= 0 && v < b.Animations.Count =>
                (LOC.Get("BHV_Link_Go"), LOC.Get("BHV_Link_Animation_TT"), () => View.Selection.SelectAnimation(v, true)),
            BhvRefKind.TaeAnimation when v >= 0 =>
                (LOC.Get("BHV_Link_Time_Act"), LOC.Get("BHV_Link_Time_Act_TT", file.Binder.Name, v),
                    () => EditorCommandQueue.AddCommand($"tae/select/{file.Binder.Name}/{v}")),
            BhvRefKind.ChangeStateMatrixParam when v != 0 =>
                (LOC.Get("BHV_Link_Param"), LOC.Get("BHV_Link_Param_TT", ChangeStateMatrixParam, v),
                    () => EditorCommandQueue.AddCommand($"param/select/-1/{ChangeStateMatrixParam}/{v}")),
            BhvRefKind.SpEffectParam when v > 0 =>
                (LOC.Get("BHV_Link_Param"), LOC.Get("BHV_Link_Param_TT", SpEffectParam, v),
                    () => EditorCommandQueue.AddCommand($"param/select/-1/{SpEffectParam}/{v}")),
            _ => (null, null, null),
        };

        if (link.Open == null)
            return;

        ImGui.SameLine();
        if (ImGui.SmallButton($"{link.Label}##{id}"))
            link.Open();
        GUI.Tooltip(link.Tooltip);
    }

    /// <summary>
    /// The known fields of a blob as a table: offset, name, value (editable), meaning. <paramref name="blob"/>
    /// fetches the blob from its owner, so edits go to the right array.
    /// </summary>
    public void FieldTable(string id, BhvFileWrapper file, Func<byte[]> blob, List<BhvField> fields)
    {
        if (fields.Count == 0)
            return;

        var b = file.BHV;
        var bytes = blob();

        if (!ImGui.BeginTable($"{id}_fields", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Offset"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Field"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Value"), ImGuiTableColumnFlags.WidthFixed, 220);
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Meaning"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (int f = 0; f < fields.Count; f++)
        {
            var field = fields[f];
            var value = field.Read(bytes);
            if (value == null)
                continue;

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextDisabled($"0x{field.Offset:X2}");

            ImGui.TableNextColumn();
            ImGui.Text(LOC.Get(field.LabelKey));
            GUI.Tooltip(LOC.Get("BHV_Field_Type_TT", field.Type));

            ImGui.TableNextColumn();
            if (FieldInput($"##{id}_field{f}", b, field, value.Value, out var newValue))
                View.Edit(new BhvBytesChange(file, blob, field.Offset, field.Encode(newValue)));

            ImGui.TableNextColumn();
            var meaning = Meaning(b, field, value.Value);
            if (meaning.Length > 0)
                ImGui.TextColored(field.Enum != null ? UI.Current.ImGui_EnumName_Text : UI.Current.ImGui_Ref_Text, meaning);

            ReferenceLink($"{id}_link{f}", file, field, (int)value.Value);
        }

        ImGui.EndTable();
    }

    /// <summary>
    /// The input for one known field: an enum combo, an index box limited to the file's records, or a number box
    /// </summary>
    private bool FieldInput(string id, BHV b, BhvField field, double value, out double result)
    {
        result = value;

        if (field.Type is BhvFieldType.F32)
        {
            var ok = View.Input.Float(id, (float)value, out var f);
            result = f;
            return ok;
        }

        if (field.Enum != null)
        {
            var ok = View.Input.Enum(id, View.Enums, field.Enum, (int)value, out var e);
            result = e;
            return ok;
        }

        var (min, max) = field.Range;

        // Index slots: -1 (none) or a record that exists. Vanilla values always are (P0 renumber survey).
        switch (field.Ref)
        {
            case BhvRefKind.State: min = -1; max = b.States.Count - 1; break;
            case BhvRefKind.Animation: min = -1; max = b.Animations.Count - 1; break;
        }

        var changed = View.Input.Int(id, (long)value, min, max, out var i);
        result = i;
        return changed;
    }

    private static readonly Vector4[] FieldColors =
    [
        new(1.0f, 0.85f, 0.35f, 1.0f),
        new(0.45f, 0.85f, 1.0f, 1.0f),
    ];

    // The hex cursor: which blob (by its window id) and byte. Kept when the selection moves to another record of
    // the same kind, so the same offset can be compared across states.
    private string HexCursorBlob;
    private int HexCursor = -1;

    /// <summary>
    /// The blob as hex, 16 bytes a row, with known fields coloured and named on hover. Undecoded bytes stay grey
    /// (zero) or white (set). Click a byte to edit at it: the inspector below the rows reads and writes the bytes
    /// there as u8/s16/s32/f32 or as a hex string. The blob keeps its length.
    /// </summary>
    public void HexView(string id, BhvFileWrapper file, Func<byte[]> blob, List<BhvField> fields)
    {
        var bytes = blob();

        if (bytes == null || bytes.Length == 0)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Empty_Blob"));
            return;
        }

        var cursor = HexCursorBlob == id && HexCursor < bytes.Length ? HexCursor : -1;

        // Which field covers each byte
        var owner = new int[bytes.Length];
        Array.Fill(owner, -1);
        for (int f = 0; f < fields.Count; f++)
            for (int i = fields[f].Offset; i < fields[f].Offset + fields[f].Size && i < bytes.Length; i++)
                owner[i] = f;

        ImGui.PushID(id);

        for (int row = 0; row < bytes.Length; row += 16)
        {
            ImGui.TextDisabled($"{row:X4}");

            for (int i = row; i < Math.Min(row + 16, bytes.Length); i++)
            {
                ImGui.SameLine(0, i % 4 == 0 ? 10 : 4);

                var text = bytes[i].ToString("X2");

                if (owner[i] >= 0)
                    ImGui.TextColored(FieldColors[owner[i] % FieldColors.Length], text);
                else if (bytes[i] == 0)
                    ImGui.TextDisabled(text);
                else
                    ImGui.Text(text);

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(owner[i] >= 0
                        ? $"0x{i:X2}: {LOC.Get(fields[owner[i]].LabelKey)} ({fields[owner[i]].Type})"
                        : $"0x{i:X2}");
                }

                if (ImGui.IsItemClicked())
                {
                    HexCursorBlob = id;
                    HexCursor = i == cursor ? -1 : i;
                    cursor = HexCursor;
                }

                if (i == cursor)
                {
                    ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin() - new Vector2(2, 1), ImGui.GetItemRectMax() + new Vector2(2, 1),
                        ImGui.GetColorU32(ImGuiCol.Text));
                }
            }
        }

        if (cursor >= 0)
            HexInspector(file, blob, bytes, cursor);
        else
            ImGui.TextDisabled(LOC.Get("BHV_Hex_Click_To_Edit"));

        ImGui.PopID();
    }

    /// <summary>The bytes at the hex cursor as numbers, each editable</summary>
    private void HexInspector(BhvFileWrapper file, Func<byte[]> blob, byte[] bytes, int at)
    {
        if (!ImGui.BeginTable("hexInspector", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn("type");
        ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(LOC.Get("BHV_Hex_Offset"));
        ImGui.TableNextColumn();
        ImGui.Text($"0x{at:X2} / 0x{bytes.Length:X2}");

        void IntRow(string label, int size, long current, long min, long max, Func<long, byte[]> encode)
        {
            if (at + size > bytes.Length)
                return;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(label);
            ImGui.TableNextColumn();
            if (View.Input.Int($"##hex{label}", current, min, max, out var v, null, 200))
                View.Edit(new BhvBytesChange(file, blob, at, encode(v)));
        }

        IntRow("u8", 1, bytes[at], byte.MinValue, byte.MaxValue, v => [(byte)v]);
        IntRow("s8", 1, (sbyte)bytes[at], sbyte.MinValue, sbyte.MaxValue, v => [(byte)(sbyte)v]);
        if (at + 2 <= bytes.Length)
            IntRow("s16", 2, BitConverter.ToInt16(bytes, at), short.MinValue, short.MaxValue, v => BitConverter.GetBytes((short)v));
        if (at + 4 <= bytes.Length)
        {
            IntRow("s32", 4, BitConverter.ToInt32(bytes, at), int.MinValue, int.MaxValue, v => BitConverter.GetBytes((int)v));

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("f32");
            ImGui.TableNextColumn();
            if (View.Input.Float("##hexf32", BitConverter.ToSingle(bytes, at), out var f, 200))
                View.Edit(new BhvBytesChange(file, blob, at, BitConverter.GetBytes(f)));
        }

        // Up to 16 bytes from the cursor, written back over the same bytes (anything past the end is dropped)
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(LOC.Get("BHV_Hex_Bytes"));
        GUI.Tooltip(LOC.Get("BHV_Hex_Bytes_TT"));
        ImGui.TableNextColumn();
        var span = bytes.AsSpan(at, Math.Min(16, bytes.Length - at));
        var current = string.Join(" ", span.ToArray().Select(x => x.ToString("X2")));
        if (View.Input.HexBytes("##hexBytes", current, out var written))
            View.Edit(new BhvBytesChange(file, blob, at, written));

        ImGui.EndTable();
    }
    #endregion

    #region Per-file facts
    private BhvFileWrapper CachedFile;
    private int CachedRevision = -1;
    private List<(int State, int Transition)>[] Incoming = [];
    private int[] AnimationUsers = [];

    private void Refresh(BhvFileWrapper file)
    {
        if (file == CachedFile && file.Revision == CachedRevision)
            return;

        CachedFile = file;
        CachedRevision = file.Revision;

        var b = file.BHV;

        Incoming = new List<(int, int)>[b.States.Count];
        for (int i = 0; i < Incoming.Length; i++)
            Incoming[i] = new();

        for (int s = 0; s < b.States.Count; s++)
            for (int t = 0; t < b.States[s].Transitions.Count; t++)
            {
                var target = b.States[s].Transitions[t].TargetStateIndex;
                if (target >= 0 && target < Incoming.Length)
                    Incoming[target].Add((s, t));
            }

        AnimationUsers = new int[b.Animations.Count];
        var slots = BhvLayout.BodyAnimationSlots(b);
        foreach (var s in b.States)
            foreach (var off in slots)
            {
                if (off + 2 > s.Body.Length)
                    continue;

                var a = BitConverter.ToInt16(s.Body, off);
                if (a >= 0 && a < AnimationUsers.Length)
                    AnimationUsers[a]++;
            }
    }

    /// <summary>The transitions that lead into a state, as (state, transition) indices</summary>
    public List<(int State, int Transition)> IncomingTransitions(BhvFileWrapper file, int state)
    {
        Refresh(file);
        return state >= 0 && state < Incoming.Length ? Incoming[state] : new();
    }

    /// <summary>How many state body slots play an animation record</summary>
    public int AnimationUserCount(BhvFileWrapper file, int animation)
    {
        Refresh(file);
        return animation >= 0 && animation < AnimationUsers.Length ? AnimationUsers[animation] : 0;
    }
    #endregion
}
