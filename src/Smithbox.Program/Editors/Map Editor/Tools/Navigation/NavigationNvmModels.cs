using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

// The NVM mode's model edits (Models tab): building an NVM from generated faces, naming a new model, adding and
// removing a map's room in its MCP and MCG, marking the gates where navmeshes meet, and adding and removing MCG nodes.
// The rules come from the game files (NavSurvey `nvm <GAME> models`):
// - triangles wind up (cross(b - a, c - a).Y > 0 for 97 % of DS1R's), and ordinary ones have no flags;
// - models are n####B{BB} (the map's BB), DS1/DS1R files add A{AA}; binder files are sorted by name, with contiguous IDs;
// - MCP connections hold every pair of rooms that share an MCG node (and a few more), both ways, sorted;
// - a gate node joins two rooms; a room's edges are mostly every pair of its nodes.
// Renderer-free, so NavSurvey checks them on the game files (`nvm <GAME> modeledit`).

public static class NvmModelEdit
{
    /// <summary>
    /// The generator settings for an NVM: a 4 m face grid, which gives the game's triangle density on DS1 (2.9 against
    /// the game's 2.7 per m² summed over 6 maps' rooms; ER's 2 m gives twice that, none a third), and no links, which
    /// NVMs don't have. The grid doesn't change how much of the game's navmesh is covered (78 % either way).
    /// </summary>
    public static NavGenParameters DefaultParameters => NavGenParameters.DefaultN with { FaceSize = 4f, Links = false, MaxEdgeLength = 12f };

    /// <summary>
    /// An NVM from polygons in its space (shared corners as the same points, as the generator makes them): corners welded,
    /// each polygon fanned into triangles wound up, neighbours across shared edges, no flags, no entities, and the root
    /// box (0.5 m padding) and box tree the game's way.
    /// </summary>
    public static NVM FromPolygons(IReadOnlyList<IReadOnlyList<Vector3>> polygons, bool bigEndian, float padding = 0.5f)
    {
        var nvm = new NVM { BigEndian = bigEndian, Vertices = new List<Vector3>(), Triangles = new List<NVM.Triangle>(), Entities = new List<NVM.Entity>() };
        var index = new Dictionary<Vector3, int>();

        int Vertex(Vector3 p)
        {
            if (!index.TryGetValue(p, out var i))
            {
                i = nvm.Vertices.Count;
                nvm.Vertices.Add(p);
                index[p] = i;
            }
            return i;
        }

        foreach (var polygon in polygons)
        {
            if (polygon.Count < 3)
                continue;

            var corners = polygon.Select(Vertex).ToList();

            // Repeated corners (a welded sliver) fall out
            corners = corners.Where((c, k) => c != corners[(k + 1) % corners.Count]).ToList();
            if (corners.Count < 3)
                continue;

            for (int k = 1; k + 1 < corners.Count; k++)
            {
                int a = corners[0], b = corners[k], c = corners[k + 1];
                var normal = Vector3.Cross(nvm.Vertices[b] - nvm.Vertices[a], nvm.Vertices[c] - nvm.Vertices[a]);

                if (normal.LengthSquared() < 1e-12f)
                    continue;

                if (normal.Y < 0)
                    (b, c) = (c, b);

                nvm.Triangles.Add(new NVM.Triangle { VertexIndex1 = a, VertexIndex2 = b, VertexIndex3 = c, EdgeIndex1 = -1, EdgeIndex2 = -1, EdgeIndex3 = -1, Flags = NVM.TriangleFlags.NONE });
            }
        }

        LinkNeighbours(nvm);
        NvmEdit.RebuildBoxes(nvm, padding);
        return nvm;
    }

    /// <summary>
    /// A collision part's triangles wound so its floors face up, for the generator (Recast takes a triangle as walkable
    /// only when it faces up). The hkp games' collision is mostly wound the other way: generating over DS1 collision as
    /// read covered 23 of 293 sampled game triangles, flipped 438 of 488. Flips the whole list when most of its area
    /// faces down; returns the list itself otherwise.
    /// </summary>
    public static List<Vector3> FacingUp(List<Vector3> triangles)
    {
        float up = 0, down = 0;
        for (int i = 0; i + 2 < triangles.Count; i += 3)
        {
            float y = Vector3.Cross(triangles[i + 1] - triangles[i], triangles[i + 2] - triangles[i]).Y;
            if (y > 0) up += y; else down -= y;
        }

        if (down <= up)
            return triangles;

        var flipped = new List<Vector3>(triangles.Count);
        for (int i = 0; i + 2 < triangles.Count; i += 3)
        {
            flipped.Add(triangles[i]);
            flipped.Add(triangles[i + 2]);
            flipped.Add(triangles[i + 1]);
        }

        return flipped;
    }

    /// <summary>
    /// Brings a map's NVM binder up to date: removed files out, edited files rewritten, added files in (named and flagged
    /// as the binder's others, under the same folder, or the game's when it has none). After an add or remove the files
    /// are sorted by name and numbered on from the first ID, as 846 of 847 DS1R files are; untouched files keep their bytes.
    /// </summary>
    public static void UpdateBinder(BND3 binder, string mapId, NvmGameProfile profile, Func<string, NVM> nvmOf,
        ICollection<string> edited, ICollection<string> added, ICollection<string> removed)
    {
        static string NameOf(BinderFile f) => System.IO.Path.GetFileNameWithoutExtension(f.Name.Replace('\\', '/'));

        int firstId = binder.Files.Count > 0 ? binder.Files.Min(f => f.ID) : 0;
        var template = binder.Files.FirstOrDefault();
        bool sorted = binder.Files.Select(f => f.Name).SequenceEqual(binder.Files.Select(f => f.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

        binder.Files.RemoveAll(f => removed.Contains(NameOf(f)));

        foreach (var file in binder.Files)
        {
            var name = NameOf(file);
            if (edited.Contains(name) && !added.Contains(name) && nvmOf(name) is NVM nvm)
                file.Bytes = nvm.Write();
        }

        var folder = template != null ? template.Name.Substring(0, template.Name.Replace('/', '\\').LastIndexOf('\\') + 1) : profile.BinderFolder(mapId);

        foreach (var name in added)
        {
            if (binder.Files.Any(f => NameOf(f).Equals(name, StringComparison.OrdinalIgnoreCase)) || nvmOf(name) is not NVM nvm)
                continue;

            binder.Files.Add(new BinderFile(template?.Flags ?? Binder.FileFlags.Flag1, -1, folder + name + ".nvm", nvm.Write()));
        }

        if (added.Count == 0 && removed.Count == 0)
            return;

        if (sorted)
            binder.Files = binder.Files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();

        for (int i = 0; i < binder.Files.Count; i++)
            binder.Files[i].ID = firstId + i;
    }

    /// <summary>Sets every triangle's neighbours from the edges they share (an edge three triangles share is left open).</summary>
    public static void LinkNeighbours(NVM nvm)
    {
        var byEdge = new Dictionary<(int, int), List<(int Triangle, int Slot)>>();

        for (int t = 0; t < nvm.Triangles.Count; t++)
        {
            var edges = NvmGeometry.Edges(nvm.Triangles[t]);
            for (int k = 0; k < 3; k++)
            {
                var key = edges[k].Item1 < edges[k].Item2 ? edges[k] : (edges[k].Item2, edges[k].Item1);
                if (!byEdge.TryGetValue(key, out var list))
                    byEdge[key] = list = new();
                list.Add((t, k));
            }
        }

        foreach (var tri in nvm.Triangles)
        {
            tri.EdgeIndex1 = -1;
            tri.EdgeIndex2 = -1;
            tri.EdgeIndex3 = -1;
        }

        foreach (var list in byEdge.Values)
        {
            if (list.Count != 2)
                continue;

            SetNeighbour(nvm.Triangles[list[0].Triangle], list[0].Slot, list[1].Triangle);
            SetNeighbour(nvm.Triangles[list[1].Triangle], list[1].Slot, list[0].Triangle);
        }
    }

    private static void SetNeighbour(NVM.Triangle t, int slot, int other)
    {
        if (slot == 0) t.EdgeIndex1 = other;
        else if (slot == 1) t.EdgeIndex2 = other;
        else t.EdgeIndex3 = other;
    }

    /// <summary>
    /// The model and binder file names for a new navmesh of a map: n####B{BB} with the next free number (after the
    /// map's highest), and the file the model + A{AA} (DS1, DS1R) or the model (DeS).
    /// </summary>
    public static (string Model, string FileName) NewNames(NvmMap map, IEnumerable<string> takenModels = null)
    {
        var digits = map.MapId.Substring(1).Split('_');
        string area = digits[0], block = int.Parse(digits[1]).ToString();
        var taken = new HashSet<string>(map.Rooms.Select(r => r.ModelName ?? "").Concat(takenModels ?? Array.Empty<string>()), StringComparer.OrdinalIgnoreCase);
        foreach (var file in map.Files.Keys)
            taken.Add(file);

        int highest = -1;
        foreach (var name in taken.Concat(map.Files.Keys))
        {
            var m = Regex.Match(name, @"^n(\d{4})", RegexOptions.IgnoreCase);
            if (m.Success)
                highest = Math.Max(highest, int.Parse(m.Groups[1].Value));
        }

        // After the highest, else the lowest free number (some DeS test maps use n9999)
        foreach (var n in Enumerable.Range(highest + 1, Math.Max(0, 9999 - highest)).Concat(Enumerable.Range(0, Math.Max(0, highest + 1))))
        {
            var model = $"n{n:D4}B{block}";
            var file = map.Profile.BigEndian ? model : $"{model}A{area}";

            if (!taken.Contains(model) && !map.Files.ContainsKey(file))
                return (model, file);
        }

        return (null, null);
    }

    /// <summary>
    /// The MCP and MCG with a room added for a new navmesh part at the end of the map's parts: the room goes after the
    /// map's own rooms (before any neighbour's), boxed from its NVM, with no connections; the later rooms' indices in
    /// the MCP's connections and the MCG's edges shift up by one.
    /// </summary>
    public static (MCP Mcp, MCG Mcg) WithRoomAdded(NvmMap map, NVM nvm, Matrix4x4 world)
    {
        if (map.Mcp == null)
            return (null, map.Mcg);

        var mcp = NvmEdit.Copy(map.Mcp);
        int at = mcp.Rooms.FindLastIndex(r => r.MapID == map.MapUid) + 1;

        foreach (var room in mcp.Rooms)
            for (int i = 0; i < room.ConnectedRoomIndices.Count; i++)
                if (room.ConnectedRoomIndices[i] >= at)
                    room.ConnectedRoomIndices[i]++;

        var (min, max) = nvm.Vertices.Count > 0 ? NvmGeometry.RoomBox(nvm, world) : (world.Translation, world.Translation);
        mcp.Rooms.Insert(at, new MCP.Room { MapID = map.MapUid, LocalIndex = map.Rooms.Count, BoundingBoxMin = min, BoundingBoxMax = max, ConnectedRoomIndices = new List<int>() });

        MCG mcg = map.Mcg;
        if (mcg != null && mcg.Edges.Any(e => e.MCPRoomIndex >= at))
        {
            mcg = NvmEdit.Copy(mcg);
            foreach (var edge in mcg.Edges)
                if (edge.MCPRoomIndex >= at)
                    edge.MCPRoomIndex++;
        }

        return (mcp, mcg);
    }

    /// <summary>
    /// The MCP and MCG without a room of the map (its part being removed): the room's edges go, the later own rooms'
    /// LocalIndex and every later room's index in connections and edges shift down by one. The nodes stay.
    /// </summary>
    public static (MCP Mcp, MCG Mcg) WithRoomRemoved(NvmMap map, int roomIndex)
    {
        var room = roomIndex >= 0 && roomIndex < map.Rooms.Count ? map.Rooms[roomIndex] : null;
        int at = room != null ? map.McpIndexOf(room) : -1;

        if (map.Mcp == null || at < 0)
            return (map.Mcp, map.Mcg);

        var mcp = NvmEdit.Copy(map.Mcp);
        mcp.Rooms.RemoveAt(at);

        foreach (var r in mcp.Rooms)
        {
            if (r.MapID == map.MapUid && r.LocalIndex > roomIndex)
                r.LocalIndex--;

            r.ConnectedRoomIndices = r.ConnectedRoomIndices.Where(c => c != at).Select(c => c > at ? c - 1 : c).ToList();
        }

        var mcg = map.Mcg;
        if (mcg != null)
        {
            for (int e = mcg.Edges.Count - 1; e >= 0; e--)
                if (mcg.Edges[e].MCPRoomIndex == at)
                    mcg = NvmEdit.WithoutEdge(mcg, e);

            if (mcg.Edges.Any(e => e.MCPRoomIndex > at))
            {
                if (ReferenceEquals(mcg, map.Mcg))
                    mcg = NvmEdit.Copy(mcg);

                foreach (var edge in mcg.Edges)
                    if (edge.MCPRoomIndex > at)
                        edge.MCPRoomIndex--;
            }
        }

        return (mcp, mcg);
    }

    /// <summary>
    /// Marks GATE on the triangles of a room whose open edge lies on another room's open edge (both ends within the
    /// tolerance, map space), and on those other triangles too. Returns the NVMs that change, by file name.
    /// </summary>
    public static Dictionary<string, NVM> MarkGates(NvmMap map, NvmRoom room, float tolerance, out int marked)
    {
        marked = 0;
        var result = new Dictionary<string, NVM>(StringComparer.OrdinalIgnoreCase);
        if (room?.Nvm == null)
            return result;

        var mine = OpenEdges(room);
        var flags = new Dictionary<(string File, int Triangle), bool>();

        foreach (var other in map.Rooms)
        {
            if (other.Nvm == null || other.FileName == null || other.Index == room.Index)
                continue;

            // Cheap reject on the placed root boxes
            var (amin, amax) = NvmGeometry.Placed(room.Nvm.RootBox.MinValueCorner, room.Nvm.RootBox.MaxValueCorner, room.World);
            var (bmin, bmax) = NvmGeometry.Placed(other.Nvm.RootBox.MinValueCorner, other.Nvm.RootBox.MaxValueCorner, other.World);
            if (amin.X > bmax.X + tolerance || bmin.X > amax.X + tolerance || amin.Z > bmax.Z + tolerance || bmin.Z > amax.Z + tolerance
                || amin.Y > bmax.Y + tolerance || bmin.Y > amax.Y + tolerance)
                continue;

            var theirs = OpenEdges(other);

            foreach (var (t, a, b) in mine)
            {
                foreach (var (u, c, d) in theirs)
                {
                    bool same = (Vector3.Distance(a, c) <= tolerance && Vector3.Distance(b, d) <= tolerance)
                        || (Vector3.Distance(a, d) <= tolerance && Vector3.Distance(b, c) <= tolerance);
                    if (!same)
                        continue;

                    flags[(room.FileName, t)] = true;
                    flags[(other.FileName, u)] = true;
                }
            }
        }

        foreach (var group in flags.Keys.GroupBy(k => k.File, StringComparer.OrdinalIgnoreCase))
        {
            var nvm = map.Files[group.Key];
            var changes = group.Where(k => (nvm.Triangles[k.Triangle].Flags & NVM.TriangleFlags.GATE) == 0)
                .ToDictionary(k => k.Triangle, k => nvm.Triangles[k.Triangle].Flags | NVM.TriangleFlags.GATE);

            if (changes.Count == 0)
                continue;

            marked += changes.Count;
            result[group.Key] = NvmEdit.WithFlags(nvm, changes);
        }

        return result;
    }

    /// <summary>A room's open edges, placed: (triangle, one end, the other).</summary>
    public static List<(int Triangle, Vector3 A, Vector3 B)> OpenEdges(NvmRoom room)
    {
        var list = new List<(int, Vector3, Vector3)>();
        var nvm = room.Nvm;

        for (int t = 0; t < nvm.Triangles.Count; t++)
        {
            var tri = nvm.Triangles[t];
            var neighbours = NvmGeometry.Neighbours(tri);
            var edges = NvmGeometry.Edges(tri);

            for (int k = 0; k < 3; k++)
            {
                if (neighbours[k] >= 0 || edges[k].Item1 >= nvm.Vertices.Count || edges[k].Item2 >= nvm.Vertices.Count)
                    continue;

                list.Add((t, Vector3.Transform(nvm.Vertices[edges[k].Item1], room.World), Vector3.Transform(nvm.Vertices[edges[k].Item2], room.World)));
            }
        }

        return list;
    }

    /// <summary>
    /// The MCG with the gate triangles of every edge in a room found again (around each end's node, within 2 m), after
    /// the room's NVM was replaced and its triangle numbers changed. The same MCG when no edge is in the room.
    /// </summary>
    public static MCG WithEdgeGatesFound(NvmMap map, MCG mcg, NvmRoom room)
    {
        int mcpRoom = map.McpIndexOf(room);
        if (mcg == null || mcpRoom < 0 || !mcg.Edges.Any(e => e.MCPRoomIndex == mcpRoom))
            return mcg;

        var copy = NvmEdit.Copy(mcg);
        foreach (var edge in copy.Edges.Where(e => e.MCPRoomIndex == mcpRoom))
        {
            edge.UnkIndicesA = NvmEdit.GateTrianglesNear(room, copy.Nodes[edge.NodeIndexA].Position, 2f);
            edge.UnkIndicesB = NvmEdit.GateTrianglesNear(room, copy.Nodes[edge.NodeIndexB].Position, 2f);
        }

        return copy;
    }

    /// <summary>A copy with a node added at the end, with no connections (Unk18 -1, Unk1C 0, as the generator's).</summary>
    public static MCG WithNode(MCG mcg, Vector3 position)
    {
        var copy = NvmEdit.Copy(mcg);
        copy.Nodes.Add(new MCG.Node { Position = position, Unk18 = -1, Unk1C = 0 });
        return copy;
    }

    /// <summary>
    /// A copy without a node: its edges go (see NvmEdit.WithoutEdge), and later nodes' indices shift down by one in the
    /// edges and the nodes' lists. Node Unk18, which may name a node, is shifted down where it names a later one (one DeS
    /// node names the slot past the last node, so one equal to the removed node is left as it is).
    /// </summary>
    public static MCG WithoutNode(MCG mcg, int node)
    {
        var copy = mcg;
        for (int e = copy.Edges.Count - 1; e >= 0; e--)
            if (copy.Edges[e].NodeIndexA == node || copy.Edges[e].NodeIndexB == node)
                copy = NvmEdit.WithoutEdge(copy, e);

        if (ReferenceEquals(copy, mcg))
            copy = NvmEdit.Copy(mcg);

        copy.Nodes.RemoveAt(node);

        foreach (var edge in copy.Edges)
        {
            if (edge.NodeIndexA > node) edge.NodeIndexA--;
            if (edge.NodeIndexB > node) edge.NodeIndexB--;
        }

        foreach (var n in copy.Nodes)
        {
            for (int i = 0; i < n.ConnectedNodeIndices.Count; i++)
                if (n.ConnectedNodeIndices[i] > node)
                    n.ConnectedNodeIndices[i]--;

            if (n.Unk18 > node) n.Unk18--;
        }

        return copy;
    }

    /// <summary>
    /// The nodes of a room: those within the distance of one of its gate triangles' centres (map space).
    /// </summary>
    public static List<int> NodesOnGates(NvmMap map, NvmRoom room, float within)
    {
        var list = new List<int>();
        if (map.Mcg == null || room?.Nvm == null)
            return list;

        var gates = new List<Vector3>();
        for (int t = 0; t < room.Nvm.Triangles.Count; t++)
            if ((room.Nvm.Triangles[t].Flags & NVM.TriangleFlags.GATE) != 0)
                gates.Add(Vector3.Transform(NvmGeometry.Centre(room.Nvm, t), room.World));

        for (int n = 0; n < map.Mcg.Nodes.Count; n++)
            if (gates.Any(g => Vector3.Distance(g, map.Mcg.Nodes[n].Position) <= within))
                list.Add(n);

        return list;
    }

    /// <summary>
    /// The MCG with an edge in a room between every two of its nodes (on its gates) that have none, costing their
    /// distance, as most of the game's rooms have every pair; and the MCP with the room connected to every other room
    /// those nodes have edges in, both ways (the game's MCPs connect every two rooms that share a node).
    /// </summary>
    public static (MCP Mcp, MCG Mcg, int Added) WithRoomLinked(NvmMap map, NvmRoom room, float within)
    {
        int mcpRoom = map.McpIndexOf(room);
        if (map.Mcg == null || mcpRoom < 0)
            return (map.Mcp, map.Mcg, 0);

        var nodes = NodesOnGates(map, room, within);
        var mcg = map.Mcg;
        int added = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                int a = nodes[i], b = nodes[j];
                if (mcg.Edges.Any(e => e.MCPRoomIndex == mcpRoom && ((e.NodeIndexA == a && e.NodeIndexB == b) || (e.NodeIndexA == b && e.NodeIndexB == a))))
                    continue;

                mcg = NvmEdit.WithEdge(map, mcg, a, b, mcpRoom, Vector3.Distance(mcg.Nodes[a].Position, mcg.Nodes[b].Position));
                added++;
            }
        }

        return (WithConnectionsOf(map, map.Mcp, mcg, mcpRoom), mcg, added);
    }

    /// <summary>The MCP with a room connected (both ways, sorted) to every room an edge of one of its nodes is in.</summary>
    public static MCP WithConnectionsOf(NvmMap map, MCP mcp, MCG mcg, int mcpRoom)
    {
        if (mcp == null || mcg == null || mcpRoom < 0 || mcpRoom >= mcp.Rooms.Count)
            return mcp;

        var others = new HashSet<int>();
        var nodes = mcg.Edges.Where(e => e.MCPRoomIndex == mcpRoom).SelectMany(e => new[] { e.NodeIndexA, e.NodeIndexB }).ToHashSet();

        foreach (var e in mcg.Edges)
            if (e.MCPRoomIndex != mcpRoom && e.MCPRoomIndex >= 0 && e.MCPRoomIndex < mcp.Rooms.Count && (nodes.Contains(e.NodeIndexA) || nodes.Contains(e.NodeIndexB)))
                others.Add(e.MCPRoomIndex);

        var missing = others.Where(o => !mcp.Rooms[mcpRoom].ConnectedRoomIndices.Contains(o) || !mcp.Rooms[o].ConnectedRoomIndices.Contains(mcpRoom)).ToList();
        if (missing.Count == 0)
            return mcp;

        var copy = NvmEdit.Copy(mcp);
        foreach (var o in missing)
        {
            if (!copy.Rooms[mcpRoom].ConnectedRoomIndices.Contains(o)) copy.Rooms[mcpRoom].ConnectedRoomIndices.Add(o);
            if (!copy.Rooms[o].ConnectedRoomIndices.Contains(mcpRoom)) copy.Rooms[o].ConnectedRoomIndices.Add(mcpRoom);
            copy.Rooms[mcpRoom].ConnectedRoomIndices.Sort();
            copy.Rooms[o].ConnectedRoomIndices.Sort();
        }

        return copy;
    }
}
