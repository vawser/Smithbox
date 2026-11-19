using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Optimizes out jumps to jumps, and deletes labels too if they become unused as a result
/// </summary>
public class PeepholeOptimizationPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                if (b.Instructions[i] is not IJumpLabel jmp1) continue;
                var dest = b.Instructions[b.Instructions.IndexOf(jmp1.Destination) + 1];
                while (dest is JumpLabel jmp2)
                {
                    jmp1.Destination.UsageCount--;
                    if (jmp1.Destination.UsageCount <= 0)
                    {
                        b.Instructions.Remove(jmp1.Destination);
                    }
                    jmp1.Destination = jmp2.Destination;
                    dest = b.Instructions[b.Instructions.IndexOf(jmp1.Destination) + 1];
                    changed = true;
                }
            }
        }

        return changed;
    }
}