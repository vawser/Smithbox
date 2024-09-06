using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Development;

public static class FmgMetaTool
{
    public static void GetNames()
    {
        string Names = "";

        var entries = Smithbox.BankHandler.FMGBank.FmgInfoBank.ToList();

        foreach (var info in entries)
        {
            Names = Names + $"{info.Name} - {info.FmgID}\n";
        }

        PlatformUtils.Instance.SetClipboardText(Names);
    }
}
