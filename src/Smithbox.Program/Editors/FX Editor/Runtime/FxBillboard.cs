using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>The billboard appearance actions the renderer draws (FX Editor Plan, phase V3).</summary>
public enum FxBillboardKind
{
    PointSprite,      // 600
    BillboardEx,      // 603
    MultiTexture,     // 604
    Distortion,       // 607 (phase V5, plan §20.7): bends the scene behind it
    RadialBlur,       // 608: blurs the scene behind it towards its centre
}

/// <summary>
/// A PointSprite, BillboardEx or MultiTextureBillboardEx action read for drawing: its fields, and its properties
/// ready to evaluate per particle (<see cref="Evaluate"/>). Built once per effect definition.
/// </summary>
public sealed class FxBillboardDef
{
    public FxBillboardKind Kind;
    public FxrAction Action;

    // Fields (or properties evaluated once: texture ids and the blend mode are Constant0 properties)
    public int Texture;              // BillboardEx/PointSprite texture, MultiTexture layer 1
    public int Layer2, Layer3;       // MultiTexture
    public int BlendMode = 2;
    public int Orientation = 1;
    public int Columns = 1, TotalFrames = 1;
    public bool InterpolateFrames = true;
    public bool DepthBlend = true;
    public bool UniformScale;
    public bool ScaleBeforeRotation = true;
    public bool PremultiplyAlpha;

    /// <summary>
    /// MultiTextureBillboardEx's layer opacity mode (the schema's unk_ds3_f2_10): 0 every layer's colour is
    /// multiplied and the opacity is layer 1's alpha; 1 layers 2 and 3 lose their colour and their brightness is
    /// opacity; 2 the same for layer 2 only. Mode 1 is how a sigil without an alpha channel (DXT1) is cut out.
    /// </summary>
    public int LayerOpacityMode;
    public bool Octagonal;
    public float ScaleVariationX = 1, ScaleVariationY = 1;
    public float MinFadeDistance = -1, MinDistance = -1, MaxFadeDistance = -1, MaxDistance = -1;
    public float MinDistanceThreshold = -1, MaxDistanceThreshold = -1;

    // Properties
    public FxValue Width, Height, Size;
    public FxValue OffsetX, OffsetY, OffsetZ;
    public FxValue Color1, Color2, Color3, RgbMultiplier, AlphaMultiplier;
    public FxValue AlphaThreshold, AlphaFadeThreshold;
    public FxValue RotationX, RotationY, RotationZ;
    public FxValue[] AngularSpeed; // x, mx, y, my, z, mz
    public FxValue FrameIndex, FrameIndexOffset;
    public FxValue DepthOffset;
    public FxValue[] LayerColor = new FxValue[3];
    public FxValue[] LayerSpeedU = new FxValue[3], LayerSpeedV = new FxValue[3];
    public FxValue[] LayerOffsetU = new FxValue[3], LayerOffsetV = new FxValue[3];
    public FxValue[] LayerScaleU = new FxValue[3], LayerScaleV = new FxValue[3];

    // Distortion and RadialBlur (phase V5). Texture is the mask; Layer2 Distortion's normal map.
    /// <summary>DistortionMode: 0 Stir, 1 NormalMap, 2 Twist, 3 like NormalMap, 4 unknown (drawn as NormalMap).</summary>
    public int DistortionMode = 1;
    /// <summary>DistortionShape: 0 Rectangle, 1 Hemiellipsoid, 2 Ellipsoid (drawn as a camera-facing ellipse, bulging for 1 and 2).</summary>
    public int DistortionShape;
    public int Iterations = 1;
    public FxValue Intensity, Radius, StirSpeed, BlurRadius;

    /// <summary>Whether this draws the scene behind it (Distortion, RadialBlur) rather than a texture.</summary>
    public bool IsScreenEffect => Kind is FxBillboardKind.Distortion or FxBillboardKind.RadialBlur;

    /// <summary>The texture ids this appearance draws with, 0 for none.</summary>
    public IEnumerable<int> TextureIds
    {
        get
        {
            if (Texture > 0) yield return Texture;
            if (Layer2 > 0) yield return Layer2;
            if (Layer3 > 0) yield return Layer3;
        }
    }

    /// <summary>Reads a billboard appearance, null for other appearances or one whose layout doesn't fit.</summary>
    public static FxBillboardDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true)
            return null;

        FxBillboardKind kind;
        switch (appearance.Action.Type)
        {
            case 600: kind = FxBillboardKind.PointSprite; break;
            case 603: kind = FxBillboardKind.BillboardEx; break;
            case 604: kind = FxBillboardKind.MultiTexture; break;
            case 607: kind = FxBillboardKind.Distortion; break;
            case 608: kind = FxBillboardKind.RadialBlur; break;
            default: return null;
        }

        var d = new FxBillboardDef { Kind = kind, Action = appearance.Action };
        var at0 = new FxEvalContext();

        if (d.IsScreenEffect)
            return BuildScreenEffect(d, view, at0);

        d.Texture = IntValue(view, kind == FxBillboardKind.MultiTexture ? "layer1" : "texture", at0, 0);
        d.Layer2 = IntValue(view, "layer2", at0, 0);
        d.Layer3 = IntValue(view, "layer3", at0, 0);
        d.BlendMode = IntValue(view, "blendMode", at0, 2);
        d.Orientation = IntValue(view, "orientation", at0, 1);
        d.Columns = Math.Max(1, IntValue(view, "columns", at0, 1));
        d.TotalFrames = Math.Max(1, IntValue(view, "totalFrames", at0, 1));
        d.InterpolateFrames = IntValue(view, "interpolateFrames", at0, 1) != 0;
        d.DepthBlend = IntValue(view, "depthBlend", at0, 1) != 0;
        d.UniformScale = IntValue(view, "uniformScale", at0, 0) != 0;
        d.ScaleBeforeRotation = IntValue(view, "scaleBeforeRotation", at0, 1) != 0;
        d.PremultiplyAlpha = IntValue(view, "premultiplyAlpha", at0, 0) != 0;
        if (kind == FxBillboardKind.MultiTexture)
            d.LayerOpacityMode = Math.Clamp(IntValue(view, "unk_ds3_f2_10", at0, 0), 0, 2);
        d.Octagonal = IntValue(view, "octagonal", at0, 0) != 0;
        d.ScaleVariationX = FloatValue(view, "scaleVariationX", 1);
        d.ScaleVariationY = FloatValue(view, "scaleVariationY", 1);
        d.MinFadeDistance = FloatValue(view, "minFadeDistance", -1);
        d.MinDistance = FloatValue(view, "minDistance", -1);
        d.MaxFadeDistance = FloatValue(view, "maxFadeDistance", -1);
        d.MaxDistance = FloatValue(view, "maxDistance", -1);
        d.MinDistanceThreshold = FloatValue(view, "minDistanceThreshold", -1);
        d.MaxDistanceThreshold = FloatValue(view, "maxDistanceThreshold", -1);

        d.Width = Prop(view, "width");
        d.Height = Prop(view, "height");
        d.Size = Prop(view, "size");
        d.OffsetX = Prop(view, "offsetX");
        d.OffsetY = Prop(view, "offsetY");
        d.OffsetZ = Prop(view, "offsetZ");
        d.Color1 = Prop(view, "color1");
        d.Color2 = Prop(view, "color2");
        d.Color3 = Prop(view, "color3");
        d.RgbMultiplier = Prop(view, "rgbMultiplier");
        d.AlphaMultiplier = Prop(view, "alphaMultiplier");
        d.AlphaThreshold = Prop(view, "alphaThreshold");
        d.AlphaFadeThreshold = Prop(view, "alphaFadeThreshold");
        d.RotationX = Prop(view, "rotationX");
        d.RotationY = Prop(view, "rotationY");
        d.RotationZ = Prop(view, "rotationZ");
        d.AngularSpeed = new[]
        {
            Prop(view, "angularSpeedX"), Prop(view, "angularSpeedMultiplierX"),
            Prop(view, "angularSpeedY"), Prop(view, "angularSpeedMultiplierY"),
            Prop(view, "angularSpeedZ"), Prop(view, "angularSpeedMultiplierZ"),
        };
        d.FrameIndex = Prop(view, "frameIndex");
        d.FrameIndexOffset = Prop(view, "frameIndexOffset");
        d.DepthOffset = Prop(view, "depthOffset");
        for (int i = 0; i < 3; i++)
        {
            int n = i + 1;
            d.LayerColor[i] = Prop(view, $"layer{n}Color");
            d.LayerSpeedU[i] = Prop(view, $"layer{n}SpeedU");
            d.LayerSpeedV[i] = Prop(view, $"layer{n}SpeedV");
            d.LayerOffsetU[i] = Prop(view, $"layer{n}OffsetU");
            d.LayerOffsetV[i] = Prop(view, $"layer{n}OffsetV");
            d.LayerScaleU[i] = Prop(view, $"layer{n}ScaleU");
            d.LayerScaleV[i] = Prop(view, $"layer{n}ScaleV");
        }

        return d;
    }

    /// <summary>
    /// Distortion (607) and RadialBlur (608): the same quad as a billboard, sized by sizeX/sizeY (Distortion) or
    /// width/height (RadialBlur), coloured by color × the multipliers, with a mask texture; Distortion also has a
    /// normal map with its own uv offset, speed and scale, an intensity, an ellipse radius, a mode and a stir speed;
    /// RadialBlur a blur radius and iterations.
    /// </summary>
    private static FxBillboardDef BuildScreenEffect(FxBillboardDef d, FxrActionView view, FxEvalContext at0)
    {
        bool distortion = d.Kind == FxBillboardKind.Distortion;
        d.Texture = IntValue(view, "mask", at0, 0);
        d.Layer2 = distortion ? IntValue(view, "normalMap", at0, 0) : 0;
        d.BlendMode = 2;
        d.Orientation = distortion ? IntValue(view, "orientation", at0, 1) : 1;
        d.UniformScale = IntValue(view, "uniformScale", at0, 0) != 0;
        d.DepthBlend = true;
        d.InterpolateFrames = false;
        d.DistortionMode = distortion ? Math.Clamp(IntValue(view, "mode", at0, 1), 0, 4) : 0;
        d.DistortionShape = distortion ? Math.Clamp(IntValue(view, "shape", at0, 0), 0, 2) : 0;
        d.Iterations = distortion ? 1 : Math.Clamp(IntValue(view, "iterations", at0, 1), 1, 8);
        d.ScaleVariationX = FloatValue(view, "scaleVariationX", 1);
        d.ScaleVariationY = FloatValue(view, "scaleVariationY", 1);
        d.MinFadeDistance = FloatValue(view, "minFadeDistance", -1);
        d.MinDistance = FloatValue(view, "minDistance", -1);
        d.MaxFadeDistance = FloatValue(view, "maxFadeDistance", -1);
        d.MaxDistance = FloatValue(view, "maxDistance", -1);
        d.MinDistanceThreshold = FloatValue(view, "minDistanceThreshold", -1);
        d.MaxDistanceThreshold = FloatValue(view, "maxDistanceThreshold", -1);

        d.Width = Prop(view, distortion ? "sizeX" : "width");
        d.Height = Prop(view, distortion ? "sizeY" : "height");
        d.OffsetX = Prop(view, "offsetX");
        d.OffsetY = Prop(view, "offsetY");
        d.OffsetZ = Prop(view, "offsetZ");
        d.Color1 = Prop(view, "color");
        d.RgbMultiplier = Prop(view, "rgbMultiplier");
        d.AlphaMultiplier = Prop(view, "alphaMultiplier");
        d.AlphaThreshold = Prop(view, "alphaThreshold");
        d.Intensity = Prop(view, "intensity");
        d.Radius = Prop(view, "radius");
        d.StirSpeed = Prop(view, "stirSpeed");
        d.BlurRadius = Prop(view, "blurRadius");
        d.LayerOffsetU[1] = Prop(view, "normalMapOffsetU");
        d.LayerOffsetV[1] = Prop(view, "normalMapOffsetV");
        d.LayerSpeedU[1] = Prop(view, "normalMapSpeedU");
        d.LayerSpeedV[1] = Prop(view, "normalMapSpeedV");
        d.LayerScaleU[1] = Prop(view, "normalMapScaleU");
        d.LayerScaleV[1] = Prop(view, "normalMapScaleV");
        return d;
    }

    /// <summary>
    /// One particle's billboard values (FX Editor Plan §10.4):
    /// - Colour: color1 and color2 clamped to 0–1, times color3 (unclamped), times the ParticleModifier's colour;
    ///   rgb × rgbMultiplier, alpha × alphaMultiplier and the node's fade.
    /// - Size: width/height (or PointSprite's size), × the ParticleModifier's scale, × a random scale variation
    ///   between the field and 1 per particle.
    /// - Rotation: rotation + angular speed × multiplier × age (the speed taken as constant over the age).
    /// - Frame: frameIndex + frameIndexOffset.
    /// Returns false when the particle draws nothing (zero alpha or size).
    /// </summary>
    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 worldPosition, out FxBillboardInstance b)
    {
        b = default;
        var config = node.Config;
        var ctx = new FxEvalContext
        {
            ActiveTime = Math.Max(node.ActiveTime, 0),
            ParticleAge = p.Age,
            EmissionTime = p.EmissionTime,
            Seed = p.Seed,
            External = node.Effect.External,
        };
        // The ParticleModifier's scale is driven by the emission time; the context's argument picks it
        var emission = ctx;

        // Size
        float w, h;
        if (Kind == FxBillboardKind.PointSprite)
        {
            w = h = Size?.Scalar(ctx) ?? 1;
        }
        else
        {
            w = Width?.Scalar(ctx) ?? 1;
            h = UniformScale ? w : Height?.Scalar(ctx) ?? 1;
        }

        float sx = config?.ParticleScaleX?.Scalar(emission) ?? 1;
        float sy = config?.UniformScale == true ? sx : config?.ParticleScaleY?.Scalar(emission) ?? 1;
        w *= sx;
        h *= sy;

        // Each particle picks a factor between the variation and 1
        if (ScaleVariationX != 1)
            w *= ScaleVariationX + (1 - ScaleVariationX) * FxValue.Unit(p.Seed, 0x51ED27u);
        if (ScaleVariationY != 1)
            h *= ScaleVariationY + (1 - ScaleVariationY) * FxValue.Unit(p.Seed, 0x2545F491u);

        // Colour
        var c = Vector4.Clamp(Color1?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One)
            * Vector4.Clamp(Color2?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One)
            * (Color3?.Vector4(ctx) ?? Vector4.One)
            * (config?.ParticleColor?.Vector4(ctx) ?? Vector4.One);
        float rgb = RgbMultiplier?.Scalar(ctx) ?? 1;
        float alpha = (AlphaMultiplier?.Scalar(ctx) ?? 1) * node.FadeAlpha;
        c = new Vector4(c.X * rgb, c.Y * rgb, c.Z * rgb, c.W * alpha);

        if (c.W <= 0 || w == 0 || h == 0 || float.IsNaN(c.W) || float.IsNaN(w) || float.IsNaN(h))
            return false;

        // Rotation, in degrees
        float Rot(FxValue rotation, int axis)
        {
            float r = rotation?.Scalar(ctx) ?? 0;
            var speed = AngularSpeed?[axis * 2];
            if (speed != null)
                r += speed.Scalar(ctx) * (AngularSpeed[axis * 2 + 1]?.Scalar(ctx) ?? 1) * p.Age;
            return r;
        }

        // The offsets are in the particle's own space; taken along the node's axes
        var offset = new Vector3(OffsetX?.Scalar(ctx) ?? 0, OffsetY?.Scalar(ctx) ?? 0, OffsetZ?.Scalar(ctx) ?? 0);
        var position = worldPosition;
        if (offset != Vector3.Zero)
            position += Vector3.TransformNormal(offset, node.World);

        b = new FxBillboardInstance
        {
            Position = position,
            Size = new Vector2(w, h),
            Rotation = new Vector3(Rot(RotationX, 0), Rot(RotationY, 1), Rot(RotationZ, 2)),
            Frame = (FrameIndex?.Scalar(ctx) ?? 0) + (FrameIndexOffset?.Scalar(ctx) ?? 0),
            Color = c,
            AlphaThreshold = (AlphaThreshold?.Scalar(ctx) ?? 0) / 255f,
            AlphaFadeThreshold = (AlphaFadeThreshold?.Scalar(ctx) ?? 0) / 255f,
            DepthOffset = DepthOffset?.Scalar(ctx) ?? 0,
            NodeRight = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, node.World)),
            NodeUp = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, node.World)),
        };

        if (IsScreenEffect)
        {
            // Layer 1: the mask; layer 2: the normal map, scrolling; layer 3's uv slot carries the effect's own values:
            // intensity, ellipse radius, stir angle (radians: stirSpeed in degrees per second × age), blur radius
            b.SetLayer(0, new Vector4(0, 0, 1, 1), Vector4.One);
            float u = (LayerOffsetU[1]?.Scalar(ctx) ?? 0) + (LayerSpeedU[1]?.Scalar(ctx) ?? 0) * p.Age;
            float v = (LayerOffsetV[1]?.Scalar(ctx) ?? 0) + (LayerSpeedV[1]?.Scalar(ctx) ?? 0) * p.Age;
            b.SetLayer(1, new Vector4(u, v, LayerScaleU[1]?.Scalar(ctx) ?? 1, LayerScaleV[1]?.Scalar(ctx) ?? 1), Vector4.One);
            b.SetLayer(2, new Vector4(
                Intensity?.Scalar(ctx) ?? 1,
                Radius?.Scalar(ctx) ?? 1,
                (StirSpeed?.Scalar(ctx) ?? 60) * p.Age * (MathF.PI / 180f),
                BlurRadius?.Scalar(ctx) ?? 0.5f), Vector4.One);
        }
        else if (Kind == FxBillboardKind.MultiTexture)
        {
            for (int i = 0; i < 3; i++)
            {
                float u = (LayerOffsetU[i]?.Scalar(ctx) ?? 0) + (LayerSpeedU[i]?.Scalar(ctx) ?? 0) * p.Age;
                float v = (LayerOffsetV[i]?.Scalar(ctx) ?? 0) + (LayerSpeedV[i]?.Scalar(ctx) ?? 0) * p.Age;
                b.SetLayer(i, new Vector4(u, v, LayerScaleU[i]?.Scalar(ctx) ?? 1, LayerScaleV[i]?.Scalar(ctx) ?? 1),
                    Vector4.Clamp(LayerColor[i]?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One));
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
                b.SetLayer(i, new Vector4(0, 0, 1, 1), Vector4.One);
        }

        return true;
    }

    // ---------------------------------------------------------------- reading

    private static FxValue Prop(FxrActionView view, string name)
    {
        if (!view.Names.Contains(name) || view.KindOf(name) != FxrValueKind.Property)
            return null;
        return FxValue.From(view, name);
    }

    /// <summary>A field, or a property evaluated at time 0 (texture ids and blend modes are Constant0 properties).</summary>
    private static int IntValue(FxrActionView view, string name, FxEvalContext at0, int fallback)
    {
        if (!view.Names.Contains(name))
            return fallback;
        try
        {
            if (view.KindOf(name) == FxrValueKind.Property)
                return (int)MathF.Round(FxValue.From(view, name)?.Scalar(at0) ?? fallback);
            return (int)view.GetValue(name)[0];
        }
        catch
        {
            return fallback;
        }
    }

    private static float FloatValue(FxrActionView view, string name, float fallback)
    {
        if (!view.Names.Contains(name))
            return fallback;
        try { return (float)view.GetValue(name)[0]; }
        catch { return fallback; }
    }
}

/// <summary>One particle's billboard, ready for the GPU.</summary>
public struct FxBillboardInstance
{
    public Vector3 Position;
    public Vector2 Size;
    /// <summary>Degrees.</summary>
    public Vector3 Rotation;
    public float Frame;
    public Vector4 Color;
    public float AlphaThreshold, AlphaFadeThreshold;
    public float DepthOffset;
    public Vector3 NodeRight, NodeUp;

    /// <summary>Per layer: UV offset (xy) and scale (zw).</summary>
    public Vector4 Layer1Uv, Layer2Uv, Layer3Uv;
    public Vector4 Layer1Color, Layer2Color, Layer3Color;

    public void SetLayer(int i, Vector4 uv, Vector4 color)
    {
        switch (i)
        {
            case 0: Layer1Uv = uv; Layer1Color = color; break;
            case 1: Layer2Uv = uv; Layer2Color = color; break;
            default: Layer3Uv = uv; Layer3Color = color; break;
        }
    }
}
