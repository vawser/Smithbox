using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
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


                    if (CFG.Current.ModEngineInstall != "")
                    {
                        if (SelectedProject.ProjectType is ProjectType.DS3 or ProjectType.ER or ProjectType.AC6)
                        {
                            if (ImGui.MenuItem($"Launch Mod##launchMod"))
                            {
                                ModEngineUtils.LaunchMod(SelectedProject);
                            }
                            UIHelper.Tooltip("Launch this project with ModEngine2.");
                        }
                    }
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

        for (int i = 0; i < orderedProjects.Count; i++)
        {
            var project = orderedProjects[i];
            var imGuiID = project.ProjectGUID;

            // Highlight selectable
            if (ImGui.Selectable($"{project.ProjectName}##{imGuiID}", SelectedProject == project))
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

            if (ImGui.BeginPopupContextItem($"ProjectListContextMenu{imGuiID}"))
            {
                if (ImGui.MenuItem($"New Project##newProject_{imGuiID}"))
                {
                    ProjectCreation.Show();
                }

                if (ImGui.MenuItem($"Open Project Settings##projectSettings_{imGuiID}"))
                {
                    ProjectSettings.Show(BaseEditor, SelectedProject);
                }

                if (ImGui.MenuItem($"Open Project Aliases##projectAliases_{imGuiID}"))
                {
                    ProjectAliasEditor.Show(BaseEditor, SelectedProject);
                }


                if (CFG.Current.ModEngineInstall != "")
                {
                    if (SelectedProject.ProjectType is ProjectType.DS3 or ProjectType.ER or ProjectType.AC6)
                    {
                        if (ImGui.MenuItem($"Launch Mod##launchMod"))
                        {
                            ModEngineUtils.LaunchMod(SelectedProject);
                        }
                        UIHelper.Tooltip("Launch this project with ModEngine2.");
                    }
                }

                ImGui.EndPopup();
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

