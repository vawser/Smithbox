using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The selected project's header card, shown above every tab, and its Overview tab: settings, editors, Mod Engine 3
/// and maintenance.
/// </summary>
public class ProjectOverviewView
{
    public ProjectScreen Editor;

    // Sections the user showed or hid with their header's eye toggle; set from the pending section when another
    // editor deep-links here
    private bool _openSettings = true;
    private bool _openEditors = false;
    private bool _openME3 = true;
    private bool _openMaintenance = false;

    public ProjectOverviewView(ProjectScreen editor)
    {
        Editor = editor;
    }

    #region Header

    /// <summary>
    /// Name, game, load state, paths and unsaved metadata, with the action that fits the load state.
    /// </summary>
    public void DisplayHeader(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;
        var descriptor = project.Descriptor;

        ImGui.BeginChild("projectHeaderCard", Vector2.Zero, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        // Name · Game · Status
        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, descriptor.ProjectName);

        ImGui.SameLine();
        ImGui.TextDisabled($"· {LOC.Get(descriptor.ProjectType.GetDisplayName())} ·");

        ImGui.SameLine();
        var (status, statusColor) = GetStatus(project);
        ImGui.TextColored(statusColor, status);

        // Paths
        DisplayPathLine("headerProjectPath", LOC.Get("PROJECT_Header_Project_Path"), descriptor.ProjectPath);
        DisplayPathLine("headerDataPath", LOC.Get("PROJECT_Header_Data_Path"), descriptor.DataPath);

        // Unsaved alias and enum edits, which are saved from their tabs
        var unsaved = GetUnsavedMetadata(project);

        if (unsaved != null)
            GUI.WrappedYellowText(unsaved);

        // Actions
        DisplayHeaderActions(project);

        ImGui.EndChild();
    }

    private static (string, Vector4) GetStatus(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;

        if (orchestrator.IsProjectLoading && orchestrator.LoadingProject == project)
            return (LOC.Get("PROJECT_Header_Status_Loading"), UI.Current.ImGui_AliasName_Text);

        if (!project.IsActive)
            return (LOC.Get("PROJECT_Header_Status_Not_Loaded"), UI.Current.ImGui_Default_Text_Color);

        if (orchestrator.SelectedProject == project)
            return (LOC.Get("PROJECT_Header_Status_Selected"), UI.Current.ImGui_Benefit_Text_Color);

        return (LOC.Get("PROJECT_Header_Status_Loaded"), UI.Current.ImGui_Benefit_Text_Color);
    }

    private static void DisplayPathLine(string id, string label, string path)
    {
        var exists = !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

        if (!exists)
            ImGui.BeginDisabled();

        if (ImGui.Button($"{Icons.FolderOpen}##{id}_open"))
        {
            ProjectConfigureMenu.OpenDirectory(path);
        }

        if (!exists)
            ImGui.EndDisabled();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(exists ? LOC.Get("PROJECT_Header_Open_Folder_TT") : LOC.Get("PROJECT_Header_Folder_Missing_TT"));
        }

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(label);

        ImGui.SameLine();

        if (exists)
            ImGui.TextUnformatted(path);
        else
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, string.IsNullOrWhiteSpace(path) ? "-" : path);
    }

    private static string GetUnsavedMetadata(ProjectEntry project)
    {
        var parts = new List<string>();

        if (project.UnsavedAliasTypes.Count > 0)
            parts.Add(LOC.Get("PROJECT_Header_Unsaved_Aliases", project.UnsavedAliasTypes.Count));

        if (project.HasUnsavedEnums)
            parts.Add(LOC.Get("PROJECT_Header_Unsaved_Enums"));

        return parts.Count > 0 ? LOC.Get("PROJECT_Header_Unsaved", string.Join(", ", parts)) : null;
    }

    private void DisplayHeaderActions(ProjectEntry project)
    {
        var orchestrator = Smithbox.Orchestrator;
        var loading = orchestrator.IsProjectLoading;

        if (!project.IsActive)
        {
            InlineButton($"{Icons.Play} {LOC.Get("PROJECT_Header_Load")}##headerLoad",
                LOC.Get("PROJECT_Header_Load_TT"), !loading, () => Editor.LoadOrSwitch(project));

            return;
        }

        if (orchestrator.SelectedProject != project)
        {
            InlineButton($"{Icons.Exchange} {LOC.Get("PROJECT_Header_Switch")}##headerSwitch",
                LOC.Get("PROJECT_Header_Switch_TT"), !loading, () => Editor.LoadOrSwitch(project));

            ImGui.SameLine();
        }

        InlineButton($"{Icons.Refresh} {LOC.Get("PROJECT_Configuration_Global_Context_Reload")}##headerReload",
            LOC.Get("PROJECT_Header_Reload_TT"), !loading, () => Editor.ReloadAction(project));

        ImGui.SameLine();

        InlineButton($"{Icons.SignOut} {LOC.Get("PROJECT_Configuration_Global_Context_Unload")}##headerUnload",
            LOC.Get("PROJECT_Header_Unload_TT"), !loading, () => Editor.UnloadAction(project));
    }

    private static void InlineButton(string label, string tooltip, bool enabled, Action action)
    {
        if (!enabled)
            ImGui.BeginDisabled();

        if (ImGui.Button(label))
            action();

        if (!enabled)
            ImGui.EndDisabled();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(tooltip);
    }

    #endregion

    #region Overview

    public void DisplayOverview(ProjectEntry project)
    {
        var form = Editor.ConfigureMenu;

        // Opened from elsewhere, e.g. the Enums tab asking for the Param Editor
        var pending = Editor.PendingSection;
        Editor.PendingSection = ProjectOverviewSection.None;

        if (pending is ProjectOverviewSection.Settings)
            _openSettings = true;

        if (pending is ProjectOverviewSection.Editors)
            _openEditors = true;

        // Settings
        GUI.ConditionalHeader(
            LOC.Get("PROJECT_Overview_Settings"),
            LOC.Get("PROJECT_Overview_Settings_TT"),
            ref _openSettings);

        if (pending is ProjectOverviewSection.Settings)
            ImGui.SetScrollHereY(0f);

        if (_openSettings)
        {
            GUI.Spacer();
            form.DisplayNameField(LOC.Get("PROJECT_Configuration_Header_Project_Name"), false);

            GUI.Spacer();
            form.DisplayReadOnlyGameField();

            GUI.Spacer();
            form.DisplayProjectDirectoryField(LOC.Get("PROJECT_Configuration_Header_Project_Directory"), false);

            GUI.Spacer();
            form.DisplayDataDirectoryField(LOC.Get("PROJECT_Configuration_Header_Data_Directory"));

            GUI.Spacer();
            form.DisplayGroupTagField();
        }

        // Editors
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PROJECT_Overview_Editors"),
            LOC.Get("PROJECT_Overview_Editors_TT"),
            ref _openEditors);

        if (pending is ProjectOverviewSection.Editors)
            ImGui.SetScrollHereY(0f);

        if (_openEditors)
        {
            GUI.WrappedText(LOC.Get("PROJECT_Overview_Editors_Hint"));
            GUI.Spacer();

            form.DisplayEditorToggles();
        }

        // Apply, outside the sections so it stays visible whichever one was edited
        GUI.Spacer();
        DisplayApply(project);

        // Mod Engine 3
        if (ModEngineHandler.IsME3Project(project))
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PROJECT_Overview_ME3"),
                LOC.Get("PROJECT_Overview_ME3_TT"),
                ref _openME3);

            if (_openME3)
            {
                GUI.ConditionalMultiButtonInput("me3Actions",
                    "launchMod",
                    LOC.Get("PROJECT_Configuration_Action_Launch_Mod"),
                    LOC.Get("PROJECT_Configuration_Action_Launch_Mod_TT"),
                    form.LaunchModAction, ModEngineHandler.ME3ProfileExists(project),

                    "createProfile",
                    LOC.Get("PROJECT_Configuration_Action_Create_Mod_Profile"),
                    LOC.Get("PROJECT_Configuration_Action_Create_Mod_Profile_TT"),
                    form.CreateProfileAction, !ModEngineHandler.ME3ProfileExists(project));
            }
        }

        // Maintenance
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PROJECT_Overview_Maintenance"),
            LOC.Get("PROJECT_Overview_Maintenance_TT"),
            ref _openMaintenance);

        if (_openMaintenance)
        {
            GUI.WrappedText(LOC.Get("PROJECT_Overview_Maintenance_Hint"));
            GUI.Spacer();

            GUI.MultiButtonInput("maintenanceActions",
                "clearProjectBackups",
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files_TT"),
                form.DeleteBackupFilesAction,

                "clearComprehensiveBackups",
                LOC.Get("PROJECT_Configuration_Action_Delete_Comprehensive_Backups"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Comprehensive_Backups_TT"),
                form.DeleteComprehensiveBackupsAction);

            // Remove Project, apart from the other actions and in the warning colour
            GUI.Spacer();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

            var remove = GUI.ExplainedButton(
                $"{Icons.Trash} {LOC.Get("PROJECT_Overview_Remove_Project")}##removeProject",
                LOC.Get("PROJECT_Overview_Remove_Project_TT"),
                true);

            ImGui.PopStyleColor(1);

            if (remove)
                form.DeleteProjectAction();
        }
    }

    /// <summary>
    /// The unsaved note and the Update and Revert buttons for the settings and editor toggles.
    /// </summary>
    private void DisplayApply(ProjectEntry project)
    {
        var form = Editor.ConfigureMenu;
        var hasUnsavedChanges = form.HasUnsavedChanges();
        var firstIssue = form.GetFirstIssue();

        if (!hasUnsavedChanges)
        {
            ImGui.TextDisabled(LOC.Get("PROJECT_Overview_No_Changes"));
            return;
        }

        GUI.WrappedYellowText(LOC.Get(project.IsActive
            ? "PROJECT_Overview_Unsaved_Changes_Active"
            : "PROJECT_Overview_Unsaved_Changes"));

        var updateTooltip = firstIssue != null
            ? LOC.Get("PROJECT_Overview_Update_Blocked_TT", firstIssue)
            : LOC.Get("PROJECT_Configuration_Action_Update_Project_TT");

        if (GUI.ExplainedButton(
            $"{Icons.Check} {LOC.Get("PROJECT_Configuration_Action_Update_Project")}##updateProject",
            updateTooltip,
            firstIssue == null))
        {
            form.UpdateProjectAction();
        }

        if (GUI.ExplainedButton(
            $"{Icons.Undo} {LOC.Get("PROJECT_Configuration_Action_Revert_Project")}##revertProject",
            LOC.Get("PROJECT_Configuration_Action_Revert_Project_TT"),
            true))
        {
            form.RevertChangesAction();
        }
    }

    #endregion
}
