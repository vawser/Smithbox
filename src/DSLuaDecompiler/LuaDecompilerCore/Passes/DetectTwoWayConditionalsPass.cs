using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detects and labels two way statements (if statements)
/// </summary>
public class DetectTwoWayConditionalsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var debugVisited = new HashSet<CFG.BasicBlock>();
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        HashSet<CFG.BasicBlock> Visit(CFG.BasicBlock b)
        {
            var unresolved = new HashSet<CFG.BasicBlock>();
            var unresolved2 = new HashSet<CFG.BasicBlock>();
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                if (debugVisited.Contains(successor))
                {
                    throw new Exception("Revisited dom tree node " + successor);
                }

                debugVisited.Add(successor);
                unresolved.UnionWith(Visit(successor));
            });

            if (b.IsConditionalJump && (!b.IsLoopHead || b.LoopType != CFG.LoopType.LoopPretested))
            {
                // The follow of a conditional block is the block that a conditional if statement converges
                // on after the if and the optional else cause a control flow divergence
                CFG.BasicBlock? followCandidate = null;
                
                // The initial candidate for the follow is the block that we immediately dominate with the
                // most incoming blocks
                var maxPredecessors = 0;
                dominance.RunOnDominanceTreeSuccessors(f, b, d =>
                {
                    // At least two incoming blocks are required for a block to be a follow
                    var predecessorsRequired = 2;

                    if (d.Predecessors.Count >= predecessorsRequired && d.Predecessors.Count > maxPredecessors &&
                        d != f.EndBlock)
                    {
                        maxPredecessors = d.Predecessors.Count;
                        followCandidate = d;
                    }
                });
                
                
                // Heuristic: If the true branch leads to a return or is if-orphaned and the follow isn't defined already,
                // then the follow is always the false branch.
                // If the true branch also has a follow chain defined that leads to a return or if-orphaned node,
                // then it is also disjoint from the rest of the CFG and the false branch is the follow
                var isDisjoint = false;
                var testFollow = b.EdgeTrue.Follow;
                while (testFollow != null)
                {
                    if (testFollow.IsReturn || testFollow.IfOrphaned)
                    {
                        isDisjoint = true;
                        break;
                    }
                    testFollow = testFollow.Follow;
                }
                
                if (followCandidate == null && (b.EdgeTrue.IsReturn || b.EdgeTrue.IfOrphaned || isDisjoint))
                {
                    // If the false branch leads to an isolated return node or an if-orphaned node, then we are if-orphaned,
                    // which essentially means we don't have a follow defined in the CFG. This means that to structure this,
                    // the if-orphaned node must be adopted by the next node with a CFG determined follow and this node will
                    // inherit that follow.
                    if (b.EdgeFalse is { IsReturn: true, Predecessors.Count: 1 } || b.EdgeFalse.IfOrphaned)
                    {
                        b.IfOrphaned = true;
                    }
                    else
                    {
                        followCandidate = b.EdgeFalse;
                    }
                }
                
                // If you don't match anything, but you dominate the end node, then it's probably the follow
                if (followCandidate == null && dominance.DominanceTreeSuccessors(b.BlockIndex).Contains((uint)f.EndBlock.BlockIndex))
                {
                    followCandidate = f.EndBlock;
                }

                // If we are a latch and the false node leads to a loop head, then the follow is the loop head
                if (followCandidate == null && b is { IsLoopLatch: true, EdgeFalse.IsLoopHead: true })
                {
                    followCandidate = b.EdgeFalse;
                }

                if (followCandidate != null)
                {
                    b.Follow = followCandidate;
                    var unresolvedClone = new HashSet<CFG.BasicBlock>(unresolved);
                    foreach (var x in unresolvedClone)
                    {
                        if (x != followCandidate && !dominance.Dominance(x.BlockIndex).Contains((uint)followCandidate.BlockIndex))
                        {
                            var inc = dominance.DominanceTreeSuccessors(x.BlockIndex).Length == 0;
                            // Do a BFS down the dominance hierarchy to search for a follow
                            var bfsQueue = new Queue<uint>();
                            foreach (var d in dominance.DominanceTreeSuccessors(x.BlockIndex))
                                bfsQueue.Enqueue(d);
                            //foreach (var domsucc in x.DominanceTreeSuccessors)
                            //{
                            while (bfsQueue.Count > 0)
                            {
                                var dominanceSuccessor = f.BlockList[(int)bfsQueue.Dequeue()];
                                if (dominanceSuccessor.Successors.Contains(followCandidate) || dominanceSuccessor.Follow == followCandidate)
                                {
                                    inc = true;
                                    break;
                                }
                                dominance.RunOnDominanceTreeSuccessors(f, dominanceSuccessor, s => bfsQueue.Enqueue((uint)s.BlockIndex));
                            }
                            //}
                            if (x.IfOrphaned)
                            {
                                inc = true;
                            }
                            if (inc)
                            {
                                x.Follow = followCandidate;
                                unresolved.Remove(x);
                            }
                        }
                    }
                }
                else
                {
                    unresolved.Add(b);
                }
            }

            // The loop head or latch is the implicit follow of any unmatched conditionals
            if (b.IsLoopHead)
            {
                unresolved2.Clear();
                foreach (var ur in unresolved)
                {
                    // If the unresolved block is the follow of the loop then the loop head is not the follow. This may
                    // need to be updated to use the interval information and exclude blocks that occur outside the
                    // loop's interval.
                    //if (b.LoopFollow == ur)
                    if (b.LoopFollow is not null && ur.BlockId >= b.LoopFollow.BlockId)
                    {
                        unresolved2.Add(ur);
                        continue;
                    }

                    // If there's a single loop latch and it has multiple predecessors, it's probably the follow
                    if (b.LoopLatches is [{ Predecessors.Count: > 1 }])
                    {
                        ur.Follow = b.LoopLatches[0];
                    }
                    // Otherwise the detected latch (of multiple) is probably within an if statement and the head is the
                    // true follow
                    else
                    {
                        ur.Follow = b;
                    }
                }
                unresolved.Clear();
                unresolved = unresolved2;
            }

            return unresolved;
        }

        // Unsure about this logic, but the idea is that an if chain at the end that only returns will be left unmatched and unadopted,
        // and thus the follows need to be the end blocks
        var unmatched = Visit(f.BeginBlock);
        foreach (var u in unmatched)
        {
            u.Follow = f.EndBlock;
        }

        return false;
    }
}