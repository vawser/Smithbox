using System.Collections.Generic;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Eliminate phi functions that don't end up being used at all by actual code
/// </summary>
public class EliminateUnusedPhiFunctionsPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        bool irChanged = false;
        
        // GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var usesSet = new HashSet<Identifier>(10);
        
        var phisToKeep = new HashSet<PhiFunction>(f.BlockList.Count);
        var usedIdentifiers = new HashSet<Identifier>(f.SsaVariables.Count);

        // First, iterate through all the non-phi instructions to get all the used identifiers
        foreach (var b in f.BlockList)
        {
            foreach (var inst in b.Instructions)
            {
                usesSet.Clear();
                inst.GetUsedRegisters(usesSet);
                foreach (var use in usesSet)
                {
                    usedIdentifiers.Add(use);
                }
            }
        }

        // Next do an expansion cycle where phi functions that use the identifiers are marked kept, and then phi
        // functions that the marked phi uses are also kept
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var b in f.BlockList)
            {
                foreach (var phi in b.PhiFunctions)
                {
                    if (!phisToKeep.Contains(phi.Value) && usedIdentifiers.Contains(phi.Value.Left))
                    {
                        phisToKeep.Add(phi.Value);
                        foreach (var use in phi.Value.Right)
                        {
                            if (!use.IsNull)
                            {
                                usedIdentifiers.Add(use);
                            }
                        }

                        changed = true;
                    }
                }
            }
        }

        // Now prune any phi functions that aren't marked
        foreach (var b in f.BlockList)
        {
            var phiToRemove = new List<uint>();
            foreach (var phi in b.PhiFunctions)
            {
                if (!phisToKeep.Contains(phi.Value))
                {
                    phiToRemove.Add(phi.Key);
                }
            }

            if (phiToRemove.Count > 0)
                irChanged = true;
            
            phiToRemove.ForEach(x => b.PhiFunctions.Remove(x));
        }

        return irChanged;
    }
}