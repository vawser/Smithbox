using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Core;

/// <summary>
/// Manager for the project data
/// </summary>
public class ProjectManager
{
    private Smithbox BaseEditor;

    public List<ProjectEntry> Projects = new();
    public ProjectEntry SelectedProject;

    public bool IsProjectLoading = false;

    public string ProjectListFilter = "";

    public ProjectManager(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Update(float dt)
    {
        // Project Dock
        if (SelectedProject != null)
        {
            SelectedProject.Update(dt);
        }
    }

    public void Menubar()
    {
        if (ImGui.BeginMenu($"Project"))
        {
            // General Actions
            if (ImGui.MenuItem("New Project"))
            {
                ProjectCreation.Show();
            }

            UIHelper.Tooltip($"Add a new project to the project list.");

            UIHelper.SimpleHeader("selectedProjectHeader", "Selected Project",
                "The project that is currently selected.", UI.Current.ImGui_AliasName_Text);

            DisplaySelectedProjectEntry();

            UIHelper.SimpleHeader("possibleProjectsHeader", "Available Projects", "The projects that are avaliable.",
                UI.Current.ImGui_AliasName_Text);

            ImGui.InputText("##projectListFilter", ref ProjectListFilter, 255);
            UIHelper.Tooltip("Filter the project list by this term.");

            // Project List
            DisplayProjectListGroup(ProjectType.DES);
            DisplayProjectListGroup(ProjectType.DS1);
            DisplayProjectListGroup(ProjectType.DS1R);
            DisplayProjectListGroup(ProjectType.DS2);
            DisplayProjectListGroup(ProjectType.DS2S);
            DisplayProjectListGroup(ProjectType.DS3);
            DisplayProjectListGroup(ProjectType.BB);
            DisplayProjectListGroup(ProjectType.SDT);
            DisplayProjectListGroup(ProjectType.ER);
            DisplayProjectListGroup(ProjectType.AC6);
            DisplayProjectListGroup(ProjectType.NR);

            ImGui.EndMenu();
        }
    }

    private void DisplaySelectedProjectEntry()
    {
        if (Projects.Count > 0)
        {
            foreach (var project in Projects)
            {
                if (project != SelectedProject)
                    continue;

                var imGuiID = project.ProjectGUID;
                var projectName = $"{project.ProjectName}";

                if (CFG.Current.DisplayProjectPrefix)
                {
                    projectName = $"[{project.ProjectType}] {projectName}";
                }

                if (ImGui.BeginMenu($"{projectName}##projectEntry_{imGuiID}"))
                {
                    DisplayProjectActions(project);

                    ImGui.EndMenu();
                }
            }
        }
    }

    private void DisplayProjectListGroup(ProjectType projectType)
    {
        var projectList = Projects.Where(e => e.ProjectType == projectType).ToList();
        if (!projectList.Any()) return;
        // When filtering the list, don't use the collapsible sections since it makes it harder to see the filtered results
        if (CFG.Current.DisplayCollapsibleProjectCategories && ProjectListFilter == "")
        {
            if(ImGui.CollapsingHeader($"{projectType}##collapsibleSection_{projectType}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                DisplayProjectList(projectList);
            }
        }
        else
        {
            DisplayProjectList(projectList);
        }
    }

    private void DisplayProjectList(List<ProjectEntry> projectList)
    {
        if (projectList.Count > 0)
        {
            foreach (var project in projectList)
            {
                if (project == SelectedProject)
                    continue;

                if (!FilterProjectEntry(project))
                    continue;

                var imGuiID = project.ProjectGUID;
                var projectName = $"{project.ProjectName}";

                // Only display type prefix if user is not using the categories
                if (CFG.Current.DisplayProjectPrefix && !CFG.Current.DisplayCollapsibleProjectCategories)
                {
                    projectName = $"[{project.ProjectType}] {projectName}";
                }

                if (ImGui.BeginMenu($"{projectName}##projectEntry_{imGuiID}"))
                {
                    DisplayProjectActions(project);

                    ImGui.EndMenu();
                }
            }
        }
    }

    private bool FilterProjectEntry(ProjectEntry curProject)
    {
        var projName = curProject.ProjectName;

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
        if (!curProject.Initialized)
        {
            if (ImGui.MenuItem("Load"))
            {
                SelectedProject = curProject;
                if (!IsProjectLoading)
                {
                    StartupProject(curProject);
                }
            }
        }
        else
        {
            if (ImGui.MenuItem("Select"))
            {
                SelectedProject = curProject;
                BaseEditor.SetProgramName(curProject);
            }

            if (ImGui.MenuItem($"Unload##unloadProject"))
            {
                curProject.Unload();
            }

            if (ImGui.MenuItem($"Reload##reloadProject"))
            {
                SelectedProject = curProject;
                if (!IsProjectLoading)
                {
                    ReloadProject(curProject);
                }
            }
        }

        ImGui.Separator();

        if (ImGui.MenuItem($"Open Project Settings##projectSettings"))
        {
            ProjectSettings.Show(BaseEditor, curProject);
        }

        if (curProject.Initialized)
        {
            if (ImGui.MenuItem($"Open Project Aliases##projectAliases"))
            {
                ProjectAliasEditor.Show(BaseEditor, curProject);
            }

            if (ImGui.MenuItem($"Open Project Enums##projectEnums"))
            {
                ProjectEnumEditor.Show(BaseEditor, curProject);
            }
        }

        // ME2
        if (ModEngineHandler.IsME2Project(curProject))
        {
            ImGui.Separator();

            if (CFG.Current.ModEngine2Install != "")
            {
                if (ImGui.MenuItem($"Launch Mod##launchME2Mod"))
                {
                    ModEngineHandler.LaunchME2Mod(curProject);
                }

                UIHelper.Tooltip("Launch this project with ModEngine2.");
            }
            else
            {
                if (ImGui.MenuItem($"Set ME2 Executable Location"))
                {
                    var modEnginePath = "";
                    var result =
                        PlatformUtils.Instance.OpenFileDialog("Select ME2 Executable", ["exe"], out modEnginePath);

                    if (result)
                    {
                        if (modEnginePath.Contains("modengine2_launcher.exe"))
                        {
                            CFG.Current.ModEngine2Install = modEnginePath;
                        }
                        else
                        {
                            PlatformUtils.Instance.MessageBox("Error",
                                "The file you selected was not modengine2_launcher.exe", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }

                UIHelper.Tooltip("Set the ME2 executable location so you can launch this mod via ModEngine2.");
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
                if (CFG.Current.ModEngine3ProfileDirectory != "")
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
                            CFG.Current.ModEngine3ProfileDirectory = profilePath;
                        }
                    }

                    UIHelper.Tooltip("Set the directory you wish to store ME3 profiles in.");
                }
            }
        }
    }

    /// <summary>
    /// Pass editor resized along to selected project (if present)
    /// </summary>
    /// <param name="window"></param>
    /// <param name="device"></param>
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (SelectedProject == null)
            return;

        SelectedProject.EditorResized(window, device);
    }

    /// <summary>
    /// Pass editor draw along to selected project (if present)
    /// </summary>
    /// <param name="device"></param>
    /// <param name="cl"></param>
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (SelectedProject == null)
            return;

        SelectedProject.Draw(device, cl);
    }

    public void Exit()
    {
        foreach (var projectEntry in Projects)
        {
            SaveProject(projectEntry);
        }
    }

    public void Setup()
    {
        SetupFolders();
        LoadExistingProjects();
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
            var file = Path.Combine(folder, $"{curProject.ProjectGUID}.json");

            // Update the legacy project.json
            CreateLegacyProjectJSON(curProject, firstCreated);

            var json = JsonSerializer.Serialize(curProject, SmithboxSerializerContext.Default.ProjectEntry);

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

    private void LoadExistingProjects()
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
                    var options = new JsonSerializerOptions();

                    var curProject =
                        JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectEntry);

                    if (curProject == null)
                    {
                        TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        curProject.BaseEditor = BaseEditor;

                        Projects.Add(curProject);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Error,
                        Tasks.LogPriority.High, e);
                }
            }
        }

        if (Projects.Count > 0)
        {
            foreach (var projectEntry in Projects)
            {
                if (projectEntry.AutoSelect)
                {
                    if (!IsProjectLoading)
                    {
                        StartupProject(projectEntry);
                        SelectedProject = projectEntry;
                        SelectedProject.IsSelected = true;
                    }
                }
            }
        }
    }

    public void CreateProject()
    {
        var guid = Guid.NewGuid();
        var projectName = ProjectCreation.ProjectName;
        var projectPath = ProjectCreation.ProjectPath;
        var dataPath = ProjectCreation.DataPath;
        var projectType = ProjectCreation.ProjectType;

        var newProject = new ProjectEntry(BaseEditor, guid, projectName, projectPath, dataPath, projectType);

        newProject.AutoSelect = ProjectCreation.AutoSelect;
        newProject.ImportedParamRowNames = !ProjectCreation.RowNameImport;

        newProject.EnableMapEditor = ProjectCreation.EnableMapEditor;
        newProject.EnableModelEditor = ProjectCreation.EnableModelEditor;
        newProject.EnableTextEditor = ProjectCreation.EnableTextEditor;
        newProject.EnableParamEditor = ProjectCreation.EnableParamEditor;
        newProject.EnableGparamEditor = ProjectCreation.EnableGparamEditor;
        newProject.EnableMaterialEditor = ProjectCreation.EnableMaterialEditor;
        newProject.EnableTextureViewer = ProjectCreation.EnableTextureViewer;
        newProject.EnableFileBrowser = ProjectCreation.EnableFileBrowser;

        newProject.EnableExternalMaterialData = ProjectCreation.EnableExternalMaterialData;

        ProjectCreation.Reset();

        Projects.Add(newProject);

        SaveProject(newProject, true);

        if (!IsProjectLoading)
            StartupProject(newProject);
    }

    public async void StartupProject(ProjectEntry curProject)
    {
        ProjectEntry oldProject = SelectedProject;

        BaseEditor.SetProgramName(curProject);
        CreateLegacyProjectJSON(curProject);

        // Signal shutdown to existing project if it is loaded
        if (oldProject != null)
            SelectedProject.Suspend();

        IsProjectLoading = true;

        // Only setup editors 
        if (!curProject.Initialized)
        {
            Task<bool> projectSetupTask = curProject.Init();
            bool projectSetupTaskResult = await projectSetupTask;
        }

        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }

        curProject.IsSelected = true;

        SelectedProject = curProject;

        // Used for the DCX heuristic
        BinaryReaderEx.CurrentProjectType = $"{curProject.ProjectType}";

        IsProjectLoading = false;

        if (oldProject != null)
            oldProject.Reset();
    }

    public async void ReloadProject(ProjectEntry curProject)
    {
        ProjectEntry oldProject = SelectedProject;

        BaseEditor.SetProgramName(curProject);
        CreateLegacyProjectJSON(curProject);

        // Signal shutdown to existing project if it is loaded
        if (oldProject != null)
            SelectedProject.Suspend();

        IsProjectLoading = true;

        Task<bool> projectSetupTask = curProject.Init();
        bool projectSetupTaskResult = await projectSetupTask;

        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }

        curProject.IsSelected = true;

        SelectedProject = curProject;

        // Used for the DCX heuristic
        BinaryReaderEx.CurrentProjectType = $"{curProject.ProjectType}";

        IsProjectLoading = false;

        if (oldProject != null)
            oldProject.Reset();
    }

    /// <summary>
    /// Creates a legacy project.json file for other tools to use.
    /// </summary>
    /// <param name="curProject"></param>
    public void CreateLegacyProjectJSON(ProjectEntry curProject, bool firstTime = false)
    {
        var jsonPath = $"{curProject.ProjectPath}/project.json";

        // Only create this if it doesn't already exist
        if (firstTime)
        {
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                try
                {
                    LegacyProjectJSON project =
                        JsonSerializer.Deserialize(json, SmithboxSerializerContext.Default.LegacyProjectJSON);
                    curProject.PinnedRows = project.PinnedRows;
                    curProject.PinnedFields = project.PinnedFields;
                    curProject.PinnedParams = project.PinnedParams;
                    return;
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to parse existing project.json: {e}", LogLevel.Error,
                        Tasks.LogPriority.High, e);
                }
            }
        }

        if (!File.Exists(jsonPath) && Directory.Exists(curProject.ProjectPath))
        {
            var legacyProjectJson = new LegacyProjectJSON(curProject);

            var json = JsonSerializer.Serialize(legacyProjectJson,
                SmithboxSerializerContext.Default.LegacyProjectJSON);

            File.WriteAllText(jsonPath, json);
        }
    }
}