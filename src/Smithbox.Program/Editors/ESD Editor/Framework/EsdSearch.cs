using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>One place a value is used</summary>
public class EsdSearchHit
{
    public long Machine;
    public long State;

    /// The condition (a test literal) or command (an arg literal) holding it
    public ESD.Condition Condition;
    public ESD.CommandCall Command;

    /// The command arg it's in, or -1 for a test
    public int Arg = -1;

    /// The call it's an arg of: a command (CallBank/CallId) or a function (CallBank -1)
    public int CallBank;
    public long CallId;
    /// Its index among that call's args (-1: not directly an arg of a call)
    public int CallArg = -1;

    public EsdRoleKind Role;
}

/// <summary>
/// Value search inside an ESD file (P5): every literal equal to the value, as a command arg or a function arg in a
/// test or arg, with the role the arg has. Calls to a machine count as uses of the machine's ID. Each condition is
/// searched once even when shared. Pure: compiled into Claude/Tools/EsdSurvey (search gate).
/// </summary>
public static class EsdSearch
{
    public static List<EsdSearchHit> Find(ESD esd, long value, string game, EsdKindCatalog cat, bool flagsOnly)
    {
        var hits = new List<EsdSearchHit>();

        foreach (var (mid, machine) in esd.StateGroups)
        {
            foreach (var (sid, state) in machine)
            {
                foreach (var c in state.EntryCommands.Concat(state.WhileCommands).Concat(state.ExitCommands))
                    Command(c, mid, sid);
            }

            // Conditions (and their pass commands) once, with the first state that holds them
            var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
            foreach (var (sid, state) in machine)
            {
                var stack = new Stack<ESD.Condition>(state.Conditions);
                while (stack.Count > 0)
                {
                    var c = stack.Pop();
                    if (!seen.Add(c))
                        continue;
                    Expression(EsdExpr.Decode(c.Evaluator), mid, sid, c, null, -1);
                    foreach (var p in c.PassCommands)
                        Command(p, mid, sid);
                    foreach (var s in c.Subconditions)
                        stack.Push(s);
                }
            }
        }

        return hits;

        void Command(ESD.CommandCall c, long mid, long sid)
        {
            var kind = EsdLayout.KindOf(c);

            if (kind == EsdCommandKind.CallMachine && c.CommandID == value && !flagsOnly)
                hits.Add(new EsdSearchHit { Machine = mid, State = sid, Command = c, CallBank = c.CommandBank, CallId = c.CommandID, Role = EsdRoleKind.Machine });

            var def = kind == EsdCommandKind.Game ? cat?.Command(c.CommandBank, c.CommandID) : null;
            for (int i = 0; i < c.Arguments.Count; i++)
            {
                var node = EsdExpr.Decode(c.Arguments[i]);
                if (EsdExpr.Constant(node) == value)
                    Add(mid, sid, null, c, i, c.CommandBank, c.CommandID, i, EsdRoles.Of(game, def, i).Kind);
                Expression(node, mid, sid, null, c, i);
            }
        }

        void Expression(EsdExprNode root, long mid, long sid, ESD.Condition cond, ESD.CommandCall cmd, int arg)
        {
            if (root == null)
                return;

            // Literals that are args of a function call, with that arg's role
            foreach (var call in EsdExpr.Calls(root))
            {
                if (call.FunctionId is not long fid)
                    continue;
                var def = cat?.Function(fid);
                for (int k = 0; k < call.CallArgCount; k++)
                    if (EsdExpr.Constant(call.Children[k + 1]) == value)
                        Add(mid, sid, cond, cmd, arg, -1, fid, k, EsdRoles.Of(game, def, k).Kind);
            }

            // A test literal outside a call (e.g. "== 1234"), for value searches
            if (!flagsOnly && cond != null)
                foreach (var (lit, _) in EsdEdits.Literals(root))
                    if (lit.Kind == EsdExprKind.Int && lit.IntValue == value && !InCallArg(root, lit))
                        hits.Add(new EsdSearchHit { Machine = mid, State = sid, Condition = cond, Arg = -1, Role = EsdRoleKind.None });
        }

        void Add(long mid, long sid, ESD.Condition cond, ESD.CommandCall cmd, int arg, int bank, long id, int callArg, EsdRoleKind role)
        {
            if (flagsOnly && role != EsdRoleKind.EventFlag)
                return;
            hits.Add(new EsdSearchHit { Machine = mid, State = sid, Condition = cond, Command = cmd, Arg = arg, CallBank = bank, CallId = id, CallArg = callArg, Role = role });
        }
    }

    /// <summary>Whether a literal is directly an arg of a call in the tree (already reported with its role)</summary>
    private static bool InCallArg(EsdExprNode root, EsdExprNode lit)
    {
        foreach (var call in EsdExpr.Calls(root))
            for (int k = 1; k < call.Children.Count; k++)
                if (ReferenceEquals(EsdExpr.Strip(call.Children[k]), lit))
                    return true;
        return false;
    }

    /// <summary>The call sites of a machine in its file: each machine and state with a bank 6 call to it</summary>
    public static List<EsdSearchHit> CallsTo(ESD esd, long target)
    {
        var hits = new List<EsdSearchHit>();
        foreach (var (mid, machine) in esd.StateGroups)
        {
            foreach (var (sid, state) in machine)
                foreach (var c in state.EntryCommands.Concat(state.WhileCommands).Concat(state.ExitCommands))
                    if (EsdLayout.KindOf(c) == EsdCommandKind.CallMachine && c.CommandID == target)
                        hits.Add(new EsdSearchHit { Machine = mid, State = sid, Command = c, CallBank = 6, CallId = target, Role = EsdRoleKind.Machine });

            var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
            foreach (var (sid, state) in machine)
            {
                var stack = new Stack<ESD.Condition>(state.Conditions);
                while (stack.Count > 0)
                {
                    var c = stack.Pop();
                    if (!seen.Add(c))
                        continue;
                    foreach (var p in c.PassCommands)
                        if (EsdLayout.KindOf(p) == EsdCommandKind.CallMachine && p.CommandID == target)
                            hits.Add(new EsdSearchHit { Machine = mid, State = sid, Command = p, CallBank = 6, CallId = target, Role = EsdRoleKind.Machine });
                    foreach (var s in c.Subconditions)
                        stack.Push(s);
                }
            }
        }
        return hits;
    }
}
