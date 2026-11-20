#nullable enable
namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Runs a battle goal as the game does, until stopped: Goal.Activate is evaluated, Common_Battle_Activate picks an
/// act at random by weight, the act plays (its function's sub-goals, then the after function's), and when it has
/// finished the goal starts over from Activate with the situation as it now is. Each cycle the inputs that come from
/// the scene are set from it: ai:GetDist(TARGET_ENE_0) from where the characters stand (the target can be moved with
/// the gizmo while it runs), and a SetCoolTime input from whether its animation played within its seconds. The other
/// inputs keep the values the user set.
/// </summary>
public class LuaSimulation
{
    private readonly LuaViewer Viewer;
    private readonly Random Rng = new();

    private LuaBattleGoal? Goal;
    private LuaSituation? Situation;

    public bool Running { get; private set; }

    /// <summary>The act playing, and the chance it was picked with</summary>
    public (int Act, double Percent)? Current { get; private set; }

    /// <summary>The acts picked so far, most recent last (kept to the last 50)</summary>
    public List<(int Act, double Percent)> History { get; } = new();

    // The simulation's clock, and when each animation last started (for cooldowns)
    private float Clock;
    private readonly Dictionary<long, float> LastPlayed = new();

    // Start the next cycle on the next frame (never from inside the player's callback)
    private bool NextPending;
    private float WaitUntil;

    public LuaSimulation(LuaViewer viewer)
    {
        Viewer = viewer;
        Viewer.Player.Finished += () =>
        {
            if (Running)
                NextPending = true;
        };
        Viewer.Player.ClipStarted += id => LastPlayed[id] = Clock;
    }

    public void Start(LuaBattleGoal goal, LuaSituation situation)
    {
        Goal = goal;
        situation.Scene = Viewer;
        situation.Live = true;
        Situation = situation;
        Running = true;
        History.Clear();
        LastPlayed.Clear();
        Clock = 0;
        WaitUntil = 0;
        NextPending = true;
    }

    public void Stop()
    {
        if (!Running)
            return;
        Running = false;
        if (Situation != null)
            Situation.Live = false;
        Current = null;
        NextPending = false;
        Viewer.Player.Idle();
    }

    /// <summary>Sets the inputs that come from the scene (distance, cooldowns)</summary>
    public void ApplyLive(LuaSituation situation)
    {
        foreach (var input in situation.Inputs)
        {
            if (input.SceneQuery != null && situation.SceneValue(input.SceneQuery) is { } value)
            {
                if (value is bool b)
                    input.Boolean = b;
                else
                    input.Number = Convert.ToDouble(value);
            }
            if (input.CooldownAnimation is { } anim)
                input.Boolean = LastPlayed.TryGetValue(anim, out var at) && Clock - at < (input.CooldownSeconds ?? 0);
        }
    }

    public void Update(float dt)
    {
        if (!Running)
            return;
        Clock += dt;

        if (NextPending && Clock >= WaitUntil)
        {
            NextPending = false;
            Cycle();
        }
    }

    private void Cycle()
    {
        if (Goal == null || Situation == null || Viewer.Self == null)
        {
            Stop();
            return;
        }

        ApplyLive(Situation);
        var result = Situation.Evaluate();
        var percent = LuaSituation.Percentages(result);

        // Common_Battle_Activate: an act at random, in proportion to its weight
        var total = result.Weights.Values.Where(w => w > 0).Sum();
        if (total <= 0)
        {
            Current = null;
            Viewer.Status = LOC.Get("LUA_Simulation_No_Weight");
            Wait(1);
            return;
        }

        var roll = Rng.NextDouble() * total;
        var act = result.Weights.Where(w => w.Value > 0).Last().Key;
        foreach (var (id, w) in result.Weights)
        {
            if (w <= 0)
                continue;
            if (roll < w)
            {
                act = id;
                break;
            }
            roll -= w;
        }

        Current = (act, percent.GetValueOrDefault(act));
        History.Add(Current.Value);
        if (History.Count > 50)
            History.RemoveAt(0);
        Viewer.Status = null;

        // The act's sub-goals, then the after function's (the game queues both when the goal activates)
        var distance = Viewer.CurrentDistance();
        var steps = Goal.Acts.TryGetValue(act, out var picked) ? Situation.PlanAct(picked, distance) : new List<LuaActStep>();
        steps.AddRange(Situation.PlanFunction(Goal.AfterFunctionNode, distance));

        if (steps.All(s => s.Kind == LuaStepKind.Other))
        {
            // Nothing to show: count it as a short act, so the loop moves on without spinning
            Wait(0.5f);
            return;
        }

        Viewer.Player.Play(steps);
    }

    private void Wait(float seconds)
    {
        WaitUntil = Clock + seconds;
        NextPending = true;
    }
}
