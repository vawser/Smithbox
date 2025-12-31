using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class HKLib_Helper
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

            if (resource.ER_HKX.m_namedVariants.Count == 0)
            {
                // Yes this happens for some cols wtf From???
                return false;
            }

            var scene = resource.ER_HKX.m_namedVariants[0].m_variant;
            if (scene is hknpPhysicsSceneData physicsscene)
            {
                foreach (hknpBodyCinfo bodyInfo in physicsscene.m_systemDatas[0].m_bodyCinfos)
                {
                    if (bodyInfo.m_shape is fsnpCustomParamCompressedMeshShape ncol)
                    {
                        try
                        {
                            var mesh = new CollisionSubmesh();
                            var indices = new List<int>();
                            var vertices = new List<Vector3>();

                            if (bodyInfo.m_shape is fsnpCustomParamCompressedMeshShape shape2)
                            {
                                (mesh, vertices, indices) = ProcessColData(
                                    (hknpCompressedMeshShapeData)shape2.m_data, bodyInfo, mesh);

                                RenderMesh(mesh, vertices, indices, resource.IsConnectCollision);
                            }
                            else if (bodyInfo.m_shape is hknpCompressedMeshShape shape1)
                            {
                                (mesh, vertices, indices) = ProcessColData(
                                    (hknpCompressedMeshShapeData)shape1.m_data, bodyInfo, mesh);

                                RenderMesh(mesh, vertices, indices, resource.IsConnectCollision);
                            }

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
                            TaskLogs.AddLog($"[Smithbox] Failed to load HKLIB.",
                                LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    else if (bodyInfo.m_shape is hknpExternMeshShape extern_ncol)
                    {
                        try
                        {
                            var mesh = new CollisionSubmesh();
                            var indices = new List<int>();
                            var vertices = new List<Vector3>();

                            if(extern_ncol.m_geometry is hknpDefaultExternMeshShapeGeometry meshShape)
                            {
                                if (meshShape.m_geometry is hkGeometry geo)
                                {
                                    vertices = geo.m_vertices
                                        .Select(v => new Vector3(v.X, v.Y, v.Z))
                                        .ToList();

                                    foreach (var tri in geo.m_triangles)
                                    {
                                        indices.Add(tri.m_a);
                                        indices.Add(tri.m_b);
                                        indices.Add(tri.m_c);
                                    }

                                    RenderMesh(mesh, vertices, indices, resource.IsConnectCollision);
                                }
                            }

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
                            TaskLogs.AddLog($"[Smithbox] Failed to load HKLIB.",
                                LogLevel.Error, LogPriority.High, e);
                        }
                    }
                }
            }
            else if (scene is hkxScene actualscene)
            {
                // TODO: This is a case that exists as well
            }

            resource.GPUMeshes = submeshes.ToArray();
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.ER_HKX = null;
        }

        return true;
    }

    /// <summary>
    /// Processes the HKX collision data into data usable for the rendering system
    /// </summary>
    /// <param name="coldata"></param>
    /// <param name="bodyinfo"></param>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static (CollisionSubmesh, List<Vector3>, List<int>) ProcessColData(
    hknpCompressedMeshShapeData coldata,
    hknpBodyCinfo bodyinfo,
    CollisionSubmesh mesh)
    {
        List<Vector3> vector3List = new List<Vector3>();
        List<int> intList = new List<int>();

        for (int index1 = 0; index1 < coldata.m_meshTree.m_sections.Count; ++index1)
        {
            Section section = coldata.m_meshTree.m_sections[index1];
            for (int index2 = 0; index2 < (int)section.m_numPrimitives; ++index2)
            {
                Primitive primitive = coldata.m_meshTree.m_primitives[index2 + (int)section.m_firstPrimitiveIndex];
                if (primitive.m_indices[0] != (byte)222 || primitive.m_indices[1] != (byte)173 || primitive.m_indices[2] != (byte)222 || primitive.m_indices[3] != (byte)173)
                {
                    byte numPackedVertices = section.m_numPackedVertices;
                    uint sharedVertexIndex = section.m_firstSharedVertexIndex;
                    Vector3 offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
                    Vector3 scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);
                    if ((int)primitive.m_indices[0] < (int)numPackedVertices)
                    {
                        ushort index3 = (ushort)((uint)primitive.m_indices[0] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index3], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index4 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[0] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index4], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[1] < (int)numPackedVertices)
                    {
                        ushort index5 = (ushort)((uint)primitive.m_indices[1] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index5], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index6 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[1] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index6], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[2] < (int)numPackedVertices)
                    {
                        ushort index7 = (ushort)((uint)primitive.m_indices[2] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index7], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index8 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[2] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index8], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[2] != (int)primitive.m_indices[3])
                    {
                        intList.Add(vector3List.Count);
                        vector3List.Add(vector3List[vector3List.Count - 3]);
                        intList.Add(vector3List.Count);
                        vector3List.Add(vector3List[vector3List.Count - 2]);
                        if ((int)primitive.m_indices[3] < (int)numPackedVertices)
                        {
                            ushort index9 = (ushort)((uint)primitive.m_indices[3] + section.m_firstPackedVertexIndex);
                            intList.Add(vector3List.Count);
                            Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index9], scale, offset);
                            vector3List.Add(TransformVert(vert, bodyinfo));
                        }
                        else
                        {
                            ushort index10 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[3] + (int)sharedVertexIndex - (int)numPackedVertices];
                            intList.Add(vector3List.Count);
                            Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index10], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                            vector3List.Add(TransformVert(vert, bodyinfo));
                        }
                    }
                }
            }
        }

        return (mesh, vector3List, intList);
    }

    /// <summary>
    /// Render the collision mesh
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="verts"></param>
    /// <param name="indices"></param>
    private static unsafe void RenderMesh(CollisionSubmesh dest, List<Vector3> verts, List<int> indices, bool isConnectCollision = false)
    {
        byte colR = (byte)CFG.Current.GFX_Renderable_Collision_Color.X;
        byte colG = (byte)CFG.Current.GFX_Renderable_Collision_Color.Y;
        byte colB = (byte)CFG.Current.GFX_Renderable_Collision_Color.Z;
        byte colA = 255;

        if(isConnectCollision)
        {
            colR = (byte)CFG.Current.GFX_Renderable_ConnectCollision_Color.X;
            colG = (byte)CFG.Current.GFX_Renderable_ConnectCollision_Color.Y;
            colB = (byte)CFG.Current.GFX_Renderable_ConnectCollision_Color.Z;
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

    private static Vector3 DecompressSharedVertex(ulong vertex, Vector4 bbMin, Vector4 bbMax)
    {
        float num1 = (float)(((double)bbMax.X - (double)bbMin.X) / 2097151.0);
        float num2 = (float)(((double)bbMax.Y - (double)bbMin.Y) / 2097151.0);
        float num3 = (float)(((double)bbMax.Z - (double)bbMin.Z) / 4194303.0);
        double x = (double)(vertex & 2097151UL) * (double)num1 + (double)bbMin.X;
        float num4 = (float)(vertex >> 21 & 2097151UL) * num2 + bbMin.Y;
        float num5 = (float)(vertex >> 42 & 4194303UL) * num3 + bbMin.Z;
        double y = (double)num4;
        double z = (double)num5;
        return new Vector3((float)x, (float)y, (float)z);
    }

    private static Vector3 DecompressPackedVertex(uint vertex, Vector3 scale, Vector3 offset)
    {
        double x = (double)(vertex & 2047U) * (double)scale.X + (double)offset.X;
        float num1 = (float)(vertex >> 11 & 2047U) * scale.Y + offset.Y;
        float num2 = (float)(vertex >> 22 & 1023U) * scale.Z + offset.Z;
        double y = (double)num1;
        double z = (double)num2;
        return new Vector3((float)x, (float)y, (float)z);
    }

}
