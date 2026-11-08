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
///
/// Other permutations lerp some layers over the finished albedo instead, so they replace its color rather than detail
/// it: sat(lerp(lerp(base * c0 T1 * 4.55, c5 T10, w2), c2 T4, w3)) (permutation 8), or with the layer at the bottom,
/// lerp(c1 T2, base * d * 4.55, w1), which is the layer by 1 - w1 (permutation 2). Their normals still lerp with the
/// details'. Permutations 15 and up draw back faces with an albedo and normal of their own: front ? that : c3 T8.
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

        List<string> AlbedoLeaves(Dxil.Arg a) => a.IsConst ? [] : d.LeafNames(a.Id).Where(l => Slot(l) is int s && albedoSlots.Contains(s)).ToList();
        bool HasAlbedo(Dxil.Arg a) => AlbedoLeaves(a).Any(l => Channel(l) == "r");
        int AlbedoCount(Dxil.Arg a) => AlbedoLeaves(a).Select(l => Slot(l)!.Value).Distinct().Count();

        // A weight of params and the blue or alpha of one normal map, and nothing else
        bool IsStackWeight(Dxil.Arg w)
        {
            if (w.IsConst) return false;
            var textures = d.LeafNames(w.Id).Where(l => !l.StartsWith("cb:")).ToList();
            return textures.Count > 0 && textures.All(l => l.StartsWith("tex:") && Slot(l) is int s && normalSlots.Contains(s) && Channel(l) is "b" or "a");
        }

        bool IsStackLerp(Lerp l) => l != null && HasAlbedo(l.Top) && HasAlbedo(l.Bottom) && IsStackWeight(l.Weight);

        // One albedo's red and params: a layer. The base albedo's luma (green, blue) means it isn't one
        (int? slot, string color) Term(Dxil.Arg a)
        {
            if (a.IsConst) return (null, null);
            var leaves = d.LeafNames(a.Id).ToList();
            var albedos = leaves.Where(x => Slot(x) is int s && albedoSlots.Contains(s)).Select(x => Slot(x)!.Value).Distinct().ToList();
            if (albedos.Count != 1 || leaves.Any(x => x.StartsWith("tex:") && Channel(x) != "r") || leaves.Any(x => x.StartsWith("in:"))) return (null, null);
            var color = leaves.Select(x => ColorParam.Match(x)).FirstOrDefault(m => m.Success)?.Groups[1].Value;
            return (albedos[0], color);
        }

        int? NormalOf(Dxil.Arg? a)
        {
            if (a == null || a.Value.IsConst) return null;
            var slots = d.LeafNames(a.Value.Id).Where(x => Slot(x) is int s && normalSlots.Contains(s) && Channel(x) is "r" or "g").Select(x => Slot(x)!.Value).Distinct().ToList();
            return slots.Count == 1 ? slots[0] : null;
        }

        bool IsFrontFaceSelect(Dxil.Node n) =>
            n is { Op: Dxil.Op.Other, Args.Length: 3 } && !n.Args[0].IsConst && d.LeafNames(n.Args[0].Id).Any(l => l.Contains("SV_IsFrontFace"));

        // 1. The finished albedo: the clamp under RT2.x of a value with two albedos or more (base and detail)
        Dxil.Arg? final = null;
        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(root.Value);
        while (stack.Count > 0 && final == null)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            if (Unsaturated(d, new Dxil.Arg(id, 0)) is { } inner && AlbedoCount(inner) >= 2) { final = inner; break; }
            foreach (var a in d.NodeOf(id).Args.Where(a => !a.IsConst).Reverse()) stack.Push(a.Id);
        }
        if (final == null) { result.Problems.Add("no clamped albedo"); return result; }
        var f = final.Value;
        while (Unsaturated(d, f) is { } again) f = again;

        // 2. A back face that picks another albedo: front ? f : back (permutations 15 and up)
        var recipe = result.Recipe;
        var backFace = false;
        if (!f.IsConst && d.NodeOf(f.Id) is var select && IsFrontFaceSelect(select))
        {
            var (backSlot, backColor) = Term(select.Args[2]);
            if (backSlot == null) { result.Problems.Add("the back face isn't one albedo"); return result; }
            recipe.BackAlbedo = backSlot;
            recipe.BackColor = backColor;
            backFace = true;
            f = select.Args[1];
        }

        // 3. Whole-albedo layers over the finished albedo: lerps of one albedo term and the rest, gathered top first.
        // A layer on the bottom of its lerp takes one minus the weight
        var replaced = new List<(Lerp lerp, bool invert)>();
        while (!f.IsConst && AsLerp(d, f.Id) is { } l && IsStackWeight(l.Weight))
        {
            if (Term(l.Top).slot != null && AlbedoCount(l.Bottom) >= 2) { replaced.Insert(0, (l, false)); f = l.Bottom; }
            else if (Term(l.Bottom).slot != null && AlbedoCount(l.Top) >= 2) { replaced.Insert(0, (l, true)); f = l.Top; }
            else break;
        }

        // 4. What is left is base * detail * 4.55: the detail is what the 4.55 multiplies, a stack of lerps or one albedo
        Dxil.Arg? detail = null;
        if (!f.IsConst)
        {
            seen.Clear();
            stack.Push(f.Id);
            while (stack.Count > 0 && detail == null)
            {
                var id = stack.Pop();
                if (!seen.Add(id)) continue;
                var n = d.NodeOf(id);
                if (n.Op == Dxil.Op.Mul && n.Args.Any(a => a.IsConst && Math.Abs(a.Value - 4.55) < 1e-3)
                    && n.Args.FirstOrDefault(a => !a.IsConst) is { Id: >= 0 } other && HasAlbedo(other))
                    detail = other;
                foreach (var a in n.Args.Where(a => !a.IsConst)) stack.Push(a.Id);
            }
        }
        if (detail == null) { result.Problems.Add("no detail multiplied by 4.55"); return result; }

        var chain = new List<Lerp>();
        for (var l = AsLerp(d, detail.Value.Id); IsStackLerp(l);)
        {
            chain.Insert(0, l);
            var next = AsLerp(d, l.Bottom.Id);
            l = IsStackLerp(next) ? next : null;
        }
        var layerCount = chain.Count + replaced.Count;
        if (layerCount > MaterialResolvedDetail.MaxStackLayers) { result.Problems.Add($"{layerCount} layers, more than the viewport draws"); return result; }
        if (layerCount == 0 && !backFace) { result.Problems.Add("no lerp of details weighed by a normal map"); return result; }

        if (chain.Count > 0)
        {
            (recipe.Albedo, recipe.Color) = Term(chain[0].Bottom);
            if (recipe.Albedo == null) { result.Problems.Add("the bottom detail isn't one albedo"); return result; }
        }
        else
        {
            // One detail: its albedo is what the 4.55 multiplies, its tint the one color param left in the rest
            var slots = AlbedoLeaves(detail.Value).Select(l => Slot(l)!.Value).Distinct().ToList();
            var colors = d.LeafNames(f.Id).Select(x => ColorParam.Match(x)).Where(m => m.Success).Select(m => m.Groups[1].Value).Distinct().ToList();
            if (slots.Count != 1 || colors.Count > 1) { result.Problems.Add("the single detail isn't one albedo and color"); return result; }
            recipe.Albedo = slots[0];
            recipe.Color = colors.FirstOrDefault();
        }

        // Normals: the lerps in RT0.x with the same weights, and the back face's normal
        var weights = chain.Select(c => c.Weight.Id).Concat(replaced.Select(r => r.lerp.Weight.Id)).ToHashSet();
        var normalLerps = new Dictionary<int, Lerp>();
        Dxil.Arg? frontNormal = null;
        if (d.StoreId("RT0.x") is int normalRoot)
        {
            var nseen = new HashSet<int>();
            var nstack = new Stack<int>();
            nstack.Push(normalRoot);
            while (nstack.Count > 0)
            {
                var id = nstack.Pop();
                if (!nseen.Add(id)) continue;
                var n = d.NodeOf(id);
                if (AsLerp(d, id) is { } l && !l.Weight.IsConst && weights.Contains(l.Weight.Id)) normalLerps.TryAdd(l.Weight.Id, l);
                if (backFace && recipe.BackNormal == null && IsFrontFaceSelect(n))
                {
                    recipe.BackNormal = NormalOf(n.Args[2]);
                    frontNormal = n.Args[1];
                }
                foreach (var a in n.Args.Where(a => !a.IsConst)) nstack.Push(a.Id);
            }
        }

        // The bottom detail's normal: the bottom of the first stack lerp's normal; failing a stack, the far side of the
        // first whole-albedo layer's; failing that, the one detail normal on the front face
        if (chain.Count > 0)
        {
            recipe.Normal = normalLerps.TryGetValue(chain[0].Weight.Id, out var firstNormal) ? NormalOf(firstNormal.Bottom) : null;
        }
        else if (replaced.Count > 0)
        {
            recipe.Normal = normalLerps.TryGetValue(replaced[0].lerp.Weight.Id, out var under) ? NormalOf(replaced[0].invert ? under.Top : under.Bottom) : null;
        }
        else if (frontNormal != null)
        {
            recipe.Normal = NormalOf(frontNormal);
        }

        string weightMap = null;
        recipe.Stack = new List<MaterialDetailStackLayer>();
        var layers = chain.Select(c => (lerp: c, invert: false, replace: false)).Concat(replaced.Select(r => (r.lerp, r.invert, replace: true)));
        foreach (var (l, invert, replace) in layers)
        {
            var (slot, color) = Term(invert ? l.Bottom : l.Top);
            if (slot == null) { result.Problems.Add("a layer isn't one albedo"); return result; }
            var w = ReadStackWeight(d, l.Weight);
            if (w == null) { result.Problems.Add($"layer {slot}: weight not understood"); return result; }
            if (weightMap != null && weightMap != w.Texture) { result.Problems.Add("layers read different weight maps"); return result; }
            weightMap = w.Texture;

            recipe.Stack.Add(new MaterialDetailStackLayer
            {
                Albedo = slot,
                Color = color,
                Normal = normalLerps.TryGetValue(l.Weight.Id, out var nl) ? NormalOf(invert ? nl.Bottom : nl.Top) : null,
                Channel = w.Channel,
                Scale = w.Scale,
                Bias = w.Bias,
                Power = (float)w.Power,
                Invert = invert ? true : null,
                Replace = replace ? true : null
            });
        }
        if (recipe.Stack.Count == 0) recipe.Stack = null;
        recipe.StackWeightMap = weightMap == null ? null : Slot(weightMap + ".r");
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
