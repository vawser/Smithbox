using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GraphicsEditor;
public static class GparamParamBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, GparamInfo> ParamBank { get; set; }

    public static void SaveGraphicsParams()
    {
        foreach (var (name, info) in ParamBank)
        {
            if (info.WasModified)
            {
                SaveGraphicsParam(info);
                info.WasModified = false;
            }
        }
    }

    public static void SaveGraphicsParam(GparamInfo info)
    {
        GPARAM param = info.Gparam;

        if (param == null)
            return;

        //TaskLogs.AddLog($"SaveGraphicsParams: {info.Path}");

        byte[] fileBytes = null;

        switch (Project.Type)
        {
            case ProjectType.DS2:
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

        TaskLogs.AddLog($"Saved {info.Name} to {assetMod}");
    }

    public static void LoadGraphicsParams()
    {
        IsLoaded = false;
        IsLoading = true;

        ParamBank = new();

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if(Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        // TODO: add support for DS2
        if(Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            return;
        }

        foreach (var name in GetGparamFileNames())
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if(File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadGraphicsParam($"{Project.GameModDirectory}\\{filePath}", true);
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadGraphicsParam($"{Project.GameRootDirectory}\\{filePath}", false);
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        //TaskLogs.AddLog($"Graphics Param Bank - Load Complete");
    }

    private static void LoadGraphicsParam(string path, bool isModFile)
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
        gStruct.Gparam = new GPARAM();
        gStruct.IsModFile = isModFile;

        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            gStruct.Gparam = GPARAM.Read(path);
        }
        else
        {
            gStruct.Gparam = GPARAM.Read(DCX.Decompress(path));
        }

        ParamBank.Add(name, gStruct);
    }

    public static List<string> GetGparamFileNames()
    {
        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        HashSet<string> paramNames = new();
        List<string> ret = new();

        if (Directory.Exists(Project.GameRootDirectory + paramDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + paramDir, $@"*{paramExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                paramNames.Add(name);
            }

            // MOD
            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Project.GameModDirectory + paramDir, $@"*{paramExt}").ToList();

                foreach (var f in paramFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));

                    if (!paramNames.Contains(name))
                    {
                        ret.Add(name);
                        paramNames.Add(name);
                    }
                }
            }
        }

        return ret;
    }

    public class GparamInfo : IComparable<string>
    {
        public GparamInfo(string name, string path)
        {
            Name = name;
            Path = path;
            WasModified = false;
        }

        public GPARAM Gparam { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsModFile { get; set; }

        public bool WasModified { get; set; }

        public int CompareTo(string other)
        {
            return Name.CompareTo(other);
        }
    }
}
