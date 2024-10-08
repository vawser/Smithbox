﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using System;
using System.Linq;

namespace StudioCore.Editors.ModelEditor
{
    public class ModelPropertyDecorator
    {
        private ModelEditorScreen Screen;
        private ModelSelectionManager Selection;
        private ModelViewportManager ViewportManager;

        public ModelPropertyDecorator(ModelEditorScreen screen)
        {
            Screen = screen;
            Selection = screen.Selection;
            ViewportManager = screen.ViewportManager;
        }

        public void GXListIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##gxListIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResManager.GetCurrentFLVER().GXLists.Count; i++)
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
                        Selection.ForceOpenGXListSection = true;
                        Selection.ResetSelection();
                        Selection._selectedGXList = index;
                        Selection._selectedFlverGroupType = GroupSelectionType.GXList;
                        Selection.FocusSelection = true;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void MaterialIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##materialIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResManager.GetCurrentFLVER().Materials.Count; i++)
                {
                    if (i == index)
                    {
                        alias = Screen.ResManager.GetCurrentFLVER().Materials[i].Name;
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
                        Selection.ForceOpenMaterialSection = true;
                        Selection.ResetSelection();
                        Selection._selectedMaterial = index;
                        Selection._selectedFlverGroupType = GroupSelectionType.Material;
                        Selection.FocusSelection = true;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void NodeIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##nodeListIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResManager.GetCurrentFLVER().Nodes.Count; i++)
                {
                    if (i == index)
                    {
                        alias = Screen.ResManager.GetCurrentFLVER().Nodes[i].Name;
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
                        Selection.ForceOpenNodeSection = true;
                        Selection.ResetSelection();
                        Selection._selectedNode = index;
                        Selection._selectedFlverGroupType = GroupSelectionType.Node;
                        Selection.FocusSelection = true;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void LayoutIndexDecorator(int index)
        {
            var alias = "";

            ImGui.AlignTextToFramePadding();
            ImGui.Selectable("##bufferLayoutIndexDecoratorSelectable", false, ImGuiSelectableFlags.AllowItemOverlap);

            if (index != -1)
            {
                for (int i = 0; i < Screen.ResManager.GetCurrentFLVER().BufferLayouts.Count; i++)
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
                        Selection.ForceOpenBufferLayoutSection = true;
                        Selection.ResetSelection();
                        Selection._selectedBufferLayout = index;
                        Selection._selectedFlverGroupType = GroupSelectionType.BufferLayout;
                        Selection.FocusSelection = true;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void LayoutTypeDecorator(int value)
        {
            var alias = "";

            var layoutType = (FLVER.LayoutType)value;
            alias = layoutType.ToString();

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }

        public void LayoutSemanticDecorator(int value)
        {
            var alias = "";

            var layoutType = (FLVER.LayoutSemantic)value;
            alias = layoutType.ToString();

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, @$"{alias}");
        }
    }
}
