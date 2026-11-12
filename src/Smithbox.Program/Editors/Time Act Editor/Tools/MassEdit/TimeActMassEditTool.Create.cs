using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The create half of the mass edit: one new event in each matched animation, or one next to each matched event.
/// The new events start from their defaults and take any parameters the steps give them, and go in as one action.
/// </summary>
public partial class TimeActMassEditTool
{
    private record CreateOutcome(int Created, int Animations, int Stale);

    private int? CreateEventType = null;
    private string CreateEventFilter = "";
    private TimeActMassEditPlacement Placement = TimeActMassEditPlacement.InEachAnimation;
    private string CreateStartText = "0";
    private string CreateEndText = "1";
    private bool CreateFrames = true;
    private bool CreateSkipExisting = true;
    private List<StepInput> CreateSteps = [new()];

    private TimeActMassCreatePlan CreatePlan;
    private int CreatePlanVersion = -1;
    private List<ResultRow> CreatePlanFor;
    private int CreatePlanForCount;
    private TAE.Template.EventTemplate CreatePlanTemplate;

    // The label of each new event, made the first time it is drawn or filtered
    private string[] CreateLabels;

    private string CreateFilter = "";
    private bool ExactCreateFilter = false;
    private List<int> FilteredCreates;
    private TimeActMassCreatePlan FilteredCreatesFor;
    private string CachedCreateFilter = "";
    private bool CachedExactCreateFilter = false;

    // Set once the events have been created, so they can't be created a second time
    private CreateOutcome CreateResult;

    private void ClearCreatePlan()
    {
        CreatePlan = null;
        CreatePlanFor = null;
        CreatePlanVersion = -1;
        CreateLabels = null;
        FilteredCreates = null;
        CreateResult = null;

        ClearDuplicatePlan();
    }

    private void DisplayCreate(TAE.Template.BankTemplate template)
    {
        if (template == null)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_No_Template"));
            return;
        }

        // Animations can be copied rather than given events
        if (DisplayCreateKind())
            return;

        // Animations only take new events at given times, as there are no events to place them by
        if (ResultTarget is TimeActMassEditTarget.Animations && Placement is not TimeActMassEditPlacement.InEachAnimation)
        {
            Placement = TimeActMassEditPlacement.InEachAnimation;
            StepsChanged();
        }

        // New Event
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Create_Event_Header"),
            LOC.Get("TAE_MassEditTool_Create_Event_Header_TT"));

        DisplayCreateEventPicker(template);

        var eventTemplate = CreateEventType is { } type && template.TryGetValue(type, out var found) ? found : null;

        // Placement
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Placement_Header"),
            LOC.Get("TAE_MassEditTool_Placement_Header_TT"));

        DisplayPlacement();

        // Times
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Create_Times_Header"),
            LOC.Get(Placement is TimeActMassEditPlacement.InEachAnimation
                ? "TAE_MassEditTool_Create_Times_Header_TT"
                : "TAE_MassEditTool_Create_Offsets_Header_TT"));

        DisplayCreateTimes();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Options_Header"),
            LOC.Get("TAE_MassEditTool_Create_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_MassEditTool_Create_Skip_Existing")}##massEditCreateSkipExisting", ref CreateSkipExisting))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get(Placement is TimeActMassEditPlacement.InEachAnimation
            ? "TAE_MassEditTool_Create_Skip_Existing_Type_TT"
            : "TAE_MassEditTool_Create_Skip_Existing_Same_TT"));

        if (eventTemplate == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Create_Pick_Event"));
            return;
        }

        // Parameters
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Create_Parameters_Header"),
            LOC.Get("TAE_MassEditTool_Create_Parameters_Header_TT"));

        DisplaySteps(CreateSteps, GetCreateFields(eventTemplate));

        UpdateCreatePlan(eventTemplate);

        DisplayCreations(template, eventTemplate);
    }

    #region Inputs
    private void DisplayCreateEventPicker(TAE.Template.BankTemplate template)
    {
        var preview = CreateEventType is { } type
            ? template.TryGetValue(type, out var picked) ? $"[{type}] {picked.Name}" : $"[{type}]"
            : LOC.Get("TAE_MassEditTool_Create_Pick_Event_Type");

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditCreateEventType", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##massEditCreateEventFilter", LOC.Get("TAE_MassEditTool_Event_Filter_Hint"), ref CreateEventFilter, 255);

            var filter = CreateEventFilter.Trim();

            foreach (var eventTemplate in template.Values.OrderBy(e => e.ID))
            {
                var name = $"[{eventTemplate.ID}] {eventTemplate.Name}";

                if (filter != "" && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{name}##massEditCreateEvent{eventTemplate.ID}", CreateEventType == eventTemplate.ID))
                {
                    if (CreateEventType != eventTemplate.ID)
                    {
                        CreateEventType = eventTemplate.ID;
                        CreateEventFilter = "";

                        // The parameters belong to the old type
                        CreateSteps = [new()];
                        StepsChanged();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Create_Event_Header_TT"));
    }

    private void DisplayPlacement()
    {
        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditPlacement", GetPlacementName(Placement)))
        {
            foreach (var placement in Enum.GetValues<TimeActMassEditPlacement>())
            {
                if (ResultTarget is TimeActMassEditTarget.Animations && placement is not TimeActMassEditPlacement.InEachAnimation)
                    continue;

                if (ImGui.Selectable($"{GetPlacementName(placement)}##massEditPlacement{placement}", Placement == placement))
                {
                    if (Placement != placement)
                    {
                        Placement = placement;

                        // Offsets start from the event's own times, while set times start at the beginning
                        CreateStartText = "0";
                        CreateEndText = placement is TimeActMassEditPlacement.InEachAnimation ? "1" : "0";

                        StepsChanged();
                    }
                }
                GUI.Tooltip(GetPlacementTooltip(placement));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(GetPlacementTooltip(Placement));
    }

    private static string GetPlacementName(TimeActMassEditPlacement placement) => placement switch
    {
        TimeActMassEditPlacement.InEachAnimation => LOC.Get("TAE_MassEditTool_Placement_Each_Animation"),
        TimeActMassEditPlacement.AlongsideEachEvent => LOC.Get("TAE_MassEditTool_Placement_Each_Event"),
        _ => $"{placement}"
    };

    private static string GetPlacementTooltip(TimeActMassEditPlacement placement) => placement switch
    {
        TimeActMassEditPlacement.InEachAnimation => LOC.Get("TAE_MassEditTool_Placement_Each_Animation_TT"),
        TimeActMassEditPlacement.AlongsideEachEvent => LOC.Get("TAE_MassEditTool_Placement_Each_Event_TT"),
        _ => ""
    };

    private void DisplayCreateTimes()
    {
        var isOffset = Placement is TimeActMassEditPlacement.AlongsideEachEvent;

        var framesWidth = ImGui.CalcTextSize(LOC.Get("TAE_MassEditTool_Step_Frames")).X + ImGui.GetFrameHeight() +
            ImGui.GetStyle().ItemInnerSpacing.X + ImGui.GetStyle().ItemSpacing.X;

        var width = (ImGui.GetContentRegionAvail().X - framesWidth - ImGui.GetStyle().ItemSpacing.X) / 2;

        ImGui.SetNextItemWidth(width);

        if (ImGui.InputTextWithHint("##massEditCreateStart",
            LOC.Get(isOffset ? "TAE_MassEditTool_Create_Start_Offset_Hint" : "TAE_MassEditTool_Create_Start_Hint"),
            ref CreateStartText, 64))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get(isOffset ? "TAE_MassEditTool_Create_Start_Offset_TT" : "TAE_MassEditTool_Create_Start_TT"));

        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);

        if (ImGui.InputTextWithHint("##massEditCreateEnd",
            LOC.Get(isOffset ? "TAE_MassEditTool_Create_End_Offset_Hint" : "TAE_MassEditTool_Create_End_Hint"),
            ref CreateEndText, 64))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get(isOffset ? "TAE_MassEditTool_Create_End_Offset_TT" : "TAE_MassEditTool_Create_End_TT"));

        ImGui.SameLine();

        if (ImGui.Checkbox($"{LOC.Get("TAE_MassEditTool_Step_Frames")}##massEditCreateFrames", ref CreateFrames))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Step_Frames_TT"));
    }

    /// <summary>
    /// The times as given, in seconds. Empty counts as zero.
    /// </summary>
    private bool TryGetCreateTimes(out float start, out float end)
    {
        start = 0;
        end = 0;

        if (!TryParseTime(CreateStartText, out start) || !TryParseTime(CreateEndText, out end))
            return false;

        return true;
    }

    private bool TryParseTime(string text, out float time)
    {
        time = 0;

        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (!TimeActMassEditFilter.TryParseNumber(text.Trim(), out var value))
            return false;

        time = (float)(CreateFrames ? value * TimeActMassEditFields.FrameDuration : value);

        return true;
    }

    /// <summary>
    /// The parameters of the new event type, which the steps can set or change from their defaults
    /// </summary>
    private List<EditField> GetCreateFields(TAE.Template.EventTemplate eventTemplate)
    {
        var data = Project.Handler.TimeActData;
        var fields = new List<EditField>();

        foreach (var parameter in eventTemplate.Values)
        {
            if (parameter.ValueToAssert != null)
                continue;

            var meta = data.GetFieldMeta(eventTemplate, parameter.Name);
            var kind = TimeActMassEditFields.GetKind(parameter.Name, parameter, meta);

            // Every new event has every field, so none are marked as only in some
            fields.Add(new(parameter.Name, data.GetFieldDisplayName(eventTemplate, parameter.Name), kind, parameter, Results.Count));
        }

        return fields;
    }
    #endregion

    #region Creations
    private void UpdateCreatePlan(TAE.Template.EventTemplate eventTemplate)
    {
        if (HasApplied)
            return;

        if (CreatePlan != null && CreatePlanVersion == StepsVersion && CreatePlanFor == Results &&
            CreatePlanForCount == Results.Count && CreatePlanTemplate == eventTemplate)
        {
            return;
        }

        CreatePlanVersion = StepsVersion;
        CreatePlanFor = Results;
        CreatePlanForCount = Results.Count;
        CreatePlanTemplate = eventTemplate;
        CreateLabels = null;
        FilteredCreates = null;

        if (!TryGetCreateTimes(out var start, out var end))
        {
            CreatePlan = null;
            return;
        }

        var settings = new TimeActCreateSettings(eventTemplate, Placement, start, end, CreateSkipExisting);

        CreatePlan = Planner.PlanCreate(Results.Select(e => e.Match).ToList(), settings,
            CreateSteps.Select(e => e.ToStep()).ToList(), TemplateFor);

        CreateLabels = new string[CreatePlan.Creates.Count];
    }

    private void DisplayCreations(TAE.Template.BankTemplate template, TAE.Template.EventTemplate eventTemplate)
    {
        // New Events
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("TAE_MassEditTool_Creations_Header"),
            LOC.Get("TAE_MassEditTool_Creations_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (CreatePlan == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Create_Invalid_Times"));
            return;
        }

        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Create_Summary", CreatePlan.Creates.Count, CreatePlan.AnimationCount));

        DisplaySkips(CreatePlan.Skips, template);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Changes_Stale"));
        }

        // Create
        var create = false;

        GUI.ConditionalMultiButtonInput("massEditCreateActions",
            "massEditCreate",
            LOC.Get("TAE_MassEditTool_Create"),
            LOC.Get("TAE_MassEditTool_Create_TT"),
            () => create = true,
            !stale && CreatePlan.Creates.Count > 0);

        // Creating drops the plan, so the rest of the section waits for the next frame
        if (create)
        {
            ApplyCreate();
            return;
        }

        if (CreatePlan.Creates.Count == 0 || !ShowChangesList)
            return;

        EditorFilters.DisplayFramedListFilter("massEditCreateFilter", ref CreateFilter, ref ExactCreateFilter);

        var filtered = GetFilteredCreates(template, eventTemplate);

        ImGui.BeginChild("##massEditCreations", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var planned = CreatePlan.Creates[index];

                if (ImGui.Button($"{Icons.LocationArrow}##massEditCreateJump{index}"))
                {
                    jumpTarget = planned.Match;
                }
                GUI.Tooltip(LOC.Get("TAE_MassEditTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{GetCreateLabel(index, template, eventTemplate)}##massEditCreation{index}",
                    View.Selection.SelectedAnimation == planned.Animation))
                {
                    jumpTarget = planned.Match;
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

    private string GetCreateLabel(int index, TAE.Template.BankTemplate template, TAE.Template.EventTemplate eventTemplate)
    {
        if (CreateLabels[index] != null)
            return CreateLabels[index];

        var planned = CreatePlan.Creates[index];

        // Next to an event, the event it goes by is named; in an animation, just the animation
        var where = Placement is TimeActMassEditPlacement.AlongsideEachEvent
            ? GetMatchLabel(planned.Match, template)
            : GetMatchLabel(planned.Match with { Event = null, EventIndex = -1 }, template);

        var ev = planned.Event;
        var label = $"{where}  + [{eventTemplate.ID}] {eventTemplate.Name}  " +
            $"{TimeActMassEditPlanner.FormatTime(ev.StartTime)} -> {TimeActMassEditPlanner.FormatTime(ev.EndTime)}";

        CreateLabels[index] = label;

        return label;
    }

    private List<int> GetFilteredCreates(TAE.Template.BankTemplate template, TAE.Template.EventTemplate eventTemplate)
    {
        if (FilteredCreates != null && FilteredCreatesFor == CreatePlan &&
            CachedCreateFilter == CreateFilter && CachedExactCreateFilter == ExactCreateFilter)
        {
            return FilteredCreates;
        }

        var filtered = new List<int>();

        for (int i = 0; i < CreatePlan.Creates.Count; i++)
        {
            if (CreateFilter == "" || EditorFilters.IsMatch(CreateFilter, GetCreateLabel(i, template, eventTemplate), ExactCreateFilter))
            {
                filtered.Add(i);
            }
        }

        FilteredCreates = filtered;
        FilteredCreatesFor = CreatePlan;
        CachedCreateFilter = CreateFilter;
        CachedExactCreateFilter = ExactCreateFilter;

        return filtered;
    }

    /// <summary>
    /// Each animation is looked up again first, as is the event a new one goes next to. One that has gone since,
    /// or that now has the event already when that is asked to be skipped, is left out and counted as stale.
    /// </summary>
    private void ApplyCreate()
    {
        if (CreatePlan == null || IsStale())
            return;

        AdoptReadSources();

        var groups = new Dictionary<(TimeActEntry, TAE.Animation), List<TAE.Event>>();
        var stale = 0;

        foreach (var planned in CreatePlan.Creates)
        {
            var match = Placement is TimeActMassEditPlacement.AlongsideEachEvent
                ? planned.Match
                : planned.Match with { Event = null, EventIndex = -1 };

            if (!TimeActQueryRunner.TryResolve(match, out var entry, out var animation, out _))
            {
                stale++;
                continue;
            }

            if (CreateSkipExisting &&
                TimeActMassEditPlanner.HasExisting(animation, planned.Event.Type, Placement, planned.Event.StartTime, planned.Event.EndTime))
            {
                stale++;
                continue;
            }

            if (!groups.TryGetValue((entry, animation), out var events))
            {
                events = new();
                groups[(entry, animation)] = events;
            }

            events.Add(planned.Event);
        }

        var action = new TimeActMassAddEventsAction(View.Selection,
            groups.Select(e => (e.Key.Item1, e.Key.Item2, e.Value)), Project.Handler.TimeActData.GetGroupMap());

        var created = action.EventCount;

        if (created > 0)
        {
            View.ViewportActionManager.ExecuteAction(action);
        }

        Smithbox.Log(this, LOC.Get("TAE_MassEditTool_Created_Log", created, groups.Count, stale));

        CreatePlan = null;
        CreateLabels = null;
        FilteredCreates = null;
        CreateResult = new CreateOutcome(created, groups.Count, stale);
    }
    #endregion
}
