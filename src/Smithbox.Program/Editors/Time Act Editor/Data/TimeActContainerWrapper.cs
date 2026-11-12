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
    /// A source read from another file system, such as the game's own copy of this one
    /// </summary>
    private TimeActContainerWrapper(ProjectEntry project, string name, string sourcePath, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = name;
        SourcePath = sourcePath;

        Models = new();
        TimeActs = new();
    }

    /// <summary>
    /// The same source as the game has it, read from its own files
    /// </summary>
    public TimeActContainerWrapper CreateVanillaCopy()
    {
        return new TimeActContainerWrapper(Project, Name, SourcePath, Project.VFS.VanillaFS);
    }

    private TimeActVanillaSource _vanilla;

    /// <summary>
    /// The comparison of this source with the game's version of it
    /// </summary>
    public TimeActVanillaSource Vanilla => _vanilla ??= new TimeActVanillaSource(this);

    private TimeActReverseLinks _reverseLinks;

    /// <summary>
    /// What imports each animation and which events are shared, found on demand
    /// </summary>
    public TimeActReverseLinks ReverseLinks => _reverseLinks ??= new TimeActReverseLinks(this);

    /// <summary>
    /// How many animation IDs each category of animations spans
    /// </summary>
    public int AnimationCategorySize =>
        Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

    /// <summary>
    /// Whether any of the time acts have changes that are yet to be saved
    /// </summary>
    public bool HasUnsavedTimeActs => HasStructuralChanges || TimeActs.Any(e => e.IsDirty) || PendingAnimationFiles.Count > 0;

    /// <summary>
    /// Animation HKX files that have been imported, keyed by their full animation ID.
    /// They are written into their binders when the source is saved.
    /// </summary>
    public Dictionary<long, TimeActPendingAnimationFile> PendingAnimationFiles { get; } = new();

    /// <summary>
    /// Characters keep their animations in anibnds next to the chrbnd, while objects and assets nest them in their container
    /// </summary>
    public bool IsCharacter => SourcePath.TrimStart('/').StartsWith("chr", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether time acts have been added or removed since the source was last saved
    /// </summary>
    public bool HasStructuralChanges
    {
        get => _hasStructuralChanges;
        set
        {
            _hasStructuralChanges = value;

            if (value)
            {
                _hadStructuralChanges = true;
            }
        }
    }

    private bool _hasStructuralChanges = false;
    private bool _hadStructuralChanges = false;

    /// <summary>
    /// Whether the time acts have been edited at any point this session, saved or not. The undo history holds
    /// these entries, so they must stay the source's list: read again, an undo would change copies that are no
    /// longer listed, and the list it put them back in would be missing the rest of the binder.
    /// </summary>
    public bool HasEditHistory => _hadStructuralChanges || TimeActs.Any(e => e.HasBeenEdited);

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
        // Keep the unsaved changes instead of replacing them with the stored version, and keep edited
        // time acts the undo history points at
        if (HasUnsavedTimeActs || HasEditHistory)
            return;

        TimeActs = ReadTimeActs();
    }

    /// <summary>
    /// Gets the display name of an animation, which matches the name of its HKX file
    /// </summary>
    public string GetAnimationName(TimeActEntry entry, TAE.Animation animation)
    {
        return FormatAnimationName(entry.GetFullAnimationID(animation, AnimationCategorySize));
    }

    /// <summary>
    /// Gets the name of the HKX file of a full animation ID, such as a000_003000
    /// </summary>
    public string FormatAnimationName(long id)
    {
        var size = AnimationCategorySize;

        return size == 10000 ? $"a{id / size:D2}_{id % size:D4}" : $"a{id / size:D3}_{id % size:D6}";
    }

    // Binder IDs of later games have this added on top of the animation ID
    private const int FirstAnimationBinderID = 1000000000;

    /// <summary>
    /// Gets the binder ID that the HKX file of a full animation ID is stored under
    /// </summary>
    public int GetAnimationBinderID(long id)
    {
        return UsesBinder3() ? (int)id : FirstAnimationBinderID + (int)id;
    }

    /// <summary>
    /// Whether a binder file is the HKX of an animation, rather than a skeleton or other havok data.
    /// Gives the full animation ID of the file if it is.
    /// </summary>
    public bool TryGetAnimationID(int binderID, string fileName, out long id)
    {
        id = -1;

        if (!fileName.Contains(".hkx", StringComparison.OrdinalIgnoreCase))
            return false;

        if (UsesBinder3())
        {
            if (binderID < 0 || binderID > 999999)
                return false;

            id = binderID;
            return true;
        }

        if (binderID < FirstAnimationBinderID)
            return false;

        id = binderID - FirstAnimationBinderID;
        return true;
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
    /// Whether the source can be written without changes. Its time acts have to be read: the list is the source of
    /// truth on save, so writing an empty one would remove every time act from the binder.
    /// </summary>
    public bool CanSaveUnmodified => TimeActs.Count > 0;

    /// <summary>
    /// Writes the modified time acts into the source container, and the imported animation files into
    /// the containers they belong in. For a character these can be the anibnds split off from the source.
    /// </summary>
    public void SaveTimeActs(bool allowUnmodified = false)
    {
        var writeUnmodified = allowUnmodified && CanSaveUnmodified;

        if (!HasUnsavedTimeActs && !writeUnmodified)
            return;

        var timeActsChanged = HasStructuralChanges || TimeActs.Any(e => e.IsDirty) || writeUnmodified;

        var paths = new List<string>();

        if (timeActsChanged)
        {
            paths.Add(SourcePath);
        }

        foreach (var path in PendingAnimationFiles.Values.SelectMany(e => e.BinderPaths))
        {
            if (!paths.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                paths.Add(path);
            }
        }

        var failed = false;

        foreach (var path in paths)
        {
            var updateTimeActs = timeActsChanged && path.Equals(SourcePath, StringComparison.OrdinalIgnoreCase);

            var animationFiles = PendingAnimationFiles.Values
                .Where(e => e.BinderPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!SaveBinder(path, updateTimeActs, animationFiles))
            {
                failed = true;
            }
        }

        // Writing a file again only replaces it, so whatever failed is kept for the next save
        if (!failed)
        {
            PendingAnimationFiles.Clear();
        }
    }

    private bool SaveBinder(string path, bool updateTimeActs, List<TimeActPendingAnimationFile> animationFiles)
    {
        using var binder = ReadBinderFile(path);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", path));
            return false;
        }

        var nestedFile = FindNestedAnibnd(binder);

        var written = new List<(TimeActEntry Entry, byte[] Bytes)>();

        if (nestedFile == null)
        {
            UpdateFiles(binder);
        }
        else
        {
            using var anibnd = ReadBinder(nestedFile.Bytes);

            UpdateFiles(anibnd);

            nestedFile.Bytes = anibnd is BND3 bnd3 ? bnd3.Write() : ((BND4)anibnd).Write();
        }

        WriteBinderFile(binder, path);

        foreach (var (entry, bytes) in written)
        {
            entry.MarkSaved(bytes);
        }

        if (updateTimeActs)
        {
            HasStructuralChanges = false;
        }

        return true;

        void UpdateFiles(IBinder target)
        {
            if (updateTimeActs)
            {
                written = UpdateTimeActFiles(target);
            }

            foreach (var file in animationFiles)
            {
                UpdateAnimationFile(target, file);
            }
        }
    }

    /// <summary>
    /// Replaces the HKX file of an animation, or adds it alongside the animation files already in the binder,
    /// sharing the folder, flags and compression of the one closest to it
    /// </summary>
    private void UpdateAnimationFile(IBinder binder, TimeActPendingAnimationFile animationFile)
    {
        var binderID = GetAnimationBinderID(animationFile.ID);

        var file = binder.Files.FirstOrDefault(e => e.ID == binderID);

        if (file != null)
        {
            file.Bytes = animationFile.Bytes;
            return;
        }

        var sibling = binder.Files
            .Where(e => TryGetAnimationID(e.ID, e.Name, out _))
            .OrderBy(e => Math.Abs((long)e.ID - binderID))
            .FirstOrDefault()
            ?? binder.Files.FirstOrDefault(e => e.Name.Contains(".hkx", StringComparison.OrdinalIgnoreCase));

        var name = $"{FormatAnimationName(animationFile.ID)}.hkx";
        var flags = Binder.FileFlags.Flag1;
        var compression = DCX.Type.Zlib;

        if (sibling != null)
        {
            var siblingName = System.IO.Path.GetFileName(sibling.Name);

            name = sibling.Name.Substring(0, sibling.Name.Length - siblingName.Length) + name;
            flags = sibling.Flags;
            compression = sibling.CompressionType;
        }
        else if (binder.Files.FirstOrDefault() is { } first)
        {
            flags = first.Flags;
            compression = first.CompressionType;
        }

        file = new BinderFile(flags, binderID, name, animationFile.Bytes)
        {
            CompressionType = compression
        };

        var index = binder.Files.FindIndex(e => e.ID > binderID);

        binder.Files.Insert(index < 0 ? binder.Files.Count : index, file);
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
    /// BXF containers are split into a header (BHD) and data (BDT) file. Only the extension is swapped,
    /// so a folder or name that happens to contain "bhd" is left alone.
    /// </summary>
    private static string GetBdtPath(string path)
    {
        var nameStart = path.LastIndexOf('/') + 1;
        var index = path.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);

        if (index < nameStart)
            return path;

        return path.Substring(0, index) + "bdt" + path.Substring(index + 3);
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