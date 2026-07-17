using HKX2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MEGlobalOperation : MEOperation<ParamSelection, bool>
{
    public ParamEditorView CurrentView;

    public MEGlobalOperation(ParamEditorView curView)
    {
        CurrentView = curView;
        operations.Clear();
        Setup();
    }

    internal override void Setup()
    {
        operations.Add("clear", (new string[0],
            LOC.Get("PARAM_GlobalOp_Clear_TT"), (selectionState, args) =>
        {
            CurrentView.GetPrimaryBank().ClipboardParam = null;
            CurrentView.GetPrimaryBank().ClipboardRows.Clear();
            return true;
        }
        ));

        operations.Add("newvar", (new[] { 
            LOC.Get("PARAM_GlobalOp_NewVar_Hint_1"),
            LOC.Get("PARAM_GlobalOp_NewVar_Hint_2") },
            LOC.Get("PARAM_GlobalOp_NewVar_TT"), (selectionState, args) =>
            {
                int asInt;
                double asDouble;
                if (int.TryParse(args[1], out asInt))
                {
                    MassParamEdit.massEditVars[args[0]] = asInt;
                }
                else if (double.TryParse(args[1], out asDouble))
                {
                    MassParamEdit.massEditVars[args[0]] = asDouble;
                }
                else
                {
                    MassParamEdit.massEditVars[args[0]] = args[1];
                }

                return true;
            }
        ));

        operations.Add("clearvars", (new string[0], 
            LOC.Get("PARAM_GlobalOp_ClearVars_Hint"), (selectionState, args) =>
        {
            MassParamEdit.massEditVars.Clear();
            return true;
        }
        ));
    }
}
