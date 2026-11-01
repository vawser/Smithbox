using System;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     What the wall stands in for, as bullets treat each kind of surface differently.
/// </summary>
public enum ImpactSurface
{
    Map,
    Character,
    Water
}

/// <summary>
///     The way that the first bullet is pointing. In-game this comes from the casting animation,
///     which the rows know nothing about, and all of a bullet's angles are relative to it.
/// </summary>
public enum CastOrientation
{
    Forward,
    Down,
    Up
}

public enum ProjectileOrigin
{
    Root,
    Chain,
    Interval
}

public enum ProjectileEnd
{
    Expired,
    HitWall,
    HitGround,
    HitTarget,
    TimeLimit
}

/// <summary>
///     Reasons that a simulation doesn't match the row exactly.
/// </summary>
[Flags]
public enum SimulationWarning
{
    None = 0,
    ProjectileLimit = 1,
    TimeLimit = 2,
    MissingRow = 4,
    SelfReference = 8,
    UndocumentedCondition = 16,
    AbsorbedCondition = 32
}

/// <param name="LaunchHeight">The height above the ground that the first bullet is fired from.</param>
/// <param name="Orientation">The way that the first bullet is pointing.</param>
/// <param name="GroundCollision">Whether bullets land on the ground, rather than falling through it.</param>
/// <param name="SimulateWall">Whether there is a wall.</param>
/// <param name="WallDistance">How far in front of the shooter the wall stands.</param>
/// <param name="WallWidth">The width of the wall, which is centered on the line of fire.</param>
/// <param name="WallHeight">The height of the wall, which stands on the ground.</param>
/// <param name="Surface">What the ground and wall are made of. The target always counts as a character.</param>
/// <param name="SimulateTarget">Whether there is a target, which is what homing bullets need.</param>
/// <param name="TargetPosition">The center of the target.</param>
/// <param name="TargetRadius">The radius of the target.</param>
/// <param name="Seed">Rows can vary their timing and aim, so this keeps a simulation repeatable.</param>
public sealed record SimulationSettings(
    float LaunchHeight,
    CastOrientation Orientation,
    bool GroundCollision,
    bool SimulateWall,
    float WallDistance,
    float WallWidth,
    float WallHeight,
    ImpactSurface Surface,
    bool SimulateTarget,
    Vector3 TargetPosition,
    float TargetRadius,
    int Seed);

public readonly record struct TrackSample(float Time, Vector3 Position);

/// <summary>
///     A bullet landing a hit on the target.
/// </summary>
public readonly record struct DamageEvent(float Time, int Track, AttackDefinition Attack);

/// <summary>
///     The recorded flight of a single bullet.
/// </summary>
public sealed class ProjectileTrack
{
    public int RowId { get; init; }
    public string Name { get; init; }
    public ProjectileOrigin Origin { get; init; }
    public int Parent { get; init; }
    public int Generation { get; init; }
    public float Radius { get; init; }
    public float RadiusMax { get; init; }
    public float SpreadTime { get; init; }
    public AttackDefinition Attack { get; init; }

    public float SpawnTime { get; init; }
    public float EndTime { get; set; }
    public ProjectileEnd End { get; set; }

    /// <summary>
    ///     True if the bullet ended without creating the bullets chained to it.
    /// </summary>
    public bool ChainWithheld { get; set; }

    public List<TrackSample> Samples { get; } = new();

    public bool IsActive(float time)
    {
        return time >= SpawnTime && time <= EndTime;
    }

    public float GetRadius(float time)
    {
        if (SpreadTime <= 0)
        {
            return RadiusMax;
        }

        return float.Lerp(Radius, RadiusMax, Math.Clamp((time - SpawnTime) / SpreadTime, 0, 1));
    }

    public Vector3 GetPosition(float time)
    {
        if (Samples.Count < 2)
        {
            return Samples[0].Position;
        }

        // Samples are evenly spaced except for the last, so this lands on the right one or just before it
        var after = Math.Clamp((int)((time - SpawnTime) / ProjectileSimulation.SampleTime), 1, Samples.Count - 1);

        while (after < Samples.Count - 1 && Samples[after].Time < time)
        {
            after++;
        }

        TrackSample from = Samples[after - 1];
        TrackSample to = Samples[after];

        var span = to.Time - from.Time;

        return span > 0
            ? Vector3.Lerp(from.Position, to.Position, Math.Clamp((time - from.Time) / span, 0, 1))
            : to.Position;
    }
}

public sealed class SimulationResult
{
    public List<ProjectileTrack> Tracks { get; } = new();
    public List<DamageEvent> Damage { get; } = new();
    public float Duration { get; set; }
    public SimulationWarning Warnings { get; set; }
    public BulletIgnoredBehavior Ignored { get; set; }
    public Vector3 BoundsMin { get; set; }
    public Vector3 BoundsMax { get; set; }
}

/// <summary>
///     Flies a bullet and everything it creates, recording the lot so that it can be played back afterwards.
///     Bullets fly along the +Z axis from above the origin, with Y being up. This isn't the game's own code: the
///     params only describe bullets, so how the fields combine is worked out from how the game's data uses them.
/// </summary>
public sealed class ProjectileSimulation
{
    public const float DurationLimit = 60.0f;
    public const int ProjectileLimit = 512;

    private const float StepTime = 1.0f / 60.0f;
    private const int StepsPerSample = 2;

    public const float SampleTime = StepTime * StepsPerSample;

    private const float Epsilon = 0.0001f;
    private const float VerticalLimit = 0.999f;

    // A bullet stopped by the target sits at the very edge of its reach
    private const float ReachTolerance = 0.01f;

    private readonly Func<int, BulletDefinition> Lookup;
    private readonly SimulationSettings Settings;
    private readonly Random Rng;

    private readonly SimulationResult Result = new();
    private readonly Dictionary<int, BulletDefinition> Definitions = new();
    private readonly List<ActiveShot> Active = new();
    private readonly PriorityQueue<PendingShot, float> Pending = new();

    private Vector3 BoundsMin;
    private Vector3 BoundsMax;

    private float Time;
    private int Step;

    private sealed record PendingShot(
        BulletDefinition Definition,
        Vector3 Position,
        Vector3 Direction,
        float? Speed,
        ProjectileOrigin Origin,
        int Parent,
        int Generation);

    private sealed class ActiveShot
    {
        public BulletDefinition Definition;
        public ProjectileTrack Track;
        public int Index;
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Drift;
        public Vector3 Heading;
        public float Speed;
        public float Fall;
        public float Age;
        public float Travelled;
        public float Life;
        public float NextInterval;
        public int IntervalCount;
        public Vector3 AimPoint;
        public bool HomingStopped;
        public float NextDamageTime;
    }

    /// <summary>
    ///     Something a bullet ran into while moving.
    /// </summary>
    /// <param name="Progress">How far along the move it happened, from 0 to 1.</param>
    /// <param name="End">What the bullet hit.</param>
    /// <param name="Surface">What the thing it hit counts as.</param>
    /// <param name="Normal">The direction that the surface it hit is facing.</param>
    private readonly record struct Impact(float Progress, ProjectileEnd End, ImpactSurface Surface, Vector3 Normal);

    private ProjectileSimulation(Func<int, BulletDefinition> lookup, SimulationSettings settings)
    {
        Lookup = lookup;
        Settings = settings;
        Rng = new Random(settings.Seed);
    }

    /// <param name="roots">The bullets to fire together, by row ID.</param>
    /// <param name="lookup">Finds a bullet by its row ID, returning null if there is no such row.</param>
    /// <param name="settings">The setup to fire them in.</param>
    public static SimulationResult Run(IReadOnlyList<int> roots, Func<int, BulletDefinition> lookup,
        SimulationSettings settings)
    {
        return new ProjectileSimulation(lookup, settings).Run(roots);
    }

    private SimulationResult Run(IReadOnlyList<int> roots)
    {
        Vector3 origin = new(0, Settings.LaunchHeight, 0);

        BoundsMin = BoundsMax = origin;

        Vector3 forward = Settings.Orientation switch
        {
            CastOrientation.Down => -Vector3.UnitY,
            CastOrientation.Up => Vector3.UnitY,
            _ => Vector3.UnitZ
        };

        foreach (var id in roots)
        {
            if (GetDefinition(id) is { } root)
            {
                Fire(root, 0, origin, forward, null, ProjectileOrigin.Root, -1, 0);
            }
            else
            {
                Result.Warnings |= SimulationWarning.MissingRow;
            }
        }

        while ((Active.Count > 0 || Pending.Count > 0) && Time < DurationLimit)
        {
            SpawnDueShots();

            Time += StepTime;
            Step++;

            // Spawning waits for the next step, so that this can't grow while it runs
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                if (!Advance(Active[i]))
                {
                    Active.RemoveAt(i);
                }
            }
        }

        if (Active.Count > 0 || Pending.Count > 0)
        {
            Result.Warnings |= SimulationWarning.TimeLimit;
        }

        foreach (ActiveShot shot in Active)
        {
            EndTrack(shot, ProjectileEnd.TimeLimit);
        }

        if (Settings.SimulateWall)
        {
            Include(new Vector3(-Settings.WallWidth / 2, 0, Settings.WallDistance));
            Include(new Vector3(Settings.WallWidth / 2, Settings.WallHeight, Settings.WallDistance));
        }

        if (Settings.SimulateTarget)
        {
            Include(Settings.TargetPosition - new Vector3(Settings.TargetRadius));
            Include(Settings.TargetPosition + new Vector3(Settings.TargetRadius));
        }

        Result.BoundsMin = BoundsMin;
        Result.BoundsMax = BoundsMax;
        Result.Duration = Result.Tracks.Count > 0 ? Result.Tracks.Max(track => track.EndTime) : 0;

        return Result;
    }

    private void Fire(BulletDefinition definition, float time, Vector3 position, Vector3 forward, float? speed,
        ProjectileOrigin origin, int parent, int generation)
    {
        Result.Ignored |= Settings.SimulateTarget ? definition.Ignored & ~BulletIgnoredBehavior.Homing : definition.Ignored;

        for (var volley = 0; volley < definition.VolleyCount; volley++)
        {
            for (var shot = 0; shot < definition.ShotsPerVolley; shot++)
            {
                if (Result.Tracks.Count + Pending.Count >= ProjectileLimit)
                {
                    Result.Warnings |= SimulationWarning.ProjectileLimit;
                    return;
                }

                var angleY = definition.AngleY + volley * definition.VolleyAngleY + shot * definition.ShotAngleY +
                             Random(definition.AngleYRandom);
                var angleX = definition.AngleX + volley * definition.VolleyAngleX + shot * definition.ShotAngleX +
                             Random(definition.AngleXRandom);

                Vector3 direction = RotateFrom(forward, float.DegreesToRadians(angleY), float.DegreesToRadians(angleX));

                Pending.Enqueue(new PendingShot(definition, position, direction, speed, origin, parent, generation),
                    time + volley * definition.VolleyInterval);
            }
        }
    }

    private void SpawnDueShots()
    {
        while (Pending.TryPeek(out PendingShot shot, out var due) && due <= Time + Epsilon)
        {
            Pending.Dequeue();

            BulletDefinition definition = shot.Definition;

            ProjectileTrack track = new()
            {
                RowId = definition.RowId,
                Name = definition.Name,
                Origin = shot.Origin,
                Parent = shot.Parent,
                Generation = shot.Generation,
                Radius = definition.Radius,
                RadiusMax = definition.RadiusMax,
                SpreadTime = definition.SpreadTime,
                Attack = definition.Attack,
                SpawnTime = Time
            };

            track.Samples.Add(new TrackSample(Time, shot.Position));

            Result.Tracks.Add(track);
            Include(shot.Position);

            Active.Add(new ActiveShot
            {
                Definition = definition,
                Track = track,
                Index = Result.Tracks.Count - 1,
                Position = shot.Position,
                Direction = shot.Direction,
                Heading = shot.Direction,
                Speed = shot.Speed ?? definition.InitialSpeed,
                Life = definition.Life < 0 ? float.MaxValue : definition.Life + Random(definition.LifeRandom),
                NextInterval = definition.IntervalWait + GetInterval(definition),
                AimPoint = Settings.TargetPosition + Vector3.UnitY * definition.HomingHeightOffset +
                           new Vector3(Random(definition.HomingOffset), Random(definition.HomingOffset),
                               Random(definition.HomingOffset))
            });
        }
    }

    /// <returns>False once the bullet has ended.</returns>
    private bool Advance(ActiveShot shot)
    {
        BulletDefinition definition = shot.Definition;

        shot.Age += StepTime;

        Home(shot);

        var inRange = shot.Travelled < definition.Range;
        var accel = inRange ? definition.AccelInRange : definition.AccelOutRange;
        var gravity = inRange ? definition.GravityInRange : definition.GravityOutRange;

        if (shot.Age >= definition.AccelStartTime && shot.Age < definition.AccelEndTime)
        {
            shot.Speed += accel * StepTime;

            // Rows can start outside of these limits, as they only matter once the speed is changing
            if (accel > 0 && definition.MaxSpeed > 0)
            {
                shot.Speed = Math.Min(shot.Speed, definition.MaxSpeed);
            }
            else if (accel < 0)
            {
                shot.Speed = Math.Max(shot.Speed, definition.MinSpeed);
            }
        }

        // Rows with their speed locked fly straight, whatever their gravity says. Vacuum Slice and
        // Founding Rain of Stars have a gravity of 999, which is what ground-seeking bullets use
        var speedLocked = definition.MinSpeed > 0 && definition.MinSpeed == definition.MaxSpeed;

        if (shot.Age >= definition.GravityStartTime && !speedLocked)
        {
            shot.Fall += gravity * StepTime;
        }

        Vector3 flat = new(shot.Direction.X, 0, shot.Direction.Z);

        if (flat.LengthSquared() > Epsilon)
        {
            shot.Drift += Vector3.Normalize(flat) * definition.ExternalForce * StepTime;
        }

        Vector3 velocity = shot.Direction * shot.Speed + shot.Drift - Vector3.UnitY * shot.Fall;
        Vector3 next = shot.Position + velocity * StepTime;

        if (velocity.LengthSquared() > Epsilon)
        {
            shot.Heading = Vector3.Normalize(velocity);
        }

        var radius = shot.Track.GetRadius(Time);

        if (FindImpact(definition, shot.Position, next, radius) is { } impact)
        {
            Vector3 contact = Vector3.Lerp(shot.Position, next, impact.Progress);

            shot.Position = contact;
            Include(contact);
            shot.Track.Samples.Add(new TrackSample(Time, contact));

            DealDamage(shot, radius);

            EndTrack(shot, impact.End);
            Chain(shot, impact);

            return false;
        }

        shot.Travelled += Vector3.Distance(shot.Position, next);
        shot.Position = next;

        Include(next);

        DealDamage(shot, radius);

        if (shot.Age + Epsilon >= shot.Life)
        {
            shot.Track.Samples.Add(new TrackSample(Time, next));

            EndTrack(shot, ProjectileEnd.Expired);
            Chain(shot, null);

            return false;
        }

        if (Step % StepsPerSample == 0)
        {
            shot.Track.Samples.Add(new TrackSample(Time, next));
        }

        Emit(shot);

        return true;
    }

    /// <summary>
    ///     Anything in reach counts, as an explosion can hurt the target without moving towards it.
    /// </summary>
    private void DealDamage(ActiveShot shot, float radius)
    {
        AttackDefinition attack = shot.Definition.Attack;

        if (!Settings.SimulateTarget || attack == null || Time < shot.NextDamageTime)
        {
            return;
        }

        var reach = GetReach(shot.Definition, radius) + Settings.TargetRadius + ReachTolerance;

        if (Vector3.Distance(shot.Position, Settings.TargetPosition) > reach)
        {
            return;
        }

        Result.Damage.Add(new DamageEvent(Time, shot.Index, attack));

        var interval = shot.Definition.DamageInterval;

        shot.NextDamageTime = interval > 0 ? Time + interval : float.MaxValue;
    }

    private static float GetReach(BulletDefinition definition, float radius)
    {
        return MathF.Max(radius, definition.Attack?.Radius ?? 0);
    }

    /// <summary>
    ///     Turns the bullet towards the target. Left and right turn separately from up and down, as rows can
    ///     limit them differently. Gravity is left out of this, so a heavy bullet can drop short of what it aims at.
    /// </summary>
    private void Home(ActiveShot shot)
    {
        BulletDefinition definition = shot.Definition;

        if (!Settings.SimulateTarget || shot.HomingStopped || shot.Age < definition.HomingBeginTime ||
            shot.Travelled < definition.HomingBeginDistance ||
            definition.HomingRateY <= 0 && definition.HomingRateX <= 0)
        {
            return;
        }

        Vector3 toTarget = shot.AimPoint - shot.Position;
        var distance = toTarget.Length();

        if (distance < Epsilon || distance <= definition.HomingStopRange ||
            definition.HomingEndDistance > 0 && shot.Travelled >= definition.HomingEndDistance)
        {
            shot.HomingStopped = true;
            return;
        }

        Vector3 wanted = toTarget / distance;

        if (definition.HomingStopAngle > 0 &&
            float.RadiansToDegrees(MathF.Acos(Math.Clamp(Vector3.Dot(shot.Direction, wanted), -1, 1))) > definition.HomingStopAngle)
        {
            shot.HomingStopped = true;
            return;
        }

        var yaw = MathF.Atan2(shot.Direction.X, shot.Direction.Z);
        var pitch = MathF.Asin(Math.Clamp(shot.Direction.Y, -1, 1));

        var yawChange = MathF.IEEERemainder(MathF.Atan2(wanted.X, wanted.Z) - yaw, MathF.Tau);
        var pitchChange = MathF.Asin(Math.Clamp(wanted.Y, -1, 1)) - pitch;

        var maxYaw = float.DegreesToRadians(definition.HomingRateY) * StepTime;
        var maxPitch = float.DegreesToRadians(definition.HomingRateX) * StepTime;

        shot.Direction = GetDirection(yaw + Math.Clamp(yawChange, -maxYaw, maxYaw),
            pitch + Math.Clamp(pitchChange, -maxPitch, maxPitch));
    }

    private void Emit(ActiveShot shot)
    {
        BulletDefinition definition = shot.Definition;

        // A maximum of 0 is what turns this off
        if (definition.IntervalChildren.Count == 0 || definition.IntervalMax <= 0)
        {
            return;
        }

        while (shot.Age >= shot.NextInterval)
        {
            if (definition.IntervalLimit > 0 && shot.IntervalCount >= definition.IntervalLimit)
            {
                return;
            }

            shot.IntervalCount++;
            shot.NextInterval += Math.Max(GetInterval(definition), StepTime);

            FireChildren(shot, definition.IntervalChildren, Time, ProjectileOrigin.Interval, null);
        }
    }

    private void Chain(ActiveShot shot, Impact? impact)
    {
        BulletDefinition definition = shot.Definition;

        if (definition.ChainChildren.Count == 0)
        {
            return;
        }

        if (definition.ChainCondition is BulletChainCondition.Undocumented)
        {
            Result.Warnings |= SimulationWarning.UndocumentedCondition;
        }
        else if (definition.ChainCondition is BulletChainCondition.Absorbed)
        {
            Result.Warnings |= SimulationWarning.AbsorbedCondition;
        }

        if (!AllowsChain(definition.ChainCondition, impact?.Surface))
        {
            shot.Track.ChainWithheld = true;
            return;
        }

        FireChildren(shot, definition.ChainChildren, Time + (impact != null ? definition.ChainDelay : 0),
            ProjectileOrigin.Chain, impact?.Normal);
    }

    private void FireChildren(ActiveShot shot, IReadOnlyList<int> children, float time, ProjectileOrigin origin,
        Vector3? impactNormal)
    {
        BulletDefinition definition = shot.Definition;

        foreach (var id in children)
        {
            // A lot of rows name themselves here, so it can't be something that the game acts on
            if (id == definition.RowId)
            {
                Result.Warnings |= SimulationWarning.SelfReference;
                continue;
            }

            BulletDefinition child = GetDefinition(id);

            if (child == null)
            {
                Result.Warnings |= SimulationWarning.MissingRow;
                continue;
            }

            Fire(child, time, shot.Position, GetChildForward(shot, child, impactNormal),
                definition.ChildInheritsSpeed ? shot.Speed : null, origin, shot.Index, shot.Track.Generation + 1);
        }
    }

    private static Vector3 GetChildForward(ActiveShot shot, BulletDefinition child, Vector3? impactNormal)
    {
        if (child.ChildAimedFromOwner)
        {
            return Vector3.UnitZ;
        }

        switch (shot.Definition.ChildPosture)
        {
            case BulletChildPosture.Horizontal:
                Vector3 flat = new(shot.Heading.X, 0, shot.Heading.Z);

                return flat.LengthSquared() > Epsilon ? Vector3.Normalize(flat) : Vector3.UnitZ;

            case BulletChildPosture.Impact when impactNormal != null:
                return impactNormal.Value;

            default:
                return shot.Heading;
        }
    }

    private BulletDefinition GetDefinition(int id)
    {
        if (!Definitions.TryGetValue(id, out BulletDefinition definition))
        {
            definition = Lookup(id);
            Definitions.Add(id, definition);
        }

        return definition;
    }

    /// <summary>
    ///     Whether a bullet that has just ended creates its chained bullets.
    /// </summary>
    /// <param name="condition">The condition of the bullet.</param>
    /// <param name="surface">What the bullet hit, or null if it didn't hit anything.</param>
    public static bool AllowsChain(BulletChainCondition condition, ImpactSurface? surface)
    {
        return condition switch
        {
            BulletChainCondition.Water or BulletChainCondition.WaterOrSwamp => surface is ImpactSurface.Water,
            BulletChainCondition.NotCharacter => surface is not ImpactSurface.Character,

            // Documented as only being for hitting a character, but Elden Ring mostly uses it for bullets with
            // several stages of flight, such as Star Shower, where the next stage takes over when one runs out
            BulletChainCondition.CharacterOnly => surface is null or ImpactSurface.Character,

            BulletChainCondition.NoImpact => surface == null,
            BulletChainCondition.ImpactOnly => surface != null,
            BulletChainCondition.Absorbed => false,
            _ => true
        };
    }

    private static bool Penetrates(BulletDefinition definition, ImpactSurface surface)
    {
        return surface switch
        {
            ImpactSurface.Map => definition.PenetratesMap,
            ImpactSurface.Character => definition.PenetratesCharacter,
            _ => false
        };
    }

    /// <summary>
    ///     Finds the first thing that a move runs into, ignoring anything that the bullet passes through.
    ///     A stationary bullet never hits anything, so that an explosion can sit against what created it.
    /// </summary>
    private Impact? FindImpact(BulletDefinition definition, Vector3 from, Vector3 to, float radius)
    {
        Impact? first = null;

        foreach (Impact? candidate in new[]
                 {
                     HitGround(from, to, radius),
                     HitWall(from, to, radius),
                     HitTarget(from, to, GetReach(definition, radius))
                 })
        {
            if (candidate is { } impact && !Penetrates(definition, impact.Surface) &&
                (first == null || impact.Progress < first.Value.Progress))
            {
                first = impact;
            }
        }

        return first;
    }

    private Impact? HitGround(Vector3 from, Vector3 to, float radius)
    {
        if (!Settings.GroundCollision || to.Y >= from.Y || from.Y < 0 || to.Y - radius > 0)
        {
            return null;
        }

        var progress = Math.Clamp((from.Y - radius) / (from.Y - to.Y), 0, 1);

        return new Impact(progress, ProjectileEnd.HitGround, Settings.Surface, Vector3.UnitY);
    }

    private Impact? HitWall(Vector3 from, Vector3 to, float radius)
    {
        if (!Settings.SimulateWall || to.Z <= from.Z || from.Z > Settings.WallDistance ||
            to.Z + radius < Settings.WallDistance)
        {
            return null;
        }

        var progress = Math.Clamp((Settings.WallDistance - radius - from.Z) / (to.Z - from.Z), 0, 1);

        Vector3 contact = Vector3.Lerp(from, to, progress);

        if (MathF.Abs(contact.X) > Settings.WallWidth / 2 || contact.Y < 0 || contact.Y > Settings.WallHeight)
        {
            return null;
        }

        return new Impact(progress, ProjectileEnd.HitWall, Settings.Surface, -Vector3.UnitZ);
    }

    private Impact? HitTarget(Vector3 from, Vector3 to, float radius)
    {
        if (!Settings.SimulateTarget)
        {
            return null;
        }

        // Where a sphere moving in a line first touches another sphere, as a quadratic in progress
        Vector3 move = to - from;
        Vector3 offset = from - Settings.TargetPosition;

        var reach = radius + Settings.TargetRadius;
        var a = move.LengthSquared();
        var b = Vector3.Dot(offset, move);
        var c = offset.LengthSquared() - reach * reach;

        // Moving away, which includes the bullets created by a bullet that has just hit the target
        if (a <= 0 || b >= 0)
        {
            return null;
        }

        var progress = 0.0f;

        if (c > 0)
        {
            var discriminant = b * b - a * c;

            if (discriminant < 0)
            {
                return null;
            }

            progress = (-b - MathF.Sqrt(discriminant)) / a;

            if (progress > 1)
            {
                return null;
            }
        }

        Vector3 contact = Vector3.Lerp(from, to, progress) - Settings.TargetPosition;
        Vector3 normal = contact.LengthSquared() > Epsilon ? Vector3.Normalize(contact) : -Vector3.UnitZ;

        return new Impact(progress, ProjectileEnd.HitTarget, ImpactSurface.Character, normal);
    }

    private void EndTrack(ActiveShot shot, ProjectileEnd end)
    {
        shot.Track.EndTime = Time;
        shot.Track.End = end;

        if (shot.Track.Samples[^1].Time < Time)
        {
            shot.Track.Samples.Add(new TrackSample(Time, shot.Position));
        }
    }

    private float GetInterval(BulletDefinition definition)
    {
        // A few rows have these the wrong way around
        var min = Math.Min(definition.IntervalMin, definition.IntervalMax);
        var max = Math.Max(definition.IntervalMin, definition.IntervalMax);

        return min + (max - min) * Rng.NextSingle();
    }

    private float Random(float range)
    {
        return range > 0 ? (Rng.NextSingle() * 2 - 1) * range : 0;
    }

    private static Vector3 GetDirection(float yaw, float pitch)
    {
        return new Vector3(MathF.Sin(yaw) * MathF.Cos(pitch), MathF.Sin(pitch), MathF.Cos(yaw) * MathF.Cos(pitch));
    }

    /// <summary>
    ///     Turns away from a direction, with left and right and up and down being relative to that direction.
    ///     Adding to its yaw and pitch instead would do nothing for a bullet pointing straight down, as
    ///     yaw only spins it on the spot, and a whole fan would end up in a line.
    /// </summary>
    private static Vector3 RotateFrom(Vector3 forward, float yaw, float pitch)
    {
        // Pointing straight up or down leaves the top of the bullet towards the shooter's back or front
        Vector3 reference = MathF.Abs(forward.Y) < VerticalLimit ? Vector3.UnitY
            : forward.Y > 0 ? -Vector3.UnitZ : Vector3.UnitZ;

        Vector3 right = Vector3.Normalize(Vector3.Cross(reference, forward));
        Vector3 up = Vector3.Cross(forward, right);
        Vector3 local = GetDirection(yaw, pitch);

        return right * local.X + up * local.Y + forward * local.Z;
    }

    private void Include(Vector3 position)
    {
        BoundsMin = Vector3.Min(BoundsMin, position);
        BoundsMax = Vector3.Max(BoundsMax, position);
    }
}
