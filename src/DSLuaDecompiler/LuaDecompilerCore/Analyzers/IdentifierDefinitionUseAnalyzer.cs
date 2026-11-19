using System;
using System.Collections.Generic;
using System.Diagnostics;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.Analyzers;

/// <summary>
/// Analyzer that analyzes the definitions and use counts of all the identifiers. 
/// </summary>
public class IdentifierDefinitionUseAnalyzer : IAnalyzer
{
    private struct IdentifierInfo
    {
        public Instruction? DefiningInstruction;
        public int DefiningInstructionBlock;
        public int DefiningInstructionIndex;
        public int UseCount;
        public bool IsListInitializerElement;

        public IdentifierInfo()
        {
            UseCount = 0;
            DefiningInstructionBlock = -1;
            DefiningInstructionIndex = -1;
            IsListInitializerElement = false;
        }
    }

    private JaggedArray<IdentifierInfo>? _identifierInfos;

    private ref IdentifierInfo GetIdentifierInfo(Identifier identifier)
    {
        Debug.Assert(_identifierInfos != null);
        return ref _identifierInfos.Value[(int)identifier.RegNum][(int)identifier.RegSubscriptNum];
    }

    private void DetectListInitializerElements(BasicBlock block, int instruction)
    {
        var initializer = block.Instructions[instruction];
        var listIdentifier = ((initializer as Assignment ?? throw new Exception())
            .Left as IdentifierReference ?? throw new Exception()).Identifier;
        Interval elementRegisters = new Interval();
        
        // Match assignments to registers that occur in a linear sequence
        int i;
        for (i = instruction + 1; i < block.Instructions.Count; i++)
        {
            if (block.Instructions[i] is Assignment
                {
                    IsSingleAssignment: true, 
                    Left: IdentifierReference { Identifier.IsRegister: true } ir
                } &&
                (elementRegisters.Count == 0 || ir.Identifier.RegNum == elementRegisters.End))
            {
                elementRegisters.AddToRange((int)ir.Identifier.RegNum);
            }
            else
            {
                break;
            }
        }
        var elementAssignmentEnd = i;
        
        if (elementRegisters.Count == 0)
            return;
        
        // If we have assigned registers, make sure they are all added to the list in sequential order
        var initIndex = 1;
        for (; i < elementAssignmentEnd + elementRegisters.Count; i++)
        {
            if (i >= block.Instructions.Count)
                return;
            if (block.Instructions[i] is Assignment
                {
                    IsSingleAssignment: true,
                    Left: TableAccess { Table: IdentifierReference ir, TableIndex: Constant c },
                    Right: IdentifierReference { Identifier: { IsRegister: true} identifier }
                } &&
                Math.Abs(c.Number - initIndex) < 0.0001 &&
                ir.Identifier == listIdentifier && identifier.RegNum == elementRegisters.Begin + initIndex - 1)
            {
                initIndex++;
            }
            else
            {
                return;
            }
        }
        
        // If we survived all of that, we have a list initializer and can mark the element defines as such
        for (i = instruction + 1; i < elementAssignmentEnd; i++)
        {
            var def = block.Instructions[i].GetSingleDefine() ?? throw new Exception();
            GetIdentifierInfo(def).IsListInitializerElement = true;
        }
    }
    
    public void Run(DecompilationContext decompilationContext, FunctionContext functionContext, Function function)
    {
        if (function.RenamedRegisterCounts == null)
        {
            throw new Exception("Analysis requires SSA form");
        }
        
        _identifierInfos = new JaggedArray<IdentifierInfo>(function.RenamedRegisterCounts, true);

        var definesSet = new HashSet<Identifier>(2);
        var usesSet = new HashSet<Identifier>(10);
        for (var b = 0; b < function.BlockList.Count; b++)
        {
            var block = function.BlockList[b];
            foreach (var phi in block.PhiFunctions)
            {
                definesSet.Clear();
                foreach (var def in phi.Value.GetDefinedRegisters(definesSet))
                {
                    GetIdentifierInfo(def).DefiningInstruction = phi.Value;
                }
                
                usesSet.Clear();
                foreach (var use in phi.Value.GetUsedRegisters(definesSet))
                {
                    GetIdentifierInfo(use).UseCount++;
                }
            }
            
            for (var i = 0; i < block.Instructions.Count; i++)
            {
                var instruction = block.Instructions[i];
                definesSet.Clear();
                foreach (var def in instruction.GetDefinedRegisters(definesSet))
                {
                    GetIdentifierInfo(def).DefiningInstruction = instruction;
                    GetIdentifierInfo(def).DefiningInstructionBlock = b;
                    GetIdentifierInfo(def).DefiningInstructionIndex = i;
                }
                
                usesSet.Clear();
                foreach (var use in instruction.GetUsedRegisters(usesSet))
                {
                    if (!definesSet.Contains(use))
                        GetIdentifierInfo(use).UseCount += instruction.UseCount(use);
                }
                
                // If instruction is an initializer list, we need to follow up and identify element definitions
                if (instruction is Assignment
                    {
                        IsSingleAssignment: true,
                        Left: IdentifierReference,
                        Right: InitializerList
                        {
                            ExpressionsEmpty: true
                        }
                    })
                {
                    DetectListInitializerElements(block, i);
                }
            }
        }
    }

    public Instruction? DefiningInstruction(Identifier identifier)
    {
        if (_identifierInfos == null)
            throw new Exception("Analysis not run");
        
        if (!identifier.IsRenamedRegister)
            return null;
        
        return GetIdentifierInfo(identifier).DefiningInstruction;
    }
    
    public int DefiningInstructionBlock(Identifier identifier)
    {
        if (_identifierInfos == null)
            throw new Exception("Analysis not run");
        
        if (!identifier.IsRenamedRegister)
            return -1;
        
        return GetIdentifierInfo(identifier).DefiningInstructionBlock;
    }
    
    public int DefiningInstructionIndex(Identifier identifier)
    {
        if (_identifierInfos == null)
            throw new Exception("Analysis not run");
        
        if (!identifier.IsRenamedRegister)
            return -1;
        
        return GetIdentifierInfo(identifier).DefiningInstructionIndex;
    }

    public bool IsListInitializerElement(Identifier identifier)
    {
        if (_identifierInfos == null)
            throw new Exception("Analysis not run");
        
        return identifier.IsRenamedRegister && GetIdentifierInfo(identifier).IsListInitializerElement;
    }

    public int UseCount(Identifier identifier)
    {
        if (_identifierInfos == null)
            throw new Exception("Analysis not run");
        
        if (!identifier.IsRenamedRegister)
            return 0;
        
        return GetIdentifierInfo(identifier).UseCount;
    }

    public void Dispose()
    {
        _identifierInfos?.Dispose();
    }
}