using System.Collections.Generic;

namespace StudioCore.Application;

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


// -------------- Map Object Names --------------
public class MapObjectNameMapEntry
{
    public string Name { get; set; }
    public List<MapObjectNameEntry> Entries { get; set; }
}

public class MapObjectNameEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
}