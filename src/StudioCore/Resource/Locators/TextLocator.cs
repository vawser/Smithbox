using Octokit;
using StudioCore.Core.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;

public static class TextLocator
{
    public static List<string> FileList = new();

    /// <summary>
    /// Get all msgbnd paths
    /// </summary>
    public static List<string> GetFmgContainers(bool rootOnly = false, string targetDir = "")
    {
        FileList = new();

        if(targetDir == "")
        {
            targetDir = Smithbox.GameRoot;
        }

        var rootPath = $"{targetDir}\\msg\\";
        var filePattern = $".msgbnd";

        if (Directory.Exists(rootPath))
        {
            SearchFolder(rootPath, filePattern, rootOnly);
        }

        return FileList;
    }

    /// <summary>
    /// Get all fmg paths (for DS2)
    /// </summary>
    public static List<string> GetFmgs(bool rootOnly = false, string targetDir = "")
    {
        FileList = new();

        if (targetDir == "")
        {
            targetDir = Smithbox.GameRoot;
        }

        var rootPath = $"{targetDir}\\menu\\text\\";
        var filePattern = $".fmg";

        if (Directory.Exists(rootPath))
        {
            SearchFolder(rootPath, filePattern, rootOnly);
        }
        return FileList;
    }

    public static void SearchFolder(string rootPath, string filePattern, bool rootOnly)
    {
        var entries = Directory.GetFileSystemEntries(rootPath);

        foreach (var entry in entries)
        {
            if (IsDirectory(entry))
            {
                SearchFolder(entry, filePattern, rootOnly);
            }
            else
            {
                if (entry.EndsWith(filePattern) || entry.EndsWith($"{filePattern}.dcx"))
                {
                    AddFile(entry, rootOnly);
                }
            }
        }
    }

    private static void AddFile(string rootPath, bool rootOnly)
    {
        var path = rootPath;

        if (!rootOnly)
        {
            var projectPath = rootPath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

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

    public static string GetStoredTextDirectory()
    {
        return $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\Exported Text";
    }

    public static List<string> GetStoredContainerWrappers()
    {
        List<string> results = new();

        var wrapperDir = GetStoredTextDirectory();

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
