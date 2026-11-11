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
            BhvRefKind.ChangeStateMatrixParam => v == 0 ? LOC.Get("BHV_Label_None") : LOC.Get("BHV_Label_Param_Row", "BehaviorChangeStateMatrixParam"),
            BhvRefKind.SpEffectParam => LOC.Get("BHV_Label_Param_Row", "SpEffectParam"),
            _ => "",
        };
    }

    /// <summary>
    /// The known fields of a blob as a table: offset, name, value, meaning
    /// </summary>
    public void FieldTable(string id, BHV b, byte[] blob, List<BhvField> fields)
    {
        if (fields.Count == 0)
            return;

        if (!ImGui.BeginTable($"{id}_fields", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Offset"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Field"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Value"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Meaning"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        foreach (var field in fields)
        {
            var value = field.Read(blob);
            if (value == null)
                continue;

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextDisabled($"0x{field.Offset:X2}");

            ImGui.TableNextColumn();
            ImGui.Text(LOC.Get(field.LabelKey));
            GUI.Tooltip(LOC.Get("BHV_Field_Type_TT", field.Type));

            ImGui.TableNextColumn();
            ImGui.Text(FormatValue(field, value.Value));

            ImGui.TableNextColumn();
            var meaning = Meaning(b, field, value.Value);
            if (meaning.Length > 0)
                ImGui.TextColored(field.Enum != null ? UI.Current.ImGui_EnumName_Text : UI.Current.ImGui_Ref_Text, meaning);
        }

        ImGui.EndTable();
    }

    private static readonly Vector4[] FieldColors =
    [
        new(1.0f, 0.85f, 0.35f, 1.0f),
        new(0.45f, 0.85f, 1.0f, 1.0f),
    ];

    /// <summary>
    /// The blob as hex, 16 bytes a row, with known fields coloured and named on hover. Undecoded bytes stay grey
    /// (zero) or white (set).
    /// </summary>
    public void HexView(string id, byte[] blob, List<BhvField> fields)
    {
        if (blob == null || blob.Length == 0)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Empty_Blob"));
            return;
        }

        // Which field covers each byte
        var owner = new int[blob.Length];
        Array.Fill(owner, -1);
        for (int f = 0; f < fields.Count; f++)
            for (int i = fields[f].Offset; i < fields[f].Offset + fields[f].Size && i < blob.Length; i++)
                owner[i] = f;

        ImGui.PushID(id);

        for (int row = 0; row < blob.Length; row += 16)
        {
            ImGui.TextDisabled($"{row:X4}");

            for (int i = row; i < Math.Min(row + 16, blob.Length); i++)
            {
                ImGui.SameLine(0, i % 4 == 0 ? 10 : 4);

                var text = blob[i].ToString("X2");

                if (owner[i] >= 0)
                {
                    ImGui.TextColored(FieldColors[owner[i] % FieldColors.Length], text);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"0x{fields[owner[i]].Offset:X2} {LOC.Get(fields[owner[i]].LabelKey)} ({fields[owner[i]].Type})");
                }
                else if (blob[i] == 0)
                {
                    ImGui.TextDisabled(text);
                }
                else
                {
                    ImGui.Text(text);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"0x{i:X2}");
                }
            }
        }

        ImGui.PopID();
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
