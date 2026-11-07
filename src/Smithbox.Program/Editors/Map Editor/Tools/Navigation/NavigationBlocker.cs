using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A box that navmesh generation goes around (Navmesh Generation Plan, Blockers): Origin + a·AxisX + b·AxisY + c·AxisZ
/// for a, b, c in [0, 1]. The axes may be any lengths and directions (a rotated, scaled part), but are taken to be at right
/// angles, as a part's transform keeps them. AxisY is the box's up.
/// </summary>
public sealed record NavBlocker(Vector3 Origin, Vector3 AxisX, Vector3 AxisY, Vector3 AxisZ)
{
    /// <summary>A box from its eight corners, numbered with the high X, Y, Z side for bits 1, 2, 4.</summary>
    public static NavBlocker FromCorners(IReadOnlyList<Vector3> c) => new(c[0], c[1] - c[0], c[2] - c[0], c[4] - c[0]);

    /// <summary>The eight corners, numbered with the high X, Y, Z side for bits 1, 2, 4.</summary>
    public List<Vector3> Corners()
    {
        var corners = new List<Vector3>(8);
        for (int i = 0; i < 8; i++)
            corners.Add(Origin + ((i & 1) != 0 ? AxisX : Vector3.Zero) + ((i & 2) != 0 ? AxisY : Vector3.Zero) + ((i & 4) != 0 ? AxisZ : Vector3.Zero));
        return corners;
    }

    /// <summary>The box's twelve triangles (three points each).</summary>
    public List<Vector3> Triangles()
    {
        var c = Corners();
        var quads = new[] { (0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3) };
        var triangles = new List<Vector3>(36);
        foreach (var (a, b, d, e) in quads)
        {
            triangles.Add(c[a]); triangles.Add(c[b]); triangles.Add(c[d]);
            triangles.Add(c[a]); triangles.Add(c[d]); triangles.Add(c[e]);
        }
        return triangles;
    }

    /// <summary>
    /// The box grown by a margin across its two horizontal-most axes, and extended below its base by a depth (along the
    /// axis nearest to world up, on its lower side), so floor touching or just under it counts as inside.
    /// </summary>
    public NavBlocker Widened(float margin, float depthBelow)
    {
        var axes = new[] { AxisX, AxisY, AxisZ };
        int up = Enumerable.Range(0, 3).OrderByDescending(i => MathF.Abs(Vector3.Normalize(axes[i]).Y)).First();
        var origin = Origin;
        var grown = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            var dir = axes[i].LengthSquared() > 1e-12f ? Vector3.Normalize(axes[i]) : Vector3.Zero;
            if (i == up)
            {
                // Down along up's lower side: the side the axis points away from if it points up, else the other
                bool pointsUp = dir.Y >= 0;
                grown[i] = axes[i] + dir * depthBelow;
                if (pointsUp)
                    origin -= dir * depthBelow;
            }
            else
            {
                grown[i] = axes[i] + dir * 2 * margin;
                origin -= dir * margin;
            }
        }

        return new NavBlocker(origin, grown[0], grown[1], grown[2]);
    }

    /// <summary>Whether a point is inside the box (within a tolerance).</summary>
    public bool Contains(Vector3 point, float tolerance = 0)
    {
        var d = point - Origin;
        foreach (var axis in new[] { AxisX, AxisY, AxisZ })
        {
            float length = axis.Length();
            if (length < 1e-6f)
                return false;
            float t = Vector3.Dot(d, axis / length);
            if (t < -tolerance || t > length + tolerance)
                return false;
        }
        return true;
    }

    /// <summary>
    /// The parts of a convex polygon (corners in order) outside the box, each convex and in the same winding: the
    /// polygon is split by each of the box's six planes in turn, keeping the piece outside each plane and carrying on
    /// with the rest; what is left at the end is inside, and dropped. A polygon the box doesn't touch comes back as is.
    /// </summary>
    public List<List<Vector3>> Outside(List<Vector3> polygon)
    {
        // Quick out: a polygon whose bounds miss the box's
        var corners = Corners();
        var bmin = corners.Aggregate(new Vector3(float.MaxValue), Vector3.Min);
        var bmax = corners.Aggregate(new Vector3(float.MinValue), Vector3.Max);
        var pmin = polygon.Aggregate(new Vector3(float.MaxValue), Vector3.Min);
        var pmax = polygon.Aggregate(new Vector3(float.MinValue), Vector3.Max);
        if (pmax.X < bmin.X || pmin.X > bmax.X || pmax.Y < bmin.Y || pmin.Y > bmax.Y || pmax.Z < bmin.Z || pmin.Z > bmax.Z)
            return new List<List<Vector3>> { polygon };

        var pieces = new List<List<Vector3>>();
        var rest = polygon;

        foreach (var axis in new[] { AxisX, AxisY, AxisZ })
        {
            float length = axis.Length();
            if (length < 1e-6f)
                return new List<List<Vector3>> { polygon };
            var n = axis / length;

            // The two planes of this axis: inside is t >= 0 and t <= length
            foreach (var (normal, offset) in new[] { (-n, -Vector3.Dot(Origin, n)), (n, Vector3.Dot(Origin, n) + length) })
            {
                // Signed distance outside the plane: > 0 is outside
                var (outside, inside) = Split(rest, q => Vector3.Dot(q, normal) - offset);
                if (outside.Count >= 3)
                    pieces.Add(outside);
                rest = inside;
                if (rest.Count < 3)
                    return pieces;
            }
        }

        // What remains is inside the box
        return pieces;
    }

    /// <summary>Splits a convex polygon by a plane (Sutherland–Hodgman): the part where the distance is positive, and the rest.</summary>
    internal static (List<Vector3> Positive, List<Vector3> Remainder) Split(List<Vector3> polygon, Func<Vector3, float> distance)
    {
        var positive = new List<Vector3>();
        var rest = new List<Vector3>();
        const float epsilon = 1e-5f;

        for (int i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            float da = distance(a), db = distance(b);

            if (da > epsilon) positive.Add(a);
            else if (da < -epsilon) rest.Add(a);
            else { positive.Add(a); rest.Add(a); }

            if ((da > epsilon && db < -epsilon) || (da < -epsilon && db > epsilon))
            {
                var crossing = a + (b - a) * (da / (da - db));
                positive.Add(crossing);
                rest.Add(crossing);
            }
        }

        // A polygon entirely on one side keeps its corners on that side only
        if (positive.Count >= 3 && rest.Count < 3)
            return (polygon, new List<Vector3>());
        if (rest.Count >= 3 && positive.Count < 3)
            return (new List<Vector3>(), polygon);

        return (positive, rest);
    }
}
