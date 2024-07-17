using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Vulkan;

namespace StudioCore.Editors.ModelEditor.Tools
{
    // Credit to original author of the implementations in FLVER Editor
    public static class FlverTools
    {
        public static void ReverseNormals(ModelEditorScreen screen)
        {
            var model = screen.ResourceHandler.CurrentFLVER;
            var selectedMesh = screen.ModelHierarchy._selectedMesh;

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

            // TODO: include viewport model update here

            TaskLogs.AddLog("Mesh normals have been reversed.");
        }

        public static void ReverseFaceSet(ModelEditorScreen screen)
        {
            var model = screen.ResourceHandler.CurrentFLVER;
            var selectedMesh = screen.ModelHierarchy._selectedMesh;
            var selectedFaceSet = screen.ModelHierarchy._subSelectedFaceSetRow;

            if (selectedMesh == -1)
                return;

            if (selectedFaceSet == -1)
                return;

            var faceSet = model.Meshes[selectedMesh].FaceSets[selectedFaceSet];

            for (int j = 0; j < faceSet.Indices.Count; j += 3)
            {
                if (j > faceSet.Indices.Count - 2) 
                    continue;

                (faceSet.Indices[j + 1], faceSet.Indices[j + 2]) = (faceSet.Indices[j + 2], faceSet.Indices[j + 1]);
            }

            // TODO: include viewport model update here

            TaskLogs.AddLog("Face Set reversed.");
        }

        public static void SolveBoundingBoxes(FLVER2 model)
        {
            model.Header.BoundingBoxMin = new Vector3();
            model.Header.BoundingBoxMax = new Vector3();
            foreach (FLVER.Node node in model.Nodes)
            {
                node.BoundingBoxMin = new Vector3();
                node.BoundingBoxMax = new Vector3();
            }
            foreach (FLVER2.Mesh mesh in model.Meshes)
            {
                foreach (FLVER.Vertex vertex in mesh.Vertices)
                {
                    UpdateHeaderBoundingBox(model.Header, vertex.Position);
                    UpdateMeshBoundingBox(mesh, vertex.Position);

                    if (BoneIndicesToIntArray(vertex.BoneIndices) == null) 
                        continue;

                    foreach (int nodeIndex in BoneIndicesToIntArray(vertex.BoneIndices))
                    {
                        bool boneDoesNotExist = false;

                        if (nodeIndex >= 0 && nodeIndex < model.Nodes.Count)
                            model.Nodes[nodeIndex].Flags = 0;
                        else 
                            boneDoesNotExist = true;

                        if (!boneDoesNotExist) 
                            UpdateBonesBoundingBox(model.Nodes[nodeIndex], model.Nodes, vertex.Position);
                    }
                }
            }

            TaskLogs.AddLog("Bounding Boxes solved.");
        }

        private static void UpdateHeaderBoundingBox(FLVER2.FLVERHeader header, Vector3 vertexPos)
        {
            float minX = Math.Min(header.BoundingBoxMin.X, vertexPos.X);
            float minY = Math.Min(header.BoundingBoxMin.Y, vertexPos.Y);
            float minZ = Math.Min(header.BoundingBoxMin.Z, vertexPos.Z);
            float maxX = Math.Max(header.BoundingBoxMax.X, vertexPos.X);
            float maxY = Math.Max(header.BoundingBoxMax.Y, vertexPos.Y);
            float maxZ = Math.Max(header.BoundingBoxMax.Z, vertexPos.Z);
            header.BoundingBoxMin = new Vector3(minX, minY, minZ);
            header.BoundingBoxMax = new Vector3(maxX, maxY, maxZ);
        }

        private static void UpdateMeshBoundingBox(FLVER2.Mesh mesh, Vector3 vertexPos)
        {
            mesh.BoundingBox ??= new FLVER2.Mesh.BoundingBoxes();
            float minX = Math.Min(mesh.BoundingBox.Min.X, vertexPos.X);
            float minY = Math.Min(mesh.BoundingBox.Min.Y, vertexPos.Y);
            float minZ = Math.Min(mesh.BoundingBox.Min.Z, vertexPos.Z);
            float maxX = Math.Max(mesh.BoundingBox.Max.X, vertexPos.X);
            float maxY = Math.Max(mesh.BoundingBox.Max.Y, vertexPos.Y);
            float maxZ = Math.Max(mesh.BoundingBox.Max.Z, vertexPos.Z);
            mesh.BoundingBox.Min = new Vector3(minX, minY, minZ);
            mesh.BoundingBox.Max = new Vector3(maxX, maxY, maxZ);
        }

        private static void UpdateBonesBoundingBox(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes, Vector3 vertexPos)
        {
            Matrix4x4 boneAbsoluteMatrix = GetAbsoluteNMatrix(node, nodes);

            if (!Matrix4x4.Invert(boneAbsoluteMatrix, out Matrix4x4 invertedBoneMatrix)) 
                return;

            Vector3 posForBBox = Vector3.Transform(vertexPos, invertedBoneMatrix);
            float minX = Math.Min(node.BoundingBoxMin.X, posForBBox.X);
            float minY = Math.Min(node.BoundingBoxMin.Y, posForBBox.Y);
            float minZ = Math.Min(node.BoundingBoxMin.Z, posForBBox.Z);
            float maxX = Math.Max(node.BoundingBoxMax.X, posForBBox.X);
            float maxY = Math.Max(node.BoundingBoxMax.Y, posForBBox.Y);
            float maxZ = Math.Max(node.BoundingBoxMax.Z, posForBBox.Z);
            node.BoundingBoxMin = new Vector3(minX, minY, minZ);
            node.BoundingBoxMax = new Vector3(maxX, maxY, maxZ);
        }

        private static Matrix4x4 GetAbsoluteNMatrix(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes)
        {
            Matrix4x4 result = Matrix4x4.Identity;
            FLVER.Node parentNode = node;

            while (parentNode != null)
            {
                Matrix4x4 m = GetNMatrix(parentNode);
                result *= m;
                parentNode = GetParent(parentNode, nodes);
            }
            return result;
        }
        private static Matrix4x4 GetNMatrix(FLVER.Node node)
        {
            return Matrix4x4.CreateScale(node.Scale)
                * Matrix4x4.CreateRotationX(node.Rotation.X)
                * Matrix4x4.CreateRotationZ(node.Rotation.Z)
                * Matrix4x4.CreateRotationY(node.Rotation.Y)
                * Matrix4x4.CreateTranslation(node.Position);
        }
        private static FLVER.Node GetParent(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes)
        {
            if (node.ParentIndex >= 0 && node.ParentIndex < nodes.Count) 
                return nodes[node.ParentIndex];

            return null;
        }

        private static int[] BoneIndicesToIntArray(FLVER.VertexBoneIndices boneIndices)
        {
            return new int[] { boneIndices[0], boneIndices[1], boneIndices[2], boneIndices[3] };
        }
        private static float[] BoneWeightsToFloatArray(FLVER.VertexBoneWeights boneWeights)
        {
            return new float[] { boneWeights[0], boneWeights[1], boneWeights[2], boneWeights[3] };
        }
    }
}
