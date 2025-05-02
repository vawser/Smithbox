using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor.PropertyEditor;
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
            props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            props = props.OrderBy(p => p.MetadataToken).ToArray();
            PropCache.Add(type.FullName, props);
        }

        return props;
    }
}
