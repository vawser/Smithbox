using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Utilities for walking a Havok object graph (as reflected via HKLib) to find
/// every instance of a given type, independent of the ImGui property tree.
///
/// Mirrors the traversal rules used by HavokPropertyView.HavokPropEditGeneric:
/// arrays, List&lt;T&gt;, and plain reference-type fields are recursed into;
/// value types, strings, and enums are treated as leaves.
/// </summary>
public static class HavokTreeSearch
{
    private const BindingFlags DefaultFieldFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly Dictionary<Type, FieldInfo[]> DefaultFieldCache = new();

    /// <summary>
    /// Returns every instance of T found anywhere in the object graph rooted at <paramref name="root"/>.
    /// </summary>
    /// <param name="root">The object to start walking from (e.g. an hkRootLevelContainer).</param>
    /// <param name="fieldProvider">
    /// Optional field lookup, e.g. View.PropertyCache.GetCachedHavokFields, to keep this search
    /// in sync with whatever fields the property tree actually displays. Falls back to a
    /// locally-cached reflection lookup if omitted.
    /// </param>
    /// <param name="includeDerivedTypes">
    /// If true (default), matches T and any subclass of T (e.g. searching for hkbGenerator
    /// would also return hkbStateMachine instances). If false, only exact type matches.
    /// </param>
    public static List<T> FindAll<T>(
        object root,
        Func<Type, FieldInfo[]> fieldProvider = null,
        bool includeDerivedTypes = true) where T : class
    {
        var results = new List<T>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        Walk(root, fieldProvider ?? GetDefaultFields, visited, obj =>
        {
            if (includeDerivedTypes ? obj is T match : obj.GetType() == typeof(T))
            {
                results.Add((T)obj);
            }
        });

        return results;
    }

    /// <summary>
    /// Walks the object graph once and buckets every reference-typed object encountered by
    /// its exact runtime type. Prefer this over repeated FindAll&lt;T&gt; calls when you need
    /// instances of several different types (e.g. powering multiple discrete sub-editors),
    /// since it only walks the tree a single time.
    /// </summary>
    public static Dictionary<Type, List<object>> BuildTypeIndex(
        object root,
        Func<Type, FieldInfo[]> fieldProvider = null)
    {
        var index = new Dictionary<Type, List<object>>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        Walk(root, fieldProvider ?? GetDefaultFields, visited, obj =>
        {
            var type = obj.GetType();
            if (!index.TryGetValue(type, out var list))
            {
                list = new List<object>();
                index[type] = list;
            }
            list.Add(obj);
        });

        return index;
    }

    private static void Walk(
        object obj,
        Func<Type, FieldInfo[]> fieldProvider,
        HashSet<object> visited,
        Action<object> onVisit)
    {
        if (obj == null)
        {
            return;
        }

        Type type = obj.GetType();

        if (!type.IsClass || type == typeof(string))
        {
            return; // leaf: value type, enum, or string
        }

        // Reference dedup: also protects against cycles in the graph
        if (!visited.Add(obj))
        {
            return;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType != null && elementType.IsClass && elementType != typeof(string) && !elementType.IsArray)
            {
                foreach (var item in (Array)obj)
                {
                    Walk(item, fieldProvider, visited, onVisit);
                }
            }
            // primitive/value-element arrays (float[], byte[], etc.) - nothing further to walk
            return;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            if (elementType.IsClass && elementType != typeof(string) && !elementType.IsArray)
            {
                foreach (var item in (IList)obj)
                {
                    Walk(item, fieldProvider, visited, onVisit);
                }
            }
            return;
        }

        // Plain Havok class instance
        onVisit(obj);

        foreach (var field in fieldProvider(type))
        {
            var value = field.GetValue(obj);
            if (value != null)
            {
                Walk(value, fieldProvider, visited, onVisit);
            }
        }
    }

    private static FieldInfo[] GetDefaultFields(Type type)
    {
        if (!DefaultFieldCache.TryGetValue(type, out var fields))
        {
            fields = type.GetFields(DefaultFieldFlags);
            DefaultFieldCache[type] = fields;
        }
        return fields;
    }
}
