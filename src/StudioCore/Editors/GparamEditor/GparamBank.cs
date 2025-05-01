using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamBank
{
    private GparamData DataParent;

    private VirtualFileSystem FS;

    public string BankName = "Undefined";

    public Dictionary<string, GPARAM> GraphicsParams = new();


    public GparamBank(GparamData parent, string bankName, VirtualFileSystem targetFs)
    {
        DataParent = parent;
        BankName = bankName;
        FS = targetFs;
    }

    public void LoadGraphicsParam(string relFilePath)
    {
        if (!GraphicsParams.ContainsKey(relFilePath))
        {
            var editor = DataParent.Project.MapEditor;
            var gparamData = FS.ReadFileOrThrow(relFilePath);

            try
            {
                var gparam = GPARAM.Read(gparamData);
                GraphicsParams.Add(relFilePath, gparam);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to read graphics param file: {relFilePath}.\n{ex}", LogLevel.Error);
            }
        }
    }

    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        // Save all loaded gparams
        foreach (var entry in GraphicsParams)
        {
            var relPath = entry.Key;

            var outputPath = @$"{DataParent.Project.ProjectPath}\{relPath}";

            var outpurDir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outpurDir))
            {
                Directory.CreateDirectory(outpurDir);
            }

            try
            {
                var outputBytes = entry.Value.Write();
                File.WriteAllBytes(outputPath, outputBytes);
                TaskLogs.AddLog($"Saved graphics param at: {outputPath}");
                successfulSave = true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to save graphics param at: {outputPath} - {ex}");
                successfulSave = false;
            }
        }


        return successfulSave;
    }
}
