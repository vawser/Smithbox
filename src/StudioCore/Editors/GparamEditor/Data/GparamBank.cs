using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Data;

public class GparamBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public SortedDictionary<string, GparamInfo> VanillaParamBank { get; set; }
    public SortedDictionary<string, GparamInfo> ParamBank { get; set; }

    public GparamBank(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        // GPARAM
        Task<bool> gparamTask = LoadGPARAM();
        bool gparamTaskResult = await gparamTask;

        return true;
    }

    public async Task<bool> LoadGPARAM()
    {
        await Task.Delay(1000);

        ParamBank = new();
        VanillaParamBank = new();

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        // TODO: add support for DS2
        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            return false;
        }

        foreach (var name in GetGparamFileNames())
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
            {
                LoadGraphicsParam($"{Project.ProjectPath}\\{filePath}", true);
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadGraphicsParam($"{Project.DataPath}\\{filePath}", false);
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }

            LoadVanillaGraphicsParam($"{Project.DataPath}\\{filePath}", false);
        }

        //TaskLogs.AddLog($"Graphics Param Bank - Load Complete");

        return true;
    }

    public void LoadGraphicsParam(string path, bool isModFile)
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
            gStruct.Gparam = GPARAM.Read(path);
            gStruct.Bytes = File.ReadAllBytes(path);

            ParamBank.Add(name, gStruct);
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to read GPARAM file: {filename} at {path}.\n{ex}", LogLevel.Error);
        }
    }

    public void LoadVanillaGraphicsParam(string path, bool isModFile)
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

            if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
            {
                try
                {
                    gStruct.Gparam = GPARAM.Read(path);
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(path);
                    TaskLogs.AddLog($"Failed to read GPARAM file: {filename} at {path}.\n{ex}", LogLevel.Error);
                }
            }
            else
            {
                try
                {
                    gStruct.Gparam = GPARAM.Read(DCX.Decompress(path));
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(path);
                    TaskLogs.AddLog($"Failed to read GPARAM file: {filename} at {path}.\n{ex}", LogLevel.Error);
                }
            }

            VanillaParamBank.Add(name, gStruct);
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to read GPARAM file: {filename} at {path}.\n{ex}", LogLevel.Error);
        }
    }

    public void SaveGraphicsParams()
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

    public void SaveGraphicsParam(GparamInfo info)
    {
        if (info == null)
            return;

        GPARAM param = info.Gparam;

        byte[] fileBytes = null;

        switch (Project.ProjectType)
        {
            case ProjectType.DS2:
            case ProjectType.DS2S:
                fileBytes = param.Write(DCX.Type.None);
                break;
            case ProjectType.BB:
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
                return;
        }

        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Project.ProjectType == ProjectType.DS2S)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        var assetRoot = $@"{Project.DataPath}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Project.ProjectPath}\{paramDir}\{info.Name}{paramExt}";

        if (Project.ProjectPath != "")
        {
            // Add drawparam folder if it does not exist in GameModDirectory
            if (!Directory.Exists($"{Project.ProjectPath}\\{paramDir}\\"))
            {
                Directory.CreateDirectory($"{Project.ProjectPath}\\{paramDir}\\");
            }

            // Make a backup of the original file if a mod path doesn't exist
            if (Project.ProjectPath == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
            {
                File.Copy(assetRoot, $@"{assetRoot}.bak", true);
            }

            if (fileBytes != null)
            {
                // Write to GameModDirectory
                File.WriteAllBytes(assetMod, fileBytes);
                //TaskLogs.AddLog($"Saved at: {assetMod}");
            }

            TaskLogs.AddLog($"Saved GPARAM File: {info.Name} at {assetMod}");
        }
    }


    public List<string> GetGparamFileNames()
    {
        var paramDir = @"\param\drawparam";
        var paramExt = @".gparam.dcx";

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\filter";
            paramExt = @".fltparam";
        }

        HashSet<string> paramNames = new();
        List<string> ret = new();

        if (Directory.Exists(Project.DataPath + paramDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Project.DataPath + paramDir, $@"*{paramExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                paramNames.Add(name);
            }

            // MOD
            if (Project.ProjectPath != null && Directory.Exists(Project.ProjectPath + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Project.ProjectPath + paramDir, $@"*{paramExt}").ToList();

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
        public byte[] Bytes { get; set; }

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
