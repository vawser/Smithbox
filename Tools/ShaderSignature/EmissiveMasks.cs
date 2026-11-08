using StudioCore.Renderer;

/// <summary>
/// Shaders whose glow is a mask's red, tinted ({GAME}_EmissiveMasks.json, MaterialEmissiveMask). M[Crystal] in NR:
///   RT4.rgb = Mask1Map.r * (1 - TEXCOORD1.y) * color_0.rgb * f3 * (engine factors)
/// The emissive is one product: one texture's red, one color param, and float params. Other factors (a vertex fade,
/// a sat(... + 1)) are left out, so the glow can come out brighter than the game's where those are below 1.
/// </summary>
static partial class Recipes
{
    public static MaterialEmissiveMask ReadEmissiveMask(Dxil d)
    {
        var outputs = new[] { "RT4.x", "RT4.y", "RT4.z" };
        string texture = null;
        string color = null;
        List<string> factors = null;

        for (var c = 0; c < 3; c++)
        {
            if (d.StoreId(outputs[c]) is not int id) return null;
            // Each term the one texture's red and the one color, with what else differs from term to term (a vertex
            // fade 1 - v expands to two) left out; the floats every term shares are the factor
            var poly = Expand(d, new Dxil.Arg(id, 0), new Dictionary<int, Poly>());
            if (poly.Count == 0 || poly.Count > 4) return null;
            HashSet<string> shared = null;
            foreach (var term in poly.Keys)
            {
                var factorsHere = term.Split('*').Where(x => x != "").ToList();

                var textures = factorsHere.Where(x => x.StartsWith("tex:")).ToList();
                if (textures.Count != 1 || Channel(textures[0]) != "r" || Slot(textures[0]) == null) return null;
                if (texture != null && texture != textures[0]) return null;
                texture = textures[0];

                var colors = factorsHere.Where(x => ColorParam.IsMatch(x)).ToList();
                if (colors.Count != 1) return null;
                var name = ColorParam.Match(colors[0]).Groups[1].Value;
                if (color != null && color != name) return null;
                color = name;

                // The material's own floats; engine fields and anything nonlinear are left out
                var floats = factorsHere.Where(x => x.StartsWith("cb:") && x.Contains("snp_") && !ColorParam.IsMatch(x)).Select(Param).ToHashSet();
                if (shared == null) shared = floats;
                else shared.IntersectWith(floats);
            }
            factors = shared.OrderBy(x => x, StringComparer.Ordinal).ToList();
        }

        return new MaterialEmissiveMask
        {
            Slot = Slot(texture)!.Value,
            Color = color,
            Factor = factors is { Count: > 0 } ? string.Join("*", factors) : null
        };
    }
}
