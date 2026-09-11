using Hexa.NET.ImNodes;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public static class HavokShapeGenerator_Shared
{
    public static Vector3 ToVector3(Vector4 v) => new(v.X, v.Y, v.Z);

    public sealed class BvhBuildNode
    {
        public bool IsLeaf;
        public List<int> TriangleIndices;
        public List<BvhBuildNode> Children;
        public (Vector3 Min, Vector3 Max) Bounds;
    }

    public const int SimdTreeArity = 4;
    public const int SimdTreeLeafMaxTriangles = 4;

    public static BvhBuildNode BuildNode(
        List<int> triIndices,
        (Vector3 Min, Vector3 Max)[] triAabbs,
        Vector3[] centroids)
    {
        var bounds = ComputeBounds(triIndices, triAabbs);

        if (triIndices.Count <= SimdTreeLeafMaxTriangles)
        {
            return new BvhBuildNode
            {
                IsLeaf = true,
                TriangleIndices = triIndices,
                Bounds = bounds
            };
        }

        // Split along the longest axis of the centroid extent, then chop the
        // sorted list into up to 4 roughly equal contiguous chunks.
        Vector3 centroidMin = triIndices.Select(i => centroids[i]).Aggregate(Vector3.Min);
        Vector3 centroidMax = triIndices.Select(i => centroids[i]).Aggregate(Vector3.Max);
        Vector3 extent = centroidMax - centroidMin;

        int axis = 0;
        if (extent.Y > extent.X && extent.Y >= extent.Z) axis = 1;
        else if (extent.Z > extent.X && extent.Z >= extent.Y) axis = 2;

        var sorted = triIndices
            .OrderBy(i => axis == 0 ? centroids[i].X : axis == 1 ? centroids[i].Y : centroids[i].Z)
            .ToList();

        var children = new List<BvhBuildNode>(SimdTreeArity);
        int chunkSize = (int)Math.Ceiling(sorted.Count / (double)SimdTreeArity);
        for (int start = 0; start < sorted.Count; start += chunkSize)
        {
            int count = Math.Min(chunkSize, sorted.Count - start);
            var chunk = sorted.GetRange(start, count);
            children.Add(BuildNode(chunk, triAabbs, centroids));
        }

        return new BvhBuildNode
        {
            IsLeaf = false,
            Children = children,
            Bounds = bounds
        };
    }

    public static (Vector3 Min, Vector3 Max) ComputeBounds(
        List<int> triIndices,
        (Vector3 Min, Vector3 Max)[] triAabbs)
    {
        Vector3 min = new(float.MaxValue);
        Vector3 max = new(-float.MaxValue);
        foreach (int i in triIndices)
        {
            min = Vector3.Min(min, triAabbs[i].Min);
            max = Vector3.Max(max, triAabbs[i].Max);
        }
        return (min, max);
    }

    public static void SetComponent(ref Vector4 v, int lane, float value)
    {
        switch (lane)
        {
            case 0: v.X = value; break;
            case 1: v.Y = value; break;
            case 2: v.Z = value; break;
            default: v.W = value; break;
        }
    }

    public static byte ShapeKeyBits(int primitiveCount)
    {
        if (primitiveCount <= 1)
        {
            return 1;
        }

        return (byte)Math.Ceiling(Math.Log2(primitiveCount));
    }

}