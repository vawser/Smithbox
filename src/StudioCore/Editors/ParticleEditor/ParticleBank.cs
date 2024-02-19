using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParticleEditor;
public static class ParticleBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<ParticleFileInfo, IBinder> FileBank { get; private set; } = new();

    public static void SaveParticles()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveParticle(info, binder);
        }
    }

    public static void SaveParticle(ParticleFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        //TaskLogs.AddLog($"SaveParticle: {info.Path}");

        var fileDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        foreach (BinderFile file in binder.Files)
        {
            foreach (FXR3 cFile in info.ParticleFiles)
            {
                file.Bytes = cFile.Write();
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveCutscene");
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

    public static void LoadParticles()
    {
        if (Project.Type == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        List<string> fileNames = FileAssetLocator.GetParticleBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadParticle($"{Project.GameModDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadParticle($"{Project.GameRootDirectory}\\{filePath}");
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Particle File Bank - Load Complete");
    }

    public static void LoadParticle(string path)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading ffx file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading ffx file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        ParticleFileInfo fileStruct = new ParticleFileInfo(name, path);

        IBinder binder = BND4.Read(DCX.Decompress(path));

        foreach (var file in binder.Files)
        {
            if (file.Name.Contains(".fxr"))
            {
                try
                {
                    FXR3 cFile = FXR3.Read(file.Bytes);
                    fileStruct.ParticleFiles.Add(cFile);
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{file.ID} - Failed to read.\n{ex.ToString()}");
                }
            }
        }

        FileBank.Add(fileStruct, binder);
    }

    public class ParticleFileInfo
    {
        public ParticleFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            ParticleFiles = new List<FXR3>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public List<FXR3> ParticleFiles { get; set; }
    }
}
