using StudioCore.EzStateEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static StudioCore.Editors.EsdEditor.EsdLang.AST;

namespace StudioCore.Editors.EsdEditor.EsdLang;

/// <summary>
/// Taken from EsdLang
/// Credit to thefifthmatt
/// </summary>
public static class EzInfixor
{
    private static Dictionary<string, int[]> Operators = new Dictionary<string, int[]>
    {
        { "^", new int[] { 5, 1 } },
        { "*", new int[] { 4, 0 } },
        { "/", new int[] { 4, 0 } },
        { "+", new int[] { 3, 0 } },
        { "-", new int[] { 3, 0 } },
        { "<", new int[] { 2, 0 } },
        { ">", new int[] { 2, 0 } },
        { "==", new int[] { 2, 0 } },
        { "!=", new int[] { 2, 0 } },
        { ">=", new int[] { 2, 0 } },
        { "<=", new int[] { 2, 0 } },
        { "&&", new int[] { 1, 0 } },
        { "||", new int[] { 1, 0 } },
    };

    private static string GetExpressionWithParenthesesIfContainsOperator(string exp, string op)
    {
        if (op == "&&" || op == "||")
        {
            if (exp.Contains("&&") || exp.Contains("||"))
                return $"({exp})";
            else
                return exp;
        }
        else
        {
            if (Operators.Any(x => exp.Contains(x.Key)) || exp.Contains(" "))
                return $"({exp})";
            else
                return exp;
        }

    }

    private static string GetFunctionInfo(EsdEditorScreen editor, int id)
    {
        if (editor.BaseEditor.ProjectManager.SelectedProject == null)
            return $"f{id}";

        var curProject = editor.BaseEditor.ProjectManager.SelectedProject;

        var functionMeta = curProject.EsdData.Meta.GetAllFunctionMeta();
        foreach(var entry in functionMeta)
        {
            if(entry.id == id)
            {
                return entry.name;
            }
        }

        return $"f{id}";
    }

    public static Expr BytecodeToInfix(EsdEditorScreen editor, byte[] Bytes, bool isBigEndian = false)
    {
        var bigEndianReverseBytes = Bytes.Reverse().ToArray();

        Stack<Expr> exprs = new Stack<Expr>();
        List<Expr> popArgs(int amount)
        {
            List<Expr> args = new List<Expr>();
            for (int i = 0; i < amount; i++)
            {
                args.Add(exprs.Pop());
            }
            args.Reverse();
            return args;
        }

        for (int i = 0; i < Bytes.Length; i++)
        {
            var b = Bytes[i];
            if (b >= 0 && b <= 0x7F)
            {
                exprs.Push(new ConstExpr { Value = (sbyte)(b - 64) });
            }
            else if (b == 0xA5)
            {
                int j = 0;
                while (Bytes[i + j + 1] != 0 || Bytes[i + j + 2] != 0)
                    j += 2;
                string text = isBigEndian ?
                    Encoding.BigEndianUnicode.GetString(Bytes, i + 1, j)
                    : Encoding.Unicode.GetString(Bytes, i + 1, j);

                if (text.Contains('"') || text.Contains('\r') || text.Contains('\n'))
                    throw new Exception("Illegal character in string literal");
                exprs.Push(new ConstExpr { Value = text });
                i += j + 2;
            }
            else if (b == 0x80)
            {
                float val;
                if (!isBigEndian)
                {
                    val = BitConverter.ToSingle(Bytes, i + 1);
                }
                else
                {
                    val = BitConverter.ToSingle(bigEndianReverseBytes, (bigEndianReverseBytes.Length - 1) - (i + 1) - 4);
                }
                exprs.Push(new ConstExpr { Value = val });

                i += 4;
            }
            else if (b == 0x81)
            {
                double val;
                if (!isBigEndian)
                {
                    val = BitConverter.ToDouble(Bytes, i + 1);
                }
                else
                {
                    val = BitConverter.ToDouble(bigEndianReverseBytes, (bigEndianReverseBytes.Length - 1) - (i + 1) - 8);
                }
                exprs.Push(new ConstExpr { Value = val });

                i += 8;
            }
            else if (b == 0x82)
            {
                int val;
                if (!isBigEndian)
                {
                    val = BitConverter.ToInt32(Bytes, i + 1);
                }
                else
                {
                    val = BitConverter.ToInt32(bigEndianReverseBytes, (bigEndianReverseBytes.Length - 1) - (i + 1) - 4);
                }
                exprs.Push(new ConstExpr { Value = val });

                i += 4;
            }
            else if (b == 0x84)
            {
                exprs.Push(new FunctionCall { Args = popArgs(0), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x85)
            {
                exprs.Push(new FunctionCall { Args = popArgs(1), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x86)
            {
                exprs.Push(new FunctionCall { Args = popArgs(2), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x87)
            {
                exprs.Push(new FunctionCall { Args = popArgs(3), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x88)
            {
                exprs.Push(new FunctionCall { Args = popArgs(4), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x89)
            {
                exprs.Push(new FunctionCall { Args = popArgs(5), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (b == 0x8A)
            {
                exprs.Push(new FunctionCall { Args = popArgs(6), Name = GetFunctionInfo(editor, exprs.Pop().AsInt()) });
            }
            else if (CommonDefinitions.OperatorsByByte.ContainsKey(b))
            {
                if (CommonDefinitions.OperatorsByByte[b] == "N")
                {
                    exprs.Push(new UnaryExpr { Op = "N", Arg = exprs.Pop() });
                }
                else
                {
                    exprs.Push(new BinaryExpr { Op = CommonDefinitions.OperatorsByByte[b], Rhs = exprs.Pop(), Lhs = exprs.Pop() });
                }
            }
            else if (b == 0xA6)
            {
                Expr top = exprs.Peek();
                top.IfFalse = FalseCond.CONTINUE;
            }
            else if (b >= 0xA7 && b <= 0xAE)
            {
                byte regIndex = (byte)(b - 0xA7);
                exprs.Push(new FunctionCall { Args = popArgs(1), Name = $"SetREG{regIndex}" });
            }
            else if (b >= 0xAF && b <= 0xB6)
            {
                byte regIndex = (byte)(b - 0xAF);
                exprs.Push(new FunctionCall { Args = popArgs(0), Name = $"GetREG{regIndex}" });
            }
            else if (b == 0xB7)
            {
                Expr top = exprs.Peek();
                top.IfFalse = FalseCond.ABORT;
            }
            else if (b == 0xB8)
            {
                // exprs.Push(new FunctionCall { Args = popArgs(1), Name = "StateGroupArg" });
                FunctionCall func = new FunctionCall { Args = popArgs(1), Name = "StateGroupArg" };
                ConstExpr ce = func.Args[0] as ConstExpr;
                // Console.WriteLine($"{ce} {ce.Value.GetType()}");
                exprs.Push(func);
            }
            else if (b == 0xB9)
            {
                exprs.Push(new CallResult());
            }
            else if (b == 0xBA)
            {
                // This opcode just returns a constant value 0x7FFFFFFF
                // But use higher-level representation of it
                exprs.Push(new CallOngoing());
            }
            else if (b == 0xA1)
            {
                //break;
            }
            else
            {
                exprs.Push(new Unknown { Opcode = b });
            }
        }
        if (exprs.Count != 1) throw new Exception("Could not parse expr. Remaining stack: " + string.Join("; ", exprs) + $"; = {string.Join(" ", Bytes.Select(x => x.ToString("X2")))}");
        return exprs.Pop();
    }
}
