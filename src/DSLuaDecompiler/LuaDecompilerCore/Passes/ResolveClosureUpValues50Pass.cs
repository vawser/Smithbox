using System;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detect the upvalue bindings for the child closures for Lua 5.0
/// </summary>
public class ResolveClosureUpValues50Pass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                // Recognize a closure instruction
                if (b.Instructions[i] is Assignment { Right: Closure c } a)
                {
                    // Fetch the closure bindings from the following instructions
                    for (var j = 0; j < c.Function.UpValueCount; j++)
                    {
                        if (b.Instructions[i + 1] is ClosureBinding binding)
                        {
                            c.Function.UpValueBindings.Add(binding.Identifier);
                            a.Absorb(binding);
                            b.Instructions.RemoveAt(i + 1);
                            changed = true;
                        }
                        else
                        {
                            throw new Exception("Unrecognized up-value binding pattern following closure");
                        }
                    }
                }
            }
        }

        return changed;
    }
}