using DotNext;
using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.Serialization.hk2018.Binary;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{

    public class ModelResourceHandler : IResourceEventListener
    {
        public FlverContainer LoadedFlverContainer { get; set; }

        public ModelEditorScreen Screen;

        public Task _loadingTask;

        public IViewport Viewport;

        public ModelResourceHandler(ModelEditorScreen screen, IViewport viewport)
        {
            Screen = screen;
            Viewport = viewport;
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

            if(LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
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

        public void SetLoadedState(string name)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

            Screen.ToolWindow.ModelUsageSearch._searchInput = name;
        }

        /// <summary>
        /// Loads a loose FLVER
        /// </summary>
        /// <param name="name"></param>
        public void LoadLooseFLVER(string name, string loosePath)
        {
            SetLoadedState(name);

            LoadedFlverContainer = new FlverContainer(name, loosePath);

            LoadEditableModel(name, name, ModelEditorModelType.Loose);

            // Set loaded FLVER to first file.
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

            LoadRepresentativeModel(name, name, ModelEditorModelType.Loose);
        }

        /// <summary>
        /// Loads a Character FLVER
        /// </summary>
        /// <param name="name"></param>
        public void LoadCharacter(string name)
        {
            SetLoadedState(name);

            LoadedFlverContainer = new FlverContainer(name, ModelEditorModelType.Character, "");

            LoadEditableModel(name, name, ModelEditorModelType.Character);

            // Set loaded FLVER to first file.
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

            LoadRepresentativeModel(name, name, ModelEditorModelType.Character);
        }

        /// <summary>
        /// Loads a Asset FLVER
        /// </summary>
        public void LoadAsset(string name)
        {
            HavokCollisionManager.Screen = Screen;
            HavokCollisionManager.OnLoadModel(name, ModelEditorModelType.Object);

            SetLoadedState(name);

            LoadedFlverContainer = new FlverContainer(name, ModelEditorModelType.Object, "");

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

            LoadEditableModel(name, name, ModelEditorModelType.Object);

            // Set loaded FLVER to first file.
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

            LoadRepresentativeModel(name, name, ModelEditorModelType.Object);
        }

        /// <summary>
        /// Loads a Part FLVER
        /// </summary>
        public void LoadPart(string name)
        {
            SetLoadedState(name);

            LoadedFlverContainer = new FlverContainer(name, ModelEditorModelType.Parts, "");

            LoadEditableModel(name, name, ModelEditorModelType.Parts);

            // Set loaded FLVER to first file.
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

            LoadRepresentativeModel(name, name, ModelEditorModelType.Parts);
        }

        /// <summary>
        /// Loads a MapPiece FLVER
        /// </summary>
        public void LoadMapPiece(string name, string mapId)
        {
            SetLoadedState(name);

            LoadedFlverContainer = new FlverContainer(name, ModelEditorModelType.MapPiece, mapId);

            LoadEditableModel(name, name, ModelEditorModelType.MapPiece, mapId);

            // Set loaded FLVER to first file.
            LoadedFlverContainer.CurrentInternalFlver = LoadedFlverContainer.InternalFlvers.First();

            LoadRepresentativeModel(name, name, ModelEditorModelType.MapPiece, mapId);
        }

        /// <summary>
        /// Loads the editable FLVER model, this is the model that the editor actually uses
        /// </summary>
        private void LoadEditableModel(string containerId, string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor modelAsset = GetModelAssetDescriptor(containerId, modelid, modelType, mapid);

            //TaskLogs.AddLog(modelAsset.AssetPath);

            if (modelType == ModelEditorModelType.Loose)
            {
                var internalFlver = new InternalFlver();

                internalFlver.Name = modelid;
                internalFlver.ModelID = modelid;
                internalFlver.CurrentFLVER = FLVER2.Read(LoadedFlverContainer.LoosePath);
                internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                LoadedFlverContainer.CurrentInternalFlver = internalFlver;
            }
            else
            {
                if (modelAsset.AssetPath != null)
                {
                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        if (modelType == ModelEditorModelType.MapPiece)
                        {
                            var internalFlver = new InternalFlver();

                            internalFlver.Name = modelid;
                            internalFlver.ModelID = modelid;
                            internalFlver.CurrentFLVER = FLVER2.Read(modelAsset.AssetPath);
                            internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                            LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                            LoadedFlverContainer.CurrentInternalFlver = internalFlver;
                        }
                        else
                        {
                            // BND3
                            BND3Reader reader = new BND3Reader(modelAsset.AssetPath);
                            foreach (var file in reader.Files)
                            {
                                var fileName = file.Name.ToLower();
                                var modelName = modelid.ToLower();

                                if (fileName.Contains(modelName) && (fileName.EndsWith(".flver") || fileName.EndsWith(".flv")))
                                {
                                    var internalFlver = new InternalFlver();

                                    internalFlver.Name = Path.GetFileNameWithoutExtension(fileName);
                                    internalFlver.ModelID = modelid;
                                    internalFlver.CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
                                    internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                                    LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                                    LoadedFlverContainer.CurrentInternalFlver = internalFlver;
                                }
                            }
                            reader.Dispose();
                        }
                    }
                    // DS2, DS3, SDT, ER, AC6
                    else
                    {
                        // DS2 Map Pieces
                        if (modelAsset.AssetPath.Contains("mapbhd"))
                        {
                            var bhdPath = modelAsset.AssetPath;
                            var bdtPath = modelAsset.AssetPath.Replace("bhd", "bdt");
                            BXF4Reader reader = new BXF4Reader(bhdPath, bdtPath);
                            foreach (var file in reader.Files)
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
                                        internalFlver.CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
                                        internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                                        LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                                        LoadedFlverContainer.CurrentInternalFlver = internalFlver;
                                    }
                                }
                            }
                        }
                        // DS2, DS3, SDT, ER, AC6
                        else
                        {
                            // BND4
                            BND4Reader reader = new BND4Reader(modelAsset.AssetPath);
                            foreach (var file in reader.Files)
                            {
                                var fileName = file.Name.ToLower();
                                var modelName = modelid.ToLower();

                                //TaskLogs.AddLog(fileName);
                                //TaskLogs.AddLog(modelName);

                                if (fileName.Contains(modelName))
                                {
                                    if ((fileName.EndsWith(".flver") || fileName.EndsWith(".flv")))
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
                                            internalFlver.CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
                                            internalFlver.VirtualResourcePath = modelAsset.AssetVirtualPath;

                                            LoadedFlverContainer.InternalFlvers.Add(internalFlver);
                                            LoadedFlverContainer.CurrentInternalFlver = internalFlver;
                                        }
                                    }
                                }
                            }
                            reader.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the viewport FLVER model, this is the model displayed in the viewport
        /// </summary>
        public void LoadRepresentativeModel(string containerId, string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            if (modelType is ModelEditorModelType.Object)
            {
                LoadCollisionInternal(modelid, "h");
                LoadCollisionInternal(modelid, "l");
            }

            LoadModelInternal(containerId, modelid, modelType, mapid);

            // If model ID has additional textures associated with it, load them
            if (Smithbox.BankHandler.AdditionalTextureInfo.HasAdditionalTextures(modelid))
            {
                foreach (var entry in Smithbox.BankHandler.AdditionalTextureInfo.GetAdditionalTextures(modelid))
                {
                    LoadModelInternal(containerId, entry, modelType, mapid, true);
                }
            }
        }

        /// <summary>
        /// Send the viewport FLVER model request to the Resource Manager
        /// </summary>
        private void LoadModelInternal(string containerId, string modelid, ModelEditorModelType modelType, string mapid = null, bool skipModel = false)
        {
            ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

            ResourceDescriptor modelAsset = GetModelAssetDescriptor(containerId, modelid, modelType, mapid);
            ResourceDescriptor textureAsset = GetTextureAssetDescriptor(modelid, modelType, mapid);

            if (modelType == ModelEditorModelType.Loose)
            {
                modelAsset = new ResourceDescriptor();
                modelAsset.AssetVirtualPath = $"loose/flver/{LoadedFlverContainer.LoosePath}";
            }

            Screen.ViewportHandler.UpdateRenderMesh(modelAsset, skipModel);

            // PIPELINE: resource has not already been loaded
            if (!ResourceManager.IsResourceLoadedOrInFlight(modelAsset.AssetVirtualPath, AccessLevel.AccessFull))
            {
                // Ignore this if we are only loading textures
                if (!skipModel)
                {
                    // PIPELINE: resource path is a archive path (MAPBND.DCX or MAPBHD/MAPBDT)
                    if (modelAsset.AssetArchiveVirtualPath != null)
                    {
                        job.AddLoadArchiveTask(modelAsset.AssetArchiveVirtualPath, AccessLevel.AccessFull, false, ResourceManager.ResourceType.Flver);
                    }
                    // PIPELINE: resource path is a direct path (FLVER.DCX)
                    else if (modelAsset.AssetVirtualPath != null)
                    {
                        job.AddLoadFileTask(modelAsset.AssetVirtualPath, AccessLevel.AccessFull);
                    }
                }

                if (Universe.IsRendering)
                {
                    if (CFG.Current.Viewport_Enable_Texturing)
                    {
                        if (textureAsset.AssetArchiveVirtualPath != null)
                        {
                            job.AddLoadArchiveTask(textureAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Texture);
                        }
                        else if (textureAsset.AssetVirtualPath != null)
                        {
                            job.AddLoadFileTask(textureAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                        }
                    }
                }

                _loadingTask = job.Complete();
            }

            ResourceManager.AddResourceListener<FlverResource>(modelAsset.AssetVirtualPath, this, AccessLevel.AccessFull);
        }

        private void LoadCollisionInternal(string modelid, string postfix)
        {
            ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading collision");

            ResourceDescriptor colAsset = AssetLocator.GetAssetGeomHKXBinder(modelid, postfix);

            // Ignore if the col type doesn't exist
            if (!File.Exists(colAsset.AssetPath))
            {
                return;
            }

            Screen.ViewportHandler.UpdateRenderMeshCollision(colAsset);

            // PIPELINE: resource has not already been loaded
            if (!ResourceManager.IsResourceLoadedOrInFlight(colAsset.AssetVirtualPath, AccessLevel.AccessFull))
            {
                // PIPELINE: resource path is a archive path (MAPBND.DCX or MAPBHD/MAPBDT)
                if (colAsset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(colAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.CollisionHKX);
                }
                // PIPELINE: resource path is a direct path (FLVER.DCX)
                else if (colAsset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(colAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }

                _loadingTask = job.Complete();
            }

            ResourceManager.AddResourceListener<HavokCollisionResource>(colAsset.AssetVirtualPath, this, AccessLevel.AccessFull);
        }

        public ResourceDescriptor GetModelAssetDescriptor(string containerId, string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor asset;

            switch (modelType)
            {
                case ModelEditorModelType.Character:
                    asset = ModelLocator.GetChrModel(containerId, modelid);
                    break;
                case ModelEditorModelType.Object:
                    asset = ModelLocator.GetObjModel(containerId, modelid);
                    break;
                case ModelEditorModelType.Parts:
                    asset = ModelLocator.GetPartsModel(containerId, modelid);
                    break;
                case ModelEditorModelType.MapPiece:
                    asset = ModelLocator.GetMapModel(mapid, containerId, modelid);
                    break;
                default:
                    asset = ModelLocator.GetNullAsset();
                    break;
            }

            return asset;
        }

        public ResourceDescriptor GetTextureAssetDescriptor(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor asset;

            switch (modelType)
            {
                case ModelEditorModelType.Character:
                    asset = TextureLocator.GetChrTextures(modelid);
                    break;
                case ModelEditorModelType.Object:
                    asset = TextureLocator.GetObjTextureContainer(modelid);
                    break;
                case ModelEditorModelType.Parts:
                    asset = TextureLocator.GetPartTextureContainer(modelid);
                    break;
                case ModelEditorModelType.MapPiece:
                    asset = ModelLocator.GetNullAsset();
                    break;
                default:
                    asset = ModelLocator.GetNullAsset();
                    break;
            }

            return asset;
        }

        /// <summary>
        /// Save the Pure FLVER model
        /// </summary>
        public void SaveModel()
        {
            if (LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER == null)
            {
                TaskLogs.AddLog("Failed to save FLVER as current FLVER is null.");
                return;
            }

            if (LoadedFlverContainer != null)
            {
                // For loose files, save directly
                if (LoadedFlverContainer.Type == ModelEditorModelType.Loose)
                {
                    WriteLooseFlver();
                }
                // Copy the binder to the mod directory if it does not already exist.
                else
                {
                    var exists = LoadedFlverContainer.CopyBinderToMod();

                    if (exists)
                    {
                        if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                        {
                            if (LoadedFlverContainer.Type == ModelEditorModelType.MapPiece)
                            {
                                // TODO
                                //WriteModelFlver(); // DS1 doesn't wrap the mappiece flver within a container
                            }
                            else
                            {
                                WriteModelBinderBND3();
                            }
                        }
                        else
                        {
                            WriteModelBinderBND4();
                        }
                    }
                }
            }
        }

        private void WriteLooseFlver()
        {
            // Backup loose file
            File.Copy(LoadedFlverContainer.LoosePath, $@"{LoadedFlverContainer.LoosePath}.bak", true);

            byte[] flverBytes = LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER.Write();
            File.WriteAllBytes(LoadedFlverContainer.LoosePath, flverBytes);
            TaskLogs.AddLog($"Saved model at: {LoadedFlverContainer.LoosePath}");
        }

        /// <summary>
        /// Save the FLVER model within BND4 container
        /// </summary>
        private void WriteModelBinderBND4()
        {
            FlverContainer info = LoadedFlverContainer;

            byte[] fileBytes = null;

            // Backup container file
            File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

            using (IBinder binder = BND4.Read(DCX.Decompress(info.ModBinderPath)))
            {
                foreach (var file in binder.Files)
                {
                    var curName = Path.GetFileNameWithoutExtension(file.Name);
                    var curFileName = $"{Path.GetFileName(file.Name)}";

                    foreach(var internalFlver in LoadedFlverContainer.InternalFlvers)
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
                BND4 writeBinder = binder as BND4;

                switch (Smithbox.ProjectType)
                {
                    case ProjectType.BB:
                    case ProjectType.DS3:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                        break;
                    case ProjectType.SDT:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                        break;
                    case ProjectType.ER:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                        break;
                    case ProjectType.AC6:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                        break;
                    default:
                        TaskLogs.AddLog($"Invalid ProjectType during Model Editor Save");
                        return;
                }
            }

            if (fileBytes != null)
            {
                try
                {
                    File.WriteAllBytes(info.ModBinderPath, fileBytes);
                    TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"Failed to save model: {info.ModBinderPath}\n{ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Save the FLVER model within BND3 container
        /// </summary>
        public void WriteModelBinderBND3()
        {
            FlverContainer info = LoadedFlverContainer;
            byte[] fileBytes = null;

            // Backup container file
            File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

            using (IBinder binder = BND3.Read(DCX.Decompress(info.ModBinderPath)))
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
                BND3 writeBinder = binder as BND3;

                switch (Smithbox.ProjectType)
                {
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                        break;
                    default:
                        TaskLogs.AddLog($"Invalid ProjectType during Model Editor Save");
                        return;
                }
            }

            if (fileBytes != null)
            {
                try
                {
                    File.WriteAllBytes(info.ModBinderPath, fileBytes);
                    TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"Failed to save model: {info.ModBinderPath}\n{ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Viewport setup upon viewport FLVER model is loaded
        /// </summary>
        public void OnResourceLoaded(IResourceHandle handle, int tag)
        {
            Screen.ViewportHandler.OnResourceLoaded(handle, tag);
        }

        /// <summary>
        /// Viewport setup upon viewport FLVER model is unloaded
        /// </summary>
        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            Screen.ViewportHandler.OnResourceUnloaded(handle, tag);
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
}
