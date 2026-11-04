using SoulsFormats;
using System;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Decides which events the event graph shows, from a filter typed by the user. The filter looks inside the events:
/// <list type="bullet">
/// <item><c>SpEffectID 450</c> (or <c>SpEffectID=450</c>) matches events with a field whose name contains
/// SpEffectID, and whose value is 450.</item>
/// <item><c>450</c> matches events with any field whose value is 450.</item>
/// <item>Any other text matches events whose name contains it, such as <c>Invoke</c> or <c>[96]</c>.</item>
/// </list>
/// Numbers are compared by value, so a hex field matches its decimal value as well as the hex shown for it,
/// and enum fields match the name of their entry as well as its number.
/// </summary>
public class TimeActEventFilter
{
    // For the annotated names of the fields, which match as well as the internal ones
    private readonly ProjectEntry Project;

    private string Text = "";

    // The filter is parsed once whenever the text changes, as it is tested against every event each frame
    private string Query = "";
    private string FieldName;
    private string FieldValue;
    private string SingleValue;

    public TimeActEventFilter(ProjectEntry project)
    {
        Project = project;
    }

    public bool IsActive => Query.Length > 0;

    public string GetText() => Text;

    /// <summary>
    /// Sets the filter text. Returns true if it changed what the filter does.
    /// </summary>
    public bool SetText(string text)
    {
        text ??= "";

        if (text == Text)
            return false;

        Text = text;

        var oldQuery = Query;

        Parse(text);

        return Query != oldQuery;
    }

    private void Parse(string text)
    {
        Query = text.Trim();
        FieldName = null;
        FieldValue = null;
        SingleValue = null;

        if (Query.Length == 0)
            return;

        // Field=Value, which also allows field names with spaces in them
        var equals = Query.IndexOf('=');

        if (equals >= 0)
        {
            FieldName = Query[..equals].Trim();
            FieldValue = Query[(equals + 1)..].Trim();
            return;
        }

        var lastSpace = Query.LastIndexOfAny([' ', '\t']);

        // Field Value: the last word is the value, and the rest is the field
        if (lastSpace >= 0)
        {
            FieldName = Query[..lastSpace].Trim();
            FieldValue = Query[(lastSpace + 1)..].Trim();
            return;
        }

        SingleValue = Query;
    }

    /// <summary>
    /// Whether the event passes the filter. The name is the one the graph shows for it, with its type ID.
    /// </summary>
    public bool IsMatch(TAE.Event ev, string eventName)
    {
        if (!IsActive)
            return true;

        if (FieldName != null && FieldName.Length > 0 && FieldValue.Length > 0 && HasField(ev, FieldName, FieldValue))
            return true;

        if (SingleValue != null && HasField(ev, null, SingleValue))
            return true;

        // A lone number is a value, so it doesn't also pick up every event with that number in its name or ID.
        // Brackets find an event by its type, as in [96].
        if (SingleValue != null && IsNumber(SingleValue))
            return false;

        return eventName != null && eventName.Contains(Query, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether any field of the event has the value, among the fields whose internal or annotated name
    /// contains the one given, or among all of them if no name is given
    /// </summary>
    private bool HasField(TAE.Event ev, string fieldName, string valueText)
    {
        if (ev.Parameters == null)
            return false;

        foreach (var (key, parameter) in ev.Parameters.Template)
        {
            // Fixed values aren't parameters as such, and the properties don't show them either
            if (parameter.ValueToAssert != null)
                continue;

            if (fieldName != null && !IsFieldNameMatch(ev, parameter.Name, fieldName))
                continue;

            if (!ev.Parameters.Values.TryGetValue(key, out var value) || value == null)
                continue;

            if (IsValueMatch(parameter, value, valueText))
                return true;
        }

        return false;
    }

    private bool IsFieldNameMatch(TAE.Event ev, string parameterName, string fieldName)
    {
        if (parameterName.Contains(fieldName, StringComparison.OrdinalIgnoreCase))
            return true;

        var altName = Project?.Handler?.TimeActData?.GetFieldAnnotation(ev, parameterName)?.Name;

        return !string.IsNullOrEmpty(altName) && altName.Contains(fieldName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValueMatch(TAE.Template.ParameterTemplate parameter, object value, string valueText)
    {
        // As the properties show it: hex digits, enum names and True or False
        if (string.Equals(parameter.ValueToString(value), valueText, StringComparison.OrdinalIgnoreCase))
            return true;

        switch (value)
        {
            // Compared as floats, so that 0.1 matches the float closest to it
            case float f:
                return float.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var typedFloat)
                    && f == typedFloat;

            case double d:
                return double.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var typedDouble)
                    && d == typedDouble;

            case bool b:
                return valueText is "1" or "0" && b == (valueText == "1");

            case byte or sbyte or short or ushort or int or uint or long:
                return long.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var typedLong)
                    && Convert.ToInt64(value) == typedLong;

            case ulong u:
                return ulong.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var typedULong)
                    && u == typedULong;

            default:
                return false;
        }
    }

    private static bool IsNumber(string text)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }
}
