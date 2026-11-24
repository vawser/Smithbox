#nullable enable annotations
using Havok;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The fields of an open Havok file, as they are now (unsaved edits included), against the game's copy of the same
/// file: for the Properties window's field colours.
/// <para>
/// The two object graphs are paired by walking them together from the root: the same field of paired objects, and
/// the same index of paired arrays and lists, pair up. An object with no pair (a list entry beyond the game's count,
/// or one added in the editor) is Added. A value field is Modified when it differs from its pair; a class, array or
/// list node is Modified when anything below it differs.
/// </para>
/// The game's file is read once, in the background (HavokBank.ReadVanillaDocument); pairings and statuses are
/// rebuilt after each edit, undo or redo (Invalidate). Plan: Claude/Documentation/UI.md (Vanilla state).
/// </summary>
public class HavokFieldCompare
{
    private readonly HavokBank Bank;

    private sealed class FileState
    {
        public HavokDocument? Vanilla;
        public bool Loaded;
        public object? PairedRoot;
        public Dictionary<object, object> Pairs = new(ReferenceEqualityComparer.Instance);
        public Dictionary<object, VanillaStatus> NodeStatus = new(ReferenceEqualityComparer.Instance);
    }

    private readonly Dictionary<HavokFileWrapper, FileState> States = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<HavokFileWrapper> Loading = new(ReferenceEqualityComparer.Instance);
    private readonly object LoadLock = new();

    public HavokFieldCompare(HavokBank bank)
    {
        Bank = bank;
    }

    /// <summary>Drops the pairings and statuses (an edit, undo or redo may have changed any of them)</summary>
    public void Invalidate()
    {
        lock (LoadLock)
        {
            foreach (var state in States.Values)
            {
                state.PairedRoot = null;
                state.Pairs.Clear();
                state.NodeStatus.Clear();
            }
        }
    }

    /// <summary>
    /// The file's state with its pairs built, or null while the game's copy is being read (or can't be: then every
    /// status is None)
    /// </summary>
    private FileState? GetState(HavokFileWrapper wrapper, FileDictionaryEntry binder, bool isCombined, Func<Type, FieldInfo[]> fields)
    {
        if (wrapper?.Document?.Root == null || binder == null)
            return null;

        FileState state;
        lock (LoadLock)
        {
            if (!States.TryGetValue(wrapper, out state))
            {
                if (Loading.Add(wrapper))
                {
                    var key = wrapper.Key;
                    Task.Run(() =>
                    {
                        HavokDocument? vanilla = null;
                        try
                        {
                            vanilla = Bank.ReadVanillaDocument(binder, isCombined, key);
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, $"Failed to read the game's copy of {key} for vanilla comparison", StudioCore.Logger.LogPriority.Low, e);
                        }

                        lock (LoadLock)
                        {
                            States[wrapper] = new FileState { Vanilla = vanilla, Loaded = true };
                            Loading.Remove(wrapper);
                        }
                    });
                }

                return null;
            }
        }

        var root = wrapper.Document.Root;

        if (state.Vanilla?.Root != null && !ReferenceEquals(state.PairedRoot, root))
        {
            state.Pairs.Clear();
            state.NodeStatus.Clear();
            Pair(root, state.Vanilla.Root, state.Pairs, fields);
            state.PairedRoot = root;
        }

        return state;
    }

    /// <summary>
    /// A class object (a tree node): Added with no pair, Modified when anything below it differs, else Unchanged.
    /// Every object of a file the game doesn't have is Added.
    /// </summary>
    public VanillaStatus ObjectStatus(HavokFileWrapper wrapper, FileDictionaryEntry binder, bool isCombined, Func<Type, FieldInfo[]> fields, object obj)
    {
        var state = GetState(wrapper, binder, isCombined, fields);
        if (state == null || obj == null)
            return VanillaStatus.None;

        if (state.Vanilla?.Root == null)
            return VanillaStatus.Added;

        return NodeStatus(state, obj, fields, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    /// <summary>
    /// A field of an object: for a value field (or one element of a value array / list, <paramref name="index" />),
    /// Modified when it differs from its pair, Added beyond the game's count; for a class, array or list field, as
    /// the node below it.
    /// </summary>
    public VanillaStatus FieldStatus(HavokFileWrapper wrapper, FileDictionaryEntry binder, bool isCombined, Func<Type, FieldInfo[]> fields,
        object obj, FieldInfo prop, int index = -1)
    {
        var state = GetState(wrapper, binder, isCombined, fields);
        if (state == null || obj == null || prop == null)
            return VanillaStatus.None;

        if (state.Vanilla?.Root == null)
            return VanillaStatus.Added;

        if (!state.Pairs.TryGetValue(obj, out var twin))
            return VanillaStatus.Added;

        var a = prop.GetValue(obj);
        var b = prop.GetValue(twin);

        if (index >= 0)
        {
            if (a is not IList listA)
                return VanillaStatus.None;
            if (b is not IList listB || index >= listB.Count)
                return VanillaStatus.Added;

            return ValueStatus(state, listA[index], listB[index], fields, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        return ValueStatus(state, a, b, fields, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    // ---- pairing and comparison

    private static bool IsNode(object value) => value != null && value is not string && !value.GetType().IsValueType && value is not IList;

    private static void Pair(object a, object b, Dictionary<object, object> pairs, Func<Type, FieldInfo[]> fields)
    {
        // Iterative, as behaviour graphs are deep; a pair already made (a shared or cyclic reference) isn't walked again
        var stack = new Stack<(object A, object B)>();
        stack.Push((a, b));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            if (x == null || y == null || x.GetType() != y.GetType() || pairs.ContainsKey(x))
                continue;

            pairs[x] = y;

            foreach (var f in fields(x.GetType()))
            {
                var vx = f.GetValue(x);
                var vy = f.GetValue(y);

                if (vx is IList lx && vy is IList ly)
                {
                    for (var i = 0; i < Math.Min(lx.Count, ly.Count); i++)
                    {
                        if (IsNode(lx[i]))
                            stack.Push((lx[i], ly[i]));
                    }
                }
                else if (IsNode(vx))
                {
                    stack.Push((vx, vy));
                }
            }
        }
    }

    private VanillaStatus NodeStatus(FileState state, object obj, Func<Type, FieldInfo[]> fields, HashSet<object> visiting)
    {
        if (state.NodeStatus.TryGetValue(obj, out var cached))
            return cached;

        if (!state.Pairs.TryGetValue(obj, out var twin))
            return VanillaStatus.Added;

        // A reference back into an object being compared: judged where it was first reached
        if (!visiting.Add(obj))
            return VanillaStatus.Unchanged;

        var status = VanillaStatus.Unchanged;

        foreach (var f in fields(obj.GetType()))
        {
            if (ValueStatus(state, f.GetValue(obj), f.GetValue(twin), fields, visiting) != VanillaStatus.Unchanged)
            {
                status = VanillaStatus.Modified;
                break;
            }
        }

        visiting.Remove(obj);
        state.NodeStatus[obj] = status;
        return status;
    }

    private VanillaStatus ValueStatus(FileState state, object a, object b, Func<Type, FieldInfo[]> fields, HashSet<object> visiting)
    {
        if (a == null && b == null)
            return VanillaStatus.Unchanged;

        if (a == null || b == null)
            return VanillaStatus.Modified;

        if (a is IList la)
        {
            if (b is not IList lb || la.Count != lb.Count)
                return VanillaStatus.Modified;

            for (var i = 0; i < la.Count; i++)
            {
                if (ValueStatus(state, la[i], lb[i], fields, visiting) != VanillaStatus.Unchanged)
                    return VanillaStatus.Modified;
            }

            return VanillaStatus.Unchanged;
        }

        if (IsNode(a))
        {
            var node = NodeStatus(state, a, fields, visiting);
            return node == VanillaStatus.Added ? VanillaStatus.Modified : node;
        }

        return Equals(a, b) ? VanillaStatus.Unchanged : VanillaStatus.Modified;
    }
}
