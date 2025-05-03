using StudioCore.Core;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Resource.Locators;

public static class TextLocator
{
    public static List<string> FileList = new();

    /// <summary>
    /// Get all msgbnd paths
    /// </summary>
    public static List<string> GetFmgContainers(ProjectEntry project, bool rootOnly = false, string targetDir = "")
    {
        FileList = new();

        if(targetDir == "")
        {
            targetDir = project.DataPath;
        }

        var rootPath = $"{targetDir}\\msg\\";
        var filePattern = $".msgbnd";

        if (Directory.Exists(rootPath))
        {
            SearchFolder(project, rootPath, filePattern, rootOnly);
        }

        return FileList;
    }

    /// <summary>
    /// Get all fmg paths (for DS2, ACFA, ACV, and ACVD)
    /// </summary>
    public static List<string> GetFmgs(ProjectEntry project, string dir, bool rootOnly = false, string targetDir = "")
    {
        FileList = new();

        if (targetDir == "")
        {
            targetDir = project.DataPath;
        }

        var rootPath = $"{targetDir}\\{dir}";
        var filePattern = $".fmg";

        if (Directory.Exists(rootPath))
        {
            SearchFolder(project, rootPath, filePattern, rootOnly);
        }
        return FileList;
    }

    public static void SearchFolder(ProjectEntry project, string rootPath, string filePattern, bool rootOnly)
    {
        var entries = Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories);
        foreach (var entry in entries)
        {
            if (entry.EndsWith(filePattern) || entry.EndsWith($"{filePattern}.dcx"))
            {
                AddFile(project, entry, rootOnly);
            }
        }
    }

    private static void AddFile(ProjectEntry project, string rootPath, bool rootOnly)
    {
        var path = rootPath;

        if (!rootOnly)
        {
            var projectPath = rootPath.Replace(project.DataPath, project.ProjectPath);

            if (File.Exists(projectPath))
            {
                path = projectPath;
            }
        }

        FileList.Add(path);
    }

    public static bool IsDirectory(string path)
    {
        FileAttributes attr = File.GetAttributes(path);

        if (attr.HasFlag(FileAttributes.Directory))
            return true;

        return false;
    }

    public static string GetStoredTextDirectory(ProjectEntry project)
    {
        return $"{project.ProjectPath}\\.smithbox\\Workflow\\Exported Text";
    }

    public static List<string> GetStoredContainerWrappers(ProjectEntry project)
    {
        List<string> results = new();

        var wrapperDir = GetStoredTextDirectory(project);

        if (Directory.Exists(wrapperDir))
        {
            foreach (var entry in Directory.GetFiles(wrapperDir))
            {
                results.Add(entry);
            }
        }

        return results;
    }
}
