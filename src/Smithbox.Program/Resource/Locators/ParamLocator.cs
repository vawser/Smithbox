using Octokit;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource.Locators;
public static class ParamLocator
{
    internal static ResourceDescriptor GetDS2Param(ProjectEntry project, string paramStr, string appendStr, string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        var path = $@"Param\{paramStr}_{mapid}";

        if (project.ProjectPath != null && File.Exists($@"{project.ProjectPath}\{path}.param") || writemode && project.ProjectPath != null)
        {
            ad.AssetPath = $@"{project.ProjectPath}\{path}.param";
        }
        else if (File.Exists($@"{project.DataPath}\{path}.param"))
        {
            ad.AssetPath = $@"{project.DataPath}\{path}.param";
        }

        ad.AssetName = mapid + $"_{appendStr}";

        return ad;
    }

    public static ResourceDescriptor GetDS2GeneratorParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "generatorparam", "generators", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2GeneratorLocationParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "generatorlocation", "generator_locations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2GeneratorRegistParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "generatorregistparam", "generator_registrations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2EventParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "eventparam", "event_params", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2EventLocationParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "eventlocation", "event_locations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2ObjInstanceParam(ProjectEntry project, string mapid, bool writemode = false)
    {
        return GetDS2Param(project, "mapobjectinstanceparam", "object_instance_params", mapid, writemode);
    }

    public static PARAMDEF GetParamdefForParam(ProjectEntry project, string paramType)
    {
        var pd = PARAMDEF.XmlDeserialize($@"{GetParamdefDir(project)}\{paramType}.xml");

        return pd;
    }

    public static string GetUpgraderAssetsDir(ProjectEntry project)
    {
        return $@"{GetParamAssetsDir(project)}\Upgrader";
    }

    public static string GetGameOffsetsAssetsDir(ProjectEntry project)
    {
        return $@"Assets\PARAM\{ProjectUtils.GetGameDirectory(project)}";
    }

    public static string GetParamAssetsDir(ProjectEntry project)
    {
        return $@"Assets\PARAM\{ProjectUtils.GetGameDirectory(project)}";
    }

    public static string GetParamdefDir(ProjectEntry project)
    {
        return $@"{GetParamAssetsDir(project)}\Defs";
    }

    public static string GetTentativeParamTypePath(ProjectEntry project)
    {
        return $@"{GetParamAssetsDir(project)}\Defs\TentativeParamType.csv";
    }

    public static ulong[] GetParamdefPatches(ProjectEntry project)
    {
        if (Directory.Exists($@"{GetParamAssetsDir(project)}\DefsPatch"))
        {
            var entries = Directory.GetFileSystemEntries($@"{GetParamAssetsDir(project)}\DefsPatch");
            return entries.Select(e => ulong.Parse(Path.GetFileNameWithoutExtension(e))).ToArray();
        }

        return new ulong[] { };
    }

    public static string GetParamdefPatchDir(ProjectEntry project, ulong patch)
    {
        return $@"{GetParamAssetsDir(project)}\DefsPatch\{patch}";
    }

    public static string GetParammetaDir(ProjectEntry project)
    {
        return $@"{GetParamAssetsDir(project)}\Meta";
    }

    public static string GetParamNamesDir(ProjectEntry project)
    {
        return $@"{GetParamAssetsDir(project)}\Names";
    }

    public static string GetStrippedRowNamesPath(ProjectEntry project, string paramName)
    {
        var dir = $"{project.ProjectPath}\\.smithbox\\Workflow\\Stripped Row Names";

        return $@"{dir}\{paramName}.txt";
    }

    public static string GetMassEditScriptCommonDir()
    {
        return @"Assets\Scripts\Common";
    }

    public static string GetMassEditScriptGameDir(ProjectEntry project)
    {
        return $@"Assets\Scripts\{ProjectUtils.GetGameDirectory(project)}";
    }
}
