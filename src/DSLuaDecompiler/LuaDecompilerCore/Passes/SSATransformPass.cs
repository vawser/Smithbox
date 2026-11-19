using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Converts the function representation to single-static analysis form:
/// * each register is renamed such that values are only assigned once
/// * convergence is handled with phi nodes
/// </summary>
public class SsaTransformPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        // GetDefinedRegisters and GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        
        var allRegisters = new HashSet<Identifier>(f.RegisterCount);
        for (uint r = 0; r < f.RegisterCount; r++)
        {
            allRegisters.Add(Identifier.GetRegister(r));
        }

        f.ComputeGlobalLiveness(allRegisters);

        f.SsaVariables = new HashSet<Identifier>(f.BlockList.Count * 10);

        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        
        // Now insert all the needed phi functions
        foreach (var g in f.GlobalIdentifiers)
        {
            var work = new Queue<CFG.BasicBlock>();
            //var visitedSet = new HashSet<CFG.BasicBlock>();
            foreach (var b in f.BlockList)
            {
                if (b != f.EndBlock && b.KilledIdentifiers.Contains(g))
                {
                    work.Enqueue(b);
                    //visitedSet.Add(b);
                }
            }
            while (work.Count > 0)
            {
                var b = work.Dequeue();
                foreach (var d in dominance.DominanceFrontier(b.BlockIndex))
                {
                    var dBlock = f.BlockList[(int)d];
                    if (d != f.EndBlock.BlockIndex && !dBlock.PhiFunctions.ContainsKey(g.RegNum))
                    {
                        // Heuristic: if the block is just a single return, we don't need phi functions
                        if (dBlock.First is Return { ReturnExpressions.Count: 0 })
                        {
                            continue;
                        }

                        var phiArgs = new List<Identifier>();
                        for (var i = 0; i < dBlock.Predecessors.Count; i++)
                        {
                            phiArgs.Add(g);
                        }
                        dBlock.PhiFunctions.Add(g.RegNum, new PhiFunction(g, phiArgs));
                        //if (!visitedSet.Contains(d))
                        //{
                        work.Enqueue(dBlock);
                        //visitedSet.Add(d);
                        //}
                    }
                }
            }
        }

        // Prepare for renaming
        var counters = new uint[allRegisters.Count];
        var stacks = new Stack<Identifier>[allRegisters.Count];
        foreach (var reg in allRegisters)
        {
            stacks[reg.RegNum] = new Stack<Identifier>(5);
        }

        // Creates a new identifier based on an original identifier
        Identifier NewName(Identifier orig)
        {
            var regNum = orig.RegNum;
            var newName = Identifier.GetRenamedRegister(regNum, counters[regNum]);
            stacks[regNum].Push(newName);
            counters[regNum]++;
            f.SsaVariables.Add(newName);
            return newName;
        }

        void RenameBlock(CFG.BasicBlock b)
        {
            b.ScopeKilled = new HashSet<uint>(b.Instructions.Count / 2);
            
            // Rewrite phi function definitions
            foreach (var phi in b.PhiFunctions)
            {
                var identifier = Identifier.GetRegister(phi.Key);
                phi.Value.RenameDefines(identifier, NewName(identifier));
            }

            // Rename other instructions
            foreach (var inst in b.Instructions)
            {
                usesSet.Clear();
                inst.GetUsedRegisters(usesSet);
                foreach (var use in usesSet)
                {
                    if (stacks[use.RegNum].Count != 0)
                    {
                        inst.RenameUses(use, stacks[use.RegNum].Peek());
                    }
                }
                definesSet.Clear();
                inst.GetDefinedRegisters(definesSet);
                foreach (var def in definesSet)
                {
                    inst.RenameDefines(def, NewName(def));
                }
            }
                
            // Rename successor phi functions
            foreach (var successor in b.Successors)
            {
                if (successor == f.EndBlock) continue;
                var index = successor.IndexOfPredecessor(b);
                foreach (var phi in successor.PhiFunctions)
                {
                    if (phi.Value.Right[index] is { RegNum: var k } && stacks[k].Count > 0)
                    {
                        phi.Value.Right[index] = stacks[k].Peek();
                    }
                    else
                    {
                        // Sometimes a phi function is forced when one of the predecessor paths don't actually define the register.
                        // These phi functions are usually not needed and optimized out in a later pass, so we set it to null to detect
                        // errors in case the phi function result is actually used.
                        phi.Value.Right[index] = Identifier.GetNull();
                    }
                }
            }
                
            // Rename successors in the dominator tree
            foreach (var s in dominance.DominanceTreeSuccessors(b.BlockIndex))
            {
                var successor = f.BlockList[(int)s];
                if (successor != f.EndBlock)
                {
                    RenameBlock(successor);
                }

                // Add to the scope killed set based on the dom tree successor's killed and scope killed
                foreach (var killed in successor.KilledIdentifiers)
                {
                    if (killed.IsRegister)
                    {
                        b.ScopeKilled.Add(killed.RegNum);
                    }
                    b.ScopeKilled.UnionWith(successor.ScopeKilled);
                }
            }

            // Pop off anything we pushed
            foreach (var phi in b.PhiFunctions)
            {
                stacks[phi.Value.Left.RegNum].Pop();
            }
            foreach (var inst in b.Instructions)
            {
                definesSet.Clear();
                inst.GetDefinedRegisters(definesSet);
                foreach (var def in definesSet)
                {
                    stacks[def.RegNum].Pop();
                }
            }
        }
        
        // Rename the arguments first
        for (var i = 0; i < f.ParameterCount; i++)
        {
            NewName(Identifier.GetRegister((uint)i));
        }

        // Rename everything else recursively
        RenameBlock(f.BeginBlock);

        // Save rename counts for future use
        f.RenamedRegisterCounts = counters;

        return true;
    }
}