using System;
using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR
{
    public sealed class Return : Instruction
    {
        public readonly List<Expression> ReturnExpressions;
        public uint BeginRet = 0;
        public bool IsAmbiguousReturnCount = false;
        public bool IsImplicit = false;
        public bool IsTailReturn = false;

        public Return(List<Expression> expr)
        {
            ReturnExpressions = expr;
        }

        public Return(Expression expr)
        {
            ReturnExpressions = new List<Expression> { expr };
        }

        public override void Parenthesize()
        {
            ReturnExpressions.ForEach(x => x.Parenthesize());
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var exp in ReturnExpressions)
            {
                exp.GetUsedRegisters(uses);
            }

            return uses;
        }
        
        public override Interval GetTemporaryRegisterRange()
        {
            var temporaries = new Interval();
            foreach (var e in ReturnExpressions)
            {
                temporaries.AddToTemporaryRegisterRange(e.GetOriginalUseRegisters());
            }
        
            foreach (var e in ReturnExpressions)
            {
                temporaries.MergeTemporaryRegisterRange(e.GetTemporaryRegisterRange());
            }

            return temporaries;
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            foreach (var exp in ReturnExpressions)
            {
                exp.RenameUses(original, newIdentifier);
            }
        }

        public override bool ReplaceUses(Identifier orig, Expression sub)
        {
            var replace = false;
            for (var i = 0; i < ReturnExpressions.Count; i++)
            {
                if (Expression.ShouldReplace(orig, ReturnExpressions[i], sub))
                {
                    ReturnExpressions[i] = sub;
                    replace = true;
                }
                else
                {
                    replace = replace || ReturnExpressions[i].ReplaceUses(orig, sub);
                }
            }
            return replace;
        }

        public override int UseCount(Identifier use)
        {
            return ReturnExpressions.Sum(e => e.UseCount(use));
        }
        
        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            foreach (var exp in ReturnExpressions)
            {
                result = result || exp.MatchAny(condition);
            }
            return result;
        }

        public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
        {
            foreach (var exp in ReturnExpressions)
            {
                IterateUsesSuccessor(exp, UseType.Argument, function);
            }
        }

        public override List<Expression> GetExpressions()
        {
            var ret = new List<Expression>();
            foreach (var r in ReturnExpressions)
            {
                ret.AddRange(r.GetExpressions());
            }
            return ret;
        }
        
    }
}
