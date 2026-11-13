using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The edit half of the mass edit: the steps, the preview of every change, and Apply; or Delete. The preview is
/// worked out again whenever the steps or the matches change, so Apply always does what it shows.
/// </summary>
public partial class ModelMassEditTool
{
    /// <summary>
    /// A step as it is being edited. It is copied into a <see cref="ModelMassEditStep"/> when the plan is made.
    /// </summary>
    private class StepInput
    {
        public string Field = "";
        public ModelMassEditOperator Operator = ModelMassEditOperator.Set;
        public string Value = "";
        public string Value2 = "";

        // The text typed to narrow the list of fields
        public string FieldFilter = "";

        public ModelMassEditStep ToStep() => new(Field, Operator, Value.Trim(), Value2);
    }

    /// <summary>
    /// A field the steps can edit, with how many of the matches have it
    /// </summary>
    private record EditField(string Path, ModelMassEditFieldKind Kind, string TypeName, int Count)
    {
        public IReadOnlyList<ModelMassEditOperator> Operators => ModelMassEditFields.GetOperators(Kind);
    }

    // The fields are gathered from this many matches at most, which covers every kind of object in practice
    private const int MaxFieldSamples = 5000;

    private ModelMassEditAction Action = ModelMassEditAction.Edit;
    private List<StepInput> Steps = [new()];
    private int StepsVersion;

    private ModelMassEditPlan Plan;
    private int PlanStepsVersion = -1;
    private List<ResultRow> PlanFor;
    private int PlanForCount;

    private List<EditField> EditFields;
    private List<ResultRow> EditFieldsFor;
    private int EditFieldsForCount;

    // Toggled by the eye on the list's header
    private bool ShowChangesList = true;

    private string ChangeFilter = "";
    private bool ExactChangeFilter = false;
    private List<int> FilteredChanges;
    private ModelMassEditPlan FilteredChangesFor;
    private string CachedChangeFilter = "";
    private bool CachedExactChangeFilter = false;
    private string[] ChangeLabels;

    // Set once the plan has been applied or the matches deleted, so neither happens twice on top of itself
    private ModelMassEditApplier.Result AppliedResult;
    private ModelMassEditAction AppliedAction;

    private const string DeleteConfirmId = "###modelMassEditConfirmDelete";

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
        AppliedResult = null;
    }

    private void DisplayEdit()
    {
        if (Results == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Edit_No_Matches"));
            return;
        }

        if (Run is { Done: false })
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Edit_Finding", Results.Count));
            return;
        }

        GUI.WrappedText(GetMatchCountText());

        // Models that aren't loaded are edited directly, outside the history
        if (ResultsGlobal || Scope is ModelMassEditScope.CurrentFile)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Unloaded_Warning"));
        }

        // Action
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Action_Header"),
            LOC.Get("MODEL_MassEditTool_Action_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##modelMassEditAction", GetActionName(Action)))
        {
            foreach (var action in Enum.GetValues<ModelMassEditAction>())
            {
                if (ImGui.Selectable($"{GetActionName(action)}##modelMassEditAction{action}", Action == action))
                {
                    Action = action;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Action_Header_TT"));

        if (Action is ModelMassEditAction.Delete)
        {
            DisplayDelete();
            return;
        }

        if (Results.Count == 0)
            return;

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_MassEditTool_Steps_Header"),
            LOC.Get("MODEL_MassEditTool_Steps_Header_TT"));

        DisplaySteps(GetEditFields());

        UpdatePlan();

        DisplayChanges();
    }

    private static string GetActionName(ModelMassEditAction action) => action switch
    {
        ModelMassEditAction.Edit => LOC.Get("MODEL_MassEditTool_Action_Edit"),
        ModelMassEditAction.Delete => LOC.Get("MODEL_MassEditTool_Action_Delete"),
        _ => $"{action}"
    };

    #region Fields
    /// <summary>
    /// The fields of the matched objects that can be set, by path, worked out again as matches arrive
    /// </summary>
    private List<EditField> GetEditFields()
    {
        if (EditFields != null && EditFieldsFor == Results && EditFieldsForCount == Results.Count)
            return EditFields;

        var counts = new Dictionary<string, (ModelMassEditFieldKind Kind, string TypeName, int Count)>(StringComparer.Ordinal);

        foreach (var (match, _) in Results.Take(MaxFieldSamples))
        {
            foreach (var field in ModelQueryMatcher.GetFields(match.Object))
            {
                if (counts.TryGetValue(field.Name, out var known))
                {
                    counts[field.Name] = (known.Kind, known.TypeName, known.Count + 1);
                    continue;
                }

                if (!ModelMassEditFields.TryResolve(match.Object, field.Name, out var target))
                    continue;

                var kind = ModelMassEditFields.GetKind(target.Property);

                if (kind is ModelMassEditFieldKind.ReadOnly)
                    continue;

                counts[field.Name] = (kind, target.Property.PropertyType.Name, 1);
            }
        }

        EditFields = counts
            .Select(e => new EditField(e.Key, e.Value.Kind, e.Value.TypeName, e.Value.Count))
            .OrderBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        EditFieldsFor = Results;
        EditFieldsForCount = Results.Count;

        return EditFields;
    }
    #endregion

    #region Steps
    private void DisplaySteps(List<EditField> fields)
    {
        StepInput removed = null;

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            var field = fields.FirstOrDefault(e => e.Path == step.Field);

            var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(-removeWidth);

            DisplayStepFieldPicker(step, i, field, fields);

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##modelMassEditStepRemove{i}", DPI.IconButtonSize))
            {
                removed = step;
            }
            GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Step_Remove_TT"));

            if (field != null)
            {
                DisplayStepOperator(step, i, field);
            }
        }

        if (removed != null)
        {
            Steps.Remove(removed);
            StepsChanged();
        }

        GUI.MultiButtonInput("modelMassEditStepActions",
            "modelMassEditStepAdd",
            LOC.Get("MODEL_MassEditTool_Step_Add"),
            LOC.Get("MODEL_MassEditTool_Step_Add_TT"),
            () =>
            {
                Steps.Add(new());
                StepsChanged();
            });
    }

    private void DisplayStepFieldPicker(StepInput step, int index, EditField field, List<EditField> fields)
    {
        var preview = field != null ? field.Path : LOC.Get("MODEL_MassEditTool_Step_Pick_Field");

        if (ImGui.BeginCombo($"##modelMassEditStepField{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##modelMassEditStepFieldFilter{index}", LOC.Get("MODEL_MassEditTool_Field_Filter_Hint"),
                ref step.FieldFilter, 255);

            var filter = step.FieldFilter.Trim();
            var sampled = Math.Min(Results.Count, MaxFieldSamples);

            foreach (var candidate in fields)
            {
                var label = $"{candidate.Path}  [{candidate.TypeName}]";

                // Fields only some of the matches have are marked with how many do
                if (candidate.Count < sampled)
                {
                    label = $"{label}  {LOC.Get("MODEL_MassEditTool_Field_Count", candidate.Count)}";
                }

                if (filter != "" && !label.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##modelMassEditStepField{index}_{candidate.Path}", step.Field == candidate.Path))
                {
                    if (step.Field != candidate.Path)
                    {
                        step.Field = candidate.Path;
                        step.FieldFilter = "";

                        if (!candidate.Operators.Contains(step.Operator))
                        {
                            step.Operator = candidate.Operators[0];
                        }

                        step.Value = "";
                        step.Value2 = "";

                        StepsChanged();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Step_Field_TT"));
    }

    private void DisplayStepOperator(StepInput step, int index, EditField field)
    {
        var operators = field.Operators;

        if (!operators.Contains(step.Operator))
        {
            step.Operator = operators[0];
            StepsChanged();
        }

        var takesValue = ModelMassEditFields.TakesValue(step.Operator);

        ImGui.SetNextItemWidth(takesValue ? ImGui.GetContentRegionAvail().X * 0.35f : -1);

        if (ImGui.BeginCombo($"##modelMassEditStepOperator{index}", GetOperatorName(step.Operator)))
        {
            foreach (var op in operators)
            {
                if (ImGui.Selectable($"{GetOperatorName(op)}##modelMassEditStepOperator{index}_{op}", step.Operator == op))
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

        if (step.Operator is ModelMassEditOperator.Replace)
        {
            var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

            ImGui.SetNextItemWidth(width);

            if (ImGui.InputTextWithHint($"##modelMassEditStepValue{index}", LOC.Get("MODEL_MassEditTool_Replace_Find_Hint"), ref step.Value, 255))
                StepsChanged();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);

            if (ImGui.InputTextWithHint($"##modelMassEditStepValue2{index}", LOC.Get("MODEL_MassEditTool_Replace_With_Hint"), ref step.Value2, 255))
                StepsChanged();

            return;
        }

        // An enum is picked from its values
        if (field.Kind is ModelMassEditFieldKind.Enum && DisplayEnumChoices(step, index, field))
            return;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint($"##modelMassEditStepValue{index}", GetValueHint(field.Kind), ref step.Value, 255))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Step_Value_TT"));
    }

    /// <summary>
    /// The values of an enum field, found from a match that has the field. False when there is none to read it from.
    /// </summary>
    private bool DisplayEnumChoices(StepInput step, int index, EditField field)
    {
        Type enumType = null;

        foreach (var (match, _) in Results)
        {
            if (ModelMassEditFields.TryResolve(match.Object, field.Path, out var target))
            {
                enumType = target.Property.PropertyType;
                break;
            }
        }

        if (enumType == null || !enumType.IsEnum)
            return false;

        ImGui.SetNextItemWidth(-1);

        var preview = string.IsNullOrEmpty(step.Value) ? LOC.Get("MODEL_MassEditTool_Step_Pick_Value") : step.Value;

        if (ImGui.BeginCombo($"##modelMassEditStepEnum{index}", preview))
        {
            foreach (var name in Enum.GetNames(enumType))
            {
                if (ImGui.Selectable($"{name}##modelMassEditStepEnum{index}_{name}", step.Value == name))
                {
                    step.Value = name;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_MassEditTool_Step_Enum_TT"));

        return true;
    }

    private static string GetValueHint(ModelMassEditFieldKind kind) => kind switch
    {
        ModelMassEditFieldKind.Vector => LOC.Get("MODEL_MassEditTool_Value_Vector_Hint"),
        ModelMassEditFieldKind.Colour => LOC.Get("MODEL_MassEditTool_Value_Colour_Hint"),
        ModelMassEditFieldKind.Bool => LOC.Get("MODEL_MassEditTool_Value_Bool_Hint"),
        _ => LOC.Get("MODEL_MassEditTool_Value_Hint")
    };

    private static string GetOperatorName(ModelMassEditOperator op) => op switch
    {
        ModelMassEditOperator.Set => LOC.Get("MODEL_MassEditTool_Operator_Set"),
        ModelMassEditOperator.Add => "+",
        ModelMassEditOperator.Subtract => "-",
        ModelMassEditOperator.MultiplyBy => "*",
        ModelMassEditOperator.DivideBy => "/",
        ModelMassEditOperator.AtLeast => LOC.Get("MODEL_MassEditTool_Operator_At_Least"),
        ModelMassEditOperator.AtMost => LOC.Get("MODEL_MassEditTool_Operator_At_Most"),
        ModelMassEditOperator.Toggle => LOC.Get("MODEL_MassEditTool_Operator_Toggle"),
        ModelMassEditOperator.Replace => LOC.Get("MODEL_MassEditTool_Operator_Replace"),
        _ => $"{op}"
    };

    private static string GetOperatorTooltip(ModelMassEditOperator op) => op switch
    {
        ModelMassEditOperator.Set => LOC.Get("MODEL_MassEditTool_Operator_Set_TT"),
        ModelMassEditOperator.Add => LOC.Get("MODEL_MassEditTool_Operator_Add_TT"),
        ModelMassEditOperator.Subtract => LOC.Get("MODEL_MassEditTool_Operator_Subtract_TT"),
        ModelMassEditOperator.MultiplyBy => LOC.Get("MODEL_MassEditTool_Operator_Multiply_TT"),
        ModelMassEditOperator.DivideBy => LOC.Get("MODEL_MassEditTool_Operator_Divide_TT"),
        ModelMassEditOperator.AtLeast => LOC.Get("MODEL_MassEditTool_Operator_At_Least_TT"),
        ModelMassEditOperator.AtMost => LOC.Get("MODEL_MassEditTool_Operator_At_Most_TT"),
        ModelMassEditOperator.Toggle => LOC.Get("MODEL_MassEditTool_Operator_Toggle_TT"),
        ModelMassEditOperator.Replace => LOC.Get("MODEL_MassEditTool_Operator_Replace_TT"),
        _ => ""
    };
    #endregion

    #region Changes
    private void UpdatePlan()
    {
        if (AppliedResult != null)
            return;

        if (Plan != null && PlanStepsVersion == StepsVersion && PlanFor == Results && PlanForCount == Results.Count)
            return;

        Plan = ModelMassEditPlanner.Plan(Results.Select(e => e.Match).ToList(), Steps.Select(e => e.ToStep()).ToList());
        PlanStepsVersion = StepsVersion;
        PlanFor = Results;
        PlanForCount = Results.Count;

        ChangeLabels = new string[Plan.Changes.Count];
        FilteredChanges = null;
    }

    private void DisplayChanges()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("MODEL_MassEditTool_Changes_Header"),
            LOC.Get("MODEL_MassEditTool_Changes_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Plan == null || !Steps.Any(e => e.Field != ""))
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Changes_No_Steps"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Changes_Summary", Plan.Changes.Count, Plan.Objects.Count, Plan.ModelCount));

        if (Plan.Unchanged > 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Changes_Unchanged", Plan.Unchanged));
        }

        DisplaySkips(Plan.Skips);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Changes_Stale"));
        }

        var apply = false;

        GUI.ConditionalMultiButtonInput("modelMassEditApplyActions",
            "modelMassEditApply",
            LOC.Get("MODEL_MassEditTool_Apply"),
            LOC.Get("MODEL_MassEditTool_Apply_TT"),
            () => apply = true,
            !stale && Plan.Objects.Count > 0);

        // Applying drops the plan, so the rest of the section, which draws the plan, waits for the next frame
        if (apply)
        {
            ApplyPlan();
            return;
        }

        if (Plan.Changes.Count == 0 || !ShowChangesList)
            return;

        EditorFilters.DisplayFramedListFilter("modelMassEditChangeFilter", ref ChangeFilter, ref ExactChangeFilter);

        var filtered = GetFilteredChanges();

        ImGui.BeginChild("##modelMassEditChanges", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        ModelQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var change = Plan.Changes[index];

                if (ImGui.Button($"{Icons.LocationArrow}##modelMassEditChangeJump{index}"))
                {
                    jumpTarget = change.Match;
                }
                GUI.Tooltip(LOC.Get("MODEL_SearchTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{GetChangeLabel(index)}##modelMassEditChange{index}", ModelQueryRunner.IsSelected(View, change.Match)))
                {
                    jumpTarget = change.Match;
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

    private void DisplaySkips(List<ModelPlannedSkip> allSkips)
    {
        if (allSkips.Count == 0)
            return;

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Changes_Skipped", allSkips.Count));

        var reasonIndex = 0;

        foreach (var group in allSkips.GroupBy(e => e.Reason))
        {
            var skips = group.ToList();

            if (ImGui.TreeNodeEx($"{group.Key} ({skips.Count})##modelMassEditSkip{reasonIndex}"))
            {
                for (int i = 0; i < Math.Min(skips.Count, MaxSkipsShown); i++)
                {
                    var match = skips[i].Match;

                    if (ImGui.Selectable($"{PrefixLabel(match, GetObjectLabel(match))}##modelMassEditSkip{reasonIndex}_{i}"))
                    {
                        Runner.Jump(match);
                    }
                }

                if (skips.Count > MaxSkipsShown)
                {
                    ImGui.TextDisabled(LOC.Get("MODEL_MassEditTool_Changes_Skipped_More", skips.Count - MaxSkipsShown));
                }

                ImGui.TreePop();
            }

            reasonIndex++;
        }
    }

    private static string GetObjectLabel(ModelQueryMatch match)
    {
        var name = ModelQueryMatcher.GetObjectName(match.Kind, match.Object, match.Index);
        var kind = ModelQueryMatcher.GetKindName(match.Kind);

        return string.IsNullOrEmpty(name) ? kind : $"{kind} {name}";
    }

    private string GetChangeLabel(int index)
    {
        if (ChangeLabels[index] != null)
            return ChangeLabels[index];

        var change = Plan.Changes[index];
        var label = $"{PrefixLabel(change.Match, GetObjectLabel(change.Match))}  {change.Field}: {change.OldText} -> {change.NewText}";

        ChangeLabels[index] = label;

        return label;
    }

    private List<int> GetFilteredChanges()
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
            if (ChangeFilter == "" || EditorFilters.IsMatch(ChangeFilter, GetChangeLabel(i), ExactChangeFilter))
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

    private void ApplyPlan()
    {
        if (Plan == null || IsStale())
            return;

        var result = ModelMassEditApplier.Apply(View, Plan);

        Smithbox.Log(this, LOC.Get("MODEL_MassEditTool_Applied_Log", result.Applied, result.Stale, result.Skipped));

        // The values the plan was read from are now out of date, so it is dropped until the next Find
        Plan = null;
        ChangeLabels = null;
        FilteredChanges = null;
        AppliedResult = result;
        AppliedAction = ModelMassEditAction.Edit;
    }
    #endregion

    #region Delete
    private void DisplayDelete()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("MODEL_MassEditTool_Deletions_Header"),
            LOC.Get("MODEL_MassEditTool_Deletions_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Delete_Nothing"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Delete_Summary", Results.Count, ResultModels.Count));
        GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Delete_Loaded_Only"));

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Changes_Stale"));
        }

        var delete = false;

        GUI.ConditionalMultiButtonInput("modelMassEditDeleteActions",
            "modelMassEditDelete",
            LOC.Get("MODEL_MassEditTool_Delete"),
            LOC.Get("MODEL_MassEditTool_Delete_TT"),
            () => delete = true,
            !stale);

        // The popup is opened here rather than in the button, so it is opened and drawn under the same ID
        if (delete)
        {
            ImGui.OpenPopup($"{LOC.Get("MODEL_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}");
        }

        if (DisplayDeleteConfirm())
            return;

        if (!ShowChangesList)
            return;

        DisplayMatchList("modelMassEditDeletions", GetFilteredResults());
    }

    /// <summary>
    /// Asks before deleting, as bones and materials take the references to them along. Returns true once deleted.
    /// </summary>
    private bool DisplayDeleteConfirm()
    {
        var open = true;
        var confirmed = false;

        if (ImGui.BeginPopupModal($"{LOC.Get("MODEL_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}",
            ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Delete_Confirm_Text", Results.Count));

            var cancelled = false;

            GUI.MultiButtonInput("modelMassEditDeleteConfirmActions",
                "modelMassEditDeleteConfirm",
                LOC.Get("MODEL_MassEditTool_Delete"),
                LOC.Get("MODEL_MassEditTool_Delete_TT"),
                () => confirmed = true,

                "modelMassEditDeleteCancel",
                LOC.Get("MODEL_MassEditTool_Delete_Cancel"),
                LOC.Get("MODEL_MassEditTool_Delete_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (!confirmed)
            return false;

        var result = ModelMassEditApplier.Delete(Project, Results.Select(e => e.Match).ToList());

        Smithbox.Log(this, LOC.Get("MODEL_MassEditTool_Deleted_Log", result.Applied, result.Stale, result.Skipped));

        AppliedResult = result;
        AppliedAction = ModelMassEditAction.Delete;

        return true;
    }
    #endregion

    /// <summary>
    /// Shows what the last apply or delete did. Returns true if anything has been done since Find, as the matches no
    /// longer show what is there.
    /// </summary>
    private bool DisplayAppliedResult()
    {
        if (AppliedResult == null)
            return false;

        GUI.WrappedText(AppliedAction is ModelMassEditAction.Delete
            ? LOC.Get("MODEL_MassEditTool_Deleted", AppliedResult.Applied)
            : LOC.Get("MODEL_MassEditTool_Applied", AppliedResult.Applied));

        if (AppliedResult.Stale > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Applied_Stale", AppliedResult.Stale));
        }

        if (AppliedResult.Skipped > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_MassEditTool_Applied_Skipped", AppliedResult.Skipped));

            foreach (var reason in AppliedResult.Reasons)
            {
                GUI.WrappedText($"  {reason}");
            }
        }

        // Models that aren't loaded are left unsaved, and Save only writes the loaded one
        if (AppliedResult.Unloaded > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("MODEL_MassEditTool_Applied_Save_All", AppliedResult.Unloaded, LOC.Get("EDITOR_Menubar_Action_SaveAll")));
        }

        GUI.WrappedText(LOC.Get("MODEL_MassEditTool_Applied_Find_Again"));

        return true;
    }
}
