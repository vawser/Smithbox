using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Interface.Settings;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static SoulsFormats.MSB_AC6.Part;
using static SoulsFormats.TAE;
using static StudioCore.Editors.TimeActEditor.AnimationBank;
using static StudioCore.Interface.Settings.TimeActEditorTab;

namespace StudioCore.Editors.TimeActEditor;
public static class AnimationBank
{
    public static bool IsSaving { get; set; }
    public static bool IsLoaded { get; set; }
    public static bool IsTemplatesLoaded { get; set; }
    public static bool IsCharacterTimeActsLoaded { get; set; }
    public static bool IsObjectTimeActsLoaded { get; set; }
    public static bool IsVanillaCharacterTimeActsLoaded { get; set; }
    public static bool IsVanillaObjectTimeActsLoaded { get; set; }

    public static Dictionary<ContainerFileInfo, BinderInfo> FileChrBank { get; set; } = new();
    public static Dictionary<ContainerFileInfo, BinderInfo> FileObjBank { get; set; } = new();
    public static Dictionary<ContainerFileInfo, BinderInfo> VanillaChrFileBank { get; set; } = new();
    public static Dictionary<ContainerFileInfo, BinderInfo> VanillaObjFileBank { get; set; } = new();

    public static Dictionary<string, Template> TimeActTemplates = new Dictionary<string, Template>();

    public static void Load()
    {
        var title = $"{AnimationBank.GetObjectTitle()}s";

        if (!IsLoaded)
        {
            if (!IsTemplatesLoaded)
            {
                TaskManager.Run(
                    new TaskManager.LiveTask($"Setup Time Act Editor: Templates", TaskManager.RequeueType.None, false,
                () =>
                {
                    LoadTimeActTemplates();
                }));
            }

            // Project - Character Time Acts
            if (CFG.Current.TimeActEditor_Load_CharacterTimeActs)
            {
                if (!IsCharacterTimeActsLoaded)
                {
                    TaskManager.Run(
                        new TaskManager.LiveTask($"Setup Time Act Editor: Characters", TaskManager.RequeueType.None, false,
                    () =>
                    {
                        LoadProjectCharacterTimeActs();
                    }));
                }
            }
            else
            {
                IsCharacterTimeActsLoaded = true;
            }
            // Vanilla - Character Time Acts
            if (CFG.Current.TimeActEditor_Load_VanillaCharacterTimeActs)
            {
                if (!IsVanillaCharacterTimeActsLoaded)
                {
                    TaskManager.Run(
                        new TaskManager.LiveTask($"Setup Time Act Editor: Characters (Vanilla)", TaskManager.RequeueType.None, false,
                    () =>
                    {
                        LoadVanillaCharacterTimeActs();
                    }));
                }
            }
            else
            {
                IsVanillaCharacterTimeActsLoaded = true;
            }

            // Project - Object Time Acts
            if (CFG.Current.TimeActEditor_Load_ObjectTimeActs)
            {
                if (!IsObjectTimeActsLoaded)
                {
                    TaskManager.Run(
                        new TaskManager.LiveTask($"Setup Time Act Editor: {title}", TaskManager.RequeueType.None, false,
                    () =>
                    {
                        LoadProjectObjectTimeActs();
                    }));
                }
            }
            else
            {
                IsObjectTimeActsLoaded = true;
            }

            // Vanilla - Object Time Acts
            if (CFG.Current.TimeActEditor_Load_VanillaObjectTimeActs)
            {
                if (!IsVanillaObjectTimeActsLoaded)
                {
                    TaskManager.Run(
                        new TaskManager.LiveTask($"Setup Time Act Editor: {title} (Vanilla)", TaskManager.RequeueType.None, false,
                    () =>
                    {
                        LoadProjectObjectTimeActs();
                    }));
                }
            }
            else
            {
                IsVanillaObjectTimeActsLoaded = true;
            }

            if (IsTemplatesLoaded && IsCharacterTimeActsLoaded && IsObjectTimeActsLoaded && IsVanillaCharacterTimeActsLoaded  && IsVanillaObjectTimeActsLoaded)
            {
                IsLoaded = true;
            }
        }
    }

    public static void DisplayLoadState()
    {
        ImGui.Text($"This editor is still loading:");
        if (AnimationBank.IsTemplatesLoaded)
        {
            ImGui.Text($"Templates:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"Templates:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Warning_Text_Color, "LOADING");
        }

        if (AnimationBank.IsCharacterTimeActsLoaded)
        {
            ImGui.Text($"Character Time Acts:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"Character Time Acts:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Warning_Text_Color, "LOADING");
        }

        var title = $"{AnimationBank.GetObjectTitle()}";

        if (AnimationBank.IsObjectTimeActsLoaded)
        {
            ImGui.Text($"{title} Time Acts:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"{title} Time Acts:");
            ImGui.SameLine();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Warning_Text_Color, "LOADING");
        }
    }

    public static void LoadTimeActTemplates()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsTemplatesLoaded = false;

        TimeActTemplates = new();

        // Load templates
        var templateDir = $"{AppContext.BaseDirectory}Assets\\TAE\\";
        foreach (var file in Directory.EnumerateFiles(templateDir, "*.xml"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var template = TAE.Template.ReadXMLFile(file);

            TimeActTemplates.Add(name, template);
        }

        IsTemplatesLoaded = true;

    }

    public static void LoadProjectCharacterTimeActs()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsCharacterTimeActsLoaded = false;

        FileChrBank = new();
        LoadChrTimeActs(FileChrBank);

        IsCharacterTimeActsLoaded = true;
    }

    public static void LoadProjectObjectTimeActs()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsObjectTimeActsLoaded = false;

        FileObjBank = new();
        LoadObjTimeActs(FileObjBank);

        IsObjectTimeActsLoaded = true;
    }

    public static void LoadVanillaCharacterTimeActs()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsVanillaCharacterTimeActsLoaded = false;

        VanillaChrFileBank = new();
        LoadChrTimeActs(VanillaChrFileBank);

        IsVanillaCharacterTimeActsLoaded = true;
    }

    public static void LoadVanillaObjectTimeActs()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsVanillaObjectTimeActsLoaded = false;

        VanillaObjFileBank = new();
        LoadObjTimeActs(VanillaObjFileBank);

        IsVanillaObjectTimeActsLoaded = true;
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
    }


    public static void LoadObjTimeActs(Dictionary<ContainerFileInfo, BinderInfo> targetBank, bool rootOnly = false)
    {
        var fileDir = @"\obj";
        var fileExt = @".objbnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileDir = @"\timeact\obj";
            fileExt = @".tae";
        }

        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            fileDir = @"\asset\environment\geometry\";
            fileExt = @".geombnd.dcx";
        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            fileDir = @"\asset\aeg\";
            fileExt = @".geombnd.dcx";

            Dictionary<string, List<string>> assetDict = new();

            if (Smithbox.ProjectType is ProjectType.ER)
            {
                assetDict = MiscLocator.GetAssetTimeActBinders_ER();
            }

            foreach (var entry in assetDict)
            {
                var folder = entry.Key;
                var files = entry.Value;

                foreach (var name in files)
                {
                    var filePath = $"{fileDir}\\{folder}\\{name}{fileExt}";

                    if (rootOnly)
                    {
                        LoadObjTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank, folder);
                    }
                    else
                    {
                        if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
                        {
                            LoadObjTimeAct($"{Smithbox.ProjectRoot}\\{filePath}", targetBank, folder);
                        }
                        else
                        {
                            LoadObjTimeAct($"{Smithbox.GameRoot}\\{filePath}", targetBank, folder);
                        }
                    }
                }
            }
        }
        else
        {
            List<string> fileNames = MiscLocator.GetObjectTimeActBinders();

            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                fileNames = MiscLocator.GetAssetTimeActBinders_AC6();
            }

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
        }
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
                var bytes = DCX.Decompress(path);
                if(bytes.Length > 0)
                    binder = BND3.Read(bytes);
            }
            else
            {
                var bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND4.Read(bytes);
            }

            if (binder != null)
            {
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
        }

        // Only add if the file contains at least one TAE entry
        if (validFile)
        {
            fileStruct.InternalFiles.Sort();
            var binderInfo = new BinderInfo(binder, null, "");
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    public static void LoadObjTimeAct(string path, Dictionary<ContainerFileInfo, BinderInfo> targetBank, string aegFolder="")
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

        if(aegFolder != "")
        {
            fileStruct.AegFolder = aegFolder;
        }

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
                var bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND3.Read(bytes);
            }
            else
            {
                var bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND4.Read(bytes);
            }

            if (binder != null)
            {
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

                        if (aniBinder != null)
                        {
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
            }
        }

        // Only add if the file contains at least one TAE entry
        if (validFile)
        {
            fileStruct.InternalFiles.Sort();
            var binderInfo = new BinderInfo(binder, aniBinder, aniBinderName);
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    public static async void SaveTimeActsTask()
    {
        IsSaving = true;

        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeActs = SaveTimeActs();

        bool result = await saveTimeActs;
        IsSaving = result;
    }

    public static async Task<bool> SaveTimeActs()
    {
        foreach (var (info, binder) in FileChrBank)
        {
            await SaveTimeAct(info, binder);
        }

        return false;
    }

    public static void HandleDS2TimeActSave(ContainerFileInfo info, BinderInfo binderInfo)
    {
        var fileDir = @"\timeact\chr";
        var fileExt = @".tae";

        if (info.Path.Contains("obj"))
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
        else if (File.Exists(assetMod))
        {
            File.Copy(assetMod, $@"{assetMod}.bak", true);
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(assetMod, fileBytes);
            TaskLogs.AddLog($"Saved at: {assetMod}");
        }
    }

    public static void HandleBinderContents(ContainerFileInfo info, BinderInfo binderInfo, IBinder binder)
    {
        // Write existing TAE, and discover files that need to be added
        foreach (InternalFileInfo tInfo in info.InternalFiles)
        {
            foreach (BinderFile file in binder.Files)
            {
                if (file.Name == tInfo.Filepath)
                {
                    file.Bytes = tInfo.TAE.Write();
                }
            }
        }

        // TAE files within anibnd.dcx container
        var internalFilepath = "";

        // Grab correct path for a TAE binderFile
        foreach (BinderFile file in binder.Files)
        {
            if (internalFilepath == "" && file.Name.Contains(".tae"))
                internalFilepath = file.Name;
        }

        // Get internal path without the tae file part
        var filename = Path.GetFileName(internalFilepath);
        var internalPath = internalFilepath.Replace(filename, "");

        // Create new binder files for the newly added internal files
        foreach (InternalFileInfo tInfo in info.InternalFiles)
        {
            if (tInfo.MarkForAddition)
            {
                var newBinderfile = new BinderFile();
                var id = int.Parse(tInfo.Name.Substring(1));

                newBinderfile.ID = id;
                newBinderfile.Name = $"{internalPath}\\{tInfo.Name}.tae";
                newBinderfile.Bytes = tInfo.TAE.Write();

                binder.Files.Add(newBinderfile);
            }
        }

        // Remove internal files marked for removal
        foreach (InternalFileInfo tInfo in info.InternalFiles)
        {
            if (tInfo.MarkForRemoval)
            {
                BinderFile fileToRemove = null;

                foreach (BinderFile file in binder.Files)
                {
                    if (file.Name == tInfo.Filepath)
                    {
                        fileToRemove = file;
                    }
                }

                if(fileToRemove != null)
                {
                    binder.Files.Remove(fileToRemove);
                }
            }
        }
    }

    public static async void SaveTimeActTask(ContainerFileInfo info, BinderInfo binderInfo)
    {
        IsSaving = true;

        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeAct = SaveTimeAct(info, binderInfo);

        bool result = await saveTimeAct;

        IsSaving = result;
    }

    public static async Task<bool> SaveTimeAct(ContainerFileInfo info, BinderInfo binderInfo)
    {
        await Task.Delay(1000);

        if (!info.IsModified)
            return false;

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            HandleDS2TimeActSave(info, binderInfo);
        }
        else
        {
            if (binderInfo.ContainerBinder == null)
                return false;

            var fileDir = @"\chr";
            var fileExt = @".anibnd.dcx";

            // Dealing with objbnd
            if(binderInfo.InternalBinder != null)
            {
                fileDir = @"\obj";
                fileExt = @".objbnd.dcx";

                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    fileDir = @$"\asset\aeg\{info.AegFolder}";
                    fileExt = @".geombnd.dcx";
                }

                if (Smithbox.ProjectType is ProjectType.AC6)
                {
                    fileDir = @"\asset\environment\geometry\";
                    fileExt = @".geombnd.dcx";
                }
            }

            // TAE files within the direct container
            HandleBinderContents(info, binderInfo, binderInfo.ContainerBinder);

            // TAE files within internal anibnd container
            if (binderInfo.InternalBinder != null)
            {
                foreach (BinderFile file in binderInfo.ContainerBinder.Files)
                {
                    if (file.Name == binderInfo.InternalBinderName)
                    {
                        HandleBinderContents(info, binderInfo, binderInfo.InternalBinder);

                        byte[] tempBytes = GetBinderBytes(binderInfo.InternalBinder);
                        file.Bytes = tempBytes;
                    }
                }
            }

            byte[] fileBytes = GetBinderBytes(binderInfo.ContainerBinder);

            var assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            var assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

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
                else if(File.Exists(assetMod))
                {
                    File.Copy(assetMod, $@"{assetMod}.bak", true);
                }

                File.WriteAllBytes(assetMod, fileBytes);
                TaskLogs.AddLog($"Saved {info.Name} - {assetMod}.");
            }
            else
            {
                TaskLogs.AddLog($"Failed to save {info.Name} - {assetMod}.");
            }
        }

        return false;
    }

    public static byte[] GetBinderBytes(IBinder targetBinder)
    {
        // Allow older compression types for these two since KRAK is slow
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            BND4 writeBinder = targetBinder as BND4;
            var currentCompressionType = CFG.Current.CurrentTimeActCompressionType;

            if (currentCompressionType != TimeactCompressionType.Default)
            {
                switch (currentCompressionType)
                {
                    case TimeactCompressionType.DFLT:
                        return writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                    case TimeactCompressionType.KRAK:
                        return writeBinder.Write(DCX.Type.DCX_KRAK);
                    case TimeactCompressionType.KRAK_MAX:
                        return writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                }
            }
        }

        // Otherwise use the normal compression types
        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            BND3 writeBinder = targetBinder as BND3;

            switch (Smithbox.ProjectType)
            {
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    return writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
            }
        }
        else
        {
            BND4 writeBinder = targetBinder as BND4;

            switch (Smithbox.ProjectType)
            {
                case ProjectType.DS3:
                    return writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                case ProjectType.SDT:
                    return writeBinder.Write(DCX.Type.DCX_KRAK);
                case ProjectType.ER:
                    return writeBinder.Write(DCX.Type.DCX_KRAK);
                case ProjectType.AC6:
                    return writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
            }
        }

        TaskLogs.AddLog($"Invalid Project Type during Save Time Act");

        return null;
    }

    public static string GetObjectTitle()
    {
        var title = "Object";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            title = "Asset";
        }

        return title;
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

        public string AegFolder { get; set; }

        public bool IsContainerFile { get; set; }

        public bool IsModified { get; set; }

        public List<InternalFileInfo> InternalFiles { get; set; }
    }

    public class InternalFileInfo : IComparable<InternalFileInfo>
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public TAE TAE { get; set; }

        public bool MarkForAddition { get; set; }
        public bool MarkForRemoval { get; set; }

        public InternalFileInfo(string path, TAE taeData)
        {
            Filepath = path;
            TAE = taeData;
            Name = Path.GetFileNameWithoutExtension(path);
        }

        public int CompareTo(InternalFileInfo other)
        {
            var thisID = int.Parse(Name.Substring(1));
            var otherID = int.Parse(other.Name.Substring(1));

            if(thisID > otherID) 
                return 1;

            if(otherID > thisID) 
                return -1;

            if(thisID == otherID) 
                return 0;

            return 0;
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
