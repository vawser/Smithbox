using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using Octokit;
using Silk.NET.OpenGL;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokBank : IDisposable
{
    public ProjectEntry Project;

    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> AnimationBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> BehaviorBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> CharacterBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> CutsceneBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> NavmeshBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> PartBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> RumbleBank = new();

    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> MapCollisionBank = new();
    public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> AssetCollisionBank = new();

    public HavokBank(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        return true;
    }

    #region Populate
    public void PopulateAnimationBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(AnimationBank, entry, clearCache);
    }

    public void PopulateBehaviorBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(BehaviorBank, entry, clearCache);
    }

    public void PopulateCharacterBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(CharacterBank, entry, clearCache);
    }

    public void PopulateNavmeshBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(NavmeshBank, entry, clearCache);
    }

    public void PopulateCutsceneBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(CutsceneBank, entry, clearCache);
    }

    public void PopulatePartBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(PartBank, entry, clearCache);
    }

    public void PopulateRumbleBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(RumbleBank, entry, clearCache);
    }

    public void PopulateMapCollisionBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateCombinedFiles(MapCollisionBank, entry,
                "HAVOK_Data_Failed_to_Read_Collision_Binder_File", clearCache);
    }

    public void PopulateAssetCollisionBank(FileDictionaryEntry entry, bool clearCache = false)
    {
        PopulateFiles(AssetCollisionBank, entry, clearCache);
    }

    #endregion

    #region Load
    public void LoadAnimationFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(AnimationBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Animation_HKX",
            "HAVOK_Data_Failed_to_Read_Animation_Binder_File", true);
    }

    public void LoadBehaviorFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(BehaviorBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Behavior_HKX",
            "HAVOK_Data_Failed_to_Read_Behavior_Binder_File");
    }

    public void LoadCharacterFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(CharacterBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Character_HKX",
            "HAVOK_Data_Failed_to_Read_Character_Binder_File");
    }

    public void LoadCutsceneFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(CutsceneBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Cutscene_HKX",
            "HAVOK_Data_Failed_to_Read_Cutscene_Binder_File");
    }

    public void LoadNavmeshFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(NavmeshBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Navmesh_HKX",
            "HAVOK_Data_Failed_to_Read_Navmesh_Binder_File");
    }

    public void LoadPartFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(PartBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Part_HKX",
            "HAVOK_Data_Failed_to_Read_Part_Binder_File");
    }

    public void LoadRumbleFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(RumbleBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Rumble_HKX",
            "HAVOK_Data_Failed_to_Read_Rumble_Binder_File");
    }

    public void LoadMapCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadCombinedHavokFile(MapCollisionBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Collision_HKX",
            "HAVOK_Data_Failed_to_Read_Collision_Binder_File");
    }

    public void LoadAssetCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        LoadHavokFile(AssetCollisionBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Read_Collision_HKX",
            "HAVOK_Data_Failed_to_Read_Collision_Binder_File");
    }
    #endregion

    #region Save
    public void SaveAnimationFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(AnimationBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Animation_HKX",
            "HAVOK_Data_Failed_to_Write_Animation_Binder_File");
    }

    public void SaveBehaviorFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(BehaviorBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Behavior_HKX",
            "HAVOK_Data_Failed_to_Write_Behavior_Binder_File");
    }

    public void SaveCharacterFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(CharacterBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Character_HKX",
            "HAVOK_Data_Failed_to_Write_Character_Binder_File");
    }

    public void SaveCutsceneFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(CutsceneBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Cutscene_HKX",
            "HAVOK_Data_Failed_to_Write_Cutscene_Binder_File");
    }

    public void SaveNavmeshFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(NavmeshBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Navmesh_HKX",
            "HAVOK_Data_Failed_to_Write_Navmesh_Binder_File");
    }

    public void SavePartFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(PartBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Part_HKX",
            "HAVOK_Data_Failed_to_Write_Part_Binder_File");
    }

    public void SaveRumbleFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(RumbleBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Rumble_HKX",
            "HAVOK_Data_Failed_to_Write_Rumble_Binder_File");
    }
    public void SaveMapCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveCombinedHavokFile(MapCollisionBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Collision_HKX",
            "HAVOK_Data_Failed_to_Write_Collision_Binder_File");
    }

    public void SaveAssetCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        SaveHavokFile(AssetCollisionBank, fileEntry, internalFilePath,
            "HAVOK_Data_Failed_to_Write_Collision_HKX",
            "HAVOK_Data_Failed_to_Write_Collision_Binder_File");
    }

    #endregion

    #region Populate Internals
    public void PopulateFiles(
        Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict,
        FileDictionaryEntry fileEntry,
        bool clearCache = false)
    {
        if (clearCache)
            bankDict.Clear();

        if (!bankDict.ContainsKey(fileEntry))
            bankDict.Add(fileEntry, new Dictionary<string, hkRootLevelContainer>());

        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        var binder = new BND4Reader(binderData.Value);
        foreach (var file in binder.Files)
        {
            if (bankDict.ContainsKey(fileEntry))
            {
                var curTopDict = bankDict[fileEntry];

                if (!curTopDict.ContainsKey(file.Name))
                {
                    bankDict[fileEntry].Add(file.Name, null);

                    Project.Handler.HavokEditor.ViewHandler.ActiveView.Selection.AddToFileAliasCache(file.Name);
                }
            }
        }
    }

    public void PopulateCombinedFiles(
        Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict,
        FileDictionaryEntry fileEntry,
        string failedBinderReadLocKey,
        bool clearCache = false)
    {
        if (clearCache)
            bankDict.Clear();

        if (!bankDict.ContainsKey(fileEntry))
            bankDict.Add(fileEntry, new Dictionary<string, hkRootLevelContainer>());

        var bhdPath = fileEntry.Path;
        var bdtPath = fileEntry.Path.Replace("bhd", "bdt");

        var name = Path.GetFileNameWithoutExtension(fileEntry.Path);

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var file in packedBinder.Files)
            {
                if (bankDict.ContainsKey(fileEntry))
                {
                    var curTopDict = bankDict[fileEntry];

                    if (!curTopDict.ContainsKey(file.Name))
                    {
                        bankDict[fileEntry].Add(file.Name, null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this,
                LOC.Get(failedBinderReadLocKey, name), ex);
        }
    }
    #endregion

    #region Load Internals
    public void LoadHavokFile(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict, FileDictionaryEntry fileEntry, string internalFilePath, string fileReadFailLocKey, string binderReadFailLocKey, bool loadCompendium = false)
    {
        if (!bankDict.ContainsKey(fileEntry))
            return;

        var curTopDict = bankDict[fileEntry];

        if (!curTopDict.ContainsKey(internalFilePath))
            return;

        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        HavokBinarySerializer serializer = new HavokBinarySerializer();

        var binder = new BND4Reader(binderData.Value);

        if (loadCompendium)
        {
            // Get compendium
            byte[] compendiumFileBytes = null;

            foreach (var file in binder.Files)
            {
                if (file.Name.Contains(".compendium.dcx"))
                {
                    var fileBytes = binder.ReadFile(file).ToArray();

                    compendiumFileBytes = DCX.Decompress(fileBytes).ToArray();
                }
                else if (file.Name.Contains(".compendium"))
                {
                    compendiumFileBytes = binder.ReadFile(file).ToArray();
                }
            }

            if (compendiumFileBytes != null)
            {
                using MemoryStream memoryStream = new MemoryStream(compendiumFileBytes);
                serializer.LoadCompendium(memoryStream);
            }
        }

        foreach (var file in binder.Files)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (file.Name != internalFilePath)
                continue;

            try
            {
                var fileBytes = binder.ReadFile(file).ToArray();

                using (MemoryStream memoryStream = new MemoryStream(fileBytes))
                {
                    hkRootLevelContainer fileHkx;

                    try
                    {
                        fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                        bankDict[fileEntry][internalFilePath] = fileHkx;
                    }
                    catch (InvalidDataException ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get(fileReadFailLocKey, name), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this,
                    LOC.Get(binderReadFailLocKey, name), ex);
            }
        }
    }

    public void LoadCombinedHavokFile(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict, FileDictionaryEntry fileEntry, string internalFilePath, string fileReadFailLocKey, string binderReadFailLocKey)
    {
        if (!bankDict.ContainsKey(fileEntry))
            return;

        var curTopDict = bankDict[fileEntry];

        if (!curTopDict.ContainsKey(internalFilePath))
            return;

        var bhdPath = fileEntry.Path;
        var bdtPath = fileEntry.Path.Replace("bhd", "bdt");

        var name = Path.GetFileNameWithoutExtension(internalFilePath);

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            HavokBinarySerializer serializer = new HavokBinarySerializer();

            // Get compendium
            byte[] compendiumFileBytes = null;

            foreach (var file in packedBinder.Files)
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

            foreach (var file in packedBinder.Files)
            {
                if (file.Name != internalFilePath)
                    continue;

                byte[] fileBytes = null;

                if (file.Name.Contains(".dcx"))
                {
                    fileBytes = DCX.Decompress(file.Bytes).ToArray();
                }
                else
                {
                    fileBytes = file.Bytes.ToArray();
                }

                using (MemoryStream memoryStream = new MemoryStream(fileBytes))
                {
                    hkRootLevelContainer fileHkx;

                    try
                    {
                        fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                        bankDict[fileEntry][internalFilePath] = fileHkx;
                    }
                    catch (InvalidDataException ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get(fileReadFailLocKey, name), ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this,
                LOC.Get(binderReadFailLocKey, name), ex);
        }
    }

    #endregion

    #region Save Internals
    public void SaveHavokFile(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict, FileDictionaryEntry fileEntry, string internalFilePath, string writeFileFailLocKey, string writeBinderFailLocKey)
    {
        if (!bankDict.ContainsKey(fileEntry))
            return;

        var curTopDict = bankDict[fileEntry];

        if (!curTopDict.ContainsKey(internalFilePath))
            return;

        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        bool anyWritten = false;

        HavokBinarySerializer serializer = new HavokBinarySerializer();

        var binder = BND4.Read(binderData.Value);

        foreach (var file in binder.Files)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (file.Name != internalFilePath)
                continue;

            if (!bankDict[fileEntry].ContainsKey(internalFilePath))
                continue;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                {
                    var objEntry = bankDict[fileEntry][internalFilePath];

                    if (objEntry != null)
                    {
                        serializer.Write(objEntry, memoryStream);

                        file.Bytes = memoryStream.ToArray();
                        anyWritten = true;
                    }
                    else
                    {
                        Smithbox.LogError(this, LOC.Get("HAVOK_Data_Invalid_Root_Container"));
                    }
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get(writeFileFailLocKey, name), ex);
            }
        }

        if (!anyWritten)
            return;

        try
        {
            var writtenBinder = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get(writeBinderFailLocKey, fileEntry.Path), ex);
        }
    }

    public void SaveCombinedHavokFile(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict, FileDictionaryEntry fileEntry, string internalFilePath, string writeFileFailLocKey, string writeBinderFailLocKey)
    {
        if (!bankDict.ContainsKey(fileEntry))
            return;

        var curTopDict = bankDict[fileEntry];

        if (!curTopDict.ContainsKey(internalFilePath))
            return;

        var bhdPath = fileEntry.Path;
        var bdtPath = fileEntry.Path.Replace("bhd", "bdt");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            bool anyWritten = false;

            HavokBinarySerializer serializer = new HavokBinarySerializer();

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var file in packedBinder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Name != internalFilePath)
                    continue;

                if (!bankDict[fileEntry].ContainsKey(internalFilePath))
                    continue;

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        var objEntry = bankDict[fileEntry][internalFilePath];
                        if (objEntry != null)
                        {
                            serializer.Write(objEntry, memoryStream);

                            if (file.Name.Contains(".dcx"))
                            {
                                var compressedBytes = DCX.Compress(memoryStream.ToArray(), DCX.Type.DCX_KRAK);
                                file.Bytes = compressedBytes;
                            }
                            else
                            {
                                file.Bytes = file.Bytes.ToArray();
                            }

                            anyWritten = true;
                        }
                        else
                        {
                            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Invalid_Root_Container"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get(writeFileFailLocKey, name), ex);
                }
            }

            if (!anyWritten)
                return;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get(writeBinderFailLocKey, fileEntry.Path), ex);
        }
    }

    #endregion

    #region File Manipulation
    public void PasteHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.ClipboardBinder == null)
            return;

        if (fileAction.ClipboardFiles.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.ClipboardBinder))
            return;

        var sourceBinderData = Project.VFS.FS.ReadFile(fileAction.ClipboardBinder.Path);
        var targetBinderData = Project.VFS.FS.ReadFile(fileAction.BinderEntry.Path);

        if (sourceBinderData == null)
            return;

        if (targetBinderData == null)
            return;

        var sourceBinder = BND4.Read(sourceBinderData.Value);
        var targetBinder = BND4.Read(targetBinderData.Value);

        var filesToCopy = new List<BinderFile>();

        // Get BinderFile list from source binder (based on clipboard)
        foreach (var file in sourceBinder.Files)
        {
            if(fileAction.ClipboardFiles.Contains(file.Name))
            {
                filesToCopy.Add(file);
            }
        }

        var currentNames = new List<string>();

        foreach (var file in targetBinder.Files)
        {
            currentNames.Add(file.Name);
        }

        var lastFile = targetBinder.Files.Last();
        var newIdBase = lastFile.ID + 1;

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

        try
        {
            var writtenBinder = targetBinder.Write();

            Project.VFS.ProjectFS.WriteFile(fileAction.BinderEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void PasteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.ClipboardBinder == null)
            return;

        if (fileAction.ClipboardFiles.Count == 0)
            return;

        var sourceBhdPath = fileAction.ClipboardBinder.Path;
        var sourceBdtPath = fileAction.ClipboardBinder.Path.Replace("bhd", "bdt");

        var targetBhdPath = fileAction.BinderEntry.Path;
        var targetBdtPath = fileAction.BinderEntry.Path.Replace("bhd", "bdt");

        // Target
        var targetBdtData = Project.VFS.FS.ReadFile(targetBdtPath);
        var targetBhdData = Project.VFS.FS.ReadFile(targetBhdPath);

        if (Project.VFS.ProjectFS.FileExists(targetBdtPath))
        {
            targetBdtData = Project.VFS.ProjectFS.ReadFile(targetBdtPath);
        }
        if (Project.VFS.ProjectFS.FileExists(targetBhdPath))
        {
            targetBhdData = Project.VFS.ProjectFS.ReadFile(targetBhdPath);
        }

        // Source
        var sourceBdtData = Project.VFS.FS.ReadFile(sourceBdtPath);
        var sourceBhdData = Project.VFS.FS.ReadFile(sourceBhdPath);

        if (Project.VFS.ProjectFS.FileExists(sourceBdtPath))
        {
            sourceBdtData = Project.VFS.ProjectFS.ReadFile(sourceBdtPath);
        }
        if (Project.VFS.ProjectFS.FileExists(sourceBhdPath))
        {
            sourceBhdData = Project.VFS.ProjectFS.ReadFile(sourceBhdPath);
        }

        if (targetBdtData == null || targetBhdData == null)
            return;

        if (sourceBdtData == null || sourceBhdData == null)
            return;

        var targetPackedBinder = BXF4.Read((Memory<byte>)targetBhdData, (Memory<byte>)targetBdtData);
        var sourcePackedBinder = BXF4.Read((Memory<byte>)sourceBhdData, (Memory<byte>)sourceBdtData);

        var filesToCopy = new List<BinderFile>();

        // Get BinderFile list from source binder (based on clipboard)
        foreach (var file in sourcePackedBinder.Files)
        {
            if (fileAction.ClipboardFiles.Contains(file.Name))
            {
                filesToCopy.Add(file);
            }
        }

        // Get names for unique naming of pasted file
        var currentNames = new List<string>();

        foreach (var file in targetPackedBinder.Files)
        {
            currentNames.Add(file.Name);
        }

        var lastFile = targetPackedBinder.Files.Last();
        var newIdBase = lastFile.ID + 1;

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
            targetPackedBinder.Files.Add(newBinderFile);

            newIdBase = newIdBase + 1;

            Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Paste_Log", newName, fileAction.BinderEntry.Filename));
        }

        try
        {
            targetPackedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(targetBhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(targetBdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void DeleteHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

            if (!curTopDict.ContainsKey(entry))
                return;
        }

        var binderData = Project.VFS.FS.ReadFile(fileAction.BinderEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileAction.BinderEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileAction.BinderEntry.Path);
        }

        if (binderData == null)
            return;

        bool anyWritten = false;

        var binder = BND4.Read(binderData.Value);

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var sourceFile = new BinderFile();

            foreach (var file in binder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Name != entry)
                    continue;

                sourceFile = file;
                anyWritten = true;
            }

            binder.Files.Remove(sourceFile);

            var logName = Path.GetFileNameWithoutExtension(sourceFile.Name);

            Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Delete_Log", logName, fileAction.BinderEntry.Filename));
        }

        if (!anyWritten)
            return;

        try
        {
            var writtenBinder = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileAction.BinderEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void DeleteCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

            if (!curTopDict.ContainsKey(fileAction.FilePath))
                return;
        }

        var bhdPath = fileAction.BinderEntry.Path;
        var bdtPath = fileAction.BinderEntry.Path.Replace("bhd", "bdt");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            bool anyWritten = false;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var entry in fileAction.MultipleFilePaths)
            {
                var sourceFile = new BinderFile();

                foreach (var file in packedBinder.Files)
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);

                    if (file.Name != entry)
                        continue;

                    sourceFile = file;
                    anyWritten = true;
                }

                packedBinder.Files.Remove(sourceFile);

                var logName = Path.GetFileNameWithoutExtension(sourceFile.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Delete_Log", logName, fileAction.BinderEntry.Filename));
            }

            if (!anyWritten)
                return;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void RenameHavokFile(HavokFileView.FileAction fileAction)
    {
        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

        if (!curTopDict.ContainsKey(fileAction.FilePath))
            return;

        var binderData = Project.VFS.FS.ReadFile(fileAction.BinderEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileAction.BinderEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileAction.BinderEntry.Path);
        }

        if (binderData == null)
            return;

        bool anyWritten = false;

        var binder = BND4.Read(binderData.Value);

        foreach (var file in binder.Files)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (file.Name != fileAction.FilePath)
                continue;

            file.Name = HavokBinderUtils.ReplaceFileName(file.Name, fileAction.NewFilename);

            var logName = Path.GetFileNameWithoutExtension(file.Name);

            Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Log", name, logName));

            anyWritten = true;
        }

        if (!anyWritten)
            return;

        try
        {
            var writtenBinder = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileAction.BinderEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void RenameCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

        if (!curTopDict.ContainsKey(fileAction.FilePath))
            return;

        var bhdPath = fileAction.BinderEntry.Path;
        var bdtPath = fileAction.BinderEntry.Path.Replace("bhd", "bdt");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            bool anyWritten = false;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var file in packedBinder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Name != fileAction.FilePath)
                    continue;

                file.Name = HavokBinderUtils.ReplaceFileName(file.Name, fileAction.NewFilename);

                var logName = Path.GetFileNameWithoutExtension(file.Name);

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Rename_Log", name, logName));

                anyWritten = true;
            }

            if (!anyWritten)
                return;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void InsertHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.Inserts.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        var binderData = Project.VFS.FS.ReadFile(fileAction.BinderEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileAction.BinderEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileAction.BinderEntry.Path);
        }

        if (binderData == null)
            return;

        bool anyWritten = false;

        var binder = BND4.Read(binderData.Value);

        foreach (var entry in fileAction.Inserts)
        {
            var sourceFile = new BinderFile();
            var lastFile = binder.Files.Last();

            var filename = Path.GetFileName(entry.FilePath);
            var insertFile = HavokBinderUtils.GetInsertFile(fileAction, lastFile, filename, entry.FileData);

            binder.Files.Add(insertFile);

            var logName = Path.GetFileNameWithoutExtension(insertFile.Name);
            Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Insert_Log", logName, fileAction.BinderEntry.Path));
            anyWritten = true;
        }

        if (!anyWritten)
            return;

        try
        {
            var writtenBinder = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileAction.BinderEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void InsertCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.Inserts.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        var bhdPath = fileAction.BinderEntry.Path;
        var bdtPath = fileAction.BinderEntry.Path.Replace("bhd", "bdt");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            bool anyWritten = false;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var entry in fileAction.Inserts)
            {
                var sourceFile = new BinderFile();
                var lastFile = packedBinder.Files.Last();

                var filename = Path.GetFileName(entry.FilePath);
                var insertFile = HavokBinderUtils.GetInsertFile(fileAction, lastFile, filename, entry.FileData);

                packedBinder.Files.Add(insertFile);

                var logName = Path.GetFileNameWithoutExtension(insertFile.Name);
                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Insert_Log", logName, fileAction.BinderEntry.Path));

                anyWritten = true;
            }

            if (!anyWritten)
                return;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_AddFile_Binder_Write_FAIL", fileAction.BinderEntry.Path), ex);
        }
    }

    public void ExportHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

            if (!curTopDict.ContainsKey(entry))
                return;
        }

        var binderData = Project.VFS.FS.ReadFile(fileAction.BinderEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileAction.BinderEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileAction.BinderEntry.Path);
        }

        if (binderData == null)
            return;

        var binder = BND4.Read(binderData.Value);

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var sourceFile = new BinderFile();

            foreach (var file in binder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Name.Contains(".dcx"))
                {
                    name = Path.GetFileNameWithoutExtension(name);
                }

                if (file.Name != entry)
                    continue;

                var extension = ".hkx";
                if (file.Name.Contains(".dcx"))
                {
                    extension = ".hkx.dcx";
                }

                var exportPath = Path.Join(Project.Descriptor.ProjectPath, $"{name}{extension}");

                File.WriteAllBytes(exportPath, file.Bytes.ToArray());

                Smithbox.Log(this, LOC.Get("HAVOK_FileView_ContextAction_Export_Log", exportPath));
            }
        }
    }

    public void ExportCombinedHavokFile(HavokFileView.FileAction fileAction)
    {
        if (fileAction.MultipleFilePaths.Count == 0)
            return;

        if (!fileAction.BankDict.ContainsKey(fileAction.BinderEntry))
            return;

        foreach (var entry in fileAction.MultipleFilePaths)
        {
            var curTopDict = fileAction.BankDict[fileAction.BinderEntry];

            if (!curTopDict.ContainsKey(fileAction.FilePath))
                return;
        }

        var bhdPath = fileAction.BinderEntry.Path;
        var bdtPath = fileAction.BinderEntry.Path.Replace("bhd", "bdt");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var entry in fileAction.MultipleFilePaths)
            {
                var sourceFile = new BinderFile();

                foreach (var file in packedBinder.Files)
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);

                    if (file.Name.Contains(".dcx"))
                    {
                        name = Path.GetFileNameWithoutExtension(name);
                    }

                    if (file.Name != entry)
                        continue;

                    var extension = ".hkx";
                    if (file.Name.Contains(".dcx"))
                    {
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

    #region Dispose
    public void Dispose()
    {
    }
    #endregion

}
