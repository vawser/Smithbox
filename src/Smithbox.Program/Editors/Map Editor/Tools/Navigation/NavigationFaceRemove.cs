using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Removing a face from a navmesh (the Navigation tool's Edit tab, Remove Face). Kept free of the rest of Smithbox, so
// NavSurvey can link it (faceremove).
//
// hkaiNavMesh: the links (user edge pairs) on the face or leading to it go first (NavUserEdges.Remove). Then the edges of
// other faces joined to it are left open (clearance 0, as the game's open edges), the face's cache block goes and its
// neighbours' blocks lose those edges, its own edges go (later edges move down; the result's EdgeMap), the face goes
// (later faces move down; FaceMap), and vertices no other edge uses go. Connections and node bank faces must follow the
// maps (NavigationRemap.RemapNavmesh drops those naming the face or its edges). The clearances around the face are
// worked out again, the face trees built again, and the cost graph (DS3, BB, SDT) kept if it still fits the mesh, else
// built again.
//
// NVM: the triangle goes, later triangles move down in every neighbour, entity and MCG edge list of the rooms loading the
// file, vertices no other triangle uses go, and the box tree is built again.

public static class NavFaceRemove
{
    private const uint None = 0xFFFFFFFF;

    private static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    private static bool IsLocal(uint opposite) => opposite != None && (opposite >> 22) == 0;

    /// <summary>A copy of a navmesh file with a face removed. The last face of a navmesh isn't removed (the Models tab
    /// makes a model a placeholder).</summary>
    public static NavFaceEditResult Build(Havok.HavokDocument document, int face)
    {
        var result = new NavFaceEditResult { Face = face };

        var copy = NavModelEdit.Clone(document);
        var mesh = copy?.ObjectsOf<IHavokNavMeshEdit>().FirstOrDefault();

        if (mesh == null)
            return Fail(result, "MAP_Navigation_Face_Error_File");

        if (face < 0 || face >= mesh.m_faces.Count)
            return Fail(result, "MAP_Navigation_Face_Error_Face");

        if (mesh.m_faces.Count <= 1)
            return Fail(result, "MAP_Navigation_Face_Error_Last");

        var caches = NavUserEdges.CachesOf(mesh);

        foreach (var cache in caches)
        {
            if (cache.m_mcpData.Count != mesh.m_edges.Count || cache.m_vertexClearances.Count != mesh.m_vertices.Count
                || cache.m_faceOffsets.Count != mesh.m_faces.Count)
                return Fail(result, "MAP_Navigation_Face_Error_Cache");
        }

        int oldFaceCount = mesh.m_faces.Count;
        var graph = NavCostGraph.Of(copy);
        int[] nodeFaces = graph != null ? NavCostGraph.NodeFaces(graph, mesh) : null;
        int cluster = mesh.m_faces[face].m_clusterIndex;

        Func<int, int> edgeMap = e => e;

        void Then(Func<int, int> step)
        {
            var before = edgeMap;
            edgeMap = e =>
            {
                int x = before(e);
                return x < 0 ? -1 : step(x);
            };
        }

        // 1. The links on the face, and those of other faces leading to it
        for (int guard = 0; guard < 10000; guard++)
        {
            int user = UserEdgeOf(mesh, face);
            if (user < 0)
                break;

            var removed = NavUserEdges.Remove(mesh, caches, user);
            if (!removed.Ok)
                return Fail(result, removed.Error == "MAP_Navigation_Link_Error_Cache" ? "MAP_Navigation_Face_Error_Cache" : removed.Error);

            Then(removed.EdgeMap);
            result.LinksRemoved++;
        }

        var f = mesh.m_faces[face];
        int start = f.m_startEdgeIndex, count = f.m_numEdges;

        // The face of each original edge, and the edges of other faces joined to this one
        var faceOf = new Dictionary<int, int>();
        for (int g = 0; g < mesh.m_faces.Count; g++)
        {
            var mg = mesh.m_faces[g];
            for (int k = mg.m_startEdgeIndex; k < mg.m_startEdgeIndex + mg.m_numEdges; k++)
                faceOf[k] = g;
        }

        var joined = new List<int>();
        foreach (var (k, g) in faceOf)
        {
            if (g != face && IsLocal(mesh.m_edges[k].m_oppositeFace) && mesh.m_edges[k].m_oppositeFace == face)
                joined.Add(k);
        }

        var neighbours = joined.Select(k => faceOf[k]).Distinct().ToList();

        if (!NavUserEdges.BlocksMatch(mesh, caches, face) || neighbours.Any(g => !NavUserEdges.BlocksMatch(mesh, caches, g)))
            return Fail(result, "MAP_Navigation_Face_Error_Cache");

        // 2. The joined edges are left open, their neighbours' blocks lose them
        var linkedBefore = neighbours.ToDictionary(g => g, g => NavUserEdges.LinkedEdges(mesh, g));

        foreach (var k in joined)
        {
            mesh.m_edges[k].m_oppositeEdge = None;
            mesh.m_edges[k].m_oppositeFace = None;

            foreach (var cache in caches)
            {
                cache.m_mcpData[k].m_clearance = 0;
                cache.m_mcpData[k].m_interpolant = 0;
            }
        }

        result.Opened = joined.Count;

        foreach (var cache in caches)
        {
            foreach (var g in neighbours)
                NavFaceAdd.ReshapeBlock(cache, g, linkedBefore[g], NavUserEdges.LinkedEdges(mesh, g));
        }

        // 3. The face's block goes (its pairs, then the marker)
        int blockSize = NavUserEdges.Triangle(NavUserEdges.LinkedEdges(mesh, face).Count) + 1;

        foreach (var cache in caches)
        {
            uint at = cache.m_faceOffsets[face];

            if (at != None)
            {
                cache.m_edgePairClearances.RemoveRange((int)at, blockSize);

                for (int g = 0; g < cache.m_faceOffsets.Count; g++)
                {
                    if (g != face && cache.m_faceOffsets[g] != None && cache.m_faceOffsets[g] > at)
                        cache.m_faceOffsets[g] -= (uint)blockSize;
                }
            }

            cache.m_faceOffsets.RemoveAt(face);
        }

        // 4. Its edges go; later edges move down
        var corners = Enumerable.Range(start, count).Select(k => mesh.m_edges[k].m_a).ToHashSet();

        for (int k = start + count - 1; k >= start; k--)
            mesh.m_edges.RemoveAt(k);

        if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count >= (start + count) * mesh.m_edgeDataStriding)
            mesh.m_edgeData.RemoveRange(start * mesh.m_edgeDataStriding, count * mesh.m_edgeDataStriding);

        foreach (var cache in caches)
        {
            for (int k = start + count - 1; k >= start; k--)
                cache.m_mcpData.RemoveAt(k);
        }

        NavUserEdges.ShiftEdgeReferences(mesh, start + count, -count);
        Then(e => e >= start && e < start + count ? -1 : e >= start + count ? e - count : e);

        // 5. The face goes; later faces move down
        mesh.m_faces.RemoveAt(face);

        int faceStride = mesh.m_faceDataStriding;
        if (faceStride > 0 && mesh.m_faceData.Count == oldFaceCount * faceStride)
            mesh.m_faceData.RemoveRange(face * faceStride, faceStride);

        foreach (var e in mesh.m_edges)
        {
            if (IsLocal(e.m_oppositeFace) && e.m_oppositeFace > face)
                e.m_oppositeFace--;
        }

        // 6. Vertices nothing uses any more, highest first
        var used = new HashSet<int>();
        foreach (var e in mesh.m_edges)
        {
            used.Add(e.m_a);
            used.Add(e.m_b);
        }

        var dropped = corners.Where(v => !used.Contains(v)).OrderByDescending(v => v).ToList();
        foreach (var v in dropped)
            NavUserEdges.RemoveVertex(mesh, caches, v);

        result.VerticesRemoved = dropped.Count;

        // 7. The clearances around the face: its corners that stay count as moved, so the entries within the ceiling of
        // the edges now open are worked out again
        if (caches.Count > 0)
        {
            var kept = corners.Where(v => !dropped.Contains(v)).Select(v => v - dropped.Count(d => d < v)).ToHashSet();
            var positions = mesh.m_vertices.Select(Xyz).ToList();
            result.Clearance = NavClearance.Recalculate(mesh, positions, kept);

            foreach (var cache in caches)
            {
                if (cache.m_uncalculatedFacesLowerBound == oldFaceCount)
                    cache.m_uncalculatedFacesLowerBound = mesh.m_faces.Count;
            }
        }

        foreach (var tree in NavigationTree.TreesOf(copy, mesh))
            NavigationTree.Build(mesh).WriteTo(tree);

        // 8. The cost graph: kept when it still fits (the cluster keeps faces, its node wasn't on the face, and the
        // clusters that touch are the ones joined), else built again
        if (graph != null)
        {
            bool nodeOnFace = cluster >= 0 && cluster < nodeFaces.Length && nodeFaces[cluster] == face;

            if (nodeOnFace || NavCostGraph.Check(graph, mesh) != null)
            {
                NavCostGraph.Rebuild(graph, mesh);
                result.GraphRebuilt = true;
            }
        }

        result.Document = copy;
        result.EdgeMap = edgeMap;
        result.FaceMap = g => g == face ? -1 : g > face ? g - 1 : g;
        return result;
    }

    /// <summary>A user edge on the face, or one of another face leading to it, or -1.</summary>
    private static int UserEdgeOf(IHavokNavMeshEdit mesh, int face)
    {
        var f = mesh.m_faces[face];
        if (f.m_numUserEdges > 0)
            return f.m_startUserEdgeIndex;

        foreach (var g in mesh.m_faces)
        {
            for (int k = g.m_startUserEdgeIndex; g.m_numUserEdges > 0 && k < g.m_startUserEdgeIndex + g.m_numUserEdges; k++)
            {
                if (IsLocal(mesh.m_edges[k].m_oppositeFace) && mesh.m_edges[k].m_oppositeFace == face)
                    return k;
            }
        }

        return -1;
    }

    private static NavFaceEditResult Fail(NavFaceEditResult result, string error)
    {
        result.Error = error;
        return result;
    }
}

public static class NvmFaceRemove
{
    /// <summary>
    /// A copy of a room's NVM with a triangle removed, and the MCG with the triangles that moved renumbered in the edges of
    /// every room loading the file. Refused for the last triangle, and for the only triangle an MCG edge lists at one of its
    /// ends (remove or link the node again in the Graph tab first).
    /// </summary>
    public static NavFaceEditResult Build(NvmMap map, NvmRoom room, int triangle)
    {
        var result = new NavFaceEditResult { Face = triangle };
        var nvm = room?.Nvm;

        if (nvm == null)
            return Fail(result, "MAP_Navigation_Face_Error_File");

        if (triangle < 0 || triangle >= nvm.Triangles.Count)
            return Fail(result, "MAP_Navigation_Face_Error_Face");

        if (nvm.Triangles.Count <= 1)
            return Fail(result, "MAP_Navigation_Face_Error_Last");

        int Map(int t) => t == triangle ? -1 : t > triangle ? t - 1 : t;

        // The MCG edges of the rooms loading the file
        var mcg = map?.Mcg;
        MCG newMcg = mcg;

        if (mcg != null)
        {
            var edges = new List<int>();

            for (int e = 0; e < mcg.Edges.Count; e++)
            {
                var r = map.RoomOfMcp(mcg.Edges[e].MCPRoomIndex);
                if (r?.FileName != null && r.FileName.Equals(room.FileName, StringComparison.OrdinalIgnoreCase))
                    edges.Add(e);
            }

            bool Names(List<int> list) => list != null && list.Any(t => t >= triangle);

            if (edges.Any(e => Names(mcg.Edges[e].UnkIndicesA) || Names(mcg.Edges[e].UnkIndicesB)))
            {
                foreach (var e in edges)
                {
                    var edge = mcg.Edges[e];
                    if ((edge.UnkIndicesA.Contains(triangle) && edge.UnkIndicesA.All(t => t == triangle))
                        || (edge.UnkIndicesB.Contains(triangle) && edge.UnkIndicesB.All(t => t == triangle)))
                        return Fail(result, "MAP_Navigation_Face_Error_Mcg");
                }

                newMcg = NvmEdit.Copy(mcg);

                foreach (var e in edges)
                {
                    var edge = newMcg.Edges[e];
                    edge.UnkIndicesA = edge.UnkIndicesA.Select(Map).Where(t => t >= 0).ToList();
                    edge.UnkIndicesB = edge.UnkIndicesB.Select(Map).Where(t => t >= 0).ToList();
                }
            }
        }

        var copy = NvmEdit.Copy(nvm);
        float padding = NvmGeometry.RootPadding(nvm);
        var corners = NvmGeometry.VertexIndices(copy.Triangles[triangle]);

        copy.Triangles.RemoveAt(triangle);

        foreach (var t in copy.Triangles)
        {
            t.EdgeIndex1 = t.EdgeIndex1 < 0 ? t.EdgeIndex1 : Map(t.EdgeIndex1);
            t.EdgeIndex2 = t.EdgeIndex2 < 0 ? t.EdgeIndex2 : Map(t.EdgeIndex2);
            t.EdgeIndex3 = t.EdgeIndex3 < 0 ? t.EdgeIndex3 : Map(t.EdgeIndex3);
        }

        result.Opened = nvm.Triangles.Sum(t => NvmGeometry.Neighbours(t).Count(n => n == triangle)) - NvmGeometry.Neighbours(nvm.Triangles[triangle]).Count(n => n == triangle);

        foreach (var entity in copy.Entities)
            entity.TriangleIndices = entity.TriangleIndices.Select(Map).Where(t => t >= 0).ToList();

        // Vertices nothing uses any more, highest first
        var used = copy.Triangles.SelectMany(NvmGeometry.VertexIndices).ToHashSet();
        var dropped = corners.Distinct().Where(v => !used.Contains(v)).OrderByDescending(v => v).ToList();

        foreach (var v in dropped)
        {
            copy.Vertices.RemoveAt(v);

            foreach (var t in copy.Triangles)
            {
                if (t.VertexIndex1 > v) t.VertexIndex1--;
                if (t.VertexIndex2 > v) t.VertexIndex2--;
                if (t.VertexIndex3 > v) t.VertexIndex3--;
            }
        }

        result.VerticesRemoved = dropped.Count;

        NvmEdit.RebuildBoxes(copy, padding);

        result.Nvm = copy;
        result.Mcg = newMcg;
        result.FaceMap = Map;
        return result;
    }

    private static NavFaceEditResult Fail(NavFaceEditResult result, string error)
    {
        result.Error = error;
        return result;
    }
}
