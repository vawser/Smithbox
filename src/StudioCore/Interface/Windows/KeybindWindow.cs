using ImGuiNET;
using Octokit;
using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Interface.Windows;
public class KeybindWindow
{
    private KeyBind _currentKeyBind;
    public bool MenuOpenState;

    public KeybindWindow()
    {
    }

    public void SaveSettings()
    {
        CFG.Save();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private void DisplaySettings_Keybinds()
    {
        if (ImGui.IsAnyItemActive())
            _currentKeyBind = null;

        FieldInfo[] binds = KeyBindings.Current.GetType().GetFields();

        KeybindSection("Core", binds, Category.Core);
        KeybindSection("Window", binds, Category.Window);
        KeybindSection("Viewport", binds, Category.Viewport);
        KeybindSection("Map Editor", binds, Category.MapEditor);
        KeybindSection("Param Editor", binds, Category.ParamEditor);
        KeybindSection("Text Editor", binds, Category.TextEditor);

        if (ImGui.BeginTabItem($"Defaults"))
        {
            if (ImGui.Button("Restore defaults"))
                KeyBindings.ResetKeyBinds();

            ImGui.EndTabItem();
        }
    }

    public void KeybindSection(string title, FieldInfo[] binds, Category keyCategory)
    {
        if (ImGui.BeginTabItem($"{title}"))
        {
            ImGui.Columns(2);

            foreach (FieldInfo bind in binds)
            {
                KeyBind bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);

                if (bindVal.KeyCategory == keyCategory)
                {
                    KeybindTitle(bind, bindVal);
                }
            }

            ImGui.NextColumn();

            foreach (FieldInfo bind in binds)
            {
                KeyBind bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);

                if (bindVal.KeyCategory == keyCategory)
                {
                    KeybindEntry(bind, bindVal);
                }
            }

            ImGui.Columns(1);

            ImGui.EndTabItem();
        }
    }

    public void KeybindTitle(FieldInfo bind, KeyBind bindVal)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(bindVal.PresentationName);
    }

    public void KeybindEntry(FieldInfo bind, KeyBind bindVal)
    {
        var keyText = bindVal.HintText;
        if (keyText == "")
            keyText = "[None]";

        if (_currentKeyBind == bindVal)
        {
            ImGui.Button("Press Key <Esc - Clear>");
            if (InputTracker.GetKeyDown(Key.Escape))
            {
                bind.SetValue(KeyBindings.Current, new KeyBind());
                _currentKeyBind = null;
            }
            else
            {
                KeyBind newkey = InputTracker.GetNewKeyBind(bindVal.PresentationName, bindVal.KeyCategory);
                if (newkey != null)
                {
                    bind.SetValue(KeyBindings.Current, newkey);
                    _currentKeyBind = null;
                }
            }
        }
        else if (ImGui.Button($"{keyText}##{bind.Name}"))
            _currentKeyBind = bindVal;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#KeybindsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.6f, 0.4f));
            ImGui.PushItemWidth(300f);

            DisplaySettings_Keybinds();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }
}
