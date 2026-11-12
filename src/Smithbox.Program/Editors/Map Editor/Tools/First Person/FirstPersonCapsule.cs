using System;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The player's capsule and its tests against the collision: an upright segment from the bottom sphere's centre to the
/// top sphere's, with a radius. Placed by the feet (the bottom of the capsule). Depends only on System.Numerics and
/// the collision set, so the survey tool links it as it is.
/// </summary>
public readonly struct FirstPersonCapsule
{
    public readonly float Radius;
    public readonly float Height;

    public FirstPersonCapsule(float radius, float height)
    {
        Radius = MathF.Max(0.01f, radius);
        Height = MathF.Max(Radius * 2, height);
    }

    public Vector3 Bottom(Vector3 feet) => feet + new Vector3(0, Radius, 0);

    public Vector3 Top(Vector3 feet) => feet + new Vector3(0, Height - Radius, 0);

    /// <summary>
    /// Whether the capsule at the feet overlaps the collision by more than the allowance, and the deepest
    /// penetration found (how far into a triangle the capsule reaches).
    /// </summary>
    public bool Overlaps(FirstPersonCollisionSet set, Vector3 feet, float allowance, out float deepest)
    {
        var bottom = Bottom(feet);
        var top = Top(feet);
        var radius = Radius;
        var reach = new Vector3(radius);
        var depth = 0f;

        set.Overlap(Vector3.Min(bottom, top) - reach, Vector3.Max(bottom, top) + reach, (mesh, t, a, b, c) =>
        {
            var distance = MathF.Sqrt(SegmentTriangle(bottom, top, a, b, c, out _, out _));
            depth = MathF.Max(depth, radius - distance);
        });

        deepest = depth;
        return depth > allowance;
    }

    /// <summary>
    /// Pushes the capsule out of the collision it overlaps: each round moves it out of the deepest triangle, along the
    /// line between the closest points (or the triangle's normal, upward, when the segment passes through it).
    /// Returns the feet, and whether it ended clear (within the allowance).
    /// </summary>
    public bool Depenetrate(FirstPersonCollisionSet set, ref Vector3 feet, float allowance, int rounds = 8)
    {
        for (var round = 0; round < rounds; round++)
        {
            var bottom = Bottom(feet);
            var top = Top(feet);
            var radius = Radius;
            var reach = new Vector3(radius);
            var depth = allowance;
            var push = Vector3.Zero;

            set.Overlap(Vector3.Min(bottom, top) - reach, Vector3.Max(bottom, top) + reach, (mesh, t, a, b, c) =>
            {
                var distance = MathF.Sqrt(SegmentTriangle(bottom, top, a, b, c, out var onSegment, out var onTriangle));
                var d = radius - distance;

                if (d <= depth)
                    return;

                Vector3 away;

                if (distance > 1e-5f)
                {
                    away = (onSegment - onTriangle) / distance;
                }
                else
                {
                    // Through the triangle: out along its normal, the side facing up
                    away = Vector3.Normalize(Vector3.Cross(b - a, c - a));

                    if (away.Y < 0)
                        away = -away;
                }

                depth = d;
                push = away * (d - allowance * 0.5f);
            });

            if (push == Vector3.Zero)
                return true;

            feet += push;
        }

        return !Overlaps(set, feet, allowance, out _);
    }

    /// <summary>
    /// The deepest overlap of the capsule at the feet, past the allowance: how deep, and the direction out of it (the
    /// line between the closest points, or the triangle's normal, upward, when the segment passes through it).
    /// False if the capsule is clear.
    /// </summary>
    public bool Deepest(FirstPersonCollisionSet set, Vector3 feet, float allowance, out float depth, out Vector3 normal)
        => Deepest(set, feet, allowance, out depth, out normal, out _, out _, -2f);

    /// <summary>
    /// As above, also giving the touched triangle's face normal (turned to face up). With groundSlopeCos above -1,
    /// only ground contacts count (<see cref="IsGround"/>), so the wall beside the feet isn't taken for the floor.
    /// The point is where on the triangle the contact is.
    /// </summary>
    public bool Deepest(FirstPersonCollisionSet set, Vector3 feet, float allowance, out float depth, out Vector3 normal,
        out Vector3 face, out Vector3 point, float groundSlopeCos)
        => Deepest(set, feet, allowance, out depth, out normal, out face, out point, out _, groundSlopeCos, false);

    /// <summary>
    /// As above, also giving the touched triangle's collision material. With slideNotGround, a triangle with the Slide
    /// property isn't ground (<see cref="FirstPersonCollisionReader.IsSlide"/>).
    /// </summary>
    public bool Deepest(FirstPersonCollisionSet set, Vector3 feet, float allowance, out float depth, out Vector3 normal,
        out Vector3 face, out Vector3 point, out uint material, float groundSlopeCos, bool slideNotGround)
    {
        var bestMaterial = FirstPersonTriangles.NoMaterial;
        var bottom = Bottom(feet);
        var top = Top(feet);
        var radius = Radius;
        var reach = new Vector3(radius);
        var best = allowance;
        var direction = Vector3.Zero;
        var bestFace = Vector3.UnitY;
        var bestPoint = feet;

        set.Overlap(Vector3.Min(bottom, top) - reach, Vector3.Max(bottom, top) + reach, (mesh, t, a, b, c) =>
        {
            var distance = MathF.Sqrt(SegmentTriangle(bottom, top, a, b, c, out var onSegment, out var onTriangle));
            var d = radius - distance;

            if (d <= best)
                return;

            var faceNormal = Vector3.Cross(b - a, c - a);
            var faceLength = faceNormal.Length();

            if (faceLength < 1e-12f)
                return;

            faceNormal /= faceLength;

            if (faceNormal.Y < 0)
                faceNormal = -faceNormal;

            var away = distance > 1e-5f ? (onSegment - onTriangle) / distance : faceNormal;
            var triangleMaterial = mesh.Tree.Material(t);

            if (groundSlopeCos > -1f && (!IsGround(away, faceNormal, groundSlopeCos)
                || slideNotGround && FirstPersonCollisionReader.IsSlide(triangleMaterial)))
                return;

            best = d;
            direction = away;
            bestFace = faceNormal;
            bestPoint = onTriangle;
            bestMaterial = triangleMaterial;
        });

        depth = best;
        normal = direction;
        face = bestFace;
        point = bestPoint;
        material = bestMaterial;
        return direction != Vector3.Zero;
    }

    /// <summary>
    /// Whether a contact is ground the player stands on: the triangle's face is walkable, and the contact is below the
    /// bottom sphere's centre (pointing up at it). The edge of a stair's tread meets the round bottom diagonally, so
    /// the face is what tells it from a slope; a ledge's top edge touching the capsule's side isn't below the centre,
    /// so it stays a wall.
    /// </summary>
    public static bool IsGround(Vector3 contact, Vector3 face, float slopeCos) => face.Y >= slopeCos && contact.Y >= 0.05f;

    #region Geometry

    /// <summary>The point of triangle abc closest to p (Ericson, Real-Time Collision Detection 5.1.5).</summary>
    public static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = b - a;
        var ac = c - a;
        var ap = p - a;
        var d1 = Vector3.Dot(ab, ap);
        var d2 = Vector3.Dot(ac, ap);

        if (d1 <= 0 && d2 <= 0)
            return a;

        var bp = p - b;
        var d3 = Vector3.Dot(ab, bp);
        var d4 = Vector3.Dot(ac, bp);

        if (d3 >= 0 && d4 <= d3)
            return b;

        var vc = d1 * d4 - d3 * d2;

        if (vc <= 0 && d1 >= 0 && d3 <= 0)
            return a + ab * (d1 / (d1 - d3));

        var cp = p - c;
        var d5 = Vector3.Dot(ab, cp);
        var d6 = Vector3.Dot(ac, cp);

        if (d6 >= 0 && d5 <= d6)
            return c;

        var vb = d5 * d2 - d1 * d6;

        if (vb <= 0 && d2 >= 0 && d6 <= 0)
            return a + ac * (d2 / (d2 - d6));

        var va = d3 * d6 - d5 * d4;

        if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0)
            return b + (c - b) * ((d4 - d3) / (d4 - d3 + (d5 - d6)));

        var denom = 1f / (va + vb + vc);
        return a + ab * (vb * denom) + ac * (vc * denom);
    }

    /// <summary>The closest points of segments p0p1 and q0q1, and their squared distance (Ericson 5.1.9).</summary>
    public static float SegmentSegment(Vector3 p0, Vector3 p1, Vector3 q0, Vector3 q1, out Vector3 onP, out Vector3 onQ)
    {
        var d1 = p1 - p0;
        var d2 = q1 - q0;
        var r = p0 - q0;
        var a = Vector3.Dot(d1, d1);
        var e = Vector3.Dot(d2, d2);
        var f = Vector3.Dot(d2, r);
        float s, t;

        if (a <= 1e-12f && e <= 1e-12f)
        {
            onP = p0;
            onQ = q0;
            return Vector3.DistanceSquared(p0, q0);
        }

        if (a <= 1e-12f)
        {
            s = 0;
            t = Math.Clamp(f / e, 0, 1);
        }
        else
        {
            var c = Vector3.Dot(d1, r);

            if (e <= 1e-12f)
            {
                t = 0;
                s = Math.Clamp(-c / a, 0, 1);
            }
            else
            {
                var b = Vector3.Dot(d1, d2);
                var denom = a * e - b * b;

                s = denom > 1e-12f ? Math.Clamp((b * f - c * e) / denom, 0, 1) : 0;
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

        onP = p0 + d1 * s;
        onQ = q0 + d2 * t;
        return Vector3.DistanceSquared(onP, onQ);
    }

    /// <summary>
    /// The closest points of segment p0p1 and triangle abc, and their squared distance. Zero where the segment passes
    /// through the triangle; otherwise the nearest of the ends to the triangle and the segment to its edges.
    /// </summary>
    public static float SegmentTriangle(Vector3 p0, Vector3 p1, Vector3 a, Vector3 b, Vector3 c, out Vector3 onSegment, out Vector3 onTriangle)
    {
        // Through the face
        var direction = p1 - p0;

        if (FirstPersonTriangleTree.RayTriangle(p0, direction, a, b, c, out var t) && t <= 1)
        {
            onSegment = onTriangle = p0 + direction * t;
            return 0;
        }

        var best = float.MaxValue;
        onSegment = p0;
        onTriangle = a;

        void Consider(float distanceSquared, Vector3 s, Vector3 q, ref float bestSoFar, ref Vector3 bs, ref Vector3 bq)
        {
            if (distanceSquared < bestSoFar)
            {
                bestSoFar = distanceSquared;
                bs = s;
                bq = q;
            }
        }

        var q0 = ClosestPointOnTriangle(p0, a, b, c);
        Consider(Vector3.DistanceSquared(p0, q0), p0, q0, ref best, ref onSegment, ref onTriangle);

        var q1 = ClosestPointOnTriangle(p1, a, b, c);
        Consider(Vector3.DistanceSquared(p1, q1), p1, q1, ref best, ref onSegment, ref onTriangle);

        Consider(SegmentSegment(p0, p1, a, b, out var s0, out var e0), s0, e0, ref best, ref onSegment, ref onTriangle);
        Consider(SegmentSegment(p0, p1, b, c, out var s1, out var e1), s1, e1, ref best, ref onSegment, ref onTriangle);
        Consider(SegmentSegment(p0, p1, c, a, out var s2, out var e2), s2, e2, ref best, ref onSegment, ref onTriangle);

        return best;
    }

    #endregion
}
