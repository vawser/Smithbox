using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Application;

public class QuickScript
{
    public static void ApplyQuickScript(Smithbox baseEditor, ProjectEntry curProject)
    {
        var file = curProject.FS.ReadFile("/expression/menu.expb.dcx");
        if(file != null)
        {
            var fmc = FMB.Read((Memory<byte>)file);

            TaskLogs.AddLog("");
        }
    }
}
