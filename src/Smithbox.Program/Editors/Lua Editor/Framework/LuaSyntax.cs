namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The syntax tree the parser builds. Every node keeps the range of tokens it was parsed from (First..Last,
/// inclusive, indexes into <see cref="LuaParseResult.Tokens"/>), so views can show a node's source text and edits
/// can replace a literal exactly where it is.
/// </summary>
public abstract class LuaNode
{
    public int First;
    public int Last;
}

public sealed class LuaBlock : LuaNode
{
    public List<LuaStat> Stats { get; } = new();
}

public abstract class LuaStat : LuaNode { }

/// <summary>if / elseif clauses in order, then the else block (or null)</summary>
public sealed class LuaIfStat : LuaStat
{
    public List<(LuaExpr Condition, LuaBlock Body)> Clauses { get; } = new();
    public LuaBlock Else;
}

public sealed class LuaAssignStat : LuaStat
{
    public List<LuaExpr> Targets { get; } = new();
    public List<LuaExpr> Values { get; } = new();
}

public sealed class LuaLocalStat : LuaStat
{
    public List<string> Names { get; } = new();
    public List<LuaExpr> Values { get; } = new();
}

public sealed class LuaCallStat : LuaStat
{
    public LuaCallExpr Call;
}

/// <summary>function a.b:c(...) end, or local function a(...) end</summary>
public sealed class LuaFunctionStat : LuaStat
{
    public string Name;
    public bool IsLocal;
    public LuaFunctionExpr Function;
}

public sealed class LuaReturnStat : LuaStat
{
    public List<LuaExpr> Values { get; } = new();
}

/// <summary>while, numeric / generic for, repeat, do: only the body matters to the views</summary>
public sealed class LuaLoopStat : LuaStat
{
    public string Keyword;
    public LuaBlock Body;
}

public sealed class LuaBreakStat : LuaStat { }

public abstract class LuaExpr : LuaNode { }

public sealed class LuaNumberExpr : LuaExpr
{
    public double Value;
}

public sealed class LuaStringExpr : LuaExpr { }

/// <summary>nil, true or false</summary>
public sealed class LuaConstantExpr : LuaExpr
{
    public string Keyword;
}

public sealed class LuaNameExpr : LuaExpr
{
    public string Name;
}

/// <summary>obj.name</summary>
public sealed class LuaFieldExpr : LuaExpr
{
    public LuaExpr Object;
    public string Name;
}

/// <summary>obj[key]</summary>
public sealed class LuaIndexExpr : LuaExpr
{
    public LuaExpr Object;
    public LuaExpr Key;
}

/// <summary>f(args), or obj:Method(args) (Method set)</summary>
public sealed class LuaCallExpr : LuaExpr
{
    public LuaExpr Callee;
    public string Method;
    public List<LuaExpr> Arguments { get; } = new();
}

public sealed class LuaFunctionExpr : LuaExpr
{
    public List<string> Parameters { get; } = new();
    public bool IsVararg;
    public LuaBlock Body;
}

public sealed class LuaBinaryExpr : LuaExpr
{
    public string Operator;
    public LuaExpr Left;
    public LuaExpr Right;
}

public sealed class LuaUnaryExpr : LuaExpr
{
    public string Operator;
    public LuaExpr Operand;
}

public sealed class LuaParenExpr : LuaExpr
{
    public LuaExpr Inner;
}

/// <summary>{ … }: its fields in order — [key] = value (Key), name = value (Name), or a positional value (neither)</summary>
public sealed class LuaTableExpr : LuaExpr
{
    public List<LuaTableField> Fields { get; } = new();
}

public sealed record LuaTableField(LuaExpr Key, string Name, LuaExpr Value);

public static class LuaSyntaxExtensions
{
    /// <summary>A chunk's top-level global functions (function Name(…)), added to a lookup (an earlier definition is kept)</summary>
    public static void AddGlobalFunctions(this LuaBlock chunk, Dictionary<string, LuaFunctionExpr> into)
    {
        foreach (var s in chunk.Stats)
        {
            if (s is LuaFunctionStat { IsLocal: false } fs && !fs.Name.Contains('.') && !fs.Name.Contains(':'))
                into.TryAdd(fs.Name, fs.Function);
        }
    }

    /// <summary>The node's source text (from its first token's start to its last token's end)</summary>
    public static string SourceOf(this LuaNode node, string src, List<LuaToken> tokens)
    {
        var a = tokens[node.First];
        var b = tokens[node.Last];
        return src.Substring(a.Start, b.Start + b.Length - a.Start);
    }

    /// <summary>A number literal, with a leading minus folded in; null for anything else</summary>
    public static double? AsNumber(this LuaExpr e) => e switch
    {
        LuaNumberExpr n => n.Value,
        LuaUnaryExpr { Operator: "-", Operand: LuaNumberExpr n } => -n.Value,
        _ => null,
    };

    /// <summary>Every statement in a block and the blocks under it, depth first, in source order</summary>
    public static IEnumerable<LuaStat> AllStatements(this LuaBlock block)
    {
        foreach (var s in block.Stats)
        {
            yield return s;
            foreach (var child in ChildBlocks(s))
                foreach (var inner in child.AllStatements())
                    yield return inner;
        }
    }

    public static IEnumerable<LuaBlock> ChildBlocks(LuaStat s)
    {
        switch (s)
        {
            case LuaIfStat i:
                foreach (var c in i.Clauses)
                    yield return c.Body;
                if (i.Else != null)
                    yield return i.Else;
                break;
            case LuaLoopStat l:
                yield return l.Body;
                break;
            case LuaFunctionStat f:
                yield return f.Function.Body;
                break;
        }
    }

    /// <summary>Every call in an expression or statement tree (not descending into nested function bodies)</summary>
    public static IEnumerable<LuaCallExpr> Calls(this LuaNode node)
    {
        switch (node)
        {
            case LuaCallExpr c:
                yield return c;
                foreach (var x in c.Callee.Calls()) yield return x;
                foreach (var a in c.Arguments)
                    foreach (var x in a.Calls()) yield return x;
                break;
            case LuaCallStat cs:
                foreach (var x in cs.Call.Calls()) yield return x;
                break;
            case LuaAssignStat a:
                foreach (var e in a.Targets.Concat(a.Values))
                    foreach (var x in e.Calls()) yield return x;
                break;
            case LuaLocalStat l:
                foreach (var e in l.Values)
                    foreach (var x in e.Calls()) yield return x;
                break;
            case LuaReturnStat r:
                foreach (var e in r.Values)
                    foreach (var x in e.Calls()) yield return x;
                break;
            case LuaIfStat i:
                foreach (var cl in i.Clauses)
                    foreach (var x in cl.Condition.Calls()) yield return x;
                break;
            case LuaFieldExpr f:
                foreach (var x in f.Object.Calls()) yield return x;
                break;
            case LuaIndexExpr ix:
                foreach (var x in ix.Object.Calls().Concat(ix.Key.Calls())) yield return x;
                break;
            case LuaBinaryExpr b:
                foreach (var x in b.Left.Calls().Concat(b.Right.Calls())) yield return x;
                break;
            case LuaUnaryExpr u:
                foreach (var x in u.Operand.Calls()) yield return x;
                break;
            case LuaParenExpr p:
                foreach (var x in p.Inner.Calls()) yield return x;
                break;
            case LuaTableExpr table:
                foreach (var field in table.Fields)
                {
                    if (field.Key != null)
                        foreach (var x in field.Key.Calls()) yield return x;
                    foreach (var x in field.Value.Calls()) yield return x;
                }
                break;
        }
    }
}
