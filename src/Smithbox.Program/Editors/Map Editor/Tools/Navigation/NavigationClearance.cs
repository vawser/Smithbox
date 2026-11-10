using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The clearance cache of a navmesh (hkaiNavMeshClearanceCache, in each initial cache of the clearance seeding), worked out
// again where vertices moved. The rules, as NavSurvey `clearance fit` found them against the game's files:
// - a clearance is a distance on X/Z to the nearest boundary edge (an edge with no opposite face, including those joined to
//   other navmeshes by the NVA or NVC), capped at m_clearanceCeiling, stored as (int)(d × m_clearanceRealToIntMultiplier),
//   at most 254. Only boundaries reached by walking across faces count: a face is crossed while the edge into it lies
//   within the ceiling of the query (so a floor above or below doesn't count);
// - a vertex's clearance is that at the vertex; user edge vertices are 0xFF;
// - an edge's m_mcpData is its maximum clearance point: the largest clearance along it, and where (0 = start, 253 = end).
//   A boundary edge is 0 at 0. When capped, the point is the middle if it is at the ceiling, else the start, else the
//   end, else the first point that is;
// - each face's pair block holds, per pair of linked edges, the smaller of their two values (the game's value is lower
//   for a few percent of pairs, by a step or two in most).
// Only entries within the ceiling of a moved edge (before or after the move) are worked out again; the others keep the
// game's values, so an edit leaves the rest of the file as it was. User edges keep their values (they are link clearances).

/// <summary>What a clearance recalculation changed.</summary>
public sealed class NavClearanceResult
{
    public int Caches { get; set; }
    public int Vertices { get; set; }
    public int Edges { get; set; }
    public int Pairs { get; set; }

    /// <summary>Caches whose layout doesn't fit the mesh, which were left as they were.</summary>
    public int Skipped { get; set; }
}

public static class NavClearance
{
    /// <summary>Samples along an edge to find its maximum, before the search between the best two.</summary>
    private const int Samples = 254;

    /// <summary>
    /// Works the clearances around moved vertices out again, in every initial cache of the mesh's clearance seeding.
    /// <paramref name="before"/> holds every vertex's position before the move (the mesh holds them after).
    /// </summary>
    public static NavClearanceResult Recalculate(IHavokNavMeshEdit mesh, IReadOnlyList<Vector3> before, IReadOnlyCollection<int> moved)
    {
        var result = new NavClearanceResult();
        var caches = mesh.ClearanceCaches;

        if (caches.Count == 0 || moved.Count == 0 || mesh.m_faces.Count == 0)
            return result;

        var geometry = new Geometry(mesh);
        var movedSet = moved as ISet<int> ?? moved.ToHashSet();

        // The edges a move changed, where they were and where they are
        var changed = new List<(Vector3, Vector3)>();
        foreach (var k in geometry.OriginalEdges)
        {
            var e = mesh.m_edges[k];
            if (!movedSet.Contains(e.m_a) && !movedSet.Contains(e.m_b))
                continue;
            changed.Add((before[e.m_a], before[e.m_b]));
            changed.Add((geometry.Vertex(e.m_a), geometry.Vertex(e.m_b)));
        }

        foreach (var cache in caches)
        {
            if (cache.m_vertexClearances.Count != mesh.m_vertices.Count || cache.m_mcpData.Count != mesh.m_edges.Count
                || cache.m_faceOffsets.Count != mesh.m_faces.Count || cache.m_clearanceCeiling <= 0)
            {
                result.Skipped++;
                continue;
            }

            Recalculate(mesh, geometry, cache, changed, movedSet, result);
            result.Caches++;
        }

        return result;
    }

    private static void Recalculate(IHavokNavMeshEdit mesh, Geometry geometry, IHavokNavClearanceCache cache, List<(Vector3, Vector3)> changed,
        ISet<int> moved, NavClearanceResult result)
    {
        float ceiling = cache.m_clearanceCeiling;
        float toInt = cache.m_clearanceRealToIntMultiplier;
        var near = new SegmentGrid(changed, ceiling);
        int Quantize(float d) => d >= ceiling ? 254 : Math.Clamp((int)(d * toInt), 0, 254);

        // Vertices
        for (int v = 0; v < mesh.m_vertices.Count; v++)
        {
            if (geometry.UserVertices.Contains(v) || cache.m_vertexClearances[v] == 0xFF)
                continue;

            var p = geometry.Vertex(v);
            if (!moved.Contains(v) && !near.Within(p, p, ceiling))
                continue;

            var reach = geometry.Reach(geometry.VertexFaces(v), (a, b) => Planar.PointSegment(p, a, b), ceiling);
            byte value = (byte)Quantize(Nearest(reach, p, ceiling));

            if (cache.m_vertexClearances[v] != value)
            {
                cache.m_vertexClearances[v] = value;
                result.Vertices++;
            }
        }

        // Edges, and the faces whose pair values follow them
        var changedEdges = new HashSet<int>();

        foreach (var k in geometry.OriginalEdges)
        {
            var e = mesh.m_edges[k];
            var mcp = cache.m_mcpData[k];

            if (e.m_oppositeFace == 0xFFFFFFFF)
                continue;

            var a = geometry.Vertex(e.m_a);
            var b = geometry.Vertex(e.m_b);

            if (!moved.Contains(e.m_a) && !moved.Contains(e.m_b) && !near.Within(a, b, ceiling))
                continue;

            var starts = new List<int> { geometry.FaceOf[k] };
            int opposite = (int)(e.m_oppositeFace & 0x3FFFFF);
            if (opposite < mesh.m_faces.Count && (e.m_oppositeFace >> 22) is 0 or 0x3FF)
                starts.Add(opposite);

            var reach = geometry.Reach(starts, (x, y) => Planar.SegmentSegment(a, b, x, y), ceiling);
            var (clearance, interpolant) = MaximumPoint(reach, a, b, ceiling, Quantize);

            if (mcp.m_clearance != clearance || mcp.m_interpolant != interpolant)
            {
                if (mcp.m_clearance != clearance)
                    changedEdges.Add(k);

                mcp.m_clearance = (byte)clearance;
                mcp.m_interpolant = (byte)interpolant;
                result.Edges++;
            }
        }

        if (changedEdges.Count == 0)
            return;

        foreach (var f in changedEdges.Select(k => geometry.FaceOf[k]).Distinct())
            result.Pairs += RewritePairs(mesh, cache, f, changedEdges);
    }

    /// <summary>
    /// The largest clearance along an edge and where it is, as stored: sampled, then searched between the samples next to
    /// the best one. A capped edge takes the middle, the start, the end or the first capped sample, in that order.
    /// </summary>
    private static (int Clearance, int Interpolant) MaximumPoint(List<(Vector3, Vector3)> reach, Vector3 a, Vector3 b, float ceiling, Func<float, int> quantize)
    {
        float At(float t) => Nearest(reach, Vector3.Lerp(a, b, t), ceiling);

        float best = -1;
        int bestI = 0;

        for (int i = 0; i < Samples; i++)
        {
            float d = At(i / (float)(Samples - 1));
            if (d > best + 1e-6f)
            {
                best = d;
                bestI = i;
            }
        }

        if (quantize(best) >= 254)
        {
            if (quantize(At(0.5f)) >= 254) return (254, 127);
            if (quantize(At(0)) >= 254) return (254, 0);
            if (quantize(At(1)) >= 254) return (254, 253);
            return (254, Math.Clamp((int)MathF.Round(bestI / (float)(Samples - 1) * 253), 0, 253));
        }

        float lo = Math.Max(0, bestI - 1) / (float)(Samples - 1);
        float hi = Math.Min(Samples - 1, bestI + 1) / (float)(Samples - 1);

        for (int it = 0; it < 40; it++)
        {
            float m1 = lo + (hi - lo) / 3, m2 = hi - (hi - lo) / 3;
            if (At(m1) < At(m2)) lo = m1; else hi = m2;
        }

        float tm = (lo + hi) / 2, dm = At(tm);
        float t = bestI / (float)(Samples - 1);

        if (dm > best)
        {
            best = dm;
            t = tm;
        }

        return (quantize(best), Math.Clamp((int)MathF.Round(t * 253), 0, 253));
    }

    /// <summary>
    /// Writes a face's pair values again where one of their edges changed: the smaller of the two edges' values, in the
    /// game's layout (a lower triangle by column over the linked edges, original then user, then 0xFF). Faces whose block
    /// doesn't have that layout are left as they are. Returns the number of values changed.
    /// </summary>
    private static int RewritePairs(IHavokNavMeshEdit mesh, IHavokNavClearanceCache cache, int f, HashSet<int> changedEdges)
    {
        var face = mesh.m_faces[f];
        var linked = new List<int>();

        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);
        for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
            if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);

        int count = linked.Count * (linked.Count - 1) / 2;
        int start = (int)cache.m_faceOffsets[f];

        if (start + count >= cache.m_edgePairClearances.Count || cache.m_edgePairClearances[start + count] != 0xFF)
            return 0;

        int written = 0, index = start;

        for (int j = 1; j < linked.Count; j++)
        {
            for (int i = 0; i < j; i++, index++)
            {
                if (!changedEdges.Contains(linked[i]) && !changedEdges.Contains(linked[j]))
                    continue;

                byte value = Math.Min(cache.m_mcpData[linked[i]].m_clearance, cache.m_mcpData[linked[j]].m_clearance);

                if (cache.m_edgePairClearances[index] != value)
                {
                    cache.m_edgePairClearances[index] = value;
                    written++;
                }
            }
        }

        return written;
    }

    private static float Nearest(List<(Vector3 A, Vector3 B)> segments, Vector3 p, float ceiling)
    {
        float best = ceiling;
        foreach (var (a, b) in segments)
            best = MathF.Min(best, Planar.PointSegment(p, a, b));
        return best;
    }

    /// <summary>The faces, edges and vertices of a mesh, with what the clearance search needs to walk it.</summary>
    private sealed class Geometry
    {
        private readonly IHavokNavMeshEdit Mesh;

        /// <summary>The face of each original edge.</summary>
        public readonly int[] FaceOf;

        public readonly List<int> OriginalEdges = new();
        public readonly HashSet<int> UserVertices = new();
        private readonly Dictionary<int, List<int>> FacesOfVertex = new();

        public Geometry(IHavokNavMeshEdit mesh)
        {
            Mesh = mesh;
            FaceOf = new int[mesh.m_edges.Count];

            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                var face = mesh.m_faces[f];

                for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                {
                    FaceOf[k] = f;
                    OriginalEdges.Add(k);
                    int v = mesh.m_edges[k].m_a;
                    (FacesOfVertex.TryGetValue(v, out var list) ? list : FacesOfVertex[v] = new()).Add(f);
                }

                for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                {
                    UserVertices.Add(mesh.m_edges[k].m_a);
                    UserVertices.Add(mesh.m_edges[k].m_b);
                }
            }
        }

        public Vector3 Vertex(int i)
        {
            var v = Mesh.m_vertices[i];
            return new Vector3(v.X, v.Y, v.Z);
        }

        public IReadOnlyList<int> VertexFaces(int v) => FacesOfVertex.TryGetValue(v, out var list) ? list : Array.Empty<int>();

        /// <summary>
        /// The boundary edges reached from start faces, crossing into a face while the edge into it lies within the
        /// ceiling of the query.
        /// </summary>
        public List<(Vector3, Vector3)> Reach(IEnumerable<int> start, Func<Vector3, Vector3, float> distanceTo, float ceiling)
        {
            var found = new List<(Vector3, Vector3)>();
            var seen = new HashSet<int>(start);
            var queue = new Queue<int>(seen);

            while (queue.Count > 0)
            {
                var face = Mesh.m_faces[queue.Dequeue()];

                for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                {
                    var e = Mesh.m_edges[k];
                    var a = Vertex(e.m_a);
                    var b = Vertex(e.m_b);

                    if (e.m_oppositeFace == 0xFFFFFFFF)
                    {
                        found.Add((a, b));
                        continue;
                    }

                    int other = (int)(e.m_oppositeFace & 0x3FFFFF);

                    if (other >= Mesh.m_faces.Count || seen.Contains(other) || distanceTo(a, b) > ceiling)
                        continue;

                    seen.Add(other);
                    queue.Enqueue(other);
                }
            }

            return found;
        }
    }

    /// <summary>Segments bucketed on X/Z, to ask whether a point or segment lies within a distance of any of them.</summary>
    private sealed class SegmentGrid
    {
        private readonly List<(Vector3 A, Vector3 B)> Segments;
        private readonly Dictionary<(int, int), List<int>> Cells = new();
        private readonly float Size;

        public SegmentGrid(List<(Vector3, Vector3)> segments, float size)
        {
            Segments = segments;
            Size = size;

            for (int i = 0; i < segments.Count; i++)
            {
                var (a, b) = segments[i];
                foreach (var cell in CellsOf(a, b, 0))
                    (Cells.TryGetValue(cell, out var list) ? list : Cells[cell] = new()).Add(i);
            }
        }

        private IEnumerable<(int, int)> CellsOf(Vector3 a, Vector3 b, float pad)
        {
            int x0 = (int)MathF.Floor((MathF.Min(a.X, b.X) - pad) / Size), x1 = (int)MathF.Floor((MathF.Max(a.X, b.X) + pad) / Size);
            int z0 = (int)MathF.Floor((MathF.Min(a.Z, b.Z) - pad) / Size), z1 = (int)MathF.Floor((MathF.Max(a.Z, b.Z) + pad) / Size);

            for (int x = x0; x <= x1; x++)
                for (int z = z0; z <= z1; z++)
                    yield return (x, z);
        }

        /// <summary>Whether the segment a–b (a point when a = b) lies within a distance of any of the segments.</summary>
        public bool Within(Vector3 a, Vector3 b, float distance)
        {
            var tested = new HashSet<int>();

            foreach (var cell in CellsOf(a, b, distance))
            {
                if (!Cells.TryGetValue(cell, out var list))
                    continue;

                foreach (var i in list)
                {
                    if (!tested.Add(i))
                        continue;

                    var (c, d) = Segments[i];
                    if (Planar.SegmentSegment(a, b, c, d) <= distance + 1e-4f)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>Distances on X/Z.</summary>
    private static class Planar
    {
        public static float PointSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            float px = p.X - a.X, pz = p.Z - a.Z, abx = b.X - a.X, abz = b.Z - a.Z;
            float length = abx * abx + abz * abz;
            float t = length < 1e-12f ? 0 : Math.Clamp((px * abx + pz * abz) / length, 0, 1);
            float dx = px - abx * t, dz = pz - abz * t;
            return MathF.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>0 if the segments cross, else the nearest end of one to the other.</summary>
        public static float SegmentSegment(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            static float Cross(Vector3 o, Vector3 p, Vector3 q) => (p.X - o.X) * (q.Z - o.Z) - (p.Z - o.Z) * (q.X - o.X);

            if (Cross(a, b, c) * Cross(a, b, d) < 0 && Cross(c, d, a) * Cross(c, d, b) < 0)
                return 0;

            return MathF.Min(MathF.Min(PointSegment(a, c, d), PointSegment(b, c, d)), MathF.Min(PointSegment(c, a, b), PointSegment(d, a, b)));
        }
    }
}
