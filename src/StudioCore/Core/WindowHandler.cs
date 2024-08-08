using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Interface.Windows;
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
    public SettingsWindow SettingsWindow;
    public HelpWindow HelpWindow;
    public DebugWindow DebugWindow;
    public KeybindWindow KeybindWindow;

    public WindowHandler(IGraphicsContext _context)
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
    }

    public void HandleWindowShortcuts()
    {
        // Shortcut: Open Settings Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Settings))
        {
            SettingsWindow.ToggleMenuVisibility();
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
    }

    public void HandleWindowIconBar()
    {
        if (ImGui.Button($"{ForkAwesome.Cogs}##SettingsWindow"))
        {
            SettingsWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"Configuration\n{KeyBindings.Current.ToggleWindow_Settings.HintText}");

        if (ImGui.Button($"{ForkAwesome.Book}##HelpWindow"))
        {
            HelpWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"Help\n{KeyBindings.Current.ToggleWindow_Help.HintText}");

        if (ImGui.Button($"{ForkAwesome.KeyboardO}##KeybindWindow"))
        {
            KeybindWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"Keybinds\n{KeyBindings.Current.ToggleWindow_Keybind.HintText}");

        if (FeatureFlags.DebugMenu)
        {
            if (ImGui.Button($"{ForkAwesome.LightbulbO}##DebugWindow"))
            {
                DebugWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Debug");
        }
    }
}
