using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptBank
{
    private EventScriptData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, EMEVD> EventScripts = new();

    public EventScriptBank(EventScriptData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    public void LoadEventScript(string filename, string relFilePath)
    {
        if (!EventScripts.ContainsKey(relFilePath))
        {
            var editor = DataParent.Project.MapEditor;
            var eventScriptData = DataParent.Project.FS.ReadFileOrThrow(relFilePath);

            try
            {
                var emevd = EMEVD.Read(eventScriptData);
                EventScripts.Add(relFilePath, emevd);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to read EMEVD file: {relFilePath}.\n{ex}", LogLevel.Error);
            }
        }
    }

    // TODO: DS2 support, requires special handling as the EMEVD is within enc_regulation.bnd.dcx
    // So we need to read that by default, and then pass in th

    public void LoadEventScript(string filename, string relFilePath, byte[] data)
    {
        if (!EventScripts.ContainsKey(relFilePath))
        {
            var editor = DataParent.Project.MapEditor;

            try
            {
                var emevd = EMEVD.Read(data);
                EventScripts.Add(relFilePath, emevd);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to read EMEVD file: {relFilePath}.\n{ex}", LogLevel.Error);
            }
        }
    }

    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        // Save all loaded scripts
        foreach(var entry in EventScripts)
        {
            var relPath = entry.Key;

            var outputPath = @$"{DataParent.Project.ProjectPath}\{relPath}";

            var outpurDir = Path.GetDirectoryName(outputPath);
            if(!Directory.Exists(outpurDir))
            {
                Directory.CreateDirectory(outpurDir);
            }

            try
            {
                var outputBytes = entry.Value.Write();
                File.WriteAllBytes(outputPath, outputBytes);
                TaskLogs.AddLog($"Saved event script at: {outputPath}");
                successfulSave = true;
            }
            catch(Exception ex)
            {
                TaskLogs.AddLog($"Failed to save event script at: {outputPath} - {ex}");
                successfulSave = false;
            }
        }


        return successfulSave;
    }
}

