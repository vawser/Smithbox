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
    }

    public void DisplayDropdown()
    {
        ImGui.Separator();

        if (ImGui.MenuItem($"Toggle ImGui Demo"))
        {
            ImGui.ShowDemoWindow();
        }

        if (ImGui.MenuItem($"Toggle Developer Panel"))
        {
            Visible = !Visible;
        }
    }

    public void Display()
    {
        if (!Visible)
            return;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if(ImGui.Begin("Developer Panel", ref Visible))
        {
            ImGui.BeginTabBar("developerTabs");

            // Quick Script
            if (ImGui.BeginTabItem("Script"))
            {
                ImGui.BeginChild("scriptSection", ImGuiChildFlags.Borders);

                UIHelper.MultiButtonInput("script",
                    "scriptExecute", "Execute Script", "", Null);

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Task Viewer
            if (ImGui.BeginTabItem("Task Viewer"))
            {
                ImGui.BeginChild("taskViewerSection", ImGuiChildFlags.Borders);
                TaskViewer.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Resource Viewer
            if (ImGui.BeginTabItem("Resource Viewer"))
            {
                ImGui.BeginChild("resourceViewerSection", ImGuiChildFlags.Borders);
                ResourceViewer.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // File Dictionary Generator
            if (ImGui.BeginTabItem("File Dictionary Generator"))
            {
                ImGui.BeginChild("fileDictSection", ImGuiChildFlags.Borders);
                FileDictionaryGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // World Map Layout Generator
            if (ImGui.BeginTabItem("World Map Generator"))
            {
                ImGui.BeginChild("worldMapSection", ImGuiChildFlags.Borders);
                WorldMapLayoutGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Doku Wiki Generator
            if (ImGui.BeginTabItem("Doku Wiki Generator"))
            {
                ImGui.BeginChild("dokuSection", ImGuiChildFlags.Borders);
                DokuWikiGenerator.Display();
                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            // Validators
            if (ImGui.BeginTabItem("Validators"))
            {
                ImGui.BeginChild("validatorSection", ImGuiChildFlags.Borders);

                UIHelper.Spacer();
                UIHelper.SimpleHeader("Validator Type", "");

                UIHelper.SetInputWidth();
                if (ImGui.BeginCombo("##validatorType", ValidatorType.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ValidatorType)))
                    {
                        var curType = (ValidatorType)entry;

                        if (ImGui.Selectable($"{curType.GetDisplayName()}", curType == ValidatorType))
                        {
                            ValidatorType = curType;
                        }
                    }

                    ImGui.EndCombo();
                }

                UIHelper.Spacer();

                if (ValidatorType is ValidatorType.None)
                {
                    ImGui.Text("No validator type has been selected.");
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

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    public void Null() { }
}

public enum ValidatorType
{
    [Display(Name = "None")]
    None,
    [Display(Name = "Param")]
    Param,
    [Display(Name = "NVA")]
    NVA,
    [Display(Name = "MSB")]
    MSB,
    [Display(Name = "BTL")]
    BTL
}
