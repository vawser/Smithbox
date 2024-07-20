using DotNext;
using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Gui;
using StudioCore.Locators;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelResourceHandler : IResourceEventListener
    {
        public FLVER2 CurrentFLVER;
        public FlverModelInfo CurrentFLVERInfo;
        public string VirtualResourcePath = "";

        public hkRootLevelContainer ER_CollisionLow;
        public hkRootLevelContainer ER_CollisionHigh;

        public ModelEditorScreen Screen;

        public Task _loadingTask;

        public IViewport Viewport;

        public ModelResourceHandler(ModelEditorScreen screen, IViewport viewport)
        {
            Screen = screen;
            Viewport = viewport;
        }

        /// <summary>
        /// Loads a loose FLVER
        /// </summary>
        /// <param name="name"></param>
        public void LoadLooseFLVER(string name, string loosePath)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

            CurrentFLVERInfo = new FlverModelInfo(name, loosePath);

            LoadEditableModel(name, ModelEditorModelType.Loose);
            LoadRepresentativeModel(name, ModelEditorModelType.Loose);
        }

        /// <summary>
        /// Loads a Character FLVER
        /// </summary>
        /// <param name="name"></param>
        public void LoadCharacter(string name)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

            LoadEditableModel(name, ModelEditorModelType.Character);
            LoadRepresentativeModel(name, ModelEditorModelType.Character);

            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Character, "");
        }

        /// <summary>
        /// Loads a Asset FLVER
        /// </summary>
        public void LoadAsset(string name)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

            LoadEditableModel(name, ModelEditorModelType.Object);
            LoadEditableCollisionLow(name, ModelEditorModelType.Object);
            LoadEditableCollisionHigh(name, ModelEditorModelType.Object);
            LoadRepresentativeModel(name, ModelEditorModelType.Object);

            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Object, "");
        }

        /// <summary>
        /// Loads a Part FLVER
        /// </summary>
        public void LoadPart(string name)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

            LoadEditableModel(name, ModelEditorModelType.Parts);
            LoadRepresentativeModel(name, ModelEditorModelType.Parts);

            CurrentFLVERInfo = new FlverModelInfo(name, ModelEditorModelType.Parts, "");
        }

        /// <summary>
        /// Loads a MapPiece FLVER
        /// </summary>
        public void LoadMapPiece(string name, string mapId)
        {
            Screen.EditorActionManager.Clear();
            Screen.ModelHierarchy.ResetSelection();
            Screen.ModelHierarchy.ResetMultiSelection();
            Screen._selection.ClearSelection();

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

            if (modelType == ModelEditorModelType.Loose)
            {
                CurrentFLVER = FLVER2.Read(CurrentFLVERInfo.LoosePath);
            }
            else
            {
                if (modelAsset.AssetPath != null)
                {
                    if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                    {
                        if (modelType == ModelEditorModelType.MapPiece)
                        {
                            CurrentFLVER = FLVER2.Read(modelAsset.AssetPath);
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
                                    CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
                                    break;
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
                                        CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
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
                                            //TaskLogs.AddLog("New CurrentFLVER");
                                            CurrentFLVER = FLVER2.Read(reader.ReadFile(file));
                                            break;
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
        /// Loads the editable collision
        /// </summary>
        private void LoadEditableCollisionLow(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                ResourceDescriptor collisionAsset = AssetLocator.GetAssetGeomHKXBinder(modelid, "_l");

                if (collisionAsset.AssetPath != null)
                {
                    if (Smithbox.ProjectType is ProjectType.ER)
                    {
                        BND4Reader reader = new BND4Reader(collisionAsset.AssetPath);

                        foreach (var file in reader.Files)
                        {
                            var fileName = file.Name.ToLower();
                            var modelName = modelid.ToLower();

                            var fileBytes = reader.ReadFile(file);

                            if (fileName.Contains(modelName) && fileName.Contains(".hkx"))
                            {
                                HavokBinarySerializer serializer = new HavokBinarySerializer();
                                using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                                {
                                    ER_CollisionLow = (hkRootLevelContainer)serializer.Read(memoryStream);
                                }
                                break;
                            }
                        }
                        reader.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Loads the editable collision
        /// </summary>
        private void LoadEditableCollisionHigh(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                ResourceDescriptor collisionAsset = AssetLocator.GetAssetGeomHKXBinder(modelid, "_h");

                if (collisionAsset.AssetPath != null)
                {
                    if (Smithbox.ProjectType is ProjectType.ER)
                    {
                        BND4Reader reader = new BND4Reader(collisionAsset.AssetPath);

                        foreach (var file in reader.Files)
                        {
                            var fileName = file.Name.ToLower();
                            var modelName = modelid.ToLower();

                            var fileBytes = reader.ReadFile(file);

                            if (fileName.Contains(modelName) && fileName.Contains(".hkx"))
                            {
                                HavokBinarySerializer serializer = new HavokBinarySerializer();
                                using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                                {
                                    ER_CollisionHigh = (hkRootLevelContainer)serializer.Read(memoryStream);
                                }
                                break;
                            }
                        }
                        reader.Dispose();
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

            VirtualResourcePath = modelAsset.AssetVirtualPath;

            if (modelType == ModelEditorModelType.Loose)
            {
                modelAsset = new ResourceDescriptor();
                modelAsset.AssetVirtualPath = $"loose/flver/{CurrentFLVERInfo.LoosePath}";
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


        public ResourceDescriptor GetModelAssetDescriptor(string modelid, ModelEditorModelType modelType, string mapid = null)
        {
            ResourceDescriptor asset;

            switch (modelType)
            {
                case ModelEditorModelType.Character:
                    asset = ModelLocator.GetChrModel(modelid);
                    break;
                case ModelEditorModelType.Object:
                    asset = ModelLocator.GetObjModel(modelid);
                    break;
                case ModelEditorModelType.Parts:
                    asset = ModelLocator.GetPartsModel(modelid);
                    break;
                case ModelEditorModelType.MapPiece:
                    asset = ModelLocator.GetMapModel(mapid, modelid);
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
                // For loose files, save directly
                if (CurrentFLVERInfo.Type == ModelEditorModelType.Loose)
                {
                    // Backup loose file
                    File.Copy(CurrentFLVERInfo.LoosePath, $@"{CurrentFLVERInfo.LoosePath}.bak", true);

                    byte[] flverBytes = CurrentFLVER.Write();
                    File.WriteAllBytes(CurrentFLVERInfo.LoosePath, flverBytes);
                }
                // Copy the binder to the mod directory if it does not already exist.
                else
                {
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

            // Backup container file
            File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

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
        /// Save the PureFLVER model within BND3 container
        /// </summary>
        public void WriteModelBinderBND3()
        {
            FlverModelInfo info = CurrentFLVERInfo;
            byte[] fileBytes = null;

            // Backup container file
            File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);

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
    }
}
