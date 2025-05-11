using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.DebugNS;

public static class ParamUniqueInserter
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.Button("Run Test"))
        {
            Run(baseEditor, project);
        }
    }

    public static void Run(Smithbox baseEditor, ProjectEntry project)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        var baseID = curProject.ParamData.PrimaryBank.Params.Values.Max(p => p.Rows.Max(r => r.ID)) + 1;
        var i = baseID;
        foreach (var p in curProject.ParamData.PrimaryBank.Params.Values)
        {
            Andre.Formats.Param.Row row = new(p.Rows.First());
            row.ID = i;
            i++;
            p.AddRow(row);
        }
        TaskLogs.AddLog($"Added rows to all params with IDs {baseID}-{i - 1} ",
            Microsoft.Extensions.Logging.LogLevel.Debug, LogPriority.High);
    }
}
