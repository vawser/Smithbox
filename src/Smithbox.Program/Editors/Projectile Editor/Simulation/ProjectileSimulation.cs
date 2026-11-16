using System;
using System.Numerics;

namespace StudioCore.Editors.ProjectileEditor;

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

/// <summary>
///     How the target moves around its position, which is what shows how well a bullet homes.
/// </summary>
public enum TargetMotion
{
    Still,
    Strafe,
    Advance,
    Circle
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
    AbsorbedCondition = 32
}

/// <param name="LaunchHeight">The height above the ground that the first bullet is fired from.</param>
/// <param name="EmitHeight">How far above their source that bullets set to start above it begin.</param>
/// <param name="FixedEmitRadius">How far around their source bullets can start, in games with no field for it.</param>
/// <param name="BehindTargetHeight">How far above the target that bullets set to start behind it begin.</param>
/// <param name="BehindTargetDistance">How far past the target that bullets set to start behind it begin.</param>
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
/// <param name="Motion">How the target moves.</param>
/// <param name="TargetSpeed">How fast the target moves.</param>
/// <param name="TargetRange">How far the target moves from its position.</param>
/// <param name="Seed">Rows can vary their timing and aim, so this keeps a simulation repeatable.</param>
/// <param name="EmitterDuration">
///     How long the emitter of the first bullets lasts (the length of the animation's bullet event), during
///     which a volley with a shootInterval fires again. 0 fires once.
/// </param>
/// <param name="DistanceScale">The range multiplier of the weapon or catalyst that fires the bullets.</param>
/// <param name="DistanceUpEffect">Whether the shooter has the special effect that applies spBulletDistUpRate.</param>
public sealed record SimulationSettings(
    float LaunchHeight,
    float EmitHeight,
    float FixedEmitRadius,
    float BehindTargetHeight,
    float BehindTargetDistance,
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
    TargetMotion Motion,
    float TargetSpeed,
    float TargetRange,
    int Seed,
    float EmitterDuration = 0,
    float DistanceScale = 1,
    bool DistanceUpEffect = false)
{
    public bool IsTargetMoving => Motion is not TargetMotion.Still && TargetSpeed > 0 && TargetRange > 0;

    /// <summary>
    ///     How long the target takes to get back to where it started.
    /// </summary>
    public float TargetPeriod => IsTargetMoving ? MathF.Tau * TargetRange / TargetSpeed : 0;

    /// <summary>
    ///     Where the target is at a point in the simulation, which is a sine wave so that it keeps to its speed
    ///     through the middle of its range and slows down to turn around.
    /// </summary>
    public Vector3 GetTargetPosition(float time)
    {
        if (!IsTargetMoving)
        {
            return TargetPosition;
        }

        var phase = time * TargetSpeed / TargetRange;
        var sine = MathF.Sin(phase) * TargetRange;

        return Motion switch
        {
            TargetMotion.Strafe => TargetPosition + Vector3.UnitX * sine,
            TargetMotion.Advance => TargetPosition + Vector3.UnitZ * sine,
            _ => TargetPosition + new Vector3(sine, 0, MathF.Cos(phase) * TargetRange)
        };
    }
}

public readonly record struct TrackSample(float Time, Vector3 Position);

/// <summary>
///     A bullet landing a hit on the target.
/// </summary>
/// <param name="Rates">
///     How much of each type of damage is left, from 1 down to 0, for bullets that lose damage past their range.
/// </param>
public readonly record struct DamageEvent(float Time, int Track, AttackDefinition Attack, AttackDamage Rates)
{
    public AttackDamage Flat => AttackDamage.Scale(Attack.Flat, Rates);
    public AttackDamage Correction => AttackDamage.Scale(Attack.Correction, Rates);
}

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

    private Vector3 Target => Settings.GetTargetPosition(Time);

    private sealed record PendingShot(
        BulletDefinition Definition,
        Vector3 Position,
        Vector3 Direction,
        Vector3 Aim,
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

        // As fired, since Position and Direction move on
        public Vector3 FiredFrom;
        public Vector3 Aim;

        // One vector, so a turn swings the fall with it and a limit caps the whole
        public Vector3 Velocity;
        public float Age;
        public float Travelled;
        public float Life;
        public float NextInterval;
        public int IntervalCount;
        public Vector3 AimOffset;
        public bool HomingStopped;
        public float NextDamageTime;

        // The direction it was fired in (externalForce pushes along it), its range with the shooter's
        // multipliers, and how much of its damage is left
        public Vector3 ShootDirection;
        public float Range;
        public AttackDamage DamageRates = AttackDamage.One;
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
                // The emitter fires the volley again every shootInterval for as long as it lasts
                var batches = root.BatchInterval > 0 && Settings.EmitterDuration > 0
                    ? (int)MathF.Floor(Settings.EmitterDuration / root.BatchInterval + Epsilon) + 1
                    : 1;

                for (var batch = 0; batch < batches; batch++)
                {
                    Fire(root, batch * root.BatchInterval, origin, forward, null, ProjectileOrigin.Root, -1, 0);
                }
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
            var reach = Settings.TargetRadius + (Settings.IsTargetMoving ? Settings.TargetRange : 0);

            Include(Settings.TargetPosition - new Vector3(reach));
            Include(Settings.TargetPosition + new Vector3(reach));
        }

        Result.BoundsMin = BoundsMin;
        Result.BoundsMax = BoundsMax;
        Result.Duration = Result.Tracks.Count > 0 ? Result.Tracks.Max(track => track.EndTime) : 0;

        return Result;
    }

    private void Fire(BulletDefinition definition, float time, Vector3 position, Vector3 forward, float? speed,
        ProjectileOrigin origin, int parent, int generation)
    {
        // These are only out of reach for want of a target, or of something to aim at
        Result.Ignored |= definition.Ignored & (Settings.SimulateTarget
            ? ~(BulletIgnoredBehavior.Homing | BulletIgnoredBehavior.EmitPosition | BulletIgnoredBehavior.Arc)
            : definition.NoLockTargetDistance != 0 ? ~BulletIgnoredBehavior.Homing : BulletIgnoredBehavior.None);

        var root = origin is ProjectileOrigin.Root;

        // A bullet set to be level when created is levelled from the cast too
        if (root && definition.Posture is BulletChildPosture.Horizontal)
        {
            forward = Flatten(forward, Vector3.UnitZ);
        }

        // The emitter's shot angles only apply to bullets that start at the shooter, or above it. Bullets created by
        // other bullets are left with theirs, as the rows that use them suggest
        var applyAngles = !root || definition.EmitPosition is BulletEmitPosition.Source or BulletEmitPosition.AboveSource;

        for (var volley = 0; volley < definition.VolleyCount; volley++)
        {
            for (var shot = 0; shot < definition.ShotsPerVolley; shot++)
            {
                if (Result.Tracks.Count + Pending.Count >= ProjectileLimit)
                {
                    Result.Warnings |= SimulationWarning.ProjectileLimit;
                    return;
                }

                var baseY = applyAngles ? definition.AngleY : 0;
                var baseX = applyAngles ? definition.AngleX : 0;
                var fanY = applyAngles ? volley * definition.VolleyAngleY + shot * definition.ShotAngleY : 0;
                var fanX = applyAngles ? volley * definition.VolleyAngleX + shot * definition.ShotAngleX : 0;

                var angleY = baseY + fanY + Random(definition.AngleYRandom);
                var angleX = baseX + fanX + Random(definition.AngleXRandom);

                var due = time + volley * definition.VolleyInterval;

                Vector3 start = GetStart(definition, position, forward, due);
                Vector3 facing = definition.EmitPosition is BulletEmitPosition.BehindTarget
                    ? GetFacingTarget(start, forward, due)
                    : forward;

                float? shotSpeed = speed;

                // The emitter turns towards the target first, as far as the row lets it
                if (root && Settings.SimulateTarget && definition.LockAngle > 0)
                {
                    facing = TurnTowards(facing, GetAimPoint(definition, start, due) - start,
                        float.DegreesToRadians(definition.LockAngle));
                }

                if (definition.Howitzer && Settings.SimulateTarget &&
                    SolveHowitzer(definition, start, due, out Vector3 level, out var lobAngle, out var lobSpeed))
                {
                    // The solved elevation replaces the row's, and the fan and spread still apply on top
                    facing = level;
                    angleY = fanY + Random(definition.AngleYRandom);
                    angleX = lobAngle + fanX + Random(definition.AngleXRandom);
                    shotSpeed = lobSpeed;
                }

                Vector3 direction = RotateFrom(facing, float.DegreesToRadians(angleY), float.DegreesToRadians(angleX));

                Pending.Enqueue(new PendingShot(definition, start, direction, facing, shotSpeed, origin, parent,
                    generation), due);
            }
        }
    }

    /// <summary>
    ///     Works out where a bullet begins. Anything that goes by the target falls back on
    ///     the source when there isn't one, which is also what the game does.
    /// </summary>
    private Vector3 GetStart(BulletDefinition definition, Vector3 source, Vector3 forward, float time)
    {
        Vector3 target = Settings.SimulateTarget ? Settings.GetTargetPosition(time) : source;

        switch (definition.EmitPosition)
        {
            case BulletEmitPosition.AroundSource:
                return GetPointAround(source, definition) with { Y = 0 };

            case BulletEmitPosition.AboveSource:
                return source + Vector3.UnitY * Settings.EmitHeight;

            case BulletEmitPosition.Target:
                return target;

            case BulletEmitPosition.AroundTarget:
                return GetPointAround(target, definition);

            case BulletEmitPosition.BehindTarget:
                Vector3 away = Settings.SimulateTarget ? Flatten(target - source, forward) : -Flatten(forward, Vector3.UnitZ);

                return target + Vector3.UnitY * Settings.BehindTargetHeight + away * Settings.BehindTargetDistance;

            default:
                return source;
        }
    }

    /// <summary>
    ///     Where a bullet aims when it's fired: the target, raised by targetYOffsetRange, and ahead of
    ///     where the target is going when the row predicts it.
    /// </summary>
    private Vector3 GetAimPoint(BulletDefinition definition, Vector3 start, float time, bool raise = true)
    {
        Vector3 target = Settings.GetTargetPosition(time);

        if (definition.LeadsTarget && Settings.IsTargetMoving)
        {
            var speed = MathF.Max(definition.InitialSpeed, 1);

            // Twice is enough for the time of flight to settle
            for (var i = 0; i < 2; i++)
            {
                target = Settings.GetTargetPosition(time + Vector3.Distance(start, target) / speed);
            }
        }

        return raise ? target + Vector3.UnitY * definition.HomingHeightOffset : target;
    }

    /// <summary>
    ///     Turns a direction towards another one, by at most an angle in radians.
    /// </summary>
    private static Vector3 TurnTowards(Vector3 from, Vector3 to, float maxAngle)
    {
        if (to.LengthSquared() < Epsilon)
        {
            return from;
        }

        from = Vector3.Normalize(from);
        to = Vector3.Normalize(to);

        var angle = MathF.Acos(Math.Clamp(Vector3.Dot(from, to), -1, 1));

        if (angle <= maxAngle)
        {
            return to;
        }

        Vector3 axis = Vector3.Cross(from, to);

        // Straight away from it there is no one way to turn, so it turns over the top
        if (axis.LengthSquared() < Epsilon)
        {
            axis = Vector3.Cross(from, MathF.Abs(from.Y) < VerticalLimit ? Vector3.UnitY : Vector3.UnitZ);
        }

        return Vector3.Normalize(Vector3.Transform(from, Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), maxAngle)));
    }

    /// <summary>
    ///     The howitzer, as eldenring.exe 1403ACC20 solves it. At its own speed, a bullet takes whichever of the two
    ///     elevations that reach the target is nearest its row's angle, inside the row's limits. If neither fits, it
    ///     takes the angle with the longest reach (halfway between the target and straight up, inside the limits) and
    ///     the speed that angle needs, inside the row's speed limits.
    /// </summary>
    /// <param name="level">The level direction towards the target.</param>
    /// <param name="angle">The elevation in degrees.</param>
    /// <param name="speed">The speed to fire at.</param>
    private bool SolveHowitzer(BulletDefinition definition, Vector3 start, float time, out Vector3 level,
        out float angle, out float speed)
    {
        Vector3 offset = GetAimPoint(definition, start, time, false) - start;
        var dx = (offset with { Y = 0 }).Length();
        var dy = offset.Y;

        level = default;
        angle = 0;
        speed = 0;

        if (dx < Epsilon)
        {
            return false;
        }

        level = (offset with { Y = 0 }) / dx;

        var min = Math.Clamp(Math.Min(definition.HowitzerAngleMin, definition.HowitzerAngleMax), -90, 90);
        var max = Math.Clamp(Math.Max(definition.HowitzerAngleMin, definition.HowitzerAngleMax), -90, 90);
        var preferred = definition.AngleX;
        var g = definition.GravityInRange;
        var v0 = definition.InitialSpeed;

        if (MathF.Abs(g) < Epsilon)
        {
            angle = Math.Clamp(preferred, min, max);
            speed = v0;
            return true;
        }

        // The two elevations that reach the target at the bullet's own speed
        var v2 = v0 * v0;
        var discriminant = v2 * v2 - g * (g * dx * dx + 2 * dy * v2);

        if (discriminant >= 0)
        {
            var root = MathF.Sqrt(discriminant);
            var found = false;
            var closest = float.MaxValue;

            foreach (var tangent in new[] { (v2 + root) / (g * dx), (v2 - root) / (g * dx) })
            {
                var candidate = float.RadiansToDegrees(MathF.Atan(tangent));

                if (candidate >= min && candidate <= max && MathF.Abs(candidate - preferred) < closest)
                {
                    closest = MathF.Abs(candidate - preferred);
                    angle = candidate;
                    found = true;
                }
            }

            if (found)
            {
                speed = v0;
                return true;
            }
        }

        // Out of reach at that speed: the angle with the longest reach, and the speed it needs
        var elevation = float.RadiansToDegrees(MathF.Atan2(dy, dx));

        angle = Math.Clamp(elevation / 2 + 45, min, max);

        var tan = MathF.Tan(float.DegreesToRadians(angle));
        var climb = dx * tan - dy;
        var needed = climb > 0 ? g * dx * dx * (1 + tan * tan) / (2 * climb) : -1;
        var maxSpeed = definition.HowitzerSpeedMax > 0 ? definition.HowitzerSpeedMax : float.MaxValue;

        speed = needed > 0 ? MathF.Min(MathF.Sqrt(needed), maxSpeed) : maxSpeed == float.MaxValue ? v0 : maxSpeed;
        speed = MathF.Max(speed, definition.HowitzerSpeedMin);

        return true;
    }

    /// <summary>
    ///     Picks a point around a position, spread evenly over the whole circle.
    /// </summary>
    private Vector3 GetPointAround(Vector3 position, BulletDefinition definition)
    {
        var radius = definition.EmitRadius ?? Settings.FixedEmitRadius;
        var distance = radius * MathF.Sqrt(Rng.NextSingle());
        var angle = Rng.NextSingle() * MathF.Tau;

        return position + new Vector3(MathF.Sin(angle), 0, MathF.Cos(angle)) * distance;
    }

    /// <summary>
    ///     The way that a bullet behind the target faces, so that its angles carry it back towards the target.
    /// </summary>
    private Vector3 GetFacingTarget(Vector3 start, Vector3 forward, float time)
    {
        return Settings.SimulateTarget ? Flatten(Settings.GetTargetPosition(time) - start, forward) : forward;
    }

    private static Vector3 Flatten(Vector3 direction, Vector3 fallback)
    {
        Vector3 flat = direction with { Y = 0 };

        return flat.LengthSquared() > Epsilon ? Vector3.Normalize(flat) : fallback;
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

                // Where it was aimed, which leaves out the row's own launch angle
                FiredFrom = shot.Position,
                Aim = shot.Aim,

                Velocity = shot.Direction * (shot.Speed ?? definition.InitialSpeed),
                Life = GetLife(definition),
                ShootDirection = shot.Direction,
                Range = GetRange(definition),
                NextInterval = definition.IntervalWait + GetInterval(definition),
                AimOffset = Vector3.UnitY * definition.HomingHeightOffset +
                            new Vector3(Random(definition.HomingOffset), Random(definition.HomingOffset),
                               Random(definition.HomingOffset))
            });
        }
    }

    private float GetLife(BulletDefinition definition)
    {
        if (definition.Life < 0)
        {
            return float.MaxValue;
        }

        // The random range only ever adds to the life
        return definition.Life + Rng.NextSingle() * MathF.Max(0, definition.LifeRandom);
    }

    /// <summary>
    ///     The distance past which a bullet uses its out-of-range gravity and acceleration, and loses damage: scaled by
    ///     the weapon, and by spBulletDistUpRate while the shooter has its effect.
    /// </summary>
    private float GetRange(BulletDefinition definition)
    {
        return definition.Range * Settings.DistanceScale * (Settings.DistanceUpEffect ? definition.DistanceUpRate : 1);
    }

    /// <returns>False once the bullet has ended.</returns>
    private bool Advance(ActiveShot shot)
    {
        BulletDefinition definition = shot.Definition;

        shot.Age += StepTime;

        // For the howitzer, which moves by the average of this step's velocity and the last
        Vector3 previousVelocity = shot.Velocity;

        Home(shot);

        // Out of range once it has gone past it
        var inRange = shot.Travelled <= shot.Range;
        var accel = inRange ? definition.AccelInRange : definition.AccelOutRange;
        var gravity = inRange ? definition.GravityInRange : definition.GravityOutRange;

        // Once a frame rather than per second, along the way it was fired with the height taken out (not normalized),
        // and only ever forward
        if (definition.ExternalForce > 0)
        {
            shot.Velocity += new Vector3(shot.ShootDirection.X, 0, shot.ShootDirection.Z) * definition.ExternalForce;
        }

        if (shot.Age >= definition.AccelStartTime && shot.Age < definition.AccelEndTime)
        {
            // Along the way it is going, not the way it points: gravity pulls those apart
            shot.Velocity += Normalize(shot.Velocity, shot.Direction) * (accel * StepTime);
        }

        if (shot.Age >= definition.GravityStartTime)
        {
            shot.Velocity -= Vector3.UnitY * (gravity * StepTime);
        }

        // Limits hold every step, and rows can start outside of them. The minimum is checked first, and a maximum of 0
        // stops the bullet
        var speed = shot.Velocity.Length();
        var limit = speed < definition.MinSpeed ? definition.MinSpeed : MathF.Min(speed, definition.MaxSpeed);

        if (limit != speed)
        {
            shot.Velocity = Normalize(shot.Velocity, shot.Direction) * limit;
        }

        // Past its range, each type of damage drops by its own percentage per second
        if (!inRange && !definition.DamageFalloff.IsZero)
        {
            shot.DamageRates = AttackDamage.Reduce(shot.DamageRates,
                AttackDamage.Scale(definition.DamageFalloff, new AttackDamage(StepTime, StepTime, StepTime, StepTime,
                    StepTime, StepTime)));
        }

        Vector3 next = shot.Position + (definition.Howitzer ? (previousVelocity + shot.Velocity) / 2 : shot.Velocity) * StepTime;

        var radius = shot.Track.GetRadius(Time);

        if (FindImpact(definition, shot.Position, next, radius) is { } impact)
        {
            Vector3 contact = Vector3.Lerp(shot.Position, next, impact.Progress);

            shot.Position = contact;
            Include(contact);
            shot.Track.Samples.Add(new TrackSample(Time, contact));

            DealDamage(shot, radius);

            EndTrack(shot, impact.End);

            // The bullet stops with its edge touching, which leaves what it hit that far from its center
            var reach = impact.End is ProjectileEnd.HitTarget ? GetReach(definition, radius) : radius;

            Chain(shot, impact, contact - impact.Normal * reach);

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
            Chain(shot, null, null);

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

        if (Vector3.Distance(shot.Position, Target) > reach)
        {
            return;
        }

        Result.Damage.Add(new DamageEvent(Time, shot.Index, attack, shot.DamageRates));

        var interval = shot.Definition.DamageInterval;

        shot.NextDamageTime = interval > 0 ? Time + interval : float.MaxValue;
    }

    private static float GetReach(BulletDefinition definition, float radius)
    {
        return MathF.Max(radius, definition.Attack?.Radius ?? 0);
    }

    private static Vector3 Normalize(Vector3 value, Vector3 fallback)
    {
        return value.LengthSquared() > Epsilon ? Vector3.Normalize(value) : fallback;
    }

    /// <summary>
    ///     Turns the bullet towards the target, within a cone of its homing rate.
    ///     Nothing is aimed high to make up for gravity, so a heavy bullet can still
    ///     drop short of the point it turns towards.
    /// </summary>
    private void Home(ActiveShot shot)
    {
        BulletDefinition definition = shot.Definition;

        if (shot.HomingStopped || shot.Age < definition.HomingBeginTime ||
            shot.Travelled < definition.HomingBeginDistance ||
            definition.HomingRate <= 0)
        {
            return;
        }

        var reach = definition.NoLockTargetDistance < 0 ? definition.Range : definition.NoLockTargetDistance;

        if (!Settings.SimulateTarget && reach <= 0)
        {
            return;
        }

        // With nothing locked on, a homing bullet also locks on to a virtual point
        Vector3 aimPoint = Settings.SimulateTarget ? Target : shot.FiredFrom + shot.Aim * reach;
        Vector3 toTarget = aimPoint + shot.AimOffset - shot.Position;
        var distance = toTarget.Length();

        // A bullet can start on top of the target, which leaves nothing to aim at for now
        if (distance < Epsilon)
        {
            return;
        }

        if (distance <= definition.HomingStopRange ||
            definition.HomingEndDistance > 0 && shot.Travelled >= definition.HomingEndDistance)
        {
            shot.HomingStopped = true;
            return;
        }

        Vector3 wanted = toTarget / distance;

        // A turn starts from where the bullet is going, fall included
        Vector3 heading = Normalize(shot.Velocity, shot.Direction);

        if (definition.HomingStopAngle > 0 &&
            float.RadiansToDegrees(MathF.Acos(Math.Clamp(Vector3.Dot(heading, wanted), -1, 1))) > definition.HomingStopAngle)
        {
            shot.HomingStopped = true;
            return;
        }

        // Straight towards the target, by at most the rate in any direction (eldenring.exe 140398280). Turning only
        // costs the bullet its heading, not the speed it has
        shot.Direction = TurnTowards(heading, wanted, float.DegreesToRadians(definition.HomingRate) * StepTime);
        shot.Velocity = shot.Direction * shot.Velocity.Length();
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

            FireChildren(shot, definition.IntervalChildren, Time, ProjectileOrigin.Interval, null, null);
        }
    }

    private void Chain(ActiveShot shot, Impact? impact, Vector3? hitLocation)
    {
        BulletDefinition definition = shot.Definition;

        if (definition.ChainChildren.Count == 0)
        {
            return;
        }

        if (definition.ChainCondition is BulletChainCondition.Absorbed)
        {
            Result.Warnings |= SimulationWarning.AbsorbedCondition;
        }

        if (!AllowsChain(definition.ChainCondition, impact?.Surface))
        {
            shot.Track.ChainWithheld = true;
            return;
        }

        FireChildren(shot, definition.ChainChildren, Time + (impact != null ? definition.ChainDelay : 0),
            ProjectileOrigin.Chain, impact?.Normal, hitLocation);
    }

    private void FireChildren(ActiveShot shot, IReadOnlyList<int> children, float time, ProjectileOrigin origin,
        Vector3? impactNormal, Vector3? hitLocation)
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

            Vector3 source = child.EmitPosition is BulletEmitPosition.Parent
                ? shot.Position
                : hitLocation ?? shot.Position;

            Fire(child, time, source, GetChildForward(shot, child, impactNormal),
                definition.ChildInheritsSpeed ? shot.Velocity.Length() : null, origin, shot.Index, shot.Track.Generation + 1);
        }
    }

    private static Vector3 GetChildForward(ActiveShot shot, BulletDefinition child, Vector3? impactNormal)
    {
        if (child.ChildAimedFromOwner)
        {
            return Vector3.UnitZ;
        }

        // A new bullet is turned by its own posture
        switch (child.Posture ?? BulletChildPosture.Horizontal)
        {
            case BulletChildPosture.Horizontal:
                Vector3 flat = new(shot.Velocity.X, 0, shot.Velocity.Z);

                return flat.LengthSquared() > Epsilon ? Vector3.Normalize(flat) : Vector3.UnitZ;

            case BulletChildPosture.Impact when impactNormal != null:
                return impactNormal.Value;

            default:
                return Normalize(shot.Velocity, shot.Direction);
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

            // Never held back, despite the name (eldenring.exe 14039DA50)
            BulletChainCondition.NotCharacter => true,

            // Any hit on the map or an object, water included, but not a character or running out
            BulletChainCondition.MapOnly => surface is ImpactSurface.Map or ImpactSurface.Water,

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
        Vector3 target = Target;
        Vector3 offset = from - target;

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

        Vector3 contact = Vector3.Lerp(from, to, progress) - target;
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
