using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.TimeActEditor;
public static class AnimationBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<AnimationFileInfo, IBinder> FileBank { get; private set; } = new();

    public static void SaveTimeActs()
    {
        foreach(var (info, binder) in FileBank)
        {
            SaveTimeAct(info, binder);
        }
    }

    public static void SaveTimeAct(AnimationFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        //TaskLogs.AddLog($"SaveTimeAct: {info.Path}");

        var fileDir = @"\chr";
        var fileExt = @".anibnd.dcx";

        foreach (BinderFile file in binder.Files)
        {
            foreach (TAE tFile in info.TimeActFiles)
            {
                if (file.ID == tFile.ID)
                {
                    file.Bytes = tFile.Write();
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveTimeAct");
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

    public static void LoadTimeActs()
    {
        if(Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\chr";
        var fileExt = @".anibnd.dcx";

        List<string> fileNames = ResourceMiscLocator.GetAnimationBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadTimeAct($"{Smithbox.ProjectRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadTimeAct($"{Smithbox.GameRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Animation File Bank - Load Complete");
    }

    public static void LoadTimeAct(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Tae file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Tae file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        AnimationFileInfo fileStruct = new AnimationFileInfo(name, path);

        IBinder binder = BND4.Read(DCX.Decompress(path));

        foreach(var file in binder.Files)
        {
            if (file.Name.Contains(".tae"))
            {
                // Ignore the empty TAE files
                if (file.Bytes.Length > 0)
                {
                    try
                    {
                        TAE taeFile = TAE.Read(file.Bytes);
                        fileStruct.TimeActFiles.Add(taeFile);
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"{name} {file.Name} - Failed to read.\n{ex.ToString()}");
                    }
                }
            }
        }

        FileBank.Add(fileStruct, binder);
    }

    public class AnimationFileInfo
    {
        public AnimationFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            TimeActFiles = new List<TAE>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<TAE> TimeActFiles { get; set; }
    }
}
