using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public class MapGroupsList
{
    public Dictionary<string, MapGroups> List { get; set; } = new();
}

public class MapGroups
{
    public string MapID { get; set; }
    public List<MapGroupEntry> Groups { get; set; } = new();
}

public class MapGroupEntry
{
    public string GUID { get; set; }
    public string Name { get; set; }
    public List<string> Objects { get; set; } = new();
}