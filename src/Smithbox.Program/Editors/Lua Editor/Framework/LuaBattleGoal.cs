using System.Globalization;

namespace StudioCore.Editors.LuaEditor;

/// <summary>A number literal in the source: its token range (a leading minus included) and value</summary>
public readonly record struct LuaLiteral(int First, int Last, double Value, int Line)
{
    public string Text => LuaBattleGoal.Format(Value);
}

/// <summary>One act: the function Common_Battle_Activate runs when the act is picked</summary>
public class LuaAct
{
    public int Id;
    public string Function;
    public int RegisterLine;

    /// <summary>The act function's definition line; 0 when it isn't defined in this script</summary>
    public int FunctionLine;

    /// <summary>The act function itself (null when it isn't defined in this script), for planning what the act does</summary>
    public LuaFunctionExpr FunctionNode;

    /// <summary>SetCoolTime(ai, goal, anim, seconds, weight, k) calls on this act's weight</summary>
    public List<(LuaLiteral? Anim, LuaLiteral? Seconds, int Line)> Cooldowns { get; } = new();

    public List<LuaSubGoal> SubGoals { get; } = new();

    /// <summary>
    /// The animations the act plays, in order: each sub-goal's animation argument (AddSubGoal(GOAL_…, life, anim, …):
    /// the literal after life, followed through a local), then the cooldown animations if no sub-goal gave one.
    /// Candidates only: the viewer keeps the ones the character's TAE has, as some sub-goals take other numbers there.
    /// </summary>
    public List<(long Id, string Source)> AnimationCandidates()
    {
        var list = new List<(long, string)>();
        foreach (var s in SubGoals)
        {
            if (s.Arguments.Count > 1 && s.Arguments[1].Literal is { } lit && IsAnimationId(lit.Value))
                list.Add(((long)lit.Value, s.Goal));
        }
        if (list.Count == 0)
        {
            foreach (var c in Cooldowns)
            {
                if (c.Anim is { } a && IsAnimationId(a.Value) && !list.Any(x => x.Item1 == (long)a.Value))
                    list.Add(((long)a.Value, "SetCoolTime"));
            }
        }
        return list;
    }

    private static bool IsAnimationId(double v) => v == Math.Floor(v) && v >= 1000 && v < 1_000_000;
}

/// <summary>goal:AddSubGoal(GOAL_..., life, args...) in an act function</summary>
public class LuaSubGoal
{
    public string Goal;
    public int Line;

    /// <summary>The arguments after the goal ID: source text, and the literal behind it (directly, or through a local set once to a number)</summary>
    public List<(string Text, LuaLiteral? Literal)> Arguments { get; } = new();
}

/// <summary>A run of weight assignments in one branch of Activate's conditions</summary>
public class LuaWeightRow
{
    /// <summary>The branch conditions leading here, outermost first ("if …", "elseif …", "else")</summary>
    public List<string> Path { get; } = new();
    public int Line;
    public SortedDictionary<int, LuaLiteral> Weights { get; } = new();
}

/// <summary>A special effect ID the script checks or observes (ai:HasSpecialEffectId(target, id), …)</summary>
public readonly record struct LuaSpEffectUse(int Id, string Method, int Line);

/// <summary>
/// The battle-goal template (395 of ER's 396 battle scripts): Goal.Activate fills a weight table and a function table
/// per act, then calls Common_Battle_Activate(ai, goal, weights, funcs, after, params), which picks an act by weight.
/// This reads the template from the syntax tree: the acts (REGIST_FUNC), the weights by condition branch, the
/// cooldowns (SetCoolTime), each act function's sub-goals, and the special effects used. Values are literals with
/// their token ranges, so the inspector can edit them in the text.
/// </summary>
public class LuaBattleGoal
{
    public string GoalName;

    /// <summary>The battle goal's Goal.Activate, and the names of its weight and function tables</summary>
    public LuaFunctionExpr Activate;
    public string WeightTable;
    public string FunctionTable;

    public int ActivateLine;
    public int InterruptLine;
    public string AfterFunction;
    public int AfterFunctionLine;

    /// <summary>The function Common_Battle_Activate runs after each act (…ActAfter_AdjustSpace); null if not in this script</summary>
    public LuaFunctionExpr AfterFunctionNode;

    /// <summary>The battle section's Goal.Interrupt (or Goal.Interupt), if it has one</summary>
    public LuaFunctionExpr Interrupt;

    /// <summary>The script's global functions by name, for calls the Interrupt makes (an act function, a helper)</summary>
    public Dictionary<string, LuaFunctionExpr> Functions { get; } = new();

    /// <summary>The SpEffects the script observes (AddObserveSpecialEffectAttribute): on whom, which, where</summary>
    public List<(string Target, int Id, int Line)> ObservedSpEffects { get; } = new();

    /// <summary>The interrupt types the Interrupt tests (IsInterupt(INTERUPT_x)), x without the prefix, first use first</summary>
    public List<(string Type, int Line)> InterruptTypes { get; } = new();

    /// <summary>The SpEffect IDs the Interrupt tests (GetSpecialEffectActivateInterruptId(id))</summary>
    public List<(int Id, int Line)> InterruptSpEffects { get; } = new();

    public const string InterruptPrefix = "INTERUPT_";

    public SortedDictionary<int, LuaAct> Acts { get; } = new();
    public List<LuaWeightRow> Rows { get; } = new();
    public List<LuaSpEffectUse> SpEffects { get; } = new();

    private static readonly HashSet<string> SpEffectMethods = new()
    {
        "HasSpecialEffectId", "AddObserveSpecialEffectAttribute", "DeleteObserveSpecialEffectAttribute",
    };

    /// <summary>The template read from a valid parse, or null if the script doesn't follow it</summary>
    public static LuaBattleGoal Read(string src, LuaParseResult parse) => Read(src, parse, out _);

    /// <summary>As <see cref="Read(string, LuaParseResult)"/>; why is the localization key of the reason it isn't the template</summary>
    public static LuaBattleGoal Read(string src, LuaParseResult parse, out string why)
    {
        if (parse?.Tree == null)
        {
            why = "LUA_Battle_Why_Invalid";
            return null;
        }

        var t = parse.Tokens;
        int Line(LuaNode n) => t[n.First].Line;

        // A script can register several goals: each RegisterTableGoal(GOAL_…, "…") starts a section with its own Goal
        // table (Goal.Activate = function (self, ai, goal) … end). The battle goal is the section whose Activate
        // calls Common_Battle_Activate; the others are helpers (e.g. an AfterAttackAct goal).
        var sections = new List<(string Goal, LuaFunctionExpr Activate, LuaFunctionExpr Interrupt)>();
        var functions = new Dictionary<string, LuaFunctionStat>();
        (string Goal, LuaFunctionExpr Activate, LuaFunctionExpr Interrupt) current = (null, null, null);

        foreach (var s in parse.Tree.Stats)
        {
            if (s is LuaCallStat { Call: { Callee: LuaNameExpr { Name: "RegisterTableGoal" } } reg } && reg.Arguments.Count > 0)
            {
                if (current.Activate != null || current.Goal != null)
                    sections.Add(current);
                current = ((reg.Arguments[0] as LuaNameExpr)?.Name, null, null);
            }
            else if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a &&
                a.Targets[0] is LuaFieldExpr { Object: LuaNameExpr { Name: "Goal" } } f && a.Values[0] is LuaFunctionExpr fn)
            {
                if (f.Name == "Activate")
                    current.Activate = fn;
                else if (f.Name is "Interrupt" or "Interupt")
                    current.Interrupt = fn;
            }
            else if (s is LuaFunctionStat fs && !fs.IsLocal)
                functions.TryAdd(fs.Name, fs);
        }
        if (current.Activate != null || current.Goal != null)
            sections.Add(current);

        if (!sections.Any(x => x.Activate != null))
        {
            why = "LUA_Battle_Why_No_Activate";
            return null;
        }

        LuaFunctionExpr activate = null, interrupt = null;
        LuaCallExpr battle = null;
        string goalName = null;
        foreach (var section in sections.Where(x => x.Activate != null))
        {
            battle = section.Activate.Body.AllStatements().SelectMany(s => s.Calls())
                .FirstOrDefault(c => c.Callee is LuaNameExpr { Name: "Common_Battle_Activate" } && c.Arguments.Count is 5 or 6);
            if (battle != null)
            {
                (goalName, activate, interrupt) = section;
                break;
            }
        }

        if (battle == null || battle.Arguments[2] is not LuaNameExpr weights || battle.Arguments[3] is not LuaNameExpr funcs)
        {
            why = "LUA_Battle_Why_No_Battle_Activate";
            return null;
        }

        var body = activate.Body;

        var result = new LuaBattleGoal
        {
            GoalName = goalName,
            Activate = activate,
            WeightTable = weights.Name,
            FunctionTable = funcs.Name,
            ActivateLine = Line(activate),
            InterruptLine = interrupt != null ? Line(interrupt) : 0,
        };
        var afterName = (battle.Arguments[4] as LuaNameExpr)?.Name;

        LuaLiteral? Literal(LuaExpr e) => e.AsNumber() is { } v ? new LuaLiteral(e.First, e.Last, v, Line(e)) : null;
        int? KeyOf(LuaExpr target, string table) =>
            target is LuaIndexExpr { Object: LuaNameExpr o, Key: var k } && o.Name == table && k.AsNumber() is { } n ? (int)n : null;

        // Acts, cooldowns and the after function
        foreach (var s in body.AllStatements())
        {
            if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a)
            {
                var value = a.Values[0] as LuaCallExpr;
                if (KeyOf(a.Targets[0], funcs.Name) is { } actId && value?.Callee is LuaNameExpr { Name: "REGIST_FUNC" } &&
                    value.Arguments.Count == 3 && value.Arguments[2] is LuaNameExpr target)
                {
                    result.Act(actId).Function = target.Name;
                    result.Act(actId).RegisterLine = Line(s);
                }
                else if (KeyOf(a.Targets[0], weights.Name) is { } coolId && value?.Callee is LuaNameExpr { Name: "SetCoolTime" } &&
                         value.Arguments.Count >= 4)
                {
                    result.Act(coolId).Cooldowns.Add((Literal(value.Arguments[2]), Literal(value.Arguments[3]), Line(s)));
                }
            }
            else if (s is LuaLocalStat { Names.Count: 1, Values.Count: 1 } l && l.Names[0] == afterName &&
                     l.Values[0] is LuaCallExpr { Callee: LuaNameExpr { Name: "REGIST_FUNC" } } after && after.Arguments.Count == 3 &&
                     after.Arguments[2] is LuaNameExpr afterTarget)
            {
                result.AfterFunction = afterTarget.Name;
            }
        }

        if (result.Acts.Count == 0)
        {
            why = "LUA_Battle_Why_No_Acts";
            return null;
        }

        // Weights, by the branch that sets them
        result.ReadRows(body, new List<string>(), weights.Name, src, t, KeyOf, Literal);

        // Act functions: sub-goals
        foreach (var act in result.Acts.Values)
        {
            if (act.Function == null || !functions.TryGetValue(act.Function, out var fs))
                continue;
            act.FunctionLine = Line(fs);
            act.FunctionNode = fs.Function;
            result.ReadSubGoals(act, fs.Function, src, t, Literal);
        }
        if (result.AfterFunction != null && functions.TryGetValue(result.AfterFunction, out var af))
        {
            result.AfterFunctionLine = Line(af);
            result.AfterFunctionNode = af.Function;
        }

        // The Interrupt, the functions it can call, and what it reacts to
        result.Interrupt = interrupt;
        foreach (var (name, fs) in functions)
            result.Functions[name] = fs.Function;

        // Observed SpEffects, in every function of the script (Goal.X = function … included)
        var bodies = new List<LuaBlock> { parse.Tree };
        bodies.AddRange(parse.Tree.Stats.OfType<LuaAssignStat>().SelectMany(a => a.Values).OfType<LuaFunctionExpr>().Select(fn => fn.Body));
        foreach (var block in bodies)
        {
            foreach (var c in block.AllStatements().SelectMany(s => s.Calls()))
            {
                if (c.Method == "AddObserveSpecialEffectAttribute" && c.Arguments.Count == 2 &&
                    c.Arguments[0] is LuaNameExpr who && c.Arguments[1].AsNumber() is { } observed &&
                    !result.ObservedSpEffects.Any(o => o.Target == who.Name && o.Id == (int)observed))
                    result.ObservedSpEffects.Add((who.Name, (int)observed, Line(c)));
            }
        }

        if (interrupt != null)
        {
            foreach (var c in interrupt.Body.AllStatements().SelectMany(s => s.Calls()))
            {
                if (c.Method == "IsInterupt" && c.Arguments.Count == 1 && c.Arguments[0] is LuaNameExpr { Name: var type } &&
                    type.StartsWith(InterruptPrefix, StringComparison.Ordinal) &&
                    !result.InterruptTypes.Any(x => x.Type == type[InterruptPrefix.Length..]))
                    result.InterruptTypes.Add((type[InterruptPrefix.Length..], Line(c)));
                else if (c.Method == "GetSpecialEffectActivateInterruptId" && c.Arguments.Count == 1 &&
                         c.Arguments[0].AsNumber() is { } tested && !result.InterruptSpEffects.Any(x => x.Id == (int)tested))
                    result.InterruptSpEffects.Add(((int)tested, Line(c)));
            }
        }

        // Special effects, in Activate, Interrupt and the act functions
        var scanned = new List<LuaBlock> { body };
        if (interrupt != null)
            scanned.Add(interrupt.Body);
        scanned.AddRange(result.Acts.Values.Where(a => a.Function != null && functions.ContainsKey(a.Function))
            .Select(a => functions[a.Function].Function.Body));
        foreach (var block in scanned)
        {
            foreach (var c in block.AllStatements().SelectMany(s => s.Calls()))
            {
                if (c.Method != null && SpEffectMethods.Contains(c.Method) && c.Arguments.Count >= 2 && c.Arguments[1].AsNumber() is { } id)
                    result.SpEffects.Add(new LuaSpEffectUse((int)id, c.Method, Line(c)));
            }
        }

        why = null;
        return result;
    }

    private LuaAct Act(int id)
    {
        if (!Acts.TryGetValue(id, out var act))
            Acts[id] = act = new LuaAct { Id = id };
        return act;
    }

    private void ReadRows(LuaBlock block, List<string> path, string weights, string src, List<LuaToken> t,
        Func<LuaExpr, string, int?> keyOf, Func<LuaExpr, LuaLiteral?> literal)
    {
        LuaWeightRow run = null;

        foreach (var s in block.Stats)
        {
            if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a && keyOf(a.Targets[0], weights) is { } act &&
                literal(a.Values[0]) is { } value)
            {
                if (run == null)
                {
                    run = new LuaWeightRow { Line = t[s.First].Line };
                    run.Path.AddRange(path);
                    Rows.Add(run);
                }
                run.Weights[act] = value;
                continue;
            }

            switch (s)
            {
                case LuaIfStat i:
                    run = null;
                    for (int c = 0; c < i.Clauses.Count; c++)
                    {
                        var label = (c == 0 ? "if " : "elseif ") + i.Clauses[c].Condition.SourceOf(src, t);
                        ReadRows(i.Clauses[c].Body, Append(path, label), weights, src, t, keyOf, literal);
                    }
                    if (i.Else != null)
                        ReadRows(i.Else, Append(path, "else"), weights, src, t, keyOf, literal);
                    break;
                case LuaLoopStat l:
                    run = null;
                    ReadRows(l.Body, Append(path, l.Keyword + " …"), weights, src, t, keyOf, literal);
                    break;
                default:
                    // Other statements (a SetCoolTime, a call) end the run, so rows keep their source order
                    run = null;
                    break;
            }
        }
    }

    private static List<string> Append(List<string> path, string label)
    {
        var p = new List<string>(path) { label };
        return p;
    }

    private void ReadSubGoals(LuaAct act, LuaFunctionExpr fn, string src, List<LuaToken> t, Func<LuaExpr, LuaLiteral?> literal)
    {
        // Locals set once to a number (local anim = 3000 … AddSubGoal(…, anim, …)): their literal is the value to edit
        var assigned = fn.Body.AllStatements().OfType<LuaAssignStat>()
            .SelectMany(a => a.Targets).OfType<LuaNameExpr>().Select(n => n.Name).ToHashSet();
        var constants = new Dictionary<string, LuaLiteral>();
        foreach (var l in fn.Body.AllStatements().OfType<LuaLocalStat>())
        {
            if (l.Names.Count == 1 && l.Values.Count == 1 && !assigned.Contains(l.Names[0]) && literal(l.Values[0]) is { } v)
            {
                if (!constants.TryAdd(l.Names[0], v))
                    constants[l.Names[0]] = default; // declared twice: not one value
            }
        }

        foreach (var c in fn.Body.AllStatements().SelectMany(s => s.Calls()))
        {
            if (c.Method != "AddSubGoal" || c.Arguments.Count < 2)
                continue;

            var sub = new LuaSubGoal
            {
                Goal = c.Arguments[0] is LuaNameExpr g ? g.Name : c.Arguments[0].SourceOf(src, t),
                Line = t[c.First].Line,
            };
            foreach (var arg in c.Arguments.Skip(1))
            {
                LuaLiteral? lit = literal(arg);
                if (lit == null && arg is LuaNameExpr n && constants.TryGetValue(n.Name, out var cv) && cv.Last > 0)
                    lit = cv;
                sub.Arguments.Add((arg.SourceOf(src, t), lit));
            }
            act.SubGoals.Add(sub);
        }
    }

    /// <summary>A number as Lua source: integers without a decimal point</summary>
    public static string Format(double v) =>
        v == Math.Floor(v) && Math.Abs(v) < 1e15 ? ((long)v).ToString(CultureInfo.InvariantCulture) : v.ToString("R", CultureInfo.InvariantCulture);

    /// <summary>The source with one literal replaced by a new value</summary>
    public static string ReplaceLiteral(string src, List<LuaToken> tokens, LuaLiteral literal, double value)
    {
        var a = tokens[literal.First];
        var b = tokens[literal.Last];
        var end = b.Start + b.Length;
        return string.Concat(src.AsSpan(0, a.Start), Format(value), src.AsSpan(end));
    }
}
