using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Interface.Windows;
using StudioCore.Localization;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

/// <summary>
/// Handler class that holds all of the floating windows and related window state for access elsewhere.
/// </summary>
public class WindowHandler
{
    public ProjectWindow ProjectWindow;
    public AliasWindow AliasWindow;
    public SettingsWindow SettingsWindow;
    public HelpWindow HelpWindow;
    public DebugWindow DebugWindow;
    public KeybindWindow KeybindWindow;
    public MemoryWindow MemoryWindow;
    public ToolWindow ToolWindow;

    public WindowHandler(IGraphicsContext _context)
    {
        ProjectWindow = new ProjectWindow();
        AliasWindow = new AliasWindow();
        SettingsWindow = new SettingsWindow();
        HelpWindow = new HelpWindow();
        DebugWindow = new DebugWindow();
        KeybindWindow = new KeybindWindow();
        MemoryWindow = new MemoryWindow();
        ToolWindow = new ToolWindow();

        MemoryWindow._activeView = Smithbox.EditorHandler.ParamEditor._activeView;
    }

    public void OnGui()
    {
        ProjectWindow.Display();
        AliasWindow.Display();
        SettingsWindow.Display();
        HelpWindow.Display();
        DebugWindow.Display();
        KeybindWindow.Display();
        MemoryWindow.Display();
        ToolWindow.Display();
    }

    public void HandleWindowShortcuts()
    {
        // Shortcut: Open Project Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Project))
        {
            ProjectWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Help Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Help))
        {
            HelpWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Keybind Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Keybind))
        {
            KeybindWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Memory Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Memory))
        {
            MemoryWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Settings Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Settings))
        {
            SettingsWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Alias Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Alias))
        {
            AliasWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Color Picker Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_QuickTools))
        {
            ToolWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Debug Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Debug))
        {
            DebugWindow.ToggleMenuVisibility();
        }
    }

    public void HandleWindowIconBar()
    {
        if (ImGui.Button($"{ForkAwesome.Wrench}##ProjectWindow"))
        {
            ProjectWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__PROJECT")}" + $"\n{KeyBindings.Current.ToggleWindow_Project.HintText}");


        if (ImGui.Button($"{ForkAwesome.FileText}##AliasWindow"))
        {
            AliasWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__ALIAS")}" + $"\n{KeyBindings.Current.ToggleWindow_Alias.HintText}");

        if (ImGui.Button($"{ForkAwesome.Book}##HelpWindow"))
        {
            HelpWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__HELP")}" + $"\n{KeyBindings.Current.ToggleWindow_Help.HintText}");

        if (ImGui.Button($"{ForkAwesome.Cogs}##SettingsWindow"))
        {
            SettingsWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__SETTINGS")}" + $"\n{KeyBindings.Current.ToggleWindow_Settings.HintText}");

        if (ImGui.Button($"{ForkAwesome.KeyboardO}##KeybindWindow"))
        {
            KeybindWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__KEYBINDS")}" + $"\n{KeyBindings.Current.ToggleWindow_Keybind.HintText}");

        if (ImGui.Button($"{ForkAwesome.Database}##MemoryWindow"))
        {
            MemoryWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__MEMORY")}" + $"\n{KeyBindings.Current.ToggleWindow_Memory.HintText}");

        if (ImGui.Button($"{ForkAwesome.Briefcase}##QuickToolWindow"))
        {
            ToolWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__TOOLS")}" + $"\n{KeyBindings.Current.ToggleWindow_QuickTools.HintText}");

        if (FeatureFlags.DebugMenu)
        {
            if (ImGui.Button($"{ForkAwesome.Bell}##DebugWindow"))
            {
                DebugWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"{LOC.Get("WINDOW_HANDLER__DEBUG")}" + $"\n{KeyBindings.Current.ToggleWindow_Debug.HintText}");
        }
    }
}
