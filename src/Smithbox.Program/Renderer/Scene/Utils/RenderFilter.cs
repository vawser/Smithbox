using System;

namespace StudioCore.Renderer;

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
    AutoInvade = 1024,

    Meshes = 2048,
    Dummies = 4096,
    Nodes = 8192,

    LightProbe = 16384,

    All = 0xFFFFFFF
}
