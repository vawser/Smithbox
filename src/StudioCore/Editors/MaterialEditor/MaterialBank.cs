using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;
public static class MaterialBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<MaterialFileInfo, IBinder> FileBank { get; private set; } = new();

    public static void SaveMaterials()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveMaterial(info, binder);
        }
    }

    public static void SaveMaterial(MaterialFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        //TaskLogs.AddLog($"SaveCutscene: {info.Path}");

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        if (Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\material";
            fileExt = @".matbinbnd.dcx";
        }

        foreach (BinderFile file in binder.Files)
        {
            foreach (MTD mFile in info.MaterialFiles)
            {
                file.Bytes = mFile.Write();
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveMaterial");
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
            //TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public static void LoadMaterials()
    {
        if (Project.Type == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        if (Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\material";
            fileExt = @".matbinbnd.dcx";
        }

        List<string> fileNames = ResourceMiscLocator.GetMaterialBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadMaterial($"{Project.GameModDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadMaterial($"{Project.GameRootDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Material File Bank - Load Complete");
    }

    public static void LoadMaterial(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Mtd file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Mtd file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        MaterialFileInfo fileStruct = new MaterialFileInfo(name, path);

        IBinder binder = BND4.Read(DCX.Decompress(path));

        var fileExt = @".mtd";

        if (Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            fileExt = @".matbin";
        }

        foreach (var file in binder.Files)
        {
            if (file.Name.Contains($"{fileExt}"))
            {
                try
                {
                    MTD cFile = MTD.Read(file.Bytes);
                    fileStruct.MaterialFiles.Add(cFile);
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{file.ID} - Failed to read.\n{ex.ToString()}");
                }
            }
        }

        FileBank.Add(fileStruct, binder);
    }

    public class MaterialFileInfo
    {
        public MaterialFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            MaterialFiles = new List<MTD>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<MTD> MaterialFiles { get; set; }
    }
}
