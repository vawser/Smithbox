using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class LegacyProjectJSON
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public Dictionary<string, List<int>> PinnedRows { get; set; }
    public Dictionary<string, List<string>> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectJSON() { }

    public LegacyProjectJSON(ProjectEntry curProject)
    {
        ProjectName = curProject.ProjectName;
        GameRoot = curProject.DataPath;
        GameType = curProject.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.UseLooseParams;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}
