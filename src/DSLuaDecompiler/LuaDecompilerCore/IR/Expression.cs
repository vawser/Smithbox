using System;
using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// Base class for an expression, which can basically do anything expressive
    /// </summary>
    public abstract class Expression : IIrNode
    {
        /// <summary>
        /// Range of registers that this expression was first assigned to before expression propagation
        /// </summary>
        public Interval OriginalAssignmentRegisters;

        public HashSet<Identifier> GetDefinedRegisters(HashSet<Identifier> defines)
        {
            return defines;
        }
        
        public virtual HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            return uses;
        }

        public virtual Interval GetTemporaryRegisterRange()
        {
            return new Interval();
        }

        public HashSet<Identifier> GetUsedRegisters()
        {
            var uses = new HashSet<Identifier>(5);
            GetUsedRegisters(uses);
            return uses;
        }

        public virtual void RenameUses(Identifier original, Identifier newIdentifier) { }

        public static bool ShouldReplace(Identifier orig, Expression candidate, Expression replacement)
        {
            if (candidate is IdentifierReference { MinConstantReplacement: var minConstant } &&
                replacement is Constant { ConstantId: var cid } && cid < minConstant && cid != -1)
                return false;
            return candidate is IdentifierReference ident && ident.Identifier == orig;
        }

        public virtual bool ReplaceUses(Identifier original, Expression sub) { return false; }

        public virtual int UseCount(Identifier use) { return 0; }

        public virtual void Parenthesize() { }

        public virtual List<Expression> GetExpressions()
        {
            return new List<Expression> { this };
        }

        public virtual int GetLowestConstantId()
        {
            return -1;
        }

        public override string ToString()
        {
            return FunctionPrinter.DebugPrintExpression(this);
        }

        public abstract bool MatchAny(Func<IIrNode, bool> condition);
        
        public virtual void IterateUses(Action<IIrNode, UseType, IdentifierReference> function) { }

        protected void IterateUsesSuccessor(IIrNode expression, UseType useType, 
            Action<IIrNode, UseType, IdentifierReference> function)
        {
            if (expression is IdentifierReference { IsRegister: true } ir)
                function.Invoke(this, useType, ir);
            else
                expression.IterateUses(function);
        }

        /// <summary>
        /// Gets the original temporary register range for this expression discounting any inlining that has occured.
        /// </summary>
        public Interval GetOriginalUseRegisters()
        {
            if (OriginalAssignmentRegisters.Count == 0)
                return GetTemporaryRegisterRange();
            return OriginalAssignmentRegisters;
        }
    }

    /// <summary>
    /// Empty expression that has nothing and prints nothing. Mostly used for empty expressions in control flow
    /// structures
    /// </summary>
    public sealed class EmptyExpression : Expression
    {
        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            return condition.Invoke(this);
        }
    }

    public sealed class Constant : Expression
    {
        public enum ConstantType
        {
            ConstNumber,
            ConstInteger,
            ConstString,
            ConstBool,
            ConstTable,
            ConstVarargs,
            ConstNil,
        }

        public readonly ConstantType ConstType;
        public readonly double Number;
        public readonly ulong Integer;
        public readonly string String = "";
        public readonly bool Boolean;

        public readonly int ConstantId;

        public Constant(double number, int id)
        {
            ConstType = ConstantType.ConstNumber;
            Number = number;
            ConstantId = id;
        }

        public Constant(ulong integer, int id)
        {
            ConstType = ConstantType.ConstInteger;
            Integer = integer;
            ConstantId = id;
        }

        public Constant(string str, int id)
        {
            ConstType = ConstantType.ConstString;
            String = str;
            ConstantId = id;
        }

        public Constant(bool b, int id)
        {
            ConstType = ConstantType.ConstBool;
            Boolean = b;
            ConstantId = id;
        }

        public Constant(ConstantType type, int id)
        {
            ConstType = type;
            ConstantId = id;
        }

        public override int GetLowestConstantId()
        {
            return ConstantId;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            return condition.Invoke(this);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Constant other || ConstType != other.ConstType)
                return false;

            return ConstType switch
            {
                ConstantType.ConstNumber => Number == other.Number,
                ConstantType.ConstInteger => Integer == other.Integer,
                ConstantType.ConstString => String == other.String,
                ConstantType.ConstBool => Boolean == other.Boolean,
                ConstantType.ConstTable => true,
                ConstantType.ConstVarargs => true,
                ConstantType.ConstNil => true,
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return ConstType switch
            {
                ConstantType.ConstNumber => Number.GetHashCode(),
                ConstantType.ConstInteger => Integer.GetHashCode(),
                ConstantType.ConstString => String.GetHashCode(),
                ConstantType.ConstBool => Boolean.GetHashCode(),
                ConstantType.ConstTable => ConstType.GetHashCode(),
                ConstantType.ConstVarargs => ConstType.GetHashCode(),
                ConstantType.ConstNil => ConstType.GetHashCode(),
                _ => 0
            };
        }
    }

    public sealed class Closure : Expression
    {
        public readonly Function Function;

        public Closure(Function fun)
        {
            Function = fun;
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var binding in Function.UpValueBindings)
            {
                if (binding.IsRegister)
                    uses.Add(binding);
            }

            return uses;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            for (var i = 0; i < Function.UpValueBindings.Count; i++)
            {
                if (Function.UpValueBindings[i] == original)
                {
                    Function.UpValueBindings[i] = newIdentifier;
                }
            }
        }

        public override int UseCount(Identifier use)
        {
            // Bit of a hack but multiplying the use count by 2 ensures that the identifier gets marked as a local and
            // expression propagation doesn't try to inline.
            return Function.UpValueBindings.Count(binding => use == binding) * 2;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            return condition.Invoke(this);
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            foreach (var binding in Function.UpValueBindings)
            {
                // TODO: not sure if this is needed
                //if (binding.IsRegister)
                //    function.Invoke(this, UseType.Argument, binding);
            }
        }
    }

    /// <summary>
    /// An expression that can be assigned to a value
    /// </summary>
    public interface IAssignable : IIrNode
    {
        /// <summary>
        /// The base of this assignable is a register. A base can either be a table that is indexed or an outright
        /// register reference.
        /// </summary>
        public bool IsRegisterBase { get; }
        
        public Identifier RegisterBase { get; }
    }
    
    /// <summary>
    /// Reference to a single identifier (register, upValue, or global)
    /// </summary>
    public sealed class IdentifierReference : Expression, IAssignable
    {
        public Identifier Identifier;

        /// <summary>
        /// The minimum constant ID that a constant needs to be able to be inlined. This is because if a constant was
        /// lower than the ID, the compiler would have placed the constant in the op if it weren't part of a separate
        /// expression.
        /// </summary>
        public int MinConstantReplacement = 0;

        public bool IsRegister => Identifier.IsRegister;

        public bool IsRegisterBase => IsRegister;

        public Identifier RegisterBase => Identifier;

        public IdentifierReference(Identifier id)
        {
            Identifier = id;
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            if (Identifier.IsRegister)
            {
                uses.Add(Identifier);
            }

            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            return IsRegister ? new Interval((int)Identifier.RegNum) : new Interval();
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            if (Identifier == original)
            {
                Identifier = newIdentifier;
            }
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            if (original != Identifier) return false;
            if (sub is not IdentifierReference ir) return false;
            Identifier = ir.Identifier;
            return true;
        }

        public override int UseCount(Identifier use)
        {
            return Identifier == use ? 1 : 0;
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            return ret;
        }

        public override int GetLowestConstantId()
        {
            return Identifier.IsGlobal ? (int)Identifier.ConstantId : 0;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            return condition.Invoke(this);
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            if (Identifier.IsRegister)
            {
                throw new Exception("Iteration should be handled by parent");
            }
        }
    }
    
    public sealed class TableAccess : Expression, IAssignable
    {
        public Expression Table;
        public Expression TableIndex;

        /// <summary>
        /// This identifier reference comes from a SELF op and should be printed with this (':') semantics
        /// </summary>
        public bool IsSelfReference = false;

        public bool IsRegisterBase => Table is IdentifierReference { IsRegister: true };

        public Identifier RegisterBase => (Table as IdentifierReference ?? throw new Exception()).Identifier;

        public TableAccess(Expression table, Expression index)
        {
            Table = table;
            TableIndex = index;
        }

        public override void Parenthesize()
        {
            Table.Parenthesize();
            TableIndex.Parenthesize();
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            Table.GetUsedRegisters(uses);
            TableIndex.GetUsedRegisters(uses);

            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            temporaries.AddToTemporaryRegisterRange(Table.GetOriginalUseRegisters());
            temporaries.AddToTemporaryRegisterRange(TableIndex.GetOriginalUseRegisters());
            temporaries.MergeTemporaryRegisterRange(Table.GetTemporaryRegisterRange());
            temporaries.MergeTemporaryRegisterRange(TableIndex.GetTemporaryRegisterRange());
            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            Table.RenameUses(original, newIdentifier);
            TableIndex.RenameUses(original, newIdentifier);
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            var changed = false;
            if (ShouldReplace(original, TableIndex, sub))
            {
                TableIndex = sub;
                changed = true;
            }
            else
            {
                changed = changed || TableIndex.ReplaceUses(original, sub);
            }
            
            // Don't substitute in initializer lists
            if (sub is InitializerList)
                return changed;
            
            if (ShouldReplace(original, Table, sub))
            {
                Table = sub;
                changed = true;
            }
            else
            {
                changed = changed || Table.ReplaceUses(original, sub);
            }
            return changed;
        }

        public override int UseCount(Identifier use)
        {
            return TableIndex.UseCount(use) + Table.UseCount(use);
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            ret.AddRange(Table.GetExpressions());
            ret.AddRange(TableIndex.GetExpressions());
            return ret;
        }

        public override int GetLowestConstantId()
        {
            var id = Table.GetLowestConstantId();
            var nid = TableIndex.GetLowestConstantId();
            if (id == -1)
            {
                id = nid;
            }
            else if (nid != -1)
            {
                id = Math.Min(id, TableIndex.GetLowestConstantId());
            }
            return id;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            result = result || Table.MatchAny(condition);
            result = result || TableIndex.MatchAny(condition);
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            IterateUsesSuccessor(Table, UseType.Table, function);
            IterateUsesSuccessor(TableIndex, UseType.TableIndex, function);
        }
    }

    public interface IOperator
    {
        int GetPrecedence();
        void SetHasParentheses(bool paren);
    }

    public sealed class Concat : Expression, IOperator
    {
        public readonly List<Expression> Expressions;
        public bool HasParentheses;

        public Concat(List<Expression> expression)
        {
            Expressions = expression;
        }

        public int GetPrecedence()
        {
            return 4;
        }

        public override void Parenthesize()
        {
            foreach (var expr in Expressions)
            {
                if (expr is IOperator op && op.GetPrecedence() > GetPrecedence())
                {
                    op.SetHasParentheses(true);
                }
            }
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var arg in Expressions)
            {
                arg.GetUsedRegisters(uses);
            }

            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            foreach (var exp in Expressions)
            {
                temporaries.AddToTemporaryRegisterRange(exp.GetOriginalUseRegisters());
            }
            
            foreach (var exp in Expressions)
            {
                temporaries.MergeTemporaryRegisterRange(exp.GetTemporaryRegisterRange());
            }

            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            foreach (var arg in Expressions)
            {
                arg.RenameUses(original, newIdentifier);
            }
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            var replaced = false;
            for (var i = 0; i < Expressions.Count; i++)
            {
                if (ShouldReplace(original, Expressions[i], sub))
                {
                    Expressions[i] = sub;
                    replaced = true;
                }
                else
                {
                    replaced = replaced || Expressions[i].ReplaceUses(original, sub);
                }
            }
            return replaced;
        }

        public override int UseCount(Identifier use)
        {
            return Expressions.Sum(e => e.UseCount(use));
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            foreach(var exp in Expressions)
            {
                ret.AddRange(exp.GetExpressions());
            }
            return ret;
        }

        public void SetHasParentheses(bool paren)
        {
            HasParentheses = paren;
        }

        public override int GetLowestConstantId()
        {
            var id = int.MaxValue;
            foreach (var e in Expressions)
            {
                var nid = e.GetLowestConstantId();
                if (nid != -1)
                {
                    id = Math.Min(id, e.GetLowestConstantId());
                }
            }
            return id != int.MaxValue ? id : -1;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            foreach (var idx in Expressions)
            {
                result = result || idx.MatchAny(condition);
            }
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            foreach (var arg in Expressions)
            {
                IterateUsesSuccessor(arg, UseType.Argument, function);
            }
        }
    }

    public sealed class InitializerList : Expression
    {
        public readonly List<Expression> Expressions;
        public readonly List<Expression> Assignments = new();
        public readonly List<Interval> ListRangeAssignments = new();
        
        /// <summary>
        /// The number of elements the table will be initialized with in this list
        /// </summary>
        public readonly int InitSize = -1;
        
        public bool ExpressionsEmpty => Expressions.Count == 0;
        public bool HasExpressions => Expressions.Count > 0;

        public InitializerList(int initSize)
        {
            Expressions = new List<Expression>();
            InitSize = initSize;
        }
        
        public InitializerList(List<Expression> expression)
        {
            Expressions = expression;
        }

        public void AddTableElement(Expression key, Expression value)
        {
            Assignments.Add(key);
            Expressions.Add(value);
        }

        public void AddListRange(Interval range, List<Expression> values)
        {
            ListRangeAssignments.Add(new Interval(Expressions.Count, Expressions.Count + values.Count));
            Expressions.AddRange(values);
            for (var i = range.Begin; i < range.End; i++)
            {
                Assignments.Add(new Constant(i, -1));
            }
        }

        public override void Parenthesize()
        {
            Expressions.ForEach(x => x.Parenthesize());
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var arg in Expressions)
            {
                arg.GetUsedRegisters(uses);
            }

            foreach (var arg in Assignments)
            {
                arg.GetUsedRegisters(uses);
            }

            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            for (var i = 0; i < Expressions.Count; i++)
            {
                temporaries.MergeTemporaryRegisterRange(Assignments[i].GetTemporaryRegisterRange());
                temporaries.MergeTemporaryRegisterRange(Expressions[i].GetTemporaryRegisterRange());
            }

            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            foreach (var arg in Expressions)
            {
                arg.RenameUses(original, newIdentifier);
            }
            
            foreach (var arg in Assignments)
            {
                arg.RenameUses(original, newIdentifier);
            }
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            var replaced = false;
            for (var i = 0; i < Expressions.Count; i++)
            {
                if (ShouldReplace(original, Assignments[i], sub))
                {
                    Assignments[i] = sub;
                    replaced = true;
                }
                else
                {
                    replaced = replaced || Assignments[i].ReplaceUses(original, sub);
                }
                
                if (ShouldReplace(original, Expressions[i], sub))
                {
                    Expressions[i] = sub;
                    replaced = true;
                }
                else
                {
                    replaced = replaced || Expressions[i].ReplaceUses(original, sub);
                }
            }
            return replaced;
        }

        public override int UseCount(Identifier use)
        {
            return Assignments.Sum(e => e.UseCount(use)) + Expressions.Sum(e => e.UseCount(use));
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            for (var i = 0; i < Expressions.Count; i++)
            {
                ret.AddRange(Assignments[i].GetExpressions());
                ret.AddRange(Expressions[i].GetExpressions());
            }
            return ret;
        }

        public override int GetLowestConstantId()
        {
            var id = int.MaxValue;
            foreach (var expression in Assignments)
            {
                var nid = expression.GetLowestConstantId();
                if (nid != -1)
                {
                    id = Math.Min(id, expression.GetLowestConstantId());
                }
            }
            foreach (var expression in Expressions)
            {
                var nid = expression.GetLowestConstantId();
                if (nid != -1)
                {
                    id = Math.Min(id, expression.GetLowestConstantId());
                }
            }
            return id != int.MaxValue ? id : -1;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            foreach (var idx in Assignments)
            {
                result = result || idx.MatchAny(condition);
            }
            foreach (var idx in Expressions)
            {
                result = result || idx.MatchAny(condition);
            }
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            for (var i = 0; i < Expressions.Count; i++)
            {
                IterateUsesSuccessor(Assignments[i], UseType.Argument, function);
                IterateUsesSuccessor(Expressions[i], UseType.Argument, function);
            }
        }
    }

    /// <summary>
    /// Interface for a IR node that maps to an instruction that does a "boolean" operation (i.e. evaluates to true or
    /// false).
    /// </summary>
    public interface IConditionalOperatorPrimitive : IIrNode
    {
        /// <summary>
        /// Is it for real a conditional operator though?
        /// </summary>
        public bool IsConditionalOp { get; }
        
        /// <summary>
        /// Is this a non equality comparison op? (i.e. <, <=, >, >=)
        /// </summary>
        public bool IsNonEqualityComparisonOp { get; }
        
        /// <summary>
        /// Is it a comparison op including equality? (i.e. ==, ~=)
        /// </summary>
        public bool IsComparisonOp { get; }

        /// <summary>
        /// Is it a boolean op? (i.e. and, or)
        /// </summary>
        public bool IsBooleanOp { get; }
        
        /// <summary>
        /// Is it a not op? (not)
        /// </summary>
        public bool IsNotOp { get; }

        /// <summary>
        /// Negate conditional expression by applying a "not" operation
        /// </summary>
        public void NegateConditionalExpression();

        /// <summary>
        /// Solve conditional expression by either applying or factoring out implicit "nots" such that conditional
        /// expression compiles to same ops as source bytecode.
        /// </summary>
        public bool SolveConditionalExpression(bool negated = false);

    }

    public sealed class BinOp : Expression, IOperator, IConditionalOperatorPrimitive
    {
        public enum OperationType
        {
            OpAdd,
            OpSub,
            OpMul,
            OpDiv,
            OpFloorDiv,
            OpMod,
            OpPow,
            OpEqual,
            OpNotEqual,
            OpLessThan,
            OpLessEqual,
            OpGreaterThan,
            OpGreaterEqual,
            OpAnd,
            OpOr,
            OpBAnd,
            OpBOr,
            OpBxOr,
            OpShiftRight,
            OpShiftLeft,
            OpLoopCompare,
        }

        /// <summary>
        /// Original binary op code generated for this expression
        /// </summary>
        public enum OriginalOpType
        {
            OpNotApplicable,
            OpLe,
            OpLt,
            OpLeBk,
            OpLtBk,
        }

        public Expression Left;
        public Expression Right;
        public OperationType Operation;

        public readonly OriginalOpType OriginalOp;

        /// <summary>
        /// Whether or not this bin-op comparison came from a LT_BK or LE_BK instruction and has position requirements
        /// for the left constant.
        /// </summary>
        public bool IsBkComparison => OriginalOp is OriginalOpType.OpLeBk or OriginalOpType.OpLtBk;

        /// <summary>
        /// Whether this operation has an "implicit" not operator on it. The Lua compiler sometimes does folding of
        /// "not" operators as an optimization instead of explicitly generating a not instruction, and this is used as
        /// a convenience to help factor them out instead of generating explicit "not" unary ops.
        /// </summary>
        public bool HasImplicitNot { get; set; }
        
        public bool HasParentheses { get; private set; }

        /// <summary>
        /// If this is set to true, it means that this op is the result of merging two compound conditional blocks
        /// together. Has implications on temporary register use.
        /// </summary>
        public bool IsMergedCompoundConditional = false;
        
        public bool IsNonEqualityComparisonOp => Operation is OperationType.OpLessThan or OperationType.OpLessEqual
            or OperationType.OpGreaterThan or OperationType.OpGreaterEqual;

        public bool IsComparisonOp =>
            IsNonEqualityComparisonOp || Operation is OperationType.OpEqual or OperationType.OpNotEqual;

        public bool IsBooleanOp => Operation is OperationType.OpAnd or OperationType.OpOr;

        public bool IsConditionalOp => IsComparisonOp || IsBooleanOp;

        public bool IsNotOp => false;

        public bool IsBkLegal => IsBkComparison ? Left is Constant : Left is not Constant;

        public BinOp(Expression left, Expression right, OperationType op, 
            OriginalOpType originalOp = OriginalOpType.OpNotApplicable)
        {
            Left = left;
            Right = right;
            Operation = op;
            OriginalOp = originalOp;
        }

        /// <summary>
        /// Negates this op as a single condition
        /// </summary>
        public BinOp NegateCondition()
        {
            switch (Operation)
            {
                case OperationType.OpEqual:
                    Operation = OperationType.OpNotEqual;
                    break;
                case OperationType.OpNotEqual:
                    Operation = OperationType.OpEqual;
                    break;
                case OperationType.OpLessThan:
                    Operation = OperationType.OpGreaterEqual;
                    break;
                case OperationType.OpLessEqual:
                    Operation = OperationType.OpGreaterThan;
                    break;
                case OperationType.OpGreaterThan:
                    Operation = OperationType.OpLessEqual;
                    break;
                case OperationType.OpGreaterEqual:
                    Operation = OperationType.OpLessThan;
                    break;
                case OperationType.OpLoopCompare:
                    break;
                case OperationType.OpAdd:
                case OperationType.OpSub:
                case OperationType.OpMul:
                case OperationType.OpDiv:
                case OperationType.OpFloorDiv:
                case OperationType.OpMod:
                case OperationType.OpPow:
                case OperationType.OpAnd:
                case OperationType.OpOr:
                case OperationType.OpBAnd:
                case OperationType.OpBOr:
                case OperationType.OpBxOr:
                case OperationType.OpShiftRight:
                case OperationType.OpShiftLeft:
                default:
                    throw new Exception("Attempting to negate non-conditional binary operation");
            }
            return this;
        }

        /// <summary>
        /// Negates this op as a conditional expression that may be built of multiple parts
        /// </summary>
        public void NegateConditionalExpression()
        {
            if (!IsConditionalOp)
                throw new Exception("Unexpected negation of non boolean op");
            HasImplicitNot = !HasImplicitNot;
        }

        public bool SolveConditionalExpression(bool negated = false)
        {
            if (!IsConditionalOp)
                return true;
            
            // The first thing we need to do is if we have an implicit not try to eliminate it by applying a transform
            if (Operation is OperationType.OpEqual or OperationType.OpNotEqual && HasImplicitNot)
            {
                // Comparison op negation is trivial
                NegateCondition();
                HasImplicitNot = false;
            }
            else if (HasImplicitNot && IsBooleanOp && 
                     Left is BinOp { IsConditionalOp: true } or UnaryOp { Operation: UnaryOp.OperationType.OpNot} &&
                     Right is BinOp { IsConditionalOp: true } or UnaryOp { Operation: UnaryOp.OperationType.OpNot})
            {
                // Constraints on whether we can apply this transform
                bool valid = Left is UnaryOp { IsImplicit: true } || Right is UnaryOp { IsImplicit: true } || 
                             Left is BinOp || Right is BinOp;
                // Use DeMorgan's Law:
                // not (a and b) -> not a or not b
                // not (a or b) -> not a and not b
                if (valid)
                {
                    if (Left is BinOp bl)
                        bl.HasImplicitNot = !bl.HasImplicitNot;
                    else if (Left is UnaryOp l)
                        Left = l.Expression;
                    if (Right is BinOp br)
                        br.HasImplicitNot = !br.HasImplicitNot;
                    else if (Right is UnaryOp r)
                        Right = r.Expression;
                    Operation = Operation == OperationType.OpAnd ? OperationType.OpOr : OperationType.OpAnd;
                    HasImplicitNot = false;
                }
            }

            // If we have an unresolved implicit not, flip the negated flag
            negated ^= HasImplicitNot;
            bool negatedLeft = Operation == OperationType.OpOr ? !negated : negated;
            bool negatedRight = negated;
            
            // Process the children if they are conditional expressions as well
            if (Left is IConditionalOperatorPrimitive { IsConditionalOp: true } l2)
                l2.SolveConditionalExpression(negatedLeft);
            if (Right is IConditionalOperatorPrimitive { IsConditionalOp: true } r2)
                r2.SolveConditionalExpression(negatedRight);
            
            // If we are a boolean op, see if child expressions now have an implicit not that can be factored out
            if (IsBooleanOp && 
                Left is BinOp { IsConditionalOp: true, HasImplicitNot: true } or 
                    UnaryOp { Operation: UnaryOp.OperationType.OpNot} &&
                Right is BinOp { IsConditionalOp: true, HasImplicitNot: true } or 
                    UnaryOp { Operation: UnaryOp.OperationType.OpNot})
            {
                // Use DeMorgan's Law again to factor out the not
                bool valid = Left is UnaryOp { IsImplicit: true } || Right is UnaryOp { IsImplicit: true } || 
                             Left is BinOp || Right is BinOp;
                if (valid)
                {
                    if (Left is BinOp bl)
                        bl.HasImplicitNot = false;
                    else if (Left is UnaryOp l)
                        Left = l.Expression;
                    if (Right is BinOp br)
                        br.HasImplicitNot = false;
                    else if (Right is UnaryOp r)
                        Right = r.Expression;
                    Operation = Operation == OperationType.OpAnd ? OperationType.OpOr : OperationType.OpAnd;
                    HasImplicitNot = true;
                }
            }

            return true;
        }

        /// <summary>
        /// The lower the number the higher the precedence
        /// </summary>
        /// <returns></returns>
        public int GetPrecedence()
        {
            switch (Operation)
            {
                case OperationType.OpPow:
                    return 0;
                case OperationType.OpMul:
                case OperationType.OpDiv:
                case OperationType.OpFloorDiv:
                case OperationType.OpMod:
                    return 2;
                case OperationType.OpAdd:
                case OperationType.OpSub:
                    return 3;
                case OperationType.OpShiftRight:
                case OperationType.OpShiftLeft:
                    return 4;
                case OperationType.OpBAnd:
                    return 5;
                case OperationType.OpBxOr:
                    return 6;
                case OperationType.OpBOr:
                    return 7;
                case OperationType.OpEqual:
                case OperationType.OpNotEqual:
                case OperationType.OpLessThan:
                case OperationType.OpLessEqual:
                case OperationType.OpGreaterThan:
                case OperationType.OpGreaterEqual:
                case OperationType.OpLoopCompare:
                    return 8;
                case OperationType.OpAnd:
                    return 9;
                case OperationType.OpOr:
                    return 10;
                default:
                    return 99999;
            }
        }

        public override void Parenthesize()
        {
            // If left has a lower precedence than this op, then add parentheses to evaluate it first. If the right side
            // is of equal precedence, then we also need to insert parentheses.
            if (Left is IOperator op1 && op1.GetPrecedence() > GetPrecedence())
            {
                op1.SetHasParentheses(true);
            }
            if (Right is IOperator op2 && (op2.GetPrecedence() > GetPrecedence() || 
                                           (op2.GetPrecedence() == GetPrecedence() && !IsConditionalOp)))
            {
                op2.SetHasParentheses(true);
            }

            // If we're a comparison op, we may need to swap the left and right if they both refer to constants, or if
            // they both were temporary registers with the left register being greater than the right one. This
            // indicates that the Lua compiler swapped the operands when emitting the op, and we must swap back in the
            // emitted source code.
            var leftConstId = Left.GetLowestConstantId();
            var rightConstId = Right.GetLowestConstantId();

            if (IsNonEqualityComparisonOp && 
                ((leftConstId != -1 && rightConstId != -1 && leftConstId > rightConstId &&
                  (Left.OriginalAssignmentRegisters.Count == 0 || Right.OriginalAssignmentRegisters.Count == 0)) ||
                (IsNonEqualityComparisonOp && Left.OriginalAssignmentRegisters.Count > 0 && 
                 Right.OriginalAssignmentRegisters.Count > 0 && 
                 Left.OriginalAssignmentRegisters.Begin > Right.OriginalAssignmentRegisters.Begin)))
            {
                // We need to swap the left and right to keep matching recompiles
                (Right, Left) = (Left, Right);
                Operation = Operation switch
                {
                    OperationType.OpLessThan => OperationType.OpGreaterThan,
                    OperationType.OpGreaterThan => OperationType.OpLessThan,
                    OperationType.OpLessEqual => OperationType.OpGreaterEqual,
                    OperationType.OpGreaterEqual => OperationType.OpLessEqual,
                    _ => Operation
                };
            }

            Left.Parenthesize();
            Right.Parenthesize();
        }

        public bool IsCompare => Operation switch
        {
            OperationType.OpEqual => true,
            OperationType.OpNotEqual => true,
            OperationType.OpLessThan => true,
            OperationType.OpLessEqual => true,
            OperationType.OpGreaterThan => true,
            OperationType.OpGreaterEqual => true,
            OperationType.OpLoopCompare => true,
            _ => false
        };

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            Left.GetUsedRegisters(uses);
            Right.GetUsedRegisters(uses);
            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            var leftOriginal = Left.GetOriginalUseRegisters();
            var rightOriginal = Right.GetOriginalUseRegisters();
            var leftTemporary = Left.GetTemporaryRegisterRange();
            var rightTemporary = Right.GetTemporaryRegisterRange();
            
            // Comparison ops may have their arguments swapped for comparisons, so account for this by accepting the
            // lowest interval first
            if (IsNonEqualityComparisonOp && leftOriginal.Count > 0 && rightOriginal.Count > 0 &&
                rightOriginal.Begin < leftOriginal.Begin)
            {
                (leftOriginal, rightOriginal) = (rightOriginal, leftOriginal);
                (leftTemporary, rightTemporary) = (rightTemporary, leftTemporary);
            }
            
            // If this was a compound conditional, the right side will have no temporaries left and should be ignored
            temporaries.AddToTemporaryRegisterRange(leftOriginal);
            temporaries.AddToTemporaryRegisterRange(rightOriginal, IsMergedCompoundConditional);
            temporaries.MergeTemporaryRegisterRange(leftTemporary);
            temporaries.MergeTemporaryRegisterRange(rightTemporary);
            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            Left.RenameUses(original, newIdentifier);
            Right.RenameUses(original, newIdentifier);
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            bool replaced;
            if (ShouldReplace(original, Left, sub))
            {
                Left = sub;
                replaced = true;
            }
            else
            {
                replaced = Left.ReplaceUses(original, sub);
            }

            // Don't perform replacements on the right side if it's been merged in
            if (IsMergedCompoundConditional)
                return replaced;
            
            if (ShouldReplace(original, Right, sub))
            {
                Right = sub;
                replaced = true;
            }
            else
            {
                replaced = replaced || Right.ReplaceUses(original, sub);
            }
            return replaced;
        }

        public override int UseCount(Identifier use)
        {
            return Left.UseCount(use) + Right.UseCount(use);
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            ret.AddRange(Left.GetExpressions());
            ret.AddRange(Right.GetExpressions());
            return ret;
        }

        public override int GetLowestConstantId()
        {
            var left = Left.GetLowestConstantId();
            var right = Right.GetLowestConstantId();
            if (left == -1)
            {
                return right;
            }
            if (right == -1)
            {
                return left;
            }
            return Math.Min(left, right);
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            result = result || Left.MatchAny(condition) || Right.MatchAny(condition);
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            IterateUsesSuccessor(Left, UseType.ExpressionLeft, function);
            IterateUsesSuccessor(Right, UseType.ExpressionRight, function);
        }

        public void SetHasParentheses(bool paren)
        {
            HasParentheses = paren;
        }
    }

    public sealed class UnaryOp : Expression, IOperator, IConditionalOperatorPrimitive
    {
        public enum OperationType
        {
            OpNegate,
            OpNot,
            OpBNot,
            OpLength,
        }

        public Expression Expression;
        public readonly OperationType Operation;

        public bool HasParentheses { get; private set; }

        public bool IsImplicit = false;
        
        public UnaryOp(Expression expression, OperationType op)
        {
            Expression = expression;
            Operation = op;
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            Expression.GetUsedRegisters(uses);
            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            temporaries.AddToTemporaryRegisterRange(Expression.GetOriginalUseRegisters());
            temporaries.MergeTemporaryRegisterRange(Expression.GetTemporaryRegisterRange());

            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            Expression.RenameUses(original, newIdentifier);
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            if (ShouldReplace(original, Expression, sub))
            {
                Expression = sub;
                return true;
            }

            return Expression.ReplaceUses(original, sub);
        }

        public override int UseCount(Identifier use)
        {
            return Expression.UseCount(use);
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            ret.AddRange(Expression.GetExpressions());
            return ret;
        }

        public bool IsConditionalOp => Operation == OperationType.OpNot;
        public bool IsNonEqualityComparisonOp => false;
        public bool IsComparisonOp => false;
        public bool IsBooleanOp => false;
        public bool IsNotOp => Operation == OperationType.OpNot;

        /// <summary>
        /// Negates this op as a conditional expression that may be built of multiple parts. Since this op may result
        /// in the unary op needing to be removed, the expression returned will indicate what this needs to be replaced
        /// with.
        /// </summary>
        public void NegateConditionalExpression()
        {
            if (Operation != OperationType.OpNot || Expression is not BinOp {} b)
                throw new Exception("Attempting to negate non-not expression");
            b.NegateConditionalExpression();
        }

        public bool SolveConditionalExpression(bool negated = false)
        {
            // End of conditional expression
            if (!IsNotOp)
                return true;
            
            // Explicit ops are always emitted so passthrough
            if (Expression is IConditionalOperatorPrimitive p)
                return p.SolveConditionalExpression(negated);

            return true;
        }

        public override int GetLowestConstantId()
        {
            return Expression.GetLowestConstantId();
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            result = result || Expression.MatchAny(condition);
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            IterateUsesSuccessor(Expression, UseType.ExpressionLeft, function);
        }

        public int GetPrecedence()
        {
            return 1;
        }
        public override void Parenthesize()
        {
            // If left has a lower precedence than this op, then add parentheses to evaluate it first
            if (Expression is IOperator op1 && op1.GetPrecedence() > GetPrecedence())
            {
                op1.SetHasParentheses(true);
            }
            Expression.Parenthesize();
        }

        public void SetHasParentheses(bool paren)
        {
            HasParentheses = paren;
        }
    }

    public sealed class FunctionCall : Expression
    {
        public Expression Function;
        public readonly List<Expression> Args;

        public uint BeginArg = 0;
        
        /// <summary>
        /// Set to true if the number of values returned from this call isn't explicitly stated in the opcode and
        /// needs analysis to resolve.
        /// </summary>
        public bool HasAmbiguousReturnCount = false;
        
        /// <summary>
        /// Set to true if the number of arguments for this call isn't explicitly stated in the opcode and
        /// needs analysis to resolve.
        /// </summary>
        public bool HasAmbiguousArgumentCount = false;

        /// <summary>
        /// Index of where the function def register was originally defined. Used to help decide what expressions to inline
        /// </summary>
        public int FunctionDefIndex = -1;

        /// <summary>
        /// Call is a "this" call where the first argument is implicit
        /// </summary>
        public bool IsThisCall;

        // Once the self variables are replaced in this call, the first argument should be ignored for any further use
        // counts/iterations/queries since in the Lua source it functions as a single use semantically
        private bool _selfReplaced;

        public FunctionCall(Expression fun, List<Expression> args)
        {
            Function = fun;
            Args = args;
        }

        public override void Parenthesize()
        {
            Function.Parenthesize();
            Args.ForEach(x => x.Parenthesize());
            
            // ':' and '.' have higher precedence than other binary operators so we must make sure we parenthesize
            // the table expression if needed.
            if (Function is TableAccess
                {
                    Table: IOperator o,
                    TableIndex: Constant { ConstType: Constant.ConstantType.ConstString }
                })
            {
                o.SetHasParentheses(true);
            }
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                Args[i].GetUsedRegisters(uses);
            }
            Function.GetUsedRegisters(uses);
            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            
            temporaries.AddToTemporaryRegisterRange(Function.GetOriginalUseRegisters());
            foreach (var t in Args)
            {
                temporaries.AddToTemporaryRegisterRange(t.GetOriginalUseRegisters());
            }

            temporaries.MergeTemporaryRegisterRange(Function.GetTemporaryRegisterRange());
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                temporaries.MergeTemporaryRegisterRange(Args[i].GetTemporaryRegisterRange());
            }
            
            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            Function.RenameUses(original, newIdentifier);
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                Args[i].RenameUses(original, newIdentifier);
            }
        }

        public override bool ReplaceUses(Identifier original, Expression sub)
        {
            bool replaced;
            if (ShouldReplace(original, Function, sub) && 
                sub is IdentifierReference or TableAccess or Constant or FunctionCall)
            {
                if (sub is TableAccess { IsSelfReference : true })
                {
                    IsThisCall = true;
                }
                Function = sub;
                replaced = true;
            }
            else
            {
                replaced = Function.ReplaceUses(original, sub);
            }
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                if (ShouldReplace(original, Args[i], sub))
                {
                    Args[i] = sub;
                    replaced = true;
                    if (IsThisCall && i == 0)
                        _selfReplaced = true;
                }
                else
                {
                    replaced = replaced || Args[i].ReplaceUses(original, sub);
                }
            }
            return replaced;
        }

        public override int UseCount(Identifier use)
        {
            int count = Function.UseCount(use);
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                count += Args[i].UseCount(use);
            }
            return count;
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression> { this };
            ret.AddRange(Function.GetExpressions());
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                ret.AddRange(Args[i].GetExpressions());
            }
            return ret;
        }

        public override int GetLowestConstantId()
        {
            var id = Function.GetLowestConstantId();
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                var nid = Args[i].GetLowestConstantId();
                if (id == -1)
                {
                    id = nid;
                }
                else if (nid != -1)
                {
                    id = Math.Min(id, Args[i].GetLowestConstantId());
                }
            }
            return id;
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                result = result || Args[i].MatchAny(condition);
            }

            result = result || Function.MatchAny(condition);
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            IterateUsesSuccessor(Function, UseType.Closure, function);
            for (var i = _selfReplaced ? 1 : 0; i < Args.Count; i++)
            {
                IterateUsesSuccessor(Args[i], UseType.Argument, function);
            }
        }
    }
}
