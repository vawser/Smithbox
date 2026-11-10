using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Re-stitching a map's connections (Navigation Editor Plan, Phase 3): finding the connections whose edges no longer fit
// and the open boundary edges that fit each other, then making those changes in the NVA and NVC files. Kept free of the
// rest of Smithbox; NavSurvey `stitchedit` runs it on every map.

/// <summary>A connection of a map's data: where it is stored (the NVA when Nvc is null), its two navmeshes and the connection.</summary>
public sealed record NavStoredConnection(NvcFile Nvc, string MapA, int MainNameID, string MapB, int TargetNameID, NVA.NavMeshConnection Connection);

/// <summary>
/// What re-stitching one set of a map would change: the connections whose edges no longer fit, and the pairs of open
/// boundary edges that fit and would be joined.
/// </summary>
public sealed class NavStitchPlan
{
    public string MapId { get; init; }

    public NavmeshSet Set { get; init; }

    /// <summary>How many connections were looked at: those of the navmeshes in scope, with both meshes at hand.</summary>
    public int Checked { get; set; }

    /// <summary>Connections whose indices no longer fit the meshes, or whose edges are far apart.</summary>
    public List<NavStoredConnection> Broken { get; } = new();

    /// <summary>For each broken connection, how far apart its edges are; float.MaxValue when its indices don't fit the meshes.</summary>
    public Dictionary<NVA.NavMeshConnection, float> BrokenGaps { get; } = new();

    /// <summary>The pairs to join, one per direction stored: two for a pair within the map, one for a pair across maps.</summary>
    public List<NavStitchPair> Added { get; } = new();

    /// <summary>Of the added pairs, how many are the missing reverse of a kept in-map connection.</summary>
    public int MissingReverse { get; set; }

    /// <summary>The neighbour maps whose NVC file with this map was looked at (loaded, with their navmesh files).</summary>
    public List<string> Neighbours { get; } = new();

    public int AddedInMap => Added.Count(p => p.MapA == p.MapB);

    public int AddedCrossMap => Added.Count(p => p.MapA != p.MapB);

    public bool IsEmpty => Broken.Count == 0 && Added.Count == 0;
}

/// <summary>
/// The connectors of a map's NVA and of its NVC files, copied, so a re-stitch can be undone exactly.
/// </summary>
public sealed class NavConnectionState
{
    private readonly NVA Nva;
    private readonly List<NVA.Connector> NvaCopy;
    private readonly List<(NvcFile File, List<NVC.Connector> Copy)> NvcCopies;

    public NavConnectionState(NVA nva, IEnumerable<NvcFile> nvcs)
    {
        Nva = nva;
        NvaCopy = nva.Connectors.Select(Copy).ToList();
        NvcCopies = nvcs.Select(n => (n, n.Nvc.Connectors.Select(Copy).ToList())).ToList();
    }

    private static NVA.Connector Copy(NVA.Connector c) => new()
    {
        MainNameID = c.MainNameID,
        TargetNameID = c.TargetNameID,
        Unk14 = c.Unk14,
        Unk1C = c.Unk1C,
        NavMeshConnections = c.NavMeshConnections.Select(Copy).ToList(),
        GraphConnections = c.GraphConnections.Select(Copy).ToList(),
    };

    private static NVC.Connector Copy(NVC.Connector c) => new()
    {
        MainNameID = c.MainNameID,
        TargetNameID = c.TargetNameID,
        Unk14 = c.Unk14,
        Unk1C = c.Unk1C,
        NavMeshConnections = c.NavMeshConnections.Select(Copy).ToList(),
        GraphConnections = c.GraphConnections.Select(Copy).ToList(),
    };

    private static NVA.NavMeshConnection Copy(NVA.NavMeshConnection c) => new()
    {
        FaceIndex = c.FaceIndex,
        EdgeIndex = c.EdgeIndex,
        OppositeFaceIndex = c.OppositeFaceIndex,
        OppositeEdgeIndex = c.OppositeEdgeIndex,
    };

    private static NVA.GraphConnection Copy(NVA.GraphConnection g) => new() { NodeIndex = g.NodeIndex, OppositeNodeIndex = g.OppositeNodeIndex };

    /// <summary>Puts the connectors back as they were when copied, as new copies, so the state can be restored again.</summary>
    public void Restore()
    {
        Nva.Connectors.Clear();
        Nva.Connectors.AddRange(NvaCopy.Select(Copy));

        foreach (var (file, copy) in NvcCopies)
        {
            file.Nvc.Connectors.Clear();
            file.Nvc.Connectors.AddRange(copy.Select(Copy));
        }
    }
}

public static class NavStitchPlanner
{
    /// <summary>
    /// How far apart the two edges of a kept connection may be. The game's own are at most 1.39 m apart within a map, and
    /// up to 2.8 m across maps in the coarser o set (NavSurvey stitch, stitchedit), so only connections whose edges have
    /// really moved apart count as broken.
    /// </summary>
    public const float BrokenGap = 5f;

    /// <summary>
    /// Where map B's origin is in map A's space, for two open world tiles of the same area (m60 or m61), or null.
    /// Scale 1 and 2 tiles are centred on their 2 × 2 and 4 × 4 small tiles, as the Map Editor places them. Every NVC
    /// file of the game pairs two small tiles (_00 or _10) of one area.
    /// </summary>
    public static Vector3? TileOffset(string a, string b)
    {
        static (int X, int Z, int Scale, string Area)? Tile(string m)
        {
            if (!NavigationMap.IsOpenWorld(m) || m.Length < 12)
                return null;
            return (int.Parse(m.Substring(4, 2)), int.Parse(m.Substring(7, 2)), m[11] - '0', m[..3]);
        }

        static Vector3 Origin((int X, int Z, int Scale, string Area) t)
        {
            int f = t.Scale == 1 ? 2 : t.Scale == 2 ? 4 : 1;
            var centre = t.Scale == 1 ? new Vector3(128, 0, 128) : t.Scale == 2 ? new Vector3(384, 0, 384) : Vector3.Zero;
            return new Vector3(t.X * f * 256, 0, t.Z * f * 256) + centre;
        }

        var ta = Tile(a);
        var tb = Tile(b);
        if (ta == null || tb == null || ta.Value.Area != tb.Value.Area)
            return null;

        return Origin(tb.Value) - Origin(ta.Value);
    }

    /// <summary>
    /// A map's meshes of a set that have faces and that the game joins, placed at an offset. Entries with Unk4C = 1 are
    /// left out: no game connection names one (AC6: 0 of 115,572, NavSurvey stitch), and AC6's 42 such entries with faces
    /// (ModelID 900000 and up) lie over the other meshes, so joining them made 17,028 of the 21,048 pairs the game leaves
    /// apart. In ER, NR and DB_NT every Unk4C = 1 entry has no faces, so nothing changes there.
    /// </summary>
    public static List<NavStitchMesh> Meshes(NavigationMap map, NavmeshSet set, Vector3 offset)
        => NavigationOverlayBuilder.EntriesOf(map, set)
            .Where(e => e.Unk4C != 1)
            .Select(e => (Entry: e, File: map.FileOf(e)))
            .Where(x => x.File?.Mesh != null && x.File.Mesh.m_faces.Count > 0)
            // A rotated or scaled entry (DS3, BB) turns and moves its mesh by its matrix; the rest move by Position only
            .Select(x => NavigationMap.EntryPlacement(x.Entry) is Matrix4x4 turn
                ? new NavStitchMesh(x.Entry.NameID, x.File.Mesh, offset, map.MapId, turn)
                : new NavStitchMesh(x.Entry.NameID, x.File.Mesh, x.Entry.Position + offset, map.MapId))
            .ToList();

    /// <summary>Every connection stored in the map's NVA and in its NVC files.</summary>
    public static IEnumerable<NavStoredConnection> Stored(NavigationMap map)
    {
        foreach (var connector in map.Nva?.Connectors ?? new())
            foreach (var c in connector.NavMeshConnections)
                yield return new NavStoredConnection(null, map.MapId, connector.MainNameID, map.MapId, connector.TargetNameID, c);

        foreach (var nvc in map.Nvcs)
            foreach (var connector in nvc.Nvc.Connectors)
                foreach (var c in connector.NavMeshConnections)
                    yield return new NavStoredConnection(nvc, nvc.MapA, connector.MainNameID, nvc.MapB, connector.TargetNameID, c);
    }

    /// <summary>
    /// Plans a re-stitch of one set of a map. With a NameID, only the connections and pairs of that navmesh of this map
    /// are looked at; else every navmesh of the set. Neighbours (by map name; null when not loaded) are joined through
    /// their existing NVC file with this map; no NVC file is made.
    /// </summary>
    public static NavStitchPlan Plan(NavigationMap map, NavmeshSet set, Func<string, NavigationMap> neighbour, NavStitchParameters parameters, int? onlyNameId = null, bool openEdgesOnly = true)
    {
        var plan = new NavStitchPlan { MapId = map.MapId, Set = set };
        var meshes = Meshes(map, set, Vector3.Zero);

        foreach (var nvc in map.Nvcs)
        {
            var otherId = nvc.Opposite(map.MapId);
            if (otherId == map.MapId || plan.Neighbours.Contains(otherId))
                continue;

            var offset = TileOffset(map.MapId, otherId);
            var other = offset == null ? null : neighbour?.Invoke(otherId);
            if (other?.Nva == null)
                continue;

            plan.Neighbours.Add(otherId);
            meshes.AddRange(Meshes(other, set, offset.Value));
        }

        var placed = new Dictionary<(string, int), NavStitchMesh>();
        foreach (var m in meshes)
            placed[(m.MapId, m.NameID)] = m;

        bool InScope(string mapA, int nameA, string mapB, int nameB)
            => onlyNameId == null || (mapA == map.MapId && nameA == onlyNameId) || (mapB == map.MapId && nameB == onlyNameId);

        // The edges that have a connection, so new pairs only join open edges
        var connected = new HashSet<(string, int, int)>();

        foreach (var stored in Stored(map))
        {
            var c = stored.Connection;
            connected.Add((stored.MapA, stored.MainNameID, c.EdgeIndex));
            connected.Add((stored.MapB, stored.TargetNameID, c.OppositeEdgeIndex));

            // Only this set's, with both meshes at hand
            if (!placed.TryGetValue((stored.MapA, stored.MainNameID), out var a) || !placed.TryGetValue((stored.MapB, stored.TargetNameID), out var b))
                continue;
            if (!InScope(stored.MapA, stored.MainNameID, stored.MapB, stored.TargetNameID))
                continue;

            plan.Checked++;

            float gap = !Fits(a.Mesh, c.FaceIndex, c.EdgeIndex) || !Fits(b.Mesh, c.OppositeFaceIndex, c.OppositeEdgeIndex)
                ? float.MaxValue : NavigationStitch.Gap(a, c.EdgeIndex, b, c.OppositeEdgeIndex);

            if (gap > BrokenGap)
            {
                plan.Broken.Add(stored);
                plan.BrokenGaps[c] = gap;
            }
        }

        var brokenSet = plan.Broken.Select(b => b.Connection).ToHashSet();

        // The game stores every NVA connection in both directions: a kept one without its reverse gets it back
        var nvaKeys = Stored(map).Where(s => s.Nvc == null && !brokenSet.Contains(s.Connection))
            .Select(s => (s.MainNameID, s.Connection.EdgeIndex, s.TargetNameID, s.Connection.OppositeEdgeIndex)).ToHashSet();

        foreach (var stored in Stored(map).Where(s => s.Nvc == null && !brokenSet.Contains(s.Connection)))
        {
            var c = stored.Connection;
            if (nvaKeys.Contains((stored.TargetNameID, c.OppositeEdgeIndex, stored.MainNameID, c.EdgeIndex)))
                continue;
            if (!placed.ContainsKey((map.MapId, stored.MainNameID)) || !placed.ContainsKey((map.MapId, stored.TargetNameID)))
                continue;
            if (!InScope(map.MapId, stored.MainNameID, map.MapId, stored.TargetNameID))
                continue;

            plan.Added.Add(new NavStitchPair(map.MapId, stored.TargetNameID, c.OppositeFaceIndex, c.OppositeEdgeIndex, map.MapId, stored.MainNameID, c.FaceIndex, c.EdgeIndex));
            plan.MissingReverse++;
        }

        // A broken connection's edges are free for a new pair, unless another connection still uses them
        var storedPairs = new HashSet<(string, int, int, string, int, int)>();
        var stillUsed = new HashSet<(string, int, int)>();
        foreach (var stored in Stored(map).Where(s => !brokenSet.Contains(s.Connection)))
        {
            stillUsed.Add((stored.MapA, stored.MainNameID, stored.Connection.EdgeIndex));
            stillUsed.Add((stored.MapB, stored.TargetNameID, stored.Connection.OppositeEdgeIndex));
            storedPairs.Add((stored.MapA, stored.MainNameID, stored.Connection.EdgeIndex, stored.MapB, stored.TargetNameID, stored.Connection.OppositeEdgeIndex));
            storedPairs.Add((stored.MapB, stored.TargetNameID, stored.Connection.OppositeEdgeIndex, stored.MapA, stored.MainNameID, stored.Connection.EdgeIndex));
        }
        connected = stillUsed;

        // Pairs touching this map; across maps, only the direction its NVC file stores (from the file's first map)
        var nvcPairs = map.Nvcs.Select(n => (n.MapA, n.MapB)).ToHashSet();

        foreach (var pair in NavigationStitch.Find(meshes, parameters, (x, y) => x.MapId == map.MapId || y.MapId == map.MapId))
        {
            if (storedPairs.Contains((pair.MapA, pair.NameA, pair.EdgeA, pair.MapB, pair.NameB, pair.EdgeB)))
                continue;
            if (openEdgesOnly && (connected.Contains((pair.MapA, pair.NameA, pair.EdgeA)) || connected.Contains((pair.MapB, pair.NameB, pair.EdgeB))))
                continue;
            if (!InScope(pair.MapA, pair.NameA, pair.MapB, pair.NameB))
                continue;
            if (pair.MapA != pair.MapB && !nvcPairs.Contains((pair.MapA, pair.MapB)))
                continue;

            plan.Added.Add(pair);
        }

        return plan;
    }

    /// <summary>The face exists, and the edge is one of its own and on the boundary.</summary>
    private static bool Fits(IHavokNavMeshEdit mesh, int face, int edge)
    {
        if (face < 0 || face >= mesh.m_faces.Count || edge < 0 || edge >= mesh.m_edges.Count)
            return false;

        var f = mesh.m_faces[face];
        return edge >= f.m_startEdgeIndex && edge < f.m_startEdgeIndex + f.m_numEdges && mesh.m_edges[edge].m_oppositeFace == 0xFFFFFFFF;
    }

    /// <summary>
    /// Makes a plan's changes: removes the broken connections (and connectors left empty), and adds the new pairs, each
    /// in the connector of its two navmeshes, made if needed and kept in the game's order (by unsigned MainNameID, then
    /// TargetNameID). Returns what changed, for NavigationBank.MarkEdited.
    /// </summary>
    public static NavRemapResult Apply(NavigationMap map, NavStitchPlan plan, bool removeBroken, bool addNew)
    {
        var result = new NavRemapResult();

        if (removeBroken)
        {
            foreach (var broken in plan.Broken)
            {
                if (broken.Nvc == null)
                {
                    foreach (var connector in map.Nva.Connectors.Where(c => c.MainNameID == broken.MainNameID && c.TargetNameID == broken.TargetNameID))
                        if (connector.NavMeshConnections.Remove(broken.Connection))
                            result.NvaChanged = true;
                }
                else
                {
                    foreach (var connector in broken.Nvc.Nvc.Connectors.Where(c => c.MainNameID == broken.MainNameID && c.TargetNameID == broken.TargetNameID))
                        if (connector.NavMeshConnections.Remove(broken.Connection))
                            result.NvcsChanged.Add(broken.Nvc);
                }

                result.Dropped++;
            }

            if (map.Nva.Connectors.RemoveAll(c => c.NavMeshConnections.Count == 0 && c.GraphConnections.Count == 0) > 0)
                result.NvaChanged = true;

            foreach (var nvc in result.NvcsChanged)
                nvc.Nvc.Connectors.RemoveAll(c => c.NavMeshConnections.Count == 0 && c.GraphConnections.Count == 0);
        }

        if (addNew)
        {
            foreach (var pair in plan.Added)
            {
                var connection = new NVA.NavMeshConnection
                {
                    FaceIndex = pair.FaceA,
                    EdgeIndex = pair.EdgeA,
                    OppositeFaceIndex = pair.FaceB,
                    OppositeEdgeIndex = pair.EdgeB,
                };

                if (pair.MapA == pair.MapB)
                {
                    ConnectorOf(map.Nva.Connectors, pair.NameA, pair.NameB, c => c.MainNameID, c => c.TargetNameID,
                        () => new NVA.Connector { MainNameID = pair.NameA, TargetNameID = pair.NameB }).NavMeshConnections.Add(connection);
                    result.NvaChanged = true;
                }
                else
                {
                    var nvc = map.Nvcs.First(n => n.MapA == pair.MapA && n.MapB == pair.MapB);
                    ConnectorOf(nvc.Nvc.Connectors, pair.NameA, pair.NameB, c => c.MainNameID, c => c.TargetNameID,
                        () => new NVC.Connector { MainNameID = pair.NameA, TargetNameID = pair.NameB }).NavMeshConnections.Add(connection);
                    result.NvcsChanged.Add(nvc);
                }

                result.Changed++;
            }
        }

        return result;
    }

    /// <summary>The connector of two navmeshes, made and put in the game's order if there is none.</summary>
    private static T ConnectorOf<T>(List<T> connectors, int main, int target, Func<T, int> mainOf, Func<T, int> targetOf, Func<T> make)
    {
        var existing = connectors.FirstOrDefault(c => mainOf(c) == main && targetOf(c) == target);
        if (existing != null)
            return existing;

        var key = ((uint)main, (uint)target);
        int at = connectors.FindIndex(c => ((uint)mainOf(c), (uint)targetOf(c)).CompareTo(key) > 0);
        var made = make();
        connectors.Insert(at < 0 ? connectors.Count : at, made);
        return made;
    }
}
