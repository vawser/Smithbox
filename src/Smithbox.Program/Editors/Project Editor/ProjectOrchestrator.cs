using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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

    /// <summary>
    /// The project being started or reloaded, if any. Read by the crash recovery to stop a project that crashed
    /// while loading from being loaded automatically again.
    /// </summary>
    public volatile ProjectEntry LoadingProject;

    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
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
        // First, so the frame draws the state the finished loads left
        RunMainThreadActions();

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
        // Started without being awaited, so a failure has to be reported here or it is lost
        try
        {
            await LoadExistingProjectsCore();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, "Failed to load the existing projects.", LogPriority.High, e);
        }
    }

    private async Task LoadExistingProjectsCore()
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
                        Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Load_Project_FAIL", entry));
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
                    Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Read_Project_FAIL", entry), e);
                }
            }
        }

        if (CFG.Current.Project_Enable_Auto_Load)
        {
            // Only one project can be open, so the first marked for auto-load is used. A snapshot is iterated, as
            // the user can add projects while this one loads.
            var projectEntry = Projects.ToList().FirstOrDefault(e => e.Descriptor.AutoSelect);

            if (projectEntry != null && !IsProjectLoading)
            {
                // StartupProject has already selected it; this points the project screen at it, on the main thread
                await StartupProject(projectEntry, () =>
                {
                    ProjectEditor.SelectedLoadedEntry = projectEntry;
                    ProjectEditor.SelectedAvaliableEntry = projectEntry;

                    ProjectEditor.ConfigureMenu.SetupForEdit(projectEntry);
                });
            }
        }

        return;
    }

    public ProjectEntry CreateProject(ProjectDescriptor newProjectDescriptor)
    {
        var newProject = new ProjectEntry();
        newProject.Descriptor = ProjectDescriptor.CreateNew(newProjectDescriptor);
        newProject.Descriptor.ProjectName = newProject.Descriptor.ProjectName.Trim();

        Projects.Add(newProject);

        SaveProject(newProject, true);

        if (!IsProjectLoading)
        {
            _ = StartupProject(newProject);
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PROJECT_Setup_Create_Not_Loaded_Loading"));
        }

        return newProject;
    }

    /// <summary>
    /// Applies the edited settings to the project and saves it. The project is reloaded only if it is the open one.
    /// </summary>
    /// <remarks>
    /// Only the settings the configuration form edits are copied, so state changed elsewhere since the form was
    /// filled (pins, auto-load) is kept, and the form keeps its own descriptor rather than sharing the live one.
    /// </remarks>
    public void UpdateProject(ProjectEntry project, ProjectDescriptor editedDescriptor)
    {
        project.Descriptor.ApplyConfiguration(editedDescriptor);

        SaveProject(project);

        if (project != SelectedProject)
            return;

        if (IsProjectLoading)
        {
            Smithbox.Log(this, LOC.Get("PROJECT_Setup_Update_Deferred_Loading"));
            return;
        }

        _ = ReloadProject(project);
    }

    /// <summary>
    /// Removes a project from Smithbox. The files in its project directory are not touched.
    /// </summary>
    public void RemoveProject(ProjectEntry project)
    {
        if (project == null)
            return;

        var file = Path.Combine(ProjectUtils.GetProjectsFolder(), $"{project.Descriptor.ProjectGUID}.json");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        if (project == SelectedProject)
        {
            UnloadProject(project);
        }
        else if (project.Initialized || project.IsInitializing)
        {
            project.Dispose();
        }

        Projects.Remove(project);

        ProjectEditor.OnProjectRemoved(project);
    }

    /// <summary>
    /// Loads a project and selects it.
    /// </summary>
    /// <param name="onLoaded">Run on the main thread once the project is selected.</param>
    /// <returns>Whether the project loaded.</returns>
    public Task<bool> StartupProject(ProjectEntry curProject, Action onLoaded = null)
    {
        return LoadProject(curProject, CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment, onLoaded);
    }

    public Task ReloadProject(ProjectEntry curProject)
    {
        return LoadProject(curProject, false, null);
    }

    /// <summary>
    /// Initializes a project, then selects it on the main thread.
    /// </summary>
    /// <remarks>
    /// The banks load in parallel and their continuations resume on thread pool threads, while the UI reads the
    /// selection every frame. So only the loading runs off the main thread: selecting the project is queued and
    /// applied at the start of the next <see cref="Update"/>.
    /// </remarks>
    private async Task<bool> LoadProject(ProjectEntry curProject, bool assignAutoLoad, Action onLoaded)
    {
        // Checked before the open project is disposed, so a project that can't load leaves the open one in place
        if (!curProject.ValidateForInit())
            return false;

        var oldProject = SelectedProject;

        Smithbox.Instance.SetProgramName(curProject);

        CreateLegacyProjectJSON(curProject);

        if (oldProject != null)
        {
            // Deselected first, so nothing draws the disposed project while the new one loads
            SelectedProject = null;
            oldProject.IsSelected = false;

            oldProject.Dispose();
            ResourceManager.ClearAllPools();
        }

        // A full load replaces any auxiliary (comparison-only) initialization
        curProject.IsAuxiliary = false;

        IsProjectLoading = true;
        LoadingProject = curProject;

        try
        {
            LoadProgress = default;

            SetProgress(new ProjectLoadProgress
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Project_Phase_Start"),
                StepLabel = LOC.Get("PROJECT_Setup_Project_Step_Preparing"),
                Percent = 0f
            });

            if (!await curProject.Init(ReportProgress))
            {
                RunOnMainThread(() =>
                {
                    Smithbox.Instance.ResetProgramName();
                    EndProjectLoad();
                });

                return false;
            }
        }
        catch (Exception e)
        {
            // Callers don't await this, so without the catch the failure would vanish and leave the load
            // popup open, with every further load refused
            LogProjectLoadFailure(curProject, e);

            RunOnMainThread(EndProjectLoad);

            return false;
        }

        // Continuations run asynchronously so awaiting callers don't run inside the queue drain
        var selected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        RunOnMainThread(() =>
        {
            try
            {
                SelectLoadedProject(curProject, assignAutoLoad);

                onLoaded?.Invoke();

                selected.SetResult(true);
            }
            catch (Exception e)
            {
                LogProjectLoadFailure(curProject, e);

                selected.SetResult(false);
            }
            finally
            {
                EndProjectLoad();
            }
        });

        return await selected.Task;
    }

    private void SelectLoadedProject(ProjectEntry curProject, bool assignAutoLoad)
    {
        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }

        curProject.IsSelected = true;

        SelectedProject = curProject;

        // Used for the DCX heuristic
        BinaryReaderEx.CurrentProjectType = $"{curProject.Descriptor.ProjectType}";

        if (assignAutoLoad)
        {
            SetAsAutoLoad(curProject);
        }

        SetProgress(new ProjectLoadProgress
        {
            PhaseLabel = LOC.Get("PROJECT_Setup_Project_Phase_Complete"),
            StepLabel = "",
            Percent = 1f
        });

        FinishedProjectLoad = true;
    }

    private void EndProjectLoad()
    {
        IsProjectLoading = false;
        LoadingProject = null;
    }

    /// <summary>
    /// Queues an action to run on the main thread, at the start of the next <see cref="Update"/>.
    /// </summary>
    public void RunOnMainThread(Action action)
    {
        _mainThreadActions.Enqueue(action);
    }

    private void RunMainThreadActions()
    {
        // Only what was queued before this frame, so an action that queues another can't stall the frame
        var count = _mainThreadActions.Count;

        for (int i = 0; i < count && _mainThreadActions.TryDequeue(out var action); i++)
        {
            action();
        }
    }

    private void LogProjectLoadFailure(ProjectEntry curProject, Exception e)
    {
        Smithbox.LogError(this,
            $"Failed to load project \"{curProject.Descriptor?.ProjectName}\".",
            LogPriority.High, e);
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
        // The open project is already fully initialized; re-initializing it for one editor would replace its editors
        if (targetProject == SelectedProject && targetProject.Initialized)
            return true;

        if (!targetProject.Initialized)
            targetProject.IsAuxiliary = true;

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
        var isSelected = SelectedProject == project;

        if (isSelected)
            SelectedProject = null;

        project.IsSelected = false;
        project.Dispose();

        if (isSelected)
        {
            ResourceManager.ClearAllPools();

            Smithbox.Instance.ResetProgramName();
        }
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
    public ProjectEntry CreateProjectFromLegacyJson(string path)
    {
        ProjectDescriptor newProjectDescriptor = null;

        // Unfortunately the PinnedRows/PinnedFields have been saved as both List and Dictionary,
        // so we need to fallback to the incorrect form if the correct form fails deserialization.
        // Only the reading is retried: creating the project twice would register it twice.
        try
        {
            var jsonText = File.ReadAllText(path);

            try
            {
                var legacyDescriptor = JsonSerializer.Deserialize(jsonText, ProjectJsonSerializerContext.Default.LegacyProjectDescriptor);

                newProjectDescriptor = new ProjectDescriptor(legacyDescriptor, path);
            }
            catch (Exception)
            {
                var legacyDescriptor = JsonSerializer.Deserialize(jsonText, ProjectJsonSerializerContext.Default.LegacyProjectDescriptorAlt);

                newProjectDescriptor = new ProjectDescriptor(legacyDescriptor, path);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Read_Legacy_Project_FAIL", path), e);

            return null;
        }

        try
        {
            return CreateProject(newProjectDescriptor);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Read_Legacy_Project_FAIL", path), e);

            return null;
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
            GUI.SetupPopupWindow();
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