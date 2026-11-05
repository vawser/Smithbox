using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// What the mass edit acts on
/// </summary>
public enum GparamMassEditTarget
{
    Values,
    Fields,
    Groups
}

/// <summary>
/// Where the mass edit looks for its targets
/// </summary>
public enum GparamMassEditScope
{
    SelectedValues,
    SelectedField,
    SelectedGroup,
    SelectedFile,
    AllFiles
}

/// <summary>
/// What the mass edit does to its matches
/// </summary>
public enum GparamMassEditAction
{
    Edit,
    Delete,
    Duplicate
}

/// <summary>
/// One condition on the value rows: the part of the row it reads, how it compares and with what
/// </summary>
public record GparamMassEditCondition(GparamQueryValueTarget Target, GparamQueryComparison Comparison,
    string Value, string Value2)
{
    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is GparamQueryComparison.Between && !string.IsNullOrWhiteSpace(Value2));
}

/// <summary>
/// The filter of a mass edit, as it was when Find was pressed. The text inputs are matched as the search matches
/// them, and every condition must match. The value rows it is limited to come from the scope; null means no limit.
/// </summary>
public class GparamMassEditFilter : IGparamQueryFilter
{
    public GparamMassEditTarget Target { get; init; }

    public string File { get; init; } = "";
    public string Group { get; init; } = "";
    public string Field { get; init; } = "";

    public IReadOnlyList<GparamMassEditCondition> Conditions { get; init; } = [];

    /// <summary>
    /// Whether typed text must equal what it is matched against, rather than be contained in it
    /// </summary>
    public bool Exact { get; init; }

    public IReadOnlySet<GPARAM.IFieldValue> LimitValues { get; init; }

    private GparamQuery textQuery;
    private GparamQuery TextQuery => textQuery ??= new(File ?? "", Group ?? "", Field ?? "", "", Exact);

    // Each condition as a search query of its own, so it is compared as the search compares
    private List<GparamQuery> conditionQueries;
    private List<GparamQuery> ConditionQueries => conditionQueries ??= Conditions
        .Where(e => e.HasValue)
        .Select(e => new GparamQuery("", "", "", e.Value?.Trim() ?? "", Exact, e.Comparison, e.Value2?.Trim() ?? "", e.Target))
        .ToList();

    // Reads the value of a row for its label, matching any value
    private static readonly GparamQuery ValueQuery = new("", "", "", "", false);

    public GparamQueryLevel Level => Target switch
    {
        GparamMassEditTarget.Groups => GparamQueryLevel.Group,
        GparamMassEditTarget.Fields => GparamQueryLevel.Field,
        _ => GparamQueryLevel.Value
    };

    public bool MatchFile(GparamQueryMatcher matcher, FileDictionaryEntry fileEntry, string alias)
        => matcher.MatchFile(TextQuery, fileEntry, alias);

    public bool MatchGroup(GparamQueryMatcher matcher, GPARAM.Param group)
        => matcher.MatchGroup(TextQuery, group);

    public bool MatchField(GparamQueryMatcher matcher, string groupKey, GPARAM.IField field)
        => matcher.MatchField(TextQuery, groupKey, field);

    public bool MatchValue(GparamQueryMatcher matcher, string groupKey, GPARAM.IField field, GPARAM.IFieldValue value,
        out string label, out string valueText)
    {
        matcher.MatchValue(ValueQuery, groupKey, field, value, out valueText);

        label = $"= {valueText}";

        if (LimitValues != null && !LimitValues.Contains(value))
            return false;

        foreach (var condition in ConditionQueries)
        {
            if (!matcher.MatchValue(condition, groupKey, field, value, out _))
                return false;
        }

        return true;
    }
}
