using StudioCore.Utilities;
using System.Diagnostics;
using System.IO;

namespace StudioCore.Application;

public static class ModEngineHandler
{

    public static bool IsME3Project(ProjectEntry curProject)
    {
        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            return true;
        }

        return false;
    }

    public static void CreateME3Profile(ProjectEntry curProject)
    {
        if (CFG.Current.Project_ME3_Profile_Directory == "")
            return;

        if (!Directory.Exists(CFG.Current.Project_ME3_Profile_Directory))
        {
            TaskLogs.AddLog("The current ME3 profile directory does not exist. Please configure the ME3 profile directory within the settings to a valid directory.");
            return;
        }

        var projectName = $"{curProject.Descriptor.ProjectName}-{curProject.Descriptor.ProjectGUID}";
        var absPath = $"{curProject.Descriptor.ProjectPath}".Replace(@"\", "/"); // Correct backslash to forward slash
        var gametype = "nr";

        if (curProject.Descriptor.ProjectType is ProjectType.ER)
        {
            gametype = "er";
        }

        if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            gametype = "ac6";
        }

        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            gametype = "ds3";
        }

        if (curProject.Descriptor.ProjectType is ProjectType.SDT)
        {
            gametype = "sdt";
        }

        var profileString = $"profileVersion = \"v1\"\r\nnatives = []\r\n\r\n[[packages]] \r\nid = \"{projectName}\" \r\nsource = \"{absPath}\" \r\n\r\n[[supports]]\r\ngame = \"{gametype}\"\r\n";

        var writePath = $"{CFG.Current.Project_ME3_Profile_Directory}/{projectName}.me3";

        File.WriteAllText(writePath, profileString);
    }

    public static bool ME3ProfileExists(ProjectEntry curProject)
    {
        var projectName = $"{curProject.Descriptor.ProjectName}-{curProject.Descriptor.ProjectGUID}";

        var readPath = $"{CFG.Current.Project_ME3_Profile_Directory}/{projectName}.me3";

        if (File.Exists(readPath))
            return true;

        return false;
    }

    public static void LaunchME3Mod(ProjectEntry curProject)
    {
        if (CFG.Current.Project_ME3_Profile_Directory == "")
            return;

        if (!Directory.Exists(CFG.Current.Project_ME3_Profile_Directory))
            return;

        var projectName = $"{curProject.Descriptor.ProjectName}-{curProject.Descriptor.ProjectGUID}";

        var readPath = CFG.Current.Project_ME3_Profile_Directory;

        var targetFile = Path.Combine(readPath, $"{projectName}.me3");

        if (File.Exists(targetFile))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {projectName}.me3",
                WorkingDirectory = CFG.Current.Project_ME3_Profile_Directory,
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
}
