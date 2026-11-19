using System;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Validates that each jump instruction to a label has that label in the instruction list
/// </summary>
public class ValidateJumpDestinationLabelsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        foreach (var b in f.BlockList)
        {
            foreach (var t in b.Instructions)
            {
                if ((t is JumpLabel jmp && !b.Instructions.Contains(jmp.Destination)) ||
                     (t is ConditionalJumpLabel cJmp && !b.Instructions.Contains(cJmp)))
                {
                    throw new Exception("Control flow is corrupted");
                }
            }
        }

        return false;
    }
}