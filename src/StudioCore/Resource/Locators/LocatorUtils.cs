using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Resource.Locators;

public static class LocatorUtils
{
    public static bool IsTPF(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".TPF") || upperPath.EndsWith(".TPF.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsFLVER(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".FLVER") || upperPath.EndsWith(".FLV") || upperPath.EndsWith(".FLVER.DCX") || upperPath.EndsWith(".FLV.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsNavmesh(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".NVM"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsHavokNavmesh(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static bool IsHavokCollision(string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
        {
            success = true;
        }

        return success;
    }

    public static List<string> GetAssetFiles(ProjectEntry project, string paramDir, string paramExt, bool ignoreProject = false)
    {
        try
        {
            HashSet<string> fileList = new();
            List<string> ret = new();

            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(project.DataPath + paramDir, $@"*{paramExt}")
                .ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileList.Add(name);
            }

            // PROJECT
            if (!ignoreProject)
            {
                if (project.ProjectPath != null && Directory.Exists(project.ProjectPath + paramDir))
                {
                    paramFiles = Directory.GetFileSystemEntries(project.ProjectPath + paramDir, $@"*{paramExt}").ToList();
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
            }

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            TaskLogs.AddLog($"[Smithbox] Failed to find directory: {paramDir}", LogLevel.Error, Tasks.LogPriority.High, e);

            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static string GetAssetPath(ProjectEntry project, string relpath)
    {
        if (project.ProjectPath != null)
        {
            var modpath = $@"{project.ProjectPath}\{relpath}";
            if (File.Exists(modpath))
                return modpath;
        }

        return $@"{project.DataPath}\{relpath}";
    }
    public static string GetAssetPath_CollisionHack(string relpath)
    {
        return $@"{CFG.Current.PTDE_Collision_Root}\{relpath}";
    }

    public static bool CheckFilesExpanded(string gamepath, ProjectType game)
    {
        if (game is ProjectType.ER or ProjectType.AC6)
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

        if (game is ProjectType.DS2S or ProjectType.DS2)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\model\obj"))
                return false;
        }

        if (game is ProjectType.ACV or ProjectType.ACVD)
        {
            if (!Directory.Exists($@"{gamepath}\model\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\model\obj"))
                return false;
        }

        return true;
    }

    public static bool FileExists(ProjectEntry project, string relpath)
    {
        if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{relpath}"))
            return true;

        if (File.Exists($@"{project.DataPath}\{relpath}"))
            return true;

        return false;
    }

    public static string GetOverridenFilePath(ProjectEntry project, string relpath)
    {
        var rootPath = $@"{project.DataPath}\{relpath}";
        var modPath = $@"{project.ProjectPath}\{relpath}";

        if (project.ProjectPath != null && File.Exists(modPath))
            return modPath;

        if (File.Exists($@"{rootPath}"))
            return rootPath;

        return null;
    }




}
