using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The NGP mode's geometry helpers (Navigation.md (NGP (DS2, DS2S)), Phase G5): a ladder strip at a ladder object and a drop
// strip from a ledge to the floor below. Both cut their joins into the floors, as the game's own data does. Vanilla
// strips are manifold: no ladder face shares an edge with two other faces, so the floor ends where a strip joins it.
// Renderer-free, so NavSurvey checks them on the game files (`nvm GAME ngp-strips`).

/// <summary>
/// An NGP mesh being cut: vertices, faces (vertex triples, null once removed) and their flags, with each vertex's faces
/// kept up to date. Floors are cut by inserting points (a face split in three, or an edge split in two), then making the
/// wanted edges exist by flipping the edges that cross them. Positions are map-local; the tests work on the XZ plane.
/// </summary>
public sealed class NgpWorkMesh
{
    public readonly List<Vector3> Vertices = new();
    public readonly List<int[]> Faces = new();
    public readonly List<uint> Flags = new();
    private readonly List<List<int>> VertexFaces = new();
    public int RuntimeSlot;

    /// <summary>A point closer than this to a vertex is that vertex.</summary>
    public const float VertexSnap = 0.03f;

    /// <summary>A point closer than this to an edge splits that edge.</summary>
    public const float EdgeSnap = 0.01f;

    public static NgpWorkMesh From(NGP.Mesh mesh)
    {
        var work = new NgpWorkMesh { RuntimeSlot = mesh.RuntimeSlot };
        foreach (var v in mesh.Vertices)
            work.AddVertex(v);

        for (int f = 0; f < mesh.Faces.Count; f++)
        {
            var face = mesh.Faces[f];
            uint flags = f < mesh.FaceFlags.Count ? mesh.FaceFlags[f] : NgpFlags.DefaultLevel;
            if (NgpGeometry.InRange(face, mesh.Vertices.Count))
                work.AddFace(face.V1, face.V2, face.V3, flags);
        }

        return work;
    }

    public NgpWorkMesh Clone()
    {
        var copy = new NgpWorkMesh { RuntimeSlot = RuntimeSlot };
        copy.Vertices.AddRange(Vertices);
        foreach (var vf in VertexFaces)
            copy.VertexFaces.Add(new List<int>(vf));
        copy.Faces.AddRange(Faces.Select(f => f == null ? null : (int[])f.Clone()));
        copy.Flags.AddRange(Flags);
        return copy;
    }

    /// <summary>The mesh as an NGP mesh, removed faces and unused vertices left out. Its tables are left to NgpRebuild.</summary>
    public NGP.Mesh ToMesh()
    {
        var mesh = new NGP.Mesh { RuntimeSlot = RuntimeSlot };
        var map = new Dictionary<int, int>();

        int Map(int v)
        {
            if (!map.TryGetValue(v, out var i))
            {
                i = mesh.Vertices.Count;
                map[v] = i;
                mesh.Vertices.Add(Vertices[v]);
            }
            return i;
        }

        for (int f = 0; f < Faces.Count; f++)
        {
            if (Faces[f] is not int[] face)
                continue;
            mesh.Faces.Add(new NGP.Face { V1 = (short)Map(face[0]), V2 = (short)Map(face[1]), V3 = (short)Map(face[2]) });
            mesh.FaceFlags.Add(Flags[f]);
        }

        return mesh;
    }

    public int FaceCount => Faces.Count(f => f != null);

    public int VertexCount => VertexFaces.Count(v => v.Count > 0);

    public int AddVertex(Vector3 p)
    {
        Vertices.Add(p);
        VertexFaces.Add(new List<int>());
        return Vertices.Count - 1;
    }

    public int AddFace(int a, int b, int c, uint flags)
    {
        int f = Faces.Count;
        Faces.Add(new[] { a, b, c });
        Flags.Add(flags);
        VertexFaces[a].Add(f);
        VertexFaces[b].Add(f);
        VertexFaces[c].Add(f);
        return f;
    }

    public void SetFace(int f, int a, int b, int c)
    {
        foreach (var v in Faces[f])
            VertexFaces[v].Remove(f);
        Faces[f] = new[] { a, b, c };
        VertexFaces[a].Add(f);
        VertexFaces[b].Add(f);
        VertexFaces[c].Add(f);
    }

    public void RemoveFace(int f)
    {
        if (Faces[f] == null)
            return;
        foreach (var v in Faces[f])
            VertexFaces[v].Remove(f);
        Faces[f] = null;
    }

    public IReadOnlyList<int> FacesOf(int vertex) => VertexFaces[vertex];

    /// <summary>The faces that use the edge between two vertices.</summary>
    public List<int> EdgeFaces(int a, int b) => VertexFaces[a].Where(f => Array.IndexOf(Faces[f], b) >= 0).ToList();

    public bool HasEdge(int a, int b) => VertexFaces[a].Any(f => Array.IndexOf(Faces[f], b) >= 0);

    /// <summary>The vertex of a face that is neither a nor b.</summary>
    public int Third(int f, int a, int b) => Faces[f].First(v => v != a && v != b);

    /// <summary>Whether a face has the directed edge a to b (in its winding order).</summary>
    public bool HasDirected(int f, int a, int b)
    {
        var face = Faces[f];
        for (int k = 0; k < 3; k++)
            if (face[k] == a && face[(k + 1) % 3] == b)
                return true;
        return false;
    }

    #region Geometry

    public static Vector2 XZ(Vector3 p) => new(p.X, p.Z);

    /// <summary>Twice the signed area of the XZ triangle a, b, c.</summary>
    public static float Orient(Vector2 a, Vector2 b, Vector2 c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

    /// <summary>Whether two XZ segments cross at a point inside both (not at an end).</summary>
    public static bool ProperCross(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float o1 = Orient(p1, p2, q1), o2 = Orient(p1, p2, q2), o3 = Orient(q1, q2, p1), o4 = Orient(q1, q2, p2);
        const float eps = 1e-7f;
        return (o1 > eps && o2 < -eps || o1 < -eps && o2 > eps) && (o3 > eps && o4 < -eps || o3 < -eps && o4 > eps);
    }

    /// <summary>A face is a floor when its normal is within 60° of vertical; points are only placed on floors.</summary>
    public bool IsFloor(int f)
    {
        var face = Faces[f];
        var n = Vector3.Cross(Vertices[face[1]] - Vertices[face[0]], Vertices[face[2]] - Vertices[face[0]]);
        float length = n.Length();
        return length > 1e-6f && MathF.Abs(n.Y) / length > 0.5f;
    }

    /// <summary>The height of a face's plane over an XZ point, and whether the point lies inside the face (on XZ).</summary>
    public bool HeightIn(int f, Vector2 p, out float y, float eps = 1e-4f)
    {
        var face = Faces[f];
        Vector3 a = Vertices[face[0]], b = Vertices[face[1]], c = Vertices[face[2]];
        Vector2 A = XZ(a), B = XZ(b), C = XZ(c);
        float d = Orient(A, B, C);
        y = 0;
        if (MathF.Abs(d) < 1e-9f)
            return false;

        float wa = Orient(B, C, p) / d, wb = Orient(C, A, p) / d, wc = 1 - wa - wb;
        y = wa * a.Y + wb * b.Y + wc * c.Y;
        return wa >= -eps && wb >= -eps && wc >= -eps;
    }

    public static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b, out float t)
    {
        var ab = b - a;
        float len = ab.LengthSquared();
        t = len < 1e-12f ? 0 : Math.Clamp(Vector2.Dot(p - a, ab) / len, 0, 1);
        return Vector2.Distance(p, a + ab * t);
    }

    public IEnumerable<int> LiveFaces()
    {
        for (int f = 0; f < Faces.Count; f++)
            if (Faces[f] != null)
                yield return f;
    }

    #endregion

    #region Locating and inserting points

    /// <summary>Where a point goes in the mesh: inside a floor face, or on an open edge it was moved to.</summary>
    public sealed record Location(int Face, int EdgeA, int EdgeB, Vector3 Point, bool OnBoundary)
    {
        public float Height => Point.Y;
    }

    /// <summary>
    /// The floor at an XZ point: the floor face over it whose height <paramref name="accept"/> takes, nearest to a height
    /// first; else, within <paramref name="snap"/> metres on XZ, the nearest point of an open edge whose height it takes.
    /// Null when neither exists.
    /// </summary>
    public Location Locate(Vector2 p, float preferY, Func<float, bool> accept, float snap)
    {
        Location best = null;
        float bestScore = float.MaxValue;

        foreach (int f in LiveFaces())
        {
            if (!IsFloor(f) || !HeightIn(f, p, out float y) || !accept(y))
                continue;
            float score = MathF.Abs(y - preferY);
            if (score < bestScore)
            {
                bestScore = score;
                best = new Location(f, -1, -1, new Vector3(p.X, y, p.Y), false);
            }
        }

        return best ?? (snap > 0 ? LocateOpenEdge(p, preferY, accept, snap) : null);
    }

    /// <summary>The nearest point (on XZ, within <paramref name="snap"/>) of an open edge whose height <paramref name="accept"/> takes, or null.</summary>
    public Location LocateOpenEdge(Vector2 p, float preferY, Func<float, bool> accept, float snap)
    {
        Location best = null;
        float bestDistance = snap;
        foreach (int f in LiveFaces())
        {
            var face = Faces[f];
            for (int k = 0; k < 3; k++)
            {
                int a = face[k], b = face[(k + 1) % 3];
                if (EdgeFaces(a, b).Count != 1)
                    continue;

                float distance = SegmentDistance(p, XZ(Vertices[a]), XZ(Vertices[b]), out float t);
                var point = Vector3.Lerp(Vertices[a], Vertices[b], t);
                if (!accept(point.Y))
                    continue;
                if (distance < bestDistance - 1e-4f || MathF.Abs(distance - bestDistance) <= 1e-4f && best != null && MathF.Abs(point.Y - preferY) < MathF.Abs(best.Point.Y - preferY))
                {
                    bestDistance = distance;
                    best = new Location(f, a, b, point, true);
                }
            }
        }

        return best;
    }

    /// <summary>Puts a located point into the mesh and returns its vertex: an existing vertex near it, or a new one splitting an edge or the face.</summary>
    public int Insert(Location location, float vertexSnap = VertexSnap)
    {
        var p = location.Point;
        var p2 = XZ(p);

        // A wider snap takes the nearest used vertex of the floor's height, so a corner meant for a vertex lands on it
        if (vertexSnap > VertexSnap)
        {
            int nearest = -1;
            float nearestDistance = vertexSnap;
            for (int v = 0; v < Vertices.Count; v++)
            {
                if (VertexFaces[v].Count == 0 || MathF.Abs(Vertices[v].Y - p.Y) > 0.3f)
                    continue;
                float distance = Vector2.Distance(XZ(Vertices[v]), p2);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = v;
                }
            }
            if (nearest >= 0)
                return nearest;
        }

        if (location.OnBoundary)
        {
            int a = location.EdgeA, b = location.EdgeB;
            if (Vector2.Distance(XZ(Vertices[a]), p2) < VertexSnap) return a;
            if (Vector2.Distance(XZ(Vertices[b]), p2) < VertexSnap) return b;
            return SplitEdge(a, b, p);
        }

        var face = Faces[location.Face];
        foreach (var v in face)
            if (Vector2.Distance(XZ(Vertices[v]), p2) < VertexSnap)
                return v;

        for (int k = 0; k < 3; k++)
        {
            int a = face[k], b = face[(k + 1) % 3];
            if (SegmentDistance(p2, XZ(Vertices[a]), XZ(Vertices[b]), out float t) < EdgeSnap)
                return SplitEdge(a, b, Vector3.Lerp(Vertices[a], Vertices[b], t));
        }

        return SplitFace(location.Face, p);
    }

    /// <summary>Splits the edge a-b at a point: each face on it becomes two, keeping its winding and flags.</summary>
    public int SplitEdge(int a, int b, Vector3 p)
    {
        int x = AddVertex(p);
        foreach (int f in EdgeFaces(a, b))
        {
            int s = HasDirected(f, a, b) ? a : b, t = s == a ? b : a, o = Third(f, a, b);
            SetFace(f, s, x, o);
            AddFace(x, t, o, Flags[f]);
        }
        return x;
    }

    /// <summary>Splits a face in three at a point inside it.</summary>
    public int SplitFace(int f, Vector3 p)
    {
        int x = AddVertex(p);
        var face = Faces[f];
        int a = face[0], b = face[1], c = face[2];
        SetFace(f, a, b, x);
        AddFace(b, c, x, Flags[f]);
        AddFace(c, a, x, Flags[f]);
        return x;
    }

    #endregion

    #region Recovering edges

    /// <summary>
    /// The edges the XZ segment a-b crosses, walking face to face from a; null when the walk leaves the mesh (an open
    /// edge), meets a non-manifold edge, or passes exactly through a vertex.
    /// </summary>
    public List<(int, int)> Crossed(int a, int b)
    {
        Vector2 A = XZ(Vertices[a]), B = XZ(Vertices[b]);
        var crossed = new List<(int, int)>();
        int previous = -1;

        foreach (int f in VertexFaces[a])
        {
            var others = Faces[f].Where(v => v != a).ToArray();
            if (ProperCross(A, B, XZ(Vertices[others[0]]), XZ(Vertices[others[1]])))
            {
                crossed.Add((others[0], others[1]));
                previous = f;
                break;
            }
        }

        if (previous < 0)
            return null;

        for (int guard = 0; guard < 100000; guard++)
        {
            var (u, w) = crossed[^1];
            var next = EdgeFaces(u, w).Where(f => f != previous).ToList();
            if (next.Count != 1)
                return null;

            int g = next[0], x = Third(g, u, w);
            if (x == b)
                return crossed;

            var X = XZ(Vertices[x]);
            if (ProperCross(A, B, XZ(Vertices[u]), X))
                crossed.Add((u, x));
            else if (ProperCross(A, B, XZ(Vertices[w]), X))
                crossed.Add((w, x));
            else
                return null;

            previous = g;
        }

        return null;
    }

    /// <summary>Whether the edge u-w has two faces of the same winding forming a convex XZ quad, so it can be flipped.</summary>
    public bool CanFlip(int u, int w, out int c, out int d)
    {
        c = d = -1;
        var faces = EdgeFaces(u, w);
        if (faces.Count != 2)
            return false;

        int f1 = faces[0], f2 = faces[1];
        if (HasDirected(f1, u, w) == HasDirected(f2, u, w))
            return false;

        c = Third(f1, u, w);
        d = Third(f2, u, w);
        return c != d && !HasEdge(c, d) && ProperCross(XZ(Vertices[c]), XZ(Vertices[d]), XZ(Vertices[u]), XZ(Vertices[w]));
    }

    /// <summary>Replaces the edge u-w by the other diagonal of its two faces' quad, keeping the winding.</summary>
    public void Flip(int u, int w)
    {
        var faces = EdgeFaces(u, w);
        int f1 = faces[0], f2 = faces[1];
        int s = HasDirected(f1, u, w) ? u : w, t = s == u ? w : u;
        int c = Third(f1, u, w), d = Third(f2, u, w);
        // f1 = (s, t, c), f2 = (t, s, d) become (c, s, d) and (d, t, c)
        SetFace(f1, c, s, d);
        SetFace(f2, d, t, c);
    }

    /// <summary>Makes the edge a-b exist by flipping the edges that cross it (on XZ). False when it can't.</summary>
    public bool Recover(int a, int b)
    {
        if (a == b)
            return false;

        Vector2 A = XZ(Vertices[a]), B = XZ(Vertices[b]);

        for (int iteration = 0; iteration < 1000; iteration++)
        {
            if (HasEdge(a, b))
                return true;

            var crossed = Crossed(a, b);
            if (crossed == null || crossed.Count == 0)
                return false;

            // A flip whose new edge doesn't cross the segment first; else any flip, in turn, so it doesn't cycle
            (int, int)? chosen = null;
            foreach (var (u, w) in crossed)
            {
                if (CanFlip(u, w, out int c, out int d) && (c == a || c == b || d == a || d == b || !ProperCross(A, B, XZ(Vertices[c]), XZ(Vertices[d]))))
                {
                    chosen = (u, w);
                    break;
                }
            }

            if (chosen == null)
            {
                var flippable = crossed.Where(e => CanFlip(e.Item1, e.Item2, out _, out _)).ToList();
                if (flippable.Count == 0)
                    return false;
                chosen = flippable[iteration % flippable.Count];
            }

            Flip(chosen.Value.Item1, chosen.Value.Item2);
        }

        return HasEdge(a, b);
    }

    /// <summary>
    /// The faces inside a loop of vertices whose edges all exist: from the faces on the loop's inner side, every face
    /// reached without crossing the loop. Null when that spills out (the faces' XZ area is not the loop's).
    /// </summary>
    public List<int> FacesInside(IReadOnlyList<int> loop)
    {
        var points = loop.Select(v => XZ(Vertices[v])).ToList();
        float area = 0;
        for (int i = 0; i < points.Count; i++)
            area += points[i].X * points[(i + 1) % points.Count].Y - points[(i + 1) % points.Count].X * points[i].Y;
        float sign = MathF.Sign(area);
        area = MathF.Abs(area) / 2;
        if (area < 1e-4f)
            return null;

        var walls = new HashSet<(int, int)>();
        for (int i = 0; i < loop.Count; i++)
        {
            int a = loop[i], b = loop[(i + 1) % loop.Count];
            walls.Add((Math.Min(a, b), Math.Max(a, b)));
        }

        var inside = new HashSet<int>();
        var queue = new Queue<int>();
        for (int i = 0; i < loop.Count; i++)
        {
            int a = loop[i], b = loop[(i + 1) % loop.Count];
            foreach (int f in EdgeFaces(a, b))
            {
                int x = Third(f, a, b);
                if (Orient(points[i], points[(i + 1) % points.Count], XZ(Vertices[x])) * sign > 0 && inside.Add(f))
                    queue.Enqueue(f);
            }
        }

        float found = 0;
        while (queue.Count > 0)
        {
            int f = queue.Dequeue();
            var face = Faces[f];
            found += MathF.Abs(Orient(XZ(Vertices[face[0]]), XZ(Vertices[face[1]]), XZ(Vertices[face[2]]))) / 2;
            if (found > area * 1.1f + 0.01f)
                return null;

            for (int k = 0; k < 3; k++)
            {
                int a = face[k], b = face[(k + 1) % 3];
                if (walls.Contains((Math.Min(a, b), Math.Max(a, b))))
                    continue;
                foreach (int g in EdgeFaces(a, b))
                    if (inside.Add(g))
                        queue.Enqueue(g);
            }
        }

        return inside.Count > 0 && found > area * 0.9f - 0.01f ? inside.ToList() : null;
    }

    /// <summary>
    /// The shortest path from a to b along open edges (a floor's edge), as its vertices from a to b: null when there is
    /// none within 12 edges, or it runs more than half again the straight distance (it went round a corner).
    /// </summary>
    public List<int> OpenPath(int a, int b)
    {
        var from = new Dictionary<int, int> { [a] = -1 };
        var distance = new Dictionary<int, float> { [a] = 0 };
        var frontier = new List<int> { a };

        for (int step = 0; step < 12 && frontier.Count > 0 && !from.ContainsKey(b); step++)
        {
            var next = new List<int>();
            foreach (int v in frontier)
            {
                foreach (int f in VertexFaces[v])
                {
                    foreach (int w in Faces[f])
                    {
                        if (w == v || EdgeFaces(v, w).Count != 1)
                            continue;
                        float d = distance[v] + Vector3.Distance(Vertices[v], Vertices[w]);
                        if (!distance.TryGetValue(w, out var known) || d < known)
                        {
                            distance[w] = d;
                            from[w] = v;
                            next.Add(w);
                        }
                    }
                }
            }
            frontier = next.Distinct().ToList();
        }

        if (!from.ContainsKey(b) || distance[b] > Vector3.Distance(Vertices[a], Vertices[b]) * 1.5f + 0.5f)
            return null;

        var path = new List<int>();
        for (int v = b; v >= 0; v = from[v])
            path.Add(v);
        path.Reverse();
        return path;
    }

    /// <summary>The first open edge the XZ ray from p along dir meets within reach, at a height within tolerance of p's, or null.</summary>
    public Location RayOpenEdge(Vector3 p, Vector2 dir, float reach, float tolerance)
    {
        var from = XZ(p);
        var to = from + dir * reach;
        Location best = null;
        float bestT = float.MaxValue;

        foreach (int f in LiveFaces())
        {
            var face = Faces[f];
            for (int k = 0; k < 3; k++)
            {
                int a = face[k], b = face[(k + 1) % 3];
                Vector2 A = XZ(Vertices[a]), B = XZ(Vertices[b]);
                float d = Orient(Vector2.Zero, to - from, B - A);
                if (MathF.Abs(d) < 1e-9f)
                    continue;
                // from + (to - from) t = A + (B - A) s
                float t = Orient(Vector2.Zero, A - from, B - A) / d;
                float s = Orient(Vector2.Zero, A - from, to - from) / d;
                if (t < 0 || t > 1 || s < 0 || s > 1 || t >= bestT || EdgeFaces(a, b).Count != 1)
                    continue;
                var point = Vector3.Lerp(Vertices[a], Vertices[b], s);
                if (MathF.Abs(point.Y - p.Y) > tolerance)
                    continue;
                bestT = t;
                best = new Location(f, a, b, point, true);
            }
        }

        return best;
    }

    /// <summary>
    /// For an edge i0-i1 that lies off the floor: the floor's open edge ahead of it along dir, as the path of open
    /// edges between where each end's ray meets it (vertices put in as needed), and the floor face on its first edge.
    /// Null (the mesh as it was) when an end is over the floor, a ray meets nothing, or the two meet no common edge path.
    /// </summary>
    public (List<int> Path, int FloorFace)? OpenEdgeAhead(Vector3 i0, Vector3 i1, Vector2 dir, float reach, float tolerance)
    {
        bool OverFloor(Vector3 p) => Locate(XZ(p), p.Y, y => MathF.Abs(y - p.Y) <= tolerance, 0) != null;
        if (OverFloor(i0) || OverFloor(i1))
            return null;

        var before = Clone();
        var hit0 = RayOpenEdge(i0, dir, reach, tolerance);
        if (hit0 == null)
            return null;
        int h0 = Insert(hit0, CornerSnap);

        // After the first insert, as it may have split the edge the second ray meets
        var hit1 = RayOpenEdge(i1, dir, reach, tolerance);
        int h1 = hit1 == null ? -1 : Insert(hit1, CornerSnap);
        var path = h1 < 0 || h1 == h0 ? null : OpenPath(h0, h1);

        if (path == null || path.Count < 2 || EdgeFaces(path[0], path[1]) is not { Count: 1 } first)
        {
            CopyFrom(before);
            return null;
        }

        return (path, first[0]);
    }

    #endregion

    #region Cutting

    /// <summary>A landing corner this close to a floor vertex is that vertex (vanilla ladders sit up to 4 cm off their landings).</summary>
    public const float CornerSnap = 0.10f;

    /// <summary>
    /// Cuts a polygon (map-local corners, in loop order) into the floor near a height: each corner placed on the floor
    /// (on a vertex within <see cref="CornerSnap"/>) or moved to its nearest open edge within <paramref name="snap"/>,
    /// every side made an edge. A side that can't be cut straight may follow the floor's open edge instead, except the
    /// first when <paramref name="straightFirst"/> (a ladder's foot). Returns the corner vertices and the faces inside,
    /// or null when it can't (the mesh is then as it was).
    /// </summary>
    public (int[] Corners, List<int> Loop, List<int> Faces)? Cut(IReadOnlyList<Vector3> corners, float tolerance, float snap, bool straightFirst = false, float cornerSnap = CornerSnap)
    {
        var before = Clone();
        var indices = new int[corners.Count];

        for (int i = 0; i < corners.Count; i++)
        {
            float wanted = corners[i].Y;
            var location = Locate(XZ(corners[i]), wanted, y => MathF.Abs(y - wanted) <= tolerance, snap);
            if (location == null)
                return Restore(before, $"corner {i} not on a floor");
            indices[i] = Insert(location, cornerSnap);
        }

        if (indices.Distinct().Count() != indices.Length)
            return Restore(before, "corners merged");

        var loop = new List<int>();
        for (int i = 0; i < indices.Length; i++)
        {
            int a = indices[i], b = indices[(i + 1) % indices.Length];
            var afterCorners = Clone();
            if (Recover(a, b))
            {
                loop.Add(a);
                continue;
            }

            CopyFrom(afterCorners);
            var path = i == 0 && straightFirst ? null : OpenPath(a, b);
            if (path == null)
                return Restore(before, $"side {i} not made");
            loop.AddRange(path.Take(path.Count - 1));
        }

        if (loop.Distinct().Count() != loop.Count)
            return Restore(before, "sides cross");

        var inside = FacesInside(loop);
        if (inside == null)
            return Restore(before, "inside not closed");

        return (indices, loop, inside);
    }

    /// <summary>Why the last Cut failed (diagnostics).</summary>
    public string CutFailure;

    private (int[], List<int>, List<int>)? Restore(NgpWorkMesh before, string why)
    {
        CopyFrom(before);
        CutFailure = why;
        return null;
    }

    public void CopyFrom(NgpWorkMesh other)
    {
        Vertices.Clear();
        Vertices.AddRange(other.Vertices);
        VertexFaces.Clear();
        foreach (var vf in other.VertexFaces)
            VertexFaces.Add(new List<int>(vf));
        Faces.Clear();
        Faces.AddRange(other.Faces.Select(f => f == null ? null : (int[])f.Clone()));
        Flags.Clear();
        Flags.AddRange(other.Flags);
        RuntimeSlot = other.RuntimeSlot;
    }

    /// <summary>
    /// Makes the edge a-b single-sided: the faces on the side away from <paramref name="keep"/> (an XZ direction) are cut
    /// off along a strip <paramref name="depth"/> wide and removed, so a strip face added on a-b leaves it manifold. A
    /// face that small already is removed outright. False when the far side can't be cut.
    /// </summary>
    public bool ClearBehind(int a, int b, Vector2 keep, float depth)
    {
        Vector2 A = XZ(Vertices[a]), B = XZ(Vertices[b]);
        List<int> Behind() => EdgeFaces(a, b).Where(f => Vector2.Dot(XZ(Vertices[Third(f, a, b)]) - (A + B) / 2, keep) < 0).ToList();

        var behind = Behind();
        if (behind.Count == 0)
            return true;

        var back = new Vector3(-keep.X, 0, -keep.Y) * depth;
        var cut = Cut(new[] { Vertices[a], Vertices[b], Vertices[b] + back, Vertices[a] + back }, 0.5f, 0.1f, cornerSnap: VertexSnap);
        if (cut is var (_, _, faces))
        {
            foreach (int f in faces)
                RemoveFace(f);
            return Behind().Count == 0;
        }

        // Nothing to cut against: a sliver of floor behind the edge goes
        float area = behind.Sum(f => MathF.Abs(Orient(XZ(Vertices[Faces[f][0]]), XZ(Vertices[Faces[f][1]]), XZ(Vertices[Faces[f][2]]))) / 2);
        if (area > 0.25f)
            return false;
        foreach (int f in behind)
            RemoveFace(f);
        return true;
    }

    #endregion
}

/// <summary>What a strip edit made, for the message and the NavSurvey check.</summary>
public sealed class NgpStripResult
{
    public NGP Ngp;

    /// <summary>The LOC key's suffix of why nothing was made, or null.</summary>
    public string Problem;

    /// <summary>The mesh the strip faces went into.</summary>
    public int Mesh = -1;

    /// <summary>The meshes the edit changed.</summary>
    public List<int> Changed = new();

    public bool BottomJoined, TopJoined;
    public int BottomMesh = -1, TopMesh = -1;
    public float Bottom, Top;
    public int StripFaces, LandingFaces;
    public float MinSlope;

    /// <summary>Why a landing was not joined, per end (diagnostics; NavSurvey prints them).</summary>
    public List<string> Notes = new();

    /// <summary>A ladder's faces against the object's +Z: +1 all along it (as vanilla), -1 all against, 0 mixed.</summary>
    public int Facing;

    /// <summary>A drop's bottom: the floor vertices it joins (2 for a straight cut, more when it follows an open edge).</summary>
    public int BottomPoints;
}

/// <summary>
/// Builds ladder and drop strips into an NGP as the game's data has them (NgpSurvey strips, ladderfit, laddermesh,
/// stripjoin, DS2S):
/// - a ladder is a column of vertical quads 0.70 m wide (type 0x20), at the ladder object's position (median 3 cm off)
///   and from its height (median 4 cm), facing the object's +Z, with a 0.70 m square landing (type 0x18) at each end:
///   the bottom one behind (−Z), the top one ahead (+Z). Both landings are cut into the floors, and the ladder is always
///   in the mesh of a floor; a landing on another mesh's floor meets it through a gate (98 of 98 such edges);
/// - a drop (type 0x40) joins a ledge's open edge to the floor below it.
/// </summary>
public static class NgpStripEdit
{
    public const float LadderWidth = 0.70f;
    public const float LandingDepth = 0.70f;

    /// <summary>The most a ladder quad spans upwards (vanilla quads: median 2 m).</summary>
    public const float LadderRowHeight = 2.5f;

    /// <summary>The thin strip cut from the floor behind a join, so the join's edge has one floor face.</summary>
    public const float BackDepth = 0.05f;

    /// <summary>How far a landing corner may move to reach the floor's open edge.</summary>
    public const float LandingSnap = 1.0f;

    private static (NGP Copy, Dictionary<int, NgpWorkMesh> Work) Begin(NGP ngp) => (NgpEdit.Copy(ngp), new Dictionary<int, NgpWorkMesh>());

    private static NgpWorkMesh Work(NGP copy, Dictionary<int, NgpWorkMesh> work, int mesh)
    {
        if (!work.TryGetValue(mesh, out var w))
            work[mesh] = w = NgpWorkMesh.From(copy.Meshes[mesh]);
        return w;
    }

    private static bool Finish(NGP copy, Dictionary<int, NgpWorkMesh> work, NgpStripResult result)
    {
        var meshes = work.ToDictionary(kv => kv.Key, kv => kv.Value.ToMesh());
        if (meshes.Values.Any(m => !NgpModelEdit.WithinLimits(m)))
        {
            result.Problem = "Limit";
            return false;
        }

        foreach (var (index, mesh) in meshes)
            copy.Meshes[index] = mesh;

        result.Changed = work.Keys.OrderBy(k => k).ToList();
        result.Ngp = NgpRebuild.Rebuild(copy, result.Changed);
        return true;
    }

    /// <summary>The floor under an XZ point among all meshes: a face over it first, else an open edge within the snap.</summary>
    private static (int Mesh, NgpWorkMesh.Location Location)? FindFloor(NGP copy, Dictionary<int, NgpWorkMesh> work, Vector2 p, float preferY, Func<float, bool> accept, float snap)
    {
        (int, NgpWorkMesh.Location)? best = null;
        bool bestInside = false;
        float bestScore = float.MaxValue;

        for (int m = 0; m < copy.Meshes.Count; m++)
        {
            var location = Work(copy, work, m).Locate(p, preferY, accept, snap);
            if (location == null)
                continue;

            bool inside = !location.OnBoundary;
            float score = MathF.Abs(location.Point.Y - preferY) + (inside ? 0 : Vector2.Distance(NgpWorkMesh.XZ(location.Point), p));
            if (inside && !bestInside || inside == bestInside && score < bestScore)
            {
                best = (m, location);
                bestInside = inside;
                bestScore = score;
            }
        }

        // Only the meshes a strip changes keep their working copy
        foreach (var m in work.Keys.ToList())
            if (best == null || m != best.Value.Item1)
                work.Remove(m);

        return best;
    }

    #region Ladder

    /// <summary>
    /// Adds a ladder at a ladder object: <paramref name="foot"/> its map-local position, <paramref name="forward"/> its
    /// +Z (horizontal), <paramref name="topHint"/> the height its top is near (the top of its bounds). The landings are
    /// cut into the floors found behind the foot and ahead of the top; one not on a floor is added unjoined. The ladder
    /// goes into the bottom floor's mesh (else the top's, else <paramref name="fallbackMesh"/>).
    /// </summary>
    public static NgpStripResult WithLadder(NGP ngp, Vector3 foot, Vector3 forward, float topHint, int fallbackMesh = -1)
    {
        var result = new NgpStripResult();
        var (copy, _) = Begin(ngp);

        forward.Y = 0;
        if (forward.LengthSquared() < 1e-8f)
        {
            result.Problem = "Ladder_Facing";
            return result;
        }
        forward = Vector3.Normalize(forward);
        var side = new Vector3(-forward.Z, 0, forward.X) * (LadderWidth / 2);
        var forwardXZ = NgpWorkMesh.XZ(forward);

        // The floors: behind the foot near its height, and ahead of the top near the hint, at least 1 m higher
        var probe = new Dictionary<int, NgpWorkMesh>();
        var bottomFloor = FindFloor(copy, probe, NgpWorkMesh.XZ(foot - forward * (LandingDepth / 2)), foot.Y, y => MathF.Abs(y - foot.Y) <= 1.0f, LandingSnap);
        float bottomY = bottomFloor?.Location.Height ?? foot.Y;

        probe.Clear();
        var topFloor = FindFloor(copy, probe, NgpWorkMesh.XZ(foot + forward * (LandingDepth / 2)), topHint, y => y > bottomY + 1.0f && MathF.Abs(y - topHint) <= 1.5f, LandingSnap);
        float topY = topFloor?.Location.Height ?? topHint;

        if (topY < bottomY + 1.0f)
        {
            result.Problem = "Ladder_Height";
            return result;
        }

        int home = bottomFloor?.Mesh ?? topFloor?.Mesh ?? fallbackMesh;
        if (home < 0 || home >= copy.Meshes.Count)
        {
            result.Problem = "Ladder_No_Floor";
            return result;
        }

        var work = new Dictionary<int, NgpWorkMesh>();
        var homeWork = Work(copy, work, home);
        uint level = NgpFlags.DefaultLevel;

        // A landing: its inner edge on the ladder's plane, reaching away along dir. Returns the inner edge's two
        // vertices in the home mesh, and whether it was cut into a floor
        (int I0, int I1, bool Joined) Landing((int Mesh, NgpWorkMesh.Location Location)? floor, float y, Vector3 dir)
        {
            var basePoint = new Vector3(foot.X, y, foot.Z);
            Vector3 i0 = basePoint - side, i1 = basePoint + side, o0 = i0 + dir * LandingDepth, o1 = i1 + dir * LandingDepth;

            if (floor is var (mesh, _))
            {
                var w = Work(copy, work, mesh);
                var before = w.Clone();
                var cut = w.Cut(new[] { i0, i1, o1, o0 }, 0.75f, LandingSnap, straightFirst: true);
                if (cut == null)
                    result.Notes.Add($"{(Vector3.Dot(dir, forward) > 0 ? "top" : "bottom")}: {w.CutFailure}");
                else if (!w.ClearBehind(cut.Value.Corners[0], cut.Value.Corners[1], NgpWorkMesh.XZ(dir), BackDepth))
                {
                    result.Notes.Add($"{(Vector3.Dot(dir, forward) > 0 ? "top" : "bottom")}: behind not cleared");
                    cut = null;
                }

                if (cut is var (corners, loop, faces))
                {
                    // ClearBehind may have split the landing's faces too: find them again
                    faces = w.FacesInside(loop) ?? faces.Where(f => w.Faces[f] != null).ToList();
                    if (faces.Count > 0)
                        level = NgpFlags.Level(w.Flags[faces[0]]);

                    foreach (int f in faces)
                        w.Flags[f] = NgpFlags.WithType(w.Flags[f] & ~NgpFlags.AttributeMask, 0x18);
                    result.LandingFaces += faces.Count;

                    if (mesh == home)
                        return (corners[0], corners[1], true);

                    // On another mesh's floor: the landing moves into the home mesh, and the hole it leaves meets it
                    // through a gate (the rebuild finds coincident open edges)
                    var moved = new Dictionary<int, int>();
                    int Move(int v) => moved.TryGetValue(v, out var n) ? n : moved[v] = homeWork.AddVertex(w.Vertices[v]);
                    foreach (int f in faces)
                    {
                        var face = w.Faces[f];
                        homeWork.AddFace(Move(face[0]), Move(face[1]), Move(face[2]), w.Flags[f]);
                        w.RemoveFace(f);
                    }
                    return (Move(corners[0]), Move(corners[1]), true);
                }

                w.CopyFrom(before);

                // The floor stops short of the ladder (a generated floor keeps the agent radius back from a ledge): the
                // landing bridges the gap, from the ladder's plane to the floor's open edge ahead along dir
                if (w.OpenEdgeAhead(i0, i1, NgpWorkMesh.XZ(dir), LandingDepth + LandingSnap, 0.75f) is var (path, floorFace))
                {
                    level = NgpFlags.Level(w.Flags[floorFace]);
                    uint bridgeFlags = NgpFlags.WithType(level, 0x18);
                    var target = mesh == home ? w : homeWork;
                    var down = mesh == home ? path : path.Select(v => homeWork.AddVertex(w.Vertices[v])).ToList();
                    int n0 = target.AddVertex(i0), n1 = target.AddVertex(i1);

                    // Against the floor's own winding along the path: the floor face has path0 → path1, or the strip turns round
                    bool floorForward = w.HasDirected(floorFace, path[0], path[1]);
                    var added = floorForward ? AddStrip(target, n0, n1, down, bridgeFlags)
                        : AddStrip(target, n1, n0, down.AsEnumerable().Reverse().ToList(), bridgeFlags);

                    result.LandingFaces += added.Count;
                    result.Notes.Add($"{(Vector3.Dot(dir, forward) > 0 ? "top" : "bottom")}: bridged to the floor's edge ({path.Count} vertices)");
                    return (n0, n1, true);
                }
            }

            // Not on a floor: a free landing quad in the home mesh, wound clockwise from above (normal Y < 0) as floors are
            int a = homeWork.AddVertex(i0), b = homeWork.AddVertex(i1), c = homeWork.AddVertex(o1), d = homeWork.AddVertex(o0);
            uint flags = NgpFlags.WithType(level, 0x18);
            if (Vector3.Cross(i1 - i0, o1 - i0).Y > 0)
            {
                homeWork.AddFace(a, c, b, flags);
                homeWork.AddFace(a, d, c, flags);
            }
            else
            {
                homeWork.AddFace(a, b, c, flags);
                homeWork.AddFace(a, c, d, flags);
            }
            result.LandingFaces += 2;
            return (a, b, false);
        }

        var (b0, b1, bottomJoined) = Landing(bottomFloor, bottomY, -forward);
        var (t0, t1, topJoined) = Landing(topFloor, topY, forward);

        // A floor whose cut failed is as it was: it stays out of the rebuild, so its tables stay byte for byte
        foreach (var m in work.Keys.ToList())
            if (m != home && !(bottomJoined && m == bottomFloor.Value.Mesh) && !(topJoined && m == topFloor.Value.Mesh))
                work.Remove(m);

        result.Mesh = home;
        result.BottomJoined = bottomJoined;
        result.TopJoined = topJoined;
        result.BottomMesh = bottomJoined ? bottomFloor.Value.Mesh : -1;
        result.TopMesh = topJoined ? topFloor.Value.Mesh : -1;
        result.Bottom = homeWork.Vertices[b0].Y;
        result.Top = homeWork.Vertices[t0].Y;

        // The column: rows of quads from the bottom edge to the top edge, wound as the bottom landing's edge needs
        // (the opposite way round to the landing's), which also faces the ladder's normal forward
        int rows = Math.Max(1, (int)MathF.Ceiling((result.Top - result.Bottom) / LadderRowHeight));
        var left = new List<int> { b0 };
        var right = new List<int> { b1 };
        for (int r = 1; r < rows; r++)
        {
            float t = r / (float)rows;
            left.Add(homeWork.AddVertex(Vector3.Lerp(homeWork.Vertices[b0], homeWork.Vertices[t0], t)));
            right.Add(homeWork.AddVertex(Vector3.Lerp(homeWork.Vertices[b1], homeWork.Vertices[t1], t)));
        }
        left.Add(t0);
        right.Add(t1);

        var landing = homeWork.EdgeFaces(b0, b1).FirstOrDefault(-1);
        bool reverse = landing >= 0 && homeWork.HasDirected(landing, b0, b1);
        uint ladderFlags = NgpFlags.WithType(level, 0x20);
        var added = new List<int>();

        for (int r = 0; r < rows; r++)
        {
            int p0 = left[r], p1 = right[r], q0 = left[r + 1], q1 = right[r + 1];
            // Loop p0 → p1 → q1 → q0 has the bottom edge p0 → p1
            if (!reverse)
            {
                added.Add(homeWork.AddFace(p0, p1, q1, ladderFlags));
                added.Add(homeWork.AddFace(p0, q1, q0, ladderFlags));
            }
            else
            {
                added.Add(homeWork.AddFace(p1, p0, q0, ladderFlags));
                added.Add(homeWork.AddFace(p1, q0, q1, ladderFlags));
            }
        }

        result.MinSlope = MinSlope(homeWork, added);
        result.Facing = Facing(homeWork, added, forward);
        result.StripFaces = added.Count;
        Finish(copy, work, result);
        return result;
    }

    private static float MinSlope(NgpWorkMesh w, IEnumerable<int> faces)
    {
        float min = 90;
        foreach (int f in faces)
        {
            var face = w.Faces[f];
            var n = Vector3.Cross(w.Vertices[face[1]] - w.Vertices[face[0]], w.Vertices[face[2]] - w.Vertices[face[0]]);
            if (n.Length() > 1e-6f)
                min = MathF.Min(min, MathF.Acos(Math.Clamp(MathF.Abs(n.Y) / n.Length(), 0, 1)) * 180 / MathF.PI);
        }
        return min;
    }

    /// <summary>+1 when every face's normal points along a direction (vanilla ladders: the object's +Z), -1 when every one points against it, else 0.</summary>
    private static int Facing(NgpWorkMesh w, IEnumerable<int> faces, Vector3 direction)
    {
        int along = 0, against = 0;
        foreach (int f in faces)
        {
            var face = w.Faces[f];
            var n = Vector3.Cross(w.Vertices[face[1]] - w.Vertices[face[0]], w.Vertices[face[2]] - w.Vertices[face[0]]);
            if (Vector3.Dot(n, direction) > 0) along++; else against++;
        }
        return against == 0 ? 1 : along == 0 ? -1 : 0;
    }

    #endregion

    #region Strips

    /// <summary>
    /// Faces between an edge [t0, t1] and a path [down0 … downK] (down0 opposite t0), wound as the loop t0 → t1 →
    /// downK → … → down0, always along the shorter diagonal. A face on the path's side then has each path edge as
    /// down(j+1) → down(j), the other way round to a floor face that has down(j) → down(j+1).
    /// </summary>
    private static List<int> AddStrip(NgpWorkMesh w, int t0, int t1, IReadOnlyList<int> down, uint flags)
    {
        var added = new List<int>();
        int i = 0, j = 0, k = down.Count - 1;
        int[] topIndex = { t0, t1 };
        while (i < 1 || j < k)
        {
            bool advanceTop = j == k || i < 1 && Vector3.Distance(w.Vertices[t1], w.Vertices[down[j]]) < Vector3.Distance(w.Vertices[topIndex[i]], w.Vertices[down[j + 1]]);
            if (advanceTop)
            {
                added.Add(w.AddFace(t0, t1, down[j], flags));
                i = 1;
            }
            else
            {
                added.Add(w.AddFace(topIndex[i], down[j + 1], down[j], flags));
                j++;
            }
        }
        return added;
    }

    #endregion

    #region Drop

    /// <summary>The open edge (not a gate's) of a mesh nearest a map-local point, as its two vertex indices, or null.</summary>
    public static (int A, int B)? NearestOpenEdge(NGP ngp, int mesh, Vector3 point, float maxDistance = 3f)
    {
        if (mesh < 0 || mesh >= ngp.Meshes.Count)
            return null;

        var m = ngp.Meshes[mesh];
        var gated = ngp.GateEdgeRefs.Where(r => NGP.Ids.MeshOf(r) == mesh).Select(r => NGP.Ids.IndexOf(r)).ToHashSet();
        (int, int)? best = null;
        float bestDistance = maxDistance;

        for (int e = 0; e < m.Edges.Count; e++)
        {
            var edge = m.Edges[e];
            bool open = edge.Face1 < 0 || edge.Face1 >= m.Faces.Count;
            if (!open || gated.Contains(e) || edge.VertexA < 0 || edge.VertexB < 0 || edge.VertexA >= m.Vertices.Count || edge.VertexB >= m.Vertices.Count)
                continue;

            Vector3 a = m.Vertices[edge.VertexA], b = m.Vertices[edge.VertexB];
            var ab = b - a;
            float t = ab.LengthSquared() < 1e-12f ? 0 : Math.Clamp(Vector3.Dot(point - a, ab) / ab.LengthSquared(), 0, 1);
            float distance = Vector3.Distance(point, a + ab * t);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = (edge.VertexA, edge.VertexB);
            }
        }

        return best;
    }

    /// <summary>
    /// Adds a drop from a ledge, the open edge a-b of mesh <paramref name="mesh"/>, to the floor below it: each end's
    /// floor found <paramref name="run"/> metres out from the ledge and between <paramref name="minDrop"/> and
    /// <paramref name="maxDrop"/> lower (an open edge within <paramref name="snap"/> if no face is there). The bottom
    /// edge is cut into that floor and the floor behind it (under the ledge) cut away, so the drop's faces (type 0x40,
    /// the ledge's level) join both floors manifold; on another mesh's floor the bottom meets it through a gate.
    /// </summary>
    public static NgpStripResult WithDrop(NGP ngp, int mesh, int a, int b, float run = 0.5f, float minDrop = 0.5f, float maxDrop = 30f, float snap = 1.5f)
    {
        var result = new NgpStripResult { Mesh = mesh };
        var (copy, work) = Begin(ngp);

        if (mesh < 0 || mesh >= copy.Meshes.Count)
        {
            result.Problem = "Drop_No_Edge";
            return result;
        }

        var upper = Work(copy, work, mesh);
        if (a < 0 || b < 0 || a >= upper.Vertices.Count || b >= upper.Vertices.Count)
        {
            result.Problem = "Drop_No_Edge";
            return result;
        }

        var ledge = upper.EdgeFaces(a, b);
        if (ledge.Count != 1)
        {
            result.Problem = "Drop_No_Edge";
            return result;
        }

        int top = ledge[0];
        Vector3 A = upper.Vertices[a], B = upper.Vertices[b];
        var faceCentre = (upper.Vertices[upper.Faces[top][0]] + upper.Vertices[upper.Faces[top][1]] + upper.Vertices[upper.Faces[top][2]]) / 3;
        var outward = new Vector3(-(B.Z - A.Z), 0, B.X - A.X);
        if (outward.LengthSquared() < 1e-8f)
        {
            result.Problem = "Drop_No_Edge";
            return result;
        }
        outward = Vector3.Normalize(outward);
        if (Vector3.Dot(outward, faceCentre - (A + B) / 2) > 0)
            outward = -outward;

        // The floor below each end, in one mesh: a wall's foot first (an open edge of a lower floor between the ledge
        // and the run out from it), else the floor face the run reaches, else an open edge within the snap
        float highest = MathF.Max(A.Y, B.Y);
        Func<float, bool> Below(Vector3 p) => y => y <= p.Y - minDrop && y >= p.Y - maxDrop;
        float footReach = run + 0.25f;

        NgpWorkMesh.Location Place(NgpWorkMesh w, Vector3 p)
        {
            var foot = w.LocateOpenEdge(NgpWorkMesh.XZ(p + outward * (run / 2)), p.Y - minDrop, Below(p), footReach / 2 + 0.05f);

            // Only a foot whose floor reaches out from it (not a floor under the ledge ending there)
            if (foot != null)
            {
                var face = w.Faces[foot.Face];
                var centre = (w.Vertices[face[0]] + w.Vertices[face[1]] + w.Vertices[face[2]]) / 3;
                if (Vector3.Dot(centre - foot.Point, outward) <= 0)
                    foot = null;
            }

            return foot ?? w.Locate(NgpWorkMesh.XZ(p + outward * run), p.Y - minDrop, Below(p), snap);
        }

        int lowerMesh = -1;
        float lowerScore = float.MaxValue;
        for (int m = 0; m < copy.Meshes.Count; m++)
        {
            var w = m == mesh ? upper : NgpWorkMesh.From(copy.Meshes[m]);
            if (Place(w, A) is not NgpWorkMesh.Location la0 || Place(w, B) is not NgpWorkMesh.Location lb0)
                continue;
            // The nearest floor below the ledge
            float score = A.Y - la0.Height + B.Y - lb0.Height;
            if (score < lowerScore)
            {
                lowerScore = score;
                lowerMesh = m;
            }
        }

        if (lowerMesh < 0)
        {
            result.Problem = "Drop_No_Floor";
            return result;
        }

        var lower = Work(copy, work, lowerMesh);
        var before = lower.Clone();
        var beforeUpper = ReferenceEquals(lower, upper) ? null : upper.Clone();

        void Undo()
        {
            lower.CopyFrom(before);
            if (beforeUpper != null)
                upper.CopyFrom(beforeUpper);
        }

        int la = lower.Insert(Place(lower, A));
        var locB = Place(lower, B);
        int lb = locB == null ? -1 : lower.Insert(locB);

        // The bottom: a straight cut through the floor, its far side (under the ledge) cut away; else the floor's own
        // open edges between the two ends (a wall's foot, often split into several edges)
        List<int> bottom = null;
        if (lb >= 0 && la != lb)
        {
            var afterInsert = lower.Clone();
            if (lower.Recover(la, lb) && lower.ClearBehind(la, lb, NgpWorkMesh.XZ(outward), BackDepth))
                bottom = new List<int> { la, lb };
            else
            {
                lower.CopyFrom(afterInsert);
                bottom = lower.OpenPath(la, lb);
            }
        }

        if (bottom == null)
        {
            Undo();
            result.Problem = "Drop_No_Cut";
            return result;
        }

        // The ledge face may have been split by the cut (one mesh): find the face on a-b again
        ledge = upper.EdgeFaces(a, b);
        if (ledge.Count != 1)
        {
            Undo();
            result.Problem = "Drop_No_Cut";
            return result;
        }
        top = ledge[0];

        // In another mesh the bottom gets its own vertices, and meets that floor through a gate
        var down = lowerMesh == mesh ? bottom : bottom.Select(v => upper.AddVertex(lower.Vertices[v])).ToList();

        // Wound the other way round to the ledge face's edge, as a manifold mesh needs: the loop t0 → t1, then the
        // bottom back from t1's end to t0's
        bool forward = upper.HasDirected(top, a, b);
        int t0 = forward ? b : a, t1 = forward ? a : b;
        if (t0 != a)
            down.Reverse();
        uint flags = NgpFlags.WithType(NgpFlags.Level(upper.Flags[top]), 0x40);

        var added = AddStrip(upper, t0, t1, down, flags);

        result.StripFaces = added.Count;
        result.BottomPoints = bottom.Count;
        result.TopJoined = true;
        result.BottomJoined = true;
        result.BottomMesh = lowerMesh;
        result.Top = highest;
        result.Bottom = bottom.Min(v => lower.Vertices[v].Y);
        result.MinSlope = MinSlope(upper, added);

        Finish(copy, work, result);
        return result;
    }

    #endregion
}
