using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Help;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tests;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Windows;

public class MemoryWindow
{
    private bool MenuOpenState;

    public ParamEditorView _activeView;

    private bool ShowParamList = false;

    public MemoryWindow()
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

        if (ImGui.Begin("Memory##MemoryWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##memoryTabs");

            DisplayParamHotReloader();
            DisplayParamItemGib();

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    public void DisplayParamHotReloader()
    {
        if (ImGui.BeginTabItem("Param Reloader"))
        {
            if (!ParamReloader.GameIsSupported(Project.Type))
            {
                ImGui.Text("This project type does not support this feature.");
                ImGui.EndTabItem();
                return;
            }

            ImGui.Text("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

            ImGui.Separator();

            var canHotReload = ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank);

            // Currently Selected Param
            if (ImGui.MenuItem("Current Param", KeyBindings.Current.Param_HotReload.HintText, false, canHotReload && _activeView._selection.GetActiveParam() != null))
            {
                ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, _activeView._selection.GetActiveParam());
            }

            // All Params
            if (ImGui.MenuItem("All Params", KeyBindings.Current.Param_HotReloadAll.HintText, false, canHotReload))
            {
                ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.Params.Keys.ToArray());
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Toggle Individual Param List"))
            {
                ShowParamList = !ShowParamList;
            }

            ImGui.Separator();

            if (ShowParamList)
            {
                // List each param
                foreach (var param in ParamReloader.GetReloadableParams())
                {
                    if (ImGui.MenuItem(param, "", false, canHotReload))
                    {
                        ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, new[] { param });
                    }
                }
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayParamItemGib()
    {
        if (ImGui.BeginTabItem("Item Gib"))
        {
            if (Project.Type != ProjectType.DS3)
            {
                ImGui.Text("This project type does not support this feature.");
                ImGui.EndTabItem();
                return;
            }

            ImGui.Text("Select an item param in the Param Editor.");

            ImGui.Separator();

            var activeParam = _activeView._selection.GetActiveParam();
            if (activeParam != null && Project.Type == ProjectType.DS3)
            {
                ParamReloader.GiveItemMenu(_activeView._selection.GetSelectedRows(), _activeView._selection.GetActiveParam());
            }

            ImGui.EndTabItem();
        }
    }
}
