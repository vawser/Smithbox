using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>Replaces a list's contents, and puts the old contents back on undo</summary>
public class EsdListContentOp<T> : IEsdOp
{
    private readonly List<T> List;
    private readonly List<T> NewItems;
    private List<T> Old;

    public EsdListContentOp(List<T> list, IEnumerable<T> items)
    {
        List = list;
        NewItems = items.ToList();
    }

    public void Do() { Old = List.ToList(); List.Clear(); List.AddRange(NewItems); }
    public void Undo() { List.Clear(); List.AddRange(Old); }
}

/// <summary>Moves an item from one list to another (a command dragged to another state or transition)</summary>
public class EsdMoveBetweenOp<T> : IEsdOp
{
    private readonly List<T> From;
    private readonly int FromIndex;
    private readonly List<T> To;
    private readonly int ToIndex;
    private T Item;
    private int Placed;

    /// <param name="toIndex">Index in the target list after the item is taken out of the source</param>
    public EsdMoveBetweenOp(List<T> from, int fromIndex, List<T> to, int toIndex)
    {
        From = from;
        FromIndex = fromIndex;
        To = to;
        ToIndex = toIndex;
    }

    public void Do()
    {
        Item = From[FromIndex];
        From.RemoveAt(FromIndex);
        Placed = Math.Clamp(ToIndex, 0, To.Count);
        To.Insert(Placed, Item);
    }

    public void Undo()
    {
        To.RemoveAt(Placed);
        From.Insert(FromIndex, Item);
    }
}

/// <summary>
/// Adds or removes an entry of a keyed dictionary (a file's machines), rebuilding it in its old order: the writer
/// writes in dictionary order, so undo must give the same order back (as EsdStateOp does for states).
/// </summary>
public class EsdKeyedOp<T> : IEsdOp
{
    private readonly Dictionary<long, T> Dict;
    private readonly long Id;
    private readonly T Value;
    private readonly bool Add;
    private List<KeyValuePair<long, T>> Before;

    private EsdKeyedOp(Dictionary<long, T> dict, long id, T value, bool add)
    {
        Dict = dict;
        Id = id;
        Value = value;
        Add = add;
    }

    /// <summary>Adds an entry at the end</summary>
    public static EsdKeyedOp<T> Adding(Dictionary<long, T> dict, long id, T value) => new(dict, id, value, true);
    public static EsdKeyedOp<T> Removing(Dictionary<long, T> dict, long id) => new(dict, id, default, false);

    public void Do()
    {
        Before = Dict.ToList();
        var after = Add ? Before.Append(new(Id, Value)).ToList() : Before.Where(kv => kv.Key != Id).ToList();
        Rebuild(after);
    }

    public void Undo() => Rebuild(Before);

    private void Rebuild(List<KeyValuePair<long, T>> entries)
    {
        Dict.Clear();
        Dict.TrimExcess();
        foreach (var kv in entries)
            Dict.Add(kv.Key, kv.Value);
    }
}

/// <summary>An edit's op, or why it can't be made</summary>
public readonly record struct EsdEditPlan(IEsdOp Op, string Refusal, long NewId = -1, ESD.State NewState = null)
{
    public static EsdEditPlan Refuse(string why) => new(null, why);
}

/// <summary>
/// P4's structural edits (Claude/Documentation/ESD Block Editor Analysis.md §8): edits that add or remove states and
/// machines while keeping every transition going somewhere that exists. Each returns one op built from EsdEdits'
/// reversible ops, or a refusal (a LOC key the editor shows). Gated by EsdSurvey structedit: on every vanilla machine,
/// done → written and read back as edited, no new problems, no new unreached states, the code view still valid;
/// undone → byte-identical file.
/// Pure: compiled into Claude/Tools/EsdSurvey.
/// </summary>
public static class EsdStructureEdit
{
    // --- states ---

    /// <summary>
    /// A new empty state after S in the flow: S's transitions (and its while / exit commands, which run while it waits
    /// and as it leaves) move to the new state, and S goes to it always. Where S went before, the new state goes now,
    /// so nothing changes until commands or a test are added.
    /// </summary>
    public static EsdEditPlan InsertAfter(Dictionary<long, ESD.State> machine, long s)
    {
        if (!machine.TryGetValue(s, out var state))
            return EsdEditPlan.Refuse("ESD_Edit_No_State");

        var id = EsdEdits.NextStateId(machine);
        var added = new ESD.State();
        added.Conditions.AddRange(state.Conditions);
        added.WhileCommands.AddRange(state.WhileCommands);
        added.ExitCommands.AddRange(state.ExitCommands);

        var op = new EsdCompoundOp(new List<IEsdOp>
        {
            EsdStateOp.AddState(machine, id, added, s),
            new EsdListContentOp<ESD.Condition>(state.Conditions, new[] { new ESD.Condition(id, EsdEdits.Always) }),
            new EsdListContentOp<ESD.CommandCall>(state.WhileCommands, Array.Empty<ESD.CommandCall>()),
            new EsdListContentOp<ESD.CommandCall>(state.ExitCommands, Array.Empty<ESD.CommandCall>()),
        });
        return new EsdEditPlan(op, null, id, added);
    }

    /// <summary>
    /// A new "if" branch in S: a transition (never, until its test is set) to a new state that goes on to where S's
    /// final "always" goes. Inserted just before that "always", so it reads "else if" (or "if"). Needs S to end with
    /// an "always" that has a target (a state that decides at once, not one that waits).
    /// </summary>
    public static EsdEditPlan AddBranch(Dictionary<long, ESD.State> machine, long s)
    {
        if (!machine.TryGetValue(s, out var state))
            return EsdEditPlan.Refuse("ESD_Edit_No_State");

        var last = state.Conditions.Count > 0 ? state.Conditions[^1] : null;
        if (last == null || !EsdExpr.IsAlways(last.Evaluator) || last.TargetState is not long join)
            return EsdEditPlan.Refuse("ESD_Edit_Branch_Refused");

        var id = EsdEdits.NextStateId(machine);
        var arm = new ESD.State();
        arm.Conditions.Add(new ESD.Condition(join, EsdEdits.Always));

        var op = new EsdCompoundOp(new List<IEsdOp>
        {
            EsdStateOp.AddState(machine, id, arm, s),
            new EsdInsertOp<ESD.Condition>(state.Conditions, state.Conditions.Count - 1, new ESD.Condition(id, EsdEdits.Never)),
        });
        return new EsdEditPlan(op, null, id, arm);
    }

    /// <summary>
    /// A new state after S that calls a machine of the file and waits for it: its entry is the call (0 for every arg
    /// the callee reads), its one transition the "call done" test, holding S's old transitions as subconditions.
    /// </summary>
    public static EsdEditPlan InsertCall(ESD esd, Dictionary<long, ESD.State> machine, long s, long callee)
    {
        if (!machine.TryGetValue(s, out var state))
            return EsdEditPlan.Refuse("ESD_Edit_No_State");
        if (!esd.StateGroups.TryGetValue(callee, out var target))
            return EsdEditPlan.Refuse("ESD_Edit_No_Machine");
        if (callee < int.MinValue || callee > int.MaxValue)
            return EsdEditPlan.Refuse("ESD_Edit_No_Machine");

        var argc = EsdStructure.ArgCount(target);
        var call = new ESD.CommandCall(6, (int)callee, Enumerable.Range(0, argc).Select(_ => EsdEdits.DefaultArg("int")).ToArray());

        var id = EsdEdits.NextStateId(machine);
        var added = new ESD.State();
        added.EntryCommands.Add(call);
        var done = new ESD.Condition { Evaluator = EsdStructure.CallDoneCode.ToArray() };
        done.Subconditions.AddRange(state.Conditions);
        added.WhileCommands.AddRange(state.WhileCommands);
        added.ExitCommands.AddRange(state.ExitCommands);
        added.Conditions.Add(done);

        var op = new EsdCompoundOp(new List<IEsdOp>
        {
            EsdStateOp.AddState(machine, id, added, s),
            new EsdListContentOp<ESD.Condition>(state.Conditions, new[] { new ESD.Condition(id, EsdEdits.Always) }),
            new EsdListContentOp<ESD.CommandCall>(state.WhileCommands, Array.Empty<ESD.CommandCall>()),
            new EsdListContentOp<ESD.CommandCall>(state.ExitCommands, Array.Empty<ESD.CommandCall>()),
        });
        return new EsdEditPlan(op, null, id, added);
    }

    /// <summary>
    /// Takes S out of the flow: every transition to S goes where S went instead, and S is deleted (its commands with
    /// it). Needs S to have one transition, "always", with a target other than itself and nothing inside, and not to
    /// be where the machine starts.
    /// </summary>
    public static EsdEditPlan RemoveStep(Dictionary<long, ESD.State> machine, long s)
    {
        if (!machine.TryGetValue(s, out var state))
            return EsdEditPlan.Refuse("ESD_Edit_No_State");
        if (s == machine.Keys.Min())
            return EsdEditPlan.Refuse("ESD_Edit_Delete_Start_Refused");

        var only = state.Conditions.Count == 1 ? state.Conditions[0] : null;
        if (only == null || !EsdExpr.IsAlways(only.Evaluator) || only.TargetState is not long next || next == s
            || only.PassCommands.Count > 0 || only.Subconditions.Count > 0)
            return EsdEditPlan.Refuse("ESD_Edit_Remove_Step_Refused");

        var ops = new List<IEsdOp>();
        foreach (var c in Conditions(machine).Where(c => c.TargetState == s))
            ops.Add(new EsdSetOp<long?>(() => c.TargetState, v => c.TargetState = v, next));
        ops.Add(EsdStateOp.RemoveState(machine, s));
        return new EsdEditPlan(new EsdCompoundOp(ops), null, next);
    }

    /// <summary>Every condition of a machine once (shared ones once), states' and subconditions' alike</summary>
    public static List<ESD.Condition> Conditions(Dictionary<long, ESD.State> machine)
    {
        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        var result = new List<ESD.Condition>();
        var stack = new Stack<ESD.Condition>();
        foreach (var st in machine.Values)
            foreach (var c in st.Conditions)
                stack.Push(c);
        while (stack.Count > 0)
        {
            var c = stack.Pop();
            if (!seen.Add(c))
                continue;
            result.Add(c);
            foreach (var sc in c.Subconditions)
                stack.Push(sc);
        }
        return result;
    }

    // --- commands ---

    /// <summary>A command moved to another list (or another place in its own), as a drag and drop makes it</summary>
    public static EsdEditPlan MoveCommand(List<ESD.CommandCall> from, int index, List<ESD.CommandCall> to, int toIndex)
    {
        if (index < 0 || index >= from.Count)
            return EsdEditPlan.Refuse("ESD_Edit_No_Command");
        if (ReferenceEquals(from, to))
        {
            // toIndex counts the list as it is before the move: dropping on an item puts the command before it
            var target = toIndex > index ? toIndex - 1 : toIndex;
            target = Math.Clamp(target, 0, from.Count - 1);
            return target == index ? EsdEditPlan.Refuse("ESD_Edit_No_Change") : new EsdEditPlan(new EsdMoveOp<ESD.CommandCall>(from, index, target), null);
        }
        return new EsdEditPlan(new EsdMoveBetweenOp<ESD.CommandCall>(from, index, to, toIndex), null);
    }

    // --- machines ---

    /// <summary>The next free sub-machine id (x&lt;n&gt; one past the highest used), or null when the range is full</summary>
    public static long? NextSubMachineId(ESD esd)
    {
        var used = esd.StateGroups.Keys.Where(EsdLayout.IsSubMachine).Select(k => EsdLayout.SubMachineTop - k).DefaultIfEmpty(-1).Max();
        var n = used + 1;
        return n < EsdLayout.SubMachineRange ? EsdLayout.SubMachineTop - n : null;
    }

    /// <summary>The next free entry-machine id: one past the highest that isn't a sub-machine</summary>
    public static long NextEntryMachineId(ESD esd)
    {
        var top = esd.StateGroups.Keys.Where(k => !EsdLayout.IsSubMachine(k)).DefaultIfEmpty(0).Max();
        return top + 1;
    }

    /// <summary>
    /// A new sub-machine: state 0 goes always to state 1, which returns 0 (the shape of vanilla's smallest
    /// functions).
    /// </summary>
    public static EsdEditPlan NewMachine(ESD esd)
    {
        if (NextSubMachineId(esd) is not long id)
            return EsdEditPlan.Refuse("ESD_Edit_Machine_Range_Full");

        var machine = new Dictionary<long, ESD.State>();
        var start = new ESD.State();
        start.Conditions.Add(new ESD.Condition(1, EsdEdits.Always));
        var end = new ESD.State();
        var ret = new ESD.Condition { Evaluator = EsdEdits.Always };
        ret.PassCommands.Add(new ESD.CommandCall(7, -1, EsdEdits.DefaultArg("int")));
        end.Conditions.Add(ret);
        machine[0] = start;
        machine[1] = end;

        return new EsdEditPlan(EsdKeyedOp<Dictionary<long, ESD.State>>.Adding(esd.StateGroups, id, machine), null, id);
    }

    /// <summary>A copy of a machine with a new id: a sub-machine's copy is the next sub-machine, an entry machine's the next entry id</summary>
    public static EsdEditPlan DuplicateMachine(ESD esd, long mid)
    {
        if (!esd.StateGroups.TryGetValue(mid, out var machine))
            return EsdEditPlan.Refuse("ESD_Edit_No_Machine");

        long id;
        if (EsdLayout.IsSubMachine(mid))
        {
            if (NextSubMachineId(esd) is not long sub)
                return EsdEditPlan.Refuse("ESD_Edit_Machine_Range_Full");
            id = sub;
        }
        else
            id = NextEntryMachineId(esd);

        return new EsdEditPlan(EsdKeyedOp<Dictionary<long, ESD.State>>.Adding(esd.StateGroups, id, CloneMachine(machine)), null, id);
    }

    /// <summary>Deletes a machine no call in the file starts (other than its own calls to itself)</summary>
    public static EsdEditPlan DeleteMachine(ESD esd, long mid)
    {
        if (!esd.StateGroups.ContainsKey(mid))
            return EsdEditPlan.Refuse("ESD_Edit_No_Machine");
        if (esd.StateGroups.Count < 2)
            return EsdEditPlan.Refuse("ESD_Edit_Delete_Last_Machine_Refused");
        if (EsdLayout.Callers(esd, mid).Any(c => c.Machine != mid))
            return EsdEditPlan.Refuse("ESD_Edit_Delete_Called_Machine_Refused");

        return new EsdEditPlan(EsdKeyedOp<Dictionary<long, ESD.State>>.Removing(esd.StateGroups, mid), null);
    }

    /// <summary>A deep copy of a machine; conditions shared between its states stay shared in the copy</summary>
    public static Dictionary<long, ESD.State> CloneMachine(Dictionary<long, ESD.State> machine)
    {
        var map = new Dictionary<ESD.Condition, ESD.Condition>(ReferenceEqualityComparer.Instance);

        ESD.Condition Clone(ESD.Condition c)
        {
            if (map.TryGetValue(c, out var done))
                return done;
            var copy = new ESD.Condition { TargetState = c.TargetState, Evaluator = EsdEdits.Copy(c.Evaluator) };
            map[c] = copy;
            copy.PassCommands.AddRange(c.PassCommands.Select(EsdEdits.CloneCommand));
            foreach (var s in c.Subconditions)
                copy.Subconditions.Add(Clone(s));
            return copy;
        }

        var result = new Dictionary<long, ESD.State>();
        foreach (var (id, s) in machine)
        {
            var copy = new ESD.State();
            copy.EntryCommands.AddRange(s.EntryCommands.Select(EsdEdits.CloneCommand));
            copy.WhileCommands.AddRange(s.WhileCommands.Select(EsdEdits.CloneCommand));
            copy.ExitCommands.AddRange(s.ExitCommands.Select(EsdEdits.CloneCommand));
            copy.Conditions.AddRange(s.Conditions.Select(Clone));
            result[id] = copy;
        }
        return result;
    }
}
