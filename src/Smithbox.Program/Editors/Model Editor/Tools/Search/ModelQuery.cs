using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// How a value input is compared with the value of a field. Contains matches text; the others compare numbers
/// as numbers, as the Time Act Editor's search does.
/// </summary>
public enum ModelQueryComparison
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
/// The kinds of FLVER object a search looks through. Each is found again by its position in the matching list of
/// the loaded model (Header is the model itself, shown by the model root in Contents).
/// </summary>
public enum ModelQueryObjectKind
{
    Header,
    Dummy,
    Material,
    Node,
    Mesh
}

/// <summary>
/// What a query asks of the models and objects it goes through: the search's inputs, or the mass edit's filter.
/// It is read off the main thread while the query runs, so it must not change once the query has started.
/// </summary>
public interface IModelQueryFilter
{
    /// <summary>
    /// Whether objects are matched. If not, each matching model is a match of its own.
    /// </summary>
    bool MatchesObjects { get; }

    /// <summary>
    /// Whether a model can match, by its name (known before it is read)
    /// </summary>
    bool MatchModel(string modelName, ModelQuerySource source);

    /// <summary>
    /// Whether an object can match by what it is, before its fields are read
    /// </summary>
    bool MatchObject(ModelQueryObjectKind kind, object obj, string name);

    /// <summary>
    /// Whether the fields of an object match. Gives what matched to show in the label, or null.
    /// </summary>
    bool MatchFields(object obj, out string match);
}

/// <summary>
/// The inputs of a search as they were when it started, as the tool's inputs can be edited while it runs
/// </summary>
public record ModelQuery(string Model, string Object, string Field, string Value, bool Exact,
    ModelQueryComparison Comparison = ModelQueryComparison.Contains, string Value2 = "") : IModelQueryFilter
{
    bool IModelQueryFilter.MatchModel(string modelName, ModelQuerySource source) =>
        ModelQueryMatcher.MatchModel(this, modelName, source);

    bool IModelQueryFilter.MatchObject(ModelQueryObjectKind kind, object obj, string name) =>
        ModelQueryMatcher.MatchObject(this, kind, name);

    bool IModelQueryFilter.MatchFields(object obj, out string match) =>
        ModelQueryMatcher.MatchFields(this, obj, out match);

    public bool HasModel => !string.IsNullOrWhiteSpace(Model);
    public bool HasObject => !string.IsNullOrWhiteSpace(Object);
    public bool HasField => !string.IsNullOrWhiteSpace(Field);

    /// <summary>
    /// A range can be given by its upper end alone
    /// </summary>
    public bool HasValue => !string.IsNullOrWhiteSpace(Value) ||
        (Comparison is ModelQueryComparison.Between && !string.IsNullOrWhiteSpace(Value2));

    public bool HasAny => HasModel || HasObject || HasField || HasValue;

    /// <summary>
    /// Only the models are matched when nothing is asked of their objects
    /// </summary>
    public bool MatchesObjects => HasObject || HasField || HasValue;
}

/// <summary>
/// A model to search, with what is needed of the editor taken beforehand, as the search runs off the main thread.
/// Flver is the model as it is in the editor when it has edits in memory; null to read the stored file.
/// </summary>
public record ModelQueryModel(string Name, FLVER2 Flver);

/// <summary>
/// A container to search. Models lists the models to search by name: those with edits in memory carry them, and
/// the rest are read from the stored file. A null Models reads every model of the container (a global search).
/// </summary>
public record ModelQuerySource(ModelContainerWrapper Container, ModelListType ListType, string Alias,
    List<ModelQueryModel> Models, bool ReadOthers);

/// <summary>
/// What a search found. Object is the FLVER object searched, which for a model read from its file is a copy, so
/// the kind and position are kept to find it again once the model is loaded. Index is -1 for the model itself.
/// </summary>
public record ModelQueryMatch(
    ModelContainerWrapper Container, ModelListType ListType, string ModelName,
    ModelQueryObjectKind Kind, int Index, object Object,
    string Label);

/// <summary>
/// A field of a FLVER object, as it is matched: its path (Textures[0].Path), its value as text, and the name of that
/// value if the field is an enum
/// </summary>
public readonly record struct ModelQueryField(string Name, string Value, string EnumName);

/// <summary>
/// Matches models and their objects against a query: by their names, aliases, fields and values. Safe to use off
/// the main thread, as it only reads.
/// </summary>
public static class ModelQueryMatcher
{
    // Lists that are the geometry rather than settings: large, and not worth reading as text
    private static readonly HashSet<string> SkippedMembers = new(StringComparer.Ordinal)
    {
        "Vertices", "FaceSets", "VertexBuffers"
    };

    // A list of numbers longer than this is geometry too
    private const int MaxListValues = 512;

    // Lists of objects are read into, as a material's textures are, up to this many
    private const int MaxListObjects = 64;

    private const int MaxDepth = 2;

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    #region Names
    public static string GetKindName(ModelQueryObjectKind kind) => kind switch
    {
        ModelQueryObjectKind.Header => LOC.Get("MODEL_SearchTool_Kind_Header"),
        ModelQueryObjectKind.Dummy => LOC.Get("MODEL_SearchTool_Kind_Dummy"),
        ModelQueryObjectKind.Material => LOC.Get("MODEL_SearchTool_Kind_Material"),
        ModelQueryObjectKind.Node => LOC.Get("MODEL_SearchTool_Kind_Node"),
        ModelQueryObjectKind.Mesh => LOC.Get("MODEL_SearchTool_Kind_Mesh"),
        _ => $"{kind}"
    };

    /// <summary>
    /// The name an object is listed under: bones and materials by name, dummies by reference ID, meshes by position
    /// </summary>
    public static string GetObjectName(ModelQueryObjectKind kind, object obj, int index) => obj switch
    {
        FLVER.Node node => string.IsNullOrEmpty(node.Name) ? $"#{index}" : node.Name,
        FLVER2.Material material => string.IsNullOrEmpty(material.Name) ? $"#{index}" : material.Name,
        FLVER.Dummy dummy => $"{dummy.ReferenceID}",
        FLVER2.Mesh => $"{index}",
        _ => ""
    };

    /// <summary>
    /// The objects of a model, as the search goes through them
    /// </summary>
    public static IEnumerable<(ModelQueryObjectKind Kind, int Index, object Object)> GetObjects(FLVER2 flver)
    {
        if (flver.Header != null)
            yield return (ModelQueryObjectKind.Header, -1, flver.Header);

        for (int i = 0; i < flver.Dummies.Count; i++)
            yield return (ModelQueryObjectKind.Dummy, i, flver.Dummies[i]);

        for (int i = 0; i < flver.Materials.Count; i++)
            yield return (ModelQueryObjectKind.Material, i, flver.Materials[i]);

        for (int i = 0; i < flver.Nodes.Count; i++)
            yield return (ModelQueryObjectKind.Node, i, flver.Nodes[i]);

        for (int i = 0; i < flver.Meshes.Count; i++)
            yield return (ModelQueryObjectKind.Mesh, i, flver.Meshes[i]);
    }
    #endregion

    #region Matching
    /// <summary>
    /// Matches the model input against the model name, its container name and the container's alias
    /// </summary>
    public static bool MatchModel(ModelQuery query, string modelName, ModelQuerySource source)
    {
        return MatchText(query, query.Model, modelName, source.Container.Name, source.Alias);
    }

    /// <summary>
    /// Matches the object input against the kind of an object (as named in Contents, or its localised name), and its
    /// name, so "Dummy", "Material" or "c0000_body" all work
    /// </summary>
    public static bool MatchObject(ModelQuery query, ModelQueryObjectKind kind, string name)
    {
        // A bone is a node, by the name most will look for
        var alias = kind is ModelQueryObjectKind.Node ? "Bone" : null;

        return MatchText(query, query.Object, $"{kind}", GetKindName(kind), alias, name);
    }

    /// <summary>
    /// Matches the field and value inputs against the fields of an object. A field and a value must match on the same
    /// one. Gives the field that matched.
    /// </summary>
    public static bool MatchFields(ModelQuery query, object obj, out string match)
    {
        match = null;

        if (!query.HasField && !query.HasValue)
            return true;

        foreach (var field in GetFields(obj))
        {
            if (query.HasField && !MatchText(query, query.Field, field.Name, GetLeafName(field.Name)))
                continue;

            if (query.HasValue && !MatchValue(query, field.Value, field.EnumName))
                continue;

            var shown = field.EnumName != null ? $"{field.EnumName} ({field.Value})" : field.Value;

            match = $"{field.Name} = {shown}";
            return true;
        }

        return false;
    }

    /// <summary>
    /// The last part of a field path, so Path finds Textures[0].Path when exact
    /// </summary>
    private static string GetLeafName(string name)
    {
        var dot = name.LastIndexOf('.');
        return dot >= 0 ? name[(dot + 1)..] : name;
    }

    public static bool MatchValue(ModelQuery query, string value, string enumName)
    {
        if (query.Comparison is not ModelQueryComparison.Contains)
            return Compare(query, value, enumName);

        // Numbers are compared as numbers when exact, so 1 finds 1.0
        if (query.Exact && TryParseNumber(query.Value, out var inputNumber) && TryParseNumber(value, out var valueNumber))
            return inputNumber == valueNumber;

        return MatchText(query, query.Value, value, enumName);
    }

    /// <summary>
    /// The comparisons other than Contains: numbers as numbers, text as text
    /// </summary>
    private static bool Compare(ModelQuery query, string value, string enumName)
    {
        return Compare(query.Comparison, query.Value, query.Value2, value, enumName);
    }

    /// <summary>
    /// Compares the value of a field with an input. Contains matches the text or enum name; the others compare
    /// numbers as numbers, falling back on the text for equality.
    /// </summary>
    public static bool Compare(ModelQueryComparison comparison, string inputText, string input2, string value, string enumName)
    {
        var input = inputText?.Trim() ?? "";

        if (comparison is ModelQueryComparison.Contains)
        {
            return !string.IsNullOrEmpty(value) && value.Contains(input, StringComparison.OrdinalIgnoreCase) ||
                   !string.IsNullOrEmpty(enumName) && enumName.Contains(input, StringComparison.OrdinalIgnoreCase);
        }

        var hasNumbers = TryParseNumber(input, out var inputNumber) & TryParseNumber(value, out var number);

        switch (comparison)
        {
            case ModelQueryComparison.Equals:
                return IsEqual(hasNumbers, number, inputNumber, value, enumName, input);

            case ModelQueryComparison.NotEquals:
                return !IsEqual(hasNumbers, number, inputNumber, value, enumName, input);

            case ModelQueryComparison.Less:
                return hasNumbers && number < inputNumber;

            case ModelQueryComparison.LessOrEqual:
                return hasNumbers && number <= inputNumber;

            case ModelQueryComparison.Greater:
                return hasNumbers && number > inputNumber;

            case ModelQueryComparison.GreaterOrEqual:
                return hasNumbers && number >= inputNumber;

            case ModelQueryComparison.Between:
                // Either bound can be left out, and they can be given either way round
                if (!TryParseNumber(value, out number))
                    return false;

                var hasLow = TryParseNumber(input, out var low);
                var hasHigh = TryParseNumber(input2?.Trim(), out var high);

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

    public static bool TryParseNumber(string text, out double number)
    {
        number = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Booleans count as 0 and 1, so a flag can be found by number as well as by name
        if (bool.TryParse(text, out var flag))
        {
            number = flag ? 1 : 0;
            return true;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }

    /// <summary>
    /// Whether any of the texts contains the input, or equals it when the query is exact
    /// </summary>
    public static bool MatchText(ModelQuery query, string input, params string[] texts)
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
    #endregion

    #region Fields
    /// <summary>
    /// The fields of a FLVER object, read by reflection so they follow the properties the Properties view shows.
    /// Nested objects and lists of objects (a mesh's bounding box, a material's textures) are read into, named by
    /// their path; the geometry lists are left out.
    /// </summary>
    public static IEnumerable<ModelQueryField> GetFields(object obj)
    {
        var fields = new List<ModelQueryField>();

        AddFields(fields, obj, "", 0);

        // Not a property, but a count worth finding meshes by
        if (obj is FLVER2.Mesh mesh)
        {
            fields.Add(new("VertexCount", FormatValue(mesh.Vertices?.Count ?? 0), null));
        }

        return fields;
    }

    private static void AddFields(List<ModelQueryField> fields, object obj, string prefix, int depth)
    {
        if (obj == null)
            return;

        foreach (var property in GetProperties(obj.GetType()))
        {
            if (SkippedMembers.Contains(property.Name))
                continue;

            object value;

            try
            {
                value = property.GetValue(obj);
            }
            catch
            {
                continue;
            }

            AddValue(fields, $"{prefix}{property.Name}", property.PropertyType, value, depth);
        }
    }

    private static void AddValue(List<ModelQueryField> fields, string name, Type type, object value, int depth)
    {
        if (value == null)
        {
            if (type == typeof(string))
                fields.Add(new(name, "", null));

            return;
        }

        if (value is Enum enumValue)
        {
            var number = Convert.ToInt64(enumValue, CultureInfo.InvariantCulture);
            fields.Add(new(name, FormatValue(number), enumValue.ToString()));
            return;
        }

        if (IsSimple(value))
        {
            fields.Add(new(name, FormatValue(value), null));
            return;
        }

        if (value is IList list)
        {
            var elementType = list.GetType().IsGenericType ? list.GetType().GetGenericArguments()[0] : list.GetType().GetElementType();

            if (elementType != null && (elementType.IsPrimitive || elementType == typeof(string)))
            {
                // A list of numbers is one field, so an index can be found in it (a mesh's bone indices)
                if (list.Count <= MaxListValues)
                {
                    var values = new List<string>(list.Count);

                    foreach (var item in list)
                        values.Add(FormatValue(item));

                    fields.Add(new(name, string.Join(", ", values), null));
                }

                return;
            }

            if (depth >= MaxDepth || list.Count > MaxListObjects)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                AddFields(fields, list[i], $"{name}[{i}].", depth + 1);
            }

            return;
        }

        if (value is IEnumerable || depth >= MaxDepth || !type.IsClass)
            return;

        AddFields(fields, value, $"{name}.", depth + 1);
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t => Array.FindAll(
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            p => p.CanRead && p.GetIndexParameters().Length == 0));
    }

    private static bool IsSimple(object value) => value is string or bool or char or byte or sbyte or short or ushort
        or int or uint or long or ulong or float or double or decimal or Vector2 or Vector3 or Vector4 or Quaternion
        or System.Drawing.Color;

    public static string FormatValue(object value) => value switch
    {
        null => "",
        Vector2 v => $"{F(v.X)}, {F(v.Y)}",
        Vector3 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}",
        Vector4 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}, {F(v.W)}",
        Quaternion q => $"{F(q.X)}, {F(q.Y)}, {F(q.Z)}, {F(q.W)}",
        System.Drawing.Color c => $"{c.R}, {c.G}, {c.B}, {c.A}",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static string F(float value) => value.ToString(CultureInfo.InvariantCulture);
    #endregion
}
