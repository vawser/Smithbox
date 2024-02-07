using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.ProjectCore;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public static class UserProject
{
    public static ProjectType Type { get; set; } = ProjectType.Undefined;

    /// <summary>
    /// The game interroot where all the game assets are
    /// </summary>
    public static string GameRootDirectory { get; set; }

    /// <summary>
    /// An optional override mod directory where modded files are stored
    /// </summary>
    public static string GameModDirectory { get; set; }

    /// <summary>
    /// Directory where misc Smithbox files associated with a project are stored.
    /// </summary>
    public static string ProjectDataDir => @$"{GameModDirectory}\.smithbox";

    public static void SetModProjectDirectory(string dir)
    {
        GameModDirectory = dir;
    }

    public static void SetFromProjectSettings(ProjectSettings settings, string moddir)
    {
        Type = settings.GameType;
        GameRootDirectory = settings.GameRoot;
        GameModDirectory = moddir;
    }

    public static ProjectType GetGameTypeForExePath(string exePath)
    {
        var type = ProjectType.Undefined;
        if (exePath.ToLower().Contains("darksouls.exe"))
            type = ProjectType.DS1;
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
            type = ProjectType.DS1R;
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
            type = ProjectType.DS2S;
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
            type = ProjectType.DS3;
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
                type = ProjectType.BB;
            else
                type = ProjectType.DES;
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
            type = ProjectType.SDT;
        else if (exePath.ToLower().Contains("eldenring.exe"))
            type = ProjectType.ER;
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
            type = ProjectType.AC6;

        return type;
    }

    public static bool CreateRecoveryProject()
    {
        if (GameRootDirectory == null || GameModDirectory == null)
            return false;

        try
        {
            var time = DateTime.Now.ToString("dd-MM-yyyy-(hh-mm-ss)", CultureInfo.InvariantCulture);
            GameModDirectory = GameModDirectory + $@"\recovery\{time}";
            if (!Directory.Exists(GameModDirectory))
                Directory.CreateDirectory(GameModDirectory);

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
