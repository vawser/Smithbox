using Hexa.NET.ImGui;
using StudioCore.Configuration.Settings;
using StudioCore.Interface;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace StudioCore.Configuration.Windows;
public class KeybindWindow
{
    public Smithbox BaseEditor;

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

    public KeybindWindow(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;

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

    public void ToggleWindow(SelectedKeybindTab focusedTab, bool ignoreIfOpen = true)
    {
        SelectedTab = focusedTab;

        if (!ignoreIfOpen)
        {
            MenuOpenState = !MenuOpenState;
        }

        if (!MenuOpenState)
        {
            MenuOpenState = true;
        }
    }


    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private bool wasOpenLastFrame = true;

    public void Display()
    {
        var scale = DPI.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##KeybindWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginChild($"KeybindSectionList");

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
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);

        if (wasOpenLastFrame && !MenuOpenState)
        {
            KeyBindings.Save();
        }

        wasOpenLastFrame = MenuOpenState;
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

