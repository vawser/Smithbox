using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What the mass edit acts on
/// </summary>
public enum TimeActMassEditTarget
{
    Events,
    Animations
}

/// <summary>
/// Where the mass edit looks for its targets
/// </summary>
public enum TimeActMassEditScope
{
    SelectedEvents,
    CurrentAnimation,
    CurrentTimeAct,
    CurrentSource,
    AllSources
}

/// <summary>
/// How a condition compares the value of a field with its input
/// </summary>
public enum TimeActMassEditComparison
{
    Contains,
    Equals,
    NotEquals,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,
    Between
}

/// <summary>
/// One condition on the fields of a target. A field picked from the template is matched by its internal name
/// exactly; a typed field name is matched as text, against the internal name and the annotation, so it can match
/// fields of several event types. An empty field matches any field.
/// </summary>
public record TimeActMassEditCondition(string Field, bool PickedField, TimeActMassEditComparison Comparison,
    string Value, string Value2)
{
    public bool HasField => !string.IsNullOrWhiteSpace(Field);
    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is TimeActMassEditComparison.Between && !string.IsNullOrWhiteSpace(Value2));

    /// <summary>
    /// A condition with neither a field nor a value asks nothing
    /// </summary>
    public bool IsEmpty => !HasField && !HasValue;
}

/// <summary>
/// The filter of a mass edit, as it was when Find was pressed. The animations and events it is limited to come
/// from the scope; null means no limit.
/// </summary>
public class TimeActMassEditFilter : ITimeActQueryFilter
{
    public TimeActMassEditTarget Target { get; init; }

    public string Animation { get; init; } = "";

    /// <summary>
    /// The event type picked from the template, or null to match by <see cref="EventText"/>
    /// </summary>
    public int? EventType { get; init; }
    public string EventText { get; init; } = "";

    public IReadOnlyList<TimeActMassEditCondition> Conditions { get; init; } = [];

    /// <summary>
    /// Whether typed text must equal what it is matched against, rather than be contained in it
    /// </summary>
    public bool Exact { get; init; }

    public IReadOnlySet<TAE.Animation> LimitAnimations { get; init; }
    public IReadOnlySet<TAE.Event> LimitEvents { get; init; }

    /// <summary>
    /// The template of each source, which names events that don't carry their own name. Sources can use different
    /// templates, such as the objects of Dark Souls.
    /// </summary>
    public IReadOnlyDictionary<TimeActContainerWrapper, TAE.Template.BankTemplate> Templates { get; init; }

    // The typed text is matched as the search matches it
    private TimeActQuery textQuery;
    private TimeActQuery TextQuery => textQuery ??= new(Animation ?? "", EventText ?? "", "", "", Exact);

    public bool HasEventFilter => EventType != null || !string.IsNullOrWhiteSpace(EventText);

    public bool MatchesEvents => Target is TimeActMassEditTarget.Events;

    public bool MatchAnimation(TimeActQueryMatcher matcher, TimeActContainerWrapper source, TAE.Animation animation,
        string name, out string match)
    {
        match = null;

        if (LimitAnimations != null && !LimitAnimations.Contains(animation))
            return false;

        if (!matcher.MatchAnimation(TextQuery, source, animation, name, out var animationMatch))
            return false;

        if (Target is TimeActMassEditTarget.Events)
        {
            match = animationMatch;
            return true;
        }

        // An animation can be picked by the events it has
        var template = Templates?.GetValueOrDefault(source);

        if (HasEventFilter && !animation.Events.Any(e => MatchEventType(matcher, e, null, template)))
            return false;

        if (!MatchConditions(TimeActQueryMatcher.GetAnimationProperties(animation).ToList(), out var conditionMatch))
            return false;

        match = JoinMatches(animationMatch, conditionMatch);
        return true;
    }

    /// <summary>
    /// The events are the targets, so the animation is never a match of its own
    /// </summary>
    public bool MatchAnimationProperties(TimeActQueryMatcher matcher, TAE.Animation animation, out string match)
    {
        match = null;
        return false;
    }

    /// <summary>
    /// The runner gives the name of the event, from the template of its source
    /// </summary>
    public bool MatchEventType(TimeActQueryMatcher matcher, TAE.Event ev, string eventName)
    {
        return MatchEventType(matcher, ev, eventName, null);
    }

    private bool MatchEventType(TimeActQueryMatcher matcher, TAE.Event ev, string eventName, TAE.Template.BankTemplate template)
    {
        if (LimitEvents != null && !LimitEvents.Contains(ev))
            return false;

        if (EventType != null)
            return ev.Type == EventType.Value;

        if (string.IsNullOrWhiteSpace(EventText))
            return true;

        // The name is only worked out when it is needed, as an animation is checked against all of its events
        eventName ??= TimeActQueryMatcher.GetEventName(ev, template);

        return matcher.MatchEvent(TextQuery, ev, eventName);
    }

    public bool MatchEvent(TimeActQueryMatcher matcher, TAE.Event original, TAE.Event readable, out string match)
    {
        return MatchConditions(matcher.GetEventFields(readable).ToList(), out match);
    }

    #region Conditions
    /// <summary>
    /// Every condition must be met by at least one of the fields. Gives the fields that met them.
    /// </summary>
    private bool MatchConditions(List<TimeActQueryField> fields, out string match)
    {
        match = null;

        List<string> matched = null;

        foreach (var condition in Conditions)
        {
            if (condition.IsEmpty)
                continue;

            var found = false;

            foreach (var field in fields)
            {
                if (!MatchFieldName(condition, field.Name))
                    continue;

                if (!Compare(condition, field.Value, field.EnumName))
                    continue;

                var shown = field.EnumName != null ? $"{field.EnumName} ({field.Value})" : field.Value;

                matched ??= new();
                matched.Add($"{field.DisplayName} = {shown}");

                found = true;
                break;
            }

            if (!found)
                return false;
        }

        if (matched != null)
        {
            match = string.Join(", ", matched);
        }

        return true;
    }

    /// <summary>
    /// The name of a field holds the internal name and the annotation, split by a line break
    /// </summary>
    private bool MatchFieldName(TimeActMassEditCondition condition, string name)
    {
        if (!condition.HasField)
            return true;

        var names = name.Split('\n');

        if (condition.PickedField)
            return names[0].Equals(condition.Field, StringComparison.OrdinalIgnoreCase);

        foreach (var part in names)
        {
            if (Exact ? part.Trim().Equals(condition.Field.Trim(), StringComparison.OrdinalIgnoreCase)
                      : part.Contains(condition.Field.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Numbers are compared as numbers, so that 1 doesn't match 10 and 1.0 matches 1. The other comparisons
    /// fall back on the text of the value, or the name of its enum entry.
    /// </summary>
    public static bool Compare(TimeActMassEditCondition condition, string value, string enumName)
    {
        if (!condition.HasValue)
            return true;

        var input = condition.Value.Trim();

        var hasNumbers = TryParseNumber(input, out var inputNumber) & TryParseNumber(value, out var number);

        switch (condition.Comparison)
        {
            case TimeActMassEditComparison.Contains:
                return Contains(value, input) || Contains(enumName, input);

            case TimeActMassEditComparison.Equals:
                return IsEqual(hasNumbers, number, inputNumber, value, enumName, input);

            case TimeActMassEditComparison.NotEquals:
                return !IsEqual(hasNumbers, number, inputNumber, value, enumName, input);

            case TimeActMassEditComparison.Less:
                return hasNumbers && number < inputNumber;

            case TimeActMassEditComparison.LessOrEqual:
                return hasNumbers && number <= inputNumber;

            case TimeActMassEditComparison.Greater:
                return hasNumbers && number > inputNumber;

            case TimeActMassEditComparison.GreaterOrEqual:
                return hasNumbers && number >= inputNumber;

            case TimeActMassEditComparison.Between:
                // Either bound can be left out, and they can be given either way round
                if (!TryParseNumber(value, out number))
                    return false;

                var hasLow = TryParseNumber(input, out var low);
                var hasHigh = TryParseNumber(condition.Value2?.Trim(), out var high);

                if (!hasLow && !hasHigh)
                    return false;

                if (hasLow && hasHigh && low > high)
                    (low, high) = (high, low);

                return (!hasLow || number >= low) && (!hasHigh || number <= high);
        }

        return false;
    }

    private static bool IsEqual(bool hasNumbers, double number, double inputNumber, string value, string enumName, string input)
    {
        if (hasNumbers)
            return number == inputNumber;

        return string.Equals(value?.Trim(), input, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(enumName?.Trim(), input, StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contains(string text, string input)
    {
        return !string.IsNullOrEmpty(text) && text.Contains(input, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryParseNumber(string text, out double number)
    {
        number = 0;

        return !string.IsNullOrWhiteSpace(text) &&
               double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }

    private static string JoinMatches(string first, string second)
    {
        if (first == null)
            return second;

        if (second == null)
            return first;

        return $"{first}, {second}";
    }
    #endregion
}
