using HKLib.hk2019;
using HKLib.hk2019.hkcdStaticMeshTree;
using Microsoft.Extensions.Logging;
using StudioCore.Logger;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Builds the collision meshes of Havok 2019 (AC6) collision. Most AC6 collision is made of compound shapes, which
/// place mesh shapes by their instance transforms.
/// </summary>
public static class HKLib2019_Helper
{
    public static bool LoadCollisionMesh(HavokCollisionResource resource, AccessLevel al)
    {
        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Bounds = new BoundingBox();
            var submeshes = new List<CollisionSubmesh>();

            if (resource.AC6_HKX.m_namedVariants.Count == 0)
                return false;

            var scene = resource.AC6_HKX.m_namedVariants
                .Select(v => v.m_variant)
                .OfType<hknpPhysicsSceneData>()
                .FirstOrDefault();

            if (scene != null)
            {
                foreach (hknpBodyCinfo bodyInfo in scene.m_systemDatas.Where(s => s != null).SelectMany(s => s.m_bodyCinfos))
                {
                    AddBody(resource, submeshes, bodyInfo);
                }
            }

            resource.GPUMeshes = submeshes.ToArray();
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.AC6_HKX = null;
        }

        return true;
    }

    /// <summary>
    /// Each body becomes one submesh. Compound shapes can hold thousands of instances, so they are merged rather
    /// than given a submesh each.
    /// </summary>
    private static void AddBody(HavokCollisionResource resource, List<CollisionSubmesh> submeshes, hknpBodyCinfo body)
    {
        try
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            CollectShape(body.m_shape, GetBodyTransform(body), vertices, indices);

            if (indices.Count == 0)
                return;

            var mesh = new CollisionSubmesh();

            HKLib_Helper.RenderMesh(mesh, vertices, indices, resource.Color);

            resource.Bounds = submeshes.Count == 0
                ? mesh.Bounds
                : BoundingBox.Combine(resource.Bounds, mesh.Bounds);

            submeshes.Add(mesh);
        }
        catch (Exception e)
        {
            Smithbox.Log(typeof(HKLib2019_Helper), $"Failed to load HKLIB ({body.m_shape?.GetType().Name}).",
                LogLevel.Error, LogPriority.High, e);
        }
    }

    /// <summary>
    /// Appends the triangles of a shape, in world space, to the vertices and indices.
    /// </summary>
    private static void CollectShape(hknpShape shape, Matrix4x4 transform, List<Vector3> vertices, List<int> indices)
    {
        List<Vector3> shapeVertices;
        List<int> shapeIndices;

        switch (shape)
        {
            case hknpCompoundShape compound:
                // Free list elements without a shape are unused slots
                foreach (var instance in compound.m_instances.m_elements.Where(e => e.m_shape != null))
                {
                    CollectShape(instance.m_shape, GetInstanceTransform(instance) * transform, vertices, indices);
                }
                return;
            case hknpCompressedMeshShape { m_data: hknpCompressedMeshShapeData data }:
                (shapeVertices, shapeIndices) = ProcessCompressedMesh(data);
                break;
            case hknpExternMeshShape { m_geometry: hknpDefaultExternMeshShapeGeometry { m_geometry: { } geometry } }:
                shapeVertices = geometry.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                shapeIndices = geometry.m_triangles.SelectMany(t => new[] { t.m_a, t.m_b, t.m_c }).ToList();
                break;
            case hknpConvexShape convex:
                (shapeVertices, shapeIndices) = ProcessConvexHull(convex.m_hull);
                break;
            default:
                return;
        }

        int first = vertices.Count;
        vertices.AddRange(shapeVertices.Select(v => Vector3.Transform(v, transform)));
        indices.AddRange(shapeIndices.Select(i => first + i));
    }

    private static Matrix4x4 GetBodyTransform(hknpBodyCinfo body)
    {
        var orientation = body.m_orientation;

        // Bodies without a rotation leave it zeroed
        if (orientation.LengthSquared() < 1e-6f)
            orientation = Quaternion.Identity;

        return Matrix4x4.CreateFromQuaternion(orientation) *
               Matrix4x4.CreateTranslation(body.m_position.X, body.m_position.Y, body.m_position.Z);
    }

    private static Matrix4x4 GetInstanceTransform(hknpShapeInstance instance)
    {
        var scale = new Vector3(instance.m_scale.X, instance.m_scale.Y, instance.m_scale.Z);

        if (scale == Vector3.Zero)
            scale = Vector3.One;

        return Matrix4x4.CreateScale(scale) * instance.m_transform;
    }

    /// <summary>
    /// Decodes the primitives of a compressed mesh. Each primitive is a triangle, or a quad when its last two
    /// indices differ. Indices below the section's packed vertex count refer to its packed vertices, the rest to
    /// the shared vertices.
    /// </summary>
    private static (List<Vector3>, List<int>) ProcessCompressedMesh(hknpCompressedMeshShapeData data)
    {
        var tree = data.m_meshTree;
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        // Some meshes hold primitives and shared vertex indices but no vertices, so there is nothing to draw
        if (tree.m_packedVertices.Count == 0 && tree.m_sharedVertices.Count == 0)
            return (vertices, indices);

        foreach (Section section in tree.m_sections)
        {
            var offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
            var scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);

            Vector3 GetVertex(byte index)
            {
                if (index < section.m_numPackedVertices)
                {
                    return data.DecompressPackedVertex(
                        tree.m_packedVertices[(int)(index + section.m_firstPackedVertexIndex)], scale, offset);
                }

                var sharedIndex = tree.m_sharedVerticesIndex[(int)(index + section.m_firstSharedVertexIndex - section.m_numPackedVertices)];
                return data.DecompressSharedVertex(tree.m_sharedVertices[sharedIndex], tree.m_domain.m_min, tree.m_domain.m_max);
            }

            for (int i = 0; i < section.m_numPrimitives; i++)
            {
                byte[] primitive = tree.m_primitives[(int)(i + section.m_firstPrimitiveIndex)].m_indices;

                // Marks a primitive which isn't stored as vertex indices
                if (primitive[0] == 0xDE && primitive[1] == 0xAD && primitive[2] == 0xDE && primitive[3] == 0xAD)
                    continue;

                int first = vertices.Count;
                vertices.Add(GetVertex(primitive[0]));
                vertices.Add(GetVertex(primitive[1]));
                vertices.Add(GetVertex(primitive[2]));
                indices.AddRange(new[] { first, first + 1, first + 2 });

                if (primitive[2] != primitive[3])
                {
                    vertices.Add(GetVertex(primitive[3]));
                    indices.AddRange(new[] { first, first + 2, first + 3 });
                }
            }
        }

        return (vertices, indices);
    }

    /// <summary>
    /// The hull is stored as faces, each listing the vertices of a convex polygon, so each face is split into a
    /// triangle fan around its first vertex.
    /// </summary>
    private static (List<Vector3>, List<int>) ProcessConvexHull(hknpConvexHull hull)
    {
        var vertices = hull.m_vertices.Select(v => new Vector3(v.m_x, v.m_y, v.m_z)).ToList();
        var indices = new List<int>();

        foreach (var face in hull.m_faces)
        {
            for (var i = 1; i < face.m_numIndices - 1; i++)
            {
                indices.Add(hull.m_indices[face.m_firstIndex]);
                indices.Add(hull.m_indices[face.m_firstIndex + i]);
                indices.Add(hull.m_indices[face.m_firstIndex + i + 1]);
            }
        }

        return (vertices, indices);
    }
}
