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

    /// <summary>An odds goal's act: its clause of the odds chain (FunctionNode is then Activate), and the weight term</summary>
    public int? OddsClause;
    public LuaExpr OddsTerm;

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

    /// <summary>
    /// The smallest number read as an animation ID: DS3 and BB play their steps as a000_000700-703 (SpinStep), so 100;
    /// smaller ones are flags and counts. Candidates only: the viewer plays what the character has.
    /// </summary>
    public const int MinAnimationId = 100;

    private static bool IsAnimationId(double v) => v == Math.Floor(v) && v >= MinAnimationId && v < 1_000_000;
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

/// <summary>How a battle goal picks an act</summary>
public enum LuaGoalKind
{
    /// <summary>A weight table and Common_Battle_Activate (ER, SDT, most of DS3 and BB)</summary>
    Table,

    /// <summary>Activate's own odds chain: weights in locals, then if roll &lt;= w1 … elseif roll &lt;= w1 + w2 … (DS3)</summary>
    Odds,

    /// <summary>Activate adds its sub-goals itself, by its conditions: one act, Activate</summary>
    Direct,
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

    /// <summary>The act table this was read from: the function that picks its acts (see <see cref="Tables"/>)</summary>
    public string Table;

    /// <summary>
    /// The battle goal's act tables, by the function that picks each one's acts: <see cref="MainTable"/> when Activate
    /// does; SDT's deflect-exchange table (<see cref="KengekiEntry"/>); and any function of the script Activate calls
    /// that picks acts itself (BB: Avater_B_BoarKing701000Battle, called from some of Activate's branches)
    /// </summary>
    public List<string> Tables { get; } = new();

    /// <summary>The table Activate picks itself</summary>
    public const string MainTable = "Activate";

    /// <summary>The goal method of SDT's deflect-exchange act table</summary>
    public const string KengekiEntry = MethodPrefix + "Kengeki_Activate";

    // The functions that pick an act from a weight table: aicommon's Common_Battle_Activate, SDT's Common_Kengeki_Activate,
    // DB_NT's Common_Battle_Activate_PCbot (player-companion NPCs) — the same (ai, goal, weights, funcs, after[, params])
    private static readonly string[] Pickers = { "Common_Battle_Activate", "Common_Kengeki_Activate" };

    private static bool IsPicker(string name) => Pickers.Any(p => name.StartsWith(p, StringComparison.Ordinal));

    /// <summary>The template read from a valid parse, or null if the script doesn't follow it</summary>
    public static LuaBattleGoal Read(string src, LuaParseResult parse) => Read(src, parse, out _);

    /// <summary>The call in an act table's function that picks an act (Common_Battle_Activate(ai, goal, weights, funcs, after[, params]))</summary>
    private static LuaCallExpr PickerCall(LuaFunctionExpr fn) =>
        fn.Body.AllStatements().SelectMany(s => s.Calls())
            .FirstOrDefault(c => c.Callee is LuaNameExpr { Name: var n } && IsPicker(n) && c.Arguments.Count is 5 or 6);

    /// <summary>
    /// A section's act tables, in order: Activate itself, the Kengeki method, then the functions Activate calls that
    /// pick acts (a global, or the goal's method self.X / Goal.X), each once
    /// </summary>
    private static List<(string Name, LuaFunctionExpr Fn, LuaCallExpr Picker)> ActTables(Section section,
        Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)> functions)
    {
        var list = new List<(string, LuaFunctionExpr, LuaCallExpr)>();
        if (PickerCall(section.Activate) is { } own)
            list.Add((MainTable, section.Activate, own));
        if (section.Methods.TryGetValue(KengekiEntry, out var k) && PickerCall(k.Fn) is { } kengeki)
            list.Add((KengekiEntry, k.Fn, kengeki));

        var self = section.Activate.Parameters.Count > 0 ? section.Activate.Parameters[0] : null;
        foreach (var call in section.Activate.Body.AllStatements().SelectMany(s => s.Calls()))
        {
            var name = call switch
            {
                { Method: null, Callee: LuaNameExpr n } => n.Name,
                { Method: null, Callee: LuaFieldExpr { Object: LuaNameExpr o, Name: var m } } when o.Name == self || o.Name == "Goal" => MethodPrefix + m,
                _ => null,
            };
            if (name == null || list.Any(x => x.Item1 == name))
                continue;
            var fn = functions.TryGetValue(name, out var g) ? g.Fn : section.Methods.TryGetValue(name, out var m2) ? m2.Fn : null;
            if (fn != null && fn != section.Activate && PickerCall(fn) is { } picker)
                list.Add((name, fn, picker));
        }
        return list;
    }

    /// <summary>
    /// As <see cref="Read(string, LuaParseResult)"/>; why is the localization key of the reason it isn't the template.
    /// Another act table (<paramref name="table"/>, one of <see cref="Tables"/>; null for the first) is read as the
    /// battle table is, from its function in place of Activate (the script's functions, Interrupt and observed
    /// SpEffects are the same).
    /// </summary>
    public static LuaBattleGoal Read(string src, LuaParseResult parse, out string why, string table = null)
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
        // Each section's other Goal.X = function … are its methods (SDT's acts: Goal.Act01, registered as self.Act01),
        // named "Goal.X" beside the script's global functions
        var sections = new List<Section>();
        var functions = new Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)>();
        var current = new Section();

        foreach (var s in parse.Tree.Stats)
        {
            if (s is LuaCallStat { Call: { Callee: LuaNameExpr { Name: "RegisterTableGoal" } } reg } && reg.Arguments.Count > 0)
            {
                if (current.Activate != null || current.Goal != null)
                    sections.Add(current);
                current = new Section { Goal = (reg.Arguments[0] as LuaNameExpr)?.Name };
            }
            else if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a &&
                a.Targets[0] is LuaFieldExpr { Object: LuaNameExpr { Name: "Goal" } } f && a.Values[0] is LuaFunctionExpr fn)
            {
                if (f.Name == "Activate")
                    current.Activate = fn;
                else if (f.Name is "Interrupt" or "Interupt")
                    current.Interrupt = fn;
                else
                    current.Methods.TryAdd(MethodPrefix + f.Name, (s, fn));
            }
            else if (s is LuaFunctionStat fs && !fs.IsLocal)
                functions.TryAdd(fs.Name, (fs, fs.Function));
        }
        if (current.Activate != null || current.Goal != null)
            sections.Add(current);

        // The older form (BB): a goal's functions are globals named after it, X_Activate(ai, goal) with X_Interupt
        // (X_Update, X_Terminate) beside it, and no goal table. Each X_Activate is a section, after the table ones.
        foreach (var (name, fn) in functions)
        {
            if (!name.EndsWith(GlobalActivateSuffix, StringComparison.Ordinal) || name.Length == GlobalActivateSuffix.Length)
                continue;
            var goal = name[..^GlobalActivateSuffix.Length];
            var interruptFn = functions.TryGetValue(goal + "_Interupt", out var i1) ? i1.Fn
                : functions.TryGetValue(goal + "_Interrupt", out var i2) ? i2.Fn : null;
            sections.Add(new Section { Goal = goal, Activate = fn.Fn, Interrupt = interruptFn });
        }

        // AC6: the logic script is the battle goal, in aicommon's EZOP framework (TacCtrlFuncArr, OpSetArr)
        if (ReadEzop(src, parse, functions, out var ezopWhy) is { } ezop)
        {
            why = null;
            return ezop;
        }
        if (ezopWhy != null)
        {
            why = ezopWhy;
            return null;
        }

        if (!sections.Any(x => x.Activate != null))
        {
            why = "LUA_Battle_Why_No_Activate";
            return null;
        }

        LuaFunctionExpr activate = null, interrupt = null;
        LuaCallExpr battle = null;
        string goalName = null;
        var tables = new List<string>();
        foreach (var section in sections.Where(x => x.Activate != null))
        {
            // The section's act tables (see ActTables): the first section that has one is the battle goal
            var found = ActTables(section, functions);
            if (found.Count == 0)
                continue;

            (goalName, interrupt) = (section.Goal, section.Interrupt);
            // The battle section's methods, beside the globals (a global of the same name is kept)
            foreach (var (name, m) in section.Methods)
                functions.TryAdd(name, m);

            tables.AddRange(found.Select(f => f.Name));
            var picked = table == null ? found[0] : found.FirstOrDefault(f => f.Name == table);
            if (picked.Fn == null)
            {
                why = "LUA_Battle_Why_No_Table";
                return null;
            }
            (table, activate, battle) = picked;
            break;
        }

        // No act table: Activate picks what to do itself (DS3's older goals): its odds chain, or Activate as one act
        if (battle == null)
            return ReadWithoutTable(src, parse, sections, functions, out why);

        if (battle.Arguments[2] is not LuaNameExpr weights || battle.Arguments[3] is not LuaNameExpr funcs)
        {
            why = "LUA_Battle_Why_No_Battle_Activate";
            return null;
        }

        var body = activate.Body;

        var result = new LuaBattleGoal
        {
            GoalName = goalName,
            Chunk = parse.Tree,
            Table = table,
            Activate = activate,
            WeightTable = weights.Name,
            FunctionTable = funcs.Name,
            ActivateLine = Line(activate),
            InterruptLine = interrupt != null ? Line(interrupt) : 0,
        };
        result.Tables.AddRange(tables);
        var afterName = (battle.Arguments[4] as LuaNameExpr)?.Name;

        LuaLiteral? Literal(LuaExpr e) => e.AsNumber() is { } v ? new LuaLiteral(e.First, e.Last, v, Line(e)) : null;
        int? KeyOf(LuaExpr target, string table) =>
            target is LuaIndexExpr { Object: LuaNameExpr o, Key: var k } && o.Name == table && k.AsNumber() is { } n ? (int)n : null;

        // REGIST_FUNC's function: a global (ER: Name_Act01), or the goal's method (SDT: self.Act01, Goal.Act01)
        var self = activate.Parameters.Count > 0 ? activate.Parameters[0] : null;
        string Registered(LuaExpr e) => e switch
        {
            LuaNameExpr n => n.Name,
            LuaFieldExpr { Object: LuaNameExpr o, Name: var m } when o.Name == self || o.Name == "Goal" => MethodPrefix + m,
            _ => null,
        };

        // Acts, cooldowns and the after function
        foreach (var s in body.AllStatements())
        {
            if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a)
            {
                var value = a.Values[0] as LuaCallExpr;
                if (KeyOf(a.Targets[0], funcs.Name) is { } actId && value?.Callee is LuaNameExpr { Name: "REGIST_FUNC" } &&
                    value.Arguments.Count >= 3 && Registered(value.Arguments[2]) is { } target)
                {
                    result.Act(actId).Function = target;
                    result.Act(actId).RegisterLine = Line(s);
                }
                else if (KeyOf(a.Targets[0], weights.Name) is { } coolId && value?.Callee is LuaNameExpr { Name: "SetCoolTime" } &&
                         value.Arguments.Count >= 4)
                {
                    result.Act(coolId).Cooldowns.Add((Literal(value.Arguments[2]), Literal(value.Arguments[3]), Line(s)));
                }
            }
            else if (s is LuaLocalStat { Names.Count: 1, Values.Count: 1 } l && l.Names[0] == afterName &&
                     l.Values[0] is LuaCallExpr { Callee: LuaNameExpr { Name: "REGIST_FUNC" } } after && after.Arguments.Count >= 3 &&
                     Registered(after.Arguments[2]) is { } afterTarget)
            {
                result.AfterFunction = afterTarget;
            }

            // The function table built in one go: local funcs = {[1] = REGIST_FUNC(ai, goal, GeneralNPC_Act01), …}
            // (ER / NR's common NPC goal, 029999_battle)
            if (s is LuaLocalStat { Names.Count: 1, Values.Count: 1 } built && built.Names[0] == funcs.Name &&
                built.Values[0] is LuaTableExpr constructor)
            {
                foreach (var field in constructor.Fields)
                {
                    if (field.Key?.AsNumber() is { } actId && field.Value is LuaCallExpr { Callee: LuaNameExpr { Name: "REGIST_FUNC" } } reg &&
                        reg.Arguments.Count >= 3 && Registered(reg.Arguments[2]) is { } target)
                    {
                        result.Act((int)actId).Function = target;
                        result.Act((int)actId).RegisterLine = Line(field.Key);
                    }
                }
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
            act.FunctionLine = Line(fs.Node);
            act.FunctionNode = fs.Fn;
            result.ReadSubGoals(act, fs.Fn, src, t, Literal);
        }
        if (result.AfterFunction != null && functions.TryGetValue(result.AfterFunction, out var af))
        {
            result.AfterFunctionLine = Line(af.Node);
            result.AfterFunctionNode = af.Fn;
        }

        result.Finish(parse, interrupt, functions, body);
        why = null;
        return result;
    }

    /// <summary>
    /// What every kind of goal reads besides its acts: the Interrupt, the script's functions it can call and what it
    /// reacts to, the SpEffects observed, and those Activate, the Interrupt and the act functions use
    /// </summary>
    private void Finish(LuaParseResult parse, LuaFunctionExpr interrupt, Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)> functions, LuaBlock body)
    {
        var t = parse.Tokens;
        int Line(LuaNode n) => t[n.First].Line;
        var result = this;

        // The Interrupt, the functions it can call, and what it reacts to
        result.Interrupt = interrupt;
        result.InterruptLine = interrupt != null ? Line(interrupt) : 0;
        foreach (var (name, fs) in functions)
            result.Functions[name] = fs.Fn;

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
            .Select(a => functions[a.Function].Fn.Body));
        foreach (var block in scanned)
        {
            foreach (var c in block.AllStatements().SelectMany(s => s.Calls()))
            {
                if (c.Method != null && SpEffectMethods.Contains(c.Method) && c.Arguments.Count >= 2 && c.Arguments[1].AsNumber() is { } id)
                    result.SpEffects.Add(new LuaSpEffectUse((int)id, c.Method, Line(c)));
            }
        }
    }

    // --- goals without an act table ---

    /// <summary>How the goal picks its acts</summary>
    public LuaGoalKind Kind = LuaGoalKind.Table;

    /// <summary>
    /// An odds goal's chain (<see cref="LuaGoalKind.Odds"/>): if roll &lt;= w1 then … elseif roll &lt;= w1 + w2 then …;
    /// each clause is an act, the term it adds (w2) its weight
    /// </summary>
    public LuaIfStat OddsChain;

    /// <summary>The locals an odds goal's weights are (w1, w2, … when they are names), each with its act</summary>
    public Dictionary<string, int> OddsTerms { get; } = new();

    /// <summary>The single act of a direct goal (<see cref="LuaGoalKind.Direct"/>): Activate itself</summary>
    public const int DirectAct = 1;

    /// <summary>
    /// The odds chain in a block: an if / elseif chain of two or more clauses whose conditions are roll &lt;= (or &lt;)
    /// a sum, the same roll each time, each sum the last one plus one term. Its terms, in order; null if there is none.
    /// </summary>
    private static (LuaIfStat Chain, List<LuaExpr> Terms)? FindOddsChain(LuaBlock block)
    {
        foreach (var s in block.AllStatements())
        {
            if (s is not LuaIfStat { Clauses.Count: >= 2 } i)
                continue;
            string roll = null;
            var terms = new List<LuaExpr>();
            var ok = true;
            foreach (var clause in i.Clauses)
            {
                if (clause.Condition is not LuaBinaryExpr { Operator: "<=" or "<", Left: LuaNameExpr r } cmp ||
                    roll != null && r.Name != roll)
                {
                    ok = false;
                    break;
                }
                roll = r.Name;
                var sum = Summands(cmp.Right);
                // Each sum is the last one plus one term (compared by source position of the same names)
                if (sum.Count != terms.Count + 1 || !sum.Take(terms.Count).Zip(terms).All(p => SameTerm(p.First, p.Second)))
                {
                    ok = false;
                    break;
                }
                terms.Add(sum[^1]);
            }
            if (ok)
                return (i, terms);
        }
        return null;

        static List<LuaExpr> Summands(LuaExpr e) => e is LuaBinaryExpr { Operator: "+" } b
            ? Summands(b.Left).Append(b.Right).ToList()
            : new List<LuaExpr> { e is LuaParenExpr p ? p.Inner : e };

        static bool SameTerm(LuaExpr a, LuaExpr b) => (a, b) switch
        {
            (LuaNameExpr x, LuaNameExpr y) => x.Name == y.Name,
            _ => a.AsNumber() is { } x2 && b.AsNumber() == x2,
        };
    }

    /// <summary>
    /// A battle goal whose Activate picks what to do itself (no Common_Battle_Activate): its odds chain as acts, else
    /// Activate as its one act. Only a battle section (its goal named …Battle…) is read so, not aicommon's goals.
    /// </summary>
    private static LuaBattleGoal ReadWithoutTable(string src, LuaParseResult parse, List<Section> sections,
        Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)> functions, out string why)
    {
        var t = parse.Tokens;
        int Line(LuaNode n) => t[n.First].Line;
        LuaLiteral? Literal(LuaExpr e) => e.AsNumber() is { } v ? new LuaLiteral(e.First, e.Last, v, Line(e)) : null;

        var section = sections.FirstOrDefault(x => x.Activate != null && x.Goal != null &&
                                                    x.Goal.Contains("Battle", StringComparison.OrdinalIgnoreCase) &&
                                                    x.Activate.Body.AllStatements().SelectMany(s => s.Calls()).Any(c =>
                                                        c.Method == "AddSubGoal" || c.Method == null && c.Callee is LuaNameExpr n && functions.ContainsKey(n.Name)));
        if (section == null)
        {
            why = "LUA_Battle_Why_No_Battle_Activate";
            return null;
        }
        foreach (var (name, m) in section.Methods)
            functions.TryAdd(name, m);

        var activate = section.Activate;
        var result = new LuaBattleGoal
        {
            GoalName = section.Goal,
            Chunk = parse.Tree,
            Table = MainTable,
            Activate = activate,
            ActivateLine = Line(activate),
        };
        result.Tables.Add(MainTable);

        if (FindOddsChain(activate.Body) is { } odds)
        {
            // Each clause an act (Branch01, …), its weight the term it adds; the else (if any) is the rest of the roll
            result.Kind = LuaGoalKind.Odds;
            result.OddsChain = odds.Chain;
            for (int k = 0; k < odds.Terms.Count; k++)
            {
                var clause = odds.Chain.Clauses[k];
                var act = result.Act(k + 1);
                act.Function = $"Branch{k + 1:D2}";
                act.RegisterLine = Line(clause.Condition);
                act.FunctionLine = Line(clause.Condition);
                act.FunctionNode = activate;
                act.OddsClause = k;
                act.OddsTerm = odds.Terms[k];
                if (odds.Terms[k] is LuaNameExpr term)
                    result.OddsTerms.TryAdd(term.Name, k + 1);
                result.ReadSubGoals(act, new LuaFunctionExpr { Body = clause.Body }, src, t, Literal);
            }

            // Weights: the literals the terms are set to, by the branch that sets them; a literal term is its own row
            result.ReadRows(activate.Body, new List<string>(), null, src, t,
                (target, _) => target is LuaNameExpr n && result.OddsTerms.TryGetValue(n.Name, out var id) ? id : null, Literal,
                name => result.OddsTerms.TryGetValue(name, out var id) ? id : null);
            var literalTerms = odds.Terms.Select((e, k) => (Lit: Literal(e), Act: k + 1)).Where(x => x.Lit != null).ToList();
            if (literalTerms.Count > 0)
            {
                var row = new LuaWeightRow { Line = Line(odds.Chain) };
                foreach (var (lit, id) in literalTerms)
                    row.Weights[id] = lit!.Value;
                result.Rows.Add(row);
            }
        }
        else
        {
            // Activate is the act: what it does in the situation is its plan
            result.Kind = LuaGoalKind.Direct;
            var act = result.Act(DirectAct);
            act.Function = "Activate";
            act.RegisterLine = Line(activate);
            act.FunctionLine = Line(activate);
            act.FunctionNode = activate;
            result.ReadSubGoals(act, activate, src, t, Literal);
        }

        result.Finish(parse, section.Interrupt, functions, activate.Body);
        why = null;
        return result;
    }

    // --- AC6: aicommon's EZOP framework ---

    /// <summary>
    /// An AC6 logic script as a battle goal (aicommon's EZOP framework): TacCtrlFuncArr[logicId] = function (ai, goal)
    /// is the act table — weights by branch (tbl[act] = n, or EZOP_CActLot(tbl, act, weight, otherwise, conditions…)),
    /// cooldowns (EZOP_SetCoolTime(ai, tbl, act, seconds, weight)), return EZOP_MakeActLotTable(tbl); OpSetArr[logicId]
    /// = { [act] = function (ai, goal) … arr:Add(EZOP_SetAct_X(ai, …)) … end } are the acts; the interrupt is the
    /// function EZOP_SetUpInterruptFunc registers. Each is found inline or through a main-chunk local. Null (why null)
    /// when the script isn't one; why set when it is but has no act table.
    /// </summary>
    private static LuaBattleGoal ReadEzop(string src, LuaParseResult parse,
        Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)> functions, out string why)
    {
        why = null;
        var t = parse.Tokens;
        int Line(LuaNode n) => t[n.First].Line;
        LuaLiteral? Literal(LuaExpr e) => e.AsNumber() is { } v ? new LuaLiteral(e.First, e.Last, v, Line(e)) : null;

        // Main-chunk locals by name (local f0_local10 = function …, local f0_local8 = { … })
        var chunkLocals = new Dictionary<string, LuaExpr>();
        foreach (var l in parse.Tree.Stats.OfType<LuaLocalStat>())
            for (int i = 0; i < l.Names.Count && i < l.Values.Count; i++)
                chunkLocals[l.Names[i]] = l.Values[i];
        LuaExpr Resolve(LuaExpr e) => e is LuaNameExpr n && chunkLocals.TryGetValue(n.Name, out var v) ? v
            : e is LuaNameExpr g && functions.TryGetValue(g.Name, out var fn) ? fn.Fn : e;

        LuaExpr Registered(string array) => parse.Tree.Stats.OfType<LuaAssignStat>()
            .Where(a => a.Targets.Count == 1 && a.Values.Count == 1 && a.Targets[0] is LuaIndexExpr { Object: LuaNameExpr { Name: var o } } && o == array)
            .Select(a => Resolve(a.Values[0])).FirstOrDefault();

        if (Registered("TacCtrlFuncArr") is not LuaFunctionExpr tactics)
            return null;

        var weights = tactics.Body.AllStatements().OfType<LuaReturnStat>()
            .Select(r => r.Values.Count == 1 && r.Values[0] is LuaCallExpr { Callee: LuaNameExpr { Name: "EZOP_MakeActLotTable" } } m &&
                         m.Arguments.Count == 1 && m.Arguments[0] is LuaNameExpr w ? w.Name : null)
            .FirstOrDefault(n => n != null);
        if (weights == null)
        {
            why = "LUA_Battle_Why_No_Ezop_Table";
            return null;
        }

        var result = new LuaBattleGoal
        {
            GoalName = "TacCtrlFuncArr",
            Chunk = parse.Tree,
            Table = MainTable,
            Activate = tactics,
            WeightTable = weights,
            ActivateLine = Line(tactics),
        };
        result.Tables.Add(MainTable);

        // Acts: the OpSetArr table's [act] = function entries, and their operations (arr:Add(EZOP_SetAct_X(ai, …)))
        if (Registered("OpSetArr") is LuaTableExpr acts)
        {
            foreach (var field in acts.Fields)
            {
                if (field.Key?.AsNumber() is not { } id || field.Value is not LuaFunctionExpr actFn)
                    continue;
                var act = result.Act((int)id);
                act.Function = $"Act{LuaBattleGoal.Format(id)}";
                act.RegisterLine = Line(field.Key);
                act.FunctionLine = Line(field.Key);
                act.FunctionNode = actFn;
                foreach (var c in actFn.Body.AllStatements().SelectMany(s => s.Calls()))
                {
                    if (c is not { Method: "Add", Arguments.Count: 1 } || c.Arguments[0] is not LuaCallExpr { Method: null, Callee: LuaNameExpr opName } op)
                        continue;
                    var sub = new LuaSubGoal { Goal = opName.Name, Line = Line(op) };
                    foreach (var arg in op.Arguments.Skip(1))
                        sub.Arguments.Add((arg.SourceOf(src, t), Literal(arg)));
                    act.SubGoals.Add(sub);
                }
            }
        }

        // Cooldowns
        foreach (var s in tactics.Body.AllStatements().OfType<LuaCallStat>())
        {
            if (s.Call is { Method: null, Callee: LuaNameExpr { Name: "EZOP_SetCoolTime" } } cool && cool.Arguments.Count >= 5 &&
                cool.Arguments[1] is LuaNameExpr { Name: var tbl } && tbl == weights && cool.Arguments[2].AsNumber() is { } coolAct)
                result.Act((int)coolAct).Cooldowns.Add((null, Literal(cool.Arguments[3]), Line(s)));
        }

        // Weights by branch: tbl[act] = n, and calls handed the table with a literal act and weight (EZOP_CActLot)
        int? KeyOf(LuaExpr target, string table) =>
            target is LuaIndexExpr { Object: LuaNameExpr o, Key: var k } && o.Name == table && k.AsNumber() is { } n ? (int)n : null;
        (int, LuaExpr)? CallKey(LuaCallExpr c) =>
            c.Arguments.Count > 2 && c.Arguments[0] is LuaNameExpr { Name: var a0 } && a0 == weights && c.Arguments[1].AsNumber() is { } ca
                ? ((int)ca, c.Arguments[2]) : null;
        result.ReadRows(tactics.Body, new List<string>(), weights, src, t, KeyOf, Literal, null, CallKey);
        foreach (var row in result.Rows)
            foreach (var id in row.Weights.Keys)
                result.Act(id);

        if (result.Acts.Count == 0)
        {
            why = "LUA_Battle_Why_No_Acts";
            return null;
        }

        // The interrupt: EZOP_SetUpInterruptFunc(ai, InterrunptCallBack_…) in the setup
        LuaFunctionExpr interrupt = null;
        foreach (var c in parse.Tree.AllStatements().SelectMany(s => s.Calls()))
        {
            if (c is { Method: null, Callee: LuaNameExpr { Name: "EZOP_SetUpInterruptFunc" } } && c.Arguments.Count >= 2 &&
                Resolve(c.Arguments[1]) is LuaFunctionExpr fn)
            {
                interrupt = fn;
                break;
            }
        }

        result.Finish(parse, interrupt, functions, tactics.Body);
        return result;
    }

    /// <summary>How a goal's method is named among <see cref="Functions"/> and act functions (Goal.Act01)</summary>
    public const string MethodPrefix = "Goal.";

    /// <summary>The older global form's Activate: Shousitai_Fist104000Battle_Activate(ai, goal)</summary>
    private const string GlobalActivateSuffix = "_Activate";

    /// <summary>The script's main chunk, whose top-level locals the goal's functions read (BB: local f0_local2 = 2.6 - f0_local0)</summary>
    public LuaBlock Chunk;

    /// <summary>One RegisterTableGoal section: its goal, Activate, Interrupt, and other Goal.X functions</summary>
    private sealed class Section
    {
        public string Goal;
        public LuaFunctionExpr Activate;
        public LuaFunctionExpr Interrupt;
        public Dictionary<string, (LuaNode Node, LuaFunctionExpr Fn)> Methods { get; } = new();
    }

    private LuaAct Act(int id)
    {
        if (!Acts.TryGetValue(id, out var act))
            Acts[id] = act = new LuaAct { Id = id };
        return act;
    }

    /// <param name="localKey">An odds goal's weight locals: local w = 20 sets act w's weight too (null: not read)</param>
    /// <param name="callKey">A call statement setting an act's weight (EZOP_CActLot(tbl, act, weight, …)): the act and the weight argument</param>
    private void ReadRows(LuaBlock block, List<string> path, string weights, string src, List<LuaToken> t,
        Func<LuaExpr, string, int?> keyOf, Func<LuaExpr, LuaLiteral?> literal, Func<string, int?> localKey = null,
        Func<LuaCallExpr, (int Act, LuaExpr Weight)?> callKey = null)
    {
        LuaWeightRow run = null;

        foreach (var s in block.Stats)
        {
            int? key = s switch
            {
                LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a => keyOf(a.Targets[0], weights),
                LuaLocalStat { Names.Count: 1, Values.Count: 1 } l when localKey != null => localKey(l.Names[0]),
                LuaCallStat cs when callKey?.Invoke(cs.Call) is { } ck => ck.Act,
                _ => null,
            };
            var assigned = s switch
            {
                LuaAssignStat a2 => a2.Values[0],
                LuaLocalStat l2 when l2.Values.Count == 1 => l2.Values[0],
                LuaCallStat cs2 when callKey?.Invoke(cs2.Call) is { } ck2 => ck2.Weight,
                _ => null,
            };
            if (key is { } act && assigned != null && literal(assigned) is { } value)
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
                        ReadRows(i.Clauses[c].Body, Append(path, label), weights, src, t, keyOf, literal, localKey, callKey);
                    }
                    if (i.Else != null)
                        ReadRows(i.Else, Append(path, "else"), weights, src, t, keyOf, literal, localKey, callKey);
                    break;
                case LuaLoopStat l:
                    run = null;
                    ReadRows(l.Body, Append(path, l.Keyword + " …"), weights, src, t, keyOf, literal, localKey, callKey);
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
