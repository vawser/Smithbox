using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource;

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