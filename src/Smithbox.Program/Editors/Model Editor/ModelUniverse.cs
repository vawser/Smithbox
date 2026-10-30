using Microsoft.Extensions.Logging;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Logger;
using StudioCore.Renderer;

namespace StudioCore.Editors.ModelEditor;

public class ModelUniverse : IUniverse
{
    public ModelEditorView View;
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

    public ModelUniverse(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        RenderScene = view.RenderScene;
        Selection = view.ViewportSelection;

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
            Smithbox.LogError(this,
                LOC.Get("MODEL_Universe_Model_Already_Loaded", modelWrapper.Name));
            return;
        }

        ResourceManager.ClearUnusedResources();
        View.ViewportSelection.ClearSelection();

        var newContainer = new ModelContainer(View, Project, modelWrapper.Name);

        newContainer.Load(modelWrapper.FLVER, modelWrapper);

        SetupModelLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);
        SetupTextureLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);

        modelWrapper.Container = newContainer;

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            Selection.ClearSelection();
            Selection.AddSelection(newContainer.RootObject);

            View.ViewportWindow.PendingFrameContainer = newContainer;
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            Tasks = LoadTextures(Tasks, newContainer);
            await Task.WhenAll(Tasks);
            Tasks = LoadModels(Tasks, newContainer);
            await Task.WhenAll(Tasks);

            ScheduleTextureRefresh();
        }

        await Task.WhenAll(Tasks);
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
                        var name = ModelLocator.GetMapModelName(View.Project, mapID, modelName);
                        var modelAsset = ModelLocator.GetMapModel(View.Project, mapID, name, name);

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
                var modelAsset = ModelLocator.GetChrModel(View.Project, modelName, modelName);

                if (modelAsset.IsValid())
                    LoadList_Character_Model.Add(modelAsset);
            }
        }

        // Object / Asset
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
        {
            if (modelName.StartsWith('o') || (modelName.StartsWith("AEG") || modelName.StartsWith("aeg")))
            {
                var modelAsset = ModelLocator.GetObjModel(View.Project, modelName, modelName);

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
                var modelAsset = ModelLocator.GetPartsModel(View.Project, modelName, modelName);

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
                        var modelAsset = ModelLocator.GetMapCollisionModel(View.Project, mapID,
                        ModelLocator.GetMapModelName(View.Project, mapID, modelName));

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
                    foreach (ResourceDescriptor asset in TextureLocator.GetMapTextureVirtualPaths(View.Project, mapID))
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
                var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, modelName, false);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);

                // BND
                textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, modelName, true);

                if (textureAsset.IsValid())
                    LoadList_Character_Texture.Add(textureAsset);
            }
        }

        // Object
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
        {
            if (modelName.StartsWith('o'))
            {
                var textureAsset = TextureLocator.GetObjectTextureVirtualPath(View.Project, modelName);

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
            }

            // Assets
            if (modelName.StartsWith("AEG") || modelName.StartsWith("aeg"))
            {
                var textureAsset = TextureLocator.GetAssetTextureVirtualPath(View.Project, modelName);

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
                var textureAsset = TextureLocator.GetPartTextureVirtualPath(View.Project, modelName);

                if (textureAsset.IsValid())
                    LoadList_Part_Texture.Add(textureAsset);
            }
        }

        // AAT
        if (CFG.Current.ModelEditor_TextureLoad_Misc)
        {
            if (View.Project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                var textureAsset = TextureLocator.GetCharacterCommonTextureVirtualPath(View.Project, "common_body");

                if (textureAsset.IsValid())
                    LoadList_Asset_Texture.Add(textureAsset);
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
                    LoadList_Asset_Texture.Add(textureAsset);
            }
        }
    }

    /// <summary>
    ///     Loads the containers that hold a set of assets into the default pool as one job.
    /// </summary>
    private Task LoadAssets(IEnumerable<ResourceDescriptor> assets, string jobName, ResourceType types,
        bool onlyListedAssets = false)
    {
        var options = new LoadOptions
        {
            JobName = jobName,
            Types = types,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        };

        return ResourceManager.DefaultPool.LoadAssets(View.Project, assets, options, onlyListedAssets);
    }

    public List<Task> LoadTextures(List<Task> tasks, ModelContainer container)
    {
        if (!CFG.Current.Viewport_Enable_Texturing)
            return tasks;

        // Map Pieces
        if (CFG.Current.ModelEditor_TextureLoad_MapPieces)
        {
            task = LoadAssets(LoadList_Map_Texture, "Map Piece Textures", ResourceType.Texture);
            tasks.Add(task);
        }

        // Character
        if (CFG.Current.ModelEditor_TextureLoad_Characters)
        {
            task = LoadAssets(LoadList_Character_Texture, "Character Textures", ResourceType.Texture);
            tasks.Add(task);
        }

        // Objects / Assets
        if (CFG.Current.ModelEditor_TextureLoad_Objects)
        {
            task = LoadAssets(LoadList_Asset_Texture, "Asset Textures", ResourceType.Texture);
            tasks.Add(task);
        }

        // Parts
        if (CFG.Current.ModelEditor_TextureLoad_Parts)
        {
            task = LoadAssets(LoadList_Part_Texture, "Part Textures", ResourceType.Texture);
            tasks.Add(task);
        }

        return tasks;
    }

    public List<Task> LoadModels(List<Task> tasks, ModelContainer container)
    {
        // Model archives can also contain texture packs, so those are loaded along with the models
        var modelTypes = ResourceType.Flver | ResourceType.Texture;

        // MapPieces
        if (CFG.Current.ModelEditor_ModelLoad_MapPieces)
        {
            task = LoadAssets(LoadList_MapPiece_Model, "Map Pieces", modelTypes);
            tasks.Add(task);
        }

        // Characters
        if (CFG.Current.ModelEditor_ModelLoad_Characters)
        {
            task = LoadAssets(LoadList_Character_Model, "Characters", modelTypes);
            tasks.Add(task);
        }

        // Objects
        if (CFG.Current.ModelEditor_ModelLoad_Objects)
        {
            task = LoadAssets(LoadList_Asset_Model, "Assets", modelTypes);
            tasks.Add(task);
        }

        // Parts
        if (CFG.Current.ModelEditor_ModelLoad_Parts)
        {
            task = LoadAssets(LoadList_Part_Model, "Parts", modelTypes);
            tasks.Add(task);
        }

        // Collisions: only the collisions that were asked for are loaded from the collision archive
        if (CFG.Current.ModelEditor_ModelLoad_Collisions)
        {
            task = LoadAssets(LoadList_Collision, "Collisions", ResourceType.CollisionHKX, onlyListedAssets: true);
            tasks.Add(task);
        }

        return tasks;
    }

    public void UnloadModel(ModelWrapper modelWrapper)
    {
        View.ViewportSelection.ClearSelection();
        View.ViewportActionManager.Clear();
        View.ActionManager.Clear();
        View.ViewportWindow.PendingFrameContainer = null;

        ResourceManager.ClearUnusedResources();

        if (modelWrapper.Container != null)
        {
            View.EntityTypeCache.RemoveModelFromCache(modelWrapper.Container);

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
