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

namespace StudioCore.Editors.MapEditor.Prefabs;

public class Prefab_ER
{
    public string PrefabName = "";
    public string PrefixSeparator = "[]";
    public ProjectType Type = ProjectType.ER;

    public List<string> TagList { get; set; }

    /// <summary>
    /// Bytes of the MSB that stores prefab data.
    /// </summary>
    public byte[] AssetContainerBytes { get; set; }

    /// <summary>
    /// List of AssetInfo derived from MSB AssetContainerBytes.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<PrefabInfo_ER> PrefabInfoChildren = new();

    /// <summary>
    /// List of Msb Entities derived from AssetInfoChildren.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<MsbEntity> MsbEntityChildren = new();


    // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
    [Newtonsoft.Json.JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;

    public Prefab_ER()
    { }

    public Prefab_ER(HashSet<MsbEntity> entities)
    {
        foreach (var ent in entities)
        {
            // Parts
            if (ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mapPiece));
            }
            if (ent.WrappedObject is MSBE.Part.Enemy enemy)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, enemy));
            }
            if (ent.WrappedObject is MSBE.Part.Player player)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, player));
            }
            if (ent.WrappedObject is MSBE.Part.Collision col)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, col));
            }
            if (ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, dummyAsset));
            }
            if (ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, dummyEnemy));
            }
            if (ent.WrappedObject is MSBE.Part.ConnectCollision connectCol)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, connectCol));
            }
            if (ent.WrappedObject is MSBE.Part.Asset asset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, asset));
            }

            // Regions
            if (ent.WrappedObject is MSBE.Region.InvasionPoint invasionPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, invasionPoint));
            }
            if (ent.WrappedObject is MSBE.Region.EnvironmentMapPoint envMapPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, envMapPoint));
            }
            if (ent.WrappedObject is MSBE.Region.Sound sound)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, sound));
            }
            if (ent.WrappedObject is MSBE.Region.SFX sfx)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, sfx));
            }
            if (ent.WrappedObject is MSBE.Region.WindSFX windSfx)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, windSfx));
            }
            if (ent.WrappedObject is MSBE.Region.SpawnPoint spawnPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, spawnPoint));
            }
            if (ent.WrappedObject is MSBE.Region.Message message)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, message));
            }
            if (ent.WrappedObject is MSBE.Region.EnvironmentMapEffectBox envMapEffectbox)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, envMapEffectbox));
            }
            if (ent.WrappedObject is MSBE.Region.WindArea windArea)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, windArea));
            }
            if (ent.WrappedObject is MSBE.Region.Connection connection)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, connection));
            }
            if (ent.WrappedObject is MSBE.Region.PatrolRoute22 patrolRoute22)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, patrolRoute22));
            }
            if (ent.WrappedObject is MSBE.Region.BuddySummonPoint buddySummonPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, buddySummonPoint));
            }
            if (ent.WrappedObject is MSBE.Region.MufflingBox mufflingBox)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mufflingBox));
            }
            if (ent.WrappedObject is MSBE.Region.MufflingPortal mufflingPortal)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mufflingPortal));
            }
            if (ent.WrappedObject is MSBE.Region.SoundRegion soundRegion)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, soundRegion));
            }
            if (ent.WrappedObject is MSBE.Region.MufflingPlane mufflingPlane)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mufflingPlane));
            }
            if (ent.WrappedObject is MSBE.Region.PatrolRoute patrolRoute)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, patrolRoute));
            }
            if (ent.WrappedObject is MSBE.Region.MapPoint mapPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mapPoint));
            }
            if (ent.WrappedObject is MSBE.Region.WeatherOverride weatherOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, weatherOverride));
            }
            if (ent.WrappedObject is MSBE.Region.AutoDrawGroupPoint autoDrawGroupPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, autoDrawGroupPoint));
            }
            if (ent.WrappedObject is MSBE.Region.GroupDefeatReward groupDefeatReward)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, groupDefeatReward));
            }
            if (ent.WrappedObject is MSBE.Region.MapPointDiscoveryOverride mapPointDiscoveryOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mapPointDiscoveryOverride));
            }
            if (ent.WrappedObject is MSBE.Region.MapPointParticipationOverride mapPointParticapationOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mapPointParticapationOverride));
            }
            if (ent.WrappedObject is MSBE.Region.Hitset hitset)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, hitset));
            }
            if (ent.WrappedObject is MSBE.Region.FastTravelRestriction fastTravelRestriction)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, fastTravelRestriction));
            }
            if (ent.WrappedObject is MSBE.Region.WeatherCreateAssetPoint weatherCreateAssetPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, weatherCreateAssetPoint));
            }
            if (ent.WrappedObject is MSBE.Region.PlayArea playArea)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, playArea));
            }
            if (ent.WrappedObject is MSBE.Region.EnvironmentMapOutput envMapOutput)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, envMapOutput));
            }
            if (ent.WrappedObject is MSBE.Region.MountJump mountJump)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mountJump));
            }
            if (ent.WrappedObject is MSBE.Region.Dummy dummy)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, dummy));
            }
            if (ent.WrappedObject is MSBE.Region.FallPreventionRemoval fallPreventionRemoval)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, fallPreventionRemoval));
            }
            if (ent.WrappedObject is MSBE.Region.NavmeshCutting navmeshCutting)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, navmeshCutting));
            }
            if (ent.WrappedObject is MSBE.Region.MapNameOverride mapNameOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mapNameOverride));
            }
            if (ent.WrappedObject is MSBE.Region.MountJumpFall mountJumpFall)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mountJumpFall));
            }
            if (ent.WrappedObject is MSBE.Region.HorseRideOverride horseRideOverride)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, horseRideOverride));
            }
            if (ent.WrappedObject is MSBE.Region.Other region)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, region));
            }

            // Events
            if (ent.WrappedObject is MSBE.Event.Treasure treasure)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, treasure));
            }
            if (ent.WrappedObject is MSBE.Event.Generator generator)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, generator));
            }
            if (ent.WrappedObject is MSBE.Event.ObjAct objAct)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, objAct));
            }
            if (ent.WrappedObject is MSBE.Event.Navmesh navmesh)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, navmesh));
            }
            if (ent.WrappedObject is MSBE.Event.PseudoMultiplayer pseudoMultiplayer)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, pseudoMultiplayer));
            }
            if (ent.WrappedObject is MSBE.Event.PlatoonInfo platoonInfo)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, platoonInfo));
            }
            if (ent.WrappedObject is MSBE.Event.PatrolInfo patrolInfo)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, patrolInfo));
            }
            if (ent.WrappedObject is MSBE.Event.Mount mount)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, mount));
            }
            if (ent.WrappedObject is MSBE.Event.SignPool signPool)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, signPool));
            }
            if (ent.WrappedObject is MSBE.Event.RetryPoint retryPoint)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, retryPoint));
            }
            if (ent.WrappedObject is MSBE.Event.Other other)
            {
                PrefabInfoChildren.Add(new PrefabInfo_ER(this, other));
            }
        }
    }
    public class PrefabInfo_ER
    {

        public Prefab_ER Parent;

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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.MapPiece mapPiece)
        {
            InnerObject = mapPiece.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.MapPiece temp = InnerObject as MSBE.Part.MapPiece;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.Enemy enemy)
        {
            InnerObject = enemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Enemy temp = InnerObject as MSBE.Part.Enemy;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.Player player)
        {
            InnerObject = player.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Player temp = InnerObject as MSBE.Part.Player;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.Collision collision)
        {
            InnerObject = collision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Collision temp = InnerObject as MSBE.Part.Collision;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.DummyAsset dummyAsset)
        {
            InnerObject = dummyAsset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.DummyAsset temp = InnerObject as MSBE.Part.DummyAsset;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                Array.Clear(temp.EntityGroupIDs);
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.DummyEnemy dummyEnemy)
        {
            InnerObject = dummyEnemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.DummyEnemy temp = InnerObject as MSBE.Part.DummyEnemy;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.ConnectCollision connectCollision)
        {
            InnerObject = connectCollision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.ConnectCollision temp = InnerObject as MSBE.Part.ConnectCollision;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Part.Asset asset)
        {
            InnerObject = asset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Asset temp = InnerObject as MSBE.Part.Asset;
            Array.Clear(temp.UnkPartNames);

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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.InvasionPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.InvasionPoint temp = InnerObject as MSBE.Region.InvasionPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.EnvironmentMapPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.EnvironmentMapPoint temp = InnerObject as MSBE.Region.EnvironmentMapPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Sound region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Sound temp = InnerObject as MSBE.Region.Sound;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;
            Array.Clear(temp.ChildRegionNames);
            Array.Clear(temp.ChildRegionIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.SFX region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.SFX temp = InnerObject as MSBE.Region.SFX;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.WindSFX region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.WindSFX temp = InnerObject as MSBE.Region.WindSFX;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;
            temp.WindAreaName = "";
            temp.WindAreaIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.SpawnPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.SpawnPoint temp = InnerObject as MSBE.Region.SpawnPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Message region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Message temp = InnerObject as MSBE.Region.Message;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.EnvironmentMapEffectBox region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.EnvironmentMapEffectBox temp = InnerObject as MSBE.Region.EnvironmentMapEffectBox;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.WindArea region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.WindArea temp = InnerObject as MSBE.Region.WindArea;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Connection region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Connection temp = InnerObject as MSBE.Region.Connection;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.PatrolRoute22 region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.PatrolRoute22 temp = InnerObject as MSBE.Region.PatrolRoute22;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.BuddySummonPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.BuddySummonPoint temp = InnerObject as MSBE.Region.BuddySummonPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MufflingBox region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MufflingBox temp = InnerObject as MSBE.Region.MufflingBox;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MufflingPortal region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MufflingPortal temp = InnerObject as MSBE.Region.MufflingPortal;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.SoundRegion region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.SoundRegion temp = InnerObject as MSBE.Region.SoundRegion;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MufflingPlane region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MufflingPlane temp = InnerObject as MSBE.Region.MufflingPlane;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.PatrolRoute region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.PatrolRoute temp = InnerObject as MSBE.Region.PatrolRoute;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MapPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MapPoint temp = InnerObject as MSBE.Region.MapPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.WeatherOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.WeatherOverride temp = InnerObject as MSBE.Region.WeatherOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.AutoDrawGroupPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.AutoDrawGroupPoint temp = InnerObject as MSBE.Region.AutoDrawGroupPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.GroupDefeatReward region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.GroupDefeatReward temp = InnerObject as MSBE.Region.GroupDefeatReward;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;
            Array.Clear(temp.PartNames);
            Array.Clear(temp.PartIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MapPointDiscoveryOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MapPointDiscoveryOverride temp = InnerObject as MSBE.Region.MapPointDiscoveryOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MapPointParticipationOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MapPointParticipationOverride temp = InnerObject as MSBE.Region.MapPointParticipationOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Hitset region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Hitset temp = InnerObject as MSBE.Region.Hitset;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.FastTravelRestriction region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.FastTravelRestriction temp = InnerObject as MSBE.Region.FastTravelRestriction;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.WeatherCreateAssetPoint region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.WeatherCreateAssetPoint temp = InnerObject as MSBE.Region.WeatherCreateAssetPoint;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.PlayArea region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.PlayArea temp = InnerObject as MSBE.Region.PlayArea;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.EnvironmentMapOutput region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.EnvironmentMapOutput temp = InnerObject as MSBE.Region.EnvironmentMapOutput;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MountJump region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MountJump temp = InnerObject as MSBE.Region.MountJump;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Dummy region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Dummy temp = InnerObject as MSBE.Region.Dummy;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.FallPreventionRemoval region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.FallPreventionRemoval temp = InnerObject as MSBE.Region.FallPreventionRemoval;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.NavmeshCutting region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.NavmeshCutting temp = InnerObject as MSBE.Region.NavmeshCutting;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MapNameOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MapNameOverride temp = InnerObject as MSBE.Region.MapNameOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.MountJumpFall region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.MountJumpFall temp = InnerObject as MSBE.Region.MountJumpFall;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.HorseRideOverride region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.HorseRideOverride temp = InnerObject as MSBE.Region.HorseRideOverride;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Region.Other region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            MSBE.Region.Other temp = InnerObject as MSBE.Region.Other;
            temp.ActivationPartName = "";
            temp.ActivationPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }

        // Events
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.Treasure mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.Treasure temp = InnerObject as MSBE.Event.Treasure;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.Generator mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.Generator temp = InnerObject as MSBE.Event.Generator;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.ObjAct mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.ObjAct temp = InnerObject as MSBE.Event.ObjAct;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.ObjActPartName = "";
            temp.ObjActPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.Navmesh mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.Navmesh temp = InnerObject as MSBE.Event.Navmesh;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.NavmeshRegionName = "";
            temp.NavmeshRegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.PseudoMultiplayer mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.PseudoMultiplayer temp = InnerObject as MSBE.Event.PseudoMultiplayer;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.PlatoonInfo mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.PlatoonInfo temp = InnerObject as MSBE.Event.PlatoonInfo;
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
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.PatrolInfo mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.PatrolInfo temp = InnerObject as MSBE.Event.PatrolInfo;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            Array.Clear(temp.WalkRegionNames);
            Array.Clear(temp.WalkRegionIndices);

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.Mount mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.Mount temp = InnerObject as MSBE.Event.Mount;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.RiderPartName = "";
            temp.RiderPartIndex = -1;
            temp.MountPartName = "";
            temp.MountPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.SignPool mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.SignPool temp = InnerObject as MSBE.Event.SignPool;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.SignPartName = "";
            temp.SignPartIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.RetryPoint mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.RetryPoint temp = InnerObject as MSBE.Event.RetryPoint;
            temp.PartName = "";
            temp.PartIndex = -1;
            temp.RegionName = "";
            temp.RegionIndex = -1;
            temp.RetryPartName = "";
            temp.RetryPartIndex = -1;
            temp.RetryRegionName = "";
            temp.RetryRegionIndex = -1;

            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                temp.EntityID = 0;
            }
        }
        public PrefabInfo_ER(Prefab_ER parent, MSBE.Event.Other mapEvent)
        {
            InnerObject = mapEvent.DeepCopy();
            DataType = AssetInfoDataType.Event;
            Parent = parent;

            MSBE.Event.Other temp = InnerObject as MSBE.Event.Other;
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
                throw new InvalidDataException($"Prefab_ER operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                throw new InvalidDataException($"Prefab_ER operation failed, {InnerObject.GetType()} does not contain Name property.");
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

    public List<MsbEntity> GenerateMapEntities(Map targetMap)
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
                case PrefabInfo_ER.AssetInfoDataType.Part:
                    ent.Type = MsbEntity.MsbEntityType.Part;
                    break;
                case PrefabInfo_ER.AssetInfoDataType.Region:
                    ent.Type = MsbEntity.MsbEntityType.Region;
                    break;
                case PrefabInfo_ER.AssetInfoDataType.Event:
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
            MSBE map = new();
            foreach (var assetInfo in PrefabInfoChildren)
            {
                assetInfo.StripNamePrefix();

                // Parts
                if (assetInfo.InnerObject is MSBE.Part.Enemy enemy)
                {
                    map.Parts.Enemies.Add(enemy);
                    MSBE.Model.Enemy model = new();
                    model.Name = enemy.ModelName;
                    map.Models.Enemies.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.MapPiece mapPiece)
                {
                    map.Parts.MapPieces.Add(mapPiece);
                    MSBE.Model.MapPiece model = new();
                    model.Name = mapPiece.ModelName;
                    map.Models.MapPieces.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.Player player)
                {
                    map.Parts.Players.Add(player);
                    MSBE.Model.Player model = new();
                    model.Name = player.ModelName;
                    map.Models.Players.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.Collision collision)
                {
                    map.Parts.Collisions.Add(collision);
                    MSBE.Model.Collision model = new();
                    model.Name = collision.ModelName;
                    map.Models.Collisions.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.DummyAsset dummyAsset)
                {
                    map.Parts.DummyAssets.Add(dummyAsset);
                    MSBE.Model.Asset model = new();
                    model.Name = dummyAsset.ModelName;
                    map.Models.Assets.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.DummyEnemy dummyEnemy)
                {
                    map.Parts.DummyEnemies.Add(dummyEnemy);
                    MSBE.Model.Enemy model = new();
                    model.Name = dummyEnemy.ModelName;
                    map.Models.Enemies.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.ConnectCollision connectCol)
                {
                    map.Parts.ConnectCollisions.Add(connectCol);
                    MSBE.Model.Collision model = new();
                    model.Name = connectCol.ModelName;
                    map.Models.Collisions.Add(model);
                }
                if (assetInfo.InnerObject is MSBE.Part.Asset asset)
                {
                    map.Parts.Assets.Add(asset);
                    MSBE.Model.Asset model = new();
                    model.Name = asset.ModelName;
                    map.Models.Assets.Add(model);
                }

                // Regions
                if (assetInfo.InnerObject is MSBE.Region.InvasionPoint invasionPoint)
                {
                    map.Regions.InvasionPoints.Add(invasionPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.EnvironmentMapPoint envMapPoint)
                {
                    map.Regions.EnvironmentMapPoints.Add(envMapPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.Sound sound)
                {
                    map.Regions.Sounds.Add(sound);
                }
                if (assetInfo.InnerObject is MSBE.Region.SFX sfx)
                {
                    map.Regions.SFX.Add(sfx);
                }
                if (assetInfo.InnerObject is MSBE.Region.WindSFX windSfx)
                {
                    map.Regions.WindSFX.Add(windSfx);
                }
                if (assetInfo.InnerObject is MSBE.Region.SpawnPoint spawnPoint)
                {
                    map.Regions.SpawnPoints.Add(spawnPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.Message message)
                {
                    map.Regions.Messages.Add(message);
                }
                if (assetInfo.InnerObject is MSBE.Region.EnvironmentMapEffectBox envMapEffectBox)
                {
                    map.Regions.EnvironmentMapEffectBoxes.Add(envMapEffectBox);
                }
                if (assetInfo.InnerObject is MSBE.Region.WindArea windArea)
                {
                    map.Regions.WindAreas.Add(windArea);
                }
                if (assetInfo.InnerObject is MSBE.Region.Connection connection)
                {
                    map.Regions.Connections.Add(connection);
                }
                if (assetInfo.InnerObject is MSBE.Region.PatrolRoute22 patrolRoute22)
                {
                    map.Regions.PatrolRoute22s.Add(patrolRoute22);
                }
                if (assetInfo.InnerObject is MSBE.Region.BuddySummonPoint buddySummonPoint)
                {
                    map.Regions.BuddySummonPoints.Add(buddySummonPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.MufflingBox mufflingBox)
                {
                    map.Regions.MufflingBoxes.Add(mufflingBox);
                }
                if (assetInfo.InnerObject is MSBE.Region.MufflingPortal mufflingPortal)
                {
                    map.Regions.MufflingPortals.Add(mufflingPortal);
                }
                if (assetInfo.InnerObject is MSBE.Region.SoundRegion soundRegion)
                {
                    map.Regions.SoundRegions.Add(soundRegion);
                }
                if (assetInfo.InnerObject is MSBE.Region.MufflingPlane mufflingPlane)
                {
                    map.Regions.MufflingPlanes.Add(mufflingPlane);
                }
                if (assetInfo.InnerObject is MSBE.Region.PatrolRoute patrolRoute)
                {
                    map.Regions.PatrolRoutes.Add(patrolRoute);
                }
                if (assetInfo.InnerObject is MSBE.Region.MapPoint mapPoint)
                {
                    map.Regions.MapPoints.Add(mapPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.WeatherOverride weatherOverride)
                {
                    map.Regions.WeatherOverrides.Add(weatherOverride);
                }
                if (assetInfo.InnerObject is MSBE.Region.AutoDrawGroupPoint autoDrawGroupPoint)
                {
                    map.Regions.AutoDrawGroupPoints.Add(autoDrawGroupPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.GroupDefeatReward groupDefeatReward)
                {
                    map.Regions.GroupDefeatRewards.Add(groupDefeatReward);
                }
                if (assetInfo.InnerObject is MSBE.Region.MapPointDiscoveryOverride mapPointDiscoveryOverride)
                {
                    map.Regions.MapPointDiscoveryOverrides.Add(mapPointDiscoveryOverride);
                }
                if (assetInfo.InnerObject is MSBE.Region.MapPointParticipationOverride mapPointParticipationOverride)
                {
                    map.Regions.MapPointParticipationOverrides.Add(mapPointParticipationOverride);
                }
                if (assetInfo.InnerObject is MSBE.Region.Hitset hitset)
                {
                    map.Regions.Hitsets.Add(hitset);
                }
                if (assetInfo.InnerObject is MSBE.Region.FastTravelRestriction fastTravelRestriction)
                {
                    map.Regions.FastTravelRestriction.Add(fastTravelRestriction);
                }
                if (assetInfo.InnerObject is MSBE.Region.WeatherCreateAssetPoint weatherCreateAssetPoint)
                {
                    map.Regions.WeatherCreateAssetPoints.Add(weatherCreateAssetPoint);
                }
                if (assetInfo.InnerObject is MSBE.Region.PlayArea playArea)
                {
                    map.Regions.PlayAreas.Add(playArea);
                }
                if (assetInfo.InnerObject is MSBE.Region.EnvironmentMapOutput envMapOutput)
                {
                    map.Regions.EnvironmentMapOutputs.Add(envMapOutput);
                }
                if (assetInfo.InnerObject is MSBE.Region.MountJump mountJump)
                {
                    map.Regions.MountJumps.Add(mountJump);
                }
                if (assetInfo.InnerObject is MSBE.Region.Dummy dummy)
                {
                    map.Regions.Dummies.Add(dummy);
                }
                if (assetInfo.InnerObject is MSBE.Region.FallPreventionRemoval fallPreventionRemoval)
                {
                    map.Regions.FallPreventionRemovals.Add(fallPreventionRemoval);
                }
                if (assetInfo.InnerObject is MSBE.Region.NavmeshCutting navmeshCutting)
                {
                    map.Regions.NavmeshCuttings.Add(navmeshCutting);
                }
                if (assetInfo.InnerObject is MSBE.Region.MapNameOverride mapNameOverride)
                {
                    map.Regions.MapNameOverrides.Add(mapNameOverride);
                }
                if (assetInfo.InnerObject is MSBE.Region.MountJumpFall mountJumpFall)
                {
                    map.Regions.MountJumpFalls.Add(mountJumpFall);
                }
                if (assetInfo.InnerObject is MSBE.Region.HorseRideOverride horseRideOverride)
                {
                    map.Regions.HorseRideOverrides.Add(horseRideOverride);
                }
                if (assetInfo.InnerObject is MSBE.Region.Other region)
                {
                    map.Regions.Others.Add(region);
                }

                // Events
                if (assetInfo.InnerObject is MSBE.Event.Treasure treasure)
                {
                    map.Events.Treasures.Add(treasure);
                }
                if (assetInfo.InnerObject is MSBE.Event.Generator generator)
                {
                    map.Events.Generators.Add(generator);
                }
                if (assetInfo.InnerObject is MSBE.Event.ObjAct objAct)
                {
                    map.Events.ObjActs.Add(objAct);
                }
                if (assetInfo.InnerObject is MSBE.Event.Navmesh navmesh)
                {
                    map.Events.Navmeshes.Add(navmesh);
                }
                if (assetInfo.InnerObject is MSBE.Event.PseudoMultiplayer pseudoMultiplayer)
                {
                    map.Events.PseudoMultiplayers.Add(pseudoMultiplayer);
                }
                if (assetInfo.InnerObject is MSBE.Event.PlatoonInfo platoonInfo)
                {
                    map.Events.PlatoonInfo.Add(platoonInfo);
                }
                if (assetInfo.InnerObject is MSBE.Event.PatrolInfo patrolInfo)
                {
                    map.Events.PatrolInfo.Add(patrolInfo);
                }
                if (assetInfo.InnerObject is MSBE.Event.Mount mount)
                {
                    map.Events.Mounts.Add(mount);
                }
                if (assetInfo.InnerObject is MSBE.Event.SignPool signPool)
                {
                    map.Events.SignPools.Add(signPool);
                }
                if (assetInfo.InnerObject is MSBE.Event.RetryPoint retryPoint)
                {
                    map.Events.RetryPoints.Add(retryPoint);
                }
                if (assetInfo.InnerObject is MSBE.Event.Other other)
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
    public static Prefab_ER ImportJson(string path)
    {
        try
        {
            var settings = new JsonSerializerSettings();
            Prefab_ER prefab = JsonConvert.DeserializeObject<Prefab_ER>(File.ReadAllText(path), settings);

            MSBE pseudoMap = MSBE.Read(prefab.AssetContainerBytes);

            // Parts
            foreach (var mapPiece in pseudoMap.Parts.MapPieces)
            {
                PrefabInfo_ER info = new(prefab, mapPiece);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var enemy in pseudoMap.Parts.Enemies)
            {
                PrefabInfo_ER info = new(prefab, enemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var player in pseudoMap.Parts.Players)
            {
                PrefabInfo_ER info = new(prefab, player);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var col in pseudoMap.Parts.Collisions)
            {
                PrefabInfo_ER info = new(prefab, col);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var dummyAsset in pseudoMap.Parts.DummyAssets)
            {
                PrefabInfo_ER info = new(prefab, dummyAsset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
            {
                PrefabInfo_ER info = new(prefab, dummyEnemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
            {
                PrefabInfo_ER info = new(prefab, connectCol);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Assets)
            {
                PrefabInfo_ER info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            // Regions
            foreach (var invasionPoint in pseudoMap.Regions.InvasionPoints)
            {
                PrefabInfo_ER info = new(prefab, invasionPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var envMapPoints in pseudoMap.Regions.EnvironmentMapPoints)
            {
                PrefabInfo_ER info = new(prefab, envMapPoints);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var sound in pseudoMap.Regions.Sounds)
            {
                PrefabInfo_ER info = new(prefab, sound);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var sfx in pseudoMap.Regions.SFX)
            {
                PrefabInfo_ER info = new(prefab, sfx);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var windSfx in pseudoMap.Regions.WindSFX)
            {
                PrefabInfo_ER info = new(prefab, windSfx);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var spawnPoint in pseudoMap.Regions.SpawnPoints)
            {
                PrefabInfo_ER info = new(prefab, spawnPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var message in pseudoMap.Regions.Messages)
            {
                PrefabInfo_ER info = new(prefab, message);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var envMapEffectBox in pseudoMap.Regions.EnvironmentMapEffectBoxes)
            {
                PrefabInfo_ER info = new(prefab, envMapEffectBox);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var windArea in pseudoMap.Regions.WindAreas)
            {
                PrefabInfo_ER info = new(prefab, windArea);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var connection in pseudoMap.Regions.Connections)
            {
                PrefabInfo_ER info = new(prefab, connection);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var patrolRoute22 in pseudoMap.Regions.PatrolRoute22s)
            {
                PrefabInfo_ER info = new(prefab, patrolRoute22);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var buddySummonPoint in pseudoMap.Regions.BuddySummonPoints)
            {
                PrefabInfo_ER info = new(prefab, buddySummonPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mufflingBox in pseudoMap.Regions.MufflingBoxes)
            {
                PrefabInfo_ER info = new(prefab, mufflingBox);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mufflingPortal in pseudoMap.Regions.MufflingPortals)
            {
                PrefabInfo_ER info = new(prefab, mufflingPortal);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var soundRegion in pseudoMap.Regions.SoundRegions)
            {
                PrefabInfo_ER info = new(prefab, soundRegion);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mufflingPlane in pseudoMap.Regions.MufflingPlanes)
            {
                PrefabInfo_ER info = new(prefab, mufflingPlane);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var patrolRoute in pseudoMap.Regions.PatrolRoutes)
            {
                PrefabInfo_ER info = new(prefab, patrolRoute);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mapPoint in pseudoMap.Regions.MapPoints)
            {
                PrefabInfo_ER info = new(prefab, mapPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var weatherOverride in pseudoMap.Regions.WeatherOverrides)
            {
                PrefabInfo_ER info = new(prefab, weatherOverride);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var autoDrawGroupPoint in pseudoMap.Regions.AutoDrawGroupPoints)
            {
                PrefabInfo_ER info = new(prefab, autoDrawGroupPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var groupDefeatReward in pseudoMap.Regions.GroupDefeatRewards)
            {
                PrefabInfo_ER info = new(prefab, groupDefeatReward);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mapPointDiscoveryOverride in pseudoMap.Regions.MapPointDiscoveryOverrides)
            {
                PrefabInfo_ER info = new(prefab, mapPointDiscoveryOverride);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mapPointParticipationOverride in pseudoMap.Regions.MapPointParticipationOverrides)
            {
                PrefabInfo_ER info = new(prefab, mapPointParticipationOverride);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var hitset in pseudoMap.Regions.Hitsets)
            {
                PrefabInfo_ER info = new(prefab, hitset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var fastTravelRestriction in pseudoMap.Regions.FastTravelRestriction)
            {
                PrefabInfo_ER info = new(prefab, fastTravelRestriction);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var weatherCreateAssetPoint in pseudoMap.Regions.WeatherCreateAssetPoints)
            {
                PrefabInfo_ER info = new(prefab, weatherCreateAssetPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var playArea in pseudoMap.Regions.PlayAreas)
            {
                PrefabInfo_ER info = new(prefab, playArea);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var envMapOutput in pseudoMap.Regions.EnvironmentMapOutputs)
            {
                PrefabInfo_ER info = new(prefab, envMapOutput);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mountJump in pseudoMap.Regions.MountJumps)
            {
                PrefabInfo_ER info = new(prefab, mountJump);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var dummy in pseudoMap.Regions.Dummies)
            {
                PrefabInfo_ER info = new(prefab, dummy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var fallPreventionRemoval in pseudoMap.Regions.FallPreventionRemovals)
            {
                PrefabInfo_ER info = new(prefab, fallPreventionRemoval);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var navmeshCutting in pseudoMap.Regions.NavmeshCuttings)
            {
                PrefabInfo_ER info = new(prefab, navmeshCutting);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mapNameOverride in pseudoMap.Regions.MapNameOverrides)
            {
                PrefabInfo_ER info = new(prefab, mapNameOverride);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mountJumpFall in pseudoMap.Regions.MountJumpFalls)
            {
                PrefabInfo_ER info = new(prefab, mountJumpFall);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var horseRideOverride in pseudoMap.Regions.HorseRideOverrides)
            {
                PrefabInfo_ER info = new(prefab, horseRideOverride);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var region in pseudoMap.Regions.Others)
            {
                PrefabInfo_ER info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            // Events
            foreach (var treasure in pseudoMap.Events.Treasures)
            {
                PrefabInfo_ER info = new(prefab, treasure);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var generator in pseudoMap.Events.Generators)
            {
                PrefabInfo_ER info = new(prefab, generator);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var objAct in pseudoMap.Events.ObjActs)
            {
                PrefabInfo_ER info = new(prefab, objAct);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var navmesh in pseudoMap.Events.Navmeshes)
            {
                PrefabInfo_ER info = new(prefab, navmesh);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var pseudoMultiplayer in pseudoMap.Events.PseudoMultiplayers)
            {
                PrefabInfo_ER info = new(prefab, pseudoMultiplayer);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var platoonInfo in pseudoMap.Events.PlatoonInfo)
            {
                PrefabInfo_ER info = new(prefab, platoonInfo);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var patrolInfo in pseudoMap.Events.PatrolInfo)
            {
                PrefabInfo_ER info = new(prefab, patrolInfo);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var mount in pseudoMap.Events.Mounts)
            {
                PrefabInfo_ER info = new(prefab, mount);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var signPool in pseudoMap.Events.SignPools)
            {
                PrefabInfo_ER info = new(prefab, signPool);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var retryPoint in pseudoMap.Events.RetryPoints)
            {
                PrefabInfo_ER info = new(prefab, retryPoint);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }
            foreach (var other in pseudoMap.Events.Others)
            {
                PrefabInfo_ER info = new(prefab, other);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.PrefabInfoChildren.Add(info);
            }

            return prefab;
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox(
                $"Unable to import Prefab_ER due to the following error:" +
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
    public static void ImportSelectedPrefab(PrefabInfo info, (string, MapObjectContainer) _comboTargetMap, Universe _universe, RenderScene _scene, EntityActionManager _actionManager)
    {
        Prefab_ER _selectedAssetPrefab;

        _selectedAssetPrefab = Prefab_ER.ImportJson(info.Path);
        Map targetMap = (Map)_comboTargetMap.Item2;

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

        Prefab_ER prefab = new(_selection.GetFilteredSelection<MsbEntity>());

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

    public static List<string> GetSelectedPrefabObjects(PrefabInfo info, (string, MapObjectContainer) _comboTargetMap)
    {
        List<string> entNames = new List<string>();
        Prefab_ER _selectedAssetPrefab;

        _selectedAssetPrefab = Prefab_ER.ImportJson(info.Path);
        Map targetMap = (Map)_comboTargetMap.Item2;

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

        if (ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
        {
            foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        if (ent.WrappedObject is MSBE.Part.Enemy enemy || ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
        {
            foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        if (ent.WrappedObject is MSBE.Part.Asset asset || ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
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
        if (ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
        {
            name = $"Map Piece: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.Enemy enemy)
        {
            name = $"Enemy: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.Player player)
        {
            name = $"Player: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.Collision col)
        {
            name = $"Collision: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
        {
            name = $"Dummy Asset: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
        {
            name = $"Dummy Enemy: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.ConnectCollision connectCol)
        {
            name = $"Connect Collision: {name}";
        }
        if (ent.WrappedObject is MSBE.Part.Asset asset)
        {
            name = $"Asset: {name}";
        }

        // Regions
        if (ent.WrappedObject is MSBE.Region.InvasionPoint invasionPoint)
        {
            name = $"Invasion Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.EnvironmentMapPoint envMapPoint)
        {
            name = $"Environment Map Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Sound sound)
        {
            name = $"Sound: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.SFX sfx)
        {
            name = $"SFX: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.WindSFX windSfx)
        {
            name = $"Wind SFX: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.SpawnPoint spawnPoint)
        {
            name = $"Spawn Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Message message)
        {
            name = $"Message: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.EnvironmentMapEffectBox envMapEffectbox)
        {
            name = $"Environment Map Effect Box: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.WindArea windArea)
        {
            name = $"Wind Area: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Connection connection)
        {
            name = $"Connection: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.PatrolRoute22 patrolRoute22)
        {
            name = $"Patrol Route 22: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.BuddySummonPoint buddySummonPoint)
        {
            name = $"Buddy Summon Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MufflingBox mufflingBox)
        {
            name = $"Muffling Box: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MufflingPortal mufflingPortal)
        {
            name = $"Muffling Portal: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.SoundRegion soundRegion)
        {
            name = $"Sound Region: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MufflingPlane mufflingPlane)
        {
            name = $"Muffling Plane: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.PatrolRoute patrolRoute)
        {
            name = $"Patrol Route: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MapPoint mapPoint)
        {
            name = $"Map Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.WeatherOverride weatherOverride)
        {
            name = $"Weather Override: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.AutoDrawGroupPoint autoDrawGroupPoint)
        {
            name = $"Auto Draw Group Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.GroupDefeatReward groupDefeatReward)
        {
            name = $"Group Defeat Reward: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MapPointDiscoveryOverride mapPointDiscoveryOverride)
        {
            name = $"Map Point Discovery Override: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MapPointParticipationOverride mapPointParticapationOverride)
        {
            name = $"Map Point Participation Override: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Hitset hitset)
        {
            name = $"Hitset: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.FastTravelRestriction fastTravelRestriction)
        {
            name = $"Fast Travel Restriction: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.WeatherCreateAssetPoint weatherCreateAssetPoint)
        {
            name = $"Weather Create Asset Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.PlayArea playArea)
        {
            name = $"Play Area: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.EnvironmentMapOutput envMapOutput)
        {
            name = $"Environment Map Output: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MountJump mountJump)
        {
            name = $"Mount Jump: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Dummy dummy)
        {
            name = $"Dummy: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.FallPreventionRemoval fallPreventionRemoval)
        {
            name = $"Fall Prevention Removal: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.NavmeshCutting navmeshCutting)
        {
            name = $"Navmesh Cutting: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MapNameOverride mapNameOverride)
        {
            name = $"Map Name Override: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.MountJumpFall mountJumpFall)
        {
            name = $"Mount Jump Fall: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.HorseRideOverride horseRideOverride)
        {
            name = $"Horse Ride Override: {name}";
        }
        if (ent.WrappedObject is MSBE.Region.Other region)
        {
            name = $"Other: {name}";
        }

        // Events
        if (ent.WrappedObject is MSBE.Event.Treasure treasure)
        {
            name = $"Treasure: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.Generator generator)
        {
            name = $"Generator: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.ObjAct objAct)
        {
            name = $"ObjAct: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.Navmesh navmesh)
        {
            name = $"Navmesh: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.PseudoMultiplayer pseudoMultiplayer)
        {
            name = $"Pseudo Multiplayer: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.PlatoonInfo platoonInfo)
        {
            name = $"Platoon Info: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.PatrolInfo patrolInfo)
        {
            name = $"Patrol Info: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.Mount mount)
        {
            name = $"Mount: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.SignPool signPool)
        {
            name = $"Sign Pool: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.RetryPoint retryPoint)
        {
            name = $"Retry Point: {name}";
        }
        if (ent.WrappedObject is MSBE.Event.Other other)
        {
            name = $"Other: {name}";
        }

        return name;
    }
}