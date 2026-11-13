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

    /// <summary>
    /// Whether a model has been loaded in this view, so the viewport environment (map lighting and irradiance
    /// volume) is only loaded once there is something for it to light
    /// </summary>
    public bool HasLoadedModel;

    public ViewportSelection Selection { get; }

    private Task task;

    // Counts the loads and unloads, so a load that finishes after another has begun knows it is out of date
    private int _loadGeneration;

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

        // Viewport_Enable_Rendering is left to the user: the view always has a render scene, and setting it here
        // switched rendering back on for every editor whenever a view was created
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
        HasLoadedModel = true;

        // A model loaded again with unsaved edits is drawn from them, not from the stored file
        if (modelWrapper.IsDirty)
        {
            newContainer.MarkPreviewStale(immediate: true);
        }

        // The model of a composite character is only a skeleton, so the rest of it comes from the part models
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering &&
            CFG.Current.ModelEditor_ModelLoad_Parts && CharacterComposition.IsComposite(modelWrapper.Name))
        {
            newContainer.AssignCharacterParts(View.ModelCharacterPreview.GetAppearance());
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            Selection.ClearSelection();
            Selection.AddSelection(newContainer.RootObject);

            View.ViewportWindow.PendingFrameContainer = newContainer;
        }

        await LoadResources(modelWrapper, newContainer);
    }

    /// <summary>
    /// Brings back a model parked in this view (see <see cref="ModelEditorView.LoadModel"/>): its objects are put
    /// back in the scene as they were, and the resources it is drawn from are loaded again in case another model's
    /// load released them meanwhile. Nothing is read from the model file.
    /// </summary>
    public async void RestoreParkedModel(ModelWrapper modelWrapper, ModelContainer container)
    {
        container.Unpark();

        SetupModelLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);
        SetupTextureLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);

        HasLoadedModel = true;

        // A parked model was edited (it has history), so it may be drawn from an in-memory preview. Those are kept
        // while used, but a fresh one also covers a save made while it was parked.
        container.MarkPreviewStale(immediate: true);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            View.ViewportWindow.PendingFrameContainer = container;
        }

        await LoadResources(modelWrapper, container);
    }

    /// <summary>
    /// Loads the textures and models the container draws from into the default pool. Resources already loaded are
    /// left as they are.
    /// </summary>
    private async Task LoadResources(ModelWrapper modelWrapper, ModelContainer container)
    {
        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan || !CFG.Current.Viewport_Enable_Rendering)
            return;

        ResourceLoadTracker.BeginSession(modelWrapper.Name);

        var loadGeneration = ++_loadGeneration;

        try
        {
            ReserveLoadCategories();

            // Each load keeps its own tasks, so they don't pile up over the life of the view
            var textureTasks = LoadTextures(new List<Task>(), container);
            await Task.WhenAll(textureTasks);

            var modelTasks = LoadModels(new List<Task>(), container);
            await Task.WhenAll(modelTasks);

            // Another model was loaded or this one unloaded meanwhile, so the refresh is left to that load
            if (loadGeneration == _loadGeneration)
            {
                ScheduleTextureRefresh();
            }
        }
        catch (Exception e)
        {
            // The callers are async void, so an exception would otherwise end the program
            Smithbox.LogError(this, LOC.Get("MODEL_Universe_Model_Load_Failed", modelWrapper.Name), e);
        }
        finally
        {
            ResourceLoadTracker.EndSession(modelWrapper.Name);
        }
    }

    /// <summary>
    /// Takes the loaded model out of the scene while keeping its objects, for <see cref="ModelEditorView.LoadModel"/>
    /// </summary>
    public void ParkModel(ModelContainer container)
    {
        // A load still running for this model mustn't refresh into the next one
        _loadGeneration++;

        View.ViewportSelection.ClearSelection();

        if (View.ViewportWindow.PendingFrameContainer == container)
        {
            View.ViewportWindow.PendingFrameContainer = null;
        }

        View.EntityTypeCache.RemoveModelFromCache(container);

        container.Park();
    }

    /// <summary>
    /// Releases a parked model's objects and drawables, dropping its history
    /// </summary>
    public void DiscardParkedModel(ModelContainer container)
    {
        View.EntityTypeCache.RemoveModelFromCache(container);

        container.Unload();
        container.Clear();

        ResourceManager.ClearUnusedResources();
    }

    /// <summary>
    ///     Lists everything this model is going to load, so it can be displayed before its load begins.
    /// </summary>
    private void ReserveLoadCategories()
    {
        void Reserve(bool enabled, HashSet<ResourceDescriptor> assets, string name)
        {
            if (enabled && assets.Count > 0)
            {
                ResourceLoadTracker.Reserve(name);
            }
        }

        if (CFG.Current.Viewport_Enable_Texturing)
        {
            Reserve(CFG.Current.ModelEditor_TextureLoad_MapPieces, LoadList_Map_Texture, "Map Piece Textures");
            Reserve(CFG.Current.ModelEditor_TextureLoad_Characters, LoadList_Character_Texture, "Character Textures");
            Reserve(CFG.Current.ModelEditor_TextureLoad_Objects, LoadList_Asset_Texture, "Asset Textures");
            Reserve(CFG.Current.ModelEditor_TextureLoad_Parts, LoadList_Part_Texture, "Part Textures");
        }

        Reserve(CFG.Current.ModelEditor_ModelLoad_MapPieces, LoadList_MapPiece_Model, "Map Pieces");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Characters, LoadList_Character_Model, "Characters");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Objects, LoadList_Asset_Model, "Assets");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Parts, LoadList_Part_Model, "Parts");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Collisions, LoadList_Collision, "Collisions");
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
        if (CFG.Current.ModelEditor_ModelLoad_Objects)
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
            if (IsPartModelName(modelName))
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

    /// <summary>
    /// Whether a model is a part: arms, legs, bodies, heads and weapons in every game, plus the hair (hr),
    /// accessories (as) and faces (fc) of Demon's Souls
    /// </summary>
    public bool IsPartModelName(string modelName)
    {
        foreach (var prefix in new[] { "am", "lg", "bd", "hd", "wp" })
        {
            if (modelName.StartsWith(prefix) || modelName.StartsWith(prefix.ToUpperInvariant()))
                return true;
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.DES)
        {
            foreach (var prefix in new[] { "hr", "as", "fc" })
            {
                if (modelName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
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

                if (CharacterComposition.IsComposite(modelName) &&
                    CharacterComposition.GetCommonTexture(View.Project) is { } commonTexture)
                {
                    LoadList_Character_Texture.Add(commonTexture);
                }
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
            if (IsPartModelName(modelName))
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
            else if (View.Project.Descriptor.ProjectType is ProjectType.DES)
            {
                foreach (var textureAsset in TextureLocator.GetDesCommonTextureVirtualPaths(View.Project))
                {
                    if (textureAsset.IsValid())
                        LoadList_Asset_Texture.Add(textureAsset);
                }
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

        Task load = ResourceManager.DefaultPool.LoadAssets(View.Project, assets, options, onlyListedAssets);
        ResourceLoadTracker.MarkRequested(jobName);

        return load;
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
        _loadGeneration++;

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
