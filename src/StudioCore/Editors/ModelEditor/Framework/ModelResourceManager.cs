using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelResourceManager : IResourceEventListener
{
    public ViewportActionManager EditorActionManager;
    public FlverContainer LoadedFlverContainer { get; set; }

    public ModelEditorScreen Screen;
    public Universe Universe;
    public ModelSelectionManager Selection;
    public ModelToolView ToolView;
    public ModelViewportManager ViewportManager;

    public Task _loadingTask;

    public IViewport Viewport;

    public ModelResourceManager(ModelEditorScreen screen, IViewport viewport)
    {
        Screen = screen;
        Viewport = viewport;
        EditorActionManager = screen.EditorActionManager;
        Universe = screen._universe;
        ToolView = screen.ToolView;
        ViewportManager = screen.ViewportManager;
        Selection = screen.Selection;

        LoadedFlverContainer = null;
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

        Screen._selection.ClearSelection();

        Screen.ToolView.ModelUsageSearch._searchInput = name;

        Screen._universe.UnloadTransformableEntities();

        if (ViewportManager._flverhandle != null)
        {
            ViewportManager._flverhandle.CompleteRelease();
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
        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(name, loosePath);

        LoadEditableModel(name, name, FlverContainerType.Loose);

        // Set loaded FLVER to first file.
        LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

        LoadRepresentativeModel(name, name, FlverContainerType.Loose);
    }

    /// <summary>
    /// Loads a Character FLVER
    /// </summary>
    /// <param name="name"></param>
    public void LoadCharacter(string name)
    {
        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(name, FlverContainerType.Character, "");

        LoadEditableModel(name, name, FlverContainerType.Character);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Character);
    }

    /// <summary>
    /// Loads a Asset FLVER
    /// </summary>
    public void LoadAsset(string name)
    {
        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        // Load HKX for collision
        HavokCollisionManager.Screen = Screen;
        HavokCollisionManager.OnLoadModel(name, FlverContainerType.Object);

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(name, FlverContainerType.Object, "");

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            if (HavokCollisionManager.HavokContainers.ContainsKey($"{name}_h".ToLower()))
            {
                LoadedFlverContainer.ER_HighCollision = HavokCollisionManager.HavokContainers[$"{name}_h".ToLower()];
            }
            if (HavokCollisionManager.HavokContainers.ContainsKey($"{name}_l".ToLower()))
            {
                LoadedFlverContainer.ER_LowCollision = HavokCollisionManager.HavokContainers[$"{name}_l".ToLower()];
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
        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(name, FlverContainerType.Parts, "");

        LoadEditableModel(name, name, FlverContainerType.Parts);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.Parts);
    }

    /// <summary>
    /// Loads a MapPiece FLVER
    /// </summary>
    public void LoadMapPiece(string name, string mapId)
    {
        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        ResetState(name);

        LoadedFlverContainer = new FlverContainer(name, FlverContainerType.MapPiece, mapId);

        LoadEditableModel(name, name, FlverContainerType.MapPiece, mapId);
        SetDefaultAssociatedModel();
        LoadRepresentativeModel(name, name, FlverContainerType.MapPiece, mapId);
    }

    /// <summary>
    /// Loads the editable FLVER model, this is the model that the editor actually uses
    /// </summary>
    private void LoadEditableModel(string containerId, string modelid, FlverContainerType modelType, string mapid = null)
    {
        ResourceDescriptor modelAsset = GetModelAssetDescriptor(containerId, modelid, modelType, mapid);

        if (!File.Exists(modelAsset.AssetPath))
            return;

        var binderType = LoadedFlverContainer.BinderType;

        var fileBytes = File.ReadAllBytes(modelAsset.AssetPath);

        // Get the DCX Type for the container
        DCX.Type dcxType;
        var reader = new BinaryReaderEx(false, fileBytes);
        SFUtil.GetDecompressedBR(reader, out dcxType);

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

                using (BXF4Reader bxfReader = new BXF4Reader(bhdPath, bdtPath))
                {
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
            }

            // DS3, SDT, ER, AC6
            if (binderType is FlverBinderType.BND)
            {
                using (BND4Reader bndReader = new BND4Reader(modelAsset.AssetPath))
                {

                    foreach (var file in bndReader.Files)
                    {
                        var fileName = file.Name.ToLower();
                        var modelName = modelid.ToLower();

                        if (fileName.Contains(modelName))
                        {
                            if (fileName.EndsWith(".flver") || fileName.EndsWith(".flv"))
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
                }
            }
        }
        else if (modelType is FlverContainerType.Character or FlverContainerType.Parts or FlverContainerType.Object)
        {
            if (binderType is FlverBinderType.BND)
            {
                // BND3: DES, DS1
                if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
                {
                    using (BND3Reader bndReader = new BND3Reader(modelAsset.AssetPath))
                    {
                        foreach (var file in bndReader.Files)
                        {
                            var fileName = file.Name.ToLower();
                            var modelName = modelid.ToLower();

                            if (fileName.Contains(modelName) && (fileName.EndsWith(".flver") || fileName.EndsWith(".flv")))
                            {
                                var internalFlver = new InternalFlver();
                                internalFlver.Name = Path.GetFileNameWithoutExtension(fileName);
                                internalFlver.ModelID = modelid;
                                internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;
                                internalFlver.CurrentFLVER = FLVER2.Read(bndReader.ReadFile(file));
                                internalFlver.InitialFlverBytes = bndReader.ReadFile(file).ToArray();

                                AddInternalFlver(internalFlver);
                            }
                        }
                    }
                }
                // BND4: DS2, DS3, SDT, ER, AC6
                else
                {
                    using (BND4Reader bndReader = new BND4Reader(modelAsset.AssetPath))
                    {
                        foreach (var file in bndReader.Files)
                        {
                            var fileName = file.Name.ToLower();
                            var modelName = modelid.ToLower();

                            if (fileName.Contains(modelName))
                            {
                                if (fileName.EndsWith(".flver") || fileName.EndsWith(".flv"))
                                {
                                    var proceed = true;

                                    // DS2
                                    if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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

                                        LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                                        LoadedFlverContainer.CurrentInternalFlver = internalFlver;
                                    }
                                }
                            }
                        }
                    }
                }
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
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            if (modelType is FlverContainerType.Object)
            {
                LoadCollisionInternal(modelid, "h");
                LoadCollisionInternal(modelid, "l");
            }
        }

        LoadModelInternal(containerId, modelid, modelType, mapid);
        LoadTexturesInternal(modelid, modelType, mapid);

        // If model ID has additional textures associated with it, load them
        if (Smithbox.BankHandler.AdditionalTextureInfo.HasAdditionalTextures(modelid))
        {
            foreach (var entry in Smithbox.BankHandler.AdditionalTextureInfo.GetAdditionalTextures(modelid))
            {
                LoadTexturesInternal(modelid, modelType, mapid);
            }
        }
    }

    /// <summary>
    /// Load model into the resource system
    /// </summary>
    private void LoadModelInternal(string containerId, string modelid, FlverContainerType modelType, string mapid = null)
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

        /*
        ResourceDescriptor modelAsset = GetModelAssetDescriptor(containerId, modelid, modelType, mapid);

        if (modelType == ModelEditorModelType.Loose)
        {
            modelAsset = new ResourceDescriptor();
            modelAsset.AssetVirtualPath = $"loose/flver/{LoadedFlverContainer.LoosePath}";
        }
        */

        var modelAsset = new ResourceDescriptor();
        modelAsset.AssetVirtualPath = $"direct/flver/{modelid}";

        LoadedFlverContainer.ModelAssetDescriptor = modelAsset;

        if (CFG.Current.ModelEditor_ViewMeshes)
        {
            Screen.ViewportManager.UpdateRenderMesh(modelAsset);

            if (modelAsset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(modelAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Flver);
            }
            else if (modelAsset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(modelAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            _loadingTask = job.Complete();

            ResourceManager.AddResourceListener<FlverResource>(modelAsset.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);
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
        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            textureAssets.Add(TextureLocator.GetPartTpf_Ac6(modelid));
        }

        foreach (var entry in textureAssets)
        {
            if (Universe.IsRendering)
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

        ResourceDescriptor colAsset = AssetLocator.GetAssetGeomHKXBinder(modelid, postfix);

        // Ignore if the col type doesn't exist
        if (!File.Exists(colAsset.AssetPath))
        {
            return;
        }

        if (CFG.Current.ModelEditor_ViewHighCollision && postfix == "h" ||
            CFG.Current.ModelEditor_ViewLowCollision && postfix == "l")
        {
            Screen.ViewportManager.UpdateRenderMeshCollision(colAsset);

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
                asset = ModelLocator.GetChrModel(containerId, modelid);
                break;
            case FlverContainerType.Object:
                asset = ModelLocator.GetObjModel(containerId, modelid);
                break;
            case FlverContainerType.Parts:
                asset = ModelLocator.GetPartsModel(containerId, modelid);
                break;
            case FlverContainerType.MapPiece:
                asset = ModelLocator.GetMapModel(mapid, containerId, modelid);
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
                assets.Add(TextureLocator.GetChrTextures(modelid));
                break;
            case FlverContainerType.Object:
                assets.Add(TextureLocator.GetObjTextureContainer(modelid));
                break;
            case FlverContainerType.Parts:
                assets.Add(TextureLocator.GetPartTextureContainer(modelid));
                break;
            case FlverContainerType.MapPiece:
                assets = TextureLocator.GetMapTextures(mapid);
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
            TaskLogs.AddLog("Failed to save FLVER as LoadedFlverContainer is null.");
            return;
        }

        if (LoadedFlverContainer.CurrentInternalFlver == null)
        {
            TaskLogs.AddLog("Failed to save FLVER as CurrentInternalFlver is null.");
            return;
        }

        if (LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
        {
            TaskLogs.AddLog("Failed to save FLVER as CurrentFLVER is null.");
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
                var containerName = $"{LoadedFlverContainer.MapID}";

                exists = container.CopyBXFtoMod($"{containerName}.mapbhd");
                exists = container.CopyBXFtoMod($"{containerName}.mapbdt");
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

                    var projectDirectory = $"{Smithbox.ProjectRoot}\\{directory}";
                    var savePath = $"{Smithbox.ProjectRoot}\\{path}";

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
                    if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
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
        if(Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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
                            TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
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
                            TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
                        }
                    }
                }
            }

            using (BND4 writeBinder = binder as BND4)
            {
                fileBytes = writeBinder.Write(compressionType);
            }
        }

        TaskLogs.AddLog($"Saved model container BND file at: {info.ModBinderPath}");

        // NOTE: memory-mapped files write directly to the passed paths when .Write is invoked,
        // so no need to handle the write process manually.
        //WriteFile(fileBytes, info.ModBinderPath);
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
                            TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
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

        TaskLogs.AddLog($"Saved model container BND file at: {info.ModBinderPath}");

        // NOTE: memory-mapped files write directly to the passed paths when .Write is invoked,
        // so no need to handle the write process manually.
        //WriteFile(fileBytes, info.ModBinderPath);
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
                TaskLogs.AddLog($"Saved model at: {path}");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to save model: {path}\n{ex.ToString()}");
            }
        }
    }

    /// <summary>
    /// Viewport setup upon viewport FLVER model is loaded
    /// </summary>
    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        Screen.ViewportManager.OnResourceLoaded(handle, tag);
    }

    /// <summary>
    /// Viewport setup upon viewport FLVER model is unloaded
    /// </summary>
    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        Screen.ViewportManager.OnResourceUnloaded(handle, tag);
    }

    public void OnProjectChange()
    {
        if (_loadingTask != null)
        {
            TaskLogs.AddLog(
                "ModelResourceHandler loadingTask was not null during project switch. This may cause unexpected behavior.",
                LogLevel.Warning);
        }

        LoadedFlverContainer = null;
    }
}
