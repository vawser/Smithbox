using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;

public static class BrowserFileLocator
{
    public static List<ResourceDescriptor> GetMapModels(string mapid)
    {
        List<ResourceDescriptor> ret = new();
        if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.SDT)
        {
            if (!Directory.Exists(Project.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var mapfiles = Directory
                .GetFileSystemEntries(Project.GameRootDirectory + $@"\map\{mapid}\", @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            ResourceDescriptor ad = new();
            var name = mapid;
            ad.AssetName = name;
            ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/";
            ret.Add(ad);
        }
        else if (Project.Type == ProjectType.ER)
        {
            var mapPath = Project.GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
                return ret;

            var mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }
        else if (Project.Type == ProjectType.AC6)
        {
            var mapPath = Project.GameRootDirectory + $@"\map\{mapid[..3]}\{mapid}";
            if (!Directory.Exists(mapPath))
                return ret;

            var mapfiles = Directory.GetFileSystemEntries(mapPath, @"*.mapbnd.dcx").ToList();
            foreach (var f in mapfiles)
            {
                ResourceDescriptor ad = new();
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
            if (!Directory.Exists(Project.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var ext = Project.Type == ProjectType.DS1 ? @"*.flver" : @"*.flver.dcx";
            var mapfiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + $@"\map\{mapid}\", ext)
                .ToList();
            foreach (var f in mapfiles)
            {
                ResourceDescriptor ad = new();
                ad.AssetPath = f;
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ad.AssetName = name;
                // ad.AssetArchiveVirtualPath = $@"map/{mapid}/model/{name}";
                ad.AssetVirtualPath = $@"map/{mapid}/model/{name}/{name}.flver";
                ret.Add(ad);
            }
        }

        ret.Sort();

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

            if (Project.Type == ProjectType.DS1)
                modelExt = ".chrbnd";
            else if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            {
                modelDir = @"\model\chr";
                modelExt = ".bnd";
            }

            if (Project.Type == ProjectType.DES)
            {
                var chrdirs = Directory.GetDirectories(Project.GameRootDirectory + modelDir);
                foreach (var f in chrdirs)
                {
                    var name = Path.GetFileNameWithoutExtension(f + ".dummy");
                    if (name.StartsWith("c"))
                        ret.Add(name);
                }

                return ret;
            }

            var chrfiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in chrfiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                chrs.Add(name);
            }

            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + modelDir))
            {
                chrfiles = Directory.GetFileSystemEntries(Project.GameModDirectory + modelDir, $@"*{modelExt}").ToList();
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

            ret.Sort();

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static List<string> GetObjModels(bool useProject = false)
    {
        try
        {
            HashSet<string> objs = new();
            List<string> ret = new();

            var modelDir = @"\obj";
            var modelExt = @".objbnd.dcx";

            if (Project.Type == ProjectType.DS1)
                modelExt = ".objbnd";
            else if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            {
                modelDir = @"\model\obj";
                modelExt = ".bnd";
            }
            else if (Project.Type == ProjectType.ER)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\aeg";
                modelExt = ".geombnd.dcx";
            }
            else if (Project.Type == ProjectType.AC6)
            {
                // AEGs are objs in my heart :(
                modelDir = @"\asset\environment\geometry";
                modelExt = ".geombnd.dcx";
            }

            var rootDir = Project.GameRootDirectory + modelDir;
            var modDir = Project.GameModDirectory + modelDir;

            foreach (var f in Directory.GetFileSystemEntries(rootDir, $@"*{modelExt}").ToList())
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                objs.Add(name);
            }

            if (Project.Type == ProjectType.ER)
            {
                foreach (var folder in Directory.GetDirectories(rootDir).ToList())
                {
                    var tempRootDir = $@"{rootDir}\{folder.Substring(folder.Length - 6)}";

                    foreach (var f in Directory.GetFileSystemEntries(tempRootDir, $@"*{modelExt}").ToList())
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

            if (Project.GameModDirectory != null && Directory.Exists(modDir))
            {
                foreach (var f in Directory.GetFileSystemEntries(modDir, $@"*{modelExt}").ToList())
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    if (!objs.Contains(name))
                    {
                        ret.Add(name);
                        objs.Add(name);
                    }
                }

                if (Project.Type == ProjectType.ER)
                {
                    foreach (var folder in Directory.GetDirectories(modDir).ToList())
                    {
                        var tempModDir = $@"{modDir}\{folder.Substring(folder.Length - 6)}";

                        foreach (var f in Directory.GetFileSystemEntries(tempModDir, $@"*{modelExt}").ToList())
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
            }

            ret.Sort();

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static List<string> GetPartsModels()
    {
        try
        {
            HashSet<string> parts = new();
            List<string> ret = new();

            var modelDir = @"\parts";
            var modelExt = @".partsbnd.dcx";

            if (Project.Type == ProjectType.DS1)
            {
                modelExt = ".partsbnd";
            }
            else if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
            {
                modelDir = @"\model\parts";
                modelExt = ".bnd";
                var partsGatheredFiles = Directory.GetFiles(Project.GameRootDirectory + modelDir, "*", SearchOption.AllDirectories);

                foreach (var f in partsGatheredFiles)
                {
                    if (!f.EndsWith("common.commonbnd.dcx") && !f.EndsWith("common_cloth.commonbnd.dcx") &&
                        !f.EndsWith("facepreset.bnd"))
                        ret.Add(Path.GetFileNameWithoutExtension(f));
                }

                return ret;
            }

            var partsFiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + modelDir, $@"*{modelExt}")
                .ToList();

            foreach (var f in partsFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                parts.Add(name);
            }

            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + modelDir))
            {
                partsFiles = Directory.GetFileSystemEntries(Project.GameModDirectory + modelDir, $@"*{modelExt}").ToList();
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

            ret.Sort();

            return ret;
        }
        catch (DirectoryNotFoundException e)
        {
            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }
}
