using Havok.hk2018;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The geometry the Navigation tool draws over a map, and the face picking it uses. Kept free of the renderer, so
// Tools/NavSurvey can build it from the game files (Navigation Editor Plan, Phase 1).

/// <summary>
/// The things the overlay can draw. Each is drawn in its own colour; faces in one colour per face data value.
/// </summary>
public enum NavOverlayLayer
{
    /// <summary>The faces, filled, coloured by their face data (hkaiNavMesh m_faceData).</summary>
    Faces,

    /// <summary>The edges between two faces of the same navmesh.</summary>
    InnerEdges,

    /// <summary>Boundary edges no connection uses.</summary>
    OpenEdges,

    /// <summary>Boundary edges joined to another navmesh of the map by an NVA connector.</summary>
    ConnectedInMap,

    /// <summary>Boundary edges joined to a navmesh of a neighbouring map by an NVC file.</summary>
    ConnectedCrossMap,

    /// <summary>User edges (jump, ladder and drop links): the edge, and an arc to the face it leads to.</summary>
    UserEdges,

    /// <summary>User edges that are also blocked (EDGE_BLOCKED).</summary>
    BlockedUserEdges,

    /// <summary>The gate nodes of the NVA that pair with a gate of their neighbour, as crosses with a tick into their navmesh.</summary>
    GateNodes,

    /// <summary>Gate nodes without a partner (the game's all point at a navmesh that has 16 gates).</summary>
    GateNodesUnpaired,

    /// <summary>Gate nodes whose target or partner is wrong (see NavGateStatus.Broken).</summary>
    GateNodesBroken,

    /// <summary>For the navmesh chosen in the Gates tab, a line between each two of its gates that can reach each other.</summary>
    GateCosts,

    /// <summary>The gate chosen in the Gates tab, drawn over everything.</summary>
    GateSelected,

    /// <summary>The ONAV's grid bounds and the position of each area.</summary>
    OnavAreas,

    /// <summary>The links between ONAV areas.</summary>
    OnavLinks,

    /// <summary>The grid cells each ONAV area's cell mask covers. The bit order is a guess (X fastest, then Y, then Z).</summary>
    OnavCells,

    /// <summary>The outline of the picked face, drawn over everything.</summary>
    Highlight,

    /// <summary>The collision of the parts tagged as blockers, filled in red: generation goes around them.</summary>
    Blockers,
}

/// <summary>
/// One colour's worth of overlay geometry.
/// </summary>
public sealed class NavOverlayBatch
{
    public NavOverlayLayer Layer { get; init; }

    /// <summary>For faces, the face data value they share; 0 for the other layers.</summary>
    public uint Key { get; init; }

    public List<(Vector3 A, Vector3 B)> Lines { get; } = new();

    public List<(Vector3 A, Vector3 B, Vector3 C)> Triangles { get; } = new();

    public int FaceCount { get; set; }
}

/// <summary>
/// The overlay of one map and navmesh set.
/// </summary>
public sealed class NavOverlay
{
    public string MapId { get; init; }

    public NavmeshSet Set { get; init; }

    public Dictionary<(NavOverlayLayer Layer, uint Key), NavOverlayBatch> Batches { get; } = new();

    public NavOverlayBatch Batch(NavOverlayLayer layer, uint key = 0)
    {
        if (!Batches.TryGetValue((layer, key), out var batch))
            Batches[(layer, key)] = batch = new NavOverlayBatch { Layer = layer, Key = key };
        return batch;
    }

    /// <summary>The user edges whose target face couldn't be found in their own navmesh, drawn without an arc.</summary>
    public int UserEdgesWithoutTarget { get; set; }
}

/// <summary>
/// A face under the cursor.
/// </summary>
public sealed record NavFaceHit(NVA.Navmesh Entry, NavmeshFile File, int Face, float Distance, Vector3 Point);

public static class NavigationOverlayBuilder
{
    /// <summary>How far faces and edges are lifted above the navmesh, so they show over the Map Editor's own drawing of it.</summary>
    public const float Lift = 0.04f;

    /// <summary>
    /// The entries of a set whose faces are drawn: for the n set, the n entries and the quarters (whose n file is empty).
    /// </summary>
    public static IEnumerable<NVA.Navmesh> EntriesOf(NavigationMap map, NavmeshSet set)
        => map.Nva.Navmeshes.Where(e => NavigationMap.SetOf(e) == set);

    /// <summary>
    /// Builds the overlay of a map's set. The world transforms place a navmesh entry and the map's own data
    /// (gate nodes, ONAV) in the viewport.
    /// </summary>
    public static NavOverlay Build(NavigationMap map, NavmeshSet set, Func<NVA.Navmesh, Matrix4x4> entryWorld, Matrix4x4 mapWorld,
        ISet<NavOverlayLayer> layers, Func<NVA.Navmesh, bool> only = null)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = set };
        var (inMap, crossMap) = ConnectionEnds(map);

        foreach (var entry in EntriesOf(map, set))
        {
            var mesh = map.FileOf(entry)?.Mesh;

            if (mesh == null || mesh.m_faces.Count == 0 || (only != null && !only(entry)))
                continue;

            AddMesh(overlay, entry, mesh, entryWorld(entry), layers, inMap, crossMap);
        }

        AddGates(overlay, map, set, entryWorld, mapWorld, layers, only);

        if (map.Onav != null && map.Onav.Areas.Count > 0)
            AddOnav(overlay, map.Onav, mapWorld, layers);

        return overlay;
    }

    /// <summary>The size of a gate's cross.</summary>
    public const float GateSize = 0.6f;

    /// <summary>
    /// The gates of a set, as crosses coloured by how each stands with its partner. A paired gate shares its point with
    /// its partner, so each gets a short tick towards the middle of the nearest face of its own navmesh.
    /// </summary>
    private static void AddGates(NavOverlay overlay, NavigationMap map, NavmeshSet set, Func<NVA.Navmesh, Matrix4x4> entryWorld, Matrix4x4 mapWorld,
        ISet<NavOverlayLayer> layers, Func<NVA.Navmesh, bool> only)
    {
        if (!layers.Contains(NavOverlayLayer.GateNodes) && !layers.Contains(NavOverlayLayer.GateNodesUnpaired) && !layers.Contains(NavOverlayLayer.GateNodesBroken))
            return;

        foreach (var entry in EntriesOf(map, set))
        {
            if (entry.GateNodes.Count == 0 || (only != null && !only(entry)))
                continue;

            var mesh = map.FileOf(entry)?.Mesh;
            var world = entryWorld(entry);
            List<Vector3> centres = null;

            for (int g = 0; g < entry.GateNodes.Count; g++)
            {
                var layer = NavGates.Status(map, entry, g) switch
                {
                    NavGateStatus.Paired => NavOverlayLayer.GateNodes,
                    NavGateStatus.Broken => NavOverlayLayer.GateNodesBroken,
                    _ => NavOverlayLayer.GateNodesUnpaired,
                };

                if (!layers.Contains(layer))
                    continue;

                var batch = overlay.Batch(layer);
                var point = Vector3.Transform(entry.GateNodes[g].Position, mapWorld);
                AddGateMarker(batch, point, GateSize);

                if (mesh == null || mesh.m_faces.Count == 0)
                    continue;

                centres ??= Enumerable.Range(0, mesh.m_faces.Count).Select(f => Vector3.Transform(FaceCentre(mesh, f), world)).ToList();
                var nearest = centres.OrderBy(c => Vector3.DistanceSquared(c, point)).First();
                var towards = new Vector3(nearest.X - point.X, 0, nearest.Z - point.Z);

                if (towards.LengthSquared() > 1e-6f)
                    batch.Lines.Add((point, point + Vector3.Normalize(towards) * GateSize * 1.5f));
            }
        }
    }

    /// <summary>
    /// The chosen navmesh's gates in the Gates tab: a line between each two that can reach each other, and the chosen
    /// gate's cross, larger, over everything.
    /// </summary>
    public static NavOverlay GateHighlight(NavigationMap map, NVA.Navmesh entry, int selectedGate, Matrix4x4 mapWorld, bool costs)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = NavigationMap.SetOf(entry) };
        var gates = entry.GateNodes;
        var lift = new Vector3(0, Lift * 3, 0);

        if (costs)
        {
            var batch = overlay.Batch(NavOverlayLayer.GateCosts);
            for (int i = 0; i < gates.Count; i++)
                for (int j = i + 1; j < gates.Count; j++)
                    if (j < gates[i].NeighbourGateNodeCosts.Count && gates[i].NeighbourGateNodeCosts[j] != NavGates.Unreachable)
                        batch.Lines.Add((Vector3.Transform(gates[i].Position, mapWorld) + lift, Vector3.Transform(gates[j].Position, mapWorld) + lift));
        }

        if (selectedGate >= 0 && selectedGate < gates.Count)
        {
            var batch = overlay.Batch(NavOverlayLayer.GateSelected);
            var point = Vector3.Transform(gates[selectedGate].Position, mapWorld) + lift;
            AddGateMarker(batch, point, GateSize * 1.6f);
            batch.Lines.Add((point, point + new Vector3(0, GateSize * 3, 0)));
        }

        return overlay;
    }

    /// <summary>The gate of a set nearest a ray, within a distance of it that grows with the distance along it, or null.</summary>
    public static (NVA.Navmesh Entry, int Gate)? PickGate(NavigationMap map, NavmeshSet set, Vector3 origin, Vector3 direction, Matrix4x4 mapWorld)
    {
        (NVA.Navmesh, int)? best = null;
        float bestDistance = float.MaxValue;
        var dir = Vector3.Normalize(direction);

        foreach (var entry in EntriesOf(map, set))
        {
            for (int g = 0; g < entry.GateNodes.Count; g++)
            {
                var point = Vector3.Transform(entry.GateNodes[g].Position, mapWorld);
                float along = Vector3.Dot(point - origin, dir);

                if (along <= 0)
                    continue;

                float off = Vector3.Distance(point, origin + dir * along);
                if (off <= Math.Max(GateSize, along * 0.015f) && off < bestDistance)
                {
                    bestDistance = off;
                    best = (entry, g);
                }
            }
        }

        return best;
    }

    private static Vector3 Vertex(hkaiNavMesh mesh, int index)
    {
        var v = mesh.m_vertices[index];
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>The corners of a face, in order.</summary>
    public static List<Vector3> FaceCorners(hkaiNavMesh mesh, int face)
    {
        var f = mesh.m_faces[face];
        var corners = new List<Vector3>(f.m_numEdges);
        for (int k = f.m_startEdgeIndex; k < f.m_startEdgeIndex + f.m_numEdges; k++)
            corners.Add(Vertex(mesh, mesh.m_edges[k].m_a));
        return corners;
    }

    public static Vector3 FaceCentre(hkaiNavMesh mesh, int face)
    {
        var corners = FaceCorners(mesh, face);
        var sum = Vector3.Zero;
        foreach (var c in corners)
            sum += c;
        return corners.Count == 0 ? sum : sum / corners.Count;
    }

    /// <summary>
    /// The face a packed hkaiPackedKey points at in its own navmesh: the low 22 bits are the face, the high 10 the
    /// section. Null if it points nowhere, or at another section.
    /// </summary>
    public static int? OppositeFace(hkaiNavMesh mesh, uint packed)
    {
        if (packed == 0xFFFFFFFF)
            return null;

        int face = (int)(packed & 0x3FFFFF);
        uint section = packed >> 22;

        if ((section != 0 && section != 0x3FF) || face >= mesh.m_faces.Count)
            return null;

        return face;
    }

    /// <summary>The ends of every connection of a map, by navmesh NameID and edge, from this map's side: in the map, and to its neighbours.</summary>
    public static (HashSet<(int, int)> InMap, HashSet<(int, int)> CrossMap) ConnectionEnds(NavigationMap map)
    {
        var inMap = new HashSet<(int, int)>();
        var crossMap = new HashSet<(int, int)>();

        foreach (var view in NavigationRemap.Connections(map))
        {
            var target = view.IsCrossMap ? crossMap : inMap;
            bool sideA = !view.IsCrossMap || view.Nvc.MapA == map.MapId;
            bool sideB = !view.IsCrossMap || view.Nvc.MapB == map.MapId;

            if (sideA)
                target.Add((view.MainNameID, view.Connection.EdgeIndex));
            if (sideB)
                target.Add((view.TargetNameID, view.Connection.OppositeEdgeIndex));
        }

        return (inMap, crossMap);
    }

    /// <summary>
    /// The overlay of a single navmesh, drawn from the given mesh (e.g. one with edited vertices), with the connection
    /// ends of its map from <see cref="ConnectionEnds"/>. Only its faces, edges and user edges.
    /// </summary>
    public static NavOverlay BuildEntry(string mapId, NavmeshSet set, NVA.Navmesh entry, hkaiNavMesh mesh, Matrix4x4 world, ISet<NavOverlayLayer> layers,
        (HashSet<(int, int)> InMap, HashSet<(int, int)> CrossMap) ends)
    {
        var overlay = new NavOverlay { MapId = mapId, Set = set };

        if (mesh != null && mesh.m_faces.Count > 0)
            AddMesh(overlay, entry, mesh, world, layers, ends.InMap, ends.CrossMap);

        return overlay;
    }

    private static void AddMesh(NavOverlay overlay, NVA.Navmesh entry, hkaiNavMesh mesh, Matrix4x4 world, ISet<NavOverlayLayer> layers,
        HashSet<(int, int)> inMap, HashSet<(int, int)> crossMap)
    {
        var lift = new Vector3(0, Lift, 0);
        Vector3 W(Vector3 v) => Vector3.Transform(v, world) + lift;

        bool hasFaceData = mesh.m_faceDataStriding > 0 && mesh.m_faceData.Count >= mesh.m_faces.Count * mesh.m_faceDataStriding;

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];

            if (layers.Contains(NavOverlayLayer.Faces) && face.m_numEdges >= 3)
            {
                uint data = hasFaceData ? (uint)mesh.m_faceData[f * mesh.m_faceDataStriding] : 0;
                var batch = overlay.Batch(NavOverlayLayer.Faces, data);
                var corners = FaceCorners(mesh, f);
                var first = W(corners[0]);

                for (int i = 1; i + 1 < corners.Count; i++)
                    batch.Triangles.Add((first, W(corners[i]), W(corners[i + 1])));

                batch.FaceCount++;
            }

            for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            {
                var edge = mesh.m_edges[k];
                bool boundary = edge.m_oppositeFace == 0xFFFFFFFF;
                NavOverlayLayer layer;

                if (!boundary)
                {
                    // Each inner edge is shared by two faces; draw it once, from the lower face
                    if (!layers.Contains(NavOverlayLayer.InnerEdges) || OppositeFace(mesh, edge.m_oppositeFace) is int other && other < f)
                        continue;
                    layer = NavOverlayLayer.InnerEdges;
                }
                else if (crossMap.Contains((entry.NameID, k)))
                    layer = NavOverlayLayer.ConnectedCrossMap;
                else if (inMap.Contains((entry.NameID, k)))
                    layer = NavOverlayLayer.ConnectedInMap;
                else
                    layer = NavOverlayLayer.OpenEdges;

                if (layers.Contains(layer))
                    overlay.Batch(layer).Lines.Add((W(Vertex(mesh, edge.m_a)), W(Vertex(mesh, edge.m_b))));
            }

            // User edges: the edge, and an arc from its middle to the face it leads to
            if (face.m_numUserEdges > 0 && (layers.Contains(NavOverlayLayer.UserEdges) || layers.Contains(NavOverlayLayer.BlockedUserEdges)))
            {
                for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                {
                    var edge = mesh.m_edges[k];
                    bool blocked = ((int)edge.m_flags & (int)hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED) != 0;
                    var layer = blocked ? NavOverlayLayer.BlockedUserEdges : NavOverlayLayer.UserEdges;

                    if (!layers.Contains(layer))
                        continue;

                    var batch = overlay.Batch(layer);
                    var a = W(Vertex(mesh, edge.m_a));
                    var b = W(Vertex(mesh, edge.m_b));
                    batch.Lines.Add((a, b));

                    if (OppositeFace(mesh, edge.m_oppositeFace) is int target)
                        AddArc(batch, (a + b) / 2, W(FaceCentre(mesh, target)));
                    else
                        overlay.UserEdgesWithoutTarget++;
                }
            }
        }
    }

    private static void AddArc(NavOverlayBatch batch, Vector3 from, Vector3 to)
    {
        const int segments = 10;
        float height = 0.5f + Vector3.Distance(from, to) * 0.25f;
        var control = (from + to) / 2 + new Vector3(0, height, 0);
        var previous = from;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            var point = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * control + t * t * to;
            batch.Lines.Add((previous, point));
            previous = point;
        }

        // An arrow head at the target
        var direction = Vector3.Normalize(to - previous);
        var side = Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY)) * 0.15f;
        batch.Lines.Add((to, to - direction * 0.3f + side));
        batch.Lines.Add((to, to - direction * 0.3f - side));
    }

    /// <summary>
    /// A gate's marker: a diamond (an octahedron) whose faces the renderer fills semi-transparently in the layer's
    /// colour, with its edges drawn over the fill.
    /// </summary>
    private static void AddGateMarker(NavOverlayBatch batch, Vector3 at, float size)
    {
        var up = at + Vector3.UnitY * size;
        var down = at - Vector3.UnitY * size;
        Vector3[] ring =
        {
            at + Vector3.UnitX * size, at + Vector3.UnitZ * size,
            at - Vector3.UnitX * size, at - Vector3.UnitZ * size,
        };

        for (int i = 0; i < ring.Length; i++)
        {
            var a = ring[i];
            var b = ring[(i + 1) % ring.Length];

            batch.Triangles.Add((up, b, a));
            batch.Triangles.Add((down, a, b));

            batch.Lines.Add((a, b));
            batch.Lines.Add((a, up));
            batch.Lines.Add((a, down));
        }
    }

    private static void AddCross(NavOverlayBatch batch, Vector3 at, float size)
    {
        batch.Lines.Add((at - Vector3.UnitX * size, at + Vector3.UnitX * size));
        batch.Lines.Add((at - Vector3.UnitY * size, at + Vector3.UnitY * size));
        batch.Lines.Add((at - Vector3.UnitZ * size, at + Vector3.UnitZ * size));
    }

    private static void AddBox(NavOverlayBatch batch, Vector3 min, Vector3 max, Matrix4x4 world)
    {
        Vector3 C(int i) => Vector3.Transform(new Vector3((i & 1) != 0 ? max.X : min.X, (i & 2) != 0 ? max.Y : min.Y, (i & 4) != 0 ? max.Z : min.Z), world);

        foreach (var (i, j) in new[] { (0, 1), (2, 3), (4, 5), (6, 7), (0, 2), (1, 3), (4, 6), (5, 7), (0, 4), (1, 5), (2, 6), (3, 7) })
            batch.Lines.Add((C(i), C(j)));
    }

    private static void AddOnav(NavOverlay overlay, ONAV onav, Matrix4x4 world, ISet<NavOverlayLayer> layers)
    {
        var min = Vector3.Min(onav.BoundsMin, onav.BoundsMax);
        var max = Vector3.Max(onav.BoundsMin, onav.BoundsMax);
        var positions = new Dictionary<int, Vector3>();

        foreach (var area in onav.Areas)
            positions[area.ID] = area.Position;
        foreach (var area in onav.UnkAreas)
            positions.TryAdd(area.ID, area.Position);

        if (layers.Contains(NavOverlayLayer.OnavAreas))
        {
            var batch = overlay.Batch(NavOverlayLayer.OnavAreas);
            AddBox(batch, min, max, world);
            foreach (var position in positions.Values)
                AddBox(batch, position - new Vector3(0.5f), position + new Vector3(0.5f), world);
        }

        if (layers.Contains(NavOverlayLayer.OnavLinks))
        {
            var batch = overlay.Batch(NavOverlayLayer.OnavLinks);
            foreach (var area in onav.Areas)
                foreach (var link in area.Links)
                    if (positions.TryGetValue(link.AreaID, out var to))
                        batch.Lines.Add((Vector3.Transform(area.Position, world), Vector3.Transform(to, world)));
        }

        if (layers.Contains(NavOverlayLayer.OnavCells))
        {
            var batch = overlay.Batch(NavOverlayLayer.OnavCells);
            int nx = onav.GridCountX, ny = onav.GridCountY, nz = onav.GridCountZ;

            if (nx <= 0 || ny <= 0 || nz <= 0)
                return;

            var cell = (max - min) / new Vector3(nx, ny, nz);

            foreach (var area in onav.Areas.Cast<ONAV.AreaBase>().Concat(onav.UnkAreas))
            {
                for (int i = 0; i < nx * ny * nz && i / 8 < area.CellMask.Length; i++)
                {
                    if ((area.CellMask[i / 8] & (1 << (i % 8))) == 0)
                        continue;

                    int x = i % nx, y = i / nx % ny, z = i / (nx * ny);
                    var c0 = min + cell * new Vector3(x, y, z);
                    AddBox(batch, c0, c0 + cell, world);
                }
            }
        }
    }

    /// <summary>
    /// The outline of a face, with a cross at its centre, as an overlay of its own.
    /// </summary>
    public static NavOverlay Highlight(NavigationMap map, NavmeshSet set, NavFaceHit hit, Matrix4x4 world)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = set };
        var mesh = hit.File?.Mesh;

        if (mesh == null || hit.Face >= mesh.m_faces.Count)
            return overlay;

        var batch = overlay.Batch(NavOverlayLayer.Highlight);
        var lift = new Vector3(0, Lift * 2, 0);
        var corners = FaceCorners(mesh, hit.Face).Select(c => Vector3.Transform(c, world) + lift).ToList();

        for (int i = 0; i < corners.Count; i++)
            batch.Lines.Add((corners[i], corners[(i + 1) % corners.Count]));

        AddCross(batch, Vector3.Transform(FaceCentre(mesh, hit.Face), world) + lift, 0.25f);
        return overlay;
    }

    /// <summary>
    /// The nearest face of a set that a ray in viewport space hits, or null.
    /// </summary>
    public static NavFaceHit Pick(NavigationMap map, NavmeshSet set, Vector3 origin, Vector3 direction, Func<NVA.Navmesh, Matrix4x4> entryWorld)
    {
        NavFaceHit best = null;

        foreach (var entry in EntriesOf(map, set))
        {
            var file = map.FileOf(entry);
            var mesh = file?.Mesh;

            if (mesh == null || mesh.m_faces.Count == 0)
                continue;

            var world = entryWorld(entry);
            if (!Matrix4x4.Invert(world, out var inverse))
                continue;

            // Picked in the navmesh's own space, then measured back in the viewport's
            var localOrigin = Vector3.Transform(origin, inverse);
            var localDirection = Vector3.TransformNormal(direction, inverse);

            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                var corners = FaceCorners(mesh, f);

                for (int i = 1; i + 1 < corners.Count; i++)
                {
                    if (!RayTriangle(localOrigin, localDirection, corners[0], corners[i], corners[i + 1], out float t))
                        continue;

                    var point = Vector3.Transform(localOrigin + localDirection * t, world);
                    float distance = Vector3.Distance(origin, point);

                    if (best == null || distance < best.Distance)
                        best = new NavFaceHit(entry, file, f, distance, point);
                }
            }
        }

        return best;
    }

    /// <summary>Möller–Trumbore, both sides.</summary>
    internal static bool RayTriangle(Vector3 origin, Vector3 direction, Vector3 a, Vector3 b, Vector3 c, out float t)
    {
        t = 0;
        var e1 = b - a;
        var e2 = c - a;
        var p = Vector3.Cross(direction, e2);
        float det = Vector3.Dot(e1, p);

        if (MathF.Abs(det) < 1e-8f)
            return false;

        float inv = 1 / det;
        var s = origin - a;
        float u = Vector3.Dot(s, p) * inv;
        if (u < 0 || u > 1)
            return false;

        var q = Vector3.Cross(s, e1);
        float v = Vector3.Dot(direction, q) * inv;
        if (v < 0 || u + v > 1)
            return false;

        t = Vector3.Dot(e2, q) * inv;
        return t > 0;
    }
}
