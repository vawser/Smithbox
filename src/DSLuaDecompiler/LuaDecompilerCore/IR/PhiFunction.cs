using System.Collections.Generic;
using System.Linq;

namespace LuaDecompilerCore.IR
{
    public sealed class PhiFunction : Instruction
    {
        public Identifier Left;
        public readonly List<Identifier> Right;

        public PhiFunction(Identifier left, List<Identifier> right)
        {
            Left = left;
            Right = right;
        }

        public override HashSet<Identifier> GetDefinedRegisters(HashSet<Identifier> defines)
        {
            defines.Add(Left);
            return defines;
        }
        
        public override Identifier? GetSingleDefine()
        {
            return Left;
        }

        public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
        {
            foreach (var id in Right)
            {
                if (!id.IsNull)
                    uses.Add(id);
            }

            return uses;
        }

        public override void RenameDefines(Identifier original, Identifier newIdentifier)
        {
            if (Left == original)
            {
                Left = newIdentifier;
            }
        }

        public override void RenameUses(Identifier original, Identifier newIdentifier)
        {
            for (var i = 0; i < Right.Count; i++)
            {
                if (Right[i] != original) continue;
                Right[i] = newIdentifier;
            }
        }
    }
}
