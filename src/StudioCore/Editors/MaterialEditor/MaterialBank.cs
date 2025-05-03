using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public Dictionary<MaterialFileInfo, IBinder> FileBank { get; private set; } = new();

    public MaterialBank(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        // MTD
        Task<bool> mtdTask = LoadMaterials();
        bool mtdTaskResult = await mtdTask;

        return true;
    }

    public async Task<bool> LoadMaterials()
    {
        await Task.Delay(1000);

        FileBank = new();

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            fileDir = @"\material";
            fileExt = @".matbinbnd.dcx";
        }

        List<string> fileNames = MiscLocator.GetMaterialBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
            {
                LoadMaterial($"{Project.ProjectPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadMaterial($"{Project.DataPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        return true;
    }

    public void LoadMaterial(string path)
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

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
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


    public void SaveMaterials()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveMaterial(info, binder);
        }
    }

    public void SaveMaterial(MaterialFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        //TaskLogs.AddLog($"SaveCutscene: {info.Path}");

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
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

        var assetRoot = $@"{Project.DataPath}\{fileDir}\{info.Name}{fileExt}";
        var assetMod = $@"{Project.ProjectPath}\{fileDir}\{info.Name}{fileExt}";

        switch (Project.ProjectType)
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
        if (!Directory.Exists($"{Project.ProjectPath}\\{fileDir}\\"))
        {
            Directory.CreateDirectory($"{Project.ProjectPath}\\{fileDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Project.ProjectPath == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(assetMod, fileBytes);
            //TaskLogs.AddLog($"Saved at: {assetMod}");
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
