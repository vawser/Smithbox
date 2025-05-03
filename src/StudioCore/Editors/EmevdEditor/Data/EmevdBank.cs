using Microsoft.Extensions.Logging;
using Silk.NET.Core;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the load and save processes for the EMEVD files and their containers, 
/// as well as applying the EMEDF templates to the EMEVD Files.
/// </summary>
public class EmevdBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public SortedDictionary<EventScriptInfo, EMEVD> ScriptBank { get; private set; } = new();
    public EMEDF InfoBank { get; private set; } = new();

    public bool IsSupported = false;

    public EmevdBank(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    // TODO: switch editor to FileDictionary method, where files are only loaded on demand, not all upfront
    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        // EMEDF
        Task<bool> emedfTask = LoadEMEDF();
        bool emedfTaskResult = await emedfTask;

        // EMEVD
        Task<bool> emevdTask = LoadEMEVD();
        bool emevdTaskResult = await emevdTask;

        return true;
    }

    public async Task<bool> LoadEMEDF()
    {
        await Task.Delay(1000);

        IsSupported = false;

        var path = "";
        switch(Project.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds1-common.emedf.json";
                break;
            case ProjectType.DS2:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2-common.emedf.json";
                break;
            case ProjectType.DS2S:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2scholar-common.emedf.json";
                break;
            case ProjectType.BB:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//bb-common.emedf.json";
                break;
            case ProjectType.DS3:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds3-common.emedf.json";
                break;
            case ProjectType.SDT:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//sekiro-common.emedf.json";
                break;
            case ProjectType.ER:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//er-common.emedf.json";
                break;
            case ProjectType.AC6:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ac6-common.emedf.json";
                break;
            default: break;
        }

        if (IsSupported)
            InfoBank = EMEDF.ReadFile(path);
        else
            return false;

        return true;
    }

    public async Task<bool> LoadEMEVD()
    {
        await Task.Delay(1000);

        ScriptBank = new();

        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            LoadDS2EventScripts();
        }
        else
        {
            List<string> paramNames = MiscLocator.GetEventBinders();

            foreach (var name in paramNames)
            {
                var filePath = $"{paramDir}\\{name}{paramExt}";

                if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
                {
                    LoadEventScript($"{Project.ProjectPath}\\{filePath}");
                    //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
                }
                else
                {
                    LoadEventScript($"{Project.DataPath}\\{filePath}");
                    //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
                }
            }
        }

        return true;
    }

    private void LoadEventScript(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EMEVD file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EMEVD file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        EventScriptInfo eventInfo = new EventScriptInfo(name, path);
        EMEVD eventScript = new EMEVD();

        try
        {
            eventScript = EMEVD.Read(DCX.Decompress(path));
            ScriptBank.Add(eventInfo, eventScript);
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to read EMEVD file: {filename} at {path}.\n{ex}", LogLevel.Error);
        }
    }

    // parambank process here as emevd it within regulation.bin
    private void LoadDS2EventScripts()
    {
        var dir = Project.DataPath;
        var mod = Project.ProjectPath;

        var regulationPath = $@"{mod}\enc_regulation.bnd.dcx";
        if (!File.Exists(regulationPath))
        {
            regulationPath = $@"{dir}\enc_regulation.bnd.dcx";
        }

        BND4 emevdBnd = null;
        if (!BND4.Is(regulationPath))
        {
            try
            {
                emevdBnd = SFUtil.DecryptDS2Regulation(regulationPath);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Regulation load failed: {regulationPath} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            try
            {
                emevdBnd = BND4.Read(regulationPath);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Regulation load failed: {regulationPath} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        LoadScriptsFromBinder(emevdBnd);
    }

    private void LoadScriptsFromBinder(IBinder emevdBnd)
    {
        // Load every script in the regulation
        foreach (BinderFile f in emevdBnd.Files)
        {
            //TaskLogs.AddLog(f.Name);
            var scriptName = Path.GetFileNameWithoutExtension(f.Name);
            EventScriptInfo info = new EventScriptInfo(scriptName, f.Name);

            if (!f.Name.ToUpper().EndsWith(".EMEVD"))
            {
                //TaskLogs.AddLog("Skipped due to lacking .emevd");
                continue;
            }

            if (ScriptBank.ContainsKey(info))
            {
                //TaskLogs.AddLog("Skipped as already added");
                continue;
            }

            try
            {
                EMEVD script = EMEVD.Read(f.Bytes);
                ScriptBank.Add(info, script);
                //TaskLogs.AddLog($"{scriptName} added");
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load {scriptName}", LogLevel.Warning, LogPriority.Normal, e);
            }
        }
    }

    public void SaveEventScripts()
    {
        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            SaveDS2EventScripts();
        }
        else
        {
            foreach (var (info, script) in ScriptBank)
            {
                // Only save all modified files
                if (info.IsModified)
                {
                    SaveEventScript(info, script);
                }
            }
        }
    }

    public void SaveEventScript(EventScriptInfo info, EMEVD script)
    {
        if (script == null)
            return;

        byte[] fileBytes = null;

        switch (Project.ProjectType)
        {
            case ProjectType.DS1:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS1R:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.BB:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.DS3:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            case ProjectType.SDT:
                fileBytes = script.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.ER:
                fileBytes = script.Write(DCX.Type.DCX_KRAK);
                break;
            case ProjectType.AC6:
                fileBytes = script.Write(DCX.Type.DCX_KRAK_MAX);
                break;
            default:
                return;
        }

        var paramDir = @"\event\";
        var paramExt = @".emevd.dcx";

        var assetRoot = $@"{Project.DataPath}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Project.ProjectPath}\{paramDir}\{info.Name}{paramExt}";

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
            TaskLogs.AddLog($"Saved EMEVD file: {info.Name} at {assetMod}");
        }
    }

    // parambank process here as emevd it within regulation.bin
    private void SaveDS2EventScripts()
    {
        var dir = Project.DataPath;
        var mod = Project.ProjectPath;

        if (!File.Exists($@"{dir}\enc_regulation.bnd.dcx"))
        {
            TaskLogs.AddLog("Cannot locate regulation. Save failed.", LogLevel.Error, LogPriority.High);
            return;
        }

        var regulation = $@"{mod}\enc_regulation.bnd.dcx";
        BND4 emevdBnd;

        if (!File.Exists(regulation))
        {
            // If there is no mod file, check the base file. Decrypt it if you have to.
            regulation = $@"{dir}\enc_regulation.bnd.dcx";

            if (!BND4.Is($@"{dir}\enc_regulation.bnd.dcx"))
            {
                // Decrypt the file
                emevdBnd = SFUtil.DecryptDS2Regulation(regulation);

                // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                if (!File.Exists($@"{regulation}.bak"))
                {
                    File.Copy(regulation, $@"{regulation}.bak", true);
                    emevdBnd.Write(regulation);
                }
            }
            // No need to decrypt
            else
            {
                emevdBnd = BND4.Read(regulation);
            }
        }
        // Mod file exists, use that.
        else
        {
            emevdBnd = BND4.Read(regulation);
        }

        // Write in edited EMEVD here
        foreach (var entry in ScriptBank)
        {
            var info = entry.Key;
            var script = entry.Value;

            // Always save everything for DS2
            foreach (BinderFile f in emevdBnd.Files)
            {
                var scriptName = Path.GetFileNameWithoutExtension(f.Name);

                if (!f.Name.ToUpper().EndsWith(".EMEVD"))
                {
                    continue;
                }

                if (scriptName == info.Name)
                {
                    var bytes = script.Write();
                    f.Bytes = bytes;
                }
            }
        }


        Utils.WriteWithBackup(dir, mod, @"enc_regulation.bnd.dcx", emevdBnd);
        emevdBnd.Dispose();

        TaskLogs.AddLog("Saved EMEVD scripts.", LogLevel.Information);
    }


    public class EventScriptInfo : IComparable<EventScriptInfo>
    {
        public EventScriptInfo(string name, string path)
        {
            Name = name;
            Path = path;
            IsModified = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsModified { get; set; }

        public int CompareTo(EventScriptInfo other)
        {
            return string.Compare(Name, other.Name);
        }
    }
}
