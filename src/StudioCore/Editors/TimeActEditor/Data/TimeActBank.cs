using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SoulsFormats.TAE;
using static StudioCore.Configuration.Settings.TimeActEditorTab;

namespace StudioCore.Editors.TimeActEditor.Bank;
public static class TimeActBank
{
    public static bool IsSaving { get; set; }
    public static bool IsLoaded { get; set; }
    public static bool IsTemplatesLoaded { get; set; }
    public static bool IsCharacterTimeActsLoaded { get; set; }
    public static bool IsObjectTimeActsLoaded { get; set; }
    public static bool IsVanillaCharacterTimeActsLoaded { get; set; }
    public static bool IsVanillaObjectTimeActsLoaded { get; set; }

    public static Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> FileChrBank { get; set; } = new();
    public static Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> FileObjBank { get; set; } = new();
    public static Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> VanillaChrFileBank { get; set; } = new();
    public static Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> VanillaObjFileBank { get; set; } = new();

    public static Dictionary<string, Template> TimeActTemplates = new Dictionary<string, Template>();

    public static bool IsSupportedProjectType()
    {
        // Some of these AC games use TAE but it's not researched for their version yet
        if (Smithbox.ProjectType is ProjectType.Undefined or ProjectType.DS2 or ProjectType.AC4 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
            return false;

        return true;
    }

    /// <summary>
    /// Loads all relevant files into the banks for the Time Act Editor
    /// </summary>
    public static void Load()
    {
        string title = $"{TimeActUtils.GetObjectTitle()}s";

        if (!IsLoaded)
        {
            if (!IsTemplatesLoaded)
            {
                TaskManager.LiveTask task = new(
                    "timeActEditor_templateSetup",
                    "Time Act Editor",
                    "The TAE templates have been setup successfully.",
                    "The TAE template setup has failed.",
                    TaskManager.RequeueType.None,
                    false,
                    LoadTimeActTemplates
                );

                TaskManager.Run(task);
            }

            // Project - Character Time Acts
            if (CFG.Current.TimeActEditor_Load_CharacterTimeActs)
            {
                if (!IsCharacterTimeActsLoaded)
                {
                    TaskManager.LiveTask task = new(
                        "timeActEditor_characterTaeSetup",
                        "Time Act Editor",
                        "The Character TAE containers have been setup successfully.",
                        "The Character TAE container setup has failed.",
                        TaskManager.RequeueType.None,
                        false,
                        LoadProjectCharacterTimeActs
                    );

                    TaskManager.Run(task);
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
                    TaskManager.LiveTask task = new(
                        "timeActEditor_characterVanillaTaeSetup",
                        "Time Act Editor",
                        "The vanilla Character TAE containers have been setup successfully.",
                        "The vanilla Character TAE container setup has failed.",
                        TaskManager.RequeueType.None,
                        false,
                        LoadVanillaCharacterTimeActs
                    );

                    TaskManager.Run(task);
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
                    TaskManager.LiveTask task = new(
                        "timeActEditor_objectTaeSetup",
                        "Time Act Editor",
                        "The Object TAE containers have been setup successfully.",
                        "The Object TAE container setup has failed.",
                        TaskManager.RequeueType.None,
                        false,
                        LoadProjectObjectTimeActs
                    );

                    TaskManager.Run(task);
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
                    TaskManager.LiveTask task = new(
                        "timeActEditor_objectVanillaTaeSetup",
                        "Time Act Editor",
                        "The vanilla Object TAE containers have been setup successfully.",
                        "The vanilla Object TAE container setup has failed.",
                        TaskManager.RequeueType.None,
                        false,
                        LoadProjectObjectTimeActs
                    );

                    TaskManager.Run(task);
                }
            }
            else
            {
                IsVanillaObjectTimeActsLoaded = true;
            }

            if (IsTemplatesLoaded && IsCharacterTimeActsLoaded && IsObjectTimeActsLoaded && IsVanillaCharacterTimeActsLoaded && IsVanillaObjectTimeActsLoaded)
            {
                IsLoaded = true;
            }
        }
    }

    /// <summary>
    /// Displays the current load state for the Time Act Editor
    /// </summary>
    public static void DisplayLoadState()
    {
        UIHelper.WrappedText($"This editor is still loading:");
        if (IsTemplatesLoaded)
        {
            ImGui.Text($"Templates:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"Templates:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, "LOADING");
        }

        if (IsCharacterTimeActsLoaded)
        {
            ImGui.Text($"Character Time Acts:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"Character Time Acts:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, "LOADING");
        }

        var title = $"{TimeActUtils.GetObjectTitle()}";

        if (IsObjectTimeActsLoaded)
        {
            ImGui.Text($"{title} Time Acts:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
        }
        else
        {
            ImGui.Text($"{title} Time Acts:");
            ImGui.SameLine();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, "LOADING");
        }
    }

    /// <summary>
    /// Loads the TAE Template files for the Time Act Editor
    /// </summary>
    public static void LoadTimeActTemplates()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        IsTemplatesLoaded = false;

        TimeActTemplates = new();

        // Load templates
        string templateDir = $"{AppContext.BaseDirectory}Assets\\TAE\\";
        foreach (string file in Directory.EnumerateFiles(templateDir, "*.xml"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            Template template = Template.ReadXMLFile(file);

            TimeActTemplates.Add(name, template);
        }

        IsTemplatesLoaded = true;

    }

    /// <summary>
    /// Loads the ChrBND files from the Project folder for the Time Act Editor
    /// </summary>
    public static void LoadProjectCharacterTimeActs()
    {
        if (!IsSupportedProjectType())
            return;

        IsCharacterTimeActsLoaded = false;

        FileChrBank = new();
        LoadChrTimeActs(FileChrBank);

        // Add the bXXXX.tae from the behavior binder
        if(Smithbox.ProjectType is ProjectType.AC6)
        {
            LoadChrBehaviorTimeActs(FileChrBank);
        }

        IsCharacterTimeActsLoaded = true;
    }

    /// <summary>
    /// Loads the ObjBND files from the Project folder for the Time Act Editor
    /// </summary>
    public static void LoadProjectObjectTimeActs()
    {
        if (!IsSupportedProjectType())
            return;

        IsObjectTimeActsLoaded = false;

        FileObjBank = new();
        LoadObjTimeActs(FileObjBank);

        IsObjectTimeActsLoaded = true;
    }

    /// <summary>
    /// Loads the ChrBND files from the Game folder for the Time Act Editor
    /// </summary>
    public static void LoadVanillaCharacterTimeActs()
    {
        if (!IsSupportedProjectType())
            return;

        IsVanillaCharacterTimeActsLoaded = false;

        VanillaChrFileBank = new();
        LoadChrTimeActs(VanillaChrFileBank);

        // Add the bXXXX.tae from the behavior binder
        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            LoadChrBehaviorTimeActs(VanillaChrFileBank);
        }

        IsVanillaCharacterTimeActsLoaded = true;
    }

    /// <summary>
    /// Loads the ObjBND files from the Game folder for the Time Act Editor
    /// </summary>
    public static void LoadVanillaObjectTimeActs()
    {
        if (!IsSupportedProjectType())
            return;

        IsVanillaObjectTimeActsLoaded = false;

        VanillaObjFileBank = new();
        LoadObjTimeActs(VanillaObjFileBank);

        IsVanillaObjectTimeActsLoaded = true;
    }

    /// <summary>
    /// Loads all of the Character TAE
    /// </summary>
    public static void LoadChrTimeActs(Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> targetBank, bool rootOnly = false)
    {
        string fileDir = @"\chr";
        string fileExt = @".anibnd.dcx";

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileDir = @"\timeact\chr";
            fileExt = @".tae";
        }

        List<string> fileNames = MiscLocator.GetCharacterTimeActBinders();

        foreach (string name in fileNames)
        {
            string filePath = $"{fileDir}\\{name}{fileExt}";

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

    /// <summary>
    /// Loads all of the Character TAE (AC6)
    /// </summary>
    public static void LoadChrBehaviorTimeActs(Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> targetBank, bool rootOnly = false)
    {
        string fileDir = @"\chr";
        string fileExt = @".behbnd.dcx";

        List<string> fileNames = MiscLocator.GetCharacterBehaviorTimeActBinders();

        foreach (string name in fileNames)
        {
            string filePath = $"{fileDir}\\{name}{fileExt}";

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

    /// <summary>
    /// Loads all of the Object TAE
    /// </summary>
    public static void LoadObjTimeActs(Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> targetBank, bool rootOnly = false)
    {
        string fileDir = @"\obj";
        string fileExt = @".objbnd.dcx";

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
                string folder = entry.Key;
                List<string> files = entry.Value;

                foreach (string name in files)
                {
                    string filePath = $"{fileDir}\\{folder}\\{name}{fileExt}";

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

            foreach (string name in fileNames)
            {
                string filePath = $"{fileDir}\\{name}{fileExt}";

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

    /// <summary>
    /// Loads passed Character TAE file (via path string)
    /// </summary>
    public static void LoadChrTimeAct(string path, Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> targetBank)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading TAE file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading TAE file.",
                    LogLevel.Warning);
            return;
        }

        string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        TimeActContainerWrapper fileStruct = new TimeActContainerWrapper(name, path);
        bool validFile = false;

        IBinder binder = null;

        // Loose .tae
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileStruct.IsContainerFile = false;

            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                TAE taeFile = TAE.Read(fileBytes);
                InternalTimeActWrapper tInfo = new(path, taeFile);
                fileStruct.InternalFiles.Add(tInfo);
                validFile = true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to read TAE file: {name} at {path}\n{ex}", LogLevel.Error);
            }
        }
        // Within .anibnd.dcx
        else
        {
            fileStruct.IsContainerFile = true;

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                Memory<byte> bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND3.Read(bytes);
            }
            else
            {
                Memory<byte> bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND4.Read(bytes);
            }

            if (binder != null)
            {
                foreach (BinderFile file in binder.Files)
                {
                    if (file.Name.Contains(".tae"))
                    {
                        if (file.Bytes.Length > 0)
                        {
                            try
                            {
                                TAE taeFile = TAE.Read(file.Bytes);
                                InternalTimeActWrapper tInfo = new(file.Name, taeFile);
                                fileStruct.InternalFiles.Add(tInfo);
                                validFile = true;
                            }
                            catch (Exception ex)
                            {
                                TaskLogs.AddLog($"Failed to read TAE file: {file.ID}\n{ex}", LogLevel.Error);
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
            var binderInfo = new TimeActBinderWrapper(binder, null, "");
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    /// <summary>
    /// Loads passed Object TAE file (via path string)
    /// </summary>
    public static void LoadObjTimeAct(string path, Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> targetBank, string aegFolder = "")
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading TAE file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading TAE file.",
                    LogLevel.Warning);
            return;
        }

        string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        TimeActContainerWrapper fileStruct = new TimeActContainerWrapper(name, path);
        bool validFile = false;

        IBinder binder = null;
        IBinder aniBinder = null;
        string aniBinderName = "";

        if (aegFolder != "")
        {
            fileStruct.AegFolder = aegFolder;
        }

        // Loose .tae
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fileStruct.IsContainerFile = false;

            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                TAE taeFile = TAE.Read(fileBytes);
                InternalTimeActWrapper tInfo = new(path, taeFile);
                fileStruct.InternalFiles.Add(tInfo);
                validFile = true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to read TAE file: {name} at {path}\n{ex}", LogLevel.Error);
            }
        }
        // Within .anibnd.dcx
        else
        {
            fileStruct.IsContainerFile = true;

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                Memory<byte> bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND3.Read(bytes);
            }
            else
            {
                Memory<byte> bytes = DCX.Decompress(path);
                if (bytes.Length > 0)
                    binder = BND4.Read(bytes);
            }

            if (binder != null)
            {
                foreach (BinderFile file in binder.Files)
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
                            foreach (BinderFile aniFile in aniBinder.Files)
                            {
                                if (aniFile.Name.Contains(".tae"))
                                {
                                    if (aniFile.Bytes.Length > 0)
                                    {
                                        try
                                        {
                                            TAE taeFile = TAE.Read(aniFile.Bytes);
                                            InternalTimeActWrapper tInfo = new(aniFile.Name, taeFile);
                                            fileStruct.InternalFiles.Add(tInfo);
                                            validFile = true;

                                            //TaskLogs.AddLog($"Added to bank: {file.Name}");
                                        }
                                        catch (Exception ex)
                                        {
                                            TaskLogs.AddLog($"Failed to read TAE file: {aniFile.ID}\n{ex}", LogLevel.Error);
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
            TimeActBinderWrapper binderInfo = new TimeActBinderWrapper(binder, aniBinder, aniBinderName);
            targetBank.Add(fileStruct, binderInfo);
        }
    }

    /// <summary>
    /// Async task for saving all modified TAE files in Time Act Editor
    /// </summary>
    public static async void SaveTimeActsTask()
    {
        IsSaving = true;

        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeActs = SaveTimeActs();

        bool result = await saveTimeActs;
        IsSaving = result;
    }

    /// <summary>
    /// Save modified TAE files (e.g. ChrBND/ObjBND containers with TAE files within).
    /// </summary>
    public static async Task<bool> SaveTimeActs()
    {
        foreach (var (info, binder) in FileChrBank)
        {
            if (info.IsModified)
            {
                await SaveTimeAct(info, binder);
            }
        }

        return false;
    }

    /// <summary>
    /// Save DS2 TAE files (e.g. loose compared to containered)
    /// </summary>
    public static void HandleDS2TimeActSave(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        string fileDir = @"\timeact\chr";
        string fileExt = @".tae";

        if (info.Path.Contains("obj"))
        {
            fileDir = @"\timeact\obj";
        }

        // Direct file with DS2
        byte[] fileBytes = info.InternalFiles.First().TAE.Write();

        string assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
        string assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";
        string assetModDir = $@"{Smithbox.ProjectRoot}\{fileDir}\";

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

            var filename = Path.GetFileNameWithoutExtension(assetMod);
            TaskLogs.AddLog($"Successfully saved TAE file: {filename} at {assetMod}");
        }
    }

    /// <summary>
    /// Save containered TAE files
    /// </summary>
    public static void HandleBinderContents(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo, IBinder binder)
    {
        // Write existing TAE, and discover files that need to be added
        foreach (InternalTimeActWrapper tInfo in info.InternalFiles)
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
        string internalFilepath = "";

        // Grab correct path for a TAE binderFile
        foreach (BinderFile file in binder.Files)
        {
            if (internalFilepath == "" && file.Name.Contains(".tae"))
                internalFilepath = file.Name;
        }

        // Get internal path without the tae file part
        string filename = Path.GetFileName(internalFilepath);
        string internalPath = internalFilepath.Replace(filename, "");

        // Create new binder files for the newly added internal files
        foreach (InternalTimeActWrapper tInfo in info.InternalFiles)
        {
            if (tInfo.MarkForAddition)
            {
                BinderFile newBinderfile = new BinderFile();
                int id = int.Parse(tInfo.Name.Substring(1));

                newBinderfile.ID = id;
                newBinderfile.Name = $"{internalPath}\\{tInfo.Name}.tae";
                newBinderfile.Bytes = tInfo.TAE.Write();

                binder.Files.Add(newBinderfile);
            }
        }

        // Remove internal files marked for removal
        foreach (InternalTimeActWrapper tInfo in info.InternalFiles)
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

                if (fileToRemove != null)
                {
                    binder.Files.Remove(fileToRemove);
                }
            }
        }
    }

    /// <summary>
    /// Async task for saving single modified TAE file in Time Act Editor
    /// </summary>
    public static async void SaveTimeActTask(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        IsSaving = true;

        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeAct = SaveTimeAct(info, binderInfo);

        bool result = await saveTimeAct;

        IsSaving = result;
    }

    /// <summary>
    /// Save modified TAE file (e.g. ChrBND/ObjBND containers with TAE files within).
    /// </summary>
    public static async Task<bool> SaveTimeAct(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        await Task.Delay(1000);

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
            if (binderInfo.InternalBinder != null)
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

            string assetRoot = $@"{Smithbox.GameRoot}\{fileDir}\{info.Name}{fileExt}";
            string assetMod = $@"{Smithbox.ProjectRoot}\{fileDir}\{info.Name}{fileExt}";

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
                else if (File.Exists(assetMod))
                {
                    File.Copy(assetMod, $@"{assetMod}.bak", true);
                }

                File.WriteAllBytes(assetMod, fileBytes);

                TaskLogs.AddLog($"Successfully saved TAE container: {info.Name} at {assetMod}");
            }
            else
            {
                TaskLogs.AddLog($"Failed to save TAE container: {info.Name} at {assetMod}.");
            }
        }

        return false;
    }

    /// <summary>
    /// Return the byte array for the passed binder.
    /// </summary>
    public static byte[] GetBinderBytes(IBinder targetBinder)
    {
        // Allow older compression types for these two since KRAK is slow
        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            BND4 writeBinder = targetBinder as BND4;
            TimeactCompressionType currentCompressionType = CFG.Current.CurrentTimeActCompressionType;

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
                case ProjectType.BB:
                case ProjectType.DS3:
                    return writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                case ProjectType.SDT:
                    return writeBinder.Write(DCX.Type.DCX_KRAK);
                case ProjectType.ER:
                    return writeBinder.Write(DCX.Type.DCX_KRAK);
                case ProjectType.AC6:
                    return writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                default:
                    break;
            }
        }

        return null;
    }

}
