#nullable enable
namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// One interrupt that was fired: when (scene clock), what the goal's Interrupt did with it, and what fired it (by hand,
/// by the TAE, by the player's action in First Person Mode)
/// </summary>
public sealed record LuaInterruptFiring(float SceneTime, LuaInterruptResult Result, string Via);

/// <summary>
/// Fires the battle goal's Goal.Interrupt (see Claude/Documentation/Lua and AI.md (AI Simulation) §5):
/// <list type="bullet">
/// <item>by itself, when the character's TAE puts on a SpEffect the goal observes on TARGET_SELF
/// (AddObserveSpecialEffectAttribute): INTERUPT_ActivateSpecialEffect with that SpEffect;</item>
/// <item>by hand, from the AI Simulation window: any interrupt type the Interrupt tests, and any observed SpEffect
/// (on the character or the target), for what the simulation doesn't make happen (damage, the target attacking, …).</item>
/// </list>
/// A handled interrupt (Interrupt returned true) stops the playing act: its queued steps play instead, and the
/// simulation picks its next act when they have played (at once, when there are none).
/// </summary>
public sealed class LuaInterrupts
{
    private const int HistoryLimit = 30;
    public const string ActivateSpecialEffect = "ActivateSpecialEffect";
    private const string TargetSelf = "TARGET_SELF";

    private readonly LuaViewer Viewer;

    private LuaBattleGoal? Goal;
    private LuaSituation? Situation;

    private const string TargetEnemy = "TARGET_ENE_0";

    // SpEffects put on since the last update, on the character (by its TAE) or the target (by the player): fired on
    // the update, not from inside the TAE's or the player's
    private readonly Queue<(int Id, bool OnTarget)> Activated = new();

    /// <summary>The interrupts fired, newest last</summary>
    public List<LuaInterruptFiring> History { get; } = new();

    /// <summary>Bumped with each firing, for the window to follow</summary>
    public int Version { get; private set; }

    public LuaInterrupts(LuaViewer viewer)
    {
        Viewer = viewer;
        viewer.TimeAct.SpEffectActivated += id => Activated.Enqueue((id, false));
        viewer.FirstPerson.TargetSpEffectActivated += id => Activated.Enqueue((id, true));
    }

    /// <summary>The goal whose Interrupt runs, with the situation it reads (null: none)</summary>
    public void Bind(LuaBattleGoal? goal, LuaSituation? situation)
    {
        Goal = goal;
        Situation = situation;
        Activated.Clear();
    }

    public bool HasInterrupt => Goal?.Interrupt != null && Situation != null;

    /// <summary>The interrupt types the goal's Interrupt tests (without the INTERUPT_ prefix)</summary>
    public IEnumerable<string> TestedTypes => Goal?.InterruptTypes.Select(t => t.Type) ?? Enumerable.Empty<string>();

    /// <summary>Whether the goal observes a SpEffect on someone (TARGET_SELF, TARGET_ENE_0)</summary>
    private bool Observes(string target, int id) => Goal?.ObservedSpEffects.Any(o => o.Target == target && o.Id == id) == true;

    public void Update()
    {
        while (Activated.Count > 0)
        {
            var (id, onTarget) = Activated.Dequeue();
            if (HasInterrupt && Observes(onTarget ? TargetEnemy : TargetSelf, id))
                Fire(ActivateSpecialEffect, id, LOC.Get(onTarget ? "LUA_Interrupt_By_Target_SpEffect" : "LUA_Interrupt_Auto"));
        }
    }

    /// <summary>Runs the Interrupt for an interrupt, and plays its reaction when it takes it</summary>
    /// <param name="type">The INTERUPT_ type without the prefix</param>
    /// <param name="via">What fired it, for the list (null: by hand)</param>
    public LuaInterruptResult? Fire(string type, int? spEffect, string? via = null)
    {
        if (!HasInterrupt || Viewer.Self == null)
            return null;

        var result = Situation!.RunInterrupt(type, spEffect, Viewer.CurrentDistance());
        History.Add(new LuaInterruptFiring(Viewer.TimeAct.SceneTime, result, via ?? LOC.Get("LUA_Interrupt_Manual")));
        if (History.Count > HistoryLimit)
            History.RemoveAt(0);
        Version++;

        if (!result.Handled)
            return result;

        // Taken: the act stops, and what the Interrupt queued plays instead
        var steps = result.Steps.Where(s => s.Kind != LuaStepKind.Other).ToList();
        if (steps.Count > 0)
        {
            Viewer.Simulation.Interrupted(true);
            Viewer.Player.Play(steps);
        }
        else if (Viewer.Simulation.Running)
        {
            // The simulation picks its next act at once: the pose is held until then (it blends from it)
            Viewer.Player.Stop();
            Viewer.Simulation.Interrupted(false);
        }
        else
        {
            Viewer.Player.Idle();
        }
        return result;
    }

    public void Clear()
    {
        History.Clear();
        Activated.Clear();
        Version++;
    }
}
