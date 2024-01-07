using DotNext.IO.MemoryMappedFiles;
using HKX2;
using SoulsFormats;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Resource;

public class HavokNavmeshResource : IResource, IDisposable
{
    public int GraphIndexCount;

    public hkRootLevelContainer HkxRoot;
    public int IndexCount;
    public int[] PickingIndices;

    public Vector3[] PickingVertices;

    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeomBuffer { get; set; }

    public VertexIndexBufferAllocator.VertexIndexBufferHandle CostGraphGeomBuffer { get; set; }


    public int VertexCount { get; set; }
    public int GraphVertexCount { get; set; }


    public BoundingBox Bounds { get; set; }

    public bool _Load(Memory<byte> bytes, AccessLevel al, GameType type)
    {
        BinaryReaderEx br = new(false, bytes);
        var des = new PackFileDeserializer();
        HkxRoot = (hkRootLevelContainer)des.Deserialize(br);
        return LoadInternal(al);
    }

    public bool _Load(string path, AccessLevel al, GameType type)
    {
        using var file =
            MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using IMappedMemoryOwner accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
        var des = new PackFileDeserializer();
        HkxRoot = (hkRootLevelContainer)des.Deserialize(new BinaryReaderEx(false, accessor.Memory));

        return LoadInternal(al);
    }

    private unsafe void ProcessMesh(hkaiNavMesh mesh)
    {
        List<Vector4> verts = mesh.m_vertices;
        var indexCount = 0;
        foreach (hkaiNavMeshFace f in mesh.m_faces)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += (f.m_numEdges - 2) * 3;
        }

        VertexCount = indexCount * 3;
        IndexCount = indexCount * 3;
        var buffersize = (uint)IndexCount * 4u;
        var vbuffersize = (uint)VertexCount * NavmeshLayout.SizeInBytes;
        GeomBuffer =
            Renderer.GeometryBufferAllocator.Allocate(
                vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);
        var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
        var MeshVertices =
            new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);
        PickingVertices = new Vector3[VertexCount];
        PickingIndices = new int[IndexCount];

        ResourceFactory factory = Renderer.Factory;

        var idx = 0;

        var maxcluster = 0;

        for (var id = 0; id < mesh.m_faces.Count; id++)
        {
            if (mesh.m_faces[id].m_clusterIndex > maxcluster)
            {
                maxcluster = mesh.m_faces[id].m_clusterIndex;
            }

            var sedge = mesh.m_faces[id].m_startEdgeIndex;
            var ecount = mesh.m_faces[id].m_numEdges;

            // Use simple algorithm for convex polygon trianglization
            for (var t = 0; t < ecount - 2; t++)
            {
                if (ecount > 3)
                {
                    //ecount = ecount;
                }

                var end = t + 2 >= ecount ? sedge : sedge + t + 2;
                Vector4 vert1 = mesh.m_vertices[mesh.m_edges[sedge].m_a];
                Vector4 vert2 = mesh.m_vertices[mesh.m_edges[sedge + t + 1].m_a];
                Vector4 vert3 = mesh.m_vertices[mesh.m_edges[end].m_a];

                MeshVertices[idx] = new NavmeshLayout();
                MeshVertices[idx + 1] = new NavmeshLayout();
                MeshVertices[idx + 2] = new NavmeshLayout();

                MeshVertices[idx].Position = new Vector3(vert1.X, vert1.Y, vert1.Z);
                MeshVertices[idx + 1].Position = new Vector3(vert2.X, vert2.Y, vert2.Z);
                MeshVertices[idx + 2].Position = new Vector3(vert3.X, vert3.Y, vert3.Z);
                PickingVertices[idx] = new Vector3(vert1.X, vert1.Y, vert1.Z);
                PickingVertices[idx + 1] = new Vector3(vert2.X, vert2.Y, vert2.Z);
                PickingVertices[idx + 2] = new Vector3(vert3.X, vert3.Y, vert3.Z);
                Vector3 n = Vector3.Normalize(Vector3.Cross(
                    MeshVertices[idx + 2].Position - MeshVertices[idx].Position,
                    MeshVertices[idx + 1].Position - MeshVertices[idx].Position));
                MeshVertices[idx].Normal[0] = (sbyte)(n.X * 127.0f);
                MeshVertices[idx].Normal[1] = (sbyte)(n.Y * 127.0f);
                MeshVertices[idx].Normal[2] = (sbyte)(n.Z * 127.0f);
                MeshVertices[idx + 1].Normal[0] = (sbyte)(n.X * 127.0f);
                MeshVertices[idx + 1].Normal[1] = (sbyte)(n.Y * 127.0f);
                MeshVertices[idx + 1].Normal[2] = (sbyte)(n.Z * 127.0f);
                MeshVertices[idx + 2].Normal[0] = (sbyte)(n.X * 127.0f);
                MeshVertices[idx + 2].Normal[1] = (sbyte)(n.Y * 127.0f);
                MeshVertices[idx + 2].Normal[2] = (sbyte)(n.Z * 127.0f);

                MeshVertices[idx].Color[0] = 157;
                MeshVertices[idx].Color[1] = 53;
                MeshVertices[idx].Color[2] = 255;
                MeshVertices[idx].Color[3] = 255;
                MeshVertices[idx + 1].Color[0] = 157;
                MeshVertices[idx + 1].Color[1] = 53;
                MeshVertices[idx + 1].Color[2] = 255;
                MeshVertices[idx + 1].Color[3] = 255;
                MeshVertices[idx + 2].Color[0] = 157;
                MeshVertices[idx + 2].Color[1] = 53;
                MeshVertices[idx + 2].Color[2] = 255;
                MeshVertices[idx + 2].Color[3] = 255;

                MeshVertices[idx].Barycentric[0] = 0;
                MeshVertices[idx].Barycentric[1] = 0;
                MeshVertices[idx + 1].Barycentric[0] = 1;
                MeshVertices[idx + 1].Barycentric[1] = 0;
                MeshVertices[idx + 2].Barycentric[0] = 0;
                MeshVertices[idx + 2].Barycentric[1] = 1;

                MeshIndices[idx] = idx;
                MeshIndices[idx + 1] = idx + 1;
                MeshIndices[idx + 2] = idx + 2;
                PickingIndices[idx] = idx;
                PickingIndices[idx + 1] = idx + 1;
                PickingIndices[idx + 2] = idx + 2;

                idx += 3;
            }
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

    private unsafe void ProcessGraph(hkaiDirectedGraphExplicitCost graph)
    {
        List<Vector4> verts = graph.m_positions;
        var indexCount = 0;
        foreach (hkaiDirectedGraphExplicitCostNode g in graph.m_nodes)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += g.m_numEdges;
        }

        GraphVertexCount = indexCount * 2;
        GraphIndexCount = indexCount * 2;
        var buffersize = (uint)GraphIndexCount * 4u;
        var lsize = MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);
        var vbuffersize = (uint)GraphVertexCount * lsize;

        CostGraphGeomBuffer = Renderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)lsize, 4);
        var MeshIndices = new Span<int>(CostGraphGeomBuffer.MapIBuffer().ToPointer(), GraphIndexCount);
        var MeshVertices =
            new Span<PositionColor>(CostGraphGeomBuffer.MapVBuffer().ToPointer(), GraphVertexCount);
        var vertPos = new Vector3[indexCount * 2];

        var idx = 0;

        for (var id = 0; id < graph.m_nodes.Count; id++)
        {
            var sedge = graph.m_nodes[id].m_startEdgeIndex;
            var ecount = graph.m_nodes[id].m_numEdges;

            for (var e = 0; e < ecount; e++)
            {
                Vector4 vert1 = graph.m_positions[id];
                Vector4 vert2 =
                    graph.m_positions[(int)graph.m_edges[graph.m_nodes[id].m_startEdgeIndex + e].m_target];

                MeshVertices[idx] = new PositionColor();
                MeshVertices[idx + 1] = new PositionColor();

                MeshVertices[idx].Position = new Vector3(vert1.X, vert1.Y, vert1.Z);
                MeshVertices[idx + 1].Position = new Vector3(vert2.X, vert2.Y, vert2.Z);
                vertPos[idx] = new Vector3(vert1.X, vert1.Y, vert1.Z);
                vertPos[idx + 1] = new Vector3(vert2.X, vert2.Y, vert2.Z);

                MeshVertices[idx].Color[0] = 235;
                MeshVertices[idx].Color[1] = 200;
                MeshVertices[idx].Color[2] = 255;
                MeshVertices[idx].Color[3] = 255;
                MeshVertices[idx + 1].Color[0] = 235;
                MeshVertices[idx + 1].Color[1] = 200;
                MeshVertices[idx + 1].Color[2] = 255;
                MeshVertices[idx + 1].Color[3] = 255;

                MeshIndices[idx] = idx;
                MeshIndices[idx + 1] = idx + 1;

                idx += 2;
            }
        }

        CostGraphGeomBuffer.UnmapIBuffer();
        CostGraphGeomBuffer.UnmapVBuffer();

        if (GraphVertexCount > 0)
        {
            fixed (void* ptr = vertPos)
            {
                Bounds = BoundingBox.CreateFromPoints((Vector3*)ptr, vertPos.Count(), 12, Quaternion.Identity,
                    Vector3.Zero, Vector3.One);
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
            var mesh = HkxRoot.FindVariant<hkaiNavMesh>();
            if (mesh != null)
            {
                ProcessMesh(mesh);
            }

            var graph = HkxRoot.FindVariant<hkaiDirectedGraphExplicitCost>();
            if (graph != null)
            {
                ProcessGraph(graph);
            }
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            HkxRoot = null;
        }

        return true;
    }

    public static HavokNavmeshResource ResourceFromNavmeshRoot(hkRootLevelContainer root)
    {
        var ret = new HavokNavmeshResource();
        ret.HkxRoot = root;
        ret.LoadInternal(AccessLevel.AccessFull);
        return ret;
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
            CostGraphGeomBuffer.Dispose();

            disposedValue = true;
        }
    }

    ~HavokNavmeshResource()
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
