#nullable enable
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Plays an act's steps (<see cref="LuaSituation.PlanAct"/>) on the viewer's character, toward its target:
/// - Clip: plays the animation once. It turns toward the target while the clip's TAE turn speed events (SetTurnSpeed)
///   are active, at their speed; a clip without them faces the target as it starts, as before;
/// - Approach / Leave: loops walk or run (or walk back) until within (or beyond) the goal's distance, or its life runs
///   out, turning toward the target as it goes;
/// - Sideway: strafes left or right, facing the target, for the goal's life (1 to 4 s);
/// - Turn: plays a 90 or 180 degree turn clip if the target is outside the goal's angle, then faces it; without a
///   clip, turns on the spot;
/// - Wait: stands for the goal's life (0.5 to 3 s).
/// The moving steps turn at the NpcParam row's turnVellocity; without one they face the target at once (each cycle).
/// When the steps run out, the character goes back to its idle, looping, instead of holding the last frame.
/// </summary>
public class LuaActPlayer
{
    private readonly LuaViewer Viewer;

    private List<LuaActStep>? Steps;
    private int Index = -1;
    private float StepTime;

    // A clip step's facing has been settled (turned at once, for a clip without turn events)
    private bool ClipFacingDone;

    // A Turn step without a turn clip, turning on the spot at the character's own speed
    private bool TurningInPlace;

    /// <summary>The step being played, for the status line; null when idle</summary>
    public LuaActStep? Current => Steps != null && Index >= 0 && Index < Steps.Count ? Steps[Index] : null;

    /// <summary>Steps skipped for want of a clip (named in the status line)</summary>
    public List<string> Skipped { get; } = new();

    /// <summary>How the character turned this frame (degrees a second, and from what), for the status line</summary>
    public (float Speed, string Source)? Turning { get; private set; }

    /// <summary>
    /// When the steps run out, hold the last pose instead of going back to idle: set while the simulation runs, as it
    /// starts the next act at once (it goes to idle itself when it waits)
    /// </summary>
    public bool HoldWhenDone;

    /// <summary>The steps of an act have all played (not raised when stopped)</summary>
    public event Action? Finished;

    /// <summary>A step's clip has started (for the simulation's cooldowns)</summary>
    public event Action<long>? ClipStarted;

    public LuaActPlayer(LuaViewer viewer)
    {
        Viewer = viewer;
    }

    private LuaCharacter? Self => Viewer.Self;

    /// <summary>How fast the character turns on its own: its NpcParam row's turnVellocity, or null to face at once</summary>
    private float? OwnTurnSpeed => Viewer.SelfNpc is { } npc && npc.Character == Self?.Name ? npc.TurnVelocity : null;

    public void Play(List<LuaActStep> steps)
    {
        Stop();
        Steps = steps.Where(s => s.Kind != LuaStepKind.Other).ToList();
        Skipped.Clear();
        Index = -1;
        Next();
    }

    public void Stop()
    {
        Steps = null;
        Index = -1;
        Turning = null;
    }

    /// <summary>Back to the idle, looping, from where the character now stands</summary>
    public void Idle()
    {
        Stop();
        if (Self == null)
            return;
        Self.Carry();
        if (!Self.PlayLoop(Self.Locomotion.Idle))
            Self.Carry();
    }

    /// <summary>Where the character is heading: the target character, else the point the distance and bearing give</summary>
    private Vector3 TargetPoint() => Viewer.TargetPoint();

    private float Distance()
    {
        if (Self == null)
            return 0;
        var d = TargetPoint() - Self.Position;
        return new Vector2(d.X, d.Z).Length();
    }

    /// <summary>
    /// The target's angle from the character's front, in degrees (-180..180): the yaw that turns the front onto it
    /// (a positive <see cref="StudioCore.Renderer.ModelAnimator.Turn"/> turns +Z toward +X, as this angle measures)
    /// </summary>
    private float AngleToTarget()
    {
        if (Self == null)
            return 0;
        var f = Self.WorldForward;
        var d = TargetPoint() - Self.Position;
        d.Y = 0;
        if (d.LengthSquared() < 1e-6f)
            return 0;
        var angle = MathF.Atan2(d.X, d.Z) - MathF.Atan2(f.X, f.Z);
        while (angle > MathF.PI) angle -= 2 * MathF.PI;
        while (angle < -MathF.PI) angle += 2 * MathF.PI;
        return angle * 180 / MathF.PI;
    }

    /// <summary>
    /// Turns the character toward the target by at most speed × dt degrees (all the way for no speed), on top of the
    /// clip's own root motion, so the clip keeps playing
    /// </summary>
    private void TurnToward(float? speed, float dt, string source)
    {
        if (Self?.Animator is not { } animator)
            return;
        var angle = AngleToTarget();
        var most = speed is { } s ? s * dt : float.MaxValue;
        var step = Math.Clamp(angle, -most, most);
        if (Math.Abs(step) > 1e-3f)
            animator.Turn += step * MathF.PI / 180;
        Turning = speed is { } shown ? (shown, source) : null;
    }

    private void Next()
    {
        while (true)
        {
            Index++;
            Turning = null;
            if (Steps == null || Index >= Steps.Count)
            {
                var played = Steps != null;
                // The simulation plays its next act straight on: the last pose is held until then, rather than idle for
                // a frame (the next clip blends from it)
                if (HoldWhenDone && played)
                    Stop();
                else
                    Idle();
                if (played)
                    Finished?.Invoke();
                return;
            }
            StepTime = 0;
            ClipFacingDone = false;
            TurningInPlace = false;
            if (Begin(Steps[Index], true))
                return;
        }
    }

    /// <summary>
    /// Puts the character where the last clip left it, ready for the next clip: facing the target at once when it has
    /// no turn speed of its own (it turns as it goes otherwise)
    /// </summary>
    private void Ready(LuaCharacter self)
    {
        if (OwnTurnSpeed == null)
            self.Face(TargetPoint());
        else
            self.Carry();
    }

    /// <summary>Starts a step (again, for a looping one); false when there is nothing to play for it (done, or no clip)</summary>
    private bool Begin(LuaActStep step, bool first)
    {
        var self = Self;
        if (self == null)
            return false;

        // (the step's clock is reset by Next, so a looping step restarted here keeps its life)
        switch (step.Kind)
        {
            case LuaStepKind.Clip:
                // The clip's TAE says how it turns: decided once it is playing (see Update)
                self.Carry();
                return PlayOrSkip(step, step.Animation);

            case LuaStepKind.Approach:
                if (Distance() <= step.Distance)
                    return false;
                Ready(self);
                return PlayOrSkip(step, step.Walk ? self.Locomotion.WalkFront : self.Locomotion.Run ?? self.Locomotion.WalkFront);

            case LuaStepKind.Leave:
                if (Distance() >= step.Distance)
                    return false;
                Ready(self);
                return PlayOrSkip(step, self.Locomotion.WalkBack);

            case LuaStepKind.Sideway:
                Ready(self);
                return PlayOrSkip(step, step.Right ? self.Locomotion.WalkRight : self.Locomotion.WalkLeft);

            case LuaStepKind.Turn:
                var angle = AngleToTarget();
                if (Math.Abs(angle) <= step.Angle)
                    return false;
                var left = angle > 0;
                var clip = Math.Abs(angle) > 135
                    ? left ? self.Locomotion.TurnLeft180 : self.Locomotion.TurnRight180
                    : left ? self.Locomotion.TurnLeft90 : self.Locomotion.TurnRight90;
                if (clip is { } c && self.Has(c) && self.Play(c))
                    return true;
                // No turn clip: turn on the spot at the character's own speed, else face the target straight away
                if (first && OwnTurnSpeed != null)
                {
                    self.Carry();
                    self.PlayLoop(self.Locomotion.Idle);
                    TurningInPlace = true;
                    return true;
                }
                self.Face(TargetPoint());
                return false;

            case LuaStepKind.Wait:
                self.Carry();
                self.PlayLoop(self.Locomotion.Idle);
                return true;
        }
        return false;
    }

    private bool PlayOrSkip(LuaActStep step, long? clip)
    {
        if (clip is { } c && Self is { } self && (self.IsLoading || self.Has(c)) && self.Play(c))
        {
            ClipStarted?.Invoke(c);
            return true;
        }
        Skipped.Add(LOC.Get("LUA_Viewer_Skipped_Step", step.Goal, clip is { } id ? id.ToString() : "-"));
        return false;
    }

    /// <summary>
    /// A clip step's turning: during the clip's SetTurnSpeed events at their speed (a negative one is taken to mean
    /// the character's own turnVellocity). A clip whose TAE has none (or isn't known) faces the target as it starts.
    /// </summary>
    private void TurnDuringClip(LuaCharacter self, float dt)
    {
        Turning = null;
        var play = Viewer.TimeAct.Current;

        // The clip's play is made by the TAE playback once the clip is showing (the frame after it starts)
        if (play == null || play.Character != self || play.AnimationId != self.CurrentClip)
            return;

        if (play.TurnEvents.Count == 0)
        {
            if (!ClipFacingDone)
                TurnToward(null, dt, "");
            ClipFacingDone = true;
            return;
        }

        ClipFacingDone = true;
        if (play.TurnSpeedAt(play.LastTime) is not { } speed || speed == 0)
            return;
        if (speed < 0)
        {
            if (OwnTurnSpeed is { } own)
                TurnToward(own, dt, LOC.Get("LUA_Turn_Source_Npc"));
            return;
        }
        TurnToward(speed, dt, LOC.Get("LUA_Turn_Source_Tae"));
    }

    public void Update(float dt)
    {
        var step = Current;
        var self = Self;
        if (step == null || self == null || self.IsLoading)
            return;

        StepTime += dt;

        switch (step.Kind)
        {
            case LuaStepKind.Clip:
                TurnDuringClip(self, dt);
                if (self.ClipEnded)
                    Next();
                break;

            case LuaStepKind.Turn:
                // Turning on the spot (no turn clip): until within the goal's angle, or its life runs out
                if (TurningInPlace)
                {
                    TurnToward(OwnTurnSpeed, dt, LOC.Get("LUA_Turn_Source_Npc"));
                    if (Math.Abs(AngleToTarget()) <= Math.Max(step.Angle, 1) || StepTime > Math.Clamp(step.Life, 1, 6))
                        Next();
                    break;
                }
                if (self.ClipEnded)
                {
                    self.Face(TargetPoint());
                    Next();
                }
                break;

            case LuaStepKind.Approach:
                if (OwnTurnSpeed is { } approachTurn)
                    TurnToward(approachTurn, dt, LOC.Get("LUA_Turn_Source_Npc"));
                if (Distance() <= step.Distance || StepTime > Math.Clamp(step.Life, 1, 15))
                    Next();
                else if (self.ClipEnded)
                    Begin(step, false);
                break;

            case LuaStepKind.Leave:
                if (OwnTurnSpeed is { } leaveTurn)
                    TurnToward(leaveTurn, dt, LOC.Get("LUA_Turn_Source_Npc"));
                if (Distance() >= step.Distance || StepTime > Math.Clamp(step.Life, 1, 10))
                    Next();
                else if (self.ClipEnded)
                    Begin(step, false);
                break;

            case LuaStepKind.Sideway:
                if (OwnTurnSpeed is { } sideTurn)
                    TurnToward(sideTurn, dt, LOC.Get("LUA_Turn_Source_Npc"));
                if (StepTime > Math.Clamp(step.Life, 1, 4))
                    Next();
                else if (self.ClipEnded)
                    Begin(step, false);
                break;

            case LuaStepKind.Wait:
                if (StepTime > Math.Clamp(step.Life, 0.5, 3))
                    Next();
                break;
        }
    }
}
