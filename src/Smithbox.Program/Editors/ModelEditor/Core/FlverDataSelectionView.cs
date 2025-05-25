using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Scene;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor.Core;

public class FlverDataSelectionView
{
    private ModelEditorScreen Editor;
    private ModelContextMenu ContextMenu;
    private ModelSelectionManager Selection;
    private ModelResourceManager ResManager;
    private ModelFilters Filters;

    public bool SuspendView = false;

    public FlverDataSelectionView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        ResManager = screen.ResManager;
        Filters = screen.Filters;
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!CFG.Current.Interface_ModelEditor_ModelHierarchy)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Model Hierarchy##ModelEditorModelHierarchy"))
        {
            Selection.SwitchWindowContext(ModelEditorContext.ModelHierarchy);

            Filters.DisplayFlverFilter();

            if (Editor.ResManager.GetCurrentFLVER() != null && !SuspendView)
            {
                ImGui.BeginChild("modelHierarchySection");
                Selection.SwitchWindowContext(ModelEditorContext.ModelHierarchy);

                DisplaySection_Header();
                DisplaySection_Dummies();
                DisplaySection_Materials();
                DisplaySection_GXLists();
                DisplaySection_Nodes();
                DisplaySection_Meshes();
                DisplaySection_BufferLayouts();
                DisplaySection_BaseSkeletons();
                DisplaySection_AllSkeletons();
                DisplaySection_LowCollision();
                DisplaySection_HighCollision();

                ImGui.EndChild();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    private void DisplaySection_Header()
    {
        if (ImGui.Selectable("Header", Selection._selectedEntry == "Header"))
        {
            Selection.ResetSelection();
            Selection.SetHeaderSelection();
        }
    }

    /// <summary>
    /// Dummy Polgon list
    /// </summary>
    private void DisplaySection_Dummies()
    {
        // List
        if (ImGui.CollapsingHeader("Dummies"))
        {
            for (int i = 0; i < Editor.ResManager.GetCurrentFLVER().Dummies.Count; i++)
            {
                var curDummy = Editor.ResManager.GetCurrentFLVER().Dummies[i];

                if (Filters.IsModelEditorSearchMatch_Dummy(curDummy, Editor.ResManager.GetCurrentFLVER(), i))
                {
                    // Dummy Row
                    if (ImGui.Selectable($"Dummy {i} - [{curDummy.ReferenceID}]",
                        Selection.IsDummySelected(i),
                        ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection.SetDummySelection(i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectDummy)
                    {
                        Selection.SelectDummy = false;
                        Selection.SetDummySelection(i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectDummy = true;
                    }

                    if (Selection.IsDummySelected(i))
                    {
                        if (Selection.DummyMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.DummyRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.DummyRowContextMenu(i);
                        }
                    }

                    Editor.ViewportManager.DisplayRepresentativeDummyState(i);

                    if (Selection.FocusSelection && Selection.IsDummySelected(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the dummy list is empty
        if (Editor.ResManager.GetCurrentFLVER().Dummies.Count < 1)
        {
            ContextMenu.DummyHeaderContextMenu();
        }
    }

    /// <summary>
    /// Material List
    /// </summary>
    private void DisplaySection_Materials()
    {
        if (Selection.ForceOpenMaterialSection)
        {
            Selection.ForceOpenMaterialSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Materials"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().Materials.Count; i++)
            {
                var curMaterial = ResManager.GetCurrentFLVER().Materials[i];
                var materialName = curMaterial.Name;

                if (Filters.IsModelEditorSearchMatch_Material(curMaterial, ResManager.GetCurrentFLVER(), i))
                {
                    // Material Row
                    if (ImGui.Selectable($"{materialName}##material{i}", Selection.IsMaterialSelected(i)))
                    {
                        Selection.SetMaterialSelection(i, curMaterial);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectMaterial)
                    {
                        Selection.SelectMaterial = false;
                        Selection.SetMaterialSelection(i, curMaterial);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectMaterial = true;
                    }

                    if (Selection.IsMaterialSelected(i))
                    {
                        if (Selection.MaterialMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.MaterialRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.MaterialRowContextMenu(i);
                        }
                    }

                    if (Selection.FocusSelection && Selection.IsMaterialSelected(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().Materials.Count < 1)
        {
            ContextMenu.MaterialHeaderContextMenu();
        }
    }

    /// <summary>
    /// GX List List
    /// </summary>
    private void DisplaySection_GXLists()
    {
        if (Selection.ForceOpenGXListSection)
        {
            Selection.ForceOpenGXListSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("GX List"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().GXLists.Count; i++)
            {
                var curGXList = ResManager.GetCurrentFLVER().GXLists[i];

                if (Filters.IsModelEditorSearchMatch_GXList(curGXList, ResManager.GetCurrentFLVER(), i))
                {
                    // GX List Row
                    if (ImGui.Selectable($"GX List {i}", Selection.IsGxListSelected(i)))
                    {
                        Selection.SetGxListSelection(i, curGXList);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectGxList)
                    {
                        Selection.SelectGxList = false;
                        Selection.SetGxListSelection(i, curGXList);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectGxList = true;
                    }

                    if (Selection.IsGxListSelected(i))
                    {
                        if (Selection.GxListMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.GXListRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.GXListRowContextMenu(i);
                        }
                    }

                    if (Selection.FocusSelection && Selection.IsGxListSelected(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().GXLists.Count < 1)
        {
            ContextMenu.GXListHeaderContextMenu();
        }
    }

    /// <summary>
    /// Node List
    /// </summary>
    private void DisplaySection_Nodes()
    {
        if (Selection.ForceOpenNodeSection)
        {
            Selection.ForceOpenNodeSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Nodes"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().Nodes.Count; i++)
            {
                var curNode = ResManager.GetCurrentFLVER().Nodes[i];

                if (Filters.IsModelEditorSearchMatch_Node(curNode, ResManager.GetCurrentFLVER(), i))
                {
                    // Node row
                    if (ImGui.Selectable($"Node {i} - {curNode.Name}", Selection.IsNodeSelection(i), ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection.SetNodeSelection(i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNode)
                    {
                        Selection.SelectNode = false;
                        Selection.SetNodeSelection(i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectNode = true;
                    }

                    if (Selection.IsNodeSelection(i))
                    {
                        if (Selection.NodeMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.NodeRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.NodeRowContextMenu(i);
                        }
                    }

                    Editor.ViewportManager.DisplayRepresentativeNodeState(i);

                    if (Selection.FocusSelection && Selection.IsNodeSelection(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().Nodes.Count < 1)
        {
            ContextMenu.NodeHeaderContextMenu();
        }
    }

    /// <summary>
    /// Mesh List
    /// </summary>
    private void DisplaySection_Meshes()
    {
        if (ImGui.CollapsingHeader("Meshes"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().Meshes.Count; i++)
            {
                var curMesh = ResManager.GetCurrentFLVER().Meshes[i];

                var materialIndex = ResManager.GetCurrentFLVER().Meshes[i].MaterialIndex;
                var nodeIndex = ResManager.GetCurrentFLVER().Meshes[i].NodeIndex;

                var material = "";
                if (materialIndex < ResManager.GetCurrentFLVER().Materials.Count && materialIndex > -1)
                    material = ResManager.GetCurrentFLVER().Materials[materialIndex].Name;

                var node = "";
                if (nodeIndex < ResManager.GetCurrentFLVER().Nodes.Count && nodeIndex > -1)
                    node = ResManager.GetCurrentFLVER().Nodes[nodeIndex].Name;

                if (Filters.IsModelEditorSearchMatch_Mesh(curMesh, ResManager.GetCurrentFLVER(), i))
                {
                    // Mesh row
                    if (ImGui.Selectable($"Mesh {i} - {material} : {node}", Selection.IsMeshSelection(i), ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection.SetMeshSelection(i, curMesh);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectMesh)
                    {
                        Selection.SelectMesh = false;
                        Selection.SetMeshSelection(i, curMesh);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectMesh = true;
                    }

                    if (Selection.IsMeshSelection(i))
                    {
                        if (Selection.MeshMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.MeshRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.MeshRowContextMenu(i);
                        }
                    }

                    Editor.ViewportManager.DisplayRepresentativeMeshState(i);

                    if (Selection.FocusSelection && Selection.IsMeshSelection(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().Meshes.Count < 1)
        {
            ContextMenu.MeshHeaderContextMenu();
        }
    }

    /// <summary>
    /// Buffer Layout List
    /// </summary>
    private void DisplaySection_BufferLayouts()
    {
        if (Selection.ForceOpenBufferLayoutSection)
        {
            Selection.ForceOpenBufferLayoutSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Buffer Layout"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().BufferLayouts.Count; i++)
            {
                var curLayout = ResManager.GetCurrentFLVER().BufferLayouts[i];

                if (Filters.IsModelEditorSearchMatch_BufferLayout(curLayout, ResManager.GetCurrentFLVER(), i))
                {
                    // Buffer Layout row
                    if (ImGui.Selectable($"Buffer Layout {i}", Selection.IsBufferLayoutSelection(i)))
                    {
                        Selection.SetBufferLayoutSelection(i, curLayout);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectBuffer)
                    {
                        Selection.SelectBuffer = false;
                        Selection.SetBufferLayoutSelection(i, curLayout);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectBuffer = true;
                    }

                    if (Selection.IsBufferLayoutSelection(i))
                    {
                        if (Selection.BufferLayoutMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.BufferLayoutRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.BufferLayoutRowContextMenu(i, curLayout);
                        }
                    }

                    if (Selection.FocusSelection && Selection.IsBufferLayoutSelection(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().BufferLayouts.Count < 1)
        {
            ContextMenu.BufferLayoutHeaderContextMenu();
        }
    }

    /// <summary>
    /// Base Skeleton List
    /// </summary>
    private void DisplaySection_BaseSkeletons()
    {
        if (ResManager.GetCurrentFLVER().Skeletons == null)
            return;

        if (ImGui.CollapsingHeader("Base Skeleton"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton.Count; i++)
            {
                var curBaseSkeleton = ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton[i];

                var nodeIndex = ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton[i].NodeIndex;

                var node = "";
                if (nodeIndex < ResManager.GetCurrentFLVER().Nodes.Count && nodeIndex > -1)
                    node = ResManager.GetCurrentFLVER().Nodes[nodeIndex].Name;

                if (Filters.IsModelEditorSearchMatch_SkeletonBone(curBaseSkeleton, ResManager.GetCurrentFLVER(), i))
                {
                    // Base Skeleton Bone row
                    if (ImGui.Selectable($"Bone {i} - {node}##baseSkeletonBone{i}", Selection.IsBaseSkeletonSelection(i)))
                    {
                        Selection.SetBaseSkeletonSelection(i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectBaseSkeleton)
                    {
                        Selection.SelectBaseSkeleton = false;
                        Selection.SetBaseSkeletonSelection(i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectBaseSkeleton = true;
                    }

                    if (Selection.IsBaseSkeletonSelection(i))
                    {
                        if (Selection.BaseSkeletonMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.BaseSkeletonRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.BaseSkeletonRowContextMenu(i, curBaseSkeleton);
                        }
                    }

                    if (Selection.FocusSelection && Selection.IsBaseSkeletonSelection(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().Skeletons.BaseSkeleton.Count < 1)
        {
            ContextMenu.BaseSkeletonHeaderContextMenu();
        }
    }

    /// <summary>
    /// All Skeleton List
    /// </summary>
    private void DisplaySection_AllSkeletons()
    {
        if (ResManager.GetCurrentFLVER().Skeletons == null)
            return;

        if (ImGui.CollapsingHeader("All Skeleton"))
        {
            for (int i = 0; i < ResManager.GetCurrentFLVER().Skeletons.AllSkeletons.Count; i++)
            {
                var curAllSkeleton = ResManager.GetCurrentFLVER().Skeletons.AllSkeletons[i];

                var nodeIndex = ResManager.GetCurrentFLVER().Skeletons.AllSkeletons[i].NodeIndex;

                var node = "";
                if (nodeIndex < ResManager.GetCurrentFLVER().Nodes.Count && nodeIndex > -1)
                    node = ResManager.GetCurrentFLVER().Nodes[nodeIndex].Name;

                if (Filters.IsModelEditorSearchMatch_SkeletonBone(curAllSkeleton, ResManager.GetCurrentFLVER(), i))
                {
                    // All Skeleton Bone row
                    if (ImGui.Selectable($"Bone {i} - {node}##allSkeletonBone{i}", Selection.IsAllSkeletonSelection(i)))
                    {
                        Selection.SetAllSkeletonSelection(i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectAllSkeleton)
                    {
                        Selection.SelectAllSkeleton = false;
                        Selection.SetAllSkeletonSelection(i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
                    {
                        Selection.SelectAllSkeleton = true;
                    }

                    if (Selection.IsAllSkeletonSelection(i))
                    {
                        if (Selection.AllSkeletonMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.AllSkeletonRowContextMenu_MultiSelect();
                        }
                        else
                        {
                            ContextMenu.AllSkeletonRowContextMenu(i, curAllSkeleton);
                        }
                    }

                    if (Selection.FocusSelection && Selection.IsAllSkeletonSelection(i))
                    {
                        Selection.FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (ResManager.GetCurrentFLVER().Skeletons.AllSkeletons.Count < 1)
        {
            ContextMenu.AllSkeletonHeaderContextMenu();
        }
    }

    /// <summary>
    /// Low Collision List
    /// </summary>
    private void DisplaySection_LowCollision()
    {
        if (ResManager.LoadedFlverContainer.ER_LowCollision == null)
            return;

        // Currently only one collision is supported in this list
        var i = 0;

        if (ImGui.CollapsingHeader("Low Collision"))
        {
            // Low Collision Row
            if (ImGui.Selectable($"Low Collision##lowCollision_0", Selection.IsLowCollisionSelection(i)))
            {
                Selection.SetLowCollisionSelection(i);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectLowCollision)
            {
                Selection.SelectLowCollision = false;
                Selection.SetLowCollisionSelection(i);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                Selection.SelectLowCollision = true;
            }

            if (Selection.IsLowCollisionSelection(i))
            {
                ContextMenu.LowCollisionContextMenu(i);
            }

            if (Selection.FocusSelection && Selection.IsLowCollisionSelection(i))
            {
                Selection.FocusSelection = false;
                ImGui.SetScrollHereY();
            }
        }
    }

    /// <summary>
    /// High Collision List
    /// </summary>
    private void DisplaySection_HighCollision()
    {
        if (ResManager.LoadedFlverContainer.ER_HighCollision == null)
            return;

        // Currently only one collision is supported in this list
        var i = 0;

        if (ImGui.CollapsingHeader("High Collision"))
        {
            // High Collision Row
            if (ImGui.Selectable($"High Collision##highCollision_0", Selection.IsHighCollisionSelection(i)))
            {
                Selection.SetHighCollisionSelection(i);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectHighCollision)
            {
                Selection.SelectHighCollision = false;
                Selection.SetHighCollisionSelection(i);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                Selection.SelectHighCollision = true;
            }

            if (Selection.IsHighCollisionSelection(i))
            {
                ContextMenu.HighCollisionContextMenu(i);
            }

            if (Selection.FocusSelection && Selection.IsHighCollisionSelection(i))
            {
                Selection.FocusSelection = false;
                ImGui.SetScrollHereY();
            }
        }
    }
}
