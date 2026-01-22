using Andre.Formats;
using HKX2;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MERowOperation : MEOperation<(string, Param.Row), (Param, Param.Row)>
{
    public ParamView CurrentView;

    public MERowOperation(ParamView curView)
    {
        CurrentView = curView;
        operations.Clear();
        Setup();
    }

    internal override void Setup()
    {
        operations.Add("copy", (new string[0],
            "Adds the selected rows into clipboard. If the clipboard param is different, the clipboard is emptied first",
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(@"Could not locate param");
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception($@"Could not locate param {paramKey}");
                }

                Param p = bank.Params[paramKey];
                // Only supporting single param in clipboard
                if (bank.ClipboardParam != paramKey)
                {
                    bank.ClipboardParam = paramKey;
                    bank.ClipboardRows.Clear();
                }

                bank.ClipboardRows.Add(new Param.Row(row, p));
                return (p, null);
            }
        ));
        operations.Add("copyN", (new[] { "count" },
            "Adds the selected rows into clipboard the given number of times. If the clipboard param is different, the clipboard is emptied first",
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(@"Could not locate param");
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception($@"Could not locate param {paramKey}");
                }

                var count = uint.Parse(args[0]);
                Param p = bank.Params[paramKey];
                // Only supporting single param in clipboard
                if (bank.ClipboardParam != paramKey)
                {
                    bank.ClipboardParam = paramKey;
                    bank.ClipboardRows.Clear();
                }

                for (var i = 0; i < count; i++)
                {
                    bank.ClipboardRows.Add(new Param.Row(row, p));
                }

                return (p, null);
            }
        ));
        operations.Add("paste", (new string[0],
            "Adds the selected rows to the primary regulation or parambnd in the selected param",
            (paramAndRow, args) =>
            {
                var bank = CurrentView.GetPrimaryBank();

                var paramKey = paramAndRow.Item1;
                Param.Row row = paramAndRow.Item2;
                if (paramKey == null)
                {
                    throw new Exception(@"Could not locate param");
                }

                if (!bank.Params.ContainsKey(paramKey))
                {
                    throw new Exception($@"Could not locate param {paramKey}");
                }

                Param p = bank.Params[paramKey];
                return (p, new Param.Row(row, p));
            }
        ));
    }
}

