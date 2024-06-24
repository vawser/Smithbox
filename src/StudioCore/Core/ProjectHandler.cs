using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Interface.Modals;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Drawing;
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

    public bool IsInitialLoad = false;
    public bool ShowProjectLoadSelection = true;

    public ProjectHandler()
    {
        CurrentProject = new Project();
        NewProjectModal = new NewProjectModal();
    }

    public void HandleProjectSelection()
    {
        // Load previous project if it exists
        if (CFG.Current.LastProjectFile != null && CFG.Current.LastProjectFile != "" && File.Exists(CFG.Current.LastProjectFile))
        {
            var lastProjectPath = CFG.Current.LastProjectFile;

            CurrentProject.Config = ReadProjectConfig(lastProjectPath);
            
            UpdateProjectVariables();

            LoadProject(lastProjectPath, false);
        }
        // Otherwise display new project window
        else
        {
            IsInitialLoad = true;
            UpdateProjectVariables();
        }
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

        if(path == "")
        {
            PlatformUtils.Instance.MessageBox(
                $"Path parameter was empty: {path}",
                "Project Load Error", MessageBoxButtons.OK);
            return;
        }


        CurrentProject.ProjectJsonPath = path;

        SetGameRootPrompt(CurrentProject);
        CheckUnpackedState(CurrentProject);
        CheckDecompressionDLLs(CurrentProject);

        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(path);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";

        if (Smithbox.ProjectRoot == "")
            TaskLogs.AddLog("Smithbox.ProjectRoot is empty!");

        Smithbox.SetProgramTitle($"Smithbox - {CurrentProject.Config.ProjectName}");

        if (update)
        {
            ResourceMapLocator.FullMapList = null;
            Smithbox.BankHandler.UpdateBanks();
            Smithbox.EditorHandler.UpdateEditors();
            Smithbox.NameCacheHandler.UpdateCaches();

            CFG.Current.LastProjectFile = path;
            CFG.Save();

            AddProjectToRecentList(CurrentProject);
        }
    }

    public void LoadProjectFromJSON(string jsonPath)
    {
        if (CurrentProject == null)
        {
            CurrentProject = new Project();
        }

        // Fill CurrentProject.Config with contents
        CurrentProject.Config = ReadProjectConfig(jsonPath);

        if (CurrentProject.Config == null)
        {
            return;
        }

        LoadProject(jsonPath);
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
        Smithbox.SetProgramTitle($"Smithbox - {CurrentProject.Config.ProjectName}");
        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(CurrentProject.ProjectJsonPath);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";
    }
    public void OnGui()
    {
        if (IsInitialLoad)
        {
            ImGui.OpenPopup("Project Creation");
        }
        if (ImGui.BeginPopupModal("Project Creation", ref IsInitialLoad, ImGuiWindowFlags.AlwaysAutoResize))
        {
            NewProjectModal.DisplayProjectSelection();

            ImGui.EndPopup();
        }
    }

    public void AddProjectToRecentList(Project targetProject)
    {
        // Add to recent project list
        CFG.RecentProject recent = new()
        {
            Name = targetProject.Config.ProjectName,
            GameType = targetProject.Config.GameType,
            ProjectFile = targetProject.ProjectJsonPath
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

    public void WriteProjectConfig(Project targetProject)
    {
        if(targetProject == null) 
            return;

        var config = targetProject.Config;
        var writePath = targetProject.ProjectJsonPath;

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

    public void SetGameRootPrompt(Project targetProject)
    {
        if (targetProject == null)
            return;

        if (!Directory.Exists(targetProject.Config.GameRoot))
        {
            PlatformUtils.Instance.MessageBox(
                $@"Could not find game data directory for {targetProject.Config.GameType}. Please select the game executable.",
                "Error",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"Select executable for {targetProject.Config.GameType}...",
                        new[] { FilterStrings.GameExecutableFilter },
                        out var path))
                {
                    targetProject.Config.GameRoot = path;
                    ProjectType gametype = GetProjectTypeFromExecutable(targetProject.Config.GameRoot);

                    if (gametype == targetProject.Config.GameType)
                    {
                        targetProject.Config.GameRoot = Path.GetDirectoryName(targetProject.Config.GameRoot);

                        if (targetProject.Config.GameType == ProjectType.BB)
                        {
                            targetProject.Config.GameRoot += @"\dvdroot_ps4";
                        }

                        WriteProjectConfig(targetProject);

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

    public void CheckUnpackedState(Project targetProject)
    {
        if (targetProject == null)
            return;

        if (!ResourceLocatorUtils.CheckFilesExpanded(targetProject.Config.GameRoot, targetProject.Config.GameType))
        {
            if (targetProject.Config.GameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
            {
                TaskLogs.AddLog(
                    $"The files for {targetProject.Config.GameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                    LogLevel.Error, TaskLogs.LogPriority.High);
            }

            TaskLogs.AddLog(
                $"The files for {targetProject.Config.GameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
                LogLevel.Warning);
        }
    }

    public void CheckDecompressionDLLs(Project targetProject)
    {
        if (targetProject == null)
            return;

        if (targetProject.Config.GameType == ProjectType.SDT || targetProject.Config.GameType == ProjectType.ER)
        {
            StealGameDllIfMissing(targetProject, "oo2core_6_win64");
        }
        else if (targetProject.Config.GameType == ProjectType.AC6)
        {
            StealGameDllIfMissing(targetProject, "oo2core_8_win64");
        }
    }

    public void StealGameDllIfMissing(Project targetProject, string dllName)
    {
        if (targetProject == null)
            return;

        dllName = dllName + ".dll";

        var rootDllPath = Path.Join(targetProject.Config.GameRoot, dllName);
        var projectDllPath = Path.Join(Path.GetFullPath("."), dllName);

        if (!File.Exists(rootDllPath))
        {
            PlatformUtils.Instance.MessageBox(
                $"Could not find file \"{dllName}\" in \"{targetProject.Config.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
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
                    WriteProjectConfig(CurrentProject);
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

        LoadProjectFromJSON(projectJsonPath);
        Smithbox.ProjectHandler.IsInitialLoad = false;
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

                LoadProjectFromJSON(path);
                Smithbox.ProjectHandler.IsInitialLoad = false;
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
