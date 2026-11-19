using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Readable names for the decompiler's locals and arguments (fN_localK, fN_argK), for scripts shipped as bytecode
/// without debug info. Only names a rule can work out are changed; the rest stay as they are.
///
/// Rules:
/// - local x = obj:GetDist(TARGET_ENE_0) → targetDist (a Get/Is/Has method; TARGET_ENE_0 / TARGET_SELF prefix the
///   name with target / self); local x = Func(…) → func; GetRandam_Int → randInt.
/// - The tables passed to Common_Battle_Activate(ai, goal, weights, funcs, after, params) → actWeights, actFuncs,
///   actAfter, actParams.
/// - An argument used with goal methods (AddSubGoal, GetParam, …) → goal; one used only with other methods → ai;
///   the unused first argument of a Goal.X = function → self.
///
/// Safety: a local can be seen only inside the top-level statement that declares it (or, declared in the main
/// chunk, everywhere after it). New names are unique within each top-level statement, file-wide for main-chunk
/// locals, and never equal to any other name in the file, so no reference can change meaning. LuaAiSurvey checks this
/// by compiling the renamed vanilla scripts back to identical bytecode.
/// </summary>
public static class LuaNames
{
    private static readonly Regex DecompilerName = new(@"^f\d+_(local|arg)\d+$", RegexOptions.Compiled);

    private static readonly HashSet<string> GoalMethods = new()
    {
        "AddSubGoal", "ClearSubGoal", "GetParam", "GetLife", "SetLifeEndSuccess", "SetFailedEndOption",
    };

    private static readonly Dictionary<string, string> MethodNames = new()
    {
        ["GetRandam_Int"] = "randInt",
        ["GetRandam_Float"] = "randFloat",
        ["GetSpecialEffectActivateInterruptId"] = "spEffectInterruptId",
    };

    private static readonly string[] BattleTables = { "actWeights", "actFuncs", "actAfter", "actParams" };

    /// <summary>The source with readable names; the source itself if it doesn't parse or nothing can be named</summary>
    public static string Apply(string src)
    {
        var parse = LuaParser.Parse(src);
        if (!parse.IsValid)
            return src;

        var t = parse.Tokens;
        var occurrences = new Dictionary<string, List<int>>();
        var reserved = new HashSet<string>(LuaLexer.Keywords);

        for (int i = 0; i < t.Count; i++)
        {
            if (t[i].Kind != LuaTokenKind.Name)
                continue;
            if (DecompilerName.IsMatch(t[i].Text))
            {
                if (!occurrences.TryGetValue(t[i].Text, out var list))
                    occurrences[t[i].Text] = list = new List<int>();
                list.Add(i);
            }
            else
                reserved.Add(t[i].Text);
        }

        if (occurrences.Count == 0)
            return src;

        var wanted = new Dictionary<string, string>();
        foreach (var (name, list) in occurrences)
        {
            var base_ = name.Contains("_arg") ? ArgumentName(t, list) : LocalName(t, list[0]);
            if (base_ != null)
                wanted[name] = base_;
        }
        BattleActivateNames(t, wanted);

        if (wanted.Count == 0)
            return src;

        // The top-level statement each name lives in (main-chunk locals: everywhere, group -1)
        int GroupOf(int tokenIndex)
        {
            var s = parse.TopLevelStatements;
            int lo = 0, hi = s.Count - 1;
            while (lo <= hi)
            {
                var mid = (lo + hi) / 2;
                if (tokenIndex < s[mid].First)
                    hi = mid - 1;
                else if (tokenIndex > s[mid].Last)
                    lo = mid + 1;
                else
                    return mid;
            }
            return -2;
        }

        var final = new Dictionary<string, string>();
        var fileWide = new HashSet<string>();
        var groupNames = new Dictionary<int, HashSet<string>>();

        string Unique(string b, HashSet<string> taken)
        {
            var n = b;
            for (int k = 2; reserved.Contains(n) || fileWide.Contains(n) || taken.Contains(n); k++)
                n = b + k;
            return n;
        }

        // Main-chunk locals first: their names are taken in every statement
        foreach (var (name, b) in wanted.Where(w => parse.TopLevelLocals.Contains(w.Key)).OrderBy(w => occurrences[w.Key][0]))
        {
            var allTaken = groupNames.Values.SelectMany(x => x).ToHashSet();
            var n = Unique(b, allTaken);
            fileWide.Add(n);
            final[name] = n;
        }

        foreach (var (name, b) in wanted.Where(w => !parse.TopLevelLocals.Contains(w.Key)).OrderBy(w => occurrences[w.Key][0]))
        {
            // Every occurrence must sit in one statement; otherwise leave the name alone
            var groups = occurrences[name].Select(GroupOf).Distinct().ToList();
            if (groups.Count != 1 || groups[0] < 0)
                continue;
            if (!groupNames.TryGetValue(groups[0], out var taken))
                groupNames[groups[0]] = taken = new HashSet<string>();
            var n = Unique(b, taken);
            taken.Add(n);
            final[name] = n;
        }

        if (final.Count == 0)
            return src;

        var sb = new StringBuilder(src.Length);
        var at = 0;
        foreach (var tok in t)
        {
            if (tok.Kind != LuaTokenKind.Name || !final.TryGetValue(tok.Text, out var replacement))
                continue;
            sb.Append(src, at, tok.Start - at);
            sb.Append(replacement);
            at = tok.Start + tok.Length;
        }
        sb.Append(src, at, src.Length - at);
        return sb.ToString();
    }

    /// <summary>local NAME = &lt;value&gt;: a name from the value's call, or null</summary>
    private static string LocalName(List<LuaToken> t, int i)
    {
        if (i < 1 || !t[i - 1].Is("local") || !t[i + 1].Is("="))
            return null;

        var v = i + 2;
        if (v + 4 >= t.Count || t[v].Kind != LuaTokenKind.Name)
            return null;

        // obj:Method(args)
        if (t[v + 1].Is(":") && t[v + 2].Kind == LuaTokenKind.Name && t[v + 3].Is("("))
            return FromCall(t[v + 2].Text, t[v + 4]);

        // Func(args)
        if (t[v + 1].Is("(") && !DecompilerName.IsMatch(t[v].Text))
            return FromCall(t[v].Text, t[v + 2]);

        return null;
    }

    private static string FromCall(string method, LuaToken firstArg)
    {
        string b;
        if (MethodNames.TryGetValue(method, out var mapped))
            b = mapped;
        else
        {
            var m = method.StartsWith("Get") && method.Length > 3 && char.IsUpper(method[3]) ? method[3..] : method;
            b = Camel(m);
        }

        if (method == "GetParam" || method == "GetNumber")
        {
            if (firstArg.Kind == LuaTokenKind.Number)
                return b + firstArg.Text;
            return b;
        }

        if (method.StartsWith("Get") && firstArg.Kind == LuaTokenKind.Name)
        {
            if (firstArg.Text == "TARGET_ENE_0")
                return "target" + Pascal(b);
            if (firstArg.Text == "TARGET_SELF")
                return "self" + Pascal(b);
        }
        return b;
    }

    /// <summary>goal / ai / self from how a function argument is used, or null</summary>
    private static string ArgumentName(List<LuaToken> t, List<int> uses)
    {
        bool goal = false, other = false;
        foreach (var i in uses)
        {
            if (i + 2 < t.Count && t[i + 1].Is(":") && t[i + 2].Kind == LuaTokenKind.Name)
            {
                if (GoalMethods.Contains(t[i + 2].Text))
                    goal = true;
                else
                    other = true;
            }
        }

        if (goal && !other)
            return "goal";
        if (other && !goal)
            return "ai";
        if (goal || other)
            return null;

        // Goal.X = function (self, ai, goal): FromSoftware's goal callbacks, by position, for an argument that is
        // never a method receiver (only passed on)
        var d = uses[0];
        var name = t[d].Text;
        var start = d;
        while (start > 0 && !t[start - 1].Is("("))
            start--;
        if (start >= 6 && t[start - 2].Is("function") && t[start - 3].Is("=") && t[start - 4].Kind == LuaTokenKind.Name &&
            t[start - 5].Is(".") && t[start - 6].Text == "Goal")
        {
            if (name.EndsWith("_arg0"))
                return "self";
            if (name.EndsWith("_arg1"))
                return "ai";
            if (name.EndsWith("_arg2"))
                return "goal";
        }
        return null;
    }

    /// <summary>Common_Battle_Activate(ai, goal, weights, funcs, after, params): name the four tables passed to it</summary>
    private static void BattleActivateNames(List<LuaToken> t, Dictionary<string, string> wanted)
    {
        for (int i = 0; i + 1 < t.Count; i++)
        {
            if (t[i].Kind != LuaTokenKind.Name || t[i].Text != "Common_Battle_Activate" || !t[i + 1].Is("("))
                continue;

            // Plain name arguments only: Common_Battle_Activate(a, b, c, d, e, f)
            var args = new List<string>();
            var j = i + 2;
            while (j < t.Count && t[j].Kind == LuaTokenKind.Name)
            {
                args.Add(t[j].Text);
                if (t[j + 1].Is(","))
                    j += 2;
                else
                    break;
            }
            if (args.Count != 6 || !t[j + 1].Is(")"))
                continue;

            for (int k = 0; k < 4; k++)
            {
                if (DecompilerName.IsMatch(args[k + 2]) && args[k + 2].Contains("_local"))
                    wanted[args[k + 2]] = BattleTables[k];
            }
        }
    }

    private static string Camel(string s)
    {
        var parts = s.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "value";
        var sb = new StringBuilder();
        for (int p = 0; p < parts.Length; p++)
        {
            var w = parts[p];
            sb.Append(p == 0 ? char.ToLowerInvariant(w[0]) : char.ToUpperInvariant(w[0]));
            sb.Append(w, 1, w.Length - 1);
        }
        var r = sb.ToString();
        return LuaLexer.Keywords.Contains(r) ? r + "_" : r;
    }

    private static string Pascal(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
