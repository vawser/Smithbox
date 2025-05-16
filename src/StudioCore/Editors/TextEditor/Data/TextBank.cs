using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Data;
using StudioCore.Formats.JSON;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public ConcurrentDictionary<FileDictionaryEntry, TextContainerWrapper> Entries = new();

    public TextBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        var tasks = new List<Task>();

        // msgbnd
        foreach (var entry in Project.TextData.FmgFiles.Entries)
        {
            if (entry.Extension != "msgbnd")
                continue;

            tasks.Add(Task.Run(() => LoadFmgContainer(entry)));
        }

        // fmg
        foreach (var entry in Project.TextData.FmgFiles.Entries)
        {
            if (entry.Extension != "fmg")
                continue;

            tasks.Add(Task.Run(() => LoadFmg(entry)));
        }

        await Task.WhenAll(tasks);

        return true;
    }

    /// <summary>
    /// Load FMG file
    /// </summary>
    public void LoadFmg(FileDictionaryEntry entry)
    {
        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(Project, entry.Path);

        // Skip non-English if this is disabled
        if(!CFG.Current.TextEditor_IncludeNonPrimaryContainers)
        {
            if(containerCategory != CFG.Current.TextEditor_PrimaryCategory)
            {
                return;
            }
        }

        try
        {
            // Get compression type
            var fmgFileBytes = TargetFS.ReadFileOrThrow(entry.Path);

            DCX.Type compressionType;
            var reader = new BinaryReaderEx(false, fmgFileBytes);
            SFUtil.GetDecompressedBR(reader, out compressionType);

            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            var id = -1;
            var fmg = FMG.Read(fmgFileBytes);
            fmg.Name = entry.Filename; // Assign this to make it easier to grab FMGs

            TextContainerWrapper containerWrapper = new(Project);
            containerWrapper.FileEntry = entry;

            containerWrapper.CompressionType = compressionType;
            containerWrapper.ContainerType = containerType;
            containerWrapper.ContainerDisplayCategory = containerCategory;

            TextFmgWrapper fmgInfo = new();
            fmgInfo.ID = id;
            fmgInfo.Name = entry.Filename;
            fmgInfo.File = fmg;
            fmgInfo.Parent = containerWrapper;

            fmgWrappers.Add(fmgInfo);

            containerWrapper.FmgWrappers = fmgWrappers;

            if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                containerWrapper.ContainerDisplaySubCategory = TextUtils.GetSubCategory(entry.Path);
            }

            Entries.TryAdd(entry, containerWrapper);
        }
        catch (Exception e)
        {
            var filename = Path.GetFileNameWithoutExtension(entry.Path);
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to load FMG: {filename} at {entry.Path}", LogLevel.Error, Tasks.LogPriority.High, e);
        }
    }

    /// <summary>
    /// Load FMG container
    /// </summary>
    public void LoadFmgContainer(FileDictionaryEntry entry)
    {
        var containerType = TextContainerType.BND;
        var containerCategory = TextUtils.GetLanguageCategory(Project, entry.Path);

        // Skip non-English if this is disabled
        if (!CFG.Current.TextEditor_IncludeNonPrimaryContainers)
        {
            if (containerCategory is not TextContainerCategory.English)
            {
                return;
            }
        }

        try
        {
            var containerBytes = TargetFS.ReadFileOrThrow(entry.Path);

            DCX.Type compressionType;
            var reader = new BinaryReaderEx(false, containerBytes);
            SFUtil.GetDecompressedBR(reader, out compressionType);

            // Create the Text Container wrapper and and add it to the bank
            TextContainerWrapper containerWrapper = new(Project);
            containerWrapper.FileEntry = entry;

            containerWrapper.CompressionType = compressionType;
            containerWrapper.ContainerType = containerType;
            containerWrapper.ContainerDisplayCategory = containerCategory;

            // Populate the Text Fmg wrappers with their contents
            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                using (IBinder binder = BND3.Read(containerBytes))
                {
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.Contains(".fmg"))
                        {
                            var fmgName = Path.GetFileName(file.Name);
                            var id = file.ID;
                            var fmg = FMG.Read(file.Bytes);
                            fmg.Name = fmgName;

                            TextFmgWrapper fmgInfo = new();
                            fmgInfo.ID = id;
                            fmgInfo.Name = fmgName;
                            fmgInfo.File = fmg;
                            fmgInfo.Parent = containerWrapper;

                            fmgWrappers.Add(fmgInfo);
                        }
                    }
                }
            }
            else
            {
                using (IBinder binder = BND4.Read(containerBytes))
                {
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.Contains(".fmg"))
                        {
                            var fmgName = Path.GetFileName(file.Name);
                            var id = file.ID;
                            var fmg = FMG.Read(file.Bytes);
                            fmg.Name = fmgName;

                            TextFmgWrapper fmgInfo = new();
                            fmgInfo.ID = id;
                            fmgInfo.Name = fmgName;
                            fmgInfo.File = fmg;
                            fmgInfo.Parent = containerWrapper;

                            fmgWrappers.Add(fmgInfo);
                        }
                    }
                }
            }
            
            // Add the fmg wrappers to the container wrapper
            containerWrapper.FmgWrappers = fmgWrappers;

            Entries.TryAdd(entry, containerWrapper);
        }
        catch (Exception e)
        {
            var filename = Path.GetFileNameWithoutExtension(entry.Path);
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to load FMG container: {filename} at {entry.Path}", LogLevel.Error, Tasks.LogPriority.High, e);
        }
    }

    /// <summary>
    /// Save all modified FMG Containers
    /// </summary>
    public async Task<bool> SaveTextFiles()
    {
        var success = true;

        foreach (var (fileEntry, containerInfo) in Entries)
        {
            // Only save all modified files
            if (containerInfo.IsModified)
            {
                if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    Task<bool> saveTask = SaveLooseFmg(fileEntry, containerInfo);
                    bool saveTaskResult = await saveTask;

                    if (!saveTaskResult)
                        success = false;
                }
                else
                {
                    Task<bool> saveTask = SaveFmgContainer(fileEntry, containerInfo);
                    bool saveTaskResult = await saveTask;

                    if (!saveTaskResult)
                        success = false;
                }
            }
        }

        return success;
    }

    /// <summary>
    /// Save passed FMG container
    /// </summary>
    public async Task<bool> SaveFmgContainer(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        await Task.Yield();

        if (entry == null || containerWrapper == null)
            return false;

        byte[] fileBytes = null;

        try
        {
            var containerBytes = TargetFS.ReadFileOrThrow(entry.Path);

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES or ProjectType.ACFA)
            {
                using (IBinder binder = BND3.Read(containerBytes))
                {
                    foreach (var file in binder.Files)
                    {
                        WriteFmgContents(file, containerWrapper.FmgWrappers);
                    }

                    using (BND3 writeBinder = binder as BND3)
                    {
                        fileBytes = writeBinder.Write(containerWrapper.CompressionType);
                    }
                }
            }
            else
            {
                using (IBinder binder = BND4.Read(containerBytes))
                {
                    foreach (var file in binder.Files)
                    {
                        WriteFmgContents(file, containerWrapper.FmgWrappers);
                    }

                    using (BND4 writeBinder = binder as BND4)
                    {
                        fileBytes = writeBinder.Write(containerWrapper.CompressionType);
                    }
                }
            }

            try
            {
                Project.ProjectFS.WriteFile(entry.Path, fileBytes);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to write {entry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to read {entry.Filename} from VFS.", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return true;
    }

    private void WriteFmgContents(BinderFile file, List<TextFmgWrapper> fmgList)
    {
        if (file.Name.Contains(".fmg"))
        {
            foreach (var entry in fmgList)
            {
                if (entry.ID == file.ID)
                {
                    try
                    {
                        file.Bytes = entry.File.Write();
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to write FMG file: {file.ID}\n{ex}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saved passed loose FMG file
    /// </summary>
    public async Task<bool> SaveLooseFmg(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        await Task.Yield();

        if (entry == null || containerWrapper == null)
            return false;

        try
        {
            var newFmgBytes = containerWrapper.FmgWrappers.First().File.Write();

            try
            {
                Project.ProjectFS.WriteFile(entry.Path, newFmgBytes);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to write {entry.Filename} as file.", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to write {entry.Filename} as FMG", LogLevel.Error, Tasks.LogPriority.High, e);
            return false;
        }

        return true;
    }
}



