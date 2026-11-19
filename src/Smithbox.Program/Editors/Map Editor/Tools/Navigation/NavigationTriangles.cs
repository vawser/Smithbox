using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Triangles face shape (NavGenParameters.Shape): Recast's faces as triangles of varied size, as the game's navmeshes
/// have them, instead of grid tiles.
/// - Each face is fanned into triangles (Recast makes triangles already for this shape; the fan covers the rest).
/// - Edges longer than NavGenParameters.TriangleEdge across are halved, in rounds. An edge is split at the same point in
///   both faces on it (keyed by its two ends in a fixed order), so no crack opens; the new corner goes onto the collision.
///   A triangle with one split edge becomes two, with two splits three, with three four (its corners and the midpoints).
/// - Slivers are then flipped: two triangles sharing an edge are turned onto their quad's other diagonal when that makes
///   their smallest angle larger, the quad is convex seen from above, and the new edge lies on the collision.
/// Renderer-free, so NavSurvey can run it.
/// </summary>
public static class NavTriangles
{
    /// <summary>The faces as refined triangles; split is the number of edges halved, flipped the number of flips.</summary>
    internal static List<List<Vector3>> Refine(List<List<Vector3>> polygons, NavigationGenerator.SurfaceGrid surface, NavGenParameters p,
        out int split, out int flipped)
    {
        split = 0;
        float range = p.MaxClimb + 2 * p.CellHeight;
        Vector3 OnCollision(Vector3 q) => surface?.Nearest(q, range) is float y ? q with { Y = y } : q;

        var triangles = Triangulate(polygons);

        // Long edges halved, until none is longer than the limit (each round halves them, so a few rounds do)
        if (p.TriangleEdge > 0)
        {
            float limit = MathF.Max(0.5f, p.TriangleEdge);
            for (int round = 0; round < 12; round++)
            {
                var splits = new Dictionary<(Vector3, Vector3), Vector3>();
                foreach (var t in triangles)
                    for (int k = 0; k < 3; k++)
                    {
                        var key = Key(t[k], t[(k + 1) % 3]);
                        if (!splits.ContainsKey(key) && LengthXZ(key.Item1, key.Item2) > limit)
                            splits[key] = OnCollision((key.Item1 + key.Item2) / 2);
                    }

                if (splits.Count == 0)
                    break;
                split += splits.Count;

                var next = new List<List<Vector3>>(triangles.Count * 2);
                foreach (var t in triangles)
                    next.AddRange(Subdivide(t, splits));
                triangles = next;
            }
        }

        triangles = Flip(triangles, surface, p, out flipped);
        return triangles;
    }

    /// <summary>Faces as triangles: a triangle as it is, a larger (convex) face fanned from its first corner.</summary>
    internal static List<List<Vector3>> Triangulate(List<List<Vector3>> polygons)
    {
        var result = new List<List<Vector3>>(polygons.Count);
        foreach (var polygon in polygons)
        {
            if (polygon.Count == 3)
            {
                result.Add(polygon);
                continue;
            }
            for (int k = 1; k + 1 < polygon.Count; k++)
            {
                var t = new List<Vector3> { polygon[0], polygon[k], polygon[k + 1] };
                if (AreaXZ(t) > 1e-6f)
                    result.Add(t);
            }
        }
        return result;
    }

    /// <summary>A triangle cut at the midpoints of its split edges, its winding kept.</summary>
    private static IEnumerable<List<Vector3>> Subdivide(List<Vector3> t, Dictionary<(Vector3, Vector3), Vector3> splits)
    {
        var m = new Vector3?[3];
        int count = 0;
        for (int k = 0; k < 3; k++)
            if (splits.TryGetValue(Key(t[k], t[(k + 1) % 3]), out var mid))
            {
                m[k] = mid;
                count++;
            }

        if (count == 0)
        {
            yield return t;
            yield break;
        }

        if (count == 3)
        {
            // Corners and midpoints: three corner triangles and the middle one
            Vector3 m0 = m[0].Value, m1 = m[1].Value, m2 = m[2].Value;
            yield return new List<Vector3> { t[0], m0, m2 };
            yield return new List<Vector3> { m0, t[1], m1 };
            yield return new List<Vector3> { m2, m1, t[2] };
            yield return new List<Vector3> { m0, m1, m2 };
            yield break;
        }

        if (count == 1)
        {
            // a → mid → b of the split edge, then c: two triangles from c
            int k = Array.FindIndex(m, x => x != null);
            Vector3 a = t[k], b = t[(k + 1) % 3], c = t[(k + 2) % 3], mid = m[k].Value;
            yield return new List<Vector3> { a, mid, c };
            yield return new List<Vector3> { mid, b, c };
            yield break;
        }

        // Two splits: the unsplit edge is k → k+1; the corner between the split edges gets its own triangle, and the
        // quad left is cut along its shorter diagonal
        {
            int k = Array.FindIndex(m, x => x == null);
            Vector3 a = t[k], b = t[(k + 1) % 3], c = t[(k + 2) % 3];
            Vector3 mbc = m[(k + 1) % 3].Value, mca = m[(k + 2) % 3].Value;
            yield return new List<Vector3> { mbc, c, mca };

            // Quad a, b, mbc, mca
            if (LengthXZ(a, mbc) <= LengthXZ(b, mca))
            {
                yield return new List<Vector3> { a, b, mbc };
                yield return new List<Vector3> { a, mbc, mca };
            }
            else
            {
                yield return new List<Vector3> { a, b, mca };
                yield return new List<Vector3> { b, mbc, mca };
            }
        }
    }

    /// <summary>Sliver pairs turned onto their quad's other diagonal, where that is fatter and stays on the collision.</summary>
    internal static List<List<Vector3>> Flip(List<List<Vector3>> triangles, NavigationGenerator.SurfaceGrid surface, NavGenParameters p, out int flipped)
    {
        flipped = 0;
        float range = p.MaxClimb + 2 * p.CellHeight;
        float error = MathF.Max(0.15f, p.ContourError);
        float maxEdge = p.TriangleEdge > 0 ? p.TriangleEdge * 1.5f : float.MaxValue;

        // Welded: shared corners are the same points
        var points = new List<Vector3>();
        var index = new Dictionary<Vector3, int>();
        int V(Vector3 q)
        {
            if (index.TryGetValue(q, out var i)) return i;
            index[q] = points.Count;
            points.Add(q);
            return points.Count - 1;
        }

        var faces = triangles.Select(t => new[] { V(t[0]), V(t[1]), V(t[2]) }).Where(f => f[0] != f[1] && f[1] != f[2] && f[0] != f[2]).ToList();
        var sign = faces.Select(f => MathF.Sign(Cross2(points[f[0]], points[f[1]], points[f[2]]))).ToList();

        // Each directed edge's face
        var owner = new Dictionary<(int, int), int>();
        void Own(int f)
        {
            var t = faces[f];
            for (int k = 0; k < 3; k++)
                owner[(t[k], t[(k + 1) % 3])] = f;
        }
        void Disown(int f)
        {
            var t = faces[f];
            for (int k = 0; k < 3; k++)
                if (owner.TryGetValue((t[k], t[(k + 1) % 3]), out var o) && o == f)
                    owner.Remove((t[k], t[(k + 1) % 3]));
        }
        for (int f = 0; f < faces.Count; f++)
            Own(f);

        bool OnCollision(Vector3 q) => surface == null || (surface.Nearest(q, range) is float y && MathF.Abs(y - q.Y) <= error);

        for (int pass = 0; pass < 6; pass++)
        {
            int count = 0;
            for (int f1 = 0; f1 < faces.Count; f1++)
            {
                for (int k = 0; k < 3; k++)
                {
                    var g = faces[f1];
                    int s = g[k], t = g[(k + 1) % 3], c = g[(k + 2) % 3];
                    if (!owner.TryGetValue((t, s), out int f2) || f2 == f1)
                        continue;

                    var h = faces[f2];
                    int j = Array.IndexOf(h, t);
                    if (h[(j + 1) % 3] != s)
                        continue;
                    int d = h[(j + 2) % 3];
                    if (c == d || owner.ContainsKey((c, d)) || owner.ContainsKey((d, c)))
                        continue;

                    Vector3 S = points[s], T = points[t], C = points[c], D = points[d];
                    float before = MathF.Min(MinAngle(S, T, C), MinAngle(T, S, D));
                    float after = MathF.Min(MinAngle(C, S, D), MinAngle(D, T, C));
                    if (after <= before + 1f)
                        continue;

                    // Convex seen from above: both new triangles keep the winding, with area
                    float a1 = Cross2(C, S, D), a2 = Cross2(D, T, C);
                    if (MathF.Sign(a1) != sign[f1] || MathF.Sign(a2) != sign[f1] || MathF.Abs(a1) < 1e-4f || MathF.Abs(a2) < 1e-4f)
                        continue;

                    // The new edge on the collision, and no longer than the limit allows
                    if (LengthXZ(C, D) > maxEdge || !OnCollision((C + D) / 2) || !OnCollision((C + S + D) / 3) || !OnCollision((D + T + C) / 3))
                        continue;

                    Disown(f1);
                    Disown(f2);
                    faces[f1] = new[] { c, s, d };
                    faces[f2] = new[] { d, t, c };
                    sign[f2] = sign[f1];
                    Own(f1);
                    Own(f2);
                    count++;
                }
            }

            flipped += count;
            if (count == 0)
                break;
        }

        return faces.Select(f => new List<Vector3> { points[f[0]], points[f[1]], points[f[2]] }).ToList();
    }

    private static (Vector3, Vector3) Key(Vector3 a, Vector3 b)
        => a.X < b.X || a.X == b.X && (a.Z < b.Z || a.Z == b.Z && a.Y <= b.Y) ? (a, b) : (b, a);

    private static float LengthXZ(Vector3 a, Vector3 b) => new Vector2(b.X - a.X, b.Z - a.Z).Length();

    private static float Cross2(Vector3 a, Vector3 b, Vector3 c) => (b.X - a.X) * (c.Z - a.Z) - (b.Z - a.Z) * (c.X - a.X);

    private static float AreaXZ(List<Vector3> t) => MathF.Abs(Cross2(t[0], t[1], t[2])) / 2;

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
}
