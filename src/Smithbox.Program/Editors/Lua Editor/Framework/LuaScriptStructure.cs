using SoulsFormats;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// A goal registration at a script's top level: RegisterTableGoal(GOAL_X, "X"), REGISTER_GOAL(GOAL_X, "XBattle"), or a
/// logic's REGISTER_LOGIC_FUNC(LOGIC_ID_X, "X_Logic", "X_Interupt") (DES; one registration per name string)
/// </summary>
/// <param name="Constant">The goal-list (logic-list) constant it names</param>
/// <param name="NameToken">The token of its name string, or -1</param>
public readonly record struct LuaRegistration(string Constant, int NameToken);

/// <summary>A top-level NAME = number of a list script (goal_list: GOAL_… = goal ID; logic_list: LOGIC_ID_… = logic ID)</summary>
public readonly record struct LuaListConstant(string Name, double Value, int FirstLine, int LastLine);

/// <summary>
/// Text work for creating and duplicating battle and logic scripts: how a script registers its goals, renaming a copy
/// for its new goal ID, the lines of the list scripts, and the minimal scripts a new goal starts from. See
/// Claude/Documentation/Lua and AI.md (Text editing).
/// </summary>
public static class LuaScriptStructure
{
    /// <summary>aicommon's lists of goal IDs (goal_list.lua, ER's goal_list_dlc1.lua) and logic IDs (logic_list.lua)</summary>
    public const string GoalListPrefix = "goal_list";
    public const string LogicListPrefix = "logic_list";

    /// <summary>The goal registrations at the script's top level</summary>
    public static List<LuaRegistration> Registrations(LuaParseResult parse)
    {
        var list = new List<LuaRegistration>();
        if (parse?.Tree == null)
            return list;
        foreach (var s in parse.Tree.Stats)
        {
            if (s is LuaCallStat { Call: { Callee: LuaNameExpr { Name: "RegisterTableGoal" or "REGISTER_GOAL" } } reg } &&
                reg.Arguments.Count > 0 && reg.Arguments[0] is LuaNameExpr goal)
            {
                var nameToken = reg.Arguments.Count > 1 && reg.Arguments[1] is LuaStringExpr str ? str.First : -1;
                list.Add(new LuaRegistration(goal.Name, nameToken));
            }
            // A logic and its interrupt, by name (DES): both strings are the script's function names
            else if (s is LuaCallStat { Call: { Callee: LuaNameExpr { Name: "REGISTER_LOGIC_FUNC" } } logic } &&
                logic.Arguments.Count > 0 && logic.Arguments[0] is LuaNameExpr id)
            {
                var names = logic.Arguments.Skip(1).OfType<LuaStringExpr>().ToList();
                if (names.Count == 0)
                    list.Add(new LuaRegistration(id.Name, -1));
                foreach (var name in names)
                    list.Add(new LuaRegistration(id.Name, name.First));
            }
        }
        return list;
    }

    /// <summary>The top-level NAME = number assignments of a list script, with their lines</summary>
    public static List<LuaListConstant> ListConstants(LuaParseResult parse)
    {
        var list = new List<LuaListConstant>();
        if (parse?.Tree == null)
            return list;
        var t = parse.Tokens;
        foreach (var s in parse.Tree.Stats)
        {
            if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a && a.Targets[0] is LuaNameExpr n && a.Values[0].AsNumber() is { } v)
                list.Add(new LuaListConstant(n.Name, v, t[s.First].Line, t[s.Last].Line));
        }
        return list;
    }

    /// <summary>The global functions the script defines (function X, X = function), by name</summary>
    public static HashSet<string> DefinedFunctions(LuaParseResult parse)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (parse == null)
            return set;
        foreach (var d in parse.Definitions)
        {
            if (d.IsFunction && d.Name.IndexOfAny(new[] { '.', ':' }) < 0)
                set.Add(d.Name);
        }
        return set;
    }

    /// <summary>
    /// Whether a goal-table (.luainfo) entry belongs to a script: the script defines the goal's functions (a battle
    /// goal's Name_Activate, a logic's Name, or its logic interrupt)
    /// </summary>
    public static bool OwnsGoal(LUAINFO.Goal goal, HashSet<string> functions) =>
        goal.Name != null && (functions.Contains(goal.Name + "_Activate") || functions.Contains(goal.Name))
        || goal.LogicInterruptName != null && functions.Contains(goal.LogicInterruptName);

    /// <summary>The goal ID as it appears in a name: not inside a longer number, with or without leading zeros (006130, 6130)</summary>
    private static Regex IdPattern(int id) => new($@"(?<!\d)0*{id}(?!\d)", RegexOptions.CultureInvariant);

    /// <summary>A name with one goal ID changed to another, keeping its zero padding (430000_battle → 430500_battle, Pot6130Battle → Pot6200Battle)</summary>
    public static string RenameId(string s, int oldId, int newId)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        var n = newId.ToString();
        return IdPattern(oldId).Replace(s, m => n.Length < m.Length ? n.PadLeft(m.Length, '0') : n);
    }

    public static bool ContainsId(string s, int id) => !string.IsNullOrEmpty(s) && IdPattern(id).IsMatch(s);

    /// <summary>
    /// A name with the first of several ID changes that applies: the script's goal ID first, then its other goals'
    /// (ER's helper goal of 300000_battle is GOAL_…300001_AfterAttackAct = 300001)
    /// </summary>
    public static string RenameIds(string s, IReadOnlyList<(int From, int To)> ids)
    {
        foreach (var (from, to) in ids)
        {
            var r = RenameId(s, from, to);
            if (r != s)
                return r;
        }
        return s;
    }

    /// <summary>The copy's name for a goal constant: its ID renamed, or one named after no ID (DS3's GOAL_NPC_YUPASAMA) given the new value after it</summary>
    public static string CopyConstantName(string name, IReadOnlyList<(int From, int To)> ids, double newValue)
    {
        var r = RenameIds(name, ids);
        return r != name ? r : $"{name}_{(long)newValue}";
    }

    /// <summary>
    /// The copy's name string for a registration: its IDs renamed; else, when it is its constant's name (with or
    /// without GOAL_), that constant's new name
    /// </summary>
    public static string CopyRegistrationName(string s, IReadOnlyList<(int From, int To)> ids, IReadOnlyDictionary<string, string> constants)
    {
        var r = RenameIds(s, ids);
        if (r != s)
            return r;
        foreach (var (from, to) in constants)
        {
            if (s == from)
                return to;
            if ("GOAL_" + s == from && to.StartsWith("GOAL_"))
                return to[5..];
        }
        return s;
    }

    /// <summary>
    /// A copy of a script's text for a new goal ID: every name in <paramref name="renames"/> renamed where it is used
    /// as a name (not a field: goal.X, ai:X), and each goal registration's name string renamed by
    /// <paramref name="renameString"/>. Comments and everything else are kept as they are.
    /// </summary>
    public static string RenameScript(string src, LuaParseResult parse, Func<string, string> renameString, IReadOnlyDictionary<string, string> renames)
    {
        var tokens = parse.Tokens;
        var edits = new List<(int Start, int Length, string Text)>();

        for (var i = 0; i < tokens.Count; i++)
        {
            var tk = tokens[i];
            if (tk.Kind != LuaTokenKind.Name || !renames.TryGetValue(tk.Text, out var to))
                continue;
            if (i > 0 && tokens[i - 1].Kind == LuaTokenKind.Symbol && tokens[i - 1].Text is "." or ":")
                continue;
            edits.Add((tk.Start, tk.Length, to));
        }

        foreach (var reg in Registrations(parse))
        {
            if (reg.NameToken < 0)
                continue;
            var tk = tokens[reg.NameToken];
            if (tk.Text.Length < 2 || tk.Text[0] is not ('"' or '\''))
                continue;
            var renamed = tk.Text[0] + renameString(tk.Text[1..^1]) + tk.Text[^1];
            if (renamed != tk.Text)
                edits.Add((tk.Start, tk.Length, renamed));
        }

        var sb = new StringBuilder(src);
        foreach (var e in edits.OrderByDescending(e => e.Start))
            sb.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
        return sb.ToString();
    }

    /// <summary>The text with lines added, each after a line (1-based; past the end appends), in the order given per place</summary>
    public static string InsertLines(string src, IEnumerable<(int AfterLine, string Line)> lines)
    {
        var nl = src.Contains("\r\n") ? "\r\n" : "\n";
        var list = src.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        // A text ending with a newline ends with an empty line: append before it
        var end = list.Count > 0 && list[^1].Length == 0 ? list.Count - 1 : list.Count;

        foreach (var group in lines.GroupBy(l => Math.Clamp(l.AfterLine, 0, end)).OrderByDescending(g => g.Key))
            list.InsertRange(group.Key, group.Select(l => l.Line));
        return string.Join(nl, list);
    }

    /// <summary>The text without some lines (1-based, inclusive ranges)</summary>
    public static string RemoveLines(string src, IEnumerable<(int First, int Last)> ranges)
    {
        var nl = src.Contains("\r\n") ? "\r\n" : "\n";
        var list = src.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        foreach (var (first, last) in ranges.OrderByDescending(r => r.First))
        {
            var from = Math.Clamp(first - 1, 0, list.Count);
            var count = Math.Clamp(last - first + 1, 0, list.Count - from);
            list.RemoveRange(from, count);
        }
        return string.Join(nl, list);
    }

    public static bool IsIdentifier(string s) =>
        !string.IsNullOrEmpty(s) && Regex.IsMatch(s, "^[A-Za-z_][A-Za-z0-9_]*$") && !LuaLexer.Keywords.Contains(s);

    /// <summary>
    /// A new battle goal that only waits. The table form (where aicommon defines RegisterTableGoal: ER, NR, SDT, DS3,
    /// DB_NT) registers GOAL_{base}_Battle; the global form (BB, DS1R) is the {base}Battle_Activate family, with
    /// REGISTER_GOAL when the game's scripts call it (DS1R).
    /// </summary>
    public static string BattleTemplate(string baseName, bool table, bool registerGoal)
    {
        var goal = $"GOAL_{baseName}_Battle";
        if (table)
        {
            return $$"""
                RegisterTableGoal({{goal}}, "{{baseName}}_Battle")
                REGISTER_GOAL_NO_UPDATE({{goal}}, true)

                Goal.Initialize = function (self, ai, goal, arg)
                end

                Goal.Activate = function (self, ai, goal)
                    goal:AddSubGoal(GOAL_COMMON_Wait, 5, TARGET_NONE, 0, 0, 0)
                end

                Goal.Update = function (self, ai, goal)
                    return Update_Default_NoSubGoal(self, ai, goal)
                end

                Goal.Terminate = function (self, ai, goal)
                end

                Goal.Interrupt = function (self, ai, goal)
                    return false
                end

                """.Replace("\r\n", "\n");
        }

        var fn = $"{baseName}Battle";
        var register = registerGoal ? $"REGISTER_GOAL({goal}, \"{fn}\")\n\n" : "";
        return register + $$"""
            function {{fn}}_Activate(ai, goal)
                goal:AddSubGoal(GOAL_COMMON_Wait, 5, TARGET_NONE, 0, 0, 0)
            end

            function {{fn}}_Update(ai, goal)
                return GOAL_RESULT_Continue
            end

            function {{fn}}_Terminate(ai, goal)
            end

            function {{fn}}_Interupt(ai, goal)
                return false
            end

            """.Replace("\r\n", "\n");
    }

    /// <summary>
    /// A new logic that only waits: {base}_Logic with its interrupt {base}_Interupt (the names the .luainfo gives), with
    /// REGISTER_LOGIC_FUNC naming them when the game's logic scripts call it (DES, which has no .luainfo)
    /// </summary>
    public static string LogicTemplate(string baseName, bool registerLogic = false)
    {
        var register = registerLogic ? $"REGISTER_LOGIC_FUNC(LOGIC_ID_{baseName}, \"{baseName}_Logic\", \"{baseName}_Interupt\")\n\n" : "";
        return register + $$"""
            function {{baseName}}_Logic(ai)
                ai:AddTopGoal(GOAL_COMMON_Wait, 5, TARGET_NONE, 0, 0, 0)
            end

            function {{baseName}}_Interupt(ai, goal)
                return false
            end

            """.Replace("\r\n", "\n");
    }
}
