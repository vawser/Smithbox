using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A parameter an edit changes, with the key it is stored under in the event
/// </summary>
public record TimeActPlannedParameter(string Key, TAE.Template.ParameterTemplate Parameter, object OldValue, object NewValue);

/// <summary>
/// An event an edit changes, with the values it had when the edit was planned, so the applier can tell
/// whether it has been changed since
/// </summary>
public class TimeActPlannedEvent
{
    public TimeActQueryMatch Match;

    public float OldStart;
    public float OldEnd;
    public float NewStart;
    public float NewEnd;

    public List<TimeActPlannedParameter> Parameters = new();

    public bool TimesChanged => OldStart != NewStart || OldEnd != NewEnd;
}

/// <summary>
/// An animation an edit changes: each property, with the value it had when the edit was planned
/// </summary>
public class TimeActPlannedAnimation
{
    public TimeActQueryMatch Match;

    public List<(TimeActAnimationField Field, object OldValue, object NewValue)> Values = new();
}

/// <summary>
/// One line of the preview: one field of one event or animation, from its old value to its new one
/// </summary>
public record TimeActPlannedChange(TimeActQueryMatch Match, string Field, string OldText, string NewText);

/// <summary>
/// A match the edit can't be applied to, and why
/// </summary>
public record TimeActPlannedSkip(TimeActQueryMatch Match, string Reason);

/// <summary>
/// Everything an edit will do, worked out before anything is changed. The preview shows it and the applier
/// carries it out, so the two can't disagree.
/// </summary>
public class TimeActMassEditPlan
{
    public List<TimeActPlannedEvent> Events = new();
    public List<TimeActPlannedAnimation> Animations = new();
    public List<TimeActPlannedChange> Changes = new();

    /// <summary>
    /// How many events or animations the edit changes
    /// </summary>
    public int TargetCount => Events.Count + Animations.Count;
    public List<TimeActPlannedSkip> Skips = new();

    /// <summary>
    /// Matches the edit leaves as they are, as they already have the values it would give them
    /// </summary>
    public int Unchanged;

    public int AnimationCount;
}

/// <summary>
/// Where new events go
/// </summary>
public enum TimeActMassEditPlacement
{
    /// <summary>
    /// One event in each animation among the matches, at the given times
    /// </summary>
    InEachAnimation,

    /// <summary>
    /// One event next to each matched event, at its times moved by the given offsets
    /// </summary>
    AlongsideEachEvent
}

/// <summary>
/// What to create. The times are in seconds; for <see cref="TimeActMassEditPlacement.AlongsideEachEvent"/> they
/// are offsets from the start and end of each matched event.
/// </summary>
public record TimeActCreateSettings(TAE.Template.EventTemplate EventTemplate, TimeActMassEditPlacement Placement,
    float Start, float End, bool SkipExisting);

/// <summary>
/// An event to add, made ready with its parameters when the plan is made
/// </summary>
public record TimeActPlannedCreate(TimeActQueryMatch Match, TimeActEntry Entry, TAE.Animation Animation, TAE.Event Event);

/// <summary>
/// A copy of an animation to add, under its new ID and file name. The copy itself is made when it is applied,
/// so it holds the events the animation has then.
/// </summary>
public record TimeActPlannedDuplicate(TimeActQueryMatch Match, long NewID, string FileName);

public class TimeActMassDuplicatePlan
{
    public List<TimeActPlannedDuplicate> Duplicates = new();
    public List<TimeActPlannedSkip> Skips = new();
}

public class TimeActMassCreatePlan
{
    public List<TimeActPlannedCreate> Creates = new();
    public List<TimeActPlannedSkip> Skips = new();

    public int AnimationCount;
}

/// <summary>
/// Works out what an edit does to each match. Nothing is changed: events without their template applied are
/// read through copies, which are cached for as long as the matches are kept.
/// </summary>
public class TimeActMassEditPlanner
{
    public ProjectEntry Project;

    public TimeActMassEditPlanner(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// The state of an event as the steps go through it
    /// </summary>
    private class WorkingEvent
    {
        public float Start;
        public float End;
        public Dictionary<string, (string Key, TAE.Template.ParameterTemplate Parameter, object Old, object Current)> Parameters = new();
    }

    public TimeActMassEditPlan Plan(IReadOnlyList<TimeActQueryMatch> matches, IReadOnlyList<TimeActMassEditStep> steps,
        Func<TimeActContainerWrapper, TAE.Template.BankTemplate> templateFor, Dictionary<TAE.Event, TAE.Event> readableCache)
    {
        var plan = new TimeActMassEditPlan();
        var activeSteps = steps.Where(e => e.HasField).ToList();

        if (activeSteps.Count == 0)
            return plan;

        var animations = new HashSet<TAE.Animation>();

        foreach (var match in matches)
        {
            // A match without an event is an animation, whose properties are edited
            if (match.Event == null)
            {
                if (PlanAnimation(plan, match, activeSteps))
                {
                    animations.Add(match.Animation);
                }

                continue;
            }

            var original = match.Event;

            if (!readableCache.TryGetValue(original, out var readable))
            {
                readable = match.Entry.GetReadableEvent(original, templateFor(match.Source));
                readableCache[original] = readable;
            }

            var working = new WorkingEvent { Start = original.StartTime, End = original.EndTime };

            string reason = null;

            foreach (var step in activeSteps)
            {
                reason = TimeActMassEditFields.IsTimeField(step.Field)
                    ? ApplyTimeStep(step, working)
                    : ApplyParameterStep(step, readable, working);

                if (reason != null)
                    break;
            }

            if (reason == null && working.End < working.Start)
            {
                reason = LOC.Get("TAE_MassEditTool_Skip_End_Before_Start");
            }

            // An event is changed by every step or by none, so it is never left half edited
            if (reason != null)
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, reason));
                continue;
            }

            var planned = new TimeActPlannedEvent
            {
                Match = match,
                OldStart = original.StartTime,
                OldEnd = original.EndTime,
                NewStart = working.Start,
                NewEnd = working.End
            };

            foreach (var (name, (key, parameter, old, current)) in working.Parameters)
            {
                if (!Equals(old, current))
                {
                    planned.Parameters.Add(new TimeActPlannedParameter(key, parameter, old, current));
                }
            }

            if (!planned.TimesChanged && planned.Parameters.Count == 0)
            {
                plan.Unchanged++;
                continue;
            }

            plan.Events.Add(planned);
            animations.Add(match.Animation);

            AddChanges(plan, planned, readable);
        }

        plan.AnimationCount = animations.Count;

        return plan;
    }

    private void AddChanges(TimeActMassEditPlan plan, TimeActPlannedEvent planned, TAE.Event readable)
    {
        if (planned.OldStart != planned.NewStart)
        {
            plan.Changes.Add(new TimeActPlannedChange(planned.Match, LOC.Get("TAE_PropertiesWindow_Start_Time"),
                FormatTime(planned.OldStart), FormatTime(planned.NewStart)));
        }

        if (planned.OldEnd != planned.NewEnd)
        {
            plan.Changes.Add(new TimeActPlannedChange(planned.Match, LOC.Get("TAE_PropertiesWindow_End_Time"),
                FormatTime(planned.OldEnd), FormatTime(planned.NewEnd)));
        }

        var data = Project.Handler.TimeActData;

        foreach (var parameter in planned.Parameters)
        {
            plan.Changes.Add(new TimeActPlannedChange(planned.Match,
                data.GetFieldDisplayName(readable, parameter.Parameter.Name),
                FormatParameter(parameter.Parameter, parameter.OldValue),
                FormatParameter(parameter.Parameter, parameter.NewValue)));
        }
    }

    #region Animations
    /// <summary>
    /// Works out the steps for an animation's properties. Returns true if the animation is changed.
    /// As with events, the animation takes every step or none.
    /// </summary>
    private bool PlanAnimation(TimeActMassEditPlan plan, TimeActQueryMatch match, List<TimeActMassEditStep> steps)
    {
        var animation = match.Animation;
        var values = new Dictionary<string, (TimeActAnimationField Field, object Old, object Current)>();

        foreach (var step in steps)
        {
            var field = TimeActAnimationFields.Find(step.Field);

            if (field == null)
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_No_Field", step.Field)));
                return false;
            }

            if (!values.TryGetValue(field.Name, out var state))
            {
                var value = field.Get(animation);

                // The mini header decides which properties the animation has
                if (value == null)
                {
                    plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_No_Animation_Field", LOC.Get(field.LabelKey))));
                    return false;
                }

                state = (field, value, value);
            }

            if (!TryApplyAnimationStep(step, field, state.Current, out var result, out var reason))
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, reason));
                return false;
            }

            values[field.Name] = (field, state.Old, result);
        }

        var planned = new TimeActPlannedAnimation { Match = match };

        foreach (var (field, old, current) in values.Values)
        {
            if (!Equals(old, current))
            {
                planned.Values.Add((field, old, current));
            }
        }

        if (planned.Values.Count == 0)
        {
            plan.Unchanged++;
            return false;
        }

        plan.Animations.Add(planned);

        foreach (var (field, old, current) in planned.Values)
        {
            plan.Changes.Add(new TimeActPlannedChange(match, LOC.Get(field.LabelKey),
                FormatAnimationValue(old), FormatAnimationValue(current)));
        }

        return true;
    }

    private static bool TryApplyAnimationStep(TimeActMassEditStep step, TimeActAnimationField field, object current,
        out object result, out string reason)
    {
        result = null;
        reason = null;

        switch (step.Operator)
        {
            case TimeActMassEditOperator.Set:
                switch (field.Kind)
                {
                    case TimeActMassEditFieldKind.Text:
                        result = step.Value ?? "";
                        return true;

                    case TimeActMassEditFieldKind.Bool:
                        if (bool.TryParse(step.Value, out var flag))
                        {
                            result = flag;
                            return true;
                        }

                        reason = LOC.Get("TAE_MassEditTool_Skip_Invalid_Value", step.Value, "bool");
                        return false;
                }
                break;

            case TimeActMassEditOperator.Toggle:
                if (current is bool value)
                {
                    result = !value;
                    return true;
                }

                reason = LOC.Get("TAE_MassEditTool_Skip_Invalid_Operator");
                return false;
        }

        // Set on a number, and the arithmetic
        if (!TryToNumber(current, out var number))
        {
            reason = LOC.Get("TAE_MassEditTool_Skip_Not_Number");
            return false;
        }

        if (!TryParseNumber(step.Value, out var operand))
        {
            reason = LOC.Get("TAE_MassEditTool_Skip_Invalid_Number", step.Value);
            return false;
        }

        if (!TryApplyNumber(step.Operator, number, operand, out var computed, out reason))
            return false;

        if (!TryFromNumber(computed, current, out result))
        {
            reason = LOC.Get("TAE_MassEditTool_Skip_Out_Of_Range", FormatNumber(computed), current.GetType().Name);
            return false;
        }

        return true;
    }

    private static string FormatAnimationValue(object value) => value switch
    {
        string text => $"\"{text}\"",
        _ => TimeActQueryMatcher.FormatValue(value)
    };
    #endregion

    #region Duplicate
    /// <summary>
    /// Works out the new ID of a copy of each matched animation: its ID plus the offset. An ID already in use,
    /// by the time act or by another copy, is skipped, or moved on to the next free one if asked.
    /// </summary>
    public TimeActMassDuplicatePlan PlanDuplicates(IReadOnlyList<TimeActQueryMatch> matches, long offset, bool useNextFree)
    {
        var plan = new TimeActMassDuplicatePlan();
        var used = new Dictionary<TimeActEntry, HashSet<long>>();

        foreach (var match in matches.GroupBy(e => (e.Entry, e.Animation)).Select(e => e.First()))
        {
            var entry = match.Entry;

            if (entry.TimeAct == null)
                continue;

            if (!used.TryGetValue(entry, out var ids))
            {
                ids = entry.TimeAct.Animations.Select(e => e.ID).ToHashSet();
                used[entry] = ids;
            }

            var id = match.Animation.ID + offset;

            if (id < 0)
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_Negative_ID", id)));
                continue;
            }

            if (ids.Contains(id))
            {
                if (!useNextFree)
                {
                    plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_ID_In_Use")));
                    continue;
                }

                while (ids.Contains(id))
                {
                    id++;
                }
            }

            ids.Add(id);

            var fileName = TimeActAnimationSelectionWindow.GetAnimFileName(match.Source, entry, match.Animation, id,
                match.Animation.AnimFileName);

            plan.Duplicates.Add(new TimeActPlannedDuplicate(match, id, fileName));
        }

        return plan;
    }
    #endregion

    #region Create
    /// <summary>
    /// Makes the events to add. Each starts from the template's defaults, then takes the value the meta gives
    /// as usual for each field, then the steps, which can set or change any parameter.
    /// </summary>
    public TimeActMassCreatePlan PlanCreate(IReadOnlyList<TimeActQueryMatch> matches, TimeActCreateSettings settings,
        IReadOnlyList<TimeActMassEditStep> steps, Func<TimeActContainerWrapper, TAE.Template.BankTemplate> templateFor)
    {
        var plan = new TimeActMassCreatePlan();
        var eventTemplate = settings.EventTemplate;

        if (eventTemplate == null)
            return plan;

        var activeSteps = steps.Where(e => e.HasField).ToList();

        // One event to each animation, or one to each matched event
        var targets = settings.Placement is TimeActMassEditPlacement.InEachAnimation
            ? matches.GroupBy(e => (e.Entry, e.Animation)).Select(e => e.First()).ToList()
            : matches.Where(e => e.Event != null).ToList();

        var animations = new HashSet<TAE.Animation>();
        var planned = new HashSet<(TAE.Animation, float, float)>();

        foreach (var match in targets)
        {
            var animation = match.Animation;
            var timeAct = match.Entry.TimeAct;

            if (timeAct == null)
                continue;

            // The event type is picked from one template, which a source with templates of its own may not share
            if (templateFor(match.Source) is not { } sourceTemplate ||
                !sourceTemplate.TryGetValue(eventTemplate.ID, out var sourceEventTemplate) || sourceEventTemplate != eventTemplate)
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_Other_Templates")));
                continue;
            }

            float start, end;

            if (settings.Placement is TimeActMassEditPlacement.AlongsideEachEvent)
            {
                start = match.Event.StartTime + settings.Start;
                end = match.Event.EndTime + settings.End;
            }
            else
            {
                start = settings.Start;
                end = settings.End;
            }

            if (end < start)
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_End_Before_Start")));
                continue;
            }

            // Two matched events at the same times would otherwise each get the same new event
            if (settings.SkipExisting && !planned.Add((animation, start, end)))
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get("TAE_MassEditTool_Skip_Has_Same_Event")));
                continue;
            }

            if (settings.SkipExisting && HasExisting(animation, eventTemplate.ID, settings.Placement, start, end))
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, LOC.Get(settings.Placement is TimeActMassEditPlacement.InEachAnimation
                    ? "TAE_MassEditTool_Skip_Has_Type"
                    : "TAE_MassEditTool_Skip_Has_Same_Event")));
                continue;
            }

            if (!TryCreateEvent(animation, timeAct.BigEndian, eventTemplate, start, end, activeSteps, out var ev, out var reason))
            {
                plan.Skips.Add(new TimeActPlannedSkip(match, reason));
                continue;
            }

            plan.Creates.Add(new TimeActPlannedCreate(match, match.Entry, animation, ev));
            animations.Add(animation);
        }

        plan.AnimationCount = animations.Count;

        return plan;
    }

    /// <summary>
    /// Whether the animation already has the event: any of the type when adding one to each animation, or one of
    /// the type at the same times when adding one next to each event
    /// </summary>
    public static bool HasExisting(TAE.Animation animation, int type, TimeActMassEditPlacement placement, float start, float end)
    {
        return placement is TimeActMassEditPlacement.InEachAnimation
            ? animation.Events.Any(e => e.Type == type)
            : animation.Events.Any(e => e.Type == type && e.StartTime == start && e.EndTime == end);
    }

    private bool TryCreateEvent(TAE.Animation animation, bool bigEndian, TAE.Template.EventTemplate eventTemplate,
        float start, float end, List<TimeActMassEditStep> steps, out TAE.Event ev, out string reason)
    {
        reason = null;

        // The meaning of this field is unknown, so go by how the type is used elsewhere in the animation,
        // as the graph does when it adds an event
        var unk04 = animation.Events.FirstOrDefault(e => e.Type == eventTemplate.ID)?.Unk04 ?? 0;

        ev = new TAE.Event(start, end, eventTemplate.ID, unk04, bigEndian, eventTemplate);

        if (ev.Parameters == null)
        {
            reason = LOC.Get("TAE_MassEditTool_Skip_No_Template");
            return false;
        }

        var data = Project.Handler.TimeActData;

        foreach (var (key, parameter) in ev.Parameters.Template)
        {
            if (parameter.ValueToAssert != null)
                continue;

            var meta = data.GetFieldMeta(eventTemplate, parameter.Name);

            if (meta != null && meta.DefaultValue.Length > 0 && TryParseValue(parameter, meta.DefaultValue, out var value))
            {
                ev.Parameters[key] = value;
            }
        }

        var working = new WorkingEvent { Start = start, End = end };

        foreach (var step in steps)
        {
            reason = ApplyParameterStep(step, ev, working);

            if (reason != null)
                return false;
        }

        foreach (var (key, _, _, current) in working.Parameters.Values)
        {
            ev.Parameters[key] = current;
        }

        return true;
    }
    #endregion

    #region Times
    /// <summary>
    /// Returns the reason the step can't be applied, or null
    /// </summary>
    private static string ApplyTimeStep(TimeActMassEditStep step, WorkingEvent working)
    {
        if (!TryParseNumber(step.Value, out var value))
            return LOC.Get("TAE_MassEditTool_Skip_Invalid_Number", step.Value);

        if (step.Frames && TimeActMassEditFields.TakesTime(step.Operator))
        {
            value *= TimeActMassEditFields.FrameDuration;
        }

        switch (step.Field)
        {
            case TimeActMassEditFields.StartTime:
                if (!TryApplyNumber(step.Operator, working.Start, value, out var start, out var startReason))
                    return startReason;

                working.Start = (float)start;
                return null;

            case TimeActMassEditFields.EndTime:
                if (!TryApplyNumber(step.Operator, working.End, value, out var end, out var endReason))
                    return endReason;

                working.End = (float)end;
                return null;

            case TimeActMassEditFields.Times:
                switch (step.Operator)
                {
                    case TimeActMassEditOperator.Shift:
                        working.Start = (float)(working.Start + value);
                        working.End = (float)(working.End + value);
                        return null;

                    case TimeActMassEditOperator.Scale:
                        working.Start = (float)(working.Start * value);
                        working.End = (float)(working.End * value);
                        return null;

                    case TimeActMassEditOperator.SetLength:
                        working.End = (float)(working.Start + value);
                        return null;
                }
                break;
        }

        return LOC.Get("TAE_MassEditTool_Skip_Invalid_Operator");
    }

    public static string FormatTime(float time)
    {
        var frames = time / TimeActMassEditFields.FrameDuration;

        return $"{time.ToString(CultureInfo.InvariantCulture)} ({Math.Round(frames, 2).ToString(CultureInfo.InvariantCulture)}f)";
    }
    #endregion

    #region Parameters
    /// <summary>
    /// Returns the reason the step can't be applied, or null
    /// </summary>
    private string ApplyParameterStep(TimeActMassEditStep step, TAE.Event readable, WorkingEvent working)
    {
        if (readable.Parameters == null)
            return LOC.Get("TAE_MassEditTool_Skip_No_Template");

        if (!working.Parameters.TryGetValue(step.Field, out var state))
        {
            var found = readable.Parameters.Template.FirstOrDefault(e => e.Value.Name == step.Field && e.Value.ValueToAssert == null);

            if (found.Value == null)
                return LOC.Get("TAE_MassEditTool_Skip_No_Field", step.Field);

            var value = readable.Parameters[found.Key];

            state = (found.Key, found.Value, value, value);
        }

        var parameter = state.Parameter;
        var current = state.Current;

        object result;

        switch (step.Operator)
        {
            case TimeActMassEditOperator.Set:
                if (!TryParseValue(parameter, step.Value, out result))
                    return LOC.Get("TAE_MassEditTool_Skip_Invalid_Value", step.Value, parameter.Type);
                break;

            case TimeActMassEditOperator.ResetToDefault:
                if (!TryGetDefault(readable, parameter, out result))
                    return LOC.Get("TAE_MassEditTool_Skip_No_Default");
                break;

            case TimeActMassEditOperator.Toggle:
                if (current is bool flag)
                {
                    result = !flag;
                }
                else if (TryToNumber(current, out var number))
                {
                    if (!TryFromNumber(number != 0 ? 0 : 1, current, out result))
                        return LOC.Get("TAE_MassEditTool_Skip_Not_Number");
                }
                else
                {
                    return LOC.Get("TAE_MassEditTool_Skip_Not_Number");
                }
                break;

            default:
                if (!TryToNumber(current, out var currentNumber))
                    return LOC.Get("TAE_MassEditTool_Skip_Not_Number");

                if (!TryParseNumber(step.Value, out var operand))
                    return LOC.Get("TAE_MassEditTool_Skip_Invalid_Number", step.Value);

                if (!TryApplyNumber(step.Operator, currentNumber, operand, out var computed, out var reason))
                    return reason;

                if (!TryFromNumber(computed, current, out result))
                    return LOC.Get("TAE_MassEditTool_Skip_Out_Of_Range", FormatNumber(computed), parameter.Type);
                break;
        }

        working.Parameters[step.Field] = (state.Key, parameter, state.Old, result);

        return null;
    }

    /// <summary>
    /// Enum entries are looked up by name or by value first, as the template's own parsing reads the value of an
    /// entry back as hex for the hex types
    /// </summary>
    public static bool TryParseValue(TAE.Template.ParameterTemplate parameter, string text, out object value)
    {
        value = null;
        text = text?.Trim() ?? "";

        if (parameter.EnumEntries != null)
        {
            if (parameter.EnumEntries.TryGetValue(text, out value))
                return true;

            foreach (var entry in parameter.EnumEntries.Values)
            {
                if (TimeActQueryMatcher.FormatValue(entry) == text)
                {
                    value = entry;
                    return true;
                }
            }
        }

        try
        {
            value = parameter.StringToValue(text);
            return value != null;
        }
        catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// The value the meta gives as usual for the field, or failing that the template's default
    /// </summary>
    private bool TryGetDefault(TAE.Event readable, TAE.Template.ParameterTemplate parameter, out object value)
    {
        value = null;

        var meta = Project.Handler.TimeActData.GetFieldMeta(readable, parameter.Name);

        if (meta != null && meta.DefaultValue.Length > 0 && TryParseValue(parameter, meta.DefaultValue, out value))
            return true;

        value = parameter.DefaultValue;

        return value != null;
    }

    public static string FormatParameter(TAE.Template.ParameterTemplate parameter, object value)
    {
        try
        {
            var text = parameter.ValueToString(value);

            // Enum entries are shown with the value they stand for
            if (parameter.EnumEntries != null && parameter.EnumEntries.ContainsKey(text))
                return $"{text} ({TimeActQueryMatcher.FormatValue(value)})";

            return text;
        }
        catch (Exception)
        {
            return TimeActQueryMatcher.FormatValue(value);
        }
    }
    #endregion

    #region Numbers
    private static bool TryParseNumber(string text, out double number)
    {
        return TimeActMassEditFilter.TryParseNumber(text, out number);
    }

    private static string FormatNumber(double number) => number.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Applies an arithmetic operator. Returns the reason it can't be applied, if it can't.
    /// </summary>
    private static bool TryApplyNumber(TimeActMassEditOperator op, double current, double operand, out double result, out string reason)
    {
        reason = null;

        switch (op)
        {
            case TimeActMassEditOperator.Set: result = operand; break;
            case TimeActMassEditOperator.Add: result = current + operand; break;
            case TimeActMassEditOperator.Subtract: result = current - operand; break;
            case TimeActMassEditOperator.MultiplyBy: result = current * operand; break;
            case TimeActMassEditOperator.AtLeast: result = Math.Max(current, operand); break;
            case TimeActMassEditOperator.AtMost: result = Math.Min(current, operand); break;

            case TimeActMassEditOperator.DivideBy:
                if (operand == 0)
                {
                    result = 0;
                    reason = LOC.Get("TAE_MassEditTool_Skip_Divide_By_Zero");
                    return false;
                }

                result = current / operand;
                break;

            default:
                result = 0;
                reason = LOC.Get("TAE_MassEditTool_Skip_Invalid_Operator");
                return false;
        }

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            reason = LOC.Get("TAE_MassEditTool_Skip_Out_Of_Range", FormatNumber(result), "number");
            return false;
        }

        return true;
    }

    private static bool TryToNumber(object value, out double number)
    {
        switch (value)
        {
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double:
                number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;

            default:
                number = 0;
                return false;
        }
    }

    /// <summary>
    /// Puts a number back into the type of the value it came from. Whole number types are rounded, and a number
    /// outside the range of the type fails rather than wrapping around.
    /// </summary>
    private static bool TryFromNumber(double number, object like, out object value)
    {
        value = null;

        double rounded = Math.Round(number, MidpointRounding.AwayFromZero);

        switch (like)
        {
            case byte: if (rounded < byte.MinValue || rounded > byte.MaxValue) return false; value = (byte)rounded; return true;
            case sbyte: if (rounded < sbyte.MinValue || rounded > sbyte.MaxValue) return false; value = (sbyte)rounded; return true;
            case short: if (rounded < short.MinValue || rounded > short.MaxValue) return false; value = (short)rounded; return true;
            case ushort: if (rounded < ushort.MinValue || rounded > ushort.MaxValue) return false; value = (ushort)rounded; return true;
            case int: if (rounded < int.MinValue || rounded > int.MaxValue) return false; value = (int)rounded; return true;
            case uint: if (rounded < uint.MinValue || rounded > uint.MaxValue) return false; value = (uint)rounded; return true;

            // The edges of the 64-bit ranges can't be told apart as doubles, so they are kept clear of
            case long: if (rounded < -9.2e18 || rounded > 9.2e18) return false; value = (long)rounded; return true;
            case ulong: if (rounded < 0 || rounded > 1.8e19) return false; value = (ulong)rounded; return true;

            case float:
                if (number < float.MinValue || number > float.MaxValue) return false;
                value = (float)number;
                return true;

            case double:
                value = number;
                return true;
        }

        return false;
    }
    #endregion
}
