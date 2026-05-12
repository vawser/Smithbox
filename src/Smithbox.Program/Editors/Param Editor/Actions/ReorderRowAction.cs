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

    // Full Replace
    private readonly bool IsFullReplace;
    private readonly List<Param.Row> DesiredOrder;
    private List<Param.Row> PreviousOrder = new();

    // Drag to Order
    public ReorderRowAction(Param param, List<Param.Row> rowsToMove, int dropTargetIndex)
    {
        TargetParam = param;
        RowsToMove = new List<Param.Row>(rowsToMove);
        DropTargetIndex = dropTargetIndex;
    }

    // Full Replace
    public ReorderRowAction(Param param, List<Param.Row> orderedRows, bool fullReplace)
    {
        TargetParam = param;
        IsFullReplace = true;
        DesiredOrder = new List<Param.Row>(orderedRows);
        RowsToMove = new List<Param.Row>();
        DropTargetIndex = -1;
    }

    public override ActionEvent Execute()
    {
        return IsFullReplace ? ExecuteFullReplace() : ExecuteDrag();
    }

    private ActionEvent ExecuteDrag()
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
            TargetParam.RemoveRowAt(idx);

        int insertAt = ResolvedInsertionIndex;
        foreach (Param.Row row in RowsToMove)
        {
            TargetParam.InsertRow(insertAt, row);
            insertAt++;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private ActionEvent ExecuteFullReplace()
    {
        // Snapshot the current order for Undo.
        PreviousOrder = new List<Param.Row>(TargetParam.Rows);

        ApplyOrder(DesiredOrder);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        return IsFullReplace ? UndoFullReplace() : UndoDrag();
    }

    private ActionEvent UndoFullReplace()
    {
        ApplyOrder(PreviousOrder);
        return ActionEvent.ObjectAddedRemoved;
    }

    private ActionEvent UndoDrag()
    {
        foreach (Param.Row row in RowsToMove)
            TargetParam.RemoveRow(row);

        var pairs = RowsToMove
            .Zip(OriginalIndices, (row, idx) => (row, idx))
            .OrderBy(p => p.idx);

        foreach (var (row, idx) in pairs)
            TargetParam.InsertRow(idx, row);

        return ActionEvent.ObjectAddedRemoved;
    }
    private void ApplyOrder(List<Param.Row> order)
    {
        // Remove from back to front to keep indices valid.
        for (int i = TargetParam.Rows.Count - 1; i >= 0; i--)
            TargetParam.RemoveRowAt(i);

        for (int i = 0; i < order.Count; i++)
            TargetParam.InsertRow(i, order[i]);
    }
}