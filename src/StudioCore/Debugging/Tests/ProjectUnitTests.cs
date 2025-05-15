using Hexa.NET.ImGui;
using Octokit;
using Silk.NET.SDL;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public static class ProjectUnitTests
{
    public static ProjectEntry TestProject;

    public static string ProjectName = "";
    public static string ProjectPath = "";
    public static string DataPath = "";
    public static ProjectType ProjectType = ProjectType.Undefined;
    public static bool AutoSelect = false;

    public static bool EnableMapEditor = true;
    public static bool EnableModelEditor = true;
    public static bool EnableTextEditor = true;
    public static bool EnableParamEditor = true;
    public static bool EnableTimeActEditor = true;
    public static bool EnableGparamEditor = true;
    public static bool EnableMaterialEditor = true;
    public static bool EnableEmevdEditor = true;
    public static bool EnableEsdEditor = true;
    public static bool EnableTextureViewer = true;
    public static bool EnableFileBrowser = true;

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var inputWidth = 400.0f;
        var buttonSize = new Vector2(200, 24);

        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        //ImGui.SetNextWindowSize(new Vector2(670, 356), ImGuiCond.Always);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

        if (ImGui.Begin("Project Test Suite##projectTestSuite", flags))
        {
            DisplayMainSettings();
            DisplayEditorToggles();
            DisplayActions(baseEditor);

            ImGui.End();
        }

        ImGui.PopStyleColor(1);
    }

    private static void DisplayMainSettings()
    {
        var inputWidth = 400.0f;

        // Main Settings
        if (ImGui.BeginTable($"projectSettingsTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

            // Project Name
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name");
            UIHelper.Tooltip("The name of the project.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            ImGui.InputText("##projectNameInput", ref ProjectName, 255);

            ImGui.TableSetColumnIndex(2);

            // Project Type
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Project Type");
            UIHelper.Tooltip("The game this project is targeting.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            if (ImGui.BeginCombo("##projectTypePicker", ProjectType.GetDisplayName()))
            {
                // Make the combo-box dropdown bigger so there is no need to scroll
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 600));

                foreach (var entry in ProjectCreation.ProjectTypeOrder)
                {
                    var type = (ProjectType)entry;

                    if (ImGui.Selectable(type.GetDisplayName()))
                    {
                        ProjectType = type;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.TableSetColumnIndex(2);

            // Project Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Project Directory");
            UIHelper.Tooltip("The location of the project.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            ImGui.InputText("##projectPathInput", ref ProjectPath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##projectPathSelect"))
            {
                var newProjectPath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath);

                if (result)
                {
                    ProjectPath = newProjectPath;
                }
            }

            // Data Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Data Directory");
            UIHelper.Tooltip("The location of the game data.\nSelect the game executable directory.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            ImGui.InputText("##dataPathInput", ref DataPath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##dataPathSelect"))
            {
                var newDataPath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Game Directory", out newDataPath);

                if (result)
                {
                    DataPath = newDataPath;
                }
            }

            // Automatic Load
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Automatic Load");
            UIHelper.Tooltip("If true, then this project will be automatically loaded when Smithbox launches.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectAutoLoad", ref AutoSelect);

            ImGui.TableSetColumnIndex(2);

            ImGui.EndTable();
        }
    }
    private static void DisplayEditorToggles()
    {
        var inputWidth = 400.0f;

        // Editor Toggles
        if (ImGui.BeginTable($"editorToggleTable", 6, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("EditorName_Col1", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("EditorToggle_Col1", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("EditorName_Col2", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("EditorToggle_Col2", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("EditorName_Col3", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("EditorToggle_Col3", ImGuiTableColumnFlags.WidthFixed);

            // Section 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableMapEditor", ref EnableMapEditor);

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Map Editor");
            UIHelper.Tooltip("If true, the Map Editor and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(2);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableModelEditor", ref EnableModelEditor);
            ImGui.TableSetColumnIndex(3);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Model Editor");
            UIHelper.Tooltip("If true, the Model Editor and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(4);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableTextEditor", ref EnableTextEditor);

            ImGui.TableSetColumnIndex(5);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Text Editor");
            UIHelper.Tooltip("If true, the Text Editor and associated data will be initialized for this project.");


            // Section 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableParamEditor", ref EnableParamEditor);
            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Param Editor");
            UIHelper.Tooltip("If true, the Param Editor and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(2);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableGparamEditor", ref EnableGparamEditor);
            ImGui.TableSetColumnIndex(3);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Graphics Param Editor");
            UIHelper.Tooltip("If true, the Graphics Param Editor and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(4);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableTimeActEditor", ref EnableTimeActEditor);

            ImGui.TableSetColumnIndex(5);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Time Act Editor");
            UIHelper.Tooltip("If true, the Time Act Editor and associated data will be initialized for this project.");

            // Section 3
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableMaterialEditor", ref EnableMaterialEditor);

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Material Editor");
            UIHelper.Tooltip("If true, the Material Editor and associated data will be initialized for this project.");


            ImGui.TableSetColumnIndex(2);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableEmevdEditor", ref EnableEmevdEditor);
            ImGui.TableSetColumnIndex(3);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Event Script Editor");
            UIHelper.Tooltip("If true, the Event Script Editor and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(4);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableEsdEditor", ref EnableEsdEditor);
            ImGui.TableSetColumnIndex(5);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("EzState Script Editor");
            UIHelper.Tooltip("If true, the EzState Script Editor and associated data will be initialized for this project.");

            // Section 4
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableTextureViewer", ref EnableTextureViewer);
            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Texture Viewer");
            UIHelper.Tooltip("If true, the Texture Viewer and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(2);

            ImGui.SetNextItemWidth(inputWidth);

            ImGui.Checkbox("##projectEnableFileBrowser", ref EnableFileBrowser);

            ImGui.TableSetColumnIndex(3);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("File Browser");
            UIHelper.Tooltip("If true, the File Browser and associated data will be initialized for this project.");

            ImGui.TableSetColumnIndex(4);

            //ImGui.SetNextItemWidth(inputWidth);

            //ImGui.Checkbox("##projectEnableEsdEditor", ref EnableEsdEditor);

            ImGui.TableSetColumnIndex(5);

            //ImGui.AlignTextToFramePadding();
            //ImGui.Text("Enable EzState Script Editor");
            //UIHelper.Tooltip("If true, the EzState Script Editor and associated data will be initialized for this project.");

            ImGui.EndTable();
        }
    }

    private static void DisplayActions(Smithbox baseEditor)
    {
        var buttonSize = new Vector2(200, 24);

        if (ImGui.Button("Test", buttonSize))
        {
            var guid = Guid.NewGuid();
            var projectName = ProjectName;
            var projectPath = ProjectPath;
            var dataPath = DataPath;
            var projectType = ProjectType;

            var newProject = new ProjectEntry(baseEditor, guid, projectName, projectPath, dataPath, projectType);

            newProject.AutoSelect = AutoSelect;

            newProject.EnableMapEditor = EnableMapEditor;
            newProject.EnableModelEditor = EnableModelEditor;
            newProject.EnableTextEditor = EnableTextEditor;
            newProject.EnableParamEditor = EnableParamEditor;
            newProject.EnableTimeActEditor = EnableTimeActEditor;
            newProject.EnableGparamEditor = EnableGparamEditor;
            newProject.EnableMaterialEditor = EnableMaterialEditor;
            newProject.EnableEmevdEditor = EnableEmevdEditor;
            newProject.EnableEsdEditor = EnableEsdEditor;
            newProject.EnableTextureViewer = EnableTextureViewer;
            newProject.EnableFileBrowser = EnableFileBrowser;

            baseEditor.ProjectManager.Projects.Add(newProject);

            baseEditor.ProjectManager.SaveProject(newProject);

            if (!baseEditor.ProjectManager.IsProjectLoading)
                baseEditor.ProjectManager.StartupProject(newProject);
        }
    }
}
