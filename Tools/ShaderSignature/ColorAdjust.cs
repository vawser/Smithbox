using StudioCore.Renderer;

/// <summary>
/// The [MbN_ColorAdjust] form of ER/NR's terrain blends ({GAME}_ColorAdjust.json, MaterialColorAdjustRecipe), which
/// the checked-in recipes only have the hard part of. In M[AMSN_V][Ov_N][Mb3_ColorAdjust][3Mask_ch18]:
///   v      = sat(Mask.r + TEXCOORD1.x), times COLOR1.w where bool_0 is set
///   hard   = sat(1 - 1.4 f6 (2 (1 - v) - (1 - T3.r)))          the recipe's own weight
///   weight = lerp(v, hard, f3)                                  f3 is 0.1 - 0.2 in most materials
/// and each layer's albedo, the bottom too, is bool_k ? lerp(T, color_k, f_k) : T * color_k.
/// </summary>
static partial class Recipes
{
    public static MaterialColorAdjustRecipe ReadColorAdjust(Dxil d, HashSet<int> albedoSlots, Func<string, string> vertexSource)
    {
        var result = new MaterialColorAdjustRecipe();
        if (d.StoreId("RT2.x") is not int root) return result;

        bool IsParam(Dxil.Arg a, string kind) =>
            !a.IsConst && d.NodeOf(a.Id) is { Op: Dxil.Op.Leaf } n && n.Leaf.StartsWith("cb:") && n.Leaf.Contains($"_{kind}_");
        string ParamOf(Dxil.Arg a) => Param(d.NodeOf(a.Id).Leaf);
        int? AlbedoLeaf(Dxil.Arg a) =>
            !a.IsConst && d.NodeOf(a.Id) is { Op: Dxil.Op.Leaf } n && Slot(n.Leaf) is int s && albedoSlots.Contains(s) && Channel(n.Leaf) == "r" ? s : null;

        // bool ? x : y, the bool a material param
        (string flag, Dxil.Arg yes, Dxil.Arg no)? Select(Dxil.Arg a)
        {
            if (a.IsConst || d.NodeOf(a.Id) is not { Op: Dxil.Op.Other, Args.Length: 3 } n || n.Args[0].IsConst) return null;
            var leaves = d.LeafNames(n.Args[0].Id).ToList();
            if (leaves.Count != 1 || !leaves[0].StartsWith("cb:") || !leaves[0].Contains("_bool_")) return null;
            return (Param(leaves[0]), n.Args[1], n.Args[2]);
        }

        MaterialBlendRecipeLayer Layer(int slot) => result.Layers.TryGetValue(slot, out var l) ? l : result.Layers[slot] = new MaterialBlendRecipeLayer { Albedo = slot };

        var nodes = new List<int>();
        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            nodes.Add(id);
            foreach (var a in d.NodeOf(id).Args.Where(a => !a.IsConst)) stack.Push(a.Id);
        }

        // Albedos: bool ? lerp(T, color, f) : T * color
        foreach (var id in nodes)
        {
            if (Select(new Dxil.Arg(id, 0)) is not { } s || s.yes.IsConst) continue;
            if (AsLerp(d, s.yes.Id) is not { } l || !IsParam(l.Weight, "float") || !IsParam(l.Top, "color") || AlbedoLeaf(l.Bottom) is not int slot) continue;
            var layer = Layer(slot);
            layer.ColorAdjust = ParamOf(l.Weight);
            layer.ColorAdjustIf = s.flag;
        }

        // Weights: lerp(v, hard, K), and the albedo lerp they weigh
        foreach (var id in nodes)
        {
            if (AsLerp(d, id) is not { } mix || !IsParam(mix.Weight, "float") || Unsaturated(d, mix.Top) == null) continue;

            // v: sat(mask + vertex), or bool ? that * vertex : that
            var v = mix.Bottom;
            string scale = null, scaleIf = null;
            if (Select(v) is { } sel && !sel.yes.IsConst && d.NodeOf(sel.yes.Id) is { Op: Dxil.Op.Mul } scaled)
            {
                var plain = scaled.Args.FirstOrDefault(a => !a.IsConst && a.Id == sel.no.Id);
                var by = scaled.Args.FirstOrDefault(a => !a.IsConst && a.Id != sel.no.Id);
                if (plain.IsConst || by.IsConst || d.NodeOf(by.Id) is not { Op: Dxil.Op.Leaf } byLeaf || !byLeaf.Leaf.StartsWith("in:")) continue;
                scale = vertexSource(byLeaf.Leaf);
                scaleIf = sel.flag;
                v = sel.no;
            }
            // v as the mask alone (M[Cave_001]), or sat(mask + vertex)
            Dxil.Node inLeaf = null, maskLeaf = null;
            if (!v.IsConst && d.NodeOf(v.Id) is { Op: Dxil.Op.Leaf } bare && bare.Leaf.StartsWith("tex:"))
            {
                maskLeaf = bare;
            }
            else
            {
                if (Unsaturated(d, v) is not { } sum || sum.IsConst || d.NodeOf(sum.Id) is not { Op: Dxil.Op.Add } add) continue;
                inLeaf = add.Args.Select(a => a.IsConst ? null : d.NodeOf(a.Id)).FirstOrDefault(n => n is { Op: Dxil.Op.Leaf } && n.Leaf.StartsWith("in:"));
                maskLeaf = add.Args.Select(a => a.IsConst ? null : d.NodeOf(a.Id)).FirstOrDefault(n => n is { Op: Dxil.Op.Leaf } && n.Leaf.StartsWith("tex:"));
                if (inLeaf == null || maskLeaf == null) continue;
            }

            // The albedo lerp this weighs, and its top layer's albedo (through its color adjust select)
            foreach (var aid in nodes)
            {
                if (AsLerp(d, aid) is not { } al || al.Weight.IsConst || al.Weight.Id != id) continue;
                var top = Select(al.Top) is { } ts ? ts.no : al.Top;
                var slots = top.IsConst ? [] : d.LeafNames(top.Id).Where(x => Slot(x) is int s && albedoSlots.Contains(s) && Channel(x) == "r").Select(x => Slot(x)!.Value).Distinct().ToList();
                if (slots.Count != 1) continue;
                var layer = Layer(slots[0]);
                layer.HardMix = ParamOf(mix.Weight);
                layer.AddSource = inLeaf == null ? null : vertexSource(inLeaf.Leaf);
                layer.ScaleSource = scale;
                layer.ScaleIf = scaleIf;
                layer.Source = $"Mask.{Channel(maskLeaf.Leaf)}";
            }
        }

        return result;
    }
}
