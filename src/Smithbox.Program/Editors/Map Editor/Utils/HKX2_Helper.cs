using HKX2;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class HKX2_Helper
{
    /// <summary>
    /// Load the HKX collision mesh
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="al"></param>
    /// <returns></returns>
    public static bool LoadCollisionMesh(HavokCollisionResource resource, AccessLevel al)
    {
        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Bounds = new BoundingBox();
            var submeshes = new List<CollisionSubmesh>();
            var first = true;
            if (resource.Hkx2.m_namedVariants.Count == 0)
            {
                // Yes this happens for some cols wtf From???
                return false;
            }

            var physicsscene = (hknpPhysicsSceneData)resource.Hkx2.m_namedVariants[0].m_variant;

            foreach (hknpBodyCinfo bodyInfo in physicsscene.m_systemDatas[0].m_bodyCinfos)
            {
                if (bodyInfo.m_shape is not fsnpCustomParamCompressedMeshShape ncol)
                {
                    continue;
                }

                try
                {
                    var mesh = new CollisionSubmesh();

                    var indices = new List<int>();
                    var vertices = new List<Vector3>();

                    (mesh, vertices, indices) = ProcessMesh(ncol, bodyInfo, mesh);

                    RenderMesh(mesh, vertices, indices, resource.IsConnectCollision);

                    if (first)
                    {
                        resource.Bounds = mesh.Bounds;
                        first = false;
                    }
                    else
                    {
                        resource.Bounds = BoundingBox.Combine(resource.Bounds, mesh.Bounds);
                    }

                    submeshes.Add(mesh);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load HKX2.", LogLevel.Error, LogPriority.High, e);
                }
            }

            resource.GPUMeshes = submeshes.ToArray();
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Hkx2 = null;
        }

        return true;
    }

    /// <summary>
    /// Processes the HKX collision data into data usable for the rendering system
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="bodyinfo"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static unsafe (CollisionSubmesh, List<Vector3>, List<int>) ProcessMesh(
        fsnpCustomParamCompressedMeshShape mesh, hknpBodyCinfo bodyinfo, CollisionSubmesh dest)
    {
        var verts = new List<Vector3>();
        var indices = new List<int>();

        hknpCompressedMeshShapeData coldata = mesh.m_data;
        foreach (hkcdStaticMeshTreeBaseSection section in coldata.m_meshTree.m_sections)
        {
            for (var i = 0; i < (section.m_primitives.m_data & 0xFF); i++)
            {
                hkcdStaticMeshTreeBasePrimitive tri =
                    coldata.m_meshTree.m_primitives[i + (int)(section.m_primitives.m_data >> 8)];
                //if (tri.Idx2 == tri.Idx3 && tri.Idx1 != tri.Idx2)
                //{

                if (tri.m_indices_0 == 0xDE && tri.m_indices_1 == 0xAD && tri.m_indices_2 == 0xDE &&
                    tri.m_indices_3 == 0xAD)
                {
                    continue; // Don't know what to do with this shape yet
                }

                var sharedVerticesLength = section.m_sharedVertices.m_data & 0xFF;
                var sharedVerticesIndex = section.m_sharedVertices.m_data >> 8;
                var smallVertexOffset = new Vector3(section.m_codecParms_0, section.m_codecParms_1,
                    section.m_codecParms_2);
                var smallVertexScale = new Vector3(section.m_codecParms_3, section.m_codecParms_4,
                    section.m_codecParms_5);
                if (tri.m_indices_0 < sharedVerticesLength)
                {
                    var index = (ushort)(tri.m_indices_0 + section.m_firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[index],
                        smallVertexScale, smallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index =
                        coldata.m_meshTree.m_sharedVerticesIndex[
                            (int)(tri.m_indices_0 + sharedVerticesIndex - sharedVerticesLength)];
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[index],
                        coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.m_indices_1 < sharedVerticesLength)
                {
                    var index = (ushort)(tri.m_indices_1 + section.m_firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[index],
                        smallVertexScale, smallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index =
                        coldata.m_meshTree.m_sharedVerticesIndex[
                            (int)(tri.m_indices_1 + sharedVerticesIndex - sharedVerticesLength)];
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[index],
                        coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.m_indices_2 < sharedVerticesLength)
                {
                    var index = (ushort)(tri.m_indices_2 + section.m_firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[index],
                        smallVertexScale, smallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index =
                        coldata.m_meshTree.m_sharedVerticesIndex[
                            (int)(tri.m_indices_2 + sharedVerticesIndex - sharedVerticesLength)];
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[index],
                        coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.m_indices_2 != tri.m_indices_3)
                {
                    indices.Add(verts.Count);
                    verts.Add(verts[verts.Count - 3]);
                    indices.Add(verts.Count);
                    verts.Add(verts[verts.Count - 2]);
                    if (tri.m_indices_3 < sharedVerticesLength)
                    {
                        var index = (ushort)(tri.m_indices_3 + section.m_firstPackedVertex);
                        indices.Add(verts.Count);

                        Vector3 vert = coldata.DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[index],
                            smallVertexScale, smallVertexOffset);
                        verts.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        var index =
                            coldata.m_meshTree.m_sharedVerticesIndex[
                                (int)(tri.m_indices_3 + sharedVerticesIndex - sharedVerticesLength)];
                        indices.Add(verts.Count);

                        Vector3 vert = coldata.DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[index],
                            coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        verts.Add(TransformVert(vert, bodyinfo));
                    }
                }
            }
        }
        return (dest, verts, indices);
    }

    /// <summary>
    /// Render the collision mesh
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="verts"></param>
    /// <param name="indices"></param>
    public static unsafe void RenderMesh(CollisionSubmesh dest, List<Vector3> verts, List<int> indices, bool isConnectCollision = false)
    {
        byte colR = (byte)CFG.Current.Viewport_Collision_Color.X;
        byte colG = (byte)CFG.Current.Viewport_Collision_Color.Y;
        byte colB = (byte)CFG.Current.Viewport_Collision_Color.Z;
        byte colA = 255;

        if (isConnectCollision)
        {
            colR = (byte)CFG.Current.Viewport_Connect_Collision_Color.X;
            colG = (byte)CFG.Current.Viewport_Connect_Collision_Color.Y;
            colB = (byte)CFG.Current.Viewport_Connect_Collision_Color.Z;
        }

        dest.PickingIndices = indices.ToArray();
        dest.PickingVertices = verts.ToArray();

        dest.VertexCount = indices.Count;
        dest.IndexCount = indices.Count;
        var buffersize = (uint)dest.IndexCount * 4u;
        var vbuffersize = (uint)dest.VertexCount * CollisionLayout.SizeInBytes;
        dest.GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)CollisionLayout.SizeInBytes, 4);
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

            MeshVertices[i].Normal[0] = (sbyte)(n.X * 127.0f);
            MeshVertices[i].Normal[1] = (sbyte)(n.Y * 127.0f);
            MeshVertices[i].Normal[2] = (sbyte)(n.Z * 127.0f);
            MeshVertices[i + 1].Normal[0] = (sbyte)(n.X * 127.0f);
            MeshVertices[i + 1].Normal[1] = (sbyte)(n.Y * 127.0f);
            MeshVertices[i + 1].Normal[2] = (sbyte)(n.Z * 127.0f);
            MeshVertices[i + 2].Normal[0] = (sbyte)(n.X * 127.0f);
            MeshVertices[i + 2].Normal[1] = (sbyte)(n.Y * 127.0f);
            MeshVertices[i + 2].Normal[2] = (sbyte)(n.Z * 127.0f);

            MeshVertices[i].Color[0] = colR;
            MeshVertices[i].Color[1] = colG;
            MeshVertices[i].Color[2] = colB;
            MeshVertices[i].Color[3] = colA;
            MeshVertices[i + 1].Color[0] = colR;
            MeshVertices[i + 1].Color[1] = colG;
            MeshVertices[i + 1].Color[2] = colB;
            MeshVertices[i + 1].Color[3] = colA;
            MeshVertices[i + 2].Color[0] = colR;
            MeshVertices[i + 2].Color[1] = colG;
            MeshVertices[i + 2].Color[2] = colB;
            MeshVertices[i + 2].Color[3] = colA;

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

        dest.GeomBuffer.UnmapIBuffer();
        dest.GeomBuffer.UnmapVBuffer();

        fixed (void* ptr = dest.PickingVertices)
        {
            dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, dest.PickingVertices.Count(), 12,
                Quaternion.Identity, Vector3.Zero, Vector3.One);
        }
    }

    private static Vector3 TransformVert(Vector3 vert, hknpBodyCinfo body)
    {
        Vector3 vector3_1 = new Vector3(vert.X, vert.Y, vert.Z);
        if (body == null)
            return vector3_1;
        Vector3 vector3_2 = new Vector3(body.m_position.X, body.m_position.Y, body.m_position.Z);
        return Vector3.Transform(vector3_1, body.m_orientation) + vector3_2;
    }
}