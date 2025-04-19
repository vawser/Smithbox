using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Configuration.Help;
using StudioCore.Configuration.Keybinds;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Tools.Development;
using System.Diagnostics;
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

    public CommonMenubarHandler(IGraphicsContext _context)
    {
        SettingsWindow = new SettingsWindow();
        HelpWindow = new HelpWindow();
        DebugWindow = new DebugWindow();
        KeybindWindow = new KeybindWindow();
    }

    public void OnGui()
    {
        SettingsWindow.Display();
        HelpWindow.Display();
        DebugWindow.Display();
        KeybindWindow.Display();

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
                    //Smithbox.ProjectHandler.ClearProject();
                    //Smithbox.ProjectHandler.IsInitialLoad = true;
                }
            }
            UIHelper.Tooltip("Create a new project.");

            // Open Project
            if (ImGui.MenuItem("Open Project"))
            {
                if (MayChangeProject())
                {
                    //Smithbox.ProjectHandler.OpenProjectDialog();
                }
            }
            UIHelper.Tooltip("Open and load an existing project.");

            // Project Configuration
            if (ImGui.MenuItem("Project Configuration"))
            {
                ProjectConfigurationWindow.ToggleWindow();
            }
            UIHelper.Tooltip("View the status details of your currently loaded project.");

            // Project Enums
            if (ImGui.MenuItem("Project Enums"))
            {
                ProjectEnumWindow.ToggleWindow();
            }
            UIHelper.Tooltip("View the enums associated with your currently loaded project.");

            // Project Settings
            if (ImGui.MenuItem("Project Settings"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Project);
            }
            UIHelper.Tooltip("Open the settings related to the Project.");

            ImGui.Separator();

            // Recent Projects
            if (ImGui.BeginMenu("Recent Projects", MayChangeProject() && CFG.Current.RecentProjects.Count > 0))
            {
                //Smithbox.ProjectHandler.DisplayRecentProjects();

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Quick-load existing projects that have been recently loaded.");

            // Open in Explorer
            if (ImGui.BeginMenu("Open in Explorer", !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
            {
                if (ImGui.MenuItem("Project Folder"))
                {
                   // var projectPath = Smithbox.ProjectRoot;
                   // Process.Start("explorer.exe", projectPath);
                }
                UIHelper.Tooltip("Open the currently loaded project directory in Windows Explorer.");

                if (ImGui.MenuItem("Game Folder"))
                {
                    //var gamePath = Smithbox.GameRoot;
                    //Process.Start("explorer.exe", gamePath);
                }
                UIHelper.Tooltip("Open the currently loaded game directory in Windows Explorer.");

                if (ImGui.MenuItem("Config Folder"))
                {
                    var configPath = CFG.GetConfigFolderPath();
                    Process.Start("explorer.exe", configPath);
                }
                UIHelper.Tooltip("Open the Smithbox config directory in Windows Explorer.");

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
            UIHelper.Tooltip("Open the settings related to Smithbox's systems.");

            if (ImGui.MenuItem("Viewport"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Viewport);
            }
            UIHelper.Tooltip("Open the settings related to Viewport in Smithbox.");

            if (ImGui.MenuItem("Interface"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.Interface);
            }
            UIHelper.Tooltip("Open the settings related to interface of Smithbox.");

            if (ImGui.MenuItem("Map Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.MapEditor);
            }
            UIHelper.Tooltip("Open the settings related to Map Editor in Smithbox.");

            if (ImGui.MenuItem("Model Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ModelEditor);
            }
            UIHelper.Tooltip("Open the settings related to Model Editor in Smithbox.");

            if (ImGui.MenuItem("Param Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ParamEditor);
            }
            UIHelper.Tooltip("Open the settings related to Param Editor in Smithbox.");

            if (ImGui.MenuItem("Text Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextEditor);
            }
            UIHelper.Tooltip("Open the settings related to Text Editor in Smithbox.");

            if (ImGui.MenuItem("Gparam Editor"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.GparamEditor);
            }
            UIHelper.Tooltip("Open the settings related to Gparam Editor in Smithbox.");

            if (CFG.Current.EnableEditor_TAE)
            {
                if (ImGui.MenuItem("Time Act Editor"))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.TimeActEditor);
                }
                UIHelper.Tooltip("Open the settings related to Time Act Editor in Smithbox.");
            }

            if (CFG.Current.EnableEditor_EMEVD)
            {
                if (ImGui.MenuItem("EMEVD Editor"))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EmevdEditor);
                }
                UIHelper.Tooltip("Open the settings related to Emevd Editor in Smithbox.");
            }

            if (CFG.Current.EnableEditor_ESD)
            {
                if (ImGui.MenuItem("ESD Editor"))
                {
                    SettingsWindow.ToggleWindow(SelectedSettingTab.EsdEditor);
                }
                UIHelper.Tooltip("Open the settings related to Esd Editor in Smithbox.");
            }

            if (ImGui.MenuItem("Texture Viewer"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.TextureViewer);
            }
            UIHelper.Tooltip("Open the settings related to Texture Viewer in Smithbox.");

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
            UIHelper.Tooltip("View the character aliases used by this project.");

            if (ImGui.MenuItem("Assets"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Assets);
            }
            UIHelper.Tooltip("View the asset aliases used by this project.");

            if (ImGui.MenuItem("Parts"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Parts);
            }
            UIHelper.Tooltip("View the part aliases used by this project.");

            if (ImGui.MenuItem("Map Pieces"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapPieces);
            }
            UIHelper.Tooltip("View the map piece aliases used by this project.");

            if (ImGui.MenuItem("Gparams"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Gparams);
            }
            UIHelper.Tooltip("View the gparam aliases used by this project.");

            if (ImGui.MenuItem("Event Flags"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_EventFlags);
            }
            UIHelper.Tooltip("View the event flag aliases used by this project.");

            if (ImGui.MenuItem("Particles"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Particles);
            }
            UIHelper.Tooltip("View the particle aliases used by this project.");

            if (ImGui.MenuItem("Cutscenes"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Cutscenes);
            }
            UIHelper.Tooltip("View the cutscene aliases used by this project.");

            if (ImGui.MenuItem("Movies"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Movies);
            }
            UIHelper.Tooltip("View the movie aliases used by this project.");

            if (ImGui.MenuItem("Sounds"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_Sounds);
            }
            UIHelper.Tooltip("View the sound aliases used by this project.");

            if (ImGui.MenuItem("Map Names"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_MapNames);
            }
            UIHelper.Tooltip("View the map aliases used by this project.");

            if (ImGui.MenuItem("Time Acts"))
            {
                SettingsWindow.ToggleWindow(SelectedSettingTab.ProjectAliases_TimeActs);
            }
            UIHelper.Tooltip("View the time act aliases used by this project.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void HelpDropdown()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.MenuItem("Articles"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Articles);
            }
            UIHelper.Tooltip("View the articles that relate to this project.");

            if (ImGui.MenuItem("Tutorials"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Tutorials);
            }
            UIHelper.Tooltip("View the tutorials that relate to this project.");

            if (ImGui.MenuItem("Glossary"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Glossary);
            }
            UIHelper.Tooltip("View the glossary that relate to this project.");

            if (ImGui.MenuItem("Mass Edit"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.MassEdit);
            }
            UIHelper.Tooltip("View the mass edit help instructions.");

            if (ImGui.MenuItem("Regex"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Regex);
            }
            UIHelper.Tooltip("View the regex help instructions.");

            if (ImGui.MenuItem("Links"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Links);
            }
            UIHelper.Tooltip("View the community links.");

            if (ImGui.MenuItem("Credits"))
            {
                HelpWindow.ToggleWindow(SelectedHelpTab.Credits);
            }
            UIHelper.Tooltip("View the credits.");

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
            UIHelper.Tooltip("View the common keybinds shared between all editors.");

            if (ImGui.MenuItem("Viewport"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.Viewport);
            }
            UIHelper.Tooltip("View the keybinds that apply to the viewport.");

            if (ImGui.MenuItem("Map Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.MapEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Map Editor.");

            if (ImGui.MenuItem("Model Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ModelEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Model Editor.");

            if (ImGui.MenuItem("Param Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.ParamEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Param Editor.");

            if (ImGui.MenuItem("Text Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Text Editor.");

            if (ImGui.MenuItem("Gparam Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.GparamEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Gparam Editor.");

            if (ImGui.MenuItem("Time Act Editor"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TimeActEditor);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Time Act Editor.");

            if (ImGui.MenuItem("Texture Viewer"))
            {
                KeybindWindow.ToggleWindow(SelectedKeybindTab.TextureViewer);
            }
            UIHelper.Tooltip("View the keybinds that apply when in the Texture Viewer.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void DebugDropdown()
    {
        if (CFG.Current.DisplayDebugTools)
        {
            if (ImGui.BeginMenu("Debugging"))
            {
                if (ImGui.MenuItem($"Tasks"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.DisplayTaskStatus);
                }
                UIHelper.Tooltip("Display on-going tasks.");

                ImGui.Separator();

                if (ImGui.MenuItem($"ImGui Demo"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiDemo);
                }
                UIHelper.Tooltip("Display the ImGui demo panel.");

                if (ImGui.MenuItem($"ImGui Metrics"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiMetrics);
                }
                UIHelper.Tooltip("Display the ImGui metrics panel.");

                if (ImGui.MenuItem($"ImGui Log"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiLog);
                }
                UIHelper.Tooltip("Display the ImGui log panel.");

                if (ImGui.MenuItem($"ImGui Stack Tool"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiStackTool);
                }
                UIHelper.Tooltip("Display the ImGui stack tool.");

                if (ImGui.MenuItem($"ImGui Test Panel"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ImGuiTestPanel);
                }
                UIHelper.Tooltip("Display the ImGui test panel.");

                ImGui.Separator();

                if (ImGui.MenuItem($"PARAMDEF Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateParamdef);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"MSB Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateMSB);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"TAE Validation"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.ValidateTAE);
                }
                UIHelper.Tooltip("Display the test panel.");

                ImGui.Separator();

                if (ImGui.MenuItem($"FMG Ref Helper"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.FmgRefPrint);
                }
                UIHelper.Tooltip("Display the helper.");

                if (ImGui.MenuItem($"FLVER Layout Helper"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.FlverDumpHelper);
                }
                UIHelper.Tooltip("Display the helper.");

                ImGui.Separator();

                if (ImGui.MenuItem($"Test: MSBE Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBE_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: MSB_AC6 Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSB_AC6_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: MSBFA Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBFA_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: MSBV Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBV_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: MSBVD Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_MSBVD_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: BTL Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_BTL_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: FLVER2 Byte-Perfect Write"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_FLVER2_BytePerfect);
                }
                UIHelper.Tooltip("Display the test panel.");

                if (ImGui.MenuItem($"Test: Unique Param Row IDs"))
                {
                    DebugWindow.ToggleWindow(SelectedDebugTab.Test_UniqueParamRowIDs);
                }
                UIHelper.Tooltip("Display the test panel.");

                ImGui.EndMenu();
            }

            ImGui.Separator();
        }
    }

    public void SmithboxUpdateButton()
    {
        //// Program Update
        //if (Smithbox._programUpdateAvailable)
        //{
        //    ImGui.Separator();

        //    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);
        //    if (ImGui.Button("Update Available"))
        //    {
        //        Process myProcess = new();
        //        myProcess.StartInfo.UseShellExecute = true;
        //        myProcess.StartInfo.FileName = Smithbox._releaseUrl;
        //        myProcess.Start();
        //    }

        //    ImGui.PopStyleColor();
        //}

    }
    private bool MayChangeProject()
    {
        if (TaskManager.AnyActiveTasks())
        {
            return false;
        }

        // If vanilla bank is in the process of loading, don't allow switch
        if (TextBank.VanillaBankLoading && !TextBank.VanillaBankLoaded)
        {
            return false;
        }

        // Add async stuff here that doesn't directly use the TaskManager system
        //if (Smithbox.EditorHandler.MapEditor.MapQueryHandler.UserLoadedData && !Smithbox.EditorHandler.MapEditor.MapQueryHandler.Bank.MapBankInitialized)
        //{
        //    return false;
        //}

        return true;
    }

}
