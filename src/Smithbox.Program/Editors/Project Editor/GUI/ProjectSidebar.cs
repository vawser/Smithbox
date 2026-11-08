using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The Project Editor's list of projects: an Open tab first, then one tab per game, grouped by group tag.
/// </summary>
public class ProjectSidebar
{
    public ProjectScreen Editor;

    private string Filter = "";
    private bool ExactFilter = false;

    public ProjectSidebar(ProjectScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var orchestrator = Smithbox.Orchestrator;

        // Actions
        GUI.MultiButtonInput("projectSidebarActions",
            "sidebarNewProject",
            $"{Icons.Plus} {LOC.Get("PROJECT_Sidebar_New_Project")}",
            LOC.Get("PROJECT_Sidebar_New_Project_TT"),
            () => Editor.ShowCreate(),

            "sidebarImportProject",
            $"{Icons.Download} {LOC.Get("PROJECT_Sidebar_Import_Project")}",
            LOC.Get("PROJECT_Sidebar_Import_Project_TT"),
            Editor.ConfigureMenu.CreateProjectFromJsonAction);

        EditorFilters.DisplayFramedListFilter("projectSidebarFilter", ref Filter, ref ExactFilter);

        var projects = orchestrator.Projects.ToList();

        if (projects.Count == 0)
        {
            ImGui.BeginChild("projectSidebarList", Vector2.Zero, ImGuiChildFlags.Borders);

            if (orchestrator.ExistingProjectsRead)
                ImGui.TextDisabled(LOC.Get("PROJECT_Sidebar_No_Projects"));

            ImGui.EndChild();
            return;
        }

        // Tabs come from the unfiltered list so typing in the filter never removes the tab being viewed
        if (ImGui.BeginTabBar("projectSidebarTabs", ImGuiTabBarFlags.FittingPolicyScroll))
        {
            DisplayOpenTab(projects);
            DisplayGameTabs(projects);

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    /// The loaded projects. Projects loaded only for another project's comparison data aren't open projects.
    /// </summary>
    private void DisplayOpenTab(List<ProjectEntry> projects)
    {
        if (!projects.Any(e => e.IsActive))
            return;

        if (!ImGui.BeginTabItem($"{LOC.Get("PROJECT_Sidebar_Open_Header")}##sidebarOpenTab"))
            return;

        ImGui.BeginChild("projectSidebarList_open", Vector2.Zero, ImGuiChildFlags.Borders);

        foreach (var project in projects.Where(e => e.IsActive && IsMatch(e)))
        {
            DisplayEntry(project, "open", true);
        }

        ImGui.EndChild();
        ImGui.EndTabItem();
    }

    /// <summary>
    /// One tab per game in the project type order; within a tab, projects are grouped by group tag.
    /// </summary>
    private void DisplayGameTabs(List<ProjectEntry> projects)
    {
        foreach (var projectType in ProjectTypeOrder.Order)
        {
            if (!projects.Any(e => e.Descriptor.ProjectType == projectType))
                continue;

            var shortName = LOC.Get($"EDITOR_Short_Project_Type_{projectType}");
            var tabOpen = ImGui.BeginTabItem($"{shortName}##sidebarGameTab_{projectType}");

            if (ImGui.IsItemHovered())
                GUI.Tooltip(LOC.Get(projectType.GetDisplayName()));

            if (!tabOpen)
                continue;

            ImGui.BeginChild($"projectSidebarList_{projectType}", Vector2.Zero, ImGuiChildFlags.Borders);

            DisplayGameProjects(projects, projectType);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }

    private void DisplayGameProjects(List<ProjectEntry> projects, ProjectType projectType)
    {
        var gameProjects = projects
            .Where(e => e.Descriptor.ProjectType == projectType && IsMatch(e))
            .OrderBy(e => e.Descriptor.ProjectName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Group Tags
        var tagged = gameProjects
            .Where(e => e.Descriptor.FolderTag != "")
            .GroupBy(e => e.Descriptor.FolderTag)
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in tagged)
        {
            if (ImGui.TreeNodeEx($"{Icons.Folder} {group.Key}##sidebarTag_{projectType}_{group.Key}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var project in group)
                {
                    DisplayEntry(project, "game", false);
                }

                ImGui.TreePop();
            }
        }

        // Untagged
        foreach (var project in gameProjects.Where(e => e.Descriptor.FolderTag == ""))
        {
            DisplayEntry(project, "game", false);
        }
    }

    private bool IsMatch(ProjectEntry project)
    {
        return EditorFilters.IsMatch(Filter, project.Descriptor.ProjectName, ExactFilter);
    }

    /// <param name="inOpenGroup">The entry is in the Open group, which shows the game rather than repeating the
    /// open state.</param>
    private void DisplayEntry(ProjectEntry project, string imguiKey, bool inOpenGroup)
    {
        var orchestrator = Smithbox.Orchestrator;

        var isShown = Editor.SelectedEntry == project && !Editor.IsCreating;
        var isEditorProject = orchestrator.SelectedProject == project;
        var isActive = project.IsActive;

        // ● on the project the editors show; open projects in the benefit colour
        var marker = isEditorProject ? $"{Icons.Circle} " : "";
        var suffix = inOpenGroup
            ? $"  ({LOC.Get($"EDITOR_Short_Project_Type_{project.Descriptor.ProjectType}")})"
            : "";

        if (isActive)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);

        if (ImGui.Selectable($"{marker}{project.Descriptor.ProjectName}{suffix}##{project.Descriptor.ProjectGUID}_{imguiKey}", isShown))
        {
            Editor.SelectEntry(project);
        }

        if (isActive)
            ImGui.PopStyleColor(1);

        if (ImGui.IsItemHovered())
        {
            // Double-click loads the project, or switches to it if it is already active
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                Editor.LoadOrSwitch(project);
            }

            GUI.Tooltip(GetEntryTooltip(project));
        }

        DisplayContextMenu(project, imguiKey);
    }

    private static string GetEntryTooltip(ProjectEntry project)
    {
        var descriptor = project.Descriptor;

        return LOC.Get("PROJECT_List_Entry_TT",
            LOC.Get(descriptor.ProjectType.GetDisplayName()),
            descriptor.ProjectPath,
            descriptor.DataPath);
    }

    private void DisplayContextMenu(ProjectEntry project, string imguiKey)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (!ImGui.BeginPopupContextItem($"##contextMenu_{project.Descriptor.ProjectGUID}_{imguiKey}"))
            return;

        // The orchestrator selects the project once it has loaded. Other active projects stay loaded.
        var isOpen = project.IsActive;

        if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Load")}##loadAction", false, !isOpen && !orchestrator.IsProjectLoading))
        {
            Editor.SelectEntry(project);
            Editor.LoadOrSwitch(project);
        }

        if (ImGui.MenuItem($"{LOC.Get("PROJECT_Header_Switch")}##switchAction", false, isOpen && orchestrator.SelectedProject != project))
        {
            Editor.LoadOrSwitch(project);
        }

        if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Reload")}##reloadAction", false, isOpen && !orchestrator.IsProjectLoading))
        {
            Editor.ReloadAction(project);
        }

        if (ImGui.MenuItem($"{LOC.Get("PROJECT_Configuration_Global_Context_Unload")}##unloadAction", false, isOpen))
        {
            Editor.UnloadAction(project);
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
