using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Copies a behavior object and what's below it.
///
/// An object below the root that's also referenced from outside the copied subtree is shared in the game's
/// files (ER c0000: 4,839 clips, 97 CMSGs), so by default the copy keeps referencing it rather than duplicating it.
/// Transition effects are always kept shared: one ER CustomTransitionEffect is used in 1,163 places.
/// Objects referenced twice inside the subtree are copied once, so the copy has the same internal sharing.
/// </summary>
public static class HavokBehaviorCloner
{
    private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// A copy of the root and the objects below it.
    /// With keepShared, objects with a parent outside the subtree are referenced instead of copied.
    /// </summary>
    public static object Clone(object root, HavokBehaviorSymbols symbols, bool keepShared)
        => Clone(root, symbols, keepShared, null, out _);

    /// <summary>
    /// As <see cref="Clone(object, HavokBehaviorSymbols, bool)"/>, but the objects in alwaysCopy are copied even if
    /// they're shared (Duplicate copies the selector or state machine it was asked for, inside a copy of its state).
    /// copies maps each original object to its copy (an object kept shared isn't in it).
    /// </summary>
    public static object Clone(object root, HavokBehaviorSymbols symbols, bool keepShared, IReadOnlySet<object> alwaysCopy,
        out IReadOnlyDictionary<object, object> copies)
    {
        var subtree = keepShared ? CollectSubtree(root) : null;
        var copyMap = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

        copies = copyMap;

        return CloneValue(root, root, symbols, subtree, copyMap, alwaysCopy, false);
    }

    /// <summary>
    /// A copy of the root and everything below it, sharing nothing with the source, transition effects included:
    /// for pasting into another file. Objects referenced twice inside are still copied once.
    /// </summary>
    public static object CloneAll(object root, out IReadOnlyDictionary<object, object> copies)
    {
        var copyMap = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        copies = copyMap;

        return CloneValue(root, root, null, null, copyMap, null, true);
    }

    /// <summary>
    /// A copy of the root and everything below it, except objects <paramref name="substitute"/> gives a replacement for
    /// (e.g. the target file's own object of the same name), which are referenced instead and not looked into.
    /// <paramref name="copies"/> is shared between calls, so objects copied by an earlier call are reused rather than
    /// copied again; <paramref name="added"/> gets the (original, copy) pairs this call made.
    /// </summary>
    public static object CloneWith(object root, Func<object, object> substitute, Dictionary<object, object> copies,
        List<(object Original, object Copy)> added)
    {
        var before = new HashSet<object>(copies.Keys, ReferenceEqualityComparer.Instance);
        var copy = CloneValue(root, root, null, null, copies, null, true, substitute);

        foreach (var (original, made) in copies)
        {
            if (!before.Contains(original))
                added.Add((original, made));
        }

        return copy;
    }

    private static object CloneValue(object value, object root, HavokBehaviorSymbols symbols,
        HashSet<object> subtree, Dictionary<object, object> copies, IReadOnlySet<object> alwaysCopy, bool copyAll,
        Func<object, object> substitute = null)
    {
        if (value == null)
            return null;

        var type = value.GetType();

        if (type.IsValueType || value is string)
            return value;

        if (copies.TryGetValue(value, out var existing))
            return existing;

        if (substitute != null && substitute(value) is { } replacement)
            return replacement;

        if (!copyAll && !ReferenceEquals(value, root) && alwaysCopy?.Contains(value) != true)
        {
            if (IsTransitionEffect(type))
                return value;

            // Used from outside the copied subtree too: keep the reference
            if (subtree != null && symbols.GetParents(value).Any(p => !subtree.Contains(p)))
                return value;
        }

        if (value is Array array)
        {
            var arrayCopy = Array.CreateInstance(type.GetElementType()!, array.Length);
            copies[value] = arrayCopy;

            for (var i = 0; i < array.Length; i++)
                arrayCopy.SetValue(CloneValue(array.GetValue(i), root, symbols, subtree, copies, alwaysCopy, copyAll, substitute), i);

            return arrayCopy;
        }

        if (value is IList list)
        {
            var listCopy = (IList)Activator.CreateInstance(type)!;
            copies[value] = listCopy;

            foreach (var entry in list)
                listCopy.Add(CloneValue(entry, root, symbols, subtree, copies, alwaysCopy, copyAll, substitute));

            return listCopy;
        }

        if (type.GetConstructor(Type.EmptyTypes) == null)
            return value;

        var copy = Activator.CreateInstance(type)!;
        copies[value] = copy;

        foreach (var field in type.GetFields(FieldFlags))
        {
            if (field.IsInitOnly)
                continue;

            field.SetValue(copy, CloneValue(field.GetValue(value), root, symbols, subtree, copies, alwaysCopy, copyAll, substitute));
        }

        return copy;
    }

    /// <summary>
    /// The root and every object reachable below it, not looking into transition effects
    /// </summary>
    private static HashSet<object> CollectSubtree(object root)
    {
        var subtree = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();

        stack.Push(root);

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            if (obj == null || !subtree.Add(obj))
                continue;

            if (!ReferenceEquals(obj, root) && IsTransitionEffect(obj.GetType()))
                continue;

            foreach (var field in obj.GetType().GetFields(FieldFlags))
            {
                if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    continue;

                var value = field.GetValue(obj);

                if (value is IList entries)
                {
                    // The list itself isn't an object in the parent map; its entries' parent is obj
                    foreach (var entry in entries)
                    {
                        if (entry != null && entry.GetType().IsClass && entry is not string)
                            stack.Push(entry);
                    }
                }
                else if (value != null && value is not string)
                {
                    stack.Push(value);
                }
            }
        }

        return subtree;
    }

    private static bool IsTransitionEffect(Type type)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name == "hkbTransitionEffect")
                return true;
        }

        return false;
    }
}
