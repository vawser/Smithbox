using Hexa.NET.ImGui;
using HKLib.hk2018.hk;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Developer;

public class DeveloperPanel
{
    private bool Visible = false;

    private ResourceViewer ResourceViewer;
    private TaskViewer TaskViewer;
    private WorldMapLayoutGenerator WorldMapLayoutGenerator;
    private DokuWikiGenerator DokuWikiGenerator;
    private FileDictionaryGenerator FileDictionaryGenerator;

    private ValidatorType ValidatorType = ValidatorType.None;

    private ParamValidator ParamValidator;
    private NvaValidator NvaValidator;
    private MsbValidator MsbValidator;
    private BtlValidator BtlValidator;
    private GparamValidator GparamValidator;

    public bool ShowDemoWindow = false;

    public DeveloperPanel() 
    {
        ResourceViewer = new();
        TaskViewer = new();

        WorldMapLayoutGenerator = new();
        DokuWikiGenerator = new();
        FileDictionaryGenerator = new();

        ParamValidator = new();
        NvaValidator = new();
        MsbValidator = new();
        BtlValidator = new();
        GparamValidator = new();
    }

    public void DisplayDropdown()
    {
        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("DEV_PNL_Toggle_ImGui_Demo")}##toggleImguiDemo"))
        {
            ShowDemoWindow = !ShowDemoWindow;
        }

        if (ImGui.MenuItem($"{LOC.Get("DEV_PNL_Toggle_Developer_Panel")}##toggleDeveloperPanel"))
        {
            Visible = !Visible;
        }
    }

    public void Display(uint mainDockspaceID)
    {
        if (!Visible)
            return;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (ImGui.Begin($"{LOC.Get("DEV_Window_Developer_Panel")}###developerPanel", ref Visible, GUI.GetFloatingWindowFlags()))
        {
            ImGui.BeginTabBar("developerTabs");

            // Quick Script
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_Script")}##scriptTab"))
            {
                ImGui.BeginChild("scriptSection", ImGuiChildFlags.Borders);

                GUI.MultiButtonInput("script",
                    "scriptExecute",
                    LOC.Get("DEV_PNL_Action_Execute_Script"),
                    LOC.Get("DEV_PNL_Action_Execute_Script_TT"),
                    ExecuteScript);

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Task Viewer
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_Task_Viewer")}##taskViewerTab"))
            {
                ImGui.BeginChild("taskViewerSection", ImGuiChildFlags.Borders);
                TaskViewer.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Resource Viewer
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_Resource_Viewer")}##resourceViewerTab"))
            {
                ImGui.BeginChild("resourceViewerSection", ImGuiChildFlags.Borders);
                ResourceViewer.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // File Dictionary Generator
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_File_Dictionary_Generator")}##fileDictionaryGeneratorTab"))
            {
                ImGui.BeginChild("fileDictSection", ImGuiChildFlags.Borders);
                FileDictionaryGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // World Map Layout Generator
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_World_Map_Generator")}##worldMapGeneratorTab"))
            {
                ImGui.BeginChild("worldMapSection", ImGuiChildFlags.Borders);
                WorldMapLayoutGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Doku Wiki Generator
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_Doku_Wiki_Generator")}##dokuWikiTab"))
            {
                ImGui.BeginChild("dokuSection", ImGuiChildFlags.Borders);
                DokuWikiGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Validators
            if (ImGui.BeginTabItem($"{LOC.Get("DEV_PNL_Tab_Validators")}##validatorsTab"))
            {
                ImGui.BeginChild("validatorSection", ImGuiChildFlags.Borders);

                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("DEV_PNL_Header_Validator_Type"),
                    LOC.Get("DEV_PNL_Header_Validator_Type_TT"));

                GUI.SetInputWidth();

                var previewName = LOC.Get(ValidatorType.GetDisplayName());

                if (ImGui.BeginCombo("##validatorType", previewName))
                {
                    foreach (var entry in Enum.GetValues(typeof(ValidatorType)))
                    {
                        var curType = (ValidatorType)entry;

                        var displayName = LOC.Get(curType.GetDisplayName());

                        if (ImGui.Selectable(displayName, curType == ValidatorType))
                        {
                            ValidatorType = curType;
                        }
                    }

                    ImGui.EndCombo();
                }

                GUI.Spacer();

                if (ValidatorType is ValidatorType.None)
                {
                    ImGui.Text(LOC.Get("DEV_PNL_No_Validator_Type_Selected"));
                }
                else if (ValidatorType is ValidatorType.Param)
                {
                    ParamValidator.Display();
                }
                else if (ValidatorType is ValidatorType.MSB)
                {
                    MsbValidator.Display();
                }
                else if (ValidatorType is ValidatorType.BTL)
                {
                    BtlValidator.Display();
                }
                else if (ValidatorType is ValidatorType.NVA)
                {
                    NvaValidator.Display();
                }
                else if (ValidatorType is ValidatorType.GPARAM)
                {
                    GparamValidator.Display();
                }

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    public void ExecuteScript() 
    {
        ParamMetadata.GenerateAnnotations(Smithbox.Orchestrator.SelectedProject);
    }
}

public enum ValidatorType
{
    [Display(Name = "DEV_ENUM_Validator_Type_None")]
    None,
    [Display(Name = "DEV_ENUM_Validator_Type_Param")]
    Param,
    [Display(Name = "DEV_ENUM_Validator_Type_NVA")]
    NVA,
    [Display(Name = "DEV_ENUM_Validator_Type_MSB")]
    MSB,
    [Display(Name = "DEV_ENUM_Validator_Type_BTL")]
    BTL,
    [Display(Name = "DEV_ENUM_Validator_Type_GPARAM")]
    GPARAM
}
