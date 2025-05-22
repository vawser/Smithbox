using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Core;

public static class ProjectCreation
{
    private static bool Display = false;
    public static bool Create = false;

    public static string ProjectName = "";
    public static string ProjectPath = "";
    public static string DataPath = "";
    public static ProjectType ProjectType = ProjectType.Undefined;

    // These are the automatic Data directory assignments (if they exist on the user's machine)
    private static string SteamExecutable_DS1 = "";
    private static string SteamExecutable_DS1R = "";
    private static string SteamExecutable_DS2 = "";
    private static string SteamExecutable_DS2S = "";
    private static string SteamExecutable_DS3 = "";
    private static string SteamExecutable_SDT = "";
    private static string SteamExecutable_ER = "";
    private static string SteamExecutable_AC6 = "";

    public static bool AutoSelect = false;

    public static bool EnableMapEditor = true;
    public static bool EnableModelEditor = true;
    public static bool EnableTextEditor = true;
    public static bool EnableParamEditor = true;
    public static bool EnableTimeActEditor = false;
    public static bool EnableGparamEditor = true;
    public static bool EnableMaterialEditor = false;
    public static bool EnableEmevdEditor = false;
    public static bool EnableEsdEditor = false;
    public static bool EnableTextureViewer = true;
    public static bool EnableFileBrowser = false;

    public static bool EnableExternalMaterialData = true;

    // Used so the project type combo box has a specific order
    public static List<ProjectType> ProjectTypeOrder = new()
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
        ProjectType.AC6,
    };

    public static void Reset()
    {
        ProjectName = "";
        ProjectPath = "";
        DataPath = "";
        ProjectType = ProjectType.Undefined;
    }

    public static void Show()
    {
        SteamExecutable_DS1 = SteamGameLocator.FindGameExecutable(211420, "DATA\\DARKSOULS.exe");
        SteamExecutable_DS1R = SteamGameLocator.FindGameExecutable(570940, "DarkSoulsRemastered.exe");
        SteamExecutable_DS2 = SteamGameLocator.FindGameExecutable(236430, "Game\\DarkSoulsII.exe");
        SteamExecutable_DS2S = SteamGameLocator.FindGameExecutable(335300, "Game\\DarkSoulsII.exe");
        SteamExecutable_DS3 = SteamGameLocator.FindGameExecutable(374320, "Game\\DarkSoulsIII.exe");
        SteamExecutable_SDT = SteamGameLocator.FindGameExecutable(814380, "sekiro.exe");
        SteamExecutable_ER = SteamGameLocator.FindGameExecutable(1245620, "Game\\eldenring.exe");
        SteamExecutable_AC6 = SteamGameLocator.FindGameExecutable(1888160, "Game\\armoredcore6.exe");

        Display = true;
    }

    public static void Draw()
    {
        var inputWidth = 400.0f;
        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        //ImGui.SetNextWindowSize(new Vector2(640, 240), ImGuiCond.Always);

        if (Display)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

            if (ImGui.Begin("Project Creation##projectCreationWindow", ref Display, flags))
            {
                if (ImGui.BeginTable($"projectCreationTable", 3, ImGuiTableFlags.SizingFixedFit))
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

                        foreach (var entry in ProjectTypeOrder)
                        {
                            var type = (ProjectType)entry;

                            if (ImGui.Selectable(type.GetDisplayName()))
                            {
                                ProjectType = type;

                                if(ProjectType is ProjectType.DS1 && SteamExecutable_DS1 != "" && SteamExecutable_DS1 != null)
                                {
                                    DataPath = SteamExecutable_DS1;
                                }
                                if (ProjectType is ProjectType.DS1R && SteamExecutable_DS1R != "" && SteamExecutable_DS1R != null)
                                {
                                    DataPath = SteamExecutable_DS1R;
                                }
                                if (ProjectType is ProjectType.DS2 && SteamExecutable_DS2 != "" && SteamExecutable_DS2 != null)
                                {
                                    DataPath = SteamExecutable_DS2;
                                }
                                if (ProjectType is ProjectType.DS2S && SteamExecutable_DS2S != "" && SteamExecutable_DS2S != null)
                                {
                                    DataPath = SteamExecutable_DS2S;
                                }
                                if (ProjectType is ProjectType.DS3 && SteamExecutable_DS3 != "" && SteamExecutable_DS3 != null)
                                {
                                    DataPath = SteamExecutable_DS3;
                                }
                                if (ProjectType is ProjectType.SDT && SteamExecutable_SDT != "" && SteamExecutable_SDT != null)
                                {
                                    DataPath = SteamExecutable_SDT;
                                }
                                if (ProjectType is ProjectType.ER && SteamExecutable_ER != "" && SteamExecutable_ER != null)
                                {
                                    DataPath = SteamExecutable_ER;
                                }
                                if (ProjectType is ProjectType.AC6 && SteamExecutable_AC6 != "" && SteamExecutable_AC6 != null)
                                {
                                    DataPath = SteamExecutable_AC6;
                                }
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
                    UIHelper.Tooltip("The location of the project.\nHint: for most mods, this is the folder in which the mod's regulation.bin is stored.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    ImGui.InputText("##projectPathInput", ref ProjectPath, 255);

                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Select##projectPathSelect"))
                    {
                        if (CFG.Current.DefaultModDirectory != "")
                        {
                            var newProjectPath = "";
                            var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath, CFG.Current.DefaultModDirectory);

                            if (result)
                            {
                                ProjectPath = newProjectPath;
                            }
                        }
                        else
                        {
                            var newProjectPath = "";
                            var result = PlatformUtils.Instance.OpenFolderDialog("Select Project Directory", out newProjectPath);

                            if (result)
                            {
                                ProjectPath = newProjectPath;
                            }
                        }
                    }

                    // Data Path
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Data Directory");
                    UIHelper.Tooltip("The location of the game data.\nHint: select the directory that the game executable resides in.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    ImGui.InputText("##dataPathInput", ref DataPath, 255);

                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Select##dataPathSelect"))
                    {
                        if (CFG.Current.DefaultDataDirectory != "")
                        {
                            var newDataPath = "";
                            var result = PlatformUtils.Instance.OpenFolderDialog("Select Game Directory", out newDataPath, CFG.Current.DefaultDataDirectory);

                            if (result)
                            {
                                DataPath = newDataPath;
                            }
                        }
                        else
                        {
                            var newDataPath = "";
                            var result = PlatformUtils.Instance.OpenFolderDialog("Select Game Directory", out newDataPath);

                            if (result)
                            {
                                DataPath = newDataPath;
                            }
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

                    DisplayEditorToggles();
                    DisplayDataToggles();

                    if (!AllowCreation())
                    {
                        ImGui.Separator();

                        ImGui.Text(GetCreationBlockedTooltip());

                        ImGui.Separator();
                    }

                    var buttonSize = new Vector2(310, 24);

                    // Create
                    if (AllowCreation())
                    {
                        if (ImGui.Button("Create##createProjectCreation", buttonSize))
                        {
                            Display = false;
                            Create = true;
                        }
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Create##createProjectCreation", buttonSize))
                        {
                        }
                        ImGui.EndDisabled();
                    }

                    ImGui.SameLine();

                    // Cancel
                    if (ImGui.Button("Cancel##cancelProjectCreation", buttonSize))
                    {
                        Display = false;
                    }
                }


                ImGui.End();
            }

            ImGui.PopStyleColor(1);
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

    private static bool AllowCreation()
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

        if (ProjectType is ProjectType.Undefined)
            isAllowed = false;

        return isAllowed;
    }

    private static string GetCreationBlockedTooltip()
    {
        var tooltip = "You cannot create a project due to the following issues:";

        if (ProjectName == "")
            tooltip = tooltip + "\n" + "Project Name cannot be empty.";

        if (!Directory.Exists(ProjectPath))
            tooltip = tooltip + "\n" + "Project Path is set to an invalid path.";

        if (!Directory.Exists(DataPath))
            tooltip = tooltip + "\n" + "Data Path is set to an invalid path.";

        if (ProjectType is ProjectType.Undefined)
            tooltip = tooltip + "\n" + "Project type cannot be undefined.";

        return tooltip;
    }
}
