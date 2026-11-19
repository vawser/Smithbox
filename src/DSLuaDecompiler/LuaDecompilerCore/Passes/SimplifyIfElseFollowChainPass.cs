using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Sometimes, due to if edges always leading to returns, the follows selected aren't always the most optimal for clean lua generation,
/// even though they technically generate correct code. For example, you might get:
/// if a then
///     blah
/// elseif b then
///     blah
/// else
///     if c then
///         return
///     elseif d then
///         blah
///     end
/// end
/// 
/// This can be simplified into a single if else chain. The problem is since "if c then" leads to a return, there's never an explicit jump
/// to the last block, or the true logical follow. It becomes "orphaned" and is adopted by "elseif d then" as the follow. This pass detects such
/// cases and simplifies them.
/// </summary>
public class SimplifyIfElseFollowChainPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        
        bool IsIsolated(CFG.BasicBlock b, CFG.BasicBlock target)
        {
            var visited = new HashSet<CFG.BasicBlock>();
            var queue = new Queue<CFG.BasicBlock>();

            queue.Enqueue(b);
            visited.Add(b);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c == target)
                {
                    return false;
                }
                foreach (var successor in c.Successors)
                {
                    if (!visited.Contains(successor))
                    {
                        queue.Enqueue(successor);
                        visited.Add(successor);
                    }
                }
            }

            // No follow found
            return true;
        }

        // This relies on reverse postorder
        f.NumberReversePostorder();

        var processed = new HashSet<CFG.BasicBlock>();
        foreach (var b in f.PostorderTraversal(false))
        {
            var chain = new List<CFG.BasicBlock>();
            if (b.Follow != null)
            {
                var iter = b;
                var highestFollow = b.Follow;
                var highestFollowNumber = b.Follow.ReversePostorderNumber;
                chain.Add(b);
                while (!processed.Contains(iter) && iter.Successors.Count == 2 && 
                       iter.Follow == iter.EdgeFalse && iter.EdgeFalse.Instructions.Count == 1 && 
                       IsIsolated(iter.EdgeTrue, b.Follow)
                       && b.EdgeFalse != iter && iter.Follow.Predecessors.Count == 1)
                {
                    processed.Add(iter);
                    iter = iter.Follow;
                    chain.Add(iter);
                    if (iter.Follow != null && iter.Follow.ReversePostorderNumber > highestFollowNumber)
                    {
                        highestFollowNumber = iter.Follow.ReversePostorderNumber;
                        highestFollow = iter.Follow;
                    }
                }
                if (highestFollow != null && chain.Last().Successors.Count == 2)
                {
                    foreach (var c in chain)
                    {
                        var oldFollow = c.Follow;
                        var newFollow = chain.Last().Follow;

                        // Update any matching follows inside the dominance tree of the true branch
                        var toVisit = new Stack<CFG.BasicBlock>();
                        toVisit.Push(c.EdgeTrue);
                        while (toVisit.Count > 0)
                        {
                            var v = toVisit.Pop();
                            if (v.Follow == oldFollow)
                            {
                                v.Follow = newFollow;
                            }

                            dominance.RunOnDominanceTreeSuccessors(f, v, d =>
                            {
                                toVisit.Push(d);
                            });
                        }
                        c.Follow = newFollow;
                    }
                }
            }
            processed.Add(b);
        }

        return false;
    }
}