using Hexa.NET.ImGui;
using Octokit;
using Silk.NET.SDL;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

public class ProjectConfigureMenu
{
    public ProjectEditorMode EditorMode = ProjectEditorMode.Create;

    public ProjectScreen Editor;

    public ProjectEntry Project = new();
    public ProjectDescriptor Descriptor = new();

    private string SteamExecutable_DS1 = "";
    private string SteamExecutable_DS1R = "";
    private string SteamExecutable_DS2 = "";
    private string SteamExecutable_DS2S = "";
    private string SteamExecutable_DS3 = "";
    private string SteamExecutable_SDT = "";
    private string SteamExecutable_ER = "";
    private string SteamExecutable_AC6 = "";
    private string SteamExecutable_NR = "";
    private bool SteamPathsInitialized = false;

    public ProjectConfigureMenu(ProjectScreen editor)
    {
        Editor = editor;

        SetupForCreation();
    }

    public void SetupForCreation()
    {
        EditorMode = ProjectEditorMode.Create;

        Descriptor = new ProjectDescriptor();
        Project = new ProjectEntry();

        // Undefined, so the user picks the game, which also fills in the data directory from Steam
        Descriptor.ProjectType = ProjectType.Undefined;

        Descriptor.EnableAllEditors();
    }

    /// <summary>
    /// Whether the form holds edits not yet applied to the project with Update.
    /// </summary>
    public bool HasUnsavedChanges()
    {
        return EditorMode is ProjectEditorMode.Edit
            && Project?.Descriptor != null
            && !Project.Descriptor.ConfigurationEquals(Descriptor);
    }

    /// <summary>
    /// Applies the form's edits to the project and writes its project file, without reloading it. For the unsaved
    /// changes prompt, before Smithbox closes or the project is unloaded, so the settings take effect on its next
    /// load. Settings the Update button wouldn't accept are left unapplied, so the prompt lists them again.
    /// </summary>
    public void ApplyWithoutReload()
    {
        if (!HasUnsavedChanges() || !AllowCreation())
            return;

        Project.Descriptor.ApplyConfiguration(Descriptor);
        Smithbox.Orchestrator.SaveProject(Project);

        SetupForEdit(Project);
    }

    public void RevertChangesAction()
    {
        SetupForEdit(Project);
    }

    public void SetupForEdit(ProjectEntry project)
    {
        EditorMode = ProjectEditorMode.Edit;

        if (project != null)
        {
            Project = project;
            Descriptor = project.Descriptor.Clone();
        }
    }

    public void ResetSetup()
    {
        Descriptor = new ProjectDescriptor();
        Project = new ProjectEntry();
    }

    public void Display()
    {
        if (!SteamPathsInitialized)
        {
            FindSteamExecutables();
            SteamPathsInitialized = true;
        }

        DisplayMenu();
    }

    public void DisplayMenu()
    {
        var columnCount = 2;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("projectCreateTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##ProjectSettingsCol", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##ProjectEditorTogglesCol", ImGuiTableColumnFlags.WidthStretch, 0.25f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ProjectSettingsArea", new Vector2(0, 0), ImGuiChildFlags.Borders, windowFlags);

            DisplayProjectSettings();
            DisplayProjectActions();

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ProjectEditorToggles", new Vector2(0, 0), ImGuiChildFlags.Borders, windowFlags);

            DisplayProjectEditorToggles();

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }

    public void DisplayProjectActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECT_Configuration_Header_Actions"), "");

        if (!AllowCreation())
        {
            if(EditorMode is ProjectEditorMode.Create)
                GUI.WrappedText(LOC.Get("PROJECT_Configuration_Project_Creation_Block_Create"));

            if (EditorMode is ProjectEditorMode.Edit)
                GUI.WrappedText(LOC.Get("PROJECT_Configuration_Project_Creation_Block_Edit"));

            if (string.IsNullOrWhiteSpace(Descriptor.ProjectName))
                GUI.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Name"));

            if (!Directory.Exists(Descriptor.ProjectPath))
                GUI.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Path"));

            if (!Directory.Exists(Descriptor.DataPath))
                GUI.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Data_Path"));

            if (Descriptor.ProjectType is ProjectType.Undefined)
                GUI.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Type"));

            GUI.Spacer();
        }

        // Likely mistakes that don't stop the project from working
        var warnings = GetConfigurationWarnings();

        if (warnings.Count > 0)
        {
            foreach (var warning in warnings)
            {
                GUI.WrappedYellowText(warning);
            }

            GUI.Spacer();
        }

        var hasUnsavedChanges = HasUnsavedChanges();

        if (hasUnsavedChanges)
        {
            GUI.WrappedYellowText(LOC.Get("PROJECT_Configuration_Unsaved_Changes"));
            GUI.Spacer();
        }

        if (EditorMode is ProjectEditorMode.Create)
        {
            GUI.ConditionalMultiButtonInput("projectCreateActions",
                "createProject",
                LOC.Get("PROJECT_Configuration_Action_Create_Project"),
                LOC.Get("PROJECT_Configuration_Action_Create_Project_TT"),
                CreateProjectAction, AllowCreation());
        }
        else if (EditorMode is ProjectEditorMode.Edit)
        {
            GUI.ConditionalMultiButtonInput("projectEditActions",
                "editProject",
                LOC.Get("PROJECT_Configuration_Action_Update_Project"),
                LOC.Get("PROJECT_Configuration_Action_Update_Project_TT"),
                UpdateProjectAction, AllowCreation() && hasUnsavedChanges,

                "revertProject",
                LOC.Get("PROJECT_Configuration_Action_Revert_Project"),
                LOC.Get("PROJECT_Configuration_Action_Revert_Project_TT"),
                RevertChangesAction, hasUnsavedChanges,

                "deleteProject",
                LOC.Get("PROJECT_Configuration_Action_Delete_Project"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Project_TT"), 
                DeleteProjectAction, true,

                "clearProjectBackups",
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files_TT"),
                DeleteBackupFilesAction, true,

                "clearComprehensiveBackups",
                LOC.Get("PROJECT_Configuration_Action_Delete_Comprehensive_Backups"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Comprehensive_Backups_TT"),
                DeleteComprehensiveBackupsAction, true);
        }

        if (ModEngineHandler.IsME3Project(Project))
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("PROJECT_Configuration_Header_ME3"), "");

            GUI.ConditionalMultiButtonInput("me3Actions",
                "launchMod",
                LOC.Get("PROJECT_Configuration_Action_Launch_Mod"),
                LOC.Get("PROJECT_Configuration_Action_Launch_Mod_TT"), 
                LaunchModAction, ModEngineHandler.ME3ProfileExists(Project),

                "createProfile",
                LOC.Get("PROJECT_Configuration_Action_Create_Mod_Profile"),
                LOC.Get("PROJECT_Configuration_Action_Create_Mod_Profile_TT"), 
                CreateProfileAction, !ModEngineHandler.ME3ProfileExists(Project));
        }
    }

    public void CreateProjectAction()
    {
        var newProject = Smithbox.Orchestrator.CreateProject(Descriptor);

        // Switch to editing the new project, so pressing Create again doesn't register a duplicate
        Editor.SelectedAvaliableEntry = newProject;
        SetupForEdit(newProject);
    }

    public void UpdateProjectAction()
    {
        var project = Project;
        var descriptor = Descriptor;

        // An active project is reloaded with the new settings, which would lose its unsaved changes
        if (!project.IsActive)
        {
            Smithbox.Orchestrator.UpdateProject(project, descriptor);
            return;
        }

        Smithbox.Orchestrator.RequestProjectAction(project, UnsavedChangesAction.Reload, () =>
        {
            Smithbox.Orchestrator.UpdateProject(project, descriptor);
        });
    }

    public void DeleteProjectAction()
    {
        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PROJECT_Configuration_Dialog_Delete_Project", Project.Descriptor.ProjectName),
            LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is DialogResult.OK)
        {
            var project = Project;

            Smithbox.Orchestrator.RequestProjectAction(project, UnsavedChangesAction.Remove, () =>
            {
                ProjectUtils.DeleteProject(project);
            });
        }
    }

    public void LaunchModAction()
    {
        ModEngineHandler.LaunchME3Mod(Project);
    }

    public void CreateProfileAction()
    {
        if (CFG.Current.Project_ME3_Profile_Directory == "")
        {
            var profilePath = "";
            var result =
                PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PROJECT_Configuration_Dialog_Select_ME3_Profile_Dir"), out profilePath);

            if (!result)
                return;

            CFG.Current.Project_ME3_Profile_Directory = profilePath;
        }

        ModEngineHandler.CreateME3Profile(Project);
    }

    public void DeleteBackupFilesAction()
    {
        var root = Project.Descriptor.ProjectPath;

        if (!Directory.Exists(root))
            return;

        var filesToDelete = ProjectUtils.GetBackupFiles(root);

        // .bak files are only written into the game directory, where they are the unmodified originals. When the
        // project directory is (or is inside) the game directory, they are the only copy, so they are kept.
        var keepOriginals = ProjectUtils.IsSameOrInsideDirectory(root, Project.Descriptor.DataPath);

        if (keepOriginals)
        {
            filesToDelete = filesToDelete
                .Where(e => !Path.GetExtension(e).Equals(".bak", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (filesToDelete.Count == 0)
        {
            PlatformUtils.Instance.MessageBox(
                LOC.Get("PROJECT_Configuration_Dialog_No_Backup_Files"),
                LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        var bakCount = filesToDelete.Count(e => Path.GetExtension(e).Equals(".bak", StringComparison.OrdinalIgnoreCase));
        var prevCount = filesToDelete.Count - bakCount;
        var totalSize = filesToDelete.Sum(GetFileSize);

        var fileList = "";

        int i = 0;

        foreach (var entry in filesToDelete)
        {
            fileList = fileList + $"\n{Path.GetRelativePath(root, entry)}";

            i++;

            if (i > 25)
            {
                fileList = fileList + $"\n....";
                break;
            }
        }

        if (keepOriginals)
        {
            fileList = fileList + LOC.Get("PROJECT_Configuration_Dialog_Backup_Bak_Excluded");
        }

        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PROJECT_Configuration_Dialog_Delete_Backup_Files_Summary", bakCount, prevCount, FormatSize(totalSize), fileList),
            LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is DialogResult.OK)
        {
            ProjectUtils.DeleteFiles(filesToDelete);
        }
    }

    /// <summary>
    /// The timestamped folders the comprehensive backups write to, in the project's .backup folder.
    /// </summary>
    private static List<string> GetComprehensiveBackupFolders(string root)
    {
        var backupRoot = Path.Combine(root, ".backup");

        if (!Directory.Exists(backupRoot))
            return new();

        return Directory.EnumerateDirectories(backupRoot).ToList();
    }

    public void DeleteComprehensiveBackupsAction()
    {
        var root = Project.Descriptor.ProjectPath;

        if (!Directory.Exists(root))
            return;

        var folders = GetComprehensiveBackupFolders(root);

        if (folders.Count == 0)
        {
            PlatformUtils.Instance.MessageBox(
                LOC.Get("PROJECT_Configuration_Dialog_No_Comprehensive_Backups"),
                LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        var totalSize = folders
            .SelectMany(e => Directory.EnumerateFiles(e, "*", SearchOption.AllDirectories))
            .Sum(GetFileSize);

        var ordered = folders.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList();

        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PROJECT_Configuration_Dialog_Delete_Comprehensive_Backups",
                folders.Count, FormatSize(totalSize),
                Path.GetFileName(ordered.First()), Path.GetFileName(ordered.Last())),
            LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is not DialogResult.OK)
            return;

        foreach (var folder in folders)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch (Exception ex)
            {
                Smithbox.Log(this, LOC.Get("PROJECT_Util_Failed_To_Delete", $"{folder}: {ex.Message}"));
            }
        }
    }

    private static long GetFileSize(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];

        double size = bytes;
        var unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }

    private void DisplayProjectEditorToggles()
    {
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Available_Editors"), 
            LOC.Get("PROJECT_Configuration_Header_Available_Editors_TT"));

        GUI.MultiButtonInput("editorToggleActions",
            "enableAllEditors",
            LOC.Get("PROJECT_Configuration_Action_Enable_All_Editors"),
            LOC.Get("PROJECT_Configuration_Action_Enable_All_Editors_TT"),
            () => SetAllEditors(true),

            "disableAllEditors",
            LOC.Get("PROJECT_Configuration_Action_Disable_All_Editors"),
            LOC.Get("PROJECT_Configuration_Action_Disable_All_Editors_TT"),
            () => SetAllEditors(false));

        GUI.Spacer();

        var type = Descriptor.ProjectType;

        EditorToggle("PROJECT_Configuration_Checkbox_Visual_Map_Editor",
            ref Descriptor.EnableMapEditor, EditorConditions.SupportsMapEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Model_Editor",
            ref Descriptor.EnableModelEditor, EditorConditions.SupportsModelEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_TimeAct_Editor",
            ref Descriptor.EnableTimeActEditor, EditorConditions.SupportsTimeActEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Param_Editor",
            ref Descriptor.EnableParamEditor, EditorConditions.SupportsParamEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Text_Editor",
            ref Descriptor.EnableTextEditor, EditorConditions.SupportsTextEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Havok_Editor",
            ref Descriptor.EnableHavokEditor, EditorConditions.SupportsHavokEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Gparam_Editor",
            ref Descriptor.EnableGparamEditor, EditorConditions.SupportsGraphicsParamEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Material_Editor",
            ref Descriptor.EnableMaterialEditor, EditorConditions.SupportsMaterialEditor(type));

        EditorToggle("PROJECT_Configuration_Checkbox_Texture_Viewer",
            ref Descriptor.EnableTextureViewer, EditorConditions.SupportsTextureViewer(type));

        EditorToggle("PROJECT_Configuration_Checkbox_File_Browser",
            ref Descriptor.EnableFileBrowser, EditorConditions.SupportsFileBrowser(type));

        // Material Data
        EditorToggle("PROJECT_Configuration_Checkbox_Material_Data",
            ref Descriptor.EnableExternalMaterialData, true);
    }

    /// <summary>
    /// An editor's checkbox. Editors the game doesn't support are shown disabled, with the reason in the tooltip,
    /// rather than hidden, so it's clear why they are missing.
    /// </summary>
    private void EditorToggle(string locKey, ref bool value, bool isSupported)
    {
        if (!isSupported)
        {
            var unsupported = false;

            ImGui.BeginDisabled();
            ImGui.Checkbox($"{LOC.Get(locKey)}##{locKey}", ref unsupported);
            ImGui.EndDisabled();

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip(LOC.Get("PROJECT_Configuration_Checkbox_Unsupported_TT"));
            }

            return;
        }

        ImGui.Checkbox($"{LOC.Get(locKey)}##{locKey}", ref value);

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private void SetAllEditors(bool enabled)
    {
        Descriptor.EnableMapEditor = enabled;
        Descriptor.EnableModelEditor = enabled;
        Descriptor.EnableTimeActEditor = enabled;
        Descriptor.EnableParamEditor = enabled;
        Descriptor.EnableTextEditor = enabled;
        Descriptor.EnableHavokEditor = enabled;
        Descriptor.EnableGparamEditor = enabled;
        Descriptor.EnableMaterialEditor = enabled;
        Descriptor.EnableTextureViewer = enabled;
        Descriptor.EnableFileBrowser = enabled;
        Descriptor.EnableAnimEditor = enabled;
    }

    public void DisplayProjectSettings()
    {
        GUI.MultiButtonInput("globalProjectActions",
            "createNewProject",
            LOC.Get("PROJECT_Configuration_Global_Action_Create_Project"),
            LOC.Get("PROJECT_Configuration_Global_Action_Create_Project_TT"),
            SetupForCreation,

            "createProjectFromJson",
            LOC.Get("PROJECT_Configuration_Global_Action_Create_Project_From_JSON"),
            LOC.Get("PROJECT_Configuration_Global_Action_Create_Project_From_JSON_TT"),
            CreateProjectFromJsonAction);

        // Project Name
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Name"),
            LOC.Get("PROJECT_Configuration_Header_Project_Name_TT"));

        GUI.HintTextInput(
            "ProjectNameInput", 
            ref Descriptor.ProjectName,
            LOC.Get("PROJECT_Configuration_Project_Name_Input_Hint"));

        // Project Type
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Type"),
            LOC.Get("PROJECT_Configuration_Header_Project_Type_TT"));

        GUI.SetInputWidth();

        var previewName = LOC.Get(Descriptor.ProjectType.GetDisplayName());

        if (ImGui.BeginCombo("##projectTypePicker", previewName))
        {
            // Make the combo-box dropdown bigger so there is no need to scroll
            ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

            foreach (var entry in ProjectTypeOrder.Order)
            {
                var type = (ProjectType)entry;

                var displayName = LOC.Get(type.GetDisplayName());

                if (ImGui.Selectable(displayName))
                {
                    Descriptor.ProjectType = type;

                    // Only fill in an empty data directory, so a path the user entered isn't replaced
                    var steamPath = GetSteamDataPath(type);

                    if (string.IsNullOrWhiteSpace(Descriptor.DataPath) && !string.IsNullOrEmpty(steamPath))
                    {
                        Descriptor.DataPath = steamPath;
                    }
                }
            }
            ImGui.EndCombo();
        }

        // Project Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Header_Project_Directory_TT"));

        GUI.HintTextInput(
            "ProjectDirPath", 
            ref Descriptor.ProjectPath,
            LOC.Get("PROJECT_Configuration_Project_Directory_Path_Hint"));

        GUI.MultiButtonInput("projectDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory_TT"),
            SelectProjectDirectory,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory_TT"), 
            OpenProjectDirectory);

        // Data Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Data_Directory"), GetDataDirectoryTooltip());

        GUI.HintTextInput("DataDirPath", 
            ref Descriptor.DataPath,
            LOC.Get("PROJECT_Configuration_Data_Directory_Path_Hint"));

        var steamDataPath = GetSteamDataPath(Descriptor.ProjectType);

        GUI.ConditionalMultiButtonInput("dataDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory_TT"),
            SelectDataDirectory, true,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory_TT"),
            OpenDataDirectory, true,

            "useSteamDir",
            LOC.Get("PROJECT_Configuration_Action_Use_Steam_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Use_Steam_Data_Directory_TT", steamDataPath ?? ""),
            UseSteamDataPathAction,
            !string.IsNullOrEmpty(steamDataPath) && steamDataPath != Descriptor.DataPath);

        // Group Tag
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag"),
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag_TT"));

        GUI.HintTextInput(
            "FolderTagInput", 
            ref Descriptor.FolderTag,
            LOC.Get("PROJECT_Configuration_Group_Tag_Hint"));
    }

    public void SelectProjectDirectory()
    {
        if (CFG.Current.Project_Default_Mod_Directory != "")
        {
            var newProjectPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Project_Directory"), 
                out newProjectPath, CFG.Current.Project_Default_Mod_Directory);

            if (result)
            {
                Descriptor.ProjectPath = newProjectPath;
            }
        }
        else
        {
            var newProjectPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Project_Directory"), out newProjectPath);

            if (result)
            {
                Descriptor.ProjectPath = newProjectPath;
            }
        }
    }

    public void OpenProjectDirectory()
    {
        OpenDirectory(Descriptor.ProjectPath);
    }

    /// <summary>
    /// Opens a directory in the platform's file manager. Does nothing for an empty or missing path, rather than
    /// opening a default location.
    /// </summary>
    private void OpenDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        try
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"Failed to open the directory \"{path}\".", e);
        }
    }

    public void SelectDataDirectory()
    {
        if (CFG.Current.Project_Default_Data_Directory != "")
        {
            var newDataPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Game_Directory"), out newDataPath, CFG.Current.Project_Default_Data_Directory);

            if (result)
            {
                Descriptor.DataPath = newDataPath;
            }
        }
        else
        {
            var newDataPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Game_Directory"), out newDataPath);

            if (result)
            {
                Descriptor.DataPath = newDataPath;
            }
        }
    }
    public void OpenDataDirectory()
    {
        OpenDirectory(Descriptor.DataPath);
    }

    private bool AllowCreation()
    {
        bool isAllowed = true;

        if (string.IsNullOrWhiteSpace(Descriptor.ProjectName))
            isAllowed = false;

        if (!Directory.Exists(Descriptor.ProjectPath))
            isAllowed = false;

        if (!Directory.Exists(Descriptor.DataPath))
            isAllowed = false;

        if (Descriptor.ProjectType is ProjectType.Undefined)
            isAllowed = false;

        return isAllowed;
    }

    /// <summary>
    /// The executable each game's data directory holds, relative to it. Games without a PC release are absent.
    /// </summary>
    private static readonly Dictionary<ProjectType, string> GameExecutables = new()
    {
        { ProjectType.DS1, "DARKSOULS.exe" },
        { ProjectType.DS1R, "DarkSoulsRemastered.exe" },
        { ProjectType.DS2, "DarkSoulsII.exe" },
        { ProjectType.DS2S, "DarkSoulsII.exe" },
        { ProjectType.DS3, "DarkSoulsIII.exe" },
        { ProjectType.SDT, "sekiro.exe" },
        { ProjectType.ER, "eldenring.exe" },
        { ProjectType.AC6, "armoredcore6.exe" },
        { ProjectType.NR, "nightreign.exe" },
    };

    // The warnings touch the disk, so they are only rebuilt when the inputs change
    private string _warningsKey;
    private List<string> _warnings = new();

    /// <summary>
    /// Warnings for settings that are allowed but are probably mistakes.
    /// </summary>
    private List<string> GetConfigurationWarnings()
    {
        var orchestrator = Smithbox.Orchestrator;

        var key = $"{Descriptor.ProjectPath}|{Descriptor.DataPath}|{Descriptor.ProjectType}|{Project?.Descriptor?.ProjectGUID}|{orchestrator.Projects.Count}";

        if (key == _warningsKey)
            return _warnings;

        _warningsKey = key;
        _warnings = new();

        var projectPath = Descriptor.ProjectPath;
        var dataPath = Descriptor.DataPath;

        if (Directory.Exists(projectPath) && Directory.Exists(dataPath))
        {
            if (ProjectUtils.IsSameOrInsideDirectory(projectPath, dataPath))
            {
                _warnings.Add(LOC.Get("PROJECT_Configuration_Warning_Project_In_Data_Dir"));
            }

            var duplicate = orchestrator.Projects.FirstOrDefault(e =>
                e != Project &&
                e.Descriptor.ProjectGUID != Project?.Descriptor?.ProjectGUID &&
                PathsEqual(e.Descriptor.ProjectPath, projectPath));

            if (duplicate != null)
            {
                _warnings.Add(LOC.Get("PROJECT_Configuration_Warning_Duplicate_Project_Path", duplicate.Descriptor.ProjectName));
            }
        }

        if (Directory.Exists(dataPath) && GameExecutables.TryGetValue(Descriptor.ProjectType, out var exe))
        {
            if (!File.Exists(Path.Combine(dataPath, exe)))
            {
                _warnings.Add(LOC.Get("PROJECT_Configuration_Warning_Missing_Game_Executable", exe));
            }
        }

        return _warnings;
    }

    private static bool PathsEqual(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            return false;

        try
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(a))
                .Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(b)), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// The game's Steam install directory, if one was found.
    /// </summary>
    private string GetSteamDataPath(ProjectType type)
    {
        return type switch
        {
            ProjectType.DS1 => SteamExecutable_DS1,
            ProjectType.DS1R => SteamExecutable_DS1R,
            ProjectType.DS2 => SteamExecutable_DS2,
            ProjectType.DS2S => SteamExecutable_DS2S,
            ProjectType.DS3 => SteamExecutable_DS3,
            ProjectType.SDT => SteamExecutable_SDT,
            ProjectType.ER => SteamExecutable_ER,
            ProjectType.AC6 => SteamExecutable_AC6,
            ProjectType.NR => SteamExecutable_NR,
            _ => null
        };
    }

    public void UseSteamDataPathAction()
    {
        var steamPath = GetSteamDataPath(Descriptor.ProjectType);

        if (!string.IsNullOrEmpty(steamPath))
        {
            Descriptor.DataPath = steamPath;
        }
    }

    public void FindSteamExecutables()
    {
        SteamExecutable_DS1 = SteamGameLocator.FindGameExecutable(211420, "DATA\\DARKSOULS.exe");
        SteamExecutable_DS1R = SteamGameLocator.FindGameExecutable(570940, "DarkSoulsRemastered.exe");
        SteamExecutable_DS2 = SteamGameLocator.FindGameExecutable(236430, "Game\\DarkSoulsII.exe");
        SteamExecutable_DS2S = SteamGameLocator.FindGameExecutable(335300, "Game\\DarkSoulsII.exe");
        SteamExecutable_DS3 = SteamGameLocator.FindGameExecutable(374320, "Game\\DarkSoulsIII.exe");
        SteamExecutable_SDT = SteamGameLocator.FindGameExecutable(814380, "sekiro.exe");
        SteamExecutable_ER = SteamGameLocator.FindGameExecutable(1245620, "Game\\eldenring.exe");
        SteamExecutable_AC6 = SteamGameLocator.FindGameExecutable(1888160, "Game\\armoredcore6.exe");
        SteamExecutable_NR = SteamGameLocator.FindGameExecutable(2622380, "Game\\nightreign.exe");

        // If we find the PTDE install, auto-set the PTDE path for DS1R. FindGameExecutable already returns the
        // executable's directory (DATA), which is where the PTDE files are read from.
        if(!string.IsNullOrEmpty(SteamExecutable_DS1) && CFG.Current.PTDE_Data_Path == "")
        {
            var dir = Path.GetFullPath(SteamExecutable_DS1);
            if(Directory.Exists(dir))
            {
                CFG.Current.PTDE_Data_Path = dir;
            }
        }
    }

    public string GetDataDirectoryTooltip()
    {
        var tooltip = LOC.Get("PROJECT_Configuration_Header_Data_Directory_TT_Base");
        if (Descriptor.ProjectType is ProjectType.DES)
        {
            tooltip = $"{tooltip}{LOC.Get("PROJECT_Configuration_Header_Data_Directory_TT_DES")}";
        }
        else if (Descriptor.ProjectType is ProjectType.BB)
        {
            tooltip = $"{tooltip}{LOC.Get("PROJECT_Configuration_Header_Data_Directory_TT_BB")}";
        }
        else
        {
            tooltip = $"{tooltip}{LOC.Get("PROJECT_Configuration_Header_Data_Directory_TT_Default")}";
        }

        return tooltip;
    }

    public void CreateProjectFromJsonAction()
    {
        var orchestrator = Smithbox.Orchestrator;

        var projectJsonPath = "";
        var result = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PROJECT_Configuration_Action_Select_Project_JSON"), out projectJsonPath);

        if (result)
        {
            orchestrator.CreateProjectFromLegacyJson(projectJsonPath);
        }
    }
}

public static class ProjectTypeOrder
{
    // Used so the project type combo box has a specific order
    public static List<ProjectType> Order = new()
    {
        ProjectType.DES,
        ProjectType.DS1,
        ProjectType.DS1R,
        ProjectType.DS2,
        ProjectType.DS2S,
        ProjectType.BB,
        ProjectType.DS3,
        ProjectType.SDT,
        ProjectType.ER,
        ProjectType.NR,
        ProjectType.AC6,
        ProjectType.DB_NT,
        ProjectType.DB
    };

}
public enum ProjectEditorMode
{
    Create,
    Edit
}