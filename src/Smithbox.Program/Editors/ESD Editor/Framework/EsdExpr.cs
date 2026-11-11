using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Kinds of node in a decoded ESD expression.
/// </summary>
public enum EsdExprKind
{
    /// Integer literal: one byte (0x00-0x7F, value - 64) or 0x82 + s32
    Int,
    /// 0x80 + f32
    Float,
    /// 0x81 + f64
    Double,
    /// 0xA5 + UTF-16, 0-terminated
    String,
    /// 0x84-0x8A: call with 0-6 args; Children[0] is the function id, the rest are the args
    Call,
    /// 0x8C-0x96, 0x98, 0x99: Children[0] op Children[1]
    Binary,
    /// 0x9A, 0xA6, 0xA7-0xAE, 0xB7, 0xB8: an op on the value below it (Children[0])
    Unary,
    /// 0xAF-0xB6 (load register), 0xB9 / 0xBA (call status): pushes a value with no operand
    Leaf,
}

/// <summary>
/// One node of an ESD expression. Keeps the opcode it came from, so encoding gives the same bytes back.
/// </summary>
public class EsdExprNode
{
    public EsdExprKind Kind;
    public byte Op;
    public long IntValue;
    public double FloatValue;
    public string StringValue;
    public List<EsdExprNode> Children = new();

    public bool IsLiteral => Kind is EsdExprKind.Int or EsdExprKind.Float or EsdExprKind.Double or EsdExprKind.String;

    /// For a call with a literal function id: the id, else null.
    public long? FunctionId => Kind == EsdExprKind.Call && Children.Count > 0 && Children[0].Kind == EsdExprKind.Int ? Children[0].IntValue : null;
    public int CallArgCount => Kind == EsdExprKind.Call ? Children.Count - 1 : 0;

    public static EsdExprNode Integer(long value) => new()
    {
        Kind = EsdExprKind.Int,
        Op = value >= -64 && value <= 63 ? (byte)(value + 64) : (byte)0x82,
        IntValue = value
    };
}

/// <summary>
/// Decodes ESD expression bytecode into a node tree and encodes it back byte-for-byte.
/// </summary>
public static class EsdExpr
{
    public const byte End = 0xA1;

    /// Binary operators 0x8C-0x96 in order, then 0x98 / 0x99. 0x96's meaning is unconfirmed (seen comparing a machine
    /// call's status); the order follows ESDLang.
    static readonly string[] BinaryNames = { "+", "-", "*", "/", "<=", ">=", "<", ">", "==", "!=", "op96" };

    public static bool IsBinary(byte op) => op is >= 0x8C and <= 0x96 or 0x98 or 0x99;
    public static bool IsUnary(byte op) => op is 0x9A or 0xA6 or >= 0xA7 and <= 0xAE or 0xB7 or 0xB8;
    public static bool IsLeaf(byte op) => op is >= 0xAF and <= 0xB6 or 0xB9 or 0xBA;

    public static string BinaryName(byte op) => op switch
    {
        >= 0x8C and <= 0x96 => BinaryNames[op - 0x8C],
        0x98 => "&&",
        0x99 => "||",
        _ => $"op{op:X2}"
    };

    /// <summary>
    /// Decodes a whole expression (ending with 0xA1). Returns null when the bytes don't form exactly one value
    /// followed by the end op, with nothing after it.
    /// </summary>
    public static EsdExprNode Decode(byte[] code)
    {
        if (code == null) return null;
        var stack = new List<EsdExprNode>();
        int i = 0;
        while (i < code.Length)
        {
            byte b = code[i++];
            if (b < 0x80) { stack.Add(new EsdExprNode { Kind = EsdExprKind.Int, Op = b, IntValue = b - 64 }); continue; }
            switch (b)
            {
                case 0x80:
                    if (i + 4 > code.Length) return null;
                    stack.Add(new EsdExprNode { Kind = EsdExprKind.Float, Op = b, FloatValue = BitConverter.ToSingle(code, i) });
                    i += 4;
                    break;
                case 0x81:
                    if (i + 8 > code.Length) return null;
                    stack.Add(new EsdExprNode { Kind = EsdExprKind.Double, Op = b, FloatValue = BitConverter.ToDouble(code, i) });
                    i += 8;
                    break;
                case 0x82:
                    if (i + 4 > code.Length) return null;
                    stack.Add(new EsdExprNode { Kind = EsdExprKind.Int, Op = b, IntValue = BitConverter.ToInt32(code, i) });
                    i += 4;
                    break;
                case 0xA5:
                    {
                        int start = i;
                        while (i + 1 < code.Length && (code[i] != 0 || code[i + 1] != 0)) i += 2;
                        if (i + 1 >= code.Length) return null;
                        stack.Add(new EsdExprNode { Kind = EsdExprKind.String, Op = b, StringValue = Encoding.Unicode.GetString(code, start, i - start) });
                        i += 2;
                        break;
                    }
                case >= 0x84 and <= 0x8A:
                    {
                        int n = b - 0x84;
                        if (stack.Count < n + 1) return null;
                        var call = new EsdExprNode { Kind = EsdExprKind.Call, Op = b };
                        call.Children.AddRange(stack.GetRange(stack.Count - n - 1, n + 1));
                        stack.RemoveRange(stack.Count - n - 1, n + 1);
                        stack.Add(call);
                        break;
                    }
                case End:
                    return stack.Count == 1 && i == code.Length ? stack[0] : null;
                default:
                    if (IsBinary(b))
                    {
                        if (stack.Count < 2) return null;
                        var node = new EsdExprNode { Kind = EsdExprKind.Binary, Op = b };
                        node.Children.Add(stack[^2]);
                        node.Children.Add(stack[^1]);
                        stack.RemoveRange(stack.Count - 2, 2);
                        stack.Add(node);
                    }
                    else if (IsUnary(b))
                    {
                        if (stack.Count < 1) return null;
                        var node = new EsdExprNode { Kind = EsdExprKind.Unary, Op = b };
                        node.Children.Add(stack[^1]);
                        stack[^1] = node;
                    }
                    else if (IsLeaf(b))
                    {
                        stack.Add(new EsdExprNode { Kind = EsdExprKind.Leaf, Op = b });
                    }
                    else return null;
                    break;
            }
        }
        return null;
    }

    /// <summary>
    /// Encodes a node tree back to bytecode, ending with 0xA1.
    /// </summary>
    public static byte[] Encode(EsdExprNode root)
    {
        var bytes = new List<byte>();
        Write(root, bytes);
        bytes.Add(End);
        return bytes.ToArray();
    }

    static void Write(EsdExprNode n, List<byte> o)
    {
        switch (n.Kind)
        {
            case EsdExprKind.Int:
                if (n.Op < 0x80) o.Add((byte)(n.IntValue + 64));
                else { o.Add(0x82); o.AddRange(BitConverter.GetBytes((int)n.IntValue)); }
                return;
            case EsdExprKind.Float:
                o.Add(0x80); o.AddRange(BitConverter.GetBytes((float)n.FloatValue));
                return;
            case EsdExprKind.Double:
                o.Add(0x81); o.AddRange(BitConverter.GetBytes(n.FloatValue));
                return;
            case EsdExprKind.String:
                o.Add(0xA5); o.AddRange(Encoding.Unicode.GetBytes(n.StringValue ?? "")); o.Add(0); o.Add(0);
                return;
            case EsdExprKind.Leaf:
                o.Add(n.Op);
                return;
            default:
                foreach (var c in n.Children) Write(c, o);
                o.Add(n.Op);
                return;
        }
    }

    /// <summary>
    /// Plain text for a node (no catalogue names): f15(10002855) != 1. Separators (0xA6) are left out.
    /// </summary>
    public static string Text(EsdExprNode n, Func<long, int, string> functionName = null)
    {
        if (n == null) return "?";
        switch (n.Kind)
        {
            case EsdExprKind.Int: return n.IntValue.ToString();
            case EsdExprKind.Float: return n.FloatValue.ToString("0.###") + "f";
            case EsdExprKind.Double: return n.FloatValue.ToString("0.###");
            case EsdExprKind.String: return "\"" + n.StringValue + "\"";
            case EsdExprKind.Leaf:
                return n.Op switch
                {
                    >= 0xAF and <= 0xB6 => $"r{n.Op - 0xAF}",
                    0xB9 => "CallStatus",
                    0xBA => "CallDone",
                    _ => $"op{n.Op:X2}"
                };
            case EsdExprKind.Call:
                {
                    var args = new List<string>();
                    for (int k = 1; k < n.Children.Count; k++) args.Add(Text(n.Children[k], functionName));
                    var id = n.FunctionId;
                    var name = id != null ? functionName?.Invoke(id.Value, n.CallArgCount) ?? $"f{id}" : $"call[{Text(n.Children[0], functionName)}]";
                    return $"{name}({string.Join(", ", args)})";
                }
            case EsdExprKind.Binary:
                {
                    string Side(EsdExprNode c) { var t = Text(c, functionName); return Strip(c).Kind == EsdExprKind.Binary ? $"({t})" : t; }
                    return $"{Side(n.Children[0])} {BinaryName(n.Op)} {Side(n.Children[1])}";
                }
            case EsdExprKind.Unary:
                {
                    var inner = Text(n.Children[0], functionName);
                    return n.Op switch
                    {
                        0xA6 => inner,
                        >= 0xA7 and <= 0xAE => $"(r{n.Op - 0xA7} = {inner})",
                        0xB7 => inner,
                        0xB8 => $"arg[{inner}]",
                        0x9A => $"op9A({inner})",
                        _ => $"op{n.Op:X2}({inner})"
                    };
                }
        }
        return "?";
    }

    /// The node under any separator (0xA6) and short-circuit (0xB7) markers, which don't change the value.
    public static EsdExprNode Strip(EsdExprNode n)
    {
        while (n != null && n.Kind == EsdExprKind.Unary && n.Op is 0xA6 or 0xB7) n = n.Children[0];
        return n;
    }

    /// Every call in the tree, outermost first.
    public static IEnumerable<EsdExprNode> Calls(EsdExprNode n)
    {
        if (n == null) yield break;
        if (n.Kind == EsdExprKind.Call) yield return n;
        foreach (var c in n.Children)
            foreach (var x in Calls(c)) yield return x;
    }

    /// The value of a literal expression (an Int under at most separator markers), else null.
    public static long? Constant(EsdExprNode n)
    {
        n = Strip(n);
        return n != null && n.Kind == EsdExprKind.Int ? n.IntValue : null;
    }

    /// True when an evaluator is the constant 1 (the "always" transition).
    public static bool IsAlways(byte[] code) => code != null && code.Length == 2 && code[0] == 0x41 && code[1] == End;
}
