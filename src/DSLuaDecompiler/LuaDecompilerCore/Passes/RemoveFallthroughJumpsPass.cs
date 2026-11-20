using System.Collections.Generic;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Smithbox: runs after build-ast. A jump left at the end of an if branch to the if's follow (or, for a branch of an
/// elseif chain, to the follow the chain falls through to) is a fall-through: the branch ends there and control reaches
/// that block anyway. Build-ast keeps such jumps to detect empty elses and removes them as it does; one it leaves would
/// be printed as "goto basicblock_N", which Lua 5.0 can't compile. Seen where an elseif's condition was merged from a
/// nested if and the chain ends in an explicit else (SDT m11_02_00_00 102010_battle Kengeki_Activate, BB m34_00_00_00
/// 451000_battle ActAfter_RealTime). Only jumps that would otherwise be printed are removed, so output that compiles is
/// unchanged. Jumps inside a loop body are left alone (a jump there can be to the loop head).
/// </summary>
public class RemoveFallthroughJumpsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var visited = new HashSet<BasicBlock>();
        Walk(f.BeginBlock, null, visited);
        return false;
    }

    /// <param name="follow">The block control reaches when this block's scope ends; null if unknown (a loop body, the function body)</param>
    private static void Walk(BasicBlock? b, BasicBlock? follow, HashSet<BasicBlock> visited)
    {
        if (b == null || !visited.Add(b))
            return;

        if (follow != null && b is { HasInstructions: true, Last: Jump j } && j.Destination == follow)
        {
            b.Instructions.RemoveAt(b.Instructions.Count - 1);
            return;
        }

        foreach (var inst in b.Instructions)
        {
            switch (inst)
            {
                case IfStatement i:
                    var branchFollow = i.Follow ?? follow;
                    Walk(i.True, branchFollow, visited);
                    Walk(i.False, branchFollow, visited);
                    Walk(i.Follow, follow, visited);
                    break;
                case While w:
                    Walk(w.Body, null, visited);
                    Walk(w.Follow, follow, visited);
                    break;
                case GenericFor g:
                    Walk(g.Body, null, visited);
                    Walk(g.Follow, follow, visited);
                    break;
                case NumericFor n:
                    Walk(n.Body, null, visited);
                    Walk(n.Follow, follow, visited);
                    break;
            }
        }
    }
}
