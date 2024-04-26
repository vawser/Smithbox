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

    public ProjectSettings ProjSettings = null;

    private ParticleTab ParticleAliasTab;
    private EventFlagTab EventFlagAliasTab;
    private MapNameTab MapAliasTab;
    private GparamNameTab GparamAliasTab;
    private SoundTab SoundAliasTab;
    private CutsceneTab CutsceneAliasTab;
    private MovieTab MovieAliasTab;

    public AliasWindow()
    {
        ParticleAliasTab = new ParticleTab();
        EventFlagAliasTab = new EventFlagTab();
        MapAliasTab = new MapNameTab();
        GparamAliasTab = new GparamNameTab();
        SoundAliasTab = new SoundTab();
        CutsceneAliasTab = new CutsceneTab();
        MovieAliasTab = new MovieTab();
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

            DisplayEventFlagTab();
            DisplayParticleTab();
            DisplaySoundTab();
            DisplayCutsceneTab();
            DisplayMovieTab();
            DisplayMapNameTab();
            DisplayGparamNameTab();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayEventFlagTab()
    {
        if (ImGui.BeginTabItem("Event Flags"))
        {
            EventFlagAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayMapNameTab()
    {
        if (ImGui.BeginTabItem("Maps"))
        {
            MapAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayGparamNameTab()
    {
        if (ImGui.BeginTabItem("Gparams"))
        {
            GparamAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayParticleTab()
    {
        if (ImGui.BeginTabItem("Particles"))
        {
            ParticleAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplaySoundTab()
    {
        if (ImGui.BeginTabItem("Sounds"))
        {
            SoundAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayCutsceneTab()
    {
        if (ImGui.BeginTabItem("Cutscenes"))
        {
            CutsceneAliasTab.Display();

            ImGui.EndTabItem();
        }
    }

    public void DisplayMovieTab()
    {
        if (ImGui.BeginTabItem("Movies"))
        {
            MovieAliasTab.Display();

            ImGui.EndTabItem();
        }
    }
}
