using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Cuts a box out of a triangle mesh for the Subtract tab of the Collision Editor. Triangles wholly inside the box
/// are removed, triangles wholly outside are kept as they are, and triangles that cross it are cut along its faces:
/// only the part outside the box is kept, triangulated again, with the material and shape key of the triangle it came
/// from.
///
/// The cut is worked out in the box's unit space (-0.5 to 0.5 on each axis), which the caller gives as a matrix from
/// the space of the mesh. That map is affine, so a point a fraction along an edge in one space is the same fraction
/// along it in the other, and new points are placed in the mesh's own space by that fraction.
///
/// So that the mesh stays welded along the cut, a point where an original edge crosses a face of the box is made once
/// and shared by both triangles of that edge (keyed by the edge's two vertices and the face); one within a millimetre of
/// an end of its edge is that end. A triangle that only touches the box is kept, but split at any points its cut
/// neighbours left on their shared edges, so there are no T-junctions along the cut. The part inside the box
/// is found by clipping the triangle to each face in turn (Sutherland-Hodgman); the part kept is the triangle with
/// that convex piece taken out, traced as loops of the triangle's outline and the piece's inner edges, then
/// ear-clipped. Where the box is inside a large triangle without reaching its edges, the triangle is first split in
/// three about the middle of the piece, so each part is notched rather than holed.
/// </summary>
public static class CollisionBoxSubtractor
{
    /// <summary>
    /// The mesh with the box cut out: its vertices (unused ones dropped), triangles, and each triangle's material and
    /// key. Removed counts the original triangles taken out whole, Cut those that were cut and partly kept.
    /// </summary>
    public sealed record Result(List<Vector3> Vertices, List<int> Indices, List<uint> Materials, List<uint> Keys,
        int Removed, int Cut)
    {
        public bool Changed => Removed > 0 || Cut > 0;
    }

    /// <summary>How many triangles the box would remove or cut, without cutting them.</summary>
    public static (int Removed, int Cut) Count(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, Matrix4x4 toBox)
    {
        var result = Subtract(vertices, indices, null, null, toBox, countOnly: true);
        return (result.Removed, result.Cut);
    }

    /// <summary>
    /// Cuts the box out of the mesh. toBox takes a point of the mesh into the box's unit space.
    /// </summary>
    public static Result Subtract(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices,
        IReadOnlyList<uint> materials, IReadOnlyList<uint> keys, Matrix4x4 toBox)
    {
        return Subtract(vertices, indices, materials, keys, toBox, countOnly: false);
    }

    private static Result Subtract(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices,
        IReadOnlyList<uint> materials, IReadOnlyList<uint> keys, Matrix4x4 toBox, bool countOnly)
    {
        var cutter = new Cutter(vertices, toBox, countOnly);
        var removed = 0;
        var cut = 0;

        for (var t = 0; t < indices.Count / 3; t++)
        {
            var a = indices[t * 3];
            var b = indices[t * 3 + 1];
            var c = indices[t * 3 + 2];

            var material = materials?[t] ?? 0;
            var key = keys?[t] ?? 0;

            switch (cutter.Process(a, b, c, material, key))
            {
                case Outcome.Removed: removed++; break;
                case Outcome.Cut: cut++; break;
            }
        }

        if (countOnly)
            return new Result(null, null, null, null, removed, cut);

        cutter.KeepDeferred();

        return cutter.Compact(removed, cut);
    }

    private enum Outcome
    {
        Kept,
        Removed,
        Cut
    }

    /// <summary>A corner of a clipped polygon: its vertex, and the edges of the triangle it lies on (bits 0 to 2).</summary>
    private readonly record struct ClipVertex(int Id, int Edges);

    private sealed class Cutter
    {
        // Positions in the mesh's space, and the same points in the box's unit space; new points are added to both
        private readonly List<Vector3> Positions;
        private readonly List<Vector3> BoxPositions;

        private readonly bool CountOnly;

        // The point where an edge (its lower and higher vertex) crosses a face, made once for both its triangles
        private readonly Dictionary<(int Low, int High, int Face), int> EdgePoints = new();

        // The new points made on each edge, whichever face they are on
        private readonly Dictionary<(int Low, int High), List<int>> PointsOnEdge = new();

        // Triangles that reach the box without any of their area being inside it: kept, once every triangle has been
        // cut, with the points their cut neighbours left on their edges, so the mesh has no cracks there
        private readonly List<(int A, int B, int C, uint Material, uint Key)> Deferred = new();

        private readonly List<int> OutIndices = new();
        private readonly List<uint> OutMaterials = new();
        private readonly List<uint> OutKeys = new();

        // Crossings closer than this to an end of their edge, as a fraction of it, are taken as the end
        private const float ParameterEpsilon = 1e-6f;

        // Crossings closer than this (in the mesh's units, metres) to an end of their edge are taken as the end, so a
        // face passing just by a vertex doesn't leave a sliver
        private const float SnapDistance = 1e-3f;

        // Areas (square metres) at or below this have nothing to collide with
        private const float AreaEpsilon = 1e-8f;

        public Cutter(IReadOnlyList<Vector3> vertices, Matrix4x4 toBox, bool countOnly)
        {
            CountOnly = countOnly;
            Positions = new List<Vector3>(vertices);
            BoxPositions = new List<Vector3>(vertices.Count);

            foreach (var v in vertices)
                BoxPositions.Add(Vector3.Transform(v, toBox));
        }

        #region Triangle

        public Outcome Process(int a, int b, int c, uint material, uint key)
        {
            var pa = BoxPositions[a];
            var pb = BoxPositions[b];
            var pc = BoxPositions[c];

            // Apart from the box on an axis: kept as it is
            var min = Vector3.Min(pa, Vector3.Min(pb, pc));
            var max = Vector3.Max(pa, Vector3.Max(pb, pc));

            if (min.X > 0.5f || min.Y > 0.5f || min.Z > 0.5f || max.X < -0.5f || max.Y < -0.5f || max.Z < -0.5f)
            {
                Emit(a, b, c, material, key);
                return Outcome.Kept;
            }

            // Every corner inside: removed whole
            if (IsInside(pa) && IsInside(pb) && IsInside(pc))
                return Outcome.Removed;

            if (CountOnly)
            {
                // The box's corners reach in from outside the triangle's corners: clipped to see if any of it is inside
                var inside = ClipToBox(a, b, c);
                return inside == null ? Outcome.Kept : IsWholeTriangle(inside, a, b, c) ? Outcome.Removed : Outcome.Cut;
            }

            return CutTriangle(a, b, c, material, key, allowSplit: true);
        }

        private static bool IsInside(Vector3 p)
        {
            return MathF.Abs(p.X) <= 0.5f && MathF.Abs(p.Y) <= 0.5f && MathF.Abs(p.Z) <= 0.5f;
        }

        private Outcome CutTriangle(int a, int b, int c, uint material, uint key, bool allowSplit)
        {
            var inside = ClipToBox(a, b, c);

            if (inside == null)
            {
                // Only touches the box: kept, but a neighbour cut along a shared edge may put points on it
                Deferred.Add((a, b, c, material, key));
                return Outcome.Kept;
            }

            if (IsWholeTriangle(inside, a, b, c))
                return Outcome.Removed;

            // The piece inside doesn't reach the triangle's outline: split about its middle, so each part is notched
            if (!inside.Exists(v => v.Edges != 0))
            {
                if (!allowSplit)
                {
                    // Shouldn't happen, as the middle is inside the box; kept whole rather than left with a hole
                    Deferred.Add((a, b, c, material, key));
                    return Outcome.Kept;
                }

                var middle = Vector3.Zero;
                var boxMiddle = Vector3.Zero;

                foreach (var v in inside)
                {
                    middle += Positions[v.Id];
                    boxMiddle += BoxPositions[v.Id];
                }

                var g = AddVertex(middle / inside.Count, boxMiddle / inside.Count);

                CutTriangle(a, b, g, material, key, allowSplit: false);
                CutTriangle(b, c, g, material, key, allowSplit: false);
                CutTriangle(c, a, g, material, key, allowSplit: false);

                return Outcome.Cut;
            }

            foreach (var loop in TraceOutsideLoops(a, b, c, inside))
                Triangulate(loop, a, b, c, material, key);

            return Outcome.Cut;
        }

        /// <summary>Whether the piece inside the box is all of the triangle.</summary>
        private bool IsWholeTriangle(List<ClipVertex> inside, int a, int b, int c)
        {
            var whole = Area(Positions[a], Positions[b], Positions[c]);
            var piece = PolygonArea(inside);

            return piece >= whole * (1f - 1e-5f);
        }

        #endregion

        #region Clip

        /// <summary>
        /// The part of the triangle inside the box, as a convex polygon wound as the triangle is, or null if no part of
        /// it with any area is. Each corner says which of the triangle's edges it lies on.
        /// </summary>
        private List<ClipVertex> ClipToBox(int a, int b, int c)
        {
            int[] corners = { a, b, c };

            // Corner 0 is on edges 2 (c-a) and 0 (a-b), corner 1 on 0 and 1, corner 2 on 1 and 2
            var polygon = new List<ClipVertex>
            {
                new(a, 0b101),
                new(b, 0b011),
                new(c, 0b110)
            };

            for (var face = 0; face < 6 && polygon.Count >= 3; face++)
            {
                var clipped = new List<ClipVertex>(polygon.Count + 2);

                for (var i = 0; i < polygon.Count; i++)
                {
                    var current = polygon[i];
                    var next = polygon[(i + 1) % polygon.Count];

                    var dc = FaceDistance(BoxPositions[current.Id], face);
                    var dn = FaceDistance(BoxPositions[next.Id], face);

                    var currentIn = dc <= 0;
                    var nextIn = dn <= 0;

                    if (currentIn)
                        AddDistinct(clipped, current);

                    if (currentIn != nextIn)
                        AddDistinct(clipped, Crossing(corners, current, next, dc, dn, face));
                }

                // The first and last may be the same point
                while (clipped.Count > 1 && clipped[0].Id == clipped[^1].Id)
                    clipped.RemoveAt(clipped.Count - 1);

                polygon = clipped;
            }

            if (polygon.Count < 3 || PolygonArea(polygon) <= AreaEpsilon)
                return null;

            return polygon;
        }

        private static void AddDistinct(List<ClipVertex> polygon, ClipVertex vertex)
        {
            if (polygon.Count == 0 || polygon[^1].Id != vertex.Id)
                polygon.Add(vertex);
        }

        /// <summary>How far a point (in the box's unit space) is outside a face: x+, x-, y+, y-, z+, z-.</summary>
        private static float FaceDistance(Vector3 p, int face)
        {
            var value = (face / 2) switch
            {
                0 => p.X,
                1 => p.Y,
                _ => p.Z
            };

            return (face % 2 == 0 ? value : -value) - 0.5f;
        }

        /// <summary>
        /// Where a side of the polygon crosses a face. On an edge of the triangle, the point is the edge's own crossing,
        /// shared with the other triangle of the edge; otherwise it is a new point inside the triangle.
        /// </summary>
        private ClipVertex Crossing(int[] corners, ClipVertex current, ClipVertex next, float dc, float dn, int face)
        {
            var shared = current.Edges & next.Edges;

            if (shared != 0)
            {
                var edge = shared switch
                {
                    0b001 => 0,
                    0b010 => 1,
                    _ => 2
                };

                var id = EdgePoint(corners[edge], corners[(edge + 1) % 3], face);

                // A crossing at a corner is the corner itself
                for (var k = 0; k < 3; k++)
                {
                    if (corners[k] == id)
                        return new ClipVertex(id, k switch { 0 => 0b101, 1 => 0b011, _ => 0b110 });
                }

                return new ClipVertex(id, 1 << edge);
            }

            // Not snapped by distance: a point on the triangle's outline is shared with a neighbour, which doesn't make
            // this crossing, so snapping to it would put the piece's corner where the neighbour's isn't
            var t = dc / (dc - dn);

            if (t <= ParameterEpsilon)
                return current;

            if (t >= 1f - ParameterEpsilon)
                return next;

            var position = Vector3.Lerp(Positions[current.Id], Positions[next.Id], t);
            var boxPosition = Vector3.Lerp(BoxPositions[current.Id], BoxPositions[next.Id], t);

            return new ClipVertex(AddVertex(position, boxPosition), 0);
        }

        /// <summary>
        /// The point where an edge of the mesh crosses a face, worked out from the edge's two vertices in the same
        /// order whichever triangle asks, so both triangles of the edge get the same vertex.
        /// </summary>
        private int EdgePoint(int u, int w, int face)
        {
            var low = Math.Min(u, w);
            var high = Math.Max(u, w);

            if (EdgePoints.TryGetValue((low, high, face), out var existing))
                return existing;

            var dl = FaceDistance(BoxPositions[low], face);
            var dh = FaceDistance(BoxPositions[high], face);
            var denominator = dl - dh;

            var t = MathF.Abs(denominator) < 1e-12f ? 0.5f : Math.Clamp(dl / denominator, 0f, 1f);
            var length = Vector3.Distance(Positions[low], Positions[high]);

            int id;

            // A crossing next to an end is the end itself: both triangles of the edge snap the same way, as they share
            // this point, so no sliver or crack is left there
            if (t <= ParameterEpsilon || t * length <= SnapDistance)
                id = low;
            else if (t >= 1f - ParameterEpsilon || (1f - t) * length <= SnapDistance)
                id = high;
            else
            {
                id = AddVertex(Vector3.Lerp(Positions[low], Positions[high], t), Vector3.Lerp(BoxPositions[low], BoxPositions[high], t));

                if (!PointsOnEdge.TryGetValue((low, high), out var points))
                    PointsOnEdge[(low, high)] = points = new List<int>();

                points.Add(id);
            }

            EdgePoints[(low, high, face)] = id;
            return id;
        }

        /// <summary>
        /// Keeps the triangles that only touched the box. A cut neighbour may have left points on a shared edge (where
        /// its piece inside the box reaches the edge): the triangle is then split at them, so the edge stays welded.
        /// </summary>
        public void KeepDeferred()
        {
            if (Deferred.Count == 0)
                return;

            var used = new HashSet<int>(OutIndices);

            foreach (var (a, b, c, material, key) in Deferred)
            {

                int[] corners = { a, b, c };
                var loop = new List<int>();

                for (var edge = 0; edge < 3; edge++)
                {
                    var start = corners[edge];
                    var end = corners[(edge + 1) % 3];

                    loop.Add(start);

                    if (!PointsOnEdge.TryGetValue((Math.Min(start, end), Math.Max(start, end)), out var points))
                        continue;

                    var direction = Positions[end] - Positions[start];

                    foreach (var id in points.Where(used.Contains)
                        .OrderBy(id => Vector3.Dot(Positions[id] - Positions[start], direction)))
                    {
                        loop.Add(id);
                    }
                }

                if (loop.Count == 3)
                    Emit(a, b, c, material, key);
                else
                    Triangulate(loop, a, b, c, material, key);
            }

            Deferred.Clear();
        }

        private int AddVertex(Vector3 position, Vector3 boxPosition)
        {
            Positions.Add(position);
            BoxPositions.Add(boxPosition);
            return Positions.Count - 1;
        }

        #endregion

        #region Loops

        /// <summary>
        /// The outlines of what is left of the triangle once the piece inside the box is taken out, wound as the
        /// triangle is: the triangle's outline where it isn't the piece's, and the piece's inner sides run backwards.
        /// </summary>
        private List<List<int>> TraceOutsideLoops(int a, int b, int c, List<ClipVertex> inside)
        {
            int[] corners = { a, b, c };

            // The triangle's outline, with the piece's points on each edge in order along it
            var outline = new List<int>();

            for (var edge = 0; edge < 3; edge++)
            {
                var start = corners[edge];
                var end = corners[(edge + 1) % 3];
                var direction = Positions[end] - Positions[start];

                outline.Add(start);

                var onEdge = new List<(int Id, float Along)>();

                foreach (var v in inside)
                {
                    if (v.Edges == 1 << edge && v.Id != start && v.Id != end)
                        onEdge.Add((v.Id, Vector3.Dot(Positions[v.Id] - Positions[start], direction)));
                }

                onEdge.Sort((x, y) => x.Along.CompareTo(y.Along));

                foreach (var (id, _) in onEdge)
                {
                    if (outline[^1] != id)
                        outline.Add(id);
                }
            }

            var pieceSides = new HashSet<(int, int)>();

            for (var i = 0; i < inside.Count; i++)
                pieceSides.Add((inside[i].Id, inside[(i + 1) % inside.Count].Id));

            var outlineSides = new HashSet<(int, int)>();
            var outlineNext = new Dictionary<int, int>();

            for (var i = 0; i < outline.Count; i++)
            {
                var side = (outline[i], outline[(i + 1) % outline.Count]);
                outlineSides.Add(side);

                // A side of the outline inside the box is taken out
                if (!pieceSides.Contains(side))
                    outlineNext[side.Item1] = side.Item2;
            }

            // The piece's sides across the triangle, run backwards so what is left is on their left as well
            var innerNext = new Dictionary<int, int>();

            foreach (var side in pieceSides)
            {
                if (!outlineSides.Contains(side))
                    innerNext[side.Item2] = side.Item1;
            }

            var loops = new List<List<int>>();
            var guard = outline.Count + inside.Count + 4;

            while (outlineNext.Count > 0)
            {
                var loop = new List<int>();
                var current = -1;

                foreach (var start in outlineNext.Keys)
                {
                    current = start;
                    break;
                }

                var cameAlongOutline = false;

                for (var step = 0; step < guard * 2; step++)
                {
                    loop.Add(current);

                    // Where the piece touches the outline at one point, the loop turns in along the piece rather than
                    // carrying on, so the piece isn't left inside what is kept
                    int next;

                    if (cameAlongOutline && innerNext.TryGetValue(current, out next))
                    {
                        innerNext.Remove(current);
                        cameAlongOutline = false;
                    }
                    else if (outlineNext.TryGetValue(current, out next))
                    {
                        outlineNext.Remove(current);
                        cameAlongOutline = true;
                    }
                    else if (innerNext.TryGetValue(current, out next))
                    {
                        innerNext.Remove(current);
                        cameAlongOutline = false;
                    }
                    else
                    {
                        break;
                    }

                    current = next;
                }

                // Closed on itself: the last point added is the first
                if (loop.Count > 1 && loop[^1] == loop[0])
                    loop.RemoveAt(loop.Count - 1);

                if (loop.Count >= 3)
                    loops.Add(loop);
            }

            return loops;
        }

        #endregion

        #region Triangulate

        /// <summary>
        /// Ear-clips a loop in the plane of the triangle (a, b, c), which it is wound the same way as.
        /// </summary>
        private void Triangulate(List<int> loop, int a, int b, int c, uint material, uint key)
        {
            var origin = Positions[a];
            var normal = Vector3.Cross(Positions[b] - origin, Positions[c] - origin);

            if (normal.LengthSquared() < 1e-20f)
                return;

            var u = Vector3.Normalize(Positions[b] - origin);
            var v = Vector3.Normalize(Vector3.Cross(normal, u));

            Vector2 Flat(int id)
            {
                var d = Positions[id] - origin;
                return new Vector2(Vector3.Dot(d, u), Vector3.Dot(d, v));
            }

            var polygon = new List<int>(loop);
            var points = new Dictionary<int, Vector2>();

            foreach (var id in polygon)
                points[id] = Flat(id);

            var guard = polygon.Count * polygon.Count + 16;

            while (polygon.Count > 3 && guard-- > 0)
            {
                var clipped = false;

                for (var i = 0; i < polygon.Count; i++)
                {
                    var prev = polygon[(i + polygon.Count - 1) % polygon.Count];
                    var current = polygon[i];
                    var next = polygon[(i + 1) % polygon.Count];

                    var pp = points[prev];
                    var pc = points[current];
                    var pn = points[next];

                    if (Cross(pp, pc, pn) <= AreaEpsilon)
                        continue;

                    var blocked = false;

                    for (var j = 0; j < polygon.Count && !blocked; j++)
                    {
                        var other = polygon[j];

                        if (other == prev || other == current || other == next)
                            continue;

                        blocked = InTriangle(points[other], pp, pc, pn);
                    }

                    if (blocked)
                        continue;

                    Emit(prev, current, next, material, key);
                    polygon.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (clipped)
                    continue;

                // No ear: a corner with no turn is dropped, which leaves its neighbours' side straight
                var flat = -1;

                for (var i = 0; i < polygon.Count && flat < 0; i++)
                {
                    var prev = polygon[(i + polygon.Count - 1) % polygon.Count];
                    var next = polygon[(i + 1) % polygon.Count];

                    if (MathF.Abs(Cross(points[prev], points[polygon[i]], points[next])) <= AreaEpsilon)
                        flat = i;
                }

                if (flat < 0)
                    break;

                polygon.RemoveAt(flat);
            }

            // What is left after the ears: a last triangle, or a loop that couldn't be clipped (one that touches itself),
            // fanned. A triangle turned the other way from the one it came from would overlap its neighbours, so it is
            // left out (it can only be a sliver, where a face of the box passes a hair's breadth from an edge).
            for (var i = 1; i < polygon.Count - 1; i++)
            {
                if (Cross(points[polygon[0]], points[polygon[i]], points[polygon[i + 1]]) > 0)
                    Emit(polygon[0], polygon[i], polygon[i + 1], material, key);
            }
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        /// <summary>Whether a point is inside a triangle or on its sides (wound anticlockwise).</summary>
        private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            const float epsilon = -1e-7f;

            return Cross(a, b, p) >= epsilon && Cross(b, c, p) >= epsilon && Cross(c, a, p) >= epsilon;
        }

        #endregion

        #region Output

        private void Emit(int a, int b, int c, uint material, uint key)
        {
            if (CountOnly)
                return;

            // Corners on one line leave no area to collide with
            if (a == b || b == c || c == a || Area(Positions[a], Positions[b], Positions[c]) <= AreaEpsilon)
                return;

            OutIndices.Add(a);
            OutIndices.Add(b);
            OutIndices.Add(c);
            OutMaterials.Add(material);
            OutKeys.Add(key);
        }

        private static float Area(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a).Length() * 0.5f;
        }

        private float PolygonArea(List<ClipVertex> polygon)
        {
            var sum = Vector3.Zero;
            var origin = Positions[polygon[0].Id];

            for (var i = 1; i < polygon.Count - 1; i++)
                sum += Vector3.Cross(Positions[polygon[i].Id] - origin, Positions[polygon[i + 1].Id] - origin);

            return sum.Length() * 0.5f;
        }

        /// <summary>The kept triangles, with only the vertices they use, renumbered in order of first use.</summary>
        public Result Compact(int removed, int cut)
        {
            var remap = new Dictionary<int, int>();
            var vertices = new List<Vector3>();
            var indices = new List<int>(OutIndices.Count);

            foreach (var index in OutIndices)
            {
                if (!remap.TryGetValue(index, out var mapped))
                {
                    mapped = vertices.Count;
                    vertices.Add(Positions[index]);
                    remap[index] = mapped;
                }

                indices.Add(mapped);
            }

            return new Result(vertices, indices, new List<uint>(OutMaterials), new List<uint>(OutKeys), removed, cut);
        }

        #endregion
    }
}
