using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The generator's last, optional step (NavGenParameters.Simplify): the mesh cleaned up as an artist would.
/// - The faces are welded into one triangle mesh.
/// - Short edges are collapsed, one end moved onto the other (no new positions, so every vertex stays on the
///   collision). A collapse is made only when:
///   - the mesh stays manifold (the link condition);
///   - no face turns over seen from above or gets thinner than it was (or 10°);
///   - every changed face stays within the error of the collision, at its centre and edge middles, and at the removed
///     vertex;
///   - new edges are no longer than the max edge length;
///   - a vertex on the outline only goes along it, and only where the outline runs straight through it (within 3 cm,
///     and the error in height), so the outline, blocker cuts and map borders keep their shape.
/// - Sliver triangles are then flipped where the other diagonal of their quad gives a fatter pair, under the same tests.
/// Renderer-free, so NavSurvey checks it.
/// </summary>
public static class NavSimplify
{
    private sealed class Mesh
    {
        public readonly List<Vector3> V = new();
        public readonly List<int[]> F = new();
        public readonly List<HashSet<int>> VF = new();

        public int Add(Vector3 p) { V.Add(p); VF.Add(new HashSet<int>()); return V.Count - 1; }

        public int AddFace(int a, int b, int c)
        {
            F.Add(new[] { a, b, c });
            int f = F.Count - 1;
            VF[a].Add(f); VF[b].Add(f); VF[c].Add(f);
            return f;
        }

        public void Set(int f, int a, int b, int c)
        {
            foreach (var v in F[f]) VF[v].Remove(f);
            F[f] = new[] { a, b, c };
            VF[a].Add(f); VF[b].Add(f); VF[c].Add(f);
        }

        public void Remove(int f)
        {
            foreach (var v in F[f]) VF[v].Remove(f);
            F[f] = null;
        }

        public List<int> EdgeFaces(int a, int b) => VF[a].Where(f => Array.IndexOf(F[f], b) >= 0).ToList();

        public IEnumerable<int> Neighbours(int v) => VF[v].SelectMany(f => F[f]).Where(w => w != v).Distinct();

        public bool IsBoundaryEdge(int a, int b) => EdgeFaces(a, b).Count == 1;

        public List<int> BoundaryNeighbours(int v) => Neighbours(v).Where(w => IsBoundaryEdge(v, w)).ToList();
    }

    private static float Cross2(Vector3 a, Vector3 b, Vector3 c) => (b.X - a.X) * (c.Z - a.Z) - (b.Z - a.Z) * (c.X - a.X);

    private static float LengthXZ(Vector3 a, Vector3 b) => new Vector2(b.X - a.X, b.Z - a.Z).Length();

    /// <summary>The smallest angle of a triangle seen from above, in degrees (0 for a degenerate one).</summary>
    private static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        static float Angle(Vector3 p, Vector3 q, Vector3 r)
        {
            var u = new Vector2(q.X - p.X, q.Z - p.Z);
            var w = new Vector2(r.X - p.X, r.Z - p.Z);
            float l = u.Length() * w.Length();
            return l < 1e-9f ? 0 : MathF.Acos(Math.Clamp(Vector2.Dot(u, w) / l, -1, 1)) * 180 / MathF.PI;
        }
        return MathF.Min(Angle(a, b, c), MathF.Min(Angle(b, c, a), Angle(c, a, b)));
    }

    internal static List<List<Vector3>> Run(List<List<Vector3>> polygons, NavigationGenerator.SurfaceGrid surface, NavGenParameters p, out int removed, out int flipped)
    {
        removed = 0;
        flipped = 0;
        float range = p.MaxClimb + 2 * p.CellHeight;
        float error = MathF.Max(0.01f, p.SimplifyError);
        float maxEdge = p.MaxEdgeLength > 0 ? p.MaxEdgeLength : 12f;

        // 1. One welded triangle mesh, each polygon fanned (they are convex)
        var mesh = new Mesh();
        var index = new Dictionary<Vector3, int>();
        int V(Vector3 q) => index.TryGetValue(q, out var i) ? i : index[q] = mesh.Add(q);
        foreach (var polygon in polygons)
        {
            var ids = polygon.Select(V).ToList();
            for (int k = 1; k + 1 < ids.Count; k++)
                if (ids[0] != ids[k] && ids[k] != ids[k + 1] && ids[0] != ids[k + 1])
                    mesh.AddFace(ids[0], ids[k], ids[k + 1]);
        }

        // The sign of a face's area seen from above: every face keeps its own
        var sign = mesh.F.Select(f => MathF.Sign(Cross2(mesh.V[f[0]], mesh.V[f[1]], mesh.V[f[2]]))).ToList();

        bool OnCollision(Vector3 q) => surface.Nearest(q, range) is float y && MathF.Abs(y - q.Y) <= error;

        bool FaceFits(Vector3 a, Vector3 b, Vector3 c)
            => OnCollision((a + b + c) / 3) && OnCollision((a + b) / 2) && OnCollision((b + c) / 2) && OnCollision((c + a) / 2);

        // The height of the face over a point, if the point is inside it seen from above
        static float? HeightIn(Vector3 a, Vector3 b, Vector3 c, Vector3 q)
        {
            float d = Cross2(a, b, c);
            if (MathF.Abs(d) < 1e-9f) return null;
            float wa = Cross2(b, c, q) / d, wb = Cross2(c, a, q) / d, wc = 1 - wa - wb;
            if (wa < -1e-4f || wb < -1e-4f || wc < -1e-4f) return null;
            return wa * a.Y + wb * b.Y + wc * c.Y;
        }

        // 2. Collapses, u onto v
        bool TryCollapse(int u, int v)
        {
            var shared = mesh.EdgeFaces(u, v);
            if (shared.Count is not (1 or 2))
                return false;

            // Manifold: the only neighbours u and v share are the far corners of the faces on the edge
            var thirds = shared.Select(f => mesh.F[f].First(w => w != u && w != v)).ToHashSet();
            var common = mesh.Neighbours(u).Intersect(mesh.Neighbours(v)).ToHashSet();
            if (!common.SetEquals(thirds))
                return false;

            // The outline: a vertex on it only goes along it, where it runs straight through
            var uBoundary = mesh.BoundaryNeighbours(u);
            if (uBoundary.Count > 0)
            {
                if (uBoundary.Count != 2 || !uBoundary.Contains(v) || shared.Count != 1)
                    return false;
                int w = uBoundary.First(x => x != v);
                Vector3 a = mesh.V[w], b = mesh.V[v], q = mesh.V[u];
                var ab = new Vector2(b.X - a.X, b.Z - a.Z);
                float len = ab.Length();
                if (len < 1e-6f) return false;
                float off = MathF.Abs(ab.X * (q.Z - a.Z) - ab.Y * (q.X - a.X)) / len;
                float t = Math.Clamp(Vector2.Dot(new Vector2(q.X - a.X, q.Z - a.Z), ab) / (len * len), 0, 1);
                if (off > 0.03f || MathF.Abs(a.Y + (b.Y - a.Y) * t - q.Y) > error)
                    return false;
            }
            else if (shared.Count != 2)
                return false;

            // The faces round u that stay, with u moved onto v
            var target = mesh.V[v];
            var removedPoint = mesh.V[u];
            bool coveredRemoved = false;
            foreach (int f in mesh.VF[u])
            {
                if (shared.Contains(f)) continue;
                var face = mesh.F[f].Select(x => x == u ? v : x).ToArray();
                Vector3 a = mesh.V[face[0]], b = mesh.V[face[1]], c = mesh.V[face[2]];

                float area = Cross2(a, b, c);
                if (MathF.Sign(area) != sign[f] || MathF.Abs(area) < 1e-4f)
                    return false;

                var old = mesh.F[f];
                float before = MinAngle(mesh.V[old[0]], mesh.V[old[1]], mesh.V[old[2]]);
                if (MinAngle(a, b, c) < MathF.Min(before, 10f))
                    return false;

                foreach (int x in face)
                    if (x != v && LengthXZ(target, mesh.V[x]) > maxEdge)
                        return false;

                if (!FaceFits(a, b, c))
                    return false;

                if (HeightIn(a, b, c, removedPoint) is float y)
                {
                    coveredRemoved = true;
                    if (MathF.Abs(y - removedPoint.Y) > error)
                        return false;
                }
            }

            // An interior vertex must lie under the new faces (else they fold over its old place)
            if (uBoundary.Count == 0 && !coveredRemoved)
                return false;

            foreach (int f in shared)
                mesh.Remove(f);
            foreach (int f in mesh.VF[u].ToList())
            {
                var face = mesh.F[f].Select(x => x == u ? v : x).ToArray();
                mesh.Set(f, face[0], face[1], face[2]);
            }
            return true;
        }

        for (int pass = 0; pass < 6; pass++)
        {
            var edges = new HashSet<(int, int)>();
            foreach (var f in mesh.F)
                if (f != null)
                    for (int k = 0; k < 3; k++)
                    {
                        int a = f[k], b = f[(k + 1) % 3];
                        edges.Add((Math.Min(a, b), Math.Max(a, b)));
                    }

            int collapsed = 0;
            foreach (var (a, b) in edges.OrderBy(e => LengthXZ(mesh.V[e.Item1], mesh.V[e.Item2])))
            {
                if (mesh.VF[a].Count == 0 || mesh.VF[b].Count == 0 || mesh.EdgeFaces(a, b).Count == 0)
                    continue;
                if (TryCollapse(a, b) || TryCollapse(b, a))
                    collapsed++;
            }

            removed += collapsed;
            if (collapsed == 0)
                break;
        }

        // 3. Flips: a sliver pair turned onto its quad's other diagonal when that is fatter
        for (int pass = 0; pass < 4; pass++)
        {
            int count = 0;
            for (int f1 = 0; f1 < mesh.F.Count; f1++)
            {
                if (mesh.F[f1] == null) continue;
                for (int k = 0; k < 3 && mesh.F[f1] != null; k++)
                {
                    int s = mesh.F[f1][k], t = mesh.F[f1][(k + 1) % 3], c = mesh.F[f1][(k + 2) % 3];
                    var faces = mesh.EdgeFaces(s, t);
                    if (faces.Count != 2) continue;
                    int f2 = faces[0] == f1 ? faces[1] : faces[0];
                    var g = mesh.F[f2];
                    // f2 must run t → s (the same winding)
                    int j = Array.IndexOf(g, t);
                    if (g[(j + 1) % 3] != s) continue;
                    int d = g[(j + 2) % 3];
                    if (c == d || mesh.EdgeFaces(c, d).Count > 0) continue;

                    Vector3 S = mesh.V[s], T = mesh.V[t], C = mesh.V[c], D = mesh.V[d];
                    float before = MathF.Min(MinAngle(S, T, C), MinAngle(T, S, D));
                    float after = MathF.Min(MinAngle(C, S, D), MinAngle(D, T, C));
                    if (after <= before + 1f) continue;

                    // Convex seen from above: the new faces keep the winding
                    if (MathF.Sign(Cross2(C, S, D)) != sign[f1] || MathF.Sign(Cross2(D, T, C)) != sign[f1]) continue;
                    if (MathF.Abs(Cross2(C, S, D)) < 1e-4f || MathF.Abs(Cross2(D, T, C)) < 1e-4f) continue;
                    if (LengthXZ(C, D) > maxEdge || !FaceFits(C, S, D) || !FaceFits(D, T, C)) continue;

                    // (s, t, c) and (t, s, d) become (c, s, d) and (d, t, c)
                    mesh.Set(f1, c, s, d);
                    mesh.Set(f2, d, t, c);
                    sign[f2] = sign[f1];
                    count++;
                }
            }

            flipped += count;
            if (count == 0)
                break;
        }

        return mesh.F.Where(f => f != null).Select(f => new List<Vector3> { mesh.V[f[0]], mesh.V[f[1]], mesh.V[f[2]] }).ToList();
    }
}
