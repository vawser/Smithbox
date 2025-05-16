using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Core;

public static class ProjectSettings
{
    private static Smithbox BaseEditor;
    private static ProjectEntry TargetProject;

    private static bool Display = false;
    public static bool Create = false;

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

    public static bool EnableExternalMaterialData = true;

    public static bool EditorStateChanged = false;

    public static void Show(Smithbox baseEditor, ProjectEntry curProject)
    {
        BaseEditor = baseEditor;
        TargetProject = curProject;

        ProjectName = curProject.ProjectName;
        ProjectPath = curProject.ProjectPath;
        DataPath = curProject.DataPath;
        ProjectType = curProject.ProjectType;
        AutoSelect = curProject.AutoSelect;

        EnableMapEditor = curProject.EnableMapEditor;
        EnableModelEditor = curProject.EnableModelEditor;
        EnableTextEditor = curProject.EnableTextEditor;
        EnableParamEditor = curProject.EnableParamEditor;
        EnableTimeActEditor = curProject.EnableTimeActEditor;
        EnableGparamEditor = curProject.EnableGparamEditor;
        EnableMaterialEditor = curProject.EnableMaterialEditor;
        EnableEmevdEditor = curProject.EnableEmevdEditor;
        EnableEsdEditor = curProject.EnableEsdEditor;
        EnableTextureViewer = curProject.EnableTextureViewer;
        EnableFileBrowser = curProject.EnableFileBrowser;

        EditorStateChanged = false;
        Display = true;
    }

    public static void Draw()
    {
        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        //ImGui.SetNextWindowSize(new Vector2(670, 356), ImGuiCond.Always);

        if (Display)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

            if (ImGui.Begin("Project Settings##projectSettingsWindow", ref Display, flags))
            {
                DisplayMainSettings();
                DisplayEditorToggles();
                DisplayDataToggles();
                DisplayActions();

                ImGui.End();
            }

            ImGui.PopStyleColor(1);
        }
    }

    private static void DisplayActions()
    {
        if (!AllowUpdate())
        {
            ImGui.Separator();

            ImGui.Text(GetUpdateBlockedTooltip());

            ImGui.Separator();
        }

        var buttonSize = new Vector2(200, 24);

        // Update
        if (AllowUpdate())
        {
            if (ImGui.Button("Update##updateProjectSettings", buttonSize))
            {
                Display = false;

                TargetProject.ProjectName = ProjectName;
                TargetProject.ProjectPath = ProjectPath;
                TargetProject.DataPath = DataPath;
                TargetProject.ProjectType = ProjectType;
                TargetProject.AutoSelect = AutoSelect;

                TargetProject.EnableMapEditor = EnableMapEditor;
                TargetProject.EnableModelEditor = EnableModelEditor;
                TargetProject.EnableTextEditor = EnableTextEditor;
                TargetProject.EnableParamEditor = EnableParamEditor;
                TargetProject.EnableTimeActEditor = EnableTimeActEditor;
                TargetProject.EnableGparamEditor = EnableGparamEditor;
                TargetProject.EnableMaterialEditor = EnableMaterialEditor;
                TargetProject.EnableEmevdEditor = EnableEmevdEditor;
                TargetProject.EnableEsdEditor = EnableEsdEditor;
                TargetProject.EnableTextureViewer = EnableTextureViewer;
                TargetProject.EnableFileBrowser = EnableFileBrowser;

                TargetProject.EnableExternalMaterialData = EnableExternalMaterialData;

                BaseEditor.ProjectManager.SaveProject(TargetProject);

                if (EditorStateChanged)
                {
                    TargetProject.InitializeEditors(InitType.ProjectDefined);
                }
            }
            UIHelper.Tooltip("Updates the project settings.");
        }
        else
        {
            ImGui.BeginDisabled();
            if (ImGui.Button("Update##updateProjectSettings", buttonSize))
            {
            }
            ImGui.EndDisabled();
        }

        ImGui.SameLine();

        // Delete
        if (ImGui.Button("Delete##deleteProject", buttonSize))
        {
            Display = false;

            // Delete the project file
            var filename = @$"{AppContext.BaseDirectory}\.smithbox\Projects\{TargetProject.ProjectGUID}.json";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            // Remove the project from the ordering file and retain sequential ids
            var curOrderEntry = BaseEditor.ProjectManager.ProjectDisplayOrder.DisplayOrder.Where(e => e.Value == TargetProject.ProjectGUID).FirstOrDefault();

            var existingOrder = BaseEditor.ProjectManager.ProjectDisplayOrder.DisplayOrder;
            var newOrder = new Dictionary<int, Guid>();

            var count = 0;
            for (int i = 0; i < existingOrder.Count; i++)
            {
                var curGuid = existingOrder[i];
                if (curGuid != TargetProject.ProjectGUID)
                {
                    newOrder.Add(count, curGuid);
                    count++;
                }
            }

            BaseEditor.ProjectManager.ProjectDisplayOrder.DisplayOrder = newOrder;

            BaseEditor.ProjectManager.SaveProjectDisplayOrder();

            // Unload the project editor stuff
            BaseEditor.ProjectManager.SelectedProject = null;
            BaseEditor.ProjectManager.Projects.Remove(TargetProject);
        }
        UIHelper.Tooltip("Deletes this project entry (not the mod data) and unloads the current project editors.");

        ImGui.SameLine();

        // Cancel
        if (ImGui.Button("Close##closeProjectSettings", buttonSize))
        {
            Display = false;
        }
        UIHelper.Tooltip("Closes the project settings menu.");
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

        if (ImGui.CollapsingHeader("Editors", ImGuiTreeNodeFlags.DefaultOpen))
        {
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
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableMapEditor != EnableMapEditor)
                    {
                        EditorStateChanged = true;
                    }
                }

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Map Editor");
                UIHelper.Tooltip("If true, the Map Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableModelEditor", ref EnableModelEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableModelEditor != EnableModelEditor)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Model Editor");
                UIHelper.Tooltip("If true, the Model Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(4);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableTextEditor", ref EnableTextEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableTextEditor != EnableTextEditor)
                    {
                        EditorStateChanged = true;
                    }
                }

                ImGui.TableSetColumnIndex(5);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Text Editor");
                UIHelper.Tooltip("If true, the Text Editor and associated data will be initialized for this project.");


                // Section 2
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableParamEditor", ref EnableParamEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableParamEditor != EnableParamEditor)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Param Editor");
                UIHelper.Tooltip("If true, the Param Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableGparamEditor", ref EnableGparamEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableGparamEditor != EnableGparamEditor)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Graphics Param Editor");
                UIHelper.Tooltip("If true, the Graphics Param Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(4);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableTimeActEditor", ref EnableTimeActEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableTimeActEditor != EnableTimeActEditor)
                    {
                        EditorStateChanged = true;
                    }
                }

                ImGui.TableSetColumnIndex(5);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Time Act Editor");
                UIHelper.Tooltip("If true, the Time Act Editor and associated data will be initialized for this project.");

                // Section 3
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableMaterialEditor", ref EnableMaterialEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableMaterialEditor != EnableMaterialEditor)
                    {
                        EditorStateChanged = true;
                    }
                }

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Material Editor");
                UIHelper.Tooltip("If true, the Material Editor and associated data will be initialized for this project.");


                ImGui.TableSetColumnIndex(2);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableEmevdEditor", ref EnableEmevdEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableEmevdEditor != EnableEmevdEditor)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Event Script Editor");
                UIHelper.Tooltip("If true, the Event Script Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(4);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableEsdEditor", ref EnableEsdEditor);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableEsdEditor != EnableEsdEditor)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(5);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("EzState Script Editor");
                UIHelper.Tooltip("If true, the EzState Script Editor and associated data will be initialized for this project.");

                // Section 4
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableTextureViewer", ref EnableTextureViewer);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableTextureViewer != EnableTextureViewer)
                    {
                        EditorStateChanged = true;
                    }
                }
                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Texture Viewer");
                UIHelper.Tooltip("If true, the Texture Viewer and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                ImGui.SetNextItemWidth(inputWidth);

                ImGui.Checkbox("##projectEnableFileBrowser", ref EnableFileBrowser);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TargetProject.EnableFileBrowser != EnableFileBrowser)
                    {
                        EditorStateChanged = true;
                    }
                }

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
    }

    private static void DisplayDataToggles()
    {
        var inputWidth = 400.0f;

        if (ImGui.CollapsingHeader("Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Editor Toggles
            if (ImGui.BeginTable($"editorDataToggleTable", 6, ImGuiTableFlags.SizingFixedFit))
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

                ImGui.Checkbox("##projectEnableExternalMaterialData", ref EnableExternalMaterialData);

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Material Data");
                UIHelper.Tooltip("If true, the Map Editor and Model Editor will load all Material Data, which is required for texturing. Note: this increases RAM usage significantly.");

                ImGui.TableSetColumnIndex(2);

                ImGui.TableSetColumnIndex(4);

                ImGui.TableSetColumnIndex(5);

                ImGui.EndTable();
            }
        }
    }

    private static bool AllowUpdate()
    {
        bool isAllowed = true;

        if (ProjectName == "")
            isAllowed = false;

        if (!Directory.Exists(ProjectPath))
            isAllowed = false;

        if (!Directory.Exists(DataPath))
            isAllowed = false;

        if (ProjectName == "")
            isAllowed = false;

        return isAllowed;
    }

    private static string GetUpdateBlockedTooltip()
    {
        var tooltip = "You cannot update this project due to the following issues:";

        if (ProjectName == "")
            tooltip = tooltip + "\n" + "Project Name cannot be empty.";

        if (!Directory.Exists(ProjectPath))
            tooltip = tooltip + "\n" + "Project Path is set to an invalid path.";

        if (!Directory.Exists(DataPath))
            tooltip = tooltip + "\n" + "Data Path is set to an invalid path.";

        return tooltip;
    }
}
