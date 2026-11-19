using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detects and labels blocks involved in looping
/// </summary>
public class DetectLoopsPass : IPass
{
    private void AddLoopLatch(CFG.AbstractGraph graph, CFG.AbstractGraph.Node head, CFG.AbstractGraph.Node latch, HashSet<CFG.AbstractGraph.Node> interval)
    {
        if (!graph.LoopLatches.ContainsKey(latch))
        {
            graph.LoopLatches.Add(latch, new List<CFG.AbstractGraph.Node>());
        }
        graph.LoopLatches[latch].Add(head);

        var loopNodes = new HashSet<CFG.AbstractGraph.Node>();

        // Analyze the loop to determine the beginning of the range of postordered nodes that represent the loop
        var beginNum = head.ReversePostorderNumber;
        /*foreach (var succ in head.Successors)
        {
            if (succ.ReversePostorderNumber > beginNum && succ.ReversePostorderNumber <= latch.ReversePostorderNumber)
            {
                beginNum = succ.ReversePostorderNumber;
            }
        }
        if (beginNum == -1)
        {
            throw new Exception("Bad loop analysis");
        }*/

        //loopNodes.Add(head);
        //head.InLoop = true;
        head.IsHead = true;
        graph.LoopHeads[head] = head;
        foreach (var l in interval
                     .Where(x => x.ReversePostorderNumber >= beginNum && 
                                 x.ReversePostorderNumber <= latch.ReversePostorderNumber))
        {
            loopNodes.Add(l);
            l.InLoop = true;
        }

        if (!graph.LoopFollows.ContainsKey(head))
        {
            CFG.LoopType type;
            if (head.Successors.Any(next => !loopNodes.Contains(next)))
            {
                type = CFG.LoopType.LoopPretested;
            }
            else
            {
                type = latch.Successors.Any(next => !loopNodes.Contains(next)) ? CFG.LoopType.LoopPosttested : CFG.LoopType.LoopEndless;
            }
            graph.LoopTypes[head] = type;
            List<CFG.AbstractGraph.Node> follows = type switch
            {
                CFG.LoopType.LoopPretested => head.Successors.Where(next => !loopNodes.Contains(next)).ToList(),
                CFG.LoopType.LoopPosttested => latch.Successors.Where(next => !loopNodes.Contains(next)).ToList(),
                _ => loopNodes
                    .SelectMany(loopNode => loopNode.Successors
                        .Where(next => next.ReversePostorderNumber > latch.ReversePostorderNumber))
                    .ToList()
            };

            if (follows.Count == 0 && type != CFG.LoopType.LoopEndless)
            {
                throw new Exception("No follow for loop found");
            }
            
            graph.LoopFollows[head] = follows.Count > 0 ? 
                follows.OrderBy(candidate => candidate.ReversePostorderNumber).First() : null;
        }
    }

    private void DetectLoopsForIntervalLevel(CFG.AbstractGraph graph)
    {
        Debug.Assert(graph.Intervals != null);
        foreach (var (head, intervalNodes) in graph.Intervals)
        {
            var latches = head.Predecessors
                .Where(p => intervalNodes.Contains(p))
                .OrderBy(p => p.ReversePostorderNumber)
                .ToList();
            foreach (var latch in latches)
            {
                AddLoopLatch(graph, head, latch, intervalNodes);
            }
        }
        var subgraph = graph.GetIntervalSubgraph();
        if (subgraph != null)
        {
            DetectLoopsForIntervalLevel(subgraph);
            foreach (var entry in subgraph.LoopLatches)
            {
                var parentInterval = entry.Key.IntervalGraphParent?.Interval ?? throw new Exception();
                foreach (var head in entry.Value)
                {
                    var latches = head.IntervalGraphParent?.Predecessors
                        .Where(p => parentInterval.Contains(p))
                        .ToList() ?? throw new Exception();
                    var headersInLoop = subgraph.LoopHeads
                        .Where(e => e.Value == head)
                        .Select(e => e.Key.IntervalGraphParent)
                        .ToList();
                    var intervalsInLoop = new HashSet<CFG.AbstractGraph.Node>(graph.Intervals.Where(e => (headersInLoop.Contains(e.Key)/* || e.Value == parentInterval*/)).SelectMany(e => e.Value));
                    foreach (var latch in latches)
                    {
                        AddLoopLatch(graph, head.IntervalGraphParent, latch, intervalsInLoop);
                    }
                }
            }
        }
    }
        
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        // Build an abstract graph to analyze with
        var blockIdMap = new Dictionary<CFG.BasicBlock, int>();
        var abstractNodes = new List<CFG.AbstractGraph.Node>();
        for (var i = 0; i < f.BlockList.Count; i++)
        {
            blockIdMap.Add(f.BlockList[i], i);
            var node = new CFG.AbstractGraph.Node
            {
                OriginalBlock = f.BlockList[i],
                IsTerminal = i == f.BlockList.Count - 1
            };
            abstractNodes.Add(node);
        }
        foreach (var b in blockIdMap)
        {
            foreach (var pred in b.Key.Predecessors)
            {
                abstractNodes[b.Value].Predecessors.Add(abstractNodes[blockIdMap[pred]]);
            }
            foreach (var successor in b.Key.Successors)
            {
                abstractNodes[b.Value].Successors.Add(abstractNodes[blockIdMap[successor]]);
            }
        }

        // Calculate intervals and the graph sequence in preparation for loop detection
        var headGraph = new CFG.AbstractGraph
        {
            BeginNode = abstractNodes[blockIdMap[f.BeginBlock]],
            Nodes = abstractNodes
        };
        headGraph.CalculateIntervals();
        headGraph.LabelReversePostorderNumbers();

        DetectLoopsForIntervalLevel(headGraph);

        foreach (var latch in headGraph.LoopLatches)
        {
            foreach (var head in latch.Value)
            {
                var b = head.OriginalBlock;
                b.IsLoopHead = true;
                b.LoopLatches.Add(latch.Key.OriginalBlock);
                b.LoopType = headGraph.LoopTypes[head];
                if(headGraph.LoopFollows[head] == null)
                    continue;
                b.LoopFollow = headGraph.LoopFollows[head]?.OriginalBlock ?? throw new Exception();
                latch.Key.OriginalBlock.IsLoopLatch = true;
            }
        }

        return false;
    }
}