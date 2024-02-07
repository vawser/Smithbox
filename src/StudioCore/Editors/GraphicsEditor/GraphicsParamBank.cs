using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GraphicsEditor;
public static class GraphicsParamBank
{
    internal static AssetLocator AssetLocator;

    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<GraphicsParamInfo, GPARAM> ParamBank { get; private set; } = new();

    public static void SetAssetLocator(AssetLocator assetLocator)
    {
        AssetLocator = assetLocator;
    }

    public static void SaveGraphicsParams()
    {
        foreach (var (info, param) in ParamBank)
        {
            SaveGraphicsParam(info, param);
        }
    }

    public static void SaveGraphicsParam(GraphicsParamInfo info, GPARAM param)
    {
        TaskLogs.AddLog($"SaveGraphicsParams: {info.Path}");

        switch (AssetLocator.Type)
        {
            case GameType.DarkSoulsIISOTFS:
                param.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case GameType.Bloodborne:
                param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case GameType.DarkSoulsIII:
                param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case GameType.Sekiro:
                param.Write(DCX.Type.DCX_KRAK);
                break;
            case GameType.EldenRing:
                param.Write(DCX.Type.DCX_KRAK);
                break;
            case GameType.ArmoredCoreVI:
                param.Write(DCX.Type.DCX_KRAK_MAX);
                break;
            default:
                TaskLogs.AddLog($"Invalid GameType during SaveGraphicsParam");
                break;
        }
    }

    public static void LoadGraphicsParams()
    {
        IsLoaded = false;
        IsLoading = true;

        ParamBank = new();

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        List<string> paramNames = AssetLocator.GetDrawParams();

        foreach (var name in paramNames)
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if(File.Exists($"{AssetLocator.GameModDirectory}\\{filePath}"))
            {
                LoadGraphicsParam($"{AssetLocator.GameModDirectory}\\{filePath}");
            }
            else
            {
                LoadGraphicsParam($"{AssetLocator.GameRootDirectory}\\{filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Graphics Param Bank - Load Complete");
    }

    private static void LoadGraphicsParam(string path)
    {
        if(path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading GraphicsParam file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        GraphicsParamInfo gStruct = new GraphicsParamInfo(name, path);

        GPARAM gPARAM = GPARAM.Read(DCX.Decompress(path));
        ParamBank.Add(gStruct, gPARAM);
    }

    public struct GraphicsParamInfo
    {
        public GraphicsParamInfo(string name, string path)
        {
            Name = name;
            Path = path;
            Modified = false;
            Added = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public bool Modified { get; set; }

        public bool Added { get; set; }
    }
}
