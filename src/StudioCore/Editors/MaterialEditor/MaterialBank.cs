using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.ParamEditor;
using StudioCore.Resource.Locators;
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

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
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

    public static void LoadMaterials()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\material";
            fileExt = @".matbinbnd.dcx";
        }

        List<string> fileNames = MiscLocator.GetMaterialBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                LoadMaterial($"{Smithbox.ProjectRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadMaterial($"{Smithbox.GameRoot}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;
    }

    public static void LoadMaterial(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading material file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading material file.",
                    LogLevel.Warning);
            return;
        }

        var fileExt = @".mtd";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            fileExt = @".matbin";
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        MaterialFileInfo fileStruct = new MaterialFileInfo(name, path);

        IBinder binder = null;

        try
        {
            binder = BND4.Read(DCX.Decompress(path));
        }
        catch (Exception ex)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            TaskLogs.AddLog($"Failed to read Material file: {filename} at {path}.\n{ex}", LogLevel.Error);
        }

        if (binder != null)
        {
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
                        TaskLogs.AddLog($"Failed to read Material file: {file.ID}.\n{ex}", LogLevel.Error);
                    }
                }
            }

            FileBank.Add(fileStruct, binder);
        }
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
