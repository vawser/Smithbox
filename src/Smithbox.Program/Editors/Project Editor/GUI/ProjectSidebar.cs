using Hexa.NET.ImGui;
using StudioCore.Editors;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The Project Editor's list of projects, in the Tools window's style: an Open group first, then one group per
/// game (its projects grouped by group tag). Double-clicking a group's title hides or shows its projects.
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

        ImGui.BeginChild("projectSidebarList", Vector2.Zero, ImGuiChildFlags.Borders);

        if (projects.Count == 0)
        {
            if (orchestrator.ExistingProjectsRead)
                ImGui.TextDisabled(LOC.Get("PROJECT_Sidebar_No_Projects"));
        }
        else
        {
            DisplayOpenGroup(projects);
            DisplayGameGroups(projects);
        }

        ImGui.EndChild();
    }

    // The groups whose projects are hidden, toggled by double-clicking the group's title. Kept in the config so it
    // survives a restart; read through CFG.Current each time, as reverting the preferences replaces it.
    private static List<string> CollapsedGroups =>
        CFG.Current.Interface.Interface_Project_List_Collapsed_Groups ??= new List<string>();

    private const string OpenGroupKey = "open";

    // Spacing goes between groups, not above the first
    private bool _firstGroup;

    /// <summary>
    /// A group's title, as the Tools window draws its categories. Returns true when its projects should be drawn.
    /// </summary>
    private bool DisplayGroupHeader(string key, string title, int count)
    {
        if (!_firstGroup)
            ImGui.Spacing();

        _firstGroup = false;

        return ToolSections.CategoryHeader(key, title, count, CollapsedGroups,
            LOC.Get("PROJECT_Sidebar_Group_TT"));
    }

    /// <summary>
    /// The loaded projects. Projects loaded only for another project's comparison data aren't open projects.
    /// </summary>
    private void DisplayOpenGroup(List<ProjectEntry> projects)
    {
        _firstGroup = true;

        var openProjects = projects.Where(e => e.IsActive && IsMatch(e)).ToList();

        if (openProjects.Count == 0)
            return;

        if (!DisplayGroupHeader(OpenGroupKey, LOC.Get("PROJECT_Sidebar_Open_Header"), openProjects.Count))
            return;

        ImGui.Indent();

        foreach (var project in openProjects)
        {
            DisplayEntry(project, "open", true);
        }

        ImGui.Unindent();
    }

    /// <summary>
    /// One group per game in the project type order; within a game, projects are grouped by group tag. Games
    /// with no project matching the filter are left out.
    /// </summary>
    private void DisplayGameGroups(List<ProjectEntry> projects)
    {
        foreach (var projectType in ProjectTypeOrder.Order)
        {
            var gameProjects = projects
                .Where(e => e.Descriptor.ProjectType == projectType && IsMatch(e))
                .OrderBy(e => e.Descriptor.ProjectName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (gameProjects.Count == 0)
                continue;

            var title = LOC.Get(projectType.GetDisplayName());

            if (!DisplayGroupHeader($"game_{projectType}", title, gameProjects.Count))
                continue;

            ImGui.Indent();

            DisplayGameProjects(gameProjects, projectType);

            ImGui.Unindent();
        }
    }

    private void DisplayGameProjects(List<ProjectEntry> gameProjects, ProjectType projectType)
    {
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
