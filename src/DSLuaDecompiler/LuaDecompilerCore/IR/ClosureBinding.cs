using System.Collections.Generic;

namespace LuaDecompilerCore.IR;

/// <summary>
/// Instruction that is intended to follow a closure assignment instruction. Adds the contained identifier
/// to the previously defined closure
/// </summary>
public class ClosureBinding : Instruction
{
    public Identifier Identifier;

    public ClosureBinding(Identifier identifier)
    {
        Identifier = identifier;
    }
    
    public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
    {
        if (Identifier.IsRegister)
            uses.Add(Identifier);
        return uses;
    }
    
    public override void RenameUses(Identifier original, Identifier newIdentifier)
    {
        if (Identifier == original)
        {
            Identifier = newIdentifier;
        }
    }

    public override int UseCount(Identifier use)
    {
        return (Identifier == use) ? 1 : 0;
    }
}