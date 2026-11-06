using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// What the mass edit's find asks of the map objects: the search's map, category, type and name, then any number of
/// conditions on their fields, all of which must match. Matches are the objects, with the fields they were found
/// with kept for the edit. Read off the main thread while the find runs, so it must not change once started.
/// </summary>
public class MsbMassEditFilter : IMsbQueryFilter
{
    public string Map = "";
    public MsbQueryCategory Category = MsbQueryCategory.All;
    public string Type = "";
    public string Name = "";
    public List<MsbMassEditCondition> Conditions = [];
    public bool Exact = false;
    public bool IgnoreDummies = false;

    /// <summary>
    /// For the Selected Objects scope: only these objects can match. Null for any.
    /// </summary>
    public HashSet<object> LimitObjects;

    public MsbQueryLevel Level => MsbQueryLevel.Entry;

    public bool NeedsFields => Conditions.Count > 0;

    public bool KeepFields => true;

    public bool MatchMap(MsbQueryMatcher matcher, string mapID, string alias)
        => MsbQueryMatcher.MatchText(Map, Exact, mapID, alias);

    public bool MatchEntry(MsbQueryMatcher matcher, MsbQueryEntry entry)
    {
        if (LimitObjects != null && !LimitObjects.Contains(entry.Object))
            return false;

        return MsbQueryMatcher.MatchEntry(entry, Category, Type, Name, Exact, IgnoreDummies);
    }

    /// <summary>
    /// Every condition must be met by some field at its path. The label shows the field that met each one.
    /// </summary>
    public bool MatchFields(MsbQueryMatcher matcher, MsbQueryEntry entry, List<MsbQueryField> fields, out string match)
    {
        match = null;

        var shown = new List<string>();

        foreach (var condition in Conditions)
        {
            var hasValue = !string.IsNullOrWhiteSpace(condition.Value) ||
                (condition.Comparison is MsbQueryComparison.Between && !string.IsNullOrWhiteSpace(condition.Value2));

            var found = fields.FirstOrDefault(field =>
                MsbQueryMatcher.MatchText(condition.Field, Exact, field.Path, field.DisplayName) &&
                (!hasValue || MsbQueryMatcher.Compare(condition.Comparison, condition.Value, condition.Value2, Exact, field)));

            if (found == null)
                return false;

            shown.Add(MsbQueryRunner.GetFieldLabel(found));
        }

        if (shown.Count > 0)
        {
            match = string.Join("  ", shown);
        }

        return true;
    }

    public bool MatchField(MsbQueryMatcher matcher, MsbQueryEntry entry, MsbQueryField field) => true;
}
