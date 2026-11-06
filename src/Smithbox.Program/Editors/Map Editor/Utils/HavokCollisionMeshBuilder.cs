using Havok.Shared;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Builds the collision meshes of every game the unified Havok library reads (all but DS2): the shared
/// CollisionGeometry gives the triangles, a submesh per body (or per mesh storage for the hkp games), and this uploads
/// them. Replaces the per-library HKX2_Helper, HKLib_Helper and HKLib2019_Helper.
/// </summary>
public static class HavokCollisionMeshBuilder
{
    public static bool LoadCollisionMesh(HavokCollisionResource resource, AccessLevel al)
    {
        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Bounds = new BoundingBox();
            var submeshes = new List<CollisionSubmesh>();

            var triangleLists = CollisionGeometry.Submeshes(resource.Document.Objects, (shape, e) =>
                Smithbox.Log(typeof(HavokCollisionMeshBuilder), $"Failed to load collision ({shape?.GetType().Name}).",
                    LogLevel.Error, LogPriority.High, e));

            foreach (List<Vector3> triangles in triangleLists)
            {
                var mesh = new CollisionSubmesh();
                RenderMesh(mesh, triangles, Enumerable.Range(0, triangles.Count).ToList(), resource.Color);

                resource.Bounds = submeshes.Count == 0 ? mesh.Bounds : BoundingBox.Combine(resource.Bounds, mesh.Bounds);
                submeshes.Add(mesh);
            }

            resource.GPUMeshes = submeshes.ToArray();
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Document = null;
        }

        return true;
    }

    /// <summary>
    /// Rebuilds the GPU meshes of a loaded collision from an edited document, a submesh per body as loading builds
    /// them, so the drawn mesh follows the edit. The submesh providers index into the resource's meshes, so the
    /// count must stay the same: if it doesn't, nothing is changed and null is returned. Otherwise returns the old
    /// submeshes, whose buffers are to be freed once nothing draws from them.
    /// </summary>
    public static CollisionSubmesh[] RefreshCollisionMesh(HavokCollisionResource resource, Havok.HavokDocument document)
    {
        var triangleLists = CollisionGeometry.Submeshes(document.Objects, (shape, e) =>
            Smithbox.Log(typeof(HavokCollisionMeshBuilder), $"Failed to load collision ({shape?.GetType().Name}).",
                LogLevel.Error, LogPriority.High, e));

        if (resource.GPUMeshes == null || triangleLists.Count != resource.GPUMeshes.Length)
            return null;

        var bounds = new BoundingBox();
        var submeshes = new CollisionSubmesh[triangleLists.Count];

        for (var i = 0; i < triangleLists.Count; i++)
        {
            var triangles = triangleLists[i];
            var mesh = new CollisionSubmesh();
            RenderMesh(mesh, triangles, Enumerable.Range(0, triangles.Count).ToList(), resource.Color);

            bounds = i == 0 ? mesh.Bounds : BoundingBox.Combine(bounds, mesh.Bounds);
            submeshes[i] = mesh;
        }

        var old = resource.GPUMeshes;

        resource.GPUMeshes = submeshes;
        resource.Bounds = bounds;

        return old;
    }

    /// <summary>
    /// Uploads a triangle list (indices in threes) as a collision submesh, with flat normals.
    /// </summary>
    internal static unsafe void RenderMesh(CollisionSubmesh dest, List<Vector3> verts, List<int> indices, Vector3 color)
    {
        byte colR = (byte)color.X;
        byte colG = (byte)color.Y;
        byte colB = (byte)color.Z;
        byte colA = 255;

        dest.PickingIndices = indices.ToArray();
        dest.PickingVertices = verts.ToArray();

        dest.VertexCount = indices.Count;
        dest.IndexCount = indices.Count;
        var buffersize = (uint)dest.IndexCount * 4u;
        var vbuffersize = (uint)dest.VertexCount * CollisionLayout.SizeInBytes;
        dest.GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)CollisionLayout.SizeInBytes, 4);

        // The allocator only uploads a buffer once everything allocated in it is filled, so this is always marked
        // filled, even if filling fails, or nothing else in the buffer would be drawn
        try
        {
            var MeshIndices = new Span<int>(dest.GeomBuffer.MapIBuffer().ToPointer(), dest.IndexCount);
            var MeshVertices =
                new Span<CollisionLayout>(dest.GeomBuffer.MapVBuffer().ToPointer(), dest.VertexCount);

            for (var i = 0; i < indices.Count; i += 3)
            {
                Vector3 vert1 = verts[indices[i]];
                Vector3 vert2 = verts[indices[i + 1]];
                Vector3 vert3 = verts[indices[i + 2]];

                MeshVertices[i] = new CollisionLayout();
                MeshVertices[i + 1] = new CollisionLayout();
                MeshVertices[i + 2] = new CollisionLayout();

                MeshVertices[i].Position = vert1;
                MeshVertices[i + 1].Position = vert2;
                MeshVertices[i + 2].Position = vert3;
                Vector3 n = Vector3.Normalize(Vector3.Cross(MeshVertices[i + 2].Position - MeshVertices[i].Position,
                    MeshVertices[i + 1].Position - MeshVertices[i].Position));

                for (var k = 0; k < 3; k++)
                {
                    MeshVertices[i + k].Normal[0] = (sbyte)(n.X * 127.0f);
                    MeshVertices[i + k].Normal[1] = (sbyte)(n.Y * 127.0f);
                    MeshVertices[i + k].Normal[2] = (sbyte)(n.Z * 127.0f);
                    MeshVertices[i + k].Color[0] = colR;
                    MeshVertices[i + k].Color[1] = colG;
                    MeshVertices[i + k].Color[2] = colB;
                    MeshVertices[i + k].Color[3] = colA;
                }

                MeshVertices[i].Barycentric[0] = 0;
                MeshVertices[i].Barycentric[1] = 0;
                MeshVertices[i + 1].Barycentric[0] = 1;
                MeshVertices[i + 1].Barycentric[1] = 0;
                MeshVertices[i + 2].Barycentric[0] = 0;
                MeshVertices[i + 2].Barycentric[1] = 1;

                MeshIndices[i] = i;
                MeshIndices[i + 1] = i + 1;
                MeshIndices[i + 2] = i + 2;
            }
        }
        finally
        {
            dest.GeomBuffer.UnmapIBuffer();
            dest.GeomBuffer.UnmapVBuffer();
        }

        fixed (void* ptr = dest.PickingVertices)
        {
            dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, dest.PickingVertices.Count(), 12,
                Quaternion.Identity, Vector3.Zero, Vector3.One);
        }
    }
}
