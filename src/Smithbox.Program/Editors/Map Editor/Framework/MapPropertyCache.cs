using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class MapPropertyCache
{
    public readonly Dictionary<string, PropertyInfo[]> PropCache = new();

    public MapPropertyCache()
    { }

    public PropertyInfo[] GetCachedFields(object obj)
    {
        return GetCachedProperties(obj.GetType());
    }

    public PropertyInfo[] GetCachedProperties(Type type)
    {
        if (!PropCache.TryGetValue(type.FullName, out PropertyInfo[] props))
        {
            props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken)
                .ToArray();
            PropCache.Add(type.FullName, props);
        }

        return props;
    }

    public readonly Dictionary<string, FieldInfo[]> FieldCache = new();

    public FieldInfo[] GetCachedHavokFields(Type type)
    {
        if (!FieldCache.TryGetValue(type.FullName, out FieldInfo[] fields))
        {
            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            fields = fields.OrderBy(f => f.MetadataToken).ToArray();
            FieldCache.Add(type.FullName, fields);
        }

        return fields;
    }
}
