using HKX2;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static SoulsFormats.HKXPWV;
using System.Xml;
using StudioCore.UserProject;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StudioCore.Platform;

namespace StudioCore.Editors.MapEditor.Prefabs;

public class ER_AssetPrefab
{
    public string PrefabName = "";
    public string PrefixSeparator = "[]";
    public ProjectType Type = ProjectType.ER;

    /// <summary>
    /// Bytes of the MSB that stores prefab data.
    /// </summary>
    public byte[] AssetContainerBytes { get; set; }

    /// <summary>
    /// List of AssetInfo derived from MSB AssetContainerBytes.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<ER_AssetInfo> AssetInfoChildren = new();

    /// <summary>
    /// List of Msb Entities derived from AssetInfoChildren.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<MsbEntity> MsbEntityChildren = new();


    // JsonExtensionData stores fields json that are not present in class in order to retain data between versions.
    [Newtonsoft.Json.JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;

    public ER_AssetPrefab()
    { }

    public ER_AssetPrefab(HashSet<MsbEntity> entities)
    {
        foreach (var ent in entities)
        {
            // Parts
            if (ent.WrappedObject is MSBE.Part.MapPiece mapPiece)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, mapPiece));
            }
            if (ent.WrappedObject is MSBE.Part.Enemy enemy)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, enemy));
            }
            if (ent.WrappedObject is MSBE.Part.Player player)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, player));
            }
            if (ent.WrappedObject is MSBE.Part.Collision col)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, col));
            }
            if (ent.WrappedObject is MSBE.Part.DummyAsset dummyAsset)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, dummyAsset));
            }
            if (ent.WrappedObject is MSBE.Part.DummyEnemy dummyEnemy)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, dummyEnemy));
            }
            if (ent.WrappedObject is MSBE.Part.ConnectCollision connectCol)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, connectCol));
            }
            if (ent.WrappedObject is MSBE.Part.Asset asset)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, asset));
            }

            // Regions
            if (ent.WrappedObject is MSBE.Region.Other region)
            {
                AssetInfoChildren.Add(new ER_AssetInfo(this, region));
            }
        }
    }
    public class ER_AssetInfo
    {

        public ER_AssetPrefab Parent;

        public AssetInfoDataType DataType = AssetInfoDataType.None;

        public enum AssetInfoDataType
        {
            None = -1,
            Part = 1000,
            Region = 2000,
            Event = 3000
        }

        public AssetInfoPartDataType PartDataType = AssetInfoPartDataType.None;

        public enum AssetInfoPartDataType
        {
            None = -1,
            MapPiece = 1,
            Enemy = 2,
            Player = 3,
            Collision = 4,
            DummyAsset = 5,
            DummyEnemy = 6,
            ConnectCollision = 7,
            Asset = 8,
        }

        public AssetInfoRegionDataType RegionDataType = AssetInfoRegionDataType.None;

        public enum AssetInfoRegionDataType
        {
            None = -1,
            Other = 1
        }

        public object InnerObject = null;

        // Parts
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.MapPiece mapPiece)
        {
            InnerObject = mapPiece.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.MapPiece temp = InnerObject as MSBE.Part.MapPiece;
            temp.EntityID = 0;
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.MapPiece;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.Enemy enemy)
        {
            InnerObject = enemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Enemy temp = InnerObject as MSBE.Part.Enemy;
            temp.EntityID = 0;
            temp.CollisionPartIndex = -1;
            temp.CollisionPartName = "";
            temp.WalkRouteIndex = -1;
            temp.WalkRouteName = "";
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.Enemy;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.Player player)
        {
            InnerObject = player.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Player temp = InnerObject as MSBE.Part.Player;
            temp.EntityID = 0;
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.Player;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.Collision collision)
        {
            InnerObject = collision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Collision temp = InnerObject as MSBE.Part.Collision;
            temp.EntityID = 0;
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.Collision;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.DummyAsset dummyAsset)
        {
            InnerObject = dummyAsset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.DummyAsset temp = InnerObject as MSBE.Part.DummyAsset;
            temp.EntityID = 0;
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.DummyAsset;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.DummyEnemy dummyEnemy)
        {
            InnerObject = dummyEnemy.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.DummyEnemy temp = InnerObject as MSBE.Part.DummyEnemy;
            temp.EntityID = 0;
            temp.CollisionPartIndex = -1;
            temp.CollisionPartName = "";
            temp.WalkRouteIndex = -1;
            temp.WalkRouteName = "";
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.DummyEnemy;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.ConnectCollision connectCollision)
        {
            InnerObject = connectCollision.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.ConnectCollision temp = InnerObject as MSBE.Part.ConnectCollision;
            temp.EntityID = 0;
            temp.CollisionName = "";
            temp.CollisionIndex = -1;
            Array.Clear(temp.EntityGroupIDs);

            PartDataType = AssetInfoPartDataType.ConnectCollision;
        }
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Part.Asset asset)
        {
            InnerObject = asset.DeepCopy();
            DataType = AssetInfoDataType.Part;
            Parent = parent;

            MSBE.Part.Asset temp = InnerObject as MSBE.Part.Asset;
            temp.EntityID = 0;
            Array.Clear(temp.EntityGroupIDs);
            Array.Clear(temp.UnkPartNames);

            PartDataType = AssetInfoPartDataType.Asset;
        }

        // Region
        public ER_AssetInfo(ER_AssetPrefab parent, MSBE.Region.Other region)
        {
            InnerObject = region.DeepCopy();
            DataType = AssetInfoDataType.Region;
            Parent = parent;

            RegionDataType = AssetInfoRegionDataType.Other;
        }

        public void AddNamePrefix(string prefix)
        {
            var prop = InnerObject.GetType().GetProperty("Name");
            if (prop == null)
            {
                throw new InvalidDataException($"AssetPrefab operation failed, {InnerObject.GetType()} does not contain Name property.");
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
                throw new InvalidDataException($"AssetPrefab operation failed, {InnerObject.GetType()} does not contain Name property.");
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
        foreach (var assetInfo in AssetInfoChildren)
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
                case ER_AssetInfo.AssetInfoDataType.Part:
                    ent.Type = MsbEntity.MsbEntityType.Part;
                    break;
                case ER_AssetInfo.AssetInfoDataType.Region:
                    ent.Type = MsbEntity.MsbEntityType.Region;
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
            foreach (var assetInfo in AssetInfoChildren)
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
                if (assetInfo.InnerObject is MSBE.Region.Other region)
                {
                    map.Regions.Others.Add(region);
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
    public static ER_AssetPrefab ImportJson(string path)
    {
        try
        {
            var settings = new JsonSerializerSettings();
            ER_AssetPrefab prefab = JsonConvert.DeserializeObject<ER_AssetPrefab>(File.ReadAllText(path), settings);

            MSBE pseudoMap = MSBE.Read(prefab.AssetContainerBytes);

            // Parts
            foreach (var mapPiece in pseudoMap.Parts.MapPieces)
            {
                ER_AssetInfo info = new(prefab, mapPiece);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var enemy in pseudoMap.Parts.Enemies)
            {
                ER_AssetInfo info = new(prefab, enemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var player in pseudoMap.Parts.Players)
            {
                ER_AssetInfo info = new(prefab, player);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var col in pseudoMap.Parts.Collisions)
            {
                ER_AssetInfo info = new(prefab, col);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var dummyAsset in pseudoMap.Parts.DummyAssets)
            {
                ER_AssetInfo info = new(prefab, dummyAsset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var dummyEnemy in pseudoMap.Parts.DummyEnemies)
            {
                ER_AssetInfo info = new(prefab, dummyEnemy);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var connectCol in pseudoMap.Parts.ConnectCollisions)
            {
                ER_AssetInfo info = new(prefab, connectCol);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }
            foreach (var asset in pseudoMap.Parts.Assets)
            {
                ER_AssetInfo info = new(prefab, asset);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }

            // Regions
            foreach (var region in pseudoMap.Regions.Others)
            {
                ER_AssetInfo info = new(prefab, region);
                info.AddNamePrefix(prefab.PrefabName);
                prefab.AssetInfoChildren.Add(info);
            }

            return prefab;
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox(
                $"Unable to import AssetPrefab due to the following error:" +
                $"\n\n{e.Message}"
                , "Asset prefab import error"
                , MessageBoxButtons.OK, MessageBoxIcon.Information);

            return null;
        }
    }
}