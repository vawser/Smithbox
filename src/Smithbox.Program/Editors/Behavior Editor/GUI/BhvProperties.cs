using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected record: its structure fields, the known fields inside its raw blobs, and the blobs as hex.
/// Read-only in P1.
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

        switch (Selection.Target)
        {
            case BhvTarget.File: DrawFile(file, b); break;
            case BhvTarget.State: DrawState(file, b); break;
            case BhvTarget.Transition: DrawTransition(b); break;
            case BhvTarget.Condition: DrawCondition(b); break;
            case BhvTarget.Animation: DrawAnimation(file, b); break;
            case BhvTarget.StructC: DrawStructC(b); break;
            case BhvTarget.StructD: DrawStructD(b); break;
            default: ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File")); break;
        }

        ImGui.EndChild();
    }

    #region Targets
    private void DrawFile(BhvFileWrapper file, BHV b)
    {
        var transitions = b.States.Sum(s => s.Transitions.Count);
        var conditions = b.States.Sum(s => s.Transitions.Sum(t => t.Conditions.Count));

        Info("bhvFileInfo",
            (LOC.Get("BHV_Properties_File"), file.Key),
            (LOC.Get("BHV_Properties_Entry_ID"), file.EntryID.ToString()),
            (LOC.Get("BHV_Properties_Version"), b.Version.ToString()),
            (LOC.Get("BHV_Properties_Kind"), BhvDisplay.KindLabel(b)),
            (LOC.Get("BHV_Properties_Counts"), LOC.Get("BHV_Properties_Counts_Value", b.States.Count, transitions, conditions, b.Animations.Count)),
            (LOC.Get("BHV_Properties_Struct_C_Size"), $"0x{b.StructCSize:X}"));

        if (b.Version != 12)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("BHV_Properties_Old_Version", b.Version));

        Blob("bhvPreamble", LOC.Get("BHV_Properties_Preamble", b.Preamble.Length), b, b.Preamble, BhvLayout.PreambleFields(b));
    }

    private void DrawState(BhvFileWrapper file, BHV b)
    {
        var state = Selection.SelectedState;
        if (state == null)
            return;

        var index = Selection.StateIndex;
        var (_, preambleRefs) = BhvIndexRemap.CountStateRefs(b, index);
        var incoming = View.Format.IncomingTransitions(file, index).Count;

        Info("bhvStateInfo",
            (LOC.Get("BHV_Properties_Index"), index.ToString()),
            (LOC.Get("BHV_Properties_ID"), state.ID.ToString()),
            (LOC.Get("BHV_Properties_State_Kind"), state.Kind.ToString()),
            (LOC.Get("BHV_Properties_Transitions"), LOC.Get("BHV_Properties_Transitions_Value", state.Transitions.Count, incoming)),
            (LOC.Get("BHV_Properties_Preamble_Refs"), preambleRefs.ToString()));

        Blob("bhvStateBody", LOC.Get("BHV_Properties_Body", state.Body.Length), b, state.Body, BhvLayout.StateBodyFields(b));
        Blob("bhvStateData", LOC.Get("BHV_Properties_State_Data", state.Data.Length), b, state.Data, new());
    }

    private void DrawTransition(BHV b)
    {
        var transition = Selection.SelectedTransition;
        if (transition == null)
            return;

        Info("bhvTransitionInfo",
            (LOC.Get("BHV_Properties_From"), BhvDisplay.StateLabel(b, Selection.StateIndex)),
            (LOC.Get("BHV_Properties_Index"), Selection.TransitionIndex.ToString()),
            (LOC.Get("BHV_Properties_Target"), BhvDisplay.StateLabel(b, transition.TargetStateIndex)),
            (LOC.Get("BHV_Properties_Conditions"), transition.Conditions.Count.ToString()));

        Blob("bhvTransitionParams", LOC.Get("BHV_Properties_Params", transition.Params.Length), b, transition.Params, BhvLayout.TransitionParamFields(b, transition.Params));
    }

    private void DrawCondition(BHV b)
    {
        var condition = Selection.SelectedCondition;
        if (condition == null)
            return;

        Info("bhvConditionInfo",
            (LOC.Get("BHV_Properties_Index"), Selection.ConditionIndex.ToString()),
            (LOC.Get("BHV_Properties_Condition_Type"), condition.Type.ToString()),
            (LOC.Get("BHV_Properties_Condition_Unk02"), condition.Unk02.ToString()),
            (LOC.Get("BHV_Properties_Condition_Unk03"), condition.Unk03.ToString()),
            (LOC.Get("BHV_Properties_Condition_Unk08"), condition.Unk08.ToString()),
            (LOC.Get("BHV_Properties_Condition_Unk0B"), condition.Unk0B.ToString()));
        GUI.Tooltip(LOC.Get("BHV_Properties_Condition_Unk02_TT"));

        Blob("bhvConditionData", LOC.Get("BHV_Properties_Condition_Data", condition.Data.Length), b, condition.Data, BhvLayout.ConditionDataFields(b, condition));
    }

    private void DrawAnimation(BhvFileWrapper file, BHV b)
    {
        var index = Selection.AnimationIndex;
        if (index < 0 || index >= b.Animations.Count)
            return;

        Info("bhvAnimationInfo",
            (LOC.Get("BHV_Properties_Index"), index.ToString()),
            (LOC.Get("BHV_Properties_Used_By"), View.Format.AnimationUserCount(file, index).ToString()));

        Blob("bhvAnimation", LOC.Get("BHV_Properties_Record", b.Animations[index].Length), b, b.Animations[index], BhvLayout.AnimationFields(b, b.Kind is BHVKind.Weapon));
    }

    private void DrawStructC(BHV b)
    {
        var index = Selection.StructIndex;
        if (index < 0 || index >= b.StructCs.Count)
            return;

        Info("bhvStructCInfo", (LOC.Get("BHV_Properties_Index"), index.ToString()));
        Blob("bhvStructC", LOC.Get("BHV_Properties_Record", b.StructCs[index].Length), b, b.StructCs[index], new());
    }

    private void DrawStructD(BHV b)
    {
        var index = Selection.StructIndex;
        if (index < 0 || index >= b.StructDs.Count)
            return;

        var d = b.StructDs[index];

        Info("bhvStructDInfo",
            (LOC.Get("BHV_Properties_Index"), index.ToString()),
            (LOC.Get("BHV_Properties_Unk10"), d.Unk10.ToString()),
            (LOC.Get("BHV_Properties_Struct_DA_Count"), d.StructDAs.Count.ToString()));

        for (int i = 0; i < d.StructDAs.Count; i++)
            Blob($"bhvStructDA_{i}", LOC.Get("BHV_Properties_Struct_DA", i, d.StructDAs[i].Length), b, d.StructDAs[i], new());
    }
    #endregion

    #region Helpers
    private static void Info(string id, params (string Name, string Value)[] rows)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn("name");
        ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

        foreach (var (name, value) in rows)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(name);
            ImGui.TableNextColumn();
            ImGui.Text(value);
        }

        ImGui.EndTable();
    }

    private void Blob(string id, string title, BHV b, byte[] blob, List<BhvField> fields)
    {
        GUI.Spacer();
        GUI.SimpleHeader(title, LOC.Get("BHV_Properties_Blob_TT"));

        View.Format.FieldTable(id, b, blob, fields);

        if (CFG.Current.BehaviorEditor_Properties_Show_Hex)
            View.Format.HexView(id, blob, fields);
    }
    #endregion
}
