using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public enum NavIssueSeverity
{
    /// <summary>Something the game files never do, which an edit has probably broken.</summary>
    Error,

    /// <summary>Something unusual that a few game files also do.</summary>
    Warning
}

/// <summary>
/// A problem the validator found. The code names the check; the Map Editor shows it with the
/// MAP_Navigation_Issue_{Code} text and the arguments.
/// </summary>
public sealed record NavIssue(NavIssueSeverity Severity, NavIssueCode Code, string File, params object[] Args)
{
    /// <summary>
    /// The vanilla game files have the same issue, so the project didn't cause it. Some vanilla NVC files that pair an
    /// open world _00 tile with a _10 one were built against another version of a tile, and fail several checks.
    /// </summary>
    public bool Inherited { get; init; }

    // Where the issue is, for the Check tab to go to it. Not part of the Key, so the vanilla comparison is unchanged

    /// <summary>The map of the navmesh below; the checked map unless the issue is on a neighbour's side of an NVC file.</summary>
    public string MapId { get; init; }

    /// <summary>The navmesh the issue is on, by NameID, if it is on one.</summary>
    public uint? NameID { get; init; }

    /// <summary>The face of that navmesh the issue is on, if it names one that exists.</summary>
    public int? Face { get; init; }

    /// <summary>The gate node of that navmesh the issue is on, if it is on one.</summary>
    public int? Gate { get; init; }

    /// <summary>The code, file and arguments, which say whether two issues are the same.</summary>
    public string Key => $"{Code}|{File}|{string.Join("|", Args)}";
}

public enum NavIssueCode
{
    // NVA
    NvaMissing,
    MapUidMissing,
    OEntryUnpaired,
    NameIdRule,
    NameIdDuplicate,
    FaceDataIndex,
    FaceCountMismatch,
    MeshMissing,
    PlaceholderHasFaces,
    GateNodeNavmeshIndex,
    GateCount,
    GateNotConnected,
    GatePairBroken,
    GateUnpaired,
    GateCostCount,
    GateCostSelf,
    GateCostAsymmetric,
    NodeBankNavmeshMissing,
    NodeBankFaceRange,

    // Navmesh files
    MediatorMissing,
    CostGraphMismatch,

    // Connections, in NVA connectors and NVC files
    ConnectorNavmeshMissing,
    ConnectionFaceRange,
    ConnectionEdgeNotOnFace,
    NvcMapUid,

    // ONAV
    OnavGrid,
    OnavLinkTarget,
    OnavLinkLayer,
}

/// <summary>
/// Checks the invariants of a map's navigation data (Navigation Editor Plan, section 5). The vanilla game files
/// pass it without errors (Tools/NavSurvey `validate`); the warnings are things a few game files do too.
/// </summary>
public static class NavigationValidator
{
    /// <param name="map">The map to check.</param>
    /// <param name="neighbour">Finds another map's navigation data, for the far side of its NVC files; may return null,
    /// in which case only this map's side is checked.</param>
    public static List<NavIssue> Validate(NavigationMap map, Func<string, NavigationMap> neighbour = null)
    {
        var issues = new List<NavIssue>();
        var nvaName = $"{map.MapId}.nva";

        if (map.Nva == null)
        {
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NvaMissing, nvaName));
            return issues;
        }

        ValidateNva(map, nvaName, issues);
        ValidateNodeBanks(map, nvaName, issues);
        ValidateFiles(map, issues);
        ValidateConnectors(map, nvaName, issues);
        ValidateNvcs(map, neighbour, issues);
        ValidateOnav(map, issues);

        return issues;
    }

    private static void ValidateNva(NavigationMap map, string file, List<NavIssue> issues)
    {
        var nva = map.Nva;
        var navmeshes = nva.Navmeshes;

        if (map.Profile.HasMapUid && map.MapUid == null)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.MapUidMissing, file));

        int faceTotal = 0;
        var seenIds = new HashSet<int>();
        var byNameId = navmeshes.GroupBy(n => n.NameID).ToDictionary(g => g.Key, g => g.First());

        for (int i = 0; i < navmeshes.Count; i++)
        {
            var n = navmeshes[i];
            var set = NavigationMap.SetOf(n, map.Profile);

            if ((uint)n.NameID != NavGameProfile.NoNameId && !seenIds.Add(n.NameID))
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NameIdDuplicate, file, (uint)n.NameID) { MapId = map.MapId, NameID = (uint)n.NameID });

            if (set == NavmeshSet.N)
            {
                // An n entry's NameID by the game's rule: its position in the list (ER), or its ModelID (DS3, SDT). The
                // -1 of a placeholder without a NameID, and a game without one rule (BB), aren't checked
                bool unnamed = (uint)n.NameID == NavGameProfile.NoNameId && n.FaceCount == 0;
                if (!unnamed && map.Profile.ExpectedNameId(map.NameBase, i, n.ModelID) is uint expected && (uint)n.NameID != expected)
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.NameIdRule, file, i, (uint)n.NameID, expected) { MapId = map.MapId, NameID = (uint)n.NameID });
            }
            else
            {
                // An o entry repeats an n entry of the same model, the profile's offset on (2 billions in ER)
                bool paired = byNameId.TryGetValue((int)map.Profile.UnmirrorOf((uint)n.NameID), out var partner) && partner.ModelID == n.ModelID;
                if (!paired)
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.OEntryUnpaired, file, (uint)n.NameID, n.ModelID) { MapId = map.MapId, NameID = (uint)n.NameID });
            }

            // The running total of FaceCount (ER, NR); in DB_NT a value per model, the same in its o entry as in its n entry
            if (map.Profile.FaceDataIndexIsRunningTotal)
            {
                if (n.FaceDataIndex != faceTotal)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.FaceDataIndex, file, (uint)n.NameID, n.FaceDataIndex, faceTotal) { MapId = map.MapId, NameID = (uint)n.NameID });
            }
            else if (set == NavmeshSet.O && byNameId.TryGetValue((int)map.Profile.UnmirrorOf((uint)n.NameID), out var nEntry) && n.FaceDataIndex != nEntry.FaceDataIndex)
            {
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.FaceDataIndex, file, (uint)n.NameID, n.FaceDataIndex, nEntry.FaceDataIndex) { MapId = map.MapId, NameID = (uint)n.NameID });
            }
            faceTotal += n.FaceCount;

            var meshFile = map.FileOf(n);
            var meshName = map.FileNameOf(n);

            if (meshFile == null)
            {
                if (n.FaceCount > 0)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.MeshMissing, file, (uint)n.NameID, meshName) { MapId = map.MapId, NameID = (uint)n.NameID });
            }
            else if (meshFile.FaceCount != n.FaceCount)
            {
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.FaceCountMismatch, file, (uint)n.NameID, n.FaceCount, meshName, meshFile.FaceCount) { MapId = map.MapId, NameID = (uint)n.NameID });
            }

            if (map.Profile.Unk4CMeansNoFaces && n.Unk4C == 1 && n.FaceCount > 0)
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.PlaceholderHasFaces, file, (uint)n.NameID, n.FaceCount) { MapId = map.MapId, NameID = (uint)n.NameID });

            ValidateGates(map, file, n, issues);
        }
    }

    /// <summary>
    /// A navmesh's gate nodes (NavGates): at most 16, each pointing at a navmesh of the map it is connected to, paired
    /// with a gate there that points back unless that navmesh is full, and a symmetric table of 16 costs with 0 for itself.
    /// </summary>
    private static void ValidateGates(NavigationMap map, string file, NVA.Navmesh n, List<NavIssue> issues)
    {
        var navmeshes = map.Nva.Navmeshes;
        var gates = n.GateNodes;
        uint id = (uint)n.NameID;

        if (gates.Count > map.Profile.MaxGates)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCount, file, id, gates.Count) { MapId = map.MapId, NameID = id });

        for (int g = 0; g < gates.Count; g++)
        {
            var gate = gates[g];

            if (gate.ConnectedNavmeshIndex < 0 || gate.ConnectedNavmeshIndex >= navmeshes.Count)
            {
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateNodeNavmeshIndex, file, id, gate.ConnectedNavmeshIndex, navmeshes.Count) { MapId = map.MapId, NameID = id, Gate = g });
                continue;
            }

            var target = navmeshes[gate.ConnectedNavmeshIndex];
            uint targetId = (uint)target.NameID;

            switch (NavGates.Status(map, n, g))
            {
                case NavGateStatus.Unpaired:
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateUnpaired, file, id, g, targetId) { MapId = map.MapId, NameID = id, Gate = g });
                    break;
                case NavGateStatus.Broken:
                    if (NavGates.Neighbours(map, n).Contains(target))
                        issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GatePairBroken, file, id, g, targetId, gate.NodeSubID) { MapId = map.MapId, NameID = id, Gate = g });
                    else
                        issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateNotConnected, file, id, g, targetId) { MapId = map.MapId, NameID = id, Gate = g });
                    break;
            }

            var costs = gate.NeighbourGateNodeCosts;
            if (costs.Count != map.Profile.GateCostSlots(gates.Count))
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCostCount, file, id, g, costs.Count) { MapId = map.MapId, NameID = id, Gate = g });

            if (g < costs.Count && costs[g] != 0)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCostSelf, file, id, g, costs[g]) { MapId = map.MapId, NameID = id, Gate = g });

            for (int h = g + 1; h < gates.Count; h++)
            {
                var other = gates[h].NeighbourGateNodeCosts;
                if (h < costs.Count && g < other.Count && costs[h] != other[g])
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateCostAsymmetric, file, id, g, h, (ushort)costs[h], (ushort)other[g]) { MapId = map.MapId, NameID = id, Gate = g });
            }
        }
    }

    /// <summary>
    /// Node bank faces name their navmesh by NameID (the field SoulsFormats calls CollisionID); EMEVD 2003[13] changes
    /// the flags of these faces by the bank's entity ID.
    /// </summary>
    private static void ValidateNodeBanks(NavigationMap map, string file, List<NavIssue> issues)
    {
        foreach (var bank in map.Nva.NodeBanks ?? new())
        {
            foreach (var face in bank.Faces)
            {
                var entry = map.FindNavmesh(face.CollisionID);

                if (entry == null)
                {
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NodeBankNavmeshMissing, file, bank.BankID, (uint)face.CollisionID));
                    continue;
                }

                int faces = map.FileOf(entry)?.FaceCount ?? entry.FaceCount;
                if (face.FaceIndex < 0 || face.FaceIndex >= faces)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NodeBankFaceRange, file, bank.BankID, (uint)face.CollisionID, face.FaceIndex, faces) { MapId = map.MapId, NameID = (uint)entry.NameID });
            }
        }
    }

    private static void ValidateFiles(NavigationMap map, List<NavIssue> issues)
    {
        foreach (var f in map.Files.Values)
        {
            // The navmesh the file is drawn for, to go to it; none for a file no entry uses
            var owner = map.Nva.Navmeshes.FirstOrDefault(e => ReferenceEquals(map.FileOf(e), f));
            uint? ownerId = owner == null ? null : (uint)owner.NameID;

            // SDT keeps no face tree in its navmesh files
            if (map.Profile.HasFaceTree && f.FaceCount > 0 && !f.HasMediator)
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.MediatorMissing, $"{f.Name}.hkx", f.FaceCount) { MapId = map.MapId, NameID = ownerId });

            // DS3, BB and SDT: the file's cost graph must match its mesh's clusters, or the game's hierarchical paths
            // go through faces that aren't there
            if (f.Mesh != null && NavCostGraph.Of(f.Document) is IHavokDirectedGraphEdit graph && NavCostGraph.Check(graph, f.Mesh) is string problem)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.CostGraphMismatch, $"{f.Name}.hkx", problem) { MapId = map.MapId, NameID = ownerId });
        }
    }

    private static void ValidateConnectors(NavigationMap map, string file, List<NavIssue> issues)
    {
        foreach (var connector in map.Nva.Connectors ?? new())
        {
            var a = map.FindNavmesh(connector.MainNameID);
            var b = map.FindNavmesh(connector.TargetNameID);

            if (a == null)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectorNavmeshMissing, file, (uint)connector.MainNameID, map.MapId));
            if (b == null)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectorNavmeshMissing, file, (uint)connector.TargetNameID, map.MapId));

            var fileA = a == null ? null : map.FileOf(a);
            var fileB = b == null ? null : map.FileOf(b);

            foreach (var c in connector.NavMeshConnections)
            {
                CheckSide(issues, file, map.MapId, connector.MainNameID, a, fileA, c.FaceIndex, c.EdgeIndex);
                CheckSide(issues, file, map.MapId, connector.TargetNameID, b, fileB, c.OppositeFaceIndex, c.OppositeEdgeIndex);
            }
        }
    }

    private static void ValidateNvcs(NavigationMap map, Func<string, NavigationMap> neighbour, List<NavIssue> issues)
    {
        foreach (var nvc in map.Nvcs)
        {
            var file = $"{nvc.Name}.nvc";
            bool isA = nvc.MapA == map.MapId;
            var other = neighbour?.Invoke(nvc.Opposite(map.MapId));
            var mapA = isA ? map : other;
            var mapB = isA ? other : map;

            // Each side's UID is checked when that map is here
            if (mapA?.MapUid is uint uidA && nvc.Nvc.MapUID != uidA)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NvcMapUid, file, mapA.MapId, nvc.Nvc.MapUID, uidA));
            if (mapB?.MapUid is uint uidB && nvc.Nvc.OppositeMapUID != uidB)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NvcMapUid, file, mapB.MapId, nvc.Nvc.OppositeMapUID, uidB));

            foreach (var connector in nvc.Nvc.Connectors)
            {
                var a = mapA?.FindNavmesh(connector.MainNameID);
                var b = mapB?.FindNavmesh(connector.TargetNameID);

                if (mapA != null && a == null)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectorNavmeshMissing, file, (uint)connector.MainNameID, mapA.MapId));
                if (mapB != null && b == null)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectorNavmeshMissing, file, (uint)connector.TargetNameID, mapB.MapId));

                var fileA = a == null ? null : mapA.FileOf(a);
                var fileB = b == null ? null : mapB.FileOf(b);

                foreach (var c in connector.NavMeshConnections)
                {
                    CheckSide(issues, file, mapA?.MapId, connector.MainNameID, a, fileA, c.FaceIndex, c.EdgeIndex);
                    CheckSide(issues, file, mapB?.MapId, connector.TargetNameID, b, fileB, c.OppositeFaceIndex, c.OppositeEdgeIndex);
                }
            }
        }
    }

    /// <summary>
    /// One end of a connection: the face must be in the navmesh, and the edge one of that face's edges or user edges.
    /// </summary>
    private static void CheckSide(List<NavIssue> issues, string file, string mapId, int nameId, NVA.Navmesh entry, NavmeshFile meshFile,
        int face, int edge)
    {
        if (entry == null)
            return;

        int faceCount = meshFile?.FaceCount ?? entry.FaceCount;

        if (face < 0 || face >= faceCount)
        {
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectionFaceRange, file, (uint)nameId, face, faceCount) { MapId = mapId, NameID = (uint)nameId });
            return;
        }

        if (meshFile?.Mesh == null)
            return;

        var f = meshFile.Mesh.m_faces[face];
        bool onFace = (edge >= f.m_startEdgeIndex && edge < f.m_startEdgeIndex + f.m_numEdges)
            || (f.m_numUserEdges > 0 && edge >= f.m_startUserEdgeIndex && edge < f.m_startUserEdgeIndex + f.m_numUserEdges);

        if (!onFace)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectionEdgeNotOnFace, file, (uint)nameId, face, edge) { MapId = mapId, NameID = (uint)nameId, Face = face });
    }

    private static void ValidateOnav(NavigationMap map, List<NavIssue> issues)
    {
        var onav = map.Onav;

        if (onav == null)
            return;

        var file = $"{map.MapId}.onav";

        if (onav.GridCountX * onav.GridCountY * onav.GridCountZ != ONAV.CellMaskSize * 8)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.OnavGrid, file, onav.GridCountX, onav.GridCountY, onav.GridCountZ));

        var layersById = new Dictionary<int, int>();
        foreach (var area in onav.Areas)
            layersById[area.ID] = area.Layers.Count;
        foreach (var area in onav.UnkAreas)
            layersById.TryAdd(area.ID, 0);

        foreach (var area in onav.Areas)
        {
            foreach (var link in area.Links)
            {
                if (!layersById.ContainsKey(link.AreaID))
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.OnavLinkTarget, file, area.ID, link.AreaID));
                else if (area.Layers.Count > 0 && !area.Layers.Any(l => l.Index == link.LayerIndex))
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.OnavLinkLayer, file, area.ID, link.AreaID, link.LayerIndex));
            }
        }
    }

    /// <summary>
    /// Marks the issues the vanilla data also has as inherited.
    /// </summary>
    public static List<NavIssue> MarkInherited(List<NavIssue> issues, IEnumerable<NavIssue> vanilla)
    {
        var known = vanilla.Select(i => i.Key).ToHashSet();
        return issues.Select(i => known.Contains(i.Key) ? i with { Inherited = true } : i).ToList();
    }

    /// <summary>A short English description, for logs and tools without the Map Editor's localization.</summary>
    public static string Describe(NavIssue issue) => $"{issue.Severity} {issue.Code} in {issue.File}: {string.Join(", ", issue.Args)}";
}
