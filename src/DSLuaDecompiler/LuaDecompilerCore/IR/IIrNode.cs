using System;
using System.Collections.Generic;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR;

/// <summary>
/// High level categorizing of the type of a register "use"
/// </summary>
public enum UseType
{
    /// <summary>
    /// Use is a direct assignee in a list or register assignment
    /// </summary>
    Assignee,
    
    /// <summary>
    /// Use is a table in a table access expression or list assignment
    /// </summary>
    Table,
    
    /// <summary>
    /// Use is a table index in a table access expression or list assignment
    /// </summary>
    TableIndex,
    
    /// <summary>
    /// Use is the left side of a binary op expression or the single use of a unary op expression
    /// </summary>
    ExpressionLeft,
    
    /// <summary>
    /// Use is the right side of a binary op expression
    /// </summary>
    ExpressionRight,
    
    /// <summary>
    /// Use is closure that is called in a function call
    /// </summary>
    Closure,
    
    /// <summary>
    /// Use is an argument in a function call or return instruction
    /// </summary>
    Argument,
}

/// <summary>
/// Root interface for all nodes in the intermediate representation. Typically this will be either an instruction or
/// "statement", which is an operation in a basic block, or an expression, which is a computation that yields a result.
/// </summary>
public interface IIrNode
{
    /// <summary>
    /// A defined register is a register that is "defined" or has a value written to within this IR node. This adds all
    /// the defined registers to the "defines" set and returns the set.
    /// </summary>
    /// <param name="defines">The set to add defined registers to</param>
    /// <returns>The defines set after the defined registers are added</returns>
    public HashSet<Identifier> GetDefinedRegisters(HashSet<Identifier> defines);

    /// <summary>
    /// A used register is a register that is "used" or has its value read at least once within this IR node. This adds
    /// all the used registers to the "uses" set and returns the set.
    /// </summary>
    /// <param name="uses">The set to add used registers to</param>
    /// <returns>The uses set after the used registers are added</returns>
    public HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses);

    /// <summary>
    /// Get the range of registers used in this node that are potentially temporary registers. This would be the highest
    /// contiguous range of registers with the gaps being known locals in compiler emit order.
    /// </summary>
    /// <returns></returns>
    public Interval GetTemporaryRegisterRange();

    public void Parenthesize();
    
    public bool MatchAny(Func<IIrNode, bool> condition);

    /// <summary>
    /// Calls "function" on all the uses of this node in depth first traversal order.
    /// </summary>
    public void IterateUses(Action<IIrNode, UseType, IdentifierReference> function);
}