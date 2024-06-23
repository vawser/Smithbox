using ImGuiNET;
using StudioCore.Core;
using StudioCore.Platform;
using StudioCore.Tools;
using System;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class ToolWindow
{
    private bool MenuOpenState;

    private Vector4 currentColor;

    public ToolWindow()
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

        if (ImGui.Begin("Tools##quickToolWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Checkbox("Show Development Tools", ref CFG.Current.ShowDeveloperTools);
            ImguiUtils.ShowHoverTooltip("This will toggle the display of 'development' tools, which are not normally of interest to users.");

            ImGui.BeginTabBar("#QuickToolMenuBar");

            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            DisplayTool_ColorPicker();
            DisplayTool_MSB_Report();

            if (CFG.Current.ShowDeveloperTools)
            {
                DisplayTool_ParamValidation();
                DisplayTool_MapValidation();
                DisplayTool_WorldMapLayoutGenerator();
            }

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private void DisplayTool_ColorPicker()
    {
        if (ImGui.BeginTabItem("Color Picker"))
        {
            ImGui.ColorPicker4("##colorPicker", ref currentColor);

            if (ImGui.Button("Copy RGB Color"))
            {
                var rgbColor = $"<{Math.Round(currentColor.X * 255)}, {Math.Round(currentColor.Y * 255)}, {Math.Round(currentColor.Z * 255)}>";

                PlatformUtils.Instance.SetClipboardText(rgbColor);
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy Decimal Color"))
            {
                PlatformUtils.Instance.SetClipboardText(currentColor.ToString());
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_MSB_Report()
    {
        if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (ImGui.BeginTabItem("Map Information"))
            {
                ImGui.Text("This tool will dump all of the information with each MSB file to text, presenting it in a readable and searchable fashion.");

                ImGui.Text("Export Path: " + MapInformationTool.exportPath);

                ImGui.Checkbox("Use project files", ref MapInformationTool.TargetProject);
                ImguiUtils.ShowHoverTooltip("The report will use the game root files by default, if you want to use your project's specific files, tick this.");

                //ImGui.SameLine();
                //ImGui.Checkbox("Export as single file", ref MapInformationTool.OneFile);
                //ImguiUtils.ShowHoverTooltip("The report will be placed in one file, and each MSB will be separated by a header.");

                if (ImGui.Button("Select Report Export Directory"))
                {
                    MapInformationTool.SelectExportDirectory();
                }
                ImguiUtils.ShowHoverTooltip("Select the directory that the MSB text files will be placed in. There will be one file for each MSB.");

                if (ImGui.Button("Generate Report"))
                {
                    MapInformationTool.GenerateReport();
                }

                /*
                if (ImGui.Button("Target Report"))
                {
                    MapInformationTool.GenerateTargetReport();
                }
                */

                ImGui.Separator();



                ImGui.EndTabItem();
            }
        }
    }

    private void DisplayTool_ParamValidation()
    {
        if (ImGui.BeginTabItem("Param Validation"))
        {
            ImGui.Text("This tool will validate the PARAMDEF and padding values. Issues will be printed to the Logger.");

            if(ImGui.Button("Validate PARAMDEF"))
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

            if(MapValidationTool.HasFinished)
            {
                ImGui.Text("Validation has finished.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayTool_WorldMapLayoutGenerator()
    {
        if (Smithbox.ProjectType == ProjectType.ER)
        {
            if (ImGui.BeginTabItem("World Map Layout Generator"))
            {
                ImGui.Text("This tool will generate the .layout file used by the World Map feature.");

                ImGui.Text("Export Path: " + WorldMapLayoutGenerator.exportPath);

                if (ImGui.Button("Select Layout Export Directory"))
                {
                    WorldMapLayoutGenerator.SelectExportDirectory();
                }
                ImguiUtils.ShowHoverTooltip("Select the directory that the world map layout file will be placed in.");

                if (ImGui.Button("Generate Layout"))
                {
                    WorldMapLayoutGenerator.CalcWorldMapLayout();
                }

                ImGui.EndTabItem();
            }
        }
    }
}
