using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ModelToolView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    public GlobalModelSearch ModelUsageSearch;

    public ModelToolView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ModelUsageSearch = new GlobalModelSearch(screen);
    }

    public void OnGui()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ModelEditor"))
        {
            Selection.SwitchWindowContext(ModelEditorContext.ToolWindow);

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
                UIHelper.Tooltip("Change the type of model export to use.");

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
                        ModelColladaExporter.ExportModel(Editor);
                    }
                    if (CFG.Current.ModelEditor_ExportType is Enums.ModelExportType.OBJ)
                    {
                        ModelObjectExporter.ExportModel(Editor);
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
                    Editor.ActionHandler.SolveBoundingBoxes();
                }
            }

            // Reverse Face Set
            if (ImGui.CollapsingHeader("Reverse Mesh Face Set"))
            {
                UIHelper.WrappedText("Reverse the currently selected face set for our selected mesh.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    Editor.ActionHandler.ReverseMeshFaceSet();
                }
            }

            // Reverse Normals
            if (ImGui.CollapsingHeader("Reverse Mesh Normals"))
            {
                UIHelper.WrappedText("Reverse the normals for the currently selected mesh.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Reverse", defaultButtonSize))
                {
                    Editor.ActionHandler.ReverseMeshNormals();
                }
            }

            ImGui.Separator();

            // FLVER Groups
            if (ImGui.CollapsingHeader("Groups: FLVER"))
            {
                FlverGroups.DisplayConfiguration(Editor);
            }
            // Dummy Groups
            if (ImGui.CollapsingHeader("Groups: Dummy"))
            {
                DummyGroups.DisplayConfiguration(Editor);
            }
            // Material Groups
            if (ImGui.CollapsingHeader("Groups: Material"))
            {
                MaterialGroups.DisplayConfiguration(Editor);
            }
            // GX List Groups
            if (ImGui.CollapsingHeader("Groups: GX List"))
            {
                GXListGroups.DisplayConfiguration(Editor);
            }
            // Node Groups
            if (ImGui.CollapsingHeader("Groups: Node"))
            {
                NodeGroups.DisplayConfiguration(Editor);
            }
            // Mesh Groups
            if (ImGui.CollapsingHeader("Groups: Mesh"))
            {
                MeshGroups.DisplayConfiguration(Editor);
            }
            // Buffer Layout Groups
            if (ImGui.CollapsingHeader("Groups: Buffer Layout"))
            {
                BufferLayoutGroups.DisplayConfiguration(Editor);
            }
            // Base Skeleton Bone Groups
            if (ImGui.CollapsingHeader("Groups: Base Skeleton Bone"))
            {
                BaseSkeletonBoneGroups.DisplayConfiguration(Editor);
            }
            // All Skeleton Bone Groups
            if (ImGui.CollapsingHeader("Groups: All Skeleton Bone"))
            {
                AllSkeletonBoneGroups.DisplayConfiguration(Editor);
            }

            ImGui.Separator();

            // Global Model Search
            if (ImGui.CollapsingHeader("Global Model Search"))
            {
                UIHelper.WrappedText("Search through all maps for usage of the specificed model name.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Model Name:");
                ImGui.InputText("##modelNameInput", ref ModelUsageSearch._searchInput, 255);

                UIHelper.WrappedText("");
                ImGui.Checkbox("Target Project Files", ref ModelUsageSearch._targetProjectFiles);
                UIHelper.Tooltip("Uses the project map files instead of game root.");
                ImGui.Checkbox("Loose Name Match", ref ModelUsageSearch._looseModelNameMatch);
                UIHelper.Tooltip("Only require the Model Name field to contain the search string, instead of requiring an exact match.");

                UIHelper.WrappedText("");

                if (ImGui.Button("Search", defaultButtonSize))
                {
                    ModelUsageSearch.SearchMaps();
                }
                UIHelper.Tooltip("Initial usage will be slow as all maps have to be loaded. Subsequent usage will be instant.");

                UIHelper.WrappedText("");

                ModelUsageSearch.DisplayInstances();
            }

            // Model Mask Toggler
            if (Editor.Selection._selectedFileModelType is FileSelectionType.Character)
            {
                if (ModelMaskToggler.IsSupportedProjectType(Editor.Project))
                {
                    if (ImGui.CollapsingHeader("Model Mask Toggler"))
                    {
                        UIHelper.WrappedText("Quickly toggle between model mask combinations by selecting a NPC Param entry.");
                        UIHelper.WrappedText("");

                        ImGui.Separator();

                        ModelMaskToggler.Display(Editor);
                    }
                }
            }

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
