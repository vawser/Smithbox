using Hexa.NET.ImGui;
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

    public unsafe void OnGUI()
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

        if (ImGui.Begin("Projects##ProjectEditor", UIHelper.GetMainWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            Shortcuts();

            if (ImGui.BeginMenuBar())
            {
                EditMenu();
                OptionsMenu();

                ImGui.EndMenuBar();
            }

            var dsid = ImGui.GetID("DockSpace_ProjectEditor");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

            if (ImGui.Begin("Project View##ProjectView", UIHelper.GetInnerWindowFlags()))
            {
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.Project);
                }

                Display();

                ImGui.End();
            }

            ImGui.End();
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void Display()
    {
        var columnCount = 3;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("projectTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##ProjectList", ImGuiTableColumnFlags.WidthStretch, 0.25f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ProjectListArea", new Vector2(0, 0), windowFlags);

            DisplayProjectList();

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ProjectContentArea", new Vector2(0, 0), windowFlags);

            DisplayProjectCreator();

            ImGui.EndChild();

            // --- Column 3 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ProjectMetadataArea", new Vector2(0, 0), windowFlags);

            DisplayMetadataEditors();

            ImGui.EndChild();


            ImGui.EndTable();
        }
    }

    public void DisplayProjectList()
    {
        float width = ImGui.GetContentRegionAvail().X;
        float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("globalProjectActions",
            "createNewProject", "Create Project", "", CreateProjectAction,
            "createProjectFromJson", "Create Project from JSON", "", CreateProjectFromJsonAction);

        UIHelper.SimpleHeader("Loaded Projects", "");

        EditorFilters.DisplayFramedListFilter("loadedProjectsFilter", ref LoadedListFilter, ref ExactLoadedListFilter);

        ImGui.BeginChild("loadedProjectList", new Vector2(width, height * CFG.Current.Project_Display_LoadedProjectList), ImGuiChildFlags.Borders);

        DisplayLoadedProjectEntries();

        ImGui.EndChild();

        UIHelper.SimpleHeader("Available Projects", "");


        EditorFilters.DisplayFramedListFilter("availableProjectsFilter", ref AvailableListFilter, ref ExactAvailableListFilter);

        if (SelectedAvaliableEntry != null)
        {
            if (!SelectedAvaliableEntry.Initialized)
            {
                UIHelper.MultiButtonInput("availableProjectActions",
                    "loadSelectedProject", "Load Selected Project", "", LoadSelectedProjectAction);
            }
        }
        ImGui.BeginChild("availableProjectList", new Vector2(width, height * CFG.Current.Project_Display_AvailableProjectList), ImGuiChildFlags.Borders);

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
            if (ImGui.BeginTabItem($"DES##tab_DES_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DES, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1))
        {
            if (ImGui.BeginTabItem($"DS1##tab_DS1_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS1, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS1R))
        {
            if (ImGui.BeginTabItem($"DS1R##tab_DS1R_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS1R, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2))
        {
            if (ImGui.BeginTabItem($"DS2##tab_DS2_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS2, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS2S))
        {
            if (ImGui.BeginTabItem($"DS2S##tab_DS2S_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS2S, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.DS3))
        {
            if (ImGui.BeginTabItem($"DS3##tab_DS3_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.DS3, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.BB))
        {
            if (ImGui.BeginTabItem($"BB##tab_BB_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.BB, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.SDT))
        {
            if (ImGui.BeginTabItem($"SDT##tab_SDT_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.SDT, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.ER))
        {
            if (ImGui.BeginTabItem($"ER##tab_ER_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.ER, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.AC6))
        {
            if (ImGui.BeginTabItem($"AC6##tab_AC6_{imguiKey}"))
            {
                DisplayProjectListGroup(ProjectType.AC6, imguiKey, isLoaded);
                ImGui.EndTabItem();
            }
        }

        if (orchestrator.Projects.Any(e => e.Descriptor.ProjectType is ProjectType.NR))
        {
            if (ImGui.BeginTabItem($"NR##tab_NR_{imguiKey}"))
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
            if (ImGui.MenuItem("Load", false, !project.Initialized))
            {
                orchestrator.SelectedProject = project;

                if (!orchestrator.IsProjectLoading)
                {
                    _ = orchestrator.StartupProject(project);
                }

                SelectedLoadedEntry = project;
                ConfigureMenu.EditorMode = ProjectEditorMode.Edit;
            }

            if (ImGui.MenuItem("Reload", false, project.Initialized))
            {
                orchestrator.SelectedProject = project;

                if (!orchestrator.IsProjectLoading)
                {
                    _ = orchestrator.ReloadProject(project);
                }
            }

            if (ImGui.MenuItem("Unload", false, project.Initialized))
            {
                orchestrator.UnloadProject(project);

                ConfigureMenu.EditorMode = ProjectEditorMode.Create;
            }

            if (ImGui.BeginMenu("Information"))
            {
                if (ImGui.Selectable("Copy GUID"))
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
        var result = PlatformUtils.Instance.OpenFileDialog("Select Project JSON", out projectJsonPath);

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

    public void DisplayMetadataEditors()
    {
        ImGui.BeginChild("MetadataEditorsSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar($"##projectMetaEditorTabbar");

        if(ImGui.BeginTabItem("Alias Editor"))
        {
            DisplayAliasEditor();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Enum Editor"))
        {
            DisplayEnumEditor();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void DisplayEnumEditor()
    {
        EnumMenu.Draw();
    }

    public void DisplayAliasEditor()
    {
        AliasMenu.Draw();
    }

    public void FileMenu()
    {
        var orchestrator = Smithbox.Orchestrator;

        if (ImGui.BeginMenu("File"))
        {
            if(ImGui.Selectable("Open Project JSON Directory"))
            {
                var storedDir = ProjectUtils.GetProjectsFolder();
                Process.Start("explorer.exe", storedDir);
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if(ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (Smithbox.Orchestrator.ActionManager.CanUndo())
                {
                    Smithbox.Orchestrator.ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (Smithbox.Orchestrator.ActionManager.CanUndo())
                {
                    Smithbox.Orchestrator.ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (Smithbox.Orchestrator.ActionManager.CanRedo())
                {
                    Smithbox.Orchestrator.ActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("Display"))
            {
                ImGui.SliderFloat("Loaded Projects##loadedProjectListPercent", ref CFG.Current.Project_Display_LoadedProjectList, 0.01f, 0.99f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    var remainder = 1f - CFG.Current.Project_Display_LoadedProjectList;

                    CFG.Current.Project_Display_AvailableProjectList = remainder;
                }
                UIHelper.Tooltip("The percentage of the window the Loaded Projects section occupies.");

                ImGui.SliderFloat("Available Projects##availableProjectListPercent", ref CFG.Current.Project_Display_AvailableProjectList, 0.01f, 0.99f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    var remainder = 1f - CFG.Current.Project_Display_AvailableProjectList;

                    CFG.Current.Project_Display_LoadedProjectList = remainder;
                }
                UIHelper.Tooltip("The percentage of the window the Available Projects section occupies.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Shortcuts()
    {

    }

}
