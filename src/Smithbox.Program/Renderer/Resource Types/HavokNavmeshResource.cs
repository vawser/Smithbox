using HKX2;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

public class HavokNavmeshResource : IResource, IDisposable
{
    public int GraphIndexCount;

    public int IndexCount;
    public int[] PickingIndices;

    public Vector3[] PickingVertices;

    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeomBuffer { get; set; }

    public VertexIndexBufferAllocator.VertexIndexBufferHandle CostGraphGeomBuffer { get; set; }


    public int VertexCount { get; set; }
    public int GraphVertexCount { get; set; }


    public BoundingBox Bounds { get; set; }

    public HKX Root_HKX1;
    public hkRootLevelContainer Root_HKX2;
    public HKLib.hk2018.hkRootLevelContainer Root_HKX3;

    public bool _Load(Memory<byte> bytes, AccessLevel al, string virtPath)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        BinaryReaderEx br = new(false, bytes);

        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
        {
            var des = new PackFileDeserializer();
            Root_HKX2 = (hkRootLevelContainer)des.Deserialize(br);
        }

        if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            if (curProject.Handler.MapEditor == null)
                return false;

            var activeView = curProject.Handler.MapEditor.ViewHandler.ActiveView;

            if (activeView == null)
                return false;

            var pathElements = virtPath.Split('/');
            var filename = Path.GetFileNameWithoutExtension(pathElements[3]);

            if (activeView.HavokNavmeshBank.HKX3_Containers.ContainsKey(filename))
            {
                Root_HKX3 = activeView.HavokNavmeshBank.HKX3_Containers[filename];
            }
            else
            {
                return false;
            }
        }

        return LoadInternal(al);
    }

    public bool _Load(string relativePath, AccessLevel al, string virtPath)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        try
        {
            var fileData = curProject.VFS.FS.ReadFile(relativePath);

            // Intercept and load the collision from PTDE FS for DS1R projects
            if (CFG.Current.MapEditor_Use_PTDE_Collisions_In_DS1R_Projects && curProject.Descriptor.ProjectType is ProjectType.DS1R)
            {
                fileData = curProject.VFS.PTDE_FS.ReadFile(relativePath);
            }

            if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
            {
                var des = new PackFileDeserializer();
                Root_HKX2 = (hkRootLevelContainer)des.Deserialize(new BinaryReaderEx(false, fileData.Value));
            }
            else if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (curProject.Handler.MapEditor == null)
                    return false;

                var activeView = curProject.Handler.MapEditor.ViewHandler.ActiveView;

                if (activeView == null)
                    return false;

                var pathElements = virtPath.Split('/');
                var filename = Path.GetFileNameWithoutExtension(pathElements[3]);

                if (activeView.HavokNavmeshBank.HKX3_Containers.ContainsKey(filename))
                {
                    Root_HKX3 = activeView.HavokNavmeshBank.HKX3_Containers[filename];
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Smithbox] Failed to load {relativePath} during HavokCollisionResource load.", LogPriority.High, e);
        }

        return LoadInternal(al);
    }

    private bool LoadInternal(AccessLevel al)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
            {
                Bounds = new BoundingBox();

                if (Root_HKX2 != null)
                {
                    var mesh = Root_HKX2.FindVariant<hkaiNavMesh>();
                    if (mesh != null)
                    {
                        ProcessMesh_HKX2(mesh);
                    }

                    var graph = Root_HKX2.FindVariant<hkaiDirectedGraphExplicitCost>();
                    if (graph != null)
                    {
                        ProcessGraph_HKX2(graph);
                    }
                }
            }
        }

        if (curProject.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
            {
                Bounds = new BoundingBox();

                if (Root_HKX3 != null)
                {
                    var mesh = FindMeshVariant(Root_HKX3);
                    if (mesh != null)
                    {
                        ProcessMesh_HKX3(mesh);
                    }

                    var graph = FindGraphVariant(Root_HKX3);
                    if (graph != null)
                    {
                        ProcessGraph_HKX3(graph);
                    }
                }
            }
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Root_HKX1 = null;
            Root_HKX2 = null;
            Root_HKX3 = null;
        }

        return true;
    }

    private HKLib.hk2018.hkaiNavMesh FindMeshVariant(HKLib.hk2018.hkRootLevelContainer container)
    {
        foreach (var v in container.m_namedVariants)
        {
            if (v.m_className == typeof(HKLib.hk2018.hkaiNavMesh).Name)
            {
                return (HKLib.hk2018.hkaiNavMesh)v.m_variant;
            }
        }
        return null;
    }

    private HKLib.hk2018.hkaiDirectedGraphExplicitCost FindGraphVariant(HKLib.hk2018.hkRootLevelContainer container)
    {
        foreach (var v in container.m_namedVariants)
        {
            if (v.m_className == typeof(HKLib.hk2018.hkaiDirectedGraphExplicitCost).Name)
            {
                return (HKLib.hk2018.hkaiDirectedGraphExplicitCost)v.m_variant;
            }
        }
        return null;
    }

    private unsafe void ProcessMesh_HKX2(hkaiNavMesh mesh)
    {
        byte navR = (byte)CFG.Current.Viewport_Navmesh_Color.X;
        byte navG = (byte)CFG.Current.Viewport_Navmesh_Color.Y;
        byte navB = (byte)CFG.Current.Viewport_Navmesh_Color.Z;
        byte navA = 255;

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
            SceneRenderer.GeometryBufferAllocator.Allocate(
                vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);
        var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
        var MeshVertices =
            new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);
        PickingVertices = new Vector3[VertexCount];
        PickingIndices = new int[IndexCount];

        ResourceFactory factory = SceneRenderer.Factory;

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

                MeshVertices[idx].Color[0] = navR;
                MeshVertices[idx].Color[1] = navG;
                MeshVertices[idx].Color[2] = navB;
                MeshVertices[idx].Color[3] = navA;
                MeshVertices[idx + 1].Color[0] = navR;
                MeshVertices[idx + 1].Color[1] = navG;
                MeshVertices[idx + 1].Color[2] = navB;
                MeshVertices[idx + 1].Color[3] = navA;
                MeshVertices[idx + 2].Color[0] = navR;
                MeshVertices[idx + 2].Color[1] = navG;
                MeshVertices[idx + 2].Color[2] = navB;
                MeshVertices[idx + 2].Color[3] = navA;

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


    private unsafe void ProcessGraph_HKX2(hkaiDirectedGraphExplicitCost graph)
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

        CostGraphGeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)lsize, 4);
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


    private unsafe void ProcessMesh_HKX3(HKLib.hk2018.hkaiNavMesh mesh)
    {
        List<Vector4> verts = mesh.m_vertices;
        var indexCount = 0;

        foreach (HKLib.hk2018.hkaiNavMesh.Face f in mesh.m_faces)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += (f.m_numEdges - 2) * 3;
        }

        VertexCount = indexCount * 3;
        IndexCount = indexCount * 3;
        var buffersize = (uint)IndexCount * 4u;
        var vbuffersize = (uint)VertexCount * NavmeshLayout.SizeInBytes;
        GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(
                vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);
        var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
        var MeshVertices =
            new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);
        PickingVertices = new Vector3[VertexCount];
        PickingIndices = new int[IndexCount];

        ResourceFactory factory = SceneRenderer.Factory;

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

    private unsafe void ProcessGraph_HKX3(HKLib.hk2018.hkaiDirectedGraphExplicitCost graph)
    {
        List<Vector4> verts = graph.m_positions;
        var indexCount = 0;
        foreach (HKLib.hk2018.hkaiDirectedGraphExplicitCost.Node g in graph.m_nodes)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += g.m_numEdges;
        }

        GraphVertexCount = indexCount * 2;
        GraphIndexCount = indexCount * 2;
        var buffersize = (uint)GraphIndexCount * 4u;
        var lsize = MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);
        var vbuffersize = (uint)GraphVertexCount * lsize;

        CostGraphGeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)lsize, 4);
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

    /// <summary>
    /// Navmesh Builder
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public static HavokNavmeshResource ResourceFromNavmeshRoot(hkRootLevelContainer root)
    {
        var ret = new HavokNavmeshResource();
        ret.Root_HKX2 = root;
        ret.LoadInternal(AccessLevel.AccessFull);
        return ret;
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

            if(GeomBuffer != null)
                GeomBuffer.Dispose();

            if (CostGraphGeomBuffer != null)
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
