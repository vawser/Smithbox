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
    public static List<string> GetFmgContainers(bool rootOnly = false)
    {
        FileList = new();

        var rootPath = $"{Smithbox.GameRoot}\\msg\\";
        var filePattern = $".msgbnd";

        if (Path.Exists(rootPath))
        {
            SearchFolder(rootPath, filePattern, rootOnly);
        }

        return FileList;
    }

    /// <summary>
    /// Get all fmg paths (for DS2)
    /// </summary>
    public static List<string> GetFmgs(bool rootOnly = false)
    {
        FileList = new();

        var rootPath = $"{Smithbox.GameRoot}\\menu\\text\\";
        var filePattern = $".fmg";

        if (Path.Exists(rootPath))
        {
            SearchFolder(rootPath, filePattern, rootOnly);
        }
        return FileList;
    }

    private static void SearchFolder(string rootPath, string filePattern, bool rootOnly)
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

    public static string GetFmgWrapperDirectory()
    {
        return $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\FMG Wrappers";
    }

    public static List<string> GetFmgWrappers()
    {
        List<string> results = new();

        foreach (var entry in Directory.GetFiles(GetFmgWrapperDirectory()))
        {

        }

        return results;
    }
}
