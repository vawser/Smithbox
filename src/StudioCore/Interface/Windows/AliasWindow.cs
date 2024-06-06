using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Help;
using StudioCore.Interface.Tabs;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Windows;

public class AliasWindow
{
    private bool MenuOpenState;

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

    // Special-case tabs
    private MapGroupTab MapGroupTab;



    public AliasWindow()
    {
        MapAliasTab = new AliasTab(MapAliasBank.Bank, "Maps", ref CFG.Current.MapAtlas_ShowTags);
        CharacterAliasTab = new AliasTab(ModelAliasBank.Bank, "Characters", ref CFG.Current.CharacterAtlas_ShowTags);
        AssetAliasTab = new AliasTab(ModelAliasBank.Bank, "Objects", ref CFG.Current.AssetAtlas_ShowTags);
        PartAliasTab = new AliasTab(ModelAliasBank.Bank, "Parts", ref CFG.Current.PartAtlas_ShowTags);
        MapPieceAliasTab = new AliasTab(ModelAliasBank.Bank, "MapPieces", ref CFG.Current.MapPieceAtlas_ShowTags);
        GparamAliasTab = new AliasTab(GparamAliasBank.Bank, "Gparams", ref CFG.Current.GparamNameAtlas_ShowTags);
        CutsceneAliasTab = new AliasTab(CutsceneAliasBank.Bank, "Cutscenes", ref CFG.Current.CutsceneAtlas_ShowTags);
        EventFlagAliasTab = new AliasTab(FlagAliasBank.Bank, "Flags", ref CFG.Current.EventFlagAtlas_ShowTags);
        SoundAliasTab = new AliasTab(SoundAliasBank.Bank, "Sounds", ref CFG.Current.SoundAtlas_ShowTags, true);
        ParticleAliasTab = new AliasTab(ParticleAliasBank.Bank, "Particles", ref CFG.Current.ParticleAtlas_ShowTags);
        MovieAliasTab = new AliasTab(MovieAliasBank.Bank, "Movies", ref CFG.Current.MovieAtlas_ShowTags);

        MapGroupTab = new MapGroupTab();

    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
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

            DisplayMapGroupTab();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayAliasTab(AliasTab tab, string name)
    {
        if (ImGui.BeginTabItem(name))
        {
            tab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayMapGroupTab()
    {
        if (ImGui.BeginTabItem("Map Groups"))
        {
            MapGroupTab.Display();

            ImGui.EndTabItem();
        }
    }
}
