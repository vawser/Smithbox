using System;
using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.CFG
{
    /// <summary>
    /// A block of instructions in which control flow only enters at the beginning and leaves at the end
    /// </summary>
    public sealed class BasicBlock
    {
        public readonly int BlockId;
        private List<BasicBlock> _predecessors;
        private List<BasicBlock> _successors;
        public IReadOnlyList<BasicBlock> Predecessors => _predecessors;
        public IReadOnlyList<BasicBlock> Successors => _successors;
        public List<Instruction> Instructions;
        public Dictionary<uint, PhiFunction> PhiFunctions;

        public readonly HashSet<Identifier> PhiMerged = new();

        /// <summary>
        /// Local variable register range that is always killed (made available for temporaries again) upon leaving this
        /// block into dominance successors.
        /// </summary>
        public Interval KilledLocals;

        // Live analysis stuff
        public int BlockIndex;
        public HashSet<Identifier> KilledIdentifiers;
        public HashSet<Identifier> UpwardExposed;
        public HashSet<Identifier> LiveOut;

        /// <summary>
        /// Register IDs of registers killed (i.e. redefined) under the scope of this block (excluding this block)
        /// </summary>
        public HashSet<uint> ScopeKilled = new();

        /// <summary>
        /// If this is set to true, then the "true" edge was unreachable logically but a control flow edge was inserted
        /// when building the CFG.
        /// </summary>
        public bool UnreachableSuccessor = false;
        
        // Control flow analysis
        public int ReversePostorderNumber = 0;
        public bool IsLoopHead = false;
        public bool IsLoopLatch = false;
        public BasicBlock? LoopFollow = null;
        public readonly List<BasicBlock> LoopLatches = new();
        public LoopType LoopType = LoopType.LoopNone;
        public BasicBlock? Follow = null;
        
        // Set to true if both the true and false branch lead to a return
        public bool IfOrphaned = false;

        // Code gen
        public bool IsInfiniteLoop = false;
        
        /// <summary>
        /// Indicates that this block was completely empty in the source bytecode and any jumps in it were decompiler
        /// generated.
        /// </summary>
        public bool IsEmptyIf = false;
        
        private bool _isCodeGenerated;

        /// <summary>
        /// Printer friendly name of the basic block
        /// </summary>
        public string Name => $"basicblock_{BlockId}";

        /// <summary>
        /// First instruction in instruction list
        /// </summary>
        public Instruction First => Instructions.First();

        /// <summary>
        /// Last instruction in instruction list
        /// </summary>
        public Instruction Last
        {
            get => Instructions[^1];
            set => Instructions[^1] = value;
        }

        /// <summary>
        /// If block is a condition, the successor block for when the condition is "true"
        /// </summary>
        public BasicBlock EdgeTrue
        {
            get => Successors[0];
            set => ChangeSuccessor(0, value);
        }
        
        /// <summary>
        /// If block is a condition, the successor block for when the condition is "false"
        /// </summary>
        public BasicBlock EdgeFalse
        {
            get => Successors[1];
            set => ChangeSuccessor(1, value);
        }

        /// <summary>
        /// Block is a conditional block where a successor is chosen based on a conditional jump
        /// </summary>
        public bool IsConditionalJump => Successors.Count == 2 && Last is ConditionalJump;

        /// <summary>
        /// Block ends with an unconditional jump with a single successor
        /// </summary>
        public bool IsUnconditionalJump => Successors.Count == 1 && Last is Jump;
        
        /// <summary>
        /// Block ends with an explicit conditional or unconditional jump to another block
        /// </summary>
        public bool IsJump => Last is Jump or ConditionalJump;
        
        /// <summary>
        /// Block is a return block that ends the function call when the block is done executing
        /// </summary>
        public bool IsReturn => Successors.Count == 1 && Last is Return;

        /// <summary>
        /// Block falls through to the next logical block in the program order without an explicit jump or return
        /// </summary>
        public bool IsFallthrough => Successors.Count == 1 && !IsJump && !IsReturn;

        /// <summary>
        /// This block is a branch/leaf off of an if statement
        /// </summary>
        public bool IsConditionalBranch =>
            Predecessors is [{ Follow: not null, HasInstructions: true, IsConditionalJump: true }] &&
            Predecessors[0].Follow != this;
        
        public bool Empty => Instructions.Count == 0;
        public bool HasInstructions => Instructions.Count > 0;

        public bool HasBackEdge => Successors.Any(s => s.BlockId < BlockId);

        /// <summary>
        /// Debug validation of block to ensure it is well formed and no unexpected/illegal states have formed. Validation
        /// is local to the block and does not validate the entire CFG.
        /// </summary>
        public void Validate(BasicBlock beginBlock, BasicBlock endBlock)
        {
            // End block should have >1 predecessor, no successors, and no instructions
            if (this == endBlock)
            {
                if (endBlock.Predecessors.Count == 0)
                    throw new Exception($@"End block {Name} has no predecessors");
                if (endBlock.Successors.Count > 0)
                    throw new Exception($@"End block {Name} has successors");
                if (endBlock.HasInstructions)
                    throw new Exception($@"End block has instructions");
                return;
            }
            
            // Begin block should have no predecessors
            if (this == beginBlock && Predecessors.Count > 0)
                throw new Exception($@"Begin block {Name} has predecessors");
            
            // Blocks that end with return should have a single end block as successor
            if (HasInstructions && Last is Return && (Successors.Count != 1 || Successors[0] == endBlock))
                throw new Exception($@"Block {Name} has return but does not have end block as single successor");
            
            // All blocks except the end block should have 1 or 2 successors in current implementation
            if (Successors.Count is < 1 or > 2)
                throw new Exception($@"Block {Name} has {Successors.Count} successors");
            
            // Blocks with a single successor should not have a conditional jump
            if (Successors.Count == 1 && HasInstructions && Last is ConditionalJump)
                throw new Exception($@"Block {Name} has 1 successor but ends with conditional jump");
            
            // Blocks with a two successors must have a conditional jump
            if (Successors.Count == 2 && (Empty || Last is not ConditionalJump))
                throw new Exception($@"Block {Name} has 2 successor but does not end with conditional jump");
            
            ValidateBasic();
        }

        public void ValidateBasic()
        {
            // Ensure all successors have us as a predecessor
            foreach (var successor in Successors)
            {
                if (!successor.Predecessors.Contains(this))
                    throw new Exception($@"{Name} successor {successor} does not contain {Name} as predecessor");
            }
            
            // Ensure all predecessors have us as a successors
            foreach (var predecessor in Predecessors)
            {
                if (!predecessor.Successors.Contains(this))
                    throw new Exception($@"{Name} predecessor {predecessor} does not contain {Name} as successor");
            }
        }
        
        public BasicBlock(int blockId)
        {
            BlockId = blockId;
            _predecessors = new List<BasicBlock>();
            _successors = new List<BasicBlock>();
            Instructions = new List<Instruction>(10);
            PhiFunctions = new Dictionary<uint, PhiFunction>();
            KilledIdentifiers = new HashSet<Identifier>();
            LiveOut = new HashSet<Identifier>();
        }

        /// <summary>
        /// Adds a new instruction to the block
        /// </summary>
        /// <param name="instruction">The instruction to add</param>
        public void AddInstruction(Instruction instruction)
        {
            instruction.OriginalBlock = BlockId;
            Instructions.Add(instruction);
        }

        /// <summary>
        /// Inserts an instruction at the specified index
        /// </summary>
        /// <param name="index">The index to insert the instruction at</param>
        /// <param name="instruction">The instruction to insert</param>
        public void InsertInstruction(int index, Instruction instruction)
        {
            instruction.OriginalBlock = BlockId;
            Instructions.Insert(index, instruction);
        }

        public void AbsorbInstructions(BasicBlock block, bool changeOriginal = false)
        {
            foreach (var instruction in block.Instructions)
            {
                if (changeOriginal && instruction.OriginalBlock == block.BlockId)
                    instruction.OriginalBlock = BlockId;
            }
            Instructions.AddRange(block.Instructions);
            block.Instructions.Clear();
        }

        /// <summary>
        /// Ensures predecessors are always sorted by their block index. List is small so this shouldn't be that costly
        /// </summary>
        private void SortPredecessors()
        {
            _predecessors.Sort((l, r) => l.BlockIndex < r.BlockIndex ? -1 : 1);
        }

        /// <summary>
        /// Adds a new successor to the block
        /// </summary>
        /// <param name="successor">The block to add as a successor</param>
        public void AddSuccessor(BasicBlock successor)
        {
            _successors.Add(successor);
            successor._predecessors.Add(this);
            successor.SortPredecessors();
        }

        /// <summary>
        /// Inserts a new successor at the specified index
        /// </summary>
        /// <param name="index">The index to insert to</param>
        /// <param name="successor">The block to add as a successor</param>
        public void InsertSuccessor(int index, BasicBlock successor)
        {
            _successors.Insert(index, successor);
            successor._predecessors.Add(this);
            successor.SortPredecessors();
        }
        
        /// <summary>
        /// Removes a successor from the block if it exists
        /// </summary>
        /// <param name="successor">The block to remove as a successor</param>
        public void RemoveSuccessor(BasicBlock successor)
        {
            _successors.Remove(successor);
            successor._predecessors.Remove(this);
        }

        /// <summary>
        /// Replace an existing successor with a new one
        /// </summary>
        /// <param name="index">The index of the successor to replace</param>
        /// <param name="successor">The successor to change to</param>
        public void ChangeSuccessor(int index, BasicBlock successor)
        {
            if (index >= Successors.Count)
                throw new Exception("Successor out of range");
            _successors[index]._predecessors.Remove(this);
            _successors[index] = successor;
            successor._predecessors.Add(this);
            successor.SortPredecessors();
        }

        /// <summary>
        /// Takes the successors from another block and adds them to this one. This will clear the successors of the
        /// other block.
        /// </summary>
        /// <param name="block">The block to take successors from</param>
        public void StealSuccessors(BasicBlock block)
        {
            foreach (var successor in block.Successors)
            {
                _successors.Add(successor);
                var idx = successor._predecessors.IndexOf(block);
                successor._predecessors[idx] = this;
                successor.SortPredecessors();
                successor.ValidateBasic();
            }
            block._successors.Clear();
            ValidateBasic();
        }

        /// <summary>
        /// Clear all successors to this block
        /// </summary>
        public void ClearSuccessors()
        {
            foreach (var successor in _successors)
            {
                successor._predecessors.Remove(this);
            }
            _successors.Clear();
        }

        public int IndexOfSuccessor(BasicBlock b)
        {
            return _successors.IndexOf(b);
        }
        
        public int IndexOfPredecessor(BasicBlock b)
        {
            return _predecessors.IndexOf(b);
        }
        
        /// <summary>
        /// Get an instruction by index with bounds checking.
        /// </summary>
        public Instruction? GetInstruction(int index)
        {
            if (index >= 0 && index < Instructions.Count)
                return Instructions[index];
            return null;
        }

        /// <summary>
        /// Gets the "peephole" successor, which ultimately follows blocks that are "jumps to jumps" to the final
        /// destination
        /// </summary>
        public BasicBlock PeepholeSuccessor()
        {
            var b = this;
            while (b.Successors.Count == 1 && b.Instructions.Count == 1 && b.Last is Jump)
            {
                b = b.Successors[0];
            }
            return b;
        }

        public void MarkCodeGenerated(int debugFuncId, List<string> warnings)
        {
            if (_isCodeGenerated)
            {
                warnings.Add($"-- Warning: Function {debugFuncId} using already code-generated block {Name}");
            }
            _isCodeGenerated = true;
        }

        public bool IsCodeGenerated => _isCodeGenerated;

        public string ToStringWithDF()
        {
            /*var ret = $@"basicblock_{BlockId}: (DF = {{ ";
            for (var i = 0; i < DominanceFrontier.Count; i++)
            {
                ret += DominanceFrontier.ToArray()[i].Name;
                if (i != DominanceFrontier.Count - 1)
                {
                    ret += ", ";
                }
            }
            ret += " })";
            return ret;*/
            return "";
        }

        public string ToStringWithUpwardExposed()
        {
            /*var ret = $@"basicblock_{BlockId}: (DF = {{ ";
            for (var i = 0; i < UpwardExposedIdentifiers.Count; i++)
            {
                ret += UpwardExposedIdentifiers.ToArray()[i].ToString();
                if (i != UpwardExposedIdentifiers.Count - 1)
                {
                    ret += ", ";
                }
            }
            ret += " })";*/
            return "";
        }

        public string ToStringWithLiveOut()
        {
            var ret = $@"basicblock_{BlockId}: (LiveOut = {{ ";
            for (var i = 0; i < LiveOut.Count; i++)
            {
                ret += LiveOut.ToArray()[i].ToString();
                if (i != LiveOut.Count - 1)
                {
                    ret += ", ";
                }
            }
            ret += " })";
            return ret;
        }

        public string ToStringWithFollow()
        {
            /*var ret = $@"basicblock_{BlockId}:";
            if (Follow != null)
            {
                ret += $@" (Follow: {Follow})";
            }
            ret += $@" (Dominance tree: {{";
            for (var i = 0; i < DominanceTreeSuccessors.Count; i++)
            {
                ret += DominanceTreeSuccessors[i].ToString();
                if (i != DominanceTreeSuccessors.Count - 1)
                {
                    ret += ", ";
                }
            }
            ret += " })";
            return ret;*/
            return "";
        }

        public string ToStringWithLoop()
        {
            var ret = $@"basicblock_{BlockId}:";
            if (IsLoopHead)
            {
                ret += " (Loop head";
                if (LoopType != LoopType.LoopNone)
                {
                    if (LoopType == LoopType.LoopPretested)
                    {
                        ret += ": pretested";
                    }
                    else if (LoopType == LoopType.LoopPosttested)
                    {
                        ret += ": posttested";
                    }
                    else if (LoopType == LoopType.LoopEndless)
                    {
                        ret += ": endless";
                    }
                }
                if (LoopLatches.Count > 0)
                {
                    ret += $@" Latch: ";
                    foreach (var latch in LoopLatches)
                    {
                        ret += $@"{latch}, ";
                    }
                }
                if (LoopFollow != null)
                {
                    ret += $@" LoopFollow: {LoopFollow}";
                }
                if (Follow != null)
                {
                    ret += $@" IfFollow: {Follow}";
                }
                ret += ")";
            }
            else if (Follow != null)
            {
                ret += $@"(IfFollow: {Follow})";
            }
            return ret;
        }
        
        public override string? ToString()
        {
            return FunctionPrinter.DebugPrintBasicBlock(this);
        }
    }
}
