using Hexa.NET.ImGui;
using StudioCore.Configuration.Settings;
using StudioCore.Interface;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace StudioCore.Configuration;

public class SettingsWindow
{
    public Smithbox BaseEditor;

    public bool MenuOpenState;

    private SystemTab SystemSettings;
    private ViewportTab ViewportSettings;
    private MapEditorTab MapEditorSettings;
    private ModelEditorTab ModelEditorSettings;
    private ParamEditorTab ParamEditorSettings;
    private TextEditorTab TextEditorSettings;
    private GparamEditorTab GparamEditorSettings;
    private TextureViewerTab TextureViewerSettings;
    private InterfaceTab InterfaceSettings;
    private TimeActEditorTab TimeActEditorSettings;
    private EmevdEditorTab EmevdEditorSettings;
    private EsdEditorTab EsdEditorSettings;

    public bool TabInitialized = false;

    public SettingsWindow(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;

        SystemSettings = new SystemTab(BaseEditor);
        ViewportSettings = new ViewportTab(BaseEditor);
        MapEditorSettings = new MapEditorTab(BaseEditor);
        ModelEditorSettings = new ModelEditorTab(BaseEditor);
        ParamEditorSettings = new ParamEditorTab(BaseEditor);
        TextEditorSettings = new TextEditorTab(BaseEditor);
        GparamEditorSettings = new GparamEditorTab(BaseEditor);
        TextureViewerSettings = new TextureViewerTab(BaseEditor);
        TimeActEditorSettings = new TimeActEditorTab(BaseEditor);
        EmevdEditorSettings = new EmevdEditorTab(BaseEditor);
        EsdEditorSettings = new EsdEditorTab(BaseEditor);
        InterfaceSettings = new InterfaceTab(BaseEditor);
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(1200.0f, 1000.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Settings##SettingsWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##settingTabs");

            ImGui.BeginTabItem("System");
            SystemSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Interface");
            InterfaceSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Viewport");
            ViewportSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Map Editor");
            MapEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Model Editor");
            ModelEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Param Editor");
            ParamEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Text Editor");
            TextEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Graphics Param Editor");
            GparamEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Time Act Editor");
            TimeActEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Event Script Editor");
            EmevdEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("EzState Script Editor");
            EsdEditorSettings.Display();
            ImGui.EndTabItem();

            ImGui.BeginTabItem("Texture Viewer");
            TextureViewerSettings.Display();
            ImGui.EndTabItem();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
