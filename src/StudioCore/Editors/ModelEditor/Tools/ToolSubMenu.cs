using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
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
        if (ImGui.BeginMenu("工具 Tools"))
        {
            // Export Model
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("导出模型 Export Model", KeyBindings.Current.ModelEditor_ExportModel.HintText))
            {
                ModelExporter.ExportModel(Screen);
            }
            // Solve Bounding Boxes
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("解决包围盒 Solve Bounding Boxes"))
            {
                FlverTools.SolveBoundingBoxes(Screen.ResourceHandler.CurrentFLVER);
            }
            // Reverse Face Set
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("反转网格面集合 Reverse Mesh Face Set"))
            {
                FlverTools.ReverseFaceSet(Screen);
            }
            // Reverse Normals
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("反转网格法线 Reverse Mesh Normals"))
            {
                FlverTools.ReverseNormals(Screen);
            }
            // DFLVERummy Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("模型组 FLVER Groups"))
            {
                FlverGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Dummy Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("占位组 Dummy Groups"))
            {
                DummyGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Material Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("材质组 Material Groups"))
            {
                MaterialGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // GX List Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("图形列表组 GX List Groups"))
            {
                GXListGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Node Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("节点组 Node Groups"))
            {
                NodeGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Mesh Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("网格组 Mesh Groups"))
            {
                MeshGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Buffer Layout Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("缓冲区布局组 Buffer Layout Groups"))
            {
                BufferLayoutGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // Base Skeleton Bone Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("基础骨架骨骼组 Base Skeleton Bone Groups"))
            {
                BaseSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }
            // All Skeleton Bone Groups
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("所有骨架骨骼组 All Skeleton Bone Groups"))
            {
                AllSkeletonBoneGroups.DisplaySubMenu(Screen);

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
