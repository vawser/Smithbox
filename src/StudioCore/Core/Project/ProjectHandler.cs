using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SoulsFormats.Other.PlayStation3;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
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

namespace StudioCore.Core.Project;

public class ProjectHandler
{
    public Project CurrentProject;

    public ProjectModal ProjectModal;

    public Timer AutomaticSaveTimer;

    public RecentProject RecentProject;

    public bool IsInitialLoad = false;
    public bool ShowProjectLoadSelection = true;
    public bool RecentProjectLoad = false;

    public bool ImportRowNames = false;

    public ProjectHandler()
    {
        CurrentProject = new Project();
        ProjectModal = new ProjectModal();

        IsInitialLoad = true;
        UpdateProjectVariables();
    }
    public void OnGui()
    {
        if (!RecentProjectLoad && Current.Project_LoadRecentProjectImmediately)
        {
            RecentProjectLoad = true;
            IsInitialLoad = false;
            try
            {
                Smithbox.ProjectHandler.LoadProjectFromJSON(Current.LastProjectFile);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog("Failed to load recent project.");
            }
        }

        if (IsInitialLoad)
        {
            ImGui.OpenPopup("Project Creation");
        }

        if (ImGui.BeginPopupModal("Project Creation", ref IsInitialLoad, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
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
                "Failed to load last project. Project will not be loaded after restart.",
                "Project Load Error", MessageBoxButtons.OK);
            return false;
        }

        if (path == "")
        {
            PlatformUtils.Instance.MessageBox(
                $"Path parameter was empty: {path}",
                "Project Load Error", MessageBoxButtons.OK);
            return false;
        }

        CurrentProject.ProjectJsonPath = path;

        SetGameRootPrompt(CurrentProject);
        CheckUnpackedState(CurrentProject);
        CheckPtdeCollisionRoot(CurrentProject);

        // Only proceed if dll are found
        if (!CheckDecompressionDLLs(CurrentProject))
            return false;

        Smithbox.ProjectType = CurrentProject.Config.GameType;
        Smithbox.GameRoot = CurrentProject.Config.GameRoot;
        Smithbox.ProjectRoot = Path.GetDirectoryName(path);
        Smithbox.SmithboxDataRoot = $"{Smithbox.ProjectRoot}\\.smithbox";

        if (Smithbox.ProjectRoot == "")
            TaskLogs.AddLog("Smithbox.ProjectRoot is empty!");

        Smithbox.SetProgramTitle($"{CurrentProject.Config.ProjectName} - Smithbox");

        MapLocator.FullMapList = null;
        Smithbox.InitializeBanks();
        Smithbox.InitializeNameCaches();
        Smithbox.EditorHandler.UpdateEditors();

        Current.LastProjectFile = path;
        CFG.Save();
        UI.Save();

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
        RecentProject recent = new()
        {
            Name = targetProject.Config.ProjectName,
            GameType = targetProject.Config.GameType,
            ProjectFile = targetProject.ProjectJsonPath
        };
        AddMostRecentProject(recent);
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
        if (targetProject == null)
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

        if (!LocatorUtils.CheckFilesExpanded(targetProject.Config.GameRoot, targetProject.Config.GameType))
        {
            if (targetProject.Config.GameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
            {
                TaskLogs.AddLog(
                    $"The files for {targetProject.Config.GameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                    LogLevel.Error, LogPriority.High);
            }

            TaskLogs.AddLog(
                $"The files for {targetProject.Config.GameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
                LogLevel.Warning);
        }
    }

    /// <summary>
    /// When loading a DS1R project, displays a warning if PTDE_Collision_Root is not set or if the directory it points
    /// to does not exist.
    /// </summary>
    /// <param name="targetProject"></param>
    public void CheckPtdeCollisionRoot(Project targetProject)
    {
        if (targetProject.Config.GameType is not ProjectType.DS1R)
            return;

        if (Current.PTDE_Collision_Root == "" && Current.PTDE_Collision_Root_Warning)
        {
            TaskLogs.AddLog("No directory is set for Dark Souls 1 collision files. No collision functionality will be available. You can set the directory or disable this warning under Project Status in settings.",
                LogLevel.Warning, LogPriority.High);
        }
        else if (!Directory.Exists(Current.PTDE_Collision_Root))
        {
            TaskLogs.AddLog("The set Dark Souls 1 collision directory does not exist.", LogLevel.Warning);
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
                $"Could not find file \"{dllName}\" in \"{targetProject.Config.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
                "Error",
                MessageBoxButtons.OK);
            return false;
        }
        else
        {
            if (!File.Exists(projectDllPath))
            {
                File.Copy(rootDllPath, projectDllPath);
            }
        }

        return true;
    }

    public ProjectType GetProjectTypeFromExecutable(string exePath)
    {
        var type = ProjectType.Undefined;

        if (exePath.Contains("darksouls.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.DS1;
        }
        else if (exePath.Contains("darksoulsremastered.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.DS1R;
        }
        else if (exePath.Contains("darksoulsii.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.DS2S; // Default to SOTFS
        }
        else if (exePath.Contains("darksoulsiii.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.DS3;
        }
        else if (exePath.Contains("eboot.bin", StringComparison.InvariantCultureIgnoreCase))
        {
            var usrDir = Path.GetDirectoryName(exePath);
            var gameDir = Path.GetDirectoryName(usrDir);
            var sfoPath = $@"{gameDir}\PARAM.SFO";
            if (Directory.Exists($@"{usrDir}\dvdroot_ps4"))
            {
                type = ProjectType.BB;
            }
            else if (File.Exists(sfoPath))
            {
                PARAMSFO sfo = PARAMSFO.Read(sfoPath);
                type = GetProjectTypeFromPARAMSFO(sfo);
            }
            else
            {
                type = ProjectType.DES;
            }
        }
        else if (exePath.Contains("sekiro.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.SDT;
        }
        else if (exePath.Contains("eldenring.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.ER;
        }
        else if (exePath.Contains("armoredcore6.exe", StringComparison.InvariantCultureIgnoreCase))
        {
            type = ProjectType.AC6;
        }

        return type;
    }

    public ProjectType GetProjectTypeFromPARAMSFO(PARAMSFO sfo)
    {
        // Try to find the title name
        if (sfo.Parameters.TryGetValue("TITLE", out PARAMSFO.Parameter parameter))
        {
            switch (parameter.Data)
            {
                case "Armored Core 4":
                    return ProjectType.AC4;
                case "ARMORED CORE for Answer":
                    return ProjectType.ACFA;
                case "ARMORED CORE V":
                    return ProjectType.ACV;
                case "Armored Core Verdict Day":
                    return ProjectType.ACVD;
                case "Demon's Souls":
                    return ProjectType.DES;
            }
        }

        // Try to find the title ID
        if (sfo.Parameters.TryGetValue("TITLE_ID", out parameter))
        {
            switch (parameter.Data)
            {
                case "BLKS20001":
                case "BLJM60012":
                case "BLJM60062":
                case "BLUS30027":
                case "BLES00039":
                    return ProjectType.AC4;
                case "BLKS20066":
                case "BLJM55005":
                case "BLJM60066":
                case "BLUS30187":
                case "BLES00370":
                    return ProjectType.ACFA;
                case "BLKS20356":
                case "BLAS50448":
                case "BLJM60378":
                case "BLUS30516":
                case "BLES01440":
                    return ProjectType.ACV;
                case "BLKS20441":
                case "BLAS50618":
                case "BLJM61014":
                case "BLJM61020":
                case "BLUS31194":
                case "BLES01898":
                case "NPUB31245":
                case "NPEB01428":
                    return ProjectType.ACVD;
                case "BCKS10071":
                case "BCJS30022":
                case "BCJS70013":
                case "BCAS20071":
                case "BCAS20115":
                case "BLUS30443":
                case "BLUS30443CE":
                case "BLES00932":
                case "NPJA00102":
                case "NPUB30910":
                case "NPEB01202":
                    return ProjectType.DES;
            }
        }

        // Just return Demon's Souls
        return ProjectType.DES;
    }

    public void UpdateTimer()
    {
        if (AutomaticSaveTimer != null)
        {
            AutomaticSaveTimer.Close();
        }

        if (Current.System_EnableAutoSave)
        {
            var interval = Current.System_AutoSaveIntervalSeconds * 1000;
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
        if (Current.System_EnableAutoSave)
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                if (Current.System_EnableAutoSave_Project)
                {
                    WriteProjectConfig(CurrentProject);
                }

                if (Current.System_EnableAutoSave_MapEditor)
                {
                    Smithbox.EditorHandler.MapEditor.SaveAll();
                }

                if (Current.System_EnableAutoSave_ModelEditor)
                {
                    Smithbox.EditorHandler.ModelEditor.SaveAll();
                }

                if (Current.System_EnableAutoSave_ParamEditor)
                {
                    Smithbox.EditorHandler.ParamEditor.SaveAll();
                }

                if (Current.System_EnableAutoSave_TextEditor)
                {
                    Smithbox.EditorHandler.TextEditor.SaveAll();
                }

                if (Current.System_EnableAutoSave_GparamEditor)
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

        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DES
            if (p.GameType == ProjectType.DES)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DS1
            if (p.GameType == ProjectType.DS1)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DS1R
            if (p.GameType == ProjectType.DS1R)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DS2
            if (p.GameType == ProjectType.DS2)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DS2S
            if (p.GameType == ProjectType.DS2S)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // BB
            if (p.GameType == ProjectType.BB)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // DS3
            if (p.GameType == ProjectType.DS3)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // SDT
            if (p.GameType == ProjectType.SDT)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // ER
            if (p.GameType == ProjectType.ER)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // AC6
            if (p.GameType == ProjectType.AC6)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // ACFA
            if (p.GameType == ProjectType.ACFA)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // ACV
            if (p.GameType == ProjectType.ACV)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (RecentProject p in Current.RecentProjects.ToArray())
        {
            // ACVD
            if (p.GameType == ProjectType.ACVD)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
    }

    public void RecentProjectEntry(RecentProject p, int id)
    {
        // Just remove invalid recent projects immediately
        if (!File.Exists(p.ProjectFile))
        {
            RemoveRecentProject(p);
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
                    RemoveRecentProject(p);
                }
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                    $"Remove project from list of recent projects?",
                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove from list"))
            {
                RemoveRecentProject(p);
                CFG.Save();
                UI.Save();
            }

            ImGui.EndPopup();
        }
    }
}
