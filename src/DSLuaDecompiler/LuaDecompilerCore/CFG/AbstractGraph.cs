using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LuaDecompilerCore.CFG
{
    /// <summary>
    /// A minimalist graph for encoding control flow graphs. Intended to be used for interval analysis
    /// </summary>
    public sealed class AbstractGraph
    {
        public required Node BeginNode;
        public List<Node> Nodes = new();
        public Dictionary<Node, HashSet<Node>>? Intervals = null;

        public readonly Dictionary<Node, Node?> LoopHeads = new();
        public readonly Dictionary<Node, List<Node>> LoopLatches = new();
        public readonly Dictionary<Node, Node?> LoopFollows = new();
        public readonly Dictionary<Node, LoopType> LoopTypes = new();

        public class Node : IComparable<Node>
        {
            public readonly List<Node> Successors = new List<Node>();
            public readonly List<Node> Predecessors = new List<Node>();

            /// <summary>
            /// Child in the successor graph. Only set if this is an interval graph head
            /// </summary>
            public Node? IntervalGraphChild = null;

            /// <summary>
            /// The interval head in the parent graph that collapses into this node
            /// </summary>
            public Node? IntervalGraphParent = null;

            /// <summary>
            /// Pointer to the interval set this node belongs to
            /// </summary>
            public HashSet<Node>? Interval = null;

            /// <summary>
            /// The basic block that this node ultimately maps to
            /// </summary>
            public required BasicBlock OriginalBlock;

            /// <summary>
            /// Node is marked as being part of a loop
            /// </summary>
            public bool InLoop = false;

            public bool IsHead = false;

            /// <summary>
            /// Node is the terminal node (where returns jump to)
            /// </summary>
            public bool IsTerminal = false;

            // ReSharper disable once InvalidXmlDocComment
            /// <summary>
            /// The node that's the predecessor to the loop follow
            /// </summary>
            //public Node FollowLeader = null;

            /// <summary>
            /// Number in this graph based on the reverse postorder traversal number
            /// </summary>
            public int ReversePostorderNumber = 0;

            public int CompareTo(Node? other)
            {
                if (other != null && other.ReversePostorderNumber > ReversePostorderNumber)
                    return -1;
                if (other != null && other.ReversePostorderNumber == ReversePostorderNumber)
                    return 0;
                return 1;
            }
        }

        private List<Node> PostorderTraversal(bool reverse)
        {
            var ret = new List<Node>();
            var visited = new HashSet<Node>();

            void Visit(Node b)
            {
                visited.Add(b);
                foreach (var successor in b.Successors.OrderByDescending(n => n.OriginalBlock.BlockId))
                {
                    if (!visited.Contains(successor))
                    {
                        Visit(successor);
                    }
                }
                ret.Add(b);
            }

            Visit(BeginNode);

            if (reverse)
            {
                ret.Reverse();
            }
            return ret;
        }

        public void LabelReversePostorderNumbers()
        {
            var postorder = PostorderTraversal(true);
            for (var i = 0; i < postorder.Count; i++)
            {
                postorder[i].ReversePostorderNumber = i;
            }
        }

        // Thanks @thefifthmatt :forestcat:
        public void CalculateIntervals()
        {
            Intervals = new Dictionary<Node, HashSet<Node>>();
            var headers = new HashSet<Node>(Nodes.Count) { BeginNode };
            var toAdd = new HashSet<Node>(10);
            while (headers.Count > 0)
            {
                var h = headers.First();
                headers.Remove(h);
                var interval = new HashSet<Node>(Nodes.Count / 8) { h };
                Intervals.Add(h, interval);
                h.Interval = interval;
                var lastCount = 0;
                while (lastCount != interval.Count)
                {
                    lastCount = interval.Count;
                    toAdd.Clear();
                    foreach (var start in interval)
                    {
                        foreach (var candidate in start.Successors)
                        {
                            bool allContains = true;
                            foreach (var predecessor in candidate.Predecessors)
                            {
                                allContains = allContains && interval.Contains(predecessor);
                            }
                            if (allContains && !Intervals.ContainsKey(candidate))
                            {
                                toAdd.Add(candidate);
                                candidate.Interval = interval;
                            }
                        }
                    }
                    interval.UnionWith(toAdd);
                }
                foreach (var candidate in interval)
                {
                    foreach (var successor in candidate.Successors)
                    {
                        if (!interval.Contains(successor) && !Intervals.ContainsKey(successor))
                        {
                            headers.Add(successor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get a collapsed version of the graph where interval heads become nodes
        /// </summary>
        /// <returns>A new collapsed graph</returns>
        public AbstractGraph? GetIntervalSubgraph()
        {
            if (Intervals == null)
            {
                CalculateIntervals();
                Debug.Assert(Intervals != null);
            }
            
            if (Intervals.Count == Nodes.Count || Intervals.Values.All(i => i.Count == 1))
            {
                return null;
            }
            
            var nodes = new List<Node>();
            foreach (var n in Intervals.Keys)
            {
                var node = new Node
                {
                    IntervalGraphParent = n,
                    OriginalBlock = n.OriginalBlock
                };
                n.IntervalGraphChild = node;
                nodes.Add(node);
            }

            var header = Intervals
                .SelectMany(e => e.Value.Select(i => (i, e.Key)))
                .ToDictionary(i => i.i, i => i.Key);
            foreach (var entry in Nodes)
            {
                if (!header.ContainsKey(entry))
                {
                    continue;
                }
                var h1 = header[entry];
                var headerNode = header[entry].IntervalGraphChild ?? throw new Exception();
                var nodesToAdd = entry.Successors
                    .Select(n => header[n])
                    .Where(h => h != h1)
                    .Select(n => n.IntervalGraphChild ?? throw new Exception());
                foreach (var n in nodesToAdd)
                {
                    if (!headerNode.Successors.Contains(n))
                    {
                        headerNode.Successors.Add(n);
                    }
                }
            }
            foreach (var entry in nodes)
            {
                foreach (var successor in entry.Successors)
                {
                    successor.Predecessors.Add(entry);
                }
            }

            var subgraph = new AbstractGraph
            {
                Nodes = nodes,
                BeginNode = nodes.First(x => x.Predecessors.Count == 0)
            };
            subgraph.LabelReversePostorderNumbers();
            subgraph.CalculateIntervals();
            return subgraph;
        }
    }
}
