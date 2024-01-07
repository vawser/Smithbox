using SoulsFormats;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Resource;

public class NVMNavmeshResource : IResource, IDisposable
{
    public int IndexCount;

    public NVM Nvm;
    public int[] PickingIndices;

    public Vector3[] PickingVertices;

    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeomBuffer { get; set; }

    public int VertexCount { get; set; }


    public BoundingBox Bounds { get; set; }

    public bool _Load(Memory<byte> bytes, AccessLevel al, GameType type)
    {
        Nvm = NVM.Read(bytes);
        return LoadInternal(al);
    }

    public bool _Load(string file, AccessLevel al, GameType type)
    {
        Nvm = NVM.Read(file);
        return LoadInternal(al);
    }

    private unsafe void ProcessMesh(NVM mesh)
    {
        List<Vector3> verts = mesh.Vertices;
        VertexCount = mesh.Triangles.Count * 3;
        IndexCount = mesh.Triangles.Count * 3;
        var buffersize = (uint)IndexCount * 4u;
        var vbuffersize = (uint)VertexCount * NavmeshLayout.SizeInBytes;
        GeomBuffer =
            Renderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);
        var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
        var MeshVertices =
            new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);
        PickingVertices = new Vector3[mesh.Triangles.Count * 3];
        PickingIndices = new int[mesh.Triangles.Count * 3];

        for (var id = 0; id < mesh.Triangles.Count; id++)
        {
            var i = id * 3;
            Vector3 vert1 = mesh.Vertices[mesh.Triangles[id].VertexIndex1];
            Vector3 vert2 = mesh.Vertices[mesh.Triangles[id].VertexIndex2];
            Vector3 vert3 = mesh.Vertices[mesh.Triangles[id].VertexIndex3];

            MeshVertices[i] = new NavmeshLayout();
            MeshVertices[i + 1] = new NavmeshLayout();
            MeshVertices[i + 2] = new NavmeshLayout();

            MeshVertices[i].Position = new Vector3(vert1.X, vert1.Y, vert1.Z);
            MeshVertices[i + 1].Position = new Vector3(vert2.X, vert2.Y, vert2.Z);
            MeshVertices[i + 2].Position = new Vector3(vert3.X, vert3.Y, vert3.Z);
            PickingVertices[i] = new Vector3(vert1.X, vert1.Y, vert1.Z);
            PickingVertices[i + 1] = new Vector3(vert2.X, vert2.Y, vert2.Z);
            PickingVertices[i + 2] = new Vector3(vert3.X, vert3.Y, vert3.Z);
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

            if ((mesh.Triangles[id].Flags & NVM.TriangleFlags.GATE) > 0)
            {
                MeshVertices[i].Color[0] = 50;
                MeshVertices[i].Color[1] = 220;
                MeshVertices[i].Color[2] = 0;
                MeshVertices[i].Color[3] = 255;
                MeshVertices[i + 1].Color[0] = 50;
                MeshVertices[i + 1].Color[1] = 220;
                MeshVertices[i + 1].Color[2] = 0;
                MeshVertices[i + 1].Color[3] = 255;
                MeshVertices[i + 2].Color[0] = 50;
                MeshVertices[i + 2].Color[1] = 220;
                MeshVertices[i + 2].Color[2] = 0;
                MeshVertices[i + 2].Color[3] = 255;
            }
            else
            {
                MeshVertices[i].Color[0] = 157;
                MeshVertices[i].Color[1] = 53;
                MeshVertices[i].Color[2] = 255;
                MeshVertices[i].Color[3] = 255;
                MeshVertices[i + 1].Color[0] = 157;
                MeshVertices[i + 1].Color[1] = 53;
                MeshVertices[i + 1].Color[2] = 255;
                MeshVertices[i + 1].Color[3] = 255;
                MeshVertices[i + 2].Color[0] = 157;
                MeshVertices[i + 2].Color[1] = 53;
                MeshVertices[i + 2].Color[2] = 255;
                MeshVertices[i + 2].Color[3] = 255;
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
            PickingIndices[i] = i;
            PickingIndices[i + 1] = i + 1;
            PickingIndices[i + 2] = i + 2;
        }

        GeomBuffer.UnmapIBuffer();
        GeomBuffer.UnmapVBuffer();

        if (VertexCount > 0)
        {
            fixed (void* ptr = PickingVertices)
            {
                Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, PickingVertices.Count(), 12,
                    Quaternion.Identity, Vector3.Zero, Vector3.One);
            }
        }
        else
        {
            Bounds = new BoundingBox();
        }
    }

    private bool LoadInternal(AccessLevel al)
    {
        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Bounds = new BoundingBox();
            ProcessMesh(Nvm);
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Nvm = null;
        }

        return true;
    }

    public bool RayCast(Ray ray, Matrix4x4 transform, out float dist)
    {
        var hit = false;
        var mindist = float.MaxValue;
        Matrix4x4 invw = transform.Inverse();
        Vector3 newo = Vector3.Transform(ray.Origin, invw);
        Vector3 newd = Vector3.TransformNormal(ray.Direction, invw);
        var tray = new Ray(newo, newd);
        if (!tray.Intersects(Bounds))
        {
            dist = float.MaxValue;
            return false;
        }

        for (var index = 0; index < PickingIndices.Count(); index += 3)
        {
            float locdist;
            if (tray.Intersects(ref PickingVertices[PickingIndices[index]],
                    ref PickingVertices[PickingIndices[index + 1]],
                    ref PickingVertices[PickingIndices[index + 2]],
                    out locdist))
            {
                hit = true;
                if (locdist < mindist)
                {
                    mindist = locdist;
                }
            }
        }

        dist = mindist;
        return hit;
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            GeomBuffer.Dispose();

            disposedValue = true;
        }
    }

    ~NVMNavmeshResource()
    {
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
