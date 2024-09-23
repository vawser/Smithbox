using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParticleEditor;
public static class ParticleBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static List<ParticleFileInfo> FileBank { get; set; } = new();

    public static Dictionary<string, FxrInfo> LoadedFXR { get; set; } = new();

    public static Dictionary<string, ResourceInfo> LoadedResourceLists { get; set; } = new();

    // File Info:
    // Holds the file name list(s), and the binder itself
    public class ParticleFileInfo
    {
        public ParticleFileInfo(string name, string path, IBinder binder, List<string> fxrFiles, List<string> resourceFiles)
        {
            Name = name;
            Path = path;
            FxrFiles = fxrFiles;
            ResourceFiles = resourceFiles;
            Binder = binder;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public IBinder Binder { get; set; }

        public List<String> FxrFiles { get; set; }
        public List<String> ResourceFiles { get; set; }
    }

    // Particle Info:
    // Holds the particle FXR3 contents, and a link to the parent (thus the binder it is in)
    public class FxrInfo
    {
        public ParticleFileInfo Parent { get; set; }
        public string Name { get; set; }
        public FXR3 Content { get; set; }
    }

    // Resource Info:
    // Holds the ffx resource list contents, and a link to the parent (thus the binder it is in)
    public class ResourceInfo
    {
        public ParticleFileInfo Parent { get; set; }
        public string Name { get; set; }
        public FFXResourceList Content { get; set; }
    }

    public static void LoadParticles()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();

        var fileDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        List<string> fileNames = MiscLocator.GetParticleBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            var realPath = "";

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                realPath = $"{Smithbox.ProjectRoot}\\{filePath}";
            }
            else
            {
                realPath = $"{Smithbox.GameRoot}\\{filePath}";
            }

            if (realPath != "")
            {
                IBinder binder = BND4.Read(DCX.Decompress(realPath));
                List<string> fxrFiles = new List<string>();
                List<string> resourceFiles = new List<string>();

                foreach (var file in binder.Files)
                {
                    // FXR
                    if (file.Name.Contains(".fxr"))
                    {
                        fxrFiles.Add(file.Name);
                    }

                    // FFXRESLIST
                    if (file.Name.Contains(".ffxreslist"))
                    {
                        resourceFiles.Add(file.Name);
                    }
                }

                var fileInfo = new ParticleFileInfo(name, realPath, binder, fxrFiles, resourceFiles);

                if(!FileBank.Contains(fileInfo))
                {
                    FileBank.Add(fileInfo);
                }
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Particle File Bank - Load Complete");
    }

    public static bool LoadParticle(string name, ParticleFileInfo info)
    {
        if(LoadedFXR.ContainsKey(name))
        {
            // Skip if already loaded
            //TaskLogs.AddLog($"{name} has already been loaded.", LogLevel.Warning);
            return false;
        }

        foreach (var file in info.Binder.Files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.Name);

            if (fileName == name)
            {
                try
                {
                    FXR3 cFile = FXR3.Read(file.Bytes);

                    FxrInfo newFxrInfo = new FxrInfo();
                    newFxrInfo.Parent = info;
                    newFxrInfo.Name = fileName;
                    newFxrInfo.Content = cFile;

                    LoadedFXR.Add(name, newFxrInfo);

                    return true;
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{file.ID} - Failed to read.\n{ex.ToString()}");
                }
            }
        }

        return false;
    }

    public static void SaveParticles()
    {
        // Iterate through each loaded particle
        foreach (var (name, info) in LoadedFXR)
        {
            SaveParticle(info);
        }

        // Iterate through each loaded resource list
        foreach (var (name, info) in LoadedResourceLists)
        {
            SaveResourceList(info);
        }
    }

    public static void SaveParticle(FxrInfo info)
    {
        ParticleFileInfo parent = info.Parent;

        if (parent.Binder == null)
            return;

        //TaskLogs.AddLog($"SaveParticle: {info.Path}");

        var fileDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        // Enter parent binder and then write the current particle's data
        foreach (BinderFile file in parent.Binder.Files)
        {
            file.Bytes = info.Content.Write();
        }

        BND4 writeBinder = parent.Binder as BND4;
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveParticle");
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

    public static void SaveResourceList(ResourceInfo info)
    {
        ParticleFileInfo parent = info.Parent;

        if (parent.Binder == null)
            return;

        //TaskLogs.AddLog($"SaveParticle: {info.Path}");

        var fileDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        // Enter parent binder and then write the current resource list data
        foreach (BinderFile file in parent.Binder.Files)
        {
            Encoding u8 = Encoding.UTF8;
            byte[] result = info.Content.Entries.SelectMany(x => u8.GetBytes(x)).ToArray();

            file.Bytes = result;
        }

        BND4 writeBinder = parent.Binder as BND4;
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
                TaskLogs.AddLog($"Invalid ProjectType during SaveResourceList");
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
}
