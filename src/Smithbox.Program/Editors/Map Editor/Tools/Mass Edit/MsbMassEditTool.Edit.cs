using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Edit tab of the mass edit: edit or delete the matches. Each works out what it will do first and shows it,
/// then applies it: one undoable action for the loaded maps, and a direct write for each map that isn't loaded.
/// </summary>
public partial class MsbMassEditTool
{
    /// <summary>
    /// A step as it is being edited. It is copied into a <see cref="MsbMassEditStep"/> when the plan is made.
    /// </summary>
    private class StepInput
    {
        public string Field = null;
        public MsbMassEditOperator Operator = MsbMassEditOperator.Set;
        public string Value = "";
        public string Value2 = "";
        public string FieldFilter = "";

        public MsbMassEditStep ToStep() => new(Field, Operator, Value.Trim(), Value2);
    }

    private enum PendingConfirm
    {
        None,
        Edit,
        Delete
    }

    private MsbMassEditAction Action = MsbMassEditAction.Edit;
    private List<StepInput> Steps = [new()];
    private int StepsVersion;

    private readonly MsbMassEditPlanner Planner;

    private MsbMassEditPlan Plan;
    private int PlanStepsVersion = -1;
    private List<ResultRow> PlanFor;
    private int PlanForCount;

    // The fields of the matches, worked out once the matches are in
    private List<MsbMassEditFieldInfo> FieldInfos;
    private Dictionary<string, MsbMassEditFieldInfo> FieldInfosByPath;
    private List<ResultRow> FieldInfosFor;
    private int FieldInfosForCount;

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
    private MsbMassEditApplier.Result AppliedResult;
    private MsbMassEditAction AppliedAction;

    // A confirm asked for by a button, opened where the popups are drawn
    private PendingConfirm OpenConfirm = PendingConfirm.None;
    private List<string> ConfirmUnloadedMaps = new();

    private const string EditConfirmId = "###msbMassEditConfirmEdit";
    private const string DeleteConfirmId = "###msbMassEditConfirmDelete";

    // Skipped objects are only listed up to this many under each reason
    private const int MaxSkipsShown = 200;

    // Unloaded maps are only named up to this many in a confirm
    private const int MaxMapsNamed = 20;

    private void StepsChanged()
    {
        StepsVersion++;
    }

    private void ClearPlan()
    {
        Plan = null;
        PlanFor = null;
        PlanStepsVersion = -1;
        FieldInfos = null;
        FieldInfosByPath = null;
        FieldInfosFor = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = null;
    }

    private void DisplayEdit()
    {
        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Edit_No_Matches"));
            return;
        }

        if (Run is { Done: false })
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Edit_Finding", Results.Count));
            return;
        }

        // What the edit works on, as the matches are in the other tab
        GUI.WrappedText(GetMatchCountText());

        // Action
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_MassEditTool_Action_Header"),
            LOC.Get("MAP_MassEditTool_Action_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##msbMassEditAction", GetActionName(Action)))
        {
            foreach (var action in Enum.GetValues<MsbMassEditAction>())
            {
                if (ImGui.Selectable($"{GetActionName(action)}##msbMassEditAction{action}", Action == action))
                {
                    Action = action;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_MassEditTool_Action_Header_TT"));

        if (Action is MsbMassEditAction.Delete)
        {
            DisplayDelete();
            return;
        }

        if (Results.Count == 0)
            return;

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_MassEditTool_Steps_Header"),
            LOC.Get("MAP_MassEditTool_Steps_Header_TT"));

        DisplaySteps();

        UpdatePlan();

        DisplayChanges();
    }

    private static string GetActionName(MsbMassEditAction action) => action switch
    {
        MsbMassEditAction.Edit => LOC.Get("MAP_MassEditTool_Action_Edit"),
        MsbMassEditAction.Delete => LOC.Get("MAP_MassEditTool_Action_Delete"),
        _ => $"{action}"
    };

    /// <summary>
    /// Shows what the last apply did. Returns true if anything has been applied since Find, as the matches no
    /// longer show what is there.
    /// </summary>
    private bool DisplayAppliedResult()
    {
        var result = AppliedResult;

        if (result == null)
            return false;

        GUI.WrappedText(LOC.Get(AppliedAction is MsbMassEditAction.Delete
            ? "MAP_MassEditTool_Deleted" : "MAP_MassEditTool_Applied", result.Applied));

        if (result.Stale > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Applied_Stale", result.Stale));
        }

        if (result.OtherView > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Applied_Other_View", result.OtherView));
        }

        if (result.Written.Count > 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Applied_Written", result.Written.Count));
        }

        foreach (var (mapID, error) in result.Failed)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Applied_Failed", mapID, error));
        }

        // Edits to loaded maps are kept until the maps are saved
        if (result.Applied > result.Written.Count)
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Applied_Save"));
        }

        GUI.WrappedText(LOC.Get("MAP_MassEditTool_Applied_Find_Again"));

        return true;
    }

    private void Applied(MsbMassEditAction action, MsbMassEditApplier.Result result)
    {
        Smithbox.Log(this, LOC.Get("MAP_MassEditTool_Applied_Log", result.Applied, result.Stale, result.Written.Count,
            result.Failed.Count));

        // The values the plan was read from are now out of date, so it is dropped until the next Find
        Plan = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = result;
        AppliedAction = action;
    }

    /// <summary>
    /// A warning that the maps that aren't loaded are written straight to the project, which can't be undone
    /// </summary>
    private void DisplayUnloadedWarning(IEnumerable<MsbQueryMatch> matches)
    {
        var unloaded = MsbMassEditApplier.GetUnloadedMaps(View, Runner, matches);

        if (unloaded.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Unloaded_Warning", unloaded.Count));
        }
    }

    #region Steps
    /// <summary>
    /// The fields of the matches, worked out again when the matches change
    /// </summary>
    private List<MsbMassEditFieldInfo> GetFieldInfos()
    {
        if (FieldInfos != null && FieldInfosFor == Results && FieldInfosForCount == Results.Count)
            return FieldInfos;

        FieldInfos = Planner.GetFields(Results.Select(e => e.Match).ToList());
        FieldInfosByPath = FieldInfos.ToDictionary(e => e.Path);
        FieldInfosFor = Results;
        FieldInfosForCount = Results.Count;

        return FieldInfos;
    }

    /// <summary>
    /// The rows of steps: [field] [-], then [operator] [value]
    /// </summary>
    private void DisplaySteps()
    {
        var fields = GetFieldInfos();

        StepInput removed = null;

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            DisplayStepField(step, i, fields);

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##msbMassEditStepRemove{i}", DPI.IconButtonSize))
            {
                removed = step;
            }
            GUI.Tooltip(LOC.Get("MAP_MassEditTool_Step_Remove_TT"));

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

        GUI.MultiButtonInput("msbMassEditStepActions",
            "msbMassEditStepAdd",
            LOC.Get("MAP_MassEditTool_Step_Add"),
            LOC.Get("MAP_MassEditTool_Step_Add_TT"),
            () =>
            {
                Steps.Add(new());
                StepsChanged();
            });
    }

    private void DisplayStepField(StepInput step, int index, List<MsbMassEditFieldInfo> fields)
    {
        var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(-removeWidth);

        var preview = step.Field != null && FieldInfosByPath.TryGetValue(step.Field, out var current)
            ? GetFieldLabel(current)
            : step.Field ?? LOC.Get("MAP_MassEditTool_Step_Pick_Field");

        if (ImGui.BeginCombo($"##msbMassEditStepField{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##msbMassEditStepFieldFilter{index}", LOC.Get("MAP_MassEditTool_Step_Field_Filter_Hint"),
                ref step.FieldFilter, 255);

            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere(-1);
            }

            for (int f = 0; f < fields.Count; f++)
            {
                var field = fields[f];

                if (!EditorFilters.IsMatch(step.FieldFilter, field.Path, false, field.DisplayName))
                    continue;

                // A field of a type no step can change is listed, but can't be picked
                var supported = field.Kind is not MsbMassEditFieldKind.Unsupported;

                if (!supported)
                    ImGui.BeginDisabled();

                if (ImGui.Selectable($"{GetFieldLabel(field)}##msbMassEditStepField{index}_{f}", step.Field == field.Path))
                {
                    if (step.Field != field.Path)
                    {
                        step.Field = field.Path;
                        step.Value = "";
                        step.Value2 = "";

                        var operators = MsbMassEditStep.GetOperators(field.Kind);

                        if (!operators.Contains(step.Operator))
                        {
                            step.Operator = operators[0];
                        }

                        StepsChanged();
                    }
                }
                GUI.Tooltip(LOC.Get("MAP_MassEditTool_Step_Field_Type_TT", field.ValueType?.Name ?? "?"));

                if (!supported)
                    ImGui.EndDisabled();
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_MassEditTool_Step_Field_TT"));
    }

    /// <summary>
    /// A field as the step's list shows it: its path, its community name, and how many of the matches have it when
    /// not all do
    /// </summary>
    private string GetFieldLabel(MsbMassEditFieldInfo field)
    {
        var label = field.DisplayName != field.Path && !field.Path.EndsWith(field.DisplayName, StringComparison.Ordinal)
            ? $"{field.Path}  ({field.DisplayName})"
            : field.Path;

        if (field.Count < Results.Count)
        {
            label = $"{label}  {LOC.Get("MAP_MassEditTool_Step_Field_Count", field.Count)}";
        }

        return label;
    }

    private void DisplayStepOperator(StepInput step, int index)
    {
        FieldInfosByPath.TryGetValue(step.Field, out var field);

        var kind = field?.Kind ?? MsbMassEditFieldKind.Unsupported;
        var operators = MsbMassEditStep.GetOperators(kind);

        if (!operators.Contains(step.Operator))
        {
            step.Operator = operators[0];
            StepsChanged();
        }

        var takesValue = MsbMassEditStep.TakesValue(step.Operator);

        ImGui.SetNextItemWidth(takesValue ? ImGui.GetContentRegionAvail().X * 0.3f : -1);

        if (ImGui.BeginCombo($"##msbMassEditStepOperator{index}", GetOperatorName(step.Operator)))
        {
            foreach (var op in operators)
            {
                if (ImGui.Selectable($"{GetOperatorName(op)}##msbMassEditStepOperator{index}_{op}", step.Operator == op))
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

        // Bools and enums are set from a list
        if (step.Operator is MsbMassEditOperator.Set && kind is MsbMassEditFieldKind.Bool or MsbMassEditFieldKind.Enum)
        {
            List<(string Label, string Value)> choices = kind is MsbMassEditFieldKind.Bool
                ? [("True", "True"), ("False", "False")]
                : Enum.GetNames(field.ValueType).Select(e => (e, e)).ToList();

            DisplayStepChoices(step, index, choices);
            return;
        }

        if (MsbMassEditStep.TakesTwoValues(step.Operator))
        {
            var isReplace = step.Operator is MsbMassEditOperator.Replace;
            var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

            ImGui.SetNextItemWidth(width);

            if (ImGui.InputTextWithHint($"##msbMassEditStepValue{index}",
                LOC.Get(isReplace ? "MAP_MassEditTool_Step_Find_Hint" : "MAP_SearchTool_Value_Low_Hint"), ref step.Value, 255))
            {
                StepsChanged();
            }
            GUI.Tooltip(LOC.Get(isReplace ? "MAP_MassEditTool_Step_Replace_TT" : "MAP_MassEditTool_Step_Random_TT"));

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);

            if (ImGui.InputTextWithHint($"##msbMassEditStepValue2{index}",
                LOC.Get(isReplace ? "MAP_MassEditTool_Step_Replace_Hint" : "MAP_SearchTool_Value_High_Hint"), ref step.Value2, 255))
            {
                StepsChanged();
            }
            GUI.Tooltip(LOC.Get(isReplace ? "MAP_MassEditTool_Step_Replace_TT" : "MAP_MassEditTool_Step_Random_TT"));

            return;
        }

        ImGui.SetNextItemWidth(-1);

        var hint = step.Operator is MsbMassEditOperator.MultiplyBy or MsbMassEditOperator.DivideBy
            ? LOC.Get("MAP_MassEditTool_Step_Factor_Hint")
            : LOC.Get("MAP_MassEditTool_Step_Value_Hint");

        if (ImGui.InputTextWithHint($"##msbMassEditStepValue{index}", hint, ref step.Value, 255))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get(kind is MsbMassEditFieldKind.Vector
            ? "MAP_MassEditTool_Step_Vector_TT"
            : "MAP_MassEditTool_Step_Value_TT"));
    }

    private void DisplayStepChoices(StepInput step, int index, List<(string Label, string Value)> choices)
    {
        var preview = step.Value == ""
            ? LOC.Get("MAP_MassEditTool_Step_Pick_Value")
            : choices.FirstOrDefault(e => e.Value == step.Value).Label ?? step.Value;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##msbMassEditStepChoice{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            for (int i = 0; i < choices.Count; i++)
            {
                var (label, value) = choices[i];

                if (ImGui.Selectable($"{label}##msbMassEditStepChoice{index}_{i}", step.Value == value))
                {
                    step.Value = value;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_MassEditTool_Step_Value_TT"));
    }

    private static string GetOperatorName(MsbMassEditOperator op) => op switch
    {
        MsbMassEditOperator.Set => LOC.Get("MAP_MassEditTool_Operator_Set"),
        MsbMassEditOperator.Add => LOC.Get("MAP_MassEditTool_Operator_Add"),
        MsbMassEditOperator.Subtract => LOC.Get("MAP_MassEditTool_Operator_Subtract"),
        MsbMassEditOperator.MultiplyBy => LOC.Get("MAP_MassEditTool_Operator_Multiply"),
        MsbMassEditOperator.DivideBy => LOC.Get("MAP_MassEditTool_Operator_Divide"),
        MsbMassEditOperator.AtLeast => LOC.Get("MAP_MassEditTool_Operator_At_Least"),
        MsbMassEditOperator.AtMost => LOC.Get("MAP_MassEditTool_Operator_At_Most"),
        MsbMassEditOperator.SetRandom => LOC.Get("MAP_MassEditTool_Operator_Set_Random"),
        MsbMassEditOperator.AddRandom => LOC.Get("MAP_MassEditTool_Operator_Add_Random"),
        MsbMassEditOperator.Toggle => LOC.Get("MAP_MassEditTool_Operator_Toggle"),
        MsbMassEditOperator.Prefix => LOC.Get("MAP_MassEditTool_Operator_Prefix"),
        MsbMassEditOperator.Suffix => LOC.Get("MAP_MassEditTool_Operator_Suffix"),
        MsbMassEditOperator.Replace => LOC.Get("MAP_MassEditTool_Operator_Replace"),
        _ => $"{op}"
    };

    private static string GetOperatorTooltip(MsbMassEditOperator op) => op switch
    {
        MsbMassEditOperator.Set => LOC.Get("MAP_MassEditTool_Operator_Set_TT"),
        MsbMassEditOperator.Add => LOC.Get("MAP_MassEditTool_Operator_Add_TT"),
        MsbMassEditOperator.Subtract => LOC.Get("MAP_MassEditTool_Operator_Subtract_TT"),
        MsbMassEditOperator.MultiplyBy => LOC.Get("MAP_MassEditTool_Operator_Multiply_TT"),
        MsbMassEditOperator.DivideBy => LOC.Get("MAP_MassEditTool_Operator_Divide_TT"),
        MsbMassEditOperator.AtLeast => LOC.Get("MAP_MassEditTool_Operator_At_Least_TT"),
        MsbMassEditOperator.AtMost => LOC.Get("MAP_MassEditTool_Operator_At_Most_TT"),
        MsbMassEditOperator.SetRandom => LOC.Get("MAP_MassEditTool_Operator_Set_Random_TT"),
        MsbMassEditOperator.AddRandom => LOC.Get("MAP_MassEditTool_Operator_Add_Random_TT"),
        MsbMassEditOperator.Toggle => LOC.Get("MAP_MassEditTool_Operator_Toggle_TT"),
        MsbMassEditOperator.Prefix => LOC.Get("MAP_MassEditTool_Operator_Prefix_TT"),
        MsbMassEditOperator.Suffix => LOC.Get("MAP_MassEditTool_Operator_Suffix_TT"),
        MsbMassEditOperator.Replace => LOC.Get("MAP_MassEditTool_Operator_Replace_TT"),
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
            LOC.Get("MAP_MassEditTool_Changes_Header"),
            LOC.Get("MAP_MassEditTool_Changes_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Plan == null || !Steps.Any(e => e.Field != null))
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Changes_No_Steps"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_MassEditTool_Changes_Summary", Plan.Changes.Count, Plan.Objects.Count, Plan.MapCount));

        if (Plan.Unchanged > 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Changes_Unchanged", Plan.Unchanged));
        }

        DisplaySkips(Plan.Skips);

        DisplayUnloadedWarning(Plan.Objects);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Changes_Stale"));
        }

        // Apply
        var apply = false;

        GUI.ConditionalMultiButtonInput("msbMassEditApplyActions",
            "msbMassEditApply",
            LOC.Get("MAP_MassEditTool_Apply"),
            LOC.Get("MAP_MassEditTool_Apply_TT"),
            () => apply = true,
            !stale && Plan.Objects.Count > 0);

        // Writing a map that isn't loaded can't be undone, so it is confirmed first
        if (apply)
        {
            var unloaded = MsbMassEditApplier.GetUnloadedMaps(View, Runner, Plan.Objects);

            if (unloaded.Count == 0)
            {
                ApplyEdit();
                return;
            }

            ConfirmUnloadedMaps = unloaded;
            OpenConfirm = PendingConfirm.Edit;
        }

        // The summary, skips and Apply stay on show when the list is hidden
        if (Plan.Changes.Count == 0 || !ShowChangesList)
            return;

        var plan = Plan;

        DisplayPreviewList("msbMassEditChanges", plan, plan.Changes.Count,
            i => plan.Changes[i].Match,
            i => ChangeLabels[i] ??= $"{PrefixLabel(plan.Changes[i].Match, MsbQueryRunner.GetEntryLabel(plan.Changes[i].Match.Entry))}  " +
                $"{plan.Changes[i].Path}: {plan.Changes[i].OldText} -> {plan.Changes[i].NewText}");
    }

    private void ApplyEdit()
    {
        if (Plan == null || IsStale())
            return;

        Applied(MsbMassEditAction.Edit, MsbMassEditApplier.ApplyEdit(View, Project, Runner, Plan));
    }

    /// <summary>
    /// A filterable list of what an action will do, each row jumping to its object
    /// </summary>
    private void DisplayPreviewList(string id, object source, int count, Func<int, MsbQueryMatch> matchAt, Func<int, string> labelAt)
    {
        EditorFilters.DisplayFramedListFilter($"{id}Filter", ref ChangeFilter, ref ExactChangeFilter);

        var filtered = GetFilteredChanges(source, count, labelAt);

        ImGui.BeginChild($"##{id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        MsbQueryMatch jumpTarget = null;

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
                GUI.Tooltip(LOC.Get("MAP_SearchTool_Jump_TT"));

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

    private void DisplaySkips(List<MsbPlannedSkip> allSkips)
    {
        if (allSkips.Count == 0)
            return;

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Changes_Skipped", allSkips.Count));

        var reasonIndex = 0;

        foreach (var group in allSkips.GroupBy(e => e.Reason))
        {
            var skips = group.ToList();

            if (ImGui.TreeNodeEx($"{group.Key} ({skips.Count})##msbMassEditSkip{reasonIndex}"))
            {
                for (int i = 0; i < Math.Min(skips.Count, MaxSkipsShown); i++)
                {
                    var match = skips[i].Match;

                    if (ImGui.Selectable($"{PrefixLabel(match, MsbQueryRunner.GetEntryLabel(match.Entry))}##msbMassEditSkip{reasonIndex}_{i}"))
                    {
                        Runner.Jump(match);
                    }
                }

                if (skips.Count > MaxSkipsShown)
                {
                    ImGui.TextDisabled(LOC.Get("MAP_MassEditTool_Changes_Skipped_More", skips.Count - MaxSkipsShown));
                }

                ImGui.TreePop();
            }

            reasonIndex++;
        }
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
            LOC.Get("MAP_MassEditTool_Deletions_Header"),
            LOC.Get("MAP_MassEditTool_Deletions_Header_TT"));

        if (DisplayAppliedResult())
            return;

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MassEditTool_Delete_Nothing"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_MassEditTool_Delete_Summary", Results.Count, ResultMaps.Count));

        DisplayUnloadedWarning(Results.Select(e => e.Match));

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MassEditTool_Changes_Stale"));
        }

        // Every delete is confirmed, as references to the objects are left pointing at nothing
        GUI.ConditionalMultiButtonInput("msbMassEditDeleteActions",
            "msbMassEditDelete",
            LOC.Get("MAP_MassEditTool_Delete"),
            LOC.Get("MAP_MassEditTool_Delete_TT"),
            () =>
            {
                ConfirmUnloadedMaps = MsbMassEditApplier.GetUnloadedMaps(View, Runner, Results.Select(e => e.Match));
                OpenConfirm = PendingConfirm.Delete;
            },
            !stale);
    }

    private void ApplyDelete()
    {
        if (Results == null || IsStale())
            return;

        var matches = Results.Select(e => e.Match).ToList();

        Applied(MsbMassEditAction.Delete, MsbMassEditApplier.ApplyDelete(View, Project, Runner, matches));
    }
    #endregion

    #region Popups
    /// <summary>
    /// The confirms, opened and drawn under the same ID, outside the tab bar so they stay open whichever tab is shown
    /// </summary>
    private void DisplayPopups()
    {
        var editTitle = $"{LOC.Get("MAP_MassEditTool_Edit_Confirm_Title")}{EditConfirmId}";
        var deleteTitle = $"{LOC.Get("MAP_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}";

        if (OpenConfirm is PendingConfirm.Edit)
            ImGui.OpenPopup(editTitle);
        else if (OpenConfirm is PendingConfirm.Delete)
            ImGui.OpenPopup(deleteTitle);

        OpenConfirm = PendingConfirm.None;

        if (DisplayConfirm(editTitle, LOC.Get("MAP_MassEditTool_Edit_Confirm", Plan?.Objects.Count ?? 0),
            LOC.Get("MAP_MassEditTool_Apply"), LOC.Get("MAP_MassEditTool_Apply_TT")))
        {
            ApplyEdit();
        }

        if (DisplayConfirm(deleteTitle, LOC.Get("MAP_MassEditTool_Delete_Confirm", Results?.Count ?? 0),
            LOC.Get("MAP_MassEditTool_Delete"), LOC.Get("MAP_MassEditTool_Delete_TT")))
        {
            ApplyDelete();
        }
    }

    /// <summary>
    /// A confirm that names the unloaded maps an apply writes. Returns true when it is confirmed.
    /// </summary>
    private bool DisplayConfirm(string title, string text, string confirmLabel, string confirmTooltip)
    {
        var confirmed = false;
        var open = true;

        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            GUI.WrappedText(text);

            if (ConfirmUnloadedMaps.Count > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                    LOC.Get("MAP_MassEditTool_Confirm_Unloaded", ConfirmUnloadedMaps.Count));

                var named = string.Join(", ", ConfirmUnloadedMaps.Take(MaxMapsNamed));

                if (ConfirmUnloadedMaps.Count > MaxMapsNamed)
                {
                    named = $"{named} {LOC.Get("MAP_MassEditTool_Confirm_Unloaded_More", ConfirmUnloadedMaps.Count - MaxMapsNamed)}";
                }

                GUI.WrappedText(named);
            }

            var cancelled = false;

            GUI.MultiButtonInput("msbMassEditConfirmActions",
                "msbMassEditConfirm",
                confirmLabel,
                confirmTooltip,
                () => confirmed = true,

                "msbMassEditConfirmCancel",
                LOC.Get("MAP_MassEditTool_Cancel"),
                LOC.Get("MAP_MassEditTool_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        return confirmed;
    }
    #endregion
}
