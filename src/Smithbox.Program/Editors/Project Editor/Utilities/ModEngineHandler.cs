using StudioCore.Utilities;
using System.Diagnostics;
using System.IO;

namespace StudioCore.Application;

public static class ModEngineHandler
{

    public static bool IsME3Project(ProjectEntry curProject)
    {
        if (curProject.Descriptor == null)
            return false;

        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// The profile name for a project: its name, made safe for a file name, followed by its GUID.
    /// </summary>
    private static string GetProfileName(ProjectEntry curProject)
    {
        var name = curProject.Descriptor.ProjectName.Trim();

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return $"{name}-{curProject.Descriptor.ProjectGUID}";
    }

    /// <summary>
    /// Finds the project's profile in the profile directory. Matched by the GUID suffix, so a profile made before
    /// the project was renamed is still found.
    /// </summary>
    /// <returns>The profile's path, or null if there is none.</returns>
    public static string FindME3Profile(ProjectEntry curProject)
    {
        var profileDir = CFG.Current.Project_ME3_Profile_Directory;

        if (string.IsNullOrEmpty(profileDir) || !Directory.Exists(profileDir))
            return null;

        var exactPath = Path.Combine(profileDir, $"{GetProfileName(curProject)}.me3");

        if (File.Exists(exactPath))
            return exactPath;

        try
        {
            return Directory.EnumerateFiles(profileDir, $"*-{curProject.Descriptor.ProjectGUID}.me3").FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void CreateME3Profile(ProjectEntry curProject)
    {
        if (CFG.Current.Project_ME3_Profile_Directory == "")
            return;

        if (!Directory.Exists(CFG.Current.Project_ME3_Profile_Directory))
        {
            Smithbox.Log(typeof(ModEngineHandler),
                LOC.Get("PROJECT_Util_ME3_Profile_Missing"));
            return;
        }

        var projectName = GetProfileName(curProject);
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

        var writePath = Path.Combine(CFG.Current.Project_ME3_Profile_Directory, $"{projectName}.me3");

        try
        {
            File.WriteAllText(writePath, profileString);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ModEngineHandler), LOC.Get("PROJECT_Util_ME3_Create_Profile_FAIL", writePath), e);
        }
    }

    // ME3ProfileExists is called every frame by the configuration menu, so the directory search is cached briefly
    private static Guid _cachedProfileGuid;
    private static bool _cachedProfileExists;
    private static DateTime _cachedProfileTime;

    public static bool ME3ProfileExists(ProjectEntry curProject)
    {
        var now = DateTime.UtcNow;

        if (_cachedProfileGuid != curProject.Descriptor.ProjectGUID || now - _cachedProfileTime > TimeSpan.FromSeconds(1))
        {
            _cachedProfileGuid = curProject.Descriptor.ProjectGUID;
            _cachedProfileExists = FindME3Profile(curProject) != null;
            _cachedProfileTime = now;
        }

        return _cachedProfileExists;
    }

    public static void LaunchME3Mod(ProjectEntry curProject)
    {
        if (CFG.Current.Project_ME3_Profile_Directory == "")
            return;

        if (!Directory.Exists(CFG.Current.Project_ME3_Profile_Directory))
            return;

        var targetFile = FindME3Profile(curProject);

        if (targetFile != null)
        {
            // Output isn't redirected: nothing reads it, and a full pipe would stall ME3
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = CFG.Current.Project_ME3_Profile_Directory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add(Path.GetFileName(targetFile));

            try
            {
                using var process = Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(ModEngineHandler), LOC.Get("PROJECT_Util_ME3_Launch_FAIL", targetFile), e);
            }
        }
        else
        {
            Smithbox.Log(typeof(ModEngineHandler), LOC.Get("PROJECT_Util_ME3_Find_Profile_FAIL", CFG.Current.Project_ME3_Profile_Directory));
        }
    }
}
