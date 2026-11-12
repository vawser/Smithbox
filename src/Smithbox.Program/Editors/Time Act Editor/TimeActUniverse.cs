using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActUniverse : IUniverse
{
    public TimeActEditorView View;
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
    private List<Task> Tasks = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Model = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Texture = new();

    public TimeActUniverse(TimeActEditorView view, ProjectEntry project)
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
    public async void LoadModel(TimeActModelWrapper modelWrapper)
    {
        if (modelWrapper.Container != null)
        {
            Smithbox.LogError(this,
                LOC.Get("TAE_Universe_Model_Already_Loaded", modelWrapper.Name));
            return;
        }

        ResourceManager.ClearUnusedResources();
        View.ViewportSelection.ClearSelection();

        var newContainer = new TimeActContainer(View, Project, modelWrapper.Name);

        newContainer.Load(modelWrapper.FLVER, modelWrapper);

        SetupModelLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);
        SetupTextureLoadList(modelWrapper.Name.ToLower(), modelWrapper.Parent);

        modelWrapper.Container = newContainer;
        HasLoadedModel = true;

        // The model of a composite character is only a skeleton, so the rest of it comes from the part models
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering &&
            CFG.Current.ModelEditor_ModelLoad_Parts && CharacterComposition.IsComposite(modelWrapper.Name))
        {
            newContainer.AssignCharacterParts(View.CharacterPreview.GetAppearance());
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            Selection.ClearSelection();
            Selection.AddSelection(newContainer.RootObject);

            View.ViewportWindow.PendingFrameContainer = newContainer;
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
        {
            ResourceLoadTracker.BeginSession(modelWrapper.Name);

            try
            {
                ReserveLoadCategories();

                Tasks = LoadTextures(Tasks, newContainer);
                await Task.WhenAll(Tasks);
                Tasks = LoadModels(Tasks, newContainer);
                await Task.WhenAll(Tasks);

                ScheduleTextureRefresh();
            }
            finally
            {
                ResourceLoadTracker.EndSession(modelWrapper.Name);
            }
        }

        await Task.WhenAll(Tasks);
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
            Reserve(CFG.Current.ModelEditor_TextureLoad_Characters, LoadList_Character_Texture, "Character Textures");
            Reserve(CFG.Current.ModelEditor_TextureLoad_Objects, LoadList_Asset_Texture, "Asset Textures");
            Reserve(CFG.Current.ModelEditor_TextureLoad_Parts, LoadList_Part_Texture, "Part Textures");
        }

        Reserve(CFG.Current.ModelEditor_ModelLoad_Characters, LoadList_Character_Model, "Characters");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Objects, LoadList_Asset_Model, "Assets");
        Reserve(CFG.Current.ModelEditor_ModelLoad_Parts, LoadList_Part_Model, "Parts");
    }

    public void SetupModelLoadList(string modelName, TimeActContainerWrapper parent)
    {
        LoadList_Character_Model.Clear();
        LoadList_Asset_Model.Clear();
        LoadList_Part_Model.Clear();

        var loadList = new List<ResourceDescriptor>();

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
    }

    public void SetupTextureLoadList(string modelName, TimeActContainerWrapper parent)
    {
        LoadList_Character_Texture.Clear();
        LoadList_Asset_Texture.Clear();
        LoadList_Part_Texture.Clear();

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

        Task load = ResourceManager.DefaultPool.LoadAssets(View.Project, assets, options, onlyListedAssets);
        ResourceLoadTracker.MarkRequested(jobName);

        return load;
    }


    public List<Task> LoadTextures(List<Task> tasks, TimeActContainer container)
    {
        if (!CFG.Current.Viewport_Enable_Texturing)
            return tasks;

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

    public List<Task> LoadModels(List<Task> tasks, TimeActContainer container)
    {
        // Model archives can also contain texture packs, so those are loaded along with the models
        var modelTypes = ResourceType.Flver | ResourceType.Texture;

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

        return tasks;
    }

    public void UnloadModel(TimeActModelWrapper modelWrapper)
    {
        View.ViewportSelection.ClearSelection();
        // Only the actions on the model's entities go, as those are discarded with it. The time act edits
        // don't depend on the model, so they can still be undone after another source is opened.
        View.ViewportActionManager.KeepOnlyActionsTouching(TimeActAction.Scope);
        View.ActionManager.Clear();
        View.ViewportWindow.PendingFrameContainer = null;

        ResourceManager.ClearUnusedResources();

        if (modelWrapper.Container != null)
        {
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
