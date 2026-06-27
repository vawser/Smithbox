using Google.Protobuf.Reflection;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Application;

public class ProjectScreen
{
    public ActionManager EditorActionManager = new();

    public ProjectConfigureMenu ConfigureMenu;
    public ProjectEnumMenu EnumMenu;
    public ProjectAliasMenu AliasMenu;

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
        EnumMenu = new();
        AliasMenu = new();
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
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_EditorView);
        if (ImGui.Begin($"{LOC.Get("PRJ_Window_Project_Editor")}###ProjectEditor", UIHelper.GetMainWindowFlags()))
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
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref UIHelper.DockGroup_ProjectEditor);

            ImGui.SetNextWindowDockID(dsid, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditor);
            if (ImGui.Begin($"{LOC.Get("PRJ_Window_Project_View")}###ProjectView", UIHelper.GetInnerWindowFlags()))
            {
            }

            var viewDockId = ImGui.GetID($"DockSpace_ProjectEditorView");
            ImGui.DockSpace(viewDockId, new Vector2(0, 0), ref UIHelper.DockGroup_ProjectEditorView);

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
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Active_Projects")}###projectEditor_ActiveProjects", UIHelper.GetInnerWindowFlags()))
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
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Available_Projects")}###projectEditor_AvaliableProjects", UIHelper.GetInnerWindowFlags()))
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
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Project_Configuration")}###projectEditor_ProjectConfiguration", UIHelper.GetInnerWindowFlags()))
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

        if (CFG.Current.Interface_ProjectEditor_ProjectAliases)
        {
            // Project Aliases
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Project_Aliases")}###projectEditor_ProjectAliases", UIHelper.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_AliasEditor);
                }

                DisplayAliasEditor();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_ProjectEditor_ProjectEnums)
        {
            // Project Enums
            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ProjectEditorView);
            if (ImGui.Begin($@"{LOC.Get("PRJ_Window_Project_Enums")}###projectEditor_ProjectEnums", UIHelper.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project_EnumEditor);
                }

                DisplayEnumEditor();
            }

            ImGui.End();
        }
    }

    public void DisplayActiveProjectList()
    {
        UIHelper.SimpleHeader(
            LOC.Get("PRJ_PCM_Header_Active_Projects"), 
            LOC.Get("PRJ_PCM_Header_Active_Projects_TT"));

        EditorFilters.DisplayFramedListFilter("loadedProjectsFilter", ref LoadedListFilter, ref ExactLoadedListFilter);

        ImGui.BeginChild("loadedProjectList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayLoadedProjectEntries();

        ImGui.EndChild();
    }

    public void DisplayAvailableProjectList()
    {
        UIHelper.SimpleHeader(
            LOC.Get("PRJ_PCM_Header_Global_Actions"),
            LOC.Get("PRJ_PCM_Header_Global_Actions_TT"));

        UIHelper.MultiButtonInput("globalProjectActions",
            "createNewProject",
            LOC.Get("PRJ_PCM_Global_Action_Create_Project"), 
            LOC.Get("PRJ_PCM_Global_Action_Create_Project_TT"),
            CreateProjectAction,

            "createProjectFromJson",
            LOC.Get("PRJ_PCM_Global_Action_Create_Project_From_JSON"),
            LOC.Get("PRJ_PCM_Global_Action_Create_Project_From_JSON_TT"),
            CreateProjectFromJsonAction);

        UIHelper.SimpleHeader(
            LOC.Get("PRJ_PCM_Header_Available_Projects"),
            LOC.Get("PRJ_PCM_Header_Available_Projects_TT"));

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
            // Skip these as they are handled above
            if (project.Descriptor.FolderTag != "")
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

        // Project List
        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DES))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DES")}##tab_DES_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DES, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DS1")}##tab_DS1_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS1, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1R))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DS1R")}##tab_DS1R_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS1R, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DS2")}##tab_DS2_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS2, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2S))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DS2S")}##tab_DS2S_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS2S, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS3))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_DS3")}##tab_DS3_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS3, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.BB))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_BB")}##tab_BB_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.BB, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.SDT))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_SDT")}##tab_SDT_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.SDT, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.ER))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_ER")}##tab_ER_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.ER, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.AC6))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_AC6")}##tab_AC6_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.AC6, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.NR))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("EDC_Short_Project_Type_NR")}##tab_NR_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.NR, imguiKey, isLoaded);
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
            if(!project.Initialized)
                return;

            var isSelected = SelectedLoadedEntry == project;

            if(ImGui.Selectable($"{displayName}##{selectId}", isSelected))
            {
                SelectedLoadedEntry = project;

                orchestrator.SelectedProject = project;
                Smithbox.Instance.SetProgramName(project);

                if (CFG.Current.Project_Enable_Automatic_Auto_Load_Assignment)
                {
                    orchestrator.SetAsAutoLoad(project);
                }

                ConfigureMenu.SetupForEdit(project);

                AliasMenu.ActionManager.Clear();
                EnumMenu.ActionManager.Clear();
            }
        }
        else
        {
            var isSelected = SelectedAvaliableEntry == project;

            if (ImGui.Selectable($"{displayName}##{selectId}", isSelected))
            {
                SelectedAvaliableEntry = project;

                ConfigureMenu.SetupForEdit(project);
            }
        }

        ProjectContextMenu(project, imguiKey);
    }

    public void ProjectContextMenu(ProjectEntry project, string imguiKey)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (ImGui.BeginPopupContextItem($"##contextMenu_{project.Descriptor.ProjectGUID}_{imguiKey}"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_Global_Context_Load")}##loadAction", false, !project.Initialized))
            {
                orchestrator.SelectedProject = project;

                if (!orchestrator.IsProjectLoading)
                {
                    _ = orchestrator.StartupProject(project);
                }

                SelectedLoadedEntry = project;
                ConfigureMenu.EditorMode = ProjectEditorMode.Edit;
            }

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_Global_Context_Reload")}##reloadAction", false, project.Initialized))
            {
                orchestrator.SelectedProject = project;

                if (!orchestrator.IsProjectLoading)
                {
                    _ = orchestrator.ReloadProject(project);
                }
            }

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_Global_Context_Unload")}##unloadAction", false, project.Initialized))
            {
                orchestrator.UnloadProject(project);

                ConfigureMenu.EditorMode = ProjectEditorMode.Create;
            }

            if (ImGui.BeginMenu($"{LOC.Get("PRJ_PCM_Global_Context_Header_Information")}##infoHeader"))
            {
                if (ImGui.Selectable($"{LOC.Get("PRJ_PCM_Global_Context_Copy_GUID")}##copyGuidAction"))
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
        var project = SelectedAvaliableEntry;
        var orchestrator = Smithbox.Orchestrator;

        orchestrator.SelectedProject = project;

        if (!orchestrator.IsProjectLoading)
        {
            _ = orchestrator.StartupProject(project);
        }

        SelectedLoadedEntry = project;
        ConfigureMenu.EditorMode = ProjectEditorMode.Edit;
    }

    public void CreateProjectAction()
    {
        ConfigureMenu.SetupForCreation();
    }

    public void CreateProjectFromJsonAction()
    {
        var orchestrator = Smithbox.Orchestrator;

        var projectJsonPath = "";
        var result = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PRJ_PCM_Action_Select_Project_JSON"), out projectJsonPath);

        if (result)
        {
            orchestrator.CreateProjectFromLegacyJson(projectJsonPath);
        }
    }

    public void DisplayProjectCreator()
    {
        ImGui.BeginChild("ProjectEditorSection", ImGuiChildFlags.None);

        ConfigureMenu.Display();

        ImGui.EndChild();
    }

    public void DisplayEnumEditor()
    {
        EnumMenu.Display();
    }

    public void DisplayAliasEditor()
    {
        AliasMenu.Display();
    }

    public void EditMenu()
    {
        if(ImGui.BeginMenu($"{LOC.Get("PRJ_PCM_Header_Edit")}##editMenuHeader"))
        {
            AliasMenu.EditMenu();
            EnumMenu.EditMenu();

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_PCM_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_List")}##projectListToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectList = !CFG.Current.Interface_ProjectEditor_ProjectList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectList);

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_Configuration")}##projectConfigurationToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectConfiguration = !CFG.Current.Interface_ProjectEditor_ProjectConfiguration;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectConfiguration);

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_Aliases")}##projectAliasesToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectAliases = !CFG.Current.Interface_ProjectEditor_ProjectAliases;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectAliases);

            if (ImGui.MenuItem($"{LOC.Get("PRJ_PCM_View_Project_Enums")}##projectEnumsToggle"))
            {
                CFG.Current.Interface_ProjectEditor_ProjectEnums = !CFG.Current.Interface_ProjectEditor_ProjectEnums;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ProjectEditor_ProjectEnums);

            ImGui.EndMenu();
        }
    }

    public void Shortcuts()
    {
        if (!FocusManager.IsInProjectEditor())
            return;

        AliasMenu.Shortcuts();
        EnumMenu.Shortcuts();
    }
}
