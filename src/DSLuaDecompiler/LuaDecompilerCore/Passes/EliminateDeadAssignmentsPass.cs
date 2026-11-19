using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

public class EliminateDeadAssignmentsPass : IPass
{
    private readonly bool _phiOnly;

    public EliminateDeadAssignmentsPass(bool phiOnly)
    {
        _phiOnly = phiOnly;
    }
    
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var irChanged = false;
        
        // GetDefinedRegisters and GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        
        var usedUses = new HashSet<Identifier>(10);
        var usageCounts = new Dictionary<Identifier, int>(f.ParameterCount + f.SsaVariables.Count);
        var phiToRemove = new List<Identifier>(10);
        bool changed = true;
        while (changed)
        {
            changed = false;
            usageCounts.Clear();
            for (uint reg = 0; reg < f.ParameterCount; reg++)
            {
                usageCounts.Add(Identifier.GetRegister(reg), 0);
            }

            // Used for phi function cycle detection
            var singleUses = new Dictionary<Identifier, PhiFunction>();

            // Do a reverse-postorder traversal to register all the definitions and uses
            foreach (var b in f.PostorderTraversal(true))
            {
                // Defines/uses in phi functions
                foreach (var phi in b.PhiFunctions)
                {
                    usedUses.Clear();
                    foreach (var use in phi.Value.Right)
                    {
                        // If a phi function has multiple uses of the same identifier, only count it as one use for
                        // the purposes of this analysis
                        if (!use.IsNull && !usedUses.Contains(use))
                        {
                            usageCounts.TryAdd(use, 0);
                            usageCounts[use]++;
                            if (usageCounts[use] == 1)
                            {
                                if (!singleUses.Remove(use))
                                {
                                    singleUses.Add(use, phi.Value);
                                }
                            }
                            else
                            {
                                singleUses.Remove(use);
                            }
                            usedUses.Add(use);
                        }
                    }

                    usageCounts.TryAdd(phi.Value.Left, 0);
                }

                // Defines/uses for everything else
                foreach (var inst in b.Instructions)
                {
                    usesSet.Clear();
                    definesSet.Clear();
                    inst.GetUsedRegisters(usesSet);
                    inst.GetDefinedRegisters(definesSet);
                    foreach (var use in usesSet)
                    {
                        usageCounts.TryAdd(use, 0);
                        usageCounts[use]++;
                        singleUses.Remove(use);
                    }
                    foreach (var def in definesSet)
                    {
                        usageCounts.TryAdd(def, 0);
                    }
                }
            }

            // Do an elimination pass
            foreach (var b in f.BlockList)
            {
                // Eliminate unused phi functions
                phiToRemove.Clear();
                foreach (var phi in b.PhiFunctions)
                {
                    if (usageCounts[phi.Value.Left] == 0)
                    {
                        changed = true;
                        irChanged = true;
                        phiToRemove.Add(phi.Value.Left);
                    }

                    // If this phi function has a single use, which is also a phi function, and that phi function has
                    // a single use, which is this phi function, then we have a useless phi function dependency cycle
                    // that can be broken and removed
                    if (singleUses.ContainsKey(phi.Value.Left) && singleUses.ContainsKey(singleUses[phi.Value.Left].Left) &&
                        singleUses[singleUses[phi.Value.Left].Left] == phi.Value)
                    {
                        changed = true;
                        irChanged = true;
                        phiToRemove.Add(phi.Value.Left);
                        singleUses[phi.Value.Left].RenameUses(phi.Value.Left, Identifier.GetNull());
                    }
                }
                phiToRemove.ForEach(x => b.PhiFunctions.Remove(x.RegNum));

                // Eliminate unused assignments
                var toRemove = new List<Instruction>();
                foreach (var inst in b.Instructions)
                {
                    if (inst.GetSingleDefine() is {} define && usageCounts[define] == 0)
                    {
                        if (inst is Assignment { Right: FunctionCall } a && !_phiOnly)
                        {
                            a.LeftList.Clear();
                        }
                        else
                        {
                            if (!_phiOnly)
                            {
                                changed = true;
                                toRemove.Add(inst);
                            }
                        }
                    }
                }
                toRemove.ForEach(x => b.Instructions.Remove(x));
            }
        }

        return irChanged;
    }
}