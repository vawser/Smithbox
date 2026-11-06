using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The Project Editor's list of projects: the open ones first, then every project grouped by game and group tag.
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

        GUI.Spacer();

        EditorFilters.DisplayFramedListFilter("projectSidebarFilter", ref Filter, ref ExactFilter);

        ImGui.BeginChild("projectSidebarList", Vector2.Zero, ImGuiChildFlags.Borders);

        var projects = orchestrator.Projects.ToList();

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

    /// <summary>
    /// The loaded projects. Projects loaded only for another project's comparison data aren't open projects.
    /// </summary>
    private void DisplayOpenGroup(List<ProjectEntry> projects)
    {
        var open = projects.Where(e => e.IsActive && IsMatch(e)).ToList();

        if (open.Count == 0)
            return;

        if (ImGui.CollapsingHeader($"{LOC.Get("PROJECT_Sidebar_Open_Header")}##sidebarOpenGroup", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var project in open)
            {
                DisplayEntry(project, "open", true);
            }
        }
    }

    /// <summary>
    /// Every project, one group per game in the project type order, then by group tag.
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

            var gameName = LOC.Get(projectType.GetDisplayName());

            if (!ImGui.CollapsingHeader($"{gameName}##sidebarGame_{projectType}", ImGuiTreeNodeFlags.DefaultOpen))
                continue;

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
