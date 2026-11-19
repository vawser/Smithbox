using System;
using System.Collections.Generic;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// In Lua, local variables that are declared but never assigned do not generate any instructions but still affect
/// register allocation: a register is still reserved for the local and will never be assigned. This leaves gaps in
/// the allocated registers where local variable declarations need to be inserted for byte perfect compilation.
/// </summary>
public class DetectUnassignedLocalsPassOld : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var localVariableAnalysis = functionContext.GetAnalysis<LocalVariablesAnalyzer>();
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        var definesSet = new HashSet<Identifier>(2);
        
        Identifier NewName(uint register)
        {
            if (f.RenamedRegisterCounts == null) throw new Exception();
            var newName = Identifier.GetRenamedRegister(register, f.RenamedRegisterCounts[register]);
            f.RenamedRegisterCounts[register]++;
            f.SsaVariables.Add(newName);
            return newName;
        }
        
        int Visit(BasicBlock b, int inputMaxLocal)
        {
            // Highest local variable defined so far in the program
            var maxLocal = inputMaxLocal;

            // Highest local or active temporary defined so far
            var maxActiveTemporary = maxLocal;
            
            // The first new register defined in this block (i.e. higher than inputMaxLocal)
            var firstAllocatedRegister = int.MaxValue;

            var i = 0;
            while (i < b.Instructions.Count)
            {
                // Get all the defined registers
                var insertionPoint = i;
                int currentOpcodeIndex = b.Instructions[i].OpLocation;
                definesSet.Clear();
                while (i < b.Instructions.Count && b.Instructions[i].OpLocation == currentOpcodeIndex)
                {
                    var instruction = b.Instructions[i];
                    instruction.GetDefinedRegisters(definesSet);
                    i++;
                }
                
                // Skip instructions that don't define anything
                if (definesSet.Count == 0)
                    continue;
                
                // Build interval for defined registers and see if they are locals or temporaries
                Interval definedRegisters = new Interval();
                var anyLocal = false;
                foreach (var define in definesSet)
                {
                    anyLocal |= localVariableAnalysis.LocalVariables.Contains(define);
                    definedRegisters.AddToRange((int)define.RegNum);
                }

                // Update first allocated register
                if (definedRegisters.Begin > inputMaxLocal)
                    firstAllocatedRegister = Math.Min(firstAllocatedRegister, definedRegisters.Begin);
                
                // Check for gaps between what this opcode defined vs what has been defined so far
                if (definedRegisters.Begin > maxActiveTemporary + 1)
                {
                    // There's gaps so insert assignment instructions before this opcode
                    for (var j = definedRegisters.Begin - 1; j > maxActiveTemporary; j--)
                    {
                        var assignment = new Assignment(NewName((uint)j), null)
                        {
                            OpLocation = currentOpcodeIndex
                        };
                        b.Instructions.Insert(insertionPoint, assignment);
                        i++;
                    }

                    maxLocal = maxActiveTemporary = definedRegisters.End - 1;
                }
                else if (definedRegisters.Begin == maxActiveTemporary + 1)
                {
                    // More temporaries/locals in a chain
                    maxActiveTemporary = definedRegisters.End - 1;
                    if (anyLocal)
                        maxLocal = definedRegisters.End - 1;
                }
                else
                {
                    // Defined is less than max active temporary. Update max local if applicable and reset
                    if (anyLocal)
                        maxLocal = Math.Max(definedRegisters.End - 1, maxLocal);
                    maxActiveTemporary = maxLocal;
                }
            }
            
            // Visit dominance successors and get the first new register allocated
            var minFirstChildAllocatedRegister = int.MaxValue;
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                var fd = Visit(successor, maxLocal);
                minFirstChildAllocatedRegister = Math.Min(minFirstChildAllocatedRegister, fd);
            });

            if (minFirstChildAllocatedRegister == int.MaxValue)
                return firstAllocatedRegister;
            
            // Insert assignments to cover gaps in register definitions at the end
            var opIndex = b.Instructions.Count > 0 ? b.Last.OpLocation : 0;
            var lastIsJumpReturn = b.Instructions.Count > 0 && b.Last is IJump or Return;
            for (i = maxLocal + 1; i < minFirstChildAllocatedRegister; i++)
            {
                var assignment = new Assignment(NewName((uint)i), null)
                {
                    OpLocation = opIndex
                };
                var insertIndex = lastIsJumpReturn ? b.Instructions.Count - 1 : b.Instructions.Count;
                //b.Instructions.Insert(insertIndex, assignment);
            }
            
            return firstAllocatedRegister;
        }

        Visit(f.BeginBlock, f.ParameterCount - 1);
        
        functionContext.InvalidateAnalysis<DetectLocalVariablesPass>();
        
        return false;
    }
}