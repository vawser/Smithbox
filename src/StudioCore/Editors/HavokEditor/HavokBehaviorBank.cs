using HKLib.hk2018;
using HKLib.Serialization.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Binary.Util;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;
public static class HavokBehaviorBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<BehaviorFileInfo, BND4Reader> FileBank { get; private set; } = new();

    public static void SaveBehaviors()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveBehavior(info, binder);
        }
    }

    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }

    public static void SaveBehavior(BehaviorFileInfo info, BND4Reader binder)
    {
        return;
        /*
        if (binder == null)
            return;

        if (!info.IsModified)
            return;

        TaskLogs.AddLog($"SaveBehavior: {info.Path}");

        var fileDir = @"\chr";
        var fileExt = @".behbnd.dcx";

        foreach (var file in binder.Files)
        {
            foreach (var hkxInfo in info.HkxFiles)
            {
                // TODO
                //file.Bytes = ReadFully(hkxInfo.Data);
            }
        }

        BND4 writeBinder = binder as BND4;
        byte[] fileBytes = null;

        var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
        var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

        switch (Smithbox.ProjectType)
        {
            case ProjectType.ER:
                fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                break;
            default:
                TaskLogs.AddLog($"Invalid ProjectType during SaveBehavior");
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
            TaskLogs.AddLog($"Saved at: {assetMod}");
        }
        */
    }

    public static void LoadBehaviors()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\chr";
        var fileExt = @".behbnd.dcx";

        List<string> fileNames = MiscLocator.GetBehaviorBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadBehavior($"{Smithbox.ProjectRoot}\\{filePath}");
                TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadBehavior($"{Smithbox.GameRoot}\\{filePath}");
                TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Behavior File Bank - Load Complete");
    }

    public static void LoadBehavior(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Behavior file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Behavior file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        BehaviorFileInfo fileStruct = new BehaviorFileInfo(name, path);

        BND4Reader binder = new BND4Reader(path);

        FileBank.Add(fileStruct, binder);
    }

    public static hkRootLevelContainer LoadSelectedHavokBehaviorFile(BehaviorFileInfo info, BND4Reader binder)
    {
        TaskLogs.AddLog($"LoadSelectedHkxFiles");

        if (!info.LoadedHkxFiles)
        {
            info.LoadedHkxFiles = true;

            foreach (var file in binder.Files)
            {
                var fileName = file.Name.ToLower();
                var fileBytes = binder.ReadFile(file);

                TaskLogs.AddLog($"fileName: {fileName}");

                if (fileName.Contains("behaviors") && fileName.EndsWith(".hkx"))
                {
                    TaskLogs.AddLog($"fileName: HavokBinarySerializer");

                    HavokBinarySerializer serializer = new HavokBinarySerializer();
                    using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                    {
                        return (hkRootLevelContainer)serializer.Read(memoryStream);
                    }
                }
            }
        }

        return null;
    }

    public class BehaviorFileInfo
    {
        public BehaviorFileInfo(string name, string path)
        {
            IsModified = false;
            Name = name;
            Path = path;
            HkxFiles = new List<HkxFileInfo>();
            LoadedHkxFiles = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsModified { get; set; }

        public List<HkxFileInfo> HkxFiles { get; set; }

        public bool LoadedHkxFiles { get; set; }
    }

    public class HkxFileInfo
    {
        public HkxFileInfo(string name, Stream data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; set; }

        public Stream Data { get; set; }
    }
}
