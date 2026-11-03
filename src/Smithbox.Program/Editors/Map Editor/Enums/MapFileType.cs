using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The files that make up a map, used to track which of them have unsaved changes
/// </summary>
[Flags]
public enum MapFileType
{
    None = 0,

    /// <summary>
    /// The MSB, including the DS2 generator params that are saved alongside it
    /// </summary>
    MSB = 1 << 0,
    BTL = 1 << 1,
    AIP = 1 << 2,
    BTAB = 1 << 3,
    BTPB = 1 << 4,
    ACB = 1 << 5,
    NVA = 1 << 6,
    Collision = 1 << 7,
    Navmesh = 1 << 8,

    /// <summary>
    /// The WALL files of the collisions (SDT)
    /// </summary>
    Wall = 1 << 9,

    /// <summary>
    /// The MPW file of an open world tile (ER)
    /// </summary>
    MPW = 1 << 10,

    /// <summary>
    /// The NVVA file of a map, the nav volume for flying movement (AC6)
    /// </summary>
    NVVA = 1 << 11
}
