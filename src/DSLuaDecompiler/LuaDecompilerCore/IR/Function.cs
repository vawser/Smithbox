using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR
{
    /// <summary>
    /// A Lua function. A function contains a CFG, a list of instructions, and child functions used for closures
    /// </summary>
    public sealed class Function
    {
        public List<Function> Closures { get; }

        public Dictionary<uint, Label> Labels { get; }
        
        public LuaFile.Constant[] Constants { get; set; }

        /// <summary>
        /// When the CFG has been converted to an AST
        /// </summary>
        public bool IsAst { get; set; }

        /// <summary>
        /// The first basic block in which control flow enters upon the function being called
        /// </summary>
        public BasicBlock BeginBlock { get; set; }

        /// <summary>
        /// The final (empty) basic block that is the successor to the end of the function and any blocks that end with a return instruction
        /// </summary>
        public BasicBlock EndBlock { get; set; }

        private List<BasicBlock> _blockList;

        /// <summary>
        /// List of all the blocks for some analyses
        /// </summary>
        public IReadOnlyList<BasicBlock> BlockList => _blockList;

        /// <summary>
        /// Identifiers that are used in more than one basic block
        /// </summary>
        public HashSet<Identifier> GlobalIdentifiers { get; set; }

        /// <summary>
        /// All the renamed SSA variables
        /// </summary>
        public HashSet<Identifier> SsaVariables { get; set; }

        public uint[]? RenamedRegisterCounts { get; set; } = null;
        
        /// <summary>
        /// Unique identifier for the function used for various purposes
        /// </summary>
        public int FunctionId { get; }

        public bool InsertDebugComments { get; set; }

        public List<LuaFile.Local>? ArgumentNames = null;

        public bool IsVarargs = false;

        /// <summary>
        /// For each up value in Lua 5.3, the register in the parent its bound to
        /// </summary>
        public readonly List<int> UpValueRegisterBinding = new();

        /// <summary>
        /// For each up value in Lua 5.3, if the up value exists on the stack
        /// </summary>
        public readonly List<bool> UpValueIsStackBinding = new();

        /// <summary>
        /// UpValue binding symbol from parent closure
        /// </summary>
        public readonly List<Identifier> UpValueBindings = new();

        public readonly Dictionary<Identifier, string> IdentifierNames = new();
        
        public readonly List<string> Warnings = new();

        private int _currentBlockId;
        
        /// <summary>
        /// Number of parameters in this function
        /// </summary>
        public int ParameterCount { get; set; }
        
        /// <summary>
        /// Number of registers (before renaming) that this function uses. This includes the registers
        /// used for function parameters.
        /// </summary>
        public int RegisterCount { get; set; }
        
        /// <summary>
        /// Number of upValues this function uses
        /// </summary>
        public int UpValueCount = 0;

        public Function(int functionId)
        {
            Closures = new List<Function>();
            Labels = new Dictionary<uint, Label>();
            _blockList = new List<BasicBlock>();
            GlobalIdentifiers = new HashSet<Identifier>();
            SsaVariables = new HashSet<Identifier>();
            FunctionId = functionId;
            
            // Create initial basic block
            _blockList.Add(CreateBasicBlock());
            BeginBlock = EndBlock = BlockList[0];
        }

        public void AddClosure(Function fun)
        {
            Closures.Add(fun);
        }

        public Function LookupClosure(uint i)
        {
            return Closures[(int)i];
        }

        public Label GetLabel(uint pc)
        {
            if (Labels.TryGetValue(pc, out var value))
            {
                value.UsageCount++;
                return Labels[pc];
            }
            var label = new Label(Labels.Count)
            {
                OpLocation = (int)pc,
                UsageCount = 1
            };
            Labels.Add(pc, label);
            return label;
        }
        
        /// <summary>
        /// Gets a register identifier and updates the register count if needed
        /// </summary>
        /// <param name="reg">Register number</param>
        /// <returns>Identifier representing this register</returns>
        public Identifier GetRegister(uint reg)
        {
            if (reg + 1 > RegisterCount)
                RegisterCount = (int)reg + 1;
            return Identifier.GetRegister(reg);
        }

        /// <summary>
        /// Gets or looks up a new UpValue identifier and inserts it into the local symbol table
        /// </summary>
        /// <param name="upValue">UpValue number</param>
        /// <returns>Identifier representing this UpValue</returns>
        public static Identifier GetUpValue(uint upValue)
        {
            return Identifier.GetUpValue(upValue);
        }
        
        public static Identifier GetStackUpValue(uint upValue)
        {
            return Identifier.GetStackUpValue(upValue);
        }

        public void ClearBlocks()
        {
            _blockList.Clear();
        }
        
        public BasicBlock CreateBasicBlock(bool end = false)
        {
            return new BasicBlock(end ? int.MaxValue : _currentBlockId++);
        }

        public void AddBasicBlock(BasicBlock block)
        {
            block.BlockIndex = _blockList.Count;
            _blockList.Add(block);
        }

        public void RemoveBlockAt(int index)
        {
            _blockList.RemoveAt(index);
            for (var i = index; i < BlockList.Count; i++)
            {
                BlockList[i].BlockIndex = i;
            }
        }

        public void RemoveAllBlocks(Predicate<BasicBlock> match)
        {
            _blockList.RemoveAll(match);
            RefreshBlockIndices();
        }
        
        public BasicBlock CreateAndAddBasicBlock()
        {
            var b = new BasicBlock(_currentBlockId++)
            {
                BlockIndex = _blockList.Count
            };
            _blockList.Add(b);
            return b;
        }

        /// <summary>
        /// Gets the next block in the program order regardless of control flow.
        /// </summary>
        public BasicBlock? NextBlock(BasicBlock block)
        {
            return block.BlockIndex + 1 >= _blockList.Count ? null : _blockList[block.BlockIndex + 1];
        }

        private void RefreshBlockIndices()
        {
            for (var i = 0; i < BlockList.Count; i++)
            {
                BlockList[i].BlockIndex = i;
            }
        }

        /// <summary>
        /// Compute global liveness information for all registers in the function
        /// </summary>
        public void ComputeGlobalLiveness(HashSet<Identifier> allRegisters)
        {
            RefreshBlockIndices();

            // Map each identifier to an ID
            var identifierToId = new Dictionary<Identifier, int>(allRegisters.Count);
            int index = 0;
            var allRegistersArray = allRegisters.ToArray();
            foreach (var register in allRegistersArray)
            {
                identifierToId[register] = index++;
            }

            void BitSetFromSet(BitSetArray.BitSet target, HashSet<Identifier> set, bool invert = false)
            {
                foreach (var identifier in set)
                {
                    target[identifierToId[identifier]] = !invert;
                }
            }

            HashSet<Identifier> SetFromBitSet(BitSetArray.BitSet set)
            {
                var ret = new HashSet<Identifier>(set.Count / 4);
                for (int i = 0; i < set.Count; i++)
                {
                    if (set[i])
                        ret.Add(allRegistersArray[i]);
                }

                return ret;
            }

            // Compute killed and upward exposed for each block
            using var killedIdentifiers = new BitSetArray(BlockList.Count, allRegisters.Count);
            using var upwardExposedIdentifiers = new BitSetArray(BlockList.Count, allRegisters.Count);
            using var globalIdentifiers = new BitSetArray(1, allRegisters.Count);
            var definesSet = new HashSet<Identifier>(2);
            var usesSet = new HashSet<Identifier>(10);
            foreach (var block in BlockList)
            {
                foreach (var phi in block.PhiFunctions)
                {
                    definesSet.Clear();
                    phi.Value.GetDefinedRegisters(definesSet);
                    foreach(var def in definesSet)
                    {
                        killedIdentifiers.Set(block.BlockIndex, identifierToId[def], true);
                    }
                }
                
                foreach (var inst in block.Instructions)
                {
                    if (inst is not PhiFunction)
                    {
                        usesSet.Clear();
                        inst.GetUsedRegisters(usesSet);
                        foreach (var use in usesSet)
                        {
                            if (killedIdentifiers.Get(block.BlockIndex, identifierToId[use])) continue;
                            upwardExposedIdentifiers.Set(block.BlockIndex, identifierToId[use], true);
                            globalIdentifiers.Set(0, identifierToId[use], true);
                        }
                    }
                    definesSet.Clear();
                    inst.GetDefinedRegisters(definesSet);
                    foreach(var def in definesSet)
                    {
                        killedIdentifiers.Set(block.BlockIndex, identifierToId[def], true);
                    }
                }
                
                killedIdentifiers.Not(block.BlockIndex);
            }
            
            // Build bitsets for needed sets from the blocks
            using var liveOut = new BitSetArray(BlockList.Count + 2, allRegisters.Count);

            // Compute live out for each block iteratively. Working sets are the last 2 of the liveout array
            var equation = liveOut[^2];
            var temp = liveOut[^1];
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var block in BlockList)
                {
                    temp.SetAll(false);
                    foreach (var successor in block.Successors)
                    {
                        equation.SetAll(true);
                        equation.And(killedIdentifiers[successor.BlockIndex]);
                        equation.And(liveOut[successor.BlockIndex]);
                        equation.Or(upwardExposedIdentifiers[successor.BlockIndex]);
                        temp.Or(equation);
                    }
                    if (!liveOut[block.BlockIndex].CompareCopyFrom(temp))
                    {
                        changed = true;
                    }
                }
            }

            foreach (var block in BlockList)
            {
                block.LiveOut = SetFromBitSet(liveOut[block.BlockIndex]);
                killedIdentifiers.Not(block.BlockIndex);
                block.KilledIdentifiers = SetFromBitSet(killedIdentifiers[block.BlockIndex]);
                block.UpwardExposed = SetFromBitSet(upwardExposedIdentifiers[block.BlockIndex]);
            }

            GlobalIdentifiers = SetFromBitSet(globalIdentifiers[0]);
        }

        public BasicBlock[] PostorderTraversal(bool reverse, bool skipEndBlock = true)
        {
            var ret = new BasicBlock[skipEndBlock ? BlockList.Count - 1 : BlockList.Count];
            using var visitedArray = new BitSetArray(1, BlockList.Count);
            var visited = visitedArray[0];
            RefreshBlockIndices();
            if (skipEndBlock)
            {
                visited[EndBlock.BlockIndex] = true;
            }

            var counter = 0;
            void Visit(BasicBlock b)
            {
                visited[b.BlockIndex] = true;
                foreach (var successor in b.Successors)
                {
                    if (!visited[successor.BlockIndex])
                    {
                        Visit(successor);
                    }
                }
                ret[counter] = b;
                counter++;
            }

            Visit(BeginBlock);

            if (reverse)
            {
                for (var i = 0; i < ret.Length / 2; i++)
                {
                    (ret[i], ret[ret.Length - i - 1]) = (ret[ret.Length - i - 1], ret[i]);
                }
            }
            return ret;
        }

        /// <summary>
        /// Labels all the blocks in the CFG with a number in order of their reverse postorder traversal
        /// </summary>
        public BasicBlock[] NumberReversePostorder(bool skipEndBlock = true)
        {
            var ordering = PostorderTraversal(true, skipEndBlock);
            for (var i = 0; i < ordering.Length; i++)
            {
                ordering[i].ReversePostorderNumber = i;
            }

            return ordering;
        }
        
        public override string ToString()
        {
            return FunctionPrinter.DebugPrintFunction(this);
        }
    }
}
