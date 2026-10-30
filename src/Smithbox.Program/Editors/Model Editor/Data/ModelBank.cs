using Andre.IO.VFS;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ModelEditor;

public class ModelBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Models = new();

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> MapPieces = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Characters = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Assets = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Parts = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Collisions = new();

    public ModelBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Map Pieces
        foreach (var entry in Project.Locator.MapPieceFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);
            newEntry.DeriveMapID();

            if(!MapPieces.ContainsKey(entry))
                MapPieces.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Characters
        foreach (var entry in Project.Locator.ChrFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Characters.ContainsKey(entry))
                Characters.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Assets
        foreach (var entry in Project.Locator.AssetFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Assets.ContainsKey(entry))
                Assets.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Parts
        foreach (var entry in Project.Locator.PartFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Parts.ContainsKey(entry))
                Parts.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Collisions
        foreach (var entry in Project.Locator.CollisionFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Collisions.ContainsKey(entry))
                Collisions.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        return true;
    }

    /// <summary>
    /// Save all models that have been modified
    /// </summary>
    public bool SaveAllModels()
    {
        var success = true;

        foreach (var container in Models.Values)
        {
            foreach (var model in container.Models)
            {
                // Only save the models that have been modified
                if (!model.IsDirty)
                    continue;

                if (!model.Save())
                    success = false;
            }
        }

        return success;
    }

    #region Dispose
    public void Dispose()
    {
        Models.Clear();
        MapPieces.Clear();
        Characters.Clear();
        Assets.Clear();
        Parts.Clear();
        Collisions.Clear();

        Models = null;
        MapPieces = null;
        Characters = null;
        Assets = null;
        Parts = null;
        Collisions = null;
    }
    #endregion
}

public class ModelContainerWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string MapID { get; set; }

    public string Name { get; set; }
    public string Path { get; set; }

    public List<ModelWrapper> Models { get; set; }

    public ModelContainerWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;

        Models = new();
    }

    /// <summary>
    /// Used to store the map ID for map pieces and collision wrappers
    /// </summary>
    public void DeriveMapID()
    {
        var parts = Path.Split("/");

        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            // '/map/m30/m30_00_00_00/...'
            if(parts.Length >= 3)
            {
                MapID = parts[3];
            }
        }
        else
        {
            // '/map/m30_00_00_00/...'
            if (parts.Length >= 2)
            {
                MapID = parts[2];

                if(Project.Descriptor.ProjectType is ProjectType.DS2 
                    or ProjectType.DS2S)
                {
                    MapID = FilePathUtils.GetPureFilename(parts[3]);
                }
            }
        }
    }

    public void PopulateModelList()
    {
        // Keep the wrappers that are loaded or have unsaved changes, so their state is not lost
        var existingModels = new Dictionary<string, ModelWrapper>();

        foreach (var model in Models)
        {
            if (model.IsDirty || model.Container != null)
            {
                existingModels.TryAdd(model.Name, model);
            }
        }

        Models.Clear();

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, Path);

        if (binderType is ResourceContainerType.None)
        {
            Models.Add(GetModelWrapper(existingModels, Name));
            return;
        }

        try
        {
            using var binder = ReadContainer();

            if (binder == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Path));
                return;
            }

            foreach (var file in binder.Files)
            {
                var filename = FilePathUtils.GetPureFilename(file.Name);
                var filepath = file.Name.ToLower();

                if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                {
                    Models.Add(GetModelWrapper(existingModels, filename));
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Path), e);
        }
    }

    private ModelWrapper GetModelWrapper(Dictionary<string, ModelWrapper> existingModels, string name)
    {
        if (existingModels.TryGetValue(name, out var existingModel))
            return existingModel;

        return new ModelWrapper(this, name);
    }

    /// <summary>
    /// Whether any of the models within this container have been modified
    /// </summary>
    public bool IsDirty()
    {
        return Models.Any(e => e.IsDirty);
    }

    /// <summary>
    /// DES, DS1 and DS1R use the version 3 binder formats
    /// </summary>
    private bool UsesBinder3()
    {
        return Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }

    /// <summary>
    /// BXF containers are split into a header (BHD) and data (BDT) file
    /// </summary>
    private string GetBdtPath()
    {
        return Path.Replace("bhd", "bdt");
    }

    /// <summary>
    /// Read the container as a BND, or as a BXF if it is split into a header and data file.
    /// Returns null if the container could not be found or is not a binder.
    /// </summary>
    public IBinder ReadContainer()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, Path);

        if (binderType is ResourceContainerType.BND)
        {
            if (!TargetFS.FileExists(Path))
                return null;

            var binderData = TargetFS.ReadFile(Path);

            if (binderData == null)
                return null;

            if (UsesBinder3())
            {
                return BND3.Read(binderData.Value);
            }

            return BND4.Read(binderData.Value);
        }

        if (binderType is ResourceContainerType.BXF)
        {
            var bdtPath = GetBdtPath();

            if (!TargetFS.FileExists(Path) || !TargetFS.FileExists(bdtPath))
                return null;

            var bhdData = TargetFS.ReadFile(Path);
            var bdtData = TargetFS.ReadFile(bdtPath);

            if (bhdData == null || bdtData == null || bhdData.Value.Length == 0 || bdtData.Value.Length == 0)
                return null;

            if (UsesBinder3())
            {
                return BXF3.Read(bhdData.Value, bdtData.Value);
            }

            return BXF4.Read(bhdData.Value, bdtData.Value);
        }

        return null;
    }

    /// <summary>
    /// Write the container to the project, writing both the BHD and BDT for a BXF
    /// </summary>
    public void WriteContainer(IBinder binder)
    {
        byte[] bhdBytes;
        byte[] bdtBytes;

        switch (binder)
        {
            case BND3 bnd3:
                Project.VFS.ProjectFS.WriteFile(Path, bnd3.Write());
                break;

            case BND4 bnd4:
                Project.VFS.ProjectFS.WriteFile(Path, bnd4.Write());
                break;

            case BXF3 bxf3:
                bxf3.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(Path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(), bdtBytes);
                break;

            case BXF4 bxf4:
                bxf4.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(Path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(), bdtBytes);
                break;
        }
    }
}

public class ModelWrapper
{
    public ModelContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public ModelContainer Container { get; set; }

    public ModelWrapper(ModelContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    // FLVER
    public FLVER2 FLVER { get; set; }

    // HKXPWV
    public HKXPWV HKXPWV { get; set; }

    // CLM2
    public CLM2 CLM2 { get; set; }

    // EDGE
    public EDGE EDGE { get; set; }

    // GRASS
    public GRASS GRASS { get; set; }

    public bool HasCollision { get; set; } = false;
    public string HKX_Name { get; set; }
    public string HKX_BinderName { get; set; }
    public IRootContainer HKX { get; set; }

    /// <summary>
    /// Whether the model has been modified since it was last loaded or saved
    /// </summary>
    public bool IsDirty { get; set; } = false;

    public void Load()
    {
        // Disable the visualisation of these when the file changes
        CFG.Current.DisplayEdgePoints = false;

        // Keep the unsaved changes instead of replacing them with the stored version
        if (!IsDirty || FLVER == null)
        {
            LoadFromStorage();
        }

        // FLVER
        if (FLVER != null)
        {
            var modelEditor = Parent.Project.Handler.ModelEditor;

            var activeView = modelEditor.ViewHandler.ActiveView;

            if (activeView != null)
            {
                activeView.Universe.LoadModel(this);
            }
        }
    }

    private void LoadFromStorage()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        if (binderType is ResourceContainerType.None)
        {
            LoadDirect();
            return;
        }

        try
        {
            using var binder = Parent.ReadContainer();

            if (binder == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
                return;
            }

            foreach (var file in binder.Files)
            {
                var filename = FilePathUtils.GetPureFilename(file.Name);
                var filepath = file.Name.ToLower();

                ReadFile(filepath, filename, file.Bytes);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
        }

        LoadAssetCollision();
    }

    /// <summary>
    /// Assets keep their collision in separate binders beside the model binder, one per collision type.
    /// Each model within the asset has its own entry in those binders.
    /// </summary>
    private void LoadAssetCollision()
    {
        if (Parent.Project.Descriptor.ProjectType is not (ProjectType.ER
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB))
        {
            return;
        }

        var folder = Parent.Path.Substring(0, Parent.Path.LastIndexOf('/') + 1);

        // Not every asset has both collision types, so fall back to the other one
        var suffixes = CFG.Current.CurrentHavokCollisionType is HavokCollisionType.High
            ? new[] { "h", "l" }
            : new[] { "l", "h" };

        foreach (var suffix in suffixes)
        {
            var binderName = $"{Parent.Name}_{suffix}";
            var binderPath = $"{folder}{binderName}.geomhkxbnd.dcx";

            if (!Parent.TargetFS.FileExists(binderPath))
                continue;

            try
            {
                var binderData = Parent.TargetFS.ReadFile(binderPath);

                if (binderData == null)
                    continue;

                using var binder = BND4.Read(binderData.Value);

                foreach (var file in binder.Files)
                {
                    var filename = FilePathUtils.GetPureFilename(file.Name);

                    if (filename.Equals($"{Name}_{suffix}", StringComparison.OrdinalIgnoreCase))
                    {
                        HKX_BinderName = binderName;
                        HandleHavokCollisionLoad(filename, file.Bytes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", binderPath), e);
            }
        }
    }

    public void LoadDirect()
    {
        var fileData = Parent.TargetFS.ReadFile(Parent.Path);
        if (fileData != null)
        {
            FLVER = FLVER2.Read(fileData.Value);
        }
    }

    public void ReadFile(string filepath, string filename, Memory<byte> data)
    {
        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (filename == Name)
            {
                try
                {
                    FLVER = FLVER2.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_FLVER", filepath), e);
                }
            }
        }

        // HKX
        if(filepath.Contains(".hkx"))
        {
            if (filename == Name)
            {
                HandleHavokCollisionLoad(filename, data);
            }
        }

        // CLM2
        if (filepath.Contains(".clm2"))
        {
            if (filename.Contains(Name))
            {
                try
                {
                    CLM2 = CLM2.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_CLM2", filepath), e);
                }
            }
        }

        // HKXPWV
        if (filepath.Contains(".hkxpwv"))
        {
            if (filename == Name)
            {
                try
                {
                    HKXPWV = HKXPWV.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_HKXPWV", filepath), e);
                }
            }
        }

        // GRASS
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".grass"))
            {
                if (filename == Name)
                {
                    try
                    {
                        GRASS = GRASS.Read(data);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_GRASS", filepath), e);
                    }
                }
            }
        }

        // EDGE
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            // TODO: map pieces have .edge.dcx files in the mapbnds

            if (filepath.Contains(".edge"))
            {
                // Assumption of only one per
                try
                {
                    EDGE = EDGE.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_EDGE", filepath), e);
                }
            }
        }
    }

    /// <summary>
    /// Save the model, if it has been modified or unmodified saves are allowed.
    /// Returns true if the model was written.
    /// </summary>
    public bool Save()
    {
        if (!CFG.Current.ModelEditor_Allow_Unmodified_Save && !IsDirty)
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", Name));
            return false;
        }

        if (FLVER == null)
            return false;

        // Updates the FLVER object with changes from the ModelContainer.
        // Unloaded models already had their changes stored in the FLVER when they were unloaded.
        if (Container != null)
        {
            UpdateFLVER();
        }

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        var isSaved = binderType is ResourceContainerType.None
            ? SaveDirect()
            : SaveContainer();

        if (isSaved)
        {
            IsDirty = false;
        }

        return isSaved;
    }

    public bool SaveDirect()
    {
        var containerPath = Parent.Path;

        try
        {
            var data = FLVER.Write();
            Parent.Project.VFS.ProjectFS.WriteFile(containerPath, data);

            Smithbox.Log(this, LOC.Get("MODEL_Data_Write_FLVER_Log", containerPath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_FLVER", containerPath), e);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Write the model files into the container. Returns false if any of the files failed to write.
    /// </summary>
    private bool SaveContainer()
    {
        IBinder binder;

        try
        {
            binder = Parent.ReadContainer();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            return false;
        }

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
            return false;
        }

        using (binder)
        {
            if (binder.Files.Count == 0)
                return false;

            var isFilesWritten = true;

            foreach (var file in binder.Files)
            {
                var filename = FilePathUtils.GetPureFilename(file.Name);
                var filepath = file.Name.ToLower();

                if (!WriteFile(filepath, filename, file))
                    isFilesWritten = false;
            }

            try
            {
                Parent.WriteContainer(binder);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", Parent.Path), e);
                return false;
            }

            return isFilesWritten;
        }
    }

    /// <summary>
    /// Write the matching model data into the binder file. Returns false if the data failed to write.
    /// Files whose data was never loaded are left as they are.
    /// </summary>
    public bool WriteFile(string filepath, string filename, BinderFile file)
    {
        var isWritten = true;

        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (filename == Name)
            {
                try
                {
                    file.Bytes = FLVER.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_FLVER_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_FLVER", filepath), e);
                    isWritten = false;
                }
            }
        }

        // CLM2
        if (filepath.Contains(".clm2") && CLM2 != null)
        {
            if (filename.Contains(Name))
            {
                try
                {
                    file.Bytes = CLM2.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_CLM2_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_CLM2", filepath), e);
                    isWritten = false;
                }
            }
        }

        // HKXPWV
        if (filepath.Contains(".hkxpwv") && HKXPWV != null)
        {
            if (filename == Name)
            {
                try
                {
                    file.Bytes = HKXPWV.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_HKXPWV_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_HKXPWV", filepath), e);
                    isWritten = false;
                }
            }
        }

        // GRASS
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".grass") && GRASS != null)
            {
                try
                {
                    file.Bytes = GRASS.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_GRASS_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_GRASS", filepath), e);
                    isWritten = false;
                }
            }
        }

        // EDGE
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".edge") && EDGE != null)
            {
                try
                {
                    file.Bytes = EDGE.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_EDGE_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_EDGE", filepath), e);
                    isWritten = false;
                }
            }
        }

        return isWritten;
    }

    public void Unload()
    {
        // Store the unsaved changes in the FLVER, as the container holding them is discarded on unload
        if (IsDirty && Container != null)
        {
            UpdateFLVER();
        }

        var modelEditor = Parent.Project.Handler.ModelEditor;

        var activeView = modelEditor.ViewHandler.ActiveView;

        if (activeView != null)
        {
            activeView.ViewportActionManager.Clear();
            activeView.ActionManager.Clear();

            activeView.EntityTypeCache.InvalidateCache();

            activeView.Universe.UnloadModel(this);
        }
    }

    public void UpdateFLVER()
    {
        // Dummies
        FLVER.Dummies.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Dummy entDummy)
            {
                FLVER.Dummies.Add(entDummy);
            }
        }

        // Nodes
        FLVER.Nodes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Node entNode)
            {
                FLVER.Nodes.Add(entNode);
            }
        }

        // Materials
        FLVER.Materials.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.Material entMat)
            {
                FLVER.Materials.Add(entMat);
            }
        }

        // Meshes
        FLVER.Meshes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.Mesh entMesh)
            {
                FLVER.Meshes.Add(entMesh);
            }
        }

        // SkeletonSet
        FLVER.Skeletons = null;
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.SkeletonSet entSkeletonSet)
            {
                FLVER.Skeletons = entSkeletonSet;
            }
        }
    }

    private void HandleHavokCollisionLoad(string filename, Memory<byte> data)
    {
        HasCollision = true;
        HKX_Name = filename;

        if (Parent.Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer = new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer();

            using (MemoryStream memoryStream = new MemoryStream(data.ToArray()))
            {
                try
                {
                    HKX = (HKLib.hk2018.hkRootLevelContainer)serializer.Read(memoryStream);
                }
                catch (InvalidDataException)
                {
                }
            }
        }
        else if (Parent.Project.Descriptor.ProjectType is ProjectType.DS3)
        {
            HKX2.PackFileDeserializer deserializer = new HKX2.PackFileDeserializer();

            using (MemoryStream memoryStream = new MemoryStream(data.ToArray()))
            {
                try
                {
                    var br = new BinaryReaderEx(false, memoryStream.ToArray());
                    HKX = (HKX2.hkRootLevelContainer)deserializer.Deserialize(br);
                }
                catch (InvalidDataException)
                {
                }
            }
        }
    }
}