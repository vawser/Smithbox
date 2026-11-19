using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Transform from the control flow graph representation of the function to an abstract syntax tree of the
/// Lua program which can then be printed out.
/// </summary>
public class AstTransformPass : IPass
{
    private static BasicBlock LastBlockInScope(BasicBlock b)
    {
        while (b.Instructions.Count > 0)
        {
            switch (b.Last)
            {
                case IfStatement { Follow: { } f }:
                    b = f;
                    continue;
                case IfStatement { Follow: null, False: { } e}:
                    b = e;
                    continue;
                case IfStatement { Follow: null, False: null, True: { } t}:
                    b = t;
                    continue;
                case While { Follow: { } wf }:
                    b = wf;
                    continue;
                case GenericFor { Follow: { } gf }:
                    b = gf;
                    continue;
                case NumericFor { Follow: { } nf }:
                    b = nf;
                    continue;
            }
            break;
        }

        return b;
    }
    
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        if (f.IsAst)
            return false;
        
        // Traverse all the nodes in post-order and try to convert jumps to if statements
        var usedFollows = new HashSet<BasicBlock>();

        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();

        void TransformNumericFor(BasicBlock node, BasicBlock loopInitializer)
        {
            // If the loop instruction hasn't had its jump to the loop head removed yet then go ahead and do that
            if (loopInitializer.Instructions[^1] is IJump)
                loopInitializer.Instructions.RemoveAt(loopInitializer.Instructions.Count - 1);
                    
            Assignment incrementInstruction = node.Instructions[^2] as Assignment ?? throw new Exception();
            Expression increment = (incrementInstruction.Right as BinOp)?.Right ?? throw new Exception();

            // Lua 5.0 has a sub instruction at the end of the initializer that needs to be removed
            if (loopInitializer.GetInstruction(loopInitializer.Instructions.Count - 1) is Assignment
                {
                    IsSingleAssignment: true,
                    Left: IdentifierReference { Identifier: { IsRegister: true } id1 },
                    Right: BinOp
                    {
                        Operation: BinOp.OperationType.OpSub,
                        Left: IdentifierReference { Identifier: { IsRegister: true } id2 },
                        Right: IdentifierReference { Identifier: { IsRegister: true } id3 }
                    }
                } && id1.RegNum == id2.RegNum && id1.RegNum + 2 == id3.RegNum)
            {
                loopInitializer.Instructions.RemoveAt(loopInitializer.Instructions.Count - 1);
            }

            // Extract the step variable definition
            if (loopInitializer.GetInstruction(loopInitializer.Instructions.Count - 1) is 
                Assignment { Right: not null } incrementAssignment)
            {
                increment = incrementAssignment.Right;
                loopInitializer.Instructions.RemoveAt(loopInitializer.Instructions.Count - 1);
            }

            // Extract the limit variable definition
            Expression? limit = null;
            if (loopInitializer.GetInstruction(loopInitializer.Instructions.Count - 1) is 
                Assignment limitAssignment)
            {
                limit = limitAssignment.Right;
                loopInitializer.Instructions.RemoveAt(loopInitializer.Instructions.Count - 1);
            }

            // Extract the initializer variable definition
            Assignment? initial = null;
            if (loopInitializer.GetInstruction(loopInitializer.Instructions.Count - 1) is 
                Assignment initAssignment)
            {
                initial = initAssignment;
                loopInitializer.Instructions.RemoveAt(loopInitializer.Instructions.Count - 1);
            }

            var body = node.EdgeFalse;
            body.MarkCodeGenerated(f.FunctionId, f.Warnings);
            BasicBlock? follow = null;
            Debug.Assert(node.LoopFollow != null);
            if (!usedFollows.Contains(node.LoopFollow))
            {
                follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }

            var numericFor = new NumericFor(initial, limit, increment, body, follow);
            if (loopInitializer.GetInstruction(loopInitializer.Instructions.Count - 1) is IJump)
            {
                loopInitializer.Instructions[^1] = numericFor;
            }
            else
            {
                loopInitializer.Instructions.Add(numericFor);
            }
            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);

            // Remove any jump instructions from the latches if they exist
            foreach (var latch in node.LoopLatches)
            {
                if (latch is { HasInstructions: true, IsConditionalBranch: false, Last: Jump jmp2 } && 
                    jmp2.Destination == node)
                {
                    latch.Instructions.RemoveAt(latch.Instructions.Count - 1);
                }
            }
        }

        void TransformGenericFor(BasicBlock node, BasicBlock loopInitializer)
        {
            // Search the predecessor block for the initial assignment which contains the right expression
            Expression right = new EmptyExpression();
            for (var i = loopInitializer.Instructions.Count - 1; i >= 0; i--)
            {
                if (loopInitializer.Instructions[i] is Assignment { Right: not null } a)
                {
                    right = a.Right;
                    loopInitializer.Instructions.RemoveAt(i);
                    break;
                }
            }

            // Loop head has the loop variables
            Assignment iterator;
            if (node.Instructions.First() is Assignment a2)
            {
                iterator = new Assignment(a2.LeftList, right);
                node.Instructions.RemoveAt(0);
            }
            else
            {
                throw new Exception("Unknown for pattern");
            }

            // Body contains more loop bytecode that can be removed
            var body = 
                (node.EdgeTrue.ReversePostorderNumber < node.EdgeFalse.ReversePostorderNumber) ? 
                    node.EdgeTrue : node.EdgeFalse;
            body.MarkCodeGenerated(f.FunctionId, f.Warnings);

            BasicBlock? follow = null;
            Debug.Assert(node.LoopFollow != null);
            if (!usedFollows.Contains(node.LoopFollow))
            {
                follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }

            var genericFor = new GenericFor(iterator, body, follow);
            if (loopInitializer.Instructions[^1] is IJump)
            {
                loopInitializer.Instructions[^1] = genericFor;
            }
            else
            {
                loopInitializer.Instructions.Add(genericFor);
            }
            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);
        }

        void TransformWhile(BasicBlock node, BasicBlock loopInitializer, Expression loopCondition)
        {
            node.Instructions.RemoveAt(node.Instructions.Count - 1);

            //whiles.Body = (node.Successors[0].ReversePostorderNumber > node.Successors[1].ReversePostorderNumber) ?
            //node.Successors[0] : node.Successors[1];
            var body = node.EdgeTrue;
            body.MarkCodeGenerated(f.FunctionId, f.Warnings);
            BasicBlock? follow = null;
            Debug.Assert(node.LoopFollow != null);
            if (!usedFollows.Contains(node.LoopFollow))
            {
                follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }
            // If there's a goto to this loop head, replace it with the while. Otherwise replace the last instruction
            // of this node
            var whiles = new While
            {
                Condition = loopCondition,
                Body = body,
                Follow = follow,
            };
            if (loopInitializer.Successors.Count == 1)
            {
                if (loopInitializer is { HasInstructions: true, Last: IJump })
                {
                    loopInitializer.Instructions[^1] = whiles;
                }
                else
                {
                    loopInitializer.Instructions.Add(whiles);
                }
            }
            else
            {
                node.Instructions.Add(whiles);
            }

            // Remove gotos in latch
            foreach (var pred in node.Predecessors)
            {
                if (pred is { IsLoopLatch: true, IsConditionalBranch: false, Last: Jump })
                {
                    pred.Instructions.RemoveAt(pred.Instructions.Count - 1);
                }
            }

            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);
        }

        void TransformRepeatWhile(BasicBlock node, BasicBlock loopInitializer, Expression loopCondition)
        {
            node.Instructions.RemoveAt(node.Instructions.Count - 1);

            //whiles.Body = (node.Successors[0].ReversePostorderNumber > node.Successors[1].ReversePostorderNumber) ? node.Successors[0] : node.Successors[1];
            var body = node.EdgeFalse;
            body.MarkCodeGenerated(f.FunctionId, f.Warnings);
            BasicBlock? follow = null;
            Debug.Assert(node.LoopFollow != null);
            if (!usedFollows.Contains(node.LoopFollow))
            {
                follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }
                    
            var whiles = new While
            {
                Condition = loopCondition,
                Body = body,
                Follow = follow,
                IsPostTested = true
            };
                    
            // If there's a goto to this loop head, replace it with the while. Otherwise replace the last instruction of this node
            if (loopInitializer.Successors.Count == 1)
            {
                if (loopInitializer.Last is IJump)
                {
                    loopInitializer.Instructions[^1] = whiles;
                }
                else
                {
                    loopInitializer.Instructions.Add(whiles);
                }
            }
            else
            {
                node.Instructions.Add(whiles);
            }

            // Remove gotos in latch
            foreach (var pred in node.Predecessors)
            {
                if (pred is { IsLoopLatch: true, IsConditionalBranch: false, Last: Jump })
                {
                    pred.Instructions.RemoveAt(pred.Instructions.Count - 1);
                }
            }

            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);
        }

        void TransformRepeatUntil(BasicBlock node)
        {
            // Loop head has condition
            if (node.LoopLatches.Count != 1 || !node.LoopLatches[0].HasInstructions ||
                node.LoopLatches[0].Last is not IJump)
            {
                throw new Exception("Unrecognized post-tested loop");
            }
                
            var condition = ((ConditionalJump)node.LoopLatches[0].Last).Condition ?? throw new Exception();
            BasicBlock? follow = null;
            Debug.Assert(usedFollows != null);
            if (node.LoopFollow != null && !usedFollows.Contains(node.LoopFollow))
            {
                follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }

            var whiles = new While
            {
                Condition = condition,
                Body = node,
                Follow = follow,
                IsPostTested = true
            };
            if (node.Predecessors.Count == 2)
            {
                var loopInitializer = node.Predecessors.First(x => x != node.LoopLatches[0]);
                if (loopInitializer.Successors.Count == 1)
                {
                    if (loopInitializer.Instructions.Count > 0 && loopInitializer.Last is IJump)
                    {
                        loopInitializer.Instructions[^1] = whiles;
                    }
                    else
                    {
                        loopInitializer.Instructions.Add(whiles);
                    }
                }
                else
                {
                    whiles.IsBlockInlined = true;
                    node.IsInfiniteLoop = true;
                    node.Instructions.Insert(0, whiles);
                }
            }
            else
            {
                whiles.IsBlockInlined = true;
                node.IsInfiniteLoop = true;
                node.Instructions.Insert(0, whiles);
            }

            // Remove jumps in latch
            foreach (var pred in node.Predecessors)
            {
                if (pred is { IsLoopLatch: true, IsConditionalBranch: false, Last: IJump })
                {
                    pred.Instructions.RemoveAt(pred.Instructions.Count - 1);
                }
            }

            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);
        }

        void TransformInfiniteWhile(BasicBlock node)
        {
            var whiles = new While
            {
                // Loop head has condition
                Condition = new Constant(true, -1),
                Body = node
            };

            if (node.LoopFollow != null && !usedFollows.Contains(node.LoopFollow))
            {
                whiles.Follow = node.LoopFollow;
                usedFollows.Add(node.LoopFollow);
                node.LoopFollow.MarkCodeGenerated(f.FunctionId, f.Warnings);
            }

            if (node.Predecessors.Count == 2)
            {
                var loopInitializer = node.Predecessors.First(x => !node.LoopLatches.Contains(x));
                if (loopInitializer.Successors.Count == 1)
                {
                    if (loopInitializer is { HasInstructions: true, Last: IJump })
                    {
                        loopInitializer.Instructions[^1] = whiles;
                    }
                    else
                    {
                        loopInitializer.Instructions.Add(whiles);
                    }
                }
                else
                {
                    whiles.IsBlockInlined = true;
                    node.IsInfiniteLoop = true;
                    node.Instructions.Insert(0, whiles);
                }
            }
            else
            {
                whiles.IsBlockInlined = true;
                node.IsInfiniteLoop = true;
                node.Instructions.Insert(0, whiles);
            }

            // Remove gotos in latch
            foreach (var pred in node.Predecessors)
            {
                if (pred is { IsLoopLatch: true, IsConditionalBranch: false, Last: Jump })
                {
                    pred.Instructions.RemoveAt(pred.Instructions.Count - 1);
                }
            }

            node.MarkCodeGenerated(f.FunctionId, f.Warnings);
            // The head might be the follow of an if statement, so do this to not codegen it
            usedFollows.Add(node);
        }

        void TransformIfStatement(BasicBlock node, ConditionalJump jmp)
        {
            var ifStatement = new IfStatement
            {
                Condition = jmp.Condition
            };
            // Check for empty if block
            if (node.EdgeTrue != node.Follow)
            {
                ifStatement.True = node.EdgeTrue;
                ifStatement.True.MarkCodeGenerated(f.FunctionId, f.Warnings);
                if (ifStatement.True.Last is Jump lj)
                {
                    if (ifStatement.True.IsLoopLatch || !ifStatement.True.EdgeTrue.IsLoopHead)
                    {
                        // Remove jumps if they don't jump immediately to the following block
                        // (these will be handled later)
                        if (node.Follow == null || ifStatement.True.BlockIndex + 1 != lj.Destination.BlockIndex)
                        {
                            ifStatement.True.Instructions.Remove(lj);
                        }
                    }
                }
            }
            if (node.EdgeFalse != node.Follow)
            {
                ifStatement.False = node.EdgeFalse;
                ifStatement.False.MarkCodeGenerated(f.FunctionId, f.Warnings);
                // Don't remove the jump if it's a jump to the follow
                if (ifStatement.False.Last is Jump fj &&
                    fj.Destination.BlockIndex != ifStatement.False.BlockIndex + 1)
                {
                    ifStatement.False.Instructions.Remove(fj);
                }
            }
            Debug.Assert(node.Follow != null);
            if (!usedFollows.Contains(node.Follow))
            {
                ifStatement.Follow = node.Follow;
                ifStatement.Follow.MarkCodeGenerated(f.FunctionId, f.Warnings);
                usedFollows.Add(node.Follow);
            }
            node.Instructions[^1] = ifStatement;
        }
        
        // If statements may have a jump to a follow that occurs right after, which indicates that there may be an empty
        // else statement
        void InsertEmptyElseIfNeeded(BasicBlock node, BasicBlock? loopHead)
        {
            var newLoopHead = loopHead;
            if (node.IsLoopHead)
            {
                newLoopHead = node;
            }
            
            dominance.RunOnDominanceTreeSuccessors(f, node, successor =>
            {
                InsertEmptyElseIfNeeded(successor, successor == node.LoopFollow ? loopHead : newLoopHead);
            });
            
            var follow = loopHead;
            if (node is { HasInstructions: true, Last: IfStatement { }, Follow: { } ifFollow })
            {
                follow = ifFollow;
            }

            if (node.HasInstructions && follow != null)
            {
                if (node.Last is IfStatement { True: { } t, False: null } i)
                {
                    // A jump from the end of the true edge to the follow typically indicates an empty else statement
                    var endBlock = LastBlockInScope(t);
                    if (endBlock is { HasInstructions: true, Last: Jump j } &&
                        endBlock.BlockIndex + 1 == follow.BlockIndex &&
                        j.Destination == follow)
                    {
                        // If we were an empty if then it's not an empty else. Just remove the jump.
                        if (!endBlock.IsEmptyIf)
                        {
                            i.False = f.CreateBasicBlock();
                            i.False.MarkCodeGenerated(f.FunctionId, f.Warnings);
                        }

                        endBlock.Instructions.RemoveAt(endBlock.Instructions.Count - 1);
                    }
                }
                else if (node.Last is IfStatement { False: { Instructions.Count: 1 } f2 } ifStatement)
                {
                    var endBlock = LastBlockInScope(f2);
                    if (endBlock is { HasInstructions: true, Last: Jump j } &&
                        endBlock.BlockIndex + 1 == follow.BlockIndex &&
                        j.Destination == follow)
                    {
                        // We have a jump to the end in an if-else chain. We need to get to the last statement in there
                        // to insert an empty else if needed
                        BasicBlock searchBlock = ifStatement.False;
                        while (searchBlock is { Instructions.Count: 1 })
                        {
                            if (searchBlock.Last is IfStatement { False: null, Follow: null } foundStatement)
                            {
                                // Found the last block in the if/else chain and it doesn't have an else.
                                // Insert empty else if this block wasn't an empty if in the bytecode.
                                if (!endBlock.IsEmptyIf)
                                {
                                    foundStatement.False = f.CreateBasicBlock();
                                    foundStatement.False.MarkCodeGenerated(f.FunctionId, f.Warnings);
                                }

                                endBlock.Instructions.RemoveAt(endBlock.Instructions.Count - 1);
                                break;
                            }

                            // Otherwise see if there's another else if leaf to check
                            if (searchBlock.Last is IfStatement
                                {
                                    False.Instructions.Count: 1, Follow: null
                                } searchStatement)
                            {
                                searchBlock = searchStatement.False;
                                continue;
                            }

                            break;
                        }
                    }
                }
            }
        }

        // Step 1: build the AST for ifs/loops based on follow information
        foreach (var node in f.PostorderTraversal(true))
        {
            // A for loop is a pretested loop where the follow does not match the head
            if (node.LoopFollow != null && node.LoopFollow != node && node.Predecessors.Count >= 2 && 
                node.LoopType == LoopType.LoopPretested)
            {
                var loopInitializer = node.Predecessors.First(x => !node.LoopLatches.Contains(x));

                // Match a numeric for
                if (node.Instructions.Last() is 
                    ConditionalJump { Condition: BinOp { Operation: BinOp.OperationType.OpLoopCompare } })
                {
                    TransformNumericFor(node, loopInitializer);
                }

                // Match a generic for with a predecessor initializer
                else if (node is 
                             { 
                                 HasInstructions: true, 
                                 Last: ConditionalJump { Condition: BinOp }, 
                                 First: Assignment { Right: FunctionCall { Function: IdentifierReference fci } } 
                             } && loopInitializer.Instructions is [.., Assignment { Left: IdentifierReference f2 }, _] && 
                         fci.Identifier == f2.Identifier)
                {
                    TransformGenericFor(node, loopInitializer);
                }

                // Match a while
                else if (node.First is ConditionalJump { Condition: { } loopCondition4 })
                {
                    TransformWhile(node, loopInitializer, loopCondition4);
                }

                // Match a repeat while (single block)
                else if (node is { Last: ConditionalJump { Condition: { } loopCondition5 }, LoopLatches.Count: 1 } && 
                         node.LoopLatches[0] == node)
                {
                    TransformRepeatWhile(node, loopInitializer, loopCondition5);
                }
            }

            // repeat...until loop
            if (node.LoopType == LoopType.LoopPosttested)
            {
                TransformRepeatUntil(node);
            }

            // Infinite while loop
            if (node.LoopType == LoopType.LoopEndless)
            {
                TransformInfiniteWhile(node);
            }

            // Pattern match for an if statement
            if (node is { HasInstructions: true, Follow: not null, Last: ConditionalJump jmp })
            {
                TransformIfStatement(node, jmp);
            }
        }
        
        // Step 2: Do some post operations that require the full context of things
        InsertEmptyElseIfNeeded(f.BeginBlock, null);

        // Step 3: Remove Jump instructions from follows if they exist
        foreach (var follow in usedFollows)
        {
            if (follow is { HasInstructions: true, Last: Jump or ConditionalJump })
            {
                follow.Instructions.RemoveAt(follow.Instructions.Count - 1);
            }
        }

        // Step 4: For debug walk the CFG and print blocks that haven't been codegened
        foreach (var b in f.PostorderTraversal(true).Where(b => b != f.BeginBlock && !b.IsCodeGenerated))
        {
            f.Warnings.Add($@"-- Warning: block_{b.BlockId} in function {f.FunctionId} was not used in code generation. THIS IS LIKELY A DECOMPILER BUG!");
        }

        f.IsAst = true;
        return true;
    }
}