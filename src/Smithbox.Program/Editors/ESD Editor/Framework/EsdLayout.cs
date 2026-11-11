using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>What an ESD scripts: picks the catalogue kind and groups the file list</summary>
public enum EsdScriptKind
{
    Talk,
    Chr,
    Menu,
    Event,
    AI,
    Other,
}

public enum EsdRowKind
{
    /// A state's hat block
    State,
    /// "on entry" / "while" / "on exit" label above a state's command list
    Section,
    /// A command call
    Command,
    /// A transition: if (evaluator) → target, with pass commands and subconditions inside
    Condition,
    /// The foot of a condition that holds pass commands or subconditions
    ConditionEnd,
}

/// <summary>Which list of a state or condition a command row is in</summary>
public enum EsdCommandList
{
    Entry,
    While,
    Exit,
    Pass,
}

/// <summary>One row of the state view</summary>
public class EsdRow
{
    public EsdRowKind Kind;
    public int Depth;

    public long StateId;
    public ESD.State State;

    public ESD.Condition Condition;
    /// For a condition row: its index in its parent's list; for a command row: its index in its list
    public int Index;

    public ESD.CommandCall Command;
    public EsdCommandList List;

    /// How many places in the machine list this condition (it can be shared between states and parents)
    public int SharedCount;

    /// For a condition row: the list holding it (its state's conditions, or its parent condition's subconditions)
    public List<ESD.Condition> ParentList;

    /// For a condition row: the row of its foot (-1 when it has none)
    public int EndRow = -1;
}

/// <summary>What a command is, by bank and id (banks 5/6/7 are the language's own, not the game's)</summary>
public enum EsdCommandKind
{
    /// Bank 1: a game command
    Game,
    /// Bank 6: run another machine of the same file, by its id
    CallMachine,
    /// Bank 7, id -1: end this machine, returning the arg
    Return,
    /// Anything else (bank 5, other bank 7 ids)
    Other,
}

/// <summary>
/// Builds the state view's rows for a machine, and the facts the windows show about machines. Pure: compiled into
/// Claude/Tools/EsdSurvey too.
/// </summary>
public static class EsdLayout
{
    /// The ids of sub-machines (ESDLang's x0, x1...) count down from 0x7FFFFFFF
    public const long SubMachineTop = 0x7FFFFFFF;
    public const long SubMachineRange = 0x10000;

    public static bool IsSubMachine(long id) => id <= SubMachineTop && id > SubMachineTop - SubMachineRange;

    /// "x12" for a sub-machine, else the id
    public static string MachineLabel(long id) => IsSubMachine(id) ? $"x{SubMachineTop - id}" : id.ToString();

    public static EsdCommandKind KindOf(ESD.CommandCall c) => c.CommandBank switch
    {
        1 => EsdCommandKind.Game,
        6 => EsdCommandKind.CallMachine,
        7 when c.CommandID == -1 => EsdCommandKind.Return,
        _ => EsdCommandKind.Other
    };

    /// <summary>States in id order, each with its sections, commands and conditions (nested), as rows</summary>
    public static List<EsdRow> Build(Dictionary<long, ESD.State> machine)
    {
        var rows = new List<EsdRow>();
        if (machine == null)
            return rows;

        var refs = ReferenceCounts(machine);

        foreach (var (id, state) in machine.OrderBy(s => s.Key))
        {
            rows.Add(new EsdRow { Kind = EsdRowKind.State, Depth = 0, StateId = id, State = state });

            void Commands(List<ESD.CommandCall> list, EsdCommandList which)
            {
                if (list.Count == 0)
                    return;

                rows.Add(new EsdRow { Kind = EsdRowKind.Section, Depth = 1, StateId = id, State = state, List = which });
                for (int i = 0; i < list.Count; i++)
                    rows.Add(new EsdRow { Kind = EsdRowKind.Command, Depth = 1, StateId = id, State = state, Command = list[i], List = which, Index = i });
            }

            Commands(state.EntryCommands, EsdCommandList.Entry);
            Commands(state.WhileCommands, EsdCommandList.While);
            Commands(state.ExitCommands, EsdCommandList.Exit);

            for (int i = 0; i < state.Conditions.Count; i++)
                Condition(rows, refs, id, state, state.Conditions, i, 1, new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance));
        }

        return rows;
    }

    private static void Condition(List<EsdRow> rows, Dictionary<ESD.Condition, int> refs, long id, ESD.State state, List<ESD.Condition> parent, int index, int depth, HashSet<ESD.Condition> path)
    {
        var c = parent[index];
        var row = new EsdRow
        {
            Kind = EsdRowKind.Condition,
            Depth = depth,
            StateId = id,
            State = state,
            Condition = c,
            Index = index,
            ParentList = parent,
            SharedCount = refs.TryGetValue(c, out var n) ? n : 1,
        };
        rows.Add(row);

        // A condition can't hold itself in vanilla data, but an edited file might: don't loop
        if (!path.Add(c))
            return;

        for (int i = 0; i < c.PassCommands.Count; i++)
            rows.Add(new EsdRow { Kind = EsdRowKind.Command, Depth = depth + 1, StateId = id, State = state, Condition = c, Command = c.PassCommands[i], List = EsdCommandList.Pass, Index = i });

        for (int i = 0; i < c.Subconditions.Count; i++)
            Condition(rows, refs, id, state, c.Subconditions, i, depth + 1, path);

        path.Remove(c);

        if (c.PassCommands.Count > 0 || c.Subconditions.Count > 0)
        {
            row.EndRow = rows.Count;
            rows.Add(new EsdRow { Kind = EsdRowKind.ConditionEnd, Depth = depth, StateId = id, State = state, Condition = c });
        }
    }

    /// <summary>How many lists of the machine (states' conditions, conditions' subconditions) hold each condition</summary>
    public static Dictionary<ESD.Condition, int> ReferenceCounts(Dictionary<long, ESD.State> machine)
    {
        var counts = new Dictionary<ESD.Condition, int>(ReferenceEqualityComparer.Instance);
        var expanded = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);

        void Visit(ESD.Condition c)
        {
            counts[c] = counts.TryGetValue(c, out var n) ? n + 1 : 1;
            if (!expanded.Add(c))
                return;
            foreach (var s in c.Subconditions)
                Visit(s);
        }

        foreach (var state in machine.Values)
            foreach (var c in state.Conditions)
                Visit(c);

        return counts;
    }

    /// <summary>Every command of a machine: states' entry/while/exit lists and every condition's pass commands</summary>
    public static IEnumerable<ESD.CommandCall> AllCommands(Dictionary<long, ESD.State> machine)
    {
        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<ESD.Condition>();

        foreach (var state in machine.Values)
        {
            foreach (var c in state.EntryCommands) yield return c;
            foreach (var c in state.WhileCommands) yield return c;
            foreach (var c in state.ExitCommands) yield return c;
            foreach (var c in state.Conditions) stack.Push(c);
        }

        while (stack.Count > 0)
        {
            var c = stack.Pop();
            if (!seen.Add(c))
                continue;
            foreach (var p in c.PassCommands) yield return p;
            foreach (var s in c.Subconditions) stack.Push(s);
        }
    }

    /// <summary>For each machine of a file, how many bank-6 calls start it from the file's machines</summary>
    public static Dictionary<long, int> CallCounts(ESD esd)
    {
        var counts = new Dictionary<long, int>();
        foreach (var machine in esd.StateGroups.Values)
            foreach (var c in AllCommands(machine))
                if (KindOf(c) == EsdCommandKind.CallMachine)
                    counts[c.CommandID] = counts.TryGetValue(c.CommandID, out var n) ? n + 1 : 1;
        return counts;
    }

    /// <summary>The machines calling a machine, with how many calls each</summary>
    public static List<(long Machine, int Calls)> Callers(ESD esd, long target)
    {
        var result = new List<(long, int)>();
        foreach (var (id, machine) in esd.StateGroups)
        {
            var n = AllCommands(machine).Count(c => KindOf(c) == EsdCommandKind.CallMachine && c.CommandID == target);
            if (n > 0)
                result.Add((id, n));
        }
        return result;
    }

    /// <summary>States no transition of the machine leads to (other than the lowest-id state, where it starts)</summary>
    public static HashSet<long> Unreached(Dictionary<long, ESD.State> machine)
    {
        var targets = new HashSet<long>();
        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<ESD.Condition>();

        foreach (var state in machine.Values)
            foreach (var c in state.Conditions)
                stack.Push(c);

        while (stack.Count > 0)
        {
            var c = stack.Pop();
            if (!seen.Add(c))
                continue;
            if (c.TargetState is long t)
                targets.Add(t);
            foreach (var s in c.Subconditions)
                stack.Push(s);
        }

        var start = machine.Count > 0 ? machine.Keys.Min() : 0;
        return machine.Keys.Where(k => k != start && !targets.Contains(k)).ToHashSet();
    }
}
