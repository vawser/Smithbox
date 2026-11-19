using System;
using System.Collections.Generic;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Does global liveness analysis to verify no copies are needed coming out of SSA form
/// </summary>
public class ValidateLivenessNoInterferencePass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        // GetDefinedRegisters and GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        
        // Just computes liveout despite the name
        f.ComputeGlobalLiveness(f.SsaVariables);

        var globalLiveness = new Dictionary<Identifier, HashSet<Identifier>>();
        // Initialise the disjoint sets
        foreach (var id in f.SsaVariables)
        {
            globalLiveness.Add(id, new HashSet<Identifier> { id });
        }

        // Do a super shitty suboptimal union find algorithm to merge all the global ranges using phi functions
        // Rewrite this with a proper union-find if performance becomes an issue (lol)
        foreach (var b in f.BlockList)
        {
            foreach (var phi in b.PhiFunctions.Values)
            {
                foreach (var r in phi.Right)
                {
                    if (!r.IsNull && globalLiveness[phi.Left] != globalLiveness[r])
                    {
                        globalLiveness[phi.Left].UnionWith(globalLiveness[r]);
                        globalLiveness[r] = globalLiveness[phi.Left];
                    }
                }
            }
        }

        foreach (var b in f.BlockList)
        {
            var liveNow = new HashSet<Identifier>(b.LiveOut);
            for (var i = b.Instructions.Count - 1; i >= 0; i--)
            {
                definesSet.Clear();
                b.Instructions[i].GetDefinedRegisters(definesSet);
                foreach (var def in definesSet)
                {
                    foreach (var live in liveNow)
                    {
                        if (live != def && live.RegNum == def.RegNum)
                        {
                            f.Warnings.Add("-- Warning: SSA live range interference detected in function " +
                                $"{f.FunctionId} ({live} overlaps with {def}). Results are probably wrong.");
                        }
                    }
                    liveNow.Remove(def);
                }
                usesSet.Clear();
                b.Instructions[i].GetUsedRegisters(usesSet);
                foreach (var use in usesSet)
                {
                    liveNow.Add(use);
                }
            }
        }

        return false;
    }
}