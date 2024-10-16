using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Configuration.Help;
using StudioCore.Configuration.Keybinds;
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

            // Open Project
            if (ImGui.Button("Open Project", UI.MenuButtonSize))
            {
                if (MayChangeProject())
                {
                    Smithbox.ProjectHandler.OpenProjectDialog();
                }
            }

            // Project Configuration
            if (ImGui.Button("Project Configuration", UI.MenuButtonSize))
            {
                ProjectConfigurationWindow.ToggleWindow();
            }

            // Project Enums
            if (ImGui.Button("Project Enums", UI.MenuButtonSize))
            {
                ProjectEnumWindow.ToggleWindow();
            }

            ImGui.Separator();

            // Recent Projects
            if (ImGui.BeginMenu("Recent Projects", MayChangeProject() && CFG.Current.RecentProjects.Count > 0))
            {
                Smithbox.ProjectHandler.DisplayRecentProjects();

                ImGui.EndMenu();
            }

            // Open in Explorer
            if (ImGui.BeginMenu("Open in Explorer", !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
            {
                if (ImGui.Button("Project Folder", UI.MenuButtonSize))
                {
                    var projectPath = Smithbox.ProjectRoot;
                    Process.Start("explorer.exe", projectPath);
                }

                if (ImGui.Button("Game Folder", UI.MenuButtonSize))
                {
                    var gamePath = Smithbox.GameRoot;
                    Process.Start("explorer.exe", gamePath);
                }

                if (ImGui.Button("Config Folder", UI.MenuButtonSize))
                {
                    var configPath = CFG.GetConfigFolderPath();
                    Process.Start("explorer.exe", configPath);
                }

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
            if (ImGui.Button("Viewport", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Viewport);
            }
            if (ImGui.Button("Map Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.MapEditor);
            }
            if (ImGui.Button("Model Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ModelEditor);
            }
            if (ImGui.Button("Param Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ParamEditor);
            }
            if (ImGui.Button("Text Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextEditor);
            }
            if (ImGui.Button("Gparam Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.GparamEditor);
            }
            if (ImGui.Button("Time Act Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TimeActEditor);
            }
            if (ImGui.Button("EMEVD Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.EmevdEditor);
            }
            if (ImGui.Button("ESD Editor", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.EsdEditor);
            }
            if (ImGui.Button("Texture Viewer", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextureViewer);
            }
            if (ImGui.Button("Interface", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Interface);
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Aliases"))
        {
            if (ImGui.Button("Characters", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Characters);
            }
            if (ImGui.Button("Assets", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Assets);
            }
            if (ImGui.Button("Parts", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Parts);
            }
            if (ImGui.Button("Map Pieces", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapPieces);
            }
            if (ImGui.Button("Gparams", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Gparams);
            }
            if (ImGui.Button("Event Flags", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_EventFlags);
            }
            if (ImGui.Button("Particles", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Particles);
            }
            if (ImGui.Button("Cutscenes", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Cutscenes);
            }
            if (ImGui.Button("Movies", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Movies);
            }
            if (ImGui.Button("Sounds", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Sounds);
            }
            if (ImGui.Button("Map Names", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapNames);
            }
            if (ImGui.Button("Time Acts", UI.MenuButtonSize))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_TimeActs);
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.Button("Articles", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Articles);
            }
            if (ImGui.Button("Tutorials", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Tutorials);
            }
            if (ImGui.Button("Glossary", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Glossary);
            }
            if (ImGui.Button("Mass Edit", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.MassEdit);
            }
            if (ImGui.Button("Regex", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Regex);
            }
            if (ImGui.Button("Links", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Links);
            }
            if (ImGui.Button("Credits", UI.MenuButtonSize))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Credits);
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Keybinds"))
        {
            if (ImGui.Button("Common", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Common);
            }
            if (ImGui.Button("Viewport", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Viewport);
            }
            if (ImGui.Button("Map Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.MapEditor);
            }
            if (ImGui.Button("Model Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ModelEditor);
            }
            if (ImGui.Button("Param Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ParamEditor);
            }
            if (ImGui.Button("Text Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextEditor);
            }
            if (ImGui.Button("Gparam Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.GparamEditor);
            }
            if (ImGui.Button("Time Act Editor", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TimeActEditor);
            }
            if (ImGui.Button("Texture Viewer", UI.MenuButtonSize))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextureViewer);
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Logger"))
        {
            if (ImGui.Button($"Display", UI.MenuButtonSize))
            {
                TaskLogs.ToggleLoggerVisibility();
            }

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
                if (ImGui.Button($"Resources", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ResourceManager);
                }

                ImGui.Separator();

                if (ImGui.Button($"ImGui Demo", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiDemo);
                }
                if (ImGui.Button($"ImGui Metrics", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiMetrics);
                }
                if (ImGui.Button($"ImGui Log", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiLog);
                }
                if (ImGui.Button($"ImGui Stack Tool", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiStackTool);
                }
                if (ImGui.Button($"ImGui Test Panel", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiTestPanel);
                }

                ImGui.Separator();

                if (ImGui.Button($"PARAMDEF Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateParamdef);
                }
                if (ImGui.Button($"MSB Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateMSB);
                }
                if (ImGui.Button($"TAE Validation", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateTAE);
                }

                ImGui.Separator();

                if (ImGui.Button($"FLVER Layout Helper", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.FlverLayoutHelper);
                }

                ImGui.Separator();

                if (ImGui.Button($"Test: MSBE Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBE_BytePerfect);
                }
                if (ImGui.Button($"Test: MSB_AC6 Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSB_AC6_BytePerfect);
                }
                if (ImGui.Button($"Test: BTL Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_BTL_BytePerfect);
                }
                if (ImGui.Button($"Test: FLVER2 Byte-Perfect Write", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_FLVER2_BytePerfect);
                }
                if (ImGui.Button($"Test: Unique Param Row IDs", UI.MenuButtonSize))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_UniqueParamRowIDs);
                }

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
