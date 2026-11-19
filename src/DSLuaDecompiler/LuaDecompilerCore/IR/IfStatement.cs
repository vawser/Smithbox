using System;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// Higher level AST node for encoding if statements
    /// </summary>
    public sealed class IfStatement : Instruction
    {
        public required Expression Condition;
        public CFG.BasicBlock? True = null;
        public CFG.BasicBlock? False = null;
        public CFG.BasicBlock? Follow = null;
        public bool IsElseIf = false;
        
        public override bool MatchAny(Func<IIrNode, bool> condition)
        {
            var result = condition.Invoke(this);
            result = result || Condition.MatchAny(condition);
            return result;
        }
    }
}
