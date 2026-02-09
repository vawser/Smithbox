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
            containerWrapper.ContainerData = containerBytes.ToArray();
            containerWrapper.FmgWrappers = new();

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

            Smithbox.LogError(this, $"[Text Editor] Failed to load FMG container: {filename} at {entry.Path} for {Name}", e);
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

    public void LoadFmgWrappers(TextContainerWrapper container)
    {
        // Populate the Text Fmg wrappers with their contents
        List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

        if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
        {
            using (IBinder binder = BND3.Read(container.ContainerData))
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
            using (IBinder binder = BND4.Read(container.ContainerData))
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

    /// <summary>
    /// Load FMG file
    /// </summary>
    public void LoadFmg(FileDictionaryEntry entry)
    {
        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(Project, entry.Path);

        if (containerCategory != CFG.Current.TextEditor_Primary_Category)
        {
            return;
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

            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                containerWrapper.ContainerDisplaySubCategory = TextUtils.GetSubCategory(entry.Path);
            }

            Containers.TryAdd(entry, containerWrapper);
        }
        catch (Exception e)
        {
            var filename = Path.GetFileNameWithoutExtension(entry.Path);

            Smithbox.LogError(this, $"[Text Editor] Failed to load FMG: {filename} at {entry.Path} for {Name}", e);
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
            // Only save all modified files
            if (containerInfo.IsModified)
            {
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

            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
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
                Project.VFS.ProjectFS.WriteFile(entry.Path, fileBytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Text Editor] Failed to write {entry.Filename} as file for {Name}.", e);
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Text Editor] Failed to read {entry.Filename} from VFS for {Name}.", e);
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
                        Smithbox.LogError(this, $"[Text Editor] Failed to write FMG file: {file.ID} for {Name}", ex);
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
                Project.VFS.ProjectFS.WriteFile(entry.Path, newFmgBytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Text Editor] Failed to write {entry.Filename} as file for {Name}.", e);
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Text Editor] Failed to write {entry.Filename} as FMG for {Name}.", e);

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