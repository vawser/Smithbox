using Hexa.NET.ImGui;
using StudioCore.Logger;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Application;

public static class ParamUniqueInserter
{
    public static void Display(ProjectEntry project)
    {
        if (ImGui.Button("Run Test", DPI.StandardButtonSize))
        {
            Run(project);
        }
    }

    public static void Run(ProjectEntry project)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var baseID = curProject.Handler.ParamData.PrimaryBank.Params.Values.Max(p => p.Rows.Max(r => r.ID)) + 1;
        var i = baseID;
        foreach (var p in curProject.Handler.ParamData.PrimaryBank.Params.Values)
        {
            Andre.Formats.Param.Row row = new(p.Rows.First());
            row.ID = i;
            i++;
            p.AddRow(row);
        }
        Smithbox.Log(typeof(ParamUniqueInserter), $"Added rows to all params with IDs {baseID}-{i - 1} ",
            Microsoft.Extensions.Logging.LogLevel.Debug, LogPriority.High);
    }
}
