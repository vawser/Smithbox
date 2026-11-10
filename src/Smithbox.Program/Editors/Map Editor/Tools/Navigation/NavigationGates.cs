using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The NVA's gate nodes (Navigation Editor Plan, Phase 3: Gate nodes). Kept free of the rest of Smithbox; NavSurvey
// `gateedit` runs it on every map.
//
// What the game's files do (NavSurvey gatesurvey, 997 maps, 9,243 gates):
// - a gate is a point shared by two navmeshes of one map: a paired gate has exactly its partner's position, and
//   ConnectedNavmeshIndex / NodeSubID name the partner's navmesh (by list position) and the partner;
// - it sits on a connection edge, at the middle of the overlap of that edge and the neighbour's edge it is joined to
//   (90.5% of n gates within 1 cm);
// - most neighbours get one gate; long or broken-up borders get more (one per run of connection edges within 5 m
//   matches 76% of n neighbours);
// - at most 16 per navmesh. A gate whose target is full has no partner (NodeSubID -1);
// - NeighbourGateNodeCosts holds 16 unsigned 16-bit costs, in cm of walking distance from this gate to each of its
//   own navmesh's gates: 0 to itself, 0xFFFF (-1) when unreachable, -1 past the owner's gates. The table is symmetric.
//
// Gate positions are in map space; meshes in their navmesh's own space. Every function takes toMap, which places a
// navmesh's mesh in map space (identity for the game's entries without a rotation).

/// <summary>A copy of every navmesh's gate nodes of an NVA, which can be put back.</summary>
public sealed class NavGateState
{
    private readonly List<(NVA.Navmesh Entry, List<NVA.GateNode> Gates)> Entries;

    public NavGateState(NVA nva)
    {
        Entries = nva.Navmeshes.Select(e => (e, e.GateNodes.Select(Copy).ToList())).ToList();
    }

    public static NVA.GateNode Copy(NVA.GateNode g) => new()
    {
        Position = g.Position,
        ConnectedNavmeshIndex = g.ConnectedNavmeshIndex,
        NodeSubID = g.NodeSubID,
        NeighbourGateNodeCosts = g.NeighbourGateNodeCosts.ToList(),
        Unk14 = g.Unk14,
        Unk18 = g.Unk18,
    };

    /// <summary>Puts the gates back as new copies, so the state can be restored again.</summary>
    public void Restore()
    {
        foreach (var (entry, gates) in Entries)
            entry.GateNodes = gates.Select(Copy).ToList();
    }
}

/// <summary>How a gate stands with its partner.</summary>
public enum NavGateStatus
{
    /// <summary>Its partner points back at it.</summary>
    Paired,

    /// <summary>No partner, because the target navmesh has 16 gates, as the game's unpaired gates all do.</summary>
    UnpairedFull,

    /// <summary>No partner although the target has room.</summary>
    Unpaired,

    /// <summary>Its target or partner doesn't exist, the partner doesn't point back, or the target isn't connected to it.</summary>
    Broken,
}

/// <summary>What a gate edit did, or why it was refused.</summary>
public sealed class NavGateEditResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    public int Added { get; set; }
    public int Removed { get; set; }

    /// <summary>The navmeshes whose gates changed.</summary>
    public HashSet<NVA.Navmesh> Touched { get; } = new();

    public bool Ok => Error == null;
}

public static class NavGates
{
    /// <summary>The most gates a navmesh can have: its gates' cost lists have 16 entries.</summary>
    public const int MaxGates = 16;

    /// <summary>The cost of an unreachable gate, 0xFFFF.</summary>
    public const short Unreachable = -1;

    /// <summary>Connection edges within this distance are one run, which gets one gate.</summary>
    public const float DefaultRunJoin = 5f;

    /// <summary>A walking distance in metres as a gate cost: cm, unsigned, at most 0xFFFE.</summary>
    public static short EncodeCost(float metres)
    {
        if (!float.IsFinite(metres) || metres < 0)
            return Unreachable;

        int cm = (int)MathF.Round(metres * 100);
        return unchecked((short)(ushort)Math.Clamp(cm, 0, 0xFFFE));
    }

    /// <summary>A gate cost in metres, or null for unreachable.</summary>
    public static float? CostMetres(short cost) => cost == Unreachable ? null : (ushort)cost / 100f;

    #region Status

    /// <summary>How a gate of a navmesh stands with its partner.</summary>
    public static NavGateStatus Status(NavigationMap map, NVA.Navmesh owner, int gate)
    {
        var list = map.Nva.Navmeshes;
        int ownerIndex = list.IndexOf(owner);
        var g = owner.GateNodes[gate];

        if (ownerIndex < 0 || g.ConnectedNavmeshIndex < 0 || g.ConnectedNavmeshIndex >= list.Count || g.ConnectedNavmeshIndex == ownerIndex)
            return NavGateStatus.Broken;

        var target = list[g.ConnectedNavmeshIndex];

        if (!Connected(map.Nva, owner.NameID, target.NameID))
            return NavGateStatus.Broken;

        if (g.NodeSubID < 0)
            return target.GateNodes.Count >= MaxGates ? NavGateStatus.UnpairedFull : NavGateStatus.Unpaired;

        if (g.NodeSubID >= target.GateNodes.Count)
            return NavGateStatus.Broken;

        var back = target.GateNodes[g.NodeSubID];
        return back.ConnectedNavmeshIndex == ownerIndex && back.NodeSubID == gate ? NavGateStatus.Paired : NavGateStatus.Broken;
    }

    /// <summary>The partner of a paired gate (its navmesh and index), or null.</summary>
    public static (NVA.Navmesh Entry, int Gate)? Partner(NavigationMap map, NVA.Navmesh owner, int gate)
    {
        if (Status(map, owner, gate) != NavGateStatus.Paired)
            return null;

        var g = owner.GateNodes[gate];
        return (map.Nva.Navmeshes[g.ConnectedNavmeshIndex], g.NodeSubID);
    }

    private static bool Connected(NVA nva, int a, int b)
        => nva.Connectors.Any(k => k.MainNameID == a && k.TargetNameID == b && k.NavMeshConnections.Count > 0);

    /// <summary>The navmeshes of the map a navmesh is joined to by NVA connectors (in the map; gates never go across maps).</summary>
    public static List<NVA.Navmesh> Neighbours(NavigationMap map, NVA.Navmesh owner)
        => map.Nva.Connectors.Where(k => k.MainNameID == owner.NameID && k.NavMeshConnections.Count > 0)
            .Select(k => map.FindNavmesh(k.TargetNameID)).Where(e => e != null && e != owner).Distinct().ToList();

    #endregion

    #region Sites

    private static Vector3 V(IHavokNavMeshEdit mesh, int i)
    {
        var v = mesh.m_vertices[i];
        return new Vector3(v.X, v.Y, v.Z);
    }

    private static Vector3 Closest(Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        float d = ab.LengthSquared();
        float t = d < 1e-12f ? 0 : Math.Clamp(Vector3.Dot(p - a, ab) / d, 0, 1);
        return a + ab * t;
    }

    private static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b) => Vector3.Distance(p, Closest(p, a, b));

    /// <summary>One connection from the owner to a neighbour, in map space: the owner's edge and its overlap with the neighbour's edge.</summary>
    private readonly record struct Joint(int Edge, Vector3 A, Vector3 B, Vector3 OverlapMiddle, float OverlapLength);

    /// <summary>The connections from one navmesh to another of the map, in map space.</summary>
    private static List<Joint> Joints(NavigationMap map, NVA.Navmesh owner, NVA.Navmesh target, Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var joints = new List<Joint>();
        var mesh = map.FileOf(owner)?.Mesh;
        var targetMesh = map.FileOf(target)?.Mesh;

        if (mesh == null)
            return joints;

        var ownerWorld = toMap(owner);
        var targetWorld = toMap(target);

        foreach (var k in map.Nva.Connectors.Where(k => k.MainNameID == owner.NameID && k.TargetNameID == target.NameID))
        {
            foreach (var n in k.NavMeshConnections)
            {
                if (n.EdgeIndex < 0 || n.EdgeIndex >= mesh.m_edges.Count)
                    continue;

                var a = Vector3.Transform(V(mesh, mesh.m_edges[n.EdgeIndex].m_a), ownerWorld);
                var b = Vector3.Transform(V(mesh, mesh.m_edges[n.EdgeIndex].m_b), ownerWorld);
                var middle = (a + b) / 2;
                float length = Vector3.Distance(a, b);

                // The part of the owner's edge the neighbour's edge lies along
                if (targetMesh != null && n.OppositeEdgeIndex >= 0 && n.OppositeEdgeIndex < targetMesh.m_edges.Count && length > 1e-5f)
                {
                    var c = Vector3.Transform(V(targetMesh, targetMesh.m_edges[n.OppositeEdgeIndex].m_a), targetWorld);
                    var d = Vector3.Transform(V(targetMesh, targetMesh.m_edges[n.OppositeEdgeIndex].m_b), targetWorld);
                    var ab = b - a;
                    float l2 = ab.LengthSquared();
                    float t0 = Math.Clamp(Vector3.Dot(c - a, ab) / l2, 0, 1), t1 = Math.Clamp(Vector3.Dot(d - a, ab) / l2, 0, 1);
                    float lo = Math.Min(t0, t1), hi = Math.Max(t0, t1);
                    middle = a + ab * ((lo + hi) / 2);
                    length = (hi - lo) * MathF.Sqrt(l2);
                }

                joints.Add(new Joint(n.EdgeIndex, a, b, middle, length));
            }
        }

        return joints;
    }

    /// <summary>
    /// Where the gates between two navmeshes go, in map space: the connections from the owner to the target are joined
    /// into runs whose edges come within the join distance, and each run gets a gate at the middle of the overlap of
    /// its longest connection.
    /// </summary>
    public static List<Vector3> Sites(NavigationMap map, NVA.Navmesh owner, NVA.Navmesh target, Func<NVA.Navmesh, Matrix4x4> toMap,
        float join = DefaultRunJoin)
    {
        var joints = Joints(map, owner, target, toMap);
        var parent = Enumerable.Range(0, joints.Count).ToArray();
        int Find(int x) { while (parent[x] != x) x = parent[x] = parent[parent[x]]; return x; }

        for (int i = 0; i < joints.Count; i++)
        {
            for (int j = i + 1; j < joints.Count; j++)
            {
                var p = joints[i];
                var q = joints[j];
                float d = Math.Min(Math.Min(SegmentDistance(p.A, q.A, q.B), SegmentDistance(p.B, q.A, q.B)),
                    Math.Min(SegmentDistance(q.A, p.A, p.B), SegmentDistance(q.B, p.A, p.B)));

                if (d < join)
                    parent[Find(i)] = Find(j);
            }
        }

        // The longest overlap places more gates where the game has them than the one nearest the run's middle
        // (gateedit: 37% against 32% within 1 cm)
        return Enumerable.Range(0, joints.Count).GroupBy(Find)
            .Select(g => g.Select(i => joints[i]).OrderByDescending(j => j.OverlapLength).First().OverlapMiddle)
            .ToList();
    }

    /// <summary>The point on the owner's connection edges to the target nearest a point, in map space, or null if none.</summary>
    public static Vector3? Snap(NavigationMap map, NVA.Navmesh owner, NVA.Navmesh target, Vector3 point, Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var joints = Joints(map, owner, target, toMap);

        if (joints.Count == 0)
            return null;

        return joints.Select(j => Closest(point, j.A, j.B)).OrderBy(p => Vector3.Distance(p, point)).First();
    }

    #endregion

    #region Costs

    /// <summary>
    /// The walking distances in metres from each point to each other over a navmesh's faces, all in map space; infinity
    /// where there is no walk. The walk goes from edge middle to edge middle across faces (blocked edges are skipped,
    /// unblocked user edges are taken), so it is a little longer than the game's.
    /// </summary>
    public static float[,] WalkingDistances(IHavokNavMeshEdit mesh, Matrix4x4 toMap, IReadOnlyList<Vector3> points)
    {
        int n = points.Count;
        var result = new float[n, n];

        if (!Matrix4x4.Invert(toMap, out var toMesh) || mesh == null || mesh.m_faces.Count == 0)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    result[i, j] = i == j ? 0 : float.PositiveInfinity;
            return result;
        }

        // In the mesh's own space, which keeps distances (toMap is a rotation and a move)
        var local = points.Select(p => Vector3.Transform(p, toMesh)).ToList();
        var faces = local.Select(p => FaceOfPoint(mesh, p)).ToList();

        var centres = new Vector3[mesh.m_faces.Count];
        for (int f = 0; f < centres.Length; f++)
            centres[f] = NavigationOverlayBuilder.FaceCentre(mesh, f);

        int blocked = (int)HavokNavEdgeFlags.EDGE_BLOCKED;

        for (int i = 0; i < n; i++)
        {
            result[i, i] = 0;

            // A node is an edge crossed: the walk is at its entry point, in the face it leads to
            var dist = new Dictionary<int, float>();
            var entry = new Dictionary<int, (int Face, Vector3 Point)>();
            var queue = new PriorityQueue<int, float>();
            var best = new float[n];
            Array.Fill(best, float.PositiveInfinity);
            best[i] = 0;

            void Leave(int face, Vector3 from, float d)
            {
                var f = mesh.m_faces[face];

                for (int j = 0; j < n; j++)
                    if (faces[j] == face)
                        best[j] = Math.Min(best[j], d + Vector3.Distance(from, local[j]));

                void Try(int k, bool user)
                {
                    var edge = mesh.m_edges[k];
                    if (((int)edge.m_flags & blocked) != 0)
                        return;
                    if (NavigationOverlayBuilder.OppositeFace(mesh, edge.m_oppositeFace) is not int other)
                        return;

                    var middle = (V(mesh, edge.m_a) + V(mesh, edge.m_b)) / 2;
                    // A user edge lands in the middle of its target face; an inner edge is crossed at its middle
                    var land = user ? centres[other] : middle;
                    float nd = d + Vector3.Distance(from, middle) + (user ? Vector3.Distance(middle, land) : 0);

                    if (nd < dist.GetValueOrDefault(k, float.PositiveInfinity))
                    {
                        dist[k] = nd;
                        entry[k] = (other, land);
                        queue.Enqueue(k, nd);
                    }
                }

                for (int k = f.m_startEdgeIndex; k < f.m_startEdgeIndex + f.m_numEdges; k++)
                    Try(k, false);
                if (f.m_numUserEdges > 0)
                    for (int k = f.m_startUserEdgeIndex; k < f.m_startUserEdgeIndex + f.m_numUserEdges; k++)
                        Try(k, true);
            }

            if (faces[i] >= 0)
                Leave(faces[i], local[i], 0);

            while (queue.TryDequeue(out int k, out float d))
            {
                if (d > dist[k])
                    continue;
                var (face, point) = entry[k];
                Leave(face, point, d);
            }

            for (int j = 0; j < n; j++)
                if (j != i)
                    result[i, j] = faces[i] < 0 || faces[j] < 0 ? float.PositiveInfinity : best[j];
        }

        // The game's tables are symmetric; the walk can differ by direction through one-way links
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                result[i, j] = result[j, i] = Math.Min(result[i, j], result[j, i]);

        return result;
    }

    /// <summary>The face whose edges come nearest a point (a gate sits on a boundary edge of it), or -1.</summary>
    private static int FaceOfPoint(IHavokNavMeshEdit mesh, Vector3 p)
    {
        int bestFace = -1;
        float bestDistance = float.MaxValue;

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            {
                float d = SegmentDistance(p, V(mesh, mesh.m_edges[k].m_a), V(mesh, mesh.m_edges[k].m_b));
                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestFace = f;
                }
            }
        }

        return bestFace;
    }

    /// <summary>The costs of a navmesh's gates to each other, by gate object, so they can be kept across a change of its list.</summary>
    public static Dictionary<(NVA.GateNode, NVA.GateNode), short> CostsByGate(NVA.Navmesh owner)
    {
        var costs = new Dictionary<(NVA.GateNode, NVA.GateNode), short>(ReferencePairComparer.Instance);
        var gates = owner.GateNodes;

        for (int i = 0; i < gates.Count; i++)
            for (int j = 0; j < gates.Count && j < gates[i].NeighbourGateNodeCosts.Count; j++)
                costs[(gates[i], gates[j])] = gates[i].NeighbourGateNodeCosts[j];

        return costs;
    }

    /// <summary>
    /// Writes a navmesh's cost table: 16 entries per gate, 0 to itself, -1 past its gates. Pairs whose two gates are in
    /// <paramref name="keep"/> and in neither <paramref name="stale"/> keep that cost; the others are walked.
    /// </summary>
    public static void RefreshCosts(NavigationMap map, NVA.Navmesh owner, Func<NVA.Navmesh, Matrix4x4> toMap,
        IReadOnlyDictionary<(NVA.GateNode, NVA.GateNode), short> keep = null, ISet<NVA.GateNode> stale = null)
    {
        var gates = owner.GateNodes;
        int n = gates.Count;

        bool Kept(int i, int j, out short cost)
        {
            cost = 0;
            return keep != null && (stale == null || (!stale.Contains(gates[i]) && !stale.Contains(gates[j])))
                && keep.TryGetValue((gates[i], gates[j]), out cost);
        }

        float[,] walk = null;
        bool needWalk = false;
        for (int i = 0; i < n && !needWalk; i++)
            for (int j = 0; j < n && !needWalk; j++)
                if (i != j && !Kept(i, j, out _))
                    needWalk = true;

        if (needWalk)
            walk = WalkingDistances(map.FileOf(owner)?.Mesh, toMap(owner), gates.Select(g => g.Position).ToList());

        var table = new short[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                table[i, j] = i == j ? (short)0 : Kept(i, j, out var cost) ? cost : EncodeCost(walk[i, j]);

        for (int i = 0; i < n; i++)
        {
            // 16 slots, -1 past the gates (ER, NR), or one per gate (DB_NT)
            int slots = map.Profile.GateCostSlots(n);
            var costs = new List<short>(slots);
            for (int j = 0; j < slots; j++)
                costs.Add(j < n ? table[i, j] : Unreachable);
            gates[i].NeighbourGateNodeCosts = costs;
        }
    }

    /// <summary>Walks every cost of a navmesh's gates again, e.g. after its faces moved.</summary>
    public static void RecomputeCosts(NavigationMap map, NVA.Navmesh owner, Func<NVA.Navmesh, Matrix4x4> toMap)
        => RefreshCosts(map, owner, toMap);

    /// <summary>Sets the cost between two gates of a navmesh, both ways.</summary>
    public static void SetCost(NVA.Navmesh owner, int a, int b, short cost, NavGameProfile profile = null)
    {
        if (a == b || a < 0 || b < 0 || a >= owner.GateNodes.Count || b >= owner.GateNodes.Count)
            return;

        int slots = (profile ?? NavGameProfile.ER).GateCostSlots(owner.GateNodes.Count);
        Pad(owner.GateNodes[a], slots);
        Pad(owner.GateNodes[b], slots);
        owner.GateNodes[a].NeighbourGateNodeCosts[b] = cost;
        owner.GateNodes[b].NeighbourGateNodeCosts[a] = cost;
    }

    private static void Pad(NVA.GateNode gate, int slots)
    {
        while (gate.NeighbourGateNodeCosts.Count < slots)
            gate.NeighbourGateNodeCosts.Add(Unreachable);
    }

    #endregion

    #region Edits

    /// <summary>
    /// Rebuilds the gates between each of the owners and each navmesh of the map it is connected to: the gates the two
    /// have to each other go, and each site (see <see cref="Sites"/>) gets a new pair, or a gate on the side with room
    /// when the other is full. The cost tables of every navmesh that changed are written again; costs between gates
    /// that stayed are kept.
    /// </summary>
    public static NavGateEditResult Rebuild(NavigationMap map, IEnumerable<NVA.Navmesh> owners, Func<NVA.Navmesh, Matrix4x4> toMap,
        float join = DefaultRunJoin)
    {
        var pairs = new HashSet<(NVA.Navmesh, NVA.Navmesh)>();
        var list = map.Nva.Navmeshes;

        foreach (var owner in owners.Distinct())
        {
            foreach (var other in Neighbours(map, owner))
            {
                // Each pair once, from the side earlier in the list
                var pair = list.IndexOf(owner) < list.IndexOf(other) ? (owner, other) : (other, owner);
                pairs.Add(pair);
            }

            // Gates to navmeshes it is no longer connected to go too
            pairs.UnionWith(owner.GateNodes
                .Where(g => g.ConnectedNavmeshIndex >= 0 && g.ConnectedNavmeshIndex < list.Count)
                .Select(g => list[g.ConnectedNavmeshIndex])
                .Where(t => t != owner && !Connected(map.Nva, owner.NameID, t.NameID))
                .Select(t => list.IndexOf(owner) < list.IndexOf(t) ? (owner, t) : (t, owner)));
        }

        return RebuildPairs(map, pairs, toMap, join);
    }

    /// <summary>Rebuilds the gates of some pairs of navmeshes (see <see cref="Rebuild"/>).</summary>
    public static NavGateEditResult RebuildPairs(NavigationMap map, IEnumerable<(NVA.Navmesh A, NVA.Navmesh B)> pairs,
        Func<NVA.Navmesh, Matrix4x4> toMap, float join = DefaultRunJoin)
    {
        var result = new NavGateEditResult();
        var list = map.Nva.Navmeshes;
        var pairList = pairs.Where(p => p.A != p.B && list.Contains(p.A) && list.Contains(p.B)).Distinct().ToList();
        var keep = new Dictionary<NVA.Navmesh, Dictionary<(NVA.GateNode, NVA.GateNode), short>>();

        foreach (var (a, b) in pairList)
        {
            keep.TryAdd(a, CostsByGate(a));
            keep.TryAdd(b, CostsByGate(b));
        }

        // The old gates between each pair, both ways
        foreach (var (a, b) in pairList)
        {
            result.Removed += RemoveGatesBetween(list, a, b, map.Profile);
            result.Removed += RemoveGatesBetween(list, b, a, map.Profile);
        }

        // The new ones
        foreach (var (a, b) in pairList)
        {
            if (!Connected(map.Nva, a.NameID, b.NameID))
                continue;

            foreach (var site in Sites(map, a, b, toMap, join))
                result.Added += AddPair(list, a, b, site);
        }

        foreach (var entry in keep.Keys)
        {
            RefreshCosts(map, entry, toMap, keep[entry]);
            result.Touched.Add(entry);
        }

        return result;
    }

    /// <summary>Removes the gates of a navmesh that lead to another; returns how many.</summary>
    private static int RemoveGatesBetween(List<NVA.Navmesh> list, NVA.Navmesh owner, NVA.Navmesh target, NavGameProfile profile)
    {
        int ownerIndex = list.IndexOf(owner);
        int targetIndex = list.IndexOf(target);
        int removed = 0;

        for (int g = owner.GateNodes.Count - 1; g >= 0; g--)
        {
            if (owner.GateNodes[g].ConnectedNavmeshIndex == targetIndex)
            {
                NavModelEdit.RemoveGate(list, ownerIndex, g, profile);
                removed++;
            }
        }

        return removed;
    }

    /// <summary>Adds a gate at a point to each side that has room, paired when both do; returns how many were added.</summary>
    private static int AddPair(List<NVA.Navmesh> list, NVA.Navmesh a, NVA.Navmesh b, Vector3 point)
    {
        int ia = list.IndexOf(a), ib = list.IndexOf(b);
        bool roomA = a.GateNodes.Count < MaxGates, roomB = b.GateNodes.Count < MaxGates;
        int added = 0;

        NVA.GateNode ga = null, gb = null;
        if (roomA)
        {
            ga = NewGate(point, ib);
            a.GateNodes.Add(ga);
            added++;
        }
        if (roomB)
        {
            gb = NewGate(point, ia);
            b.GateNodes.Add(gb);
            added++;
        }

        if (ga != null && gb != null)
        {
            ga.NodeSubID = (short)(b.GateNodes.Count - 1);
            gb.NodeSubID = (short)(a.GateNodes.Count - 1);
        }

        return added;
    }

    private static NVA.GateNode NewGate(Vector3 point, int target)
    {
        var gate = new NVA.GateNode { Position = point, ConnectedNavmeshIndex = (short)target, NodeSubID = -1 };
        // ER's 16 slots; the RefreshCosts that follows every add sizes the lists as the game does (one per gate in DB_NT)
        Pad(gate, MaxGates);
        return gate;
    }

    /// <summary>
    /// Adds a gate between a navmesh and one it is connected to, at the point of their connection edges nearest a point
    /// (or a new site when null), paired when both have room.
    /// </summary>
    public static NavGateEditResult Add(NavigationMap map, NVA.Navmesh owner, NVA.Navmesh target, Vector3? near,
        Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var result = new NavGateEditResult();

        if (!Connected(map.Nva, owner.NameID, target.NameID))
        {
            result.Error = "MAP_Navigation_Gates_Error_Not_Connected";
            return result;
        }

        if (owner.GateNodes.Count >= MaxGates)
        {
            result.Error = "MAP_Navigation_Gates_Error_Full";
            return result;
        }

        Vector3? point = near is Vector3 p ? Snap(map, owner, target, p, toMap) : Sites(map, owner, target, toMap).FirstOrDefault();
        if (point == null)
        {
            result.Error = "MAP_Navigation_Gates_Error_Not_Connected";
            return result;
        }

        var keepOwner = CostsByGate(owner);
        var keepTarget = CostsByGate(target);
        result.Added = AddPair(map.Nva.Navmeshes, owner, target, point.Value);

        RefreshCosts(map, owner, toMap, keepOwner);
        RefreshCosts(map, target, toMap, keepTarget);
        result.Touched.Add(owner);
        result.Touched.Add(target);
        return result;
    }

    /// <summary>Removes a gate and its partner, with their cost rows and columns.</summary>
    public static NavGateEditResult Remove(NavigationMap map, NVA.Navmesh owner, int gate)
    {
        var result = new NavGateEditResult();
        var list = map.Nva.Navmeshes;

        if (gate < 0 || gate >= owner.GateNodes.Count)
            return result;

        var partner = Partner(map, owner, gate);
        NavModelEdit.RemoveGate(list, list.IndexOf(owner), gate, map.Profile);
        result.Removed++;
        result.Touched.Add(owner);

        // The partner's index isn't changed by taking out a gate of another navmesh
        if (partner is (NVA.Navmesh entry, int index))
        {
            NavModelEdit.RemoveGate(list, list.IndexOf(entry), index, map.Profile);
            result.Removed++;
            result.Touched.Add(entry);
        }

        return result;
    }

    /// <summary>
    /// Moves a gate, kept on the owner's connection edges to its target, and its partner with it, then walks the costs
    /// of the two again.
    /// </summary>
    public static NavGateEditResult Move(NavigationMap map, NVA.Navmesh owner, int gate, Vector3 point, Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var result = new NavGateEditResult();
        var list = map.Nva.Navmeshes;
        var g = owner.GateNodes[gate];

        if (g.ConnectedNavmeshIndex < 0 || g.ConnectedNavmeshIndex >= list.Count)
        {
            result.Error = "MAP_Navigation_Gates_Error_Broken";
            return result;
        }

        var target = list[g.ConnectedNavmeshIndex];
        if (Snap(map, owner, target, point, toMap) is not Vector3 snapped)
        {
            result.Error = "MAP_Navigation_Gates_Error_Not_Connected";
            return result;
        }

        var partner = Partner(map, owner, gate);
        MoveTo(map, owner, g, snapped, toMap);
        result.Touched.Add(owner);

        if (partner is (NVA.Navmesh entry, int index))
        {
            MoveTo(map, entry, entry.GateNodes[index], snapped, toMap);
            result.Touched.Add(entry);
        }

        return result;
    }

    /// <summary>Moves a gate without its costs, for a drag; <see cref="Move"/> walks them at the end.</summary>
    public static Vector3? DragTo(NavigationMap map, NVA.Navmesh owner, int gate, Vector3 point, Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var list = map.Nva.Navmeshes;
        var g = owner.GateNodes[gate];

        if (g.ConnectedNavmeshIndex < 0 || g.ConnectedNavmeshIndex >= list.Count)
            return null;

        if (Snap(map, owner, list[g.ConnectedNavmeshIndex], point, toMap) is not Vector3 snapped)
            return null;

        var partner = Partner(map, owner, gate);
        g.Position = snapped;
        if (partner is (NVA.Navmesh entry, int index))
            entry.GateNodes[index].Position = snapped;

        return snapped;
    }

    private static void MoveTo(NavigationMap map, NVA.Navmesh owner, NVA.GateNode gate, Vector3 point, Func<NVA.Navmesh, Matrix4x4> toMap)
    {
        var keep = CostsByGate(owner);
        gate.Position = point;
        RefreshCosts(map, owner, toMap, keep, new HashSet<NVA.GateNode>(ReferenceEqualityComparer.Instance) { gate });
    }

    #endregion

    /// <summary>Compares gate pairs by reference, as gate nodes have no identity of their own.</summary>
    private sealed class ReferencePairComparer : IEqualityComparer<(NVA.GateNode, NVA.GateNode)>
    {
        public static readonly ReferencePairComparer Instance = new();

        public bool Equals((NVA.GateNode, NVA.GateNode) x, (NVA.GateNode, NVA.GateNode) y)
            => ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

        public int GetHashCode((NVA.GateNode, NVA.GateNode) obj)
            => HashCode.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item1), System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item2));
    }
}
