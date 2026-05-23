using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

public static class ReflectionHelper
{
    public static readonly Dictionary<string, PropertyInfo[]> PropCache = new();

    public static bool IsPropertyArray(Type type, object entry, PropertyInfo prop)
    {
        return prop.PropertyType.IsArray;
    }

    public static bool IsPropertyList(Type type, object entry, PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        return propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>);
    }

    public static bool IsPropertyClass(Type type, object entry, PropertyInfo prop)
    {
        var propType = prop.PropertyType;

        // Exclude string (class but scalar), arrays (handled separately),
        // and generic collections such as List<T> (also handled separately).
        if (!propType.IsClass || propType == typeof(string) || propType.IsArray)
            return false;

        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
            return false;

        return true;
    }

    public static bool IsScalarType(Type t)
    {
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t.IsValueType;
    }

    public static PropertyInfo[] GetCachedFields(object obj)
    {
        return GetCachedProperties(obj.GetType());
    }

    public static PropertyInfo[] GetCachedProperties(Type type)
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
