using StudioCore.Renderer;
using System.Text.Json;

/// <summary>What Tools/ShaderSignature says a shader's slot is to the game, see its SlotMeanings.json</summary>
class SlotMeaning
{
    public string Meaning { get; set; } = "";
    public int UvSet { get; set; } = -1;
}

class SlotMeanings
{
    public SortedDictionary<int, SlotMeaning> Slots { get; set; } = new();
    public SortedDictionary<int, List<int>> Permutations { get; set; } = new();
}

/// <summary>
/// Material Fidelity Phase 3: each texture's viewport route (MaterialBinding, the renderer's own code) against what
/// the game's shader does with that slot (the signatures), for the permutation the material picks.
/// </summary>
static class Audit
{
    public static Dictionary<string, SlotMeanings> Load(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"No slot meanings at {path}, the audit is skipped");
            return new();
        }

        return new(JsonSerializer.Deserialize<Dictionary<string, SlotMeanings>>(File.ReadAllText(path))!, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>What a route means in the game's terms</summary>
    static string MeaningOf(MaterialTextureUse use) => use switch
    {
        MaterialTextureUse.Albedo or MaterialTextureUse.OverlayAlbedo or MaterialTextureUse.FurAlbedo => "Albedo",
        MaterialTextureUse.Normal or MaterialTextureUse.OverlayNormal or MaterialTextureUse.WaterNormal or MaterialTextureUse.FurNormal => "Normal",
        MaterialTextureUse.Metallic => "Metallic",
        MaterialTextureUse.Emissive or MaterialTextureUse.MagmaGlow => "Emissive",
        MaterialTextureUse.BlendMask or MaterialTextureUse.BlendDetailMask => "Mask",
        MaterialTextureUse.Dropped => "",
        var other => other.ToString()
    };

    /// <summary>
    /// The status of one texture:
    /// Ok, Dropped (the game uses it, the viewport doesn't), Misrouted (bound as something else),
    /// UvMismatch (right meaning, other UV set), Extra (bound, but the material's permutation doesn't sample it),
    /// Approximate (a stand-in surface: water, fur), Unverified (bound where the signature can't say what it is).
    /// </summary>
    /// <param name="layerAlbedos">The albedo the viewport puts in each layer, which decides the layer's UV set</param>
    public static (string expected, string status) Classify(MaterialDefinition d, MaterialSampler s, MaterialTextureRoute route,
        SlotMeanings? meanings, string?[] layerAlbedos)
    {
        if (meanings == null) return ("(no signature)", "Unverified");
        // g_ named samplers (SpeedTree, older-style shaders) have no Texture2D_N, so no slot to look up
        if (s.Slot < 0) return ("(no slot)", "Unverified");

        // The permutation the material picks; a shader without numbered ones has a single set of slots
        var sampled = true;
        if (meanings.Permutations.Count > 0)
        {
            var perm = d.Params.TryGetValue("GXFT_SATStaticSwitch", out var v) && v.Length > 0 ? (int)v[0] : 0;
            if (meanings.Permutations.TryGetValue(perm, out var slots)) sampled = slots.Contains(s.Slot);
        }

        SlotMeaning? m = null;
        var expected = sampled && meanings.Slots.TryGetValue(s.Slot, out m) ? m.Meaning : "NotSampled";
        var bound = route.Use != MaterialTextureUse.Dropped;
        var have = MeaningOf(route.Use);

        if (expected is "NotSampled" or "Unused") return (expected, bound ? "Extra" : "Ok");
        if (route.Use is MaterialTextureUse.WaterNormal or MaterialTextureUse.FurAlbedo or MaterialTextureUse.FurNormal) return (expected, "Approximate");
        if (!bound) return (expected, "Dropped");

        var same = have == expected || (have == "Normal" && expected == "NormalGloss");
        if (same)
        {
            // As FlverResource.ConfigureMaterialShader: a layer uses its albedo's UV set where the table says it, and
            // otherwise the bottom layer and known blends read the first set, other layers the second
            var albedo = route.Layer < layerAlbedos.Length ? layerAlbedos[route.Layer] : null;
            var albedoSet = albedo == null ? -1 : d.Samplers.Find(x => x.Type == albedo)?.UvSet ?? -1;
            var uv = albedoSet >= 0 ? albedoSet : route.Layer == 0 || MaterialBinding.HasKnownBlend(d) ? 0 : 1;
            var gameUv = m!.UvSet;
            return (expected, gameUv >= 0 && gameUv != uv && route.Use is MaterialTextureUse.Albedo or MaterialTextureUse.Normal or MaterialTextureUse.Metallic
                ? "UvMismatch" : "Ok");
        }

        return (expected, expected is "Other" or "Alpha" or "Coordinates" ? "Unverified" : "Misrouted");
    }
}
