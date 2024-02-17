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
    public class Prefab_DS2
    {
        public string PrefabName = "";
        public string PrefixSeparator = "[]";
        public ProjectType Type = ProjectType.DS2S;

        public List<string> TagList;

        /// <summary>
        /// Bytes of the MSB that stores prefab data.
        /// </summary>
        public byte[] AssetContainerBytes { get; set; }

        /// <summary>
        /// List of AssetInfo derived from MSB AssetContainerBytes.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<PrefabInfo_DS2> PrefabInfoChildren = new();

        /// <summary>
        /// List of Msb Entities derived from AssetInfoChildren.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public List<MsbEntity> MsbEntityChildren = new();


        // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
        [Newtonsoft.Json.JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Prefab_DS2()
        { }

        public Prefab_DS2(HashSet<MsbEntity> entities)
        {
            foreach (var ent in entities)
            {
                // Parts
                if (ent.WrappedObject is MSB2.Part.MapPiece mapPiece)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, mapPiece));
                }
                if (ent.WrappedObject is MSB2.Part.Object mapObject)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, mapObject));
                }
                if (ent.WrappedObject is MSB2.Part.Collision col)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, col));
                }
                if (ent.WrappedObject is MSB2.Part.Navmesh navmesh)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, navmesh));
                }
                if (ent.WrappedObject is MSB2.Part.ConnectCollision connectCol)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, connectCol));
                }

                // Regions
                if (ent.WrappedObject is MSB2.Region.Region0 regionZero)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, regionZero));
                }
                if (ent.WrappedObject is MSB2.Region.Light light)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, light));
                }
                if (ent.WrappedObject is MSB2.Region.StartPoint startPoint)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, startPoint));
                }
                if (ent.WrappedObject is MSB2.Region.Sound sound)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, sound));
                }
                if (ent.WrappedObject is MSB2.Region.SFX sfx)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, sfx));
                }
                if (ent.WrappedObject is MSB2.Region.Wind wind)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, wind));
                }
                if (ent.WrappedObject is MSB2.Region.EnvLight envLight)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, envLight));
                }
                if (ent.WrappedObject is MSB2.Region.Fog fog)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, fog));
                }

                // Events
                if (ent.WrappedObject is MSB2.Event.Light evtLight)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, evtLight));
                }
                if (ent.WrappedObject is MSB2.Event.Shadow shadow)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, shadow));
                }
                if (ent.WrappedObject is MSB2.Event.Fog evtFog)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, evtFog));
                }
                if (ent.WrappedObject is MSB2.Event.BGColor bgColor)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, bgColor));
                }
                if (ent.WrappedObject is MSB2.Event.MapOffset mapOffset)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, mapOffset));
                }
                if (ent.WrappedObject is MSB2.Event.Warp warp)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, warp));
                }
                if (ent.WrappedObject is MSB2.Event.CheapMode cheapMode)
                {
                    PrefabInfoChildren.Add(new PrefabInfo_DS2(this, cheapMode));
                }
            }
        }
        public class PrefabInfo_DS2
        {

            public Prefab_DS2 Parent;

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
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Part.MapPiece mapPiece)
            {
                InnerObject = mapPiece.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB2.Part.MapPiece temp = InnerObject as MSB2.Part.MapPiece;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Part.Object mapObject)
            {
                InnerObject = mapObject.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB2.Part.Object temp = InnerObject as MSB2.Part.Object;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Part.Collision collision)
            {
                InnerObject = collision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB2.Part.Collision temp = InnerObject as MSB2.Part.Collision;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Part.Navmesh navMesh)
            {
                InnerObject = navMesh.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB2.Part.Object temp = InnerObject as MSB2.Part.Object;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Part.ConnectCollision connectCollision)
            {
                InnerObject = connectCollision.DeepCopy();
                DataType = AssetInfoDataType.Part;
                Parent = parent;

                MSB2.Part.ConnectCollision temp = InnerObject as MSB2.Part.ConnectCollision;
                temp.CollisionName = "";
                temp.CollisionIndex = -1;
            }

            // Regions
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.Region0 region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.Region0 temp = InnerObject as MSB2.Region.Region0;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.Light region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.Light temp = InnerObject as MSB2.Region.Light;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.StartPoint region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.StartPoint temp = InnerObject as MSB2.Region.StartPoint;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.Sound region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.Sound temp = InnerObject as MSB2.Region.Sound;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.SFX region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.SFX temp = InnerObject as MSB2.Region.SFX;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.Wind region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.Wind temp = InnerObject as MSB2.Region.Wind;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.EnvLight region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.EnvLight temp = InnerObject as MSB2.Region.EnvLight;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Region.Fog region)
            {
                InnerObject = region.DeepCopy();
                DataType = AssetInfoDataType.Region;
                Parent = parent;

                MSB2.Region.Fog temp = InnerObject as MSB2.Region.Fog;
            }

            // Events
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.Light mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.Light temp = InnerObject as MSB2.Event.Light;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.Shadow mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.Shadow temp = InnerObject as MSB2.Event.Shadow;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.Fog mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.Fog temp = InnerObject as MSB2.Event.Fog;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.BGColor mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.BGColor temp = InnerObject as MSB2.Event.BGColor;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.MapOffset mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.MapOffset temp = InnerObject as MSB2.Event.MapOffset;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.Warp mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.Warp temp = InnerObject as MSB2.Event.Warp;
            }
            public PrefabInfo_DS2(Prefab_DS2 parent, MSB2.Event.CheapMode mapEvent)
            {
                InnerObject = mapEvent.DeepCopy();
                DataType = AssetInfoDataType.Event;
                Parent = parent;

                MSB2.Event.CheapMode temp = InnerObject as MSB2.Event.CheapMode;
            }

            public void AddNamePrefix(string prefix)
            {
                var prop = InnerObject.GetType().GetProperty("Name");
                if (prop == null)
                {
                    throw new InvalidDataException($"Prefab_DS2 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    throw new InvalidDataException($"Prefab_DS2 operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                    case PrefabInfo_DS2.AssetInfoDataType.Part:
                        ent.Type = MsbEntity.MsbEntityType.Part;
                        break;
                    case PrefabInfo_DS2.AssetInfoDataType.Region:
                        ent.Type = MsbEntity.MsbEntityType.Region;
                        break;
                    case PrefabInfo_DS2.AssetInfoDataType.Event:
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
                MSB2 map = new();
                foreach (var assetInfo in PrefabInfoChildren)
                {
                    assetInfo.StripNamePrefix();

                    // Parts
                    if (assetInfo.InnerObject is MSB2.Part.MapPiece mapPiece)
                    {
                        map.Parts.MapPieces.Add(mapPiece);
                        MSB2.Model.MapPiece model = new();
                        model.Name = mapPiece.ModelName;
                        map.Models.MapPieces.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB2.Part.Object mapObject)
                    {
                        map.Parts.Objects.Add(mapObject);
                        MSB2.Model.Object model = new();
                        model.Name = mapObject.ModelName;
                        map.Models.Objects.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB2.Part.Collision collision)
                    {
                        map.Parts.Collisions.Add(collision);
                        MSB2.Model.Collision model = new();
                        model.Name = collision.ModelName;
                        map.Models.Collisions.Add(model);
                    }
                    if (assetInfo.InnerObject is MSB2.Part.ConnectCollision connectCol)
                    {
                        map.Parts.ConnectCollisions.Add(connectCol);
                        MSB2.Model.Collision model = new();
                        model.Name = connectCol.ModelName;
                        map.Models.Collisions.Add(model);
                    }

                    // Regions
                    if (assetInfo.InnerObject is MSB2.Region.Region0 region)
                    {
                        map.Regions.Region0s.Add(region);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.Light light)
                    {
                        map.Regions.Lights.Add(light);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.StartPoint startPoint)
                    {
                        map.Regions.StartPoints.Add(startPoint);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.Sound sound)
                    {
                        map.Regions.Sounds.Add(sound);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.SFX sfx)
                    {
                        map.Regions.SFXs.Add(sfx);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.Wind wind)
                    {
                        map.Regions.Winds.Add(wind);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.EnvLight envLight)
                    {
                        map.Regions.EnvLights.Add(envLight);
                    }
                    if (assetInfo.InnerObject is MSB2.Region.Fog fog)
                    {
                        map.Regions.Fogs.Add(fog);
                    }

                    // Events
                    if (assetInfo.InnerObject is MSB2.Event.Light evtLight)
                    {
                        map.Events.Lights.Add(evtLight);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.Shadow shadow)
                    {
                        map.Events.Shadows.Add(shadow);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.Fog evtFog)
                    {
                        map.Events.Fogs.Add(evtFog);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.BGColor bgColor)
                    {
                        map.Events.BGColors.Add(bgColor);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.MapOffset mapOffset)
                    {
                        map.Events.MapOffsets.Add(mapOffset);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.Warp warp)
                    {
                        map.Events.Warps.Add(warp);
                    }
                    if (assetInfo.InnerObject is MSB2.Event.CheapMode cheapMode)
                    {
                        map.Events.CheapModes.Add(cheapMode);
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
        public static Prefab_DS2 ImportJson(string path)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                Prefab_DS2 prefab = JsonConvert.DeserializeObject<Prefab_DS2>(File.ReadAllText(path), settings);

                MSB2 pseudoMap = MSB2.Read(prefab.AssetContainerBytes);

                // Parts
                foreach (var mapPiece in pseudoMap.Parts.MapPieces)
                {
                    PrefabInfo_DS2 info = new(prefab, mapPiece);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var col in pseudoMap.Parts.Collisions)
                {
                    PrefabInfo_DS2 info = new(prefab, col);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var navmesh in pseudoMap.Parts.Navmeshes)
                {
                    PrefabInfo_DS2 info = new(prefab, navmesh);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
                {
                    PrefabInfo_DS2 info = new(prefab, connectCol);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapObject in pseudoMap.Parts.Objects)
                {
                    PrefabInfo_DS2 info = new(prefab, mapObject);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Regions
                foreach (var region in pseudoMap.Regions.Region0s)
                {
                    PrefabInfo_DS2 info = new(prefab, region);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var light in pseudoMap.Regions.Lights)
                {
                    PrefabInfo_DS2 info = new(prefab, light);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var startPoint in pseudoMap.Regions.StartPoints)
                {
                    PrefabInfo_DS2 info = new(prefab, startPoint);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sound in pseudoMap.Regions.Sounds)
                {
                    PrefabInfo_DS2 info = new(prefab, sound);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var sfx in pseudoMap.Regions.SFXs)
                {
                    PrefabInfo_DS2 info = new(prefab, sfx);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var wind in pseudoMap.Regions.Winds)
                {
                    PrefabInfo_DS2 info = new(prefab, wind);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var envLight in pseudoMap.Regions.EnvLights)
                {
                    PrefabInfo_DS2 info = new(prefab, envLight);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var fog in pseudoMap.Regions.Fogs)
                {
                    PrefabInfo_DS2 info = new(prefab, fog);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                // Events
                foreach (var evtLight in pseudoMap.Events.Lights)
                {
                    PrefabInfo_DS2 info = new(prefab, evtLight);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var shadow in pseudoMap.Events.Shadows)
                {
                    PrefabInfo_DS2 info = new(prefab, shadow);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var fog in pseudoMap.Events.Fogs)
                {
                    PrefabInfo_DS2 info = new(prefab, fog);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var bgColor in pseudoMap.Events.BGColors)
                {
                    PrefabInfo_DS2 info = new(prefab, bgColor);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var mapOffset in pseudoMap.Events.MapOffsets)
                {
                    PrefabInfo_DS2 info = new(prefab, mapOffset);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var warp in pseudoMap.Events.Warps)
                {
                    PrefabInfo_DS2 info = new(prefab, warp);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }
                foreach (var cheapMode in pseudoMap.Events.CheapModes)
                {
                    PrefabInfo_DS2 info = new(prefab, cheapMode);
                    info.AddNamePrefix(prefab.PrefabName);
                    prefab.PrefabInfoChildren.Add(info);
                }

                return prefab;
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox(
                    $"Unable to import Prefab_DS2 due to the following error:" +
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
            Prefab_DS2 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS2.ImportJson(info.Path);
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

            Prefab_DS2 prefab = new(_selection.GetFilteredSelection<MsbEntity>());

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
            Prefab_DS2 _selectedAssetPrefab;

            _selectedAssetPrefab = Prefab_DS2.ImportJson(info.Path);
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

            if (ent.WrappedObject is MSB2.Part.MapPiece mapPiece)
            {
                foreach (var entry in ModelAliasBank.Bank._loadedAliasBank.GetEntries("MapPieces"))
                {
                    if (modelName == entry.id)
                    {
                        fullname = $"{modelName} <{entry.name}>";
                    }
                }
            }

            if (ent.WrappedObject is MSB2.Part.Object mapObject)
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
            if (ent.WrappedObject is MSB2.Part.MapPiece mapPiece)
            {
                name = $"Map Piece: {name}";
            }
            if (ent.WrappedObject is MSB2.Part.Collision col)
            {
                name = $"Collision: {name}";
            }
            if (ent.WrappedObject is MSB2.Part.Navmesh navmesh)
            {
                name = $"Navmesh: {name}";
            }
            if (ent.WrappedObject is MSB2.Part.ConnectCollision connectCol)
            {
                name = $"Connect Collision: {name}";
            }
            if (ent.WrappedObject is MSB2.Part.Object mapObject)
            {
                name = $"Object: {name}";
            }

            // Regions
            if (ent.WrappedObject is MSB2.Region.Region0 region)
            {
                name = $"Region: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.Light light)
            {
                name = $"Light: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.StartPoint startPoint)
            {
                name = $"Start Point: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.Sound sound)
            {
                name = $"Sound: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.SFX sfx)
            {
                name = $"SFX: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.Wind wind)
            {
                name = $"Wind: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.EnvLight envLight)
            {
                name = $"Environment Light: {name}";
            }
            if (ent.WrappedObject is MSB2.Region.Fog fog)
            {
                name = $"Fog: {name}";
            }

            // Events
            if (ent.WrappedObject is MSB2.Event.Light evtLight)
            {
                name = $"Light: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.Shadow shadow)
            {
                name = $"Shadow: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.Fog evtFog)
            {
                name = $"Fog: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.BGColor bgColor)
            {
                name = $"Background Color: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.MapOffset mapOffset)
            {
                name = $"Map Offset: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.Warp warp)
            {
                name = $"Warp: {name}";
            }
            if (ent.WrappedObject is MSB2.Event.CheapMode cheapMode)
            {
                name = $"Cheap Mode: {name}";
            }

            return name;
        }
    }
}
