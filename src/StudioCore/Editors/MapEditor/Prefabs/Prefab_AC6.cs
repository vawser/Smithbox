using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using StudioCore.UserProject;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StudioCore.Platform;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using System.Linq;
using StudioCore.Banks;
using System.Reflection;
using StudioCore.BanksMain;

namespace StudioCore.Editors.MapEditor.Prefabs;

public class Prefab_AC6
{
    public string PrefabName = "";
    public string PrefixSeparator = "[]";
    public ProjectType Type = ProjectType.AC6;

    public List<string> TagList { get; set; }

    /// <summary>
    /// Bytes of the MSB that stores prefab data.
    /// </summary>
    public byte[] AssetContainerBytes { get; set; }

    /// <summary>
    /// List of AssetInfo derived from MSB AssetContainerBytes.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<PrefabInfo_AC6> PrefabInfoChildren = new();

    /// <summary>
    /// List of Msb Entities derived from AssetInfoChildren.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<MsbEntity> MsbEntityChildren = new();


    // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
    [Newtonsoft.Json.JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;

    public Prefab_AC6()
    { }

    public Prefab_AC6(HashSet<MsbEntity> entities)
    {
        foreach (var ent in entities)
        {
            // Parts
            if (ent.WrappedObject is MSB_AC6.Part.MapPiece mapPiece)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mapPiece));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Enemy enemy)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, enemy));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Player player)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, player));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Collision col)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, col));
            }
            if (ent.WrappedObject is MSB_AC6.Part.DummyAsset dummyAsset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, dummyAsset));
            }
            if (ent.WrappedObject is MSB_AC6.Part.DummyEnemy dummyEnemy)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, dummyEnemy));
            }
            if (ent.WrappedObject is MSB_AC6.Part.ConnectCollision connectCol)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, connectCol));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Asset asset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, asset));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Object obj)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, obj));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Item item)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, item));
            }
            if (ent.WrappedObject is MSB_AC6.Part.NPCWander npcWander)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, npcWander));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Protoboss protoboss)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, protoboss));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Navmesh navmesh)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, navmesh));
            }
            if (ent.WrappedObject is MSB_AC6.Part.Invalid invalid)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, invalid));
            }

            // Regions
            if (ent.WrappedObject is MSB_AC6.Region.EntryPoint entryPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, entryPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.EnvMapPoint envMapPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, envMapPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Sound sound)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, sound));
            }
            if (ent.WrappedObject is MSB_AC6.Region.SFX sfx)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, sfx));
            }
            if (ent.WrappedObject is MSB_AC6.Region.WindSFX windSfx)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, windSfx));
            }
            if (ent.WrappedObject is MSB_AC6.Region.EnvMapEffectBox envMapEffectBox)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, envMapEffectBox));
            }
            if (ent.WrappedObject is MSB_AC6.Region.MufflingBox mufflingBox)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mufflingBox));
            }
            if (ent.WrappedObject is MSB_AC6.Region.MufflingPortal mufflingPortal)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mufflingPortal));
            }
            if (ent.WrappedObject is MSB_AC6.Region.SoundOverride soundOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, soundOverride));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Patrol patrol)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, patrol));
            }
            if (ent.WrappedObject is MSB_AC6.Region.FeMapDisplay feMapDisplay)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, feMapDisplay));
            }
            if (ent.WrappedObject is MSB_AC6.Region.OperationalArea operationalArea)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, operationalArea));
            }
            if (ent.WrappedObject is MSB_AC6.Region.AiTarget aiTarget)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, aiTarget));
            }
            if (ent.WrappedObject is MSB_AC6.Region.WwiseEnvironmentSound wwiseEnvironmentalSound)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, wwiseEnvironmentalSound));
            }
            if (ent.WrappedObject is MSB_AC6.Region.CharacterFollowing characterFollowing)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, characterFollowing));
            }
            if (ent.WrappedObject is MSB_AC6.Region.NavmeshCostControl navmeshCostControl)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, navmeshCostControl));
            }
            if (ent.WrappedObject is MSB_AC6.Region.ArenaControl arenaControl)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, arenaControl));
            }
            if (ent.WrappedObject is MSB_AC6.Region.ArenaAppearance arenaApperance)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, arenaApperance));
            }
            if (ent.WrappedObject is MSB_AC6.Region.GarageCamera garageCamera)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, garageCamera));
            }
            if (ent.WrappedObject is MSB_AC6.Region.CutscenePlayback cutscenePlayback)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, cutscenePlayback));
            }
            if (ent.WrappedObject is MSB_AC6.Region.FallPreventionWallRemoval fallPreventionWallRemoval)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, fallPreventionWallRemoval));
            }
            if (ent.WrappedObject is MSB_AC6.Region.BigJump bigJump)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, bigJump));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Other other)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, other));
            }
            if (ent.WrappedObject is MSB_AC6.Region.None none)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, none));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_3 unk3)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk3));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_7 unk7)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk7));
            }
            if (ent.WrappedObject is MSB_AC6.Region.ReturnPoint returnPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, returnPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Message message)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, message));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_10 unk10)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk10));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_11 unk11)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk11));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_12 unk12)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk12));
            }
            if (ent.WrappedObject is MSB_AC6.Region.FallReturnPoint fallReturnPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, fallReturnPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_14 unk14)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk14));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_15 unk15)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk15));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_16 unk16)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk16));
            }
            if (ent.WrappedObject is MSB_AC6.Region.WindPlacement windPlacement)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, windPlacement));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_19 unk19)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk19));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Unknown_20 unk20)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk20));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Connection connection)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, connection));
            }
            if (ent.WrappedObject is MSB_AC6.Region.SourceWaypoint sourceWayPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, sourceWayPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.StaticWaypoint staticWaypoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, staticWaypoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.MapGridLayerConnection mapGridLayerConnection)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mapGridLayerConnection));
            }
            if (ent.WrappedObject is MSB_AC6.Region.EnemySpawnPoint enemySpawnPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, enemySpawnPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.BuddySummonPoint buddySummonPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, buddySummonPoint));
            }
            if (ent.WrappedObject is MSB_AC6.Region.RollingAssetGeneration rollingAssetGeneration)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, rollingAssetGeneration));
            }
            if (ent.WrappedObject is MSB_AC6.Region.ElectroMagneticStorm electroStorm)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, electroStorm));
            }
            if (ent.WrappedObject is MSB_AC6.Region.AiInformationSharing aiInfoSharing)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, aiInfoSharing));
            }
            if (ent.WrappedObject is MSB_AC6.Region.WaveSimulation waveSim)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, waveSim));
            }
            if (ent.WrappedObject is MSB_AC6.Region.Cover cover)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, cover));
            }
            if (ent.WrappedObject is MSB_AC6.Region.MissionPlacement missionPlacement)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, missionPlacement));
            }
            if (ent.WrappedObject is MSB_AC6.Region.NaviVolumeResolution naviVolumeResolution)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, naviVolumeResolution));
            }
            if (ent.WrappedObject is MSB_AC6.Region.MiniArea miniArea)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, miniArea));
            }
            if (ent.WrappedObject is MSB_AC6.Region.ConnectionBorder connectionBorder)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, connectionBorder));
            }
            if (ent.WrappedObject is MSB_AC6.Region.NaviGeneration naviGeneration)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, naviGeneration));
            }
            if (ent.WrappedObject is MSB_AC6.Region.TopdownView topdownView)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, topdownView));
            }
            if (ent.WrappedObject is MSB_AC6.Region.NaviCvCancel naviCvCancel)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, naviCvCancel));
            }
            if (ent.WrappedObject is MSB_AC6.Region.JumpEdgeRestriction jumpEdgeRestriction)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, jumpEdgeRestriction));
            }

            // Events
            if (ent.WrappedObject is MSB_AC6.Event.Treasure treasure)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, treasure));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Generator generator)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, generator));
            }
            if (ent.WrappedObject is MSB_AC6.Event.MapOffset mapOffset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mapOffset));
            }
            if (ent.WrappedObject is MSB_AC6.Event.PlatoonInfo platoonInfo)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, platoonInfo));
            }
            if (ent.WrappedObject is MSB_AC6.Event.PatrolInfo patrolInfo)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, patrolInfo));
            }
            if (ent.WrappedObject is MSB_AC6.Event.MapGimmick mapGimmick)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mapGimmick));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Light lightEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, lightEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Sound soundEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, soundEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Sfx sfxEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, sfxEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.MapWindSfx mapWindSfx)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, mapWindSfx));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Message messageEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, messageEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.ObjAct objAct)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, objAct));
            }
            if (ent.WrappedObject is MSB_AC6.Event.ReturnPoint returnPointEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, returnPointEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Navmesh navmeshEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, navmeshEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Unknown_11 unk11Event)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk11Event));
            }
            if (ent.WrappedObject is MSB_AC6.Event.WindSfx windSfxEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, windSfxEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Unknown_16 unk16Event)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk16Event));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Unknown_17 unk17Event)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk17Event));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Unknown_18 unk18Event)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk18Event));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Unknown_19 unk19Event)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, unk19Event));
            }
            if (ent.WrappedObject is MSB_AC6.Event.PatrolRoute patrolRouteEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, patrolRouteEvent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Riding riding)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, riding));
            }
            if (ent.WrappedObject is MSB_AC6.Event.StrategyRoute strategyRoute)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, strategyRoute));
            }
            if (ent.WrappedObject is MSB_AC6.Event.PatrolRoutePermanent patrolRoutePermanent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, patrolRoutePermanent));
            }
            if (ent.WrappedObject is MSB_AC6.Event.Other otherEvent)
            {
                PrefabInfoChildren.Add(new PrefabInfo_AC6(this, otherEvent));
            }
        }
    }
    public class PrefabInfo_AC6
    {

        public Prefab_AC6 Parent;

        public AssetInfoDataType DataType = AssetInfoDataType.None;

        public enum AssetInfoDataType
        {
            None = -1,
            Part = 1000,
            Region = 2000,
            Event = 3000
        }

        public object InnerObject = null;

        // Parts
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.MapPiece mapPiece)
        {
            InnerObject = mapPiece.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.MapPiece temp = InnerObject as MSB_AC6.Part.MapPiece;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Enemy enemy)
        {
            InnerObject = enemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Enemy temp = InnerObject as MSB_AC6.Part.Enemy;
            temp.CollisionPartIndex = -1;
            temp.CollisionPartName = "";
            temp.WalkRouteIndex = -1;
            temp.WalkRouteName = "";

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Player player)
        {
            InnerObject = player.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Player temp = InnerObject as MSB_AC6.Part.Player;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Collision collision)
        {
            InnerObject = collision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Collision temp = InnerObject as MSB_AC6.Part.Collision;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.DummyAsset dummyAsset)
        {
            InnerObject = dummyAsset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.DummyAsset temp = InnerObject as MSB_AC6.Part.DummyAsset;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.DummyEnemy dummyEnemy)
        {
            InnerObject = dummyEnemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.DummyEnemy temp = InnerObject as MSB_AC6.Part.DummyEnemy;
            temp.CollisionPartIndex = -1;
            temp.CollisionPartName = "";
            temp.WalkRouteIndex = -1;
            temp.WalkRouteName = "";

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.ConnectCollision connectCollision)
        {
            InnerObject = connectCollision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.ConnectCollision temp = InnerObject as MSB_AC6.Part.ConnectCollision;
            temp.CollisionName = "";
            temp.CollisionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Asset asset)
        {
            InnerObject = asset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Asset temp = InnerObject as MSB_AC6.Part.Asset;
            Array.Clear(temp.PartNames);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Object obj)
        {
            InnerObject = obj.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Object temp = InnerObject as MSB_AC6.Part.Object;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Item item)
        {
            InnerObject = item.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Item temp = InnerObject as MSB_AC6.Part.Item;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.NPCWander npcWander)
        {
            InnerObject = npcWander.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.NPCWander temp = InnerObject as MSB_AC6.Part.NPCWander;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Protoboss protoboss)
        {
            InnerObject = protoboss.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Protoboss temp = InnerObject as MSB_AC6.Part.Protoboss;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Navmesh navmesh)
        {
            InnerObject = navmesh.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Navmesh temp = InnerObject as MSB_AC6.Part.Navmesh;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Part.Invalid invalid)
        {
            InnerObject = invalid.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSB_AC6.Part.Invalid temp = InnerObject as MSB_AC6.Part.Invalid;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }

        // Regions
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.EntryPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.EntryPoint temp = InnerObject as MSB_AC6.Region.EntryPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.EnvMapPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.EnvMapPoint temp = InnerObject as MSB_AC6.Region.EnvMapPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Sound region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Sound temp = InnerObject as MSB_AC6.Region.Sound;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;
            Array.Clear(temp.ChildRegionNames);
            Array.Clear(temp.ChildRegionIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.SFX region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.SFX temp = InnerObject as MSB_AC6.Region.SFX;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.WindSFX region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.WindSFX temp = InnerObject as MSB_AC6.Region.WindSFX;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;
            temp.WindAreaName = "";
            temp.WindAreaIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.EnvMapEffectBox region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.EnvMapEffectBox temp = InnerObject as MSB_AC6.Region.EnvMapEffectBox;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MufflingBox region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MufflingBox temp = InnerObject as MSB_AC6.Region.MufflingBox;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MufflingPortal region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MufflingPortal temp = InnerObject as MSB_AC6.Region.MufflingPortal;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.SoundOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.SoundOverride temp = InnerObject as MSB_AC6.Region.SoundOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Patrol region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Patrol temp = InnerObject as MSB_AC6.Region.Patrol;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.FeMapDisplay region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.FeMapDisplay temp = InnerObject as MSB_AC6.Region.FeMapDisplay;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.OperationalArea region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.OperationalArea temp = InnerObject as MSB_AC6.Region.OperationalArea;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.AiTarget region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.AiTarget temp = InnerObject as MSB_AC6.Region.AiTarget;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.WwiseEnvironmentSound region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.WwiseEnvironmentSound temp = InnerObject as MSB_AC6.Region.WwiseEnvironmentSound;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.CharacterFollowing region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.CharacterFollowing temp = InnerObject as MSB_AC6.Region.CharacterFollowing;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.NavmeshCostControl region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.NavmeshCostControl temp = InnerObject as MSB_AC6.Region.NavmeshCostControl;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.ArenaControl region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.ArenaControl temp = InnerObject as MSB_AC6.Region.ArenaControl;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.ArenaAppearance region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.ArenaAppearance temp = InnerObject as MSB_AC6.Region.ArenaAppearance;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.GarageCamera region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.GarageCamera temp = InnerObject as MSB_AC6.Region.GarageCamera;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.CutscenePlayback region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.CutscenePlayback temp = InnerObject as MSB_AC6.Region.CutscenePlayback;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.FallPreventionWallRemoval region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.FallPreventionWallRemoval temp = InnerObject as MSB_AC6.Region.FallPreventionWallRemoval;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.BigJump region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.BigJump temp = InnerObject as MSB_AC6.Region.BigJump;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Other region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Other temp = InnerObject as MSB_AC6.Region.Other;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.None region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.None temp = InnerObject as MSB_AC6.Region.None;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_3 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_3 temp = InnerObject as MSB_AC6.Region.Unknown_3;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_7 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_7 temp = InnerObject as MSB_AC6.Region.Unknown_7;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.ReturnPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.ReturnPoint temp = InnerObject as MSB_AC6.Region.ReturnPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Message region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Message temp = InnerObject as MSB_AC6.Region.Message;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_10 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_10 temp = InnerObject as MSB_AC6.Region.Unknown_10;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_11 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_11 temp = InnerObject as MSB_AC6.Region.Unknown_11;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_12 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_12 temp = InnerObject as MSB_AC6.Region.Unknown_12;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.FallReturnPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.FallReturnPoint temp = InnerObject as MSB_AC6.Region.FallReturnPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_14 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_14 temp = InnerObject as MSB_AC6.Region.Unknown_14;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_15 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_15 temp = InnerObject as MSB_AC6.Region.Unknown_15;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_16 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_16 temp = InnerObject as MSB_AC6.Region.Unknown_16;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.WindPlacement region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.WindPlacement temp = InnerObject as MSB_AC6.Region.WindPlacement;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_19 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_19 temp = InnerObject as MSB_AC6.Region.Unknown_19;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Unknown_20 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Unknown_20 temp = InnerObject as MSB_AC6.Region.Unknown_20;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Connection region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Connection temp = InnerObject as MSB_AC6.Region.Connection;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.SourceWaypoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.SourceWaypoint temp = InnerObject as MSB_AC6.Region.SourceWaypoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.StaticWaypoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.StaticWaypoint temp = InnerObject as MSB_AC6.Region.StaticWaypoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MapGridLayerConnection region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MapGridLayerConnection temp = InnerObject as MSB_AC6.Region.MapGridLayerConnection;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.EnemySpawnPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.EnemySpawnPoint temp = InnerObject as MSB_AC6.Region.EnemySpawnPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.BuddySummonPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.BuddySummonPoint temp = InnerObject as MSB_AC6.Region.BuddySummonPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.RollingAssetGeneration region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.RollingAssetGeneration temp = InnerObject as MSB_AC6.Region.RollingAssetGeneration;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MufflingPlane region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MufflingPlane temp = InnerObject as MSB_AC6.Region.MufflingPlane;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.ElectroMagneticStorm region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.ElectroMagneticStorm temp = InnerObject as MSB_AC6.Region.ElectroMagneticStorm;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.AiInformationSharing region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.AiInformationSharing temp = InnerObject as MSB_AC6.Region.AiInformationSharing;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.WaveSimulation region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.WaveSimulation temp = InnerObject as MSB_AC6.Region.WaveSimulation;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.Cover region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.Cover temp = InnerObject as MSB_AC6.Region.Cover;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MissionPlacement region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MissionPlacement temp = InnerObject as MSB_AC6.Region.MissionPlacement;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.NaviVolumeResolution region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.NaviVolumeResolution temp = InnerObject as MSB_AC6.Region.NaviVolumeResolution;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.MiniArea region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.MiniArea temp = InnerObject as MSB_AC6.Region.MiniArea;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.ConnectionBorder region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.ConnectionBorder temp = InnerObject as MSB_AC6.Region.ConnectionBorder;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.NaviGeneration region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.NaviGeneration temp = InnerObject as MSB_AC6.Region.NaviGeneration;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.TopdownView region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.TopdownView temp = InnerObject as MSB_AC6.Region.TopdownView;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.NaviCvCancel region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.NaviCvCancel temp = InnerObject as MSB_AC6.Region.NaviCvCancel;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Region.JumpEdgeRestriction region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSB_AC6.Region.JumpEdgeRestriction temp = InnerObject as MSB_AC6.Region.JumpEdgeRestriction;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }

        // Events
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Treasure mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Treasure temp = InnerObject as MSB_AC6.Event.Treasure;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.TreasurePartName = "";
            temp.TreasurePartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Generator mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Generator temp = InnerObject as MSB_AC6.Event.Generator;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            Array.Clear(temp.SpawnRegionNames);
            Array.Clear(temp.SpawnRegionIndices);
            Array.Clear(temp.SpawnPartNames);
            Array.Clear(temp.SpawnPartIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.ObjAct mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.ObjAct temp = InnerObject as MSB_AC6.Event.ObjAct;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Navmesh mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Navmesh temp = InnerObject as MSB_AC6.Event.Navmesh;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.PlatoonInfo mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.PlatoonInfo temp = InnerObject as MSB_AC6.Event.PlatoonInfo;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            Array.Clear(temp.GroupPartsNames);
            Array.Clear(temp.GroupPartsIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.PatrolInfo mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.PatrolInfo temp = InnerObject as MSB_AC6.Event.PatrolInfo;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            Array.Clear(temp.WalkPointNames);
            Array.Clear(temp.WalkPointIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.MapGimmick mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.MapGimmick temp = InnerObject as MSB_AC6.Event.MapGimmick;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Light mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Light temp = InnerObject as MSB_AC6.Event.Light;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Sound mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Sound temp = InnerObject as MSB_AC6.Event.Sound;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Sfx mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Sfx temp = InnerObject as MSB_AC6.Event.Sfx;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.MapWindSfx mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.MapWindSfx temp = InnerObject as MSB_AC6.Event.MapWindSfx;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Message mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Message temp = InnerObject as MSB_AC6.Event.Message;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.ReturnPoint mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.ReturnPoint temp = InnerObject as MSB_AC6.Event.ReturnPoint;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Unknown_11 mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Unknown_11 temp = InnerObject as MSB_AC6.Event.Unknown_11;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.NpcEntryPoint mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.NpcEntryPoint temp = InnerObject as MSB_AC6.Event.NpcEntryPoint;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.WindSfx mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.WindSfx temp = InnerObject as MSB_AC6.Event.WindSfx;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Unknown_16 mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Unknown_16 temp = InnerObject as MSB_AC6.Event.Unknown_16;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Unknown_17 mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Unknown_17 temp = InnerObject as MSB_AC6.Event.Unknown_17;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Unknown_18 mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Unknown_18 temp = InnerObject as MSB_AC6.Event.Unknown_18;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Unknown_19 mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Unknown_19 temp = InnerObject as MSB_AC6.Event.Unknown_19;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.PatrolRoute mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.PatrolRoute temp = InnerObject as MSB_AC6.Event.PatrolRoute;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Riding mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Riding temp = InnerObject as MSB_AC6.Event.Riding;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.StrategyRoute mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.StrategyRoute temp = InnerObject as MSB_AC6.Event.StrategyRoute;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.PatrolRoutePermanent mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.PatrolRoutePermanent temp = InnerObject as MSB_AC6.Event.PatrolRoutePermanent;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.MapOffset mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.MapOffset temp = InnerObject as MSB_AC6.Event.MapOffset;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_AC6(Prefab_AC6 parent, MSB_AC6.Event.Other mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSB_AC6.Event.Other temp = InnerObject as MSB_AC6.Event.Other;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }

        public void AddNamePrefix(string prefix)
        {
            var prop = InnerObject.GetType().GetProperty("Name");
            if (prop == null)
            {
                throw new InvalidDataException($"Prefab_AC6 operation failed, {InnerObject.GetType()} does not contain Name property.");
            }
            var name = prop.GetValue(InnerObject);
            name = $"{prefix}{Parent.PrefixSeparator}{name}";
            prop.SetValue(InnerObject, name);
        }

        public void StripNamePrefix()
        {
            var prop = InnerObject.GetType().GetProperty("Name");
            if (prop == null)
            {
                throw new InvalidDataException($"Prefab_AC6 operation failed, {InnerObject.GetType()} does not contain Name property.");
            }
            string name = (string)prop.GetValue(InnerObject);
            try
            {
                name = name.Split(Parent.PrefixSeparator)[1];
            }
            catch
            { }
            prop.SetValue(InnerObject, name);
        }
    }

    public List<MsbEntity> GenerateMapEntities(MapContainer targetMap)
    {
        List<MsbEntity> ents = new();
        foreach (var assetInfo in PrefabInfoChildren)
        {
            // Notes for grouped prefabs/scene tree support:
            // * Problem: to retain this information in MSB upon saving/loading, something will need to be saved somewhere. Maybe a meta file?
            // * Make a map entity of the prefab
            // * Add that to ents list
            // * Make the asset objects children of that
            // * Modify scenetree to handle AssetPrefabs.

            MsbEntity ent = new(targetMap, assetInfo.InnerObject);
            switch (assetInfo.DataType)
            {
                case PrefabInfo_AC6.AssetInfoDataType.Part:
                    ent.Type = MsbEntity.MsbEntityType.Part;
                    break;
                case PrefabInfo_AC6.AssetInfoDataType.Region:
                    ent.Type = MsbEntity.MsbEntityType.Region;
                    break;
                case PrefabInfo_AC6.AssetInfoDataType.Event:
                    ent.Type = MsbEntity.MsbEntityType.Event;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported AssetInfoDataType {assetInfo.DataType}");
            }
            ents.Add(ent);

            MsbEntityChildren.Add(ent);
        }
        return ents;
    }

    /// <summary>
    /// Exports AssetPrefab to json file.
    /// </summary>
    /// <returns>True if successful, false otherwise.</returns>
    public bool Write(string path)
    {
        try
        {
            MSB_AC6 map = new();
            foreach (var assetInfo in PrefabInfoChildren)
            {
                assetInfo.StripNamePrefix();

                // Parts
                if (assetInfo.InnerObject is MSB_AC6.Part.Enemy enemy)
                {
                    map.Parts.Enemies.Add(enemy);
                    MSB_AC6.Model.Enemy model = new();
                    model.Name = enemy.ModelName;
                    map.Models.Enemies.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.MapPiece mapPiece)
                {
                    map.Parts.MapPieces.Add(mapPiece);
                    MSB_AC6.Model.MapPiece model = new();
                    model.Name = mapPiece.ModelName;
                    map.Models.MapPieces.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Player player)
                {
                    map.Parts.Players.Add(player);
                    MSB_AC6.Model.Player model = new();
                    model.Name = player.ModelName;
                    map.Models.Players.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Collision collision)
                {
                    map.Parts.Collisions.Add(collision);
                    MSB_AC6.Model.Collision model = new();
                    model.Name = collision.ModelName;
                    map.Models.Collisions.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.DummyAsset dummyAsset)
                {
                    map.Parts.DummyAssets.Add(dummyAsset);
                    MSB_AC6.Model.Asset model = new();
                    model.Name = dummyAsset.ModelName;
                    map.Models.Assets.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.DummyEnemy dummyEnemy)
                {
                    map.Parts.DummyEnemies.Add(dummyEnemy);
                    MSB_AC6.Model.Enemy model = new();
                    model.Name = dummyEnemy.ModelName;
                    map.Models.Enemies.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.ConnectCollision connectCol)
                {
                    map.Parts.ConnectCollisions.Add(connectCol);
                    MSB_AC6.Model.Collision model = new();
                    model.Name = connectCol.ModelName;
                    map.Models.Collisions.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Asset asset)
                {
                    map.Parts.Assets.Add(asset);
                    MSB_AC6.Model.Asset model = new();
                    model.Name = asset.ModelName;
                    map.Models.Assets.Add(model);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Object obj)
                {
                    map.Parts.Objects.Add(obj);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Item item)
                {
                    map.Parts.Items.Add(item);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.NPCWander npcWander)
                {
                    map.Parts.NPCWanders.Add(npcWander);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Protoboss protoboss)
                {
                    map.Parts.Protobosses.Add(protoboss);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Navmesh navmesh)
                {
                    map.Parts.Navmeshes.Add(navmesh);
                }
                if (assetInfo.InnerObject is MSB_AC6.Part.Invalid invalid)
                {
                    map.Parts.Invalids.Add(invalid);
                }

                // Regions
                if (assetInfo.InnerObject is MSB_AC6.Region.EntryPoint entryPoint)
                {
                    map.Regions.EntryPoints.Add(entryPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.EnvMapPoint envMapPoint)
                {
                    map.Regions.EnvMapPoints.Add(envMapPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Sound sound)
                {
                    map.Regions.Sounds.Add(sound);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.SFX sfx)
                {
                    map.Regions.SFX.Add(sfx);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.WindSFX windSfx)
                {
                    map.Regions.WindSFX.Add(windSfx);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.EnvMapEffectBox envMapEffectBox)
                {
                    map.Regions.EnvMapEffectBoxes.Add(envMapEffectBox);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MufflingBox mufflingBox)
                {
                    map.Regions.MufflingBoxes.Add(mufflingBox);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MufflingPortal mufflingPortal)
                {
                    map.Regions.MufflingPortals.Add(mufflingPortal);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.SoundOverride soundOverride)
                {
                    map.Regions.SoundOverrides.Add(soundOverride);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Patrol patrol)
                {
                    map.Regions.Patrols.Add(patrol);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.FeMapDisplay feMapDisplay)
                {
                    map.Regions.FeMapDisplays.Add(feMapDisplay);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.OperationalArea operationalArea)
                {
                    map.Regions.OperationalAreas.Add(operationalArea);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.AiTarget aiTarget)
                {
                    map.Regions.AiTargets.Add(aiTarget);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.WwiseEnvironmentSound wwiseEnvironmentSound)
                {
                    map.Regions.WwiseEnvironmentSounds.Add(wwiseEnvironmentSound);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.CharacterFollowing characterFollowing)
                {
                    map.Regions.CharacterFollowings.Add(characterFollowing);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.NavmeshCostControl navmeshCostControl)
                {
                    map.Regions.NavmeshCostControls.Add(navmeshCostControl);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.ArenaControl arenaControl)
                {
                    map.Regions.ArenaControls.Add(arenaControl);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.ArenaAppearance arenaApperance)
                {
                    map.Regions.ArenaAppearances.Add(arenaApperance);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.GarageCamera garageCamera)
                {
                    map.Regions.GarageCameras.Add(garageCamera);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.CutscenePlayback cutscenePlayback)
                {
                    map.Regions.CutscenePlaybacks.Add(cutscenePlayback);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.FallPreventionWallRemoval fallPreventionWallRemoval)
                {
                    map.Regions.FallPreventionWallRemovals.Add(fallPreventionWallRemoval);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.BigJump bigJump)
                {
                    map.Regions.BigJumps.Add(bigJump);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_3 unkRegion3)
                {
                    map.Regions.Unknown_3s.Add(unkRegion3);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_7 unkRegion7)
                {
                    map.Regions.Unknown_7s.Add(unkRegion7);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.ReturnPoint returnPoint)
                {
                    map.Regions.ReturnPoints.Add(returnPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Message message)
                {
                    map.Regions.Messages.Add(message);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_10 unkRegion10)
                {
                    map.Regions.Unknown_10s.Add(unkRegion10);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_11 unkRegion11)
                {
                    map.Regions.Unknown_11s.Add(unkRegion11);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_12 unkRegion12)
                {
                    map.Regions.Unknown_12s.Add(unkRegion12);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.FallReturnPoint fallReturnPoint)
                {
                    map.Regions.FallReturnPoints.Add(fallReturnPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_14 unkRegion14)
                {
                    map.Regions.Unknown_14s.Add(unkRegion14);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_15 unkRegion15)
                {
                    map.Regions.Unknown_15s.Add(unkRegion15);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_16 unkRegion16)
                {
                    map.Regions.Unknown_16s.Add(unkRegion16);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.WindPlacement windPlacement)
                {
                    map.Regions.WindPlacements.Add(windPlacement);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_19 unkRegion19)
                {
                    map.Regions.Unknown_19s.Add(unkRegion19);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Unknown_20 unkRegion20)
                {
                    map.Regions.Unknown_20s.Add(unkRegion20);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Connection connection)
                {
                    map.Regions.Connections.Add(connection);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.SourceWaypoint sourceWaypoint)
                {
                    map.Regions.SourceWaypoints.Add(sourceWaypoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.StaticWaypoint staticWaypoint)
                {
                    map.Regions.StaticWaypoints.Add(staticWaypoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MapGridLayerConnection mapGridLayerConnection)
                {
                    map.Regions.MapGridLayerConnections.Add(mapGridLayerConnection);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.EnemySpawnPoint enemySpawnPoint)
                {
                    map.Regions.EnemySpawnPoints.Add(enemySpawnPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.BuddySummonPoint buddySummonPoint)
                {
                    map.Regions.BuddySummonPoints.Add(buddySummonPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.RollingAssetGeneration rollingAssetGeneration)
                {
                    map.Regions.RollingAssetGenerations.Add(rollingAssetGeneration);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MufflingPlane mufflingPlane)
                {
                    map.Regions.MufflingPlanes.Add(mufflingPlane);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.ElectroMagneticStorm electroMagneticStorm)
                {
                    map.Regions.ElectroMagneticStorms.Add(electroMagneticStorm);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.AiInformationSharing aiInfoSharing)
                {
                    map.Regions.AiInformationSharings.Add(aiInfoSharing);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.WaveSimulation waveSim)
                {
                    map.Regions.WaveSimulations.Add(waveSim);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Cover cover)
                {
                    map.Regions.Covers.Add(cover);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MissionPlacement missionPlacement)
                {
                    map.Regions.MissionPlacements.Add(missionPlacement);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.NaviVolumeResolution naviVolumeResolution)
                {
                    map.Regions.NaviVolumeResolutions.Add(naviVolumeResolution);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.MiniArea miniArea)
                {
                    map.Regions.MiniAreas.Add(miniArea);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.ConnectionBorder connectionBorder)
                {
                    map.Regions.ConnectionBorders.Add(connectionBorder);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.NaviGeneration naviGeneration)
                {
                    map.Regions.NaviGenerations.Add(naviGeneration);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.TopdownView topdownView)
                {
                    map.Regions.TopdownViews.Add(topdownView);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.NaviCvCancel naviCvCancel)
                {
                    map.Regions.NaviCvCancels.Add(naviCvCancel);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.JumpEdgeRestriction jumpEdgeRestriction)
                {
                    map.Regions.JumpEdgeRestrictions.Add(jumpEdgeRestriction);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.None none)
                {
                    map.Regions.Nones.Add(none);
                }
                if (assetInfo.InnerObject is MSB_AC6.Region.Other region)
                {
                    map.Regions.Others.Add(region);
                }

                // Events
                if (assetInfo.InnerObject is MSB_AC6.Event.Treasure treasure)
                {
                    map.Events.Treasures.Add(treasure);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Generator generator)
                {
                    map.Events.Generators.Add(generator);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.MapOffset mapOffset)
                {
                    map.Events.MapOffsets.Add(mapOffset);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.PlatoonInfo platoonInfo)
                {
                    map.Events.PlatoonInfo.Add(platoonInfo);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.PatrolInfo patrolInfo)
                {
                    map.Events.PatrolInfo.Add(patrolInfo);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.MapGimmick mapGimmick)
                {
                    map.Events.MapGimmicks.Add(mapGimmick);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Light lightEvent)
                {
                    map.Events.Lights.Add(lightEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Sound soundEvent)
                {
                    map.Events.Sounds.Add(soundEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Sfx sfxEvent)
                {
                    map.Events.Sfxs.Add(sfxEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.MapWindSfx mapWindSfx)
                {
                    map.Events.MapWindSfxs.Add(mapWindSfx);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Message messageEvent)
                {
                    map.Events.Messages.Add(messageEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.ObjAct objAct)
                {
                    map.Events.ObjActs.Add(objAct);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.ReturnPoint returnPointEvent)
                {
                    map.Events.ReturnPoints.Add(returnPointEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Navmesh navmeshEvent)
                {
                    map.Events.Navmeshes.Add(navmeshEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Unknown_11 unkEvent11)
                {
                    map.Events.Unknown_11s.Add(unkEvent11);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.NpcEntryPoint npcEntryPoint)
                {
                    map.Events.NpcEntryPoints.Add(npcEntryPoint);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.WindSfx windSfxEvent)
                {
                    map.Events.WindSfxs.Add(windSfxEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Unknown_16 unkEvent16)
                {
                    map.Events.Unknown_16s.Add(unkEvent16);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Unknown_17 unkEvent17)
                {
                    map.Events.Unknown_17s.Add(unkEvent17);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Unknown_18 unkEvent18)
                {
                    map.Events.Unknown_18s.Add(unkEvent18);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Unknown_19 unkEvent19)
                {
                    map.Events.Unknown_19s.Add(unkEvent19);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.PatrolRoute patrolRouteEvent)
                {
                    map.Events.PatrolRoutes.Add(patrolRouteEvent);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Riding riding)
                {
                    map.Events.Ridings.Add(riding);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.StrategyRoute strategyRoute)
                {
                    map.Events.StrategyRoutes.Add(strategyRoute);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.PatrolRoutePermanent patrolRoutePerm)
                {
                    map.Events.PatrolRoutePermanents.Add(patrolRoutePerm);
                }
                if (assetInfo.InnerObject is MSB_AC6.Event.Other other)
                {
                    map.Events.Others.Add(other);
                }
            }

            AssetContainerBytes = map.Write();

            string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox(
                $"Unable to export Asset Prefab due to the following error:\n\n{e.Message}\n{e.StackTrace}",
                "Asset Prefab export error",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
    }

    /// <summary>
    /// Imports AssetPrefab info from json file.
    /// </summary>
    /// <returns>Asset Prefab if successful, null otherwise.</returns>
    public static Prefab_AC6 ImportJson(string path)
    {
        try
        {
            var settings = new JsonSerializerSettings();
            Prefab_AC6 prefab = JsonConvert.DeserializeObject<Prefab_AC6>(File.ReadAllText(path), settings);

            MSB_AC6 pseudoMap = MSB_AC6.Read(prefab.AssetContainerBytes);

            // Parts
            foreach (var mapPiece in pseudoMap.Parts.MapPieces)
            {
                PrefabInfo_AC6 info = new(prefab, mapPiece);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var enemy in pseudoMap.Parts.Enemies)
            {
                PrefabInfo_AC6 info = new(prefab, enemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var player in pseudoMap.Parts.Players)
            {
                PrefabInfo_AC6 info = new(prefab, player);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var col in pseudoMap.Parts.Collisions)
            {
                PrefabInfo_AC6 info = new(prefab, col);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var dummyAsset in pseudoMap.Parts.DummyAssets)
            {
                PrefabInfo_AC6 info = new(prefab, dummyAsset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
            {
                PrefabInfo_AC6 info = new(prefab, dummyEnemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
            {
                PrefabInfo_AC6 info = new(prefab, connectCol);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Assets)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Objects)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Items)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.NPCWanders)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Protobosses)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Navmeshes)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Invalids)
            {
                PrefabInfo_AC6 info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            // Regions
            foreach (var region in pseudoMap.Regions.EntryPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.EnvMapPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Sounds)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.SFX)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.WindSFX)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.EnvMapEffectBoxes)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MufflingBoxes)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MufflingPortals)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.SoundOverrides)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Patrols)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.FeMapDisplays)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.OperationalAreas)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.AiTargets)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.WwiseEnvironmentSounds)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.CharacterFollowings)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.NavmeshCostControls)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.ArenaControls)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.ArenaAppearances)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.GarageCameras)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.CutscenePlaybacks)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.FallPreventionWallRemovals)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.BigJumps)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_3s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_7s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.ReturnPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Messages)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_10s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_11s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_12s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.FallReturnPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_14s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_15s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_16s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.WindPlacements)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_19s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Unknown_20s)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Connections)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.SourceWaypoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.StaticWaypoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MapGridLayerConnections)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.EnemySpawnPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.BuddySummonPoints)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.RollingAssetGenerations)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MufflingPlanes)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.ElectroMagneticStorms)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.AiInformationSharings)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.WaveSimulations)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Covers)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MissionPlacements)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.NaviVolumeResolutions)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.MiniAreas)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.ConnectionBorders)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.NaviGenerations)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.TopdownViews)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.NaviCvCancels)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.JumpEdgeRestrictions)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Others)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Nones)
            {
                PrefabInfo_AC6 info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            // Events
            foreach (var evt in pseudoMap.Events.Treasures)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Generators)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.MapOffsets)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.PlatoonInfo)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.PatrolInfo)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.MapGimmicks)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Others)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Lights)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Sounds)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Sfxs)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.MapWindSfxs)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Messages)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.ObjActs)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.ReturnPoints)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Navmeshes)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Unknown_11s)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.NpcEntryPoints)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.WindSfxs)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Unknown_16s)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Unknown_17s)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Unknown_18s)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Unknown_19s)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.PatrolRoutes)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.Ridings)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.StrategyRoutes)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var evt in pseudoMap.Events.PatrolRoutePermanents)
            {
                PrefabInfo_AC6 info = new(prefab, evt);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            return prefab;
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox(
                $"Unable to import Prefab_AC6 due to the following error:" +
                $"\n\n{e.Message}"
                , "Asset prefab import error"
                , MessageBoxButtons.OK, MessageBoxIcon.Information);

            return null;
        }
    }

    /// <summary>
    /// Import
    /// </summary>
    /// <param name="info"></param>
    /// <param name="_comboTargetMap"></param>
    /// <param name="_universe"></param>
    /// <param name="_scene"></param>
    /// <param name="_actionManager"></param>
    public static void ImportSelectedPrefab(PrefabInfo info, (string, ObjectContainer) _comboTargetMap, Universe _universe, RenderScene _scene, ViewportActionManager _actionManager)
    {
        Prefab_AC6 _selectedAssetPrefab;

        _selectedAssetPrefab = Prefab_AC6.ImportJson(info.Path);
        MapContainer targetMap = (MapContainer)_comboTargetMap.Item2;

        if (targetMap != null)
        {
            if (_selectedAssetPrefab != null)
            {
                var parent = targetMap.RootObject;
                List<MsbEntity> ents = _selectedAssetPrefab.GenerateMapEntities(targetMap);

                AddMapObjectsAction act = new(_universe, targetMap, _scene, ents, true, parent, targetMap);
                _actionManager.ExecuteAction(act);
                _selectedAssetPrefab = null;
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("Import failed, no map has been selected.", "Prefab Error", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Export
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="_selection"></param>
    public static void ExportSelection(string filepath, ViewportSelection _selection, string tags)
    {
        List<string> tagList = new List<string>();
        if (tags.Contains(","))
        {
            tagList = tags.Split(",").ToList();
        }
        else
        {
            tagList.Add(tags);
        }

        Prefab_AC6 prefab = new(_selection.GetFilteredSelection<MsbEntity>());

        if (!prefab.PrefabInfoChildren.Any())
        {
            PlatformUtils.Instance.MessageBox("Export failed, nothing in selection could be exported.", "Prefab Error", MessageBoxButtons.OK);
        }
        else
        {
            prefab.PrefabName = System.IO.Path.GetFileNameWithoutExtension(filepath);
            prefab.TagList = tagList;
            prefab.Write(filepath);
        }
    }

    public static List<string> GetSelectedPrefabObjects(PrefabInfo info, (string, ObjectContainer) _comboTargetMap)
    {
        List<string> entNames = new List<string>();
        Prefab_AC6 _selectedAssetPrefab;

        _selectedAssetPrefab = Prefab_AC6.ImportJson(info.Path);
        MapContainer targetMap = (MapContainer)_comboTargetMap.Item2;

        if (targetMap != null)
        {
            if (_selectedAssetPrefab != null)
            {
                List<MsbEntity> ents = _selectedAssetPrefab.GenerateMapEntities(targetMap);
                int index = 1;
                foreach (var ent in ents)
                {
                    string name = $"{index}";

                    // For parts only, replace Name with ModelName + Alias name
                    if (ent.IsPart())
                    {
                        name = BuildModelAliasName(ent);
                    }

                    name = PrependEntityType(ent, name);
                    entNames.Add(name);
                    index++;
                }
            }
        }
        else
        {
            // Fail silently
        }

        return entNames;
    }

    public static string BuildModelAliasName(MsbEntity ent)
    {
        string fullname = "";

        PropertyInfo prop = ent.WrappedObject.GetType().GetProperty("ModelName");
        string modelName = prop.GetValue(ent.WrappedObject) as string;
        fullname = $"{modelName} <>";

        if (ent.WrappedObject is MSB_AC6.Part.MapPiece mapPiece)
        {
            foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        if (ent.WrappedObject is MSB_AC6.Part.Enemy enemy || ent.WrappedObject is MSB_AC6.Part.DummyEnemy dummyEnemy)
        {
            foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        if (ent.WrappedObject is MSB_AC6.Part.Asset asset || ent.WrappedObject is MSB_AC6.Part.DummyAsset dummyAsset)
        {
            foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Objects"))
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        return fullname;
    }

    public static string PrependEntityType(MsbEntity ent, string existingName)
    {
        string name = existingName;

        // Parts
        if (ent.WrappedObject is MSB_AC6.Part.MapPiece mapPiece)
        {
            name = $"Map Piece: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Enemy enemy)
        {
            name = $"Enemy: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Player player)
        {
            name = $"Player: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Collision col)
        {
            name = $"Collision: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.DummyAsset dummyAsset)
        {
            name = $"Dummy Asset: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.DummyEnemy dummyEnemy)
        {
            name = $"Dummy Enemy: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.ConnectCollision connectCol)
        {
            name = $"Connect Collision: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Asset asset)
        {
            name = $"Asset: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Object obj)
        {
            name = $"Object: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Item item)
        {
            name = $"Item: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.NPCWander npcWander)
        {
            name = $"NPCWander: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Protoboss protoboss)
        {
            name = $"Protoboss: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Navmesh navmesh)
        {
            name = $"Navmesh (Part): {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Part.Invalid invalid)
        {
            name = $"Invalid: {name}";
        }

        // Regions
        if (ent.WrappedObject is MSB_AC6.Region.EntryPoint entryPoint)
        {
            name = $"Entry Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.EnvMapPoint envMapPoint)
        {
            name = $"Environment Map Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Sound sound)
        {
            name = $"Sound: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.SFX sfx)
        {
            name = $"SFX: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.WindSFX windSfx)
        {
            name = $"Wind SFX: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.EnvMapEffectBox envMapEffectBox)
        {
            name = $"Environment Map Effect Box: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MufflingBox mufflingBox)
        {
            name = $"Muffling Box: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MufflingPortal mufflingPortal)
        {
            name = $"Muffling Portal: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.SoundOverride soundOverride)
        {
            name = $"Sound Override: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Patrol patrol)
        {
            name = $"Patrol: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.FeMapDisplay feMapDisplay)
        {
            name = $"Map Display: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.OperationalArea operationalArea)
        {
            name = $"Operational Area: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.AiTarget aiTarget)
        {
            name = $"AI Target: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.WwiseEnvironmentSound wwiseEnvironmentSound)
        {
            name = $"Environment Sound: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.CharacterFollowing chrFollow)
        {
            name = $"Character Following: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.NavmeshCostControl navmeshCostControl)
        {
            name = $"Navmesh Cost Control: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.ArenaControl arenaControl)
        {
            name = $"Arena Control: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.ArenaAppearance arenaAppearance)
        {
            name = $"Arena Appearance: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.GarageCamera garageCamera)
        {
            name = $"Garage Camera: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.CutscenePlayback cutscenePlayback)
        {
            name = $"Cutscene Playback: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.FallPreventionWallRemoval fallPreventionWallRemoval)
        {
            name = $"Fall Prevention Wall Removal: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.BigJump bigJump)
        {
            name = $"Big Jump: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_3 unkRegion3)
        {
            name = $"Unknown Region 3: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_7 unkRegion7)
        {
            name = $"Unknown Region 7: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Message message)
        {
            name = $"Message: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_10 unkRegion10)
        {
            name = $"Unknown Region 10: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_11 unkRegion11)
        {
            name = $"Unknown Region 11: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_12 unkRegion12)
        {
            name = $"Unknown Region 12: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.FallReturnPoint fallReturnPoint)
        {
            name = $"Fall Return Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_14 unkRegion14)
        {
            name = $"Unknown Region 14: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_15 unkRegion15)
        {
            name = $"Unknown Region 15: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_16 unkRegion16)
        {
            name = $"Unknown Region 16: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.WindPlacement windPlacement)
        {
            name = $"Wind Placement: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_19 unkRegion19)
        {
            name = $"Unknown Region 19: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Unknown_20 unkRegion20)
        {
            name = $"Unknown Region 20: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Connection connection)
        {
            name = $"Connection: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.SourceWaypoint sourceWaypoint)
        {
            name = $"Source Waypoint: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.StaticWaypoint staticWaypoint)
        {
            name = $"Static Waypoint: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MapGridLayerConnection mapGridLayerConnection)
        {
            name = $"Map Grid Layer Connection: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.EnemySpawnPoint enemySpawnPoint)
        {
            name = $"Enemy Spawn Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.BuddySummonPoint buddySummonPoint)
        {
            name = $"Buddy Summon Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.RollingAssetGeneration rollingAssetGeneration)
        {
            name = $"Rolling Asset Generation: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MufflingPlane mufflingPlane)
        {
            name = $"Muffling Plane: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.ElectroMagneticStorm electroMagneticStorm)
        {
            name = $"Electro Magnetic Storm: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.AiInformationSharing aiInformationSharing)
        {
            name = $"AI Information Sharing: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.WaveSimulation waveSimulation)
        {
            name = $"Wave Simulation: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Cover cover)
        {
            name = $"Cover: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MissionPlacement missionPlacement)
        {
            name = $"Mission Placement: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.NaviVolumeResolution naviVolumeResolution)
        {
            name = $"Navigation Volume Resolution: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.MiniArea miniArea)
        {
            name = $"Mini Area: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.ConnectionBorder connectionBorder)
        {
            name = $"Connection Border: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.NaviGeneration naviGeneration)
        {
            name = $"Navigation Generation: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.TopdownView topdownView)
        {
            name = $"Topdown View: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.NaviCvCancel naviCvCancel)
        {
            name = $"Navigation Cover Cancel: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.JumpEdgeRestriction jumpEdgeRestriction)
        {
            name = $"Jump Edge Restriction: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.Other region)
        {
            name = $"Other: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Region.None none)
        {
            name = $"None: {name}";
        }

        // Events
        if (ent.WrappedObject is MSB_AC6.Event.Treasure treasure)
        {
            name = $"Treasure: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Generator generator)
        {
            name = $"Generator: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.MapOffset mapOffset)
        {
            name = $"Map Offset: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.PlatoonInfo platoonInfo)
        {
            name = $"Platoon Info: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.PatrolInfo patrolInfo)
        {
            name = $"Patrol Info: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.MapGimmick mapGimmick)
        {
            name = $"Map Gimmick: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Light lightEvent)
        {
            name = $"Light: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Sound soundEvent)
        {
            name = $"Sound: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Sfx sfxEvent)
        {
            name = $"SFX: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.MapWindSfx mapWindSfxEvent)
        {
            name = $"Map Wind SFX: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Message messageEvent)
        {
            name = $"Message: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.ObjAct objActEvent)
        {
            name = $"ObjAct: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.ReturnPoint returnPointEvent)
        {
            name = $"Return Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Navmesh navmeshEvent)
        {
            name = $"Navmesh (Event): {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Unknown_11 unkEvent11)
        {
            name = $"Unknown Event 11: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.NpcEntryPoint npcEntryPoint)
        {
            name = $"NPC Entry Point: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.WindSfx windSfxEvent)
        {
            name = $"Wind SFX: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Unknown_16 unkEvent16)
        {
            name = $"Unknown Event 16: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Unknown_17 unkEvent17)
        {
            name = $"Unknown Event 17: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Unknown_18 unkEvent18)
        {
            name = $"Unknown Event 18: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Unknown_19 unkEvent19)
        {
            name = $"Unknown Event 19: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.PatrolRoute patrolRouteEvent)
        {
            name = $"Patrol Route: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Riding riding)
        {
            name = $"Riding: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.StrategyRoute strategyRoute)
        {
            name = $"Strategy Route: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.PatrolRoutePermanent patrolRoutePermanent)
        {
            name = $"Patrol Route Permanent: {name}";
        }
        if (ent.WrappedObject is MSB_AC6.Event.Other other)
        {
            name = $"Other: {name}";
        }

        return name;
    }
}