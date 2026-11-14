using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>What an EDGE handle moves.</summary>
public enum EdgeHandleKind
{
    /// <summary>A grapple point: V1, with V2 (its few-centimetre direction point) kept at the same offset.</summary>
    Grapple,

    /// <summary>The pull point of a grapple: V3.</summary>
    Pull,

    /// <summary>An end of one or more ledge or wall segments: every end of the same chain at that spot.</summary>
    Vertex
}

/// <summary>One point of an entry: 1 is V1, 2 is V2, 3 is V3.</summary>
public readonly record struct EdgePointRef(int Entry, int Point);

/// <summary>
/// A point of an EDGE file that is picked and moved as one, in the collision part's space.
/// </summary>
public sealed record EdgeHandle(EdgeHandleKind Kind, Vector3 Position, IReadOnlyList<EdgePointRef> Points)
{
    /// <summary>The entry the handle was made for first, which the EDGE Editor selects with it.</summary>
    public int Entry => Points[0].Entry;

    public bool Contains(EdgePointRef point) => Points.Contains(point);
}

/// <summary>
/// The handles of an EDGE file and moving them (EDGE Editor M3). Uses only SoulsFormats, so EdgeSurvey's check
/// compiles this same file.
///
/// Ledges and walls are chains of segments that meet end to end with the same chain ID (Unk30) and type: an end of one
/// segment at exactly the spot of an end of another (EdgeSurvey values: 5941 of 5951 touching pairs, 5907 sharing the
/// chain ID) is one handle, so moving it keeps the chain joined. Ends of other chains or types at the same spot stay
/// their own handles.
/// </summary>
public static class EdgeHandles
{
    public static List<EdgeHandle> Build(EDGE edge)
    {
        List<EdgeHandle> handles = new();

        // Chain ends, grouped by (type, chain ID, position) in the order they are first seen
        Dictionary<(EDGE.EdgeType, int, Vector3), List<EdgePointRef>> vertices = new();
        List<(EDGE.EdgeType, int, Vector3)> order = new();

        void AddVertex(EDGE.Edge entry, int index, int point, Vector3 position)
        {
            var key = (entry.Type, entry.Unk30, position);

            if (!vertices.TryGetValue(key, out var refs))
            {
                refs = new List<EdgePointRef>();
                vertices[key] = refs;
                order.Add(key);
            }

            refs.Add(new EdgePointRef(index, point));
        }

        for (var i = 0; i < edge.Edges.Count; i++)
        {
            var entry = edge.Edges[i];

            if (entry.Type is EDGE.EdgeType.Grapple)
            {
                handles.Add(new EdgeHandle(EdgeHandleKind.Grapple, entry.V1, new[] { new EdgePointRef(i, 1), new EdgePointRef(i, 2) }));

                if (entry.V3 != Vector3.Zero)
                {
                    handles.Add(new EdgeHandle(EdgeHandleKind.Pull, entry.V3, new[] { new EdgePointRef(i, 3) }));
                }
            }
            else
            {
                AddVertex(entry, i, 1, entry.V1);
                AddVertex(entry, i, 2, entry.V2);
            }
        }

        foreach (var key in order)
        {
            handles.Add(new EdgeHandle(EdgeHandleKind.Vertex, key.Item3, vertices[key]));
        }

        return handles;
    }

    /// <summary>
    /// A copy of the file with the handle at a new position. A grapple's direction point moves with it.
    /// </summary>
    public static EDGE Move(EDGE edge, EdgeHandle handle, Vector3 position)
    {
        var moved = new EDGE
        {
            ID = edge.ID,
            Edges = edge.Edges.Select(e => e.Clone()).ToList()
        };

        foreach (var point in handle.Points)
        {
            if (point.Entry < 0 || point.Entry >= moved.Edges.Count)
                continue;

            var entry = moved.Edges[point.Entry];

            switch (handle.Kind, point.Point)
            {
                case (EdgeHandleKind.Grapple, 1):
                    // The direction point keeps its offset, so it is moved by the same amount
                    entry.V2 += position - entry.V1;
                    entry.V1 = position;
                    break;
                case (EdgeHandleKind.Grapple, 2):
                    break;
                case (_, 1):
                    entry.V1 = position;
                    break;
                case (_, 2):
                    entry.V2 = position;
                    break;
                case (_, 3):
                    entry.V3 = position;
                    break;
            }
        }

        return moved;
    }

    /// <summary>
    /// The handle of a file that holds a point, e.g. to keep the same handle picked after the file changed.
    /// </summary>
    public static EdgeHandle Find(List<EdgeHandle> handles, EdgeHandleKind kind, EdgePointRef point)
    {
        return handles.FirstOrDefault(h => h.Kind == kind && h.Contains(point));
    }

    /// <summary>
    /// Where a ray from <paramref name="origin"/> along the unit <paramref name="direction"/> passes nearest a sphere's
    /// centre, if within <paramref name="radius"/>: the distance along the ray, or null.
    /// </summary>
    public static float? RaySphere(Vector3 origin, Vector3 direction, Vector3 centre, float radius)
    {
        // Starting inside it
        if (Vector3.DistanceSquared(origin, centre) <= radius * radius)
            return 0;

        var t = Vector3.Dot(centre - origin, direction);

        if (t < 0)
            return null;

        var closest = origin + direction * t;

        return Vector3.DistanceSquared(closest, centre) <= radius * radius ? t : null;
    }

    /// <summary>
    /// The distance along a ray (unit direction) to where it passes within <paramref name="radius"/> of a segment, or
    /// null.
    /// </summary>
    public static float? RaySegment(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, float radius)
    {
        var u = b - a;
        var w = origin - a;

        var uu = Vector3.Dot(u, u);
        var ud = Vector3.Dot(u, direction);
        var uw = Vector3.Dot(u, w);
        var dw = Vector3.Dot(direction, w);

        var denominator = uu - ud * ud;

        // Parallel to the segment: test its nearer end
        if (uu < 1e-12f || MathF.Abs(denominator) < 1e-9f)
            return RaySphere(origin, direction, a, radius);

        // Starting inside it
        var fromOrigin = Math.Clamp(Vector3.Dot(origin - a, u) / uu, 0, 1);

        if (Vector3.DistanceSquared(origin, a + u * fromOrigin) <= radius * radius)
            return 0;

        var s = Math.Clamp((uw - ud * dw) / denominator, 0, 1);
        var t = Vector3.Dot(a + u * s - origin, direction);

        if (t < 0)
            return null;

        var onRay = origin + direction * t;
        var onSegment = a + u * s;

        return Vector3.DistanceSquared(onRay, onSegment) <= radius * radius ? t : null;
    }
}
