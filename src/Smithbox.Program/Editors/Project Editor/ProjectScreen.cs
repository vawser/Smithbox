using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The tabs shown for the project selected in the Project Editor.
/// </summary>
public enum ProjectDetailTab
{
    Overview,
    Aliases,
    Enums,
    Tools
}

/// <summary>
/// The sections of the Overview tab that can be opened from elsewhere.
/// </summary>
public enum ProjectOverviewSection
{
    None,
    Settings,
    Editors
}

/// <summary>
/// The Project Editor: a list of the projects on the left, and on the right whatever fits the moment. That is a
/// Get Started panel with no projects, the Create Project form, a prompt to pick a project, or the selected
/// project's header and its Overview, Aliases, Enums and Tools tabs.
/// </summary>
public class ProjectScreen
{
    public ProjectConfigureMenu ConfigureMenu;
    public ProjectToolWindow ToolWindow;

    // Shared by every project; they edit the selected project's loaded data
    public ProjectAliasMenu AliasMenu;
    public ProjectEnumMenu EnumMenu;

    public ProjectSidebar Sidebar;
    public ProjectCreateView CreateView;
    public ProjectOverviewView OverviewView;

    /// <summary>
    /// The project shown in the detail pane. It may not be loaded; the loaded one the editors show is
    /// <see cref="ProjectOrchestrator.SelectedProject"/>.
    /// </summary>
    public ProjectEntry SelectedEntry = null;

    /// <summary>
    /// The detail pane shows the Create Project form.
    /// </summary>
    public bool IsCreating = false;

    public bool RequestFocus = false;

    // Set to switch tab or open an Overview section from elsewhere; cleared once applied
    private ProjectDetailTab? _pendingTab;
    public ProjectOverviewSection PendingSection = ProjectOverviewSection.None;

    public ProjectScreen()
    {
        ConfigureMenu = new(this);
        ToolWindow = new(this);

        AliasMenu = new();
        EnumMenu = new();

        Sidebar = new(this);
        CreateView = new(this);
        OverviewView = new(this);
    }

    #region Navigation

    /// <summary>
    /// Opens the Create Project form, for a game if one is given.
    /// </summary>
    public void ShowCreate(ProjectType type = ProjectType.Undefined)
    {
        ConfigureMenu.SetupForCreation(type);

        IsCreating = true;
        RequestFocus = true;
    }

    /// <summary>
    /// Shows a project, on a tab and, for the Overview, with a section opened.
    /// </summary>
    public void ShowProject(ProjectEntry project, ProjectDetailTab tab = ProjectDetailTab.Overview,
        ProjectOverviewSection section = ProjectOverviewSection.None)
    {
        SelectEntry(project);

        _pendingTab = tab;
        PendingSection = section;

        RequestFocus = true;
    }

    /// <summary>
    /// Shows a project in the detail pane, without changing which project the editors show.
    /// </summary>
    public void SelectEntry(ProjectEntry project)
    {
        IsCreating = false;

        if (SelectedEntry == project && ConfigureMenu.EditorMode is ProjectEditorMode.Edit && ConfigureMenu.Project == project)
            return;

        SelectedEntry = project;

        if (project != null)
            ConfigureMenu.SetupForEdit(project);
        else
            ConfigureMenu.SetupForCreation();
    }

    /// <summary>
    /// Called by the orchestrator when a loaded project becomes the one the editors show.
    /// </summary>
    public void OnProjectSelected(ProjectEntry project)
    {
        // The alias and enum edits belong to the previously selected project
        AliasMenu.ActionManager.Clear();
        EnumMenu.ActionManager.Clear();

        // Leave the Create form open if the user is filling it in
        if (!IsCreating)
            SelectEntry(project);
    }

    /// <summary>
    /// Called once the Create Project form has registered a project.
    /// </summary>
    public void OnProjectCreated(ProjectEntry project)
    {
        IsCreating = false;

        ShowProject(project);
    }

    /// <summary>
    /// Clears every reference the screen holds to a project removed from Smithbox, so it can't be saved back.
    /// </summary>
    public void OnProjectRemoved(ProjectEntry project)
    {
        if (SelectedEntry == project)
            SelectedEntry = null;

        if (ConfigureMenu.Project == project)
            ConfigureMenu.SetupForCreation();
    }

    #endregion

    #region Project Actions

    /// <summary>
    /// Loads a project, or switches to it if it is already loaded.
    /// </summary>
    public void LoadOrSwitch(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (project == null || orchestrator.IsProjectLoading)
            return;

        if (project.IsActive)
        {
            if (orchestrator.SelectedProject != project)
                orchestrator.SelectProject(project);

            return;
        }

        _ = orchestrator.StartupProject(project);
    }

    public void ReloadAction(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (project == null || !project.IsActive || orchestrator.IsProjectLoading)
            return;

        orchestrator.RequestProjectAction(project, UnsavedChangesAction.Reload, () =>
        {
            // Checked again, as the prompt can run this on a later frame
            if (!orchestrator.IsProjectLoading)
                _ = orchestrator.ReloadProject(project);
        });
    }

    public void UnloadAction(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (project == null || !project.IsActive)
            return;

        orchestrator.RequestProjectAction(project, UnsavedChangesAction.Unload, () =>
        {
            orchestrator.UnloadProject(project);

            // The form is reset, so settings edited before the unload aren't kept for an unloaded project
            if (SelectedEntry == project)
                ConfigureMenu.SetupForEdit(project);
        });
    }

    #endregion

    #region Display

    public void OnGUI(uint mainDockspaceID)
    {
        if (RequestFocus)
        {
            ImGui.SetNextWindowFocus();
            RequestFocus = false;
        }

        ImGui.SetNextWindowDockID(mainDockspaceID, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);

        var flags = GUI.GetMainWindowFlags() & ~ImGuiWindowFlags.MenuBar;

        if (ImGui.Begin($"{LOC.Get("PROJECT_Window_Project_Editor")}###ProjectEditor", flags))
        {
            Shortcuts();

            Display();
        }

        ImGui.End();
    }

    private void Display()
    {
        var tableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV;

        if (ImGui.BeginTable("projectEditorLayout", 2, tableFlags))
        {
            ImGui.TableSetupColumn("##projectSidebarCol", ImGuiTableColumnFlags.WidthFixed, 280f * DPI.UIScale());
            ImGui.TableSetupColumn("##projectDetailCol", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            // Projects
            ImGui.TableSetColumnIndex(0);

            ImGui.BeginChild("projectSidebar", Vector2.Zero, ImGuiChildFlags.None);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(EditorFocusContext.Project_None);

            Sidebar.Display();

            ImGui.EndChild();

            // Detail
            ImGui.TableSetColumnIndex(1);

            ImGui.BeginChild("projectDetail", Vector2.Zero, ImGuiChildFlags.None);

            DisplayDetail();

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }

    private void DisplayDetail()
    {
        var orchestrator = Smithbox.Orchestrator;

        // The stored projects are read on the first frames; until then there is nothing to say yet
        if (!orchestrator.ExistingProjectsRead)
            return;

        if (SelectedEntry != null && !orchestrator.Projects.Contains(SelectedEntry))
            SelectedEntry = null;

        if (IsCreating)
        {
            CreateView.Display();
            return;
        }

        if (orchestrator.Projects.Count == 0)
        {
            DisplayGetStarted();
            return;
        }

        if (SelectedEntry == null)
        {
            DisplayOpenProject();
            return;
        }

        // The form edits the shown project; something else (an unload, a removal) may have reset it
        if (ConfigureMenu.EditorMode is not ProjectEditorMode.Edit || ConfigureMenu.Project != SelectedEntry)
            ConfigureMenu.SetupForEdit(SelectedEntry);

        OverviewView.DisplayHeader(SelectedEntry);

        DisplayTabs(SelectedEntry);
    }

    /// <summary>
    /// With no projects: what a project is, and buttons to make one.
    /// </summary>
    private void DisplayGetStarted()
    {
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            FocusManager.SetFocus(EditorFocusContext.Project_None);

        GUI.EmptyState(LOC.Get("PROJECT_Start_Title"), LOC.Get("PROJECT_Start_Body"));

        // Detected Games
        var games = ConfigureMenu.GetDetectedGames();

        if (games.Count > 0)
        {
            GUI.SimpleHeader(LOC.Get("PROJECT_Start_Detected_Header"), LOC.Get("PROJECT_Start_Detected_Header_TT"));

            foreach (var (type, path) in games)
            {
                var gameName = LOC.Get(type.GetDisplayName());

                if (GUI.ExplainedButton(
                    $"{LOC.Get("PROJECT_Start_Create_For_Game", gameName)}##startCreate_{type}",
                    LOC.Get("PROJECT_Start_Create_For_Game_TT", path),
                    true))
                {
                    ShowCreate(type);
                }
            }

            GUI.Spacer();
        }

        // Other Ways
        GUI.SimpleHeader(LOC.Get("PROJECT_Start_Other_Header"), "");

        GUI.MultiButtonInput("startActions",
            "startNewProject",
            LOC.Get("PROJECT_Sidebar_New_Project"),
            LOC.Get("PROJECT_Sidebar_New_Project_TT"),
            () => ShowCreate(),

            "startImportProject",
            LOC.Get("PROJECT_Sidebar_Import_Project"),
            LOC.Get("PROJECT_Sidebar_Import_Project_TT"),
            ConfigureMenu.CreateProjectFromJsonAction);
    }

    /// <summary>
    /// With projects but none picked: how to pick one.
    /// </summary>
    private void DisplayOpenProject()
    {
        var orchestrator = Smithbox.Orchestrator;

        ImGui.BeginChild("NoProjectSection", ImGuiChildFlags.Borders);

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            FocusManager.SetFocus(EditorFocusContext.Project_None);

        GUI.EmptyState(LOC.Get("PROJECT_Open_Title"), LOC.Get("PROJECT_Open_Body"));

        // The project that loads at start-up, if it isn't loaded (auto-load is off, or it failed)
        var lastProject = orchestrator.Projects.FirstOrDefault(e => e.Descriptor.AutoSelect && !e.IsActive);

        if (lastProject != null)
        {
            if (GUI.ExplainedButton(
                $"{LOC.Get("PROJECT_Open_Load_Last", lastProject.Descriptor.ProjectName)}##openLoadLast",
                LOC.Get("PROJECT_Open_Load_Last_TT"),
                !orchestrator.IsProjectLoading))
            {
                SelectEntry(lastProject);
                LoadOrSwitch(lastProject);
            }
        }

        if (GUI.ExplainedButton(
            $"{LOC.Get("PROJECT_Sidebar_New_Project")}##openNewProject",
            LOC.Get("PROJECT_Sidebar_New_Project_TT"),
            true))
        {
            ShowCreate();
        }

        ImGui.EndChild();
    }

    private void DisplayTabs(ProjectEntry project)
    {
        if (!ImGui.BeginTabBar("projectDetailTabs"))
            return;

        // Overview
        if (BeginTab(ProjectDetailTab.Overview, LOC.Get("PROJECT_Tab_Overview")))
        {
            ImGui.BeginChild("projectOverviewTab", Vector2.Zero, ImGuiChildFlags.None);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(EditorFocusContext.Project_None);

            OverviewView.DisplayOverview(project);

            ImGui.EndChild();

            ImGui.EndTabItem();
        }

        // Aliases
        if (BeginTab(ProjectDetailTab.Aliases, LOC.Get("PROJECT_Tab_Aliases")))
        {
            if (IsReadyForLoadedData(project, LOC.Get("PROJECT_Tab_Aliases_Need")))
            {
                ImGui.BeginChild("projectAliasTab", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.MenuBar);

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                    FocusManager.SetFocus(EditorFocusContext.Project_AliasEditor);

                AliasMenu.Display();

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }

        // Enums
        if (BeginTab(ProjectDetailTab.Enums, LOC.Get("PROJECT_Tab_Enums")))
        {
            if (IsReadyForLoadedData(project, LOC.Get("PROJECT_Tab_Enums_Need")))
            {
                if (project.Handler.ParamData == null)
                {
                    DisplayEnumsNeedParamEditor(project);
                }
                else
                {
                    ImGui.BeginChild("projectEnumTab", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.MenuBar);

                    if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                        FocusManager.SetFocus(EditorFocusContext.Project_EnumEditor);

                    EnumMenu.Display();

                    ImGui.EndChild();
                }
            }

            ImGui.EndTabItem();
        }

        // Tools
        if (BeginTab(ProjectDetailTab.Tools, LOC.Get("PROJECT_Tab_Tools")))
        {
            if (IsReadyForLoadedData(project, LOC.Get("PROJECT_Tab_Tools_Need")))
            {
                ImGui.BeginChild("projectToolsTab", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.MenuBar);

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                    FocusManager.SetFocus(EditorFocusContext.Project_None);

                ToolWindow.Draw();

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private bool BeginTab(ProjectDetailTab tab, string label)
    {
        var flags = _pendingTab == tab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

        if (_pendingTab == tab)
            _pendingTab = null;

        return ImGui.BeginTabItem($"{label}##projectTab_{tab}", flags);
    }

    /// <summary>
    /// Whether the tabs that edit loaded data can show the project. If not, says why, with a button that fixes it.
    /// </summary>
    /// <param name="purpose">What the tab is for, finishing "Load this project to ...".</param>
    private bool IsReadyForLoadedData(ProjectEntry project, string purpose)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (orchestrator.IsProjectLoading && orchestrator.LoadingProject == project)
        {
            GUI.EmptyState(LOC.Get("PROJECT_Tab_Loading_Title", project.Descriptor.ProjectName), "");
            return false;
        }

        if (!project.IsActive)
        {
            GUI.EmptyState(LOC.Get("PROJECT_Tab_Not_Loaded_Title"), LOC.Get("PROJECT_Tab_Not_Loaded_Body", purpose));

            if (GUI.ExplainedButton(
                $"{LOC.Get("PROJECT_Header_Load")}##tabLoadProject",
                LOC.Get("PROJECT_Header_Load_TT"),
                !orchestrator.IsProjectLoading))
            {
                LoadOrSwitch(project);
            }

            return false;
        }

        if (orchestrator.SelectedProject != project)
        {
            GUI.EmptyState(LOC.Get("PROJECT_Tab_Not_Selected_Title"), LOC.Get("PROJECT_Tab_Not_Selected_Body", purpose));

            if (GUI.ExplainedButton(
                $"{LOC.Get("PROJECT_Header_Switch")}##tabSwitchProject",
                LOC.Get("PROJECT_Header_Switch_TT"),
                !orchestrator.IsProjectLoading))
            {
                LoadOrSwitch(project);
            }

            return false;
        }

        if (project.Handler?.ProjectData == null)
        {
            GUI.EmptyState(LOC.Get("PROJECT_Tab_No_Data_Title"), LOC.Get("PROJECT_Tab_No_Data_Body"));
            return false;
        }

        return true;
    }

    private void DisplayEnumsNeedParamEditor(ProjectEntry project)
    {
        GUI.EmptyState(LOC.Get("PROJECT_Tab_Enums_No_Params_Title"), LOC.Get("PROJECT_Tab_Enums_No_Params_Body"));

        if (GUI.ExplainedButton(
            $"{LOC.Get("PROJECT_Tab_Enums_Enable_Params")}##enumsEnableParams",
            LOC.Get("PROJECT_Tab_Enums_Enable_Params_TT"),
            true))
        {
            ShowProject(project, ProjectDetailTab.Overview, ProjectOverviewSection.Editors);
        }
    }

    #endregion

    public void Shortcuts()
    {
        if (!FocusManager.IsInProjectEditor())
            return;

        // The editors check for their own focus context, which stays set after their tab is left, so the data
        // they act on is checked here too
        var project = Smithbox.Orchestrator.SelectedProject;

        if (project?.Handler?.ProjectData == null || Smithbox.Orchestrator.IsProjectLoading)
            return;

        AliasMenu.Shortcuts();

        if (project.Handler.ParamData != null)
            EnumMenu.Shortcuts();
    }
}
