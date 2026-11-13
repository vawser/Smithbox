#nullable enable
using System;
using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>What the viewport does with one texture of a material</summary>
public enum MaterialTextureUse
{
    /// <summary>Bound nowhere, see <see cref="MaterialTextureRoute.Reason"/></summary>
    Dropped,
    Albedo,
    Normal,
    /// <summary>A specular color map (DS3, Sekiro, Bloodborne)</summary>
    Specular,
    Shininess,
    /// <summary>A metallic map, in the specular slot (Elden Ring and later)</summary>
    Metallic,
    Emissive,
    BlendMask,
    BlendDetailMask,
    /// <summary>DS3's g_DOLTexture: the textures are the map's, so only the material is marked</summary>
    Lightmap,
    MagmaGlow,
    OverlayAlbedo,
    OverlayNormal,
    WaterNormal,
    FurAlbedo,
    FurNormal
}

/// <summary>
/// Where a texture goes. <see cref="LoadedElsewhere"/> textures are loaded from the definition before the
/// material's own textures are gone through (FlverResource.LoadBlendRecipe), so they need nothing more then.
/// </summary>
public readonly record struct MaterialTextureRoute(MaterialTextureUse Use, int Layer = 0, bool LoadedElsewhere = false,
    bool FromKnownLayers = false, string Reason = "")
{
    public static MaterialTextureRoute Drop(string reason) => new(MaterialTextureUse.Dropped, Reason: reason);
}

/// <summary>
/// The one place that decides where a material's textures go, so that the viewport (FlverResource) and the
/// census (Tools/MaterialSurvey) can't disagree. It only decides: loading is left to the caller.
/// </summary>
public static class MaterialBinding
{
    /// <summary>The most layers the viewport draws, Material.MaxLayers</summary>
    public const int MaxLayers = 4;

    /// <summary>
    /// Whether the renderer knows how to weigh the layers of this material. Otherwise only its
    /// first layer is shown, since blending by the wrong weights looks worse than not blending.
    /// </summary>
    public static bool HasKnownBlend(MaterialDefinition? definition)
    {
        return definition is { BlendWeight: MaterialBlendWeight.Recipe } or { BlendWeight: MaterialBlendWeight.TexCoord1X, BlendLayers.Count: 2 };
    }

    /// <summary>
    /// The metallic map of a material that is drawn without known layers: the bottom layer's (the lowest
    /// numbered that has a file), as the blend recipes don't say which layer the others belong to.
    /// </summary>
    public static string? ChooseMetallic(MaterialDefinition? definition, IReadOnlyDictionary<string, string> flverPaths)
    {
        string? best = null;
        var bestKey = (int.MaxValue, int.MaxValue);

        foreach (MaterialSampler s in definition?.Samplers ?? [])
        {
            if (s.Role != "MetallicMap" || (s.Path == "" && (!flverPaths.TryGetValue(s.Type, out var p) || p == "")))
            {
                continue;
            }

            var key = (Math.Max(s.Suffix, 0), s.Slot);
            if (key.CompareTo(bestKey) < 0)
            {
                bestKey = key;
                best = s.Type;
            }
        }

        return best;
    }

    /// <summary>
    /// Where a texture of a material goes.
    /// </summary>
    /// <param name="texType">The texture's type as the FLVER names it, which is the material's sampler type</param>
    /// <param name="metallicType">What <see cref="ChooseMetallic"/> picked for the material</param>
    /// <param name="glossInSpecular">DS1R and DS2 keep their gloss in the specular slot</param>
    public static MaterialTextureRoute Route(MaterialDefinition? d, string? texType, string? metallicType, bool glossInSpecular)
    {
        MaterialSampler? sampler = texType == null ? null : d?.Samplers.Find(x => x.Type == texType);
        var role = sampler?.Role;

        if (d?.Magma?.TextureType == texType && texType != null)
        {
            return new(MaterialTextureUse.MagmaGlow, LoadedElsewhere: true);
        }

        // A mask whose red is the glow (MaterialEmissiveMask)
        if (texType != null && d?.EmissiveMaskType == texType)
        {
            return new(MaterialTextureUse.Emissive);
        }

        if (d?.Detail is { } overlay && texType != null && (texType == overlay.AlbedoType || texType == overlay.NormalType))
        {
            return new(texType == overlay.AlbedoType ? MaterialTextureUse.OverlayAlbedo : MaterialTextureUse.OverlayNormal, LoadedElsewhere: true);
        }

        // The details stacked over the first (C[DetailBlend]), layer 1 up. Their weight map is the base normal, which
        // is loaded a second time for it and so still routed as itself below
        if (d?.Detail is { HasStackEntries: true } stacked && texType != null)
        {
            for (var i = 0; i < stacked.Stack.Count; i++)
            {
                if (texType == stacked.Stack[i].AlbedoType) return new(MaterialTextureUse.OverlayAlbedo, i + 1, LoadedElsewhere: true);
                if (texType == stacked.Stack[i].NormalType) return new(MaterialTextureUse.OverlayNormal, i + 1, LoadedElsewhere: true);
            }

            // The back face's, after the stack's three
            if (texType == stacked.BackAlbedoType) return new(MaterialTextureUse.OverlayAlbedo, MaterialResolvedDetail.MaxStackLayers + 1, LoadedElsewhere: true);
            if (texType == stacked.BackNormalType) return new(MaterialTextureUse.OverlayNormal, MaterialResolvedDetail.MaxStackLayers + 1, LoadedElsewhere: true);
        }

        // All of the normals of water end the same, and the last would replace the one that was chosen
        if (d?.Water != null && d.SurfaceType != null && role == "NormalMap")
        {
            return texType == d.SurfaceType || texType == d.Water.DetailType
                ? new(MaterialTextureUse.WaterNormal, LoadedElsewhere: true)
                : MaterialTextureRoute.Drop("a water normal the water recipe doesn't use");
        }

        // Fur's masked layer (MaterialFurMask): the mask in the blend mask slot and the texture in the third albedo slot,
        // which fur has no other use for
        if (d?.Fur is { MaskType: { } furMask } && texType == furMask)
        {
            return new(MaterialTextureUse.BlendMask, LoadedElsewhere: true);
        }

        if (d?.Fur is { MaskedType: { } furMasked } && texType == furMasked)
        {
            return new(MaterialTextureUse.FurAlbedo, 2, LoadedElsewhere: true);
        }

        // The same goes for fur, and its albedos as well
        if (d?.Fur is { } fur && role is "AlbedoMap" or "NormalMap")
        {
            if (texType == fur.BaseType || texType == fur.StrandType) return new(MaterialTextureUse.FurAlbedo, LoadedElsewhere: true);
            if (texType == fur.BaseNormalType || texType == fur.StrandNormalType) return new(MaterialTextureUse.FurNormal, LoadedElsewhere: true);
            return MaterialTextureRoute.Drop("a fur texture the fur recipe doesn't use");
        }

        if (texType != null && HasKnownBlend(d))
        {
            List<MaterialBlendLayer> blendLayers = d!.BlendLayers;

            if (d.Blend != null)
            {
                if (texType == d.Blend.MaskType) return new(MaterialTextureUse.BlendMask, LoadedElsewhere: true);
                if (texType == d.Blend.DetailType) return new(MaterialTextureUse.BlendDetailMask, LoadedElsewhere: true);

                // The extended form's detail mask of a layer (FlverResource.LoadBlendRecipe)
                var layerMask = Array.IndexOf(d.Blend.LayerDetailTypes, texType);
                if (layerMask >= 0) return new(MaterialTextureUse.BlendDetailMask, layerMask + 1, LoadedElsewhere: true);
            }

            for (var i = 0; i < blendLayers.Count; i++)
            {
                if (blendLayers[i].Albedo == texType) return new(MaterialTextureUse.Albedo, i, LoadedElsewhere: d.Blend != null, FromKnownLayers: true);
                if (blendLayers[i].Normal == texType) return new(MaterialTextureUse.Normal, i, LoadedElsewhere: d.Blend != null, FromKnownLayers: true);
            }

            // Anything else in the role of a layer would take that layer's slot
            if (role is "AlbedoMap" or "NormalMap")
            {
                return MaterialTextureRoute.Drop("a layer the blend doesn't use");
            }
        }

        // DS3's lightmaps, g_DOLTexture and g_DOLTexture2: their textures are the map's, found per part
        if (texType != null && texType.Contains("DOLTexture", StringComparison.OrdinalIgnoreCase))
        {
            return new(MaterialTextureUse.Lightmap, LoadedElsewhere: true);
        }

        // Only what follows the slot says what this is: the rest is the shader's name, which can be M[...]_Emissive_Glow
        var samplerName = texType?[(texType.LastIndexOf("Texture2D_", StringComparison.Ordinal) + 1)..];

        if (samplerName != null && samplerName.Contains("Emissive", StringComparison.OrdinalIgnoreCase))
        {
            return new(MaterialTextureUse.Emissive);
        }

        // Elden Ring and later are metallic: the red of MetallicMap is how much of a metal the surface is, and the
        // shader works F0 out from it and the albedo (c_metallic). Where the layers are known each has its own
        // (MaterialDefinition.ResolveMetallic), otherwise the bottom one
        if (role == "MetallicMap")
        {
            List<MaterialBlendLayer> blendLayers = d!.BlendLayers;
            var perLayer = HasKnownBlend(d) && blendLayers.Exists(l => l.Metallic != null);
            var layer = perLayer ? blendLayers.FindIndex(l => l.Metallic == texType) : texType == metallicType ? 0 : -1;

            if (layer >= 0 && layer < MaxLayers)
            {
                return new(MaterialTextureUse.Metallic, layer, FromKnownLayers: perLayer);
            }

            return MaterialTextureRoute.Drop(perLayer ? "a metallic map no layer pairs with" : "a metallic map of a layer above the bottom one");
        }

        // Matched on the sampler's own name too, as shader names such as M[...][Ov_N_AlbedoMask_MetallicMask] or
        // C[Fur]_Albedo_FallOff would otherwise make every texture of theirs an albedo. Older games' g_ names have
        // no slot, so for them this is the whole name.
        var name = samplerName == null ? "G_DIFFUSE" : samplerName.ToUpperInvariant();

        if (MatchLayer(name, "G_DIFFUSETEXTURE", "G_DIFFUSE", "ALBEDO_", "ALBEDO", out var n)) return new(MaterialTextureUse.Albedo, n);
        if (MatchLayer(name, "G_BUMPMAPTEXTURE", "G_BUMPMAP", "NORMAL_", "NORMAL", out n)) return new(MaterialTextureUse.Normal, n);
        if (MatchLayer(name, "G_SPECULARTEXTURE", "G_SPECULAR", "SPECULAR_", "SPECULAR", out n))
        {
            // These games keep their gloss in the specular slot, which the shader reads as shininess
            return new(glossInSpecular ? MaterialTextureUse.Shininess : MaterialTextureUse.Specular, n);
        }
        if (MatchLayer(name, "G_SHININESSTEXTURE", "G_SHININESS", "SHININESS", "SHININESS", out n)) return new(MaterialTextureUse.Shininess, n);
        if (name.Contains("BLENDMASK")) return new(MaterialTextureUse.BlendMask);

        return MaterialTextureRoute.Drop(role == null ? "no sampler of the material has this type" : $"no route for the role {role}");
    }

    /// <summary>
    /// Works out which layer (0 based) of a kind of texture a parameter name refers to.
    /// Layer 1 (index 0) has no number, the layers after it are numbered 2, 3 and 4. DES and DS1 put an underscore
    /// before the number (g_Diffuse_2, g_Specular_2, g_Bumpmap_2 of their [M] multi texture materials).
    /// </summary>
    public static bool MatchLayer(string name, string exact1, string exact2, string containsPrefix, string containsBase, out int layer)
    {
        for (var n = MaxLayers; n >= 2; n--)
        {
            if (name == exact1 + n || name == exact2 + n || name == exact2 + "_" + n || name.Contains(containsPrefix + n))
            {
                layer = n - 1;
                return true;
            }
        }

        if (name == exact1 || name == exact2 || name.Contains(containsBase))
        {
            layer = 0;
            return true;
        }

        layer = 0;
        return false;
    }
}
