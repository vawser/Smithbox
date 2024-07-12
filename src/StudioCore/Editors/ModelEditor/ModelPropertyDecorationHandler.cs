using ImGuiNET;
using StudioCore.Editors.TextureViewer;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelPropertyDecorationHandler
    {
        private ModelEditorScreen Screen;

        public ModelPropertyDecorationHandler(ModelEditorScreen editor)
        {
            Screen = editor;
        }

        public void GXListIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##gxListIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.GXLists.Count; i++)
                {
                    if (i == index)
                    {
                        alias = $"GX List {i}";
                    }
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"GXListIndexAliasMenu{index}");
                }

                if (ImGui.BeginPopup($"GXListIndexAliasMenu{index}"))
                {
                    if (ImGui.Selectable($"Go to GX List##goToGXListOption{index}"))
                    {
                        Screen.ModelHierarchy.ForceOpenGXListSection = true;
                        Screen.ModelHierarchy.ResetSelection();
                        Screen.ModelHierarchy._selectedGXList = index;
                        Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.GXList;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void MaterialIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##materialIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Materials.Count; i++)
                {
                    if (i == index)
                    {
                        alias = Screen.ResourceHandler.CurrentFLVER.Materials[i].Name;
                    }
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"MaterialIndexAliasMenu{index}");
                }

                if (ImGui.BeginPopup($"MaterialIndexAliasMenu{index}"))
                {
                    if (ImGui.Selectable($"Go to Material##goToMaterialOption{index}"))
                    {
                        Screen.ModelHierarchy.ForceOpenMaterialSection = true;
                        Screen.ModelHierarchy.ResetSelection();
                        Screen.ModelHierarchy._selectedMaterial = index;
                        Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.Material;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void NodeIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##nodeListIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Nodes.Count; i++)
                {
                    if (i == index)
                    {
                        alias = Screen.ResourceHandler.CurrentFLVER.Nodes[i].Name;
                    }
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"NodeIndexAliasMenu{index}");
                }

                if (ImGui.BeginPopup($"NodeIndexAliasMenu{index}"))
                {
                    if (ImGui.Selectable($"Go to Node##goToNodeOption{index}"))
                    {
                        Screen.ModelHierarchy.ForceOpenNodeSection = true;
                        Screen.ModelHierarchy.ResetSelection();
                        Screen.ModelHierarchy._selectedNode = index;
                        Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.Node;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void LayoutIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##bufferLayoutIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count; i++)
                {
                    if (i == index)
                    {
                        alias = $"Buffer Layout {index}";
                    }
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"LayoutIndexAliasMenu{index}");
                }

                if (ImGui.BeginPopup($"LayoutIndexAliasMenu{index}"))
                {
                    if (ImGui.Selectable($"Go to Buffer Layout##goToLayoutOption{index}"))
                    {
                        Screen.ModelHierarchy.ForceOpenBufferLayoutSection = true;
                        Screen.ModelHierarchy.ResetSelection();
                        Screen.ModelHierarchy._selectedBufferLayout = index;
                        Screen.ModelHierarchy._lastSelectedEntry = ModelEntrySelectionType.BufferLayout;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{alias}");
        }
    }
}
