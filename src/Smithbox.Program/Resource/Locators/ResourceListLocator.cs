using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;

public static class ResourceListLocator
{
    // Used to get the map model list from within the mapbhd/bdt
    public static List<ResourceDescriptor> GetMapModelsFromBXF(ProjectEntry project, string mapid)
    {
        List<ResourceDescriptor> ret = new();

        if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            var path = $@"{project.ProjectPath}/model/map/{mapid}.mapbdt";

            if (!File.Exists(path))
            {
                path = $@"{project.DataPath}/model/map/{mapid}.mapbdt";
            }

            if (File.Exists(path))
            {
                var bdtPath = path;
                var bhdPath = path.Replace("bdt", "bhd");

                var bxf = BXF4.Read(bhdPath, bdtPath);

                if (bxf != null)
                {
                    foreach (var file in bxf.Files)
                    {
                        if (file.Name.Contains(".flv"))
                        {
                            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.Name));

                            ResourceDescriptor ad = new();
                            ad.AssetName = name;
                            ad.AssetArchiveVirtualPath = $@"map/{name}/model/";

                            ret.Add(ad);
                        }
                    }
                }
            }
        }

        return ret;
    }

    public static List<ResourceDescriptor> GetMapModels(ProjectEntry project, string mapid)
    {
        List<ResourceDescriptor> ret = new();
        if (project.ProjectType == ProjectType.DS3 || project.ProjectType == ProjectType.SDT)
        {
            if (!Directory.Exists(project.DataPath + $@"\map\{mapid}\"))
                return ret;

            var mapfiles = Directory
                .GetFileSystemEntries(project.DataPath + $@"\map\{mapid}\", @"*.mapbnd.dcx").ToList();
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
        else if (project.ProjectType == ProjectType.ER)
        {
            var mapPath = project.DataPath + $@"\map\{mapid[..3]}\{mapid}";
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
        else if (project.ProjectType == ProjectType.AC6)
        {
            var mapPath = project.DataPath + $@"\map\{mapid[..3]}\{mapid}";
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
            if (!Directory.Exists(project.DataPath + $@"\map\{mapid}\"))
                return ret;

            var ext = project.ProjectType == ProjectType.DS1 ? @"*.flver" : @"*.flver.dcx";
            var mapfiles = Directory.GetFileSystemEntries(project.DataPath + $@"\map\{mapid}\", ext)
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

    public static List<string> GetChrModels(ProjectEntry project)
    {
        try
        {
            HashSet<string> chrs = new();
            List<string> ret = new();

            var modelDir = @"\chr";
            var modelExt = @".chrbnd.dcx";

            if (project.ProjectType == ProjectType.DS1)
                modelExt = ".chrbnd";
            else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            {
                modelDir = @"\model\chr";
                modelExt = ".bnd";
            }

            if (project.ProjectType == ProjectType.DES)
            {
                var chrdirs = Directory.GetDirectories(project.DataPath + modelDir);
                foreach (var f in chrdirs)
                {
                    var name = Path.GetFileNameWithoutExtension(f + ".dummy");
                    if (name.StartsWith("c"))
                        ret.Add(name);
                }

                return ret;
            }

            var chrfiles = Directory.GetFileSystemEntries(project.DataPath + modelDir, $@"*{modelExt}")
                .ToList();
            foreach (var f in chrfiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                chrs.Add(name);
            }

            if (project.ProjectPath != null && Directory.Exists(project.ProjectPath + modelDir))
            {
                chrfiles = Directory.GetFileSystemEntries(project.ProjectPath + modelDir, $@"*{modelExt}").ToList();
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
            TaskLogs.AddLog($"[Smithbox] Failed to find characer models.", LogLevel.Error, Tasks.LogPriority.High, e);

            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }

    public static List<string> GetObjModels(ProjectEntry project, bool useProject = false)
    {
        HashSet<string> objs = new();
        List<string> ret = new();

        var modelDir = @"\obj";
        var modelExt = @".objbnd.dcx";

        if (project.ProjectType == ProjectType.DS1)
        {
            modelExt = ".objbnd";
        }
        else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
        {
            modelDir = @"\model\obj";
            modelExt = ".bnd";
        }
        else if (project.ProjectType == ProjectType.ER)
        {
            // AEGs are objs in my heart :(
            modelDir = @"\asset\aeg";
            modelExt = ".geombnd.dcx";
        }
        else if (project.ProjectType == ProjectType.AC6)
        {
            // AEGs are objs in my heart :(
            modelDir = @"\asset\environment\geometry";
            modelExt = ".geombnd.dcx";
        }

        var rootDir = project.DataPath + modelDir;
        var modDir = project.ProjectPath + modelDir;

        if (!Directory.Exists(rootDir))
        {
            return ret;
        }

        foreach (var f in Directory.GetFileSystemEntries(rootDir, $@"*{modelExt}").ToList())
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
            ret.Add(name);
            objs.Add(name);
        }

        if (project.ProjectType == ProjectType.ER)
        {
            foreach (var folder in Directory.GetDirectories(rootDir).ToList())
            {
                if (Directory.Exists(folder))
                {
                    var tempRootDir = $@"{rootDir}\{folder.Substring(folder.Length - 6)}";

                    if (Directory.Exists(tempRootDir))
                    {
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
            }
        }

        if (project.ProjectPath != null && Directory.Exists(modDir))
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

            if (project.ProjectType == ProjectType.ER)
            {
                foreach (var folder in Directory.GetDirectories(modDir).ToList())
                {
                    if (Directory.Exists(folder))
                    {
                        var tempModDir = $@"{modDir}\{folder.Substring(folder.Length - 6)}";

                        if (Directory.Exists(tempModDir))
                        {
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
            }
        }

        ret.Sort();

        return ret;
    }

    public static List<string> GetPartsModels(ProjectEntry project)
    {
        try
        {
            HashSet<string> parts = new();
            List<string> ret = new();

            var modelDir = @"\parts";
            var modelExt = @".partsbnd.dcx";

            if (project.ProjectType == ProjectType.DS1)
            {
                modelExt = ".partsbnd";
            }
            else if (project.ProjectType == ProjectType.DS2S || project.ProjectType == ProjectType.DS2)
            {
                modelDir = @"\model\parts";
                modelExt = ".bnd";
                var partsGatheredFiles = Directory.GetFiles(project.DataPath + modelDir, "*", SearchOption.AllDirectories);

                foreach (var f in partsGatheredFiles)
                {
                    if (!f.EndsWith("common.commonbnd.dcx") && !f.EndsWith("common_cloth.commonbnd.dcx") &&
                        !f.EndsWith("facepreset.bnd"))
                        ret.Add(Path.GetFileNameWithoutExtension(f));
                }

                return ret;
            }

            var partsFiles = Directory.GetFileSystemEntries(project.DataPath + modelDir, $@"*{modelExt}")
                .ToList();

            foreach (var f in partsFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                parts.Add(name);
            }

            if (project.ProjectPath != null && Directory.Exists(project.ProjectPath + modelDir))
            {
                partsFiles = Directory.GetFileSystemEntries(project.ProjectPath + modelDir, $@"*{modelExt}").ToList();
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
            TaskLogs.AddLog($"[Smithbox] Failed to find part models.", LogLevel.Error, Tasks.LogPriority.High, e);

            // Game likely isn't UXM unpacked
            return new List<string>();
        }
    }
}
