using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>How a transition's target is shown in the code view</summary>
public enum EsdFlow
{
    /// No target (a condition that only holds subconditions)
    None,
    /// The target state is drawn inside the transition: it's the only way to reach it
    Inline,
    /// The target is the state drawn right after the block the transition is in ("continue below")
    Next,
    /// The target is drawn later in an enclosing block ("go to", forward)
    Forward,
    /// The target is a loop's head around the transition ("repeat")
    Repeat,
    /// Any other jump: back to a state that isn't a loop head around it, or out of an unreached part
    Goto,
    /// The machine has no such state
    Missing,
}

/// <summary>The word a transition's block starts with in the code view</summary>
public enum EsdBranchLabel
{
    If,
    ElseIf,
    Else,
    /// A state that waits: none of its transitions is "always", so it checks them every frame until one passes
    When,
    /// The first transition is "always" and holds commands or subconditions
    Then,
    /// The first transition is "always" with nothing inside: a plain jump
    GoTo,
    /// The B9 BA 96 test of a state that called a machine: ESDLang's call.Done()
    CallDone,
}

/// <summary>A state placed in the code view</summary>
public class EsdCodeStep
{
    public long StateId;
    public ESD.State State;

    /// Drawn inside the one transition that reaches it
    public bool Inline;
    /// A transition inside the state's own block comes back to it
    public bool LoopHead;
    /// Not reachable from the machine's start
    public bool Unreached;
    /// None of its own transitions is "always": it waits until one passes
    public bool Waits;

    /// No transitions: the machine stays here
    public bool Stops => State.Conditions.Count == 0;

    /// Reverse postorder index (0 = the start)
    public int Order;

    /// Its transitions, in the state's order
    public List<EsdCodeBranch> Branches = new();

    /// States drawn after this one at the same depth: those it dominates that more than one transition reaches
    public List<EsdCodeStep> After = new();
}

/// <summary>A transition (condition) placed in the code view</summary>
public class EsdCodeBranch
{
    public ESD.Condition Condition;
    public List<ESD.Condition> ParentList;
    public int Index;

    public EsdFlow Flow;
    public EsdBranchLabel Label;

    /// The jump's target (the condition's target state), when it has one
    public long? Target;

    /// For an Inline flow: the target, drawn inside this transition
    public EsdCodeStep Inlined;

    /// A state's only transition, "always", with nothing inside, going to the next thing drawn: it has no row
    public bool Fallthrough;

    public List<EsdCodeBranch> Sub = new();
}

public class EsdCodeStats
{
    public int States, Inlined, Placed, LoopHeads, Unreached, Fallthroughs;
    public int Edges, InlineEdges, NextEdges, ForwardEdges, RepeatEdges, GotoEdges, MissingEdges;
}

/// <summary>A machine decompiled for the code view</summary>
public class EsdCode
{
    /// The start state, then the states placed at the top (unreached parts, and states only the top dominates)
    public List<EsdCodeStep> Top = new();

    /// How many args the machine reads (one past the highest arg[n])
    public int Args;

    public EsdCodeStats Stats = new();

    public Dictionary<long, EsdCodeStep> Steps = new();
}

/// <summary>
/// The code view's structure (Claude/Documentation/ESD Block Editor Analysis.md §4-5, P3): a machine's states placed
/// by the dominator tree of its transition graph.
///
/// - A state reached by exactly one transition (and not a loop head) is drawn inside that transition (Inline), so
///   chains read as sequences and branches as if / else if / else.
/// - Every other state is drawn after the state that dominates it, at that state's depth, in reverse postorder.
///   Transitions to it are "continue below" (Next) when it's the next thing drawn, else "go to" (Forward).
/// - A transition back to a state that dominates it is "repeat" (a loop).
/// - States the start can't reach are drawn at the top after the start's part.
///
/// Every state, condition and command keeps its place in the model: compiling only reads targets back from the tree.
/// The gate (EsdSurvey structure) checks Compile(Decompile(m)) equals m, and Validate, on every vanilla machine.
/// Pure: compiled into Claude/Tools/EsdSurvey.
/// </summary>
public static class EsdStructure
{
    private static readonly byte[] CallDoneCode = { 0xB9, 0xBA, 0x96, EsdExpr.End };

    public static bool IsCallDone(byte[] code) => code != null && code.AsSpan().SequenceEqual(CallDoneCode);

    private class Edge
    {
        public long From;
        public long To;
        public EsdCodeBranch Branch;
    }

    public static EsdCode Decompile(Dictionary<long, ESD.State> machine)
    {
        var code = new EsdCode();
        if (machine == null || machine.Count == 0)
            return code;

        var ids = machine.Keys.OrderBy(k => k).ToList();
        var start = ids[0];

        // Steps and branches, with the transitions between states
        var edges = new List<Edge>();
        foreach (var id in ids)
        {
            var step = new EsdCodeStep { StateId = id, State = machine[id] };
            code.Steps[id] = step;

            var path = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
            step.Branches = Branches(id, step.State.Conditions, path, edges);
            step.Waits = step.State.Conditions.Count > 0 && !step.State.Conditions.Any(c => EsdExpr.IsAlways(c.Evaluator));
        }

        var succ = ids.ToDictionary(i => i, _ => new List<long>());
        var preds = ids.ToDictionary(i => i, _ => new List<long>());
        var inCount = ids.ToDictionary(i => i, _ => 0);
        foreach (var e in edges)
        {
            if (!machine.ContainsKey(e.To))
                continue;
            succ[e.From].Add(e.To);
            preds[e.To].Add(e.From);
            inCount[e.To]++;
        }

        // Roots: the start, then parts it can't reach (states nothing reaches first, then the rest by id)
        var roots = new List<long> { start };
        var reached = Reach(start, succ);
        while (reached.Count < ids.Count)
        {
            var next = ids.FirstOrDefault(i => !reached.Contains(i) && preds[i].All(reached.Contains), long.MinValue);
            if (next == long.MinValue)
                next = ids.First(i => !reached.Contains(i));
            roots.Add(next);
            reached.UnionWith(Reach(next, succ));
        }
        var rootSet = roots.ToHashSet();
        var startReach = Reach(start, succ);

        // Reverse postorder from a virtual root (index 0) whose children are the roots
        var order = ReversePostorder(roots, succ);
        var rpo = new Dictionary<long, int>();
        for (int i = 0; i < order.Count; i++)
            rpo[order[i]] = i + 1;

        // Dominators (Cooper, Harvey & Kennedy), over node indexes; 0 is the virtual root
        int n = order.Count + 1;
        var idom = new int[n];
        Array.Fill(idom, -1);
        idom[0] = 0;
        var predIdx = new List<int>[n];
        predIdx[0] = new();
        for (int i = 0; i < order.Count; i++)
        {
            var id = order[i];
            predIdx[i + 1] = preds[id].Select(p => rpo[p]).ToList();
            if (rootSet.Contains(id))
                predIdx[i + 1].Add(0);
        }

        int Intersect(int a, int b)
        {
            while (a != b)
            {
                while (a > b) a = idom[a];
                while (b > a) b = idom[b];
            }
            return a;
        }

        for (var changed = true; changed;)
        {
            changed = false;
            for (int b = 1; b < n; b++)
            {
                int newIdom = -1;
                foreach (var p in predIdx[b])
                {
                    if (idom[p] < 0)
                        continue;
                    newIdom = newIdom < 0 ? p : Intersect(p, newIdom);
                }
                if (newIdom >= 0 && idom[b] != newIdom)
                {
                    idom[b] = newIdom;
                    changed = true;
                }
            }
        }

        bool Dominates(long a, long b)
        {
            int x = rpo[a], y = rpo[b];
            while (y > x) y = idom[y];
            return y == x;
        }

        foreach (var id in ids)
        {
            var s = code.Steps[id];
            s.Order = rpo[id] - 1;
            s.Unreached = !startReach.Contains(id);
            s.LoopHead = preds[id].Any(p => Dominates(id, p));
        }

        // Inline: the one transition reaching a state that isn't a root or a loop head
        foreach (var e in edges)
        {
            e.Branch.Target = e.To;
            if (!machine.ContainsKey(e.To))
            {
                e.Branch.Flow = EsdFlow.Missing;
                continue;
            }

            var to = code.Steps[e.To];
            if (!rootSet.Contains(e.To) && inCount[e.To] == 1 && !to.LoopHead)
            {
                e.Branch.Flow = EsdFlow.Inline;
                e.Branch.Inlined = to;
                to.Inline = true;
            }
            else if (Dominates(e.To, e.From))
                e.Branch.Flow = EsdFlow.Repeat;
            else
                e.Branch.Flow = rpo[e.To] > rpo[e.From] ? EsdFlow.Forward : EsdFlow.Goto;
        }

        // Placement: every other state after the state dominating it (the top for the virtual root)
        foreach (var id in order)
        {
            var s = code.Steps[id];
            if (s.Inline)
                continue;

            var d = idom[rpo[id]];
            if (d == 0)
                code.Top.Add(s);
            else
                code.Steps[order[d - 1]].After.Add(s);
        }

        // order is reverse postorder already, so Top and After lists are in it too; the start is first

        // Next: a forward jump to what's drawn right after the block it's in
        for (int i = 0; i < code.Top.Count; i++)
            Follow(code.Top[i], i + 1 < code.Top.Count ? code.Top[i + 1].StateId : null);

        // Labels, fallthroughs
        foreach (var s in code.Steps.Values)
        {
            Label(s.Branches, s.Waits);
            if (s.Branches.Count == 1 && s.Branches[0] is var b && EsdExpr.IsAlways(b.Condition.Evaluator) &&
                b.Condition.PassCommands.Count == 0 && b.Sub.Count == 0 && b.Flow is EsdFlow.Inline or EsdFlow.Next)
                b.Fallthrough = true;
        }

        code.Args = ArgCount(machine);
        code.Stats = Stats(code, edges);
        return code;
    }

    private static List<EsdCodeBranch> Branches(long state, List<ESD.Condition> list, HashSet<ESD.Condition> path, List<Edge> edges)
    {
        var result = new List<EsdCodeBranch>();
        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            var b = new EsdCodeBranch { Condition = c, ParentList = list, Index = i, Flow = EsdFlow.None };
            result.Add(b);

            if (c.TargetState is long t)
                edges.Add(new Edge { From = state, To = t, Branch = b });

            // A condition can't hold itself in vanilla data, but an edited file might: don't loop
            if (path.Add(c))
            {
                b.Sub = Branches(state, c.Subconditions, path, edges);
                path.Remove(c);
            }
        }
        return result;
    }

    private static HashSet<long> Reach(long from, Dictionary<long, List<long>> succ)
    {
        var seen = new HashSet<long> { from };
        var stack = new Stack<long>();
        stack.Push(from);
        while (stack.Count > 0)
            foreach (var t in succ[stack.Pop()])
                if (seen.Add(t))
                    stack.Push(t);
        return seen;
    }

    /// <summary>
    /// Each root's part in reverse postorder (a depth-first walk taking successors in order, skipping what an earlier
    /// part holds), parts in root order so the start comes first. Every state's dominators still come before it: they
    /// lie on its walk path, inside its own part.
    /// </summary>
    private static List<long> ReversePostorder(List<long> roots, Dictionary<long, List<long>> succ)
    {
        var result = new List<long>();
        var seen = new HashSet<long>();
        var stack = new Stack<(long Id, int Next)>();

        foreach (var root in roots)
        {
            if (!seen.Add(root))
                continue;

            var post = new List<long>();
            stack.Push((root, 0));
            while (stack.Count > 0)
            {
                var (id, next) = stack.Pop();
                var list = succ[id];
                if (next < list.Count)
                {
                    stack.Push((id, next + 1));
                    if (seen.Add(list[next]))
                        stack.Push((list[next], 0));
                }
                else
                    post.Add(id);
            }

            post.Reverse();
            result.AddRange(post);
        }
        return result;
    }

    private static void Follow(EsdCodeStep step, long? follow)
    {
        var inner = step.After.Count > 0 ? step.After[0].StateId : follow;
        FollowBranches(step.Branches, inner);
        for (int i = 0; i < step.After.Count; i++)
            Follow(step.After[i], i + 1 < step.After.Count ? step.After[i + 1].StateId : follow);
    }

    private static void FollowBranches(List<EsdCodeBranch> branches, long? follow)
    {
        foreach (var b in branches)
        {
            if (b.Flow is EsdFlow.Forward or EsdFlow.Goto && b.Target == follow)
                b.Flow = EsdFlow.Next;
            if (b.Inlined != null)
                Follow(b.Inlined, follow);
            FollowBranches(b.Sub, follow);
        }
    }

    private static void Label(List<EsdCodeBranch> list, bool waits)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var b = list[i];
            var c = b.Condition;
            var always = EsdExpr.IsAlways(c.Evaluator);

            if (IsCallDone(c.Evaluator))
                b.Label = EsdBranchLabel.CallDone;
            else if (waits)
                b.Label = EsdBranchLabel.When;
            else if (i == 0)
                b.Label = !always ? EsdBranchLabel.If
                    : c.PassCommands.Count == 0 && c.Subconditions.Count == 0 && b.Flow != EsdFlow.Inline ? EsdBranchLabel.GoTo
                    : EsdBranchLabel.Then;
            else
                b.Label = always ? EsdBranchLabel.Else : EsdBranchLabel.ElseIf;

            Label(b.Sub, false);
        }
    }

    /// <summary>One past the highest literal arg[n] the machine reads</summary>
    public static int ArgCount(Dictionary<long, ESD.State> machine)
    {
        int max = -1;

        void Scan(byte[] bytes)
        {
            var node = EsdExpr.Decode(bytes);
            if (node == null)
                return;
            var stack = new Stack<EsdExprNode>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var x = stack.Pop();
                if (x.Kind == EsdExprKind.Unary && x.Op == 0xB8 && EsdExpr.Constant(x.Children[0]) is long a && a >= 0 && a < 64)
                    max = Math.Max(max, (int)a);
                foreach (var c in x.Children)
                    stack.Push(c);
            }
        }

        foreach (var cmd in EsdLayout.AllCommands(machine))
            foreach (var arg in cmd.Arguments)
                Scan(arg);

        var seen = new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance);
        var conds = new Stack<ESD.Condition>();
        foreach (var s in machine.Values)
            foreach (var c in s.Conditions)
                conds.Push(c);
        while (conds.Count > 0)
        {
            var c = conds.Pop();
            if (!seen.Add(c))
                continue;
            Scan(c.Evaluator);
            foreach (var sc in c.Subconditions)
                conds.Push(sc);
        }

        return max + 1;
    }

    private static EsdCodeStats Stats(EsdCode code, List<Edge> edges)
    {
        var st = new EsdCodeStats { States = code.Steps.Count, Edges = edges.Count };
        foreach (var s in code.Steps.Values)
        {
            if (s.Inline) st.Inlined++; else st.Placed++;
            if (s.LoopHead) st.LoopHeads++;
            if (s.Unreached) st.Unreached++;
            if (s.Branches.Any(b => b.Fallthrough)) st.Fallthroughs++;
        }
        foreach (var e in edges)
        {
            switch (e.Branch.Flow)
            {
                case EsdFlow.Inline: st.InlineEdges++; break;
                case EsdFlow.Next: st.NextEdges++; break;
                case EsdFlow.Forward: st.ForwardEdges++; break;
                case EsdFlow.Repeat: st.RepeatEdges++; break;
                case EsdFlow.Goto: st.GotoEdges++; break;
                case EsdFlow.Missing: st.MissingEdges++; break;
            }
        }
        return st;
    }

    /// <summary>
    /// Builds the machine back from the tree: each placed state's commands and conditions, with every target read from
    /// how the transition is drawn (its inlined state, or its jump). New State and Condition objects; commands are
    /// the same objects. States keep the original machine's order.
    /// </summary>
    public static Dictionary<long, ESD.State> Compile(EsdCode code, IEnumerable<long> keyOrder)
    {
        var built = new Dictionary<long, ESD.State>();

        void Place(EsdCodeStep s)
        {
            if (built.ContainsKey(s.StateId))
                throw new InvalidOperationException($"state {s.StateId} placed twice");

            var state = new ESD.State();
            state.EntryCommands.AddRange(s.State.EntryCommands);
            state.WhileCommands.AddRange(s.State.WhileCommands);
            state.ExitCommands.AddRange(s.State.ExitCommands);
            built[s.StateId] = state;
            state.Conditions.AddRange(s.Branches.Select(CompileBranch));

            foreach (var a in s.After)
                Place(a);
        }

        ESD.Condition CompileBranch(EsdCodeBranch b)
        {
            var c = new ESD.Condition
            {
                Evaluator = b.Condition.Evaluator,
                TargetState = b.Flow switch
                {
                    EsdFlow.None => null,
                    EsdFlow.Inline => b.Inlined.StateId,
                    _ => b.Target
                }
            };
            c.PassCommands.AddRange(b.Condition.PassCommands);
            c.Subconditions.AddRange(b.Sub.Select(CompileBranch));

            if (b.Inlined != null)
                Place(b.Inlined);
            return c;
        }

        foreach (var s in code.Top)
            Place(s);

        var result = new Dictionary<long, ESD.State>();
        foreach (var id in keyOrder)
            if (built.TryGetValue(id, out var s))
                result[id] = s;
        foreach (var (id, s) in built)
            result.TryAdd(id, s);
        return result;
    }

    /// <summary>
    /// Checks the tree is a proper structure: every state placed once, inlined states reached only through their
    /// transition, "continue below" / "go to" targets drawn later in an enclosing block, "repeat" targets around the
    /// transition. Returns the problems found (none for a good tree).
    /// </summary>
    public static List<string> Validate(EsdCode code)
    {
        var errors = new List<string>();
        var placed = new HashSet<long>();
        var open = new List<EsdCodeStep>();
        // States drawn later in a block that's open (each open sequence's remaining items)
        var pending = new List<HashSet<long>>();

        bool IsPending(long id) => pending.Any(p => p.Contains(id));

        void Step(EsdCodeStep s, long? follow)
        {
            if (!placed.Add(s.StateId))
                errors.Add($"state {s.StateId} placed twice");

            open.Add(s);
            var later = s.After.Select(a => a.StateId).ToHashSet();
            pending.Add(later);

            var inner = s.After.Count > 0 ? s.After[0].StateId : follow;
            BranchList(s.Branches, s.StateId, inner);

            for (int i = 0; i < s.After.Count; i++)
            {
                later.Remove(s.After[i].StateId);
                Step(s.After[i], i + 1 < s.After.Count ? s.After[i + 1].StateId : follow);
            }

            pending.RemoveAt(pending.Count - 1);
            open.RemoveAt(open.Count - 1);
        }

        void BranchList(List<EsdCodeBranch> list, long from, long? follow)
        {
            foreach (var b in list)
            {
                switch (b.Flow)
                {
                    case EsdFlow.Inline:
                        if (b.Inlined == null || !b.Inlined.Inline || b.Inlined.StateId != b.Target)
                            errors.Add($"state {from}: bad inline to {b.Target}");
                        else
                            Step(b.Inlined, follow);
                        break;
                    case EsdFlow.Next:
                        if (b.Target != follow || b.Target is not long nt || !IsPending(nt))
                            errors.Add($"state {from}: 'continue below' to {b.Target} isn't next");
                        break;
                    case EsdFlow.Forward:
                        if (b.Target is not long ft || !IsPending(ft))
                            errors.Add($"state {from}: 'go to' {b.Target} isn't drawn later in an open block");
                        break;
                    case EsdFlow.Repeat:
                        if (b.Target is not long rt || !open.Any(o => o.StateId == rt && o.LoopHead))
                            errors.Add($"state {from}: 'repeat' {b.Target} isn't a loop head around it");
                        break;
                    case EsdFlow.None:
                        if (b.Condition.TargetState != null)
                            errors.Add($"state {from}: target {b.Condition.TargetState} lost");
                        break;
                }
                BranchList(b.Sub, from, follow);
            }
        }

        var top = new HashSet<long>(code.Top.Select(t => t.StateId));
        pending.Add(top);
        for (int i = 0; i < code.Top.Count; i++)
        {
            top.Remove(code.Top[i].StateId);
            Step(code.Top[i], i + 1 < code.Top.Count ? code.Top[i + 1].StateId : null);
        }

        foreach (var id in code.Steps.Keys)
            if (!placed.Contains(id))
                errors.Add($"state {id} not placed");

        if (code.Top.Count > 0 && code.Top[0].StateId != code.Steps.Keys.Min())
            errors.Add("the start isn't first");

        return errors;
    }

    /// <summary>
    /// The code view's rows: a function hat, then the states as placed, each state's commands, then its transitions
    /// as C-blocks holding their pass commands, subconditions and inlined target. A fallthrough has no row: its target
    /// follows at the same depth. Rows carry the same lists and indexes as the state view's, so the same edits work.
    /// </summary>
    public static List<EsdRow> Rows(EsdCode code, Dictionary<long, ESD.State> machine)
    {
        var rows = new List<EsdRow>();
        if (machine == null || code.Top.Count == 0)
            return rows;

        var refs = EsdLayout.ReferenceCounts(machine);
        rows.Add(new EsdRow { Kind = EsdRowKind.Function, Depth = 0, StateId = -1, Args = code.Args });

        void Step(EsdCodeStep s, int depth)
        {
            var id = s.StateId;
            var state = s.State;
            rows.Add(new EsdRow { Kind = EsdRowKind.State, Depth = depth, StateId = id, State = state, Step = s });

            void Commands(List<ESD.CommandCall> list, EsdCommandList which, bool section)
            {
                if (list.Count == 0)
                    return;
                if (section)
                    rows.Add(new EsdRow { Kind = EsdRowKind.Section, Depth = depth, StateId = id, State = state, List = which });
                for (int i = 0; i < list.Count; i++)
                    rows.Add(new EsdRow { Kind = EsdRowKind.Command, Depth = depth, StateId = id, State = state, Command = list[i], List = which, Index = i });
            }

            // Entry commands are the state's code; while / exit keep their section label
            var sections = state.WhileCommands.Count > 0 || state.ExitCommands.Count > 0;
            Commands(state.EntryCommands, EsdCommandList.Entry, sections);
            Commands(state.WhileCommands, EsdCommandList.While, true);
            Commands(state.ExitCommands, EsdCommandList.Exit, true);

            foreach (var b in s.Branches)
                Branch(b, s, depth);

            foreach (var a in s.After)
                Step(a, depth);
        }

        void Branch(EsdCodeBranch b, EsdCodeStep s, int depth)
        {
            if (b.Fallthrough)
            {
                if (b.Inlined != null)
                    Step(b.Inlined, depth);
                return;
            }

            var c = b.Condition;
            var row = new EsdRow
            {
                Kind = EsdRowKind.Condition,
                Depth = depth,
                StateId = s.StateId,
                State = s.State,
                Condition = c,
                Index = b.Index,
                ParentList = b.ParentList,
                SharedCount = refs.TryGetValue(c, out var n) ? n : 1,
                Branch = b,
            };
            var at = rows.Count;
            rows.Add(row);

            for (int i = 0; i < c.PassCommands.Count; i++)
                rows.Add(new EsdRow { Kind = EsdRowKind.Command, Depth = depth + 1, StateId = s.StateId, State = s.State, Condition = c, Command = c.PassCommands[i], List = EsdCommandList.Pass, Index = i });

            foreach (var sub in b.Sub)
                Branch(sub, s, depth + 1);

            if (b.Inlined != null)
                Step(b.Inlined, depth + 1);

            if (rows.Count > at + 1)
            {
                row.EndRow = rows.Count;
                rows.Add(new EsdRow { Kind = EsdRowKind.ConditionEnd, Depth = depth, StateId = s.StateId, State = s.State, Condition = c, Branch = b });
            }
        }

        foreach (var t in code.Top)
            Step(t, 1);

        return rows;
    }

    /// <summary>A machine as text, for comparing two models (targets, evaluators, commands, in list order)</summary>
    public static string Canonical(Dictionary<long, ESD.State> machine)
    {
        var sb = new System.Text.StringBuilder();

        void Cmds(string tag, List<ESD.CommandCall> list, int depth)
        {
            foreach (var c in list)
            {
                sb.Append(' ', depth * 2).Append(tag).Append(' ').Append(c.CommandBank).Append(':').Append(c.CommandID);
                foreach (var a in c.Arguments)
                    sb.Append(' ').Append(Convert.ToHexString(a));
                sb.Append('\n');
            }
        }

        void Cond(ESD.Condition c, int depth, HashSet<ESD.Condition> path)
        {
            sb.Append(' ', depth * 2).Append("if ").Append(Convert.ToHexString(c.Evaluator)).Append(" -> ").Append(c.TargetState?.ToString() ?? "-").Append('\n');
            if (!path.Add(c))
                return;
            Cmds("pass", c.PassCommands, depth + 1);
            foreach (var s in c.Subconditions)
                Cond(s, depth + 1, path);
            path.Remove(c);
        }

        foreach (var (id, s) in machine)
        {
            sb.Append("state ").Append(id).Append('\n');
            Cmds("entry", s.EntryCommands, 1);
            Cmds("while", s.WhileCommands, 1);
            Cmds("exit", s.ExitCommands, 1);
            foreach (var c in s.Conditions)
                Cond(c, 1, new HashSet<ESD.Condition>(ReferenceEqualityComparer.Instance));
        }
        return sb.ToString();
    }
}
