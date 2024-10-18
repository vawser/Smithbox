using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Configuration.Help;
using StudioCore.Configuration.Keybinds;
using StudioCore.Configuration.Settings;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Settings;
using StudioCore.Tools.Development;
using StudioCore.Tools.Randomiser;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Configuration.Help.HelpWindow;
using static StudioCore.Configuration.Keybinds.KeybindWindow;
using static StudioCore.Configuration.SettingsWindow;
using static StudioCore.Tools.Development.DebugWindow;

namespace StudioCore.Core;

/// <summary>
/// Handler class that holds all of the floating windows and related window state for access elsewhere.
/// </summary>
public class CommonMenubarHandler
{
    public SettingsWindow SettingsWindow;
    public HelpWindow HelpWindow;
    public DebugWindow DebugWindow;
    public KeybindWindow KeybindWindow;
    public RandomiserWindow RandomiserWindow;
    public CommonMenubarHandler(IGraphicsContext _context)
    {
        SettingsWindow = new SettingsWindow();
        HelpWindow = new HelpWindow();
        DebugWindow = new DebugWindow();
        KeybindWindow = new KeybindWindow();
        RandomiserWindow = new RandomiserWindow();
    }

    public void OnGui()
    {
        SettingsWindow.Display();
        HelpWindow.Display();
        DebugWindow.Display();
        KeybindWindow.Display();
        RandomiserWindow.Display();

        ProjectConfigurationWindow.Display();
        ProjectEnumWindow.Display();
    }

    public void ProjectDropdown()
    {
        // Project
        if (ImGui.BeginMenu("Project"))
        {
            // New Project
            if (ImGui.MenuItem("New Project"))
            {
                if (MayChangeProject())
                {
                    Smithbox.ProjectHandler.ClearProject();
                    Smithbox.ProjectHandler.IsInitialLoad = true;
                }
            }
            UIHelper.ShowHoverTooltip("Create a new project.");

            // Open Project
            if (ImGui.MenuItem("Open Project"))
            {
                if (MayChangeProject())
                {
                    Smithbox.ProjectHandler.OpenProjectDialog();
                }
            }
            UIHelper.ShowHoverTooltip("Open and load an existing project.");

            // Project Configuration
            if (ImGui.MenuItem("Project Configuration"))
            {
                ProjectConfigurationWindow.ToggleWindow();
            }
            UIHelper.ShowHoverTooltip("View the status details of your currently loaded project.");

            // Project Enums
            if (ImGui.MenuItem("Project Enums"))
            {
                ProjectEnumWindow.ToggleWindow();
            }
            UIHelper.ShowHoverTooltip("View the enums associated with your currently loaded project.");

            // Project Settings
            if (ImGui.MenuItem("Project Settings"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Project);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to the Project.");

            ImGui.Separator();

            // Recent Projects
            if (ImGui.BeginMenu("Recent Projects", MayChangeProject() && CFG.Current.RecentProjects.Count > 0))
            {
                Smithbox.ProjectHandler.DisplayRecentProjects();

                ImGui.EndMenu();
            }
            UIHelper.ShowHoverTooltip("Quick-load existing projects that have been recently loaded.");

            // Open in Explorer
            if (ImGui.BeginMenu("Open in Explorer", !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
            {
                if (ImGui.MenuItem("Project Folder"))
                {
                    var projectPath = Smithbox.ProjectRoot;
                    Process.Start("explorer.exe", projectPath);
                }
                UIHelper.ShowHoverTooltip("Open the currently loaded project directory in Windows Explorer.");

                if (ImGui.MenuItem("Game Folder"))
                {
                    var gamePath = Smithbox.GameRoot;
                    Process.Start("explorer.exe", gamePath);
                }
                UIHelper.ShowHoverTooltip("Open the currently loaded game directory in Windows Explorer.");

                if (ImGui.MenuItem("Config Folder"))
                {
                    var configPath = CFG.GetConfigFolderPath();
                    Process.Start("explorer.exe", configPath);
                }
                UIHelper.ShowHoverTooltip("Open the Smithbox config directory in Windows Explorer.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void SettingsDropdown()
    {
        if (ImGui.BeginMenu("Settings"))
        {
            if (ImGui.MenuItem("System"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.System);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Smithbox's systems.");

            if (ImGui.MenuItem("Viewport"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Viewport);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Viewport in Smithbox.");

            if (ImGui.MenuItem("Interface"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Interface);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to interface of Smithbox.");

            if (ImGui.MenuItem("Map Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.MapEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Map Editor in Smithbox.");

            if (ImGui.MenuItem("Model Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ModelEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Model Editor in Smithbox.");

            if (ImGui.MenuItem("Param Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ParamEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Param Editor in Smithbox.");

            if (ImGui.MenuItem("Text Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Text Editor in Smithbox.");

            if (ImGui.MenuItem("Gparam Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.GparamEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Gparam Editor in Smithbox.");

            if (ImGui.MenuItem("Time Act Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TimeActEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Time Act Editor in Smithbox.");

            if (FeatureFlags.EnableEditor_Evemd)
            {
                if (ImGui.MenuItem("EMEVD Editor"))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EmevdEditor);
                }
                UIHelper.ShowHoverTooltip("Open the settings related to Emevd Editor in Smithbox.");
            }

            if (FeatureFlags.EnableEditor_Esd)
            {
                if (ImGui.MenuItem("ESD Editor"))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EsdEditor);
                }
                UIHelper.ShowHoverTooltip("Open the settings related to Esd Editor in Smithbox.");
            }

            if (ImGui.MenuItem("Texture Viewer"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextureViewer);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Texture Viewer in Smithbox.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void AliasDropdown()
    {
        if (ImGui.BeginMenu("Aliases"))
        {
            if (ImGui.MenuItem("Characters"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Characters);
            }
            UIHelper.ShowHoverTooltip("View the character aliases used by this project.");

            if (ImGui.MenuItem("Assets"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Assets);
            }
            UIHelper.ShowHoverTooltip("View the asset aliases used by this project.");

            if (ImGui.MenuItem("Parts"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Parts);
            }
            UIHelper.ShowHoverTooltip("View the part aliases used by this project.");

            if (ImGui.MenuItem("Map Pieces"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapPieces);
            }
            UIHelper.ShowHoverTooltip("View the map piece aliases used by this project.");

            if (ImGui.MenuItem("Gparams"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Gparams);
            }
            UIHelper.ShowHoverTooltip("View the gparam aliases used by this project.");

            if (ImGui.MenuItem("Event Flags"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_EventFlags);
            }
            UIHelper.ShowHoverTooltip("View the event flag aliases used by this project.");

            if (ImGui.MenuItem("Particles"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Particles);
            }
            UIHelper.ShowHoverTooltip("View the particle aliases used by this project.");

            if (ImGui.MenuItem("Cutscenes"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Cutscenes);
            }
            UIHelper.ShowHoverTooltip("View the cutscene aliases used by this project.");

            if (ImGui.MenuItem("Movies"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Movies);
            }
            UIHelper.ShowHoverTooltip("View the movie aliases used by this project.");

            if (ImGui.MenuItem("Sounds"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Sounds);
            }
            UIHelper.ShowHoverTooltip("View the sound aliases used by this project.");

            if (ImGui.MenuItem("Map Names"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapNames);
            }
            UIHelper.ShowHoverTooltip("View the map aliases used by this project.");

            if (ImGui.MenuItem("Time Acts"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_TimeActs);
            }
            UIHelper.ShowHoverTooltip("View the time act aliases used by this project.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void HelpDropdown()
    {
        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.MenuItem("Articles"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Articles);
            }
            UIHelper.ShowHoverTooltip("View the articles that relate to this project.");

            if (ImGui.MenuItem("Tutorials"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Tutorials);
            }
            UIHelper.ShowHoverTooltip("View the tutorials that relate to this project.");

            if (ImGui.MenuItem("Glossary"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Glossary);
            }
            UIHelper.ShowHoverTooltip("View the glossary that relate to this project.");

            if (ImGui.MenuItem("Mass Edit"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.MassEdit);
            }
            UIHelper.ShowHoverTooltip("View the mass edit help instructions.");

            if (ImGui.MenuItem("Regex"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Regex);
            }
            UIHelper.ShowHoverTooltip("View the regex help instructions.");

            if (ImGui.MenuItem("Links"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Links);
            }
            UIHelper.ShowHoverTooltip("View the community links.");

            if (ImGui.MenuItem("Credits"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Credits);
            }
            UIHelper.ShowHoverTooltip("View the credits.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void KeybindsDropdown()
    {
        if (ImGui.BeginMenu("Keybinds"))
        {
            if (ImGui.MenuItem("Common"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Common);
            }
            UIHelper.ShowHoverTooltip("View the common keybinds shared between all editors.");

            if (ImGui.MenuItem("Viewport"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Viewport);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply to the viewport.");

            if (ImGui.MenuItem("Map Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.MapEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Map Editor.");

            if (ImGui.MenuItem("Model Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ModelEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Model Editor.");

            if (ImGui.MenuItem("Param Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ParamEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Param Editor.");

            if (ImGui.MenuItem("Text Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Text Editor.");

            if (ImGui.MenuItem("Gparam Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.GparamEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Gparam Editor.");

            if (ImGui.MenuItem("Time Act Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TimeActEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Time Act Editor.");

            if (ImGui.MenuItem("Texture Viewer"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextureViewer);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Texture Viewer.");

            ImGui.EndMenu();
        }
    }

    public void DebugDropdown()
    {
        if (CFG.Current.DisplayDebugTools)
        {
            ImGui.Separator();

            if (ImGui.BeginMenu("Debugging"))
            {
                if (ImGui.MenuItem($"Tasks"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.DisplayTaskStatus);
                }
                UIHelper.ShowHoverTooltip("Display on-going tasks.");

                ImGui.Separator();

                if (ImGui.MenuItem($"ImGui Demo"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiDemo);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui demo panel.");

                if (ImGui.MenuItem($"ImGui Metrics"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiMetrics);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui metrics panel.");

                if (ImGui.MenuItem($"ImGui Log"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiLog);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui log panel.");

                if (ImGui.MenuItem($"ImGui Stack Tool"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiStackTool);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui stack tool.");

                if (ImGui.MenuItem($"ImGui Test Panel"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiTestPanel);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui test panel.");

                ImGui.Separator();

                if (ImGui.MenuItem($"PARAMDEF Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateParamdef);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"MSB Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateMSB);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"TAE Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateTAE);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                ImGui.Separator();

                if (ImGui.MenuItem($"FLVER Layout Helper"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.FlverLayoutHelper);
                }
                UIHelper.ShowHoverTooltip("Display the helper.");

                ImGui.Separator();

                if (ImGui.MenuItem($"Test: MSBE Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBE_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: MSB_AC6 Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSB_AC6_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: BTL Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_BTL_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: FLVER2 Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_FLVER2_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: Unique Param Row IDs"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_UniqueParamRowIDs);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                ImGui.EndMenu();
            }
        }
    }

    public void SmithboxUpdateButton()
    {
        // Program Update
        if (Smithbox._programUpdateAvailable)
        {
            ImGui.Separator();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);
            if (ImGui.Button("Update Available"))
            {
                Process myProcess = new();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = Smithbox._releaseUrl;
                myProcess.Start();
            }

            ImGui.PopStyleColor();
        }

    }
    private bool MayChangeProject()
    {
        if (TaskManager.AnyActiveTasks())
        {
            return false;
        }

        // Add async stuff here that doesn't directly use the TaskManager system
        if (Smithbox.EditorHandler.MapEditor.MapQueryHandler.UserLoadedData && !Smithbox.EditorHandler.MapEditor.MapQueryHandler.Bank.MapBankInitialized)
        {
            return false;
        }

        return true;
    }

}
