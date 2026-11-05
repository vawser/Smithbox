using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// How the value input is compared with a value
/// </summary>
public enum GparamQueryComparison
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
/// Which part of a value row the value input is matched against
/// </summary>
public enum GparamQueryValueTarget
{
    Value,
    ID,
    TimeOfDay
}

/// <summary>
/// The inputs of a search as they were when it started, as the tool's inputs can be edited while it runs.
/// The deepest level given decides what is found: groups, fields, or the value rows of fields.
/// </summary>
public record GparamQuery(string File, string Group, string Field, string Value, bool Exact,
    GparamQueryComparison Comparison = GparamQueryComparison.Contains, string Value2 = "",
    GparamQueryValueTarget ValueTarget = GparamQueryValueTarget.Value)
{
    public bool HasFile => !string.IsNullOrWhiteSpace(File);
    public bool HasGroup => !string.IsNullOrWhiteSpace(Group);
    public bool HasField => !string.IsNullOrWhiteSpace(Field);

    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is GparamQueryComparison.Between && !string.IsNullOrWhiteSpace(Value2));

    public bool HasAny => HasFile || HasGroup || HasField || HasValue;

    public bool MatchesGroups => HasGroup || HasField || HasValue;
    public bool MatchesFields => HasField || HasValue;
}

/// <summary>
/// A file to query, with what is needed of the editor taken beforehand, as the query runs off the main thread.
/// Gparam is null for a file that isn't open, which is read again for the query without being opened.
/// GroupKey and FieldKey narrow a local search down to the selected group or field.
/// </summary>
public record GparamQuerySource(FileDictionaryEntry FileEntry, string Alias, GPARAM Gparam,
    string GroupKey = null, string FieldKey = null);

/// <summary>
/// What a query found. The references are to the objects queried, which for a file that wasn't open are copies
/// read for the query, so the keys, IDs and positions are kept as well to find them again once the file is opened.
/// FieldIndex is -1 for a match on a group, ValueIndex -1 for a match on a field.
/// </summary>
public record GparamQueryMatch(
    FileDictionaryEntry FileEntry,
    GPARAM.Param Group, string GroupKey, int GroupIndex,
    GPARAM.IField Field, string FieldKey, int FieldIndex,
    GPARAM.IFieldValue Value, int ValueID, int ValueIndex,
    string ValueText,
    string Label);

/// <summary>
/// Matches the files, groups, fields and values of GPARAMs against a query, by their names, annotations and values.
/// Safe to use off the main thread, as it only reads. The annotation lookups are cached, as they are linear
/// searches, so a matcher is made per query.
/// </summary>
public class GparamQueryMatcher
{
    public ProjectEntry Project;

    private readonly Dictionary<string, string> GroupNames = new();
    private readonly Dictionary<(string, string), (string Name, Dictionary<string, string> EnumNames)> FieldInfo = new();
    private readonly Dictionary<string, Dictionary<string, string>> EnumNames = new();

    public GparamQueryMatcher(ProjectEntry project)
    {
        Project = project;
    }

    #region Names
    /// <summary>
    /// The annotated name of a group, or its key if it has none
    /// </summary>
    public string GetGroupName(string groupKey)
    {
        if (GroupNames.TryGetValue(groupKey, out var name))
            return name;

        try
        {
            name = GparamMetaUtils.GetGroupName(Project, groupKey);
        }
        catch
        {
            // No annotations for the language
            name = groupKey;
        }

        GroupNames[groupKey] = name;

        return name;
    }

    /// <summary>
    /// The annotated name of a field, or its key if it has none
    /// </summary>
    public string GetFieldName(string groupKey, string fieldKey) => GetFieldInfo(groupKey, fieldKey).Name;

    private (string Name, Dictionary<string, string> EnumNames) GetFieldInfo(string groupKey, string fieldKey)
    {
        if (FieldInfo.TryGetValue((groupKey, fieldKey), out var info))
            return info;

        string name = fieldKey;
        string enumKey = null;

        try
        {
            var annotation = GparamMetaUtils.GetFieldAnnotation(Project, groupKey, fieldKey);

            if (annotation != null)
            {
                name = annotation.Name ?? fieldKey;
                enumKey = annotation.Enum;
            }
        }
        catch
        {
            // No annotations for the language
        }

        info = (name, GetEnumNames(enumKey));

        FieldInfo[(groupKey, fieldKey)] = info;

        return info;
    }

    /// <summary>
    /// The names of an enum's options, by their values as text
    /// </summary>
    private Dictionary<string, string> GetEnumNames(string enumKey)
    {
        if (string.IsNullOrEmpty(enumKey))
            return null;

        if (EnumNames.TryGetValue(enumKey, out var names))
            return names;

        names = null;

        var enumEntry = Project.Handler.GparamData.Enums?.List.FirstOrDefault(e => e.Key == enumKey);

        if (enumEntry != null)
        {
            var language = CFG.Current.GparamEditor_Annotation_Language;

            names = new();

            foreach (var option in enumEntry.Options)
            {
                if (option.Key == null)
                    continue;

                var optionName = option.Names.FirstOrDefault(e => e.Language == language)?.Text;

                if (!string.IsNullOrEmpty(optionName))
                {
                    names.TryAdd(option.Key.Trim(), optionName);
                }
            }
        }

        EnumNames[enumKey] = names;

        return names;
    }
    #endregion

    #region Matching
    public bool MatchFile(GparamQuery query, FileDictionaryEntry fileEntry, string alias)
    {
        return MatchText(query, query.File, fileEntry.Filename, alias);
    }

    public bool MatchGroup(GparamQuery query, GPARAM.Param group)
    {
        return MatchText(query, query.Group, group.Key, group.Name, GetGroupName(group.Key));
    }

    public bool MatchField(GparamQuery query, string groupKey, GPARAM.IField field)
    {
        return MatchText(query, query.Field, field.Key, field.Name, GetFieldName(groupKey, field.Key));
    }

    /// <summary>
    /// Matches the value input against the part of the row the query targets.
    /// Gives the value as text, with its enum name if it has one.
    /// </summary>
    public bool MatchValue(GparamQuery query, string groupKey, GPARAM.IField field, GPARAM.IFieldValue value,
        out string shown)
    {
        var text = query.ValueTarget switch
        {
            GparamQueryValueTarget.ID => FormatValue(value.ID),
            GparamQueryValueTarget.TimeOfDay => FormatValue(value.TimeOfDay),
            _ => FormatValue(value.Value)
        };

        string enumName = null;

        if (query.ValueTarget is GparamQueryValueTarget.Value)
        {
            GetFieldInfo(groupKey, field.Key).EnumNames?.TryGetValue(text, out enumName);
        }

        shown = enumName != null ? $"{enumName} ({text})" : text;

        if (!query.HasValue)
            return true;

        return Compare(query, text, enumName);
    }

    /// <summary>
    /// Text is matched as text. Numbers are compared as numbers, so 1 finds 1.0. A vector or colour is a list of
    /// numbers: it equals an input with the same count of numbers when they all match, and otherwise matches a
    /// single number when any of its components does.
    /// </summary>
    public static bool Compare(GparamQuery query, string value, string enumName)
    {
        var input = query.Value?.Trim() ?? "";

        var components = ParseNumbers(value);
        var inputComponents = ParseNumbers(input);

        switch (query.Comparison)
        {
            case GparamQueryComparison.Contains:
                // Numbers are compared as numbers when exact
                if (query.Exact && inputComponents != null && components != null)
                    return IsEqual(components, inputComponents);

                return MatchText(query, input, value, enumName);

            case GparamQueryComparison.Equals:
                return components != null && inputComponents != null
                    ? IsEqual(components, inputComponents)
                    : IsTextEqual(input, value, enumName);

            case GparamQueryComparison.NotEquals:
                return components != null && inputComponents != null
                    ? !IsEqual(components, inputComponents)
                    : !IsTextEqual(input, value, enumName);

            case GparamQueryComparison.Less:
                return AnyComponent(components, inputComponents, (a, b) => a < b);

            case GparamQueryComparison.LessOrEqual:
                return AnyComponent(components, inputComponents, (a, b) => a <= b);

            case GparamQueryComparison.Greater:
                return AnyComponent(components, inputComponents, (a, b) => a > b);

            case GparamQueryComparison.GreaterOrEqual:
                return AnyComponent(components, inputComponents, (a, b) => a >= b);

            case GparamQueryComparison.Between:
                // Either bound can be left out, and they can be given either way round
                if (components == null)
                    return false;

                var hasLow = TryParseNumber(input, out var low);
                var hasHigh = TryParseNumber(query.Value2?.Trim(), out var high);

                if (!hasLow && !hasHigh)
                    return false;

                if (hasLow && hasHigh && low > high)
                    (low, high) = (high, low);

                return components.Any(c => (!hasLow || c >= low) && (!hasHigh || c <= high));
        }

        return false;
    }

    /// <summary>
    /// The same count of numbers must all be equal, or a single number must equal any component
    /// </summary>
    private static bool IsEqual(double[] components, double[] input)
    {
        if (input.Length == components.Length)
            return components.SequenceEqual(input);

        return input.Length == 1 && components.Contains(input[0]);
    }

    private static bool AnyComponent(double[] components, double[] input, Func<double, double, bool> compare)
    {
        if (components == null || input is not { Length: 1 })
            return false;

        return components.Any(c => compare(c, input[0]));
    }

    private static bool IsTextEqual(string input, string value, string enumName)
    {
        return string.Equals(value?.Trim(), input, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(enumName?.Trim(), input, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether any of the texts contains the input, or equals it when the query is exact
    /// </summary>
    public static bool MatchText(GparamQuery query, string input, params string[] texts)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        input = input.Trim();

        foreach (var text in texts)
        {
            if (string.IsNullOrEmpty(text))
                continue;

            if (query.Exact ? text.Trim().Equals(input, StringComparison.OrdinalIgnoreCase)
                            : text.Contains(input, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
    #endregion

    #region Values
    /// <summary>
    /// A value as text: vectors and colours as their components split by commas, colours as R, G, B, A
    /// </summary>
    public static string FormatValue(object value) => value switch
    {
        null => "",
        Vector2 v => $"{F(v.X)}, {F(v.Y)}",
        Vector3 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}",
        Vector4 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}, {F(v.W)}",
        Color c => $"{c.R}, {c.G}, {c.B}, {c.A}",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static string F(float value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The numbers of a value or input split by commas, or null if any part isn't a number
    /// </summary>
    public static double[] ParseNumbers(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var parts = text.Split(',');
        var numbers = new double[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (!TryParseNumber(parts[i], out numbers[i]))
                return null;
        }

        return numbers;
    }

    public static bool TryParseNumber(string text, out double number)
    {
        number = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        if (bool.TryParse(text, out var flag))
        {
            number = flag ? 1 : 0;
            return true;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }
    #endregion
}
