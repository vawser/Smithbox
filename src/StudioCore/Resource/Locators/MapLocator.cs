using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Resource.Locators;
public static class MapLocator
{
    public static List<string> FullMapList;

    /// <summary>
    /// Get a MSB asset.
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="writemode"></param>
    /// <returns></returns>
    public static ResourceDescriptor GetMapMSB(ProjectEntry project, string mapid, bool writemode = false, bool gameRootOnly = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;
        if (mapid.Length != 12 &&
            project.ProjectType != ProjectType.AC4 &&
            project.ProjectType != ProjectType.ACFA &&
            project.ProjectType != ProjectType.ACV &&
            project.ProjectType != ProjectType.ACVD)
            return ad;

        string preferredPath = "";
        string backupPath = "";

        // SOFTS
        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            preferredPath = $@"map\{mapid}\{mapid}.msb";
            backupPath = $@"map\{mapid}\{mapid}.msb";
        }
        // BB chalice maps
        else if (project.ProjectType is ProjectType.BB && mapid.StartsWith("m29"))
        {
            preferredPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid.Substring(0, 9)}_00\{mapid}.msb";
        }
        // DeS, DS1, DS1R
        else if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb";
            backupPath = $@"\map\MapStudio\{mapid}.msb.dcx";
        }
        // BB, DS3, ER, SDT, AC6
        else if (project.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.ER or ProjectType.SDT or ProjectType.AC6)
        {
            preferredPath = $@"\map\MapStudio\{mapid}.msb.dcx";
            backupPath = $@"\map\MapStudio\{mapid}.msb";
        }
        // ACFA
        else if (project.ProjectType is ProjectType.ACFA)
        {
            if (mapid.StartsWith("model") || mapid.StartsWith("system"))
            {
                // system maps
                preferredPath = $@"model\system\{mapid}.msb";
                backupPath = $@"model\system\{mapid}.msb";
            }
            else if (!mapid.StartsWith('m'))
            {
                // test maps starting without "m"
                if (mapid.Length < 3)
                    throw new IndexOutOfRangeException($"Map ID is too short: {mapid}");

                preferredPath = $@"model\map\m{mapid[..3]}\{mapid}.msb";
                backupPath = $@"bind\event\m{mapid[..3]}_event.bnd";
            }
            else
            {
                // normal maps
                if (mapid.Length < 4)
                    throw new IndexOutOfRangeException($"Map ID is too short: {mapid}");

                preferredPath = $@"model\map\{mapid[..4]}\{mapid}.msb";
                backupPath = $@"bind\event\{mapid[..4]}_event.bnd";
            }
        }
        // ACV
        else if (project.ProjectType is ProjectType.ACV)
        {
            if (mapid.StartsWith("ingamegarage"))
            {
                // garage maps
                preferredPath = $@"\model\map\ingamegarage\{mapid}.msb";
                backupPath = $@"\model\map\ingamegarage\{mapid}.msb";
            }
            else if (mapid.StartsWith("worldtop"))
            {
                // main menu map
                preferredPath = $@"\model\map\worldtop\{mapid}.msb";
                backupPath = $@"\model\map\worldtop\{mapid}.msb";
            }
            else if (mapid.EndsWith("_env"))
            {
                // env maps
                preferredPath = $@"\model\map\ch_env\{mapid}.msb";
                backupPath = $@"\model\map\ch_env\{mapid}.msb";
            }
            else if (mapid.Length > 5)
            {
                // maps with longer names
                preferredPath = $@"\model\map\{mapid[..5]}\{mapid}.msb";
                backupPath = $@"\model\map\{mapid[..5]}\{mapid}.msb";
            }
            else
            {
                // normal maps
                preferredPath = $@"\model\map\{mapid}\{mapid}.msb";
                backupPath = $@"\model\map\{mapid}\{mapid}.msb";
            }
        }
        // ACVD
        else if (project.ProjectType is ProjectType.ACVD)
        {
            if (mapid.StartsWith("ch"))
            {
                // mission maps
                preferredPath = $@"\model\map\ch_mission\{mapid}.msb";
                backupPath = $@"\model\map\ch_mission\{mapid}.msb";
            }
            else if (mapid.StartsWith("ingamegarage"))
            {
                // garage maps
                preferredPath = $@"\model\map\ingamegarage\{mapid}.msb";
                backupPath = $@"\model\map\ingamegarage\{mapid}.msb";
            }
            else if (mapid.StartsWith("worldtop"))
            {
                // main menu map
                preferredPath = $@"\model\map\worldtop\{mapid}.msb";
                backupPath = $@"\model\map\worldtop\{mapid}.msb";
            }
            else if (mapid.EndsWith("_env"))
            {
                // env maps
                preferredPath = $@"\model\map\ch_env\{mapid}.msb";
                backupPath = $@"\model\map\ch_env\{mapid}.msb";
            }
            else if (mapid.Length > 5)
            {
                // maps with longer names
                preferredPath = $@"\model\map\{mapid[..5]}\{mapid}.msb";
                backupPath = $@"\model\map\{mapid[..5]}\{mapid}.msb";
            }
            else
            {
                // normal maps
                preferredPath = $@"\model\map\{mapid}\{mapid}.msb";
                backupPath = $@"\model\map\{mapid}\{mapid}.msb";
            }
        }

        if (!gameRootOnly && project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{preferredPath}") || writemode && project.ProjectPath != null)
        {
            ad.AssetPath = $@"{project.ProjectPath}\{preferredPath}";
        }
        else if (!gameRootOnly && project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{backupPath}") || writemode && project.ProjectPath != null)
        {
            ad.AssetPath = $@"{project.ProjectPath}\{backupPath}";
        }
        else if (File.Exists($@"{project.DataPath}\{preferredPath}"))
        {
            ad.AssetPath = $@"{project.DataPath}\{preferredPath}";
        }
        else if (File.Exists($@"{project.DataPath}\{backupPath}"))
        {
            ad.AssetPath = $@"{project.DataPath}\{backupPath}";
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
    public static List<ResourceDescriptor> GetMapBTLs(ProjectEntry project, string mapid, bool writemode = false)
    {
        List<ResourceDescriptor> adList = new();
        if (mapid.Length != 12)
            return adList;

        if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            // DS2 BTL is located inside map's .gibdt file
            ResourceDescriptor ad = new();
            var path = $@"model\map\g{mapid[1..]}.gibhd";

            if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}") || writemode && project.ProjectPath != null)
            {
                ad.AssetPath = $@"{project.ProjectPath}\{path}";
            }
            else if (File.Exists($@"{project.DataPath}\{path}"))
            {
                ad.AssetPath = $@"{project.DataPath}\{path}";
            }

            if (ad.AssetPath != null)
            {
                ad.AssetName = $@"g{mapid[1..]}";
                ad.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad);
            }

            ResourceDescriptor ad2 = new();
            path = $@"model_lq\map\g{mapid[1..]}.gibhd";

            if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}") || writemode && project.ProjectPath != null)
            {
                ad2.AssetPath = $@"{project.ProjectPath}\{path}";
            }
            else if (File.Exists($@"{project.DataPath}\{path}"))
            {
                ad2.AssetPath = $@"{project.DataPath}\{path}";
            }

            if (ad2.AssetPath != null)
            {
                ad2.AssetName = $@"g{mapid[1..]}_lq";
                ad2.AssetVirtualPath = $@"{mapid}\light.btl.dcx";
                adList.Add(ad2);
            }
        }
        else if (project.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            string path;
            if (project.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                path = $@"map\{mapid[..3]}\{mapid}";
            }
            else
            {
                path = $@"map\{mapid}";
            }

            List<string> files = new();

            if (Directory.Exists($@"{project.DataPath}\{path}"))
            {
                files.AddRange(Directory.GetFiles($@"{project.DataPath}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{project.DataPath}\{path}", "*.btl.dcx").ToList());
            }

            if (Directory.Exists($@"{project.ProjectPath}\{path}"))
            {
                // Check for additional BTLs the user has created.
                files.AddRange(Directory.GetFiles($@"{project.ProjectPath}\{path}", "*.btl").ToList());
                files.AddRange(Directory.GetFiles($@"{project.ProjectPath}\{path}", "*.btl.dcx").ToList());
                files = files.DistinctBy(f => f.Split("\\").Last()).ToList();
            }

            foreach (var file in files)
            {
                ResourceDescriptor ad = new();
                var fileName = file.Split("\\").Last();

                if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}\{fileName}") || writemode && project.ProjectPath != null)
                {
                    ad.AssetPath = $@"{project.ProjectPath}\{path}\{fileName}";
                }
                else if (File.Exists($@"{project.DataPath}\{path}\{fileName}"))
                {
                    ad.AssetPath = $@"{project.DataPath}\{path}\{fileName}";
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
    public static ResourceDescriptor GetMapNVA(ProjectEntry project, string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        ad.AssetPath = null;

        if (mapid.Length != 12)
            return ad;

        if (project.ProjectType == ProjectType.BB && mapid.StartsWith("m29"))
        {
            var path = $@"\map\{mapid.Substring(0, 9)}_00\{mapid}";

            if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}.nva.dcx") || writemode && project.ProjectPath != null && project.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $@"{project.ProjectPath}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{project.DataPath}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{project.DataPath}\{path}.nva.dcx";
            }
        }
        else
        {
            var path = $@"\map\{mapid}\{mapid}";

            if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}.nva.dcx") || writemode && project.ProjectPath != null && project.ProjectType != ProjectType.DS1)
            {
                ad.AssetPath = $@"{project.ProjectPath}\{path}.nva.dcx";
            }
            else if (File.Exists($@"{project.DataPath}\{path}.nva.dcx"))
            {
                ad.AssetPath = $@"{project.DataPath}\{path}.nva.dcx";
            }
            else if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}.nva") || writemode && project.ProjectPath != null)
            {
                ad.AssetPath = $@"{project.ProjectPath}\{path}.nva";
            }
            else if (File.Exists($@"{project.DataPath}\{path}.nva"))
            {
                ad.AssetPath = $@"{project.DataPath}\{path}.nva";
            }
        }

        ad.AssetName = mapid;
        return ad;
    }

    /// <summary>
    /// Gets the full list of maps in the game. 
    /// Basically if there's an msb for it, it will be in this list.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetFullMapList(ProjectEntry project)
    {
        if (project.DataPath == null)
            return null;

        if (FullMapList != null)
            return FullMapList;

        try
        {
            HashSet<string> mapSet = [];
            bool checkRegex = true;
            void SearchLooseMaps(string dir, string wildcard)
            {
                if (!Directory.Exists(dir))
                {
                    return;
                }

                foreach (string path in Directory.EnumerateFiles(dir, wildcard, SearchOption.AllDirectories))
                {
                    mapSet.Add(Path.GetFileNameWithoutExtension(path));
                }
            }

            // DS2, ACFA, ACV, and ACVD have their own structure for msbs, where they are all inside individual folders
            if (project.ProjectType is ProjectType.ACFA)
            {
                SearchLooseMaps(project.DataPath + @"\model\map", "*.msb");
                SearchLooseMaps(project.DataPath + @"\model\system", "*.msb");
                if (project.ProjectPath != null)
                {
                    SearchLooseMaps(project.ProjectPath + @"\model\map", "*.msb");
                    SearchLooseMaps(project.ProjectPath + @"\model\system", "*.msb");
                }

                checkRegex = false;
            }
            else if (project.ProjectType is ProjectType.ACV)
            {
                SearchLooseMaps(project.DataPath + @"\model\map", "*.msb");
                if (project.ProjectPath != null)
                {
                    SearchLooseMaps(project.ProjectPath + @"\model\map", "*.msb");
                }

                checkRegex = false;
            }
            else if (project.ProjectType is ProjectType.ACVD)
            {
                SearchLooseMaps(project.DataPath + @"\model\map", "*.msb");
                if (project.ProjectPath != null)
                {
                    SearchLooseMaps(project.ProjectPath + @"\model\map", "*.msb");
                }

                // Remove a few ACV format MSBs
                mapSet.Remove("m3107");
                mapSet.Remove("m3107_map"); // Could be loaded as ACV or ACVD, but is still companion to an ACV format map
                mapSet.Remove("map_template");
                mapSet.Remove("map_template_map");
                mapSet.Remove("ingamegaragemenu0320_env");
                mapSet.Remove("ingamegaragemenu0350_env");

                checkRegex = false;
            }
            else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            {
                SearchLooseMaps(project.DataPath + @"\map", @"m*");
                if (project.ProjectPath != null)
                {
                    SearchLooseMaps(project.ProjectPath + @"\map", @"m*");
                }
            }
            else
            {
                var msbFiles = Directory.GetFileSystemEntries(project.DataPath + @"\map\MapStudio\", @"*.msb")
                    .Select(Path.GetFileNameWithoutExtension).ToList();

                msbFiles.AddRange(Directory.GetFileSystemEntries(project.DataPath + @"\map\MapStudio\", @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());

                if(project.ProjectType is ProjectType.BB)
                {
                    string[] folders = Directory.GetDirectories(project.DataPath + @"\map\MapStudio\", "m29_*");
                    foreach (var folder in folders)
                    {
                        msbFiles.AddRange(Directory.GetFileSystemEntries(folder, @"*.msb.dcx")
                        .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());
                    }
                }

                if (project.ProjectPath != null && Directory.Exists(project.ProjectPath + @"\map\MapStudio\"))
                {
                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(project.ProjectPath + @"\map\MapStudio\", @"*.msb")
                        .Select(Path.GetFileNameWithoutExtension).ToList());

                    msbFiles.AddRange(Directory
                        .GetFileSystemEntries(project.ProjectPath + @"\map\MapStudio\", @"*.msb.dcx")
                        .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                        .ToList());

                    if (project.ProjectType is ProjectType.BB)
                    {
                        string[] folders = Directory.GetDirectories(project.ProjectPath + @"\map\MapStudio\", "m29_*");
                        foreach (var folder in folders)
                        {
                            msbFiles.AddRange(Directory.GetFileSystemEntries(folder, @"*.msb.dcx")
                            .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());
                        }
                    }
                }

                foreach (var msb in msbFiles)
                    mapSet.Add(msb);
            }

            List<string> mapList;
            if (checkRegex)
                mapList = mapSet.Where(x => GeneratedRegexMethods.DSMapRegex().IsMatch(x)).ToList();
            else
                mapList = mapSet.ToList();

            mapList.Sort();
            FullMapList = mapList;
            return FullMapList;
        }
        catch (DirectoryNotFoundException e)
        {
            TaskLogs.AddLog($"[Smithbox] Failed to find map files.", LogLevel.Error, Tasks.LogPriority.High, e);

            // Game is likely not UXM unpacked
            return new List<string>();
        }
    }

    /// <summary>
    /// Get list of maps for target project path
    /// </summary>
    /// <returns></returns>
    public static List<string> GetMapList(ProjectEntry project, string projectPath)
    {
        HashSet<string> mapSet = new();

        bool checkRegex = true;
        void SearchLooseMaps(string dir, string wildcard)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (string path in Directory.EnumerateFiles(dir, wildcard, SearchOption.AllDirectories))
            {
                mapSet.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        // DS2, ACFA, ACV, and ACVD have their own structure for msbs, where they are all inside individual folders
        if (project.ProjectType is ProjectType.ACFA)
        {
            SearchLooseMaps(projectPath + @"\model\map", "*.msb");
            SearchLooseMaps(projectPath + @"\model\system", "*.msb");

            checkRegex = false;
        }
        else if (project.ProjectType is ProjectType.ACV)
        {
            SearchLooseMaps(projectPath + @"\model\map", "*.msb");

            checkRegex = false;
        }
        else if (project.ProjectType is ProjectType.ACVD)
        {
            SearchLooseMaps(projectPath + @"\model\map", "*.msb");

            // Remove a few ACV format MSBs
            mapSet.Remove("m3107");
            mapSet.Remove("m3107_map"); // Could be loaded as ACV or ACVD, but is still companion to an ACV format map
            mapSet.Remove("map_template");
            mapSet.Remove("map_template_map");
            mapSet.Remove("ingamegaragemenu0320_env");
            mapSet.Remove("ingamegaragemenu0350_env");

            checkRegex = false;
        }
        else if (project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            SearchLooseMaps(projectPath + @"\map", @"m*");
        }
        else
        {
            var msbFiles = Directory
                .GetFileSystemEntries(projectPath + @"\map\MapStudio\", @"*.msb")
                .Select(Path.GetFileNameWithoutExtension).ToList();

            msbFiles.AddRange(Directory
                .GetFileSystemEntries(projectPath + @"\map\MapStudio\", @"*.msb.dcx")
                .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());

            if (project.ProjectType is ProjectType.BB)
            {
                string[] folders = Directory.GetDirectories(projectPath + @"\map\MapStudio\", "m29_*");
                foreach (var folder in folders)
                {
                    msbFiles.AddRange(Directory.GetFileSystemEntries(folder, @"*.msb.dcx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension).ToList());
                }
            }

            foreach (var msb in msbFiles)
                mapSet.Add(msb);
        }

        List<string> mapList;
        if (checkRegex)
            mapList = mapSet.Where(x => GeneratedRegexMethods.DSMapRegex().IsMatch(x)).ToList();
        else
            mapList = mapSet.ToList();

        mapList.Sort();
        return mapList;
    }

    /// <summary>
    /// Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(ProjectEntry project, string mapid)
    {
        if (project.ProjectType is ProjectType.DES or ProjectType.ER or ProjectType.AC6)
        {
            return mapid;
        }
        else if (project.ProjectType is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (project.ProjectType is ProjectType.BB)
        {
            if (mapid.StartsWith("m29"))
            {
                // Special case for chalice dungeon assets
                return "m29_00_00_00";
            }
        }
        else if (project.ProjectType is ProjectType.ACFA)
        {
            return mapid[..4];
        }
        else if (project.ProjectType is ProjectType.ACV)
        {
            return mapid[..5];
        }
        else if (project.ProjectType is ProjectType.ACVD)
        {
            if (mapid.Length == 12 && mapid.StartsWith("ch"))
            {
                return string.Concat(mapid.AsSpan(7, 4), "0");
            }

            return mapid[..4] + '0';
        }

        // Default
        return mapid.Substring(0, 6) + "_00_00";
    }

    /// <summary>
    /// Get a BTAB asset.
    /// </summary>
    public static List<ResourceDescriptor> GetMapBTABs(ProjectEntry project, string mapid)
    {
        List<ResourceDescriptor> resourceDescriptors = new();

        var rootDirectory = $"{project.DataPath}\\map\\{mapid}";
        var projectDirectory = $"{project.ProjectPath}\\map\\{mapid}";

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
        foreach (var name in names)
        {
            var path = LocatorUtils.GetAssetPath(project, $"\\map\\{mapid}\\{name}.btab.dcx");
            paths.Add(path);
        }

        foreach (var path in paths)
        {
            ResourceDescriptor resource = new ResourceDescriptor();

            resource.AssetPath = path;

            resourceDescriptors.Add(resource);
        }

        return resourceDescriptors;
    }

    /// <summary>
    /// Get a HKX Collision asset.
    /// </summary>
    public static List<ResourceDescriptor> GetMapCollisions(ProjectEntry project, string mapid)
    {
        List<ResourceDescriptor> resourceDescriptors = new();

        var rootDirectory = $"{project.DataPath}\\map\\{mapid.Substring(0, 3)}\\{mapid}";
        var projectDirectory = $"{project.ProjectPath}\\map\\{mapid.Substring(0, 3)}\\{mapid}";

        // Get the names
        var names = new List<string>();

        if (Directory.Exists(rootDirectory))
        {
            foreach (var file in Directory.GetFiles(rootDirectory))
            {
                var path = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));

                if (file.Contains(".hkxbhd"))
                    names.Add(path.Replace(".hkxbhd", ""));
            }
        }

        if (Directory.Exists(projectDirectory))
        {
            foreach (var file in Directory.GetFiles(projectDirectory))
            {
                var path = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                if (file.Contains(".hkxbhd"))
                {
                    if (!names.Contains(path))
                        names.Add(path.Replace(".hkxbhd", ""));
                }
            }
        }

        var paths = new List<string>();

        // Get the resource descriptors
        foreach (var name in names)
        {
            var path = LocatorUtils.GetAssetPath(project, $"\\map\\{mapid}\\{name}.hkxbhd");
            paths.Add(path);
        }

        foreach (var path in paths)
        {
            ResourceDescriptor resource = new ResourceDescriptor();

            resource.AssetPath = path;

            resourceDescriptors.Add(resource);
        }

        return resourceDescriptors;
    }
}
