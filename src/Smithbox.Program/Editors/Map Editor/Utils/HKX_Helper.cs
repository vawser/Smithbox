using HKX2;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

public static class HKX_Helper
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
            foreach (HKX.HKXObject obj in resource.Hkx.DataSection.Objects)
            {
                if (obj is HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage col)
                {
                    var mesh = new CollisionSubmesh();

                    ProcessMesh(col, mesh, resource.IsConnectCollision);

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

                if (obj is HKX.FSNPCustomParamCompressedMeshShape ncol)
                {
                    // Find a body data for this
                    HKX.HKNPBodyCInfo bodyInfo = null;
                    foreach (HKX.HKXObject scene in resource.Hkx.DataSection.Objects)
                    {
                        if (scene is HKX.HKNPPhysicsSystemData)
                        {
                            var sys = (HKX.HKNPPhysicsSystemData)scene;
                            foreach (HKX.HKNPBodyCInfo info in sys.Bodies.GetArrayData().Elements)
                            {
                                if (info.ShapeReference.DestObject == ncol)
                                {
                                    bodyInfo = info;
                                    break;
                                }
                            }

                            break;
                        }

                        try
                        {
                            var mesh = new CollisionSubmesh();

                            ProcessMesh(ncol, bodyInfo, mesh, resource.IsConnectCollision);

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
                            TaskLogs.AddLog($"[Smithbox] Failed to load HKX.", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                }
                //Bounds = BoundingBox.CreateMerged(Bounds, GPUMeshes[i].Bounds);
            }

            resource.GPUMeshes = submeshes.ToArray();
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            resource.Hkx = null;
        }

        return true;
    }

    /// <summary>
    /// Processes the HKX collision data and renders the collision mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="dest"></param>
    public static unsafe void ProcessMesh(HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage mesh, CollisionSubmesh dest, bool isConnectCollision = false)
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

        List<HKX.HKVector4> verts = mesh.Vertices.GetArrayData().Elements;
        dynamic indices;
        if (mesh.Indices8?.Capacity > 0)
        {
            indices = mesh.Indices8.GetArrayData().Elements;
        }
        else if (mesh.Indices16?.Capacity > 0)
        {
            indices = mesh.Indices16.GetArrayData().Elements;
        }
        else //Indices32 have to be there if those aren't
        {
            indices = mesh.Indices32.GetArrayData().Elements;
        }

        dest.VertexCount = indices.Count / 4 * 3;
        dest.IndexCount = indices.Count / 4 * 3;
        var buffersize = (uint)dest.IndexCount * 4u;
        var vbuffersize = (uint)dest.VertexCount * CollisionLayout.SizeInBytes;
        dest.GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)CollisionLayout.SizeInBytes, 4);
        var MeshIndices = new Span<int>(dest.GeomBuffer.MapIBuffer().ToPointer(), dest.IndexCount);
        var MeshVertices =
            new Span<CollisionLayout>(dest.GeomBuffer.MapVBuffer().ToPointer(), dest.VertexCount);
        dest.PickingVertices = new Vector3[indices.Count / 4 * 3];
        dest.PickingIndices = new int[indices.Count / 4 * 3];

        for (var id = 0; id < indices.Count; id += 4)
        {
            var i = id / 4 * 3;
            Vector4 vert1 = mesh.Vertices[(int)indices[id].data].Vector;
            Vector4 vert2 = mesh.Vertices[(int)indices[id + 1].data].Vector;
            Vector4 vert3 = mesh.Vertices[(int)indices[id + 2].data].Vector;

            MeshVertices[i] = new CollisionLayout();
            MeshVertices[i + 1] = new CollisionLayout();
            MeshVertices[i + 2] = new CollisionLayout();

            MeshVertices[i].Position = new Vector3(vert1.X, vert1.Y, vert1.Z);
            MeshVertices[i + 1].Position = new Vector3(vert2.X, vert2.Y, vert2.Z);
            MeshVertices[i + 2].Position = new Vector3(vert3.X, vert3.Y, vert3.Z);
            dest.PickingVertices[i] = new Vector3(vert1.X, vert1.Y, vert1.Z);
            dest.PickingVertices[i + 1] = new Vector3(vert2.X, vert2.Y, vert2.Z);
            dest.PickingVertices[i + 2] = new Vector3(vert3.X, vert3.Y, vert3.Z);
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
            dest.PickingIndices[i] = i;
            dest.PickingIndices[i + 1] = i + 1;
            dest.PickingIndices[i + 2] = i + 2;
        }

        dest.GeomBuffer.UnmapIBuffer();
        dest.GeomBuffer.UnmapVBuffer();

        fixed (void* ptr = dest.PickingVertices)
        {
            dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, dest.PickingVertices.Count(), 12,
                Quaternion.Identity, Vector3.Zero, Vector3.One);
        }
    }

    /// <summary>
    /// Processes the HKX collision data and renders the collision mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="bodyinfo"></param>
    /// <param name="dest"></param>
    public static unsafe void ProcessMesh(HKX.FSNPCustomParamCompressedMeshShape mesh, HKX.HKNPBodyCInfo bodyinfo, CollisionSubmesh dest, bool isConnectCollision = false)
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

        var verts = new List<Vector3>();
        var indices = new List<int>();

        HKX.HKNPCompressedMeshShapeData coldata = mesh.GetMeshShapeData();
        foreach (HKX.CollisionMeshChunk section in coldata.sections.GetArrayData().Elements)
        {
            for (var i = 0; i < section.primitivesLength; i++)
            {
                HKX.MeshPrimitive tri = coldata.primitives.GetArrayData().Elements[i + section.primitivesIndex];
                //if (tri.Idx2 == tri.Idx3 && tri.Idx1 != tri.Idx2)
                //{

                if (tri.Idx0 == 0xDE && tri.Idx1 == 0xAD && tri.Idx2 == 0xDE && tri.Idx3 == 0xAD)
                {
                    continue; // Don't know what to do with this shape yet
                }

                if (tri.Idx0 < section.sharedVerticesLength)
                {
                    var index = (ushort)(tri.Idx0 + section.firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.packedVertices.GetArrayData().Elements[index]
                        .Decompress(section.SmallVertexScale, section.SmallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index = coldata.sharedVerticesIndex.GetArrayData()
                        .Elements[tri.Idx0 + section.sharedVerticesIndex - section.sharedVerticesLength].data;
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.sharedVertices.GetArrayData().Elements[index]
                        .Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.Idx1 < section.sharedVerticesLength)
                {
                    var index = (ushort)(tri.Idx1 + section.firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.packedVertices.GetArrayData().Elements[index]
                        .Decompress(section.SmallVertexScale, section.SmallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index = coldata.sharedVerticesIndex.GetArrayData()
                        .Elements[tri.Idx1 + section.sharedVerticesIndex - section.sharedVerticesLength].data;
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.sharedVertices.GetArrayData().Elements[index]
                        .Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.Idx2 < section.sharedVerticesLength)
                {
                    var index = (ushort)(tri.Idx2 + section.firstPackedVertex);
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.packedVertices.GetArrayData().Elements[index]
                        .Decompress(section.SmallVertexScale, section.SmallVertexOffset);
                    verts.Add(TransformVert(vert, bodyinfo));
                }
                else
                {
                    var index = coldata.sharedVerticesIndex.GetArrayData()
                        .Elements[tri.Idx2 + section.sharedVerticesIndex - section.sharedVerticesLength].data;
                    indices.Add(verts.Count);

                    Vector3 vert = coldata.sharedVertices.GetArrayData().Elements[index]
                        .Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                    verts.Add(TransformVert(vert, bodyinfo));
                }

                if (tri.Idx2 != tri.Idx3)
                {
                    indices.Add(verts.Count);
                    verts.Add(verts[verts.Count - 3]);
                    indices.Add(verts.Count);
                    verts.Add(verts[verts.Count - 2]);
                    if (tri.Idx3 < section.sharedVerticesLength)
                    {
                        var index = (ushort)(tri.Idx3 + section.firstPackedVertex);
                        indices.Add(verts.Count);

                        Vector3 vert = coldata.packedVertices.GetArrayData().Elements[index]
                            .Decompress(section.SmallVertexScale, section.SmallVertexOffset);
                        verts.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        var index = coldata.sharedVerticesIndex.GetArrayData()
                            .Elements[tri.Idx3 + section.sharedVerticesIndex - section.sharedVerticesLength].data;
                        indices.Add(verts.Count);

                        Vector3 vert = coldata.sharedVertices.GetArrayData().Elements[index]
                            .Decompress(coldata.BoundingBoxMin, coldata.BoundingBoxMax);
                        verts.Add(TransformVert(vert, bodyinfo));
                    }
                }
            }
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

        dest.GeomBuffer.UnmapVBuffer();
        dest.GeomBuffer.UnmapIBuffer();

        fixed (void* ptr = dest.PickingVertices)
        {
            dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, dest.PickingVertices.Count(), 12,
                Quaternion.Identity, Vector3.Zero, Vector3.One);
        }
    }
    public static Vector3 TransformVert(Vector3 vert, HKX.HKNPBodyCInfo body)
    {
        var newVert = new Vector3(vert.X, vert.Y, vert.Z);
        if (body == null)
        {
            return newVert;
        }

        Vector3 trans = new(body.Position.Vector.X, body.Position.Vector.Y, body.Position.Vector.Z);
        Quaternion quat = new(body.Orientation.Vector.X, body.Orientation.Vector.Y, body.Orientation.Vector.Z,
            body.Orientation.Vector.W);
        return Vector3.Transform(newVert, quat) + trans;
    }
}