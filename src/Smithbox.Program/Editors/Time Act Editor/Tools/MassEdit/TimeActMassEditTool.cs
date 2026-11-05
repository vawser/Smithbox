using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Finds animations or events with a filter built from pickers, then edits, creates or deletes across every match
/// in one undoable step. The filter is matched by <see cref="TimeActQueryRunner"/>, as the searches are.
/// See Documentation/Claude/TAE Mass Edit Plan.md.
/// </summary>
public partial class TimeActMassEditTool
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private readonly TimeActQueryRunner Runner;

    /// <summary>
    /// A condition as it is being edited. It is copied into a <see cref="TimeActMassEditCondition"/> on Find.
    /// </summary>
    private class ConditionInput
    {
        public string Field = "";
        public bool PickedField = false;
        public TimeActMassEditComparison Comparison = TimeActMassEditComparison.Equals;
        public string Value = "";
        public string Value2 = "";

        // The text typed to narrow the list of fields
        public string FieldFilter = "";

        public TimeActMassEditCondition ToCondition() =>
            new(Field.Trim(), PickedField, Comparison, Value.Trim(), Value2.Trim());
    }

    private TimeActMassEditScope Scope = TimeActMassEditScope.CurrentTimeAct;
    private TimeActMassEditTarget Target = TimeActMassEditTarget.Events;

    private string AnimationText = "";
    private int? EventType = null;
    private string EventText = "";
    private string EventTypeFilter = "";
    private List<ConditionInput> Conditions = [new()];
    private bool Exact = false;

    // Raised by every change to the inputs, so the matches can tell they no longer show what the inputs ask for
    private int InputsVersion;
    private int FoundVersion = -1;

    // What the scope pointed to when Find was pressed
    private TimeActEntry FoundTimeAct;
    private TAE.Animation FoundAnimation;
    private TAE.Event[] FoundEvents;

    private TimeActQueryRun Run;

    private record ResultRow(TimeActQueryMatch Match, string Label);

    private List<ResultRow> Results;
    private HashSet<TAE.Animation> ResultAnimations;
    private HashSet<TimeActContainerWrapper> ResultSources;
    private TimeActContainerWrapper ResultSource;
    private bool ResultsGlobal;
    private TimeActMassEditTarget ResultTarget;
    private bool ShowSourceInLabel;
    private bool ShowTimeActInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Toggled by the eye on the list's header
    private bool ShowMatchesList = true;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    public TimeActMassEditTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Runner = new TimeActQueryRunner(view, project);
        Planner = new TimeActMassEditPlanner(project);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TAE_MassEditTool_Header")}##timeActMassEditToolHeader"))
            return;

        ImGui.BeginChild("TimeActMassEditToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Hint"));

        if (!Project.Handler.TimeActData.CanSaveTimeActs)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Not_Supported"));
            return;
        }

        var source = View.Selection.SelectedTimeActContainerWrapper;

        // Local matches point into the source they were found in. Matches across all sources are kept while
        // the user jumps between them, which is how they are looked at.
        if (Results != null && !ResultsGlobal && ResultSource != source)
        {
            ClearResults();
        }

        // The pickers list the event types of the selected source, or of the first one when none is selected
        var template = GetPickerTemplate(source);

        // A search handed its query over
        if (PendingFind)
        {
            PendingFind = false;

            if (GetScopeUnavailableReason() == null)
            {
                StartFind(source, template);
            }
        }

        // Picked up whichever tab is open, so a find keeps going while the Edit tab is shown
        PickUpResults();

        if (ImGui.BeginTabBar("##massEditTabs"))
        {
            var findFlags = SelectFindTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            SelectFindTab = false;

            // Find
            if (ImGui.BeginTabItem($"{LOC.Get("TAE_MassEditTool_Tab_Find")}##massEditTabFind", findFlags))
            {
                DisplayScope();
                DisplayTarget();
                DisplayFilter(template);

                if (DisplayActions())
                {
                    StartFind(source, template);
                }

                DisplayProgress();
                DisplayResults();

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Tab_Find_TT"));

            // Edit
            if (ImGui.BeginTabItem($"{LOC.Get("TAE_MassEditTool_Tab_Edit")}##massEditTabEdit"))
            {
                DisplayEdit(template);

                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Tab_Edit_TT"));

            ImGui.EndTabBar();
        }
    }

    private void Changed()
    {
        InputsVersion++;
    }

    // Set by a search handing its query over, so Find runs on the next draw and the Find tab is brought forward
    private bool PendingFind;
    private bool SelectFindTab;

    /// <summary>
    /// Takes over the inputs of a search, so its results can be edited all at once. The search's text inputs
    /// become typed text here, matched as the search matches them: the field and value become a condition, with
    /// an exact search's contains turned into equals. The tool is shown, and Find runs once it is next drawn.
    /// </summary>
    public void LoadQuery(TimeActQuery query, TimeActMassEditScope scope)
    {
        Scope = scope;
        Target = query.MatchesEvents ? TimeActMassEditTarget.Events : TimeActMassEditTarget.Animations;

        AnimationText = query.Animation ?? "";
        EventType = null;
        EventText = query.Event ?? "";
        EventTypeFilter = "";
        Exact = query.Exact;

        var condition = new ConditionInput();

        if (query.HasField || query.HasValue)
        {
            condition.Field = query.Field ?? "";
            condition.PickedField = false;
            condition.Comparison = query.Comparison is TimeActMassEditComparison.Contains
                ? query.Exact ? TimeActMassEditComparison.Equals : TimeActMassEditComparison.Contains
                : query.Comparison;
            condition.Value = query.Value ?? "";
            condition.Value2 = query.Value2 ?? "";
        }

        Conditions = [condition];

        Changed();
        ClearResults();

        CFG.Current.Interface_TimeActEditor_Tool_MassEdit = true;

        PendingFind = true;
        SelectFindTab = true;
    }

    /// <summary>
    /// The event templates of a source. Sources can differ, such as the objects of Dark Souls, so every match is
    /// read and edited with the templates of its own source.
    /// </summary>
    private TAE.Template.BankTemplate TemplateFor(TimeActContainerWrapper source)
    {
        return source == null ? null : Project.Handler.TimeActData.GetTemplate(source);
    }

    private TAE.Template.BankTemplate GetPickerTemplate(TimeActContainerWrapper source)
    {
        source ??= Project.Handler.TimeActData.PrimaryBank?.GetCharacters().FirstOrDefault();

        return TemplateFor(source);
    }

    #region Scope and Target
    private void DisplayScope()
    {
        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Scope_Header"),
            LOC.Get("TAE_MassEditTool_Scope_Header_TT"));

        // Animations can't be picked by their selected events
        if (Target is TimeActMassEditTarget.Animations && Scope is TimeActMassEditScope.SelectedEvents)
        {
            Scope = TimeActMassEditScope.CurrentAnimation;
            Changed();
        }

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditScope", GetScopeName(Scope)))
        {
            foreach (var scope in Enum.GetValues<TimeActMassEditScope>())
            {
                if (Target is TimeActMassEditTarget.Animations && scope is TimeActMassEditScope.SelectedEvents)
                    continue;

                if (ImGui.Selectable($"{GetScopeName(scope)}##massEditScope{scope}", Scope == scope))
                {
                    Scope = scope;
                    Changed();
                }

                GUI.Tooltip(GetScopeTooltip(scope));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetScopeTooltip(Scope));

        var unavailable = GetScopeUnavailableReason();

        if (unavailable != null)
        {
            GUI.WrappedText(unavailable);
        }

        // The list of sources is still being filled in, so some would be left out
        if (Scope is TimeActMassEditScope.AllSources &&
            Project.Handler.TimeActData.PrimaryBank is { IsScanning: true } bank)
        {
            GUI.WrappedText(LOC.Get("TAE_SearchTool_Global_Scanning", bank.ScanProgress, bank.ScanTotal));
        }
    }

    /// <summary>
    /// The scope needs the selection to point at what it covers. Gives the reason it can't be used, or null.
    /// </summary>
    private string GetScopeUnavailableReason()
    {
        var selection = View.Selection;

        if (Scope is not TimeActMassEditScope.AllSources && selection.SelectedTimeActContainerWrapper == null)
            return LOC.Get("TAE_MassEditTool_No_Source");

        return Scope switch
        {
            TimeActMassEditScope.SelectedEvents when selection.SelectedEvents.Count == 0 =>
                LOC.Get("TAE_MassEditTool_Scope_No_Events"),
            TimeActMassEditScope.CurrentAnimation when selection.SelectedAnimation == null =>
                LOC.Get("TAE_MassEditTool_Scope_No_Animation"),
            TimeActMassEditScope.CurrentTimeAct when selection.SelectedTimeAct == null =>
                LOC.Get("TAE_MassEditTool_Scope_No_TimeAct"),
            TimeActMassEditScope.SelectedEvents or TimeActMassEditScope.CurrentAnimation
                when selection.SelectedTimeAct == null => LOC.Get("TAE_MassEditTool_Scope_No_TimeAct"),
            _ => null
        };
    }

    private static string GetScopeName(TimeActMassEditScope scope) => scope switch
    {
        TimeActMassEditScope.SelectedEvents => LOC.Get("TAE_MassEditTool_Scope_Selected_Events"),
        TimeActMassEditScope.CurrentAnimation => LOC.Get("TAE_MassEditTool_Scope_Current_Animation"),
        TimeActMassEditScope.CurrentTimeAct => LOC.Get("TAE_MassEditTool_Scope_Current_TimeAct"),
        TimeActMassEditScope.CurrentSource => LOC.Get("TAE_MassEditTool_Scope_Current_Source"),
        TimeActMassEditScope.AllSources => LOC.Get("TAE_MassEditTool_Scope_All_Sources"),
        _ => $"{scope}"
    };

    private static string GetScopeTooltip(TimeActMassEditScope scope) => scope switch
    {
        TimeActMassEditScope.SelectedEvents => LOC.Get("TAE_MassEditTool_Scope_Selected_Events_TT"),
        TimeActMassEditScope.CurrentAnimation => LOC.Get("TAE_MassEditTool_Scope_Current_Animation_TT"),
        TimeActMassEditScope.CurrentTimeAct => LOC.Get("TAE_MassEditTool_Scope_Current_TimeAct_TT"),
        TimeActMassEditScope.CurrentSource => LOC.Get("TAE_MassEditTool_Scope_Current_Source_TT"),
        TimeActMassEditScope.AllSources => LOC.Get("TAE_MassEditTool_Scope_All_Sources_TT"),
        _ => ""
    };

    private void DisplayTarget()
    {
        // Target
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Target_Header"),
            LOC.Get("TAE_MassEditTool_Target_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditTarget", GetTargetName(Target)))
        {
            foreach (var target in Enum.GetValues<TimeActMassEditTarget>())
            {
                if (ImGui.Selectable($"{GetTargetName(target)}##massEditTarget{target}", Target == target))
                {
                    if (Target != target)
                    {
                        Target = target;

                        // The fields of events and animations differ, so the picked ones no longer apply
                        ClearPickedFields();
                        Changed();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Target_Header_TT"));
    }

    private static string GetTargetName(TimeActMassEditTarget target) => target switch
    {
        TimeActMassEditTarget.Events => LOC.Get("TAE_MassEditTool_Target_Events"),
        TimeActMassEditTarget.Animations => LOC.Get("TAE_MassEditTool_Target_Animations"),
        _ => $"{target}"
    };
    #endregion

    #region Filter
    private void DisplayFilter(TAE.Template.BankTemplate template)
    {
        // Animation
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Animation_Header"),
            LOC.Get("TAE_MassEditTool_Animation_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint("##massEditAnimation", LOC.Get("TAE_MassEditTool_Any_Hint"), ref AnimationText, 255))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Animation_Header_TT"));

        // Event
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Event_Header"),
            LOC.Get(Target is TimeActMassEditTarget.Events ? "TAE_MassEditTool_Event_Header_TT" : "TAE_MassEditTool_Event_Header_Animations_TT"));

        DisplayEventTypePicker(template);

        // Conditions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Conditions_Header"),
            LOC.Get("TAE_MassEditTool_Conditions_Header_TT"));

        DisplayConditions(template);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Options_Header"),
            LOC.Get("TAE_MassEditTool_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_MassEditTool_Exact")}##massEditExact", ref Exact))
        {
            Changed();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Exact_TT"));
    }

    /// <summary>
    /// A combo of the event types of the template. Text typed in it narrows the list, and can be used as the filter
    /// itself, matched against the names and IDs of the events as the search does.
    /// </summary>
    private void DisplayEventTypePicker(TAE.Template.BankTemplate template)
    {
        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditEventType", GetEventTypePreview(template), ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##massEditEventTypeFilter", LOC.Get("TAE_MassEditTool_Event_Filter_Hint"), ref EventTypeFilter, 255);
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Event_Filter_TT"));

            var filter = EventTypeFilter.Trim();

            if (ImGui.Selectable($"{LOC.Get("TAE_MassEditTool_Event_Any")}##massEditEventAny", EventType == null && EventText == ""))
            {
                SetEventType(null, "", template);
            }

            if (filter != "" &&
                ImGui.Selectable($"{LOC.Get("TAE_MassEditTool_Event_Match_Text", filter)}##massEditEventText", EventType == null && EventText == filter))
            {
                SetEventType(null, filter, template);
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Event_Match_Text_TT"));

            if (template == null)
            {
                ImGui.TextDisabled(LOC.Get("TAE_MassEditTool_No_Template"));
            }
            else
            {
                foreach (var eventTemplate in template.Values.OrderBy(e => e.ID))
                {
                    var name = $"[{eventTemplate.ID}] {eventTemplate.Name}";

                    if (filter != "" && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ImGui.Selectable($"{name}##massEditEvent{eventTemplate.ID}", EventType == eventTemplate.ID))
                    {
                        SetEventType(eventTemplate.ID, "", template);
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get(Target is TimeActMassEditTarget.Events ? "TAE_MassEditTool_Event_Header_TT" : "TAE_MassEditTool_Event_Header_Animations_TT"));
    }

    private void SetEventType(int? type, string text, TAE.Template.BankTemplate template)
    {
        if (EventType == type && EventText == text)
            return;

        EventType = type;
        EventText = text;
        EventTypeFilter = "";

        // Picked fields belong to an event type, so those the new type doesn't have are let go
        if (Target is TimeActMassEditTarget.Events)
        {
            var eventTemplate = GetEventTemplate(template);

            foreach (var condition in Conditions)
            {
                if (condition.PickedField && !IsTimeField(condition.Field) &&
                    (eventTemplate == null || !eventTemplate.Values.Any(e => e.Name == condition.Field)))
                {
                    ClearField(condition);
                }
            }
        }

        Changed();
    }

    private string GetEventTypePreview(TAE.Template.BankTemplate template)
    {
        if (EventType is { } type)
        {
            return template != null && template.TryGetValue(type, out var eventTemplate)
                ? $"[{type}] {eventTemplate.Name}"
                : $"[{type}]";
        }

        if (EventText != "")
            return LOC.Get(Exact ? "TAE_MassEditTool_Event_Text_Exact_Preview" : "TAE_MassEditTool_Event_Text_Preview", EventText);

        return LOC.Get("TAE_MassEditTool_Event_Any");
    }

    private TAE.Template.EventTemplate GetEventTemplate(TAE.Template.BankTemplate template)
    {
        if (EventType is { } type && template != null && template.TryGetValue(type, out var eventTemplate))
            return eventTemplate;

        return null;
    }

    private void DisplayConditions(TAE.Template.BankTemplate template)
    {
        var eventTemplate = Target is TimeActMassEditTarget.Events ? GetEventTemplate(template) : null;

        ConditionInput removed = null;

        for (int i = 0; i < Conditions.Count; i++)
        {
            var condition = Conditions[i];

            if (i > 0)
            {
                ImGui.Separator();
            }

            // Field
            var removeWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SetNextItemWidth(-removeWidth);

            DisplayFieldPicker(condition, i, eventTemplate);

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##massEditConditionRemove{i}", DPI.IconButtonSize))
            {
                removed = condition;
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Condition_Remove_TT"));

            // Comparison and value
            DisplayComparison(condition, i, eventTemplate);
        }

        if (removed != null)
        {
            Conditions.Remove(removed);
            Changed();
        }

        GUI.MultiButtonInput("massEditConditionActions",
            "massEditConditionAdd",
            LOC.Get("TAE_MassEditTool_Condition_Add"),
            LOC.Get("TAE_MassEditTool_Condition_Add_TT"),
            () =>
            {
                Conditions.Add(new());
                Changed();
            });
    }

    /// <summary>
    /// The fields that can be picked: the times and the parameters of the picked event type, or the properties
    /// of an animation. Typed text narrows the list, and can be used as a field name to match as text.
    /// </summary>
    private void DisplayFieldPicker(ConditionInput condition, int index, TAE.Template.EventTemplate eventTemplate)
    {
        if (ImGui.BeginCombo($"##massEditConditionField{index}", GetFieldPreview(condition, eventTemplate), ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"##massEditConditionFieldFilter{index}", LOC.Get("TAE_MassEditTool_Field_Filter_Hint"), ref condition.FieldFilter, 255);
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Field_Filter_TT"));

            var filter = condition.FieldFilter.Trim();

            if (ImGui.Selectable($"{LOC.Get("TAE_MassEditTool_Field_Any")}##massEditFieldAny{index}", condition.Field == ""))
            {
                SetField(condition, "", false);
            }

            if (filter != "" &&
                ImGui.Selectable($"{LOC.Get("TAE_MassEditTool_Field_Match_Text", filter)}##massEditFieldText{index}",
                    !condition.PickedField && condition.Field == filter))
            {
                SetField(condition, filter, false);
            }
            GUI.Tooltip(LOC.Get("TAE_MassEditTool_Field_Match_Text_TT"));

            foreach (var (name, displayName) in GetPickableFields(eventTemplate))
            {
                var label = displayName == name ? name : $"{displayName}  ({name})";

                if (filter != "" && !label.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##massEditField{index}_{name}", condition.PickedField && condition.Field == name))
                {
                    SetField(condition, name, true);
                }
            }

            if (Target is TimeActMassEditTarget.Events && eventTemplate == null)
            {
                ImGui.TextDisabled(LOC.Get("TAE_MassEditTool_Field_Pick_Event_Type"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Field_TT"));
    }

    private void SetField(ConditionInput condition, string field, bool picked)
    {
        if (condition.Field == field && condition.PickedField == picked)
            return;

        condition.Field = field;
        condition.PickedField = picked;
        condition.FieldFilter = "";

        Changed();
    }

    private void ClearField(ConditionInput condition)
    {
        condition.Field = "";
        condition.PickedField = false;
        condition.FieldFilter = "";
    }

    private void ClearPickedFields()
    {
        foreach (var condition in Conditions.Where(e => e.PickedField))
        {
            ClearField(condition);
        }
    }

    private static bool IsTimeField(string name) => name is "StartTime" or "EndTime";

    /// <summary>
    /// The internal name of each field that can be picked, with the name it is shown under
    /// </summary>
    private IEnumerable<(string Name, string DisplayName)> GetPickableFields(TAE.Template.EventTemplate eventTemplate)
    {
        if (Target is TimeActMassEditTarget.Animations)
        {
            yield return ("ID", LOC.Get("TAE_PropertiesWindow_Animation_ID"));
            yield return ("AnimFileName", LOC.Get("TAE_PropertiesWindow_Animation_File_Name"));
            yield return ("ImportHKXSourceAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_HKX_Source"));
            yield return ("ImportFromAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_From"));
            yield break;
        }

        yield return ("StartTime", LOC.Get("TAE_PropertiesWindow_Start_Time"));
        yield return ("EndTime", LOC.Get("TAE_PropertiesWindow_End_Time"));

        if (eventTemplate == null)
            yield break;

        var data = Project.Handler.TimeActData;

        foreach (var parameter in eventTemplate.Values)
        {
            // Fixed values aren't parameters as such, so they can't be matched either
            if (parameter.ValueToAssert != null)
                continue;

            yield return (parameter.Name, data.GetFieldDisplayName(eventTemplate, parameter.Name));
        }
    }

    private string GetFieldPreview(ConditionInput condition, TAE.Template.EventTemplate eventTemplate)
    {
        if (condition.Field == "")
            return LOC.Get("TAE_MassEditTool_Field_Any");

        if (!condition.PickedField)
            return LOC.Get(Exact ? "TAE_MassEditTool_Field_Text_Exact_Preview" : "TAE_MassEditTool_Field_Text_Preview", condition.Field);

        var picked = GetPickableFields(eventTemplate).FirstOrDefault(e => e.Name == condition.Field);

        return picked.Name != null ? picked.DisplayName : condition.Field;
    }

    /// <summary>
    /// The kind of value a condition compares, which decides the comparisons offered and how the value is put in
    /// </summary>
    private enum ValueKind
    {
        Any,
        Enum,
        Bool,
        NumberBool
    }

    private (ValueKind Kind, TAE.Template.ParameterTemplate Parameter) GetValueKind(ConditionInput condition,
        TAE.Template.EventTemplate eventTemplate)
    {
        var parameter = condition.PickedField ? eventTemplate?.Values.FirstOrDefault(e => e.Name == condition.Field) : null;

        if (parameter == null)
        {
            return (ValueKind.Any, null);
        }

        if (parameter.EnumEntries != null)
            return (ValueKind.Enum, parameter);

        if (parameter.Type is TAE.Template.ParamType.b)
            return (ValueKind.Bool, parameter);

        if (Project.Handler.TimeActData.GetFieldMeta(eventTemplate, parameter.Name)?.IsBool == true)
            return (ValueKind.NumberBool, parameter);

        return (ValueKind.Any, parameter);
    }

    private void DisplayComparison(ConditionInput condition, int index, TAE.Template.EventTemplate eventTemplate)
    {
        var (kind, parameter) = GetValueKind(condition, eventTemplate);

        // Values that are picked from a list are only ever the same or not
        var comparisons = kind is ValueKind.Any
            ? Enum.GetValues<TimeActMassEditComparison>()
            : [TimeActMassEditComparison.Equals, TimeActMassEditComparison.NotEquals];

        if (!comparisons.Contains(condition.Comparison))
        {
            condition.Comparison = TimeActMassEditComparison.Equals;
            Changed();
        }

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.35f);

        if (ImGui.BeginCombo($"##massEditConditionComparison{index}", GetComparisonName(condition.Comparison)))
        {
            foreach (var comparison in comparisons)
            {
                if (ImGui.Selectable($"{GetComparisonName(comparison)}##massEditComparison{index}_{comparison}",
                    condition.Comparison == comparison))
                {
                    condition.Comparison = comparison;
                    Changed();
                }
                GUI.Tooltip(GetComparisonTooltip(comparison));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetComparisonTooltip(condition.Comparison));

        ImGui.SameLine();

        switch (kind)
        {
            case ValueKind.Enum:
                DisplayValueChoices(condition, index,
                    parameter.EnumEntries.Select(e => ($"{e.Key} ({TimeActQueryMatcher.FormatValue(e.Value)})",
                        TimeActQueryMatcher.FormatValue(e.Value))));
                break;

            case ValueKind.Bool:
                DisplayValueChoices(condition, index,
                    [(LOC.Get("TAE_MassEditTool_Value_True"), TimeActQueryMatcher.FormatValue(true)),
                     (LOC.Get("TAE_MassEditTool_Value_False"), TimeActQueryMatcher.FormatValue(false))]);
                break;

            case ValueKind.NumberBool:
                DisplayValueChoices(condition, index,
                    [(LOC.Get("TAE_MassEditTool_Value_On"), "1"), (LOC.Get("TAE_MassEditTool_Value_Off"), "0")]);
                break;

            default:
                if (condition.Comparison is TimeActMassEditComparison.Between)
                {
                    var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;

                    ImGui.SetNextItemWidth(width);

                    if (ImGui.InputTextWithHint($"##massEditConditionValue{index}", LOC.Get("TAE_MassEditTool_Value_Low_Hint"),
                        ref condition.Value, 255))
                    {
                        Changed();
                    }
                    GUI.Tooltip(LOC.Get("TAE_MassEditTool_Value_Between_TT"));

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(-1);

                    if (ImGui.InputTextWithHint($"##massEditConditionValue2{index}", LOC.Get("TAE_MassEditTool_Value_High_Hint"),
                        ref condition.Value2, 255))
                    {
                        Changed();
                    }
                    GUI.Tooltip(LOC.Get("TAE_MassEditTool_Value_Between_TT"));
                }
                else
                {
                    ImGui.SetNextItemWidth(-1);

                    if (ImGui.InputTextWithHint($"##massEditConditionValue{index}", LOC.Get("TAE_MassEditTool_Any_Hint"),
                        ref condition.Value, 255))
                    {
                        Changed();
                    }
                    GUI.Tooltip(parameter != null
                        ? LOC.Get("TAE_MassEditTool_Value_Typed_TT", parameter.Type)
                        : LOC.Get("TAE_MassEditTool_Value_TT"));
                }
                break;
        }
    }

    /// <summary>
    /// A combo of the values a field can have, each with the text the value is matched by
    /// </summary>
    private void DisplayValueChoices(ConditionInput condition, int index, IEnumerable<(string Label, string Value)> choices)
    {
        var list = choices.ToList();

        var preview = condition.Value == ""
            ? LOC.Get("TAE_MassEditTool_Value_Any")
            : list.FirstOrDefault(e => e.Value == condition.Value).Label ?? condition.Value;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##massEditConditionChoice{index}", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.Selectable($"{LOC.Get("TAE_MassEditTool_Value_Any")}##massEditChoiceAny{index}", condition.Value == ""))
            {
                condition.Value = "";
                Changed();
            }

            for (int i = 0; i < list.Count; i++)
            {
                var (label, value) = list[i];

                if (ImGui.Selectable($"{label}##massEditChoice{index}_{i}", condition.Value == value))
                {
                    condition.Value = value;
                    Changed();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Value_TT"));
    }

    public static string GetComparisonName(TimeActMassEditComparison comparison) => comparison switch
    {
        TimeActMassEditComparison.Contains => LOC.Get("TAE_MassEditTool_Comparison_Contains"),
        TimeActMassEditComparison.Equals => "=",
        TimeActMassEditComparison.NotEquals => "!=",
        TimeActMassEditComparison.Less => "<",
        TimeActMassEditComparison.LessOrEqual => "<=",
        TimeActMassEditComparison.Greater => ">",
        TimeActMassEditComparison.GreaterOrEqual => ">=",
        TimeActMassEditComparison.Between => LOC.Get("TAE_MassEditTool_Comparison_Between"),
        _ => $"{comparison}"
    };

    public static string GetComparisonTooltip(TimeActMassEditComparison comparison) => comparison switch
    {
        TimeActMassEditComparison.Contains => LOC.Get("TAE_MassEditTool_Comparison_Contains_TT"),
        TimeActMassEditComparison.Equals => LOC.Get("TAE_MassEditTool_Comparison_Equals_TT"),
        TimeActMassEditComparison.NotEquals => LOC.Get("TAE_MassEditTool_Comparison_Not_Equals_TT"),
        TimeActMassEditComparison.Between => LOC.Get("TAE_MassEditTool_Comparison_Between_TT"),
        _ => LOC.Get("TAE_MassEditTool_Comparison_Number_TT")
    };
    #endregion

    #region Find
    private bool DisplayActions()
    {
        var find = false;
        var canFind = GetScopeUnavailableReason() == null;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Actions_Header"),
            LOC.Get("TAE_MassEditTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("massEditActions",
            "massEditFind",
            LOC.Get("TAE_MassEditTool_Find"),
            LOC.Get("TAE_MassEditTool_Find_TT"),
            () => find = true,
            canFind,

            "massEditClear",
            LOC.Get("TAE_MassEditTool_Clear"),
            LOC.Get("TAE_MassEditTool_Clear_TT"),
            ClearInputs,
            true);

        return find && canFind;
    }

    private void ClearInputs()
    {
        AnimationText = "";
        EventType = null;
        EventText = "";
        EventTypeFilter = "";
        Conditions = [new()];

        Changed();
        ClearResults();
    }

    private void CancelRun()
    {
        Run?.Cancel();
        Run = null;
    }

    private void ClearResults()
    {
        CancelRun();

        Results = null;
        ResultAnimations = null;
        ResultSources = null;
        ResultSource = null;
        ResultsGlobal = false;
        FoundVersion = -1;

        ClearPlan();
    }

    /// <summary>
    /// Takes what the scope covers from the selection on the main thread, then hands the filter to a background task
    /// </summary>
    private void StartFind(TimeActContainerWrapper source, TAE.Template.BankTemplate template)
    {
        ClearResults();

        var selection = View.Selection;
        var global = Scope is TimeActMassEditScope.AllSources;

        List<TimeActQuerySource> sources;

        if (global)
        {
            sources = Runner.GetGlobalSources();
        }
        else
        {
            var entries = Scope is TimeActMassEditScope.CurrentSource
                ? source.TimeActs.ToList()
                : [selection.SelectedTimeAct];

            sources = [Runner.GetLocalSource(source, entries)];
        }

        HashSet<TAE.Animation> limitAnimations = Scope is TimeActMassEditScope.SelectedEvents or TimeActMassEditScope.CurrentAnimation
            ? [selection.SelectedAnimation]
            : null;

        HashSet<TAE.Event> limitEvents = Scope is TimeActMassEditScope.SelectedEvents
            ? selection.SelectedEvents.ToHashSet()
            : null;

        var filter = new TimeActMassEditFilter
        {
            Target = Target,
            Animation = AnimationText.Trim(),
            EventType = EventType,
            EventText = EventText.Trim(),
            Conditions = Conditions.Select(e => e.ToCondition()).ToList(),
            Exact = Exact,
            Templates = sources.GroupBy(e => e.Source).ToDictionary(e => e.Key, e => e.First().Template),
            LimitAnimations = limitAnimations,
            LimitEvents = limitEvents
        };

        Results = new();
        ResultAnimations = new();
        ResultSources = new();
        ResultSource = source;
        ResultsGlobal = global;
        ResultTarget = Target;
        ShowSourceInLabel = global;
        ShowTimeActInLabel = Scope is TimeActMassEditScope.CurrentSource or TimeActMassEditScope.AllSources;

        FoundVersion = InputsVersion;
        FoundTimeAct = selection.SelectedTimeAct;
        FoundAnimation = selection.SelectedAnimation;
        FoundEvents = selection.SelectedEvents.ToArray();

        // Every match is kept, as the edit is applied to all of them. Across all sources, the time acts read for
        // the sources that aren't open are kept too, as the edit is made to those copies.
        Run = Runner.Start(filter, sources, !global, 0, keepReadEntries: global);
    }

    /// <summary>
    /// Hands the time acts read for each unopened source that had a match over to the source, so the edit is made
    /// to what the matches point at, and saving writes it. The whole list is handed over: saving removes the files
    /// of time acts that aren't in it. A source opened since Find already has its own, and the matches are looked
    /// up again in those.
    /// </summary>
    private void AdoptReadSources()
    {
        if (Run == null || !Run.KeepReadEntries)
            return;

        foreach (var (source, entries) in Run.ReadEntries)
        {
            if (source.TimeActs.Count == 0)
            {
                source.TimeActs = entries;
            }
        }
    }

    /// <summary>
    /// Whether the matches no longer show what the inputs ask for: the inputs have changed, or the selection that
    /// the scope covers has
    /// </summary>
    private bool IsStale()
    {
        if (FoundVersion != InputsVersion)
            return true;

        var selection = View.Selection;

        return Scope switch
        {
            TimeActMassEditScope.SelectedEvents =>
                selection.SelectedAnimation != FoundAnimation || !selection.SelectedEvents.SequenceEqual(FoundEvents),
            TimeActMassEditScope.CurrentAnimation => selection.SelectedAnimation != FoundAnimation,
            TimeActMassEditScope.CurrentTimeAct => selection.SelectedTimeAct != FoundTimeAct,
            _ => false
        };
    }
    #endregion

    #region Results
    /// <summary>
    /// Puts the time act in front of a label when the matches span time acts, and the source when they span sources
    /// </summary>
    private string PrefixLabel(TimeActQueryMatch match, string label)
    {
        if (ShowTimeActInLabel)
            label = $"{match.EntryName}  {label}";

        if (ShowSourceInLabel)
            label = $"{match.Source.Name}  {label}";

        return label;
    }

    /// <summary>
    /// How many matches there are, and in how many animations and sources
    /// </summary>
    private string GetMatchCountText()
    {
        var text = ResultTarget is TimeActMassEditTarget.Events
            ? LOC.Get("TAE_MassEditTool_Matches_Events", Results.Count, ResultAnimations.Count)
            : LOC.Get("TAE_MassEditTool_Matches_Animations", Results.Count);

        if (ResultsGlobal)
        {
            text = $"{text} {LOC.Get("TAE_MassEditTool_Matches_Sources", ResultSources.Count)}";
        }

        return text;
    }

    /// <summary>
    /// Picks up what has been found so far
    /// </summary>
    private void PickUpResults()
    {
        var run = Run;

        if (run == null)
            return;

        while (run.Found.TryDequeue(out var match))
        {
            if (Results == null)
                continue;

            Results.Add(new ResultRow(match, PrefixLabel(match, match.Label)));
            ResultAnimations.Add(match.Animation);
            ResultSources.Add(match.Source);
        }
    }

    /// <summary>
    /// Shows how far the find has got
    /// </summary>
    private void DisplayProgress()
    {
        var run = Run;

        if (run == null || run.Done)
            return;

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Progress_Header"),
            LOC.Get("TAE_MassEditTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get(ResultsGlobal ? "TAE_MassEditTool_Progress_Sources" : "TAE_MassEditTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Finding", run.Status));

        GUI.MultiButtonInput("massEditProgressActions",
            "massEditCancel",
            LOC.Get("TAE_MassEditTool_Cancel"),
            LOC.Get("TAE_MassEditTool_Cancel_TT"),
            CancelRun);
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        // Matches
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("TAE_MassEditTool_Matches_Header"),
            LOC.Get("TAE_MassEditTool_Matches_Header_TT"),
            ref ShowMatchesList);

        var countText = GetMatchCountText();

        var filtered = GetFilteredResults();

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("TAE_MassEditTool_Matches_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (IsStale())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Matches_Stale"));
        }

        // The counts stay on show when the list is hidden
        if (Results.Count == 0 || !ShowMatchesList)
            return;

        EditorFilters.DisplayFramedListFilter("massEditResultFilter", ref ResultFilter, ref ExactResultFilter);

        // The list is the last thing in its tab, so it takes the rest of the height
        ImGui.BeginChild("##massEditResults", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        var selection = View.Selection;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (match, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##massEditJump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("TAE_MassEditTool_Jump_TT"));

                ImGui.SameLine();

                var selected = selection.SelectedAnimation == match.Animation &&
                    (match.Event == null || selection.IsEventSelected(match.Event));

                if (ImGui.Selectable($"{label}##massEditResult{index}", selected))
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

    /// <summary>
    /// The positions of the results that match the filter. The results arrive while the search runs,
    /// so only the new ones are checked, unless the filter has changed.
    /// </summary>
    private List<int> GetFilteredResults()
    {
        if (FilteredResults == null || FilteredResultsFor != Results ||
            CachedResultFilter != ResultFilter || CachedExactResultFilter != ExactResultFilter ||
            CheckedResultCount > Results.Count)
        {
            FilteredResults = new();
            FilteredResultsFor = Results;
            CachedResultFilter = ResultFilter;
            CachedExactResultFilter = ExactResultFilter;
            CheckedResultCount = 0;
        }

        for (int i = CheckedResultCount; i < Results.Count; i++)
        {
            if (EditorFilters.IsMatch(ResultFilter, Results[i].Label, ExactResultFilter))
            {
                FilteredResults.Add(i);
            }
        }

        CheckedResultCount = Results.Count;

        return FilteredResults;
    }
    #endregion
}
