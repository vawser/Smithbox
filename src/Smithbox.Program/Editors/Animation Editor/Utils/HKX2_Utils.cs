using HKX2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public static class HKX2_Utils
{
    /// <summary>
    /// Finds all objects of type T within the Havok object graph
    /// </summary>
    public static List<T> FindAllOfType<T>(this IHavokObject root) where T : class
    {
        var results = new List<T>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        TraverseObject(root, results, visited, typeof(T));
        return results;
    }

    /// <summary>
    /// Finds all objects of a specific type within the Havok object graph
    /// </summary>
    public static List<object> FindAllOfType(this IHavokObject root, Type targetType)
    {
        var results = new List<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        TraverseObject(root, results, visited, targetType);
        return results;
    }

    private static void TraverseObject(object obj, IList results, HashSet<object> visited, Type targetType)
    {
        // Null check
        if (obj == null) return;

        // Prevent infinite loops from circular references
        if (!visited.Add(obj)) return;

        var objType = obj.GetType();

        // Check if this object matches the target type
        if (targetType.IsAssignableFrom(objType))
        {
            results.Add(obj);
        }

        // Skip primitive types and strings
        if (objType.IsPrimitive || objType == typeof(string) || objType.IsEnum)
            return;

        // Handle collections (Lists, Arrays, etc.)
        if (obj is IEnumerable enumerable && !(obj is string))
        {
            foreach (var item in enumerable)
            {
                TraverseObject(item, results, visited, targetType);
            }
            return;
        }

        // Traverse all fields and properties
        var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            TraverseObject(value, results, visited, targetType);
        }

        var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            // Skip indexed properties
            if (property.GetIndexParameters().Length > 0) continue;

            try
            {
                var value = property.GetValue(obj);
                TraverseObject(value, results, visited, targetType);
            }
            catch
            {
                // Skip properties that throw exceptions when accessed
            }
        }
    }

    private class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}