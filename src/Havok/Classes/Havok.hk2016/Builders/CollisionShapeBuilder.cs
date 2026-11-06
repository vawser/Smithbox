// The Collision Generator's shapes for Sekiro's Havok 2016 classes. The algorithm is the shared
// HavokCompressedMeshBuilder (Havok.Shared/Builders); this fills the 2016 classes from its build.
//
// 2016 sits between DS3 and ER (checked against SDT map collision with Tools/CollisionSurvey):
// - The mesh tree is DS3's: sections pack their counts with their start indices (count | first << 8).
// - The shape header is 2018's: m_type and the dispatch type enum, a material table (null), no edge welding map, no
//   m_quadIsFlat. The flags are IS_COMPOSITE_SHAPE alone and the convex radius 0.01.
// - The SIMD tree is a real tree, unlike DS3's stub, but without 2018's leaf and compact flags: each lane holds
//   value << 1 | isLeaf, the value being a child node index or a primitive's shape key. Node 0 is an empty sentinel and
//   node 1 the root.
// (Documentation/Claude/Collision Generator All Versions.md)
namespace Havok.hk2016.Builders;

using System.Numerics;
using Havok.Builders;
using Section = hkcdStaticMeshTreeBase.Section;
using Primitive = hkcdStaticMeshTreeBase.Primitive;

public static class CollisionShapeBuilder
{
    public const float DefaultConvexRadius = 0.01f;

    /// <summary>
    /// A FromSoftware compressed mesh of the triangles, built by <see cref="HavokCompressedMeshBuilder" />, with each
    /// triangle's collision material (or <paramref name="defaultMaterial" />).
    /// </summary>
    public static fsnpCustomParamCompressedMeshShape BuildCompressedMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> indices,
        IReadOnlyList<uint>? triangleMaterials = null,
        uint defaultMaterial = 0,
        float convexRadius = DefaultConvexRadius)
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
            var node = new hkcdStaticTree.Codec3Axis5 { m_hiData = n.HiData, m_loData = n.LoData };
            node.m_xyz[0] = n.X;
            node.m_xyz[1] = n.Y;
            node.m_xyz[2] = n.Z;
            tree.m_nodes.Add(node);
        }

        foreach (var p in build.Primitives)
        {
            var primitive = new Primitive();
            p.CopyTo(primitive.m_indices, 0);
            tree.m_primitives.Add(primitive);
        }

        for (int i = 0; i < build.Sections.Count; i++)
        {
            var s = build.Sections[i];
            uint nextShared = i + 1 < build.Sections.Count
                ? build.Sections[i + 1].FirstSharedVertexIndex
                : (uint)build.SharedVerticesIndex.Count;

            var section = new Section
            {
                m_domain = Domain(s.DomainMin, s.DomainMax, r),
                m_firstPackedVertex = s.FirstPackedVertexIndex,
                m_sharedVertices = new Section.SharedVertices { m_data = s.NumPackedVertices | (s.FirstSharedVertexIndex << 8) },
                m_primitives = new Section.Primitives { m_data = s.NumPrimitives | (s.FirstPrimitiveIndex << 8) },
                m_dataRuns = new Section.DataRuns { m_data = 1u | (s.FirstDataRunIndex << 8) },
                m_numPackedVertices = s.NumPackedVertices,
                m_numSharedIndices = (byte)(nextShared - s.FirstSharedVertexIndex),
                m_leafIndex = s.LeafIndex,
                m_page = 0,
                m_flags = 0,
                m_layerData = 0,
                m_unusedData = 0
            };
            s.CodecParms.CopyTo(section.m_codecParms, 0);

            foreach (var n in s.Nodes)
            {
                var node = new hkcdStaticTree.Codec3Axis4 { m_data = n.Data };
                node.m_xyz[0] = n.X;
                node.m_xyz[1] = n.Y;
                node.m_xyz[2] = n.Z;
                section.m_nodes.Add(node);
            }

            tree.m_sections.Add(section);

            // One run per section, covering all of its primitives
            tree.m_primitiveDataRuns.Add(new hknpCompressedMeshShapeTreeDataRun
            {
                m_value = new hknpCompressedMeshShapeTreeDataRunData { m_data = 0xFFFF },
                m_index = 0,
                m_count = s.NumPrimitives
            });
        }

        var data = new hknpCompressedMeshShapeData
        {
            m_meshTree = tree,
            m_simdTree = BuildSimdTree(build),
            m_connectivity = new hkcdStaticMeshTreeBase.Connectivity()
        };

        // Interior triangles are a welding hint; none are marked
        int interiorBits = (int)build.MaxKeyValue + 1;

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
            m_materialTable = null,
            m_data = data,
            m_triangleIsInterior = new hkBitField
            {
                m_storage = new hkBitFieldStorage<List<uint>>
                {
                    m_words = Enumerable.Repeat(0u, (interiorBits + 31) / 32).ToList(),
                    m_numBits = interiorBits
                }
            },
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

    /// <summary>
    /// The build's SIMD tree in the 2016 encoding: the same nodes, with each lane's leaf flag in its lowest data bit.
    /// The build makes no quads, so a triangle's key is its primitive's.
    /// </summary>
    private static hkcdSimdTree BuildSimdTree(HavokCompressedMeshBuilder.Result build)
    {
        var tree = new hkcdSimdTree();

        foreach (var n in build.SimdNodes)
        {
            var node = new hkcdSimdTree.Node
            {
                m_lx = new Vector4(n.Min[0].X, n.Min[1].X, n.Min[2].X, n.Min[3].X),
                m_hx = new Vector4(n.Max[0].X, n.Max[1].X, n.Max[2].X, n.Max[3].X),
                m_ly = new Vector4(n.Min[0].Y, n.Min[1].Y, n.Min[2].Y, n.Min[3].Y),
                m_hy = new Vector4(n.Max[0].Y, n.Max[1].Y, n.Max[2].Y, n.Max[3].Y),
                m_lz = new Vector4(n.Min[0].Z, n.Min[1].Z, n.Min[2].Z, n.Min[3].Z),
                m_hz = new Vector4(n.Max[0].Z, n.Max[1].Z, n.Max[2].Z, n.Max[3].Z)
            };

            for (int lane = 0; lane < 4; lane++)
            {
                // Empty lanes (inverted bounds) keep 0, as the sentinel's do
                bool empty = n.Min[lane].X > n.Max[lane].X;
                node.m_data[lane] = empty ? 0u : (n.Data[lane] << 1) | (n.IsLeaf ? 1u : 0u);
            }

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

    /// <summary>
    /// Swaps the mesh shape of the first physics system's body for another, and back: for an undoable edit. The system's
    /// referenced objects list the shape too, so the swap is made there as well. Null if the file has no such body, or
    /// its shape isn't a mesh (compressed or extern) and <paramref name="force" /> is off.
    /// </summary>
    public static (Action Apply, Action Undo)? ReplaceMeshShape(hkRootLevelContainer container, hknpShape newShape,
        int bodyIndex = 0, bool force = false)
    {
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
        var referenced = systemData.m_referencedObjects;

        return (
            () =>
            {
                body.m_shape = newShape;
                int i = referenced.IndexOf(oldShape);
                if (i >= 0) referenced[i] = newShape;
            },
            () =>
            {
                body.m_shape = oldShape;
                int i = referenced.IndexOf(newShape);
                if (i >= 0) referenced[i] = oldShape;
            });
    }
}
