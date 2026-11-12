using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Adding a triangle to a navmesh (the Navigation tool's Edit tab, Add Face). Kept free of the rest of Smithbox, so
// NavSurvey can link it (faceadd).
//
// hkaiNavMesh (every Havok game): the new face goes at the end of m_faces, so no face is renumbered. Its three edges go
// after the last original edge, before the user edges (the game's files keep user edges last), so the user edges move up
// by three: every reference to them is shifted, and so must be any connection that names one (rare; the result's
// EdgeMap). An edge the face shares with an existing face's open edge is joined both ways. The clearance caches get an
// mcpData entry per new edge, a vertexClearances byte per new vertex, the joined faces' pair blocks grow, the new face's
// block is added after the last, and the clearances around the face are worked out again (NavClearance). The face trees
// are built again, and the cost graph (DS3, BB, SDT) gives the face the cluster of the face it joins, or is built again.
//
// NVM (DES, DS1, DS1R): the triangle goes at the end, its neighbours across shared edges are set both ways, and the box
// tree is built again the game's way. Nothing is renumbered, so the MCG and MCP only need the room box.

/// <summary>
/// A corner of a new face, in the navmesh's own space: an existing vertex of the mesh (<see cref="Vertex"/> 0 or more,
/// whose position the mesh gives), or a new vertex at <see cref="Point"/>.
/// </summary>
public readonly record struct NavFaceCorner(int Vertex, Vector3 Point)
{
    public static NavFaceCorner Existing(int vertex, Vector3 point) => new(vertex, point);

    public static NavFaceCorner New(Vector3 point) => new(-1, point);

    public bool IsNew => Vertex < 0;
}

/// <summary>What adding a face did, or why it didn't.</summary>
public sealed class NavFaceEditResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    /// <summary>The file's new document (Havok).</summary>
    public Havok.HavokDocument Document { get; set; }

    /// <summary>The new NVM (NVM mode).</summary>
    public NVM Nvm { get; set; }

    /// <summary>The index of the new face (or triangle).</summary>
    public int Face { get; set; } = -1;

    /// <summary>How many of its edges were joined to existing faces.</summary>
    public int Joined { get; set; }

    /// <summary>How many new vertices it made.</summary>
    public int NewVertices { get; set; }

    /// <summary>The face whose face data (or flags) the new face took; -1 when it joins none.</summary>
    public int DataFrom { get; set; } = -1;

    /// <summary>The cost graph was built again, as the face joined more than one cluster or none (DS3, BB, SDT).</summary>
    public bool GraphRebuilt { get; set; }

    /// <summary>What the clearance recalculation changed (Havok).</summary>
    public NavClearanceResult Clearance { get; set; } = new();

    /// <summary>The first edge the new face's edges took; the old edges from there on moved up by three (Havok).</summary>
    public int FirstEdge { get; set; } = -1;

    /// <summary>The renumbering of the mesh's edges: the new index of an old one, -1 for a removed one.</summary>
    public Func<int, int> EdgeMap { get; set; } = e => e;

    /// <summary>The renumbering of the mesh's faces (a removal): the new index of an old one, -1 for the removed one.</summary>
    public Func<int, int> FaceMap { get; set; } = f => f;

    /// <summary>The new MCG of a triangle removal (NVM), or the one given when no edge named the triangles that moved.</summary>
    public MCG Mcg { get; set; }

    /// <summary>The links (user edge pairs) a removal took with the face (Havok).</summary>
    public int LinksRemoved { get; set; }

    /// <summary>The vertices a removal dropped, as no other face used them.</summary>
    public int VerticesRemoved { get; set; }

    /// <summary>The edges of other faces a removal left open.</summary>
    public int Opened { get; set; }

    public bool Ok => Error == null;
}

public static class NavFaceAdd
{
    private const uint None = 0xFFFFFFFF;

    /// <summary>The smallest area a new face may have seen from above, in square metres: one that can be walked on.</summary>
    public const float MinArea = 1e-3f;

    private static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    /// <summary>
    /// The vertices a new face may use: those of the faces' own (original) edges. User edges have vertices of their own
    /// inside their face, which a face corner must not take.
    /// </summary>
    public static HashSet<int> FaceVertices(IHavokNavMeshEdit mesh)
    {
        var vertices = new HashSet<int>();

        foreach (var f in mesh.m_faces)
        {
            for (int k = f.m_startEdgeIndex; k < f.m_startEdgeIndex + f.m_numEdges && k < mesh.m_edges.Count; k++)
                vertices.Add(mesh.m_edges[k].m_a);
        }

        return vertices;
    }

    /// <summary>
    /// The sum the faces' winding is read from, seen from above: the game's faces (and the generator's) wind with a
    /// negative one (counter-clockwise, as the generator's TurnXZ has it).
    /// </summary>
    public static double WindingSum(Vector3 a, Vector3 b, Vector3 c)
    {
        double Cross(Vector3 p, Vector3 q) => (double)p.X * q.Z - (double)q.X * p.Z;
        return Cross(a, b) + Cross(b, c) + Cross(c, a);
    }

    /// <summary>
    /// A copy of a navmesh file with a triangle added. <paramref name="edgeConnected"/> says whether an edge is named by a
    /// connection to another navmesh (NVA or NVC): such an edge isn't joined to the new face, as it would no longer be on
    /// the boundary. The face takes the face data of the first face it joins, or zero.
    /// </summary>
    public static NavFaceEditResult Build(Havok.HavokDocument document, IReadOnlyList<NavFaceCorner> corners, Func<int, bool> edgeConnected = null)
    {
        var result = new NavFaceEditResult();

        if (corners == null || corners.Count != 3)
            return Fail(result, "MAP_Navigation_Face_Error_Corners");

        var copy = NavModelEdit.Clone(document);
        var mesh = copy?.ObjectsOf<IHavokNavMeshEdit>().FirstOrDefault();

        if (mesh == null)
            return Fail(result, "MAP_Navigation_Face_Error_File");

        int original = NavUserEdges.OriginalEdgeCount(mesh);

        if (original > mesh.m_edges.Count)
            return Fail(result, "MAP_Navigation_Face_Error_File");

        // The corners' positions, from the mesh for the existing ones
        var faceVertices = FaceVertices(mesh);
        var points = new Vector3[3];

        for (int k = 0; k < 3; k++)
        {
            var c = corners[k];

            if (!c.IsNew && (c.Vertex >= mesh.m_vertices.Count || !faceVertices.Contains(c.Vertex)))
                return Fail(result, "MAP_Navigation_Face_Error_Vertex");

            points[k] = c.IsNew ? c.Point : Xyz(mesh.m_vertices[c.Vertex]);
        }

        if (Degenerate(corners, points))
            return Fail(result, "MAP_Navigation_Face_Error_Degenerate");

        // Wound as the game's faces are
        var order = WindingSum(points[0], points[1], points[2]) < 0 ? new[] { 0, 1, 2 } : new[] { 0, 2, 1 };

        // The original edges by their two vertices
        var byVertices = new Dictionary<(int, int), int>();
        var faceOfEdge = new Dictionary<int, int>();

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            {
                byVertices.TryAdd((mesh.m_edges[k].m_a, mesh.m_edges[k].m_b), k);
                faceOfEdge[k] = f;
            }
        }

        // The existing open edge each new edge joins (it runs the other way), or -1
        var joins = new int[3];

        for (int k = 0; k < 3; k++)
        {
            joins[k] = -1;
            var a = corners[order[k]];
            var b = corners[order[(k + 1) % 3]];

            if (a.IsNew || b.IsNew)
                continue;

            // An edge the same way round is an existing face on the same side: the new one would overlap it
            if (byVertices.ContainsKey((a.Vertex, b.Vertex)))
                return Fail(result, "MAP_Navigation_Face_Error_Overlap");

            if (!byVertices.TryGetValue((b.Vertex, a.Vertex), out int edge))
                continue;

            if (mesh.m_edges[edge].m_oppositeFace != None)
                return Fail(result, "MAP_Navigation_Face_Error_Edge_Taken");

            if (edgeConnected?.Invoke(edge) == true)
                return Fail(result, "MAP_Navigation_Face_Error_Edge_Connected");

            joins[k] = edge;
        }

        var neighbours = joins.Where(e => e >= 0).Select(e => faceOfEdge[e]).Distinct().ToList();

        // The caches must be laid out as the mesh is, at least where the face changes them
        var caches = NavUserEdges.CachesOf(mesh);

        foreach (var cache in caches)
        {
            if (cache.m_mcpData.Count != mesh.m_edges.Count || cache.m_vertexClearances.Count != mesh.m_vertices.Count
                || cache.m_faceOffsets.Count != mesh.m_faces.Count)
                return Fail(result, "MAP_Navigation_Face_Error_Cache");
        }

        foreach (var f in neighbours)
        {
            if (!NavUserEdges.BlocksMatch(mesh, caches, f))
                return Fail(result, "MAP_Navigation_Face_Error_Cache");
        }

        int oldFaceCount = mesh.m_faces.Count;
        int newFace = oldFaceCount;
        int dataFrom = neighbours.Count > 0 ? neighbours[0] : -1;
        float w = mesh.m_vertices.Count > 0 ? mesh.m_vertices[0].W : 1f;

        // Vertices: the new ones after the last, with a clearance of 0 so the recalculation works theirs out
        var vertexOf = new int[3];

        for (int k = 0; k < 3; k++)
        {
            var c = corners[k];

            if (!c.IsNew)
            {
                vertexOf[k] = c.Vertex;
                continue;
            }

            vertexOf[k] = mesh.m_vertices.Count;
            mesh.m_vertices.Add(new Vector4(c.Point, w));
            result.NewVertices++;

            foreach (var cache in caches)
                cache.m_vertexClearances.Add(0);

            var p = new Vector4(c.Point, 1);
            mesh.m_aabb.m_min = Vector4.Min(mesh.m_aabb.m_min, p) with { W = mesh.m_aabb.m_min.W };
            mesh.m_aabb.m_max = Vector4.Max(mesh.m_aabb.m_max, p) with { W = mesh.m_aabb.m_max.W };
        }

        // Edges: after the last original edge, so the user edges stay last
        NavUserEdges.ShiftEdgeReferences(mesh, original, 3);

        for (int k = 0; k < 3; k++)
        {
            int a = vertexOf[order[k]], b = vertexOf[order[(k + 1) % 3]];
            mesh.m_edges.Insert(original + k, mesh.NewEdge(a, b, None, None, HavokNavEdgeFlags.EDGE_ORIGINAL, 0));
        }

        if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count >= original * mesh.m_edgeDataStriding)
            mesh.m_edgeData.InsertRange(original * mesh.m_edgeDataStriding, new int[3 * mesh.m_edgeDataStriding]);

        foreach (var cache in caches)
        {
            for (int k = 0; k < 3; k++)
                cache.m_mcpData.Insert(original + k, cache.NewMcp(0, 0));
        }

        // The face, at the end
        var template = dataFrom >= 0 ? mesh.m_faces[dataFrom] : mesh.m_faces.LastOrDefault();
        mesh.m_faces.Add(mesh.NewFace(original, 3, -1, 0, template?.m_clusterIndex ?? -1, template?.m_padding ?? 0));

        int faceStride = mesh.m_faceDataStriding;
        if (faceStride > 0 && mesh.m_faceData.Count == oldFaceCount * faceStride)
        {
            for (int i = 0; i < faceStride; i++)
                mesh.m_faceData.Add(dataFrom >= 0 ? mesh.m_faceData[dataFrom * faceStride + i] : 0);
        }

        // The linked edges of the faces it joins, before and after the join (their user edges have moved up by now)
        var linkedBefore = neighbours.ToDictionary(f => f, f => NavUserEdges.LinkedEdges(mesh, f));

        for (int k = 0; k < 3; k++)
        {
            if (joins[k] < 0)
                continue;

            var mine = mesh.m_edges[original + k];
            var theirs = mesh.m_edges[joins[k]];

            mine.m_oppositeEdge = (uint)joins[k];
            mine.m_oppositeFace = (uint)faceOfEdge[joins[k]];
            theirs.m_oppositeEdge = (uint)(original + k);
            theirs.m_oppositeFace = (uint)newFace;
            result.Joined++;
        }

        // The caches' pair blocks: the joined faces' grow, the new face's goes after the last block
        foreach (var cache in caches)
        {
            foreach (var f in neighbours)
                ReshapeBlock(cache, f, linkedBefore[f], NavUserEdges.LinkedEdges(mesh, f));

            AddBlock(cache, NavUserEdges.Triangle(NavUserEdges.LinkedEdges(mesh, newFace).Count));

            // A cache that marked every face as worked out still does
            if (cache.m_uncalculatedFacesLowerBound == oldFaceCount)
                cache.m_uncalculatedFacesLowerBound = mesh.m_faces.Count;
        }

        // The clearances around the face: its corners count as moved, from where they are, so every entry within the
        // ceiling of its edges (and of the edges that were open before the join) is worked out again
        if (caches.Count > 0)
        {
            var positions = mesh.m_vertices.Select(Xyz).ToList();
            result.Clearance = NavClearance.Recalculate(mesh, positions, vertexOf.ToHashSet());

            // The pairs the join made, which the recalculation only writes where an edge's value changed
            var newEdges = Enumerable.Range(original, 3).ToHashSet();
            var joined = joins.Where(e => e >= 0).ToHashSet();

            foreach (var cache in caches)
            {
                WritePairs(mesh, cache, newFace, null);

                foreach (var f in neighbours)
                    WritePairs(mesh, cache, f, joined);
            }
        }

        // The face trees, over the new face too
        foreach (var tree in NavigationTree.TreesOf(copy, mesh))
            NavigationTree.Build(mesh).WriteTo(tree);

        // The cost graph (DS3, BB, SDT): the face joins its neighbour's cluster when it touches one only; else the graph
        // is built again, as the game's are, so the clusters stay joined as the mesh is
        var graph = NavCostGraph.Of(copy);
        if (graph != null)
        {
            var clusters = neighbours.Select(f => (int)mesh.m_faces[f].m_clusterIndex).Where(c => c >= 0).Distinct().ToList();

            if (clusters.Count == 1)
            {
                mesh.m_faces[newFace].m_clusterIndex = (short)clusters[0];
            }
            else
            {
                NavCostGraph.Rebuild(graph, mesh);
                result.GraphRebuilt = true;
            }
        }

        result.Document = copy;
        result.Face = newFace;
        result.DataFrom = dataFrom;
        result.FirstEdge = original;
        result.EdgeMap = e => e >= original ? e + 3 : e;
        return result;
    }

    private static NavFaceEditResult Fail(NavFaceEditResult result, string error)
    {
        result.Error = error;
        return result;
    }

    /// <summary>Two corners the same, or a face with (almost) no area seen from above, which can't be walked on.</summary>
    private static bool Degenerate(IReadOnlyList<NavFaceCorner> corners, Vector3[] points)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = i + 1; j < 3; j++)
            {
                if (!corners[i].IsNew && corners[i].Vertex == corners[j].Vertex)
                    return true;
                if (Vector3.DistanceSquared(points[i], points[j]) < 1e-8f)
                    return true;
            }
        }

        return Math.Abs(WindingSum(points[0], points[1], points[2])) / 2 < MinArea;
    }

    /// <summary>
    /// Lays a face's pair block out again for its new linked edges: the pairs it had keep their values, new ones are 0
    /// until written (WritePairs). Later faces' offsets follow.
    /// </summary>
    internal static void ReshapeBlock(IHavokNavClearanceCache cache, int face, List<int> before, List<int> after)
    {
        int start = (int)cache.m_faceOffsets[face];
        int oldSize = NavUserEdges.Triangle(before.Count);
        var old = cache.m_edgePairClearances.GetRange(start, oldSize);
        var values = new List<byte>(NavUserEdges.Triangle(after.Count));

        for (int j = 1; j < after.Count; j++)
        {
            for (int i = 0; i < j; i++)
            {
                int oi = before.IndexOf(after[i]), oj = before.IndexOf(after[j]);

                if (oi >= 0 && oj >= 0)
                    values.Add(old[NavUserEdges.Triangle(Math.Max(oi, oj)) + Math.Min(oi, oj)]);
                else
                    values.Add(0);
            }
        }

        cache.m_edgePairClearances.RemoveRange(start, oldSize);
        cache.m_edgePairClearances.InsertRange(start, values);

        int grow = values.Count - oldSize;
        if (grow == 0)
            return;

        for (int g = face + 1; g < cache.m_faceOffsets.Count; g++)
        {
            if (cache.m_faceOffsets[g] != None)
                cache.m_faceOffsets[g] = (uint)(cache.m_faceOffsets[g] + grow);
        }
    }

    /// <summary>
    /// Adds the last face's block (pairs, then the 0xFF marker) after the block that ends last, so any bytes the array
    /// has past the blocks stay past them.
    /// </summary>
    private static void AddBlock(IHavokNavClearanceCache cache, int pairs)
    {
        int at = cache.m_edgePairClearances.Count;
        int last = -1;

        for (int f = 0; f < cache.m_faceOffsets.Count; f++)
        {
            if (cache.m_faceOffsets[f] != None && (last < 0 || cache.m_faceOffsets[f] > cache.m_faceOffsets[last]))
                last = f;
        }

        if (last >= 0)
            at = (int)Math.Min(NavUserEdges.FindBlockEnd(cache, last), cache.m_edgePairClearances.Count);

        var block = new byte[pairs + 1];
        block[pairs] = 0xFF;

        cache.m_edgePairClearances.InsertRange(at, block);
        cache.m_faceOffsets.Add((uint)at);
    }

    /// <summary>
    /// Writes a face's pair values from its edges' clearances (the smaller of the two): all of them, or only the pairs
    /// with one of the given edges.
    /// </summary>
    private static void WritePairs(IHavokNavMeshEdit mesh, IHavokNavClearanceCache cache, int face, HashSet<int> only)
    {
        var linked = NavUserEdges.LinkedEdges(mesh, face);
        int index = (int)cache.m_faceOffsets[face];

        for (int j = 1; j < linked.Count; j++)
        {
            for (int i = 0; i < j; i++, index++)
            {
                if (only != null && !only.Contains(linked[i]) && !only.Contains(linked[j]))
                    continue;

                cache.m_edgePairClearances[index] = Math.Min(cache.m_mcpData[linked[i]].m_clearance, cache.m_mcpData[linked[j]].m_clearance);
            }
        }
    }
}

public static class NvmFaceAdd
{
    /// <summary>Flags a new triangle doesn't take from the one it joins: a gate is where a room meets the next.</summary>
    public const NVM.TriangleFlags NotCopied = NVM.TriangleFlags.GATE;

    /// <summary>
    /// A copy of an NVM with a triangle added at the end: wound up (as the generated ones are, and against the triangle
    /// it joins), neighbours set both ways across shared edges, the flags of the first triangle it joins (except a gate's)
    /// or none, and the root box and box tree built again the game's way.
    /// </summary>
    public static NavFaceEditResult Build(NVM nvm, IReadOnlyList<NavFaceCorner> corners)
    {
        var result = new NavFaceEditResult();

        if (corners == null || corners.Count != 3)
            return Fail(result, "MAP_Navigation_Face_Error_Corners");

        if (nvm == null)
            return Fail(result, "MAP_Navigation_Face_Error_File");

        var points = new Vector3[3];

        for (int k = 0; k < 3; k++)
        {
            var c = corners[k];

            if (!c.IsNew && c.Vertex >= nvm.Vertices.Count)
                return Fail(result, "MAP_Navigation_Face_Error_Vertex");

            points[k] = c.IsNew ? c.Point : nvm.Vertices[c.Vertex];
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = i + 1; j < 3; j++)
            {
                if ((!corners[i].IsNew && corners[i].Vertex == corners[j].Vertex) || Vector3.DistanceSquared(points[i], points[j]) < 1e-8f)
                    return Fail(result, "MAP_Navigation_Face_Error_Degenerate");
            }
        }

        var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
        if (MathF.Abs(normal.Y) / 2 < NavFaceAdd.MinArea)
            return Fail(result, "MAP_Navigation_Face_Error_Degenerate");

        // The triangles on each edge of the existing vertices
        var onEdge = new Dictionary<(int, int), List<(int Triangle, int Slot)>>();

        for (int t = 0; t < nvm.Triangles.Count; t++)
        {
            var edges = NvmGeometry.Edges(nvm.Triangles[t]);
            for (int s = 0; s < 3; s++)
            {
                var key = Key(edges[s].Item1, edges[s].Item2);
                if (!onEdge.TryGetValue(key, out var list))
                    onEdge[key] = list = new();
                list.Add((t, s));
            }
        }

        // Up, unless a triangle it joins says otherwise: a shared edge runs the other way round in the two
        var order = normal.Y > 0 ? new[] { 0, 1, 2 } : new[] { 0, 2, 1 };

        for (int k = 0; k < 3; k++)
        {
            var a = corners[order[k]];
            var b = corners[order[(k + 1) % 3]];

            if (a.IsNew || b.IsNew || !onEdge.TryGetValue(Key(a.Vertex, b.Vertex), out var list) || list.Count != 1)
                continue;

            if (RunsForward(nvm.Triangles[list[0].Triangle], a.Vertex, b.Vertex))
                order = new[] { order[0], order[2], order[1] };
            break;
        }

        var copy = NvmEdit.Copy(nvm);
        float padding = NvmGeometry.RootPadding(nvm);

        var vertexOf = new int[3];
        for (int k = 0; k < 3; k++)
        {
            if (!corners[k].IsNew)
            {
                vertexOf[k] = corners[k].Vertex;
                continue;
            }

            vertexOf[k] = copy.Vertices.Count;
            copy.Vertices.Add(corners[k].Point);
            result.NewVertices++;
        }

        var triangle = new NVM.Triangle
        {
            VertexIndex1 = vertexOf[order[0]],
            VertexIndex2 = vertexOf[order[1]],
            VertexIndex3 = vertexOf[order[2]],
            EdgeIndex1 = -1,
            EdgeIndex2 = -1,
            EdgeIndex3 = -1,
            Flags = NVM.TriangleFlags.NONE,
        };

        int index = copy.Triangles.Count;
        var mine = NvmGeometry.Edges(triangle);

        for (int s = 0; s < 3; s++)
        {
            var (a, b) = mine[s];
            if (!onEdge.TryGetValue(Key(a, b), out var list))
                continue;

            // An edge two triangles share already would have three
            if (list.Count > 1 || NvmGeometry.Neighbours(copy.Triangles[list[0].Triangle])[list[0].Slot] >= 0)
                return Fail(result, "MAP_Navigation_Face_Error_Edge_Taken");

            var (other, slot) = list[0];
            SetNeighbour(triangle, s, other);
            SetNeighbour(copy.Triangles[other], slot, index);
            result.Joined++;

            if (result.DataFrom < 0)
            {
                result.DataFrom = other;
                triangle.Flags = copy.Triangles[other].Flags & ~NotCopied;
            }
        }

        copy.Triangles.Add(triangle);
        NvmEdit.RebuildBoxes(copy, padding);

        result.Nvm = copy;
        result.Face = index;
        return result;
    }

    private static (int, int) Key(int a, int b) => a < b ? (a, b) : (b, a);

    /// <summary>Whether a triangle's corners run from a to b (rather than b to a) going round it.</summary>
    private static bool RunsForward(NVM.Triangle t, int a, int b)
    {
        var v = NvmGeometry.VertexIndices(t);
        for (int k = 0; k < 3; k++)
        {
            if (v[k] == a && v[(k + 1) % 3] == b)
                return true;
        }
        return false;
    }

    private static void SetNeighbour(NVM.Triangle t, int slot, int other)
    {
        if (slot == 0) t.EdgeIndex1 = other;
        else if (slot == 1) t.EdgeIndex2 = other;
        else t.EdgeIndex3 = other;
    }

    private static NavFaceEditResult Fail(NavFaceEditResult result, string error)
    {
        result.Error = error;
        return result;
    }
}
