using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class SpawnStates
{
    public List<SpawnStateEntry> List { get; set; } = new();
}

public class SpawnStateEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<SpawnStateContents> states { get; set; }
}

public class SpawnStateContents
{
    public string value { get; set; }
    public string name { get; set; }
}