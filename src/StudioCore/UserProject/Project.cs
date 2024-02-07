using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.UserProject;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public static class Project
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

    /// <summary>
    /// Return the ProjectType as a string for a filepath.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string GetGameIDForDir()
    {
        switch (Project.Type)
        {
            case ProjectType.DES:
                return "DES";
            case ProjectType.DS1:
                return "DS1";
            case ProjectType.DS1R:
                return "DS1R";
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

    /// <summary>
    /// Get a ProjectType based on the executable name.
    /// </summary>
    /// <param name="exePath"></param>
    /// <returns></returns>
    public static ProjectType GetProjectTypeFromExecutable(string exePath)
    {
        var type = ProjectType.Undefined;

        if (exePath.ToLower().Contains("darksouls.exe"))
        {
            type = ProjectType.DS1;
        }
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
        {
            type = ProjectType.DS1R;
        }
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
        {
            type = ProjectType.DS2S;
        }
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
        {
            type = ProjectType.DS3;
        }
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
            {
                type = ProjectType.BB;
            }
            else
            {
                type = ProjectType.DES;
            }
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
        {
            type = ProjectType.SDT;
        }
        else if (exePath.ToLower().Contains("eldenring.exe"))
        {
            type = ProjectType.ER;
        }
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
        {
            type = ProjectType.AC6;
        }

        return type;
    }

    /// <summary>
    /// Create a recovery project folder.
    /// </summary>
    /// <returns></returns>
    public static bool CreateRecoveryProject()
    {
        if (GameRootDirectory == null || GameModDirectory == null)
            return false;

        try
        {
            var time = DateTime.Now.ToString("dd-MM-yyyy-(hh-mm-ss)", CultureInfo.InvariantCulture);

            GameModDirectory = GameModDirectory + $@"\recovery\{time}";

            if (!Directory.Exists(GameModDirectory))
            {
                Directory.CreateDirectory(GameModDirectory);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
