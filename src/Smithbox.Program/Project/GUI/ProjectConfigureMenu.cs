using Hexa.NET.ImGui;
using Octokit;
using Silk.NET.SDL;
using StudioCore.Application;
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
    }

    public void SetupForCreation()
    {
        EditorMode = ProjectEditorMode.Create;

        Descriptor = new ProjectDescriptor();
        Project = new ProjectEntry();

        Descriptor.EnableAllEditors();
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
        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        if (!AllowCreation())
        {
            ImGui.Text(GetCreationBlockedTooltip());
        }

        if (EditorMode is ProjectEditorMode.Create)
        {
            UIHelper.MultiButtonInput("projectCreateActions",
                "createProject", "Create Project", "", CreateProjectAction);
        }
        else if (EditorMode is ProjectEditorMode.Edit)
        {
            UIHelper.MultiButtonInput("projectEditActions",
                "editProject", "Update Project", "Update the currently selected project configuration to use the newly configured parameters.", UpdateProjectAction,
                "deleteProject", "Delete Project", "Delete the currently selected project entry.", DeleteProjectAction,
                "clearProjectBackups", "Delete Backup Files", "Delete the .prev and .bak files within the currently selected project's directory.", DeleteBackupFilesAction);
        }

        if (ModEngineHandler.IsME3Project(Project))
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("ME3", "");

            UIHelper.MultiButtonInput("me3Actions",
                "launchMod", "Launch Mod", "Launch this project with ME3.", LaunchModAction,
                "createProfile", "Create Profile", "Create a basic ME3 profile for this project.", CreateProfileAction);
        }
    }

    public void CreateProjectAction()
    {
        Smithbox.Orchestrator.CreateProject(Descriptor);
    }

    public void UpdateProjectAction()
    {
        Smithbox.Orchestrator.UpdateProject(Project, Descriptor);
    }

    public void DeleteProjectAction()
    {
        ProjectUtils.DeleteProject(Project);
    }

    public void LaunchModAction()
    {
        ModEngineHandler.LaunchME3Mod(Project);
    }

    public void CreateProfileAction()
    {
        if (CFG.Current.Project_ME3_Profile_Directory != "")
        {
            ModEngineHandler.CreateME3Profile(Project);
        }
        else
        {
            var profilePath = "";
            var result =
                PlatformUtils.Instance.OpenFolderDialog("Select ME3 Profile Directory", out profilePath);

            if (result)
            {
                CFG.Current.Project_ME3_Profile_Directory = profilePath;
            }
        }
    }

    public void DeleteBackupFilesAction()
    {
        var root = Project.Descriptor.ProjectPath;

        if (!Directory.Exists(root))
            return;

        var filesToDelete = ProjectUtils.GetBackupFiles(root);

        var fileList = "";

        int i = 0;

        foreach (var entry in filesToDelete)
        {
            fileList = fileList + $"\n{entry}";

            i++;

            if (i > 25)
            {
                fileList = fileList + $"\n....";
                break;
            }
        }

        var dialog = PlatformUtils.Instance.MessageBox($"You will delete the following files:\n{fileList}", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is DialogResult.OK)
        {
            ProjectUtils.DeleteFiles(filesToDelete);
        }
    }

    private void DisplayProjectEditorToggles()
    {
        UIHelper.SimpleHeader("Available Editors", "Which editors this project will use.");

        // Map Editor
        if (ProjectUtils.SupportsMapEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Visual Map Editor", ref Descriptor.EnableMapEditor);
            UIHelper.Tooltip("If true, the Visual Map Editor and associated data will be initialized for this project.");
        }

        // Map Param Editor
        if (ProjectUtils.SupportsMapDataEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Map Data Editor", ref Descriptor.EnableMapDataEditor);
            UIHelper.Tooltip("If true, the Map Data Editor and associated data will be initialized for this project.");
        }

        // Model Editor
        if (ProjectUtils.SupportsModelEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Model Editor", ref Descriptor.EnableModelEditor);
            UIHelper.Tooltip("If true, the Model Editor and associated data will be initialized for this project.");
        }

        // Param Editor
        if (ProjectUtils.SupportsParamEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Param Editor", ref Descriptor.EnableParamEditor);
            UIHelper.Tooltip("If true, the Param Editor and associated data will be initialized for this project.");
        }

        // Text Editor
        if (ProjectUtils.SupportsTextEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Text Editor", ref Descriptor.EnableTextEditor);
            UIHelper.Tooltip("If true, the Text Editor and associated data will be initialized for this project.");
        }

        // Animation Editor
        if (ProjectUtils.SupportsAnimEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Animation Editor", ref Descriptor.EnableAnimEditor);
            UIHelper.Tooltip("If true, the Animation Browser and associated data will be initialized for this project.");
        }

        // Graphics Param Editor
        if (ProjectUtils.SupportsGraphicsParamEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Graphics Param Editor", ref Descriptor.EnableGparamEditor);
            UIHelper.Tooltip("If true, the Graphics Param Editor and associated data will be initialized for this project.");
        }

        // Material Editor
        if (ProjectUtils.SupportsMaterialEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Material Editor", ref Descriptor.EnableMaterialEditor);
            UIHelper.Tooltip("If true, the Material Editor and associated data will be initialized for this project.");
        }

        // Texture Viewer
        if (ProjectUtils.SupportsTextureViewer(Descriptor.ProjectType))
        {
            ImGui.Checkbox("Texture Viewer", ref Descriptor.EnableTextureViewer);
            UIHelper.Tooltip("If true, the Texture Viewer and associated data will be initialized for this project.");
        }

        // File Browser
        if (ProjectUtils.SupportsFileBrowser(Descriptor.ProjectType))
        {
            ImGui.Checkbox("File Browser", ref Descriptor.EnableFileBrowser);
            UIHelper.Tooltip("If true, the File Browser and associated data will be initialized for this project.");
        }

        // Material Data
        ImGui.Checkbox("Material Data", ref Descriptor.EnableExternalMaterialData);
        UIHelper.Tooltip("If true, the Map Editor and Model Editor will load all Material Data, which is required for texturing. Note: this increases RAM usage significantly.");
    }

    public void DisplayProjectSettings()
    {
        // Project Name
        UIHelper.SimpleHeader("Project Name", "The name of the project.");

        UIHelper.HintTextInput("ProjectNameInput", ref Descriptor.ProjectName, "Enter the name of the project...");

        // Project Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader("Project Type", "The game this project targets.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##projectTypePicker", Descriptor.ProjectType.GetDisplayName()))
        {
            // Make the combo-box dropdown bigger so there is no need to scroll
            ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

            foreach (var entry in ProjectTypeOrder.Order)
            {
                var type = (ProjectType)entry;

                if (ImGui.Selectable(type.GetDisplayName()))
                {
                    Descriptor.ProjectType = type;

                    if (Descriptor.ProjectType is ProjectType.DS1
                        && !string.IsNullOrEmpty(SteamExecutable_DS1))
                    {
                        Descriptor.DataPath = SteamExecutable_DS1;
                    }
                    if (Descriptor.ProjectType is ProjectType.DS1R
                        && !string.IsNullOrEmpty(SteamExecutable_DS1R))
                    {
                        Descriptor.DataPath = SteamExecutable_DS1R;
                    }
                    if (Descriptor.ProjectType is ProjectType.DS2
                        && !string.IsNullOrEmpty(SteamExecutable_DS2))
                    {
                        Descriptor.DataPath = SteamExecutable_DS2;
                    }
                    if (Descriptor.ProjectType is ProjectType.DS2S
                        && !string.IsNullOrEmpty(SteamExecutable_DS2S))
                    {
                        Descriptor.DataPath = SteamExecutable_DS2S;
                    }
                    if (Descriptor.ProjectType is ProjectType.DS3
                        && !string.IsNullOrEmpty(SteamExecutable_DS3))
                    {
                        Descriptor.DataPath = SteamExecutable_DS3;
                    }
                    if (Descriptor.ProjectType is ProjectType.SDT
                        && !string.IsNullOrEmpty(SteamExecutable_SDT))
                    {
                        Descriptor.DataPath = SteamExecutable_SDT;
                    }
                    if (Descriptor.ProjectType is ProjectType.ER
                        && !string.IsNullOrEmpty(SteamExecutable_ER))
                    {
                        Descriptor.DataPath = SteamExecutable_ER;
                    }
                    if (Descriptor.ProjectType is ProjectType.AC6
                        && !string.IsNullOrEmpty(SteamExecutable_AC6))
                    {
                        Descriptor.DataPath = SteamExecutable_AC6;
                    }
                    if (Descriptor.ProjectType is ProjectType.NR
                        && !string.IsNullOrEmpty(SteamExecutable_NR))
                    {
                        Descriptor.DataPath = SteamExecutable_NR;
                    }
                }
            }
            ImGui.EndCombo();
        }

        // Project Directory
        UIHelper.Spacer();
        UIHelper.SimpleHeader("Project Directory", "The location of the project.\nHint: for most mods, this is the folder in which the mod's regulation.bin is stored.");

        UIHelper.HintTextInput("ProjectDirPath", ref Descriptor.ProjectPath, "Enter the directory your mod is placed at...");

        UIHelper.MultiButtonInput("projectDirActions",
            "selectDir", "Select Project Directory", "Select the directory for the project files.", SelectProjectDirectory,
            "openDir", "Open Project Directory", "Open the current project directory.", OpenProjectDirectory);

        // Data Directory
        UIHelper.Spacer();
        UIHelper.SimpleHeader("Data Directory", GetDataDirectoryTooltip());

        UIHelper.HintTextInput("DataDirPath", ref Descriptor.DataPath, "Enter the directory the game data is stored at...");

        UIHelper.MultiButtonInput("dataDirActions",
            "selectDir", "Select Data Directory", "Select the directory for the game data files.", SelectDataDirectory,
            "openDir", "Open Data Directory", "Open the current data directory.", OpenDataDirectory);

        // Group Tag
        UIHelper.Spacer();
        UIHelper.SimpleHeader("Group Tag", "A tag to apply to this project. Used to group the project under a collapsible section within the selection lists.");

        UIHelper.HintTextInput("FolderTagInput", ref Descriptor.FolderTag, "Enter the tag you wish to assign to this project...");
    }

    public void SelectProjectDirectory()
    {
        if (CFG.Current.Project_Default_Mod_Directory != "")
        {
            var newProjectPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath, CFG.Current.Project_Default_Mod_Directory);

            if (result)
            {
                Descriptor.ProjectPath = newProjectPath;
            }
        }
        else
        {
            var newProjectPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath);

            if (result)
            {
                Descriptor.ProjectPath = newProjectPath;
            }
        }
    }

    public void OpenProjectDirectory()
    {
        Process.Start("explorer.exe", Descriptor.ProjectPath);
    }

    public void SelectDataDirectory()
    {
        if (CFG.Current.Project_Default_Data_Directory != "")
        {
            var newDataPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog("Select Game Directory", out newDataPath, CFG.Current.Project_Default_Data_Directory);

            if (result)
            {
                Descriptor.DataPath = newDataPath;
            }
        }
        else
        {
            var newDataPath = "";
            var result = PlatformUtils.Instance.OpenFolderDialog("Select Game Directory", out newDataPath);

            if (result)
            {
                Descriptor.DataPath = newDataPath;
            }
        }
    }
    public void OpenDataDirectory()
    {
        Process.Start("explorer.exe", Descriptor.DataPath);
    }

    private bool AllowCreation()
    {
        bool isAllowed = true;

        if (Descriptor.ProjectName == "")
            isAllowed = false;

        if (!Directory.Exists(Descriptor.ProjectPath))
            isAllowed = false;

        if (!Directory.Exists(Descriptor.DataPath))
            isAllowed = false;

        if (Descriptor.ProjectName == "")
            isAllowed = false;

        if (Descriptor.ProjectType is ProjectType.Undefined)
            isAllowed = false;

        return isAllowed;
    }

    private string GetCreationBlockedTooltip()
    {
        var tooltip = "You cannot create a project due to the following issues:";

        if (Descriptor.ProjectName == "")
            tooltip = tooltip + "\n" + "Project Name cannot be empty.";

        if (!Directory.Exists(Descriptor.ProjectPath))
            tooltip = tooltip + "\n" + "Project Path is set to an invalid path.";

        if (!Directory.Exists(Descriptor.DataPath))
            tooltip = tooltip + "\n" + "Data Path is set to an invalid path.";

        if (Descriptor.ProjectType is ProjectType.Undefined)
            tooltip = tooltip + "\n" + "Project type cannot be undefined.";

        return tooltip;
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

        // If we find the PTDE install, auto-set the PTDE path for DS1R.
        if(!string.IsNullOrEmpty(SteamExecutable_DS1) && CFG.Current.PTDE_Data_Path == "")
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(SteamExecutable_DS1));
            if(Directory.Exists(dir))
            {
                CFG.Current.PTDE_Data_Path = dir;
            }
        }
    }

    public string GetDataDirectoryTooltip()
    {
        var tooltip = "The location of the game data.";
        if (Descriptor.ProjectType is ProjectType.DES)
        {
            tooltip = $"{tooltip}\nSelect the USRDIR folder.";
        }
        else if (Descriptor.ProjectType is ProjectType.BB)
        {
            tooltip = $"{tooltip}\nSelect the dvdroot_ps4 folder.";
        }
        else
        {
            tooltip = $"{tooltip}\nSelect the folder that contains the game executable.";
        }

        return tooltip;
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
    };

}
public enum ProjectEditorMode
{
    Create,
    Edit
}