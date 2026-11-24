namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// What an older-game effect uses, through the model (FX.md, FFX §21.9): the effects it calls and its
/// textures and models. Used by the FX Editor's Used By / Uses Resource for DES, DS2, DS2S, DS1, DS1R and BB (FXR3 has
/// its own, FxUsage.Targets / ResourceNames). SoulsFormats + BCL only, so the check tools can link it.
/// </summary>
public static class FxModelUsage
{
    /// <summary>Every action of a model effect: nodes', configs', states', and their nested calls, each once.</summary>
    public static IEnumerable<IFxAction> Actions(IFxEffect effect)
    {
        var effects = effect is IFxVariants { Variants.Count: > 0 } v ? v.Variants : new[] { effect };
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<IFxAction>();
        foreach (var e in effects)
        {
            foreach (var s in e.States)
                foreach (var a in s.Actions) stack.Push(a);
            var nodes = new Stack<IFxNode>();
            if (e.Root != null) nodes.Push(e.Root);
            while (nodes.Count > 0)
            {
                var n = nodes.Pop();
                if (!seen.Add(n)) continue;
                foreach (var a in n.Actions) stack.Push(a);
                if (n is IFxConfigured c)
                    foreach (var cfg in c.Configs) foreach (var a in cfg.Actions) stack.Push(a);
                if (n is IFxStatesNode sn)
                    foreach (var s in sn.States) foreach (var a in s.Actions) stack.Push(a);
                foreach (var ch in n.Children) nodes.Push(ch);
            }
        }
        while (stack.Count > 0)
        {
            var a = stack.Pop();
            if (!seen.Add(a)) continue;
            yield return a;
            foreach (var call in a.Calls) stack.Push(call);
        }
    }

    /// <summary>The effect ids a model effect calls (DLSE effect calls, FXR1 effect call nodes), without repeats.</summary>
    public static List<int> Targets(IFxEffect effect) =>
        Actions(effect).OfType<IFxProxy>().Select(p => p.TargetEffect).Where(id => id > 0).Distinct().ToList();

    /// <summary>A model effect's textures and models as names (s01000), from every value the model marks as one.</summary>
    public static HashSet<string> Resources(IFxEffect effect)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var a in Actions(effect))
                foreach (var v in a.Values)
                    if (v.Resource is "texture" or "model" && v.Get() is { Length: > 0 } d && d[0] > 0 && d[0] < 99990)
                        set.Add($"s{(int)d[0]:D5}");
        }
        catch
        {
            // a malformed action can't break the scan of the rest
        }
        return set;
    }

}
