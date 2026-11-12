using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A bounding volume hierarchy over a fixed triangle list, for the first person mode's ray and overlap queries. Built
/// once by a binned surface area split; the triangles are reordered so each leaf's are contiguous. Depends only on
/// System.Numerics, so the survey tool links it as it is.
/// </summary>
public sealed class FirstPersonTriangleTree
{
    /// <summary>Most triangles in a leaf.</summary>
    public const int LeafSize = 6;

    private const int Bins = 12;

    /// <summary>The triangles, three corners each, in leaf order.</summary>
    public readonly Vector3[] Corners;

    // Node i: bounds, and either two children (Count == 0: Left, Left + 1) or a run of triangles (Start, Count)
    private readonly Vector3[] NodeMin;
    private readonly Vector3[] NodeMax;
    private readonly int[] NodeStart;
    private readonly int[] NodeCount;

    public int TriangleCount => Corners.Length / 3;

    public int NodeCountUsed { get; }

    public Vector3 Min => NodeCountUsed > 0 ? NodeMin[0] : Vector3.Zero;

    public Vector3 Max => NodeCountUsed > 0 ? NodeMax[0] : Vector3.Zero;

    /// <summary>Bytes held by the tree, for the build status.</summary>
    public long MemoryBytes => Corners.Length * 12L + NodeCountUsed * (24L + 8L);

    /// <summary>Builds the tree over a triangle list (three corners a triangle). Degenerate triangles are dropped.</summary>
    public FirstPersonTriangleTree(IReadOnlyList<Vector3> triangles)
    {
        var count = triangles.Count / 3;
        var keep = new List<int>(count);

        for (var t = 0; t < count; t++)
        {
            var a = triangles[t * 3];
            var b = triangles[t * 3 + 1];
            var c = triangles[t * 3 + 2];

            if (!IsFinite(a) || !IsFinite(b) || !IsFinite(c))
                continue;

            if (Vector3.Cross(b - a, c - a).LengthSquared() < 1e-12f)
                continue;

            keep.Add(t);
        }

        count = keep.Count;

        var centroids = new Vector3[count];
        var triMin = new Vector3[count];
        var triMax = new Vector3[count];
        var order = new int[count];

        for (var i = 0; i < count; i++)
        {
            var t = keep[i];
            var a = triangles[t * 3];
            var b = triangles[t * 3 + 1];
            var c = triangles[t * 3 + 2];

            triMin[i] = Vector3.Min(a, Vector3.Min(b, c));
            triMax[i] = Vector3.Max(a, Vector3.Max(b, c));
            centroids[i] = (a + b + c) / 3f;
            order[i] = i;
        }

        // A binary tree with leaves of at least one triangle has fewer than 2n nodes
        var capacity = Math.Max(1, count * 2);
        NodeMin = new Vector3[capacity];
        NodeMax = new Vector3[capacity];
        NodeStart = new int[capacity];
        NodeCount = new int[capacity];

        var used = 0;

        if (count > 0)
        {
            used = 1;
            var stack = new Stack<(int Node, int Start, int Count)>();
            stack.Push((0, 0, count));

            while (stack.Count > 0)
            {
                var (node, start, n) = stack.Pop();

                var min = new Vector3(float.MaxValue);
                var max = new Vector3(float.MinValue);
                var cmin = new Vector3(float.MaxValue);
                var cmax = new Vector3(float.MinValue);

                for (var i = start; i < start + n; i++)
                {
                    var o = order[i];
                    min = Vector3.Min(min, triMin[o]);
                    max = Vector3.Max(max, triMax[o]);
                    cmin = Vector3.Min(cmin, centroids[o]);
                    cmax = Vector3.Max(cmax, centroids[o]);
                }

                NodeMin[node] = min;
                NodeMax[node] = max;

                var split = n > LeafSize ? FindSplit(order, start, n, centroids, triMin, triMax, cmin, cmax) : -1;

                if (split <= start || split >= start + n)
                {
                    NodeStart[node] = start;
                    NodeCount[node] = n;
                    continue;
                }

                var left = used;
                used += 2;

                NodeStart[node] = left;
                NodeCount[node] = 0;

                stack.Push((left + 1, split, start + n - split));
                stack.Push((left, start, split - start));
            }
        }

        NodeCountUsed = used;

        Corners = new Vector3[count * 3];

        for (var i = 0; i < count; i++)
        {
            var t = keep[order[i]];
            Corners[i * 3] = triangles[t * 3];
            Corners[i * 3 + 1] = triangles[t * 3 + 1];
            Corners[i * 3 + 2] = triangles[t * 3 + 2];
        }
    }

    /// <summary>
    /// Partitions order[start..start+n] by the cheapest binned surface area split along the longest centroid axis.
    /// Returns the first index of the right half, or -1 to make a leaf. Falls back to a median split when the
    /// centroids are all in one bin.
    /// </summary>
    private static int FindSplit(int[] order, int start, int n, Vector3[] centroids, Vector3[] triMin, Vector3[] triMax,
        Vector3 cmin, Vector3 cmax)
    {
        var extent = cmax - cmin;
        var axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;
        var lo = Get(cmin, axis);
        var size = Get(extent, axis);

        if (size < 1e-6f)
        {
            // All centroids in one place: halve the run, as no split separates them
            return start + n / 2;
        }

        var binCount = new int[Bins];
        var binMin = new Vector3[Bins];
        var binMax = new Vector3[Bins];

        for (var b = 0; b < Bins; b++)
        {
            binMin[b] = new Vector3(float.MaxValue);
            binMax[b] = new Vector3(float.MinValue);
        }

        int BinOf(int o) => Math.Min(Bins - 1, (int)((Get(centroids[o], axis) - lo) / size * Bins));

        for (var i = start; i < start + n; i++)
        {
            var o = order[i];
            var b = BinOf(o);
            binCount[b]++;
            binMin[b] = Vector3.Min(binMin[b], triMin[o]);
            binMax[b] = Vector3.Max(binMax[b], triMax[o]);
        }

        // Sweep: cost of splitting after bin k = area(left) * count(left) + area(right) * count(right)
        var rightArea = new float[Bins];
        var rightCount = new int[Bins];
        var rmin = new Vector3(float.MaxValue);
        var rmax = new Vector3(float.MinValue);
        var rc = 0;

        for (var b = Bins - 1; b > 0; b--)
        {
            rc += binCount[b];

            if (binCount[b] > 0)
            {
                rmin = Vector3.Min(rmin, binMin[b]);
                rmax = Vector3.Max(rmax, binMax[b]);
            }

            rightCount[b] = rc;
            rightArea[b] = rc > 0 ? Area(rmin, rmax) : 0;
        }

        var bestCost = float.MaxValue;
        var bestBin = -1;
        var lmin = new Vector3(float.MaxValue);
        var lmax = new Vector3(float.MinValue);
        var lc = 0;

        for (var b = 0; b < Bins - 1; b++)
        {
            lc += binCount[b];

            if (binCount[b] > 0)
            {
                lmin = Vector3.Min(lmin, binMin[b]);
                lmax = Vector3.Max(lmax, binMax[b]);
            }

            if (lc == 0 || rightCount[b + 1] == 0)
                continue;

            var cost = Area(lmin, lmax) * lc + rightArea[b + 1] * rightCount[b + 1];

            if (cost < bestCost)
            {
                bestCost = cost;
                bestBin = b;
            }
        }

        if (bestBin < 0)
            return start + n / 2;

        // Partition in place: bins <= bestBin to the left
        var i0 = start;
        var i1 = start + n - 1;

        while (i0 <= i1)
        {
            if (BinOf(order[i0]) <= bestBin)
            {
                i0++;
            }
            else
            {
                (order[i0], order[i1]) = (order[i1], order[i0]);
                i1--;
            }
        }

        return i0;
    }

    private static float Get(Vector3 v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    private static float Area(Vector3 min, Vector3 max)
    {
        var d = Vector3.Max(max - min, Vector3.Zero);
        return d.X * d.Y + d.Y * d.Z + d.Z * d.X;
    }

    private static bool IsFinite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);

    #region Queries

    /// <summary>
    /// The nearest triangle the ray hits within maxDistance, both faces counting. The direction needn't be normalised:
    /// distance is in units of its length.
    /// </summary>
    public bool RayCast(Vector3 origin, Vector3 direction, float maxDistance, out float distance, out int triangle)
    {
        distance = maxDistance;
        triangle = -1;

        if (NodeCountUsed == 0)
            return false;

        var inverse = new Vector3(1f / direction.X, 1f / direction.Y, 1f / direction.Z);
        Span<int> stack = stackalloc int[64];
        var top = 0;
        stack[top++] = 0;

        while (top > 0)
        {
            var node = stack[--top];

            if (!RayBox(origin, inverse, NodeMin[node], NodeMax[node], distance, out _))
                continue;

            if (NodeCount[node] > 0)
            {
                var end = NodeStart[node] + NodeCount[node];

                for (var t = NodeStart[node]; t < end; t++)
                {
                    if (RayTriangle(origin, direction, Corners[t * 3], Corners[t * 3 + 1], Corners[t * 3 + 2], out var hit)
                        && hit < distance)
                    {
                        distance = hit;
                        triangle = t;
                    }
                }

                continue;
            }

            var left = NodeStart[node];

            // Nearer child last, so it is visited first; depth is bounded by the split, but guard the stack anyway
            if (top + 2 > stack.Length)
                continue;

            var hitLeft = RayBox(origin, inverse, NodeMin[left], NodeMax[left], distance, out var tl);
            var hitRight = RayBox(origin, inverse, NodeMin[left + 1], NodeMax[left + 1], distance, out var tr);

            if (hitLeft && hitRight)
            {
                if (tl <= tr)
                {
                    stack[top++] = left + 1;
                    stack[top++] = left;
                }
                else
                {
                    stack[top++] = left;
                    stack[top++] = left + 1;
                }
            }
            else if (hitLeft)
            {
                stack[top++] = left;
            }
            else if (hitRight)
            {
                stack[top++] = left + 1;
            }
        }

        return triangle >= 0;
    }

    /// <summary>Adds the triangles whose bounds overlap the box to the list.</summary>
    public void Overlap(Vector3 min, Vector3 max, List<int> result)
    {
        if (NodeCountUsed == 0)
            return;

        Span<int> stack = stackalloc int[64];
        var top = 0;
        stack[top++] = 0;

        while (top > 0)
        {
            var node = stack[--top];

            if (!BoxBox(min, max, NodeMin[node], NodeMax[node]))
                continue;

            if (NodeCount[node] > 0)
            {
                var end = NodeStart[node] + NodeCount[node];

                for (var t = NodeStart[node]; t < end; t++)
                {
                    var a = Corners[t * 3];
                    var b = Corners[t * 3 + 1];
                    var c = Corners[t * 3 + 2];

                    if (BoxBox(min, max, Vector3.Min(a, Vector3.Min(b, c)), Vector3.Max(a, Vector3.Max(b, c))))
                        result.Add(t);
                }

                continue;
            }

            if (top + 2 > stack.Length)
                continue;

            stack[top++] = NodeStart[node];
            stack[top++] = NodeStart[node] + 1;
        }
    }

    /// <summary>The unit normal of a triangle, by its winding (a, b, c).</summary>
    public Vector3 Normal(int triangle)
    {
        var a = Corners[triangle * 3];
        var n = Vector3.Cross(Corners[triangle * 3 + 1] - a, Corners[triangle * 3 + 2] - a);
        var length = n.Length();
        return length > 0 ? n / length : Vector3.UnitY;
    }

    /// <summary>The deepest leaf, for the survey's checks.</summary>
    public int Depth()
    {
        if (NodeCountUsed == 0)
            return 0;

        var deepest = 0;
        var stack = new Stack<(int Node, int Depth)>();
        stack.Push((0, 1));

        while (stack.Count > 0)
        {
            var (node, depth) = stack.Pop();
            deepest = Math.Max(deepest, depth);

            if (NodeCount[node] == 0)
            {
                stack.Push((NodeStart[node], depth + 1));
                stack.Push((NodeStart[node] + 1, depth + 1));
            }
        }

        return deepest;
    }

    public static bool BoxBox(Vector3 amin, Vector3 amax, Vector3 bmin, Vector3 bmax)
    {
        return amin.X <= bmax.X && amax.X >= bmin.X
            && amin.Y <= bmax.Y && amax.Y >= bmin.Y
            && amin.Z <= bmax.Z && amax.Z >= bmin.Z;
    }

    /// <summary>Slab test; near is where the ray enters the box (0 if it starts inside).</summary>
    public static bool RayBox(Vector3 origin, Vector3 inverse, Vector3 min, Vector3 max, float maxDistance, out float near)
    {
        var t0 = (min - origin) * inverse;
        var t1 = (max - origin) * inverse;
        var tmin = Vector3.Min(t0, t1);
        var tmax = Vector3.Max(t0, t1);

        // NaN (0 * inf, a ray along a face of the box) is treated as a hit on that axis
        var enter = MathF.Max(MathF.Max(Nan(tmin.X, float.MinValue), Nan(tmin.Y, float.MinValue)), MathF.Max(Nan(tmin.Z, float.MinValue), 0f));
        var exit = MathF.Min(MathF.Min(Nan(tmax.X, float.MaxValue), Nan(tmax.Y, float.MaxValue)), MathF.Min(Nan(tmax.Z, float.MaxValue), maxDistance));

        near = enter;
        return enter <= exit;
    }

    private static float Nan(float v, float fallback) => float.IsNaN(v) ? fallback : v;

    /// <summary>Möller–Trumbore, both faces.</summary>
    public static bool RayTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c, out float distance)
    {
        distance = 0;

        var e1 = b - a;
        var e2 = c - a;
        var p = Vector3.Cross(direction, e2);
        var det = Vector3.Dot(e1, p);

        if (MathF.Abs(det) < 1e-12f)
            return false;

        var inv = 1f / det;
        var s = origin - a;
        var u = Vector3.Dot(s, p) * inv;

        if (u < 0 || u > 1)
            return false;

        var q = Vector3.Cross(s, e1);
        var v = Vector3.Dot(direction, q) * inv;

        if (v < 0 || u + v > 1)
            return false;

        distance = Vector3.Dot(e2, q) * inv;
        return distance >= 0;
    }

    #endregion
}
