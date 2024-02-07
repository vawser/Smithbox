using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.ProjectCore;

/// <summary>
/// Exposes an interface to retrieve game assets from the various souls games. Also allows layering
/// of an additional mod directory on top of the game assets.
/// </summary>
public static class AssetLocator
{
    public static List<string> FullMapList;

    public static string GetAssetPath(string relpath)
    {
        if (UserProject.GameModDirectory != null)
        {
            var modpath = $@"{UserProject.GameModDirectory}\{relpath}";
            if (File.Exists(modpath))
                return modpath;
        }

        return $@"{UserProject.GameRootDirectory}\{relpath}";
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
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{relpath}"))
            return true;

        if (File.Exists($@"{UserProject.GameRootDirectory}\{relpath}"))
            return true;

        return false;
    }

    public static string GetOverridenFilePath(string relpath)
    {
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{relpath}"))
            return $@"{UserProject.GameModDirectory}\{relpath}";

        if (File.Exists($@"{UserProject.GameRootDirectory}\{relpath}"))
            return $@"{UserProject.GameRootDirectory}\{relpath}";

        return null;
    }

    /// <summary>
    ///     Gets the full list of maps in the game (excluding chalice dungeons). Basically if there's an msb for it,
    ///     it will be in this list.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetFullMapList()
    {
        if (UserProject.GameRootDirectory == null)
            return null;

        if (FullMapList != null)
            return FullMapList;

        try
        {
            HashSet<string> mapSet = new();

            // DS2 has its own structure for msbs, where they are all inside individual folders
            if (UserProject.Type == ProjectType.DS2S)
            {
                var maps = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + @"\map", @"m*").ToList();
                if (UserProject.GameModDirectory != null)
                    if (Directory.Exists(UserProject.GameModDirectory + @"\map"))
                        maps.AddRange(Directory.GetFileSystemEntries(UserProject.GameModDirectory + @"\map", @"m*").ToList());

                foreach (var map in maps)
                    mapSet.Add(Path.GetFileNameWithoutExtension($@"{map}.blah"));
            }
            else
            {
                var msbFiles = Directory
                    .GetFileSystemEntries(UserProject.GameRootDirectory + @"\map\MapStudio\", @"*.msb")
                    .Select(Path.GetFileNameWithoutExtension).ToList();
                msbFiles.AddRange(Directory
                    .GetFileSystemEntries(UserProject.GameRootDirectory + @"\map\MapStudio\", @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());
                if (UserProject.GameModDirectory != null && Directory.Exists(UserProject.GameModDirectory + @"\map\MapStudio\"))
                {
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(UserProject.GameModDirectory + @"\map\MapStudio\", @"*.msb")
                        .Select(Path.GetFileNameWithoutExtension).ToList());
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(UserProject.GameModDirectory + @"\map\MapStudio\", @"*.msb.dcx")
                        .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                        .ToList());
                }

                foreach (var msb in msbFiles)
                    mapSet.Add(msb);
            }

            Regex mapRegex = new(@"^m\d{2}_\d{2}_\d{2}_\d{2}$");
            var mapList = mapSet.Where(x => mapRegex.IsMatch(x)).ToList();
            mapList.Sort();
            FullMapList = mapList;
            return FullMapList;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game is likely not UXM unpacked
            return new List<string>();
        }
    }

    public static AssetDescription GetMapMSB(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
            return ad;

        string preferredPath;
        string backupPath;
        // SOFTS
        if (UserProject.Type == ProjectType.DS2S)
        {
            preferredPath = $@"map\{mapid}\{mapid}.msb";
            backupPath = $@"map\{mapid}\{mapid}.msb";
        }
        // BB chalice maps
        else if (UserProject.Type == ProjectType.BB && mapid.StartsWith("m29"))
        {
            preferredPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb";
        }
        // DeS, DS1, DS1R
        else if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DS1R ||
                 UserProject.Type == ProjectType.DES)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb";
            backupPath = $@"\map\MapStudio\{mapid}.msb.dcx";
        }
        // BB, DS3, ER, SSDT
        else if (UserProject.Type == ProjectType.BB || UserProject.Type == ProjectType.DS3 || UserProject.Type == ProjectType.ER ||
                 UserProject.Type == ProjectType.SDT)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }
        else
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }

        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{preferredPath}") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{preferredPath}";
        else if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{backupPath}") ||
                 writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{backupPath}";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{preferredPath}"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{preferredPath}";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{backupPath}"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{backupPath}";

        ad.AssetName = mapid;
        return ad;
    }


    public static List<AssetDescription> GetMapBTLs(string mapid, bool writemode = false)
    {
        List<AssetDescription> adList = new();
        if (mapid.Length != 12)
            return adList;

        if (UserProject.Type is ProjectType.DS2S)
        {
            // DS2 BTL is located inside map's .gibdt file
            AssetDescription ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}") ||
                writemode && UserProject.GameModDirectory != null)
                ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}";
            else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}"))
                ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}";

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            AssetDescription ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}") ||
                writemode && UserProject.GameModDirectory != null)
                ad2.AssetPath = $@"{UserProject.GameModDirectory}\{path}";
            else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}"))
                ad2.AssetPath = $@"{UserProject.GameRootDirectory}\{path}";

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (UserProject.Type is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            string path;
            if (UserProject.Type is ProjectType.ER or ProjectType.AC6)
                path = $@"map\{mapid[..3]}\{mapid}";
            else
                path = $@"map\{mapid}";

            List<string> files = new();

            if (Directory.Exists($@"{UserProject.GameRootDirectory}\{path}"))
            {
                files.AddRange(Directory.GetFiles($@"{UserProject.GameRootDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{UserProject.GameRootDirectory}\{path}", "*.btl.dcx").ToList());
            }

            if (Directory.Exists($@"{UserProject.GameModDirectory}\{path}"))
            {
                // Check for additional BTLs the user has created.
                files.AddRange(Directory.GetFiles($@"{UserProject.GameModDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{UserProject.GameModDirectory}\{path}", "*.btl.dcx").ToList());
                files = files.DistinctBy(f => f.Split("\\").Last()).ToList();
            }

            foreach (var file in files)
            {
                AssetDescription ad = new();
                var fileName = file.Split("\\").Last();
                if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}\{fileName}") ||
                    writemode && UserProject.GameModDirectory != null)
                    ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}\{fileName}";
                else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}\{fileName}"))
                    ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}\{fileName}";

                if (ad.AssetPath != null)
                {
                    ad.AssetName = fileName;
                    adList.Add(ad);
                }
            }
        }

        return adList;
    }

    public static AssetDescription GetMapNVA(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
            return ad;
        // BB chalice maps

        if (UserProject.Type == ProjectType.BB && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid.Substring(0, 9)}_00\{mapid}";
            if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.nva.dcx") ||
                writemode && UserProject.GameModDirectory != null && UserProject.Type != ProjectType.DS1)
                ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.nva.dcx";
            else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.nva.dcx"))
                ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.nva.dcx";
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";
            if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.nva.dcx") ||
                writemode && UserProject.GameModDirectory != null && UserProject.Type != ProjectType.DS1)
                ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.nva.dcx";
            else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.nva.dcx"))
                ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.nva.dcx";
            else if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.nva") ||
                     writemode && UserProject.GameModDirectory != null)
                ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.nva";
            else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.nva"))
                ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.nva";
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    ///     Get folders with msgbnds used in-game
    /// </summary>
    /// <returns>Dictionary with language name and path</returns>
    public static Dictionary<string, string> GetMsgLanguages()
    {
        Dictionary<string, string> dict = new();
        List<string> folders = new();
        try
        {
            if (UserProject.Type == ProjectType.DES)
            {
                folders = Directory.GetDirectories(UserProject.GameRootDirectory + @"\msg").ToList();
                // Japanese uses root directory
                if (File.Exists(UserProject.GameRootDirectory + @"\msg\menu.msgbnd.dcx") ||
                    File.Exists(UserProject.GameRootDirectory + @"\msg\item.msgbnd.dcx"))
                    dict.Add("Japanese", "");
            }
            else if (UserProject.Type == ProjectType.DS2S)
                folders = Directory.GetDirectories(UserProject.GameRootDirectory + @"\menu\text").ToList();
            else
                // Exclude folders that don't have typical msgbnds
                folders = Directory.GetDirectories(UserProject.GameRootDirectory + @"\msg")
                    .Where(x => !"common,as,eu,jp,na,uk,japanese".Contains(x.Split("\\").Last())).ToList();

            foreach (var path in folders)
                dict.Add(path.Split("\\").Last(), path);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
        }

        return dict;
    }

    /// <summary>
    ///     Get path of item.msgbnd (english by default)
    /// </summary>
    public static AssetDescription GetItemMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("item", langFolder, writemode);
    }

    /// <summary>
    ///     Get path of menu.msgbnd (english by default)
    /// </summary>
    public static AssetDescription GetMenuMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("menu", langFolder, writemode);
    }

    public static AssetDescription GetMsgbnd(string msgBndType, string langFolder, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        if (UserProject.Type == ProjectType.DES)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
            // Demon's Souls has msgbnds directly in the msg folder
            if (!File.Exists($@"{UserProject.GameRootDirectory}\{path}"))
                path = $@"msg\{msgBndType}.msgbnd.dcx";
        }
        else if (UserProject.Type == ProjectType.DS1)
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd";
        else if (UserProject.Type == ProjectType.DS1R)
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        else if (UserProject.Type == ProjectType.DS2S)
        {
            // DS2 does not have an msgbnd but loose fmg files instead
            path = $@"menu\text\{langFolder}";
            AssetDescription ad2 = new();
            ad2.AssetPath = writemode ? path : $@"{UserProject.GameRootDirectory}\{path}";
            //TODO: doesn't support project files
            return ad2;
        }
        else if (UserProject.Type == ProjectType.DS3)
            path = $@"msg\{langFolder}\{msgBndType}_dlc2.msgbnd.dcx";

        if (writemode)
        {
            ad.AssetPath = path;
            return ad;
        }

        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}";

        return ad;
    }

    public static string GetGameIDForDir()
    {
        switch (UserProject.Type)
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

    public static string GetScriptAssetsCommonDir()
    {
        return @"Assets\MassEditScripts\Common";
    }

    public static string GetScriptAssetsDir()
    {
        return $@"Assets\MassEditScripts\{GetGameIDForDir()}";
    }

    public static string GetUpgraderAssetsDir()
    {
        return $@"{GetParamAssetsDir()}\Upgrader";
    }

    public static string GetGameOffsetsAssetsDir()
    {
        return $@"Assets\GameOffsets\{GetGameIDForDir()}";
    }

    public static string GetParamAssetsDir()
    {
        return $@"Assets\Paramdex\{GetGameIDForDir()}";
    }

    public static string GetParamdefDir()
    {
        return $@"{GetParamAssetsDir()}\Defs";
    }

    public static string GetTentativeParamTypePath()
    {
        return $@"{GetParamAssetsDir()}\Defs\TentativeParamType.csv";
    }

    public static ulong[] GetParamdefPatches()
    {
        if (Directory.Exists($@"{GetParamAssetsDir()}\DefsPatch"))
        {
            var entries = Directory.GetFileSystemEntries($@"{GetParamAssetsDir()}\DefsPatch");
            return entries.Select(e => ulong.Parse(Path.GetFileNameWithoutExtension(e))).ToArray();
        }

        return new ulong[] { };
    }

    public static string GetParamdefPatchDir(ulong patch)
    {
        return $@"{GetParamAssetsDir()}\DefsPatch\{patch}";
    }

    public static string GetParammetaDir()
    {
        return $@"{GetParamAssetsDir()}\Meta";
    }

    public static string GetParamNamesDir()
    {
        return $@"{GetParamAssetsDir()}\Names";
    }

    public static string GetStrippedRowNamesPath(string paramName)
    {
        var dir = $@"{UserProject.ProjectDataDir}\Stripped Row Names";
        return $@"{dir}\{paramName}.txt";
    }

    public static PARAMDEF GetParamdefForParam(string paramType)
    {
        var pd = PARAMDEF.XmlDeserialize($@"{GetParamdefDir()}\{paramType}.xml");
        return pd;
    }

    public static AssetDescription GetDS2GeneratorParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorparam_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_generators";
        return ad;
    }

    public static AssetDescription GetDS2GeneratorLocationParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorlocation_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_generator_locations";
        return ad;
    }

    public static AssetDescription GetDS2GeneratorRegistParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorregistparam_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_generator_registrations";
        return ad;
    }

    public static AssetDescription GetDS2EventParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\eventparam_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_event_params";
        return ad;
    }

    public static AssetDescription GetDS2EventLocationParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\eventlocation_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_event_locations";
        return ad;
    }

    public static AssetDescription GetDS2ObjInstanceParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\mapobjectinstanceparam_{mapid}";
        if (UserProject.GameModDirectory != null && File.Exists($@"{UserProject.GameModDirectory}\{path}.param") ||
            writemode && UserProject.GameModDirectory != null)
            ad.AssetPath = $@"{UserProject.GameModDirectory}\{path}.param";
        else if (File.Exists($@"{UserProject.GameRootDirectory}\{path}.param"))
            ad.AssetPath = $@"{UserProject.GameRootDirectory}\{path}.param";

        ad.AssetName = mapid + "_object_instance_params";
        return ad;
    }

    public static List<AssetDescription> GetMapModels(string mapid)
    {
        List<AssetDescription> ret = new();
        if (UserProject.Type == ProjectType.DS3 || UserProject.Type == ProjectType.SDT)
        {
            if (!Directory.Exists(UserProject.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var mapfiles = Directory
                .GetFileSystemEntries(UserProject.GameRootDirectory + $@"\map\{mapid}\", @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                AssetDescription ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (UserProject.Type == ProjectType.DS2S)
        {
            AssetDescription ad = new();
            var name = mapid;
            ad.AssetName = name;
            ad.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.Add(ad);
        }
        else if (UserProject.Type == ProjectType.ER)
        {
            var mapPath = UserProject.GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
                return ret;

            var mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                AssetDescription ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (UserProject.Type == ProjectType.AC6)
        {
            var mapPath = UserProject.GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
                return ret;

            var mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                AssetDescription ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else
        {
            if (!Directory.Exists(UserProject.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var ext = UserProject.Type == ProjectType.DS1 ? @"*.flver" : @"*.flver.dcx";
            var mapfiles = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + $@"\map\{mapid}\", ext)
                .ToList();
            foreach (var f in mapfiles)
            {
                AssetDescription ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                // ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }

        return ret;
    }

    public static string MapModelNameToAssetName(string mapid, string modelname)
    {
        if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (UserProject.Type == ProjectType.DES)
            return $@"{modelname}";

        if (UserProject.Type == ProjectType.DS2S)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    /// <summary>
    ///     Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(string mapid)
    {
        if (UserProject.Type is ProjectType.ER or ProjectType.AC6)
            return mapid;

        if (UserProject.Type is ProjectType.DS1R)
            if (mapid.StartsWith("m99"))
                // DSR m99 maps contain their own assets
                return mapid;
            else if (UserProject.Type is ProjectType.DES)
                return mapid;
            else if (UserProject.Type is ProjectType.BB)
                if (mapid.StartsWith("m29"))
                    // Special case for chalice dungeon assets
                    return "m29_00_00_00";

        // Default
        return mapid.Substring(0, 6) + "_00_00";
    }

    public static AssetDescription GetMapModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.BB || UserProject.Type == ProjectType.DES)
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.flver");
        else if (UserProject.Type == ProjectType.DS1R)
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.flver.dcx");
        else if (UserProject.Type == ProjectType.DS2S)
            ret.AssetPath = GetAssetPath($@"model\map\{mapid}.mapbhd");
        else if (UserProject.Type == ProjectType.ER)
            ret.AssetPath = GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else if (UserProject.Type == ProjectType.AC6)
            ret.AssetPath = GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.mapbnd.dcx");

        ret.AssetName = model;
        if (UserProject.Type == ProjectType.DS2S)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}.flv.dcx";
        }
        else
        {
            if (UserProject.Type is not ProjectType.DES
                and not ProjectType.DS1
                and not ProjectType.DS1R
                and not ProjectType.BB)
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/{model}";

            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}/{model}.flver";
        }

        return ret;
    }

    public static AssetDescription GetMapCollisionModel(string mapid, string model, bool hi = true)
    {
        AssetDescription ret = new();
        if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DES)
            if (hi)
            {
                ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
            }
            else
            {
                ret.AssetPath = GetAssetPath($@"map\{mapid}\l{model.Substring(1)}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
            }
        else if (UserProject.Type == ProjectType.DS2S)
        {
            ret.AssetPath = GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (UserProject.Type == ProjectType.DS3 || UserProject.Type == ProjectType.BB)
            if (hi)
            {
                ret.AssetPath = GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/lo";
            }
        else
            return GetNullAsset();

        return ret;
    }

    public static List<AssetDescription> GetMapTextures(string mapid)
    {
        List<AssetDescription> ads = new();

        if (UserProject.Type == ProjectType.DS2S)
        {
            AssetDescription t = new();
            t.AssetPath = GetAssetPath($@"model\map\t{mapid.Substring(1)}.tpfbhd");
            t.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(t);
        }
        else if (UserProject.Type == ProjectType.DS1)
        {
            // TODO
        }
        else if (UserProject.Type == ProjectType.ER)
        {
            // TODO ER
        }
        else if (UserProject.Type == ProjectType.AC6)
        {
            // TODO AC6
        }
        else if (UserProject.Type == ProjectType.DES)
        {
            var mid = mapid.Substring(0, 3);
            var paths = Directory.GetFileSystemEntries($@"{UserProject.GameRootDirectory}\map\{mid}\", "*.tpf.dcx");
            foreach (var path in paths)
            {
                AssetDescription ad = new();
                ad.AssetPath = path;
                var tid = Path.GetFileNameWithoutExtension(path).Substring(4, 4);
                ad.AssetVirtualPath = $@"map/tex/{mid}/{tid}";
                ads.Add(ad);
            }
        }
        else
        {
            // Clean this up. Even if it's common code having something like "!=Sekiro" can lead to future issues
            var mid = mapid.Substring(0, 3);

            AssetDescription t0000 = new();
            t0000.AssetPath = GetAssetPath($@"map\{mid}\{mid}_0000.tpfbhd");
            t0000.AssetArchiveVirtualPath = $@"map/tex/{mid}/0000";
            ads.Add(t0000);

            AssetDescription t0001 = new();
            t0001.AssetPath = GetAssetPath($@"map\{mid}\{mid}_0001.tpfbhd");
            t0001.AssetArchiveVirtualPath = $@"map/tex/{mid}/0001";
            ads.Add(t0001);

            AssetDescription t0002 = new();
            t0002.AssetPath = GetAssetPath($@"map\{mid}\{mid}_0002.tpfbhd");
            t0002.AssetArchiveVirtualPath = $@"map/tex/{mid}/0002";
            ads.Add(t0002);

            AssetDescription t0003 = new();
            t0003.AssetPath = GetAssetPath($@"map\{mid}\{mid}_0003.tpfbhd");
            t0003.AssetArchiveVirtualPath = $@"map/tex/{mid}/0003";
            ads.Add(t0003);

            if (UserProject.Type == ProjectType.DS1R)
            {
                AssetDescription env = new();
                env.AssetPath = GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpfbhd");
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (UserProject.Type == ProjectType.BB || UserProject.Type == ProjectType.DS3)
            {
                AssetDescription env = new();
                env.AssetPath = GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                env.AssetVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (UserProject.Type == ProjectType.SDT)
            {
                //TODO SDT
            }
        }

        return ads;
    }

    public static List<string> GetEnvMapTextureNames(string mapid)
    {
        List<string> l = new();
        if (UserProject.Type == ProjectType.DS3)
        {
            var mid = mapid.Substring(0, 3);
            if (File.Exists(GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx")))
            {
                var t = TPF.Read(GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx"));
                foreach (TPF.Texture tex in t.Textures)
                    l.Add(tex.Name);
            }
        }

        return l;
    }

    private static string GetChrTexturePath(string chrid)
    {
        if (UserProject.Type is ProjectType.DES)
            return GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");

        if (UserProject.Type is ProjectType.DS1)
        {
            var path = GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
            if (path != null)
                return path;

            return GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (UserProject.Type is ProjectType.DS2S)
            return GetOverridenFilePath($@"model\chr\{chrid}.texbnd");

        if (UserProject.Type is ProjectType.DS1R)
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            return GetOverridenFilePath($@"chr\{chrid}.chrbnd");

        if (UserProject.Type is ProjectType.BB)
            return GetOverridenFilePath($@"chr\{chrid}_2.tpf.dcx");

        if (UserProject.Type is ProjectType.DS3 or ProjectType.SDT)
            return GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");

        if (UserProject.Type is ProjectType.ER)
            // TODO: Maybe add an option down the line to load lower quality
            return GetOverridenFilePath($@"chr\{chrid}_h.texbnd.dcx");

        if (UserProject.Type is ProjectType.AC6)
            return GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");

        return null;
    }

    public static AssetDescription GetChrTextures(string chrid)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (UserProject.Type is ProjectType.DES)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.DS1)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                if (path.EndsWith(".chrbnd"))
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                else
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.DS1R)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new AssetDescription();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.DS2S)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new AssetDescription();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.BB)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.DS3 or ProjectType.SDT)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (UserProject.Type is ProjectType.ER or ProjectType.AC6)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }

        return ad;
    }

    public static AssetDescription GetMapNVMModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DS1R || UserProject.Type == ProjectType.DES)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.nvm");
            ret.AssetName = model;
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
            ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.nvm";
        }
        else
            return GetNullAsset();

        return ret;
    }

    public static AssetDescription GetHavokNavmeshes(string mapid)
    {
        AssetDescription ret = new();
        ret.AssetPath = GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public static AssetDescription GetHavokNavmeshModel(string mapid, string model)
    {
        AssetDescription ret = new();
        ret.AssetPath = GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = model;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.hkx";

        return ret;
    }

    public static List<string> GetChrModels()
    {
        try
        {
            HashSet<string> chrs = new();
            List<string> ret = new();

            var modelDir = @"\chr";
            var modelExt = @".chrbnd.dcx";
            if (UserProject.Type == ProjectType.DS1)
                modelExt = ".chrbnd";
            else if (UserProject.Type == ProjectType.DS2S)
            {
                modelDir = @"\model\chr";
                modelExt = ".bnd";
            }

            if (UserProject.Type == ProjectType.DES)
            {
                var chrdirs = Directory.GetDirectories(UserProject.GameRootDirectory + modelDir);
                foreach (var f in chrdirs)
                {
                    var name = Path.GetFileNameWithoutExtension(f + ".dummy");
                    if (name.StartsWith("c"))
                        ret.Add(name);
                }

                return ret;
            }

            var chrfiles = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in chrfiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                chrs.Add(name);
            }

            if (UserProject.GameModDirectory != null && Directory.Exists(UserProject.GameModDirectory + modelDir))
            {
                chrfiles = Directory.GetFileSystemEntries(UserProject.GameModDirectory + modelDir, $@"*{modelExt}").ToList();
                foreach (var f in chrfiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!chrs.Contains(name))
                    {
                        ret.Add(name);
                        chrs.Add(name);
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

    public static AssetDescription GetChrModel(string chr)
    {
        AssetDescription ret = new();
        ret.AssetName = chr;
        ret.AssetArchiveVirtualPath = $@"chr/{chr}/model";
        if (UserProject.Type == ProjectType.DS2S)
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flv";
        else
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flver";

        return ret;
    }

    public static List<string> GetObjModels()
    {
        try
        {
            HashSet<string> objs = new();
            List<string> ret = new();

            var modelDir = @"\obj";
            var modelExt = @".objbnd.dcx";
            if (UserProject.Type == ProjectType.DS1)
                modelExt = ".objbnd";
            else if (UserProject.Type == ProjectType.DS2S)
            {
                modelDir = @"\model\obj";
                modelExt = ".bnd";
            }
            else if (UserProject.Type == ProjectType.ER)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\aeg";
                modelExt = ".geombnd.dcx";
            }
            else if (UserProject.Type == ProjectType.AC6)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\environment\geometry";
                modelExt = ".geombnd.dcx";
            }

            // Directories to search for obj models
            List<string> searchDirs = new();
            if (UserProject.Type == ProjectType.ER)
                searchDirs = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + modelDir, @"aeg*").ToList();
            else
                searchDirs.Add(UserProject.GameRootDirectory + modelDir);

            foreach (var searchDir in searchDirs)
            {
                var objfiles = Directory.GetFileSystemEntries(searchDir, $@"*{modelExt}").ToList();
                foreach (var f in objfiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    ret.Add(name);
                    objs.Add(name);
                }

                if (UserProject.GameModDirectory != null && Directory.Exists(searchDir))
                {
                    objfiles = Directory.GetFileSystemEntries(searchDir, $@"*{modelExt}").ToList();
                    foreach (var f in objfiles)
                    {
                        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                        if (!objs.Contains(name))
                        {
                            ret.Add(name);
                            objs.Add(name);
                        }
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

    public static AssetDescription GetObjModel(string obj)
    {
        AssetDescription ret = new();
        ret.AssetName = obj;
        ret.AssetArchiveVirtualPath = $@"obj/{obj}/model";
        if (UserProject.Type == ProjectType.DS2S)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flv";
        else if (UserProject.Type is ProjectType.ER or ProjectType.AC6)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj.ToUpper()}.flver";
        else
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flver";

        return ret;
    }

    public static AssetDescription GetObjTexture(string obj)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;
        if (UserProject.Type == ProjectType.DS1)
            path = GetOverridenFilePath($@"obj\{obj}.objbnd");
        else if (UserProject.Type is ProjectType.DES or ProjectType.DS1R or ProjectType.BB
                 or ProjectType.DS3 or ProjectType.SDT)
            path = GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"obj/{obj}/tex";
        }

        return ad;
    }

    public static AssetDescription GetAetTexture(string aetid)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path;
        if (UserProject.Type == ProjectType.ER)
            path = GetOverridenFilePath($@"asset\aet\{aetid.Substring(0, 6)}\{aetid}.tpf.dcx");
        else if (UserProject.Type is ProjectType.AC6)
            path = GetOverridenFilePath($@"\asset\environment\texture\{aetid}.tpf.dcx");
        else
            throw new NotSupportedException();

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"aet/{aetid}/tex";
        }

        return ad;
    }
    public static List<string> GetPartsModels()
    {
        try
        {
            HashSet<string> parts = new();
            List<string> ret = new();

            var modelDir = @"\parts";
            var modelExt = @".partsbnd.dcx";
            if (UserProject.Type == ProjectType.DS1)
                modelExt = ".partsbnd";
            else if (UserProject.Type == ProjectType.DS2S)
            {
                modelDir = @"\model\parts";
                modelExt = ".bnd";
                var partsGatheredFiles =
                    Directory.GetFiles(UserProject.GameRootDirectory + modelDir, "*", SearchOption.AllDirectories);
                foreach (var f in partsGatheredFiles)
                    if (!f.EndsWith("common.commonbnd.dcx") && !f.EndsWith("common_cloth.commonbnd.dcx") &&
                        !f.EndsWith("facepreset.bnd"))
                        ret.Add(Path.GetFileNameWithoutExtension(f));

                return ret;
            }

            var partsFiles = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in partsFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                parts.Add(name);
            }

            if (UserProject.GameModDirectory != null && Directory.Exists(UserProject.GameModDirectory + modelDir))
            {
                partsFiles = Directory.GetFileSystemEntries(UserProject.GameModDirectory + modelDir, $@"*{modelExt}").ToList();
                foreach (var f in partsFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!parts.Contains(name))
                    {
                        ret.Add(name);
                        parts.Add(name);
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

    public static AssetDescription GetPartsModel(string part)
    {
        AssetDescription ret = new();
        ret.AssetName = part;
        ret.AssetArchiveVirtualPath = $@"parts/{part}/model";
        if (UserProject.Type == ProjectType.DS2S)
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flv";
        else if (UserProject.Type is ProjectType.DS1)
            ret.AssetVirtualPath = $@"parts/{part}/model/{part.ToUpper()}.flver";
        else
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flver";

        return ret;
    }

    public static AssetDescription GetPartTextures(string partsId)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (UserProject.Type == ProjectType.AC6)
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

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (UserProject.Type == ProjectType.ER)
        {
            // Maybe add an option down the line to load lower quality
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (UserProject.Type == ProjectType.DS3 || UserProject.Type == ProjectType.SDT)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (UserProject.Type == ProjectType.BB)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (UserProject.Type == ProjectType.DS1)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (UserProject.Type == ProjectType.DES)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }

        return ad;
    }

    public static AssetDescription GetNullAsset()
    {
        AssetDescription ret = new();
        ret.AssetPath = "null";
        ret.AssetName = "null";
        ret.AssetArchiveVirtualPath = "null";
        ret.AssetVirtualPath = "null";
        return ret;
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
                if (UserProject.Type == ProjectType.DS2S)
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
                else if (UserProject.Type == ProjectType.DES)
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
                        if (UserProject.Type == ProjectType.DS1R)
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
                    if (UserProject.Type == ProjectType.DS1)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver");

                    if (UserProject.Type == ProjectType.DS1R)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (UserProject.Type == ProjectType.DS2S)
                        return GetAssetPath($@"model\map\{mapid}.mapbhd");

                    if (UserProject.Type == ProjectType.BB || UserProject.Type == ProjectType.DES)
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");

                    if (UserProject.Type == ProjectType.ER)
                        return GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");

                    if (UserProject.Type == ProjectType.AC6)
                        return GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");

                    return GetAssetPath($@"map\{mapid}\{pathElements[i]}.mapbnd.dcx");
                }

                if (pathElements[i].Equals("hit"))
                {
                    i++;
                    var hittype = pathElements[i];
                    i++;
                    if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DES)
                    {
                        bndpath = "";
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}");
                    }

                    if (UserProject.Type == ProjectType.DS2S)
                    {
                        bndpath = "";
                        return GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (UserProject.Type == ProjectType.DS3 || UserProject.Type == ProjectType.BB)
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
                    if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DES ||
                        UserProject.Type == ProjectType.DS1R)
                    {
                        if (i < pathElements.Length)
                            bndpath = $@"{pathElements[i]}";
                        else
                            bndpath = "";

                        if (UserProject.Type == ProjectType.DS1R)
                            return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd.dcx");

                        return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd");
                    }

                    if (UserProject.Type == ProjectType.DS3)
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
                if (UserProject.Type == ProjectType.DS1)
                    return GetOverridenFilePath($@"chr\{chrid}.chrbnd");

                if (UserProject.Type == ProjectType.DS2S)
                    return GetOverridenFilePath($@"model\chr\{chrid}.bnd");

                if (UserProject.Type == ProjectType.DES)
                    return GetOverridenFilePath($@"chr\{chrid}\{chrid}.chrbnd.dcx");

                return GetOverridenFilePath($@"chr\{chrid}.chrbnd.dcx");
            }

            if (pathElements[i].Equals("tex"))
            {
                bndpath = "";
                return GetChrTexturePath(chrid);
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
                if (UserProject.Type == ProjectType.DS1)
                    return GetOverridenFilePath($@"obj\{objid}.objbnd");

                if (UserProject.Type == ProjectType.DS2S)
                    return GetOverridenFilePath($@"model\obj\{objid}.bnd");

                if (UserProject.Type == ProjectType.ER)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                        return GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{objid}.geombnd.dcx");

                    return null;
                }

                if (UserProject.Type == ProjectType.AC6)
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
                if (UserProject.Type == ProjectType.DS1 || UserProject.Type == ProjectType.DS1R)
                    return GetOverridenFilePath($@"parts\{partsId}.partsbnd");

                if (UserProject.Type == ProjectType.DS2S)
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

                if (UserProject.Type == ProjectType.ER)
                    return GetOverridenFilePath($@"parts\{partsId}\{partsId}.partsbnd.dcx");

                if (UserProject.Type == ProjectType.AC6 && pathElements[i].Equals("tex"))
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

    public static List<string> GetDrawParams()
    {
        try
        {
            HashSet<string> drawParams = new();
            List<string> ret = new();

            var paramDir = @"\param\drawparam";
            var paramExt = @".gparam.dcx";

            var paramFiles = Directory.GetFileSystemEntries(UserProject.GameRootDirectory + paramDir, $@"*{paramExt}")
                .ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                drawParams.Add(name);
            }

            if (UserProject.GameModDirectory != null && Directory.Exists(UserProject.GameModDirectory + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(UserProject.GameModDirectory + paramDir, $@"*{paramExt}").ToList();
                foreach (var f in paramFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!drawParams.Contains(name))
                    {
                        ret.Add(name);
                        drawParams.Add(name);
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
}
