using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;
public static class EventScriptBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<EventScriptInfo, EMEVD> ScriptBank { get; private set; } = new();

    public static void SaveEventScripts()
    {
        foreach (var (info, script) in ScriptBank)
        {
            SaveEventScript(info, script);
        }
    }

    public static void SaveEventScript(EventScriptInfo info, EMEVD script)
    {
        //TaskLogs.AddLog($"SaveEventScript: {info.Path}");

        byte[] fileBytes = null;

        switch (Project.Type)
        {
            case ProjectType.DS1:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS1R:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS2S:
                fileBytes = script.Write(DCX.Type.None);
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveEventScript");
                return;
        }

        var paramDir = @"\event\";
        var paramExt = @".emevd.dcx";

        if (Project.Type == ProjectType.DS2S)
        {
            paramDir = @"\param";
            paramExt = @".emevd";
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

    public static void LoadEventScripts()
    {
        IsLoaded = false;
        IsLoading = true;

        ScriptBank = new();

        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        if (Project.Type == ProjectType.DS2S)
        {
            paramDir = @"\param";
            paramExt = @".emevd";
        }

        List<string> paramNames = FileAssetLocator.GetEventBinders();

        foreach (var name in paramNames)
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadEventScript($"{Project.GameModDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadEventScript($"{Project.GameRootDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Event Script Bank - Load Complete");
    }

    private static void LoadEventScript(string path)
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

        if (Project.Type == ProjectType.DS2S)
        {
            eventScript = EMEVD.Read(path);
        }
        else
        {
            eventScript = EMEVD.Read(DCX.Decompress(path));
        }

        ScriptBank.Add(eventInfo, eventScript);
    }

    public struct EventScriptInfo
    {
        public EventScriptInfo(string name, string path)
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
