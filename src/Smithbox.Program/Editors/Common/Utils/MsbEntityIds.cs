using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace StudioCore.Editors.Common;

/// <summary>
/// Reads and writes the entity ID of an MSB entry, wherever its format keeps it:
/// - <c>EntityID</c> on the entry (most games, and every event);
/// - <c>Common_EntityID</c> on Nightreign-format regions (NR, DB_NT);
/// - <c>EntityData.EntityID</c> on Nightreign-format parts, inside the part's entity struct.
/// A lookup of "EntityID" on the entry finds only the first, so tools that did that skipped every NR part and
/// region. The path is found once per type. Values are int in the older games and uint in the newer ones; they are
/// returned as long so both compare the same way.
/// </summary>
public static class MsbEntityIds
{
    private static readonly string[][] Paths =
    {
        new[] { "EntityID" },
        new[] { "Common_EntityID" },
        new[] { "EntityData", "EntityID" }
    };

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PathCache = new();

    /// <summary>The property chain to the entry type's entity ID, or null if it has none</summary>
    public static PropertyInfo[] GetPath(Type type)
    {
        if (type == null)
            return null;

        return PathCache.GetOrAdd(type, t =>
        {
            foreach (var path in Paths)
            {
                var chain = new PropertyInfo[path.Length];
                var current = t;
                var ok = true;

                for (var i = 0; i < path.Length; i++)
                {
                    var prop = current.GetProperty(path[i], BindingFlags.Public | BindingFlags.Instance);

                    if (prop == null || prop.GetIndexParameters().Length > 0)
                    {
                        ok = false;
                        break;
                    }

                    chain[i] = prop;
                    current = prop.PropertyType;
                }

                if (ok && (current == typeof(int) || current == typeof(uint)))
                    return chain;
            }

            return null;
        });
    }

    /// <summary>Whether the entry's type has an entity ID</summary>
    public static bool Has(object entry) => entry != null && GetPath(entry.GetType()) != null;

    /// <summary>
    /// The object that holds the entity ID property, and the property: for a property change action
    /// </summary>
    public static bool TryGetField(object entry, out object owner, out PropertyInfo property)
    {
        owner = null;
        property = null;

        var path = entry == null ? null : GetPath(entry.GetType());

        if (path == null)
            return false;

        owner = entry;

        for (var i = 0; i < path.Length - 1; i++)
        {
            owner = path[i].GetValue(owner);

            if (owner == null)
                return false;
        }

        property = path[^1];
        return true;
    }

    /// <summary>The entry's entity ID; false if it has none</summary>
    public static bool TryGet(object entry, out long id)
    {
        id = 0;

        if (!TryGetField(entry, out var owner, out var property))
            return false;

        id = property.GetValue(owner) switch
        {
            int i => i,
            uint u => u,
            _ => 0
        };

        return true;
    }

    /// <summary>The entry's entity ID, or 0 if it has none (−1 stays −1 in the int games)</summary>
    public static long Get(object entry) => TryGet(entry, out var id) ? id : 0;

    /// <summary>The entry's entity ID as the newer games store it (uint), or 0</summary>
    public static uint GetUInt(object entry) => unchecked((uint)Get(entry));

    /// <summary>The type of the entry's entity ID (int or uint), or null</summary>
    public static Type GetValueType(object entry)
    {
        var path = entry == null ? null : GetPath(entry.GetType());
        return path?[^1].PropertyType;
    }

    /// <summary>
    /// Sets the entry's entity ID, converted to the type it is stored as. False if it has none.
    /// </summary>
    public static bool Set(object entry, long value)
    {
        if (!TryGetField(entry, out var owner, out var property))
            return false;

        property.SetValue(owner, ToStoredType(property.PropertyType, value));
        return true;
    }

    /// <summary>A value as the entity ID's type stores it</summary>
    public static object ToStoredType(Type type, long value) =>
        type == typeof(uint) ? unchecked((uint)value) : (object)unchecked((int)value);

    /// <summary>
    /// The field path of the entity ID as the Map Data view and mass edit name it ("EntityID", "Common_EntityID",
    /// "EntityData.EntityID"), or null
    /// </summary>
    public static string GetFieldPath(Type type)
    {
        var path = GetPath(type);
        return path == null ? null : string.Join(".", Array.ConvertAll(path, p => p.Name));
    }
}
