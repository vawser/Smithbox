using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected state's transitions with their conditions, and the transitions that lead into it.
/// The game tries transitions last to first, skips one to the current state, and runs conditions in order
/// (exe 0x140814C30); Game Order shows them in that order.
/// </summary>
public class BhvTransitionView
{
    private readonly BhvEditorView View;

    public BhvTransitionView(BhvEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_TransitionView_Header"), LOC.Get("BHV_TransitionView_Header_TT"));

        var file = View.Selection.SelectedFile;
        var state = View.Selection.SelectedState;

        if (file == null || state == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_State"));
            return;
        }

        var b = file.BHV;
        var stateIndex = View.Selection.StateIndex;

        ImGui.BeginChild("bhvTransitions", Vector2.Zero, ImGuiChildFlags.Borders);

        ImGui.Text(BhvDisplay.StateLabel(b, stateIndex));

        // Outgoing
        GUI.SimpleHeader(LOC.Get("BHV_TransitionView_Outgoing", state.Transitions.Count), LOC.Get("BHV_TransitionView_Outgoing_TT"));

        if (state.Transitions.Count == 0)
            ImGui.TextDisabled(LOC.Get("BHV_TransitionView_No_Outgoing"));

        var order = Enumerable.Range(0, state.Transitions.Count);
        if (CFG.Current.BehaviorEditor_Transitions_Game_Order)
            order = order.Reverse();

        foreach (var t in order)
            DrawTransition(b, state, stateIndex, t);

        GUI.Spacer();

        // Incoming
        var incoming = View.Format.IncomingTransitions(file, stateIndex);
        GUI.SimpleHeader(LOC.Get("BHV_TransitionView_Incoming", incoming.Count), LOC.Get("BHV_TransitionView_Incoming_TT"));

        if (incoming.Count == 0)
            ImGui.TextDisabled(LOC.Get("BHV_StateList_No_Incoming_TT"));

        foreach (var (from, t) in incoming)
        {
            if (ImGui.Selectable($"{BhvDisplay.StateLabel(b, from)}  #{t}##bhvIncoming_{from}_{t}"))
                View.Selection.SelectState(from, true);
            GUI.Tooltip(LOC.Get("BHV_TransitionView_Jump_TT"));
        }

        ImGui.EndChild();
    }

    private void DrawTransition(BHV b, BHV.State state, int stateIndex, int t)
    {
        var transition = state.Transitions[t];
        var selected = View.Selection.TransitionIndex == t && View.Selection.Target is BhvTarget.Transition;

        var matrixId = transition.Params.Length >= 8 && b.Version >= 10 ? BitConverter.ToInt32(transition.Params, BHV.V12.ParamsChangeStateMatrixId) : 0;
        var label = LOC.Get("BHV_TransitionView_Transition", t, BhvDisplay.StateLabel(b, transition.TargetStateIndex), transition.Conditions.Count);
        if (matrixId != 0)
            label += "  " + LOC.Get("BHV_TransitionView_Matrix", matrixId);

        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow;
        if (selected)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (transition.Conditions.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;

        var open = ImGui.TreeNodeEx($"{label}##bhvTransition_{t}", flags);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !ImGui.IsItemToggledOpen())
            View.Selection.SelectTransition(t);

        // Double-click follows the transition
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            View.Selection.SelectState(transition.TargetStateIndex, true);

        GUI.Tooltip(transition.TargetStateIndex == stateIndex
            ? LOC.Get("BHV_TransitionView_Self_TT")
            : LOC.Get("BHV_TransitionView_Transition_TT"));

        if (!open)
            return;

        for (int c = 0; c < transition.Conditions.Count; c++)
            DrawCondition(b, transition.Conditions[c], t, c);

        ImGui.TreePop();
    }

    private void DrawCondition(BHV b, BHV.Condition condition, int t, int c)
    {
        var selected = View.Selection.Target is BhvTarget.Condition
            && View.Selection.TransitionIndex == t && View.Selection.ConditionIndex == c;

        var label = LOC.Get("BHV_TransitionView_Condition", c, condition.Type, condition.Data.Length);

        // The first known field, as a hint of what the condition checks
        var field = BhvLayout.ConditionDataFields(b, condition).FirstOrDefault();
        var value = field?.Read(condition.Data);
        if (value != null)
        {
            var meaning = View.Format.Meaning(b, field, value.Value);
            label += $"  {LOC.Get(field.LabelKey)} = {BhvDisplay.FormatValue(field, value.Value)}";
            if (meaning.Length > 0)
                label += $" ({meaning})";
        }

        // Unk02 keeps a failed condition from failing the transition
        if (condition.Unk02 != 0)
            label += "  " + LOC.Get("BHV_TransitionView_Optional");

        if (ImGui.Selectable($"{label}##bhvCondition_{t}_{c}", selected))
            View.Selection.SelectCondition(t, c);
    }
}
