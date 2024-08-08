using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ToolSubMenu
{
    private ModelEditorScreen Screen;

    public ToolSubMenu(ModelEditorScreen screen) 
    {
        Screen = screen;
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            // Export Model
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Export Model", KeyBindings.Current.ModelEditor_ExportModel.HintText))
            {
                ModelExporter.ExportModel(Screen);
            }
            // Solve Bounding Boxes
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Solve Bounding Boxes"))
            {
                FlverTools.SolveBoundingBoxes(Screen.ResourceHandler.CurrentFLVER);
            }
            // Reverse Face Set
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Face Set"))
            {
                FlverTools.ReverseFaceSet(Screen);
            }
            // Reverse Normals
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Normals"))
            {
                FlverTools.ReverseNormals(Screen);
            }
            // DFLVERummy Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("FLVER Groups"))
            {
                FlverGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Dummy Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Dummy Groups"))
            {
                DummyGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Material Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Material Groups"))
            {
                MaterialGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // GX List Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("GX List Groups"))
            {
                GXListGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Node Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Node Groups"))
            {
                NodeGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Mesh Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Mesh Groups"))
            {
                MeshGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Buffer Layout Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Material Groups"))
            {
                BufferLayoutGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Base Skeleton Bone Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Base Skeleton Bone Groups"))
            {
                BaseSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // All Skeleton Bone Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("All Skeleton Bone Groups"))
            {
                AllSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
