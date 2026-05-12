using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

using Andre.Formats;

public class ReorderRowAction : EditorAction
{
    private readonly Param TargetParam;

    private readonly List<Param.Row> RowsToMove;

    private readonly int DropTargetIndex;

    private List<int> OriginalIndices = new();

    private int ResolvedInsertionIndex = -1;

    public ReorderRowAction(Param param, List<Param.Row> rowsToMove, int dropTargetIndex)
    {
        TargetParam = param;
        RowsToMove = new List<Param.Row>(rowsToMove);
        DropTargetIndex = dropTargetIndex;
    }

    public override ActionEvent Execute()
    {
        OriginalIndices = RowsToMove
            .Select(r => TargetParam.IndexOfRow(r))
            .ToList();

        if (ResolvedInsertionIndex == -1)
        {
            int shiftCount = OriginalIndices.Count(idx => idx < DropTargetIndex);
            ResolvedInsertionIndex = DropTargetIndex - shiftCount;
        }

        foreach (int idx in OriginalIndices.OrderByDescending(i => i))
        {
            TargetParam.RemoveRowAt(idx);
        }

        int insertAt = ResolvedInsertionIndex;
        foreach (Param.Row row in RowsToMove)
        {
            TargetParam.InsertRow(insertAt, row);
            insertAt++;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (Param.Row row in RowsToMove)
        {
            TargetParam.RemoveRow(row);
        }

        var pairs = RowsToMove
            .Zip(OriginalIndices, (row, idx) => (row, idx))
            .OrderBy(p => p.idx);

        foreach (var (row, idx) in pairs)
        {
            TargetParam.InsertRow(idx, row);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}