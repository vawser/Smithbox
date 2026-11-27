using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// What a Demon's Souls ESD was read from: the DLSE file, and which <see cref="ESD"/> objects came from which of its
/// objects. Kept with the file so a save can restore what <see cref="ESD"/> doesn't hold (the machines' transition
/// list order, shared commands, the header value and the tail value).
/// </summary>
public sealed class EsdDlseSource
{
    public ESDDLSE Original;
    internal readonly Dictionary<ESD.Condition, ESDDLSE.Transition> ConditionOrigin = new(ReferenceEqualityComparer.Instance);
    internal readonly Dictionary<ESD.CommandCall, ESDDLSE.ExternalEvent> CommandOrigin = new(ReferenceEqualityComparer.Instance);
}

/// <summary>What DES can't hold (<see cref="EsdDlseConvert.Problems"/>); the editor shows ESD_Des_Problem_&lt;kind&gt;</summary>
public enum EsdDlseProblemKind
{
    /// A machine or state id beyond 32 bits (detail: the id)
    IdTooLarge,
    /// DES states have no While list
    WhileCommands,
    /// A command bank other than 1: DES has no machine calls (6), returns (7) or bank 5 (detail: the bank)
    CommandBank,
    /// More than 6 arguments (detail: the count)
    TooManyArgs,
    /// A condition with subconditions that also has pass commands
    SubconditionsWithPassCommands,
    /// A condition with subconditions that also has a target state
    SubconditionsWithTarget,
    /// A target state that isn't in the machine (detail: the state id)
    TargetOutsideMachine,
}

/// <summary>One thing DES can't hold, where it is (machine, and state if any), and a number about it</summary>
public sealed record EsdDlseProblem(EsdDlseProblemKind Kind, long Machine, long? State, long Detail)
{
    public override string ToString() => $"machine {Machine}{(State is long s ? $" state {s}" : "")}: {Kind switch
    {
        EsdDlseProblemKind.IdTooLarge => $"the id {Detail} doesn't fit 32 bits",
        EsdDlseProblemKind.WhileCommands => "DES has no While commands",
        EsdDlseProblemKind.CommandBank => $"a bank {Detail} command (DES commands have no bank)",
        EsdDlseProblemKind.TooManyArgs => $"a command with {Detail} arguments (DES allows {ESDDLSE.MaxEventArgs})",
        EsdDlseProblemKind.SubconditionsWithPassCommands => "a condition with subconditions can't have pass commands in DES",
        EsdDlseProblemKind.SubconditionsWithTarget => "a condition with subconditions can't have a target state in DES",
        EsdDlseProblemKind.TargetOutsideMachine => $"leads to state {Detail}, which isn't in the machine",
        _ => Kind.ToString(),
    }}";
}

/// <summary>
/// Converts Demon's Souls ESDs (<see cref="ESDDLSE"/>) to and from the <see cref="ESD"/> model the editor works on.
/// Machines → state groups, states → states (entry / exit; no While list in DES), transitions → conditions, commands
/// → bank 1 command calls. A transition with host events becomes a condition with subconditions: its own target is
/// the first, each host event's target the next (the null-target host event that ends the list isn't kept).
/// Unedited, <see cref="FromEsd"/> of <see cref="ToEsd"/> writes the original bytes. Pure: compiled into
/// Claude/Tools/EsdSurvey too.
/// </summary>
public static class EsdDlseConvert
{
    #region To ESD

    public static ESD ToEsd(ESDDLSE file, out EsdDlseSource source)
    {
        source = new EsdDlseSource { Original = file };
        var src = source;
        var esd = new ESD(false, 1);
        foreach (var map in file.Maps)
        {
            if (esd.StateGroups.ContainsKey(map.Id))
                throw new NotSupportedException($"DES ESD: machine {map.Id} appears twice.");
            var group = new Dictionary<long, ESD.State>();
            esd.StateGroups[map.Id] = group;
            var inMap = new HashSet<ESDDLSE.MapState>(map.States, ReferenceEqualityComparer.Instance);
            foreach (var s in map.States)
            {
                if (group.ContainsKey(s.Id))
                    throw new NotSupportedException($"DES ESD: machine {map.Id} has state {s.Id} twice.");
                group[s.Id] = new ESD.State();
            }

            var conditions = new Dictionary<ESDDLSE.Transition, ESD.Condition>(ReferenceEqualityComparer.Instance);
            ESD.Condition Condition(ESDDLSE.Transition t)
            {
                if (conditions.TryGetValue(t, out var known))
                    return known;
                var c = new ESD.Condition { Evaluator = t.Evaluator.ToArray() };
                conditions[t] = c;
                src.ConditionOrigin[c] = t;
                var hosts = t.PassEvents.OfType<ESDDLSE.HostEvent>().ToList();
                if (hosts.Count > 0)
                {
                    // subconditions: own target first, then each host event's target; the last host event ends it
                    if (hosts.Count != t.PassEvents.Count || hosts[^1].Target != null || hosts.Take(hosts.Count - 1).Any(h => h.Target is not ESDDLSE.Transition)
                        || t.Target is not ESDDLSE.Transition first)
                        throw new NotSupportedException($"DES ESD: machine {map.Id} has a subcondition list of an unknown shape.");
                    c.TargetState = null;
                    c.Subconditions.Add(Condition(first));
                    foreach (var h in hosts.Take(hosts.Count - 1))
                        c.Subconditions.Add(Condition((ESDDLSE.Transition)h.Target));
                    return c;
                }
                c.PassCommands.AddRange(t.PassEvents.Select(Command));
                c.TargetState = t.Target switch
                {
                    null => null,
                    ESDDLSE.MapState s when inMap.Contains(s) => s.Id,
                    ESDDLSE.MapState s => throw new NotSupportedException($"DES ESD: machine {map.Id} leads to state {s.Id} of another machine."),
                    _ => throw new NotSupportedException($"DES ESD: machine {map.Id} has a transition leading to a transition without host events."),
                };
                return c;
            }

            foreach (var s in map.States)
            {
                var state = group[s.Id];
                state.EntryCommands.AddRange(s.EntryEvents.Select(Command));
                state.ExitCommands.AddRange(s.ExitEvents.Select(Command));
                state.Conditions.AddRange(s.Transitions.Select(Condition));
            }
            // subconditions only the machine's list names, so they're linked too
            foreach (var t in map.Transitions)
                Condition(t);
        }
        return esd;

        ESD.CommandCall Command(ESDDLSE.Event e)
        {
            if (e is not ESDDLSE.ExternalEvent ev)
                throw new NotSupportedException("DES ESD: a host event outside a subcondition list.");
            // one CommandCall per list entry, even where DES shares the command: the editor edits commands in place
            var call = new ESD.CommandCall(1, ev.CommandId, ev.Args.Select(a => a.ToArray()).ToArray());
            src.CommandOrigin[call] = ev;
            return call;
        }
    }

    #endregion

    #region Checks

    /// <summary>What in this ESD can't be written as a DES ESD, or an empty list</summary>
    public static List<EsdDlseProblem> Problems(ESD esd)
    {
        var problems = new List<EsdDlseProblem>();
        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        foreach (var (gid, group) in esd.StateGroups)
        {
            if (gid < int.MinValue || gid > int.MaxValue) problems.Add(new(EsdDlseProblemKind.IdTooLarge, gid, null, gid));
            foreach (var (sid, state) in group)
            {
                void Add(EsdDlseProblemKind kind, long detail = 0) => problems.Add(new(kind, gid, sid, detail));
                if (sid < int.MinValue || sid > int.MaxValue) Add(EsdDlseProblemKind.IdTooLarge, sid);
                if (state.WhileCommands.Count > 0) Add(EsdDlseProblemKind.WhileCommands);
                foreach (var c in state.EntryCommands.Concat(state.ExitCommands)) CheckCommand(c, Add);
                foreach (var c in state.Conditions) CheckCondition(c, group, Add);
            }
        }
        return problems;

        void CheckCommand(ESD.CommandCall c, Action<EsdDlseProblemKind, long> add)
        {
            if (c.CommandBank != 1) add(EsdDlseProblemKind.CommandBank, c.CommandBank);
            if (c.Arguments.Count > ESDDLSE.MaxEventArgs) add(EsdDlseProblemKind.TooManyArgs, c.Arguments.Count);
        }

        void CheckCondition(ESD.Condition c, Dictionary<long, ESD.State> group, Action<EsdDlseProblemKind, long> add)
        {
            if (!seen.Add(c)) return;
            if (c.Subconditions.Count > 0)
            {
                if (c.PassCommands.Count > 0) add(EsdDlseProblemKind.SubconditionsWithPassCommands, 0);
                if (c.TargetState != null) add(EsdDlseProblemKind.SubconditionsWithTarget, 0);
                foreach (var s in c.Subconditions) CheckCondition(s, group, add);
                return;
            }
            if (c.TargetState is long target && !group.ContainsKey(target)) add(EsdDlseProblemKind.TargetOutsideMachine, target);
            foreach (var p in c.PassCommands) CheckCommand(p, add);
        }
    }

    /// <summary>
    /// Whether the condition can take a subcondition in DES: it has some already, or it has neither a target nor pass
    /// commands (DES stores a subcondition list in the target and pass list's place)
    /// </summary>
    public static bool CanHoldSubconditions(ESD.Condition c) =>
        c.Subconditions.Count > 0 || c.TargetState == null && c.PassCommands.Count == 0;

    #endregion

    #region From ESD

    /// <summary>
    /// The DES file for this ESD. With the source it was read from, what's unchanged is written as it was read;
    /// without (a new file), the header value is estimated.
    /// </summary>
    public static ESDDLSE FromEsd(ESD esd, EsdDlseSource source)
    {
        var problems = Problems(esd);
        if (problems.Count > 0)
            throw new InvalidOperationException($"DES ESD: {problems[0]}{(problems.Count > 1 ? $" (and {problems.Count - 1} more)" : "")}.");

        var original = source?.Original;
        var file = new ESDDLSE { TailValue = original?.TailValue.ToArray() ?? new byte[8] };
        var originToCondition = new Dictionary<ESDDLSE.Transition, ESD.Condition>(ReferenceEqualityComparer.Instance);
        if (source != null)
            foreach (var (c, t) in source.ConditionOrigin) originToCondition.TryAdd(t, c);
        // unchanged commands that DES shared are shared again: one new event per original event
        var sharedEvents = new Dictionary<ESDDLSE.ExternalEvent, ESDDLSE.ExternalEvent>(ReferenceEqualityComparer.Instance);

        foreach (var (gid, group) in esd.StateGroups)
        {
            var map = new ESDDLSE.Map { Id = (int)gid };
            var states = new Dictionary<long, ESDDLSE.MapState>();
            foreach (var (sid, _) in group)
            {
                var s = new ESDDLSE.MapState { Id = (int)sid };
                states[sid] = s;
                map.States.Add(s);
            }
            // vanilla: the initial state is the first listed
            map.Initial = map.States.FirstOrDefault();

            var transitions = new Dictionary<ESD.Condition, ESDDLSE.Transition>(ReferenceEqualityComparer.Instance);
            var reached = new List<ESDDLSE.Transition>();
            ESDDLSE.Transition Transition(ESD.Condition c)
            {
                if (transitions.TryGetValue(c, out var known))
                    return known;
                var t = new ESDDLSE.Transition { Evaluator = c.Evaluator.ToArray() };
                transitions[c] = t;
                reached.Add(t);
                if (c.Subconditions.Count > 0)
                {
                    t.Target = Transition(c.Subconditions[0]);
                    foreach (var sub in c.Subconditions.Skip(1))
                        t.PassEvents.Add(new ESDDLSE.HostEvent { Target = Transition(sub) });
                    t.PassEvents.Add(new ESDDLSE.HostEvent { Target = null });
                }
                else
                {
                    t.PassEvents.AddRange(c.PassCommands.Select(Event));
                    t.Target = c.TargetState is long target ? states[target] : null;
                }
                return t;
            }

            foreach (var (sid, st) in group)
            {
                var s = states[sid];
                s.EntryEvents.AddRange(st.EntryCommands.Select(Event));
                s.ExitEvents.AddRange(st.ExitCommands.Select(Event));
                s.Transitions.AddRange(st.Conditions.Select(Transition));
            }

            // the machine's transition list: the original order for what still exists, then new ones as reached
            var originalMap = original?.Maps.FirstOrDefault(m => m.Id == map.Id);
            var listed = new HashSet<ESDDLSE.Transition>(ReferenceEqualityComparer.Instance);
            if (originalMap != null)
                foreach (var ot in originalMap.Transitions)
                    if (originToCondition.TryGetValue(ot, out var c) && transitions.TryGetValue(c, out var t) && listed.Add(t))
                        map.Transitions.Add(t);
            foreach (var t in reached)
                if (listed.Add(t))
                    map.Transitions.Add(t);

            file.Maps.Add(map);
        }

        // Header A looks like an in-memory size (EsdSurvey dlsea): never lower it; raise it for added objects
        if (original == null)
            file.HeaderA = RoundUp16(EstimateHeaderA(file) * 1.1);
        else
        {
            var growth = EstimateHeaderA(file) - EstimateHeaderA(original);
            file.HeaderA = growth > 0 ? original.HeaderA + RoundUp16(growth * 1.1) : original.HeaderA;
        }
        return file;

        ESDDLSE.Event Event(ESD.CommandCall c)
        {
            if (source != null && source.CommandOrigin.TryGetValue(c, out var ev) && Same(c, ev))
            {
                if (!sharedEvents.TryGetValue(ev, out var copy))
                    sharedEvents[ev] = copy = new ESDDLSE.ExternalEvent { CommandId = ev.CommandId, Args = ev.Args.Select(a => a.ToArray()).ToList() };
                return copy;
            }
            return new ESDDLSE.ExternalEvent { CommandId = c.CommandID, Args = c.Arguments.Select(a => a.ToArray()).ToList() };
        }
    }

    static bool Same(ESD.CommandCall c, ESDDLSE.ExternalEvent ev) =>
        c.CommandBank == 1 && c.CommandID == ev.CommandId && c.Arguments.Count == ev.Args.Count
        && c.Arguments.Zip(ev.Args).All(p => p.First.AsSpan().SequenceEqual(p.Second));

    static uint RoundUp16(double v) => (uint)(Math.Ceiling(v / 16) * 16);

    /// <summary>
    /// The least-squares estimate of header A (EsdSurvey dlsea, max error 58 bytes over the vanilla files): per
    /// machine, state, transition, host event, command and bytecode byte
    /// </summary>
    public static double EstimateHeaderA(ESDDLSE file)
    {
        var transitions = new HashSet<ESDDLSE.Transition>(ReferenceEqualityComparer.Instance);
        var events = new HashSet<ESDDLSE.Event>(ReferenceEqualityComparer.Instance);
        int states = 0;
        foreach (var m in file.Maps)
        {
            states += m.States.Count;
            foreach (var s in m.States)
            {
                foreach (var e in s.EntryEvents.Concat(s.ExitEvents)) events.Add(e);
                foreach (var t in s.Transitions) transitions.Add(t);
            }
            foreach (var t in m.Transitions) transitions.Add(t);
        }
        foreach (var t in transitions)
            foreach (var e in t.PassEvents) events.Add(e);
        var commands = events.OfType<ESDDLSE.ExternalEvent>().ToList();
        long bytecode = transitions.Sum(t => (long)t.Evaluator.Length) + commands.Sum(e => e.Args.Sum(a => (long)a.Length));
        return -65 + 106 * file.Maps.Count + 57 * states + 32 * transitions.Count + 8.5 * events.Count(e => e is ESDDLSE.HostEvent)
            + 62 * commands.Count + bytecode;
    }

    #endregion
}
