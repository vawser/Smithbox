using Havok.hk2018;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

// The Elden Ring navigation data of a map, joined across its files: the NVA, the navmesh HKX files of its nvmhktbnd,
// its ONAV and the NVC files that connect it to its neighbours. Kept free of the rest of Smithbox, so Tools/NavSurvey
// can build it from the game files and check it on every map. See Documentation/Claude/Navigation Editor Plan.md.

/// <summary>
/// The two navmesh sets of a map. Every navmesh model has one of each: an `n` file and an `o` file in the nvmhktbnd,
/// listed in the first and second half of the NVA's navmeshes. Which characters use which set is unknown.
/// </summary>
public enum NavmeshSet
{
    N,
    O
}

/// <summary>
/// A navmesh HKX file of a map's nvmhktbnd, with the Havok data the validator looks for.
/// </summary>
public sealed class NavmeshFile
{
    /// <summary>The file name without extension, e.g. n10_01_00_00_001000.</summary>
    public string Name { get; init; }

    public hkaiNavMesh Mesh { get; init; }

    /// <summary>The file holds a query mediator (the AABB tree the game queries faces through).</summary>
    public bool HasMediator { get; init; }

    /// <summary>The file holds a clearance cache.</summary>
    public bool HasClearanceCache { get; init; }

    public int FaceCount => Mesh?.m_faces.Count ?? 0;
}

/// <summary>
/// One navmesh model of a map: its NVA entry and file in each set, and the quarters its n mesh may be split into.
/// </summary>
public sealed class NavmeshModel
{
    public int ModelID { get; init; }

    public NVA_ER.Navmesh NEntry { get; init; }

    public NVA_ER.Navmesh OEntry { get; init; }

    public NavmeshFile NFile { get; init; }

    public NavmeshFile OFile { get; init; }

    /// <summary>
    /// The n mesh split into quarters (q files, open world only), each with its own NVA entry. When a model has them,
    /// its n file is empty. Indexed by quarter, 0 to 3.
    /// </summary>
    public IReadOnlyList<(NVA_ER.Navmesh Entry, NavmeshFile File)> Quarters { get; init; } = Array.Empty<(NVA_ER.Navmesh, NavmeshFile)>();

    public NVA_ER.Navmesh Entry(NavmeshSet set) => set == NavmeshSet.N ? NEntry : OEntry;

    public NavmeshFile File(NavmeshSet set) => set == NavmeshSet.N ? NFile : OFile;

    /// <summary>A placeholder: no faces in either set, which the game files mostly mark with Unk4C = 1.</summary>
    public bool IsPlaceholder => (NEntry?.FaceCount ?? 0) == 0 && (OEntry?.FaceCount ?? 0) == 0 && Quarters.Count == 0;
}

/// <summary>
/// An NVC file that names a map, on either side.
/// </summary>
public sealed class NvcFile
{
    /// <summary>The file name without extension: the two map names, e.g. m60_33_41_00_m60_33_42_00.</summary>
    public string Name { get; init; }

    public NVC Nvc { get; init; }

    /// <summary>The first map of the file name, whose navmeshes are the connectors' MainNameIDs.</summary>
    public string MapA => Name.Substring(0, 12);

    /// <summary>The second map of the file name, whose navmeshes are the connectors' TargetNameIDs.</summary>
    public string MapB => Name.Substring(13, 12);

    /// <summary>The map on the other side from the given one.</summary>
    public string Opposite(string mapId) => mapId == MapA ? MapB : MapA;
}

/// <summary>
/// The navigation data of one map.
/// </summary>
public sealed class NavigationMap
{
    public string MapId { get; }

    public NVA_ER Nva { get; }

    /// <summary>The navmesh files of the nvmhktbnd, by name.</summary>
    public IReadOnlyDictionary<string, NavmeshFile> Files { get; }

    /// <summary>Null when the map has no ONAV.</summary>
    public ONAV Onav { get; }

    /// <summary>The NVC files that name this map.</summary>
    public IReadOnlyList<NvcFile> Nvcs { get; }

    /// <summary>The navmesh models, in the order of their n entries in the NVA.</summary>
    public IReadOnlyList<NavmeshModel> Models { get; }

    /// <summary>
    /// The map name the navmesh files and NameIDs carry. Usually the map itself; some open world tiles ending in _10
    /// are versions of the _00 tile and use its names (m60_44_36_10 has q60_44_36_00_443600_0), others their own
    /// (m60_45_36_10). Taken from the files when the map has any, else the map.
    /// </summary>
    public string NameBase { get; }

    public NavigationMap(string mapId, NVA_ER nva, IReadOnlyDictionary<string, NavmeshFile> files, ONAV onav, IReadOnlyList<NvcFile> nvcs)
    {
        MapId = mapId;
        Nva = nva;
        Files = files;
        Onav = onav;
        Nvcs = nvcs;
        NameBase = DetectNameBase(mapId, files.Keys);
        Models = BuildModels();
    }

    private static string DetectNameBase(string mapId, IEnumerable<string> fileNames)
    {
        var own = mapId.Substring(1);
        var names = fileNames.ToList();

        if (names.Count == 0 || names.Any(n => n.AsSpan(1).StartsWith(own)))
            return mapId;

        if (IsOpenWorld(mapId) && mapId.EndsWith("_10"))
        {
            var baseMap = mapId[..^3] + "_00";
            if (names.Any(n => n.AsSpan(1).StartsWith(baseMap.AsSpan(1))))
                return baseMap;
        }

        return mapId;
    }

    private List<NavmeshModel> BuildModels()
    {
        var models = new List<NavmeshModel>();
        var navmeshes = Nva?.Navmeshes;

        if (navmeshes == null)
            return models;

        foreach (var n in navmeshes.Where(e => SetOf(e) == NavmeshSet.N && QuarterOf(e) == null))
        {
            // The o entry repeats the n entry's NameID with 2 more billions
            var o = navmeshes.FirstOrDefault(e => (uint)e.NameID == (uint)n.NameID + 2_000_000_000u)
                ?? navmeshes.FirstOrDefault(e => SetOf(e) == NavmeshSet.O && QuarterOf(e) == null && e.ModelID == n.ModelID);

            var quarters = navmeshes.Where(e => e.ModelID == n.ModelID && SetOf(e) == NavmeshSet.N && QuarterOf(e) != null)
                .OrderBy(e => QuarterOf(e))
                .Select(e => (e, FileOf(e)))
                .ToList();

            models.Add(new NavmeshModel
            {
                ModelID = n.ModelID,
                NEntry = n,
                OEntry = o,
                NFile = FileOf(n),
                OFile = o == null ? null : FileOf(o),
                Quarters = quarters,
            });
        }

        return models;
    }

    /// <summary>
    /// The set an NVA entry belongs to, from its NameID's prefix: 0 or 1 for n, 2 or 3 for o.
    /// </summary>
    public static NavmeshSet SetOf(NVA_ER.Navmesh entry) => (uint)entry.NameID / 1_000_000_000u >= 2 ? NavmeshSet.O : NavmeshSet.N;

    /// <summary>
    /// The quarter of a q file an entry's faces are in, from the high byte of Unk50 (0xFF when it has none), or null.
    /// The game files use 0xFF01 for most entries and 0x0001, 0x0101, 0x0201, 0x0301 for the quarters.
    /// </summary>
    public static int? QuarterOf(NVA_ER.Navmesh entry)
    {
        int quarter = (entry.Unk50 >> 8) & 0xFF;
        return quarter == 0xFF ? null : quarter;
    }

    /// <summary>The navmesh entry with the given NameID, or null.</summary>
    public NVA_ER.Navmesh FindNavmesh(int nameId) => Nva?.Navmeshes.FirstOrDefault(n => n.NameID == nameId);

    /// <summary>The file an NVA entry's faces are in: its q file for a quarter, else its n or o file. Null if it has none.</summary>
    public NavmeshFile FileOf(NVA_ER.Navmesh entry) => Files.GetValueOrDefault(FileNameOf(entry));

    /// <summary>The name of the file an NVA entry's faces are in.</summary>
    public string FileNameOf(NVA_ER.Navmesh entry)
    {
        var quarter = QuarterOf(entry);
        return quarter != null ? QuarterName(NameBase, entry.ModelID, quarter.Value) : MeshName(NameBase, SetOf(entry), entry.ModelID);
    }

    /// <summary>
    /// The name of the file an NVA entry's faces are in, from the entry alone, for when the binder's file names
    /// aren't at hand (the Map Editor's navmesh drawing). The name base comes from the NameID's digits: an open world
    /// _10 tile whose NameIDs carry DD = 00 names its files after the _00 tile.
    /// </summary>
    public static string FileNameFromEntry(string mapId, NVA_ER.Navmesh entry)
    {
        var nameBase = mapId;

        if (IsOpenWorld(mapId))
        {
            // BBCCDD of the map the NameID was given for
            uint digits = (uint)entry.NameID / 1000 % 1_000_000;
            var own = mapId.Substring(1).Replace("_", "");
            var other = $"m{own.Substring(0, 2)}_{digits / 10000:D2}_{digits / 100 % 100:D2}_{digits % 100:D2}";

            // Only another version of the same tile, never another tile
            if (other.AsSpan(0, 9).SequenceEqual(mapId.AsSpan(0, 9)))
                nameBase = other;
        }

        var quarter = QuarterOf(entry);
        return quarter != null ? QuarterName(nameBase, entry.ModelID, quarter.Value) : MeshName(nameBase, SetOf(entry), entry.ModelID);
    }

    /// <summary>The map UID of the NVA (its Entries11), which the NVC files repeat.</summary>
    public uint? MapUid => Nva?.Entries11?.Count > 0 ? (uint)Nva.Entries11[0].Unk00 : null;

    /// <summary>The name of a navmesh file, e.g. n10_01_00_00_001000, for the map whose names the files carry.</summary>
    public static string MeshName(string nameBase, NavmeshSet set, int modelId)
        => $"{(set == NavmeshSet.N ? 'n' : 'o')}{nameBase.Substring(1)}_{modelId:D6}";

    /// <summary>The name of a quarter of a model's n mesh, e.g. q60_49_37_00_493700_2.</summary>
    public static string QuarterName(string nameBase, int modelId, int quarter) => $"q{nameBase.Substring(1)}_{modelId:D6}_{quarter}";

    /// <summary>The NameID the game files give the n entry at a position in this map's navmesh list.</summary>
    public uint ExpectedNameId(int position) => ExpectedNameId(NameBase, position);

    /// <summary>
    /// The open world maps, whose NameIDs use odd prefixes and the BBCCDD digits of the map name.
    /// </summary>
    public static bool IsOpenWorld(string mapId) => mapId.StartsWith("m60") || mapId.StartsWith("m61");

    /// <summary>
    /// The NameID the game files give an n entry at a position in the NVA's navmesh list:
    /// prefix * 1,000,000,000 + map digits * 1,000 + position. The prefix is 0 in legacy maps, with the digits AABBCC,
    /// and 1 in the open world, with BBCCDD; the game tests the odd prefix (0x140BDA2A0).
    /// An o entry has its n entry's NameID plus 2,000,000,000. The map is the one whose names the files carry.
    /// </summary>
    public static uint ExpectedNameId(string nameBase, int position)
    {
        var digits = nameBase.Substring(1).Replace("_", "");
        bool openWorld = IsOpenWorld(nameBase);
        uint middle = uint.Parse(openWorld ? digits.Substring(2, 6) : digits.Substring(0, 6));
        return (openWorld ? 1_000_000_000u : 0u) + middle * 1_000u + (uint)position;
    }
}

/// <summary>
/// A connection between two navmesh face edges, from an NVA connector (within a map) or an NVC connector
/// (between two maps). Side A is the connector's MainNameID, side B its TargetNameID.
/// </summary>
public readonly record struct NavConnectionView(
    NvcFile Nvc,
    int MainNameID,
    int TargetNameID,
    NVA_ER.NavMeshConnection Connection)
{
    public bool IsCrossMap => Nvc != null;
}

/// <summary>
/// What a renumbering of a navmesh's faces or edges changed.
/// </summary>
public sealed class NavRemapResult
{
    public int Changed { get; set; }

    public int Dropped { get; set; }

    public bool NvaChanged { get; set; }

    public HashSet<NvcFile> NvcsChanged { get; } = new();
}

/// <summary>
/// Keeps the connections pointing at the right faces and edges when a navmesh's faces or edges are renumbered,
/// in the map's NVA connectors and in every NVC file that names the map.
/// </summary>
public static class NavigationRemap
{
    /// <summary>
    /// Every connection that names a navmesh of the map, within the map and across its NVC files.
    /// </summary>
    public static IEnumerable<NavConnectionView> Connections(NavigationMap map)
    {
        if (map.Nva?.Connectors != null)
        {
            foreach (var connector in map.Nva.Connectors)
                foreach (var connection in connector.NavMeshConnections)
                    yield return new NavConnectionView(null, connector.MainNameID, connector.TargetNameID, connection);
        }

        foreach (var nvc in map.Nvcs)
        {
            foreach (var connector in nvc.Nvc.Connectors)
                foreach (var connection in connector.NavMeshConnections)
                    yield return new NavConnectionView(nvc, connector.MainNameID, connector.TargetNameID, connection);
        }
    }

    /// <summary>
    /// Applies a renumbering of one navmesh of the map to every connection that names it. The maps return the new
    /// index for an old one, or -1 when it no longer exists, which drops the connection.
    /// NameIDs repeat between m60 and m61, so a NVC side only counts when its map is this map.
    /// </summary>
    public static NavRemapResult RemapNavmesh(NavigationMap map, int nameId, Func<int, int> faceMap, Func<int, int> edgeMap)
    {
        var result = new NavRemapResult();

        if (map.Nva?.Connectors != null)
        {
            foreach (var connector in map.Nva.Connectors)
            {
                if (RemapConnector(connector.NavMeshConnections, connector.MainNameID == nameId, connector.TargetNameID == nameId, faceMap, edgeMap, result))
                    result.NvaChanged = true;
            }
        }

        foreach (var nvc in map.Nvcs)
        {
            bool isA = nvc.MapA == map.MapId;
            bool isB = nvc.MapB == map.MapId;

            foreach (var connector in nvc.Nvc.Connectors)
            {
                if (RemapConnector(connector.NavMeshConnections, isA && connector.MainNameID == nameId, isB && connector.TargetNameID == nameId, faceMap, edgeMap, result))
                    result.NvcsChanged.Add(nvc);
            }
        }

        return result;
    }

    private static bool RemapConnector(List<NVA_ER.NavMeshConnection> connections, bool sideA, bool sideB,
        Func<int, int> faceMap, Func<int, int> edgeMap, NavRemapResult result)
    {
        if (!sideA && !sideB)
            return false;

        bool changed = false;

        for (int i = connections.Count - 1; i >= 0; i--)
        {
            var c = connections[i];
            int face = c.FaceIndex, edge = c.EdgeIndex, oppositeFace = c.OppositeFaceIndex, oppositeEdge = c.OppositeEdgeIndex;

            if (sideA)
            {
                face = faceMap(face);
                edge = edgeMap(edge);
            }

            if (sideB)
            {
                oppositeFace = faceMap(oppositeFace);
                oppositeEdge = edgeMap(oppositeEdge);
            }

            if (face < 0 || edge < 0 || oppositeFace < 0 || oppositeEdge < 0)
            {
                connections.RemoveAt(i);
                result.Dropped++;
                changed = true;
                continue;
            }

            if (face != c.FaceIndex || edge != c.EdgeIndex || oppositeFace != c.OppositeFaceIndex || oppositeEdge != c.OppositeEdgeIndex)
            {
                c.FaceIndex = face;
                c.EdgeIndex = edge;
                c.OppositeFaceIndex = oppositeFace;
                c.OppositeEdgeIndex = oppositeEdge;
                result.Changed++;
                changed = true;
            }
        }

        return changed;
    }
}
