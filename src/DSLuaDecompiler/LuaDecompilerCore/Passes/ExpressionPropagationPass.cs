using System;
using System.Collections.Generic;
using System.Diagnostics;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Performs expression propagation which substitutes register definitions into their users to build more complex
/// expressions
/// </summary>
public class ExpressionPropagationPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var irChanged = false;
        
        // GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var usesSet = new HashSet<Identifier>(10);
        var usesSet2 = new HashSet<Identifier>(10);

        var localVariableAnalysis = functionContext.GetAnalysis<LocalVariablesAnalyzer>();
        var defineUseAnalysis = functionContext.GetAnalysis<IdentifierDefinitionUseAnalyzer>();

        bool changed;
        do
        {
            changed = false;
            foreach (var b in f.BlockList)
            {
                for (var i = 0; i < b.Instructions.Count; i++)
                {
                    var inst = b.Instructions[i];
                    usesSet.Clear();
                    inst.GetUsedRegisters(usesSet);
                    foreach (var use in usesSet)
                    {
                        var definingInstruction = defineUseAnalysis.DefiningInstruction(use);
                        bool isListElementDefine = definingInstruction?.GetSingleDefine() is { } def &&
                                                   defineUseAnalysis.IsListInitializerElement(def);
                        if (definingInstruction is Assignment
                            {
                                IsSingleAssignment: true, 
                                LocalAssignments: null, 
                                Right: not null
                            } a &&
                            ((defineUseAnalysis.UseCount(use) == 1 && 
                              (((i - 1 >= 0 && b.Instructions[i - 1] == definingInstruction || isListElementDefine) &&
                                definingInstruction.OriginalBlock == inst.OriginalBlock) || 
                               inst is Assignment { IsListAssignment: true }) && 
                              !localVariableAnalysis.LocalVariables.Contains(use)) ||
                             (inst is Assignment
                             {
                                 IsLocalDeclaration: true, 
                                 IsSingleAssignment: true,
                                 Left: IdentifierReference { Identifier: { IsRegister: true, RegNum: var reg } }
                             } && use.RegNum == reg) ||
                             a.PropagateAlways))
                        {
                            // Don't substitute if this use's define was defined before the code gen for the function
                            // call even began
                            usesSet2.Clear();
                            if (!a.PropagateAlways && inst is Assignment { Right: FunctionCall fc } && 
                                definingInstruction.InstructionIndices.End - 1 < fc.FunctionDefIndex &&
                                fc.GetUsedRegisters(usesSet2).Contains(use))
                            {
                                // This is extremely ugly but we still need to allow function chaining if the function
                                // itself is a register
                                if (!(fc.Function is IdentifierReference ir && ir.Identifier == use) &&
                                    !(fc is { IsThisCall: true, Function: TableAccess 
                                          { Table: IdentifierReference ir2, TableIndex: Constant }
                                      } &&
                                      ir2.Identifier == use))
                                    continue;
                            }
                            if (!a.PropagateAlways && inst is Return { ReturnExpressions: [FunctionCall fc2] } && 
                                definingInstruction.InstructionIndices.End - 1 < fc2.FunctionDefIndex)
                            {
                                continue;
                            }
                            
                            // Don't inline a function call into a non-tail call return because the Lua compiler would
                            // have generated a tail call instead if this weren't a local
                            if (a is { PropagateAlways: false, Right: FunctionCall } &&
                                inst is Return
                                {
                                    IsTailReturn: false, ReturnExpressions: [IdentifierReference]
                                })
                            {
                                continue;
                            }
                            
                            // Make sure the use falls within the temporary range for this instruction
                            if (!a.PropagateAlways && !inst.GetTemporaryRegisterRange().Contains((int)use.RegNum))
                                continue;
                            
                            if (inst.ReplaceUses(use, a.Right))
                            {
                                var definingBlock = f.BlockList[defineUseAnalysis.DefiningInstructionBlock(use)];
                                irChanged = true;
                                changed = true;
                                inst.Absorb(a);
                                definingBlock.Instructions.Remove(a);
                                f.SsaVariables.Remove(use);
                                if (b == definingBlock)
                                {
                                    i = -1;
                                }
                            }
                        }
                    }
                }
            }

            // Lua might generate the following (decompiled) code when doing a this call on a global variable:
            //     REG0 = someGlobal
            //     REG0:someFunction(blah...)
            // This rewrites such statements to
            //     someGlobal:someFunction(blah...)
            foreach (var b in f.BlockList)
            {
                for (var i = 0; i < b.Instructions.Count; i++)
                {
                    var inst = b.Instructions[i];
                    if (inst is Assignment { Right: FunctionCall { Args.Count: > 0, IsThisCall: true } fc } a &&
                        fc.Args[0] is IdentifierReference ir &&
                        defineUseAnalysis.UseCount(ir.Identifier) == 2 &&
                        i > 0 && b.Instructions[i - 1] is Assignment { IsSingleAssignment: true, Left: IdentifierReference ir2 } a2 && 
                        ir2.Identifier == ir.Identifier &&
                        a2.Right is IdentifierReference or Constant)
                    {
                        a.ReplaceUses(ir2.Identifier, a2.Right);
                        a.Absorb(a2);
                        b.Instructions.RemoveAt(i - 1);
                        i--;
                        changed = true;
                        irChanged = true;
                    }
                    
                    // match tail calls
                    if (inst is Return { ReturnExpressions: [FunctionCall { Args.Count: > 0 } fc2] } ret &&
                        fc2.Args[0] is IdentifierReference ir3 &&
                        defineUseAnalysis.UseCount(ir3.Identifier) == 2 &&
                        i > 0 && b.Instructions[i - 1] is Assignment { IsSingleAssignment: true, Left: IdentifierReference ir4 } a4 && 
                        ir4.Identifier == ir3.Identifier &&
                        a4.Right is IdentifierReference or Constant)
                    {
                        ret.ReplaceUses(ir4.Identifier, a4.Right);
                        ret.Absorb(a4);
                        b.Instructions.RemoveAt(i - 1);
                        i--;
                        changed = true;
                        irChanged = true;
                    }
                }
            }
        } while (changed);

        if (irChanged)
        {
            functionContext.InvalidateAnalysis<IdentifierDefinitionUseAnalyzer>();
            functionContext.InvalidateAnalysis<LocalVariablesAnalyzer>();
        }

        return irChanged;
    }
}