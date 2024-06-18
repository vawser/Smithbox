using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;
public static class EmevdBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<EventScriptInfo, EMEVD> ScriptBank { get; private set; } = new();
    public static EMEDF InfoBank { get; private set; } = new();

    public static void LoadEMEDF()
    {
        var path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ac6-common.emedf.json";
        InfoBank = EMEDF.ReadFile(path);
    }

    public static void SaveEventScripts()
    {
        foreach (var (info, script) in ScriptBank)
        {
            SaveEventScript(info, script);
        }
    }

    public static void SaveEventScript(EventScriptInfo info, EMEVD script)
    {
        if (script == null)
            return;

        // Ignore loaded scripts that have not been modified
        // This is to prevent mass-transfer to project folder on Save-All
        if(!info.IsModified)
            return;

        //TaskLogs.AddLog($"SaveEventScript: {info.Path}");

        byte[] fileBytes = null;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS1:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS1R:
                fileBytes = script.Write(DCX.Type.DCX_DFLT_10000_24_9);
                break;
            case ProjectType.DS2:
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

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            paramDir = @"\param";
            paramExt = @".emevd";
        }

        var assetRoot = $@"{Smithbox.GameRoot}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Smithbox.ProjectRoot}\{paramDir}\{info.Name}{paramExt}";

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
    }

    public static void LoadEventScripts()
    {
        IsLoaded = false;
        IsLoading = true;

        ScriptBank = new();

        var paramDir = @"\event";
        var paramExt = @".emevd.dcx";

        if (Smithbox.ProjectType == ProjectType.DS2S)
        {
            paramDir = @"\param";
            paramExt = @".emevd";
        }

        List<string> paramNames = ResourceMiscLocator.GetEventBinders();

        foreach (var name in paramNames)
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadEventScript($"{Smithbox.ProjectRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadEventScript($"{Smithbox.GameRoot}\\{filePath}");
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

        try
        {
            eventScript = EMEVD.Read(DCX.Decompress(path));
            ScriptBank.Add(eventInfo, eventScript);
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to read {path}");
        }
    }

    public class EventScriptInfo
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
    }
}
