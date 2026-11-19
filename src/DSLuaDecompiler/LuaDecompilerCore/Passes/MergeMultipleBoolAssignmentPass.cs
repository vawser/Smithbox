using System.Diagnostics;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// HKS as an optimization seems to optimize the following:
///     local a, b, c, d = false
/// as two instructions: LOADBOOL followed by LOADNIL. It seems that
/// LOADNIL can recognize when it's preceded by LOADBOOL and load a bool
/// instead of a nil into the register. This pass recognizes this idiom and
/// merges them back together. This only works when the bool is false as it's unknown
/// if it does this for true as well.
/// </summary>
public class MergeMultipleBoolAssignmentPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count - 2; i++)
            {
                if (b.Instructions[i] is Assignment 
                    { 
                        LeftAny: true, 
                        Left: IdentifierReference, 
                        Right: Constant { ConstType: Constant.ConstantType.ConstBool, Boolean: false }
                    } a1 &&
                    b.Instructions[i + 1] is Assignment
                    { 
                        LeftAny: true, 
                        Left: IdentifierReference, 
                        Right: Constant { ConstType: Constant.ConstantType.ConstNil }
                    } a2)
                {
                    a1.LeftList.AddRange(a2.LeftList);
                    if (a1.LocalAssignments == null)
                    {
                        a1.LocalAssignments = a2.LocalAssignments;
                    }
                    else
                    {
                        Debug.Assert(a2.LocalAssignments != null);
                        a1.LocalAssignments.AddRange(a2.LocalAssignments);
                    }
                    a1.Absorb(a2);
                    b.Instructions.RemoveAt(i + 1);
                    changed = true;
                }
            }
        }

        return changed;
    }
}