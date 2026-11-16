using Andre.Formats;

using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MERowOperation : MEOperation<(string, Param.Row), (Param, Param.Row)>
{
    public ParamEditorView CurrentView;

    // Whether the current command's copy has already cleared the clipboard. A copy runs once per matched row, so the
    // clipboard is cleared by the first row only. Reset by BeginCommand before each mass-edit command.
    private bool _clipboardStartedForCommand;

    /// <summary>
    /// Called before each mass-edit command, so a copy command starts a new clipboard instead of adding to it
    /// </summary>
    public void BeginCommand()
    {
        _clipboardStartedForCommand = false;
    }

    public MERowOperation(ParamEditorView curView)
    {
        CurrentView = curView;
        operations.Clear();
        Setup();
    }

    internal override void Setup()
    {
        operations.Add("copy", (new string[0],
            LOC.Get("PARAM_RowOp_Copy_TT"),
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_Copy_Error_Locate_Param"));
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_Copy_Error_Locate_Param_Key", paramKey));
                }

                Param p = bank.Params[paramKey];
                // Only supporting single param in clipboard. Each copy command starts a new clipboard, as Ctrl+C does
                // (it used to add to the rows already there when the param was the same).
                if (!_clipboardStartedForCommand || bank.ClipboardParam != paramKey)
                {
                    bank.ClipboardParam = paramKey;
                    bank.ClipboardRows.Clear();
                    _clipboardStartedForCommand = true;
                }

                bank.ClipboardRows.Add(new Param.Row(row, p));
                return (p, null);
            }
        ));

        operations.Add("copyN", (new[] { 
            LOC.Get("PARAM_RowOp_CopyN_Hint_1")},
            LOC.Get("PARAM_RowOp_CopyN_TT"),
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_CopyN_Error_Locate_Param"));
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_CopyN_Error_Locate_Param_Key", paramKey));
                }

                var count = uint.Parse(args[0]);
                Param p = bank.Params[paramKey];
                // Only supporting single param in clipboard. Each copy command starts a new clipboard, as Ctrl+C does
                // (it used to add to the rows already there when the param was the same).
                if (!_clipboardStartedForCommand || bank.ClipboardParam != paramKey)
                {
                    bank.ClipboardParam = paramKey;
                    bank.ClipboardRows.Clear();
                    _clipboardStartedForCommand = true;
                }

                for (var i = 0; i < count; i++)
                {
                    bank.ClipboardRows.Add(new Param.Row(row, p));
                }

                return (p, null);
            }
        ));

        operations.Add("paste", (new string[0],
            LOC.Get("PARAM_RowOp_Paste_TT"),
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_Paste_Error_Locate_Param"));
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception(
                        LOC.Get("PARAM_RowOp_Paste_Error_Locate_Param_Key", paramKey));
                }

                Param p = bank.Params[paramKey];
                return (p, new Param.Row(row, p));
            }
        ));
    }
}

