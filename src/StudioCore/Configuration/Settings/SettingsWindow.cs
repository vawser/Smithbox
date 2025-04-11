using ImGuiNET;
using SoapstoneLib;
using StudioCore.Editor;
using StudioCore.Scene;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Veldrid;
using StudioCore.Editors;
using StudioCore.Settings;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.TextureViewer;
using System.IO;
using StudioCore.Platform;
using StudioCore.Core;
using System.ComponentModel.DataAnnotations;
using StudioCore.Utilities;
using StudioCore.Interface;
using StudioCore.Configuration.Settings;

namespace StudioCore.Configuration;

public class SettingsWindow
{
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

    private ProjectSettingsTab ProjectSettingsTab;

    private AliasTab CharacterAliasTab;
    private AliasTab AssetAliasTab;
    private AliasTab PartAliasTab;
    private AliasTab MapPieceAliasTab;
    private AliasTab CutsceneAliasTab;
    private AliasTab GparamAliasTab;
    private AliasTab EventFlagAliasTab;
    private AliasTab SoundAliasTab;
    private AliasTab ParticleAliasTab;
    private AliasTab MovieAliasTab;
    private AliasTab MapAliasTab;
    private AliasTab TimeActsTab;
    private AliasTab TalksTab;

    public bool TabInitialized = false;

    public SettingsWindow()
    {
        SystemSettings = new SystemTab();
        ViewportSettings = new ViewportTab();
        MapEditorSettings = new MapEditorTab();
        ModelEditorSettings = new ModelEditorTab();
        ParamEditorSettings = new ParamEditorTab();
        TextEditorSettings = new TextEditorTab();
        GparamEditorSettings = new GparamEditorTab();
        TextureViewerSettings = new TextureViewerTab();
        TimeActEditorSettings = new TimeActEditorTab();
        EmevdEditorSettings = new EmevdEditorTab();
        EsdEditorSettings = new EsdEditorTab();
        InterfaceSettings = new InterfaceTab();

        ProjectSettingsTab = new ProjectSettingsTab();
    }

    public void SaveSettings()
    {
        CFG.Save();
        UI.Save();
    }

    private SelectedSettingTab CurrentTab;

    public void ToggleWindow(SelectedSettingTab focusedTab, bool ignoreIfOpen = true)
    {
        CurrentTab = focusedTab;

        if (!ignoreIfOpen)
        {
            MenuOpenState = !MenuOpenState;
        }
        
        if(!MenuOpenState)
        {
            MenuOpenState = true;
        }

        SetupAliasTabs();
    }

    public void SetupAliasTabs()
    {
        if (Smithbox.BankHandler != null)
        {
            if (!TabInitialized)
            {
                TabInitialized = true;

                MapAliasTab = new AliasTab(Smithbox.BankHandler.MapAliases, "Maps", ref CFG.Current.MapAtlas_ShowTags);
                CharacterAliasTab = new AliasTab(Smithbox.BankHandler.CharacterAliases, "Characters", ref CFG.Current.CharacterAtlas_ShowTags, false, false);
                AssetAliasTab = new AliasTab(Smithbox.BankHandler.AssetAliases, "Assets", ref CFG.Current.AssetAtlas_ShowTags, false, false);
                PartAliasTab = new AliasTab(Smithbox.BankHandler.PartAliases, "Parts", ref CFG.Current.PartAtlas_ShowTags, false, false);
                MapPieceAliasTab = new AliasTab(Smithbox.BankHandler.MapPieceAliases, "MapPieces", ref CFG.Current.MapPieceAtlas_ShowTags, false, false);
                GparamAliasTab = new AliasTab(Smithbox.BankHandler.GparamAliases, "Gparams", ref CFG.Current.GparamNameAtlas_ShowTags);
                CutsceneAliasTab = new AliasTab(Smithbox.BankHandler.CutsceneAliases, "Cutscenes", ref CFG.Current.CutsceneAtlas_ShowTags);
                EventFlagAliasTab = new AliasTab(Smithbox.BankHandler.EventFlagAliases, "Event Flags", ref CFG.Current.EventFlagAtlas_ShowTags);
                SoundAliasTab = new AliasTab(Smithbox.BankHandler.SoundAliases, "Sounds", ref CFG.Current.SoundAtlas_ShowTags, true);
                ParticleAliasTab = new AliasTab(Smithbox.BankHandler.ParticleAliases, "Particles", ref CFG.Current.ParticleAtlas_ShowTags);
                MovieAliasTab = new AliasTab(Smithbox.BankHandler.MovieAliases, "Movies", ref CFG.Current.MovieAtlas_ShowTags);
                TimeActsTab = new AliasTab(Smithbox.BankHandler.TimeActAliases, "Time acts", ref CFG.Current.TimeActAtlas_ShowTags);
                TalksTab = new AliasTab(Smithbox.BankHandler.TalkAliases, "Talk scripts", ref CFG.Current.TalkAtlas_ShowTags);
            }
        }
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
                case SelectedSettingTab.Project:
                    ProjectSettingsTab.Display();
                    break;
                case SelectedSettingTab.ProjectAliases_Characters:
                    DisplayAliasTab(CharacterAliasTab, "Characters");
                    break;
                case SelectedSettingTab.ProjectAliases_Assets:
                    DisplayAliasTab(AssetAliasTab, "Assets");
                    break;
                case SelectedSettingTab.ProjectAliases_Parts:
                    DisplayAliasTab(PartAliasTab, "Parts");
                    break;
                case SelectedSettingTab.ProjectAliases_MapPieces:
                    DisplayAliasTab(MapPieceAliasTab, "Map Pieces");
                    break;
                case SelectedSettingTab.ProjectAliases_Gparams:
                    DisplayAliasTab(GparamAliasTab, "Gparams");
                    break;
                case SelectedSettingTab.ProjectAliases_EventFlags:
                    DisplayAliasTab(EventFlagAliasTab, "Event Flags");
                    break;
                case SelectedSettingTab.ProjectAliases_Particles:
                    DisplayAliasTab(ParticleAliasTab, "Particles");
                    break;
                case SelectedSettingTab.ProjectAliases_Cutscenes:
                    DisplayAliasTab(CutsceneAliasTab, "Cutscenes");
                    break;
                case SelectedSettingTab.ProjectAliases_Movies:
                    DisplayAliasTab(MovieAliasTab, "Movies");
                    break;
                case SelectedSettingTab.ProjectAliases_Sounds:
                    DisplayAliasTab(SoundAliasTab, "Sounds");
                    break;
                case SelectedSettingTab.ProjectAliases_MapNames:
                    DisplayAliasTab(MapAliasTab, "Map Names");
                    break;
                case SelectedSettingTab.ProjectAliases_TimeActs:
                    DisplayAliasTab(TimeActsTab, "Time Acts");
                    break;
                case SelectedSettingTab.ProjectAliases_Talks:
                    DisplayAliasTab(TalksTab, "Talk Scripts");
                    break;
            }
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public bool DisplayCharacterTab = false;
    public string TargetChrID = "";

    public bool DisplayAssetTab = false;
    public string TargetAssetID = "";

    public bool DisplayPartTab = false;
    public string TargetPartID = "";

    public bool DisplayMapPieceTab = false;
    public string TargetMapPieceID = "";

    public void DisplayAliasTab(AliasTab tab, string name)
    {
        if (!TabInitialized)
            return;

        if (DisplayCharacterTab)
        {
            if (name == "Characters")
            {
                foreach (var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
                {
                    if (entry.id == TargetChrID)
                    {
                        CharacterAliasTab._newRefId = "";

                        CharacterAliasTab.FocusSelection = true;
                        CharacterAliasTab._selectedEntry = entry;
                        CharacterAliasTab._refUpdateId = entry.id;
                        CharacterAliasTab._refUpdateName = entry.name;
                        CharacterAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                    else
                    {
                        CharacterAliasTab._newRefId = TargetChrID;
                    }
                }
            }

            DisplayCharacterTab = false;
        }
        if (DisplayAssetTab)
        {
            if (name == "Assets")
            {
                foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
                {
                    if (entry.id == TargetAssetID)
                    {
                        AssetAliasTab._newRefId = "";

                        AssetAliasTab.FocusSelection = true;
                        AssetAliasTab._selectedEntry = entry;
                        AssetAliasTab._refUpdateId = entry.id;
                        AssetAliasTab._refUpdateName = entry.name;
                        AssetAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                    else
                    {
                        AssetAliasTab._newRefId = TargetAssetID;
                    }
                }
            }

            DisplayAssetTab = false;
        }
        if (DisplayPartTab)
        {
            if (name == "Parts")
            {
                foreach (var entry in Smithbox.BankHandler.PartAliases.Aliases.list)
                {
                    if (entry.id == TargetPartID)
                    {
                        PartAliasTab._newRefId = "";

                        PartAliasTab.FocusSelection = true;
                        PartAliasTab._selectedEntry = entry;
                        PartAliasTab._refUpdateId = entry.id;
                        PartAliasTab._refUpdateName = entry.name;
                        PartAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                    else
                    {
                        PartAliasTab._newRefId = TargetPartID;
                    }
                }
            }

            DisplayPartTab = false;
        }
        if (DisplayMapPieceTab)
        {
            if (name == "Map Pieces")
            {
                foreach (var entry in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
                {
                    if (entry.id == TargetMapPieceID)
                    {
                        MapPieceAliasTab._newRefId = "";

                        MapPieceAliasTab.FocusSelection = true;
                        MapPieceAliasTab._selectedEntry = entry;
                        MapPieceAliasTab._refUpdateId = entry.id;
                        MapPieceAliasTab._refUpdateName = entry.name;
                        MapPieceAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                    else
                    {
                        MapPieceAliasTab._newRefId = TargetMapPieceID;
                    }
                }
            }

            DisplayMapPieceTab = false;
        }

        tab.Display();
    }

    // private SelectedSettingTab SelectedTab = SelectedSettingTab.System;

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
