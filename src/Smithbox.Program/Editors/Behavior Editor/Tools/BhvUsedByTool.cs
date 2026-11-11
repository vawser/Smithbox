using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// Used By: every known field that refers to a TAE animation ID, a BehaviorChangeStateMatrixParam row or a
/// SpEffectParam row, across the files in scope. "From Selection" fills in the value of the selected animation
/// record, transition or condition.
/// </summary>
public class BhvUsedByTool
{
    private readonly BhvEditorView View;

    private static readonly BhvRefKind[] Kinds =
        [BhvRefKind.TaeAnimation, BhvRefKind.ChangeStateMatrixParam, BhvRefKind.SpEffectParam];

    private readonly BhvToolScope Scope;
    private BhvRefKind Kind = BhvRefKind.TaeAnimation;
    private long Value;

    private List<BhvToolResult> Results;
    private string Summary = "";

    public BhvUsedByTool(BhvEditorView view)
    {
        View = view;
        Scope = new(view, "bhvUsedByScope", BhvScope.All);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("BHV_UsedByTool_Header")}##behaviorUsedByToolHeader"))
            return;

        ImGui.BeginChild("BehaviorUsedByToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private static string KindLabel(BhvRefKind kind) => LOC.Get($"BHV_UsedByTool_Kind_{kind}");

    private void Display()
    {
        GUI.WrappedText(LOC.Get("BHV_UsedByTool_Hint"));

        // Reference
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_UsedByTool_Reference_Header"), LOC.Get("BHV_UsedByTool_Reference_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##behaviorUsedByKind", KindLabel(Kind)))
        {
            foreach (var kind in Kinds)
                if (ImGui.Selectable($"{KindLabel(kind)}##behaviorUsedByKind_{kind}", kind == Kind))
                    Kind = kind;
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("BHV_UsedByTool_Kind_TT"));

        if (View.Input.Int("##behaviorUsedByValue", Value, int.MinValue, int.MaxValue, out var value))
            Value = value;
        GUI.Tooltip(LOC.Get("BHV_UsedByTool_Value_TT"));

        var scopeOk = Scope.Draw(allowModified: false);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_UsedByTool_Actions_Header"), LOC.Get("BHV_UsedByTool_Actions_Header_TT"));

        var fromSelection = SelectionValue();

        GUI.ConditionalMultiButtonInput("behaviorUsedByActions",
            "behaviorUsedByFind", LOC.Get("BHV_UsedByTool_Find"), LOC.Get("BHV_UsedByTool_Find_TT"),
            Find, scopeOk,

            "behaviorUsedBySelection", LOC.Get("BHV_UsedByTool_From_Selection"), LOC.Get("BHV_UsedByTool_From_Selection_TT"),
            () => { Kind = fromSelection.Value.Kind; Value = fromSelection.Value.Value; Find(); }, scopeOk && fromSelection != null);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("BHV_UsedByTool_Results_Header"), LOC.Get("BHV_UsedByTool_Results_Header_TT"));

        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("BHV_Tool_Not_Run"));
            return;
        }

        GUI.WrappedText(Summary);
        BhvResultList.Draw(View, "BehaviorUsedByResults", Results);
    }

    /// <summary>The reference the selected record holds: animation TAE ID, transition matrix, condition 43 SpEffect</summary>
    private (BhvRefKind Kind, long Value)? SelectionValue()
    {
        var file = View.Selection.SelectedFile;
        if (file == null)
            return null;

        var b = file.BHV;

        switch (View.Selection.Target)
        {
            case BhvTarget.Animation:
                var id = BhvDisplay.AnimationId(b, View.Selection.AnimationIndex);
                return id >= 0 ? (BhvRefKind.TaeAnimation, id) : null;

            case BhvTarget.Transition:
                var t = View.Selection.SelectedTransition;
                if (t != null && b.Version >= 10 && t.Params.Length >= 8)
                {
                    var matrix = BitConverter.ToInt32(t.Params, BHV.V12.ParamsChangeStateMatrixId);
                    return matrix != 0 ? (BhvRefKind.ChangeStateMatrixParam, matrix) : null;
                }
                return null;

            case BhvTarget.Condition:
                var c = View.Selection.SelectedCondition;
                if (c is { Type: 43 } && c.Data.Length >= BHV.V12.Condition43SpEffectId + 4 && b.Version != 10)
                    return (BhvRefKind.SpEffectParam, BitConverter.ToInt32(c.Data, BHV.V12.Condition43SpEffectId));
                return null;
        }

        return null;
    }

    private void Find()
    {
        var files = Scope.Files();
        Results = new();
        var total = 0;
        var fileHits = 0;

        foreach (var file in files)
        {
            var b = file.BHV;
            var hit = false;

            foreach (var record in BhvRecords.Enumerate(b))
            {
                foreach (var field in record.Fields.Where(f => f.Ref == Kind))
                {
                    if (field.Read(record.Blob) is not double v || (long)v != Value)
                        continue;

                    total++;
                    hit = true;

                    if (Results.Count < BhvResultList.MaxResults)
                    {
                        var where = BhvDisplay.LocationLabel(b, record.Kind, record.State, record.Transition, record.Condition, record.Index, record.SubIndex);
                        Results.Add(new(file, record.Kind, record.State, record.Transition, record.Condition, record.Index,
                            $"0x{field.Offset:X2}", UI.Current.ImGui_Ref_Text, $"{where}  ·  {LOC.Get(field.LabelKey)}"));
                    }
                }
            }

            if (hit)
                fileHits++;
        }

        Summary = LOC.Get("BHV_UsedByTool_Summary", KindLabel(Kind), Value, total, fileHits, files.Count);
        if (total > Results.Count)
            Summary += "\n" + LOC.Get("BHV_Tool_Results_Capped", Results.Count);
    }
}
