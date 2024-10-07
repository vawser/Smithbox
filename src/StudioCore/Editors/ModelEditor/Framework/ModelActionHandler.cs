using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.AllSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BufferLayout;
using StudioCore.Editors.ModelEditor.Actions.Dummy;
using StudioCore.Editors.ModelEditor.Actions.GxList;
using StudioCore.Editors.ModelEditor.Actions.Material;
using StudioCore.Editors.ModelEditor.Actions.Mesh;
using StudioCore.Editors.ModelEditor.Actions.Node;
using StudioCore.Editors.ModelEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Framework;

public class ModelActionHandler
{
    private ModelEditorScreen Screen;
    public ModelSelectionManager Selection;

    public ModelActionHandler(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }

    public void CreateHandler()
    {
        if (!Screen.ResManager.HasCurrentFLVER())
            return;

        var currentFlver = Screen.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Screen.Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                action = new AddDummy(Screen, currentFlver);
                break;

            case GroupSelectionType.Material:
                action = new AddMaterial(Screen, currentFlver);
                break;

            case GroupSelectionType.GXList:
                action = new AddGxList(Screen, currentFlver);
                break;

            case GroupSelectionType.Node:
                action = new AddNode(Screen, currentFlver);
                break;

            case GroupSelectionType.Mesh:
                action = new AddMesh(Screen, currentFlver);
                break;

            case GroupSelectionType.BufferLayout:
                action = new AddBufferLayout(Screen, currentFlver);
                break;

            case GroupSelectionType.BaseSkeleton:
                action = new AddBaseSkeletonBone(Screen, currentFlver);
                break;

            case GroupSelectionType.AllSkeleton:
                action = new AddAllSkeletonBone(Screen, currentFlver);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }

    public void DuplicateHandler()
    {
        var currentFlver = Screen.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                if (Selection.DummyMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleDummies(Screen, currentFlver, Selection.DummyMultiselect);
                else
                    if (Selection._selectedDummy != -1)
                    action = new DuplicateDummy(Screen, currentFlver, Selection._selectedDummy);
                break;

            case GroupSelectionType.Material:
                if (Selection.MaterialMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleMaterials(Screen, currentFlver, Selection.MaterialMultiselect);
                else
                    if (Selection._selectedMaterial != -1)
                    action = new DuplicateMaterial(Screen, currentFlver, Selection._selectedMaterial);
                break;

            case GroupSelectionType.GXList:
                if (Selection.GxListMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleGxLists(Screen, currentFlver, Selection.GxListMultiselect);
                else
                    if (Selection._selectedGXList != -1)
                    action = new DuplicateGxList(Screen, currentFlver, Selection._selectedGXList);
                break;

            case GroupSelectionType.Node:
                if (Selection.NodeMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleNodes(Screen, currentFlver, Selection.NodeMultiselect);
                else
                    if (Selection._selectedNode != -1)
                    action = new DuplicateNode(Screen, currentFlver, Selection._selectedNode);
                break;

            case GroupSelectionType.Mesh:
                if (Selection.MeshMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleMeshes(Screen, currentFlver, Selection.MeshMultiselect);
                else
                    if (Selection._selectedMesh != -1)
                    action = new DuplicateMesh(Screen, currentFlver, Selection._selectedMesh);
                break;

            case GroupSelectionType.BufferLayout:
                if (Selection.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleBufferLayouts(Screen, currentFlver, Selection.BufferLayoutMultiselect);
                else
                    if (Selection._selectedBufferLayout != -1)
                    action = new DuplicateBufferLayout(Screen, currentFlver, Selection._selectedBufferLayout);
                break;

            case GroupSelectionType.BaseSkeleton:
                if (Selection.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleBaseSkeletonBones(Screen, currentFlver, Selection.BaseSkeletonMultiselect);
                else
                    if (Selection._selectedBaseSkeletonBone != -1)
                    action = new DuplicateBaseSkeletonBone(Screen, currentFlver, Selection._selectedBaseSkeletonBone);
                break;

            case GroupSelectionType.AllSkeleton:
                if (Selection.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleAllSkeletonBones(Screen, currentFlver, Selection.AllSkeletonMultiselect);
                else
                    if (Selection._selectedAllSkeletonBone != -1)
                    action = new DuplicateAllSkeletonBone(Screen, currentFlver, Selection._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }

    public void DeleteHandler()
    {
        var currentFlver = Screen.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                if (Selection.DummyMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleDummies(Screen, currentFlver, Selection.DummyMultiselect);
                else
                    if (Selection._selectedDummy != -1)
                    action = new RemoveDummy(Screen, currentFlver, Selection._selectedDummy);
                break;

            case GroupSelectionType.Material:
                if (Selection.MaterialMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleMaterials(Screen, currentFlver, Selection.MaterialMultiselect);
                else
                    if (Selection._selectedMaterial != -1)
                    action = new RemoveMaterial(Screen, currentFlver, Selection._selectedMaterial);
                break;

            case GroupSelectionType.GXList:
                if (Selection.GxListMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleGxLists(Screen, currentFlver, Selection.GxListMultiselect);
                else
                    if (Selection._selectedGXList != -1)
                    action = new RemoveGxList(Screen, currentFlver, Selection._selectedGXList);
                break;

            case GroupSelectionType.Node:
                if (Selection.NodeMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleNodes(Screen, currentFlver, Selection.NodeMultiselect);
                else
                    if (Selection._selectedNode != -1)
                    action = new RemoveNode(Screen, currentFlver, Selection._selectedNode);
                break;

            case GroupSelectionType.Mesh:
                if (Selection.MeshMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleMeshes(Screen, currentFlver, Selection.MeshMultiselect);
                else
                    if (Selection._selectedMesh != -1)
                    action = new RemoveMesh(Screen, currentFlver, Selection._selectedMesh);
                break;

            case GroupSelectionType.BufferLayout:
                if (Selection.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleBufferLayouts(Screen, currentFlver, Selection.BufferLayoutMultiselect);
                else
                    if (Selection._selectedBufferLayout != -1)
                    action = new RemoveBufferLayout(Screen, currentFlver, Selection._selectedBufferLayout);
                break;

            case GroupSelectionType.BaseSkeleton:
                if (Selection.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleBaseSkeletonBones(Screen, currentFlver, Selection.BaseSkeletonMultiselect);
                else
                    if (Selection._selectedBaseSkeletonBone != -1)
                    action = new RemoveBaseSkeletonBone(Screen, currentFlver, Selection._selectedBaseSkeletonBone);
                break;

            case GroupSelectionType.AllSkeleton:
                if (Selection.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleAllSkeletonBones(Screen, currentFlver, Selection.AllSkeletonMultiselect);
                else
                    if (Selection._selectedAllSkeletonBone != -1)
                    action = new RemoveAllSkeletonBone(Screen, currentFlver, Selection._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }
}
