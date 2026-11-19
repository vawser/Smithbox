using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Insert created labels into the instruction list
/// </summary>
public class ApplyLabelsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        bool changed = false;
        
        // O(n^2) naive algorithm but it hasn't been a problem yet
        foreach (var l in f.Labels)
        {
            foreach (var b in f.BlockList)
            {
                for (int i = 0; i < b.Instructions.Count; i++)
                {
                    if (b.Instructions[i].OpLocation == l.Key)
                    {
                        b.Instructions.Insert(i, l.Value);
                        changed = true;
                        break;
                    }
                }
            }
        }

        // Mark the implicit return lua always generates
        if (f.EndBlock.Instructions.Last() is Return { IsImplicit: false, ReturnExpressions.Count: 0 } r)
        {
            changed = true;
            r.IsImplicit = true;
        }

        return changed;
    }
}