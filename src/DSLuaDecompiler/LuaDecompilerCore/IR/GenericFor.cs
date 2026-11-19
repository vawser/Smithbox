using System;
using LuaDecompilerCore.CFG;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// A numeric for statement
    /// for var in exp do
    ///     something
    /// end
    /// </summary>
    public sealed class GenericFor : Instruction
    {
        public readonly Assignment Iterator;
        public readonly BasicBlock Body;
        public readonly BasicBlock? Follow;

        public GenericFor(Assignment iterator, BasicBlock body, BasicBlock? follow)
        {
            Iterator = iterator;
            Body = body;
            Follow = follow;
        }
        
        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            result = result || Iterator.MatchAny(condition);
            return result;
        }
    }
}
