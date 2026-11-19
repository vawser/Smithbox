using System;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Resolves function calls that have 0 as its "b", which means the actual arguments are determined by a
/// previous function with an indefinite return count being the last argument
/// </summary>
public class ResolveAmbiguousCallArguments : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        foreach (var b in f.BlockList)
        {
            Identifier? lastAmbiguousReturn = null;
            foreach (var i in b.Instructions)
            {
                switch (i)
                {
                    case Assignment { Right: FunctionCall { HasAmbiguousArgumentCount: true } fc2 } when lastAmbiguousReturn == null:
                        throw new Exception("Error: Ambiguous argument function call without preceding ambiguous return function call");
                    case Assignment { Right: FunctionCall { HasAmbiguousArgumentCount: true } fc2 }:
                    {
                        for (var r = fc2.BeginArg; r <= lastAmbiguousReturn.Value.RegNum; r++)
                        {
                            fc2.Args.Add(new IdentifierReference(f.GetRegister(r)));
                            changed = true;
                        }
                        lastAmbiguousReturn = null;
                        break;
                    }
                    case Return { ReturnExpressions: [FunctionCall { HasAmbiguousArgumentCount: true } fc2]} when lastAmbiguousReturn == null:
                        throw new Exception("Error: Ambiguous argument function tail call without preceding ambiguous return function call");
                    case Return { ReturnExpressions: [FunctionCall { HasAmbiguousArgumentCount: true } fc2]}:
                        for (var r = fc2.BeginArg; r <= lastAmbiguousReturn.Value.RegNum; r++)
                        {
                            fc2.Args.Add(new IdentifierReference(f.GetRegister(r)));
                            changed = true;
                        }
                        lastAmbiguousReturn = null;
                        break;
                    case Return { IsAmbiguousReturnCount: true } when lastAmbiguousReturn == null:
                        throw new Exception("Error: Ambiguous return without preceding ambiguous return function call");
                    case Return { IsAmbiguousReturnCount: true } ret:
                    {
                        for (var r = ret.BeginRet; r <= lastAmbiguousReturn.Value.RegNum; r++)
                        {
                            ret.ReturnExpressions.Add(new IdentifierReference(f.GetRegister(r)));
                            changed = true;
                        }

                        break;
                    }
                }

                if (i is Assignment 
                    { 
                        IsSingleAssignment: true, 
                        Left: IdentifierReference ir, 
                        Right: FunctionCall { HasAmbiguousReturnCount: true }
                    } a)
                {
                    lastAmbiguousReturn = ir.Identifier;
                }
            }
        }

        return changed;
    }
}