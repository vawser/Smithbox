using Havok.hk2018;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Vertex editing of navmeshes (the Navigation tool's Edit tab). A vertex edit keeps the topology: faces, edges, face and
// edge data, user edges and every index that connections, node banks and the clearance cache use stay as they are.
// What follows the vertices on commit is the mesh's AABB (keeping its padding), the query mediator's face tree, and the
// clearance cache around the moved vertices (NavClearance; its layout depends only on the topology, so only values
// change). No renderer dependencies, so NavSurvey links it (vertexedit).

/// <summary>
/// The vertices of one navmesh file being edited: a working copy of its positions, in the navmesh's own space, placed in
/// the viewport by its entry's world transform. The file's document isn't changed until the edit is committed.
/// </summary>
public sealed class NavVertexFile
{
    public NVA_ER.Navmesh Entry { get; }

    public NavmeshFile File { get; }

    /// <summary>The positions as read, and as edited.</summary>
    public Vector3[] Original { get; }
    public Vector3[] Vertices { get; }

    public Matrix4x4 World { get; private set; } = Matrix4x4.Identity;
    private Matrix4x4 Inverse = Matrix4x4.Identity;

    /// <summary>Bumped by every move, so a preview knows when to be drawn again.</summary>
    public int Revision { get; private set; }

    /// <summary>The vertices a move has reached, so the moved ones are counted without a scan of the whole mesh.</summary>
    private readonly HashSet<int> Touched = new();

    private Vector3 LocalMin, LocalMax;
    private bool BoundsDirty = true;

    public NavVertexFile(NVA_ER.Navmesh entry, NavmeshFile file, Matrix4x4 world)
    {
        Entry = entry;
        File = file;

        var mesh = file.Mesh;
        Original = new Vector3[mesh.m_vertices.Count];

        for (int i = 0; i < Original.Length; i++)
        {
            var v = mesh.m_vertices[i];
            Original[i] = new Vector3(v.X, v.Y, v.Z);
        }

        Vertices = (Vector3[])Original.Clone();
        SetWorld(world);
    }

    public hkaiNavMesh Mesh => File.Mesh;

    /// <summary>Places the file in the viewport; false if the transform can't be inverted (it is then left as it was).</summary>
    public bool SetWorld(Matrix4x4 world)
    {
        if (!Matrix4x4.Invert(world, out var inverse))
            return false;

        World = world;
        Inverse = inverse;
        return true;
    }

    public int MovedCount => Touched.Count(i => Vertices[i] != Original[i]);

    public bool IsEdited => Touched.Any(i => Vertices[i] != Original[i]);

    public Vector3 WorldVertex(int index) => Vector3.Transform(Vertices[index], World);

    /// <summary>The vertex index of each corner of a face, in order.</summary>
    public IEnumerable<int> FaceCorners(int face)
    {
        var f = Mesh.m_faces[face];
        for (int k = f.m_startEdgeIndex; k < f.m_startEdgeIndex + f.m_numEdges; k++)
            yield return Mesh.m_edges[k].m_a;
    }

    private void UpdateBounds()
    {
        if (!BoundsDirty)
            return;

        BoundsDirty = false;
        LocalMin = new Vector3(float.MaxValue);
        LocalMax = new Vector3(float.MinValue);

        foreach (var v in Vertices)
        {
            LocalMin = Vector3.Min(LocalMin, v);
            LocalMax = Vector3.Max(LocalMax, v);
        }
    }

    /// <summary>The box around the edited vertices in the viewport, from the corners of their local box.</summary>
    public (Vector3 Min, Vector3 Max) WorldBounds()
    {
        UpdateBounds();

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (int i = 0; i < 8; i++)
        {
            var c = new Vector3((i & 1) != 0 ? LocalMax.X : LocalMin.X, (i & 2) != 0 ? LocalMax.Y : LocalMin.Y, (i & 4) != 0 ? LocalMax.Z : LocalMin.Z);
            var w = Vector3.Transform(c, World);
            min = Vector3.Min(min, w);
            max = Vector3.Max(max, w);
        }

        return (min, max);
    }

    /// <summary>
    /// The nearest face under a ray in the viewport (both sides), with the distance to it and where it is hit, in the
    /// viewport's units. Uses the edited vertices.
    /// </summary>
    public bool RayCast(Vector3 origin, Vector3 direction, out int face, out float distance, out Vector3 point)
    {
        face = -1;
        distance = float.MaxValue;
        point = default;

        if (Vertices.Length == 0 || Mesh.m_faces.Count == 0)
            return false;

        var localOrigin = Vector3.Transform(origin, Inverse);
        var localDirection = Vector3.TransformNormal(direction, Inverse);

        UpdateBounds();

        if (!RayHitsBox(localOrigin, localDirection, LocalMin - new Vector3(0.01f), LocalMax + new Vector3(0.01f)))
            return false;

        float best = float.MaxValue;

        for (int f = 0; f < Mesh.m_faces.Count; f++)
        {
            var mf = Mesh.m_faces[f];
            if (mf.m_numEdges < 3)
                continue;

            var a = Vertices[Mesh.m_edges[mf.m_startEdgeIndex].m_a];

            for (int k = mf.m_startEdgeIndex + 1; k + 1 < mf.m_startEdgeIndex + mf.m_numEdges; k++)
            {
                var b = Vertices[Mesh.m_edges[k].m_a];
                var c = Vertices[Mesh.m_edges[k + 1].m_a];

                if (NavigationOverlayBuilder.RayTriangle(localOrigin, localDirection, a, b, c, out float t) && t < best)
                {
                    best = t;
                    face = f;
                }
            }
        }

        if (face < 0)
            return false;

        point = Vector3.Transform(localOrigin + localDirection * best, World);
        distance = Vector3.Distance(origin, point);
        return true;
    }

    private static bool RayHitsBox(Vector3 origin, Vector3 direction, Vector3 min, Vector3 max)
    {
        float tMin = 0, tMax = float.MaxValue;

        for (int axis = 0; axis < 3; axis++)
        {
            float o = origin[axis], d = direction[axis], lo = min[axis], hi = max[axis];

            if (MathF.Abs(d) < 1e-12f)
            {
                if (o < lo || o > hi)
                    return false;
                continue;
            }

            float t1 = (lo - o) / d, t2 = (hi - o) / d;
            if (t1 > t2)
                (t1, t2) = (t2, t1);

            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);

            if (tMin > tMax)
                return false;
        }

        return true;
    }

    /// <summary>The corner of a face nearest a point in the viewport.</summary>
    public int NearestCorner(int face, Vector3 worldPoint)
    {
        int best = -1;
        float bestDistance = float.MaxValue;

        foreach (var v in FaceCorners(face))
        {
            float d = Vector3.DistanceSquared(WorldVertex(v), worldPoint);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = v;
            }
        }

        return best;
    }

    /// <summary>
    /// The vertices that follow an edit at a point in the viewport, with their weights: those within the inner distance
    /// follow fully, then the radius fades them smoothly out. The distances are in the viewport's units.
    /// </summary>
    public List<(int Vertex, float Weight)> Falloff(Vector3 worldPoint, float inner, float radius)
    {
        var result = new List<(int, float)>();
        radius = Math.Max(0f, radius);
        inner = Math.Max(0f, inner);
        float reach = inner + radius;

        // Nothing of the file is near enough
        var (min, max) = WorldBounds();
        var nearest = Vector3.Clamp(worldPoint, min, max);
        if (Vector3.DistanceSquared(nearest, worldPoint) > reach * reach)
            return result;

        for (int v = 0; v < Vertices.Length; v++)
        {
            float d = Vector3.Distance(WorldVertex(v), worldPoint);

            if (d <= inner)
            {
                result.Add((v, 1f));
            }
            else if (radius > 0 && d < reach)
            {
                // Smoothstep from the edge of the radius in to the inner distance
                float x = 1f - (d - inner) / radius;
                result.Add((v, x * x * (3f - 2f * x)));
            }
        }

        return result;
    }

    /// <summary>Moves vertices by an offset in the viewport, scaled by their weights.</summary>
    public void MoveWorld(List<(int Vertex, float Weight)> vertices, Vector3 worldOffset)
    {
        if (vertices.Count == 0)
            return;

        var local = Vector3.TransformNormal(worldOffset, Inverse);

        foreach (var (v, weight) in vertices)
        {
            Vertices[v] += local * weight;
            Touched.Add(v);
        }

        BoundsDirty = true;
        Revision++;
    }

    /// <summary>Puts every vertex back where it was read.</summary>
    public void Reset()
    {
        Array.Copy(Original, Vertices, Original.Length);
        Touched.Clear();
        BoundsDirty = true;
        Revision++;
    }

    /// <summary>
    /// A mesh to draw the edit with: the file's faces, edges and data, with the edited vertices. It shares the file's
    /// lists, so it must only be read.
    /// </summary>
    public hkaiNavMesh PreviewMesh()
    {
        var mesh = Mesh;
        var vertices = new List<Vector4>(Vertices.Length);

        for (int i = 0; i < Vertices.Length; i++)
            vertices.Add(new Vector4(Vertices[i], mesh.m_vertices[i].W));

        return new hkaiNavMesh
        {
            m_faces = mesh.m_faces,
            m_edges = mesh.m_edges,
            m_vertices = vertices,
            m_faceData = mesh.m_faceData,
            m_edgeData = mesh.m_edgeData,
            m_faceDataStriding = mesh.m_faceDataStriding,
            m_edgeDataStriding = mesh.m_edgeDataStriding,
        };
    }
}

public static class NavVertexEdit
{
    /// <summary>
    /// A copy of a navmesh file with its vertices moved to new positions (in the navmesh's space, one per vertex): the
    /// mesh's AABB is moved with them, keeping the padding it had around its vertices, and the face tree of each query
    /// mediator of the mesh is built again. Faces, edges and data are kept; the clearance cache is worked out again around the moved vertices. Null if the file has no
    /// mesh, or the positions don't match its vertices.
    /// </summary>
    public static Havok.HavokDocument Build(Havok.HavokDocument document, IReadOnlyList<Vector3> vertices)
        => Build(document, vertices, out _);

    /// <summary>
    /// As <see cref="Build(Havok.HavokDocument, IReadOnlyList{Vector3})"/>, and the clearance cache is worked out again
    /// around the moved vertices (NavClearance), with what that changed.
    /// </summary>
    public static Havok.HavokDocument Build(Havok.HavokDocument document, IReadOnlyList<Vector3> vertices, out NavClearanceResult clearance)
    {
        clearance = new NavClearanceResult();

        var copy = NavModelEdit.Clone(document);
        var mesh = copy.ObjectsOf<hkaiNavMesh>().FirstOrDefault();

        if (mesh == null || mesh.m_vertices.Count != vertices.Count)
            return null;

        var before = mesh.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
        var moved = new HashSet<int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] != before[i])
                moved.Add(i);
        }

        if (vertices.Count > 0)
        {
            var (oldMin, oldMax) = Bounds(mesh.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)));
            var padMin = Vector3.Max(Vector3.Zero, oldMin - Xyz(mesh.m_aabb.m_min));
            var padMax = Vector3.Max(Vector3.Zero, Xyz(mesh.m_aabb.m_max) - oldMax);

            for (int i = 0; i < vertices.Count; i++)
                mesh.m_vertices[i] = new Vector4(vertices[i], mesh.m_vertices[i].W);

            // An axis whose bound didn't move keeps the game's value exactly, rather than one rounded through the padding
            var (min, max) = Bounds(vertices);
            var aabbMin = Xyz(mesh.m_aabb.m_min);
            var aabbMax = Xyz(mesh.m_aabb.m_max);

            for (int axis = 0; axis < 3; axis++)
            {
                if (min[axis] != oldMin[axis])
                    aabbMin[axis] = min[axis] - padMin[axis];
                if (max[axis] != oldMax[axis])
                    aabbMax[axis] = max[axis] + padMax[axis];
            }

            mesh.m_aabb.m_min = new Vector4(aabbMin, mesh.m_aabb.m_min.W);
            mesh.m_aabb.m_max = new Vector4(aabbMax, mesh.m_aabb.m_max.W);
        }

        foreach (var mediator in copy.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>())
        {
            var tree = mediator.m_tree?.m_treePtr;

            if (tree == null || !ReferenceEquals(mediator.m_navMesh, mesh) || mesh.m_faces.Count == 0)
                continue;

            tree.m_tree = NavigationTree.Build(mesh);
        }

        clearance = NavClearance.Recalculate(mesh, before, moved);

        return copy;
    }

    private static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    private static (Vector3 Min, Vector3 Max) Bounds(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var p in points)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return (min, max);
    }
}
