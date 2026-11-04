using Andre.IO.VFS;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
    public bool HasUnsavedTimeActs => HasStructuralChanges || TimeActs.Any(e => e.IsDirty);

    /// <summary>
    /// Whether time acts have been added or removed since the source was last saved
    /// </summary>
    public bool HasStructuralChanges { get; set; } = false;

    /// <summary>
    /// Raised whenever a time act is added or removed, so lists built from them know to rebuild
    /// </summary>
    public int TimeActsVersion { get; private set; } = 0;

    /// <summary>
    /// Gets the first free category after the time act, for a copy of it.
    /// Empty placeholders in the binder aren't listed, so their slot is taken over by the copy.
    /// </summary>
    public int GetFreeBindIndex(TimeActEntry entry)
    {
        var index = entry.BindIndex + 1;

        while (TimeActs.Any(e => e.BindIndex == index))
        {
            index++;
        }

        return index;
    }

    /// <summary>
    /// Adds a time act, keeping the list in the order of the binder IDs. Returns the position it went in.
    /// </summary>
    public int InsertTimeAct(TimeActEntry entry, int index = -1)
    {
        if (index < 0 || index > TimeActs.Count)
        {
            index = TimeActs.FindIndex(e => e.BinderID > entry.BinderID);

            if (index < 0)
                index = TimeActs.Count;
        }

        TimeActs.Insert(index, entry);
        HasStructuralChanges = true;
        TimeActsVersion++;

        return index;
    }

    /// <summary>
    /// Puts a time act back in order after its binder ID has changed
    /// </summary>
    public void ResortTimeAct(TimeActEntry entry)
    {
        TimeActs.Remove(entry);

        InsertTimeAct(entry);
    }

    /// <summary>
    /// The number of digits after the 'a' of a time act name, which matches the names already in the source
    /// </summary>
    public int NameDigits
    {
        get
        {
            return 3;
        }
    }

    private static readonly Regex TimeActNameRegex = new(@"^a(\d+)$");

    public string FormatTimeActName(int bindIndex)
    {
        return $"a{bindIndex.ToString($"D{NameDigits}")}";
    }

    /// <summary>
    /// Gets the category a time act name stands for. The name must be 'a' followed by the digits the source uses.
    /// </summary>
    public bool TryParseTimeActName(string name, out int bindIndex)
    {
        bindIndex = -1;

        var match = TimeActNameRegex.Match(name ?? "");

        if (!match.Success || match.Groups[1].Value.Length != NameDigits)
            return false;

        bindIndex = int.Parse(match.Groups[1].Value);

        return true;
    }

    /// <summary>
    /// Checks a name for a time act, returning the reason it can't be used, or null if it can.
    /// The time act being renamed is allowed to keep its own name.
    /// </summary>
    public string ValidateTimeActName(string name, TimeActEntry renamed = null)
    {
        if (!TryParseTimeActName(name, out var bindIndex))
            return LOC.Get("TAE_Name_Invalid_Mask", $"a{new string('X', NameDigits)}");

        if (TimeActs.Any(e => e != renamed && e.BindIndex == bindIndex))
            return LOC.Get("TAE_Name_Already_Exists", name);

        return null;
    }

    /// <summary>
    /// Swaps one time act for another in the same position
    /// </summary>
    public void ReplaceTimeAct(TimeActEntry oldEntry, TimeActEntry newEntry)
    {
        var index = TimeActs.IndexOf(oldEntry);

        if (index < 0)
            return;

        TimeActs[index] = newEntry;
        HasStructuralChanges = true;
        TimeActsVersion++;
    }

    public void RemoveTimeAct(TimeActEntry entry)
    {
        TimeActs.Remove(entry);
        HasStructuralChanges = true;
        TimeActsVersion++;
    }

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
        if (!HasUnsavedTimeActs)
            return;

        using var binder = ReadBinderFile(SourcePath);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", SourcePath));
            return;
        }

        var nestedFile = FindNestedAnibnd(binder);

        List<(TimeActEntry Entry, byte[] Bytes)> written;

        if (nestedFile == null)
        {
            written = UpdateTimeActFiles(binder);
        }
        else
        {
            using var anibnd = ReadBinder(nestedFile.Bytes);

            written = UpdateTimeActFiles(anibnd);

            nestedFile.Bytes = anibnd is BND3 bnd3 ? bnd3.Write() : ((BND4)anibnd).Write();
        }

        WriteBinderFile(binder, SourcePath);

        foreach (var (entry, bytes) in written)
        {
            entry.MarkSaved(bytes);
        }

        HasStructuralChanges = false;
    }

    /// <summary>
    /// The list of time acts is the source of truth: the files of deleted time acts are removed,
    /// and the files of new ones are added.
    /// </summary>
    private List<(TimeActEntry Entry, byte[] Bytes)> UpdateTimeActFiles(IBinder binder)
    {
        var written = new List<(TimeActEntry Entry, byte[] Bytes)>();

        var listedIDs = TimeActs.Select(e => e.BinderID).ToHashSet();

        // Empty placeholders were never listed, so they are kept
        binder.Files.RemoveAll(e => e.Name.ToLower().Contains(".tae") && e.Bytes.Length >= 4 && !listedIDs.Contains(e.ID));

        foreach (var entry in TimeActs)
        {
            var file = binder.Files.FirstOrDefault(e => e.ID == entry.BinderID);

            // Unchanged time acts are left as they are, unless they are new to the binder
            if (file != null && !entry.IsDirty)
                continue;

            var bytes = entry.GetBytes();

            if (file == null)
            {
                file = new BinderFile(entry.BinderFlags, entry.BinderID, entry.BinderFileName, bytes)
                {
                    CompressionType = entry.CompressionType
                };

                var index = binder.Files.FindIndex(e => e.ID > entry.BinderID);

                binder.Files.Insert(index < 0 ? binder.Files.Count : index, file);
            }
            else
            {
                file.Bytes = bytes;
            }

            written.Add((entry, bytes));
        }

        return written;
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
                    file.Bytes.ToArray(), file.Name, file.Flags, file.CompressionType));
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