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
            m_shapeTagCodecInfo = 0
        };

        return shape;
    }
    public static hknpMaterialDescriptor MakeMaterialDescriptor(
        int slot,
        hknpMaterial material,
        string name = null)
    {
        return new hknpMaterialDescriptor
        {
            m_materialId = (ushort)slot,
            m_name = name,
            m_material = new hknpRefMaterial { m_material = material }
        };
    }

    private static hknpExternMeshShapeData BuildBoundingVolumeData(
    hkGeometry geometry)
    {
        var data = new hknpExternMeshShapeData();

        var tree = new hkcdSimdTree
        {
            m_nodes = new List<hkcdSimdTreeNode>()
        };

        int triangleCount = geometry.m_triangles.Count;

        if (triangleCount == 0)
        {
            // Degenerate case: still needs a well-formed (but empty) root node,
            // never a node with valid-looking-but-meaningless lanes.
            var emptyRoot = new hkcdSimdTreeNode { };
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
            Vector3 a = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_a]);
            Vector3 b = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_b]);
            Vector3 c = HavokShapeGenerator_Shared.ToVector3(geometry.m_vertices[tri.m_c]);

            Vector3 min = Vector3.Min(Vector3.Min(a, b), c);
            Vector3 max = Vector3.Max(Vector3.Max(a, b), c);
            triAabbs[i] = (min, max);
            centroids[i] = (min + max) * 0.5f;
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
        data.m_simdTree = tree;

        return data;
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
