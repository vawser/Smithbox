using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamAndRowSearchEngine : MultiStageSearchEngine<ParamSelection, (ParamMassEditRowSource,
    Param.Row), (ParamBank, Param), Param.Row>
{
    public ParamEditorView CurrentView;

    public ParamAndRowSearchEngine(ParamEditorView curView, MassEdit massEdit)
    {
        CurrentView = curView;

        var pBank = CurrentView.GetPrimaryBank();

        unpacker = selection =>
        {
            var pBank = CurrentView.GetPrimaryBank();

            List<(ParamMassEditRowSource, Param.Row)> list = new();
            list.AddRange(selection.GetSelectedRows().Select((x, i) => (ParamMassEditRowSource.Selection, x)));
            list.AddRange(pBank.ClipboardRows.Select((x, i) => (ParamMassEditRowSource.Clipboard, x)));
            return list;
        };

        filterList.Add("selection", newCmd(new string[0], 
            LOC.Get("PARAM_PARSE_Selection_TT"),
            noArgs(noContext(row => row.Item1 == ParamMassEditRowSource.Selection))));

        filterList.Add("clipboard", newCmd(new string[0], 
            LOC.Get("PARAM_PARSE_Clipboard_TT"),
            noArgs(noContext(row => row.Item1 == ParamMassEditRowSource.Clipboard)),
            () => CurrentView.GetPrimaryBank().ClipboardRows?.Count > 0));

        contextGetterForMultiStage = (state, exampleItem) => (pBank,
            pBank.Params[
                exampleItem.Item1 == ParamMassEditRowSource.Selection
                    ? state.GetActiveParam()
                    : pBank.ClipboardParam]);

        sourceListGetterForMultiStage = row => row.Item2;
        searchEngineForMultiStage = massEdit.RSE;
        resultRetrieverForMultiStage = (row, exampleItem) => (exampleItem.Item1, row);
    }
}