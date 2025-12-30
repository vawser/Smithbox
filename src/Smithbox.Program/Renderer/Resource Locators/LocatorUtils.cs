using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

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

    public static bool IsHavokNavmesh(string virtPath, string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (virtPath.Contains(@"/nav"))
        {
            if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
            {
                success = true;
            }
        }

        return success;
    }

    public static bool IsHavokCollision(string virtPath, string path)
    {
        string upperPath = path.ToUpper();
        bool success = false;

        if (virtPath.Contains(@"/hit"))
        {
            if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
            {
                success = true;
            }
        }

        if (virtPath.Contains(@"/connect"))
        {
            if (upperPath.EndsWith(".HKX") || upperPath.EndsWith(".HKX.DCX"))
            {
                success = true;
            }
        }

        return success;
    }

    public static string GetAssetPath(ProjectEntry project, string relpath)
    {
        if (project.ProjectPath != null)
        {
            var modpath = Path.Join(project.ProjectPath, relpath);
            if (File.Exists(modpath))
                return modpath;
        }

        return Path.Join(project.DataPath, relpath);
    }

    public static string GetOverridenFilePath(ProjectEntry project, string relpath)
    {
        var rootPath = Path.Join(project.DataPath, relpath);
        var modPath = Path.Join(project.ProjectPath, relpath);

        if (project.ProjectPath != null && File.Exists(modPath))
            return modPath;

        if (File.Exists($@"{rootPath}"))
            return rootPath;

        return null;
    }
}
