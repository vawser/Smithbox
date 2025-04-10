using HKLib.hk2018;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor.Util;

public static class ObjectHierarchyScanner
{
    public static Dictionary<Type, List<object>> GetAllObjectsByType(object root)
    {
        var typeMap = new Dictionary<Type, List<object>>();
        var visited = new HashSet<object>();
        Traverse(root, typeMap, visited);
        return typeMap;
    }

    private static void Traverse(object obj, Dictionary<Type, List<object>> typeMap, HashSet<object> visited)
    {
        if (obj == null || visited.Contains(obj))
            return;

        visited.Add(obj);

        Type type = obj.GetType();

        // Only include havok objects
        if (typeof(hkReferencedObject).IsAssignableFrom(type))
        {
            if (!typeMap.TryGetValue(type, out var list))
            {
                list = new List<object>();
                typeMap[type] = list;
            }

            list.Add(obj);
        }

        // Traverse fields
        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object fieldValue = field.GetValue(obj);
            HandleValue(fieldValue, typeMap, visited);
        }

        // Optional: Traverse properties
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (property.GetIndexParameters().Length == 0 && property.CanRead)
            {
                try
                {
                    object propertyValue = property.GetValue(obj);
                    HandleValue(propertyValue, typeMap, visited);
                }
                catch
                {
                    // Ignore property access exceptions
                }
            }
        }
    }

    private static void HandleValue(object value, Dictionary<Type, List<object>> typeMap, HashSet<object> visited)
    {
        if (value == null)
            return;

        if (value is IEnumerable enumerable && !(value is string))
        {
            foreach (var item in enumerable)
            {
                Traverse(item, typeMap, visited);
            }
        }
        else
        {
            Traverse(value, typeMap, visited);
        }
    }
}