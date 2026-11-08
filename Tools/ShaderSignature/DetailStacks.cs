using StudioCore.Renderer;

/// <summary>
/// The detail stacks of C[DetailBlend] and its relatives (MaterialDetailRecipe.Stack, {GAME}_DetailStacks.json).
///
/// Their permutations above 0 lerp up to four detail albedos before the result multiplies the base albedo by 4.55:
///   d = lerp(lerp(lerp(c1 T2, c0 T1, w1), c5 T10, w2), c2 T4, w3),  albedo = sat(base * d * 4.55)
/// and lerp the detail normals by the same weights before (d * 2 - 1) * strength is added to the base normal. Each
/// weight reads one channel of the base normal map, which is otherwise only normal and gloss:
///   sat((T0.b * 22)^260)                          a step at a blue of 1/22
///   sat((0.5 - f19 (T0.a - 0.5)) * 2.05 - 1.05)   a ramp on the alpha, f20 with the other sign for the next layer
/// Both are sat(pow(A x + B, P)) with A and B polynomials of the material's params, which is what the recipe stores.
/// Which slots each permutation wires in differs, so the stack is read from every permutation's own shader.
/// </summary>
static partial class Recipes
{
    public class StackResult
    {
        public MaterialDetailRecipe Recipe = new();
        public List<string> Problems = new();
        public bool Ok => Problems.Count == 0;
    }

    /// <summary>A stack weight read: the weight map texture (as a leaf without its channel), the channel, and A, B, P</summary>
    record StackWeight(string Texture, string Channel, string Scale, string Bias, double Power);

    public static StackResult BuildStack(Dxil d, HashSet<int> albedoSlots, HashSet<int> normalSlots)
    {
        var result = new StackResult();
        var root = d.StoreId("RT2.x");
        if (root == null) { result.Problems.Add("RT2.x isn't written"); return result; }

        bool HasAlbedo(Dxil.Arg a) => !a.IsConst && d.LeafNames(a.Id).Any(l => Slot(l) is int s && albedoSlots.Contains(s) && Channel(l) == "r");

        // A weight of params and the blue or alpha of one normal map, and nothing else
        bool IsStackWeight(Dxil.Arg w)
        {
            if (w.IsConst) return false;
            var textures = d.LeafNames(w.Id).Where(l => !l.StartsWith("cb:")).ToList();
            return textures.Count > 0 && textures.All(l => l.StartsWith("tex:") && Slot(l) is int s && normalSlots.Contains(s) && Channel(l) is "b" or "a");
        }

        bool IsStackLerp(Lerp l) => l != null && HasAlbedo(l.Top) && HasAlbedo(l.Bottom) && IsStackWeight(l.Weight);

        // The topmost such lerp, and what multiplies it
        Lerp top = null;
        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(root.Value);
        while (stack.Count > 0 && top == null)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            var l = AsLerp(d, id);
            if (IsStackLerp(l)) { top = l; break; }
            foreach (var a in d.NodeOf(id).Args.Where(a => !a.IsConst).Reverse()) stack.Push(a.Id);
        }
        if (top == null) { result.Problems.Add("no lerp of details weighed by a normal map"); return result; }

        // The stack has to be the detail that multiplies the albedo, d * 4.55, or it's another kind of layering
        var multiplied = false;
        seen.Clear();
        stack.Push(root.Value);
        while (stack.Count > 0 && !multiplied)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            var n = d.NodeOf(id);
            if (n.Op == Dxil.Op.Mul && n.Args.Any(a => !a.IsConst && a.Id == top.Id) && n.Args.Any(a => a.IsConst && Math.Abs(a.Value - 4.55) < 1e-3))
                multiplied = true;
            foreach (var a in n.Args.Where(a => !a.IsConst)) stack.Push(a.Id);
        }
        if (!multiplied)
        {
            result.Problems.Add("the stack isn't multiplied by 4.55");
            if (Environment.GetEnvironmentVariable("STACK_DEBUG") == "2")
            {
                // What uses the top lerp, one level up, short
                foreach (var id in seen)
                    if (d.NodeOf(id).Args.Any(a => !a.IsConst && a.Id == top.Id))
                        Console.WriteLine($"   parent %{id} {d.NodeOf(id).Op}: {d.ExpressionOf(id, 7)}");
            }
            return result;
        }

        var chain = new List<Lerp>();
        for (var l = top; l != null;)
        {
            chain.Insert(0, l);
            var next = AsLerp(d, l.Bottom.Id);
            l = IsStackLerp(next) ? next : null;
        }
        if (chain.Count > MaterialResolvedDetail.MaxStackLayers) { result.Problems.Add($"{chain.Count} layers, more than the viewport draws"); return result; }

        (int? slot, string color) Term(Dxil.Arg a)
        {
            var leaves = d.LeafNames(a.Id).ToList();
            var albedos = leaves.Where(x => Slot(x) is int s && albedoSlots.Contains(s)).Select(x => Slot(x)!.Value).Distinct().ToList();
            // Only the red of the one albedo: the base albedo's luma (green, blue) means a layer that isn't a detail
            if (albedos.Count != 1 || leaves.Any(x => x.StartsWith("tex:") && Channel(x) != "r") || leaves.Any(x => x.StartsWith("in:"))) return (null, null);
            var color = leaves.Select(x => ColorParam.Match(x)).FirstOrDefault(m => m.Success)?.Groups[1].Value;
            return (albedos[0], color);
        }

        var recipe = result.Recipe;
        var (bottomSlot, bottomColor) = Term(chain[0].Bottom);
        if (bottomSlot == null) { result.Problems.Add("the bottom detail isn't one albedo"); return result; }
        recipe.Albedo = bottomSlot;
        recipe.Color = bottomColor;

        // Normals: the lerps in RT0.x with the same weights
        var normalLerps = new Dictionary<int, Lerp>();
        if (d.StoreId("RT0.x") is int normalRoot)
        {
            var nseen = new HashSet<int>();
            var nstack = new Stack<int>();
            nstack.Push(normalRoot);
            while (nstack.Count > 0)
            {
                var id = nstack.Pop();
                if (!nseen.Add(id)) continue;
                if (AsLerp(d, id) is { } l && !l.Weight.IsConst && chain.Exists(c => c.Weight.Id == l.Weight.Id)) normalLerps.TryAdd(l.Weight.Id, l);
                foreach (var a in d.NodeOf(id).Args.Where(a => !a.IsConst)) nstack.Push(a.Id);
            }
        }

        int? NormalOf(Dxil.Arg? a)
        {
            if (a == null || a.Value.IsConst) return null;
            var slots = d.LeafNames(a.Value.Id).Where(x => Slot(x) is int s && normalSlots.Contains(s) && Channel(x) is "r" or "g").Select(x => Slot(x)!.Value).Distinct().ToList();
            return slots.Count == 1 ? slots[0] : null;
        }
        recipe.Normal = normalLerps.TryGetValue(chain[0].Weight.Id, out var firstNormal) ? NormalOf(firstNormal.Bottom) : null;

        string weightMap = null;
        recipe.Stack = new List<MaterialDetailStackLayer>();
        foreach (var l in chain)
        {
            var (slot, color) = Term(l.Top);
            if (slot == null) { result.Problems.Add("a layer isn't one albedo"); return result; }
            var w = ReadStackWeight(d, l.Weight);
            if (w == null) { result.Problems.Add($"layer {slot}: weight not understood"); return result; }
            if (weightMap != null && weightMap != w.Texture) { result.Problems.Add("layers read different weight maps"); return result; }
            weightMap = w.Texture;

            recipe.Stack.Add(new MaterialDetailStackLayer
            {
                Albedo = slot,
                Color = color,
                Normal = normalLerps.TryGetValue(l.Weight.Id, out var nl) ? NormalOf(nl.Top) : null,
                Channel = w.Channel,
                Scale = w.Scale,
                Bias = w.Bias,
                Power = (float)w.Power
            });
        }
        recipe.StackWeightMap = Slot(weightMap + ".r");
        return result;
    }

    /// <summary>sat(pow(A x + B, P)), with P = 1 where there is no pow: exp2(log2(X) * P), in either operand order</summary>
    static StackWeight ReadStackWeight(Dxil d, Dxil.Arg w)
    {
        if (Unsaturated(d, w) is not { } inner || inner.IsConst) return null;

        var power = 1.0;
        var linear = inner;
        if (d.NodeOf(inner.Id) is { Op: Dxil.Op.Exp2 } exp && !exp.Args[0].IsConst && d.NodeOf(exp.Args[0].Id) is { Op: Dxil.Op.Mul } mul)
        {
            var c = mul.Args.FirstOrDefault(a => a.IsConst);
            var log = mul.Args.FirstOrDefault(a => !a.IsConst);
            if (!c.IsConst || log.IsConst || d.NodeOf(log.Id) is not { Op: Dxil.Op.Log2 } logNode || logNode.Args[0].IsConst) return null;
            power = c.Value;
            linear = logNode.Args[0];
        }

        var poly = Expand(d, linear, new Dictionary<int, Poly>());
        if (poly.Keys.Any(k => k.Contains("op"))) return null;
        var sources = poly.Keys.SelectMany(k => k.Split('*')).Where(x => x.StartsWith("tex:") || x.StartsWith("in:")).Distinct().ToList();
        if (sources.Count != 1 || !sources[0].StartsWith("tex:")) return null;

        var a = Without(poly, sources[0], out var b);
        if (a == null || a.Count == 0) return null;
        var scale = Text(a);
        var bias = Text(b);
        if (scale == null || bias == null) return null;
        return new StackWeight(sources[0][..^2], Channel(sources[0]), scale, bias, power);
    }
}
