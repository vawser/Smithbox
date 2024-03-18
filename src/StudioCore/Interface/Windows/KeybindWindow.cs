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

        KeybindSection("Core", binds, KeybindCategory.Core, 0);
        KeybindSection("Window", binds, KeybindCategory.Window, 1);
        KeybindSection("Viewport", binds, KeybindCategory.Viewport, 2);
        KeybindSection("Map Editor", binds, KeybindCategory.MapEditor, 3);
        KeybindSection("Model Editor", binds, KeybindCategory.ModelEditor, 4);
        KeybindSection("Param Editor", binds, KeybindCategory.ParamEditor, 5);
        KeybindSection("Text Editor", binds, KeybindCategory.TextEditor, 6);

        if (ImGui.BeginTabItem($"Defaults"))
        {
            if (ImGui.Button("Restore defaults"))
                KeyBindings.ResetKeyBinds();

            ImGui.EndTabItem();
        }
    }

    public void KeybindSection(string title, FieldInfo[] binds, KeybindCategory keyCategory, int idx)
    {
        if (ImGui.BeginTabItem($"{title}##KeyBind{title}{idx}"))
        {
            ImGui.Columns(2);

            foreach (FieldInfo bind in binds)
            {
                KeyBind bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);

                if (bindVal.KeyCategory == keyCategory)
                {
                    KeybindTitle(bind, bindVal, keyCategory, idx);
                }
            }

            ImGui.NextColumn();

            foreach (FieldInfo bind in binds)
            {
                KeyBind bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);

                if (bindVal.KeyCategory == keyCategory)
                {
                    KeybindEntry(bind, bindVal, keyCategory, idx);
                }
            }

            ImGui.Columns(1);

            ImGui.EndTabItem();
        }
    }

    public void KeybindTitle(FieldInfo bind, KeyBind bindVal, KeybindCategory keyCategory, int idx)
    {
        var name = bindVal.PresentationName;
        if (bindVal.PresentationName == null)
        {
            name = "";
        }
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{name}");
    }

    public void KeybindEntry(FieldInfo bind, KeyBind bindVal, KeybindCategory keyCategory, int idx)
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
        else if (ImGui.Button($"{keyText}##{bind.Name}{keyCategory}{idx}"))
            _currentKeyBind = bindVal;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#KeybindsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            DisplaySettings_Keybinds();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
