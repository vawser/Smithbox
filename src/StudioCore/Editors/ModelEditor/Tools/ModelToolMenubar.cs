using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ModelToolMenubar
{
    private ModelEditorScreen Screen;

    public ModelToolMenubar(ModelEditorScreen screen) 
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            // Export Model
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Export Model", KeyBindings.Current.MODEL_ExportModel.HintText))
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

            // Solve Bounding Boxes
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Solve Bounding Boxes"))
            {
                if (Screen.ResManager.HasCurrentFLVER())
                {
                    FlverTools.SolveBoundingBoxes(Screen.ResManager.GetCurrentFLVER());
                }
            }
            // Reverse Face Set
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Face Set"))
            {
                FlverTools.ReverseFaceSet(Screen);
            }
            // Reverse Normals
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Normals"))
            {
                FlverTools.ReverseNormals(Screen);
            }
            // DFLVERummy Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("FLVER Groups"))
            {
                FlverGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Dummy Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Dummy Groups"))
            {
                DummyGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Material Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Material Groups"))
            {
                MaterialGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // GX List Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("GX List Groups"))
            {
                GXListGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Node Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Node Groups"))
            {
                NodeGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Mesh Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Mesh Groups"))
            {
                MeshGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Buffer Layout Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Buffer Layout Groups"))
            {
                BufferLayoutGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Base Skeleton Bone Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Base Skeleton Bone Groups"))
            {
                BaseSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // All Skeleton Bone Groups
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("All Skeleton Bone Groups"))
            {
                AllSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
