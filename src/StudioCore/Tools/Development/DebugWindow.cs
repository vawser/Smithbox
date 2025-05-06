using Hexa.NET.ImGui;
using SoulsFormats;
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
using System;
using static SoulsFormats.MSB_AC6;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using SoapstoneLib;
using static StudioCore.Editor.TaskManager;
using StudioCore.Configuration;

namespace StudioCore.Tools.Development;

public class DebugWindow
{
    private Smithbox BaseEditor;

    private bool MenuOpenState;

    public ParamValidationTool ParamValidationTool;
    public MapValidationTool MapValidationTool;
    public TimeActValidationTool TimeActValidationTool;
    public DokuWikiHelper DokuWikiHelper;

    public DebugWindow(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;

        ParamValidationTool = new(baseEditor);
        MapValidationTool = new(baseEditor);
        TimeActValidationTool = new(baseEditor);
        DokuWikiHelper = new(baseEditor);
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private Task _loadingTask;

    private SelectedDebugTab SelectedTab = SelectedDebugTab.DisplayTaskStatus;

    public void ToggleWindow(SelectedDebugTab focusedTab, bool ignoreIfOpen = true)
    {
        SelectedTab = focusedTab;

        if (!ignoreIfOpen)
        {
            MenuOpenState = !MenuOpenState;
        }

        if (!MenuOpenState)
        {
            MenuOpenState = true;
        }
    }

    public enum SelectedDebugTab
    {
        // Information
        [Display(Name = "Task Status")] DisplayTaskStatus,
        [Display(Name = "Resource Manager")] ResourceManager,

        // Validation
        [Display(Name = "Paramdef Validation")] ValidateParamdef,
        [Display(Name = "MSB Validation")] ValidateMSB,
        [Display(Name = "TAE Validation")] ValidateTAE,

        // Helpers
        [Display(Name = "FLVER Layout Helper")] FlverDumpHelper,
        [Display(Name = "Mod Generator")] ModGenerator,

        // Tests
        [Display(Name = "MSBE - Byte Perfect Test")] Test_MSBE_BytePerfect,
        [Display(Name = "MSB_AC6 - Byte Perfect Test")] Test_MSB_AC6_BytePerfect,
        [Display(Name = "MSBFA - Byte Perfect Test")] Test_MSBFA_BytePerfect,
        [Display(Name = "MSBV - Byte Perfect Test")] Test_MSBV_BytePerfect,
        [Display(Name = "MSBVD - Byte Perfect Test")] Test_MSBVD_BytePerfect,
        [Display(Name = "BTL - Byte Perfect Test")] Test_BTL_BytePerfect,
        [Display(Name = "Unique Param Row ID Insert")] Test_UniqueParamRowIDs,
        [Display(Name = "FLVER2 - Byte Perfect Test")] Test_FLVER2_BytePerfect,

        [Display(Name = "BHV - Read Test")] Test_BHV_File,
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Debug##TestWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            switch (SelectedTab)
            {
                // Information
                case SelectedDebugTab.DisplayTaskStatus:
                    DisplayTasks();
                    break;
                case SelectedDebugTab.ResourceManager:
                    DisplayResourceManager();
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
                case SelectedDebugTab.FlverDumpHelper:
                    DisplayHelper_FLVERDumper();
                    break;

                // Tests
                case SelectedDebugTab.Test_MSBE_BytePerfect:
                    DisplayTest_MSBE();
                    break;
                case SelectedDebugTab.Test_MSB_AC6_BytePerfect:
                    DisplayTest_MSB_AC6();
                    break;
                case SelectedDebugTab.Test_MSBFA_BytePerfect:
                    DisplayTest_MSBFA();
                    break;
                case SelectedDebugTab.Test_MSBV_BytePerfect:
                    DisplayTest_MSBV();
                    break;
                case SelectedDebugTab.Test_MSBVD_BytePerfect:
                    DisplayTest_MSBVD();
                    break;
                case SelectedDebugTab.Test_BTL_BytePerfect:
                    DisplayTest_BTL();
                    break;
                case SelectedDebugTab.Test_UniqueParamRowIDs:
                    DisplayTest_UniqueParamRows();
                    break;
                case SelectedDebugTab.Test_FLVER2_BytePerfect:
                    DisplayTest_FLVER2();
                    break;
                case SelectedDebugTab.Test_BHV_File:
                    DisplayTest_BHV();
                    break;
            }
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
            foreach ((string taskName, LiveTask task) in TaskManager.GetTasks())
            {
                ImGui.Text("" +
                    $"ID: {task.TaskId}\n" +
                    $"Location: {task.TaskName}\n" +
                    $"Success Message: {task.TaskCompletedMessage}\n" +
                    $"Failure Message: {task.TaskFailedMessage}");
            }
        }

        if (ImGui.CollapsingHeader("Soapstone Server"))
        {
            var running = SoapstoneServer.GetRunningPort() is int port
                ? $"running on port {port}"
                : "not running";
            ImGui.Text(
                $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
        }
    }
    private void DisplayResourceManager()
    {
        ResourceManagerWindow.Display();
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
        UIHelper.Tooltip("Validate that the current PARAMDEF works with the old-style SF PARAM class.");

        if (ImGui.Button("Validate Padding (for selected param)", buttonSize))
        {
            ParamValidationTool.ValidatePadding();
        }
        UIHelper.Tooltip("Validate that there are no non-zero values within padding fields.");

        if (ImGui.Button("Validate Padding (for all params)", buttonSize))
        {
            ParamValidationTool.ValidatePadding(true);
        }
        UIHelper.Tooltip("Validate that there are no non-zero values within padding fields.");

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
        UIHelper.Tooltip("The check will use the game root files by default, if you want to use your project's specific files, tick this.");
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

    private void DisplayHelper_FLVERDumper()
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
            Test_MSB_ER_BytePerfect.Run(BaseEditor);
        }
    }

    private void DisplayTest_MSB_AC6()
    {
        Test_MSB_AC6_BytePerfect.Display(BaseEditor);
    }

    private void DisplayTest_MSBFA()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("MSBFA read/write test", buttonSize))
        {
            Test_MSB_ACFA_BytePerfect.Run(BaseEditor);
        }
    }

    private void DisplayTest_MSBV()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("MSBV read/write test", buttonSize))
        {
            Test_MSB_ACV_BytePerfect.Run(BaseEditor);
        }
    }

    private void DisplayTest_MSBVD()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("MSBVD read/write test", buttonSize))
        {
            Test_MSB_ACVD_BytePerfect.Run(BaseEditor);
        }
    }

    private void DisplayTest_BTL()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("BTL read/write test", buttonSize))
        {
            Test_BTL_BytePerfect.Run(BaseEditor);
        }
    }

    private void DisplayTest_UniqueParamRows()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Insert unique rows IDs into params", buttonSize))
        {
            ParamUniqueRowFinder.Run(BaseEditor);
        }
    }
    private void DisplayTest_FLVER2()
    {
        Test_FLVER2_BytePerfect.Display(BaseEditor);
    }

    private void DisplayTest_BHV()
    {
        Test_BHV_Read_Test.Display();
    }
}
