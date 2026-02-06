using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Application;

public class ProjectOrchestrator : IDisposable
{
    public List<ProjectEntry> Projects = new();

    public ProjectEntry SelectedProject;

    public bool IsProjectLoading = false;

    public ProjectLoadProgress LoadProgress;
    public Action<ProjectLoadProgress> ReportProgress;
    private readonly object _progressLock = new();

    public ActionManager ActionManager;

    public ProjectCreationMenu CreationMenu;
    public ProjectEnumMenu EnumMenu;
    public ProjectAliasMenu AliasMenu;

    public string ProjectListFilter = "";
    private bool InitExistingProjects = false;

    public ProjectOrchestrator()
    {
        ReportProgress = SetProgress;

        ActionManager = new();

        CreationMenu = new(this);
        EnumMenu = new(this);
        AliasMenu = new(this);

        SetupFolders();
    }

    public void Update(float dt)
    {
        // Project Dock
        if (SelectedProject != null && SelectedProject.Initialized)
        {
            SelectedProject.Update(dt);
        }
        else
        {
            ImGui.Begin("No Project##LoadingWindow", UIHelper.GetMainWindowFlags());
            ImGui.Text("No project has been loaded.");
            ImGui.End();
        }

        CreationMenu.Draw();
        EnumMenu.Draw();
        AliasMenu.Draw();

        if (!Smithbox.FirstFrame && !InitExistingProjects)
        {
            InitExistingProjects = true;

            if (CFG.Current.Project_Enable_Auto_Load)
            {
                _ = LoadExistingProjects();
            }
        }

        DrawProjectLoadingUI();
    }

    public void DisplayMenuOptions()
    {
        // Current Project
        if (Projects.Count > 0)
        {
            DisplayCurrentProjectMenu();

            ImGui.Separator();
        }

        // Project Selection
        if (ImGui.BeginMenu($"Available Projects"))
        {
            DisplayProjectSelectionMenu();

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Project Creation"))
        {
            if (ImGui.MenuItem("Create New Project"))
            {
                CreationMenu.IsEditMode = false;
                CreationMenu.IsDisplayed = true;
            }
            UIHelper.Tooltip($"Add a new project to the project list.");

            if (ImGui.MenuItem("Create Project from Project.JSON"))
            {
                var projectJsonPath = "";
                var result = PlatformUtils.Instance.OpenFileDialog("Select Project JSON", out projectJsonPath);

                if (result)
                {
                    CreateProjectFromLegacyJson(projectJsonPath);
                }
            }
            UIHelper.Tooltip($"Create a new project from a project.json file.");

            ImGui.EndMenu();
        }
    }

    private void DisplayCurrentProjectMenu()
    {
        foreach (var project in Projects)
        {
            if (project != SelectedProject)
                continue;

            var imGuiID = project.Descriptor.ProjectGUID;
            var projectName = $"{project.Descriptor.ProjectName}";

            if (ImGui.BeginMenu($"Current Project##projectEntry_{imGuiID}"))
            {
                DisplayProjectActions(project);

                ImGui.EndMenu();
            }
        }
    }
    private void DisplayProjectSelectionMenu()
    {
        // Project Selection
        var projectNameInput = ImGui.InputText("##projectFilterInput", ref ProjectListFilter, 255);

        UIHelper.Tooltip("Filter the project list by this term.");

        ImGui.BeginTabBar("##projectSelectionTabBar");

        // Project List
        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DES))
        {
            if (ImGui.BeginTabItem("DES##tab_DES"))
            {
                DisplayProjectListGroup(ProjectType.DES);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1))
        {
            if (ImGui.BeginTabItem("DS1##tab_DS1"))
            {
                DisplayProjectListGroup(ProjectType.DS1);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1R))
        {
            if (ImGui.BeginTabItem("DS1R##tab_DS1R"))
            {
                DisplayProjectListGroup(ProjectType.DS1R);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2))
        {
            if (ImGui.BeginTabItem("DS2##tab_DS2"))
            {
                DisplayProjectListGroup(ProjectType.DS2);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2S))
        {
            if (ImGui.BeginTabItem("DS2S##tab_DS2S"))
            {
                DisplayProjectListGroup(ProjectType.DS2S);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS3))
        {
            if (ImGui.BeginTabItem("DS3##tab_DS3"))
            {
                DisplayProjectListGroup(ProjectType.DS3);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.BB))
        {
            if (ImGui.BeginTabItem("BB##tab_BB"))
            {
                DisplayProjectListGroup(ProjectType.BB);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.SDT))
        {
            if (ImGui.BeginTabItem("SDT##tab_SDT"))
            {
                DisplayProjectListGroup(ProjectType.SDT);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.ER))
        {
            if (ImGui.BeginTabItem("ER##tab_ER"))
            {
                DisplayProjectListGroup(ProjectType.ER);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.AC6))
        {
            if (ImGui.BeginTabItem("AC6##tab_AC6"))
            {
                DisplayProjectListGroup(ProjectType.AC6);
                ImGui.EndTabItem();
            }
        }

        if (Projects.Any(e => e.Descriptor.ProjectType is ProjectType.NR))
        {
            if (ImGui.BeginTabItem("NR##tab_NR"))
            {
                DisplayProjectListGroup(ProjectType.NR);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
    }

    private void DisplayProjectListGroup(ProjectType projectType)
    {
        var projectList = Projects.Where(e => e.Descriptor.ProjectType == projectType).ToList();

        if (!projectList.Any()) 
            return;

        DisplayProjectList(projectList);
    }

    private void DisplayProjectList(List<ProjectEntry> projectList)
    {
        Dictionary<string, List<ProjectEntry>> _projectFolders = new();

        if (projectList.Count > 0)
        {
            foreach (var project in projectList)
            {
                if (project == SelectedProject)
                    continue;

                if(project.Descriptor.FolderTag != "")
                {
                    if (_projectFolders.ContainsKey(project.Descriptor.FolderTag))
                    {
                        _projectFolders[project.Descriptor.FolderTag].Add(project);
                    }
                    else
                    {
                        _projectFolders.Add(project.Descriptor.FolderTag, new List<ProjectEntry>() { project });
                    }
                }
            }
        }

        // Associated Projects
        foreach(var folder in _projectFolders)
        {
            var name = folder.Key;
            var projects = folder.Value;

            if (ImGui.BeginMenu($"{name}##projectFolder_{name}"))
            {
                foreach (var project in projects)
                {
                    if (project == SelectedProject)
                        continue;

                    if (!FilterProjectEntry(project))
                        continue;

                    DisplayProjectListEntry(project);
                }

                ImGui.EndMenu();
            }
        }

        if(_projectFolders.Count > 0 && projectList.Any(e => e.Descriptor.FolderTag == ""))
        {
            ImGui.Separator();
        }

        // Unassociated Projects
        if (projectList.Count > 0)
        {
            foreach (var project in projectList)
            {
                if (project == SelectedProject)
                    continue;

                if (!FilterProjectEntry(project))
                    continue;

                // Skip these as they are handled above
                if (project.Descriptor.FolderTag != "")
                    continue;

                DisplayProjectListEntry(project);
            }
        }
    }

    private void DisplayProjectListEntry(ProjectEntry project)
    {
        var imGuiID = project.Descriptor.ProjectGUID;
        var projectName = $"{project.Descriptor.ProjectName}";

        if (ImGui.BeginMenu($"{projectName}##projectEntry_{project.Descriptor.FolderTag}_{imGuiID}"))
        {
            DisplayProjectActions(project);

            ImGui.EndMenu();
        }
    }

    private bool FilterProjectEntry(ProjectEntry curProject)
    {
        var projName = curProject.Descriptor.ProjectName;

        if(ProjectListFilter != "")
        {
            if(!projName.ToLower().Contains(ProjectListFilter.ToLower()))
            {
                return false;
            }
        }

        return true;
    }

    private void DisplayProjectActions(ProjectEntry curProject)
    {
        ImGui.PushID(curProject.Descriptor.ProjectGUID.ToString());

        ImGui.Text(curProject.Descriptor.ProjectName);

        if (ImGui.BeginPopupContextItem($"##projectNameContext"))
        {
            ImGui.Text($"{curProject.Descriptor.ProjectGUID}");

            ImGui.Separator();

            if (ImGui.Selectable("Copy GUID"))
            {
                PlatformUtils.Instance.SetClipboardText($"{curProject.Descriptor.ProjectGUID}");
            }

            ImGui.EndPopup();
        }

        ImGui.Separator();

        if (!curProject.Initialized)
        {
            if (ImGui.MenuItem("Load"))
            {
                SelectedProject = curProject;

                if (!IsProjectLoading)
                {
                    _ = StartupProject(curProject);
                }
            }
        }
        else
        {
            if (ImGui.MenuItem("Select"))
            {
                SelectedProject = curProject;
                Smithbox.Instance.SetProgramName(curProject);

                if (CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment)
                {
                    SetAsAutoLoad(curProject);
                }
            }

            if (ImGui.MenuItem($"Unload##unloadProject"))
            {
                UnloadProject(curProject);
            }

            if (ImGui.MenuItem($"Reload##reloadProject"))
            {
                SelectedProject = curProject;

                if (!IsProjectLoading)
                {
                    _ = ReloadProject(curProject);
                }
            }
        }

        ImGui.Separator();

        if (ImGui.MenuItem($"Open Project Settings##projectSettings"))
        {
            CreationMenu.Project = curProject;
            CreationMenu.Descriptor = curProject.Descriptor;
            CreationMenu.IsEditMode = true;
            CreationMenu.IsDisplayed = true;
        }

        if (curProject.Initialized)
        {
            if (ImGui.MenuItem($"Open Project Aliases##projectAliases"))
            {
                AliasMenu.Setup(SelectedProject);
                AliasMenu.IsDisplayed = true;
            }

            if (ImGui.MenuItem($"Open Project Enums##projectEnums"))
            {
                EnumMenu.IsDisplayed = true;
            }
        }


        if (!CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment)
        {
            ImGui.Separator();

            if (ImGui.MenuItem($"Set to Auto-Load"))
            {
                SetAsAutoLoad(curProject);
            }
        }

        // ME3
        if (ModEngineHandler.IsME3Project(curProject))
        {
            ImGui.Separator();

            if (ModEngineHandler.ME3ProfileExists(curProject))
            {
                if (ImGui.MenuItem($"Launch Mod##launchME3mod"))
                {
                    ModEngineHandler.LaunchME3Mod(curProject);
                }
            }
            else
            {
                if (CFG.Current.Project_ME3_Profile_Directory != "")
                {
                    if (ImGui.MenuItem($"Create Mod Profile##createME3profile"))
                    {
                        ModEngineHandler.CreateME3Profile(curProject);
                    }

                    UIHelper.Tooltip("Create a ME3 profile file for this mod.");
                }
                else
                {
                    if (ImGui.MenuItem($"Set ME3 Profile Directory"))
                    {
                        var profilePath = "";
                        var result =
                            PlatformUtils.Instance.OpenFolderDialog("Select ME3 Profile Directory", out profilePath);

                        if (result)
                        {
                            CFG.Current.Project_ME3_Profile_Directory = profilePath;
                        }
                    }

                    UIHelper.Tooltip("Set the directory you wish to store ME3 profiles in.");
                }
            }
        }

        ImGui.Separator();

        if (ImGui.MenuItem($"Open Project Folder"))
        {
            StudioCore.Common.FileExplorer.Start(curProject.Descriptor.ProjectPath);
        }

        if (ImGui.MenuItem($"Open Project JSON Folder"))
        {
            var jsonPath = ProjectUtils.GetProjectsFolder();

            StudioCore.Common.FileExplorer.Start(jsonPath);
        }

        if (ImGui.MenuItem($"Clear Backup Files##clearBackupFiles"))
        {
            var root = curProject.Descriptor.ProjectPath;

            var filesToDelete = ProjectUtils.GetBackupFiles(root);

            var fileList = "";

            int i = 0;

            foreach (var entry in filesToDelete)
            {
                fileList = fileList + $"\n{entry}";

                i++;

                if (i > 25)
                {
                    fileList = fileList + $"\n....";
                    break;
                }
            }

            var dialog = PlatformUtils.Instance.MessageBox($"You will delete the following files:\n{fileList}", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (dialog is DialogResult.OK)
            {
                ProjectUtils.DeleteFiles(filesToDelete);
            }
        }

        ImGui.PopID();
    }

    /// <summary>
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (SelectedProject == null)
            return;

        if (SelectedProject.Initialized)
            SelectedProject.EditorResized(window, device);
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (SelectedProject == null)
            return;

        if(SelectedProject.Initialized)
            SelectedProject.Draw(device, cl);
    }

    public void Exit()
    {
        foreach (var projectEntry in Projects)
        {
            SaveProject(projectEntry);
        }
    }

    public void SetupFolders()
    {
        var folder = ProjectUtils.GetBaseFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = ProjectUtils.GetConfigurationFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = ProjectUtils.GetProjectsFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    public void SaveProject(ProjectEntry curProject, bool firstCreated = false)
    {
        if (curProject != null)
        {
            var folder = ProjectUtils.GetProjectsFolder();
            var file = Path.Combine(folder, $"{curProject.Descriptor.ProjectGUID}.json");

            // Update the legacy project.json
            CreateLegacyProjectJSON(curProject, firstCreated);

            var json = JsonSerializer.Serialize(curProject.Descriptor, ProjectJsonSerializerContext.Default.ProjectDescriptor);

            File.WriteAllText(file, json);
        }
    }

    public static List<string> GetStoredProjectJsonList()
    {
        var projectJsonList = new List<string>();
        var folder = ProjectUtils.GetProjectsFolder();

        var files = Directory.EnumerateFiles(folder, "*.json");
        if (files.Any())
        {
            projectJsonList = files.ToList();
        }

        return projectJsonList;
    }

    private async Task LoadExistingProjects()
    {
        // Read all the stored project jsons and create an existing Project dict
        var projectJsonList = GetStoredProjectJsonList();

        for (int i = 0; i < projectJsonList.Count; i++)
        {
            var entry = projectJsonList[i];

            if (File.Exists(entry))
            {
                try
                {
                    var filestring = File.ReadAllText(entry);

                    var curProjectDescriptor =
                        JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.ProjectDescriptor);

                    if (curProjectDescriptor == null)
                    {
                        TaskLogs.AddError($"[Smithbox] Failed to load project: {entry}");
                    }
                    else
                    {
                        var newProject = new ProjectEntry();
                        newProject.Descriptor = curProjectDescriptor;

                        Projects.Add(newProject);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Smithbox] Failed to load project: {entry}", e);
                }
            }
        }

        if (Projects.Count > 0)
        {
            foreach (var projectEntry in Projects)
            {
                if (projectEntry.Descriptor.AutoSelect)
                {
                    if (!IsProjectLoading)
                    {
                        await StartupProject(projectEntry);

                        SelectedProject = projectEntry;
                        SelectedProject.IsSelected = true;
                    }
                }
            }
        }

        return;
    }

    public void CreateProject(ProjectDescriptor newProjectDescriptor)
    {
        var newProject = new ProjectEntry();
        newProject.Descriptor = new ProjectDescriptor(newProjectDescriptor);

        Projects.Add(newProject);

        SaveProject(newProject, true);

        if (!IsProjectLoading)
        {
            _ = StartupProject(newProject);
        }
    }

    public void UpdateProject(ProjectDescriptor newProjectDescriptor)
    {
        SelectedProject.Descriptor = newProjectDescriptor;

        SaveProject(SelectedProject);

        _ = ReloadProject(SelectedProject);
    }

    public async Task<bool> StartupProject(ProjectEntry curProject)
    {
        var oldProject = SelectedProject;

        Smithbox.Instance.SetProgramName(curProject);

        CreateLegacyProjectJSON(curProject);

        if (oldProject != null)
        {
            oldProject.Dispose();
        }

        IsProjectLoading = true;

        SetProgress(new ProjectLoadProgress
        {
            PhaseLabel = "Starting Project",
            StepLabel = "Preparing",
            Percent = 0f
        });

        LoadProgress = default;

        await curProject.Init(ReportProgress);

        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }

        curProject.IsSelected = true;

        SelectedProject = curProject;

        // Used for the DCX heuristic
        BinaryReaderEx.CurrentProjectType = $"{curProject.Descriptor.ProjectType}";

        if (CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment)
        {
            SetAsAutoLoad(curProject);
        }

        SetProgress(new ProjectLoadProgress
        {
            PhaseLabel = "Complete",
            StepLabel = "",
            Percent = 1f
        });

        IsProjectLoading = false;

        return true;
    }

    public void SetAsAutoLoad(ProjectEntry curProject)
    {
        // Automatically set the latest project to auto-load, and disable it for all others
        foreach (var project in Projects)
        {
            project.Descriptor.AutoSelect = false;

            if (curProject == project)
            {
                curProject.Descriptor.AutoSelect = true;
            }

            SaveProject(project);
        }
    }

    public async Task<bool> LoadAuxiliaryProject(ProjectEntry targetProject, ProjectInitType initType, bool reloadProject)
    {
        if (reloadProject)
        {
            await targetProject.Init(ReportProgress, true, initType);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(ReportProgress, true, initType);
            }
        }

        return true;
    }

    public void UnloadProject(ProjectEntry project)
    {
        if (SelectedProject == project)
            SelectedProject = null;

        project.Dispose();
    }

    public async Task ReloadProject(ProjectEntry curProject)
    {
        var oldProject = SelectedProject;

        Smithbox.Instance.SetProgramName(curProject);

        CreateLegacyProjectJSON(curProject);

        // Signal shutdown to existing project if it is loaded
        if (oldProject != null)
        {
            SelectedProject.Dispose();
        }

        IsProjectLoading = true;

        SetProgress(new ProjectLoadProgress
        {
            PhaseLabel = "Starting Project",
            StepLabel = "Preparing",
            Percent = 0f
        });

        LoadProgress = default;

        await curProject.Init(ReportProgress);

        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }

        curProject.IsSelected = true;

        SelectedProject = curProject;

        // Used for the DCX heuristic
        BinaryReaderEx.CurrentProjectType = $"{curProject.Descriptor.ProjectType}";

        SetProgress(new ProjectLoadProgress
        {
            PhaseLabel = "Complete",
            StepLabel = "",
            Percent = 1f
        });

        IsProjectLoading = false;
    }

    /// <summary>
    /// Creates a legacy project.json file for other tools to use.
    /// </summary>
    /// <param name="curProject"></param>
    public void CreateLegacyProjectJSON(ProjectEntry curProject, bool firstTime = false)
    {
        var jsonPath = Path.Combine(curProject.Descriptor.ProjectPath, "project.json");

        // Only create this if it doesn't already exist
        if (firstTime)
        {
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                try
                {
                    LegacyProjectDescriptor project =
                        JsonSerializer.Deserialize(json, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

                    return;
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to parse existing project.json: {e}", LogLevel.Error,
                        LogPriority.High, e);
                }
            }
        }

        if (!File.Exists(jsonPath) && Directory.Exists(curProject.Descriptor.ProjectPath))
        {
            var legacyProjectJson = new LegacyProjectDescriptor(curProject);

            var json = JsonSerializer.Serialize(legacyProjectJson, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

            File.WriteAllText(jsonPath, json);
        }
    }
    public void CreateProjectFromLegacyJson(string path)
    {
        try
        {
            var jsonText = File.ReadAllText(path);

            var legacyDescriptor = JsonSerializer.Deserialize(jsonText, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

            var newProjectDescriptor = new ProjectDescriptor(legacyDescriptor, path);

            CreateProject(newProjectDescriptor);
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to read legacy project.json", e);
        }
    }

    private void SetProgress(ProjectLoadProgress progress)
    {
        lock (_progressLock)
        {
            LoadProgress = progress;
        }
    }

    private bool InitialLayout = false;

    private void DrawProjectLoadingUI()
    {
        if (!IsProjectLoading)
            return;

        ImGui.OpenPopup("Loading Project##ProjectLoad");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            "Loading Project##ProjectLoad",
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ProjectLoadProgress progress;
            lock (_progressLock)
                progress = LoadProgress;

            ImGui.Text(progress.PhaseLabel);
            ImGui.Spacing();

            ImGui.ProgressBar(
                Math.Clamp(progress.Percent, 0f, 1f),
                new System.Numerics.Vector2(400, 0),
                $"{(int)(progress.Percent * 100)}%"
            );

            if (!string.IsNullOrEmpty(progress.StepLabel))
            {
                ImGui.Spacing();
                ImGui.TextDisabled(progress.StepLabel);
            }

            ImGui.EndPopup();
        }
    }

    #region Dispose
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var project in Projects)
        {
            project.Dispose();
        }

        Projects.Clear();
        SelectedProject = null;

        _disposed = true;
    }
    #endregion
}

public struct ProjectLoadProgress
{
    public string PhaseLabel;
    public string StepLabel;
    public float Percent; 
}