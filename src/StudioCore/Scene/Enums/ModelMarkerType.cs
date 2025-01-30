using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Scene.Enums;

/// <summary>
/// Model marker type for meshes that may not be visible in the editor (c0000, fogwalls, etc)
/// </summary>
public enum ModelMarkerType
{
    Enemy,
    Object,
    Player,
    Other,
    None
}