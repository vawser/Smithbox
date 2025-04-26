using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Utilities;

public class ProjectUtils
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

    public static string GetLocalProjectFolder(Project curProject)
    {
        var folder = $"{curProject.ProjectPath}/{LocalProjectFolder}/";

        return folder;
    }
    public static string GetGameDirectory(Project curProject)
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
    
    public static bool IsSupportedProjectType(ProjectType type)
    {
        // ERN -- Unreleased
        if (type is ProjectType.ERN)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// A bit hacky, but good enough for now
    /// </summary>
    /// <param name="curProject"></param>
    public static void LaunchMod(Project curProject)
    {
        var modEngineInstallFolderPath = Path.GetDirectoryName(CFG.Current.ModEngineInstall);
        var modTomlPath = @$"{modEngineInstallFolderPath}\smithbox_launch_config.toml";

        var dllInput = "";
        if (CFG.Current.ModEngineDlls != "")
        {
            var dlls = CFG.Current.ModEngineDlls.Split(" ");

            for (int i = 0; i < dlls.Length; i++)
            {
                var curEntry = dlls[i];

                dllInput = $"{dllInput}\"{curEntry}\"";

                if (i != dlls.Length - 1)
                {
                    // Add the comma for all but the last
                    dllInput = $"{dllInput}, ";
                }
            }
        }

        var looseParams = "false";
        if (CFG.Current.UseLooseParams)
            looseParams = "true";

        string tomlString = $@"[modengine]
debug = false
external_dlls = [
    {dllInput}
]

[extension.mod_loader]
enabled = true
loose_params = {looseParams}

mods = [
    {{ enabled = true, name = ""{curProject.ProjectName}"", path = ""{curProject.ProjectPath.Replace("\\", "\\\\")}"" }}
]

[extension.scylla_hide]
enabled = false";

        File.WriteAllText(modTomlPath, tomlString);

        if (File.Exists(modTomlPath))
        {
            var tomlPath = $@"{modEngineInstallFolderPath}\smithbox_launch_config.toml";
            var projectType = $"{curProject.ProjectType}".ToLower();

            var inputStr = $"'-t' '{projectType}' '-c' '{tomlPath}'".Replace("'", "\"");

            bool isRunning = Process.GetProcessesByName("Steam.exe").Any();
            if (isRunning)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = CFG.Current.ModEngineInstall,
                    Arguments = inputStr,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Process.Start(psi);
            }
            else
            {
                MessageBox.Show("Steam is not currently running. Start Steam.");
            }
        }
    }
}
