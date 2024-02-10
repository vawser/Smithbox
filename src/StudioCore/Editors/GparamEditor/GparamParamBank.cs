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
public static class GparamParamBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<GparamInfo, GPARAM> ParamBank { get; private set; } = new();

    public static void SaveGraphicsParams()
    {
        foreach (var (info, param) in ParamBank)
        {
            SaveGraphicsParam(info, param);
        }
    }

    public static void SaveGraphicsParam(GparamInfo info, GPARAM param)
    {
        if (param == null)
            return;

        //TaskLogs.AddLog($"SaveGraphicsParams: {info.Path}");

        byte[] fileBytes = null;

        switch (Project.Type)
        {
            case ProjectType.DS2S:
                fileBytes = param.Write(DCX.Type.None);
                break;
            case ProjectType.DS3:
                fileBytes = param.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.SDT:
                fileBytes = param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.ER:
                fileBytes = param.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.AC6:
                fileBytes = param.Write(DCX.Type.DCX_KRAK_MAX);
                break;
            default:
                TaskLogs.AddLog($"Invalid ProjectType during SaveGraphicsParam");
                return;
        }

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Project.Type == ProjectType.DS2S)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        var assetRoot = $@"{Project.GameRootDirectory}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Project.GameModDirectory}\{paramDir}\{info.Name}{paramExt}";

        // Add drawparam folder if it does not exist in GameModDirectory
        if (!Directory.Exists($"{Project.GameModDirectory}\\{paramDir}\\"))
        {
            Directory.CreateDirectory($"{Project.GameModDirectory}\\{paramDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Project.GameModDirectory == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            // Write to GameModDirectory
            File.WriteAllBytes(assetMod, fileBytes);
            //TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public static void LoadGraphicsParams()
    {
        IsLoaded = false;
        IsLoading = true;

        ParamBank = new();

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if(Project.Type == ProjectType.DS2S)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        List<string> paramNames = ParamAssetLocator.GetGraphicsParams();

        foreach (var name in paramNames)
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if(File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadGraphicsParam($"{Project.GameModDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadGraphicsParam($"{Project.GameRootDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
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
        GparamInfo gStruct = new GparamInfo(name, path);
        GPARAM gPARAM = new GPARAM();

        if (Project.Type == ProjectType.DS2S)
        {
            gPARAM = GPARAM.Read(path);
        }
        else
        {
            gPARAM = GPARAM.Read(DCX.Decompress(path));
        }

        ParamBank.Add(gStruct, gPARAM);
    }

    public struct GparamInfo
    {
        public GparamInfo(string name, string path)
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
