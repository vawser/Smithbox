namespace StudioCore.Editors.LuaEditor;

/// <summary>A named thing a script defines: a function (function X / X.Y = function) or a top-level global value</summary>
public readonly record struct LuaDefinition(string Name, int Line, int Column, bool IsFunction, bool IsTopLevel);

public class LuaParseResult
{
    /// <summary>The first syntax problem (Lua reports one), or none; lexer problems are listed too</summary>
    public List<LuaProblem> Problems { get; } = new();

    /// <summary>Tokens without comments, ending with EndOfFile</summary>
    public List<LuaToken> Tokens { get; set; }

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
/// collects what the editor needs from valid source: the assigned globals (for .luagnl), definitions (outline and
/// go-to-definition) and the main chunk's statement ranges (for the local renaming).
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
            parser.Chunk();
            foreach (var name in parser.TopGlobals.Concat(parser.NestedGlobals))
            {
                if (!result.Globals.Contains(name))
                    result.Globals.Add(name);
            }
        }
        catch (SyntaxError e)
        {
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

    private void Chunk()
    {
        OpenScope();
        while (!BlockFollows())
        {
            var first = P;
            var last = Statement();
            if (FunctionDepth == 0 && Scopes.Count == 1)
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
    }

    private bool BlockFollows() =>
        Cur.Kind == LuaTokenKind.EndOfFile || Check("else") || Check("elseif") || Check("end") || Check("until");

    /// <summary>A block in its own scope</summary>
    private void Block()
    {
        OpenScope();
        BlockBody();
        CloseScope();
    }

    private void BlockBody()
    {
        while (!BlockFollows())
        {
            var first = P;
            if (Statement())
            {
                if (!BlockFollows())
                    throw Error(LOC.Get("LUA_Syntax_Must_Be_Last", T[first].Text));
                break;
            }
        }
    }

    /// <summary>One statement (and its optional ';'); true for return / break, which must end their block</summary>
    private bool Statement()
    {
        var t = Cur;
        var isLast = false;

        if (t.Is("if"))
        {
            Next();
            Expression();
            Expect("then");
            Block();
            while (Check("elseif"))
            {
                Next();
                Expression();
                Expect("then");
                Block();
            }
            if (Accept("else"))
                Block();
            ExpectMatch("end", "if", t.Line);
        }
        else if (t.Is("while"))
        {
            Next();
            Expression();
            Expect("do");
            Block();
            ExpectMatch("end", "while", t.Line);
        }
        else if (t.Is("do"))
        {
            Next();
            Block();
            ExpectMatch("end", "do", t.Line);
        }
        else if (t.Is("for"))
        {
            Next();
            var first = ExpectName();
            OpenScope();
            if (Accept("="))
            {
                Expression();
                Expect(",");
                Expression();
                if (Accept(","))
                    Expression();
                Declare(first.Text);
            }
            else
            {
                var names = new List<string> { first.Text };
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
            Block();
            CloseScope();
            ExpectMatch("end", "for", t.Line);
        }
        else if (t.Is("repeat"))
        {
            Next();
            // Lua 5.0: the condition can't see the block's locals
            Block();
            ExpectMatch("until", "repeat", t.Line);
            Expression();
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
            FunctionBody(method);
        }
        else if (t.Is("local"))
        {
            Next();
            if (Accept("function"))
            {
                var name = ExpectName();
                Declare(name.Text);
                FunctionBody(false);
            }
            else
            {
                var names = new List<string> { ExpectName().Text };
                while (Accept(","))
                    names.Add(ExpectName().Text);
                if (Accept("="))
                    ExpressionList();
                foreach (var n in names)
                    Declare(n);
            }
        }
        else if (t.Is("return"))
        {
            Next();
            if (!BlockFollows() && !Check(";"))
                ExpressionList();
            isLast = true;
        }
        else if (t.Is("break"))
        {
            Next();
            isLast = true;
        }
        else
        {
            ExpressionStatement();
        }

        Accept(";");
        return isLast;
    }

    private void Define(string name, LuaToken at, bool isFunction)
    {
        Result.Definitions.Add(new LuaDefinition(name, at.Line, at.Column, isFunction, FunctionDepth == 0));
    }

    /// <summary>A call, or an assignment to one or more variables</summary>
    private void ExpressionStatement()
    {
        var first = Cur;
        var (kind, name) = SuffixedExpression();

        if (Check("=") || Check(","))
        {
            var targets = new List<(TargetKind Kind, string Name, LuaToken At)> { (kind, name, first) };
            while (Accept(","))
            {
                var at = Cur;
                var (k, n) = SuffixedExpression();
                targets.Add((k, n, at));
            }
            Expect("=");
            var valueStart = P;
            ExpressionList();
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
            return;
        }

        if (kind != TargetKind.Call)
            throw new SyntaxError(first, LOC.Get("LUA_Syntax_Not_A_Statement"));
    }

    private enum TargetKind { Name, Field, Call, Other }

    /// <summary>
    /// prefixexp { '.' Name | '[' exp ']' | ':' Name args | args }. Returns what it ends with: a plain name, an indexed
    /// field (assignable), a call, or something else; and the dotted name for a Name.Name… chain (for definitions).
    /// </summary>
    private (TargetKind Kind, string Name) SuffixedExpression()
    {
        TargetKind kind;
        string name = null;

        if (Cur.Kind == LuaTokenKind.Name)
        {
            name = Next().Text;
            kind = TargetKind.Name;
        }
        else if (Check("("))
        {
            var open = Next();
            Expression();
            ExpectMatch(")", "(", open.Line);
            kind = TargetKind.Other;
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
            }
            else if (Check("["))
            {
                Next();
                Expression();
                Expect("]");
                kind = TargetKind.Field;
                name = null;
            }
            else if (Accept(":"))
            {
                ExpectName();
                CallArguments();
                kind = TargetKind.Call;
                name = null;
            }
            else if (Check("(") || Check("{") || Cur.Kind == LuaTokenKind.String)
            {
                CallArguments();
                kind = TargetKind.Call;
                name = null;
            }
            else
                break;
        }
        return (kind, name);
    }

    private void CallArguments()
    {
        if (Cur.Kind == LuaTokenKind.String)
        {
            Next();
            return;
        }
        if (Check("{"))
        {
            TableConstructor();
            return;
        }
        var open = Expect("(");
        if (!Check(")"))
            ExpressionList();
        ExpectMatch(")", "(", open.Line);
    }

    private void FunctionBody(bool isMethod)
    {
        var start = Cur;
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
                    break;
                }
                Declare(ExpectName().Text);
                if (!Accept(","))
                    break;
            }
        }
        Expect(")");
        BlockBody();
        CloseScope();
        FunctionDepth--;
        ExpectMatch("end", "function", start.Line);
    }

    private void ExpressionList()
    {
        Expression();
        while (Accept(","))
            Expression();
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

    private void Expression() => SubExpression(0);

    private void SubExpression(int limit)
    {
        if (Check("not") || Check("-"))
        {
            Next();
            SubExpression(UnaryPriority);
        }
        else
            SimpleExpression();

        while (BinaryPriority(Cur) is { } p && p.Left > limit)
        {
            Next();
            SubExpression(p.Right);
        }
    }

    private void SimpleExpression()
    {
        var t = Cur;
        switch (t.Kind)
        {
            case LuaTokenKind.Number:
            case LuaTokenKind.String:
                Next();
                return;
            case LuaTokenKind.Keyword when t.Text is "nil" or "true" or "false":
                Next();
                return;
            case LuaTokenKind.Keyword when t.Text == "function":
                Next();
                FunctionBody(false);
                return;
        }

        if (Check("{"))
        {
            TableConstructor();
            return;
        }

        if (Check("..."))
            throw Error(LOC.Get("LUA_Syntax_No_Vararg_Expression"));

        SuffixedExpression();
    }

    private void TableConstructor()
    {
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
    }
}
