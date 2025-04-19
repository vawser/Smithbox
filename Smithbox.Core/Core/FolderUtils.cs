using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Core;

public class FolderUtils
{
    public const string DataFolder = ".smithbox";
    public const string ConfigurationFolder = ".smithbox/Configuration";
    public const string ProjectFolder = ".smithbox/Projects";
    public const string LocalProjectFolder = ".smithbox/Project/";

    public static void SetupFolders()
    {
        var folder = $"{AppContext.BaseDirectory}/{DataFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = $"{AppContext.BaseDirectory}/{ConfigurationFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = $"{AppContext.BaseDirectory}/{ProjectFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    public static string GetConfigurationFolder()
    {
        var folder = $"{AppContext.BaseDirectory}/{ConfigurationFolder}/";

        return folder;
    }

    public static string GetProjectFolder()
    {
        var folder = $"{AppContext.BaseDirectory}/{ProjectFolder}/";

        return folder;
    }

    public static List<string> GetStoredProjectJsonList()
    {
        var projectJsonList = new List<string>();
        var projectFolder = GetProjectFolder();

        projectJsonList = Directory.EnumerateFiles(projectFolder, "*.json").ToList();

        return projectJsonList;
    }

    public static string GetLocalProjectFolder(ProjectInstance curProject)
    {
        var folder = $"{curProject.ProjectPath}/{LocalProjectFolder}/";

        return folder;
    }
}
