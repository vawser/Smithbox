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

    public void HandleWindowShortcuts()
    {

    }

    public void HandleWindowBar()
    {
        // Project
        if (ImGui.BeginMenu("Project"))
        {
            // New Project
            if (ImGui.Button("New Project", UI.MenuButtonSize))
            {
                if (MayChangeProject())
                {
                    Smithbox.ProjectHandler.ClearProject();
                    Smithbox.ProjectHandler.IsInitialLoad = true;
                }
            }
            UIHelper.ShowHoverTooltip("Create a new project.");

            // Open Project
            if (ImGui.Button("Open Project", UI.MenuButtonSize))
            {
                if (MayChangeProject())
                {
                    Smithbox.ProjectHandler.OpenProjectDialog();
                }
            }
            UIHelper.ShowHoverTooltip("Open and load an existing project.");

            // Project Configuration
            if (ImGui.Button("Project Configuration", UI.MenuButtonSize))
            {
                ProjectConfigurationWindow.ToggleWindow();
            }
            UIHelper.ShowHoverTooltip("View the status details of your currently loaded project.");

            // Project Enums
            if (ImGui.Button("Project Enums", UI.MenuButtonSize))
            {
                ProjectEnumWindow.ToggleWindow();
            }
            UIHelper.ShowHoverTooltip("View the enums associated with your currently loaded project.");

            // Project Settings
            if (ImGui.Button("Project Settings", UI.MenuButtonSize))
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
                if (ImGui.Button("Project Folder", UI.MenuButtonSize))
                {
                    var projectPath = Smithbox.ProjectRoot;
                    Process.Start("explorer.exe", projectPath);
                }
                UIHelper.ShowHoverTooltip("Open the currently loaded project directory in Windows Explorer.");

                if (ImGui.Button("Game Folder", UI.MenuButtonSize))
                {
                    var gamePath = Smithbox.GameRoot;
                    Process.Start("explorer.exe", gamePath);
                }
                UIHelper.ShowHoverTooltip("Open the currently loaded game directory in Windows Explorer.");

                if (ImGui.Button("Config Folder", UI.MenuButtonSize))
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

        if (ImGui.BeginMenu("Settings"))
        {
            if (ImGui.Button("System", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.System);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Smithbox's systems.");

            if (ImGui.Button("Viewport", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Viewport);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Viewport in Smithbox.");

            if (ImGui.Button("Interface", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Interface);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to interface of Smithbox.");

            if (ImGui.Button("Map Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.MapEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Map Editor in Smithbox.");

            if (ImGui.Button("Model Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ModelEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Model Editor in Smithbox.");

            if (ImGui.Button("Param Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ParamEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Param Editor in Smithbox.");

            if (ImGui.Button("Text Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Text Editor in Smithbox.");

            if (ImGui.Button("Gparam Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.GparamEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Gparam Editor in Smithbox.");

            if (ImGui.Button("Time Act Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TimeActEditor);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Time Act Editor in Smithbox.");

            if (FeatureFlags.EnableEditor_Evemd)
            {
                if (ImGui.Button("EMEVD Editor", UI.MenuButtonSize))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EmevdEditor);
                }
                UIHelper.ShowHoverTooltip("Open the settings related to Emevd Editor in Smithbox.");
            }

            if (FeatureFlags.EnableEditor_Esd)
            {
                if (ImGui.Button("ESD Editor", UI.MenuButtonSize))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EsdEditor);
                }
                UIHelper.ShowHoverTooltip("Open the settings related to Esd Editor in Smithbox.");
            }

            if (ImGui.Button("Texture Viewer", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextureViewer);
            }
            UIHelper.ShowHoverTooltip("Open the settings related to Texture Viewer in Smithbox.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Aliases"))
        {
            if (ImGui.Button("Characters", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Characters);
            }
            UIHelper.ShowHoverTooltip("View the character aliases used by this project.");

            if (ImGui.Button("Assets", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Assets);
            }
            UIHelper.ShowHoverTooltip("View the asset aliases used by this project.");

            if (ImGui.Button("Parts", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Parts);
            }
            UIHelper.ShowHoverTooltip("View the part aliases used by this project.");

            if (ImGui.Button("Map Pieces", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapPieces);
            }
            UIHelper.ShowHoverTooltip("View the map piece aliases used by this project.");

            if (ImGui.Button("Gparams", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Gparams);
            }
            UIHelper.ShowHoverTooltip("View the gparam aliases used by this project.");

            if (ImGui.Button("Event Flags", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_EventFlags);
            }
            UIHelper.ShowHoverTooltip("View the event flag aliases used by this project.");

            if (ImGui.Button("Particles", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Particles);
            }
            UIHelper.ShowHoverTooltip("View the particle aliases used by this project.");

            if (ImGui.Button("Cutscenes", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Cutscenes);
            }
            UIHelper.ShowHoverTooltip("View the cutscene aliases used by this project.");

            if (ImGui.Button("Movies", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Movies);
            }
            UIHelper.ShowHoverTooltip("View the movie aliases used by this project.");

            if (ImGui.Button("Sounds", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Sounds);
            }
            UIHelper.ShowHoverTooltip("View the sound aliases used by this project.");

            if (ImGui.Button("Map Names", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapNames);
            }
            UIHelper.ShowHoverTooltip("View the map aliases used by this project.");

            if (ImGui.Button("Time Acts", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_TimeActs);
            }
            UIHelper.ShowHoverTooltip("View the time act aliases used by this project.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.Button("Articles", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Articles);
            }
            UIHelper.ShowHoverTooltip("View the articles that relate to this project.");

            if (ImGui.Button("Tutorials", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Tutorials);
            }
            UIHelper.ShowHoverTooltip("View the tutorials that relate to this project.");

            if (ImGui.Button("Glossary", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Glossary);
            }
            UIHelper.ShowHoverTooltip("View the glossary that relate to this project.");

            if (ImGui.Button("Mass Edit", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.MassEdit);
            }
            UIHelper.ShowHoverTooltip("View the mass edit help instructions.");

            if (ImGui.Button("Regex", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Regex);
            }
            UIHelper.ShowHoverTooltip("View the regex help instructions.");

            if (ImGui.Button("Links", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Links);
            }
            UIHelper.ShowHoverTooltip("View the community links.");

            if (ImGui.Button("Credits", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Credits);
            }
            UIHelper.ShowHoverTooltip("View the credits.");

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Keybinds"))
        {
            if (ImGui.Button("Common", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Common);
            }
            UIHelper.ShowHoverTooltip("View the common keybinds shared between all editors.");

            if (ImGui.Button("Viewport", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Viewport);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply to the viewport.");

            if (ImGui.Button("Map Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.MapEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Map Editor.");

            if (ImGui.Button("Model Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ModelEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Model Editor.");

            if (ImGui.Button("Param Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ParamEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Param Editor.");

            if (ImGui.Button("Text Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Text Editor.");

            if (ImGui.Button("Gparam Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.GparamEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Gparam Editor.");

            if (ImGui.Button("Time Act Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TimeActEditor);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Time Act Editor.");

            if (ImGui.Button("Texture Viewer", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextureViewer);
            }
            UIHelper.ShowHoverTooltip("View the keybinds that apply when in the Texture Viewer.");

            ImGui.EndMenu();
        }

        if (CFG.Current.DisplayDebugTools)
        {
            ImGui.Separator();

            if (ImGui.BeginMenu("Debugging"))
            {
                if (ImGui.Button($"Tasks", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.DisplayTaskStatus);
                }
                UIHelper.ShowHoverTooltip("Display on-going tasks.");

                ImGui.Separator();

                if (ImGui.Button($"ImGui Demo", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiDemo);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui demo panel.");

                if (ImGui.Button($"ImGui Metrics", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiMetrics);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui metrics panel.");

                if (ImGui.Button($"ImGui Log", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiLog);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui log panel.");

                if (ImGui.Button($"ImGui Stack Tool", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiStackTool);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui stack tool.");

                if (ImGui.Button($"ImGui Test Panel", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiTestPanel);
                }
                UIHelper.ShowHoverTooltip("Display the ImGui test panel.");

                ImGui.Separator();

                if (ImGui.Button($"PARAMDEF Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateParamdef);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"MSB Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateMSB);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"TAE Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateTAE);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                ImGui.Separator();

                if (ImGui.Button($"FLVER Layout Helper", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.FlverLayoutHelper);
                }
                UIHelper.ShowHoverTooltip("Display the helper.");

                ImGui.Separator();

                if (ImGui.Button($"Test: MSBE Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBE_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"Test: MSB_AC6 Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSB_AC6_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"Test: BTL Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_BTL_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"Test: FLVER2 Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_FLVER2_BytePerfect);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                if (ImGui.Button($"Test: Unique Param Row IDs", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_UniqueParamRowIDs);
                }
                UIHelper.ShowHoverTooltip("Display the test panel.");

                ImGui.EndMenu();
            }
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
