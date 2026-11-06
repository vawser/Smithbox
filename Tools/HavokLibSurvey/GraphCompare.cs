using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Compares two object graphs field by field, and lists the Havok classes in a graph. Works on the HKLib and HKX2
/// classes alike, since both store their data in fields. Floats compare by bits, so NaN equals NaN.
/// </summary>
public static class GraphCompare
{
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new();

    private sealed class PairComparer : IEqualityComparer<(object, object)>
    {
        public static readonly PairComparer Instance = new();
        public bool Equals((object, object) x, (object, object) y) => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
        public int GetHashCode((object, object) p) => HashCode.Combine(RuntimeHelpers.GetHashCode(p.Item1), RuntimeHelpers.GetHashCode(p.Item2));
    }

    /// <summary>
    /// Null if the graphs are equal, otherwise the path of the first difference.
    /// </summary>
    public static string Compare(object a, object b)
    {
        var seen = new HashSet<(object, object)>(PairComparer.Instance);
        return Compare(a, b, "root", seen);
    }

    [ThreadStatic] private static bool _byName;

    /// <summary>
    /// Compares graphs of two different class sets (HKLib's and a generated Havok.hkXXXX): types match by name, fields
    /// by name, enums by value.
    /// </summary>
    public static string CompareAcross(object a, object b)
    {
        _byName = true;
        try { return Compare(a, b); }
        finally { _byName = false; }
    }

    private static string StripArity(string name) => name.IndexOf('`') is var i and >= 0 ? name[..i] : name;

    private static long EnumValue(object e) =>
        Enum.GetUnderlyingType(e.GetType()) == typeof(ulong) ? unchecked((long)Convert.ToUInt64(e)) : Convert.ToInt64(e);

    private static FieldInfo[] Fields(Type t) => FieldCache.GetOrAdd(t, type =>
    {
        var list = new List<FieldInfo>();
        for (var cur = type; cur != null && cur != typeof(object); cur = cur.BaseType)
        {
            foreach (var f in cur.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (typeof(Delegate).IsAssignableFrom(f.FieldType))
                    continue;
                if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Lazy<>))
                    continue;
                list.Add(f);
            }
        }
        return list.ToArray();
    });

    private static bool IsLeaf(Type t) => t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);

    private static bool LeafEquals(object a, object b) => a switch
    {
        float fa => BitConverter.SingleToInt32Bits(fa) == BitConverter.SingleToInt32Bits((float)b),
        double da => BitConverter.DoubleToInt64Bits(da) == BitConverter.DoubleToInt64Bits((double)b),
        _ => a.Equals(b)
    };

    private static string Compare(object a, object b, string path, HashSet<(object, object)> seen)
    {
        if (a == null || b == null)
            return a == null && b == null ? null : $"{path}: {(a == null ? "null" : a.GetType().Name)} vs {(b == null ? "null" : b.GetType().Name)}";

        var ta = a.GetType();
        var tb = b.GetType();
        if (_byName && ta != tb)
        {
            if (ta.IsEnum && tb.IsEnum)
                return EnumValue(a) == EnumValue(b) ? null : $"{path}: {a} vs {b}";
            if (StripArity(ta.Name) != StripArity(tb.Name) || ta.IsGenericType != tb.IsGenericType)
                return $"{path}: type {ta.Name} vs {tb.Name}";
        }
        else if (ta != tb)
            return $"{path}: type {ta.Name} vs {tb.Name}";

        if (IsLeaf(ta))
            return LeafEquals(a, b) ? null : $"{path}: {a} vs {b}";

        if (!ta.IsValueType && !seen.Add((a, b)))
            return null;

        if (a is IList la)
        {
            var lb = (IList)b;
            if (la.Count != lb.Count)
                return $"{path}: count {la.Count} vs {lb.Count}";
            for (var i = 0; i < la.Count; i++)
            {
                var d = Compare(la[i], lb[i], $"{path}[{i}]", seen);
                if (d != null) return d;
            }
            return null;
        }

        if (a is IDictionary da)
        {
            var db = (IDictionary)b;
            if (da.Count != db.Count)
                return $"{path}: count {da.Count} vs {db.Count}";
            foreach (DictionaryEntry e in da)
            {
                if (!db.Contains(e.Key))
                    return $"{path}: key {e.Key} missing";
                var d = Compare(e.Value, db[e.Key], $"{path}[{e.Key}]", seen);
                if (d != null) return d;
            }
            return null;
        }

        if (_byName && ta != tb)
        {
            var fieldsB = Fields(tb).GroupBy(f => f.Name).ToDictionary(g => g.Key, g => g.First());
            foreach (var f in Fields(ta))
            {
                if (!fieldsB.TryGetValue(f.Name, out var fb)) return $"{path}.{f.Name}: missing in {tb.FullName}";
                var d = Compare(f.GetValue(a), fb.GetValue(b), $"{path}.{f.Name}", seen);
                if (d != null) return d;
            }
            // fields only the second graph's classes have are fine: generated classes hold every game variant's fields
            return null;
        }

        foreach (var f in Fields(ta))
        {
            var d = Compare(f.GetValue(a), f.GetValue(b), $"{path}.{f.Name}", seen);
            if (d != null) return d;
        }
        return null;
    }

    /// <summary>
    /// Counts of the Havok class names reachable from the root (classes in the HKLib or HKX2 namespaces).
    /// </summary>
    public static List<string> HavokClasses(object root)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var names = new HashSet<string>();
        var stack = new Stack<object>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var o = stack.Pop();
            if (o == null) continue;
            var t = o.GetType();
            if (IsLeaf(t)) continue;
            if (!t.IsValueType && !seen.Add(o)) continue;

            if (t.Namespace != null && (t.Namespace.StartsWith("HKLib.hk20") || t.Namespace.StartsWith("Havok.hk20") || t.Namespace == "HKX2"))
                names.Add(t.Name);

            if (o is IList l)
            {
                foreach (var e in l) stack.Push(e);
                continue;
            }
            if (o is IDictionary d)
            {
                foreach (DictionaryEntry e in d) stack.Push(e.Value);
                continue;
            }
            foreach (var f in Fields(t))
                stack.Push(f.GetValue(o));
        }
        return names.OrderBy(n => n).ToList();
    }
}
