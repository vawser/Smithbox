using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace StudioCore.Editors.EsdEditor;

/// <summary>A parse failure: what's wrong and where (a character index of the text)</summary>
public record EsdParseError(string Message, int Position);

/// <summary>
/// The editable text form of ESD expressions (P4 text input; Claude/Documentation/ESD Block Editor Analysis.md §9
/// decision 5): Format writes a tree as text, Parse reads text back into a tree, Merge keeps an old expression's exact
/// bytes wherever the new tree matches it.
///
/// Syntax: GetEventFlag(1234) == 1 && f57(0) != 1 || arg0 > 2.5f
/// - Functions by catalogue name (when the name is unique) or f&lt;id&gt;; an enum arg's value by its name.
/// - Ints (decimal or 0x hex, s32), floats with f (1.5f), doubles with a point or exponent (1.5), strings "...".
/// - arg0..argN (or arg[expr]), registers r0..r7, (r0 = expr) to store, CallStatus / CallDone, op96, op9A(x).
/// - Precedence: * / then + - then comparisons (&lt; &lt;= &gt; &gt;= == != op96) then &amp;&amp; then ||; all left to right.
///
/// Vanilla's A6 (separator) and B7 (stop if false) markers follow no single rule (the same call has A6 in one file and
/// not in another), so the text leaves them out. Parse makes none; Merge puts the old ones back on unchanged parts. The
/// gate (EsdSurvey exprtext) checks every vanilla expression: Format → Parse gives the same tree without markers, and
/// Merge with the original gives its exact bytes back.
/// Pure: compiled into Claude/Tools/EsdSurvey.
/// </summary>
public static class EsdExprText
{
    // --- names ---

    private class Names
    {
        public Dictionary<string, long> Functions = new();
        public HashSet<string> Ambiguous = new();
    }

    private static readonly ConditionalWeakTable<EsdKindCatalog, Names> NameCache = new();

    private static Names NamesOf(EsdKindCatalog cat) => NameCache.GetValue(cat, c =>
    {
        var names = new Names();
        foreach (var (id, def) in c.Functions)
        {
            if (string.IsNullOrEmpty(def.Name) || !IsIdentifier(def.Name))
                continue;
            if (names.Functions.ContainsKey(def.Name))
                names.Ambiguous.Add(def.Name);
            else
                names.Functions[def.Name] = id;
        }
        foreach (var a in names.Ambiguous)
            names.Functions.Remove(a);
        return names;
    });

    private static bool IsIdentifier(string s) =>
        s.Length > 0 && (char.IsLetter(s[0]) || s[0] == '_') && s.All(ch => char.IsLetterOrDigit(ch) || ch == '_');

    /// <summary>A function's name in text: its catalogue name when that names only it, else f&lt;id&gt;</summary>
    public static string FunctionName(EsdKindCatalog cat, long id)
    {
        var def = cat?.Function(id);
        if (def?.Name != null && NamesOf(cat).Functions.TryGetValue(def.Name, out var back) && back == id)
            return def.Name;
        return $"f{id}";
    }

    /// <summary>An enum value's name in text, when the arg is an enum and the name is unique in it</summary>
    private static string EnumName(EsdKindCatalog cat, EsdArgDef arg, long value)
    {
        var e = arg?.Enum != null ? cat?.FindEnum(arg.Enum) : null;
        var name = e?.ValueName(value);
        if (name == null || !IsIdentifier(name) || e.Values.Count(v => v.Value == name) != 1 || IsReserved(name))
            return null;
        return name;
    }

    private static bool IsReserved(string name) =>
        name is "CallStatus" or "CallDone" or "op96" or "op9A" or "call" || ArgIndex(name) != null || RegisterIndex(name) != null || name.StartsWith("f") && long.TryParse(name[1..], out _);

    private static int? ArgIndex(string name) => name.StartsWith("arg") && int.TryParse(name[3..], NumberStyles.None, CultureInfo.InvariantCulture, out var i) ? i : null;
    private static int? RegisterIndex(string name) => name.Length == 2 && name[0] == 'r' && name[1] is >= '0' and <= '7' ? name[1] - '0' : null;

    // --- format ---

    private static int Precedence(byte op) => op switch
    {
        0x8E or 0x8F => 5,
        0x8C or 0x8D => 4,
        >= 0x90 and <= 0x96 => 3,
        0x98 => 2,
        0x99 => 1,
        _ => 0
    };

    /// <summary>The expression as editable text (markers left out)</summary>
    public static string Format(EsdExprNode n, EsdKindCatalog cat)
    {
        var sb = new StringBuilder();
        Write(sb, n, cat, null);
        return sb.ToString();
    }

    private static void Write(StringBuilder sb, EsdExprNode n, EsdKindCatalog cat, EsdArgDef argDef)
    {
        n = EsdExpr.Strip(n);
        switch (n.Kind)
        {
            case EsdExprKind.Int:
                sb.Append(EnumName(cat, argDef, n.IntValue) ?? n.IntValue.ToString(CultureInfo.InvariantCulture));
                return;
            case EsdExprKind.Float:
                sb.Append(Real(((float)n.FloatValue).ToString("R", CultureInfo.InvariantCulture))).Append('f');
                return;
            case EsdExprKind.Double:
                sb.Append(Real(n.FloatValue.ToString("R", CultureInfo.InvariantCulture)));
                return;
            case EsdExprKind.String:
                sb.Append('"');
                foreach (var ch in n.StringValue ?? "")
                    sb.Append(ch switch { '"' => "\\\"", '\\' => "\\\\", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t", _ => ch.ToString() });
                sb.Append('"');
                return;
            case EsdExprKind.Leaf:
                sb.Append(n.Op switch
                {
                    >= 0xAF and <= 0xB6 => $"r{n.Op - 0xAF}",
                    0xB9 => "CallStatus",
                    _ => "CallDone"
                });
                return;
            case EsdExprKind.Call:
                {
                    var id = n.FunctionId;
                    var def = id != null ? cat?.Function(id.Value) : null;
                    if (id != null)
                        sb.Append(FunctionName(cat, id.Value));
                    else
                    {
                        sb.Append("call[");
                        Write(sb, n.Children[0], cat, null);
                        sb.Append(']');
                    }
                    sb.Append('(');
                    for (int k = 1; k < n.Children.Count; k++)
                    {
                        if (k > 1) sb.Append(", ");
                        Write(sb, n.Children[k], cat, def != null && k - 1 < def.Args.Count ? def.Args[k - 1] : null);
                    }
                    sb.Append(')');
                    return;
                }
            case EsdExprKind.Binary:
                {
                    var p = Precedence(n.Op);
                    var l = EsdExpr.Strip(n.Children[0]);
                    var r = EsdExpr.Strip(n.Children[1]);
                    bool lp = l.Kind == EsdExprKind.Binary && Precedence(l.Op) < p;
                    bool rp = r.Kind == EsdExprKind.Binary && Precedence(r.Op) <= p;
                    if (lp) sb.Append('(');
                    Write(sb, l, cat, null);
                    if (lp) sb.Append(')');
                    sb.Append(' ').Append(EsdExpr.BinaryName(n.Op)).Append(' ');
                    if (rp) sb.Append('(');
                    Write(sb, r, cat, null);
                    if (rp) sb.Append(')');
                    return;
                }
            case EsdExprKind.Unary:
                switch (n.Op)
                {
                    case >= 0xA7 and <= 0xAE:
                        sb.Append($"(r{n.Op - 0xA7} = ");
                        Write(sb, n.Children[0], cat, null);
                        sb.Append(')');
                        return;
                    case 0xB8 when EsdExpr.Strip(n.Children[0]) is { Kind: EsdExprKind.Int } i && i.IntValue >= 0:
                        sb.Append("arg").Append(i.IntValue);
                        return;
                    case 0xB8:
                        sb.Append("arg[");
                        Write(sb, n.Children[0], cat, null);
                        sb.Append(']');
                        return;
                    default:
                        sb.Append($"op{n.Op:X2}(");
                        Write(sb, n.Children[0], cat, null);
                        sb.Append(')');
                        return;
                }
        }
    }

    /// <summary>A real number's digits, with a point when they'd read as an int</summary>
    private static string Real(string s) => s.Contains('.') || s.Contains('E') || s.Contains('e') || s.Contains("Infinity") || s.Contains("NaN") ? s : s + ".0";

    // --- parse ---

    private enum Tok { Number, Float, Double, String, Name, Op, LParen, RParen, LBracket, RBracket, Comma, End }

    private record struct Token(Tok Kind, string Text, int Pos, long Int = 0, double Real = 0);

    private static readonly string[] Ops = { "&&", "||", "<=", ">=", "==", "!=", "<", ">", "+", "-", "*", "/", "=" };

    private static List<Token> Lex(string text, out EsdParseError error)
    {
        error = null;
        var list = new List<Token>();
        int i = 0;
        while (i < text.Length)
        {
            var ch = text[i];
            if (char.IsWhiteSpace(ch)) { i++; continue; }
            int start = i;

            if (char.IsDigit(ch) || ch == '.' && i + 1 < text.Length && char.IsDigit(text[i + 1]))
            {
                if (ch == '0' && i + 1 < text.Length && text[i + 1] is 'x' or 'X')
                {
                    i += 2;
                    while (i < text.Length && Uri.IsHexDigit(text[i])) i++;
                    if (!long.TryParse(text[(start + 2)..i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) || i == start + 2)
                    {
                        error = new("ESD_Text_Bad_Number", start);
                        return null;
                    }
                    // 0xFFFFFFFF reads as the s32 -1
                    if (hex > int.MaxValue && hex <= uint.MaxValue) hex = unchecked((int)(uint)hex);
                    list.Add(new Token(Tok.Number, text[start..i], start, hex));
                    continue;
                }

                bool real = false;
                while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.')) { if (text[i] == '.') real = true; i++; }
                if (i < text.Length && text[i] is 'e' or 'E')
                {
                    real = true;
                    i++;
                    if (i < text.Length && text[i] is '+' or '-') i++;
                    while (i < text.Length && char.IsDigit(text[i])) i++;
                }
                var digits = text[start..i];
                if (i < text.Length && text[i] is 'f' or 'F')
                {
                    i++;
                    if (!double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) { error = new("ESD_Text_Bad_Number", start); return null; }
                    list.Add(new Token(Tok.Float, text[start..i], start, Real: f));
                }
                else if (real)
                {
                    if (!double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) { error = new("ESD_Text_Bad_Number", start); return null; }
                    list.Add(new Token(Tok.Double, digits, start, Real: d));
                }
                else
                {
                    if (!long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var v)) { error = new("ESD_Text_Bad_Number", start); return null; }
                    list.Add(new Token(Tok.Number, digits, start, v));
                }
                continue;
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                var word = text[start..i];
                list.Add(word == "op96" ? new Token(Tok.Op, word, start) : new Token(Tok.Name, word, start));
                continue;
            }

            if (ch == '"')
            {
                i++;
                var sb = new StringBuilder();
                while (i < text.Length && text[i] != '"')
                {
                    if (text[i] == '\\' && i + 1 < text.Length)
                    {
                        i++;
                        sb.Append(text[i] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', var c => c });
                    }
                    else sb.Append(text[i]);
                    i++;
                }
                if (i >= text.Length) { error = new("ESD_Text_Unclosed_String", start); return null; }
                i++;
                list.Add(new Token(Tok.String, sb.ToString(), start));
                continue;
            }

            var single = ch switch { '(' => Tok.LParen, ')' => Tok.RParen, '[' => Tok.LBracket, ']' => Tok.RBracket, ',' => Tok.Comma, _ => Tok.End };
            if (single != Tok.End)
            {
                list.Add(new Token(single, ch.ToString(), start));
                i++;
                continue;
            }

            var op = Ops.FirstOrDefault(o => string.CompareOrdinal(text, i, o, 0, o.Length) == 0);
            if (op == null) { error = new("ESD_Text_Bad_Char", start); return null; }
            list.Add(new Token(Tok.Op, op, start));
            i += op.Length;
        }
        list.Add(new Token(Tok.End, "", text.Length));
        return list;
    }

    private static readonly Dictionary<string, byte> BinaryOps = new()
    {
        ["+"] = 0x8C, ["-"] = 0x8D, ["*"] = 0x8E, ["/"] = 0x8F,
        ["<="] = 0x90, [">="] = 0x91, ["<"] = 0x92, [">"] = 0x93, ["=="] = 0x94, ["!="] = 0x95, ["op96"] = 0x96,
        ["&&"] = 0x98, ["||"] = 0x99,
    };

    /// <summary>Text into a tree (no markers, ints in their shortest form). Null with an error when it doesn't parse.</summary>
    public static EsdExprNode Parse(string text, EsdKindCatalog cat, out EsdParseError error)
    {
        var tokens = Lex(text ?? "", out error);
        if (tokens == null)
            return null;
        var p = new Parser { Tokens = tokens, Cat = cat };
        try
        {
            var node = p.Expression(1);
            if (p.Peek.Kind != Tok.End)
                throw new ParseException("ESD_Text_Unexpected", p.Peek.Pos);
            return node;
        }
        catch (ParseException e)
        {
            error = new EsdParseError(e.Message, e.Position);
            return null;
        }
    }

    /// <summary>Text into bytecode, or null with an error</summary>
    public static byte[] Compile(string text, EsdKindCatalog cat, out EsdParseError error)
    {
        var node = Parse(text, cat, out error);
        return node != null ? EsdExpr.Encode(node) : null;
    }

    private class ParseException : Exception
    {
        public int Position;
        public ParseException(string key, int pos) : base(key) { Position = pos; }
    }

    private class Parser
    {
        public List<Token> Tokens;
        public EsdKindCatalog Cat;
        private int At;

        public Token Peek => Tokens[At];
        private Token Next() => Tokens[At++];

        private Token Expect(Tok kind, string key)
        {
            if (Peek.Kind != kind)
                throw new ParseException(key, Peek.Pos);
            return Next();
        }

        /// <summary>Precedence climbing: operators of at least minPrec, left to right</summary>
        public EsdExprNode Expression(int minPrec, EsdArgDef argDef = null)
        {
            var left = Primary(argDef);
            while (Peek.Kind == Tok.Op && BinaryOps.TryGetValue(Peek.Text, out var op) && Precedence(op) >= minPrec)
            {
                Next();
                var right = Expression(Precedence(op) + 1);
                left = new EsdExprNode { Kind = EsdExprKind.Binary, Op = op, Children = { left, right } };
            }
            return left;
        }

        private EsdExprNode Primary(EsdArgDef argDef)
        {
            var t = Next();
            switch (t.Kind)
            {
                case Tok.Number:
                    return Int(t.Int, t.Pos);
                case Tok.Float:
                    return new EsdExprNode { Kind = EsdExprKind.Float, Op = 0x80, FloatValue = (float)t.Real };
                case Tok.Double:
                    return new EsdExprNode { Kind = EsdExprKind.Double, Op = 0x81, FloatValue = t.Real };
                case Tok.String:
                    return new EsdExprNode { Kind = EsdExprKind.String, Op = 0xA5, StringValue = t.Text };
                case Tok.Op when t.Text == "-":
                    {
                        // Only a literal can be negative: ESD has no negation op
                        var n = Next();
                        return n.Kind switch
                        {
                            Tok.Number => Int(-n.Int, n.Pos),
                            Tok.Float => new EsdExprNode { Kind = EsdExprKind.Float, Op = 0x80, FloatValue = (float)-n.Real },
                            Tok.Double => new EsdExprNode { Kind = EsdExprKind.Double, Op = 0x81, FloatValue = -n.Real },
                            _ => throw new ParseException("ESD_Text_No_Negation", t.Pos)
                        };
                    }
                case Tok.LParen:
                    {
                        // (rN = expr) stores into a register
                        if (Peek.Kind == Tok.Name && RegisterIndex(Peek.Text) is int reg && Tokens[At + 1].Kind == Tok.Op && Tokens[At + 1].Text == "=")
                        {
                            At += 2;
                            var value = Expression(1);
                            Expect(Tok.RParen, "ESD_Text_Expected_Close");
                            return new EsdExprNode { Kind = EsdExprKind.Unary, Op = (byte)(0xA7 + reg), Children = { value } };
                        }
                        var inner = Expression(1, argDef);
                        Expect(Tok.RParen, "ESD_Text_Expected_Close");
                        return inner;
                    }
                case Tok.Name:
                    return Name(t, argDef);
            }
            throw new ParseException(t.Kind == Tok.End ? "ESD_Text_Unexpected_End" : "ESD_Text_Unexpected", t.Pos);
        }

        private static EsdExprNode Int(long v, int pos)
        {
            if (v < int.MinValue || v > int.MaxValue)
                throw new ParseException("ESD_Text_Int_Range", pos);
            return EsdExprNode.Integer(v);
        }

        private EsdExprNode Name(Token t, EsdArgDef argDef)
        {
            var word = t.Text;

            if (Peek.Kind == Tok.LParen || word == "call" && Peek.Kind == Tok.LBracket)
                return Call(t);

            if (word == "CallStatus") return new EsdExprNode { Kind = EsdExprKind.Leaf, Op = 0xB9 };
            if (word == "CallDone") return new EsdExprNode { Kind = EsdExprKind.Leaf, Op = 0xBA };
            if (RegisterIndex(word) is int r) return new EsdExprNode { Kind = EsdExprKind.Leaf, Op = (byte)(0xAF + r) };

            if (word == "arg" && Peek.Kind == Tok.LBracket)
            {
                Next();
                var index = Expression(1);
                Expect(Tok.RBracket, "ESD_Text_Expected_Bracket");
                return new EsdExprNode { Kind = EsdExprKind.Unary, Op = 0xB8, Children = { index } };
            }
            if (ArgIndex(word) is int a)
                return new EsdExprNode { Kind = EsdExprKind.Unary, Op = 0xB8, Children = { EsdExprNode.Integer(a) } };

            // An enum value's name, in an enum arg's place
            var e = argDef?.Enum != null ? Cat?.FindEnum(argDef.Enum) : null;
            if (e != null)
            {
                var matches = e.Values.Where(v => v.Value == word).ToList();
                if (matches.Count == 1)
                    return Int(matches[0].Key, t.Pos);
            }

            throw new ParseException("ESD_Text_Unknown_Name", t.Pos);
        }

        private EsdExprNode Call(Token t)
        {
            var word = t.Text;
            EsdExprNode id;
            EsdCallDef def = null;

            if (word == "call")
            {
                Expect(Tok.LBracket, "ESD_Text_Expected_Bracket");
                id = Expression(1);
                Expect(Tok.RBracket, "ESD_Text_Expected_Bracket");
            }
            else if (word is "op9A")
            {
                Expect(Tok.LParen, "ESD_Text_Expected_Open");
                var operand = Expression(1);
                Expect(Tok.RParen, "ESD_Text_Expected_Close");
                return new EsdExprNode { Kind = EsdExprKind.Unary, Op = 0x9A, Children = { operand } };
            }
            else if (word.Length > 1 && word[0] == 'f' && long.TryParse(word[1..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var fid))
            {
                id = Int(fid, t.Pos);
                def = Cat?.Function(fid);
            }
            else if (Cat != null && NamesOf(Cat).Functions.TryGetValue(word, out var named))
            {
                id = EsdExprNode.Integer(named);
                def = Cat.Function(named);
            }
            else
                throw new ParseException(Cat != null && NamesOf(Cat).Ambiguous.Contains(word) ? "ESD_Text_Ambiguous_Name" : "ESD_Text_Unknown_Function", t.Pos);

            Expect(Tok.LParen, "ESD_Text_Expected_Open");
            var call = new EsdExprNode { Kind = EsdExprKind.Call, Children = { id } };
            if (Peek.Kind != Tok.RParen)
            {
                while (true)
                {
                    var k = call.Children.Count - 1;
                    call.Children.Add(Expression(1, def != null && k < def.Args.Count ? def.Args[k] : null));
                    if (Peek.Kind == Tok.Comma) { Next(); continue; }
                    break;
                }
            }
            Expect(Tok.RParen, "ESD_Text_Expected_Close");

            var n = call.Children.Count - 1;
            if (n > 6)
                throw new ParseException("ESD_Text_Too_Many_Args", t.Pos);
            call.Op = (byte)(0x84 + n);
            return call;
        }
    }

    // --- merge ---

    /// <summary>
    /// The new tree, with the old tree's markers and literal encodings kept wherever the two match (same op, same
    /// literal value, same shape): an unchanged expression encodes to its old bytes, and an edit changes only the
    /// part it touches.
    /// </summary>
    public static EsdExprNode Merge(EsdExprNode old, EsdExprNode updated)
    {
        if (old == null)
            return updated;

        // The old node's marker chain, outermost first
        var markers = new List<EsdExprNode>();
        var o = old;
        while (o.Kind == EsdExprKind.Unary && o.Op is 0xA6 or 0xB7)
        {
            markers.Add(o);
            o = o.Children[0];
        }

        var u = EsdExpr.Strip(updated);
        if (!SameHead(o, u))
            return updated;

        var merged = new EsdExprNode { Kind = u.Kind, Op = u.Op, IntValue = u.IntValue, FloatValue = u.FloatValue, StringValue = u.StringValue };
        if (u.Kind == EsdExprKind.Int)
            merged.Op = o.Op;   // the same value: keep its old encoding
        for (int i = 0; i < u.Children.Count; i++)
            merged.Children.Add(Merge(o.Children[i], u.Children[i]));

        for (int i = markers.Count - 1; i >= 0; i--)
            merged = new EsdExprNode { Kind = EsdExprKind.Unary, Op = markers[i].Op, Children = { merged } };
        return merged;
    }

    private static bool SameHead(EsdExprNode a, EsdExprNode b)
    {
        if (a.Kind != b.Kind || a.Children.Count != b.Children.Count)
            return false;
        return a.Kind switch
        {
            EsdExprKind.Int => a.IntValue == b.IntValue,
            EsdExprKind.Float => (float)a.FloatValue == (float)b.FloatValue,
            EsdExprKind.Double => a.FloatValue.Equals(b.FloatValue),
            EsdExprKind.String => a.StringValue == b.StringValue,
            _ => a.Op == b.Op
        };
    }

    /// <summary>
    /// New text for an existing expression: parsed, then merged with the old bytes. Null with an error when the text
    /// doesn't parse.
    /// </summary>
    public static byte[] Recompile(byte[] oldCode, string text, EsdKindCatalog cat, out EsdParseError error)
    {
        var node = Parse(text, cat, out error);
        if (node == null)
            return null;
        return EsdExpr.Encode(Merge(EsdExpr.Decode(oldCode), node));
    }

    // --- palette edits ---

    /// <summary>
    /// The expression with the node at a path replaced (the palette's drop on a chip); the rest keeps its bytes. An
    /// empty path replaces the whole expression. Null if the path doesn't exist.
    /// </summary>
    public static byte[] ReplaceAt(byte[] code, IReadOnlyList<int> path, EsdExprNode node)
    {
        if (path.Count == 0)
            return EsdExpr.Encode(node);

        var root = EsdExpr.Decode(code);
        var parent = root != null ? EsdEdits.At(root, path.Take(path.Count - 1).ToList()) : null;
        var i = path[^1];
        if (parent == null || i < 0 || i >= parent.Children.Count)
            return null;

        parent.Children[i] = node;
        return EsdExpr.Encode(root);
    }

    /// <summary>
    /// The right-hand default of an operator: the value that leaves the left side's meaning unchanged where there is
    /// one (x &amp;&amp; 1, x || 0, x + 0, x - 0, x * 1, x / 1); 0 for a comparison
    /// </summary>
    public static long WrapDefault(byte op) => op is 0x98 or 0x8E or 0x8F ? 1 : 0;

    /// <summary>A binary op with a node on the left and its default on the right</summary>
    public static EsdExprNode Wrap(EsdExprNode left, byte op) => new()
    {
        Kind = EsdExprKind.Binary,
        Op = op,
        Children = { left, EsdExprNode.Integer(WrapDefault(op)) }
    };

    /// <summary>A call of a function with the default value for each documented arg (0, 0.0f, "")</summary>
    public static EsdExprNode NewCall(EsdCallDef def, long id)
    {
        var call = new EsdExprNode { Kind = EsdExprKind.Call, Children = { EsdExprNode.Integer(id) } };
        foreach (var a in def?.Args.Where(a => !a.Optional).Take(6) ?? Enumerable.Empty<EsdArgDef>())
            call.Children.Add(EsdExpr.Decode(EsdEdits.DefaultArg(a.Type)));
        call.Op = (byte)(0x84 + call.Children.Count - 1);
        return call;
    }

    /// <summary>A tree without markers, ints in their shortest form: what two expressions mean, to compare them</summary>
    public static byte[] Canonical(EsdExprNode n)
    {
        EsdExprNode Clean(EsdExprNode x)
        {
            x = EsdExpr.Strip(x);
            if (x.Kind == EsdExprKind.Int)
                return EsdExprNode.Integer(x.IntValue);
            var c = new EsdExprNode { Kind = x.Kind, Op = x.Op, IntValue = x.IntValue, FloatValue = x.FloatValue, StringValue = x.StringValue };
            foreach (var ch in x.Children)
                c.Children.Add(Clean(ch));
            return c;
        }
        return EsdExpr.Encode(Clean(n));
    }
}
