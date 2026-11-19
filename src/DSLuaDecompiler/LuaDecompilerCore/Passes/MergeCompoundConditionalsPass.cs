using System;
using System.Diagnostics;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Merge conditionals that follow short circuit patterns into compound conditionals (like "if a and b")
/// </summary>
public class MergeCompoundConditionalsPass : IPass
{
    public bool MutatesCfg => true;
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var irChanged = false;
        var changed = true;
        while (changed)
        {
            changed = false;
            f.NumberReversePostorder();
            foreach (var node in f.PostorderTraversal(false))
            {
                if (node is 
                    { 
                        HasInstructions: true, 
                        Last: ConditionalJump 
                        { 
                            Condition: BinOp 
                            {
                                Operation: BinOp.OperationType.OpLoopCompare
                            }
                        }
                    })
                {
                    continue;
                }
                if (node is
                    {
                        IsConditionalJump: true, 
                        Last: ConditionalJump n
                    })
                {
                    var t = node.EdgeTrue;
                    var e = node.EdgeFalse;
                    if (t is { IsConditionalJump: true, First: ConditionalJump tj, Predecessors.Count: 1 })
                    {
                        var trueTruePeepholeSuccessor = t.EdgeTrue.PeepholeSuccessor();
                        if ((t.EdgeTrue == e || trueTruePeepholeSuccessor == e) && t.EdgeFalse != e)
                        {
                            // If there was a peephole optimization that "broke" the CFG, attempt to restore it so
                            // CFG collapsing will work properly
                            if (trueTruePeepholeSuccessor != t.EdgeTrue)
                            {
                                node.EdgeFalse = t.EdgeTrue;
                                e = t.EdgeTrue;
                            }
                            
                            Expression newCond;
                            if (n.Condition is BinOp { IsCompare: true } b)
                            {
                                b.NegateConditionalExpression();
                                newCond = new BinOp(b, tj.Condition, BinOp.OperationType.OpOr)
                                {
                                    IsMergedCompoundConditional = true
                                };
                            }
                            else
                            {
                                var left = n.Condition;
                                if (n.Condition is UnaryOp { Operation: UnaryOp.OperationType.OpNot } unaryOp)
                                    left = unaryOp.Expression;
                                else if (n.Condition is BinOp binOp)
                                    binOp.NegateConditionalExpression();
                                else
                                    left = new UnaryOp(n.Condition, UnaryOp.OperationType.OpNot) { IsImplicit = true };
                                newCond = new BinOp(left, tj.Condition, BinOp.OperationType.OpOr)
                                {
                                    IsMergedCompoundConditional = true
                                };
                            }
                            n.Condition = newCond;
                            n.Absorb(tj);
                            if (t.Follow != null)
                            {
                                Debug.Assert(node.Follow != null);
                                node.Follow = node.Follow.ReversePostorderNumber > t.Follow.ReversePostorderNumber ? 
                                    node.Follow : t.Follow;
                            }
                            node.EdgeFalse = t.EdgeFalse;
                            n.Destination = node.EdgeFalse;
                            node.EdgeTrue = e;
                            t.ClearSuccessors();
                            f.RemoveBlockAt(t.BlockIndex);
                            irChanged = true;
                            changed = true;
                        }
                        else if (t.EdgeFalse == e)
                        {
                            var newCond = new BinOp(n.Condition, tj.Condition, BinOp.OperationType.OpAnd)
                            {
                                IsMergedCompoundConditional = true
                            };
                            n.Condition = newCond;
                            n.Absorb(tj);
                            if (t.Follow != null)
                            {
                                Debug.Assert(node.Follow != null);
                                node.Follow = node.Follow.ReversePostorderNumber > t.Follow.ReversePostorderNumber ? 
                                    node.Follow : t.Follow;
                            }
                            node.EdgeTrue = t.EdgeTrue;
                            t.ClearSuccessors();
                            f.RemoveBlockAt(t.BlockIndex);
                            irChanged = true;
                            changed = true;
                        }
                    }
                    else if (e is { IsConditionalJump: true, First: ConditionalJump ej, Predecessors.Count: 1 })
                    {
                        if (e.EdgeTrue == t)
                        {
                            var left = n.Condition;
                            if (n.Condition is UnaryOp { Operation: UnaryOp.OperationType.OpNot } unaryOp)
                                left = unaryOp.Expression;
                            else if (n.Condition is BinOp binOp)
                                binOp.NegateConditionalExpression();
                            else
                                left = new UnaryOp(n.Condition, UnaryOp.OperationType.OpNot) { IsImplicit = true };
                            var newCond = new BinOp(left, ej.Condition, BinOp.OperationType.OpOr)
                            {
                                IsMergedCompoundConditional = true
                            };
                            n.Condition = newCond;
                            n.Absorb(ej);
                            if (e.Follow != null)
                            {
                                Debug.Assert(node.Follow != null);
                                node.Follow = node.Follow.ReversePostorderNumber > e.Follow.ReversePostorderNumber ? 
                                    node.Follow : e.Follow;
                            }
                            node.EdgeFalse = e.EdgeFalse;
                            n.Destination = node.EdgeFalse;
                            e.ClearSuccessors();
                            f.RemoveBlockAt(e.BlockIndex);
                            irChanged = true;
                            changed = true;
                        }
                        else if (e.EdgeFalse == t)
                        {
                            throw new Exception("Unused compound CFG pattern");
                        }
                    }
                }
            }
        }
        functionContext.InvalidateAnalysis<DominanceAnalyzer>();

        return irChanged;
    }
}