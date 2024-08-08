using ImGuiNET;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Interface.Settings;
using StudioCore.Interface.Tabs;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static StudioCore.Interface.Windows.SettingsWindow;

namespace StudioCore.Interface.Windows;
public class KeybindWindow
{
    private KeyBind _currentKeyBind;
    public bool MenuOpenState;

    private CommonKeybindTab CommonKeybinds;
    private ViewportKeybindTab ViewportKeybinds;
    private MapEditorKeybindTab MapEditorKeybinds;
    private ModelEditorKeybindTab ModelEditorKeybinds;
    private ParamEditorKeybindTab ParamEditorKeybinds;
    private TextEditorKeybindTab TextEditorKeybinds;
    private GparamEditorKeybindTab GparamEditorKeybinds;
    private TimeActEditorKeybindTab TimeActEditorKeybinds;
    private TextureViewerKeybindTab TextureViewerKeybinds;

    public KeybindWindow()
    {
        CommonKeybinds = new CommonKeybindTab();
        ViewportKeybinds = new ViewportKeybindTab();
        MapEditorKeybinds = new MapEditorKeybindTab();
        ModelEditorKeybinds = new ModelEditorKeybindTab();
        ParamEditorKeybinds = new ParamEditorKeybindTab();
        TextEditorKeybinds = new TextEditorKeybindTab();
        GparamEditorKeybinds = new GparamEditorKeybindTab();
        TimeActEditorKeybinds = new TimeActEditorKeybindTab();
        TextureViewerKeybinds = new TextureViewerKeybindTab();
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

        KeybindSection("Core", binds, KeybindCategory.Core, 0, "These keybinds are available across all editors.");
        KeybindSection("Window", binds, KeybindCategory.Window, 1, "These keybinds are available across all editors.");
        KeybindSection("Map Editor", binds, KeybindCategory.MapEditor, 2, "These keybinds are only available in the Map Editor.");
        KeybindSection("Model Editor", binds, KeybindCategory.ModelEditor, 3, "These keybinds are only available in the Model Editor.");
        KeybindSection("Param Editor", binds, KeybindCategory.ParamEditor, 4, "These keybinds are only available in the Param Editor.");
        KeybindSection("Text Editor", binds, KeybindCategory.TextEditor, 5, "These keybinds are only available in the Text Editor.");
        KeybindSection("Viewport", binds, KeybindCategory.Viewport, 6, "These keybinds are only available within the Viewport of the Map or Model Editor.");
        KeybindSection("Texture Viewer", binds, KeybindCategory.TextureViewer, 7, "These keybinds are only available within the Texture Viewer.");

        if (ImGui.BeginTabItem($"Defaults"))
        {
            ImGui.Text("This button will reset your shortcuts to the default assignments.");

            if (ImGui.Button("Restore keybinds"))
            {
                KeyBindings.ResetKeyBinds();
            }

            ImGui.EndTabItem();
        }
    }

    public void KeybindSection(string title, FieldInfo[] binds, KeybindCategory keyCategory, int idx, string contextInfo)
    {
        if (ImGui.BeginTabItem($"{title}##KeyBind{title}{idx}"))
        {
            ImGui.Text(contextInfo);

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
        var fixedKey = bindVal.FixedKey;
        var keyText = bindVal.HintText;

        if (!fixedKey)
        {
            if (bindVal.PrimaryKey == Key.Unknown)
                keyText = "[None]";

            if (_currentKeyBind == bindVal)
            {
                ImGui.Button("Press Key <Esc - Clear>");
                if (InputTracker.GetKeyDown(Key.Escape))
                {
                    KeyBind newkey = InputTracker.GetEmptyKeyBind(bindVal.PresentationName, bindVal.KeyCategory);
                    bind.SetValue(KeyBindings.Current, newkey);
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
            {
                _currentKeyBind = bindVal;
            }
        }
        else
        {
            ImGui.Text($"{keyText}");
        }
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
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##KeybindWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("keybindTabList");

            var arr = Enum.GetValues(typeof(SelectedKeybindTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedKeybindTab)arr.GetValue(i);

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("keybindTab");
            switch (SelectedTab)
            {
                case SelectedKeybindTab.Common:
                    CommonKeybinds.Display();
                    break;
                case SelectedKeybindTab.Viewport:
                    ViewportKeybinds.Display();
                    break;
                case SelectedKeybindTab.MapEditor:
                    MapEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.ModelEditor:
                    ModelEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.ParamEditor:
                    ParamEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TextEditor:
                    TextEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.GparamEditor:
                    GparamEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TimeActEditor:
                    TimeActEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TextureViewer:
                    TextureViewerKeybinds.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private SelectedKeybindTab SelectedTab = SelectedKeybindTab.Common;

    public enum SelectedKeybindTab
    {
        [Display(Name = "Common")] Common,
        [Display(Name = "Viewport")] Viewport,
        [Display(Name = "Map Editor")] MapEditor,
        [Display(Name = "Model Editor")] ModelEditor,
        [Display(Name = "Param Editor")] ParamEditor,
        [Display(Name = "Text Editor")] TextEditor,
        [Display(Name = "GPARAM Editor")] GparamEditor,
        [Display(Name = "Time Act Editor")] TimeActEditor,
        [Display(Name = "Texture Viewer")] TextureViewer
    }
}

public class CommonKeybindTab
{
    public CommonKeybindTab()
    {

    }

    public void Display()
    {
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImguiUtils.WrappedText(KeyBindings.Current.Core_Create.PresentationName);

        ImGui.NextColumn();

        KeyBindings.Current.Core_Create = KeybindEntry(KeyBindings.Current.Core_Create);

        ImGui.Columns(1);
    }

    private KeyBind _currentKeyBind;

    public KeyBind KeybindEntry(KeyBind bindVal)
    {
        var newKeyBind = bindVal;
        var fixedKey = bindVal.FixedKey;
        var keyText = bindVal.HintText;

        if (!fixedKey)
        {
            if (bindVal.PrimaryKey == Key.Unknown)
                keyText = "[None]";

            if (_currentKeyBind == bindVal)
            {
                ImGui.Button("Press Key <Esc - Clear>");
                if (InputTracker.GetKeyDown(Key.Escape))
                {
                    KeyBind newkey = InputTracker.GetEmptyKeyBind(bindVal.PresentationName, bindVal.KeyCategory);
                    newKeyBind = newkey;
                    _currentKeyBind = null;
                }
                else
                {
                    KeyBind newkey = InputTracker.GetNewKeyBind(bindVal.PresentationName, bindVal.KeyCategory);
                    if (newkey != null)
                    {
                        newKeyBind = newkey;
                        _currentKeyBind = null;
                    }
                }
            }
            else if (ImGui.Button($"{keyText}##{bindVal}"))
            {
                _currentKeyBind = bindVal;
            }
        }
        else
        {
            ImGui.Text($"{keyText}");
        }

        return newKeyBind;
    }
}

public class ViewportKeybindTab
{
    public ViewportKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class MapEditorKeybindTab
{
    public MapEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class ModelEditorKeybindTab
{
    public ModelEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class ParamEditorKeybindTab
{
    public ParamEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class TextEditorKeybindTab
{
    public TextEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}
public class GparamEditorKeybindTab
{
    public GparamEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class TimeActEditorKeybindTab
{
    public TimeActEditorKeybindTab()
    {

    }

    public void Display()
    {

    }
}

public class TextureViewerKeybindTab
{
    public TextureViewerKeybindTab()
    {

    }

    public void Display()
    {

    }
}