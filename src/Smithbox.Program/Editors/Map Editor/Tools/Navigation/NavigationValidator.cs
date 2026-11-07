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

        if (map.MapUid == null)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.MapUidMissing, file));

        int faceTotal = 0;
        var seenIds = new HashSet<int>();
        var byNameId = navmeshes.GroupBy(n => n.NameID).ToDictionary(g => g.Key, g => g.First());

        for (int i = 0; i < navmeshes.Count; i++)
        {
            var n = navmeshes[i];
            var set = NavigationMap.SetOf(n);

            if (!seenIds.Add(n.NameID))
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NameIdDuplicate, file, (uint)n.NameID));

            if (set == NavmeshSet.N)
            {
                // An n entry's NameID ends in its position in the list
                uint expected = map.ExpectedNameId(i);
                if ((uint)n.NameID != expected)
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.NameIdRule, file, i, (uint)n.NameID, expected));
            }
            else
            {
                // An o entry repeats an n entry of the same model, 2 billions on
                bool paired = byNameId.TryGetValue((int)((uint)n.NameID - 2_000_000_000u), out var partner) && partner.ModelID == n.ModelID;
                if (!paired)
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.OEntryUnpaired, file, (uint)n.NameID, n.ModelID));
            }

            if (n.FaceDataIndex != faceTotal)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.FaceDataIndex, file, (uint)n.NameID, n.FaceDataIndex, faceTotal));
            faceTotal += n.FaceCount;

            var meshFile = map.FileOf(n);
            var meshName = map.FileNameOf(n);

            if (meshFile == null)
            {
                if (n.FaceCount > 0)
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.MeshMissing, file, (uint)n.NameID, meshName));
            }
            else if (meshFile.FaceCount != n.FaceCount)
            {
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.FaceCountMismatch, file, (uint)n.NameID, n.FaceCount, meshName, meshFile.FaceCount));
            }

            if (n.Unk4C == 1 && n.FaceCount > 0)
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.PlaceholderHasFaces, file, (uint)n.NameID, n.FaceCount));

            ValidateGates(map, file, n, issues);
        }
    }

    /// <summary>
    /// A navmesh's gate nodes (NavGates): at most 16, each pointing at a navmesh of the map it is connected to, paired
    /// with a gate there that points back unless that navmesh is full, and a symmetric table of 16 costs with 0 for itself.
    /// </summary>
    private static void ValidateGates(NavigationMap map, string file, NVA_ER.Navmesh n, List<NavIssue> issues)
    {
        var navmeshes = map.Nva.Navmeshes;
        var gates = n.GateNodes;
        uint id = (uint)n.NameID;

        if (gates.Count > NavGates.MaxGates)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCount, file, id, gates.Count));

        for (int g = 0; g < gates.Count; g++)
        {
            var gate = gates[g];

            if (gate.ConnectedNavmeshIndex < 0 || gate.ConnectedNavmeshIndex >= navmeshes.Count)
            {
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateNodeNavmeshIndex, file, id, gate.ConnectedNavmeshIndex, navmeshes.Count));
                continue;
            }

            var target = navmeshes[gate.ConnectedNavmeshIndex];
            uint targetId = (uint)target.NameID;

            switch (NavGates.Status(map, n, g))
            {
                case NavGateStatus.Unpaired:
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateUnpaired, file, id, g, targetId));
                    break;
                case NavGateStatus.Broken:
                    if (NavGates.Neighbours(map, n).Contains(target))
                        issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GatePairBroken, file, id, g, targetId, gate.NodeSubID));
                    else
                        issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateNotConnected, file, id, g, targetId));
                    break;
            }

            var costs = gate.NeighbourGateNodeCosts;
            if (costs.Count != NavGates.MaxGates)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCostCount, file, id, g, costs.Count));

            if (g < costs.Count && costs[g] != 0)
                issues.Add(new(NavIssueSeverity.Error, NavIssueCode.GateCostSelf, file, id, g, costs[g]));

            for (int h = g + 1; h < gates.Count; h++)
            {
                var other = gates[h].NeighbourGateNodeCosts;
                if (h < costs.Count && g < other.Count && costs[h] != other[g])
                    issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.GateCostAsymmetric, file, id, g, h, (ushort)costs[h], (ushort)other[g]));
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
                    issues.Add(new(NavIssueSeverity.Error, NavIssueCode.NodeBankFaceRange, file, bank.BankID, (uint)face.CollisionID, face.FaceIndex, faces));
            }
        }
    }

    private static void ValidateFiles(NavigationMap map, List<NavIssue> issues)
    {
        foreach (var f in map.Files.Values)
        {
            if (f.FaceCount > 0 && !f.HasMediator)
                issues.Add(new(NavIssueSeverity.Warning, NavIssueCode.MediatorMissing, $"{f.Name}.hkx", f.FaceCount));
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
                CheckSide(issues, file, connector.MainNameID, a, fileA, c.FaceIndex, c.EdgeIndex);
                CheckSide(issues, file, connector.TargetNameID, b, fileB, c.OppositeFaceIndex, c.OppositeEdgeIndex);
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
                    CheckSide(issues, file, connector.MainNameID, a, fileA, c.FaceIndex, c.EdgeIndex);
                    CheckSide(issues, file, connector.TargetNameID, b, fileB, c.OppositeFaceIndex, c.OppositeEdgeIndex);
                }
            }
        }
    }

    /// <summary>
    /// One end of a connection: the face must be in the navmesh, and the edge one of that face's edges or user edges.
    /// </summary>
    private static void CheckSide(List<NavIssue> issues, string file, int nameId, NVA_ER.Navmesh entry, NavmeshFile meshFile,
        int face, int edge)
    {
        if (entry == null)
            return;

        int faceCount = meshFile?.FaceCount ?? entry.FaceCount;

        if (face < 0 || face >= faceCount)
        {
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectionFaceRange, file, (uint)nameId, face, faceCount));
            return;
        }

        if (meshFile?.Mesh == null)
            return;

        var f = meshFile.Mesh.m_faces[face];
        bool onFace = (edge >= f.m_startEdgeIndex && edge < f.m_startEdgeIndex + f.m_numEdges)
            || (f.m_numUserEdges > 0 && edge >= f.m_startUserEdgeIndex && edge < f.m_startUserEdgeIndex + f.m_numUserEdges);

        if (!onFace)
            issues.Add(new(NavIssueSeverity.Error, NavIssueCode.ConnectionEdgeNotOnFace, file, (uint)nameId, face, edge));
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
