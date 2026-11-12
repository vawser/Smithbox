using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Checks the invariants of a map's NVM navigation data (Phase N6), as the game files have them (NavSurvey
/// `nvm <GAME> links`). Issues the vanilla data also has are marked inherited by the Check tab, as for the Havok games.
/// Errors are what the game files never do; warnings what a few of them do.
/// </summary>
public static class NvmValidator
{
    public static List<NavIssue> Validate(NvmMap map)
    {
        var issues = new List<NavIssue>();
        var mapId = map.MapId;

        void Add(NavIssueSeverity severity, NavIssueCode code, string file, NvmRoom room, int? triangle, params object[] args)
            => issues.Add(new NavIssue(severity, code, file, args) { MapId = mapId, Room = room?.Index, Face = triangle });

        foreach (var room in map.Rooms)
        {
            if (room.Nvm == null)
                Add(NavIssueSeverity.Error, NavIssueCode.NvmPartFileMissing, $"{mapId}.nvmbnd", room, null, room.PartName, room.ModelName);
        }

        foreach (var (name, nvm) in map.Files)
            ValidateNvm(map, name, nvm, map.Rooms.FirstOrDefault(r => r.Nvm == nvm), Add);

        ValidateMcp(map, Add);
        ValidateMcg(map, Add);
        return issues;
    }

    private delegate void Adder(NavIssueSeverity severity, NavIssueCode code, string file, NvmRoom room, int? triangle, params object[] args);

    private static void ValidateNvm(NvmMap map, string name, NVM nvm, NvmRoom room, Adder add)
    {
        var file = $"{name}.nvm";
        int vertices = nvm.Vertices.Count;
        bool indicesOk = true;

        for (int t = 0; t < nvm.Triangles.Count; t++)
        {
            var tri = nvm.Triangles[t];
            var corners = NvmGeometry.VertexIndices(tri);

            if (corners.Any(v => v < 0 || v >= vertices))
            {
                add(NavIssueSeverity.Error, NavIssueCode.NvmVertexRange, file, room, t, t, string.Join(", ", corners), vertices);
                indicesOk = false;
                continue;
            }

            var neighbours = NvmGeometry.Neighbours(tri);
            var edges = NvmGeometry.Edges(tri);

            for (int k = 0; k < 3; k++)
            {
                int n = neighbours[k];
                if (n < 0)
                    continue;

                if (n >= nvm.Triangles.Count)
                {
                    add(NavIssueSeverity.Error, NavIssueCode.NvmNeighbourRange, file, room, t, t, n, nvm.Triangles.Count);
                    continue;
                }

                var other = nvm.Triangles[n];
                var otherCorners = NvmGeometry.VertexIndices(other);

                // A few game files join triangles that don't share the edge's two vertices (28 of 847 DS1R files)
                if (!otherCorners.Contains(edges[k].Item1) || !otherCorners.Contains(edges[k].Item2))
                    add(NavIssueSeverity.Warning, NavIssueCode.NvmNeighbourNotShared, file, room, t, t, n);

                if (!NvmGeometry.Neighbours(other).Contains(t))
                    add(NavIssueSeverity.Error, NavIssueCode.NvmNeighbourOneWay, file, room, t, t, n);
            }
        }

        // The root box holds the vertices, and every triangle is in a leaf it touches
        if (nvm.RootBox != null && vertices > 0)
        {
            var min = nvm.RootBox.MinValueCorner - new Vector3(1e-3f);
            var max = nvm.RootBox.MaxValueCorner + new Vector3(1e-3f);
            int outside = nvm.Vertices.Count(v => v.X < min.X || v.Y < min.Y || v.Z < min.Z || v.X > max.X || v.Y > max.Y || v.Z > max.Z);

            if (outside > 0)
                add(NavIssueSeverity.Error, NavIssueCode.NvmVertexOutsideRoot, file, room, null, outside);
        }

        if (nvm.RootBox != null && indicesOk)
        {
            var inLeaf = new bool[nvm.Triangles.Count];
            int extra = 0;

            foreach (var leaf in NvmBoxRule.Leaves(nvm.RootBox))
            {
                foreach (var t in leaf.TriangleIndices)
                {
                    if (t < 0 || t >= inLeaf.Length)
                    {
                        extra++;
                        continue;
                    }

                    inLeaf[t] = true;

                    if (!NvmBoxRule.TriangleTouchesRect(nvm, t, leaf.MinValueCorner, leaf.MaxValueCorner))
                        extra++;
                }
            }

            int missing = inLeaf.Count(x => !x);
            if (missing > 0)
                add(NavIssueSeverity.Error, NavIssueCode.NvmTriangleNotInTree, file, room, Array.IndexOf(inLeaf, false), missing);

            if (extra > 0)
                add(NavIssueSeverity.Warning, NavIssueCode.NvmTreeExtraTriangle, file, room, null, extra);
        }

        for (int e = 0; e < nvm.Entities.Count; e++)
        {
            var entity = nvm.Entities[e];
            int bad = entity.TriangleIndices.Count(t => t < 0 || t >= nvm.Triangles.Count);

            if (bad > 0)
                add(NavIssueSeverity.Error, NavIssueCode.NvmEntityTriangleRange, file, room, null, entity.EntityID, bad);
        }
    }

    private static void ValidateMcp(NvmMap map, Adder add)
    {
        var file = $"{map.MapId}.mcp";
        var mcp = map.Mcp;

        if (mcp == null)
        {
            add(NavIssueSeverity.Warning, NavIssueCode.McpMissing, file, null, null);
            return;
        }

        int own = mcp.Rooms.Count(r => r.MapID == map.MapUid);
        if (own != map.Rooms.Count)
            add(NavIssueSeverity.Error, NavIssueCode.McpRoomCount, file, null, null, own, map.Rooms.Count);

        for (int i = 0; i < mcp.Rooms.Count; i++)
        {
            var room = mcp.Rooms[i];

            foreach (var c in room.ConnectedRoomIndices)
            {
                if (c < 0 || c >= mcp.Rooms.Count)
                    add(NavIssueSeverity.Error, NavIssueCode.McpConnectionRange, file, null, null, i, c);
                else if (!mcp.Rooms[c].ConnectedRoomIndices.Contains(i))
                    add(NavIssueSeverity.Error, NavIssueCode.McpConnectionAsymmetric, file, null, null, i, c);
            }

            // An own room's box is its part's placed NVM root box, with its Y padding (1 m in nearly every game room)
            var nvmRoom = map.RoomOfMcp(i);
            if (nvmRoom?.Nvm?.RootBox == null)
                continue;

            var (min, max) = NvmGeometry.Placed(nvmRoom.Nvm.RootBox.MinValueCorner, nvmRoom.Nvm.RootBox.MaxValueCorner, nvmRoom.World);
            bool fits = Math.Abs(min.X - room.BoundingBoxMin.X) < 0.1f && Math.Abs(max.X - room.BoundingBoxMax.X) < 0.1f
                && Math.Abs(min.Z - room.BoundingBoxMin.Z) < 0.1f && Math.Abs(max.Z - room.BoundingBoxMax.Z) < 0.1f
                && min.Y - room.BoundingBoxMin.Y is >= -0.1f and <= 3f && room.BoundingBoxMax.Y - max.Y is >= -0.1f and <= 3f;

            if (!fits)
                add(NavIssueSeverity.Warning, NavIssueCode.McpRoomBox, file, nvmRoom, null, i, nvmRoom.PartName);
        }
    }

    private static void ValidateMcg(NvmMap map, Adder add)
    {
        var file = $"{map.MapId}.mcg";
        var mcg = map.Mcg;

        if (mcg == null)
        {
            add(NavIssueSeverity.Warning, NavIssueCode.McgMissing, file, null, null);
            return;
        }

        int rooms = map.Mcp?.Rooms.Count ?? int.MaxValue;

        for (int e = 0; e < mcg.Edges.Count; e++)
        {
            var edge = mcg.Edges[e];

            if (edge.NodeIndexA < 0 || edge.NodeIndexA >= mcg.Nodes.Count || edge.NodeIndexB < 0 || edge.NodeIndexB >= mcg.Nodes.Count)
            {
                add(NavIssueSeverity.Error, NavIssueCode.McgEdgeNodeRange, file, null, null, e, edge.NodeIndexA, edge.NodeIndexB);
                continue;
            }

            if (edge.MCPRoomIndex < 0 || edge.MCPRoomIndex >= rooms)
            {
                add(NavIssueSeverity.Error, NavIssueCode.McgEdgeRoomRange, file, null, null, $"{edge.NodeIndexA}-{edge.NodeIndexB}", edge.MCPRoomIndex);
                continue;
            }

            if (!mcg.Nodes[edge.NodeIndexA].ConnectedEdgeIndices.Contains(e) || !mcg.Nodes[edge.NodeIndexB].ConnectedEdgeIndices.Contains(e))
                add(NavIssueSeverity.Error, NavIssueCode.McgNodeEdgeMismatch, file, null, null, $"{edge.NodeIndexA}-{edge.NodeIndexB}");

            // The gate triangles at each end are triangles of the edge's room (some DeS edges list others: inherited)
            var room = map.RoomOfMcp(edge.MCPRoomIndex);
            if (room?.Nvm != null)
            {
                int count = room.Nvm.Triangles.Count;
                if (edge.UnkIndicesA.Any(t => t < 0 || t >= count) || edge.UnkIndicesB.Any(t => t < 0 || t >= count))
                    add(NavIssueSeverity.Error, NavIssueCode.McgEdgeTriangleRange, file, room, null, $"{edge.NodeIndexA}-{edge.NodeIndexB}", room.PartName);
            }
        }

        // Edges are named by their nodes (A-B) rather than their index, so removing an edge leaves the vanilla issues
        // of later edges matching the baseline

        // Each connection a node lists is an edge between it and the node it names
        for (int n = 0; n < mcg.Nodes.Count; n++)
        {
            var node = mcg.Nodes[n];
            bool ok = node.ConnectedNodeIndices.Count == node.ConnectedEdgeIndices.Count;

            for (int i = 0; ok && i < node.ConnectedEdgeIndices.Count; i++)
            {
                int e = node.ConnectedEdgeIndices[i];
                int other = node.ConnectedNodeIndices[i];
                ok = e >= 0 && e < mcg.Edges.Count
                    && ((mcg.Edges[e].NodeIndexA == n && mcg.Edges[e].NodeIndexB == other) || (mcg.Edges[e].NodeIndexB == n && mcg.Edges[e].NodeIndexA == other));
            }

            if (!ok)
                add(NavIssueSeverity.Error, NavIssueCode.McgNodeEdgeMismatch, file, null, null, $"node {n}");
        }

        // A node of this map's edges sits on a gate triangle (within 2 m of one's centre in 1,091 of 1,099 DS1R nodes)
        var ownNodes = new HashSet<int>();
        foreach (var edge in mcg.Edges)
        {
            if (edge.MapID == map.MapUid && edge.NodeIndexA >= 0 && edge.NodeIndexA < mcg.Nodes.Count && edge.NodeIndexB >= 0 && edge.NodeIndexB < mcg.Nodes.Count)
            {
                ownNodes.Add(edge.NodeIndexA);
                ownNodes.Add(edge.NodeIndexB);
            }
        }

        foreach (var n in ownNodes.OrderBy(x => x))
        {
            var p = mcg.Nodes[n].Position;
            float best = float.MaxValue;

            foreach (var room in map.Rooms)
            {
                if (room.Nvm == null)
                    continue;

                for (int t = 0; t < room.Nvm.Triangles.Count; t++)
                {
                    if ((room.Nvm.Triangles[t].Flags & NVM.TriangleFlags.GATE) == 0)
                        continue;

                    var tri = room.Nvm.Triangles[t];
                    if (NvmGeometry.VertexIndices(tri).Any(v => v < 0 || v >= room.Nvm.Vertices.Count))
                        continue;

                    best = Math.Min(best, Vector3.Distance(Vector3.Transform(NvmGeometry.Centre(room.Nvm, t), room.World), p));
                }
            }

            if (best > 5f)
                add(NavIssueSeverity.Warning, NavIssueCode.McgNodeOffGate, file, null, null, n, best == float.MaxValue ? "-" : best.ToString("0.0"));
        }
    }
}
