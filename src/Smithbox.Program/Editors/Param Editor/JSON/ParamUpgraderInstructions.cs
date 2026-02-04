using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.ParamEditor;

public class ParamUpgraderInfo
{
    public string MaxVersion { get; set; }

    public List<OldRegulationEntry> RegulationEntries { get; set; }

    public List<UpgraderMassEditEntry> UpgradeCommands { get; set; }
}

public class OldRegulationEntry
{
    public string Version { get; set; }
    public string Folder { get; set; }
}

public class UpgraderMassEditEntry
{
    public string Version { get; set; }
    public string Message { get; set; }
    public string Command { get; set; }
}