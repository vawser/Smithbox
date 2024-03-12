
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.AssetLocator;
public static class ModelAssetLocator
{
    public static AssetDescription GetNullAsset()
    {
        AssetDescription ret = new();
        ret.AssetPath = "null";
        ret.AssetName = "null";
        ret.AssetArchiveVirtualPath = "null";
        ret.AssetVirtualPath = "null";
        return ret;
    }

    public static List<AssetDescription> GetMapModels(string mapid)
    {
        List<AssetDescription> ret = new();
        if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.SDT)
        {
            if (!Directory.Exists(Project.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var mapfiles = Directory
                .GetFileSystemEntries(Project.GameRootDirectory + $@"\map\{mapid}\", @"*.mapbnd.dcx").ToList();
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
        else if (Project.Type == ProjectType.DS2S)
        {
            AssetDescription ad = new();
            var name = mapid;
            ad.AssetName = name;
            ad.AssetArchiveVirtualPath = $@"map/{mapid}/model";
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
                AssetDescription ad = new();
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
            if (!Directory.Exists(Project.GameRootDirectory + $@"\map\{mapid}\"))
                return ret;

            var ext = Project.Type == ProjectType.DS1 ? @"*.flver" : @"*.flver.dcx";
            var mapfiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + $@"\map\{mapid}\", ext)
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
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
            return $@"{modelname}A{mapid.Substring(1, 2)}";

        if (Project.Type == ProjectType.DES)
            return $@"{modelname}";

        if (Project.Type == ProjectType.DS2S)
            return modelname;

        return $@"{mapid}_{modelname.Substring(1)}";
    }

    public static AssetDescription GetMapModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.BB || Project.Type == ProjectType.DES)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver");
        else if (Project.Type == ProjectType.DS1R)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.flver.dcx");
        else if (Project.Type == ProjectType.DS2S)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"model\map\{mapid}.mapbhd");
        else if (Project.Type == ProjectType.ER)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else if (Project.Type == ProjectType.AC6)
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid[..3]}\{mapid}\{model}.mapbnd.dcx");
        else
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.mapbnd.dcx");

        ret.AssetName = model;
        if (Project.Type == ProjectType.DS2S)
        {
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/model";
            ret.AssetVirtualPath = $@"map/{mapid}/model/{model}.flv.dcx";
        }
        else
        {
            if (Project.Type is not ProjectType.DES
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
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DES)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\l{model.Substring(1)}.hkx");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/lo/l{model.Substring(1)}.hkx";
            }
        }
        else if (Project.Type == ProjectType.DS2S)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath($@"model\map\h{mapid.Substring(1)}.hkxbhd");
            ret.AssetName = model;
            ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/{model}.hkx.dcx";
            ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
        }
        else if (Project.Type == ProjectType.DS3 || Project.Type == ProjectType.BB)
        {
            if (hi)
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\h{mapid.Substring(1)}.hkxbhd");
                ret.AssetName = model;
                ret.AssetVirtualPath = $@"map/{mapid}/hit/hi/h{model.Substring(1)}.hkx.dcx";
                ret.AssetArchiveVirtualPath = $@"map/{mapid}/hit/hi";
            }
            else
            {
                ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\l{mapid.Substring(1)}.hkxbhd");
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
    public static AssetDescription GetMapNVMModel(string mapid, string model)
    {
        AssetDescription ret = new();
        if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R || Project.Type == ProjectType.DES)
        {
            ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{model}.nvm");
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
        ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
        ret.AssetName = mapid;
        ret.AssetArchiveVirtualPath = $@"map/{mapid}/nav";
        return ret;
    }

    public static AssetDescription GetHavokNavmeshModel(string mapid, string model)
    {
        AssetDescription ret = new();
        ret.AssetPath = LocatorUtils.GetAssetPath($@"map\{mapid}\{mapid}.nvmhktbnd.dcx");
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
            if (Project.Type == ProjectType.DS1)
                modelExt = ".chrbnd";
            else if (Project.Type == ProjectType.DS2S)
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
        if (Project.Type == ProjectType.DS2S)
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
            if (Project.Type == ProjectType.DS1)
                modelExt = ".objbnd";
            else if (Project.Type == ProjectType.DS2S)
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

            // Directories to search for obj models
            List<string> searchDirs = new();
            List<string> searchRootDirs = new();
            List<string> searchModDirs = new();

            if (Project.Type == ProjectType.ER)
            {
                searchRootDirs = Directory.GetFileSystemEntries(Project.GameRootDirectory + modelDir, @"aeg*").ToList();
                searchModDirs = Directory.GetFileSystemEntries(Project.GameModDirectory + modelDir, @"aeg*").ToList();

                searchDirs = searchRootDirs.Concat(searchModDirs).ToList();
            }
            else
            {
                searchDirs.Add(Project.GameRootDirectory + modelDir);
            }

            foreach (var searchDir in searchDirs)
            {
                var objfiles = Directory.GetFileSystemEntries(searchDir, $@"*{modelExt}").ToList();
                foreach (var f in objfiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                    ret.Add(name);
                    objs.Add(name);
                }

                if (Project.GameModDirectory != null && Directory.Exists(searchDir))
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
        if (Project.Type == ProjectType.DS2S)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flv";
        else if (Project.Type is ProjectType.ER or ProjectType.AC6)
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj.ToUpper()}.flver";
        else
            ret.AssetVirtualPath = $@"obj/{obj}/model/{obj}.flver";

        return ret;
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
            else if (Project.Type == ProjectType.DS2S)
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

        if (Project.Type == ProjectType.DS2S)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flv";
        }
        else if (Project.Type is ProjectType.DS1)
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part.ToUpper()}.flver";
        }
        else
        {
            ret.AssetVirtualPath = $@"parts/{part}/model/{part}.flver";
        }
        return ret;
    }
}
