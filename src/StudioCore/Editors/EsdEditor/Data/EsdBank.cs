using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.EsdEditor;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.TalkEditor;

public class EsdBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public bool IsLoaded { get; private set; }
    public bool IsLoading { get; private set; }

    public Dictionary<EsdScriptInfo, IBinder> TalkBank { get; private set; } = new();

    public EsdMeta Meta;

    public EsdBank(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Meta = new EsdMeta(baseEditor, project);
    }
    
    // TODO: switch editor to FileDictionary method, where files are only loaded on demand, not all upfront
    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        // Meta
        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        // EMEVD
        Task<bool> esdTask = LoadESD();
        bool esdTaskResult = await esdTask;

        return true;
    }

    public async Task<bool> LoadESD()
    {
        await Task.Delay(1000);

        TalkBank = new();

        var fileDir = @"\script\talk";
        var fileExt = @".talkesdbnd.dcx";

        List<string> talkNames = MiscLocator.GetTalkBinders();

        foreach (var name in talkNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
            {
                LoadEsdScript($"{Project.ProjectPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadEsdScript($"{Project.DataPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        return true;
    }

    private void LoadEsdScript(string path)
    {
        // TODO: add DS2 ESD support
        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            return;

        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading ESD file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading ESD file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        EsdScriptInfo talkInfo = new EsdScriptInfo(name, path);

        IBinder binder = null;

        if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            try
            {
                binder = BND3.Read(DCX.Decompress(path));
            }
            catch (Exception ex)
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                TaskLogs.AddLog($"Failed to read ESD file: {filename} at {path}.\n{ex}", LogLevel.Error);
            }
        }
        else
        {
            try
            {
                binder = BND4.Read(DCX.Decompress(path));
            }
            catch (Exception ex)
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                TaskLogs.AddLog($"Failed to read ESD file: {filename} at {path}.\n{ex}", LogLevel.Error);
            }
        }

        if (binder != null)
        {
            foreach (var file in binder.Files)
            {
                if (file.Name.Contains(".esd"))
                {
                    try
                    {
                        ESD eFile = ESD.Read(file.Bytes);
                        talkInfo.EsdFiles.Add(eFile);
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"Failed to read ESD script: {file.ID}.\n{ex}", LogLevel.Error);
                    }
                }
            }

            TalkBank.Add(talkInfo, binder);
        }
    }

    public void SaveEsdScripts()
    {
        foreach (var (info, binder) in TalkBank)
        {
            SaveEsdScript(info, binder);
        }
    }

    public void SaveEsdScript(EsdScriptInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        // Ignore loaded scripts that have not been modified
        // This is to prevent mass-transfer to project folder on Save-All
        if (!info.IsModified)
            return;

        //TaskLogs.AddLog($"SaveTalkScript: {info.Path}");

        var fileDir = @"\script\talk";
        var fileExt = @".talkesdbnd.dcx";

        foreach (BinderFile file in binder.Files)
        {
            foreach (ESD eFile in info.EsdFiles)
            {
                file.Bytes = eFile.Write();
            }
        }

        byte[] fileBytes = null;
        var assetRoot = $@"{Project.DataPath}\{fileDir}\{info.Name}{fileExt}";
        var assetMod = $@"{Project.ProjectPath}\{fileDir}\{info.Name}{fileExt}";

        if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            BND3 writeBinder = binder as BND3;

            switch (Project.ProjectType)
            {
                case ProjectType.DS1:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                    break;
                case ProjectType.DS1R:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                    break;
                default:
                    return;
            }
        }
        else
        {
            BND4 writeBinder = binder as BND4;

            switch (Project.ProjectType)
            {
                case ProjectType.BB:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                    break;
                case ProjectType.DS3:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                    break;
                case ProjectType.SDT:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                    break;
                case ProjectType.ER:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                    break;
                case ProjectType.AC6:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                    break;
                default:
                    return;
            }
        }

        // Add folder if it does not exist in GameModDirectory
        if (!Directory.Exists($"{Project.ProjectPath}\\{fileDir}\\"))
        {
            Directory.CreateDirectory($"{Project.ProjectPath}\\{fileDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Project.ProjectPath == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(assetMod, fileBytes);
            //TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public class EsdScriptInfo
    {
        public EsdScriptInfo(string name, string path)
        {
            IsModified = false;
            Name = name;
            Path = path;
            EsdFiles = new List<ESD>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsModified { get; set; }

        public List<ESD> EsdFiles { get; set; }
    }
}
