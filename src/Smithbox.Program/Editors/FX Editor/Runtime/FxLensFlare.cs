using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// LensFlare (10014), phase V5 (plan §20.8): up to four textured layers of sprites around a source. Each layer draws
/// `count` flares, sized in world units at the source (vanilla's sizes scale with sourceSize), coloured by the
/// layer's colour (clamped) × its colour multiplier, and moved on screen along the reflection vector (Radial: through
/// the screen centre; Horizontal: mirrored left-right) by offset × a random factor between offsetVariation and 1.
/// The renderer fades them by how much of the source sphere can be seen and by the attenuation radius.
/// Like lights and forces, a flare is placed by the instances its config emits.
/// </summary>
public sealed class FxLensFlareDef
{
    public sealed class Layer
    {
        public int Texture;
        public int Count = 1;
        public FxValue Width, Height, Color;
        public Vector4 ColorMultiplier = Vector4.One;
        public bool UniformScale;
        public float ScaleVariationX = 1, ScaleVariationY = 1;
        public int Reflection;
        public float Offset, OffsetVariation = 1, AttenuationRadius = -1;
    }

    public FxrAction Action;
    public int BlendMode = 4;
    public float SourceSize = 1;
    public readonly Layer[] Layers = new Layer[4];

    /// <summary>The most flares drawn per layer and instance (vanilla goes up to 9).</summary>
    public const int MaxCount = 16;

    public IEnumerable<int> TextureIds => Layers.Where(l => l != null && l.Texture > 0).Select(l => l.Texture);

    public static FxLensFlareDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type != 10014)
            return null;

        var d = new FxLensFlareDef
        {
            Action = appearance.Action,
            BlendMode = FxRead.Int(view, "blendMode", 4),
            SourceSize = FxRead.Float(view, "sourceSize", 1),
        };
        for (int i = 0; i < 4; i++)
        {
            int n = i + 1;
            int texture = FxRead.Int(view, $"layer{n}", n == 1 ? 1 : 0);
            if (texture <= 0)
                continue;
            var mult = view.Names.Contains($"layer{n}ColorMultiplier") ? view.GetValue($"layer{n}ColorMultiplier") : null;
            d.Layers[i] = new Layer
            {
                Texture = texture,
                Count = Math.Clamp(FxRead.Int(view, $"layer{n}Count", 1), 0, MaxCount),
                Width = FxRead.Prop(view, $"layer{n}Width"),
                Height = FxRead.Prop(view, $"layer{n}Height"),
                Color = FxRead.Prop(view, $"layer{n}Color"),
                ColorMultiplier = mult is { Length: >= 4 } ? new Vector4((float)mult[0], (float)mult[1], (float)mult[2], (float)mult[3]) : Vector4.One,
                UniformScale = FxRead.Int(view, $"layer{n}UniformScale", 0) != 0,
                ScaleVariationX = FxRead.Float(view, $"layer{n}ScaleVariationX", 1),
                ScaleVariationY = FxRead.Float(view, $"layer{n}ScaleVariationY", 1),
                Reflection = Math.Clamp(FxRead.Int(view, $"layer{n}Reflection", 0), 0, 2),
                Offset = FxRead.Float(view, $"layer{n}Offset", 0),
                OffsetVariation = FxRead.Float(view, $"layer{n}OffsetVariation", 1),
                AttenuationRadius = FxRead.Float(view, $"layer{n}AttenuationRadius", -1),
            };
        }
        return d;
    }

    /// <summary>
    /// The <paramref name="index"/>th flare of a layer for one instance; false when it draws nothing. The random
    /// scale and offset factors are fixed per instance, layer and flare, so a replay matches.
    /// </summary>
    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 position, int layer, int index, out FxLensFlareInstance f)
    {
        f = default;
        var l = Layers[layer];
        if (l == null || index >= l.Count)
            return false;

        var ctx = FxRead.Ctx(node, p);
        float w = l.Width?.Scalar(ctx) ?? 1;
        float h = l.UniformScale ? w : l.Height?.Scalar(ctx) ?? 1;
        uint key = p.Seed ^ (uint)(layer * 0x9E3779B1) ^ (uint)(index * 0x85EBCA77);
        if (l.ScaleVariationX != 1)
            w *= l.ScaleVariationX + (1 - l.ScaleVariationX) * FxValue.Unit(key, 0x1B873593u);
        if (l.ScaleVariationY != 1)
            h *= (l.UniformScale ? l.ScaleVariationX : l.ScaleVariationY) + (1 - (l.UniformScale ? l.ScaleVariationX : l.ScaleVariationY))
                * FxValue.Unit(key, l.UniformScale ? 0x1B873593u : 0xCC9E2D51u);

        var color = Vector4.Clamp(l.Color?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One) * l.ColorMultiplier;
        color.W *= node.FadeAlpha;
        if (color.W <= 0 || w == 0 || h == 0 || float.IsNaN(color.W) || float.IsNaN(w) || float.IsNaN(h))
            return false;

        float offset = l.Offset * (l.OffsetVariation + (1 - l.OffsetVariation) * FxValue.Unit(key, 0xE6546B64u));

        f = new FxLensFlareInstance
        {
            Position = position,
            Size = new Vector2(w, h),
            Color = color,
            Offset = l.Reflection == 0 ? 0 : offset,
            Reflection = l.Reflection,
            // Each flare's own radius is the layer's divided by its offset; none for a flare at the source
            AttenuationRadius = l.AttenuationRadius > 0 && MathF.Abs(offset) > 1e-4f && l.Reflection != 0
                ? l.AttenuationRadius / MathF.Abs(offset) : -1,
            SourceSize = SourceSize,
        };
        return true;
    }
}

/// <summary>One lens flare sprite, ready for the GPU.</summary>
public struct FxLensFlareInstance
{
    public Vector3 Position;
    public Vector2 Size;
    public Vector4 Color;
    public float Offset;
    public int Reflection;
    public float AttenuationRadius;
    public float SourceSize;
}

/// <summary>
/// LightShaft (10003, DS3 only; fxr-main: "a simple light shafts effect", which later games ignore), phase V5 (plan
/// §20.10). Almost nothing about it is documented, so it's drawn as an approximation through the lens flare path:
/// `layers` copies of its texture (or a soft spot) at the source, each larger than the last by the unknown field
/// unk_ds3_f1_4 read as a spread, coloured along color1 → color2 → color3 (vanilla's color3 alpha is near 0, so the
/// outer layers fade out), the total opacity shared out between the layers and weighted by unk_ds3_f1_3. Drawn over
/// the scene and faded by how much of the source can be seen, like a flare.
/// </summary>
public sealed class FxLightShaftDef
{
    public FxrAction Action;
    public int Texture;
    public int BlendMode = 4;
    public int Layers = 30;
    public float Spread = 1, Weight = 1;
    public FxValue Width, Height, Color1, Color2, Color3;

    /// <summary>The most layers drawn (vanilla uses 35 and 200).</summary>
    public const int MaxLayers = 64;

    public static FxLightShaftDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type != 10003)
            return null;
        return new FxLightShaftDef
        {
            Action = appearance.Action,
            Texture = FxRead.Int(view, "texture", 0),
            BlendMode = FxRead.Int(view, "blendMode", 4),
            Layers = Math.Clamp(FxRead.Int(view, "layers", 30), 1, MaxLayers),
            Spread = Math.Max(FxRead.Float(view, "unk_ds3_f1_4", 1), 0),
            Weight = Math.Clamp(FxRead.Float(view, "unk_ds3_f1_3", 1), 0, 4),
            Width = FxRead.Prop(view, "width"),
            Height = FxRead.Prop(view, "height"),
            Color1 = FxRead.Prop(view, "color1"),
            Color2 = FxRead.Prop(view, "color2"),
            Color3 = FxRead.Prop(view, "color3"),
        };
    }

    /// <summary>One layer of the shaft as a flare at the source; false when it draws nothing.</summary>
    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 position, int layer, out FxLensFlareInstance f)
    {
        f = default;
        var ctx = FxRead.Ctx(node, p);
        float t = Layers > 1 ? layer / (float)(Layers - 1) : 0;
        var c1 = Color1?.Vector4(ctx) ?? Vector4.One;
        var c2 = Color2?.Vector4(ctx) ?? Vector4.One;
        var c3 = Color3?.Vector4(ctx) ?? Vector4.One;
        var color = t < 0.5f ? Vector4.Lerp(c1, c2, t * 2) : Vector4.Lerp(c2, c3, t * 2 - 1);
        color = Vector4.Clamp(color, Vector4.Zero, Vector4.One);
        // The layers add up: each gets a share, so the whole has about the colours' opacity × the weight
        color.W *= Weight * 2f / Layers * node.FadeAlpha;

        float scale = 1 + t * Spread;
        float w = (Width?.Scalar(ctx) ?? 1) * scale;
        float h = (Height?.Scalar(ctx) ?? 1) * scale;
        if (color.W <= 0 || w == 0 || h == 0 || float.IsNaN(color.W) || float.IsNaN(w) || float.IsNaN(h))
            return false;

        f = new FxLensFlareInstance
        {
            Position = position,
            Size = new Vector2(w, h),
            Color = color,
            Offset = 0,
            Reflection = 0,
            AttenuationRadius = -1,
            // The visibility test looks at a disc a quarter of the shaft's size
            SourceSize = MathF.Min(MathF.Abs(Width?.Scalar(ctx) ?? 1), MathF.Abs(Height?.Scalar(ctx) ?? 1)) * 0.25f,
        };
        return true;
    }
}
