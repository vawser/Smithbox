using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public static class MsbCategories
{
    // Defines the base categories for each project type
    public static Dictionary<ProjectType, Dictionary<string, Type>> BaseCategories { get; set; } = new()
    {
        [ProjectType.DES] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSBD.ModelParam),
            ["Event"] = typeof(MSBD.EventParam),
            ["Region"] = typeof(MSBD.PointParam),
            ["Part"] = typeof(MSBD.PartsParam)
        },
        [ProjectType.DS1] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB1.ModelParam),
            ["Event"] = typeof(MSB1.EventParam),
            ["Region"] = typeof(MSB1.PointParam),
            ["Part"] = typeof(MSB1.PartsParam)
        },
        [ProjectType.DS1R] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB1.ModelParam),
            ["Event"] = typeof(MSB1.EventParam),
            ["Region"] = typeof(MSB1.PointParam),
            ["Part"] = typeof(MSB1.PartsParam)
        },
        [ProjectType.DS2] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB2.ModelParam),
            ["Event"] = typeof(MSB2.EventParam),
            ["Region"] = typeof(MSB2.PointParam),
            ["Part"] = typeof(MSB2.PartsParam),
            ["PartPose"] = typeof(MSB2.PartPose) // Direct category
        },
        [ProjectType.DS2S] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB2.ModelParam),
            ["Event"] = typeof(MSB2.EventParam),
            ["Region"] = typeof(MSB2.PointParam),
            ["Part"] = typeof(MSB2.PartsParam),
            ["PartPose"] = typeof(MSB2.PartPose) // Direct category
        },
        [ProjectType.BB] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSBB.ModelParam),
            ["Event"] = typeof(MSBB.EventParam),
            ["Region"] = typeof(MSBB.PointParam),
            ["Part"] = typeof(MSBB.PartsParam)
        },
        [ProjectType.DS3] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB3.ModelParam),
            ["Event"] = typeof(MSB3.EventParam),
            ["Region"] = typeof(MSB3.PointParam),
            ["Part"] = typeof(MSB3.PartsParam),
            ["Route"] = typeof(MSB3.Route), // Direct category
            ["Layer"] = typeof(MSB3.Layer), // Direct category
            ["PartsPose"] = typeof(MSB3.PartsPose) // Direct category
        },
        [ProjectType.SDT] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSBS.ModelParam),
            ["Event"] = typeof(MSBS.EventParam),
            ["Region"] = typeof(MSBS.PointParam),
            ["Part"] = typeof(MSBS.PartsParam),
            ["Route"] = typeof(MSBS.Route), // Direct category
        },
        [ProjectType.ER] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSBE.ModelParam),
            ["Event"] = typeof(MSBE.EventParam),
            ["Region"] = typeof(MSBE.PointParam),
            ["Part"] = typeof(MSBE.PartsParam),
            ["Route"] = typeof(MSBE.RouteParam), 
            ["Layer"] = typeof(MSBE.EmptyParam) // Direct category
        },
        [ProjectType.AC6] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB_AC6.ModelParam),
            ["Event"] = typeof(MSB_AC6.EventParam),
            ["Region"] = typeof(MSB_AC6.PointParam),
            ["Part"] = typeof(MSB_AC6.PartsParam),
            ["Route"] = typeof(MSB_AC6.RouteParam), // Direct category
            ["Layer"] = typeof(MSB_AC6.LayerParam) // Direct category
        },
        [ProjectType.NR] = new Dictionary<string, Type>()
        {
            ["Model"] = typeof(MSB_NR.ModelParam),
            ["Event"] = typeof(MSB_NR.EventParam),
            ["Region"] = typeof(MSB_NR.PointParam),
            ["Part"] = typeof(MSB_NR.PartsParam),
            ["Route"] = typeof(MSB_NR.RouteParam),
            ["Layer"] = typeof(MSB_NR.EmptyParam) // Direct category
        },
    };

    // Defines the sub-categories for each base category for each project type
    public static Dictionary<ProjectType, Dictionary<string, Dictionary<string, Type>>> SubCategories { get; set; } = new()
    {
        [ProjectType.DES] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBD.Model.MapPiece),
                ["Object"] = typeof(MSBD.Model.Object),
                ["Enemy"] = typeof(MSBD.Model.Enemy),
                ["Player"] = typeof(MSBD.Model.Player),
                ["Collision"] = typeof(MSBD.Model.Collision),
                ["Navmesh"] = typeof(MSBD.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSBD.Event.Light),
                ["Sound"] = typeof(MSBD.Event.Sound),
                ["SFX"] = typeof(MSBD.Event.SFX),
                ["Wind"] = typeof(MSBD.Event.Wind),
                ["Treasure"] = typeof(MSBD.Event.Treasure),
                ["Generator"] = typeof(MSBD.Event.Generator),
                ["Message"] = typeof(MSBD.Event.Message)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region"] = typeof(MSBD.Region)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBD.Part.MapPiece),
                ["Object"] = typeof(MSBD.Part.Object),
                ["Enemy"] = typeof(MSBD.Part.Enemy),
                ["Player"] = typeof(MSBD.Part.Player),
                ["Collision"] = typeof(MSBD.Part.Collision),
                ["Protoboss"] = typeof(MSBD.Part.Protoboss),
                ["Navmesh"] = typeof(MSBD.Part.Navmesh),
                ["DummyObject"] = typeof(MSBD.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSBD.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSBD.Part.ConnectCollision)
            }
        },
        [ProjectType.DS1] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Model.MapPiece),
                ["Object"] = typeof(MSB1.Model.Object),
                ["Enemy"] = typeof(MSB1.Model.Enemy),
                ["Player"] = typeof(MSB1.Model.Player),
                ["Collision"] = typeof(MSB1.Model.Collision),
                ["Navmesh"] = typeof(MSB1.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSB1.Event.Light),
                ["Sound"] = typeof(MSB1.Event.Sound),
                ["SFX"] = typeof(MSB1.Event.SFX),
                ["Wind"] = typeof(MSB1.Event.Wind),
                ["Treasure"] = typeof(MSB1.Event.Treasure),
                ["Generator"] = typeof(MSB1.Event.Generator),
                ["Message"] = typeof(MSB1.Event.Message),
                ["ObjAct"] = typeof(MSB1.Event.ObjAct),
                ["SpawnPoint"] = typeof(MSB1.Event.SpawnPoint),
                ["MapOffset"] = typeof(MSB1.Event.MapOffset),
                ["Navmesh"] = typeof(MSB1.Event.Navmesh),
                ["Environment"] = typeof(MSB1.Event.Environment),
                ["PseudoMultiplayer"] = typeof(MSB1.Event.PseudoMultiplayer)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region"] = typeof(MSB1.Region)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Part.MapPiece),
                ["Object"] = typeof(MSB1.Part.Object),
                ["Enemy"] = typeof(MSB1.Part.Enemy),
                ["Player"] = typeof(MSB1.Part.Player),
                ["Collision"] = typeof(MSB1.Part.Collision),
                ["Navmesh"] = typeof(MSB1.Part.Navmesh),
                ["DummyObject"] = typeof(MSB1.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSB1.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB1.Part.ConnectCollision)
            }
        },
        [ProjectType.DS1R] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Model.MapPiece),
                ["Object"] = typeof(MSB1.Model.Object),
                ["Enemy"] = typeof(MSB1.Model.Enemy),
                ["Player"] = typeof(MSB1.Model.Player),
                ["Collision"] = typeof(MSB1.Model.Collision),
                ["Navmesh"] = typeof(MSB1.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSB1.Event.Light),
                ["Sound"] = typeof(MSB1.Event.Sound),
                ["SFX"] = typeof(MSB1.Event.SFX),
                ["Wind"] = typeof(MSB1.Event.Wind),
                ["Treasure"] = typeof(MSB1.Event.Treasure),
                ["Generator"] = typeof(MSB1.Event.Generator),
                ["Message"] = typeof(MSB1.Event.Message),
                ["ObjAct"] = typeof(MSB1.Event.ObjAct),
                ["SpawnPoint"] = typeof(MSB1.Event.SpawnPoint),
                ["MapOffset"] = typeof(MSB1.Event.MapOffset),
                ["Navmesh"] = typeof(MSB1.Event.Navmesh),
                ["Environment"] = typeof(MSB1.Event.Environment),
                ["PseudoMultiplayer"] = typeof(MSB1.Event.PseudoMultiplayer)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region"] = typeof(MSB1.Region)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB1.Part.MapPiece),
                ["Object"] = typeof(MSB1.Part.Object),
                ["Enemy"] = typeof(MSB1.Part.Enemy),
                ["Player"] = typeof(MSB1.Part.Player),
                ["Collision"] = typeof(MSB1.Part.Collision),
                ["Navmesh"] = typeof(MSB1.Part.Navmesh),
                ["DummyObject"] = typeof(MSB1.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSB1.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB1.Part.ConnectCollision)
            }
        },
        [ProjectType.DS2] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB2.Model.MapPiece),
                ["Object"] = typeof(MSB2.Model.Object),
                ["Collision"] = typeof(MSB2.Model.Collision),
                ["Navmesh"] = typeof(MSB2.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSB2.Event.Light),
                ["Shadow"] = typeof(MSB2.Event.Shadow),
                ["Fog"] = typeof(MSB2.Event.Fog),
                ["MapColor"] = typeof(MSB2.Event.MapColor),
                ["MapOffset"] = typeof(MSB2.Event.MapOffset),
                ["Warp"] = typeof(MSB2.Event.Warp),
                ["CheapMode"] = typeof(MSB2.Event.CheapMode)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region0"] = typeof(MSB2.Region.Region0),
                ["Light"] = typeof(MSB2.Region.Light),
                ["StartPoint"] = typeof(MSB2.Region.StartPoint),
                ["Sound"] = typeof(MSB2.Region.Sound),
                ["SFX"] = typeof(MSB2.Region.SFX),
                ["Wind"] = typeof(MSB2.Region.Wind),
                ["EnvLight"] = typeof(MSB2.Region.EnvLight),
                ["Fog"] = typeof(MSB2.Region.Fog)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB2.Part.MapPiece),
                ["Object"] = typeof(MSB2.Part.Object),
                ["Collision"] = typeof(MSB2.Part.Collision),
                ["Navmesh"] = typeof(MSB2.Part.Navmesh),
                ["ConnectCollision"] = typeof(MSB2.Part.ConnectCollision),
                ["WarpCollision"] = typeof(MSB2.Part.WarpCollision)
            }
        },
        [ProjectType.DS2S] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB2.Model.MapPiece),
                ["Object"] = typeof(MSB2.Model.Object),
                ["Collision"] = typeof(MSB2.Model.Collision),
                ["Navmesh"] = typeof(MSB2.Model.Navmesh),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Light"] = typeof(MSB2.Event.Light),
                ["Shadow"] = typeof(MSB2.Event.Shadow),
                ["Fog"] = typeof(MSB2.Event.Fog),
                ["MapColor"] = typeof(MSB2.Event.MapColor),
                ["MapOffset"] = typeof(MSB2.Event.MapOffset),
                ["Warp"] = typeof(MSB2.Event.Warp),
                ["CheapMode"] = typeof(MSB2.Event.CheapMode)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region0"] = typeof(MSB2.Region.Region0),
                ["Light"] = typeof(MSB2.Region.Light),
                ["StartPoint"] = typeof(MSB2.Region.StartPoint),
                ["Sound"] = typeof(MSB2.Region.Sound),
                ["SFX"] = typeof(MSB2.Region.SFX),
                ["Wind"] = typeof(MSB2.Region.Wind),
                ["EnvLight"] = typeof(MSB2.Region.EnvLight),
                ["Fog"] = typeof(MSB2.Region.Fog)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB2.Part.MapPiece),
                ["Object"] = typeof(MSB2.Part.Object),
                ["Collision"] = typeof(MSB2.Part.Collision),
                ["Navmesh"] = typeof(MSB2.Part.Navmesh),
                ["ConnectCollision"] = typeof(MSB2.Part.ConnectCollision),
                ["WarpCollision"] = typeof(MSB2.Part.WarpCollision)
            },
            ["PartPose"] = new Dictionary<string, Type>()
            {
                ["PartPose"] = typeof(MSB2.PartPose)
            }
        },
        [ProjectType.BB] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBB.Model.MapPiece),
                ["Object"] = typeof(MSBB.Model.Object),
                ["Enemy"] = typeof(MSBB.Model.Enemy),
                ["Item"] = typeof(MSBB.Model.Item),
                ["Player"] = typeof(MSBB.Model.Player),
                ["Collision"] = typeof(MSBB.Model.Collision),
                ["Navmesh"] = typeof(MSBB.Model.Navmesh),
                ["Other"] = typeof(MSBB.Model.Other),
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Sound"] = typeof(MSBB.Event.Sound),
                ["SFX"] = typeof(MSBB.Event.SFX),
                ["Treasure"] = typeof(MSBB.Event.Treasure),
                ["Generator"] = typeof(MSBB.Event.Generator),
                ["Message"] = typeof(MSBB.Event.Message),
                ["ObjAct"] = typeof(MSBB.Event.ObjAct),
                ["SpawnPoint"] = typeof(MSBB.Event.SpawnPoint),
                ["MapOffset"] = typeof(MSBB.Event.MapOffset),
                ["Navmesh"] = typeof(MSBB.Event.Navmesh),
                ["Environment"] = typeof(MSBB.Event.Environment),
                ["WindSFX"] = typeof(MSBB.Event.WindSFX),
                ["PatrolInfo"] = typeof(MSBB.Event.PatrolInfo),
                ["DarkLock"] = typeof(MSBB.Event.DarkLock),
                ["PlatoonInfo"] = typeof(MSBB.Event.PlatoonInfo),
                ["MultiSummon"] = typeof(MSBB.Event.MultiSummon),
                ["Other"] = typeof(MSBB.Event.Other)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["Region"] = typeof(MSBB.Region)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBB.Part.MapPiece),
                ["Object"] = typeof(MSBB.Part.Object),
                ["Enemy"] = typeof(MSBB.Part.Enemy),
                ["Player"] = typeof(MSBB.Part.Player),
                ["Collision"] = typeof(MSBB.Part.Collision),
                ["Navmesh"] = typeof(MSBB.Part.Navmesh),
                ["DummyObject"] = typeof(MSBB.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSBB.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSBB.Part.ConnectCollision),
                ["Other"] = typeof(MSBB.Part.Other),
            }
        },
        [ProjectType.DS3] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB3.Model.MapPiece),
                ["Object"] = typeof(MSB3.Model.Object),
                ["Enemy"] = typeof(MSB3.Model.Enemy),
                ["Player"] = typeof(MSB3.Model.Player),
                ["Collision"] = typeof(MSB3.Model.Collision),
                ["Other"] = typeof(MSB3.Model.Other)
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Treasure"] = typeof(MSB3.Event.Treasure),
                ["Generator"] = typeof(MSB3.Event.Generator),
                ["ObjAct"] = typeof(MSB3.Event.ObjAct),
                ["MapOffset"] = typeof(MSB3.Event.MapOffset),
                ["PseudoMultiplayer"] = typeof(MSB3.Event.PseudoMultiplayer),
                ["PatrolInfo"] = typeof(MSB3.Event.PatrolInfo),
                ["PlatoonInfo"] = typeof(MSB3.Event.PlatoonInfo),
                ["Other"] = typeof(MSB3.Event.Other)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["InvasionPoint"] = typeof(MSB3.Region.InvasionPoint),
                ["EnvironmentMapPoint"] = typeof(MSB3.Region.EnvironmentMapPoint),
                ["Sound"] = typeof(MSB3.Region.Sound),
                ["SFX"] = typeof(MSB3.Region.SFX),
                ["WindSFX"] = typeof(MSB3.Region.WindSFX),
                ["SpawnPoint"] = typeof(MSB3.Region.SpawnPoint),
                ["Message"] = typeof(MSB3.Region.Message),
                ["PatrolRoute"] = typeof(MSB3.Region.PatrolRoute),
                ["MovementPoint"] = typeof(MSB3.Region.MovementPoint),
                ["WarpPoint"] = typeof(MSB3.Region.WarpPoint),
                ["ActivationArea"] = typeof(MSB3.Region.ActivationArea),
                ["Event"] = typeof(MSB3.Region.Event),
                ["Logic"] = typeof(MSB3.Region.Logic),
                ["EnvironmentMapEffectBox"] = typeof(MSB3.Region.EnvironmentMapEffectBox),
                ["WindArea"] = typeof(MSB3.Region.WindArea),
                ["MufflingBox"] = typeof(MSB3.Region.MufflingBox),
                ["MufflingPortal"] = typeof(MSB3.Region.MufflingPortal),
                ["Other"] = typeof(MSB3.Region.Other)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB3.Part.MapPiece),
                ["Object"] = typeof(MSB3.Part.Object),
                ["Enemy"] = typeof(MSB3.Part.Enemy),
                ["Player"] = typeof(MSB3.Part.Player),
                ["Collision"] = typeof(MSB3.Part.Collision),
                ["DummyObject"] = typeof(MSB3.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSB3.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB3.Part.ConnectCollision)
            },
            ["Route"] = new Dictionary<string, Type>()
            {
                ["Route"] = typeof(MSB3.Route)
            },
            ["Layer"] = new Dictionary<string, Type>()
            {
                ["Layer"] = typeof(MSB3.Layer)
            },
            ["PartsPose"] = new Dictionary<string, Type>()
            {
                ["PartsPose"] = typeof(MSB3.PartsPose)
            }
        },
        [ProjectType.SDT] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBS.Model.MapPiece),
                ["Object"] = typeof(MSBS.Model.Object),
                ["Enemy"] = typeof(MSBS.Model.Enemy),
                ["Player"] = typeof(MSBS.Model.Player),
                ["Collision"] = typeof(MSBS.Model.Collision)
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Treasure"] = typeof(MSBS.Event.Treasure),
                ["Generator"] = typeof(MSBS.Event.Generator),
                ["ObjAct"] = typeof(MSBS.Event.ObjAct),
                ["MapOffset"] = typeof(MSBS.Event.MapOffset),
                ["PatrolInfo"] = typeof(MSBS.Event.PatrolInfo),
                ["PlatoonInfo"] = typeof(MSBS.Event.PlatoonInfo),
                ["ResourceItemInfo"] = typeof(MSBS.Event.ResourceItemInfo),
                ["GrassLodParam"] = typeof(MSBS.Event.GrassLodParam),
                ["SkitInfo"] = typeof(MSBS.Event.SkitInfo),
                ["PlacementGroup"] = typeof(MSBS.Event.PlacementGroup),
                ["PartsGroup"] = typeof(MSBS.Event.PartsGroup),
                ["Talk"] = typeof(MSBS.Event.Talk),
                ["AutoDrawGroupCollision"] = typeof(MSBS.Event.AutoDrawGroupCollision),
                ["Other"] = typeof(MSBS.Event.Other)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["InvasionPoint"] = typeof(MSBS.Region.InvasionPoint),
                ["EnvironmentMapPoint"] = typeof(MSBS.Region.EnvironmentMapPoint),
                ["Sound"] = typeof(MSBS.Region.Sound),
                ["SFX"] = typeof(MSBS.Region.SFX),
                ["WindSFX"] = typeof(MSBS.Region.WindSFX),
                ["SpawnPoint"] = typeof(MSBS.Region.SpawnPoint),
                ["PatrolRoute"] = typeof(MSBS.Region.PatrolRoute),
                ["WarpPoint"] = typeof(MSBS.Region.WarpPoint),
                ["ActivationArea"] = typeof(MSBS.Region.ActivationArea),
                ["Event"] = typeof(MSBS.Region.Event),
                ["Logic"] = typeof(MSBS.Region.Logic),
                ["EnvironmentMapEffectBox"] = typeof(MSBS.Region.EnvironmentMapEffectBox),
                ["WindArea"] = typeof(MSBS.Region.WindArea),
                ["MufflingBox"] = typeof(MSBS.Region.MufflingBox),
                ["MufflingPortal"] = typeof(MSBS.Region.MufflingPortal),
                ["SoundSpaceOverride"] = typeof(MSBS.Region.SoundSpaceOverride),
                ["MufflingPlane"] = typeof(MSBS.Region.MufflingPlane),
                ["PartsGroupArea"] = typeof(MSBS.Region.PartsGroupArea),
                ["AutoDrawGroupPoint"] = typeof(MSBS.Region.AutoDrawGroupPoint),
                ["Other"] = typeof(MSBS.Region.Other)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBS.Part.MapPiece),
                ["Object"] = typeof(MSBS.Part.Object),
                ["Enemy"] = typeof(MSBS.Part.Enemy),
                ["Player"] = typeof(MSBS.Part.Player),
                ["Collision"] = typeof(MSBS.Part.Collision),
                ["DummyObject"] = typeof(MSBS.Part.DummyObject),
                ["DummyEnemy"] = typeof(MSBS.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSBS.Part.ConnectCollision),
            },
            ["Route"] = new Dictionary<string, Type>()
            {
                ["MufflingPortalLink"] = typeof(MSBS.Route.MufflingPortalLink),
                ["MufflingBoxLink"] = typeof(MSBS.Route.MufflingBoxLink)
            }
        },
        [ProjectType.ER] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBE.Model.MapPiece),
                ["Asset"] = typeof(MSBE.Model.Asset),
                ["Enemy"] = typeof(MSBE.Model.Enemy),
                ["Player"] = typeof(MSBE.Model.Player),
                ["Collision"] = typeof(MSBE.Model.Collision)
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Treasure"] = typeof(MSBE.Event.Treasure),
                ["Generator"] = typeof(MSBE.Event.Generator),
                ["ObjAct"] = typeof(MSBE.Event.ObjAct),
                ["Navmesh"] = typeof(MSBE.Event.Navmesh),
                ["PseudoMultiplayer"] = typeof(MSBE.Event.PseudoMultiplayer),
                ["PlatoonInfo"] = typeof(MSBE.Event.PlatoonInfo),
                ["PatrolInfo"] = typeof(MSBE.Event.PatrolInfo),
                ["Mount"] = typeof(MSBE.Event.Mount),
                ["SignPool"] = typeof(MSBE.Event.SignPool),
                ["RetryPoint"] = typeof(MSBE.Event.RetryPoint),
                ["OnlinePseudoMultiplayer"] = typeof(MSBE.Event.OnlinePseudoMultiplayer),
                ["Other"] = typeof(MSBE.Event.Other)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["InvasionPoint"] = typeof(MSBE.Region.InvasionPoint),
                ["EnvironmentMapPoint"] = typeof(MSBE.Region.EnvironmentMapPoint),
                ["Sound"] = typeof(MSBE.Region.Sound),
                ["SFX"] = typeof(MSBE.Region.SFX),
                ["WindSFX"] = typeof(MSBE.Region.WindSFX),
                ["SpawnPoint"] = typeof(MSBE.Region.SpawnPoint),
                ["Message"] = typeof(MSBE.Region.Message),
                ["EnvironmentMapEffectBox"] = typeof(MSBE.Region.EnvironmentMapEffectBox),
                ["WindArea"] = typeof(MSBE.Region.WindArea),
                ["Connection"] = typeof(MSBE.Region.Connection),
                ["PatrolRoute22"] = typeof(MSBE.Region.PatrolRoute22),
                ["BuddySummonPoint"] = typeof(MSBE.Region.BuddySummonPoint),
                ["DisableTumbleweed"] = typeof(MSBE.Region.DisableTumbleweed),
                ["MufflingBox"] = typeof(MSBE.Region.MufflingBox),
                ["MufflingPortal"] = typeof(MSBE.Region.MufflingPortal),
                ["SoundRegion"] = typeof(MSBE.Region.SoundRegion),
                ["MufflingPlane"] = typeof(MSBE.Region.MufflingPlane),
                ["PatrolRoute"] = typeof(MSBE.Region.PatrolRoute),
                ["MapPoint"] = typeof(MSBE.Region.MapPoint),
                ["WeatherOverride"] = typeof(MSBE.Region.WeatherOverride),
                ["AutoDrawGroupPoint"] = typeof(MSBE.Region.AutoDrawGroupPoint),
                ["GroupDefeatReward"] = typeof(MSBE.Region.GroupDefeatReward),
                ["MapPointDiscoveryOverride"] = typeof(MSBE.Region.MapPointDiscoveryOverride),
                ["MapPointParticipationOverride"] = typeof(MSBE.Region.MapPointParticipationOverride),
                ["Hitset"] = typeof(MSBE.Region.Hitset),
                ["FastTravelRestriction"] = typeof(MSBE.Region.FastTravelRestriction),
                ["WeatherCreateAssetPoint"] = typeof(MSBE.Region.WeatherCreateAssetPoint),
                ["PlayArea"] = typeof(MSBE.Region.PlayArea),
                ["EnvironmentMapOutput"] = typeof(MSBE.Region.EnvironmentMapOutput),
                ["MountJump"] = typeof(MSBE.Region.MountJump),
                ["Dummy"] = typeof(MSBE.Region.Dummy),
                ["FallPreventionRemoval"] = typeof(MSBE.Region.FallPreventionRemoval),
                ["NavmeshCutting"] = typeof(MSBE.Region.NavmeshCutting),
                ["MapNameOverride"] = typeof(MSBE.Region.MapNameOverride),
                ["MountJumpFall"] = typeof(MSBE.Region.MountJumpFall),
                ["HorseRideOverride"] = typeof(MSBE.Region.HorseRideOverride),
                ["LockedMountJump"] = typeof(MSBE.Region.LockedMountJump),
                ["LockedMountJumpFall"] = typeof(MSBE.Region.LockedMountJumpFall)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSBE.Part.MapPiece),
                ["Enemy"] = typeof(MSBE.Part.Enemy),
                ["Player"] = typeof(MSBE.Part.Player),
                ["Collision"] = typeof(MSBE.Part.Collision),
                ["DummyAsset"] = typeof(MSBE.Part.DummyAsset),
                ["DummyEnemy"] = typeof(MSBE.Part.DummyEnemy),
                ["MapPiece"] = typeof(MSBE.Part.MapPiece),
                ["ConnectCollision"] = typeof(MSBE.Part.ConnectCollision),
                ["Asset"] = typeof(MSBE.Part.Asset)
            },
            ["Route"] = new Dictionary<string, Type>()
            {
                ["MufflingPortalLink"] = typeof(MSBE.Route.MufflingPortalLink),
                ["MufflingBoxLink"] = typeof(MSBE.Route.MufflingBoxLink),
                ["Other"] = typeof(MSBE.Route.Other)
            }
        },
        [ProjectType.AC6] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB_AC6.Model.MapPiece),
                ["Asset"] = typeof(MSB_AC6.Model.Asset),
                ["Enemy"] = typeof(MSB_AC6.Model.Enemy),
                ["Player"] = typeof(MSB_AC6.Model.Player),
                ["Collision"] = typeof(MSB_AC6.Model.Collision)
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Treasure"] = typeof(MSB_AC6.Event.Treasure),
                ["Generator"] = typeof(MSB_AC6.Event.Generator),
                ["MapOffset"] = typeof(MSB_AC6.Event.MapOffset),
                ["PlatoonInfo"] = typeof(MSB_AC6.Event.PlatoonInfo),
                ["PatrolRoute"] = typeof(MSB_AC6.Event.PatrolRoute),
                ["MapGimmick"] = typeof(MSB_AC6.Event.MapGimmick),
                ["Other"] = typeof(MSB_AC6.Event.Other)
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["EntryPoint"] = typeof(MSB_AC6.Region.EntryPoint),
                ["EnvMapPoint"] = typeof(MSB_AC6.Region.EnvMapPoint),
                ["Sound"] = typeof(MSB_AC6.Region.Sound),
                ["SFX"] = typeof(MSB_AC6.Region.SFX),
                ["WindSFX"] = typeof(MSB_AC6.Region.WindSFX),
                ["EnvMapEffectBox"] = typeof(MSB_AC6.Region.EnvMapEffectBox),
                ["WindPlacement"] = typeof(MSB_AC6.Region.WindPlacement),
                ["MufflingBox"] = typeof(MSB_AC6.Region.MufflingBox),
                ["MufflingPortal"] = typeof(MSB_AC6.Region.MufflingPortal),
                ["SoundOverride"] = typeof(MSB_AC6.Region.SoundOverride),
                ["Patrol"] = typeof(MSB_AC6.Region.Patrol),
                ["FeMapDisplay"] = typeof(MSB_AC6.Region.FeMapDisplay),
                ["OperationalArea"] = typeof(MSB_AC6.Region.OperationalArea),
                ["AiInformationSharing"] = typeof(MSB_AC6.Region.AiInformationSharing),
                ["AiTarget"] = typeof(MSB_AC6.Region.AiTarget),
                ["WwiseEnvironmentSound"] = typeof(MSB_AC6.Region.WwiseEnvironmentSound),
                ["NaviGeneration"] = typeof(MSB_AC6.Region.NaviGeneration),
                ["TopdownView"] = typeof(MSB_AC6.Region.TopdownView),
                ["CharacterFollowing"] = typeof(MSB_AC6.Region.CharacterFollowing),
                ["NavmeshCostControl"] = typeof(MSB_AC6.Region.NavmeshCostControl),
                ["ArenaControl"] = typeof(MSB_AC6.Region.ArenaControl),
                ["ArenaAppearance"] = typeof(MSB_AC6.Region.ArenaAppearance),
                ["GarageCamera"] = typeof(MSB_AC6.Region.GarageCamera),
                ["JumpEdgeRestriction"] = typeof(MSB_AC6.Region.JumpEdgeRestriction),
                ["CutscenePlayback"] = typeof(MSB_AC6.Region.CutscenePlayback),
                ["FallPreventionWallRemoval"] = typeof(MSB_AC6.Region.FallPreventionWallRemoval),
                ["BigJump"] = typeof(MSB_AC6.Region.BigJump),
                ["Other"] = typeof(MSB_AC6.Region.Other)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB_AC6.Part.MapPiece),
                ["Asset"] = typeof(MSB_AC6.Part.Asset),
                ["Enemy"] = typeof(MSB_AC6.Part.Enemy),
                ["Player"] = typeof(MSB_AC6.Part.Player),
                ["Collision"] = typeof(MSB_AC6.Part.Collision),
                ["DummyAsset"] = typeof(MSB_AC6.Part.DummyAsset),
                ["DummyEnemy"] = typeof(MSB_AC6.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB_AC6.Part.ConnectCollision)
            },
            ["Route"] = new Dictionary<string, Type>()
            {
                ["Route"] = typeof(MSB_AC6.Route)
            },
            ["Layer"] = new Dictionary<string, Type>()
            {
                ["Layer"] = typeof(MSB_AC6.Layer)
            }
        },

        [ProjectType.NR] = new Dictionary<string, Dictionary<string, Type>>()
        {
            ["Model"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB_NR.Model.MapPiece),
                ["Asset"] = typeof(MSB_NR.Model.Asset),
                ["Enemy"] = typeof(MSB_NR.Model.Enemy),
                ["Player"] = typeof(MSB_NR.Model.Player),
                ["Collision"] = typeof(MSB_NR.Model.Collision),
                ["UniqueCharacter"] = typeof(MSB_NR.Model.UniqueCharacter),
                ["UniqueAsset"] = typeof(MSB_NR.Model.UniqueAsset)
            },
            ["Event"] = new Dictionary<string, Type>()
            {
                ["Treasure"] = typeof(MSB_NR.Event.Treasure),
                ["Generator"] = typeof(MSB_NR.Event.Generator),
                ["ObjAct"] = typeof(MSB_NR.Event.ObjAct),
                ["MapOffset"] = typeof(MSB_NR.Event.MapOffset),
                ["PseudoMultiplayer"] = typeof(MSB_NR.Event.PseudoMultiplayer),
                ["PatrolInfo"] = typeof(MSB_NR.Event.PatrolInfo),
                ["PlatoonInfo"] = typeof(MSB_NR.Event.PlatoonInfo),
                ["PatrolRoute"] = typeof(MSB_NR.Event.PatrolRoute),
                ["Riding"] = typeof(MSB_NR.Event.Riding),
                ["AutoDrawGroup"] = typeof(MSB_NR.Event.AutoDrawGroup),
                ["SignPuddle"] = typeof(MSB_NR.Event.SignPuddle),
                ["RetryPoint"] = typeof(MSB_NR.Event.RetryPoint),
                ["BirdRoute"] = typeof(MSB_NR.Event.BirdRoute),
                ["TalkInfo"] = typeof(MSB_NR.Event.TalkInfo),
                ["TeamFight"] = typeof(MSB_NR.Event.TeamFight),
                ["Other"] = typeof(MSB_NR.Event.Other),
            },
            ["Region"] = new Dictionary<string, Type>()
            {
                ["EntryPoint"] = typeof(MSB_NR.Region.EntryPoint),
                ["EnvMapPoint"] = typeof(MSB_NR.Region.EnvMapPoint),
                ["RespawnPoint"] = typeof(MSB_NR.Region.RespawnPoint),
                ["Sound"] = typeof(MSB_NR.Region.Sound),
                ["SFX"] = typeof(MSB_NR.Region.SFX),
                ["WindSFX"] = typeof(MSB_NR.Region.WindSFX),
                ["ReturnPoint"] = typeof(MSB_NR.Region.ReturnPoint),
                ["Message"] = typeof(MSB_NR.Region.Message),
                ["EnvMapEffectBox"] = typeof(MSB_NR.Region.EnvMapEffectBox),
                ["WindPlacement"] = typeof(MSB_NR.Region.WindPlacement),
                ["MapConnection"] = typeof(MSB_NR.Region.MapConnection),
                ["SourceWaypoint"] = typeof(MSB_NR.Region.SourceWaypoint),
                ["StaticWaypoint"] = typeof(MSB_NR.Region.StaticWaypoint),
                ["MapGridLayerConnection"] = typeof(MSB_NR.Region.MapGridLayerConnection),
                ["EnemySpawnPoint"] = typeof(MSB_NR.Region.EnemySpawnPoint),
                ["BuddySummonPoint"] = typeof(MSB_NR.Region.BuddySummonPoint),
                ["RollingObjectOverride"] = typeof(MSB_NR.Region.RollingObjectOverride),
                ["MufflingBox"] = typeof(MSB_NR.Region.MufflingBox),
                ["MufflingPortal"] = typeof(MSB_NR.Region.MufflingPortal),
                ["SoundOverride"] = typeof(MSB_NR.Region.SoundOverride),
                ["MufflingPlane"] = typeof(MSB_NR.Region.MufflingPlane),
                ["PatrolPoint"] = typeof(MSB_NR.Region.PatrolPoint),
                ["MapPoint"] = typeof(MSB_NR.Region.MapPoint),
                ["SoundState"] = typeof(MSB_NR.Region.SoundState),
                ["MufflingBox"] = typeof(MSB_NR.Region.MufflingBox),
                ["MapInfoOverride"] = typeof(MSB_NR.Region.MapInfoOverride),
                ["AutoDrawGroupSample"] = typeof(MSB_NR.Region.AutoDrawGroupSample),
                ["MassPlacement"] = typeof(MSB_NR.Region.MassPlacement),
                ["MapPointDiscoveryOverride"] = typeof(MSB_NR.Region.MapPointDiscoveryOverride),
                ["MapPointParticipationOverride"] = typeof(MSB_NR.Region.MapPointParticipationOverride),
                ["HitSetting"] = typeof(MSB_NR.Region.HitSetting),
                ["FastTravelOverride"] = typeof(MSB_NR.Region.FastTravelOverride),
                ["WeatherAssetGeneration"] = typeof(MSB_NR.Region.WeatherAssetGeneration),
                ["PlayArea"] = typeof(MSB_NR.Region.PlayArea),
                ["MidRangeEnvMapOutput"] = typeof(MSB_NR.Region.MidRangeEnvMapOutput),
                ["MapVisibilityOverride"] = typeof(MSB_NR.Region.MapVisibilityOverride),
                ["BigJump"] = typeof(MSB_NR.Region.BigJump),
                ["OpenCharacterActivateLimit"] = typeof(MSB_NR.Region.OpenCharacterActivateLimit),
                ["SoundDummy"] = typeof(MSB_NR.Region.SoundDummy),
                ["FallPreventionOverride"] = typeof(MSB_NR.Region.FallPreventionOverride),
                ["NavmeshCutting"] = typeof(MSB_NR.Region.NavmeshCutting),
                ["MapNameOverride"] = typeof(MSB_NR.Region.MapNameOverride),
                ["BigJumpExit"] = typeof(MSB_NR.Region.BigJumpExit),
                ["MountOverride"] = typeof(MSB_NR.Region.MountOverride),
                ["SmallBaseAttach"] = typeof(MSB_NR.Region.SmallBaseAttach),
                ["BirdRoute"] = typeof(MSB_NR.Region.BirdRoute),
                ["ClearInfo"] = typeof(MSB_NR.Region.ClearInfo),
                ["RespawnOverride"] = typeof(MSB_NR.Region.RespawnOverride),
                ["UserEdgeRemovalInner"] = typeof(MSB_NR.Region.UserEdgeRemovalInner),
                ["UserEdgeRemovalOuter"] = typeof(MSB_NR.Region.UserEdgeRemovalOuter),
                ["BigJumpSealable"] = typeof(MSB_NR.Region.BigJumpSealable),
                ["Other"] = typeof(MSB_NR.Region.Other)
            },
            ["Part"] = new Dictionary<string, Type>()
            {
                ["MapPiece"] = typeof(MSB_NR.Part.MapPiece),
                ["Enemy"] = typeof(MSB_NR.Part.Enemy),
                ["Player"] = typeof(MSB_NR.Part.Player),
                ["Collision"] = typeof(MSB_NR.Part.Collision),
                ["DummyAsset"] = typeof(MSB_NR.Part.DummyAsset),
                ["DummyEnemy"] = typeof(MSB_NR.Part.DummyEnemy),
                ["ConnectCollision"] = typeof(MSB_NR.Part.ConnectCollision),
                ["Asset"] = typeof(MSB_NR.Part.Asset)
            },
            ["Route"] = new Dictionary<string, Type>()
            {
                ["MufflingPortalLink"] = typeof(MSB_NR.Route.MufflingPortalLink),
                ["MufflingBoxLink"] = typeof(MSB_NR.Route.MufflingBoxLink),
                ["Other"] = typeof(MSB_NR.Route.Other)
            }
        }
    };

    public static Dictionary<string, Type> GetBaseCategories(ProjectEntry project)
    {
        var dict = new Dictionary<string, Type>();

        if(BaseCategories.ContainsKey(project.Descriptor.ProjectType))
        {
            dict = BaseCategories[project.Descriptor.ProjectType];
        }

        return dict;
    }

    public static Dictionary<string, Type> GetSubCategories(ProjectEntry project, string baseCategory)
    {
        var dict = new Dictionary<string, Type>();
        var subCategory = new Dictionary<string, Dictionary<string, Type>>();

        if (SubCategories.ContainsKey(project.Descriptor.ProjectType))
        {
            subCategory = SubCategories[project.Descriptor.ProjectType];
        }

        if (subCategory.ContainsKey(baseCategory))
        {
            dict = subCategory[baseCategory];
        }

        return dict;
    }
}
