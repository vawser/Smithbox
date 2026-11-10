using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Finding the connections between navmeshes from their geometry: two boundary edges of different navmeshes of the same
// set that lie along each other (Navigation Editor Plan, Phase 3, re-stitching). The same idea as Havok's
// hkaiNavMeshEdgeMatchingParameters. Kept free of the rest of Smithbox; NavSurvey `stitch` checks it against the game's
// own connectors.
//
// How the game's connections sit (NavSurvey stitchsurvey):
// - they join boundary edges (no opposite face, flags 4) of an n mesh to an n mesh, or an o mesh to an o mesh;
// - an NVA connection is stored in both directions, each in the connector of its MainNameID → TargetNameID;
// - an NVC file holds one direction only, from its first map to its second;
// - the edges run opposite ways; 65% meet end to end within 1 mm, the rest overlap in part along a shared line.

/// <summary>How close two boundary edges must be to be joined.</summary>
public sealed record NavStitchParameters
{
    /// <summary>The largest distance across, in the XZ plane, between the two edges' lines (metres).</summary>
    public float MaxSeparation { get; init; } = 0.1f;

    /// <summary>The largest height step between the two edges where they overlap (metres).</summary>
    public float MaxStepHeight { get; init; } = 0.5f;

    /// <summary>The smallest length the two edges must overlap by (metres).</summary>
    public float MinOverlap { get; init; } = 0.02f;

    /// <summary>How anti-parallel the edges must be in XZ: the cosine of the largest angle between one and the other reversed.</summary>
    public float MinAlignment { get; init; } = 0.98f;
}

/// <summary>A navmesh placed for stitching: its NameID, its mesh, and where it sits relative to the other meshes.</summary>
public sealed record NavStitchMesh(int NameID, IHavokNavMeshEdit Mesh, Vector3 Offset, string MapId);

/// <summary>A connection found between an edge of one mesh and an edge of another.</summary>
public readonly record struct NavStitchPair(string MapA, int NameA, int FaceA, int EdgeA, string MapB, int NameB, int FaceB, int EdgeB);

/// <summary>How two edges sit against each other.</summary>
public readonly record struct NavEdgeFit(float Alignment, float Separation, float Overlap, float Step);

public static class NavigationStitch
{
    private const float Cell = 4f;

    private sealed class Boundary
    {
        public NavStitchMesh Owner;
        public int Face;
        public int Edge;
        public Vector3 A;
        public Vector3 B;
    }

    /// <summary>The boundary edges of a mesh (no opposite face; user edges excluded), with their faces, in placed space.</summary>
    private static IEnumerable<Boundary> BoundaryEdges(NavStitchMesh mesh)
    {
        var m = mesh.Mesh;
        for (int f = 0; f < m.m_faces.Count; f++)
        {
            var face = m.m_faces[f];
            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
            {
                var edge = m.m_edges[e];
                if (edge.m_oppositeFace != 0xFFFFFFFF)
                    continue;

                yield return new Boundary
                {
                    Owner = mesh,
                    Face = f,
                    Edge = e,
                    A = Vertex(m, edge.m_a) + mesh.Offset,
                    B = Vertex(m, edge.m_b) + mesh.Offset,
                };
            }
        }
    }

    private static Vector3 Vertex(IHavokNavMeshEdit mesh, int index)
    {
        var v = mesh.m_vertices[index];
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// How edge B sits against edge A: their alignment (1 = exactly opposite ways), the largest XZ distance of B's
    /// overlapping part from A's line, the length they overlap by along A, and the largest height step over the overlap.
    /// </summary>
    public static NavEdgeFit Fit(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1)
    {
        var da = new Vector2(a1.X - a0.X, a1.Z - a0.Z);
        var db = new Vector2(b1.X - b0.X, b1.Z - b0.Z);
        float la = da.Length(), lb = db.Length();

        if (la < 1e-5f || lb < 1e-5f)
            return new NavEdgeFit(-1, float.MaxValue, 0, float.MaxValue);

        var ua = da / la;
        float alignment = -Vector2.Dot(ua, db / lb);

        // Along A's line
        float t0 = Vector2.Dot(new Vector2(b0.X - a0.X, b0.Z - a0.Z), ua);
        float t1 = Vector2.Dot(new Vector2(b1.X - a0.X, b1.Z - a0.Z), ua);
        float lo = MathF.Max(0, MathF.Min(t0, t1));
        float hi = MathF.Min(la, MathF.Max(t0, t1));
        float overlap = hi - lo;

        if (overlap <= 0)
            return new NavEdgeFit(alignment, float.MaxValue, overlap, float.MaxValue);

        // At each end of the overlap: B's point there, its distance across A's line, and the height step
        var normal = new Vector2(-ua.Y, ua.X);
        float separation = 0, step = 0;

        foreach (var t in new[] { lo, hi })
        {
            var pa = a0 + (a1 - a0) * (t / la);
            float s = (t - t0) / (t1 - t0);
            var pb = b0 + (b1 - b0) * s;
            separation = MathF.Max(separation, MathF.Abs(Vector2.Dot(new Vector2(pb.X - pa.X, pb.Z - pa.Z), normal)));
            step = MathF.Max(step, MathF.Abs(pb.Y - pa.Y));
        }

        return new NavEdgeFit(alignment, separation, overlap, step);
    }

    /// <summary>The shortest distance between an edge of one placed mesh and an edge of another, or MaxValue if either is out of range.</summary>
    public static float Gap(NavStitchMesh a, int edgeA, NavStitchMesh b, int edgeB)
    {
        if (edgeA < 0 || edgeB < 0 || edgeA >= a.Mesh.m_edges.Count || edgeB >= b.Mesh.m_edges.Count)
            return float.MaxValue;

        var ea = a.Mesh.m_edges[edgeA];
        var eb = b.Mesh.m_edges[edgeB];
        return SegmentDistance(Vertex(a.Mesh, ea.m_a) + a.Offset, Vertex(a.Mesh, ea.m_b) + a.Offset,
            Vertex(b.Mesh, eb.m_a) + b.Offset, Vertex(b.Mesh, eb.m_b) + b.Offset);
    }

    /// <summary>The shortest distance between two segments (closest points, clamped to both).</summary>
    public static float SegmentDistance(Vector3 p0, Vector3 p1, Vector3 q0, Vector3 q1)
    {
        var d1 = p1 - p0;
        var d2 = q1 - q0;
        var r = p0 - q0;
        float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
        float s, t;

        if (a < 1e-10f && e < 1e-10f)
            return r.Length();

        if (a < 1e-10f)
        {
            s = 0;
            t = Math.Clamp(f / e, 0, 1);
        }
        else
        {
            float c = Vector3.Dot(d1, r);
            if (e < 1e-10f)
            {
                t = 0;
                s = Math.Clamp(-c / a, 0, 1);
            }
            else
            {
                float b = Vector3.Dot(d1, d2);
                float denom = a * e - b * b;
                s = denom > 1e-10f ? Math.Clamp((b * f - c * e) / denom, 0, 1) : 0;
                t = (b * s + f) / e;

                if (t < 0)
                {
                    t = 0;
                    s = Math.Clamp(-c / a, 0, 1);
                }
                else if (t > 1)
                {
                    t = 1;
                    s = Math.Clamp((b - c) / a, 0, 1);
                }
            }
        }

        return Vector3.Distance(p0 + d1 * s, q0 + d2 * t);
    }

    public static bool Accepts(NavStitchParameters p, NavEdgeFit fit)
        => fit.Alignment >= p.MinAlignment && fit.Separation <= p.MaxSeparation && fit.Overlap >= p.MinOverlap && fit.Step <= p.MaxStepHeight;

    /// <summary>
    /// Every pair of boundary edges of two different meshes that the parameters accept, in both directions (A to B and
    /// B to A), as the NVA stores them. Meshes are only paired when the filter allows it (e.g. only across two maps).
    /// </summary>
    public static List<NavStitchPair> Find(IReadOnlyList<NavStitchMesh> meshes, NavStitchParameters parameters, Func<NavStitchMesh, NavStitchMesh, bool> pairFilter = null)
        => FindWithFit(meshes, parameters, pairFilter).Where(x => Accepts(parameters, x.Fit)).Select(x => x.Pair).ToList();

    /// <summary>The candidate pairs the parameters' search distance reaches, with how each fits; Find keeps those it accepts.</summary>
    public static List<(NavStitchPair Pair, NavEdgeFit Fit)> FindWithFit(IReadOnlyList<NavStitchMesh> meshes, NavStitchParameters parameters, Func<NavStitchMesh, NavStitchMesh, bool> pairFilter = null)
    {
        var edges = meshes.SelectMany(BoundaryEdges).ToList();
        var grid = new Dictionary<(int, int), List<int>>();

        static (int, int) CellOf(float x, float z) => ((int)MathF.Floor(x / Cell), (int)MathF.Floor(z / Cell));

        for (int i = 0; i < edges.Count; i++)
        {
            var (x0, z0) = CellOf(MathF.Min(edges[i].A.X, edges[i].B.X) - parameters.MaxSeparation, MathF.Min(edges[i].A.Z, edges[i].B.Z) - parameters.MaxSeparation);
            var (x1, z1) = CellOf(MathF.Max(edges[i].A.X, edges[i].B.X) + parameters.MaxSeparation, MathF.Max(edges[i].A.Z, edges[i].B.Z) + parameters.MaxSeparation);

            for (int x = x0; x <= x1; x++)
                for (int z = z0; z <= z1; z++)
                {
                    if (!grid.TryGetValue((x, z), out var list))
                        grid[(x, z)] = list = new List<int>();
                    list.Add(i);
                }
        }

        var found = new HashSet<(int, int)>();
        var result = new List<(NavStitchPair, NavEdgeFit)>();

        foreach (var list in grid.Values)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    var a = edges[list[i]];
                    var b = edges[list[j]];

                    if (ReferenceEquals(a.Owner, b.Owner) || (pairFilter != null && !pairFilter(a.Owner, b.Owner)))
                        continue;

                    int lo = Math.Min(list[i], list[j]), hi = Math.Max(list[i], list[j]);
                    if (!found.Add((lo, hi)))
                        continue;

                    var fit = Fit(a.A, a.B, b.A, b.B);
                    if (fit.Overlap <= 0 || fit.Separation > parameters.MaxSeparation)
                        continue;

                    result.Add((new NavStitchPair(a.Owner.MapId, a.Owner.NameID, a.Face, a.Edge, b.Owner.MapId, b.Owner.NameID, b.Face, b.Edge), fit));
                    result.Add((new NavStitchPair(b.Owner.MapId, b.Owner.NameID, b.Face, b.Edge, a.Owner.MapId, a.Owner.NameID, a.Face, a.Edge), fit));
                }
            }
        }

        return result;
    }
}
