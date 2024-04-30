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

public class ProjectWindow
{
    private bool MenuOpenState;

    public ProjectWindow()
    {
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

        if (ImGui.Begin("Project##ProjectManagementWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##ProjectTabs");

            DisplayProjectTab();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayProjectTab()
    {
        if (ImGui.BeginTabItem("General"))
        {
            if (Project.Config == null || Project.Config.ProjectName == null)
            {
                ImGui.Text("No project loaded");
                ImguiUtils.ShowHoverTooltip("No project has been loaded yet.");
            }
            else if (TaskManager.AnyActiveTasks())
            {
                ImGui.Text("Waiting for program tasks to finish...");
                ImguiUtils.ShowHoverTooltip("Smithbox must finished all program tasks before it can load a project.");
            }
            else
            {
                ImGui.Text($"Project Name: {Project.Config.ProjectName}");
                ImGui.Text($"Project Type: {Project.Type}");
                ImGui.Text($"Project Root Directory: {Project.GameRootDirectory}");
                ImGui.Text($"Project Mod Directory: {Project.GameModDirectory}");

                ImGui.Separator();

                if (ImGui.MenuItem("Open project settings file"))
                {
                    var projectPath = CFG.Current.LastProjectFile;
                    Process.Start("explorer.exe", projectPath);
                }

                ImGui.Separator();

                var useLoose = Project.Config.UseLooseParams;
                if (Project.Config.GameType is ProjectType.DS2S or ProjectType.DS3)
                {
                    if (ImGui.Checkbox("Use loose params", ref useLoose))
                        Project.Config.UseLooseParams = useLoose;
                    ImguiUtils.ShowHoverTooltip("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                }

                var usepartial = Project.Config.PartialParams;
                if (FeatureFlags.EnablePartialParam || usepartial)
                {
                    if (Project.Config.GameType == ProjectType.ER &&
                    ImGui.Checkbox("Partial params", ref usepartial))
                        Project.Config.PartialParams = usepartial;
                    ImguiUtils.ShowHoverTooltip("Partial params.");
                }


            }

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Project Tools"))
        {
            ImGui.Checkbox("Enable Recovery Folder", ref CFG.Current.System_EnableRecoveryFolder);
            ImguiUtils.ShowHoverTooltip("Enable a recovery project to be created upon an unexpected crash.");

            ImGui.Separator();

            ImGui.Checkbox("Enable Automatic Save", ref CFG.Current.System_EnableAutoSave);
            ImguiUtils.ShowHoverTooltip("All changes will be saved at the interval specificed below.");

            ImGui.Text("Automatic Save Interval");
            if (ImGui.InputInt("##AutomaticSaveInterval", ref CFG.Current.System_AutoSaveIntervalSeconds))
            {
                if (CFG.Current.System_AutoSaveIntervalSeconds < 10)
                {
                    CFG.Current.System_AutoSaveIntervalSeconds = 10;
                }

                Project.UpdateTimer();
            }
            ImguiUtils.ShowHoverTooltip("Interval in seconds between each automatic save.");

            ImGui.EndTabItem();
        }
    }
}
