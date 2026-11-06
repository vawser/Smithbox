using Havok.Builders;
using Havok.hk2014;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Fills the Havok.hk2014 (DS3) compressed mesh classes from a <see cref="HavokCompressedMeshBuilder"/> build.
/// The sections, trees, keys and FromSoftware parameters are the same as in the HKX3 games; what differs, as the DS3
/// map collision holds it (checked with Tools/CollisionSurvey):
/// <list type="bullet">
/// <item>A section packs its counts with their start indices: m_sharedVertices holds the packed vertex count and
/// the first shared vertex index (count | first &lt;&lt; 8), m_primitives and m_dataRuns hold count | first &lt;&lt; 8.</item>
/// <item>The shape flags are IS_COMPOSITE_SHAPE | NO_GET_SHAPE_KEYS_ON_SPU, the convex radius 0.01, and the edge
/// welding map is empty.</item>
/// <item>m_quadIsFlat has a bit per primitive key (key &gt;&gt; 1), m_triangleIsInterior a bit per shape key.</item>
/// <item>The SIMD tree is an unused stub of two empty nodes.</item>
/// </list>
/// Kept apart from the rest of the shape generator so Tools/CollisionSurvey can round-trip it against game files.
/// </summary>
public static class HavokCompressedMeshGenerator_hk2014
{
    public const float DefaultConvexRadius = 0.01f;

    private const hknpShape.FlagsEnum ShapeFlags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE | hknpShape.FlagsEnum.NO_GET_SHAPE_KEYS_ON_SPU;

    public static fsnpCustomParamCompressedMeshShape BuildCompressedMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> indices,
        IReadOnlyList<uint> triangleMaterials = null,
        uint defaultMaterial = 0,
        float convexRadius = DefaultConvexRadius)
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
            m_packedVertices = build.PackedVertices.ToList(),
            m_sharedVertices = build.SharedVertices.ToList(),
            m_sharedVerticesIndex = build.SharedVerticesIndex.ToList(),
            m_nodes = new List<hkcdStaticTreeCodec3Axis5>(),
            m_primitives = new List<hkcdStaticMeshTreeBasePrimitive>(),
            m_sections = new List<hkcdStaticMeshTreeBaseSection>(),
            m_primitiveDataRuns = new List<hknpCompressedMeshShapeTreeDataRun>()
        };

        foreach (var n in build.TreeNodes)
        {
            tree.m_nodes.Add(new hkcdStaticTreeCodec3Axis5
            {
                m_xyz = { [0] = n.X, [1] = n.Y, [2] = n.Z },
                m_hiData = n.HiData,
                m_loData = n.LoData
            });
        }

        foreach (var p in build.Primitives)
        {
            tree.m_primitives.Add(new hkcdStaticMeshTreeBasePrimitive
            {
                m_indices = { [0] = p[0], [1] = p[1], [2] = p[2], [3] = p[3] }
            });
        }

        for (int i = 0; i < build.Sections.Count; i++)
        {
            var s = build.Sections[i];
            int numShared = (int)(i + 1 < build.Sections.Count
                ? build.Sections[i + 1].FirstSharedVertexIndex - s.FirstSharedVertexIndex
                : (uint)build.SharedVerticesIndex.Count - s.FirstSharedVertexIndex);

            var section = new hkcdStaticMeshTreeBaseSection
            {
                m_domain = Domain(s.DomainMin, s.DomainMax, r),
                m_codecParms = { [0] = s.CodecParms[0], [1] = s.CodecParms[1], [2] = s.CodecParms[2], [3] = s.CodecParms[3], [4] = s.CodecParms[4], [5] = s.CodecParms[5] },
                m_firstPackedVertex = s.FirstPackedVertexIndex,
                m_sharedVertices = new hkcdStaticMeshTreeBaseSectionSharedVertices
                {
                    m_data = s.NumPackedVertices | (s.FirstSharedVertexIndex << 8)
                },
                m_primitives = new hkcdStaticMeshTreeBaseSectionPrimitives
                {
                    m_data = s.NumPrimitives | (s.FirstPrimitiveIndex << 8)
                },
                m_dataRuns = new hkcdStaticMeshTreeBaseSectionDataRuns
                {
                    m_data = 1u | (s.FirstDataRunIndex << 8)
                },
                m_numPackedVertices = s.NumPackedVertices,
                m_numSharedIndices = (byte)numShared,
                m_leafIndex = s.LeafIndex,
                m_page = 0,
                m_flags = 0,
                m_layerData = 0,
                m_unusedData = 0,
                m_nodes = new List<hkcdStaticTreeCodec3Axis4>()
            };

            foreach (var n in s.Nodes)
            {
                section.m_nodes.Add(new hkcdStaticTreeCodec3Axis4
                {
                    m_xyz = { [0] = n.X, [1] = n.Y, [2] = n.Z },
                    m_data = n.Data
                });
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
            m_simdTree = BuildSimdStub()
        };

        var shape = new fsnpCustomParamCompressedMeshShape
        {
            m_flags = ShapeFlags,
            m_numShapeKeyBits = (byte)build.BitsPerKey,
            m_dispatchType = (byte)hkcdShapeDispatchType.ShapeDispatchTypeEnum.COMPOSITE,
            m_convexRadius = r,
            m_userData = 0,
            m_properties = null,
            m_shapeTagCodecInfo = 0xFFFFFFFF,
            m_edgeWeldingMap = new hknpSparseCompactMapunsignedshort
            {
                m_secondaryKeyMask = 0xFFFFFFFF,
                m_sencondaryKeyBits = 0,
                m_primaryKeyToIndex = new List<ushort>(),
                m_valueAndSecondaryKeys = new List<ushort>()
            },
            m_data = data,
            // Neither is set: there are no quads, and interior triangles are only a welding hint
            m_quadIsFlat = BitField((int)(build.MaxKeyValue >> 1) + 1),
            m_triangleIsInterior = BitField((int)build.MaxKeyValue + 1),
            m_pParam = BuildParam(build),
            m_triangleIndexToShapeKey = new List<uint>()
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

    private static hkBitField BitField(int bits)
    {
        return new hkBitField
        {
            m_storage = new hkBitFieldStoragehkArrayunsignedinthkContainerHeapAllocator
            {
                m_words = Enumerable.Repeat(0u, (bits + 31) / 32).ToList(),
                m_numBits = bits
            }
        };
    }

    /// <summary>
    /// The DS3 files hold two empty SIMD tree nodes; the compressed mesh tree is what the game queries
    /// </summary>
    private static hkcdSimdTree BuildSimdStub()
    {
        var tree = new hkcdSimdTree { m_nodes = new List<hkcdSimdTree.Node>() };

        for (int i = 0; i < 2; i++)
        {
            tree.m_nodes.Add(new hkcdSimdTree.Node
            {
                m_lx = new Vector4(float.MaxValue),
                m_ly = new Vector4(float.MaxValue),
                m_lz = new Vector4(float.MaxValue),
                m_hx = new Vector4(-float.MaxValue),
                m_hy = new Vector4(-float.MaxValue),
                m_hz = new Vector4(-float.MaxValue)
            });
        }

        return tree;
    }

    private static fsnpCustomMeshParameter BuildParam(HavokCompressedMeshBuilder.Result build)
    {
        var param = new fsnpCustomMeshParameter
        {
            m_version = HavokCompressedMeshBuilder.ParamVersion,
            m_vertexDataStride = 1,
            m_triangleDataStride = 0,
            m_triangleDataArray = new List<fsnpCustomMeshParameter.TriangleData>(),
            m_primitiveDataArray = new List<fsnpCustomMeshParameter.PrimitiveData>()
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
                m_triangleData = new List<byte>(),
                m_primitiveData = new List<byte>(),
                m_materialNameData = g.MaterialId
            });
        }

        return param;
    }
}
