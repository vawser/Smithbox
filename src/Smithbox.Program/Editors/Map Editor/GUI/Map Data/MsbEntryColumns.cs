using StudioCore.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The field columns of the Map Data entry table. Each entry type keeps its own columns, as field paths
/// (the paths of <see cref="MsbQueryMatcher.GetFields"/>, such as "Position" or "EntityGroupIDs[0]").
/// A type nobody has set up shows the default fields it has, so the columns follow the fields, not the game.
/// </summary>
public static class MsbEntryColumns
{
    /// <summary>
    /// The fields shown by default, in order, when the entry type has them
    /// </summary>
    private static readonly string[] DefaultFields =
    {
        "ModelName",
        "EntityID",
        "NPCParamID",
        "ThinkParamID",
        "TreasurePartName",
        "ItemLotID",
        "PartName",
        "RegionName",
        "Position"
    };

    private const int MaxDefaultColumns = 4;

    /// <summary>
    /// Fields shown after the defaults, past the limit, when the entry type has them: the per-session placement
    /// fields, which decide whether the entry exists at all (<see cref="NrPlacementFilter"/>)
    /// </summary>
    private static readonly string[] PlacementFields =
    {
        NrPlacementFilter.VariationField,
        NrPlacementFilter.RandomAppearField
    };

    private static readonly Dictionary<(Type, string), PropertyInfo> PropertyCache = new();

    /// <summary>
    /// The columns of an entry type: the ones set for it, or the defaults that the sample entry has
    /// </summary>
    public static List<string> GetColumns(Type entryType, object sample)
    {
        if (entryType == null)
            return new List<string>();

        if (CFG.Current.MapEditor.MapDataEditor_EntryView_Columns.TryGetValue(GetKey(entryType), out var columns))
            return columns;

        if (sample == null)
            return new List<string>();

        return DefaultFields
            .Where(path => TryGetValue(sample, path, out _))
            .Take(MaxDefaultColumns)
            .Concat(PlacementFields.Where(path => TryGetValue(sample, path, out _)))
            .ToList();
    }

    /// <summary>
    /// Sets the columns of an entry type, which are kept in the settings
    /// </summary>
    public static void SetColumns(Type entryType, List<string> columns)
    {
        CFG.Current.MapEditor.MapDataEditor_EntryView_Columns[GetKey(entryType)] = columns;
    }

    /// <summary>
    /// Returns an entry type to its default columns
    /// </summary>
    public static void ResetColumns(Type entryType)
    {
        CFG.Current.MapEditor.MapDataEditor_EntryView_Columns.Remove(GetKey(entryType));
    }

    public static bool HasCustomColumns(Type entryType)
    {
        return CFG.Current.MapEditor.MapDataEditor_EntryView_Columns.ContainsKey(GetKey(entryType));
    }

    private static string GetKey(Type entryType) => entryType.FullName;

    /// <summary>
    /// The value of a field of an entry as it is shown in a cell, or empty if the entry has no such field
    /// </summary>
    public static string GetText(object entry, string path)
    {
        return TryGetValue(entry, path, out var value) ? Format(value) : "";
    }

    /// <summary>
    /// The column header: the field's name, without the path of the classes it is in
    /// </summary>
    public static string GetHeader(string path)
    {
        var dot = path.LastIndexOf('.');
        return dot >= 0 ? path[(dot + 1)..] : path;
    }

    /// <summary>
    /// Reads the field at a path: names split by '.', each of which may be indexed, as in "Unk1.Values[2]"
    /// </summary>
    public static bool TryGetValue(object obj, string path, out object value)
    {
        var found = TryGetField(obj, path, out var field);
        value = field.Value;
        return found;
    }

    /// <summary>
    /// The field at a path, with the object that holds it, so it can be changed by a property change action
    /// </summary>
    public static bool TryGetField(object obj, string path, out MsbEntryField field)
    {
        field = default;

        if (obj == null || string.IsNullOrEmpty(path))
            return false;

        object value = obj;
        object owner = null;
        PropertyInfo lastProp = null;
        var lastIndex = -1;

        foreach (var segment in path.Split('.'))
        {
            if (value == null)
                return false;

            owner = value;

            var name = segment;
            var index = -1;

            var bracket = segment.IndexOf('[');

            if (bracket >= 0)
            {
                if (!segment.EndsWith(']') ||
                    !int.TryParse(segment[(bracket + 1)..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    return false;
                }

                name = segment[..bracket];
            }

            var prop = GetProperty(value.GetType(), name);

            if (prop == null)
                return false;

            try
            {
                value = prop.GetValue(value);
            }
            catch
            {
                return false;
            }

            if (index >= 0)
            {
                if (value is not IList list || index >= list.Count)
                    return false;

                value = list[index];
            }

            lastProp = prop;
            lastIndex = index;
        }

        Type valueType;

        if (lastIndex >= 0)
        {
            valueType = lastProp.PropertyType.IsArray
                ? lastProp.PropertyType.GetElementType()
                : MsbQueryMatcher.GetListElementType(lastProp.PropertyType);
        }
        else
        {
            valueType = lastProp.PropertyType;
        }

        field = new MsbEntryField(owner, lastProp, lastIndex, valueType, value);
        return true;
    }

    /// <summary>
    /// A field's value as it is written into a cell being edited: in full, so an edit keeps the precision of the
    /// parts the user didn't change
    /// </summary>
    public static string GetEditText(object value) => MsbQueryMatcher.FormatValue(value);

    private static PropertyInfo GetProperty(Type type, string name)
    {
        if (!PropertyCache.TryGetValue((type, name), out var prop))
        {
            prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            // Indexers aren't fields
            if (prop != null && prop.GetIndexParameters().Length > 0)
                prop = null;

            PropertyCache[(type, name)] = prop;
        }

        return prop;
    }

    /// <summary>
    /// Values as the search writes them, with floats kept short so vectors fit in a cell
    /// </summary>
    private static string Format(object value) => value switch
    {
        float f => F(f),
        Vector2 v => $"{F(v.X)}, {F(v.Y)}",
        Vector3 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}",
        Vector4 v => $"{F(v.X)}, {F(v.Y)}, {F(v.Z)}, {F(v.W)}",
        _ => MsbQueryMatcher.FormatValue(value)
    };

    private static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>
/// A field found at a path: the object holding it, its property, the index into it (or -1), and its value's type
/// </summary>
public readonly record struct MsbEntryField(object Owner, PropertyInfo Property, int Index, Type ValueType, object Value)
{
    /// <summary>
    /// Whether the field can be set: a writable property, or an element of an array or list
    /// </summary>
    public bool CanSet => Property != null && (Index >= 0 || Property.CanWrite) &&
                          MsbMassEditStep.GetKind(ValueType) != MsbMassEditFieldKind.Unsupported;
}
