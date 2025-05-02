using Silk.NET.OpenGL;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public static class TextBank
{
    public static bool PrimaryBankLoaded = false;
    public static bool VanillaBankLoaded = false;
    public static bool VanillaBankLoading = false;

    public static SortedDictionary<string, TextContainerWrapper> FmgBank { get; private set; } = new();

    public static SortedDictionary<string, TextContainerWrapper> VanillaFmgBank { get; private set; } = new();

    public static SortedDictionary<string, TextContainerWrapper> TargetFmgBank { get; private set; } = new();

    /// <summary>
    /// Load all FMG containers
    /// </summary>
    public static void LoadTextFiles()
    {
        PrimaryBankLoaded = false;
        VanillaBankLoaded = false;

        FmgBank = new();
        VanillaFmgBank = new();

        TaskManager.LiveTask task = new(
            "textEditor_primaryBankSetup",
            "Text Editor",
            "primary text bank has been setup.",
            "primary text bank setup has failed.",
            TaskManager.RequeueType.None,
            false,
            () =>
            {
                if (Smithbox.ProjectType is not ProjectType.Undefined)
                {
                    if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                    {
                        var fmgList = TextLocator.GetFmgs("menu\\text\\");

                        foreach (var path in fmgList)
                        {
                            LoadFmg(path, FmgBank);
                        }
                    }
                    else if (Smithbox.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                    {
                        var fmgList = TextLocator.GetFmgs("lang\\");

                        foreach (var path in fmgList)
                        {
                            LoadFmg(path, FmgBank);
                        }
                    }
                    else
                    {
                        var fmgContainerList = TextLocator.GetFmgContainers();

                        foreach (var path in fmgContainerList)
                        {
                            LoadFmgContainer(path, FmgBank);
                        }
                    }
                }

                PrimaryBankLoaded = true;
            }
        );

        TaskManager.Run(task);
    }

    /// <summary>
    /// Load text files for target project
    /// </summary>
    public static void LoadTargetTextFiles(string targetDir)
    {
        TargetFmgBank = new();

        if (Smithbox.ProjectType is not ProjectType.Undefined)
        {
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                var fmgList = TextLocator.GetFmgs("menu\\text\\", false, targetDir);

                foreach (var path in fmgList)
                {
                    LoadFmg(path, TargetFmgBank);
                }
            }
            else if (Smithbox.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
            {
                var fmgList = TextLocator.GetFmgs("lang\\", false, targetDir);

                foreach (var path in fmgList)
                {
                    LoadFmg(path, TargetFmgBank);
                }
            }
            else
            {
                var fmgContainerList = TextLocator.GetFmgContainers(false, targetDir);

                foreach (var path in fmgContainerList)
                {
                    LoadFmgContainer(path, TargetFmgBank);
                }
            }
        }
    }

    /// <summary>
    /// Load all vanilla FMG containers
    /// </summary>
    public static void LoadVanillaTextFiles()
    {
        VanillaBankLoading = true;

        // Skip if disabled
        if (!CFG.Current.TextEditor_IncludeVanillaCache)
        {
            VanillaBankLoaded = true;
            VanillaBankLoading = false;

            return;
        }

        TaskManager.LiveTask task = new(
            "textEditor_vanillaBankSetup",
            "Text Editor",
            "vanilla text bank has been setup.",
            "vanilla text bank setup has failed.",
            TaskManager.RequeueType.None,
            false,
            () =>
            {
                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    var fmgList = TextLocator.GetFmgs("menu\\text\\", true);

                    foreach (var path in fmgList)
                    {
                        LoadFmg(path, VanillaFmgBank);
                    }
                }
                else if (Smithbox.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
                {
                    var fmgList = TextLocator.GetFmgs("lang\\", true);

                    foreach (var path in fmgList)
                    {
                        LoadFmg(path, VanillaFmgBank);
                    }
                }
                else if (Smithbox.ProjectType is not ProjectType.Undefined)
                {
                    var fmgContainerList = TextLocator.GetFmgContainers(true);

                    foreach (var path in fmgContainerList)
                    {
                        LoadFmgContainer(path, VanillaFmgBank);
                    }
                }

                VanillaBankLoaded = true;
                VanillaBankLoading = false;
            }
        );

        TaskManager.Run(task);
    }

    /// <summary>
    /// Load FMG file
    /// </summary>
    public static void LoadFmg(string path, SortedDictionary<string, TextContainerWrapper> bank)
    {
        var name = Path.GetFileName(path);

        // TODO: need a better method for building the relative path, this seems to cause issues with some people's setups, causing a crash as the resulting relative path is empty
        var containerRelPath = path;
        if (containerRelPath.Contains(Smithbox.ProjectRoot))
        {
            containerRelPath = containerRelPath.Replace(Smithbox.ProjectRoot, "");
        }
        if (containerRelPath.Contains(Smithbox.GameRoot))
        {
            containerRelPath = containerRelPath.Replace(Smithbox.GameRoot, "");
        }
        if (containerRelPath.Contains(name))
        {
            containerRelPath = containerRelPath.Replace(name, "");
        }

        //TaskLogs.AddLog(containerRelPath);

        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(containerRelPath);

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

            TextContainerWrapper containerWrapper = new();
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

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                containerWrapper.ContainerDisplaySubCategory = TextUtils.GetSubCategory(path);
            }

            bank.Add(path, containerWrapper);
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
    public static void LoadFmgContainer(string path, SortedDictionary<string, TextContainerWrapper> bank)
    {
        var name = Path.GetFileName(path);
        var containerRelPath = path;
        if (containerRelPath.Contains(Smithbox.ProjectRoot))
        {
            containerRelPath = containerRelPath.Replace(Smithbox.ProjectRoot, "");
        }
        if (containerRelPath.Contains(Smithbox.GameRoot))
        {
            containerRelPath = containerRelPath.Replace(Smithbox.GameRoot, "");
        }
        if (containerRelPath.Contains(name))
        {
            containerRelPath = containerRelPath.Replace(name, "");
        }

        //TaskLogs.AddLog(containerRelPath);

        var containerType = TextContainerType.BND;
        var containerCategory = TextUtils.GetLanguageCategory(containerRelPath);

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
            TextContainerWrapper containerWrapper = new();
            containerWrapper.Filename = name;
            containerWrapper.ReadPath = path;
            containerWrapper.RelativePath = containerRelPath;

            containerWrapper.CompressionType = compressionType;
            containerWrapper.ContainerType = containerType;
            containerWrapper.ContainerDisplayCategory = containerCategory;

            // Populate the Text Fmg wrappers with their contents
            List<TextFmgWrapper> fmgWrappers = new List<TextFmgWrapper>();

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
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

            bank.Add(path, containerWrapper);
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
    public static void SaveTextFiles()
    {
        foreach (var (path, containerInfo) in FmgBank)
        {
            // Only save all modified files
            if (containerInfo.IsModified)
            {
                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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
    public static void SaveFmgContainer(TextContainerWrapper containerWrapper)
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

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES or ProjectType.ACFA)
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

    private static void WriteFmgContents(BinderFile file, List<TextFmgWrapper> fmgList)
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
    public static void SaveLooseFmgs(TextContainerWrapper containerWrapper)
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
    public static void WriteFile(byte[] data, string path)
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



