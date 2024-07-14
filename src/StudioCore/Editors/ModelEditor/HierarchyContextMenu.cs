using ImGuiNET;
using StudioCore.Formats;
using StudioCore.Formats.PureFLVER;
using StudioCore.Formats.PureFLVER.FLVER2;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class HierarchyContextMenu
    {
        private ModelEditorScreen Screen;

        public HierarchyContextMenu(ModelEditorScreen editor)
        {
            Screen = editor;
        }

        // Dummies
        public void DummyHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"DummyHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Dummy"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyDummy = new FLVER.Dummy();
                    currentFlver.Dummies.Add(emptyDummy);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Dummy at end of Dummies list.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu(int index, FLVER.Dummy curDummy)
        {
            if (ImGui.BeginPopupContextItem($"DummyRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Dummy##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyDummy = new FLVER.Dummy();
                    currentFlver.Dummies.Add(emptyDummy);
                    Screen.ModelHierarchy._selectedDummy = currentFlver.Dummies.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Dummy below the currently selected Dummy.");

                if (ImGui.Selectable($"Duplicate Dummy##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeDummy = curDummy.Clone();
                    currentFlver.Dummies.Add(dupeDummy);
                    Screen.ModelHierarchy._selectedDummy = currentFlver.Dummies.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Dummy, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Dummy##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedDummy = -1;
                    currentFlver.Dummies.Remove(curDummy);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Dummy.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu_MultiSelect(HierarchyMultiselect dummyMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"DummyRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Dummies##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedDummy = -1;

                    foreach (var idx in dummyMultiselect.StoredIndices)
                    {
                        var newDummy = Screen.ResourceHandler.CurrentFLVER.Dummies[idx].Clone();
                        currentFlver.Dummies.Add(newDummy);
                    }

                    dummyMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Dummies, new dummies will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Dummies##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedDummy = -1;

                    List<FLVER.Dummy> dummiesToRemove = new List<FLVER.Dummy>();

                    foreach (var idx in dummyMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Dummies[idx] != null)
                            dummiesToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Dummies[idx]);
                    }

                    foreach(var entry in dummiesToRemove)
                    {
                        currentFlver.Dummies.Remove(entry);
                    }

                    dummyMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Dummies.");

                ImGui.EndPopup();
            }
        }

        // Materials
        public void MaterialHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"MaterialHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Material"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyMaterial = new FLVER2.Material();
                    currentFlver.Materials.Add(emptyMaterial);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Material at end of Materials list.");

                ImGui.EndPopup();
            }
        }

        public void MaterialRowContextMenu(int index, FLVER2.Material curMaterial)
        {
            if (ImGui.BeginPopupContextItem($"MaterialRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Material##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyMaterial = new FLVER2.Material();
                    var emptyMaterialTexture = new FLVER2.Texture();
                    emptyMaterial.Textures.Add(emptyMaterialTexture);
                    currentFlver.Materials.Add(emptyMaterial);
                    Screen.ModelHierarchy._selectedMaterial = currentFlver.Materials.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Material at the end of the list.");

                if (ImGui.Selectable($"Duplicate Material##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeMaterial = curMaterial.Clone();
                    currentFlver.Materials.Add(dupeMaterial);
                    Screen.ModelHierarchy._selectedMaterial = currentFlver.Materials.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Material, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Material##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMaterial = -1;
                    currentFlver.Materials.Remove(curMaterial);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Material.");

                ImGui.EndPopup();
            }
        }

        public void MaterialRowContextMenu_MultiSelect(HierarchyMultiselect MaterialMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"MaterialRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Materials##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMaterial = -1;

                    foreach (var idx in MaterialMultiselect.StoredIndices)
                    {
                        var newMaterial = Screen.ResourceHandler.CurrentFLVER.Materials[idx].Clone();
                        currentFlver.Materials.Add(newMaterial);
                    }

                    MaterialMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Materials, new Materials will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Materials##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMaterial = -1;

                    List<FLVER2.Material> MaterialsToRemove = new List<FLVER2.Material>();

                    foreach (var idx in MaterialMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Materials[idx] != null)
                            MaterialsToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Materials[idx]);
                    }

                    foreach (var entry in MaterialsToRemove)
                    {
                        currentFlver.Materials.Remove(entry);
                    }

                    MaterialMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Materials.");

                ImGui.EndPopup();
            }
        }

        // GX Lists
        public void GXListHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"GXListHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New GXList"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyGXList = new FLVER2.GXList();
                    currentFlver.GXLists.Add(emptyGXList);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty GXList at end of GXLists list.");

                ImGui.EndPopup();
            }
        }

        public void GXListRowContextMenu(int index, FLVER2.GXList curGXList)
        {
            if (ImGui.BeginPopupContextItem($"GXListRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New GX List##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyGXList = new FLVER2.GXList();
                    var emptyGXItem = new FLVER2.GXItem();
                    emptyGXList.Add(emptyGXItem);
                    currentFlver.GXLists.Add(emptyGXList);
                    Screen.ModelHierarchy._selectedGXList = currentFlver.GXLists.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty GX List at the end of the list.");

                if (ImGui.Selectable($"Duplicate GX List##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeGXList = curGXList.Clone();
                    currentFlver.GXLists.Add(dupeGXList);
                    Screen.ModelHierarchy._selectedGXList = currentFlver.GXLists.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX List, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX List##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedGXList = -1;
                    currentFlver.GXLists.Remove(curGXList);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected GX List.");

                ImGui.EndPopup();
            }
        }

        public void GXListRowContextMenu_MultiSelect(HierarchyMultiselect GXListMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"GXListRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate GX Lists##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedGXList = -1;

                    foreach (var idx in GXListMultiselect.StoredIndices)
                    {
                        var newGXList = Screen.ResourceHandler.CurrentFLVER.GXLists[idx].Clone();
                        currentFlver.GXLists.Add(newGXList);
                    }

                    GXListMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX Lists, inserting them at the end of the list.");

                if (ImGui.Selectable($"Remove GX Lists##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedGXList = -1;

                    List<FLVER2.GXList> GXListsToRemove = new List<FLVER2.GXList>();

                    foreach (var idx in GXListMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.GXLists[idx] != null)
                            GXListsToRemove.Add(Screen.ResourceHandler.CurrentFLVER.GXLists[idx]);
                    }

                    foreach (var entry in GXListsToRemove)
                    {
                        currentFlver.GXLists.Remove(entry);
                    }

                    GXListMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected GX Lists.");

                ImGui.EndPopup();
            }
        }

        // Nodes
        public void NodeHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"NodeHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Node"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyNode = new FLVER.Node();
                    currentFlver.Nodes.Add(emptyNode);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Node at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void NodeRowContextMenu(int index, FLVER.Node curNode)
        {
            if (ImGui.BeginPopupContextItem($"NodeRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Node##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyNode = new FLVER.Node();
                    currentFlver.Nodes.Add(emptyNode);
                    Screen.ModelHierarchy._selectedNode = currentFlver.Nodes.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Node at the end of list.");

                if (ImGui.Selectable($"Duplicate Node##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeNode = curNode.Clone();
                    currentFlver.Nodes.Add(dupeNode);
                    Screen.ModelHierarchy._selectedNode = currentFlver.Nodes.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Node, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Node##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedNode = -1;
                    currentFlver.Nodes.Remove(curNode);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Node.");

                ImGui.EndPopup();
            }
        }

        public void NodeRowContextMenu_MultiSelect(HierarchyMultiselect NodeMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"NodeRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Nodes##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedNode = -1;

                    foreach (var idx in NodeMultiselect.StoredIndices)
                    {
                        var newNode = Screen.ResourceHandler.CurrentFLVER.Nodes[idx].Clone();
                        currentFlver.Nodes.Add(newNode);
                    }

                    NodeMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Nodes, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Nodes##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedNode = -1;

                    List<FLVER.Node> NodesToRemove = new List<FLVER.Node>();

                    foreach (var idx in NodeMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Nodes[idx] != null)
                            NodesToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Nodes[idx]);
                    }

                    foreach (var entry in NodesToRemove)
                    {
                        currentFlver.Nodes.Remove(entry);
                    }

                    NodeMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Nodes.");

                ImGui.EndPopup();
            }
        }

        // Meshes
        // Meshes
        public void MeshHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"MeshHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Mesh"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyMesh = new FLVER2.Mesh();
                    var emptyBoundingBoxes = new FLVER2.Mesh.BoundingBoxes();
                    var emptyFaceset = new FLVER2.FaceSet();
                    var emptyVertexBuffer = new FLVER2.VertexBuffer(0);
                    emptyMesh.BoundingBox = emptyBoundingBoxes;
                    emptyMesh.FaceSets.Add(emptyFaceset);
                    emptyMesh.VertexBuffers.Add(emptyVertexBuffer);
                    currentFlver.Meshes.Add(emptyMesh);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Mesh at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void MeshRowContextMenu(int index, FLVER2.Mesh curMesh)
        {
            if (ImGui.BeginPopupContextItem($"MeshRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Mesh##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyMesh = new FLVER2.Mesh();
                    var emptyBoundingBoxes = new FLVER2.Mesh.BoundingBoxes();
                    var emptyFaceset = new FLVER2.FaceSet();
                    var emptyVertexBuffer = new FLVER2.VertexBuffer(0);
                    emptyMesh.BoundingBox = emptyBoundingBoxes;
                    emptyMesh.FaceSets.Add(emptyFaceset);
                    emptyMesh.VertexBuffers.Add(emptyVertexBuffer);

                    currentFlver.Meshes.Add(emptyMesh);
                    Screen.ModelHierarchy._selectedMesh = currentFlver.Meshes.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Mesh at the end of the list.");

                if (ImGui.Selectable($"Duplicate Mesh##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeMesh = curMesh.Clone();
                    currentFlver.Meshes.Add(dupeMesh);
                    Screen.ModelHierarchy._selectedMesh = currentFlver.Meshes.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Mesh, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Mesh##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMesh = -1;
                    currentFlver.Meshes.Remove(curMesh);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Mesh.");

                ImGui.EndPopup();
            }
        }

        public void MeshRowContextMenu_MultiSelect(HierarchyMultiselect MeshMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"MeshRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Meshes##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMesh = -1;

                    foreach (var idx in MeshMultiselect.StoredIndices)
                    {
                        var newMesh = Screen.ResourceHandler.CurrentFLVER.Meshes[idx].Clone();
                        currentFlver.Meshes.Add(newMesh);
                    }

                    MeshMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Meshes, new Meshes will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Meshes##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedMesh = -1;

                    List<FLVER2.Mesh> MeshesToRemove = new List<FLVER2.Mesh>();

                    foreach (var idx in MeshMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Meshes[idx] != null)
                            MeshesToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Meshes[idx]);
                    }

                    foreach (var entry in MeshesToRemove)
                    {
                        currentFlver.Meshes.Remove(entry);
                    }

                    MeshMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Meshes.");

                ImGui.EndPopup();
            }
        }

        // Buffer Layouts
        // BufferLayouts
        public void BufferLayoutHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Buffer Layout"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyBufferLayout = new FLVER2.BufferLayout();
                    var emptyLayoutMember = new FLVER.LayoutMember();
                    emptyBufferLayout.Add(emptyLayoutMember);
                    currentFlver.BufferLayouts.Add(emptyBufferLayout);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Buffer Layout at end of Buffer Layouts list.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutRowContextMenu(int index, FLVER2.BufferLayout curBufferLayout)
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Buffer Layout##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyBufferLayout = new FLVER2.BufferLayout();
                    var emptyLayoutMember = new FLVER.LayoutMember();
                    emptyBufferLayout.Add(emptyLayoutMember);
                    currentFlver.BufferLayouts.Add(emptyBufferLayout);
                    Screen.ModelHierarchy._selectedBufferLayout = currentFlver.BufferLayouts.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Buffer Layout at the end of the list.");

                if (ImGui.Selectable($"Duplicate Buffer Layout##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeBufferLayout = curBufferLayout.Clone();
                    currentFlver.BufferLayouts.Add(dupeBufferLayout);
                    Screen.ModelHierarchy._selectedBufferLayout = currentFlver.BufferLayouts.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Buffer Layout, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Buffer Layout##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBufferLayout = -1;
                    currentFlver.BufferLayouts.Remove(curBufferLayout);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Buffer Layout.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutRowContextMenu_MultiSelect(HierarchyMultiselect BufferLayoutMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Buffer Layouts##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBufferLayout = -1;

                    foreach (var idx in BufferLayoutMultiselect.StoredIndices)
                    {
                        var newBufferLayout = Screen.ResourceHandler.CurrentFLVER.BufferLayouts[idx].Clone();
                        currentFlver.BufferLayouts.Add(newBufferLayout);
                    }

                    BufferLayoutMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Buffer Layouts, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove BufferLayouts##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBufferLayout = -1;

                    List<FLVER2.BufferLayout> BufferLayoutsToRemove = new List<FLVER2.BufferLayout>();

                    foreach (var idx in BufferLayoutMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.BufferLayouts[idx] != null)
                            BufferLayoutsToRemove.Add(Screen.ResourceHandler.CurrentFLVER.BufferLayouts[idx]);
                    }

                    foreach (var entry in BufferLayoutsToRemove)
                    {
                        currentFlver.BufferLayouts.Remove(entry);
                    }

                    BufferLayoutMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Buffer Layouts.");

                ImGui.EndPopup();
            }
        }

        // Base Skeleton Bones
        public void BaseSkeletonHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"BaseSkeletonHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Bone"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyBaseSkeleton = new FLVER2.SkeletonSet.Bone(-1);
                    currentFlver.Skeletons.BaseSkeleton.Add(emptyBaseSkeleton);
                }
                ImguiUtils.ShowHoverTooltip("Insert new emptyBone at end of Bone list.");

                ImGui.EndPopup();
            }
        }

        public void BaseSkeletonRowContextMenu(int index, FLVER2.SkeletonSet.Bone curBaseSkeleton)
        {
            if (ImGui.BeginPopupContextItem($"BaseSkeletonRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Bone##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyBaseSkeleton = new FLVER2.SkeletonSet.Bone(-1);
                    //currentFlver.Skeletons.BaseSkeleton.Insert(index + 1, emptyBaseSkeleton);
                    currentFlver.Skeletons.BaseSkeleton.Add(emptyBaseSkeleton);
                    Screen.ModelHierarchy._selectedBaseSkeleton = currentFlver.Skeletons.BaseSkeleton.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Bone at the end of the list.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeBaseSkeleton = curBaseSkeleton.Clone();
                    //currentFlver.Skeletons.BaseSkeleton.Insert(index + 1, dupeBaseSkeleton);
                    currentFlver.Skeletons.BaseSkeleton.Add(dupeBaseSkeleton);
                    Screen.ModelHierarchy._selectedBaseSkeleton = currentFlver.Skeletons.BaseSkeleton.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBaseSkeleton = -1;
                    currentFlver.Skeletons.BaseSkeleton.Remove(curBaseSkeleton);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Bone.");

                ImGui.EndPopup();
            }
        }

        public void BaseSkeletonRowContextMenu_MultiSelect(HierarchyMultiselect BaseSkeletonMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"BaseSkeletonRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Bones##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBaseSkeleton = -1;

                    foreach (var idx in BaseSkeletonMultiselect.StoredIndices)
                    {
                        var newBaseSkeleton = Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[idx].Clone();
                        currentFlver.Skeletons.BaseSkeleton.Add(newBaseSkeleton);
                    }

                    BaseSkeletonMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedBaseSkeleton = -1;

                    List<FLVER2.SkeletonSet.Bone> BonesToRemove = new List<FLVER2.SkeletonSet.Bone>();

                    foreach (var idx in BaseSkeletonMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[idx] != null)
                            BonesToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[idx]);
                    }

                    foreach (var entry in BonesToRemove)
                    {
                        currentFlver.Skeletons.BaseSkeleton.Remove(entry);
                    }

                    BaseSkeletonMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Bones.");

                ImGui.EndPopup();
            }
        }

        // All Skeletons
        public void AllSkeletonHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"AllSkeletonHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Bone"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyAllSkeleton = new FLVER2.SkeletonSet.Bone(-1);
                    currentFlver.Skeletons.AllSkeletons.Add(emptyAllSkeleton);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Bone at end of Bone list.");

                ImGui.EndPopup();
            }
        }

        public void AllSkeletonRowContextMenu(int index, FLVER2.SkeletonSet.Bone curAllSkeleton)
        {
            if (ImGui.BeginPopupContextItem($"AllSkeletonRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Bone##newAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var emptyAllSkeleton = new FLVER2.SkeletonSet.Bone(-1);
                    //currentFlver.Skeletons.AllSkeletons.Insert(index + 1, emptyAllSkeleton);
                    currentFlver.Skeletons.AllSkeletons.Add(emptyAllSkeleton);
                    Screen.ModelHierarchy._selectedAllSkeleton = currentFlver.Skeletons.AllSkeletons.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Bone below the currently selected Bone.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    var dupeAllSkeleton = curAllSkeleton.Clone();
                    //currentFlver.Skeletons.AllSkeletons.Insert(index + 1, dupeAllSkeleton);
                    currentFlver.Skeletons.AllSkeletons.Add(dupeAllSkeleton);
                    Screen.ModelHierarchy._selectedAllSkeleton = currentFlver.Skeletons.AllSkeletons.Count - 1;
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedAllSkeleton = -1;
                    currentFlver.Skeletons.AllSkeletons.Remove(curAllSkeleton);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Bone.");

                ImGui.EndPopup();
            }
        }

        public void AllSkeletonRowContextMenu_MultiSelect(HierarchyMultiselect AllSkeletonMultiselect)
        {
            if (ImGui.BeginPopupContextItem($"AllSkeletonRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Bones##dupeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedAllSkeleton = -1;

                    foreach (var idx in AllSkeletonMultiselect.StoredIndices)
                    {
                        var newAllSkeleton = Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[idx].Clone();
                        currentFlver.Skeletons.AllSkeletons.Add(newAllSkeleton);
                    }

                    AllSkeletonMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var currentFlver = Screen.ResourceHandler.CurrentFLVER;

                    Screen.ModelHierarchy._selectedAllSkeleton = -1;

                    List<FLVER2.SkeletonSet.Bone> BonesToRemove = new List<FLVER2.SkeletonSet.Bone>();

                    foreach (var idx in AllSkeletonMultiselect.StoredIndices)
                    {
                        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[idx] != null)
                            BonesToRemove.Add(Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[idx]);
                    }

                    foreach (var entry in BonesToRemove)
                    {
                        currentFlver.Skeletons.AllSkeletons.Remove(entry);
                    }

                    AllSkeletonMultiselect.StoredIndices = new List<int>();
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Bones.");

                ImGui.EndPopup();
            }
        }

        // Texture
        public void TextureHeaderContextMenu(FLVER2.Material curMaterial)
        {
            if (ImGui.BeginPopupContextItem($"TextureHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Texture"))
                {
                    var emptyMaterialTexture = new FLVER2.Texture();
                    curMaterial.Textures.Add(emptyMaterialTexture);
                }
                ImguiUtils.ShowHoverTooltip("Add new Texture entry to the currently selected Material.");

                ImGui.EndPopup();
            }
        }

        public void TextureHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"TextureRow_ContextMenu"))
            {
                var currentFlver = Screen.ResourceHandler.CurrentFLVER;
                var materialIndex = Screen.ModelHierarchy._selectedMaterial;

                if (ImGui.Selectable($"Add New Texture##newAction{index}"))
                {
                    var emptyMaterialTexture = new FLVER2.Texture();
                    if (currentFlver.Materials[materialIndex] != null)
                    {
                        currentFlver.Materials[materialIndex].Textures.Add(emptyMaterialTexture);
                        Screen.ModelHierarchy._subSelectedTextureRow = currentFlver.Materials[materialIndex].Textures.Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Add new Texture entry to the currently selected Material.");

                if (ImGui.Selectable($"Duplicate Texture##dupeAction{index}"))
                {
                    if (currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow] != null)
                    {
                        var dupeTexture = currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow].Clone();
                        currentFlver.Materials[materialIndex].Textures.Add(dupeTexture);
                        Screen.ModelHierarchy._subSelectedTextureRow = currentFlver.Materials[materialIndex].Textures.Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Texture entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Texture##removeAction{index}"))
                {
                    if (currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow] != null)
                    {
                        var removeTexture = currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow];

                        currentFlver.Materials[materialIndex].Textures.Remove(removeTexture);

                        if (currentFlver.Materials[materialIndex].Textures.Count > 0)
                            Screen.ModelHierarchy._subSelectedTextureRow = 0;
                        else
                            Screen.ModelHierarchy._subSelectedTextureRow = -1;
                    }

                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Texture.");

                ImGui.EndPopup();
            }
        }

        public void GXItemHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"GXItemRow_ContextMenu"))
            {
                var currentFlver = Screen.ResourceHandler.CurrentFLVER;
                var gxList = Screen.ModelHierarchy._selectedGXList;

                if (ImGui.Selectable($"Add New GX Item##newAction{index}"))
                {
                    var emptyGXItem = new FLVER2.GXItem();
                    if (currentFlver.GXLists[gxList] != null)
                    {
                        currentFlver.GXLists[gxList].Add(emptyGXItem);
                        Screen.ModelHierarchy._subSelectedGXItemRow = currentFlver.GXLists[gxList].Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Add new GX Item entry to the currently selected GX List.");

                if (ImGui.Selectable($"Duplicate GX Item##dupeAction{index}"))
                {
                    if (currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow] != null)
                    {
                        var dupeGxItem = currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow].Clone();
                        currentFlver.GXLists[gxList].Add(dupeGxItem);
                        Screen.ModelHierarchy._subSelectedGXItemRow = currentFlver.GXLists[gxList].Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX Item entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX Item##removeAction{index}"))
                {
                    if (currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow] != null)
                    {
                        var remove = currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow];

                        currentFlver.GXLists[gxList].Remove(remove);

                        if (currentFlver.GXLists[gxList].Count > 0)
                            Screen.ModelHierarchy._subSelectedGXItemRow = 0;
                        else
                            Screen.ModelHierarchy._subSelectedGXItemRow = -1;
                    }

                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected GX Item.");

                ImGui.EndPopup();
            }
        }

        public void FaceSetHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"FaceSetRow_ContextMenu"))
            {
                var currentFlver = Screen.ResourceHandler.CurrentFLVER;
                var selectedMesh = Screen.ModelHierarchy._selectedMesh;

                if (ImGui.Selectable($"Add New Face Set##newAction{index}"))
                {
                    var emptyFaceSet = new FLVER2.FaceSet();
                    if (currentFlver.Meshes[selectedMesh] != null)
                    {
                        currentFlver.Meshes[selectedMesh].FaceSets.Add(emptyFaceSet);
                        Screen.ModelHierarchy._subSelectedFaceSetRow = currentFlver.Meshes[selectedMesh].FaceSets.Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Add new Face Set entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Face Set##dupeAction{index}"))
                {
                    if (currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow] != null)
                    {
                        var dupeFaceSet = currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow].Clone();
                        if (currentFlver.Meshes[selectedMesh] != null)
                        {
                            currentFlver.Meshes[selectedMesh].FaceSets.Add(dupeFaceSet);
                            Screen.ModelHierarchy._subSelectedFaceSetRow = currentFlver.Meshes[selectedMesh].FaceSets.Count - 1;
                        }
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Face Set entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Face Set##removeAction{index}"))
                {
                    if (currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow] != null)
                    {
                        var remove = currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow];

                        currentFlver.Meshes[selectedMesh].FaceSets.Remove(remove);

                        if (currentFlver.Meshes[selectedMesh].FaceSets.Count > 0)
                            Screen.ModelHierarchy._subSelectedFaceSetRow = 0;
                        else
                            Screen.ModelHierarchy._subSelectedFaceSetRow = -1;
                    }

                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Face Set.");

                ImGui.EndPopup();
            }
        }

        public void VertexBufferHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"VertexBufferRow_ContextMenu"))
            {
                var currentFlver = Screen.ResourceHandler.CurrentFLVER;
                var selectedMesh = Screen.ModelHierarchy._selectedMesh;

                if (ImGui.Selectable($"Add New Vertex Buffer##newAction{index}"))
                {
                    var emptyVertexBuffer = new FLVER2.VertexBuffer(0);
                    if (currentFlver.Meshes[selectedMesh] != null)
                    {
                        currentFlver.Meshes[selectedMesh].VertexBuffers.Add(emptyVertexBuffer);
                        Screen.ModelHierarchy._subSelectedVertexBufferRow = currentFlver.Meshes[selectedMesh].VertexBuffers.Count - 1;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Add new Vertex Buffer entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Vertex Buffer##dupeAction{index}"))
                {
                    if (currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow] != null)
                    {
                        var dupeVertexBuffer = currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow].Clone();
                        if (currentFlver.Meshes[selectedMesh] != null)
                        {
                            currentFlver.Meshes[selectedMesh].VertexBuffers.Add(dupeVertexBuffer);
                            Screen.ModelHierarchy._subSelectedVertexBufferRow = currentFlver.Meshes[selectedMesh].VertexBuffers.Count - 1;
                        }
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Vertex Buffer entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Vertex Buffer##removeAction{index}"))
                {
                    if (currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow] != null)
                    {
                        var remove = currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow];

                        currentFlver.Meshes[selectedMesh].VertexBuffers.Remove(remove);

                        if (currentFlver.Meshes[selectedMesh].VertexBuffers.Count > 0)
                            Screen.ModelHierarchy._subSelectedVertexBufferRow = 0;
                        else
                            Screen.ModelHierarchy._subSelectedVertexBufferRow = -1;
                    }

                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Vertex Buffer.");

                ImGui.EndPopup();
            }
        }
    }
}
