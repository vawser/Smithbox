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
    public HKLib.hk2019.hkRootLevelContainer Root_HKX3_2019;

    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        BinaryReaderEx br = new(false, bytes);

        if (curProject.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
        {
            var des = new PackFileDeserializer();
            Root_HKX2 = (hkRootLevelContainer)des.Deserialize(br);
        }

        if (!HandleHKLibLoad(curProject, bytes, ctx))
        {
            return false;
        }

        return LoadInternal(al);
    }

    public bool _Load(string relativePath, AccessLevel al, ResourceLoadContext ctx)
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
            else if (!HandleHKLibLoad(curProject, fileData.Value, ctx))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this,
                LOC.Get("REND_HavokNavmeshResource_Load_Failed", relativePath), e);
        }

        return LoadInternal(al);
    }

    /// <summary>
    /// Reads the navmesh of the projects that use HKLib: Havok 2018 for ER, Havok 2019 for AC6.
    /// Each file holds its own types. Other projects have nothing to read here.
    /// </summary>
    private bool HandleHKLibLoad(ProjectEntry curProject, Memory<byte> bytes, ResourceLoadContext ctx)
    {
        try
        {
            if (curProject.Descriptor.ProjectType is ProjectType.AC6)
            {
                Root_HKX3_2019 = HKLibTagfileReader.Read2019(bytes);
                return Root_HKX3_2019 != null;
            }

            if (curProject.Descriptor.ProjectType is ProjectType.ER
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                Root_HKX3 = HKLibTagfileReader.Read2018(bytes);
                return Root_HKX3 != null;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_HavokNavmeshResource_Load_Failed", ctx.FileName), e);
            return false;
        }

        return true;
    }

    private bool LoadInternal(AccessLevel al)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
            {
                Bounds = new BoundingBox();

                if (Root_HKX3_2019 != null)
                {
                    var variants = Root_HKX3_2019.m_namedVariants.Select(v => v.m_variant).ToList();

                    if (variants.OfType<HKLib.hk2019.hkaiNavMesh>().FirstOrDefault() is { } mesh)
                    {
                        ProcessMesh_HKX3(mesh);
                    }

                    if (variants.OfType<HKLib.hk2019.hkaiDirectedGraphExplicitCost>().FirstOrDefault() is { } graph)
                    {
                        ProcessGraph_HKX3(graph);
                    }
                }
            }
        }

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

        if (curProject.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
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
            Root_HKX3_2019 = null;
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

        // indexCount already counts each triangle's 3 indices, and every index gets its own vertex
        VertexCount = indexCount;
        IndexCount = indexCount;
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


    private void ProcessMesh_HKX3(HKLib.hk2018.hkaiNavMesh mesh)
    {
        ProcessMesh_HKX3(mesh.m_vertices,
            mesh.m_faces.Select(f => new NavmeshFace(f.m_startEdgeIndex, f.m_numEdges, f.m_clusterIndex)).ToList(),
            mesh.m_edges.Select(e => e.m_a).ToList());
    }

    private void ProcessMesh_HKX3(HKLib.hk2019.hkaiNavMesh mesh)
    {
        ProcessMesh_HKX3(mesh.m_vertices,
            mesh.m_faces.Select(f => new NavmeshFace(f.m_startEdgeIndex, f.m_numEdges, f.m_clusterIndex)).ToList(),
            mesh.m_edges.Select(e => e.m_a).ToList());
    }

    /// <summary>
    /// The parts of an hkaiNavMesh face used to draw it, so 2018 and 2019 navmeshes are drawn the same way
    /// </summary>
    private record struct NavmeshFace(int StartEdgeIndex, int NumEdges, int ClusterIndex);

    /// <param name="edgeVertices">The vertex index each edge starts at</param>
    private unsafe void ProcessMesh_HKX3(List<Vector4> verts, List<NavmeshFace> faces, List<int> edgeVertices)
    {
        var indexCount = 0;

        foreach (NavmeshFace f in faces)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += Math.Max(f.NumEdges - 2, 0) * 3;
        }

        // indexCount already counts each triangle's 3 indices, and every index gets its own vertex
        VertexCount = indexCount;
        IndexCount = indexCount;
        var buffersize = (uint)IndexCount * 4u;
        var vbuffersize = (uint)VertexCount * NavmeshLayout.SizeInBytes;
        GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(
                vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);

        // The allocator only uploads a buffer once everything allocated in it is filled, so this is always marked
        // filled, even if reading fails, or nothing else in the buffer would be drawn
        try
        {
            var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
            var MeshVertices =
                new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);
            PickingVertices = new Vector3[VertexCount];
            PickingIndices = new int[IndexCount];

            ResourceFactory factory = SceneRenderer.Factory;

            var idx = 0;

            var maxcluster = 0;

            for (var id = 0; id < faces.Count; id++)
            {
                if (faces[id].ClusterIndex > maxcluster)
                {
                    maxcluster = faces[id].ClusterIndex;
                }

                var sedge = faces[id].StartEdgeIndex;
                var ecount = faces[id].NumEdges;

                // Use simple algorithm for convex polygon trianglization
                for (var t = 0; t < ecount - 2; t++)
                {
                    if (ecount > 3)
                    {
                        //ecount = ecount;
                    }

                    var end = t + 2 >= ecount ? sedge : sedge + t + 2;
                    Vector4 vert1 = verts[edgeVertices[sedge]];
                    Vector4 vert2 = verts[edgeVertices[sedge + t + 1]];
                    Vector4 vert3 = verts[edgeVertices[end]];

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
        }
        finally
        {
            GeomBuffer.UnmapIBuffer();
            GeomBuffer.UnmapVBuffer();
        }

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

    private void ProcessGraph_HKX3(HKLib.hk2018.hkaiDirectedGraphExplicitCost graph)
    {
        ProcessGraph_HKX3(graph.m_positions,
            graph.m_nodes.Select(n => new NavmeshGraphNode(n.m_startEdgeIndex, n.m_numEdges)).ToList(),
            graph.m_edges.Select(e => (int)e.m_target).ToList());
    }

    private void ProcessGraph_HKX3(HKLib.hk2019.hkaiDirectedGraphExplicitCost graph)
    {
        ProcessGraph_HKX3(graph.m_positions,
            graph.m_nodes.Select(n => new NavmeshGraphNode(n.m_startEdgeIndex, n.m_numEdges)).ToList(),
            graph.m_edges.Select(e => (int)e.m_target).ToList());
    }

    /// <summary>
    /// The parts of a cost graph node used to draw it, so 2018 and 2019 graphs are drawn the same way
    /// </summary>
    private record struct NavmeshGraphNode(int StartEdgeIndex, int NumEdges);

    /// <param name="edgeTargets">The node each edge leads to</param>
    private unsafe void ProcessGraph_HKX3(List<Vector4> positions, List<NavmeshGraphNode> nodes, List<int> edgeTargets)
    {
        List<Vector4> verts = positions;
        var indexCount = 0;
        foreach (NavmeshGraphNode g in nodes)
        {
            // Simple formula for indices count for a triangulation of a poly
            indexCount += g.NumEdges;
        }

        GraphVertexCount = indexCount * 2;
        GraphIndexCount = indexCount * 2;
        var buffersize = (uint)GraphIndexCount * 4u;
        var lsize = MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);
        var vbuffersize = (uint)GraphVertexCount * lsize;

        CostGraphGeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)lsize, 4);
        var vertPos = new Vector3[indexCount * 2];

        // The allocator only uploads a buffer once everything allocated in it is filled, so this is always marked
        // filled, even if reading fails, or nothing else in the buffer would be drawn
        try
        {
            var MeshIndices = new Span<int>(CostGraphGeomBuffer.MapIBuffer().ToPointer(), GraphIndexCount);
            var MeshVertices =
                new Span<PositionColor>(CostGraphGeomBuffer.MapVBuffer().ToPointer(), GraphVertexCount);

            var idx = 0;

            for (var id = 0; id < nodes.Count; id++)
            {
                var sedge = nodes[id].StartEdgeIndex;
                var ecount = nodes[id].NumEdges;

                for (var e = 0; e < ecount; e++)
                {
                    Vector4 vert1 = positions[id];
                    Vector4 vert2 =
                        positions[edgeTargets[nodes[id].StartEdgeIndex + e]];

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
        }
        finally
        {
            CostGraphGeomBuffer.UnmapIBuffer();
            CostGraphGeomBuffer.UnmapVBuffer();
        }

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
