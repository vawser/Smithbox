using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Drop SSA by simply dropping the subscripts. This requires no interference in the live ranges of the definitions
/// </summary>
public class DropSsaSubscriptsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        
        // GetDefinedRegisters and GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        
        foreach (var b in f.BlockList)
        {
            foreach (var phi in b.PhiFunctions)
            {
                b.PhiMerged.Add(Identifier.GetRegister(phi.Value.Left.RegNum));
            }
            b.PhiFunctions.Clear();
            foreach (var i in b.Instructions)
            {
                definesSet.Clear();
                usesSet.Clear();
                i.GetDefinedRegisters(definesSet);
                i.GetUsedRegisters(usesSet);
                foreach (var def in definesSet)
                {
                    if (def.IsRenamedRegister)
                        i.RenameDefines(def, Identifier.GetRegister(def.RegNum));
                }
                foreach (var use in usesSet)
                {
                    if (use.IsRenamedRegister)
                        i.RenameUses(use, Identifier.GetRegister(use.RegNum));
                }
            }
        }

        var counter = 0;
        Identifier NewName(Identifier orig)
        {
            var newName = Identifier.GetRenamedRegister(orig.RegNum, (uint)counter);
            counter++;
            return newName;
        }

        // If we have debug information, we can split up variables based on if a definition is associated with the start
        // of a local variable. If so, everything dominated by the containing block is renamed to that definition
        void Visit(CFG.BasicBlock b, Dictionary<Identifier, Identifier> replacements)
        {
            var newReplacements = new Dictionary<Identifier, Identifier>(replacements);

            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var phi in b.PhiMerged.ToList())
                {
                    if (newReplacements.TryGetValue(phi, out var replacement))
                    {
                        b.PhiMerged.Remove(phi);
                        b.PhiMerged.Add(replacement);
                        changed = true;
                    }
                }
            }
            // Walk down all the instructions, replacing things that need to be replaced and renaming as needed
            foreach (var instruction in b.Instructions)
            {
                changed = true;
                var reassigned = false;
                Identifier? newDef = null;
                while (changed)
                {
                    changed = false;
                    usesSet.Clear();
                    instruction.GetUsedRegisters(usesSet);
                    foreach (var use in usesSet)
                    {
                        if (newReplacements.TryGetValue(use, out var value) && newReplacements[use] != newDef)
                        {
                            instruction.RenameUses(use, value);
                            changed = true;
                        }
                    }
                    definesSet.Clear();
                    instruction.GetDefinedRegisters(definesSet);
                    foreach (var def in definesSet)
                    {
                        if (instruction is Assignment { LocalAssignments: not null } && !reassigned)
                        {
                            var newName = NewName(def);
                            instruction.RenameDefines(def, newName);
                            if (newReplacements.ContainsKey(def))
                            {
                                newReplacements[def] = newName;
                                newDef = newName;
                            }
                            else
                            {
                                newReplacements.Add(def, newName);
                                newDef = newName;
                            }
                            changed = true;
                            reassigned = true;
                        }
                        else if (newReplacements.TryGetValue(def, out var replacement))
                        {
                            instruction.RenameDefines(def, replacement);
                            changed = true;
                        }
                    }
                }
            }

            // Propagate to children in the dominance hierarchy
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                Visit(successor, newReplacements);
            });
        }
        Visit(f.BeginBlock, new Dictionary<Identifier, Identifier>());

        return true;
    }
}