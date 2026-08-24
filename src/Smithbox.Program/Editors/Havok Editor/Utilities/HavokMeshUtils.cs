using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Builds Havok Physics (hknp) collision shapes from arbitrary point/triangle data.
/// This is the inverse of HKLib_Helper: instead of decompressing a shape into a
/// renderable mesh, it takes a renderable mesh (points representing a plane, box,
/// or any other geometry) and produces a Havok shape object.
///
/// Two shape types are supported, mirroring the two branches HKLib_Helper reads from:
///
///   1) hknpExternMeshShape       - uncompressed, explicit vertex/triangle list.
///                                  Simple, robust, no acceleration structure required.
///                                  RECOMMENDED for authored/simple shapes (planes, boxes,
///                                  ramps, small trigger volumes, etc).
///
///   2) fsnpCustomParamCompressedMeshShape - the quantized "section/primitive" format
///                                  FromSoftware uses for most map collision. See the
///                                  big warning on BuildCompressedMeshShape below before
///                                  using this one.
/// </summary>
public static class HKLib_MeshBuilder
{
    // ============================================================================
    // 1) hknpExternMeshShape - reliable, use this unless you specifically need the
    //    compressed format.
    // ============================================================================

    /// <summary>
    /// Builds an hknpExternMeshShape from a flat vertex list and a triangle index list
    /// (3 indices per triangle, matching the layout produced by HKLib_Helper.RenderMesh).
    /// </summary>
    public static hknpExternMeshShape BuildExternMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> triangleIndices,
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
                m_material = 0
            });
        }

        var geometryWrapper = new hknpDefaultExternMeshShapeGeometry
        {
            m_geometry = geometry,
            m_useTriangleMaterialAsShapeTag = useTriangleMaterialAsShapeTag
        };

        int triangleCount = geometry.m_triangles.Count;

        var shape = new hknpExternMeshShape
        {
            m_geometry = geometryWrapper,
            m_boundingVolumeData = null, // left null; the runtime will rebuild it on load if required
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

    // ============================================================================
    // 2) fsnpCustomParamCompressedMeshShape
    // ============================================================================
    //
    // IMPORTANT - read before using this path:
    //
    // The compressed mesh format stores triangle geometry as quantized
    // "shared"/"packed" vertices inside per-section Primitive records
    // (this is exactly what HKLib_Helper.ProcessColData decodes). That part
    // of the format is well understood and BuildCompressedMeshShapeTree below
    // reproduces it faithfully - it is the exact mathematical inverse of
    // DecompressSharedVertex/DecompressPackedVertex in HKLib_Helper.
    //
    // However, hknpCompressedMeshShapeData ALSO carries acceleration structures
    // used by Havok's broadphase to avoid scanning every triangle:
    //   - each Section (hkcdStaticTree.Section) is itself a compressed AABB tree
    //     (Aabb4BytesCodec nodes) over its own primitives
    //   - the mesh tree itself is a compressed AABB tree (Aabb5BytesCodec nodes)
    //     over its sections
    //   - hknpCompressedMeshShapeData.m_simdTree is a second, SIMD-friendly
    //     acceleration structure over the same data
    //
    // HKLib is a tagfile reader/writer only - it does not implement Havok's
    // internal bit-packing for the compressed AABB codecs, and that packing
    // isn't public. HKLib_Helper never has to touch it either, because reading
    // triangle geometry only requires walking m_sections/m_primitives/vertex
    // tables linearly, which is what the loader above already does.
    //
    // BuildCompressedMeshShape below produces fully correct section/primitive/
    // vertex data, but leaves the AABB tree nodes and the simd tree EMPTY. This
    // means the resulting file may load and even render correctly in a viewer
    // that walks primitives linearly (like this repo's own loader), but is not
    // guaranteed to behave correctly for actual in-game physics queries, since
    // the runtime broadphase may expect valid tree nodes. Treat this as a
    // starting point you must verify in-game, not a drop-in replacement.
    //
    // If you don't specifically need the compressed format (e.g. you're not
    // trying to match an existing map's collision authoring convention),
    // prefer BuildExternMeshShape above - it has no acceleration structure to
    // get wrong.
    // ============================================================================

    private const int MaxLocalVerticesPerSection = 256; // primitive indices are bytes
    // Note: HKLib_Helper.ProcessColData skips any primitive whose 4 indices are
    // 0xDE 0xAD 0xDE 0xAD (a "dead" sentinel). We never emit that byte pattern here.

    public static fsnpCustomParamCompressedMeshShape BuildCompressedMeshShape(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> triangleIndices)
    {
        var meshTree = BuildCompressedMeshShapeTree(vertices, triangleIndices);

        var data = new hknpCompressedMeshShapeData
        {
            m_meshTree = meshTree,
            m_simdTree = new hkcdSimdTree(), // left empty, see warning above
            m_connectivity = new HKLib.hk2018.hkcdStaticMeshTree.Connectivity(),
            m_hasSimdTree = false
        };

        int triangleCount = triangleIndices.Count / 3;

        var shape = new fsnpCustomParamCompressedMeshShape
        {
            m_data = data,
            m_triangleIsInterior = new hkBitField(),
            m_externShapes = new List<hknpShapeInstance>(),
            m_pParam = null,
            m_triangleIndexToShapeKey = new List<uint>(),
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.COMPRESSED_MESH,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_numShapeKeyBits = ShapeKeyBits(triangleCount),
            m_convexRadius = 0f,
            m_userData = 0,
            m_shapeTagCodecInfo = 0
        };

        return shape;
    }

    /// <summary>
    /// Builds just the mesh tree (section/primitive/vertex table) portion of the
    /// compressed format. Exposed separately in case you want to slot this into
    /// an existing hknpCompressedMeshShapeData (e.g. one copied from a template
    /// file so its m_simdTree / acceleration nodes stay intact) rather than a
    /// freshly built shape.
    /// </summary>
    public static hknpCompressedMeshShapeTree BuildCompressedMeshShapeTree(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> triangleIndices)
    {
        if (triangleIndices.Count % 3 != 0)
        {
            throw new ArgumentException("triangleIndices must be a multiple of 3.", nameof(triangleIndices));
        }

        // Global domain used to quantize every "shared" vertex (matches
        // coldata.m_meshTree.m_domain in HKLib_Helper.ProcessColData).
        Vector3 min = vertices[0];
        Vector3 max = vertices[0];
        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        // Guard against a degenerate (flat) domain on any axis - a zero range
        // would divide by zero during quantization.
        const float epsilon = 0.01f;
        if (max.X - min.X < epsilon) max.X = min.X + epsilon;
        if (max.Y - min.Y < epsilon) max.Y = min.Y + epsilon;
        if (max.Z - min.Z < epsilon) max.Z = min.Z + epsilon;

        var tree = new hknpCompressedMeshShapeTree
        {
            m_domain = new hkAabb
            {
                m_min = new Vector4(min, 0f),
                m_max = new Vector4(max, 0f)
            },
            m_numPrimitiveKeys = triangleIndices.Count / 3,
            m_bitsPerKey = ShapeKeyBits(triangleIndices.Count / 3),
            m_maxKeyValue = (uint)Math.Max(0, triangleIndices.Count / 3 - 1),
            m_primitiveStoresIsFlatConvex = 0
        };

        // Chunk triangles into sections of at most MaxLocalVerticesPerSection
        // distinct vertices each, since a Primitive's indices are single bytes.
        int triCount = triangleIndices.Count / 3;
        int triIndex = 0;

        while (triIndex < triCount)
        {
            var localVertexOf = new Dictionary<int, byte>(); // source vertex index -> local (0-255) index
            var sectionSharedVertices = new List<ulong>();   // compressed shared vertices for this section
            var sectionPrimitives = new List<Primitive>();

            while (triIndex < triCount)
            {
                int a = triangleIndices[triIndex * 3 + 0];
                int b = triangleIndices[triIndex * 3 + 1];
                int c = triangleIndices[triIndex * 3 + 2];

                int newVertsNeeded =
                    (localVertexOf.ContainsKey(a) ? 0 : 1) +
                    (localVertexOf.ContainsKey(b) ? 0 : 1) +
                    (localVertexOf.ContainsKey(c) ? 0 : 1);

                if (localVertexOf.Count + newVertsNeeded > MaxLocalVerticesPerSection)
                {
                    break; // start a new section
                }

                byte la = GetOrAddLocalVertex(a, vertices, min, max, localVertexOf, sectionSharedVertices);
                byte lb = GetOrAddLocalVertex(b, vertices, min, max, localVertexOf, sectionSharedVertices);
                byte lc = GetOrAddLocalVertex(c, vertices, min, max, localVertexOf, sectionSharedVertices);

                sectionPrimitives.Add(new Primitive
                {
                    // indices[2] == indices[3] marks this as a triangle, not a quad
                    // (see HKLib_Helper.ProcessColData).
                    m_indices = { [0] = la, [1] = lb, [2] = lc, [3] = lc }
                });

                triIndex++;
            }

            var section = new Section
            {
                m_firstPackedVertexIndex = 0,
                m_firstSharedVertexIndex = (uint)tree.m_sharedVertices.Count,
                m_firstPrimitiveIndex = (uint)tree.m_primitives.Count,
                m_firstDataRunIndex = 0,
                m_numPackedVertices = 0, // we only use shared vertices - see class remarks
                m_numPrimitives = (byte)sectionPrimitives.Count,
                m_numDataRuns = 0,
                m_page = 0,
                m_leafIndex = 0,
                m_layerData = 0,
                m_flags = 0
                // m_codecParms left at default: unused since m_numPackedVertices == 0
            };

            // Local index -> global m_sharedVertices slot, referenced indirectly through
            // m_sharedVerticesIndex (this indirection table is what ProcessColData walks
            // via coldata.m_meshTree.m_sharedVerticesIndex[localIndex + firstSharedVertexIndex]).
            foreach (ulong sv in sectionSharedVertices)
            {
                tree.m_sharedVerticesIndex.Add((ushort)tree.m_sharedVertices.Count);
                tree.m_sharedVertices.Add(sv);
            }

            tree.m_primitives.AddRange(sectionPrimitives);
            tree.m_sections.Add(section);
        }

        return tree;
    }

    private static byte GetOrAddLocalVertex(
        int sourceIndex,
        IReadOnlyList<Vector3> vertices,
        Vector3 domainMin,
        Vector3 domainMax,
        Dictionary<int, byte> localVertexOf,
        List<ulong> sectionSharedVertices)
    {
        if (localVertexOf.TryGetValue(sourceIndex, out byte existing))
        {
            return existing;
        }

        ulong compressed = CompressSharedVertex(vertices[sourceIndex], domainMin, domainMax);
        byte local = (byte)sectionSharedVertices.Count;
        sectionSharedVertices.Add(compressed);
        localVertexOf[sourceIndex] = local;
        return local;
    }

    /// <summary>
    /// Exact inverse of HKLib_Helper.DecompressSharedVertex: packs a position into a
    /// 21/21/22-bit (X/Y/Z) fixed-point value within [domainMin, domainMax].
    /// </summary>
    private static ulong CompressSharedVertex(Vector3 vert, Vector3 bbMin, Vector3 bbMax)
    {
        const ulong xyMask = 0x1FFFFF;   // 21 bits, matches 2097151
        const ulong zMask = 0x3FFFFF;    // 22 bits, matches 4194303

        double scaleX = (bbMax.X - bbMin.X) / 2097151.0;
        double scaleY = (bbMax.Y - bbMin.Y) / 2097151.0;
        double scaleZ = (bbMax.Z - bbMin.Z) / 4194303.0;

        ulong rawX = QuantizeAxis((vert.X - bbMin.X) / scaleX, xyMask);
        ulong rawY = QuantizeAxis((vert.Y - bbMin.Y) / scaleY, xyMask);
        ulong rawZ = QuantizeAxis((vert.Z - bbMin.Z) / scaleZ, zMask);

        return rawX | (rawY << 21) | (rawZ << 42);
    }

    private static ulong QuantizeAxis(double value, ulong mask)
    {
        long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
        if (rounded < 0) rounded = 0;
        if ((ulong)rounded > mask) rounded = (long)mask;
        return (ulong)rounded;
    }

    private static byte ShapeKeyBits(int primitiveCount)
    {
        if (primitiveCount <= 1)
        {
            return 1;
        }

        return (byte)Math.Ceiling(Math.Log2(primitiveCount));
    }
}