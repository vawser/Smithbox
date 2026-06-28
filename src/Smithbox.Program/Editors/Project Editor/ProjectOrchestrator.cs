using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Logger;
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
    public bool FinishedProjectLoad = false;

    public ProjectLoadProgress LoadProgress;
    public Action<ProjectLoadProgress> ReportProgress;
    private readonly object _progressLock = new();

    public ActionManager ActionManager;

    private bool InitExistingProjects = false;

    // Top-Level Editors
    public ProjectScreen ProjectEditor;
    public ProjectMetadataScreen ProjectMetadataEditor;

    public ProjectOrchestrator()
    {
        ReportProgress = SetProgress;

        ProjectEditor = new();
        ProjectMetadataEditor = new();

        ActionManager = new();

        SetupFolders();
    }

    public void Update(float dt, uint mainDockspaceID)
    {
        ProjectEditor.OnGUI(mainDockspaceID);
        ProjectMetadataEditor.OnGUI(mainDockspaceID);

        // Project Dock
        if (SelectedProject != null && SelectedProject.Initialized)
        {
            SelectedProject.Update(dt);
        }

        if (!Smithbox.FirstFrame && !InitExistingProjects)
        {
            InitExistingProjects = true;

            _ = LoadExistingProjects();
        }

        if (FinishedProjectLoad)
        {
            FinishedProjectLoad = false;
            ProjectEditor.RequestFocus = true;
        }

        DrawProjectLoadingUI();
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
                        Smithbox.LogError(this, LOC.Get("PRJ_STP_Load_Project_FAIL", entry));
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
                    Smithbox.LogError(this, LOC.Get("PRJ_STP_Read_Project_FAIL", entry), e);
                }
            }
        }

        if (CFG.Current.Project_Enable_Auto_Load)
        {
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

                            ProjectEditor.SelectedLoadedEntry = projectEntry;
                            ProjectEditor.SelectedAvaliableEntry = projectEntry;

                            ProjectEditor.ConfigureMenu.SetupForEdit(projectEntry);
                        }
                    }
                }
            }
        }

        return;
    }

    public void CreateProject(ProjectDescriptor newProjectDescriptor)
    {
        var newProject = new ProjectEntry(); 
        newProject.Descriptor = ProjectDescriptor.CreateNew(newProjectDescriptor);

        Projects.Add(newProject);

        SaveProject(newProject, true);

        if (!IsProjectLoading)
        {
            _ = StartupProject(newProject);
        }
    }

    public void UpdateProject(ProjectEntry project, ProjectDescriptor newProjectDescriptor)
    {
        project.Descriptor = newProjectDescriptor;

        SaveProject(project);

        _ = ReloadProject(project);
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
            PhaseLabel = LOC.Get("PRJ_STP_Project_Phase_Start"),
            StepLabel = LOC.Get("PRJ_STP_Project_Step_Preparing"),
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
            PhaseLabel = LOC.Get("PRJ_STP_Project_Phase_Complete"),
            StepLabel = "",
            Percent = 1f
        });

        IsProjectLoading = false;
        FinishedProjectLoad = true;

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

        Smithbox.Instance.ResetProgramName();
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
            PhaseLabel = LOC.Get("PRJ_STP_Project_Phase_Start"),
            StepLabel = LOC.Get("PRJ_STP_Project_Step_Preparing"),
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
            PhaseLabel = LOC.Get("PRJ_STP_Project_Phase_Complete"),
            StepLabel = "",
            Percent = 1f
        });

        IsProjectLoading = false;
        FinishedProjectLoad = true;
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
            if (!File.Exists(jsonPath) && Directory.Exists(curProject.Descriptor.ProjectPath))
            {
                var legacyProjectJson = new LegacyProjectDescriptor(curProject);

                var json = JsonSerializer.Serialize(legacyProjectJson, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

                File.WriteAllText(jsonPath, json);
            }
        }
    }
    public void CreateProjectFromLegacyJson(string path)
    {
        // Unfortunately the PinnedRows/PinnedFields have been saved as both List and Dictionary,
        // so we need to fallback to the incorrect form if the correct form fails deserialization.
        try
        {
            var jsonText = File.ReadAllText(path);

            var legacyDescriptor = JsonSerializer.Deserialize(jsonText, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

            var newProjectDescriptor = new ProjectDescriptor(legacyDescriptor, path);

            CreateProject(newProjectDescriptor);
        }
        catch (Exception)
        {
            try
            {
                var jsonText = File.ReadAllText(path);

                var legacyDescriptor = JsonSerializer.Deserialize(jsonText, ProjectJsonSerializerContext.Default.LegacyProjectDescriptorAlt);

                var newProjectDescriptor = new ProjectDescriptor(legacyDescriptor, path);

                CreateProject(newProjectDescriptor);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PRJ_STP_Read_Legacy_Project_FAIL", path), e);
            }
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

        ImGui.OpenPopup($"Loading Project##ProjectLoad");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            $"Loading Project##ProjectLoad",
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