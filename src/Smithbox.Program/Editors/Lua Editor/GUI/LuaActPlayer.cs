#nullable enable
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Plays an act's steps (<see cref="LuaSituation.PlanAct"/>) on the viewer's character, toward its target:
/// - Clip: turns to face the target, then plays the animation once;
/// - Approach / Leave: loops walk or run (or walk back), re-aiming at the target each cycle, until within (or beyond)
///   the goal's distance, or its life runs out;
/// - Sideway: strafes left or right, facing the target, for the goal's life (1 to 4 s);
/// - Turn: plays a 90 or 180 degree turn clip if the target is outside the goal's angle, then faces it;
/// - Wait: stands for the goal's life (0.5 to 3 s).
/// When the steps run out, the character goes back to its idle, looping, instead of holding the last frame.
/// </summary>
public class LuaActPlayer
{
    private readonly LuaViewer Viewer;

    private List<LuaActStep>? Steps;
    private int Index = -1;
    private float StepTime;

    /// <summary>The step being played, for the status line; null when idle</summary>
    public LuaActStep? Current => Steps != null && Index >= 0 && Index < Steps.Count ? Steps[Index] : null;

    /// <summary>Steps skipped for want of a clip (named in the status line)</summary>
    public List<string> Skipped { get; } = new();

    public LuaActPlayer(LuaViewer viewer)
    {
        Viewer = viewer;
    }

    private LuaCharacter? Self => Viewer.Self;

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
    private Vector3 TargetPoint()
    {
        if (Viewer.Target != null)
            return Viewer.Target.Position;
        var forward = Self?.Forward ?? Vector3.UnitZ;
        return Vector3.Transform(forward, Matrix4x4.CreateRotationY(Viewer.TargetBearing * MathF.PI / 180)) * Viewer.TargetDistance;
    }

    private float Distance()
    {
        if (Self == null)
            return 0;
        var d = TargetPoint() - Self.Position;
        return new Vector2(d.X, d.Z).Length();
    }

    /// <summary>The target's angle from the character's front, in degrees: positive to the left</summary>
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

    private void Next()
    {
        while (true)
        {
            Index++;
            if (Steps == null || Index >= Steps.Count)
            {
                Idle();
                return;
            }
            StepTime = 0;
            if (Begin(Steps[Index]))
                return;
        }
    }

    /// <summary>Starts a step; false when there is nothing to play for it (done already, or no clip)</summary>
    private bool Begin(LuaActStep step)
    {
        var self = Self;
        if (self == null)
            return false;

        // (the step's clock is reset by Next, so a looping step restarted here keeps its life)
        switch (step.Kind)
        {
            case LuaStepKind.Clip:
                self.Face(TargetPoint());
                return PlayOrSkip(step, step.Animation);

            case LuaStepKind.Approach:
                if (Distance() <= step.Distance)
                    return false;
                self.Face(TargetPoint());
                return PlayOrSkip(step, step.Walk ? self.Locomotion.WalkFront : self.Locomotion.Run ?? self.Locomotion.WalkFront);

            case LuaStepKind.Leave:
                if (Distance() >= step.Distance)
                    return false;
                self.Face(TargetPoint());
                return PlayOrSkip(step, self.Locomotion.WalkBack);

            case LuaStepKind.Sideway:
                self.Face(TargetPoint());
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
                // No turn clip: face the target straight away
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
            return true;
        Skipped.Add(LOC.Get("LUA_Viewer_Skipped_Step", step.Goal, clip is { } id ? id.ToString() : "-"));
        return false;
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
                if (self.ClipEnded)
                    Next();
                break;

            case LuaStepKind.Turn:
                if (self.ClipEnded)
                {
                    self.Face(TargetPoint());
                    Next();
                }
                break;

            case LuaStepKind.Approach:
                if (Distance() <= step.Distance || StepTime > Math.Clamp(step.Life, 1, 15))
                    Next();
                else if (self.ClipEnded)
                    Begin(step);
                break;

            case LuaStepKind.Leave:
                if (Distance() >= step.Distance || StepTime > Math.Clamp(step.Life, 1, 10))
                    Next();
                else if (self.ClipEnded)
                    Begin(step);
                break;

            case LuaStepKind.Sideway:
                if (StepTime > Math.Clamp(step.Life, 1, 4))
                    Next();
                else if (self.ClipEnded)
                    Begin(step);
                break;

            case LuaStepKind.Wait:
                if (StepTime > Math.Clamp(step.Life, 0.5, 3))
                    Next();
                break;
        }
    }
}
