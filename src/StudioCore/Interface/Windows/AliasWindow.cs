using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface.Tabs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class AliasWindow
{
    public bool MenuOpenState;

    // Alias tabs
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

    private bool TabInitialized = false;

    public AliasWindow()
    {
        InitializeTabs();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private void InitializeTabs()
    {
        if (Smithbox.BankHandler != null)
        {
            if (!TabInitialized)
            {
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

                TabInitialized = true;
            }
        }
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        InitializeTabs();

        if (!MenuOpenState)
            return;

        if (Smithbox.BankHandler == null)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Aliases##AliasWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##AliasTabs");

            DisplayAliasTab(CharacterAliasTab, "Characters");
            DisplayAliasTab(AssetAliasTab, "Assets");
            DisplayAliasTab(PartAliasTab, "Parts");
            DisplayAliasTab(MapPieceAliasTab, "Map Pieces");
            DisplayAliasTab(GparamAliasTab, "Gparams");
            DisplayAliasTab(EventFlagAliasTab, "Event Flags");
            DisplayAliasTab(ParticleAliasTab, "Particles");
            DisplayAliasTab(CutsceneAliasTab, "Cutscenes");
            DisplayAliasTab(MovieAliasTab, "Movies");
            DisplayAliasTab(SoundAliasTab, "Sounds");
            DisplayAliasTab(MapAliasTab, "Map Names");
            DisplayAliasTab(TimeActsTab, "Time Acts");

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    bool tabOpen = true;

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
        var flags = ImGuiTabItemFlags.UnsavedDocument;

        if (DisplayCharacterTab)
        {
            if(name == "Characters")
            {
                DisplayCharacterTab = false;
                tabOpen = true;
                flags = flags | ImGuiTabItemFlags.SetSelected;

                foreach(var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
                {
                    if(entry.id == TargetChrID)
                    {
                        CharacterAliasTab.FocusSelection = true;
                        CharacterAliasTab._selectedEntry = entry;
                        CharacterAliasTab._refUpdateId = entry.id;
                        CharacterAliasTab._refUpdateName = entry.name;
                        CharacterAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                }
            }
        }
        if (DisplayAssetTab)
        {
            if (name == "Assets")
            {
                DisplayAssetTab = false;
                tabOpen = true;
                flags = flags | ImGuiTabItemFlags.SetSelected;

                foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
                {
                    if (entry.id == TargetAssetID)
                    {
                        AssetAliasTab.FocusSelection = true;
                        AssetAliasTab._selectedEntry = entry;
                        AssetAliasTab._refUpdateId = entry.id;
                        AssetAliasTab._refUpdateName = entry.name;
                        AssetAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                }
            }
        }
        if (DisplayPartTab)
        {
            if (name == "Parts")
            {
                DisplayPartTab = false;
                tabOpen = true;
                flags = flags | ImGuiTabItemFlags.SetSelected;

                foreach (var entry in Smithbox.BankHandler.PartAliases.Aliases.list)
                {
                    if (entry.id == TargetPartID)
                    {
                        PartAliasTab.FocusSelection = true;
                        PartAliasTab._selectedEntry = entry;
                        PartAliasTab._refUpdateId = entry.id;
                        PartAliasTab._refUpdateName = entry.name;
                        PartAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                }
            }
        }
        if (DisplayMapPieceTab)
        {
            if (name == "Map Pieces")
            {
                DisplayMapPieceTab = false;
                tabOpen = true;
                flags = flags | ImGuiTabItemFlags.SetSelected;

                foreach (var entry in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
                {
                    if (entry.id == TargetMapPieceID)
                    {
                        MapPieceAliasTab.FocusSelection = true;
                        MapPieceAliasTab._selectedEntry = entry;
                        MapPieceAliasTab._refUpdateId = entry.id;
                        MapPieceAliasTab._refUpdateName = entry.name;
                        MapPieceAliasTab._refUpdateTags = AliasUtils.GetTagListString(entry.tags);
                        break;
                    }
                }
            }
        }

        tabOpen = true;

        if (ImGui.BeginTabItem(name, ref tabOpen, flags))
        {
            tab.Display();

            ImGui.EndTabItem();
        }
    }
}
