using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class DeleteParamsAction : EditorAction
{
    private ParamEditorScreen Editor;
    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();

    // The view the delete was made from, and the param key it was showing. When set, Execute moves that view's
    // selection to the row after the deleted ones, and Undo restores the deleted rows as the selection.
    private readonly ParamEditorView SelectionView;
    private readonly string ParamKey;
    private Param.Row PreviousActiveRow;

    public DeleteParamsAction(ParamEditorScreen editor, Param param, List<Param.Row> rows,
        ParamEditorView selectionView = null, string paramKey = null)
    {
        Editor = editor;
        Param = param;
        Deletables.AddRange(rows);
        SelectionView = selectionView;
        ParamKey = paramKey;
    }

    public override ActionEvent Execute()
    {
        var nextRow = FindRowAfterDeletion();

        if (SelectionView != null)
        {
            PreviousActiveRow = SelectionView.Selection.GetActiveRow();
        }

        // Re-record on every execute (redo included), so Undo always uses this run's indices
        RemoveIndices.Clear();

        foreach (Param.Row row in Deletables)
        {
            RemoveIndices.Add(Param.IndexOfRow(row));
            Param.RemoveRowAt(RemoveIndices.Last());
        }

        ParamDirtyTracker.MarkDirty(Param);

        // Deleted rows can't stay selected in any view (this used to be done by the caller only, so a redo
        // left them selected)
        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var row in Deletables)
            {
                view.Selection.RemoveRowFromAllSelections(row);
            }
        }

        if (nextRow != null && IsSelectionViewOnParam())
        {
            SelectionView.Selection.SetActiveRow(nextRow, true, true);
            SelectionView.JumpToSelectedRow = true;
        }

        RefreshTableGroups();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Deletables.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemoveIndices[i], Deletables[i]);
        }

        ParamDirtyTracker.MarkDirty(Param);

        // Restore the deleted rows as the selection, with the previously active row active again
        if (IsSelectionViewOnParam())
        {
            var selection = SelectionView.Selection;
            var active = PreviousActiveRow != null && Deletables.Contains(PreviousActiveRow)
                ? PreviousActiveRow
                : Deletables.FirstOrDefault();

            selection.SetActiveRow(active, true, true);

            foreach (var row in Deletables)
            {
                selection.AddRowToSelection(row);
            }

            SelectionView.JumpToSelectedRow = true;
        }

        RefreshTableGroups();

        return ActionEvent.NoEvent;
    }

    /// <summary>
    /// The first surviving row after the last deleted row, or failing that the last surviving row before the
    /// first deleted row. Rows are in param order, so this is the row that ends up in the deleted rows' place.
    /// </summary>
    private Param.Row FindRowAfterDeletion()
    {
        if (SelectionView == null || Deletables.Count == 0)
            return null;

        var deleted = new HashSet<Param.Row>(Deletables);
        var indices = Deletables.Select(r => Param.IndexOfRow(r)).Where(i => i >= 0).ToList();

        if (indices.Count == 0)
            return null;

        var rows = Param.Rows.ToList();

        for (var i = indices.Max() + 1; i < rows.Count; i++)
        {
            if (!deleted.Contains(rows[i]))
                return rows[i];
        }

        for (var i = indices.Min() - 1; i >= 0; i--)
        {
            if (!deleted.Contains(rows[i]))
                return rows[i];
        }

        return null;
    }

    /// <summary>
    /// The view still exists and is still showing this param, so changing its selection makes sense
    /// </summary>
    private bool IsSelectionViewOnParam()
    {
        return SelectionView != null
            && ParamKey != null
            && Editor.ViewHandler.ParamEditorViews.Contains(SelectionView)
            && SelectionView.Selection.GetActiveParam() == ParamKey;
    }

    private void RefreshTableGroups()
    {
        var curParam = Editor.ViewHandler.ActiveView.Selection.GetActiveParam();

        if (Editor.ViewHandler.ActiveView.ParamTableWindow.IsInTableGroupMode(curParam))
        {
            var curGroup = Editor.ViewHandler.ActiveView.ParamTableWindow.CurrentTableGroupID;
            Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
            Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableSelection();
        }
    }
}
