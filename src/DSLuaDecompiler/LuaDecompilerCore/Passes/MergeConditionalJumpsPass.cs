using System;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

public class MergeConditionalJumpsPass : IPass
{
    /// <summary>
    /// Simple analysis pass to recognize lua conditional jumping patterns and merge them into a single instruction
    /// </summary>
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        
        // Lua conditional jumps often follow this pattern when naively translated into the IR:
        //   if REGA == b then goto Label_1:
        //   goto Label_2:
        //   Label_1:
        //   ...
        //
        // This pass recognizes and simplifies this into:
        //   if REGA ~= b then goto Label_2:
        //   ...
        //
        // This will greatly simplify the generated control flow graph, so this is done first
        // This algorithm is run until convergence
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count - 2; i++)
            {
                // Pattern match the prerequisites
                if (b.Instructions[i] is ConditionalJumpLabel jmp1 &&
                    b.Instructions[i + 1] is JumpLabel jmp2 &&
                    b.Instructions[i + 2] is Label shortLabel && jmp1.Destination == shortLabel)
                {
                    // flip the condition and change the destination to the far jump. Then remove the following goto and label
                    if (jmp1.Condition is BinOp op)
                    {
                        op.NegateCondition();
                    }
                    else if (jmp1.Condition is UnaryOp { Operation: UnaryOp.OperationType.OpNot } or IdentifierReference)
                    {
                    }
                    else
                    {
                        throw new Exception("Recognized jump pattern does not use a binary op conditional");
                    }
                    jmp1.Destination.UsageCount--;
                    jmp1.Absorb(jmp2);
                    b.Instructions.RemoveRange(i + 1, jmp1.Destination.UsageCount <= 0 ? 2 : 1);
                    jmp1.Destination = jmp2.Destination;
                    changed = true;
                }
            }
        }

        return changed;
    }
}