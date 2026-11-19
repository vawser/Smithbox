using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

public class RewriteVarargListAssignmentPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count - 2; i++)
            {
                if (b.Instructions[i] is Assignment { Right: InitializerList { Expressions.Count: 0 } l1 } a1 &&
                    b.Instructions[i + 1] is Assignment { IsAmbiguousVararg: true } a2 && 
                    a1.VarargAssignmentReg == a2.VarargAssignmentReg - 1 &&
                    b.Instructions[i + 2] is Assignment { IsAmbiguousVararg: true } a3 && 
                    a3.VarargAssignmentReg == a1.VarargAssignmentReg)
                {
                    l1.AddTableElement(new Constant(Constant.ConstantType.ConstVarargs, -1), 
                        new Constant(Constant.ConstantType.ConstVarargs, -1));
                    a1.LocalAssignments = a3.LocalAssignments;
                    a1.Absorb(a2);
                    a1.Absorb(a3);
                    b.Instructions.RemoveRange(i + 1, 2);
                    changed = true;
                }
            }
        }

        return changed;
    }
}