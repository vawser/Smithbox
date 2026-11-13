using SoulsFormats;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// One field that differs between a map object and its vanilla counterpart. <see cref="Owner" /> is the current
/// object holding <see cref="Property" /> (the entry itself, or a nested struct of it), and <see cref="Index" /> the
/// element of a list field, or -1. Reverting sets the property (element) on the owner to <see cref="Vanilla" />.
/// </summary>
public sealed record MsbFieldDiff(string Path, object Owner, PropertyInfo Property, int Index, object Current, object Vanilla);

/// <summary>
/// Compares MSB entries with their vanilla counterparts. Self-contained (SoulsFormats and reflection only), so
/// Claude/Tools/VanillaCompareSurvey compiles this same file to check it against game data.
/// Plan: Documentation/Map Editor Vanilla Compare.md.
/// </summary>
public static class MsbDiff
{
    private const int MaxDepth = 8;

    /// <summary>The MSB list an entry belongs to (parts, regions or events), or null</summary>
    public static Type ListOf(object entry)
    {
        return entry switch
        {
            IMsbPart => typeof(IMsbPart),
            IMsbRegion => typeof(IMsbRegion),
            IMsbEvent => typeof(IMsbEvent),
            _ => null
        };
    }

    /// <summary>
    /// Pairs current entries with vanilla ones by list and name; the k-th entry of a repeated name pairs with the
    /// k-th. Returns the pairs (vanilla null for an entry vanilla doesn't have) and the vanilla entries left over.
    /// </summary>
    public static (List<(T Current, IMsbEntry Vanilla)> Pairs, List<IMsbEntry> Removed) Match<T>(
        IEnumerable<T> current, Func<T, IMsbEntry> entryOf, IEnumerable<IMsbEntry> vanilla)
    {
        var pool = new Dictionary<(Type, string), Queue<IMsbEntry>>();
        var vanillaList = vanilla.Where(v => ListOf(v) != null).ToList();

        foreach (var v in vanillaList)
        {
            var key = (ListOf(v), v.Name ?? "");

            if (!pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<IMsbEntry>();
                pool[key] = queue;
            }

            queue.Enqueue(v);
        }

        var pairs = new List<(T, IMsbEntry)>();
        var matched = new HashSet<IMsbEntry>(ReferenceEqualityComparer.Instance);

        foreach (var c in current)
        {
            var entry = entryOf(c);

            if (entry == null || ListOf(entry) == null)
                continue;

            IMsbEntry match = null;

            if (pool.TryGetValue((ListOf(entry), entry.Name ?? ""), out var queue) && queue.Count > 0)
            {
                match = queue.Dequeue();
                matched.Add(match);
            }

            pairs.Add((c, match));
        }

        var removed = vanillaList.Where(v => !matched.Contains(v)).ToList();

        return (pairs, removed);
    }

    /// <summary>Whether the two entries differ in any compared field (stops at the first difference)</summary>
    public static bool Differs(object current, object vanilla)
    {
        var diffs = new List<MsbFieldDiff>();
        CompareObject("", current, vanilla, diffs, 0, 1);
        return diffs.Count > 0;
    }

    /// <summary>The fields that differ, up to <paramref name="max" /></summary>
    public static List<MsbFieldDiff> Compare(object current, object vanilla, int max = 500)
    {
        var diffs = new List<MsbFieldDiff>();
        CompareObject("", current, vanilla, diffs, 0, max);
        return diffs;
    }

    // Property lists per type: public, readable, not indexers, without the index fields the save rewrites
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
        {
            var all = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && p.GetGetMethod() != null)
                .ToList();

            // The name references, split into stem and trailing digits (ObjPartName1 → "ObjPart", "1")
            var references = all
                .Where(p => p.GetCustomAttribute<MSBReference>() != null)
                .Select(p => SplitReference(p.Name))
                .Where(r => r.Stem != null)
                .ToList();

            return all.Where(p => !IsIndexField(p, references)).ToArray();
        });
    }

    private static readonly System.Text.RegularExpressions.Regex IndexPattern =
        new(@"^(?<stem>.*?)(Index|Indices|Indexes)(?<digits>\d*)$");

    private static readonly System.Text.RegularExpressions.Regex NamePattern =
        new(@"^(?<stem>.*?)(Name|Names)(?<digits>\d*)$");

    private static (string Stem, string Digits) SplitReference(string name)
    {
        var m = NamePattern.Match(name);
        return m.Success ? (m.Groups["stem"].Value, m.Groups["digits"].Value) : (null, null);
    }

    /// <summary>
    /// SoulsFormats keeps an index beside each name reference and rewrites it from the name on save (GetIndices),
    /// so an insertion anywhere would make every later reference look changed. Only the names are compared. The
    /// pairs are named loosely (CollisionName / CollisionPartIndex in DS3, ObjPartName1 / ObjPartIndex1 in Sekiro,
    /// SpawnPartNames / SpawnPartIndices), so an integer "…Index" field counts as one when an [MSBReference]
    /// sibling has the same trailing digits and a stem equal to, or a prefix of, its own (or the reverse).
    /// Fields like AC6's TypeIndex, with no such sibling, are real data and stay. Checked by the insertion test of
    /// Claude/Tools/VanillaCompareSurvey.
    /// </summary>
    private static bool IsIndexField(PropertyInfo prop, List<(string Stem, string Digits)> references)
    {
        var type = prop.PropertyType;
        var element = type.IsArray ? type.GetElementType() : type.IsGenericType ? type.GetGenericArguments()[0] : type;

        if (element != typeof(int) && element != typeof(short) && element != typeof(ushort) && element != typeof(uint))
            return false;

        var m = IndexPattern.Match(prop.Name);

        if (!m.Success)
            return false;

        var stem = m.Groups["stem"].Value;
        var digits = m.Groups["digits"].Value;

        return references.Any(r => r.Digits == digits && r.Stem.Length > 0 && stem.Length > 0 &&
            (stem == r.Stem || stem.StartsWith(r.Stem, StringComparison.Ordinal) || r.Stem.StartsWith(stem, StringComparison.Ordinal)));
    }

    private static bool IsLeaf(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type.IsValueType;
    }

    private static void CompareObject(string path, object current, object vanilla, List<MsbFieldDiff> diffs, int depth, int max)
    {
        foreach (var prop in GetProperties(current.GetType()))
        {
            if (diffs.Count >= max)
                return;

            object cv, vv;

            try
            {
                cv = prop.GetValue(current);
                vv = prop.GetValue(vanilla);
            }
            catch
            {
                // A property that can't be read on one of them (computed from state the copy lacks) isn't compared
                continue;
            }

            CompareValue(path + prop.Name, current, prop, -1, cv, vv, diffs, depth, max);
        }
    }

    private static void CompareValue(string path, object owner, PropertyInfo prop, int index, object cv, object vv,
        List<MsbFieldDiff> diffs, int depth, int max)
    {
        if (cv == null && vv == null)
            return;

        if (cv == null || vv == null || cv.GetType() != vv.GetType())
        {
            diffs.Add(new MsbFieldDiff(path, owner, prop, index, cv, vv));
            return;
        }

        var type = cv.GetType();

        if (IsLeaf(type) || depth >= MaxDepth)
        {
            if (!Equals(cv, vv))
            {
                diffs.Add(new MsbFieldDiff(path, owner, prop, index, cv, vv));
            }

            return;
        }

        if (cv is IList cl && vv is IList vl)
        {
            // A list of another length is replaced as a whole
            if (cl.Count != vl.Count)
            {
                diffs.Add(new MsbFieldDiff(path, owner, prop, index, cv, vv));
                return;
            }

            for (var i = 0; i < cl.Count && diffs.Count < max; i++)
            {
                var ce = cl[i];
                var ve = vl[i];

                if (ce != null && ve != null && ce.GetType() == ve.GetType() && !IsLeaf(ce.GetType()))
                {
                    // An element that is an object: its fields, owned by the element
                    CompareObject($"{path}[{i}].", ce, ve, diffs, depth + 1, max);
                }
                else if (index < 0)
                {
                    // A leaf element: reverted by element, as the list is the owner's property
                    CompareValue($"{path}[{i}]", owner, prop, i, ce, ve, diffs, depth + 1, max);
                }
                else if (!Equals(ce, ve))
                {
                    // A list inside a list element can't be set by element; the whole value differs
                    diffs.Add(new MsbFieldDiff(path, owner, prop, index, cv, vv));
                    return;
                }
            }

            return;
        }

        // A nested object (a shape, a later game's sub-struct): its fields, owned by it
        CompareObject(path + ".", cv, vv, diffs, depth + 1, max);
    }
}
