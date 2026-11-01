using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContainerWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string MapID { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The container that holds the model. Null if this source has no model.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The anibnd, or the container that the anibnd is nested in for objects and assets
    /// </summary>
    public string SourcePath { get; set; }

    public List<TimeActModelWrapper> Models { get; set; }

    public List<TimeActEntry> TimeActs { get; set; }

    public TimeActContainerWrapper(ProjectEntry project, FileDictionaryEntry sourceEntry, FileDictionaryEntry modelEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = sourceEntry.Filename;
        SourcePath = sourceEntry.Path;
        Path = modelEntry?.Path;

        Models = new();
        TimeActs = new();
    }

    /// <summary>
    /// How many animation IDs each category of animations spans
    /// </summary>
    public int AnimationCategorySize =>
        Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

    /// <summary>
    /// Whether any of the time acts have changes that are yet to be saved
    /// </summary>
    public bool HasUnsavedTimeActs => TimeActs.Any(e => e.IsDirty);

    public void PopulateTimeActList()
    {
        // Keep the unsaved changes instead of replacing them with the stored version
        if (HasUnsavedTimeActs)
            return;

        TimeActs = ReadTimeActs();
    }

    /// <summary>
    /// Gets the display name of an animation, which matches the name of its HKX file
    /// </summary>
    public string GetAnimationName(TimeActEntry entry, TAE.Animation animation)
    {
        var size = AnimationCategorySize;
        var id = entry.GetFullAnimationID(animation, size);

        return size == 10000 ? $"a{id / size:D2}_{id % size:D4}" : $"a{id / size:D3}_{id % size:D6}";
    }

    /// <summary>
    /// Finds the animation with a full animation ID, as used by the animations that import another
    /// </summary>
    public bool TryFindAnimation(long id, out TimeActEntry entry, out TAE.Animation animation)
    {
        var size = AnimationCategorySize;

        foreach (var candidate in TimeActs)
        {
            candidate.Load();

            animation = candidate.TimeAct?.Animations.FirstOrDefault(e => candidate.GetFullAnimationID(e, size) == id);

            if (animation != null)
            {
                entry = candidate;
                return true;
            }
        }

        entry = null;
        animation = null;
        return false;
    }

    /// <summary>
    /// Writes the modified time acts into the source container
    /// </summary>
    public void SaveTimeActs()
    {
        var modified = TimeActs.Where(e => e.IsDirty && e.TimeAct != null).ToList();

        if (modified.Count == 0)
            return;

        using var binder = ReadBinderFile(SourcePath);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", SourcePath));
            return;
        }

        var nestedFile = FindNestedAnibnd(binder);

        if (nestedFile == null)
        {
            UpdateTimeActFiles(binder, modified);
        }
        else
        {
            using var anibnd = ReadBinder(nestedFile.Bytes);

            UpdateTimeActFiles(anibnd, modified);

            nestedFile.Bytes = anibnd is BND3 bnd3 ? bnd3.Write() : ((BND4)anibnd).Write();
        }

        WriteBinderFile(binder, SourcePath);

        foreach (var entry in modified)
        {
            entry.IsDirty = false;
        }
    }

    private void UpdateTimeActFiles(IBinder binder, List<TimeActEntry> modified)
    {
        foreach (var entry in modified)
        {
            var file = binder.Files.First(e => e.ID == entry.BinderID);

            file.Bytes = entry.TimeAct.Write();
        }
    }

    private static BinderFile FindNestedAnibnd(IBinder binder)
    {
        return binder.Files.FirstOrDefault(e => e.Name.ToLower().Contains(".anibnd"));
    }

    /// <summary>
    /// Reads the .tae files of this source, in the order of their binder IDs.
    /// The file data is copied, so the entries can outlive the binder.
    /// </summary>
    public List<TimeActEntry> ReadTimeActs()
    {
        var entries = new List<TimeActEntry>();

        using var binder = ReadBinderFile(SourcePath);

        if (binder == null)
            return entries;

        var nestedFile = FindNestedAnibnd(binder);

        if (nestedFile == null)
        {
            AddTimeActs(binder, entries);
        }
        else
        {
            using var anibnd = ReadBinder(nestedFile.Bytes);

            AddTimeActs(anibnd, entries);
        }

        entries.Sort((a, b) => a.BinderID.CompareTo(b.BinderID));

        return entries;
    }

    private void AddTimeActs(IBinder binder, List<TimeActEntry> entries)
    {
        // Binders with a version marker moved their time acts
        var firstID = binder.Files.Any(e => e.ID == 9999999) && Project.Descriptor.ProjectType is not ProjectType.DES
            ? 5000000 : 3000000;

        foreach (var file in binder.Files)
        {
            // Some binders hold empty placeholders for unused categories
            if (file.Name.ToLower().Contains(".tae") && file.Bytes.Length >= 4)
            {
                entries.Add(new TimeActEntry(FilePathUtils.GetPureFilename(file.Name), file.ID, file.ID % firstID,
                    file.Bytes.ToArray()));
            }
        }
    }

    /// <summary>
    /// Gets the model this source animates, or the first model of the container if none shares its name
    /// </summary>
    public TimeActModelWrapper GetPrimaryModel()
    {
        return Models.FirstOrDefault(e => e.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            ?? Models.FirstOrDefault();
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
            if (parts.Length >= 3)
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

                if (Project.Descriptor.ProjectType is ProjectType.DS2
                    or ProjectType.DS2S)
                {
                    MapID = FilePathUtils.GetPureFilename(parts[3]);
                }
            }
        }
    }

    public void PopulateModelList()
    {
        if (Path == null)
            return;

        // Keep the wrappers that are loaded or have unsaved changes, so their state is not lost
        var existingModels = new Dictionary<string, TimeActModelWrapper>();

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

    private TimeActModelWrapper GetModelWrapper(Dictionary<string, TimeActModelWrapper> existingModels, string name)
    {
        if (existingModels.TryGetValue(name, out var existingModel))
            return existingModel;

        return new TimeActModelWrapper(this, name);
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
    private string GetBdtPath(string path)
    {
        return path.Replace("bhd", "bdt");
    }

    /// <summary>
    /// Read the model container. Returns null if it could not be found or is not a binder.
    /// </summary>
    public IBinder ReadContainer()
    {
        return ReadBinderFile(Path);
    }

    private IBinder ReadBinder(Memory<byte> data)
    {
        if (UsesBinder3())
        {
            return BND3.Read(data);
        }

        return BND4.Read(data);
    }

    /// <summary>
    /// Read the file as a BND, or as a BXF if it is split into a header and data file.
    /// Returns null if the file could not be found or is not a binder.
    /// </summary>
    private IBinder ReadBinderFile(string path)
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, path);

        if (binderType is ResourceContainerType.BND)
        {
            if (!TargetFS.FileExists(path))
                return null;

            var binderData = TargetFS.ReadFile(path);

            if (binderData == null)
                return null;

            return ReadBinder(binderData.Value);
        }

        if (binderType is ResourceContainerType.BXF)
        {
            var bdtPath = GetBdtPath(path);

            if (!TargetFS.FileExists(path) || !TargetFS.FileExists(bdtPath))
                return null;

            var bhdData = TargetFS.ReadFile(path);
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
        WriteBinderFile(binder, Path);
    }

    private void WriteBinderFile(IBinder binder, string path)
    {
        byte[] bhdBytes;
        byte[] bdtBytes;

        switch (binder)
        {
            case BND3 bnd3:
                Project.VFS.ProjectFS.WriteFile(path, bnd3.Write());
                break;

            case BND4 bnd4:
                Project.VFS.ProjectFS.WriteFile(path, bnd4.Write());
                break;

            case BXF3 bxf3:
                bxf3.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(path), bdtBytes);
                break;

            case BXF4 bxf4:
                bxf4.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(path), bdtBytes);
                break;
        }
    }
}