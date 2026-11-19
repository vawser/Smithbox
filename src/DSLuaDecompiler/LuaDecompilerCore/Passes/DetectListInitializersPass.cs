using System;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Detects list initializers as a series of statements that serially add data to a newly initialized list
/// </summary>
public class DetectListInitializersPass : IPass
{
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f)
    {
        bool irChanged = false;
        foreach (var b in f.BlockList)
        {
            for (var i = 0; i < b.Instructions.Count; i++)
            {
                var changed = false;
                if (b.Instructions[i] is Assignment 
                    {
                        IsSingleAssignment: true, 
                        Left: IdentifierReference tableIr,
                        Right: InitializerList il 
                    } a)
                {
                    // Eat up any statements that follow that match the initializer list pattern
                    //var initIndex = 1;
                    while (i + 1 < b.Instructions.Count)
                    {
                        if (b.Instructions[i + 1] is Assignment
                            {
                                IsSingleAssignment: true,
                                Left: TableAccess
                                {
                                    Table: IdentifierReference ir,
                                    TableIndex: { } e
                                }
                            } a2 && 
                            //Math.Abs(c.Number - initIndex) < 0.0001 && 
                            ir.Identifier == tableIr.Identifier)
                        {
                            if (a2.Right == null)
                                throw new Exception("Expected assignment");
                            il.AddTableElement(e, a2.Right);
                            if (a2.LocalAssignments != null)
                            {
                                a.LocalAssignments = a2.LocalAssignments;
                            }
                            a.Absorb(a2);
                            b.Instructions.RemoveAt(i + 1);
                            //initIndex++;
                            changed = true;
                            irChanged = true;
                        }
                        else if (b.Instructions[i + 1] is ListRangeAssignment a3 && 
                                 a3.Table.Identifier == tableIr.Identifier)
                        {
                            il.AddListRange(a3.Indices, a3.Values);
                            a.Absorb(a3);
                            b.Instructions.RemoveAt(i + 1);
                            changed = true;
                            irChanged = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (changed)
                    {
                        i = -1;
                    }
                }
            }
        }

        return irChanged;
    }
}