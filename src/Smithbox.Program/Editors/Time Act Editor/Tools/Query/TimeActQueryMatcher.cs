using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Matches animations and events against a query: by their names, aliases, properties, fields and values.
/// Shared by the search and mass edit tools, so both find the same things. Safe to use off the main thread,
/// as it only reads.
/// </summary>
public class TimeActQueryMatcher
{
    public ProjectEntry Project;

    public TimeActQueryMatcher(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Matches the name and alias of an animation, or failing those the values of its properties.
    /// Gives the property that matched, or null if it was the name.
    /// </summary>
    public bool MatchAnimation(TimeActQuery query, TimeActContainerWrapper source, TAE.Animation animation, string name, out string match)
    {
        match = null;

        if (!query.HasAnimation)
            return true;

        var alias = AliasHelper.GetAnimationAlias(Project, source.Name, name);

        if (MatchText(query, query.Animation, name, alias))
            return true;

        foreach (var field in GetAnimationProperties(animation))
        {
            if (MatchText(query, query.Animation, field.Value))
            {
                match = $"{field.DisplayName}: {field.Value}";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Matches the event input against the name and type of an event
    /// </summary>
    public bool MatchEvent(TimeActQuery query, TAE.Event ev, string eventName)
    {
        return MatchText(query, query.Event, eventName, $"{ev.Type}");
    }

    public static IEnumerable<TimeActQueryField> GetAnimationProperties(TAE.Animation animation)
    {
        yield return new("ID", LOC.Get("TAE_PropertiesWindow_Animation_ID"), $"{animation.ID}", null);

        if (!string.IsNullOrEmpty(animation.AnimFileName))
            yield return new("AnimFileName", LOC.Get("TAE_PropertiesWindow_Animation_File_Name"), animation.AnimFileName, null);

        switch (animation.MiniHeader)
        {
            case TAE.Animation.AnimMiniHeader.Standard standard:
                if (standard.ImportsHKX)
                {
                    yield return new("ImportHKXSourceAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_HKX_Source"),
                        $"{standard.ImportHKXSourceAnimID}", null);
                }
                break;

            case TAE.Animation.AnimMiniHeader.ImportOtherAnim import:
                yield return new("ImportFromAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_From"),
                    $"{import.ImportFromAnimID}", null);
                break;
        }
    }

    /// <summary>
    /// The times and parameters of an event. The event needs its parameters filled in to list them,
    /// see <see cref="TimeActEntry.GetReadableEvent"/>.
    /// </summary>
    public IEnumerable<TimeActQueryField> GetEventFields(TAE.Event ev)
    {
        yield return new("StartTime", LOC.Get("TAE_PropertiesWindow_Start_Time"), FormatValue(ev.StartTime), null);
        yield return new("EndTime", LOC.Get("TAE_PropertiesWindow_End_Time"), FormatValue(ev.EndTime), null);

        if (ev.Parameters == null)
            yield break;

        var data = Project.Handler.TimeActData;

        foreach (var (key, parameter) in ev.Parameters.Template)
        {
            // Fixed values aren't parameters as such, so they aren't shown in the properties either
            if (parameter.ValueToAssert != null)
                continue;

            var value = ev.Parameters[key];
            var annotation = data.GetFieldAnnotation(ev, parameter.Name)?.Name;
            var displayName = data.GetFieldDisplayName(ev, parameter.Name);

            string enumName = null;

            if (parameter.EnumEntries != null)
            {
                enumName = parameter.EnumEntries.FirstOrDefault(e => Equals(e.Value, value)).Key;
            }

            // The annotation is matched as well as the internal name, whichever is on show
            var name = string.IsNullOrEmpty(annotation) || annotation == parameter.Name
                ? parameter.Name
                : $"{parameter.Name}\n{annotation}";

            yield return new(name, displayName, FormatValue(value), enumName);
        }
    }

    /// <summary>
    /// Matches the field and value inputs against a set of fields. A field and a value must match on the same one.
    /// Gives the field that matched.
    /// </summary>
    public static bool MatchFields(TimeActQuery query, IEnumerable<TimeActQueryField> fields, out string match)
    {
        match = null;

        if (!query.HasField && !query.HasValue)
            return true;

        foreach (var field in fields)
        {
            if (query.HasField && !MatchFieldName(query, field.Name))
                continue;

            if (query.HasValue && !MatchValue(query, field.Value, field.EnumName))
                continue;

            var shown = field.EnumName != null ? $"{field.EnumName} ({field.Value})" : field.Value;

            match = $"{field.DisplayName} = {shown}";
            return true;
        }

        return false;
    }

    /// <summary>
    /// The internal name and the annotation of a field are held together, split by a line break
    /// </summary>
    public static bool MatchFieldName(TimeActQuery query, string name)
    {
        return MatchText(query, query.Field, name.Split('\n'));
    }

    public static bool MatchValue(TimeActQuery query, string value, string enumName)
    {
        // Numbers are compared as numbers when exact, so 1 finds 1.0
        if (query.Exact &&
            double.TryParse(query.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var inputNumber) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var valueNumber))
        {
            return inputNumber == valueNumber;
        }

        return MatchText(query, query.Value, value, enumName);
    }

    /// <summary>
    /// Whether any of the texts contains the input, or equals it when the query is exact
    /// </summary>
    public static bool MatchText(TimeActQuery query, string input, params string[] texts)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

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

    public static string FormatValue(object value) => value switch
    {
        null => "",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    /// <summary>
    /// The type of an event and its name, from the event itself or else from the template
    /// </summary>
    public static string GetEventName(TAE.Event ev, TAE.Template.BankTemplate template)
    {
        var name = ev.TypeName;

        if (name == null && template != null && template.TryGetValue(ev.Type, out var eventTemplate))
        {
            name = eventTemplate.Name;
        }

        return name != null ? $"[{ev.Type}] {name}" : $"[{ev.Type}]";
    }
}
