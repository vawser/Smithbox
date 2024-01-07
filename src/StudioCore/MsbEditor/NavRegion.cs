using SoulsFormats;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using Veldrid.Utilities;

namespace StudioCore.MsbEditor;

/// <summary>
///     A (DS1) navigation region which marks a room/area that a navmesh
///     is active in. Corresponds with an MCP room
/// </summary>
public class NavRegion : ISelectable
{
    /// <summary>
    ///     Indices of neighbors before resolution
    /// </summary>
    private readonly List<int> _nindices = new();

    /// <summary>
    ///     Index of the map navmesh before resolution
    /// </summary>
    private int _navidx = -1;

    /// <summary>
    ///     Construct a region from a deserialized mcp room
    /// </summary>
    public NavRegion(ObjectContainer enclosingMap, MCP.Room room)
    {
        BoundingBox = new BoundingBox(room.BoundingBoxMin, room.BoundingBoxMax);
        _navidx = room.LocalIndex;
        _nindices.AddRange(room.ConnectedRoomIndices);
    }

    public IDrawable RenderMesh { get; set; } = null;

    /// <summary>
    ///     Navmesh this region is associated with
    /// </summary>
    public Entity Navmesh { get; set; }

    /// <summary>
    ///     Bounding volume of this region
    /// </summary>
    public BoundingBox BoundingBox { get; set; }

    /// <summary>
    ///     The regions that neighbor and are connected to this region
    /// </summary>
    public List<NavRegion> Neighbors { get; }

    public void OnDeselected()
    {
        throw new NotImplementedException();
    }

    public void OnSelected()
    {
        throw new NotImplementedException();
    }
}
