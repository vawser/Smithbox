using Andre.Formats;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassParamEditOther
{
    public static AddParamsAction SortRows(ParamEditorView curView, ParamBank bank, string paramName)
    {
        Param param = bank.Params[paramName];
        List<Param.Row> newRows = new(param.Rows);
        newRows.Sort((a, b) => { return a.ID - b.ID; });

        return new AddParamsAction(curView.Editor, param, paramName, newRows, true,
            true); //appending same params and allowing overwrite
    }
}
