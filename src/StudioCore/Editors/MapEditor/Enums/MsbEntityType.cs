using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Enums;

/// <summary>
/// Enum for Entity Type within the MSB.
/// </summary>
public enum MsbEntityType
{
    MapRoot,
    Editor,
    Part,
    Region,
    Event,
    Models,
    Routes,
    Layers,
    Light,
    DS2Generator,
    DS2GeneratorRegist,
    DS2Event,
    DS2EventLocation,
    DS2ObjectInstance
}