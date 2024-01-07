using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.MsbEditor;

/// <summary>
///     Navigation context for a map. Stores all the meta navigation information for a DS1
///     map (i.e. mcg and mcp stuff).
/// </summary>
public class MapNavigationDS1
{
    public MapNavigationDS1(ObjectContainer map, MCP mcp, MCG mcg)
    {
        Map = map;

        foreach (MCP.Room r in mcp.Rooms)
        {
            Regions.Add(new NavRegion(Map, r));
        }
    }

    public ObjectContainer Map { get; }

    public List<NavRegion> Regions { get; } = null;
}
