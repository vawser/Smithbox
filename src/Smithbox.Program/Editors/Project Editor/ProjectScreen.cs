using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

public class ProjectScreen
{
    public ActionManager EditorActionManager = new();

    public ProjectConfigureMenu ConfigureMenu;
    public ProjectToolWindow ToolWindow;

    public ProjectEntry SelectedLoadedEntry = null;
    public ProjectEntry SelectedAvaliableEntry = null;

    public string LoadedListFilter = "";
    public bool ExactLoadedListFilter = false;

    public string AvailableListFilter = "";
    public bool ExactAvailableListFilter = false;

    public bool RequestFocus = false;

    public ProjectScreen()
    {
        ConfigureMenu = new(this);
        ToolWindow = new(this);
    }

    public unsafe void OnGUI(uint mainDockspaceID)
    {
        if (RequestFocus)
        {
            ImGui.SetNextWindowFocus();
            RequestFocus = false;
        }

        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.SetNextWindowDockID(mainDockspaceID, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);
        if (ImGui.Begin($"{LOC.Get("PROJECT_Window_Project_Editor")}###ProjectEditor", GUI.GetMainWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            Shortcuts();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            var dsid = ImGui.GetID("DockSpace_ProjectEditor");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_ProjectEditor);

            ImGui.SetNextWindowDockID(dsid, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectEditor);
            if (ImGui.Begin($"{LOC.Get("PROJECT_Window_Project_View")}###ProjectView", GUI.GetInnerWindowFlags()))
            {
            }

            var viewDockId = ImGui.GetID($"DockSpace_ProjectEditorView");
            ImGui.DockSpace(viewDockId, new Vector2(0, 0), ref GUI.DockGroup_ProjectEditorView);

            Display(viewDockId);

            ImGui.End();

            ImGui.End();
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void Display(uint editorDockspaceId)
    {
        if (CFG.Current.Interface_ProjectEditor_ProjectList)
        {
            // Active Projects
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PROJECT_Window_Active_Projects")}###projectEditor_ActiveProjects", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_None);
                }

                DisplayActiveProjectList();
            }

            ImGui.End();

            // Project List
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PROJECT_Window_Available_Projects")}###projectEditor_AvaliableProjects", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_None);
                }

                DisplayAvailableProjectList();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_ProjectEditor_ProjectConfiguration)
        {
            // Project Configuration
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PROJECT_Window_Project_Configuration")}###projectEditor_ProjectConfiguration", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_None);
                }

                DisplayProjectCreator();
            }

            ImGui.End();
        }

        if(CFG.Current.Interface_ProjectEditor_Tools)
        {
            // Tools 
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PROJECT_Window_Project_Tools")}###projectEditor_Tools", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_None);
                }

                ToolWindow.Draw();
            }

            ImGui.End();
        }
    }

    public void DisplayActiveProjectList()
    {
        GUI.TitleHeader(
            LOC.Get("PROJECT_Configuration_Header_Active_Projects"), 
            LOC.Get("PROJECT_Configuration_Header_Active_Projects_TT"));

        EditorFilters.DisplayFramedListFilter("loadedProjectsFilter", ref LoadedListFilter, ref ExactLoadedListFilter);

        ImGui.BeginChild("loadedProjectList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayLoadedProjectEntries();

        ImGui.EndChild();
    }

    public void DisplayAvailableProjectList()
    {
        GUI.TitleHeader(
            LOC.Get("PROJECT_Configuration_Header_Available_Projects"),
            LOC.Get("PROJECT_Configuration_Header_Available_Projects_TT"));

        EditorFilters.DisplayFramedListFilter("availableProjectsFilter", ref AvailableListFilter, ref ExactAvailableListFilter);

        ImGui.BeginChild("availableProjectList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayAvailableProjectEntries();

        ImGui.EndChild();
    }

    public void DisplayLoadedProjectEntries()
    {
        string imguiKey = "loaded";
        bool isLoaded = true;

        var orchestrator = Smithbox.Orchestrator;

        var projectList = orchestrator.Projects.ToList();

        if (!projectList.Any())
            return;

        foreach (var project in projectList)
        {
            // Projects initialized only for another project's comparison data aren't open projects
            if (project.IsAuxiliary)
                continue;

            var isMatch = EditorFilters.IsMatch(LoadedListFilter, project.Descriptor.ProjectName, ExactLoadedListFilter);

            if (!isMatch)
                continue;

            DisplayProjectListEntry(project, imguiKey, isLoaded);
        }
    }

    public void DisplayAvailableProjectEntries()
    {
        string imguiKey = "available";
        bool isLoaded = false;

        var orchestrator = Smithbox.Orchestrator;

        ImGui.BeginTabBar($"##projectSelectionTabBar_{imguiKey}");

        // One tab per game with projects, in the same order as the project type combo
        foreach (var projectType in ProjectTypeOrder.Order)
        {
            if (!orchestrator.Projects.Any(e => e.Descriptor.ProjectType == projectType))
                continue;

            if (ImGui.BeginTabItem($"{LOC.Get($"EDITOR_Short_Project_Type_{projectType}")}##tab_{projectType}_{imguiKey}"))
            {
                DisplayProjectListGroup(projectType, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
    }

    private void DisplayProjectListGroup(ProjectType projectType, string imguiKey, bool isLoaded)
    {
        var orchestrator = Smithbox.Orchestrator;

        var projectList = orchestrator.Projects.Where(e => e.Descriptor.ProjectType == projectType).ToList();

        if (!projectList.Any())
            return;

        Dictionary<string, List<ProjectEntry>> _projectFolders = new();

        if (projectList.Count > 0)
        {
            foreach (var project in projectList)
            {
                if (project.Descriptor.FolderTag != "")
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
        foreach (var folder in _projectFolders)
        {
            var name = folder.Key;
            var projects = folder.Value;

            if (ImGui.CollapsingHeader($"{name}##projectFolder_{name}_{imguiKey}"))
            {
                foreach (var project in projects)
                {
                    if (isLoaded)
                    {
                        var isMatch = EditorFilters.IsMatch(LoadedListFilter, project.Descriptor.ProjectName, ExactLoadedListFilter);

                        if (!isMatch)
                            continue;

                        DisplayProjectListEntry(project, imguiKey, isLoaded);
                    }
                    else
                    {
                        var isMatch = EditorFilters.IsMatch(AvailableListFilter, project.Descriptor.ProjectName, ExactAvailableListFilter);

                        if (!isMatch)
                            continue;

                        DisplayProjectListEntry(project, imguiKey, isLoaded);
                    }
                }
            }
        }

        if (_projectFolders.Count > 0 && projectList.Any(e => e.Descriptor.FolderTag == ""))
        {
            ImGui.Separator();
        }

        // Unassociated Projects
        if (projectList.Count > 0)
        {
            foreach (var project in projectList)
            {
                // Skip these as they are handled above
                if (project.Descriptor.FolderTag != "")
                    continue;

                if (isLoaded)
                {
                    var isMatch = EditorFilters.IsMatch(LoadedListFilter, project.Descriptor.ProjectName, ExactLoadedListFilter);

                    if (!isMatch)
                        continue;

                    DisplayProjectListEntry(project, imguiKey, isLoaded);
                }
                else
                {
                    var isMatch = EditorFilters.IsMatch(AvailableListFilter, project.Descriptor.ProjectName, ExactAvailableListFilter);

                    if (!isMatch)
                        continue;

                    DisplayProjectListEntry(project, imguiKey, isLoaded);
                }
            }
        }
    }
    private void DisplayProjectListEntry(ProjectEntry project, string imguiKey, bool isLoaded)
    {
        var orchestrator = Smithbox.Orchestrator;

        var selectId = $"{project.Descriptor.ProjectGUID}_{imguiKey}";
        var displayName = $"{project.Descriptor.ProjectName}";

        if(isLoaded)
        {
            if(!project.IsActive)
                return;

            // Highlights the project the editors show, which can change outside this list (load, unload)
            var isSelected = orchestrator.SelectedProject == project;

            if(ImGui.Selectable($"{displayName}##{selectId}", isSelected))
            {
                orchestrator.SelectProject(project);

                if (CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment)
                {
                    orchestrator.SetAsAutoLoad(project);
                }

                ConfigureMenu.SetupForEdit(project);

                Smithbox.Orchestrator.ProjectMetadataEditor.AliasMenu.ActionManager.Clear();
                Smithbox.Orchestrator.ProjectMetadataEditor.EnumMenu.ActionManager.Clear();
            }
        }
        else
        {
            var isSelected = SelectedAvaliableEntry == project;
            var isActive = project.IsActive;

            if (isActive)
            {
                displayName = $"{displayName}{LOC.Get("PROJECT_List_Active_Suffix")}";
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);
            }

            if (ImGui.Selectable($"{displayName}##{selectId}", isSelected))
            {
                SelectedAvaliableEntry = project;

                ConfigureMenu.SetupForEdit(project);
            }

            if (isActive)
            {
                ImGui.PopStyleColor(1);
            }

            if (ImGui.IsItemHovered())
            {
                // Double-click loads the project, or switches to it if it is already active
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (!isActive)
                    {
                        LoadProjectAction(project);
                    }
                    else if (orchestrator.SelectedProject != project)
                    {
                        orchestrator.SelectProject(project);
                    }
                }

                GUI.Tooltip(GetProjectEntryTooltip(project));
            }
        }

        ProjectContextMenu(project, imguiKey);
    }

    private static string GetProjectEntryTooltip(ProjectEntry project)
    {
        var descriptor = project.Descriptor;

        return LOC.Get("PROJECT_List_Entry_TT",
            LOC.Get(descriptor.ProjectType.GetDisplayName()),
            descriptor.ProjectPath,
            descriptor.DataPath);
    }

    public void ProjectContextMenu(ProjectEntry project, string imguiKey)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (ImGui.BeginPopupContextItem($"##contextMenu_{project.Descriptor.ProjectGUID}_{imguiKey}"))
        {
            // The orchestrator selects the project once it has loaded. Other active projects stay loaded.
            var isOpen = project.IsActive;

            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Load")}##loadAction", false, !isOpen && !orchestrator.IsProjectLoading))
            {
                LoadProjectAction(project);
            }

            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Reload")}##reloadAction", false, isOpen && !orchestrator.IsProjectLoading))
            {
                orchestrator.RequestProjectAction(project, UnsavedChangesAction.Reload, () =>
                {
                    // Checked again, as the prompt can run this on a later frame
                    if (!orchestrator.IsProjectLoading)
                        _ = orchestrator.ReloadProject(project);
                });
            }

            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Unload")}##unloadAction", false, isOpen))
            {
                orchestrator.RequestProjectAction(project, UnsavedChangesAction.Unload, () =>
                {
                    orchestrator.UnloadProject(project);

                    if (SelectedLoadedEntry == project)
                        SelectedLoadedEntry = null;

                    ConfigureMenu.SetupForCreation();
                });
            }

            if (ImGui.BeginMenu($"{LOC.Get("PROJECT_Configuration_Global_Context_Header_Information")}##infoHeader"))
            {
                if (ImGui.Selectable($"{LOC.Get("PROJECT_Configuration_Global_Context_Copy_GUID")}##copyGuidAction"))
                {
                    PlatformUtils.Instance.SetClipboardText($"{project.Descriptor.ProjectGUID}");
                }

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    public void LoadSelectedProjectAction()
    {
        LoadProjectAction(SelectedAvaliableEntry);
    }

    private void LoadProjectAction(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (project == null || orchestrator.IsProjectLoading)
            return;

        _ = orchestrator.StartupProject(project, () =>
        {
            SelectedLoadedEntry = project;
            ConfigureMenu.SetupForEdit(project);
        });
    }

    /// <summary>
    /// Clears every reference the screen holds to a project removed from Smithbox, so it can't be saved back.
    /// </summary>
    public void OnProjectRemoved(ProjectEntry project)
    {
        if (SelectedLoadedEntry == project)
            SelectedLoadedEntry = null;

        if (SelectedAvaliableEntry == project)
            SelectedAvaliableEntry = null;

        if (ConfigureMenu.Project == project)
            ConfigureMenu.SetupForCreation();
    }

    public void DisplayProjectCreator()
    {
        ImGui.BeginChild("ProjectEditorSection", ImGuiChildFlags.None);

        ConfigureMenu.Display();

        ImGui.EndChild();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PROJECT_Configuration_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_View_Project_List")}##projectListToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectList = !CFG.Current.Interface_ProjectEditor_ProjectList;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectList);

            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_View_Project_Configuration")}##projectConfigurationToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectConfiguration = !CFG.Current.Interface_ProjectEditor_ProjectConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectConfiguration);

            if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_View_Tools")}##projectToolsToggle"))
            {
                CFG.Current.Interface_ProjectEditor_Tools = !CFG.Current.Interface_ProjectEditor_Tools;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_Tools);

            ImGui.EndMenu();
        }
    }

    public void Shortcuts()
    {
        if (!FocusManager.IsInProjectEditor())
            return;
    }
}
