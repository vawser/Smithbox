using ImGuiNET;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tests;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using StudioCore.Formats;

namespace StudioCore.Interface.Windows;

public class DebugWindow
{
    private bool MenuOpenState;

    public bool _showImGuiDemoWindow = false;
    public bool _showImGuiMetricsWindow = false;
    public bool _showImGuiDebugLogWindow = false;
    public bool _showImGuiStackToolWindow = false;

    public DebugWindow()
    {
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private Task _loadingTask;

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

        if (ImGui.Begin("Debug##TestWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("##DebugTabs");

            if (ImGui.BeginTabItem("Actions"))
            {
                DisplayActions();

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Tests"))
            {
                DisplayTests();

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("ImGui"))
            {
                DisplayImGuiDemo();

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Soapstone"))
            {
                DisplayTasks();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private string msbPropertyToFind = "";
    private string msbValueToFind = "";

    private void DisplayActions()
    {
        if (ImGui.Button("Focus Gparam Editor"))
        {
            EditorCommandQueue.AddCommand($@"gparam/view/m00_00_0000/LightSet ParamEditor/Directional Light DiffColor0/100");
        }

        if (ImGui.Button("Focus Texture Viewer"))
        {
            EditorCommandQueue.AddCommand($@"texture/view/01_common/SB_GarageTop_04");
        }

        if (ImGui.Button("Load MSB Data"))
        {
            DebugActions.LoadMsbData();
        }

        ImGui.InputText("##valueDistribution", ref msbPropertyToFind, 255);
        ImGui.SameLine();
        if (ImGui.Button("Log Value Distribution"))
        {
            DebugActions.LogValueDistribution(msbPropertyToFind);
        }

        ImGui.InputText("##valueSearch", ref msbValueToFind, 255);
        ImGui.SameLine();
        if (ImGui.Button("Find Value Instances"))
        {
            DebugActions.FindValueInstances(msbValueToFind);
        }
    }

    private void DisplayImGuiDemo()
    {
        if (ImGui.Button("Demo"))
        {
            _showImGuiDemoWindow = !_showImGuiDemoWindow;
        }

        if (ImGui.Button("Metrics"))
        {
            _showImGuiMetricsWindow = !_showImGuiMetricsWindow;
        }

        if (ImGui.Button("Debug Log"))
        {
            _showImGuiDebugLogWindow = !_showImGuiDebugLogWindow;
        }

        if (ImGui.Button("Stack Tool"))
        {
            _showImGuiStackToolWindow = !_showImGuiStackToolWindow;
        }
    }

    private void DisplayTasks()
    {
        if (TaskManager.GetLiveThreads().Count > 0)
        {
            foreach (var task in TaskManager.GetLiveThreads())
            {
                ImGui.Text(task);
            }
        }
    }
    private void DisplayTests()
    {
        if (ImGui.Button("MSBE read/write test"))
        {
            MSBReadWrite.Run();
        }

        if (ImGui.Button("MSB_AC6 Read/Write Test"))
        {
            MSB_AC6_Read_Write.Run();
        }

        if (ImGui.Button("BTL read/write test"))
        {
            BTLReadWrite.Run();
        }

        if (ImGui.Button("Insert unique rows IDs into params"))
        {
            ParamUniqueRowFinder.Run();
        }
    }
}
