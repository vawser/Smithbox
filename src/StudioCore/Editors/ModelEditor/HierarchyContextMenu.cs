using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using StudioCore.Editors.ModelEditor.Actions;
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
                    var action = new AddDummyEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Dummy at end of Dummies list.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu(int index)
        {
            var currentFlver = Screen.ResourceHandler.CurrentFLVER;

            if (ImGui.BeginPopupContextItem($"DummyRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Dummy##newAction{index}"))
                {
                    var action = new AddDummyEntry(Screen, currentFlver);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Dummy below the currently selected Dummy.");

                if (ImGui.Selectable($"Duplicate Dummy##dupeAction{index}"))
                {
                    var action = new DuplicateDummyEntry(Screen, currentFlver, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Dummy, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Dummy##removeAction{index}"))
                {
                    var action = new RemoveDummyEntry(Screen, currentFlver, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Dummy.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu_MultiSelect(HierarchyMultiselect dummyMultiselect)
        {
            var currentFlver = Screen.ResourceHandler.CurrentFLVER;

            if (ImGui.BeginPopupContextItem($"DummyRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Dummies##dupeAction"))
                {
                    var action = new DuplicateDummyEntryMulti(Screen, currentFlver, dummyMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Dummies, new dummies will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Dummies##removeAction"))
                {
                    var action = new RemoveDummyEntryMulti(Screen, currentFlver, dummyMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddMaterialEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Material at end of Materials list.");

                ImGui.EndPopup();
            }
        }

        public void MaterialRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"MaterialRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Material##newAction{index}"))
                {
                    var action = new AddMaterialEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Material at the end of the list.");

                if (ImGui.Selectable($"Duplicate Material##dupeAction{index}"))
                {
                    var action = new DuplicateMaterialEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Material, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Material##removeAction{index}"))
                {
                    var action = new RemoveMaterialEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateMaterialEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, MaterialMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Materials, new Materials will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Materials##removeAction"))
                {
                    var action = new RemoveMaterialEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, MaterialMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddGXListEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty GXList at end of GXLists list.");

                ImGui.EndPopup();
            }
        }

        public void GXListRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"GXListRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New GX List##newAction{index}"))
                {
                    var action = new AddGXListEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty GX List at the end of the list.");

                if (ImGui.Selectable($"Duplicate GX List##dupeAction{index}"))
                {
                    var action = new DuplicateGXListEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX List, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX List##removeAction{index}"))
                {
                    var action = new RemoveGXListEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateGXListEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, GXListMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX Lists, inserting them at the end of the list.");

                if (ImGui.Selectable($"Remove GX Lists##removeAction"))
                {
                    var action = new RemoveGXListEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, GXListMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddNodeEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Node at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void NodeRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"NodeRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Node##newAction{index}"))
                {
                    var action = new AddNodeEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Node at the end of list.");

                if (ImGui.Selectable($"Duplicate Node##dupeAction{index}"))
                {
                    var action = new DuplicateNodeEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Node, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Node##removeAction{index}"))
                {
                    var action = new RemoveNodeEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateNodeEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, NodeMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Nodes, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Nodes##removeAction"))
                {
                    var action = new RemoveNodeEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, NodeMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Nodes.");

                ImGui.EndPopup();
            }
        }

        // Meshes
        public void MeshHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"MeshHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Mesh"))
                {
                    var action = new AddMeshEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Mesh at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void MeshRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"MeshRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Mesh##newAction{index}"))
                {
                    var action = new AddMeshEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Mesh at the end of the list.");

                if (ImGui.Selectable($"Duplicate Mesh##dupeAction{index}"))
                {
                    var action = new DuplicateMeshEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Mesh, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Mesh##removeAction{index}"))
                {
                    var action = new RemoveMeshEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateMeshEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, MeshMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Meshes, new Meshes will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Meshes##removeAction"))
                {
                    var action = new RemoveMeshEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, MeshMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Meshes.");

                ImGui.EndPopup();
            }
        }

        // Buffer Layouts
        public void BufferLayoutHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Buffer Layout"))
                {
                    var action = new AddBufferLayoutEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddBufferLayoutEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Buffer Layout at the end of the list.");

                if (ImGui.Selectable($"Duplicate Buffer Layout##dupeAction{index}"))
                {
                    var action = new DuplicateBufferLayoutEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Buffer Layout, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Buffer Layout##removeAction{index}"))
                {
                    var action = new RemoveBufferLayoutEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateBufferLayoutEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, BufferLayoutMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Buffer Layouts, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove BufferLayouts##removeAction"))
                {
                    var action = new RemoveBufferLayoutEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, BufferLayoutMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddBaseSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddBaseSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Bone at the end of the list.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var action = new DuplicateBaseSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var action = new RemoveBaseSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateBaseSkeletonBoneEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, BaseSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var action = new RemoveBaseSkeletonBoneEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, BaseSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddAllSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddAllSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Insert new empty Bone below the currently selected Bone.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var action = new DuplicateAllSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var action = new RemoveAllSkeletonBoneEntry(Screen, Screen.ResourceHandler.CurrentFLVER, index);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new DuplicateAllSkeletonBoneEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, AllSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var action = new RemoveAllSkeletonBoneEntryMulti(Screen, Screen.ResourceHandler.CurrentFLVER, AllSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddMaterialTextureEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddMaterialTextureEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Add new Texture entry to the currently selected Material.");

                if (ImGui.Selectable($"Duplicate Texture##dupeAction{index}"))
                {
                    var selectedTexture = currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow];

                    if (selectedTexture != null)
                    {
                        var action = new DuplicateMaterialTextureEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedTexture);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Texture entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Texture##removeAction{index}"))
                {
                    var selectedTexture = currentFlver.Materials[materialIndex].Textures[Screen.ModelHierarchy._subSelectedTextureRow];

                    if (selectedTexture != null)
                    {
                        var action = new RemoveMaterialTextureEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedTexture);
                        Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddGXItemEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Add new GX Item entry to the currently selected GX List.");

                if (ImGui.Selectable($"Duplicate GX Item##dupeAction{index}"))
                {
                    var selectedGxItem = currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow];

                    if (selectedGxItem != null)
                    {
                        var action = new DuplicateGXItemTextureEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedGxItem);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected GX Item entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX Item##removeAction{index}"))
                {
                    var selectedGxItem = currentFlver.GXLists[gxList][Screen.ModelHierarchy._subSelectedGXItemRow];

                    if (selectedGxItem != null)
                    {
                        var action = new RemoveGXItemEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedGxItem);
                        Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddFaceSetEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Add new Face Set entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Face Set##dupeAction{index}"))
                {
                    var selectedFaceSet = currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow];

                    if (selectedFaceSet != null)
                    {
                        var action = new DuplicateFaceSetEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedFaceSet);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Face Set entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Face Set##removeAction{index}"))
                {
                    var selectedFaceSet = currentFlver.Meshes[selectedMesh].FaceSets[Screen.ModelHierarchy._subSelectedFaceSetRow];

                    if (selectedFaceSet != null)
                    {
                        var action = new RemoveFaceSetEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedFaceSet);
                        Screen.EditorActionManager.ExecuteAction(action);
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
                    var action = new AddVertexBufferEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Add new Vertex Buffer entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Vertex Buffer##dupeAction{index}"))
                {
                    var selectedBuffer = currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow];

                    if (selectedBuffer != null)
                    {
                        var action = new DuplicateVertexBufferEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedBuffer);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Vertex Buffer entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Vertex Buffer##removeAction{index}"))
                {
                    var selectedBuffer = currentFlver.Meshes[selectedMesh].VertexBuffers[Screen.ModelHierarchy._subSelectedVertexBufferRow];

                    if (selectedBuffer != null)
                    {
                        var action = new RemoveVertexBufferEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedBuffer);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Vertex Buffer.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutMemberHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"LayoutMemberRow_ContextMenu"))
            {
                var currentFlver = Screen.ResourceHandler.CurrentFLVER;
                var selectedBufferLayout = Screen.ModelHierarchy._selectedBufferLayout;

                if (ImGui.Selectable($"Add New Layout Member##newAction{index}"))
                {
                    var action = new AddLayoutMemberEntry(Screen, Screen.ResourceHandler.CurrentFLVER);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Add new Layout Member entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Layout Member##dupeAction{index}"))
                {
                    var selectedMember = currentFlver.BufferLayouts[selectedBufferLayout][Screen.ModelHierarchy._subSelectedBufferLayoutMember];

                    if (selectedMember != null)
                    {
                        var action = new DuplicateLayoutMemberEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedMember);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected Layout Member entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Layout Member##removeAction{index}"))
                {
                    var selectedMember = currentFlver.BufferLayouts[selectedBufferLayout][Screen.ModelHierarchy._subSelectedBufferLayoutMember];

                    if (selectedMember != null)
                    {
                        var action = new RemoveLayoutMemberEntry(Screen, Screen.ResourceHandler.CurrentFLVER, selectedMember);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                ImguiUtils.ShowHoverTooltip("Remove the currently selected Layout Member.");

                ImGui.EndPopup();
            }
        }
    }
}
