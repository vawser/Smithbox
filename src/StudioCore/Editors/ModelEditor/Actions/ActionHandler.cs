using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ActionHandler
{
    private ModelEditorScreen Screen;
    public ActionHandler(ModelEditorScreen screen)
    {
        Screen = screen;
    }

    public void CreateHandler()
    {
        if (!Screen.ResourceHandler.HasCurrentFLVER())
            return;

        var currentFlver = Screen.ResourceHandler.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Screen.ModelHierarchy._lastSelectedEntry)
        {
            case ModelEntrySelectionType.None:
            case ModelEntrySelectionType.Header:
                break;

            case ModelEntrySelectionType.Dummy:
                action = new AddDummyEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.Material:
                action = new AddMaterialEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.GXList:
                action = new AddGXListEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.Node:
                action = new AddNodeEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.Mesh:
                action = new AddMeshEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.BufferLayout:
                action = new AddBufferLayoutEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.BaseSkeleton:
                action = new AddBaseSkeletonBoneEntry(Screen, currentFlver);
                break;

            case ModelEntrySelectionType.AllSkeleton:
                action = new AddAllSkeletonBoneEntry(Screen, currentFlver);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }

    public void DuplicateHandler()
    {
        var currentFlver = Screen.ResourceHandler.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Screen.ModelHierarchy._lastSelectedEntry)
        {
            case ModelEntrySelectionType.None:
            case ModelEntrySelectionType.Header:
                break;

            case ModelEntrySelectionType.Dummy:
                if (Screen.ModelHierarchy.DummyMultiselect.HasValidMultiselection())
                    action = new DuplicateDummyEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.DummyMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedDummy != -1)
                        action = new DuplicateDummyEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedDummy);
                break;

            case ModelEntrySelectionType.Material:
                if (Screen.ModelHierarchy.MaterialMultiselect.HasValidMultiselection())
                    action = new DuplicateMaterialEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.MaterialMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedMaterial != -1)
                        action = new DuplicateMaterialEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedMaterial);
                break;

            case ModelEntrySelectionType.GXList:
                if (Screen.ModelHierarchy.GxListMultiselect.HasValidMultiselection())
                    action = new DuplicateGXListEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.GxListMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedGXList != -1)
                        action = new DuplicateGXListEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedGXList);
                break;

            case ModelEntrySelectionType.Node:
                if (Screen.ModelHierarchy.NodeMultiselect.HasValidMultiselection())
                    action = new DuplicateNodeEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.NodeMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedNode != -1)
                        action = new DuplicateNodeEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedNode);
                break;

            case ModelEntrySelectionType.Mesh:
                if (Screen.ModelHierarchy.MeshMultiselect.HasValidMultiselection())
                    action = new DuplicateMeshEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.MeshMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedMesh != -1)
                        action = new DuplicateMeshEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedMesh);
                break;

            case ModelEntrySelectionType.BufferLayout:
                if (Screen.ModelHierarchy.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new DuplicateBufferLayoutEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.BufferLayoutMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedBufferLayout != -1)
                        action = new DuplicateBufferLayoutEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedBufferLayout);
                break;

            case ModelEntrySelectionType.BaseSkeleton:
                if (Screen.ModelHierarchy.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateBaseSkeletonBoneEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.BaseSkeletonMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedBaseSkeletonBone != -1)
                        action = new DuplicateBaseSkeletonBoneEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedBaseSkeletonBone);
                break;

            case ModelEntrySelectionType.AllSkeleton:
                if (Screen.ModelHierarchy.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateAllSkeletonBoneEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.AllSkeletonMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedAllSkeletonBone != -1)
                        action = new DuplicateAllSkeletonBoneEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }

    public void DeleteHandler()
    {
        var currentFlver = Screen.ResourceHandler.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Screen.ModelHierarchy._lastSelectedEntry)
        {
            case ModelEntrySelectionType.None:
            case ModelEntrySelectionType.Header:
                break;

            case ModelEntrySelectionType.Dummy:
                if (Screen.ModelHierarchy.DummyMultiselect.HasValidMultiselection())
                    action = new RemoveDummyEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.DummyMultiselect);
                else
                    if(Screen.ModelHierarchy._selectedDummy != -1)
                        action = new RemoveDummyEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedDummy);
                break;

            case ModelEntrySelectionType.Material:
                if (Screen.ModelHierarchy.MaterialMultiselect.HasValidMultiselection())
                    action = new RemoveMaterialEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.MaterialMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedMaterial != -1)
                        action = new RemoveMaterialEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedMaterial);
                break;

            case ModelEntrySelectionType.GXList:
                if (Screen.ModelHierarchy.GxListMultiselect.HasValidMultiselection())
                    action = new RemoveGXListEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.GxListMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedGXList != -1)
                        action = new RemoveGXListEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedGXList);
                break;

            case ModelEntrySelectionType.Node:
                if (Screen.ModelHierarchy.NodeMultiselect.HasValidMultiselection())
                    action = new RemoveNodeEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.NodeMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedNode != -1)
                        action = new RemoveNodeEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedNode);
                break;

            case ModelEntrySelectionType.Mesh:
                if (Screen.ModelHierarchy.MeshMultiselect.HasValidMultiselection())
                    action = new RemoveMeshEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.MeshMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedMesh != -1)
                        action = new RemoveMeshEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedMesh);
                break;

            case ModelEntrySelectionType.BufferLayout:
                if (Screen.ModelHierarchy.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new RemoveBufferLayoutEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.BufferLayoutMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedBufferLayout != -1)
                        action = new RemoveBufferLayoutEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedBufferLayout);
                break;

            case ModelEntrySelectionType.BaseSkeleton:
                if (Screen.ModelHierarchy.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveBaseSkeletonBoneEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.BaseSkeletonMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedBaseSkeletonBone != -1)
                        action = new RemoveBaseSkeletonBoneEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedBaseSkeletonBone);
                break;

            case ModelEntrySelectionType.AllSkeleton:
                if (Screen.ModelHierarchy.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveAllSkeletonBoneEntryMulti(Screen, currentFlver, Screen.ModelHierarchy.AllSkeletonMultiselect);
                else
                    if (Screen.ModelHierarchy._selectedAllSkeletonBone != -1)
                        action = new RemoveAllSkeletonBoneEntry(Screen, currentFlver, Screen.ModelHierarchy._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Screen.EditorActionManager.ExecuteAction(action);
    }
}
