using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Generation;

public static class FmgRefPrintTool
{
    public static void Print()
    {
        var outputText = "";

        foreach (var entry in ParamBank.PrimaryBank.Params)
        {
            var paramName = entry.Key;
            var entries = TextParamUtils.GetFmgEntriesByAssociatedParam(paramName);

            outputText = $"{outputText}\n\n**********************";
            outputText = $"{outputText}\n{paramName}";
            outputText = $"{outputText}\n**********************";

            if (entries.Count != 0)
            {
                foreach (var fmgEntry in entries)
                {
                    if (fmgEntry.Text != null && fmgEntry.Text != "" && fmgEntry.Text.Length > 1)
                    {
                        outputText = $"{outputText}\n{fmgEntry.ID} {fmgEntry.Text}";
                    }
                }
            }
        }

        PlatformUtils.Instance.SetClipboardText(outputText);
    }
}
