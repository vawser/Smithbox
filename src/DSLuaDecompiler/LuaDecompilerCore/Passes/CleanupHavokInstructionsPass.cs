using System.Diagnostics;
using System.Reflection.Metadata;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Removes Hks ops like Data which seem to be used for performance reasons but don't appear to affect the
/// decompilation result.
/// </summary>
public class CleanupHavokInstructionsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        bool changed = false;
        foreach (var b in f.BlockList)
        {
            for (var i = b.Instructions.Count - 1; i > 0; i--)
            {
                if (b.Instructions[i] is Data d1)
                {
                    if (b.Instructions[i - 1] is Data d2)
                    {
                        d2.Locals = d1.Locals;
                    }
                    else if (b.Instructions[i - 1] is Assignment a)
                    {
                        a.LocalAssignments = d1.Locals;
                        a.Absorb(d1);
                    }

                    changed = true;
                    b.Instructions.RemoveAt(i);
                    i++;
                }
            }
        }

        return changed;
    }
}