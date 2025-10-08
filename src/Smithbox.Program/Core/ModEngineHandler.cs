using StudioCore;
using StudioCore.Core;
using StudioCore.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Utilities;
public static class ModEngineHandler
{

    public static bool IsME3Project(ProjectEntry curProject)
    {
        if (curProject.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            return true;
        }

        return false;
    }

    public static void CreateME3Profile(ProjectEntry curProject)
    {
        if (CFG.Current.ModEngine3ProfileDirectory == "")
            return;

        if (!Directory.Exists(CFG.Current.ModEngine3ProfileDirectory))
        {
            TaskLogs.AddLog("The current ME3 profile directory does not exist. Please configure the ME3 profile directory within the settings to a valid directory.");
            return;
        }

        var projectName = $"{curProject.ProjectName}-{curProject.ProjectGUID}";
        var absPath = $"{curProject.ProjectPath}".Replace(@"\", "/"); // Correct backslash to forward slash
        var gametype = "nr";

        if (curProject.ProjectType is ProjectType.ER)
        {
            gametype = "er";
        }

        if (curProject.ProjectType is ProjectType.AC6)
        {
            gametype = "ac6";
        }

        if (curProject.ProjectType is ProjectType.DS3)
        {
            gametype = "ds3";
        }

        if (curProject.ProjectType is ProjectType.SDT)
        {
            gametype = "sdt";
        }

        var profileString = $"profileVersion = \"v1\"\r\nnatives = []\r\n\r\n[[packages]] \r\nid = \"{projectName}\" \r\nsource = \"{absPath}\" \r\n\r\n[[supports]]\r\ngame = \"{gametype}\"\r\n";

        var writePath = $"{CFG.Current.ModEngine3ProfileDirectory}/{projectName}.me3";

        File.WriteAllText(writePath, profileString);
    }

    public static bool ME3ProfileExists(ProjectEntry curProject)
    {
        var projectName = $"{curProject.ProjectName}-{curProject.ProjectGUID}";

        var readPath = $"{CFG.Current.ModEngine3ProfileDirectory}/{projectName}.me3";

        if (File.Exists(readPath))
            return true;

        return false;
    }

    public static void LaunchME3Mod(ProjectEntry curProject)
    {
        if (CFG.Current.ModEngine3ProfileDirectory == "")
            return;

        if (!Directory.Exists(CFG.Current.ModEngine3ProfileDirectory))
            return;

        var projectName = $"{curProject.ProjectName}-{curProject.ProjectGUID}";

        var readPath = $"{CFG.Current.ModEngine3ProfileDirectory}/{projectName}.me3";

        if (File.Exists(readPath))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {projectName}.me3",
                WorkingDirectory = CFG.Current.ModEngine3ProfileDirectory,
                UseShellExecute = false, 
                RedirectStandardOutput = true, 
                RedirectStandardError = true,  
                CreateNoWindow = true          
            };

            Process.Start(startInfo);
        }
        else
        {
            TaskLogs.AddLog($"[Smithbox] Failed to find profile: {readPath}");
        }
    }

    public static bool IsME2Project(ProjectEntry curProject)
    {
        if(curProject.ProjectType is ProjectType.DS3)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// A bit hacky, but good enough for now
    /// </summary>
    /// <param name="curProject"></param>
    public static void LaunchME2Mod(ProjectEntry curProject)
    {
        var modEngineInstallFolderPath = Path.GetDirectoryName(CFG.Current.ModEngine2Install);
        var modTomlPath = @$"{modEngineInstallFolderPath}\smithbox_launch_config.toml";

        var dllInput = "";
        if (CFG.Current.ModEngine2Dlls != "")
        {
            var dlls = CFG.Current.ModEngine2Dlls.Split(" ");

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

        if(File.Exists(modTomlPath))
        {
            var tomlPath = $@"{modEngineInstallFolderPath}\smithbox_launch_config.toml";
            var projectType = $"{curProject.ProjectType}".ToLower();

            var inputStr = $"'-t' '{projectType}' '-c' '{tomlPath}'".Replace("'", "\"");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = CFG.Current.ModEngine2Install,
                Arguments = inputStr,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process.Start(psi);
        }
    }
}
