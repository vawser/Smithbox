using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Passes;
using LuaDecompilerCore.Utilities;
using static LuaDecompilerCore.ILanguageDecompiler;

namespace LuaDecompilerCore.LanguageDecompilers;

/// <summary>
/// Language decompiler for the HavokScript variants used in the Souls games
/// </summary>
public class HksDecompiler : ILanguageDecompiler
{
    private enum LuaHksOps
    {
        OpGetField = 0,
        OpTest = 1,
        OpCallI = 2,
        OpCallC = 3,
        OpEq = 4,
        OpEqBk = 5,
        OpGetGlobal = 6,
        OpMove = 7,
        OpSelf = 8,
        OpReturn = 9,
        OpGetTableS = 10,
        OpGetTableN = 11,
        OpGetTable = 12,
        OpLoadBool = 13,
        OpTForLoop = 14,
        OpSetField = 15,
        OpSetTableS = 16,
        OpSetTableSBK = 17,
        OpSetTableN = 18,
        OpSetTableNBK = 19,
        OpSetTable = 20,
        OpSetTableBK = 21,
        OpTailCallI = 22,
        OpTailCallC = 23,
        OpTailCallM = 24,
        OpLoadK = 25,
        OpLoadNil = 26,
        OpSetGlobal = 27,
        OpJmp = 28,
        OpCallM = 29,
        OpCall = 30,
        OpIntrinsicIndex = 31,
        OpIntrinsicNewIndex = 32,
        OpIntrinsicSelf = 33,
        OpIntrinsicIndexLiteral = 34,
        OpIntrinsicNewIndexLiteral = 35,
        OpIntrinsicSelfLiteral = 36,
        OpTailCall = 37,
        OpGetUpVal = 38,
        OpSetUpVal = 39,
        OpAdd = 40,
        OpAddBk = 41,
        OpSub = 42,
        OpSubBk = 43,
        OpMul = 44,
        OpMulBk = 45,
        OpDiv = 46,
        OpDivBk = 47,
        OpMod = 48,
        OpModBk = 49,
        OpPow = 50,
        OpPowBk = 51,
        OpNewTable = 52,
        OpUnm = 53,
        OpNot = 54,
        OpLen = 55,
        OpLt = 56,
        OpLtBk = 57,
        OpLe = 58,
        OpLeBk = 59,
        OpConcat = 60,
        OpTestSet = 61,
        OpForPrep = 62,
        OpForLoop = 63,
        OpSetList = 64,
        OpClose = 65,
        OpClosure = 66,
        OpVarArg = 67,
        OpTailCallIR1 = 68,
        OpCallIR1 = 69,
        OpSetUpValR1 = 70,
        OpTestR1 = 71,
        OpNotR1 = 72,
        OpGetFieldR1 = 73,
        OpSetFieldR1 = 74,
        OpNewStruct = 75,
        OpData = 76,
        OpSetSlotN = 77,
        OpSetSlotI = 78,
        OpSetSlot = 79,
        OpSetSlotS = 80,
        OpSetSlotMT = 81,
        OpCheckType = 82,
        OpCheckTypeS = 83,
        OpGetSlot = 84,
        OpGetSlotMT = 85,
        OpSelfSlot = 86,
        OpSelfSlotMT = 87,
        OpGetFieldMM = 88,
        OpCheckTypeD = 89,
        OpGetSlotD = 90,
        OpGetGlobalMem = 91,
    }

    private static readonly OpProperties[] OpProperties =
    {
        new OpProperties("GETFIELD", OpMode.IABC),
        new OpProperties("TEST", OpMode.IABC),
        new OpProperties("CALL_I", OpMode.IABC),
        new OpProperties("CALL_C"),
        new OpProperties("EQ", OpMode.IABC),
        new OpProperties("EQ_BK"),
        new OpProperties("GETGLOBAL"),
        new OpProperties("MOVE", OpMode.IABC),
        new OpProperties("SELF", OpMode.IABC),
        new OpProperties("RETURN", OpMode.IABC),
        new OpProperties("GETTABLE_S", OpMode.IABC),
        new OpProperties("GETTABLE_N"),
        new OpProperties("GETTABLE"),
        new OpProperties("LOADBOOL", OpMode.IABC),
        new OpProperties("TFORLOOP", OpMode.IABC),
        new OpProperties("SETFIELD", OpMode.IABC),
        new OpProperties("SETTABLE_S", OpMode.IABC),
        new OpProperties("SETTABLE_S_BK", OpMode.IABC),
        new OpProperties("SETTABLE_N"),
        new OpProperties("SETTABLE_N_BK"),
        new OpProperties("SETTABLE", OpMode.IABC),
        new OpProperties("SETTABLE_BK"),
        new OpProperties("TAILCALL_I", OpMode.IABC),
        new OpProperties("TAILCALL_C"),
        new OpProperties("TAILCALL_M"),
        new OpProperties("LOADK", OpMode.IABx),
        new OpProperties("LOADNIL", OpMode.IABC),
        new OpProperties("SETGLOBAL", OpMode.IABx),
        new OpProperties("JMP", OpMode.IAsBx),
        new OpProperties("CALL_M"),
        new OpProperties("CALL"),
        new OpProperties("INTRINSIC_INDEX"),
        new OpProperties("INTRINSIC_NEWINDEX"),
        new OpProperties("INTRINSIC_SELF"),
        new OpProperties("INTRINSIC_INDEX_LITERAL"),
        new OpProperties("INTRINSIC_NEWINDEX_LITERAL"),
        new OpProperties("INTRINSIC_SELF_LITERAL"),
        new OpProperties("TAILCALL"),
        new OpProperties("GETUPVAL", OpMode.IABC),
        new OpProperties("SETUPVAL", OpMode.IABC),
        new OpProperties("ADD", OpMode.IABC),
        new OpProperties("ADD_BK", OpMode.IABC),
        new OpProperties("SUB", OpMode.IABC),
        new OpProperties("SUB_BK", OpMode.IABC),
        new OpProperties("MUL", OpMode.IABC),
        new OpProperties("MUL_BK", OpMode.IABC),
        new OpProperties("DIV", OpMode.IABC),
        new OpProperties("DIV_BK", OpMode.IABC),
        new OpProperties("MOD", OpMode.IABC),
        new OpProperties("MOD_BK", OpMode.IABC),
        new OpProperties("POW", OpMode.IABC),
        new OpProperties("POW_BK", OpMode.IABC),
        new OpProperties("NEWTABLE", OpMode.IABC),
        new OpProperties("UNM", OpMode.IABC),
        new OpProperties("NOT", OpMode.IABC),
        new OpProperties("LEN", OpMode.IABC),
        new OpProperties("LT", OpMode.IABC),
        new OpProperties("LT_BK", OpMode.IABC),
        new OpProperties("LE", OpMode.IABC),
        new OpProperties("LE_BK", OpMode.IABC),
        new OpProperties("CONCAT", OpMode.IABC),
        new OpProperties("TESTSET"),
        new OpProperties("FORPREP", OpMode.IAsBx),
        new OpProperties("FORLOOP", OpMode.IAsBx),
        new OpProperties("SETLIST", OpMode.IABC),
        new OpProperties("CLOSE"),
        new OpProperties("CLOSURE", OpMode.IABx),
        new OpProperties("VARARG", OpMode.IABC),
        new OpProperties("TAILCALL_I_R1"),
        new OpProperties("CALL_I_R1", OpMode.IABC),
        new OpProperties("SETUPVAL_R1", OpMode.IABC),
        new OpProperties("TEST_R1", OpMode.IABC),
        new OpProperties("NOT_R1"),
        new OpProperties("GETFIELD_R1", OpMode.IABC),
        new OpProperties("SETFIELD_R1", OpMode.IABC),
        new OpProperties("NEWSTRUCT"),
        new OpProperties("DATA", OpMode.IABx),
        new OpProperties("SETSLOTN"),
        new OpProperties("SETSLOTI"),
        new OpProperties("SETSLOT"),
        new OpProperties("SETSLOTS"),
        new OpProperties("SETSLOTMT"),
        new OpProperties("CHECKTYPE"),
        new OpProperties("CHECKTYPES"),
        new OpProperties("GETSLOT"),
        new OpProperties("GETSLOTMT"),
        new OpProperties("SELFSLOT"),
        new OpProperties("SELFSLOTMT"),
        new OpProperties("GETFIELD_MM"),
        new OpProperties("CHECKTYPE_D"),
        new OpProperties("GETSLOT_D"),
        new OpProperties("GETGLOBAL_MEM", OpMode.IABx),
    };

    private static IdentifierReference Register(Function function, uint reg)
    {
        return new IdentifierReference(function.GetRegister(reg));
    }

    private static Constant ToConstantIr(LuaFile.Constant con, int id)
    {
        return con.Type switch
        {
            LuaFile.Constant.ConstantType.TypeNumber => new Constant(con.NumberValue, id),
            LuaFile.Constant.ConstantType.TypeString => new Constant(con.StringValue ?? "", id),
            LuaFile.Constant.ConstantType.TypeBoolean => new Constant(con.BoolValue, id),
            _ => new Constant(Constant.ConstantType.ConstNil, id)
        };
    }

    private static string RkHks(LuaFile.Function function, int val, bool sZero)
    {
        if (val >= 0 && !sZero)
        {
            return $"R({val})";
        }
        return sZero ? function.Constants[val].ToString() : function.Constants[-val].ToString();
    }
    
    private static Expression RkIrHks(Function irFunction, LuaFile.Function function, int val, bool sZero)
    {
        if (val >= 0 && !sZero)
        {
            return new IdentifierReference(irFunction.GetRegister((uint)val));
        }

        return sZero ? ToConstantIr(function.Constants[val], val) : ToConstantIr(function.Constants[-val], -val);
    }

    private static void CheckLocal(Assignment a, LuaFile.Function function, int index)
    {
        a.LocalAssignments = function.LocalsAt(index + 1);
    }

    private static void CheckLocal(Data d, LuaFile.Function function, int index)
    {
        d.Locals = function.LocalsAt(index + 1);
    }

    private static uint JumpOffset(int i, int sbx)
    {
        var addr = (uint)(i / 4 + 2 + ((sbx << 16) >> 16));
        if ((sbx & 0x10000) != 0)
        {
            // Unsigned address?
            addr = (uint)((sbx & 0xFFFF) + 2 + (uint)(i / 4));
        }

        return addr;
    }

    public void InitializeFunction(LuaFile.Function function, Function irFunction)
    {
        //var debugCounter = Identifier.GetGlobalTable();
        //irFunction.UpValueBindings.Add(debugCounter);

        // Register closures for all the children
        foreach (var t in function.ChildFunctions)
        {
            var childFunction = new Function(t.FunctionId)
            {
                // UpValue count needs to be set for child functions for analysis to be correct
                UpValueCount = t.UpValuesCount
            };
            irFunction.AddClosure(childFunction);
        }
        
        // Local variable names if they exist in the debug information
        irFunction.ArgumentNames = function.LocalsAt(0);
    }

    public string Disassemble(LuaFile.Function function)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Constants:");
        for (var i = 0; i < function.Constants.Length; i++)
        {
            builder.AppendLine($"{i}: {function.Constants[i]}");
        }

        builder.AppendLine();
        var br = new BinaryReaderEx(true, function.Bytecode);
        for (var i = 0; i < function.Bytecode.Length; i += 4)
        {
            var instruction = br.ReadUInt32();
            var opcode = (instruction & 0xFF000000) >> 25;
            var a = instruction & 0xFF;
            var c = (int)(instruction & 0x1FF00) >> 8;
            var b = (int)(instruction & 0x1FE0000) >> 17;
            var sZero = false;
            if ((b & 0x100) > 0)
            {
                b = -(b & 0xFF);
            }
            if ((c & 0x100) > 0)
            {
                if (c == 0x100)
                {
                    sZero = true;
                }

                c = -(c & 0xFF);
            }
            var bx = (instruction & 0x1FFFF00) >> 8;
            var sbx = (int)bx;

            builder.Append($"{i / 4:D4}: ");
            
            switch (OpProperties[opcode].OpMode)
            {
                case OpMode.IABC:
                    builder.Append(
                        $"{$"{OpProperties[opcode].OpName} {a} {b} {c}",-20}");
                    break;
                case OpMode.IABx:
                    builder.Append(
                        $"{$"{OpProperties[opcode].OpName} {a} {bx}",-20}");
                    break;
                case OpMode.IAsBx:
                    builder.Append(
                        $"{$"{OpProperties[opcode].OpName} {a} {sbx}",-20}");
                    break;
            }

            switch ((LuaHksOps)opcode)
            {
                case LuaHksOps.OpGetField:
                case LuaHksOps.OpGetFieldR1:
                    builder.Append($"-- R({a}) := R({b})[{function.Constants[c].ToString()}]");
                    break;
                case LuaHksOps.OpTest:
                    break;
                case LuaHksOps.OpEq:
                    builder.Append($"-- if ((R({b}) == {RkHks(function, c, sZero)}) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpEqBk:
                    builder.Append($"-- if (({function.Constants[b].ToString()} == R({c})) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpGetGlobal:
                case LuaHksOps.OpGetGlobalMem:
                    builder.Append($"-- R({a}) := Gbl[{function.Constants[bx].ToString()}]");
                    break;
                case LuaHksOps.OpMove:
                    builder.Append($"-- R({a}) := R({b})");
                    break;
                case LuaHksOps.OpSelf:
                    builder.Append($"-- R({a + 1}) := R({b}); ");
                    builder.Append($"R({a}) := R({b})[{RkHks(function, c, sZero)}]");
                    break;
                case LuaHksOps.OpReturn:
                    builder.Append("-- return ");
                    for (var arg = (int)a; arg < a + b - 1; arg++)
                    {
                        if (arg != a)
                            builder.Append(", ");
                        builder.Append($"R({arg})");
                    }
                    break;
                case LuaHksOps.OpGetTableN:
                    break;
                case LuaHksOps.OpGetTable:
                case LuaHksOps.OpGetTableS:
                    builder.Append($"-- R({a}) := R({b})[{RkHks(function, c, sZero)}]");
                    break;
                case LuaHksOps.OpLoadBool:
                    break;
                case LuaHksOps.OpTForLoop:
                    builder.Append("-- ");
                    for (var r = (int)a + 3; r <= a + c + 2; r++)
                    {
                        builder.Append($"R({r})");
                        if (r != a + c + 2)
                            builder.Append(", ");
                    }
                    builder.Append(" := ");
                    builder.Append($"R({a})(R({a + 1}), R({a + 2})); ");
                    builder.Append($"if R({a+3}) ~= nil then R({a + 2}) := R({a + 3}) else PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpSetField:
                case LuaHksOps.OpSetFieldR1:
                    builder.Append($"-- R({a})[{function.Constants[b]}] := {RkHks(function, c, false)}");
                    break;
                case LuaHksOps.OpSetTable:
                case LuaHksOps.OpSetTableS:
                    builder.Append($"-- R({a})[{RkHks(function, b, false)}] := {RkHks(function, c, false)}");
                    break;
                case LuaHksOps.OpSetTableN:
                    break;
                case LuaHksOps.OpSetTableNBK:
                    break;
                case LuaHksOps.OpSetTableBK:
                case LuaHksOps.OpSetTableSBK:
                    break;
                case LuaHksOps.OpTailCallI:
                    break;
                case LuaHksOps.OpTailCallC:
                    break;
                case LuaHksOps.OpTailCallM:
                    break;
                case LuaHksOps.OpLoadK:
                    builder.Append($"-- R({a}) := {function.Constants[bx].ToString()}");
                    break;
                case LuaHksOps.OpLoadNil:
                    break;
                case LuaHksOps.OpSetGlobal:
                    builder.Append($"-- Gbl[{function.Constants[bx].ToString()}] := R({a})");
                    break;
                case LuaHksOps.OpJmp:
                    builder.Append($"-- PC += {sbx} (PC = {JumpOffset(i, sbx)})");
                    break;
                case LuaHksOps.OpCallM:
                    break;
                case LuaHksOps.OpCallI:
                case LuaHksOps.OpCallIR1:
                case LuaHksOps.OpCallC:
                case LuaHksOps.OpCall:
                    builder.Append("-- ");
                    for (var r = (int)a; r <= a + c - 2; r++)
                    {
                        builder.Append($"R({r})");
                        if (r != a + c - 2)
                            builder.Append(", ");
                    }
                    if (c == 0)
                    {
                        builder.Append($"R({a})...");
                    }

                    if (a + c - 2 >= a || c == 0)
                        builder.Append(" := ");

                    builder.Append($"R({a})(");
                    for (var arg = (int)a + 1; arg < a + b; arg++)
                    {
                        if (arg != a + 1)
                            builder.Append(", ");
                        builder.Append($"R({arg})");
                    }

                    if (c == 0)
                    {
                        builder.Append($"R({a})...");
                    }
                    builder.Append(')');
                    break;
                case LuaHksOps.OpIntrinsicIndex:
                    break;
                case LuaHksOps.OpIntrinsicNewIndex:
                    break;
                case LuaHksOps.OpIntrinsicSelf:
                    break;
                case LuaHksOps.OpIntrinsicIndexLiteral:
                    break;
                case LuaHksOps.OpIntrinsicNewIndexLiteral:
                    break;
                case LuaHksOps.OpIntrinsicSelfLiteral:
                    break;
                case LuaHksOps.OpTailCall:
                    break;
                case LuaHksOps.OpGetUpVal:
                    break;
                case LuaHksOps.OpSetUpVal:
                    break;
                case LuaHksOps.OpAdd:
                    break;
                case LuaHksOps.OpAddBk:
                    break;
                case LuaHksOps.OpSub:
                    break;
                case LuaHksOps.OpSubBk:
                    break;
                case LuaHksOps.OpMul:
                    break;
                case LuaHksOps.OpMulBk:
                    break;
                case LuaHksOps.OpDiv:
                    break;
                case LuaHksOps.OpDivBk:
                    break;
                case LuaHksOps.OpMod:
                    break;
                case LuaHksOps.OpModBk:
                    break;
                case LuaHksOps.OpPow:
                    break;
                case LuaHksOps.OpPowBk:
                    break;
                case LuaHksOps.OpNewTable:
                    builder.Append($"-- R({a}) := {{}} size = {b}, {c}");
                    break;
                case LuaHksOps.OpUnm:
                    break;
                case LuaHksOps.OpNot:
                    break;
                case LuaHksOps.OpLen:
                    break;
                case LuaHksOps.OpLt:
                    builder.Append($"-- if ((R({b}) <  {RkHks(function, c, sZero)}) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpLtBk:
                    builder.Append($"-- if (({function.Constants[b].ToString()} <  R({c})) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpLe:
                    builder.Append($"-- if ((R({b}) <=  {RkHks(function, c, sZero)}) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpLeBk:
                    builder.Append($"-- if (({function.Constants[b].ToString()} <= R({c})) ~= {a}) PC++ (PC = {i / 4 + 2})");
                    break;
                case LuaHksOps.OpConcat:
                    break;
                case LuaHksOps.OpTestSet:
                    break;
                case LuaHksOps.OpForPrep:
                    break;
                case LuaHksOps.OpForLoop:
                    break;
                case LuaHksOps.OpSetList:
                    if (b == 0)
                    {
                        if (c == 1)
                        {
                            builder.Append($"-- R({a}) := R({a + 1})...");
                        }
                    }
                    else
                    {
                        builder.Append($"-- R({a})[{(c - 1) * 50 + 1}...{(c - 1) * 50 + b}] := ");
                        for (var j = 1; j <= b; j++)
                        {
                            if (j != 1)
                                builder.Append(", ");
                            builder.Append($"R({a + j})");
                        }
                    }
                    break;
                case LuaHksOps.OpClose:
                    break;
                case LuaHksOps.OpClosure:
                    builder.Append($"-- R({a}) := closure(KPROTO[{bx}]");
                    for (var arg = (int)a; arg < a + function.ChildFunctions[bx].NumParams; arg++)
                    {
                        builder.Append($", R({arg})");
                    }
                    builder.Append(')');
                    break;
                case LuaHksOps.OpVarArg:
                    break;
                case LuaHksOps.OpTailCallIR1:
                    break;
                case LuaHksOps.OpSetUpValR1:
                    break;
                case LuaHksOps.OpTestR1:
                    break;
                case LuaHksOps.OpNotR1:
                    break;
                case LuaHksOps.OpNewStruct:
                    break;
                case LuaHksOps.OpData:
                    break;
                case LuaHksOps.OpSetSlotN:
                    break;
                case LuaHksOps.OpSetSlotI:
                    break;
                case LuaHksOps.OpSetSlot:
                    break;
                case LuaHksOps.OpSetSlotS:
                    break;
                case LuaHksOps.OpSetSlotMT:
                    break;
                case LuaHksOps.OpCheckType:
                    break;
                case LuaHksOps.OpCheckTypeS:
                    break;
                case LuaHksOps.OpGetSlot:
                    break;
                case LuaHksOps.OpGetSlotMT:
                    break;
                case LuaHksOps.OpSelfSlot:
                    break;
                case LuaHksOps.OpSelfSlotMT:
                    break;
                case LuaHksOps.OpGetFieldMM:
                    break;
                case LuaHksOps.OpCheckTypeD:
                    break;
                case LuaHksOps.OpGetSlotD:
                    break;
            }
            
            builder.AppendLine();
        }
        
        return builder.ToString();
    }

    public void GenerateIr(LuaFile.Function function, Function irFunction)
    {
        var br = new BinaryReaderEx(false, function.Bytecode) { BigEndian = true };
        irFunction.BeginBlock.Instructions = new List<Instruction>(function.Bytecode.Length * 6 / 4);
        List<Instruction> instructions = new List<Instruction>(4);
        Interval definedRegisters = new Interval();

        // Parameters that are declared but unused aren't counted in the function header's parameter count, but still
        // affect register allocation in the bytecode. By finding the first register defined that isn't a parameter, we
        // can figure out how many actual parameters were in the source code.
        var firstAssignedRegister = -1;
        void FirstAssigned(uint register)
        {
            if (register >= irFunction.ParameterCount && firstAssignedRegister == -1)
                firstAssignedRegister = (int)register;
        }
        
        for (var i = 0; i < function.Bytecode.Length; i += 4)
        {
            var instruction = br.ReadUInt32();
            // Uhhh thanks again hork
            var opcode = (instruction & 0xFF000000) >> 25;
            var a = instruction & 0xFF;
            var c = (int)(instruction & 0x1FF00) >> 8;
            var b = (int)(instruction & 0x1FE0000) >> 17;
            var sZero = false;
            var pc = i / 4;

            if ((b & 0x100) > 0)
            {
                b = -(b & 0xFF);
            }

            if ((c & 0x100) > 0)
            {
                if (c == 0x100)
                {
                    sZero = true;
                }

                c = -(c & 0xFF);
            }

            var bx = (instruction & 0x1FFFF00) >> 8;
            var sbx = (int)bx;
            uint addr;
            List<Expression> args;
            List<IAssignable> rets;
            instructions.Clear();
            Assignment Assignment;
            switch ((LuaHksOps)opcode)
            {
                case LuaHksOps.OpMove:
                    Assignment = new Assignment(irFunction.GetRegister(a), Register(irFunction, (uint)b));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpLoadK:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        ToConstantIr(function.Constants[bx], (int)bx));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpLoadBool:
                    Assignment = new Assignment(irFunction.GetRegister(a), new Constant(b == 1, -1));
                    Assignment.NilAssignmentReg = a;
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    if (c > 0)
                    {
                        instructions.Add(new JumpLabel(irFunction.GetLabel((uint)(i / 4 + 2))));
                    }

                    break;
                case LuaHksOps.OpLoadNil:
                    var nlist = new List<IAssignable>();
                    for (var arg = (int)a; arg <= b; arg++)
                    {
                        nlist.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                        FirstAssigned((uint)arg);
                    }

                    Assignment = new Assignment(nlist, new Constant(Constant.ConstantType.ConstNil, -1));
                    Assignment.NilAssignmentReg = a;
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpGetUpVal:
                    var up = Function.GetUpValue((uint)b);
                    instructions.Add(new Assignment(irFunction.GetRegister(a), new IdentifierReference(up)));
                    FirstAssigned(a);
                    break;
                case LuaHksOps.OpSetUpVal:
                case LuaHksOps.OpSetUpValR1:
                    up = Function.GetUpValue((uint)b);
                    if (b >= irFunction.UpValueCount)
                    {
                        throw new Exception("Reference to unbound upvalue: " + up);
                    }

                    instructions.Add(new Assignment(up, new IdentifierReference(irFunction.GetRegister(a))));
                    break;
                case LuaHksOps.OpGetGlobalMem:
                case LuaHksOps.OpGetGlobal:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new IdentifierReference(Identifier.GetGlobal(bx)));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpGetTableS:
                case LuaHksOps.OpGetTable:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new TableAccess(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            RkIrHks(irFunction, function, c, sZero)));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpSetGlobal:
                    instructions.Add(new Assignment(Identifier.GetGlobal(bx),
                        new IdentifierReference(irFunction.GetRegister(a))));
                    break;
                case LuaHksOps.OpNewTable:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new InitializerList(new List<Expression>()));
                    Assignment.VarargAssignmentReg = a;
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;

                case LuaHksOps.OpSelf:
                    var op = new Assignment(
                        irFunction.GetRegister(a + 1), Register(irFunction, (uint)b));
                    op.SelfAssignMinRegister = (int)a;
                    instructions.Add(op);
                    var selfIdentifier =
                        new TableAccess(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            RkIrHks(irFunction, function, c, sZero));
                    selfIdentifier.IsSelfReference = true;
                    op = new Assignment(irFunction.GetRegister(a), selfIdentifier);
                    op.SelfAssignMinRegister = (int)a;
                    FirstAssigned(a);
                    FirstAssigned(a + 1);
                    instructions.Add(op);
                    break;
                case LuaHksOps.OpAdd:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpAdd));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpAddBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpAdd));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpSub:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpSub));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpSubBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpSub));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpMul:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpMul));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpMulBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpMul));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpDiv:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpDiv));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpDivBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpDiv));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpMod:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpMod));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpModBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpMod));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpPow:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(Register(irFunction, (uint)b),
                            RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpPow));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpPowBk:
                    Assignment = new Assignment(
                        irFunction.GetRegister(a),
                        new BinOp(ToConstantIr(function.Constants[b], b),
                            Register(irFunction, (uint)c), BinOp.OperationType.OpPow));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpUnm:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new UnaryOp(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            UnaryOp.OperationType.OpNegate));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpNot:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new UnaryOp(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            UnaryOp.OperationType.OpNot));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpLen:
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new UnaryOp(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            UnaryOp.OperationType.OpLength));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpConcat:
                    args = new List<Expression>();
                    for (var arg = b; arg <= c; arg++)
                    {
                        args.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                    }

                    Assignment = new Assignment(irFunction.GetRegister(a), new Concat(args));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpJmp:
                    addr = (uint)(i / 4 + 2 + ((sbx << 16) >> 16));
                    if ((sbx & 0x10000) != 0)
                    {
                        // Unsigned address?
                        addr = (uint)((sbx & 0xFFFF) + 2 + (uint)(i / 4));
                    }

                    instructions.Add(new JumpLabel(irFunction.GetLabel(addr)));
                    break;
                case LuaHksOps.OpEq:
                    if (a == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpEqual)));
                    }
                    else
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), BinOp.OperationType.OpNotEqual)));
                    }

                    break;
                case LuaHksOps.OpLt:
                    if (a == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), 
                                    BinOp.OperationType.OpLessThan, BinOp.OriginalOpType.OpLt)));
                    }
                    else
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), 
                                    BinOp.OperationType.OpLessThan, BinOp.OriginalOpType.OpLt) { HasImplicitNot = true}));
                    }

                    break;
                case LuaHksOps.OpLtBk:
                    if (a == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(ToConstantIr(function.Constants[b], b),
                                    Register(irFunction, (uint)c), 
                                    BinOp.OperationType.OpLessThan, BinOp.OriginalOpType.OpLtBk)));
                    }
                    else
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(ToConstantIr(function.Constants[b], b),
                                    Register(irFunction, (uint)c), 
                                    BinOp.OperationType.OpLessThan, BinOp.OriginalOpType.OpLtBk) { HasImplicitNot = true}));
                    }

                    break;
                case LuaHksOps.OpLe:
                    if (a == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), 
                                    BinOp.OperationType.OpLessEqual, BinOp.OriginalOpType.OpLe)));
                    }
                    else
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(Register(irFunction, (uint)b),
                                    RkIrHks(irFunction, function, c, sZero), 
                                    BinOp.OperationType.OpLessEqual, BinOp.OriginalOpType.OpLe) { HasImplicitNot = true}));
                    }

                    break;
                case LuaHksOps.OpLeBk:
                    if (a == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new BinOp(ToConstantIr(function.Constants[b], b),
                                    Register(irFunction, (uint)c), 
                                    BinOp.OperationType.OpLessEqual, BinOp.OriginalOpType.OpLeBk)));
                    }
                    else
                    {
                        instructions.Add(new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                            new BinOp(ToConstantIr(function.Constants[b], b),
                                Register(irFunction, (uint)c), 
                                BinOp.OperationType.OpLessEqual, BinOp.OriginalOpType.OpLeBk) { HasImplicitNot = true}));
                    }

                    break;
                case LuaHksOps.OpTest:
                case LuaHksOps.OpTestR1:
                    if (c == 0)
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)), Register(irFunction, a)));
                    }
                    else
                    {
                        instructions.Add(
                            new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                                new UnaryOp(Register(irFunction, a), UnaryOp.OperationType.OpNot)));
                    }

                    break;
                case LuaHksOps.OpSetTableS:
                case LuaHksOps.OpSetTable:
                    instructions.Add(
                        new Assignment(
                            new TableAccess(new IdentifierReference(irFunction.GetRegister(a)),
                                RkIrHks(irFunction, function, b, false)),
                            RkIrHks(irFunction, function, c, false)));
                    break;
                case LuaHksOps.OpTailCallI:
                case LuaHksOps.OpTailCallIR1:
                    args = new List<Expression>();
                    for (var arg = (int)a + 1; arg < a + b; arg++)
                    {
                        args.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                    }

                    instructions.Add(
                        new Return(new FunctionCall(
                            new IdentifierReference(irFunction.GetRegister(a)), args))
                        {
                            IsTailReturn = true
                        });
                    break;
                case LuaHksOps.OpSetTableSBK:
                    instructions.Add(
                        new Assignment(
                            new TableAccess(new IdentifierReference(irFunction.GetRegister(a)),
                                ToConstantIr(function.Constants[b], b)),
                            RkIrHks(irFunction, function, c, false)));
                    break;
                case LuaHksOps.OpCallI:
                case LuaHksOps.OpCallIR1:
                case LuaHksOps.OpCall:
                    args = new List<Expression>();
                    rets = new List<IAssignable>();
                    for (var arg = (int)a + 1; arg < a + b; arg++)
                    {
                        args.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                    }

                    for (var r = (int)a; r <= a + c - 2; r++)
                    {
                        rets.Add(new IdentifierReference(irFunction.GetRegister((uint)r)));
                        FirstAssigned((uint)r);
                    }

                    if (c == 0)
                    {
                        rets.Add(new IdentifierReference(irFunction.GetRegister(a)));
                        FirstAssigned(a);
                    }

                    var functionCall = new FunctionCall(new IdentifierReference(irFunction.GetRegister(a)), args)
                    {
                        HasAmbiguousArgumentCount = b == 0,
                        HasAmbiguousReturnCount = c == 0,
                        BeginArg = a + 1
                    };
                    Assignment = new Assignment(rets, functionCall);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpReturn:
                    args = new List<Expression>();
                    if (b != 0)
                    {
                        for (var arg = (int)a; arg < a + b - 1; arg++)
                        {
                            args.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                        }
                    }

                    var ret = new Return(args);
                    if (b == 0)
                    {
                        ret.BeginRet = a;
                        ret.IsAmbiguousReturnCount = true;
                    }

                    instructions.Add(ret);
                    break;
                case LuaHksOps.OpForLoop:
                    addr = (uint)(i / 4 + 2 + ((sbx << 16) >> 16));
                    if ((sbx & 0x10000) != 0)
                    {
                        // Unsigned address?
                        addr = (uint)((sbx & 0xFFFF) + 2 + (uint)(i / 4));
                    }

                    instructions.Add(new Assignment(new IdentifierReference(irFunction.GetRegister(a)),
                        new BinOp(new IdentifierReference(irFunction.GetRegister(a)),
                            new IdentifierReference(irFunction.GetRegister(a + 2)), BinOp.OperationType.OpAdd)));
                    var pta = new Assignment(irFunction.GetRegister(a + 3), Register(irFunction, a));
                    pta.PropagateAlways = true;
                    var jmp = new ConditionalJumpLabel(irFunction.GetLabel(addr), new BinOp(
                        new IdentifierReference(irFunction.GetRegister(a)),
                        new IdentifierReference(irFunction.GetRegister(a + 1)),
                        BinOp.OperationType.OpLoopCompare), pta, new Interval((int)a, (int)a + 4));
                    instructions.Add(jmp);
                    break;
                case LuaHksOps.OpTForLoop:
                    // The IR generated by this is technically wrong, but done in a way to make high level loop recovery
                    // much easier
                    args = new List<Expression>();
                    rets = new List<IAssignable>();
                    args.Add(new IdentifierReference(irFunction.GetRegister(a + 1)));
                    args.Add(new IdentifierReference(irFunction.GetRegister(a + 2)));
                    if (c == 0)
                    {
                        rets.Add(new IdentifierReference(irFunction.GetRegister(a + 2)));
                        FirstAssigned(a + 3);
                    }
                    else
                    {
                        for (var r = (int)a + 3; r <= a + c + 2; r++)
                        {
                            rets.Add(new IdentifierReference(irFunction.GetRegister((uint)r)));
                            FirstAssigned((uint)r);
                        }
                    }

                    functionCall = new FunctionCall(new IdentifierReference(irFunction.GetRegister(a)), args)
                    {
                        HasAmbiguousReturnCount = c == 0
                    };
                    Assignment = new Assignment(rets, functionCall);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    instructions.Add(new ConditionalJumpLabel(irFunction.GetLabel((uint)(i / 4 + 2)),
                        new BinOp(Register(irFunction, a + 3),
                            new Constant(Constant.ConstantType.ConstNil, -1), BinOp.OperationType.OpNotEqual),
                        null, new Interval((int)a, (int)a + (int)c + 3)));
                    break;
                case LuaHksOps.OpForPrep:
                    addr = (uint)(i / 4 + 2 + ((sbx << 16) >> 16));
                    if ((sbx & 0x10000) != 0)
                    {
                        // Unsigned address?
                        addr = (uint)((sbx & 0xFFFF) + 2 + (uint)(i / 4));
                    }

                    // Emit self assignments for the index, limit, and step for local names and to ensure they get
                    // properly inlined into the loop. These are marked as the local declarations for the registers in
                    // the scope so that any prior definitions get marked as temporaries and get inlined
                    Assignment = new Assignment(new IdentifierReference(irFunction.GetRegister(a)),
                        new IdentifierReference(irFunction.GetRegister(a))) { IsLocalDeclaration = true };
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc + 1);
                    instructions.Add(Assignment);
                    instructions.Add(new Assignment(new IdentifierReference(irFunction.GetRegister(a + 1)),
                        new IdentifierReference(irFunction.GetRegister(a + 1))) { IsLocalDeclaration = true });
                    FirstAssigned(a + 1);
                    instructions.Add(new Assignment(new IdentifierReference(irFunction.GetRegister(a + 2)),
                        new IdentifierReference(irFunction.GetRegister(a + 2))) { IsLocalDeclaration = true });
                    FirstAssigned(a + 2);
                    FirstAssigned(a + 3);
                    
                    instructions.Add(new JumpLabel(irFunction.GetLabel(addr)));
                    break;
                case LuaHksOps.OpSetList:
                    if (b == 0)
                    {
                        // Ambiguous Assignment
                        if (c == 1)
                        {
                            Assignment = new Assignment(irFunction.GetRegister(a), Register(irFunction, a + 1));
                            Assignment.VarargAssignmentReg = a;
                            Assignment.IsAmbiguousVararg = true;
                            CheckLocal(Assignment, function, pc);
                            instructions.Add(Assignment);
                        }
                    }
                    else
                    {
                        var listValues = new List<Expression>();
                        var listIndices = new Interval();
                        for (var j = 1; j <= b; j++)
                        {
                            listIndices.AddToRange((c - 1) * 50 + j);
                            listValues.Add(new IdentifierReference(irFunction.GetRegister(a + (uint)j)));
                        }

                        instructions.Add(
                            new ListRangeAssignment(
                                new IdentifierReference(irFunction.GetRegister(a)), listIndices, listValues)
                            {
                                AlwaysTemporaryRegister = (int)a + 1
                            });
                    }

                    break;
                case LuaHksOps.OpClosure:
                    var closureFunction = irFunction.LookupClosure(bx);
                    Assignment = new Assignment(irFunction.GetRegister(a),
                        new Closure(closureFunction)) { OpLocation = i / 4 };
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    
                    // Closure instructions for closures that have upValues seem to be followed by a data instruction
                    // for each upValue where A is 1 and Bx is number for the register that is bound to the upValue.
                    // There is probably a different pattern for binding an upValue in the child closure to an upValue
                    // in this function, but that will require more experimentation to figure out.
                    for (var j = 0; j < closureFunction.UpValueCount; j++)
                    {
                        i += 4;
                        Debug.Assert(i < function.Bytecode.Length);
                        var upValueInstruction = br.ReadUInt32();
                        var upValueOpCode = (upValueInstruction & 0xFF000000) >> 25;
                        var opBx = (upValueInstruction & 0x1FFFF00) >> 8;
                        var upValueIdentifier = upValueOpCode switch
                        {
                            (uint)LuaHksOps.OpData => Identifier.GetRegister(opBx),
                            _ => throw new Exception(
                                "Expected a data instruction for closure upValue binding")
                        };
                        var closureBinding = new ClosureBinding(upValueIdentifier) { OpLocation = i / 4 };
                        instructions.Add(closureBinding);
                    }
                    break;
                case LuaHksOps.OpGetField:
                case LuaHksOps.OpGetFieldR1:
                    Assignment = new Assignment(Register(irFunction, a),
                        new TableAccess(new IdentifierReference(irFunction.GetRegister((uint)b)),
                            new Constant(function.Constants[c].ToString(), -1)));
                    FirstAssigned(a);
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpData:
                    var dat = new Data();
                    CheckLocal(dat, function, pc);
                    instructions.Add(dat);
                    break;
                case LuaHksOps.OpSetField:
                case LuaHksOps.OpSetFieldR1:
                    Assignment = new Assignment(new TableAccess(new IdentifierReference(irFunction.GetRegister(a)),
                            new Constant(function.Constants[b].ToString(), b)),
                        RkIrHks(irFunction, function, c, false));
                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    break;
                case LuaHksOps.OpVarArg:
                    var varArgs = new List<IAssignable>();
                    for (var arg = (int)a; arg <= a + b - 1; arg++)
                    {
                        varArgs.Add(new IdentifierReference(irFunction.GetRegister((uint)arg)));
                        FirstAssigned((uint)arg);
                    }

                    if (b != 0)
                    {
                        Assignment = new Assignment(varArgs, new IdentifierReference(Identifier.GetVarArgs()));
                    }
                    else
                    {
                        Assignment = new Assignment(irFunction.GetRegister(a),
                            new IdentifierReference(Identifier.GetVarArgs()))
                        {
                            IsAmbiguousVararg = true,
                            VarargAssignmentReg = a
                        };
                        FirstAssigned(a);
                    }

                    CheckLocal(Assignment, function, pc);
                    instructions.Add(Assignment);
                    irFunction.IsVarargs = true;
                    break;
                default:
                    switch (OpProperties[opcode].OpMode)
                    {
                        case OpMode.IABC:
                            instructions.Add(
                                new PlaceholderInstruction($@"{OpProperties[opcode].OpName} {a} {b} {c}"));
                            break;
                        case OpMode.IABx:
                            instructions.Add(new PlaceholderInstruction($@"{OpProperties[opcode].OpName} {a} {bx}"));
                            break;
                        case OpMode.IAsBx:
                            instructions.Add(new PlaceholderInstruction(
                                $@"{OpProperties[opcode].OpName} {a} {(sbx & 0x10000) >> 16} {sbx & 0xFFFF}"));
                            break;
                        case OpMode.IAx:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    throw new Exception($@"Unimplemented opcode {OpProperties[opcode].OpName}");
                    if (OpProperties[opcode].OpName == null)
                    {
                        Console.WriteLine(opcode);
                    }

                    break;
            }

            foreach (var inst in instructions)
            {
                if (inst.OpLocation == -1)
                    inst.OpLocation = i / 4;
                inst.InstructionIndices = new Interval(irFunction.BeginBlock.Instructions.Count);
                irFunction.BeginBlock.Instructions.Add(inst);
            }
        }
        
        // Something is up where we can't trust lua function parameter count, so registers below the first register
        // defined in this function are parameters.
        if (irFunction.IsVarargs)
            firstAssignedRegister--;
        irFunction.ParameterCount = Math.Max(firstAssignedRegister, irFunction.ParameterCount);
    }

    public void AddDecompilePasses(PassManager passManager)
    {
        passManager.AddPass("apply-labels", new ApplyLabelsPass());
        passManager.AddPass("cleanup-havok-instructions", new CleanupHavokInstructionsPass());
        passManager.AddPass("vararg-list-assignment", new RewriteVarargListAssignmentPass());
        passManager.AddPass("merge-multiple-bool-assignment", new MergeMultipleBoolAssignmentPass());
        passManager.AddPass("merge-conditional-jumps", new MergeConditionalJumpsPass());
        passManager.AddPass("validate-jump-dest-labels", new ValidateJumpDestinationLabelsPass());

        passManager.AddPass("build-cfg", new BuildControlFlowGraphPass());
        passManager.AddPass("resolve-ambiguous-call-args", new ResolveAmbiguousCallArguments());
        passManager.AddPass("ssa-transform", new SsaTransformPass());
        passManager.AddPass("resolve-closure-up-values-50", new ResolveClosureUpValues50Pass());
        passManager.AddPass("eliminate-dead-phi-1", new EliminateDeadAssignmentsPass(true));
        passManager.AddPass("eliminate-unused-phi", new EliminateUnusedPhiFunctionsPass());
        passManager.AddPass("detect-list-initializers-initial", new DetectListInitializersPass());
        
        passManager.PushLoopUntilUnchanged();
        passManager.AddPass("expression-propagation-1", new ExpressionPropagationPass());
        passManager.AddPass("detect-list-initializers", new DetectListInitializersPass());
        passManager.AddPass("merge-compound-conditionals", new MergeCompoundConditionalsPass());
        passManager.AddPass("merge-conditional-assignments", new MergeConditionalAssignmentsPass());
        passManager.PopLoopUntilUnchanged();
        
        passManager.AddPass("detect-loops", new DetectLoopsPass());
        passManager.AddPass("detect-break-continue", new StructureLoopBreaksPass());
        passManager.AddPass("detect-two-way-conditionals", new DetectTwoWayConditionalsPass());
        passManager.AddPass("simplify-if-else-follow-chain", new SimplifyIfElseFollowChainPass());
        passManager.AddPass("eliminate-dead-phi-2", new EliminateDeadAssignmentsPass(true));
        passManager.AddPass("expression-propagation-3", new ExpressionPropagationPass());
        //passManager.AddPass("validate-liveness-no-interference", new ValidateLivenessNoInterferencePass());

        passManager.AddPass("drop-ssa-subscripts", new DropSsaSubscriptsPass());
        passManager.AddPass("detect-local-variables", new DetectLocalVariablesPass());
        // irfun.ArgumentNames = fun.LocalsAt(0);
        passManager.AddPass("rename-local-variables", new RenameVariablesPass());
        passManager.AddPass("solve-expressions", new SolveExpressionsPass());
        passManager.AddPass("parenthesize", new ParenthesizePass());
        //passManager.AddPass("annotate-env-act", new AnnotateEnvActFunctionsPass());

        passManager.AddPass("build-ast", new AstTransformPass());
    }
}