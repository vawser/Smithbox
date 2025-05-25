using System;

namespace StudioCore.Scene.Enums;

[Flags]
public enum RenderFilter
{
    Debug = 1,
    Editor = 2,
    MapPiece = 4,
    Collision = 8,
    Character = 16,
    Object = 32,
    Navmesh = 64,
    Region = 128,
    Light = 256,
    SpeedTree = 512,
    All = 0xFFFFFFF
}
