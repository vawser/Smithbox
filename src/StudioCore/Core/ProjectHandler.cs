using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Interface.Modals;
using StudioCore.Localization;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using static StudioCore.CFG;

namespace StudioCore.Core;

public class ProjectHandler
{
    public Project CurrentProject;

    public ProjectModal ProjectModal;

    public Timer AutomaticSaveTimer;

    public CFG.RecentProject RecentProject;

    public bool IsInitialLoad = false;
    public bool ShowProjectLoadSelection = true;
    public bool RecentProjectLoad = false;

    public ProjectHandler()
    {
        CurrentProject = new Project();
        ProjectModal = new ProjectModal();

        IsInitialLoad = true;
        UpdateProjectVariables();
    }
    public void OnGui()
    {
        if (!RecentProjectLoad && CFG.Current.Project_LoadRecentProjectImmediately)
        {
            RecentProjectLoad = true;
            IsInitialLoad = false;
            try
            {
                Smithbox.ProjectHandler.LoadProjectFromJSON(CFG.Current.LastProjectFile);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{LOC.Get("PROJECT__FAILED_TO_LOAD_RECENT")}" + ex.Message);
            }
        }

        if (IsInitialLoad)
        {
            ImGui.OpenPopup($"{LOC.Get("PROJECT__PROJECT_CREATION")}##projectCreationModal");
        }

        if (ImGui.BeginPopupModal($"{LOC.Get("PROJECT__PROJECT_CREATION")}##projectCreationModal", ref IsInitialLoad, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ProjectModal.Display();

            ImGui.EndPopup();
        }
    }

    public void ReloadCurrentProject()
    {
        LoadProjectFromJSON(CurrentProject.ProjectJsonPath);
        Smithbox.ProjectHandler.IsInitialLoad = false;
    }

    public bool LoadProject(string path)
    {
        if (CurrentProject.Config == null)
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("PROJECT__PROJECT_LOAD_FAILED")}",
                $"{LOC.Get("PROJECT__PROJECT_LOAD_ERROR")}", 
                MessageBoxButtons.OK);
            return false;
        }

        if(path == "")
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("PROJECT__PROJECT_LOAD_EMPTY_PATH")}" +
                $"{path}",
                $"{LOC.Get("PROJECT__PROJECT_LOAD_ERROR")}",
                MessageBoxButtons.OK);
            return false;
        }

        CurrentProject.ProjectJsonPath = path;

        SetGameRootPrompt(CurrentProject);
        CheckUnpackedState(CurrentProject);

        // Only proceed if dll are found
        if (!CheckDecompressionDLLs(CurrentProject))
            return false;

        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(path);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";

        if (Smithbox.ProjectRoot == "")
            TaskLogs.AddLog($"{LOC.Get("PROJECT__EMPTY_PROJECT_ROOT")}");

        Smithbox.SetProgramTitle($"{CurrentProject.Config.ProjectName} - Smithbox");

        MapLocator.FullMapList = null;
        Smithbox.InitializeBanks();
        Smithbox.InitializeNameCaches();
        Smithbox.EditorHandler.UpdateEditors();

        CFG.Current.LastProjectFile = path;
        CFG.Save();

        AddProjectToRecentList(CurrentProject);

        UpdateTimer();

        // Re-create this so project setup settings don't persist between projects (e.g. Import Row Names)
        ProjectModal = new ProjectModal();

        return true;
    }

    public bool LoadProjectFromJSON(string jsonPath)
    {
        if (CurrentProject == null)
        {
            CurrentProject = new Project();
        }

        // Fill CurrentProject.Config with contents
        CurrentProject.Config = ReadProjectConfig(jsonPath);

        if (CurrentProject.Config == null)
        {
            return false;
        }

        return LoadProject(jsonPath);
    }

    public void ClearProject()
    {
        CurrentProject = null;
        Smithbox.SetProgramTitle("No Project - Smithbox");
        Smithbox.ProjectType = ProjectType.Undefined;
        Smithbox.GameRoot = "";
        Smithbox.ProjectRoot = "";
        Smithbox.SmithboxDataRoot = "";

        MapLocator.FullMapList = null;
    }

    public void UpdateProjectVariables()
    {
        Smithbox.SetProgramTitle($"{CurrentProject.Config.ProjectName} - Smithbox");
        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(CurrentProject.ProjectJsonPath);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";
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
                $"{LOC.Get("PROJECT__GAME_ROOT_INVALID")}",
                $"{LOC.Get("ERROR")}",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"{LOC.Get("PROJECT__SELECT_EXECUTABLE")}" + $"{targetProject.Config.GameType}...",
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
                        $"{LOC.Get("PROJECT__GAME_ROOT_INVALID")}",
                        $"{LOC.Get("ERROR")}",
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

        if (!LocatorUtils.CheckFilesExpanded(targetProject.Config.GameRoot, targetProject.Config.GameType))
        {
            if (targetProject.Config.GameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
            {
                TaskLogs.AddLog(
                    $"{LOC.Get("PROJECT__GAME_NOT_UNPACKED_DS1_DS2")}",
                    LogLevel.Error, TaskLogs.LogPriority.High);
            }

            TaskLogs.AddLog(
                $"{LOC.Get("PROJECT__GAME_NOT_UNPACKED")}",
                LogLevel.Warning);
        }
    }

    public bool CheckDecompressionDLLs(Project targetProject)
    {
        if (targetProject == null)
            return false;

        bool success = true;

        if (targetProject.Config.GameType == ProjectType.SDT || targetProject.Config.GameType == ProjectType.ER)
        {
            success = false;
            success = StealGameDllIfMissing(targetProject, "oo2core_6_win64");
        }
        else if (targetProject.Config.GameType == ProjectType.AC6)
        {
            success = false;
            success = StealGameDllIfMissing(targetProject, "oo2core_8_win64");
        }

        return success;
    }

    public bool StealGameDllIfMissing(Project targetProject, string dllName)
    {
        if (targetProject == null)
            return false;

        dllName = dllName + ".dll";

        var rootDllPath = Path.Join(targetProject.Config.GameRoot, dllName);
        var projectDllPath = Path.Join(Path.GetFullPath("."), dllName);

        if (!File.Exists(rootDllPath))
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("PROJECT__DLL_MISSING_IN_GAME_ROOT")}" +
                $"\n{LOC.Get("PROJECT__GAME_DLL")}" + $"{dllName}" +
                $"\n{LOC.Get("PROJECT__GAME_ROOT")}" + $"{targetProject.Config.GameRoot}",
                $"{LOC.Get("ERROR")}",
                MessageBoxButtons.OK);
            return false;
        }
        else
        {
            if(!File.Exists(projectDllPath))
            {
                File.Copy(rootDllPath, projectDllPath);
            }
        }

        return true;
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

    public void SaveCurrentProject()
    {
        WriteProjectConfig(CurrentProject);
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

                TaskLogs.AddLog($"{LOC.Get("PROJECT__AUTOMATIC_SAVE_OCCURED")}" + $"{e.SignalTime}");
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
        var success = PlatformUtils.Instance.OpenFileDialog($"{LOC.Get("PROJECT__CHOOSE_PROJECT_JSON")}", new[] { FilterStrings.ProjectJsonFilter }, out var projectJsonPath);

        if (projectJsonPath != null)
        {
            if (projectJsonPath.Contains("project.json"))
            {
                if (LoadProjectFromJSON(projectJsonPath))
                {
                    Smithbox.ProjectHandler.IsInitialLoad = false;
                }
            }
        }
    }

    public void LoadRecentProject()
    {
        // Only set this to false if recent project load is sucessful
        if (LoadProjectFromJSON(Current.LastProjectFile))
        {
            Smithbox.ProjectHandler.IsInitialLoad = false;
        }
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
        // Just remove invalid recent projects immediately
        if(!File.Exists(p.ProjectFile))
        {
            CFG.RemoveRecentProject(p);
        }

        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
        {
            if (File.Exists(p.ProjectFile))
            {
                var path = p.ProjectFile;

                if (LoadProjectFromJSON(path))
                {
                    Smithbox.ProjectHandler.IsInitialLoad = false;
                    UpdateProjectVariables();
                }
                else
                {
                    // Remove it if it failed
                    CFG.RemoveRecentProject(p);
                }
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT__PROJECT_FILE_DOES_NOT_EXIST")}" +
                    $"{LOC.Get("PROJECT__PROJECT_FILE")}" + $"{p.ProjectFile}",
                    $"{LOC.Get("PROJECT__PROJECT_MISSING")}", 
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable($"{LOC.Get("PROJECT__REMOVE_FROM_LIST")}##removeFromListButton"))
            {
                CFG.RemoveRecentProject(p);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }
}
