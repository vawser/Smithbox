using System.Text;

namespace StudioCore.Editors.LuaEditor;

public enum LuaTokenKind
{
    Name,
    Keyword,
    Number,
    String,
    Comment,
    Symbol,
    EndOfFile,
    Error,
}

/// <summary>One token of Lua 5.0 source: its kind, text and place (line and column are 1-based)</summary>
public readonly record struct LuaToken(LuaTokenKind Kind, string Text, int Start, int Length, int Line, int Column)
{
    public bool Is(string symbolOrKeyword) =>
        (Kind == LuaTokenKind.Symbol || Kind == LuaTokenKind.Keyword) && Text == symbolOrKeyword;
}

/// <summary>A syntax problem in Lua source (line and column are 1-based)</summary>
public readonly record struct LuaProblem(int Line, int Column, string Message);

/// <summary>
/// Splits Lua 5.0 source into tokens: the language FromSoftware's AI scripts are written in (and the game's loader
/// parses when a binder holds text). Lua 5.0 has no '#', '%' or '::'/goto, and no '\z', hex or long-bracket levels
/// ('[==['): those are reported, so a newer-Lua habit fails here instead of in the game.
/// </summary>
public static class LuaLexer
{
    public static readonly HashSet<string> Keywords = new()
    {
        "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if", "in", "local", "nil", "not",
        "or", "repeat", "return", "then", "true", "until", "while",
    };

    // Longest first
    private static readonly string[] Symbols =
    {
        "...", "..", "==", "~=", "<=", ">=", "+", "-", "*", "/", "^", "=", "<", ">", "(", ")", "{", "}", "[", "]", ";",
        ":", ",", ".",
    };

    /// <summary>Tokens without comments (what the parser reads), ending with an EndOfFile token; problems are added to the list</summary>
    public static List<LuaToken> Tokenize(string src, List<LuaProblem> problems, bool keepComments = false)
    {
        var tokens = new List<LuaToken>();
        int i = 0, line = 1, lineStart = 0;

        void Problem(int at, string message) => problems?.Add(new LuaProblem(line, at - lineStart + 1, message));

        void Add(LuaTokenKind kind, int start, int startLine, int startCol)
        {
            tokens.Add(new LuaToken(kind, src.Substring(start, i - start), start, i - start, startLine, startCol));
        }

        while (i < src.Length)
        {
            var c = src[i];

            if (c == '\n')
            {
                i++;
                line++;
                lineStart = i;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            int start = i, startLine = line, startCol = i - lineStart + 1;

            // Comments: -- to the end of the line, or --[[ ... ]]
            if (c == '-' && Peek(src, i + 1) == '-')
            {
                i += 2;
                if (Peek(src, i) == '[' && Peek(src, i + 1) == '[')
                {
                    if (!SkipLongBracket(src, ref i, ref line, ref lineStart))
                        Problem(start, LOC.Get("LUA_Syntax_Unfinished_Long_Comment"));
                }
                else
                {
                    while (i < src.Length && src[i] != '\n')
                        i++;
                }

                if (keepComments)
                    Add(LuaTokenKind.Comment, start, startLine, startCol);
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                while (i < src.Length && (char.IsLetterOrDigit(src[i]) || src[i] == '_'))
                    i++;
                var text = src.Substring(start, i - start);
                tokens.Add(new LuaToken(Keywords.Contains(text) ? LuaTokenKind.Keyword : LuaTokenKind.Name, text, start, i - start, startLine, startCol));
                continue;
            }

            if (char.IsDigit(c) || c == '.' && char.IsDigit(Peek(src, i + 1)))
            {
                // Lua 5.0 numbers: digits, '.', exponent (no hex)
                while (i < src.Length && (char.IsLetterOrDigit(src[i]) || src[i] == '.' ||
                       (src[i] is '+' or '-') && (src[i - 1] is 'e' or 'E')))
                    i++;
                var text = src.Substring(start, i - start);
                if (!double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                    Problem(start, LOC.Get("LUA_Syntax_Bad_Number", text));
                Add(LuaTokenKind.Number, start, startLine, startCol);
                continue;
            }

            if (c == '"' || c == '\'')
            {
                i++;
                var closed = false;
                while (i < src.Length)
                {
                    var d = src[i];
                    if (d == '\\')
                    {
                        // An escaped newline continues the string
                        if (Peek(src, i + 1) == '\n')
                        {
                            i += 2;
                            line++;
                            lineStart = i;
                            continue;
                        }
                        i += 2;
                        continue;
                    }
                    if (d == '\n')
                        break;
                    i++;
                    if (d == c)
                    {
                        closed = true;
                        break;
                    }
                }
                if (!closed)
                    Problem(start, LOC.Get("LUA_Syntax_Unfinished_String"));
                Add(LuaTokenKind.String, start, startLine, startCol);
                continue;
            }

            if (c == '[' && Peek(src, i + 1) == '[')
            {
                if (!SkipLongBracket(src, ref i, ref line, ref lineStart))
                    Problem(start, LOC.Get("LUA_Syntax_Unfinished_Long_String"));
                tokens.Add(new LuaToken(LuaTokenKind.String, src.Substring(start, i - start), start, i - start, startLine, startCol));
                continue;
            }

            var symbol = Symbols.FirstOrDefault(s => string.CompareOrdinal(src, i, s, 0, s.Length) == 0);
            if (symbol != null)
            {
                i += symbol.Length;
                Add(LuaTokenKind.Symbol, start, startLine, startCol);
                continue;
            }

            // Not Lua 5.0
            i++;
            Problem(start, c switch
            {
                '#' => LOC.Get("LUA_Syntax_No_Length_Operator"),
                '%' => LOC.Get("LUA_Syntax_No_Modulo_Operator"),
                _ => LOC.Get("LUA_Syntax_Unexpected_Character", c),
            });
            Add(LuaTokenKind.Error, start, startLine, startCol);
        }

        tokens.Add(new LuaToken(LuaTokenKind.EndOfFile, "", src.Length, 0, line, i - lineStart + 1));
        return tokens;
    }

    private static char Peek(string s, int i) => i < s.Length ? s[i] : '\0';

    /// <summary>Skips [[ ... ]] (Lua 5.0 nests them); false if it doesn't close</summary>
    private static bool SkipLongBracket(string src, ref int i, ref int line, ref int lineStart)
    {
        i += 2;
        var depth = 1;
        while (i < src.Length)
        {
            if (src[i] == '\n')
            {
                i++;
                line++;
                lineStart = i;
                continue;
            }
            if (src[i] == '[' && Peek(src, i + 1) == '[')
            {
                depth++;
                i += 2;
                continue;
            }
            if (src[i] == ']' && Peek(src, i + 1) == ']')
            {
                i += 2;
                if (--depth == 0)
                    return true;
                continue;
            }
            i++;
        }
        return false;
    }

    /// <summary>The text encoding FromSoftware's Lua files use</summary>
    public static Encoding ShiftJis
    {
        get
        {
            if (_shiftJis == null)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Throws on characters Shift-JIS can't hold, instead of writing '?'
                _shiftJis = Encoding.GetEncoding("shift_jis", EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback);
            }
            return _shiftJis;
        }
    }

    private static Encoding _shiftJis;

    /// <summary>The first character Shift-JIS can't encode, and its place; null if all can</summary>
    public static (int Line, int Column, char Char)? FindUnencodable(string src)
    {
        var enc = ShiftJis;
        int line = 1, col = 1;
        for (int i = 0; i < src.Length; i++)
        {
            var c = src[i];
            if (c > 0x7F)
            {
                try
                {
                    enc.GetByteCount(char.IsHighSurrogate(c) && i + 1 < src.Length ? src.Substring(i, 2) : c.ToString());
                }
                catch (EncoderFallbackException)
                {
                    return (line, col, c);
                }
            }
            if (c == '\n')
            {
                line++;
                col = 1;
            }
            else
                col++;
        }
        return null;
    }
}
