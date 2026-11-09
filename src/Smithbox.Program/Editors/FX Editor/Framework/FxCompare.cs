using SoulsFormats;
using System.Globalization;

namespace StudioCore.Editors.FxEditor;

/// <summary>One difference between two effects: a value only in A, only in B, or different in each.</summary>
public record FxDifference(string Key, string A, string B);

/// <summary>
/// Compares two effects value by value (FX Effect List Actions Plan, phase D). Each effect is flattened into keyed
/// lines (state conditions, then every node's actions and properties by tree position) and the keys are matched.
/// Nodes are matched by position, as the game references them, so a node inserted early shows as later nodes changing.
/// Only SoulsFormats is used, so the check tool compiles this file on its own.
/// </summary>
public static class FxCompare
{
    public static List<FxDifference> Diff(FXR a, FXR b)
    {
        var la = Flatten(a);
        var lb = Flatten(b);
        var result = new List<FxDifference>();

        // Keys in A's order, then the ones only B has, in B's order
        foreach (var (key, va) in la)
        {
            if (!lb.TryGetValue(key, out var vb))
                result.Add(new FxDifference(key, va, null));
            else if (va != vb)
                result.Add(new FxDifference(key, va, vb));
        }
        foreach (var (key, vb) in lb)
            if (!la.ContainsKey(key))
                result.Add(new FxDifference(key, null, vb));
        return result;
    }

    /// <summary>The effect as keyed values, in tree order.</summary>
    public static OrderedDictionary<string, string> Flatten(FXR fxr)
    {
        var lines = new OrderedDictionary<string, string>(StringComparer.Ordinal);
        lines["Version"] = fxr.Version.ToString();
        lines["States"] = fxr.States.Count.ToString(CultureInfo.InvariantCulture);
        for (int s = 0; s < fxr.States.Count; s++)
        {
            var state = fxr.States[s];
            lines[$"State {s}/Conditions"] = state.Conditions.Count.ToString(CultureInfo.InvariantCulture);
            for (int c = 0; c < state.Conditions.Count; c++)
            {
                var cond = state.Conditions[c];
                lines[$"State {s}/Condition {c}"] =
                    $"{Operand(cond.Left)} op{cond.Operator} {Operand(cond.Right)} -> {cond.TargetState} (unk {cond.Unk1})";
            }
        }
        Node(lines, "Root", fxr.Root);
        return lines;
    }

    private static string Operand(FxrOperand o) => o.HasValue ? $"[{o.Type}:{o.RawValue}]" : $"[{o.Type}]";

    private static void Node(OrderedDictionary<string, string> lines, string path, FxrNode node)
    {
        lines[$"{path}/Type"] = node.Type.ToString(CultureInfo.InvariantCulture);
        lines[$"{path}/Children"] = node.Children.Count.ToString(CultureInfo.InvariantCulture);
        lines[$"{path}/Configs"] = node.Configs.Count.ToString(CultureInfo.InvariantCulture);
        Actions(lines, path, node.Actions);
        for (int c = 0; c < node.Configs.Count; c++)
        {
            var config = node.Configs[c];
            var cpath = $"{path}/Config {c}";
            lines[$"{cpath}/Type"] = config.Type.ToString(CultureInfo.InvariantCulture);
            Actions(lines, cpath, config.Actions);
        }
        for (int i = 0; i < node.Children.Count; i++)
            Node(lines, $"{path}/{i}", node.Children[i]);
    }

    private static void Actions(OrderedDictionary<string, string> lines, string path, List<FxrAction> actions)
    {
        lines[$"{path}/Actions"] = string.Join(" ", actions.Select(a => a.Type));
        // Each action by its type and how many of that type came before it, so one inserted action doesn't shift
        // every later one
        var seen = new Dictionary<int, int>();
        foreach (var a in actions)
        {
            int n = seen.GetValueOrDefault(a.Type);
            seen[a.Type] = n + 1;
            var apath = n == 0 ? $"{path}/Action {a.Type}" : $"{path}/Action {a.Type}#{n + 1}";
            lines[$"{apath}/Header"] = $"{a.Unk02} {a.Unk03} {a.Unk04}";
            Words(lines, $"{apath}/Fields1", a.Fields1);
            Words(lines, $"{apath}/Fields2", a.Fields2);
            for (int p = 0; p < a.Properties1.Count; p++)
                Property(lines, $"{apath}/Property1 {p}", a.Properties1[p]);
            for (int p = 0; p < a.Properties2.Count; p++)
                Property(lines, $"{apath}/Property2 {p}", a.Properties2[p]);
            for (int s = 0; s < a.Section10s.Count; s++)
                lines[$"{apath}/Section10 {s}"] = string.Join(" ", a.Section10s[s]);
        }
    }

    private static void Property(OrderedDictionary<string, string> lines, string path, FxrProperty p)
    {
        lines[$"{path}/Kind"] = $"{p.ValueType} {p.Function} loop {p.Loop} {p.TypeEnumB}";
        Words(lines, $"{path}/Fields", p.Fields);
        for (int m = 0; m < p.Modifiers.Count; m++)
        {
            var mod = p.Modifiers[m];
            var mpath = $"{path}/Modifier {m}";
            lines[$"{mpath}/Kind"] = $"{mod.TypeEnumA} {mod.TypeEnumB}";
            Words(lines, $"{mpath}/Fields", mod.Fields);
            for (int q = 0; q < mod.Properties.Count; q++)
                Property(lines, $"{mpath}/Property {q}", mod.Properties[q]);
        }
    }

    private static void Words<T>(OrderedDictionary<string, string> lines, string path, IList<T> words)
    {
        lines[$"{path}/Count"] = words.Count.ToString(CultureInfo.InvariantCulture);
        for (int i = 0; i < words.Count; i++)
            lines[$"{path} {i}"] = Word(words[i]);
    }

    /// <summary>A word as its integer and its float reading: FXR fields hold either, and only the schema says which.</summary>
    public static string Word<T>(T word)
    {
        uint bits = word switch
        {
            uint u => u,
            int i => unchecked((uint)i),
            float f => BitConverter.SingleToUInt32Bits(f),
            _ => 0,
        };
        if (word is not (uint or int or float))
            return Convert.ToString(word, CultureInfo.InvariantCulture);
        float asFloat = BitConverter.UInt32BitsToSingle(bits);
        bool plausibleFloat = float.IsFinite(asFloat) && (bits == 0 || Math.Abs(asFloat) is > 1e-6f and < 1e9f);
        return plausibleFloat && bits > 0xFFFFF
            ? $"{unchecked((int)bits)} ({asFloat.ToString("G6", CultureInfo.InvariantCulture)})"
            : unchecked((int)bits).ToString(CultureInfo.InvariantCulture);
    }
}
