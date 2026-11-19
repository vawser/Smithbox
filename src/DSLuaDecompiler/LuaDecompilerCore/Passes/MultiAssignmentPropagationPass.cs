using System.Collections.Generic;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.Passes;

public class MultiAssignmentPropagationPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var localVariableAnalysis = functionContext.GetAnalysis<LocalVariablesAnalyzer>();

        bool changed = false;

        var assignmentList = new List<Assignment>(10);
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                var inst = b.Instructions[i];
                if (inst is Assignment { IsMultiAssignment: true } multiAssignment)
                {
                    // Assignment must be a contiguous set of registers with no index and aren't locals
                    var assignedRegisters = new Interval();
                    foreach (var identifier in multiAssignment.LeftList)
                    {
                        if (identifier is not IdentifierReference ir || !ir.Identifier.IsRegister)
                            goto NEXT;
                        
                        if (localVariableAnalysis.LocalVariables.Contains(ir.Identifier))
                            goto NEXT;
            
                        if (assignedRegisters.Count == 0 || assignedRegisters.End == ir.Identifier.RegNum)
                            assignedRegisters.AddToRange((int)ir.Identifier.RegNum);
                        else
                            goto NEXT;
                    }
                    
                    // Now we can see if there is a list of assignment instructions that all assign the registers in
                    // the interval
                    assignmentList.Clear();
                    for (var j = 0; j < assignedRegisters.Count; j++)
                    {
                        if (i + j + 1 >= b.Instructions.Count)
                            goto NEXT;

                        if (b.Instructions[i + j + 1] is not Assignment { IsSingleAssignment:true, Left: IdentifierReference ir } a)
                            goto NEXT;
            
                        if (!a.IsSingleAssignment || (ir.IsRegister && 
                                                      ir.Identifier.RegNum >= assignedRegisters.End))
                            goto NEXT;

                        if (!(a.Right is IdentifierReference { Identifier: { IsRegister: true } identifier } &&
                              identifier.RegNum == assignedRegisters.End - j - 1))
                            goto NEXT;
                        
                        assignmentList.Add(a);
                    }
                    
                    // Substitution is valid so do it
                    for (var j = 0; j < assignedRegisters.Count; j++)
                    {
                        multiAssignment.LeftList[^(j + 1)] = assignmentList[j].Left;
                        multiAssignment.Absorb(assignmentList[j]);
                        b.Instructions.RemoveAt(i + 1);
                        changed = true;
                    }
                }
                
                NEXT:;
            }
        }

        if (changed)
        {
            functionContext.InvalidateAnalysis<IdentifierDefinitionUseAnalyzer>();
            functionContext.InvalidateAnalysis<LocalVariablesAnalyzer>();
        }

        return changed;
    }
}