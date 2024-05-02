using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.BehaviorEditor;
public static class BehaviorBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<BehaviorFileInfo, IBinder> FileBank { get; private set; } = new();

    public static void SaveBehaviors()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveBehavior(info, binder);
        }
    }

    public static void SaveBehavior(BehaviorFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        TaskLogs.AddLog($"SaveBehavior: {info.Path}");

        var fileDir = @"\chr";
        var fileExt = @".behbnd.dcx";

        foreach (BinderFile file in binder.Files)
        {
            foreach (var hkxInfo in info.HkxFiles)
            {
                file.Bytes = hkxInfo.Entry.Write();
            }
        }

        BND4 writeBinder = binder as BND4;
        byte[] fileBytes = null;

        var assetRoot = $@"{Project.GameRootDirectory}\{fileDir}\{info.Name}{fileExt}";
        var assetMod = $@"{Project.GameModDirectory}\{fileDir}\{info.Name}{fileExt}";

        switch (Project.Type)
        {
            case ProjectType.DS3:
                fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                break;
            default:
                TaskLogs.AddLog($"Invalid ProjectType during SaveBehavior");
                return;
        }

        // Add folder if it does not exist in GameModDirectory
        if (!Directory.Exists($"{Project.GameModDirectory}\\{fileDir}\\"))
        {
            Directory.CreateDirectory($"{Project.GameModDirectory}\\{fileDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Project.GameModDirectory == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(assetMod, fileBytes);
            TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public static void LoadBehaviors()
    {
        if (Project.Type == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\chr";
        var fileExt = @".behbnd.dcx";

        List<string> fileNames = ResourceMiscLocator.GetBehaviorBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadBehavior($"{Project.GameModDirectory}\\{filePath}");
                TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadBehavior($"{Project.GameRootDirectory}\\{filePath}");
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

        IBinder binder = BND4.Read(DCX.Decompress(path));

        FileBank.Add(fileStruct, binder);
    }

    // Done separately from the default load so we populate the BehaviorFiles of only the target .behbnd
    public static void LoadSelectedHkxFiles(BehaviorFileInfo info, IBinder binder)
    {
        TaskLogs.AddLog($"LoadSelectedHkxFiles");

        // Only add once, as this function will be called every time the binder selection is changed
        if (!info.LoadedHkxFiles)
        {
            info.LoadedHkxFiles = true;

            foreach (var file in binder.Files)
            {
                var filePath = file.Name;
                var fileName = Path.GetFileNameWithoutExtension(file.Name);

                if (filePath.Contains(".hkx") && filePath.Contains("Behaviors"))
                {
                    try
                    {
                        HKX cFile = HKX.Read(file.Bytes);

                        HkxFileInfo hkxInfoEntry = new HkxFileInfo(fileName, cFile);
                        info.HkxFiles.Add(hkxInfoEntry);


                        TaskLogs.AddLog($"Added entry: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"{file.ID} - Failed to read.\n{ex.ToString()}");
                    }
                }
            }
        }
    }

    public class BehaviorFileInfo
    {
        public BehaviorFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            HkxFiles = new List<HkxFileInfo>();
            LoadedHkxFiles = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<HkxFileInfo> HkxFiles { get; set; }

        public bool LoadedHkxFiles { get; set; }
    }

    public class HkxFileInfo
    {
        public HkxFileInfo(string name, HKX hkx)
        {
            Name = name;
            Entry = hkx;
        }

        public string Name { get; set; }
        public HKX Entry { get; set; }
    }
}
