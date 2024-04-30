using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.UserProject.Locators;
public static class MapAssetLocator
{
    public static List<string> FullMapList;

    /// <summary>
    /// Get a MSB asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static AssetDescription GetMapMSB(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
            return ad;

        string preferredPath = "";
        string backupPath = "";

        // SOFTS
        if (Project.Type == ProjectType.DS2S)
        {
            preferredPath = $@"map\{mapid}\{mapid}.msb";
            backupPath = $@"map\{mapid}\{mapid}.msb";
        }
        // BB chalice maps
        else if (Project.Type == ProjectType.BB && mapid.StartsWith("m29"))
        {
            preferredPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb";
        }
        // DeS, DS1, DS1R
        else if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R || Project.Type == ProjectType.DES)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb";
            backupPath = $@"\map\MapStudio\{mapid}.msb.dcx";
        }
        // BB, DS3, ER, SDT, AC6
        else if (Project.Type is ProjectType.BB or ProjectType.DS3 or ProjectType.ER or ProjectType.SDT or ProjectType.AC6)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }

        if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{preferredPath}") || writemode && Project.GameModDirectory != null)
        {
            ad.AssetPath = $@"{Project.GameModDirectory}\{preferredPath}";
        }
        else if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{backupPath}") || writemode && Project.GameModDirectory != null)
        {
            ad.AssetPath = $@"{Project.GameModDirectory}\{backupPath}";
        }
        else if (File.Exists($@"{Project.GameRootDirectory}\{preferredPath}"))
        {
            ad.AssetPath = $@"{Project.GameRootDirectory}\{preferredPath}";
        }
        else if (File.Exists($@"{Project.GameRootDirectory}\{backupPath}"))
        {
            ad.AssetPath = $@"{Project.GameRootDirectory}\{backupPath}";
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    /// Get a BTL asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static List<AssetDescription> GetMapBTLs(string mapid, bool writemode = false)
    {
        List<AssetDescription> adList = new();
        if (mapid.Length != 12)
            return adList;

        if (Project.Type is ProjectType.DS2S)
        {
            // DS2 BTL is located inside map's .gibdt file
            AssetDescription ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}") || writemode && Project.GameModDirectory != null)
            {
                ad.AssetPath = $@"{Project.GameModDirectory}\{path}";
            }
            else if (File.Exists($@"{Project.GameRootDirectory}\{path}"))
            {
                ad.AssetPath = $@"{Project.GameRootDirectory}\{path}";
            }

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            AssetDescription ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}") || writemode && Project.GameModDirectory != null)
            {
                ad2.AssetPath = $@"{Project.GameModDirectory}\{path}";
            }
            else if (File.Exists($@"{Project.GameRootDirectory}\{path}"))
            {
                ad2.AssetPath = $@"{Project.GameRootDirectory}\{path}";
            }

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (Project.Type is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            string path;
            if (Project.Type is ProjectType.ER or ProjectType.AC6)
            {
                path = $@"map\{mapid[..3]}\{mapid}";
            }
            else
            {
                path = $@"map\{mapid}";
            }

            List<string> files = new();

            if (Directory.Exists($@"{Project.GameRootDirectory}\{path}"))
            {
                files.AddRange(Directory.GetFiles($@"{Project.GameRootDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{Project.GameRootDirectory}\{path}", "*.btl.dcx").ToList());
            }

            if (Directory.Exists($@"{Project.GameModDirectory}\{path}"))
            {
                // Check for additional BTLs the user has created.
                files.AddRange(Directory.GetFiles($@"{Project.GameModDirectory}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{Project.GameModDirectory}\{path}", "*.btl.dcx").ToList());
                files = files.DistinctBy(f => f.Split("\\").Last()).ToList();
            }

            foreach (var file in files)
            {
                AssetDescription ad = new();
                var fileName = file.Split("\\").Last();

                if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}\{fileName}") || writemode && Project.GameModDirectory != null)
                {
                    ad.AssetPath = $@"{Project.GameModDirectory}\{path}\{fileName}";
                }
                else if (File.Exists($@"{Project.GameRootDirectory}\{path}\{fileName}"))
                {
                    ad.AssetPath = $@"{Project.GameRootDirectory}\{path}\{fileName}";
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

    /// <summary>
    /// Get a NVA asset
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static AssetDescription GetMapNVA(string mapid, bool writemode = false)
    {
        AssetDescription ad = new();
        ad.AssetPath = null;

        if (mapid.Length != 12)
            return ad;

        if (Project.Type == ProjectType.BB && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid.Substring(0, 9)}_00\{mapid}";

            if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}.nva.dcx") || writemode && Project.GameModDirectory != null && Project.Type != ProjectType.DS1)
            {
                ad.AssetPath = $@"{Project.GameModDirectory}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{Project.GameRootDirectory}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{Project.GameRootDirectory}\{path}.nva.dcx";
            }
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";

            if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}.nva.dcx") || writemode && Project.GameModDirectory != null && Project.Type != ProjectType.DS1)
            {
                ad.AssetPath = $@"{Project.GameModDirectory}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{Project.GameRootDirectory}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{Project.GameRootDirectory}\{path}.nva.dcx";
            }
            else if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}.nva") || writemode && Project.GameModDirectory != null)
            {
                ad.AssetPath = $@"{Project.GameModDirectory}\{path}.nva";
            }
            else if (File.Exists($@"{Project.GameRootDirectory}\{path}.nva"))
            {
                ad.AssetPath = $@"{Project.GameRootDirectory}\{path}.nva";
            }
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    /// Gets the full list of maps in the game (excluding chalice dungeons). 
    /// Basically if there's an msb for it, it will be in this list.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetFullMapList()
    {
        if (Project.GameRootDirectory == null)
            return null;

        if (FullMapList != null)
            return FullMapList;

        try
        {
            HashSet<string> mapSet = new();

            // DS2 has its own structure for msbs, where they are all inside individual folders
            if (Project.Type == ProjectType.DS2S)
            {
                var maps = Directory.GetFileSystemEntries(Project.GameRootDirectory + @"\map", @"m*").ToList();

                if (Project.GameModDirectory != null)
                {
                    if (Directory.Exists(Project.GameModDirectory + @"\map"))
                    {
                        maps.AddRange(Directory.GetFileSystemEntries(Project.GameModDirectory + @"\map", @"m*").ToList());
                    }
                }

                foreach (var map in maps)
                    mapSet.Add(Path.GetFileNameWithoutExtension($@"{map}.blah"));
            }
            else
            {
                var msbFiles = Directory
                    .GetFileSystemEntries(Project.GameRootDirectory + @"\map\MapStudio\", @"*.msb")
                    .Select(Path.GetFileNameWithoutExtension).ToList();

                msbFiles.AddRange(Directory
                    .GetFileSystemEntries(Project.GameRootDirectory + @"\map\MapStudio\", @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());

                if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + @"\map\MapStudio\"))
                {
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(Project.GameModDirectory + @"\map\MapStudio\", @"*.msb")
                        .Select(Path.GetFileNameWithoutExtension).ToList());

                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(Project.GameModDirectory + @"\map\MapStudio\", @"*.msb.dcx")
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

    /// <summary>
    /// Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(string mapid)
    {
        if (Project.Type is ProjectType.ER or ProjectType.AC6)
            return mapid;

        if (Project.Type is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (Project.Type is ProjectType.DES)
        {
            return mapid;
        }
        else if (Project.Type is ProjectType.BB)
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
}
