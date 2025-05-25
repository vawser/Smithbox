using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Formats.JSON;

// -------------- Entity Selection Group --------------
public class EntitySelectionGroupList
{
    public List<EntitySelectionGroupResource> Resources { get; set; }
}

public class EntitySelectionGroupResource
{
    public string Name { get; set; }
    public int SelectionGroupKeybind { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Selection { get; set; }
}

// -------------- Spawn States --------------
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
