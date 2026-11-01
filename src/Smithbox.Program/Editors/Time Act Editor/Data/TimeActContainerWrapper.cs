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

    public void PopulateTimeActList()
    {
        TimeActs = ReadTimeActs();
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

        var nestedFile = binder.Files.FirstOrDefault(e => e.Name.ToLower().Contains(".anibnd"));

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
        foreach (var file in binder.Files)
        {
            if (file.Name.ToLower().Contains(".tae"))
            {
                entries.Add(new TimeActEntry(FilePathUtils.GetPureFilename(file.Name), file.ID, file.Bytes.ToArray()));
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
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(Path), bdtBytes);
                break;

            case BXF4 bxf4:
                bxf4.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(Path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(Path), bdtBytes);
                break;
        }
    }
}