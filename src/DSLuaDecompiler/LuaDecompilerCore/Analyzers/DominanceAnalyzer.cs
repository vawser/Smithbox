using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.CFG;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.Analyzers;

/// <summary>
/// Computes dominance information (dominance, dominance tree, dominance frontier, etc)
/// </summary>
public class DominanceAnalyzer : IAnalyzer
{
    private uint[]? _immediateDominators;
    private JaggedArray<uint>? _dominance;
    private JaggedArray<uint>? _dominanceTreeSuccessors;
    private JaggedArray<uint>? _dominanceFrontier;

    private uint Intersect(IReadOnlyList<BasicBlock> blocks, uint b1, uint b2)
    {
        Debug.Assert(_immediateDominators != null);
        var finger1 = b1;
        var finger2 = b2;
        while (finger1 != finger2)
        {
            while (blocks[(int)finger1].ReversePostorderNumber > blocks[(int)finger2].ReversePostorderNumber)
            {
                finger1 = _immediateDominators[blocks[(int)finger1].BlockIndex];
            }

            while (blocks[(int)finger2].ReversePostorderNumber > blocks[(int)finger1].ReversePostorderNumber)
            {
                finger2 = _immediateDominators[blocks[(int)finger2].BlockIndex];
            }
        }

        return finger1;
    }
    
    public void Run(DecompilationContext decompilationContext, FunctionContext functionContext, Function function)
    {
        // This method is very optimized to minimize allocations as the naive implementation was one of the largest
        // performance and allocation bottlenecks.
        
        // List the blocks in reverse postorder and initialize the immediate dominator
        var reversePostorderBlocks = function.NumberReversePostorder(false);
        var blockCount = function.BlockList.Count;
        _immediateDominators = ArrayPool<uint>.Shared.Rent(blockCount);
        for (uint i = 0; i < blockCount; i++)
        {
            _immediateDominators[i] = i;
        }
        
        // Use Cooper-Harvey-Kennedy algorithm for fast computation of immediate dominators
        // http://www.hipersoft.rice.edu/grads/publications/dom14.pdf
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var block in reversePostorderBlocks)
            {
                // Begin block is always its only dominator
                if (block == function.BeginBlock) continue;
                    
                // Intersect all the predecessors to find the new dominator
                var firstProcessed = block.Predecessors
                    .First(b => block.ReversePostorderNumber > b.ReversePostorderNumber);
                uint newDominator = (uint)firstProcessed.BlockIndex;
                foreach (var predecessor in block.Predecessors)
                {
                    if (predecessor == firstProcessed) continue;
                    if (_immediateDominators[predecessor.BlockIndex] != predecessor.BlockIndex || 
                        predecessor == function.BeginBlock)
                    {
                        newDominator = Intersect(function.BlockList, (uint)predecessor.BlockIndex, newDominator);
                    }
                }

                // if dominator is unchanged go to the next block
                if (_immediateDominators[block.BlockIndex] == newDominator) continue;
                    
                // We have a new dominator
                _immediateDominators[block.BlockIndex] = newDominator;
                changed = true;
            }
        }

        // Compute full dominance and successor counts. For huge control flow graphs we may need to use something
        // sparser for the working dominance sets
        var perBlockCounts = ArrayPool<uint>.Shared.Rent(blockCount);
        Array.Clear(perBlockCounts, 0, blockCount);
        using var perBlockSets = new BitSetArray(blockCount, blockCount);
        for (var i = 0; i < blockCount; i++)
        {
            var index = reversePostorderBlocks[i].BlockIndex;
            perBlockSets.Set(index, index, true);
            if (_immediateDominators[index] == index) continue;
            perBlockSets.Set(index, (int)_immediateDominators[index], true);
            perBlockCounts[_immediateDominators[index]]++;
            perBlockSets.Or(index, (int)_immediateDominators[index], perBlockSets);
        }
        _dominanceTreeSuccessors = new JaggedArray<uint>(new ReadOnlySpan<uint>(perBlockCounts, 0, blockCount));
        
        // Compute dominance counts
        for (var i = 0; i < blockCount; i++)
        {
            perBlockCounts[i] = perBlockSets.PopCount(i);
        }
        _dominance = new JaggedArray<uint>(new ReadOnlySpan<uint>(perBlockCounts, 0, blockCount));
        
        // Fill the jagged arrays
        Array.Clear(perBlockCounts, 0, blockCount);
        for (uint i = 0; i < blockCount; i++)
        {
            perBlockSets.CopySetIndicesToSpan((int)i, _dominance.Value[(int)i]);
            var dominator = (int)_immediateDominators[i];
            if (dominator == i) continue;
            _dominanceTreeSuccessors.Value[dominator][(int)perBlockCounts[dominator]] = i;
            perBlockCounts[dominator]++;
        }
        
        // Compute dominance frontier
        perBlockSets.ClearAll();
        for (var i = 0; i < blockCount; i++)
        {
            var b = function.BlockList[i];
            if (function.BlockList[i].Predecessors.Count <= 1) continue;
            foreach (var p in b.Predecessors)
            {
                var runner = p.BlockIndex;
                while (runner != _immediateDominators[i])
                {
                    perBlockSets.Set(runner, i, true);
                    runner = (int)_immediateDominators[runner];
                }
            }
        }
        
        // Build dominance frontier array
        Array.Clear(perBlockCounts, 0, blockCount);
        for (var i = 0; i < blockCount; i++)
        {
            perBlockCounts[i] = perBlockSets.PopCount(i);
        }
        _dominanceFrontier = new JaggedArray<uint>(new ReadOnlySpan<uint>(perBlockCounts, 0, blockCount));
        Array.Clear(perBlockCounts, 0, blockCount);
        for (uint i = 0; i < blockCount; i++)
        {
            perBlockSets.CopySetIndicesToSpan((int)i, _dominanceFrontier.Value[(int)i]);
        }
        
        ArrayPool<uint>.Shared.Return(perBlockCounts);
    }

    public ReadOnlySpan<uint> Dominance(int block)
    {
        if (_dominance == null)
            throw new Exception("Analysis not run");
        return _dominance.Value.ReadOnlySpan(block);
    }
    
    public ReadOnlySpan<uint> DominanceFrontier(int block)
    {
        if (_dominanceFrontier == null)
            throw new Exception("Analysis not run");
        return _dominanceFrontier.Value.ReadOnlySpan(block);
    }
    
    public ReadOnlySpan<uint> DominanceTreeSuccessors(int block)
    {
        if (_dominanceTreeSuccessors == null)
            throw new Exception("Analysis not run");
        return _dominanceTreeSuccessors.Value.ReadOnlySpan(block);
    }
    
    public void RunOnDominanceTreeSuccessors(Function f, BasicBlock b, Action<BasicBlock> action)
    {
        if (_dominanceTreeSuccessors == null)
            throw new Exception("Analysis not run");
        foreach (var s in _dominanceTreeSuccessors.Value.ReadOnlySpan(b.BlockIndex))
        {
            action(f.BlockList[(int)s]);
        }
    }

    public void Dispose()
    {
        if (_immediateDominators != null)
            ArrayPool<uint>.Shared.Return(_immediateDominators);
        _dominance?.Dispose();
        _dominanceTreeSuccessors?.Dispose();
        _dominanceFrontier?.Dispose();
    }
}