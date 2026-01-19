using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

public class ProjectCreationMenu
{
    public bool IsDisplayed = false;
    public bool IsEditMode = false;
    private bool InitialLayout = false;

    public ProjectOrchestrator Orchestrator;

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

    public ProjectCreationMenu(ProjectOrchestrator orchestrator)
    {
        Orchestrator = orchestrator;
    }

    public void Draw()
    {
        if (IsDisplayed)
        {
            if (!SteamPathsInitialized)
            {
                FindSteamExecutables();
                SteamPathsInitialized = true;
            }

            if (!InitialLayout)
            {
                UIHelper.SetupPopupWindow();
                InitialLayout = true;
            }

            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

            if (ImGui.Begin("Project Creation##projectCreationWindow", ref IsDisplayed, UIHelper.GetPopupWindowFlags()))
            {
                DisplayCreationTable();

                ImGui.End();
            }

            ImGui.PopStyleColor(1);
        }
    }

    public void DisplayCreationTable()
    {
        var size = ImGui.GetContentRegionAvail();

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

            DPI.ApplyInputWidth();
            ImGui.InputTextWithHint("##projectNameInput", "The name of your project.", ref Descriptor.ProjectName, 255);

            ImGui.TableSetColumnIndex(2);

            // Project Type
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Project Type");
            UIHelper.Tooltip("The game this project is targeting.");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
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

                        if(Descriptor.ProjectType is ProjectType.DS1 && SteamExecutable_DS1 != "" && SteamExecutable_DS1 != null)
                        {
                            Descriptor.DataPath = SteamExecutable_DS1;
                        }
                        if (Descriptor.ProjectType is ProjectType.DS1R && SteamExecutable_DS1R != "" && SteamExecutable_DS1R != null)
                        {
                            Descriptor.DataPath = SteamExecutable_DS1R;
                        }
                        if (Descriptor.ProjectType is ProjectType.DS2 && SteamExecutable_DS2 != "" && SteamExecutable_DS2 != null)
                        {
                            Descriptor.DataPath = SteamExecutable_DS2;
                        }
                        if (Descriptor.ProjectType is ProjectType.DS2S && SteamExecutable_DS2S != "" && SteamExecutable_DS2S != null)
                        {
                            Descriptor.DataPath = SteamExecutable_DS2S;
                        }
                        if (Descriptor.ProjectType is ProjectType.DS3 && SteamExecutable_DS3 != "" && SteamExecutable_DS3 != null)
                        {
                            Descriptor.DataPath = SteamExecutable_DS3;
                        }
                        if (Descriptor.ProjectType is ProjectType.SDT && SteamExecutable_SDT != "" && SteamExecutable_SDT != null)
                        {
                            Descriptor.DataPath = SteamExecutable_SDT;
                        }
                        if (Descriptor.ProjectType is ProjectType.ER && SteamExecutable_ER != "" && SteamExecutable_ER != null)
                        {
                            Descriptor.DataPath = SteamExecutable_ER;
                        }
                        if (Descriptor.ProjectType is ProjectType.AC6 && SteamExecutable_AC6 != "" && SteamExecutable_AC6 != null)
                        {
                            Descriptor.DataPath = SteamExecutable_AC6;
                        }
                        if (Descriptor.ProjectType is ProjectType.NR && SteamExecutable_NR != "" && SteamExecutable_NR != null)
                        {
                            Descriptor.DataPath = SteamExecutable_NR;
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

            DPI.ApplyInputWidth();
            ImGui.InputTextWithHint("##projectPathInput", "The folder directory that your project is stored in.", ref Descriptor.ProjectPath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##projectPathSelect", DPI.SelectorButtonSize))
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

            // Data Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Data Directory");

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
            UIHelper.Tooltip(tooltip);

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.InputTextWithHint("##dataPathInput", "The folder directory the game data is stored in.", ref Descriptor.DataPath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##dataPathSelect", DPI.SelectorButtonSize))
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

            // Automatic Load
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Automatic Load");
            UIHelper.Tooltip("If true, then this project will be automatically loaded when Smithbox launches.");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.Checkbox("##projectAutoLoad", ref Descriptor.AutoSelect);

            ImGui.TableSetColumnIndex(2);

            // Automatic Load
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Row Name Import");
            UIHelper.Tooltip("If enabled, row names will be automatically imported in the Param Editor.");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.Checkbox("##projectParamRowNameImport", ref Descriptor.ImportedParamRowNames);

            ImGui.TableSetColumnIndex(2);

            // Folder Tag
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Folder Tag");
            UIHelper.Tooltip("A tag to apply to this project. Used to group the project under a folder in the selection list.");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth();
            ImGui.InputTextWithHint("##folderTagInput", "A tag to associate this project with.", ref Descriptor.FolderTag, 255);

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


            if (IsEditMode)
            {
                // Update
                if (AllowCreation())
                {
                    if (ImGui.Button("Update##updateProjectCreation"))
                    {
                        IsDisplayed = false;
                        IsEditMode = false;
                        Orchestrator.UpdateProject(Descriptor);
                    }
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Update##updateProjectCreation"))
                    {
                    }
                    ImGui.EndDisabled();
                }

                ImGui.SameLine();

                if (ImGui.Button("Delete##deleteCurrentProject"))
                {
                    IsDisplayed = false;
                    IsEditMode = false;
                    ProjectUtils.DeleteProject(Project);
                }

                ImGui.SameLine();

                // Cancel
                if (ImGui.Button("Cancel##cancelProjectCreation"))
                {
                    IsDisplayed = false;
                    IsEditMode = false;
                }
            }
            else
            {
                // Create
                if (AllowCreation())
                {
                    if (ImGui.Button("Create##createProjectCreation"))
                    {
                        IsDisplayed = false;
                        IsEditMode = false;
                        Orchestrator.CreateProject(Descriptor);
                    }
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Create##createProjectCreation"))
                    {
                    }
                    ImGui.EndDisabled();
                }

                ImGui.SameLine();

                // Cancel
                if (ImGui.Button("Cancel##cancelProjectCreation"))
                {
                    IsDisplayed = false;
                    IsEditMode = false;
                }
            }
        }
    }

    private void DisplayEditorToggles()
    {
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

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableMapEditor", ref Descriptor.EnableMapEditor);

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Map Editor");
                UIHelper.Tooltip("If true, the Map Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableModelEditor", ref Descriptor.EnableModelEditor);
                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Model Editor");
                UIHelper.Tooltip("If true, the Model Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(4);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableTextEditor", ref Descriptor.EnableTextEditor);

                ImGui.TableSetColumnIndex(5);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Text Editor");
                UIHelper.Tooltip("If true, the Text Editor and associated data will be initialized for this project.");


                // Section 2
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableParamEditor", ref Descriptor.EnableParamEditor);

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Param Editor");
                UIHelper.Tooltip("If true, the Param Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableGparamEditor", ref Descriptor.EnableGparamEditor);
                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Graphics Param Editor");
                UIHelper.Tooltip("If true, the Graphics Param Editor and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(4);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableMaterialEditor", ref Descriptor.EnableMaterialEditor);
                ImGui.TableSetColumnIndex(5);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Material Editor");
                UIHelper.Tooltip("If true, the Material Editor and associated data will be initialized for this project.");

                // Section 3
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableTextureViewer", ref Descriptor.EnableTextureViewer);

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Texture Viewer");
                UIHelper.Tooltip("If true, the Texture Viewer and associated data will be initialized for this project.");

                ImGui.TableSetColumnIndex(2);

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableFileBrowser", ref Descriptor.EnableFileBrowser);

                ImGui.TableSetColumnIndex(3);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("File Browser");
                UIHelper.Tooltip("If true, the File Browser and associated data will be initialized for this project.");

                ImGui.EndTable();
            }
        }
    }


    private void DisplayDataToggles()
    {
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

                DPI.ApplyInputWidth();
                ImGui.Checkbox("##projectEnableExternalMaterialData", ref Descriptor.EnableExternalMaterialData);

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
        if(SteamExecutable_DS1 != "" && CFG.Current.PTDE_Data_Path == "")
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(SteamExecutable_DS1));
            if(Directory.Exists(dir))
            {
                CFG.Current.PTDE_Data_Path = dir;
            }
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
    };

}