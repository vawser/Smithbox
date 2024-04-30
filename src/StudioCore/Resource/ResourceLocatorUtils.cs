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

namespace StudioCore.Resource;

public static class ResourceLocatorUtils
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

        if (game == ProjectType.DS2S)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
                return false;

            if (!Directory.Exists($@"{gamepath}\model\obj"))
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
        var rootPath = $@"{Project.GameRootDirectory}\{relpath}";
        var modPath = $@"{Project.GameModDirectory}\{relpath}";

        if (Project.GameModDirectory != null && File.Exists(modPath))
            return modPath;

        if (File.Exists($@"{rootPath}"))
            return rootPath;

        return null;
    }

    /// <summary>
    ///     Converts a virtual path to an actual filesystem path. Only resolves virtual paths up to the bnd level,
    ///     which the remaining string is output for additional handling
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <returns></returns>
    public static string VirtualToRealPath(string virtualPath, out string bndpath)
    {
        var pathElements = virtualPath.Split('/');
        Regex mapRegex = new(@"^m\d{2}_\d{2}_\d{2}_\d{2}$");
        var ret = "";

        // Parse the virtual path with a DFA and convert it to a game path
        var i = 0;

        if (pathElements[i].Equals("map"))
        {
            i++;
            if (pathElements[i].Equals("tex"))
            {
                i++;
                if (Project.Type == ProjectType.DS2S)
                {
                    var mid = pathElements[i];
                    i++;
                    var id = pathElements[i];
                    if (id == "tex")
                    {
                        bndpath = "";
                        return GetAssetPath($@"model\map\t{mid.Substring(1)}.tpfbhd");
                    }
                }
                else if (Project.Type == ProjectType.DES)
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    return GetAssetPath($@"map\{mid}\{mid}_{pathElements[i]}.tpf.dcx");
                }
                else
                {
                    var mid = pathElements[i];
                    i++;
                    bndpath = "";
                    if (pathElements[i] == "env")
                    {
                        if (Project.Type == ProjectType.DS1R)
                            return GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpf.dcx");

                        return GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                    }

                    return GetAssetPath($@"map\{mid}\{mid}_{pathElements[i]}.tpfbhd");
                }
            }
            else if (mapRegex.IsMatch(pathElements[i]))
            {
                var mapid = pathElements[i];
                i++;
                if (pathElements[i].Equals("model"))
                {
                    i++;
                    bndpath = "";
                    if (Project.Type == ProjectType.DS1)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver");

                    if (Project.Type == ProjectType.DS1R)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Project.Type == ProjectType.DS2S)
                        return GetAssetPath($@"model\map\{mapid}.mapbhd");

                    if (Project.Type == ProjectType.BB || Project.Type == ProjectType.DES)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (Project.Type is ProjectType.ER or ProjectType.AC6)
                        return GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");

                    return GetAssetPath($@"map\{mapid}\{pathElements[i]}.mapbnd.dcx");
                }

                if (pathElements[i].Equals("hit"))
                {
                    i++;
                    var hittype = pathElements[i];
                    i++;
                    if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES)
                    {
                        bndpath = "";
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}");
                    }

                    if (Project.Type == ProjectType.DS2S)
                    {
                        bndpath = "";
                        return GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.BB)
                    {
                        bndpath = "";
                        if (hittype == "lo")
                            return GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");

                        return GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                    }

                    bndpath = "";
                    return null;
                }

                if (pathElements[i].Equals("nav"))
                {
                    i++;
                    if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES ||
                        Project.Type == ProjectType.DS1R)
                    {
                        if (i < pathElements.Length)
                            bndpath = $@"{pathElements[i]}";
                        else
                            bndpath = "";

                        if (Project.Type == ProjectType.DS1R)
                            return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd.dcx");

                        return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd");
                    }

                    if (Project.Type == ProjectType.DS3)
                    {
                        bndpath = "";
                        return GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
                    }

                    bndpath = "";
                    return null;
                }
            }
        }
        else if (pathElements[i].Equals("chr"))
        {
            i++;
            var chrid = pathElements[i];
            i++;
            if (pathElements[i].Equals("model"))
            {
                bndpath = "";
                if (Project.Type == ProjectType.DS1)
                    return GetOverridenFilePath($@"chr\{chrid}.chrbnd");

                if (Project.Type == ProjectType.DS2S)
                    return GetOverridenFilePath($@"model\chr\{chrid}.bnd");

                if (Project.Type == ProjectType.DES)
                    return GetOverridenFilePath($@"chr\{chrid}\{chrid}.chrbnd.dcx");

                return GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
            }

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetChrTexturePath(chrid);
            }
        }
        else if (pathElements[i].Equals("obj"))
        {
            i++;
            var objid = pathElements[i];
            i++;
            if (pathElements[i].Equals("model") || pathElements[i].Equals("tex"))
            {
                bndpath = "";
                if (Project.Type == ProjectType.DS1)
                    return GetOverridenFilePath($@"obj\{objid}.objbnd");

                if (Project.Type == ProjectType.DS2S)
                    return GetOverridenFilePath($@"model\obj\{objid}.bnd");

                if (Project.Type == ProjectType.ER)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                        return GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{objid}.geombnd.dcx");

                    return null;
                }

                if (Project.Type == ProjectType.AC6)
                {
                    if (objid.Length >= 6)
                        return GetOverridenFilePath($@"asset\environment\geometry\{objid}.geombnd.dcx");

                    return null;
                }

                return GetOverridenFilePath($@"obj\{objid}.objbnd.dcx");
            }
        }
        else if (pathElements[i].Equals("parts"))
        {
            i++;
            var partsId = pathElements[i];
            i++;
            if (pathElements[i].Equals("model") || pathElements[i].Equals("tex"))
            {
                bndpath = "";

                if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
                    return GetOverridenFilePath($@"parts\{partsId}.partsbnd");

                if (Project.Type == ProjectType.DS2S)
                {
                    var partType = "";
                    switch (partsId.Substring(0, 2))
                    {
                        case "as":
                            partType = "accessories";
                            break;
                        case "am":
                            partType = "arm";
                            break;
                        case "bd":
                            partType = "body";
                            break;
                        case "fa":
                        case "fc":
                        case "fg":
                            partType = "face";
                            break;
                        case "hd":
                            partType = "head";
                            break;
                        case "leg":
                            partType = "leg";
                            break;
                        case "sd":
                            partType = "shield";
                            break;
                        case "wp":
                            partType = "weapon";
                            break;
                    }

                    return GetOverridenFilePath($@"model\parts\{partType}\{partsId}.bnd");
                }

                if (Project.Type == ProjectType.AC6 && pathElements[i].Equals("tex"))
                {
                    string path;
                    if (partsId.Substring(0, 2) == "wp")
                    {
                        string id;
                        if (partsId.EndsWith("_l"))
                        {
                            id = partsId[..^2].Split("_").Last();
                            path = GetOverridenFilePath($@"parts\wp_{id}_l.tpf.dcx");
                        }
                        else
                        {
                            id = partsId.Split("_").Last();
                            path = GetOverridenFilePath($@"parts\wp_{id}.tpf.dcx");
                        }
                    }
                    else
                        path = GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");

                    return path;
                }

                return GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            }
        }
        else if (pathElements[i].Equals("menu"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetMenuTextureContainerPath(containerName);
            }
        }
        else if (pathElements[i].Equals("aet"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetAssetTextureContainerPath(containerName);
            }
        }
        else if (pathElements[i].Equals("other"))
        {
            i++;

            var containerName = pathElements[i];
            i++;

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return ResourceTextureLocator.GetOtherTextureContainerPath(containerName);
            }
        }

        bndpath = virtualPath;
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
