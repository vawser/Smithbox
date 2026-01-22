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
    public ParamView CurrentView;

    public ParamAndRowSearchEngine(ParamView curView, MassEdit massEdit)
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

        filterList.Add("selection",
            newCmd(new string[0], "Selects the current param selection and selected rows in that param",
                noArgs(noContext(row => row.Item1 == ParamMassEditRowSource.Selection))));

        filterList.Add("clipboard",
            newCmd(new string[0], "Selects the param of the clipboard and the rows in the clipboard",
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