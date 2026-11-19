using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detects and labels declarations of local variables. These are the first definitions of variables
/// in a dominance hierarchy.
/// </summary>
public class DetectLocalVariablesPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        var changed = false;
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        var definesSet = new HashSet<Identifier>(2);
        
        // This is kinda both a pre and post-order traversal of the dominance hierarchy. In the pre traversal,
        // first local definitions are detected, marked, and propagated down the graph so that they aren't marked
        // again. In the postorder traversal, these marked definitions are backpropagated up the dominance hierarchy.
        // If a node gets multiple marked nodes for the same variable from its children in the dominance hierarchy,
        // a new local assignment must be inserted right before the node splits.
        Dictionary<Identifier, List<Assignment>> Visit(CFG.BasicBlock b, HashSet<Identifier> declared)
        {
            var newDeclared = new HashSet<Identifier>(declared);
            var declaredAssignments = new Dictionary<Identifier, List<Assignment>>();

            // Go through the graph and mark declared nodes
            foreach (var inst in b.Instructions)
            {
                if (inst is Assignment a)
                {
                    definesSet.Clear();
                    foreach (var def in a.GetDefinedRegisters(definesSet))
                    {
                        // Make this the local declaration if it hasn't been declared yet
                        if (!newDeclared.Contains(def))
                        {
                            newDeclared.Add(def);
                            a.IsLocalDeclaration = true;
                            declaredAssignments.Add(def, new List<Assignment> { a });
                        }
                    }
                }
            }
            
            // Visit and merge the children in the dominance hierarchy
            var inherited = new Dictionary<Identifier, List<Assignment>>();
            var phiInduced = new HashSet<Identifier>();
            dominance.RunOnDominanceTreeSuccessors(f, b, successor =>
            {
                // Loop heads may have temporary local variables that are declared before the actual loop itself that
                // need to go out of scope upon entry of the follow. We must manually make sure we don't send these
                // local variables into the follow or take them back up in the inherited.
                var loopFollowKilledLocals = b.IsLoopHead && b.LoopFollow == successor && b.KilledLocals.Count > 0;
                var successorDeclared = newDeclared;
                if (loopFollowKilledLocals)
                {
                    successorDeclared = new HashSet<Identifier>(newDeclared);
                    for (uint i = 0; i < b.KilledLocals.Count; i++)
                    {
                        successorDeclared.Remove(Identifier.GetRegister((uint)b.KilledLocals.Begin + i));
                    }
                }
                
                var cDeclared = Visit(successor, successorDeclared);
                foreach (var entry in cDeclared)
                {
                    // Don't take up loop locals that were killed entering the loop follow
                    if (loopFollowKilledLocals && b.KilledLocals.Contains((int)entry.Key.RegNum))
                        continue;
                    
                    if (!inherited.ContainsKey(entry.Key))
                    {
                        inherited.Add(entry.Key, new List<Assignment>(entry.Value));
                    }
                    else
                    {
                        inherited[entry.Key].AddRange(entry.Value);
                    }
                }

                phiInduced.UnionWith(successor.PhiMerged);
            });
            foreach (var entry in inherited)
            {
                if (entry.Value.Count > 1 && phiInduced.Contains(entry.Key))
                {
                    // Multiple incoming declarations that all have the same use need to be merged
                    var assignment = new Assignment(entry.Key, null)
                    {
                        IsLocalDeclaration = true
                    };
                    b.Instructions.Insert(b.Instructions.Count - 1, assignment);
                    declaredAssignments.Add(entry.Key, new List<Assignment> { assignment });
                    foreach (var e in entry.Value)
                    {
                        e.IsLocalDeclaration = false;
                    }

                    changed = true;
                }
                else
                {
                    declaredAssignments.Add(entry.Key, entry.Value);
                }
            }

            return declaredAssignments;
        }

        var root = new HashSet<Identifier>(f.ParameterCount);
        for (uint i = 0; i < f.ParameterCount; i++)
        {
            root.Add(Identifier.GetRegister(i));
        }
        Visit(f.BeginBlock, root);
        return changed;
    }
}