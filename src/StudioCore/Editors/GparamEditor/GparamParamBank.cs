using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
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
        if (info == null)
            return;

        GPARAM param = info.Gparam;

        //TaskLogs.AddLog($"SaveGraphicsParams: {info.Path}");

        byte[] fileBytes = null;

        switch (Smithbox.ProjectType)
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

        if (Smithbox.ProjectType == ProjectType.DS2S)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        var assetRoot = $@"{Smithbox.GameRoot}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Smithbox.ProjectRoot}\{paramDir}\{info.Name}{paramExt}";

        if (Uri.IsWellFormedUriString($"{Smithbox.ProjectRoot}\\{paramDir}\\", UriKind.Absolute))
        {
            // Add drawparam folder if it does not exist in GameModDirectory
            if (!Directory.Exists($"{Smithbox.ProjectRoot}\\{paramDir}\\"))
            {
                Directory.CreateDirectory($"{Smithbox.ProjectRoot}\\{paramDir}\\");
            }

            // Make a backup of the original file if a mod path doesn't exist
            if (Smithbox.ProjectRoot == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
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
    }

    public static void LoadGraphicsParams()
    {
        IsLoaded = false;
        IsLoading = true;

        ParamBank = new();

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if(Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        // TODO: add support for DS2
        if(Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            return;
        }

        foreach (var name in GetGparamFileNames())
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if(File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadGraphicsParam($"{Smithbox.ProjectRoot}\\{filePath}", true);
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadGraphicsParam($"{Smithbox.GameRoot}\\{filePath}", false);
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        //TaskLogs.AddLog($"Graphics Param Bank - Load Complete");
    }

    private static void LoadGraphicsParam(string path, bool isModFile)
    {
        try
        {
            if (path == null)
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

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                gStruct.Gparam = GPARAM.Read(path);
            }
            else
            {
                gStruct.Gparam = GPARAM.Read(DCX.Decompress(path));
            }

            ParamBank.Add(name, gStruct);
        }
        catch(Exception e) 
        {
            TaskLogs.AddLog($"Failed to load {path}: {e.Message}");
        }
    }

    public static List<string> GetGparamFileNames()
    {
        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        HashSet<string> paramNames = new();
        List<string> ret = new();

        if (Directory.Exists(Smithbox.GameRoot + paramDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Smithbox.GameRoot + paramDir, $@"*{paramExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                paramNames.Add(name);
            }

            // MOD
            if (Smithbox.ProjectRoot != null && Directory.Exists(Smithbox.ProjectRoot + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Smithbox.ProjectRoot + paramDir, $@"*{paramExt}").ToList();

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
