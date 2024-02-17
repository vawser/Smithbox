using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.AssetLocator;

public static class LocatorUtils
{
    public static List<string> GetAssetFiles(string paramDir, string paramExt)
    {
        try
        {
            HashSet<string> fileList = new();
            List<string> ret = new();

            var paramFiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + paramDir, $@"*{paramExt}")
                .ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileList.Add(name);
            }

            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Project.GameModDirectory + paramDir, $@"*{paramExt}").ToList();
                foreach (var f in paramFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!fileList.Contains(name))
                    {
                        ret.Add(name);
                        fileList.Add(name);
                    }
                }
            }

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static string GetAssetPath(string relpath)
    {
        if (Project.GameModDirectory != null)
        {
            var modpath = $@"{Project.GameModDirectory}\{relpath}";
            if (File.Exists(modpath))
                return modpath;
        }

        return $@"{Project.GameRootDirectory}\{relpath}";
    }

    public static bool CheckFilesExpanded(string gamepath, ProjectType game)
    {
        if (game == ProjectType.ER)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\asset"))
                return false;
        }

        if (game is ProjectType.DS1 or ProjectType.DS3 or ProjectType.SDT)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\obj"))
                return false;
        }

        if (game == ProjectType.DS2S)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\model\obj"))
                return false;
        }

        if (game == ProjectType.AC6)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\asset"))
                return false;
        }

        return true;
    }

    public static bool FileExists(string relpath)
    {
        if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{relpath}"))
            return true;

        if (File.Exists($@"{Project.GameRootDirectory}\{relpath}"))
            return true;

        return false;
    }

    public static string GetOverridenFilePath(string relpath)
    {
        if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{relpath}"))
            return $@"{Project.GameModDirectory}\{relpath}";

        if (File.Exists($@"{Project.GameRootDirectory}\{relpath}"))
            return $@"{Project.GameRootDirectory}\{relpath}";

        return null;
    }

    public static string GetBinderVirtualPath(string virtualPathToBinder, string binderFilePath)
    {
        var filename = Path.GetFileNameWithoutExtension($@"{binderFilePath}.blah");
        if (filename.Length > 0)
            filename = $@"{virtualPathToBinder}/{filename}";
        else
            filename = virtualPathToBinder;

        return filename;
    }
}
