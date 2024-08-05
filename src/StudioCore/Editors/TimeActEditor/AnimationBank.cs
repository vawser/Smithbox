using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using static SoulsFormats.TAE;

namespace StudioCore.Editors.TimeActEditor;
public static class AnimationBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<AnimationFileInfo, IBinder> VanillaFileBank { get; private set; } = new();
    public static Dictionary<AnimationFileInfo, IBinder> FileBank { get; private set; } = new();

    public static Dictionary<string, Template> TimeActTemplates = new Dictionary<string, Template>();

    public static void LoadTimeActs()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            return;
        }

        TimeActTemplates = new();

        // Load templates
        var templateDir = $"{AppContext.BaseDirectory}Assets\\TAE\\";
        foreach (var file in Directory.EnumerateFiles(templateDir, "*.xml"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var template = TAE.Template.ReadXMLFile(file);

            TimeActTemplates.Add(name, template);
        }

        IsLoaded = false;
        IsLoading = true;

        FileBank = new();
        VanillaFileBank = new();

        LoadTimeActs(FileBank);
        LoadTimeActs(VanillaFileBank, true);

        IsLoaded = true;
        IsLoading = false;
    }

    public static void LoadTimeActs(Dictionary<AnimationFileInfo, IBinder> targetBank, bool rootOnly = false)
    {
        var fileDir = @"\chr";
        var fileExt = @".anibnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileDir = @"\timeact\chr";
            fileExt = @".tae";
        }

        List<string> fileNames = MiscLocator.GetAnimationBinders();

        foreach (var name in fileNames)
        {
            if (!(Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
            {
                // Skip the non-TAE holding ones
                if (name.Length != 5)
                {
                    continue;
                }
            } 

            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (rootOnly)
            {
                LoadTimeAct($"{Smithbox.GameRoot}\\{filePath}", FileBank);
            }
            else
            {
                if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
                {
                    LoadTimeAct($"{Smithbox.ProjectRoot}\\{filePath}", FileBank);
                }
                else
                {
                    LoadTimeAct($"{Smithbox.GameRoot}\\{filePath}", FileBank);
                }
            }
        }

        TaskLogs.AddLog($"Project TAE File Bank - Load Complete");
    }

    public static void LoadTimeAct(string path, Dictionary<AnimationFileInfo, IBinder> targetBank)
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

        IBinder binder = null;

        // DS2 doesn't use a container
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileStruct.IsContainerFile = false;

            try
            {
                var fileBytes = File.ReadAllBytes(path);
                TAE taeFile = TAE.Read(fileBytes);
                fileStruct.TimeActFiles.Add(taeFile);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{name} - Failed to read.\n{ex.ToString()}");
            }
        }
        else
        {
            fileStruct.IsContainerFile = true;

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                binder = BND3.Read(DCX.Decompress(path));
            }
            else
            {
                binder = BND4.Read(DCX.Decompress(path));
            }

            foreach (var file in binder.Files)
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
        }

        targetBank.Add(fileStruct, binder);
    }

    public static void SaveTimeActs()
    {
        foreach (var (info, binder) in FileBank)
        {
            SaveTimeAct(info, binder);
        }
    }

    public static void SaveTimeAct(AnimationFileInfo info, IBinder binder)
    {
        if (binder == null)
            return;

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var fileDir = @"\timeact\chr";
            var fileExt = @".tae";

            // Direct file with DS2
            var fileBytes = info.TimeActFiles.First().Write();

            var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

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
        else
        {
            var fileDir = @"\chr";
            var fileExt = @".anibnd.dcx";

            foreach (BinderFile file in binder.Files)
            {
                foreach (TAE tFile in info.TimeActFiles)
                {
                    var fileID = file.ID;
                    var taeFileID = tFile.ID;

                    // Accounts for the internal File IDs
                    if(Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3)
                    {
                        taeFileID = taeFileID - 2000;
                        fileID = fileID - 5000000;
                    }

                    if (fileID == taeFileID)
                    {
                        file.Bytes = tFile.Write();
                    }
                }
            }

            byte[] fileBytes = null;

            var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                BND3 writeBinder = binder as BND3;

                switch (Smithbox.ProjectType)
                {
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                        break;
                    default:
                        TaskLogs.AddLog($"Invalid Project Type during Save Time Act");
                        return;
                }
            }
            else
            {
                BND4 writeBinder = binder as BND4;

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
                        TaskLogs.AddLog($"Invalid Project Type during Save Time Act");
                        return;
                }
            }

            if (fileBytes != null)
            {
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

                File.WriteAllBytes(assetMod, fileBytes);
                TaskLogs.AddLog($"Saved {info.Name} - {assetMod}.");
            }
            else
            {
                TaskLogs.AddLog($"Failed to save {info.Name} - {assetMod}.");
            }
        }
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

        public bool IsContainerFile { get; set; }

        public List<TAE> TimeActFiles { get; set; }
    }
}
