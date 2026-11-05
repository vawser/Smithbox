using Andre.IO.VFS;
using Havok.Shared;
using SoulsFormats;

namespace StudioCore.Editors.HavokEditor;

public class HavokBank : IDisposable
{
    public ProjectEntry Project;

    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> AnimationBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> BehaviorBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> CharacterBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> CutsceneBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> NavmeshBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> PartBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> RumbleBank = new();

    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> MapCollisionBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> AssetCollisionBank = new();

    /// <summary>
    /// The files that must not be unloaded or replaced: those in the undo/redo history (otherwise an undo would
    /// change a discarded copy instead of the displayed file) and those selected in a view (so its panels stay valid).
    /// </summary>
    public Func<HashSet<HavokFileWrapper>> GetPinnedWrappers;

    /// <summary>
    /// The binders whose file actions are in an undo/redo history. They keep their working binder and wrappers,
    /// so undoing a file action restores the binder the history refers to.
    /// </summary>
    public Func<HashSet<FileDictionaryEntry>> GetPinnedBinders;

    /// <summary>
    /// Runs a change to a binder's file list while keeping every view's selection on the same files
    /// (set by the editor screen, which knows the views)
    /// </summary>
    public Action<FileDictionaryEntry, Action> PreserveSelections;

    /// <summary>
    /// The in-memory copy of each binder that file actions have touched. File actions change it instead of
    /// the file on disk, and loading, copying and saving read from it, so staged changes behave like saved ones.
    /// </summary>
    private readonly Dictionary<FileDictionaryEntry, IBinder> WorkingBinders = new();

    /// <summary>
    /// The binders whose file list or stored bytes differ from disk (file actions not yet saved)
    /// </summary>
    private readonly HashSet<FileDictionaryEntry> StagedBinders = new();

    public HavokBank(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        return true;
    }

    #region Banks
    /// <summary>
    /// Get the bank that holds the files for the passed category
    /// </summary>
    public Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> GetBank(HavokCategoryMode category)
    {
        return category switch
        {
            HavokCategoryMode.Animation => AnimationBank,
            HavokCategoryMode.Behavior => BehaviorBank,
            HavokCategoryMode.Character => CharacterBank,
            HavokCategoryMode.Map_Collision => MapCollisionBank,
            HavokCategoryMode.Asset_Collision => AssetCollisionBank,
            HavokCategoryMode.Navmesh => NavmeshBank,
            HavokCategoryMode.Cutscene => CutsceneBank,
            HavokCategoryMode.Part_Collidable => PartBank,
            HavokCategoryMode.Rumble => RumbleBank,
            _ => null
        };
    }

    /// <summary>
    /// Whether the files for the passed category are stored in a BXF (BHD + BDT) container
    /// </summary>
    public bool IsCombinedCategory(HavokCategoryMode category)
    {
        return category is HavokCategoryMode.Map_Collision;
    }

    /// <summary>
    /// Get the wrapper for the passed file, or null if it is not in the bank
    /// </summary>
    public HavokFileWrapper GetFileWrapper(HavokCategoryMode category, FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var bankDict = GetBank(category);

        if (bankDict == null || fileEntry == null || internalFilePath == null)
            return null;

        if (!bankDict.TryGetValue(fileEntry, out var files))
            return null;

        if (!files.TryGetValue(internalFilePath, out var fileWrapper))
            return null;

        return fileWrapper;
    }

    /// <summary>
    /// Whether any of the files within the passed binder have been modified, or the binder has staged file actions
    /// </summary>
    public bool IsBinderDirty(Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict, FileDictionaryEntry fileEntry)
    {
        if (fileEntry == null || !bankDict.TryGetValue(fileEntry, out var files))
            return false;

        return IsBinderDirty(fileEntry, files);
    }

    private bool IsBinderDirty(FileDictionaryEntry fileEntry, Dictionary<string, HavokFileWrapper> files)
    {
        return StagedBinders.Contains(fileEntry) || files.Values.Any(e => e.IsDirty);
    }

    /// <summary>
    /// Find where a wrapper is in the bank: its category, binder and file name. False if it isn't in the bank.
    /// </summary>
    public bool FindWrapper(HavokFileWrapper wrapper, out HavokCategoryMode category, out FileDictionaryEntry binderEntry, out string filePath)
    {
        foreach (var curCategory in Enum.GetValues<HavokCategoryMode>())
        {
            var bankDict = GetBank(curCategory);

            if (bankDict == null)
                continue;

            foreach (var (entry, files) in bankDict)
            {
                if (files.TryGetValue(wrapper.Key, out var current) && current == wrapper)
                {
                    category = curCategory;
                    binderEntry = entry;
                    filePath = wrapper.Key;
                    return true;
                }
            }
        }

        category = HavokCategoryMode.None;
        binderEntry = null;
        filePath = null;
        return false;
    }

    /// <summary>
    /// Find the category of a binder in the bank. False if it isn't in the bank.
    /// </summary>
    public bool FindBinder(FileDictionaryEntry binderEntry, out HavokCategoryMode category)
    {
        foreach (var curCategory in Enum.GetValues<HavokCategoryMode>())
        {
            var bankDict = GetBank(curCategory);

            if (bankDict != null && bankDict.ContainsKey(binderEntry))
            {
                category = curCategory;
                return true;
            }
        }

        category = HavokCategoryMode.None;
        return false;
    }

    /// <summary>
    /// Whether the binder has file actions (paste, delete, rename...) that haven't been saved
    /// </summary>
    public bool HasStagedChanges(FileDictionaryEntry fileEntry)
    {
        return fileEntry != null && StagedBinders.Contains(fileEntry);
    }

    /// <summary>
    /// The binder and internal paths of the files with unsaved changes, for the unsaved changes prompt. Only the
    /// loaded files, as those are the ones SaveBinder writes.
    /// </summary>
    public List<string> GetDirtyFiles()
    {
        var result = new List<string>();

        foreach (var category in Enum.GetValues<HavokCategoryMode>())
        {
            var bankDict = GetBank(category);

            if (bankDict == null)
                continue;

            foreach (var (fileEntry, files) in bankDict)
            {
                // Staged file actions are saved with the binder too
                if (StagedBinders.Contains(fileEntry))
                    result.Add($"{fileEntry.Path} : {LOC.Get("HAVOK_Data_Unsaved_File_Actions")}");

                foreach (var (path, wrapper) in files)
                {
                    if (wrapper.IsDirty && wrapper.IsLoaded())
                        result.Add($"{fileEntry.Path} : {path}");
                }
            }
        }

        return result.OrderBy(e => e).ToList();
    }
    #endregion

    #region Populate
    public void PopulateAnimationBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(AnimationBank, entry, false, clearCache);
    }

    public void PopulateBehaviorBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(BehaviorBank, entry, false, clearCache);
    }

    public void PopulateCharacterBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(CharacterBank, entry, false, clearCache);
    }

    public void PopulateNavmeshBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(NavmeshBank, entry, false, clearCache);
    }

    public void PopulateCutsceneBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(CutsceneBank, entry, false, clearCache);
    }

    public void PopulatePartBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(PartBank, entry, false, clearCache);
    }

    public void PopulateRumbleBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(RumbleBank, entry, false, clearCache);
    }

    public void PopulateMapCollisionBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(MapCollisionBank, entry, true, clearCache);
    }

    public void PopulateAssetCollisionBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(AssetCollisionBank, entry, false, clearCache);
    }
    #endregion

    #region Load
    public void LoadAnimationFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(AnimationBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Animation_HKX",
            "HAVOK_Data_Failed_to_Read_Animation_Binder_File", true);
    }

    public void LoadBehaviorFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(BehaviorBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Behavior_HKX",
            "HAVOK_Data_Failed_to_Read_Behavior_Binder_File");
    }

    public void LoadCharacterFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(CharacterBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Character_HKX",
            "HAVOK_Data_Failed_to_Read_Character_Binder_File");
    }

    public void LoadCutsceneFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(CutsceneBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Cutscene_HKX",
            "HAVOK_Data_Failed_to_Read_Cutscene_Binder_File");
    }

    public void LoadNavmeshFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(NavmeshBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Navmesh_HKX",
            "HAVOK_Data_Failed_to_Read_Navmesh_Binder_File");
    }

    public void LoadPartFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(PartBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Part_HKX",
            "HAVOK_Data_Failed_to_Read_Part_Binder_File");
    }

    public void LoadRumbleFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(RumbleBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Rumble_HKX",
            "HAVOK_Data_Failed_to_Read_Rumble_Binder_File");
    }

    public void LoadMapCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(MapCollisionBank, fileEntry, internalFilePath, true,
            "HAVOK_Data_Failed_to_Read_Collision_HKX",
            "HAVOK_Data_Failed_to_Read_Collision_Binder_File", true);
    }

    public void LoadAssetCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(AssetCollisionBank, fileEntry, internalFilePath, false,
            "HAVOK_Data_Failed_to_Read_Collision_HKX",
            "HAVOK_Data_Failed_to_Read_Collision_Binder_File");
    }
    #endregion

    #region Save
    /// <summary>
    /// Save the modified files within the passed binder.
    /// The passed internal file is also saved if unmodified saves are allowed.
    /// </summary>
    public bool SaveHavokFile(HavokCategoryMode category, FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var bankDict = GetBank(category);

        if (bankDict == null || fileEntry == null)
            return false;

        if (!bankDict.TryGetValue(fileEntry, out var files))
            return false;

        return SaveBinder(fileEntry, files, internalFilePath, IsCombinedCategory(category));
    }

    /// <summary>
    /// Save all binders that contain modified files. Returns whether any binder was written
    /// (failures are logged per binder).
    /// </summary>
    public bool SaveAllHavokFiles()
    {
        var anyWritten = false;

        foreach (var category in Enum.GetValues<HavokCategoryMode>())
        {
            var bankDict = GetBank(category);

            if (bankDict == null)
                continue;

            foreach (var (fileEntry, files) in bankDict)
            {
                // Only save the binders that have been modified (edited files or staged file actions)
                if (!IsBinderDirty(fileEntry, files))
                    continue;

                if (SaveBinder(fileEntry, files, null, IsCombinedCategory(category)))
                    anyWritten = true;
            }
        }

        return anyWritten;
    }
    #endregion

    #region Container Internals
    /// <summary>
    /// Read the file, preferring the project version over the game version
    /// </summary>
    private Memory<byte>? ReadFileData(string path)
    {
        if (Project.VFS.ProjectFS.FileExists(path))
        {
            return Project.VFS.ProjectFS.ReadFile(path);
        }

        return Project.VFS.FS.ReadFile(path);
    }

    /// <summary>
    /// BXF containers are split into a header (BHD) and data (BDT) file
    /// </summary>
    private string GetBdtPath(string bhdPath)
    {
        // Only the extension changes (e.g. .hkxbhd -> .hkxbdt), never a folder name that contains "bhd"
        if (bhdPath.EndsWith("bhd", StringComparison.OrdinalIgnoreCase))
        {
            return $"{bhdPath[..^3]}bdt";
        }

        return bhdPath;
    }

    /// <summary>
    /// Read the container as a BND, or as a BXF if it is combined.
    /// Returns null if the container files could not be found.
    /// </summary>
    private IBinder ReadContainer(FileDictionaryEntry fileEntry, bool isCombined)
    {
        if (isCombined)
        {
            var bhdData = ReadFileData(fileEntry.Path);
            var bdtData = ReadFileData(GetBdtPath(fileEntry.Path));

            if (bhdData == null || bdtData == null)
                return null;

            if (HavokTypeUtils.IsBXF3(Project))
            {
                return BXF3.Read(bhdData.Value, bdtData.Value);
            }

            return BXF4.Read(bhdData.Value, bdtData.Value);
        }

        var binderData = ReadFileData(fileEntry.Path);

        if (binderData == null)
            return null;

        if (HavokTypeUtils.IsBND3(Project))
        {
            return BND3.Read(binderData.Value);
        }

        return BND4.Read(binderData.Value);
    }

    /// <summary>
    /// Write the container to the project, writing both the BHD and BDT for a BXF
    /// </summary>
    private void WriteContainer(FileDictionaryEntry fileEntry, IBinder binder)
    {
        byte[] bhdBytes;
        byte[] bdtBytes;

        switch (binder)
        {
            case BND3 bnd3:
                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bnd3.Write());
                break;

            case BND4 bnd4:
                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bnd4.Write());
                break;

            case BXF3 bxf3:
                bxf3.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(fileEntry.Path), bdtBytes);
                break;

            case BXF4 bxf4:
                bxf4.Write(out bhdBytes, out bdtBytes);

                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bhdBytes);
                Project.VFS.ProjectFS.WriteFile(GetBdtPath(fileEntry.Path), bdtBytes);
                break;
        }
    }

    /// <summary>
    /// The binder to read files from: the working binder when file actions have touched it (so staged files
    /// can be read), otherwise a fresh read from disk. Dispose it only when <paramref name="owned"/> is true.
    /// </summary>
    private IBinder GetBinderForRead(FileDictionaryEntry fileEntry, bool isCombined, out bool owned)
    {
        if (WorkingBinders.TryGetValue(fileEntry, out var workingBinder))
        {
            owned = false;
            return workingBinder;
        }

        owned = true;
        return ReadContainer(fileEntry, isCombined);
    }

    /// <summary>
    /// The working binder for the passed binder, read from disk the first time. Null if it can't be found.
    /// </summary>
    private IBinder GetWorkingBinder(FileDictionaryEntry fileEntry, bool isCombined)
    {
        if (WorkingBinders.TryGetValue(fileEntry, out var workingBinder))
            return workingBinder;

        workingBinder = ReadContainer(fileEntry, isCombined);

        if (workingBinder != null)
        {
            WorkingBinders[fileEntry] = workingBinder;
        }

        return workingBinder;
    }

    /// <summary>
    /// Forget the working binder, so the binder is read from disk again (only for binders without staged changes)
    /// </summary>
    private void DropWorkingBinder(FileDictionaryEntry fileEntry)
    {
        if (WorkingBinders.Remove(fileEntry, out var workingBinder))
        {
            workingBinder.Dispose();
        }
    }

    /// <summary>
    /// Create the undoable action for a file action. The change itself runs when the action is first executed
    /// (by the view's action manager), against the working binder, and is written to disk on Save.
    /// Returns null if the binder can't be read.
    /// </summary>
    private HavokBinderAction CreateBinderAction(HavokFileView.FileAction fileAction, bool isCombined, Func<IBinder, bool> modify)
    {
        IBinder binder;

        try
        {
            binder = GetWorkingBinder(fileAction.BinderEntry, isCombined);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileAction.BinderEntry.Path), ex);
            return null;
        }

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", fileAction.BinderEntry.Path));
            return null;
        }

        return new HavokBinderAction(this, fileAction.BankDict, fileAction.BinderEntry, binder, modify);
    }

    /// <summary>
    /// Run a binder change (or its undo/redo) with every view's selection kept on the same files
    /// </summary>
    internal void RunBinderChange(FileDictionaryEntry fileEntry, Action change)
    {
        if (PreserveSelections != null)
        {
            PreserveSelections(fileEntry, change);
        }
        else
        {
            change();
        }

        StagedBinders.Add(fileEntry);
    }

    /// <summary>
    /// Record the binder's file list and the wrapper of each file
    /// </summary>
    internal HavokBinderSnapshot CaptureBinder(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry, IBinder binder)
    {
        bankDict.TryGetValue(fileEntry, out var files);

        var snapshot = new HavokBinderSnapshot();

        foreach (var file in binder.Files)
        {
            // Every file gets a wrapper, so undo/redo always puts back the same wrapper objects
            HavokFileWrapper wrapper = null;

            if (files == null || !files.TryGetValue(file.Name, out wrapper))
            {
                wrapper = new HavokFileWrapper(file.Name);
            }

            snapshot.Files.Add(new HavokBinderSnapshot.FileState(file, wrapper, file.Name, file.Bytes, file.ID, file.Flags, file.CompressionType));
        }

        return snapshot;
    }

    /// <summary>
    /// Rebuild the binder's wrappers from its files after a change. Files that were already in the binder keep
    /// their wrapper (found by file object, so a renamed file keeps its edits); new files get a new wrapper.
    /// </summary>
    internal void SyncWrappers(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry, IBinder binder, HavokBinderSnapshot before)
    {
        var fileWrappers = new Dictionary<BinderFile, HavokFileWrapper>(ReferenceEqualityComparer.Instance);

        foreach (var state in before.Files)
        {
            fileWrappers[state.File] = state.Wrapper;
        }

        var files = new Dictionary<string, HavokFileWrapper>();

        foreach (var file in binder.Files)
        {
            if (files.ContainsKey(file.Name))
                continue;

            if (!fileWrappers.TryGetValue(file, out var wrapper))
            {
                wrapper = new HavokFileWrapper(file.Name);
            }

            wrapper.Key = file.Name;
            files.Add(file.Name, wrapper);
        }

        bankDict[fileEntry] = files;
    }

    /// <summary>
    /// Put the binder and its wrappers back to a recorded state. Stored bytes are only restored for the files in
    /// <paramref name="restoreBytes"/> (those the action changed): other files keep their current bytes, which
    /// include any saves since. A loaded file that gets bytes restored is marked modified, so Save writes its
    /// current objects with the restored compression.
    /// </summary>
    internal void RestoreBinder(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry, IBinder binder, HavokBinderSnapshot snapshot, HashSet<BinderFile> restoreBytes)
    {
        binder.Files.Clear();

        var files = new Dictionary<string, HavokFileWrapper>();

        foreach (var state in snapshot.Files)
        {
            state.File.Name = state.Name;
            state.File.ID = state.ID;
            state.File.Flags = state.Flags;
            state.File.CompressionType = state.CompressionType;

            if (restoreBytes.Contains(state.File))
            {
                state.File.Bytes = state.Bytes;

                if (state.Wrapper.IsLoaded())
                {
                    state.Wrapper.IsDirty = true;
                }
            }

            binder.Files.Add(state.File);

            // Like PopulateFiles, a duplicate name only gets the first file
            if (!files.ContainsKey(state.Name))
            {
                state.Wrapper.Key = state.Name;
                files.Add(state.Name, state.Wrapper);
            }
        }

        bankDict[fileEntry] = files;
    }

    private bool IsDcxFile(string name)
    {
        return name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The bytes of the binder file as the editor currently has it: re-serialized (and re-compressed the way it
    /// is stored) when it has unsaved changes, otherwise the stored bytes. Used by Paste and Export, so they
    /// produce what the editor shows. The serializer is created on first use, with the binder's compendium if
    /// the files refer to it, like Save.
    /// </summary>
    private Memory<byte> GetCurrentFileBytes(IBinder binder, BinderFile file,
        Dictionary<string, HavokFileWrapper> files, ref HavokTagSerializer serializer)
    {
        if (files == null || !files.TryGetValue(file.Name, out var fileWrapper) ||
            !fileWrapper.IsLoaded() || !fileWrapper.IsDirty)
        {
            return file.Bytes;
        }

        if (serializer == null && HavokTypeUtils.IsHKX3(Project))
        {
            serializer = HavokTagSerializer.Create(Project);

            if (serializer.UsesBinderCompendium)
            {
                LoadCompendium(binder, serializer);
            }
        }

        var fileBytes = WriteRoot(fileWrapper.Root, serializer);

        Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Used_Unsaved_Changes_Log",
            Path.GetFileName(file.Name)));

        return IsDcxFile(file.Name)
            ? DCX.Compress(fileBytes, GetExistingDCXType(file))
            : fileBytes;
    }
    #endregion

    #region Populate Internals
    public void PopulateFiles(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry,
        bool isCombined,
        bool clearCache = false)
    {
        var pinned = GetPinnedWrappers?.Invoke() ?? new HashSet<HavokFileWrapper>();
        var pinnedBinders = GetPinnedBinders?.Invoke() ?? new HashSet<FileDictionaryEntry>();

        // Binders with unsaved changes (edits or staged file actions) are kept so the changes are not lost,
        // and binders with pinned files or file-action history are kept so undo and the views still apply to them
        if (clearCache)
        {
            foreach (var key in bankDict.Keys.ToList())
            {
                var binderFiles = bankDict[key];

                if (!IsBinderDirty(key, binderFiles) && !pinnedBinders.Contains(key) &&
                    !binderFiles.Values.Any(pinned.Contains))
                {
                    bankDict.Remove(key);
                    DropWorkingBinder(key);
                }
            }

            // A refreshed binder with nothing staged and no file-action history is read from disk again
            if (!StagedBinders.Contains(fileEntry) && !pinnedBinders.Contains(fileEntry))
            {
                DropWorkingBinder(fileEntry);
            }
        }

        bankDict.TryGetValue(fileEntry, out var existingFiles);

        try
        {
            // The working copy, if file actions have touched the binder, so staged changes stay listed
            var binder = GetBinderForRead(fileEntry, isCombined, out var owned);

            if (binder == null)
                return;

            using var ownedBinder = owned ? binder : null;

            var files = new Dictionary<string, HavokFileWrapper>();

            foreach (var file in binder.Files)
            {
                if (files.ContainsKey(file.Name))
                    continue;

                HavokFileWrapper fileWrapper = null;

                // Keep the existing state if the file has unsaved changes, is pinned (undo history or a view's selection),
                // or if the binder is not being refreshed
                if (existingFiles != null && existingFiles.TryGetValue(file.Name, out var existingWrapper))
                {
                    if (existingWrapper.IsDirty || !clearCache || pinned.Contains(existingWrapper))
                    {
                        fileWrapper = existingWrapper;
                    }
                }

                files.Add(file.Name, fileWrapper ?? new HavokFileWrapper(file.Name));
            }

            bankDict[fileEntry] = files;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileEntry.Path), ex);
        }
    }
    #endregion

    #region Load Internals
    public void LoadHavokFile(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry,
        string internalFilePath,
        bool isCombined,
        string fileReadFailLocKey,
        string binderReadFailLocKey,
        bool loadCompendium = false)
    {
        if (!bankDict.TryGetValue(fileEntry, out var files))
            return;

        if (!files.TryGetValue(internalFilePath, out var fileWrapper))
            return;

        // Keep the unsaved changes instead of replacing them with the stored version.
        // A loaded file is also kept: replacing its objects would leave the undo history pointing at the old ones.
        // (A binder refresh replaces the wrappers of loaded files that need re-reading.)
        if (fileWrapper.IsDirty || fileWrapper.IsLoaded())
            return;

        if (!HavokTypeUtils.IsHKX2(Project) && !HavokTypeUtils.IsHKX3(Project))
            return;

        var name = Path.GetFileNameWithoutExtension(internalFilePath);

        try
        {
            // The working copy, if file actions have touched the binder, so staged files can be opened
            var binder = GetBinderForRead(fileEntry, isCombined, out var owned);

            if (binder == null)
                return;

            using var ownedBinder = owned ? binder : null;

            HavokTagSerializer serializer = null;

            if (HavokTypeUtils.IsHKX3(Project))
            {
                serializer = HavokTagSerializer.Create(Project);

                if (loadCompendium || serializer.UsesBinderCompendium)
                {
                    LoadCompendium(binder, serializer);
                }
            }

            var file = binder.Files.FirstOrDefault(e => e.Name == internalFilePath);

            if (file == null)
                return;

            var fileBytes = IsDcxFile(file.Name)
                ? DCX.Decompress(file.Bytes).ToArray()
                : file.Bytes.ToArray();

            try
            {
                fileWrapper.Root = ReadRoot(fileBytes, serializer);
            }
            catch (InvalidDataException ex)
            {
                Smithbox.LogError(this, LOC.Get(fileReadFailLocKey, name), ex);
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get(binderReadFailLocKey, name), ex);
        }
    }

    private void LoadCompendium(IBinder binder, HavokTagSerializer serializer)
    {
        byte[] compendiumFileBytes = null;

        foreach (var file in binder.Files)
        {
            if (file.Name.Contains(".compendium.dcx"))
            {
                compendiumFileBytes = DCX.Decompress(file.Bytes).ToArray();
            }
            else if (file.Name.Contains(".compendium"))
            {
                compendiumFileBytes = file.Bytes.ToArray();
            }
        }

        if (compendiumFileBytes != null)
        {
            using MemoryStream memoryStream = new MemoryStream(compendiumFileBytes);
            serializer.LoadCompendium(memoryStream);
        }
    }

    /// <summary>
    /// Deserialize the Havok file, using the HKX3 serializer if the project uses HKX3
    /// </summary>
    private IRootContainer ReadRoot(byte[] fileBytes, HavokTagSerializer serializer)
    {
        if (HavokTypeUtils.IsHKX2(Project))
        {
            var deserializer = new HKX2.PackFileDeserializer();
            var br = new BinaryReaderEx(false, fileBytes);

            return (HKX2.hkRootLevelContainer)deserializer.Deserialize(br);
        }

        using (MemoryStream memoryStream = new MemoryStream(fileBytes))
        {
            return serializer.Read(memoryStream);
        }
    }
    #endregion

    #region Save Internals
    /// <summary>
    /// Write the binder: its modified files, and its staged file actions if it has any. Uses the working binder
    /// when file actions have touched it (and keeps it, so their undo history stays valid), otherwise a fresh
    /// read from disk. Returns whether the container was written.
    /// </summary>
    private bool SaveBinder(FileDictionaryEntry fileEntry, Dictionary<string, HavokFileWrapper> files, string selectedFilePath, bool isCombined)
    {
        var targets = files.Values.Where(e => e.IsLoaded() && e.IsDirty).ToList();
        var hasStagedChanges = StagedBinders.Contains(fileEntry);

        if (CFG.Current.HavokEditor_Allow_Unmodified_Save && selectedFilePath != null)
        {
            if (files.TryGetValue(selectedFilePath, out var selectedWrapper) && selectedWrapper.IsLoaded() && !selectedWrapper.IsDirty)
            {
                targets.Add(selectedWrapper);
            }
        }

        if (targets.Count == 0 && !hasStagedChanges)
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", fileEntry.Filename));
            return false;
        }

        if (!HavokTypeUtils.IsHKX2(Project) && !HavokTypeUtils.IsHKX3(Project))
            return false;

        // Container
        IBinder binder;
        bool owned;

        try
        {
            binder = GetBinderForRead(fileEntry, isCombined, out owned);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileEntry.Path), ex);
            return false;
        }

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", fileEntry.Path));
            return false;
        }

        using var ownedBinder = owned ? binder : null;

        var serializer = HavokTypeUtils.IsHKX3(Project)
            ? HavokTagSerializer.Create(Project)
            : null;

        // Files written with the compendium loaded refer to it, like the files they replace
        if (serializer is { UsesBinderCompendium: true })
        {
            LoadCompendium(binder, serializer);
        }

        // Binder Files
        var writtenFiles = new List<HavokFileWrapper>();

        foreach (var target in targets)
        {
            if (HandleBinderFile(binder, target, serializer))
            {
                writtenFiles.Add(target);
            }
        }

        // Staged file actions are written even when no file failed or was edited
        if (writtenFiles.Count == 0 && !hasStagedChanges)
            return false;

        // Save Binder
        try
        {
            WriteContainer(fileEntry, binder);

            foreach (var fileWrapper in writtenFiles)
            {
                fileWrapper.IsDirty = false;
            }

            StagedBinders.Remove(fileEntry);

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", fileEntry.Filename));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", fileEntry.Path), ex);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Write the Havok file into its binder file. Returns false if the file failed to write.
    /// </summary>
    private bool HandleBinderFile(IBinder binder, HavokFileWrapper fileWrapper, HavokTagSerializer serializer)
    {
        var file = binder.Files.FirstOrDefault(e => e.Name == fileWrapper.Key);

        if (file == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Internal_File", fileWrapper.Key));
            return false;
        }

        // Internal File
        try
        {
            var fileBytes = WriteRoot(fileWrapper.Root, serializer);

            // Keep the compression the file already uses
            file.Bytes = IsDcxFile(file.Name)
                ? DCX.Compress(fileBytes, GetExistingDCXType(file))
                : fileBytes;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", file.Name), ex);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Serialize the Havok file, using the HKX3 serializer if the project uses HKX3
    /// </summary>
    private byte[] WriteRoot(IRootContainer root, HavokTagSerializer serializer)
    {
        if (HavokTypeUtils.IsHKX2(Project))
        {
            var packSerializer = new HKX2.PackFileSerializer();
            var bw = new BinaryWriterEx(false);

            packSerializer.Serialize((HKX2.IHavokObject)root, bw);

            return bw.FinishBytes();
        }

        using (var memoryStream = new MemoryStream())
        {
            serializer.Write(root, memoryStream);

            return memoryStream.ToArray();
        }
    }
    #endregion

    #region File Manipulation
    // File actions return an undoable action (or null if there is nothing to do), which the view executes.
    // The change is made on the binder's working copy and written to disk on Save. Export is immediate.
    public HavokBinderAction PasteHavokFile(HavokFileView.FileAction fileAction)
    {
        return PasteFiles(fileAction, false);
    }

    public HavokBinderAction PasteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return PasteFiles(fileAction, true);
    }

    public HavokBinderAction DeleteHavokFile(HavokFileView.FileAction fileAction)
    {
        return DeleteFiles(fileAction, false);
    }

    public HavokBinderAction DeleteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return DeleteFiles(fileAction, true);
    }

    public HavokBinderAction RenameHavokFile(HavokFileView.FileAction fileAction)
    {
        return RenameFile(fileAction, false);
    }

    public HavokBinderAction RenameCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return RenameFile(fileAction, true);
    }

    public HavokBinderAction InsertHavokFile(HavokFileView.FileAction fileAction)
    {
        return InsertFiles(fileAction, false);
    }

    public HavokBinderAction InsertCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return InsertFiles(fileAction, true);
    }

    public void ExportHavokFile(HavokFileView.FileAction fileAction)
    {
        ExportFiles(fileAction, false);
    }

    public void ExportCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        ExportFiles(fileAction, true);
    }

    private HavokBinderAction PasteFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.ClipboardBinder == null)
            return null;

        if (fileAction.ClipboardFiles.Count == 0)
            return null;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return null;

        // The copied files are read now, as the editor has them: from the source's working copy if it has one
        // (so staged files can be copied), and re-serialized if they have unsaved changes
        var copies = new List<(BinderFile Source, Memory<byte> Bytes)>();

        try
        {
            var sourceBinder = GetBinderForRead(fileAction.ClipboardBinder, isCombined, out var owned);

            if (sourceBinder == null)
                return null;

            try
            {
                fileAction.BankDict.TryGetValue(fileAction.ClipboardBinder, out var sourceFiles);
                HavokTagSerializer serializer = null;

                foreach (var file in sourceBinder.Files)
                {
                    if (!fileAction.ClipboardFiles.Contains(file.Name))
                        continue;

                    copies.Add((file, GetCurrentFileBytes(sourceBinder, file, sourceFiles, ref serializer)));
                }
            }
            finally
            {
                if (owned)
                    sourceBinder.Dispose();
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileAction.ClipboardBinder.Path), ex);
            return null;
        }

        if (copies.Count == 0)
            return null;

        return CreateBinderAction(fileAction, isCombined, targetBinder =>
        {
            // Get names for unique naming of pasted file
            var currentNames = targetBinder.Files.Select(e => e.Name).ToList();

            var newIdBase = HavokBinderUtils.GetNextFileId(targetBinder.Files);

            // Add in new BinderFiles to target (current binder selection)
            foreach (var (file, bytes) in copies)
            {
                var newName = HavokBinderUtils.GetUniqueFileName(file.Name, currentNames, "hkx");

                var newBinderFile = new BinderFile
                {
                    Flags = file.Flags,
                    ID = newIdBase,
                    Name = newName,
                    Bytes = bytes,
                    CompressionType = file.CompressionType
                };

                currentNames.Add(newName);
                targetBinder.Files.Add(newBinderFile);

                newIdBase = newIdBase + 1;

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Paste_Log", newName, fileAction.BinderEntry.Filename));
            }

            return true;
        });
    }

    private HavokBinderAction DeleteFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return null;

        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return null;

        if (fileAction.MultipleFilePaths.Any(e => !files.ContainsKey(e)))
            return null;

        // Copied, as the view replaces its selection list
        var targets = fileAction.MultipleFilePaths.ToList();

        return CreateBinderAction(fileAction, isCombined, binder =>
        {
            var anyRemoved = false;

            foreach (var entry in targets)
            {
                var file = binder.Files.FirstOrDefault(e => e.Name == entry);

                if (file == null)
                    continue;

                binder.Files.Remove(file);
                anyRemoved = true;

                var logName = Path.GetFileNameWithoutExtension(file.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Delete_Log", logName, fileAction.BinderEntry.Filename));
            }

            return anyRemoved;
        });
    }

    private HavokBinderAction RenameFile(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return null;

        if (!files.ContainsKey(fileAction.FilePath))
            return null;

        // The new name must be a plain file name that no other file in the binder uses
        var newName = fileAction.NewFilename?.Trim() ?? "";

        if (newName == "" ||
            newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 ||
            newName.Contains('/') || newName.Contains('\\'))
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Invalid_Name", fileAction.NewFilename ?? ""));
            return null;
        }

        var newPath = HavokBinderUtils.ReplaceFileName(fileAction.FilePath, newName);

        if (string.Equals(newPath, fileAction.FilePath, StringComparison.Ordinal))
            return null;

        if (files.Keys.Any(e => e != fileAction.FilePath && string.Equals(e, newPath, StringComparison.OrdinalIgnoreCase)))
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Name_Taken", newName, fileAction.BinderEntry.Filename));
            return null;
        }

        var oldPath = fileAction.FilePath;

        return CreateBinderAction(fileAction, isCombined, binder =>
        {
            var anyRenamed = false;

            foreach (var file in binder.Files)
            {
                if (file.Name != oldPath)
                    continue;

                var name = Path.GetFileNameWithoutExtension(file.Name);

                file.Name = HavokBinderUtils.ReplaceFileName(file.Name, newName);

                var logName = Path.GetFileNameWithoutExtension(file.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Log", name, logName));

                anyRenamed = true;
            }

            return anyRenamed;
        });
    }

    private HavokBinderAction InsertFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.Inserts.Count == 0)
            return null;

        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return null;

        // The new files copy the folder, flags and compression of an existing file
        if (files.Count == 0)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_FileView_ContextAction_Insert_Empty_Binder", fileAction.BinderEntry.Filename));
            return null;
        }

        var inserts = fileAction.Inserts.ToList();

        return CreateBinderAction(fileAction, isCombined, binder =>
        {
            if (binder.Files.Count == 0)
                return false;

            var currentNames = binder.Files.Select(e => e.Name).ToList();

            foreach (var entry in inserts)
            {
                var lastFile = binder.Files.Last();

                var filename = Path.GetFileName(entry.FilePath);
                var insertFile = HavokBinderUtils.GetInsertFile(fileAction, lastFile, filename, entry.FileData);

                // Same rules as paste: a unique name and an unused ID
                insertFile.Name = HavokBinderUtils.GetUniqueFileName(insertFile.Name, currentNames, "hkx");
                insertFile.ID = HavokBinderUtils.GetNextFileId(binder.Files);

                currentNames.Add(insertFile.Name);
                binder.Files.Add(insertFile);

                var logName = Path.GetFileNameWithoutExtension(insertFile.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Insert_Log", logName, fileAction.BinderEntry.Path));
            }

            return true;
        });
    }

    private void ExportFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return;

        if (fileAction.MultipleFilePaths.Any(e => !files.ContainsKey(e)))
            return;

        try
        {
            // The working copy if there is one, so staged files can be exported
            var binder = GetBinderForRead(fileAction.BinderEntry, isCombined, out var owned);

            if (binder == null)
                return;

            try
            {
                // Files with unsaved changes are exported as the editor has them
                HavokTagSerializer serializer = null;

                foreach (var entry in fileAction.MultipleFilePaths)
                {
                    foreach (var file in binder.Files)
                    {
                        if (file.Name != entry)
                            continue;

                        // The file keeps its own name and extensions
                        var exportFolder = fileAction.ExportFolder ?? Project.Descriptor.ProjectPath;
                        var exportPath = Path.Join(exportFolder, Path.GetFileName(file.Name));

                        if (File.Exists(exportPath))
                        {
                            Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Export_Replaced_Log", exportPath));
                        }

                        File.WriteAllBytes(exportPath, GetCurrentFileBytes(binder, file, files, ref serializer).ToArray());

                        Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Export_Log", exportPath));
                    }
                }
            }
            finally
            {
                if (owned)
                    binder.Dispose();
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }
    #endregion

    #region Compress
    /// <summary>
    /// Used when the binder has no existing .dcx entries to copy the compression type from.
    /// Matches what the Save functions use when re-compressing .dcx entries.
    /// </summary>
    private const DCX.Type FallbackDCXType = DCX.Type.DCX_KRAK;

    /// <summary>
    /// Match the compression type of an existing .dcx entry in the binder, so the new
    /// entries are compressed the same way the game's own files are.
    /// </summary>
    private DCX.Type DetectBinderDCXType(IEnumerable<BinderFile> files)
    {
        foreach (var file in files)
        {
            if (!IsDcxFile(file.Name))
                continue;

            try
            {
                DCX.Decompress(file.Bytes.ToArray(), out var type);

                if (type != DCX.Type.Unknown && type != DCX.Type.None && type != DCX.Type.Zlib)
                    return type;
            }
            catch
            {
                // Not a readable DCX, try the next one
            }
        }

        return GetGameDefaultDCXType();
    }

    /// <summary>
    /// The compression the project's game uses for Havok files, for when it can't be read from an existing file
    /// </summary>
    private DCX.Type GetGameDefaultDCXType()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            return DCX.Type.DCX_KRAK_MAX;
        }
        else if(Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            return DCX.Type.DCX_KRAK;
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS1
            or ProjectType.DS1R
            or ProjectType.DS2
            or ProjectType.DS2S)
        {
            return DCX.Type.DCX_DFLT_10000_24_9;
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS3)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            return DCX.Type.DCX_DFLT_11000_44_9;
        }

        return FallbackDCXType;
    }

    /// <summary>
    /// DCX compresses every targeted file that isn't already compressed, and appends '.dcx' to its name.
    /// Returns the number of files changed.
    /// </summary>
    private int CompressBinderFiles(IEnumerable<BinderFile> files, List<string> targets)
    {
        var dcxType = DetectBinderDCXType(files);
        var count = 0;

        foreach (var file in files)
        {
            if (!targets.Contains(file.Name))
                continue;

            var name = Path.GetFileNameWithoutExtension(file.Name);

            var rawBytes = file.Bytes.ToArray();

            if (IsDcxFile(file.Name) || DCX.Is((Memory<byte>)rawBytes))
            {
                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Compress_Skipped_Log", name));
                continue;
            }

            try
            {
                file.Bytes = DCX.Compress(rawBytes, dcxType);
                file.Name = $"{file.Name}.dcx";

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Compress_Log", name, dcxType.ToString()));
                count++;
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_FileView_ContextAction_Compress_FAIL", name), ex);
            }
        }

        return count;
    }

    public HavokBinderAction CompressHavokFile(HavokFileView.FileAction fileAction)
    {
        return CompressFiles(fileAction, false);
    }

    public HavokBinderAction CompressCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return CompressFiles(fileAction, true);
    }

    private HavokBinderAction CompressFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return null;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return null;

        var targets = fileAction.MultipleFilePaths.ToList();

        return CreateBinderAction(fileAction, isCombined, binder =>
            CompressBinderFiles(binder.Files, targets) > 0);
    }

    /// <summary>
    /// The compression type the entry currently uses, so saving doesn't silently change it.
    /// Falls back to the default if the entry can't be read as DCX.
    /// </summary>
    private DCX.Type GetExistingDCXType(BinderFile file)
    {
        try
        {
            DCX.Decompress(file.Bytes.ToArray(), out var type);

            if (type != DCX.Type.Unknown && type != DCX.Type.None && type != DCX.Type.Zlib)
                return type;
        }
        catch
        {
            // Not a readable DCX, use the game's default
        }

        return GetGameDefaultDCXType();
    }

    /// <summary>
    /// Re-compresses every targeted .dcx file with the new type. File names are unchanged.
    /// Returns the number of files changed.
    /// </summary>
    private int ChangeCompressionBinderFiles(IEnumerable<BinderFile> files, List<string> targets, DCX.Type newType)
    {
        var count = 0;

        foreach (var file in files)
        {
            if (!targets.Contains(file.Name))
                continue;

            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (!IsDcxFile(file.Name))
            {
                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_ChangeCompression_Skipped_Log", name));
                continue;
            }

            try
            {
                var decompressed = DCX.Decompress(file.Bytes.ToArray(), out var oldType);

                if (oldType == newType)
                    continue;

                file.Bytes = DCX.Compress(decompressed, newType);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_ChangeCompression_Log", name, oldType.ToString(), newType.ToString()));
                count++;
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_FileView_ContextAction_ChangeCompression_FAIL", name), ex);
            }
        }

        return count;
    }

    public HavokBinderAction ChangeCompressionHavokFile(HavokFileView.FileAction fileAction)
    {
        return ChangeCompressionFiles(fileAction, false);
    }

    public HavokBinderAction ChangeCompressionCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        return ChangeCompressionFiles(fileAction, true);
    }

    private HavokBinderAction ChangeCompressionFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0 || fileAction.NewCompressionType == DCX.Type.Unknown)
            return null;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return null;

        var targets = fileAction.MultipleFilePaths.ToList();
        var newType = fileAction.NewCompressionType;

        return CreateBinderAction(fileAction, isCombined, binder =>
            ChangeCompressionBinderFiles(binder.Files, targets, newType) > 0);
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        foreach (var workingBinder in WorkingBinders.Values)
        {
            workingBinder.Dispose();
        }

        WorkingBinders.Clear();
        StagedBinders.Clear();
    }
    #endregion
}
