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

            // Collisions are found through their map, as map pieces are
            newEntry.DeriveMapID();

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
            // '/map/m30/m30_00_00_00/...' splits into "", "map", "m30", "m30_00_00_00", ...
            if (parts.Length > 3)
            {
                MapID = parts[3];
            }
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS2
            or ProjectType.DS2S)
        {
            // DS2 keeps the map pieces of a map in one container named after the map
            if (parts.Length > 3)
            {
                MapID = FilePathUtils.GetPureFilename(parts[3]);
            }
        }
        else
        {
            // '/map/m30_00_00_00/...' splits into "", "map", "m30_00_00_00", ...
            if (parts.Length > 2)
            {
                MapID = parts[2];
            }
        }
    }

    public void PopulateModelList()
    {
        // Keep the wrappers that are loaded or have unsaved changes, so their state is not lost
        var existingModels = new Dictionary<string, ModelWrapper>();

        foreach (var model in Models)
        {
            // Parked models keep their objects and history too
            if (model.IsDirty || model.Container != null || model.Parked != null)
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

    /// <summary>
    /// Set while the model is switched away from in a view but keeps its objects and undo history there.
    /// <see cref="Container"/> is null meanwhile, as the model isn't loaded. Managed by <see cref="ModelEditorView"/>.
    /// </summary>
    public ParkedModel Parked { get; set; }

    /// <summary>
    /// The position of each mesh in the FLVER as stored on disk, which is the submesh the viewport draws it from.
    /// The viewport reads the stored file, so once meshes are deleted, copied or moved and the model is loaded again
    /// before saving, a mesh's position in the edited list no longer names its submesh. Copies use the submesh of
    /// the mesh they were copied from.
    /// </summary>
    public Dictionary<FLVER2.Mesh, int> MeshSourceIndices { get; } = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Records the meshes of the FLVER as matching the stored file, after it has been read or written
    /// </summary>
    private void ResetMeshSourceIndices()
    {
        MeshSourceIndices.Clear();

        if (FLVER == null)
            return;

        for (var i = 0; i < FLVER.Meshes.Count; i++)
        {
            MeshSourceIndices[FLVER.Meshes[i]] = i;
        }
    }

    // The file each of the other files was read from, so a save writes them back to that file only
    private string CLM2_FileName;
    private string HKXPWV_FileName;
    private string GRASS_FileName;
    private string EDGE_FileName;

    /// <summary>
    /// Load the model into a view. Use <see cref="ModelEditorView.LoadModel"/>, which also handles the selection and
    /// the model the view had loaded before.
    /// </summary>
    public void Load(ModelEditorView view)
    {
        // The objects of a loaded model wrap the FLVER, so it must not be read again underneath them
        if (Container != null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Universe_Model_Already_Loaded", Name));
            return;
        }

        // Disable the visualisation of these when the file changes
        CFG.Current.DisplayEdgePoints = false;

        // Keep the unsaved changes instead of replacing them with the stored version
        if (!IsDirty || FLVER == null)
        {
            LoadFromStorage();

            ResetMeshSourceIndices();
        }

        // FLVER
        if (FLVER != null)
        {
            view.Universe.LoadModel(this);
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

            // DES binders can name their model differently (am_f_0000_l holds AM_F_0000.flver, hd_f_9000 HD_F_0000.flver)
            FLVER ??= Flver0Converter.ReadOnlyFlver(Parent.Project, binder);
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
            FLVER = Flver0Converter.ReadEditorFlver(Parent.Project, fileData.Value);
        }
    }

    public void ReadFile(string filepath, string filename, Memory<byte> data)
    {
        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (string.Equals(filename, Name, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    FLVER = Flver0Converter.ReadEditorFlver(Parent.Project, data);
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
                    CLM2_FileName = filepath;
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
                    HKXPWV_FileName = filepath;
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
                        GRASS_FileName = filepath;
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
                    EDGE_FileName = filepath;
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

            // The stored file now matches the edited meshes. A drawn model keeps the drawables it has until it is
            // loaded again, which is when this mapping is next read.
            ResetMeshSourceIndices();
        }

        return isSaved;
    }

    public bool SaveDirect()
    {
        var containerPath = Parent.Path;

        try
        {
            var data = Flver0Converter.WriteEditorFlver(Parent.Project, FLVER);
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

            // The model the load fell back to when no file is named after the model (DES binders), as LoadFromStorage does
            var hasNamedModel = binder.Files.Any(f =>
                (f.Name.ToLower().Contains(".flver") || f.Name.ToLower().Contains(".flv")) &&
                string.Equals(FilePathUtils.GetPureFilename(f.Name), Name, StringComparison.OrdinalIgnoreCase));
            var fallbackModel = hasNamedModel ? null : Flver0Converter.GetOnlyFlverFile(binder);

            foreach (var file in binder.Files)
            {
                var filename = FilePathUtils.GetPureFilename(file.Name);
                var filepath = file.Name.ToLower();

                if (!WriteFile(filepath, filename, file, file == fallbackModel))
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
    public bool WriteFile(string filepath, string filename, BinderFile file, bool isFallbackModel = false)
    {
        var isWritten = true;

        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (isFallbackModel || string.Equals(filename, Name, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    file.Bytes = Flver0Converter.WriteEditorFlver(Parent.Project, FLVER);

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_FLVER_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_FLVER", filepath), e);
                    isWritten = false;
                }
            }
        }

        // The other files go back to the file they were read from: a binder can hold the files of several models,
        // and matching by name alone could write this model's data over another model's

        // CLM2
        if (filepath.Contains(".clm2") && CLM2 != null)
        {
            if (filepath == CLM2_FileName)
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
            if (filepath == HKXPWV_FileName)
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
            if (filepath.Contains(".grass") && GRASS != null && filepath == GRASS_FileName)
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
            if (filepath.Contains(".edge") && EDGE != null && filepath == EDGE_FileName)
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
        // Unloading a parked model drops its kept objects and history
        Parked?.View.DiscardParked(this);

        // Store the unsaved changes in the FLVER, as the container holding them is discarded on unload
        if (IsDirty && Container != null)
        {
            UpdateFLVER();
        }

        // The model is unloaded from the view it was loaded into, which need not be the active one
        var ownerView = Container?.View;

        if (ownerView != null)
        {
            ownerView.ViewportActionManager.Clear();
            ownerView.ActionManager.Clear();

            ownerView.EntityTypeCache.InvalidateCache();

            ownerView.Universe.UnloadModel(this);
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

        // The games the Model Editor has shown collision for: read through the unified Havok library
        if (Parent.Project.Descriptor.ProjectType is not (ProjectType.ER
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB
            or ProjectType.DS3))
            return;

        var profile = Havok.HavokProfile.FromGame(Parent.Project.Descriptor.ProjectType.ToString());

        if (profile == null)
            return;

        try
        {
            var bytes = DCX.Is(data) ? DCX.Decompress(data).ToArray() : data.ToArray();

            HKX = Havok.Files.HavokFile.Read(bytes, profile).Root as IRootContainer;
        }
        catch (Exception)
        {
            // As before: a collision that doesn't read is left out (e.g. one whose types are in a compendium elsewhere)
        }
    }
}
