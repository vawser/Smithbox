using Havok;
using Havok.Files;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// A Havok navmesh (DS3, BB, SDT, ER, NR, AC6): its faces, AC6's nav volume, and the cost graph. Files are read by the
/// unified Havok library, and drawn the same way for every game through the Havok.Shared navmesh views.
/// </summary>
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

    /// <summary>The navmesh file as the unified Havok library read it, until the GPU data is built.</summary>
    public HavokDocument Document;

    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        return Read(bytes, ctx.FileName) && LoadInternal(al);
    }

    public bool _Load(string relativePath, AccessLevel al, ResourceLoadContext ctx)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        try
        {
            var fileData = curProject.VFS.FS.ReadFile(relativePath);

            if (fileData == null || !Read(fileData.Value, relativePath))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this,
                LOC.Get("REND_HavokNavmeshResource_Load_Failed", relativePath), e);
            return false;
        }

        return LoadInternal(al);
    }

    /// <summary>
    /// Reads a navmesh file. Each holds its own types.
    /// </summary>
    private bool Read(Memory<byte> bytes, string name)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;
        HavokProfile profile = HavokProfile.FromGame(curProject.Descriptor.ProjectType.ToString());

        if (profile == null)
        {
            return false;
        }

        try
        {
            var data = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;
            Document = HavokFile.Read(data.ToArray(), profile);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_HavokNavmeshResource_Load_Failed", name), e);
            return false;
        }

        return true;
    }

    /// <summary>The access level the resource was loaded with, which a refresh keeps.</summary>
    private AccessLevel LoadedAccess;

    /// <summary>
    /// Rebuilds the drawn mesh from a navmesh document held in memory, e.g. one the Navigation tool edited or generated,
    /// so the drawing follows edits that aren't saved yet. Returns the geometry buffers it replaced, for the caller to
    /// dispose once the GPU no longer uses them (a few frames on).
    /// </summary>
    public List<VertexIndexBufferAllocator.VertexIndexBufferHandle> Refresh(HavokDocument document)
    {
        var replaced = new List<VertexIndexBufferAllocator.VertexIndexBufferHandle>();
        if (GeomBuffer != null) replaced.Add(GeomBuffer);
        if (CostGraphGeomBuffer != null) replaced.Add(CostGraphGeomBuffer);

        GeomBuffer = null;
        CostGraphGeomBuffer = null;
        VertexCount = 0;
        IndexCount = 0;
        GraphVertexCount = 0;
        GraphIndexCount = 0;

        Document = document;
        LoadInternal(LoadedAccess == AccessLevel.AccessGPUOptimizedOnly ? AccessLevel.AccessGPUOptimizedOnly : AccessLevel.AccessFull);
        return replaced;
    }

    private bool LoadInternal(AccessLevel al)
    {
        LoadedAccess = al;

        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Bounds = new BoundingBox();

            var curProject = Smithbox.Orchestrator.SelectedProject;

            // DS3 and BB draw in the configured navmesh colour; the later games use a fixed one
            Vector3 color = curProject?.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB
                ? CFG.Current.Viewport_Navmesh_Color
                : new Vector3(157, 53, 255);

            IHavokNavMesh mesh = Document?.ObjectsOf<IHavokNavMesh>().FirstOrDefault();
            if (mesh != null)
            {
                ProcessMesh(CollisionGeometry.NavMeshTriangles(mesh), color);
            }

            // The flying navigation of AC6 (.nvva) is a nav volume rather than a navmesh
            if (Document?.ObjectsOf<Havok.hk2019.hkaiNavVolume>().FirstOrDefault() is { } volume)
            {
                ProcessVolume_HKX3(volume);
            }

            IHavokDirectedGraph graph = Document?.ObjectsOf<IHavokDirectedGraph>().FirstOrDefault();
            if (graph != null)
            {
                ProcessGraph(CollisionGeometry.GraphSegments(graph));
            }
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Document = null;
        }

        return true;
    }

    /// <summary>
    /// Uploads the navmesh's faces, as a triangle list (three vertices a triangle), with flat normals.
    /// </summary>
    private unsafe void ProcessMesh(List<Vector3> triangles, Vector3 color)
    {
        byte navR = (byte)color.X;
        byte navG = (byte)color.Y;
        byte navB = (byte)color.Z;
        byte navA = 255;

        // Every index gets its own vertex
        VertexCount = triangles.Count;
        IndexCount = triangles.Count;
        var buffersize = (uint)IndexCount * 4u;
        var vbuffersize = (uint)VertexCount * NavmeshLayout.SizeInBytes;
        GeomBuffer =
            SceneRenderer.GeometryBufferAllocator.Allocate(
                vbuffersize, buffersize, (int)NavmeshLayout.SizeInBytes, 4);
        PickingVertices = triangles.ToArray();
        PickingIndices = Enumerable.Range(0, IndexCount).ToArray();

        // The allocator only uploads a buffer once everything allocated in it is filled, so this is always marked
        // filled, even if reading fails, or nothing else in the buffer would be drawn
        try
        {
            var MeshIndices = new Span<int>(GeomBuffer.MapIBuffer().ToPointer(), IndexCount);
            var MeshVertices =
                new Span<NavmeshLayout>(GeomBuffer.MapVBuffer().ToPointer(), VertexCount);

            for (var idx = 0; idx < triangles.Count; idx += 3)
            {
                Vector3 n = Vector3.Normalize(Vector3.Cross(triangles[idx + 2] - triangles[idx], triangles[idx + 1] - triangles[idx]));

                for (var k = 0; k < 3; k++)
                {
                    var v = new NavmeshLayout();
                    v.Position = triangles[idx + k];
                    v.Normal[0] = (sbyte)(n.X * 127.0f);
                    v.Normal[1] = (sbyte)(n.Y * 127.0f);
                    v.Normal[2] = (sbyte)(n.Z * 127.0f);
                    v.Color[0] = navR;
                    v.Color[1] = navG;
                    v.Color[2] = navB;
                    v.Color[3] = navA;
                    MeshVertices[idx + k] = v;
                    MeshIndices[idx + k] = idx + k;
                }

                MeshVertices[idx].Barycentric[0] = 0;
                MeshVertices[idx].Barycentric[1] = 0;
                MeshVertices[idx + 1].Barycentric[0] = 1;
                MeshVertices[idx + 1].Barycentric[1] = 0;
                MeshVertices[idx + 2].Barycentric[0] = 0;
                MeshVertices[idx + 2].Barycentric[1] = 1;
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

    // The flags of nav volume vertices, kept in the unused third barycentric. Matches the NavSolid shaders
    private const byte VolumeFlag = 1;
    private const byte VolumeWallFlag = 2;

    // Floors are what stands below the open space, ceilings what hangs above it
    private static ReadOnlySpan<byte> VolumeFloorColor => [80, 220, 255];
    private static ReadOnlySpan<byte> VolumeCeilingColor => [40, 90, 200];
    private static ReadOnlySpan<byte> VolumeWallColor => [50, 160, 230];

    /// <summary>
    /// Draws the cells of a nav volume as boxes. A cell face is only drawn if the cells next to it don't cover all
    /// of it, as the faces inside the volume can't be seen and there are hundreds of thousands of cells in a map.
    /// </summary>
    private unsafe void ProcessVolume_HKX3(Havok.hk2019.hkaiNavVolume volume)
    {
        var cells = volume.m_cells;

        // Each face is the cell index * 6 + the face: the axis * 2, + 1 for the max side
        var faces = new List<int>();

        for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
        {
            for (var face = 0; face < 6; face++)
            {
                if (!IsVolumeFaceCovered(volume, cellIndex, face / 2, face % 2 == 1))
                {
                    faces.Add(cellIndex * 6 + face);
                }
            }
        }

        // Each face is a quad of 2 triangles
        VertexCount = faces.Count * 4;
        IndexCount = faces.Count * 6;
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

            // Cells are stored as quantized bounds within the volume's aabb
            Vector4 scale = volume.m_quantizationScale;
            Vector4 offset = volume.m_quantizationOffset;

            Span<float> min = stackalloc float[3];
            Span<float> max = stackalloc float[3];
            Span<float> corner = stackalloc float[3];

            // The corners of the quad, as (u, w), and their barycentrics. The quad is split on its B-D diagonal,
            // and A and C share the barycentric, so the edges of the quad are drawn by the grid in both triangles
            ReadOnlySpan<int> cornerU = [0, 1, 1, 0];
            ReadOnlySpan<int> cornerW = [0, 0, 1, 1];
            ReadOnlySpan<byte> baryX = [0, 1, 0, 0];
            ReadOnlySpan<byte> baryY = [0, 0, 0, 1];

            for (var i = 0; i < faces.Count; i++)
            {
                var cell = cells[faces[i] / 6];
                var axis = faces[i] % 6 / 2;
                var isMaxSide = faces[i] % 2 == 1;

                for (var a = 0; a < 3; a++)
                {
                    min[a] = offset[a] + cell.m_min[a] * scale[a];
                    max[a] = offset[a] + cell.m_max[a] * scale[a];
                }

                var u = (axis + 1) % 3;
                var w = (axis + 2) % 3;

                // The normal points out of the volume, into what can't be flown through
                sbyte normal = isMaxSide ? (sbyte)127 : (sbyte)-127;

                // The faces on the edge of the volume's grid only bound it, rather than showing where flight is blocked,
                // and as the volume is nearly all open space they would hide everything else. They are kept so the
                // volume is closed when it is sliced, see NavVolumeClip in NavSolid.frag
                var plane = isMaxSide ? cell.m_max[axis] : cell.m_min[axis];
                var isGridWall = plane == 0 || plane == volume.m_res[axis];

                byte flags = (byte)(VolumeFlag | (isGridWall ? VolumeWallFlag : 0));

                // Coloured by what is beyond the face, so floors, ceilings and walls can be told apart
                ReadOnlySpan<byte> color = axis != 1 ? VolumeWallColor
                    : isMaxSide ? VolumeCeilingColor
                    : VolumeFloorColor;

                var vertex = i * 4;

                for (var c = 0; c < 4; c++)
                {
                    corner[axis] = isMaxSide ? max[axis] : min[axis];
                    corner[u] = cornerU[c] == 1 ? max[u] : min[u];
                    corner[w] = cornerW[c] == 1 ? max[w] : min[w];

                    var v = new NavmeshLayout();
                    v.Position = new Vector3(corner[0], corner[1], corner[2]);

                    v.Normal[axis] = normal;

                    v.Color[0] = color[0];
                    v.Color[1] = color[1];
                    v.Color[2] = color[2];
                    v.Color[3] = 255;

                    v.Barycentric[0] = baryX[c];
                    v.Barycentric[1] = baryY[c];
                    v.Barycentric[2] = flags;

                    MeshVertices[vertex + c] = v;
                }

                // A-B-D and B-C-D
                var index = i * 6;
                MeshIndices[index] = vertex;
                MeshIndices[index + 1] = vertex + 1;
                MeshIndices[index + 2] = vertex + 3;
                MeshIndices[index + 3] = vertex + 1;
                MeshIndices[index + 4] = vertex + 2;
                MeshIndices[index + 5] = vertex + 3;
            }
        }
        finally
        {
            GeomBuffer.UnmapIBuffer();
            GeomBuffer.UnmapVBuffer();
        }

        Vector4 aabbMin = volume.m_aabb.m_min;
        Vector4 aabbMax = volume.m_aabb.m_max;

        Bounds = VertexCount > 0
            ? new BoundingBox(new Vector3(aabbMin.X, aabbMin.Y, aabbMin.Z), new Vector3(aabbMax.X, aabbMax.Y, aabbMax.Z))
            : new BoundingBox();
    }

    /// <summary>
    /// Whether the face of a nav volume cell is fully covered by the cells it connects to on that side.
    /// </summary>
    private static bool IsVolumeFaceCovered(Havok.hk2019.hkaiNavVolume volume, int cellIndex, int axis, bool isMaxSide)
    {
        var cell = volume.m_cells[cellIndex];

        var u = (axis + 1) % 3;
        var w = (axis + 2) % 3;

        long faceArea = (long)(cell.m_max[u] - cell.m_min[u]) * (cell.m_max[w] - cell.m_min[w]);
        long coveredArea = 0;

        int plane = isMaxSide ? cell.m_max[axis] : cell.m_min[axis];

        for (var e = 0; e < cell.m_numEdges; e++)
        {
            var edge = volume.m_edges[cell.m_startEdgeIndex + e];

            // These lead out of the volume, or are custom connections, rather than to a cell next to this one
            if ((edge.m_flags & (Havok.hk2019.hkaiNavVolume.CellEdgeFlagBits.EDGE_EXTERNAL_OPPOSITE
                | Havok.hk2019.hkaiNavVolume.CellEdgeFlagBits.EDGE_USER)) != 0)
            {
                continue;
            }

            if (edge.m_oppositeCell >= volume.m_cells.Count)
            {
                continue;
            }

            var opposite = volume.m_cells[(int)edge.m_oppositeCell];

            // Only the cells that touch this side of the cell cover it
            if ((isMaxSide ? opposite.m_min[axis] : opposite.m_max[axis]) != plane)
            {
                continue;
            }

            long du = Math.Min(cell.m_max[u], opposite.m_max[u]) - Math.Max(cell.m_min[u], opposite.m_min[u]);
            long dw = Math.Min(cell.m_max[w], opposite.m_max[w]) - Math.Max(cell.m_min[w], opposite.m_min[w]);

            if (du > 0 && dw > 0)
            {
                coveredArea += du * dw;
            }
        }

        // Cells don't overlap, so the areas they cover add up
        return coveredArea >= faceArea;
    }

    /// <summary>
    /// Uploads the cost graph's edges, as line segments (pairs of points).
    /// </summary>
    private unsafe void ProcessGraph(List<Vector3> segments)
    {
        GraphVertexCount = segments.Count;
        GraphIndexCount = segments.Count;
        var buffersize = (uint)GraphIndexCount * 4u;
        var lsize = MeshLayoutUtils.GetLayoutVertexSize(MeshLayoutType.LayoutPositionColor);
        var vbuffersize = (uint)GraphVertexCount * lsize;

        CostGraphGeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize, buffersize, (int)lsize, 4);
        var vertPos = segments.ToArray();

        // The allocator only uploads a buffer once everything allocated in it is filled, so this is always marked
        // filled, even if reading fails, or nothing else in the buffer would be drawn
        try
        {
            var MeshIndices = new Span<int>(CostGraphGeomBuffer.MapIBuffer().ToPointer(), GraphIndexCount);
            var MeshVertices =
                new Span<PositionColor>(CostGraphGeomBuffer.MapVBuffer().ToPointer(), GraphVertexCount);

            for (var idx = 0; idx < segments.Count; idx++)
            {
                var v = new PositionColor();
                v.Position = segments[idx];
                v.Color[0] = 235;
                v.Color[1] = 200;
                v.Color[2] = 255;
                v.Color[3] = 255;
                MeshVertices[idx] = v;
                MeshIndices[idx] = idx;
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
