using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore;

/// <summary>
///     Generic asset description for a generic game asset
/// </summary>
public class AssetDescription
{
    public string AssetArchiveVirtualPath;

    /// <summary>
    ///     Where applicable, the numeric asset ID. Usually applies to chrs, objs, and various map pieces
    /// </summary>
    public int AssetID;

    /// <summary>
    ///     Pretty UI friendly name for an asset. Usually the file name without an extention i.e. c1234
    /// </summary>
    public string AssetName;

    /// <summary>
    ///     Absolute path of where the full asset is located. If this asset exists in a mod override directory,
    ///     then this path points to that instead of the base game asset.
    /// </summary>
    public string AssetPath;

    /// <summary>
    ///     Virtual friendly path for this asset to use with the resource manager
    /// </summary>
    public string AssetVirtualPath;

    public override int GetHashCode()
    {
        if (AssetVirtualPath != null)
        {
            return AssetVirtualPath.GetHashCode();
        }

        if (AssetPath != null)
        {
            return AssetPath.GetHashCode();
        }

        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is AssetDescription ad)
        {
            if (AssetVirtualPath != null)
            {
                return AssetVirtualPath.Equals(ad.AssetVirtualPath);
            }

            if (AssetPath != null)
            {
                return AssetPath.Equals(ad.AssetPath);
            }
        }

        return base.Equals(obj);
    }
}

/// <summary>
///     Exposes an interface to retrieve game assets from the various souls games. Also allows layering
///     of an additional mod directory on top of the game assets.
/// </summary>
public class AssetLocator
{
    public static readonly string GameExecutableFilter;
    public static readonly string ProjectJsonFilter;
    public static readonly string RegulationBinFilter;
    public static readonly string Data0Filter;
    public static readonly string ParamBndDcxFilter;
    public static readonly string ParamBndFilter;
    public static readonly string EncRegulationFilter;
    public static readonly string ParamLooseFilter;
    public static readonly string CsvFilter;
    public static readonly string TxtFilter;
    public static readonly string FmgJsonFilter;

    private List<string> FullMapList;

    static AssetLocator()
    {
        // These patterns are meant to be passed directly into PlatformUtils.
        // Everything about their handling should be done there.

        // Game Executable (.EXE, EBOOT.BIN)|*.EXE*;*EBOOT.BIN*
        // Windows executable (*.EXE)|*.EXE*
        // Playstation executable (*.BIN)|*.BIN*
        GameExecutableFilter = "exe,bin";
        // Project file (project.json)|PROJECT.JSON
        ProjectJsonFilter = "json";
        // Regulation file (regulation.bin)|REGULATION.BIN
        RegulationBinFilter = "bin";
        // Data file (Data0.bdt)|DATA0.BDT
        Data0Filter = "bdt";
        // ParamBndDcx (gameparam.parambnd.dcx)|GAMEPARAM.PARAMBND.DCX
        ParamBndDcxFilter = "parambnd.dcx";
        // ParamBnd (gameparam.parambnd)|GAMEPARAM.PARAMBND
        ParamBndFilter = "parambnd";
        // Enc_RegBndDcx (enc_regulation.bnd.dcx)|ENC_REGULATION.BND.DCX
        EncRegulationFilter = "bnd.dcx";
        // Loose Param file (*.Param)|*.Param
        ParamLooseFilter = "param";
        // CSV file (*.csv)|*.csv
        CsvFilter = "csv";
        // Text file (*.txt)|*.txt
        TxtFilter = "txt";
        // Exported FMGs (*.fmg.json)|*.fmg.json
        FmgJsonFilter = "fmg.json";
        // All file filter is implicitly added by NFD. Ideally this is used explicitly.
        // All files|*.*
    }

    public GameType Type { get; private set; } = GameType.Undefined;

    /// <summary>
    ///     The game interroot where all the game assets are
    /// </summary>
    public string GameRootDirectory { get; private set; }

    /// <summary>
    ///     An optional override mod directory where modded files are stored
    /// </summary>
    public string GameModDirectory { get; private set; }

    /// <summary>
    ///     Directory where misc DSMapStudio files associated with a project are stored.
    /// </summary>
    public string ProjectMiscDir => @$"{GameModDirectory}\DSMapStudio";

    public string GetAssetPath(string relpath)
    {
        if (GameModDirectory != null)
        {
            var modpath = $@"{GameModDirectory}\{relpath}";
            if (File.Exists(modpath))
            {
                return modpath;
            }
        }

        return $@"{GameRootDirectory}\{relpath}";
    }

    public GameType GetGameTypeForExePath(string exePath)
    {
        var type = GameType.Undefined;
        if (exePath.ToLower().Contains("darksouls.exe"))
        {
            type = GameType.DarkSoulsPTDE;
        }
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
        {
            type = GameType.DarkSoulsRemastered;
        }
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
        {
            type = GameType.DarkSoulsIISOTFS;
        }
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
        {
            type = GameType.DarkSoulsIII;
        }
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
            {
                type = GameType.Bloodborne;
            }
            else
            {
                type = GameType.DemonsSouls;
            }
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
        {
            type = GameType.Sekiro;
        }
        else if (exePath.ToLower().Contains("eldenring.exe"))
        {
            type = GameType.EldenRing;
        }
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
        {
            type = GameType.ArmoredCoreVI;
        }

        return type;
    }

    public bool CheckFilesExpanded(string gamepath, GameType game)
    {
        if (game == GameType.EldenRing)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
            {
                return false;
            }

            if (!Directory.Exists($@"{gamepath}\asset"))
            {
                return false;
            }
        }

        if (game is GameType.DarkSoulsPTDE or GameType.DarkSoulsIII or GameType.Sekiro)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
            {
                return false;
            }

            if (!Directory.Exists($@"{gamepath}\obj"))
            {
                return false;
            }
        }

        if (game == GameType.DarkSoulsIISOTFS)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
            {
                return false;
            }

            if (!Directory.Exists($@"{gamepath}\model\obj"))
            {
                return false;
            }
        }

        if (game == GameType.ArmoredCoreVI)
        {
            if (!Directory.Exists($@"{gamepath}\map"))
            {
                return false;
            }

            if (!Directory.Exists($@"{gamepath}\asset"))
            {
                return false;
            }
        }

        return true;
    }

    public void SetModProjectDirectory(string dir)
    {
        GameModDirectory = dir;
    }

    public void SetFromProjectSettings(ProjectSettings settings, string moddir)
    {
        Type = settings.GameType;
        GameRootDirectory = settings.GameRoot;
        GameModDirectory = moddir;
        FullMapList = null;
    }

    public bool CreateRecoveryProject()
    {
        if (GameRootDirectory == null || GameModDirectory == null)
        {
            return false;
        }

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

    public bool FileExists(string relpath)
    {
        if (GameModDirectory != null && File.Exists($@"{GameModDirectory}\{relpath}"))
        {
            return true;
        }

        if (File.Exists($@"{GameRootDirectory}\{relpath}"))
        {
            return true;
        }

        return false;
    }

    public string GetOverridenFilePath(string relpath)
    {
        if (GameModDirectory != null && File.Exists($@"{GameModDirectory}\{relpath}"))
        {
            return $@"{GameModDirectory}\{relpath}";
        }

        if (File.Exists($@"{GameRootDirectory}\{relpath}"))
        {
            return $@"{GameRootDirectory}\{relpath}";
        }

        return null;
    }

    /// <summary>
    ///     Gets the full list of maps in the game (excluding chalice dungeons). Basically if there's an msb for it,
    ///     it will be in this list.
    /// </summary>
    /// <returns></returns>
    public List<string> GetFullMapList()
    {
        if (GameRootDirectory == null)
        {
            return null;
        }

        if (FullMapList != null)
        {
            return FullMapList;
        }

        try
        {
            HashSet<string> mapSet = new();

            // DS2 has its own structure for msbs, where they are all inside individual folders
            if (Type == GameType.DarkSoulsIISOTFS)
            {
                List<string> maps = Directory.GetFileSystemEntries(GameRootDirectory + @"\map", @"m*").ToList();
                if (GameModDirectory != null)
                {
                    if (Directory.Exists(GameModDirectory + @"\map"))
                    {
                        maps.AddRange(Directory.GetFileSystemEntries(GameModDirectory + @"\map", @"m*").ToList());
                    }
                }

                foreach (var map in maps)
                {
                    mapSet.Add(Path.GetFileNameWithoutExtension($@"{map}.blah"));
                }
            }
            else
            {
                List<string> msbFiles = Directory
                    .GetFileSystemEntries(GameRootDirectory + @"\map\MapStudio\", @"*.msb")
                    .Select(Path.GetFileNameWithoutExtension).ToList();
                msbFiles.AddRange(Directory
                    .GetFileSystemEntries(GameRootDirectory + @"\map\MapStudio\", @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());
                if (GameModDirectory != null && Directory.Exists(GameModDirectory + @"\map\MapStudio\"))
                {
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(GameModDirectory + @"\map\MapStudio\", @"*.msb")
                        .Select(Path.GetFileNameWithoutExtension).ToList());
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(GameModDirectory + @"\map\MapStudio\", @"*.msb.dcx")
                        .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                        .ToList());
                }

                foreach (var msb in msbFiles)
                {
                    mapSet.Add(msb);
                }
            }

            Regex mapRegex = new(@"^m\d{2}_\d{2}_\d{2}_\d{2}$");
            List<string> mapList = mapSet.Where(x => mapRegex.IsMatch(x)).ToList();
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

    public AssetDescription GetMapMSB(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
        {
            return ad;
        }

        string preferredPath;
        string backupPath;
        // SOFTS
        if (Type == GameType.DarkSoulsIISOTFS)
        {
            preferredPath = $@"map\{mapid}\{mapid}.msb";
            backupPath = $@"map\{mapid}\{mapid}.msb";
        }
        // BB chalice maps
        else if (Type == GameType.Bloodborne && mapid.StartsWith("m29"))
        {
            preferredPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb";
        }
        // DeS, DS1, DS1R
        else if (Type == GameType.DarkSoulsPTDE || Type == GameType.DarkSoulsRemastered ||
                 Type == GameType.DemonsSouls)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb";
            backupPath = $@"\map\MapStudio\{mapid}.msb.dcx";
        }
        // BB, DS3, ER, SSDT
        else if (Type == GameType.Bloodborne || Type == GameType.DarkSoulsIII || Type == GameType.EldenRing ||
                 Type == GameType.Sekiro)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }
        else
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }

        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{preferredPath}")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{preferredPath}";
        }
        else if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{backupPath}")) ||
                 (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{backupPath}";
        }
        else if (File.Exists($@"{GameRootDirectory}\{preferredPath}"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{preferredPath}";
        }
        else if (File.Exists($@"{GameRootDirectory}\{backupPath}"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{backupPath}";
        }

        ad.AssetName = mapid;
        return ad;
    }


    public List<AssetDescription> GetMapBTLs(string mapid, bool writemode = false)
    {
        List<AssetDescription> adList = new();
        if (mapid.Length != 12)
        {
            return adList;
        }

        if (Type is GameType.DarkSoulsIISOTFS)
        {
            // DS2 BTL is located inside map's .gibdt file
            AssetDescription ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}")) ||
                (writemode && GameModDirectory != null))
            {
                ad.AssetPath = $@"{GameModDirectory}\{path}";
            }
            else if (File.Exists($@"{GameRootDirectory}\{path}"))
            {
                ad.AssetPath = $@"{GameRootDirectory}\{path}";
            }

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            AssetDescription ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}")) ||
                (writemode && GameModDirectory != null))
            {
                ad2.AssetPath = $@"{GameModDirectory}\{path}";
            }
            else if (File.Exists($@"{GameRootDirectory}\{path}"))
            {
                ad2.AssetPath = $@"{GameRootDirectory}\{path}";
            }

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (Type is GameType.Bloodborne or GameType.DarkSoulsIII or GameType.Sekiro or GameType.EldenRing or GameType.ArmoredCoreVI)
        {
            string path;
            if (Type is GameType.EldenRing or GameType.ArmoredCoreVI)
            {
                path = $@"map\{mapid[..3]}\{mapid}";
            }
            else
            {
                path = $@"map\{mapid}";
            }

            List<string> files = new();

            if (Directory.Exists($@"{GameRootDirectory}\{path}"))
            {
                files.AddRange(Directory.GetFiles($@"{GameRootDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{GameRootDirectory}\{path}", "*.btl.dcx").ToList());
            }

            if (Directory.Exists($@"{GameModDirectory}\{path}"))
            {
                // Check for additional BTLs the user has created.
                files.AddRange(Directory.GetFiles($@"{GameModDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{GameModDirectory}\{path}", "*.btl.dcx").ToList());
                files = files.DistinctBy(f => f.Split("\\").Last()).ToList();
            }

            foreach (var file in files)
            {
                AssetDescription ad = new();
                var fileName = file.Split("\\").Last();
                if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}\{fileName}")) ||
                    (writemode && GameModDirectory != null))
                {
                    ad.AssetPath = $@"{GameModDirectory}\{path}\{fileName}";
                }
                else if (File.Exists($@"{GameRootDirectory}\{path}\{fileName}"))
                {
                    ad.AssetPath = $@"{GameRootDirectory}\{path}\{fileName}";
                }

                if (ad.AssetPath != null)
                {
                    ad.AssetName = fileName;
                    adList.Add(ad);
                }
            }
        }

        return adList;
    }

    public AssetDescription GetMapNVA(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
        {
            return ad;
        }
        // BB chalice maps

        if (Type == GameType.Bloodborne && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid.Substring(0, 9)}_00\{mapid}";
            if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.nva.dcx")) ||
                (writemode && GameModDirectory != null && Type != GameType.DarkSoulsPTDE))
            {
                ad.AssetPath = $@"{GameModDirectory}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{GameRootDirectory}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{GameRootDirectory}\{path}.nva.dcx";
            }
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";
            if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.nva.dcx")) ||
                (writemode && GameModDirectory != null && Type != GameType.DarkSoulsPTDE))
            {
                ad.AssetPath = $@"{GameModDirectory}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{GameRootDirectory}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{GameRootDirectory}\{path}.nva.dcx";
            }
            else if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.nva")) ||
                     (writemode && GameModDirectory != null))
            {
                ad.AssetPath = $@"{GameModDirectory}\{path}.nva";
            }
            else if (File.Exists($@"{GameRootDirectory}\{path}.nva"))
            {
                ad.AssetPath = $@"{GameRootDirectory}\{path}.nva";
            }
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    ///     Get folders with msgbnds used in-game
    /// </summary>
    /// <returns>Dictionary with language name and path</returns>
    public Dictionary<string, string> GetMsgLanguages()
    {
        Dictionary<string, string> dict = new();
        List<string> folders = new();
        try
        {
            if (Type == GameType.DemonsSouls)
            {
                folders = Directory.GetDirectories(GameRootDirectory + @"\msg").ToList();
                // Japanese uses root directory
                if (File.Exists(GameRootDirectory + @"\msg\menu.msgbnd.dcx") ||
                    File.Exists(GameRootDirectory + @"\msg\item.msgbnd.dcx"))
                {
                    dict.Add("Japanese", "");
                }
            }
            else if (Type == GameType.DarkSoulsIISOTFS)
            {
                folders = Directory.GetDirectories(GameRootDirectory + @"\menu\text").ToList();
            }
            else
            {
                // Exclude folders that don't have typical msgbnds
                folders = Directory.GetDirectories(GameRootDirectory + @"\msg")
                    .Where(x => !"common,as,eu,jp,na,uk,japanese".Contains(x.Split("\\").Last())).ToList();
            }

            foreach (var path in folders)
            {
                dict.Add(path.Split("\\").Last(), path);
            }
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
        }

        return dict;
    }

    /// <summary>
    ///     Get path of item.msgbnd (english by default)
    /// </summary>
    public AssetDescription GetItemMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("item", langFolder, writemode);
    }

    /// <summary>
    ///     Get path of menu.msgbnd (english by default)
    /// </summary>
    public AssetDescription GetMenuMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("menu", langFolder, writemode);
    }

    public AssetDescription GetMsgbnd(string msgBndType, string langFolder, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        if (Type == GameType.DemonsSouls)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
            // Demon's Souls has msgbnds directly in the msg folder
            if (!File.Exists($@"{GameRootDirectory}\{path}"))
            {
                path = $@"msg\{msgBndType}.msgbnd.dcx";
            }
        }
        else if (Type == GameType.DarkSoulsPTDE)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd";
        }
        else if (Type == GameType.DarkSoulsRemastered)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        }
        else if (Type == GameType.DarkSoulsIISOTFS)
        {
            // DS2 does not have an msgbnd but loose fmg files instead
            path = $@"menu\text\{langFolder}";
            AssetDescription ad2 = new();
            ad2.AssetPath = writemode ? path : $@"{GameRootDirectory}\{path}";
            //TODO: doesn't support project files
            return ad2;
        }
        else if (Type == GameType.DarkSoulsIII)
        {
            path = $@"msg\{langFolder}\{msgBndType}_dlc2.msgbnd.dcx";
        }

        if (writemode)
        {
            ad.AssetPath = path;
            return ad;
        }

        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}";
        }

        return ad;
    }

    private string GetGameIDForDir()
    {
        switch (Type)
        {
            case GameType.DemonsSouls:
                return "DES";
            case GameType.DarkSoulsPTDE:
                return "DS1";
            case GameType.DarkSoulsRemastered:
                return "DS1R";
            case GameType.DarkSoulsIISOTFS:
                return "DS2S";
            case GameType.Bloodborne:
                return "BB";
            case GameType.DarkSoulsIII:
                return "DS3";
            case GameType.Sekiro:
                return "SDT";
            case GameType.EldenRing:
                return "ER";
            case GameType.ArmoredCoreVI:
                return "AC6";
            default:
                throw new Exception("Game type not set");
        }
    }

    public string GetAliasAssetsDir()
    {
        return $@"Assets\Aliases\{GetGameIDForDir()}";
    }


    public string GetScriptAssetsCommonDir()
    {
        return @"Assets\MassEditScripts\Common";
    }

    public string GetScriptAssetsDir()
    {
        return $@"Assets\MassEditScripts\{GetGameIDForDir()}";
    }

    public string GetUpgraderAssetsDir()
    {
        return $@"{GetParamAssetsDir()}\Upgrader";
    }

    public string GetGameOffsetsAssetsDir()
    {
        return $@"Assets\GameOffsets\{GetGameIDForDir()}";
    }

    public string GetParamAssetsDir()
    {
        return $@"Assets\Paramdex\{GetGameIDForDir()}";
    }

    public string GetParamdefDir()
    {
        return $@"{GetParamAssetsDir()}\Defs";
    }

    public string GetTentativeParamTypePath()
    {
        return $@"{GetParamAssetsDir()}\Defs\TentativeParamType.csv";
    }

    public ulong[] GetParamdefPatches()
    {
        if (Directory.Exists($@"{GetParamAssetsDir()}\DefsPatch"))
        {
            var entries = Directory.GetFileSystemEntries($@"{GetParamAssetsDir()}\DefsPatch");
            return entries.Select(e => ulong.Parse(Path.GetFileNameWithoutExtension(e))).ToArray();
        }

        return new ulong[] { };
    }

    public string GetParamdefPatchDir(ulong patch)
    {
        return $@"{GetParamAssetsDir()}\DefsPatch\{patch}";
    }

    public string GetParammetaDir()
    {
        return $@"{GetParamAssetsDir()}\Meta";
    }

    public string GetParamNamesDir()
    {
        return $@"{GetParamAssetsDir()}\Names";
    }

    public string GetStrippedRowNamesPath(string paramName)
    {
        var dir = $@"{ProjectMiscDir}\Stripped Row Names";
        return $@"{dir}\{paramName}.txt";
    }

    public PARAMDEF GetParamdefForParam(string paramType)
    {
        PARAMDEF pd = PARAMDEF.XmlDeserialize($@"{GetParamdefDir()}\{paramType}.xml");
        return pd;
    }

    public AssetDescription GetDS2GeneratorParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorparam_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_generators";
        return ad;
    }

    public AssetDescription GetDS2GeneratorLocationParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorlocation_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_generator_locations";
        return ad;
    }

    public AssetDescription GetDS2GeneratorRegistParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\generatorregistparam_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_generator_registrations";
        return ad;
    }

    public AssetDescription GetDS2EventParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\eventparam_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_event_params";
        return ad;
    }

    public AssetDescription GetDS2EventLocationParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\eventlocation_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_event_locations";
        return ad;
    }

    public AssetDescription GetDS2ObjInstanceParam(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"Param\mapobjectinstanceparam_{mapid}";
        if ((GameModDirectory != null && File.Exists($@"{GameModDirectory}\{path}.param")) ||
            (writemode && GameModDirectory != null))
        {
            ad.AssetPath = $@"{GameModDirectory}\{path}.param";
        }
        else if (File.Exists($@"{GameRootDirectory}\{path}.param"))
        {
            ad.AssetPath = $@"{GameRootDirectory}\{path}.param";
        }

        ad.AssetName = mapid + "_object_instance_params";
        return ad;
    }

    public List<AssetDescription> GetMapModels(string mapid)
    {
        List<AssetDescription> ret = new();
        if (Type == GameType.DarkSoulsIII || Type == GameType.Sekiro)
        {
            if (!Directory.Exists(GameRootDirectory + $@"\map\{mapid}\"))
            {
                return ret;
            }

            List<string> mapfiles = Directory
                .GetFileSystemEntries(GameRootDirectory + $@"\map\{mapid}\", @"*.mapbnd.dcx").ToList();
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
        else if (Type == GameType.DarkSoulsIISOTFS)
        {
            AssetDescription ad = new();
            var name = mapid;
            ad.AssetName = name;
            ad.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.Add(ad);
        }
        else if (Type == GameType.EldenRing)
        {
            var mapPath = GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
            {
                return ret;
            }

            List<string> mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
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
        else if (Type == GameType.ArmoredCoreVI)
        {
            var mapPath = GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
            {
                return ret;
            }

            List<string> mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
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
            if (!Directory.Exists(GameRootDirectory + $@"\map\{mapid}\"))
            {
                return ret;
            }

            var ext = Type == GameType.DarkSoulsPTDE ? @"*.flver" : @"*.flver.dcx";
            List<string> mapfiles = Directory.GetFileSystemEntries(GameRootDirectory + $@"\map\{mapid}\", ext)
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

    public string MapModelNameToAssetName(string mapid, string modelname)
    {
        if (Type == GameType.DarkSoulsPTDE || Type == GameType.DarkSoulsRemastered)
        {
            return $@"{modelname}A{mapid.Substring(1, 2)}";
        }

        if (Type == GameType.DemonsSouls)
        {
            return $@"{modelname}";
        }

        if (Type == GameType.DarkSoulsIISOTFS)
        {
            return modelname;
        }

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    /// <summary>
    ///     Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public string GetAssetMapID(string mapid)
    {
        if (Type is GameType.EldenRing or GameType.ArmoredCoreVI)
        {
            return mapid;
        }

        if (Type is GameType.DarkSoulsRemastered)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (Type is GameType.DemonsSouls)
        {
            return mapid;
        }
        else if (Type is GameType.Bloodborne)
        {
            if (mapid.StartsWith("m29"))
            {
                // Special case for chalice dungeon assets
                return "m29_00_00_00";
            }
        }

        // Default
        return mapid.Substring(0, 6) + "_00_00";
    }

    public AssetDescription GetMapModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (Type == GameType.DarkSoulsPTDE || Type == GameType.Bloodborne || Type == GameType.DemonsSouls)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.flver");
        }
        else if (Type == GameType.DarkSoulsRemastered)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.flver.dcx");
        }
        else if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetPath = GetAssetPath($@"model\map\{mapid}.mapbhd");
        }
        else if (Type == GameType.EldenRing)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        }
        else if (Type == GameType.ArmoredCoreVI)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        }
        else
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.mapbnd.dcx");
        }

        ret.AssetName = model;
        if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}.flv.dcx";
        }
        else
        {
            if (Type is not GameType.DemonsSouls
                and not GameType.DarkSoulsPTDE
                and not GameType.DarkSoulsRemastered
                and not GameType.Bloodborne)
            {
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/model/{model}";
            }

            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}/{model}.flver";
        }

        return ret;
    }

    public AssetDescription GetMapCollisionModel(string mapid, string model, bool hi = true)
    {
        AssetDescription ret = new();
        if (Type == GameType.DarkSoulsPTDE || Type == GameType.DemonsSouls)
        {
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
        }
        else if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetPath = GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (Type == GameType.DarkSoulsIII || Type == GameType.Bloodborne)
        {
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
        }
        else
        {
            return GetNullAsset();
        }

        return ret;
    }

    public List<AssetDescription> GetMapTextures(string mapid)
    {
        List<AssetDescription> ads = new();

        if (Type == GameType.DarkSoulsIISOTFS)
        {
            AssetDescription t = new();
            t.AssetPath = GetAssetPath($@"model\map\t{mapid.Substring(1)}.tpfbhd");
            t.AssetArchiveVirtualPath = $@"map/tex/{mapid}/tex";
            ads.Add(t);
        }
        else if (Type == GameType.DarkSoulsPTDE)
        {
            // TODO
        }
        else if (Type == GameType.EldenRing)
        {
            // TODO ER
        }
        else if (Type == GameType.ArmoredCoreVI)
        {
            // TODO ER
        }
        else if (Type == GameType.DemonsSouls)
        {
            var mid = mapid.Substring(0, 3);
            var paths = Directory.GetFileSystemEntries($@"{GameRootDirectory}\map\{mid}\", "*.tpf.dcx");
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

            if (Type == GameType.DarkSoulsRemastered)
            {
                AssetDescription env = new();
                env.AssetPath = GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpfbhd");
                env.AssetArchiveVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Type == GameType.Bloodborne || Type == GameType.DarkSoulsIII)
            {
                AssetDescription env = new();
                env.AssetPath = GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx");
                env.AssetVirtualPath = $@"map/tex/{mid}/env";
                ads.Add(env);
            }
            else if (Type == GameType.Sekiro)
            {
                //TODO SDT
            }
        }

        return ads;
    }

    public List<string> GetEnvMapTextureNames(string mapid)
    {
        List<string> l = new();
        if (Type == GameType.DarkSoulsIII)
        {
            var mid = mapid.Substring(0, 3);
            if (File.Exists(GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx")))
            {
                TPF t = TPF.Read(GetAssetPath($@"map\{mid}\{mid}_envmap.tpf.dcx"));
                foreach (TPF.Texture tex in t.Textures)
                {
                    l.Add(tex.Name);
                }
            }
        }

        return l;
    }

    private string GetChrTexturePath(string chrid)
    {
        if (Type is GameType.DemonsSouls)
        {
            return GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
        }

        if (Type is GameType.DarkSoulsPTDE)
        {
            var path = GetOverridenFilePath($@"chr\{chrid}\{chrid}.tpf");
            if (path != null)
            {
                return path;
            }

            return GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Type is GameType.DarkSoulsIISOTFS)
        {
            return GetOverridenFilePath($@"model\chr\{chrid}.texbnd");
        }

        if (Type is GameType.DarkSoulsRemastered)
        {
            // TODO: Some textures require getting chrtpfbhd from chrbnd, then using it with chrtpfbdt in chr folder.
            return GetOverridenFilePath($@"chr\{chrid}.chrbnd");
        }

        if (Type is GameType.Bloodborne)
        {
            return GetOverridenFilePath($@"chr\{chrid}_2.tpf.dcx");
        }

        if (Type is GameType.DarkSoulsIII or GameType.Sekiro)
        {
            return GetOverridenFilePath($@"chr\{chrid}.texbnd.dcx");
        }

        if (Type is GameType.EldenRing)
        {
            // TODO: Maybe add an option down the line to load lower quality
            return GetOverridenFilePath($@"chr\{chrid}_h.texbnd.dcx");
        }

        if (Type is GameType.ArmoredCoreVI)
        {
            // TODO AC6
        }

        return null;
    }

    public AssetDescription GetChrTextures(string chrid)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (Type is GameType.DemonsSouls)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Type is GameType.DarkSoulsPTDE)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                if (path.EndsWith(".chrbnd"))
                {
                    ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
                }
                else
                {
                    ad.AssetVirtualPath = $@"chr/{chrid}/tex";
                }
            }
        }
        else if (Type is GameType.DarkSoulsRemastered)
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
        else if (Type is GameType.DarkSoulsIISOTFS)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad = new AssetDescription();
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Type is GameType.Bloodborne)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Type is GameType.DarkSoulsIII or GameType.Sekiro)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Type is GameType.EldenRing)
        {
            var path = GetChrTexturePath(chrid);
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"chr/{chrid}/tex";
            }
        }
        else if (Type is GameType.ArmoredCoreVI)
        {
            // TODO AC6
        }

        return ad;
    }

    public AssetDescription GetMapNVMModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (Type == GameType.DarkSoulsPTDE || Type == GameType.DarkSoulsRemastered || Type == GameType.DemonsSouls)
        {
            ret.AssetPath = GetAssetPath($@"map\{mapid}\{model}.nvm");
            ret.AssetName = model;
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
            ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.nvm";
        }
        else
        {
            return GetNullAsset();
        }

        return ret;
    }

    public AssetDescription GetHavokNavmeshes(string mapid)
    {
        AssetDescription ret = new();
        ret.AssetPath = GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public AssetDescription GetHavokNavmeshModel(string mapid, string model)
    {
        AssetDescription ret = new();
        ret.AssetPath = GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = model;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        ret.AssetVirtualPath = $@"map/{mapid}/nav/{model}.hkx";

        return ret;
    }

    public List<string> GetChrModels()
    {
        try
        {
            HashSet<string> chrs = new();
            List<string> ret = new();

            var modelDir = @"\chr";
            var modelExt = @".chrbnd.dcx";
            if (Type == GameType.DarkSoulsPTDE)
            {
                modelExt = ".chrbnd";
            }
            else if (Type == GameType.DarkSoulsIISOTFS)
            {
                modelDir = @"\model\chr";
                modelExt = ".bnd";
            }

            if (Type == GameType.DemonsSouls)
            {
                var chrdirs = Directory.GetDirectories(GameRootDirectory + modelDir);
                foreach (var f in chrdirs)
                {
                    var name = Path.GetFileNameWithoutExtension(f + ".dummy");
                    if (name.StartsWith("c"))
                    {
                        ret.Add(name);
                    }
                }

                return ret;
            }

            List<string> chrfiles = Directory.GetFileSystemEntries(GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in chrfiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                chrs.Add(name);
            }

            if (GameModDirectory != null && Directory.Exists(GameModDirectory + modelDir))
            {
                chrfiles = Directory.GetFileSystemEntries(GameModDirectory + modelDir, $@"*{modelExt}").ToList();
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

    public AssetDescription GetChrModel(string chr)
    {
        AssetDescription ret = new();
        ret.AssetName = chr;
        ret.AssetArchiveVirtualPath = $@"chr/{chr}/model";
        if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flv";
        }
        else
        {
            ret.AssetVirtualPath = $@"chr/{chr}/model/{chr}.flver";
        }

        return ret;
    }

    public List<string> GetObjModels()
    {
        try
        {
            HashSet<string> objs = new();
            List<string> ret = new();

            var modelDir = @"\obj";
            var modelExt = @".objbnd.dcx";
            if (Type == GameType.DarkSoulsPTDE)
            {
                modelExt = ".objbnd";
            }
            else if (Type == GameType.DarkSoulsIISOTFS)
            {
                modelDir = @"\model\obj";
                modelExt = ".bnd";
            }
            else if (Type == GameType.EldenRing)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\aeg";
                modelExt = ".geombnd.dcx";
            }
            else if (Type == GameType.ArmoredCoreVI)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\environment\geometry";
                modelExt = ".geombnd.dcx";
            }

            // Directories to search for obj models
            List<string> searchDirs = new();
            if (Type == GameType.EldenRing)
            {
                searchDirs = Directory.GetFileSystemEntries(GameRootDirectory + modelDir, @"aeg*").ToList();
            }
            else
            {
                searchDirs.Add(GameRootDirectory + modelDir);
            }

            foreach (var searchDir in searchDirs)
            {
                List<string> objfiles = Directory.GetFileSystemEntries(searchDir, $@"*{modelExt}").ToList();
                foreach (var f in objfiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    ret.Add(name);
                    objs.Add(name);
                }

                if (GameModDirectory != null && Directory.Exists(searchDir))
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

    public AssetDescription GetObjModel(string obj)
    {
        AssetDescription ret = new();
        ret.AssetName = obj;
        ret.AssetArchiveVirtualPath = $@"obj/{obj}/model";
        if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flv";
        }
        else if (Type is GameType.EldenRing or GameType.ArmoredCoreVI)
        {
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flver";
        }

        return ret;
    }

    public AssetDescription GetObjTexture(string obj)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        ad.AssetArchiveVirtualPath = null;
        string path = null;
        if (Type == GameType.DarkSoulsPTDE)
        {
            path = GetOverridenFilePath($@"obj\{obj}.objbnd");
        }
        else if (Type is GameType.DemonsSouls or GameType.DarkSoulsRemastered or GameType.Bloodborne
                 or GameType.DarkSoulsIII or GameType.Sekiro)
        {
            path = GetOverridenFilePath($@"obj\{obj}.objbnd.dcx");
        }

        if (path != null)
        {
            ad.AssetPath = path;
            ad.AssetArchiveVirtualPath = $@"obj/{obj}/tex";
        }

        return ad;
    }

    public List<string> GetPartsModels()
    {
        try
        {
            HashSet<string> parts = new();
            List<string> ret = new();

            var modelDir = @"\parts";
            var modelExt = @".partsbnd.dcx";
            if (Type == GameType.DarkSoulsPTDE)
            {
                modelExt = ".partsbnd";
            }
            else if (Type == GameType.DarkSoulsIISOTFS)
            {
                modelDir = @"\model\parts";
                modelExt = ".bnd";
                var partsGatheredFiles =
                    Directory.GetFiles(GameRootDirectory + modelDir, "*", SearchOption.AllDirectories);
                foreach (var f in partsGatheredFiles)
                {
                    if (!f.EndsWith("common.commonbnd.dcx") && !f.EndsWith("common_cloth.commonbnd.dcx") &&
                        !f.EndsWith("facepreset.bnd"))
                    {
                        ret.Add(Path.GetFileNameWithoutExtension(f));
                    }
                }

                return ret;
            }

            List<string> partsFiles = Directory.GetFileSystemEntries(GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in partsFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                parts.Add(name);
            }

            if (GameModDirectory != null && Directory.Exists(GameModDirectory + modelDir))
            {
                partsFiles = Directory.GetFileSystemEntries(GameModDirectory + modelDir, $@"*{modelExt}").ToList();
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

    public AssetDescription GetPartsModel(string part)
    {
        AssetDescription ret = new();
        ret.AssetName = part;
        ret.AssetArchiveVirtualPath = $@"parts/{part}/model";
        if (Type == GameType.DarkSoulsIISOTFS)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flv";
        }
        else if (Type is GameType.DarkSoulsPTDE)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flver";
        }

        return ret;
    }

    public AssetDescription GetPartTextures(string partsId)
    {
        AssetDescription ad = new();
        ad.AssetArchiveVirtualPath = null;
        ad.AssetPath = null;
        if (Type == GameType.ArmoredCoreVI)
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
            {
                path = GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");
            }

            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Type == GameType.EldenRing)
        {
            // Maybe add an option down the line to load lower quality
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Type == GameType.DarkSoulsIII || Type == GameType.Sekiro)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Type == GameType.Bloodborne)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Type == GameType.DarkSoulsPTDE)
        {
            var path = GetOverridenFilePath($@"parts\{partsId}.partsbnd");
            if (path != null)
            {
                ad.AssetPath = path;
                ad.AssetArchiveVirtualPath = $@"parts/{partsId}/tex";
            }
        }
        else if (Type == GameType.DemonsSouls)
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

    public AssetDescription GetNullAsset()
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
    public string VirtualToRealPath(string virtualPath, out string bndpath)
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
                if (Type == GameType.DarkSoulsIISOTFS)
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
                else if (Type == GameType.DemonsSouls)
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
                        if (Type == GameType.DarkSoulsRemastered)
                        {
                            return GetAssetPath($@"map\{mid}\GI_EnvM_{mid}.tpf.dcx");
                        }

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
                    if (Type == GameType.DarkSoulsPTDE)
                    {
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver");
                    }

                    if (Type == GameType.DarkSoulsRemastered)
                    {
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");
                    }

                    if (Type == GameType.DarkSoulsIISOTFS)
                    {
                        return GetAssetPath($@"model\map\{mapid}.mapbhd");
                    }

                    if (Type == GameType.Bloodborne || Type == GameType.DemonsSouls)
                    {
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}.flver.dcx");
                    }

                    if (Type == GameType.EldenRing)
                    {
                        return GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");
                    }

                    if (Type == GameType.ArmoredCoreVI)
                    {
                        return GetAssetPath($@"map\{mapid.Substring(0, 3)}\{mapid}\{pathElements[i]}.mapbnd.dcx");
                    }

                    return GetAssetPath($@"map\{mapid}\{pathElements[i]}.mapbnd.dcx");
                }

                if (pathElements[i].Equals("hit"))
                {
                    i++;
                    var hittype = pathElements[i];
                    i++;
                    if (Type == GameType.DarkSoulsPTDE || Type == GameType.DemonsSouls)
                    {
                        bndpath = "";
                        return GetAssetPath($@"map\{mapid}\{pathElements[i]}");
                    }

                    if (Type == GameType.DarkSoulsIISOTFS)
                    {
                        bndpath = "";
                        return GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
                    }

                    if (Type == GameType.DarkSoulsIII || Type == GameType.Bloodborne)
                    {
                        bndpath = "";
                        if (hittype == "lo")
                        {
                            return GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
                        }

                        return GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                    }

                    bndpath = "";
                    return null;
                }

                if (pathElements[i].Equals("nav"))
                {
                    i++;
                    if (Type == GameType.DarkSoulsPTDE || Type == GameType.DemonsSouls ||
                        Type == GameType.DarkSoulsRemastered)
                    {
                        if (i < pathElements.Length)
                        {
                            bndpath = $@"{pathElements[i]}";
                        }
                        else
                        {
                            bndpath = "";
                        }

                        if (Type == GameType.DarkSoulsRemastered)
                        {
                            return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd.dcx");
                        }

                        return GetAssetPath($@"map\{mapid}\{mapid}.nvmbnd");
                    }

                    if (Type == GameType.DarkSoulsIII)
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
                if (Type == GameType.DarkSoulsPTDE)
                {
                    return GetOverridenFilePath($@"chr\{chrid}.chrbnd");
                }

                if (Type == GameType.DarkSoulsIISOTFS)
                {
                    return GetOverridenFilePath($@"model\chr\{chrid}.bnd");
                }

                if (Type == GameType.DemonsSouls)
                {
                    return GetOverridenFilePath($@"chr\{chrid}\{chrid}.chrbnd.dcx");
                }

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
                if (Type == GameType.DarkSoulsPTDE)
                {
                    return GetOverridenFilePath($@"obj\{objid}.objbnd");
                }

                if (Type == GameType.DarkSoulsIISOTFS)
                {
                    return GetOverridenFilePath($@"model\obj\{objid}.bnd");
                }

                if (Type == GameType.EldenRing)
                {
                    // Derive subfolder path from model name (all vanilla AEG are within subfolders)
                    if (objid.Length >= 6)
                    {
                        return GetOverridenFilePath($@"asset\aeg\{objid.Substring(0, 6)}\{objid}.geombnd.dcx");
                    }

                    return null;
                }

                if (Type == GameType.ArmoredCoreVI)
                {
                    if (objid.Length >= 6)
                    {
                        return GetOverridenFilePath($@"asset\environment\geometry\{objid}.geombnd.dcx");
                    }

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
                if (Type == GameType.DarkSoulsPTDE || Type == GameType.DarkSoulsRemastered)
                {
                    return GetOverridenFilePath($@"parts\{partsId}.partsbnd");
                }

                if (Type == GameType.DarkSoulsIISOTFS)
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

                if (Type == GameType.EldenRing)
                {
                    return GetOverridenFilePath($@"parts\{partsId}\{partsId}.partsbnd.dcx");
                }

                if (Type == GameType.ArmoredCoreVI && pathElements[i].Equals("tex"))
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
                    {
                        path = GetOverridenFilePath($@"parts\{partsId}_u.tpf.dcx");
                    }

                    return path;
                }

                return GetOverridenFilePath($@"parts\{partsId}.partsbnd.dcx");
            }
        }

        bndpath = virtualPath;
        return null;
    }

    public string GetBinderVirtualPath(string virtualPathToBinder, string binderFilePath)
    {
        var filename = Path.GetFileNameWithoutExtension($@"{binderFilePath}.blah");
        if (filename.Length > 0)
        {
            filename = $@"{virtualPathToBinder}/{filename}";
        }
        else
        {
            filename = virtualPathToBinder;
        }

        return filename;
    }
}
