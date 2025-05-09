using Andre.IO.VFS;
using Octokit;
using SoulsFormats;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

public class ProjectUtils
{
    public static string GetGameDirectory(ProjectEntry curProject)
    {
        return GetGameDirectory(curProject.ProjectType);
    }

    public static string GetGameDirectory(ProjectType curProjectType)
    {
        switch (curProjectType)
        {
            case ProjectType.Undefined:
                return "NONE";
            case ProjectType.DES:
                return "DES";
            case ProjectType.DS1:
                return "DS1";
            case ProjectType.DS1R:
                return "DS1R";
            case ProjectType.DS2:
                return "DS2";
            case ProjectType.DS2S:
                return "DS2S";
            case ProjectType.BB:
                return "BB";
            case ProjectType.DS3:
                return "DS3";
            case ProjectType.SDT:
                return "SDT";
            case ProjectType.ER:
                return "ER";
            case ProjectType.AC6:
                return "AC6";
            default:
                throw new Exception("Game type not set");
        }
    }

    public static string GetBaseFolder()
    {
        return @$"{AppContext.BaseDirectory}\.smithbox";
    }

    public static string GetConfigurationFolder()
    {
        return @$"{AppContext.BaseDirectory}\.smithbox\Configuration";
    }
    public static string GetThemeFolder()
    {
        return @$"{AppContext.BaseDirectory}\Assets\Themes";
    }


    public static string GetProjectsFolder()
    {
        return @$"{AppContext.BaseDirectory}\.smithbox\Projects";
    }
    public static string GetLocalProjectFolder(ProjectEntry project)
    {
        return @$"{project.ProjectPath}\.smithbox\Project";
    }

    public static List<string> GetLooseParamsInDir(VirtualFileSystem fs, string dir)
    {
        List<string> looseParams = new();

        string paramDir = Path.Combine(dir, "Param");
        looseParams.AddRange(fs.GetFileNamesWithExtensions(paramDir, ".param"));

        return looseParams;
    }

    /// <summary>
    /// Build a FileDictionary from a source path
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static FileDictionary BuildFromSource(string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source path not found: {sourcePath}");

        var fileDict = new FileDictionary();
        fileDict.Entries = new();

        var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

        string archiveName = new DirectoryInfo(sourcePath).Name;

        foreach (var filePath in allFiles)
        {
            string relativePath = Path.GetRelativePath(sourcePath, filePath);
            string folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? "";
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath)?.TrimStart('.').ToLower();

            // Special handling: if file ends with .dcx, strip both extensions (e.g., .bnd.dcx → .bnd)
            if (extension == "dcx")
            {
                string noDcx = Path.GetFileNameWithoutExtension(fileName);
                string prevExt = Path.GetExtension(fileName)?.TrimStart('.').ToLower();
                if (!string.IsNullOrEmpty(prevExt))
                {
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                    extension = prevExt;
                }
                else
                {
                    extension = "dcx"; // fallback if no prior extension
                }
            }

            fileDict.Entries.Add(new FileDictionaryEntry
            {
                Archive = archiveName,
                Path = relativePath.Replace('\\', '/'),
                Folder = folder,
                Filename = fileName,
                Extension = extension
            });
        }

        return fileDict;
    }

    public static FileDictionary MergeFileDictionaries(FileDictionary first, FileDictionary second)
    {
        var combined = new FileDictionary();
        combined.Entries = new();

        // Use a HashSet to track unique relative paths (case-insensitive)
        var seenPaths = new HashSet<string>(first.Entries.Select(e => e.Path), System.StringComparer.OrdinalIgnoreCase);

        // Add all entries from the first dictionary
        combined.Entries.AddRange(first.Entries);

        // Add only unique entries from the second dictionary
        foreach (var entry in second.Entries)
        {
            if (!seenPaths.Contains(entry.Path))
            {
                combined.Entries.Add(entry);
                seenPaths.Add(entry.Path);
            }
        }

        return combined;
    }

    /// <summary>
    /// Gets the VirtualFileSystem we should use for writes.
    /// If no suitable VFS is found, throws InvalidOperationException.
    /// </summary>
    /// <returns></returns>
    public static VirtualFileSystem GetFilesystemForWrite(ProjectEntry curProject)
    {
        if (curProject.ProjectFS is not EmptyVirtualFileSystem)
            return curProject.ProjectFS;

        if (curProject.VanillaRealFS is not EmptyVirtualFileSystem)
            return curProject.VanillaRealFS;

        throw new InvalidOperationException("No suitable VFS was found for writes");
    }

    public static void WriteWithBackup<T>(ProjectEntry curProject, string assetPath, T item,
        params object[] writeparms) where T : SoulsFile<T>, new()
    {
        WriteWithBackup(curProject, curProject.FS, curProject.ProjectFS, assetPath, item,
            curProject.ProjectType, writeparms);
    }

    public static void WriteWithBackup<T>(ProjectEntry curProject, VirtualFileSystem vanillaFs, VirtualFileSystem toFs, string assetPath,
        T item, ProjectType gameType = ProjectType.Undefined, params object[] writeparms) where T : SoulsFile<T>, new()
    {
        try
        {
            // Make a backup of the original file if a mod path doesn't exist
            if (toFs != curProject.ProjectFS && !toFs.FileExists($"{assetPath}.bak") && toFs.FileExists(assetPath))
            {
                toFs.Copy(assetPath, $"{assetPath}.bak");
            }

            if (gameType == ProjectType.DS3 && item is BND4 bndDS3)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptDS3Regulation(bndDS3));
            }
            else if (gameType == ProjectType.ER && item is BND4 bndER)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptERRegulation(bndER));
            }
            else if (gameType == ProjectType.ERN && item is BND4 bndERN)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptNightreignRegulation(bndERN));
            }
            else if (gameType == ProjectType.AC6 && item is BND4 bndAC6)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptAC6Regulation(bndAC6));
            }
            else if (item is BXF3 or BXF4)
            {
                var bhdPath = $@"{(string)writeparms[0]}";
                if (item is BXF3 bxf3)
                {
                    bxf3.Write(out var bhd, out var bdt);
                    toFs.WriteFile(bhdPath + ".temp", bhd);
                    toFs.WriteFile(assetPath + ".temp", bdt);

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf3.Dispose();
                }
                else if (item is BXF4 bxf4)
                {
                    bxf4.Write(out var bhd, out var bdt);
                    toFs.WriteFile(bhdPath + ".temp", bhd);
                    toFs.WriteFile(assetPath + ".temp", bdt);

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf4.Dispose();
                }

                if (toFs.FileExists(bhdPath))
                {
                    toFs.Copy(bhdPath, bhdPath + ".prev");
                }
                toFs.Move(bhdPath + ".temp", bhdPath);

                return;
            }
            else
            {
                toFs.WriteFile(assetPath + ".temp", item.Write());
            }

            // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
            if (item is IDisposable d)
            {
                d.Dispose();
            }

            if (toFs.FileExists(assetPath))
            {
                toFs.Copy(assetPath, assetPath + ".prev");
            }
            toFs.Move(assetPath + ".temp", assetPath);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{curProject.ProjectName}] Failed to save: {Path.GetFileName(assetPath)} - {e}");
        }
    }
}
