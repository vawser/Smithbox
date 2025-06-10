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

    public ProjectDisplayOrder ProjectDisplayOrder;

    public ProjectManager(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Update(float dt)
    {
        var flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;

        // Project List
        if (CFG.Current.Interface_Editor_ProjectList)
        {
            ImGui.Begin("Projects##projectList", flags);

            Menubar();
            DisplayProjectList();

            ImGui.End();
        }

        // Project Dock
        if (SelectedProject != null)
        {
            SelectedProject.Update(dt);
        }
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu($"File"))
            {
                if (ImGui.MenuItem("New Project"))
                {
                    ProjectCreation.Show();
                }
                UIHelper.Tooltip($"Add a new project to the project list.");

                if (SelectedProject != null)
                {
                    if (ImGui.MenuItem("Open Project Folder"))
                    {
                        Process.Start("explorer.exe", SelectedProject.ProjectPath);
                    }
                    UIHelper.Tooltip("Open the project folder for this project.");
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    /// <summary>
    /// Display the project list -- contains all stored project entries.
    /// </summary>
    private unsafe void DisplayProjectList()
    {
        UIHelper.SimpleHeader("projectListHeader", "Available Projects", "The projects currently available to select from.", UI.Current.ImGui_AliasName_Text);

        var orderedProjects = Projects
        .OrderBy(p =>
        {
            foreach (var kvp in ProjectDisplayOrder.DisplayOrder)
            {
                if (kvp.Value == p.ProjectGUID)
                {
                    return kvp.Key;
                }
            }
            return int.MaxValue; // Put untracked projects at the end
        })
        .ToList();

        int dragSourceIndex = -1;
        int dragTargetIndex = -1;

        // Help new users since the File -> New Project action isn't obvious
        if(orderedProjects.Count == 0)
        {
            var width = ImGui.GetWindowWidth();
            if(ImGui.Button("Create New Project", new Vector2(width, 32)))
            {
                ProjectCreation.Show();
            }
        }

        for (int i = 0; i < orderedProjects.Count; i++)
        {
            var project = orderedProjects[i];
            var imGuiID = project.ProjectGUID;

            var projectName = $"{project.ProjectName}";
            if(CFG.Current.DisplayProjectPrefix)
            {
                projectName = $"[{project.ProjectType}] {projectName}";
            }

            // Highlight selectable
            if (ImGui.Selectable($"{projectName}##projectEntry{imGuiID}", SelectedProject == project))
            {
                if (!IsProjectLoading)
                {
                    StartupProject(project);
                }
            }

            // Begin drag
            if (ImGui.BeginDragDropSource())
            {
                int payloadIndex = i;
                ImGui.SetDragDropPayload("PROJECT_DRAG", &payloadIndex, sizeof(int));
                ImGui.Text(project.ProjectName);
                ImGui.EndDragDropSource();
            }

            // Accept drop
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("PROJECT_DRAG");
                if (payload.Handle != null)
                {
                    int* droppedIndex = (int*)payload.Data;
                    dragSourceIndex = *droppedIndex;
                    dragTargetIndex = i;
                }
                ImGui.EndDragDropTarget();
            }

            if (SelectedProject != null && project == SelectedProject)
            {
                if (ImGui.BeginPopupContextItem($"ProjectListContextMenu{imGuiID}"))
                {
                    // ME2
                    if (ModEngineHandler.IsME2Project(project))
                    {
                        if (CFG.Current.ModEngine2Install != "")
                        {
                            if (ImGui.MenuItem($"Launch Mod##launchME2Mod"))
                            {
                                ModEngineHandler.LaunchME2Mod(project);
                            }
                            UIHelper.Tooltip("Launch this project with ModEngine2.");
                        }
                        else
                        {
                            if (ImGui.MenuItem($"Set ME2 Executable Location"))
                            {
                                var modEnginePath = "";
                                var result = PlatformUtils.Instance.OpenFileDialog("Select ME2 Executable", ["exe"], out modEnginePath);

                                if (result)
                                {
                                    if (modEnginePath.Contains("modengine2_launcher.exe"))
                                    {
                                        CFG.Current.ModEngine2Install = modEnginePath;
                                    }
                                    else
                                    {
                                        PlatformUtils.Instance.MessageBox("Error", "The file you selected was not modengine2_launcher.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                            UIHelper.Tooltip("Set the ME2 executable location so you can launch this mod via ModEngine2.");
                        }
                    }

                    // ME3
                    if (ModEngineHandler.IsME3Project(project))
                    {
                        if (ModEngineHandler.ME3ProfileExists(project))
                        {
                            if (ImGui.MenuItem($"Launch Mod##launchME3mod"))
                            {
                                ModEngineHandler.LaunchME3Mod(project);
                            }
                        }
                        else
                        {
                            if (CFG.Current.ModEngine3ProfileDirectory != "")
                            {
                                if (ImGui.MenuItem($"Create Mod Profile##createME3profile"))
                                {
                                    ModEngineHandler.CreateME3Profile(project);
                                }
                                UIHelper.Tooltip("Create a ME3 profile file for this mod.");
                            }
                            else
                            {
                                if (ImGui.MenuItem($"Set ME3 Profile Directory"))
                                {
                                    var profilePath = "";
                                    var result = PlatformUtils.Instance.OpenFolderDialog("Select ME3 Profile Directory", out profilePath);

                                    if (result)
                                    {
                                        CFG.Current.ModEngine3ProfileDirectory = profilePath;
                                    }
                                }
                                UIHelper.Tooltip("Set the directory you wish to store ME3 profiles in.");
                            }
                        }
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem($"Open Project Settings##projectSettings_{imGuiID}"))
                    {
                        ProjectSettings.Show(BaseEditor, project);
                    }

                    if (ImGui.MenuItem($"Open Project Aliases##projectAliases_{imGuiID}"))
                    {
                        ProjectAliasEditor.Show(BaseEditor, project);
                    }

                    if (ImGui.MenuItem($"Open Project Enums##projectEnums_{imGuiID}"))
                    {
                        ProjectEnumEditor.Show(BaseEditor, project);
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem($"New Project##newProject_{imGuiID}"))
                    {
                        ProjectCreation.Show();
                    }

                    if (ImGui.MenuItem($"Unload Project##unloadProject_{imGuiID}"))
                    {
                        project.Unload();
                    }

                    if (ImGui.MenuItem($"Delete Project##deleteProject_{imGuiID}"))
                    {
                        var dialog = PlatformUtils.Instance.MessageBox("Are you sure you want to delete this project?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (dialog == DialogResult.Yes)
                        {
                            ProjectUtils.DeleteProject(BaseEditor, project);
                        }
                    }

                    ImGui.EndPopup();
                }
            }

            if (project.Initialized)
            {
                UIHelper.DisplayColoredAlias($"{Icons.Bolt}", UI.Current.ImGui_Benefit_Text_Color);
            }
        }

        if (dragSourceIndex >= 0 && dragTargetIndex >= 0 && dragSourceIndex != dragTargetIndex)
        {
            var movedProject = orderedProjects[dragSourceIndex];
            orderedProjects.RemoveAt(dragSourceIndex);
            orderedProjects.Insert(dragTargetIndex, movedProject);

            // Rebuild order dictionary
            ProjectDisplayOrder.DisplayOrder.Clear();
            for (int i = 0; i < orderedProjects.Count; i++)
            {
                ProjectDisplayOrder.DisplayOrder[i] = orderedProjects[i].ProjectGUID;
            }

            SaveProjectDisplayOrder();
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
        LoadProjectDisplayOrder();
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

    public void SaveProject(ProjectEntry curProject)
    {
        if (curProject != null)
        {
            var folder = ProjectUtils.GetProjectsFolder();
            var file = Path.Combine(folder, $"{curProject.ProjectGUID}.json");

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

        var buildProjectOrder = false;

        // If it is not set, create initial order
        if (ProjectDisplayOrder.DisplayOrder == null)
        {
            ProjectDisplayOrder.DisplayOrder = new();
            buildProjectOrder = true;
        }

        for (int i = 0; i < projectJsonList.Count; i++)
        {
            var entry = projectJsonList[i];

            if (File.Exists(entry))
            {
                try
                {
                    var filestring = File.ReadAllText(entry);
                    var options = new JsonSerializerOptions();

                    var curProject = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectEntry);

                    if (curProject == null)
                    {
                        TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        curProject.BaseEditor = BaseEditor;

                        Projects.Add(curProject);

                        if (buildProjectOrder)
                        {
                            ProjectDisplayOrder.DisplayOrder.Add(i, curProject.ProjectGUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (buildProjectOrder)
        {
            SaveProjectDisplayOrder();
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

    private void LoadProjectDisplayOrder()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display Order.json");

        if (!File.Exists(file))
        {
            ProjectDisplayOrder = new ProjectDisplayOrder();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    var options = new JsonSerializerOptions();
                    ProjectDisplayOrder = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectDisplayOrder);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog("[Smithbox] Failed to deserialize Project Display Order", LogLevel.Error, Tasks.LogPriority.High, e);
                    ProjectDisplayOrder = new ProjectDisplayOrder();
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read Project Display Order", LogLevel.Error, Tasks.LogPriority.High, e);
                ProjectDisplayOrder = new ProjectDisplayOrder();
            }
        }
    }

    public void SaveProjectDisplayOrder()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display Order.json");

        var json = JsonSerializer.Serialize(ProjectDisplayOrder, SmithboxSerializerContext.Default.ProjectDisplayOrder);

        File.WriteAllText(file, json);
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
        newProject.EnableTimeActEditor = ProjectCreation.EnableTimeActEditor;
        newProject.EnableGparamEditor = ProjectCreation.EnableGparamEditor;
        newProject.EnableMaterialEditor = ProjectCreation.EnableMaterialEditor;
        newProject.EnableEmevdEditor = ProjectCreation.EnableEmevdEditor;
        newProject.EnableEsdEditor = ProjectCreation.EnableEsdEditor;
        newProject.EnableTextureViewer = ProjectCreation.EnableTextureViewer;
        newProject.EnableFileBrowser = ProjectCreation.EnableFileBrowser;

        newProject.EnableExternalMaterialData = ProjectCreation.EnableExternalMaterialData;

        ProjectCreation.Reset();

        Projects.Add(newProject);

        SaveProject(newProject);

        if(!IsProjectLoading)
            StartupProject(newProject);
    }

    public bool IsProjectLoading = false;

    public async void StartupProject(ProjectEntry curProject)
    {
        ProjectEntry oldProject = SelectedProject;

        BaseEditor.SetProgramName(curProject);

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
}

