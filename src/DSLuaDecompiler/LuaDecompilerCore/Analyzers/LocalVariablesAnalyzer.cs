using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.Analyzers;

/// <summary>
/// Analyzer that identifies identifiers that are likely to be local variables and therefore should not have their
/// definitions inlined into other expressions.
/// </summary>
public class LocalVariablesAnalyzer : IAnalyzer
{
    private readonly HashSet<Identifier> _localVariables = new(10);

    public IReadOnlySet<Identifier> LocalVariables => _localVariables;
    
    public void Run(DecompilationContext decompilationContext, FunctionContext functionContext, Function function)
    {
        var dominance = functionContext.GetAnalysis<DominanceAnalyzer>();
        
        // GetDefinedRegisters and GetUsedRegisters calls have a lot of allocation overhead so reusing the same set has huge perf gains.
        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        
        // Lua function calls (and expressions in general) have their bytecode generated recursively.
        // This means for example when doing a function call, the name of the function is loaded to a register first,
        // then all the subexpressions are computed, and finally the function is called. We can exploit this knowledge
        // to determine which expressions were actually inlined into the function call in the original source code.
        // This analysis finds the pre-propagation index in the instruction list of each instruction as well as the
        // index of the first definition instruction contributing to a function call expression.
        var defines = new Dictionary<Identifier, int>();
        var selfIdentifiers = new HashSet<Identifier>();
        
        foreach (var b in function.BlockList)
        {
            defines.Clear();
            selfIdentifiers.Clear();
            foreach (var instruction in b.Instructions)
            {
                if (instruction.GetSingleDefine() is { } define)
                {
                    defines.Add(define, instruction.InstructionIndices.Begin);
                    if (instruction is Assignment
                        {
                            SelfAssignMinRegister: < int.MaxValue,
                            IsSingleAssignment: true,
                            Right: IdentifierReference { Identifier.IsRegister: true } or 
                            TableAccess { Table: IdentifierReference { IsRegister: true } }
                        })
                    {
                        selfIdentifiers.Add(define);
                    }
                }

                switch (instruction)
                {
                    case Assignment
                    {
                        Right: FunctionCall
                        {
                            FunctionDefIndex: -1,
                            Function: IAssignable { IsRegisterBase: true } fir
                        } fc
                    } when defines.ContainsKey(fir.RegisterBase):
                    {
                        fc.FunctionDefIndex = defines[fir.RegisterBase];
                        if (selfIdentifiers.Contains(fir.RegisterBase))
                        {
                            // If a self op was used, the first arg will be loaded before the function name
                            fc.FunctionDefIndex--;
                        }

                        break;
                    }
                    // Detect tail calls
                    case Return 
                    { 
                        ReturnExpressions: [FunctionCall 
                        { 
                            FunctionDefIndex: -1, 
                            Function: IAssignable { IsRegisterBase: true } fir2 
                        } fc2] }:
                    {
                        fc2.FunctionDefIndex = defines[fir2.RegisterBase];
                        if (selfIdentifiers.Contains(fir2.RegisterBase))
                        {
                            // If a self op was used, the first arg will be loaded before the function name
                            fc2.FunctionDefIndex--;
                        }

                        break;
                    }
                }
            }
        }
        
        // Set of intervals to store the range of renamed registers that are local for an actual register
        var registerCount = function.RegisterCount;
        var blockLocals = ArrayPool<Interval>.Shared.Rent(registerCount);
        bool IsLocal(Identifier identifier)
        {
            return _localVariables.Contains(identifier) ||
                   (blockLocals[identifier.RegNum].Count > 0 &&
                    blockLocals[identifier.RegNum].Contains((int)identifier.RegSubscriptNum));
        }

        void AddLocal(Identifier identifier)
        {
            blockLocals[identifier.RegNum].AddToRange((int)identifier.RegSubscriptNum);
        }

        void FlushLocals()
        {
            for (uint i = 0; i < registerCount; i++)
            {
                if (blockLocals[i].Count > 0)
                {
                    for (var j = (uint)blockLocals[i].Begin; j < blockLocals[i].End; j++)
                        _localVariables.Add(Identifier.GetRenamedRegister(i, j));
                }
            }
        }
        
        // Lua, with its simple one-pass compiler, doesn't really have a register allocation algorithm of any kind.
        // Local variables are allocated to a single register for the lifetime of the entire scope, while temp locals
        // generated by the compiler for complex expressions only have a lifetime for that expression (i.e. once the
        // expression is done evaluating, that register is free to be used in that scope again. Of course, we can
        // exploit this to figure out local variables in the original source code even if they only had one use:
        // If the next register defined within the scope (dominance hierarchy) after the first use of a recently
        // defined register is not that register, then it's likely an actual local variable.
        // ReSharper disable AccessToModifiedClosure
        int LocalIdentifyVisit(BasicBlock b, int incomingMaxLocalRegister)
        {
            var thisMaxLocalRegister = incomingMaxLocalRegister;
            
            void MarkTemporary(Identifier identifier)
            {
                // Set max local register and clear any locals above that
                Debug.Assert(identifier.RegNum > incomingMaxLocalRegister);
                thisMaxLocalRegister = Math.Min((int)identifier.RegNum - 1, thisMaxLocalRegister);
                for (uint i = identifier.RegNum; i < registerCount; i++)
                {
                    if (blockLocals[i].Count == 0)
                        break;
                    blockLocals[i] = new Interval();
                }
            }

            // Set of defines that are never used in this block (very very likely to be locals)
            var unusedDefines = new HashSet<Identifier>(b.Instructions.Count / 2);

            // Set of recently used registers that are candidates for locals
            var recentlyUsed = new Dictionary<uint, Identifier>(b.Instructions.Count / 2);
            
            for (var i = 0; i < blockLocals.Length; i++)
                blockLocals[i] = new Interval();
            
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                // First add registers such that the set contains all the registers used up to this point
                uint minTemporaryUse = int.MaxValue;
                Interval temporaryUses = new Interval();
                b.Instructions[i].IterateUses((node, useType, useReference) =>
                {
                    var use = useReference.Identifier;
                    
                    // If it's used it's no longer an unused definition
                    if (unusedDefines.Contains(use))
                    {
                        unusedDefines.Remove(use);
                    }
                    
                    // If the instruction (such as set list) says that incoming registers above a certain value are
                    // always temporaries, then unmark them as locals if they are already marked.
                    if (use.RegNum >= b.Instructions[i].AlwaysTemporaryRegister)
                    {
                        MarkTemporary(use);
                        recentlyUsed.Remove(use.RegNum);
                    }
                    
                    if (use.RegNum <= thisMaxLocalRegister)
                    {
                        // Already marked as a local
                        return;
                    }

                    if (IsLocal(use))
                    {
                        // Add it to the local regs
                        thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)use.RegNum);
                        return;
                    }

                    // IR uses are iterated in the same order that the compiler emits them, which means that each use of
                    // a temporary register should occur in increasing order. Any uses that occur out of this increasing
                    // order are probably references to existing global variables. Additionally, uses of a register that
                    // was already used without being redefined is also probably a local.
                    if (recentlyUsed.ContainsKey(use.RegNum) || 
                        (node is not BinOp && temporaryUses.Count > 0 && use.RegNum < temporaryUses.End))
                    {
                        AddLocal(use);
                        thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)use.RegNum);
                        recentlyUsed.Remove(use.RegNum);
                        return;
                    }

                    // Otherwise this is a candidate for a temporary variable
                    recentlyUsed.Add(use.RegNum, use);
                    minTemporaryUse = Math.Min(use.RegNum, minTemporaryUse);
                    temporaryUses.AddToRange((int)use.RegNum);
                });
                // ReSharper restore AccessToModifiedClosure

                definesSet.Clear();
                b.Instructions[i].GetDefinedRegisters(definesSet);
                
                // If this instruction has no defines, but has inlined expressions that did define, then we still need
                // to compare the outstanding recently used against what has been inlined to detect if any of them are
                // local.
                /*if (definesSet.Count == 0 && temporaryUses.Count > 0 && b.Instructions[i].InlinedRegisters.Count > 0)
                {
                    if (temporaryUses.Begin < b.Instructions[i].InlinedRegisters.Begin)
                        thisMaxLocalRegister = Math.Min((int)b.Instructions[i].AlwaysTemporaryRegister - 1,
                            Math.Max(thisMaxLocalRegister, 
                            Math.Min(temporaryUses.End - 1, b.Instructions[i].InlinedRegisters.Begin)));
                    foreach (var use in recentlyUsed)
                    {
                        if (temporaryUses.Contains((int)use.Key) && use.Key <= thisMaxLocalRegister)
                            AddLocal(use.Value);
                    }
                    recentlyUsed.Clear();
                }*/
                
                // Analyze each define individually. Multiple assignment definitions are actually either all locals or
                // all temporaries, but hopefully we can get away without modeling that for now.
                foreach (var def in definesSet)
                {
                    // Self instructions have a lot of information because they always use the next available temp
                    // registers. This means that any pending uses up until this instruction that haven't been redefined
                    // yet are actually locals.
                    if (b.Instructions[i] is
                        {
                            SelfAssignMinRegister: < int.MaxValue, InlinedRegisters.Count: 0
                        } instruction)
                    {
                        foreach (var k in recentlyUsed.Keys)
                        {
                            // If the reg number is less than the second define then it's a local
                            if (k < instruction.SelfAssignMinRegister)
                            {
                                AddLocal(recentlyUsed[k]);
                                thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)k);
                            }
                        }

                        recentlyUsed.Clear();
                        i++;
                        continue;
                    }

                    // If due to the bytecode implications this assignment is *always* a declaration of a new local,
                    // then we need to invalidate previous local declarations for this register
                    if (b.Instructions[i] is Assignment { IsLocalDeclaration: true })
                    {
                        AddLocal(def);
                        thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)def.RegNum);
                        
                        // Since this is a local declaration, pending uses that are lower are also locals while those
                        // greater are temporaries
                        foreach (var k in recentlyUsed.Keys)
                        {
                            if (k < def.RegNum)
                            {
                                AddLocal(recentlyUsed[k]);
                            }
                        }
                        recentlyUsed.Clear();
                        
                        // Since this is the local declaration for this register in this scope, we need to backtrack
                        // and mark prior locals of this register number as actually temporaries
                        blockLocals[def.RegNum].SetBegin((int)def.RegSubscriptNum);
                        continue;
                    }

                    // Move on if it's a known local
                    if (def.RegNum <= thisMaxLocalRegister)
                    {
                        // Make sure the def is marked as local
                        AddLocal(def);
                        continue;
                    }

                    // Add the new def to the unused defs until it's used otherwise
                    unusedDefines.Add(def);

                    // When the Lua compiler selects a register to assign, it selects the next free register that isn't
                    // currently assigned to a local variable or a temporary in the expression. This means that we can
                    // we can infer a couple of things:
                    // 1) If we are redefining a presumed temporary register that was recently used, it is fairly likely
                    //    that this is a temporary register given that it was the first selected register. This means
                    //    that we can mark all the pending live values that have a register value lower than the def
                    //    as locals and the ones higher as defs.
                    // 2) If this assignment has a use that is presumed to be temporary that has a register number that
                    //    is lower than the defined register number, then that indicates that temporaries that are
                    //    "killed" aren't having their register number reused which indicates that they are likely
                    //    actually locals instead.
                    if (recentlyUsed.ContainsKey(def.RegNum) || def.RegNum > minTemporaryUse)
                    {
                        foreach (var k in recentlyUsed.Keys)
                        {
                            // If the reg number is less than the second define then it's a local
                            if (k < def.RegNum)
                            {
                                AddLocal(recentlyUsed[k]);
                                thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)k);
                            }
                        }

                        recentlyUsed.Clear();
                    }
                }
            }

            // Any unused defines at this point are locals
            foreach (var unused in unusedDefines)
            {
                AddLocal(unused);
                thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)unused.RegNum);
            }
            
            // Commit the local ranges
            FlushLocals();

            // Visit next blocks in scope. Here we get the lowest register defined in the successor blocks that
            // isn't already a redefinition of an already identified local, as that represents the baseline of what
            // the Lua compiler can select for a new temporary register. Any pending registers used in this block
            // that have a register number below that are very likely to be local variables since Lua doesn't select
            // them for temporaries.
            var minFirstChildTemporaryDefine = int.MaxValue;
            var maxRegister = thisMaxLocalRegister;
            
            // Loop exits may have dangling local loop variables that need to go out of scope, since the loop end
            // dominates the following blocks in Lua.
            if (b.KilledLocals is { Count: > 0 })
                maxRegister = Math.Min(maxRegister, b.KilledLocals.Begin - 1);
            dominance.RunOnDominanceTreeSuccessors(function, b, successor =>
            {
                var fd = LocalIdentifyVisit(successor, maxRegister);
                minFirstChildTemporaryDefine = Math.Min(minFirstChildTemporaryDefine, fd);
            });

            if (minFirstChildTemporaryDefine != int.MaxValue)
            {
                foreach (var k in recentlyUsed.Keys)
                {
                    // If the reg number is less than the first assigned register in the dominance successors that
                    // isn't a redefinition of an already identified local, then k likely represents a local since
                    // the Lua compiler isn't selecting it for a temporary or new local.
                    if (k < minFirstChildTemporaryDefine)
                    {
                        _localVariables.Add(recentlyUsed[k]);
                        thisMaxLocalRegister = Math.Max(thisMaxLocalRegister, (int)k);
                    }
                }
            }

            // Find the register of the first define that isn't an incoming local to return
            var firstTempDef = int.MaxValue;
            foreach (var inst in b.Instructions)
            {
                if (inst.InlinedRegisters.Count > 0)
                {
                    firstTempDef = inst.InlinedRegisters.Begin;
                }

                if (inst.GetSingleDefine() is { } def && def.RegNum > incomingMaxLocalRegister)
                {
                    firstTempDef = Math.Min(firstTempDef, (int)def.RegNum);
                }
                
                firstTempDef = Math.Min(firstTempDef, inst.SelfAssignMinRegister);
                
                if (firstTempDef != int.MaxValue)
                    break;
            }

            // If we don't have any new defines then pass up the first define from the dominance successors
            if (firstTempDef == int.MaxValue)
                firstTempDef = minFirstChildTemporaryDefine;
            
            return firstTempDef;
        }
        
        LocalIdentifyVisit(function.BeginBlock, function.ParameterCount - 1);
    }
    
    public void Dispose()
    {
    }
}