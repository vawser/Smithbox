using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
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
    public ModelUsageSearch ModelUsageSearch;

    public ToolWindow(ModelEditorScreen screen)
    {
        Screen = screen;
        ModelUsageSearch = new ModelUsageSearch(screen);
    }

    public void OnProjectChanged()
    {
        ModelUsageSearch.OnProjectChanged();
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ModelEditor"))
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

            // Reverse Face Set
            if (ImGui.CollapsingHeader("Reverse Mesh Face Set"))
            {
                ImguiUtils.WrappedText("Reverse the currently selected face set for our selected mesh.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    FlverTools.ReverseFaceSet(Screen);
                }
            }

            // Reverse Normals
            if (ImGui.CollapsingHeader("Reverse Mesh Normals"))
            {
                ImguiUtils.WrappedText("Reverse the normals for the currently selected mesh.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    FlverTools.ReverseNormals(Screen);
                }
            }

            // FLVER Groups
            if (ImGui.CollapsingHeader("Groups: FLVER"))
            {
                FlverGroups.DisplayConfiguration(Screen);
            }
            // Dummy Groups
            if (ImGui.CollapsingHeader("Groups: Dummy"))
            {
                DummyGroups.DisplayConfiguration(Screen);
            }
            // Material Groups
            if (ImGui.CollapsingHeader("Groups: Material"))
            {
                MaterialGroups.DisplayConfiguration(Screen);
            }
            // GX List Groups
            if (ImGui.CollapsingHeader("Groups: GX List"))
            {
                GXListGroups.DisplayConfiguration(Screen);
            }
            // Node Groups
            if (ImGui.CollapsingHeader("Groups: Node"))
            {
                NodeGroups.DisplayConfiguration(Screen);
            }
            // Mesh Groups
            if (ImGui.CollapsingHeader("Groups: Mesh"))
            {
                MeshGroups.DisplayConfiguration(Screen);
            }
            // Buffer Layout Groups
            if (ImGui.CollapsingHeader("Groups: Buffer Layout"))
            {
                BufferLayoutGroups.DisplayConfiguration(Screen);
            }
            // Base Skeleton Bone Groups
            if (ImGui.CollapsingHeader("Groups: Base Skeleton Bone"))
            {
                BaseSkeletonBoneGroups.DisplayConfiguration(Screen);
            }
            // All Skeleton Bone Groups
            if (ImGui.CollapsingHeader("Groups: All Skeleton Bone"))
            {
                AllSkeletonBoneGroups.DisplayConfiguration(Screen);
            }

            // Search for Usage
            if (ImGui.CollapsingHeader("Search for Usage"))
            {
                ImguiUtils.WrappedText("Search through all maps for usage of the specificed model name.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Model Name:");
                ImGui.InputText("##modelNameInput", ref ModelUsageSearch._searchInput, 255);

                ImguiUtils.WrappedText("");
                ImGui.Checkbox("Target Project Files", ref ModelUsageSearch._targetProjectFiles);
                ImguiUtils.ShowHoverTooltip("Uses the project map files instead of game root.");
                ImGui.Checkbox("Loose Name Match", ref ModelUsageSearch._looseModelNameMatch);
                ImguiUtils.ShowHoverTooltip("Only require the Model Name field to contain the search string, instead of requiring an exact match.");

                ImguiUtils.WrappedText("");

                if (ImGui.Button("Search", defaultButtonSize))
                {
                    ModelUsageSearch.SearchMaps();
                }

                ImguiUtils.WrappedText("");

                ModelUsageSearch.DisplayInstances();
            }

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
