using StudioCore.Renderer;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Works out a blend recipe (MaterialBlendRecipe, the format of {GAME}_Blends.json) from a shader's G-buffer pixel
/// shader, and its vertex shader for where the weights' vertex values come from. This takes the place of the generator
/// that made the checked-in recipes, which was lost. See Documentation/Claude/Material Fidelity.
///
/// The albedo (RT2.x) is a chain of lerps: lerp(lerp(bottom, layer1, w1), layer2, w2) ... Each lerp is
/// (top - bottom) * w + bottom in the DXIL, with the bottom the same SSA value twice. Each weight is expanded into a
/// polynomial over its leaves and matched against the two forms the viewport draws (FlverShader.frag,
/// ComputeRecipeWeights):
///   hard: sat(1 - S * s * (2v - (D * m + bias)))
///   soft: sat(v - s * (1 - v) * (D * m + bias + 0.01))
/// where s is the sharpness (the cbuffer params every term shares), v the source (a mask channel, or a vertex value),
/// possibly inverted, and m the detail mask. A layer's normal is the top of the lerp in RT0.x with the same weight.
/// </summary>
static partial class Recipes
{
    public class Result
    {
        public MaterialBlendRecipe Recipe = new();
        public List<string> Problems = new();
        public bool Ok => Problems.Count == 0;
    }

    /// <summary>A polynomial: product of leaf names (sorted, joined by *) to its coefficient. "" is the constant</summary>
    class Poly : Dictionary<string, double>
    {
        public Poly() : base(StringComparer.Ordinal) { }
        public static Poly Of(string key, double v) => new() { [key] = v };
    }

    const int MaxTerms = 96;

    static Poly Expand(Dxil d, Dxil.Arg a, Dictionary<int, Poly> memo)
    {
        if (a.IsConst) return Poly.Of("", a.Value);
        if (memo.TryGetValue(a.Id, out var done)) return done;
        var n = d.NodeOf(a.Id);
        Poly result = n.Op switch
        {
            Dxil.Op.Leaf => Poly.Of(n.Leaf, 1),
            Dxil.Op.Add => Sum(Expand(d, n.Args[0], memo), Expand(d, n.Args[1], memo), 1),
            Dxil.Op.Sub => Sum(Expand(d, n.Args[0], memo), Expand(d, n.Args[1], memo), -1),
            Dxil.Op.Mul => Product(Expand(d, n.Args[0], memo), Expand(d, n.Args[1], memo), a.Id),
            Dxil.Op.Mad => Sum(Product(Expand(d, n.Args[0], memo), Expand(d, n.Args[1], memo), a.Id), Expand(d, n.Args[2], memo), 1),
            Dxil.Op.Div when n.Args[1].IsConst && n.Args[1].Value != 0 => Scale(Expand(d, n.Args[0], memo), 1 / n.Args[1].Value),
            // a.x b.x + a.y b.y + ...: the first half of the operands against the second
            Dxil.Op.Dot => Enumerable.Range(0, n.Args.Length / 2)
                .Select(i => Product(Expand(d, n.Args[i], memo), Expand(d, n.Args[i + n.Args.Length / 2], memo), a.Id))
                .Aggregate(new Poly(), (sum, p) => Sum(sum, p, 1)),
            _ => Poly.Of($"op{a.Id}", 1)
        };
        memo[a.Id] = result;
        return result;
    }

    static Poly Sum(Poly p, Poly q, double sign)
    {
        var r = new Poly();
        foreach (var (k, v) in p) r[k] = v;
        foreach (var (k, v) in q) r[k] = r.GetValueOrDefault(k) + sign * v;
        foreach (var k in r.Where(x => Math.Abs(x.Value) < 1e-9).Select(x => x.Key).ToList()) r.Remove(k);
        return r;
    }

    static Poly Scale(Poly p, double s)
    {
        var r = new Poly();
        foreach (var (k, v) in p) r[k] = v * s;
        return r;
    }

    static Poly Product(Poly p, Poly q, int id)
    {
        if (p.Count * q.Count > MaxTerms) return Poly.Of($"op{id}", 1);
        var r = new Poly();
        foreach (var (k1, v1) in p)
            foreach (var (k2, v2) in q)
            {
                var key = string.Join("*", k1.Split('*').Concat(k2.Split('*')).Where(x => x != "").OrderBy(x => x, StringComparer.Ordinal));
                r[key] = r.GetValueOrDefault(key) + v1 * v2;
            }
        foreach (var k in r.Where(x => Math.Abs(x.Value) < 1e-9).Select(x => x.Key).ToList()) r.Remove(k);
        return r;
    }

    record Lerp(int Id, Dxil.Arg Top, Dxil.Arg Bottom, Dxil.Arg Weight);

    /// <summary>(top - bottom) * w + bottom, with the bottom the same value both times, in either operand order</summary>
    static Lerp AsLerp(Dxil d, int id)
    {
        var n = d.NodeOf(id);
        IEnumerable<(Dxil.Arg product, Dxil.Arg bottom)> Pairs()
        {
            if (n.Op == Dxil.Op.Add)
            {
                yield return (n.Args[0], n.Args[1]);
                yield return (n.Args[1], n.Args[0]);
            }
        }

        foreach (var (product, bottom) in Pairs())
        {
            if (product.IsConst || bottom.IsConst) continue;
            var m = d.NodeOf(product.Id);
            if (m.Op != Dxil.Op.Mul) continue;
            foreach (var (f, w) in new[] { (m.Args[0], m.Args[1]), (m.Args[1], m.Args[0]) })
            {
                if (f.IsConst) continue;
                var s = d.NodeOf(f.Id);
                if (s.Op == Dxil.Op.Sub && !s.Args[1].IsConst && s.Args[1].Id == bottom.Id) return new Lerp(id, s.Args[0], bottom, w);
            }
        }

        if (n.Op == Dxil.Op.Mad && !n.Args[2].IsConst)
        {
            foreach (var (f, w) in new[] { (n.Args[0], n.Args[1]), (n.Args[1], n.Args[0]) })
            {
                if (f.IsConst) continue;
                var s = d.NodeOf(f.Id);
                if (s.Op == Dxil.Op.Sub && !s.Args[1].IsConst && s.Args[1].Id == n.Args[2].Id) return new Lerp(id, s.Args[0], n.Args[2], w);
            }
        }

        return null;
    }

    static readonly Regex SlotOf = new(@"snp_\d+_Texture2D(?:Array)?_(\d+)\.([rgba])$", RegexOptions.Compiled);
    static readonly Regex ColorParam = new(@"^cb:.*?(snp_\d+_color_\d+)_\d+$", RegexOptions.Compiled);

    static int? Slot(string leaf) => SlotOf.Match(leaf) is { Success: true } m ? int.Parse(m.Groups[1].Value) : null;
    static string Channel(string leaf) => leaf[^1..];

    /// <summary>The cbuffer field as the recipes name params: from snp_ on, as EvaluateProduct reads them</summary>
    static string Param(string leaf)
    {
        var name = leaf.StartsWith("cb:") ? leaf[3..] : leaf;
        var i = name.IndexOf("snp_", StringComparison.Ordinal);
        return i >= 0 ? name[i..] : name;
    }

    static string Number(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);

    /// <param name="albedoSlots">The slots the signature says are albedos, and normals</param>
    /// <param name="vertexSource">PS input (in:TEXCOORD3.x) to the vertex input it is passed from (TEXCOORD1.x), or null</param>
    public static Result Build(Dxil d, HashSet<int> albedoSlots, HashSet<int> normalSlots, Func<string, string> vertexSource)
    {
        var result = new Result();
        var root = d.StoreId("RT2.x");
        if (root == null) { result.Problems.Add("RT2.x isn't written"); return result; }

        bool HasAlbedo(Dxil.Arg a) => !a.IsConst && d.LeafNames(a.Id).Any(l => Slot(l) is int s && albedoSlots.Contains(s) && Channel(l) == "r");
        // A term of params alone with a color among them: a layer that is a flat color
        bool IsColorLayer(Dxil.Arg a) => !a.IsConst && d.LeafNames(a.Id).ToList() is { Count: > 0 } ls && ls.All(x => x.StartsWith("cb:")) && ls.Any(x => ColorParam.IsMatch(x));
        // Through a detail overlay multiplied over the layers below it (AC6 [MS]: color_7 * 4.55 * T22.r * the blend),
        // which the overlay recipe draws: a product of one albedo and params, times the rest
        Dxil.Arg Unwrapped(Dxil.Arg a)
        {
            for (var guard = 0; guard < 4 && !a.IsConst && d.NodeOf(a.Id) is { Op: Dxil.Op.Mul } m; guard++)
            {
                var inner = new[] { (m.Args[0], m.Args[1]), (m.Args[1], m.Args[0]) }
                    .Where(p => !p.Item1.IsConst && !p.Item2.IsConst && HasAlbedo(p.Item2)
                        && d.LeafNames(p.Item1.Id).ToList() is var ls && ls.Count(x => !x.StartsWith("cb:")) == 1 && HasAlbedo(p.Item1))
                    .Select(p => (Dxil.Arg?)p.Item2).FirstOrDefault();
                if (inner == null) break;
                a = inner.Value;
            }
            return a;
        }

        // The topmost lerp of albedos whose weight uses no albedo (an overlay's weight is the overlay itself)
        Lerp top = null;
        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(root.Value);
        while (stack.Count > 0 && top == null)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            var l = AsLerp(d, id);
            if (l != null && (HasAlbedo(l.Top) || IsColorLayer(l.Top)) && HasAlbedo(l.Bottom) && !HasAlbedo(l.Weight)) { top = l; break; }
            foreach (var a in d.NodeOf(id).Args.Where(a => !a.IsConst).Reverse()) stack.Push(a.Id);
        }
        if (top == null) { result.Problems.Add("no lerp of albedos"); return result; }

        var chain = new List<Lerp>();
        for (var l = top; l != null;)
        {
            chain.Insert(0, l);
            var next = AsLerp(d, Unwrapped(l.Bottom).Id);
            l = next != null && (HasAlbedo(next.Top) || IsColorLayer(next.Top)) && HasAlbedo(next.Bottom) && !HasAlbedo(next.Weight) ? next : null;
        }

        // A layer's albedo is the one albedo red in its term, its tint the color param there
        (int? slot, string color) Term(Dxil.Arg a)
        {
            var leaves = d.LeafNames(a.Id).ToList();
            var albedos = leaves.Where(x => Slot(x) is int s && albedoSlots.Contains(s) && Channel(x) == "r").Select(x => Slot(x)!.Value).Distinct().ToList();
            var color = leaves.Select(x => ColorParam.Match(x)).FirstOrDefault(m => m.Success)?.Groups[1].Value;
            // A layer of only a color param (AC6 [MS]) is -1, which the viewport draws white, tinted
            if (albedos.Count == 0 && color != null && leaves.All(x => x.StartsWith("cb:"))) return (-1, color);
            return (albedos.Count == 1 ? albedos[0] : null, color);
        }

        var recipe = result.Recipe;
        var (bottomSlot, bottomColor) = Term(Unwrapped(chain[0].Bottom));
        if (bottomSlot == null)
        {
            result.Problems.Add("the bottom layer isn't one albedo");
            if (Environment.GetEnvironmentVariable("RECIPE_DEBUG") == "3") Console.WriteLine($"   bottom: {d.ExpressionOf(chain[0].Bottom.Id, 8)}");
        }
        recipe.Albedo = bottomSlot ?? -1;
        recipe.AlbedoColor = bottomColor;

        // Normals: the lerps in the normal with the same weights
        var normalLerps = new Dictionary<int, Lerp>();
        Dxil.Arg? normalBottom = null;
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
            if (normalLerps.TryGetValue(chain[0].Weight.Id, out var first)) normalBottom = first.Bottom;
        }

        int? NormalOf(Dxil.Arg? a)
        {
            if (a == null || a.Value.IsConst) return null;
            var slots = d.LeafNames(a.Value.Id).Where(x => Slot(x) is int s && normalSlots.Contains(s) && Channel(x) is "r" or "g").Select(x => Slot(x)!.Value).Distinct().ToList();
            return slots.Count == 1 ? slots[0] : null;
        }
        recipe.Normal = NormalOf(normalBottom);

        int? mask = null, detailMask = null;
        double? scale = null, bias = null;

        foreach (var l in chain)
        {
            var (slot, color) = Term(l.Top);
            if (slot == null) { result.Problems.Add("a layer isn't one albedo"); continue; }
            var layer = new MaterialBlendRecipeLayer { Albedo = slot.Value, Color = color, Normal = normalLerps.TryGetValue(l.Weight.Id, out var nl) ? NormalOf(nl.Top) : null };

            var weight = ReadWeight(d, l.Weight, vertexSource, result.Problems);

            // The [MbN_ColorAdjust] form, lerp(v, hard, K): the recipe takes the hard weight, with v standing for its
            // mask channel. The lerp and v's vertex value come from {GAME}_ColorAdjust.json (ColorAdjust.cs)
            if (weight == null && MixedWeight(d, l.Weight) is { } mixed)
                weight = ReadWeight(d, mixed.hard, vertexSource, result.Problems, new Dictionary<int, Poly> { [mixed.value] = Poly.Of(mixed.mask, 1) });

            // Snow: the value is how much the surface faces a direction, sat(bias + dot(N, dir)), times a vertex value
            if (weight == null && FacingValue(d, l.Weight) is { } facing)
            {
                string FacingSource(string v) => v == "in:FACING" ? "FACING" : vertexSource(v);
                weight = ReadWeight(d, l.Weight, FacingSource, result.Problems, new Dictionary<int, Poly> { [facing.value] = Poly.Of("in:FACING", 1) });
                if (weight != null)
                {
                    layer.FacingDirection = facing.direction;
                    layer.FacingBias = facing.bias;
                    layer.FacingVertex = facing.vertex == null ? null : vertexSource(facing.vertex);
                }
            }

            // A weight scaled by a param as a whole, param * sat(...) (M[Cave_001]'s snp_0_float_16)
            if (weight == null && !l.Weight.IsConst && d.NodeOf(l.Weight.Id) is { Op: Dxil.Op.Mul } scaledWeight)
            {
                foreach (var (p, inner) in new[] { (scaledWeight.Args[0], scaledWeight.Args[1]), (scaledWeight.Args[1], scaledWeight.Args[0]) })
                {
                    if (p.IsConst || inner.IsConst || d.NodeOf(p.Id) is not { Op: Dxil.Op.Leaf } pl || !pl.Leaf.StartsWith("cb:") || !pl.Leaf.Contains("snp_")) continue;
                    if (ReadWeight(d, inner, vertexSource, result.Problems) is not { } scaled) continue;
                    weight = scaled;
                    layer.WeightScale = Param(pl.Leaf);
                    break;
                }
            }

            // Failing the recipe's own form, the extended one: a detail mask of the layer's own
            if (weight == null && ReadExtended(d, l.Weight, vertexSource, result.Problems) is { } ext)
            {
                layer.Source = ext.Source;
                layer.Invert = ext.Invert;
                layer.Soft = ext.Soft;
                layer.Sharpness = ext.Sharpness;
                layer.DetailMask = ext.DetailSlot;
                layer.DetailChannels = ext.Channels;
                layer.DetailBias = ext.Bias;
                layer.SoftScale = ext.SoftScale;
                if (ext.Source.StartsWith("Mask.") && Slot(d.LeafNames(l.Weight.Id).FirstOrDefault(x => x.StartsWith("tex:") && x.EndsWith(ext.Source[^1..])) ?? "") is int ms)
                {
                    if (mask != null && mask != ms) result.Problems.Add("layers use different masks");
                    mask = ms;
                }
                recipe.Layers.Add(layer);
                continue;
            }

            if (weight == null)
            {
                result.Problems.Add($"layer {slot}: weight not understood");
                if (Environment.GetEnvironmentVariable("RECIPE_DEBUG") == "1") Console.WriteLine($"   weight of {slot}: {d.ExpressionOf(l.Weight.Id, 10)}");
                recipe.Layers.Add(layer);
                continue;
            }
            var w = weight.Value;

            layer.Source = w.source;
            layer.Invert = w.invert;
            layer.Soft = w.soft;
            layer.Sharpness = w.sharpness;
            layer.Detail = w.detail;

            if (w.maskSlot != null)
            {
                if (mask != null && mask != w.maskSlot) result.Problems.Add("layers use different masks");
                mask = w.maskSlot;
            }
            if (w.detailSlot != null)
            {
                if (detailMask != null && detailMask != w.detailSlot) result.Problems.Add("layers use different detail masks");
                detailMask = w.detailSlot;
            }
            // The recipe has one scale and one detail bias: the first hard layer's. A hard layer of another scale takes
            // the ratio into its sharpness, which the scale only multiplies; a layer of another bias overrides it
            // (MaterialBlendRecipeLayer.DetailBiasOverride, drawn with the ColorAdjust form's entries)
            if (!w.soft)
            {
                if (scale != null && Math.Abs(scale.Value - w.scale) > 1e-3)
                    layer.Sharpness = $"{Number(w.scale / scale.Value)}*{layer.Sharpness}";
                scale ??= w.scale;
            }
            if (bias != null && Math.Abs(bias.Value - w.bias) > 1e-3) layer.DetailBiasOverride = (float)w.bias;
            bias ??= w.bias;
            recipe.Layers.Add(layer);
        }

        recipe.Mask = mask;
        recipe.DetailMask = detailMask;
        recipe.Scale = (float)(scale ?? 1);
        recipe.DetailBias = (float)(bias ?? 0);
        return result;
    }

    record struct Weight(string source, bool invert, bool soft, string sharpness, string detail, double scale, double bias, int? maskSlot, int? detailSlot, bool leftOut = false);

    /// <summary>
    /// Matches a weight against the hard and soft forms. The polynomial's leaves are named as NodeOf names them:
    /// tex:... texture channels, cb:... params, in:... interpolants, op... anything nonlinear (which fails).
    /// </summary>
    /// <summary>
    /// lerp(v, hard, K) with K a material param: the hard weight, the value v's SSA id, and the mask channel v reads
    /// (v is the mask, sat(mask + vertex), or bool ? that * vertex : that)
    /// </summary>
    static (Dxil.Arg hard, int value, string mask)? MixedWeight(Dxil d, Dxil.Arg w)
    {
        if (w.IsConst || AsLerp(d, w.Id) is not { } l || l.Weight.IsConst || l.Bottom.IsConst) return null;
        if (d.NodeOf(l.Weight.Id) is not { Op: Dxil.Op.Leaf } k || !k.Leaf.StartsWith("cb:") || !k.Leaf.Contains("snp_")) return null;
        if (Unsaturated(d, l.Top) == null) return null;

        string MaskOf(Dxil.Arg v)
        {
            if (v.IsConst) return null;
            var n = d.NodeOf(v.Id);
            if (n.Op == Dxil.Op.Leaf) return n.Leaf.StartsWith("tex:") ? n.Leaf : null;
            if (n is { Op: Dxil.Op.Other, Args.Length: 3 }) return MaskOf(n.Args[2]);
            if (Unsaturated(d, v) is { IsConst: false } sum && d.NodeOf(sum.Id) is { Op: Dxil.Op.Add } add)
            {
                var masks = add.Args.Where(a => !a.IsConst).Select(a => d.NodeOf(a.Id)).Where(x => x.Op == Dxil.Op.Leaf && x.Leaf.StartsWith("tex:")).ToList();
                var inputs = add.Args.Where(a => !a.IsConst).Select(a => d.NodeOf(a.Id)).Count(x => x.Op == Dxil.Op.Leaf && x.Leaf.StartsWith("in:"));
                return masks.Count == 1 && inputs == 1 ? masks[0].Leaf : null;
            }
            return null;
        }

        return MaskOf(l.Bottom) is { } mask ? (l.Top, l.Bottom.Id, mask) : null;
    }

    /// <summary>
    /// The facing value of a snow weight: sat(bias + dot(N, dir)) with dir a float3 param, times a vertex value or not.
    /// Its SSA id (the product if there is one), the direction param ("snp_0_float3_0"), the bias param, the vertex leaf
    /// </summary>
    static (int value, string direction, string bias, string vertex)? FacingValue(Dxil d, Dxil.Arg w)
    {
        if (w.IsConst) return null;
        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(w.Id);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            var n = d.NodeOf(id);
            foreach (var a in n.Args.Where(a => !a.IsConst)) stack.Push(a.Id);

            // sat(bias + dot(N, dir))
            if (n.Op != Dxil.Op.Saturate || n.Args[0].IsConst || d.NodeOf(n.Args[0].Id) is not { Op: Dxil.Op.Add } add) continue;
            var biasArg = add.Args.FirstOrDefault(a => !a.IsConst && d.NodeOf(a.Id) is { Op: Dxil.Op.Leaf } bl && bl.Leaf.StartsWith("cb:") && bl.Leaf.Contains("snp_"));
            var dotArg = add.Args.FirstOrDefault(a => !a.IsConst && d.NodeOf(a.Id).Op == Dxil.Op.Dot);
            if (biasArg.IsConst || dotArg.IsConst) continue;
            var dot = d.NodeOf(dotArg.Id);
            var dirLeaves = dot.Args.Skip(dot.Args.Length / 2).Where(a => !a.IsConst).Select(a => d.NodeOf(a.Id)).ToList();
            if (dirLeaves.Count != 3 || dirLeaves.Any(x => x.Op != Dxil.Op.Leaf || !x.Leaf.Contains("_float3_"))) continue;
            var direction = System.Text.RegularExpressions.Regex.Replace(Param(dirLeaves[0].Leaf), @"_\d+$", "");

            // Times a vertex value, where the weight uses the product
            int value = id;
            string vertex = null;
            foreach (var pid in seen)
            {
                if (d.NodeOf(pid) is { Op: Dxil.Op.Mul } m && m.Args.Any(a => !a.IsConst && a.Id == id)
                    && m.Args.FirstOrDefault(a => !a.IsConst && a.Id != id) is { Id: >= 0 } other && d.NodeOf(other.Id) is { Op: Dxil.Op.Leaf } vl && vl.Leaf.StartsWith("in:"))
                {
                    value = pid;
                    vertex = vl.Leaf;
                    break;
                }
            }
            return (value, direction, Param(d.NodeOf(biasArg.Id).Leaf), vertex);
        }
        return null;
    }

    static Weight? ReadWeight(Dxil d, Dxil.Arg w, Func<string, string> vertexSource, List<string> problems, Dictionary<int, Poly> preset = null)
    {
        if (w.IsConst) return null;
        // sat(x), or min(max(x, 0), 1)
        var n = d.NodeOf(w.Id);
        Dxil.Arg inner;
        if (n.Op == Dxil.Op.Saturate) inner = n.Args[0];
        else if (n.Op == Dxil.Op.Min && n.Args.Any(a => a.IsConst && a.Value == 1) && n.Args.First(a => !a.IsConst) is var mx && d.NodeOf(mx.Id) is { Op: Dxil.Op.Max } maxNode && maxNode.Args.Any(a => a.IsConst && a.Value == 0))
            inner = maxNode.Args.First(a => !a.IsConst);
        else return null;

        var memo = preset != null ? new Dictionary<int, Poly>(preset) : new Dictionary<int, Poly>();
        var direct = Match(d, Expand(d, inner, memo), vertexSource, problems);
        if (direct != null) return direct;

        // A form times (1 + something): M[AMSN_V][Mb2]'s (snp_0_float_1 * TEXCOORD0.w + 1), AC6's (snp_0_float_25 + 1).
        // The recipes leave the factor out, as the checked-in ones do
        if (!inner.IsConst && d.NodeOf(inner.Id) is { Op: Dxil.Op.Mul } product)
        {
            foreach (var (core, factor) in new[] { (product.Args[0], product.Args[1]), (product.Args[1], product.Args[0]) })
            {
                if (core.IsConst || factor.IsConst) continue;
                var f = Expand(d, factor, memo);
                if (Math.Abs(f.GetValueOrDefault("") - 1) > 1e-3) continue;
                var w2 = Match(d, Expand(d, core, memo), vertexSource, problems);
                if (w2 != null) return w2.Value with { leftOut = true };
            }
        }

        return null;
    }

    static Weight? Match(Dxil d, Poly poly, Func<string, string> vertexSource, List<string> problems)
    {
        if (poly.Keys.Any(k => k.Split('*').Any(f => f.StartsWith("op")))) return null;

        var terms = poly.Where(t => t.Key != "").ToList();
        var constant = poly.GetValueOrDefault("");
        bool IsSource(string f) => f.StartsWith("in:") || f.StartsWith("tex:");
        bool IsParam(string f) => f.StartsWith("cb:");

        // The source on its own, with no param, makes it soft: v - s(1 - v)(d + 0.01) has a bare v
        var bare = terms.Where(t => !t.Key.Contains('*') && IsSource(t.Key)).ToList();
        var soft = bare.Count == 1 && Math.Abs(Math.Abs(bare[0].Value) - 1) < 1e-3;

        // The sharpness: the params every term but the bare source shares
        var paramTerms = terms.Where(t => !soft || t.Key != bare[0].Key).ToList();
        if (paramTerms.Count == 0) return null;
        var shared = paramTerms.Select(t => t.Key.Split('*').Where(IsParam).ToHashSet()).Aggregate((a, b) => { a.IntersectWith(b); return a; });
        var sharpness = shared.Count == 0 ? "1" : string.Join("*", shared.OrderBy(x => x, StringComparer.Ordinal).Select(Param));

        // Each term without the sharpness: its sources and remaining params
        var rest = paramTerms.Select(t => (factors: t.Key.Split('*').Where(f => !shared.Contains(f)).ToList(), coef: t.Value)).ToList();
        var single = rest.Where(r => r.factors.Count(IsSource) == 1).ToList();
        var none = rest.Where(r => r.factors.Count(IsSource) == 0 && r.factors.Count == 0).Sum(r => r.coef);

        string v, m = null;
        double cv, cm = 0;
        string detailExtra = "";

        if (soft)
        {
            // Not inverted: v - s(d + 0.01) + s v (d + 0.01); inverted (1 - v): 1 - v - s v (d + 0.01)
            v = bare[0].Key;
            var invert = bare[0].Value < 0;
            if (invert && Math.Abs(constant - 1) > 1e-3) return null;
            if (!invert && Math.Abs(constant) > 1e-3) return null;
            // s v terms: inverted, -s v (D m + bias + 0.01); not inverted, + s v (D m + bias + 0.01)
            var sign = invert ? -1 : 1;
            var sv = rest.Where(r => r.factors.Count == 1 && r.factors[0] == v).Sum(r => r.coef) * sign;
            var svm = rest.Where(r => r.factors.Contains(v) && r.factors.Count(IsSource) == 2).ToList();
            if (svm.Count > 1) return null;
            if (svm.Count == 1)
            {
                m = svm[0].factors.First(f => IsSource(f) && f != v);
                cm = svm[0].coef * sign;
                detailExtra = string.Join("*", svm[0].factors.Where(IsParam).Select(Param));
            }
            var softBias = sv - 0.01;
            return Make(v, invert, true, sharpness, cm, detailExtra, 1, softBias, m, vertexSource, problems);
        }

        // Hard: 1 - S s (2 v' - D m - bias), with v' = v or 1 - v
        if (Math.Abs(constant - 1) > 1e-3) return null;
        var sources = single.Select(r => r.factors.First(IsSource)).Distinct().ToList();
        if (sources.Count is 0 or > 2) return null;
        // With two, the value is the vertex input if there is one, otherwise the larger coefficient (2S against S D)
        v = sources.Count == 1 ? sources[0]
            : sources.FirstOrDefault(s => s.StartsWith("in:")) ?? sources.OrderByDescending(s => Math.Abs(single.Where(r => r.factors.Contains(s)).Sum(r => r.coef))).First();
        cv = single.Where(r => r.factors.Contains(v)).Sum(r => r.coef);
        if (sources.Count == 2)
        {
            m = sources.First(s => s != v);
            var mterm = single.First(r => r.factors.Contains(m));
            cm = single.Where(r => r.factors.Contains(m)).Sum(r => r.coef);
            detailExtra = string.Join("*", mterm.factors.Where(IsParam).Select(Param));
        }
        var hardInvert = cv > 0;
        var scale = Math.Abs(cv) / 2;
        // s alone: not inverted S bias; inverted -2S + S bias
        var hardBias = hardInvert ? (none + 2 * scale) / scale : none / scale;
        return Make(v, hardInvert, false, sharpness, cm / scale, detailExtra, scale, hardBias, m, vertexSource, problems);
    }

    static Weight? Make(string v, bool invert, bool soft, string sharpness, double detail, string detailExtra, double scale, double bias, string m,
        Func<string, string> vertexSource, List<string> problems)
    {
        int? maskSlot = null;
        string source;
        if (v.StartsWith("tex:"))
        {
            maskSlot = Slot(v);
            source = $"Mask.{Channel(v)}";
        }
        else
        {
            source = vertexSource(v);
            if (source == null) { problems.Add($"{v} doesn't come straight from a vertex input"); return null; }
        }

        int? detailSlot = null;
        if (m != null)
        {
            if (!m.StartsWith("tex:") || Channel(m) != "r") { problems.Add($"detail {m} isn't a texture's red"); return null; }
            detailSlot = Slot(m);
        }

        var detailText = detailExtra == "" ? Number(detail) : Math.Abs(detail - 1) < 1e-4 ? detailExtra : $"{detailExtra}*{Number(detail)}";
        return new Weight(source, invert, soft, sharpness, m == null ? null : detailText, scale, bias, maskSlot, detailSlot);
    }

    // ---- The extended form ---------------------------------------------------------------------------------------
    //
    // AC6's [MS] blends give each layer its own detail mask and its own detail from it, a sum over the mask's
    // channels with weights and a bias that are polynomials in the material's params (one picks a channel by
    // dot(mask.rgba, snp_0_float3_N)), and scale a soft weight by a polynomial of its own:
    //   hard: sat(1 - s (2v - d)),                      d = sum_c C_c mask.c + bias
    //   soft: sat((v - s (1 - v) (d + 0.01)) * k)
    // with v possibly inverted. The weight splits as A + B v (v appearing once in each term). Hard: A's constant is 1
    // and B a single monomial, -2s (+2s inverted). Soft, not inverted: A = -s(d + 0.01), B = 1 + s(d + 0.01);
    // inverted: A = 1, B = -1 - s(d + 0.01).

    public record ExtendedWeight(string Source, bool Invert, bool Soft, string Sharpness, string[] Channels, string Bias, string SoftScale, int? DetailSlot);

    static Poly Without(Poly p, string factor, out Poly rest)
    {
        // p = rest + result * factor, with factor at most once in a term
        var with = new Poly();
        rest = new Poly();
        foreach (var (k, v) in p)
        {
            var fs = k.Split('*').Where(x => x != "").ToList();
            var n = fs.Count(x => x == factor);
            if (n > 1) return null;
            if (n == 0) { rest[k] = v; continue; }
            fs.Remove(factor);
            with[string.Join("*", fs)] = v;
        }
        return with;
    }

    /// <summary>Each term divided by the params of the monomial, or null if one lacks them</summary>
    static Poly DivideBy(Poly p, IReadOnlyCollection<string> paramsOf, double coefficient)
    {
        var r = new Poly();
        foreach (var (k, v) in p)
        {
            var fs = k.Split('*').Where(x => x != "").ToList();
            foreach (var f in paramsOf)
                if (!fs.Remove(f)) return null;
            r[string.Join("*", fs.OrderBy(x => x, StringComparer.Ordinal))] = v / coefficient;
        }
        return r;
    }

    /// <summary>A polynomial of the material's params as the recipes write one: "2*snp_0_float_52_0 + -1"</summary>
    static string Text(Poly p)
    {
        if (p.Count == 0) return "0";
        var terms = new List<string>();
        foreach (var (k, v) in p.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            var fs = k.Split('*').Where(x => x != "").ToList();
            // Only what a material sets can be evaluated for it
            if (fs.Any(f => !f.StartsWith("cb:") || !f.Contains("snp_"))) return null;
            var names = fs.Select(Param).ToList();
            terms.Add(names.Count == 0 ? Number(v) : Math.Abs(v - 1) < 1e-6 ? string.Join("*", names) : $"{Number(v)}*{string.Join("*", names)}");
        }
        return string.Join(" + ", terms);
    }

    static Dxil.Arg? Unsaturated(Dxil d, Dxil.Arg w)
    {
        if (w.IsConst) return null;
        var n = d.NodeOf(w.Id);
        if (n.Op == Dxil.Op.Saturate) return n.Args[0];
        if (n.Op == Dxil.Op.Min && n.Args.Any(a => a.IsConst && a.Value == 1) && n.Args.FirstOrDefault(a => !a.IsConst) is { Id: >= 0 } mx
            && d.NodeOf(mx.Id) is { Op: Dxil.Op.Max } maxNode && maxNode.Args.Any(a => a.IsConst && a.Value == 0))
            return maxNode.Args.First(a => !a.IsConst);
        return null;
    }

    public static ExtendedWeight ReadExtended(Dxil d, Dxil.Arg w, Func<string, string> vertexSource, List<string> problems)
    {
        if (Unsaturated(d, w) is not { } inner) return null;
        var memo = new Dictionary<int, Poly>();
        static bool IsSource(string f) => f.StartsWith("in:") || f.StartsWith("tex:");

        // A factor of params alone scales the weight (AC6's snp_0_float_25 + 1)
        var cores = new List<(Dxil.Arg core, Poly scale)> { (inner, Poly.Of("", 1)) };
        if (!inner.IsConst && d.NodeOf(inner.Id) is { Op: Dxil.Op.Mul } product)
            foreach (var (core, factor) in new[] { (product.Args[0], product.Args[1]), (product.Args[1], product.Args[0]) })
            {
                if (core.IsConst || factor.IsConst) continue;
                var f = Expand(d, factor, memo);
                if (f.Keys.All(k => k.Split('*').All(x => x == "" || x.StartsWith("cb:")))) cores.Add((core, f));
            }

        foreach (var (core, scale) in cores)
        {
            var e = Expand(d, core, memo);
            if (Environment.GetEnvironmentVariable("RECIPE_DEBUG") == "2") Console.WriteLine($"   ext core: {string.Join(" + ", e.Select(t => $"{t.Value:0.###}*{t.Key}"))}  scale: {string.Join(" + ", scale.Select(t => $"{t.Value:0.###}*{t.Key}"))}");
            if (e.Keys.Any(k => k.Contains("op"))) continue;
            // The value: a vertex value first, then a texture channel
            var candidates = e.Keys.SelectMany(k => k.Split('*')).Where(IsSource).Distinct()
                .OrderBy(x => x.StartsWith("in:") ? 0 : 1).ToList();
            foreach (var v in candidates)
            {
                var b = Without(e, v, out var a);
                if (b == null || b.Count == 0) continue;
                var aConst = a.GetValueOrDefault("");
                var bConst = b.GetValueOrDefault("");

                Poly sd = null;     // s (d + 0.01) for soft, s d for hard
                bool soft, invert;
                List<string> sParams;
                double sCoef;

                var bTerms = b.Where(t => t.Key != "").ToList();
                if (Math.Abs(aConst - 1) < 1e-4 && b.Count == 1 && bConst == 0 && !bTerms[0].Key.Split('*').Any(IsSource))
                {
                    // Hard: B = -2s or +2s
                    soft = false;
                    invert = bTerms[0].Value > 0;
                    sParams = bTerms[0].Key.Split('*').ToList();
                    sCoef = Math.Abs(bTerms[0].Value) / 2;
                    var x = Sum(a, Poly.Of("", -1), 1);
                    if (invert) x = Sum(x, Scale(Poly.Of(bTerms[0].Key, 1), 2 * sCoef), 1);
                    var dPoly = DivideBy(x, sParams, sCoef);
                    if (dPoly == null) continue;
                    if (!scale.Keys.SequenceEqual([""]) || Math.Abs(scale[""] - 1) > 1e-6) continue; // a hard weight isn't scaled
                    return Finish(v, invert, false, sParams, sCoef, dPoly, null, vertexSource, problems);
                }

                if (Math.Abs(aConst) < 1e-4 && Math.Abs(bConst - 1) < 1e-4) { soft = true; invert = false; sd = Scale(a, -1); }
                else if (a.Count == 1 && Math.Abs(aConst - 1) < 1e-4 && Math.Abs(bConst + 1) < 1e-4) { soft = true; invert = true; sd = Sum(Scale(b, -1), Poly.Of("", -1), 1); }
                else continue;

                // The sharpness is the params every term of s (d + 0.01) shares. With nothing there it isn't soft
                if (sd.Count == 0) continue;
                var shared = sd.Keys.Select(k => k.Split('*').Where(x => x.StartsWith("cb:")).ToHashSet()).Aggregate((p, q) => { p.IntersectWith(q); return p; });
                var dd = DivideBy(sd, shared, 1);
                if (dd == null) continue;
                dd = Sum(dd, Poly.Of("", -0.01), 1);
                return Finish(v, invert, true, shared.ToList(), 1, dd, scale, vertexSource, problems);
            }
        }

        return null;
    }

    static ExtendedWeight Finish(string v, bool invert, bool soft, List<string> sParams, double sCoef, Poly detail, Poly scale,
        Func<string, string> vertexSource, List<string> problems)
    {
        string source;
        if (v.StartsWith("tex:")) source = $"Mask.{Channel(v)}";
        else
        {
            source = vertexSource(v);
            if (source == null) { problems.Add($"{v} doesn't come straight from a vertex input"); return null; }
        }

        // The detail: its texture's channels, and what is left the bias
        var channels = new[] { new Poly(), new Poly(), new Poly(), new Poly() };
        var bias = new Poly();
        string texture = null;
        foreach (var (k, c) in detail)
        {
            var fs = k.Split('*').Where(x => x != "").ToList();
            var tex = fs.Where(x => x.StartsWith("tex:")).ToList();
            if (fs.Any(x => x.StartsWith("in:")) || tex.Count > 1) return null;
            if (tex.Count == 0) { bias[k] = c; continue; }
            var name = tex[0][..^2];
            if (texture != null && texture != name) return null;
            texture = name;
            fs.Remove(tex[0]);
            channels["rgba".IndexOf(tex[0][^1])][string.Join("*", fs)] = c;
        }

        var sharpParams = sParams.Where(x => x != "").ToList();
        if (sharpParams.Any(x => !x.Contains("snp_"))) return null;
        var sharpness = string.Join("*", (sCoef != 1 ? [Number(sCoef)] : new List<string>()).Concat(sharpParams.Select(Param)));
        var texts = channels.Select(Text).ToArray();
        var biasText = Text(bias);
        var scaleText = scale == null ? null : Text(scale);
        if (texts.Any(t => t == null) || biasText == null || (scale != null && scaleText == null)) return null;
        return new ExtendedWeight(source, invert, soft, sharpness == "" ? "1" : sharpness, texts, biasText,
            scaleText is null or "1" ? null : scaleText, texture == null ? null : Slot(texture + ".r"));
    }

    /// <summary>Whether two recipes say the same, for checking against the checked-in ones</summary>
    public static List<string> Compare(MaterialBlendRecipe ours, MaterialBlendRecipe theirs)
    {
        var diffs = new List<string>();
        void Check(string what, object a, object b) { if (!Equals(a, b)) diffs.Add($"{what}: {b} vs ours {a}"); }
        Check("Albedo", ours.Albedo, theirs.Albedo);
        Check("Normal", ours.Normal, theirs.Normal);
        Check("Mask", ours.Mask, theirs.Mask);
        Check("DetailMask", ours.DetailMask, theirs.DetailMask);
        Check("Layers", ours.Layers.Count, theirs.Layers.Count);
        if (Math.Abs(ours.Scale - theirs.Scale) > 1e-3) diffs.Add($"Scale: {theirs.Scale} vs ours {ours.Scale}");
        if (Math.Abs(ours.DetailBias - theirs.DetailBias) > 1e-3) diffs.Add($"DetailBias: {theirs.DetailBias} vs ours {ours.DetailBias}");
        for (var i = 0; i < Math.Min(ours.Layers.Count, theirs.Layers.Count); i++)
        {
            var a = ours.Layers[i];
            var b = theirs.Layers[i];
            Check($"L{i}.Albedo", a.Albedo, b.Albedo);
            Check($"L{i}.Normal", a.Normal, b.Normal);
            Check($"L{i}.Source", a.Source, b.Source);
            Check($"L{i}.Invert", a.Invert, b.Invert);
            Check($"L{i}.Soft", a.Soft, b.Soft);
            Check($"L{i}.Sharpness", a.Sharpness, b.Sharpness);
            Check($"L{i}.Detail", a.Detail, b.Detail);
            Check($"L{i}.Color", a.Color, b.Color);
        }
        return diffs;
    }
}
