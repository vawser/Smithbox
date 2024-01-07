using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.MsbEditor;
public class PropertyCache
{
    public readonly Dictionary<string, PropertyInfo[]> PropCache = new();

    public PropertyCache()
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
