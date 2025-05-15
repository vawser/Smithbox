using StudioCore;
using StudioCore.Core;
using StudioCore.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Utilities;
public static class ModEngineUtils
{
    /// <summary>
    /// A bit hacky, but good enough for now
    /// </summary>
    /// <param name="curProject"></param>
    public static void LaunchMod(ProjectEntry curProject)
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

        if(File.Exists(modTomlPath))
        {
            var tomlPath = $@"{modEngineInstallFolderPath}\smithbox_launch_config.toml";
            var projectType = $"{curProject.ProjectType}".ToLower();

            var inputStr = $"'-t' '{projectType}' '-c' '{tomlPath}'".Replace("'", "\"");

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
    }
}
