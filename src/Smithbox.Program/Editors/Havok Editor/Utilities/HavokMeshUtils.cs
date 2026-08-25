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

    private static byte ShapeKeyBits(int primitiveCount)
    {
        if (primitiveCount <= 1)
        {
            return 1;
        }

        return (byte)Math.Ceiling(Math.Log2(primitiveCount));
    }

    // ============================================================================
    // 3) Replacing the shape on an existing collision's hkRootLevelContainer
    // ============================================================================
    //
    // Mirrors the navigation HKLib_Helper.LoadCollisionMesh does to reach
    // bodyInfo.m_shape (m_namedVariants[0].m_variant -> hknpPhysicsSceneData ->
    // m_systemDatas[0].m_bodyCinfos), then swaps whatever mesh shape is sitting
    // there (fsnpCustomParamCompressedMeshShape, hknpCompressedMeshShape, or an
    // existing hknpExternMeshShape) out for a freshly generated hknpExternMeshShape.

    /// <summary>
    /// Replaces the shape on a single body (by index, default the first/only body)
    /// of an existing collision's root container with <paramref name="newShape"/>.
    /// Only replaces the shape if it is currently a recognized mesh shape type
    /// (fsnpCustomParamCompressedMeshShape, hknpCompressedMeshShape, or
    /// hknpExternMeshShape) so this won't accidentally clobber some other shape
    /// type (e.g. a convex hull) sitting in the same slot. Pass <paramref name="force"/>
    /// = true to replace regardless of the current shape type.
    /// </summary>
    /// <returns>true if a shape was replaced.</returns>
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

    /// <summary>
    /// Same as <see cref="ReplaceExternMeshShape(hkRootLevelContainer, hknpExternMeshShape, int, bool)"/>
    /// but replaces every body whose shape is currently a recognized mesh shape
    /// type. <paramref name="shapeFactory"/> is called once per replaced body
    /// (its body index) so you can give each submesh its own generated shape, or
    /// just ignore the index and return the same shape instance/copy each time.
    /// </summary>
    /// <returns>the number of bodies whose shape was replaced.</returns>
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