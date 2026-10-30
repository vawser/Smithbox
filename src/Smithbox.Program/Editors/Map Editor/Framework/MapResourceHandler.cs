using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class MapResourceHandler
{
    public MapEditorView View;

    private Task task;

    private HashSet<ResourceDescriptor> LoadList_MapPiece_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Collision = new();
    private HashSet<ResourceDescriptor> LoadList_ConnectCollision = new();
    private HashSet<ResourceDescriptor> LoadList_Navmesh = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Map_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Other_Texture = new();

    public string MapID;
    public string AdjustedMapID;
    public IMsb Msb;

    public MapResourceHandler(MapEditorView view, string mapId)
    {
        View = view;
        MapID = mapId;
        AdjustedMapID = PathBuilder.GetAssetMapID(View.Project, MapID);
    }

    public async Task<bool> ReadMap(string mapid)
    {
        await View.Project.Handler.MapData.PrimaryBank.LoadMap(mapid);

        var entry = View.Project.Handler.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapid);

        if (entry.Value == null)
            return false;

        Msb = entry.Value.MSB;

        return true;
    }

    public void SetupHumanEnemySubstitute()
    {
        using var __subScope = Profiler.TracyZoneAuto();
        if (CFG.Current.MapEditor_ModelLoad_Characters)
        {
            var chrId = CFG.Current.MapEditor_Character_Substitution_ID;

            var modelAsset = ModelLocator.GetChrModel(View.Project, chrId, chrId);

            if (modelAsset.IsValid())
                LoadList_Character_Model.Add(modelAsset);

            // TPF
            var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, chrId, false);

            if (textureAsset.IsValid())
                LoadList_Character_Model.Add(textureAsset);

            // BND
            textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, chrId, true);

            if (textureAsset.IsValid())
                LoadList_Character_Model.Add(textureAsset);
        }
    }

    public void SetupModelLoadLists()
    {
        foreach (IMsbModel model in Msb.Models.GetEntries())
        {
            // MapPiece
            if (model.Name.StartsWith('m'))
            {
                var name = ModelLocator.MapModelNameToAssetName(View.Project, AdjustedMapID, model.Name);
                var modelAsset = ModelLocator.GetMapModel(View.Project, AdjustedMapID, name, name);

                if (modelAsset.IsValid())
                    LoadList_MapPiece_Model.Add(modelAsset);
            }

            // Character
            if (model.Name.StartsWith('c'))
            {
                var modelAsset = ModelLocator.GetChrModel(View.Project, model.Name, model.Name);

                if (modelAsset.IsValid())
                    LoadList_Character_Model.Add(modelAsset);
            }

            // Object / Asset
            if (model.Name.StartsWith('o') || (model.Name.StartsWith("AEG") || model.Name.StartsWith("aeg")))
            {
                var modelAsset = ModelLocator.GetObjModel(View.Project, model.Name, model.Name);

                if (modelAsset.IsValid())
                    LoadList_Asset_Model.Add(modelAsset);
            }

            // Collision
            if (model.Name.StartsWith('h'))
            {
                var modelAsset = ModelLocator.GetMapCollisionModel(View.Project, AdjustedMapID, ModelLocator.MapModelNameToAssetName(View.Project, AdjustedMapID, model.Name));

                if (modelAsset.IsValid())
                {
                    LoadList_Collision.Add(modelAsset);
                }
            }

            // Connect Collision
            if (model.Name.StartsWith('h'))
            {
                var modelAsset = ModelLocator.GetMapCollisionModel(View.Project, AdjustedMapID, ModelLocator.MapModelNameToAssetName(View.Project, AdjustedMapID, model.Name), true);

                if (modelAsset.IsValid())
                {
                    LoadList_ConnectCollision.Add(modelAsset);
                }
            }

            // Navmesh
            if (View.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                if (model.Name.StartsWith('n'))
                {
                    var modelAsset = ModelLocator.GetMapNVMModel(View.Project, AdjustedMapID, ModelLocator.MapModelNameToAssetName(View.Project, AdjustedMapID, model.Name));

                    if (modelAsset.IsValid())
                        LoadList_Navmesh.Add(modelAsset);
                }
            }
        }

        // Havok navmeshes are one archive per map, not per model
        if (View.Project.Descriptor.ProjectType is not (ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES) &&
            View.HavokNavmeshBank.CanUse())
        {
            ResourceDescriptor nav = ModelLocator.GetHavokNavmeshes(View.Project, AdjustedMapID);

            LoadList_Navmesh.Add(nav);
        }
    }

    public void SetupTextureLoadLists()
    {
        // MAP
        foreach (ResourceDescriptor asset in TextureLocator.GetMapTextureVirtualPaths(View.Project, AdjustedMapID))
        {
            if (asset.IsValid())
                LoadList_Map_Texture.Add(asset);
        }

        // Models
        foreach (IMsbModel model in Msb.Models.GetEntries())
        {
            var loadList = model.Name.StartsWith('c') ? LoadList_Character_Texture : LoadList_Asset_Texture;

            foreach (ResourceDescriptor asset in GetModelTextureAssets(View.Project, model.Name))
            {
                loadList.Add(asset);
            }
        }

        // AAT
        if (View.Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            var textureAsset = TextureLocator.GetCharacterCommonTextureVirtualPath(View.Project, "common_body");

            if (textureAsset.IsValid())
                LoadList_Other_Texture.Add(textureAsset);
        }

        // SYSTEX
        if (View.Project.Descriptor.ProjectType is ProjectType.AC6 
            or ProjectType.ER 
            or ProjectType.SDT 
            or ProjectType.DS3 
            or ProjectType.BB 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            var textureAsset = TextureLocator.GetSystexTextureVirtualPath(View.Project, "systex");

            if (textureAsset.IsValid())
                LoadList_Other_Texture.Add(textureAsset);
        }
    }

    /// <summary>
    ///     Gets the texture containers named after a character, object or asset model.
    /// </summary>
    public static List<ResourceDescriptor> GetModelTextureAssets(ProjectEntry project, string modelName)
    {
        List<ResourceDescriptor> assets = new();

        // Character
        if (modelName.StartsWith('c'))
        {
            // TPF
            assets.Add(TextureLocator.GetCharacterTextureVirtualPath(project, modelName, false));

            // BND
            assets.Add(TextureLocator.GetCharacterTextureVirtualPath(project, modelName, true));
        }

        // Object
        if (modelName.StartsWith('o'))
        {
            assets.Add(TextureLocator.GetObjectTextureVirtualPath(project, modelName));
        }

        // Assets
        if (modelName.StartsWith("AEG") || modelName.StartsWith("aeg"))
        {
            assets.Add(TextureLocator.GetAssetTextureVirtualPath(project, modelName));
        }

        return assets.Where(e => e.IsValid()).ToList();
    }

    /// <summary>
    ///     Loads the containers that hold a set of assets into a pool as one job.
    /// </summary>
    private Task LoadAssets(IEnumerable<ResourceDescriptor> assets, string jobName, ResourceType types,
        ResourcePool pool = null, bool onlyListedAssets = false, bool isConnectCollision = false)
    {
        var options = new LoadOptions
        {
            JobName = jobName,
            Types = types,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            IsConnectCollision = isConnectCollision
        };

        return (pool ?? ResourceManager.DefaultPool).LoadAssets(View.Project, assets, options, onlyListedAssets);
    }

    public List<Task> LoadTextures(List<Task> tasks, MapContainer map)
    {
        if (!CFG.Current.Viewport_Enable_Texturing)
            return tasks;

        // Map
        if (CFG.Current.MapEditor_TextureLoad_MapPieces)
        {
            var name = LOC.Get("MAP_ResourceManager_Map_Textures", AdjustedMapID);

            task = LoadAssets(LoadList_Map_Texture, name, ResourceType.Texture);
            tasks.Add(task);
        }

        // Character
        if (CFG.Current.MapEditor_TextureLoad_Characters)
        {
            task = LoadAssets(LoadList_Character_Texture, LOC.Get("MAP_ResourceManager_Character_Textures"), ResourceType.Texture);
            tasks.Add(task);
        }

        // Asset
        if (CFG.Current.MapEditor_TextureLoad_Objects)
        {
            var name = LOC.Get("MAP_ResourceManager_Object_Textures");
            if (View.Project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                name = LOC.Get("MAP_ResourceManager_Asset_Textures");
            }

            task = LoadAssets(LoadList_Asset_Texture, name, ResourceType.Texture);
            tasks.Add(task);
        }

        // Other Textures
        if (CFG.Current.MapEditor_TextureLoad_Misc)
        {
            task = LoadAssets(LoadList_Other_Texture, LOC.Get("MAP_ResourceManager_Miscellaneous_Textures"), ResourceType.Texture);
            tasks.Add(task);
        }

        return tasks;
    }

    public List<Task> LoadModels(List<Task> tasks, MapContainer map)
    {
        // Model archives can also contain texture packs, so those are loaded along with the models
        var modelTypes = ResourceType.Flver | ResourceType.Texture;

        // MapPieces
        if (CFG.Current.MapEditor_ModelLoad_MapPieces)
        {
            task = LoadAssets(LoadList_MapPiece_Model, LOC.Get("MAP_ResourceManager_Map_Piece_Models"), modelTypes);
            tasks.Add(task);
        }

        // Characters
        if (CFG.Current.MapEditor_ModelLoad_Characters)
        {
            task = LoadAssets(LoadList_Character_Model, LOC.Get("MAP_ResourceManager_Character_Models"), modelTypes);
            tasks.Add(task);
        }

        // Objects
        if (CFG.Current.MapEditor_ModelLoad_Objects)
        {
            var name = LOC.Get("MAP_ResourceManager_Object_Models");
            if (View.Project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR 
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                name = LOC.Get("MAP_ResourceManager_Asset_Models");
            }

            task = LoadAssets(LoadList_Asset_Model, name, modelTypes);
            tasks.Add(task);
        }

        // Collisions: only the collisions this map uses are loaded from the collision archives
        if (CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            task = LoadAssets(LoadList_Collision, LOC.Get("MAP_ResourceManager_Collision_Models"),
                ResourceType.CollisionHKX, onlyListedAssets: true);
            tasks.Add(task);
        }

        // Connect Collisions: stored in the same files as the low collisions, so they are loaded into their own pool
        if (CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            task = LoadAssets(LoadList_ConnectCollision, LOC.Get("MAP_ResourceManager_Connect_Collision_Models"),
                ResourceType.CollisionHKX, ResourceManager.ConnectCollisionPool, onlyListedAssets: true,
                isConnectCollision: true);
            tasks.Add(task);
        }

        // Navmesh
        if (CFG.Current.MapEditor_ModelLoad_Navmeshes)
        {
            var type = ResourceType.NavmeshHKX;

            if (View.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
                type = ResourceType.Navmesh;

            task = LoadAssets(LoadList_Navmesh, LOC.Get("MAP_ResourceManager_Navmesh_Models"), type);
            tasks.Add(task);
        }

        return tasks;
    }
}
