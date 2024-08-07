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
using static SoulsFormats.MSB_AC6.Part;
using static SoulsFormats.TAE;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;
public static class AnimationBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static Dictionary<ContainerFileInfo, BinderInfo> VanillaChrFileBank { get; private set; } = new();
    public static Dictionary<ContainerFileInfo, BinderInfo> FileChrBank { get; private set; } = new();

    public static Dictionary<ContainerFileInfo, BinderInfo> VanillaObjFileBank { get; private set; } = new();
    public static Dictionary<ContainerFileInfo, BinderInfo> FileObjBank { get; private set; } = new();

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

        FileChrBank = new();
        VanillaChrFileBank = new();

        LoadChrTimeActs(FileChrBank);
        LoadChrTimeActs(VanillaChrFileBank, true);

        FileObjBank = new();
        VanillaObjFileBank = new();

        LoadObjTimeActs(FileObjBank);
        LoadObjTimeActs(VanillaObjFileBank, true);

        IsLoaded = true;
        IsLoading = false;
    }

    public static void LoadChrTimeActs(Dictionary<ContainerFileInfo, BinderInfo> targetBank, bool rootOnly = false)
    {
        var fileDir = @"\chr";
        var fileExt = @".anibnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileDir = @"\timeact\chr";
            fileExt = @".tae";
        }

        List<string> fileNames = MiscLocator.GetCharacterTimeActBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (rootOnly)
            {
                LoadChrTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank);
            }
            else
            {
                if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
                {
                    LoadChrTimeAct($"{Smithbox.ProjectRoot}\\{filePath}", targetBank);
                }
                else
                {
                    LoadChrTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank);
                }
            }
        }

        TaskLogs.AddLog($"Project TAE File Bank - Load Complete");
    }


    public static void LoadObjTimeActs(Dictionary<ContainerFileInfo, BinderInfo> targetBank, bool rootOnly = false)
    {
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            return;

        var fileDir = @"\obj";
        var fileExt = @".objbnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileDir = @"\timeact\obj";
            fileExt = @".tae";
        }

        List<string> fileNames = MiscLocator.GetObjectTimeActBinders();

        foreach (var name in fileNames)
        {
            var filePath = $"{fileDir}\\{name}{fileExt}";

            if (rootOnly)
            {
                LoadObjTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank);
            }
            else
            {
                if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
                {
                    LoadObjTimeAct($"{Smithbox.ProjectRoot}\\{filePath}", targetBank);
                }
                else
                {
                    LoadObjTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank);
                }
            }
        }

        TaskLogs.AddLog($"Project TAE File Bank - Load Complete");
    }

    public static void LoadChrTimeAct(string path, Dictionary<ContainerFileInfo, BinderInfo> targetBank)
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
        ContainerFileInfo fileStruct = new ContainerFileInfo(name, path);
        bool validFile = false;

        IBinder binder = null;

        // Loose .tae
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileStruct.IsContainerFile = false;

            try
            {
                var fileBytes = File.ReadAllBytes(path);
                TAE taeFile = TAE.Read(fileBytes);
                InternalFileInfo tInfo = new(path, taeFile);
                fileStruct.InternalFiles.Add(tInfo);
                validFile = true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{name} {path} - Failed to read.\n{ex.ToString()}");
            }
        }
        // Within .anibnd.dcx
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
                    if (file.Bytes.Length > 0)
                    {
                        try
                        {
                            TAE taeFile = TAE.Read(file.Bytes);
                            InternalFileInfo tInfo = new(file.Name, taeFile);
                            fileStruct.InternalFiles.Add(tInfo);
                            validFile = true;
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"{name} {file.Name} - Failed to read.\n{ex.ToString()}");
                        }
                    }
                }
            }
        }

        // Only add if the file contains at least one TAE entry
        if (validFile)
        {
            var binderInfo = new BinderInfo(binder, null, "");
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    public static void LoadObjTimeAct(string path, Dictionary<ContainerFileInfo, BinderInfo> targetBank)
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
        ContainerFileInfo fileStruct = new ContainerFileInfo(name, path);
        bool validFile = false;

        IBinder binder = null;
        IBinder aniBinder = null;
        string aniBinderName = "";

        // Loose .tae
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileStruct.IsContainerFile = false;

            try
            {
                var fileBytes = File.ReadAllBytes(path);
                TAE taeFile = TAE.Read(fileBytes);
                InternalFileInfo tInfo = new(path, taeFile);
                fileStruct.InternalFiles.Add(tInfo);
                validFile = true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{name} {path} - Failed to read.\n{ex.ToString()}");
            }
        }
        // Within .anibnd.dcx
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
                if (file.Name.Contains(".anibnd"))
                {
                    aniBinderName = file.Name;

                    if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
                    {
                        aniBinder = BND3.Read(file.Bytes);
                    }
                    else
                    {
                        aniBinder = BND4.Read(file.Bytes);
                    }

                    foreach (var aniFile in aniBinder.Files)
                    {
                        if (aniFile.Name.Contains(".tae"))
                        {
                            if (aniFile.Bytes.Length > 0)
                            {
                                try
                                {
                                    TAE taeFile = TAE.Read(aniFile.Bytes);
                                    InternalFileInfo tInfo = new(aniFile.Name, taeFile);
                                    fileStruct.InternalFiles.Add(tInfo);
                                    validFile = true;

                                    //TaskLogs.AddLog($"Added to bank: {file.Name}");
                                }
                                catch (Exception ex)
                                {
                                    TaskLogs.AddLog($"{name} {aniFile.Name} - Failed to read.\n{ex.ToString()}");
                                }
                            }
                        }
                    }
                }
            }
        }

        // Only add if the file contains at least one TAE entry
        if (validFile)
        {
            var binderInfo = new BinderInfo(binder, aniBinder, aniBinderName);
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    public static void SaveTimeActs()
    {
        foreach (var (info, binder) in FileChrBank)
        {
            SaveTimeAct(info, binder);
        }
    }

    public static void SaveTimeAct(ContainerFileInfo info, BinderInfo binderInfo)
    {
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var fileDir = @"\timeact\chr";
            var fileExt = @".tae";

            if(info.Path.Contains("obj"))
            {
                fileDir = @"\timeact\obj";
            }

            // Direct file with DS2
            var fileBytes = info.InternalFiles.First().TAE.Write();

            var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetModDir = $@"{Smithbox.ProjectRoot}\{fileDir}\";

            if (!Directory.Exists(assetModDir))
            {
                Directory.CreateDirectory(assetModDir);
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
        }
        else
        {
            if (binderInfo.ContainerBinder == null)
                return;

            var fileDir = @"\chr";
            var fileExt = @".anibnd.dcx";

            // Dealing with objbnd
            if(binderInfo.InternalBinder != null)
            {
                fileDir = @"\obj";
                fileExt = @".objbnd.dcx";
            }

            // TAE files within anibnd.dcx container
            foreach (BinderFile file in binderInfo.ContainerBinder.Files)
            {
                foreach (InternalFileInfo tInfo in info.InternalFiles)
                {
                    if (file.Name == tInfo.Filepath)
                    {
                        //TaskLogs.AddLog($"Written File: {file.Name}");
                        file.Bytes = tInfo.TAE.Write();
                    }
                }
            }

            // TAE files within internal anibnd container
            if(binderInfo.InternalBinder != null)
            {
                foreach (BinderFile file in binderInfo.ContainerBinder.Files)
                {
                    if (file.Name == binderInfo.InternalBinderName)
                    {
                        var internalBinderBytes = file.Bytes;

                        foreach (BinderFile internalFile in binderInfo.InternalBinder.Files)
                        {
                            foreach (InternalFileInfo tInfo in info.InternalFiles)
                            {
                                if (internalFile.Name == tInfo.Filepath)
                                {
                                    //TaskLogs.AddLog($"Written File: {file.Name}");
                                    internalFile.Bytes = tInfo.TAE.Write();
                                }
                            }
                        }

                        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
                        {
                            BND3 writeBinder = binderInfo.InternalBinder as BND3;

                            switch (Smithbox.ProjectType)
                            {
                                case ProjectType.DS1:
                                case ProjectType.DS1R:
                                    internalBinderBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                                    break;
                                default:
                                    TaskLogs.AddLog($"Invalid Project Type during Save Time Act");
                                    return;
                            }
                        }
                        else
                        {
                            BND4 writeBinder = binderInfo.InternalBinder as BND4;

                            switch (Smithbox.ProjectType)
                            {
                                case ProjectType.DS3:
                                    internalBinderBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                                    break;
                                case ProjectType.SDT:
                                    internalBinderBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                                    break;
                                case ProjectType.ER:
                                    internalBinderBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                                    break;
                                case ProjectType.AC6:
                                    internalBinderBytes = writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                                    break;
                                default:
                                    TaskLogs.AddLog($"Invalid Project Type during Save Time Act");
                                    return;
                            }
                        }

                        file.Bytes = internalBinderBytes;
                    }
                }
            }

            byte[] fileBytes = null;

            var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                BND3 writeBinder = binderInfo.ContainerBinder as BND3;

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
                BND4 writeBinder = binderInfo.ContainerBinder as BND4;

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

    public class ContainerFileInfo
    {
        public ContainerFileInfo(string name, string path)
        {
            Name = name;
            Path = path;
            InternalFiles = new List<InternalFileInfo>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsContainerFile { get; set; }

        public List<InternalFileInfo> InternalFiles { get; set; }
    }

    public class InternalFileInfo
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public TAE TAE { get; set; }

        public InternalFileInfo(string path, TAE taeData)
        {
            Filepath = path;
            TAE = taeData;
            Name = Path.GetFileNameWithoutExtension(path);
        }
    }

    public class BinderInfo
    {
        public IBinder ContainerBinder { get; set; }
        public IBinder InternalBinder { get; set; }
        public string InternalBinderName { get; set; }

        public BinderInfo(IBinder containerBinder, IBinder internalBinder, string internalBinderName)
        {
            ContainerBinder = containerBinder;
            InternalBinder = internalBinder;
            InternalBinderName = internalBinderName;
        }
    }
}
