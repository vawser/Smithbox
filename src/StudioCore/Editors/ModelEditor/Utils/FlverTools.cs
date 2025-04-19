using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Vulkan;

namespace StudioCore.Editors.ModelEditor.Utils;

// Credit to original author of the implementations in FLVER Editor
public static class FlverTools
{
    public static void UpdateHeaderBoundingBox(FLVER2.FLVERHeader header, Vector3 vertexPos)
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


    public static void UpdateMeshBoundingBox(FLVER2.Mesh mesh, Vector3 vertexPos)
    {
        mesh.BoundingBox ??= new FLVER2.Mesh.BoundingBoxes();
        mesh.BoundingBox.Min = Vector3.Min(mesh.BoundingBox.Min, vertexPos);
        mesh.BoundingBox.Max = Vector3.Max(mesh.BoundingBox.Max, vertexPos);
    }

    public static void UpdateBonesBoundingBox(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes, Vector3 vertexPos)
    {
        Matrix4x4 boneAbsoluteMatrix = GetAbsoluteNMatrix(node, nodes);

        if (!Matrix4x4.Invert(boneAbsoluteMatrix, out Matrix4x4 invertedBoneMatrix))
            return;

        Vector3 posForBBox = Vector3.Transform(vertexPos, invertedBoneMatrix);
        node.BoundingBoxMin = Vector3.Min(node.BoundingBoxMin, posForBBox);
        node.BoundingBoxMax = Vector3.Max(node.BoundingBoxMax, posForBBox);
    }

    public static Matrix4x4 GetAbsoluteNMatrix(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes)
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
    public static Matrix4x4 GetNMatrix(FLVER.Node node)
    {
        return Matrix4x4.CreateScale(node.Scale)
            * Matrix4x4.CreateRotationX(node.Rotation.X)
            * Matrix4x4.CreateRotationZ(node.Rotation.Z)
            * Matrix4x4.CreateRotationY(node.Rotation.Y)
            * Matrix4x4.CreateTranslation(node.Translation);
    }
    public static FLVER.Node GetParent(FLVER.Node node, IReadOnlyList<FLVER.Node> nodes)
    {
        if (node.ParentIndex >= 0 && node.ParentIndex < nodes.Count)
            return nodes[node.ParentIndex];

        return null;
    }

    public static int[] BoneIndicesToIntArray(FLVER.VertexBoneIndices boneIndices)
    {
        return new int[] { boneIndices[0], boneIndices[1], boneIndices[2], boneIndices[3] };
    }
    public static float[] BoneWeightsToFloatArray(FLVER.VertexBoneWeights boneWeights)
    {
        return new float[] { boneWeights[0], boneWeights[1], boneWeights[2], boneWeights[3] };
    }
}
