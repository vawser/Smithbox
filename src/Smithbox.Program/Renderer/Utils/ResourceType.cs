using System;

namespace StudioCore.Renderer;

[Flags]
public enum ResourceType
{
    Flver = 1,
    Texture = 2,
    CollisionHKX = 4,
    Navmesh = 8,
    NavmeshHKX = 16,
    All = 0xFFFFFFF
}