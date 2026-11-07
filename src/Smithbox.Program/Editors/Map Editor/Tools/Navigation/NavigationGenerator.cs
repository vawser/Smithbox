using DotRecast.Core.Numerics;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using Havok.hk2018;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Generating a navmesh from collision (Navigation Editor Plan, Phase 4; Navmesh Generation Plan): Recast (DotRecast)
// voxelises the collision's walkable surfaces into convex polygons, which become an hkaiNavMesh laid out as the game's
// are, with a face tree for its query mediator. Kept free of the rest of Smithbox; NavSurvey `navgen` runs it on the
// game's collision and compares the result with the game's own navmeshes.

/// <summary>The agent and voxel settings a navmesh is generated with (Recast's, in metres and degrees).</summary>
public sealed record NavGenParameters
{
    /// <summary>The horizontal size of a voxel.</summary>
    public float CellSize { get; init; } = 0.2f;

    /// <summary>The vertical size of a voxel.</summary>
    public float CellHeight { get; init; } = 0.1f;

    /// <summary>The steepest slope a character walks up.</summary>
    public float MaxSlope { get; init; } = 60f;

    /// <summary>The headroom a character needs.</summary>
    public float AgentHeight { get; init; } = 1.8f;

    /// <summary>How far the navmesh is kept from walls and ledges.</summary>
    public float AgentRadius { get; init; } = 0.1f;

    /// <summary>The highest step a character climbs.</summary>
    public float MaxClimb { get; init; } = 0.8f;

    /// <summary>Islands smaller than this many square metres are dropped.</summary>
    public float MinRegionArea { get; init; } = 2f;

    /// <summary>Regions smaller than this many square metres are merged into their neighbours.</summary>
    public float MergeRegionArea { get; init; } = 20f;

    /// <summary>The longest a boundary edge may be; 0 for no limit.</summary>
    public float MaxEdgeLength { get; init; } = 8f;

    /// <summary>How far a simplified boundary may stray from the voxels' outline.</summary>
    public float MaxEdgeError { get; init; } = 1.0f;

    /// <summary>The most corners a face may have (3 to 6).</summary>
    public int MaxVerticesPerFace { get; init; } = 6;

    /// <summary>Move each vertex onto the collision surface below or just above it, as the game's sit.</summary>
    public bool SnapToCollision { get; init; } = true;

    /// <summary>
    /// Defaults for the n set (normal characters): a fine mesh close to the collision. Tuned with NavSurvey navgen on
    /// m10_01 and m12_01, whose walkable surfaces are all map collision: it covers 87% (median) of the game's n meshes.
    /// </summary>
    public static NavGenParameters DefaultN => new();

    /// <summary>Defaults for the o set (large characters): a coarser mesh. Covers 66% (median) of the game's o meshes.</summary>
    public static NavGenParameters DefaultO => new()
    {
        CellSize = 0.3f,
        AgentHeight = 2.5f,
        AgentRadius = 0.3f,
        MaxClimb = 1.2f,
        MinRegionArea = 6f,
        MaxEdgeLength = 12f,
    };
}

/// <summary>A generated navmesh, or why there is none.</summary>
public sealed class NavGenResult
{
    public hkaiNavMesh Mesh { get; init; }

    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; init; }

    public int InputTriangles { get; init; }

    public int SnappedVertices { get; init; }

    public bool Ok => Error == null;
}

public static class NavigationGenerator
{
    /// <summary>Recast's index for a missing vertex or neighbour.</summary>
    private const int Null = 0xFFFF;

    /// <summary>
    /// Generates a navmesh from triangles (three points each, in the space the navmesh is to have). The template gives
    /// the per-face and per-mesh values the game's files carry (cluster index, padding, flags, AABB w); faces get no face
    /// data, edges flags 4 (ORIGINAL) and no edge data, as the game's do.
    /// </summary>
    public static NavGenResult Generate(IReadOnlyList<Vector3> triangles, NavGenParameters p, hkaiNavMesh template = null)
    {
        if (triangles.Count < 3)
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_No_Collision" };

        var vertices = new float[triangles.Count * 3];
        var faces = new int[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
        {
            vertices[i * 3] = triangles[i].X;
            vertices[i * 3 + 1] = triangles[i].Y;
            vertices[i * 3 + 2] = triangles[i].Z;
            faces[i] = i;
        }

        var geometry = new RcSampleInputGeomProvider(vertices, faces);

        // The Recast settings in voxels; the area settings are in squared voxels
        int regionMin = Math.Max(1, (int)MathF.Round(MathF.Sqrt(p.MinRegionArea) / p.CellSize));
        int regionMerge = Math.Max(1, (int)MathF.Round(MathF.Sqrt(p.MergeRegionArea) / p.CellSize));
        var config = new RcConfig(RcPartition.WATERSHED, p.CellSize, p.CellHeight, p.MaxSlope, p.AgentHeight, p.AgentRadius, p.MaxClimb,
            regionMin, regionMerge, p.MaxEdgeLength, p.MaxEdgeError, Math.Clamp(p.MaxVerticesPerFace, 3, 6), 6f, 1f,
            true, true, true, new RcAreaModification(RcRecast.RC_WALKABLE_AREA), false);

        RcBuilderResult built;
        try
        {
            built = new RcBuilder().Build(geometry, new RcBuilderConfig(config, geometry.GetMeshBoundsMin(), geometry.GetMeshBoundsMax()), false);
        }
        catch (Exception)
        {
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_Recast", InputTriangles = triangles.Count / 3 };
        }

        var poly = built?.Mesh;
        if (poly == null || poly.npolys == 0)
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_Empty", InputTriangles = triangles.Count / 3 };

        // Vertices, from voxel coordinates
        var points = new Vector3[poly.nverts];
        for (int v = 0; v < poly.nverts; v++)
            points[v] = new Vector3(poly.bmin.X + poly.verts[v * 3] * poly.cs, poly.bmin.Y + poly.verts[v * 3 + 1] * poly.ch, poly.bmin.Z + poly.verts[v * 3 + 2] * poly.cs);

        int snapped = 0;
        if (p.SnapToCollision)
        {
            var surface = new SurfaceGrid(triangles, 4f);
            for (int v = 0; v < points.Length; v++)
            {
                if (surface.Nearest(points[v], p.MaxClimb + 2 * p.CellHeight) is float y)
                {
                    points[v].Y = y;
                    snapped++;
                }
            }
        }

        var mesh = ToNavMesh(poly, points, template);
        return new NavGenResult { Mesh = mesh, InputTriangles = triangles.Count / 3, SnappedVertices = snapped };
    }

    /// <summary>
    /// The polygons as an hkaiNavMesh: each face's corners wound counter-clockwise seen from above, as the game's are;
    /// each edge from its corner to the next; inner edges named by the edge and face on their other side.
    /// </summary>
    private static hkaiNavMesh ToNavMesh(RcPolyMesh poly, Vector3[] points, hkaiNavMesh template)
    {
        var templateFace = template?.m_faces.FirstOrDefault();
        float w = template?.m_vertices.FirstOrDefault().W ?? 1f;

        var mesh = new hkaiNavMesh
        {
            m_faceDataStriding = 1,
            m_edgeDataStriding = 1,
            m_flags = template?.m_flags ?? 0,
            m_erosionRadius = 0,
            m_userData = template?.m_userData ?? 0,
        };

        foreach (var point in points)
            mesh.m_vertices.Add(new Vector4(point, w));

        var byVertices = new Dictionary<(int, int), int>();
        var edgeFace = new List<int>();

        for (int i = 0; i < poly.npolys; i++)
        {
            int start = i * 2 * poly.nvp;
            var corners = new List<int>();
            for (int k = 0; k < poly.nvp && poly.polys[start + k] != Null; k++)
                corners.Add(poly.polys[start + k]);

            if (corners.Count < 3)
                continue;

            // Counter-clockwise from above: the normal of the corners points up
            var normal = Vector3.Zero;
            for (int k = 1; k + 1 < corners.Count; k++)
                normal += Vector3.Cross(points[corners[k]] - points[corners[0]], points[corners[k + 1]] - points[corners[0]]);
            if (normal.Y < 0)
                corners.Reverse();

            var face = new hkaiNavMesh.Face
            {
                m_startEdgeIndex = mesh.m_edges.Count,
                m_numEdges = (short)corners.Count,
                m_startUserEdgeIndex = -1,
                m_numUserEdges = 0,
                m_clusterIndex = templateFace?.m_clusterIndex ?? -1,
                m_padding = templateFace?.m_padding ?? 0,
            };

            int faceIndex = mesh.m_faces.Count;
            mesh.m_faces.Add(face);
            mesh.m_faceData.Add(0);

            for (int k = 0; k < corners.Count; k++)
            {
                int a = corners[k], b = corners[(k + 1) % corners.Count];
                byVertices[(a, b)] = mesh.m_edges.Count;
                mesh.m_edges.Add(new hkaiNavMesh.Edge
                {
                    m_a = a,
                    m_b = b,
                    m_oppositeEdge = 0xFFFFFFFF,
                    m_oppositeFace = 0xFFFFFFFF,
                    m_flags = hkaiNavMesh.EdgeFlagBits.EDGE_ORIGINAL,
                    m_userEdgeCost = 0,
                });
                mesh.m_edgeData.Add(0);
                edgeFace.Add(faceIndex);
            }
        }

        // Inner edges: the same two vertices the other way round
        for (int e = 0; e < mesh.m_edges.Count; e++)
        {
            var edge = mesh.m_edges[e];
            if (byVertices.TryGetValue((edge.m_b, edge.m_a), out int other) && other != e)
            {
                edge.m_oppositeEdge = (uint)other;
                edge.m_oppositeFace = (uint)edgeFace[other];
            }
        }

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        mesh.m_aabb = new hkAabb
        {
            m_min = new Vector4(min, template?.m_aabb.m_min.W ?? 1f),
            m_max = new Vector4(max, template?.m_aabb.m_max.W ?? 1f),
        };

        return mesh;
    }

    /// <summary>Triangles (three points each) bucketed on an XZ grid, for the surface height under a point.</summary>
    private sealed class SurfaceGrid
    {
        private readonly IReadOnlyList<Vector3> T;
        private readonly float Cell;
        private readonly Dictionary<(int, int), List<int>> Cells = new();

        public SurfaceGrid(IReadOnlyList<Vector3> triangles, float cell)
        {
            T = triangles;
            Cell = cell;

            for (int i = 0; i + 2 < T.Count; i += 3)
            {
                // Walkable-facing triangles only: a wall's height says nothing about the floor
                var n = Vector3.Cross(T[i + 1] - T[i], T[i + 2] - T[i]);
                if (n.LengthSquared() < 1e-12f || MathF.Abs(n.Y) / n.Length() < 0.1f)
                    continue;

                float x0 = MathF.Min(T[i].X, MathF.Min(T[i + 1].X, T[i + 2].X)), x1 = MathF.Max(T[i].X, MathF.Max(T[i + 1].X, T[i + 2].X));
                float z0 = MathF.Min(T[i].Z, MathF.Min(T[i + 1].Z, T[i + 2].Z)), z1 = MathF.Max(T[i].Z, MathF.Max(T[i + 1].Z, T[i + 2].Z));

                for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                    for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var list))
                            Cells[(x, z)] = list = new List<int>();
                        list.Add(i);
                    }
            }
        }

        /// <summary>The height of the surface nearest a point straight up or down, within a range, or null.</summary>
        public float? Nearest(Vector3 p, float range)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list))
                return null;

            float? best = null;
            foreach (var i in list)
            {
                if (HeightAt(T[i], T[i + 1], T[i + 2], p.X, p.Z) is not float y || MathF.Abs(y - p.Y) > range)
                    continue;
                if (best == null || MathF.Abs(y - p.Y) < MathF.Abs(best.Value - p.Y))
                    best = y;
            }
            return best;
        }

        private static float? HeightAt(Vector3 a, Vector3 b, Vector3 c, float x, float z)
        {
            float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
            if (MathF.Abs(d) < 1e-9f)
                return null;

            float u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / d;
            float v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / d;
            float w = 1 - u - v;

            // A vertex on a triangle's edge counts for both triangles
            if (u < -1e-3f || v < -1e-3f || w < -1e-3f)
                return null;

            return u * a.Y + v * b.Y + w * c.Y;
        }
    }
}

/// <summary>
/// Navmesh files for generated meshes, built from one of the map's own files so they carry the same types.
/// </summary>
public static class NavGenFile
{
    /// <summary>
    /// A navmesh file holding a generated mesh: a copy of a template file (one of the map's navmesh files with a query
    /// mediator) with its mesh replaced, a new face tree for the mediator, and no clearance seeding or user edge setups.
    /// Null when the template has no mediator.
    /// </summary>
    public static Havok.HavokDocument Build(Havok.HavokDocument template, hkaiNavMesh generated)
    {
        var copy = NavModelEdit.Clone(template);
        var mesh = copy.ObjectsOf<hkaiNavMesh>().FirstOrDefault();
        var mediator = copy.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().FirstOrDefault();
        var tree = mediator?.m_tree?.m_treePtr;

        if (mesh == null || tree == null)
            return null;

        mesh.m_faces.Clear();
        mesh.m_faces.AddRange(generated.m_faces);
        mesh.m_edges.Clear();
        mesh.m_edges.AddRange(generated.m_edges);
        mesh.m_vertices.Clear();
        mesh.m_vertices.AddRange(generated.m_vertices);
        mesh.m_faceData.Clear();
        mesh.m_faceData.AddRange(generated.m_faceData);
        mesh.m_edgeData.Clear();
        mesh.m_edgeData.AddRange(generated.m_edgeData);
        mesh.m_streamingSets.Clear();
        mesh.m_faceDataStriding = generated.m_faceDataStriding;
        mesh.m_edgeDataStriding = generated.m_edgeDataStriding;
        mesh.m_aabb = generated.m_aabb;
        mesh.m_erosionRadius = generated.m_erosionRadius;
        mesh.m_clearanceCacheSeedingDataSet = null;

        mediator.m_navMesh = mesh;
        tree.m_tree = NavigationTree.Build(mesh);

        if (copy.Root is hkRootLevelContainer root)
            root.m_namedVariants.RemoveAll(v => !ReferenceEquals(v.m_variant, mesh) && !ReferenceEquals(v.m_variant, mediator));

        return NavModelEdit.Clone(copy);
    }

    /// <summary>A file of the map to build generated files from: one with faces and a query mediator.</summary>
    public static Havok.HavokDocument FindTemplate(NavigationMap map)
        => map.Files.Values.Where(f => f.FaceCount > 0 && f.Document != null)
            .Select(f => f.Document)
            .FirstOrDefault(d => d.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().Any());
}
