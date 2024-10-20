using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Resource.Locators;
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

    public static SortedDictionary<string, TextContainerInfo> FmgBank { get; private set; } = new();

    public static SortedDictionary<string, TextContainerInfo> VanillaFmgBank { get; private set; } = new();

    public static SortedDictionary<string, TextContainerInfo> TargetFmgBank { get; private set; } = new();

    /// <summary>
    /// Load all FMG containers
    /// </summary>
    public static void LoadTextFiles()
    {
        PrimaryBankLoaded = false;
        VanillaBankLoaded = false;

        FmgBank = new();
        VanillaFmgBank = new();

        TaskManager.Run(
            new TaskManager.LiveTask($"Setup Text Editor - Primary Bank", TaskManager.RequeueType.None, false,
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
        }));
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

        TaskManager.Run(
            new TaskManager.LiveTask($"Setup Text Editor - Vanilla Bank", TaskManager.RequeueType.None, false,
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
        }));
    }

    /// <summary>
    /// Load FMG file
    /// </summary>
    public static void LoadFmg(string path, SortedDictionary<string, TextContainerInfo> bank)
    {
        var name = Path.GetFileName(path);
        var relPath = path.Replace(Smithbox.GameRoot, "").Replace(Smithbox.ProjectRoot, "");
        var containerType = TextContainerType.Loose;
        var containerCategory = TextUtils.GetLanguageCategory(relPath);

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

            List<FmgInfo> fmgInfos = new List<FmgInfo>();

            var id = -1;
            var fmg = FMG.Read(path);
            fmg.Name = name; // Assign this to make it easier to grab FMGs

            FmgInfo fmgInfo = new(id, name, fmg);
            fmgInfos.Add(fmgInfo);

            TextContainerInfo containerInfo = new(name, path, compressionType, containerType, containerCategory, fmgInfos);

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                containerInfo.SubCategory = TextUtils.GetSubCategory(path);
            }

            bank.Add(path, containerInfo);
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to load FMG: {ex}");
        }
    }

    /// <summary>
    /// Load FMG container
    /// </summary>
    public static void LoadFmgContainer(string path, SortedDictionary<string, TextContainerInfo> bank)
    {
        var name = Path.GetFileName(path);
        var relPath = path.Replace(Smithbox.GameRoot, "").Replace(Smithbox.ProjectRoot, "");
        var containerType = TextContainerType.BND;
        var containerCategory = TextUtils.GetLanguageCategory(relPath);

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
            // Get compression type
            var fileBytes = File.ReadAllBytes(path);

            DCX.Type compressionType;
            var reader = new BinaryReaderEx(false, fileBytes);
            SFUtil.GetDecompressedBR(reader, out compressionType);

            List<FmgInfo> fmgInfos = new List<FmgInfo>();

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

                            FmgInfo fmgInfo = new(id, fmgName, fmg);
                            fmgInfos.Add(fmgInfo);
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

                            FmgInfo fmgInfo = new(id, fmgName, fmg);
                            fmgInfos.Add(fmgInfo);
                        }
                    }
                }
            }

            TextContainerInfo containerInfo = new(name, path, compressionType, containerType, containerCategory, fmgInfos);

            bank.Add(path, containerInfo);
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to load FMG container: {ex}");
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
    public static void SaveFmgContainer(TextContainerInfo containerInfo)
    {
        var rootContainerPath = containerInfo.AbsolutePath;
        var projectContainerPath = containerInfo.AbsolutePath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

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
                    WriteFmgContents(file, containerInfo.FmgInfos);
                }

                using (BND3 writeBinder = binder as BND3)
                {
                    fileBytes = writeBinder.Write(containerInfo.CompressionType);
                }
            }
        }
        else
        {
            using (IBinder binder = BND4.Read(projectContainerPath))
            {
                foreach (var file in binder.Files)
                {
                    WriteFmgContents(file, containerInfo.FmgInfos);
                }

                using (BND4 writeBinder = binder as BND4)
                {
                    fileBytes = writeBinder.Write(containerInfo.CompressionType);
                }
            }
        }

        WriteFile(fileBytes, projectContainerPath);
    }

    private static void WriteFmgContents(BinderFile file, List<FmgInfo> fmgList)
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
                        TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saved passed loose FMG file
    /// </summary>
    public static void SaveLooseFmgs(TextContainerInfo containerInfo)
    {
        var rootContainerPath = containerInfo.AbsolutePath;
        var projectContainerPath = containerInfo.AbsolutePath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

        var directory = Path.GetDirectoryName(projectContainerPath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        if (!File.Exists(projectContainerPath))
        {
            File.Copy(rootContainerPath, projectContainerPath, true);
        }

        var newFmgBytes = containerInfo.FmgInfos.First().File.Write();

        WriteFile(newFmgBytes, projectContainerPath);
    }

    /// <summary>
    /// Write out a non-container file.
    /// </summary>
    public static void WriteFile(byte[] data, string path)
    {
        if (data != null)
        {
            try
            {
                TextUtils.IsSupportedProjectType
                File.WriteAllBytes(path, data);
                TaskLogs.AddLog($"Saved text at: {path}");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to save text: {path}\n{ex.ToString()}");
            }
        }
    }
}



