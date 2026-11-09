using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

// Appearances drawn in phase V4 (FX Editor Plan §10.4): trails, lines, models and lights. Like FxBillboard.cs, these
// only read the action through the schema and evaluate per particle; the renderers live outside Runtime/.

/// <summary>A trail point a tracer particle left behind, in world space.</summary>
public struct FxTrailPoint
{
    public Vector3 Position;
    /// <summary>The node's Z axis when the point was made (LocalZ tracers).</summary>
    public Vector3 Axis;
    /// <summary>The direction the particle moved to reach it.</summary>
    public Vector3 Travel;
    /// <summary>The effect's time when the point was made.</summary>
    public float Time;
    /// <summary>A running count of the particle's points, for world-fixed UVs.</summary>
    public int Index;
}

/// <summary>
/// One quad between two points (A, B) with a half-width vector at each end: a segment of a trail, or a Line or
/// QuadLine particle. Drawn by the billboard renderer in its segment mode.
/// </summary>
public struct FxSegment
{
    public Vector3 A, B, HalfA, HalfB;
    public Vector4 ColorA, ColorB;
    public float U0, U1, V0;
    public float Frame;
    public float AlphaThreshold, AlphaFadeThreshold;
}

/// <summary>Shared reading helpers for the appearance definitions.</summary>
internal static class FxRead
{
    public static FxValue Prop(FxrActionView view, string name) =>
        view.Names.Contains(name) && view.KindOf(name) == FxrValueKind.Property ? FxValue.From(view, name) : null;

    public static int Int(FxrActionView view, string name, int fallback)
    {
        if (!view.Names.Contains(name))
            return fallback;
        try
        {
            if (view.KindOf(name) == FxrValueKind.Property)
                return (int)MathF.Round(FxValue.From(view, name)?.Scalar(default) ?? fallback);
            return (int)view.GetValue(name)[0];
        }
        catch { return fallback; }
    }

    public static float Float(FxrActionView view, string name, float fallback)
    {
        if (!view.Names.Contains(name))
            return fallback;
        try { return (float)view.GetValue(name)[0]; }
        catch { return fallback; }
    }

    public static Vector3 Vec3(FxrActionView view, string name)
    {
        if (!view.Names.Contains(name))
            return default;
        try
        {
            var v = view.GetValue(name);
            return v.Length >= 3 ? new Vector3((float)v[0], (float)v[1], (float)v[2]) : default;
        }
        catch { return default; }
    }

    public static FxEvalContext Ctx(FxNodeInstance node, FxParticle p) => new()
    {
        ActiveTime = Math.Max(node.ActiveTime, 0),
        ParticleAge = p.Age,
        EmissionTime = p.EmissionTime,
        Seed = p.Seed,
        External = node.Effect.External,
    };

    /// <summary>color1 and color2 clamped, × color3, × the ParticleModifier's colour; rgb and alpha multipliers and the node's fade.</summary>
    public static Vector4 Color(in FxEvalContext ctx, FxNodeInstance node, FxValue c1, FxValue c2, FxValue c3, FxValue rgb, FxValue alpha, bool clamp3 = false)
    {
        var c = Vector4.Clamp(c1?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One)
            * Vector4.Clamp(c2?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One);
        var third = c3?.Vector4(ctx) ?? Vector4.One;
        c *= clamp3 ? Vector4.Clamp(third, Vector4.Zero, Vector4.One) : third;
        c *= node.Config?.ParticleColor?.Vector4(ctx) ?? Vector4.One;
        float m = rgb?.Scalar(ctx) ?? 1;
        return new Vector4(c.X * m, c.Y * m, c.Z * m, c.W * (alpha?.Scalar(ctx) ?? 1) * node.FadeAlpha);
    }

    /// <summary>The particle's direction of travel in world space (its velocity, else its initial direction).</summary>
    public static Vector3 Travel(FxNodeInstance node, FxParticle p)
    {
        var v = p.Velocity + p.GravityVelocity;
        if (v.LengthSquared() < 1e-10f) v = p.Direction;
        if (p.Local) v = Vector3.TransformNormal(v, node.World);
        return v.LengthSquared() > 1e-12f ? Vector3.Normalize(v) : Vector3.UnitZ;
    }

    /// <summary>A unit vector across a direction, facing the camera.</summary>
    public static Vector3 Across(Vector3 along, Vector3 toEye)
    {
        var d = Vector3.Cross(along, toEye);
        if (d.LengthSquared() < 1e-10f) d = Vector3.Cross(along, MathF.Abs(along.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX);
        return Vector3.Normalize(d);
    }
}

/// <summary>
/// Tracer (10012) and LegacyTracer (606): a ribbon through the points the particle left behind
/// (<see cref="FxParticle.Trail"/>), up to its current position. Points are made every segmentInterval (every
/// update for 0), last segmentDuration, and at most concurrentSegments are kept.
/// </summary>
public sealed class FxTracerDef
{
    public FxrAction Action;
    public int Texture, BlendMode = 2, Orientation = 1, Columns = 1, TotalFrames = 1, ConcurrentSegments = 100;
    public float SegmentInterval, SegmentDuration = 1, FadeOutTime;
    public bool AttachedUV = true, PremultiplyAlpha;

    public FxValue Width, WidthMultiplier, StartFade, EndFade, Color1, Color2, Color3, Rgb, Alpha;
    public FxValue AlphaThreshold, AlphaFadeThreshold, FrameIndex, FrameIndexOffset, TextureFraction, SpeedU, VarianceV;

    public static FxTracerDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type is not (606 or 10012))
            return null;

        return new FxTracerDef
        {
            Action = appearance.Action,
            Texture = FxRead.Int(view, "texture", 0),
            BlendMode = FxRead.Int(view, "blendMode", 2),
            Orientation = FxRead.Int(view, "orientation", 1),
            Columns = Math.Max(1, FxRead.Int(view, "columns", 1)),
            TotalFrames = Math.Max(1, FxRead.Int(view, "totalFrames", 1)),
            ConcurrentSegments = Math.Clamp(FxRead.Int(view, "concurrentSegments", 100), 1, 1000),
            SegmentInterval = FxRead.Float(view, "segmentInterval", 0),
            SegmentDuration = FxRead.Float(view, "segmentDuration", 1),
            FadeOutTime = FxRead.Float(view, "fadeOutTime", 0),
            AttachedUV = FxRead.Int(view, "attachedUV", 1) != 0,
            PremultiplyAlpha = FxRead.Int(view, "premultiplyAlpha", 0) != 0,
            Width = FxRead.Prop(view, "width"),
            WidthMultiplier = FxRead.Prop(view, "widthMultiplier"),
            StartFade = FxRead.Prop(view, "startFadeEndpoint"),
            EndFade = FxRead.Prop(view, "endFadeEndpoint"),
            Color1 = FxRead.Prop(view, "color1"),
            Color2 = FxRead.Prop(view, "color2"),
            Color3 = FxRead.Prop(view, "color3"),
            Rgb = FxRead.Prop(view, "rgbMultiplier"),
            Alpha = FxRead.Prop(view, "alphaMultiplier"),
            AlphaThreshold = FxRead.Prop(view, "alphaThreshold"),
            AlphaFadeThreshold = FxRead.Prop(view, "alphaFadeThreshold"),
            FrameIndex = FxRead.Prop(view, "frameIndex"),
            FrameIndexOffset = FxRead.Prop(view, "frameIndexOffset"),
            TextureFraction = FxRead.Prop(view, "textureFraction"),
            SpeedU = FxRead.Prop(view, "speedU"),
            VarianceV = FxRead.Prop(view, "varianceV"),
        };
    }

    /// <summary>
    /// The trail's segments, oldest first. Its fraction t along the trail runs from 0 at the oldest point to 1 at
    /// the particle; the start fade (a percentage) thins it toward the oldest end and the end fade toward the
    /// particle. UVs: with attachedUV, textureFraction of the texture spans the whole trail from the particle; without
    /// it, textureFraction per segment, staying with the points where they were made. speedU scrolls them.
    /// </summary>
    public void Segments(FxNodeInstance node, FxParticle p, Vector3 head, Vector3 eye, List<FxSegment> output)
    {
        var trail = p.Trail;
        if (trail == null || trail.Count == 0)
            return;

        var ctx = FxRead.Ctx(node, p);
        float width = (Width?.Scalar(ctx) ?? 1) * (WidthMultiplier?.Scalar(ctx) ?? 1);
        if (width == 0 || float.IsNaN(width))
            return;

        var color = FxRead.Color(ctx, node, Color1, Color2, Color3, Rgb, Alpha);
        if (color.W <= 0)
            return;

        float startFade = (StartFade?.Scalar(ctx) ?? 0) / 100f;
        float endFade = (EndFade?.Scalar(ctx) ?? 0) / 100f;
        float fraction = TextureFraction?.Scalar(ctx) ?? 0.1f;
        float scroll = (SpeedU?.Scalar(ctx) ?? 0) * p.Age;
        float variance = VarianceV?.Scalar(ctx) ?? 0;
        float frame = (FrameIndex?.Scalar(ctx) ?? 0) + (FrameIndexOffset?.Scalar(ctx) ?? 0);
        float threshold = (AlphaThreshold?.Scalar(ctx) ?? 0) / 255f;
        float fadeThreshold = (AlphaFadeThreshold?.Scalar(ctx) ?? 0) / 255f;

        // The points, then the particle itself as the head
        int count = trail.Count + 1;
        Vector3 At(int i) => i < trail.Count ? trail[i].Position : head;
        Vector3 Travel(int i) => i < trail.Count ? trail[i].Travel : FxRead.Travel(node, p);
        Vector3 Axis(int i) => i < trail.Count ? trail[i].Axis : Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, node.World));
        int IndexOf(int i) => i < trail.Count ? trail[i].Index : (trail.Count > 0 ? trail[^1].Index + 1 : 0);

        float Fade(float t)
        {
            float a = 1;
            if (startFade > 0) a *= Math.Clamp(t / startFade, 0, 1);
            if (endFade > 0) a *= Math.Clamp((1 - t) / endFade, 0, 1);
            return a;
        }

        // Attached: textureFraction of the texture spans the whole trail, from the source (0) to the oldest point.
        // Not attached: textureFraction per segment, fixed to the points where they were made.
        float U(int i) => AttachedUV
            ? (count > 1 ? (float)(count - 1 - i) / (count - 1) : 0) * fraction + scroll
            : IndexOf(i) * fraction + scroll;

        // Cross makes two ribbons, one upright and one along X
        int[] modes = Orientation == 4 ? new[] { 2, 3 } : new[] { Orientation };
        foreach (var mode in modes)
        {
            Vector3 Half(int i)
            {
                Vector3 d = mode switch
                {
                    1 => Axis(i),
                    2 => Vector3.UnitY,
                    3 => Vector3.UnitX,
                    5 => Vector3.Normalize(Vector3.One),
                    _ => FxRead.Across(Travel(i), eye - At(i)),
                };
                return d * (width * 0.5f);
            }

            for (int i = 0; i + 1 < count; i++)
            {
                var a = At(i);
                var b = At(i + 1);
                if (Vector3.DistanceSquared(a, b) < 1e-12f)
                    continue;

                float ta = (float)i / (count - 1), tb = (float)(i + 1) / (count - 1);
                output.Add(new FxSegment
                {
                    A = a,
                    B = b,
                    HalfA = Half(i),
                    HalfB = Half(i + 1),
                    ColorA = color with { W = color.W * Fade(ta) },
                    ColorB = color with { W = color.W * Fade(tb) },
                    U0 = U(i),
                    U1 = U(i + 1),
                    V0 = variance != 0 ? variance * FxValue.Signed(p.Seed, (uint)IndexOf(i)) : 0,
                    Frame = frame,
                    AlphaThreshold = threshold,
                    AlphaFadeThreshold = fadeThreshold,
                });
            }
        }
    }
}

/// <summary>
/// Line (601) and QuadLine (602): a strip from the particle back along its direction of travel, length ×
/// multiplier long, turned to face the camera. Line has no width in the file; it is drawn 1.5% of its length wide
/// (at least 5 mm). startColor is the leading end, endColor the trailing one.
/// </summary>
public sealed class FxLineDef
{
    public FxrAction Action;
    public bool Quad;
    public int BlendMode = 2;
    public FxValue Length, LengthMultiplier, Width, WidthMultiplier, Color1, Color2, Color3, StartColor, EndColor, Rgb, Alpha, AlphaThreshold;

    public static FxLineDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type is not (601 or 602))
            return null;

        return new FxLineDef
        {
            Action = appearance.Action,
            Quad = appearance.Action.Type == 602,
            BlendMode = FxRead.Int(view, "blendMode", 2),
            Length = FxRead.Prop(view, "length"),
            LengthMultiplier = FxRead.Prop(view, "lengthMultiplier"),
            Width = FxRead.Prop(view, "width"),
            WidthMultiplier = FxRead.Prop(view, "widthMultiplier"),
            Color1 = FxRead.Prop(view, "color1"),
            Color2 = FxRead.Prop(view, "color2"),
            Color3 = FxRead.Prop(view, "color3"),
            StartColor = FxRead.Prop(view, "startColor"),
            EndColor = FxRead.Prop(view, "endColor"),
            Rgb = FxRead.Prop(view, "rgbMultiplier"),
            Alpha = FxRead.Prop(view, "alphaMultiplier"),
            AlphaThreshold = FxRead.Prop(view, "alphaThreshold"),
        };
    }

    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 position, Vector3 eye, out FxSegment s)
    {
        s = default;
        var ctx = FxRead.Ctx(node, p);
        float length = (Length?.Scalar(ctx) ?? 1) * (LengthMultiplier?.Scalar(ctx) ?? 1);
        if (length == 0 || float.IsNaN(length))
            return false;

        // Lines have no unclamped colour: color3 is clamped too
        var color = FxRead.Color(ctx, node, Color1, Color2, Color3, Rgb, Alpha, clamp3: true);
        if (color.W <= 0)
            return false;

        float width = Quad ? (Width?.Scalar(ctx) ?? 1) * (WidthMultiplier?.Scalar(ctx) ?? 1) : Math.Max(MathF.Abs(length) * 0.015f, 0.005f);
        var dir = FxRead.Travel(node, p);
        var tail = position - dir * length;
        var half = FxRead.Across(dir, eye - position) * (width * 0.5f);

        var start = Vector4.Clamp(StartColor?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One);
        var end = Vector4.Clamp(EndColor?.Vector4(ctx) ?? Vector4.One, Vector4.Zero, Vector4.One);

        // A = the trailing end, B = the particle, so the strip's u runs toward the particle as a trail's does
        s = new FxSegment
        {
            A = tail,
            B = position,
            HalfA = half,
            HalfB = half,
            ColorA = color * end,
            ColorB = color * start,
            U0 = 0,
            U1 = 1,
            AlphaThreshold = (AlphaThreshold?.Scalar(ctx) ?? 0) / 255f,
        };
        return true;
    }
}

/// <summary>A model particle as drawn (FX Editor Plan §18): its transform, colour, uv offset and sprite frame.</summary>
public struct FxModelInstance
{
    public Matrix4x4 World;

    /// <summary>color1 × color2 × color3 × the node's colour, rgb × rgbMultiplier, alpha × alphaMultiplier × fade.</summary>
    public Vector4 Color;

    /// <summary>offsetU/V + speedU/V × speedMultiplierU/V × age.</summary>
    public Vector2 UvOffset;

    public float Frame;
}

/// <summary>
/// Model (605) and RichModel (10015): a FLVER (sfx/model/s&lt;id&gt;.flver) per particle, scaled, rotated (rotation
/// + angular speed × age, Z then X then Y as nodes) and turned by the orientation mode. Model particles are drawn
/// coloured by the effect (blend mode, colours, uv scrolling, sprite frames); RichModel ones with their materials.
/// Animations aren't applied.
/// </summary>
public sealed class FxModelDef
{
    public FxrAction Action;
    public int Model, Orientation = 2;
    public bool UniformScale;
    public float VariationX = 1, VariationY = 1, VariationZ = 1;
    public FxValue SizeX, SizeY, SizeZ, RotationX, RotationY, RotationZ, Color1, Color2, Color3, Rgb, Alpha;
    public FxValue[] AngularSpeed;

    /// <summary>
    /// Model (605) only. A property in the later games (read at the particle's start), a field in DS3
    /// (<see cref="BlendModeValue"/>); Normal (2) by default.
    /// </summary>
    public FxValue BlendMode;
    public int BlendModeValue = 2;
    public FxValue OffsetU, OffsetV, SpeedU, SpeedV, SpeedMultiplierU, SpeedMultiplierV, FrameIndex, FrameIndexOffset;
    public int Columns = 1, TotalFrames = 1;

    /// <summary>Model (605) particles are drawn by the FX model shader, coloured by the effect.</summary>
    public bool IsEffectColoured => Action.Type == 605;

    public static FxModelDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type is not (605 or 10015))
            return null;

        return new FxModelDef
        {
            Action = appearance.Action,
            Model = FxRead.Int(view, "model", 0),
            Orientation = FxRead.Int(view, "orientation", 2),
            UniformScale = FxRead.Int(view, "uniformScale", 0) != 0,
            VariationX = FxRead.Float(view, "scaleVariationX", 1),
            VariationY = FxRead.Float(view, "scaleVariationY", 1),
            VariationZ = FxRead.Float(view, "scaleVariationZ", 1),
            SizeX = FxRead.Prop(view, "sizeX"),
            SizeY = FxRead.Prop(view, "sizeY"),
            SizeZ = FxRead.Prop(view, "sizeZ"),
            RotationX = FxRead.Prop(view, "rotationX"),
            RotationY = FxRead.Prop(view, "rotationY"),
            RotationZ = FxRead.Prop(view, "rotationZ"),
            Color1 = FxRead.Prop(view, "color1"),
            Color2 = FxRead.Prop(view, "color2"),
            Color3 = FxRead.Prop(view, "color3"),
            Rgb = FxRead.Prop(view, "rgbMultiplier"),
            Alpha = FxRead.Prop(view, "alphaMultiplier"),
            AngularSpeed = new[]
            {
                FxRead.Prop(view, "angularSpeedX"), FxRead.Prop(view, "angularSpeedMultiplierX"),
                FxRead.Prop(view, "angularSpeedY"), FxRead.Prop(view, "angularSpeedMultiplierY"),
                FxRead.Prop(view, "angularSpeedZ"), FxRead.Prop(view, "angularSpeedMultiplierZ"),
            },
            BlendMode = appearance.Action.Type == 605 ? FxRead.Prop(view, "blendMode") : null,
            BlendModeValue = appearance.Action.Type == 605 ? FxRead.Int(view, "blendMode", 2) : 2,
            OffsetU = FxRead.Prop(view, "offsetU"),
            OffsetV = FxRead.Prop(view, "offsetV"),
            SpeedU = FxRead.Prop(view, "speedU"),
            SpeedV = FxRead.Prop(view, "speedV"),
            SpeedMultiplierU = FxRead.Prop(view, "speedMultiplierU"),
            SpeedMultiplierV = FxRead.Prop(view, "speedMultiplierV"),
            FrameIndex = FxRead.Prop(view, "frameIndex"),
            FrameIndexOffset = FxRead.Prop(view, "frameIndexOffset"),
            Columns = Math.Max(1, FxRead.Int(view, "columns", 1)),
            TotalFrames = Math.Max(1, FxRead.Int(view, "totalFrames", 1)),
        };
    }

    /// <summary>The blend mode at a particle's start, as the billboards' (Normal 2 when the action has none).</summary>
    public int BlendModeAt(FxNodeInstance node, FxParticle p) =>
        BlendMode == null ? BlendModeValue : (int)MathF.Round(BlendMode.Scalar(FxRead.Ctx(node, p)));

    /// <summary>The model's world transform; false when it is scaled to nothing or fully transparent.</summary>
    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 position, Vector3 eye, Vector3 cameraForward, out Matrix4x4 world)
    {
        bool drawn = EvaluateInstance(node, p, position, eye, cameraForward, out var instance);
        world = instance.World;
        return drawn;
    }

    /// <summary>The model particle as drawn; false when it is scaled to nothing or fully transparent.</summary>
    public bool EvaluateInstance(FxNodeInstance node, FxParticle p, Vector3 position, Vector3 eye, Vector3 cameraForward, out FxModelInstance instance)
    {
        instance = new FxModelInstance { World = Matrix4x4.Identity };
        var ctx = FxRead.Ctx(node, p);

        float sx = SizeX?.Scalar(ctx) ?? 1;
        float sy = UniformScale ? sx : SizeY?.Scalar(ctx) ?? 1;
        float sz = UniformScale ? sx : SizeZ?.Scalar(ctx) ?? 1;
        float ms = node.Config?.ParticleScaleX?.Scalar(ctx) ?? 1;
        sx *= ms * Vary(VariationX, p.Seed, 1u);
        sy *= (node.Config?.UniformScale == true ? ms : node.Config?.ParticleScaleY?.Scalar(ctx) ?? 1) * Vary(VariationY, p.Seed, 2u);
        sz *= (node.Config?.UniformScale == true ? ms : node.Config?.ParticleScaleZ?.Scalar(ctx) ?? 1) * Vary(VariationZ, p.Seed, 3u);
        if (sx == 0 && sy == 0 && sz == 0)
            return false;

        var color = FxRead.Color(ctx, node, Color1, Color2, Color3, Rgb, Alpha);
        if (color.W <= 0)
            return false;

        float Rot(FxValue rotation, int axis)
        {
            float r = rotation?.Scalar(ctx) ?? 0;
            var speed = AngularSpeed[axis * 2];
            if (speed != null)
                r += speed.Scalar(ctx) * (AngularSpeed[axis * 2 + 1]?.Scalar(ctx) ?? 1) * p.Age;
            return r * (MathF.PI / 180f);
        }

        var rotation = Matrix4x4.CreateRotationZ(Rot(RotationZ, 2)) * Matrix4x4.CreateRotationX(Rot(RotationX, 0)) * Matrix4x4.CreateRotationY(Rot(RotationY, 1));

        // The model's +Z points: along the particle (2), with the camera (1), north (0, 3), or toward the camera
        // turning about Y (4) or X (pitch: 3 and 5 for Model)
        var toEye = eye - position;
        Vector3 forward = Orientation switch
        {
            2 => FxRead.Travel(node, p),
            1 => cameraForward,
            4 => Flat(toEye, Vector3.UnitY),
            5 => Flat(toEye, Vector3.UnitX),
            3 when Action.Type == 605 => Flat(toEye, Vector3.UnitX),
            _ => Vector3.UnitZ,
        };

        instance.World = Matrix4x4.CreateScale(sx, sy, sz) * rotation * Look(forward) * Matrix4x4.CreateTranslation(position);
        instance.Color = color;
        instance.UvOffset = new Vector2(
            (OffsetU?.Scalar(ctx) ?? 0) + (SpeedU?.Scalar(ctx) ?? 0) * (SpeedMultiplierU?.Scalar(ctx) ?? 1) * p.Age,
            (OffsetV?.Scalar(ctx) ?? 0) + (SpeedV?.Scalar(ctx) ?? 0) * (SpeedMultiplierV?.Scalar(ctx) ?? 1) * p.Age);
        instance.Frame = (FrameIndex?.Scalar(ctx) ?? 0) + (FrameIndexOffset?.Scalar(ctx) ?? 0);
        return true;
    }

    private static float Vary(float variation, uint seed, uint salt) =>
        variation == 1 ? 1 : variation + (1 - variation) * FxValue.Unit(seed, 0xA511E9B3u * salt);

    /// <summary>A direction with its part along <paramref name="axis"/> taken out.</summary>
    private static Vector3 Flat(Vector3 v, Vector3 axis)
    {
        var f = v - axis * Vector3.Dot(v, axis);
        return f.LengthSquared() > 1e-10f ? Vector3.Normalize(f) : Vector3.UnitZ;
    }

    private static Matrix4x4 Look(Vector3 forward)
    {
        forward = forward.LengthSquared() > 1e-12f ? Vector3.Normalize(forward) : Vector3.UnitZ;
        var up = MathF.Abs(forward.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
        var right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);
        return new Matrix4x4(right.X, right.Y, right.Z, 0, up.X, up.Y, up.Z, 0, forward.X, forward.Y, forward.Z, 0, 0, 0, 0, 1);
    }
}

/// <summary>One light a PointLight or SpotLight particle gives.</summary>
public struct FxLightInstance
{
    public Vector3 Position, Direction, Color;
    public float Intensity, Range, ConeDegrees;
    public bool Spot;
}

/// <summary>
/// PointLight (609) and SpotLight (11000): a light at each particle. The colour is diffuseColor's rgb, the intensity
/// diffuseMultiplier × its alpha. A spot light shines along the node's +Z, its cone from far and the larger of its
/// radii. Jitter, flicker, shadows and the specular colour aren't applied.
/// </summary>
public sealed class FxLightDef
{
    public FxrAction Action;
    public bool Spot;
    public FxValue Diffuse, DiffuseMultiplier, Radius, Far, RadiusX, RadiusY;

    public static FxLightDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true || appearance.Action.Type is not (609 or 11000))
            return null;

        return new FxLightDef
        {
            Action = appearance.Action,
            Spot = appearance.Action.Type == 11000,
            Diffuse = FxRead.Prop(view, "diffuseColor"),
            DiffuseMultiplier = FxRead.Prop(view, "diffuseMultiplier"),
            Radius = FxRead.Prop(view, "radius"),
            Far = FxRead.Prop(view, "far"),
            RadiusX = FxRead.Prop(view, "radiusX"),
            RadiusY = FxRead.Prop(view, "radiusY"),
        };
    }

    public bool Evaluate(FxNodeInstance node, FxParticle p, Vector3 position, out FxLightInstance light)
    {
        var ctx = FxRead.Ctx(node, p);
        var c = Diffuse?.Vector4(ctx) ?? Vector4.One;
        float intensity = (DiffuseMultiplier?.Scalar(ctx) ?? 1) * c.W * node.FadeAlpha;

        light = new FxLightInstance
        {
            Position = position,
            Color = new Vector3(Math.Max(c.X, 0), Math.Max(c.Y, 0), Math.Max(c.Z, 0)),
            Intensity = intensity,
            Spot = Spot,
        };

        if (Spot)
        {
            float far = Math.Max(Far?.Scalar(ctx) ?? 50, 0.01f);
            float radius = Math.Max(RadiusX?.Scalar(ctx) ?? 50, RadiusY?.Scalar(ctx) ?? 50);
            light.Range = far;
            light.ConeDegrees = Math.Clamp(MathF.Atan2(radius, far) * 180f / MathF.PI, 1, 89);
            light.Direction = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, node.World));
        }
        else
        {
            light.Range = Math.Max(Radius?.Scalar(ctx) ?? 10, 0.01f);
        }

        return intensity > 0 && light.Color != Vector3.Zero;
    }
}
