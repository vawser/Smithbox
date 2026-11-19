using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore.IR;

/// <summary>
/// Bulk setting of array values to a list. Generated from Lua SETLIST op as an intermediate for list initializers
/// </summary>
public class ListRangeAssignment : Instruction
{
    /// <summary>
    /// The table to assign to
    /// </summary>
    public readonly IdentifierReference Table;

    /// <summary>
    /// The range of indices to assign in the table
    /// </summary>
    public readonly Interval Indices;

    /// <summary>
    /// The values to assign for all the indices
    /// </summary>
    public readonly List<Expression> Values;

    public ListRangeAssignment(IdentifierReference table, Interval indices, List<Expression> values)
    {
        Debug.Assert(indices.Count == values.Count);
        Table = table;
        Indices = indices;
        Values = values;
    }
    
    public override HashSet<Identifier> GetUsedRegisters(HashSet<Identifier> uses)
    {
        Table.GetUsedRegisters(uses);
        foreach (var value in Values)
        {
            value.GetUsedRegisters(uses);
        }
        return uses;
    }
    
    public override Interval GetTemporaryRegisterRange()
    {
        var temporaries = new Interval();
        temporaries.AddToTemporaryRegisterRange(Table.GetOriginalUseRegisters());
        foreach (var v in Values)
        {
            temporaries.AddToTemporaryRegisterRange(v.GetOriginalUseRegisters());
        }
        
        temporaries.MergeTemporaryRegisterRange(Table.GetTemporaryRegisterRange());
        foreach (var v in Values)
        {
            temporaries.MergeTemporaryRegisterRange(v.GetTemporaryRegisterRange());
        }

        return temporaries;
    }

    public override void RenameUses(Identifier original, Identifier newIdentifier)
    {
        Table.RenameUses(original, newIdentifier);
        foreach (var value in Values)
        {
            value.RenameUses(original, newIdentifier);
        }
    }

    public override bool ReplaceUses(Identifier orig, Expression sub)
    {
        var replaced = false;
        for (var i = 0; i < Values.Count; i++)
        {
            if (Expression.ShouldReplace(orig, Values[i], sub))
            {
                replaced = true;
                Values[i] = sub;
            }
            else
            {
                replaced |= Values[i].ReplaceUses(orig, sub);
            }
        }
        return replaced;
    }

    public override int UseCount(Identifier use)
    {
        return Table.UseCount(use) + Values.Sum(e => e.UseCount(use));
    }

    public override bool MatchAny(Func<IIrNode, bool> condition)
    {
        var result = condition.Invoke(this);
        result |= Table.MatchAny(condition);
        foreach (var value in Values)
        {
            result |= value.MatchAny(condition);
        }
        return result;
    }

    public override void IterateUses(Action<IIrNode, UseType, IdentifierReference> function)
    {
        IterateUsesSuccessor(Table, UseType.Table, function);
        foreach (var value in Values)
        {
            IterateUsesSuccessor(value, UseType.Assignee, function);
        }
    }

    public override List<Expression> GetExpressions()
    {
        var ret = new List<Expression>();
        ret.AddRange(Table.GetExpressions());
        foreach (var value in Values)
        {
            ret.AddRange(value.GetExpressions());
        }
        return ret;
    }
}