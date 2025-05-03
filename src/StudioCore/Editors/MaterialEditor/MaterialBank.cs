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

    public Dictionary<MaterialFileInfo, IBinder> Matbins { get; private set; } = new();

    public Dictionary<MaterialFileInfo, IBinder> Materials { get; private set; } = new();

    // TEMP: should be independant of a specific editor
    public Dictionary<string, MaterialInfo> MTDs = new();
    public Dictionary<string, MaterialInfo> MATBINs = new();

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

        // MATBIN
        Task<bool> matbinTask = LoadMatbins();
        bool matbinTaskResult = await mtdTask;

        // MTD (for map texturing)
        Task<bool> mtdMapTask = LoadMatbinsForMapTexturing();
        bool mtdMapTaskResult = await mtdMapTask;

        // MATBIN (for map texturing)
        Task<bool> matbinMapTask = LoadMaterialsForMapTexturing();
        bool matbinMapTaskResult = await mtdMapTask;

        return true;
    }

    public async Task<bool> LoadMaterials()
    {
        await Task.Delay(1000);

        Materials = new();

        var fileDir = @"\mtd";
        var fileExt = @".mtdbnd.dcx";

        List<string> fileNames = MiscLocator.GetMaterialBinders(Project);

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

            Materials.Add(fileStruct, binder);
        }
    }


    public async Task<bool> LoadMatbins()
    {
        await Task.Delay(1000);

        Materials = new();

       var fileDir = @"\material";
       var fileExt = @".matbinbnd.dcx";

        List<string> fileNames = MiscLocator.GetMaterialBinBinders(Project);

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
            {
                LoadMatbin($"{Project.ProjectPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadMatbin($"{Project.DataPath}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        return true;
    }

    public void LoadMatbin(string path)
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

            Materials.Add(fileStruct, binder);
        }
    }

    public void SaveMaterials()
    {
        foreach (var (info, binder) in Materials)
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

    public async Task<bool> LoadMatbinsForMapTexturing()
    {
        await Task.Delay(1000);

        MATBINs = new Dictionary<string, MaterialInfo>();

        var modPath = $"{Project.ProjectPath}//material//";
        if (Directory.Exists(modPath))
        {
            var modFiles = Directory.GetFiles(modPath);

            // Mod
            foreach (var file in modFiles)
            {
                LoadMatbinFile(file);
            }
        }

        var rootPath = $"{Project.DataPath}//material//";
        if (Directory.Exists(rootPath))
        {
            var rootFiles = Directory.GetFiles(rootPath);

            // Root
            foreach (var file in rootFiles)
            {
                LoadMatbinFile(file);
            }
        }

        return true;
    }

    public void LoadMatbinFile(string file)
    {
        IBinder binder = null;

        if (file.Contains(".matbinbnd.dcx"))
        {
            binder = BND4.Read(file);
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, MATBIN.Read(f.Bytes), null);

                    if (!MATBINs.ContainsKey(matname))
                        MATBINs.Add(matname, info);
                }
            }
        }
    }

    public async Task<bool> LoadMaterialsForMapTexturing()
    {
        await Task.Delay(1000);

        MTDs = new Dictionary<string, MaterialInfo>();

        var modPath = $"{Project.ProjectPath}//mtd//";
        if (Directory.Exists(modPath))
        {
            var modFiles = Directory.GetFiles(modPath);

            // Mod
            foreach (var file in modFiles)
            {
                LoadMtdFile(file);
            }
        }

        var rootPath = $"{Project.DataPath}//mtd//";
        if (Directory.Exists(rootPath))
        {
            var rootFiles = Directory.GetFiles(rootPath);

            // Root
            foreach (var file in rootFiles)
            {
                LoadMtdFile(file);
            }
        }

        return true;
    }

    public void LoadMtdFile(string file)
    {
        IBinder binder = null;

        if (file.Contains(".mtd.dcx"))
        {
            binder = BND4.Read(file);
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, null, MTD.Read(f.Bytes));

                    if (!MTDs.ContainsKey(matname))
                        MTDs.Add(matname, info);
                }
            }
        }
    }

    public struct MaterialInfo
    {
        public string Name;
        public string Path;
        public MATBIN Matbin;
        public MTD Mtd;

        public MaterialInfo(string name, string path, MATBIN matbin, MTD mtd)
        {
            Name = name;
            Path = path;
            Matbin = matbin;
            Mtd = mtd;
        }
    }
}
