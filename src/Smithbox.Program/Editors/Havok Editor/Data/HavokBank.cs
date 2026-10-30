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
    /// Whether any of the files within the passed binder have been modified
    /// </summary>
    public bool IsBinderDirty(Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict, FileDictionaryEntry fileEntry)
    {
        if (fileEntry == null || !bankDict.TryGetValue(fileEntry, out var files))
            return false;

        return IsBinderDirty(files);
    }

    private bool IsBinderDirty(Dictionary<string, HavokFileWrapper> files)
    {
        return files.Values.Any(e => e.IsDirty);
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
    /// Save all binders that contain modified files
    /// </summary>
    public bool SaveAllHavokFiles()
    {
        var success = true;

        foreach (var category in Enum.GetValues<HavokCategoryMode>())
        {
            var bankDict = GetBank(category);

            if (bankDict == null)
                continue;

            foreach (var (fileEntry, files) in bankDict)
            {
                // Only save the binders that have been modified
                if (!IsBinderDirty(files))
                    continue;

                if (!SaveBinder(fileEntry, files, null, IsCombinedCategory(category)))
                    success = false;
            }
        }

        return success;
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
        return bhdPath.Replace("bhd", "bdt");
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
    /// Read the container, apply the modification and write the container back if the modification made a change.
    /// The editing state of any file renamed by the modification is moved to the new name.
    /// </summary>
    private bool ModifyContainer(HavokFileView.FileAction fileAction, bool isCombined, Func<IBinder, bool> modify)
    {
        try
        {
            using var binder = ReadContainer(fileAction.BinderEntry, isCombined);

            if (binder == null)
            {
                Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", fileAction.BinderEntry.Path));
                return false;
            }

            var originalNames = binder.Files.Select(e => (File: e, Name: e.Name)).ToList();

            if (!modify(binder))
                return false;

            WriteContainer(fileAction.BinderEntry, binder);

            if (fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            {
                foreach (var (file, oldName) in originalNames)
                {
                    if (file.Name == oldName)
                        continue;

                    if (files.Remove(oldName, out var fileWrapper))
                    {
                        fileWrapper.Key = file.Name;
                        files[file.Name] = fileWrapper;
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
            return false;
        }
    }

    private bool IsDcxFile(string name)
    {
        return name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Populate Internals
    public void PopulateFiles(
        Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        FileDictionaryEntry fileEntry,
        bool isCombined,
        bool clearCache = false)
    {
        // Binders with unsaved changes are kept so the changes are not lost
        if (clearCache)
        {
            foreach (var key in bankDict.Keys.ToList())
            {
                if (!IsBinderDirty(bankDict[key]))
                {
                    bankDict.Remove(key);
                }
            }
        }

        bankDict.TryGetValue(fileEntry, out var existingFiles);

        try
        {
            using var binder = ReadContainer(fileEntry, isCombined);

            if (binder == null)
                return;

            var files = new Dictionary<string, HavokFileWrapper>();

            foreach (var file in binder.Files)
            {
                if (files.ContainsKey(file.Name))
                    continue;

                HavokFileWrapper fileWrapper = null;

                // Keep the existing state if the file has unsaved changes, or if the binder is not being refreshed
                if (existingFiles != null && existingFiles.TryGetValue(file.Name, out var existingWrapper))
                {
                    if (existingWrapper.IsDirty || !clearCache)
                    {
                        fileWrapper = existingWrapper;
                    }
                }

                files.Add(file.Name, fileWrapper ?? new HavokFileWrapper(file.Name));

                if (!isCombined)
                {
                    Project.Handler.HavokEditor.ViewHandler.ActiveView.Selection.AddToFileAliasCache(file.Name);
                }
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

        // Keep the unsaved changes instead of replacing them with the stored version
        if (fileWrapper.IsDirty)
            return;

        if (!HavokTypeUtils.IsHKX2(Project) && !HavokTypeUtils.IsHKX3(Project))
            return;

        var name = Path.GetFileNameWithoutExtension(internalFilePath);

        try
        {
            using var binder = ReadContainer(fileEntry, isCombined);

            if (binder == null)
                return;

            HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer = null;

            if (HavokTypeUtils.IsHKX3(Project))
            {
                serializer = new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer();

                if (loadCompendium)
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

    private void LoadCompendium(IBinder binder, HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer)
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
    private IRootContainer ReadRoot(byte[] fileBytes, HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer)
    {
        if (HavokTypeUtils.IsHKX2(Project))
        {
            var deserializer = new HKX2.PackFileDeserializer();
            var br = new BinaryReaderEx(false, fileBytes);

            return (HKX2.hkRootLevelContainer)deserializer.Deserialize(br);
        }

        using (MemoryStream memoryStream = new MemoryStream(fileBytes))
        {
            return (HKLib.hk2018.hkRootLevelContainer)serializer.Read(memoryStream);
        }
    }
    #endregion

    #region Save Internals
    private bool SaveBinder(FileDictionaryEntry fileEntry, Dictionary<string, HavokFileWrapper> files, string selectedFilePath, bool isCombined)
    {
        var targets = files.Values.Where(e => e.IsLoaded() && e.IsDirty).ToList();

        if (CFG.Current.HavokEditor_Allow_Unmodified_Save && selectedFilePath != null)
        {
            if (files.TryGetValue(selectedFilePath, out var selectedWrapper) && selectedWrapper.IsLoaded() && !selectedWrapper.IsDirty)
            {
                targets.Add(selectedWrapper);
            }
        }

        if (targets.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", fileEntry.Filename));
            return false;
        }

        if (!HavokTypeUtils.IsHKX2(Project) && !HavokTypeUtils.IsHKX3(Project))
            return false;

        // Container
        IBinder binder;

        try
        {
            binder = ReadContainer(fileEntry, isCombined);
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

        using (binder)
        {
            var serializer = HavokTypeUtils.IsHKX3(Project)
                ? new HKLib.Serialization.hk2018.Binary.HavokBinarySerializer()
                : null;

            // Binder Files
            var writtenFiles = new List<HavokFileWrapper>();

            foreach (var target in targets)
            {
                if (HandleBinderFile(binder, target, serializer))
                {
                    writtenFiles.Add(target);
                }
            }

            if (writtenFiles.Count == 0)
                return false;

            // Save Binder
            try
            {
                WriteContainer(fileEntry, binder);

                foreach (var fileWrapper in writtenFiles)
                {
                    fileWrapper.IsDirty = false;
                }

                Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", fileEntry.Filename));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", fileEntry.Path), ex);
                return false;
            }

            return writtenFiles.Count == targets.Count;
        }
    }

    /// <summary>
    /// Write the Havok file into its binder file. Returns false if the file failed to write.
    /// </summary>
    private bool HandleBinderFile(IBinder binder, HavokFileWrapper fileWrapper, HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer)
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
    private byte[] WriteRoot(IRootContainer root, HKLib.Serialization.hk2018.Binary.HavokBinarySerializer serializer)
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
            serializer.Write((HKLib.hk2018.hkRootLevelContainer)root, memoryStream);

            return memoryStream.ToArray();
        }
    }
    #endregion

    #region File Manipulation
    public void PasteHavokFile(HavokFileView.FileAction fileAction)
    {
        PasteFiles(fileAction, false);
    }

    public void PasteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        PasteFiles(fileAction, true);
    }

    public void DeleteHavokFile(HavokFileView.FileAction fileAction)
    {
        DeleteFiles(fileAction, false);
    }

    public void DeleteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        DeleteFiles(fileAction, true);
    }

    public void RenameHavokFile(HavokFileView.FileAction fileAction)
    {
        RenameFile(fileAction, false);
    }

    public void RenameCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        RenameFile(fileAction, true);
    }

    public void InsertHavokFile(HavokFileView.FileAction fileAction)
    {
        InsertFiles(fileAction, false);
    }

    public void InsertCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        InsertFiles(fileAction, true);
    }

    public void ExportHavokFile(HavokFileView.FileAction fileAction)
    {
        ExportFiles(fileAction, false);
    }

    public void ExportCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        ExportFiles(fileAction, true);
    }

    private void PasteFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.ClipboardBinder == null)
            return;

        if (fileAction.ClipboardFiles.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        try
        {
            using var sourceBinder = ReadContainer(fileAction.ClipboardBinder, isCombined);

            if (sourceBinder == null)
                return;

            // Get BinderFile list from source binder (based on clipboard)
            var filesToCopy = sourceBinder.Files
                .Where(e => fileAction.ClipboardFiles.Contains(e.Name))
                .ToList();

            if (filesToCopy.Count == 0)
                return;

            ModifyContainer(fileAction, isCombined, targetBinder =>
            {
                // Get names for unique naming of pasted file
                var currentNames = targetBinder.Files.Select(e => e.Name).ToList();

                var newIdBase = targetBinder.Files.Last().ID + 1;

                // Add in new BinderFiles to target (current binder selection)
                foreach (var file in filesToCopy)
                {
                    var newName = HavokBinderUtils.GetUniqueFileName(file.Name, currentNames, "hkx");

                    var newBinderFile = new BinderFile
                    {
                        Flags = file.Flags,
                        ID = newIdBase,
                        Name = newName,
                        Bytes = file.Bytes,
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
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    private void DeleteFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return;

        if (fileAction.MultipleFilePaths.Any(e => !files.ContainsKey(e)))
            return;

        ModifyContainer(fileAction, isCombined, binder =>
        {
            var anyRemoved = false;

            foreach (var entry in fileAction.MultipleFilePaths)
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

    private void RenameFile(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (!fileAction.BankDict.TryGetValue(fileAction.BinderEntry, out var files))
            return;

        if (!files.ContainsKey(fileAction.FilePath))
            return;

        ModifyContainer(fileAction, isCombined, binder =>
        {
            var anyRenamed = false;

            foreach (var file in binder.Files)
            {
                if (file.Name != fileAction.FilePath)
                    continue;

                var name = Path.GetFileNameWithoutExtension(file.Name);

                file.Name = HavokBinderUtils.ReplaceFileName(file.Name, fileAction.NewFilename);

                var logName = Path.GetFileNameWithoutExtension(file.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Log", name, logName));

                anyRenamed = true;
            }

            return anyRenamed;
        });
    }

    private void InsertFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.Inserts.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        ModifyContainer(fileAction, isCombined, binder =>
        {
            foreach (var entry in fileAction.Inserts)
            {
                var lastFile = binder.Files.Last();

                var filename = Path.GetFileName(entry.FilePath);
                var insertFile = HavokBinderUtils.GetInsertFile(fileAction, lastFile, filename, entry.FileData);

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
            using var binder = ReadContainer(fileAction.BinderEntry, isCombined);

            if (binder == null)
                return;

            foreach (var entry in fileAction.MultipleFilePaths)
            {
                foreach (var file in binder.Files)
                {
                    if (file.Name != entry)
                        continue;

                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = ".hkx";

                    if (IsDcxFile(file.Name))
                    {
                        name = Path.GetFileNameWithoutExtension(name);
                        extension = ".hkx.dcx";
                    }

                    var exportPath = Path.Join(Project.Descriptor.ProjectPath, $"{name}{extension}");

                    File.WriteAllBytes(exportPath, file.Bytes.ToArray());

                    Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Export_Log", exportPath));
                }
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

        if(Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
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

    public void CompressHavokFile(HavokFileView.FileAction fileAction)
    {
        CompressFiles(fileAction, false);
    }

    public void CompressCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        CompressFiles(fileAction, true);
    }

    private void CompressFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        ModifyContainer(fileAction, isCombined, binder =>
            CompressBinderFiles(binder.Files, fileAction.MultipleFilePaths) > 0);
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
            // Not a readable DCX, use the fallback
        }

        return FallbackDCXType;
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

    public void ChangeCompressionHavokFile(HavokFileView.FileAction fileAction)
    {
        ChangeCompressionFiles(fileAction, false);
    }

    public void ChangeCompressionCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        ChangeCompressionFiles(fileAction, true);
    }

    private void ChangeCompressionFiles(HavokFileView.FileAction fileAction, bool isCombined)
    {
        if (fileAction.MultipleFilePaths.Count == 0 || fileAction.NewCompressionType == DCX.Type.Unknown)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        ModifyContainer(fileAction, isCombined, binder =>
            ChangeCompressionBinderFiles(binder.Files, fileAction.MultipleFilePaths, fileAction.NewCompressionType) > 0);
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
    }
    #endregion
}
