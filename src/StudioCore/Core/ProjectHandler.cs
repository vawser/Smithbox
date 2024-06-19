using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Interface;
using StudioCore.Interface.Modals;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using static StudioCore.CFG;

namespace StudioCore.Core;

public class ProjectHandler
{
    public Project CurrentProject;

    public NewProjectModal NewProjectModal;

    public Timer AutomaticSaveTimer;

    public CFG.RecentProject RecentProject;

    public ProjectHandler()
    {
        CurrentProject = new Project();
        NewProjectModal = new NewProjectModal();
    }

    public void LoadLastProject()
    {
        if (CFG.Current.LastProjectFile == null)
            return;

        if (CFG.Current.LastProjectFile == "")
            return;

        if (!File.Exists(CFG.Current.LastProjectFile))
            return;

        var lastProjectPath = CFG.Current.LastProjectFile;

        // Fill CurrentProject.Config with contents
        CurrentProject.Config = ReadProjectConfig(lastProjectPath);

        if (CurrentProject.Config == null)
        {
            CFG.Current.LastProjectFile = "";
            CFG.Save();
            return;
        }

        LoadProject(lastProjectPath, false);
    }

    public void LoadProject(string path, bool update = true)
    {
        if (CurrentProject.Config == null)
        {
            PlatformUtils.Instance.MessageBox(
                "Failed to load last project. Project will not be loaded after restart.",
                "Project Load Error", MessageBoxButtons.OK);
            return;
        }

        SetGameRootPrompt();
        CheckUnpackedState();
        CheckDecompressionDLLs();

        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(path);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";
        CurrentProject.ProjectJsonPath = path;
        CFG.Current.LastProjectFile = path;
        CFG.Save();

        Smithbox.SetProgramTitle($"Smithbox - {CurrentProject.Config.ProjectName}");

        if (update)
        {
            ResourceMapLocator.FullMapList = null;
            Smithbox.BankHandler.UpdateBanks();
            Smithbox.EditorHandler.UpdateEditors();
            Smithbox.NameCacheHandler.UpdateCaches();
        }
    }

    public void ClearProject()
    {
        CurrentProject = null;
        Smithbox.SetProgramTitle("Smithbox - No Project");
        Smithbox.ProjectType = ProjectType.Undefined;
        Smithbox.GameRoot = "";
        Smithbox.ProjectRoot = "";
        Smithbox.SmithboxDataRoot = "";

        ResourceMapLocator.FullMapList = null;
    }

    public void UpdateProjectVariables()
    {
        if (CurrentProject == null)
            return;

        Smithbox.SetProgramTitle($"Smithbox - {CurrentProject.Config.ProjectName}");
        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(CurrentProject.ProjectJsonPath);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";
    }

    public void AddProjectToRecentList()
    {
        // Add to recent project list
        CFG.RecentProject recent = new()
        {
            Name = CurrentProject.Config.ProjectName,
            GameType = CurrentProject.Config.GameType,
            ProjectFile = CurrentProject.ProjectJsonPath
        };
        CFG.AddMostRecentProject(recent);
    }

    public ProjectConfiguration ReadProjectConfig(string path)
    {
        var config = new ProjectConfiguration();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                config = JsonSerializer.Deserialize(stream, ProjectConfigurationSerializationContext.Default.ProjectConfiguration);
            }
        }

        return config;
    }

    public void WriteProjectConfig()
    {
        if(CurrentProject == null) 
            return;

        var config = CurrentProject.Config;
        var writePath = CurrentProject.ProjectJsonPath;

        if (writePath != "")
        {
            string jsonString = JsonSerializer.Serialize(config, typeof(ProjectConfiguration), ProjectConfigurationSerializationContext.Default);

            try
            {
                var fs = new FileStream(writePath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public void SetGameRootPrompt()
    {
        if (CurrentProject == null)
            return;

        if (!Directory.Exists(CurrentProject.Config.GameRoot))
        {
            PlatformUtils.Instance.MessageBox(
                $@"Could not find game data directory for {CurrentProject.Config.GameType}. Please select the game executable.",
                "Error",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"Select executable for {CurrentProject.Config.GameType}...",
                        new[] { FilterStrings.GameExecutableFilter },
                        out var path))
                {
                    CurrentProject.Config.GameRoot = path;
                    ProjectType gametype = GetProjectTypeFromExecutable(CurrentProject.Config.GameRoot);

                    if (gametype == CurrentProject.Config.GameType)
                    {
                        CurrentProject.Config.GameRoot = Path.GetDirectoryName(CurrentProject.Config.GameRoot);

                        if (CurrentProject.Config.GameType == ProjectType.BB)
                        {
                            CurrentProject.Config.GameRoot += @"\dvdroot_ps4";
                        }

                        WriteProjectConfig();

                        break;
                    }

                    PlatformUtils.Instance.MessageBox(
                        $@"Selected executable was not for {CurrentProject.Config.GameType}. Please select the correct game executable.",
                        "Error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void CheckUnpackedState()
    {
        if (CurrentProject == null)
            return;

        if (!ResourceLocatorUtils.CheckFilesExpanded(CurrentProject.Config.GameRoot, CurrentProject.Config.GameType))
        {
            if (CurrentProject.Config.GameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
            {
                TaskLogs.AddLog(
                    $"The files for {CurrentProject.Config.GameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                    LogLevel.Error, TaskLogs.LogPriority.High);
            }

            TaskLogs.AddLog(
                $"The files for {CurrentProject.Config.GameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
                LogLevel.Warning);
        }
    }

    public void CheckDecompressionDLLs()
    {
        if (CurrentProject == null)
            return;

        if (CurrentProject.Config.GameType == ProjectType.SDT || CurrentProject.Config.GameType == ProjectType.ER)
        {
            StealGameDllIfMissing("oo2core_6_win64");
        }
        else if (CurrentProject.Config.GameType == ProjectType.AC6)
        {
            StealGameDllIfMissing("oo2core_8_win64");
        }
    }

    public void StealGameDllIfMissing(string dllName)
    {
        if (CurrentProject == null)
            return;

        dllName = dllName + ".dll";

        var rootDllPath = Path.Join(CurrentProject.Config.GameRoot, dllName);
        var projectDllPath = Path.Join(Path.GetFullPath("."), dllName);

        if (!File.Exists(rootDllPath))
        {
            PlatformUtils.Instance.MessageBox(
                $"Could not find file \"{dllName}\" in \"{CurrentProject.Config.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
                "Error",
                MessageBoxButtons.OK);
        }
        else
        {
            if(!File.Exists(projectDllPath))
            {
                File.Copy(rootDllPath, projectDllPath);
            }
        }
    }

    public ProjectType GetProjectTypeFromExecutable(string exePath)
    {
        var type = ProjectType.Undefined;

        if (exePath.ToLower().Contains("darksouls.exe"))
        {
            type = ProjectType.DS1;
        }
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
        {
            type = ProjectType.DS1R;
        }
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
        {
            type = ProjectType.DS2S; // Default to SOTFS
        }
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
        {
            type = ProjectType.DS3;
        }
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
            {
                type = ProjectType.BB;
            }
            else
            {
                type = ProjectType.DES;
            }
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
        {
            type = ProjectType.SDT;
        }
        else if (exePath.ToLower().Contains("eldenring.exe"))
        {
            type = ProjectType.ER;
        }
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
        {
            type = ProjectType.AC6;
        }

        return type;
    }

    public void UpdateTimer()
    {
        if (AutomaticSaveTimer != null)
        {
            AutomaticSaveTimer.Close();
        }

        if (CFG.Current.System_EnableAutoSave)
        {
            var interval = CFG.Current.System_AutoSaveIntervalSeconds * 1000;
            if (interval < 10000)
                interval = 10000;

            AutomaticSaveTimer = new Timer(interval);
            AutomaticSaveTimer.Elapsed += OnAutomaticSave;
            AutomaticSaveTimer.AutoReset = true;
            AutomaticSaveTimer.Enabled = true;
        }
    }

    public void OnAutomaticSave(object source, ElapsedEventArgs e)
    {
        if (CFG.Current.System_EnableAutoSave)
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                if (CFG.Current.System_EnableAutoSave_Project)
                {
                    WriteProjectConfig();
                }

                if (CFG.Current.System_EnableAutoSave_MapEditor)
                {
                    Smithbox.EditorHandler.MapEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ModelEditor)
                {
                    Smithbox.EditorHandler.ModelEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ParamEditor)
                {
                    Smithbox.EditorHandler.ParamEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_TextEditor)
                {
                    Smithbox.EditorHandler.TextEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_GparamEditor)
                {
                    Smithbox.EditorHandler.GparamEditor.SaveAll();
                }

                TaskLogs.AddLog($"Automatic Save occured at {e.SignalTime}");
            }
        }
    }

    public bool CreateRecoveryProject()
    {
        if (Smithbox.GameRoot == null || Smithbox.ProjectRoot == null)
            return false;

        try
        {
            var time = DateTime.Now.ToString("dd-MM-yyyy-(hh-mm-ss)", CultureInfo.InvariantCulture);

            Smithbox.ProjectRoot = Smithbox.ProjectRoot + $@"\recovery\{time}";

            if (!Directory.Exists(Smithbox.ProjectRoot))
            {
                Directory.CreateDirectory(Smithbox.ProjectRoot);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public void OpenProjectDialog()
    {
        var success = PlatformUtils.Instance.OpenFileDialog("Choose the project json file", new[] { FilterStrings.ProjectJsonFilter }, out var projectJsonPath);

        // Fill CurrentProject.Config with contents
        CurrentProject.Config = ReadProjectConfig(projectJsonPath);

        if (CurrentProject.Config == null)
        {
            return;
        }

        LoadProject(projectJsonPath);
    }

    public void DisplayRecentProjects()
    {
        RecentProject = null;
        var id = 0;

        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DES
            if (p.GameType == ProjectType.DES)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1
            if (p.GameType == ProjectType.DS1)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1R
            if (p.GameType == ProjectType.DS1R)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2
            if (p.GameType == ProjectType.DS2)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2S
            if (p.GameType == ProjectType.DS2S)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // BB
            if (p.GameType == ProjectType.BB)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS3
            if (p.GameType == ProjectType.DS3)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // SDT
            if (p.GameType == ProjectType.SDT)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // ER
            if (p.GameType == ProjectType.ER)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // AC6
            if (p.GameType == ProjectType.AC6)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
    }

    public void RecentProjectEntry(CFG.RecentProject p, int id)
    {
        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
        {
            if (File.Exists(p.ProjectFile))
            {
                var path = p.ProjectFile;

                if(CurrentProject == null)
                {
                    CurrentProject = new Project();
                }

                // Fill CurrentProject.Config with contents
                CurrentProject.Config = ReadProjectConfig(path);

                if (CurrentProject.Config == null)
                {
                    return;
                }

                LoadProject(path);
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                    $"Remove project from list of recent projects?",
                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove from list"))
            {
                CFG.RemoveRecentProject(p);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }
}
