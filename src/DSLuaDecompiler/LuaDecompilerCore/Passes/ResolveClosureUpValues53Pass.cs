using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detect upvalue bindings for child closures in Lua 5.3
/// </summary>
public class ResolveClosureUpValues53Pass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        foreach (var c in f.Closures)
        {
            for (int i = 0; i < c.UpValueCount; i++)
            {
                if (c.UpValueIsStackBinding[i])
                {
                    c.UpValueBindings.Add(Identifier.GetRegister((uint)c.UpValueRegisterBinding[i]));
                }
                else
                {
                    c.UpValueBindings.Add(f.UpValueBindings[c.UpValueRegisterBinding[i]]);
                }
            }
        }

        /*foreach (var b in BlockList)
        {
            for (int i = 0; i < b.Instructions.Count(); i++)
            {
                // Recognize a closure instruction
                if (b.Instructions[i] is Assignment a && a.Right is Closure c)
                {
                    for (int j = 0; j < c.Function.UpvalCount; j++)
                    {
                        if (c.Function.UpvalueIsStackBinding[i])
                        {
                            
                        }
                        else
                        {
                            // Otherwise inherit the upvalue
                            c.Function.UpvalueBindings.Add(UpvalueBindings[f.UpvalueRegisterBinding[i]]);
                        }
                    }
                }
            }
        }*/

        return false;
    }
}