using HKLib.hk2018.hkHashMapDetail;
using HKLib.hk2018.hkWeakPtrTest;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.PureFLVER.FLVER2;
using StudioCore.Gui;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Platform;
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
        public string VirtualResourcePath = "";

        public ModelEditorScreen Screen;

        public Task _loadingTask;

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
            Screen.ModelHierarchy.ResetSelection();
            LoadEditableModel(name, ModelEditorModelType.Character);
            LoadRepresentativeModel(name, ModelEditorModelType.Character);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Character, "");
        }

        /// <summary>
        /// Loads a Asset FLVER
        /// </summary>
        public void LoadAsset(string name)
        {
            Screen.ModelHierarchy.ResetSelection();
            LoadEditableModel(name, ModelEditorModelType.Object);
            LoadRepresentativeModel(name, ModelEditorModelType.Object);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Object, "");
        }

        /// <summary>
        /// Loads a Part FLVER
        /// </summary>
        public void LoadPart(string name)
        {
            Screen.ModelHierarchy.ResetSelection();
            LoadEditableModel(name, ModelEditorModelType.Parts);
            LoadRepresentativeModel(name, ModelEditorModelType.Parts);
            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Parts, "");
        }

        /// <summary>
        /// Loads a MapPiece FLVER
        /// </summary>
        public void LoadMapPiece(string name, string mapId)
        {
            Screen.ModelHierarchy.ResetSelection();
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

            //TaskLogs.AddLog(modelAsset.AssetPath);

            if (modelAsset.AssetPath != null)
            {
                if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                {
                    // BND3
                    BND3Reader reader = new BND3Reader(modelAsset.AssetPath);
                    foreach (var file in reader.Files)
                    {
                        var fileName = file.Name.ToLower();
                        var modelName = modelid.ToLower();

                        if (fileName.Contains(modelName) && fileName.Contains(".flv"))
                        {
                            CurrentFLVER = Formats.PureFLVER.FLVER2.FLVER2.Read(reader.ReadFile(file));
                            break;
                        }
                    }
                    reader.Dispose();
                }
                else
                {
                    // BND4
                    BND4Reader reader = new BND4Reader(modelAsset.AssetPath);
                    foreach(var file in reader.Files)
                    {
                        var fileName = file.Name.ToLower();
                        var modelName = modelid.ToLower();

                        //TaskLogs.AddLog(fileName);
                        //TaskLogs.AddLog(modelName);

                        if (fileName.Contains(modelName) && fileName.Contains(".flv"))
                        {
                            //TaskLogs.AddLog("New CurrentFLVER");
                            CurrentFLVER = Formats.PureFLVER.FLVER2.FLVER2.Read(reader.ReadFile(file));
                            break;
                        }
                    }
                    reader.Dispose();
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

            VirtualResourcePath = modelAsset.AssetVirtualPath;

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

            bool success = true;
            List<string> issues = new List<string>();
            (success, issues) = VerifyDataIntegrity();

            // Verift Data Integrity
            if (!success)
            {
                string issuesStr = "";
                foreach(var entry in issues)
                {
                    issuesStr = $"{issuesStr}\n{entry}";
                }

                var result = PlatformUtils.Instance.MessageBox($"This model currently has invalid index values:\n{issuesStr}\n\nDo you wish to proceed?", "Warning", MessageBoxButtons.OKCancel);

                if(result == DialogResult.Cancel)
                {
                    return;
                }
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

        // This checks index properties to see if they point to existing entries.
        // If not, they are added to the issues list
        public (bool, List<string>) VerifyDataIntegrity()
        {
            bool success = true;
            List<string> issues = new List<string>();

            // Dummies
            for (int i = 0; i < CurrentFLVER.Dummies.Count; i++)
            {
                var dummy = CurrentFLVER.Dummies[i];

                if(dummy.ParentBoneIndex >= CurrentFLVER.Nodes.Count || dummy.ParentBoneIndex < -1)
                {
                    issues.Add($"Dummy {i} has invalid ParentBoneIndex value: {dummy.ParentBoneIndex}");
                    success = false;
                }
                if (dummy.AttachBoneIndex >= CurrentFLVER.Nodes.Count || dummy.AttachBoneIndex < -1)
                {
                    issues.Add($"Dummy {i} has invalid AttachBoneIndex value: {dummy.AttachBoneIndex}");
                    success = false;
                }
            }

            // Materials
            for (int i = 0; i < CurrentFLVER.Materials.Count; i++)
            {
                var material = CurrentFLVER.Materials[i];

                if (material.GXIndex >= CurrentFLVER.GXLists.Count || material.GXIndex < -1)
                {
                    issues.Add($"Material {i} has invalid GXIndex value: {material.GXIndex}");
                    success = false;
                }
            }

            // Nodes
            for (int i = 0; i < CurrentFLVER.Nodes.Count; i++)
            {
                var node = CurrentFLVER.Nodes[i];

                if (node.ParentIndex >= CurrentFLVER.Nodes.Count || node.ParentIndex < -1)
                {
                    issues.Add($"Node {i} has invalid ParentIndex value: {node.ParentIndex}");
                    success = false;
                }
                if (node.FirstChildIndex >= CurrentFLVER.Nodes.Count || node.FirstChildIndex < -1)
                {
                    issues.Add($"Node {i} has invalid FirstChildIndex value: {node.FirstChildIndex}");
                    success = false;
                }
                if (node.NextSiblingIndex >= CurrentFLVER.Nodes.Count || node.NextSiblingIndex < -1)
                {
                    issues.Add($"Node {i} has invalid NextSiblingIndex value: {node.NextSiblingIndex}");
                    success = false;
                }
                if (node.PreviousSiblingIndex >= CurrentFLVER.Nodes.Count || node.PreviousSiblingIndex < -1)
                {
                    issues.Add($"Node {i} has invalid PreviousSiblingIndex value: {node.PreviousSiblingIndex}");
                    success = false;
                }
            }

            // Meshes
            for (int i = 0; i < CurrentFLVER.Meshes.Count; i++)
            {
                var mesh = CurrentFLVER.Meshes[i];

                if (mesh.MaterialIndex >= CurrentFLVER.Materials.Count || mesh.MaterialIndex < -1)
                {
                    issues.Add($"Mesh {i} has invalid MaterialIndex value: {mesh.MaterialIndex}");
                    success = false;
                }
                if (mesh.NodeIndex >= CurrentFLVER.Nodes.Count || mesh.NodeIndex < -1)
                {
                    issues.Add($"Mesh {i} has invalid NodeIndex value: {mesh.NodeIndex}");
                    success = false;
                }
            }

            return (success, issues);
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
            Screen.ViewportHandler.OnResourceLoaded(handle, tag);
        }

        /// <summary>
        /// Viewport setup upon viewport FLVER model is unloaded
        /// </summary>
        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            Screen.ViewportHandler.OnResourceUnloaded(handle, tag);
        }
    }
}
