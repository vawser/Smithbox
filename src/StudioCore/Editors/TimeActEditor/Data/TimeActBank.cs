using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static StudioCore.Configuration.Settings.TimeActEditorTab;

namespace StudioCore.Editors.TimeActEditor.Bank;

public class TimeActBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public string SourcePath;
    public string FallbackPath;

    public TimeActType TimeActType;

    public Dictionary<TimeActContainerWrapper, TimeActBinderWrapper> Entries;

    public TimeActBank(Smithbox baseEditor, ProjectEntry project, TimeActType taeType, string sourcePath, string fallbackPath)
    {
        BaseEditor = baseEditor;
        Project = project;

        SourcePath = sourcePath;
        FallbackPath = fallbackPath;

        TimeActType = taeType;

        Entries = new();
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        if(TimeActType is TimeActType.Character)
        {
            string fileDir = @"\chr";
            string fileExt = @".anibnd.dcx";

            if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                fileDir = @"\timeact\chr";
                fileExt = @".tae";
            }

            List<string> fileNames = MiscLocator.GetCharacterTimeActBinders(Project);

            foreach (string name in fileNames)
            {
                string filePath = $"{fileDir}\\{name}{fileExt}";

                if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
                {
                    LoadChrTimeAct($"{Project.ProjectPath}\\{filePath}");
                }
                else
                {
                    LoadChrTimeAct($"{Project.DataPath}\\{filePath}");
                }
            }

            // AC6 - Load behbnd ones too
            if(Project.ProjectType is ProjectType.AC6)
            {
                fileDir = @"\chr";
                fileExt = @".behbnd.dcx";
                fileNames = MiscLocator.GetCharacterBehaviorTimeActBinders(Project);

                foreach (string name in fileNames)
                {
                    string filePath = $"{fileDir}\\{name}{fileExt}";

                    if (File.Exists($"{Project.ProjectPath}\\{filePath}"))
                    {
                        LoadChrTimeAct($"{Project.ProjectPath}\\{filePath}");
                    }
                    else
                    {
                        LoadChrTimeAct($"{Project.DataPath}\\{filePath}");
                    }
                }
            }
        }

        if (TimeActType is TimeActType.Object)
        {
            string fileDir = @"\obj";
            string fileExt = @".objbnd.dcx";

            if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                fileDir = @"\timeact\obj";
                fileExt = @".tae";
            }

            if (Project.ProjectType is ProjectType.AC6)
            {
                fileDir = @"\asset\environment\geometry\";
                fileExt = @".geombnd.dcx";
            }

            // ER handling for AEG folders
            if (Project.ProjectType is ProjectType.ER)
            {
                fileDir = @"\asset\aeg\";
                fileExt = @".geombnd.dcx";

                Dictionary<string, List<string>> assetDict = new();

                if (Project.ProjectType is ProjectType.ER)
                {
                    assetDict = MiscLocator.GetAssetTimeActBinders_ER(Project);
                }

                foreach (var entry in assetDict)
                {
                    string folder = entry.Key;
                    List<string> files = entry.Value;

                    foreach (string name in files)
                    {
                        string filePath = $"{fileDir}\\{folder}\\{name}{fileExt}";
                        if (File.Exists($"{SourcePath}\\{filePath}"))
                        {
                            LoadObjTimeAct($"{SourcePath}\\{filePath}", folder);
                        }
                        else
                        {
                            LoadObjTimeAct($"{FallbackPath}\\{filePath}", folder);
                        }
                    }
                }
            }
            else
            {
                List<string> fileNames = MiscLocator.GetObjectTimeActBinders(Project);

                if (Project.ProjectType is ProjectType.AC6)
                {
                    fileNames = MiscLocator.GetAssetTimeActBinders_AC6(Project);
                }

                foreach (string name in fileNames)
                {
                    string filePath = $"{fileDir}\\{name}{fileExt}";

                    if (File.Exists($"{SourcePath}\\{filePath}"))
                    {
                        LoadObjTimeAct($"{SourcePath}\\{filePath}");
                    }
                    else
                    {
                        LoadObjTimeAct($"{FallbackPath}\\{filePath}");
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Loads passed Character TAE file (via path string)
    /// </summary>
    public void LoadChrTimeAct(string path)
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
        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
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
            Entries.Add(fileStruct, binderInfo);
        }
    }

    /// <summary>
    /// Loads passed Object TAE file (via path string)
    /// </summary>
    public void LoadObjTimeAct(string path, string aegFolder = "")
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
        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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

            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
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

                        if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
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
            Entries.Add(fileStruct, binderInfo);
        }
    }

    /// <summary>
    /// Async task for saving all modified TAE files in Time Act Editor
    /// </summary>
    public async Task<bool> SaveTimeActsTask()
    {
        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeActs = SaveTimeActs();
        bool result = await saveTimeActs;

        return result;
    }

    /// <summary>
    /// Save modified TAE files (e.g. ChrBND/ObjBND containers with TAE files within).
    /// </summary>
    public async Task<bool> SaveTimeActs()
    {
        foreach (var (info, binder) in Entries)
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
    public void HandleDS2TimeActSave(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        string fileDir = @"\timeact\chr";
        string fileExt = @".tae";

        if (info.Path.Contains("obj"))
        {
            fileDir = @"\timeact\obj";
        }

        // Direct file with DS2
        byte[] fileBytes = info.InternalFiles.First().TAE.Write();

        string assetRoot = $@"{Project.DataPath}\{fileDir}\{info.Name}{fileExt}";
        string assetMod = $@"{Project.ProjectPath}\{fileDir}\{info.Name}{fileExt}";
        string assetModDir = $@"{Project.ProjectPath}\{fileDir}\";

        if (!Directory.Exists(assetModDir))
        {
            Directory.CreateDirectory(assetModDir);
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Project.ProjectPath == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
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
            TaskLogs.AddLog($"Time Act Editor: saved TAE file: {filename} at {assetMod}");
        }
    }

    /// <summary>
    /// Save containered TAE files
    /// </summary>
    public void HandleBinderContents(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo, IBinder binder)
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
    public async void SaveTimeActTask(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        // Load the maps async so the main thread isn't blocked
        Task<bool> saveTimeAct = SaveTimeAct(info, binderInfo);
        bool result = await saveTimeAct;
    }

    /// <summary>
    /// Save modified TAE file (e.g. ChrBND/ObjBND containers with TAE files within).
    /// </summary>
    public async Task<bool> SaveTimeAct(TimeActContainerWrapper info, TimeActBinderWrapper binderInfo)
    {
        await Task.Delay(1);

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
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

                if (Project.ProjectType is ProjectType.ER)
                {
                    fileDir = @$"\asset\aeg\{info.AegFolder}";
                    fileExt = @".geombnd.dcx";
                }

                if (Project.ProjectType is ProjectType.AC6)
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

            string assetRoot = $@"{Project.DataPath}\{fileDir}\{info.Name}{fileExt}";
            string assetMod = $@"{Project.ProjectPath}\{fileDir}\{info.Name}{fileExt}";

            if (fileBytes != null)
            {
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
                else if (File.Exists(assetMod))
                {
                    File.Copy(assetMod, $@"{assetMod}.bak", true);
                }

                File.WriteAllBytes(assetMod, fileBytes);

                TaskLogs.AddLog($"Time Act Editor: saved TAE container: {info.Name} at {assetMod}");
            }
            else
            {
                TaskLogs.AddLog($"Time Act Editor: failed to save TAE container: {info.Name} at {assetMod}.");
            }
        }

        return false;
    }

    /// <summary>
    /// Return the byte array for the passed binder.
    /// </summary>
    public byte[] GetBinderBytes(IBinder targetBinder)
    {
        // Allow older compression types for these two since KRAK is slow
        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
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
        if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            BND3 writeBinder = targetBinder as BND3;

            switch (Project.ProjectType)
            {
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    return writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
            }
        }
        else
        {
            BND4 writeBinder = targetBinder as BND4;

            switch (Project.ProjectType)
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

public enum TimeActType
{
    Character,
    Object
}
