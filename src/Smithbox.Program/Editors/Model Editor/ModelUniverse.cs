using HKLib.hk2018.hkHashMapDetail;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelUniverse
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public RenderScene RenderScene;

    public bool HasProcessedModelLoad;

    public ViewportSelection Selection { get; }

    private Task task;
    private List<Task> Tasks = new();

    private HashSet<ResourceDescriptor> LoadList_MapPiece_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Collision = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Map_Texture = new();

    public ModelUniverse(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        RenderScene = editor.ModelViewportView.RenderScene;
        Selection = editor.ViewportSelection;

        if (RenderScene == null)
        {
            CFG.Current.Viewport_Enable_Rendering = false;
        }
        else
        {
            CFG.Current.Viewport_Enable_Rendering = true;
        }
    }

    /// <summary>
    /// Load the actual model file from the source file
    /// </summary>
    /// <param name="fileEntry"></param>
    /// <param name="modelName"></param>
    public async void LoadModel(ModelWrapper modelWrapper)
    {
        if (modelWrapper.Container != null)
        {
            TaskLogs.AddLog($"Model \"{modelWrapper.Name}\" is already loaded",
                LogLevel.Information, LogPriority.Normal);
            return;
        }

        ResourceManager.ClearUnusedResources();
        Editor.ViewportSelection.ClearSelection(Editor);

        var newContainer = new ModelContainer(Editor, Project, modelWrapper.Name);

        ModelInsightHelper.AddEntry(newContainer);

        newContainer.Load(modelWrapper.FLVER, modelWrapper);

        SetupModelLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);
        SetupTextureLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);

        modelWrapper.Container = newContainer;

        if (CFG.Current.Viewport_Enable_Rendering)
        {
            Selection.ClearSelection(Editor);
            Selection.AddSelection(Editor, newContainer.RootObject);
        }

        if (CFG.Current.Viewport_Enable_Rendering)
        {
            Tasks = LoadTextures(Tasks, newContainer);
            await Task.WhenAll(Tasks);
            Tasks = LoadModels(Tasks, newContainer);
            await Task.WhenAll(Tasks);

            ScheduleTextureRefresh();
        }

        await Task.WhenAll(Tasks);

        if (CFG.Current.Viewport_Enable_Rendering)
        {
            // FIXME: sub-meshes aren't associated with the individual mesh entries yet
            //int index = 0;
            //foreach (var ent in newContainer.Meshes)
            //{
            //    if (ent.RenderSceneMesh is MeshRenderableProxy meshProxy)
            //    {
            //        if (meshProxy.Submeshes.Count > 0 && index < meshProxy.Submeshes.Count)
            //        {
            //            ent.RenderSceneMesh = meshProxy.Submeshes[index];
            //            meshProxy.Submeshes[index].SetSelectable(ent);
            //        }
            //    }
            //    index++;
            //}

            foreach (Entity obj in newContainer.Objects)
            {
                obj.UpdateRenderModel(Editor);
            }
        }
    }

    public void SetupModelLoadList(string modelName, ModelContainerWrapper parent)
    {
        LoadList_MapPiece_Model.Clear();
        LoadList_Character_Model.Clear();
        LoadList_Asset_Model.Clear();
        LoadList_Part_Model.Clear();
        LoadList_Collision.Clear();

        var loadList = new List<ResourceDescriptor>();

        // MapPiece
        if (CFG.Current.ModelEditor_ModelLoad_MapPieces)
        {
            if (modelName.StartsWith('m'))
            {
                if (parent != null)
                {
                    var mapID = parent.MapID;

                    if (mapID != null)
                    {
                        var name = ModelLocator.GetMapModelName(Editor.Project, mapID, modelName);
                        var modelAsset = ModelLocator.GetMapModel(Editor.Project, mapID, name, name);

                        if (modelAsset.IsValid())
                            LoadList_MapPiece_Model.Add(modelAsset);
                    }
                }
            }
        }

        // Character
        if (CFG.Current.ModelEditor_ModelLoad_Characters)
        {
            if (modelName.StartsWith('c'))
            {
                var modelAsset = ModelLocator.GetChrModel(Editor.Project, modelName, modelName);

                if (modelAsset.IsValid())
                    LoadList_Character_Model.Add(modelAsset);
            }
        }

        // Object / Asset
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
        {
            if (modelName.StartsWith('o') || (modelName.StartsWith("AEG") || modelName.StartsWith("aeg")))
            {
                var modelAsset = ModelLocator.GetObjModel(Editor.Project, modelName, modelName);

                if (modelAsset.IsValid())
                    LoadList_Asset_Model.Add(modelAsset);
            }
        }

        // Part
        if (CFG.Current.ModelEditor_ModelLoad_Parts)
        {
            if (modelName.StartsWith("am") || modelName.StartsWith("AM") ||
            modelName.StartsWith("lg") || modelName.StartsWith("LG") ||
            modelName.StartsWith("bd") || modelName.StartsWith("BD") ||
            modelName.StartsWith("hd") || modelName.StartsWith("HD") ||
            modelName.StartsWith("wp") || modelName.StartsWith("WP"))
            {
                var modelAsset = ModelLocator.GetPartsModel(Editor.Project, modelName, modelName);

                if (modelAsset.IsValid())
                    LoadList_Part_Model.Add(modelAsset);
            }
        }

        // Collision
        if (CFG.Current.ModelEditor_ModelLoad_Collisions)
        {
            if (modelName.StartsWith('h'))
            {
                if (parent != null)
                {
                    var mapID = parent.MapID;

                    if (mapID != null)
                    {
                        var modelAsset = ModelLocator.GetMapCollisionModel(Editor.Project, mapID,
                        ModelLocator.GetMapModelName(Editor.Project, mapID, modelName));

                        if (modelAsset.IsValid())
                            LoadList_Collision.Add(modelAsset);
                    }
                }
            }
        }
    }

    public void SetupTextureLoadList(string modelName, ModelContainerWrapper parent)
    {
        LoadList_Character_Texture.Clear();
        LoadList_Asset_Texture.Clear();
        LoadList_Part_Texture.Clear();
        LoadList_Map_Texture.Clear();

        // MAP
        if (CFG.Current.ModelEditor_TextureLoad_MapPieces)
        {
            if (parent != null)
            {
                var mapID = parent.MapID;

                if (mapID != null)
                {
                    foreach (ResourceDescriptor asset in TextureLocator.GetMapTextureVirtualPaths(Editor.Project, mapID))
                    {
                        if (asset.IsValid())
                            LoadList_Map_Texture.Add(asset);
                    }
                }
            }
        }

        // Character
        if (CFG.Current.ModelEditor_TextureLoad_Characters)
        {
            if (modelName.StartsWith('c'))
            {
                // TPF
                var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(Editor.Project, modelName, false);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);

                // BND
                textureAsset = TextureLocator.GetCharacterTextureVirtualPath(Editor.Project, modelName, true);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);
            }
        }

        // Object
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
        {
            if (modelName.StartsWith('o'))
            {
                var textureAsset = TextureLocator.GetObjectTextureVirtualPath(Editor.Project, modelName);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }

            // Assets
            if (modelName.StartsWith("AEG") || modelName.StartsWith("aeg"))
            {
                var textureAsset = TextureLocator.GetAssetTextureVirtualPath(Editor.Project, modelName);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }
        }

        // Part
        if (CFG.Current.ModelEditor_TextureLoad_Parts)
        {
            if (modelName.StartsWith("am") || modelName.StartsWith("AM") ||
            modelName.StartsWith("lg") || modelName.StartsWith("LG") ||
            modelName.StartsWith("bd") || modelName.StartsWith("BD") ||
            modelName.StartsWith("hd") || modelName.StartsWith("HD") ||
            modelName.StartsWith("wp") || modelName.StartsWith("WP"))
            {
                var textureAsset = TextureLocator.GetPartTextureVirtualPath(Editor.Project, modelName);

                if (textureAsset.IsValid())
                    LoadList_Part_Texture.Add(textureAsset);
            }
        }

        // AAT
        if (CFG.Current.ModelEditor_TextureLoad_Misc)
        {
            if (Editor.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                var textureAsset = TextureLocator.GetCharacterCommonTextureVirtualPath(Editor.Project, "common_body");

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }

            // SYSTEX
            if (Editor.Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB or ProjectType.NR)
            {
                var textureAsset = TextureLocator.GetSystexTextureVirtualPath(Editor.Project, "systex");

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }
        }
    }

    public List<Task> LoadTextures(List<Task> tasks, ModelContainer container)
    {
        if (!CFG.Current.Viewport_Enable_Texturing)
            return tasks;

        // Map Pieces
        if (CFG.Current.ModelEditor_TextureLoad_MapPieces)
        {
            var texJob = ResourceManager.CreateNewJob($@"Map Piece Textures");

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
        if (CFG.Current.ModelEditor_TextureLoad_Characters)
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
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
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

        // Part
        if (CFG.Current.ModelEditor_TextureLoad_Parts)
        {
            var texJob = ResourceManager.CreateNewJob($@"Part Textures");

            foreach (ResourceDescriptor asset in LoadList_Part_Texture)
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

    public List<Task> LoadModels(List<Task> tasks, ModelContainer container)
    {
        // MapPieces
        if (CFG.Current.ModelEditor_ModelLoad_MapPieces)
        {
            var job = ResourceManager.CreateNewJob($@"Map Pieces");

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
        if (CFG.Current.ModelEditor_ModelLoad_Characters)
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

        // Assets
        if (CFG.Current.ModelEditor_ModelLoad_Objects)
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

        // Parts
        if (CFG.Current.ModelEditor_ModelLoad_Parts)
        {
            var job = ResourceManager.CreateNewJob($@"Parts");

            foreach (ResourceDescriptor asset in LoadList_Part_Model)
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
        if (CFG.Current.ModelEditor_ModelLoad_Collisions)
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


        return tasks;
    }

    public void UnloadModel(ModelWrapper modelWrapper)
    {
        Editor.ViewportSelection.ClearSelection(Editor);
        Editor.EditorActionManager.Clear();

        ResourceManager.ClearUnusedResources();

        if (modelWrapper.Container != null)
        {
            Editor.EntityTypeCache.RemoveModelFromCache(modelWrapper.Container);

            ModelInsightHelper.ClearEntry(modelWrapper.Container);

            modelWrapper.Container.Unload();
            modelWrapper.Container.Clear();
            modelWrapper.Container = null;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public void ScheduleTextureRefresh()
    {
        ResourceManager.SchedulePostTextureRefresh();
    }
}
