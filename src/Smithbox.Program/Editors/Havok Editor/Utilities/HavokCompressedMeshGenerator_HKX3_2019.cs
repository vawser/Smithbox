using Havok.hk2019;
using Havok.hk2019.hkcdCompressedAabbCodecs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CompressedMeshSection = Havok.hk2019.hkcdStaticMeshTree.Section;
using CompressedMeshPrimitive = Havok.hk2019.hkcdStaticMeshTree.Primitive;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Fills the Havok 2019 (AC6) compressed mesh classes from a <see cref="HavokCompressedMeshBuilder"/> build.
/// The same as <see cref="HavokCompressedMeshGenerator_HKX3"/>, as the 2019 classes match the 2018 ones.
/// </summary>
public static class HavokCompressedMeshGenerator_HKX3_2019
{
    public static fsnpCustomParamCompressedMeshShape BuildCompressedMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> indices,
        IReadOnlyList<uint> triangleMaterials = null,
        uint defaultMaterial = 0,
        float convexRadius = HavokCompressedMeshBuilder.DefaultConvexRadius)
    {
        var build = HavokCompressedMeshBuilder.Build(vertices, indices, triangleMaterials, defaultMaterial, convexRadius);
        return ToShape(build);
    }

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
            var node = new Aabb5BytesCodec { m_hiData = n.HiData, m_loData = n.LoData };
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
                var node = new Aabb4BytesCodec { m_data = n.Data };
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
            m_connectivity = new Havok.hk2019.hkcdStaticMeshTree.Connectivity(),
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
            m_data = data,
            m_triangleIsInterior = interior,
            m_pParam = BuildParam(build)
        };

        for (int t = 0; t < build.TriangleKeys.Length; t++)
            shape.m_triangleIndexToShapeKey.Add(build.StoredKey(t));

        return shape;
    }

    private static hkAabb Domain(Vector3 min, Vector3 max, float radius)
    {
        return new hkAabb
        {
            m_min = new Vector4(min, 1f - radius),
            m_max = new Vector4(max, 1f + radius)
        };
    }

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
}
