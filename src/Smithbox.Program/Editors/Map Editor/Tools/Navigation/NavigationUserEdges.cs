using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Adding and removing user edges (jump, ladder and drop links) in an hkaiNavMesh, laid out as the game's files lay them
// out (Navigation Editor Plan, Phase 3; NavSurvey useredges, cachedump and linkedit check it). Kept free of the rest of
// Smithbox, so NavSurvey can run it on every map.
//
// How the game's files hold user edges:
// - after all the original edges; each face's user edges are one range (m_startUserEdgeIndex, -1 when it has none);
// - each with two vertices of its own, appended after the original vertices, inside the face;
// - as pairs: an edge on face A pointing at face B, and one on B pointing back at A, each naming the other in
//   m_oppositeEdge. Flags are 20 (ORIGINAL|USER) both ways, or 20 and 52 (|BLOCKED) for a one-way link. Both carry the
//   same edgeData;
// - in each clearance cache: one mcpData entry per edge, one vertexClearances byte per vertex (0xFF for user edge
//   vertices), and per face a block of T(k) + 1 bytes: the clearance of each pair of the face's k linked edges (those
//   with an opposite face; original edges first, then user edges, in order), as a lower triangle by column,
//   (1,0) (2,0) (2,1) (3,0) ..., then 0xFF. A pair's clearance is the smaller of its two edges' mcp clearances.

/// <summary>A new link: from a point on one face to a point on another face of the same navmesh, in the mesh's own space.</summary>
public sealed record NavUserEdgeLink(int FromFace, Vector3 FromPoint, int ToFace, Vector3 ToPoint, bool Bidirectional, int EdgeData, byte Clearance);

/// <summary>
/// The whole editable state of a navmesh file, copied, so an edit that renumbers edges can be undone exactly.
/// </summary>
public sealed class NavMeshState
{
    private readonly (int Start, int StartUser, short Num, short NumUser, short Cluster, ushort Padding)[] Faces;
    private readonly (int A, int B, uint OppositeEdge, uint OppositeFace, HavokNavEdgeFlags Flags, float Cost)[] Edges;
    private readonly Vector4[] Vertices;
    private readonly int[] FaceData;
    private readonly int[] EdgeData;
    private readonly Vector4 AabbMin, AabbMax;
    private readonly List<(IHavokNavClearanceCache Cache, uint[] FaceOffsets, byte[] Pairs, (byte, byte)[] Mcp, byte[] VertexClearances)> Caches = new();

    public NavMeshState(IHavokNavMeshEdit mesh, IEnumerable<IHavokNavClearanceCache> caches)
    {
        Faces = mesh.m_faces.Select(f => (f.m_startEdgeIndex, f.m_startUserEdgeIndex, f.m_numEdges, f.m_numUserEdges, f.m_clusterIndex, f.m_padding)).ToArray();
        Edges = mesh.m_edges.Select(e => (e.m_a, e.m_b, e.m_oppositeEdge, e.m_oppositeFace, e.m_flags, e.m_userEdgeCost)).ToArray();
        Vertices = mesh.m_vertices.ToArray();
        FaceData = mesh.m_faceData.ToArray();
        EdgeData = mesh.m_edgeData.ToArray();
        AabbMin = mesh.m_aabb.m_min;
        AabbMax = mesh.m_aabb.m_max;

        foreach (var c in caches)
            Caches.Add((c, c.m_faceOffsets.ToArray(), c.m_edgePairClearances.ToArray(), c.m_mcpData.Select(m => (m.m_interpolant, m.m_clearance)).ToArray(), c.m_vertexClearances.ToArray()));
    }

    /// <summary>Puts the mesh back as it was when copied.</summary>
    public void Restore(IHavokNavMeshEdit mesh)
    {
        mesh.m_faces.Clear();
        foreach (var f in Faces)
            mesh.m_faces.Add(mesh.NewFace(f.Start, f.Num, f.StartUser, f.NumUser, f.Cluster, f.Padding));

        mesh.m_edges.Clear();
        foreach (var e in Edges)
            mesh.m_edges.Add(mesh.NewEdge(e.A, e.B, e.OppositeEdge, e.OppositeFace, e.Flags, e.Cost));

        mesh.m_vertices.Clear();
        mesh.m_vertices.AddRange(Vertices);
        mesh.m_faceData.Clear();
        mesh.m_faceData.AddRange(FaceData);
        mesh.m_edgeData.Clear();
        mesh.m_edgeData.AddRange(EdgeData);
        mesh.m_aabb.m_min = AabbMin;
        mesh.m_aabb.m_max = AabbMax;

        foreach (var (cache, offsets, pairs, mcp, vertexClearances) in Caches)
        {
            cache.m_faceOffsets.Clear();
            cache.m_faceOffsets.AddRange(offsets);
            cache.m_edgePairClearances.Clear();
            cache.m_edgePairClearances.AddRange(pairs);
            cache.m_mcpData.Clear();
            cache.m_mcpData.AddRange(mcp.Select(m => cache.NewMcp(m.Item1, m.Item2)));
            cache.m_vertexClearances.Clear();
            cache.m_vertexClearances.AddRange(vertexClearances);
        }
    }
}

/// <summary>What an add or remove did, or why it didn't.</summary>
public sealed class NavUserEdgeResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    /// <summary>The edges the link now has (add), in the order forward, back.</summary>
    public List<int> Edges { get; } = new();

    /// <summary>The renumbering of the mesh's edges: the new index of an old one, -1 if removed.</summary>
    public Func<int, int> EdgeMap { get; set; } = e => e;

    public bool Ok => Error == null;
}

public static class NavUserEdges
{
    /// <summary>The clearance a new link's edges get when none is chosen: the highest a cache can hold below its 0xFF marker.</summary>
    public const byte DefaultClearance = 0xFE;

    /// <summary>The edge data a new link gets when none is chosen: 0x2, which the game's cost reads as a jump.</summary>
    public const int DefaultEdgeData = 0x2;

    /// <summary>Half the width of a new user edge, in metres. The game's are short segments inside their face.</summary>
    public const float HalfWidth = 0.25f;

    private const uint None = 0xFFFFFFFF;

    private static int Triangle(int k) => k * (k - 1) / 2;

    /// <summary>
    /// The clearance caches of a mesh: the initial caches of its clearance cache seeding (two in most files, one per
    /// seeding entry, mostly identical). Files without user edges have them too.
    /// </summary>
    public static List<IHavokNavClearanceCache> CachesOf(IHavokNavMeshEdit mesh) => mesh.ClearanceCaches.ToList();

    /// <summary>
    /// Whether any connection of the map names an edge of a navmesh at or after an index. Adding or removing a user edge
    /// only moves edges after the original ones, which connections never name (they join boundary edges), so the edit
    /// needs no remapping. It is refused if one does.
    /// </summary>
    public static bool ConnectionsNameEdgesFrom(NavigationMap map, int nameId, int firstEdge)
    {
        foreach (var view in NavigationRemap.Connections(map))
        {
            bool sideA = !view.IsCrossMap || view.Nvc.MapA == map.MapId;
            bool sideB = !view.IsCrossMap || view.Nvc.MapB == map.MapId;

            if (sideA && view.MainNameID == nameId && view.Connection.EdgeIndex >= firstEdge)
                return true;
            if (sideB && view.TargetNameID == nameId && view.Connection.OppositeEdgeIndex >= firstEdge)
                return true;
        }
        return false;
    }

    /// <summary>The number of original edges: the user edges come after them.</summary>
    public static int OriginalEdgeCount(IHavokNavMeshEdit mesh) => mesh.m_faces.Sum(f => f.m_numEdges);

    /// <summary>The edges of a face that the clearance cache gives pairs to: those with an opposite face, original edges first.</summary>
    public static List<int> LinkedEdges(IHavokNavMeshEdit mesh, int face)
    {
        var f = mesh.m_faces[face];
        var linked = new List<int>();

        for (int e = f.m_startEdgeIndex; e < f.m_startEdgeIndex + f.m_numEdges; e++)
            if (mesh.m_edges[e].m_oppositeFace != None)
                linked.Add(e);

        for (int e = f.m_startUserEdgeIndex; f.m_numUserEdges > 0 && e < f.m_startUserEdgeIndex + f.m_numUserEdges; e++)
            if (mesh.m_edges[e].m_oppositeFace != None)
                linked.Add(e);

        return linked;
    }

    /// <summary>The size of a face's block of pair clearances, or -1 if its offsets don't give one.</summary>
    private static int BlockSize(IHavokNavClearanceCache cache, int face)
    {
        if (face >= cache.m_faceOffsets.Count)
            return -1;

        long start = cache.m_faceOffsets[face];
        long end = face + 1 < cache.m_faceOffsets.Count ? cache.m_faceOffsets[face + 1] : FindBlockEnd(cache, face);

        return start == None || end < start ? -1 : (int)(end - start);
    }

    /// <summary>The last face's block ends at its 0xFF marker: the arrays are sometimes longer than the blocks.</summary>
    private static long FindBlockEnd(IHavokNavClearanceCache cache, int face)
    {
        int start = (int)cache.m_faceOffsets[face];
        int marker = cache.m_edgePairClearances.IndexOf(0xFF, start);
        return marker < 0 ? cache.m_edgePairClearances.Count : marker + 1;
    }

    /// <summary>A face's block is the size its linked edges give, in every cache.</summary>
    private static bool BlocksMatch(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, int face)
    {
        int expected = Triangle(LinkedEdges(mesh, face).Count) + 1;
        return caches.All(c => BlockSize(c, face) == expected);
    }

    private static byte Mcp(IHavokNavClearanceCache cache, int edge) => edge < cache.m_mcpData.Count ? cache.m_mcpData[edge].m_clearance : (byte)0;

    /// <summary>
    /// Adds a link from one face to another of the same mesh: a user edge on each, pointing at each other. The back edge
    /// is blocked unless the link goes both ways. Edges after the insertion points move up, so the result's map must be
    /// passed on to the map's connections.
    /// </summary>
    public static NavUserEdgeResult Add(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, NavUserEdgeLink link)
    {
        var result = new NavUserEdgeResult();

        if (link.FromFace == link.ToFace)
        {
            result.Error = "MAP_Navigation_Link_Error_Same_Face";
            return result;
        }

        if (link.FromFace < 0 || link.ToFace < 0 || link.FromFace >= mesh.m_faces.Count || link.ToFace >= mesh.m_faces.Count)
        {
            result.Error = "MAP_Navigation_Link_Error_Face";
            return result;
        }

        if (!BlocksMatch(mesh, caches, link.FromFace) || !BlocksMatch(mesh, caches, link.ToFace))
        {
            result.Error = "MAP_Navigation_Link_Error_Cache";
            return result;
        }

        var across = Across(link);
        int fromVertexA = AddVertex(mesh, caches, link.FromPoint - across);
        int fromVertexB = AddVertex(mesh, caches, link.FromPoint + across);
        int toVertexA = AddVertex(mesh, caches, link.ToPoint + across);
        int toVertexB = AddVertex(mesh, caches, link.ToPoint - across);

        var (userFlags, backFlags) = Flags(link);

        var forward = mesh.NewEdge(fromVertexA, fromVertexB, None, (uint)link.ToFace, userFlags);
        var back = mesh.NewEdge(toVertexA, toVertexB, None, (uint)link.FromFace, backFlags);

        int forwardIndex = Insert(mesh, caches, link.FromFace, forward, link.EdgeData, link.Clearance);
        int backIndex = Insert(mesh, caches, link.ToFace, back, link.EdgeData, link.Clearance);
        if (backIndex <= forwardIndex)
            forwardIndex++;

        mesh.m_edges[forwardIndex].m_oppositeEdge = (uint)backIndex;
        mesh.m_edges[backIndex].m_oppositeEdge = (uint)forwardIndex;

        result.Edges.Add(forwardIndex);
        result.Edges.Add(backIndex);

        // The old indices at or after each insertion point moved up by one per insertion before them
        int first = Math.Min(forwardIndex, backIndex);
        int second = Math.Max(forwardIndex, backIndex);
        result.EdgeMap = e => e < first ? e : e + 1 < second ? e + 1 : e + 2;
        return result;
    }

    /// <summary>Half a new user edge: a short segment across the direction of the link, the same at each end.</summary>
    private static Vector3 Across(NavUserEdgeLink link)
    {
        var direction = link.ToPoint - link.FromPoint;
        direction.Y = 0;
        var across = direction.LengthSquared() > 1e-6f ? Vector3.Normalize(new Vector3(-direction.Z, 0, direction.X)) : Vector3.UnitX;
        return across * HalfWidth;
    }

    /// <summary>The flags of a link's forward and back edges: the back one is blocked unless the link goes both ways.</summary>
    private static (HavokNavEdgeFlags Forward, HavokNavEdgeFlags Back) Flags(NavUserEdgeLink link)
    {
        var userFlags = HavokNavEdgeFlags.EDGE_ORIGINAL | HavokNavEdgeFlags.EDGE_USER;
        return (userFlags, link.Bidirectional ? userFlags : userFlags | HavokNavEdgeFlags.EDGE_BLOCKED);
    }

    /// <summary>
    /// Adds many links at once to a mesh that has no user edges and no clearance caches (a generated one), as Add would
    /// one by one, in one pass: the user edges after the original ones, packed in face order, each face's in the order
    /// of the links, with their own vertices after the original ones. Refused if the mesh has user edges or caches.
    /// </summary>
    public static NavUserEdgeResult AddAll(IHavokNavMeshEdit mesh, IReadOnlyList<NavUserEdgeLink> links)
    {
        var result = new NavUserEdgeResult();

        if (mesh.m_faces.Any(f => f.m_numUserEdges > 0) || CachesOf(mesh).Count > 0 || OriginalEdgeCount(mesh) != mesh.m_edges.Count)
        {
            result.Error = "MAP_Navigation_Link_Error_Cache";
            return result;
        }

        foreach (var link in links)
        {
            if (link.FromFace == link.ToFace)
            {
                result.Error = "MAP_Navigation_Link_Error_Same_Face";
                return result;
            }
            if (link.FromFace < 0 || link.ToFace < 0 || link.FromFace >= mesh.m_faces.Count || link.ToFace >= mesh.m_faces.Count)
            {
                result.Error = "MAP_Navigation_Link_Error_Face";
                return result;
            }
        }

        // Each face's user edges: (link, forward or back), in link order
        var byFace = new List<(int Link, bool Forward)>[mesh.m_faces.Count];
        for (int i = 0; i < links.Count; i++)
        {
            (byFace[links[i].FromFace] ??= new()).Add((i, true));
            (byFace[links[i].ToFace] ??= new()).Add((i, false));
        }

        var forwardEdge = new int[links.Count];
        var backEdge = new int[links.Count];
        bool striding = mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count == mesh.m_edges.Count * mesh.m_edgeDataStriding;

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            if (byFace[f] == null)
                continue;

            var face = mesh.m_faces[f];
            face.m_startUserEdgeIndex = mesh.m_edges.Count;
            face.m_numUserEdges = (short)byFace[f].Count;

            foreach (var (i, forward) in byFace[f])
            {
                var link = links[i];
                var across = Across(link);
                var (forwardFlags, backFlags) = Flags(link);

                int index = mesh.m_edges.Count;
                int va = forward ? AddVertex(mesh, Array.Empty<IHavokNavClearanceCache>(), link.FromPoint - across) : AddVertex(mesh, Array.Empty<IHavokNavClearanceCache>(), link.ToPoint + across);
                int vb = forward ? AddVertex(mesh, Array.Empty<IHavokNavClearanceCache>(), link.FromPoint + across) : AddVertex(mesh, Array.Empty<IHavokNavClearanceCache>(), link.ToPoint - across);

                mesh.m_edges.Add(mesh.NewEdge(va, vb, None, (uint)(forward ? link.ToFace : link.FromFace), forward ? forwardFlags : backFlags));

                if (striding)
                {
                    mesh.m_edgeData.Add(link.EdgeData);
                    for (int k = 1; k < mesh.m_edgeDataStriding; k++)
                        mesh.m_edgeData.Add(0);
                }

                if (forward) forwardEdge[i] = index;
                else backEdge[i] = index;
                result.Edges.Add(index);
            }
        }

        for (int i = 0; i < links.Count; i++)
        {
            mesh.m_edges[forwardEdge[i]].m_oppositeEdge = (uint)backEdge[i];
            mesh.m_edges[backEdge[i]].m_oppositeEdge = (uint)forwardEdge[i];
        }

        return result;
    }

    /// <summary>
    /// Removes a user edge and the edge it is paired with, their vertices if nothing else uses them, and their clearance
    /// cache entries. Edges after them move down, so the result's map must be passed on to the map's connections.
    /// </summary>
    public static NavUserEdgeResult Remove(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, int edge)
    {
        var result = new NavUserEdgeResult();
        int owner = OwnerOfUserEdge(mesh, edge);

        if (owner < 0)
        {
            result.Error = "MAP_Navigation_Link_Error_Not_User_Edge";
            return result;
        }

        var edges = new List<(int Edge, int Face)> { (edge, owner) };
        var opposite = mesh.m_edges[edge].m_oppositeEdge;

        if (opposite != None && (opposite >> 22) == 0)
        {
            int other = (int)opposite;
            int otherOwner = OwnerOfUserEdge(mesh, other);
            if (otherOwner >= 0 && other != edge && (mesh.m_edges[other].m_oppositeEdge & 0x3FFFFF) == edge)
                edges.Add((other, otherOwner));
        }

        foreach (var (_, face) in edges)
        {
            if (!BlocksMatch(mesh, caches, face))
            {
                result.Error = "MAP_Navigation_Link_Error_Cache";
                return result;
            }
        }

        var removed = edges.Select(e => e.Edge).OrderBy(e => e).ToArray();
        var vertices = new HashSet<int>();

        // Highest first, so the lower one keeps its index
        foreach (var (e, face) in edges.OrderByDescending(x => x.Edge))
        {
            vertices.Add(mesh.m_edges[e].m_a);
            vertices.Add(mesh.m_edges[e].m_b);
            RemoveEdge(mesh, caches, face, e);
        }

        // Vertices nothing uses any more, highest first
        var used = new HashSet<int>();
        foreach (var e in mesh.m_edges)
        {
            used.Add(e.m_a);
            used.Add(e.m_b);
        }

        foreach (var v in vertices.Where(v => !used.Contains(v)).OrderByDescending(v => v))
            RemoveVertex(mesh, caches, v);

        result.EdgeMap = e =>
        {
            if (Array.IndexOf(removed, e) >= 0)
                return -1;
            int below = 0;
            foreach (var r in removed)
                if (r < e)
                    below++;
            return e - below;
        };
        return result;
    }

    /// <summary>The face whose user edge range holds an edge, or -1.</summary>
    public static int OwnerOfUserEdge(IHavokNavMeshEdit mesh, int edge)
    {
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            if (face.m_numUserEdges > 0 && edge >= face.m_startUserEdgeIndex && edge < face.m_startUserEdgeIndex + face.m_numUserEdges)
                return f;
        }
        return -1;
    }

    private static int AddVertex(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, Vector3 at)
    {
        mesh.m_vertices.Add(new Vector4(at, 1));

        // The game's user edge vertices are inside their face, so this only grows the bounds for a point at an edge
        var p = new Vector4(at, 1);
        mesh.m_aabb.m_min = Vector4.Min(mesh.m_aabb.m_min, p) with { W = mesh.m_aabb.m_min.W };
        mesh.m_aabb.m_max = Vector4.Max(mesh.m_aabb.m_max, p) with { W = mesh.m_aabb.m_max.W };

        foreach (var cache in caches)
            if (cache.m_vertexClearances.Count == mesh.m_vertices.Count - 1)
                cache.m_vertexClearances.Add(0xFF);

        return mesh.m_vertices.Count - 1;
    }

    private static void RemoveVertex(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, int vertex)
    {
        mesh.m_vertices.RemoveAt(vertex);

        foreach (var cache in caches)
            if (vertex < cache.m_vertexClearances.Count)
                cache.m_vertexClearances.RemoveAt(vertex);

        foreach (var e in mesh.m_edges)
        {
            if (e.m_a > vertex) e.m_a--;
            if (e.m_b > vertex) e.m_b--;
        }
    }

    /// <summary>Moves every reference to an edge at or after a point by a step, keeping its section bits.</summary>
    private static void ShiftEdgeReferences(IHavokNavMeshEdit mesh, int from, int step)
    {
        foreach (var f in mesh.m_faces)
        {
            if (f.m_startEdgeIndex >= from)
                f.m_startEdgeIndex += step;
            if (f.m_numUserEdges > 0 && f.m_startUserEdgeIndex >= from)
                f.m_startUserEdgeIndex += step;
        }

        foreach (var e in mesh.m_edges)
        {
            if (e.m_oppositeEdge == None || (e.m_oppositeEdge >> 22) != 0)
                continue;
            if ((int)e.m_oppositeEdge >= from)
                e.m_oppositeEdge = (uint)((int)e.m_oppositeEdge + step);
        }
    }

    /// <summary>Inserts a user edge at the end of a face's user edge range, and its cache entries. Returns its index.</summary>
    private static int Insert(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, int face, IHavokNavEdge edge, int edgeData, byte clearance)
    {
        var f = mesh.m_faces[face];

        // The pairs the new edge makes with the face's linked edges, read before anything moves
        var linked = LinkedEdges(mesh, face);
        var pairValues = caches.Select(c => linked.Select(e => Math.Min(Mcp(c, e), clearance)).Select(v => (byte)v).ToArray()).ToList();

        int index = f.m_numUserEdges > 0 ? f.m_startUserEdgeIndex + f.m_numUserEdges : mesh.m_edges.Count;

        ShiftEdgeReferences(mesh, index, 1);
        mesh.m_edges.Insert(index, edge);

        if (f.m_numUserEdges == 0)
            f.m_startUserEdgeIndex = index;
        f.m_numUserEdges++;

        if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count >= index * mesh.m_edgeDataStriding)
        {
            mesh.m_edgeData.Insert(index * mesh.m_edgeDataStriding, edgeData);
            for (int i = 1; i < mesh.m_edgeDataStriding; i++)
                mesh.m_edgeData.Insert(index * mesh.m_edgeDataStriding + i, 0);
        }

        for (int c = 0; c < caches.Count; c++)
        {
            var cache = caches[c];

            if (cache.m_mcpData.Count >= index)
                cache.m_mcpData.Insert(index, cache.NewMcp(0, clearance));

            // The new edge is the face's last linked edge, so its pairs are the block's last column, before the marker
            int start = (int)cache.m_faceOffsets[face];
            int at = start + Triangle(linked.Count);
            cache.m_edgePairClearances.InsertRange(at, pairValues[c]);

            for (int g = face + 1; g < cache.m_faceOffsets.Count; g++)
                if (cache.m_faceOffsets[g] != None)
                    cache.m_faceOffsets[g] += (uint)pairValues[c].Length;
        }

        return index;
    }

    /// <summary>Removes a user edge of a face, and its cache entries.</summary>
    private static void RemoveEdge(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches, int face, int edge)
    {
        var f = mesh.m_faces[face];
        var linked = LinkedEdges(mesh, face);
        int position = linked.IndexOf(edge);

        foreach (var cache in caches)
        {
            if (position >= 0)
            {
                // Rebuild the face's triangle without the edge's row and column
                int k = linked.Count;
                int start = (int)cache.m_faceOffsets[face];
                var kept = new List<byte>();

                for (int j = 1; j < k; j++)
                    for (int i = 0; i < j; i++)
                        if (i != position && j != position)
                            kept.Add(cache.m_edgePairClearances[start + Triangle(j) + i]);

                int oldSize = Triangle(k);
                cache.m_edgePairClearances.RemoveRange(start, oldSize);
                cache.m_edgePairClearances.InsertRange(start, kept);

                int shrink = oldSize - kept.Count;
                for (int g = face + 1; g < cache.m_faceOffsets.Count; g++)
                    if (cache.m_faceOffsets[g] != None)
                        cache.m_faceOffsets[g] -= (uint)shrink;
            }

            if (edge < cache.m_mcpData.Count)
                cache.m_mcpData.RemoveAt(edge);
        }

        // Whatever pointed at it points nowhere now
        foreach (var e in mesh.m_edges)
            if (e.m_oppositeEdge != None && (e.m_oppositeEdge >> 22) == 0 && e.m_oppositeEdge == edge)
                e.m_oppositeEdge = None;

        mesh.m_edges.RemoveAt(edge);

        if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count >= (edge + 1) * mesh.m_edgeDataStriding)
            mesh.m_edgeData.RemoveRange(edge * mesh.m_edgeDataStriding, mesh.m_edgeDataStriding);

        f.m_numUserEdges--;
        ShiftEdgeReferences(mesh, edge + 1, -1);

        if (f.m_numUserEdges == 0)
            f.m_startUserEdgeIndex = -1;
    }

    /// <summary>
    /// The mesh holds its user edges as the game's files do. Returns a description of the first thing that doesn't hold,
    /// or null. Used by NavSurvey after its edits.
    /// </summary>
    public static string Check(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches)
    {
        int original = mesh.m_faces.Sum(f => f.m_numEdges);
        int user = mesh.m_faces.Sum(f => f.m_numUserEdges);

        if (original + user != mesh.m_edges.Count)
            return $"edges {mesh.m_edges.Count} != {original} original + {user} user";

        var owners = new int[mesh.m_edges.Count];
        Array.Fill(owners, -1);

        for (int fi = 0; fi < mesh.m_faces.Count; fi++)
        {
            var f = mesh.m_faces[fi];
            if (f.m_numUserEdges == 0)
            {
                if (f.m_startUserEdgeIndex != -1)
                    return $"face {fi} has no user edges but starts them at {f.m_startUserEdgeIndex}";
                continue;
            }

            for (int e = f.m_startUserEdgeIndex; e < f.m_startUserEdgeIndex + f.m_numUserEdges; e++)
            {
                if (e < original || e >= mesh.m_edges.Count)
                    return $"face {fi} user edge {e} out of the user range";
                if (owners[e] >= 0)
                    return $"user edge {e} in faces {owners[e]} and {fi}";
                owners[e] = fi;
            }
        }

        for (int e = original; e < mesh.m_edges.Count; e++)
        {
            var edge = mesh.m_edges[e];
            if (owners[e] < 0)
                return $"user edge {e} in no face";
            if (edge.m_a >= mesh.m_vertices.Count || edge.m_b >= mesh.m_vertices.Count)
                return $"user edge {e} vertex out of range";
            if (edge.m_oppositeEdge == None || (edge.m_oppositeEdge >> 22) != 0)
                continue;

            int other = (int)edge.m_oppositeEdge;
            if (other >= mesh.m_edges.Count || other < original)
                return $"user edge {e} pairs with {other}, not a user edge";
            if (mesh.m_edges[other].m_oppositeEdge != e)
                return $"user edge {e} pairs with {other}, which doesn't point back";
            if (owners[other] != (int)(edge.m_oppositeFace & 0x3FFFFF))
                return $"user edge {e} targets face {edge.m_oppositeFace & 0x3FFFFF} but its pair is on face {owners[other]}";
        }

        if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count != mesh.m_edges.Count * mesh.m_edgeDataStriding)
            return $"edgeData {mesh.m_edgeData.Count} for {mesh.m_edges.Count} edges";

        foreach (var cache in caches)
        {
            if (cache.m_mcpData.Count != mesh.m_edges.Count)
                return $"mcpData {cache.m_mcpData.Count} for {mesh.m_edges.Count} edges";
            if (cache.m_vertexClearances.Count != mesh.m_vertices.Count)
                return $"vertexClearances {cache.m_vertexClearances.Count} for {mesh.m_vertices.Count} vertices";
            if (cache.m_faceOffsets.Count != mesh.m_faces.Count)
                return $"faceOffsets {cache.m_faceOffsets.Count} for {mesh.m_faces.Count} faces";
        }

        return null;
    }

    /// <summary>The faces whose cache block isn't the size their linked edges give; vanilla files have a few.</summary>
    public static int MismatchedBlocks(IHavokNavMeshEdit mesh, IReadOnlyList<IHavokNavClearanceCache> caches)
    {
        int count = 0;
        for (int f = 0; f < mesh.m_faces.Count; f++)
            if (!BlocksMatch(mesh, caches, f))
                count++;
        return count;
    }
}
