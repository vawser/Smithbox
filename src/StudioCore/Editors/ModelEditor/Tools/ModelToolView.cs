using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Configuration.Settings.TimeActEditorTab;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ModelToolView
{
    private ModelEditorScreen Screen;
    public GlobalModelSearch ModelUsageSearch;

    private bool ObjIncludeTextures = true;

    public ModelToolView(ModelEditorScreen screen)
    {
        Screen = screen;
        ModelUsageSearch = new GlobalModelSearch(screen);
    }

    public void OnProjectChanged()
    {
        ModelUsageSearch.OnProjectChanged();
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ModelEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            // Export Model
            if (ImGui.CollapsingHeader("Export Model"))
            {
                UIHelper.WrappedText("Export the currently loaded model to a directory");
                UIHelper.WrappedText("");

                if (ImGui.BeginCombo("Export Type", CFG.Current.ModelEditor_ExportType.ToString()))
                {
                    foreach (var entry in Enum.GetValues(typeof(ModelExportType)))
                    {
                        var type = (ModelExportType)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.ModelEditor_ExportType = (ModelExportType)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
                UIHelper.ShowHoverTooltip("Change the type of model export to use.");

                if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.OBJ)
                {
                    // TODO
                    //ImGui.Checkbox("Include Textures", ref ObjIncludeTextures);
                    //UIHelper.ShowHoverTooltip("The diffuse textures will be exported alongside the model.");
                }

                ImGui.Separator();
                UIHelper.WrappedText("Export Directory");
                ImGui.Separator();

                if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.DAE)
                {
                    UIHelper.WrappedText($"{ModelColladaExporter.ExportPath}");
                }
                if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.OBJ)
                {
                    UIHelper.WrappedText($"{ModelObjectExporter.ExportPath}");
                }
                UIHelper.WrappedText("");

                if (ImGui.Button("Set Export Directory##modelExportDirectoryButton", defaultButtonSize))
                {
                    if (PlatformUtils.Instance.OpenFolderDialog("Select export directory...", out var path))
                    {
                        if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.DAE)
                        {
                            ModelColladaExporter.ExportPath = path;
                        }
                        if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.OBJ)
                        {
                            ModelObjectExporter.ExportPath = path;
                        }
                    }
                }
                if(ImGui.Button("Export##modelExportApplyButton", defaultButtonSize))
                {
                    if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.DAE)
                    {
                        ModelColladaExporter.ExportModel(Screen);
                    }
                    if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.OBJ)
                    {
                        ModelObjectExporter.ExportModel(Screen);
                    }
                }
            }

            // Solve Bounding Boxes
            if (ImGui.CollapsingHeader("Solve Bounding Boxes"))
            {
                UIHelper.WrappedText("Solve all of the bounding boxes for the currently loaded model.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Solve", defaultButtonSize))
                {
                    Screen.ActionHandler.SolveBoundingBoxes();
                }
            }

            // Reverse Face Set
            if (ImGui.CollapsingHeader("Reverse Mesh Face Set"))
            {
                UIHelper.WrappedText("Reverse the currently selected face set for our selected mesh.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    Screen.ActionHandler.ReverseMeshFaceSet();
                }
            }

            // Reverse Normals
            if (ImGui.CollapsingHeader("Reverse Mesh Normals"))
            {
                UIHelper.WrappedText("Reverse the normals for the currently selected mesh.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    Screen.ActionHandler.ReverseMeshNormals();
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

            // Global Model Search
            if (ImGui.CollapsingHeader("Global Model Search"))
            {
                UIHelper.WrappedText("Search through all maps for usage of the specificed model name.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Model Name:");
                ImGui.InputText("##modelNameInput", ref ModelUsageSearch._searchInput, 255);

                UIHelper.WrappedText("");
                ImGui.Checkbox("Target Project Files", ref ModelUsageSearch._targetProjectFiles);
                UIHelper.ShowHoverTooltip("Uses the project map files instead of game root.");
                ImGui.Checkbox("Loose Name Match", ref ModelUsageSearch._looseModelNameMatch);
                UIHelper.ShowHoverTooltip("Only require the Model Name field to contain the search string, instead of requiring an exact match.");

                UIHelper.WrappedText("");

                if (ImGui.Button("Search", defaultButtonSize))
                {
                    ModelUsageSearch.SearchMaps();
                }
                UIHelper.ShowHoverTooltip("Initial usage will be slow as all maps have to be loaded. Subsequent usage will be instant.");

                UIHelper.WrappedText("");

                ModelUsageSearch.DisplayInstances();
            }

            // Model Mask Toggler
            if (Screen.Selection._selectedFileModelType is FileSelectionType.Character)
            {
                if (ModelMaskToggler.IsSupportedProjectType())
                {
                    if (ImGui.CollapsingHeader("Model Mask Toggler"))
                    {
                        UIHelper.WrappedText("Quickly toggle between model mask combinations by selecting a NPC Param entry.");
                        UIHelper.WrappedText("");

                        ImGui.Separator();

                        ModelMaskToggler.Display();
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
