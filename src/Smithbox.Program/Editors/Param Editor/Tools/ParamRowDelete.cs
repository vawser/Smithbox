using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamRowDelete
{
    #region Delete
    public static void ApplyDelete(ParamEditorView curView)
    {
        var activeParam = curView.Selection.GetActiveParam();

        if (activeParam == null)
            return;

        if (!curView.GetPrimaryBank().Params.TryGetValue(activeParam, out var param))
            return;

        // Only rows that are actually in this param: a stale or null entry would make
        // DeleteParamsAction call RemoveRowAt(-1)
        List<Param.Row> toRemove = curView.Selection.GetSelectedRows()
            .Where(row => row != null && row.GetParentParam() == param && param.IndexOfRow(row) >= 0)
            .ToList();

        if (toRemove.Count == 0)
            return;

        // The action clears the deleted rows from every view's selection and selects the next row in this view
        // (and restores the selection on undo)
        DeleteParamsAction act = new(curView.Editor, param, toRemove, curView, activeParam);

        curView.Editor.ActionManager.ExecuteAction(act);
    }
    #endregion
}
