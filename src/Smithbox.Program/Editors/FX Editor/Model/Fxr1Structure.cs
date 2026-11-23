using SoulsFormats;
using System.Collections;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Structure edits on an FXR1 / FXR v2 effect (DS1, DS1R, BB; FFX Older Games Plan §21.8, F7b): a root's states, their
/// exits and actions, and the entries of call lists, with actions and exits from vanilla templates. An FXR1 effect is
/// an object graph: an exit's target is the <see cref="FXR1.FXState"/> itself (null ends the effect), so states
/// reorder without renumbering, and a deleted state's incoming exits end the effect. SoulsFormats + BCL only.
///
/// Copies: the whole file is written and read back (a separate graph), and the copy is the object at the same place
/// in a deterministic walk of both. A reference from the copied part to an object outside it (an exit's target state,
/// a node shared with another action) is pointed back at the original's object, so the copy shares what the original
/// shared. The writer lays out any graph (§17), so new and removed objects need nothing more.
/// </summary>
public static class Fxr1Structure
{
    // ---------------------------------------------------------------- the graph walk

    private static readonly Dictionary<Type, FieldInfo[]> fields = new();

    /// <summary>The FX object fields of a type, in declaration order (the same every run).</summary>
    private static FieldInfo[] Fields(Type t)
    {
        lock (fields)
        {
            if (!fields.TryGetValue(t, out var f))
                fields[t] = f = t.GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => IsFx(x.FieldType) || IsFxList(x.FieldType))
                    .OrderBy(x => x.MetadataToken).ToArray();
            return f;
        }
    }

    private static bool IsFx(Type t) => t.IsClass && t != typeof(string) && t.FullName?.StartsWith("SoulsFormats.FXR1+") == true;

    private static bool IsFxList(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && IsFx(t.GetGenericArguments()[0]);

    /// <summary>The FX objects an object refers to, in field order (lists in order).</summary>
    private static IEnumerable<object> Children(object o)
    {
        foreach (var f in Fields(o.GetType()))
        {
            var v = f.GetValue(o);
            if (v == null) continue;
            if (v is IList list)
            {
                foreach (var x in list)
                    if (x != null)
                        yield return x;
            }
            else yield return v;
        }
    }

    /// <summary>Every FX object of the graph from <paramref name="root"/>, each once, depth first in field order.</summary>
    public static List<object> Walk(object root)
    {
        var order = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        if (root != null) stack.Push(root);
        while (stack.Count > 0)
        {
            var o = stack.Pop();
            if (!seen.Add(o)) continue;
            order.Add(o);
            foreach (var c in Children(o).Reverse())
                if (!seen.Contains(c))
                    stack.Push(c);
        }
        return order;
    }

    /// <summary>The objects of the subtree under <paramref name="obj"/> that it owns: reached from it without going
    /// through an exit's target state.</summary>
    private static HashSet<object> Subtree(object obj)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        stack.Push(obj);
        while (stack.Count > 0)
        {
            var o = stack.Pop();
            if (!seen.Add(o)) continue;
            foreach (var f in Fields(o.GetType()))
            {
                if (o is FXR1.FXTransition && f.Name == nameof(FXR1.FXTransition.TargetState)) continue;
                var v = f.GetValue(o);
                if (v is IList l) { foreach (var x in l) if (x != null) stack.Push(x); }
                else if (v != null) stack.Push(v);
            }
        }
        return seen;
    }

    // ---------------------------------------------------------------- copies

    /// <summary>Deep copies of <paramref name="objects"/> (all of one file), sharing what lies outside them with the original.</summary>
    public static List<T> Clone<T>(FXR1 file, IReadOnlyList<T> objects) where T : class
    {
        var copy = FXR1.Read(file.Write());
        var original = Walk(file.RootFXNode);
        var copied = Walk(copy.RootFXNode);
        var index = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < original.Count; i++) index[original[i]] = i;
        var copyIndex = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < copied.Count; i++) copyIndex[copied[i]] = i;

        var result = new List<T>();
        foreach (var obj in objects)
        {
            if (!index.TryGetValue(obj, out int at) || at >= copied.Count)
                throw new InvalidOperationException("The object isn't in the file's graph.");
            var c = (T)copied[at];
            var mine = Subtree(c);
            // an exit's target is always the original's state, even when it's the copied state itself (a state that
            // loops to itself): a duplicated state's exits lead where its source's do, as FxStateOps.Duplicate's
            foreach (var t in mine.OfType<FXR1.FXTransition>())
                if (t.TargetState != null && copyIndex.TryGetValue(t.TargetState, out int target))
                    t.TargetState = (FXR1.FXState)original[target];
            // references out of the copied part point at the original's objects
            foreach (var o in mine)
                foreach (var f in Fields(o.GetType()))
                {
                    var v = f.GetValue(o);
                    if (v is IList l)
                    {
                        for (int i = 0; i < l.Count; i++)
                            if (l[i] != null && !mine.Contains(l[i]) && copyIndex.TryGetValue(l[i], out int j))
                                l[i] = original[j];
                    }
                    else if (v != null && !mine.Contains(v) && copyIndex.TryGetValue(v, out int j))
                        f.SetValue(o, original[j]);
                }
            result.Add(c);
        }
        return result;
    }

    // the list overload named explicitly: Clone(file, new[] { obj }) binds to this overload again with T = T[]
    public static T Clone<T>(FXR1 file, T obj) where T : class => Clone(file, (IReadOnlyList<T>)new[] { obj })[0];

    // ---------------------------------------------------------------- roots and states

    /// <summary>The effect's roots: node 133 itself, or the 133s a multi-root 134 holds.</summary>
    public static List<FXR1.FXNode.EffectCreateNode133> Roots(FXR1 file) => file.RootFXNode switch
    {
        FXR1.FXNode.EffectCreateNode133 r => [r],
        FXR1.FXNode.EffectCreateNode134 m => m.Nodes.OfType<FXR1.FXNode.EffectCreateNode133>().ToList(),
        _ => [],
    };

    public static FXR1.FXNode.EffectCreateNode133 RootOf(FXR1 file, FXR1.FXState state) => Roots(file).FirstOrDefault(r => r.States.Contains(state));

    public static IEnumerable<FXR1.FXTransition> Exits(FXR1.FXNode.EffectCreateNode133 root) => root.States.SelectMany(s => s.Transitions);

    /// <summary>Adds an empty state at the end of a root's states (no actions, no exits).</summary>
    public static int AddState(FXR1.FXNode.EffectCreateNode133 root)
    {
        root.States.Add(new FXR1.FXState { Actions = new(), Transitions = new() });
        return root.States.Count - 1;
    }

    /// <summary>Puts a copy of a state just after it; its exits lead where the original's do.</summary>
    public static int DuplicateState(FXR1 file, FXR1.FXNode.EffectCreateNode133 root, int index)
    {
        if (index < 0 || index >= root.States.Count)
            return -1;
        root.States.Insert(index + 1, Clone(file, root.States[index]));
        return index + 1;
    }

    public static int ExitsInto(FXR1.FXNode.EffectCreateNode133 root, FXR1.FXState state) => Exits(root).Count(t => ReferenceEquals(t.TargetState, state));

    /// <summary>Removes a state: exits into it end the effect. The last state of a root can't be removed.</summary>
    public static bool DeleteState(FXR1.FXNode.EffectCreateNode133 root, int index)
    {
        if (root.States.Count <= 1 || index < 0 || index >= root.States.Count)
            return false;
        var gone = root.States[index];
        root.States.RemoveAt(index);
        foreach (var t in Exits(root))
            if (ReferenceEquals(t.TargetState, gone))
                t.TargetState = null;
        return true;
    }

    /// <summary>Swaps two states (state 0 is where the root starts); exits follow them, being references.</summary>
    public static bool SwapStates(FXR1.FXNode.EffectCreateNode133 root, int a, int b)
    {
        if (a == b || a < 0 || b < 0 || a >= root.States.Count || b >= root.States.Count)
            return false;
        (root.States[a], root.States[b]) = (root.States[b], root.States[a]);
        return true;
    }

    // ---------------------------------------------------------------- lists

    /// <summary>
    /// Whether a container's node list is a list of calls (every entry an effect or action call node, at least one),
    /// whose entries may be added or removed. Other containers hold their action's fixed fields.
    /// </summary>
    public static bool IsCallList(FXR1.FXContainer c) =>
        c?.FXNodes?.Count > 0 && c.FXNodes.All(n => n is FXR1.FXNode.EffectCallNode or FXR1.FXNode.ActionCallNode);

    /// <summary>Every container of the graph, in walk order.</summary>
    public static IEnumerable<FXR1.FXContainer> Containers(FXR1 file) => Walk(file.RootFXNode).OfType<FXR1.FXContainer>();

    /// <summary>The state whose action list holds an action, and its root.</summary>
    public static (FXR1.FXNode.EffectCreateNode133 Root, FXR1.FXState State) OwnerOf(FXR1 file, FXR1.FXAction action)
    {
        foreach (var r in Roots(file))
            foreach (var s in r.States)
                if (s.Actions.Contains(action))
                    return (r, s);
        return (null, null);
    }

    /// <summary>The call list holding a call node, or null when its container has fixed fields.</summary>
    public static FXR1.FXContainer CallListOf(FXR1 file, FXR1.FXNode node) =>
        Containers(file).FirstOrDefault(c => c.FXNodes?.Contains(node) == true) is { } c && IsCallList(c) ? c : null;

    // ---------------------------------------------------------------- templates

    /// <summary>
    /// A structural fingerprint of an object's subtree: every type and simple field value in walk order, references
    /// out of it as their kind only. Equal fingerprints = the same action / exit in content.
    /// </summary>
    public static string Fingerprint(object obj)
    {
        var mine = Subtree(obj);
        var sb = new StringBuilder();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        void Dump(object o)
        {
            if (!seen.Add(o)) { sb.Append('^'); return; }
            var t = o.GetType();
            sb.Append(t.Name).Append('{');
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.MetadataToken))
            {
                var v = f.GetValue(o);
                if (o is FXR1.FXTransition && f.Name == nameof(FXR1.FXTransition.TargetState)) continue;
                // an object's XID names its file offset ("FXNode_0x9A0"): a label of the layout, not content
                if (f.Name == nameof(FXR1.XIDable.XID)) continue;
                if (v == null) { sb.Append("_,"); continue; }
                if (IsFx(f.FieldType) || IsFxList(f.FieldType))
                {
                    if (v is IList l) { sb.Append('['); foreach (var x in l) { if (x == null) sb.Append("_"); else if (mine.Contains(x)) Dump(x); else sb.Append("ext"); sb.Append(';'); } sb.Append(']'); }
                    else if (mine.Contains(v)) Dump(v);
                    else sb.Append("ext");
                }
                else if (v is IEnumerable e and not string)
                {
                    sb.Append('(');
                    foreach (var x in e) sb.Append(Simple(x)).Append(';');
                    sb.Append(')');
                }
                else sb.Append(Simple(v));
                sb.Append(',');
            }
            sb.Append('}');
        }
        static string Simple(object v) => v switch
        {
            float f => BitConverter.SingleToInt32Bits(f).ToString(),
            double d => BitConverter.DoubleToInt64Bits(d).ToString(),
            null => "_",
            _ when v.GetType().IsPrimitive || v is string || v.GetType().IsEnum => v.ToString(),
            _ => Fingerprint(v),
        };
        Dump(obj);
        return sb.ToString();
    }

    /// <summary>
    /// New actions and exits from vanilla: per action type, the commonest instance (by <see cref="Fingerprint"/>) over
    /// the given files; and the commonest exit condition. Copies come from their source file (<see cref="Clone"/>), so
    /// they can be put in any effect of the same game.
    /// </summary>
    public sealed class Templates
    {
        // each template's source file, written to bytes: a template is read out of them afresh each time it's used
        private readonly Dictionary<int, (byte[] File, int At)> byType = new();
        private readonly Dictionary<int, int> uses = new();
        private (byte[] File, int At) exit;

        /// <summary>
        /// Two passes over <paramref name="files"/> (called twice: it must give the same files in the same order):
        /// the first counts every action's and exit's fingerprint and keeps nothing else; the second keeps the first
        /// instance of each winner. So a game's whole bank (BB: 10,540 effects) isn't held at once.
        /// </summary>
        public Templates(Func<IEnumerable<FXR1>> files)
        {
            var counts = new Dictionary<int, Dictionary<string, int>>();
            var exitCounts = new Dictionary<string, int>();
            foreach (var file in files())
                foreach (var (a, t) in Items(file))
                {
                    if (a != null)
                    {
                        if (!counts.TryGetValue(a.ActionType, out var forms)) counts[a.ActionType] = forms = new();
                        var key = Fingerprint(a);
                        forms[key] = forms.GetValueOrDefault(key) + 1;
                        uses[a.ActionType] = uses.GetValueOrDefault(a.ActionType) + 1;
                    }
                    else
                    {
                        var key = Fingerprint(t);
                        exitCounts[key] = exitCounts.GetValueOrDefault(key) + 1;
                    }
                }
            var wanted = counts.ToDictionary(kv => kv.Key, kv => kv.Value.OrderByDescending(x => x.Value).First().Key);
            string wantedExit = exitCounts.Count > 0 ? exitCounts.OrderByDescending(x => x.Value).First().Key : null;
            ExitUses = wantedExit == null ? 0 : exitCounts[wantedExit];

            foreach (var file in files())
            {
                if (byType.Count == wanted.Count && (wantedExit == null || exit.File != null))
                    break;
                List<object> walk = null;
                foreach (var (a, t) in Items(file))
                {
                    if (a != null && !byType.ContainsKey(a.ActionType) && Fingerprint(a) == wanted[a.ActionType])
                    {
                        walk ??= Walk(file.RootFXNode);
                        byType[a.ActionType] = (file.Write(), walk.FindIndex(o => ReferenceEquals(o, a)));
                    }
                    else if (t != null && exit.File == null && wantedExit != null && Fingerprint(t) == wantedExit)
                    {
                        walk ??= Walk(file.RootFXNode);
                        exit = (file.Write(), walk.FindIndex(o => ReferenceEquals(o, t)));
                    }
                }
            }
        }

        private static IEnumerable<(FXR1.FXAction, FXR1.FXTransition)> Items(FXR1 file)
        {
            foreach (var r in Roots(file))
                foreach (var s in r.States)
                {
                    foreach (var a in s.Actions) yield return (a, null);
                    foreach (var t in s.Transitions) yield return (null, t);
                }
        }

        public IReadOnlyDictionary<int, int> Uses => uses;
        public int ExitUses { get; }

        /// <summary>A new copy of the template action of a type (its own graph), or null.</summary>
        public FXR1.FXAction New(int type) => byType.TryGetValue(type, out var t) ? (FXR1.FXAction)Walk(FXR1.Read(t.File).RootFXNode)[t.At] : null;

        /// <summary>A new exit with the commonest condition, ending the effect.</summary>
        public FXR1.FXTransition NewExit()
        {
            if (exit.File == null) return null;
            var t = (FXR1.FXTransition)Walk(FXR1.Read(exit.File).RootFXNode)[exit.At];
            t.TargetState = null;
            return t;
        }

    }

    // ---------------------------------------------------------------- snapshots

    public static byte[] Snapshot(FXR1 file) => file.Write();
}
