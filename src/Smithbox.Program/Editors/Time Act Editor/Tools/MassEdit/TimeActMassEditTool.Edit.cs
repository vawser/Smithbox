using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The edit half of the mass edit: the steps built from pickers, the preview of every change, and Apply.
/// The preview is worked out again whenever the steps or the matches change, so Apply always does what it shows.
/// </summary>
public partial class TimeActMassEditTool
{
    /// <summary>
    /// A step as it is being edited. It is copied into a <see cref="TimeActMassEditStep"/> when the plan is made.
    /// </summary>
    private class StepInput
    {
        public string Field = "";
        public TimeActMassEditOperator Operator = TimeActMassEditOperator.Set;
        public string Value = "";
        public bool Frames = true;

        // The text typed to narrow the list of fields
        public string FieldFilter = "";

        public TimeActMassEditStep ToStep() => new(Field, Operator, Value.Trim(), Frames);
    }

    /// <summary>
    /// A field that the steps can edit: the times, and the parameters of the event types among the matches
    /// </summary>
    private record EditField(string Name, string DisplayName, TimeActMassEditFieldKind Kind,
        TAE.Template.ParameterTemplate Parameter, int EventCount);

    private TimeActMassEditAction Action = TimeActMassEditAction.Edit;
    private List<StepInput> Steps = [new()];
    private int StepsVersion;

    private readonly TimeActMassEditPlanner Planner;

    private TimeActMassEditPlan Plan;
    private int PlanStepsVersion = -1;
    private List<ResultRow> PlanFor;
    private int PlanForCount;

    // Events without their template applied are read through copies, made once for as long as the matches are kept
    private Dictionary<TAE.Event, TAE.Event> ReadableCache = new();

    // The fields of the matched event types, worked out once the matches are in
    private List<EditField> EditFields;
    private List<ResultRow> EditFieldsFor;
    private int EditFieldsForCount;

    // The label of each change, made the first time it is drawn or filtered
    private string[] ChangeLabels;

    // Toggled by the eye on the list's header
    private bool ShowChangesList = true;

    private string ChangeFilter = "";
    private bool ExactChangeFilter = false;
    private List<int> FilteredChanges;
    private TimeActMassEditPlan FilteredChangesFor;
    private string CachedChangeFilter = "";
    private bool CachedExactChangeFilter = false;

    // Set once the plan has been applied, so it can't be applied a second time on top of itself
    private TimeActMassEditApplier.Result AppliedResult;

    private void StepsChanged()
    {
        StepsVersion++;
    }

    private void ClearPlan()
    {
        Plan = null;
        PlanFor = null;
        PlanStepsVersion = -1;
        ChangeLabels = null;
        FilteredChanges = null;
        EditFields = null;
        ReadableCache = new();
        AppliedResult = null;
        DeleteResult = null;
        ClearCreatePlan();
    }

    private void DisplayEdit(TAE.Template.BankTemplate template)
    {
        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Edit_No_Matches"));
            return;
        }

        if (Run is { Done: false })
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Edit_Finding", Results.Count));
            return;
        }

        // What the edit works on, as the matches are in the other tab
        GUI.WrappedText(ResultTarget is TimeActMassEditTarget.Events
            ? LOC.Get("TAE_MassEditTool_Matches_Events", Results.Count, ResultAnimations.Count)
            : LOC.Get("TAE_MassEditTool_Matches_Animations", Results.Count));

        // Action
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Action_Header"),
            LOC.Get("TAE_MassEditTool_Action_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditAction", GetActionName(Action)))
        {
            foreach (var action in Enum.GetValues<TimeActMassEditAction>())
            {
                if (ImGui.Selectable($"{GetActionName(action)}##massEditAction{action}", Action == action))
                {
                    Action = action;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Action_Header_TT"));

        if (Action is TimeActMassEditAction.Delete)
        {
            DisplayDelete();
            return;
        }

        if (Action is TimeActMassEditAction.Create)
        {
            DisplayCreate(template);
            return;
        }

        if (ResultTarget is TimeActMassEditTarget.Animations)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Edit_Animations_Not_Supported"));
            return;
        }

        if (Results.Count == 0)
            return;

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Steps_Header"),
            LOC.Get("TAE_MassEditTool_Steps_Header_TT"));

        var fields = GetEditFields(template);

        DisplaySteps(Steps, fields);

        UpdatePlan(template);

        DisplayChanges(template);
    }

    private static string GetActionName(TimeActMassEditAction action) => action switch
    {
        TimeActMassEditAction.Edit => LOC.Get("TAE_MassEditTool_Action_Edit"),
        TimeActMassEditAction.Delete => LOC.Get("TAE_MassEditTool_Action_Delete"),
        TimeActMassEditAction.Create => LOC.Get("TAE_MassEditTool_Action_Create"),
        _ => $"{action}"
    };

    #region Fields
    /// <summary>
    /// The times, then the parameters of each event type among the matches, in the order of the types.
    /// A parameter shared by several types is listed once, with the number of matched events that have it.
    /// </summary>
    private List<EditField> GetEditFields(TAE.Template.BankTemplate template)
    {
        if (EditFields != null && EditFieldsFor == Results && EditFieldsForCount == Results.Count)
            return EditFields;

        var fields = new List<EditField>
        {
            new(TimeActMassEditFields.Times, LOC.Get("TAE_MassEditTool_Field_Times"), TimeActMassEditFieldKind.Times, null, Results.Count),
            new(TimeActMassEditFields.StartTime, LOC.Get("TAE_PropertiesWindow_Start_Time"), TimeActMassEditFieldKind.Time, null, Results.Count),
            new(TimeActMassEditFields.EndTime, LOC.Get("TAE_PropertiesWindow_End_Time"), TimeActMassEditFieldKind.Time, null, Results.Count)
        };

        if (template != null)
        {
            var typeCounts = Results
                .Where(e => e.Match.Event != null)
                .GroupBy(e => e.Match.Event.Type)
                .OrderBy(e => e.Key)
                .Select(e => (Type: e.Key, Count: e.Count()));

            var data = Project.Handler.TimeActData;
            var index = new Dictionary<string, int>();

            foreach (var (type, count) in typeCounts)
            {
                if (!template.TryGetValue(type, out var eventTemplate))
                    continue;

                foreach (var parameter in eventTemplate.Values)
                {
                    // Fixed values aren't parameters as such, so they can't be edited either
                    if (parameter.ValueToAssert != null)
                        continue;

                    if (index.TryGetValue(parameter.Name, out var at))
                    {
                        fields[at] = fields[at] with { EventCount = fields[at].EventCount + count };
                        continue;
                    }

                    var meta = data.GetFieldMeta(eventTemplate, parameter.Name);
                    var kind = TimeActMassEditFields.GetKind(parameter.Name, parameter, meta);

                    index[parameter.Name] = fields.Count;
                    fields.Add(new(parameter.Name, data.GetFieldDisplayName(eventTemplate, parameter.Name), kind, parameter, count));
                }
            }
        }

        EditFields = fields;
        EditFieldsFor = Results;
        EditFieldsForCount = Results.Count;

        return fields;
    }
    #endregion

    #region Steps
    /// <summary>
    /// The rows of steps, which edit the matches, or set the parameters of the events Create adds
    /// </summary>
    private void DisplaySteps(List<StepInput> steps, List<EditField> fields)
    {
        StepInput removed = null;

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            var field = fields.FirstOrDefault(e => e.Name == step.Field);

            // Field
            var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(-removeWidth);

            DisplayStepFieldPicker(step, i, field, fields);

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##massEditStepRemove{i}", DPI.IconButtonSize))
            {
                removed = step;
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Step_Remove_TT"));

            if (field != null)
            {
                DisplayStepOperator(step, i, field);
            }
        }

        if (removed != null)
        {
            steps.Remove(removed);
            StepsChanged();
        }

        GUI.MultiButtonInput("massEditStepActions",
            "massEditStepAdd",
            LOC.Get("TAE_MassEditTool_Step_Add"),
            LOC.Get("TAE_MassEditTool_Step_Add_TT"),
            () =>
            {
                steps.Add(new());
                StepsChanged();
            });
    }

    private void DisplayStepFieldPicker(StepInput step, int index, EditField field, List<EditField> fields)
    {
        var preview = field != null ? field.DisplayName : LOC.Get("TAE_MassEditTool_Step_Pick_Field");

        if (ImGui.BeginCombo($"##massEditStepField{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##massEditStepFieldFilter{index}", LOC.Get("TAE_MassEditTool_Field_Filter_Hint"), ref step.FieldFilter, 255);

            var filter = step.FieldFilter.Trim();

            foreach (var candidate in fields)
            {
                var label = candidate.DisplayName == candidate.Name || candidate.Parameter == null
                    ? candidate.DisplayName
                    : $"{candidate.DisplayName}  ({candidate.Name})";

                // Fields only some of the matches have are marked with how many do
                if (candidate.EventCount < Results.Count)
                {
                    label = $"{label}  {LOC.Get("TAE_MassEditTool_Field_Event_Count", candidate.EventCount)}";
                }

                if (filter != "" && !label.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##massEditStepField{index}_{candidate.Name}", step.Field == candidate.Name))
                {
                    if (step.Field != candidate.Name)
                    {
                        step.Field = candidate.Name;
                        step.FieldFilter = "";

                        var operators = TimeActMassEditFields.GetOperators(candidate.Kind);

                        if (!operators.Contains(step.Operator))
                        {
                            step.Operator = operators[0];
                        }

                        step.Value = "";

                        StepsChanged();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Step_Field_TT"));
    }

    private void DisplayStepOperator(StepInput step, int index, EditField field)
    {
        var operators = TimeActMassEditFields.GetOperators(field.Kind);

        if (!operators.Contains(step.Operator))
        {
            step.Operator = operators[0];
            StepsChanged();
        }

        var takesValue = TimeActMassEditFields.TakesValue(step.Operator);
        var isTime = field.Kind is TimeActMassEditFieldKind.Time or TimeActMassEditFieldKind.Times;

        ImGui.SetNextItemWidth(takesValue ? ImGui.GetContentRegionAvail().X * 0.35f : -1);

        if (ImGui.BeginCombo($"##massEditStepOperator{index}", GetOperatorName(step.Operator)))
        {
            foreach (var op in operators)
            {
                if (ImGui.Selectable($"{GetOperatorName(op)}##massEditStepOperator{index}_{op}", step.Operator == op))
                {
                    step.Operator = op;
                    StepsChanged();
                }
                GUI.Tooltip(GetOperatorTooltip(op));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetOperatorTooltip(step.Operator));

        if (!takesValue)
            return;

        ImGui.SameLine();

        var choices = step.Operator is TimeActMassEditOperator.Set ? GetValueChoices(field) : null;

        if (choices != null)
        {
            DisplayStepChoices(step, index, choices);
            return;
        }

        var showFrames = isTime && TimeActMassEditFields.TakesTime(step.Operator);
        var framesWidth = showFrames
            ? ImGui.CalcTextSize(LOC.Get("TAE_MassEditTool_Step_Frames")).X + ImGui.GetFrameHeight() +
              ImGui.GetStyle().ItemInnerSpacing.X + ImGui.GetStyle().ItemSpacing.X
            : 0;

        // A width of 0 means the default width rather than none, which runs off the edge after the operator,
        // so the input fills the row up to the checkbox, or all of it without one
        ImGui.SetNextItemWidth(showFrames ? -framesWidth : -1);

        if (ImGui.InputTextWithHint($"##massEditStepValue{index}", GetValueHint(step, field), ref step.Value, 255))
        {
            StepsChanged();
        }
        GUI.Tooltip(field.Parameter != null
            ? LOC.Get("TAE_MassEditTool_Step_Value_Typed_TT", field.Parameter.Type)
            : LOC.Get("TAE_MassEditTool_Step_Value_TT"));

        if (showFrames)
        {
            ImGui.SameLine();

            if (ImGui.Checkbox($"{LOC.Get("TAE_MassEditTool_Step_Frames")}##massEditStepFrames{index}", ref step.Frames))
            {
                StepsChanged();
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Step_Frames_TT"));
        }
    }

    private static string GetValueHint(StepInput step, EditField field)
    {
        if (step.Operator is TimeActMassEditOperator.Scale or TimeActMassEditOperator.MultiplyBy or TimeActMassEditOperator.DivideBy)
            return LOC.Get("TAE_MassEditTool_Step_Factor_Hint");

        if (field.Kind is TimeActMassEditFieldKind.Time or TimeActMassEditFieldKind.Times)
            return LOC.Get(step.Frames ? "TAE_MassEditTool_Step_Frames_Hint" : "TAE_MassEditTool_Step_Seconds_Hint");

        return LOC.Get("TAE_MassEditTool_Step_Value_Hint");
    }

    /// <summary>
    /// The values a field can be set to from a list, each with the text it is parsed from
    /// </summary>
    private static List<(string Label, string Value)> GetValueChoices(EditField field)
    {
        return field.Kind switch
        {
            TimeActMassEditFieldKind.Enum => field.Parameter.EnumEntries
                .Select(e => ($"{e.Key} ({TimeActQueryMatcher.FormatValue(e.Value)})", e.Key))
                .ToList(),
            TimeActMassEditFieldKind.Bool =>
                [(LOC.Get("TAE_MassEditTool_Value_True"), "True"), (LOC.Get("TAE_MassEditTool_Value_False"), "False")],
            TimeActMassEditFieldKind.NumberBool =>
                [(LOC.Get("TAE_MassEditTool_Value_On"), "1"), (LOC.Get("TAE_MassEditTool_Value_Off"), "0")],
            _ => null
        };
    }

    private void DisplayStepChoices(StepInput step, int index, List<(string Label, string Value)> choices)
    {
        var preview = step.Value == ""
            ? LOC.Get("TAE_MassEditTool_Step_Pick_Value")
            : choices.FirstOrDefault(e => e.Value == step.Value).Label ?? step.Value;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##massEditStepChoice{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            for (int i = 0; i < choices.Count; i++)
            {
                var (label, value) = choices[i];

                if (ImGui.Selectable($"{label}##massEditStepChoice{index}_{i}", step.Value == value))
                {
                    step.Value = value;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Step_Value_TT"));
    }

    private static string GetOperatorName(TimeActMassEditOperator op) => op switch
    {
        TimeActMassEditOperator.Set => LOC.Get("TAE_MassEditTool_Operator_Set"),
        TimeActMassEditOperator.Add => LOC.Get("TAE_MassEditTool_Operator_Add"),
        TimeActMassEditOperator.Subtract => LOC.Get("TAE_MassEditTool_Operator_Subtract"),
        TimeActMassEditOperator.MultiplyBy => LOC.Get("TAE_MassEditTool_Operator_Multiply"),
        TimeActMassEditOperator.DivideBy => LOC.Get("TAE_MassEditTool_Operator_Divide"),
        TimeActMassEditOperator.AtLeast => LOC.Get("TAE_MassEditTool_Operator_At_Least"),
        TimeActMassEditOperator.AtMost => LOC.Get("TAE_MassEditTool_Operator_At_Most"),
        TimeActMassEditOperator.ResetToDefault => LOC.Get("TAE_MassEditTool_Operator_Reset"),
        TimeActMassEditOperator.Toggle => LOC.Get("TAE_MassEditTool_Operator_Toggle"),
        TimeActMassEditOperator.Shift => LOC.Get("TAE_MassEditTool_Operator_Shift"),
        TimeActMassEditOperator.Scale => LOC.Get("TAE_MassEditTool_Operator_Scale"),
        TimeActMassEditOperator.SetLength => LOC.Get("TAE_MassEditTool_Operator_Set_Length"),
        _ => $"{op}"
    };

    private static string GetOperatorTooltip(TimeActMassEditOperator op) => op switch
    {
        TimeActMassEditOperator.Set => LOC.Get("TAE_MassEditTool_Operator_Set_TT"),
        TimeActMassEditOperator.Add => LOC.Get("TAE_MassEditTool_Operator_Add_TT"),
        TimeActMassEditOperator.Subtract => LOC.Get("TAE_MassEditTool_Operator_Subtract_TT"),
        TimeActMassEditOperator.MultiplyBy => LOC.Get("TAE_MassEditTool_Operator_Multiply_TT"),
        TimeActMassEditOperator.DivideBy => LOC.Get("TAE_MassEditTool_Operator_Divide_TT"),
        TimeActMassEditOperator.AtLeast => LOC.Get("TAE_MassEditTool_Operator_At_Least_TT"),
        TimeActMassEditOperator.AtMost => LOC.Get("TAE_MassEditTool_Operator_At_Most_TT"),
        TimeActMassEditOperator.ResetToDefault => LOC.Get("TAE_MassEditTool_Operator_Reset_TT"),
        TimeActMassEditOperator.Toggle => LOC.Get("TAE_MassEditTool_Operator_Toggle_TT"),
        TimeActMassEditOperator.Shift => LOC.Get("TAE_MassEditTool_Operator_Shift_TT"),
        TimeActMassEditOperator.Scale => LOC.Get("TAE_MassEditTool_Operator_Scale_TT"),
        TimeActMassEditOperator.SetLength => LOC.Get("TAE_MassEditTool_Operator_Set_Length_TT"),
        _ => ""
    };
    #endregion

    #region Changes
    /// <summary>
    /// Works the plan out again if the steps or the matches have changed since it was made
    /// </summary>
    private void UpdatePlan(TAE.Template.BankTemplate template)
    {
        if (HasApplied)
            return;

        if (Plan != null && PlanStepsVersion == StepsVersion && PlanFor == Results && PlanForCount == Results.Count)
            return;

        var matches = Results.Select(e => e.Match).ToList();
        var steps = Steps.Select(e => e.ToStep()).ToList();

        Plan = Planner.Plan(matches, steps, template, ReadableCache);
        PlanStepsVersion = StepsVersion;
        PlanFor = Results;
        PlanForCount = Results.Count;

        ChangeLabels = new string[Plan.Changes.Count];
        FilteredChanges = null;
    }

    private void DisplayChanges(TAE.Template.BankTemplate template)
    {
        // Changes
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("TAE_MassEditTool_Changes_Header"),
            LOC.Get("TAE_MassEditTool_Changes_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Plan == null || !Steps.Any(e => e.Field != ""))
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Changes_No_Steps"));
            return;
        }

        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Changes_Summary", Plan.Changes.Count, Plan.Events.Count, Plan.AnimationCount));

        if (Plan.Unchanged > 0)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Changes_Unchanged", Plan.Unchanged));
        }

        DisplaySkips(Plan.Skips, template);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Changes_Stale"));
        }

        // Apply
        var apply = false;

        GUI.ConditionalMultiButtonInput("massEditApplyActions",
            "massEditApply",
            LOC.Get("TAE_MassEditTool_Apply"),
            LOC.Get("TAE_MassEditTool_Apply_TT"),
            () => apply = true,
            !stale && Plan.Events.Count > 0);

        // Applying drops the plan, so the rest of the section, which draws the plan, waits for the next frame
        if (apply)
        {
            ApplyPlan(template);
            return;
        }

        // The summary, skips and Apply stay on show when the list is hidden
        if (Plan.Changes.Count == 0 || !ShowChangesList)
            return;

        EditorFilters.DisplayFramedListFilter("massEditChangeFilter", ref ChangeFilter, ref ExactChangeFilter);

        var filtered = GetFilteredChanges(template);

        ImGui.BeginChild("##massEditChanges", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var change = Plan.Changes[index];

                if (ImGui.Button($"{Icons.LocationArrow}##massEditChangeJump{index}"))
                {
                    jumpTarget = change.Event.Match;
                }
                GUI.Tooltip(LOC.Get("TAE_MassEditTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{GetChangeLabel(index, template)}##massEditChange{index}",
                    View.Selection.IsEventSelected(change.Event.Match.Event)))
                {
                    jumpTarget = change.Event.Match;
                }
            }
        }

        clipper.End();

        ImGui.EndChild();

        if (jumpTarget != null)
        {
            Runner.Jump(jumpTarget);
        }
    }

    // Skipped matches are only listed up to this many under each reason
    private const int MaxSkipsShown = 200;

    private void DisplaySkips(List<TimeActPlannedSkip> allSkips, TAE.Template.BankTemplate template)
    {
        if (allSkips.Count == 0)
            return;

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Changes_Skipped", allSkips.Count));

        var reasonIndex = 0;

        foreach (var group in allSkips.GroupBy(e => e.Reason))
        {
            var skips = group.ToList();

            if (ImGui.TreeNodeEx($"{group.Key} ({skips.Count})##massEditSkip{reasonIndex}"))
            {
                for (int i = 0; i < Math.Min(skips.Count, MaxSkipsShown); i++)
                {
                    var match = skips[i].Match;

                    if (ImGui.Selectable($"{GetMatchLabel(match, template)}##massEditSkip{reasonIndex}_{i}"))
                    {
                        Runner.Jump(match);
                    }
                }

                if (skips.Count > MaxSkipsShown)
                {
                    ImGui.TextDisabled(LOC.Get("TAE_MassEditTool_Changes_Skipped_More", skips.Count - MaxSkipsShown));
                }

                ImGui.TreePop();
            }

            reasonIndex++;
        }
    }

    private string GetMatchLabel(TimeActQueryMatch match, TAE.Template.BankTemplate template)
    {
        var animationName = match.Source.GetAnimationName(match.Entry, match.Animation);
        var label = match.Event != null
            ? $"{animationName}  {TimeActQueryMatcher.GetEventName(match.Event, template)}"
            : animationName;

        return ShowTimeActInLabel ? $"{match.EntryName}  {label}" : label;
    }

    private string GetChangeLabel(int index, TAE.Template.BankTemplate template)
    {
        if (ChangeLabels[index] != null)
            return ChangeLabels[index];

        var change = Plan.Changes[index];
        var label = $"{GetMatchLabel(change.Event.Match, template)}  {change.Field}: {change.OldText} -> {change.NewText}";

        ChangeLabels[index] = label;

        return label;
    }

    private List<int> GetFilteredChanges(TAE.Template.BankTemplate template)
    {
        if (FilteredChanges != null && FilteredChangesFor == Plan &&
            CachedChangeFilter == ChangeFilter && CachedExactChangeFilter == ExactChangeFilter)
        {
            return FilteredChanges;
        }

        var filtered = new List<int>();

        for (int i = 0; i < Plan.Changes.Count; i++)
        {
            // The labels are only made for every change once the filter needs them
            if (ChangeFilter == "" || EditorFilters.IsMatch(ChangeFilter, GetChangeLabel(i, template), ExactChangeFilter))
            {
                filtered.Add(i);
            }
        }

        FilteredChanges = filtered;
        FilteredChangesFor = Plan;
        CachedChangeFilter = ChangeFilter;
        CachedExactChangeFilter = ExactChangeFilter;

        return filtered;
    }

    private void ApplyPlan(TAE.Template.BankTemplate template)
    {
        if (Plan == null || IsStale())
            return;

        var result = TimeActMassEditApplier.Apply(View, Plan, template);

        Smithbox.Log(this, LOC.Get("TAE_MassEditTool_Applied_Log", result.Applied, result.Stale));

        // The values the plan was read from are now out of date, so it is dropped until the next Find
        Plan = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = result;
    }
    #endregion
}
