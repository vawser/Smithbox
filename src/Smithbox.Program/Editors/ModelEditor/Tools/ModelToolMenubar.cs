using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ModelToolMenubar
{
    private ModelEditorScreen Editor;

    public ModelToolMenubar(ModelEditorScreen screen) 
    {
        Editor = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }
            UIHelper.Tooltip($"Display the color picker.");

            // Export Model
            if (ImGui.MenuItem("Export Model", KeyBindings.Current.MODEL_ExportModel.HintText))
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
            UIHelper.Tooltip($"Export currently loaded model.");

            // Solve Bounding Boxes
            if (ImGui.MenuItem("Solve Bounding Boxes"))
            {
                Editor.ActionHandler.SolveBoundingBoxes();
            }
            // Reverse Face Set
            if (ImGui.MenuItem("Reverse Mesh Face Set"))
            {
                Editor.ActionHandler.ReverseMeshFaceSet();
            }
            // Reverse Normals
            if (ImGui.MenuItem("Reverse Mesh Normals"))
            {
                Editor.ActionHandler.ReverseMeshNormals();
            }

            ImGui.Separator();

            // DFLVERummy Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("FLVER Groups"))
            {
                FlverGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Dummy Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Dummy Groups"))
            {
                DummyGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Material Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Material Groups"))
            {
                MaterialGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // GX List Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("GX List Groups"))
            {
                GXListGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Node Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Node Groups"))
            {
                NodeGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Mesh Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Mesh Groups"))
            {
                MeshGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Buffer Layout Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Buffer Layout Groups"))
            {
                BufferLayoutGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // Base Skeleton Bone Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("Base Skeleton Bone Groups"))
            {
                BaseSkeletonBoneGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }
            // All Skeleton Bone Groups
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.BeginMenu("All Skeleton Bone Groups"))
            {
                AllSkeletonBoneGroups.DisplaySubMenu(Editor);

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
