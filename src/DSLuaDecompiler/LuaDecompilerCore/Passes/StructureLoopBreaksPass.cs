using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Lua supports "break" statements within a loop. In the bytecode, these manifest as jumps to the loop's follow within
/// the loop. The goal of this pass is to replace these jumps with explicit break instructions and rewrite the control
/// flow such that it is structured.
/// </summary>
public class StructureLoopBreaksPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        var visited = new HashSet<BasicBlock>();
        var toRemove = new List<BasicBlock>();
        void Visit(BasicBlock b, BasicBlock? loopHead)
        {
            visited.Add(b);
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                if (visited.Contains(successor))
                    return;
                Visit(successor, b.IsLoopHead && b.LoopFollow != successor ? b : loopHead);
            });
            
            // Loop heads won't contain any breaks
            if (b.IsLoopHead || loopHead?.LoopFollow == null)
                return;
            
            // Due to Lua compiler optimizations the loop follow might jump to another block and breaks will go to that
            // block instead. We need to take those targets into account too.
            var followTarget = loopHead.LoopFollow.PeepholeSuccessor();
            
            // An unconditional jump to the loop follow can be replaced with a break and rewriting the CFG to fall
            // through to the next block
            if (b.Successors.Count == 1 && b.Instructions.Count > 0 && b.Last is Jump j &&
                (b.Successors[0] == loopHead.LoopFollow || b.Successors[0] == followTarget))
            {
                b.Last = new Break();
                b.Last.Absorb(j);
                var next = f.NextBlock(b) ?? throw new Exception("Next is null?");
                b.ChangeSuccessor(0, next);
                changed = true;
            }
            
            // A block with an unreachable successor may actually be a break, in which case we can replace with a break
            // and merge the following block in
            if (b is { UnreachableSuccessor: true, Last: ConditionalJump cj } && 
                (b.EdgeFalse == loopHead.LoopFollow || b.EdgeFalse == followTarget))
            {
                b.Last = new Break();
                b.Last.Absorb(cj);
                var toMerge = b.EdgeTrue;
                b.ClearSuccessors();
                b.StealSuccessors(toMerge);
                b.AbsorbInstructions(toMerge, true);
                toRemove.Add(toMerge);
                
                // Workaround for now but we really should be able to reanalyze loops
                if (b.Successors.Contains(loopHead))
                {
                    b.IsLoopLatch = true;
                    loopHead.LoopLatches.Add(b);
                }

                changed = true;
            }
        }
        Visit(f.BeginBlock, null);

        foreach (var b in toRemove)
        {
            f.RemoveBlockAt(b.BlockIndex);
        }

        if (changed)
        {
            functionContext.InvalidateAnalysis<DominanceAnalyzer>();
        }
        
        return changed;
    }
}