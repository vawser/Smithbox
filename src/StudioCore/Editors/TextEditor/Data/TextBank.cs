using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public string SourcePath;
    public string FallbackPath;

    public SortedDictionary<string, TextContainerWrapper> Entries { get; private set; } = new();

    public TextBank(Smithbox baseEditor, ProjectEntry project, string sourcePath, string fallbackPath)
    {
        BaseEditor = baseEditor;
        Project = project;
        SourcePath = sourcePath;
        FallbackPath = fallbackPath;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var fmgList = TextLocator.GetFmgs(Project, "menu\\text\\");

            foreach (var path in fmgList)
            {
                LoadFmg(path);
            }
        }
        else if (Project.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            var fmgList = TextLocator.GetFmgs(Project, "lang\\");

            foreach (var path in fmgList)
            {
                LoadFmg(path);
            }
        }
        else
        {
            var fmgContainerList = TextLocator.GetFmgContainers(Project);

            foreach (var path in fmgContainerList)
            {
                LoadFmgContainer(path);
            }
        }

        return true;
    }

    /// <summary>
    /// Load FMG file
    /// </summary>
    public void LoadFmg(string path)
    {
        var name = Path.GetFileName(path);

        // TODO: need a better method for building the relative path, this seems to cause issues with some people's setups, causing a crash as the resulting relative path is empty
        var containerRelPath = path;
        if (containerRelPath.Contains(Project.ProjectPath))
        {
            containerRelPath = containerRelPath.Replace(Project.ProjectPath, "");
        }

        if (containerRelPath.Contains(Project.DataPath))
        {
            containerRelPath = containerRelPath.Replace(Project.DataPath, "");
        }

        if (containerRelPath.Contains(name))
        {
            containerRelPath = containerRelPath.Replace(name, "");
        }

        //TaskLogs.AddLog(containerRelPath);

        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(Project, containerRelPath);

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
            var fileBytes = File.ReadAllBytes(path);

            DCX.Type compressionType;
            var reader = new BinaryReaderEx(false, fileBytes);
            SFUtil.GetDecompressedBR(reader, out compressionType);

            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            var id = -1;
            var fmg = FMG.Read(path);
            fmg.Name = name; // Assign this to make it easier to grab FMGs

            TextContainerWrapper containerWrapper = new(Project);
            containerWrapper.Filename = name;
            containerWrapper.ReadPath = path;
            containerWrapper.RelativePath = containerRelPath;

            containerWrapper.CompressionType = compressionType;
            containerWrapper.ContainerType = containerType;
            containerWrapper.ContainerDisplayCategory = containerCategory;

            TextFmgWrapper fmgInfo = new();
            fmgInfo.ID = id;
            fmgInfo.Name = name;
            fmgInfo.File = fmg;
            fmgInfo.Parent = containerWrapper;

            fmgWrappers.Add(fmgInfo);

            containerWrapper.FmgWrappers = fmgWrappers;

            if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                containerWrapper.ContainerDisplaySubCategory = TextUtils.GetSubCategory(path);
            }

            Entries.Add(path, containerWrapper);
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to load FMG: {filename} at {path}\n{ex}");
        }
    }

    /// <summary>
    /// Load FMG container
    /// </summary>
    public void LoadFmgContainer(string path)
    {
        var name = Path.GetFileName(path);
        var containerRelPath = path;
        if (containerRelPath.Contains(Project.ProjectPath))
        {
            containerRelPath = containerRelPath.Replace(Project.ProjectPath, "");
        }

        if (containerRelPath.Contains(Project.DataPath))
        {
            containerRelPath = containerRelPath.Replace(Project.DataPath, "");
        }

        if (containerRelPath.Contains(name))
        {
            containerRelPath = containerRelPath.Replace(name, "");
        }

        //TaskLogs.AddLog(containerRelPath);

        var containerType = TextContainerType.BND;
        var containerCategory = TextUtils.GetLanguageCategory(Project, containerRelPath);

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
            // Detect the compression type used by the container
            var fileBytes = File.ReadAllBytes(path);

            DCX.Type compressionType;
            var reader = new BinaryReaderEx(false, fileBytes);
            SFUtil.GetDecompressedBR(reader, out compressionType);

            // Create the Text Container wrapper and and add it to the bank
            TextContainerWrapper containerWrapper = new(Project);
            containerWrapper.Filename = name;
            containerWrapper.ReadPath = path;
            containerWrapper.RelativePath = containerRelPath;

            containerWrapper.CompressionType = compressionType;
            containerWrapper.ContainerType = containerType;
            containerWrapper.ContainerDisplayCategory = containerCategory;

            // Populate the Text Fmg wrappers with their contents
            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                using (IBinder binder = BND3.Read(path))
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
                using (IBinder binder = BND4.Read(path))
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

            Entries.Add(path, containerWrapper);
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to load FMG container: {filename} at {path}\n{ex}");
        }
    }

    /// <summary>
    /// Save all modified FMG Containers
    /// </summary>
    public void SaveTextFiles()
    {
        foreach (var (path, containerInfo) in Entries)
        {
            // Only save all modified files
            if (containerInfo.IsModified)
            {
                if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    SaveLooseFmgs(containerInfo);
                }
                else
                {
                    SaveFmgContainer(containerInfo);
                }
            }
        }
    }

    /// <summary>
    /// Save passed FMG container
    /// </summary>
    public void SaveFmgContainer(TextContainerWrapper containerWrapper)
    {
        var rootContainerPath = containerWrapper.ReadPath;
        var projectContainerPath = containerWrapper.GetWritePath();

        var directory = Path.GetDirectoryName(projectContainerPath);

        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        if (!File.Exists(projectContainerPath))
        {
            File.Copy(rootContainerPath, projectContainerPath, true);
        }

        byte[] fileBytes = null;

        if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES or ProjectType.ACFA)
        {
            using (IBinder binder = BND3.Read(projectContainerPath))
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
            using (IBinder binder = BND4.Read(projectContainerPath))
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

        WriteFile(fileBytes, projectContainerPath);
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
                        TaskLogs.AddLog($"Failed to write FMG file: {file.ID}\n{ex}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saved passed loose FMG file
    /// </summary>
    public void SaveLooseFmgs(TextContainerWrapper containerWrapper)
    {
        var rootContainerPath = containerWrapper.ReadPath;
        var projectContainerPath = containerWrapper.GetWritePath();

        var directory = Path.GetDirectoryName(projectContainerPath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        if (!File.Exists(projectContainerPath))
        {
            File.Copy(rootContainerPath, projectContainerPath, true);
        }

        var newFmgBytes = containerWrapper.FmgWrappers.First().File.Write();

        WriteFile(newFmgBytes, projectContainerPath);
    }

    /// <summary>
    /// Write out a non-container file.
    /// </summary>
    public void WriteFile(byte[] data, string path)
    {
        if (data != null)
        {
            var filename = Path.GetFileNameWithoutExtension(path);

            try
            {
                PathUtils.BackupPrevFile(path);
                PathUtils.BackupFile(path);
                File.WriteAllBytes(path, data);

                TaskLogs.AddLog($"Banks: saved FMG container file: {filename} at {path}");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Banks: failed to save FMG container file: {filename} at {path}\n{ex}");
            }
        }
    }
}



