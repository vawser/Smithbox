using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public static class SteamGameLocator
{
    public static string GetSteamInstallPath()
    {
        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\WOW6432Node\\Valve\\Steam"))
        {
            if (key != null)
            {
                return key.GetValue("InstallPath") as string;
            }
        }
        return null;
    }

    public static List<string> GetSteamLibraryFolders(string steamPath)
    {
        var folders = new List<string> { steamPath };
        string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

        if (File.Exists(vdfPath))
        {
            foreach (var line in File.ReadAllLines(vdfPath))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("\"path\""))
                {
                    var parts = trimmed.Split('"');
                    if (parts.Length >= 4)
                    {
                        var path = parts[3].Replace("\\\\", "\\");
                        folders.Add(path);
                    }
                }
            }
        }
        return folders;
    }

    public static string GetGameInstallDir(string libraryPath, int appId)
    {
        string acfPath = Path.Combine(libraryPath, "steamapps", $"appmanifest_{appId}.acf");
        if (!File.Exists(acfPath))
            return null;

        foreach (var line in File.ReadAllLines(acfPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("\"installdir\""))
            {
                var parts = trimmed.Split('"');
                if (parts.Length >= 4)
                {
                    return parts[3];
                }
            }
        }
        return null;
    }

    public static string FindGameExecutable(int appId, string executableRelativePath)
    {
        string steamPath = GetSteamInstallPath();
        if (steamPath == null)
            throw new Exception("Steam install path not found in registry.");

        var libraries = GetSteamLibraryFolders(steamPath);
        foreach (var library in libraries)
        {
            string installdir = GetGameInstallDir(library, appId);
            if (installdir != null)
            {
                string gameFolder = Path.Combine(library, "steamapps", "common", installdir);
                string exePath = Path.Combine(gameFolder, executableRelativePath);
                if (File.Exists(exePath))
                {
                    // Only return the directory
                    return Path.GetDirectoryName(exePath);
                }
            }
        }

        return null;
    }
}