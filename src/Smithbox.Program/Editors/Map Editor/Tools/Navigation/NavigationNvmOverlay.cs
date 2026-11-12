using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>A triangle of an NVM under the cursor (map space).</summary>
public sealed record NvmHit(NvmRoom Room, int Triangle, float Distance, Vector3 Point);

/// <summary>
/// The NVM mode's overlay (Phase N6): the triangles filled by their flags, their edges, gate and entity triangles, the
/// MCP room boxes and the MCG graph, as a <see cref="NavOverlay"/> for the shared renderer. Renderer-free, so NavSurvey
/// builds it from the game files.
/// </summary>
public static class NvmOverlayBuilder
{
    /// <summary>What the overlay highlights: a room's entity, an MCG node or edge.</summary>
    public sealed record Selection(NvmRoom EntityRoom = null, int Entity = -1, int Node = -1, int Edge = -1);

    /// <param name="vertices">A room's vertices as edited (Edit tab), or null for its NVM's.</param>
    /// <param name="include">Which rooms to draw; null for all.</param>
    public static NavOverlay Build(NvmMap map, ISet<NavOverlayLayer> layers, Func<NvmRoom, IReadOnlyList<Vector3>> vertices = null,
        Func<NvmRoom, bool> include = null, Selection selection = null)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = NavmeshSet.N };
        var lift = new Vector3(0, NavigationOverlayBuilder.Lift, 0);

        foreach (var room in map.Rooms)
        {
            if (room.Nvm == null || (include != null && !include(room)))
                continue;

            var nvm = room.Nvm;
            var local = vertices?.Invoke(room) ?? nvm.Vertices;
            var placed = new Vector3[local.Count];

            for (int i = 0; i < placed.Length; i++)
                placed[i] = Vector3.Transform(local[i], room.World) + lift;

            for (int t = 0; t < nvm.Triangles.Count; t++)
            {
                var tri = nvm.Triangles[t];
                if (!InRange(tri, placed.Length))
                    continue;

                var a = placed[tri.VertexIndex1];
                var b = placed[tri.VertexIndex2];
                var c = placed[tri.VertexIndex3];

                if (layers.Contains(NavOverlayLayer.Faces))
                {
                    var batch = overlay.Batch(NavOverlayLayer.Faces, (uint)tri.Flags);
                    batch.Triangles.Add((a, b, c));
                    batch.FaceCount++;
                }

                // Each inner edge once (from the lower triangle), every open edge
                var neighbours = NvmGeometry.Neighbours(tri);
                var ends = new[] { (a, b), (b, c), (a, c) };

                for (int k = 0; k < 3; k++)
                {
                    bool open = neighbours[k] < 0 || neighbours[k] >= nvm.Triangles.Count;

                    if (open && layers.Contains(NavOverlayLayer.OpenEdges))
                        overlay.Batch(NavOverlayLayer.OpenEdges).Lines.Add(ends[k]);
                    else if (!open && neighbours[k] > t && layers.Contains(NavOverlayLayer.InnerEdges))
                        overlay.Batch(NavOverlayLayer.InnerEdges).Lines.Add(ends[k]);
                }

                if ((tri.Flags & NVM.TriangleFlags.GATE) != 0 && layers.Contains(NavOverlayLayer.NvmGates))
                    Outline(overlay.Batch(NavOverlayLayer.NvmGates), a, b, c, lift);
            }

            if (layers.Contains(NavOverlayLayer.NvmEntities))
            {
                for (int e = 0; e < nvm.Entities.Count; e++)
                {
                    bool chosen = selection != null && selection.EntityRoom == room && selection.Entity == e;

                    foreach (var t in nvm.Entities[e].TriangleIndices)
                    {
                        if (t < 0 || t >= nvm.Triangles.Count || !InRange(nvm.Triangles[t], placed.Length))
                            continue;

                        var tri = nvm.Triangles[t];
                        Outline(overlay.Batch(chosen ? NavOverlayLayer.Highlight : NavOverlayLayer.NvmEntities),
                            placed[tri.VertexIndex1], placed[tri.VertexIndex2], placed[tri.VertexIndex3], lift);
                    }
                }
            }
        }

        // The MCP's own rooms
        if (map.Mcp != null && layers.Contains(NavOverlayLayer.McpRooms))
        {
            foreach (var room in map.Mcp.Rooms)
            {
                if (room.MapID != map.MapUid)
                    continue;

                if (include != null && (room.LocalIndex < 0 || room.LocalIndex >= map.Rooms.Count || !include(map.Rooms[room.LocalIndex])))
                    continue;

                Box(overlay.Batch(NavOverlayLayer.McpRooms), room.BoundingBoxMin, room.BoundingBoxMax);
            }
        }

        // The MCG: the nodes and edges of this map's rooms, and the nodes they reach
        if (map.Mcg != null && (layers.Contains(NavOverlayLayer.McgNodes) || layers.Contains(NavOverlayLayer.McgEdges)))
        {
            var mcg = map.Mcg;
            var shownNodes = new HashSet<int>();

            for (int e = 0; e < mcg.Edges.Count; e++)
            {
                var edge = mcg.Edges[e];
                if (edge.MapID != map.MapUid || !NodeInRange(mcg, edge.NodeIndexA) || !NodeInRange(mcg, edge.NodeIndexB))
                    continue;

                var room = map.RoomOfMcp(edge.MCPRoomIndex);
                if (include != null && (room == null || !include(room)))
                    continue;

                shownNodes.Add(edge.NodeIndexA);
                shownNodes.Add(edge.NodeIndexB);

                if (layers.Contains(NavOverlayLayer.McgEdges) && e != selection?.Edge)
                    overlay.Batch(NavOverlayLayer.McgEdges).Lines.Add((mcg.Nodes[edge.NodeIndexA].Position + lift * 4, mcg.Nodes[edge.NodeIndexB].Position + lift * 4));
            }

            if (layers.Contains(NavOverlayLayer.McgNodes))
            {
                foreach (var n in shownNodes)
                {
                    if (n != selection?.Node)
                        Cross(overlay.Batch(NavOverlayLayer.McgNodes), mcg.Nodes[n].Position + lift * 4, NavigationOverlayBuilder.GateSize);
                }
            }

            // The chosen node and edge, over everything
            if (selection != null && NodeInRange(mcg, selection.Node))
                Cross(overlay.Batch(NavOverlayLayer.McgSelected), mcg.Nodes[selection.Node].Position + lift * 4, NavigationOverlayBuilder.GateSize * 1.5f);

            if (selection != null && selection.Edge >= 0 && selection.Edge < mcg.Edges.Count)
            {
                var edge = mcg.Edges[selection.Edge];
                if (NodeInRange(mcg, edge.NodeIndexA) && NodeInRange(mcg, edge.NodeIndexB))
                    overlay.Batch(NavOverlayLayer.McgSelected).Lines.Add((mcg.Nodes[edge.NodeIndexA].Position + lift * 4, mcg.Nodes[edge.NodeIndexB].Position + lift * 4));
            }
        }

        return overlay;
    }

    /// <summary>The outline of one triangle, drawn over everything (the picked triangle).</summary>
    public static NavOverlay Highlight(NvmMap map, NvmHit hit, IReadOnlyList<Vector3> vertices = null)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = NavmeshSet.N };
        var lift = new Vector3(0, NavigationOverlayBuilder.Lift * 2, 0);
        var (a, b, c) = NvmGeometry.Corners(hit.Room.Nvm, hit.Triangle, vertices);
        Outline(overlay.Batch(NavOverlayLayer.Highlight), Vector3.Transform(a, hit.Room.World) + lift,
            Vector3.Transform(b, hit.Room.World) + lift, Vector3.Transform(c, hit.Room.World) + lift, Vector3.Zero);
        return overlay;
    }

    /// <summary>The nearest triangle a ray in map space hits, or null.</summary>
    public static NvmHit Pick(NvmMap map, Vector3 origin, Vector3 direction, Func<NvmRoom, IReadOnlyList<Vector3>> vertices = null,
        Func<NvmRoom, bool> include = null)
    {
        NvmHit best = null;

        foreach (var room in map.Rooms)
        {
            if (room.Nvm == null || (include != null && !include(room)))
                continue;

            if (!Matrix4x4.Invert(room.World, out var inverse))
                continue;

            var nvm = room.Nvm;
            var local = vertices?.Invoke(room) ?? nvm.Vertices;
            var localOrigin = Vector3.Transform(origin, inverse);
            var localDirection = Vector3.TransformNormal(direction, inverse);

            for (int t = 0; t < nvm.Triangles.Count; t++)
            {
                var tri = nvm.Triangles[t];
                if (!InRange(tri, local.Count))
                    continue;

                if (!NavigationOverlayBuilder.RayTriangle(localOrigin, localDirection, local[tri.VertexIndex1], local[tri.VertexIndex2], local[tri.VertexIndex3], out float hitT))
                    continue;

                var point = Vector3.Transform(localOrigin + localDirection * hitT, room.World);
                float distance = Vector3.Distance(origin, point);

                if (best == null || distance < best.Distance)
                    best = new NvmHit(room, t, distance, point);
            }
        }

        return best;
    }

    /// <summary>The MCG node of this map's edges nearest a ray, within the given distance of it, or -1.</summary>
    public static int PickNode(NvmMap map, Vector3 origin, Vector3 direction, float within)
    {
        var mcg = map.Mcg;
        if (mcg == null)
            return -1;

        var dir = Vector3.Normalize(direction);
        int best = -1;
        float bestAlong = float.MaxValue;

        var own = new HashSet<int>();
        foreach (var edge in mcg.Edges)
        {
            if (edge.MapID == map.MapUid)
            {
                own.Add(edge.NodeIndexA);
                own.Add(edge.NodeIndexB);
            }
        }

        foreach (var n in own)
        {
            if (!NodeInRange(mcg, n))
                continue;

            var p = mcg.Nodes[n].Position;
            float along = Vector3.Dot(p - origin, dir);
            if (along < 0)
                continue;

            float off = Vector3.Distance(origin + dir * along, p);
            if (off <= within && along < bestAlong)
            {
                bestAlong = along;
                best = n;
            }
        }

        return best;
    }

    private static bool InRange(NVM.Triangle t, int vertices)
        => t.VertexIndex1 >= 0 && t.VertexIndex1 < vertices && t.VertexIndex2 >= 0 && t.VertexIndex2 < vertices && t.VertexIndex3 >= 0 && t.VertexIndex3 < vertices;

    private static bool NodeInRange(MCG mcg, int node) => node >= 0 && node < mcg.Nodes.Count;

    private static void Outline(NavOverlayBatch batch, Vector3 a, Vector3 b, Vector3 c, Vector3 lift)
    {
        batch.Lines.Add((a + lift, b + lift));
        batch.Lines.Add((b + lift, c + lift));
        batch.Lines.Add((c + lift, a + lift));
    }

    private static void Cross(NavOverlayBatch batch, Vector3 p, float size)
    {
        float h = size / 2;
        batch.Lines.Add((p - new Vector3(h, 0, 0), p + new Vector3(h, 0, 0)));
        batch.Lines.Add((p - new Vector3(0, 0, h), p + new Vector3(0, 0, h)));
        batch.Lines.Add((p, p + new Vector3(0, size, 0)));
    }

    private static void Box(NavOverlayBatch batch, Vector3 min, Vector3 max)
    {
        Vector3 C(int k) => new((k & 1) != 0 ? max.X : min.X, (k & 2) != 0 ? max.Y : min.Y, (k & 4) != 0 ? max.Z : min.Z);

        foreach (var (i, j) in new[] { (0, 1), (2, 3), (4, 5), (6, 7), (0, 2), (1, 3), (4, 6), (5, 7), (0, 4), (1, 5), (2, 6), (3, 7) })
            batch.Lines.Add((C(i), C(j)));
    }
}
