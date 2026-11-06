using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lists the objects of an MSB and their fields, and matches them against a query by their names, types, fields
/// and values. Safe to use off the main thread, as it only reads; the lookups it repeats are cached, so a matcher is
/// made per query.
/// </summary>
public class MsbQueryMatcher
{
    public ProjectEntry Project;

    // Fields are walked no deeper than this, in case a class refers back to one holding it
    private const int MaxDepth = 8;

    private readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();
    private readonly ConcurrentDictionary<(Type, string), MapEntityPropertyFieldMeta> FieldMetas = new();
    private readonly ConcurrentDictionary<string, string> ModelAliases = new();

    public MsbQueryMatcher(ProjectEntry project)
    {
        Project = project;
    }

    #region Entries
    /// <summary>
    /// The parts, regions and events of an MSB read for the query
    /// </summary>
    public List<MsbQueryEntry> GetEntries(string mapID, IMsb msb)
    {
        var entries = new List<MsbQueryEntry>();

        AddEntries(entries, mapID, MsbEntityType.Part, msb.Parts.GetEntries());
        AddEntries(entries, mapID, MsbEntityType.Region, msb.Regions.GetEntries());
        AddEntries(entries, mapID, MsbEntityType.Event, msb.Events.GetEntries());

        return entries;
    }

    private void AddEntries<T>(List<MsbQueryEntry> entries, string mapID, MsbEntityType category, IReadOnlyList<T> list)
        where T : IMsbEntry
    {
        for (int i = 0; i < list.Count; i++)
        {
            var obj = list[i];

            entries.Add(new MsbQueryEntry(mapID, category, i, obj.GetType().Name, obj.Name ?? "", GetAlias(obj), obj, null));
        }
    }

    /// <summary>
    /// The parts, regions and events of a loaded map, from the entities taken on the main thread
    /// </summary>
    public List<MsbQueryEntry> GetEntries(string mapID, List<MsbEntity> entities)
    {
        var entries = new List<MsbQueryEntry>();
        var counts = new Dictionary<MsbEntityType, int>();

        foreach (var entity in entities)
        {
            var obj = entity.WrappedObject;

            if (obj is not IMsbEntry msbEntry)
                continue;

            counts.TryGetValue(entity.Type, out var index);
            counts[entity.Type] = index + 1;

            entries.Add(new MsbQueryEntry(mapID, entity.Type, index, obj.GetType().Name, msbEntry.Name ?? "",
                GetAlias(obj), obj, entity));
        }

        return entries;
    }

    /// <summary>
    /// The alias of a part's model: the character, asset or map piece it places. Empty for other objects.
    /// </summary>
    public string GetAlias(object obj)
    {
        if (obj is not IMsbPart part || string.IsNullOrEmpty(part.ModelName))
            return "";

        var model = part.ModelName.ToLowerInvariant();

        return ModelAliases.GetOrAdd(model, name =>
        {
            try
            {
                var alias = AliasHelper.GetCharacterAlias(Project, name);

                if (string.IsNullOrEmpty(alias))
                    alias = AliasHelper.GetAssetAlias(Project, name);

                if (string.IsNullOrEmpty(alias))
                    alias = AliasHelper.GetMapPieceAlias(Project, name);

                return alias ?? "";
            }
            catch
            {
                return "";
            }
        });
    }

    public static string GetCategoryName(MsbEntityType category) => category switch
    {
        MsbEntityType.Part => LOC.Get("MAP_SearchTool_Category_Part"),
        MsbEntityType.Region => LOC.Get("MAP_SearchTool_Category_Region"),
        MsbEntityType.Event => LOC.Get("MAP_SearchTool_Category_Event"),
        _ => $"{category}"
    };

    /// <summary>
    /// Whether an object is in the category, of the type and named as asked
    /// </summary>
    public static bool MatchEntry(MsbQueryEntry entry, MsbQueryCategory category, string type, string name, bool exact,
        bool ignoreDummies)
    {
        var inCategory = category switch
        {
            MsbQueryCategory.Parts => entry.Category is MsbEntityType.Part,
            MsbQueryCategory.Regions => entry.Category is MsbEntityType.Region,
            MsbQueryCategory.Events => entry.Category is MsbEntityType.Event,
            _ => true
        };

        if (!inCategory)
            return false;

        if (ignoreDummies && entry.Category is MsbEntityType.Part && entry.TypeName.StartsWith("Dummy", StringComparison.Ordinal))
            return false;

        if (!MatchText(type, exact, entry.TypeName))
            return false;

        return MatchText(name, exact, entry.Name, entry.Alias);
    }
    #endregion

    #region Fields
    /// <summary>
    /// Every field of an object, as the Properties window lists them: the values of nested classes, arrays and lists
    /// of values, and arrays and lists of classes, each with its path from the object
    /// </summary>
    public List<MsbQueryField> GetFields(object obj)
    {
        var fields = new List<MsbQueryField>();

        if (obj != null)
        {
            AddFields(fields, obj, "", 0, obj is IMsbEntry);
        }

        return fields;
    }

    /// <summary>
    /// The field at a path of an object, or null if it has none
    /// </summary>
    public MsbQueryField GetField(object obj, string path)
    {
        return GetFields(obj).FirstOrDefault(e => e.Path == path);
    }

    private void AddFields(List<MsbQueryField> fields, object obj, string prefix, int depth, bool isEntry)
    {
        if (depth > MaxDepth)
            return;

        var type = obj.GetType();

        foreach (var prop in GetProperties(type))
        {
            // The name is matched by the Name input, and is kept unique by the editor
            if (isEntry && prop.Name == "Name")
                continue;

            var propType = prop.PropertyType;

            if (!prop.CanWrite && !propType.IsArray)
                continue;

            var meta = GetFieldMeta(type, prop.Name);

            // Index properties are written by the MSB itself, and are hidden by the Properties window
            if (meta is { IndexProperty: true })
                continue;

            object value;

            try
            {
                value = prop.GetValue(obj);
            }
            catch
            {
                continue;
            }

            var altName = meta is { IsEmpty: false } && !string.IsNullOrEmpty(meta.AltName) ? meta.AltName : null;
            var path = $"{prefix}{prop.Name}";

            if (propType.IsArray)
            {
                if (value is not Array array)
                    continue;

                var elementType = propType.GetElementType();

                for (int i = 0; i < array.Length; i++)
                {
                    var element = array.GetValue(i);

                    if (IsNested(elementType))
                    {
                        if (element != null)
                            AddFields(fields, element, $"{path}[{i}].", depth + 1, false);
                    }
                    else
                    {
                        fields.Add(CreateField($"{path}[{i}]", altName != null ? $"{altName}[{i}]" : null, elementType,
                            element, obj, prop, i));
                    }
                }
            }
            else if (GetListElementType(propType) is Type listType)
            {
                if (value is not IList list)
                    continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var element = list[i];

                    if (IsNested(listType))
                    {
                        if (element != null)
                            AddFields(fields, element, $"{path}[{i}].", depth + 1, false);
                    }
                    else
                    {
                        fields.Add(CreateField($"{path}[{i}]", altName != null ? $"{altName}[{i}]" : null, listType,
                            element, obj, prop, i));
                    }
                }
            }
            else if (IsNested(propType))
            {
                if (value != null)
                    AddFields(fields, value, $"{path}.", depth + 1, false);
            }
            else
            {
                fields.Add(CreateField(path, altName, propType, value, obj, prop, -1));
            }
        }
    }

    private MsbQueryField CreateField(string path, string altName, Type valueType, object value, object owner,
        PropertyInfo prop, int index)
    {
        var name = path.Contains('.') ? path[(path.LastIndexOf('.') + 1)..] : path;

        string enumName = null;
        var text = FormatValue(value);

        if (value is Enum enumValue)
        {
            enumName = enumValue.ToString();
            text = FormatValue(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), CultureInfo.InvariantCulture));
        }

        return new MsbQueryField(path, name, altName ?? name, valueType, value, text, enumName, owner, prop, index);
    }

    /// <summary>
    /// Whether a type is a class whose own properties are listed, rather than a value
    /// </summary>
    private static bool IsNested(Type type)
    {
        return type.IsClass && type != typeof(string) && !type.IsArray;
    }

    public static Type GetListElementType(Type type)
    {
        for (Type t = type; t != null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                return t.GetGenericArguments()[0];
        }

        return null;
    }

    private PropertyInfo[] GetProperties(Type type)
    {
        return Properties.GetOrAdd(type, t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.MetadataToken)
            .ToArray());
    }

    private MapEntityPropertyFieldMeta GetFieldMeta(Type type, string name)
    {
        return FieldMetas.GetOrAdd((type, name), key =>
        {
            try
            {
                return Project.Handler.MapData.Meta?.GetFieldMeta(key.Item2, key.Item1);
            }
            catch
            {
                return null;
            }
        });
    }
    #endregion

    #region Matching
    /// <summary>
    /// Whether any of the texts matches the input, with the filter syntax of the selection windows ('|' for or,
    /// '+' for and). An empty input matches anything.
    /// </summary>
    public static bool MatchText(string input, bool exact, string text, string alias = "")
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        return EditorFilters.IsMatch(input.Trim(), text ?? "", exact, alias ?? "");
    }

    /// <summary>
    /// Text is matched as text. Numbers are compared as numbers, so 1 finds 1.0. A vector is a list of numbers: it
    /// equals an input with the same count of numbers when they all match, and otherwise matches a single number
    /// when any of its components does.
    /// </summary>
    public static bool Compare(MsbQueryComparison comparison, string value, string value2, bool exact, MsbQueryField field)
    {
        var input = value?.Trim() ?? "";
        var text = field.ValueText;
        var enumName = field.EnumName;

        var components = ParseNumbers(text);
        var inputComponents = ParseNumbers(input);

        switch (comparison)
        {
            case MsbQueryComparison.Contains:
                // Numbers are compared as numbers when exact
                if (exact && inputComponents != null && components != null)
                    return IsEqual(components, inputComponents);

                return MatchText(input, exact, text, enumName);

            case MsbQueryComparison.Equals:
                return components != null && inputComponents != null
                    ? IsEqual(components, inputComponents)
                    : IsTextEqual(input, text, enumName);

            case MsbQueryComparison.NotEquals:
                return components != null && inputComponents != null
                    ? !IsEqual(components, inputComponents)
                    : !IsTextEqual(input, text, enumName);

            case MsbQueryComparison.Less:
                return AnyComponent(components, inputComponents, (a, b) => a < b);

            case MsbQueryComparison.LessOrEqual:
                return AnyComponent(components, inputComponents, (a, b) => a <= b);

            case MsbQueryComparison.Greater:
                return AnyComponent(components, inputComponents, (a, b) => a > b);

            case MsbQueryComparison.GreaterOrEqual:
                return AnyComponent(components, inputComponents, (a, b) => a >= b);

            case MsbQueryComparison.Between:
                // Either bound can be left out, and they can be given either way round
                if (components == null)
                    return false;

                var hasLow = TryParseNumber(input, out var low);
                var hasHigh = TryParseNumber(value2?.Trim(), out var high);

                if (!hasLow && !hasHigh)
                    return false;

                if (hasLow && hasHigh && low > high)
                    (low, high) = (high, low);

                return components.Any(c => (!hasLow || c >= low) && (!hasHigh || c <= high));
        }

        return false;
    }

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
    #endregion

    #region Values
    /// <summary>
    /// A value as text: vectors and colours as their components split by commas, numbers in the invariant culture
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
