using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.AllSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BufferLayout;
using StudioCore.Editors.ModelEditor.Actions.Dummy;
using StudioCore.Editors.ModelEditor.Actions.FaceSet;
using StudioCore.Editors.ModelEditor.Actions.GxItem;
using StudioCore.Editors.ModelEditor.Actions.GxList;
using StudioCore.Editors.ModelEditor.Actions.LayoutMember;
using StudioCore.Editors.ModelEditor.Actions.Material;
using StudioCore.Editors.ModelEditor.Actions.Mesh;
using StudioCore.Editors.ModelEditor.Actions.Node;
using StudioCore.Editors.ModelEditor.Actions.Texture;
using StudioCore.Editors.ModelEditor.Actions.VertexBuffer;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor.Framework
{
    public class ModelContextMenu
    {
        private ModelEditorScreen Screen;
        private ModelSelectionManager Selection;
        private ModelResourceManager ResManager;

        public ModelContextMenu(ModelEditorScreen screen)
        {
            Screen = screen;
            Selection = screen.Selection;
            ResManager = screen.ResManager;
        }

        // Dummies
        public void DummyHeaderContextMenu()
        {
            if (ImGui.BeginPopupContextItem($"DummyHeader_ContextMenu"))
            {
                if (ImGui.Selectable("Add New Dummy"))
                {
                    var action = new AddDummy(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Dummy at end of Dummies list.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu(int index)
        {
            var currentFlver = ResManager.GetCurrentFLVER();

            if (ImGui.BeginPopupContextItem($"DummyRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Dummy##newAction{index}"))
                {
                    var action = new AddDummy(Screen, currentFlver);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Dummy below the currently selected Dummy.");

                if (ImGui.Selectable($"Duplicate Dummy##dupeAction{index}"))
                {
                    var action = new DuplicateDummy(Screen, currentFlver, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Dummy, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Dummy##removeAction{index}"))
                {
                    var action = new RemoveDummy(Screen, currentFlver, index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Dummy.");

                ImGui.EndPopup();
            }
        }

        public void DummyRowContextMenu_MultiSelect()
        {
            var currentFlver = ResManager.GetCurrentFLVER();

            if (ImGui.BeginPopupContextItem($"DummyRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Dummies##dupeAction"))
                {
                    var action = new DuplicateMultipleDummies(Screen, currentFlver, Selection.DummyMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Dummies, new dummies will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Dummies##removeAction"))
                {
                    var action = new RemoveMultipleDummies(Screen, currentFlver, Selection.DummyMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Dummies.");

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
                    var action = new AddMaterial(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Material at end of Materials list.");

                ImGui.EndPopup();
            }
        }

        public void MaterialRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"MaterialRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Material##newAction{index}"))
                {
                    var action = new AddMaterial(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Material at the end of the list.");

                if (ImGui.Selectable($"Duplicate Material##dupeAction{index}"))
                {
                    var action = new DuplicateMaterial(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Material, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Material##removeAction{index}"))
                {
                    var action = new RemoveMaterial(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Material.");

                ImGui.EndPopup();
            }
        }

        public void MaterialRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"MaterialRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Materials##dupeAction"))
                {
                    var action = new DuplicateMultipleMaterials(Screen, ResManager.GetCurrentFLVER(), Selection.MaterialMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Materials, new Materials will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Materials##removeAction"))
                {
                    var action = new RemoveMultipleMaterials(Screen, ResManager.GetCurrentFLVER(), Selection.MaterialMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Materials.");

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
                    var action = new AddGxList(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty GXList at end of GXLists list.");

                ImGui.EndPopup();
            }
        }

        public void GXListRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"GXListRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New GX List##newAction{index}"))
                {
                    var action = new AddGxList(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty GX List at the end of the list.");

                if (ImGui.Selectable($"Duplicate GX List##dupeAction{index}"))
                {
                    var action = new DuplicateGxList(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected GX List, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX List##removeAction{index}"))
                {
                    var action = new RemoveGxList(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected GX List.");

                ImGui.EndPopup();
            }
        }

        public void GXListRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"GXListRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate GX Lists##dupeAction"))
                {
                    var action = new DuplicateMultipleGxLists(Screen, ResManager.GetCurrentFLVER(), Selection.GxListMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected GX Lists, inserting them at the end of the list.");

                if (ImGui.Selectable($"Remove GX Lists##removeAction"))
                {
                    var action = new RemoveMultipleGxLists(Screen, ResManager.GetCurrentFLVER(), Selection.GxListMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected GX Lists.");

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
                    var action = new AddNode(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Node at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void NodeRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"NodeRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Node##newAction{index}"))
                {
                    var action = new AddNode(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Node at the end of list.");

                if (ImGui.Selectable($"Duplicate Node##dupeAction{index}"))
                {
                    var action = new DuplicateNode(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Node, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Node##removeAction{index}"))
                {
                    var action = new RemoveNode(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Node.");

                ImGui.EndPopup();
            }
        }

        public void NodeRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"NodeRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Nodes##dupeAction"))
                {
                    var action = new DuplicateMultipleNodes(Screen, ResManager.GetCurrentFLVER(), Selection.NodeMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Nodes, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Nodes##removeAction"))
                {
                    var action = new RemoveMultipleNodes(Screen, ResManager.GetCurrentFLVER(), Selection.NodeMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Nodes.");

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
                    var action = new AddMesh(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Mesh at end of the list.");

                ImGui.EndPopup();
            }
        }

        public void MeshRowContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"MeshRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Mesh##newAction{index}"))
                {
                    var action = new AddMesh(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Mesh at the end of the list.");

                if (ImGui.Selectable($"Duplicate Mesh##dupeAction{index}"))
                {
                    var action = new DuplicateMesh(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Mesh, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Mesh##removeAction{index}"))
                {
                    var action = new RemoveMesh(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Mesh.");

                ImGui.EndPopup();
            }
        }

        public void MeshRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"MeshRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Meshes##dupeAction"))
                {
                    var action = new DuplicateMultipleMeshes(Screen, ResManager.GetCurrentFLVER(), Selection.MeshMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Meshes, new Meshes will be inserted at the end of the list.");

                if (ImGui.Selectable($"Remove Meshes##removeAction"))
                {
                    var action = new RemoveMultipleMeshes(Screen, ResManager.GetCurrentFLVER(), Selection.MeshMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Meshes.");

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
                    var action = new AddBufferLayout(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Buffer Layout at end of Buffer Layouts list.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutRowContextMenu(int index, FLVER2.BufferLayout curBufferLayout)
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Buffer Layout##newAction{index}"))
                {
                    var action = new AddBufferLayout(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Buffer Layout at the end of the list.");

                if (ImGui.Selectable($"Duplicate Buffer Layout##dupeAction{index}"))
                {
                    var action = new DuplicateBufferLayout(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Buffer Layout, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Buffer Layout##removeAction{index}"))
                {
                    var action = new RemoveBufferLayout(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Buffer Layout.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"BufferLayoutRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Buffer Layouts##dupeAction"))
                {
                    var action = new DuplicateMultipleBufferLayouts(Screen, ResManager.GetCurrentFLVER(), Selection.BufferLayoutMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Buffer Layouts, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove BufferLayouts##removeAction"))
                {
                    var action = new RemoveMultipleBufferLayouts(Screen, ResManager.GetCurrentFLVER(), Selection.BufferLayoutMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Buffer Layouts.");

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
                    var action = new AddBaseSkeletonBone(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new emptyBone at end of Bone list.");

                ImGui.EndPopup();
            }
        }

        public void BaseSkeletonRowContextMenu(int index, FLVER2.SkeletonSet.Bone curBaseSkeleton)
        {
            if (ImGui.BeginPopupContextItem($"BaseSkeletonRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Bone##newAction{index}"))
                {
                    var action = new AddBaseSkeletonBone(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Bone at the end of the list.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var action = new DuplicateBaseSkeletonBone(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var action = new RemoveBaseSkeletonBone(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Bone.");

                ImGui.EndPopup();
            }
        }

        public void BaseSkeletonRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"BaseSkeletonRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Bones##dupeAction"))
                {
                    var action = new DuplicateMultipleBaseSkeletonBones(Screen, ResManager.GetCurrentFLVER(), Selection.BaseSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var action = new RemoveMultipleBaseSkeletonBones(Screen, ResManager.GetCurrentFLVER(), Selection.BaseSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Bones.");

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
                    var action = new AddAllSkeletonBone(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Bone at end of Bone list.");

                ImGui.EndPopup();
            }
        }

        public void AllSkeletonRowContextMenu(int index, FLVER2.SkeletonSet.Bone curAllSkeleton)
        {
            if (ImGui.BeginPopupContextItem($"AllSkeletonRow_ContextMenu"))
            {
                if (ImGui.Selectable($"Add New Bone##newAction{index}"))
                {
                    var action = new AddAllSkeletonBone(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Insert new empty Bone below the currently selected Bone.");

                if (ImGui.Selectable($"Duplicate Bone##dupeAction{index}"))
                {
                    var action = new DuplicateAllSkeletonBone(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Bone, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bone##removeAction{index}"))
                {
                    var action = new RemoveAllSkeletonBone(Screen, ResManager.GetCurrentFLVER(), index);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Bone.");

                ImGui.EndPopup();
            }
        }

        public void AllSkeletonRowContextMenu_MultiSelect()
        {
            if (ImGui.BeginPopupContextItem($"AllSkeletonRowMultiSelect_ContextMenu"))
            {
                if (ImGui.Selectable($"Duplicate Bones##dupeAction"))
                {
                    var action = new DuplicateMultipleAllSkeletonBones(Screen, ResManager.GetCurrentFLVER(), Selection.AllSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Duplicate the selected Bones, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Bones##removeAction"))
                {
                    var action = new RemoveMultipleAllSkeletonBones(Screen, ResManager.GetCurrentFLVER(), Selection.AllSkeletonMultiselect);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Remove the currently selected Bones.");

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
                    var action = new AddTexture(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new Texture entry to the currently selected Material.");

                ImGui.EndPopup();
            }
        }

        public void TextureHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"TextureRow_ContextMenu"))
            {
                var currentFlver = ResManager.GetCurrentFLVER();
                var materialIndex = Selection._selectedMaterial;

                if (ImGui.Selectable($"Add New Texture##newAction{index}"))
                {
                    var action = new AddTexture(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new Texture entry to the currently selected Material.");

                if (ImGui.Selectable($"Duplicate Texture##dupeAction{index}"))
                {
                    var selectedTexture = currentFlver.Materials[materialIndex].Textures[Selection._subSelectedTextureRow];

                    if (selectedTexture != null)
                    {
                        var action = new DuplicateTexture(Screen, ResManager.GetCurrentFLVER(), selectedTexture);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Duplicate the selected Texture entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Texture##removeAction{index}"))
                {
                    var selectedTexture = currentFlver.Materials[materialIndex].Textures[Selection._subSelectedTextureRow];

                    if (selectedTexture != null)
                    {
                        var action = new RemoveTexture(Screen, ResManager.GetCurrentFLVER(), selectedTexture);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Remove the currently selected Texture.");

                ImGui.EndPopup();
            }
        }

        public void GXItemHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"GXItemRow_ContextMenu"))
            {
                var currentFlver = ResManager.GetCurrentFLVER();
                var gxList = Selection._selectedGXList;

                if (ImGui.Selectable($"Add New GX Item##newAction{index}"))
                {
                    var action = new AddGxItem(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new GX Item entry to the currently selected GX List.");

                if (ImGui.Selectable($"Duplicate GX Item##dupeAction{index}"))
                {
                    var selectedGxItem = currentFlver.GXLists[gxList][Selection._subSelectedGXItemRow];

                    if (selectedGxItem != null)
                    {
                        var action = new DuplicateGxItem(Screen, ResManager.GetCurrentFLVER(), selectedGxItem);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Duplicate the selected GX Item entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove GX Item##removeAction{index}"))
                {
                    var selectedGxItem = currentFlver.GXLists[gxList][Selection._subSelectedGXItemRow];

                    if (selectedGxItem != null)
                    {
                        var action = new RemoveGxItem(Screen, ResManager.GetCurrentFLVER(), selectedGxItem);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }

                }
                UIHelper.Tooltip("Remove the currently selected GX Item.");

                ImGui.EndPopup();
            }
        }

        public void FaceSetHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"FaceSetRow_ContextMenu"))
            {
                var currentFlver = ResManager.GetCurrentFLVER();
                var selectedMesh = Selection._selectedMesh;

                if (ImGui.Selectable($"Add New Face Set##newAction{index}"))
                {
                    var action = new AddFaceSet(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new Face Set entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Face Set##dupeAction{index}"))
                {
                    var selectedFaceSet = currentFlver.Meshes[selectedMesh].FaceSets[Selection._subSelectedFaceSetRow];

                    if (selectedFaceSet != null)
                    {
                        var action = new DuplicateFaceSet(Screen, ResManager.GetCurrentFLVER(), selectedFaceSet);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Duplicate the selected Face Set entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Face Set##removeAction{index}"))
                {
                    var selectedFaceSet = currentFlver.Meshes[selectedMesh].FaceSets[Selection._subSelectedFaceSetRow];

                    if (selectedFaceSet != null)
                    {
                        var action = new RemoveFaceSet(Screen, ResManager.GetCurrentFLVER(), selectedFaceSet);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Remove the currently selected Face Set.");

                ImGui.EndPopup();
            }
        }

        public void VertexBufferHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"VertexBufferRow_ContextMenu"))
            {
                var currentFlver = ResManager.GetCurrentFLVER();
                var selectedMesh = Selection._selectedMesh;

                if (ImGui.Selectable($"Add New Vertex Buffer##newAction{index}"))
                {
                    var action = new AddVertexBuffer(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new Vertex Buffer entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Vertex Buffer##dupeAction{index}"))
                {
                    var selectedBuffer = currentFlver.Meshes[selectedMesh].VertexBuffers[Selection._subSelectedVertexBufferRow];

                    if (selectedBuffer != null)
                    {
                        var action = new DuplicateVertexBuffer(Screen, ResManager.GetCurrentFLVER(), selectedBuffer);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Duplicate the selected Vertex Buffer entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Vertex Buffer##removeAction{index}"))
                {
                    var selectedBuffer = currentFlver.Meshes[selectedMesh].VertexBuffers[Selection._subSelectedVertexBufferRow];

                    if (selectedBuffer != null)
                    {
                        var action = new RemoveVertexBuffer(Screen, ResManager.GetCurrentFLVER(), selectedBuffer);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Remove the currently selected Vertex Buffer.");

                ImGui.EndPopup();
            }
        }

        public void BufferLayoutMemberHeaderContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"LayoutMemberRow_ContextMenu"))
            {
                var currentFlver = ResManager.GetCurrentFLVER();
                var selectedBufferLayout = Selection._selectedBufferLayout;

                if (ImGui.Selectable($"Add New Layout Member##newAction{index}"))
                {
                    var action = new AddLayoutMember(Screen, ResManager.GetCurrentFLVER());
                    Screen.EditorActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Add new Layout Member entry to the end of the list.");

                if (ImGui.Selectable($"Duplicate Layout Member##dupeAction{index}"))
                {
                    var selectedMember = currentFlver.BufferLayouts[selectedBufferLayout][Selection._subSelectedBufferLayoutMember];

                    if (selectedMember != null)
                    {
                        var action = new DuplicateLayoutMember(Screen, ResManager.GetCurrentFLVER(), selectedMember);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Duplicate the selected Layout Member entry, inserting it at the end of the list.");

                if (ImGui.Selectable($"Remove Layout Member##removeAction{index}"))
                {
                    var selectedMember = currentFlver.BufferLayouts[selectedBufferLayout][Selection._subSelectedBufferLayoutMember];

                    if (selectedMember != null)
                    {
                        var action = new RemoveLayoutMember(Screen, ResManager.GetCurrentFLVER(), selectedMember);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }
                }
                UIHelper.Tooltip("Remove the currently selected Layout Member.");

                ImGui.EndPopup();
            }
        }

        /// <summary>
        /// Low Collision
        /// </summary>
        public void LowCollisionContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"LowCollision_ContextMenu{index}"))
            {

                ImGui.EndPopup();
            }
        }

        /// <summary>
        /// High Collision
        /// </summary>
        public void HighCollisionContextMenu(int index)
        {
            if (ImGui.BeginPopupContextItem($"HighCollision_ContextMenu{index}"))
            {

                ImGui.EndPopup();
            }
        }
    }
}
