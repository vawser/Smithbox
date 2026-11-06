// The Collision Generator's shapes for the Havok 2018 (ER, NR, DB) and 2019 (AC6) classes: one source, compiled into
// both class sets (Havok.hk2018 defines HK2018, Havok.hk2019 defines HK2019), so it is written once and still typed
// against each SDK's classes. The two differ only where marked: 2018 composite shapes have a material table, 2019
// extern meshes have m_isMutable. The algorithms are in Havok.Shared/Builders. DS3 (hk2014) lays the compressed mesh
// out differently and keeps its own generator in Smithbox. (Documentation/Claude/Havok Phase 6.4 Collision Generator.md)
#if HK2018
namespace Havok.hk2018.Builders;
#elif HK2019
namespace Havok.hk2019.Builders;
#endif

using System.Numerics;
using Havok.Builders;
using CompressedMeshSection = hkcdStaticMeshTree.Section;
using CompressedMeshPrimitive = hkcdStaticMeshTree.Primitive;

public static class CollisionShapeBuilder
{
    // ---- Extern mesh ------------------------------------------------------------------------------------------------

    /// <summary>
    /// An extern mesh holding the triangles as they are, with a simd tree over them. Triangles have no material.
    /// </summary>
    public static hknpExternMeshShape BuildExternMeshShape(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> triangleIndices)
    {
        if (triangleIndices.Count % 3 != 0)
            throw new ArgumentException("triangleIndices must be a multiple of 3.", nameof(triangleIndices));

        int triangleCount = triangleIndices.Count / 3;

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

        return new hknpExternMeshShape
        {
            m_geometry = new hknpDefaultExternMeshShapeGeometry
            {
                m_geometry = geometry,
                m_useTriangleMaterialAsShapeTag = false
            },
            m_boundingVolumeData = BuildBoundingVolumeData(geometry),
#if HK2019
            m_isMutable = false,
#endif
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.EXTERN_MESH,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_numShapeKeyBits = HavokShapeGenerator_Shared.ShapeKeyBits(triangleCount),
            m_convexRadius = 0f,
            m_userData = 0,
            m_shapeTagCodecInfo = 0
        };
    }

    private static hknpExternMeshShapeData BuildBoundingVolumeData(hkGeometry geometry)
    {
        var data = new hknpExternMeshShapeData();
        var tree = new hkcdSimdTree { m_isCompact = false };
        int triangleCount = geometry.m_triangles.Count;

        if (triangleCount == 0)
        {
            // Still a well-formed (but empty) root node, never one with valid-looking but meaningless lanes
            var emptyRoot = new hkcdSimdTree.Node { m_isLeaf = true };
            InitializeEmptyNode(emptyRoot);
            tree.m_nodes.Add(emptyRoot);
            data.m_simdTree = tree;
            return data;
        }

        // Per-triangle AABB and centroid, to build and split the BVH
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

        var root = HavokShapeGenerator_Shared.BuildNode(Enumerable.Range(0, triangleCount).ToList(), triAabbs, centroids);

        var nodeList = new List<hkcdSimdTree.Node>();

        // The sentinel node is critical for a new collision to work
        var sentinel = new hkcdSimdTree.Node { m_isLeaf = true };
        InitializeEmptyNode(sentinel);
        nodeList.Add(sentinel); // index 0

        var queue = new Queue<HavokShapeGenerator_Shared.BvhBuildNode>();
        var slotOf = new Dictionary<HavokShapeGenerator_Shared.BvhBuildNode, int>();

        nodeList.Add(null!); // slot 1 is the root
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
                    nodeList.Add(null!);
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

    private static void SetLaneBounds(hkcdSimdTree.Node node, int lane, Vector3 min, Vector3 max)
    {
        HavokShapeGenerator_Shared.SetComponent(ref node.m_lx, lane, min.X);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hx, lane, max.X);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_ly, lane, min.Y);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hy, lane, max.Y);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_lz, lane, min.Z);
        HavokShapeGenerator_Shared.SetComponent(ref node.m_hz, lane, max.Z);
    }

    private static void InitializeEmptyNode(hkcdSimdTree.Node node)
    {
        node.m_lx = new Vector4(float.MaxValue);
        node.m_hx = new Vector4(-float.MaxValue);
        node.m_ly = new Vector4(float.MaxValue);
        node.m_hy = new Vector4(-float.MaxValue);
        node.m_lz = new Vector4(float.MaxValue);
        node.m_hz = new Vector4(-float.MaxValue);
        Array.Fill(node.m_data, 0u);
    }

    // ---- Compressed mesh --------------------------------------------------------------------------------------------

    /// <summary>
    /// A FromSoftware compressed mesh of the triangles, built by <see cref="HavokCompressedMeshBuilder" />, with each
    /// triangle's collision material (or <paramref name="defaultMaterial" />).
    /// </summary>
    public static fsnpCustomParamCompressedMeshShape BuildCompressedMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> indices,
        IReadOnlyList<uint>? triangleMaterials = null,
        uint defaultMaterial = 0,
        float convexRadius = HavokCompressedMeshBuilder.DefaultConvexRadius)
    {
        return ToShape(HavokCompressedMeshBuilder.Build(vertices, indices, triangleMaterials, defaultMaterial, convexRadius));
    }

    /// <summary>The classes of a compressed mesh build.</summary>
    public static fsnpCustomParamCompressedMeshShape ToShape(HavokCompressedMeshBuilder.Result build)
    {
        var r = build.ConvexRadius;

        var tree = new hknpCompressedMeshShapeTree
        {
            m_domain = Domain(build.DomainMin, build.DomainMax, r),
            m_numPrimitiveKeys = build.NumPrimitiveKeys,
            m_bitsPerKey = build.BitsPerKey,
            m_maxKeyValue = build.MaxKeyValue,
            m_primitiveStoresIsFlatConvex = 0,
            m_packedVertices = build.PackedVertices.ToList(),
            m_sharedVertices = build.SharedVertices.ToList(),
            m_sharedVerticesIndex = build.SharedVerticesIndex.ToList()
        };

        foreach (var n in build.TreeNodes)
        {
            var node = new hkcdCompressedAabbCodecs.Aabb5BytesCodec { m_hiData = n.HiData, m_loData = n.LoData };
            node.m_xyz[0] = n.X;
            node.m_xyz[1] = n.Y;
            node.m_xyz[2] = n.Z;
            tree.m_nodes.Add(node);
        }

        foreach (var p in build.Primitives)
        {
            var primitive = new CompressedMeshPrimitive();
            p.CopyTo(primitive.m_indices, 0);
            tree.m_primitives.Add(primitive);
        }

        foreach (var s in build.Sections)
        {
            var section = new CompressedMeshSection
            {
                m_domain = Domain(s.DomainMin, s.DomainMax, r),
                m_firstPackedVertexIndex = s.FirstPackedVertexIndex,
                m_firstSharedVertexIndex = s.FirstSharedVertexIndex,
                m_firstPrimitiveIndex = s.FirstPrimitiveIndex,
                m_firstDataRunIndex = s.FirstDataRunIndex,
                m_numPackedVertices = s.NumPackedVertices,
                m_numPrimitives = s.NumPrimitives,
                m_numDataRuns = 1,
                m_page = 0,
                m_leafIndex = s.LeafIndex,
                m_layerData = 0,
                m_flags = 0
            };
            s.CodecParms.CopyTo(section.m_codecParms, 0);

            foreach (var n in s.Nodes)
            {
                var node = new hkcdCompressedAabbCodecs.Aabb4BytesCodec { m_data = n.Data };
                node.m_xyz[0] = n.X;
                node.m_xyz[1] = n.Y;
                node.m_xyz[2] = n.Z;
                section.m_nodes.Add(node);
            }

            tree.m_sections.Add(section);

            // One run per section, covering all of its primitives
            tree.m_primitiveDataRuns.Add(new hkcdDefaultStaticMeshTree.PrimitiveDataRun
            {
                m_value = 0xFFFF,
                m_index = 0,
                m_count = s.NumPrimitives
            });
        }

        var data = new hknpCompressedMeshShapeData
        {
            m_meshTree = tree,
            m_simdTree = BuildSimdTree(build),
            m_connectivity = new hkcdStaticMeshTree.Connectivity(),
            m_hasSimdTree = true
        };

        // Interior triangles are a welding hint; none are marked
        int interiorBits = (int)build.MaxKeyValue + 1;
        var interior = new hkBitField
        {
            m_storage = new hkBitFieldStorage<List<uint>>
            {
                m_words = Enumerable.Repeat(0u, (interiorBits + 31) / 32).ToList(),
                m_numBits = interiorBits
            }
        };

        var shape = new fsnpCustomParamCompressedMeshShape
        {
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.COMPRESSED_MESH,
            m_numShapeKeyBits = (byte)build.BitsPerKey,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_convexRadius = r,
            m_userData = 0,
            m_properties = null,
            m_shapeTagCodecInfo = 0xFFFFFFFF,
#if HK2018
            m_materialTable = null,
#endif
            m_data = data,
            m_triangleIsInterior = interior,
            m_pParam = BuildParam(build)
        };

        for (int t = 0; t < build.TriangleKeys.Length; t++)
            shape.m_triangleIndexToShapeKey.Add(build.StoredKey(t));

        return shape;
    }

    private static hkAabb Domain(Vector3 min, Vector3 max, float radius) => new()
    {
        m_min = new Vector4(min, 1f - radius),
        m_max = new Vector4(max, 1f + radius)
    };

    private static hkcdSimdTree BuildSimdTree(HavokCompressedMeshBuilder.Result build)
    {
        var tree = new hkcdSimdTree { m_isCompact = true };

        foreach (var n in build.SimdNodes)
        {
            var node = new hkcdSimdTree.Node
            {
                m_isLeaf = n.IsLeaf,
                m_lx = new Vector4(n.Min[0].X, n.Min[1].X, n.Min[2].X, n.Min[3].X),
                m_hx = new Vector4(n.Max[0].X, n.Max[1].X, n.Max[2].X, n.Max[3].X),
                m_ly = new Vector4(n.Min[0].Y, n.Min[1].Y, n.Min[2].Y, n.Min[3].Y),
                m_hy = new Vector4(n.Max[0].Y, n.Max[1].Y, n.Max[2].Y, n.Max[3].Y),
                m_lz = new Vector4(n.Min[0].Z, n.Min[1].Z, n.Min[2].Z, n.Min[3].Z),
                m_hz = new Vector4(n.Max[0].Z, n.Max[1].Z, n.Max[2].Z, n.Max[3].Z)
            };
            n.Data.CopyTo(node.m_data, 0);
            tree.m_nodes.Add(node);
        }

        return tree;
    }

    private static fsnpCustomMeshParameter BuildParam(HavokCompressedMeshBuilder.Result build)
    {
        var param = new fsnpCustomMeshParameter
        {
            m_version = HavokCompressedMeshBuilder.ParamVersion,
            m_vertexDataStride = 1,
            m_triangleDataStride = 0
        };

        foreach (var t in build.TriangleParams)
        {
            param.m_triangleDataArray.Add(new fsnpCustomMeshParameter.TriangleData
            {
                m_primitiveDataIndex = t.MaterialGroupIndex,
                m_triangleDataIndex = t.TriangleIndex,
                m_vertexIndexA = t.VertexA,
                m_vertexIndexB = t.VertexB,
                m_vertexIndexC = t.VertexC
            });
        }

        foreach (var g in build.MaterialGroups)
        {
            param.m_primitiveDataArray.Add(new fsnpCustomMeshParameter.PrimitiveData
            {
                m_vertexData = Enumerable.Repeat((byte)0xFF, g.VertexCount).ToList(),
                m_materialNameData = g.MaterialId
            });
        }

        return param;
    }

    // ---- Replacing a body's shape -----------------------------------------------------------------------------------

    /// <summary>
    /// Swaps the mesh shape of the first physics system's body for another, and back: for an undoable edit. Null if the
    /// file has no such body, or its shape isn't a mesh (compressed or extern) and <paramref name="force" /> is off.
    /// </summary>
    public static (Action Apply, Action Undo)? ReplaceMeshShape(hkRootLevelContainer container, hknpShape newShape,
        int bodyIndex = 0, bool force = false)
    {
        // Asset collision can hold an hkxScene alongside the physics scene
        var scene = container.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().FirstOrDefault();

        if (scene == null || scene.m_systemDatas.Count == 0 || scene.m_systemDatas[0] is not { } systemData)
            return null;

        var bodies = systemData.m_bodyCinfos;

        if (bodyIndex < 0 || bodyIndex >= bodies.Count)
            return null;

        var body = bodies[bodyIndex];

        if (!force && body.m_shape is not (fsnpCustomParamCompressedMeshShape or hknpCompressedMeshShape or hknpExternMeshShape))
            return null;

        var oldShape = body.m_shape;

        return (() => body.m_shape = newShape, () => body.m_shape = oldShape);
    }
}
