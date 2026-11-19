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
///
/// This pass is designed to run on the close to final IR after all expression propagation has been done.
/// </summary>
public class DetectUnassignedLocalsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
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
        
        void Visit(BasicBlock b, int inputMaxLocal)
        {
            // Highest local variable defined so far in the program
            var maxLocal = inputMaxLocal;

            // The goal of this pass is to find "register gaps" that occur between local declarations and temp
            // registers. Each IR instruction has a range or interval of temporary registers that were used to build
            // the expressions and statements. If the instruction is a local variable declaration and assignment, it
            // will mark a range of variables as "local" for the remainder of the scope. Temporary registers are
            // allocated starting at the first available register that isn't reserved as a local, and are only valid for
            // that instruction. For byte perfect recompiles we need to insert local declarations (with no assignment)
            // in the following cases:
            // 1) The beginning of an instruction's temporary registers isn't the first register after the highest
            //    active local variable register.
            // 2) The register assigned to a new local declaration isn't immediately after the last local declaration
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                var unassignedLocals = new Interval();
                var instruction = b.Instructions[i];
                
                // First check the temporaries defined for gaps
                if (instruction.InlinedRegisters.Begin > maxLocal + 1)
                {
                    unassignedLocals = new Interval(maxLocal + 1, instruction.InlinedRegisters.Begin);
                }
                
                // Next check defined locals
                definesSet.Clear();
                instruction.GetDefinedRegisters(definesSet);
                if (instruction is Assignment { IsLocalDeclaration: true } && definesSet.Count > 0)
                {
                    var defined = new Interval();
                    foreach (var define in definesSet)
                    {
                        defined.AddToRange((int)define.RegNum);
                    }

                    maxLocal = Math.Max(maxLocal, defined.End - 1);

                    if (defined.Begin > maxLocal + 1)
                    {
                        unassignedLocals.UnionWith(new Interval(maxLocal + 1, defined.Begin));
                    }
                }
                
                if (unassignedLocals.Count == 0)
                    continue;
                
                // Emit unassigned local declarations
                for (var r = unassignedLocals.Begin; r < unassignedLocals.End; r++)
                {
                    b.Instructions.Insert(i, new Assignment(NewName((uint)r), null) { IsLocalDeclaration = true });
                    i++;
                }

                maxLocal = Math.Max(maxLocal, unassignedLocals.End - 1);
            }
            
            // Run on dominance successors
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                var outMaxLocal = maxLocal;
                if (b.KilledLocals.Count > 0 && successor == b.LoopFollow)
                    outMaxLocal = Math.Min(outMaxLocal, b.KilledLocals.Begin - 1);
                Visit(successor, outMaxLocal);
            });
        }

        Visit(f.BeginBlock, f.ParameterCount - 1);
        return false;
    }
}