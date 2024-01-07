using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.MsbEditor;

/// <summary>
///     Because fromsoft divided enemy placement into two params in DS2, this class exists
///     to combine them into a "superparam" that maintains the two param rows under the hood
/// </summary>
public class MergedParamRow
{
    private readonly Dictionary<string, Param.Row> RowMap = new();

    private readonly List<Param.Row> Rows = new();

    public MergedParamRow() { }

    public MergedParamRow(MergedParamRow clone)
    {
        foreach (KeyValuePair<string, Param.Row> entry in clone.RowMap)
        {
            var n = new Param.Row(entry.Value);
            RowMap.Add(entry.Key, n);
            Rows.Add(n);
        }
    }

    public int ID
    {
        get => Rows.Count > 0 ? Rows[0].ID : -1;
        set
        {
            foreach (Param.Row r in Rows)
            {
                r.ID = value;
            }
        }
    }

    public string Name
    {
        get => Rows.Count > 0 ? Rows[0].Name : null;
        set
        {
            foreach (Param.Row r in Rows)
            {
                r.Name = value;
            }
        }
    }

    public List<Param.Cell> CellHandles
    {
        get
        {
            var ret = new List<Param.Cell>();
            foreach (Param.Row r in Rows)
            {
                ret.AddRange(r.Cells);
            }

            return ret;
        }
    }

    public Param.Cell? this[string name]
    {
        get
        {
            foreach (Param.Row r in Rows)
            {
                if (r.Columns.FirstOrDefault(cell => cell.Def.InternalName == name) != null)
                {
                    return r[name];
                }
            }

            return null;
        }
    }

    public void AddRow(string key, Param.Row row)
    {
        if (Rows.Count > 0 && row.ID != Rows[0].ID)
        {
            throw new ArgumentException("All IDs must match for param rows to be merged");
        }

        RowMap.Add(key, row);
        Rows.Add(row);
    }

    public Param.Row GetRow(string key)
    {
        if (RowMap.ContainsKey(key))
        {
            return RowMap[key];
        }

        return null;
    }
}
