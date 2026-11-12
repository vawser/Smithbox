using System;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>How the player moves: speeds, gravity, jump, steps and slopes.</summary>
public struct FirstPersonMoveSettings
{
    public float WalkSpeed;
    public float RunSpeed;
    public float Gravity;
    public float JumpHeight;
    public float StepHeight;

    /// <summary>Cosine of the steepest walkable slope: a contact whose normal's Y is at least this is ground.</summary>
    public float MaxSlopeCos;

    public float GroundAcceleration;
    public float AirAcceleration;
    public float MaxFallSpeed;

    /// <summary>A jump still works this long after walking off a ledge.</summary>
    public float CoyoteTime;

    /// <summary>
    /// Ground whose collision material has the Slide property (Slide1, Slide2) isn't stood on: the player slides off it,
    /// as on a slope steeper than the limit.
    /// </summary>
    public bool SlideNotStandable;

    /// <summary>A jump pressed this long before landing happens on landing.</summary>
    public float JumpBuffer;

    public static FirstPersonMoveSettings Default => new()
    {
        WalkSpeed = 4f,
        RunSpeed = 7f,
        Gravity = 20f,
        JumpHeight = 1f,
        StepHeight = 0.45f,
        MaxSlopeCos = MathF.Cos(50f * MathF.PI / 180f),
        GroundAcceleration = 40f,
        AirAcceleration = 8f,
        MaxFallSpeed = 50f,
        CoyoteTime = 0.1f,
        JumpBuffer = 0.1f,
        SlideNotStandable = true
    };

    /// <summary>Whether a ray hit (or ground below) is somewhere the player can stand: the slope and, if set, not Slide.</summary>
    public bool CanStandOn(FirstPersonHit hit)
        => hit.Normal.Y >= MaxSlopeCos && !(SlideNotStandable && FirstPersonCollisionReader.IsSlide(hit.Material));
}

/// <summary>What the player is asked to do this frame.</summary>
public struct FirstPersonInput
{
    /// <summary>The direction to walk in, in world space, horizontal, length up to 1.</summary>
    public Vector3 Move;

    public bool Run;

    /// <summary>Jump was pressed this frame (an edge, not held).</summary>
    public bool Jump;

    /// <summary>
    /// In noclip: the direction to fly in, in world space, with its vertical part (the camera's facing), length up to 1.
    /// Ignored when walking.
    /// </summary>
    public Vector3 Fly;

    /// <summary>In noclip: rise while held (the jump key).</summary>
    public bool Rise;
}

/// <summary>
/// The player's movement: a kinematic capsule stepped at a fixed 60 Hz, so it behaves the same at any frame rate.
/// Each step moves the capsule in small substeps (at most half its radius, so it can't pass through a triangle) and
/// pushes it out of what it overlaps, removing the velocity into walls. On the ground, walls and steep slopes are
/// pushed out of horizontally, steps up to the step height are climbed, and the feet follow the ground down slopes
/// and stairs. Depends only on System.Numerics, the capsule and the collision set, so the survey tool links it.
/// </summary>
public sealed class FirstPersonController
{
    public const float StepTime = 1f / 60f;

    /// <summary>Most steps a frame; a longer frame (a hitch, a breakpoint) is dropped rather than caught up.</summary>
    public const int MaxStepsPerFrame = 5;

    /// <summary>How far into the collision the capsule may reach and count as clear.</summary>
    public const float Allowance = 0.02f;

    public FirstPersonCapsule Capsule;
    public FirstPersonMoveSettings Settings;

    /// <summary>The feet after the last step, and before it, for drawing between steps.</summary>
    public Vector3 Feet { get; private set; }
    public Vector3 PreviousFeet { get; private set; }

    public Vector3 Velocity { get; private set; }

    public bool Grounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.UnitY;

    /// <summary>The collision material underfoot while grounded, else <see cref="FirstPersonTriangles.NoMaterial"/>.</summary>
    public uint GroundMaterial { get; private set; } = FirstPersonTriangles.NoMaterial;

    // The material of the last ground probe's contact
    private uint ProbedMaterial = FirstPersonTriangles.NoMaterial;

    /// <summary>Below this height the player has fallen out of the world.</summary>
    public float KillHeight = float.MinValue;

    /// <summary>Set when the player falls below the kill height; the owner respawns them.</summary>
    public bool FellOut { get; private set; }

    /// <summary>Flying through everything: no gravity, no collision, along the camera's facing.</summary>
    public bool Noclip { get; private set; }

    /// <summary>Noclip flies this many times the walk (or run) speed.</summary>
    public const float NoclipSpeedFactor = 2.5f;

    /// <summary>Leaving noclip moves the player out of what they're in by at most this much, or not at all.</summary>
    public const float NoclipExitReach = 1.5f;

    // Counters, for the survey and diagnostics
    public int StepCount { get; private set; }
    public int StepUps { get; private set; }

    private float Accumulator;
    private float SinceGrounded;
    private float JumpQueued;

    public FirstPersonController(FirstPersonCapsule capsule, FirstPersonMoveSettings settings)
    {
        Capsule = capsule;
        Settings = settings;
    }

    /// <summary>Places the player at the feet, at rest. Grounded is found on the first step.</summary>
    public void Reset(Vector3 feet)
    {
        Feet = feet;
        PreviousFeet = feet;
        Velocity = Vector3.Zero;
        Grounded = false;
        GroundNormal = Vector3.UnitY;
        Accumulator = 0;
        SinceGrounded = 0;
        JumpQueued = 0;
        FellOut = false;
    }

    /// <summary>
    /// Turns noclip on, or off. Off puts the player back under collision: pushed out of what they're in if that is
    /// within <see cref="NoclipExitReach"/>, falling from there. If they're too deep in (inside a wall, under the map),
    /// noclip stays on and false is returned.
    /// </summary>
    public bool SetNoclip(FirstPersonCollisionSet world, bool on)
    {
        if (on == Noclip)
            return true;

        if (on)
        {
            Noclip = true;
            Grounded = false;
            Velocity = Vector3.Zero;
            return true;
        }

        var reach = new Vector3(NoclipExitReach + Capsule.Height);
        world.CopyNearby(Feet - reach, Feet + reach, Nearby);

        var feet = Feet;

        if (!Capsule.Depenetrate(Nearby, ref feet, Allowance, 16) || Vector3.Distance(feet, Feet) > NoclipExitReach)
            return false;

        Noclip = false;
        Feet = feet;
        PreviousFeet = feet;
        Velocity = Vector3.Zero;
        Grounded = false;
        SinceGrounded = Settings.CoyoteTime + 1;
        return true;
    }

    /// <summary>
    /// Where the player could stand at what a ray from the eye hits: on the surface if it is walkable, else (a wall, a
    /// ceiling) on the ground below a point backed off from it. The feet are put clear of the collision, and the spot is
    /// refused if they would have to move more than the radius to be (too tight), or there is no ground. For teleporting.
    /// </summary>
    public static bool FindStandingSpot(FirstPersonCollisionSet world, FirstPersonCapsule capsule, FirstPersonMoveSettings settings,
        Vector3 eye, Vector3 direction, float maxDistance, out Vector3 feet, out FirstPersonHit target)
    {
        feet = default;

        if (!world.RayCast(eye, direction, maxDistance, out target))
            return false;

        FirstPersonHit ground;

        if (settings.CanStandOn(target))
        {
            ground = target;
        }
        else
        {
            // Off the wall by the radius (and a little), then down onto what is below
            var back = target.Point + target.Normal * (capsule.Radius + 0.1f);

            if (!world.RayCast(back + new Vector3(0, 0.25f, 0), -Vector3.UnitY, 200, out ground) || !settings.CanStandOn(ground))
                return false;
        }

        feet = ground.Point + new Vector3(0, 0.01f, 0);
        var start = feet;

        if (!capsule.Depenetrate(world, ref feet, Allowance) || Vector3.Distance(start, feet) > capsule.Radius)
            return false;

        return true;
    }

    /// <summary>Moves the player to the feet, keeping noclip as it is.</summary>
    public void Teleport(Vector3 feet)
    {
        var noclip = Noclip;
        Reset(feet);
        Noclip = noclip;
    }

    /// <summary>The feet between the last two steps, by how far into the next step this frame is.</summary>
    public Vector3 InterpolatedFeet => Vector3.Lerp(PreviousFeet, Feet, Math.Clamp(Accumulator / StepTime, 0f, 1f));

    /// <summary>Runs the steps that fit in a frame of the given length.</summary>
    public void Update(FirstPersonCollisionSet set, FirstPersonInput input, float frameTime)
    {
        if (input.Jump)
            JumpQueued = Settings.JumpBuffer;

        Accumulator += MathF.Min(MathF.Max(frameTime, 0f), 0.25f);

        var steps = 0;

        while (Accumulator >= StepTime && steps < MaxStepsPerFrame)
        {
            Step(set, input);
            Accumulator -= StepTime;
            steps++;

            // The press is queued already; the frame's other steps don't press again
            input.Jump = false;
        }

        if (steps == MaxStepsPerFrame && Accumulator >= StepTime)
            Accumulator = 0;
    }

    // The meshes near the player, gathered once a step, so each of the step's queries doesn't go over every mesh
    private readonly FirstPersonCollisionSet Nearby = new();

    /// <summary>One fixed step.</summary>
    public void Step(FirstPersonCollisionSet world, FirstPersonInput input)
    {
        const float dt = StepTime;
        var s = Settings;

        // Everything the step can reach: the capsule, the furthest it moves, the step up and the ground probe below
        var gather = MathF.Max(s.RunSpeed, s.MaxFallSpeed) * dt + s.StepHeight + Capsule.Radius + 0.5f;
        world.CopyNearby(Feet - new Vector3(gather), Feet + new Vector3(gather, Capsule.Height + gather, gather), Nearby);
        var set = Nearby;

        PreviousFeet = Feet;
        StepCount++;

        if (Noclip)
        {
            // Straight to the asked speed, along the facing; the jump key rises
            var fly = input.Fly + (input.Rise ? Vector3.UnitY : Vector3.Zero);

            if (fly.LengthSquared() > 1)
                fly = Vector3.Normalize(fly);

            Velocity = fly * (input.Run ? s.RunSpeed : s.WalkSpeed) * NoclipSpeedFactor;
            Feet += Velocity * dt;
            Grounded = false;
            JumpQueued = 0;
            return;
        }

        if (input.Jump)
            JumpQueued = MathF.Max(JumpQueued, s.JumpBuffer);

        // Walking: towards the asked velocity, quickly on the ground, slowly in the air
        var move = new Vector3(input.Move.X, 0, input.Move.Z);

        if (move.LengthSquared() > 1)
            move = Vector3.Normalize(move);

        var desired = move * (input.Run ? s.RunSpeed : s.WalkSpeed);
        var horizontal = new Vector3(Velocity.X, 0, Velocity.Z);

        // On the ground, towards the asked velocity (stopping when nothing is asked); in the air, steering only while a
        // direction is asked, so a jump or a slide keeps its speed
        if (Grounded)
            horizontal = MoveTowards(horizontal, desired, s.GroundAcceleration * dt);
        else if (move.LengthSquared() > 1e-6f)
            horizontal = MoveTowards(horizontal, desired, s.AirAcceleration * dt);

        // Jumping and falling
        SinceGrounded = Grounded ? 0 : SinceGrounded + dt;
        JumpQueued -= dt;

        var vertical = Velocity.Y;
        var jumped = false;

        if (JumpQueued > 0 && SinceGrounded <= s.CoyoteTime && vertical <= 0.01f)
        {
            vertical = MathF.Sqrt(2 * s.Gravity * MathF.Max(s.JumpHeight, 0));
            jumped = true;
            JumpQueued = 0;
            SinceGrounded = s.CoyoteTime + 1;
            Grounded = false;
        }
        else if (Grounded)
        {
            vertical = 0;
        }
        else
        {
            vertical = MathF.Max(vertical - s.Gravity * dt, -s.MaxFallSpeed);
        }

        var velocity = new Vector3(horizontal.X, vertical, horizontal.Z);
        var onGround = Grounded && !jumped;
        var feet = Feet;

        // Across: slide along walls; on the ground, climb steps the slide stops at
        var across = new Vector3(velocity.X, 0, velocity.Z) * dt;

        if (across.LengthSquared() > 1e-10f)
        {
            var start = feet;
            Slide(set, ref feet, across, onGround, ref velocity, out _);

            if (onGround && Horizontal(feet - start) < Horizontal(across) * 0.7f
                && TryStepUp(set, start, across, out var stepped)
                && Horizontal(stepped - start) > Horizontal(feet - start) + 0.005f)
            {
                feet = stepped;
                StepUps++;
            }
        }

        // Up and down: land on ground, stop at ceilings
        var landed = false;

        if (MathF.Abs(velocity.Y) > 1e-6f)
        {
            Slide(set, ref feet, new Vector3(0, velocity.Y * dt, 0), false, ref velocity, out var ground);

            if (ground && velocity.Y <= 0)
                landed = true;
        }

        // The ground under the feet: followed down slopes and stairs while walking, found again after landing
        if (!jumped && velocity.Y <= 0.01f)
        {
            var reach = onGround ? s.StepHeight : 0.05f;

            if (ProbeGround(set, ref feet, reach, out var normal))
            {
                Grounded = true;
                GroundNormal = normal;
                GroundMaterial = ProbedMaterial;
                velocity.Y = 0;
            }
            else
            {
                Grounded = false;
            }
        }
        else
        {
            Grounded = false;
        }

        if (!Grounded)
            GroundMaterial = FirstPersonTriangles.NoMaterial;

        if (landed && !Grounded)
            velocity.Y = MathF.Min(velocity.Y, 0);

        // Whatever is left over (corners where two pushes disagree)
        Capsule.Depenetrate(set, ref feet, Allowance, 8);

        Feet = feet;
        Velocity = velocity;

        if (Feet.Y < KillHeight)
            FellOut = true;
    }

    /// <summary>
    /// Moves the feet by the displacement in substeps of at most half the radius, pushing out of the collision after
    /// each. Contacts that aren't walkable take their part of the velocity and of the rest of the move (sliding along
    /// them); with flatten, they are pushed out of horizontally, so the ground isn't climbed by a wall. Returns whether
    /// walkable ground was touched.
    /// </summary>
    private void Slide(FirstPersonCollisionSet set, ref Vector3 feet, Vector3 displacement, bool flatten, ref Vector3 velocity,
        out bool touchedGround)
    {
        touchedGround = false;

        var length = displacement.Length();
        var count = Math.Max(1, (int)MathF.Ceiling(length / (Capsule.Radius * 0.5f)));
        var step = displacement / count;

        for (var i = 0; i < count; i++)
        {
            feet += step;

            for (var round = 0; round < 4; round++)
            {
                if (!Capsule.Deepest(set, feet, Allowance, out var depth, out var normal, out var face, out var point, out var material, -2f, false))
                    break;

                var amount = depth - Allowance * 0.5f;
                var walkable = FirstPersonCapsule.IsGround(normal, face, Settings.MaxSlopeCos)
                    && !(Settings.SlideNotStandable && FirstPersonCollisionReader.IsSlide(material));

                // While walking, an edge higher than a step is a wall, however its face lies
                if (walkable && flatten && point.Y - feet.Y > Settings.StepHeight + 0.01f)
                    walkable = false;

                if (flatten && !walkable)
                {
                    var side = new Vector3(normal.X, 0, normal.Z);
                    var sideLength = side.Length();

                    if (sideLength > 1e-3f)
                    {
                        side /= sideLength;
                        amount /= MathF.Max(sideLength, 0.2f);
                        normal = side;
                    }
                }

                // Walking over ground lifts the feet straight up, so slopes and seams don't slow the walk
                if (walkable && flatten)
                    feet.Y += amount / MathF.Max(normal.Y, 0.2f);
                else
                    feet += normal * amount;

                if (walkable)
                {
                    touchedGround = true;

                    // Landing: the fall stops; walking over ground keeps its speed
                    if (velocity.Y < 0)
                        velocity.Y = 0;

                    if (step.Y < 0)
                        step.Y = 0;
                }
                else
                {
                    velocity -= MathF.Min(0, Vector3.Dot(velocity, normal)) * normal;
                    step -= MathF.Min(0, Vector3.Dot(step, normal)) * normal;
                }
            }
        }
    }

    /// <summary>
    /// The ground below the feet within reach (plus a little), facing up enough to stand on. The feet are put on it,
    /// moved straight up or down only, so standing on a slope doesn't slide.
    /// </summary>
    private bool ProbeGround(FirstPersonCollisionSet set, ref Vector3 feet, float reach, out Vector3 normal)
        => ProbeGround(set, ref feet, reach, out normal, out _);

    /// <summary>As above, also giving where on the ground the feet stand (an edge's corner, on a stair).</summary>
    private bool ProbeGround(FirstPersonCollisionSet set, ref Vector3 feet, float reach, out Vector3 normal, out Vector3 groundPoint)
    {
        normal = Vector3.UnitY;
        groundPoint = feet;

        for (var drop = 0.03f; drop <= reach + 0.03f + 1e-4f; drop += 0.05f)
        {
            var test = feet - new Vector3(0, drop, 0);

            // Anything in the way first: a slope or wall met on the way down ends the probe, as the ground isn't
            // reached through it
            if (!Capsule.Deepest(set, test, Allowance, out _, out _, out _, out _, -2f))
                continue;

            if (!Capsule.Deepest(set, test, Allowance, out var depth, out var n, out _, out var contact, out var material,
                Settings.MaxSlopeCos, Settings.SlideNotStandable))
                return false;

            // Up out of the ground, by as much as pushing along its normal would
            var lift = (depth - Allowance * 0.5f) / MathF.Max(n.Y, 0.2f);
            feet = test + new Vector3(0, lift, 0);
            normal = n;
            groundPoint = contact;
            ProbedMaterial = material;
            return true;
        }

        return false;
    }

    /// <summary>A step up: up by the step height, across, then down onto ground. False if any part fails.</summary>
    private bool TryStepUp(FirstPersonCollisionSet set, Vector3 start, Vector3 across, out Vector3 result)
    {
        result = start;
        var feet = start;
        var scratch = Vector3.Zero;

        Slide(set, ref feet, new Vector3(0, Settings.StepHeight, 0), false, ref scratch, out _);

        var raised = feet.Y - start.Y;

        if (raised < Settings.StepHeight * 0.5f)
            return false;

        scratch = Vector3.Zero;
        Slide(set, ref feet, across, true, ref scratch, out _);

        // Down onto the step (its edge counts as ground, by its face)
        if (!ProbeGround(set, ref feet, raised + 0.05f, out _, out var groundPoint))
            return false;

        // It must have landed on the step, not back against the riser, and the step no higher than the step height
        if (feet.Y < start.Y + 0.02f || groundPoint.Y - start.Y > Settings.StepHeight + 0.01f)
            return false;

        if (Capsule.Overlaps(set, feet, Allowance * 2.5f, out _))
            return false;

        result = feet;
        return true;
    }

    private static float Horizontal(Vector3 v) => MathF.Sqrt(v.X * v.X + v.Z * v.Z);

    private static Vector3 MoveTowards(Vector3 from, Vector3 to, float maxDelta)
    {
        var delta = to - from;
        var length = delta.Length();

        return length <= maxDelta || length < 1e-6f ? to : from + delta / length * maxDelta;
    }
}
