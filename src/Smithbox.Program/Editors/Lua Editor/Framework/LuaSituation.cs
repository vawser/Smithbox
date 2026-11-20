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

    /// <summary>
    /// A question about where the characters stand (distance, facing, angles: see <see cref="LuaSituation.SceneValue"/>):
    /// the call, which the simulation answers from the scene
    /// </summary>
    public LuaCallExpr SceneQuery;

    /// <summary>A SetCoolTime input: the animation and seconds, which the simulation checks against what it played</summary>
    public long? CooldownAnimation;
    public double? CooldownSeconds;

    /// <summary>Set by the simulation, not the user, while it runs</summary>
    public bool IsLive => SceneQuery != null || CooldownAnimation != null;

    /// <summary>A question the character's NpcParam / NpcThinkParam rows answer (see <see cref="LuaSituation.NpcValue"/>)</summary>
    public LuaCallExpr NpcQuery;

    /// <summary>Answered by the NPC rows at the last evaluation (the value shown is theirs, not the user's)</summary>
    public bool FromNpc;
}

/// <summary>
/// What the character's param rows say about it, for the calls a battle goal asks of the game about itself
/// </summary>
public interface ILuaNpc
{
    /// <summary>The map capsule radius (NpcParam hitRadius), or null without one</summary>
    double? MapHitRadius { get; }

    /// <summary>Whether a SpEffect is on the character: resident (NpcParam spEffectID0..), or put on by its TAE</summary>
    bool HasSpEffect(int id);

    /// <summary>A field of the NpcThinkParam row, or null without the row or the field</summary>
    double? ThinkParam(string field);

    /// <summary>Whether a SpEffect is on the target now (the player in First Person Mode put it on)</summary>
    bool TargetHasSpEffect(int id);

    /// <summary>Whether the target guards now (First Person Mode), or null when that is the user's to set</summary>
    bool? TargetGuarding { get; }

    /// <summary>
    /// The character's role in its team now (ROLE_TYPE_: -1 none, 0 attacker, 1 surrounder, 2 spectator), which
    /// ai:GetTeamOrder(ORDER_TYPE_Role) gives; null when that is the user's to set
    /// </summary>
    double? TeamRole { get; }
}

/// <summary>
/// Where the AI's character (TARGET_SELF) and its target (TARGET_ENE_0) stand, for the facing and distance checks.
/// Angles are on the ground, in degrees, counter-clockwise (to the left) from the front, in -180..180.
/// </summary>
public interface ILuaScene
{
    /// <summary>Distance on the ground between the two</summary>
    double Distance { get; }

    /// <summary>Where the target is, seen from the character's front</summary>
    double TargetAngleFromSelf { get; }

    /// <summary>Where the character is, seen from the target's front</summary>
    double SelfAngleFromTarget { get; }
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

/// <summary>What one run of Goal.Interrupt gives, for an interrupt</summary>
public class LuaInterruptResult
{
    /// <summary>The INTERUPT_ type (without the prefix), and the SpEffect that set it off</summary>
    public string Type;
    public int? SpEffect;

    /// <summary>It returned true: the interrupt was taken, and the current act stops</summary>
    public bool Handled;

    /// <summary>It cleared the current sub-goals (goal:ClearSubGoal())</summary>
    public bool ClearsSubGoals;

    /// <summary>It asked for the goal to be planned again (ai:Replaning())</summary>
    public bool Replans;

    /// <summary>What it queued instead: its sub-goals, and those of the functions it called</summary>
    public List<LuaActStep> Steps { get; } = new();

    /// <summary>The calls that shaped the reaction (ClearSubGoal, Replaning, functions called), by line</summary>
    public List<(int Line, string Call)> Calls { get; } = new();

    /// <summary>Conditions that couldn't be decided (counted as false)</summary>
    public List<(int Line, string Message)> Notes { get; } = new();
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
            // A choice starts on the first constant compared; a team role on none of them (no role named: the attacker,
            // as a lone enemy is), not on the first role the script happens to test (the spectator)
            if (input.Kind == LuaInputKind.Choice)
                input.Choice = input.Key.Contains(":GetTeamOrder(", StringComparison.Ordinal) ? OtherChoice : input.Choices[0];
        }
    }

    // --- static pass: the inputs ---

    /// <summary>A name aicommon defines, or one that looks like a constant (see <see cref="LooksLikeConstant"/>)</summary>
    private bool IsConstantName(string name) => Constants.ContainsKey(name) || LooksLikeConstant(name);

    /// <summary>
    /// Whether a name is written as the games write their constants: all capitals (TARGET_ENE_0, AI_DIR_TYPE_F), or an
    /// all-capitals prefix before an underscore with a mixed-case name after it (ROLE_TYPE_Torimaki, ORDER_TYPE_Role,
    /// INTERUPT_FindAttack, GOAL_COMMON_ApproachTarget). Locals (f2_local0) and globals in mixed case (BackSlash_Int)
    /// aren't. A constant mistaken for a global would be read as an input of its own, so two of them compared would be
    /// equal (ROLE_TYPE_Kankyaku == GetTeamOrder(…), both false).
    /// </summary>
    public static bool LooksLikeConstant(string name)
    {
        if (name.Length < 2 || !char.IsLetter(name[0]))
            return false;
        if (name.All(c => !char.IsLower(c)))
            return name.Any(char.IsLetter);
        var underscore = name.IndexOf('_');
        return underscore >= 2 && name[..underscore].All(c => char.IsUpper(c) || char.IsDigit(c));
    }

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

        var input = MarkLive(Input(Text(call), name, Text(call), call), call);
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

    /// <summary>
    /// Marks an input the simulation answers from the scene (distance, facing, angles), or one the NPC's rows may answer
    /// </summary>
    private static LuaSituationInput MarkLive(LuaSituationInput input, LuaCallExpr call)
    {
        if (IsSceneQuery(call))
            input.SceneQuery = call;
        if (IsNpcQuery(call))
            input.NpcQuery = call;
        return input;
    }

    // --- the NPC ---

    private const string ThinkParamPrefix = "AI_EXCEL_THINK_PARAM_TYPE__";

    /// <summary>The character's param rows (null: the inputs are the user's)</summary>
    public Func<ILuaNpc> Npc;

    /// <summary>
    /// Whether a call asks about the character itself, which its param rows can answer: ai:GetMapHitRadius(TARGET_SELF),
    /// ai:HasSpecialEffectId(TARGET_SELF, id) and ai:GetExcelParam(AI_EXCEL_THINK_PARAM_TYPE__field); or about what the
    /// target does, which First Person Mode answers: ai:HasSpecialEffectId(TARGET_ENE_0, id), ai:IsTargetGuard(TARGET_ENE_0);
    /// or its team role, which the simulation's Team role setting answers: ai:GetTeamOrder(ORDER_TYPE_Role)
    /// </summary>
    public static bool IsNpcQuery(LuaCallExpr c)
    {
        var a = c.Arguments;
        return c.Method switch
        {
            "GetMapHitRadius" => a.Count == 1 && Who(a[0]) == "TARGET_SELF",
            "HasSpecialEffectId" => a.Count == 2 && Who(a[0]) is not null,
            "IsTargetGuard" => a.Count == 1 && Who(a[0]) == "TARGET_ENE_0",
            "GetTeamOrder" => a.Count == 1 && a[0] is LuaNameExpr { Name: "ORDER_TYPE_Role" },
            "GetExcelParam" => a.Count == 1 && a[0] is LuaNameExpr { Name: var n } && n.StartsWith(ThinkParamPrefix, StringComparison.Ordinal),
            _ => false,
        };
    }

    /// <summary>
    /// The NPC's answer to an NPC query, or null when it has none: the map hit radius; true for a resident SpEffect (any
    /// other is the user's to set, as events and the TAE give them); a NpcThinkParam field's value
    /// </summary>
    public object NpcValue(LuaCallExpr c)
    {
        if (Npc?.Invoke() is not { } npc || !IsNpcQuery(c))
            return null;

        switch (c.Method)
        {
            case "GetMapHitRadius":
                return npc.MapHitRadius;
            case "HasSpecialEffectId":
                var id = c.Arguments[1].AsNumber() ??
                         (c.Arguments[1] is LuaNameExpr n && Constants.TryGetValue(n.Name, out var cv) ? cv : null);
                if (id is not { } v)
                    return null;
                // The target's: on while the player has put it on; any other stays the user's
                if (Who(c.Arguments[0]) == "TARGET_ENE_0")
                    return npc.TargetHasSpEffect((int)v) ? true : null;
                return npc.HasSpEffect((int)v) ? true : null;
            case "IsTargetGuard":
                return npc.TargetGuarding;
            case "GetTeamOrder":
                return npc.TeamRole;
            case "GetExcelParam":
                return npc.ThinkParam(((LuaNameExpr)c.Arguments[0]).Name[ThinkParamPrefix.Length..]);
        }
        return null;
    }

    /// <summary>Shows the NPC's answers in their inputs (which then can't be set), and frees the rest</summary>
    private void ApplyNpc()
    {
        foreach (var input in Inputs)
        {
            input.FromNpc = false;
            if (input.NpcQuery == null || NpcValue(input.NpcQuery) is not { } value)
                continue;

            input.FromNpc = true;
            switch (value)
            {
                case bool b:
                    input.Boolean = b;
                    input.Number = b ? 1 : 0;
                    break;
                default:
                    var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    input.Number = number;
                    input.Boolean = number != 0;
                    // A choice: the constant of that value, else "other"
                    if (input.Kind == LuaInputKind.Choice)
                        input.Choice = input.Choices.FirstOrDefault(ch => Constants.TryGetValue(ch, out var chv) && chv == number) ?? OtherChoice;
                    break;
            }
        }
    }

    // --- the scene ---

    private static string Who(LuaExpr e) => e is LuaNameExpr { Name: "TARGET_SELF" or "TARGET_ENE_0" } n ? n.Name : null;

    /// <summary>
    /// Whether a call asks where the character and its target stand, between the two of them:
    /// ai:GetDist(TARGET_ENE_0); ai:IsInsideTarget(TARGET_ENE_0, dir, angle);
    /// ai:IsInsideTargetCustom(from, to, dir, angle, verticalAngle, range) and ai:IsInsideTargetEx(from, to, dir,
    /// angle, range) with from / to TARGET_SELF and TARGET_ENE_0; ai:GetToTargetAngle(TARGET_ENE_0) and
    /// ai:GetRelativeAngleFromTarget(TARGET_ENE_0).
    /// </summary>
    public static bool IsSceneQuery(LuaCallExpr c)
    {
        var a = c.Arguments;
        return c.Method switch
        {
            "GetDist" or "GetDistY" or "GetDistYSigned" or "GetToTargetAngle" or "GetRelativeAngleFromTarget" => a.Count == 1 && Who(a[0]) == "TARGET_ENE_0",
            "IsInsideTarget" => a.Count >= 3 && Who(a[0]) == "TARGET_ENE_0" && a[1] is LuaNameExpr,
            "IsInsideTargetCustom" => a.Count >= 6 && Who(a[0]) is { } f && Who(a[1]) is { } t && f != t && a[2] is LuaNameExpr,
            "IsInsideTargetEx" => a.Count >= 5 && Who(a[0]) is { } f2 && Who(a[1]) is { } t2 && f2 != t2 && a[2] is LuaNameExpr,
            _ => false,
        };
    }

    /// <summary>The scene the simulation runs in (null outside it: the inputs are the user's)</summary>
    public ILuaScene Scene;

    /// <summary>Set while the simulation runs: Evaluate answers scene queries from the scene, not the inputs</summary>
    public bool Live;

    // AI_DIR_TYPE_*: the sector's middle, counter-clockwise from the front (the To* types, from the target's side, aren't answered)
    private static readonly Dictionary<string, double> Directions = new()
    {
        ["AI_DIR_TYPE_CENTER"] = 0, ["AI_DIR_TYPE_F"] = 0, ["AI_DIR_TYPE_B"] = 180,
        ["AI_DIR_TYPE_L"] = 90, ["AI_DIR_TYPE_R"] = -90,
        ["AI_DIR_TYPE_FL"] = 45, ["AI_DIR_TYPE_FR"] = -45, ["AI_DIR_TYPE_BL"] = 135, ["AI_DIR_TYPE_BR"] = -135,
    };

    private static double Wrap(double angle)
    {
        angle %= 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }

    /// <summary>
    /// The scene's answer to a scene query (<see cref="IsSceneQuery"/>), or null without a scene or for a query it
    /// can't answer. A facing check is true when the other one is within the sector of the given width (degrees)
    /// around the direction, from the looker's front, and within the range when there is one (the vertical angle
    /// is ignored: the characters are on flat ground). GetToTargetAngle and GetRelativeAngleFromTarget are 0..360,
    /// clockwise from the front, as the scripts compare them (>= 90 and <= 270 is behind).
    /// </summary>
    public object SceneValue(LuaCallExpr c)
    {
        if (Scene == null || !IsSceneQuery(c))
            return null;
        var a = c.Arguments;
        double? Num(int i) => i < a.Count ? a[i].AsNumber() : null;

        bool? Inside(bool lookerIsSelf, LuaExpr dir, double? width, double? range)
        {
            if (dir is not LuaNameExpr d || !Directions.TryGetValue(d.Name, out var centre) || width is not { } w)
                return null;
            var bearing = lookerIsSelf ? Scene.TargetAngleFromSelf : Scene.SelfAngleFromTarget;
            return Math.Abs(Wrap(bearing - centre)) <= w / 2 && (range is not { } r || Scene.Distance <= r);
        }

        switch (c.Method)
        {
            // The characters stand on the same flat ground
            case "GetDistY" or "GetDistYSigned":
                return 0.0;
            case "GetDist":
                return Scene.Distance;
            case "GetToTargetAngle":
                return (360 - Wrap(Scene.TargetAngleFromSelf)) % 360;
            case "GetRelativeAngleFromTarget":
                return (360 - Wrap(Scene.SelfAngleFromTarget)) % 360;
            case "IsInsideTarget":
                return Inside(true, a[1], Num(2), null);
            case "IsInsideTargetCustom":
                return Inside(Who(a[0]) == "TARGET_SELF", a[2], Num(3), Num(5));
            case "IsInsideTargetEx":
                return Inside(Who(a[0]) == "TARGET_SELF", a[2], Num(3), Num(4));
        }
        return null;
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
                Mark(MarkLive(Input(Text(c), ShortCall(c), Text(c), c), c), hint, comparedWith);
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

        // Scene queries are answered from the scene (the simulation, or planning an act)
        public bool Live;

        // Running Goal.Interrupt: the interrupt that happened (its INTERUPT_ type without the prefix), the SpEffect
        // that set it off, and what the run returned
        public string InterruptType;
        public int? InterruptSpEffect;
        public V? ReturnValue;
    }

    // --- interrupts ---

    /// <summary>
    /// Runs Goal.Interrupt for an interrupt, as the game calls it, with the situation's values (and the scene and the
    /// NPC answering as they do for Activate): IsInterupt(INTERUPT_x) is true for that type only, and
    /// GetSpecialEffectActivateInterruptId(id) for that SpEffect only. Gives whether it handled the interrupt (returned
    /// true), whether it cleared the current sub-goals, and the steps it queued: its sub-goals, and the sub-goals of the
    /// script's functions it calls (an act function), as act planning reads them.
    /// </summary>
    /// <param name="type">The INTERUPT_ type without the prefix (ActivateSpecialEffect, Damaged, …)</param>
    public LuaInterruptResult RunInterrupt(string type, int? spEffect, double targetDistance)
    {
        var result = new LuaInterruptResult { Type = type, SpEffect = spEffect };
        if (Goal.Interrupt == null)
            return result;

        var run = new Run
        {
            TargetDistance = targetDistance,
            Live = Scene != null,
            InterruptType = type,
            InterruptSpEffect = spEffect,
        };
        run.OnCall = (call, r) =>
        {
            var line = Tokens[call.First].Line;
            switch (call.Method)
            {
                case "ClearSubGoal":
                    result.ClearsSubGoals = true;
                    result.Steps.Clear();
                    result.Calls.Add((line, "ClearSubGoal"));
                    return;
                case "Replaning":
                    result.Replans = true;
                    result.Calls.Add((line, "Replaning"));
                    return;
            }

            // One of the script's functions (an act, or a helper): its sub-goals, with the situation's values
            if (call.Method == null && call.Callee is LuaNameExpr { Name: var name } && Goal.Functions.TryGetValue(name, out var fn))
            {
                result.Calls.Add((line, name));
                result.Steps.AddRange(PlanFunction(fn, targetDistance));
                return;
            }

            if (StepOf(call, r, targetDistance) is { } step)
                result.Steps.Add(step);
        };

        ApplyNpc();
        Exec(Goal.Interrupt.Body, run);
        result.Handled = run.ReturnValue is { } v && v.Truthy;
        result.Notes.AddRange(run.Result.Notes);
        return result;
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

        var run = new Run { TargetDistance = targetDistance, Live = Scene != null };
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
        ApplyNpc();
        var run = new Run { Live = Live && Scene != null };
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

                case LuaReturnStat r:
                    run.ReturnValue = r.Values.Count > 0 ? Eval(r.Values[0], run) : Nil;
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

            // Live (the simulation, or planning an act): where the characters stand answers the distance and facing
            case LuaCallExpr c when run.Live && SceneValue(c) is { } scene:
                return scene is bool sb ? new V(VKind.Bool, B: sb) : new V(VKind.Number, Convert.ToDouble(scene));

            // Running the Interrupt: which interrupt happened, and the SpEffect that set it off
            case LuaCallExpr { Method: "IsInterupt", Arguments: [LuaNameExpr { Name: var interrupt }] } when run.InterruptType != null:
                return new V(VKind.Bool, B: interrupt == LuaBattleGoal.InterruptPrefix + run.InterruptType);
            case LuaCallExpr { Method: "GetSpecialEffectActivateInterruptId", Arguments: [var tested] } when run.InterruptType != null:
                return new V(VKind.Bool, B: run.InterruptSpEffect is { } fired && tested.AsNumber() == fired);

            // What the character's rows say about it (its radius, resident SpEffects, think param): in Activate and in
            // the act functions alike
            case LuaCallExpr c when NpcValue(c) is { } npcValue:
                return npcValue is bool nb ? new V(VKind.Bool, B: nb) : new V(VKind.Number, Convert.ToDouble(npcValue, CultureInfo.InvariantCulture));

            // Planning without a scene: the distance given
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
