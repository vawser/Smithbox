using Hexa.NET.ImNodes;
using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public static class HKLib_MeshBuilder
{
    /// <summary>
    /// Builds an hknpExternMeshShape from a flat vertex list and a triangle index list
    /// (3 indices per triangle, matching the layout produced by HKLib_Helper.RenderMesh).
    /// </summary>
    public static hknpExternMeshShape BuildExternMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> triangleIndices,
        bool IncludeMaterialTable,
        bool useTriangleMaterialAsShapeTag = false)
    {
        if (triangleIndices.Count % 3 != 0)
        {
            throw new ArgumentException("triangleIndices must be a multiple of 3.", nameof(triangleIndices));
        }

        var geometry = new hkGeometry();
        geometry.m_vertices.AddRange(vertices.Select(v => new Vector4(v.X, v.Y, v.Z, 0f)));

        for (int i = 0; i < triangleIndices.Count; i += 3)
        {
            geometry.m_triangles.Add(new hkGeometry.Triangle
            {
                m_a = triangleIndices[i],
                m_b = triangleIndices[i + 1],
                m_c = triangleIndices[i + 2],
                m_material = -1
            });
        }

        var geometryWrapper = new hknpDefaultExternMeshShapeGeometry
        {
            m_geometry = geometry,
            m_useTriangleMaterialAsShapeTag = useTriangleMaterialAsShapeTag
        };

        var boundingVolumeData = BuildBoundingVolumeData(geometry);

        int triangleCount = geometry.m_triangles.Count;

        if (IncludeMaterialTable)
        {
            var materialPalette = new hknpMaterialPalette();

            var shape = new hknpExternMeshShape
            {
                m_geometry = geometryWrapper,
                m_boundingVolumeData = boundingVolumeData,
                m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
                m_type = hknpShapeType.Enum.EXTERN_MESH,
                m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
                m_numShapeKeyBits = ShapeKeyBits(triangleCount),
                m_convexRadius = 0f,
                m_userData = 0,
                m_shapeTagCodecInfo = 0,
                m_materialTable = materialPalette
            };

            return shape;
        }
        else
        {
            var shape = new hknpExternMeshShape
            {
                m_geometry = geometryWrapper,
                m_boundingVolumeData = boundingVolumeData,
                m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
                m_type = hknpShapeType.Enum.EXTERN_MESH,
                m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
                m_numShapeKeyBits = ShapeKeyBits(triangleCount),
                m_convexRadius = 0f,
                m_userData = 0,
                m_shapeTagCodecInfo = 0
            };

            return shape;
        }
    }

    private static hknpExternMeshShapeData BuildBoundingVolumeData(
    hkGeometry geometry)
    {
        var data = new hknpExternMeshShapeData();

        var tree = new hkcdSimdTree
        {
            m_nodes = new List<hkcdSimdTree.Node>(),
            m_isCompact = false
        };

        int triangleCount = geometry.m_triangles.Count;

        if (triangleCount == 0)
        {
            // Degenerate case: still needs a well-formed (but empty) root node,
            // never a node with valid-looking-but-meaningless lanes.
            var emptyRoot = new hkcdSimdTree.Node { m_isLeaf = true };
            InitializeEmptyNode(emptyRoot);
            tree.m_nodes.Add(emptyRoot);
            data.m_simdTree = tree;
            return data;
        }

        // Per-triangle AABB + centroid, used to build and to split the BVH.
        var triAabbs = new (Vector3 Min, Vector3 Max)[triangleCount];
        var centroids = new Vector3[triangleCount];
        for (int i = 0; i < triangleCount; i++)
        {
            var tri = geometry.m_triangles[i];
            Vector3 a = ToVector3(geometry.m_vertices[tri.m_a]);
            Vector3 b = ToVector3(geometry.m_vertices[tri.m_b]);
            Vector3 c = ToVector3(geometry.m_vertices[tri.m_c]);

            Vector3 min = Vector3.Min(Vector3.Min(a, b), c);
            Vector3 max = Vector3.Max(Vector3.Max(a, b), c);
            triAabbs[i] = (min, max);
            centroids[i] = (min + max) * 0.5f;
        }

        var allIndices = Enumerable.Range(0, triangleCount).ToList();
        var root = BuildNode(allIndices, triAabbs, centroids);

        var nodeList = new List<hkcdSimdTree.Node>();

        // The sentinel node is critical for a new collision to work
        var sentinel = new hkcdSimdTree.Node { m_isLeaf = true };
        InitializeEmptyNode(sentinel);
        nodeList.Add(sentinel); // index 0

        var queue = new Queue<BvhBuildNode>();
        var slotOf = new Dictionary<BvhBuildNode, int>();

        nodeList.Add(null!); // reserve slot 1 for the root
        slotOf[root] = 1;
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var buildNode = queue.Dequeue();
            int slot = slotOf[buildNode];

            var node = new hkcdSimdTree.Node { m_isLeaf = buildNode.IsLeaf };
            InitializeEmptyNode(node);

            if (buildNode.IsLeaf)
            {
                for (int lane = 0; lane < buildNode.TriangleIndices!.Count; lane++)
                {
                    int triIndex = buildNode.TriangleIndices[lane];
                    SetLaneBounds(node, lane, triAabbs[triIndex].Min, triAabbs[triIndex].Max);
                    node.m_data[lane] = (uint)triIndex; // leaf data = shape key = triangle index
                }
            }
            else
            {
                for (int lane = 0; lane < buildNode.Children!.Count; lane++)
                {
                    var child = buildNode.Children[lane];
                    int childSlot = nodeList.Count;
                    nodeList.Add(null!); // reserve
                    slotOf[child] = childSlot;

                    SetLaneBounds(node, lane, child.Bounds.Min, child.Bounds.Max);
                    node.m_data[lane] = (uint)childSlot;

                    queue.Enqueue(child);
                }
            }

            nodeList[slot] = node;
        }

        tree.m_nodes = nodeList;
        data.m_simdTree = tree;

        return data;
    }

    private static Vector3 ToVector3(Vector4 v) => new(v.X, v.Y, v.Z);

    private sealed class BvhBuildNode
    {
        public bool IsLeaf;
        public List<int> TriangleIndices;
        public List<BvhBuildNode> Children;
        public (Vector3 Min, Vector3 Max) Bounds;
    }

    private const int SimdTreeArity = 4;
    private const int SimdTreeLeafMaxTriangles = 4;

    private static BvhBuildNode BuildNode(
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

    private static (Vector3 Min, Vector3 Max) ComputeBounds(
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

    private static void SetLaneBounds(hkcdSimdTree.Node node, int lane, Vector3 min, Vector3 max)
    {
        SetComponent(ref node.m_lx, lane, min.X);
        SetComponent(ref node.m_hx, lane, max.X);
        SetComponent(ref node.m_ly, lane, min.Y);
        SetComponent(ref node.m_hy, lane, max.Y);
        SetComponent(ref node.m_lz, lane, min.Z);
        SetComponent(ref node.m_hz, lane, max.Z);
    }

    private static void SetComponent(ref Vector4 v, int lane, float value)
    {
        switch (lane)
        {
            case 0: v.X = value; break;
            case 1: v.Y = value; break;
            case 2: v.Z = value; break;
            default: v.W = value; break;
        }
    }

    private static void InitializeEmptyNode(hkcdSimdTree.Node node)
    {
        node.m_lx = new Vector4(
            float.MaxValue,
            float.MaxValue,
            float.MaxValue,
            float.MaxValue);

        node.m_hx = new Vector4(
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue);

        node.m_ly = new Vector4(
            float.MaxValue,
            float.MaxValue,
            float.MaxValue,
            float.MaxValue);

        node.m_hy = new Vector4(
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue);

        node.m_lz = new Vector4(
            float.MaxValue,
            float.MaxValue,
            float.MaxValue,
            float.MaxValue);

        node.m_hz = new Vector4(
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue,
            -float.MaxValue);

        Array.Fill(node.m_data, 0u);
    }

    private static byte ShapeKeyBits(int primitiveCount)
    {
        if (primitiveCount <= 1)
        {
            return 1;
        }

        return (byte)Math.Ceiling(Math.Log2(primitiveCount));
    }

    public static bool ReplaceExternMeshShape(
        hkRootLevelContainer container,
        hknpExternMeshShape newShape,
        int bodyIndex = 0,
        bool force = false)
    {
        var bodyCinfos = GetBodyCinfos(container);
        if (bodyCinfos == null || bodyIndex < 0 || bodyIndex >= bodyCinfos.Count)
        {
            return false;
        }

        var body = bodyCinfos[bodyIndex];
        if (!force && !IsReplaceableMeshShape(body.m_shape))
        {
            return false;
        }

        body.m_shape = newShape;
        return true;
    }

    public static int ReplaceAllExternMeshShapes(
        hkRootLevelContainer container,
        Func<int, hknpExternMeshShape> shapeFactory,
        bool force = false)
    {
        var bodyCinfos = GetBodyCinfos(container);
        if (bodyCinfos == null)
        {
            return 0;
        }

        int replaced = 0;
        for (int i = 0; i < bodyCinfos.Count; i++)
        {
            var body = bodyCinfos[i];
            if (!force && !IsReplaceableMeshShape(body.m_shape))
            {
                continue;
            }

            body.m_shape = shapeFactory(i);
            replaced++;
        }

        return replaced;
    }

    private static List<hknpPhysicsSystemData.bodyCinfoWithAttachment> GetBodyCinfos(hkRootLevelContainer container)
    {
        if (container.m_namedVariants.Count == 0)
        {
            return null;
        }

        if (container.m_namedVariants[0].m_variant is not hknpPhysicsSceneData scene)
        {
            return null;
        }

        if (scene.m_systemDatas.Count == 0 || scene.m_systemDatas[0] is not hknpPhysicsSystemData systemData)
        {
            return null;
        }

        return systemData.m_bodyCinfos;
    }

    private static bool IsReplaceableMeshShape(hknpShape shape)
    {
        return shape is fsnpCustomParamCompressedMeshShape
            or hknpCompressedMeshShape
            or hknpExternMeshShape;
    }

    // ============================================================================
    // 4) Primitive shape generators (Square / Triangle / Circle)
    // ============================================================================
    //
    // All shapes are generated flat on the XZ plane (Y = 0), centered on the
    // origin, single-sided with an upward (+Y) face normal - i.e. suitable as a
    // walkable floor/trigger plane. Winding order matches the normal computed by
    // HKLib_Helper.RenderMesh (n = cross(v3 - v1, v2 - v1)); vertices are wound
    // with increasing angle around the center to get that +Y normal. Feed the
    // returned (vertices, indices) straight into BuildExternMeshShape.
    //
    // If you need the shape on a different plane or facing a different
    // direction, transform the returned vertices afterwards (or flip triangle
    // winding to flip the normal) rather than modifying these generators.

    /// <summary>
    /// Generates a flat rectangular plane, width along X and length along Z,
    /// centered at the origin.
    /// </summary>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateSquare(float width, float length)
    {
        if (width <= 0f) throw new ArgumentOutOfRangeException(nameof(width));
        if (length <= 0f) throw new ArgumentOutOfRangeException(nameof(length));

        float hw = width * 0.5f;
        float hl = length * 0.5f;

        var vertices = new List<Vector3>
        {
            new Vector3(-hw, 0f, -hl), // 0
            new Vector3( hw, 0f, -hl), // 1
            new Vector3( hw, 0f,  hl), // 2
            new Vector3(-hw, 0f,  hl), // 3
        };

        var indices = new List<int>
        {
            0, 1, 2,
            0, 2, 3
        };

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat equilateral triangle centered at the origin.
    /// </summary>
    /// <param name="size">Distance from the center to each corner (circumradius).</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateTriangle(float size)
    {
        if (size <= 0f) throw new ArgumentOutOfRangeException(nameof(size));

        var vertices = new List<Vector3>(3);
        for (int i = 0; i < 3; i++)
        {
            // Start pointing toward +Z, then sweep counter-clockwise (increasing angle).
            float angle = MathF.PI / 2f + i * (2f * MathF.PI / 3f);
            vertices.Add(new Vector3(size * MathF.Cos(angle), 0f, size * MathF.Sin(angle)));
        }

        var indices = new List<int> { 0, 1, 2 };

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat circle (approximated as a regular polygon fan) centered
    /// at the origin.
    /// </summary>
    /// <param name="radius">Circle radius.</param>
    /// <param name="segments">Number of edge segments approximating the circle. Higher = rounder.</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateCircle(float radius, int segments = 24)
    {
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius));
        if (segments < 3) throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 3 segments.");

        var vertices = new List<Vector3>(segments + 1)
        {
            Vector3.Zero // center, index 0
        };

        for (int i = 0; i < segments; i++)
        {
            float angle = i * (2f * MathF.PI / segments);
            vertices.Add(new Vector3(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
        }

        var indices = new List<int>(segments * 3);
        for (int i = 0; i < segments; i++)
        {
            int current = 1 + i;
            int next = 1 + (i + 1) % segments;
            indices.Add(0);
            indices.Add(current);
            indices.Add(next);
        }

        return (vertices, indices);
    }

    /// <summary>
    /// Generates a flat half-disc (semicircle), centered at the origin, with its
    /// straight diameter edge along the X axis and the arc bulging toward +Z.
    /// </summary>
    /// <param name="radius">Circle radius.</param>
    /// <param name="segments">Number of edge segments approximating the arc. Higher = rounder.</param>
    public static (List<Vector3> Vertices, List<int> Indices) GenerateSemiCircle(float radius, int segments = 12)
    {
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius));
        if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 1 segment.");

        // Center + (segments + 1) arc points running from (radius, 0, 0) around
        // to (-radius, 0, 0). The two radius edges from the center to the first
        // and last arc points are collinear (both along X), so together they
        // form the straight diameter edge closing the shape.
        var vertices = new List<Vector3>(segments + 2)
        {
            Vector3.Zero // center, index 0
        };

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (MathF.PI / segments); // 0 .. PI inclusive
            vertices.Add(new Vector3(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
        }

        var indices = new List<int>(segments * 3);
        for (int i = 0; i < segments; i++)
        {
            int current = 1 + i;
            int next = 1 + i + 1;
            indices.Add(0);
            indices.Add(current);
            indices.Add(next);
        }

        return (vertices, indices);
    }
}