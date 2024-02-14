using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoulsFormats;
using StudioCore.Banks;
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
    public class Prefab_DS3
    {
        public string PrefabName = "";
        public string PrefixSeparator = "[]";
        public ProjectType Type = ProjectType.DS3;

        /// <summary>
        /// Bytes of the MSB that stores prefab data.
        /// </summary>
        public byte[] AssetContainerBytes { get; set; }

        /// <summary>
        /// List of AssetInfo derived from MSB AssetContainerBytes.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<PrefabInfo_DS3> PrefabInfoChildren = new();

        /// <summary>
        /// List of Msb Entities derived from AssetInfoChildren.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<MsbEntity> MsbEntityChildren = new();


        // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
        [Newtonsoft.Json.JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Prefab_DS3()
        { }

        public Prefab_DS3(HashSet<MsbEntity> entities)
        {
            foreach (var ent in entities)
            {
                // Parts
                if (ent.WrappedObject is MSB3.Part.MapPiece mapPiece)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mapPiece));
                }
                if (ent.WrappedObject is MSB3.Part.Object mapObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mapObject));
                }
                if (ent.WrappedObject is MSB3.Part.Enemy enemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, enemy));
                }
                if (ent.WrappedObject is MSB3.Part.Player player)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, player));
                }
                if (ent.WrappedObject is MSB3.Part.Collision col)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, col));
                }
                if (ent.WrappedObject is MSB3.Part.DummyObject dummyObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, dummyObject));
                }
                if (ent.WrappedObject is MSB3.Part.DummyEnemy dummyEnemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, dummyEnemy));
                }
                if (ent.WrappedObject is MSB3.Part.ConnectCollision connectCol)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, connectCol));
                }

                // Regions
                if (ent.WrappedObject is MSB3.Region.InvasionPoint invasionPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, invasionPoint));
                }
                if (ent.WrappedObject is MSB3.Region.EnvironmentMapPoint envMapPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, envMapPoint));
                }
                if (ent.WrappedObject is MSB3.Region.Sound sound)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, sound));
                }
                if (ent.WrappedObject is MSB3.Region.SFX sfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, sfx));
                }
                if (ent.WrappedObject is MSB3.Region.WindSFX windSfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, windSfx));
                }
                if (ent.WrappedObject is MSB3.Region.SpawnPoint spawnPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, spawnPoint));
                }
                if (ent.WrappedObject is MSB3.Region.Message message)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, message));
                }
                if (ent.WrappedObject is MSB3.Region.MovementPoint movementPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, movementPoint));
                }
                if (ent.WrappedObject is MSB3.Region.WarpPoint warpPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, warpPoint));
                }
                if (ent.WrappedObject is MSB3.Region.ActivationArea activationArea)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, activationArea));
                }
                if (ent.WrappedObject is MSB3.Region.Event mapEvent)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mapEvent));
                }
                if (ent.WrappedObject is MSB3.Region.Logic logic)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, logic));
                }
                if (ent.WrappedObject is MSB3.Region.EnvironmentMapEffectBox envMapEffectBox)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, envMapEffectBox));
                }
                if (ent.WrappedObject is MSB3.Region.WindArea windArea)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, windArea));
                }
                if (ent.WrappedObject is MSB3.Region.MufflingBox mufflingBox)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mufflingBox));
                }
                if (ent.WrappedObject is MSB3.Region.MufflingPortal mufflingPortal)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mufflingPortal));
                }
                if (ent.WrappedObject is MSB3.Region.PatrolRoute patrolRoute)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, patrolRoute));
                }
                if (ent.WrappedObject is MSB3.Region.Other region)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, region));
                }

                // Events
                if (ent.WrappedObject is MSB3.Event.Treasure treasure)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, treasure));
                }
                if (ent.WrappedObject is MSB3.Event.Generator generator)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, generator));
                }
                if (ent.WrappedObject is MSB3.Event.ObjAct objAct)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, objAct));
                }
                if (ent.WrappedObject is MSB3.Event.MapOffset mapOffset)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, mapOffset));
                }
                if (ent.WrappedObject is MSB3.Event.PseudoMultiplayer pseudoMultiplayer)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, pseudoMultiplayer));
                }
                if (ent.WrappedObject is MSB3.Event.PlatoonInfo platoonInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, platoonInfo));
                }
                if (ent.WrappedObject is MSB3.Event.PatrolInfo patrolInfo)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, patrolInfo));
                }
                if (ent.WrappedObject is MSB3.Event.Other other)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS3(this, other));
                }
            }
        }
        public class PrefabInfo_DS3
        {

            public Prefab_DS3 Parent;

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
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.MapPiece mapPiece)
            {
                InnerObject = mapPiece.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.MapPiece temp = InnerObject as MSB3.Part.MapPiece;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if(!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.Object mapObject)
            {
                InnerObject = mapObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.Object temp = InnerObject as MSB3.Part.Object;
                temp.CollisionPartIndex = -1;
                temp.CollisionName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.Enemy enemy)
            {
                InnerObject = enemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.Enemy temp = InnerObject as MSB3.Part.Enemy;
                temp.CollisionPartIndex = -1;
                temp.CollisionName = "";
                temp.WalkRouteIndex = -1;
                temp.WalkRouteName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.Player player)
            {
                InnerObject = player.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.Player temp = InnerObject as MSB3.Part.Player;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.Collision collision)
            {
                InnerObject = collision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.Collision temp = InnerObject as MSB3.Part.Collision;
                temp.UnkHitName = "";
                temp.UnkHitIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.DummyObject dummyObject)
            {
                InnerObject = dummyObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.DummyObject temp = InnerObject as MSB3.Part.DummyObject;
                temp.CollisionPartIndex = -1;
                temp.CollisionName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.DummyEnemy dummyEnemy)
            {
                InnerObject = dummyEnemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.DummyEnemy temp = InnerObject as MSB3.Part.DummyEnemy;
                temp.CollisionPartIndex = -1;
                temp.CollisionName = "";
                temp.WalkRouteIndex = -1;
                temp.WalkRouteName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Part.ConnectCollision connectCollision)
            {
                InnerObject = connectCollision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB3.Part.ConnectCollision temp = InnerObject as MSB3.Part.ConnectCollision;
                temp.CollisionName = "";
                temp.CollisionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
                if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
                {
                    Array.Clear(temp.EntityGroups);
                }
            }

            // Regions
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.InvasionPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.InvasionPoint temp = InnerObject as MSB3.Region.InvasionPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.EnvironmentMapPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.EnvironmentMapPoint temp = InnerObject as MSB3.Region.EnvironmentMapPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.Sound region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.Sound temp = InnerObject as MSB3.Region.Sound;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;
                Array.Clear(temp.ChildRegionNames);
                Array.Clear(temp.ChildRegionIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.SFX region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.SFX temp = InnerObject as MSB3.Region.SFX;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.WindSFX region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.WindSFX temp = InnerObject as MSB3.Region.WindSFX;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;
                temp.WindAreaName = "";
                temp.WindAreaIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.SpawnPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.SpawnPoint temp = InnerObject as MSB3.Region.SpawnPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.Message region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.Message temp = InnerObject as MSB3.Region.Message;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.PatrolRoute region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.PatrolRoute temp = InnerObject as MSB3.Region.PatrolRoute;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.MovementPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.MovementPoint temp = InnerObject as MSB3.Region.MovementPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.WarpPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.WarpPoint temp = InnerObject as MSB3.Region.WarpPoint;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.ActivationArea region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.ActivationArea temp = InnerObject as MSB3.Region.ActivationArea;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.Event region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.Event temp = InnerObject as MSB3.Region.Event;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.Logic region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.Logic temp = InnerObject as MSB3.Region.Logic;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.EnvironmentMapEffectBox region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.EnvironmentMapEffectBox temp = InnerObject as MSB3.Region.EnvironmentMapEffectBox;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.WindArea region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.WindArea temp = InnerObject as MSB3.Region.WindArea;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.MufflingBox region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.MufflingBox temp = InnerObject as MSB3.Region.MufflingBox;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.MufflingPortal region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.MufflingPortal temp = InnerObject as MSB3.Region.MufflingPortal;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Region.Other region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB3.Region.Other temp = InnerObject as MSB3.Region.Other;
                temp.ActivationPartName = "";
                temp.ActivationPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }

            // Events
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.Treasure mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.Treasure temp = InnerObject as MSB3.Event.Treasure;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;
                temp.TreasurePartName = "";
                temp.TreasurePartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.Generator mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.Generator temp = InnerObject as MSB3.Event.Generator;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;
                Array.Clear(temp.SpawnPointNames);
                Array.Clear(temp.SpawnPointIndices);
                Array.Clear(temp.SpawnPartNames);
                Array.Clear(temp.SpawnPartIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.ObjAct mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.ObjAct temp = InnerObject as MSB3.Event.ObjAct;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;
                temp.ObjActPartName = "";
                temp.ObjActPartIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.MapOffset mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.MapOffset temp = InnerObject as MSB3.Event.MapOffset;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.PseudoMultiplayer mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.PseudoMultiplayer temp = InnerObject as MSB3.Event.PseudoMultiplayer;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.PlatoonInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.PlatoonInfo temp = InnerObject as MSB3.Event.PlatoonInfo;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;
                Array.Clear(temp.GroupPartsNames);
                Array.Clear(temp.GroupPartsIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.PatrolInfo mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.PatrolInfo temp = InnerObject as MSB3.Event.PatrolInfo;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;
                Array.Clear(temp.WalkPointNames);
                Array.Clear(temp.WalkPointIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS3(Prefab_DS3 parent, MSB3.Event.Other mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB3.Event.Other temp = InnerObject as MSB3.Event.Other;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.PointName = "";
                temp.PointIndex = -1;

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
                    throw new InvalidDataException($"Prefab_DS3 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    throw new InvalidDataException($"Prefab_DS3 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    case PrefabInfo_DS3.AssetInfoDataType.Part:
                        ent.Type = MsbEntity.MsbEntityType.Part;
                        break;
                    case PrefabInfo_DS3.AssetInfoDataType.Region:
                        ent.Type = MsbEntity.MsbEntityType.Region;
                        break;
                    case PrefabInfo_DS3.AssetInfoDataType.Event:
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
                MSB3 map = new();
                foreach (var assetInfo in PrefabInfoChildren)
                {
                    assetInfo.StripNamePrefix();

                    // Parts
                    if (assetInfo.InnerObject is MSB3.Part.Enemy enemy)
                    {
                        map.Parts.Enemies.Add(enemy);
                        MSB3.Model.Enemy model = new();
                        model.Name = enemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.Object mapObject)
                    {
                        map.Parts.Objects.Add(mapObject);
                        MSB3.Model.Object model = new();
                        model.Name = mapObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.MapPiece mapPiece)
                    {
                        map.Parts.MapPieces.Add(mapPiece);
                        MSB3.Model.MapPiece model = new();
                        model.Name = mapPiece.ModelName;
                        map.Models.MapPieces.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.Player player)
                    {
                        map.Parts.Players.Add(player);
                        MSB3.Model.Player model = new();
                        model.Name = player.ModelName;
                        map.Models.Players.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.Collision collision)
                    {
                        map.Parts.Collisions.Add(collision);
                        MSB3.Model.Collision model = new();
                        model.Name = collision.ModelName;
                        map.Models.Collisions.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.DummyObject dummyObject)
                    {
                        map.Parts.DummyObjects.Add(dummyObject);
                        MSB3.Model.Object model = new();
                        model.Name = dummyObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.DummyEnemy dummyEnemy)
                    {
                        map.Parts.DummyEnemies.Add(dummyEnemy);
                        MSB3.Model.Enemy model = new();
                        model.Name = dummyEnemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB3.Part.ConnectCollision connectCol)
                    {
                        map.Parts.ConnectCollisions.Add(connectCol);
                        MSB3.Model.Collision model = new();
                        model.Name = connectCol.ModelName;
                        map.Models.Collisions.Add(model);
                    }

                    // Regions
                    if (assetInfo.InnerObject is MSB3.Region.InvasionPoint invasionPoint)
                    {
                        map.Regions.InvasionPoints.Add(invasionPoint);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.EnvironmentMapPoint envMapPoint)
                    {
                        map.Regions.EnvironmentMapPoints.Add(envMapPoint);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.Sound sound)
                    {
                        map.Regions.Sounds.Add(sound);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.SFX sfx)
                    {
                        map.Regions.SFX.Add(sfx);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.WindSFX windSfx)
                    {
                        map.Regions.WindSFX.Add(windSfx);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.SpawnPoint spawnPoint)
                    {
                        map.Regions.SpawnPoints.Add(spawnPoint);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.Message message)
                    {
                        map.Regions.Messages.Add(message);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.MovementPoint movementPoint)
                    {
                        map.Regions.MovementPoints.Add(movementPoint);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.WarpPoint warpPoint)
                    {
                        map.Regions.WarpPoints.Add(warpPoint);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.ActivationArea activationArea)
                    {
                        map.Regions.ActivationAreas.Add(activationArea);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.Event mapEvent)
                    {
                        map.Regions.Events.Add(mapEvent);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.Logic logic)
                    {
                        map.Regions.Logic.Add(logic);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.EnvironmentMapEffectBox envMapEffectBox)
                    {
                        map.Regions.EnvironmentMapEffectBoxes.Add(envMapEffectBox);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.WindArea windArea)
                    {
                        map.Regions.WindAreas.Add(windArea);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.MufflingBox mufflingBox)
                    {
                        map.Regions.MufflingBoxes.Add(mufflingBox);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.MufflingPortal mufflingPortal)
                    {
                        map.Regions.MufflingPortals.Add(mufflingPortal);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.PatrolRoute patrolRoute)
                    {
                        map.Regions.PatrolRoutes.Add(patrolRoute);
                    }
                    if (assetInfo.InnerObject is MSB3.Region.Other region)
                    {
                        map.Regions.Others.Add(region);
                    }

                    // Events
                    if (assetInfo.InnerObject is MSB3.Event.Treasure treasure)
                    {
                        map.Events.Treasures.Add(treasure);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.Generator generator)
                    {
                        map.Events.Generators.Add(generator);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.ObjAct objAct)
                    {
                        map.Events.ObjActs.Add(objAct);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.MapOffset mapOffset)
                    {
                        map.Events.MapOffsets.Add(mapOffset);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.PseudoMultiplayer pseudoMultiplayer)
                    {
                        map.Events.PseudoMultiplayers.Add(pseudoMultiplayer);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.PlatoonInfo platoonInfo)
                    {
                        map.Events.PlatoonInfo.Add(platoonInfo);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.PatrolInfo patrolInfo)
                    {
                        map.Events.PatrolInfo.Add(patrolInfo);
                    }
                    if (assetInfo.InnerObject is MSB3.Event.Other other)
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
        public static Prefab_DS3 ImportJson(string path)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                Prefab_DS3 prefab = JsonConvert.DeserializeObject<Prefab_DS3>(File.ReadAllText(path), settings);

                MSB3 pseudoMap = MSB3.Read(prefab.AssetContainerBytes);

                // Parts
                foreach (var mapPiece in pseudoMap.Parts.MapPieces)
                {
                    PrefabInfo_DS3 info = new(prefab, mapPiece);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var enemy in pseudoMap.Parts.Enemies)
                {
                    PrefabInfo_DS3 info = new(prefab, enemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var player in pseudoMap.Parts.Players)
                {
                    PrefabInfo_DS3 info = new(prefab, player);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var col in pseudoMap.Parts.Collisions)
                {
                    PrefabInfo_DS3 info = new(prefab, col);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyObject in pseudoMap.Parts.DummyObjects)
                {
                    PrefabInfo_DS3 info = new(prefab, dummyObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
                {
                    PrefabInfo_DS3 info = new(prefab, dummyEnemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
                {
                    PrefabInfo_DS3 info = new(prefab, connectCol);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapObject in pseudoMap.Parts.Objects)
                {
                    PrefabInfo_DS3 info = new(prefab, mapObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Regions
                foreach (var invasionPoint in pseudoMap.Regions.InvasionPoints)
                {
                    PrefabInfo_DS3 info = new(prefab, invasionPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var envMapPoints in pseudoMap.Regions.EnvironmentMapPoints)
                {
                    PrefabInfo_DS3 info = new(prefab, envMapPoints);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sound in pseudoMap.Regions.Sounds)
                {
                    PrefabInfo_DS3 info = new(prefab, sound);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sfx in pseudoMap.Regions.SFX)
                {
                    PrefabInfo_DS3 info = new(prefab, sfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var windSfx in pseudoMap.Regions.WindSFX)
                {
                    PrefabInfo_DS3 info = new(prefab, windSfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var spawnPoint in pseudoMap.Regions.SpawnPoints)
                {
                    PrefabInfo_DS3 info = new(prefab, spawnPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var message in pseudoMap.Regions.Messages)
                {
                    PrefabInfo_DS3 info = new(prefab, message);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var movementPoint in pseudoMap.Regions.MovementPoints)
                {
                    PrefabInfo_DS3 info = new(prefab, movementPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var warpPoint in pseudoMap.Regions.WarpPoints)
                {
                    PrefabInfo_DS3 info = new(prefab, warpPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var activationArea in pseudoMap.Regions.ActivationAreas)
                {
                    PrefabInfo_DS3 info = new(prefab, activationArea);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapEvent in pseudoMap.Regions.Events)
                {
                    PrefabInfo_DS3 info = new(prefab, mapEvent);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var logic in pseudoMap.Regions.Logic)
                {
                    PrefabInfo_DS3 info = new(prefab, logic);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var envMapEffectBox in pseudoMap.Regions.EnvironmentMapEffectBoxes)
                {
                    PrefabInfo_DS3 info = new(prefab, envMapEffectBox);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var windArea in pseudoMap.Regions.WindAreas)
                {
                    PrefabInfo_DS3 info = new(prefab, windArea);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mufflingBox in pseudoMap.Regions.MufflingBoxes)
                {
                    PrefabInfo_DS3 info = new(prefab, mufflingBox);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mufflingPortal in pseudoMap.Regions.MufflingPortals)
                {
                    PrefabInfo_DS3 info = new(prefab, mufflingPortal);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var patrolRoute in pseudoMap.Regions.PatrolRoutes)
                {
                    PrefabInfo_DS3 info = new(prefab, patrolRoute);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var region in pseudoMap.Regions.Others)
                {
                    PrefabInfo_DS3 info = new(prefab, region);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Events
                foreach (var treasure in pseudoMap.Events.Treasures)
                {
                    PrefabInfo_DS3 info = new(prefab, treasure);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var generator in pseudoMap.Events.Generators)
                {
                    PrefabInfo_DS3 info = new(prefab, generator);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var objAct in pseudoMap.Events.ObjActs)
                {
                    PrefabInfo_DS3 info = new(prefab, objAct);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapOffset in pseudoMap.Events.MapOffsets)
                {
                    PrefabInfo_DS3 info = new(prefab, mapOffset);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var pseudoMultiplayer in pseudoMap.Events.PseudoMultiplayers)
                {
                    PrefabInfo_DS3 info = new(prefab, pseudoMultiplayer);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var platoonInfo in pseudoMap.Events.PlatoonInfo)
                {
                    PrefabInfo_DS3 info = new(prefab, platoonInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var patrolInfo in pseudoMap.Events.PatrolInfo)
                {
                    PrefabInfo_DS3 info = new(prefab, patrolInfo);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var other in pseudoMap.Events.Others)
                {
                    PrefabInfo_DS3 info = new(prefab, other);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                return prefab;
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox(
                    $"Unable to import Prefab_DS3 due to the following error:" +
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
            Prefab_DS3 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS3.ImportJson(info.Path);
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
        public static void ExportSelection(string filepath, MapSelection _selection)
        {
            Prefab_DS3 prefab = new(_selection.GetFilteredSelection<MsbEntity>());

            if (!prefab.PrefabInfoChildren.Any())
            {
                PlatformUtils.Instance.MessageBox("Export failed, nothing in selection could be exported.", "Prefab Error", MessageBoxButtons.OK);
            }
            else
            {
                prefab.PrefabName = System.IO.Path.GetFileNameWithoutExtension(filepath);
                prefab.Write(filepath);
            }
        }

        public static List<string> GetSelectedPrefabObjects(PrefabInfo info, (string, MapObjectContainer) _comboTargetMap)
        {
            List<string> entNames = new List<string>();
            Prefab_DS3 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS3.ImportJson(info.Path);
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

            if (ent.WrappedObject is MSB3.Part.MapPiece mapPiece)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSB3.Part.Enemy enemy || ent.WrappedObject is MSB3.Part.DummyEnemy dummyEnemy)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSB3.Part.Object mapObject || ent.WrappedObject is MSB3.Part.DummyObject dummyObject)
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
            if (ent.WrappedObject is MSB3.Part.MapPiece mapPiece)
            {
                name = $"Map Piece: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.Enemy enemy)
            {
                name = $"Enemy: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.Player player)
            {
                name = $"Player: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.Collision col)
            {
                name = $"Collision: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.DummyObject dummyObject)
            {
                name = $"Dummy Object: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.DummyEnemy dummyEnemy)
            {
                name = $"Dummy Enemy: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.ConnectCollision connectCol)
            {
                name = $"Connect Collision: {name}";
            }
            if (ent.WrappedObject is MSB3.Part.Object mapObject)
            {
                name = $"Object: {name}";
            }

            // Regions
            if (ent.WrappedObject is MSB3.Region.InvasionPoint invasionPoint)
            {
                name = $"Invasion Point: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.EnvironmentMapPoint envMapPoint)
            {
                name = $"Environment Map Point: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.Sound sound)
            {
                name = $"Sound: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.SFX sfx)
            {
                name = $"SFX: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.WindSFX windSfx)
            {
                name = $"Wind SFX: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.SpawnPoint spawnPoint)
            {
                name = $"Spawn Point: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.Message message)
            {
                name = $"Message: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.MovementPoint movementPoint)
            {
                name = $"Movement Point: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.WarpPoint warpPoint)
            {
                name = $"Warp Point: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.ActivationArea activationArea)
            {
                name = $"Activation Area: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.Event mapEvent)
            {
                name = $"Event: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.Logic logic)
            {
                name = $"Logic: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.EnvironmentMapEffectBox envMapEffectbox)
            {
                name = $"Environment Map Effect Box: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.WindArea windArea)
            {
                name = $"Wind Area: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.MufflingBox mufflingBox)
            {
                name = $"Muffling Box: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.MufflingPortal mufflingPortal)
            {
                name = $"Muffling Portal: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.PatrolRoute patrolRoute)
            {
                name = $"Patrol Route: {name}";
            }
            if (ent.WrappedObject is MSB3.Region.Other region)
            {
                name = $"Other: {name}";
            }

            // Events
            if (ent.WrappedObject is MSB3.Event.Treasure treasure)
            {
                name = $"Treasure: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.Generator generator)
            {
                name = $"Generator: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.ObjAct objAct)
            {
                name = $"ObjAct: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.MapOffset mapOffset)
            {
                name = $"Map Offset: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.PseudoMultiplayer pseudoMultiplayer)
            {
                name = $"Pseudo Multiplayer: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.PlatoonInfo platoonInfo)
            {
                name = $"Platoon Info: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.PatrolInfo patrolInfo)
            {
                name = $"Patrol Info: {name}";
            }
            if (ent.WrappedObject is MSB3.Event.Other other)
            {
                name = $"Other: {name}";
            }

            return name;
        }
    }
}
