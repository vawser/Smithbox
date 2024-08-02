using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Tests;
using StudioCore.Tools;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

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

    private void DisplayActions()
    {
        ImGui.BeginTabBar("DebugActions");

        ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
        ImGui.PushItemWidth(300f);

        DisplayTool_DataExplorer();
        DisplayTool_ParamValidation();
        DisplayTool_MapValidation();
        DisplayTool_FLVERDump();
        DisplayTool_RowNameHelper();
        DisplayTool_DecryptRegulation();
        DisplayTool_HavokTool();
        DisplayTool_FlverTestTool();

        ImGui.PopItemWidth();
        ImGui.PopStyleColor();
        ImGui.EndTabBar();
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

    private void DisplayTool_FLVERDump()
    {
        if (ImGui.BeginTabItem("Dump FLVER Layouts"))
        {
            if(ImGui.Button("Dump"))
            {
                DebugActions.DumpFlverLayouts();
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_DataExplorer()
    {
        if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (ImGui.BeginTabItem("Data Explorer"))
            {
                DataExplorer.Display();

                ImGui.EndTabItem();
            }
        }
    }

    private void DisplayTool_ParamValidation()
    {
        if (ImGui.BeginTabItem("Param Validation"))
        {
            ImGui.Text("This tool will validate the PARAMDEF and padding values. Issues will be printed to the Logger.");

            if (ImGui.Button("Validate PARAMDEF"))
            {
                ParamValidationTool.ValidateParamdef();
            }
            ImguiUtils.ShowHoverTooltip("Validate that the current PARAMDEF works with the old-style SF PARAM class.");

            if (ImGui.Button("Validate Padding (for selected param)"))
            {
                ParamValidationTool.ValidatePadding();
            }
            ImguiUtils.ShowHoverTooltip("Validate that there are no non-zero values within padding fields.");

            if (ImGui.Button("Validate Padding (for all params)"))
            {
                ParamValidationTool.ValidatePadding(true);
            }
            ImguiUtils.ShowHoverTooltip("Validate that there are no non-zero values within padding fields.");

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_MapValidation()
    {
        if (ImGui.BeginTabItem("Map Validation"))
        {
            ImGui.Text("This tool will validate the MSB for the current project by loading all MSB files.");

            ImGui.Checkbox("Check project files", ref MapValidationTool.TargetProject);
            ImguiUtils.ShowHoverTooltip("The check will use the game root files by default, if you want to use your project's specific files, tick this.");


            if (ImGui.Button("Validate MSB"))
            {
                MapValidationTool.ValidateMSB();
            }

            if (MapValidationTool.HasFinished)
            {
                ImGui.Text("Validation has finished.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_RowNameHelper()
    {
        if (ImGui.BeginTabItem("Row Name Helper"))
        {
            if (ImGui.Button("Generate Row Names"))
            {
                NameGenerationTool.GenerateRowNames();
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_DecryptRegulation()
    {
        if (ImGui.BeginTabItem("Decrypt Regulation"))
        {
            if (ImGui.Button("Decrypt"))
            {
                DebugActions.DecryptRegulation();
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_HavokTool()
    {
        if (ImGui.BeginTabItem("Havok Tool"))
        {
            if (ImGui.Button("Test Collision Load"))
            {
                HavokTool.TestLoad();
            }
            if (ImGui.Button("Test Collision Resource"))
            {
                HavokTool.TestResource();
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_FlverTestTool()
    {
        if (ImGui.BeginTabItem("FLVER Tool"))
        {
            if (ImGui.Button("Test FLVER2 Read"))
            {
                FlverTestTool.TestRead();
            }

            ImGui.EndTabItem();
        }
    }
}
