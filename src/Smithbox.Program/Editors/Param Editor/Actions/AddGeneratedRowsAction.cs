using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Inserts rows that were built for this param (the Weapon Generator's rows) in ID order, as they are. Unlike
/// <see cref="AddParamsAction"/> the rows aren't cloned again, so no throwaway rows are left for the finalizer,
/// and each row keeps its ID: in replace mode a row with that ID is swapped out, otherwise the caller must give
/// only free IDs.
/// </summary>
public class AddGeneratedRowsAction : EditorAction
{
    private readonly ParamEditorScreen Editor;
    private readonly Param Param;
    private readonly List<Param.Row> Rows;
    private readonly bool ReplaceExisting;

    private class Step
    {
        public Param.Row Row;
        public int InsertIndex = -1;
        public Param.Row Replaced;
        public int ReplacedIndex = -1;
    }

    private readonly List<Step> Steps = new();
    private bool HasExecuted = false;

    public AddGeneratedRowsAction(ParamEditorScreen editor, Param param, List<Param.Row> rows, bool replaceExisting)
    {
        Editor = editor;
        Param = param;
        Rows = rows;
        ReplaceExisting = replaceExisting;
    }

    public IReadOnlyList<Param.Row> GetRows() => Rows;

    public override ActionEvent Execute()
    {
        if (!HasExecuted)
        {
            FirstExecute();
            HasExecuted = true;
        }
        else
        {
            // Undo restored the param to what the first Execute saw, so the same steps reproduce it
            foreach (var step in Steps)
            {
                if (step.Replaced != null)
                    Param.RemoveRow(step.Replaced);

                Param.InsertRow(step.InsertIndex, step.Row);
            }
        }

        AfterChange(toNew: true);

        return ActionEvent.NoEvent;
    }

    private void FirstExecute()
    {
        // Rows are inserted one by one in ID order, so each index is found from the rows already in place
        foreach (var row in Rows.OrderBy(r => r.ID))
        {
            var step = new Step { Row = row };

            var existing = Param.Rows.FirstOrDefault(r => r.ID == row.ID);

            if (existing != null)
            {
                if (!ReplaceExisting)
                    continue;

                step.Replaced = existing;
                step.ReplacedIndex = Param.IndexOfRow(existing);
                Param.RemoveRow(existing);
            }

            step.InsertIndex = GetSortedIndex(row.ID);
            Param.InsertRow(step.InsertIndex, row);

            Steps.Add(step);
        }
    }

    /// <summary>
    /// After the last row with an ID at or below this one, so a row goes where the param's ID order puts it
    /// </summary>
    private int GetSortedIndex(int id)
    {
        var index = 0;
        var i = 0;

        foreach (var r in Param.Rows)
        {
            i++;

            if (r.ID <= id)
                index = i;
        }

        return index;
    }

    public override ActionEvent Undo()
    {
        for (var i = Steps.Count - 1; i >= 0; i--)
        {
            var step = Steps[i];

            Param.RemoveRow(step.Row);

            if (step.Replaced != null)
                Param.InsertRow(step.ReplacedIndex, step.Replaced);
        }

        AfterChange(toNew: false);

        return ActionEvent.NoEvent;
    }

    private void AfterChange(bool toNew)
    {
        ParamDirtyTracker.MarkDirty(Param);

        // Views that had a replaced row selected follow its replacement (or go back on undo)
        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var step in Steps.Where(s => s.Replaced != null))
            {
                if (toNew)
                    view.Selection.ReplaceRowInAllSelections(step.Replaced, step.Row);
                else
                    view.Selection.ReplaceRowInAllSelections(step.Row, step.Replaced);
            }
        }

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        Editor.ViewHandler.RefreshTableGroupsFor(Param);
    }
}
