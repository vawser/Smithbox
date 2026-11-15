using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.SettingsEditor;

/// <summary>
/// Help, Preferences and Shortcuts side by side, shown from the last button on the editor rail.
/// </summary>
/// <remarks>
/// Not one of a project's editors: it is always on the rail, with or without a project loaded. The Preferences and
/// Shortcuts panes draw the same <see cref="Preferences.PreferencesMenu"/> and <see cref="Keybinds.KeybindsMenu"/>
/// the floating windows used, which the updater and developer kit still show from their menu bar.
/// </remarks>
public class SettingsEditor
{
    private bool _wasShownLastFrame;

    /// <summary>
    /// Draws the editor over the editor host area. Called only while it is the shown editor.
    /// </summary>
    public void OnGUI()
    {
        var flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        uint dockspaceId = 0;

        if (EditorHost.Begin($"{LOC.Get("SETTINGS_Editor_Title")}###settingsEditor", flags))
        {
            DisplayMenuBar();

            dockspaceId = ImGui.GetID("DockSpace_SettingsEditor");

            BuildDefaultLayout(dockspaceId);

            ImGui.DockSpace(dockspaceId, Vector2.Zero, ImGuiDockNodeFlags.None, ref GUI.DockGroup_SettingsEditor);
        }

        ImGui.End();

        if (dockspaceId == 0)
            return;

        // Help
        if (BeginPane(LOC.Get("PROGRAM_Menu_Header_Help"), HelpWindowId, dockspaceId))
        {
            DisplayHelp();
        }

        ImGui.End();

        // Preferences
        if (BeginPane(LOC.Get("PREF_Window_Preferences"), PreferencesWindowId, dockspaceId))
        {
            Smithbox.Instance.PreferencesMenu.DrawContents();
        }

        ImGui.End();

        // Shortcuts
        if (BeginPane(LOC.Get("INPUT_Window_Shortcuts"), ShortcutsWindowId, dockspaceId))
        {
            Smithbox.Instance.KeybindsMenu.DrawContents();
        }

        ImGui.End();
    }

    /// <summary>
    /// The editor's menu bar, holding the menus the Preferences, Shortcuts and Help windows had
    /// </summary>
    private void DisplayMenuBar()
    {
        if (!ImGui.BeginMenuBar())
            return;

        var preferences = Smithbox.Instance.PreferencesMenu;
        var shortcuts = Smithbox.Instance.KeybindsMenu;

        // File
        if (ImGui.BeginMenu($"{LOC.Get("SETTINGS_Menu_File")}##settingsFileMenu"))
        {
            if (ImGui.MenuItem($"{LOC.Get("SETTINGS_Action_Save_Preferences")}##savePreferencesAction"))
            {
                preferences.SavePreferences();
            }

            if (ImGui.MenuItem($"{LOC.Get("SETTINGS_Action_Save_Shortcuts")}##saveShortcutsAction"))
            {
                shortcuts.SaveAll();
            }

            ImGui.Separator();

            preferences.DisplayRevertItem(LOC.Get("SETTINGS_Action_Revert_Preferences"));
            shortcuts.DisplayRevertAllItem(LOC.Get("SETTINGS_Action_Revert_Shortcuts"));

            ImGui.EndMenu();
        }

        // Developer, from the Help menu
        if (CFG.Current.Developer_Enable_Tools &&
            ImGui.BeginMenu($"{LOC.Get("SETTINGS_Menu_Developer")}##developerMenuHeader"))
        {
            Smithbox.Instance.DeveloperPanel.DisplayDropdown();

            ImGui.EndMenu();
        }

        ImGui.EndMenuBar();
    }

    // The pane windows' ids; their titles are localized, so only these parts are used to find them
    private const string HelpWindowId = "###settingsHelp";
    private const string PreferencesWindowId = "###settingsPreferences";
    private const string ShortcutsWindowId = "###settingsShortcuts";

    /// <summary>
    /// Splits the dockspace into Help | Preferences | Shortcuts the first time it is shown. Once the layout exists
    /// (built here, or as the user rearranged it and imgui.ini kept it) it is left alone.
    /// </summary>
    private static unsafe void BuildDefaultLayout(uint dockspaceId)
    {
        if (ImGuiP.DockBuilderGetNode(dockspaceId).Handle != null)
            return;

        ImGuiP.DockBuilderAddNode(dockspaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, ImGui.GetContentRegionAvail());

        uint helpId;
        uint restId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.25f, &helpId, &restId);

        // Of the remaining 75%: Shortcuts takes 35% of the whole, Preferences the 40% left
        uint shortcutsId;
        uint preferencesId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.35f / 0.75f, &shortcutsId, &preferencesId);

        ImGuiP.DockBuilderDockWindow(HelpWindowId, helpId);
        ImGuiP.DockBuilderDockWindow(PreferencesWindowId, preferencesId);
        ImGuiP.DockBuilderDockWindow(ShortcutsWindowId, shortcutsId);

        ImGuiP.DockBuilderFinish(dockspaceId);
    }

    /// <summary>
    /// Saves the preferences and bindings when the editor stops being shown, as closing their windows used to.
    /// Called every frame.
    /// </summary>
    public void Update(bool shown)
    {
        if (!shown && _wasShownLastFrame)
        {
            Smithbox.Instance.PreferencesMenu.SaveAll();
            Smithbox.Instance.KeybindsMenu.SaveAll();
        }

        _wasShownLastFrame = shown;
    }

    /// <summary>
    /// Begins a pane: a window that docks only into the Settings Editor's dockspace. If it has nowhere to be (no
    /// saved layout and the default one not built) it joins the dockspace. Always pair with
    /// <see cref="ImGui.End"/>.
    /// </summary>
    private static bool BeginPane(string title, string windowId, uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_SettingsEditor);

        return ImGui.Begin($"{title}{windowId}", GUI.GetInnerWindowFlags());
    }

    private void DisplayHelp()
    {
        ImGui.Text(LOC.Get("HELP_Current_Version", Smithbox.Instance._version));

        ImGui.Separator();

        ImGui.Text(LOC.Get("HELP_Developed_By"));

        ImGui.Separator();

        var buttonWidth = new Vector2(-1, 0);

        // Go to Wiki
        if (ImGui.Button($"{LOC.Get("HELP_Action_Go_To_Wiki")}##wikiAction", buttonWidth))
        {
            OpenLink(LOC.Get("HELP_Wiki_Link"));
        }
        GUI.Tooltip(LOC.Get("HELP_Action_Go_To_Wiki_TT"));

        // Go to Github Repository
        if (ImGui.Button($"{LOC.Get("HELP_Action_Go_To_Github")}##githubAction", buttonWidth))
        {
            OpenLink(LOC.Get("HELP_Github_Link"));
        }
        GUI.Tooltip(LOC.Get("HELP_Action_Go_To_Github_TT"));
    }

    private static void OpenLink(string link)
    {
        Process process = new();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = link;
        process.Start();
    }
}
