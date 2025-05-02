using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.CutsceneEditor;
public static class CutsceneBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<CutsceneFileInfo, IBinder> FileBank { get; private set; } = new();

    public static void SaveCutscenes()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveCutscene(info, binder);
        }
    }

    public static void SaveCutscene(CutsceneFileInfo info, IBinder binder)
    {
        //TaskLogs.AddLog($"SaveCutscene: {info.Path}");

        if (binder == null)
            return;

        var fileDir = @"\remo";
        var fileExt = @".remobnd.dcx";

        // Sekiro + ER + AC6
        if (Smithbox.ProjectType is ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\cutscene";
            fileExt = @".cutscenebnd.dcx";
        }

        foreach (BinderFile file in binder.Files)
        {
            if (file.Name.Contains(".mqb"))
            {
                foreach (MQB cFile in info.CutsceneFiles)
                {
                    if(file.Name == cFile.Name)
                        file.Bytes = cFile.Write();
                }
            }
        }

        BND4 writeBinder = binder as BND4;
        byte[] fileBytes = null;

        var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
        var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

        switch (Smithbox.ProjectType)
        {
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

        // Add folder if it does not exist in GameModDirectory
        if (!Directory.Exists($"{Smithbox.ProjectRoot}\\{fileDir}\\"))
        {
            Directory.CreateDirectory($"{Smithbox.ProjectRoot}\\{fileDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Smithbox.ProjectRoot == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(assetMod, fileBytes);
            //TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public static void LoadCutscenes()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\remo";
        var fileExt = @".remobnd.dcx";

        // Sekiro + ER + AC6
        if (Smithbox.ProjectType is ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\cutscene";
            fileExt = @".cutscenebnd.dcx";
        }

        List<string> fileNames = MiscLocator.GetCutsceneBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadCutscene($"{Smithbox.ProjectRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadCutscene($"{Smithbox.GameRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;
    }

    public static void LoadCutscene(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading MQB file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading MQB file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        CutsceneFileInfo fileStruct = new CutsceneFileInfo(name, path);

        IBinder binder = null;


        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            try
            {
                binder = BND3.Read(DCX.Decompress(path));
            }
            catch (Exception ex)
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                TaskLogs.AddLog($"Failed to read MQB file: {filename} at {path}.\n{ex}", LogLevel.Error);
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
                TaskLogs.AddLog($"Failed to read MQB file: {filename} at {path}.\n{ex}", LogLevel.Error);
            }
        }

        if (binder != null)
        {
            foreach (var file in binder.Files)
            {
                if (file.Name.Contains(".mqb"))
                {
                    try
                    {
                        MQB cFile = MQB.Read(file.Bytes);
                        fileStruct.CutsceneFiles.Add(cFile);
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"Failed to read MQB file: {file.ID}.\n{ex}", LogLevel.Error);
                    }
                }
            }

            FileBank.Add(fileStruct, binder);
        }
    }

    public class CutsceneFileInfo
    {
        public CutsceneFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            CutsceneFiles = new List<MQB>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<MQB> CutsceneFiles { get; set; }
    }
}
