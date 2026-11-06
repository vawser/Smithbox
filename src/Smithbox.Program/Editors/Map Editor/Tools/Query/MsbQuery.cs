using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// How the value input is compared with a value
/// </summary>
public enum MsbQueryComparison
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
/// Which of the MSB's lists are gone through
/// </summary>
public enum MsbQueryCategory
{
    All,
    Parts,
    Regions,
    Events
}

/// <summary>
/// What a query finds: map objects, or the fields of map objects
/// </summary>
public enum MsbQueryLevel
{
    Entry,
    Field
}

/// <summary>
/// What a query asks of the maps, objects and fields it goes through. It is read off the main thread while the query
/// runs, so an implementation must not change once the query has started.
/// </summary>
public interface IMsbQueryFilter
{
    /// <summary>
    /// The level the matches are at
    /// </summary>
    MsbQueryLevel Level { get; }

    bool MatchMap(MsbQueryMatcher matcher, string mapID, string alias);

    /// <summary>
    /// Whether an object can match by what it is (its list, type and name), before its fields are read
    /// </summary>
    bool MatchEntry(MsbQueryMatcher matcher, MsbQueryEntry entry);

    /// <summary>
    /// At the entry level: whether the fields of an object that passed <see cref="MatchEntry"/> match. Gives what
    /// matched to show in the label, or null.
    /// </summary>
    bool MatchFields(MsbQueryMatcher matcher, MsbQueryEntry entry, List<MsbQueryField> fields, out string match);

    /// <summary>
    /// At the field level: whether one field of an object that passed <see cref="MatchEntry"/> is a match
    /// </summary>
    bool MatchField(MsbQueryMatcher matcher, MsbQueryEntry entry, MsbQueryField field);

    /// <summary>
    /// Whether the fields of an object are needed at the entry level, as reading them takes time
    /// </summary>
    bool NeedsFields { get; }

    /// <summary>
    /// Whether an entry-level match keeps the fields it was read with (<see cref="MsbQueryMatch.Fields"/>), so they
    /// needn't be read again
    /// </summary>
    bool KeepFields { get; }
}

/// <summary>
/// The inputs of a search as they were when it started, as the tool's inputs can be edited while it runs.
/// A field or value finds each field that matches; without one, the objects are found.
/// </summary>
public record MsbQuery(string Map, MsbQueryCategory Category, string Type, string Name, string Field, string Value,
    bool Exact, MsbQueryComparison Comparison = MsbQueryComparison.Contains, string Value2 = "",
    bool IgnoreDummies = false) : IMsbQueryFilter
{
    public bool HasMap => !string.IsNullOrWhiteSpace(Map);
    public bool HasType => !string.IsNullOrWhiteSpace(Type);
    public bool HasName => !string.IsNullOrWhiteSpace(Name);
    public bool HasField => !string.IsNullOrWhiteSpace(Field);

    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is MsbQueryComparison.Between && !string.IsNullOrWhiteSpace(Value2));

    public bool HasAny => HasMap || HasType || HasName || HasField || HasValue || Category is not MsbQueryCategory.All;

    public MsbQueryLevel Level => HasField || HasValue ? MsbQueryLevel.Field : MsbQueryLevel.Entry;

    public bool NeedsFields => false;

    public bool KeepFields => false;

    public bool MatchMap(MsbQueryMatcher matcher, string mapID, string alias)
        => MsbQueryMatcher.MatchText(Map, Exact, mapID, alias);

    public bool MatchEntry(MsbQueryMatcher matcher, MsbQueryEntry entry)
        => MsbQueryMatcher.MatchEntry(entry, Category, Type, Name, Exact, IgnoreDummies);

    public bool MatchFields(MsbQueryMatcher matcher, MsbQueryEntry entry, List<MsbQueryField> fields, out string match)
    {
        match = null;
        return true;
    }

    public bool MatchField(MsbQueryMatcher matcher, MsbQueryEntry entry, MsbQueryField field)
    {
        if (!MsbQueryMatcher.MatchText(Field, Exact, field.Path, field.DisplayName))
            return false;

        if (!HasValue)
            return true;

        return MsbQueryMatcher.Compare(Comparison, Value, Value2, Exact, field);
    }
}

/// <summary>
/// A map to query, with what is needed of the editor taken beforehand, as the query runs off the main thread.
/// Entities is null for a map that isn't loaded (or is loaded in another view), which is read for the query.
/// </summary>
public record MsbQuerySource(string MapID, string Alias, FileDictionaryEntry FileEntry, List<MsbEntity> Entities);

/// <summary>
/// A map object as it is queried: the list it is in, its position in that list, its type and name, and the object.
/// Entity is the editor's entity for a loaded map, or null for an object of an MSB read for the query.
/// </summary>
public record MsbQueryEntry(string MapID, MsbEntityType Category, int Index, string TypeName, string Name,
    string Alias, object Object, MsbEntity Entity);

/// <summary>
/// A field of a map object. Path is how it is found from the object (Unk1.CharaInitID, EntityGroupIDs[3]); Owner and
/// Property are the object that holds it and its property, with Index the position in the property's array or list,
/// or -1. The value is read when the field is listed.
/// </summary>
public record MsbQueryField(string Path, string Name, string DisplayName, Type ValueType, object Value, string ValueText,
    string EnumName, object Owner, PropertyInfo Property, int Index)
{
    /// <summary>
    /// The value as it is listed: with its enum name if it has one
    /// </summary>
    public string Shown => EnumName != null ? $"{EnumName} ({ValueText})" : ValueText;
}

/// <summary>
/// What a query found: an object, or a field of it when Field isn't null. The object is the editor's for a loaded map,
/// and a copy read for the query otherwise, so its list, position and name are kept to find it again.
/// </summary>
public record MsbQueryMatch(MsbQuerySource Source, MsbQueryEntry Entry, MsbQueryField Field, string Label)
{
    public string MapID => Source.MapID;

    /// <summary>
    /// Whether the match was found in the editor's entities rather than in a copy read for the query
    /// </summary>
    public bool FromLoaded => Entry.Entity != null;

    /// <summary>
    /// The fields of the object as they were when it was found, for a filter that keeps them
    /// (<see cref="IMsbQueryFilter.KeepFields"/>). Null otherwise.
    /// </summary>
    public List<MsbQueryField> Fields { get; init; }
}
