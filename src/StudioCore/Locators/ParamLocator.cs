using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Locators;
public static class ParamLocator
{
    internal static ResourceDescriptor GetDS2Param(string paramStr, string appendStr, string mapid, bool writemode = false)
    {
        ResourceDescriptor ad = new();
        var path = $@"Param\{paramStr}_{mapid}";

        if (Smithbox.ProjectRoot != null && File.Exists($@"{Smithbox.ProjectRoot}\{path}.param") || writemode && Smithbox.ProjectRoot != null)
        {
            ad.AssetPath = $@"{Smithbox.ProjectRoot}\{path}.param";
        }
        else if (File.Exists($@"{Smithbox.GameRoot}\{path}.param"))
        {
            ad.AssetPath = $@"{Smithbox.GameRoot}\{path}.param";
        }

        ad.AssetName = mapid + $"_{appendStr}";

        return ad;
    }

    public static ResourceDescriptor GetDS2GeneratorParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("generatorparam", "generators", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2GeneratorLocationParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("generatorlocation", "generator_locations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2GeneratorRegistParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("generatorregistparam", "generator_registrations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2EventParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("eventparam", "event_params", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2EventLocationParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("eventlocation", "event_locations", mapid, writemode);
    }

    public static ResourceDescriptor GetDS2ObjInstanceParam(string mapid, bool writemode = false)
    {
        return GetDS2Param("mapobjectinstanceparam", "object_instance_params", mapid, writemode);
    }

    public static PARAMDEF GetParamdefForParam(string paramType)
    {
        var pd = PARAMDEF.XmlDeserialize($@"{GetParamdefDir()}\{paramType}.xml");

        return pd;
    }

    public static string GetUpgraderAssetsDir()
    {
        return $@"{GetParamAssetsDir()}\Upgrader";
    }

    public static string GetGameOffsetsAssetsDir()
    {
        return $@"Assets\GameOffsets\{MiscLocator.GetGameIDForDir()}";
    }

    public static string GetParamAssetsDir()
    {
        return $@"Assets\Paramdex\{MiscLocator.GetGameIDForDir()}";
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
        var dir = $@"{Smithbox.SmithboxDataRoot}\Stripped Row Names";
        return $@"{dir}\{paramName}.txt";
    }

    public static string GetMassEditScriptCommonDir()
    {
        return @"Assets\MassEditScripts\Common";
    }

    public static string GetMassEditScriptGameDir()
    {
        return $@"Assets\MassEditScripts\{MiscLocator.GetGameIDForDir()}";
    }
}
