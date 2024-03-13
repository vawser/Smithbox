using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.BanksMain;
using StudioCore.Editor;
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
    public class Prefab_DS1
    {
        public string PrefabName = "";
        public string PrefixSeparator = "[]";
        public ProjectType Type = ProjectType.DS1;


        public List<string> TagList;
        /// <summary>
        /// Bytes of the MSB that stores prefab data.
        /// </summary>
        public byte[] AssetContainerBytes { get; set; }

        /// <summary>
        /// List of AssetInfo derived from MSB AssetContainerBytes.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<PrefabInfo_DS1> PrefabInfoChildren = new();

        /// <summary>
        /// List of Msb Entities derived from AssetInfoChildren.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<MsbEntity> MsbEntityChildren = new();


        // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
        [Newtonsoft.Json.JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Prefab_DS1()
        { }

        public Prefab_DS1(HashSet<MsbEntity> entities)
        {
            foreach (var ent in entities)
            {
                // Parts
                if (ent.WrappedObject is MSB1.Part.MapPiece mapPiece)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, mapPiece));
                }
                if (ent.WrappedObject is MSB1.Part.Object mapObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, mapObject));
                }
                if (ent.WrappedObject is MSB1.Part.Enemy enemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, enemy));
                }
                if (ent.WrappedObject is MSB1.Part.Player player)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, player));
                }
                if (ent.WrappedObject is MSB1.Part.Navmesh partNavMesh)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, partNavMesh));
                }
                if (ent.WrappedObject is MSB1.Part.Collision col)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, col));
                }
                if (ent.WrappedObject is MSB1.Part.DummyObject dummyObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, dummyObject));
                }
                if (ent.WrappedObject is MSB1.Part.DummyEnemy dummyEnemy)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, dummyEnemy));
                }
                if (ent.WrappedObject is MSB1.Part.ConnectCollision connectCol)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, connectCol));
                }

                // Regions
                if (ent.WrappedObject is MSB1.Region region)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, region));
                }

                // Events
                if (ent.WrappedObject is MSB1.Event.Light light)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, light));
                }
                if (ent.WrappedObject is MSB1.Event.Sound sound)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, sound));
                }
                if (ent.WrappedObject is MSB1.Event.SFX sfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, sfx));
                }
                if (ent.WrappedObject is MSB1.Event.Wind wind)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, wind));
                }
                if (ent.WrappedObject is MSB1.Event.Treasure treasure)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, treasure));
                }
                if (ent.WrappedObject is MSB1.Event.Generator generator)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, generator));
                }
                if (ent.WrappedObject is MSB1.Event.Message message)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, message));
                }
                if (ent.WrappedObject is MSB1.Event.ObjAct objAct)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, objAct));
                }
                if (ent.WrappedObject is MSB1.Event.SpawnPoint spawnPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, spawnPoint));
                }
                if (ent.WrappedObject is MSB1.Event.MapOffset mapOffset)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, mapOffset));
                }
                if (ent.WrappedObject is MSB1.Event.Navmesh evtNavmesh)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, evtNavmesh));
                }
                if (ent.WrappedObject is MSB1.Event.Environment env)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, env));
                }
                if (ent.WrappedObject is MSB1.Event.PseudoMultiplayer pseudoMultiplayer)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS1(this, pseudoMultiplayer));
                }
            }
        }
        public class PrefabInfo_DS1
        {

            public Prefab_DS1 Parent;

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
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.MapPiece mapPiece)
            {
                InnerObject = mapPiece.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.MapPiece temp = InnerObject as MSB1.Part.MapPiece;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.Object mapObject)
            {
                InnerObject = mapObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.Object temp = InnerObject as MSB1.Part.Object;
                temp.CollisionIndex = -1;
                temp.CollisionName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.Enemy enemy)
            {
                InnerObject = enemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.Enemy temp = InnerObject as MSB1.Part.Enemy;
                temp.CollisionIndex = -1;
                temp.CollisionName = "";
                Array.Clear(temp.MovePointIndices);
                Array.Clear(temp.MovePointNames);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.Player player)
            {
                InnerObject = player.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.Player temp = InnerObject as MSB1.Part.Player;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.Collision collision)
            {
                InnerObject = collision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.Collision temp = InnerObject as MSB1.Part.Collision;
                temp.EnvLightMapSpotName = "";
                temp.EnvLightMapSpotIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.DummyObject dummyObject)
            {
                InnerObject = dummyObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.DummyObject temp = InnerObject as MSB1.Part.DummyObject;
                temp.CollisionIndex = -1;
                temp.CollisionName = "";

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.DummyEnemy dummyEnemy)
            {
                InnerObject = dummyEnemy.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.DummyEnemy temp = InnerObject as MSB1.Part.DummyEnemy;
                temp.CollisionIndex = -1;
                temp.CollisionName = "";
                Array.Clear(temp.MovePointIndices);
                Array.Clear(temp.MovePointNames);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.ConnectCollision connectCollision)
            {
                InnerObject = connectCollision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.ConnectCollision temp = InnerObject as MSB1.Part.ConnectCollision;
                temp.CollisionName = "";
                temp.CollisionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Part.Navmesh navmesh)
            {
                InnerObject = navmesh.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB1.Part.Navmesh temp = InnerObject as MSB1.Part.Navmesh;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }

            // Regions
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Region region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB1.Region temp = InnerObject as MSB1.Region;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }

            // Events
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Light mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Light temp = InnerObject as MSB1.Event.Light;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Sound mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Sound temp = InnerObject as MSB1.Event.Sound;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.SFX mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.SFX temp = InnerObject as MSB1.Event.SFX;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Wind mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Wind temp = InnerObject as MSB1.Event.Wind;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Treasure mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Treasure temp = InnerObject as MSB1.Event.Treasure;
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
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Generator mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Generator temp = InnerObject as MSB1.Event.Generator;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                Array.Clear(temp.SpawnPointNames);
                Array.Clear(temp.SpawnPointIndices);
                Array.Clear(temp.SpawnPartNames);
                Array.Clear(temp.SpawnPartIndices);

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Message mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Message temp = InnerObject as MSB1.Event.Message;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.ObjAct mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.ObjAct temp = InnerObject as MSB1.Event.ObjAct;
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
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.SpawnPoint mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.SpawnPoint temp = InnerObject as MSB1.Event.SpawnPoint;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;
                temp.SpawnPointName = "";
                temp.SpawnPointIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.MapOffset mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.MapOffset temp = InnerObject as MSB1.Event.MapOffset;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Navmesh mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Navmesh temp = InnerObject as MSB1.Event.Navmesh;
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
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.Environment mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.Environment temp = InnerObject as MSB1.Event.Environment;
                temp.PartName = "";
                temp.PartIndex = -1;
                temp.RegionName = "";
                temp.RegionIndex = -1;

                if (!CFG.Current.Prefab_IncludeEntityID)
                {
                    temp.EntityID = 0;
                }
            }
            public PrefabInfo_DS1(Prefab_DS1 parent, MSB1.Event.PseudoMultiplayer mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB1.Event.PseudoMultiplayer temp = InnerObject as MSB1.Event.PseudoMultiplayer;
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
                    throw new InvalidDataException($"Prefab_DS1 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    throw new InvalidDataException($"Prefab_DS1 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    case PrefabInfo_DS1.AssetInfoDataType.Part:
                        ent.Type = MsbEntity.MsbEntityType.Part;
                        break;
                    case PrefabInfo_DS1.AssetInfoDataType.Region:
                        ent.Type = MsbEntity.MsbEntityType.Region;
                        break;
                    case PrefabInfo_DS1.AssetInfoDataType.Event:
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
                MSB1 map = new();
                foreach (var assetInfo in PrefabInfoChildren)
                {
                    assetInfo.StripNamePrefix();

                    // Parts
                    if (assetInfo.InnerObject is MSB1.Part.Enemy enemy)
                    {
                        map.Parts.Enemies.Add(enemy);
                        MSB1.Model.Enemy model = new();
                        model.Name = enemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.Object mapObject)
                    {
                        map.Parts.Objects.Add(mapObject);
                        MSB1.Model.Object model = new();
                        model.Name = mapObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.MapPiece mapPiece)
                    {
                        map.Parts.MapPieces.Add(mapPiece);
                        MSB1.Model.MapPiece model = new();
                        model.Name = mapPiece.ModelName;
                        map.Models.MapPieces.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.Player player)
                    {
                        map.Parts.Players.Add(player);
                        MSB1.Model.Player model = new();
                        model.Name = player.ModelName;
                        map.Models.Players.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.Collision collision)
                    {
                        map.Parts.Collisions.Add(collision);
                        MSB1.Model.Collision model = new();
                        model.Name = collision.ModelName;
                        map.Models.Collisions.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.DummyObject dummyObject)
                    {
                        map.Parts.DummyObjects.Add(dummyObject);
                        MSB1.Model.Object model = new();
                        model.Name = dummyObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.DummyEnemy dummyEnemy)
                    {
                        map.Parts.DummyEnemies.Add(dummyEnemy);
                        MSB1.Model.Enemy model = new();
                        model.Name = dummyEnemy.ModelName;
                        map.Models.Enemies.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.ConnectCollision connectCol)
                    {
                        map.Parts.ConnectCollisions.Add(connectCol);
                        MSB1.Model.Collision model = new();
                        model.Name = connectCol.ModelName;
                        map.Models.Collisions.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB1.Part.Navmesh navmesh)
                    {
                        map.Parts.Navmeshes.Add(navmesh);
                        MSB1.Model.Navmesh model = new();
                        model.Name = navmesh.ModelName;
                        map.Models.Navmeshes.Add(model);
                    }

                    // Regions
                    if (assetInfo.InnerObject is MSB1.Region region)
                    {
                        map.Regions.Add(region);
                    }

                    // Events
                    if (assetInfo.InnerObject is MSB1.Event.Light light)
                    {
                        map.Events.Lights.Add(light);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Sound sound)
                    {
                        map.Events.Sounds.Add(sound);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.SFX sfx)
                    {
                        map.Events.SFX.Add(sfx);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Wind wind)
                    {
                        map.Events.Wind.Add(wind);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Treasure treasure)
                    {
                        map.Events.Treasures.Add(treasure);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Generator generator)
                    {
                        map.Events.Generators.Add(generator);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Message message)
                    {
                        map.Events.Messages.Add(message);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.ObjAct objAct)
                    {
                        map.Events.ObjActs.Add(objAct);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.SpawnPoint spawnPoint)
                    {
                        map.Events.SpawnPoints.Add(spawnPoint);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.MapOffset mapOffset)
                    {
                        map.Events.MapOffsets.Add(mapOffset);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Navmesh evtNavmesh)
                    {
                        map.Events.Navmeshes.Add(evtNavmesh);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.Environment env)
                    {
                        map.Events.Environments.Add(env);
                    }
                    if (assetInfo.InnerObject is MSB1.Event.PseudoMultiplayer pseudoMultiplayer)
                    {
                        map.Events.PseudoMultiplayers.Add(pseudoMultiplayer);
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
        public static Prefab_DS1 ImportJson(string path)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                Prefab_DS1 prefab = JsonConvert.DeserializeObject<Prefab_DS1>(File.ReadAllText(path), settings);

                MSB1 pseudoMap = MSB1.Read(prefab.AssetContainerBytes);

                // Parts
                foreach (var mapPiece in pseudoMap.Parts.MapPieces)
                {
                    PrefabInfo_DS1 info = new(prefab, mapPiece);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var enemy in pseudoMap.Parts.Enemies)
                {
                    PrefabInfo_DS1 info = new(prefab, enemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var player in pseudoMap.Parts.Players)
                {
                    PrefabInfo_DS1 info = new(prefab, player);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var col in pseudoMap.Parts.Collisions)
                {
                    PrefabInfo_DS1 info = new(prefab, col);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyObject in pseudoMap.Parts.DummyObjects)
                {
                    PrefabInfo_DS1 info = new(prefab, dummyObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
                {
                    PrefabInfo_DS1 info = new(prefab, dummyEnemy);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
                {
                    PrefabInfo_DS1 info = new(prefab, connectCol);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapObject in pseudoMap.Parts.Objects)
                {
                    PrefabInfo_DS1 info = new(prefab, mapObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapObject in pseudoMap.Parts.Navmeshes)
                {
                    PrefabInfo_DS1 info = new(prefab, mapObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Regions
                foreach (var region in pseudoMap.Regions.Regions)
                {
                    PrefabInfo_DS1 info = new(prefab, region);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Events
                foreach (var light in pseudoMap.Events.Lights)
                {
                    PrefabInfo_DS1 info = new(prefab, light);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sound in pseudoMap.Events.Sounds)
                {
                    PrefabInfo_DS1 info = new(prefab, sound);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sfx in pseudoMap.Events.SFX)
                {
                    PrefabInfo_DS1 info = new(prefab, sfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var wind in pseudoMap.Events.Wind)
                {
                    PrefabInfo_DS1 info = new(prefab, wind);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var treasure in pseudoMap.Events.Treasures)
                {
                    PrefabInfo_DS1 info = new(prefab, treasure);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var generator in pseudoMap.Events.Generators)
                {
                    PrefabInfo_DS1 info = new(prefab, generator);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var message in pseudoMap.Events.Messages)
                {
                    PrefabInfo_DS1 info = new(prefab, message);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var objAct in pseudoMap.Events.ObjActs)
                {
                    PrefabInfo_DS1 info = new(prefab, objAct);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var spawnPoint in pseudoMap.Events.SpawnPoints)
                {
                    PrefabInfo_DS1 info = new(prefab, spawnPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapOffset in pseudoMap.Events.MapOffsets)
                {
                    PrefabInfo_DS1 info = new(prefab, mapOffset);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var navMesh in pseudoMap.Events.Navmeshes)
                {
                    PrefabInfo_DS1 info = new(prefab, navMesh);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var env in pseudoMap.Events.Environments)
                {
                    PrefabInfo_DS1 info = new(prefab, env);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var pseudoMultiplayer in pseudoMap.Events.PseudoMultiplayers)
                {
                    PrefabInfo_DS1 info = new(prefab, pseudoMultiplayer);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                return prefab;
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox(
                    $"Unable to import Prefab_DS1 due to the following error:" +
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
            Prefab_DS1 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS1.ImportJson(info.Path);
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

            Prefab_DS1 prefab = new(_selection.GetFilteredSelection<MsbEntity>());

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
            Prefab_DS1 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS1.ImportJson(info.Path);
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

            if (ent.WrappedObject is MSB1.Part.MapPiece mapPiece)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSB1.Part.Enemy enemy || ent.WrappedObject is MSB1.Part.DummyEnemy dummyEnemy)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("Characters"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSB1.Part.Object mapObject || ent.WrappedObject is MSB1.Part.DummyObject dummyObject)
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
            if (ent.WrappedObject is MSB1.Part.MapPiece mapPiece)
            {
                name = $"Map Piece: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.Enemy enemy)
            {
                name = $"Enemy: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.Player player)
            {
                name = $"Player: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.Collision col)
            {
                name = $"Collision: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.DummyObject dummyObject)
            {
                name = $"Dummy Object: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.DummyEnemy dummyEnemy)
            {
                name = $"Dummy Enemy: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.ConnectCollision connectCol)
            {
                name = $"Connect Collision: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.Object mapObject)
            {
                name = $"Object: {name}";
            }
            if (ent.WrappedObject is MSB1.Part.Navmesh navmesh)
            {
                name = $"Navmesh: {name}";
            }

            // Regions
            if (ent.WrappedObject is MSB1.Region region)
            {
                name = $"Region: {name}";
            }

            // Events
            if (ent.WrappedObject is MSB1.Event.Light light)
            {
                name = $"Light: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Sound sound)
            {
                name = $"Sound: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.SFX sfx)
            {
                name = $"SFX: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Wind wind)
            {
                name = $"Wind: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Treasure treasure)
            {
                name = $"Treasure: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Generator generator)
            {
                name = $"Generator: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Message message)
            {
                name = $"Message: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.ObjAct objAct)
            {
                name = $"ObjAct: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.SpawnPoint spawnPoint)
            {
                name = $"Spawn Point: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.MapOffset mapOffset)
            {
                name = $"Map Offset: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Navmesh evtNavmesh)
            {
                name = $"Navmesh: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.Environment env)
            {
                name = $"Environment: {name}";
            }
            if (ent.WrappedObject is MSB1.Event.PseudoMultiplayer pseudoMultiplayer)
            {
                name = $"Pseudo Multiplayer: {name}";
            }

            return name;
        }
    }
}

