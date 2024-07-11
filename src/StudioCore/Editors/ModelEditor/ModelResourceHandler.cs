using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.PureFLVER.FLVER2;
using StudioCore.Gui;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelResourceHandler : IResourceEventListener
    {
        public Formats.PureFLVER.FLVER2.FLVER2 CurrentFLVER;
        public FlverModelInfo CurrentFLVERInfo;

        public ResourceHandle<FlverResource> _flverhandle;

        public ModelEditorScreen Screen;

        public Task _loadingTask;

        public MeshRenderableProxy _renderMesh;

        public IViewport Viewport;

        public ModelResourceHandler(ModelEditorScreen screen, IViewport viewport)
        {
            Screen = screen;
            Viewport = viewport;
        }

        /// <summary>
        /// Loads a Character FLVER
        /// </summary>
        /// <param name="name"></param>
        public void LoadCharacter(string name)
        {
            LoadEditableModel(name, ModelEditorModelType.Character);
            LoadRepresentativeModel(name, ModelEditorModelType.Character);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Character, "");
        }

        /// <summary>
        /// Loads a Asset FLVER
        /// </summary>
        public void LoadAsset(string name)
        {
            LoadEditableModel(name, ModelEditorModelType.Object);
            LoadRepresentativeModel(name, ModelEditorModelType.Object);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Object, "");
        }

        /// <summary>
        /// Loads a Part FLVER
        /// </summary>
        public void LoadPart(string name)
        {
            LoadEditableModel(name, ModelEditorModelType.Parts);
            LoadRepresentativeModel(name, ModelEditorModelType.Parts);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Parts, "");
        }

        /// <summary>
        /// Loads a MapPiece FLVER
        /// </summary>
        public void LoadMapPiece(string name, string mapId)
        {
            LoadEditableModel(name, ModelEditorModelType.MapPiece, mapId);
            LoadRepresentativeModel(name, ModelEditorModelType.MapPiece, mapId);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.MapPiece, mapId);
        }

        /// <summary>
        /// Loads the editable FLVER model, this is the model that the editor actually uses
        /// </summary>
        private void LoadEditableModel(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor modelAsset = GetModelAssetDescriptor(modelid, modelType, mapid);
            if(modelAsset.AssetPath != null)
            {
                if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                {
                    // BND3
                    BND3Reader reader = new BND3Reader(modelAsset.AssetPath);
                    foreach (var file in reader.Files)
                    {
                        if (file.Name == modelid)
                        {
                            CurrentFLVER = Formats.PureFLVER.FLVER2.FLVER2.Read(reader.ReadFile(file));
                        }
                        break;
                    }
                }
                else
                {
                    // BND4
                    BND4Reader reader = new BND4Reader(modelAsset.AssetPath);
                    foreach(var file in reader.Files)
                    {
                        if(file.Name == modelid)
                        {
                            CurrentFLVER = Formats.PureFLVER.FLVER2.FLVER2.Read(reader.ReadFile(file));
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the viewport FLVER model, this is the model displayed in the viewport
        /// </summary>
        private void LoadRepresentativeModel(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            LoadModelInternal(modelid, modelType, mapid);

            // If model ID has additional textures associated with it, load them
            if (Smithbox.BankHandler.AdditionalTextureInfo.HasAdditionalTextures(modelid))
            {
                foreach (var entry in Smithbox.BankHandler.AdditionalTextureInfo.GetAdditionalTextures(modelid))
                {
                    LoadModelInternal(entry, modelType, mapid, true);
                }
            }
        }

        /// <summary>
        /// Send the viewport FLVER model request to the Resource Manager
        /// </summary>
        private void LoadModelInternal(string modelid, ModelEditorModelType modelType, string mapid = null, bool skipModel = false)
        {
            ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

            ResourceDescriptor modelAsset = GetModelAssetDescriptor(modelid, modelType, mapid);
            ResourceDescriptor textureAsset = GetTextureAssetDescriptor(modelid, modelType, mapid);

            UpdateRenderMesh(modelAsset, skipModel);

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
                    // PIPELINE: resource path is adirect path (FLVER.DCX)
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


        public ResourceDescriptor GetModelAssetDescriptor(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor asset;

            switch (modelType)
            {
                case ModelEditorModelType.Character:
                    asset = ResourceModelLocator.GetChrModel(modelid);
                    break;
                case ModelEditorModelType.Object:
                    asset = ResourceModelLocator.GetObjModel(modelid);
                    break;
                case ModelEditorModelType.Parts:
                    asset = ResourceModelLocator.GetPartsModel(modelid);
                    break;
                case ModelEditorModelType.MapPiece:
                    asset = ResourceModelLocator.GetMapModel(mapid, modelid);
                    break;
                default:
                    asset = ResourceModelLocator.GetNullAsset();
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
                    asset = ResourceTextureLocator.GetChrTextures(modelid);
                    break;
                case ModelEditorModelType.Object:
                    asset = ResourceTextureLocator.GetObjTextureContainer(modelid);
                    break;
                case ModelEditorModelType.Parts:
                    asset = ResourceTextureLocator.GetPartTextureContainer(modelid);
                    break;
                case ModelEditorModelType.MapPiece:
                    asset = ResourceModelLocator.GetNullAsset();
                    break;
                default:
                    asset = ResourceModelLocator.GetNullAsset();
                    break;
            }

            return asset;
        }

        /// <summary>
        /// Save the PureFLVER model
        /// </summary>
        public void SaveModel()
        {
            if (CurrentFLVER == null)
            {
                TaskLogs.AddLog("Failed to save FLVER as current FLVER is null.");
                return;
            }

            if (CurrentFLVERInfo != null)
            {
                // Copy the binder to the mod directory if it does not already exist.

                var exists = CurrentFLVERInfo.CopyBinderToMod();

                if (exists)
                {

                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        if (CurrentFLVERInfo.Type == ModelEditorModelType.MapPiece)
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

        /// <summary>
        /// Save the PureFLVER model within BND4 container
        /// </summary>
        private void WriteModelBinderBND4()
        {
            FlverModelInfo info = CurrentFLVERInfo;

            byte[] fileBytes = null;

            using (IBinder binder = BND4.Read(DCX.Decompress(info.ModBinderPath)))
            {
                foreach (var file in binder.Files)
                {
                    var curFileName = $"{Path.GetFileName(file.Name)}";

                    if (curFileName == info.FlverFileName)
                    {
                        try
                        {
                            file.Bytes = CurrentFLVER.Write();
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
                        }
                    }
                }

                // Then write those bytes to file
                BND4 writeBinder = binder as BND4;

                switch (Smithbox.ProjectType)
                {
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
                File.WriteAllBytes(info.ModBinderPath, fileBytes);
                TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
            }
        }

        /// <summary>
        /// Save the PureFLVER model within BND3 container
        /// </summary>
        public void WriteModelBinderBND3()
        {
            FlverModelInfo info = CurrentFLVERInfo;
            byte[] fileBytes = null;

            using (IBinder binder = BND3.Read(DCX.Decompress(info.ModBinderPath)))
            {
                foreach (var file in binder.Files)
                {
                    var curFileName = $"{Path.GetFileName(file.Name)}";

                    if (curFileName == info.FlverFileName)
                    {
                        try
                        {
                            file.Bytes = CurrentFLVER.Write();
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
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
                File.WriteAllBytes(info.ModBinderPath, fileBytes);
                TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
            }
        }

        /// <summary>
        /// Viewport setup upon viewport FLVER model is loaded
        /// </summary>
        public void OnResourceLoaded(IResourceHandle handle, int tag)
        {
            _flverhandle = (ResourceHandle<FlverResource>)handle;
            _flverhandle.Acquire();

            if (_renderMesh != null)
            {
                BoundingBox box = _renderMesh.GetBounds();
                Viewport.FrameBox(box);

                Vector3 dim = box.GetDimensions();
                var mindim = Math.Min(dim.X, Math.Min(dim.Y, dim.Z));
                var maxdim = Math.Max(dim.X, Math.Max(dim.Y, dim.Z));

                var minSpeed = 1.0f;
                var basespeed = Math.Max(minSpeed, (float)Math.Sqrt(mindim / 3.0f));
                Viewport.WorldView.CameraMoveSpeed_Normal = basespeed;
                Viewport.WorldView.CameraMoveSpeed_Slow = basespeed / 10.0f;
                Viewport.WorldView.CameraMoveSpeed_Fast = basespeed * 10.0f;

                Viewport.NearClip = Math.Max(0.001f, maxdim / 10000.0f);
            }

            if (_flverhandle.IsLoaded && _flverhandle.Get() != null)
            {
                FlverResource r = _flverhandle.Get();
                if (r.Flver != null)
                {
                    Screen._universe.UnloadModels(true);
                    Screen._universe.LoadFlverInModelEditor(r.Flver, _renderMesh, CurrentFLVERInfo.ModelName);
                }
            }

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                Screen._universe.ScheduleTextureRefresh();
            }
        }

        /// <summary>
        /// Viewport setup upon viewport FLVER model is unloaded
        /// </summary>
        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            _flverhandle = null;
        }

        /// <summary>
        /// Updated the viewport FLVER model render mesh
        /// </summary>
        public void UpdateRenderMesh(ResourceDescriptor modelAsset, bool skipModel = false)
        {
            if (Universe.IsRendering)
            {
                // Ignore this if we are only loading textures
                if (!skipModel)
                {
                    if (_renderMesh != null)
                    {
                        _renderMesh.Dispose();
                    }

                    _renderMesh = MeshRenderableProxy.MeshRenderableFromFlverResource(Screen.RenderScene, modelAsset.AssetVirtualPath, ModelMarkerType.None, null);
                    _renderMesh.World = Matrix4x4.Identity;
                }
            }
        }
    }
}
