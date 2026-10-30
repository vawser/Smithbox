using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public ConcurrentDictionary<FileDictionaryEntry, TextContainerWrapper> Containers = new();

    public TextBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        var tasks = new List<Task>();

        foreach (var entry in Project.Locator.TextFiles.Entries)
        {
            if (entry.Extension == "msgbnd")
            {
                tasks.Add(Task.Run(() => LoadFmgContainer(entry)));
            }

            if (entry.Extension == "fmg")
            {
                tasks.Add(Task.Run(() => LoadFmg(entry)));
            }
        }

        await Task.WhenAll(tasks);

        return true;
    }

    /// <summary>
    /// Load FMG container
    /// </summary>
    public void LoadFmgContainer(FileDictionaryEntry entry)
    {
        var containerType = TextContainerType.BND;
        var containerCategory = TextUtils.GetLanguageCategory(Project, entry.Path);

        // Skip obsolete containers
        if (TextUtils.IsObsoleteContainer(Project, entry))
        {
            return;
        }

        if (TargetFS.FileExists(entry.Path))
        {
            try
            {
                var containerBytes = TargetFS.ReadFile(entry.Path);

                DCX.Type compressionType;
                var reader = new BinaryReaderEx(false, containerBytes.Value);
                SFUtil.GetDecompressedBR(reader, out compressionType);

                // Create the Text Container wrapper and and add it to the bank
                TextContainerWrapper containerWrapper = new(Project)
                {
                    FileEntry = entry,

                    CompressionType = compressionType,
                    ContainerType = containerType,
                    ContainerDisplayCategory = containerCategory,
                    FmgWrappers = new()
                };

                if (containerCategory == CFG.Current.TextEditor_Primary_Category)
                {
                    LoadFmgWrappers(containerWrapper);
                }

                if (containerWrapper != null)
                {
                    Containers.TryAdd(entry, containerWrapper);
                }
            }
            catch (Exception e)
            {
                var filename = Path.GetFileNameWithoutExtension(entry.Path);

                Smithbox.LogError(this,
                    LOC.Get("TEXT_Data_Load_FMG_Container_FAIL", filename, entry.Path, Name), e);
            }
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("TEXT_Data_Find_FMG_Container_FAIL", entry.Path, Name));
        }
    }

    public void LoadAllFmgWrappers(FmgWrapperLoadType type)
    {
        foreach (var entry in Containers)
        {
            if (type is FmgWrapperLoadType.PrimaryLanguage)
            {
                if (entry.Value.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                {
                    continue;
                }
            }

            LoadFmgWrappers(entry.Value);
        }
    }

    public async Task LoadFmgWrappersAsync(TextContainerWrapper container)
    {
        try
        {
            await Task.Run(() => LoadFmgWrappers(container));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Load_FMG_Wrapper_FAIL"), ex);
        }
    }

    public void LoadFmgWrappers(TextContainerWrapper container)
    {
        if (TargetFS.FileExists(container.FileEntry.Path))
        {
            var containerBytes = TargetFS.ReadFile(container.FileEntry.Path);

            // Populate the Text Fmg wrappers with their contents
            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                using (IBinder binder = BND3.Read(containerBytes.Value))
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
                            fmgInfo.Parent = container;

                            fmgWrappers.Add(fmgInfo);
                        }
                    }
                }
            }
            else
            {
                using (IBinder binder = BND4.Read(containerBytes.Value))
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
                            fmgInfo.Parent = container;

                            fmgWrappers.Add(fmgInfo);
                        }
                    }
                }
            }

            container.FmgWrappers = fmgWrappers;
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Data_Find_FMG_Wrapper_FAIL", container.FileEntry.Path));
        }
    }

    /// <summary>
    /// Load FMG file
    /// </summary>
    public void LoadFmg(FileDictionaryEntry entry)
    {
        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(Project, entry.Path);

        if (TargetFS.FileExists(entry.Path))
        {
            try
            {
                // Get compression type
                var fmgFileBytes = TargetFS.ReadFile(entry.Path);

                DCX.Type compressionType;
                var reader = new BinaryReaderEx(false, fmgFileBytes.Value);
                SFUtil.GetDecompressedBR(reader, out compressionType);

                List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

                var id = -1;
                var fmg = FMG.Read(fmgFileBytes.Value);
                fmg.Name = entry.Filename; // Assign this to make it easier to grab FMGs

                TextContainerWrapper containerWrapper = new(Project)
                {
                    FileEntry = entry,

                    CompressionType = compressionType,
                    ContainerType = containerType,
                    ContainerDisplayCategory = containerCategory
                };

                TextFmgWrapper fmgInfo = new()
                {
                    ID = id,
                    Name = entry.Filename,
                    File = fmg,
                    Parent = containerWrapper
                };

                fmgWrappers.Add(fmgInfo);

                containerWrapper.FmgWrappers = fmgWrappers;

                if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    containerWrapper.ContainerDisplaySubCategory = TextUtils.GetSubCategory(entry.Path);
                }

                Containers.TryAdd(entry, containerWrapper);
            }
            catch (Exception e)
            {
                var filename = Path.GetFileNameWithoutExtension(entry.Path);

                Smithbox.LogError(this,
                    LOC.Get("TEXT_Data_Load_FMG_FAIL", filename, entry.Path, Name), e);
            }
        }
        else
        {
            Smithbox.LogError(this,
                LOC.Get("TEXT_Data_Find_File_VFS_FAIL", entry.Path, Name));
        }
    }

    /// <summary>
    /// Save all modified FMG Containers
    /// </summary>
    public async Task<bool> SaveTextFiles()
    {
        var success = true;

        foreach (var (fileEntry, containerInfo) in Containers)
        {
            // Only save the files that have been modified
            if (!containerInfo.IsDirty())
                continue;

            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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

        return success;
    }

    /// <summary>
    /// Save passed FMG container
    /// </summary>
    public async Task<bool> SaveFmgContainer(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        await Task.Yield();

        if (entry == null || containerWrapper == null || containerWrapper.FmgWrappers == null)
            return false;

        if (!CFG.Current.TextEditor_Allow_Unmodified_Save)
        {
            if (!containerWrapper.IsDirty())
            {
                Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", entry.Filename));
                return false;
            }
        }

        if (!TargetFS.FileExists(entry.Path))
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", entry.Path));
            return false;
        }

        if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
        {
            return HandleContainer_BND3(entry, containerWrapper);
        }
        else
        {
            return HandleContainer_BND4(entry, containerWrapper);
        }
    }

    private bool HandleContainer_BND3(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        // Container
        var containerFileData = TargetFS.ReadFile(entry.Path);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", entry.Path));
            return false;
        }

        // Binder
        BND3 binder;

        try
        {
            binder = BND3.Read(containerFileData.Value);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", entry.Path), ex);
            return false;
        }

        using (binder)
        {
            // Binder Files
            var isFilesWritten = true;

            foreach (var curFile in binder.Files)
            {
                if (!HandleBinderFile(curFile, containerWrapper))
                    isFilesWritten = false;
            }

            // Save Binder
            try
            {
                var writeFile = binder.Write(containerWrapper.CompressionType);

                Project.VFS.ProjectFS.WriteFile(entry.Path, writeFile);

                // Only clear the dirty state if every FMG was written successfully
                if (isFilesWritten)
                    containerWrapper.ClearDirtyState();

                Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", entry.Filename));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", entry.Path), ex);
                return false;
            }

            return isFilesWritten;
        }
    }

    private bool HandleContainer_BND4(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        // Container
        var containerFileData = TargetFS.ReadFile(entry.Path);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", entry.Path));
            return false;
        }

        // Binder
        BND4 binder;

        try
        {
            binder = BND4.Read(containerFileData.Value);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", entry.Path), ex);
            return false;
        }

        using (binder)
        {
            // Binder Files
            var isFilesWritten = true;

            foreach (var curFile in binder.Files)
            {
                if (!HandleBinderFile(curFile, containerWrapper))
                    isFilesWritten = false;
            }

            // Save Binder
            try
            {
                var writeFile = binder.Write(containerWrapper.CompressionType);

                Project.VFS.ProjectFS.WriteFile(entry.Path, writeFile);

                // Only clear the dirty state if every FMG was written successfully
                if (isFilesWritten)
                    containerWrapper.ClearDirtyState();

                Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", entry.Filename));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", entry.Path), ex);
                return false;
            }

            return isFilesWritten;
        }
    }

    /// <summary>
    /// Write the matching FMG into the binder file. Returns false if the FMG failed to write.
    /// </summary>
    private bool HandleBinderFile(BinderFile file, TextContainerWrapper containerWrapper)
    {
        if (!file.Name.Contains(".fmg"))
            return true;

        var fmgWrapper = containerWrapper.FmgWrappers.FirstOrDefault(e => e.ID == file.ID);

        if (fmgWrapper == null)
            return true;

        // Internal File
        try
        {
            file.Bytes = fmgWrapper.File.Write();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", file.Name), ex);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Saved passed loose FMG file
    /// </summary>
    public async Task<bool> SaveLooseFmg(FileDictionaryEntry entry, TextContainerWrapper containerWrapper)
    {
        await Task.Yield();

        if (entry == null || containerWrapper == null || containerWrapper.FmgWrappers == null)
            return false;

        if (!CFG.Current.TextEditor_Allow_Unmodified_Save)
        {
            if (!containerWrapper.IsDirty())
            {
                Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", entry.Filename));
                return false;
            }
        }

        var fmgWrapper = containerWrapper.FmgWrappers.FirstOrDefault();

        if (fmgWrapper == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_External_File", entry.Path));
            return false;
        }

        // External File
        try
        {
            var fileData = fmgWrapper.File.Write();

            Project.VFS.ProjectFS.WriteFile(entry.Path, fileData);

            containerWrapper.ClearDirtyState();

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", entry.Filename));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", entry.Path), e);
            return false;
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Containers.Clear();

        Containers = null;
    }
    #endregion
}

public enum FmgWrapperLoadType
{
    All,
    PrimaryLanguage
}