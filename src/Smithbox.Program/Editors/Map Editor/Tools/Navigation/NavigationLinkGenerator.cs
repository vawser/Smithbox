using Havok.hk2018;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Jump and fall links for a generated navmesh (Navmesh Generation Plan, Links): from every boundary edge, a search
// outward for the first navmesh surface within reach, joined by a user-edge link chosen by the height change.
// Kept free of the rest of Smithbox; NavSurvey `navgen` runs it, and `linkgeom` measures the game's own links.
//
// How the game's links look (NavSurvey linkgeom, 47 maps, ~186k links):
// - jumps (edge data 0x2, 0x12) always go both ways; in n their height change is within ±4.5 m (p95 ±2.6 m). o has none;
// - falls (0x4, 0x14) are always one-way, down by up to 15.8 m. Their high byte is the drop in tenths of a metre,
//   rounded down (0x60: a drop of 9.60–9.69 m);
// - both reach 0.8–5 m across in n and 2.6–9.3 m in o, and start and end 0.4–1 m inside their faces.
// What decides jump or fall in the game isn't in their shape (both are mostly over a hole), so here it is the height:
// within the jump height both ways, a deeper drop one-way.

/// <summary>Jump and fall links found for a navmesh, and how many of each.</summary>
public sealed record NavLinkSet(List<NavUserEdgeLink> Links, int Jumps, int Falls);

public static class NavLinkGenerator
{
    private const uint None = 0xFFFFFFFF;

    /// <summary>The edge data of a jump link: the game's Jump bit.</summary>
    public const int JumpData = 0x2;

    /// <summary>The edge data of a fall link: the game's Fall bit, with the drop in tenths of a metre in the high byte.</summary>
    public static int FallData(float drop) => 0x4 | (Math.Clamp((int)MathF.Floor(drop * 10 + 1e-3f), 0, 0xFF) << 8);

    /// <summary>How far apart the starts along a boundary edge are tried.</summary>
    private const float Spacing = 1f;

    /// <summary>How far inside its face each end of a link sits, as the game's do.</summary>
    private const float Inset = 0.4f;

    /// <summary>The step of the outward search.</summary>
    private const float Step = 0.2f;

    /// <summary>The most links a face takes part in, so a ledge doesn't sprout a fan of them.</summary>
    private const int MaxLinksPerFace = 6;

    /// <summary>
    /// A link isn't made where walking there costs less than this many times the straight distance (plus a few metres):
    /// the link would only be a shortcut round a corner.
    /// </summary>
    private const float WalkFactor = 3f;
    private const float WalkSlack = 4f;

    /// <summary>
    /// The links for a mesh without user edges: from each boundary edge, every metre, outward to the first surface within
    /// the reach. A surface within the jump height either way is a jump, both ways; one lower than that and the climb,
    /// down to the fall height, is a fall, one way. None where the path through the air (out at body height, then up or
    /// down) meets the collision or a blocker, nor where the two faces are a short walk apart.
    /// </summary>
    /// <param name="solids">Triangles (three points each) that block a link's path: the collision, and the blockers' boxes.</param>
    public static NavLinkSet Find(hkaiNavMesh mesh, IReadOnlyList<Vector3> solids, NavGenParameters p)
    {
        var links = new List<NavUserEdgeLink>();
        int jumps = 0, falls = 0;
        if (mesh.m_faces.Count == 0 || p.LinkReach <= 0 || (p.JumpHeight <= 0 && p.FallHeight <= p.MaxClimb))
            return new NavLinkSet(links, 0, 0);

        var faces = new FaceGrid(mesh, 2f);
        var collision = new TriangleGrid(solids, 4f);

        var linksOf = new int[mesh.m_faces.Count];
        var made = new HashSet<(int, int)>();
        var walkable = new Dictionary<(int, int), bool>();

        float jumpHeight = MathF.Max(0, p.JumpHeight);
        float minDrop = MathF.Max(jumpHeight, p.MaxClimb);
        // Surfaces higher than this above the start are overhead (a bridge, a ceiling), not in the way
        float overhead = MathF.Max(jumpHeight, p.MaxClimb) + p.AgentHeight;
        float bodyLow = p.AgentHeight * 0.5f, bodyHigh = p.AgentHeight * 0.9f;

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            var centre = faces.Centre(f);

            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
            {
                var edge = mesh.m_edges[e];
                if (edge.m_oppositeFace != None)
                    continue;

                var a = V(mesh, edge.m_a);
                var b = V(mesh, edge.m_b);
                var along = new Vector2(b.X - a.X, b.Z - a.Z);
                float length = along.Length();
                if (length < 0.1f)
                    continue;

                // Out of the face: across the edge, away from the face's centre
                var outward = Vector2.Normalize(new Vector2(along.Y, -along.X));
                var mid = (a + b) / 2;
                if (Vector2.Dot(outward, new Vector2(mid.X - centre.X, mid.Z - centre.Z)) < 0)
                    outward = -outward;
                var out3 = new Vector3(outward.X, 0, outward.Y);

                int samples = Math.Max(1, (int)MathF.Round(length / Spacing));
                for (int s = 0; s < samples && linksOf[f] < MaxLinksPerFace; s++)
                {
                    var onEdge = Vector3.Lerp(a, b, (s + 0.5f) / samples);
                    var start = Inside(faces, f, onEdge, centre);

                    // Outward to the first surface that isn't the start's own face or below the fall height
                    for (float d = Step; d <= p.LinkReach; d += Step)
                    {
                        var q = onEdge + out3 * d;
                        var hit = faces.Highest(q.X, q.Z, f, start.Y + overhead, start.Y - MathF.Max(p.FallHeight, jumpHeight));
                        if (hit == null)
                            continue;

                        var (target, y) = hit.Value;
                        float rise = y - start.Y;

                        // A jump either way, or a fall; else something too high to reach, or ground to walk on: stop
                        bool jump = jumpHeight > 0 && MathF.Abs(rise) <= jumpHeight;
                        bool fall = !jump && -rise > minDrop && -rise <= p.FallHeight;
                        if (!jump && !fall)
                            break;

                        if (linksOf[target] >= MaxLinksPerFace || made.Contains(Key(f, target, jump)) || made.Contains(Key(target, f, true)))
                            break;

                        // The landing a little further in, if it is still on the target face
                        var end = new Vector3(q.X, y, q.Z);
                        var further = q + out3 * Inset;
                        if (faces.HeightIn(target, further.X, further.Z) is float fy)
                            end = new Vector3(further.X, fy, further.Z);

                        if (ShortWalk(mesh, faces, f, target, Vector3.Distance(start, end), walkable))
                            break;

                        if (!Clear(collision, start, end, bodyLow) || !Clear(collision, start, end, bodyHigh))
                            break;

                        float drop = start.Y - end.Y;
                        int data = jump ? JumpData : FallData(drop);
                        links.Add(new NavUserEdgeLink(f, start, target, end, jump, data, NavUserEdges.DefaultClearance));
                        made.Add(Key(f, target, jump));
                        linksOf[f]++;
                        linksOf[target]++;
                        if (jump) jumps++; else falls++;
                        break;
                    }
                }
            }
        }

        return new NavLinkSet(links, jumps, falls);
    }

    /// <summary>A both-ways link is one link whichever face it is found from.</summary>
    private static (int, int) Key(int from, int to, bool bothWays) => bothWays && to < from ? (to, from) : (from, to);

    private static Vector3 V(hkaiNavMesh mesh, int i) => new(mesh.m_vertices[i].X, mesh.m_vertices[i].Y, mesh.m_vertices[i].Z);

    /// <summary>A point on a face's edge moved the inset towards the face's centre (no further than it), on the face.</summary>
    private static Vector3 Inside(FaceGrid faces, int face, Vector3 onEdge, Vector3 centre)
    {
        var toCentre = centre - onEdge;
        float flat = new Vector2(toCentre.X, toCentre.Z).Length();
        var point = flat > 1e-4f ? onEdge + toCentre * MathF.Min(1, Inset / flat) : onEdge;
        return faces.HeightIn(face, point.X, point.Z) is float y ? point with { Y = y } : point;
    }

    /// <summary>
    /// Whether the target face is a walk from the start face of no more than a few times the link's length: a search over
    /// the faces' adjacency, by the distance between their centres, cut off at that length. Remembered per pair.
    /// </summary>
    private static bool ShortWalk(hkaiNavMesh mesh, FaceGrid faces, int from, int to, float straight, Dictionary<(int, int), bool> known)
    {
        if (known.TryGetValue((from, to), out bool cached))
            return cached;

        float limit = straight * WalkFactor + WalkSlack;
        var best = new Dictionary<int, float> { [from] = 0 };
        var queue = new PriorityQueue<int, float>();
        queue.Enqueue(from, 0);
        bool reached = false;

        while (queue.TryDequeue(out int face, out float cost))
        {
            if (face == to)
            {
                reached = true;
                break;
            }
            if (cost > best[face])
                continue;

            var f = mesh.m_faces[face];
            for (int e = f.m_startEdgeIndex; e < f.m_startEdgeIndex + f.m_numEdges; e++)
            {
                uint opposite = mesh.m_edges[e].m_oppositeFace;
                if (opposite == None)
                    continue;

                int next = (int)(opposite & 0x3FFFFF);
                if (next >= mesh.m_faces.Count)
                    continue;

                float c = cost + Vector3.Distance(faces.Centre(face), faces.Centre(next));
                if (c > limit || (best.TryGetValue(next, out float old) && old <= c))
                    continue;

                best[next] = c;
                queue.Enqueue(next, c);
            }
        }

        known[(from, to)] = reached;
        known[(to, from)] = reached;
        return reached;
    }

    /// <summary>
    /// Whether a character's path from one end to the other, at a height above the ground, is clear of the solids: out
    /// level with the higher end, then straight up or down to the lower one. Rising, it goes up first, then out.
    /// </summary>
    private static bool Clear(TriangleGrid solids, Vector3 start, Vector3 end, float height)
    {
        var up = new Vector3(0, height, 0);
        var corner = end.Y > start.Y ? new Vector3(start.X, end.Y, start.Z) : new Vector3(end.X, start.Y, end.Z);
        return !solids.Hits(start + up, corner + up) && !solids.Hits(corner + up, end + up);
    }

    /// <summary>The faces of a mesh bucketed on an XZ grid, for the surface height under a point.</summary>
    private sealed class FaceGrid
    {
        private readonly hkaiNavMesh Mesh;
        private readonly float Cell;
        private readonly Dictionary<(int, int), List<int>> Cells = new();
        private readonly Vector3[] Centres;

        public FaceGrid(hkaiNavMesh mesh, float cell)
        {
            Mesh = mesh;
            Cell = cell;
            Centres = new Vector3[mesh.m_faces.Count];

            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                var face = mesh.m_faces[f];
                var min = new Vector3(float.MaxValue);
                var max = new Vector3(float.MinValue);
                var sum = Vector3.Zero;
                for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
                {
                    var v = V(mesh, mesh.m_edges[e].m_a);
                    min = Vector3.Min(min, v);
                    max = Vector3.Max(max, v);
                    sum += v;
                }
                Centres[f] = sum / Math.Max(1, (int)face.m_numEdges);

                for (int x = (int)MathF.Floor(min.X / cell); x <= (int)MathF.Floor(max.X / cell); x++)
                    for (int z = (int)MathF.Floor(min.Z / cell); z <= (int)MathF.Floor(max.Z / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var list))
                            Cells[(x, z)] = list = new List<int>();
                        list.Add(f);
                    }
            }
        }

        public Vector3 Centre(int face) => Centres[face];

        /// <summary>The highest face (but one) under or over a point within a height range, and its height there, or null.</summary>
        public (int Face, float Y)? Highest(float x, float z, int except, float top, float bottom)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(x / Cell), (int)MathF.Floor(z / Cell)), out var list))
                return null;

            (int, float)? best = null;
            foreach (var f in list)
            {
                if (f == except || HeightIn(f, x, z) is not float y || y > top || y < bottom)
                    continue;
                if (best == null || y > best.Value.Item2)
                    best = (f, y);
            }
            return best;
        }

        /// <summary>The height of a face at a point inside it seen from above (by a fan of its corners), or null.</summary>
        public float? HeightIn(int face, float x, float z)
        {
            var f = Mesh.m_faces[face];
            var a = V(Mesh, Mesh.m_edges[f.m_startEdgeIndex].m_a);
            for (int k = 1; k + 1 < f.m_numEdges; k++)
            {
                var b = V(Mesh, Mesh.m_edges[f.m_startEdgeIndex + k].m_a);
                var c = V(Mesh, Mesh.m_edges[f.m_startEdgeIndex + k + 1].m_a);
                float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
                if (MathF.Abs(d) < 1e-9f)
                    continue;

                float u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / d;
                float v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / d;
                if (u < -1e-4f || v < -1e-4f || u + v > 1 + 1e-4f)
                    continue;
                return u * a.Y + v * b.Y + (1 - u - v) * c.Y;
            }
            return null;
        }
    }

    /// <summary>Triangles (three points each) bucketed on an XZ grid, for whether a segment passes through any.</summary>
    private sealed class TriangleGrid
    {
        private readonly IReadOnlyList<Vector3> T;
        private readonly float Cell;
        private readonly Dictionary<(int, int), List<int>> Cells = new();
        private readonly int[] Seen;
        private int Stamp;

        public TriangleGrid(IReadOnlyList<Vector3> triangles, float cell)
        {
            T = triangles;
            Cell = cell;
            Seen = new int[triangles.Count / 3];

            for (int i = 0; i + 2 < T.Count; i += 3)
            {
                float x0 = MathF.Min(T[i].X, MathF.Min(T[i + 1].X, T[i + 2].X)), x1 = MathF.Max(T[i].X, MathF.Max(T[i + 1].X, T[i + 2].X));
                float z0 = MathF.Min(T[i].Z, MathF.Min(T[i + 1].Z, T[i + 2].Z)), z1 = MathF.Max(T[i].Z, MathF.Max(T[i + 1].Z, T[i + 2].Z));

                for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                    for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var list))
                            Cells[(x, z)] = list = new List<int>();
                        list.Add(i);
                    }
            }
        }

        /// <summary>Whether the segment from p to q passes through a triangle (either side).</summary>
        public bool Hits(Vector3 p, Vector3 q)
        {
            Stamp++;
            var dir = q - p;

            for (int x = (int)MathF.Floor(MathF.Min(p.X, q.X) / Cell); x <= (int)MathF.Floor(MathF.Max(p.X, q.X) / Cell); x++)
                for (int z = (int)MathF.Floor(MathF.Min(p.Z, q.Z) / Cell); z <= (int)MathF.Floor(MathF.Max(p.Z, q.Z) / Cell); z++)
                {
                    if (!Cells.TryGetValue((x, z), out var list))
                        continue;

                    foreach (var i in list)
                    {
                        if (Seen[i / 3] == Stamp)
                            continue;
                        Seen[i / 3] = Stamp;

                        // Möller–Trumbore, for t in [0, 1]
                        var e1 = T[i + 1] - T[i];
                        var e2 = T[i + 2] - T[i];
                        var h = Vector3.Cross(dir, e2);
                        float det = Vector3.Dot(e1, h);
                        if (MathF.Abs(det) < 1e-9f)
                            continue;

                        float inv = 1 / det;
                        var s = p - T[i];
                        float u = Vector3.Dot(s, h) * inv;
                        if (u < 0 || u > 1)
                            continue;

                        var qv = Vector3.Cross(s, e1);
                        float v = Vector3.Dot(dir, qv) * inv;
                        if (v < 0 || u + v > 1)
                            continue;

                        float t = Vector3.Dot(e2, qv) * inv;
                        if (t >= 0 && t <= 1)
                            return true;
                    }
                }

            return false;
        }
    }
}
