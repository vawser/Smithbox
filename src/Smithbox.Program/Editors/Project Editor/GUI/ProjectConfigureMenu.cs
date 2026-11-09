using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The project settings form: the descriptor being created or edited, the checks on it, the actions on the project,
/// and the input widgets the Create Project view and the Project Overview are built from.
/// </summary>
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

    #region Setup

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
    /// Starts a new project for a game, with its Steam install as the data directory if one was found.
    /// </summary>
    public void SetupForCreation(ProjectType type)
    {
        SetupForCreation();

        if (type is ProjectType.Undefined)
            return;

        Descriptor.ProjectType = type;

        var steamPath = GetSteamDataPath(type);

        if (!string.IsNullOrEmpty(steamPath))
            Descriptor.DataPath = steamPath;
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

    #endregion

    #region Steam

    /// <summary>
    /// Looks up the Steam installs once, the first time they are needed.
    /// </summary>
    public void EnsureSteamPaths()
    {
        if (SteamPathsInitialized)
            return;

        FindSteamExecutables();
        SteamPathsInitialized = true;
    }

    /// <summary>
    /// The games Steam has installed, in the project type order, with their data directories.
    /// </summary>
    public List<(ProjectType Type, string Path)> GetDetectedGames()
    {
        EnsureSteamPaths();

        var result = new List<(ProjectType, string)>();

        foreach (var type in ProjectTypeOrder.Order)
        {
            var path = GetSteamDataPath(type);

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                result.Add((type, path));
        }

        return result;
    }

    /// <summary>
    /// The game's Steam install directory, if one was found.
    /// </summary>
    private string GetSteamDataPath(ProjectType type)
    {
        EnsureSteamPaths();

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

    private void FindSteamExecutables()
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
    }

    #endregion

    #region Checks

    /// <summary>
    /// How a form field stands, shown as an icon on its header.
    /// </summary>
    public enum FieldStatus
    {
        Valid,
        Warning,
        Invalid
    }

    /// <summary>
    /// The name the project gets: the one entered, or else the project directory's folder name.
    /// </summary>
    public string GetEffectiveName()
    {
        var name = Descriptor.ProjectName?.Trim() ?? "";

        if (name != "")
            return name;

        if (!string.IsNullOrWhiteSpace(Descriptor.ProjectPath) && Directory.Exists(Descriptor.ProjectPath))
        {
            return Path.GetFileName(Path.TrimEndingDirectorySeparator(Descriptor.ProjectPath)) ?? "";
        }

        return "";
    }

    public string GetTypeIssue()
    {
        return Descriptor.ProjectType is ProjectType.Undefined
            ? LOC.Get("PROJECT_Field_Type_Missing")
            : null;
    }

    public string GetDataPathIssue()
    {
        if (string.IsNullOrWhiteSpace(Descriptor.DataPath))
            return LOC.Get("PROJECT_Field_Data_Path_Missing");

        if (!Directory.Exists(Descriptor.DataPath))
            return LOC.Get("PROJECT_Field_Data_Path_Not_Found");

        return null;
    }

    public string GetProjectPathIssue()
    {
        if (string.IsNullOrWhiteSpace(Descriptor.ProjectPath))
            return LOC.Get("PROJECT_Field_Project_Path_Missing");

        if (!Directory.Exists(Descriptor.ProjectPath))
            return LOC.Get("PROJECT_Field_Project_Path_Not_Found");

        return null;
    }

    public string GetNameIssue()
    {
        // In Edit mode the name is required; when creating, the folder name stands in for an empty one
        if (EditorMode is ProjectEditorMode.Edit)
        {
            return string.IsNullOrWhiteSpace(Descriptor.ProjectName)
                ? LOC.Get("PROJECT_Field_Name_Missing_Edit")
                : null;
        }

        return GetEffectiveName() == "" ? LOC.Get("PROJECT_Field_Name_Missing") : null;
    }

    /// <summary>
    /// The first problem that stops the form being applied, or null if there is none.
    /// </summary>
    public string GetFirstIssue()
    {
        return GetTypeIssue() ?? GetDataPathIssue() ?? GetNameIssue() ?? GetProjectPathIssue();
    }

    public bool AllowCreation()
    {
        return GetFirstIssue() == null;
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
    private List<string> _dataPathWarnings = new();
    private List<string> _projectPathWarnings = new();

    /// <summary>
    /// Rebuilds the warnings for settings that are allowed but are probably mistakes, if their inputs changed.
    /// </summary>
    private void UpdateWarnings()
    {
        var orchestrator = Smithbox.Orchestrator;

        var key = $"{Descriptor.ProjectPath}|{Descriptor.DataPath}|{Descriptor.ProjectType}|{Project?.Descriptor?.ProjectGUID}|{orchestrator.Projects.Count}";

        if (key == _warningsKey)
            return;

        _warningsKey = key;
        _dataPathWarnings = new();
        _projectPathWarnings = new();

        var projectPath = Descriptor.ProjectPath;
        var dataPath = Descriptor.DataPath;

        if (Directory.Exists(projectPath) && Directory.Exists(dataPath))
        {
            if (ProjectUtils.IsSameOrInsideDirectory(projectPath, dataPath))
            {
                _projectPathWarnings.Add(LOC.Get("PROJECT_Configuration_Warning_Project_In_Data_Dir"));
            }

            var duplicate = orchestrator.Projects.FirstOrDefault(e =>
                e != Project &&
                e.Descriptor.ProjectGUID != Project?.Descriptor?.ProjectGUID &&
                PathsEqual(e.Descriptor.ProjectPath, projectPath));

            if (duplicate != null)
            {
                _projectPathWarnings.Add(LOC.Get("PROJECT_Configuration_Warning_Duplicate_Project_Path", duplicate.Descriptor.ProjectName));
            }
        }

        if (Directory.Exists(dataPath) && GameExecutables.TryGetValue(Descriptor.ProjectType, out var exe))
        {
            if (!File.Exists(Path.Combine(dataPath, exe)))
            {
                _dataPathWarnings.Add(LOC.Get("PROJECT_Configuration_Warning_Missing_Game_Executable", exe));
            }
        }
    }

    public List<string> GetDataPathWarnings()
    {
        UpdateWarnings();
        return _dataPathWarnings;
    }

    public List<string> GetProjectPathWarnings()
    {
        UpdateWarnings();
        return _projectPathWarnings;
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

    #endregion

    #region Fields

    /// <summary>
    /// A field's titled box, with an icon for how the field stands.
    /// </summary>
    public static void FieldHeader(string id, string title, string tooltip, FieldStatus status)
    {
        var icon = status switch
        {
            FieldStatus.Valid => Icons.CheckCircle,
            FieldStatus.Warning => Icons.ExclamationTriangle,
            _ => Icons.TimesCircle
        };

        var color = status switch
        {
            FieldStatus.Valid => UI.Current.ImGui_Benefit_Text_Color,
            FieldStatus.Warning => UI.Current.ImGui_AliasName_Text,
            _ => UI.Current.ImGui_Invalid_Text_Color
        };

        GUI.SimpleHeader(id, $"{icon} {title}", tooltip, color);
    }

    /// <summary>
    /// The field's problem in the invalid colour, then its warnings, under its input.
    /// </summary>
    private static void FieldMessages(string issue, List<string> warnings)
    {
        if (issue != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, issue);

        if (warnings != null)
        {
            foreach (var warning in warnings)
                GUI.WrappedYellowText(warning);
        }
    }

    private static FieldStatus GetStatus(string issue, List<string> warnings = null)
    {
        if (issue != null)
            return FieldStatus.Invalid;

        if (warnings != null && warnings.Count > 0)
            return FieldStatus.Warning;

        return FieldStatus.Valid;
    }

    /// <summary>
    /// The game combo. Picking a game fills an empty data directory from its Steam install.
    /// </summary>
    public void DisplayGameField(string title)
    {
        var issue = GetTypeIssue();

        FieldHeader("projectGameHeader", title, LOC.Get("PROJECT_Configuration_Header_Project_Type_TT"), GetStatus(issue));

        ImGui.SetNextItemWidth(-1);

        var previewName = LOC.Get(Descriptor.ProjectType.GetDisplayName());

        if (ImGui.BeginCombo("##projectTypePicker", previewName))
        {
            foreach (var type in ProjectTypeOrder.Order)
            {
                var displayName = LOC.Get(type.GetDisplayName());

                if (ImGui.Selectable($"{displayName}##projectType_{type}", type == Descriptor.ProjectType))
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
        GUI.Tooltip(LOC.Get("PROJECT_Configuration_Header_Project_Type_TT"));

        FieldMessages(issue, null);
    }

    /// <summary>
    /// The game, which can't be changed once the project is created.
    /// </summary>
    public void DisplayReadOnlyGameField()
    {
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Type"),
            LOC.Get("PROJECT_Configuration_Header_Project_Type_TT"));

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(LOC.Get(Descriptor.ProjectType.GetDisplayName()));

        ImGui.TextDisabled(LOC.Get("PROJECT_Field_Type_Fixed"));
    }

    public void DisplayDataDirectoryField(string title)
    {
        var issue = GetDataPathIssue();
        var warnings = GetDataPathWarnings();

        FieldHeader("projectDataDirHeader", title, GetDataDirectoryTooltip(), GetStatus(issue, warnings));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##DataDirPath", LOC.Get("PROJECT_Configuration_Data_Directory_Path_Hint"), ref Descriptor.DataPath, 255);
        GUI.Tooltip(GetDataDirectoryTooltip());

        FieldMessages(issue, warnings);

        var steamDataPath = GetSteamDataPath(Descriptor.ProjectType);

        GUI.ConditionalMultiButtonInput("dataDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory_TT"),
            SelectDataDirectory, true,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory_TT"),
            OpenDataDirectory, Directory.Exists(Descriptor.DataPath),

            "useSteamDir",
            LOC.Get("PROJECT_Configuration_Action_Use_Steam_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Use_Steam_Data_Directory_TT", steamDataPath ?? ""),
            UseSteamDataPathAction,
            !string.IsNullOrEmpty(steamDataPath) && steamDataPath != Descriptor.DataPath);
    }

    /// <param name="offerNewFolder">Offer to make a folder named after the project in the default mod
    /// directory, for a new project.</param>
    public void DisplayProjectDirectoryField(string title, bool offerNewFolder)
    {
        var issue = GetProjectPathIssue();
        var warnings = GetProjectPathWarnings();

        FieldHeader("projectDirHeader", title, LOC.Get("PROJECT_Configuration_Header_Project_Directory_TT"), GetStatus(issue, warnings));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##ProjectDirPath", LOC.Get("PROJECT_Configuration_Project_Directory_Path_Hint"), ref Descriptor.ProjectPath, 255);
        GUI.Tooltip(LOC.Get("PROJECT_Configuration_Header_Project_Directory_TT"));

        FieldMessages(issue, warnings);

        GUI.ConditionalMultiButtonInput("projectDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory_TT"),
            SelectProjectDirectory, true,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory_TT"),
            OpenProjectDirectory, Directory.Exists(Descriptor.ProjectPath));

        if (offerNewFolder)
        {
            var newFolder = GetNewProjectFolder();

            if (newFolder != null && !PathsEqual(newFolder, Descriptor.ProjectPath))
            {
                if (GUI.ExplainedButton(
                    $"{LOC.Get("PROJECT_Create_New_Folder")}##createProjectFolder",
                    LOC.Get("PROJECT_Create_New_Folder_TT", newFolder),
                    true))
                {
                    CreateProjectFolderAction(newFolder);
                }
            }
        }
    }

    /// <summary>
    /// A folder named after the project in the default mod directory, or null if there is no default directory or
    /// no name yet.
    /// </summary>
    private string GetNewProjectFolder()
    {
        var root = CFG.Current.Project_Default_Mod_Directory;
        var name = Descriptor.ProjectName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root) || name == "")
            return null;

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return Path.Combine(root, name);
    }

    private void CreateProjectFolderAction(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            Descriptor.ProjectPath = folder;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Create_New_Folder_Failed", folder), e);
        }
    }

    /// <param name="folderNameHint">Say that the folder name is used when the name is left empty.</param>
    public void DisplayNameField(string title, bool folderNameHint)
    {
        var issue = GetNameIssue();

        FieldHeader("projectNameHeader", title, LOC.Get("PROJECT_Configuration_Header_Project_Name_TT"), GetStatus(issue));

        var hint = LOC.Get("PROJECT_Configuration_Project_Name_Input_Hint");

        if (folderNameHint)
        {
            var folderName = GetEffectiveName();

            if (folderName != "" && string.IsNullOrWhiteSpace(Descriptor.ProjectName))
                hint = folderName;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##ProjectNameInput", hint, ref Descriptor.ProjectName, 255);
        GUI.Tooltip(LOC.Get("PROJECT_Configuration_Header_Project_Name_TT"));

        if (folderNameHint && string.IsNullOrWhiteSpace(Descriptor.ProjectName) && issue == null)
            ImGui.TextDisabled(LOC.Get("PROJECT_Field_Name_From_Folder"));

        FieldMessages(issue, null);
    }

    public void DisplayGroupTagField()
    {
        GUI.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag"),
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##FolderTagInput", LOC.Get("PROJECT_Configuration_Group_Tag_Hint"), ref Descriptor.FolderTag, 255);
        GUI.Tooltip(LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag_TT"));
    }

    public void DisplayEditorToggles()
    {
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

        EditorToggle("PROJECT_Configuration_Checkbox_Fx_Editor",
            ref Descriptor.EnableFxEditor, EditorConditions.SupportsFxEditor(type));

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
        Descriptor.EnableFxEditor = enabled;
        Descriptor.EnableMaterialEditor = enabled;
        Descriptor.EnableTextureViewer = enabled;
        Descriptor.EnableFileBrowser = enabled;
        Descriptor.EnableAnimEditor = enabled;
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

    #endregion

    #region Directories

    public void SelectProjectDirectory()
    {
        var newProjectPath = "";
        bool result;

        if (CFG.Current.Project_Default_Mod_Directory != "")
        {
            result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Project_Directory"),
                out newProjectPath, CFG.Current.Project_Default_Mod_Directory);
        }
        else
        {
            result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Project_Directory"), out newProjectPath);
        }

        if (result)
        {
            Descriptor.ProjectPath = newProjectPath;
        }
    }

    public void OpenProjectDirectory()
    {
        OpenDirectory(Descriptor.ProjectPath);
    }

    public void SelectDataDirectory()
    {
        var newDataPath = "";
        bool result;

        if (CFG.Current.Project_Default_Data_Directory != "")
        {
            result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Game_Directory"), out newDataPath, CFG.Current.Project_Default_Data_Directory);
        }
        else
        {
            result = PlatformUtils.Instance.OpenFolderDialog(
                LOC.Get("PROJECT_Configuration_Dialog_Select_Game_Directory"), out newDataPath);
        }

        if (result)
        {
            Descriptor.DataPath = newDataPath;
        }
    }

    public void OpenDataDirectory()
    {
        OpenDirectory(Descriptor.DataPath);
    }

    /// <summary>
    /// Opens a directory in the platform's file manager. Does nothing for an empty or missing path, rather than
    /// opening a default location.
    /// </summary>
    public static void OpenDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        try
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ProjectConfigureMenu), $"Failed to open the directory \"{path}\".", e);
        }
    }

    #endregion

    #region Actions

    public void CreateProjectAction()
    {
        if (!AllowCreation())
            return;

        // An empty name takes the project folder's name
        Descriptor.ProjectName = GetEffectiveName();

        var newProject = Smithbox.Orchestrator.CreateProject(Descriptor);

        // Switch to editing the new project, so pressing Create again doesn't register a duplicate
        Editor.OnProjectCreated(newProject);
    }

    public void CreateProjectFromJsonAction()
    {
        var orchestrator = Smithbox.Orchestrator;

        var projectJsonPath = "";
        var result = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PROJECT_Configuration_Action_Select_Project_JSON"), out projectJsonPath);

        if (result)
        {
            var newProject = orchestrator.CreateProjectFromLegacyJson(projectJsonPath);

            if (newProject != null)
                Editor.OnProjectCreated(newProject);
        }
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

    #endregion
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
