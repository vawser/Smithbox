using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;
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

    /// <summary>
    /// A class's name without its SDK namespace and with nesting flattened, as packfiles name classes:
    /// Havok.hk2014.hkRootLevelContainer+NamedVariant and HKX2.hkRootLevelContainerNamedVariant are both
    /// hkRootLevelContainerNamedVariant.
    /// </summary>
    private static string FlatName(Type t)
    {
        string full = StripArity(t.FullName ?? t.Name);
        foreach (var root in new[] { "HKLib.hk2018.", "HKLib.hk2019.", "HKX2." })
            if (full.StartsWith(root)) { full = full[root.Length..]; break; }
        if (full.StartsWith("Havok.hk") && full.IndexOf('.', 6) is var dot and > 0) full = full[(dot + 1)..];
        int generic = full.IndexOf('[');
        if (generic >= 0) full = full[..generic];
        return full.Replace("+", "").Replace(".", "");
    }

    private static decimal Numeric(object v) => v.GetType().IsEnum ? EnumValue(v) : Convert.ToDecimal(v);

    /// <summary>
    /// The floats of a matrix or of a record of vectors, in order, for comparing different representations of one value.
    /// </summary>
    private static List<float> Floats(object v)
    {
        var result = new List<float>();
        switch (v)
        {
            case Matrix4x4 m:
                for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) result.Add(m[i, j]);
                break;
            case Vector4 vec: result.AddRange(new[] { vec.X, vec.Y, vec.Z, vec.W }); break;
            case Quaternion q: result.AddRange(new[] { q.X, q.Y, q.Z, q.W }); break;
            case float f: result.Add(f); break;
            default:
                foreach (var f in Fields(v.GetType()))
                {
                    var value = f.GetValue(v);
                    if (value != null) result.AddRange(Floats(value));
                }
                break;
        }
        return result;
    }

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

            // hkHalf: HKX2 keeps the raw 16 bits, the generated classes the float they stand for
            if ((a is short or ushort && b is float) || (b is short or ushort && a is float))
            {
                float half = BitConverter.Int32BitsToSingle((a is float ? Convert.ToInt32(b is short s ? (ushort)s : b) : Convert.ToInt32(a is short s2 ? (ushort)s2 : a)) << 16);
                float full = a is float fa0 ? fa0 : (float)b;
                return BitConverter.SingleToInt32Bits(half) == BitConverter.SingleToInt32Bits(full) ? null : $"{path}: half {half} vs {full}";
            }

            // HKLib's Matrix3x3 and Havok.Core's: the same 3 rows of 4 floats
            if (ta.Name == "Matrix3x3" && tb.Name == "Matrix3x3")
                return Floats(a).Select(BitConverter.SingleToInt32Bits).SequenceEqual(Floats(b).Select(BitConverter.SingleToInt32Bits))
                    ? null : $"{path}: matrix differs";

            // an enum on one side and its raw storage on the other (HKX2 types some enum fields as plain ints)
            if ((ta.IsEnum || ta.IsPrimitive) && (tb.IsEnum || tb.IsPrimitive) && (ta.IsEnum || tb.IsEnum))
                return Numeric(a) == Numeric(b) ? null : $"{path}: {a} vs {b}";

            // HKX2 stores hkQsTransform, hkMatrix3 and hkRotation as a Matrix4x4 of rows; the generated classes as
            // a record of vectors or a Matrix3x3. The floats must match.
            if (ta == typeof(Matrix4x4) || tb == typeof(Matrix4x4))
            {
                var fa = Floats(a);
                var fb = Floats(b);
                // a Matrix4x4 standing in for a 3-row value has a filler 4th row: only the first 12 floats are data
                int n = Math.Min(Math.Min(fa.Count, fb.Count), 12);
                if (n == 0) return $"{path}: type {ta.Name} vs {tb.Name}";
                for (int i = 0; i < n; i++)
                    if (BitConverter.SingleToInt32Bits(fa[i]) != BitConverter.SingleToInt32Bits(fb[i]))
                        return $"{path}: float {i} {fa[i]} vs {fb[i]}";
                return null;
            }

            if (FlatName(ta) != FlatName(tb) || ta.IsGenericType != tb.IsGenericType)
                return $"{path}: type {FlatName(ta)} vs {FlatName(tb)}";
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
                // only the fields both sides have: generated classes add variant fields, and HKX2 keeps some ignored ones
                if (!fieldsB.TryGetValue(f.Name, out var fb)) continue;
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
    /// Every object of a class (by its simple name) reachable from the root, in a fixed walk order.
    /// </summary>
    public static List<object> Instances(object root, string className)
    {
        var found = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var o = stack.Pop();
            if (o == null) continue;
            var t = o.GetType();
            if (IsLeaf(t) || t.IsValueType || !seen.Add(o)) continue;
            if (t.Name == className) found.Add(o);
            if (o is IList l) { for (int i = l.Count - 1; i >= 0; i--) stack.Push(l[i]); continue; }
            var fields = Fields(t);
            for (int i = fields.Length - 1; i >= 0; i--) stack.Push(fields[i].GetValue(o));
        }
        return found;
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
