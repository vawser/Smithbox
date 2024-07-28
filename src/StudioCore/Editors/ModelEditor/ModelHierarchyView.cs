using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editor;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor;

public class ModelHierarchyView
{
    private ModelEditorScreen Screen;
    private HierarchyContextMenu ContextMenu;

    public HierarchyMultiselect DummyMultiselect;
    public HierarchyMultiselect MaterialMultiselect;
    public HierarchyMultiselect GxListMultiselect;
    public HierarchyMultiselect NodeMultiselect;
    public HierarchyMultiselect MeshMultiselect;
    public HierarchyMultiselect BufferLayoutMultiselect;
    public HierarchyMultiselect BaseSkeletonMultiselect;
    public HierarchyMultiselect AllSkeletonMultiselect;

    public string _searchInput = "";

    public bool SuspendView = false;
    public bool FocusSelection = false;

    public ModelHierarchyView(ModelEditorScreen editor)
    {
        Screen = editor;
        ContextMenu = new HierarchyContextMenu(Screen);

        DummyMultiselect = new HierarchyMultiselect();
        MaterialMultiselect = new HierarchyMultiselect();
        GxListMultiselect = new HierarchyMultiselect();
        NodeMultiselect = new HierarchyMultiselect();
        MeshMultiselect = new HierarchyMultiselect();
        BufferLayoutMultiselect = new HierarchyMultiselect();
        BaseSkeletonMultiselect = new HierarchyMultiselect();
        AllSkeletonMultiselect = new HierarchyMultiselect();
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!CFG.Current.Interface_ModelEditor_ModelHierarchy)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Model Hierarchy##ModelEditorModelHierarchy"))
        {
            ImGui.InputText($"Search", ref _searchInput, 255);
            ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
            ImGui.SameLine();
            ImGui.Checkbox("##exactSearch", ref CFG.Current.ModelEditor_ExactSearch);
            ImguiUtils.ShowHoverTooltip("Enable exact search.");

            if (Screen.ResourceHandler.CurrentFLVER != null && !SuspendView)
            {
                DisplaySection_Header();
                DisplaySection_Dummies();
                DisplaySection_Materials();
                DisplaySection_GXLists();
                DisplaySection_Nodes();
                DisplaySection_Meshes();
                DisplaySection_BufferLayouts();
                DisplaySection_Skeletons();
                DisplaySection_Collision();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
        }
    }

    public ModelEntrySelectionType _lastSelectedEntry = ModelEntrySelectionType.None;
    private string _selectedEntry = "";
    public int _selectedDummy = -1;
    public int _selectedMaterial = -1;
    public int _selectedGXList = -1;
    public int _selectedNode = -1;
    public int _selectedMesh = -1;
    public int _selectedBufferLayout = -1;
    public int _selectedBaseSkeletonBone = -1;
    public int _selectedAllSkeletonBone = -1;

    public int _subSelectedTextureRow = -1;
    public int _subSelectedGXItemRow = -1;
    public int _subSelectedFaceSetRow = -1;
    public int _subSelectedVertexBufferRow = -1;
    public int _subSelectedBufferLayoutMember = -1;

    public int _selectedLowCollision = -1;
    public int _selectedHighCollision = -1;

    public void ResetSelection()
    {
        _selectedEntry = "";
        _lastSelectedEntry = ModelEntrySelectionType.None;
        _selectedDummy = -1;
        _selectedMaterial = -1;
        _selectedGXList = -1;
        _selectedNode = -1;
        _selectedMesh = -1;
        _selectedBufferLayout = -1;
        _selectedBaseSkeletonBone = -1;
        _selectedAllSkeletonBone = -1;
        _subSelectedTextureRow = -1;
        _subSelectedGXItemRow = -1;
        _subSelectedFaceSetRow = -1;
        _subSelectedVertexBufferRow = -1;
        _subSelectedBufferLayoutMember = -1;
        _selectedLowCollision = -1;
        _selectedHighCollision = -1;
    }
    public void ResetMultiSelection()
    {
        DummyMultiselect = new HierarchyMultiselect();
        MaterialMultiselect = new HierarchyMultiselect();
        GxListMultiselect = new HierarchyMultiselect();
        NodeMultiselect = new HierarchyMultiselect();
        MeshMultiselect = new HierarchyMultiselect();
        BufferLayoutMultiselect = new HierarchyMultiselect();
        BaseSkeletonMultiselect = new HierarchyMultiselect();
        AllSkeletonMultiselect = new HierarchyMultiselect();
    }


    private void DisplaySection_Header()
    {
        if(ImGui.Selectable("Header", _selectedEntry == "Header"))
        {
            ResetSelection();
            _selectedEntry = "Header";
            _lastSelectedEntry = ModelEntrySelectionType.Header;
        }
    }

    private void DisplaySection_Dummies()
    {
        if (ImGui.CollapsingHeader("Dummies"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Dummies.Count; i++)
            {
                var curDummy = Screen.ResourceHandler.CurrentFLVER.Dummies[i];

                if (ModelEditorSearch.IsModelEditorSearchMatch_Dummy(_searchInput, curDummy, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Dummy {i} - [{curDummy.ReferenceID}]", (DummyMultiselect.IsMultiselected(i) || _selectedDummy == i), ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        DummyMultiselect.HandleMultiselect(_selectedDummy, i);

                        ResetSelection();
                        _selectedDummy = i;
                        _lastSelectedEntry = ModelEntrySelectionType.Dummy;

                        Screen.ModelPropertyEditor._trackedDummyPosition = new Vector3();

                        Screen.ViewportHandler.SelectRepresentativeDummy(_selectedDummy, DummyMultiselect);
                    }

                    if (_selectedDummy == i)
                    {
                        if (DummyMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.DummyRowContextMenu_MultiSelect(DummyMultiselect);
                        }
                        else
                        {
                            ContextMenu.DummyRowContextMenu(i);
                        }
                    }

                    Screen.ViewportHandler.DisplayRepresentativeDummyState(i);

                    if (FocusSelection && _selectedDummy == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the dummy list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Dummies.Count < 1)
        {
            ContextMenu.DummyHeaderContextMenu();
        }
    }

    public bool ForceOpenMaterialSection = false;

    private void DisplaySection_Materials()
    {
        if (ForceOpenMaterialSection)
        {
            ForceOpenMaterialSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Materials"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Materials.Count; i++)
            {
                var curMaterial = Screen.ResourceHandler.CurrentFLVER.Materials[i];
                var materialName = curMaterial.Name;

                if (ModelEditorSearch.IsModelEditorSearchMatch_Material(_searchInput, curMaterial, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"{materialName}##material{i}", (MaterialMultiselect.IsMultiselected(i) || _selectedMaterial == i)))
                    {
                        MaterialMultiselect.HandleMultiselect(_selectedMaterial, i);

                        ResetSelection();
                        _selectedMaterial = i;
                        _lastSelectedEntry = ModelEntrySelectionType.Material;

                        if(curMaterial.Textures.Count > 0)
                        {
                            _subSelectedTextureRow = 0;
                        }
                    }

                    if (_selectedMaterial == i)
                    {
                        if (MaterialMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.MaterialRowContextMenu_MultiSelect(MaterialMultiselect);
                        }
                        else
                        {
                            ContextMenu.MaterialRowContextMenu(i);
                        }
                    }

                    if (FocusSelection && _selectedMaterial == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Materials.Count < 1)
        {
            ContextMenu.MaterialHeaderContextMenu();
        }
    }

    public bool ForceOpenGXListSection = false;

    private void DisplaySection_GXLists()
    {
        if (ForceOpenGXListSection)
        {
            ForceOpenGXListSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("GX List"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.GXLists.Count; i++)
            {
                var curGXList = Screen.ResourceHandler.CurrentFLVER.GXLists[i];

                if (ModelEditorSearch.IsModelEditorSearchMatch_GXList(_searchInput, curGXList, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"GX List {i}", (GxListMultiselect.IsMultiselected(i) ||  _selectedGXList == i)))
                    {
                        GxListMultiselect.HandleMultiselect(_selectedGXList, i);

                        ResetSelection();
                        _selectedGXList = i;
                        _lastSelectedEntry = ModelEntrySelectionType.GXList;

                        if (curGXList.Count > 0)
                        {
                            _subSelectedGXItemRow = 0;
                        }
                    }

                    if (_selectedGXList == i)
                    {
                        if (GxListMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.GXListRowContextMenu_MultiSelect(GxListMultiselect);
                        }
                        else
                        {
                            ContextMenu.GXListRowContextMenu(i);
                        }
                    }

                    if (FocusSelection && _selectedGXList == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.GXLists.Count < 1)
        {
            ContextMenu.GXListHeaderContextMenu();
        }
    }

    public bool ForceOpenNodeSection = false;

    private void DisplaySection_Nodes()
    {
        if (ForceOpenNodeSection)
        {
            ForceOpenNodeSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Nodes"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Nodes.Count; i++)
            {
                var curNode = Screen.ResourceHandler.CurrentFLVER.Nodes[i];

                if (ModelEditorSearch.IsModelEditorSearchMatch_Node(_searchInput, curNode, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Node {i} - {curNode.Name}", (NodeMultiselect.IsMultiselected(i) || _selectedNode == i), ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        NodeMultiselect.HandleMultiselect(_selectedNode, i);

                        ResetSelection();
                        _selectedNode = i;
                        _lastSelectedEntry = ModelEntrySelectionType.Node;

                        Screen.ModelPropertyEditor._trackedNodePosition = new Vector3();

                        Screen.ViewportHandler.SelectRepresentativeNode(_selectedNode);
                    }

                    if (_selectedNode == i)
                    {
                        if (NodeMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.NodeRowContextMenu_MultiSelect(NodeMultiselect);
                        }
                        else
                        {
                            ContextMenu.NodeRowContextMenu(i);
                        }
                    }

                    Screen.ViewportHandler.DisplayRepresentativeNodeState(i);

                    if (FocusSelection && _selectedNode == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Nodes.Count < 1)
        {
            ContextMenu.NodeHeaderContextMenu();
        }
    }

    private void DisplaySection_Meshes()
    {
        if (ImGui.CollapsingHeader("Meshes"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Meshes.Count; i++)
            {
                var curMesh = Screen.ResourceHandler.CurrentFLVER.Meshes[i];

                var materialIndex = Screen.ResourceHandler.CurrentFLVER.Meshes[i].MaterialIndex;
                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Meshes[i].NodeIndex;

                var material = "";
                if (materialIndex < Screen.ResourceHandler.CurrentFLVER.Materials.Count)
                    material = Screen.ResourceHandler.CurrentFLVER.Materials[materialIndex].Name;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count && nodeIndex > -1)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ModelEditorSearch.IsModelEditorSearchMatch_Mesh(_searchInput, curMesh, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Mesh {i} - {material} : {node}", (MeshMultiselect.IsMultiselected(i) || _selectedMesh == i), ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        MeshMultiselect.HandleMultiselect(_selectedMesh, i);

                        ResetSelection();
                        _selectedMesh = i;
                        _lastSelectedEntry = ModelEntrySelectionType.Mesh;

                        if (curMesh.FaceSets.Count > 0)
                        {
                            _subSelectedFaceSetRow = 0;
                        }

                        if (curMesh.VertexBuffers.Count > 0)
                        {
                            _subSelectedVertexBufferRow = 0;
                        }

                        Screen.ViewportHandler.SelectRepresentativeMesh(_selectedMesh);
                    }

                    if (_selectedMesh == i)
                    {
                        if (MeshMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.MeshRowContextMenu_MultiSelect(MeshMultiselect);
                        }
                        else
                        {
                            ContextMenu.MeshRowContextMenu(i);
                        }
                    }

                    Screen.ViewportHandler.DisplayRepresentativeMeshState(i);

                    if (FocusSelection && _selectedMesh == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Meshes.Count < 1)
        {
            ContextMenu.MeshHeaderContextMenu();
        }
    }

    public bool ForceOpenBufferLayoutSection = false;

    private void DisplaySection_BufferLayouts()
    {
        if (ForceOpenBufferLayoutSection)
        {
            ForceOpenBufferLayoutSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Buffer Layout"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count; i++)
            {
                var curLayout = Screen.ResourceHandler.CurrentFLVER.BufferLayouts[i];

                if (ModelEditorSearch.IsModelEditorSearchMatch_BufferLayout(_searchInput, curLayout, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Buffer Layout {i}", (BufferLayoutMultiselect.IsMultiselected(i) || _selectedBufferLayout == i)))
                    {
                        BufferLayoutMultiselect.HandleMultiselect(_selectedBufferLayout, i);

                        ResetSelection();
                        _selectedBufferLayout = i;
                        _lastSelectedEntry = ModelEntrySelectionType.BufferLayout;

                        if (curLayout.Count > 0)
                        {
                            _subSelectedBufferLayoutMember = 0;
                        }
                    }

                    if (_selectedBufferLayout == i)
                    {
                        if (BufferLayoutMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.BufferLayoutRowContextMenu_MultiSelect(BufferLayoutMultiselect);
                        }
                        else
                        {
                            ContextMenu.BufferLayoutRowContextMenu(i, curLayout);
                        }
                    }

                    if (FocusSelection && _selectedBufferLayout == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count < 1)
        {
            ContextMenu.BufferLayoutHeaderContextMenu();
        }
    }

    private void DisplaySection_Skeletons()
    {
        if (Screen.ResourceHandler.CurrentFLVER.Skeletons == null)
            return;

        if (ImGui.CollapsingHeader("Base Skeleton"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton.Count; i++)
            {
                var curBaseSkeleton = Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[i];

                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[i].NodeIndex;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count && nodeIndex > -1)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ModelEditorSearch.IsModelEditorSearchMatch_SkeletonBone(_searchInput, curBaseSkeleton, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Bone {i} - {node}##baseSkeletonBone{i}", (BaseSkeletonMultiselect.IsMultiselected(i) || _selectedBaseSkeletonBone == i)))
                    {
                        BaseSkeletonMultiselect.HandleMultiselect(_selectedBaseSkeletonBone, i);

                        ResetSelection();
                        _selectedBaseSkeletonBone = i;
                        _lastSelectedEntry = ModelEntrySelectionType.BaseSkeleton;
                    }

                    if (_selectedBaseSkeletonBone == i)
                    {
                        if (BaseSkeletonMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.BaseSkeletonRowContextMenu_MultiSelect(BaseSkeletonMultiselect);
                        }
                        else
                        {
                            ContextMenu.BaseSkeletonRowContextMenu(i, curBaseSkeleton);
                        }
                    }

                    if (FocusSelection && _selectedBaseSkeletonBone == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton.Count < 1)
        {
            ContextMenu.BaseSkeletonHeaderContextMenu();
        }

        if (ImGui.CollapsingHeader("All Skeleton"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons.Count; i++)
            {
                var curAllSkeleton = Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[i];

                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[i].NodeIndex;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count && nodeIndex > -1)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ModelEditorSearch.IsModelEditorSearchMatch_SkeletonBone(_searchInput, curAllSkeleton, Screen.ResourceHandler.CurrentFLVER, i))
                {
                    if (ImGui.Selectable($"Bone {i} - {node}##allSkeletonBone{i}", (AllSkeletonMultiselect.IsMultiselected(i) || _selectedAllSkeletonBone == i)))
                    {
                        AllSkeletonMultiselect.HandleMultiselect(_selectedAllSkeletonBone, i);

                        ResetSelection();
                        _selectedAllSkeletonBone = i;
                        _lastSelectedEntry = ModelEntrySelectionType.AllSkeleton;
                    }

                    if (_selectedAllSkeletonBone == i)
                    {
                        if (AllSkeletonMultiselect.HasValidMultiselection())
                        {
                            ContextMenu.AllSkeletonRowContextMenu_MultiSelect(AllSkeletonMultiselect);
                        }
                        else
                        {
                            ContextMenu.AllSkeletonRowContextMenu(i, curAllSkeleton);
                        }
                    }

                    if (FocusSelection && _selectedAllSkeletonBone == i)
                    {
                        FocusSelection = false;
                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        // Only display this one if the list is empty
        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons.Count < 1)
        {
            ContextMenu.AllSkeletonHeaderContextMenu();
        }
    }

    private void DisplaySection_Collision()
    {
        var index = 0;

        if (Screen.ResourceHandler.ER_CollisionLow != null || Screen.ResourceHandler.ER_CollisionHigh != null)
        {
            if (ImGui.CollapsingHeader("Collision"))
            {
                if (Screen.ResourceHandler.ER_CollisionLow != null)
                {
                    if (ImGui.Selectable($"Low Collision {index}", _selectedLowCollision == index))
                    {
                        ResetSelection();
                        _selectedLowCollision = index;
                        _lastSelectedEntry = ModelEntrySelectionType.CollisionLow;
                    }
                }

                if (Screen.ResourceHandler.ER_CollisionHigh != null)
                {
                    if (ImGui.Selectable($"High Collision {index}", _selectedHighCollision == index))
                    {
                        ResetSelection();
                        _selectedHighCollision = index;
                        _lastSelectedEntry = ModelEntrySelectionType.CollisionHigh;
                    }
                }
            }
        }
    }
}
