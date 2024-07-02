using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class ResourceMapLocator
{
    public static List<string> FullMapList;

    /// <summary>
    /// Get a MSB asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static ResourceDescriptor GetMapMSB(string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12)
            return ad;

        string preferredPath = "";
        string backupPath = "";

        // SOFTS
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            preferredPath = $@"map\{mapid}\{mapid}.msb";
            backupPath = $@"map\{mapid}\{mapid}.msb";
        }
        // BB chalice maps
        else if (Smithbox.ProjectType == ProjectType.BB && mapid.StartsWith("m29"))
        {
            preferredPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb";
        }
        // DeS, DS1, DS1R
        else if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R || Smithbox.ProjectType == ProjectType.DES)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb";
            backupPath = $@"\map\MapStudio\{mapid}.msb.dcx";
        }
        // BB, DS3, ER, SDT, AC6
        else if (Smithbox.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.ER or ProjectType.SDT or ProjectType.AC6)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }

        if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{preferredPath}") || writemode && Smithbox.ProjectRoot != null)
        {
            ad.AssetPath = $@"{Smithbox.ProjectRoot}\{preferredPath}";
        }
        else if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{backupPath}") || writemode && Smithbox.ProjectRoot != null)
        {
            ad.AssetPath = $@"{Smithbox.ProjectRoot}\{backupPath}";
        }
        else if (File.Exists($@"{Smithbox.GameRoot}\{preferredPath}"))
        {
            ad.AssetPath = $@"{Smithbox.GameRoot}\{preferredPath}";
        }
        else if (File.Exists($@"{Smithbox.GameRoot}\{backupPath}"))
        {
            ad.AssetPath = $@"{Smithbox.GameRoot}\{backupPath}";
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
    public static List<ResourceDescriptor> GetMapBTLs(string mapid, bool writemode = false)
    {
        List<ResourceDescriptor> adList = new();
        if (mapid.Length != 12)
            return adList;

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            // DS2 BTL is located inside map's .gibdt file
            ResourceDescriptor ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}") || writemode && Smithbox.ProjectRoot != null)
            {
                ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}";
            }
            else if (File.Exists($@"{Smithbox.GameRoot}\{path}"))
            {
                ad.AssetPath = $@"{Smithbox.GameRoot}\{path}";
            }

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            ResourceDescriptor ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}") || writemode && Smithbox.ProjectRoot != null)
            {
                ad2.AssetPath = $@"{Smithbox.ProjectRoot}\{path}";
            }
            else if (File.Exists($@"{Smithbox.GameRoot}\{path}"))
            {
                ad2.AssetPath = $@"{Smithbox.GameRoot}\{path}";
            }

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (Smithbox.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            string path;
            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                path = $@"map\{mapid[..3]}\{mapid}";
            }
            else
            {
                path = $@"map\{mapid}";
            }

            List<string> files = new();

            if (Directory.Exists($@"{Smithbox.GameRoot}\{path}"))
            {
                files.AddRange(Directory.GetFiles($@"{Smithbox.GameRoot}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{Smithbox.GameRoot}\{path}", "*.btl.dcx").ToList());
            }

            if (Directory.Exists($@"{Smithbox.ProjectRoot}\{path}"))
            {
                // Check for additional BTLs the user has created.
                files.AddRange(Directory.GetFiles($@"{Smithbox.ProjectRoot}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{Smithbox.ProjectRoot}\{path}", "*.btl.dcx").ToList());
                files = files.DistinctBy(f => f.Split("\\").Last()).ToList();
            }

            foreach (var file in files)
            {
                ResourceDescriptor ad = new();
                var fileName = file.Split("\\").Last();

                if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}\{fileName}") || writemode && Smithbox.ProjectRoot != null)
                {
                    ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}\{fileName}";
                }
                else if (File.Exists($@"{Smithbox.GameRoot}\{path}\{fileName}"))
                {
                    ad.AssetPath = $@"{Smithbox.GameRoot}\{path}\{fileName}";
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
    public static ResourceDescriptor GetMapNVA(string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;

        if (mapid.Length != 12)
            return ad;

        if (Smithbox.ProjectType == ProjectType.BB && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid.Substring(0, 9)}_00\{mapid}";

            if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}.nva.dcx") || writemode && Smithbox.ProjectRoot != null && Smithbox.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{Smithbox.GameRoot}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{Smithbox.GameRoot}\{path}.nva.dcx";
            }
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";

            if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}.nva.dcx") || writemode && Smithbox.ProjectRoot != null && Smithbox.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{Smithbox.GameRoot}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{Smithbox.GameRoot}\{path}.nva.dcx";
            }
            else if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}.nva") || writemode && Smithbox.ProjectRoot != null)
            {
                ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}.nva";
            }
            else if (File.Exists($@"{Smithbox.GameRoot}\{path}.nva"))
            {
                ad.AssetPath = $@"{Smithbox.GameRoot}\{path}.nva";
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
        if (Smithbox.GameRoot == null)
            return null;

        if (FullMapList != null)
            return FullMapList;

        try
        {
            HashSet<string> mapSet = new();

            // DS2 has its own structure for msbs, where they are all inside individual folders
            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                var maps = Directory.GetFileSystemEntries(Smithbox.GameRoot + @"\map", @"m*").ToList();

                if (Smithbox.ProjectRoot != null)
                {
                    if (Directory.Exists(Smithbox.ProjectRoot + @"\map"))
                    {
                        maps.AddRange(Directory.GetFileSystemEntries(Smithbox.ProjectRoot + @"\map", @"m*").ToList());
                    }
                }

                foreach (var map in maps)
                    mapSet.Add(Path.GetFileNameWithoutExtension($@"{map}.blah"));
            }
            else
            {
                var msbFiles = Directory
                    .GetFileSystemEntries(Smithbox.GameRoot + @"\map\MapStudio\", @"*.msb")
                    .Select(Path.GetFileNameWithoutExtension).ToList();

                msbFiles.AddRange(Directory
                    .GetFileSystemEntries(Smithbox.GameRoot + @"\map\MapStudio\", @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());

                if (Smithbox.ProjectRoot != null && Directory.Exists(Smithbox.ProjectRoot + @"\map\MapStudio\"))
                {
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(Smithbox.ProjectRoot + @"\map\MapStudio\", @"*.msb")
                        .Select(Path.GetFileNameWithoutExtension).ToList());

                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(Smithbox.ProjectRoot + @"\map\MapStudio\", @"*.msb.dcx")
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
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            return mapid;

        if (Smithbox.ProjectType is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (Smithbox.ProjectType is ProjectType.DES)
        {
            return mapid;
        }
        else if (Smithbox.ProjectType is ProjectType.BB)
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

    /// <summary>
    /// Get a BTAB asset.
    /// </summary>
    public static List<ResourceDescriptor> GetMapBTABs(string mapid)
    {
        List<ResourceDescriptor> resourceDescriptors = new();

        var rootDirectory = $"{Smithbox.GameRoot}\\map\\{mapid}";
        var projectDirectory = $"{Smithbox.ProjectRoot}\\map\\{mapid}";

        // Get the names
        var names = new List<string>();

        if (Directory.Exists(rootDirectory))
        {
            foreach (var file in Directory.GetFiles(rootDirectory))
            {
                var path = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));

                if (file.Contains(".btab.dcx"))
                    names.Add(path);
            }
        }

        if (Directory.Exists(projectDirectory))
        {
            foreach (var file in Directory.GetFiles(projectDirectory))
            {
                var path = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                if (file.Contains(".btab.dcx"))
                {
                    if (!names.Contains(path))
                        names.Add(path);
                }
            }
        }

        var paths = new List<string>();

        // Get the resource descriptors
        foreach(var name in names)
        {
            var path = ResourceLocatorUtils.GetAssetPath($"\\map\\{mapid}\\{name}.btab.dcx");
            paths.Add(path);
        }

        foreach(var path in paths)
        {
            ResourceDescriptor resource = new ResourceDescriptor();

            resource.AssetPath = path;

            resourceDescriptors.Add(resource);
        }

        return resourceDescriptors;
    }
}
