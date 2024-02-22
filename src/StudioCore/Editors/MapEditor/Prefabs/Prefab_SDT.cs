using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.BanksMain;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Prefabs
{

    public class Prefab_SDT
    {
        public string PrefabName = "";
        public string PrefixSeparator = "[]";
        public ProjectType Type = ProjectType.SDT;

        public List<string> TagList;

        /// <summary>
        /// Bytes of the MSB that stores prefab data.
        /// </summary>
        public byte[] AssetContainerBytes { get; set; }

        /// <summary>
        /// List of AssetInfo derived from MSB AssetContainerBytes.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<PrefabInfo_SDT> PrefabInfoChildren = new();

        /// <summary>
        /// List of Msb Entities derived from AssetInfoChildren.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<MsbEntity> MsbEntityChildren = new();


        // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
        [Newtonsoft.Json.JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Prefab_SDT()
        { }

        public Prefab_SDT(HashSet<MsbEntity> entities)
        {
            foreach (var ent in entities)
            {
                // Parts
                if (ent.WrappedObject is MSBS.Part.MapPiece mapPiece)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mapPiece));
                }
                if (ent.WrappedObject is MSBS.Part.Object mapObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mapObject));
                }
                if (ent.WrappedObject is MSBS.Part.Enemy enemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, enemy));
                }
                if (ent.WrappedObject is MSBS.Part.Player player)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, player));
                }
                if (ent.WrappedObject is MSBS.Part.Collision col)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, col));
                }
                if (ent.WrappedObject is MSBS.Part.DummyObject dummyObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, dummyObject));
                }
                if (ent.WrappedObject is MSBS.Part.DummyEnemy dummyEnemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, dummyEnemy));
                }
                if (ent.WrappedObject is MSBS.Part.ConnectCollision connectCol)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, connectCol));
                }

                // Regions
                if (ent.WrappedObject is MSBS.Region.InvasionPoint invasionPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, invasionPoint));
                }
                if (ent.WrappedObject is MSBS.Region.EnvironmentMapPoint envMapPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, envMapPoint));
                }
                if (ent.WrappedObject is MSBS.Region.Sound sound)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, sound));
                }
                if (ent.WrappedObject is MSBS.Region.SFX sfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, sfx));
                }
                if (ent.WrappedObject is MSBS.Region.WindSFX windSfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, windSfx));
                }
                if (ent.WrappedObject is MSBS.Region.SpawnPoint spawnPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, spawnPoint));
                }
                if (ent.WrappedObject is MSBS.Region.PatrolRoute patrolRoute)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, patrolRoute));
                }
                if (ent.WrappedObject is MSBS.Region.WarpPoint warpPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, warpPoint));
                }
                if (ent.WrappedObject is MSBS.Region.ActivationArea activationArea)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, activationArea));
                }
                if (ent.WrappedObject is MSBS.Region.Event mapEvent)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mapEvent));
                }
                if (ent.WrappedObject is MSBS.Region.Logic logic)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, logic));
                }
                if (ent.WrappedObject is MSBS.Region.EnvironmentMapEffectBox envMapEffectBox)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, envMapEffectBox));
                }
                if (ent.WrappedObject is MSBS.Region.WindArea windArea)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, windArea));
                }
                if (ent.WrappedObject is MSBS.Region.MufflingBox mufflingBox)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mufflingBox));
                }
                if (ent.WrappedObject is MSBS.Region.MufflingPortal mufflingPortal)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mufflingPortal));
                }
                if (ent.WrappedObject is MSBS.Region.SoundSpaceOverride soundSpaceOverride)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, soundSpaceOverride));
                }
                if (ent.WrappedObject is MSBS.Region.MufflingPlane mufflingPlane)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mufflingPlane));
                }
                if (ent.WrappedObject is MSBS.Region.PartsGroupArea partsGroupArea)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, partsGroupArea));
                }
                if (ent.WrappedObject is MSBS.Region.AutoDrawGroupPoint autoDrawGroupPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, autoDrawGroupPoint));
                }
                if (ent.WrappedObject is MSBS.Region.Other region)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, region));
                }

                // Events
                if (ent.WrappedObject is MSBS.Event.Treasure treasure)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, treasure));
                }
                if (ent.WrappedObject is MSBS.Event.Generator generator)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, generator));
                }
                if (ent.WrappedObject is MSBS.Event.ObjAct objAct)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, objAct));
                }
                if (ent.WrappedObject is MSBS.Event.MapOffset mapOffset)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, mapOffset));
                }
                if (ent.WrappedObject is MSBS.Event.PatrolInfo patrolInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, patrolInfo));
                }
                if (ent.WrappedObject is MSBS.Event.PlatoonInfo platoonInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, platoonInfo));
                }
                if (ent.WrappedObject is MSBS.Event.ResourceItemInfo resourceItemInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, resourceItemInfo));
                }
                if (ent.WrappedObject is MSBS.Event.GrassLodParam grassLodParam)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, grassLodParam));
                }
                if (ent.WrappedObject is MSBS.Event.SkitInfo skitInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, skitInfo));
                }
                if (ent.WrappedObject is MSBS.Event.PlacementGroup placementGroup)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, placementGroup));
                }
                if (ent.WrappedObject is MSBS.Event.PartsGroup partsGroup)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, partsGroup));
                }
                if (ent.WrappedObject is MSBS.Event.Talk talk)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, talk));
                }
                if (ent.WrappedObject is MSBS.Event.AutoDrawGroupCollision autoDrawGroupCollision)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, autoDrawGroupCollision));
                }
                if (ent.WrappedObject is MSBS.Event.Other other)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_SDT(this, other));
                }
            }
        }
        public class PrefabInfo_SDT
        {

            public Prefab_SDT Parent;

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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.MapPiece mapPiece)
            {
                InnerObject = mapPiece.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.MapPiece temp = InnerObject as MSBS.Part.MapPiece;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroupIDs);
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.Object mapObject)
            {
                InnerObject = mapObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.Object temp = InnerObject as MSBS.Part.Object;
                temp.ObjPartIndex1 = -1;
                temp.ObjPartName1 = "";
                temp.ObjPartIndex2 = -1;
                temp.ObjPartName2 = "";
                temp.ObjPartIndex3 = -1;
                temp.ObjPartName3 = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroupIDs);
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.Enemy enemy)
            {
                InnerObject = enemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.Enemy temp = InnerObject as MSBS.Part.Enemy;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.Player player)
            {
                InnerObject = player.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.Player temp = InnerObject as MSBS.Part.Player;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroupIDs);
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.Collision collision)
            {
                InnerObject = collision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.Collision temp = InnerObject as MSBS.Part.Collision;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroupIDs);
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.DummyObject dummyObject)
            {
                InnerObject = dummyObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.DummyObject temp = InnerObject as MSBS.Part.DummyObject;
                temp.ObjPartIndex1 = -1;
                temp.ObjPartName1 = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroupIDs);
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.DummyEnemy dummyEnemy)
            {
                InnerObject = dummyEnemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.DummyEnemy temp = InnerObject as MSBS.Part.DummyEnemy;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Part.ConnectCollision connectCollision)
            {
                InnerObject = connectCollision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSBS.Part.ConnectCollision temp = InnerObject as MSBS.Part.ConnectCollision;
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

            // Regions
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.InvasionPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.InvasionPoint temp = InnerObject as MSBS.Region.InvasionPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.EnvironmentMapPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.EnvironmentMapPoint temp = InnerObject as MSBS.Region.EnvironmentMapPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.Sound region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.Sound temp = InnerObject as MSBS.Region.Sound;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;
                Array.Clear(temp.ChildRegionNames);
                Array.Clear(temp.ChildRegionIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.SFX region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.SFX temp = InnerObject as MSBS.Region.SFX;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.WindSFX region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.WindSFX temp = InnerObject as MSBS.Region.WindSFX;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;
                temp.WindAreaName = "";
                temp.WindAreaIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.SpawnPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.SpawnPoint temp = InnerObject as MSBS.Region.SpawnPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.PatrolRoute region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.PatrolRoute temp = InnerObject as MSBS.Region.PatrolRoute;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.WarpPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.WarpPoint temp = InnerObject as MSBS.Region.WarpPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.ActivationArea region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.ActivationArea temp = InnerObject as MSBS.Region.ActivationArea;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.Event region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.Event temp = InnerObject as MSBS.Region.Event;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.Logic region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.Logic temp = InnerObject as MSBS.Region.Logic;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.EnvironmentMapEffectBox region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.EnvironmentMapEffectBox temp = InnerObject as MSBS.Region.EnvironmentMapEffectBox;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.WindArea region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.WindArea temp = InnerObject as MSBS.Region.WindArea;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.MufflingBox region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.MufflingBox temp = InnerObject as MSBS.Region.MufflingBox;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.MufflingPortal region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.MufflingPortal temp = InnerObject as MSBS.Region.MufflingPortal;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.SoundSpaceOverride region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.SoundSpaceOverride temp = InnerObject as MSBS.Region.SoundSpaceOverride;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.MufflingPlane region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.MufflingPlane temp = InnerObject as MSBS.Region.MufflingPlane;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.PartsGroupArea region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.PartsGroupArea temp = InnerObject as MSBS.Region.PartsGroupArea;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.AutoDrawGroupPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.AutoDrawGroupPoint temp = InnerObject as MSBS.Region.AutoDrawGroupPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Region.Other region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSBS.Region.Other temp = InnerObject as MSBS.Region.Other;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }

            // Events
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.Treasure mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.Treasure temp = InnerObject as MSBS.Event.Treasure;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.Generator mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.Generator temp = InnerObject as MSBS.Event.Generator;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.ObjAct mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.ObjAct temp = InnerObject as MSBS.Event.ObjAct;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.MapOffset mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.MapOffset temp = InnerObject as MSBS.Event.MapOffset;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.PlatoonInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.PlatoonInfo temp = InnerObject as MSBS.Event.PlatoonInfo;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                Array.Clear(temp.GroupPartNames);
                Array.Clear(temp.GroupPartIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.PatrolInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.PatrolInfo temp = InnerObject as MSBS.Event.PatrolInfo;
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
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.ResourceItemInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.ResourceItemInfo temp = InnerObject as MSBS.Event.ResourceItemInfo;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.GrassLodParam mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.GrassLodParam temp = InnerObject as MSBS.Event.GrassLodParam;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.SkitInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.SkitInfo temp = InnerObject as MSBS.Event.SkitInfo;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.PlacementGroup mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.PlacementGroup temp = InnerObject as MSBS.Event.PlacementGroup;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                Array.Clear(temp.PlacementGroupEnemyNames);
                Array.Clear(temp.PlacementGroupEnemyIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.PartsGroup mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.PartsGroup temp = InnerObject as MSBS.Event.PartsGroup;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.Talk mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.Talk temp = InnerObject as MSBS.Event.Talk;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                Array.Clear(temp.EnemyNames);
                Array.Clear(temp.EnemyIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.AutoDrawGroupCollision mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.AutoDrawGroupCollision temp = InnerObject as MSBS.Event.AutoDrawGroupCollision;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                temp.AutoDrawGroupPointName = "";
                temp.AutoDrawGroupPointIndex = -1;
                temp.OwningCollisionName = "";
                temp.OwningCollisionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_SDT(Prefab_SDT parent, MSBS.Event.Other mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSBS.Event.Other temp = InnerObject as MSBS.Event.Other;
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
                    throw new InvalidDataException($"Prefab_SDT operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    throw new InvalidDataException($"Prefab_SDT operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    case PrefabInfo_SDT.AssetInfoDataType.Part:
                        ent.Type = MsbEntity.MsbEntityType.Part;
                        break;
                    case PrefabInfo_SDT.AssetInfoDataType.Region:
                        ent.Type = MsbEntity.MsbEntityType.Region;
                        break;
                    case PrefabInfo_SDT.AssetInfoDataType.Event:
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
                MSBS map = new();
                foreach (var assetInfo in PrefabInfoChildren)
                {
                    assetInfo.StripNamePrefix();

                    // Parts
                    if (assetInfo.InnerObject is MSBS.Part.Enemy enemy)
                    {
                        map.Parts.Enemies.Add(enemy);
                        MSBS.Model.Enemy model = new();
                        model.Name = enemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.Object mapObject)
                    {
                        map.Parts.Objects.Add(mapObject);
                        MSBS.Model.Object model = new();
                        model.Name = mapObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.MapPiece mapPiece)
                    {
                        map.Parts.MapPieces.Add(mapPiece);
                        MSBS.Model.MapPiece model = new();
                        model.Name = mapPiece.ModelName;
                        map.Models.MapPieces.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.Player player)
                    {
                        map.Parts.Players.Add(player);
                        MSBS.Model.Player model = new();
                        model.Name = player.ModelName;
                        map.Models.Players.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.Collision collision)
                    {
                        map.Parts.Collisions.Add(collision);
                        MSBS.Model.Collision model = new();
                        model.Name = collision.ModelName;
                        map.Models.Collisions.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.DummyObject dummyObject)
                    {
                        map.Parts.DummyObjects.Add(dummyObject);
                        MSBS.Model.Object model = new();
                        model.Name = dummyObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.DummyEnemy dummyEnemy)
                    {
                        map.Parts.DummyEnemies.Add(dummyEnemy);
                        MSBS.Model.Enemy model = new();
                        model.Name = dummyEnemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSBS.Part.ConnectCollision connectCol)
                    {
                        map.Parts.ConnectCollisions.Add(connectCol);
                        MSBS.Model.Collision model = new();
                        model.Name = connectCol.ModelName;
                        map.Models.Collisions.Add(model);
                    }

                    // Regions
                    if (assetInfo.InnerObject is MSBS.Region.InvasionPoint invasionPoint)
                    {
                        map.Regions.InvasionPoints.Add(invasionPoint);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.EnvironmentMapPoint envMapPoint)
                    {
                        map.Regions.EnvironmentMapPoints.Add(envMapPoint);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.Sound sound)
                    {
                        map.Regions.Sounds.Add(sound);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.SFX sfx)
                    {
                        map.Regions.SFX.Add(sfx);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.WindSFX windSfx)
                    {
                        map.Regions.WindSFX.Add(windSfx);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.SpawnPoint spawnPoint)
                    {
                        map.Regions.SpawnPoints.Add(spawnPoint);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.PatrolRoute patrolRoute)
                    {
                        map.Regions.PatrolRoutes.Add(patrolRoute);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.WarpPoint warpPoint)
                    {
                        map.Regions.WarpPoints.Add(warpPoint);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.ActivationArea activationArea)
                    {
                        map.Regions.ActivationAreas.Add(activationArea);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.Event mapEvent)
                    {
                        map.Regions.Events.Add(mapEvent);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.Logic logic)
                    {
                        map.Regions.Logic.Add(logic);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.EnvironmentMapEffectBox envMapEffectBox)
                    {
                        map.Regions.EnvironmentMapEffectBoxes.Add(envMapEffectBox);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.WindArea windArea)
                    {
                        map.Regions.WindAreas.Add(windArea);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.MufflingBox mufflingBox)
                    {
                        map.Regions.MufflingBoxes.Add(mufflingBox);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.MufflingPortal mufflingPortal)
                    {
                        map.Regions.MufflingPortals.Add(mufflingPortal);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.SoundSpaceOverride soundSpaceOverride)
                    {
                        map.Regions.SoundSpaceOverrides.Add(soundSpaceOverride);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.MufflingPlane mufflingPlane)
                    {
                        map.Regions.MufflingPlanes.Add(mufflingPlane);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.PartsGroupArea partsGroupArea)
                    {
                        map.Regions.PartsGroupAreas.Add(partsGroupArea);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.AutoDrawGroupPoint autoDrawGroupPoint)
                    {
                        map.Regions.AutoDrawGroupPoints.Add(autoDrawGroupPoint);
                    }
                    if (assetInfo.InnerObject is MSBS.Region.Other region)
                    {
                        map.Regions.Others.Add(region);
                    }

                    // Events
                    if (assetInfo.InnerObject is MSBS.Event.Treasure treasure)
                    {
                        map.Events.Treasures.Add(treasure);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.Generator generator)
                    {
                        map.Events.Generators.Add(generator);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.ObjAct objAct)
                    {
                        map.Events.ObjActs.Add(objAct);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.MapOffset mapOffset)
                    {
                        map.Events.MapOffsets.Add(mapOffset);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.PlatoonInfo platoonInfo)
                    {
                        map.Events.PlatoonInfo.Add(platoonInfo);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.PatrolInfo patrolInfo)
                    {
                        map.Events.PatrolInfo.Add(patrolInfo);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.ResourceItemInfo resourceItemInfo)
                    {
                        map.Events.ResourceItemInfo.Add(resourceItemInfo);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.GrassLodParam grassLodParam)
                    {
                        map.Events.GrassLodParams.Add(grassLodParam);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.SkitInfo skitInfo)
                    {
                        map.Events.SkitInfo.Add(skitInfo);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.PlacementGroup placementGroup)
                    {
                        map.Events.PlacementGroups.Add(placementGroup);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.PartsGroup partsGroup)
                    {
                        map.Events.PartsGroups.Add(partsGroup);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.Talk talk)
                    {
                        map.Events.Talks.Add(talk);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.AutoDrawGroupCollision autoDrawGroupCollision)
                    {
                        map.Events.AutoDrawGroupCollisions.Add(autoDrawGroupCollision);
                    }
                    if (assetInfo.InnerObject is MSBS.Event.Other other)
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
        public static Prefab_SDT ImportJson(string path)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                Prefab_SDT prefab = JsonConvert.DeserializeObject<Prefab_SDT>(File.ReadAllText(path), settings);

                MSBS pseudoMap = MSBS.Read(prefab.AssetContainerBytes);

                // Parts
                foreach (var mapPiece in pseudoMap.Parts.MapPieces)
                {
                    PrefabInfo_SDT info = new(prefab, mapPiece);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var enemy in pseudoMap.Parts.Enemies)
                {
                    PrefabInfo_SDT info = new(prefab, enemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var player in pseudoMap.Parts.Players)
                {
                    PrefabInfo_SDT info = new(prefab, player);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var col in pseudoMap.Parts.Collisions)
                {
                    PrefabInfo_SDT info = new(prefab, col);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyObject in pseudoMap.Parts.DummyObjects)
                {
                    PrefabInfo_SDT info = new(prefab, dummyObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
                {
                    PrefabInfo_SDT info = new(prefab, dummyEnemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
                {
                    PrefabInfo_SDT info = new(prefab, connectCol);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapObject in pseudoMap.Parts.Objects)
                {
                    PrefabInfo_SDT info = new(prefab, mapObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Regions
                foreach (var invasionPoint in pseudoMap.Regions.InvasionPoints)
                {
                    PrefabInfo_SDT info = new(prefab, invasionPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var envMapPoints in pseudoMap.Regions.EnvironmentMapPoints)
                {
                    PrefabInfo_SDT info = new(prefab, envMapPoints);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sound in pseudoMap.Regions.Sounds)
                {
                    PrefabInfo_SDT info = new(prefab, sound);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sfx in pseudoMap.Regions.SFX)
                {
                    PrefabInfo_SDT info = new(prefab, sfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var windSfx in pseudoMap.Regions.WindSFX)
                {
                    PrefabInfo_SDT info = new(prefab, windSfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var spawnPoint in pseudoMap.Regions.SpawnPoints)
                {
                    PrefabInfo_SDT info = new(prefab, spawnPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var patrolRoute in pseudoMap.Regions.PatrolRoutes)
                {
                    PrefabInfo_SDT info = new(prefab, patrolRoute);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var warpPoint in pseudoMap.Regions.WarpPoints)
                {
                    PrefabInfo_SDT info = new(prefab, warpPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var activationArea in pseudoMap.Regions.ActivationAreas)
                {
                    PrefabInfo_SDT info = new(prefab, activationArea);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapEvent in pseudoMap.Regions.Events)
                {
                    PrefabInfo_SDT info = new(prefab, mapEvent);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var logic in pseudoMap.Regions.Logic)
                {
                    PrefabInfo_SDT info = new(prefab, logic);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var envMapEffectBox in pseudoMap.Regions.EnvironmentMapEffectBoxes)
                {
                    PrefabInfo_SDT info = new(prefab, envMapEffectBox);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var windArea in pseudoMap.Regions.WindAreas)
                {
                    PrefabInfo_SDT info = new(prefab, windArea);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mufflingBox in pseudoMap.Regions.MufflingBoxes)
                {
                    PrefabInfo_SDT info = new(prefab, mufflingBox);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mufflingPortal in pseudoMap.Regions.MufflingPortals)
                {
                    PrefabInfo_SDT info = new(prefab, mufflingPortal);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var soundSpaceOverride in pseudoMap.Regions.SoundSpaceOverrides)
                {
                    PrefabInfo_SDT info = new(prefab, soundSpaceOverride);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mufflingPlane in pseudoMap.Regions.MufflingPlanes)
                {
                    PrefabInfo_SDT info = new(prefab, mufflingPlane);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var partsGroupAreas in pseudoMap.Regions.PartsGroupAreas)
                {
                    PrefabInfo_SDT info = new(prefab, partsGroupAreas);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var autoDrawGroupPoints in pseudoMap.Regions.AutoDrawGroupPoints)
                {
                    PrefabInfo_SDT info = new(prefab, autoDrawGroupPoints);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var region in pseudoMap.Regions.Others)
                {
                    PrefabInfo_SDT info = new(prefab, region);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Events
                foreach (var treasure in pseudoMap.Events.Treasures)
                {
                    PrefabInfo_SDT info = new(prefab, treasure);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var generator in pseudoMap.Events.Generators)
                {
                    PrefabInfo_SDT info = new(prefab, generator);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var objAct in pseudoMap.Events.ObjActs)
                {
                    PrefabInfo_SDT info = new(prefab, objAct);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapOffset in pseudoMap.Events.MapOffsets)
                {
                    PrefabInfo_SDT info = new(prefab, mapOffset);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var platoonInfo in pseudoMap.Events.PlatoonInfo)
                {
                    PrefabInfo_SDT info = new(prefab, platoonInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var patrolInfo in pseudoMap.Events.PatrolInfo)
                {
                    PrefabInfo_SDT info = new(prefab, patrolInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var resourceItemInfo in pseudoMap.Events.ResourceItemInfo)
                {
                    PrefabInfo_SDT info = new(prefab, resourceItemInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var grassLodParam in pseudoMap.Events.GrassLodParams)
                {
                    PrefabInfo_SDT info = new(prefab, grassLodParam);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var skitInfo in pseudoMap.Events.SkitInfo)
                {
                    PrefabInfo_SDT info = new(prefab, skitInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var placementGroups in pseudoMap.Events.PlacementGroups)
                {
                    PrefabInfo_SDT info = new(prefab, placementGroups);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var partsGroup in pseudoMap.Events.PartsGroups)
                {
                    PrefabInfo_SDT info = new(prefab, partsGroup);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var talk in pseudoMap.Events.Talks)
                {
                    PrefabInfo_SDT info = new(prefab, talk);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var autoDrawGroupCollision in pseudoMap.Events.AutoDrawGroupCollisions)
                {
                    PrefabInfo_SDT info = new(prefab, autoDrawGroupCollision);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var other in pseudoMap.Events.Others)
                {
                    PrefabInfo_SDT info = new(prefab, other);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                return prefab;
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox(
                    $"Unable to import Prefab_SDT due to the following error:" +
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
            Prefab_SDT _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_SDT.ImportJson(info.Path);
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

            Prefab_SDT prefab = new(_selection.GetFilteredSelection<MsbEntity>());

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
            Prefab_SDT _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_SDT.ImportJson(info.Path);
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

            if (ent.WrappedObject is MSBS.Part.MapPiece mapPiece)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSBS.Part.Enemy enemy || ent.WrappedObject is MSBS.Part.DummyEnemy dummyEnemy)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSBS.Part.Object mapObject || ent.WrappedObject is MSBS.Part.DummyObject dummyObject)
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
            if (ent.WrappedObject is MSBS.Part.MapPiece mapPiece)
            {
                name = $"Map Piece: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.Enemy enemy)
            {
                name = $"Enemy: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.Player player)
            {
                name = $"Player: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.Collision col)
            {
                name = $"Collision: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.DummyObject dummyObject)
            {
                name = $"Dummy Object: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.DummyEnemy dummyEnemy)
            {
                name = $"Dummy Enemy: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.ConnectCollision connectCol)
            {
                name = $"Connect Collision: {name}";
            }
            if (ent.WrappedObject is MSBS.Part.Object mapObject)
            {
                name = $"Object: {name}";
            }

            // Regions
            if (ent.WrappedObject is MSBS.Region.InvasionPoint invasionPoint)
            {
                name = $"Invasion Point: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.EnvironmentMapPoint envMapPoint)
            {
                name = $"Environment Map Point: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.Sound sound)
            {
                name = $"Sound: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.SFX sfx)
            {
                name = $"SFX: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.WindSFX windSfx)
            {
                name = $"Wind SFX: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.SpawnPoint spawnPoint)
            {
                name = $"Spawn Point: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.PatrolRoute patrolRoute)
            {
                name = $"Patrol Route: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.WarpPoint warpPoint)
            {
                name = $"Warp Point: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.ActivationArea activationArea)
            {
                name = $"Activation Area: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.Event mapEvent)
            {
                name = $"Event: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.Logic logic)
            {
                name = $"Logic: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.EnvironmentMapEffectBox envMapEffectbox)
            {
                name = $"Environment Map Effect Box: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.WindArea windArea)
            {
                name = $"Wind Area: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.MufflingBox mufflingBox)
            {
                name = $"Muffling Box: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.MufflingPortal mufflingPortal)
            {
                name = $"Muffling Portal: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.SoundSpaceOverride soundSpaceOverride)
            {
                name = $"Sound Space Override: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.MufflingPlane mufflingPlane)
            {
                name = $"Muffling Plane: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.PartsGroupArea partsGroupArea)
            {
                name = $"Parts Group Area: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.AutoDrawGroupPoint autoDrawGroupPoint)
            {
                name = $"Auto Draw Group Point: {name}";
            }
            if (ent.WrappedObject is MSBS.Region.Other region)
            {
                name = $"Other: {name}";
            }

            // Events
            if (ent.WrappedObject is MSBS.Event.Treasure treasure)
            {
                name = $"Treasure: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.Generator generator)
            {
                name = $"Generator: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.ObjAct objAct)
            {
                name = $"ObjAct: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.MapOffset mapOffset)
            {
                name = $"Map Offset: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.PlatoonInfo platoonInfo)
            {
                name = $"Platoon Info: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.PatrolInfo patrolInfo)
            {
                name = $"Patrol Info: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.ResourceItemInfo resourceItemInfo)
            {
                name = $"Resource Item Info: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.GrassLodParam grassLodParam)
            {
                name = $"Grass LOD Param: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.SkitInfo skitInfo)
            {
                name = $"Skit Info: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.PlacementGroup placementGroup)
            {
                name = $"Placement Group: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.PartsGroup partsGroup)
            {
                name = $"Parts Group: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.Talk talk)
            {
                name = $"Talk: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.AutoDrawGroupCollision autoDrawGroupCollision)
            {
                name = $"Auto Draw Group Collision: {name}";
            }
            if (ent.WrappedObject is MSBS.Event.Other other)
            {
                name = $"Other: {name}";
            }

            return name;
        }
    }
}
