using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ToolWindow
{
    private ModelEditorScreen Screen;

    public ToolWindow(ModelEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Configuration##ToolConfigureWindow_ModelEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            // Export Model
            if (ImGui.CollapsingHeader("Export Model"))
            {
                ImguiUtils.WrappedText("Export the currently loaded model to a directory as a Collada .DAE file.");
                ImguiUtils.WrappedText("");

                ImGui.Separator();
                ImguiUtils.WrappedText("Export Directory");
                ImGui.Separator();
                ImguiUtils.WrappedText($"{ModelExporter.ExportPath}");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Set Export Directory##modelExportDirectoryButton", defaultButtonSize))
                {
                    if (PlatformUtils.Instance.OpenFolderDialog("Select export directory...", out var path))
                    {
                        ModelExporter.ExportPath = path;
                    }
                }
                if(ImGui.Button("Export##modelExportApplyButton", defaultButtonSize))
                {
                    ModelExporter.ExportModel(Screen);
                }
            }

            // Solve Bounding Boxes
            if (ImGui.CollapsingHeader("Solve Bounding Boxes"))
            {
                ImguiUtils.WrappedText("Solve all of the bounding boxes for the currently loaded model.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Solve", defaultButtonSize))
                {
                    FlverTools.SolveBoundingBoxes(Screen.ResourceHandler.CurrentFLVER);
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
