using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The GPU particle actions (phase V5, plan §20.9): GPUStandardParticle (10000), GPUStandardCorrectParticle (10001),
/// GPUSparkParticle (10008) and GPUSparkCorrectParticle (10009). Each is a particle system of its own, with its own
/// emitter, run by the node instance whose config holds it (<see cref="FxGpuSystem"/>), here on the CPU. The standard
/// ones draw billboards (stretched along their motion when traced); the spark ones draw segments.
/// </summary>
public sealed class FxGpuParticleDef
{
    public FxrAction Action;
    public bool Spark;

    // Emitter
    public int Shape = 1;
    public Vector3 EmitterSize, EmitterRotation;
    public float Distribution;
    public int PerEmission = 10, PerEmissionMin, PerEmissionMax;
    public float IntervalMin, IntervalMax;
    public bool LimitEmissions;
    public int EmissionLimit;
    public float Duration = 1;

    // Particles
    public Vector3 Offset, OffsetMin, OffsetMax;
    public Vector3 Speed, SpeedMin, SpeedMax;
    public Vector3 AccelerationMin, AccelerationMax;
    public FxValue AccelerationX, AccelerationY, AccelerationZ, Gravity, FollowFactor;
    public Vector3 RotationVariance, AngularSpeedVariance, AngularAccelerationMin, AngularAccelerationMax;
    public FxValue AngularAccelerationZ;
    public bool UniformScale;
    public float SizeX = 1, SizeY = 1, SizeXMin, SizeXMax, SizeYMin, SizeYMax, SizeMultiplier = 1;
    public FxValue GrowthRateX, GrowthRateY;
    public float GrowthStaticX, GrowthStaticY, GrowthXMin, GrowthXMax, GrowthYMin, GrowthYMax;
    public float GrowthAccelXMin, GrowthAccelXMax, GrowthAccelYMin, GrowthAccelYMax;
    public FxValue Color;
    public Vector4 ColorMin, ColorMax;
    public float RgbMultiplier = 1, AlphaMultiplier = 1, Desaturate;
    public bool RandomTurns;
    public FxValue RandomTurnAngle;
    public float TurnIntervalMax = 1;

    // Drawing
    public int Texture = 1, BlendMode = 2, Columns = 1, TotalFrames = 1, MaxFrameIndex;
    public bool RandomFrame, Trace;
    public float TraceLength = 1;
    public FxValue SparkLength, SparkWidth;
    public float SparkLengthMin, SparkLengthMax, SparkWidthMin, SparkWidthMax;
    public float MinFadeDistance = -1, MinDistance = -1, MaxFadeDistance = -1, MaxDistance = -1;
    public float MinDistanceThreshold = -1, MaxDistanceThreshold = -1;

    /// <summary>The most particles one system keeps in the preview (the game's GPU keeps many more).</summary>
    public const int MaxParticles = 4000;

    /// <summary>A zero emission interval emits every frame, at the game's 30 Hz (vanilla ER, AC6 and NR always have 0).</summary>
    public const float FrameInterval = 1f / 30f;

    public static FxGpuParticleDef Build(FxAppearanceDef appearance)
    {
        var v = appearance.View;
        if (v?.IsTyped != true || appearance.Action.Type is not (10000 or 10001 or 10008 or 10009))
            return null;

        bool spark = appearance.Action.Type is 10008 or 10009;
        float F(string n, float fallback = 0) => FxRead.Float(v, n, fallback);
        int I(string n, int fallback = 0) => FxRead.Int(v, n, fallback);
        Vector3 V(string n) => FxRead.Vec3(v, n);
        Vector4 V4(string n)
        {
            if (!v.Names.Contains(n)) return default;
            try { var x = v.GetValue(n); return x.Length >= 4 ? new Vector4((float)x[0], (float)x[1], (float)x[2], (float)x[3]) : default; }
            catch { return default; }
        }

        float interval = F("emissionInterval", -1);
        var d = new FxGpuParticleDef
        {
            Action = appearance.Action,
            Spark = spark,
            Shape = I("emitterShape", 1),
            EmitterSize = V("emitterSize"),
            EmitterRotation = V("emitterRotation"),
            Distribution = F("emitterDistribution"),
            PerEmission = I("emissionParticleCount", 10),
            PerEmissionMin = I("emissionParticleCountMin"),
            PerEmissionMax = I("emissionParticleCountMax"),
            IntervalMin = interval >= 0 ? interval : F("emissionIntervalMin", 1),
            IntervalMax = interval >= 0 ? interval : F("emissionIntervalMax", 1),
            LimitEmissions = I(spark ? "limitConcurrentEmissions" : "limitEmissionCount") != 0,
            EmissionLimit = I(spark ? "concurrentEmissionsLimit" : "emissionCountLimit"),
            Duration = F("particleDuration", 1) * F("particleDurationMultiplier", 1),
            Offset = V("particleOffset"),
            OffsetMin = V("particleOffsetMin"),
            OffsetMax = V("particleOffsetMax"),
            Speed = V("particleSpeed"),
            SpeedMin = V("particleSpeedMin"),
            SpeedMax = V("particleSpeedMax"),
            AccelerationMin = V("particleAccelerationMin"),
            AccelerationMax = V("particleAccelerationMax"),
            AccelerationX = FxRead.Prop(v, "particleAccelerationX"),
            AccelerationY = FxRead.Prop(v, "particleAccelerationY"),
            AccelerationZ = FxRead.Prop(v, "particleAccelerationZ"),
            Gravity = FxRead.Prop(v, "particleGravity"),
            FollowFactor = FxRead.Prop(v, "particleFollowFactor"),
            RotationVariance = V("particleRotationVariance"),
            AngularSpeedVariance = V("particleAngularSpeedVariance"),
            AngularAccelerationMin = V("particleAngularAccelerationMin"),
            AngularAccelerationMax = V("particleAngularAccelerationMax"),
            AngularAccelerationZ = FxRead.Prop(v, "particleAngularAccelerationZ"),
            UniformScale = I("particleUniformScale") != 0,
            SizeX = F("particleSizeX", 1),
            SizeY = F("particleSizeY", 1),
            SizeXMin = F("particleSizeXMin"),
            SizeXMax = F("particleSizeXMax"),
            SizeYMin = F("particleSizeYMin"),
            SizeYMax = F("particleSizeYMax"),
            SizeMultiplier = F("particleSizeMultiplier", 1),
            GrowthRateX = FxRead.Prop(v, "particleGrowthRateX"),
            GrowthRateY = FxRead.Prop(v, "particleGrowthRateY"),
            GrowthStaticX = F("particleGrowthRateXStatic"),
            GrowthStaticY = F("particleGrowthRateYStatic"),
            GrowthXMin = F("particleGrowthRateXMin"),
            GrowthXMax = F("particleGrowthRateXMax"),
            GrowthYMin = F("particleGrowthRateYMin"),
            GrowthYMax = F("particleGrowthRateYMax"),
            GrowthAccelXMin = F("particleGrowthAccelerationXMin"),
            GrowthAccelXMax = F("particleGrowthAccelerationXMax"),
            GrowthAccelYMin = F("particleGrowthAccelerationYMin"),
            GrowthAccelYMax = F("particleGrowthAccelerationYMax"),
            Color = FxRead.Prop(v, "color"),
            ColorMin = V4("colorMin"),
            ColorMax = V4("colorMax"),
            RgbMultiplier = F("rgbMultiplier", 1),
            AlphaMultiplier = F("alphaMultiplier", 1),
            Desaturate = F("desaturate"),
            RandomTurns = I("particleRandomTurns") != 0,
            RandomTurnAngle = FxRead.Prop(v, "particleRandomTurnAngle"),
            TurnIntervalMax = F("particleRandomTurnIntervalMax", 1),
            Texture = I("texture", 1),
            BlendMode = I("blendMode", 2),
            Columns = Math.Max(1, I("columns", 1)),
            TotalFrames = Math.Max(1, I("totalFrames", 1)),
            MaxFrameIndex = I("maxFrameIndex"),
            RandomFrame = I("randomTextureFrame") != 0,
            Trace = I("traceParticles") != 0,
            TraceLength = F("particleTraceLength", 1),
            SparkLength = FxRead.Prop(v, "particleLength"),
            SparkWidth = FxRead.Prop(v, "particleWidth"),
            SparkLengthMin = F("particleLengthMin"),
            SparkLengthMax = F("particleLengthMax"),
            SparkWidthMin = F("particleWidthMin"),
            SparkWidthMax = F("particleWidthMax"),
            MinFadeDistance = F("minFadeDistance", -1),
            MinDistance = F("minDistance", -1),
            MaxFadeDistance = F("maxFadeDistance", -1),
            MaxDistance = F("maxDistance", -1),
            MinDistanceThreshold = F("minDistanceThreshold", -1),
            MaxDistanceThreshold = F("maxDistanceThreshold", -1),
        };
        return d;
    }

    /// <summary>
    /// An emission point in the emitter's space (EmitterShape): 0 and 3 Line from the origin to the size vector,
    /// distribution the share near the origin left out; 1 Box (centred), distribution the inner box left out; 2 Box2
    /// (centred), distribution 0 drawing points to the centre, 1 even; 4 Cylinder (radius x, height y, upright),
    /// distribution the inner radius left out; others as Box.
    /// </summary>
    public Vector3 EmissionPoint(FxRandom r)
    {
        var size = EmitterSize;
        float dist = Math.Clamp(Distribution, 0, 1);
        switch (Shape)
        {
            case 0:
            case 3:
                return size * (dist + (1 - dist) * r.Next());
            case 2:
            {
                var u = new Vector3(r.NextSigned(), r.NextSigned(), r.NextSigned());
                var biased = u * Vector3.Abs(u);
                return Vector3.Lerp(biased, u, dist) * size * 0.5f;
            }
            case 4:
            {
                float a = r.Next() * MathF.Tau;
                float radius = size.X * MathF.Sqrt(dist * dist + (1 - dist * dist) * r.Next());
                return new Vector3(MathF.Cos(a) * radius, r.NextSigned() * size.Y * 0.5f, MathF.Sin(a) * radius);
            }
            default:
            {
                // Rejection from the inner box, a few tries
                var u = Vector3.Zero;
                for (int i = 0; i < 8; i++)
                {
                    u = new Vector3(r.NextSigned(), r.NextSigned(), r.NextSigned());
                    if (dist <= 0 || MathF.Max(MathF.Abs(u.X), MathF.Max(MathF.Abs(u.Y), MathF.Abs(u.Z))) >= dist)
                        break;
                }
                return u * size * 0.5f;
            }
        }
    }
}

/// <summary>One GPU particle, in world space (or the node's, while it follows the node).</summary>
public struct FxGpuParticle
{
    public Vector3 Position, Velocity, Acceleration;
    public float Age, Duration;
    public Vector2 Size, Growth, GrowthAcceleration;
    public float Rotation, AngularSpeed, AngularAcceleration;
    public Vector4 ColorOffset;
    public float Frame;
    public float NextTurn;
    public uint Seed;
    public float SparkLength, SparkWidth;
}

/// <summary>
/// A running GPU particle system: emits while its node is active and the effect isn't terminated, moves its particles
/// (speed + random, acceleration properties + random, gravity along world -Y, random turns), grows, spins and ages
/// them. Deterministic from its seed, so a replay matches.
/// </summary>
public sealed class FxGpuSystem
{
    public readonly FxGpuParticleDef Def;
    public readonly List<FxGpuParticle> Particles = new();
    private readonly FxRandom Random;
    private float NextEmission;
    private int Emissions;

    /// <summary>Particles not made because the system was at <see cref="FxGpuParticleDef.MaxParticles"/>.</summary>
    public int Dropped;

    public FxGpuSystem(FxGpuParticleDef def, uint seed)
    {
        Def = def;
        Random = new FxRandom(seed == 0 ? 1 : seed);
    }

    private float Range(float min, float max) => min + (max - min) * Random.Next();
    private Vector3 Range(Vector3 min, Vector3 max) => new(Range(min.X, max.X), Range(min.Y, max.Y), Range(min.Z, max.Z));

    public void Update(float dt, FxNodeInstance node, bool emit, in FxEvalContext ctx)
    {
        var d = Def;
        var world = node.World;
        var rotation = world with { M41 = 0, M42 = 0, M43 = 0 };
        var emitterRotation = Matrix4x4.CreateRotationZ(d.EmitterRotation.Z * (MathF.PI / 180f))
            * Matrix4x4.CreateRotationX(d.EmitterRotation.X * (MathF.PI / 180f))
            * Matrix4x4.CreateRotationY(d.EmitterRotation.Y * (MathF.PI / 180f));

        // Emission: every interval (a random one between min and max; 0 = every frame), until the limit
        if (emit && !(d.LimitEmissions && Emissions >= d.EmissionLimit))
        {
            NextEmission -= dt;
            int guard = 0;
            while (NextEmission <= 0 && guard++ < 8)
            {
                Emit(world, rotation, emitterRotation, ctx);
                Emissions++;
                float interval = Range(d.IntervalMin, d.IntervalMax);
                NextEmission += interval > 0 ? interval : FxGpuParticleDef.FrameInterval;
                if (d.LimitEmissions && Emissions >= d.EmissionLimit)
                    break;
            }
        }

        // Movement
        var gravity = new Vector3(0, -(d.Gravity?.Scalar(ctx) ?? 0), 0);
        var accel = new Vector3(d.AccelerationX?.Scalar(ctx) ?? 0, d.AccelerationY?.Scalar(ctx) ?? 0, d.AccelerationZ?.Scalar(ctx) ?? 0);
        var accelWorld = Vector3.TransformNormal(accel, rotation);
        float angular = d.AngularAccelerationZ?.Scalar(ctx) ?? 0;
        float turn = d.RandomTurns ? (d.RandomTurnAngle?.Scalar(ctx) ?? 0) * (MathF.PI / 180f) : 0;

        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            var p = Particles[i];
            p.Age += dt;
            if (p.Age >= p.Duration)
            {
                Particles.RemoveAt(i);
                continue;
            }

            if (turn > 0 && p.Age >= p.NextTurn)
            {
                var axis = RandomUnit();
                var q = Quaternion.CreateFromAxisAngle(axis, Random.NextSigned() * turn);
                p.Velocity = Vector3.Transform(p.Velocity, q);
                p.Acceleration = Vector3.Transform(p.Acceleration, q);
                p.NextTurn = p.Age + Math.Max(Range(0, d.TurnIntervalMax), FxGpuParticleDef.FrameInterval);
            }

            p.Velocity += (p.Acceleration + accelWorld + gravity) * dt;
            p.Position += p.Velocity * dt;
            p.Size += (p.Growth + p.GrowthAcceleration * p.Age) * dt;
            p.AngularSpeed += (p.AngularAcceleration + angular) * dt;
            p.Rotation += p.AngularSpeed * dt;

            if (!float.IsFinite(p.Position.X) || !float.IsFinite(p.Position.Y) || !float.IsFinite(p.Position.Z))
            {
                Particles.RemoveAt(i);
                continue;
            }
            Particles[i] = p;
        }
    }

    private void Emit(Matrix4x4 world, Matrix4x4 rotation, Matrix4x4 emitterRotation, in FxEvalContext ctx)
    {
        var d = Def;
        int count = d.PerEmission + (int)MathF.Round(Range(d.PerEmissionMin, d.PerEmissionMax));
        for (int n = 0; n < count; n++)
        {
            if (Particles.Count >= FxGpuParticleDef.MaxParticles)
            {
                Dropped += count - n;
                return;
            }

            var local = Vector3.Transform(d.EmissionPoint(Random), emitterRotation) + d.Offset + Range(d.OffsetMin, d.OffsetMax);
            var velocity = Vector3.TransformNormal(Vector3.TransformNormal(d.Speed + Range(d.SpeedMin, d.SpeedMax), emitterRotation), rotation);
            float sx = (d.SizeX + Range(d.SizeXMin, d.SizeXMax)) * d.SizeMultiplier;
            float sy = d.UniformScale ? sx : (d.SizeY + Range(d.SizeYMin, d.SizeYMax)) * d.SizeMultiplier;
            float gx = (d.GrowthRateX?.Scalar(ctx) ?? 0) + d.GrowthStaticX + Range(d.GrowthXMin, d.GrowthXMax);
            float gy = d.UniformScale ? gx : (d.GrowthRateY?.Scalar(ctx) ?? 0) + d.GrowthStaticY + Range(d.GrowthYMin, d.GrowthYMax);
            int frames = d.RandomFrame ? Math.Clamp(d.MaxFrameIndex > 0 ? d.MaxFrameIndex + 1 : d.TotalFrames, 1, d.TotalFrames) : 1;

            Particles.Add(new FxGpuParticle
            {
                Position = Vector3.Transform(local, world),
                Velocity = velocity,
                Acceleration = Vector3.TransformNormal(Range(d.AccelerationMin, d.AccelerationMax), rotation),
                Duration = Math.Max(d.Duration, FxGpuParticleDef.FrameInterval),
                Size = new Vector2(sx, sy),
                Growth = new Vector2(gx, gy),
                GrowthAcceleration = new Vector2(Range(d.GrowthAccelXMin, d.GrowthAccelXMax), d.UniformScale ? 0 : Range(d.GrowthAccelYMin, d.GrowthAccelYMax)),
                Rotation = Random.NextSigned() * d.RotationVariance.Z,
                AngularSpeed = Random.NextSigned() * d.AngularSpeedVariance.Z,
                AngularAcceleration = Range(d.AngularAccelerationMin.Z, d.AngularAccelerationMax.Z),
                ColorOffset = new Vector4(Range(d.ColorMin.X, d.ColorMax.X), Range(d.ColorMin.Y, d.ColorMax.Y), Range(d.ColorMin.Z, d.ColorMax.Z), Range(d.ColorMin.W, d.ColorMax.W)),
                Frame = d.RandomFrame ? MathF.Floor(Random.Next() * frames) : 0,
                NextTurn = Range(0, d.TurnIntervalMax),
                Seed = Random.NextUInt(),
                SparkLength = (d.SparkLength?.Scalar(ctx) ?? 1) + Range(d.SparkLengthMin, d.SparkLengthMax),
                SparkWidth = (d.SparkWidth?.Scalar(ctx) ?? 0.1f) + Range(d.SparkWidthMin, d.SparkWidthMax),
            });
        }
    }

    private Vector3 RandomUnit()
    {
        for (int i = 0; i < 8; i++)
        {
            var v = new Vector3(Random.NextSigned(), Random.NextSigned(), Random.NextSigned());
            float l = v.LengthSquared();
            if (l > 1e-4f && l <= 1)
                return v / MathF.Sqrt(l);
        }
        return Vector3.UnitY;
    }

    /// <summary>
    /// A particle's colour: the colour property at its age, plus the random offset fading out over its life, × the
    /// rgb and alpha multipliers and the node's fade, then desaturated.
    /// </summary>
    public Vector4 ColorOf(in FxGpuParticle p, FxNodeInstance node)
    {
        var d = Def;
        var ctx = new FxEvalContext
        {
            ActiveTime = Math.Max(node.ActiveTime, 0),
            ParticleAge = p.Age,
            Seed = p.Seed,
            External = node.Effect.External,
        };
        float life = p.Duration > 0 ? Math.Clamp(p.Age / p.Duration, 0, 1) : 0;
        var c = (d.Color?.Vector4(ctx) ?? Vector4.One) + p.ColorOffset * (1 - life);
        var rgb = new Vector3(c.X, c.Y, c.Z) * d.RgbMultiplier;
        if (d.Desaturate > 0)
        {
            float grey = Vector3.Dot(rgb, new Vector3(0.299f, 0.587f, 0.114f));
            rgb = Vector3.Lerp(rgb, new Vector3(grey), Math.Clamp(d.Desaturate, 0, 1));
        }
        return new Vector4(rgb, c.W * d.AlphaMultiplier * node.FadeAlpha);
    }

    /// <summary>A particle's frame: its random one, or played through over its life.</summary>
    public float FrameOf(in FxGpuParticle p)
    {
        if (Def.RandomFrame || Def.TotalFrames <= 1)
            return p.Frame;
        float life = p.Duration > 0 ? Math.Clamp(p.Age / p.Duration, 0, 0.9999f) : 0;
        return MathF.Floor(life * Def.TotalFrames);
    }
}
