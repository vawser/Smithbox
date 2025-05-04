using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Configuration.Settings;
using StudioCore.Interface;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace StudioCore.Configuration.Windows;

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

    private SelectedSettingTab CurrentTab;

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

    public void ToggleWindow(SelectedSettingTab focusedTab, bool ignoreIfOpen = true)
    {
        CurrentTab = focusedTab;

        if (!ignoreIfOpen)
        {
            MenuOpenState = !MenuOpenState;
        }

        if (!MenuOpenState)
        {
            MenuOpenState = true;
        }
    }

    private bool wasOpenLastFrame = true;

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
            ImGui.BeginChild("configurationTab");
            switch (CurrentTab)
            {
                case SelectedSettingTab.System:
                    SystemSettings.Display();
                    break;
                case SelectedSettingTab.Viewport:
                    ViewportSettings.Display();
                    break;
                case SelectedSettingTab.MapEditor:
                    MapEditorSettings.Display();
                    break;
                case SelectedSettingTab.ModelEditor:
                    ModelEditorSettings.Display();
                    break;
                case SelectedSettingTab.ParamEditor:
                    ParamEditorSettings.Display();
                    break;
                case SelectedSettingTab.TextEditor:
                    TextEditorSettings.Display();
                    break;
                case SelectedSettingTab.GparamEditor:
                    GparamEditorSettings.Display();
                    break;
                case SelectedSettingTab.TimeActEditor:
                    TimeActEditorSettings.Display();
                    break;
                case SelectedSettingTab.EmevdEditor:
                    EmevdEditorSettings.Display();
                    break;
                case SelectedSettingTab.EsdEditor:
                    EsdEditorSettings.Display();
                    break;
                case SelectedSettingTab.TextureViewer:
                    TextureViewerSettings.Display();
                    break;
                case SelectedSettingTab.Interface:
                    InterfaceSettings.Display();
                    break;
            }
            ImGui.EndChild();
        }
        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);

        if (wasOpenLastFrame && !MenuOpenState)
        {
            CFG.Save();
            UI.Save();
        }

        wasOpenLastFrame = MenuOpenState;
    }
    public enum SelectedSettingTab
    {
        [Display(Name = "System")] System,
        [Display(Name = "Project")] Project,
        [Display(Name = "Viewport")] Viewport,
        [Display(Name = "Map Editor")] MapEditor,
        [Display(Name = "Model Editor")] ModelEditor,
        [Display(Name = "Param Editor")] ParamEditor,
        [Display(Name = "Text Editor")] TextEditor,
        [Display(Name = "GPARAM Editor")] GparamEditor,
        [Display(Name = "Time Act Editor")] TimeActEditor,
        [Display(Name = "EMEVD Editor")] EmevdEditor,
        [Display(Name = "ESD Editor")] EsdEditor,
        [Display(Name = "Texture Viewer")] TextureViewer,
        [Display(Name = "Interface")] Interface,

        [Display(Name = "Characters")] ProjectAliases_Characters,
        [Display(Name = "Assets")] ProjectAliases_Assets,
        [Display(Name = "Parts")] ProjectAliases_Parts,
        [Display(Name = "Map Pieces")] ProjectAliases_MapPieces,
        [Display(Name = "GPARAM")] ProjectAliases_Gparams,
        [Display(Name = "Event Flags")] ProjectAliases_EventFlags,
        [Display(Name = "Particles")] ProjectAliases_Particles,
        [Display(Name = "Cutscenes")] ProjectAliases_Cutscenes,
        [Display(Name = "Movies")] ProjectAliases_Movies,
        [Display(Name = "Sounds")] ProjectAliases_Sounds,
        [Display(Name = "Map Names")] ProjectAliases_MapNames,
        [Display(Name = "Time Acts")] ProjectAliases_TimeActs,
        [Display(Name = "Talk Scripts")] ProjectAliases_Talks
    }
}
