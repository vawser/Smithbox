using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Configuration.Help;
using StudioCore.Configuration.Keybinds;
using StudioCore.Configuration.Settings;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Settings;
using StudioCore.Tools.Development;
using StudioCore.Tools.Randomiser;
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
    public RandomiserWindow RandomiserWindow;

    public WindowHandler(IGraphicsContext _context)
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
    }

    public void HandleWindowShortcuts()
    {
        // Shortcut: Open Settings Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_ConfigurationWindow))
        {
            SettingsWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Help Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_HelpWindow))
        {
            HelpWindow.ToggleMenuVisibility();
        }

        // Shortcut: Open Keybind Window
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_KeybindConfigWindow))
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
        ImguiUtils.ShowHoverTooltip($"Configuration\n{KeyBindings.Current.CORE_ConfigurationWindow.HintText}");

        if (ImGui.Button($"{ForkAwesome.Book}##HelpWindow"))
        {
            HelpWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"Help\n{KeyBindings.Current.CORE_HelpWindow.HintText}");

        if (ImGui.Button($"{ForkAwesome.KeyboardO}##KeybindWindow"))
        {
            KeybindWindow.ToggleMenuVisibility();
        }
        ImguiUtils.ShowHoverTooltip($"Keybinds\n{KeyBindings.Current.CORE_KeybindConfigWindow.HintText}");


        if (CFG.Current.DisplayRandomiserTools)
        {
            if (ImGui.Button($"{ForkAwesome.Random}##RandomiserWindow"))
            {
                RandomiserWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Randomiser Tools");
        }

        if (CFG.Current.DisplayDebugTools)
        {
            if (ImGui.Button($"{ForkAwesome.LightbulbO}##DebugWindow"))
            {
                DebugWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Debug Tools");
        }
    }
}
