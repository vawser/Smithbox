using ImGuiNET;
using SoulsFormats;
using StudioCore.ParamEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Tests;

public static class ParamUniqueRowFinder
{
    public static void Run()
    {
        var baseID = ParamBank.PrimaryBank.Params.Values.Max(p => p.Rows.Max(r => r.ID)) + 1;
        var i = baseID;
        foreach (var p in ParamBank.PrimaryBank.Params.Values)
        {
            Andre.Formats.Param.Row row = new(p.Rows.First());
            row.ID = i;
            i++;
            p.AddRow(row);
        }
        TaskLogs.AddLog($"Added rows to all params with IDs {baseID}-{i - 1} ",
            Microsoft.Extensions.Logging.LogLevel.Debug, TaskLogs.LogPriority.High);
    }
}
