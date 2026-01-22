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
    public static void ApplyDelete(ParamView curView)
    {
        List<Param.Row> toRemove = new(curView.Selection.GetSelectedRows());

        DeleteParamsAction act = new(curView.Editor, curView.GetPrimaryBank().Params[curView.Selection.GetActiveParam()], toRemove);

        curView.Editor.ActionManager.ExecuteAction(act);

        curView.Editor.ViewHandler.ParamViews.ForEach(view =>
        {
            if (view != null)
            {
                toRemove.ForEach(row => view.Selection.RemoveRowFromAllSelections(row));
            }
        });
    }
    #endregion
}
