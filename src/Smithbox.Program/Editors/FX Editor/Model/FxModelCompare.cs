using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// Compares two effects of any format value by value, through the model (FX.md, FFX §22, F8): each effect is
/// flattened into keyed lines (states with their exits and actions, then the node tree), and the keys are matched, as
/// FxCompare does for FXR3. Actions are keyed by type and how many of that type came before, so one inserted action
/// doesn't shift every later one. SoulsFormats + BCL only, so the check tools can link it.
/// </summary>
public static class FxModelCompare
{
    /// <summary>The differences: a key with A's and B's text, null where that side lacks the key.</summary>
    public static List<(string Key, string A, string B)> Diff(IFxEffect a, IFxEffect b)
    {
        var la = Flatten(a);
        var lb = Flatten(b);
        var result = new List<(string, string, string)>();
        foreach (var (key, va) in la)
        {
            if (!lb.TryGetValue(key, out var vb))
                result.Add((key, va, null));
            else if (va != vb)
                result.Add((key, va, vb));
        }
        foreach (var (key, vb) in lb)
            if (!la.ContainsKey(key))
                result.Add((key, null, vb));
        return result;
    }

    /// <summary>The effect as keyed values, in model order. The id is left out, so an effect compares equal to its copy under another id.</summary>
    public static OrderedDictionary<string, string> Flatten(IFxEffect effect)
    {
        var lines = new OrderedDictionary<string, string>(StringComparer.Ordinal);
        if (effect is IFxVariants { Variants.Count: > 0 } v)
        {
            lines["Variants"] = Count(v.Variants.Count);
            for (int i = 0; i < v.Variants.Count; i++)
                One(lines, $"Variant {i}/", v.Variants[i]);
        }
        else
        {
            One(lines, "", effect);
        }
        return lines;
    }

    private static string Count(int n) => n.ToString(CultureInfo.InvariantCulture);

    private static void One(OrderedDictionary<string, string> lines, string prefix, IFxEffect effect)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        States(lines, $"{prefix}", effect.States, seen);
        if (effect.Root != null)
            Node(lines, $"{prefix}Root", effect.Root, seen);
    }

    private static void States(OrderedDictionary<string, string> lines, string prefix, IReadOnlyList<IFxState> states, HashSet<object> seen)
    {
        lines[$"{prefix}States"] = Count(states.Count);
        for (int s = 0; s < states.Count; s++)
        {
            var state = states[s];
            var spath = $"{prefix}State {s}";
            lines[$"{spath}/Exits"] = Count(state.Transitions.Count);
            for (int t = 0; t < state.Transitions.Count; t++)
            {
                var tr = state.Transitions[t];
                lines[$"{spath}/Exit {t}"] = $"{Expression(tr.Condition)} -> {(tr.TargetState < 0 ? "end" : Count(tr.TargetState))}";
            }
            Actions(lines, spath, state.Actions, seen);
        }
    }

    private static void Node(OrderedDictionary<string, string> lines, string path, IFxNode node, HashSet<object> seen)
    {
        if (!seen.Add(node))
            return;
        lines[$"{path}/Type"] = $"{node.Kind} {Count(node.TypeId)}";
        if (node is IFxProxy p)
            lines[$"{path}/Calls effect"] = Count(p.TargetEffect);
        Actions(lines, path, node.Actions, seen);
        if (node is IFxConfigured c)
            for (int i = 0; i < c.Configs.Count; i++)
                Actions(lines, $"{path}/Config {i} ({c.Configs[i].TypeId})", c.Configs[i].Actions, seen);
        if (node is IFxStatesNode sn)
            States(lines, $"{path}/", sn.States, seen);
        if (node is IFxResourceLists rl)
            foreach (var (kind, ids) in rl.ResourceLists)
                lines[$"{path}/Resources {kind}"] = string.Join(" ", ids);
        lines[$"{path}/Children"] = Count(node.Children.Count);
        for (int i = 0; i < node.Children.Count; i++)
            Node(lines, $"{path}/{i}", node.Children[i], seen);
    }

    private static void Actions(OrderedDictionary<string, string> lines, string path, IReadOnlyList<IFxAction> actions, HashSet<object> seen)
    {
        lines[$"{path}/Actions"] = string.Join(" ", actions.Select(a => a.TypeId));
        var counts = new Dictionary<int, int>();
        foreach (var a in actions)
        {
            int n = counts.GetValueOrDefault(a.TypeId);
            counts[a.TypeId] = n + 1;
            var label = a.Name != null ? $"{a.TypeId} {a.Name}" : Count(a.TypeId);
            Action(lines, n == 0 ? $"{path}/Action {label}" : $"{path}/Action {label}#{n + 1}", a, seen);
        }
    }

    private static void Action(OrderedDictionary<string, string> lines, string path, IFxAction action, HashSet<object> seen)
    {
        if (!seen.Add(action))
        {
            lines[$"{path}"] = "(shared)";
            return;
        }
        if (action is IFxProxy p)
            lines[$"{path}/Calls effect"] = Count(p.TargetEffect);
        for (int i = 0; i < action.Values.Count; i++)
        {
            var v = action.Values[i];
            lines[$"{path}/{i} {v.Name}"] = Value(v);
        }
        // Calls in param order, keyed like actions so one inserted call doesn't shift the rest
        var counts = new Dictionary<int, int>();
        foreach (var call in action.Calls)
        {
            int n = counts.GetValueOrDefault(call.TypeId);
            counts[call.TypeId] = n + 1;
            var label = call.Name != null ? $"{call.TypeId} {call.Name}" : Count(call.TypeId);
            Action(lines, n == 0 ? $"{path}/Call {label}" : $"{path}/Call {label}#{n + 1}", call, seen);
        }
    }

    private static string Value(IFxValue v)
    {
        try
        {
            string type = v is IFxTypedValue t ? $"[{(t.TypeName ?? Count(t.FileType))}] " : "";
            switch (v.Kind)
            {
                case FxValueKind.Curve when v is IFxCurveValue c:
                    return type + Curve(c.GetCurve());
                case FxValueKind.IntList when v is IFxListValue l:
                    return type + (l.GetList() is { } list ? string.Join(" ", list) : "(none)");
                default:
                    var d = v.Get();
                    if (d != null)
                        return type + string.Join(" ", d.Select(x => x.ToString("G7", CultureInfo.InvariantCulture)));
                    if (v is IFxArgumentValue arg)
                        return type + $"argument {arg.ArgumentIndex} ({string.Join(" ", arg.Extra)})";
                    if (v is IFxCallValue call)
                        return type + $"call {call.Call?.TypeId}";
                    return type + (v.IsStored ? "(stored)" : "(not stored)");
            }
        }
        catch (Exception e)
        {
            return $"(unreadable: {e.Message})";
        }
    }

    private static string Curve(FxrCurve c)
    {
        if (c == null)
            return "(none)";
        static string F(float f) => f.ToString("G7", CultureInfo.InvariantCulture);
        static string Arr(float[] a) => a == null ? "" : string.Join(",", a.Select(F));
        var head = $"{c.Function}{(c.Loop ? " loop" : "")}";
        if (c.Constant != null && c.Keyframes.Count == 0)
            return $"{head} {Arr(c.Constant)}";
        var keys = string.Join(" ", c.Keyframes.Select(k =>
            $"{F(k.Position)}:{Arr(k.Value)}{(k.In != null ? $" in {Arr(k.In)}" : "")}{(k.Out != null ? $" out {Arr(k.Out)}" : "")}"));
        return $"{head} [{c.Keyframes.Count}] {keys}";
    }

    private static string Expression(IFxExpression e)
    {
        if (e == null)
            return "always";
        return e.Kind switch
        {
            FxExpressionKind.Compare => $"({Expression(e.Operands.ElementAtOrDefault(0))} {e.Operator} {Expression(e.Operands.ElementAtOrDefault(1))})",
            FxExpressionKind.Constant => e.Value?.ToString("G7", CultureInfo.InvariantCulture) ?? "?",
            _ when e.Operands.Count > 0 => $"{e.Kind}({string.Join(", ", e.Operands.Select(Expression))})",
            _ => e.Value is { } val ? $"{e.Kind}[{e.Index}] {val.ToString("G7", CultureInfo.InvariantCulture)}" : $"{e.Kind}[{e.Index}]",
        };
    }
}
