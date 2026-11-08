using StudioCore.Renderer;

/// <summary>
/// Fur whose strands show only where a mask says ({GAME}_FurMasks.json, MaterialFurMask). C[Shell]_FurBlur:
///   albedo = base * detail * 4.55 * (4.55 * lerp(0.2196, strands.r, Mask1Map.r)) * ...
/// which the DXIL writes as (M.r * (S.r - 0.2196)) + 0.2196. Where the mask is 0 the strands are the neutral 0.2196,
/// which the 4.55 cancels.
/// </summary>
static partial class Recipes
{
    public static MaterialFurMask ReadFurMask(Dxil d)
    {
        if (d.StoreId("RT2.x") is not int root) return null;
        bool Is(Dxil.Arg a, double v) => a.IsConst && Math.Abs(a.Value - v) < 1e-3;
        int? Red(Dxil.Arg a) => !a.IsConst && d.NodeOf(a.Id) is { Op: Dxil.Op.Leaf } n && n.Leaf.StartsWith("tex:") && Channel(n.Leaf) == "r" ? Slot(n.Leaf) : null;

        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            var n = d.NodeOf(id);
            foreach (var a in n.Args.Where(a => !a.IsConst)) stack.Push(a.Id);

            if (n.Op != Dxil.Op.Add || !n.Args.Any(a => Is(a, 0.2196))) continue;
            var product = n.Args.FirstOrDefault(a => !a.IsConst);
            if (product.IsConst || d.NodeOf(product.Id) is not { Op: Dxil.Op.Mul } m) continue;
            foreach (var (mask, shifted) in new[] { (m.Args[0], m.Args[1]), (m.Args[1], m.Args[0]) })
            {
                if (Red(mask) is not int maskSlot || shifted.IsConst || d.NodeOf(shifted.Id) is not { Op: Dxil.Op.Add } s || !s.Args.Any(a => Is(a, -0.2196))) continue;
                if (Red(s.Args.First(a => !a.IsConst)) is not int strandSlot) continue;
                return new MaterialFurMask { Strands = strandSlot, Mask = maskSlot };
            }
        }
        return null;
    }
}
