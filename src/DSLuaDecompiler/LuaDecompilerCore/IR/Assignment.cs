using System;
using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// IL for an assignment operation
    /// Identifier := Expr
    /// </summary>
    public sealed class Assignment : Instruction
    {
        /// <summary>
        /// Functions can have multiple returns, so we store the assignment identifiers as a list
        /// </summary>
        public readonly List<IAssignable> LeftList;
        public Expression? Right;

        /// <summary>
        /// Convenience for when there's only a single assignment identifier
        /// </summary>
        public IAssignable Left => LeftList[0];

        /// <summary>
        /// If debug info exist, these are the local variables that are assigned if any (null if none are assigned and thus a "temp")
        /// </summary>
        public List<LuaFile.Local>? LocalAssignments = null;
        
        /// <summary>
        /// When this is set to true, the value defined by this is always expression/constant propagated,
        /// even if it's used more than once
        /// </summary>
        public bool PropagateAlways = false;

        /// <summary>
        /// This assignment represents an assignment to an ambiguous number of varargs
        /// </summary>
        public bool IsAmbiguousVararg = false;
        public uint VarargAssignmentReg = 0;

        public uint NilAssignmentReg = 0;

        /// <summary>
        /// Is the first assignment of a local variable, and thus starts with "local"
        /// </summary>
        public bool IsLocalDeclaration = false;

        /// <summary>
        /// If true, the assignment uses "in" instead of "="
        /// </summary>
        public bool IsGenericForAssignment = false;

        /// <summary>
        /// If true, this is a list assignment which affects how expression propagation is done
        /// </summary>
        public bool IsListAssignment = false;

        /// <summary>
        /// Assignment only assigns to a single identifier
        /// </summary>
        public bool IsSingleAssignment => LeftList.Count == 1;
        
        /// <summary>
        /// Assignment assigns to multiple identifiers
        /// </summary>
        public bool IsMultiAssignment => LeftList.Count > 1;

        public bool LeftAny => LeftList.Count > 0;

        private void InitDefinedRegisters()
        {
            foreach (var l in LeftList)
            {
                if (l is IdentifierReference { Identifier: { IsRegister: true } identifier })
                {
                    DefinedRegisters.AddToRange((int)identifier.RegNum);
                }
            }
        }
        
        public Assignment(Identifier l, Expression? r)
        {
            LeftList = new List<IAssignable>(1) { new IdentifierReference(l) };
            Right = r;
            if (l.IsRegister)
                Right?.OriginalAssignmentRegisters.AddToRange((int)l.RegNum);
            InitDefinedRegisters();
        }

        public Assignment(IAssignable l, Expression? r)
        {
            LeftList = new List<IAssignable>(1) { l };
            Right = r;
            if (l is IdentifierReference { Identifier: { IsRegister:true } identifier})
                Right?.OriginalAssignmentRegisters.AddToRange((int)identifier.RegNum);
            InitDefinedRegisters();
        }

        public Assignment(List<IAssignable> l, Expression? r)
        {
            LeftList = l;
            Right = r;
            foreach (var left in l)
            {
                if (left is IdentifierReference { Identifier: { IsRegister:true } identifier})
                    Right?.OriginalAssignmentRegisters.AddToRange((int)identifier.RegNum);
            }
            InitDefinedRegisters();
        }

        public override void Parenthesize()
        {
            LeftList.ForEach(x => x.Parenthesize());
            Right?.Parenthesize();
        }

        public override HashSet<Identifier> GetDefinedRegisters(HashSet<Identifier> set)
        {
            foreach (var id in LeftList)
            {
                // If the reference is not an indirect one (i.e. not an array access), then it is a definition
                if (id is IdentifierReference { Identifier: { IsRegister: true } identifier })
                {
                    set.Add(identifier);
                }
            }
            return set;
        }

        public override Identifier? GetSingleDefine()
        {
            Identifier? ret = null;
            int count = 0;
            foreach (var id in LeftList)
            {
                // If the reference is not an indirect one (i.e. not an array access), then it is a definition
                if (id is IdentifierReference { Identifier: { IsRegister: true } identifier })
                {
                    ret = identifier;
                    count++;
                }
            }

            return count == 1 ? ret : null;
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var id in LeftList)
            {
                if (id is TableAccess access)
                {
                    access.GetUsedRegisters(uses);
                }
            }

            Right?.GetUsedRegisters(uses);
            return uses;
        }

        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            foreach (var id in LeftList)
            {
                if (id is Expression e and not IdentifierReference)
                    temporaries.AddToTemporaryRegisterRange(e.GetOriginalUseRegisters());
            }

            if (Right != null)
            {
                temporaries.AddToTemporaryRegisterRange(Right.GetOriginalUseRegisters());
            }
            
            foreach (var id in LeftList)
            {
                if (id is Expression e and not IdentifierReference)
                    temporaries.MergeTemporaryRegisterRange(e.GetTemporaryRegisterRange());
            }

            if (Right != null)
            {
                temporaries.MergeTemporaryRegisterRange(Right.GetTemporaryRegisterRange());
            }

            return temporaries;
        }

        public override void RenameDefines(Identifier original, Identifier newIdentifier)
        {
            foreach (var id in LeftList)
            {
                // If the reference is not an indirect one (i.e. not a table access), then it is a definition
                if (id is IdentifierReference ir && ir.Identifier == original)
                {
                    ir.Identifier = newIdentifier;
                }
            }
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            foreach (var id in LeftList)
            {
                if (id is TableAccess access)
                {
                    access.RenameUses(original, newIdentifier);
                }
            }
            Right?.RenameUses(original, newIdentifier);
        }

        public override bool ReplaceUses(Identifier orig, Expression sub)
        {
            bool replaced = false;
            foreach (var l in LeftList)
            {
                if (l is TableAccess access)
                {
                    replaced = replaced || access.ReplaceUses(orig, sub);
                }
            }
            if (Right != null && Expression.ShouldReplace(orig, Right, sub))
            {
                replaced = true;
                Right = sub;
            }
            else
            {
                replaced = replaced || (Right != null && Right.ReplaceUses(orig, sub));
            }
            return replaced;
        }

        public override int UseCount(Identifier use)
        {
            return LeftList.Where(id => id is TableAccess)
                       .Sum(id => (id as TableAccess)!.UseCount(use)) + (Right?.UseCount(use) ?? 0);
        }

        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            foreach (var exp in LeftList)
            {
                result = result || exp.MatchAny(condition);
            }

            result = result || (Right != null && Right.MatchAny(condition));
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            foreach (var id in LeftList)
            {
                if (id is TableAccess)
                {
                    id.IterateUses(function);
                }
            }

            if (Right != null)
                IterateUsesSuccessor(Right, UseType.Assignee, function);
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression>();
            foreach (var left in LeftList)
            {
                if (left is TableAccess access)
                    ret.AddRange(access.GetExpressions());
            }
            if (Right != null)
                ret.AddRange(Right.GetExpressions());
            return ret;
        }
    }
}
