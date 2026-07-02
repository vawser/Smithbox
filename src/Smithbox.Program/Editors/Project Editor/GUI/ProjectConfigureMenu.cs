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
        UIHelper.SimpleHeader(LOC.Get("PROJECT_Configuration_Header_Actions"), "");

        if (!AllowCreation())
        {
            if(EditorMode is ProjectEditorMode.Create)
                UIHelper.WrappedText(LOC.Get("PROJECT_Configuration_Project_Creation_Block_Create"));

            if (EditorMode is ProjectEditorMode.Edit)
                UIHelper.WrappedText(LOC.Get("PROJECT_Configuration_Project_Creation_Block_Edit"));

            if (Descriptor.ProjectName == "")
                UIHelper.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Name"));

            if (!Directory.Exists(Descriptor.ProjectPath))
                UIHelper.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Path"));

            if (!Directory.Exists(Descriptor.DataPath))
                UIHelper.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Data_Path"));

            if (Descriptor.ProjectType is ProjectType.Undefined)
                UIHelper.WrappedYellowText(LOC.Get("PROJECT_Configuration_Invalid_Project_Type"));

            UIHelper.Spacer();
        }

        if (EditorMode is ProjectEditorMode.Create)
        {
            UIHelper.ConditionalMultiButtonInput("projectCreateActions",
                "createProject", 
                LOC.Get("PROJECT_Configuration_Action_Create_Project"), 
                LOC.Get("PROJECT_Configuration_Action_Create_Project_TT"), 
                CreateProjectAction, AllowCreation());
        }
        else if (EditorMode is ProjectEditorMode.Edit)
        {
            UIHelper.ConditionalMultiButtonInput("projectEditActions",
                "editProject",
                LOC.Get("PROJECT_Configuration_Action_Update_Project"),
                LOC.Get("PROJECT_Configuration_Action_Update_Project_TT"), 
                UpdateProjectAction, AllowCreation(),

                "deleteProject",
                LOC.Get("PROJECT_Configuration_Action_Delete_Project"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Project_TT"), 
                DeleteProjectAction, true,

                "clearProjectBackups",
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files"),
                LOC.Get("PROJECT_Configuration_Action_Delete_Backup_Files_TT"),
                DeleteBackupFilesAction, true);
        }

        if (ModEngineHandler.IsME3Project(Project))
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader(LOC.Get("PROJECT_Configuration_Header_ME3"), "");

            UIHelper.ConditionalMultiButtonInput("me3Actions",
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
                PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PROJECT_Configuration_Dialog_Select_ME3_Profile_Dir"), out profilePath);

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

        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PROJECT_Configuration_Dialog_Delete_Backup_Files_List", fileList), 
            LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is DialogResult.OK)
        {
            ProjectUtils.DeleteFiles(filesToDelete);
        }
    }

    private void DisplayProjectEditorToggles()
    {
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Available_Editors"), 
            LOC.Get("PROJECT_Configuration_Header_Available_Editors_TT"));

        // Map Editor
        if (ProjectUtils.SupportsMapEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Visual_Map_Editor"), 
                ref Descriptor.EnableMapEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Visual_Map_Editor_TT"));
        }

        // Map Param Editor
        if (ProjectUtils.SupportsMapDataEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Map_Data_Editor"), 
                ref Descriptor.EnableMapDataEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Map_Data_Editor_TT"));
        }

        // Model Editor
        if (ProjectUtils.SupportsModelEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Model_Editor"), 
                ref Descriptor.EnableModelEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Model_Editor_TT"));
        }

        // Param Editor
        if (ProjectUtils.SupportsParamEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Param_Editor"), 
                ref Descriptor.EnableParamEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Param_Editor_TT"));
        }

        // Text Editor
        if (ProjectUtils.SupportsTextEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Text_Editor"), 
                ref Descriptor.EnableTextEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Text_Editor_TT"));
        }

        // Animation Editor
        if (ProjectUtils.SupportsAnimEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Anim_Editor"), 
                ref Descriptor.EnableAnimEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Anim_Editor_TT"));
        }

        // Graphics Param Editor
        if (ProjectUtils.SupportsGraphicsParamEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Gparam_Editor"), 
                ref Descriptor.EnableGparamEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Gparam_Editor_TT"));
        }

        // Material Editor
        if (ProjectUtils.SupportsMaterialEditor(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Material_Editor"), 
                ref Descriptor.EnableMaterialEditor);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Material_Editor_TT"));
        }

        // Texture Viewer
        if (ProjectUtils.SupportsTextureViewer(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_Texture_Viewer"), 
                ref Descriptor.EnableTextureViewer);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_Texture_Viewer_TT"));
        }

        // File Browser
        if (ProjectUtils.SupportsFileBrowser(Descriptor.ProjectType))
        {
            ImGui.Checkbox(
                LOC.Get("PROJECT_Configuration_Checkbox_File_Browser"), 
                ref Descriptor.EnableFileBrowser);

            UIHelper.Tooltip(
                LOC.Get("PROJECT_Configuration_Checkbox_File_Browser_TT"));
        }

        // Material Data
        ImGui.Checkbox(
            LOC.Get("PROJECT_Configuration_Checkbox_Material_Data"), 
            ref Descriptor.EnableExternalMaterialData);

        UIHelper.Tooltip(
            LOC.Get("PROJECT_Configuration_Checkbox_Material_Data_TT"));
    }

    public void DisplayProjectSettings()
    {
        // Project Name
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Name"),
            LOC.Get("PROJECT_Configuration_Header_Project_Name_TT"));

        UIHelper.HintTextInput(
            "ProjectNameInput", 
            ref Descriptor.ProjectName,
            LOC.Get("PROJECT_Configuration_Project_Name_Input_Hint"));

        // Project Type
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Type"),
            LOC.Get("PROJECT_Configuration_Header_Project_Type_TT"));

        UIHelper.SetInputWidth();

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
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Header_Project_Directory_TT"));

        UIHelper.HintTextInput(
            "ProjectDirPath", 
            ref Descriptor.ProjectPath,
            LOC.Get("PROJECT_Configuration_Project_Directory_Path_Hint"));

        UIHelper.MultiButtonInput("projectDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Project_Directory_TT"),
            SelectProjectDirectory,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Project_Directory_TT"), 
            OpenProjectDirectory);

        // Data Directory
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Header_Data_Directory"), GetDataDirectoryTooltip());

        UIHelper.HintTextInput("DataDirPath", 
            ref Descriptor.DataPath,
            LOC.Get("PROJECT_Configuration_Data_Directory_Path_Hint"));

        UIHelper.MultiButtonInput("dataDirActions",
            "selectDir",
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Select_Data_Directory_TT"),
            SelectDataDirectory,

            "openDir",
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory"),
            LOC.Get("PROJECT_Configuration_Action_Open_Data_Directory_TT"),
            OpenDataDirectory);

        // Group Tag
        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag"),
            LOC.Get("PROJECT_Configuration_Data_Header_Group_Tag_TT"));

        UIHelper.HintTextInput(
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
        Process.Start("explorer.exe", Descriptor.ProjectPath);
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