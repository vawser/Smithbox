using System;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Renderer;

[Flags]
public enum RenderFilter
{
    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Debug")]
    Debug = 1,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Editor")]
    Editor = 2,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_MapPiece")]
    MapPiece = 4,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Collision")]
    Collision = 8,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Character")]
    Character = 16,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Object")]
    Object = 32,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Navmesh")]
    Navmesh = 64,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Region")]
    Region = 128,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Light")]
    Light = 256,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_SpeedTree")]
    SpeedTree = 512,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_AutoInvade")]
    AutoInvade = 1024,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Meshes")]
    Meshes = 2048,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Dummies")]
    Dummies = 4096,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Nodes")]
    Nodes = 8192,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_LightProbe")]
    LightProbe = 16384,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_ConnectCollision")]
    ConnectCollision = 32768,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_ObjectCollision")]
    ObjectCollision = 65536,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Grass")]
    Grass = 131072,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_EdgePoint")]
    EdgePoint = 262144,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_DummyEnemy")]
    DummyEnemy = 524288,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_DummyAsset")]
    DummyAsset = 1048576,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_Wall")]
    Wall = 2097152,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_MultiPlayerWall")]
    MultiPlayerWall = 4194304,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_NavVolume")]
    NavVolume = 8388608,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_MapLimit")]
    MapLimit = 16777216,

    [Display(Name = "VIEWPORT_ENUM_RenderFilter_All")]
    All = 0xFFFFFFF
}
