using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The Edit tab of the mass edit: edit, delete or duplicate the matches. Each works out what it will do first and
/// shows it, then applies it as one action that a single undo takes back.
/// </summary>
public partial class GparamMassEditTool
{
    /// <summary>
    /// A step as it is being edited. It is copied into a <see cref="GparamMassEditStep"/> when the plan is made.
    /// </summary>
    private class StepInput
    {
        public GparamMassEditField? Field = null;
        public GparamMassEditOperator Operator = GparamMassEditOperator.Set;
        public string Value = "";

        public GparamMassEditStep ToStep() => new(Field, Operator, Value.Trim());
    }

    private record ApplyOutcome(GparamMassEditAction Action, GparamMassEditTarget Target, int Applied, int Stale);

    private GparamMassEditAction Action = GparamMassEditAction.Edit;
    private List<StepInput> Steps = [new()];
    private int StepsVersion;

    private readonly GparamMassEditPlanner Planner;

    private GparamMassEditPlan Plan;
    private int PlanStepsVersion = -1;
    private List<ResultRow> PlanFor;
    private int PlanForCount;

    private string DuplicateOffsetText = "1";
    private GparamMassDuplicatePlan DuplicatePlan;
    private int DuplicatePlanVersion = -1;
    private List<ResultRow> DuplicatePlanFor;
    private int DuplicatePlanForCount;

    // The choices offered for Set on the value, worked out once the matches are in
    private List<(string Label, string Value)> ValueChoices;
    private List<ResultRow> ValueChoicesFor;
    private int ValueChoicesForCount;

    // The label of each change, made the first time it is drawn or filtered
    private string[] ChangeLabels;

    // Toggled by the eye on the list's header
    private bool ShowChangesList = true;

    private string ChangeFilter = "";
    private bool ExactChangeFilter = false;
    private List<int> FilteredChanges;
    private object FilteredChangesFor;
    private string CachedChangeFilter = "";
    private bool CachedExactChangeFilter = false;

    // Set once something has been applied, so it can't be applied a second time on top of itself
    private ApplyOutcome AppliedResult;

    private const string DeleteConfirmId = "###gparamMassEditConfirmDelete";

    // Skipped matches are only listed up to this many under each reason
    private const int MaxSkipsShown = 200;

    private void StepsChanged()
    {
        StepsVersion++;
    }

    private void ClearPlan()
    {
        Plan = null;
        PlanFor = null;
        PlanStepsVersion = -1;
        DuplicatePlan = null;
        DuplicatePlanFor = null;
        DuplicatePlanVersion = -1;
        ValueChoices = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = null;
    }

    private void DisplayEdit()
    {
        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Edit_No_Matches"));
            return;
        }

        if (Run is { Done: false })
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Edit_Finding", Results.Count));
            return;
        }

        // What the edit works on, as the matches are in the other tab
        GUI.WrappedText(GetMatchCountText());

        // Only value rows can be edited or duplicated
        if (ResultTarget is not GparamMassEditTarget.Values)
        {
            Action = GparamMassEditAction.Delete;
        }

        // Action
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Action_Header"),
            LOC.Get("GPARAM_MassEditTool_Action_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##gparamMassEditAction", GetActionName(Action)))
        {
            foreach (var action in Enum.GetValues<GparamMassEditAction>())
            {
                if (ResultTarget is not GparamMassEditTarget.Values && action is not GparamMassEditAction.Delete)
                    continue;

                if (ImGui.Selectable($"{GetActionName(action)}##gparamMassEditAction{action}", Action == action))
                {
                    Action = action;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Action_Header_TT"));

        switch (Action)
        {
            case GparamMassEditAction.Delete:
                DisplayDelete();
                return;

            case GparamMassEditAction.Duplicate:
                DisplayDuplicate();
                return;
        }

        if (Results.Count == 0)
            return;

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Steps_Header"),
            LOC.Get("GPARAM_MassEditTool_Steps_Header_TT"));

        DisplaySteps();

        UpdatePlan();

        DisplayChanges();
    }

    private static string GetActionName(GparamMassEditAction action) => action switch
    {
        GparamMassEditAction.Edit => LOC.Get("GPARAM_MassEditTool_Action_Edit"),
        GparamMassEditAction.Delete => LOC.Get("GPARAM_MassEditTool_Action_Delete"),
        GparamMassEditAction.Duplicate => LOC.Get("GPARAM_MassEditTool_Action_Duplicate"),
        _ => $"{action}"
    };

    /// <summary>
    /// Shows what the last apply did. Returns true if anything has been applied since Find, as the matches no
    /// longer show what is there.
    /// </summary>
    private bool DisplayAppliedResult()
    {
        if (AppliedResult == null)
            return false;

        var (action, target, applied, stale) = AppliedResult;

        GUI.WrappedText(action switch
        {
            GparamMassEditAction.Edit => LOC.Get("GPARAM_MassEditTool_Applied", applied),
            GparamMassEditAction.Duplicate => LOC.Get("GPARAM_MassEditTool_Duplicated", applied),
            _ => target switch
            {
                GparamMassEditTarget.Values => LOC.Get("GPARAM_MassEditTool_Deleted_Values", applied),
                GparamMassEditTarget.Fields => LOC.Get("GPARAM_MassEditTool_Deleted_Fields", applied),
                _ => LOC.Get("GPARAM_MassEditTool_Deleted_Groups", applied)
            }
        });

        if (stale > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Applied_Stale", stale));
        }

        // Save only writes the selected file, so an edit across files needs the menu's Save All
        if (ResultsGlobal)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("GPARAM_MassEditTool_Applied_Save_All", LOC.Get("EDITOR_Menubar_Action_SaveAll")));
        }

        GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Applied_Find_Again"));

        return true;
    }

    private void Applied(GparamMassEditAction action, GparamMassEditApplier.Result result)
    {
        Smithbox.Log(this, LOC.Get("GPARAM_MassEditTool_Applied_Log", result.Applied, result.Stale));

        // The values the plan was read from are now out of date, so it is dropped until the next Find
        Plan = null;
        DuplicatePlan = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = new ApplyOutcome(action, ResultTarget, result.Applied, result.Stale);
    }

    #region Steps
    private static readonly GparamMassEditField[] AllFields = Enum.GetValues<GparamMassEditField>();

    /// <summary>
    /// The rows of steps: [field] [-], then [operator] [value]
    /// </summary>
    private void DisplaySteps()
    {
        StepInput removed = null;

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            // Field
            var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(-removeWidth);

            var preview = step.Field != null ? GetFieldName(step.Field.Value) : LOC.Get("GPARAM_MassEditTool_Step_Pick_Field");

            if (ImGui.BeginCombo($"##gparamMassEditStepField{i}", preview))
            {
                foreach (var field in AllFields)
                {
                    if (ImGui.Selectable($"{GetFieldName(field)}##gparamMassEditStepField{i}_{field}", step.Field == field))
                    {
                        if (step.Field != field)
                        {
                            step.Field = field;
                            step.Value = "";

                            var operators = GparamMassEditStep.GetOperators(field);

                            if (!operators.Contains(step.Operator))
                            {
                                step.Operator = operators[0];
                            }

                            StepsChanged();
                        }
                    }
                    GUI.Tooltip(GetFieldTooltip(field));
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Step_Field_TT"));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##gparamMassEditStepRemove{i}", DPI.IconButtonSize))
            {
                removed = step;
            }
            GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Step_Remove_TT"));

            if (step.Field != null)
            {
                DisplayStepOperator(step, i);
            }
        }

        if (removed != null)
        {
            Steps.Remove(removed);
            StepsChanged();
        }

        GUI.MultiButtonInput("gparamMassEditStepActions",
            "gparamMassEditStepAdd",
            LOC.Get("GPARAM_MassEditTool_Step_Add"),
            LOC.Get("GPARAM_MassEditTool_Step_Add_TT"),
            () =>
            {
                Steps.Add(new());
                StepsChanged();
            });
    }

    private void DisplayStepOperator(StepInput step, int index)
    {
        var operators = GparamMassEditStep.GetOperators(step.Field.Value);

        if (!operators.Contains(step.Operator))
        {
            step.Operator = operators[0];
            StepsChanged();
        }

        var takesValue = GparamMassEditStep.TakesValue(step.Operator);

        ImGui.SetNextItemWidth(takesValue ? ImGui.GetContentRegionAvail().X * 0.35f : -1);

        if (ImGui.BeginCombo($"##gparamMassEditStepOperator{index}", GetOperatorName(step.Operator)))
        {
            foreach (var op in operators)
            {
                if (ImGui.Selectable($"{GetOperatorName(op)}##gparamMassEditStepOperator{index}_{op}", step.Operator == op))
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

        var choices = step.Operator is GparamMassEditOperator.Set && step.Field is GparamMassEditField.Value
            ? GetValueChoices()
            : null;

        if (choices != null)
        {
            DisplayStepChoices(step, index, choices);
            return;
        }

        ImGui.SetNextItemWidth(-1);

        var hint = step.Operator is GparamMassEditOperator.MultiplyBy or GparamMassEditOperator.DivideBy
            ? LOC.Get("GPARAM_MassEditTool_Step_Factor_Hint")
            : LOC.Get("GPARAM_MassEditTool_Step_Value_Hint");

        if (ImGui.InputTextWithHint($"##gparamMassEditStepValue{index}", hint, ref step.Value, 255))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get(step.Field is GparamMassEditField.Value
            ? "GPARAM_MassEditTool_Step_Value_TT"
            : "GPARAM_MassEditTool_Step_Number_TT"));
    }

    /// <summary>
    /// The values Set can pick from a list: on and off when every matched field is a bool, or the options when every
    /// matched field has the same enum. Null to type the value.
    /// </summary>
    private List<(string Label, string Value)> GetValueChoices()
    {
        if (ValueChoicesFor == Results && ValueChoicesForCount == Results.Count)
            return ValueChoices;

        ValueChoicesFor = Results;
        ValueChoicesForCount = Results.Count;
        ValueChoices = null;

        var fields = Results.Select(e => e.Match).Where(e => e.Field != null)
            .Select(e => (e.GroupKey, e.Field)).Distinct().ToList();

        if (fields.Count == 0)
            return null;

        if (fields.All(e => e.Field is GPARAM.BoolField))
        {
            ValueChoices = [(LOC.Get("GPARAM_MassEditTool_Value_True"), "True"), (LOC.Get("GPARAM_MassEditTool_Value_False"), "False")];
            return ValueChoices;
        }

        var enums = fields.Select(e => GparamMetaUtils.GetFieldEnum(Project, e.GroupKey, e.Field.Key)).Distinct().ToList();

        if (enums.Count == 1 && !string.IsNullOrEmpty(enums[0]))
        {
            var matcher = new GparamQueryMatcher(Project);
            var options = matcher.GetEnumOptions(fields[0].GroupKey, fields[0].Field.Key);

            if (options is { Count: > 0 })
            {
                ValueChoices = options.Select(e => ($"{e.Name} ({e.Value})", e.Value)).ToList();
            }
        }

        return ValueChoices;
    }

    private void DisplayStepChoices(StepInput step, int index, List<(string Label, string Value)> choices)
    {
        var preview = step.Value == ""
            ? LOC.Get("GPARAM_MassEditTool_Step_Pick_Value")
            : choices.FirstOrDefault(e => e.Value == step.Value).Label ?? step.Value;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##gparamMassEditStepChoice{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            for (int i = 0; i < choices.Count; i++)
            {
                var (label, value) = choices[i];

                if (ImGui.Selectable($"{label}##gparamMassEditStepChoice{index}_{i}", step.Value == value))
                {
                    step.Value = value;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Step_Value_TT"));
    }

    private static string GetFieldName(GparamMassEditField field) => field switch
    {
        GparamMassEditField.Value => LOC.Get("GPARAM_MassEditTool_Field_Value"),
        GparamMassEditField.Component1 => LOC.Get("GPARAM_MassEditTool_Field_Component_1"),
        GparamMassEditField.Component2 => LOC.Get("GPARAM_MassEditTool_Field_Component_2"),
        GparamMassEditField.Component3 => LOC.Get("GPARAM_MassEditTool_Field_Component_3"),
        GparamMassEditField.Component4 => LOC.Get("GPARAM_MassEditTool_Field_Component_4"),
        GparamMassEditField.ID => LOC.Get("GPARAM_MassEditTool_Field_ID"),
        GparamMassEditField.TimeOfDay => LOC.Get("GPARAM_MassEditTool_Field_Time_Of_Day"),
        _ => $"{field}"
    };

    private static string GetFieldTooltip(GparamMassEditField field) => field switch
    {
        GparamMassEditField.Value => LOC.Get("GPARAM_MassEditTool_Field_Value_TT"),
        GparamMassEditField.ID => LOC.Get("GPARAM_MassEditTool_Field_ID_TT"),
        GparamMassEditField.TimeOfDay => LOC.Get("GPARAM_MassEditTool_Field_Time_Of_Day_TT"),
        _ => LOC.Get("GPARAM_MassEditTool_Field_Component_TT")
    };

    private static string GetOperatorName(GparamMassEditOperator op) => op switch
    {
        GparamMassEditOperator.Set => LOC.Get("GPARAM_MassEditTool_Operator_Set"),
        GparamMassEditOperator.Add => LOC.Get("GPARAM_MassEditTool_Operator_Add"),
        GparamMassEditOperator.Subtract => LOC.Get("GPARAM_MassEditTool_Operator_Subtract"),
        GparamMassEditOperator.MultiplyBy => LOC.Get("GPARAM_MassEditTool_Operator_Multiply"),
        GparamMassEditOperator.DivideBy => LOC.Get("GPARAM_MassEditTool_Operator_Divide"),
        GparamMassEditOperator.AtLeast => LOC.Get("GPARAM_MassEditTool_Operator_At_Least"),
        GparamMassEditOperator.AtMost => LOC.Get("GPARAM_MassEditTool_Operator_At_Most"),
        GparamMassEditOperator.Toggle => LOC.Get("GPARAM_MassEditTool_Operator_Toggle"),
        _ => $"{op}"
    };

    private static string GetOperatorTooltip(GparamMassEditOperator op) => op switch
    {
        GparamMassEditOperator.Set => LOC.Get("GPARAM_MassEditTool_Operator_Set_TT"),
        GparamMassEditOperator.Add => LOC.Get("GPARAM_MassEditTool_Operator_Add_TT"),
        GparamMassEditOperator.Subtract => LOC.Get("GPARAM_MassEditTool_Operator_Subtract_TT"),
        GparamMassEditOperator.MultiplyBy => LOC.Get("GPARAM_MassEditTool_Operator_Multiply_TT"),
        GparamMassEditOperator.DivideBy => LOC.Get("GPARAM_MassEditTool_Operator_Divide_TT"),
        GparamMassEditOperator.AtLeast => LOC.Get("GPARAM_MassEditTool_Operator_At_Least_TT"),
        GparamMassEditOperator.AtMost => LOC.Get("GPARAM_MassEditTool_Operator_At_Most_TT"),
        GparamMassEditOperator.Toggle => LOC.Get("GPARAM_MassEditTool_Operator_Toggle_TT"),
        _ => ""
    };
    #endregion

    #region Changes
    /// <summary>
    /// Works the plan out again if the steps or the matches have changed since it was made
    /// </summary>
    private void UpdatePlan()
    {
        if (AppliedResult != null)
            return;

        if (Plan != null && PlanStepsVersion == StepsVersion && PlanFor == Results && PlanForCount == Results.Count)
            return;

        var matches = Results.Select(e => e.Match).ToList();
        var steps = Steps.Select(e => e.ToStep()).ToList();

        Plan = Planner.Plan(matches, steps);
        PlanStepsVersion = StepsVersion;
        PlanFor = Results;
        PlanForCount = Results.Count;

        ChangeLabels = new string[Plan.Changes.Count];
        FilteredChanges = null;
    }

    private void DisplayChanges()
    {
        // Changes
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("GPARAM_MassEditTool_Changes_Header"),
            LOC.Get("GPARAM_MassEditTool_Changes_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Plan == null || !Steps.Any(e => e.Field != null))
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Changes_No_Steps"));
            return;
        }

        GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Changes_Summary", Plan.Changes.Count, Plan.Values.Count, Plan.FieldCount));

        if (Plan.Unchanged > 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Changes_Unchanged", Plan.Unchanged));
        }

        DisplaySkips(Plan.Skips);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Changes_Stale"));
        }

        // Apply
        var apply = false;

        GUI.ConditionalMultiButtonInput("gparamMassEditApplyActions",
            "gparamMassEditApply",
            LOC.Get("GPARAM_MassEditTool_Apply"),
            LOC.Get("GPARAM_MassEditTool_Apply_TT"),
            () => apply = true,
            !stale && Plan.Values.Count > 0);

        // Applying drops the plan, so the rest of the section, which draws the plan, waits for the next frame
        if (apply)
        {
            Applied(GparamMassEditAction.Edit, GparamMassEditApplier.ApplyEdit(View, Project, Plan));
            return;
        }

        // The summary, skips and Apply stay on show when the list is hidden
        if (Plan.Changes.Count == 0 || !ShowChangesList)
            return;

        var plan = Plan;

        DisplayPreviewList("gparamMassEditChanges", plan, plan.Changes.Count,
            i => plan.Changes[i].Match,
            i => ChangeLabels[i] ??= $"{GetMatchLabel(plan.Changes[i].Match)}  {plan.Changes[i].Field}: {plan.Changes[i].OldText} -> {plan.Changes[i].NewText}");
    }

    /// <summary>
    /// A filterable list of what an action will do, each row jumping to its match
    /// </summary>
    private void DisplayPreviewList(string id, object source, int count, Func<int, GparamQueryMatch> matchAt, Func<int, string> labelAt)
    {
        EditorFilters.DisplayFramedListFilter($"{id}Filter", ref ChangeFilter, ref ExactChangeFilter);

        var filtered = GetFilteredChanges(source, count, labelAt);

        ImGui.BeginChild($"##{id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        GparamQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var match = matchAt(index);

                if (ImGui.Button($"{Icons.LocationArrow}##{id}Jump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("GPARAM_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{labelAt(index)}##{id}{index}", IsSelected(match)))
                {
                    jumpTarget = match;
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

    private void DisplaySkips(List<GparamPlannedSkip> allSkips)
    {
        if (allSkips.Count == 0)
            return;

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Changes_Skipped", allSkips.Count));

        var reasonIndex = 0;

        foreach (var group in allSkips.GroupBy(e => e.Reason))
        {
            var skips = group.ToList();

            if (ImGui.TreeNodeEx($"{group.Key} ({skips.Count})##gparamMassEditSkip{reasonIndex}"))
            {
                for (int i = 0; i < Math.Min(skips.Count, MaxSkipsShown); i++)
                {
                    var match = skips[i].Match;

                    if (ImGui.Selectable($"{GetMatchLabel(match)}##gparamMassEditSkip{reasonIndex}_{i}"))
                    {
                        Runner.Jump(match);
                    }
                }

                if (skips.Count > MaxSkipsShown)
                {
                    ImGui.TextDisabled(LOC.Get("GPARAM_MassEditTool_Changes_Skipped_More", skips.Count - MaxSkipsShown));
                }

                ImGui.TreePop();
            }

            reasonIndex++;
        }
    }

    /// <summary>
    /// The match as it is listed, without what it was found by: group, field and ID
    /// </summary>
    private string GetMatchLabel(GparamQueryMatch match)
    {
        var label = match.Label;

        // A value's label ends with its value, which the change shows itself
        if (match.ValueIndex >= 0)
        {
            var marker = $"[{match.ValueID}]";
            var at = label.LastIndexOf($"{marker}  = ", StringComparison.Ordinal);

            if (at >= 0)
            {
                label = label[..(at + marker.Length)];
            }
        }

        return PrefixLabel(match, label);
    }

    private List<int> GetFilteredChanges(object source, int count, Func<int, string> labelAt)
    {
        if (FilteredChanges != null && FilteredChangesFor == source &&
            CachedChangeFilter == ChangeFilter && CachedExactChangeFilter == ExactChangeFilter)
        {
            return FilteredChanges;
        }

        var filtered = new List<int>();

        for (int i = 0; i < count; i++)
        {
            // The labels are only made for every row once the filter needs them
            if (ChangeFilter == "" || EditorFilters.IsMatch(ChangeFilter, labelAt(i), ExactChangeFilter))
            {
                filtered.Add(i);
            }
        }

        FilteredChanges = filtered;
        FilteredChangesFor = source;
        CachedChangeFilter = ChangeFilter;
        CachedExactChangeFilter = ExactChangeFilter;

        return filtered;
    }
    #endregion

    #region Delete
    private void DisplayDelete()
    {
        // Deletions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Deletions_Header"),
            LOC.Get("GPARAM_MassEditTool_Deletions_Header_TT"));

        if (DisplayAppliedResult())
            return;

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Delete_Nothing"));
            return;
        }

        GUI.WrappedText(ResultTarget switch
        {
            GparamMassEditTarget.Values => LOC.Get("GPARAM_MassEditTool_Delete_Summary_Values", Results.Count, ResultFields.Count),
            GparamMassEditTarget.Fields => LOC.Get("GPARAM_MassEditTool_Delete_Summary_Fields", Results.Count),
            _ => LOC.Get("GPARAM_MassEditTool_Delete_Summary_Groups", Results.Count)
        });

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Changes_Stale"));
        }

        // Delete
        var delete = false;

        GUI.ConditionalMultiButtonInput("gparamMassEditDeleteActions",
            "gparamMassEditDelete",
            LOC.Get("GPARAM_MassEditTool_Delete"),
            LOC.Get("GPARAM_MassEditTool_Delete_TT"),
            () => delete = true,
            !stale);

        // Fields and groups take their values with them, so their deletion is confirmed first. The popup is opened
        // here rather than in the button, so it is opened and drawn under the same ID.
        if (delete)
        {
            if (ResultTarget is GparamMassEditTarget.Values)
            {
                ApplyDelete();
                return;
            }

            ImGui.OpenPopup($"{LOC.Get("GPARAM_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}");
        }

        if (DisplayDeleteConfirm())
            return;
    }

    /// <summary>
    /// Returns true if the deletion was confirmed and applied
    /// </summary>
    private bool DisplayDeleteConfirm()
    {
        var confirmed = false;
        var open = true;

        if (ImGui.BeginPopupModal($"{LOC.Get("GPARAM_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}",
            ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            GUI.WrappedText(LOC.Get(ResultTarget is GparamMassEditTarget.Fields
                ? "GPARAM_MassEditTool_Delete_Confirm_Fields"
                : "GPARAM_MassEditTool_Delete_Confirm_Groups", Results.Count));

            var cancelled = false;

            GUI.MultiButtonInput("gparamMassEditDeleteConfirmActions",
                "gparamMassEditDeleteConfirm",
                LOC.Get("GPARAM_MassEditTool_Delete"),
                LOC.Get("GPARAM_MassEditTool_Delete_TT"),
                () => confirmed = true,

                "gparamMassEditDeleteCancel",
                LOC.Get("GPARAM_MassEditTool_Delete_Cancel"),
                LOC.Get("GPARAM_MassEditTool_Delete_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (confirmed)
        {
            ApplyDelete();
            return true;
        }

        return false;
    }

    private void ApplyDelete()
    {
        if (IsStale())
            return;

        var matches = Results.Select(e => e.Match).ToList();

        Applied(GparamMassEditAction.Delete, GparamMassEditApplier.ApplyDelete(View, Project, matches, ResultTarget));
    }
    #endregion

    #region Duplicate
    private void DisplayDuplicate()
    {
        // Offset
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("GPARAM_MassEditTool_Duplicate_Offset_Header"),
            LOC.Get("GPARAM_MassEditTool_Duplicate_Offset_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint("##gparamMassEditDuplicateOffset", LOC.Get("GPARAM_MassEditTool_Duplicate_Offset_Hint"),
            ref DuplicateOffsetText, 32))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get("GPARAM_MassEditTool_Duplicate_Offset_Header_TT"));

        UpdateDuplicatePlan();

        // Duplicates
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("GPARAM_MassEditTool_Duplicates_Header"),
            LOC.Get("GPARAM_MassEditTool_Duplicates_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (DuplicatePlan == null)
            return;

        if (DuplicatePlan.InvalidOffset)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Duplicate_Invalid_Offset"));
            return;
        }

        GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Duplicate_Summary", DuplicatePlan.Duplicates.Count));

        var moved = DuplicatePlan.Duplicates.Count(e => e.NewID != e.Match.ValueID + DuplicatePlan.Offset);

        if (moved > 0)
        {
            GUI.WrappedText(LOC.Get("GPARAM_MassEditTool_Duplicate_Moved", moved));
        }

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_MassEditTool_Changes_Stale"));
        }

        var apply = false;

        GUI.ConditionalMultiButtonInput("gparamMassEditDuplicateActions",
            "gparamMassEditDuplicate",
            LOC.Get("GPARAM_MassEditTool_Duplicate"),
            LOC.Get("GPARAM_MassEditTool_Duplicate_TT"),
            () => apply = true,
            !stale && DuplicatePlan.Duplicates.Count > 0);

        if (apply)
        {
            Applied(GparamMassEditAction.Duplicate, GparamMassEditApplier.ApplyDuplicate(View, Project, DuplicatePlan));
            return;
        }

        if (DuplicatePlan.Duplicates.Count == 0 || !ShowChangesList)
            return;

        var plan = DuplicatePlan;

        DisplayPreviewList("gparamMassEditDuplicates", plan, plan.Duplicates.Count,
            i => plan.Duplicates[i].Match,
            i => $"{GetMatchLabel(plan.Duplicates[i].Match)}  -> [{plan.Duplicates[i].NewID}]");
    }

    private void UpdateDuplicatePlan()
    {
        if (AppliedResult != null)
            return;

        if (DuplicatePlan != null && DuplicatePlanVersion == StepsVersion &&
            DuplicatePlanFor == Results && DuplicatePlanForCount == Results.Count)
            return;

        DuplicatePlan = Planner.PlanDuplicates(Results.Select(e => e.Match).ToList(), DuplicateOffsetText);
        DuplicatePlanVersion = StepsVersion;
        DuplicatePlanFor = Results;
        DuplicatePlanForCount = Results.Count;

        FilteredChanges = null;
    }
    #endregion
}
