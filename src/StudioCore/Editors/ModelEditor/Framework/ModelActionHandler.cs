using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Actions.AllSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;
using StudioCore.Editors.ModelEditor.Actions.BufferLayout;
using StudioCore.Editors.ModelEditor.Actions.Dummy;
using StudioCore.Editors.ModelEditor.Actions.GxList;
using StudioCore.Editors.ModelEditor.Actions.Material;
using StudioCore.Editors.ModelEditor.Actions.Mesh;
using StudioCore.Editors.ModelEditor.Actions.Node;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Utils;
using System.Numerics;

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

    /// <summary>
    /// Reverse Mesh Normals
    /// </summary>
    public void ReverseMeshNormals()
    {
        var model = Screen.ResManager.GetCurrentFLVER();
        var selectedMesh = Screen.Selection._selectedMesh;

        if (model == null)
        {
            TaskLogs.AddLog("No FLVER loaded in Model Editor.", LogLevel.Error);
            return;
        }

        if (selectedMesh == -1)
        {
            TaskLogs.AddLog("No mesh selected in Model Editor", LogLevel.Error);
            return;
        }

        foreach (FLVER.Vertex v in model.Meshes[selectedMesh].Vertices)
        {
            v.Normal = new Vector3(
                -v.Normal.X,
                -v.Normal.Y,
                -v.Normal.Z);

            for (int j = 0; j < v.Tangents.Count; ++j)
            {
                v.Tangents[j] = new Vector4(
                    -v.Tangents[j].X,
                    -v.Tangents[j].Y,
                    -v.Tangents[j].Z,
                    v.Tangents[j].W);
            }
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel();

        TaskLogs.AddLog("Mesh normals have been reversed.");
    }

    /// <summary>
    /// Reverse Mesh Faceset
    /// </summary>
    public void ReverseMeshFaceSet()
    {
        var model = Screen.ResManager.GetCurrentFLVER();
        var selectedMesh = Screen.Selection._selectedMesh;
        var selectedFaceSet = Screen.Selection._subSelectedFaceSetRow;

        if (model == null)
        {
            TaskLogs.AddLog("No FLVER loaded in Model Editor.", LogLevel.Error);
            return;
        }

        if (selectedMesh == -1)
        {
            TaskLogs.AddLog("No mesh selected in Model Editor", LogLevel.Error);
            return;
        }

        if (selectedFaceSet == -1)
        {
            TaskLogs.AddLog("No face set selected in Model Editor", LogLevel.Error);
            return;
        }

        var faceSet = model.Meshes[selectedMesh].FaceSets[selectedFaceSet];

        for (int j = 0; j < faceSet.Indices.Count; j += 3)
        {
            if (j > faceSet.Indices.Count - 2)
                continue;

            (faceSet.Indices[j + 1], faceSet.Indices[j + 2]) = (faceSet.Indices[j + 2], faceSet.Indices[j + 1]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel();

        TaskLogs.AddLog("Face Set reversed.");
    }

    /// <summary>
    /// Solve Bounding Boxes
    /// </summary>
    public void SolveBoundingBoxes()
    {
        var model = Screen.ResManager.GetCurrentFLVER();

        if(model == null)
        {
            TaskLogs.AddLog("No FLVER loaded in Model Editor.", LogLevel.Error);
            return;
        }

        model.Header.BoundingBoxMin = new Vector3(float.PositiveInfinity);
        model.Header.BoundingBoxMax = new Vector3(float.NegativeInfinity);
        foreach (FLVER.Node node in model.Nodes)
        {
            node.BoundingBoxMin = new Vector3(float.PositiveInfinity);
            node.BoundingBoxMax = new Vector3(float.NegativeInfinity);
            node.Flags = 0;
        }
        foreach (FLVER2.Mesh mesh in model.Meshes)
        {
            mesh.BoundingBox = new FLVER2.Mesh.BoundingBoxes
            {
                Min = new Vector3(float.PositiveInfinity),
                Max = new Vector3(float.NegativeInfinity)
            };
            foreach (FLVER.Vertex vertex in mesh.Vertices)
            {
                FlverTools.UpdateHeaderBoundingBox(model.Header, vertex.Position);
                FlverTools.UpdateMeshBoundingBox(mesh, vertex.Position);

                var boneIndices = FlverTools.BoneIndicesToIntArray(vertex.BoneIndices);
                if (boneIndices != null)
                {
                    foreach (int boneIndex in boneIndices)
                    {
                        if (boneIndex >= 0 && boneIndex < model.Nodes.Count)
                        {
                            FlverTools.UpdateBonesBoundingBox(model.Nodes[boneIndex], model.Nodes, vertex.Position);
                        }
                    }
                }
            }
        }

        ValidateBoundingBoxes(model);
        TaskLogs.AddLog("Bounding Boxes solved.");
    }

    private void ValidateBoundingBoxes(FLVER2 model)
    {
        (Vector3 min, Vector3 max) GetValidBounds(Vector3 min, Vector3 max)
        {
            if (min == new Vector3(float.PositiveInfinity) || max == new Vector3(float.NegativeInfinity))
            {
                return (new Vector3(-1), new Vector3(1));
            }
            return (min, max);
        }

        // Validate header bounds
        {
            var (min, max) = GetValidBounds(model.Header.BoundingBoxMin, model.Header.BoundingBoxMax);
            model.Header.BoundingBoxMin = min;
            model.Header.BoundingBoxMax = max;
        }

        // Validate node bounds
        foreach (var node in model.Nodes)
        {
            var (min, max) = GetValidBounds(node.BoundingBoxMin, node.BoundingBoxMax);
            node.BoundingBoxMin = min;
            node.BoundingBoxMax = max;
        }

        // Validate mesh bounds
        foreach (var mesh in model.Meshes)
        {
            if (mesh.BoundingBox != null)
            {
                var (min, max) = GetValidBounds(mesh.BoundingBox.Min, mesh.BoundingBox.Max);
                mesh.BoundingBox.Min = min;
                mesh.BoundingBox.Max = max;
            }
        }
    }

}
