using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
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
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Framework;

public class ModelActionHandler
{
    private ModelEditorScreen Editor;
    public ModelSelectionManager Selection;

    public ModelActionHandler(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
    }

    public void CreateHandler()
    {
        if (!Editor.ResManager.HasCurrentFLVER())
            return;

        var currentFlver = Editor.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Editor.Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                action = new AddDummy(Editor, currentFlver);
                break;

            case GroupSelectionType.Material:
                action = new AddMaterial(Editor, currentFlver);
                break;

            case GroupSelectionType.GXList:
                action = new AddGxList(Editor, currentFlver);
                break;

            case GroupSelectionType.Node:
                action = new AddNode(Editor, currentFlver);
                break;

            case GroupSelectionType.Mesh:
                action = new AddMesh(Editor, currentFlver);
                break;

            case GroupSelectionType.BufferLayout:
                action = new AddBufferLayout(Editor, currentFlver);
                break;

            case GroupSelectionType.BaseSkeleton:
                action = new AddBaseSkeletonBone(Editor, currentFlver);
                break;

            case GroupSelectionType.AllSkeleton:
                action = new AddAllSkeletonBone(Editor, currentFlver);
                break;
        }

        if (action != null)
            Editor.EditorActionManager.ExecuteAction(action);
    }

    public void DuplicateHandler()
    {
        var currentFlver = Editor.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                if (Selection.DummyMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleDummies(Editor, currentFlver, Selection.DummyMultiselect);
                else
                    if (Selection._selectedDummy != -1)
                    action = new DuplicateDummy(Editor, currentFlver, Selection._selectedDummy);
                break;

            case GroupSelectionType.Material:
                if (Selection.MaterialMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleMaterials(Editor, currentFlver, Selection.MaterialMultiselect);
                else
                    if (Selection._selectedMaterial != -1)
                    action = new DuplicateMaterial(Editor, currentFlver, Selection._selectedMaterial);
                break;

            case GroupSelectionType.GXList:
                if (Selection.GxListMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleGxLists(Editor, currentFlver, Selection.GxListMultiselect);
                else
                    if (Selection._selectedGXList != -1)
                    action = new DuplicateGxList(Editor, currentFlver, Selection._selectedGXList);
                break;

            case GroupSelectionType.Node:
                if (Selection.NodeMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleNodes(Editor, currentFlver, Selection.NodeMultiselect);
                else
                    if (Selection._selectedNode != -1)
                    action = new DuplicateNode(Editor, currentFlver, Selection._selectedNode);
                break;

            case GroupSelectionType.Mesh:
                if (Selection.MeshMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleMeshes(Editor, currentFlver, Selection.MeshMultiselect);
                else
                    if (Selection._selectedMesh != -1)
                    action = new DuplicateMesh(Editor, currentFlver, Selection._selectedMesh);
                break;

            case GroupSelectionType.BufferLayout:
                if (Selection.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleBufferLayouts(Editor, currentFlver, Selection.BufferLayoutMultiselect);
                else
                    if (Selection._selectedBufferLayout != -1)
                    action = new DuplicateBufferLayout(Editor, currentFlver, Selection._selectedBufferLayout);
                break;

            case GroupSelectionType.BaseSkeleton:
                if (Selection.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleBaseSkeletonBones(Editor, currentFlver, Selection.BaseSkeletonMultiselect);
                else
                    if (Selection._selectedBaseSkeletonBone != -1)
                    action = new DuplicateBaseSkeletonBone(Editor, currentFlver, Selection._selectedBaseSkeletonBone);
                break;

            case GroupSelectionType.AllSkeleton:
                if (Selection.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new DuplicateMultipleAllSkeletonBones(Editor, currentFlver, Selection.AllSkeletonMultiselect);
                else
                    if (Selection._selectedAllSkeletonBone != -1)
                    action = new DuplicateAllSkeletonBone(Editor, currentFlver, Selection._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Editor.EditorActionManager.ExecuteAction(action);
    }

    public void DeleteHandler()
    {
        var currentFlver = Editor.ResManager.GetCurrentFLVER();
        ViewportAction action = null;

        switch (Selection._selectedFlverGroupType)
        {
            case GroupSelectionType.None:
            case GroupSelectionType.Header:
                break;

            case GroupSelectionType.Dummy:
                if (Selection.DummyMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleDummies(Editor, currentFlver, Selection.DummyMultiselect);
                else
                    if (Selection._selectedDummy != -1)
                    action = new RemoveDummy(Editor, currentFlver, Selection._selectedDummy);
                break;

            case GroupSelectionType.Material:
                if (Selection.MaterialMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleMaterials(Editor, currentFlver, Selection.MaterialMultiselect);
                else
                    if (Selection._selectedMaterial != -1)
                    action = new RemoveMaterial(Editor, currentFlver, Selection._selectedMaterial);
                break;

            case GroupSelectionType.GXList:
                if (Selection.GxListMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleGxLists(Editor, currentFlver, Selection.GxListMultiselect);
                else
                    if (Selection._selectedGXList != -1)
                    action = new RemoveGxList(Editor, currentFlver, Selection._selectedGXList);
                break;

            case GroupSelectionType.Node:
                if (Selection.NodeMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleNodes(Editor, currentFlver, Selection.NodeMultiselect);
                else
                    if (Selection._selectedNode != -1)
                    action = new RemoveNode(Editor, currentFlver, Selection._selectedNode);
                break;

            case GroupSelectionType.Mesh:
                if (Selection.MeshMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleMeshes(Editor, currentFlver, Selection.MeshMultiselect);
                else
                    if (Selection._selectedMesh != -1)
                    action = new RemoveMesh(Editor, currentFlver, Selection._selectedMesh);
                break;

            case GroupSelectionType.BufferLayout:
                if (Selection.BufferLayoutMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleBufferLayouts(Editor, currentFlver, Selection.BufferLayoutMultiselect);
                else
                    if (Selection._selectedBufferLayout != -1)
                    action = new RemoveBufferLayout(Editor, currentFlver, Selection._selectedBufferLayout);
                break;

            case GroupSelectionType.BaseSkeleton:
                if (Selection.BaseSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleBaseSkeletonBones(Editor, currentFlver, Selection.BaseSkeletonMultiselect);
                else
                    if (Selection._selectedBaseSkeletonBone != -1)
                    action = new RemoveBaseSkeletonBone(Editor, currentFlver, Selection._selectedBaseSkeletonBone);
                break;

            case GroupSelectionType.AllSkeleton:
                if (Selection.AllSkeletonMultiselect.HasValidMultiselection())
                    action = new RemoveMultipleAllSkeletonBones(Editor, currentFlver, Selection.AllSkeletonMultiselect);
                else
                    if (Selection._selectedAllSkeletonBone != -1)
                    action = new RemoveAllSkeletonBone(Editor, currentFlver, Selection._selectedAllSkeletonBone);
                break;
        }

        if (action != null)
            Editor.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Reverse Mesh Normals
    /// </summary>
    public void ReverseMeshNormals()
    {
        var model = Editor.ResManager.GetCurrentFLVER();
        var selectedMesh = Editor.Selection._selectedMesh;

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

        Editor.ViewportManager.UpdateRepresentativeModel();

        TaskLogs.AddLog("Mesh normals have been reversed.");
    }

    /// <summary>
    /// Reverse Mesh Faceset
    /// </summary>
    public void ReverseMeshFaceSet()
    {
        var model = Editor.ResManager.GetCurrentFLVER();
        var selectedMesh = Editor.Selection._selectedMesh;
        var selectedFaceSet = Editor.Selection._subSelectedFaceSetRow;

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

        Editor.ViewportManager.UpdateRepresentativeModel();

        TaskLogs.AddLog("Face Set reversed.");
    }

    /// <summary>
    /// Solve Bounding Boxes
    /// </summary>
    public void SolveBoundingBoxes()
    {
        var model = Editor.ResManager.GetCurrentFLVER();

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
