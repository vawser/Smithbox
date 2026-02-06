using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            else if (View.HavokNavmeshBank.CanUse())
            {
                ResourceDescriptor nav = ModelLocator.GetHavokNavmeshes(View.Project, AdjustedMapID);

                LoadList_Navmesh.Add(nav);
            }
        }
    }

    public void SetupTexturelLoadLists()
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
            // Character
            if (model.Name.StartsWith('c'))
            {
                // TPF
                var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, model.Name, false);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);
    
                // BND
                textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, model.Name, true);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);
            }

            // Object
            if (model.Name.StartsWith('o'))
            {
                var textureAsset = TextureLocator.GetObjectTextureVirtualPath(View.Project, model.Name);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }

            // Assets
            if (model.Name.StartsWith("AEG") || model.Name.StartsWith("aeg"))
            {
                var textureAsset = TextureLocator.GetAssetTextureVirtualPath(View.Project, model.Name);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }
        }

        // AAT
        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            var textureAsset = TextureLocator.GetCharacterCommonTextureVirtualPath(View.Project, "common_body");

            if (textureAsset.IsValid())
                LoadList_Asset_Texture.Add(textureAsset);
        }

        // SYSTEX
        if (View.Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB or ProjectType.NR)
        {
            var textureAsset = TextureLocator.GetSystexTextureVirtualPath(View.Project, "systex");

            if (textureAsset.IsValid())
                LoadList_Asset_Texture.Add(textureAsset);
        }
    }

    public void SetupModelMasks(MapContainer map)
    {
        foreach (Entity obj in map.Objects)
        {
            if (obj.WrappedObject is IMsbPart mp && mp.ModelName != null && mp.ModelName != string.Empty &&
                obj.RenderSceneMesh == null)
            {
                int[] masks = null;
                if (obj is MsbEntity msbEnt)
                {
                    masks = msbEnt.GetModelMasks();
                }

                var renderScene = View.Universe.GetCurrentScene();

                DrawableHelper.GetModelDrawable(View.Universe, renderScene, map, obj, mp.ModelName, false, masks);
            }
        }
    }

    public List<Task> LoadTextures(List<Task> tasks, MapContainer map)
    {
        if (!CFG.Current.Viewport_Enable_Texturing)
            return tasks;

        // Map
        if (CFG.Current.MapEditor_TextureLoad_MapPieces)
        {
            var texJob = ResourceManager.CreateNewJob($@"{AdjustedMapID} Textures");

            foreach (ResourceDescriptor asset in LoadList_Map_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);
        }

        // Character
        if (CFG.Current.MapEditor_TextureLoad_Characters)
        {
            var texJob = ResourceManager.CreateNewJob($@"Character Textures");

            foreach (ResourceDescriptor asset in LoadList_Character_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);
        }

        // Asset
        if (CFG.Current.MapEditor_TextureLoad_Objects)
        {
            var texJob = ResourceManager.CreateNewJob($@"Asset Textures");

            foreach (ResourceDescriptor asset in LoadList_Asset_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);
        }

        // Other Textures
        if (CFG.Current.MapEditor_TextureLoad_Misc)
        {
            var texJob = ResourceManager.CreateNewJob($@"Other Textures");

            foreach (ResourceDescriptor asset in LoadList_Other_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);
        }

        return tasks;
    }

    public List<Task> LoadModels(List<Task> tasks, MapContainer map)
    {
        // MapPieces
        if (CFG.Current.MapEditor_ModelLoad_MapPieces)
        {
            var job = ResourceManager.CreateNewJob($@"MapPieces");

            foreach (ResourceDescriptor asset in LoadList_MapPiece_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);
        }

        // Characters
        if (CFG.Current.MapEditor_ModelLoad_Characters)
        {
            var job = ResourceManager.CreateNewJob($@"Characters");

            foreach (ResourceDescriptor asset in LoadList_Character_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);
        }

        // Objects
        if (CFG.Current.MapEditor_ModelLoad_Objects)
        {
            var job = ResourceManager.CreateNewJob($@"Assets");

            foreach (ResourceDescriptor asset in LoadList_Asset_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);
        }

        // Collisions
        if (CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            var job = ResourceManager.CreateNewJob($@"Collisions");

            string archive = null;
            HashSet<string> collisionAssets = new();

            foreach (ResourceDescriptor asset in LoadList_Collision)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    archive = asset.AssetArchiveVirtualPath;
                    collisionAssets.Add(asset.AssetVirtualPath);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            if (archive != null)
            {
                job.AddLoadArchiveTask(archive, AccessLevel.AccessGPUOptimizedOnly, false, collisionAssets);
            }

            task = job.Complete();
            tasks.Add(task);
        }
        
        // Connect Collisions
        if (CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            var job = ResourceManager.CreateNewJob($@"Connect Collisions");

            string archive = null;
            HashSet<string> collisionAssets = new();

            foreach (ResourceDescriptor asset in LoadList_ConnectCollision)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    archive = asset.AssetArchiveVirtualPath;
                    collisionAssets.Add(asset.AssetVirtualPath);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            if (archive != null)
            {
                job.AddLoadArchiveTask(archive, AccessLevel.AccessGPUOptimizedOnly, false, collisionAssets);
            }

            task = job.Complete();
            tasks.Add(task);
        }

        // Navmesh
        if (CFG.Current.MapEditor_ModelLoad_Navmeshes)
        {
            var job = ResourceManager.CreateNewJob($@"Navmesh");

            foreach (ResourceDescriptor asset in LoadList_Navmesh)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    var type = ResourceType.NavmeshHKX;

                    if (View.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
                        type = ResourceType.Navmesh;

                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, type);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);
        }

        return tasks;
    }
}
