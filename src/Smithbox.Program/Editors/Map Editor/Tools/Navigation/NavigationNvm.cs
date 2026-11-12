using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The navigation data of DeS, DS1 and DS1R (family C of the Navigation Editor Other Games Plan, Phase N6), joined as an
// NvmMap: the MSB's navmesh parts, the NVM file each loads, the map's MCP (rooms) and MCG (graph). What links them was
// surveyed on every map (NavSurvey `nvm <GAME> links`):
// - room r of the MCP is the map's r-th MSB navmesh part (own rooms come first, LocalIndex = r), and its box is the
//   part-placed NVM root box with 1 m added below and above (DS1R 791 of 792, DeS 558 of 581);
// - an NVM's vertices are in the part's space: 339 of 792 DS1R parts are placed, rotations in degrees, applied Y·Z·X;
// - MCG nodes sit on gate triangles (map space), and an edge's UnkIndicesA/B are the gate triangles at its A and B ends
//   in the edge's room (MCPRoomIndex);
// - DS1/DS1R binder files are named model + "A##" (n0000B0 → n0000B0A10.nvm); DeS names them after the model.
// No renderer dependencies, so NavSurvey links it.

/// <summary>The NVM games and how their files are laid out.</summary>
public sealed class NvmGameProfile
{
    public string Game { get; init; }

    /// <summary>DeS is big-endian.</summary>
    public bool BigEndian { get; init; }

    /// <summary>The binder is DCX compressed (DS1R).</summary>
    public bool CompressedBinder { get; init; }

    /// <summary>The game also ships loose NVM files next to the binder (DeS).</summary>
    public bool LooseFiles { get; init; }

    /// <summary>The MSB folder's name (DeS: mapstudio).</summary>
    public string MsbFolder { get; init; }

    public static readonly NvmGameProfile DES = new() { Game = "DES", BigEndian = true, LooseFiles = true, MsbFolder = "mapstudio" };
    public static readonly NvmGameProfile DS1 = new() { Game = "DS1", MsbFolder = "MapStudio" };
    public static readonly NvmGameProfile DS1R = new() { Game = "DS1R", CompressedBinder = true, MsbFolder = "MapStudio" };

    /// <summary>The profile of an NVM game, or null for any other game.</summary>
    public static NvmGameProfile For(string game) => game switch
    {
        "DES" => DES,
        "DS1" => DS1,
        "DS1R" => DS1R,
        _ => null,
    };

    public string BinderPath(string mapId) => $"/map/{mapId}/{mapId}.nvmbnd" + (CompressedBinder ? ".dcx" : "");

    public string McpPath(string mapId) => $"/map/{mapId}/{mapId}.mcp";

    public string McgPath(string mapId) => $"/map/{mapId}/{mapId}.mcg";

    public string MsbPath(string mapId) => $"/map/{MsbFolder}/{mapId}.msb" + (CompressedBinder ? ".dcx" : "");

    public string LoosePath(string mapId, string fileName) => $"/map/{mapId}/{fileName}.nvm";

    /// <summary>The folder the game's binder files are named under, for a binder that has no file to copy it from.</summary>
    public string BinderFolder(string mapId) => Game switch
    {
        "DES" => $@"N:\DemonsSoul\data\DVDROOT\map\{mapId}\",
        "DS1" => $@"N:\FRPG\data\INTERROOT_win32\map\{mapId}\",
        _ => $@"N:\FRPG\data\INTERROOT_x64\map\{mapId}\",
    };
}

/// <summary>One MSB navmesh part and the NVM it loads: room <see cref="Index"/> of the map's MCP.</summary>
public sealed class NvmRoom
{
    /// <summary>The part's position among the map's navmesh parts, which is its MCP room.</summary>
    public int Index { get; init; }

    public string PartName { get; init; }

    public string ModelName { get; init; }

    /// <summary>The binder file it loads, without extension; null if the binder has none for its model.</summary>
    public string FileName { get; init; }

    /// <summary>Where the part places its NVM (model to map space).</summary>
    public Matrix4x4 World { get; init; } = Matrix4x4.Identity;

    /// <summary>The NVM, or null if the binder has no file for the part.</summary>
    public NVM Nvm { get; init; }

    /// <summary>The MSB part object, when built from the Map Editor (null in NavSurvey).</summary>
    public object Part { get; init; }
}

/// <summary>A map's NVM navigation data. Built anew from the documents, so it reflects the latest edits.</summary>
public sealed class NvmMap
{
    public string MapId { get; init; }

    public NvmGameProfile Profile { get; init; }

    /// <summary>The map's ID as the MCP and MCG store it (mAA_BB_CC_DD → 0xAABBCCDD).</summary>
    public uint MapUid { get; init; }

    public List<NvmRoom> Rooms { get; init; } = new();

    /// <summary>Every NVM of the binder by file name (without extension), including those no part loads.</summary>
    public Dictionary<string, NVM> Files { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public MCP Mcp { get; init; }

    public MCG Mcg { get; init; }

    /// <summary>The room of an MCP room index, if it is one of this map's (own rooms come first).</summary>
    public NvmRoom RoomOfMcp(int mcpRoom)
    {
        if (Mcp == null || mcpRoom < 0 || mcpRoom >= Mcp.Rooms.Count)
            return null;

        var room = Mcp.Rooms[mcpRoom];
        if (room.MapID != MapUid || room.LocalIndex < 0 || room.LocalIndex >= Rooms.Count)
            return null;

        return Rooms[room.LocalIndex];
    }

    /// <summary>The MCP index of a room of this map, or -1.</summary>
    public int McpIndexOf(NvmRoom room)
    {
        if (Mcp == null)
            return -1;

        for (int i = 0; i < Mcp.Rooms.Count; i++)
        {
            if (Mcp.Rooms[i].MapID == MapUid && Mcp.Rooms[i].LocalIndex == room.Index)
                return i;
        }

        return -1;
    }

    public static uint UidOf(string mapId)
    {
        var p = mapId.Substring(1).Split('_');
        if (p.Length < 4)
            return 0;

        return (uint)(byte.Parse(p[0]) << 24 | byte.Parse(p[1]) << 16 | byte.Parse(p[2]) << 8 | byte.Parse(p[3]));
    }

    /// <summary>
    /// The binder file a part's model loads: the file named as the model (DeS), or the model + "A##" (DS1, DS1R).
    /// </summary>
    public static string FileFor(string model, IEnumerable<string> fileNames)
    {
        if (string.IsNullOrEmpty(model))
            return null;

        string prefixed = null;

        foreach (var name in fileNames)
        {
            if (name.Equals(model, StringComparison.OrdinalIgnoreCase))
                return name;

            if (prefixed == null && name.Length == model.Length + 3 && name.StartsWith(model, StringComparison.OrdinalIgnoreCase)
                && (name[model.Length] is 'A' or 'a'))
                prefixed = name;
        }

        return prefixed;
    }
}

/// <summary>The MSB placement of navmesh parts, renderer-free (the Map Editor uses the entity's own matrix).</summary>
public static class NvmPlacement
{
    /// <summary>
    /// A part's model-to-map matrix: MSB1/MSBD rotations are in degrees, applied Y·Z·X as Smithbox's transform does (the
    /// placed MCP room boxes of all 109 rotated DS1R and DeS parts but 2 fit it).
    /// </summary>
    public static Matrix4x4 PartWorld(Vector3 position, Vector3 rotationDegrees, Vector3 scale)
    {
        var r = rotationDegrees * (MathF.PI / 180f);
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, r.Y)
            * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, r.Z)
            * Quaternion.CreateFromAxisAngle(Vector3.UnitX, r.X);

        return Matrix4x4.CreateScale(scale == default ? Vector3.One : scale) * Matrix4x4.CreateFromQuaternion(q) * Matrix4x4.CreateTranslation(position);
    }
}

/// <summary>Reads a map's NVM data straight from the game or project files (the vanilla baseline, NavSurvey).</summary>
public static class NvmReader
{
    public sealed record PartInfo(string Name, string Model, Vector3 Position, Vector3 Rotation, Vector3 Scale, object Part);

    /// <summary>The navmesh parts of an MSB, in MSB order.</summary>
    public static List<PartInfo> NavmeshParts(byte[] msb, NvmGameProfile profile)
    {
        if (profile.BigEndian)
            return MSBD.Read(msb).Parts.Navmeshes.Select(p => new PartInfo(p.Name, p.ModelName, p.Position, p.Rotation, p.Scale, p)).ToList();

        return MSB1.Read(msb).Parts.Navmeshes.Select(p => new PartInfo(p.Name, p.ModelName, p.Position, p.Rotation, p.Scale, p)).ToList();
    }

    private static byte[] ReadBytes(VirtualFileSystem fs, string path)
    {
        var file = fs.ReadFile(path);

        if (file == null && path.EndsWith(".dcx"))
            file = fs.ReadFile(path[..^4]);

        if (file == null)
            return null;

        var data = file.Value.ToArray();
        return DCX.Is(data) ? DCX.Decompress(data, out DCX.Type _).ToArray() : data;
    }

    /// <summary>The binder's NVMs by file name (without extension), in binder order; empty if it has none.</summary>
    public static List<(string Name, NVM Nvm)> ReadBinder(byte[] binder)
    {
        var list = new List<(string, NVM)>();

        if (binder == null)
            return list;

        foreach (var f in BND3.Read(new Memory<byte>(binder)).Files)
        {
            if (!f.Name.EndsWith(".nvm", StringComparison.OrdinalIgnoreCase))
                continue;

            list.Add((Path.GetFileNameWithoutExtension(f.Name.Replace('\\', '/')), NVM.Read(f.Bytes)));
        }

        return list;
    }

    /// <summary>
    /// A map's NVM data from a file system: its MSB's navmesh parts, its binder, MCP and MCG. Null if it has no binder.
    /// </summary>
    public static NvmMap Read(VirtualFileSystem fs, string mapId, NvmGameProfile profile)
    {
        var binder = ReadBytes(fs, profile.BinderPath(mapId));
        if (binder == null)
            return null;

        var msb = ReadBytes(fs, profile.MsbPath(mapId));
        var parts = msb != null ? NavmeshParts(msb, profile) : new List<PartInfo>();
        var mcp = ReadBytes(fs, profile.McpPath(mapId));
        var mcg = ReadBytes(fs, profile.McgPath(mapId));

        var files = ReadBinder(binder);
        return Build(mapId, profile, files, parts.Select(p => (p, NvmPlacement.PartWorld(p.Position, p.Rotation, p.Scale))).ToList(),
            mcp != null ? MCP.Read(mcp) : null, mcg != null ? MCG.Read(mcg) : null);
    }

    /// <summary>Joins the parts (in MSB order, with their placement) to the binder's files.</summary>
    public static NvmMap Build(string mapId, NvmGameProfile profile, IReadOnlyList<(string Name, NVM Nvm)> files,
        IReadOnlyList<(PartInfo Part, Matrix4x4 World)> parts, MCP mcp, MCG mcg)
    {
        var map = new NvmMap
        {
            MapId = mapId,
            Profile = profile,
            MapUid = NvmMap.UidOf(mapId),
            Mcp = mcp,
            Mcg = mcg,
        };

        foreach (var (name, nvm) in files)
            map.Files[name] = nvm;

        var names = files.Select(f => f.Name).ToList();

        for (int i = 0; i < parts.Count; i++)
        {
            var (part, world) = parts[i];
            var file = NvmMap.FileFor(part.Model, names);

            map.Rooms.Add(new NvmRoom
            {
                Index = i,
                PartName = part.Name,
                ModelName = part.Model,
                FileName = file,
                World = world,
                Nvm = file != null ? map.Files[file] : null,
                Part = part.Part,
            });
        }

        return map;
    }
}

/// <summary>Geometry of an NVM triangle, in its part's space or placed.</summary>
public static class NvmGeometry
{
    public static (Vector3 A, Vector3 B, Vector3 C) Corners(NVM nvm, int triangle, IReadOnlyList<Vector3> vertices = null)
    {
        var t = nvm.Triangles[triangle];
        var v = vertices ?? nvm.Vertices;
        return (v[t.VertexIndex1], v[t.VertexIndex2], v[t.VertexIndex3]);
    }

    public static Vector3 Centre(NVM nvm, int triangle, IReadOnlyList<Vector3> vertices = null)
    {
        var (a, b, c) = Corners(nvm, triangle, vertices);
        return (a + b + c) / 3;
    }

    /// <summary>The triangle's vertex indices, in order.</summary>
    public static int[] VertexIndices(NVM.Triangle t) => new[] { t.VertexIndex1, t.VertexIndex2, t.VertexIndex3 };

    /// <summary>The neighbour across each edge (1-2, 2-3, 1-3), -1 where there is none.</summary>
    public static int[] Neighbours(NVM.Triangle t) => new[] { t.EdgeIndex1, t.EdgeIndex2, t.EdgeIndex3 };

    /// <summary>The vertex pair of each edge, in the order of <see cref="Neighbours"/>.</summary>
    public static (int, int)[] Edges(NVM.Triangle t) => new[] { (t.VertexIndex1, t.VertexIndex2), (t.VertexIndex2, t.VertexIndex3), (t.VertexIndex1, t.VertexIndex3) };

    /// <summary>
    /// The root box the game gives an NVM: its vertex bounds grown by a padding on every side (0.5 m in all DS1R files
    /// and 1,420 of 1,520 DeS ones, none in the other 100). The padding of the existing root is kept.
    /// </summary>
    public static (Vector3 Min, Vector3 Max) RootBox(IReadOnlyList<Vector3> vertices, float padding)
    {
        if (vertices.Count == 0)
            return (Vector3.Zero, Vector3.Zero);

        var min = vertices[0];
        var max = vertices[0];

        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return (min - new Vector3(padding), max + new Vector3(padding));
    }

    /// <summary>The padding an NVM's root box has over its vertices: 0.5 or 0, as the game files have them.</summary>
    public static float RootPadding(NVM nvm)
    {
        if (nvm.Vertices.Count == 0 || nvm.RootBox == null)
            return 0.5f;

        var min = nvm.Vertices.Aggregate(Vector3.Min);
        return min.X - nvm.RootBox.MinValueCorner.X > 0.25f ? 0.5f : 0f;
    }

    /// <summary>The map-space bounds of a part-placed box (all eight corners transformed).</summary>
    public static (Vector3 Min, Vector3 Max) Placed(Vector3 min, Vector3 max, Matrix4x4 world)
    {
        var lo = new Vector3(float.MaxValue);
        var hi = new Vector3(float.MinValue);

        for (int k = 0; k < 8; k++)
        {
            var corner = new Vector3((k & 1) != 0 ? max.X : min.X, (k & 2) != 0 ? max.Y : min.Y, (k & 4) != 0 ? max.Z : min.Z);
            var p = Vector3.Transform(corner, world);
            lo = Vector3.Min(lo, p);
            hi = Vector3.Max(hi, p);
        }

        return (lo, hi);
    }

    /// <summary>The MCP room box the game gives a room: its placed NVM root box, with the room's Y padding.</summary>
    public static (Vector3 Min, Vector3 Max) RoomBox(NVM nvm, Matrix4x4 world, float padBelow = 1f, float padAbove = 1f)
    {
        var (min, max) = Placed(nvm.RootBox.MinValueCorner, nvm.RootBox.MaxValueCorner, world);
        return (min - new Vector3(0, padBelow, 0), max + new Vector3(0, padAbove, 0));
    }
}

/// <summary>
/// The NVM mode's edits, as copies: each returns new documents and leaves the ones given as they were, so an action
/// can swap them in and out (undo). Unedited files are never rewritten.
/// </summary>
public static class NvmEdit
{
    public static NVM Copy(NVM nvm)
    {
        var copy = NVM.Read(nvm.Write());
        copy.fileName = nvm.fileName;
        return copy;
    }

    public static MCP Copy(MCP mcp) => mcp == null ? null : MCP.Read(mcp.Write());

    public static MCG Copy(MCG mcg) => mcg == null ? null : MCG.Read(mcg.Write());

    /// <summary>A copy with the triangles' flags changed by the function.</summary>
    public static NVM WithFlags(NVM nvm, IReadOnlyDictionary<int, NVM.TriangleFlags> flags)
    {
        var copy = Copy(nvm);

        foreach (var (t, f) in flags)
            copy.Triangles[t].Flags = f;

        return copy;
    }

    /// <summary>
    /// A copy with new vertex positions (topology kept), its root box (keeping its padding) and box tree rebuilt the
    /// game's way (NvmBoxRule).
    /// </summary>
    public static NVM WithVertices(NVM nvm, IReadOnlyList<Vector3> vertices)
    {
        var copy = Copy(nvm);
        float padding = NvmGeometry.RootPadding(nvm);

        for (int i = 0; i < copy.Vertices.Count; i++)
            copy.Vertices[i] = vertices[i];

        RebuildBoxes(copy, padding);
        return copy;
    }

    /// <summary>Rebuilds an NVM's root box from its vertices and its box tree under it, in place.</summary>
    public static void RebuildBoxes(NVM nvm, float padding)
    {
        var (min, max) = NvmGeometry.RootBox(nvm.Vertices, padding);
        nvm.RootBox = NvmBoxRule.Rebuild(nvm, min, max);
    }

    /// <summary>A copy with an entity's triangle list and ID set; a null list removes the entity, an index past the end adds one.</summary>
    public static NVM WithEntity(NVM nvm, int entity, int entityId, IReadOnlyCollection<int> triangles)
    {
        var copy = Copy(nvm);

        if (triangles == null)
        {
            if (entity >= 0 && entity < copy.Entities.Count)
                copy.Entities.RemoveAt(entity);
            return copy;
        }

        var sorted = triangles.Distinct().OrderBy(t => t).ToList();

        if (entity >= 0 && entity < copy.Entities.Count)
        {
            copy.Entities[entity].EntityID = entityId;
            copy.Entities[entity].TriangleIndices = sorted;
        }
        else
        {
            copy.Entities.Add(new NVM.Entity { EntityID = entityId, TriangleIndices = sorted });
        }

        return copy;
    }

    /// <summary>
    /// The MCP room boxes of the rooms given, set from their NVMs' root boxes as the game has them (placed by the part,
    /// keeping each room's own Y padding when it has the game's form, else 1 m). Returns a copy, or the same MCP when no
    /// box changes.
    /// </summary>
    public static MCP WithRoomBoxes(NvmMap map, MCP mcp, IEnumerable<(NvmRoom Room, NVM Before, NVM After)> rooms)
    {
        if (mcp == null)
            return null;

        MCP copy = null;

        foreach (var (room, before, after) in rooms)
        {
            int index = map.McpIndexOf(room);
            if (index < 0 || after == null)
                continue;

            var current = (copy ?? mcp).Rooms[index];
            var (oldMin, oldMax) = NvmGeometry.Placed(before.RootBox.MinValueCorner, before.RootBox.MaxValueCorner, room.World);

            // The room's own padding where its box fits its NVM's (a few DeS rooms have 1.5 m); else the usual 1 m
            float below = oldMin.Y - current.BoundingBoxMin.Y, above = current.BoundingBoxMax.Y - oldMax.Y;
            bool fits = Math.Abs(oldMin.X - current.BoundingBoxMin.X) < 0.1f && Math.Abs(oldMax.X - current.BoundingBoxMax.X) < 0.1f
                && Math.Abs(oldMin.Z - current.BoundingBoxMin.Z) < 0.1f && Math.Abs(oldMax.Z - current.BoundingBoxMax.Z) < 0.1f
                && below is >= 0 and <= 3 && above is >= 0 and <= 3;

            var (min, max) = NvmGeometry.RoomBox(after, room.World, fits ? below : 1f, fits ? above : 1f);

            if (min == current.BoundingBoxMin && max == current.BoundingBoxMax)
                continue;

            copy ??= Copy(mcp);
            copy.Rooms[index].BoundingBoxMin = min;
            copy.Rooms[index].BoundingBoxMax = max;
        }

        return copy ?? mcp;
    }

    /// <summary>
    /// The triangles of this map an MCG node sits on: for each edge of the node in one of this map's rooms, the gate
    /// triangles the edge lists at the node's end (UnkIndicesA at A, UnkIndicesB at B).
    /// </summary>
    public static List<(NvmRoom Room, int Triangle)> NodeTriangles(NvmMap map, int node)
    {
        var list = new List<(NvmRoom, int)>();
        var mcg = map.Mcg;

        if (mcg == null || node < 0 || node >= mcg.Nodes.Count)
            return list;

        foreach (var e in mcg.Nodes[node].ConnectedEdgeIndices)
        {
            if (e < 0 || e >= mcg.Edges.Count)
                continue;

            var edge = mcg.Edges[e];
            var room = map.RoomOfMcp(edge.MCPRoomIndex);

            if (room?.Nvm == null)
                continue;

            var triangles = edge.NodeIndexA == node ? edge.UnkIndicesA : edge.NodeIndexB == node ? edge.UnkIndicesB : null;

            foreach (var t in triangles ?? new List<int>())
            {
                if (t >= 0 && t < room.Nvm.Triangles.Count && !list.Contains((room, t)))
                    list.Add((room, t));
            }
        }

        return list;
    }

    /// <summary>
    /// Moves the MCG nodes on the triangles of moved rooms by the mean move of those triangles' centres (map space), and
    /// scales each edge's cost by the change in the distance between its nodes, keeping the game's detour (as the family
    /// B cost graph does). Returns a copy, or the same MCG when nothing moves.
    /// </summary>
    public static MCG WithNodesFollowing(NvmMap map, MCG mcg, IReadOnlyDictionary<NvmRoom, NVM> after, out int movedNodes)
    {
        movedNodes = 0;

        if (mcg == null || after.Count == 0)
            return mcg;

        var moved = new Dictionary<int, Vector3>();

        for (int n = 0; n < mcg.Nodes.Count; n++)
        {
            var sum = Vector3.Zero;
            int count = 0;
            bool any = false;

            foreach (var (room, t) in NodeTriangles(map, n))
            {
                var placedBefore = Vector3.Transform(NvmGeometry.Centre(room.Nvm, t), room.World);

                if (after.TryGetValue(room, out var edited) && edited != null)
                {
                    sum += Vector3.Transform(NvmGeometry.Centre(edited, t), room.World) - placedBefore;
                    any = true;
                }

                count++;
            }

            if (!any || count == 0)
                continue;

            var delta = sum / count;
            if (delta.LengthSquared() > 1e-12f)
                moved[n] = mcg.Nodes[n].Position + delta;
        }

        if (moved.Count == 0)
            return mcg;

        var copy = Copy(mcg);

        foreach (var (n, position) in moved)
            copy.Nodes[n].Position = position;

        foreach (var edge in copy.Edges)
        {
            if (!moved.ContainsKey(edge.NodeIndexA) && !moved.ContainsKey(edge.NodeIndexB))
                continue;

            float before = Vector3.Distance(mcg.Nodes[edge.NodeIndexA].Position, mcg.Nodes[edge.NodeIndexB].Position);
            float now = Vector3.Distance(copy.Nodes[edge.NodeIndexA].Position, copy.Nodes[edge.NodeIndexB].Position);

            if (before > 1e-4f)
                edge.Unk20 *= now / before;
        }

        movedNodes = moved.Count;
        return copy;
    }

    /// <summary>A copy with a node moved; the costs of its edges scale with their length, as above.</summary>
    public static MCG WithNodePosition(MCG mcg, int node, Vector3 position)
    {
        var copy = Copy(mcg);
        copy.Nodes[node].Position = position;

        foreach (var edge in copy.Edges)
        {
            if (edge.NodeIndexA != node && edge.NodeIndexB != node)
                continue;

            float before = Vector3.Distance(mcg.Nodes[edge.NodeIndexA].Position, mcg.Nodes[edge.NodeIndexB].Position);
            float now = Vector3.Distance(copy.Nodes[edge.NodeIndexA].Position, copy.Nodes[edge.NodeIndexB].Position);

            if (before > 1e-4f)
                edge.Unk20 *= now / before;
        }

        return copy;
    }

    /// <summary>A copy with an edge's cost (Unk20) set.</summary>
    public static MCG WithEdgeCost(MCG mcg, int edge, float cost)
    {
        var copy = Copy(mcg);
        copy.Edges[edge].Unk20 = cost;
        return copy;
    }

    /// <summary>
    /// A copy without an edge: the nodes drop their connection through it, and later edges' indices in the nodes' lists
    /// shift down by one.
    /// </summary>
    public static MCG WithoutEdge(MCG mcg, int edge)
    {
        var copy = Copy(mcg);
        copy.Edges.RemoveAt(edge);

        foreach (var node in copy.Nodes)
        {
            for (int i = node.ConnectedEdgeIndices.Count - 1; i >= 0; i--)
            {
                if (node.ConnectedEdgeIndices[i] == edge)
                {
                    node.ConnectedEdgeIndices.RemoveAt(i);
                    node.ConnectedNodeIndices.RemoveAt(i);
                }
                else if (node.ConnectedEdgeIndices[i] > edge)
                {
                    node.ConnectedEdgeIndices[i]--;
                }
            }
        }

        return copy;
    }

    /// <summary>
    /// A copy with a new edge between two nodes in a room (by MCP index), at the end of the edge list, its node with the
    /// lower index as A (as every game edge has it); each node lists the other at the end of its connections. The gate
    /// triangles at each end are those of the room nearest each node, within 2 m.
    /// </summary>
    public static MCG WithEdge(NvmMap map, MCG mcg, int nodeA, int nodeB, int mcpRoom, float cost)
    {
        if (nodeA > nodeB)
            (nodeA, nodeB) = (nodeB, nodeA);

        var copy = Copy(mcg);
        var room = map.RoomOfMcp(mcpRoom);
        int index = copy.Edges.Count;

        copy.Edges.Add(new MCG.Edge
        {
            NodeIndexA = nodeA,
            NodeIndexB = nodeB,
            MCPRoomIndex = mcpRoom,
            MapID = map.Mcp != null && mcpRoom >= 0 && mcpRoom < map.Mcp.Rooms.Count ? map.Mcp.Rooms[mcpRoom].MapID : map.MapUid,
            Unk20 = cost,
            UnkIndicesA = GateTrianglesNear(room, copy.Nodes[nodeA].Position, 2f),
            UnkIndicesB = GateTrianglesNear(room, copy.Nodes[nodeB].Position, 2f),
        });

        copy.Nodes[nodeA].ConnectedNodeIndices.Add(nodeB);
        copy.Nodes[nodeA].ConnectedEdgeIndices.Add(index);
        copy.Nodes[nodeB].ConnectedNodeIndices.Add(nodeA);
        copy.Nodes[nodeB].ConnectedEdgeIndices.Add(index);
        return copy;
    }

    /// <summary>
    /// The gate triangles of a room joined to the gate triangle nearest a map-space point (within the distance), as the
    /// game's edges list them; empty if there is none.
    /// </summary>
    public static List<int> GateTrianglesNear(NvmRoom room, Vector3 point, float within)
    {
        var list = new List<int>();

        if (room?.Nvm == null)
            return list;

        var nvm = room.Nvm;
        int best = -1;
        float bestDistance = within;

        for (int t = 0; t < nvm.Triangles.Count; t++)
        {
            if ((nvm.Triangles[t].Flags & NVM.TriangleFlags.GATE) == 0)
                continue;

            float d = Vector3.Distance(Vector3.Transform(NvmGeometry.Centre(nvm, t), room.World), point);
            if (d <= bestDistance)
            {
                bestDistance = d;
                best = t;
            }
        }

        if (best < 0)
            return list;

        // The gate triangles joined to it, as one gate
        var stack = new Stack<int>();
        var seen = new HashSet<int> { best };
        stack.Push(best);

        while (stack.Count > 0)
        {
            int t = stack.Pop();
            list.Add(t);

            foreach (var n in NvmGeometry.Neighbours(nvm.Triangles[t]))
            {
                if (n >= 0 && n < nvm.Triangles.Count && (nvm.Triangles[n].Flags & NVM.TriangleFlags.GATE) != 0 && seen.Add(n))
                    stack.Push(n);
            }
        }

        list.Sort();
        return list;
    }
}
