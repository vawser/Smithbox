using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration.Settings;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Tests;
using StudioCore.Tools.Generation;
using StudioCore.Tools.Validation;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using static StudioCore.Configuration.Settings.SettingsWindow;
using System;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Tools.Development;

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

    private SelectedDebugTab SelectedTab = SelectedDebugTab.DisplayTaskStatus;

    public enum SelectedDebugTab
    {
        // Information
        [Display(Name = "Task Status")] DisplayTaskStatus,

        // ImGui
        [Display(Name = "ImGui Demo")] ImGuiDemo,
        [Display(Name = "ImGui Metrics")] ImGuiMetrics,
        [Display(Name = "ImGui Debug Log")] ImGuiLog,
        [Display(Name = "ImGui Stack Tool")] ImGuiStackTool,
        [Display(Name = "ImGui Test Panel")] ImGuiTestPanel,

        // Validation
        [Display(Name = "Paramdef Validation")] ValidateParamdef,
        [Display(Name = "MSB Validation")] ValidateMSB,
        [Display(Name = "TAE Validation")] ValidateTAE,

        // Helpers
        [Display(Name = "FMG Name Helper")] FmgNameHelper,
        [Display(Name = "FLVER Layout Helper")] FlverLayoutHelper,

        // Tests
        [Display(Name = "MSBE - Byte Perfect Test")] Test_MSBE_BytePerfect,
        [Display(Name = "MSB_AC6 - Byte Perfect Test")] Test_MSB_AC6_BytePerfect,
        [Display(Name = "BTL - Byte Perfect Test")] Test_BTL_BytePerfect,
        [Display(Name = "Unique Param Row ID Insert")] Test_UniqueParamRowIDs
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
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Debug##TestWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("debugToolList");

            var arr = Enum.GetValues(typeof(SelectedDebugTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedDebugTab)arr.GetValue(i);

                if (tab == SelectedDebugTab.DisplayTaskStatus)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Information");
                    ImGui.Separator();
                }
                if (tab == SelectedDebugTab.ImGuiDemo)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "ImGui");
                    ImGui.Separator();
                }
                if (tab == SelectedDebugTab.ValidateParamdef)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Validation");
                    ImGui.Separator();
                }
                if (tab == SelectedDebugTab.FmgNameHelper)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Helpers");
                    ImGui.Separator();
                }
                if (tab == SelectedDebugTab.Test_MSBE_BytePerfect)
                {
                    ImGui.Separator();
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Tests");
                    ImGui.Separator();
                }

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("configurationTab");
            switch (SelectedTab)
            {
                // Information
                case SelectedDebugTab.DisplayTaskStatus:
                    DisplayTasks();
                    break;

                // ImGui
                case SelectedDebugTab.ImGuiDemo:
                    DisplayImGuiDemo();
                    break;
                case SelectedDebugTab.ImGuiMetrics:
                    DisplayImGuiMetrics();
                    break;
                case SelectedDebugTab.ImGuiLog:
                    DisplayImGuiDebugLog();
                    break;
                case SelectedDebugTab.ImGuiStackTool:
                    DisplayImGuiStackTool();
                    break;
                case SelectedDebugTab.ImGuiTestPanel:
                    DisplayImGuiTestPanel();
                    break;

                // Validation
                case SelectedDebugTab.ValidateParamdef:
                    DisplayTool_ParamValidation();
                    break;
                case SelectedDebugTab.ValidateMSB:
                    DisplayTool_MapValidation();
                    break;
                case SelectedDebugTab.ValidateTAE:
                    DisplayTool_TimeActValidation();
                    break;

                // Helpers
                case SelectedDebugTab.FmgNameHelper:
                    DisplayHelper_FMGNames();
                    break;
                case SelectedDebugTab.FlverLayoutHelper:
                    DisplayHelper_FLVERDumpy();
                    break;

                // Tests
                case SelectedDebugTab.Test_MSBE_BytePerfect:
                    DisplayTest_MSBE();
                    break;
                case SelectedDebugTab.Test_MSB_AC6_BytePerfect:
                    DisplayTest_MSB_AC6();
                    break;
                case SelectedDebugTab.Test_BTL_BytePerfect:
                    DisplayTest_BTL();
                    break;
                case SelectedDebugTab.Test_UniqueParamRowIDs:
                    DisplayTest_UniqueParamRows();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    // Information
    private void DisplayTasks()
    {
        ImGui.Text("Currently running tasks:");
        ImGui.Text("");

        if (TaskManager.GetLiveThreads().Count > 0)
        {
            foreach (var task in TaskManager.GetLiveThreads())
            {
                ImGui.Text(task);
            }
        }
    }

    // ImGui
    private void DisplayImGuiDemo()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Demo", buttonSize))
        {
            _showImGuiDemoWindow = !_showImGuiDemoWindow;
        }
    }
    private void DisplayImGuiMetrics()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Metrics", buttonSize))
        {
            _showImGuiMetricsWindow = !_showImGuiMetricsWindow;
        }
    }
    private void DisplayImGuiDebugLog()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Debug Log", buttonSize))
        {
            _showImGuiDebugLogWindow = !_showImGuiDebugLogWindow;
        }
    }
    private void DisplayImGuiStackTool()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Stack Tool", buttonSize))
        {
            _showImGuiStackToolWindow = !_showImGuiStackToolWindow;
        }
    }
    private void DisplayImGuiTestPanel()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        // For testing ImGui elements

    }

    // Validation
    private void DisplayTool_ParamValidation()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        ImGui.Text("This tool will validate the PARAMDEF and padding values. Issues will be printed to the Logger.");
        ImGui.Text("");

        if (ImGui.Button("Validate PARAMDEF", buttonSize))
        {
            ParamValidationTool.ValidateParamdef();
        }
        ImguiUtils.ShowHoverTooltip("Validate that the current PARAMDEF works with the old-style SF PARAM class.");

        if (ImGui.Button("Validate Padding (for selected param)", buttonSize))
        {
            ParamValidationTool.ValidatePadding();
        }
        ImguiUtils.ShowHoverTooltip("Validate that there are no non-zero values within padding fields.");

        if (ImGui.Button("Validate Padding (for all params)", buttonSize))
        {
            ParamValidationTool.ValidatePadding(true);
        }
        ImguiUtils.ShowHoverTooltip("Validate that there are no non-zero values within padding fields.");

    }

    private void DisplayTool_MapValidation()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        ImGui.Text("This tool will validate the MSB for the current project by loading all MSB files.");
        ImGui.Text("");

        if (MapValidationTool.HasFinished)
        {
            ImGui.Text("Validation has finished.");
            ImGui.Text("");
        }

        if (ImGui.Button("Validate MSB", buttonSize))
        {
            MapValidationTool.ValidateMSB();
        }

        ImGui.Checkbox("Check project files", ref MapValidationTool.TargetProject);
        ImguiUtils.ShowHoverTooltip("The check will use the game root files by default, if you want to use your project's specific files, tick this.");
    }

    private void DisplayTool_TimeActValidation()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        ImGui.Text("This tool will validate the Time Act files for the current project by loading all TAE files.");
        ImGui.Text("");

        if (TimeActValidationTool.HasFinished)
        {
            ImGui.Text("Validation has finished.");
            ImGui.Text("");
        }

        if (ImGui.Button("Validate TAE", buttonSize))
        {
            TimeActValidationTool.ValidateTAE();
        }
    }

    // Helpers
    private void DisplayHelper_FMGNames()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        ImGui.Text("This tool will export the FMG Names usable in the Param Meta to the clipboard.");
        ImGui.Text("");

        if (ImGui.Button("Export", buttonSize))
        {
            FmgMetaTool.GetNames();
        }
    }

    private void DisplayHelper_FLVERDumpy()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Dump Layouts", buttonSize))
        {
            FlverDumpTools.DumpFlverLayouts();
        }
    }

    // Tests
    private void DisplayTest_MSBE()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("MSBE read/write test", buttonSize))
        {
            Test_MSB_ER_BytePerfect.Run();
        }
    }

    private void DisplayTest_MSB_AC6()
    {
        Test_MSB_AC6_BytePerfect.Display();
    }
    private void DisplayTest_BTL()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("BTL read/write test", buttonSize))
        {
            Test_BTL_BytePerfect.Run();
        }
    }

    private void DisplayTest_UniqueParamRows()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Insert unique rows IDs into params", buttonSize))
        {
            ParamUniqueRowFinder.Run();
        }
    }
}
