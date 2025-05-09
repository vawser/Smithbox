using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Framework;
using StudioCore.ViewportNS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

public class ModelResourceManager : IResourceEventListener
{
    public ModelEditorScreen Editor;

    public ViewportActionManager EditorActionManager;
    public FlverContainer LoadedFlverContainer { get; set; }

    public ModelSelectionManager Selection;
    public ModelToolView ToolView;
    public ModelViewportManager ViewportManager;

    public Task _loadingTask;

    public IViewport Viewport;

    public MeshRenderableProxy _Flver_RenderMesh;
    public MeshRenderableProxy _LowCollision_RenderMesh;
    public MeshRenderableProxy _HighCollision_RenderMesh;

    public ResourceHandle<FlverResource> _flverhandle;
    public ResourceHandle<HavokCollisionResource> _lowCollisionHandle;
    public ResourceHandle<HavokCollisionResource> _highCollisionHandle;

    public ModelResourceManager(ModelEditorScreen screen, IViewport viewport)
    {
        Editor = screen;
        Viewport = viewport;
        EditorActionManager = screen.EditorActionManager;
        ToolView = screen.ToolView;
        ViewportManager = screen.ViewportManager;
        Selection = screen.Selection;

        LoadedFlverContainer = null;
    }

    public void OnProjectChanged()
    {
        if (_loadingTask != null)
        {
            TaskLogs.AddLog(
                "ModelResourceHandler loadingTask was not null during project switch. This may cause unexpected behavior.",
                LogLevel.Warning);
        }

        LoadedFlverContainer = null;
        _flverhandle?.Unload();
        _flverhandle = null;
        _Flver_RenderMesh?.Dispose();
        _Flver_RenderMesh = null;
    }

    public InternalFlver GetCurrentInternalFile()
    {
        if (LoadedFlverContainer == null)
            return null;

        if (LoadedFlverContainer.CurrentInternalFlver == null)
            return null;

        return LoadedFlverContainer.CurrentInternalFlver;
    }

    public bool HasCurrentFLVER()
    {
        if (LoadedFlverContainer == null)
            return false;

        if (LoadedFlverContainer.CurrentInternalFlver == null)
            return false;

        if (LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
            return false;

        return true;
    }

    public FLVER2 GetCurrentFLVER()
    {
        if (LoadedFlverContainer == null)
            return null;

        if (LoadedFlverContainer.CurrentInternalFlver == null)
            return null;

        if (LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
            return null;

        return LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER;
    }

    public void ResetState(string name)
    {
        EditorActionManager.Clear();
        Selection.ResetSelection();
        Selection.ResetMultiSelection();

        Editor._selection.ClearSelection(Editor);

        Editor.ToolView.ModelUsageSearch._searchInput = name;

        Editor._universe.UnloadTransformableEntities();

        if (_flverhandle != null)
        {
            //_flverhandle.CompleteRelease();
        }

        // HACK: clear all viewport collisions on load
        foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
        {
            if (item.Key.Contains("collision"))
            {
                item.Value.Release(true);
            }
        }
    }

    public void SetDefaultAssociatedModel()
    {
        // Set loaded FLVER to first file.
        if (LoadedFlverContainer.InternalFlvers.Count > 0)
        {
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();
        }
    }

    /// <summary>
    /// Loads a loose FLVER
    /// </summary>
    /// <param name="name"></param>
    public void LoadLooseFLVER(string name, string loosePath)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, loosePath);

        LoadEditableModel(name, name, FlverContainerType.Loose, null, loosePath);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Loose);
    }

    /// <summary>
    /// Loads a Character FLVER
    /// </summary>
    /// <param name="name"></param>
    public void LoadCharacter(string name)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, FlverContainerType.Character, "");

        LoadEditableModel(name, name, FlverContainerType.Character);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Character);
    }

    /// <summary>
    /// Loads a Enemy FLVER
    /// </summary>
    /// <param name="name">The name of the flver.</param>
    public void LoadEnemy(string name)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, FlverContainerType.Enemy, "");

        LoadEditableModel(name, name, FlverContainerType.Enemy);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Enemy);
    }

    /// <summary>
    /// Loads a Asset FLVER
    /// </summary>
    public void LoadAsset(string name)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        // Load HKX for collision
        Editor.CollisionManager.OnLoadModel(name, FlverContainerType.Object);

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, FlverContainerType.Object, "");

        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            if(Editor.CollisionManager.HavokContainers.ContainsKey($"{name}_h".ToLower()))
            {
                LoadedFlverContainer.ER_HighCollision = Editor.CollisionManager.HavokContainers[$"{name}_h".ToLower()];
            }
            if (Editor.CollisionManager.HavokContainers.ContainsKey($"{name}_l".ToLower()))
            {
                LoadedFlverContainer.ER_LowCollision = Editor.CollisionManager.HavokContainers[$"{name}_l".ToLower()];
            }
        }

        LoadEditableModel(name, name, FlverContainerType.Object);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Object);
    }

    /// <summary>
    /// Loads a Part FLVER
    /// </summary>
    public void LoadPart(string name)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, FlverContainerType.Parts, "");

        LoadEditableModel(name, name, FlverContainerType.Parts);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Parts);
    }

    /// <summary>
    /// Loads a MapPiece FLVER
    /// </summary>
    public void LoadMapPiece(string name, string mapId)
    {
        if (!ModelEditorUtils.IsSupportedProjectType(Editor.Project.ProjectType))
        {
            TaskLogs.AddLog($"Model Editor is not supported for {Editor.Project.ProjectType}.", LogLevel.Warning);
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(Editor, name, FlverContainerType.MapPiece, mapId);

        LoadEditableModel(name, name, FlverContainerType.MapPiece, mapId);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.MapPiece, mapId);
    }

    /// <summary>
    /// Loads the editable FLVER model, this is the model that the editor actually uses
    /// </summary>
    private void LoadEditableModel(string containerId, string modelid, FlverContainerType modelType, string mapid = null, string loosePath = "")
    {
        ResourceDescriptor modelAsset = GetModelAssetDescriptor(containerId, modelid, modelType, mapid);
        var loadPath = modelAsset.AssetPath;

        if (modelType is FlverContainerType.Loose)
        {
            loadPath = loosePath;
        }

        if (modelAsset == null)
            return;

        if (!File.Exists(loadPath))
            return;

        var binderType = LoadedFlverContainer.BinderType;
        var fileBytes = File.ReadAllBytes(loadPath);

        // Get the DCX Type for the container
        var reader = new BinaryReaderEx(false, fileBytes);
        SFUtil.GetDecompressedBR(reader, out DCX.Type dcxType);

        LoadedFlverContainer.CompressionType = dcxType;

        // Loose
        if (modelType is FlverContainerType.Loose)
        {
            var internalFlver = new InternalFlver();
            internalFlver.Name = modelid;
            internalFlver.ModelID = modelid;
            internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;
            internalFlver.CurrentFLVER = FLVER2.Read(LoadedFlverContainer.LoosePath);
            internalFlver.InitialFlverBytes = fileBytes;

            AddInternalFlver(internalFlver);
        }
        else if (modelType is FlverContainerType.MapPiece)
        {
            // DES, DS1, BB
            if (binderType is FlverBinderType.None)
            {
                var internalFlver = new InternalFlver();
                internalFlver.Name = modelid;
                internalFlver.ModelID = modelid;
                internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;
                internalFlver.CurrentFLVER = FLVER2.Read(modelAsset.AssetPath);
                internalFlver.InitialFlverBytes = fileBytes;

                AddInternalFlver(internalFlver);
            }

            // DS2
            if (binderType is FlverBinderType.BXF)
            {
                var bhdPath = modelAsset.AssetPath;
                var bdtPath = modelAsset.AssetPath.Replace("bhd", "bdt");

                using BXF4Reader bxfReader = new BXF4Reader(bhdPath, bdtPath);
                foreach (var file in bxfReader.Files)
                {
                    var fileName = file.Name.ToLower();
                    var modelName = modelid.ToLower();

                    if (fileName.Contains(modelName))
                    {
                        if (fileName.Contains(".flv.dcx"))
                        {
                            var internalFlver = new InternalFlver();

                            internalFlver.Name = Path.GetFileNameWithoutExtension(fileName);
                            internalFlver.ModelID = modelid;
                            internalFlver.CurrentFLVER = FLVER2.Read(bxfReader.ReadFile(file));
                            internalFlver.InitialFlverBytes = bxfReader.ReadFile(file).ToArray();
                            internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                            AddInternalFlver(internalFlver);
                        }
                    }
                }
            }

            // ACFA, ACV, ACVD, DS3, SDT, ER, AC6
            if (binderType is FlverBinderType.BND)
            {
                BinderReader bndReader;
                if (Editor.Project.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    bndReader = new BND3Reader(modelAsset.AssetPath);
                }
                else
                {
                    bndReader = new BND4Reader(modelAsset.AssetPath);
                }

                foreach (var file in bndReader.Files)
                {
                    var fileName = file.Name.ToLower();
                    var modelName = modelid.ToLower();

                    if (fileName.Contains(modelName) && (fileName.EndsWith(".flver") || fileName.EndsWith(".flv")))
                    {
                        var internalFlver = new InternalFlver();
                        internalFlver.Name = Path.GetFileNameWithoutExtension(fileName);
                        internalFlver.ModelID = modelid;
                        internalFlver.CurrentFLVER = FLVER2.Read(bndReader.ReadFile(file));
                        internalFlver.InitialFlverBytes = bndReader.ReadFile(file).ToArray();
                        internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                        AddInternalFlver(internalFlver);
                    }
                }

                bndReader.Dispose();
            }
        }
        else if (modelType is FlverContainerType.Character or FlverContainerType.Enemy or FlverContainerType.Parts or FlverContainerType.Object)
        {
            if (binderType is FlverBinderType.BND)
            {
                BinderReader bndReader;
                if (Editor.Project.ProjectType is ProjectType.ACFA or ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.ACV or ProjectType.ACVD)
                {
                    // BND3: DES, DS1
                    bndReader = new BND3Reader(modelAsset.AssetPath);
                }
                else
                {
                    // BND4: DS2, DS3, SDT, ER, AC6
                    bndReader = new BND4Reader(modelAsset.AssetPath);
                }

                foreach (var file in bndReader.Files)
                {
                    var fileName = file.Name.ToLower();
                    var modelName = modelid.ToLower();

                    if (fileName.Contains(modelName) && (fileName.EndsWith(".flver") || fileName.EndsWith(".flv")))
                    {
                        var proceed = true;

                        // DS2
                        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            proceed = false;

                            if (fileName.Length > 4 && fileName.Substring(fileName.Length - 3) == "flv")
                            {
                                proceed = true;
                            }
                        }

                        if (proceed)
                        {
                            var internalFlver = new InternalFlver();

                            internalFlver.Name = Path.GetFileNameWithoutExtension(fileName);
                            internalFlver.ModelID = modelid;
                            internalFlver.CurrentFLVER = FLVER2.Read(bndReader.ReadFile(file));
                            internalFlver.InitialFlverBytes = bndReader.ReadFile(file).ToArray();
                            internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                            AddInternalFlver(internalFlver);
                        }
                    }
                }

                bndReader.Dispose();
            }
        }
    }

    public void AddInternalFlver(InternalFlver internalFlver)
    {
        LoadedFlverContainer.InternalFlvers.Add(internalFlver);
        LoadedFlverContainer.CurrentInternalFlver = internalFlver;
    }

    /// <summary>
    /// Loads the viewport FLVER model, this is the model displayed in the viewport
    /// </summary>
    public void LoadRepresentativeModel(string containerId, string modelid, FlverContainerType modelType, string mapid = null)
    {
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            if (modelType is FlverContainerType.Object)
            {
                LoadCollisionInternal(modelid, "h");
                LoadCollisionInternal(modelid, "l");
            }
        }

        LoadModelInternal(containerId, modelid, modelType, mapid);
        LoadTexturesInternal(modelid, modelType, mapid);

    }

    /// <summary>
    /// Load model into the resource system
    /// </summary>
    private void LoadModelInternal(string containerId, string modelid, FlverContainerType modelType, string mapid = null)
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

        var modelAsset = new ResourceDescriptor();
        modelAsset.AssetVirtualPath = $"direct/flver/{modelid}";

        LoadedFlverContainer.ModelAssetDescriptor = modelAsset;

        if (CFG.Current.ModelEditor_ViewMeshes)
        {
            UpdateRenderMesh(modelAsset);

            if (modelAsset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(modelAsset.AssetArchiveVirtualPath, AccessLevel.AccessFull, false, ResourceManager.ResourceType.Flver);
            }
            else if (modelAsset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(modelAsset.AssetVirtualPath, AccessLevel.AccessFull);
            }

            _loadingTask = job.Complete();

            ResourceManager.AddResourceListener<FlverResource>(modelAsset.AssetVirtualPath, this, AccessLevel.AccessFull);
        }
    }

    /// <summary>
    /// Load textures into the resource system
    /// </summary>
    private void LoadTexturesInternal(string modelid, FlverContainerType modelType, string mapid = null)
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading textures");

        List<ResourceDescriptor> textureAssets = GetTextureAssetDescriptorList(modelid, modelType, mapid);

        // Add the loose tpfs parts have in AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            textureAssets.Add(TextureLocator.GetPartTpf_Ac6(Editor.Project, modelid));
        }

        foreach (var entry in textureAssets)
        {
            if (CFG.Current.Viewport_Enable_Rendering)
            {
                if (CFG.Current.Viewport_Enable_Texturing)
                {
                    if (entry.AssetArchiveVirtualPath != null)
                    {
                        if (!ResourceManager.IsResourceLoaded(entry.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                        {
                            job.AddLoadArchiveTask(entry.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Texture);
                        }
                    }
                    else if (entry.AssetVirtualPath != null)
                    {
                        if (!ResourceManager.IsResourceLoaded(entry.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                        {
                            job.AddLoadFileTask(entry.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                        }
                    }
                }
            }

            _loadingTask = job.Complete();
        }
    }

    /// <summary>
    /// Load collision into the resource system
    /// </summary>
    private void LoadCollisionInternal(string modelid, string postfix)
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading collision");

        ResourceDescriptor colAsset = AssetLocator.GetAssetGeomHKXBinder(Editor.Project, modelid, postfix);

        // Ignore if the col type doesn't exist
        if (!File.Exists(colAsset.AssetPath))
        {
            return;
        }

        if (CFG.Current.ModelEditor_ViewHighCollision && postfix == "h" ||
            CFG.Current.ModelEditor_ViewLowCollision && postfix == "l")
        {
            UpdateRenderMeshCollision(colAsset);

            if (colAsset.AssetArchiveVirtualPath != null)
            {
                if (!ResourceManager.IsResourceLoaded(colAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                {
                    job.AddLoadArchiveTask(colAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.CollisionHKX);
                }
            }
            else if (colAsset.AssetVirtualPath != null)
            {
                if (!ResourceManager.IsResourceLoaded(colAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                {
                    job.AddLoadFileTask(colAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            _loadingTask = job.Complete();

            ResourceManager.AddResourceListener<HavokCollisionResource>(colAsset.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);
        }
    }

    /// <summary>
    /// Get model resource descriptor
    /// </summary>
    public ResourceDescriptor GetModelAssetDescriptor(string containerId, string modelid, FlverContainerType modelType, string mapid = null)
    {
        ResourceDescriptor asset;

        switch (modelType)
        {
            case FlverContainerType.Character:
                asset = ModelLocator.GetChrModel(Editor.Project, containerId, modelid);
                break;
            case FlverContainerType.Enemy:
                asset = ModelLocator.GetEneModel(Editor.Project, modelid);
                break;
            case FlverContainerType.Object:
                asset = ModelLocator.GetObjModel(Editor.Project, containerId, modelid);
                break;
            case FlverContainerType.Parts:
                asset = ModelLocator.GetPartsModel(Editor.Project, containerId, modelid);
                break;
            case FlverContainerType.MapPiece:
                asset = ModelLocator.GetMapModel(Editor.Project, mapid, containerId, modelid);
                break;
            default:
                asset = ModelLocator.GetNullAsset();
                break;
        }

        return asset;
    }

    /// <summary>
    /// Get texture resource descriptors
    /// </summary>
    public List<ResourceDescriptor> GetTextureAssetDescriptorList(string modelid, FlverContainerType modelType, string mapid = null)
    {
        List<ResourceDescriptor> assets = new();

        switch (modelType)
        {
            case FlverContainerType.Character:
                assets.Add(TextureLocator.GetChrTextures(Editor.Project, modelid));
                break;
            case FlverContainerType.Enemy:
                assets.Add(TextureLocator.GetObjTextureContainer(Editor.Project, modelid));
                break;
            case FlverContainerType.Object:
                assets.Add(TextureLocator.GetObjTextureContainer(Editor.Project, modelid));
                break;
            case FlverContainerType.Parts:
                assets.Add(TextureLocator.GetPartTextureContainer(Editor.Project, modelid));
                break;
            case FlverContainerType.MapPiece:
                assets = TextureLocator.GetMapTextures(Editor.Project, mapid);
                break;
            default:
                assets.Add(ModelLocator.GetNullAsset());
                break;
        }

        return assets;
    }

    /// <summary>
    /// Save the FLVER model
    /// </summary>
    public void SaveModel()
    {
        if (LoadedFlverContainer == null)
        {
            TaskLogs.AddLog("Failed to save FLVER as LoadedFlverContainer is null.", LogLevel.Error);
            return;
        }

        if (LoadedFlverContainer.CurrentInternalFlver == null)
        {
            TaskLogs.AddLog("Failed to save FLVER as CurrentInternalFlver is null.", LogLevel.Error);
            return;
        }

        if (LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
        {
            TaskLogs.AddLog("Failed to save FLVER as CurrentFLVER is null.", LogLevel.Error);
            return;
        }

        var container = LoadedFlverContainer;
        var containerType = LoadedFlverContainer.Type;
        var binderType = LoadedFlverContainer.BinderType;

        // For loose files, save directly
        if (containerType is FlverContainerType.Loose)
        {
            WriteLooseFlver(container.LoosePath);
        }
        // Copy the binder to the mod directory if it does not already exist.
        else
        {
            var exists = false;

            // DS2 Map Pieces
            if (binderType is FlverBinderType.BXF)
            {
                var containerTitle = $"{LoadedFlverContainer.MapID}";

                exists = container.CopyBXFtoMod($"{containerTitle}.mapbhd");
                exists = container.CopyBXFtoMod($"{containerTitle}.mapbdt");
            }
            else
            {
                exists = container.CopyBinderToMod();
            }

            if (exists)
            {
                // DS1 / DS1R Map Pieces
                if(binderType is FlverBinderType.None)
                {
                    var directory = $"map\\{container.MapID}\\";
                    var path = $"map\\{container.MapID}\\{container.ContainerName}.flver";

                    var projectDirectory = $"{Editor.Project.ProjectPath}\\{directory}";
                    var savePath = $"{Editor.Project.ProjectPath}\\{path}";

                    if (!Directory.Exists(projectDirectory))
                    {
                        Directory.CreateDirectory(projectDirectory);
                    }

                    WriteLooseFlver(savePath);
                }

                // DS2 Map Pieces
                if (binderType is FlverBinderType.BXF)
                {
                    WriteModelBinderBXF();
                }

                // The rest
                if (binderType is FlverBinderType.BND)
                {
                    if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
                    {
                        WriteModelBinderBND3();
                    }
                    else
                    {
                        WriteModelBinderBND4();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used for loose .FLVER and DS1/DS1R saving
    /// </summary>
    private void WriteLooseFlver(string path)
    {
        var compressionType = LoadedFlverContainer.CompressionType;

        // Backup loose file
        if (File.Exists(path))
        {
            File.Copy(path, $@"{path}.bak", true);
        }

        byte[] flverBytes = null;

        flverBytes = LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER.Write(compressionType);

        WriteFile(flverBytes, path);
    }

    /// <summary>
    /// Save the FLVER model within BXF containers
    /// </summary>
    private void WriteModelBinderBXF()
    {
        var compressionType = LoadedFlverContainer.CompressionType;

        FlverContainer info = LoadedFlverContainer;

        byte[] bhdBytes = null;
        byte[] bdtBytes = null;

        var bhdPath = info.ModBinderPath;
        var bdtPath = info.ModBinderPath.Replace("bhd", "bdt");

        // Change the name to the map ID for DS2 map pieces
        if(Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var containerName = LoadedFlverContainer.ContainerName;
            bhdPath = bhdPath.Replace(containerName, info.MapID);
            bdtPath = bdtPath.Replace(containerName, info.MapID);
        }

        // Backup container files
        File.Copy(bhdPath, $@"{bhdPath}.bak", true);
        File.Copy(bdtPath, $@"{bdtPath}.bak", true);

        using (IBinder binder = BXF4.Read(bhdPath, bdtPath))
        {
            foreach (var file in binder.Files)
            {
                var curName = Path.GetFileNameWithoutExtension(file.Name);
                var curFileName = $"{Path.GetFileName(file.Name)}";

                foreach (var internalFlver in LoadedFlverContainer.InternalFlvers)
                {
                    if (curName.ToLower() == internalFlver.Name.ToLower() && curFileName.Contains(".flv"))
                    {
                        try
                        {
                            file.Bytes = internalFlver.CurrentFLVER.Write();
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"Failed to write FLVER file: {file.ID}.\n{ex}", LogLevel.Error);
                        }
                    }
                }
            }

            using (BXF4 writeBinder = binder as BXF4)
            {
                writeBinder.Write(out bhdBytes, out bdtBytes);
            }
        }

        TaskLogs.AddLog($"Saved model container BHD file at: {bhdPath}");
        TaskLogs.AddLog($"Saved model container BDT file at: {bdtPath}");

        // NOTE: memory-mapped files write directly to the passed paths when .Write is invoked,
        // so no need to handle the write process manually.
        //WriteFile(bhdBytes, $"{bhdPath}");
        //WriteFile(bdtBytes, $"{bdtPath}");
    }

    /// <summary>
    /// Save the FLVER model within BND4 container
    /// </summary>
    private void WriteModelBinderBND4()
    {
        var compressionType = LoadedFlverContainer.CompressionType;

        FlverContainer info = LoadedFlverContainer;

        byte[] fileBytes = null;

        // Backup container file
        File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

        using (IBinder binder = BND4.Read(info.ModBinderPath))
        {
            foreach (var file in binder.Files)
            {
                var curName = Path.GetFileNameWithoutExtension(file.Name);
                var curFileName = $"{Path.GetFileName(file.Name)}";

                foreach (var internalFlver in LoadedFlverContainer.InternalFlvers)
                {
                    if (curName.ToLower() == internalFlver.Name.ToLower() && curFileName.Contains(".flv"))
                    {
                        try
                        {
                            file.Bytes = internalFlver.CurrentFLVER.Write();
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"Failed to write FLVER file: {file.ID}.\n{ex}", LogLevel.Error);
                        }
                    }
                }
            }

            using (BND4 writeBinder = binder as BND4)
            {
                fileBytes = writeBinder.Write(compressionType);
            }
        }

        WriteFile(fileBytes, info.ModBinderPath);
    }

    /// <summary>
    /// Save the FLVER model within BND3 container
    /// </summary>
    public void WriteModelBinderBND3()
    {
        var compressionType = LoadedFlverContainer.CompressionType;

        FlverContainer info = LoadedFlverContainer;
        byte[] fileBytes = null;

        // Backup container file
        File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

        using (IBinder binder = BND3.Read(info.ModBinderPath))
        {
            foreach (var file in binder.Files)
            {
                var curName = Path.GetFileNameWithoutExtension(file.Name);
                var curFileName = $"{Path.GetFileName(file.Name)}";

                foreach (var internalFlver in LoadedFlverContainer.InternalFlvers)
                {
                    if (curName.ToLower() == internalFlver.Name.ToLower() && curFileName.Contains(".flv"))
                    {
                        try
                        {
                            file.Bytes = internalFlver.CurrentFLVER.Write();
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"Failed to write FLVER file: {file.ID}.\n{ex}", LogLevel.Error);
                        }
                    }
                }
            }

            // Then write those bytes to file
            using (BND3 writeBinder = binder as BND3)
            {
                fileBytes = writeBinder.Write(compressionType);
            }
        }

        WriteFile(fileBytes, info.ModBinderPath);
    }

    /// <summary>
    /// Write out a non-container file.
    /// </summary>
    public void WriteFile(byte[] data, string path)
    {
        if (data != null)
        {
            try
            {
                File.WriteAllBytes(path, data);
                TaskLogs.AddLog($"Saved FLVER file at: {path}");
            }
            catch (Exception ex)
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                TaskLogs.AddLog($"Failed to save FLVER file: {filename} at {path}\n{ex}");
            }
        }
    }


    /// <summary>
    /// Updated the viewport FLVER model render mesh
    /// </summary>
    public void UpdateRenderMesh(ResourceDescriptor modelAsset)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (_Flver_RenderMesh != null)
            _Flver_RenderMesh.Visible = false;

        if (CFG.Current.Viewport_Enable_Rendering)
        {
            var meshRenderableProxy = MeshRenderableProxy.MeshRenderableFromFlverResource(Editor.RenderScene, modelAsset.AssetVirtualPath, ModelMarkerType.None, null);
            meshRenderableProxy.World = Matrix4x4.Identity;

            _Flver_RenderMesh = meshRenderableProxy;
            _Flver_RenderMesh.Visible = true;
        }
    }

    /// <summary>
    /// Updated the viewport FLVER model render collision mesh
    /// </summary>
    public void UpdateRenderMeshCollision(ResourceDescriptor collisionAsset)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (CFG.Current.Viewport_Enable_Rendering)
        {
            // High Collision
            if (collisionAsset.AssetVirtualPath.Contains("_h"))
            {
                if (_HighCollision_RenderMesh != null)
                {
                    _HighCollision_RenderMesh.Dispose();
                }

                _HighCollision_RenderMesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(Editor.RenderScene, collisionAsset.AssetVirtualPath, ModelMarkerType.None, collisionAsset.AssetVirtualPath);
                _HighCollision_RenderMesh.World = Matrix4x4.Identity;
            }

            // Low Collision
            if (collisionAsset.AssetVirtualPath.Contains("_l"))
            {
                if (_LowCollision_RenderMesh != null)
                {
                    _LowCollision_RenderMesh.Dispose();
                }

                _LowCollision_RenderMesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(Editor.RenderScene, collisionAsset.AssetVirtualPath, ModelMarkerType.None, collisionAsset.AssetVirtualPath);
                _LowCollision_RenderMesh.World = Matrix4x4.Identity;
            }
        }
    }

    /// <summary>
    /// Viewport setup upon viewport FLVER model is loaded
    /// </summary>
    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        // Collision
        if (handle is ResourceHandle<HavokCollisionResource>)
        {
            var colHandle = (ResourceHandle<HavokCollisionResource>)handle;

            if (colHandle.AssetVirtualPath.Contains("_h"))
            {
                _highCollisionHandle = (ResourceHandle<HavokCollisionResource>)handle;
            }
            if (colHandle.AssetVirtualPath.Contains("_l"))
            {
                _lowCollisionHandle = (ResourceHandle<HavokCollisionResource>)handle;
            }
        }

        // FLVER
        if (handle is ResourceHandle<FlverResource>)
        {
            var curFlver = Editor.ResManager.GetCurrentFLVER();

            _flverhandle = (ResourceHandle<FlverResource>)handle;
            _flverhandle.Acquire();

            if (_Flver_RenderMesh != null)
            {
                BoundingBox box = _Flver_RenderMesh.GetBounds();
                Viewport.FrameBox(box);

                Vector3 dim = box.GetDimensions();
                var mindim = Math.Min(dim.X, Math.Min(dim.Y, dim.Z));
                var maxdim = Math.Max(dim.X, Math.Max(dim.Y, dim.Z));

                var minSpeed = 1.0f;
                var basespeed = Math.Max(minSpeed, (float)Math.Sqrt(mindim / 3.0f));
                Viewport.ViewportCamera.CameraMoveSpeed_Normal = basespeed;
                Viewport.ViewportCamera.CameraMoveSpeed_Slow = basespeed / 10.0f;
                Viewport.ViewportCamera.CameraMoveSpeed_Fast = basespeed * 10.0f;

                //Viewport.NearClip = Math.Max(0.001f, maxdim / 10000.0f);
            }

            // Update Model Container
            if (curFlver != null)
            {
                var currentFlverClone = curFlver.Clone();

                Editor._universe.LoadedModelContainer = new(Editor, Editor._universe, currentFlverClone, _Flver_RenderMesh);
            }

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                Editor._universe.ScheduleTextureRefresh();
            }
        }
    }

    /// <summary>
    /// Viewport setup upon viewport FLVER model is unloaded
    /// </summary>
    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (handle is ResourceHandle<FlverResource>)
        {
            _flverhandle = null;
        }

        if (handle is ResourceHandle<HavokCollisionResource>)
        {
            var colHandle = (ResourceHandle<HavokCollisionResource>)handle;

            if (colHandle.AssetVirtualPath.Contains("_h"))
            {
                _highCollisionHandle = (ResourceHandle<HavokCollisionResource>)handle;
                _highCollisionHandle.Acquire();
            }
            if (colHandle.AssetVirtualPath.Contains("_l"))
            {
                _lowCollisionHandle = (ResourceHandle<HavokCollisionResource>)handle;
                _lowCollisionHandle.Acquire();
            }
        }
    }
}
