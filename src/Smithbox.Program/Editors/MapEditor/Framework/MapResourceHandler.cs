using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Helpers;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Resource.ResourceManager;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapResourceHandler
{
    public MapEditorScreen Editor;

    private Task task;

    private HashSet<ResourceDescriptor> LoadList_MapPiece_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Enemy_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Collision = new();
    private HashSet<ResourceDescriptor> LoadList_Navmesh = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Enemy_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Map_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Other_Texture = new();

    public string MapID;
    public string AdjustedMapID;
    public IMsb Msb;

    public MapResourceHandler(MapEditorScreen editor, string mapId)
    {
        Editor = editor;
        MapID = mapId;
        AdjustedMapID = MapLocator.GetAssetMapID(Editor.Project, MapID);
    }

    public async Task<bool> ReadMap(string mapid)
    {
        await Editor.Project.MapData.PrimaryBank.LoadMap(mapid);

        var entry = Editor.Project.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapid);

        if (entry.Value == null)
            return false;

        Msb = entry.Value.MSB;

        return true;
    }

    public void SetupHumanEnemySubstitute()
    {
        if (CFG.Current.MapEditor_ModelLoad_Characters)
        {
            var chrId = CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID;

            var modelAsset = ModelLocator.GetChrModel(Editor.Project, chrId, chrId);
            var textureAsset = TextureLocator.GetChrTextures(Editor.Project, chrId);

            if (modelAsset.IsValid())
                LoadList_Character_Model.Add(modelAsset);

            if (textureAsset.IsValid())
                LoadList_Character_Texture.Add(textureAsset);
        }
    }

    public void SetupModelLoadLists()
    {
        foreach (IMsbModel model in Msb.Models.GetEntries())
        {
            // MapPiece
            if (model.Name.StartsWith('m'))
            {
                var name = ModelLocator.MapModelNameToAssetName(Editor.Project, AdjustedMapID, model.Name);
                var modelAsset = ModelLocator.GetMapModel(Editor.Project, AdjustedMapID, name, name);

                if (modelAsset.IsValid())
                    LoadList_MapPiece_Model.Add(modelAsset);
            }

            // Character
            if (model.Name.StartsWith('c'))
            {
                var modelAsset = ModelLocator.GetChrModel(Editor.Project, model.Name, model.Name);

                if (modelAsset.IsValid())
                    LoadList_Character_Model.Add(modelAsset);
            }

            // Enemy
            if (model.Name.StartsWith('e'))
            {
                var modelAsset = ModelLocator.GetEneModel(Editor.Project, model.Name);

                if (modelAsset.IsValid())
                    LoadList_Enemy_Model.Add(modelAsset);
            }

            // Object / Asset
            if (model.Name.StartsWith('o') || model.Name.StartsWith("AEG"))
            {
                var modelAsset = ModelLocator.GetObjModel(Editor.Project, model.Name, model.Name);

                if (modelAsset.IsValid())
                    LoadList_Asset_Model.Add(modelAsset);
            }

            // Collision
            if (model.Name.StartsWith('h'))
            {
                var modelAsset = ModelLocator.GetMapCollisionModel(Editor.Project, AdjustedMapID, ModelLocator.MapModelNameToAssetName(Editor.Project, AdjustedMapID, model.Name), false);

                if (modelAsset.IsValid())
                    LoadList_Collision.Add(modelAsset);
            }

            // Navmesh
            if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if (model.Name.StartsWith('n'))
                {
                    var modelAsset = ModelLocator.GetMapNVMModel(Editor.Project, AdjustedMapID, ModelLocator.MapModelNameToAssetName(Editor.Project, AdjustedMapID, model.Name));

                    if (modelAsset.IsValid())
                        LoadList_Navmesh.Add(modelAsset);
                }
            }
            else if (Editor.HavokNavmeshManager.CanUse())
            {
                ResourceDescriptor nav = ModelLocator.GetHavokNavmeshes(Editor.Project, AdjustedMapID);

                LoadList_Navmesh.Add(nav);
            }
        }
    }

    public void SetupTexturelLoadLists()
    {
        // Models
        foreach (IMsbModel model in Msb.Models.GetEntries())
        {
            // Character
            if (model.Name.StartsWith('c'))
            {
                var textureAsset = TextureLocator.GetChrTextures(Editor.Project, model.Name);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);
            }

            // Enemy
            if (model.Name.StartsWith('e'))
            {
                var textureAsset = TextureLocator.GetEneTextureContainer(Editor.Project, model.Name);

                if (textureAsset.IsValid())
                    LoadList_Enemy_Texture.Add(textureAsset);
            }

            // Object
            if (model.Name.StartsWith('o'))
            {
                var textureAsset = TextureLocator.GetObjTextureContainer(Editor.Project, model.Name);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }
        }

        // AET
        /*
        if(Smithbox.ProjectType is ProjectType.ER)
        {
            var erMap = (MSBE)Msb;

            foreach(var entry in erMap.Parts.Assets)
            {
                var name = entry.ModelName.ToLower().Replace("aeg", "aet");
                var asset = ResourceTextureLocator.GetAetTexture(name);
                LoadList_Other_Texture.Add(asset);
            }
        }
        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            var acMap = (MSB_AC6)Msb;

            foreach (var entry in acMap.Parts.Assets)
            {
                var name = entry.ModelName.ToLower().Replace("aeg", "aet");
                var asset = ResourceTextureLocator.GetAetTexture(name);
                LoadList_Other_Texture.Add(asset);
            }
        }
        */

        // AAT

        // SYSTEX

        // Map
        foreach (ResourceDescriptor asset in TextureLocator.GetMapTextures(Editor.Project, AdjustedMapID))
        {
            if (asset.IsValid())
                LoadList_Map_Texture.Add(asset);
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

                var renderScene = Editor.Universe.RenderScene;

                DrawableHelper.GetModelDrawable(Editor, renderScene, map, obj, mp.ModelName, false, masks);
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
                        false, ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);

            // Enemy
            texJob = ResourceManager.CreateNewJob($@"Enemy Textures");

            foreach (ResourceDescriptor asset in LoadList_Enemy_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceManager.ResourceType.Flver);
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
                        false, ResourceManager.ResourceType.Flver);
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
                        false, ResourceManager.ResourceType.Flver);
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
                        ResourceManager.ResourceType.Flver);
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
                        ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);

            // Enemies
            job = ResourceManager.CreateNewJob($@"Enemies");

            foreach (ResourceDescriptor asset in LoadList_Enemy_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceManager.ResourceType.Flver);
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
                        ResourceManager.ResourceType.Flver);
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

        // Navmesh
        if (CFG.Current.MapEditor_ModelLoad_Navmeshes)
        {
            var job = ResourceManager.CreateNewJob($@"Navmesh");

            foreach (ResourceDescriptor asset in LoadList_Navmesh)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceType.NavmeshHKX);
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
