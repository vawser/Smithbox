using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What a query asks of the animations and events it goes through. It is read off the main thread while the
/// query runs, so an implementation must not change once the query has started.
/// </summary>
public interface ITimeActQueryFilter
{
    /// <summary>
    /// Whether events are matched. If not, each matching animation is a match of its own.
    /// </summary>
    bool MatchesEvents { get; }

    /// <summary>
    /// Whether an animation can match. When events are matched, this only lets its events be checked.
    /// Gives what matched to show in the label, or null if there is nothing to add to the name.
    /// </summary>
    bool MatchAnimation(TimeActQueryMatcher matcher, TimeActContainerWrapper source, TAE.Animation animation,
        string name, out string match);

    /// <summary>
    /// When events are matched, whether the properties of the animation are a match of their own as well
    /// </summary>
    bool MatchAnimationProperties(TimeActQueryMatcher matcher, TAE.Animation animation, out string match);

    /// <summary>
    /// Whether an event can match by what it is, before its fields are read. Reading the fields of an event
    /// without a template applied copies it, so this is checked first.
    /// </summary>
    bool MatchEventType(TimeActQueryMatcher matcher, TAE.Event ev, string eventName);

    /// <summary>
    /// Whether an event that passed <see cref="MatchEventType"/> matches. The readable event has its parameters
    /// filled in where the template fits it, which may be a copy of the original.
    /// </summary>
    bool MatchEvent(TimeActQueryMatcher matcher, TAE.Event original, TAE.Event readable, out string match);
}

/// <summary>
/// The inputs of a search as they were when it started, as the tool's inputs can be edited while it runs
/// </summary>
public record TimeActQuery(string Animation, string Event, string Field, string Value, bool Exact) : ITimeActQueryFilter
{
    public bool HasAnimation => !string.IsNullOrWhiteSpace(Animation);
    public bool HasEvent => !string.IsNullOrWhiteSpace(Event);
    public bool HasField => !string.IsNullOrWhiteSpace(Field);
    public bool HasValue => !string.IsNullOrWhiteSpace(Value);

    public bool HasAny => HasAnimation || HasEvent || HasField || HasValue;

    /// <summary>
    /// Only the animations are matched when nothing is asked of the events
    /// </summary>
    public bool MatchesEvents => HasEvent || HasField || HasValue;

    public bool MatchAnimation(TimeActQueryMatcher matcher, TimeActContainerWrapper source, TAE.Animation animation,
        string name, out string match)
    {
        return matcher.MatchAnimation(this, source, animation, name, out match);
    }

    /// <summary>
    /// A field or value on its own can be found in the properties of the animation, as well as in its events
    /// </summary>
    public bool MatchAnimationProperties(TimeActQueryMatcher matcher, TAE.Animation animation, out string match)
    {
        match = null;

        if (HasEvent)
            return false;

        return TimeActQueryMatcher.MatchFields(this, TimeActQueryMatcher.GetAnimationProperties(animation), out match);
    }

    public bool MatchEventType(TimeActQueryMatcher matcher, TAE.Event ev, string eventName)
    {
        return matcher.MatchEvent(this, ev, eventName);
    }

    public bool MatchEvent(TimeActQueryMatcher matcher, TAE.Event original, TAE.Event readable, out string match)
    {
        return TimeActQueryMatcher.MatchFields(this, matcher.GetEventFields(readable), out match);
    }
}

/// <summary>
/// A source to query, with what is needed of the editor taken beforehand, as the query runs off the main thread.
/// Entries is null for a source that isn't open, which is read again for the query without being opened.
/// </summary>
public record TimeActQuerySource(TimeActContainerWrapper Source, ModelListType ListType,
    List<TimeActEntry> Entries, TAE.Template.BankTemplate Template);

/// <summary>
/// What a query found. The references are to the objects queried, which for a source that wasn't open are copies
/// read for the query, so the IDs and positions are kept as well to find them again once the source is opened.
/// EventIndex is -1 for a match on the animation itself.
/// </summary>
public record TimeActQueryMatch(
    TimeActContainerWrapper Source, ModelListType ListType,
    TimeActEntry Entry, string EntryName, int BindIndex,
    TAE.Animation Animation, long AnimationID,
    TAE.Event Event, int EventIndex, int EventType,
    string Label);

/// <summary>
/// A field of an animation or event, as it is matched: its name (the internal name and the annotation, split by a
/// line break), the name on show, its value as text, and the name of that value if the field is an enum
/// </summary>
public readonly record struct TimeActQueryField(string Name, string DisplayName, string Value, string EnumName);
