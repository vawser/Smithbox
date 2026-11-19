namespace StudioCore.Editors.LuaEditor;

/// <summary>A named thing a script defines: a function (function X / X.Y = function) or a top-level global value</summary>
public readonly record struct LuaDefinition(string Name, int Line, int Column, bool IsFunction, bool IsTopLevel);

public class LuaParseResult
{
    /// <summary>The first syntax problem (Lua reports one), or none; lexer problems are listed too</summary>
    public List<LuaProblem> Problems { get; } = new();

    /// <summary>Tokens without comments, ending with EndOfFile</summary>
    public List<LuaToken> Tokens { get; set; }

    /// <summary>The main chunk's syntax tree (null when there is a problem)</summary>
    public LuaBlock Tree { get; set; }

    /// <summary>
    /// The global names the script assigns, in the order its .luagnl lists them: assignments in the main chunk in line
    /// order, then those inside functions in line order, each name once. (This order is how the compiler emits
    /// SETGLOBAL: the main function first, then nested functions in order; LuaAiSurvey checks it against every
    /// vanilla binder.) Only valid when there are no problems.
    /// </summary>
    public List<string> Globals { get; } = new();

    public List<LuaDefinition> Definitions { get; } = new();

    /// <summary>Token ranges (first, last; inclusive) of the main chunk's statements</summary>
    public List<(int First, int Last)> TopLevelStatements { get; } = new();

    /// <summary>Names declared local in the main chunk (visible to every function after them)</summary>
    public HashSet<string> TopLevelLocals { get; } = new();

    public bool IsValid => Problems.Count == 0;
}

/// <summary>
/// A Lua 5.0 syntax check (lparser.c's grammar, no code generation). It reports the first error, as Lua does, and
/// from valid source builds the syntax tree and collects what the editor needs: the assigned globals (for .luagnl),
/// definitions (outline and go-to-definition) and the main chunk's statement ranges (for the local renaming).
/// </summary>
public class LuaParser
{
    private sealed class SyntaxError : Exception
    {
        public LuaToken At { get; }
        public SyntaxError(LuaToken at, string message) : base(message) { At = at; }
    }

    private readonly List<LuaToken> T;
    private int P;
    private readonly LuaParseResult Result;

    private readonly List<HashSet<string>> Scopes = new();
    private int FunctionDepth;

    private readonly List<string> TopGlobals = new();
    private readonly List<string> NestedGlobals = new();

    private LuaParser(List<LuaToken> tokens, LuaParseResult result)
    {
        T = tokens;
        Result = result;
    }

    public static LuaParseResult Parse(string src)
    {
        var result = new LuaParseResult();
        var tokens = LuaLexer.Tokenize(src, result.Problems);
        result.Tokens = tokens;

        // The lexer's first problem is the error Lua would report
        if (result.Problems.Count > 0)
        {
            result.Problems.RemoveRange(1, result.Problems.Count - 1);
            return result;
        }

        var parser = new LuaParser(tokens, result);
        try
        {
            result.Tree = parser.Chunk();
            foreach (var name in parser.TopGlobals.Concat(parser.NestedGlobals))
            {
                if (!result.Globals.Contains(name))
                    result.Globals.Add(name);
            }
        }
        catch (SyntaxError e)
        {
            result.Tree = null;
            result.Problems.Add(new LuaProblem(e.At.Line, e.At.Column, e.Message));
        }
        return result;
    }

    // --- tokens ---

    private LuaToken Cur => T[P];

    private LuaToken Next() => T[P++];

    private bool Check(string s) => Cur.Is(s);

    private bool Accept(string s)
    {
        if (!Check(s))
            return false;
        P++;
        return true;
    }

    private LuaToken Expect(string s)
    {
        if (!Check(s))
            throw Error(LOC.Get("LUA_Syntax_Expected", s, Describe(Cur)));
        return Next();
    }

    /// <summary>Expects the closing word of a construct; names the line it opened on, as Lua does</summary>
    private void ExpectMatch(string what, string who, int line)
    {
        if (Accept(what))
            return;
        if (line == Cur.Line)
            throw Error(LOC.Get("LUA_Syntax_Expected", what, Describe(Cur)));
        throw Error(LOC.Get("LUA_Syntax_Expected_To_Close", what, who, line, Describe(Cur)));
    }

    private LuaToken ExpectName()
    {
        if (Cur.Kind != LuaTokenKind.Name)
            throw Error(LOC.Get("LUA_Syntax_Expected_Name", Describe(Cur)));
        return Next();
    }

    private SyntaxError Error(string message) => new(Cur, message);

    private static string Describe(LuaToken t) => t.Kind == LuaTokenKind.EndOfFile ? LOC.Get("LUA_Syntax_End_Of_File") : $"'{t.Text}'";

    /// <summary>Sets a node's range: from first to the last token consumed</summary>
    private TNode Done<TNode>(TNode node, int first) where TNode : LuaNode
    {
        node.First = first;
        node.Last = Math.Max(first, P - 1);
        return node;
    }

    // --- scopes ---

    private void OpenScope() => Scopes.Add(new HashSet<string>());

    private void CloseScope() => Scopes.RemoveAt(Scopes.Count - 1);

    private void Declare(string name)
    {
        Scopes[^1].Add(name);
        if (FunctionDepth == 0 && Scopes.Count == 1)
            Result.TopLevelLocals.Add(name);
    }

    private bool IsLocal(string name)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
            if (Scopes[i].Contains(name))
                return true;
        return false;
    }

    private void AssignGlobal(string name)
    {
        (FunctionDepth == 0 ? TopGlobals : NestedGlobals).Add(name);
    }

    // --- grammar ---

    private LuaBlock Chunk()
    {
        var block = new LuaBlock();
        var start = P;
        OpenScope();
        while (!BlockFollows())
        {
            var first = P;
            var (stat, last) = Statement();
            block.Stats.Add(stat);
            Result.TopLevelStatements.Add((first, P - 1));
            if (last)
            {
                if (!BlockFollows())
                    throw Error(LOC.Get("LUA_Syntax_Must_Be_Last", T[first].Text));
                break;
            }
        }
        if (Cur.Kind != LuaTokenKind.EndOfFile)
            throw Error(LOC.Get("LUA_Syntax_Expected_End_Of_File", Describe(Cur)));
        CloseScope();
        return Done(block, start);
    }

    private bool BlockFollows() =>
        Cur.Kind == LuaTokenKind.EndOfFile || Check("else") || Check("elseif") || Check("end") || Check("until");

    /// <summary>A block in its own scope</summary>
    private LuaBlock Block()
    {
        OpenScope();
        var block = BlockBody();
        CloseScope();
        return block;
    }

    private LuaBlock BlockBody()
    {
        var block = new LuaBlock();
        var start = P;
        while (!BlockFollows())
        {
            var first = P;
            var (stat, last) = Statement();
            block.Stats.Add(stat);
            if (last)
            {
                if (!BlockFollows())
                    throw Error(LOC.Get("LUA_Syntax_Must_Be_Last", T[first].Text));
                break;
            }
        }
        return Done(block, start);
    }

    /// <summary>One statement (and its optional ';'); Last is true for return / break, which must end their block</summary>
    private (LuaStat Stat, bool Last) Statement()
    {
        var t = Cur;
        var first = P;
        var isLast = false;
        LuaStat stat;

        if (t.Is("if"))
        {
            Next();
            var s = new LuaIfStat();
            var cond = Expression();
            Expect("then");
            s.Clauses.Add((cond, Block()));
            while (Check("elseif"))
            {
                Next();
                cond = Expression();
                Expect("then");
                s.Clauses.Add((cond, Block()));
            }
            if (Accept("else"))
                s.Else = Block();
            ExpectMatch("end", "if", t.Line);
            stat = s;
        }
        else if (t.Is("while"))
        {
            Next();
            Expression();
            Expect("do");
            var body = Block();
            ExpectMatch("end", "while", t.Line);
            stat = new LuaLoopStat { Keyword = "while", Body = body };
        }
        else if (t.Is("do"))
        {
            Next();
            var body = Block();
            ExpectMatch("end", "do", t.Line);
            stat = new LuaLoopStat { Keyword = "do", Body = body };
        }
        else if (t.Is("for"))
        {
            Next();
            var firstName = ExpectName();
            OpenScope();
            if (Accept("="))
            {
                Expression();
                Expect(",");
                Expression();
                if (Accept(","))
                    Expression();
                Declare(firstName.Text);
            }
            else
            {
                var names = new List<string> { firstName.Text };
                while (Accept(","))
                    names.Add(ExpectName().Text);
                if (!Check("in"))
                    throw Error(LOC.Get("LUA_Syntax_Expected_For", Describe(Cur)));
                Next();
                ExpressionList();
                foreach (var n in names)
                    Declare(n);
            }
            Expect("do");
            var body = Block();
            CloseScope();
            ExpectMatch("end", "for", t.Line);
            stat = new LuaLoopStat { Keyword = "for", Body = body };
        }
        else if (t.Is("repeat"))
        {
            Next();
            // Lua 5.0: the condition can't see the block's locals
            var body = Block();
            ExpectMatch("until", "repeat", t.Line);
            Expression();
            stat = new LuaLoopStat { Keyword = "repeat", Body = body };
        }
        else if (t.Is("function"))
        {
            Next();
            var name = ExpectName();
            var full = name.Text;
            var dotted = false;
            var method = false;
            while (Accept("."))
            {
                full += "." + ExpectName().Text;
                dotted = true;
            }
            if (Accept(":"))
            {
                full += ":" + ExpectName().Text;
                method = true;
            }
            if (!dotted && !method && !IsLocal(name.Text))
                AssignGlobal(name.Text);
            Define(full, name, true);
            stat = new LuaFunctionStat { Name = full, Function = FunctionBody(method, P) };
        }
        else if (t.Is("local"))
        {
            Next();
            if (Accept("function"))
            {
                var name = ExpectName();
                Declare(name.Text);
                stat = new LuaFunctionStat { Name = name.Text, IsLocal = true, Function = FunctionBody(false, P) };
            }
            else
            {
                var s = new LuaLocalStat();
                s.Names.Add(ExpectName().Text);
                while (Accept(","))
                    s.Names.Add(ExpectName().Text);
                if (Accept("="))
                    s.Values.AddRange(ExpressionList());
                foreach (var n in s.Names)
                    Declare(n);
                stat = s;
            }
        }
        else if (t.Is("return"))
        {
            Next();
            var s = new LuaReturnStat();
            if (!BlockFollows() && !Check(";"))
                s.Values.AddRange(ExpressionList());
            isLast = true;
            stat = s;
        }
        else if (t.Is("break"))
        {
            Next();
            isLast = true;
            stat = new LuaBreakStat();
        }
        else
        {
            stat = ExpressionStatement();
        }

        Accept(";");
        return (Done(stat, first), isLast);
    }

    private void Define(string name, LuaToken at, bool isFunction)
    {
        Result.Definitions.Add(new LuaDefinition(name, at.Line, at.Column, isFunction, FunctionDepth == 0));
    }

    /// <summary>A call, or an assignment to one or more variables</summary>
    private LuaStat ExpressionStatement()
    {
        var first = Cur;
        var (expr, kind, name) = SuffixedExpression();

        if (Check("=") || Check(","))
        {
            var stat = new LuaAssignStat();
            stat.Targets.Add(expr);
            var targets = new List<(TargetKind Kind, string Name, LuaToken At)> { (kind, name, first) };
            while (Accept(","))
            {
                var at = Cur;
                var (e, k, n) = SuffixedExpression();
                stat.Targets.Add(e);
                targets.Add((k, n, at));
            }
            Expect("=");
            var valueStart = P;
            stat.Values.AddRange(ExpressionList());
            var isFunctionValue = T[valueStart].Is("function");

            foreach (var target in targets)
            {
                if (target.Kind is TargetKind.Call or TargetKind.Other)
                    throw new SyntaxError(target.At, LOC.Get("LUA_Syntax_Cannot_Assign"));
                if (target.Kind == TargetKind.Name && !IsLocal(target.Name))
                {
                    AssignGlobal(target.Name);
                    if (FunctionDepth == 0)
                        Define(target.Name, target.At, isFunctionValue && targets.Count == 1);
                }
                else if (target.Kind == TargetKind.Field && FunctionDepth == 0 && isFunctionValue && targets.Count == 1)
                    Define(target.Name, target.At, true);
            }
            return stat;
        }

        if (kind != TargetKind.Call)
            throw new SyntaxError(first, LOC.Get("LUA_Syntax_Not_A_Statement"));
        return new LuaCallStat { Call = (LuaCallExpr)expr };
    }

    private enum TargetKind { Name, Field, Call, Other }

    /// <summary>
    /// prefixexp { '.' Name | '[' exp ']' | ':' Name args | args }. Returns the expression, what it ends with (a
    /// plain name, an indexed field (assignable), a call, or something else), and the dotted name for a Name.Name…
    /// chain (for definitions).
    /// </summary>
    private (LuaExpr Expr, TargetKind Kind, string Name) SuffixedExpression()
    {
        var start = P;
        TargetKind kind;
        string name = null;
        LuaExpr expr;

        if (Cur.Kind == LuaTokenKind.Name)
        {
            name = Next().Text;
            kind = TargetKind.Name;
            expr = Done(new LuaNameExpr { Name = name }, start);
        }
        else if (Check("("))
        {
            var open = Next();
            var inner = Expression();
            ExpectMatch(")", "(", open.Line);
            kind = TargetKind.Other;
            expr = Done(new LuaParenExpr { Inner = inner }, start);
        }
        else
            throw Error(LOC.Get("LUA_Syntax_Unexpected_Symbol", Describe(Cur)));

        while (true)
        {
            if (Accept("."))
            {
                var field = ExpectName().Text;
                name = name != null && kind is TargetKind.Name or TargetKind.Field ? $"{name}.{field}" : null;
                kind = TargetKind.Field;
                expr = Done(new LuaFieldExpr { Object = expr, Name = field }, start);
            }
            else if (Check("["))
            {
                Next();
                var key = Expression();
                Expect("]");
                kind = TargetKind.Field;
                name = null;
                expr = Done(new LuaIndexExpr { Object = expr, Key = key }, start);
            }
            else if (Accept(":"))
            {
                var method = ExpectName().Text;
                var call = new LuaCallExpr { Callee = expr, Method = method };
                CallArguments(call);
                kind = TargetKind.Call;
                name = null;
                expr = Done(call, start);
            }
            else if (Check("(") || Check("{") || Cur.Kind == LuaTokenKind.String)
            {
                var call = new LuaCallExpr { Callee = expr };
                CallArguments(call);
                kind = TargetKind.Call;
                name = null;
                expr = Done(call, start);
            }
            else
                break;
        }
        return (expr, kind, name);
    }

    private void CallArguments(LuaCallExpr call)
    {
        var start = P;
        if (Cur.Kind == LuaTokenKind.String)
        {
            Next();
            call.Arguments.Add(Done(new LuaStringExpr(), start));
            return;
        }
        if (Check("{"))
        {
            call.Arguments.Add(TableConstructor());
            return;
        }
        var open = Expect("(");
        if (!Check(")"))
            call.Arguments.AddRange(ExpressionList());
        ExpectMatch(")", "(", open.Line);
    }

    private LuaFunctionExpr FunctionBody(bool isMethod, int start)
    {
        var startToken = Cur;
        var func = new LuaFunctionExpr();
        FunctionDepth++;
        OpenScope();
        if (isMethod)
            Declare("self");

        Expect("(");
        if (!Check(")"))
        {
            while (true)
            {
                if (Accept("..."))
                {
                    // Lua 5.0 varargs arrive in the table 'arg'
                    Declare("arg");
                    func.IsVararg = true;
                    break;
                }
                var p = ExpectName().Text;
                func.Parameters.Add(p);
                Declare(p);
                if (!Accept(","))
                    break;
            }
        }
        Expect(")");
        func.Body = BlockBody();
        CloseScope();
        FunctionDepth--;
        ExpectMatch("end", "function", startToken.Line);
        return Done(func, start);
    }

    private List<LuaExpr> ExpressionList()
    {
        var list = new List<LuaExpr> { Expression() };
        while (Accept(","))
            list.Add(Expression());
        return list;
    }

    // lparser.c priorities (left, right)
    private static (int Left, int Right)? BinaryPriority(LuaToken t)
    {
        if (t.Kind != LuaTokenKind.Symbol && t.Kind != LuaTokenKind.Keyword)
            return null;
        return t.Text switch
        {
            "+" or "-" => (6, 6),
            "*" or "/" => (7, 7),
            "^" => (10, 9),
            ".." => (5, 4),
            "==" or "~=" or "<" or "<=" or ">" or ">=" => (3, 3),
            "and" => (2, 2),
            "or" => (1, 1),
            _ => null,
        };
    }

    private const int UnaryPriority = 8;

    private LuaExpr Expression() => SubExpression(0);

    private LuaExpr SubExpression(int limit)
    {
        var start = P;
        LuaExpr left;
        if (Check("not") || Check("-"))
        {
            var op = Next().Text;
            var operand = SubExpression(UnaryPriority);
            left = Done(new LuaUnaryExpr { Operator = op, Operand = operand }, start);
        }
        else
            left = SimpleExpression();

        while (BinaryPriority(Cur) is { } p && p.Left > limit)
        {
            var op = Next().Text;
            var right = SubExpression(p.Right);
            left = Done(new LuaBinaryExpr { Operator = op, Left = left, Right = right }, start);
        }
        return left;
    }

    private LuaExpr SimpleExpression()
    {
        var start = P;
        var t = Cur;
        switch (t.Kind)
        {
            case LuaTokenKind.Number:
                Next();
                double.TryParse(t.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v);
                return Done(new LuaNumberExpr { Value = v }, start);
            case LuaTokenKind.String:
                Next();
                return Done(new LuaStringExpr(), start);
            case LuaTokenKind.Keyword when t.Text is "nil" or "true" or "false":
                Next();
                return Done(new LuaConstantExpr { Keyword = t.Text }, start);
            case LuaTokenKind.Keyword when t.Text == "function":
                Next();
                return FunctionBody(false, start);
        }

        if (Check("{"))
            return TableConstructor();

        if (Check("..."))
            throw Error(LOC.Get("LUA_Syntax_No_Vararg_Expression"));

        return SuffixedExpression().Expr;
    }

    private LuaTableExpr TableConstructor()
    {
        var start = P;
        var open = Expect("{");
        while (!Check("}"))
        {
            if (Check("["))
            {
                Next();
                Expression();
                Expect("]");
                Expect("=");
                Expression();
            }
            else if (Cur.Kind == LuaTokenKind.Name && T[P + 1].Is("="))
            {
                Next();
                Next();
                Expression();
            }
            else
                Expression();

            if (!Accept(",") && !Accept(";"))
                break;
        }
        ExpectMatch("}", "{", open.Line);
        return Done(new LuaTableExpr(), start);
    }
}
