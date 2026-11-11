using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

public enum EsdProblemKind
{
    /// A transition goes to a state the machine doesn't have
    MissingTarget,
    /// A call names a machine the file doesn't have
    MissingMachine,
    /// A command or function called with an arg count outside the catalogue's
    ArgCount,
    /// Bytes that don't decode as one expression
    Undecoded,
}

public class EsdProblem
{
    public EsdProblemKind Kind;
    public long Machine;
    public long State;
    /// The condition or command the problem is on
    public object Item;
    /// The missing state / machine id, or the arg count used
    public long Value;
    /// For ArgCount: the call's name and the documented range
    public string Name;
    public int Min;
    public int Max;
}

/// <summary>
/// The problems the state view and the edit guard look for. Pure: compiled into Claude/Tools/EsdSurvey, which runs them
/// over every vanilla file for the baseline (what vanilla really has).
/// </summary>
public static class EsdChecks
{
    public static List<EsdProblem> Run(ESD esd, EsdKindCatalog cat)
    {
        var list = new List<EsdProblem>();
        foreach (var (mid, machine) in esd.StateGroups)
            Machine(list, esd, mid, machine, cat);
        return list;
    }

    public static List<EsdProblem> Machine(ESD esd, long mid, EsdKindCatalog cat)
    {
        var list = new List<EsdProblem>();
        if (esd.StateGroups.TryGetValue(mid, out var machine))
            Machine(list, esd, mid, machine, cat);
        return list;
    }

    private static void Machine(List<EsdProblem> list, ESD esd, long mid, Dictionary<long, ESD.State> machine, EsdKindCatalog cat)
    {
        foreach (var (sid, state) in machine)
        {
            foreach (var c in state.EntryCommands.Concat(state.WhileCommands).Concat(state.ExitCommands))
                Command(list, esd, mid, sid, c, cat);

            var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
            var stack = new Stack<ESD.Condition>(state.Conditions);
            while (stack.Count > 0)
            {
                var c = stack.Pop();
                if (!seen.Add(c))
                    continue;

                if (c.TargetState is long t && !machine.ContainsKey(t))
                    list.Add(new EsdProblem { Kind = EsdProblemKind.MissingTarget, Machine = mid, State = sid, Item = c, Value = t });

                Expression(list, mid, sid, c, c.Evaluator, cat);

                foreach (var p in c.PassCommands)
                    Command(list, esd, mid, sid, p, cat);
                foreach (var s in c.Subconditions)
                    stack.Push(s);
            }
        }
    }

    private static void Command(List<EsdProblem> list, ESD esd, long mid, long sid, ESD.CommandCall c, EsdKindCatalog cat)
    {
        var kind = EsdLayout.KindOf(c);

        if (kind == EsdCommandKind.CallMachine && !esd.StateGroups.ContainsKey(c.CommandID))
            list.Add(new EsdProblem { Kind = EsdProblemKind.MissingMachine, Machine = mid, State = sid, Item = c, Value = c.CommandID });

        if (kind == EsdCommandKind.Game && cat?.Command(c.CommandBank, c.CommandID) is { } def && def.Args.Count > 0
            && (c.Arguments.Count < def.RequiredArgs || c.Arguments.Count > def.Args.Count))
            list.Add(new EsdProblem { Kind = EsdProblemKind.ArgCount, Machine = mid, State = sid, Item = c, Value = c.Arguments.Count, Name = def.Name, Min = def.RequiredArgs, Max = def.Args.Count });

        foreach (var a in c.Arguments)
            Expression(list, mid, sid, c, a, cat);
    }

    private static void Expression(List<EsdProblem> list, long mid, long sid, object item, byte[] code, EsdKindCatalog cat)
    {
        var node = EsdExpr.Decode(code);
        if (node == null)
        {
            list.Add(new EsdProblem { Kind = EsdProblemKind.Undecoded, Machine = mid, State = sid, Item = item });
            return;
        }

        foreach (var call in EsdExpr.Calls(node))
        {
            if (call.FunctionId is not long id || cat?.Function(id) is not { } def || def.Args.Count == 0)
                continue;
            var n = call.CallArgCount;
            if (n < def.RequiredArgs || n > def.Args.Count)
                list.Add(new EsdProblem { Kind = EsdProblemKind.ArgCount, Machine = mid, State = sid, Item = item, Value = n, Name = def.Name, Min = def.RequiredArgs, Max = def.Args.Count });
        }
    }

    /// <summary>The transitions of a machine that go to a state (for the delete guard)</summary>
    public static int TransitionsTo(Dictionary<long, ESD.State> machine, long target)
    {
        var count = 0;
        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<ESD.Condition>(machine.Values.SelectMany(s => s.Conditions));
        while (stack.Count > 0)
        {
            var c = stack.Pop();
            if (!seen.Add(c))
                continue;
            if (c.TargetState == target)
                count++;
            foreach (var s in c.Subconditions)
                stack.Push(s);
        }
        return count;
    }
}
