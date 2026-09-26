using HKX2;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public static class HavokShapeGenerator_HKX2
{
    public static hknpExternMeshShape BuildExternMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> triangleIndices)
    {
        if (triangleIndices.Count % 3 != 0)
        {
            throw new ArgumentException("triangleIndices must be a multiple of 3.", nameof(triangleIndices));
        }
        int triangleCount = triangleIndices.Count / 3;

        var geometry = new hkGeometry();
        geometry.m_vertices = new();
        geometry.m_triangles = new();

        geometry.m_vertices.AddRange(vertices.Select(v => new Vector4(v.X, v.Y, v.Z, 0f)));

        for (int i = 0; i < triangleIndices.Count; i += 3)
        {
            geometry.m_triangles.Add(new hkGeometryTriangle
            {
                m_a = triangleIndices[i],
                m_b = triangleIndices[i + 1],
                m_c = triangleIndices[i + 2],
                m_material = 0
            });
        }

        var geometryWrapper = new hknpDefaultExternMeshShapeGeometry
        {
            m_geometry = geometry
        };

        var boundingVolumeData = BuildBoundingVolumeData(geometry);

        var shape = new hknpExternMeshShape
        {
            m_geometry = geometryWrapper,
            m_boundingVolumeData = boundingVolumeData,
            m_flags = (ushort)hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_dispatchType = HKX2.Enum.COMPOSITE,
            m_numShapeKeyBits = HavokShapeGenerator_Shared.ShapeKeyBits(triangleCount),
            m_convexRadius = 0f,
            m_userData = 0,
            m_shapeTagCodecInfo = 0,
            m_edgeWeldingMap = new()
        };

        return shape;
    }

    private static hknpExternMeshShapeData BuildBoundingVolumeData(hkGeometry geometry)
    {
        var data = new hknpExternMeshShapeData();

        int triangleCount = geometry.m_triangles.Count;

        // Per-triangle AABB + centroid, shared by both the aabbTree and the simdTree builders.
        var triAabbs = new (Vector3 Min, Vector3 Max)[triangleCount];
        var centroids = new Vector3[triangleCount];
        Vector3 domainMin = new Vector3(float.MaxValue);
        Vector3 domainMax = new Vector3(-float.MaxValue);

        for (int i = 0; i < triangleCount; i++)
        {
            var tri = geometry.m_triangles[i];
            Vector3 a = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_a]);
            Vector3 b = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_b]);
            Vector3 c = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_c]);

            Vector3 min = Vector3.Min(Vector3.Min(a, b), c);
            Vector3 max = Vector3.Max(Vector3.Max(a, b), c);
            triAabbs[i] = (min, max);
            centroids[i] = (min + max) * 0.5f;

            domainMin = Vector3.Min(domainMin, min);
            domainMax = Vector3.Max(domainMax, max);
        }

        data.m_aabbTree = new hkcdStaticTreeDefaultTreeStorage6()
        {
            m_domain = new(),
            m_nodes = new()
        };

        //data.m_aabbTree = BuildAabbTree(triangleCount, triAabbs, centroids, domainMin, domainMax);

        data.m_simdTree = new hkcdSimdTree()
        {
            m_nodes = new()
        };

        // data.m_simdTree = BuildSimdTree(triangleCount, triAabbs, centroids);

        return data;
    }

    private static hkcdSimdTree BuildSimdTree(
        int triangleCount,
        (Vector3 Min, Vector3 Max)[] triAabbs,
        Vector3[] centroids)
    {
        var tree = new hkcdSimdTree
        {
            m_nodes = new List<hkcdSimdTreeNode>()
        };

        if (triangleCount == 0)
        {
            // Degenerate case: still needs a well-formed (but empty) root node,
            // never a node with valid-looking-but-meaningless lanes.
            var emptyRoot = new hkcdSimdTreeNode { };
            InitializeEmptyNode(emptyRoot);
            tree.m_nodes.Add(emptyRoot);
            return tree;
        }

        var allIndices = Enumerable.Range(0, triangleCount).ToList();
        var root = HavokShapeGenerator_Shared.BuildNode(allIndices, triAabbs, centroids);

        var nodeList = new List<hkcdSimdTreeNode>();

        // The sentinel node is critical for a new collision to work
        var sentinel = new hkcdSimdTreeNode { };
        InitializeEmptyNode(sentinel);
        nodeList.Add(sentinel); // index 0

        var queue = new Queue<HavokShapeGenerator_Shared.BvhBuildNode>();
        var slotOf = new Dictionary<HavokShapeGenerator_Shared.BvhBuildNode, int>();

        nodeList.Add(null!); // reserve slot 1 for the root
        slotOf[root] = 1;
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var buildNode = queue.Dequeue();
            int slot = slotOf[buildNode];

            var node = new hkcdSimdTreeNode { };
            InitializeEmptyNode(node);

            if (buildNode.IsLeaf)
            {
                for (int lane = 0; lane < buildNode.TriangleIndices!.Count; lane++)
                {
                    int triIndex = buildNode.TriangleIndices[lane];
                    SetLaneBounds(node, lane, triAabbs[triIndex].Min, triAabbs[triIndex].Max);

                    if (lane == 0)
                    {
                        node.m_data_0 = (uint)triIndex;
                    }
                    else if (lane == 1)
                    {
                        node.m_data_1 = (uint)triIndex;
                    }
                    else if (lane == 2)
                    {
                        node.m_data_2 = (uint)triIndex;
                    }
                    else if (lane == 3)
                    {
                        node.m_data_3 = (uint)triIndex;
                    }
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

                    if (lane == 0)
                    {
                        node.m_data_0 = (uint)childSlot;
                    }
                    else if (lane == 1)
                    {
                        node.m_data_1 = (uint)childSlot;
                    }
                    else if (lane == 2)
                    {
                        node.m_data_2 = (uint)childSlot;
                    }
                    else if (lane == 3)
                    {
                        node.m_data_3 = (uint)childSlot;
                    }

                    queue.Enqueue(child);
                }
            }

            nodeList[slot] = node;
        }

        tree.m_nodes = nodeList;
        return tree;
    }

    // ------------------------------------------------------------------
    // aabbTree (hkcdStaticTreeDefaultTreeStorage6 / hkcdStaticTreeCodec3Axis6)
    //
    // ASSUMPTIONS - not verified against Havok's proprietary source, only
    // inferred from the 3+1+2 byte node layout:
    //
    //   m_xyz_0 / m_xyz_1 / m_xyz_2  - one byte per axis (X / Y / Z).
    //       low nibble  = quantized "lo" plane (0..15)
    //       high nibble = quantized "hi" plane (0..15)
    //     Both are relative to the *parent* node's box (the root's parent
    //     being m_domain), rounded outward so a decoded child box always
    //     fully contains the true geometry it bounds.
    //
    //   m_hiData (8 bits) + m_loData (16 bits) -> one 24-bit field:
    //       bit 0        = isLeaf
    //       bits [1..24) = triangle index (leaf) or first-child index
    //                      (internal node; the sibling is always at
    //                      firstChildIndex + 1)
    //
    //   The tree is a strict binary tree with exactly one triangle per
    //   leaf, and node 0 is an empty sentinel (mirroring the simdTree's
    //   sentinel), with the real root at node 1.
    //
    // If you have a known-good aabbTree extracted from a real game file,
    // decoding it and comparing against this layout is the fastest way to
    // confirm or correct these assumptions before trusting generated
    // output in-game.
    // ------------------------------------------------------------------

    private const int PlaneBits = 4;
    private const int PlaneMax = (1 << PlaneBits) - 1; // 15

    private sealed class AabbBinNode
    {
        public Vector3 Min, Max;
        public AabbBinNode Left;
        public AabbBinNode Right;
        public int TriangleIndex = -1; // >= 0 for leaves
        public bool IsLeaf => TriangleIndex >= 0;
    }

    private static AabbBinNode BuildAabbBinNode(
        List<int> indices,
        (Vector3 Min, Vector3 Max)[] triAabbs,
        Vector3[] centroids)
    {
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(-float.MaxValue);
        foreach (var idx in indices)
        {
            min = Vector3.Min(min, triAabbs[idx].Min);
            max = Vector3.Max(max, triAabbs[idx].Max);
        }

        if (indices.Count == 1)
        {
            return new AabbBinNode { Min = min, Max = max, TriangleIndex = indices[0] };
        }

        Vector3 extent = max - min;
        int axis = 0;
        if (extent.Y > extent.X) axis = 1;
        if (extent.Z > (axis == 1 ? extent.Y : extent.X)) axis = 2;

        indices.Sort((a, b) =>
        {
            float ca = axis == 0 ? centroids[a].X : axis == 1 ? centroids[a].Y : centroids[a].Z;
            float cb = axis == 0 ? centroids[b].X : axis == 1 ? centroids[b].Y : centroids[b].Z;
            return ca.CompareTo(cb);
        });

        int mid = indices.Count / 2;
        var left = BuildAabbBinNode(indices.GetRange(0, mid), triAabbs, centroids);
        var right = BuildAabbBinNode(indices.GetRange(mid, indices.Count - mid), triAabbs, centroids);

        return new AabbBinNode { Min = min, Max = max, Left = left, Right = right };
    }

    private static byte EncodeAxisPlanes(float parentMin, float parentMax, float childMin, float childMax)
    {
        float range = parentMax - parentMin;
        int lo, hi;
        if (range <= 1e-8f)
        {
            lo = 0;
            hi = PlaneMax;
        }
        else
        {
            float loT = (childMin - parentMin) / range;
            float hiT = (childMax - parentMin) / range;

            // Round outward so the decoded box always fully contains the real one.
            lo = (int)MathF.Floor(loT * PlaneMax);
            hi = (int)MathF.Ceiling(hiT * PlaneMax);
            lo = Math.Clamp(lo, 0, PlaneMax);
            hi = Math.Clamp(hi, 0, PlaneMax);
            if (hi < lo) hi = lo;
        }
        return (byte)((lo & 0xF) | ((hi & 0xF) << 4));
    }

    private static void EncodeNodeBounds(
        hkcdStaticTreeCodec3Axis6 node,
        Vector3 parentMin, Vector3 parentMax,
        Vector3 childMin, Vector3 childMax)
    {
        node.m_xyz_0 = EncodeAxisPlanes(parentMin.X, parentMax.X, childMin.X, childMax.X);
        node.m_xyz_1 = EncodeAxisPlanes(parentMin.Y, parentMax.Y, childMin.Y, childMax.Y);
        node.m_xyz_2 = EncodeAxisPlanes(parentMin.Z, parentMax.Z, childMin.Z, childMax.Z);
    }

    private static void EncodeNodeData(hkcdStaticTreeCodec3Axis6 node, bool isLeaf, uint payload)
    {
        uint packed = (payload << 1) | (isLeaf ? 1u : 0u);
        node.m_hiData = (byte)((packed >> 16) & 0xFF);
        node.m_loData = (ushort)(packed & 0xFFFF);
    }

    private static hkcdStaticTreeDefaultTreeStorage6 BuildAabbTree(
        int triangleCount,
        (Vector3 Min, Vector3 Max)[] triAabbs,
        Vector3[] centroids,
        Vector3 domainMin,
        Vector3 domainMax)
    {
        var storage = new hkcdStaticTreeDefaultTreeStorage6
        {
            m_domain = new hkAabb(),
            m_nodes = new List<hkcdStaticTreeCodec3Axis6>()
        };

        if (triangleCount == 0)
        {
            storage.m_domain.m_min = new Vector4(0, 0, 0, 0);
            storage.m_domain.m_max = new Vector4(0, 0, 0, 0);

            var sentinelOnly = new hkcdStaticTreeCodec3Axis6();
            EncodeNodeData(sentinelOnly, isLeaf: true, payload: 0);
            storage.m_nodes.Add(sentinelOnly);
            return storage;
        }

        storage.m_domain.m_min = new Vector4(domainMin, 0f);
        storage.m_domain.m_max = new Vector4(domainMax, 0f);

        var root = BuildAabbBinNode(Enumerable.Range(0, triangleCount).ToList(), triAabbs, centroids);

        var nodes = new List<hkcdStaticTreeCodec3Axis6>();

        // Sentinel node, mirroring the simdTree's leading empty node.
        var sentinel = new hkcdStaticTreeCodec3Axis6();
        EncodeNodeData(sentinel, isLeaf: true, payload: 0);
        nodes.Add(sentinel); // index 0

        nodes.Add(null!); // reserve slot 1 for the root

        var queue = new Queue<(AabbBinNode Node, int Slot, Vector3 ParentMin, Vector3 ParentMax)>();
        queue.Enqueue((root, 1, domainMin, domainMax));

        while (queue.Count > 0)
        {
            var (buildNode, slot, parentMin, parentMax) = queue.Dequeue();

            var node = new hkcdStaticTreeCodec3Axis6();
            EncodeNodeBounds(node, parentMin, parentMax, buildNode.Min, buildNode.Max);

            if (buildNode.IsLeaf)
            {
                EncodeNodeData(node, isLeaf: true, payload: (uint)buildNode.TriangleIndex);
            }
            else
            {
                int firstChildSlot = nodes.Count;
                nodes.Add(null!); // left
                nodes.Add(null!); // right

                EncodeNodeData(node, isLeaf: false, payload: (uint)firstChildSlot);

                queue.Enqueue((buildNode.Left, firstChildSlot, buildNode.Min, buildNode.Max));
                queue.Enqueue((buildNode.Right, firstChildSlot + 1, buildNode.Min, buildNode.Max));
            }

            nodes[slot] = node;
        }

        storage.m_nodes = nodes;
        return storage;
    }

    private static void SetLaneBounds(hkcdSimdTreeNode node, int lane, Vector3 min, Vector3 max)
    {
        HavokShapeGenerator_Shared.SetComponent(ref node.m_lx, lane, min.X);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hx, lane, max.X);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_ly, lane, min.Y);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hy, lane, max.Y);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_lz, lane, min.Z);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hz, lane, max.Z);
    }

    private static void InitializeEmptyNode(hkcdSimdTreeNode node)
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

        node.m_data_0 = 0u;
        node.m_data_1 = 0u;
        node.m_data_2 = 0u;
        node.m_data_3 = 0u;
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

        // Update referenced objects
        var systemData = GetPhysicsSystemData(container);
        systemData.m_referencedObjects.Clear();
        systemData.m_referencedObjects.Add(newShape);

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

    private static hknpPhysicsSystemData GetPhysicsSystemData(hkRootLevelContainer container)
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

        return systemData;
    }

    private static List<hknpBodyCinfo> GetBodyCinfos(hkRootLevelContainer container)
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

}