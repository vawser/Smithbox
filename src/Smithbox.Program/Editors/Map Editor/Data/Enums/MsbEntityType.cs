using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

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
    DS2ObjectInstance,
    AutoInvadePoint,
    LightAtlas,
    LightProbeVolume,
    Navmesh
}