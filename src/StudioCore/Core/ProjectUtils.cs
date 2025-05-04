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

    public static string GetProjectsFolder()
    {
        return @$"{AppContext.BaseDirectory}\.smithbox\Projects";
    }

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
}
