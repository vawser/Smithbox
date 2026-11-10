using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The per-file cost graph of DS3, BB and SDT (hkaiDirectedGraphExplicitCost), kept in step with its mesh. Kept free of the
// rest of Smithbox, so Tools/NavSurvey can link it. See Documentation/Claude/Navigation Editor Other Games Plan.md, N5.
//
// How the game's graphs are built (NavSurvey costgraph, DS3 710 / SDT 604 / BB 217 files, every one the same):
// - each face names its cluster (Face.m_clusterIndex), with no face outside one; one node per cluster, about 15 faces
//   each (16 at most in SDT, 31 in DS3);
// - a node sits exactly on the centre of one of its faces: the one nearest the mean of its faces' centres (98 %);
// - an edge each way between every two clusters that touch in the mesh, and none between others (34,168 of 34,168);
// - the cost of an edge is the same both ways and a little more than the straight distance between the two nodes
//   (median 1.6 % more; never less than 0.4 % under), no flags, no node or edge data.

public static class NavCostGraph
{
    /// <summary>The most faces a cluster gets when the graph is built again: the game's SDT clusters stop at 16.</summary>
    public const int ClusterFaces = 16;

    /// <summary>The file's cost graph, or null (ER, NR, DB_NT and AC6 have none).</summary>
    public static IHavokDirectedGraphEdit Of(Havok.HavokDocument document) => document?.ObjectsOf<IHavokDirectedGraphEdit>().FirstOrDefault();

    private static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    /// <summary>For each node, the face of its cluster it sits on: the one whose centre is nearest the node.</summary>
    public static int[] NodeFaces(IHavokDirectedGraphEdit graph, IHavokNavMeshEdit mesh, IReadOnlyList<Vector3> vertices = null)
    {
        var faces = new int[graph.NodeCount];
        var best = new float[graph.NodeCount];
        Array.Fill(faces, -1);
        Array.Fill(best, float.MaxValue);

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            int c = mesh.m_faces[f].m_clusterIndex;
            if (c < 0 || c >= graph.NodeCount || c >= graph.m_positions.Count)
                continue;

            float d = Vector3.Distance(Centre(mesh, f, vertices), Xyz(graph.m_positions[c]));
            if (d < best[c])
            {
                best[c] = d;
                faces[c] = f;
            }
        }

        return faces;
    }

    /// <summary>A face's centre, from the mesh's vertices or from the given positions of them.</summary>
    private static Vector3 Centre(IHavokNavMeshEdit mesh, int face, IReadOnlyList<Vector3> vertices)
    {
        if (vertices == null)
            return NavigationOverlayBuilder.FaceCentre(mesh, face);

        var f = mesh.m_faces[face];
        var sum = Vector3.Zero;
        for (int e = f.m_startEdgeIndex; e < f.m_startEdgeIndex + f.m_numEdges; e++)
            sum += vertices[mesh.m_edges[e].m_a];
        return f.m_numEdges > 0 ? sum / f.m_numEdges : sum;
    }

    /// <summary>
    /// After an edit that moved vertices but kept the faces (Edit, the vertex tool): each node follows the face it sits on,
    /// and each cost is scaled by how much the distance between its two nodes changed, which keeps the game's own
    /// detour. Nodes and costs whose faces didn't move stay exactly as they were.
    /// </summary>
    public static void FollowMovedVertices(IHavokDirectedGraphEdit graph, IHavokNavMeshEdit mesh, IReadOnlyList<Vector3> before)
    {
        if (graph == null || graph.NodeCount == 0)
            return;

        var nodeFaces = NodeFaces(graph, mesh, before);
        var old = graph.m_positions.ToList();

        for (int n = 0; n < nodeFaces.Length; n++)
        {
            if (nodeFaces[n] < 0)
                continue;

            var centre = NavigationOverlayBuilder.FaceCentre(mesh, nodeFaces[n]);
            if (centre != Centre(mesh, nodeFaces[n], before))
                graph.m_positions[n] = new Vector4(centre, old[n].W);
        }

        for (int n = 0; n < graph.NodeCount; n++)
        {
            var (start, count) = graph.GetNode(n);
            for (int e = start; e < start + count; e++)
            {
                int t = graph.GetEdgeTarget(e);
                if (t < 0 || t >= old.Count || (graph.m_positions[n] == old[n] && graph.m_positions[t] == old[t]))
                    continue;

                float was = Vector3.Distance(Xyz(old[n]), Xyz(old[t]));
                float now = Vector3.Distance(Xyz(graph.m_positions[n]), Xyz(graph.m_positions[t]));
                float cost = graph.GetEdgeCost(e);
                graph.SetEdgeCost(e, was > 1e-4f ? cost * now / was : now);
            }
        }
    }

    /// <summary>
    /// Builds the graph from the mesh as the game's are: the faces are grouped into clusters of up to
    /// <see cref="ClusterFaces"/> touching faces (each face's m_clusterIndex is set), each cluster's node sits on the
    /// centre of the face nearest its faces' mean centre, and touching clusters are joined both ways at the straight
    /// distance between their nodes. For new meshes (generation), whose faces have no clusters yet.
    /// </summary>
    public static void Rebuild(IHavokDirectedGraphEdit graph, IHavokNavMeshEdit mesh, int clusterFaces = ClusterFaces)
    {
        int faceCount = mesh.m_faces.Count;
        var centres = new Vector3[faceCount];
        for (int f = 0; f < faceCount; f++)
            centres[f] = NavigationOverlayBuilder.FaceCentre(mesh, f);

        // Clusters grown breadth-first from the lowest unclustered face, nearest faces first
        var cluster = new int[faceCount];
        Array.Fill(cluster, -1);
        int clusters = 0;

        for (int seed = 0; seed < faceCount; seed++)
        {
            if (cluster[seed] >= 0)
                continue;

            int id = clusters++;
            var members = new List<int> { seed };
            cluster[seed] = id;
            var frontier = new PriorityQueue<int, float>();
            foreach (var n in Neighbours(mesh, seed))
                frontier.Enqueue(n, Vector3.Distance(centres[n], centres[seed]));

            while (members.Count < clusterFaces && frontier.TryDequeue(out var f, out _))
            {
                if (cluster[f] >= 0)
                    continue;

                cluster[f] = id;
                members.Add(f);
                foreach (var n in Neighbours(mesh, f))
                    if (cluster[n] < 0)
                        frontier.Enqueue(n, Vector3.Distance(centres[n], centres[seed]));
            }
        }

        for (int f = 0; f < faceCount; f++)
            mesh.m_faces[f].m_clusterIndex = (short)cluster[f];

        // Nodes
        var positions = new Vector4[clusters];
        var sums = new Vector3[clusters];
        var counts = new int[clusters];
        for (int f = 0; f < faceCount; f++)
        {
            sums[cluster[f]] += centres[f];
            counts[cluster[f]]++;
        }

        var bestDistance = new float[clusters];
        Array.Fill(bestDistance, float.MaxValue);
        for (int f = 0; f < faceCount; f++)
        {
            int c = cluster[f];
            float d = Vector3.Distance(centres[f], sums[c] / counts[c]);
            if (d < bestDistance[c])
            {
                bestDistance[c] = d;
                positions[c] = new Vector4(centres[f], 0);
            }
        }

        // Edges between touching clusters, in target order
        var touching = new SortedSet<int>[clusters];
        for (int c = 0; c < clusters; c++)
            touching[c] = new SortedSet<int>();
        for (int f = 0; f < faceCount; f++)
            foreach (var n in Neighbours(mesh, f))
                if (cluster[n] != cluster[f])
                    touching[cluster[f]].Add(cluster[n]);

        var edges = new List<IReadOnlyList<(int, float)>>(clusters);
        for (int c = 0; c < clusters; c++)
            edges.Add(touching[c].Select(t => (t, Vector3.Distance(Xyz(positions[c]), Xyz(positions[t])))).ToList());

        graph.Replace(positions, edges);
    }

    private static IEnumerable<int> Neighbours(IHavokNavMeshEdit mesh, int face)
    {
        var f = mesh.m_faces[face];
        for (int e = f.m_startEdgeIndex; e < f.m_startEdgeIndex + f.m_numEdges; e++)
        {
            uint o = mesh.m_edges[e].m_oppositeFace;
            if (o != 0xFFFFFFFF && o < mesh.m_faces.Count)
                yield return (int)o;
        }
    }

    /// <summary>
    /// What is wrong with a graph against its mesh, or null when it is as the game builds them: one node and position
    /// per cluster, every face in a cluster, and an edge each way between exactly the clusters that touch.
    /// </summary>
    public static string Check(IHavokDirectedGraphEdit graph, IHavokNavMeshEdit mesh)
    {
        if (mesh.m_faces.Count == 0)
            return graph.NodeCount == 0 ? null : $"{graph.NodeCount} nodes for an empty mesh";

        int maxCluster = -1;
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            int c = mesh.m_faces[f].m_clusterIndex;
            if (c < 0 || c >= graph.NodeCount)
                return $"face {f} is in cluster {c}, of {graph.NodeCount} nodes";
            maxCluster = Math.Max(maxCluster, c);
        }

        if (maxCluster + 1 != graph.NodeCount)
            return $"{graph.NodeCount} nodes for {maxCluster + 1} clusters";

        if (graph.m_positions.Count != graph.NodeCount)
            return $"{graph.m_positions.Count} positions for {graph.NodeCount} nodes";

        var touching = new HashSet<(int, int)>();
        for (int f = 0; f < mesh.m_faces.Count; f++)
            foreach (var n in Neighbours(mesh, f))
                if (mesh.m_faces[n].m_clusterIndex != mesh.m_faces[f].m_clusterIndex)
                    touching.Add((mesh.m_faces[f].m_clusterIndex, mesh.m_faces[n].m_clusterIndex));

        var edges = new HashSet<(int, int)>();
        for (int n = 0; n < graph.NodeCount; n++)
        {
            var (start, count) = graph.GetNode(n);
            if (start < 0 || start + count > graph.EdgeCount)
                return $"node {n}'s edges are out of range";
            for (int e = start; e < start + count; e++)
                edges.Add((n, graph.GetEdgeTarget(e)));
        }

        if (!touching.SetEquals(edges))
            return $"{edges.Count(x => !touching.Contains(x))} edges between clusters that don't touch, {touching.Count(x => !edges.Contains(x))} touching pairs without an edge";

        return null;
    }
}
