using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class SpawnStateResource
{
    public List<SpawnStateEntry> list { get; set; }
}

public class SpawnStateEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<SpawnStatePair> states { get; set; }
}

public class SpawnStatePair
{
    public string value { get; set; }
    public string name { get; set; }
}