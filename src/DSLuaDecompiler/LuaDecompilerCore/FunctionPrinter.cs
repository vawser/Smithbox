using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore;

public partial class FunctionPrinter
{
    private StringBuilder _builder = new();
    private readonly List<Function> _functionStack = new();
    private int _indentLevel;
    private bool _insertDebugComments;
    private bool _debugPrint;

    internal static string DebugPrintFunction(Function function)
    {
        var printer = new FunctionPrinter { _debugPrint = true };
        printer.VisitFunction(function);
        return printer._builder.ToString();
    }
    
    internal static string DebugPrintBasicBlock(BasicBlock basicBlock)
    {
        var printer = new FunctionPrinter { _debugPrint = true };
        printer.VisitBasicBlock(basicBlock);
        return printer._builder.ToString();
    }
    
    internal static string DebugPrintInstruction(Instruction instruction)
    {
        var printer = new FunctionPrinter { _debugPrint = true };
        printer.VisitInstruction(instruction);
        return printer._builder.ToString();
    }
    
    internal static string DebugPrintExpression(Expression expression)
    {
        var printer = new FunctionPrinter { _debugPrint = true };
        printer.VisitExpression(expression);
        return printer._builder.ToString();
    }
    
    internal static string DebugPrintIdentifier(Identifier identifier)
    {
        var printer = new FunctionPrinter { _debugPrint = true };
        printer.VisitIdentifier(identifier);
        return printer._builder.ToString();
    }
    
    public string PrintFunction(Function function)
    {
        _builder = new StringBuilder(128 * 1024);
        _insertDebugComments = function.InsertDebugComments;
        VisitFunction(function);
        return _builder.ToString();
    }

    public void PrintFunctionToStringBuilder(Function function, StringBuilder builder)
    {
        _builder = builder;
        _insertDebugComments = function.InsertDebugComments;
        VisitFunction(function);
    }

    private void Append(char value)
    {
        _builder.Append(value);
    }

    private void Append(string value)
    {
        _builder.Append(value);
    }
    
    private void Append(double value)
    {
        _builder.Append(value);
    }
    
    private void NewLine()
    {
        Append('\n');
    }

    private void PushIndent()
    {
        _indentLevel++;
    }

    private void PopIndent()
    {
        _indentLevel--;
    }

    private void PushFunction(Function function)
    {
        _functionStack.Add(function);
    }

    private void PopFunction()
    {
        _functionStack.RemoveAt(_functionStack.Count - 1);
    }

    private Function? CurrentFunction()
    {
        return _functionStack.Count > 0 ? _functionStack[^1] : null;
    }
    
    private Function? LastFunction()
    {
        return _functionStack.Count > 1 ? _functionStack[^2] : null;
    }

    private void Indent(bool half = false)
    {
        for (var i = 0; i < _indentLevel; i++)
        { 
            if (half && i == _indentLevel - 1)
            {
                Append("  ");
                break;
            }
            Append("    ");
        }
    }

    private void VisitExpression(Expression? expression)
    {
        if (expression == null)
            return;
        switch (expression)
        {
            case EmptyExpression:
                VisitEmptyExpression();
                break;
            case Constant constant:
                VisitConstant(constant);
                break;
            case Closure closure:
                VisitClosure(closure);
                break;
            case IdentifierReference identifierReference:
                VisitIdentifierReference(identifierReference);
                break;
            case TableAccess tableAccess:
                VisitTableAccess(tableAccess);
                break;
            case Concat concat:
                VisitConcat(concat);
                break;
            case InitializerList initializerList:
                VisitInitializerList(initializerList);
                break;
            case BinOp binOp:
                VisitBinOp(binOp);
                break;
            case UnaryOp unaryOp:
                VisitUnaryOp(unaryOp);
                break;
            case FunctionCall functionCall:
                VisitFunctionCall(functionCall);
                break;
            default:
                throw new Exception($"Could not match expression {expression}");
        }
    }

    private void VisitInstruction(Instruction instruction)
    {
        switch (instruction)
        {
            case Assignment assignment:
                VisitAssignment(assignment);
                break;
            case Break:
                VisitBreak();
                break;
            case ClosureBinding closureBinding:
                VisitClosureBinding(closureBinding);
                break;
            case Continue:
                VisitContinue();
                break;
            case Data:
                VisitData();
                break;
            case GenericFor genericFor:
                VisitGenericFor(genericFor);
                break;
            case IfStatement ifStatement:
                VisitIfStatement(ifStatement);
                break;
            case JumpLabel jumpLabel:
                VisitJumpLabel(jumpLabel);
                break;
            case Jump jump:
                VisitJump(jump);
                break;
            case ConditionalJumpLabel conditionalJumpLabel:
                VisitConditionalJumpLabel(conditionalJumpLabel);
                break;
            case ConditionalJump conditionalJump:
                VisitConditionalJump(conditionalJump);
                break;
            case Label label:
                VisitLabel(label);
                break;
            case ListRangeAssignment listRangeAssignment:
                VisitListRangeAssignment(listRangeAssignment);
                break;
            case NumericFor numericFor:
                VisitNumericFor(numericFor);
                break;
            case PhiFunction phiFunction:
                VisitPhiFunction(phiFunction);
                break;
            case PlaceholderInstruction placeholderInstruction:
                VisitPlaceholderInstruction(placeholderInstruction);
                break;
            case Return @return:
                VisitReturn(@return);
                break;
            case While @while:
                VisitWhile(@while);
                break;
            default:
                throw new Exception($"Could not match instruction {instruction}");
        }
    }

    private void VisitFunction(Function function, string? name = null)
    {
        PushFunction(function);
        if (function.FunctionId != 0 || _debugPrint)
        {
            Append(name == null ? "function (" : $"function {name}(");
            for (var i = 0; i < function.ParameterCount; i++)
            {
                VisitIdentifier(Identifier.GetRegister((uint)i));
                if (i != function.ParameterCount - 1)
                {
                    Append(", ");
                }
            }
            if (function.IsVarargs)
            {
                Append(function.ParameterCount > 0 ? ", ..." : "...");
            }
            Append(')');

            if (_debugPrint)
            {
                PopFunction();
                return;
            }

            NewLine();
            _indentLevel += 1;
        }
        
        // Print warnings
        foreach (var warning in function.Warnings)
        {
            Indent();
            Append(warning);
            NewLine();
        }
        
        if (_insertDebugComments)
        {
            Indent();
            Append($"-- Function ID = {function.FunctionId}");
            NewLine();
        }
        
        if (function.IsAst)
        {
            if (function.Warnings.Count == 0)
            {
                VisitBasicBlock(function.BeginBlock);
            }
        }
        else
        {
            // Traverse the basic blocks ordered by their ID
            foreach (var b in function.BlockList.OrderBy(a => a.BlockId))
            {
                if (b == function.EndBlock && b != function.BeginBlock)
                {
                    continue;
                }
                Indent(true);
                Append(b.ToStringWithLoop());
                NewLine();
                foreach (var inst in b.PhiFunctions.Values)
                {
                    Indent();
                    VisitPhiFunction(inst);
                    NewLine();
                }
                foreach (var inst in b.Instructions)
                {
                    Indent(inst is Label);
                    VisitInstruction(inst);
                    NewLine();
                }

                // Insert an implicit goto for fallthrough blocks if the destination isn't actually the next block
                if (b.HasInstructions && 
                    ((b.Last is ConditionalJump && b.EdgeTrue.BlockId != b.BlockId + 1) ||
                     (b.Last is not IJump and not Return && b.EdgeTrue.BlockId != b.BlockId + 1)))
                {
                    for (var i = 0; i < _indentLevel; i++)
                    {
                        Append("    ");
                    }

                    Append("(goto ");
                    Append(b.EdgeTrue.Name);
                    Append(')');
                    NewLine();
                }
            }
        }
        if (function.FunctionId != 0)
        {
            _indentLevel -= 1;
            for (var i = 0; i < _indentLevel; i++)
            {
                Append("    ");
            }
            Append("end");
        }

        PopFunction();
    }

    private void VisitBasicBlock(BasicBlock basicBlock, bool printInfiniteLoop = false)
    {
        if (_debugPrint)
        {
            Append(basicBlock.Name);
            return;
        }

        if (_insertDebugComments)
        {
            Indent();
            Append("-- ");
            Append(basicBlock.Name);
            NewLine();
        }
        
        var count = basicBlock.IsInfiniteLoop && !printInfiniteLoop ? 1 : basicBlock.Instructions.Count;
        var begin = basicBlock.IsInfiniteLoop && printInfiniteLoop ? 1 : 0;
        for (var j = begin; j < count; j++)
        {
            var inst = basicBlock.Instructions[j];
            if (j != begin && (basicBlock.Instructions[j].HasClosure ||
                               basicBlock.Instructions[j - 1].HasClosure))
            {
                // Insert new lines before and after anything with a closure
                NewLine();
            }
            Indent();
            
            // Returns that don't appear at the end of the block need to be wrapped with a do..end scope to compile
            // correctly
            if (inst is Return r &&
                !(j == basicBlock.Instructions.Count - 1 ||
                 (basicBlock.Last is Return { IsImplicit: true } && j == basicBlock.Instructions.Count - 2)))
            {
                VisitReturn(r, true);
            }
            else if (inst is Break &&
                !(j == basicBlock.Instructions.Count - 1 ||
                  (basicBlock.Last is Return { IsImplicit: true } && j == basicBlock.Instructions.Count - 2)))
            {
                VisitBreak(true);
            }
            else
            {
                VisitInstruction(inst);
            }

            if (inst is not IfStatement)
            {
                NewLine();
            }
        }
    }

    private void VisitEmptyExpression()
    {
        
    }

    private void VisitConstant(Constant constant)
    {
        switch (constant.ConstType)
        {
            case Constant.ConstantType.ConstNumber:
                Append(constant.Number);
                break;
            case Constant.ConstantType.ConstInteger:
                _builder.Append(constant.Integer);
                break;
            case Constant.ConstantType.ConstString:
                Append($@"""{constant.String}""");
                break;
            case Constant.ConstantType.ConstBool:
                Append(constant.Boolean ? "true" : "false");
                break;
            case Constant.ConstantType.ConstTable:
                Append("{}");
                break;
            case Constant.ConstantType.ConstVarargs:
                Append("...");
                break;
            case Constant.ConstantType.ConstNil:
                Append("nil");
                break;
            default:
                throw new Exception("Unhandled constant type");
        }
    }

    private void VisitClosure(Closure closure)
    {
        VisitFunction(closure.Function);
    }

    private string? LookupIdentifierName(Identifier identifier, Function? currentFunction)
    {
        if (currentFunction == null)
            return null;
        return identifier.Type switch
        {
            Identifier.IdentifierType.Register or Identifier.IdentifierType.RenamedRegister =>
                currentFunction.IdentifierNames.TryGetValue(identifier, out var name) ? name : null,
            Identifier.IdentifierType.Global => currentFunction.Constants[(int)identifier.ConstantId].ToString(),
            Identifier.IdentifierType.UpValue when currentFunction.UpValueBindings.Count > 0 => 
                LookupIdentifierName(currentFunction.UpValueBindings[(int)identifier.UpValueNum], LastFunction()),
            _ => null
        };
    }
    
    private void VisitIdentifier(Identifier identifier)
    {
        if (identifier.IsVarArgs)
        {
            Append("...");
            return;
        }

        if (LookupIdentifierName(identifier, CurrentFunction()) is { } name)
        {
            Append(name);
            return;
        }

        if (identifier.IsRegister)
        {
            Append("REG");
            Append(identifier.RegNum);
            if (identifier.IsRenamedRegister)
            {
                Append('_');
                Append(identifier.RegSubscriptNum);
            }
            return;
        }

        if (identifier.IsUpValue)
        {
            Append("UPVAL");
            Append(identifier.UpValueNum);
            return;
        }

        if (identifier.IsGlobalTable)
        {
            Append("_G");
        }
    }

    private void VisitIdentifierReference(IdentifierReference identifierReference)
    {
        VisitIdentifier(identifierReference.Identifier);
    }
    
    private void VisitTableAccess(TableAccess tableAccess)
    {
        // Detect a Lua 5.3 global variable and don't display it as a table reference
        var isGlobal = tableAccess.Table is IdentifierReference { Identifier.IsGlobalTable: true };
        if (!isGlobal) 
            VisitExpression(tableAccess.Table);
        if (isGlobal && tableAccess.TableIndex is Constant { ConstType: Constant.ConstantType.ConstString } g)
        {
            Append(g.String);
        }
        else if (/*DotNotation &&*/tableAccess.TableIndex is Constant { ConstType: Constant.ConstantType.ConstString } c)
        {
            Append("." + c.String);
        }
        else
        {
            Append('[');
            VisitExpression(tableAccess.TableIndex);
            Append(']');
        }
    }

    private void VisitConcat(Concat concat)
    {
        // Pattern match special lua this call
        if (concat.HasParentheses)
        {
            Append('(');
        }
        for (var i = 0; i < concat.Expressions.Count; i++)
        {
            VisitExpression(concat.Expressions[i]);
            if (i != concat.Expressions.Count - 1)
            {
                Append(" .. ");
            }
        }
        if (concat.HasParentheses)
        {
            Append(')');
        }
    }

    private void VisitInitializerList(InitializerList initializerList)
    {
        Append('{');

        // Pattern match special lua this call
        for (var i = 0; i < initializerList.Expressions.Count; i++)
        {
            if (!initializerList.ListRangeAssignments.Exists(r => r.Contains(i)))
            {
                if (initializerList.Assignments[i] is Constant c)
                {
                    if (c.ConstType == Constant.ConstantType.ConstNumber)
                    {
                        Append($"[{(int)c.Number}] = ");
                    }
                    else if (c.ConstType == Constant.ConstantType.ConstInteger)
                    {
                        Append($"[{(int)c.Integer}] = ");
                    }
                    else if (c.ConstType == Constant.ConstantType.ConstString)
                    {
                        Append(c.String + " = ");
                    }
                }
                else
                {
                    Append('[');
                    VisitExpression(initializerList.Assignments[i]);
                    Append("] = ");
                }
            }
            
            VisitExpression(initializerList.Expressions[i]);
            if (i != initializerList.Expressions.Count - 1)
            {
                Append( ", ");
            }
        }

        Append('}');
    }

    private void VisitBinOp(BinOp binOp)
    {
        var op = binOp.Operation switch
        {
            BinOp.OperationType.OpAdd => "+",
            BinOp.OperationType.OpDiv => "/",
            BinOp.OperationType.OpFloorDiv => "//",
            BinOp.OperationType.OpMod => "%",
            BinOp.OperationType.OpMul => "*",
            BinOp.OperationType.OpPow => "^",
            BinOp.OperationType.OpSub => "-",
            BinOp.OperationType.OpEqual => "==",
            BinOp.OperationType.OpNotEqual => "~=",
            BinOp.OperationType.OpLessThan => "<",
            BinOp.OperationType.OpLessEqual => "<=",
            BinOp.OperationType.OpGreaterThan => ">",
            BinOp.OperationType.OpGreaterEqual => ">=",
            BinOp.OperationType.OpAnd => "and",
            BinOp.OperationType.OpOr => "or",
            BinOp.OperationType.OpBAnd => "&",
            BinOp.OperationType.OpBOr => "|",
            BinOp.OperationType.OpBxOr => "~",
            BinOp.OperationType.OpShiftRight => ">>",
            BinOp.OperationType.OpShiftLeft => "<<",
            BinOp.OperationType.OpLoopCompare => ">?=",
            _ => ""
        };
        if (binOp.HasParentheses)
        {
            Append('(');
        }

        if (binOp.HasImplicitNot)
        {
            Append("not (");
        }
        VisitExpression(binOp.Left);
        Append($" {op} ");
        VisitExpression(binOp.Right);
        if (binOp.HasImplicitNot)
        {
            Append(')');
        }
        if (binOp.HasParentheses)
        {
            Append(')');
        }
    }

    private void VisitUnaryOp(UnaryOp unaryOp)
    {
        var op = unaryOp.Operation switch
        {
            UnaryOp.OperationType.OpNegate => "-",
            UnaryOp.OperationType.OpNot => "not ",
            UnaryOp.OperationType.OpBNot => "~",
            UnaryOp.OperationType.OpLength => "#",
            _ => ""
        };
        if (unaryOp.HasParentheses)
        {
            Append('(');
        }
        Append($"{op}");
        VisitExpression(unaryOp.Expression);
        if (unaryOp.HasParentheses)
        {
            Append(')');
        }
    }

    private void VisitFunctionCall(FunctionCall functionCall)
    {
        // Pattern match special lua this call
        var beginArg = 0;
        if (functionCall.Function is TableAccess
            {
                TableIndex: Constant { ConstType: Constant.ConstantType.ConstString } c, 
                Table: not IdentifierReference { Identifier.IsGlobalTable: true }
            } tableAccess)
        {
            VisitExpression(tableAccess.Table);
            if (functionCall.IsThisCall)
            {
                Append($":{c.String}(");
                beginArg = 1;
            }
            else
            {
                Append($".{c.String}(");
            }
        }
        /*else if (functionCall is 
                 { 
                     Function: TableAccess
                     {
                         Table: IdentifierReference { Identifier.IsGlobalTable: true } ir2, 
                         TableIndices: 
                         [
                             Constant { ConstType: Constant.ConstantType.ConstString } c3, 
                             Constant { ConstType: Constant.ConstantType.ConstString } c2
                         ]
                     },
                     Args:
                     [
                         TableAccess
                         {
                             Table: IdentifierReference thisIdentifier,
                             TableIndices:
                             [
                                 Constant { ConstType: Constant.ConstantType.ConstString } c4
                             ]
                         }, ..
                     ]
                 } && thisIdentifier.Identifier == ir2.Identifier && c3.String == c4.String)
        {
            Append($"{c3.String}:{c2.String}(");
            beginArg = 1;
        }*/
        else
        {
            VisitExpression(functionCall.Function);
            Append('(');
        }
        for (var i = beginArg; i < functionCall.Args.Count; i++)
        {
            VisitExpression(functionCall.Args[i]);
            if (i != functionCall.Args.Count - 1)
            {
                Append(", ");
            }
        }
        Append(')');
    }

    private void VisitAssignment(Assignment assignment)
    {
        var assignmentOp = assignment.IsGenericForAssignment ? " in " : " = ";
        if (assignment.IsLocalDeclaration)
        {
            Append("local ");
        }
        if (assignment is { IsFunctionDeclaration: true, Left: IdentifierReference ir, Right: Closure c })
        {
            VisitFunction(c.Function, LookupIdentifierName(ir.Identifier, CurrentFunction()));
            return;
        }
        
        if (assignment.LeftList.Count > 0) 
        {
            for (var i = 0; i < assignment.LeftList.Count; i++)
            {
                VisitExpression((Expression)assignment.LeftList[i]);
                if (i != assignment.LeftList.Count - 1)
                {
                    Append(", ");
                }
            }
            if (assignment.Right != null)
            {
                Append(assignmentOp);
            }
        }

        if (assignment.Right != null)
        {
            VisitExpression(assignment.Right);
        }
    }

    private void VisitClosureBinding(ClosureBinding closureBinding)
    {
        Append("bind ");
        VisitIdentifier(closureBinding.Identifier);
    }

    private void VisitBreak(bool addScope = false)
    {
        if (addScope)
        {
            Append("do");
            PushIndent();
            NewLine();
            Indent();
        }
        
        Append("break");
        
        if (addScope)
        {
            PopIndent();
            NewLine();
            Indent();
            Append("end");
        }
    }

    private void VisitContinue()
    {
        Append("continue");
    }

    private void VisitData()
    {
        Append("data");
    }

    private void VisitGenericFor(GenericFor genericFor)
    {
        if (genericFor.Iterator is { } a)
        {
            a.IsLocalDeclaration = false;
            a.IsGenericForAssignment = true;
        }

        Append("for ");
        VisitAssignment(genericFor.Iterator);
        Append(" do");
        NewLine();

        PushIndent();
        VisitBasicBlock(genericFor.Body);
        PopIndent();
        Indent();
        Append("end");
        
        if (genericFor.Follow is { HasInstructions: true })
        {
            NewLine();
            VisitBasicBlock(genericFor.Follow);
        }
    }

    private void VisitIfStatement(IfStatement ifStatement)
    {
        Append(ifStatement.IsElseIf ? "elseif " : "if ");
        VisitExpression(ifStatement.Condition);
        Append(" then");
        NewLine();

        if (ifStatement.True != null)
        {
            PushIndent();
            VisitBasicBlock(ifStatement.True);
            PopIndent();
        }
        if (ifStatement.False != null)
        {
            // Check for elseif
            if (ifStatement.False.Instructions.Count == 1 && 
                ifStatement.False.Instructions.First() is IfStatement { Follow: null } s)
            {
                s.IsElseIf = true;
                VisitBasicBlock(ifStatement.False);
            }
            else
            {
                Indent();
                Append("else");
                NewLine();
                PushIndent();
                VisitBasicBlock(ifStatement.False);
                PopIndent();
            }
        }
        if (!ifStatement.IsElseIf)
        {
            Indent();
            Append("end");
            NewLine();
        }
        if (ifStatement.Follow is { HasInstructions: true })
        {
            VisitBasicBlock(ifStatement.Follow);
        }
    }

    private void VisitJumpLabel(JumpLabel jump)
    {
        Append("goto ");
        VisitLabel(jump.Destination);
    }

    private void VisitJump(Jump jump)
    {
        Append("goto ");
        Append(jump.Destination.Name);
    }
    
    private void VisitConditionalJumpLabel(ConditionalJumpLabel jump)
    {
        Append("if ");
        VisitExpression(jump.Condition);
        Append(" else ");
        Append("goto ");
        VisitLabel(jump.Destination);
    }
    
    private void VisitConditionalJump(ConditionalJump jump)
    {
        Append("if ");
        VisitExpression(jump.Condition);
        Append(" else ");
        Append("goto ");
        Append(jump.Destination.Name);
    }

    private void VisitLabel(Label label)
    {
        Append($"{label.LabelName}:");
    }
    
    private void VisitListRangeAssignment(ListRangeAssignment listRangeAssignment)
    {
        VisitIdentifierReference(listRangeAssignment.Table);
        Append($"[{listRangeAssignment.Indices.Begin}...{listRangeAssignment.Indices.End - 1}] := ");
        for (var i = 0; i < listRangeAssignment.Values.Count; i++)
        {
            VisitExpression(listRangeAssignment.Values[i]);
            if (i != listRangeAssignment.Values.Count - 1)
            {
                Append(", ");
            }
        }
    }

    private void VisitNumericFor(NumericFor numericFor)
    {
        if (numericFor.Initial is { } a)
        {
            a.IsLocalDeclaration = false;
        }

        Append("for ");
        if (numericFor.Initial != null)
            VisitAssignment(numericFor.Initial);
        Append(", ");
        VisitExpression(numericFor.Limit);
        Append(", ");
        VisitExpression(numericFor.Increment);
        Append(" do");
        NewLine();

        PushIndent();
        VisitBasicBlock(numericFor.Body);
        PopIndent();
        
        Indent();
        Append("end");
        if (numericFor.Follow is { HasInstructions: true })
        { 
            NewLine();
            VisitBasicBlock(numericFor.Follow);
        }
    }

    private void VisitPhiFunction(PhiFunction phiFunction)
    {
        VisitIdentifier(phiFunction.Left);
        Append(" = phi(");
        for (var i = 0; i < phiFunction.Right.Count; i++)
        {
            if (phiFunction.Right[i] is { IsNull: false } right)
            {
                VisitIdentifier(right);
            }
            else
            {
                Append("undefined");
            }
            
            if (i != phiFunction.Right.Count - 1)
            {
                Append(", ");
            }
        }
        Append(')');
    }

    private void VisitPlaceholderInstruction(PlaceholderInstruction placeholderInstruction)
    {
        Append(placeholderInstruction.Placeholder);
    }

    private void VisitReturn(Return @return, bool addScope = false)
    {
        if (@return.IsImplicit)
        {
            return;
        }

        if (addScope)
        {
            Append("do");
            PushIndent();
            NewLine();
            Indent();
        }
        
        Append("return");
        for (var i = 0; i < @return.ReturnExpressions.Count; i++)
        {
            if (i == 0) Append(' ');
            VisitExpression(@return.ReturnExpressions[i]);
            if (i != @return.ReturnExpressions.Count - 1)
            {
                Append(", ");
            }
        }

        if (addScope)
        {
            PopIndent();
            NewLine();
            Indent();
            Append("end");
        }
    }

    private void VisitWhile(While @while)
    {
        if (@while.IsPostTested)
        {
            Append("repeat");
            NewLine();
        }
        else
        {
            Append("while ");
            VisitExpression(@while.Condition);
            Append(" do");
            NewLine();
        }

        PushIndent();
        VisitBasicBlock(@while.Body, @while.IsBlockInlined);
        PopIndent();
        
        Indent();
        if (@while.IsPostTested)
        {
            Append("until ");
            VisitExpression(@while.Condition);
        }
        else
        {
            Append("end");
        }
        if (@while.Follow is { HasInstructions: true })
        { 
            NewLine();
            VisitBasicBlock(@while.Follow);
        }
    }
}