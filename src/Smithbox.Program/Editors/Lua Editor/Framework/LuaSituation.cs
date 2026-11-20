using System.Globalization;

namespace StudioCore.Editors.LuaEditor;

public enum LuaInputKind
{
    Boolean,
    Number,
    /// <summary>One of the named constants the value is compared with (ROLE_TYPE_Kankyaku, …), or "other"</summary>
    Choice,
}

/// <summary>
/// One thing Goal.Activate's conditions depend on that the script can't work out itself: a call into the game
/// (ai:GetDist(TARGET_ENE_0), ai:IsInsideTargetCustom(…)), a script helper, or a global. The user sets its value.
/// </summary>
public class LuaSituationInput
{
    /// <summary>What identifies it: the call's source text, a global's name, or "cooldown:&lt;anim&gt;"</summary>
    public string Key;

    /// <summary>The local it is stored in (targetDist), else a short form of the call</summary>
    public string Label;

    /// <summary>The full source text (the call), for a tooltip</summary>
    public string Detail;

    public int Line;

    public LuaInputKind Kind;
    public double Number;
    public bool Boolean;
    public string Choice;

    /// <summary>For a Choice: the constants it is compared with, in first-use order</summary>
    public List<string> Choices { get; } = new();

    // Kind hints from the static pass
    internal bool UsedAsNumber;
    internal bool UsedAsBoolean;

    // A random roll or HP rate: a number box, even when it is only tested for truth
    internal bool PrefersNumber;

    /// <summary>ai:GetDist(TARGET_ENE_0): the simulation sets it from where the characters stand</summary>
    public bool IsTargetDistance;

    /// <summary>A SetCoolTime input: the animation and seconds, which the simulation checks against what it played</summary>
    public long? CooldownAnimation;
    public double? CooldownSeconds;

    /// <summary>Set by the simulation, not the user, while it runs</summary>
    public bool IsLive => IsTargetDistance || CooldownAnimation != null;
}

public enum LuaStepKind
{
    /// <summary>Play one animation (an attack, combo or animation goal), facing the target</summary>
    Clip,
    /// <summary>Walk or run toward the target until within Distance</summary>
    Approach,
    /// <summary>Back away from the target until Distance away</summary>
    Leave,
    /// <summary>Strafe around the target, to the right or left</summary>
    Sideway,
    /// <summary>Turn to face the target (within Angle)</summary>
    Turn,
    /// <summary>Stand for Life seconds</summary>
    Wait,
    /// <summary>A sub-goal the viewer can't show (listed only)</summary>
    Other,
}

/// <summary>One thing an act does, from a sub-goal on a branch of its function that runs</summary>
public class LuaActStep
{
    public LuaStepKind Kind;
    public string Goal;
    public int Line;

    /// <summary>The goal's life: its time limit, in seconds</summary>
    public double Life;

    public long Animation;
    public double Distance;
    public bool Walk;
    public bool Right;
    public double Angle;
}

/// <summary>What one evaluation of Goal.Activate gives: each act's final weight, and what was set on the way</summary>
public class LuaSituationResult
{
    public SortedDictionary<int, double> Weights { get; } = new();

    /// <summary>The weight literals the run assigned (their first token), to show which situations ran</summary>
    public HashSet<int> AssignedLiterals { get; } = new();

    /// <summary>Conditions that couldn't be decided (counted as false), and statements skipped</summary>
    public List<(int Line, string Message)> Notes { get; } = new();

    /// <summary>The line of the last weight assignment that ran (the situation's own block)</summary>
    public int LastWeightLine;
}

/// <summary>
/// Runs a battle goal's Goal.Activate for a situation the user describes. A static pass first finds everything its
/// conditions read from the game (the inputs) and how each is used: compared with numbers, used as a condition, or
/// compared with named constants. Evaluating then follows Activate's locals, if / elseif / else chains and weight
/// assignments with those values, as Lua would (nil and false are false, everything else true; SetCoolTime gives
/// 0 when the user marks its animation as recently used). Loops and other statements are skipped and noted.
/// </summary>
public class LuaSituation
{
    private readonly LuaBattleGoal Goal;
    private readonly string Src;
    private readonly List<LuaToken> Tokens;
    private readonly IReadOnlyDictionary<string, double> Constants;

    public List<LuaSituationInput> Inputs { get; } = new();
    private readonly Dictionary<string, LuaSituationInput> ByKey = new();

    // Locals bound to an input (local targetDist = ai:GetDist(TARGET_ENE_0)), from the static pass
    private readonly Dictionary<string, List<LuaSituationInput>> LocalInputs = new();

    /// <param name="constants">Numeric global constants (from aicommon: goal IDs, enums), to compare by value; may be empty</param>
    public LuaSituation(LuaBattleGoal goal, string src, List<LuaToken> tokens, IReadOnlyDictionary<string, double> constants)
    {
        Goal = goal;
        Src = src;
        Tokens = tokens;
        Constants = constants ?? new Dictionary<string, double>();
        Collect(goal.Activate.Body);

        // Only what a condition or a weight reads is shown: a call whose result nothing tests (an act function
        // registration, a local never compared) can't change which act is picked. Evaluation still knows them all.
        Inputs.RemoveAll(i => !i.UsedAsNumber && !i.UsedAsBoolean && i.Choices.Count == 0);

        foreach (var input in Inputs)
        {
            input.Kind = input.Choices.Count > 0 ? LuaInputKind.Choice
                : input.UsedAsNumber || input.PrefersNumber && !input.UsedAsBoolean ? LuaInputKind.Number
                : LuaInputKind.Boolean;
            if (input.Kind == LuaInputKind.Choice)
                input.Choice = input.Choices[0];
        }
    }

    // --- static pass: the inputs ---

    private static bool IsConstantName(string name) => name.Length > 1 && name.All(c => !char.IsLower(c)) && name.Any(char.IsLetter);

    private LuaSituationInput Input(string key, string label, string detail, LuaNode at)
    {
        if (!ByKey.TryGetValue(key, out var input))
        {
            input = new LuaSituationInput { Key = key, Label = label, Detail = detail, Line = Tokens[at.First].Line };
            ByKey[key] = input;
            Inputs.Add(input);
        }
        return input;
    }

    private string Text(LuaNode n) => n.SourceOf(Src, Tokens);

    private void Collect(LuaBlock block)
    {
        foreach (var s in block.Stats)
        {
            switch (s)
            {
                case LuaLocalStat l:
                    for (int i = 0; i < l.Names.Count && i < l.Values.Count; i++)
                        Bind(l.Names[i], l.Values[i]);
                    break;
                case LuaAssignStat a:
                    for (int i = 0; i < a.Values.Count && i < a.Targets.Count; i++)
                    {
                        if (a.Targets[i] is LuaNameExpr target)
                            Bind(target.Name, a.Values[i]);
                        // weights[n] = <value>: what the value reads decides a weight
                        else if (a.Targets[i] is LuaIndexExpr { Object: LuaNameExpr t } && t.Name == Goal.WeightTable)
                            Visit(a.Values[i], Hint.Number);
                        // Other tables (the act functions: funcs[n] = REGIST_FUNC(…), the params) don't change the weights
                    }
                    break;
                case LuaIfStat i:
                    foreach (var c in i.Clauses)
                    {
                        Visit(c.Condition, Hint.Boolean);
                        Collect(c.Body);
                    }
                    if (i.Else != null)
                        Collect(i.Else);
                    break;
                case LuaLoopStat loop:
                    Collect(loop.Body);
                    break;
            }
        }
    }

    /// <summary>
    /// name = &lt;call&gt; (local or not): the call is an input, named after the variable, and the variable's uses tell its
    /// kind. A variable can be set from several calls (f = ai:GetRandam_Int(10, 120) in one branch, (10, 80) in
    /// another): each is its own input, and every use of the variable marks all of them.
    /// </summary>
    private void Bind(string name, LuaExpr value)
    {
        if (value is not LuaCallExpr call || IsCooldown(call))
        {
            Visit(value, null);
            return;
        }

        var input = Live(Input(Text(call), name, Text(call), call), call);
        if (!LocalInputs.TryGetValue(name, out var list))
            LocalInputs[name] = list = new List<LuaSituationInput>();
        if (!list.Contains(input))
            list.Add(input);

        if (call.Method is "GetRandam_Int" or "GetRandam_Float" && call.Arguments.Count == 2 &&
            call.Arguments[0].AsNumber() is { } lo && call.Arguments[1].AsNumber() is { } hi)
        {
            input.PrefersNumber = true;
            input.Number = Math.Round((lo + hi) / 2);
        }
        if (call.Method == "GetHpRate")
        {
            input.PrefersNumber = true;
            input.Number = 1;
        }
    }

    /// <summary>Marks an input the simulation sets from the viewport: the distance to the target</summary>
    private static LuaSituationInput Live(LuaSituationInput input, LuaCallExpr call)
    {
        if (call.Method == "GetDist" && call.Arguments is [LuaNameExpr { Name: "TARGET_ENE_0" }])
            input.IsTargetDistance = true;
        return input;
    }

    private enum Hint { Boolean, Number }

    private static bool IsCooldown(LuaCallExpr c) => c.Callee is LuaNameExpr { Name: "SetCoolTime" } && c.Arguments.Count >= 5;

    private static bool IsComparison(string op) => op is "==" or "~=" or "<" or "<=" or ">" or ">=";

    private static bool IsArithmetic(string op) => op is "+" or "-" or "*" or "/" or "^";

    /// <summary>Registers the inputs an expression reads; hint is how its value is used</summary>
    private void Visit(LuaExpr e, Hint? hint, string comparedWith = null)
    {
        switch (e)
        {
            case LuaCallExpr c when IsCooldown(c):
                var anim = c.Arguments[2].AsNumber();
                var input = Input($"cooldown:{Text(c.Arguments[2])}",
                    LOC.Get("LUA_Situation_Cooldown", anim is { } av ? LuaBattleGoal.Format(av) : Text(c.Arguments[2]),
                        c.Arguments[3].AsNumber() is { } sec ? LuaBattleGoal.Format(sec) : Text(c.Arguments[3])),
                    Text(c), c);
                input.UsedAsBoolean = true;
                input.CooldownAnimation = anim is { } ca ? (long)ca : null;
                input.CooldownSeconds = c.Arguments[3].AsNumber();
                Visit(c.Arguments[4], Hint.Number);
                break;
            case LuaCallExpr c:
                Mark(Live(Input(Text(c), ShortCall(c), Text(c), c), c), hint, comparedWith);
                break;
            case LuaNameExpr n when LocalInputs.TryGetValue(n.Name, out var bound):
                foreach (var b in bound)
                    Mark(b, hint, comparedWith);
                break;
            case LuaNameExpr n when !IsConstantName(n.Name) && n.Name != Goal.WeightTable:
                // A global the script reads (set elsewhere): the user sets it too. Locals the script computes
                // aren't inputs (they are evaluated), and aren't distinguishable here, so only names never
                // declared in Activate count
                if (!DeclaredLocals.Value.Contains(n.Name))
                    Mark(Input(n.Name, n.Name, n.Name, n), hint, comparedWith);
                break;
            case LuaBinaryExpr b when IsComparison(b.Operator):
                var leftOther = Describe(b.Right);
                var rightOther = Describe(b.Left);
                // <, <=, >, >= only compare numbers, whatever the other side is (dist <= randInt2)
                var ordering = b.Operator is "<" or "<=" or ">" or ">=";
                Visit(b.Left, leftOther.Hint ?? (ordering ? Hint.Number : null), leftOther.Constant);
                Visit(b.Right, rightOther.Hint ?? (ordering ? Hint.Number : null), rightOther.Constant);
                break;
            case LuaBinaryExpr b when IsArithmetic(b.Operator):
                Visit(b.Left, Hint.Number);
                Visit(b.Right, Hint.Number);
                break;
            case LuaBinaryExpr b:
                Visit(b.Left, b.Operator is "and" or "or" ? hint : null);
                Visit(b.Right, b.Operator is "and" or "or" ? hint : null);
                break;
            case LuaUnaryExpr u:
                Visit(u.Operand, u.Operator == "not" ? Hint.Boolean : Hint.Number);
                break;
            case LuaParenExpr p:
                Visit(p.Inner, hint, comparedWith);
                break;
            case LuaIndexExpr ix:
                Visit(ix.Key, Hint.Number);
                break;
        }
    }

    private Lazy<HashSet<string>> _declared;
    private Lazy<HashSet<string>> DeclaredLocals => _declared ??= new(() =>
        Goal.Activate.Body.AllStatements().OfType<LuaLocalStat>().SelectMany(l => l.Names)
            .Concat(Goal.Activate.Parameters).ToHashSet());

    /// <summary>How the other side of a comparison makes this side be read: a number, a boolean, or a named constant</summary>
    private (Hint? Hint, string Constant) Describe(LuaExpr other) => other switch
    {
        LuaConstantExpr { Keyword: "true" or "false" } => (Hint.Boolean, null),
        LuaNameExpr n when IsConstantName(n.Name) => (null, n.Name),
        _ when other.AsNumber() != null => (Hint.Number, null),
        LuaBinaryExpr b when IsArithmetic(b.Operator) => (Hint.Number, null),
        _ => (null, null),
    };

    private static void Mark(LuaSituationInput input, Hint? hint, string comparedWith)
    {
        if (comparedWith != null)
        {
            if (!input.Choices.Contains(comparedWith))
                input.Choices.Add(comparedWith);
            return;
        }
        if (hint == Hint.Number)
            input.UsedAsNumber = true;
        else if (hint == Hint.Boolean)
            input.UsedAsBoolean = true;
    }

    private string ShortCall(LuaCallExpr c)
    {
        var name = c.Method ?? (c.Callee is LuaNameExpr n ? n.Name : Text(c.Callee));
        var args = string.Join(", ", c.Arguments.Select(Text));
        var s = $"{name}({args})";
        return s.Length > 48 ? s[..45] + "...)" : s;
    }

    /// <summary>The value "other" stands for in a Choice: a constant the script never compares with</summary>
    public const string OtherChoice = "";

    // --- evaluation ---

    private enum VKind { Nil, Bool, Number, Symbol, Unknown }

    private readonly record struct V(VKind Kind, double N = 0, bool B = false, string S = null)
    {
        public bool Truthy => Kind switch
        {
            VKind.Nil => false,
            VKind.Bool => B,
            _ => true,
        };
    }

    private static readonly V Nil = new(VKind.Nil);
    private static readonly V Unknown = new(VKind.Unknown);

    private sealed class Run
    {
        public readonly Dictionary<string, V> Locals = new();
        public readonly LuaSituationResult Result = new();
        public bool Returned;

        // Act planning: each call statement that runs, and the live distance to the target
        public Action<LuaCallExpr, Run> OnCall;
        public double? TargetDistance;
    }

    // --- act planning ---

    /// <summary>
    /// What an act does in this situation, as steps for the viewer: its function runs with the situation's values
    /// (ai:GetDist(TARGET_ENE_0) is the live distance to the target; a random roll is its middle), and each sub-goal
    /// on a branch that runs becomes a step, in order. Approach_Act_Flex (aicommon's approach helper) is followed
    /// as the game would: walk or run by distance, then approach if further than its distance.
    /// </summary>
    public List<LuaActStep> PlanAct(LuaAct act, double targetDistance) => PlanFunction(act.FunctionNode, targetDistance);

    /// <summary>As <see cref="PlanAct"/>, for any function of the goal (the after function, …ActAfter_AdjustSpace)</summary>
    public List<LuaActStep> PlanFunction(LuaFunctionExpr function, double targetDistance)
    {
        var steps = new List<LuaActStep>();
        if (function == null)
            return steps;

        var run = new Run { TargetDistance = targetDistance };
        run.OnCall = (call, r) =>
        {
            if (StepOf(call, r, targetDistance) is { } step)
                steps.Add(step);
        };
        Exec(function.Body, run);
        return steps;
    }

    private LuaActStep StepOf(LuaCallExpr c, Run run, double targetDistance)
    {
        double? Num(int i) => i < c.Arguments.Count && Eval(c.Arguments[i], run) is { Kind: VKind.Number } v ? v.N : null;
        bool Truthy(int i) => i < c.Arguments.Count && Eval(c.Arguments[i], run).Truthy;
        var line = Tokens[c.First].Line;

        // Approach_Act_Flex(ai, goal, distance, runDistance, alwaysRunDistance, runOdds, guardOdds[, walkLife, runLife])
        if (c.Callee is LuaNameExpr { Name: "Approach_Act_Flex" } && c.Arguments.Count >= 7)
        {
            var distance = Num(2) ?? 0;
            var walk = true;
            if (Num(4) is { } always && always <= targetDistance)
                walk = false;
            else if (Num(3) is { } runFrom && runFrom <= targetDistance && (Num(5) ?? 0) >= 50)
                walk = false;
            if (distance > targetDistance)
                return null;
            return new LuaActStep
            {
                Kind = LuaStepKind.Approach, Goal = "Approach_Act_Flex", Line = line, Distance = distance, Walk = walk,
                Life = (walk ? Num(7) : Num(8)) ?? (walk ? 3 : 8),
            };
        }

        // goal:AddSubGoal(GOAL_…, life, args…)
        if (c.Method != "AddSubGoal" || c.Arguments.Count < 2)
            return null;

        var goal = c.Arguments[0] is LuaNameExpr g ? g.Name : Text(c.Arguments[0]);
        var name = goal.StartsWith("GOAL_COMMON_") ? goal["GOAL_COMMON_".Length..] : goal;
        var step = new LuaActStep { Goal = goal, Line = line, Life = Num(1) ?? 5 };

        if (name.StartsWith("ApproachTarget") || name.StartsWith("ApproachSettingDirection"))
        {
            // (life, target, distance, turnTarget, walk, guard)
            step.Kind = LuaStepKind.Approach;
            step.Distance = Num(3) ?? 1;
            step.Walk = Truthy(5);
        }
        else if (name.StartsWith("LeaveTarget"))
        {
            // (life, target, distance, turnTarget, walk, guard)
            step.Kind = LuaStepKind.Leave;
            step.Distance = Num(3) ?? 5;
            step.Walk = Truthy(5);
        }
        else if (name.StartsWith("SidewayMove") || name.StartsWith("FlexibleSideway"))
        {
            // (life, target, right (0 or 1), angle, turnTarget, walk, guard)
            step.Kind = LuaStepKind.Sideway;
            step.Right = (Num(3) ?? 0) >= 1;
            step.Angle = Num(4) ?? 45;
        }
        else if (name == "Turn" || name.StartsWith("Turn_") || name == "SpecialTurn")
        {
            // (life, target, facing angle, guard…)
            step.Kind = LuaStepKind.Turn;
            step.Angle = Num(3) ?? 30;
        }
        else if (name == "Wait")
        {
            step.Kind = LuaStepKind.Wait;
        }
        else if (name == "StepSafety")
        {
            // (life, front, back, left, right priorities, turnTarget, …): the highest priority that is set (>= 0)
            // plays its step, a000_006000 (front), 6001 (back), 6002 (left) or 6003 (right)
            var best = -1;
            var bestPriority = double.MinValue;
            for (int d = 0; d < 4; d++)
            {
                if (Num(2 + d) is { } p && p >= 0 && p > bestPriority)
                {
                    best = d;
                    bestPriority = p;
                }
            }
            step.Kind = best >= 0 ? LuaStepKind.Clip : LuaStepKind.Other;
            step.Animation = 6000 + Math.Max(best, 0);
        }
        else if (Num(2) is { } anim && anim == Math.Floor(anim) && anim >= 1000 && anim < 1_000_000)
        {
            // An attack, combo or animation goal: (life, animation, target, …)
            step.Kind = LuaStepKind.Clip;
            step.Animation = (long)anim;
        }
        else
        {
            step.Kind = LuaStepKind.Other;
        }
        return step;
    }

    /// <summary>Runs Activate with the inputs' current values</summary>
    public LuaSituationResult Evaluate()
    {
        var run = new Run();
        foreach (var act in Goal.Acts.Keys)
            run.Result.Weights[act] = 0;
        Exec(Goal.Activate.Body, run);
        return run.Result;
    }

    private void Exec(LuaBlock block, Run run)
    {
        foreach (var s in block.Stats)
        {
            if (run.Returned)
                return;

            switch (s)
            {
                case LuaLocalStat l:
                    for (int i = 0; i < l.Names.Count; i++)
                        run.Locals[l.Names[i]] = i < l.Values.Count ? Eval(l.Values[i], run) : Nil;
                    break;

                case LuaAssignStat a:
                    for (int i = 0; i < a.Targets.Count; i++)
                    {
                        var value = i < a.Values.Count ? Eval(a.Values[i], run) : Nil;
                        if (a.Targets[i] is LuaIndexExpr { Object: LuaNameExpr t, Key: var k } && t.Name == Goal.WeightTable && k.AsNumber() is { } act)
                        {
                            run.Result.Weights[(int)act] = value.Kind == VKind.Number ? value.N : 0;
                            if (i < a.Values.Count && a.Values[i].AsNumber() != null)
                            {
                                run.Result.AssignedLiterals.Add(a.Values[i].First);
                                run.Result.LastWeightLine = Tokens[s.First].Line;
                            }
                        }
                        else if (a.Targets[i] is LuaNameExpr n)
                            run.Locals[n.Name] = value;
                    }
                    break;

                case LuaIfStat i:
                    var taken = false;
                    foreach (var c in i.Clauses)
                    {
                        var cond = Eval(c.Condition, run);
                        if (cond.Kind == VKind.Unknown)
                            run.Result.Notes.Add((Tokens[c.Condition.First].Line, LOC.Get("LUA_Situation_Undecided", Text(c.Condition))));
                        if (cond.Kind != VKind.Unknown && cond.Truthy)
                        {
                            Exec(c.Body, run);
                            taken = true;
                            break;
                        }
                    }
                    if (!taken && i.Else != null)
                        Exec(i.Else, run);
                    break;

                case LuaCallStat call:
                    run.OnCall?.Invoke(call.Call, run);
                    break;

                case LuaReturnStat:
                    run.Returned = true;
                    return;

                case LuaLoopStat loop:
                    run.Result.Notes.Add((Tokens[s.First].Line, LOC.Get("LUA_Situation_Loop_Skipped", loop.Keyword)));
                    break;
            }
        }
    }

    private V Eval(LuaExpr e, Run run)
    {
        switch (e)
        {
            case LuaNumberExpr n:
                return new V(VKind.Number, n.Value);
            case LuaStringExpr:
                return Unknown;
            case LuaConstantExpr c:
                return c.Keyword == "nil" ? Nil : new V(VKind.Bool, B: c.Keyword == "true");
            case LuaParenExpr p:
                return Eval(p.Inner, run);

            case LuaNameExpr n:
                if (run.Locals.TryGetValue(n.Name, out var local))
                    return local;
                if (ByKey.TryGetValue(n.Name, out var global))
                    return Value(global);
                if (IsConstantName(n.Name))
                    return Constants.TryGetValue(n.Name, out var cv) ? new V(VKind.Number, cv) : new V(VKind.Symbol, S: n.Name);
                return Nil;

            case LuaCallExpr c when IsCooldown(c):
                var cooling = ByKey.TryGetValue($"cooldown:{Text(c.Arguments[2])}", out var cd) && cd.Boolean;
                return cooling ? new V(VKind.Number, 0) : Eval(c.Arguments[4], run);

            // Planning an act: the distance to the target is where the viewer has the characters
            case LuaCallExpr { Method: "GetDist", Arguments: [LuaNameExpr { Name: "TARGET_ENE_0" }] } when run.TargetDistance is { } d:
                return new V(VKind.Number, d);

            case LuaCallExpr c:
                if (ByKey.TryGetValue(Text(c), out var input))
                    return Value(input);
                // A random roll the situation doesn't set (in an act function): its middle
                if (run.TargetDistance != null && c.Method is "GetRandam_Int" or "GetRandam_Float" && c.Arguments.Count == 2 &&
                    c.Arguments[0].AsNumber() is { } lo && c.Arguments[1].AsNumber() is { } hi)
                    return new V(VKind.Number, c.Method == "GetRandam_Int" ? Math.Round((lo + hi) / 2) : (lo + hi) / 2);
                return Unknown;

            case LuaIndexExpr { Object: LuaNameExpr t, Key: var k } when t.Name == Goal.WeightTable && k.AsNumber() is { } act:
                return new V(VKind.Number, run.Result.Weights.GetValueOrDefault((int)act));

            case LuaUnaryExpr u:
                var operand = Eval(u.Operand, run);
                if (u.Operator == "not")
                    return operand.Kind == VKind.Unknown ? Unknown : new V(VKind.Bool, B: !operand.Truthy);
                return operand.Kind == VKind.Number ? new V(VKind.Number, -operand.N) : Unknown;

            case LuaBinaryExpr b:
                return Binary(b, run);

            default:
                return Unknown;
        }
    }

    private V Binary(LuaBinaryExpr b, Run run)
    {
        // and / or short-circuit, and give one of their operands, as in Lua
        if (b.Operator == "and")
        {
            var l = Eval(b.Left, run);
            if (l.Kind == VKind.Unknown)
                return Unknown;
            return !l.Truthy ? l : Eval(b.Right, run);
        }
        if (b.Operator == "or")
        {
            var l = Eval(b.Left, run);
            if (l.Kind == VKind.Unknown)
                return Unknown;
            return l.Truthy ? l : Eval(b.Right, run);
        }

        var x = Eval(b.Left, run);
        var y = Eval(b.Right, run);
        if (x.Kind == VKind.Unknown || y.Kind == VKind.Unknown)
            return Unknown;

        switch (b.Operator)
        {
            case "==":
                return new V(VKind.Bool, B: Same(x, y));
            case "~=":
                return new V(VKind.Bool, B: !Same(x, y));
        }

        if (x.Kind != VKind.Number || y.Kind != VKind.Number)
            return Unknown;

        return b.Operator switch
        {
            "+" => new V(VKind.Number, x.N + y.N),
            "-" => new V(VKind.Number, x.N - y.N),
            "*" => new V(VKind.Number, x.N * y.N),
            "/" => new V(VKind.Number, x.N / y.N),
            "^" => new V(VKind.Number, Math.Pow(x.N, y.N)),
            "<" => new V(VKind.Bool, B: x.N < y.N),
            "<=" => new V(VKind.Bool, B: x.N <= y.N),
            ">" => new V(VKind.Bool, B: x.N > y.N),
            ">=" => new V(VKind.Bool, B: x.N >= y.N),
            _ => Unknown,
        };
    }

    private static bool Same(V x, V y) => x.Kind == y.Kind && x.Kind switch
    {
        VKind.Nil => true,
        VKind.Bool => x.B == y.B,
        VKind.Number => x.N == y.N,
        VKind.Symbol => x.S == y.S,
        _ => false,
    };

    private V Value(LuaSituationInput input) => input.Kind switch
    {
        LuaInputKind.Boolean => new V(VKind.Bool, B: input.Boolean),
        LuaInputKind.Number => new V(VKind.Number, input.Number),
        _ => input.Choice == OtherChoice || input.Choice == null
            ? new V(VKind.Symbol, S: "\0other")
            : Constants.TryGetValue(input.Choice, out var cv) ? new V(VKind.Number, cv) : new V(VKind.Symbol, S: input.Choice),
    };

    /// <summary>Each act's share of the total weight, as a percentage (0 when nothing has weight)</summary>
    public static SortedDictionary<int, double> Percentages(LuaSituationResult result)
    {
        var total = result.Weights.Values.Where(w => w > 0).Sum();
        var p = new SortedDictionary<int, double>();
        foreach (var (act, w) in result.Weights)
            p[act] = total > 0 && w > 0 ? 100.0 * w / total : 0;
        return p;
    }

    /// <summary>A number for an input box</summary>
    public static string Format(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
}
