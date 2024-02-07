using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GraphicsEditor;
public static class GraphicsParamBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<GraphicsParamInfo, GPARAM> ParamBank { get; private set; } = new();

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

        switch (Project.Type)
        {
            case ProjectType.DS2S:
                param.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.BB:
                param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.DS3:
                param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.SDT:
                param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.ER:
                param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.AC6:
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

        List<string> paramNames = ParamAssetLocator.GetGraphicsParams();

        foreach (var name in paramNames)
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if(File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadGraphicsParam($"{Project.GameModDirectory}\\{filePath}");
            }
            else
            {
                LoadGraphicsParam($"{Project.GameRootDirectory}\\{filePath}");
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
