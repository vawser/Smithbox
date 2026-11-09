using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// What a running effect shares with the effects its Proxy nodes start: how to find them, the limits that keep a
/// broken or huge effect from running away, and the counters the limits are checked against.
/// </summary>
public sealed class FxRuntimeContext
{
    /// <summary>The definition of another effect, for Proxy nodes; null if it can't be found.</summary>
    public Func<int, FxEffectDef> ResolveEffect;

    /// <summary>External values by id (0 = Terminate, 1 = precipitation, 2 = time of day, ...).</summary>
    public Func<int, float> External = _ => 0;

    /// <summary>Where the camera is, for LevelsOfDetail nodes.</summary>
    public Vector3 CameraPosition;

    /// <summary>
    /// The top-level effect's world transform at a time, as the game moves an effect (a projectile flying, a weapon
    /// swinging); null keeps it where it is. A function of time, so a replay moves it the same way.
    /// </summary>
    public Func<float, Matrix4x4> Motion;

    /// <summary>
    /// Nodes the editor has hidden: they and everything under them still simulate (so showing them again needs no
    /// replay), but their particles aren't output. Null hides nothing.
    /// </summary>
    public Func<FxrNode, bool> IsHidden;

    public int MaxParticles = 20000;
    public int MaxNodes = 4000;
    public int MaxProxyDepth = 6;
    public int MaxEmissionsPerUpdate = 256;

    // Counted over the whole tree, nested effects included
    public int ParticleCount;
    public int NodeCount;

    /// <summary>Times a limit stopped an emission, for the survey.</summary>
    public int ParticleCapHits, NodeCapHits, ProxyMissing, ProxyDepthHits;
}

/// <summary>
/// One running effect (FX Editor Plan §10.2): its state machine and node tree. <see cref="Update"/> steps it; a
/// replay from time 0 with the same seed gives the same effect, which is how the viewport seeks.
///
/// What's simulated, and the assumptions made where fxr-main's descriptions leave a gap, are listed in plan §13.
/// </summary>
public sealed class FxEffectInstance
{
    public readonly FxEffectDef Def;
    public readonly FxRuntimeContext Context;
    public readonly int Depth;
    public readonly uint Seed;

    public Matrix4x4 World = Matrix4x4.Identity;

    public float Time;
    public int State;
    public float StateTime;

    public bool Terminated;
    public float TerminatedTime;

    /// <summary>True once the effect has terminated and nothing of it is left.</summary>
    public bool Finished;

    public FxNodeInstance Root;

    internal readonly FxRandom Random;

    public FxEffectInstance(FxEffectDef def, FxRuntimeContext context, uint seed = 1, int depth = 0)
    {
        Def = def;
        Context = context;
        Seed = seed;
        Depth = depth;
        Random = new FxRandom(seed);
        Root = new FxNodeInstance(this, null, def.Root, Random.NextUInt());
    }

    public float External(int id) => Context.External?.Invoke(id) ?? 0;

    public void Update(float dt)
    {
        if (Finished || dt <= 0)
            return;

        Time += dt;

        // The effect itself moves as the preview says (effects started by Proxy nodes follow their node instead)
        if (Depth == 0 && Context.Motion != null)
            World = Context.Motion(Time);

        if (Terminated)
        {
            TerminatedTime += dt;
        }
        else
        {
            StateTime += dt;
            UpdateState();
        }

        Root.Update(dt, World);

        if (Terminated && Root.Dead)
            Finished = true;
    }

    /// <summary>Ends the effect, as reaching state -1 does. Each node then follows its Termination action.</summary>
    public void Terminate()
    {
        if (Terminated)
            return;
        Terminated = true;
        TerminatedTime = 0;
    }

    /// <summary>
    /// The current state's conditions, in order; the first that holds moves to its target state (-1 ends the
    /// effect), and the state time starts again. One move per update.
    /// </summary>
    private void UpdateState()
    {
        if (State < 0 || State >= Def.States.Count)
            return;

        foreach (var c in Def.States[State].Conditions)
        {
            float l = Operand(c.Left), r = Operand(c.Right);
            bool holds = c.Operator switch
            {
                0 => l == r,
                1 => l != r,
                2 => l < r,
                3 => l <= r,
                _ => false,
            };
            if (!holds)
                continue;

            if (c.TargetState < 0)
            {
                Terminate();
            }
            else
            {
                State = c.TargetState;
                StateTime = 0;
            }
            return;
        }
    }

    private float Operand(FxrOperand o) => o.Type switch
    {
        -4 => o.FloatValue,
        -3 => External(o.IntValue),
        -1 => StateTime,
        _ => 0, // UnkMinus2: "based on movement in some way"
    };

    /// <summary>Every node instance, nested effects included.</summary>
    public IEnumerable<FxNodeInstance> AllNodes() => Root.Descendants();

    /// <summary>Every live particle, with its world position. Hidden nodes and what's under them are left out.</summary>
    public void CollectParticles(List<FxParticleOutput> output)
    {
        foreach (var node in Root.VisibleDescendants())
        {
            if (node.Config == null || node.Particles.Count == 0)
                continue;
            foreach (var p in node.Particles)
                output.Add(new FxParticleOutput
                {
                    Node = node,
                    Particle = p,
                    Position = p.Local ? Vector3.Transform(p.Position, node.World) : p.Position,
                });
        }
    }

    /// <summary>Steps the effect from its start to <paramref name="time"/> in fixed steps.</summary>
    public void SimulateTo(float time, float step = 1f / 30f)
    {
        while (Time + step <= time && !Finished)
            Update(step);
        if (time - Time > 1e-5f && !Finished)
            Update(time - Time);
    }
}

public struct FxParticleOutput
{
    public FxNodeInstance Node;
    public FxParticle Particle;
    public Vector3 Position;
}

public sealed class FxParticle
{
    /// <summary>In the node's space when <see cref="Local"/>, else in world space.</summary>
    public Vector3 Position;
    public Vector3 Direction;
    public Vector3 Velocity;
    public Vector3 GravityVelocity;
    public bool Local;

    public float Age;
    /// <summary>-1 for a particle that lasts until its node ends.</summary>
    public float Duration;
    public float EmissionTime;
    public uint Seed;

    /// <summary>Tracers: the points left behind, oldest first, in world space; null for particles without one.</summary>
    public List<FxTrailPoint> Trail;
    public float TrailClock;
    public int TrailCount;
    public Vector3? LastWorld;
}

/// <summary>One instance of a node: its timing, transform, emitter state, particles and child instances.</summary>
public sealed class FxNodeInstance
{
    public readonly FxEffectInstance Effect;
    public readonly FxNodeInstance Parent;
    public readonly FxNodeDef Def;
    public readonly uint Seed;

    public FxConfigDef Config { get; private set; }

    public float Age;
    public float ActiveTime => Age - (Config?.Delay ?? 0);

    /// <summary>The node's duration ran out: it stops emitting and moving, and goes once nothing of it is left.</summary>
    public bool Expired;

    /// <summary>The emitter reached its total emissions.</summary>
    public bool EmitterDone;

    public bool Dead;

    /// <summary>The node's active time when its parent emitted it (0 for nodes that aren't emitted).</summary>
    public float EmissionTime;

    /// <summary>For emitted nodes: the emission point and direction (+Z), in the parent's space.</summary>
    public Matrix4x4 SpawnFrame = Matrix4x4.Identity;

    public Matrix4x4 World = Matrix4x4.Identity;

    /// <summary>Opacity while fading out after termination.</summary>
    public float FadeAlpha = 1;

    public readonly List<FxParticle> Particles = new();
    public readonly List<FxNodeInstance> Children = new();
    public FxEffectInstance Proxy;

    /// <summary>LevelsOfDetail: the child that is shown, -1 for none.</summary>
    public int LodChild;

    private readonly FxRandom Random;
    private bool StaticChildrenMade;
    private Matrix4x4 BaseTransform = Matrix4x4.Identity;
    private FxConfigDef BaseFor;
    private float Duration = -1;
    private Matrix4x4? DetachedParent;

    // Movement
    private Vector3 Displacement;
    private float VelocityZ, VelocityY;
    private bool VelocityStarted;
    private Vector3 Spin;

    // Emitter
    private float NextEmission;
    private int Emissions;
    private bool EmittedOnce;
    private Vector3? LastEmitPosition;

    public FxNodeInstance(FxEffectInstance effect, FxNodeInstance parent, FxNodeDef def, uint seed)
    {
        Effect = effect;
        Parent = parent;
        Def = def;
        Seed = seed == 0 ? 1 : seed;
        Random = new FxRandom(Seed);
        effect.Context.NodeCount++;
    }

    public IEnumerable<FxNodeInstance> Descendants()
    {
        yield return this;
        foreach (var child in Children)
            foreach (var n in child.Descendants())
                yield return n;
        if (Proxy != null)
            foreach (var n in Proxy.AllNodes())
                yield return n;
    }

    /// <summary>As <see cref="Descendants"/>, stopping at hidden nodes (see <see cref="FxRuntimeContext.IsHidden"/>).</summary>
    public IEnumerable<FxNodeInstance> VisibleDescendants()
    {
        if (Def.Source != null && Effect.Context.IsHidden?.Invoke(Def.Source) == true)
            yield break;

        yield return this;
        foreach (var child in Children)
            foreach (var n in child.VisibleDescendants())
                yield return n;
        if (Proxy != null)
            foreach (var n in Proxy.Root.VisibleDescendants())
                yield return n;
    }

    private FxEvalContext Ctx(float particleAge = 0, float emissionTime = 0, uint seed = 0) => new()
    {
        ActiveTime = Math.Max(ActiveTime, 0),
        ParticleAge = particleAge,
        EmissionTime = emissionTime,
        Seed = seed == 0 ? Seed : seed,
        External = Effect.External,
    };

    public void Update(float dt, Matrix4x4 parentWorld)
    {
        if (Dead)
            return;

        Age += dt;

        var config = Def.ConfigFor(Effect.State);
        if (config != Config)
        {
            Config = config;
            if (config != null && config != BaseFor)
                SetupConfig(config);
        }

        // Termination
        var termination = Config?.Termination ?? FxTerminationKind.Simulate;
        if (Effect.Terminated)
        {
            if (termination == FxTerminationKind.Instant)
            {
                Kill();
                return;
            }
            if (termination == FxTerminationKind.Fade)
            {
                float fade = Config?.FadeDuration?.Scalar(Ctx()) ?? 1;
                FadeAlpha = fade > 0 ? Math.Clamp(1 - Effect.TerminatedTime / fade, 0, 1) : 0;
                if (FadeAlpha <= 0)
                {
                    Kill();
                    return;
                }
            }
        }

        // A node that lasts forever ends with its parent; one with a duration of its own runs it out (vanilla gives
        // children longer durations than their parents, so those aren't cut short)
        if (!Expired && Parent is { Expired: true } && Duration < 0)
            Expired = true;

        // Nodes with no configs at all (Proxy nodes, which only hold their SFXReference, and the root) are always
        // active; a node whose configs have none for this state is not
        bool active = (Config != null || Def.Configs.Count == 0) && ActiveTime >= 0 && !Expired;

        if (active && Config != null)
            Move(dt);

        UpdateWorld(parentWorld);

        if (Def.Kind == FxNodeKind.Proxy && active)
            UpdateProxy(dt);
        else if (Proxy != null)
        {
            Proxy.World = World;
            if (Expired || Effect.Terminated)
                Proxy.Terminate();
            Proxy.Update(dt);
        }

        bool canEmit = active && Config != null && !Effect.Terminated && !EmitterDone;
        if (canEmit && Config.Emitter != FxEmitterKind.None)
        {
            if (Def.Kind == FxNodeKind.Basic && Config.EmitsParticles)
                Emit(dt, emitNodes: false);
            else if (Def.Kind == FxNodeKind.NodeEmitter)
                Emit(dt, emitNodes: true);
        }

        // The duration ends the node after this update's emissions, so a node lasting a single frame still emits
        // what it emits at its start
        if (active && Duration >= 0 && ActiveTime > Duration)
            Expired = true;

        UpdateParticles(dt);
        UpdateChildren(dt);

        CheckDead();
    }

    private void SetupConfig(FxConfigDef config)
    {
        BaseFor = config;

        // The variance of a RandomNodeTransform is picked once per node instance and config
        var offset = config.Offset + config.OffsetVariance * new Vector3(Random.NextSigned(), Random.NextSigned(), Random.NextSigned());
        var rotation = (config.Rotation + config.RotationVariance * new Vector3(Random.NextSigned(), Random.NextSigned(), Random.NextSigned()))
            * (MathF.PI / 180f);

        // Rotations Z, then X, then Y; the offset after them (the schema's description)
        BaseTransform = Matrix4x4.CreateRotationZ(rotation.Z) * Matrix4x4.CreateRotationX(rotation.X)
            * Matrix4x4.CreateRotationY(rotation.Y) * Matrix4x4.CreateTranslation(offset);

        Duration = config.Duration?.Scalar(Ctx()) ?? -1;
    }

    private void Kill()
    {
        Effect.Context.ParticleCount -= Particles.Count;
        Particles.Clear();
        foreach (var child in Children)
            child.KillTree();
        Children.Clear();
        // A Proxy's effect shares the counters, so its tree goes the same way
        Proxy?.Root.KillTree();
        Proxy = null;
        Dead = true;
    }

    private void KillTree()
    {
        Kill();
        Effect.Context.NodeCount--;
    }

    // ---------------------------------------------------------------- transform and movement

    /// <summary>
    /// The Speed family moves the node along its Z axis at speedZ × multiplier; the Acceleration family starts at
    /// speedZ and accelerates by accelerationZ × multiplier. Both accelerate along Y by accelerationY.
    /// NodeTranslation offsets the node; NodeSpin turns it (degrees per second).
    /// </summary>
    private void Move(float dt)
    {
        var c = Config;
        var ctx = Ctx();

        switch (c.NodeMove)
        {
            case FxNodeMoveKind.Speed:
                VelocityZ = (c.NodeSpeedZ?.Scalar(ctx) ?? 0) * (c.NodeSpeedMultiplierZ?.Scalar(ctx) ?? 1);
                break;
            case FxNodeMoveKind.Acceleration:
                if (!VelocityStarted)
                {
                    VelocityZ = c.NodeSpeedZ?.Scalar(ctx) ?? 0;
                    VelocityStarted = true;
                }
                VelocityZ += (c.NodeAccelerationZ?.Scalar(ctx) ?? 0) * (c.NodeAccelerationMultiplierZ?.Scalar(ctx) ?? 1) * dt;
                break;
        }

        if (c.NodeMove != FxNodeMoveKind.None)
        {
            VelocityY += (c.NodeAccelerationY?.Scalar(ctx) ?? 0) * dt;
            Displacement += new Vector3(0, VelocityY, VelocityZ) * dt;
        }

        if (c.NodeSpin is { } spin)
        {
            float S(int i) => spin[i]?.Scalar(ctx) ?? (i % 2 == 1 ? 1 : 0);
            Spin += new Vector3(S(0) * S(1), S(2) * S(3), S(4) * S(5)) * dt;
        }
    }

    /// <summary>
    /// The node's world transform: spin, then its movement (in its own axes), then its NodeTransform, then the
    /// point it was emitted at, then the parent as its attachment mode says.
    /// </summary>
    private void UpdateWorld(Matrix4x4 parentWorld)
    {
        var translation = Config?.NodeTranslation?.Vector3(Ctx()) ?? Vector3.Zero;
        var spin = Spin * (MathF.PI / 180f);

        var local = Matrix4x4.CreateRotationZ(spin.Z) * Matrix4x4.CreateRotationX(spin.X) * Matrix4x4.CreateRotationY(spin.Y)
            * Matrix4x4.CreateTranslation(Displacement + translation)
            * BaseTransform * SpawnFrame;

        Matrix4x4 parent = (Config?.Attachment ?? 1) switch
        {
            // Detached: the parent's transform when the node was made
            0 => DetachedParent ??= parentWorld,
            // Translation only
            3 or 4 => Matrix4x4.CreateTranslation(parentWorld.Translation),
            // Parent, or a dummy poly (none in the editor), with rotation
            _ => parentWorld,
        };

        World = local * parent;
    }

    private void UpdateProxy(float dt)
    {
        // Nothing new starts once the effect has ended
        if (Proxy == null && Effect.Terminated)
        {
            Expired = true;
            return;
        }

        if (Proxy == null)
        {
            if (Effect.Depth >= Effect.Context.MaxProxyDepth)
            {
                Effect.Context.ProxyDepthHits++;
                return;
            }
            var def = Def.ProxyEffectId >= 0 ? Effect.Context.ResolveEffect?.Invoke(Def.ProxyEffectId) : null;
            if (def == null)
            {
                Effect.Context.ProxyMissing++;
                Expired = true; // nothing to start; don't look it up every frame
                return;
            }
            Proxy = new FxEffectInstance(def, Effect.Context, Random.NextUInt(), Effect.Depth + 1);
        }

        Proxy.World = World;
        // The started effect ends with the one that started it
        if (Effect.Terminated)
            Proxy.Terminate();
        Proxy.Update(dt);
    }

    // ---------------------------------------------------------------- emission

    private int LiveEmitted => Def.Kind == FxNodeKind.NodeEmitter ? Children.Count : Particles.Count;

    private void Emit(float dt, bool emitNodes)
    {
        var c = Config;
        var ctx = Ctx();
        int maxConcurrent = c.MaxConcurrent != null ? (int)MathF.Round(c.MaxConcurrent.Scalar(ctx)) : -1;
        int total = c.TotalEmissions != null ? (int)MathF.Round(c.TotalEmissions.Scalar(ctx)) : -1;

        void Do(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (maxConcurrent >= 0 && LiveEmitted >= maxConcurrent)
                    break;
                if (emitNodes) EmitNode();
                else EmitParticle();
            }
            Emissions++;
            if (total >= 0 && Emissions >= total)
                EmitterDone = true;
        }

        switch (c.Emitter)
        {
            case FxEmitterKind.OneTime:
                if (!EmittedOnce)
                {
                    EmittedOnce = true;
                    Do(1);
                    EmitterDone = true;
                }
                break;

            case FxEmitterKind.Periodic:
            {
                int guard = 0;
                // Emissions are due up to now, but not past the node's duration
                while (!EmitterDone && ActiveTime >= NextEmission && (Duration < 0 || NextEmission <= Duration)
                       && guard++ < Effect.Context.MaxEmissionsPerUpdate)
                {
                    int count = (int)MathF.Round(c.PerEmission?.Scalar(ctx) ?? 1);
                    Do(Math.Max(count, 0));

                    float interval = c.Interval?.Scalar(ctx) ?? 1;
                    if (interval < 0 || float.IsNaN(interval))
                    {
                        // A negative interval (DS3 stores -1 frame) emits once, as a burst: repeating it every
                        // update makes thousands of never-ending nodes in the effects that use it
                        NextEmission = float.PositiveInfinity;
                        break;
                    }
                    if (interval == 0)
                    {
                        // An interval of 0 emits once per update
                        NextEmission = ActiveTime + dt * 0.5f;
                        break;
                    }
                    NextEmission += interval;
                }
                break;
            }

            case FxEmitterKind.EqualDistance:
            {
                var position = World.Translation;
                float threshold = c.Threshold?.Scalar(ctx) ?? 0.1f;
                if (LastEmitPosition == null)
                {
                    LastEmitPosition = position;
                    break;
                }
                if (threshold > 0 && Vector3.Distance(position, LastEmitPosition.Value) >= threshold)
                {
                    LastEmitPosition = position;
                    Do(1);
                }
                break;
            }
        }
    }

    private void EmitParticle()
    {
        var context = Effect.Context;
        if (context.ParticleCount >= context.MaxParticles)
        {
            context.ParticleCapHits++;
            return;
        }

        var c = Config;
        uint seed = Random.NextUInt() | 1;
        float at = Math.Max(ActiveTime, 0);
        var ctx = Ctx(0, at, seed);

        SampleShape(c, ctx, out var localPos, out var localDir, out bool dirIsGlobal);
        var dir = ApplySpread(c, ctx, localDir);

        var p = new FxParticle
        {
            Seed = seed,
            EmissionTime = at,
            Duration = c.ParticleDuration?.Scalar(ctx) ?? -1,
            // Detached particles stay where they were emitted; attached ones move with the node
            Local = c.ParticleAttachment != 0,
        };

        if (p.Local)
        {
            p.Position = localPos;
            p.Direction = dirIsGlobal ? WorldToLocalDirection(dir) : dir;
        }
        else
        {
            p.Position = Vector3.Transform(localPos, World);
            p.Direction = dirIsGlobal ? dir : SafeNormalize(Vector3.TransformNormal(dir, World));
        }

        if (c.ParticleMove == FxParticleMoveKind.Acceleration)
            p.Velocity = p.Direction * (c.ParticleSpeed?.Scalar(ctx) ?? 0);

        Particles.Add(p);
        context.ParticleCount++;
    }

    private void EmitNode()
    {
        var context = Effect.Context;
        var children = Def.Children;
        if (children.Count == 0)
            return;

        var c = Config;
        uint seed = Random.NextUInt() | 1;
        float at = Math.Max(ActiveTime, 0);
        var ctx = Ctx(0, at, seed);

        SampleShape(c, ctx, out var localPos, out var localDir, out bool dirIsGlobal);
        var dir = ApplySpread(c, ctx, dirIsGlobal ? WorldToLocalDirection(localDir) : localDir);

        // The emitted node's +Z is the emission direction
        var frame = LookFrame(dir) * Matrix4x4.CreateTranslation(localPos);

        IEnumerable<FxNodeDef> picks = children;
        if (c.SelectWeights is { Count: > 0 } weights)
            picks = new[] { children[PickWeighted(weights, children.Count)] };

        foreach (var childDef in picks)
        {
            if (context.NodeCount >= context.MaxNodes)
            {
                context.NodeCapHits++;
                return;
            }
            var child = new FxNodeInstance(Effect, this, childDef, Random.NextUInt())
            {
                EmissionTime = at,
                SpawnFrame = frame,
            };
            Children.Add(child);
        }
    }

    private int PickWeighted(List<int> weights, int count)
    {
        int sum = 0;
        for (int i = 0; i < Math.Min(weights.Count, count); i++)
            sum += Math.Max(weights[i], 0);
        if (sum <= 0)
            return 0;
        int r = (int)(Random.Next() * sum);
        for (int i = 0; i < Math.Min(weights.Count, count); i++)
        {
            r -= Math.Max(weights[i], 0);
            if (r < 0)
                return i;
        }
        return 0;
    }

    /// <summary>
    /// A point on the emitter shape and the shape's own direction there (node space). Distribution 0 is uniform,
    /// 1 pulls every point to the centre and -1 pushes it to the edge (a blend in between; the exact curve the
    /// game uses isn't known). <paramref name="dirIsGlobal"/> says the direction is in world space.
    /// </summary>
    private void SampleShape(FxConfigDef c, in FxEvalContext ctx, out Vector3 pos, out Vector3 dir, out bool dirIsGlobal)
    {
        float dist = c.ShapeDistribution?.Scalar(ctx) ?? 0;
        pos = Vector3.Zero;
        var shapeDir = Vector3.UnitZ;
        bool outwards = false;

        switch (c.Shape)
        {
            case FxShapeKind.Disk:
            {
                float r = (c.ShapeRadius?.Scalar(ctx) ?? 0) * Distribute(MathF.Sqrt(Random.Next()), dist);
                float a = Random.Next() * MathF.Tau;
                pos = new Vector3(MathF.Cos(a) * r, MathF.Sin(a) * r, 0);
                break;
            }
            case FxShapeKind.Rectangle:
            {
                float hx = (c.ShapeSizeX?.Scalar(ctx) ?? 0) / 2, hy = (c.ShapeSizeY?.Scalar(ctx) ?? 0) / 2;
                float sx = Random.NextSigned(), sy = Random.NextSigned();
                pos = new Vector3(hx * MathF.Sign(sx) * Distribute(MathF.Abs(sx), dist), hy * MathF.Sign(sy) * Distribute(MathF.Abs(sy), dist), 0);
                break;
            }
            case FxShapeKind.Sphere:
            {
                var n = RandomUnit();
                float r = (c.ShapeRadius?.Scalar(ctx) ?? 0) * (c.EmitInside ? MathF.Cbrt(Random.Next()) : 1);
                pos = n * r;
                shapeDir = n;
                outwards = true;
                break;
            }
            case FxShapeKind.Box:
            {
                var h = new Vector3(c.ShapeSizeX?.Scalar(ctx) ?? 0, c.ShapeSizeY?.Scalar(ctx) ?? 0, c.ShapeSizeZ?.Scalar(ctx) ?? 0) / 2;
                if (c.EmitInside)
                {
                    pos = h * new Vector3(Random.NextSigned(), Random.NextSigned(), Random.NextSigned());
                    // Parallel to a random local axis
                    int axis = (int)(Random.Next() * 3);
                    float sign = Random.Next() < 0.5f ? -1 : 1;
                    shapeDir = axis switch { 0 => Vector3.UnitX, 1 => Vector3.UnitY, _ => Vector3.UnitZ } * sign;
                }
                else
                {
                    // A face by its area, a point on it; the direction is out of that face
                    float ax = h.Y * h.Z, ay = h.X * h.Z, az = h.X * h.Y, sum = ax + ay + az;
                    float pick = Random.Next() * (sum > 0 ? sum : 3);
                    float sign = Random.Next() < 0.5f ? -1 : 1;
                    var u = new Vector3(Random.NextSigned(), Random.NextSigned(), Random.NextSigned());
                    if (sum <= 0 ? pick < 1 : pick < ax) { pos = new Vector3(sign * h.X, u.Y * h.Y, u.Z * h.Z); shapeDir = Vector3.UnitX * sign; }
                    else if (sum <= 0 ? pick < 2 : pick < ax + ay) { pos = new Vector3(u.X * h.X, sign * h.Y, u.Z * h.Z); shapeDir = Vector3.UnitY * sign; }
                    else { pos = new Vector3(u.X * h.X, u.Y * h.Y, sign * h.Z); shapeDir = Vector3.UnitZ * sign; }
                }
                outwards = true;
                break;
            }
            case FxShapeKind.Cylinder:
            {
                float radius = (c.ShapeRadius?.Scalar(ctx) ?? 0) * (c.EmitInside ? MathF.Sqrt(Random.Next()) : 1);
                float half = (c.ShapeHeight?.Scalar(ctx) ?? 0) / 2;
                float a = Random.Next() * MathF.Tau;
                float along = Random.NextSigned() * half;
                var radial = new Vector3(MathF.Cos(a), MathF.Sin(a), 0);
                if (c.CylinderYAxis)
                {
                    radial = new Vector3(radial.X, 0, radial.Y);
                    pos = radial * radius + Vector3.UnitY * along;
                }
                else
                {
                    pos = radial * radius + Vector3.UnitZ * along;
                }
                shapeDir = radial;
                outwards = true;
                break;
            }
        }

        // InitialDirection; a sphere's can't be changed
        dirIsGlobal = false;
        dir = shapeDir;
        if (c.Shape == FxShapeKind.Sphere)
            return;

        switch (c.Direction)
        {
            case 1: dir = Vector3.UnitY; dirIsGlobal = true; break;
            case 2: dir = -Vector3.UnitY; dirIsGlobal = true; break;
            case 3: dir = Vector3.UnitZ; dirIsGlobal = true; break;
            case 4: dir = Vector3.UnitY; break;
            case 5: dir = -Vector3.UnitY; break;
            case 6: dir = Vector3.UnitZ; break;
            default: dir = outwards ? shapeDir : Vector3.UnitZ; break;
        }
    }

    /// <summary>u in [0, 1] (uniform) blended toward 0 (distribution 1) or 1 (distribution -1).</summary>
    private static float Distribute(float u, float distribution)
    {
        if (distribution > 0)
            return u * Math.Max(1 - distribution, 0);
        if (distribution < 0)
            return u + (1 - u) * Math.Min(-distribution, 1);
        return u;
    }

    /// <summary>
    /// DirectionSpread: a random direction in a cone around the default one. Circular and elliptical cones pick an
    /// angle around the axis and a tilt up to the cone's angle there; rectangular ones tilt each axis on its own.
    /// </summary>
    private Vector3 ApplySpread(FxConfigDef c, in FxEvalContext ctx, Vector3 dir)
    {
        if (c.Spread == FxSpreadKind.None)
            return dir;

        float distribution = c.SpreadDistribution?.Scalar(ctx) ?? 0;
        Basis(dir, out var u, out var v);
        const float deg = MathF.PI / 180f;

        switch (c.Spread)
        {
            case FxSpreadKind.Circular:
            case FxSpreadKind.Elliptical:
            {
                float phi = Random.Next() * MathF.Tau;
                float max = c.Spread == FxSpreadKind.Circular
                    ? c.SpreadAngle?.Scalar(ctx) ?? 30
                    : Ellipse(c.SpreadAngleX?.Scalar(ctx) ?? 30, c.SpreadAngleY?.Scalar(ctx) ?? 30, phi);
                float theta = max * deg * Distribute(MathF.Sqrt(Random.Next()), distribution);
                return SafeNormalize(dir * MathF.Cos(theta) + (u * MathF.Cos(phi) + v * MathF.Sin(phi)) * MathF.Sin(theta));
            }
            case FxSpreadKind.Rectangular:
            {
                float sx = Random.NextSigned(), sy = Random.NextSigned();
                float ax = (c.SpreadAngleX?.Scalar(ctx) ?? 30) * MathF.Sign(sx) * Distribute(MathF.Abs(sx), distribution);
                float ay = (c.SpreadAngleY?.Scalar(ctx) ?? 30) * MathF.Sign(sy) * Distribute(MathF.Abs(sy), distribution);
                ax = Math.Clamp(ax, -89, 89) * deg;
                ay = Math.Clamp(ay, -89, 89) * deg;
                return SafeNormalize(dir + u * MathF.Tan(ax) + v * MathF.Tan(ay));
            }
        }
        return dir;
    }

    private static float Ellipse(float a, float b, float phi)
    {
        float ca = MathF.Cos(phi) / Math.Max(a, 1e-4f), sb = MathF.Sin(phi) / Math.Max(b, 1e-4f);
        return 1 / MathF.Sqrt(ca * ca + sb * sb);
    }

    // ---------------------------------------------------------------- particles

    /// <summary>
    /// Particles age and end at their duration. ParticleSpeed moves them at speed × multiplier along their
    /// direction; ParticleAcceleration starts them at the ParticleModifier's speed and accelerates them along their
    /// current direction. Gravity pulls along world -Y for positive values.
    /// </summary>
    private void UpdateParticles(float dt)
    {
        if (Particles.Count == 0)
            return;

        var c = Config;
        var context = Effect.Context;

        // Gravity is along world -Y; attached particles move in the node's space
        Vector3 down = -Vector3.UnitY;
        Vector3 downLocal = WorldToLocalDirection(down);

        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            var p = Particles[i];
            p.Age += dt;

            bool end = p.Duration >= 0 && p.Age >= p.Duration;
            // On termination, SimulateTermination keeps only the particles that end by themselves
            if (Effect.Terminated && p.Duration < 0)
                end = true;
            // Particles that last forever go with their node
            if (Expired && p.Duration < 0)
                end = true;

            if (end)
            {
                Particles.RemoveAt(i);
                context.ParticleCount--;
                continue;
            }

            if (c == null)
                continue;

            var ctx = Ctx(p.Age, p.EmissionTime, p.Seed);
            var g = (p.Local ? downLocal : down) * (c.ParticleGravity?.Scalar(ctx) ?? 0);

            switch (c.ParticleMove)
            {
                case FxParticleMoveKind.Speed:
                {
                    p.GravityVelocity += g * dt;
                    float speed = (c.ParticleMoveSpeed?.Scalar(ctx) ?? 0) * (c.ParticleMoveSpeedMultiplier?.Scalar(ctx) ?? 1);
                    p.Position += (p.Direction * speed + p.GravityVelocity) * dt;
                    break;
                }
                case FxParticleMoveKind.Acceleration:
                {
                    var current = p.Velocity.LengthSquared() > 1e-10f ? Vector3.Normalize(p.Velocity) : p.Direction;
                    float accel = (c.ParticleAcceleration?.Scalar(ctx) ?? 0) * (c.ParticleAccelerationMultiplier?.Scalar(ctx) ?? 1);
                    p.Velocity += (current * accel + g) * dt;
                    p.Position += p.Velocity * dt;
                    break;
                }
            }

            if (float.IsNaN(p.Position.X) || float.IsInfinity(p.Position.X))
                p.Position = Vector3.Zero;

            if (c.Tracer != null)
                RecordTrail(p, dt, c.Tracer);
        }
    }

    /// <summary>
    /// A tracer particle leaves a point every segmentInterval (every update for 0) where it is, in world space, with
    /// the node's Z axis and the direction it came from. Points older than segmentDuration go, and at most
    /// concurrentSegments are kept.
    /// </summary>
    private void RecordTrail(FxParticle p, float dt, FxTracerDef tracer)
    {
        var world = p.Local ? Vector3.Transform(p.Position, World) : p.Position;
        float now = Effect.Time;

        p.Trail ??= new List<FxTrailPoint>();
        p.TrailClock += dt;

        if (tracer.SegmentInterval <= 0 || p.TrailClock >= tracer.SegmentInterval || p.Trail.Count == 0)
        {
            p.TrailClock = 0;
            var travel = p.LastWorld is { } last && Vector3.DistanceSquared(last, world) > 1e-12f
                ? Vector3.Normalize(world - last)
                : Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, World));
            p.Trail.Add(new FxTrailPoint
            {
                Position = world,
                Axis = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, World)),
                Travel = travel,
                Time = now,
                Index = p.TrailCount++,
            });
            p.LastWorld = world;
        }

        int expired = 0;
        while (expired < p.Trail.Count && now - p.Trail[expired].Time > tracer.SegmentDuration)
            expired++;
        int over = p.Trail.Count - expired - tracer.ConcurrentSegments;
        if (over > 0)
            expired += over;
        if (expired > 0)
            p.Trail.RemoveRange(0, Math.Min(expired, p.Trail.Count));
    }

    // ---------------------------------------------------------------- children

    private void UpdateChildren(float dt)
    {
        // Root, Basic and LevelsOfDetail nodes have one instance of each child; NodeEmitter nodes emit theirs
        if (!StaticChildrenMade)
        {
            StaticChildrenMade = true;
            if (Def.Kind != FxNodeKind.NodeEmitter)
            {
                foreach (var childDef in Def.Children)
                {
                    if (Effect.Context.NodeCount >= Effect.Context.MaxNodes)
                    {
                        Effect.Context.NodeCapHits++;
                        break;
                    }
                    Children.Add(new FxNodeInstance(Effect, this, childDef, Random.NextUInt()));
                }
            }
        }

        if (Def.Kind == FxNodeKind.LevelsOfDetail)
            LodChild = PickLod();

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (Def.Kind == FxNodeKind.LevelsOfDetail && i != LodChild)
            {
                // Children out of range don't run, but they end with the effect
                if (Effect.Terminated && !child.Dead)
                    child.Kill();
                continue;
            }

            child.Update(dt, World);

            if (child.Dead)
            {
                // Emitted nodes go; the fixed children stay as dead entries so they aren't made again
                if (Def.Kind == FxNodeKind.NodeEmitter)
                {
                    Children.RemoveAt(i);
                    Effect.Context.NodeCount--;
                }
            }
        }
    }

    /// <summary>The first child whose distance threshold the camera is within, -1 if it's beyond them all.</summary>
    private int PickLod()
    {
        var thresholds = Config?.LodThresholds;
        if (thresholds == null)
            return 0;
        float d = Vector3.Distance(Effect.Context.CameraPosition, World.Translation);
        for (int i = 0; i < Math.Min(thresholds.Length, Children.Count); i++)
            if (d < thresholds[i])
                return i;
        return -1;
    }

    /// <summary>
    /// A node goes once it has ended (duration over, total emissions reached, or the effect terminated) and nothing
    /// of it is left: no particles, no live children, no running Proxy effect.
    /// </summary>
    private void CheckDead()
    {
        // A one-time emitter is done at once, which doesn't end its node; a total of emissions does
        bool ended = Expired || Effect.Terminated
            || EmitterDone && Config?.Emitter is FxEmitterKind.Periodic or FxEmitterKind.EqualDistance;
        if (!ended)
            return;

        if (Particles.Count > 0)
            return;
        // An ended node terminates its Proxy's effect, and waits for what's left of it
        if (Proxy is { Finished: false })
            return;
        foreach (var child in Children)
            if (!child.Dead)
                return;

        // A child that hasn't been made yet (fixed children are made on the first update)
        if (!StaticChildrenMade)
            return;

        Dead = true;
    }

    // ---------------------------------------------------------------- helpers

    private Vector3 WorldToLocalDirection(Vector3 dir)
    {
        return Matrix4x4.Invert(World, out var inv) ? SafeNormalize(Vector3.TransformNormal(dir, inv)) : dir;
    }

    private Vector3 RandomUnit()
    {
        float z = Random.NextSigned();
        float a = Random.Next() * MathF.Tau;
        float r = MathF.Sqrt(Math.Max(0, 1 - z * z));
        return new Vector3(r * MathF.Cos(a), r * MathF.Sin(a), z);
    }

    private static Vector3 SafeNormalize(Vector3 v) => v.LengthSquared() > 1e-12f ? Vector3.Normalize(v) : Vector3.UnitZ;

    private static void Basis(Vector3 dir, out Vector3 u, out Vector3 v)
    {
        var up = MathF.Abs(dir.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
        u = Vector3.Normalize(Vector3.Cross(up, dir));
        v = Vector3.Cross(dir, u);
    }

    /// <summary>A rotation taking +Z to <paramref name="dir"/>.</summary>
    private static Matrix4x4 LookFrame(Vector3 dir)
    {
        dir = SafeNormalize(dir);
        Basis(dir, out var u, out var v);
        return new Matrix4x4(
            u.X, u.Y, u.Z, 0,
            v.X, v.Y, v.Z, 0,
            dir.X, dir.Y, dir.Z, 0,
            0, 0, 0, 1);
    }
}
